using Extract;
using Extract.Licensing;
using Extract.Utilities;
using System;
using System.Linq;
using System.Windows.Forms;


namespace ReportDesigner
{
    static class ReportDesignerApp
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());
                LicenseUtilities.ValidateLicense(LicenseIdName.DashboardCreator, "ELI49822", Application.ProductName);

                if (args.Length > 5 || args.Contains("/?") || args.Contains("/h", StringComparer.OrdinalIgnoreCase))
                {
                    Usage();
                }
                string reportFile = null;
                string reportName = null;
                string serverName = null;
                string databaseName = null;

                for (int a = 0; a < args.Length; a++)
                {
                    if (args[a].Equals("/d", StringComparison.OrdinalIgnoreCase) ||
                        args[a].Equals("/Database", StringComparison.OrdinalIgnoreCase))
                    {
                        ++a;
                        if (a >= args.Length)
                        {
                            Usage();
                            return;
                        }
                        databaseName = args[a];
                    }
                    else if (args[a].Equals("/s", StringComparison.OrdinalIgnoreCase) ||
                             args[a].Equals("/Server", StringComparison.OrdinalIgnoreCase))
                    {
                        ++a;
                        if (a >= args.Length)
                        {
                            Usage();
                            return;
                        }
                        serverName = args[a];
                    }
                    //else if (args[a].Equals("/r", StringComparison.OrdinalIgnoreCase) ||
                    //         args[a].Equals("/Report", StringComparison.OrdinalIgnoreCase))
                    //{
                    //    ++a;
                    //    if (a >= args.Length)
                    //    {
                    //        Usage();
                    //        return;
                    //    }
                    //    reportName = args[a];
                    //}
                    else if (string.IsNullOrEmpty(reportFile))
                    {
                        reportFile = args[a];
                    }
                    else
                    {
                        Usage();
                        return;
                    }

                }
                // Make sure only one report is specified
                if (reportName != null && reportFile != null)
                {
                    Usage();
                    return;
                }

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new ReportDesignerForm(reportFile, reportName, serverName, databaseName));
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI49821");
            }
        }

        private static void Usage()
        {
            UtilityMethods.ShowMessageBox("Usage: " +
                "\r\nReportDesigner [<ReportFileName>]  [(/Server | /s) <ServerName> (/Database | /d) <DatabaseName> " +
                //"(/Report | /r) <ReportName> ] " +
                "\r\n\r\n<ReportFileName> -Report to open. " +
                "\r\n\r\n/Server or /s <ServerName> - Server name. Requires /Database." +
                "\r\n\r\n/Database or /d <DatabaseName> - Database name. Requires /Server." +
                //"\r\n\r\n/Report or /r <ReportName> - *NOT IMPLEMENTED* - specifies report in " +
                //"given database. Requires Server and database parameters. Only valid if <ReportFileName> is not specified." +
                "\r\n\r\nNote: When Server and database are specified on the command line any report files opened will use the " +
                "Server and Database specified on the command line. ",
                                          "Report designer usage",
                                          false);
        }
    }
    
}
