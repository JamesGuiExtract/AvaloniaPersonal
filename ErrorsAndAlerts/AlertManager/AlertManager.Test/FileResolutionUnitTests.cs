using AlertManager.Interfaces;
using AlertManager.Models.AllDataClasses;
using AlertManager.Models.AllEnums;
using AlertManager.ViewModels;
using Extract.ErrorHandling;
using Extract.ErrorsAndAlerts.AlertManager.Test.TestClasses;
using Extract.ErrorsAndAlerts.ElasticDTOs;
using Moq;
using UCLID_FILEPROCESSINGLib;

namespace Extract.ErrorsAndAlerts.AlertManager.Test
{
    [TestFixture]
    public class FileResolutionUnitTests
    { 
        [Test]
        public void TestConstructor([ValueSource(nameof(AlertsSource))] AlertsObject alert,
            [ValueSource(nameof(DummyFileObjects))] List<FileObject> fileObjects)
        {
            Mock<IDBService> dbService = new Mock<IDBService>();

            List<int> listOfFileIds = new List<int> { 1, 2, 3 };

            dbService.Setup(m => m.GetFileObjects(
                It.IsAny<IList<int>>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int>()))
            .Returns(fileObjects);

            dbService.Setup(m => m.SetFileStatus(
                    It.IsAny<int>(),
                    It.IsAny<EActionStatus>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<int>()))
                .Returns(true);

            AssociatedFilesViewModel sut = new(alert, dbService.Object);

            Assert.Multiple(() =>
            {
                Assert.That(sut.ThisAlert, Is.EqualTo(alert));
            });
        }

        [Test]
        public void TestConstructorInvalidInputs([ValueSource(nameof(AlertsSource))] AlertsObject alert)
        {
            AssociatedFilesViewModel sut;

            Assert.Throws<ArgumentNullException>(
                delegate { sut = new(alert, null); });
        }

        [Test]
        public void TestGetFilesFromEvents([ValueSource(nameof(AlertsSource))] AlertsObject alert,
            [ValueSource(nameof(DummyFileObjects))] List<FileObject> fileObjects)
        {

            Mock<IDBService> dbService = new Mock<IDBService>();

            List<int> listOfFileIds = new List<int> { 1, 2, 3 };

            dbService.Setup(m => m.GetFileObjects(
                It.IsAny<IList<int>>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int>()))
            .Returns(fileObjects);

            dbService.Setup(m => m.SetFileStatus(
                    It.IsAny<int>(),
                    It.IsAny<EActionStatus>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<int>()))
                .Returns(true);

            AssociatedFilesViewModel sut = new(alert, dbService.Object);

            sut.GetFilesFromEvents();
        }

        [Test]
        public void TestGetFilesFromDB([ValueSource(nameof(AlertsSource))] AlertsObject alert,
            [ValueSource(nameof(DummyFileObjects))] List<FileObject> fileObjects)
        {
            Mock<IDBService> dbService = new Mock<IDBService>();

            List<int> listOfFileIds = new List<int> { 1, 2, 3 };

            dbService.Setup(m => m.GetFileObjects(
                It.IsAny<IList<int>>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int>()))
            .Returns(fileObjects);

            AssociatedFilesViewModel sut = new(alert, dbService.Object);

            sut.GetFilesFromDB(listOfFileIds);

            Assert.Multiple(() =>
            {
                sut.GetFilesFromDB(listOfFileIds);
                for (int i = 0; i < fileObjects.Count; i++)
                {
                  Assert.That(sut.ListOfFiles[i].FileObject, Is.EqualTo(fileObjects[i]));
                }
            });
        }

        [Test]
        public void TestSetFileStatus([ValueSource(nameof(AlertsSource))] AlertsObject alert,
            [ValueSource(nameof(DummyFileObjects))] List<FileObject> fileObjects)
        {
            Mock<IDBService> dbService = new Mock<IDBService>();

            List<int> listOfFileIds = new List<int> { 1, 2, 3 };

            dbService.Setup(m => m.GetFileObjects(
                It.IsAny<IList<int>>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int>()))
            .Returns(fileObjects);

            dbService.Setup(m => m.SetFileStatus(
                    It.IsAny<int>(),
                    It.IsAny<EActionStatus>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<int>()))
                .Returns(true);

            AssociatedFilesViewModel sut = new(alert, dbService.Object);

            Assert.Multiple(() =>
            {
                sut.SetFileStatus.Execute();
                for (int i = 0; i < fileObjects.Count; i++)
                {
                    Assert.That(sut.ListOfFiles[i].FileObject, Is.EqualTo(fileObjects[i]));
                }
            });
        }

        #region sources

        public static IEnumerable<AlertsObject> AlertsSource()
        {
            yield return new();
            yield return new AlertsObject(
                alertId: "AlertId",
                hitsType: "TestType2",
                alertName: "TestAlertName",
                configuration: "testconfig2",
                activationTime: new DateTime(2008, 5, 1, 8, 30, 52),
                associatedEvents: Array.Empty<EventDto>(),
                listOfActions: Array.Empty<AlertActionDto>());
        }

        public static IEnumerable<DataValuesForGetAndSetFileStatus> DummyDBInfo()
        {
            DataValuesForGetAndSetFileStatus testValues = new(
                0,
                "testing",
                23,
                EActionStatus.kActionPending,
                0,
                "BlueJay",
                "Testing",
                "workFlow");
            yield return testValues;
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

        #endregion sources
    }
}
