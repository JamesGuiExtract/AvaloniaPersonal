using System;
using System.Configuration;
using System.Data;
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
            sqlConnectionBuild.MultipleActiveResultSets = false;
            sqlConnectionBuild.Enlist = enlist;
            sqlConnectionBuild.Pooling = false;
            return sqlConnectionBuild.ConnectionString;
        }

        public static void RegisterProviderForInstance(string name, string description, Type type)
        {
            try
            {
                var systemData = ConfigurationManager.GetSection("system.data") as DataSet;
                int indexOfFactories = systemData.Tables.IndexOf("DbProviderFactories");
                DataTable factories;
                DataRow extractRoleProviderRow;
                if (indexOfFactories >= 0)
                {
                    factories = systemData.Tables[indexOfFactories];
                    extractRoleProviderRow = factories.Rows.Find(type.FullName);
                    if (extractRoleProviderRow != null) return;
                }
                else
                {
                    factories = systemData.Tables.Add("DbProviderFactories");
                }
                string assemblyQualifiedName = NoAppRoleFactory.Instance.GetType().AssemblyQualifiedName;
                factories.Rows.Add(
                    name
                    , description
                    , type.FullName
                    , type.AssemblyQualifiedName);
            }
            catch (Exception ex)
            {
                ex.ExtractLog("ELI51881", $"Unable to register {type.AssemblyQualifiedName}");
            }
        }
    }
}
