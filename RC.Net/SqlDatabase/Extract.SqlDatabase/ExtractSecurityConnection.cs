using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extract.SqlDatabase
{
    public sealed class ExtractSecurityConnection : ApplicationRoleConnection
    {
        private static readonly string password = "Change2This3Password";
        private static readonly string role = "ExtractSecurityRole";

        public ExtractSecurityConnection(SqlConnection sqlConnection)
            : base(sqlConnection)
        {
            AssignRole();
        }
        public ExtractSecurityConnection(string server, string database, bool enlist = true)
            : base(server, database, enlist)
        {
            AssignRole();
        }

        public ExtractSecurityConnection(string connectionString)
            : base(connectionString)
        {
            AssignRole();
        }

        protected override void AssignRole()
        {
            ApplicationRole = new SqlApplicationRole(SqlConnection, role, password);
        }
    }
}
