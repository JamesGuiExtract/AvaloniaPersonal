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
using Extract.ErrorHandling;

namespace Extract.ErrorsAndAlerts.AlertManager.Test
{
    [TestFixture]
    public class ConfigureAlertsViewModelUnitTests
    {

        [SetUp]
        public void Init()
        {
            
        }

        [Test]
        public void TestConstructorValues()
        {
            Mock<IDBService> dbService = new Mock<IDBService>();
            Mock<ConfigureAlertsViewModel> testWindow = new Mock<ConfigureAlertsViewModel>(dbService.Object);

            Assert.That(dbService.Object, Is.EqualTo(testWindow.Object.GetService));
        }

        #region Sources
        
        #endregion Sources
    }
}
