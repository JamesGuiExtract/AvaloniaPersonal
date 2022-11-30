using AlertManager.Models.AllDataClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AlertManager.Interfaces
{
    //Interface for logging alert resolutions
    public interface IAlertResolutionLogger
    {

        /// <summary>
        /// Logs a resolution for a given alert
        /// </summary>
        /// <param name="alert">The alerts object for which to post a resolution</param>
        void LogResolution(AlertsObject alert);
    }
}
