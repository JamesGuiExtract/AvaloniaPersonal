using Extract.Licensing;
using System;
using System.Windows.Forms;

namespace VerifierWorkflowConfig
{
    static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            //// Load license files from folder
            //LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());

            //// Validate the license
            //LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects,
            //    "ELI40343", "Data entry prompt utility is not licensed.");

            Application.Run(new CreateRedactionTrainingWorkflowForm());
        }
    }
}
