using Extract.ErrorHandling;
using System;
using System.ServiceProcess;

namespace ExtractEnvironmentService
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            try
            {
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[]
                {
                    new EnvironmentLogger()
                };
                ServiceBase.Run(ServicesToRun);
            }
            catch (Exception ex)
            {
                ex.AsExtract("ELI54076").Log();
            }
        }
    }
}
