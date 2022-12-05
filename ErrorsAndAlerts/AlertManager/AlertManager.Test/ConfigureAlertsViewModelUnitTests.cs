using Avalonia.Input;
using AlertManager;
using AlertManager.Interfaces;
using AlertManager.Models.AllDataClasses;
using AlertManager.Services;
using AlertManager.ViewModels;
using DynamicData;
using Extract.ErrorsAndAlerts.AlertManager.Test.MockData;
using System.Collections.ObjectModel;
using System.Diagnostics.Tracing;
using System.Net.Mail;
using System.Runtime.CompilerServices;
using Moq;

namespace Extract.ErrorsAndAlerts.AlertManager.Test
{
    [TestFixture]
    public class ConfigureAlertsViewModelUnitTests
    {
        //implmented mock services
        Mock<IDBService> _dbServiceMock;
        Mock<ConfigureAlertsViewModel> _configureAlertsViewModelMock;


        MockDBService dbService;
        ConfigureAlertsViewModel testWindow;

        [SetUp]
        public void Init()
        {
            _dbServiceMock = new Mock<IDBService>();
            _configureAlertsViewModelMock = new Mock<ConfigureAlertsViewModel>();   

            dbService = new MockDBService();
            testWindow = new ConfigureAlertsViewModel();
        }

        [Test]
        public void TestConstructorValues()
        {
            testWindow = new(dbService);
            Assert.That(dbService, Is.EqualTo(testWindow.GetService));
        }

        [Test]
        public void TestConstructorNull()
        {

            testWindow = new(null);

            Assert.Multiple(() =>
            {
                Assert.That(testWindow.GetService, Is.Not.Null);
                Assert.That(new DBService().GetDocumentTotal(), Is.EqualTo(testWindow.GetService?.GetDocumentTotal()));
            });
            
        }

        //not yet implimented
        [Test]
        [Ignore("Future test, not yet implimented in project")]
        public void TestCreateConfig()
        {
        }

        //not yet implimented
        [Test]
        [Ignore("Future test, not yet implimented in project")]
        public void TestRefreshScreen()
        {
        }

        [Test]
        [Ignore("Future test, not yet implimented in project")]
        public void TestCurrentAlertConfigRetrival()
        {

        }

        [Test]
        [Ignore("Future test, not yet implimented in project")]
        public void TestCurrentAlertNullRetrival()
        {
        }


        #region Sources
        //todo note to self, can use many different ways, can use 
        //this is where unique test cases are added, can have multiple

        //need a configure source
        #endregion Sources
    }
}
