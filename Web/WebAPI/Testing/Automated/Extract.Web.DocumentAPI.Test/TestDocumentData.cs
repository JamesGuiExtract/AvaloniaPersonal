﻿using Extract.Testing.Utilities;
using Extract.Utilities;
using Extract.Web.ApiConfiguration.Models;
using Extract.Web.ApiConfiguration.Services;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using UCLID_COMUTILSLib;
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

            var documentAPIConfiguration = new DocumentApiConfiguration(
                configurationName: "Example"
                , isDefault: true
                , workflowName: "WF"
                , attributeSet: "Attrr"
                , processingAction: "Edit"
                , postProcessingAction: "PostEdit"
                , documentFolder: ""
                , startAction: "Start"
                , endAction: "End"
                , postWorkflowAction: "PostWF"
                , outputFileNameMetadataField: "FileName"
                , outputFileNameMetadataInitialValueFunction: ""
                );

            _fileID = new Random().Next(1, int.MaxValue);

            Mock<IFileApi> _fileApiMock = new();
            _fileApiMock.SetupGet(x => x.FileProcessingDB).Returns(_databaseMock.Object);
            _fileApiMock.SetupGet(x => x.DocumentSession).Returns((true, default, _fileID, default));
            _fileApiMock.SetupGet(x => x.WebConfiguration).Returns(documentAPIConfiguration);

            _databaseMock.Setup(x => x.GetWorkflowID("WF")).Returns(_wfID);
            var map = new StrToStrMap();
            map.Set(documentAPIConfiguration.ConfigurationName, ApiTestUtils.SerializeWebConfig(documentAPIConfiguration));
            _databaseMock.Setup(x => x.GetWebAPIConfigurations()).Returns(map);

            Mock<IFileApiMgr> _fileApiMgrMock = new();
            _fileApiMgrMock.Setup(x => x.GetInterface(It.IsAny<ApiContext>(), It.IsAny<ClaimsPrincipal>())).Returns(_fileApiMock.Object);

            Mock<ClaimsPrincipal> _userMock = new();
            _userMock.SetupGet(x => x.Claims).Returns(
                new[]
                {
                    new Claim(Utils._WORKFLOW_NAME, "WF"),
                    new Claim(JwtRegisteredClaimNames.Jti, "0"),
                    new Claim(Utils._FAM_SESSION_ID, "0"),
                    new Claim(Utils._CONFIGURATION_NAME, documentAPIConfiguration.ConfigurationName)
                });

            ApiContext _apiContext = new("0", "Server", "DB", documentAPIConfiguration);
            Utils.SetCurrentApiContext(_apiContext);

            Mock<IConfigurationDatabaseService> _configurationDatabaseServiceMock = new();
            _configurationDatabaseServiceMock.SetupGet(x => x.Configurations).Returns(new List<ICommonWebConfiguration>() { documentAPIConfiguration });

            _documentDataService = new DocumentData(_userMock.Object, true, _configurationDatabaseServiceMock.Object, _fileApiMgrMock.Object);
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
                    _databaseMock.Verify(x =>
                        x.NotifyFileFailed(
                            _fileID,
                            "Edit",
                            -1,
                            It.Is<string>(stringizedExn => !generateException || ExtractException.FromStringizedByteStream("ELI0", stringizedExn).Message == exn.Message),
                            true),
                        Times.Once());
                    break;
                default:
                    _databaseMock.Verify(x => x.SetStatusForFile(
                        _fileID, "Edit", -1, setStatusTo, false, true, out It.Ref<EActionStatus>.IsAny), Times.Once());
                    break;
            }
        }

        [Test]
        public void Retry_OpenDocument([Values(10, 11)] int failures, [Values] bool getSkippedFile)
        {
            // Arrange
            int expectedResult = failures > 10 ? -1 : 1;

            // Setup the database so that it returns no files the first n times
            _databaseMock.Setup(x => x.GetFilesToProcessAdvanced(It.IsAny<string>(), It.IsAny<int>(),
                It.IsAny<EQueueType>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Returns(new Queue<IUnknownVector>(
                    Enumerable.Repeat(new IUnknownVectorClass(), failures)
                    .Concat(Enumerable.Repeat(new[] { new FileRecordClass() { FileID = 1 } }.ToIUnknownVector(), 1)))
                .Dequeue);

            // Setup the statistics calls to return 1 pending and 1 skipped so that retries will happen
            int numSkipped = getSkippedFile ? 1 : 0;
            int numPendingForUser =  0;
            int numPendingForAll =  1 ;

            _databaseMock.Setup(x => x.GetVisibleFileStats(It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .Returns(new ActionStatisticsClass() { NumDocumentsPending = numPendingForAll, NumDocumentsSkipped = numSkipped});
            _databaseMock.Setup(x => x.GetFileStatsForUser(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<bool>()))
                .Returns(new ActionStatisticsClass() { NumDocumentsPending = numPendingForUser, NumDocumentsSkipped = numSkipped});
            _databaseMock.Setup(x => x.GetNumberSkippedForUser(It.IsAny<string>(), It.IsAny<int>()))
                .Returns(numSkipped);

            // Prevent null reference
            _databaseMock.Setup(x => x.GetDBInfoSetting("VerificationSessionTimeout", true)).Returns("60");
            _databaseMock.Setup(x => x.GetActiveUsers(It.IsAny<string>())).Returns(new VariantVectorClass());

            // Act
            DocumentIdResult fileResult = _documentDataService.OpenDocument("", -1, getSkippedFile, false, "");

            // Assert
            Assert.AreEqual(expectedResult, fileResult.Id);
        }
    }
}
