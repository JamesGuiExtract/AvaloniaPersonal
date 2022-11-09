using AvaloniaDashboard.Models.AllDataClasses;
using System;
using System.Collections.Generic;

namespace AvaloniaDashboard.Interfaces
{
    //Interface for mock db service/Logging target
    public interface IAlertStatus
    {
        /// <summary>
        /// Gets a list of all logged alerts from a given source
        /// </summary>
        /// <returns>Collection of all Alerts from the logging source</returns>
        IList<LogAlert> GetAllAlerts();

        /// <summary>
        /// Gets a list of all available exceptions from a given source
        /// </summary>
        /// <returns>Collection of all Exceptions from the logging source</returns>
        IList<LogError> GetAllExceptions();
    }
}
