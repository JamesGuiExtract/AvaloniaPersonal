using Extract;
using Extract.Licensing;
using System;
using System.Collections.Generic;
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
						MessageBox.Show("SQLCDBEditor <DatabaseFileName> | /?\n\r"
							+ "\t<DatabaseFileName> - sdf file to open.\r\n"
							+ "\t</? - Displays this message.\r\n\r\n"
							+ "\tIf no arguments are specified SQLCDBEditor will be opened without a file."
							, "Usage");
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