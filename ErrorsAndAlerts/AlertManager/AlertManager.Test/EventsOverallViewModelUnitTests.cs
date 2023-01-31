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
        [Ignore("Ignoring until we complete https://extract.atlassian.net/browse/ISSUE-18936")]
        public void TestConstructorInits(ExceptionEvent eventInit)
        {
            Mock<IDBService> dbService = new Mock<IDBService>();


            List<ExceptionEvent> events = new();
            events.Add(eventInit);
            dbService.Setup(m => m.ReadEvents()).Returns(events);

            dbService.Setup(m => m.ReturnFromDatabase(0)).Returns(new DataNeededForPage());

            Mock<EventsOverallViewModel> testWindow = new(eventInit);

            Assert.Multiple(() =>
            {
                Assert.That(dbService.Object, Is.EqualTo(testWindow.Object.GetService));
                Assert.That(eventInit.ELICode, Is.EqualTo(testWindow.Object.GetEvent.ELICode));
                Assert.That(testWindow.Object.GetService, Is.Not.Null);
            });
        }

        [Test]
        public void TestNullConstructors()
        {
            Mock<EventsOverallViewModel> testWindow = new Mock<EventsOverallViewModel>(null, null);

            Assert.Throws<ExtractException>(() => { EventsOverallViewModel testWindow = new(); });
        }

        #endregion Constructor Testing



        #region Sources

        public static IEnumerable<AlertsObject> AlertsSource()
        {
            yield return new();
        }

        public static IEnumerable<ExceptionEvent> EventsSource()
        {
            yield return new();
        }

        public static IEnumerable<List<ExceptionEvent>> EventsSourceList()
        {
            yield return new();
            //todo add new list of stuff
        }
        #endregion Sources
    }
}
