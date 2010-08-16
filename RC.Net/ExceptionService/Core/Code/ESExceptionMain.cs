﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;

namespace Extract.ExceptionService
{
    static class ESExceptionMain
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[] 
			{ 
				new ESExceptionService() 
			};
            ServiceBase.Run(ServicesToRun);
        }
    }
}
