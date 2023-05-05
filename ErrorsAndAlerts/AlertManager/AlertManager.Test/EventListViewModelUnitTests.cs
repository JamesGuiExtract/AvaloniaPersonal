using AlertManager.Interfaces;
using AlertManager.Models.AllDataClasses;
using AlertManager.ViewModels;
using Extract.ErrorHandling;
using Moq;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;
using Extract.ErrorsAndAlerts.ElasticDTOs;

namespace Extract.ErrorsAndAlerts.AlertManager.Test
{
    public class EventListViewModelUnitTests
    {
        [Test]
        [TestCaseSource(nameof(EventsSource))]
        public void TestConstructorElastic(EventDto eventObject)
        {
            Mock<IElasticSearchLayer> mockAlertStatus = new();

            List<EventDto> events = new();
            events.Add(eventObject);

            //add moq here and database stuff
            Mock<EventListViewModel> testWindow = new(mockAlertStatus.Object, "Title");

            mockAlertStatus.Setup(m => m.GetAllAlerts(0)).Returns(new List<AlertsObject>());
            mockAlertStatus.Setup(m => m.GetAllEvents(0)).Returns(events);
            mockAlertStatus.Setup(m => m.GetMaxEventPages()).Returns(1);

            Assert.Multiple(() =>
            {
                for (int i = 0; i < testWindow.Object.EventTableCollection.Count; i++)
                {
                    Assert.That(testWindow.Object.EventTableCollection[i].EventObject.Level, Is.EqualTo(events[i].Level));
                    Assert.That(testWindow.Object.EventTableCollection[i].EventObject.StackTrace, Is.EqualTo(events[i].StackTrace));
                }
            });
        }

        [Test]
        [TestCaseSource(nameof(EventListSource))]

        public void TestConstructureFromList(List<EventDto> listOfEvents)
        {
            Mock<IElasticSearchLayer> mockAlertStatus = new();

            //add moq here and database stuff
            Mock<EventListViewModel> testWindow = new(listOfEvents, "Title");

            Assert.Multiple(() =>
            {
                for (int i = 0; i < testWindow.Object.EventTableCollection.Count; i++)
                {
                    Assert.That(testWindow.Object.EventTableCollection[i].EventObject.Level, Is.EqualTo(listOfEvents[i].Level));
                    Assert.That(testWindow.Object.EventTableCollection[i].EventObject.StackTrace, Is.EqualTo(listOfEvents[i].StackTrace));

                    int page = i / testWindow.Object.PageCutoffValue;
                    int itemOnPage = i % testWindow.Object.PageCutoffValue;

                    Assert.That(testWindow.Object.SeparatedEventList[page][itemOnPage].StackTrace, Is.EqualTo(testWindow.Object.EventTableCollection[i].EventObject.StackTrace));
                    Assert.That(testWindow.Object.SeparatedEventList[page][itemOnPage].Level, Is.EqualTo(testWindow.Object.EventTableCollection[i].EventObject.Level));

                }
            });
        }

        [Test]
        [Ignore("Will be covered in dedicated Jira https://extract.atlassian.net/browse/ISSUE-19046")]
        public void TestEventTableNull()
        {
            //IElasticSearchLayer? nullable = null;

            //Assert.Multiple( () => {
            //    Assert.DoesNotThrow(() => { EventListViewModel testWindow = new(nullable, "Testing"); });
            //});
            
        }

        [Test]
        [Ignore("Will be covered in dedicated Jira https://extract.atlassian.net/browse/ISSUE-19046")]
        public void TestEventTableTitle()
        {
            Mock<IElasticSearchLayer> mockAlertStatus = new();

            //Assert.Multiple(() => {
            //    EventListViewModel testWindow = new(mockAlertStatus.Object, "Testing");
            //    Assert.That(testWindow.EventTitle, Is.EqualTo("Testing"));

            //    testWindow = new EventListViewModel(new List<EventDto>(), "Title1");
            //    Assert.That(testWindow.EventTitle, Is.EqualTo("Title1"));
            //});
        }

        //todo test both singe output and error alerting
        [Test]
        [TestCaseSource(nameof(EventListSource))]
        public void TestErrorTableFromElasticMultipleValue(List<EventDto> listOfEvents)
        {
            Mock<IElasticSearchLayer> mockAlertStatus = new();

            Mock<EventListViewModel> testWindow = new(mockAlertStatus.Object, "Title");

            mockAlertStatus.Setup(m => m.GetAllEvents(0)).Returns(new List<EventDto>());
            mockAlertStatus.Setup(m => m.GetMaxEventPages()).Returns(1);

            Assert.Multiple(() =>
            {
                for (int i = 0; i < testWindow.Object.EventTableCollection.Count; i++)
                {
                    Assert.That(testWindow.Object.EventTableCollection[i].EventObject.Level, Is.EqualTo(listOfEvents[i].Level));
                    Assert.That(testWindow.Object.EventTableCollection[i].EventObject.StackTrace, Is.EqualTo(listOfEvents[i].StackTrace));
                }
            });
        }

        [Test]
        [TestCaseSource(nameof(EventListSource))]
        public void TestErrorTableFromListMultipleValues(List<EventDto> listOfEvents)
        {
            Mock<EventListViewModel> testWindow = new(listOfEvents, "Title");

            Assert.Multiple(() =>
            {
                int expectedNumberOfPages = listOfEvents.Count / testWindow.Object.PageCutoffValue;
                expectedNumberOfPages += 1; //in the program we start at page 1 so add 1
                Assert.That(testWindow.Object.SeparatedEventList.Count, Is.EqualTo(expectedNumberOfPages));
                
            });
        }


        public static IEnumerable<EventDto> EventsSource()
        {
            yield return new EventDto
            {
                EliCode = "ELI1", 
                Message = "Testing string1", 
                Id = "1",
                Context = new ContextInfoDto
                {
                    FileID = 1,
                    ActionID = 1,
                    DatabaseServer = "databaseServer",
                    DatabaseName = "databaseName",
                    FpsContext = "fpsContext",
                }, 
                ExceptionTime = DateTime.Now, 
                Data = new List<KeyValuePair<string, string>>(), 
                StackTrace = new(), 
                Level = "1", 
                Inner = new(),
            };

            yield return new EventDto
            {
                EliCode = "ELI2",
                Message = "Testing string2",
                Id = "2",
                Context = new ContextInfoDto
                {
                    FileID = 2,
                    ActionID = 2,
                    DatabaseServer = "databaseServer",
                    DatabaseName = "databaseName",
                    FpsContext = "fpsContext",
                },
                ExceptionTime = DateTime.Now,
                Data = new List<KeyValuePair<string, string>>(),
                StackTrace = new(),
                Level = "2",
                Inner = new(),
            };

            yield return new();
        }
        
        public static IEnumerable<List<EventDto>> EventListSource()
        {
            List<EventDto> listOfEvents = new();
            for(int i = 0; i < 10; i++)
            {
                string eliCode = "ELI" + i.ToString();

                EventDto eventToAdd = new EventDto
                {
                    EliCode = eliCode,
                    Message = "Testing string",
                    Id = i.ToString(),
                    Context = new ContextInfoDto
                    {
                        FileID = i,
                        ActionID = i,
                        DatabaseServer = "databaseServer",
                        DatabaseName = "databaseName",
                        FpsContext = "fpsContext",
                    },
                    ExceptionTime = DateTime.Now,
                    Data = new List<KeyValuePair<string, string>>(),
                    StackTrace = new Stack<string>(),
                    Level = i.ToString(),
                    Inner = new EventDto(),
                };

                listOfEvents.Add(eventToAdd);
            }

            List<EventDto> listOfEvents2 = new();
            for (int i = 0; i < 1000; i++)
            {
                string eliCode = "ELI" + i.ToString();

                EventDto eventToAdd = new EventDto
                {
                    EliCode = eliCode,
                    Message = "Testing string",
                    Id = i.ToString(),
                    Context = new ContextInfoDto
                    {
                        FileID = i,
                        ActionID = i,
                        DatabaseServer = "databaseServer",
                        DatabaseName = "databaseName",
                        FpsContext = "fpsContext",
                    },
                    ExceptionTime = DateTime.Now,
                    Data = new List<KeyValuePair<string, string>>(),
                    StackTrace = new Stack<string>(),
                    Level = i.ToString(),
                    Inner = new EventDto(),
                };

                listOfEvents2.Add(eventToAdd);
            }

            List<EventDto> listOfEvents3 = new();

            yield return listOfEvents;
            yield return listOfEvents2;
            yield return listOfEvents3;
        }

    }
}
