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

                bool resetForm = false;

                string fileName = null;
                if (args.Length > 1)
                {
                    ShowUsage("Too many arguments specified.");
                    return;
                }
                else if (args.Length == 1)
                {
                    var arg1 = args[0];
                    if (arg1.Equals("/?", StringComparison.Ordinal))
                    {
                        ShowUsage();
                        return;
                    }
                    else if (arg1.Equals("/reset", StringComparison.OrdinalIgnoreCase))
                    {
                        resetForm = true;
                    }
                    else
                    {
                        fileName = Path.GetFullPath(arg1);
                    }
                }

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

                var form = new FAMNetworkDashboardForm(fileName, resetForm);
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
            sb.AppendLine("FAMNetworkManager.exe [/?]|[<FileToOpen>]|[/reset]");
            sb.AppendLine("-----------------------------------------");
            sb.AppendLine("/? - Display usage message");
            sb.AppendLine("<FileToOpen> - The name of the '.fnm' file to load on opening");
            sb.AppendLine("/reset - resets the application form to installed defaults");

            MessageBox.Show(sb.ToString(), "Usage", MessageBoxButtons.OK,
                errorMessage == null ? MessageBoxIcon.Information : MessageBoxIcon.Error,
                MessageBoxDefaultButton.Button1, 0);
        }
    }
}
