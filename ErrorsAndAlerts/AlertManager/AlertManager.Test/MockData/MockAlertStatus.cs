using AlertManager.Interfaces;
using AlertManager.Models.AllDataClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Extract.ErrorsAndAlerts.AlertManager.Test.MockData
{
    public class MockAlertStatus : IAlertStatus
    {

        public List<AlertsObject> mockAlertList = new();
        public List<EventObject> mockEventList = new();

        public void AddToAlertList(AlertsObject alertObject)
        {
            mockAlertList.Add(alertObject);
        }

        public void AddToEventList(EventObject eventObject)
        {
            mockEventList.Add(eventObject);
        }

        public void AddToAlertList(List<AlertsObject> alertObjects)
        {
            mockAlertList.AddRange(alertObjects);
        }

        public void AddToEventList(List<EventObject> eventObjects)
        {
            mockEventList.AddRange(eventObjects);
        }

        public IList<AlertsObject> GetAllAlerts()
        {
            return mockAlertList;
        }

        public IList<AlertsObject> GetAllAlerts(int page)
        {
            return mockAlertList;
        }

        public IList<EventObject> GetAllEvents()
        {
            return mockEventList;
        }

        public IList<EventObject> GetAllEvents(int page)
        {
            return mockEventList;
        }

    }
}

