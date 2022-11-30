using AlertManager.Interfaces;
using AlertManager.Models;
using AlertManager.Models.AllDataClasses;
using AlertManager.Models.AllEnums;
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

            try
            {
                string? tempString = alertFileLocation;
                if(tempString == null)
                {
                    throw new Exception("Issue with Configuration path, invalid return value");
                }

                List<string> jsonLines = File.ReadAllLines(tempString).ToList();
                List<LogError> errors = ReadAllErrors();
                foreach (string line in jsonLines)
                {
                    if (line == null)
                    {
                        throw new Exception("Invalid log file");
                    }
                    string[] values = line.Split("||");
                    int id = int.Parse(values[0]);
                    List<LogError> associatedErrors;
                    if (values[1] == "")
                    {
                        associatedErrors = new List<LogError>();
                    }
                    else
                    {
                        int[] errorIds = Array.ConvertAll(values[1].Split(','), s => int.Parse(s));
                        associatedErrors = errors.Where(e => errorIds.Contains(e.Id)).ToList();
                    }
                    string type = values[2];
                    string title = values[3];
                    DateTime occurred = DateTime.Parse(values[4]);
                    string status = values[5];
                    string? fix = values[6];
                    returnList.Add(new LogAlert { Id = id, Created = occurred, Type = type, Title = title, Status = status, Resolution = fix, Errors = associatedErrors });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
            return returnList;
        }

        /// <summary>
        /// Reads all errors from a flat file
        /// </summary>
        /// <returns></returns>
        public List<LogError> ReadAllErrors()
        {
            List<LogError> returnList = new List<LogError>();
            try
            {
                string? fileLocation = errorFileLocation;

                if(fileLocation == null)
                {
                    throw new Exception("File Location not found");
                }

                List<string> jsonLines = File.ReadAllLines(fileLocation).ToList();
                foreach (string line in jsonLines)
                {
                    if (line == null)
                    {
                        throw new Exception("Invalid log file");
                    }
                    string[] values = line.Split("||");
                    int id = int.Parse(values[0]);
                    DateTime occurred = DateTime.Parse(values[1]);
                    string type = values[2];
                    string objectType = values[3];
                    string details = values[4];
                    ErrorDetails? errorDetails = JsonSerializer.Deserialize<ErrorDetails>(details);

                    if(errorDetails == null)
                    {
                        errorDetails = new();
                    }

                    returnList.Add(new LogError { Id = id, DateOccurred = occurred, Type = type, ObjectType = objectType, ErrorDetails = errorDetails });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
            }
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
        /// <param name="searchValue">M ock serch value</param>
        /// <returns> DataNeededForPage object </returns>
        public DataNeededForPage ReturnFromDatabase(int searchValue)
        {
            return new DataNeededForPage(-1, -1, new DateTime(2012, 12, 25, 10, 30, 50), "errorDataTestingOnly", ResolutionStatus.resolved, ErrorSeverityEnum.showStopper, new UserMetrics());
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
            try
            {
                string? tempString = alertFileLocation;
                if(tempString == null)
                {
                    return false;
                }

                List<LogAlert> alertList = ReadAllAlerts();
                alertList.Add(objectToAdd);
                string jsonString = JsonSerializer.Serialize(alertList);

                File.WriteAllText( tempString , jsonString);

            }
            catch(Exception e)
            {
                Console.WriteLine(e.StackTrace);
                return false;
            }
            return false;
        }

        public List<AlertsObject> ReadAlertObjects()
        {
            List<AlertsObject> returnList = new();

            AlertsObject testObject = new AlertsObject(0, "0", "TestAction", "TestType", "test alert 1", "testconfig", new DateTime(2008, 5, 1, 8, 30, 52), "testUser", "testMachine", "testResolution", TypeOfResolutionAlerts.Snoozed, new DateTime(2008, 5, 1, 8, 30, 52), "testingAlertHistory");
            AlertsObject testObject2 = new AlertsObject(1, "1", "TestAction2", "TestType2", "test alert 2", "testconfig2", new DateTime(2008, 5, 1, 8, 30, 52), "testUser2", "testMachine", "testResolution", TypeOfResolutionAlerts.Snoozed, new DateTime(2008, 5, 1, 8, 30, 52), "testingAlertHistory");
            returnList.Add(testObject);
            returnList.Add(testObject2);

            return returnList;
        }

        public List<EventObject> ReadEvents()
        {
            List<EventObject> returnList = new();
            EventObject errorObject = new EventObject("testEliCode", "testMessage", 12, true, new DateTime(2008, 5, 1, 8, 30, 52), ErrorSeverityEnum.medium, "no details", new MachineAndCustomerInformation(), "some stuff sfsaafds");
            returnList.Add(errorObject);
            return returnList;
        }
    }

}
