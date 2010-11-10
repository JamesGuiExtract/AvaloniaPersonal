using Extract.Licensing;
using System;
using System.Windows.Forms;

namespace Extract.FileActionManager.Utilities
{
    static class FAMDBLockCheckerProgram
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
                LicenseUtilities.ValidateLicense(LicenseIdName.FileActionManagerObjects,
                    "ELI31046", "FAMDBLockChecker");

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new FAMDBLockCheckerForm());
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI31047", ex);
            }
        }
    }
}
