using Extract.Utilities;
using System;
using System.Data.SqlClient;

namespace Extract.SqlDatabase
{
    public sealed class ExtractRoleConnection : SqlAppRoleConnection
    {
        private static readonly string password = "Change2This3Password";
        private static readonly string role = "ExtractRole";


        static ExtractRoleConnection()
        {
            ExtractRoleFactory.RegisterProviderForInstance();
        }

        public ExtractRoleConnection(string server, string database, bool enlist = true)
            : base(SqlUtil.NewSqlDBConnection(server, database, enlist))
        {
        }

        public ExtractRoleConnection(string connectionString)
            : base(SqlUtil.NewSqlDBConnection(connectionString))
        {
        }

        internal ExtractRoleConnection(SqlConnection connection)
            :base(connection)
        {
        }

        protected override void AssignRole()
        {
            try
            {
                SetApplicationRole(role, password);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI51836");
            }
        }
    }
}
