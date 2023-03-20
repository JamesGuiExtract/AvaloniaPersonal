using AlertManager.Models.AllDataClasses;
using System.Collections.Generic;
using UCLID_FILEPROCESSINGLib;

namespace AlertManager.Interfaces
{
    //Interface for mock db service/Logging target
    public interface IDBService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool SetFileStatus(int fileNumber, EActionStatus fileStatus, string databaseName,
            string databaseServer, int actionId)
        {
            return false;
        }

        public List<FileObject> GetFileObjects(List<int> listOfFileIds, string databaseName,
            string databaseServer, int actionId)
        {
            return new();
        }
    }

}

