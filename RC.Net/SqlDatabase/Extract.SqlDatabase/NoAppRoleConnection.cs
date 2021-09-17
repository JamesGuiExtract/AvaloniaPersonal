using Extract.Utilities;
using System;
using System.Data.SqlClient;

namespace Extract.SqlDatabase
{
    public sealed class NoAppRoleConnection : SqlAppRoleConnection
    {

        static NoAppRoleConnection()
        {
            NoAppRoleFactory.RegisterProviderForInstance();
        }

        public NoAppRoleConnection(string server, string database, bool enlist = true)
            : base(SqlUtil.NewSqlDBConnection(server, database, enlist))
        {
        }

        public NoAppRoleConnection(string connectionString)
            : base(SqlUtil.NewSqlDBConnection(connectionString))
        {
        }

        internal NoAppRoleConnection(SqlConnection connection)
            : base(connection)
        {
        }

        protected override void AssignRole()
        {
            // No role assigned
        }
    }
}
