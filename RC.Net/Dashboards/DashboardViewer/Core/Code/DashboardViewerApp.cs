using DevExpress.LookAndFeel;
using DevExpress.Skins;
using DevExpress.UserSkins;
using Extract;
using Extract.Licensing;
using Extract.Utilities;
using System;
using System.Windows.Forms;

namespace DashboardViewer
{
    static class DashboardViewerApp
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
                LicenseUtilities.ValidateLicense(LicenseIdName.FlexIndexCoreObjects, "ELI45312",
                    Application.ProductName);

                if (args.Length > 5)
                {
                    Usage();
                    return;
                }
                string dashboardFileName = string.Empty;
                string serverName = string.Empty;
                string databaseName = string.Empty;

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

                // Either Server name and Database are both specified or neither
                if (string.IsNullOrWhiteSpace(serverName) ^ string.IsNullOrWhiteSpace(databaseName))
                {
                    Usage();
                    return;
                }

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                BonusSkins.Register();
                SkinManager.EnableFormSkins();
                UserLookAndFeel.Default.SetSkinStyle("DevExpress Style");
                Application.Run(new DashboardViewerForm(dashboardFileName, serverName, databaseName));

            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45314");
            }
        }

        static void Usage()
        {
            UtilityMethods.ShowMessageBox("Usage: " +
                "\r\nDashboardViewer [<DashboardFileName>] [(/Server | /s) <ServerName> (/Database | /d) <DatabaseName>] " +
                "\r\n\r\n<DashboardFileName> -dashboard to open. " +
                "\r\n\r\n/Server or /s <ServerName> - Server name. Requires /Database." +
                "\r\n\r\n/Database or /d <DatabaseName> - Database name. Requires /Server." +
                "\r\n\r\nNote: When Server and database are specified on the command line any dashboard files opened will use the " +
                 "Server and Database specified on the command line. ",
                 "Dashboard viewer usage", false);
        }
    }
}
