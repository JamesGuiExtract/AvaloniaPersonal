using System;
using System.Windows.Forms;

namespace Extract.SharePoint.Redaction.Utilities
{
    /// <summary>
    /// Helper application to launch verification and then prompt the user to
    /// save teh redacted file.
    /// </summary>
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
                // Launch the notification tray icon
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new IdShieldForSPClientNotification());
            }
            catch (Exception ex)
            {
                ex.LogExceptionWithHelperApp("ELI31458");
                ex.DisplayInMessageBox();
            }
        }
    }
}
