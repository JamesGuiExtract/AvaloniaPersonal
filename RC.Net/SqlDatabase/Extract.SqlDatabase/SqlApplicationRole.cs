using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace Extract.SqlDatabase
{
    /// <summary>
    /// Class to use to enable an application role on a connection
    /// NOTE: This class is a C# version of the C++ class CppSqlApplicationRole
    /// Engineering\ReusableComponents\COMComponents\UCLIDFileProcessing\FAMUtils\Code\SqlApplicationRole.h
    /// </summary>
    public class SqlApplicationRole : IDisposable
    {
        public enum AppRoleAccess
        {
            NoAccess = 0,
            SelectExecuteAccess = 1,
            InsertAccess = 3,
            UpdateAccess = 5,
            DeleteAccess = 9,
            AlterAccess = 17,
            AllAccess = SelectExecuteAccess | InsertAccess | UpdateAccess | DeleteAccess | AlterAccess
        }

        #region IDisposable

        private bool disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    try
                    {
                        UnsetApplicationRole();
                    }
                    catch (ExtractException ee)
                    {
                        ee.Log();
                    }
                    catch (Exception ex)
                    {
                        ex.ExtractLog("ELI51757");
                    }
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~SqlApplicationRole()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion

        /// <summary>
        /// Created when application role is enabled and needed for disabling the created app role
        /// </summary>
        byte[] AppRoleCookie;

        /// <summary>
        /// Connection to enable the application role 
        /// </summary>
        public SqlConnection DatabaseConnection { get; set; }

        /// <summary>
        /// Name of the Application role to enable
        /// </summary>
        public string ApplicationRoleName { get; set; }

        /// <summary>
        /// Constructor to enable the given application role of the given connection
        /// </summary>
        /// <param name="sqlConnection">Connection to enable the given <paramref name="applicationRoleName"/> on</param>
        /// <param name="applicationRoleName">The Application role that is to be enabled on <paramref name="sqlConnection"/>.</param>
        /// <param name="password">Password for the given application role</param>
        public SqlApplicationRole(SqlConnection sqlConnection, string applicationRoleName, string password)
        {
            try
            {
                DatabaseConnection = sqlConnection;
                ApplicationRoleName = applicationRoleName;
                if (DatabaseConnection.State != ConnectionState.Open)
                {
                    DatabaseConnection.Open();
                }

                // If the applicationRoleName is blank there is no application role to enable
                if (!string.IsNullOrWhiteSpace(applicationRoleName))
                    SetApplicationRole(applicationRoleName, password);

            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI51769");
            }
        }

        /// <summary>
        /// Static method to create and Application role
        /// NOTE: This method will need to be called when running as a user that has the ability to create application roles
        /// </summary>
        /// <param name="sqlConnection">Connection to create the <paramref name="applicationRoleName"/> on</param>
        /// <param name="applicationRoleName">The name of the Application role to create</param>
        /// <param name="password">Password that will be used to enable the application role</param>
        /// <param name="access">Access that should be granted for the application role</param>
        public static void CreateApplicationRole(SqlConnection sqlConnection, string applicationRoleName, string password, AppRoleAccess access)
        {
            // TODO: Should password be expected to be encrypted and need to be decrypted here?
            if (string.IsNullOrWhiteSpace(applicationRoleName)) return;
            try
            {
                using var cmd = sqlConnection.CreateCommand();
                // Parameters are not being used here because the "CREATE APPLICATION ROLE" sql would not accept them.
                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"CREATE APPLICATION ROLE {applicationRoleName} WITH PASSWORD = '{password}', DEFAULT_SCHEMA = dbo;");
                if (access > 0)
                {
                    sb.AppendLine($"GRANT EXECUTE TO {applicationRoleName};");
                    sb.AppendLine($"GRANT SELECT TO {applicationRoleName};");
                }
                if ((access & AppRoleAccess.InsertAccess & ~AppRoleAccess.SelectExecuteAccess) > 0)
                    sb.AppendLine($"GRANT INSERT TO {applicationRoleName};");
                if ((access & AppRoleAccess.UpdateAccess & ~AppRoleAccess.SelectExecuteAccess) > 0)
                    sb.AppendLine($"GRANT UPDATE TO {applicationRoleName};");
                if ((access & AppRoleAccess.DeleteAccess & ~AppRoleAccess.SelectExecuteAccess) > 0)
                    sb.AppendLine($"GRANT DELETE TO {applicationRoleName};");

                cmd.CommandText = sb.ToString();

                if (sqlConnection.State != System.Data.ConnectionState.Open) sqlConnection.Open();

                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI51758");
            }
        }

        void SetApplicationRole(string roleName, string appPassword)
        {
            if (DatabaseConnection is null)
                throw new ExtractException("ELI51753", "Connection not set.");
            try
            {
                if (DatabaseConnection.State != ConnectionState.Open)
                    DatabaseConnection.Open();

                using var cmd = DatabaseConnection.CreateCommand();
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

        void UnsetApplicationRole()
        {
            if (AppRoleCookie is null) return;

            if (DatabaseConnection is null) return;

            if (DatabaseConnection.State != ConnectionState.Open) return;

            try
            {
                using var cmd = DatabaseConnection.CreateCommand();
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

    }

}
