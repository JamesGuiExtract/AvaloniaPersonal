using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extract.SqlDatabase
{
    public sealed class NoAppRoleConnection : ApplicationRoleConnection
    {
        public NoAppRoleConnection(SqlConnection sqlConnection)
            : base(sqlConnection)
        {
            AssignRole();
        }
        public NoAppRoleConnection(string server, string database, bool enlist = true)
            : base(server, database ,enlist)
        {
            AssignRole();
        }

        public NoAppRoleConnection(string connectionString)
            : base(connectionString)
        {
            AssignRole();
        }

        protected override void AssignRole()
        {
            // No role assigned
        }
    }
}
