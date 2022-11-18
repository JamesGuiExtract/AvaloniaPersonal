using AvaloniaDashboard.Models.AllEnums;
using System;
using System.Windows.Input;

namespace AvaloniaDashboard.Models.AllDataClasses
{
    /// <summary>
    /// Public class that serves to contain all data for alerts
    /// </summary>
    [Serializable]
    public class AlertsObject
    {
        //generic constructor
        public AlertsObject()
        {
            action_Type = "";
            alert_Type = "";
            this.configuration = "";
            this.activation_Time = new();
            this.user_Found = "";
            this.machine_Found_Error = "";
            this.resolution_Type = "";
            this.type_Of_Resolution = new();
            this.alert_History = "";
        }

        //constructor that initializes all fields with the parameters
        public AlertsObject(int issueId, int alertId, string actionType, string alertType, string configuration, 
            DateTime activationTime, string userFound, string machineFoundError,
            string resolutionType, TypeOfResolutionAlerts typeOfResolution, DateTime? resolutionTime = null, string? alertHistory = null)
        {
            this.issue_Id = issueId;
            this.alert_Id = alertId;
            action_Type = actionType;
            alert_Type = alertType;
            this.configuration = configuration;
            this.activation_Time = activationTime;
            this.user_Found = userFound;
            this.machine_Found_Error = machineFoundError;
            this.resolution_Type = resolutionType;
            this.resolution_Time = resolutionTime;
            this.type_Of_Resolution = typeOfResolution;
            this.alert_History = alertHistory;
        }

        //fields that contains the data
        public int issue_Id { get; set; }
        public int alert_Id { get; set; }
        public string action_Type { get; set; } = "";
        public string alert_Type { get; set; } = "";
        public string configuration { get; set; } = "";
        public DateTime activation_Time { get; set; } = new();
        public string user_Found { get; set; } = "";
        public string machine_Found_Error { get; set; } = "";
        public string resolution_Type { get; set; } = "";
        public DateTime? resolution_Time { get; set; }
        public TypeOfResolutionAlerts type_Of_Resolution { get; set; } = new();
        public string? alert_History { get; set; }

        public ICommand create_Alert_Window { get; set; }
    }
}
