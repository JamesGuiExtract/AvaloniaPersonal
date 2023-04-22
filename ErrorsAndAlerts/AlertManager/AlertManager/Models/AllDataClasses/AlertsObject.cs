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
        }

        public AlertsObject(
            string alertId,
            string hitsType,
            string alertName,
            string configuration,
            DateTime activationTime,
            IList<AlertActionDto> listOfActions)
        {
            AlertId = alertId;
            AlertName = alertName;
            HitsType = hitsType;
            Configuration = configuration;
            ActivationTime = activationTime;
            Actions = listOfActions;
        }

        public AlertsObject(
            string alertId,
            string hitsType,
            string alertName,
            string configuration,
            DateTime activationTime,
            IList<EventDto> associatedEvents,
            IList<AlertActionDto> listOfActions)
            : this(alertId, hitsType, alertName, configuration, activationTime, listOfActions)
        {
            AssociatedEvents = associatedEvents;
        }

        public AlertsObject(
            string alertId,
            string hitsType,
            string alertName,
            string configuration,
            DateTime activationTime,
            IList<EnvironmentDto> associatedEnvironments,
            IList<AlertActionDto> listOfActions)
            : this(alertId, hitsType, alertName, configuration, activationTime, listOfActions)
        {
            AssociatedEnvironments = associatedEnvironments;
        }

        public string AlertId { get; set; } = "";
        public string AlertName { get; set; } = "";
        public string HitsType { get; set; } = "";
        public string Configuration { get; set; } = "";
        public DateTime ActivationTime { get; set; } = new();
        public IList<EventDto> AssociatedEvents { get; set; } = Array.Empty<EventDto>();
        public IList<EnvironmentDto> AssociatedEnvironments { get; set; } = Array.Empty<EnvironmentDto>();
        public IList<AlertActionDto> Actions { get; set; } = Array.Empty<AlertActionDto>();
    }
}
