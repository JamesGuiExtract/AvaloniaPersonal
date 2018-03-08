using DevExpress.Skins;
using DevExpress.UserSkins;
using Extract;
using Extract.Licensing;
using System;
using System.Windows.Forms;

namespace DashboardCreator
{
    static class DashboardCreatorApp
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            try
            {
                LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());
                LicenseUtilities.ValidateLicense(LicenseIdName.FlexIndexCoreObjects, "ELI45312",
                    Application.ProductName);

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                BonusSkins.Register();
                SkinManager.EnableFormSkins();
                Application.Run(new DashboardCreatorForm());
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45313");
            }
        }
    }
}
