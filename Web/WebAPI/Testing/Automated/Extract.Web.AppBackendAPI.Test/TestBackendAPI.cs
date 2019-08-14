using Extract.FileActionManager.Database.Test;
using Extract.FileActionManager.FileProcessors;
using Extract.Imaging;
using Extract.Testing.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net;
using UCLID_AFCORELib;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;
using UCLID_RASTERANDOCRMGMTLib;
using WebAPI;
using WebAPI.Controllers;
using WebAPI.Models;

using static WebAPI.Utils;

using ComRasterZone = UCLID_RASTERANDOCRMGMTLib.RasterZone;

namespace Extract.Web.WebAPI.Test
{
    [TestFixture]
    [NUnit.Framework.Category("AppBackendAPI")]
    public class TestBackendAPI
    {
        #region Constants

        static readonly string _TEST_FILE1 = "Resources.TestImage003.tif";
        static readonly string _TEST_FILE1_USS = "Resources.TestImage003.tif.uss";
        static readonly string _TEST_FILE1_VOA = "Resources.TestImage003.tif.voa";
        static readonly string _COMPUTE_ACTION = "Compute";
        static readonly string _VERIFY_ACTION = "Verify";

        #endregion Constants

        #region Fields

        /// <summary>
        /// Manages test files.
        /// </summary>
        static TestFileManager<TestBackendAPI> _testFiles;

        /// <summary>
        /// test DB Manager, used to extract a database backup file from the resource, and the attach/detach it
        /// to the local database server. 
        /// </summary>
        static FAMTestDBManager<TestBackendAPI> _testDbManager;

        /// <summary>
        /// Represents a file processingdb
        /// </summary>
        private static FileProcessingDB fileProcessingDb;

        /// <summary>
        /// Represents a user.
        /// </summary>
        private static User user;

        /// <summary>
        /// Represents a fake controller to call.
        /// </summary>
        private static AppBackendController controller;

        #endregion Fields

        #region Setup and Teardown

        [TestFixtureSetUp]
        public static void Setup()
        {
            GeneralMethods.TestSetup();

            _testFiles = new TestFileManager<TestBackendAPI>();
            _testDbManager = new FAMTestDBManager<TestBackendAPI>();
        }

        [TestFixtureTearDown]
        public static void FinalCleanup()
        {
            if (_testFiles != null)
            {
                _testFiles.Dispose();
                _testFiles = null;
            }

            if (_testDbManager != null)
            {
                _testDbManager.Dispose();
                _testDbManager = null;
            }
        }

        #endregion Setup and Teardown

        #region Public Test Functions

        [Test]
        [Category("Automated")]
        [Category("WebAPIBackend")]
        public static void Test_LoginLogout()
        {
            string dbName = "AppBackendAPI_Test_LoginLogout";

            try
            {
                InitialzeDBAndUser(dbName);

                var result = controller.Login(user);
                var token = result.AssertGoodResult<JwtSecurityToken>();

                // Login should register an active FAM session
                Assert.IsFalse(fileProcessingDb.IsAnyFAMActive());

                controller.ApplyTokenClaimPrincipalToContext(token);

                controller.Logout()
                    .AssertGoodResult<NoContentResult>();

                // Logout should close the FAM session
                Assert.IsFalse(fileProcessingDb.IsAnyFAMActive());
            }
            finally
            {
                FileApiMgr.ReleaseAll();
                _testDbManager.RemoveDatabase(dbName);
            }
        }

        [Test]
        [Category("Automated")]
        [Category("WebAPIBackend")]
        public static void Test_LoginBlankUserName()
        {
            string dbName = "AppBackendAPI_Test_LoginBlankUserName";

            try
            {
                InitialzeDBAndUser(dbName, "", "123");

                Assert.AreEqual(((ErrorResult)(((ObjectResult)controller.Login(user)).Value)).Error.Message, "Username is empty");
            }
            finally
            {
                FileApiMgr.ReleaseAll();
                _testDbManager.RemoveDatabase(dbName);
            }
        }

        [Test]
        [Category("Automated")]
        [Category("WebAPIBackend")]
        public static void Test_SetMetadataValue()
        {
            string dbName = "AppBackendAPI_Test_SetMetadataValue";

            try
            {
                InitialzeDBAndUser(dbName);
                fileProcessingDb.AddMetadataField("DocumentType");

                var openDocumentResult = loginToWebapp(controller, user, true);

                var documenttype = controller.SetMetadataField(openDocumentResult.Id, "DocumentType", "IgnoreTheAlien");

                Assert.AreEqual("IgnoreTheAlien"
                    , controller.GetMetadataField(1, "DocumentType").AssertGoodResult<MetadataFieldResult>().Value
                    , "Document Type should have been set to Ignore the alien and failed for some reason");
            }
            finally
            {
                FileApiMgr.ReleaseAll();
                _testDbManager.RemoveDatabase(dbName);
            }
        }

        [Test]
        [Category("Automated")]
        [Category("WebAPIBackend")]
        public static void Test_GetNullMetadataValue()
        {
            string dbName = "AppBackendAPI_Test_GetNullMetadataValue";

            try
            {
                InitialzeDBAndUser(dbName);
                fileProcessingDb.AddMetadataField("DocumentType");

                var openDocumentResult = loginToWebapp(controller, user, true);

                var documenttype = controller.GetMetadataField(openDocumentResult.Id, "DocumentType").AssertGoodResult<MetadataFieldResult>().Value;
                Assert.AreEqual(null, documenttype, "DocumentType should default to null");
            }
            finally
            {
                FileApiMgr.ReleaseAll();
                _testDbManager.RemoveDatabase(dbName);
            }
        }

        [Test]
        [Category("Automated")]
        [Category("WebAPIBackend")]
        public static void Test_LoginBlankPassword()
        {
            string dbName = "AppBackendAPI_Test_LoginBlankUserName";

            try
            {
                InitialzeDBAndUser(dbName, "jane_doe","");

                Assert.AreEqual(((ErrorResult)(((ObjectResult)controller.Login(user)).Value)).Error.Message,"Password is empty");
            }
            finally
            {
                FileApiMgr.ReleaseAll();
                _testDbManager.RemoveDatabase(dbName);
            }
        }

        [Test]
        [Category("Automated")]
        [Category("WebAPIBackend")]
        public static void Test_SessionLoginLogout()
        {
            string dbName = "AppBackendAPI_Test_SessionLoginLogout";

            try
            {
                InitialzeDBAndUser(dbName);

                // Test that SessionLogin requires a logged in user
                controller.SessionLogin().AssertResultCode(500, "User should be logged in before logging into to session");

                Assert.IsFalse(fileProcessingDb.IsAnyFAMActive(), "There should not be an active FAM session with invalid SessionLogin");

                var result = controller.Login(user);
                var token = result.AssertGoodResult<JwtSecurityToken>();

                controller.ApplyTokenClaimPrincipalToContext(token);

                // get the expires claim
                var loginExpires = controller.User.GetClaim(_EXPIRES_TIME);

                result = controller.SessionLogin();
                var sessionToken = result.AssertGoodResult<JwtSecurityToken>();

                controller.ApplyTokenClaimPrincipalToContext(sessionToken);

                // SessionLogin should register an active FAM session
                Assert.IsTrue(fileProcessingDb.IsAnyFAMActive());

                Assert.AreEqual(loginExpires, controller.User.GetClaim(_EXPIRES_TIME), "Session expire time should equal login expire time.");

                // Logs out the session
                controller.Logout()
                    .AssertGoodResult<NoContentResult>();

                // Logout should close the FAM session
                Assert.IsFalse(fileProcessingDb.IsAnyFAMActive());

                controller.ApplyTokenClaimPrincipalToContext(token);

                controller.Logout()
                    .AssertGoodResult<NoContentResult>();
            }
            finally
            {
                FileApiMgr.ReleaseAll();
                _testDbManager.RemoveDatabase(dbName);
            }
        }

        [Test]
        [Category("Automated")]
        [Category("WebAPIBackend")]
        public static void Test_GetSettings()
        {
            string dbName = "AppBackendAPI_Test_GetSettings";
            var temporaryDocType = Path.GetTempFileName();

            try
            {
                File.WriteAllText(temporaryDocType, "Ambulance - Encounter \nAmbulance - Patient \nAnesthesia \nAppeal Request");
                InitialzeDBAndUser(dbName);
                var newSettings = $"{{ \"InactivityTimeout\": 5, \"RedactionTypes\": [\"SSN\", \"DOB\"], \"DocumentTypes\": \"{temporaryDocType.Replace(@"\", @"\\")}\" }}";
                fileProcessingDb.ExecuteCommandQuery($"UPDATE [dbo].[WebAppConfig] SET SETTINGS = '{newSettings}' WHERE TYPE = 'RedactionVerificationSettings'");
                var result = controller.Login(user);
                var token = result.AssertGoodResult<JwtSecurityToken>();
                controller.ApplyTokenClaimPrincipalToContext(token);


                var sessionToken = controller.SessionLogin().AssertGoodResult<JwtSecurityToken>();
                controller.ApplyTokenClaimPrincipalToContext(sessionToken);

                result = controller.GetSettings();
                var settings = result.AssertGoodResult<WebAppSettingsResult>();

                Assert.IsTrue(settings.RedactionTypes.SequenceEqual(
                    new[] { "SSN", "DOB" }), "Failed to retrieve redaction types");

                Assert.IsTrue(settings.ParsedDocumentTypes.SequenceEqual(
                    new[] { "Ambulance - Encounter ", "Ambulance - Patient ", "Anesthesia ", "Appeal Request" }),
                    "Document Settings failed to retrive");

                // Default value
                Assert.IsTrue(settings.InactivityTimeout == 5);

                controller.Logout()
                    .AssertGoodResult<NoContentResult>();
            }
            finally
            {
                File.Delete(temporaryDocType);
                FileApiMgr.ReleaseAll();
                _testDbManager.RemoveDatabase(dbName);
            }
        }

        [Test]
        [Category("Automated")]
        [Category("WebAPIBackend")]
        public static void Test_OpenDocument_NoID()
        {
            string dbName = "AppBackendAPI_Test_OpenDocument_NoID";

            try
            {
                InitialzeDBAndUser(dbName);

                var result = controller.Login(user);
                var token = result.AssertGoodResult<JwtSecurityToken>();
                controller.ApplyTokenClaimPrincipalToContext(token);

                result = controller.OpenDocument();
                result.AssertResultCode(500, "Action should have a valid session login token.");

                var sessionResult = controller.SessionLogin();
                var sessionToken = sessionResult.AssertGoodResult<JwtSecurityToken>();
                controller.ApplyTokenClaimPrincipalToContext(sessionToken);

                result = controller.OpenDocument();
                var openDocumentResult = result.AssertGoodResult<DocumentIdResult>();
                Assert.AreEqual(1, openDocumentResult.Id);

                Assert.AreEqual(EActionStatus.kActionProcessing, fileProcessingDb.GetFileStatus(1, _VERIFY_ACTION, false));

                controller.CloseDocument(openDocumentResult.Id, commit: false)
                    .AssertGoodResult<NoContentResult>();

                Assert.AreEqual(EActionStatus.kActionPending, fileProcessingDb.GetFileStatus(1, _VERIFY_ACTION, false));

                result = controller.OpenDocument();
                openDocumentResult = result.AssertGoodResult<DocumentIdResult>();

                Assert.AreEqual(EActionStatus.kActionProcessing, fileProcessingDb.GetFileStatus(1, _VERIFY_ACTION, false));

                controller.CloseDocument(openDocumentResult.Id, commit: true)
                    .AssertGoodResult<NoContentResult>();

                Assert.AreEqual(EActionStatus.kActionCompleted, fileProcessingDb.GetFileStatus(1, _VERIFY_ACTION, false));


                controller.Logout()
                    .AssertGoodResult<NoContentResult>();
            }
            finally
            {
                FileApiMgr.ReleaseAll();
                _testDbManager.RemoveDatabase(dbName);
            }
        }

        [Test]
        [Category("Automated")]
        [Category("WebAPIBackend")]
        public static void Test_OpenDocument_WithID()
        {
            string dbName = "AppBackendAPI_Test_OpenDocument_WithID";

            try
            {
                InitialzeDBAndUser(dbName);

                var result = controller.Login(user);
                var token = result.AssertGoodResult<JwtSecurityToken>();
                controller.ApplyTokenClaimPrincipalToContext(token);

                result = controller.OpenDocument(1);
                result.AssertResultCode(500, "Action should have a valid session login token.");

                var sessionResult = controller.SessionLogin();
                var sessionToken = sessionResult.AssertGoodResult<JwtSecurityToken>();
                controller.ApplyTokenClaimPrincipalToContext(sessionToken);

                result = controller.OpenDocument(1);
                var openDocumentResult = result.AssertGoodResult<DocumentIdResult>();
                Assert.AreEqual(1, openDocumentResult.Id);

                Assert.AreEqual(EActionStatus.kActionProcessing, fileProcessingDb.GetFileStatus(1, _VERIFY_ACTION, false));

                controller.CloseDocument(openDocumentResult.Id, commit: false)
                    .AssertGoodResult<NoContentResult>();

                Assert.AreEqual(EActionStatus.kActionPending, fileProcessingDb.GetFileStatus(1, _VERIFY_ACTION, false));

                result = controller.OpenDocument(2);
                openDocumentResult = result.AssertGoodResult<DocumentIdResult>();
                Assert.AreEqual(2, openDocumentResult.Id);

                Assert.AreEqual(EActionStatus.kActionProcessing, fileProcessingDb.GetFileStatus(2, _VERIFY_ACTION, false));

                controller.CloseDocument(openDocumentResult.Id, commit: true)
                    .AssertGoodResult<NoContentResult>();

                Assert.AreEqual(EActionStatus.kActionCompleted, fileProcessingDb.GetFileStatus(2, _VERIFY_ACTION, false));

                controller.Logout()
                    .AssertGoodResult<NoContentResult>();
            }
            finally
            {
                FileApiMgr.ReleaseAll();
                _testDbManager.RemoveDatabase(dbName);
            }
        }

        [Test]
        [Category("Automated")]
        [Category("WebAPIBackend")]
        public static void Test_GetQueueStatus()
        {
            string dbName = "AppBackendAPI_Test_GetQueueStatus";

            try
            {
                InitialzeDBAndUser(dbName);

                var result = controller.Login(user);
                var token = result.AssertGoodResult<JwtSecurityToken>();
                controller.ApplyTokenClaimPrincipalToContext(token);

                // Should be able to get status without a session login
                result = controller.GetQueueStatus();
                var queueStatus = result.AssertGoodResult<QueueStatusResult>();
                Assert.AreEqual(0, queueStatus.ActiveUsers);
                Assert.AreEqual(4, queueStatus.PendingDocuments);

                // ... as well as with a session login
                var sessionResult = controller.SessionLogin();
                var sessionToken = sessionResult.AssertGoodResult<JwtSecurityToken>();
                controller.ApplyTokenClaimPrincipalToContext(sessionToken);

                result = controller.GetQueueStatus();
                queueStatus = result.AssertGoodResult<QueueStatusResult>();
                Assert.AreEqual(1, queueStatus.ActiveUsers);
                Assert.AreEqual(4, queueStatus.PendingDocuments);

                result = controller.OpenDocument();
                var openDocumentResult = result.AssertGoodResult<DocumentIdResult>();

                result = controller.GetQueueStatus();
                queueStatus = result.AssertGoodResult<QueueStatusResult>();
                Assert.AreEqual(1, queueStatus.ActiveUsers);
                Assert.AreEqual(3, queueStatus.PendingDocuments);

                User user2 = ApiTestUtils.CreateUser("jon_doe", "123");
                var controller2 = ApiTestUtils.CreateController<AppBackendController>(user2);

                result = controller2.Login(user2);
                token = result.AssertGoodResult<JwtSecurityToken>();
                controller2.ApplyTokenClaimPrincipalToContext(token);

                sessionResult = controller2.SessionLogin();
                sessionToken = sessionResult.AssertGoodResult<JwtSecurityToken>();

                result = controller.GetQueueStatus();
                queueStatus = result.AssertGoodResult<QueueStatusResult>();
                Assert.AreEqual(2, queueStatus.ActiveUsers);
                Assert.AreEqual(3, queueStatus.PendingDocuments);

                controller.CloseDocument(openDocumentResult.Id, commit: true)
                    .AssertGoodResult<NoContentResult>();

                result = controller.OpenDocument();
                result.AssertGoodResult<DocumentIdResult>();

                result = controller2.GetQueueStatus();
                queueStatus = result.AssertGoodResult<QueueStatusResult>();
                Assert.AreEqual(2, queueStatus.ActiveUsers);
                Assert.AreEqual(2, queueStatus.PendingDocuments);

                controller2.ApplyTokenClaimPrincipalToContext(sessionToken);
                result = controller2.OpenDocument();
                result.AssertGoodResult<DocumentIdResult>();

                result = controller2.GetQueueStatus();
                queueStatus = result.AssertGoodResult<QueueStatusResult>();
                Assert.AreEqual(2, queueStatus.ActiveUsers);
                Assert.AreEqual(1, queueStatus.PendingDocuments);

                controller.Logout()
                    .AssertGoodResult<NoContentResult>();

                result = controller2.GetQueueStatus();
                queueStatus = result.AssertGoodResult<QueueStatusResult>();
                Assert.AreEqual(1, queueStatus.ActiveUsers);
                Assert.AreEqual(2, queueStatus.PendingDocuments);

                controller2.Logout()
                    .AssertGoodResult<NoContentResult>();
            }
            finally
            {
                FileApiMgr.ReleaseAll();
                _testDbManager.RemoveDatabase(dbName);
            }
        }

        [Test]
        [Category("Automated")]
        [Category("WebAPIBackend")]
        public static void Test_ReOpenDocument()
        {
            string dbName = "AppBackendAPI_Test_ReOpenDocument";

            try
            {
                InitialzeDBAndUser(dbName);

                var openDocumentResult = loginToWebapp(controller, user, true);
                
                controller.CloseDocument(openDocumentResult.Id, commit: false)
                    .AssertGoodResult<NoContentResult>();

                Assert.AreEqual(EActionStatus.kActionPending, fileProcessingDb.GetFileStatus(2, _VERIFY_ACTION, false));

                var result = controller.OpenDocument(2);
                openDocumentResult = result.AssertGoodResult<DocumentIdResult>();
                Assert.AreEqual(2, openDocumentResult.Id);

                // Per GGK request, allow second call to OpenDocument to not fail and return the ID of the already
                // open document.
                // https://extract.atlassian.net/browse/WEB-55
                result = controller.OpenDocument();
                openDocumentResult = result.AssertGoodResult<DocumentIdResult>();
                Assert.AreEqual(2, openDocumentResult.Id);

                controller.CloseDocument(openDocumentResult.Id, commit: false)
                    .AssertGoodResult<NoContentResult>();

                // After document is closed, OpenDocument should open the first file in the queue,
                // not the 2nd file that had been specified originally.
                result = controller.OpenDocument();
                openDocumentResult = result.AssertGoodResult<DocumentIdResult>();
                Assert.AreEqual(1, openDocumentResult.Id);

                controller.CloseDocument(openDocumentResult.Id, commit: false)
                    .AssertGoodResult<NoContentResult>();

                controller.Logout()
                    .AssertGoodResult<NoContentResult>();
            }
            finally
            {
                FileApiMgr.ReleaseAll();
                _testDbManager.RemoveDatabase(dbName);
            }
        }

        [Test]
        [Category("Automated")]
        [Category("WebAPIBackend")]
        public static void Test_LogoutClosesDocument()
        {
            string dbName = "AppBackendAPI_Test_LogoutClosesDocument";

            try
            {
                InitialzeDBAndUser(dbName);

                loginToWebapp(controller, user, true);

                Assert.AreEqual(EActionStatus.kActionProcessing, fileProcessingDb.GetFileStatus(1, _VERIFY_ACTION, false));

                controller.Logout()
                    .AssertGoodResult<NoContentResult>();

                Assert.AreEqual(EActionStatus.kActionPending, fileProcessingDb.GetFileStatus(1, _VERIFY_ACTION, false));
            }
            finally
            {
                FileApiMgr.ReleaseAll();
                _testDbManager.RemoveDatabase(dbName);
            }
        }

        [Test]
        [Category("Automated")]
        [Category("WebAPIBackend")]
        public static void Test_AbandonedSession()
        {
            string dbName = "AppBackendAPI_Test_AbandonedSession";

            try
            {
                InitialzeDBAndUser(dbName);

                // Login to register an active FAM.
                var result = controller.Login(user);
                var token = result.AssertGoodResult<JwtSecurityToken>();
                controller.ApplyTokenClaimPrincipalToContext(token);

                var sessionResult = controller.SessionLogin();
                var sessionToken = sessionResult.AssertGoodResult<JwtSecurityToken>();
                controller.ApplyTokenClaimPrincipalToContext(sessionToken);

                // Use it to open a document.
                result = controller.OpenDocument();
                var openDocumentResult = result.AssertGoodResult<DocumentIdResult>();
                Assert.AreEqual(1, openDocumentResult.Id);
                Assert.AreEqual(EActionStatus.kActionProcessing, fileProcessingDb.GetFileStatus(1, _VERIFY_ACTION, false));

                result = controller.GetQueueStatus();
                var beforeQueueStatus = result.AssertGoodResult<QueueStatusResult>();

                result = controller.GetDocumentData(1);

                // Simulate the web service being stopped.
                controller.Dispose();
                FileApiMgr.ReleaseAll();

                // Reset the default context to ensure no crossover of session IDs.
                ApiTestUtils.SetDefaultApiContext(dbName);
                var controller2 = ApiTestUtils.CreateController<AppBackendController>(user);

                // Simulate a client that still has a token a previous instance of the service that has since been closed.
                controller2.ApplyTokenClaimPrincipalToContext(sessionToken);

                // DB should still report file 1 as processing
                Assert.AreEqual(EActionStatus.kActionProcessing, fileProcessingDb.GetFileStatus(1, _VERIFY_ACTION, false));

                // Should be able to query queue status despite new controller not having access to the document session.
                result = controller2.GetQueueStatus();
                var afterQueueStatus = result.AssertGoodResult<QueueStatusResult>();

                // Document 1 should remain in processing state
                Assert.AreEqual(beforeQueueStatus.PendingDocuments, afterQueueStatus.PendingDocuments);
                Assert.AreEqual(EActionStatus.kActionProcessing, fileProcessingDb.GetFileStatus(1, _VERIFY_ACTION, false));

                // Query that required access to the document session should resume the processing of the abandoned
                // document session.
                result = controller2.GetDocumentData(1);
                result.AssertResultCode(StatusCodes.Status200OK);

                // Document should still be processing
                Assert.AreEqual(EActionStatus.kActionProcessing, fileProcessingDb.GetFileStatus(1, _VERIFY_ACTION, false));

                controller2.CloseDocument(1, false);
                controller2.Logout();
            }
            finally
            {
                FileApiMgr.ReleaseAll();
                _testDbManager.RemoveDatabase(dbName);
            }
        }

        [Test]
        [Category("Automated")]
        [Category("WebAPIBackend")]
        public static void Test_GetPageInfo_NoUSS()
        {
            string dbName = "AppBackendAPI_Test_GetPageInfo_NoUSS";

            try
            {
                InitialzeDBAndUser(dbName);

                string testFileName = _testFiles.GetFile(_TEST_FILE1);

                var fileRecord = fileProcessingDb.AddFile(
                    testFileName, _VERIFY_ACTION, 1, EFilePriority.kPriorityHigh, false, false,
                    EActionStatus.kActionPending, false, out bool t1, out EActionStatus t2);
                int fileId = fileRecord.FileID;

                var openDocumentResult = loginToWebapp(controller, user, true);
                Assert.AreEqual(fileId, openDocumentResult.Id);

                var result = controller.GetPageInfo(openDocumentResult.Id);
                var pagesInfo = result.AssertGoodResult<PagesInfoResult>();

                Assert.AreEqual(pagesInfo.PageCount, 4, "Unexpected page count");
                Assert.AreEqual(pagesInfo.PageInfos.Count, 4, "Unexpected page infos count");
                for (int page = 1; page <= 4; page++)
                {
                    var pageInfo = pagesInfo.PageInfos[page - 1];
                    Assert.AreEqual(page, pageInfo.Page, "Unexpected page number");
                    // Page 2 is rotated 90 degrees to the right.
                    Assert.AreEqual((page == 2) ? 2200 : 1712, pageInfo.Width, "Unexpected page width");
                    Assert.AreEqual((page == 2) ? 1712 : 2200, pageInfo.Height, "Unexpected page height");
                    // With no USS, the DisplayOrientation will still be zero.
                    Assert.AreEqual(0, pageInfo.DisplayOrientation, "Unexpected page orientation");
                }

                controller.Logout()
                    .AssertGoodResult<NoContentResult>();
            }
            finally
            {
                FileApiMgr.ReleaseAll();
                _testDbManager.RemoveDatabase(dbName);

                _testFiles.RemoveFile(_TEST_FILE1);
            }
        }

        [Test]
        [Category("Automated")]
        [Category("WebAPIBackend")]
        public static void Test_ProcessAnnotation()
        {
            string dbName = "AppBackendAPI_Test_ProcessAnnotation";

            try
            {
                InitialzeDBAndUser(dbName);

                string testFileName = _testFiles.GetFile(_TEST_FILE1);
                _testFiles.GetFile(_TEST_FILE1_USS);

                var fileRecord = fileProcessingDb.AddFile(
                    testFileName, _VERIFY_ACTION, 1, EFilePriority.kPriorityHigh, false, false,
                    EActionStatus.kActionPending, false, out bool t1, out EActionStatus t2);
                int fileId = fileRecord.FileID;

                var openDocumentResult = loginToWebapp(controller, user, true);
                Assert.AreEqual(fileId, openDocumentResult.Id);

                string voaFile = _testFiles.GetFile(_TEST_FILE1_VOA);
                var voa = new IUnknownVectorClass();
                voa.LoadFrom(voaFile, false);
                var mapper = new AttributeMapper(voa, EWorkflowType.kExtraction);
                var documentAttribute = mapper.MapAttribute((IAttribute)voa.At(1), true);

                var processAnnotationResult = controller.ProcessAnnotation(openDocumentResult.Id, 1, new ProcessAnnotationParameters() { Annotation = documentAttribute, Definition = "{ AutoShrinkRedactionZones: {} }", OperationType = "modify" });//WebApi.Models.ProcessAnnotationParameters
                processAnnotationResult.AssertResultCode(200, "ProcessAnnotation is failing");

                controller.Logout()
                    .AssertGoodResult<NoContentResult>();
            }
            finally
            {
                FileApiMgr.ReleaseAll();
                _testDbManager.RemoveDatabase(dbName);

                _testFiles.RemoveFile(_TEST_FILE1);
                _testFiles.RemoveFile(_TEST_FILE1_USS);
            }
        }

        [Test]
        [Category("Automated")]
        [Category("WebAPIBackend")]
        public static void Test_GetPageInfo_WithUSS()
        {
            string dbName = "AppBackendAPI_Test_GetPageInfo_WithUSS";

            try
            {
                InitialzeDBAndUser(dbName);

                string testFileName = _testFiles.GetFile(_TEST_FILE1);
                _testFiles.GetFile(_TEST_FILE1_USS);

                var fileRecord = fileProcessingDb.AddFile(
                    testFileName, _VERIFY_ACTION, 1, EFilePriority.kPriorityHigh, false, false,
                    EActionStatus.kActionPending, false, out bool t1, out EActionStatus t2);
                int fileId = fileRecord.FileID;

                var openDocumentResult = loginToWebapp(controller, user, true);
                Assert.AreEqual(fileId, openDocumentResult.Id);

                var result = controller.GetPageInfo(openDocumentResult.Id);
                var pagesInfo = result.AssertGoodResult<PagesInfoResult>();

                Assert.AreEqual(pagesInfo.PageCount, 4, "Unexpected page count");
                Assert.AreEqual(pagesInfo.PageInfos.Count, 4, "Unexpected page infos count");
                for (int page = 1; page <= 4; page++)
                {
                    var pageInfo = pagesInfo.PageInfos[page - 1];

                    Assert.AreEqual(page, pageInfo.Page, "Unexpected page number");
                    // Page 2 is rotated 90 degrees to the right.
                    Assert.AreEqual((page == 2) ? 2200 : 1712, pageInfo.Width, "Unexpected page width");
                    Assert.AreEqual((page == 2) ? 1712 : 2200, pageInfo.Height, "Unexpected page height");
                    Assert.AreEqual((page == 2) ? 270 : 0, pageInfo.DisplayOrientation, "Unexpected page orientation");
                }

                controller.Logout()
                    .AssertGoodResult<NoContentResult>();
            }
            finally
            {
                FileApiMgr.ReleaseAll();
                _testDbManager.RemoveDatabase(dbName);

                _testFiles.RemoveFile(_TEST_FILE1);
                _testFiles.RemoveFile(_TEST_FILE1_USS);
            }
        }

        /// <summary>
        /// GET GetDocumentPage/{Id}/{Page}
        /// </summary>
        [Test]
        [Category("Automated")]
        [Category("WebAPIBackend")]
        public static void Test_GetPageImage()
        {
            string dbName = "AppBackendAPI_Test_GetPageImage";

            try
            {
                InitialzeDBAndUser(dbName);

                string testFileName = _testFiles.GetFile(_TEST_FILE1);

                var fileRecord = fileProcessingDb.AddFile(
                    testFileName, _VERIFY_ACTION, 1, EFilePriority.kPriorityHigh, false, false,
                    EActionStatus.kActionPending, false, out bool t1, out EActionStatus t2);

                var openDocumentResult = loginToWebapp(controller, user, true);

                using (var codecs = new ImageCodecs())
                    for (int page = 1; page <= 4; page++)
                    {
                        var result = controller.GetDocumentPage(openDocumentResult.Id, page);
                        var fileResult = result.AssertGoodResult<FileContentResult>();

                        using (var temporaryFile = new Utilities.TemporaryFile(".pdf", false))
                        using (var fileStream = File.OpenWrite(temporaryFile.FileName))
                        {
                            fileStream.Write(fileResult.FileContents, 0, fileResult.FileContents.Length);
                            fileStream.Flush();
                            fileStream.Close();

                            using (var imageReader = codecs.CreateReader(temporaryFile.FileName))
                            {
                                Assert.AreEqual(1, imageReader.PageCount, "Image page not read correctly.");

                                // The source tif image uses a DPI of 200; though it appears the DPI is persisted
                                // into the PDF produced by GetPageImage, LeadTools does not read a DPI from the PDF
                                // (see https://extract.atlassian.net/browse/ISSUE-12273). It will be the web app's
                                // responsibility to ensure the right image dimensions are used. For the purpose of
                                // this test, simply ensure that if the image dimensions are translated using
                                // LeadTool's pageProperties DPI back to 200x200 DPI that the resulting dimensions
                                // are correct.
                                var pageProperties = imageReader.ReadPageProperties(1);
                                int width = (int)Math.Round(((double)pageProperties.Width / (double)pageProperties.XResolution) * 200.0);
                                Assert.AreEqual((page == 2) ? 2200 : 1712, width, "Unexpected page width");
                                int height = (int)Math.Round(((double)pageProperties.Height / (double)pageProperties.YResolution) * 200.0);
                                Assert.AreEqual((page == 2) ? 1712 : 2200, height, "Unexpected page height");
                            }
                        }
                    }

                controller.Logout()
                    .AssertGoodResult<NoContentResult>();
            }
            finally
            {
                FileApiMgr.ReleaseAll();
                _testDbManager.RemoveDatabase(dbName);

                _testFiles.RemoveFile(_TEST_FILE1);
            }
        }

        [Test]
        [Category("Automated")]
        [Category("WebAPIBackend")]
        public static void Test_GetDocumentData()
        {
            string dbName = "AppBackendAPI_Test_GetDocumentData";

            try
            {
                InitialzeDBAndUser(dbName);

                loginToWebapp(controller, user, true);

                var result = controller.GetDocumentData(1);
                var attributeSet = result.AssertGoodResult<DocumentDataResult>();

                Assert.IsTrue(attributeSet.Attributes.Count > 0);
                foreach (var attribute in attributeSet.Attributes)
                {
                    // Per discussion with GGK, non-spatial attributes will not be sent.
                    Assert.IsTrue(attribute.HasPositionInfo == true);
                }

                controller.Logout()
                    .AssertGoodResult<NoContentResult>();
            }
            finally
            {
                FileApiMgr.ReleaseAll();
                _testDbManager.RemoveDatabase(dbName);
            }
        }

        [Test]
        [Category("Automated")]
        [Category("WebAPIBackend")]
        public static void Test_SaveDocumentData()
        {
            string dbName = "AppBackendAPI_Test_SaveDocumentData";

            try
            {
                InitialzeDBAndUser(dbName);

                // Since the uss file will need to be used to translate the saved data into a VOA,
                // we can't use pre-existing files in the database that may or may not actually
                // exist on disk.
                string testFileName = _testFiles.GetFile(_TEST_FILE1);
                _testFiles.GetFile(_TEST_FILE1_USS);
                _testFiles.GetFile(_TEST_FILE1_VOA);

                var fileRecord = fileProcessingDb.AddFile(
                    testFileName, _COMPUTE_ACTION, 1, EFilePriority.kPriorityHigh, false, false,
                    EActionStatus.kActionPending, false, out bool t1, out EActionStatus t2);
                int fileId = fileRecord.FileID;

                var taskConfig = new StoreAttributesInDBTask();
                taskConfig.AttributeSetName = "Attr";
                var task = (IFileProcessingTask)taskConfig;

                using (var famSession = new FAMProcessingSession(
                    fileProcessingDb, _COMPUTE_ACTION, ApiTestUtils.CurrentApiContext.WorkflowName, task))
                {
                    famSession.WaitForProcessingToComplete();
                }

                fileProcessingDb.SetFileStatusToPending(fileId, _VERIFY_ACTION, false);

                var openDocumentResult = loginToWebapp(controller, user, true);

                var result = controller.GetDocumentData(openDocumentResult.Id);
                var attributeSet = result.AssertGoodResult<DocumentDataResult>();

                var updatedAttributes = attributeSet.Attributes.Skip(1);
                Assert.AreEqual(updatedAttributes.Count() + 1, attributeSet.Attributes.Count());

                var updateAttributeSet = new DocumentDataInput()
                {
                    Attributes = new List<DocumentAttribute>(updatedAttributes)
                };

                controller.SaveDocumentData(openDocumentResult.Id, updateAttributeSet)
                    .AssertGoodResult<NoContentResult>();

                result = controller.GetDocumentData(openDocumentResult.Id);
                attributeSet = result.AssertGoodResult<DocumentDataResult>();

                Assert.AreEqual(attributeSet.Attributes.Count(), updatedAttributes.Count());

                controller.Logout()
                    .AssertGoodResult<NoContentResult>();
            }
            finally
            {
                FileApiMgr.ReleaseAll();
                _testDbManager.RemoveDatabase(dbName);

                _testFiles.RemoveFile(_TEST_FILE1);
                _testFiles.RemoveFile(_TEST_FILE1_USS);
                _testFiles.RemoveFile(_TEST_FILE1_VOA);
            }
        }

        [Test]
        [Category("Automated")]
        [Category("WebAPIBackend")]
        public static void Test_GetDocumentWordZones()
        {
            string dbName = "AppBackendAPI_Test_GetDocumentWordZones";

            try
            {
                InitialzeDBAndUser(dbName);

                string testFileName = _testFiles.GetFile(_TEST_FILE1);
                string ussFileName = _testFiles.GetFile(_TEST_FILE1_USS);

                var fileRecord = fileProcessingDb.AddFile(
                    testFileName, _VERIFY_ACTION, 1, EFilePriority.kPriorityHigh, false, false,
                    EActionStatus.kActionPending, false, out bool t1, out EActionStatus t2);
                int fileId = fileRecord.FileID;

                var openDocumentResult = loginToWebapp(controller, user, true);

                var documentText = new SpatialString();
                documentText.LoadFrom(ussFileName, false);

                var documentPages = documentText.GetPages(false, "");

                for (int page = 1; page <= documentPages.Size(); page++)
                {
                    var pageText = documentText.GetSpecifiedPages(page, page);

                    List<ComRasterZone> pageWords =
                        pageText.GetLines().ToIEnumerable<SpatialString>()
                            .SelectMany(line => line.GetWords().ToIEnumerable<SpatialString>()
                                .Where(word => word.HasSpatialInfo())
                                .Select(word => (ComRasterZone)word.GetOriginalImageRasterZones().At(0)))
                            .ToList();

                    var result = controller.GetPageWordZones(openDocumentResult.Id, page);
                    var wordZoneData = result.AssertGoodResult<WordZoneDataResult>()
                        .Zones
                        .SelectMany(line => line)
                        .ToList();
                    Assert.AreEqual(pageWords.Count, wordZoneData.Count(), "Unexpected number of words");

                    for (int i = 0; i < wordZoneData.Count(); i++)
                    {
                        var wordZone = wordZoneData[i];
                        var spatialStringZone = pageWords[i];

                        Assert.AreEqual(page, wordZone.PageNumber, "Incorrect page");
                        Assert.AreEqual(spatialStringZone.StartX, wordZone.StartX, "Incorrect StartX");
                        Assert.AreEqual(spatialStringZone.StartY, wordZone.StartY, "Incorrect StartY");
                        Assert.AreEqual(spatialStringZone.EndX, wordZone.EndX, "Incorrect EndX");
                        Assert.AreEqual(spatialStringZone.EndY, wordZone.EndY, "Incorrect EndY");
                        Assert.AreEqual(spatialStringZone.Height, wordZone.Height, "Incorrect height");
                    }
                }

                controller.Logout()
                    .AssertGoodResult<NoContentResult>();
            }
            finally
            {
                FileApiMgr.ReleaseAll();
                _testDbManager.RemoveDatabase(dbName);

                _testFiles.RemoveFile(_TEST_FILE1);
                _testFiles.RemoveFile(_TEST_FILE1_USS);
            }
        }

        [Test]
        [Category("Automated")]
        [Category("WebAPIBackend")]
        public static void Test_AddGetComment()
        {
            string dbName = "AppBackendAPI_Test_AddGetComment";

            try
            {
                InitialzeDBAndUser(dbName);

                var openDocumentResult = loginToWebapp(controller, user, true);

                CommentData commentData = new CommentData()
                {
                    Comment = "Add Test Comment"
                };
                var result = controller.AddComment(openDocumentResult.Id, commentData);
                result.AssertGoodResult<NoContentResult>();

                result = controller.GetComment(openDocumentResult.Id);
                var commentResult =  result.AssertGoodResult<CommentData>();
                Assert.AreEqual(commentData.Comment, commentResult.Comment, "Retrieved Comment should equal the saved comment.");

                controller.CloseDocument(openDocumentResult.Id, true);
                controller.Logout();

            }
            finally
            {
                FileApiMgr.ReleaseAll();
                _testDbManager.RemoveDatabase(dbName);
            }
        }

        [Test]
        [Category("Automated")]
        [Category("WebAPIBackend")]
        public static void Test_SkipDocument()
        {
            string dbName = "AppBackendAPI_Test_SkipDocument";

            try
            {
                InitialzeDBAndUser(dbName);

                var openDocumentResult = loginToWebapp(controller, user, true);

                SkipDocumentData skipDocumentData = new SkipDocumentData()
                {
                    Duration = -1,
                    Comment = "Add Test Comment"
                };

                var result = controller.SkipDocument(openDocumentResult.Id, skipDocumentData);
                result.AssertGoodResult<NoContentResult>();

                var actionId = fileProcessingDb.GetActionID(_VERIFY_ACTION);
                var stats = fileProcessingDb.GetStats(actionId, true, true);
                Assert.AreEqual(1, stats.NumDocumentsSkipped, "There should be 1 skipped document.");

                string comment = fileProcessingDb.GetFileActionComment(openDocumentResult.Id, actionId);
                Assert.AreEqual(skipDocumentData.Comment, comment, "Retrieved Comment should equal the saved comment.");

                controller.CloseDocument(openDocumentResult.Id, true);
                controller.Logout();
            }
            finally
            {
                FileApiMgr.ReleaseAll();
                _testDbManager.RemoveDatabase(dbName);
            }
        }

        [Test, Category("Automated")]
        public static void Test_FailDocument()
        {
            string dbName = "AppBackendAPI_Test_FailDocument";

            try
            {
                InitialzeDBAndUser(dbName);

                var openDocumentResult = loginToWebapp(controller,user,true);

                var result = controller.FailDocument(openDocumentResult.Id);
                result.AssertGoodResult<NoContentResult>();

                var actionId = fileProcessingDb.GetActionID(_VERIFY_ACTION);
                var stats = fileProcessingDb.GetStats(actionId, true, true);
                Assert.AreEqual(1, stats.NumDocumentsFailed, "There should be 1 failed document.");

                controller.CloseDocument(openDocumentResult.Id, true);
                controller.Logout();
            }
            finally
            {
                FileApiMgr.ReleaseAll();
                _testDbManager.RemoveDatabase(dbName);
            }
        }

        #endregion Public Test Functions

        
        private static DocumentIdResult loginToWebapp(AppBackendController controller, User user, bool openDocument)
        {
            var result = controller.Login(user);
            var token = result.AssertGoodResult<JwtSecurityToken>();
            controller.ApplyTokenClaimPrincipalToContext(token);

            var sessionResult = controller.SessionLogin();
            var sessionToken = sessionResult.AssertGoodResult<JwtSecurityToken>();
            controller.ApplyTokenClaimPrincipalToContext(sessionToken);
            if(openDocument)
            {
                result = controller.OpenDocument();
                return result.AssertGoodResult<DocumentIdResult>();
            }
            return null;
        }

        private static void InitialzeDBAndUser(string dbName, string username = "jane_doe", string password = "123")
        {
            (fileProcessingDb, user, controller) =
                _testDbManager.InitializeEnvironment<TestBackendAPI, AppBackendController>
                        ("Resources.Demo_IDShield.bak", dbName, username, password);
        }
    }
}
