using AlertManager.Interfaces;
using AlertManager.Models.AllDataClasses;
using AlertManager.Models.AllEnums;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Extract.ErrorsAndAlerts.AlertManager.Test.MockData
{
    public class MockDBService : IDBService
    {
        //testing only 

        public List<AlertsObject> AlertObjectList = new();
        public List<EventObject> EventObjectsList = new();

        public DataNeededForPage ReturnDataNeeded = new();

        public MockDBService()
        {
            SetDataBaseValue( new DataNeededForPage(-1, -1, new DateTime(2012, 12, 25, 10, 30, 50), "errorDataTestingOnly", ResolutionStatus.resolved, ErrorSeverityEnum.showStopper, new UserMetrics()) );
        }

        /// <summary>
        /// This method returns a list of all FAMActions from the datasource
        /// Used in: MainWindowViewModel
        /// </summary>
        /// <returns> List<FAMAction> </returns>
        public List<FAMAction> GetActionList()
        {
            List<FAMAction> actionList = new List<FAMAction>();
            actionList.Add(new FAMAction { Id = 0, Name = "Compute" });
            actionList.Add(new FAMAction { Id = 1, Name = "Verify" });
            return actionList;
        }

        /// <summary>
        /// Reads all alerts from a flat file
        /// </summary>
        /// <returns> List<LogAlert> </returns>
        public List<LogAlert> ReadAllAlerts()
        {
            List<LogAlert> returnList = new List<LogAlert>();
            //todo add alerts
            LogAlert alert = null;

            return returnList;
        }

        /// <summary>
        /// Reads all errors from a flat file
        /// </summary>
        /// <returns></returns>
        public List<LogError> ReadAllErrors()
        {
            List<LogError> returnList = new List<LogError>();
            //todo return return list
            return returnList;
        }

        /// <summary>
        /// Mock response for File list view
        /// Used in: MainWindowViewModel
        /// </summary>
        /// <returns>Integer representing the number of files</returns>
        public int GetDocumentTotal()
        {
            return 25;
        }


        /// <summary>
        /// Returns a hard coded value DataNeededForPage
        /// Used in: EventsOverallViewModel
        /// </summary>
        /// <param name="searchValue">Mock search value</param>
        /// <returns> DataNeededForPage object </returns>
        public DataNeededForPage ReturnFromDatabase(int searchValue)
        {
            return ReturnDataNeeded;
        }

        public void SetDataBaseValue(DataNeededForPage dataToSet)
        {
            ReturnDataNeeded = dataToSet;
        }

        /// <summary>
        /// Returns a list of hard coded values representing Id numbers of alerts
        /// Used in: EventsOverallViewModel
        /// </summary>
        /// <returns> List<int> </returns>
        //todo add dependency injection here so put the parser or database. 
        public List<int> AllIssueIds()
        {
            List<int> allIssueIds = new List<int>();
            allIssueIds.Add(1);
            allIssueIds.Add(2);
            allIssueIds.Add(3);
            
            return allIssueIds;
        }

        /// <summary>
        /// 
        /// TODO: eventually as this switches to a alert system, this will be changed
        /// </summary>
        /// <param name="objectToAdd"></param>
        /// <returns></returns>
        public bool AddAlertToDatabase(LogAlert objectToAdd)
        {
            return true;
        }

        public List<AlertsObject> ReadAlertObjects()
        {
            return AlertObjectList;
        }

        public List<EventObject> ReadEvents()
        {
            return EventObjectsList;
        }

        public void AddAlertObjects(List<AlertsObject> alertsToAdd)
        {
            AlertObjectList.AddRange(alertsToAdd);
        }

        public void AddAlertObjects(AlertsObject alertsToAdd)
        {
            AlertObjectList.Add(alertsToAdd);
        }

        public void AddEventObjects(List<EventObject> eventsToAdd)
        {
            EventObjectsList.AddRange(eventsToAdd);
        }

        public void AddEventObjects(EventObject eventsToAdd)
        {
            EventObjectsList.Add(eventsToAdd);
        }

        public void AddSettingObject()
        {

        }


    }
}
