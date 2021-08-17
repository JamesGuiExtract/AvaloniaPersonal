using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extract.SqlDatabase
{
    public sealed class ExtractRoleConnection : SqlAppRoleConnection
    {
        private static readonly string password = "Change2This3Password";
        private static readonly string role = "ExtractRole";

        public ExtractRoleConnection(string server, string database, bool enlist = true)
            : base(SqlUtil.NewSqlDBConnection(server, database, enlist))
        {
        }

        public ExtractRoleConnection(string connectionString)
            : base(SqlUtil.NewSqlDBConnection(connectionString))
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
