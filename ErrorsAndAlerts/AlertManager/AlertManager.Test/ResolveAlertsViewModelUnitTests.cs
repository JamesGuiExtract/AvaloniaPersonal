using AlertManager;
using AlertManager.Interfaces;
using AlertManager.Models.AllDataClasses;
using AlertManager.Services;
using AlertManager.ViewModels;
using Extract.ErrorsAndAlerts.AlertManager.Test.MockData;
using Moq;

namespace Extract.ErrorsAndAlerts.AlertManager.Test
{
    [TestFixture]
    public class ResolveAlertsViewModelUnitTests
    {


        [SetUp]
        public void Init()
        {
            
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
            Mock<IDBService> dbService = new();
            Mock<ILoggingTarget> alertObjectToDisplay = new();
            Mock<IAlertResolutionLogger> alertResolutionLogger = new();
            Mock<ResolveAlertsViewModel> testWindow = new(alertObjectToDisplay.Object, alertResolutionLogger.Object);


            testWindow = new(alertObject); //double check this works
            testWindow.Object.RefreshScreen(alertObject);

            Assert.Multiple(() =>
            {
                Assert.That(testWindow.Object.ThisObject, Is.EqualTo(alertObject));
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
