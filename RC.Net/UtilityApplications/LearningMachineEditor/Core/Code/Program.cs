using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Extract.Licensing;
using System.IO;

namespace Extract.UtilityApplications.LearningMachineEditor
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            string fileName = null;
            if (args.Length > 0)
            {
                fileName = Path.GetFullPath(args[0]);
            }
            Application.Run(new LearningMachineConfiguration(fileName));
        }
    }
}
