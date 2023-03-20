using AlertManager.Models.AllDataClasses;
using AlertManager.Services;
using Extract.ErrorHandling;
using Extract.ErrorsAndAlerts.AlertManager.Test.TestClasses;
using Moq;
using UCLID_FILEPROCESSINGLib;

namespace Extract.ErrorsAndAlerts.AlertManager.Test
{
    [TestFixture]
    public class DBServiceUnitTests
    {
        [SetUp]
        public void Init()
        {
        }

        [Test]
        public void TestSetFileObjects([ValueSource(nameof(DummyDBInfo))] DataValuesForGetAndSetFileStatus dummyInfo,
            [ValueSource(nameof(workflowNames))] string workFlowName)
        {
            Mock<IFileProcessingDB> mockFileProc = new Mock<IFileProcessingDB>();
            Mock<DBService> dbService = new Mock<DBService>(mockFileProc.Object);

            EActionStatus mockReturn;

            mockFileProc.Setup(_ => _.SetStatusForFile(
                dummyInfo.idNumber,
                dummyInfo.actionName,
                dummyInfo.workflowId,
                dummyInfo.actionStatus,
                true,
                true,
                out mockReturn));

            Assert.Multiple(() =>
            {
                dbService.Object.SetFileStatus(
                dummyInfo.idNumber,
                dummyInfo.actionStatus,
                dummyInfo.dataBaseName,
                dummyInfo.server,
                dummyInfo.actionId);
                mockFileProc.Verify(m => m.GetWorkflowNameFromActionID(dummyInfo.actionId), Times.Exactly(1));

            });
        }

        [Test]
        public void TestSetFileInvalidDatabase([ValueSource(nameof(DummyDBInfo))] DataValuesForGetAndSetFileStatus dummyInfo,
            [ValueSource(nameof(workflowNames))] string workFlowName) //unexpected behavior should throw error instead of returning a empty value
        {
            Mock<IFileProcessingDB> mockFileProc = new Mock<IFileProcessingDB>();

            mockFileProc.SetupSet(m => m.DatabaseName = dummyInfo.dataBaseName).Throws<ExtractException>();

            Mock<DBService> dbService = new Mock<DBService>(mockFileProc.Object);
            Assert.Multiple(() =>
            {
                //todo assert throws once global error handling is set up
                Assert.Throws<ExtractException>(() => dbService.Object.SetFileStatus(
                dummyInfo.idNumber,
                dummyInfo.actionStatus,
                dummyInfo.dataBaseName,
                dummyInfo.server,
                dummyInfo.actionId)
                );

            });

        }

        [Test]

        public void TestGetFileObjects([ValueSource(nameof(DummyDBInfo))] DataValuesForGetAndSetFileStatus dummyInfo,
            [ValueSource(nameof(DummyFileObjects))] List<FileObject> fileObject)
        {
            Mock<IFileProcessingDB> mockFileProc = new Mock<IFileProcessingDB>();
            Mock<DBService> dbService = new Mock<DBService>(mockFileProc.Object);


            List<int> listOfFileIds = new();
            foreach (FileObject file in fileObject)
            {
                listOfFileIds.Add(file.FileId);
            }


            mockFileProc.Setup(m => m.GetActionName(dummyInfo.actionId)).Returns(dummyInfo.actionName);

            mockFileProc.Setup(_ => _.GetFileStatus(dummyInfo.idNumber, dummyInfo.actionName, true))
                .Returns(fileObject[0].FileStatus);

            mockFileProc.Setup(m => m.GetWorkflowNameFromActionID(
                dummyInfo.idNumber)).Returns(dummyInfo.workFlowName);

            mockFileProc.Setup(m => m.GetFileNameFromFileID(fileObject[0].FileId))
                .Returns(fileObject[0].FileName);

            mockFileProc.Setup(m => m.GetWorkflowID(
                dummyInfo.workFlowName)).Returns(dummyInfo.workflowId);


            mockFileProc.SetupSet(m => m.DatabaseName = dummyInfo.dataBaseName).Verifiable();
            mockFileProc.SetupSet(m => m.DatabaseServer = dummyInfo.server).Verifiable();
            mockFileProc.SetupSet(m => m.ActiveWorkflow = dummyInfo.workFlowName).Verifiable();

            Assert.Multiple(() =>
            {
                Assert.That(dbService.Object.GetFileObjects(listOfFileIds,
                    dummyInfo.dataBaseName,
                    dummyInfo.server,
                    dummyInfo.actionId
                )[0].FileId, Is.EqualTo(fileObject[0].FileId));

                Assert.That(dbService.Object.GetFileObjects(listOfFileIds,
                    dummyInfo.dataBaseName,
                    dummyInfo.server,
                    dummyInfo.actionId
                )[0].FileName, Is.EqualTo(fileObject[0].FileName));

                Assert.That(dbService.Object.GetFileObjects(listOfFileIds,
                    dummyInfo.dataBaseName,
                    dummyInfo.server,
                    dummyInfo.actionId
                )[0].FileStatus, Is.EqualTo(fileObject[0].FileStatus));

            });
        }

        [Test]
        public void TestGetFileObjectsInvalidDatabase([ValueSource(nameof(DummyDBInfo))] DataValuesForGetAndSetFileStatus dummyInfo)
        {
            Mock<IFileProcessingDB> mockFileProc = new Mock<IFileProcessingDB>();
            Mock<DBService> dbService = new Mock<DBService>(mockFileProc.Object);

            mockFileProc.SetupSet(m => m.DatabaseName = dummyInfo.dataBaseName).Throws<ExtractException>();

            List<int> listOfFiles = new();
            listOfFiles.Add(1);

            Assert.Throws<ExtractException>(() => { dbService.Object.GetFileObjects(listOfFiles,
                dummyInfo.dataBaseName,
                dummyInfo.server,
                dummyInfo.actionId); });
        }

        [Test]
        public void TestConstructor()
        {
            Mock<IFileProcessingDB> mockFileProc = new Mock<IFileProcessingDB>();
            Mock<DBService> dbService = new Mock<DBService>(mockFileProc.Object);

            Assert.That(dbService.Object.GetFileProcessingDB, Is.EqualTo(mockFileProc.Object));
        }

        [Test]
        public void TestConstructorNull()
        {
            Mock<DBService> db = new Mock<DBService>(null);

            Assert.NotNull(db.Object.GetFileProcessingDB);
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

        //README: note right now testing isn't set up for multiple list objects
        public static IEnumerable<List<FileObject>> DummyFileObjects()
        {
            string fileName = "somename";
            EActionStatus fileStatus = EActionStatus.kActionCompleted;
            int fileId = 0;
            yield return new List<FileObject>() {
                new FileObject(fileName, fileStatus, fileId)
            };
        }

        public static IEnumerable<string> workflowNames()
        {
            yield return "Workflow1";
        }

        public static IEnumerable<int> workflowIdString()
        {
            yield return 2;
        }
    }
    
}
