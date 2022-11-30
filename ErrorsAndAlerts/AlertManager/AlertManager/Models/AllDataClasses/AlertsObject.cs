using AlertManager.Models.AllEnums;
using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace AlertManager.Models.AllDataClasses
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
            ActionType = "";
            AlertType = "";
            AlertName = "";
            this.Configuration = "";
            this.ActivationTime = new();
            this.UserFound = "";
            this.MachineFoundError = "";
            this.Resolution = new AlertResolution()
            {
                ResolutionComment = ""
            };
            this.AlertHistory = "";
        }

        //constructor that initializes all fields with the parameters
        public AlertsObject(int issueId, string alertId, string actionType, string alertType, string alertName, string configuration, 
            DateTime activationTime, string userFound, string machineFoundError,
            string resolutionComment, TypeOfResolutionAlerts resolutionType, DateTime? resolutionTime = null, string? alertHistory = null,
            List<EventObject> associatedEvents = null)
        {
            this.IssueId = issueId;
            this.AlertId = alertId;
            this.AlertName = alertName;
            ActionType = actionType;
            AlertType = alertType;
            this.Configuration = configuration;
            this.ActivationTime = activationTime;
            this.UserFound = userFound;
            this.MachineFoundError = machineFoundError;
            this.Resolution = new AlertResolution()
            {
                ResolutionComment = resolutionComment,
                ResolutionType = resolutionType,
                ResolutionTime = resolutionTime,
            };
            this.AlertHistory = alertHistory;
            this.AssociatedEvents = associatedEvents;
        }

        //fields that contains the data
        public int IssueId { get; set; }
        public string AlertId { get; set; }
        public string AlertName { get; set; } = "";
        public string ActionType { get; set; } = "";
        public string AlertType { get; set; } = "";
        public string Configuration { get; set; } = "";
        public DateTime ActivationTime { get; set; } = new();
        public string UserFound { get; set; } = "";
        public string MachineFoundError { get; set; } = "";
        public string? AlertHistory { get; set; }
        public List<EventObject> AssociatedEvents { get; set; }
        public AlertResolution Resolution { get; set; }

        public ICommand CreateAlertWindow { get; set; }
    }
}
