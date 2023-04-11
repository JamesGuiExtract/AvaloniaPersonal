﻿using AlertManager;
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
        public void TestEmptyConstructor()
        {

        }

        //tests the initialization values made by constructor, checks if they are correct
        [Test]
        public void ConstructorTest([ValueSource(nameof(EventsSource))] ExceptionEvent eventObject, [ValueSource(nameof(AlertsSource))] AlertsObject alertObject)
        {

            Mock<IDBService> mockDatabase = new Mock<IDBService>();
            Mock<IElasticSearchLayer> mockAlertStatus = new Mock<IElasticSearchLayer>();

            List<ExceptionEvent> events = new();
            events.Add(eventObject);

            List<AlertsObject> alerts = new();
            alerts.Add(alertObject);

            mockAlertStatus.Setup(m => m.GetAllAlerts(0)).Returns(alerts);
            mockAlertStatus.Setup(m => m.GetAllEvents(0)).Returns(events);
            mockAlertStatus.Setup(m => m.GetMaxAlertPages()).Returns(1);
            mockAlertStatus.Setup(m => m.GetMaxEventPages()).Returns(1);


            //ILoggingTarget
            Mock<MainWindowViewModel> testWindow = new(mockAlertStatus.Object);

            Assert.Multiple(() => 
            {
                Assert.That(testWindow.Object._AlertTable, Is.Not.Null);


                for (int i = 0; i < testWindow.Object._AlertTable.Count; i++)
                {
                    Assert.That(testWindow.Object._AlertTable[i].AlertId, Is.EqualTo(alerts[0].AlertId));
                    Assert.That(testWindow.Object._AlertTable[i].ActivationTime, Is.EqualTo(alerts[0].ActivationTime));
                }
                Assert.That(testWindow.Object._AlertTable, Is.EqualTo(alerts));
            });
            
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        [Ignore("Constructor has been modified to work with null parameter")]
        public void ConstructorNullTest()
        {
            Assert.Throws<ExtractException>(() => { MainWindowViewModel testMainWindow = new MainWindowViewModel(null); });
        }

        [Test]
        [Ignore("not tested atm")]
        public void TestCurrentApplication()
        {
            //Assert.False(MainWindowViewModel.CurrentInstance, null);
            
        }

        #region TestTables
        
        /// <summary>
        /// tests the values displayed to table from database
        /// Uses a sourced value to test multiple times
        /// </summary>
        [Test]
        public void TestAlertsTable([ValueSource(nameof(EventsSource))] ExceptionEvent eventObject, [ValueSource(nameof(AlertsSource))] AlertsObject alertObject)
        {
            Mock<IElasticSearchLayer> mockAlertStatus = new Mock<IElasticSearchLayer>();

            //add moq here and database stuff
            Mock<MainWindowViewModel> testWindow = new Mock<MainWindowViewModel>(mockAlertStatus.Object);


            List<ExceptionEvent> events = new();
            events.Add(eventObject);


            List<AlertsObject> alerts = new();
            alerts.Add(alertObject);

            mockAlertStatus.Setup(m => m.GetAllAlerts(0)).Returns(alerts);
            mockAlertStatus.Setup(m => m.GetAllEvents(0)).Returns(events);
            mockAlertStatus.Setup(m => m.GetMaxAlertPages()).Returns(1);
            mockAlertStatus.Setup(m => m.GetMaxEventPages()).Returns(1);

            Assert.Multiple( () =>
            {
                Assert.That(alerts, Is.EqualTo(testWindow.Object._AlertTable));

                for (int i = 0; i < testWindow.Object._AlertTable.Count; i++)
                {
                    Assert.That(testWindow.Object._AlertTable[i].AlertType, Is.EqualTo(alerts[i].AlertType));
                    Assert.That(testWindow.Object._AlertTable[i].AlertName, Is.EqualTo(alerts[i].AlertName));
                }
            });
            
        }


        //guess i could add to table if we decide that, but don't see that happening

        /// <summary>
        /// tests that the table successfully handles a null/invalid value passed to it
        /// </summary>
        [Test]
        [Ignore("errors not handeled properly w/ extract exception atm")]
        public void TestAlertsTableNull()
        {
            MainWindowViewModel testWindow = new MainWindowViewModel(null);
            
            Assert.Multiple(() =>
            {
                Assert.That(testWindow._AlertTable, Is.Not.Null);
                //Assert.That(new ObservableCollection<AlertsObject>().Count, Is.EqualTo(testWindow._AlertTable.Count)); // should be 0
            });
            
        }

        [Test]
        [TestCaseSource(nameof(AlertsListSource))]
        public void TestAlertsTableMultipleValue(List<AlertsObject> listOfObjects)
        {
            Mock<IElasticSearchLayer> mockAlertStatus = new Mock<IElasticSearchLayer>();

            //add moq here and database stuff
            Mock<MainWindowViewModel> testWindow = new Mock<MainWindowViewModel>(mockAlertStatus.Object);

            List<AlertsObject> alerts = new();
            alerts.Add(listOfObjects);

            mockAlertStatus.Setup(m => m.GetAllAlerts(0)).Returns(alerts);

            mockAlertStatus.Setup(m => m.GetAllEvents(0)).Returns(new List<ExceptionEvent>());
            mockAlertStatus.Setup(m => m.GetMaxAlertPages()).Returns(1);
            mockAlertStatus.Setup(m => m.GetMaxEventPages()).Returns(1);


            Assert.Multiple(() =>
            {
                for (int i = 0; i < testWindow.Object._AlertTable.Count; i++)
                {
                    Assert.That(testWindow.Object._AlertTable[i].AlertType, Is.EqualTo(listOfObjects[i].AlertType));
                    Assert.That(testWindow.Object._AlertTable[i].AlertName, Is.EqualTo(listOfObjects[i].AlertName));
                }
            });
        }

        [Test]
        [Ignore("no automatic triggers in place to update from database yet")]
        public void TestAlertsTableAddValue()
        {
            //no automatic triggers in place to update from database yet
            
        }



        [Test]
        [Ignore("no automatic triggers in place to update from database yet")]
        public void TestErrorTableAddValue()
        {
            
        }
        //file table not tested due not part of mvp
        
        #endregion TestTables


        //README: don't think i need to do refresh multiple if tables are passing

        #region Dialogue And Window Tests
        //todo cneed to test return of dialgue as well, figure out how to do that
        [Test]
        [Ignore("Potential test for future")]
        public void TestEventsDialogue()
        {
            
        }

        [Test]
        [Ignore("Potential test for future")]
        public void TestRefreshEventDialogueExit()
        {
            
        }


        [Test]
        [Ignore("Potential test for future")]
        public void TestResolveAlertsDialogue()
        {
            
        }


        [Test]
        [Ignore("Potential test for future")]
        public void TestRefreshAlertsDialogueExit()
        {
            
        }

        [Test]
        [Ignore("Potential test for future")]
        public void TestEventsConfigurationDialogue()
        {
            
        }

        [Test]
        [Ignore("Potential test for future")]
        public void TestRefreshConfigurationDialogueExit()
        {
            
        }


        #endregion Dialogue And Window Tests

        //future tests, todo, test the auto resolving, and other filtering options

        #region Sources
        public static IEnumerable<AlertsObject> AlertsSource()
        {
            yield return new();
            yield return new AlertsObject(
                alertId: "AlertId",
                alertType: "TestType2",
                alertName: "TestAlertName",
                configuration: "testconfig2",
                activationTime: new DateTime(2008, 5, 1, 8, 30, 52),
                associatedEvents: new List<ExceptionEvent>(),
                listOfResolutions: new()
                );
        }

        public static IEnumerable<ExceptionEvent> EventsSource()
        {
            yield return new();
        }

        public static IEnumerable<DataNeededForPage> DataSource()
        {
            yield return new();
        }

        public static IEnumerable<List<AlertsObject>> AlertsListSource()
        {
            yield return new();

        }

        public static IEnumerable<List<ExceptionEvent>> EventListSource()
        {
            yield return new();
        }
        #endregion Sources

    }
}
