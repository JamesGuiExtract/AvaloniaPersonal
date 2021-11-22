using Microsoft.Win32;
using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Globalization;
using System.Threading;

namespace Extract.SqlDatabase
{
    public abstract partial class SqlAppRoleConnection : DbConnection
    {
        const string FileProcessingDBRegPath = @"Software\Extract Systems\ReusableComponents\COMComponents\UCLIDFileProcessing\FileProcessingDB";
        const string UseApplicationRolesKey = "UseApplicationRoles";

        SqlConnection _baseSqlConnection;
        internal SqlConnection BaseSqlConnection
        { 
            get
            {
                if (_baseSqlConnection == null)
                {
                    SetBaseSqlConnection(_connectionString);
                    _ownsBaseConnection = true;
                }

                return _baseSqlConnection;
            }
            set
            {
                if (value != _baseSqlConnection)
                {
                    _baseSqlConnection = value;
                    _ownsBaseConnection = false;
                }
            }
        }

        /// Determines whether this connection can be returned to the ConnectionPool when closed.
        bool _ownsBaseConnection;

        string _connectionString;

        bool _isRoleAssigned;

        protected SqlAppRoleConnection() : base()
        {
        }

        protected SqlAppRoleConnection(SqlConnection sqlConnection) : base()
        {
            _connectionString = sqlConnection?.ConnectionString;
            BaseSqlConnection = sqlConnection;
        }

        protected SqlAppRoleConnection(string connectionString) : base()
        {
            _connectionString = SqlUtil.MakeAppRoleCompatibleConnectionString(connectionString);
        }

        protected SqlAppRoleConnection(string server, string database, bool enlist = true)
        {
            _connectionString = SqlUtil.CreateConnectionString(server, database, enlist);
        }

        // Establishes the underlying SqlConnection by either acquiring one from the ConnectionPool
        // or creating a new one.
        private void SetBaseSqlConnection(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                _baseSqlConnection = new SqlConnection();
            }
            else if (!EnableConnectionPooling || !ConnectionPool.TryGetPoolConnection(this, connectionString))
            {
                _baseSqlConnection = SqlUtil.NewSqlDBConnection(connectionString);
            }
        }

        // -1 indicates we have not yet checked the registry to see whether application roles should be used.
        static int _useApplicationRoles = -1;

        internal static bool UseApplicationRoles
        {
            get
            {
                if (_useApplicationRoles < 0)
                {
                    // Read from the registry once per process whether to use application roles.
                    using RegistryKey FileProcessingDBKey = Registry.LocalMachine.OpenSubKey(FileProcessingDBRegPath);
                    string keyValue = (string)FileProcessingDBKey?.GetValue(UseApplicationRolesKey, "1") ?? "1";
                    var value = int.Parse(keyValue, CultureInfo.InvariantCulture);
                    Interlocked.CompareExchange(ref _useApplicationRoles, value, -1);
                }

                return _useApplicationRoles == 1;
            }
        }

        // Whether this class should maintain a pool of SqlConnections that have been authenticated under
        // the appropriate application role.
        public static bool EnableConnectionPooling { get; set; } = true;

        // If EnableConnectionPooling, the amount of time after which an unused connection should be closed and
        // removed from the pool.
        public static TimeSpan ConnectionPoolTimeout
        {
            get => ConnectionPool.ConnectionPoolTimeout;
            set => ConnectionPool.ConnectionPoolTimeout = value;
        }

        public override string ConnectionString
        {
            get => BaseSqlConnection.ConnectionString;
            set
            {
                SqlConnectionStringBuilder sqlConnectionStringBuilder = new(value);
                sqlConnectionStringBuilder.Pooling = false;
                if (sqlConnectionStringBuilder.ConnectionString != BaseSqlConnection.ConnectionString)
                {
                    BaseSqlConnection.ConnectionString = sqlConnectionStringBuilder.ConnectionString;
                }
            }
        }

        public override string Database => BaseSqlConnection.Database;

        public override string DataSource => BaseSqlConnection.DataSource;

        public override string ServerVersion => BaseSqlConnection.ServerVersion;

        public override ConnectionState State => BaseSqlConnection.State;

        internal abstract string RoleName { get; }
        internal abstract string RolePassword { get; }

        public override void ChangeDatabase(string databaseName)
        {
            BaseSqlConnection.ChangeDatabase(databaseName);
            if (UseApplicationRoles) SetApplicationRole();
        }

        public override void Close()
        {
            if (!EnableConnectionPooling || !ConnectionPool.TryAddPoolConnection(this))
            {
                BaseSqlConnection?.Close();
                _isRoleAssigned = false;
            }
        }

        public override void Open()
        {
            if (BaseSqlConnection.State == ConnectionState.Open)
            {
                return;
            }

            BaseSqlConnection.Open();
            if (UseApplicationRoles) SetApplicationRole();
        }

        public new SqlTransaction BeginTransaction() { return (SqlTransaction)BeginDbTransaction(default); }
        public new SqlTransaction BeginTransaction(IsolationLevel isolationLevel)
        { return (SqlTransaction)BeginDbTransaction(isolationLevel); }

        public new AppRoleCommand CreateCommand() { return (AppRoleCommand)CreateDbCommand(); }

        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
        { return BaseSqlConnection.BeginTransaction(isolationLevel); }

        protected override DbCommand CreateDbCommand()
        {
            var cmd = new AppRoleCommand();
            cmd.Connection = this;
            return cmd;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Close();
                _baseSqlConnection?.Dispose();
                _baseSqlConnection = null;
            }
            base.Dispose(disposing);
        }


        protected void SetApplicationRole()
        {
            if (BaseSqlConnection is null)
                throw new ExtractException("ELI51753", "Connection not set.");
            try
            {
                if (BaseSqlConnection.State != ConnectionState.Open)
                    BaseSqlConnection.Open();

                if (!string.IsNullOrEmpty(RoleName) && !_isRoleAssigned)
                {
                    using var cmd = BaseSqlConnection.CreateCommand();
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "sys.sp_setapprole";
                    cmd.Parameters.AddWithValue("@rolename", RoleName);
                    cmd.Parameters.AddWithValue("@password", RolePassword);
                    cmd.Parameters.AddWithValue("@encrypt", "none");
                    cmd.Parameters.AddWithValue("@fCreateCookie", false);

                    cmd.ExecuteNonQuery();

                    _isRoleAssigned = true;
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI51754");
            }
        }

        /// 
        public Type GetConnectionType() { return BaseSqlConnection?.GetType(); }
    }
}
