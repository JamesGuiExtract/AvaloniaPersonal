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
using Extract.ErrorsAndAlerts.ElasticDTOs;

namespace Extract.ErrorsAndAlerts.AlertManager.Test
{
    [TestFixture]
    public class MainWindowViewModelUnitTests
    {


        /// <summary>
        /// Initialization of project
        /// </summary>
        [SetUp]
        public void Init()
        {
            
        }

        [Test]
        public void ConstructorTest()
        {
            Mock<IWindowService> mockWindowService = new();
            Mock<IElasticSearchLayer> mockElasticClient = new();
            Mock<IAlertActionLogger> mockActionLogger = new();
            Mock<IDBService> mockDatabase = new();

            Mock<EventsOverallViewModelFactory> mockEventsVMFactory = new(mockWindowService.Object, mockElasticClient.Object);

            MainWindowViewModel sut = new(mockWindowService.Object, mockEventsVMFactory.Object, mockElasticClient.Object, mockActionLogger.Object, mockDatabase.Object);
        }

        #region TestTables
        
        /// <summary>
        /// tests the values displayed to table from database
        /// Uses a sourced value to test multiple times
        /// </summary>
        [Test]
        public void TestAlertsTable([ValueSource(nameof(EventsSource))] EventDto eventObject, [ValueSource(nameof(AlertsSource))] AlertsObject alertObject)
        {
            Mock<IWindowService> mockWindowService = new();
            Mock<IElasticSearchLayer> mockElasticClient = new();
            Mock<IAlertActionLogger> mockActionLogger = new();
            Mock<IDBService> mockDatabase = new();

            Mock<EventsOverallViewModelFactory> mockEventsVMFactory = new(mockWindowService.Object, mockElasticClient.Object);

            List<EventDto> events = new();
            events.Add(eventObject);

            List<AlertsObject> alerts = new();
            alerts.Add(alertObject);

            mockElasticClient.Setup(m => m.GetAllAlerts(0)).Returns(alerts);
            mockElasticClient.Setup(m => m.GetAllEvents(0)).Returns(events);
            mockElasticClient.Setup(m => m.GetMaxAlertPages()).Returns(1);
            mockElasticClient.Setup(m => m.GetMaxEventPages()).Returns(1);

            MainWindowViewModel sut = new(mockWindowService.Object, mockEventsVMFactory.Object, mockElasticClient.Object, mockActionLogger.Object, mockDatabase.Object);
            sut.Activator.Activate();

            Assert.Multiple( () =>
            {
                Assert.That(sut.AlertTable[0].Alert.HitsType, Is.EqualTo(alerts[0].HitsType));
                Assert.That(sut.AlertTable[0].Alert.AlertName, Is.EqualTo(alerts[0].AlertName));
            });
            
        }

        [Test]
        [TestCaseSource(nameof(AlertsListSource))]
        public void TestAlertsTableMultipleValue(List<AlertsObject> listOfObjects)
        {
            Mock<IWindowService> mockWindowService = new();
            Mock<IElasticSearchLayer> mockElasticClient = new();
            Mock<IAlertActionLogger> mockActionLogger = new();
            Mock<IDBService> mockDatabase = new();

            Mock<EventsOverallViewModelFactory> mockEventsVMFactory = new(mockWindowService.Object, mockElasticClient.Object);

            List<AlertsObject> alerts = new();
            alerts.Add(listOfObjects);

            mockElasticClient.Setup(m => m.GetAllAlerts(0)).Returns(alerts);
            mockElasticClient.Setup(m => m.GetAllEvents(0)).Returns(new List<EventDto>());
            mockElasticClient.Setup(m => m.GetMaxAlertPages()).Returns(1);
            mockElasticClient.Setup(m => m.GetMaxEventPages()).Returns(1);

            MainWindowViewModel sut = new(mockWindowService.Object, mockEventsVMFactory.Object, mockElasticClient.Object, mockActionLogger.Object, mockDatabase.Object);
            sut.Activator.Activate();

            Assert.Multiple(() =>
            {
                for (int i = 0; i < sut.AlertTable.Count; i++)
                {
                    Assert.That(sut.AlertTable[i].Alert.HitsType, Is.EqualTo(listOfObjects[i].HitsType));
                    Assert.That(sut.AlertTable[i].Alert.AlertName, Is.EqualTo(listOfObjects[i].AlertName));
                }
            });
        }
        
        #endregion TestTables

        #region Sources
        public static IEnumerable<AlertsObject> AlertsSource()
        {
            yield return new();
            yield return new AlertsObject(
                alertId: "AlertId",
                hitsType: "TestType2",
                alertName: "TestAlertName",
                configuration: "testconfig2",
                activationTime: new DateTime(2008, 5, 1, 8, 30, 52),
                associatedEvents: Array.Empty<EventDto>(),
                listOfActions: Array.Empty<AlertActionDto>()
                );
        }

        public static IEnumerable<EventDto> EventsSource()
        {
            yield return new();
        }

        public static IEnumerable<List<AlertsObject>> AlertsListSource()
        {
            yield return new();

        }

        public static IEnumerable<List<EventDto>> EventListSource()
        {
            yield return new();
        }
        #endregion Sources

    }
}
