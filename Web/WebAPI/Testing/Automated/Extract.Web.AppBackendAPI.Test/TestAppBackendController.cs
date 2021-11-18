using Extract.Testing.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using UCLID_FILEPROCESSINGLib;
using WebAPI;
using WebAPI.Controllers;
using WebAPI.Models;

namespace Extract.Web.WebAPI.Test
{
    // Unit tests AppBackendController interactions with its, mocked, dependencies
    // NOTE: Tests in this class must be run serially unless [FixtureLifeCycle(LifeCycle.InstancePerTestCase)] is used (upgrade nunit)
    [TestFixture]
    [Category("Automated")]
    [Category("AppBackendController")]
    public class TestAppBackendController
    {
        int _fileID;
        Mock<IDocumentData> _documentDataMock;
        AppBackendController _appBackendController;

        [OneTimeSetUp]
        public static void Setup()
        {
            GeneralMethods.TestSetup();
        }

        // Per-test setup
        // Setup AppBackendController and its dependencies
        [SetUp]
        public void Init()
        {
            _fileID = new Random().Next(1, int.MaxValue);

            Mock<ClaimsPrincipal> _userMock = new();
            _userMock.SetupGet(x => x.Claims).Returns(
                new[]
                {
                    new Claim(Utils._WORKFLOW_NAME, "WF"),
                    new Claim(JwtRegisteredClaimNames.Jti, "0"),
                    new Claim(Utils._FAM_SESSION_ID, "1")
                });


            ApiContext _apiContext = new("1.0", "Server", "DB", "WF");
            Utils.SetCurrentApiContext(_apiContext);

            _documentDataMock = new();
            _documentDataMock.SetupGet(x => x.DocumentSessionFileId).Returns(_fileID);

            Mock<IDocumentDataFactory> _documentDataFactoryMock = new();
            _documentDataFactoryMock.Setup(x => x.Create(It.IsAny<ApiContext>())).Returns(_documentDataMock.Object);
            _documentDataFactoryMock.Setup(x => x.Create(It.IsAny<ClaimsPrincipal>(), It.IsAny<bool>())).Returns(_documentDataMock.Object);

            var context = new DefaultHttpContext { User = _userMock.Object };
            var controllerContext = new ControllerContext() { HttpContext = context };

            _appBackendController = new(_documentDataFactoryMock.Object);
            _appBackendController.ControllerContext = controllerContext;
        }

        // Confirm that AppBackendController.CloseDocument makes the expected DocumentData method calls
        [Test]
        [Pairwise]
        public void Test_CloseDocument(
            [Values] bool isCommit,
            [Values(int.MinValue, -1, 42, int.MaxValue)] int activityTime,
            [Values(int.MinValue, -1, 42, int.MaxValue)] int overheadTime,
            [Values] bool closedBecauseOfInactivityTimeout)
        {
            // Arrange
            bool expectGoodResult = !(isCommit && closedBecauseOfInactivityTimeout);

            // Act
            IActionResult result = _appBackendController.CloseDocument(_fileID, isCommit, -1, activityTime, overheadTime, closedBecauseOfInactivityTimeout);

            // Assert
            if (expectGoodResult)
            {
                Assert.IsInstanceOf<NoContentResult>(result);

                _documentDataMock.Verify(x => x.CloseDocument(
                    isCommit ? EActionStatus.kActionCompleted : EActionStatus.kActionPending,
                    It.IsAny<ExtractException>(),
                    activityTime,
                    overheadTime,
                    closedBecauseOfInactivityTimeout), Times.Once());
            }
            else
            {
                Assert.IsInstanceOf<ObjectResult>(result);
                Assert.IsInstanceOf<ErrorResult>(((ObjectResult)result).Value);

                _documentDataMock.Verify(x => x.CloseDocument(
                    It.IsAny<EActionStatus>(),
                    It.IsAny<ExtractException>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<bool>()), Times.Never());
            }
        }

        // Confirm that AppBackendController.SkipDocument makes the expected DocumentData method calls
        [Test]
        [Pairwise]
        public void Test_SkipDocument(
            [Values] bool nullData,
            [Values(int.MinValue, -1, 42, int.MaxValue)] int duration,
            [Values(int.MinValue, -1, 42, int.MaxValue)] int activityTime,
            [Values(int.MinValue, -1, 42, int.MaxValue)] int overheadTime,
            [Values("", "Hmmm", "Ok")] string comment)
        {
            // Arrange
            SkipDocumentData skipData = nullData ? null : new()
            {
                Duration = duration,
                ActivityTime = activityTime,
                OverheadTime = overheadTime,
                Comment = comment
            };

            // Act
            IActionResult result = _appBackendController.SkipDocument(_fileID, skipData);

            // Assert
            Assert.IsInstanceOf<NoContentResult>(result);

            _documentDataMock.Verify(x => x.CloseDocument(
                EActionStatus.kActionSkipped,
                null,
                nullData ? -1 : activityTime,
                nullData ? -1 : overheadTime,
                false), Times.Once());
        }

        // Confirm that AppBackendController.FailDocument makes the expected DocumentData method calls
        [Test]
        [Pairwise]
        public void Test_FailDocument(
            [Values(int.MinValue, -1, 42, int.MaxValue)] int duration,
            [Values(int.MinValue, -1, 42, int.MaxValue)] int activityTime,
            [Values(int.MinValue, -1, 42, int.MaxValue)] int overheadTime)
        {
            // Act
            IActionResult result = _appBackendController.FailDocument(_fileID, duration, activityTime, overheadTime);

            // Assert
            Assert.IsInstanceOf<NoContentResult>(result);

            _documentDataMock.Verify(x => x.CloseDocument(
                EActionStatus.kActionFailed,
                null,
                activityTime,
                overheadTime,
                false), Times.Once());
        }
    }
}
