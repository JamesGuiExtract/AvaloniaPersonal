using AlertManager.Models.AllDataClasses;
using Extract.ErrorHandling;
using System.Collections.Generic;

namespace AlertManager.Interfaces
{
    //Interface for mock db service/Logging target
    public interface IDBService
    {


        /// <summary>
        /// Returns the specific data package from database
        /// </summary>
        /// <param name="searchValue">integer unique id of item</param>
        /// <returns> a dataneededforpage item that contains data for a item</returns>
        DataNeededForPage ReturnFromDatabase(int searchValue);

        /// <summary>
        /// Returns a list of alert objects, may need to remove this as not all data is encapsulated
        /// </summary>
        /// <returns></returns>
        List<AlertsObject> ReadAlertObjects();

        /// <summary>
        /// returns a list of events from backedn, may also need to remove this...
        /// </summary>
        /// <returns></returns>
        List<ExceptionEvent> ReadEvents();
    }
}
