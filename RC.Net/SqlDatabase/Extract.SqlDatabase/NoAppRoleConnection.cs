using System.Data.Common;
using System.Data.SqlClient;

namespace Extract.SqlDatabase
{
    public sealed class NoAppRoleConnection : SqlAppRoleConnection
    {
        internal override string InternalRoleName => null;

        // This enables support for DbProviderFactories.GetFactory()
        protected override DbProviderFactory DbProviderFactory => NoAppRoleFactory.Instance;

        public NoAppRoleConnection(string server, string database)
            : base(SqlUtil.NewSqlDBConnection(server, database))
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
    }

    internal sealed class NoAppRoleFactory : BaseRoleFactory
    {
        public static readonly NoAppRoleFactory Instance = new NoAppRoleFactory();

        private NoAppRoleFactory()
        {
        }

        public override DbConnection CreateConnection()
        {
            return new NoAppRoleConnection(new SqlConnection());
        }
    }
}
