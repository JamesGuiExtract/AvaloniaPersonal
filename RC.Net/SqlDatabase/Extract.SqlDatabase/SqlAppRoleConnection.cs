using Microsoft.Win32;
using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Threading;

namespace Extract.SqlDatabase
{
    public abstract class SqlAppRoleConnection : DbConnection
    {
        const string FileProcessingDBRegPath = @"Software\Extract Systems\ReusableComponents\COMComponents\UCLIDFileProcessing\FileProcessingDB";
        const string UseApplicationRolesKey = "UseApplicationRoles";
        const string UseConnectionPoolingKey = "UseConnectionPooling";
        const int FalseValue = 0;
        const int TrueValue = 1;
        const int UnknownValue = int.MinValue; // We have not yet checked the registry for a setting

        static int _useApplicationRoles = UnknownValue;
        static int _useConnectionPooling = UnknownValue;

        internal SqlConnection BaseSqlConnection { get; set; }

        /// <summary>
        /// Created when application role is enabled and needed for disabling the created app role
        /// </summary>
        byte[] AppRoleCookie;

        protected SqlAppRoleConnection() : base()
        {
            BaseSqlConnection = new SqlConnection();
        }

        protected SqlAppRoleConnection(SqlConnection sqlConnection) : base()
        {
            BaseSqlConnection = sqlConnection;
        }

        protected SqlAppRoleConnection(string connectionString) : base()
        {
            SqlConnectionStringBuilder sqlConnectionStringBuilder = new(connectionString);
            sqlConnectionStringBuilder.Pooling = UseConnectionPooling;
            BaseSqlConnection = new SqlConnection(sqlConnectionStringBuilder.ConnectionString);
        }

        /// Whether to use app roles (this is a cached registry setting)
        public static bool UseApplicationRoles
        {
            get
            {
                if (_useApplicationRoles == UnknownValue)
                {
                    const int defaultValue = TrueValue;

                    Interlocked.CompareExchange(ref _useApplicationRoles,
                        GetRegistryValue(UseApplicationRolesKey, defaultValue),
                        UnknownValue);
                }

                return _useApplicationRoles != FalseValue;
            }
        }

        /// Whether to use connection pooling (this is a cached registry setting)
        public static bool UseConnectionPooling
        {
            get
            {
                if (_useConnectionPooling == UnknownValue)
                {
                    const int defaultValue = TrueValue;

                    Interlocked.CompareExchange(ref _useConnectionPooling,
                        GetRegistryValue(UseConnectionPoolingKey, defaultValue),
                        UnknownValue);
                }

                return _useConnectionPooling != FalseValue;
            }
        }

        private static int GetRegistryValue(string valueName, int defaultValue)
        {
            using RegistryKey maybeKey = Registry.LocalMachine.OpenSubKey(FileProcessingDBRegPath);
            if (maybeKey is RegistryKey key
                && key.GetValue(valueName) is string stringValue
                && int.TryParse(stringValue, out int intValue))
            {
                return intValue;
            }

            return defaultValue;
        }

        public override string ConnectionString
        {
            get => BaseSqlConnection.ConnectionString;
            set
            {
                SqlConnectionStringBuilder sqlConnectionStringBuilder = new(value);
                sqlConnectionStringBuilder.Pooling = UseConnectionPooling;
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

        protected abstract void AssignRole();

        public override void ChangeDatabase(string databaseName)
        {
            BaseSqlConnection.ChangeDatabase(databaseName);
            if (UseApplicationRoles)
            {
                AssignRole();
            }
        }

        public override void Close()
        {
            if (UseApplicationRoles && BaseSqlConnection.State != ConnectionState.Closed)
            {
                UnsetApplicationRole();
            }
            BaseSqlConnection.Close();
        }

        public override void Open()
        {
            if (BaseSqlConnection.State == ConnectionState.Open)
            {
                return;
            }

            BaseSqlConnection.Open();
            if (UseApplicationRoles)
            {
                AssignRole();
            }
        }

        public new SqlTransaction BeginTransaction()
        {
            return (SqlTransaction)BeginDbTransaction(default);
        }

        public new SqlTransaction BeginTransaction(IsolationLevel isolationLevel)
        {
            return (SqlTransaction)BeginDbTransaction(isolationLevel);
        }

        public new AppRoleCommand CreateCommand() { return (AppRoleCommand)CreateDbCommand(); }

        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
        {
            return BaseSqlConnection.BeginTransaction(isolationLevel);
        }

        protected override DbCommand CreateDbCommand()
        {
            return new AppRoleCommand
            {
                Connection = this
            };
        }

        private bool disposedValue;
        protected override void Dispose(bool disposing)
        {
            if (disposedValue)
            {
                return;
            }
            try
            {
                // Call close to unset the app role when this object is finalized in case someone forgot to dispose
                // https://extract.atlassian.net/browse/ISSUE-17693
                Close();
            }
            catch (Exception) { }

            if (disposing)
            {
                BaseSqlConnection?.Dispose();
                BaseSqlConnection = null;
            }
#if DEBUG
            if (!disposing)
            {
                new ExtractException("ELI53007", "Dispose was not called on a SqlAppRoleConnection").Log();
            }
#endif

            base.Dispose(disposing);

            disposedValue = true;
        }

        protected void SetApplicationRole(string roleName, string appPassword)
        {
            if (BaseSqlConnection is null)
                throw new ExtractException("ELI51753", "Connection not set.");
            try
            {
                if (BaseSqlConnection.State != ConnectionState.Open)
                    BaseSqlConnection.Open();

                using var cmd = BaseSqlConnection.CreateCommand();

                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "sys.sp_setapprole";
                cmd.Parameters.AddWithValue("@rolename", roleName);
                cmd.Parameters.AddWithValue("@password", appPassword);
                cmd.Parameters.AddWithValue("@encrypt", "none");
                cmd.Parameters.AddWithValue("@fCreateCookie", true);
                cmd.Parameters.Add("@cookie", SqlDbType.VarBinary, 8000);
                cmd.Parameters["@cookie"].Direction = ParameterDirection.Output;


                cmd.ExecuteNonQuery();
                AppRoleCookie = cmd.Parameters["@cookie"].Value as Byte[];
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI51754");
            }
        }

        protected void UnsetApplicationRole()
        {
            if (AppRoleCookie is null)
                return;

            if (BaseSqlConnection?.State != ConnectionState.Open)
                return;

            try
            {
                using var cmd = BaseSqlConnection.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "sys.sp_unsetapprole";
                cmd.Parameters.AddWithValue("@cookie", AppRoleCookie);

                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI51756");
            }
            finally
            {
                AppRoleCookie = null;
            }
        }

        public Type GetConnectionType()
        {
            return BaseSqlConnection?.GetType();
        }
    }
}
