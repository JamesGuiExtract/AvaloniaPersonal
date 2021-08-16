using Extract.Licensing;
using System;
using static Extract.FileActionManager.Utilities.FAMServiceManager.Program;

namespace Extract.FileActionManager.Utilities.FAMServiceManager.Views
{
    static class Program
    {
        [STAThread]
        public static void Main()
        {
            // Load the licenses
            LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());

            // Validate that this is licensed
            LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects,
                "ELI51806", "FAMServiceManager Application");

            main(new MainWindow());
        }
    }
}
