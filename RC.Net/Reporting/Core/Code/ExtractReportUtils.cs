using Extract.Utilities;
using Extract.Utilities.Forms;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace Extract.Reporting
{
    static public class ExtractReportUtils
    {
        /// <summary>
        /// Relative path to the reports folder (relative to the current applications directory).
        /// </summary>
        static public readonly string ReportFolderPath =
            Path.Combine(FileSystemMethods.CommonApplicationDataPath, "Reports");

        /// <summary>
        /// Full type name of the ExtractReport class
        /// </summary>
        static public readonly string ExtractReportTypeName = "Extract.ReportingDevExpress.ExtractReport";

        /// <summary>
        /// Full type name of the ReportViewerForm class
        /// </summary>
        static public readonly string ReportViewerTypeName = "Extract.ReportingDevExpress.ReportViewerForm";

        /// <summary>
        /// Folder which contains the standard reports
        /// </summary>
        static public readonly string StandardReportFolder =
            Path.Combine(ReportFolderPath, "Standard reports");

        /// <summary>
        /// Folder which contains the saved reports
        /// </summary>
        static public readonly string SavedReportFolder =
            Path.Combine(ReportFolderPath, "Saved reports");


        /// <summary>
        /// Creates ExtractReport object using the parameters and returns the interface
        /// </summary>
        /// <param name="serverName">Server name used in the report</param>
        /// <param name="databaseName">Database used in the report</param>
        /// <param name="workflowName">Workflow used in the report</param>
        /// <param name="fileName">Report file name</param>
        /// <param name="promptForParameters">If true will be prompted for parameters</param>
        /// <returns><see cref="IExtractReport"/> interface of the created ExtractReport</returns>
        static public IExtractReport CreateExtractReport(string serverName, string databaseName, string workflowName, string fileName, bool promptForParameters)
        {
            try
            {
                return UtilityMethods.CreateTypeFromTypeName(ExtractReportTypeName,
                                                      serverName,
                                                      databaseName,
                                                      workflowName,
                                                      fileName,
                                                      promptForParameters) as IExtractReport;
            }
            catch (Exception ex)
            {
                var ee = ex.AsExtract("ELI49908");
                ee.AddDebugData("Server name", serverName, false);
                ee.AddDebugData("Database name", databaseName, false);
                ee.AddDebugData("Workflow name", workflowName, false);
                ee.AddDebugData("File name", fileName, false);
                ee.AddDebugData("Prompt for parameters", promptForParameters, false);
                throw ee;
            }
        }

        /// <summary>
        /// Creates ExtractReport object using the parameters and returns the interface
        /// </summary>
        /// <param name="fileName">Report file name</param>
        /// <returns><see cref="IExtractReport"/> interface of the created ExtractReport</returns>
        static public IExtractReport CreateExtractReport(string fileName)
        {
            try
            {
                return UtilityMethods.CreateTypeFromTypeName(ExtractReportTypeName, fileName) as IExtractReport;
            }
            catch (Exception ex)
            {
                var ee = ex.AsExtract("ELI49909");
                ee.AddDebugData("File name", fileName, false);
                throw ee;
            }
        }

        /// <summary>
        /// Create a new report with the same configuration as the input report
        /// </summary>
        /// <param name="extractReport">Report to clone</param>
        /// <returns>Cloned report</returns>
        static public IExtractReport CreateExtractReport(IExtractReport extractReport)
        {
            try
            {
                return UtilityMethods.CreateTypeFromTypeName(ExtractReportTypeName, extractReport) as IExtractReport;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI49910");
            }
        }

        /// <summary>
        /// Creates a ReportViewerForm and displays the given Extract report
        /// </summary>
        /// <param name="extractReport">Report to display</param>
        /// <returns>The Form that displays the report</returns>
        public static Form CreateReportViewerForm(IExtractReport extractReport)
        {
            try
            {
                return UtilityMethods.CreateTypeFromTypeName(ReportViewerTypeName, extractReport) as Form;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI49911");
            }
        }

        /// <summary>
        /// Creates a ReportViewerForm and displays the given Extract report
        /// </summary>
        /// <param name="serverName">Server name for the report</param>
        /// <param name="databaseName">Database to use for the report</param>
        /// <param name="workflowName">Workflow for the report</param>
        /// <returns>The Form that displays the report</returns>
        public static Form CreateReportViewerForm(string serverName, string databaseName, string workflowName)
        {
            try
            {
                return UtilityMethods.CreateTypeFromTypeName(ReportViewerTypeName, serverName, databaseName, workflowName) as Form;
            }
            catch (Exception ex)
            {
                var ee = ex.AsExtract("ELI49912");
                ee.AddDebugData("Server name", serverName, false);
                ee.AddDebugData("Database name", databaseName, false);
                ee.AddDebugData("Workflow name", workflowName, false);
                throw ee;
            }
        }

        /// <summary>
        /// Creates a ReportViewerForm and displays the given Extract report
        /// </summary>
        /// <param name="report">Report to display with the given database</param>
        /// <param name="serverName">Server name for the report</param>
        /// <param name="databaseName">Database to use for the report</param>
        /// <param name="workflowName">Workflow for the report</param>
        /// <returns>The Form that displays the report</returns>
        public static Form CreateReportViewerForm(IExtractReport report, string serverName, string databaseName, string workflowName)
        {
            try
            {
                return UtilityMethods.CreateTypeFromTypeName(ReportViewerTypeName,
                                                             report,
                                                             serverName,
                                                             databaseName,
                                                             workflowName) as Form;
            }
            catch (Exception ex)
            {
                var ee = ex.AsExtract("ELI49913");
                ee.AddDebugData("Server name", serverName, false);
                ee.AddDebugData("Database name", databaseName, false);
                ee.AddDebugData("Workflow name", workflowName, false);
                throw ee;
            }
        }
    }
}
