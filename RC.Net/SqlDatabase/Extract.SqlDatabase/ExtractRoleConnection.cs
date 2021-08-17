using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extract.SqlDatabase
{
    public sealed class ExtractRoleConnection : ApplicationRoleConnection
    {
        private static readonly string password = "Change2This3Password";
        private static readonly string role = "ExtractRole";

        public ExtractRoleConnection(SqlConnection sqlConnection)
            : base(sqlConnection)
        {
            AssignRole();
        }
        public ExtractRoleConnection(string server, string database, bool enlist = true)
            : base(server, database, enlist)
        {
            AssignRole();
        }

        public ExtractRoleConnection(string connectionString)
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
