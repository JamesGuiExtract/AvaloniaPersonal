using AlertManager;
using AlertManager.Interfaces;
using AlertManager.Models.AllDataClasses;
using AlertManager.Services;
using AlertManager.ViewModels;
using AlertManager.Views;
using DynamicData;
using Extract.ErrorsAndAlerts.AlertManager.Test.MockData;
using Moq;
using System.Collections.ObjectModel;
using System.Reflection;

namespace Extract.ErrorsAndAlerts.AlertManager.Test
{
    [TestFixture]
    public class EventsOverallViewModelUnitTests
    {
        EventsOverallViewModel testWindow;
        MockDBService dbService;


        [SetUp]
        public void Init()
        {
            dbService = new MockDBService();
            EventObject eventObject = new();

            testWindow = new EventsOverallViewModel(dbService, eventObject);
            //todo set up dbadmin and hack it so it passes specific values...
        }


        //todo note to self, can use many different ways, can use 
        //this is where unique test cases are added, can have multiple
        public static IEnumerable<object> Source()
        {

            yield return new object();
        }

        #region Constructor Testing
        [Test]
        [TestCaseSource(nameof(EventsSource))]
        public void TestConstructorInits(EventObject eventInit)
        {
            dbService.AddEventObjects(eventInit);

            testWindow = new(dbService, eventInit);

            Assert.Multiple(() =>
            {
                Assert.That(eventInit.eliCode, Is.EqualTo(testWindow.GetEvent.eliCode)); 
                Assert.That(testWindow.GetService, Is.Not.Null);
                Assert.That(dbService, Is.EqualTo(testWindow.GetService));
                Assert.That(dbService.ReturnFromDatabase(eventInit.number_Debug).id_Number, Is.EqualTo(testWindow.IdNumber));
                Assert.That(dbService.AllIssueIds(), Is.EqualTo(testWindow.ButtonIds));
            });
        }

        [Test]
        [TestCaseSource(nameof(EventsSource))]
        public void TestNullConstructors(EventObject eventObj)
        {
            testWindow = new(null, eventObj);

            Assert.Multiple(() =>
            {
                Assert.That(testWindow.GetService, Is.Not.Null);
                Assert.That(testWindow.GetEvent.additional_Details, Is.EqualTo(eventObj.additional_Details));
            });
        }

        #endregion Constructor Testing

        [Test]
        //Primarly tests robustness
        //tests the private method SetNewValues as well
        [TestCaseSource(nameof(EventsSourceList))]
        public void TestChangeInterfaceElement(List<EventObject> eventObj)
        {
            dbService.AddEventObjects(eventObj);
            testWindow = new(dbService, new());

            Assert.Multiple(() =>
            {
                Assert.That(testWindow.GetService, Is.Not.Null);
                Assert.DoesNotThrow(() => { testWindow.ChangeInterfaceElements(-1); });
                Assert.DoesNotThrow(() => { testWindow.ChangeInterfaceElements(eventObj.Count+1); });

                if(eventObj.Count > 1)
                {
                    testWindow.ChangeInterfaceElements(0);
                    Assert.That(testWindow.IdNumber, Is.EqualTo(dbService.ReturnFromDatabase(0).id_Number));
                    Assert.That(testWindow.DateErrorCreated, Is.EqualTo(dbService.ReturnFromDatabase(0).date_Error_Created));
                }
            });
        }

        [Test]
        [TestCaseSource(nameof(EventsSource))]
        [Ignore("Future test, not yet testing dialogue")]
        public void TestMakeAlertDialogue(EventObject eventObject)
        {
            
        }

        //not yet implimented in code, future tests
        [Test]
        [Ignore("Future test, not yet implimented in project")]
        public void TestPreviousItem()
        {

        }

        [Test]
        [Ignore("Future test, not yet implimented in project")]
        public void TestNextItem()
        {

        }

        [Test]
        [Ignore("Future test, not yet implimented in project")]
        public void TestIgnoreItem()
        {

        }

        [Test]
        [Ignore("Future test, not yet implimented in project")]
        public void TestDataRetrivalStackTrace()
        {

        }

        #region Sources

        public static IEnumerable<AlertsObject> AlertsSource()
        {
            yield return new();
        }

        public static IEnumerable<EventObject> EventsSource()
        {
            yield return new();
        }

        public static IEnumerable<List<EventObject>> EventsSourceList()
        {
            yield return new();
            //todo add new list of stuff
        }
        #endregion Sources
    }
}
