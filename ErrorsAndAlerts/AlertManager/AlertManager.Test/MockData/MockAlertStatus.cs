using AlertManager.Interfaces;
using AlertManager.Models.AllDataClasses;
using Extract.ErrorHandling;
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
        public List<ExceptionEvent> mockEventList = new();

        public void AddToAlertList(AlertsObject alertObject)
        {
            mockAlertList.Add(alertObject);
        }

        public void AddToEventList(ExceptionEvent eventObject)
        {
            mockEventList.Add(eventObject);
        }

        public void AddToAlertList(List<AlertsObject> alertObjects)
        {
            mockAlertList.AddRange(alertObjects);
        }

        public void AddToEventList(List<ExceptionEvent> eventObjects)
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

        public IList<ExceptionEvent> GetAllEvents()
        {
            return mockEventList;
        }

        public IList<ExceptionEvent> GetAllEvents(int page)
        {
            return mockEventList;
        }

    }
}

