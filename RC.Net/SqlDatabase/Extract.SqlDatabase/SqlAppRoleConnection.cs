using Extract.Licensing;
using Microsoft.Win32;
using System;
using System.Collections.Concurrent;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using UCLID_FILEPROCESSINGLib;

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
        byte[] _appRoleCookie;

        // For FAMDB app roles, the password will be calculated using the role name and database ID via the
        // private SetRolePassword method below. For testing the SqlAppRoleConnection class however, the
        // password can be directly specified, in which case it will be stored in this field.
        string Password;

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

        bool IsRoleAssigned => _appRoleCookie is not null;

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

        public abstract string RoleName { get; }

        // For FAMDB app roles, the password will be calculated using the role name and database ID via the
        // SetPassword call below. For testing the SqlAppRoleConnection class however, the password can be
        // directly specified, in which case it will be stored in this field.
        // NOTE: Intentionally not an XML comment that could lend itself to being included in documentation.
        protected void SetPassword(string password)
        {
            Password = password;
        }

        public override void ChangeDatabase(string databaseName)
        {
            try
            {
                BaseSqlConnection.ChangeDatabase(databaseName);
                if (UseApplicationRoles
                    && !string.IsNullOrEmpty(RoleName)
                    && !IsRoleAssigned)
                {
                    SetApplicationRole();
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53022");
            }
        }

        public override void Close()
        {
            try
            {
                if (UseApplicationRoles && BaseSqlConnection.State != ConnectionState.Closed)
                {
                    UnsetApplicationRole();
                }
                BaseSqlConnection.Close();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53023");
            }
        }

        public override void Open()
        {
            try
            {
                if (BaseSqlConnection.State == ConnectionState.Open)
                {
                    return;
                }

                BaseSqlConnection.Open();

                if (UseApplicationRoles
                   && !string.IsNullOrEmpty(RoleName)
                   && !IsRoleAssigned)
                {
                    SetApplicationRole();
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53024");
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
#if DEBUG
            // Log an exception if the connection was opened and the app role assigned but dispose was not called.
            // This also prevents an exception from being logged when Close is called instead of Dispose, which should be OK.
            // (ExtractRoleConnection.TryOpenConnection can be called using invalid connection strings.
            // This will cause an exception to be thrown before this instance is completely constructed and so it will not get disposed.
            // An exception logged for this case would be misleading)
            if (!disposing && _appRoleCookie != null)
            {
                new ExtractException("ELI53007", "Dispose was not called on a SqlAppRoleConnection").Log();
            }
#endif
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

            base.Dispose(disposing);

            disposedValue = true;
        }

        void SetApplicationRole()
        {
            if (BaseSqlConnection is null)
                throw new ExtractException("ELI51753", "Connection not set.");

            if (RoleName is null)
                throw new ExtractException("ELI53021", "No role defined.");

            if (BaseSqlConnection.State != ConnectionState.Open)
                BaseSqlConnection.Open();

            // Below block allows for a single retry to account for the case that the database ID has changed
            // since the last time the Database ID hash was obtained.
            bool retry;
            bool retried = false;
            bool usedCachedHash = false;
            do
            {
                retry = false;

                try
                {
                    if (!IsRoleAssigned)
                    {
                        using var cmd = BaseSqlConnection.CreateCommand();

                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandText = "sys.sp_setapprole";
                        cmd.Parameters.AddWithValue("@rolename", RoleName);
                        cmd.Parameters.AddWithValue("@encrypt", "none");
                        cmd.Parameters.AddWithValue("@fCreateCookie", true);
                        cmd.Parameters.Add("@cookie", SqlDbType.VarBinary, 8000);
                        cmd.Parameters["@cookie"].Direction = ParameterDirection.Output;
                        SetRolePassword(cmd.Parameters, out usedCachedHash);

                        cmd.ExecuteNonQuery();
                        _appRoleCookie = cmd.Parameters["@cookie"].Value as Byte[];
                    }
                }
                catch (Exception ex)
                {
                    // In case the database ID has been updated since being cached,
                    // try again without using the cached hash.
                    if (usedCachedHash)
                    {
                        _databaseHashes.TryRemove(BaseSqlConnection.ConnectionString, out int _);
                        retry = !retried;
                    }

                    if (retry)
                    {
                        retried = true;
                    }
                    else
                    {
                        throw ex.AsExtract("ELI51754");
                    }
                }
            } 
            while (retry);
        }

        
        [SuppressMessage("Performance", "CA1802:Use literals where appropriate", Justification = "For Obfuscation")]
        static readonly string DATABASE_HASH = "DatabaseHash";

        // To calculate the app role password in SetRolePassword, the hash component of the encrypted DBInfo table
        // DatabaseID value must be used. To avoid having to query for this value every time, cache the hash once
        // obtained.
        static ConcurrentDictionary<string, int> _databaseHashes = new(StringComparer.InvariantCultureIgnoreCase);

        // Calculates the password for a FAMDB application role via the following algorithm that matches the algorithm
        // used in SqlApplicationRole.cpp getRolePassword:
        // 1) Obtain the hash component of a FAMDB's DatabaseID field (part of the encrypted DBInfo table value)
        // 2) Create hash that combines the role name with the DB hash by interpreting each successive 4-byte
        //    chuck of the name as an int value and summing these together along with the DB hash
        // 3) Encrypt the resulting hash using encryption algorithm and password from UCLIDException.
        // 4) Add a fixed suffix with a special char, digit lowercase letter, uppercase letter to prevent it
        //    from being rejected as not sufficiently complex.
        // NOTE: Intentionally not an XML comment that could lend itself to being included in documentation.
        void SetRolePassword(SqlParameterCollection parameters, out bool usedCachedHash)
        {
            usedCachedHash = false;

            if (!string.IsNullOrEmpty(Password))
            {
                parameters.AddWithValue("@password", Password);
                return;
            }

            if (_databaseHashes.TryGetValue(BaseSqlConnection.ConnectionString, out int dbHash))
            {
                usedCachedHash = true;
            }
            else
            {
                SqlConnectionStringBuilder sqlConnectionStringBuilder = new(BaseSqlConnection.ConnectionString);
                FileProcessingDB fileProcessingDB = new();
                fileProcessingDB.DatabaseServer = sqlConnectionStringBuilder.DataSource;
                fileProcessingDB.DatabaseName = sqlConnectionStringBuilder.InitialCatalog;
                dbHash = int.Parse(fileProcessingDB.GetDBInfoSetting(DATABASE_HASH, true), CultureInfo.InvariantCulture);

                _databaseHashes[BaseSqlConnection.ConnectionString] = dbHash;
            }

            int roleHash = dbHash;
            var roleNameBytes = Encoding.ASCII.GetBytes(RoleName)
                .Concat(Enumerable.Repeat((byte)0, 4 - RoleName.Length % 4))
                .ToArray();
            for (int i = 0; i < roleNameBytes.Length; i += 4)
            {
                roleHash += BitConverter.ToInt32(roleNameBytes, i);
            }

            // For clarity; (it should be a safe assumption that little endian is being used)
            var hashBytes = BitConverter.IsLittleEndian
                ? BitConverter.GetBytes(roleHash).Reverse().ToArray()
                : BitConverter.GetBytes(roleHash);

            var password = NativeMethods.Encrypt(hashBytes);
            password += ".9fF";
            parameters.AddWithValue("@password", password);
        }

        void UnsetApplicationRole()
        {
            if (_appRoleCookie is null)
                return;

            if (BaseSqlConnection?.State != ConnectionState.Open)
                return;

            try
            {
                using var cmd = BaseSqlConnection.CreateCommand();
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "sys.sp_unsetapprole";
                cmd.Parameters.AddWithValue("@cookie", _appRoleCookie);

                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI51756");
            }
            finally
            {
                _appRoleCookie = null;
            }
        }

        public Type GetConnectionType()
        {
            return BaseSqlConnection?.GetType();
        }
    }
}
