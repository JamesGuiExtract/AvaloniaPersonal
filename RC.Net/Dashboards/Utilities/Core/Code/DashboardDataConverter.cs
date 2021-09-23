using DevExpress.DashboardCommon;
using DevExpress.DataAccess.ConnectionParameters;
using Extract.Interfaces;
using Extract.Utilities;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml.Linq;

namespace Extract.Dashboard.Utilities
{
    /// <summary>
    /// Class to implement IDashboardDataConverter
    /// This class is created indirectly with its name
    /// </summary>
    public class DashboardDataConverter : IDashboardDataConverter
    {
        /// <summary>
        /// Converts all sql datasources to Extracted datasources
        /// </summary>
        /// <param name="dashboardName">Name of dashboard</param>
        /// <param name="original">Original XDocument</param>
        /// <param name="serverName">Server name the dashboard is in</param>
        /// <param name="databaseName">Database dashboard is in</param>
        /// <param name="extractDataSourceDir">Directory to store the Extracted data files</param>
        /// <returns>Modified XDocument that uses the Extracted datasource or <c>null</c> if the the dashboard uses parameters</returns>
        public XDocument ConvertDashboardDataSources(string dashboardName, XDocument original, string serverName, string databaseName,
            string extractDataSourceDir)
        {
            try
            {
                using (var dashboard = new DevExpress.DashboardCommon.Dashboard())
                {
                    dashboard.LoadFromXDocument(original);
                    DashboardHelpers.AddAppRoleQuery(dashboard);
                    ExtractCustomData originalCustomData = new ExtractCustomData(original);

                    // Get all the SQL Data sources
                    var sqlDataSources = dashboard.DataSources
                        .OfType<DashboardSqlDataSource>().ToList();
                    foreach (var ds in sqlDataSources)
                    {
                        ds.ConnectionParameters = ds.ConnectionParameters?
                            .CreateConnectionParametersForReadOnly(serverName, databaseName, "Extract datasource converter") ?? ds.ConnectionParameters;

                        if (ds.Queries.Any(q => q.Parameters.Count() > 0))
                        {
                            return null;
                        }
                    }
                    AddExtractDataSources(dashboardName, dashboard, extractDataSourceDir);
                    var updatedXDoc = dashboard.SaveToXDocument();
                    return originalCustomData.AddExtractCustomDataToDashboardXml(updatedXDoc);
                }
            }
            catch (Exception ex)
            {
                var ee = new ExtractException("ELI46877", "Unable to create extracted datasources.", ex);
                ee.AddDebugData("ServerName", serverName);
                ee.AddDebugData("DatabaseName", databaseName);
                ee.AddDebugData("ExtractDataSource", extractDataSourceDir);
                throw ee;
            }
        }

        /// <summary>
        /// Adds Extracted data sources if none of the Sql data sources use parameters in there queries and if 
        /// Extracted data sources are added changes the dashboard to use the Extracted data sources
        /// </summary>
        /// <param name="dashboardName">Name of dashboard</param>
        /// <param name="dashboard">Dashboard being converted</param>
        /// <param name="extractDataSourceDir">Directory to store the Extracted data files</param>
        public static void AddExtractDataSources(string dashboardName, DevExpress.DashboardCommon.Dashboard dashboard, string extractDataSourceDir)
        {
            try
            {
                if (!Directory.Exists(extractDataSourceDir))
                {
                    Directory.CreateDirectory(extractDataSourceDir);
                }

                // Get all the SQL Data sources
                var sqlDataSources = dashboard.DataSources
                    .OfType<DashboardSqlDataSource>().ToList();

                // Check if any of the data sources use queries that have parameters
                if (sqlDataSources.Any(d => d.Queries.Any(q => q.Parameters.Count > 0)))
                {
                    ExtractException ee = new ExtractException("ELI46999", "Data sources contain queries that use parameters.");
                    throw ee;
                }
                foreach (var ds in sqlDataSources)
                {
                    CreateAndUseExtractedDataSources(dashboardName, dashboard, extractDataSourceDir, ds);
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI46965");
            }
        }

        /// <summary>
        /// Method to update all extracted data sources on a dashboard
        /// </summary>
        /// <param name="dashboard">Dashboard to that has the data sources that need updated</param>
        /// <param name="cancelToken"><see cref="CancellationToken"/> that can be used to cancel the process</param>
        public static void UpdateExtractedDataSources(DevExpress.DashboardCommon.Dashboard dashboard, CancellationToken cancelToken)
        {
            try
            {
                var existingExtractDataSources = dashboard.DataSources
                    .OfType<DashboardExtractDataSource>().ToList();

                foreach (var ds in existingExtractDataSources)
                {
                    cancelToken.ThrowIfCancellationRequested();
                    string tempFileName = FileSystemMethods.GetTemporaryFileName();

                    try
                    {
                        Retry<Exception> retry = new Retry<Exception>(1, 1);

                        retry.DoRetry(() =>
                        {
                            using var tempDS = new DashboardExtractDataSource();
                            // this is a clone of the datasource
                            tempDS.LoadFromXml(ds.SaveToXml());
                            tempDS.FileName = tempFileName;
                            var sqlDataSource = tempDS.ExtractSourceOptions.DataSource as DashboardSqlDataSource;
                            DashboardHelpers.AddAppRoleQuery(sqlDataSource);
                            sqlDataSource.Connection.Open();
                            sqlDataSource.Fill(sqlDataSource.Queries[0].Name);
                            tempDS.UpdateExtractFile(cancelToken);
                        });

                        // Copy the newly extracted file to extracted file
                        File.Copy(tempFileName, ds.FileName, true);
                    }
                    catch (Exception ex)
                    {
                        var ee = new ExtractException("ELI46888", "Unable to update extracted datasource.", ex);
                        ee.AddDebugData("DatasourceName", ds.Name);
                        ee.AddDebugData("Extracted FileName", ds.FileName);
                        ee.AddDebugData("Temp extracted filename", tempFileName);
                        throw ee;
                    }
                    finally
                    {
                        File.Delete(tempFileName);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI46969");
            }
        }

        #region Private Methods

        static void CreateAndUseExtractedDataSources(string dashboardName, DevExpress.DashboardCommon.Dashboard dashboard,
                                                string extractDataSourceDir,
                                                DashboardSqlDataSource ds)
        {

            var existingExtractDataSources = dashboard.DataSources
                .OfType<DashboardExtractDataSource>().ToList();

            foreach (var query in ds.Queries)
            {
                var builder = ds.ConnectionParameters.CreateSqlConnectionBuilderFromParameters();
                if (builder is null)
                    return;
                
                string extractDataSourceName = "ExtractData_" + dashboardName + "_" +
                    ds.Name +
                    builder.DataSource.Replace('\\','_').Replace('/','_') + 
                    "_" + builder.InitialCatalog + "_" + query.Name;

                // check if the extract data source already exists
                if (existingExtractDataSources
                    .Where(ed => ed.ExtractSourceOptions.DataSource.Name == extractDataSourceName)
                    .Any())
                {
                    continue;
                }

                ds.ConnectionParameters = ds.ConnectionParameters
                    .CreateConnectionParametersForReadOnly(string.Empty, string.Empty, string.Empty);

                DashboardExtractDataSource extractDataSource =
                    CreateExtractedDataSource(extractDataSourceDir, ds, query.Name, extractDataSourceName);

                dashboard.DataSources.Add(extractDataSource);

                ReplaceDataSourcesWithNew(dashboard, ds, query.Name, extractDataSource);
            }
        }

        static void ReplaceDataSourcesWithNew(DevExpress.DashboardCommon.Dashboard dashboard,
            DashboardSqlDataSource ds, string queryName, DashboardExtractDataSource extractDataSource)
        {
            // Check all current items in the dashboard that use the ds and query and change them to the
            // new extractDataSource
            var dashboardItems = dashboard.Items
                .Where(di => di is DataDashboardItem)
                .Select(ddi => ddi as DataDashboardItem)
                .Where(ddi => ddi.DataSource == ds && ddi.DataMember == queryName);
            foreach (var ddi in dashboardItems)
            {
                ddi.DataSource = extractDataSource;
                ddi.DataMember = string.Empty;
            }
        }

        static DashboardExtractDataSource CreateExtractedDataSource(string extractDataSourceDir,
            DashboardSqlDataSource ds, string queryName, string extractDataSourceName)
        {
            DashboardExtractDataSource extractDataSource = new DashboardExtractDataSource
            {
                FileName = Path.Combine(extractDataSourceDir, extractDataSourceName + ".dat"),
                Name = extractDataSourceName
            };
            extractDataSource.ExtractSourceOptions.DataSource = ds;
            extractDataSource.ExtractSourceOptions.DataMember = queryName;

            extractDataSource.UpdateExtractFile();
            extractDataSource.CalculatedFields.AddRange(ds.CalculatedFields);
            return extractDataSource;
        }

        #endregion
    }
}
