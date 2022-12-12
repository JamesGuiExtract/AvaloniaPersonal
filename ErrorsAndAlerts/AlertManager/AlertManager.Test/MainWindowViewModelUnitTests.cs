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

namespace Extract.ErrorsAndAlerts.AlertManager.Test
{
    [TestFixture]
    public class MainWindowViewModelUnitTests
    {
        MainWindowViewModel testWindow;
        MockDBService mockDatabase;
        MockAlertStatus mockAlertStatus;


        /// <summary>
        /// Initialization of project
        /// </summary>
        [SetUp]
        public void Init()
        {
            mockDatabase = new MockDBService();
            mockAlertStatus = new();

            //add moq here and database stuff
            testWindow = new(mockDatabase, mockAlertStatus);
        }

        [Test]
        public void TestEmptyConstructor()
        {

        }

        //tests the initialization values made by constructor, checks if they are correct
        [Test]
        public void ConstructorTest([ValueSource(nameof(EventsSource))] EventObject eventObject, [ValueSource(nameof(AlertsSource))] AlertsObject alertObject)
        {
            mockDatabase.AddAlertObjects(alertObject);
            mockDatabase.AddEventObjects(eventObject);

            mockAlertStatus.AddToAlertList(alertObject);
            mockAlertStatus.AddToEventList(eventObject);

            testWindow = new(mockDatabase, mockAlertStatus);

            Assert.Multiple(() => 
            {
                Assert.That(testWindow._AlertTable, Is.Not.Null);
                Assert.That(testWindow._ErrorAlertsCollection, Is.Not.Null);

                Assert.That(testWindow._AlertTable.Count, Is.EqualTo(mockDatabase.ReadAlertObjects().Count));
                Assert.That(testWindow._ErrorAlertsCollection.Count, Is.EqualTo(mockDatabase.ReadEvents().Count));


                for (int i = 0; i < testWindow._AlertTable.Count; i++)
                {
                    Assert.That(testWindow._AlertTable[i].AlertId, Is.EqualTo(mockDatabase.ReadAlertObjects()[i].AlertId));
                    Assert.That(testWindow._AlertTable[i].AlertHistory, Is.EqualTo(mockDatabase.ReadAlertObjects()[i].AlertHistory));
                }

                for (int i = 0; i < testWindow._ErrorAlertsCollection.Count; i++)
                {
                    Assert.That(testWindow._ErrorAlertsCollection[i].additional_Details, Is.EqualTo(mockDatabase.ReadEvents()[i].additional_Details));
                    Assert.That(testWindow._ErrorAlertsCollection[i].stack_Trace, Is.EqualTo(mockDatabase.ReadEvents()[i].stack_Trace));
                }

                Assert.That(testWindow._AlertTable, Is.EqualTo(mockDatabase.ReadAlertObjects()));
                Assert.That(testWindow._ErrorAlertsCollection, Is.EqualTo(mockDatabase.ReadEvents()));
            });
            
        }

        [Test]
        public void TestMockAlertStatusNull()
        {
            Assert.DoesNotThrow(() => { MainWindowViewModel testMainWindow = new MainWindowViewModel(mockDatabase, null); });
        }

        /// <summary>
        /// 
        /// </summary>
        [Test]
        public void ConstructorNullTest()
        {
            Assert.DoesNotThrow(() => { MainWindowViewModel testMainWindow = new MainWindowViewModel(null, mockAlertStatus); });
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
        public void TestAlertsTable([ValueSource(nameof(EventsSource))] EventObject eventObject, [ValueSource(nameof(AlertsSource))] AlertsObject alertObject)
        {
            mockAlertStatus.AddToAlertList(alertObject);
            mockAlertStatus.AddToEventList(eventObject);

            //right now it tests hardcoded values from mock db service
            mockDatabase.AddAlertObjects(alertObject);
            List<AlertsObject> returnList = mockDatabase.ReadAlertObjects();
            ObservableCollection<AlertsObject> alertListFromDB = new();

            alertListFromDB.AddRange(returnList);

            testWindow = new(mockDatabase, mockAlertStatus);

            Assert.Multiple( () =>
            {
                Assert.That(alertListFromDB, Is.EqualTo(testWindow._AlertTable));

                for (int i = 0; i < testWindow._AlertTable.Count; i++)
                {
                    Assert.That(testWindow._AlertTable[i].AlertType, Is.EqualTo(alertListFromDB[i].AlertType));
                    Assert.That(testWindow._AlertTable[i].AlertName, Is.EqualTo(alertListFromDB[i].AlertName));
                }
            });
            
        }

        //guess i could add to table if we decide that, but don't see that happening

        /// <summary>
        /// tests that the table successfully handles a null/invalid value passed to it
        /// </summary>
        [Test]
        public void TestAlertsTableNull()
        {
            testWindow = new MainWindowViewModel(null, null);
            
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
            mockDatabase.AddAlertObjects(listOfObjects);
            List<AlertsObject> returnList = mockDatabase.ReadAlertObjects();
            ObservableCollection<AlertsObject> alertListFromDB = new();

            alertListFromDB.AddRange(returnList);

            testWindow = new(mockDatabase, mockAlertStatus);

            Assert.Multiple(() =>
            {
                Assert.That(alertListFromDB, Is.EqualTo(testWindow._AlertTable));

                for (int i = 0; i < testWindow._AlertTable.Count; i++)
                {
                    Assert.That(testWindow._AlertTable[i].AlertType, Is.EqualTo(alertListFromDB[i].AlertType));
                    Assert.That(testWindow._AlertTable[i].AlertName, Is.EqualTo(alertListFromDB[i].AlertName));
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
        [TestCaseSource(nameof(EventsSource))]
        public void TestEventTable(EventObject eventObject)
        {
            mockAlertStatus.AddToEventList(eventObject);
            //right now it tests hardcoded values from mock db service
            mockDatabase.AddEventObjects(eventObject);
            List<EventObject> returnList = mockDatabase.ReadEvents();
            ObservableCollection<EventObject> eventList = new();

            eventList.AddRange(returnList);

            testWindow = new(mockDatabase, mockAlertStatus);

            Assert.That(eventList, Is.EqualTo(testWindow._ErrorAlertsCollection));

            Assert.Multiple(() =>
            {
                Assert.That(eventList, Is.EqualTo(testWindow._ErrorAlertsCollection));

                for (int i = 0; i < testWindow._AlertTable.Count; i++)
                {
                    Assert.That(testWindow._ErrorAlertsCollection[i].additional_Details, Is.EqualTo(eventList[i].additional_Details));
                    Assert.That(testWindow._ErrorAlertsCollection[i].contains_Stack_Trace, Is.EqualTo(eventList[i].contains_Stack_Trace));
                }
            });
        }

        [Test]
        public void TestEventTableNull()
        {
            testWindow = new MainWindowViewModel(null, null);


            //use this instead of not throws b/c its more specific
            //note interesting interaction with bindings and nulls, so maybe check if its not equal to?
            Assert.That(testWindow._ErrorAlertsCollection, Is.Not.Null);
        }

        [Test]
        [TestCaseSource(nameof(EventListSource))]
        public void TestErrorTableMultipleValue(List<EventObject> listOfEvents)
        {
            mockDatabase.AddEventObjects(listOfEvents);

            List<EventObject> returnList = mockDatabase.ReadEvents();
            ObservableCollection<EventObject> eventList = new();

            eventList.AddRange(returnList);

            testWindow = new(mockDatabase, mockAlertStatus);

            Assert.Multiple(() =>
            {
                Assert.That(eventList, Is.EqualTo(testWindow._ErrorAlertsCollection));

                for (int i = 0; i < testWindow._AlertTable.Count; i++)
                {
                    Assert.That(testWindow._ErrorAlertsCollection[i].additional_Details, Is.EqualTo(eventList[i].additional_Details));
                    Assert.That(testWindow._ErrorAlertsCollection[i].contains_Stack_Trace, Is.EqualTo(eventList[i].contains_Stack_Trace));
                }
            });
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
            yield return new AlertsObject(1, "AlertId", "TestAction2", "TestType2", "TestAlertName", "testconfig2", new DateTime(2008, 5, 1, 8, 30, 52), "testUser2", "testMachine", "testResolution", TypeOfResolutionAlerts.Snoozed, new DateTime(2008, 5, 1, 8, 30, 52), "testingAlertHistory");
        }

        public static IEnumerable<EventObject> EventsSource()
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

        public static IEnumerable<List<EventObject>> EventListSource()
        {
            yield return new();
        }
        #endregion Sources

    }
}
