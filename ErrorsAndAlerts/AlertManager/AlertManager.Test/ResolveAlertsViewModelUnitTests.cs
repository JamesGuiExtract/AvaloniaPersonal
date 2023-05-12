using AlertManager;
using AlertManager.Interfaces;
using AlertManager.Models.AllDataClasses;
using AlertManager.Services;
using AlertManager.ViewModels;
using Extract.ErrorsAndAlerts.AlertManager.Test.MockData;
using Moq;
using UCLID_FILEPROCESSINGLib;

namespace Extract.ErrorsAndAlerts.AlertManager.Test
{
    [TestFixture]
    public class ResolveAlertsViewModelUnitTests
    {


        [SetUp]
        public void Init()
        {
            
        }

        [Test]
        public void TestRefreshScreen([ValueSource(nameof(AlertsSource))] AlertsObject alertObject,
            [ValueSource(nameof(DummyFileObjects))] List<FileObject> fileObjects)
        {
            Mock<IDBService> mockDB = new();
            Mock<IElasticSearchLayer> mockElastic = new();
            Mock<IAlertActionLogger> mockActionLogger = new();

            mockDB.Setup(m => m.GetFileObjects(
                It.IsAny<IList<int>>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int>()))
            .Returns(fileObjects);

            AlertActionsViewModel sut = new(alertObject, mockActionLogger.Object, mockElastic.Object, mockDB.Object);

            Assert.Multiple(() =>
            {
                Assert.That(sut.ThisAlert, Is.EqualTo(alertObject));
            });
        }
        
        #region Sources

        public static IEnumerable<AlertsObject> AlertsSource()
        {
            yield return new();
        }

        public static IEnumerable<List<FileObject>> DummyFileObjects()
        {
            string fileName = "somename";
            EActionStatus fileStatus = EActionStatus.kActionCompleted;
            int fileId = 0;
            yield return new List<FileObject>() {
                new FileObject(fileName, fileStatus, fileId)
            };
        }

        #endregion Sources
    }
}
