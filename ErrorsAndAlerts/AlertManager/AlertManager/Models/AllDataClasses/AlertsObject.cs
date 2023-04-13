using AlertManager.Models.AllEnums;
using Extract.ErrorHandling;
using Extract.ErrorsAndAlerts.ElasticDTOs;
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
            AlertType = "";
            AlertName = "";
            this.Configuration = "";
            this.ActivationTime = new();
            this.Resolutions = new();
        }

        //constructor that initializes all fields with the parameters
        public AlertsObject(string alertId, string alertType, string alertName, string configuration, 
            DateTime activationTime, List<ExceptionEvent> associatedEvents, 
            List<AlertActionDto> listOfResolutions
            )
        {
            this.AlertId = alertId;
            this.AlertName = alertName;
            AlertType = alertType;
            this.Configuration = configuration;
            this.ActivationTime = activationTime;
            this.Resolutions = listOfResolutions;
            this.AssociatedEvents = associatedEvents;
        }

        //fields that contains the data
        public string AlertId { get; set; } = "";
        public string AlertName { get; set; } = "";
        public string AlertType { get; set; } = "";
        public string Configuration { get; set; } = "";
        public DateTime ActivationTime { get; set; } = new();
        public List<ExceptionEvent>? AssociatedEvents { get; set; }
        public List<AlertActionDto> Resolutions { get; set; } = new();


        public ICommand? CreateAlertWindow { get; set; } 
        public ICommand? ResolveAlert { get; set; }
    }
}
