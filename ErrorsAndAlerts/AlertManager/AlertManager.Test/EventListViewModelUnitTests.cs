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
            Mock<IWindowService> mockWindow = new();
            Mock<IElasticSearchLayer> mockElastic = new();
            Mock<EventsOverallViewModelFactory> mockEventFactory = new(mockWindow.Object, mockElastic.Object);
            
            string eventTitle = "Fake Test Title";

            List<EventDto> events = new()
            {
                eventObject
            };

            mockElastic.Setup(m => m.GetAllAlerts(0)).Returns(new List<AlertsObject>());
            mockElastic.Setup(m => m.GetAllEvents(0)).Returns(events);
            mockElastic.Setup(m => m.GetMaxEventPages()).Returns(1);

            EventListViewModel sut = new(mockWindow.Object, mockEventFactory.Object, mockElastic.Object, eventTitle);

            Assert.Multiple(() =>
            {
                for (int i = 0; i < sut.EventTableCollection.Count; i++)
                {
                    Assert.That(sut.EventTableCollection[i].EventObject.Level, Is.EqualTo(events[i].Level));
                    Assert.That(sut.EventTableCollection[i].EventObject.StackTrace, Is.EqualTo(events[i].StackTrace));
                }
            });
        }

        [Test]
        [TestCaseSource(nameof(EventListSource))]
        public void TestConstructorFromEventList(List<EventDto> listOfEvents)
        {
            Mock<IWindowService> mockWindow = new();
            Mock<IElasticSearchLayer> mockElastic = new();
            Mock<EventsOverallViewModelFactory> mockEventFactory = new(mockWindow.Object, mockElastic.Object);

            string eventTitle = "Fake Test Title";

            EventListViewModel sut = new(mockWindow.Object, mockEventFactory.Object, listOfEvents, eventTitle);

            Assert.Multiple(() =>
            {
                for (int i = 0; i < sut.EventTableCollection.Count; i++)
                {
                    Assert.That(sut.EventTableCollection[i].EventObject.Level, Is.EqualTo(listOfEvents[i].Level));
                    Assert.That(sut.EventTableCollection[i].EventObject.StackTrace, Is.EqualTo(listOfEvents[i].StackTrace));

                    int page = i / sut.PageCutoffValue;
                    int itemOnPage = i % sut.PageCutoffValue;

                    Assert.That(sut.SeparatedEventList[page][itemOnPage].StackTrace, Is.EqualTo(sut.EventTableCollection[i].EventObject.StackTrace));
                    Assert.That(sut.SeparatedEventList[page][itemOnPage].Level, Is.EqualTo(sut.EventTableCollection[i].EventObject.Level));
                }
            });
        }

        [Test]
        [TestCaseSource(nameof(EventListSource))]
        public void TestErrorTableFromElasticMultipleValue(List<EventDto> listOfEvents)
        {
            Mock<IWindowService> mockWindow = new();
            Mock<IElasticSearchLayer> mockElastic = new();
            Mock<EventsOverallViewModelFactory> mockEventFactory = new(mockWindow.Object, mockElastic.Object);

            string eventTitle = "Fake Test Title";

            mockElastic.Setup(m => m.GetAllAlerts(0)).Returns(new List<AlertsObject>());
            mockElastic.Setup(m => m.GetAllEvents(0)).Returns(new List<EventDto>());
            mockElastic.Setup(m => m.GetMaxEventPages()).Returns(1);

            EventListViewModel sut = new(mockWindow.Object, mockEventFactory.Object, mockElastic.Object, eventTitle);

            Assert.Multiple(() =>
            {
                for (int i = 0; i < sut.EventTableCollection.Count; i++)
                {
                    Assert.That(sut.EventTableCollection[i].EventObject.Level, Is.EqualTo(listOfEvents[i].Level));
                    Assert.That(sut.EventTableCollection[i].EventObject.StackTrace, Is.EqualTo(listOfEvents[i].StackTrace));
                }
            });
        }

        [Test]
        [TestCaseSource(nameof(EventListSource))]
        public void TestErrorTableFromListMultipleValues(List<EventDto> listOfEvents)
        {
            Mock<IWindowService> mockWindow = new();
            Mock<IElasticSearchLayer> mockElastic = new();
            Mock<EventsOverallViewModelFactory> mockEventFactory = new(mockWindow.Object, mockElastic.Object);

            string eventTitle = "Fake Test Title";

            mockElastic.Setup(m => m.GetAllAlerts(0)).Returns(new List<AlertsObject>());
            mockElastic.Setup(m => m.GetAllEvents(0)).Returns(new List<EventDto>());
            mockElastic.Setup(m => m.GetMaxEventPages()).Returns(1);

            EventListViewModel sut = new(mockWindow.Object, mockEventFactory.Object, listOfEvents, eventTitle);

            Assert.Multiple(() =>
            {
                int expectedNumberOfPages = listOfEvents.Count / sut.PageCutoffValue;
                expectedNumberOfPages += 1; //in the program we start at page 1 so add 1
                Assert.That(sut.SeparatedEventList.Count, Is.EqualTo(expectedNumberOfPages));
                
            });
        }

        #region sources

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

        #endregion sources
    }
}
