using System.Data.SqlClient;

namespace Extract.SqlDatabase
{
    public static class SqlUtil
    {
        public static SqlConnection NewSqlDBConnection(string databaseServer, string databaseName, bool enlist = true)
        {
            try
            {

                return new SqlConnection(SqlUtil.CreateConnectionString(databaseServer,databaseName, enlist));
            }
            catch (System.Exception ex)
            {
                ExtractException ee = ex.AsExtract("ELI51774");
                ee.AddDebugData("DatabaseServer", databaseServer);
                ee.AddDebugData("DatabaseName", databaseName);
                throw ee;
            }
        }

        public static SqlConnection NewSqlDBConnection(string connectionString)
        {
            SqlConnectionStringBuilder sqlConnectionStringBuilder = new(connectionString);
            sqlConnectionStringBuilder.Pooling = SqlAppRoleConnection.UseConnectionPooling;
            return new SqlConnection(sqlConnectionStringBuilder.ConnectionString);
        }

        public static string CreateConnectionString(string databaseServer, string databaseName, bool enlist = true)
        {
            SqlConnectionStringBuilder sqlConnectionBuild = new()
            {
                DataSource = databaseServer,
                InitialCatalog = databaseName,
                IntegratedSecurity = true,
                NetworkLibrary = "dbmssocn",
                Enlist = enlist,

                Pooling = SqlAppRoleConnection.UseConnectionPooling,

                // https://extract.atlassian.net/browse/ISSUE-17693
                // To avoid "Impersonate Session Security Context" exceptions when using application
                // role authentication, MARS need to be disabled.
                MultipleActiveResultSets = false
            };

            return sqlConnectionBuild.ConnectionString;
        }
    }
}
