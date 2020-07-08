using Extract.Utilities;
using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Extract.Dashboard.Forms
{
    public static class DashboardMethods
    {
        static readonly string CoreDashboardPath = Path.Combine(FileSystemMethods.CommonApplicationDataPath,
                                                                "Dashboards\\Core");

        static readonly string DashboardsQuery = "SELECT [DashboardName], [Definition] FROM [dbo].[Dashboard] ";

        static readonly string DashboardsWithDocumentOrFileNameQuery = DashboardsQuery +
            " WHERE " +
            @" [Definition].[exist]('/Dashboard/Parameters/Parameter[@Name=""FileName""]') = 1 " +
            @" OR [Definition].[exist] ('/Dashboard/Parameters/Parameter[@Name=""DocumentName""]') = 1";

        /// <summary>
        /// Gets collection of all dashboards in the "c:\ProgramData\Extract Systems\Dashboards\Core" folder
        /// </summary>
        /// <param name="withDocumentParameter">If true only dashboards with a FileName or DocumentName parameter that 
        /// has no default value will be returned</param>
        /// <returns>Returns collection of SourceLinks for dashboards from the "c:\ProgramData\Extract Systems\Dashboards\Core" 
        /// folder</returns>
        public static Collection<SourceLink> GetCoreDashboards(bool withDocumentParameter)
        {
            try
            {
                var CoreDashboards = new Collection<SourceLink>();

                // Search all core dashboards
                foreach (string DashboardFileName in
                    Directory.EnumerateFiles(CoreDashboardPath, "*.esdx", SearchOption.AllDirectories))
                {
                    var dashboardDefinition = XDocument.Load(DashboardFileName);

                    if (!withDocumentParameter || HasDocumentParameter(dashboardDefinition))
                    {
                        CoreDashboards.Add(new SourceLink(DashboardFileName, true));
                    }
                }

                return CoreDashboards;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI49921");
            }
        }

        /// <summary>
        /// Get collection of dashboards from the database based on the parameters
        /// </summary>
        /// <param name="server">Server to connect to</param>
        /// <param name="database">Database to connect to</param>
        /// <param name="withDocumentParameter">If true will only return dashboards that have FileName or DocumentName 
        /// parameter with no default value</param>
        /// <returns>Returns Collection SourceLinks for dashboards from the database based on the withDocumentParameter</returns>
        public static Collection<SourceLink> GetDashboardsFromDatabase(string server, string database, bool withDocumentParameter)
        {
            try
            {
                var DashboardNames = new Collection<SourceLink>();

                using (var connection = NewSqlDBConnection(server, database))
                using (var command = connection.CreateCommand())
                {
                    connection.Open();
                    command.CommandText = withDocumentParameter ? DashboardsWithDocumentOrFileNameQuery : DashboardsQuery;

                    DataTable dataTable = new DataTable();
                    dataTable.Locale = CultureInfo.CurrentCulture;
                    dataTable.Load(command.ExecuteReader());


                    foreach (var row in dataTable.Rows.OfType<DataRow>())
                    {
                        var name = (string)row["DashboardName"];
                        var xdoc = XDocument.Parse((string)row["Definition"]);
                        if (!withDocumentParameter || HasDocumentParameter(xdoc))
                        {
                            DashboardNames.Add(new SourceLink(name, false));
                        }
                    }
                }

                return DashboardNames;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI49928");
            }
        }

        /// <summary>
        /// Returns Collection of SourceLinks from both "C:\ProgramData\Extract Systems\Dashboards\Core" and the database
        /// </summary>
        /// <param name="server">Server to connect to</param>
        /// <param name="database">Database to connect to</param>
        /// <param name="withDocumentParameter">If true will only return dashboards that have FileName or DocumentName 
        /// <returns>Returns Collection SourceLinks for dashboards from the database and ProgramData based on the withDocumentParameter</returns>
        public static Collection<SourceLink> GetAllDashboards(string server, string database, bool withDocumentParameter = false)
        {
            try
            {
                var dashboards = GetDashboardsFromDatabase(server, database, withDocumentParameter);
                return new Collection<SourceLink>(dashboards.Concat(GetCoreDashboards(withDocumentParameter)).ToList());
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI49927");
            }
        }

        /// <summary>
        /// Returns a connection to the configured database. 
        /// </summary>
        /// <param name="DatabaseServer">Server Name to connect to</param>
        /// <param name="DatabaseName">Database Name to connect to</param>
        /// <returns></returns>
        private static SqlConnection NewSqlDBConnection(string DatabaseServer, string DatabaseName)
        {
            // Build the connection string from the settings
            SqlConnectionStringBuilder sqlConnectionBuild = new SqlConnectionStringBuilder();
            sqlConnectionBuild.DataSource = DatabaseServer;
            sqlConnectionBuild.InitialCatalog = DatabaseName;
            sqlConnectionBuild.IntegratedSecurity = true;
            sqlConnectionBuild.NetworkLibrary = "dbmssocn";
            sqlConnectionBuild.MultipleActiveResultSets = true;
            return new SqlConnection(sqlConnectionBuild.ConnectionString);
        }

        private static bool HasDocumentParameter(XDocument xDocument)
        {
            var FileNameElement = xDocument.XPathSelectElement("/Dashboard/Parameters/Parameter[@Name=\"FileName\"]");
            var DocumentNameElement = xDocument.XPathSelectElement("/Dashboard/Parameters/Parameter[@Name=\"DocumentName\"]");

            return FileNameElement != null &&
                string.IsNullOrWhiteSpace(FileNameElement.Value) ||
                DocumentNameElement != null &&
                string.IsNullOrWhiteSpace(DocumentNameElement.Value);
        }
    }
}
