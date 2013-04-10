using Extract.Licensing;
using System;
using System.Text;
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
                LicenseUtilities.ValidateLicense(LicenseIdName.PaginationUIObject,
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
                    string argument = args[0];

                    if (argument == "/?")
                    {
                        ShowUsage();
                        return;
                    }

                    paginationUtilityForm = new PaginationUtilityForm(argument);
                }

                Application.Run(paginationUtilityForm);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35544");
            }
        }

        /// <summary>
        /// Displays the usage message.
        /// </summary>
        static void ShowUsage()
        {
            StringBuilder usage = new StringBuilder();

            // Add the command line syntax
            usage.Append(Environment.GetCommandLineArgs()[0]);
            usage.AppendLine(" </?>| [<filename>]");
            usage.AppendLine();
            usage.AppendLine("Options:");
            usage.AppendLine();
            usage.AppendLine("    /? - Display help");
            usage.AppendLine("    filename - A config file whose settings should be used.");
            usage.AppendLine("      (The settings will not be editable)");

            // Display the usage as an error or as an information box
            MessageBox.Show(usage.ToString(), "Usage", MessageBoxButtons.OK,
                MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, 0);
        }
    }
}
