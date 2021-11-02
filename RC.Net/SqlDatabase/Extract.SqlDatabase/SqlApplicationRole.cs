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
    public static class SqlApplicationRole 
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


        /// <summary>
        /// Static method to create and Application role
        /// NOTE: This method will need to be called when running as a user that has the ability to create application roles
        /// </summary>
        /// <param name="sqlConnection">Connection to create the <paramref name="applicationRoleName"/> on</param>
        /// <param name="applicationRoleName">The name of the Application role to create</param>
        /// <param name="password">Password that will be used to enable the application role</param>
        /// <param name="access">Access that should be granted for the application role</param>
        public static void CreateApplicationRole(IDbConnection sqlConnection, string applicationRoleName, string password, AppRoleAccess access)
        {
            ExtractException.Assert("ELI51955", "Connection must not be null", sqlConnection != null);

            // TODO: Should password be expected to be encrypted and need to be decrypted here?
            if (string.IsNullOrWhiteSpace(applicationRoleName) || sqlConnection is null) return;
            try
            {
                using var cmd = sqlConnection.CreateCommand();
                // Parameters are not being used here because the "CREATE APPLICATION ROLE" sql would not accept them.
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("IF DATABASE_PRINCIPAL_ID('{applicationRoleName}') IS NULL");
                sb.AppendLine("BEGIN");
                sb.AppendLine($"CREATE APPLICATION ROLE {applicationRoleName} WITH PASSWORD = '{password}', DEFAULT_SCHEMA = dbo;");
                if (access > 0)
                {
                    sb.AppendLine($"GRANT VIEW DEFINITION TO {applicationRoleName}; ");
                    sb.AppendLine($"GRANT EXECUTE TO {applicationRoleName};");
                    sb.AppendLine($"GRANT SELECT TO {applicationRoleName};");
                }
                if ((access & AppRoleAccess.InsertAccess & ~AppRoleAccess.SelectExecuteAccess) > 0)
                    sb.AppendLine($"GRANT INSERT TO {applicationRoleName};");
                if ((access & AppRoleAccess.UpdateAccess & ~AppRoleAccess.SelectExecuteAccess) > 0)
                    sb.AppendLine($"GRANT UPDATE TO {applicationRoleName};");
                if ((access & AppRoleAccess.DeleteAccess & ~AppRoleAccess.SelectExecuteAccess) > 0)
                    sb.AppendLine($"GRANT DELETE TO {applicationRoleName};");
                if ((access & AppRoleAccess.AlterAccess & ~AppRoleAccess.SelectExecuteAccess) > 0)
                {
                    sb.AppendLine($"GRANT ALTER TO {applicationRoleName}; ");
                    sb.AppendLine($"GRANT REFERENCES TO {applicationRoleName}; ");
                    sb.AppendLine($"ALTER ROLE db_owner ADD MEMBER {applicationRoleName}; ");
                }
                sb.AppendLine("END");
                cmd.CommandText = sb.ToString();

                if (sqlConnection.State != System.Data.ConnectionState.Open) sqlConnection.Open();

                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI51758");
            }
        }
  
    }

}
