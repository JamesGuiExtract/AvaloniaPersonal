using AlertManager;
using AlertManager.Interfaces;
using AlertManager.Models.AllDataClasses;
using AlertManager.Services;
using AlertManager.ViewModels;
using Extract.ErrorsAndAlerts.AlertManager.Test.MockData;

namespace Extract.ErrorsAndAlerts.AlertManager.Test
{
    [TestFixture]
    public class MakeAlertViewModelUnitTests
    {
        MockDBService dbService;
        MakeAlertViewModel testWindow;

        [SetUp]
        public void Init()
        {
            dbService = new();
            testWindow = new MakeAlertViewModel();
        }

        [TearDown]
        public void Clear()
        {
           
        }
        
        /// <summary>
        /// currently same implimentation as refresh screen, might change in the future so keep test
        /// </summary>
        [Test]
        public void TestConstructor()
        {
            Assert.Ignore();
        }

        [Test]
        public void TestConstructorNull()
        {
            
            Assert.Multiple(() =>
            {
                Assert.DoesNotThrow(() => { testWindow = new MakeAlertViewModel(null, null); });

                testWindow = new MakeAlertViewModel(null, null);

                Assert.That(new DBService().ReadAllAlerts(), Is.EqualTo(testWindow.GetDB.ReadAllAlerts())); 
            });
        }

        //should i test if this window was passed correctly?

        [Test]
        public void TestRefreshScreen([ValueSource(nameof(EventsSource))] EventObject eventObject)
        {
            testWindow.RefreshScreen(eventObject);
            Assert.Multiple(() =>
            {
                //todo change it so it just calls from a eventobject? and see what else is refreshed?
                Assert.That(eventObject, Is.EqualTo(testWindow.ErrorObject));
                Assert.That(eventObject.message, Is.EqualTo(testWindow.Message));
                Assert.That(eventObject.machine_And_Customer_Information, Is.EqualTo(testWindow.MachineAndCustomerInformation));
            });
        }

        [Test]
        [Ignore("test this when alert can be generated")]
        public void TestSendAlertCreation()
        {
            //will need database and test values for this, will need to modify return area of db to work, maybe put in paramater string location?
            
        }

        //todo maybe have a full test to json? but not what is currently going to do

        [Test]
        [Ignore("Future test, not yet implimented in project")]
        public void TestCloseWindow()
        {
            //todo test string return

        }

        //not yet implimented, tests for future impimentations
        [Test]
        [Ignore("Future test, not yet implimented in project")]
        public void TestUserComments()
        {
            //not yet implimented, test in the future

        }

        [Test]
        [Ignore("Future test, not yet implimented in project")]
        public void TestRepeatAlertCreation()
        {
            //not yet implimented, test in the future

        }

        [Test]
        [Ignore("Future test, not yet implimented in project")]
        public void TestJiraAccessAndBind()
        {
            //not yet implimented, test in the future

        }

        #region Sources
        public static IEnumerable<EventObject> EventsSource()
        {
            yield return new();
        }
        #endregion Sources
    }
}
