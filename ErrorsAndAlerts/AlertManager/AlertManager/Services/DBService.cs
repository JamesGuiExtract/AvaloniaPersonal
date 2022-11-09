using AvaloniaDashboard.Interfaces;
using AvaloniaDashboard.Models;
using AvaloniaDashboard.Models.AllDataClasses;
using AvaloniaDashboard.Models.AllEnums;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace AvaloniaDashboard.Services
{
    //TODO seperate everything out
    public class DBService : IDBService
    {
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
                string? tempString = ConfigurationManager.AppSettings["AlertFilePath"];
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
                string? fileLocation = ConfigurationManager.AppSettings["ErrorFilePath"];

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
    }
}
