using Extract.FileActionManager.Database.Test;
using Extract.FileActionManager.FileProcessors;
using Extract.Imaging;
using Extract.Testing.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        static readonly string _TEST_FILE1 = "Resources.TestImage001.tif";
        static readonly string _TEST_FILE2 = "Resources.TestImage002.tif";
        static readonly string _TEST_FILE3 = "Resources.TestImage003.tif";
        static readonly string _TEST_FILE4 = "Resources.TestImage004.tif";
        static readonly string _TEST_FILE_1000Pages = "Resources.01000pages.tif";

        static readonly string[] _testFileArray = new string[] { _TEST_FILE1, _TEST_FILE2, _TEST_FILE3, _TEST_FILE4, _TEST_FILE_1000Pages };
        static readonly string[] _testFileOriginalNames = new string[]
        {
            @"C:\Demo_IDShield\Input\TestImage001.tif",
            @"C:\Demo_IDShield\Input\TestImage002.tif",
            @"C:\Demo_IDShield\Input\TestImage003.tif",
            @"C:\Demo_IDShield\Input\TestImage004.tif",
            ""
        };
        static readonly string[] _testFileNames = new string[_testFileArray.Length];
        static readonly bool[] _testFileInDB = new bool[] { true, true, true, true, false };
        static readonly bool[] _testFileHasUSS = new bool[] { true, false, true, false, true };
        static readonly bool[] _testFileHasVOA = new bool[] { false, false, true, false, false };
        

        //static readonly string _COMPUTE_ACTION = "Compute";
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
        public static void LoginLogout()
        {
            string dbName = "AppBackendAPI_Test_LoginLogout";

            try
            {
                var (fileProcessingDb, user, controller) = InitializeDBAndUser(dbName);

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
        public static void LoginBlankUserName()
        {
            string dbName = "AppBackendAPI_Test_LoginBlankUserName";

            try
            {
                var (fileProcessingDb, user, controller) = InitializeDBAndUser(dbName, "", "123");

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
        public static void SetMetadataValue()
        {
            string dbName = "AppBackendAPI_Test_SetMetadataValue";

            try
            {
                var (fileProcessingDb, user, controller) = InitializeDBAndUser(dbName);
                fileProcessingDb.AddMetadataField("DocumentType");

                LogInToWebApp(controller, user);
                var openDocumentResult = OpenDocument(controller, 1);

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
        public static void ChangePassword()
        {
            string dbName = "AppBackendAPI_Test_ChangePassword";

            try
            {
                var (fileProcessingDb, user, controller) = InitializeDBAndUser(dbName);

                LogInToWebApp(controller, user);

                var result = controller.ChangePassword("123", "F1234567");
                result.AssertResultCode(200, "Failed to change password");
                controller.Logout();
                user.Password = "F1234567";

                LogInToWebApp(controller, user);
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
        public static void ChangePasswordNoSessionToken()
        {
            string dbName = "AppBackendAPI_Test_ChangePassword";

            try
            {
                var (fileProcessingDb, user, controller) = InitializeDBAndUser(dbName);

                controller.Login(user);

                var result = controller.ChangePassword("123", "F1234567");
                result.AssertResultCode(500, "You should not be able to change your password without a session token");
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
        public static void ChangePasswordInvalidPassword()
        {
            string dbName = "AppBackendAPI_Test_ChangePasswordInvalidPassword";

            try
            {
                var (fileProcessingDb, user, controller) = InitializeDBAndUser(dbName);

                LogInToWebApp(controller, user);

                var result = controller.ChangePassword("a", "F1234567");
                result.AssertResultCode(500, "Attempting to change a password with an invalid password should yield a failure");
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
        public static void GetNullMetadataValue()
        {
            string dbName = "AppBackendAPI_Test_GetNullMetadataValue";

            try
            {
                var (fileProcessingDb, user, controller) = InitializeDBAndUser(dbName);
                fileProcessingDb.AddMetadataField("DocumentType");

                LogInToWebApp(controller, user);
                var openDocumentResult = OpenDocument(controller, 1);

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
        public static void LoginBlankPassword()
        {
            string dbName = "AppBackendAPI_Test_LoginBlankUserName";

            try
            {
                var (fileProcessingDb, user, controller) = InitializeDBAndUser(dbName, "jane_doe","");

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
        public static void SessionLoginLogout()
        {
            string dbName = "AppBackendAPI_Test_SessionLoginLogout";

            try
            {
                var (fileProcessingDb, user, controller) = InitializeDBAndUser(dbName);

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
        public static void GetSettings()
        {
            string dbName = "AppBackendAPI_Test_GetSettings";
            var temporaryDocType = Path.GetTempFileName();

            try
            {
                File.WriteAllText(temporaryDocType, "Ambulance - Encounter \nAmbulance - Patient \nAnesthesia \nAppeal Request");
                var (fileProcessingDb, user, controller) = InitializeDBAndUser(dbName);
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
        public static void OpenDocument_NoID()
        {
            string dbName = "AppBackendAPI_Test_OpenDocument_NoID";

            try
            {
                var (fileProcessingDb, user, controller) = InitializeDBAndUser(dbName);

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
        public static void OpenDocument_WithID()
        {
            string dbName = "AppBackendAPI_Test_OpenDocument_WithID";

            try
            {
                var (fileProcessingDb, user, controller) = InitializeDBAndUser(dbName);

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
        public static void GetQueueStatus()
        {
            string dbName = "AppBackendAPI_Test_GetQueueStatus";

            try
            {
                var (fileProcessingDb, user, controller) = InitializeDBAndUser(dbName);
                var pendingDocuments = _testFileArray.Length;

                var result = controller.Login(user);
                var token = result.AssertGoodResult<JwtSecurityToken>();
                controller.ApplyTokenClaimPrincipalToContext(token);

                // Should be able to get status without a session login
                result = controller.GetQueueStatus();
                var queueStatus = result.AssertGoodResult<QueueStatusResult>();
                Assert.AreEqual(0, queueStatus.ActiveUsers);
                Assert.AreEqual(pendingDocuments, queueStatus.PendingDocuments);

                // ... as well as with a session login
                var sessionResult = controller.SessionLogin();
                var sessionToken = sessionResult.AssertGoodResult<JwtSecurityToken>();
                controller.ApplyTokenClaimPrincipalToContext(sessionToken);

                result = controller.GetQueueStatus();
                queueStatus = result.AssertGoodResult<QueueStatusResult>();
                Assert.AreEqual(1, queueStatus.ActiveUsers);
                Assert.AreEqual(pendingDocuments, queueStatus.PendingDocuments);

                result = controller.OpenDocument();
                var openDocumentResult = result.AssertGoodResult<DocumentIdResult>();
                pendingDocuments--;

                result = controller.GetQueueStatus();
                queueStatus = result.AssertGoodResult<QueueStatusResult>();
                Assert.AreEqual(1, queueStatus.ActiveUsers);
                Assert.AreEqual(pendingDocuments, queueStatus.PendingDocuments);

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
                Assert.AreEqual(pendingDocuments, queueStatus.PendingDocuments);

                controller.CloseDocument(openDocumentResult.Id, commit: true)
                    .AssertGoodResult<NoContentResult>();

                result = controller.OpenDocument();
                result.AssertGoodResult<DocumentIdResult>();
                pendingDocuments--;

                result = controller2.GetQueueStatus();
                queueStatus = result.AssertGoodResult<QueueStatusResult>();
                Assert.AreEqual(2, queueStatus.ActiveUsers);
                Assert.AreEqual(pendingDocuments, queueStatus.PendingDocuments);

                controller2.ApplyTokenClaimPrincipalToContext(sessionToken);
                result = controller2.OpenDocument();
                result.AssertGoodResult<DocumentIdResult>();
                pendingDocuments--;

                result = controller2.GetQueueStatus();
                queueStatus = result.AssertGoodResult<QueueStatusResult>();
                Assert.AreEqual(2, queueStatus.ActiveUsers);
                Assert.AreEqual(pendingDocuments, queueStatus.PendingDocuments);

                controller.Logout()
                    .AssertGoodResult<NoContentResult>();
                pendingDocuments++;

                result = controller2.GetQueueStatus();
                queueStatus = result.AssertGoodResult<QueueStatusResult>();
                Assert.AreEqual(1, queueStatus.ActiveUsers);
                Assert.AreEqual(pendingDocuments, queueStatus.PendingDocuments);

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
        public static void ReOpenDocument()
        {
            string dbName = "AppBackendAPI_Test_ReOpenDocument";

            try
            {
                var (fileProcessingDb, user, controller) = InitializeDBAndUser(dbName);

                LogInToWebApp(controller, user);
                var openDocumentResult = OpenDocument(controller, 1);
                
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
        public static void LogoutClosesDocument()
        {
            string dbName = "AppBackendAPI_Test_LogoutClosesDocument";

            try
            {
                var (fileProcessingDb, user, controller) = InitializeDBAndUser(dbName);

                LogInToWebApp(controller, user);
                OpenDocument(controller, 1);

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
        public static void AbandonedSession()
        {
            string dbName = "AppBackendAPI_Test_AbandonedSession";

            try
            {
                var (fileProcessingDb, user, controller) = InitializeDBAndUser(dbName);

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
        public static void GetPageInfo_NoUSS()
        {
            string dbName = "AppBackendAPI_Test_GetPageInfo_NoUSS";

            try
            {
                var (fileProcessingDb, user, controller) = InitializeDBAndUser(dbName);
                _testFiles.RemoveFile(_TEST_FILE3 + ".uss");

                LogInToWebApp(controller, user);
                var openDocumentResult = OpenDocument(controller, 3);
                Assert.AreEqual(3, openDocumentResult.Id);

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
            }
        }

        [Test]
        [Category("Automated")]
        [Category("WebAPIBackend")]
        public static void ProcessAnnotation()
        {
            string dbName = "AppBackendAPI_Test_ProcessAnnotation";

            try
            {
                var (fileProcessingDb, user, controller) = InitializeDBAndUser(dbName);

                LogInToWebApp(controller, user);
                var openDocumentResult = OpenDocument(controller, 3);
                Assert.AreEqual(3, openDocumentResult.Id);

                string voaFile = _testFileNames[2] + ".voa";
                var voa = new IUnknownVectorClass();
                voa.LoadFrom(voaFile, false);
                var mapper = new AttributeMapper(voa, EWorkflowType.kExtraction);
                var documentAttribute = mapper.MapAttribute((IAttribute)voa.At(1), true);

                var processAnnotationResult = controller.ProcessAnnotation(openDocumentResult.Id, 1, new ProcessAnnotationParameters() { Annotation = documentAttribute, Definition = "{ AutoShrinkRedactionZones: {} }", OperationType = "modify" });
                processAnnotationResult.AssertResultCode(200, "ProcessAnnotation is failing");

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
        public static void GetPageInfo_WithUSS()
        {
            string dbName = "AppBackendAPI_Test_GetPageInfo_WithUSS";

            try
            {
                var (fileProcessingDb, user, controller) = InitializeDBAndUser(dbName);
                LogInToWebApp(controller, user);
                var openDocumentResult = OpenDocument(controller, 3);
                Assert.AreEqual(3, openDocumentResult.Id);

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
            }
        }

        /// <summary>
        /// GET GetDocumentPage/{Id}/{Page}
        /// </summary>
        [Test]
        [Category("Automated")]
        [Category("WebAPIBackend")]
        public static void GetPageImage()
        {
            string dbName = "AppBackendAPI_Test_GetPageImage";

            try
            {
                var (fileProcessingDb, user, controller) = InitializeDBAndUser(dbName);

                LogInToWebApp(controller, user);
                var openDocumentResult = OpenDocument(controller, 3);

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

                _testFiles.RemoveFile(_TEST_FILE3);
            }
        }

        [Test]
        [Category("Automated")]
        [Category("WebAPIBackend")]
        public static void GetDocumentData()
        {
            string dbName = "AppBackendAPI_Test_GetDocumentData";

            try
            {
                var (fileProcessingDb, user, controller) = InitializeDBAndUser(dbName);

                LogInToWebApp(controller, user);
                OpenDocument(controller, 1);

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
        public static void SaveDocumentData()
        {
            string dbName = "AppBackendAPI_Test_SaveDocumentData";

            try
            {
                var (fileProcessingDb, user, controller) = InitializeDBAndUser(dbName);

                LogInToWebApp(controller, user);
                var openDocumentResult = OpenDocument(controller, 3);

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
            }
        }

        [Test]
        [Category("Automated")]
        [Category("WebAPIBackend")]
        public static void GetDocumentWordZones()
        {
            string dbName = "AppBackendAPI_Test_GetDocumentWordZones";

            try
            {
                var (fileProcessingDb, user, controller) = InitializeDBAndUser(dbName);

                LogInToWebApp(controller, user);
                var openDocumentResult = OpenDocument(controller, 3);

                var documentText = new SpatialString();
                documentText.LoadFrom(_testFileNames[2] + ".uss", false);

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
            }
        }

        [Test]
        [Category("Automated")]
        [Category("WebAPIBackend")]
        public static void AddGetComment()
        {
            string dbName = "AppBackendAPI_Test_AddGetComment";

            try
            {
                var (fileProcessingDb, user, controller) = InitializeDBAndUser(dbName);

                 LogInToWebApp(controller, user);
                 var openDocumentResult = OpenDocument(controller, 1);

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
        public static void SkipDocument()
        {
            string dbName = "AppBackendAPI_Test_SkipDocument";

            try
            {
                var (fileProcessingDb, user, controller) = InitializeDBAndUser(dbName);

                LogInToWebApp(controller, user);
                var openDocumentResult = OpenDocument(controller, 1);

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
        public static void FailDocument()
        {
            string dbName = "AppBackendAPI_Test_FailDocument";

            try
            {
                var (fileProcessingDb, user, controller) = InitializeDBAndUser(dbName);

                LogInToWebApp(controller, user);
                var openDocumentResult = OpenDocument(controller, 1);

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

        /// <summary>
        /// Test search results on first IDShield document. Combined what _ought_ to be multiple tests to save run time (setup/teardown are costly because of DB)
        /// </summary>
        [Test]
        [Category("Automated")]
        [Category("WebAPIBackend")]
        public static void PostSearch()
        {
            string dbName = "AppBackendAPI_Test_PostSearch";

            try
            {
                var (fileProcessingDb, user, controller) = InitializeDBAndUser(dbName);
                var docID = 1;

                LogInToWebApp(controller, user);
                OpenDocument(controller, docID);

                // Literal, ignore case
                var result = controller.PostSearch(docID, new SearchParameters { Query = "DOE", QueryType = QueryType.Literal, CaseSensitive = false, PageNumber = null, ResultType = "Name" });
                var res = result.AssertGoodResult<DocumentDataResult>();
                Assert.AreEqual(6, res.Attributes.Count);
                Assert.That(res.Attributes.TrueForAll(attribute => attribute.HasPositionInfo == true));
                Assert.That(res.Attributes.TrueForAll(attribute => attribute.Type == "Name"));
                Assert.That(res.Attributes.TrueForAll(attribute => attribute.ConfidenceLevel == "Manual"));
                Assert.That(res.Attributes.TrueForAll(attribute => attribute.Value == "DOE" || attribute.Value == "Doe"));

                // Literal, case sensitive
                result = controller.PostSearch(docID, new SearchParameters { Query = "DOE", QueryType = QueryType.Literal, CaseSensitive = true, PageNumber = null, ResultType = "Name" });
                res = result.AssertGoodResult<DocumentDataResult>();
                Assert.AreEqual(1, res.Attributes.Count);
                Assert.That(res.Attributes.TrueForAll(attribute => attribute.HasPositionInfo == true));
                Assert.That(res.Attributes.TrueForAll(attribute => attribute.Type == "Name"));
                Assert.That(res.Attributes.TrueForAll(attribute => attribute.ConfidenceLevel == "Manual"));
                Assert.That(res.Attributes.TrueForAll(attribute => attribute.Value == "DOE"));

                // Regex, ignore case
                result = controller.PostSearch(docID, new SearchParameters { Query = "D.E", QueryType = QueryType.Regex, CaseSensitive = false, PageNumber = null, ResultType = null });
                res = result.AssertGoodResult<DocumentDataResult>();
                Assert.AreEqual(7, res.Attributes.Count);
                Assert.That(res.Attributes.TrueForAll(attribute => attribute.HasPositionInfo == true));
                Assert.That(res.Attributes.TrueForAll(attribute => attribute.Type == ""));
                Assert.That(res.Attributes.TrueForAll(attribute => attribute.ConfidenceLevel == "Manual"));
                Assert.That(res.Attributes.TrueForAll(attribute => attribute.Value == "DOE" || attribute.Value == "Doe" || attribute.Value == "due"));

                // Regex, case sensitive
                result = controller.PostSearch(docID, new SearchParameters { Query = "[Dd].e", QueryType = QueryType.Regex, CaseSensitive = true, PageNumber = null, ResultType = "NameRegex" });
                res = result.AssertGoodResult<DocumentDataResult>();
                Assert.AreEqual(6, res.Attributes.Count);
                Assert.That(res.Attributes.TrueForAll(attribute => attribute.HasPositionInfo == true));
                Assert.That(res.Attributes.TrueForAll(attribute => attribute.Type == "NameRegex"));
                Assert.That(res.Attributes.TrueForAll(attribute => attribute.ConfidenceLevel == "Manual"));
                Assert.That(res.Attributes.TrueForAll(attribute => attribute.Value == "Doe" || attribute.Value == "due"));

                controller.Logout().AssertGoodResult<NoContentResult>();
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
        public static void PostSearchNoUSS()
        {
            string dbName = "AppBackendAPI_Test_PostSearch";

            try
            {
                var (fileProcessingDb, user, controller) = InitializeDBAndUser(dbName);
                var docID = 2;

                LogInToWebApp(controller, user);
                OpenDocument(controller, docID);

                // Literal, ignore case
                var result = controller.PostSearch(docID, new SearchParameters { Query = "DOE", QueryType = QueryType.Literal, CaseSensitive = false, PageNumber = null, ResultType = "Name" });
                result.AssertResultCode(404, "An error is expected with no uss file");
            }
            finally
            {
                FileApiMgr.ReleaseAll();
                _testDbManager.RemoveDatabase(dbName);
            }
        }

        /// <summary>
        /// Test search results on bad page number
        /// </summary>
        [Test]
        [Category("Automated")]
        [Category("WebAPIBackend")]
        public static void PostSearchBadPage()
        {
            string dbName = "AppBackendAPI_Test_PostSearch_BadPage";

            try
            {
                var (fileProcessingDb, user, controller) = InitializeDBAndUser(dbName);
                var docID = 1;

                LogInToWebApp(controller, user);
                OpenDocument(controller, docID);

                var result = controller.PostSearch(docID, new SearchParameters { Query = "DOE", QueryType = QueryType.Literal, CaseSensitive = false, PageNumber = 2, ResultType = "Name" });
                result.AssertResultCode(404, "Searching on page 2 of document 1 should be an error. See ISSUE-16673");

                result = controller.PostSearch(docID, new SearchParameters { Query = "DOE", QueryType = QueryType.Literal, CaseSensitive = false, PageNumber = 0, ResultType = "Name" });
                result.AssertResultCode(404, "Searching on page 0 should be an error. See ISSUE-16673");

                result = controller.PostSearch(docID, new SearchParameters { Query = "DOE", QueryType = QueryType.Literal, CaseSensitive = false, PageNumber = -2, ResultType = "Name" });
                result.AssertResultCode(404, "Searching on page -2 should be an error. See ISSUE-16673");

                controller.Logout().AssertGoodResult<NoContentResult>();
            }
            finally
            {
                FileApiMgr.ReleaseAll();
                _testDbManager.RemoveDatabase(dbName);
            }
        }

        /// <summary>
        /// Test search results on bad page number
        /// </summary>
        [Test]
        [Category("Automated")]
        [Category("WebAPIBackend")]
        public static void PostSearchBadResultType()
        {
            string dbName = "AppBackendAPI_Test_PostSearch_BadResultType";

            try
            {
                var (fileProcessingDb, user, controller) = InitializeDBAndUser(dbName);
                var docID = 1;

                LogInToWebApp(controller, user);
                OpenDocument(controller, docID);

                var result = controller.PostSearch(docID, new SearchParameters { Query = "DOE", QueryType = QueryType.Literal, CaseSensitive = false, PageNumber = 1, ResultType = "123" });
                result.AssertResultCode(400, "Searching with invalid result type should be a 400 error. See ISSUE-16673");

                controller.Logout().AssertGoodResult<NoContentResult>();
            }
            finally
            {
                FileApiMgr.ReleaseAll();
                _testDbManager.RemoveDatabase(dbName);
            }
        }

        /// <summary>
        /// Test search results with bad query
        /// </summary>
        [Test]
        [Category("Automated")]
        [Category("WebAPIBackend")]
        public static void PostSearchBadQuery()
        {
            string dbName = "AppBackendAPI_Test_PostSearch_BadQuery";

            try
            {
                var (fileProcessingDb, user, controller) = InitializeDBAndUser(dbName);
                var docID = 1;

                LogInToWebApp(controller, user);
                OpenDocument(controller, docID);

                var result = controller.PostSearch(docID, new SearchParameters { Query = "NotFoundOnDocument", QueryType = QueryType.Literal, CaseSensitive = false, PageNumber = null, ResultType = null });
                var res = result.AssertGoodResult<DocumentDataResult>();
                Assert.AreEqual(0, res.Attributes.Count);

                result = controller.PostSearch(docID, new SearchParameters { Query = "", QueryType = QueryType.Literal, CaseSensitive = false, PageNumber = null, ResultType = null });
                result.AssertResultCode(400, "Searching with empty pattern should be a 400 error. See ISSUE-16673");

                result = controller.PostSearch(docID, new SearchParameters { Query = null, QueryType = QueryType.Literal, CaseSensitive = false, PageNumber = null, ResultType = null });
                result.AssertResultCode(400, "Searching with null pattern should be a 400 error. See ISSUE-16673");

                result = controller.PostSearch(docID, new SearchParameters { Query = @"(", QueryType = QueryType.Regex, CaseSensitive = false, PageNumber = null, ResultType = null });
                result.AssertResultCode(400, "Searching with bad regex pattern should be a 400 error. See ISSUE-16673");

                controller.Logout().AssertGoodResult<NoContentResult>();
            }
            finally
            {
                FileApiMgr.ReleaseAll();
                _testDbManager.RemoveDatabase(dbName);
            }
        }


        /// <summary>
        /// Test search results on 1000 page document
        /// </summary>
        [Test]
        [Category("Automated")]
        [Category("WebAPIBackend")]
        public static void PostSearch1kPage()
        {
            string dbName = "AppBackendAPI_Test_PostSearch_1k";

            try
            {
                var (db, user, controller) = InitializeDBAndUser(dbName);
                var docID = 5;

                LogInToWebApp(controller, user);
                OpenDocument(controller, docID);

                // Literal, ignore case, first page
                var result = controller.PostSearch(docID, new SearchParameters { Query = "DOE", QueryType = QueryType.Literal, CaseSensitive = false, PageNumber = 500, ResultType = "Name" });
                var res = result.AssertGoodResult<DocumentDataResult>();
                Assert.AreEqual(6, res.Attributes.Count);
                Assert.That(res.Attributes.TrueForAll(attribute => attribute.HasPositionInfo == true));
                Assert.That(res.Attributes.TrueForAll(attribute => attribute.Type == "Name"));
                Assert.That(res.Attributes.TrueForAll(attribute => attribute.ConfidenceLevel == "Manual"));
                Assert.That(res.Attributes.TrueForAll(attribute => attribute.Value == "DOE" || attribute.Value == "Doe"));

                // Literal, ignore case, all pages
                result = controller.PostSearch(docID, new SearchParameters { Query = "DOE", QueryType = QueryType.Literal, CaseSensitive = false, PageNumber = null, ResultType = "Name" });
                res = result.AssertGoodResult<DocumentDataResult>();
                Assert.AreEqual(6000, res.Attributes.Count);
                Assert.That(res.Attributes.TrueForAll(attribute => attribute.HasPositionInfo == true));
                Assert.That(res.Attributes.TrueForAll(attribute => attribute.Type == "Name"));
                Assert.That(res.Attributes.TrueForAll(attribute => attribute.ConfidenceLevel == "Manual"));
                Assert.That(res.Attributes.TrueForAll(attribute => attribute.Value == "DOE" || attribute.Value == "Doe"));

                controller.Logout().AssertGoodResult<NoContentResult>();
            }
            finally
            {
                FileApiMgr.ReleaseAll();
                _testDbManager.RemoveDatabase(dbName);
            }
        }

        #endregion Public Test Functions

        
        private static void LogInToWebApp(AppBackendController controller, User user)
        {
            var result = controller.Login(user);
            var token = result.AssertGoodResult<JwtSecurityToken>();
            controller.ApplyTokenClaimPrincipalToContext(token);

            var sessionResult = controller.SessionLogin();
            var sessionToken = sessionResult.AssertGoodResult<JwtSecurityToken>();
            controller.ApplyTokenClaimPrincipalToContext(sessionToken);
        }

        private static DocumentIdResult OpenDocument(AppBackendController controller, int docID)
        {
            var result = controller.OpenDocument(docID);
            return result.AssertGoodResult<DocumentIdResult>();
        }

        private static (FileProcessingDB, User, AppBackendController) InitializeDBAndUser(string dbName, string username = "jane_doe", string password = "123")
        {
            var (fileProcessingDb, user, controller) =
                _testDbManager.InitializeEnvironment<TestBackendAPI, AppBackendController>
                        ("Resources.Demo_IDShield.bak", dbName, username, password);


            var actionID = fileProcessingDb.GetActionIDForWorkflow(_VERIFY_ACTION, fileProcessingDb.GetWorkflowID("CourtOffice"));

            for (int i = 0; i < _testFileArray.Length; i++)
            {
                string testFileName = _testFiles.GetFile(_testFileArray[i]);
                _testFileNames[i] = testFileName;
                if (_testFileInDB[i])
                {
                    fileProcessingDb.RenameFile(new FileRecordClass { ActionID = actionID, FileID = i + 1, Name = _testFileOriginalNames[i] }, testFileName);
                }
                else
                {
                    fileProcessingDb.AddFile(
                        testFileName, _VERIFY_ACTION, 1, EFilePriority.kPriorityNormal, false, false,
                        EActionStatus.kActionPending, false, out bool t1, out EActionStatus t2);
                }

                if (_testFileHasUSS[i])
                {
                    _testFiles.GetFile(_testFileArray[i] + ".uss", testFileName + ".uss");
                }

                if (_testFileHasVOA[i])
                {
                    _testFiles.GetFile(_testFileArray[i] + ".voa", testFileName + ".voa");
                }
            }

            return (fileProcessingDb, user, controller);
        }
    }
}
