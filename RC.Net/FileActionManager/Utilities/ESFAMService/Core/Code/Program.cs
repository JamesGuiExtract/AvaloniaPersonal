using Extract.Licensing;
using System;
using System.Collections.Generic;
using System.ServiceProcess;
using System.Text;

namespace Extract.FileActionManager.Utilities
{
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
                // Load the licenses 
                LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());

                // Create the FAM service and launch it
                using (ESFAMService service = new ESFAMService())
                {
                    // Run the service
                    ServiceBase.Run(service);
                }
            }
            catch (Exception ex)
            {
                // Log any exception thrown when running the service
                ExtractException ee = ExtractException.AsExtractException("ELI28490", ex);
                ee.Log();
                throw ee;
            }
        }
    }
}