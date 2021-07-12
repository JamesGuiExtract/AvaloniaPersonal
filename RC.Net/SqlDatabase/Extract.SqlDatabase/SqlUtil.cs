using System.Data.SqlClient;

namespace Extract.SqlDatabase
{
    public static class SqlUtil
    {
        public static SqlConnection NewSqlDBConnection(string databaseServer, string databaseName, bool enlist = true)
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
            return new SqlConnection(sqlConnectionBuild.ConnectionString);
        }
    }
}
