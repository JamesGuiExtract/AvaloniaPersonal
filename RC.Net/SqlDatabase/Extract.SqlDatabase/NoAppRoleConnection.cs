using System.Data.Common;
using System.Data.SqlClient;

namespace Extract.SqlDatabase
{
    public sealed class NoAppRoleConnection : SqlAppRoleConnection
    {
        internal override string RoleName => null;
        internal override string RolePassword => null;

        // This enables support for DbProviderFactories.GetFactory()
        protected override DbProviderFactory DbProviderFactory => NoAppRoleFactory.Instance;

        public NoAppRoleConnection(string server, string database, bool enlist = true)
            : base(server, database, enlist)
        {
        }

        public NoAppRoleConnection(string connectionString)
            : base(connectionString)
        {
        }

        internal NoAppRoleConnection(SqlConnection connection)
            : base(connection)
        {
        }
    }
}
