using DevExpress.LookAndFeel;
using DevExpress.Skins;
using DevExpress.UserSkins;
using Extract;
using Extract.Licensing;
using System;
using System.Windows.Forms;

namespace DashboardViewer
{
    static class Program
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

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                BonusSkins.Register();
                SkinManager.EnableFormSkins();
                UserLookAndFeel.Default.SetSkinStyle("DevExpress Style");
                Application.Run(new DashboardViewerForm((args.Length == 1) ? args[0] : ""));

            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45314");
            }
        }
    }
}
