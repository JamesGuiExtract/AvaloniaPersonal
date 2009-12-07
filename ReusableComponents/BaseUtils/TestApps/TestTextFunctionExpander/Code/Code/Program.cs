using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace TestTextFuntionExpander
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Load the license files
            Extract.Licensing.LicenseUtilities.LoadLicenseFilesFromFolder(0, new Extract.Licensing.MapLabel());
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new TestForm());
        }
    }
}