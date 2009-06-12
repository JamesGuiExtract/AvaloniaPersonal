using Extract.Licensing;
using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace Extract.DataEntry.Utilities.DataEntryApplication
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            DataEntryApplicationForm mainForm = null;

            try
            {
                // Load licenses and validate
                LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());
                LicenseUtilities.ValidateLicense(LicenseIdName.FlexIndexCoreObjects, "ELI23678",
                    Application.ProductName);
                
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                string configFileName;

                // If no command-line arguments were specified, used the standard exe config file.
                if (args.Length == 0)
                {
                    configFileName = Assembly.GetExecutingAssembly().Location + ".config";
                }
                // Otherwise, use the specified config file.
                else
                {
                    configFileName = DataEntryMethods.ResolvePath(args[0]);
                }

                ExtractException.Assert("ELI25426", "A valid exe config file must exist alongside " +
                    "the application or be specified as the one and only command-line parameter",
                    File.Exists(configFileName));

                mainForm = new DataEntryApplicationForm(configFileName);

                // TODO: Splash screen?

                Application.Run(mainForm);
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI23674", ex);
            }
            finally
            {
                // Dispose of the main form
                if (mainForm != null)
                {
                    mainForm.Dispose();
                }
            }
        }
    }
}