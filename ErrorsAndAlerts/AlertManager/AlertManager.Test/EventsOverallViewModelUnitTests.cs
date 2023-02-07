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
        public void TestConstructorInits([ValueSource(nameof(EventsSource))] ExceptionEvent eventObject)
        {

            Mock<ILoggingTarget> elasticBackend = new Mock<ILoggingTarget>();

            List<ExceptionEvent> events = new();
            events.Add(eventObject);

            elasticBackend.Setup(m => m.GetAllEvents(0)).Returns(events);
            elasticBackend.Setup(m => m.GetAllAlerts(0)).Returns( new List<AlertsObject>() );
            elasticBackend.Setup(m => m.GetMaxEventPages()).Returns(1);

            Mock<EventsOverallViewModel> testWindow;


            testWindow = new Mock<EventsOverallViewModel>(elasticBackend.Object, eventObject);


            Assert.Multiple(() =>
            {
                Assert.That(testWindow.Object.GetEvent, Is.EqualTo(eventObject));
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
            List<ExceptionEvent> listOfObjects = new();
            listOfObjects.Add(new ExceptionEvent());
            yield return listOfObjects;
        }
        #endregion Sources
    }
}
