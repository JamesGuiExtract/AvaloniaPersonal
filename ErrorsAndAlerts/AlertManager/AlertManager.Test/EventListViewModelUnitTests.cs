using AlertManager;
using AlertManager.Interfaces;
using AlertManager.ViewModels;
using static System.Net.Mime.MediaTypeNames;
using Splat;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Models.TreeDataGrid;
using AlertManager.Services;
using Moq;
using Extract.ErrorsAndAlerts.AlertManager.Test.MockData;
using AlertManager.Models.AllDataClasses;
using System.Collections.ObjectModel;
using DynamicData;
using AlertManager.Models.AllEnums;
using NUnit.Framework.Internal;
using NUnit.Framework.Interfaces;
using Extract.ErrorHandling;

namespace Extract.ErrorsAndAlerts.AlertManager.Test
{
    public class EventListViewModelUnitTests
    {
        [Test]
        [TestCaseSource(nameof(EventsSource))]
        public void TestEventTable(ExceptionEvent eventObject)
        {
            Mock<IElasticSearchLayer> mockAlertStatus = new Mock<IElasticSearchLayer>();

            List<ExceptionEvent> events = new();
            events.Add(eventObject);

            //add moq here and database stuff
            Mock<EventListViewModel> testWindow = new Mock<EventListViewModel>(events, mockAlertStatus.Object);

            mockAlertStatus.Setup(m => m.GetAllAlerts(0)).Returns(new List<AlertsObject>());
            mockAlertStatus.Setup(m => m.GetAllEvents(0)).Returns(events);
            mockAlertStatus.Setup(m => m.GetMaxEventPages()).Returns(1);

            Assert.Multiple(() =>
            {
                for (int i = 0; i < testWindow.Object._ErrorAlertsCollection.Count; i++)
                {
                    Assert.That(testWindow.Object._ErrorAlertsCollection[i].Level, Is.EqualTo(events[i].Level));
                    Assert.That(testWindow.Object._ErrorAlertsCollection[i].StackTrace, Is.EqualTo(events[i].StackTrace));
                }
            });
        }

        [Test]
        [TestCaseSource(nameof(EventsSource))]
        public void TestEventTableNull(ExceptionEvent eventObject)
        {
            Mock<IElasticSearchLayer> mockAlertStatus = new Mock<IElasticSearchLayer>();

            List<ExceptionEvent> events = new();
            events.Add(eventObject);

            //use this instead of not throws b/c its more specific
            //note interesting interaction with bindings and nulls, so maybe check if its not equal to?
            Assert.Multiple( () => {
                Assert.DoesNotThrow(() => { Mock<EventListViewModel> testWindow = new Mock<EventListViewModel>(null, mockAlertStatus.Object); });
                Assert.DoesNotThrow(() => { Mock<EventListViewModel> testWindow = new Mock<EventListViewModel>(events, null); });
                Assert.DoesNotThrow(() => { Mock<EventListViewModel> testWindow = new Mock<EventListViewModel>(null); });
            });
            
        }

        [Test]
        [TestCaseSource(nameof(EventListSource))]
        public void TestErrorTableMultipleValue(List<ExceptionEvent> listOfEvents)
        {
            Mock<IElasticSearchLayer> mockAlertStatus = new Mock<IElasticSearchLayer>();

            Mock<EventListViewModel> testWindow = new Mock<EventListViewModel>(listOfEvents, mockAlertStatus.Object);

            mockAlertStatus.Setup(m => m.GetAllAlerts(0)).Returns(new List<AlertsObject>());
            mockAlertStatus.Setup(m => m.GetAllEvents(0)).Returns(new List<ExceptionEvent>());
            mockAlertStatus.Setup(m => m.GetMaxEventPages()).Returns(1);


            Assert.Multiple(() =>
            {
                for (int i = 0; i < testWindow.Object._ErrorAlertsCollection.Count; i++)
                {
                    Assert.That(testWindow.Object._ErrorAlertsCollection[i].Level, Is.EqualTo(listOfEvents[i].Level));
                    Assert.That(testWindow.Object._ErrorAlertsCollection[i].StackTrace, Is.EqualTo(listOfEvents[i].StackTrace));
                }
            });
        }


        public static IEnumerable<ExceptionEvent> EventsSource()
        {
            yield return new();
        }

        public static IEnumerable<List<ExceptionEvent>> EventListSource()
        {
            yield return new();
        }

    }
}
