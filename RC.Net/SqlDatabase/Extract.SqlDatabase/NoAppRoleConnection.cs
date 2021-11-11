using System.Data.Common;
using System.Data.SqlClient;

namespace Extract.SqlDatabase
{
    public sealed class NoAppRoleConnection : SqlAppRoleConnection
    {
        // This enables support for DbProviderFactories.GetFactory()
        protected override DbProviderFactory DbProviderFactory => NoAppRoleFactory.Instance;

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
