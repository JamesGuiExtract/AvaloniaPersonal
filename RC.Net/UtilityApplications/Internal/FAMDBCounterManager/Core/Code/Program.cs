using Extract.Licensing.Internal;
using System;
using System.Text;
using System.Windows.Forms;

namespace Extract.FAMDBCounterManager
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// <param name="args">Command line arguments for application.</param>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (!NativeMethods.IsInternalToolsLicensed)
            {
                UtilityMethods.ShowMessageBox("Unable to run FAM DB counter manager", "Error", true);

                return;
            }

            if (args.Length == 1 &&
                args[0].Equals("/ConfigureEmail", StringComparison.OrdinalIgnoreCase))
            {
                var settings = new EmailSettingsManager();
                settings.RunConfiguration();
            }
            else if (args.Length > 0)
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Usage:");
                sb.AppendLine("------------");
                sb.AppendLine("FAMDBCounterManager.exe [/ConfigureEmail]");
                sb.AppendLine("Without any arguments, allows for FAM DB counter update codes");
                sb.AppendLine("to be created. With /ConfigureEmail, a dialog is opened that");
                sb.AppendLine("allows configuration of the email settings used to send the");
                sb.AppendLine("generated codes. Write permission in the directory this");
                sb.AppendLine("utility is run from is required to update email settings.");
                sb.AppendLine();
                sb.AppendLine("NOTE:");
                sb.AppendLine("This utility is for Extract Systems internal use only. It can");
                sb.AppendLine("be run in place from any location regardless of any other");
                sb.AppendLine("Extract software installed as long as the following dlls are");
                sb.AppendLine("alongside:");
                sb.AppendLine("BaseUtils.dll, Extract.Interfaces.dll, InternalLicenseUtils.dll,");
                sb.AppendLine("mfc100.dll, mfcm100.dll, msvcp100.dll and msvcr100.dll.");

                UtilityMethods.ShowMessageBox(sb.ToString(), "FAMDBCounterManager Usage", false);
            }
            else
            {
                Application.Run(new FAMDBCounterManagerForm());
            }
        }
    }
}
