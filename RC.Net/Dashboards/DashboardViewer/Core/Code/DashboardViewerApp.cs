using DevExpress.LookAndFeel;
using DevExpress.Skins;
using DevExpress.UserSkins;
using Extract;
using Extract.Licensing;
using Extract.Utilities;
using System;
using System.Linq;
using System.Windows.Forms;

namespace Extract.DashboardViewer
{
    static class DashboardViewerApp
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            string exceptionFile = string.Empty;

            try
            {
                LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());
                LicenseUtilities.ValidateLicense(LicenseIdName.FlexIndexCoreObjects, "ELI45312",
                    Application.ProductName);

                if (args.Length > 9 || args.Contains("/?") || args.Contains("/h", StringComparer.OrdinalIgnoreCase))
                {
                    Usage();
                    return;
                }
                string dashboardFileName = string.Empty;
                string serverName = string.Empty;
                string databaseName = string.Empty;
                string dashboardName = string.Empty;

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
                    else if (args[a].Equals("/ef", StringComparison.OrdinalIgnoreCase))
                    {
                        ++a;
                        if (a >= args.Length)
                        {
                            Usage();
                            return;
                        }
                        exceptionFile = args[a];
                    }
                    else if (args[a].Equals("/b", StringComparison.OrdinalIgnoreCase) ||
                        args[a].Equals("/Dashboard", StringComparison.OrdinalIgnoreCase))
                    {
                        ++a;
                        if (a >= args.Length)
                        {
                            Usage();
                            return;
                        }
                        dashboardName = args[a];
                    }
                    else if (string.IsNullOrEmpty(dashboardFileName))
                    {
                        dashboardFileName = args[a];
                    }
                    else
                    {
                        Usage();
                        return;
                    }
                }

                // Either Server name and Database are both specified or neither or both dashboardFilename and DashboardName specified
                if ((string.IsNullOrWhiteSpace(serverName) ^ string.IsNullOrWhiteSpace(databaseName)) ||
                    (!string.IsNullOrWhiteSpace(dashboardFileName) && !string.IsNullOrWhiteSpace(dashboardName)))
                {
                    Usage();
                    return;
                }

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                BonusSkins.Register();
                SkinManager.EnableFormSkins();
                UserLookAndFeel.Default.SetSkinStyle("DevExpress Style");

                // if no dashboard file name is specified but there is a server and database open viewer in "database mode"
                if (string.IsNullOrWhiteSpace(dashboardFileName)
                        && !string.IsNullOrWhiteSpace(serverName)
                        && !string.IsNullOrWhiteSpace(databaseName))
                {
                    if (string.IsNullOrWhiteSpace(dashboardName))
                    {
                        Application.Run(new DashboardViewerForm(serverName, databaseName));
                    }
                    else
                    {
                        Application.Run(new DashboardViewerForm(dashboardName, true, serverName, databaseName));
                    }
                }
                else
                {
                    Application.Run(new DashboardViewerForm(dashboardFileName, false, serverName, databaseName));
                }

            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI45314", ex);
                if(string.IsNullOrWhiteSpace(exceptionFile))
                {
                    ee.Display();
                }
                else
                {
                    ee.Log(exceptionFile);
                }
            }
        }

        static void Usage()
        {
            UtilityMethods.ShowMessageBox("Usage: " +
                "\r\nDashboardViewer [<DashboardFileName>] [(/Server | /s) <ServerName> (/Database | /d) <DatabaseName> [(/Dashboard | /b) <DashboardName>]] " +
                "\r\n\r\n<DashboardFileName> -dashboard to open. " +
                "\r\n\r\n/Server or /s <ServerName> - Server name. Requires /Database." +
                "\r\n\r\n/Database or /d <DatabaseName> - Database name. Requires /Server." +
                "\r\n\r\nNote: When Server and database are specified on the command line any dashboard files opened will use the " +
                 "Server and Database specified on the command line. " +
                 "\r\n\r\n/Dashboard or /b <DashboardName> - Loads named dashboard from database, must specify database " +
                 "and server without DashboardFileName",
                 "Dashboard viewer usage", false);
        }
    }
}
