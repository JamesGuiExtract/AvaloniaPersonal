﻿using AlertManager.Interfaces;
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
        [Ignore("This is not testing our code at all")]
        public void TestConstructor([ValueSource(nameof(AlertsSource))] AlertsObject alert, 
            [ValueSource(nameof(DummyDBInfo))] DataValuesForGetAndSetFileStatus dummyInfo,
            [ValueSource(nameof(DummyFileObjects))] List<FileObject> fileObject)
        {


            Mock<IDBService> dbService = new Mock<IDBService>();

            List<int> listOfFileIds = new List<int> { 1, 2, 3 };

            dbService.Setup(m => m.GetFileObjects(listOfFileIds,
                    dummyInfo.dataBaseName,
                    dummyInfo.server,
                    dummyInfo.actionId)).Returns(fileObject);

            dbService.Setup(m => m.SetFileStatus(
                    dummyInfo.idNumber,
                    dummyInfo.actionStatus,
                    dummyInfo.dataBaseName,
                    dummyInfo.server,
                    dummyInfo.actionId))
                .Returns(true);

            Mock<AssociatedFilesViewModel> resolveFiles = new(alert, dbService.Object);

            Assert.Multiple(() =>
            {
                Assert.That(resolveFiles.Object.ThisAlert, Is.EqualTo(alert));
                //Assert.That(resolveFiles.Object._dbService, Is.EqualTo(dbService.Object));
            });
        }

        [Test]
        //readme, this is very specific to current setup, when the code is changed to be more flexible, change this as well
        public void TestConstructorInvalidInputs([ValueSource(nameof(AlertsSource))] AlertsObject alert)
        {
            Mock<AssociatedFilesViewModel> resolveFiles = new(alert, null);

            Assert.Multiple(() =>
            {
                Assert.That(resolveFiles.Object.ThisAlert, Is.EqualTo(alert));
            });
        }

        [Test]
        [Ignore("At the moment isn't implimented")]
        public void TestSetupDBInformation()
        {

        }

        [Test]
        public void TestGetFilesFromEvents([ValueSource(nameof(AlertsSource))] AlertsObject alert,
            [ValueSource(nameof(DummyDBInfo))] DataValuesForGetAndSetFileStatus dummyInfo,
            [ValueSource(nameof(DummyFileObjects))] List<FileObject> fileObject)
        {

            Mock<IDBService> dbService = new Mock<IDBService>();

            List<int> listOfFileIds = new List<int> { 1, 2, 3 };

            //this is needed because constructor crashes without this
            dbService.Setup(m => m.GetFileObjects(listOfFileIds,
                    dummyInfo.dataBaseName,
                    dummyInfo.server,
                    dummyInfo.actionId)).Returns(fileObject);

            dbService.Setup(m => m.SetFileStatus(
                    dummyInfo.idNumber,
                    dummyInfo.actionStatus,
                    dummyInfo.dataBaseName,
                    dummyInfo.server,
                    dummyInfo.actionId))
                .Returns(true);

            Mock<AssociatedFilesViewModel> resolveFiles = new(alert, dbService.Object);

            Assert.Multiple(() =>
            {
                resolveFiles.Object.GetFilesFromEvents();
            });
        }

        [Test]
        //sorta covered by get files from events but more specific, get file from events could have other issues
        public void TestGetFilesFromDB([ValueSource(nameof(AlertsSource))] AlertsObject alert,
            [ValueSource(nameof(DummyDBInfo))] DataValuesForGetAndSetFileStatus dummyInfo,
            [ValueSource(nameof(DummyFileObjects))] List<FileObject> fileObject)
        {
            Mock<IDBService> dbService = new Mock<IDBService>();

            List<int> listOfFileIds = new List<int> { 1, 2, 3 };

            //this is needed because constructor crashes without this lol
            dbService.Setup(m => m.GetFileObjects(listOfFileIds,
                    dummyInfo.dataBaseName,
                    dummyInfo.server,
                    dummyInfo.actionId)).Returns(fileObject);

            dbService.Setup(m => m.SetFileStatus(
                    dummyInfo.idNumber,
                    dummyInfo.actionStatus,
                    dummyInfo.dataBaseName,
                    dummyInfo.server,
                    dummyInfo.actionId))
                .Returns(true);

            Mock<AssociatedFilesViewModel> resolveFiles = new(alert, dbService.Object);


            Assert.Multiple(() =>
            {
                resolveFiles.Object.GetFilesFromDBImpl(listOfFileIds);
                for (int i = 0; i < fileObject.Count; i++)
                {
                  Assert.That(resolveFiles.Object.ListOfFiles[i].FileObject, Is.EqualTo(fileObject[i]));
                }
            });

        }

        //todo set up more functionality in the future
        [Test]
        public void TestSetFileStatus([ValueSource(nameof(AlertsSource))] AlertsObject alert,
            [ValueSource(nameof(DummyDBInfo))] DataValuesForGetAndSetFileStatus dummyInfo,
            [ValueSource(nameof(DummyFileObjects))] List<FileObject> fileObject)
        {
            Mock<IDBService> dbService = new Mock<IDBService>();

            List<int> listOfFileIds = new List<int> { 1, 2, 3 };

            //this is needed because constructor crashes without this lol
            dbService.Setup(m => m.GetFileObjects(listOfFileIds,
                    dummyInfo.dataBaseName,
                    dummyInfo.server,
                    dummyInfo.actionId)).Returns(fileObject);

            dbService.Setup(m => m.SetFileStatus(
                    dummyInfo.idNumber,
                    dummyInfo.actionStatus,
                    dummyInfo.dataBaseName,
                    dummyInfo.server,
                    dummyInfo.actionId))
                .Returns(true);

            Mock<AssociatedFilesViewModel> resolveFiles = new(alert, dbService.Object);

            Assert.Multiple(() =>
            {
                resolveFiles.Object.SetFileStatusImpl();
                for (int i = 0; i < fileObject.Count; i++)
                {
                    Assert.That(resolveFiles.Object.ListOfFiles[i].FileObject, Is.EqualTo(fileObject[i]));
                }
            });
        }

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
    }
}
