using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;

namespace Extract.SqlDatabase
{
    public abstract class SqlAppRoleConnection : DbConnection
    {
        internal SqlConnection BaseSqlConnection { get; set; }

        /// <summary>
        /// Created when application role is enabled and needed for disabling the created app role
        /// </summary>
        byte[] AppRoleCookie;

        protected SqlAppRoleConnection() :
            base()
        {
            BaseSqlConnection = new SqlConnection();
        }

        protected SqlAppRoleConnection(SqlConnection sqlConnection)
        {
            BaseSqlConnection = sqlConnection;
        }

        protected SqlAppRoleConnection(string connectionString) :
            base()
        {
            SqlConnectionStringBuilder sqlConnectionStringBuilder = new(connectionString);
            sqlConnectionStringBuilder.Pooling = false;
            BaseSqlConnection = new SqlConnection(sqlConnectionStringBuilder.ConnectionString);
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

        protected abstract void AssignRole();

        public override void ChangeDatabase(string databaseName)
        {
            BaseSqlConnection.ChangeDatabase(databaseName);
            AssignRole();
        }

        public override void Close()
        {
            if (BaseSqlConnection.State != ConnectionState.Closed)
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
            AssignRole();

        }
        public new SqlTransaction BeginTransaction()
        {
            return (SqlTransaction)BeginDbTransaction(default);
        }
        public new SqlTransaction BeginTransaction(IsolationLevel isolationLevel)
        {
            return (SqlTransaction)BeginDbTransaction(isolationLevel);
        }

        public new AppRoleCommand CreateCommand()
        {
            return (AppRoleCommand)CreateDbCommand();
        }

        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
        {
            return BaseSqlConnection.BeginTransaction(isolationLevel);
        }

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
                BaseSqlConnection?.Dispose();
                BaseSqlConnection = null;
            }
            base.Dispose(disposing);
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
            if (AppRoleCookie is null) return;

            if (BaseSqlConnection?.State != ConnectionState.Open) return;

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

        /// 
        public Type GetConnectionType()
        {
            return BaseSqlConnection?.GetType();
        }
    }
}
