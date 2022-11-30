using AlertManager.Models.AllDataClasses;
using System;
using System.Collections.Generic;

namespace AlertManager.Interfaces
{
    //Interface for mock db service/Logging target
    public interface IAlertStatus
    {
        /// <summary>
        /// Gets a list of all logged alerts from a given source
        /// </summary>
        /// <param name="page">0 indexed page number to display</param>
        /// <returns>Collection of all Alerts from the logging source</returns>
        IList<AlertsObject> GetAllAlerts(int page);

        /// <summary>
        /// Gets a list of all available exceptions from a given source
        /// </summary>
        /// <param name="page">0 indexed page number to display</param>
        /// <returns>Collection of all Exceptions from the logging source</returns>
        IList<EventObject> GetAllEvents(int page);
    }
}
