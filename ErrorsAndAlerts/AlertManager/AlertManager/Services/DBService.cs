using AlertManager.Interfaces;
using AlertManager.Models;
using AlertManager.Models.AllDataClasses;
using AlertManager.Models.AllEnums;
using Extract.ErrorHandling;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace AlertManager.Services
{
    public class DBService : IDBService
    {
        //testing only 
        string? errorFileLocation = ConfigurationManager.AppSettings["ErrorFilePath"];
        string? alertFileLocation = ConfigurationManager.AppSettings["AlertFilePath"];

        public string? ErrorFileLocation { get { return errorFileLocation; } set { errorFileLocation = value; } }

        public string? AlertFileLocation { get { return AlertFileLocation; } set { alertFileLocation = value; } }    
        public DBService()
        {
        }



        /// <summary>
        /// Returns a hard coded value DataNeededForPage
        /// Only used in testing, makes it easier than having to rely on elastic search portion, elastic search portion tested seperatly in tests
        /// </summary>
        /// <param name="searchValue">Mock search value</param>
        /// <returns> DataNeededForPage object </returns>
        public DataNeededForPage ReturnFromDatabase(int searchValue)
        {
            return new DataNeededForPage(-1, -1, new DateTime(2012, 12, 25, 10, 30, 50), "errorDataTestingOnly", ResolutionStatus.resolved, ErrorSeverityEnum.showStopper);
        }

        /// <summary>
        /// Only used in testing, makes it easier than having to rely on elastic search portion, elastic search portion tested seperatly in tests
        /// </summary>
        /// <returns></returns>
        public List<AlertsObject> ReadAlertObjects()
        {
            List<AlertsObject> returnList = new();

            AlertsObject testObject = new AlertsObject(0, "0", "TestAction", "TestType", "test alert 1", "testconfig", new DateTime(2008, 5, 1, 8, 30, 52), "testUser", "testMachine", "testResolution", TypeOfResolutionAlerts.Snoozed, new DateTime(2008, 5, 1, 8, 30, 52), "testingAlertHistory");
            AlertsObject testObject2 = new AlertsObject(1, "1", "TestAction2", "TestType2", "test alert 2", "testconfig2", new DateTime(2008, 5, 1, 8, 30, 52), "testUser2", "testMachine", "testResolution", TypeOfResolutionAlerts.Snoozed, new DateTime(2008, 5, 1, 8, 30, 52), "testingAlertHistory");
            returnList.Add(testObject);
            returnList.Add(testObject2);

            return returnList;
        }

        /// <summary>
        /// Only used in testing, makes it easier than having to rely on elastic search portion, elastic search portion tested seperatly in tests
        /// </summary>
        /// <returns></returns>
        public List<EventObject> ReadEvents()
        {
            List<EventObject> returnList = new();
            EventObject errorObject = new EventObject("ELI53748", "testMessage", 12, true, new DateTime(2008, 5, 1, 8, 30, 52), ErrorSeverityEnum.medium, "no details", new MachineAndCustomerInformation(), "some stuff sfsaafds");
            returnList.Add(errorObject);
            return returnList;
        }
    }

}
