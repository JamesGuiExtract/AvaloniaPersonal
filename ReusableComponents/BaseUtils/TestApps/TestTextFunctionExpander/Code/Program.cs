using Extract.BaseUtils.Testing.Properties;
using Extract.Licensing;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Extract.BaseUtils.Testing
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

                // Load the license files
                LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());

                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.FileActionManagerObjects,
                    "ELI28789", "TestTextFunctionExpander");

                // Create the form and set the icon
                TestForm form = new TestForm();
                form.Icon = Resources.TextFunctionExpander;

                Application.Run(form);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI28795", ex);
            }
        }
    }
}