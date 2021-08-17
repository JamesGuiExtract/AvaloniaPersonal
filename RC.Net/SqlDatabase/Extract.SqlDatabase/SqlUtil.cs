using System.Data.SqlClient;

namespace Extract.SqlDatabase
{
    public static class SqlUtil
    {
        public static SqlConnection NewSqlDBConnection(string databaseServer, string databaseName, bool enlist = true)
        {
            try
            {
                // Build the connection string from the settings
                SqlConnectionStringBuilder sqlConnectionBuild = new SqlConnectionStringBuilder();
                sqlConnectionBuild.DataSource = databaseServer;
                sqlConnectionBuild.InitialCatalog = databaseName;
                sqlConnectionBuild.IntegratedSecurity = true;
                sqlConnectionBuild.NetworkLibrary = "dbmssocn";
                sqlConnectionBuild.MultipleActiveResultSets = true;
                sqlConnectionBuild.Enlist = enlist;
                sqlConnectionBuild.MultipleActiveResultSets = true;
                sqlConnectionBuild.Pooling = false;
                return new SqlConnection(sqlConnectionBuild.ConnectionString);
            }
            catch (System.Exception ex)
            {
                ExtractException ee = ex.AsExtract("ELI51774");
                ee.AddDebugData("DatabaseServer", databaseServer);
                ee.AddDebugData("DatabaseName", databaseName);
                throw ee;
            }
        }
    }
}
