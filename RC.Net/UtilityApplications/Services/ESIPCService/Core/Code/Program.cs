using Extract.Licensing;
using System;
using System.ServiceProcess;

namespace Extract.UtilityApplications.Services
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            try
            {
                // Load the licenses 
                LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());

                // Create the IPC service and launch it
                using (ESIPCService service = new ESIPCService())
                {
                    // Run the service
                    ServiceBase.Run(service);
                }
            }
            catch (Exception ex)
            {
                // Log any exception thrown when running the service
                ExtractException ee = ExtractException.AsExtractException("ELI33125", ex);
                ee.Log();
                throw ee;
            }
        }
    }
}
