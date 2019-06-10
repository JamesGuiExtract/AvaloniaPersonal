using DevExpress.DashboardCommon;
using DevExpress.DataAccess.ConnectionParameters;
using Extract.Interfaces;
using Extract.Utilities;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
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
        public XDocument ConvertDashboardDataSources(XDocument original, string serverName, string databaseName,
            string extractDataSourceDir)
        {
            try
            {
                using (var dashboard = new DevExpress.DashboardCommon.Dashboard())
                {
                    dashboard.LoadFromXDocument(original);
                    // Get all the SQL Data sources
                    var sqlDataSources = dashboard.DataSources
                        .OfType<DashboardSqlDataSource>().ToList();
                    foreach (var ds in sqlDataSources)
                    {
                        var sqlParameters = ds.ConnectionParameters as SqlServerConnectionParametersBase;
                        sqlParameters.DatabaseName = databaseName;
                        sqlParameters.ServerName = serverName;
                    }
                    AddExtractDataSources(dashboard, extractDataSourceDir);
                    return dashboard.SaveToXDocument();
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

        public static void AddExtractDataSources(DevExpress.DashboardCommon.Dashboard dashboard, string extractDataSourceDir)
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

                var existingExtractDataSources = dashboard.DataSources
                    .OfType<DashboardExtractDataSource>().ToList();

                foreach (var ds in sqlDataSources)
                {
                    foreach (var query in ds.Queries)
                    {
                        var sqlParameters = ds.ConnectionParameters as SqlServerConnectionParametersBase;
                        string extractDataSourceName = "ExtractData_" +
                            ds.Name +
                            sqlParameters.ServerName + "_" + sqlParameters.DatabaseName + "_" + query.Name;

                        // check if the extract data source already exists
                        if (existingExtractDataSources
                            .Where(ed => ed.ExtractSourceOptions.DataSource.Name == extractDataSourceName)
                            .Any())
                        {
                            continue;
                        }

                        DashboardExtractDataSource extractDataSource = new DashboardExtractDataSource();
                        extractDataSource.ExtractSourceOptions.DataSource = ds;
                        extractDataSource.ExtractSourceOptions.DataMember = query.Name;

                        extractDataSource.FileName =
                            Path.Combine(extractDataSourceDir, extractDataSourceName + ".dat");
                        extractDataSource.Name = extractDataSourceName;
                        extractDataSource.UpdateExtractFile();
                        extractDataSource.CalculatedFields.AddRange(ds.CalculatedFields);

                        dashboard.DataSources.Add(extractDataSource);

                        // Check all current items in the dashboard that use the ds and query and change them to the
                        // new extractDataSource
                        var dashboardItems = dashboard.Items
                            .Where(di => di is DataDashboardItem)
                            .Select(ddi => ddi as DataDashboardItem)
                            .Where(ddi => ddi.DataSource == ds && ddi.DataMember == query.Name);
                        foreach (var ddi in dashboardItems)
                        {
                            ddi.DataSource = extractDataSource;
                            ddi.DataMember = string.Empty;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI46965");
            }
        }

        /// <summary>
        /// Method to update all extracted data sources on a dashbaord
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
                    string existingDataFile = ds.FileName;
                    string tempFileName = FileSystemMethods.GetTemporaryFileName();
                    try
                    {
                        using (ds)
                        {
                            ds.FileName = tempFileName;
                            ds.UpdateExtractFile(cancelToken);
                        }

                        // Copy the newly extracted file to extracted file
                        File.Copy(tempFileName, existingDataFile, true);
                    }
                    catch (Exception ex)
                    {
                        var ee = new ExtractException("ELI46888", "Unable to update extracted datasource.", ex);
                        ee.AddDebugData("DatasourceName", ds.Name);
                        ee.AddDebugData("Extracted FileName", existingDataFile);
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
    }
}
