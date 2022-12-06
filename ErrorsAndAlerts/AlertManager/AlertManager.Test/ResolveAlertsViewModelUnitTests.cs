using AlertManager;
using AlertManager.Interfaces;
using AlertManager.Models.AllDataClasses;
using AlertManager.Services;
using AlertManager.ViewModels;
using Extract.ErrorsAndAlerts.AlertManager.Test.MockData;

namespace Extract.ErrorsAndAlerts.AlertManager.Test
{
    [TestFixture]
    public class ResolveAlertsViewModelUnitTests
    {
        MockDBService dbService;

        ResolveAlertsViewModel testWindow;

        [SetUp]
        public void Init()
        {
            dbService = new MockDBService();
            AlertsObject alertObjectToDisplay = new();
            IAlertResolutionLogger? alertResolutionLogger = new AlertResolutionLogger(); 
            testWindow = new(alertObjectToDisplay, alertResolutionLogger);
        }


        //currently this is the same as test refresh screen... maybe in the future it will be different
        [Test]
        [TestCaseSource(nameof(AlertsSource))]
        public void TestConstructorInit(AlertsObject alertObject)
        {
            Assert.Ignore("just calls another function that is tested");
        }

        [Test]
        [Ignore("Windowing regestration not created, technical issue")]
        public void TestRefreshScreen([ValueSource(nameof(AlertsSource))] AlertsObject alertObject)
        {
            testWindow = new ResolveAlertsViewModel(alertObject);
            testWindow.RefreshScreen(alertObject);

            Assert.Multiple(() =>
            {
                Assert.That(testWindow.ThisObject, Is.EqualTo(alertObject));
                Assert.That(testWindow.ActionType, Is.EqualTo(alertObject.ActionType));
                Assert.That(testWindow.MachineFoundError, Is.EqualTo(alertObject.MachineFoundError));
            });
        }
        //maybe test refresh screen new?

        //tests for future implimentation
        [Test]
        public void TestResolveIssue()
        {

        }

        [Test]
        [Ignore("Future test, not yet implimented in project")]
        public void TestJiraAccess()
        {
            //todo impliment once feature implimented
        }

        [Test]
        [Ignore("Future test, not yet implimented in project")]
        public void TestJiraChange()
        {
        }

        [Test]
        [Ignore("Future test, not yet implimented in project")]
        public void TestBindingAccess()
        {
            //todo impliment once feature implimented

        }

        #region Sources

        public static IEnumerable<AlertsObject> AlertsSource()
        {
            yield return new();
        }

        #endregion Sources
    }
}
