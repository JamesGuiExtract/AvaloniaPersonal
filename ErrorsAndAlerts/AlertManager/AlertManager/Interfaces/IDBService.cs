using AlertManager.Models.AllDataClasses;
using System.Collections.Generic;

namespace AlertManager.Interfaces
{
    //Interface for mock db service/Logging target
    public interface IDBService
    {
        /// <summary>
        /// Gets the Action List 
        /// </summary>
        /// <returns> List<FAMAction></returns>
        List<FAMAction> GetActionList();

        /// <summary>
        /// Gets Total number of Documents in program
        /// </summary>
        /// <returns></returns>
        int GetDocumentTotal();

        /// <summary>
        /// Returns the specific data package from database
        /// </summary>
        /// <param name="searchValue">integer unique id of item</param>
        /// <returns> a dataneededforpage item that contains data for a item</returns>
        DataNeededForPage ReturnFromDatabase(int searchValue);

        /// <summary>
        /// Returns a list of integer values that represents all the Id issues
        /// </summary>
        /// <returns>List of integer values</returns>
        List<int> AllIssueIds();

        /// <summary>
        /// Returns of logerror values that represents all the Issues and their data
        /// </summary>
        /// <returns>list of log errors</returns>
        List<LogError> ReadAllErrors();

        /// <summary>
        /// Returns a list of Log Alerts that represents all alerts
        /// </summary>
        /// <returns>list of log alerts</returns>
        List<LogAlert> ReadAllAlerts();

        /// <summary>
        /// Adds a alert to the original database
        /// </summary>
        /// <returns>true if successfully added, false if not successfully added</returns>
        bool AddAlertToDatabase(LogAlert objectToAdd);

        /// <summary>
        /// Returns a list of alert objects, may need to remove this as not all data is encapsulated
        /// </summary>
        /// <returns></returns>
        List<AlertsObject> ReadAlertObjects();

        /// <summary>
        /// returns a list of events from backedn, may also need to remove this...
        /// </summary>
        /// <returns></returns>
        List<EventObject> ReadEvents();
    }
}
