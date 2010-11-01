using Extract.Licensing;
using System;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Extract.FileActionManager.Utilities
{
    static class FAMNetworkManagerProgram
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                if (args.Length > 1)
                {
                    ShowUsage("Too many arguments specified.");
                    return;
                }
                else if (args.Length == 1 && args[0].Equals("/?", StringComparison.Ordinal))
                {
                    ShowUsage();
                    return;
                }

                // Get the file name from the command line
                string fileName = args.Length == 1 ? Path.GetFullPath(args[0]) : null;
                if (!string.IsNullOrEmpty(fileName) && !File.Exists(fileName))
                {
                    ExtractException ee = new ExtractException("ELI30820",
                        "The specified file does not exist.");
                    ee.AddDebugData("File To Open", fileName, false);
                    throw ee;
                }

                // Load and validate the license
                LicenseUtilities.LoadLicenseFilesFromFolder(0, new Extract.Licensing.MapLabel());
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI30982",
                    "FAM Network Manager Application");

                FAMNetworkDashboardForm form = new FAMNetworkDashboardForm(fileName);
                form.Icon = Properties.Resources.FamNetworkManager;
                Application.Run(form);
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI30798", ex);
            }
        }

        /// <summary>
        /// Shows the usage.
        /// </summary>
        /// <param name="errorMessage">The error message.</param>
        static void ShowUsage(string errorMessage = null)
        {
            StringBuilder sb = new StringBuilder();
            if (!string.IsNullOrEmpty(errorMessage))
            {
                sb.Append("Error: ");
                sb.AppendLine(errorMessage);
            }

            sb.AppendLine("Usage:");
            sb.AppendLine("FAMNetworkManager.exe [<FileToOpen>]|[/?]");
            sb.AppendLine("-----------------------------------------");
            sb.AppendLine("/? - Display usage message");
            sb.AppendLine("<FileToOpen> - The name of the '.fnm' file to load on opening");

            MessageBox.Show(sb.ToString(), "Usage", MessageBoxButtons.OK,
                errorMessage == null ? MessageBoxIcon.Information : MessageBoxIcon.Error,
                MessageBoxDefaultButton.Button1, 0);
        }
    }
}
