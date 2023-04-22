using AlertManager.Models.AllDataClasses;
using System.Collections.Generic;
using UCLID_FILEPROCESSINGLib;

namespace AlertManager.Interfaces
{
    public interface IDBService
    {
        bool SetFileStatus(int fileNumber, EActionStatus fileStatus, string databaseName,
            string databaseServer, int actionId);

        List<FileObject> GetFileObjects(IList<int> listOfFileIds, string databaseName,
            string databaseServer, int actionId);
    }

}

