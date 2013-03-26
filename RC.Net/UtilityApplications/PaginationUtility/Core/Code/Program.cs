using Extract.Licensing;
using System;
using System.Windows.Forms;

namespace Extract.UtilityApplications.PaginationUtility
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
                // Load the licenses
                LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());

                // Validate that this is licensed
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects,
                    "ELI35543", "PaginationUtility");

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                PaginationUtilityForm paginationUtilityForm;
                if (args.Length == 0)
                {
                    paginationUtilityForm = new PaginationUtilityForm();
                }
                else
                {
                    paginationUtilityForm = new PaginationUtilityForm(args[0]);
                }

                Application.Run(paginationUtilityForm);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35544");
            }
        }
    }
}
