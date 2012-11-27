using Extract.Licensing;
using System;
using System.Windows.Forms;

namespace Extract.DataEntryPrompt
{
    /// <summary>
    /// The main application class.
    /// <para><b>Note:</b></para>
    /// This application is intentionally built without any dependencies on other extract assemblies.
    /// This means it is not licensed and does not use ExtractExceptions.
    /// </summary>
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

                // Load license files from folder
                LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());

                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects,
                    "ELI35256", "Data entry prompt utility is not licensed.");

                Application.Run(new DataEntryPromptForm());
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35257");
            }
        }
    }
}
