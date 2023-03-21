using AlertManager.Interfaces;
using AlertManager.Models.AllDataClasses;
using AlertManager.ViewModels;
using Extract.ErrorHandling;
using Moq;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;

namespace Extract.ErrorsAndAlerts.AlertManager.Test
{
    public class EventListViewModelUnitTests
    {
        [Test]
        [TestCaseSource(nameof(EventsSource))]
        public void TestConstructorElastic(ExceptionEvent eventObject)
        {
            Mock<IElasticSearchLayer> mockAlertStatus = new Mock<IElasticSearchLayer>();

            List<ExceptionEvent> events = new();
            events.Add(eventObject);

            //add moq here and database stuff
            Mock<EventListViewModel> testWindow = new Mock<EventListViewModel>(mockAlertStatus.Object, "Title");

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
        [TestCaseSource(nameof(EventListSource))]

        public void TestConstructureFromList(List<ExceptionEvent> listOfEvents)
        {
            Mock<IElasticSearchLayer> mockAlertStatus = new Mock<IElasticSearchLayer>();

            //add moq here and database stuff
            Mock<EventListViewModel> testWindow = new Mock<EventListViewModel>(listOfEvents, "Title");

            Assert.Multiple(() =>
            {
                for (int i = 0; i < testWindow.Object._ErrorAlertsCollection.Count; i++)
                {
                    Assert.That(testWindow.Object._ErrorAlertsCollection[i].Level, Is.EqualTo(listOfEvents[i].Level));
                    Assert.That(testWindow.Object._ErrorAlertsCollection[i].StackTrace, Is.EqualTo(listOfEvents[i].StackTrace));

                    int page = i / testWindow.Object.PageCutoffValue;
                    int itemOnPage = i % testWindow.Object.PageCutoffValue;

                    Assert.That(testWindow.Object.SeperatedEventList[page][itemOnPage].StackTrace, Is.EqualTo(testWindow.Object._ErrorAlertsCollection[i].StackTrace));
                    Assert.That(testWindow.Object.SeperatedEventList[page][itemOnPage].Level, Is.EqualTo(testWindow.Object._ErrorAlertsCollection[i].Level));

                }
            });
        }

        [Test]
        [Ignore("Will be covered in dedicated Jira https://extract.atlassian.net/browse/ISSUE-19046")]
        public void TestEventTableNull()
        {
            IElasticSearchLayer? nullable = null;

            Assert.Multiple( () => {
                Assert.DoesNotThrow(() => { EventListViewModel testWindow = new EventListViewModel(nullable, "Testing"); });
            });
            
        }

        [Test]
        [Ignore("Will be covered in dedicated Jira https://extract.atlassian.net/browse/ISSUE-19046")]
        public void TestEventTableTitle()
        {
            Mock<IElasticSearchLayer> mockAlertStatus = new Mock<IElasticSearchLayer>();

            Assert.Multiple(() => {
                EventListViewModel testWindow = new EventListViewModel(mockAlertStatus.Object, "Testing");
                Assert.That(testWindow.EventTitle, Is.EqualTo("Testing"));

                testWindow = new EventListViewModel(new List<ExceptionEvent>(), "Title1");
                Assert.That(testWindow.EventTitle, Is.EqualTo("Title1"));
            });
        }

        //todo test both singe output and error alerting
        [Test]
        [TestCaseSource(nameof(EventListSource))]
        public void TestErrorTableFromElasticMultipleValue(List<ExceptionEvent> listOfEvents)
        {
            Mock<IElasticSearchLayer> mockAlertStatus = new Mock<IElasticSearchLayer>();

            Mock<EventListViewModel> testWindow = new Mock<EventListViewModel>(mockAlertStatus.Object, "Title");

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

        [Test]
        [TestCaseSource(nameof(EventListSource))]
        public void TestErrorTableFromListMultipleValues(List<ExceptionEvent> listOfEvents)
        {
            Mock<EventListViewModel> testWindow = new Mock<EventListViewModel>(listOfEvents, "Title");

            Assert.Multiple(() =>
            {
                int expectedNumberOfPages = listOfEvents.Count / testWindow.Object.PageCutoffValue;
                expectedNumberOfPages += 1; //in the program we start at page 1 so add 1
                Assert.That(testWindow.Object.SeperatedEventList.Count, Is.EqualTo(expectedNumberOfPages));
                
            });
        }


        public static IEnumerable<ExceptionEvent> EventsSource()
        {
            yield return new ExceptionEvent("ELI1", "Testing string1", "1",
                    new(), DateTime.Now, new List<DictionaryEntry>(), new(), "1", new(), 1, 1,
                        "databaseServer", "databaseName");

            yield return new ExceptionEvent("ELI2", "Testing string2", "2",
                    new(), DateTime.Now, new List<DictionaryEntry>(), new(), "2", new(), 2, 2,
                        "databaseServer", "databaseName");

            yield return new();
        }
        
        public static IEnumerable<List<ExceptionEvent>> EventListSource()
        {
            List<ExceptionEvent> listOfEvents = new List<ExceptionEvent>();
            for(int i = 0; i < 10; i++)
            {
                string eliCode = "ELI" + i.ToString();
                ExceptionEvent exceptionToAdd = new ExceptionEvent(eliCode, "Testing string", i.ToString(),
                    new(), DateTime.Now, new List<DictionaryEntry>(),
                        new(), i.ToString(), new(), i, i, "databaseName", "databaseServer");

                listOfEvents.Add(exceptionToAdd);
            }

            List<ExceptionEvent> listOfEvents2 = new List<ExceptionEvent>();
            for (int i = 0; i < 1000; i++)
            {
                string eliCode = "ELI" + i.ToString();
                ExceptionEvent exceptionToAdd = new ExceptionEvent(eliCode, "Testing string", i.ToString(),
                    new(), DateTime.Now, new List<DictionaryEntry>(),
                        new(), i.ToString(), new(), i, i, "databaseName", "databaseServer");

                listOfEvents2.Add(exceptionToAdd);
            }

            List<ExceptionEvent> listOfEvents3 = new List<ExceptionEvent>();

            yield return listOfEvents;
            yield return listOfEvents2;
            yield return listOfEvents3;
        }

    }
}
