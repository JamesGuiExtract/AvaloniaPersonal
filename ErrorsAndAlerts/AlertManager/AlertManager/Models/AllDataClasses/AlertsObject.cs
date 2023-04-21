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
            HitsType = "";
            AlertName = "";
            this.Configuration = "";
            this.ActivationTime = new();
            this.Actions = new();
        }

        //constructor that initializes all fields with the parameters
        public AlertsObject(string alertId, string HitsType, string alertName, string configuration, 
            DateTime activationTime, List<EventDto> associatedEvents, 
            List<AlertActionDto> listOfActions
            )
        {
            this.AlertId = alertId;
            this.AlertName = alertName;
            this.HitsType = HitsType;
            this.Configuration = configuration;
            this.ActivationTime = activationTime;
            this.Actions = listOfActions;
            this.AssociatedEvents = associatedEvents;
        }

        public AlertsObject(string alertId, string HitsType, string alertName, string configuration,
            DateTime activationTime, List<EnvironmentDto> associatedEnvironments,
            List<AlertActionDto> listOfActions
            )
        {
            this.AlertId = alertId;
            this.AlertName = alertName;
            this.HitsType = HitsType;
            this.Configuration = configuration;
            this.ActivationTime = activationTime;
            this.Actions = listOfActions;
            this.AssociatedEnvironments = associatedEnvironments;
        }

        //fields that contains the data
        public string AlertId { get; set; } = "";
        public string AlertName { get; set; } = "";
        public string HitsType { get; set; } = "";
        public string Configuration { get; set; } = "";
        public DateTime ActivationTime { get; set; } = new();
        public List<EventDto>? AssociatedEvents { get; set; }
        public List<EnvironmentDto> AssociatedEnvironments { get; set; }
        public List<AlertActionDto> Actions { get; set; } = new();
    }
}
