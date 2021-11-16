using Extract.Testing.Utilities;
using Moq;
using NUnit.Framework;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using UCLID_FILEPROCESSINGLib;
using WebAPI;
using WebAPI.Models;

namespace Extract.Web.WebAPI.Test
{
    // Unit tests DocumentData interactions with its, mocked, dependencies
    // NOTE: Tests in this class must be run serially unless [FixtureLifeCycle(LifeCycle.InstancePerTestCase)] is used (upgrade nunit)
    [TestFixture]
    [Category("Automated")]
    [Category("DocumentDataService")]
    public class TestDocumentData
    {
        int _fileID;
        int _wfID;
        Mock<FileProcessingDB> _databaseMock;
        DocumentData _documentDataService;

        [OneTimeSetUp]
        public static void Setup()
        {
            GeneralMethods.TestSetup();
        }

        // Per-test setup
        // Setup DocumentData and its dependencies
        [SetUp]
        public void Init()
        {
            _databaseMock = new();

            _wfID = new Random().Next(1, int.MaxValue);
            WorkflowDefinitionClass wd = new()
            {
                ID = _wfID,
                Name = "FWF",
                Type = EWorkflowType.kRedaction,
                Description = "Fake Workflow",
                StartAction = "Start",
                EditAction = "Edit",
                PostEditAction = "PostEdit",
                EndAction = "End",
                PostWorkflowAction = "PostWF",
                DocumentFolder = "Docs",
                OutputAttributeSet = "Attrr",
                OutputFileMetadataField = "FileName"
            };

            Workflow workflow = new(wd, "Server", "DB");

            _fileID = new Random().Next(1, int.MaxValue);

            Mock<IFileApi> _fileApiMock = new();
            _fileApiMock.SetupGet(x => x.FileProcessingDB).Returns(_databaseMock.Object);
            _fileApiMock.SetupGet(x => x.DocumentSession).Returns((true, default, _fileID, default));
            _fileApiMock.SetupGet(x => x.Workflow).Returns(workflow);

            Mock<IFileApiMgr> _fileApiMgrMock = new();
            _fileApiMgrMock.Setup(x => x.GetInterface(It.IsAny<ApiContext>(), It.IsAny<ClaimsPrincipal>())).Returns(_fileApiMock.Object);

            Mock<ClaimsPrincipal> _userMock = new();
            _userMock.SetupGet(x => x.Claims).Returns(
                new[]
                {
                    new Claim(Utils._WORKFLOW_NAME, "WF"),
                    new Claim(JwtRegisteredClaimNames.Jti, "0"),
                    new Claim(Utils._FAM_SESSION_ID, "0")
                });

            ApiContext _apiContext = new("0", "Server", "DB", "WF");
            Utils.SetCurrentApiContext(_apiContext);

            _documentDataService = new(_userMock.Object, true, _fileApiMgrMock.Object);
        }

        // Confirm that DocumentData.CloseDocument makes the expected FileProcessingDB method calls
        [Test]
        [Pairwise]
        public void Test_CloseDocument(
            [Values]EActionStatus setStatusTo,
            [Values(int.MinValue, -1, 42, int.MaxValue)] int activityTime,
            [Values(int.MinValue, -1, 42, int.MaxValue)] int overheadTime,
            [Values] bool closedBecauseOfInactivityTimeout,
            [Values] bool generateException)
        {
            // Arrange
            ExtractException exn = generateException ? new ExtractException("ELILO", "Nunit Message") : null;
            int overheadTimeInSeconds = Math.Max(0, overheadTime) / 1000;
            int activityTimeInSeconds = Math.Max(0, activityTime) / 1000;

            // Act
            _documentDataService.CloseDocument(setStatusTo, exn, activityTime, overheadTime, closedBecauseOfInactivityTimeout);

            // Assert end file task session call
            _databaseMock.Verify(x => x.EndFileTaskSession(
                It.IsAny<int>(),
                overheadTimeInSeconds,
                activityTimeInSeconds,
                closedBecauseOfInactivityTimeout), Times.Once());

            // Assert file status calls
            switch (setStatusTo)
            {
                case EActionStatus.kActionCompleted:
                    _databaseMock.Verify(x => x.NotifyFileProcessed(_fileID, "Edit", -1, true), Times.Once());
                    _databaseMock.Verify(x => x.SetStatusForFile(
                        _fileID, "PostEdit", _wfID, EActionStatus.kActionPending, true, false, out It.Ref<EActionStatus>.IsAny), Times.Once());
                    break;
                case EActionStatus.kActionSkipped:
                    _databaseMock.Verify(x => x.NotifyFileSkipped(_fileID, "Edit", -1, true), Times.Once());
                    break;
                case EActionStatus.kActionFailed:
                    string expectedException = exn?.AsStringizedByteStream();
                    _databaseMock.Verify(x => x.NotifyFileFailed(_fileID, "Edit", -1, expectedException, true), Times.Once());
                    break;
                default:
                    _databaseMock.Verify(x => x.SetStatusForFile(
                        _fileID, "Edit", -1, setStatusTo, false, true, out It.Ref<EActionStatus>.IsAny), Times.Once());
                    break;
            }

            // Assert no other methods called on the database
            _databaseMock.VerifyNoOtherCalls();
        }
    }
}
