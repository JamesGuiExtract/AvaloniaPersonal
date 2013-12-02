using System.ServiceProcess;

namespace Extract.UtilityApplications.Services
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the service.
        /// </summary>
        static void Main()
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[] 
			{ 
				new AppMonitorService() 
			};
            ServiceBase.Run(ServicesToRun);
        }
    }
}
