using AlertManager;
using AlertManager.Interfaces;
using AlertManager.Models.AllDataClasses;
using AlertManager.Services;
using AlertManager.ViewModels;
using AlertManager.Views;
using DynamicData;
using Extract.ErrorHandling;
using Extract.ErrorsAndAlerts.AlertManager.Test.MockData;
using Moq;
using NUnit.Framework.Internal;
using System.Collections.ObjectModel;
using System.Reflection;

namespace Extract.ErrorsAndAlerts.AlertManager.Test
{
    [TestFixture]
    public class EventsOverallViewModelUnitTests
    {

        [SetUp]
        public void Init()
        {
            
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
            Mock<IDBService> dbService = new Mock<IDBService>();
            

            List<EventObject> events = new();
            events.Add(eventInit);
            dbService.Setup(m => m.ReadEvents()).Returns(events);

            dbService.Setup(m => m.ReturnFromDatabase(0)).Returns(new DataNeededForPage());
            
            Mock<EventsOverallViewModel> testWindow = new(dbService.Object, eventInit);

            Assert.Multiple(() =>
            {
                Assert.That(dbService.Object, Is.EqualTo(testWindow.Object.GetService));
                Assert.That(eventInit.eliCode, Is.EqualTo(testWindow.Object.GetEvent.eliCode)); 
                Assert.That(testWindow.Object.GetService, Is.Not.Null);
                Assert.That(dbService.Object.ReturnFromDatabase(eventInit.number_Debug).id_Number, Is.EqualTo(testWindow.Object.IdNumber));
                Assert.That(dbService.Object.AllIssueIds(), Is.EqualTo(testWindow.Object.ButtonIds));
            });
        }

        [Test]
        public void TestNullConstructors()
        {
            Mock<EventsOverallViewModel> testWindow = new Mock<EventsOverallViewModel>(null, null);

            Assert.Throws<ExtractException>(() => { EventsOverallViewModel testWindow = new(); });
        }

        #endregion Constructor Testing

        [Test]
        //Primarly tests robustness
        //tests the private method SetNewValues as well
        [TestCaseSource(nameof(EventsSourceList))]
        public void TestChangeInterfaceElement(List<EventObject> eventObj)
        {
            Mock<IDBService> dbService = new Mock<IDBService>();


            dbService.Setup(m => m.ReadEvents()).Returns(eventObj);

            dbService.Setup(m => m.ReturnFromDatabase(0)).Returns(new DataNeededForPage());

            Mock<EventsOverallViewModel> testWindow;
            if (eventObj.Count > 0)
            {
                testWindow = new(dbService.Object, eventObj[0]);
            }
            else
            {
                testWindow = new(dbService.Object, new EventObject());
            }

            Assert.Multiple(() =>
            {
                Assert.That(testWindow.Object.GetService, Is.Not.Null);
                Assert.Throws<ExtractException>(() => { testWindow.Object.ChangeInterfaceElements(-1); });
                Assert.Throws<ExtractException>(() => { testWindow.Object.ChangeInterfaceElements(eventObj.Count+1); });

                if(eventObj.Count > 1)
                {
                    testWindow.Object.ChangeInterfaceElements(0);
                    Assert.That(testWindow.Object.IdNumber, Is.EqualTo(dbService.Object.ReturnFromDatabase(0).id_Number));
                    Assert.That(testWindow.Object.DateErrorCreated, Is.EqualTo(dbService.Object.ReturnFromDatabase(0).date_Error_Created));
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
