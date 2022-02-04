using Extract.AttributeFinder.Rules;
using Extract.Licensing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Extract.UtilityApplications.ExpressionAndQueryTester
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
                // Load the licenses
                LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());

                // Validate that this is licensed
                LicenseUtilities.ValidateLicense(LicenseIdName.RuleDevelopmentToolkitObjects,
                    "ELI34455", "ExpressionAndQueryTester");

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new ExpressionAndQueryTesterForm());
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI34456");
            }
        }
    }
}
