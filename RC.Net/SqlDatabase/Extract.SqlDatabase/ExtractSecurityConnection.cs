using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extract.SqlDatabase
{
    public sealed class ExtractSecurityConnection : SqlAppRoleConnection
    {
        private static readonly string password = "Change2This3Password";
        private static readonly string role = "ExtractSecurityRole";

        public ExtractSecurityConnection(string server, string database, bool enlist = true)
            : base(SqlUtil.NewSqlDBConnection(server, database, enlist))
        {
        }

        public ExtractSecurityConnection(string connectionString)
            : base(SqlUtil.NewSqlDBConnection(connectionString))
        {
        }

        protected override void AssignRole()
        {
            SetApplicationRole(role, password);
        }
    }
}
