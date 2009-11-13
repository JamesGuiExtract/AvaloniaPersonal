using Extract.Licensing;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Extract.IDShieldStatisticsReporter
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                // Validate the license
                LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());
                LicenseUtilities.ValidateLicense(LicenseIdName.FlexIndexIDShieldCoreObjects,
                    "ELI28533", "ID Shield Statistics Reporter");

                Application.Run(new IDShieldStatisticsReporterForm());
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI28536", ex);

                ee.Display();
            }
        }
    }
}