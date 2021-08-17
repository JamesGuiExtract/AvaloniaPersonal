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
            sqlConnectionStringBuilder.Pooling = false;
            return new SqlConnection(sqlConnectionStringBuilder.ConnectionString);
        }
        public static string CreateConnectionString(string databaseServer, string databaseName, bool enlist = true)
        {
            SqlConnectionStringBuilder sqlConnectionBuild = new SqlConnectionStringBuilder();
            sqlConnectionBuild.DataSource = databaseServer;
            sqlConnectionBuild.InitialCatalog = databaseName;
            sqlConnectionBuild.IntegratedSecurity = true;
            sqlConnectionBuild.NetworkLibrary = "dbmssocn";
            sqlConnectionBuild.MultipleActiveResultSets = true;
            sqlConnectionBuild.Enlist = enlist;
            sqlConnectionBuild.MultipleActiveResultSets = true;
            sqlConnectionBuild.Pooling = false;
            return sqlConnectionBuild.ConnectionString;
        }
    }
}
