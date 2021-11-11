using System.Data.Common;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Security;
using System.Security.Permissions;

namespace Extract.SqlDatabase
{
    public class ExtractRoleFactory: DbProviderFactory
    {
        public static readonly ExtractRoleFactory Instance = new ExtractRoleFactory();

        private ExtractRoleFactory()
        {
        }

        public override bool CanCreateDataSourceEnumerator => true;

        public override DbCommand CreateCommand()
        {
            return new SqlCommand();
        }

        public override DbCommandBuilder CreateCommandBuilder()
        {
            return new SqlCommandBuilder();
        }

        public override DbConnectionStringBuilder CreateConnectionStringBuilder()
        {
            return new SqlConnectionStringBuilder();
        }

        public override DbDataAdapter CreateDataAdapter()
        {
            return new SqlDataAdapter();
        }

        public override DbParameter CreateParameter()
        {
            return new SqlParameter();
        }

        public override CodeAccessPermission CreatePermission(PermissionState state)
        {
            return new SqlClientPermission(state);
        }

        public override DbDataSourceEnumerator CreateDataSourceEnumerator()
        {
            return SqlDataSourceEnumerator.Instance;
        }

        public override DbConnection CreateConnection()
        {
             return new ExtractRoleConnection(new SqlConnection());
        }
    }
}
