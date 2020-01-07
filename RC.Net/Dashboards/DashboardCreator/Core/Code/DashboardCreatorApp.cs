using DevExpress.Skins;
using DevExpress.UserSkins;
using Extract;
using Extract.Licensing;
using Extract.Utilities;
using System;
using System.Linq;
using System.Windows.Forms;

namespace DashboardCreator
{
    static class DashboardCreatorApp
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
                LicenseUtilities.ValidateLicense(LicenseIdName.DashboardCreator, "ELI45312",
                    Application.ProductName);

                if (args.Length > 1 || args.Contains("/?") || args.Contains("/h", StringComparer.OrdinalIgnoreCase))
                {
                    Usage();
                }
                string fileName = null;
                for (int a = 0; a < args.Length; a++)
                {
                    if (string.IsNullOrEmpty(fileName))
                    {
                        fileName = args[a];
                    }
                }

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                BonusSkins.Register();
                SkinManager.EnableFormSkins();
                Application.Run(new DashboardCreatorForm(fileName));
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45313");
            }
        }

        static void Usage()
        {
            UtilityMethods.ShowMessageBox("Usage: DashboardCreator [<DashboardFileName>]", "DashboardCreator usage", false);
        }
    }
}
