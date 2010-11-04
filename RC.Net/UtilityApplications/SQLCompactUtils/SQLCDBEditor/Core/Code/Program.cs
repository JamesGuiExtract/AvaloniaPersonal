using Extract.Licensing;
using System;
using System.Text;
using System.Windows.Forms;

namespace Extract.SQLCDBEditor
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
                    "ELI29534", "SQLCDBEditor Application");
                
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                if (args.Length == 0)
                {
                    Application.Run(new SQLCDBEditorForm());
                }
                else
                {
                    string databaseFileName = args[0];
                    if (args[0] == "/?")
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.AppendLine("SQLCDBEditor <DatabaseFileName> | /?");
                        sb.AppendLine("\t<DatabaseFileName> - sdf file to open.");
                        sb.AppendLine("\t/? - Displays this message.");
                        sb.AppendLine();
                        sb.AppendLine("\tIf no arguments are specified SQLCDBEditor will be opened without a file.");
                        MessageBox.Show(sb.ToString() , "Usage", MessageBoxButtons.OK,
                            MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, 0);
                    }
                    else
                    {
                        Application.Run(new SQLCDBEditorForm(databaseFileName));
                    }
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI29515", ex);
            }
        }
    }
}