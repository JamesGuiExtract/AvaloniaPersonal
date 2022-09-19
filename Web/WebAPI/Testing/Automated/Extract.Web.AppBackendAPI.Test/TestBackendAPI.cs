using Extract.FileActionManager.Database.Test;
using Extract.FileActionManager.FileProcessors;
using Extract.Imaging;
using Extract.Testing.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
    [Category("AppBackendAPI")]
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

        static readonly string _STORE_ATTRIBUTE_GUID = typeof(StoreAttributesInDBTask).GUID.ToString();

        static readonly string _COMPUTE_ACTION = "Compute";
        static readonly string _VERIFY_ACTION = "Verify";

        // TestCaseSource for ProcessAnnotationConfirmShrink 
        static string[] _attributeNames = new[] { "HCData", "MCData", "LCData", "Manual" };

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

        [OneTimeSetUp]
        public static void Setup()
        {
            GeneralMethods.TestSetup();

            _testFiles = new TestFileManager<TestBackendAPI>();
            _testDbManager = new FAMTestDBManager<TestBackendAPI>();
        }

        [OneTimeTearDown]
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
        public static void LoginLogout()
        {
            string dbName = "Test_AppBackendAPI_LoginLogout";

            try
            {
                var (fileProcessingDb, user, controller) = InitializeDBAndUser(dbName, _testFiles);

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
                FileApiMgr.Instance.ReleaseAll();
                _testDbManager.RemoveDatabase(dbName);
            }
        }

        [Test]
        [Category("Automated")]
        public static void LoginBlankUserName()
        {
            string dbName = "Test_AppBackendAPI_LoginBlankUserName";

            try
            {
                var (fileProcessingDb, user, controller) = InitializeDBAndUser(dbName, _testFiles, "", "123");

                Assert.AreEqual(((ErrorResult)(((ObjectResult)controller.Login(user)).Value)).Error.Message, "Username is empty");
            }
            finally
            {
                FileApiMgr.Instance.ReleaseAll();
                _testDbManager.RemoveDatabase(dbName);
            }
        }

        [Test]
        [Category("Automated")]
        public static void SetMetadataValue()
        {
            string dbName = "Test_AppBackendAPI_SetMetadataValue";

            try
            {
                var (fileProcessingDb, user, controller) = InitializeDBAndUser(dbName, _testFiles);
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
                FileApiMgr.Instance.ReleaseAll();
                _testDbManager.RemoveDatabase(dbName);
            }
        }

        [Test]
        [Category("Automated")]
        public static void ChangePassword()
        {
            string dbName = "Test_AppBackendAPI_ChangePassword";

            try
            {
                var (fileProcessingDb, user, controller) = InitializeDBAndUser(dbName, _testFiles);

                LogInToWebApp(controller, user);

                var result = controller.ChangePassword("123", "F1234567");
                result.AssertResultCode(200, "Failed to change password");
                controller.Logout();
                user.Password = "F1234567";

                LogInToWebApp(controller, user);
            }
            finally
            {
                FileApiMgr.Instance.ReleaseAll();
                _testDbManager.RemoveDatabase(dbName);
            }
        }

        [Test]
        [Category("Automated")]
        public static void ChangePasswordNoSessionToken()
        {
            string dbName = "Test_AppBackendAPI_ChangePassword";

            try
            {
                var (fileProcessingDb, user, controller) = InitializeDBAndUser(dbName, _testFiles);

                controller.Login(user);

                var result = controller.ChangePassword("123", "F1234567");
                result.AssertResultCode(500, "You should not be able to change your password without a session token");
                controller.Logout();
            }
            finally
            {
                FileApiMgr.Instance.ReleaseAll();
                _testDbManager.RemoveDatabase(dbName);
            }
        }

        [Test]
        [Category("Automated")]
        public static void ChangePasswordInvalidPassword()
        {
            string dbName = "Test_AppBackendAPI_ChangePasswordInvalidPassword";

            try
            {
                var (fileProcessingDb, user, controller) = InitializeDBAndUser(dbName, _testFiles);

                LogInToWebApp(controller, user);

                var result = controller.ChangePassword("a", "F1234567");
                result.AssertResultCode(500, "Attempting to change a password with an invalid password should yield a failure");
            }
            finally
            {
                FileApiMgr.Instance.ReleaseAll();
                _testDbManager.RemoveDatabase(dbName);
            }
        }

        [Test]
        [Category("Automated")]
        public static void GetNullMetadataValue()
        {
            string dbName = "Test_AppBackendAPI_GetNullMetadataValue";

            try
            {
                var (fileProcessingDb, user, controller) = InitializeDBAndUser(dbName, _testFiles);
                fileProcessingDb.AddMetadataField("DocumentType");

                LogInToWebApp(controller, user);
                var openDocumentResult = OpenDocument(controller, 1);

                var documenttype = controller.GetMetadataField(openDocumentResult.Id, "DocumentType").AssertGoodResult<MetadataFieldResult>().Value;
                Assert.AreEqual(null, documenttype, "DocumentType should default to null");
            }
            finally
            {
                FileApiMgr.Instance.ReleaseAll();
                _testDbManager.RemoveDatabase(dbName);
            }
        }

        [Test]
        [Category("Automated")]
        public static void LoginBlankPassword()
        {
            string dbName = "Test_AppBackendAPI_LoginBlankUserName";

            try
            {
                var (fileProcessingDb, user, controller) = InitializeDBAndUser(dbName, _testFiles, "jane_doe", "");

                Assert.AreEqual(((ErrorResult)(((ObjectResult)controller.Login(user)).Value)).Error.Message, "Password is empty");
            }
            finally
            {
                FileApiMgr.Instance.ReleaseAll();
                _testDbManager.RemoveDatabase(dbName);
            }
        }

        [Test]
        [Category("Automated")]
        public static void SessionLoginLogout()
        {
            string dbName = "Test_AppBackendAPI_SessionLoginLogout";

            try
            {
                var (fileProcessingDb, user, controller) = InitializeDBAndUser(dbName, _testFiles);

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
                FileApiMgr.Instance.ReleaseAll();
                _testDbManager.RemoveDatabase(dbName);
            }
        }

        [Test]
        [Category("Automated")]
        public static void GetSettings()
        {
            string dbName = "Test_AppBackendAPI_GetSettings";
            var temporaryDocType = Path.GetTempFileName();

            try
            {
                File.WriteAllText(temporaryDocType, "Ambulance - Encounter \nAmbulance - Patient \nAnesthesia \nAppeal Request");
                var (fileProcessingDb, user, controller) = InitializeDBAndUser(dbName, _testFiles);
                var newSettings = $"{{ \"RedactionTypes\": [\"SSN\", \"DOB\"], \"DocumentTypes\": \"{temporaryDocType.Replace(@"\", @"\\")}\" }}";
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

                controller.Logout()
                    .AssertGoodResult<NoContentResult>();
            }
            finally
            {
                File.Delete(temporaryDocType);
                FileApiMgr.Instance.ReleaseAll();
                _testDbManager.RemoveDatabase(dbName);
            }
        }

        [Test]
        [Category("Automated")]
        public static void GetSettings_PasswordComplexityRequirements()
        {
            string dbName = "Test_AppBackendAPI_GetSettings_PasswordComplexityRequirements";
            try
            {
                var (fileProcessingDb, user, controller) = InitializeDBAndUser(dbName, _testFiles);

                // Get the settings from upgraded database
                var settings = controller.GetSettings().AssertGoodResult<WebAppSettingsResult>();

                // Requirements on old databases are just that the length isn't zero
                Assert.AreEqual("1", settings.PasswordComplexityRequirements.EncodeRequirements());

                // Set the password requirements to be more strict
                fileProcessingDb.SetDBInfoSetting("PasswordComplexityRequirements", "5ULD", true, false);

                // Get the updated requirements and verify they match
                settings = controller.GetSettings().AssertGoodResult<WebAppSettingsResult>();
                Assert.AreEqual("5ULD", settings.PasswordComplexityRequirements.EncodeRequirements());

                controller.Logout().AssertGoodResult<NoContentResult>();
            }
            finally
            {
                FileApiMgr.Instance.ReleaseAll();
                _testDbManager.RemoveDatabase(dbName);
            }
        }

        [Test]
        [Category("Automated")]
        public static void OpenDocument_NoID_UserSpecificQueue()
        {
            string dbName = "Test_AppBackendAPI_OpenDocument_NoID";

            try
            {
                var (fileProcessingDb, user, controller) = InitializeDBAndUser(dbName, _testFiles);
                var newSettings = JsonConvert.SerializeObject(new WebAppSettingsResult { EnableAllPendingQueue = false });

                fileProcessingDb.ExecuteCommandQuery($"UPDATE [dbo].[WebAppConfig] SET SETTINGS = '{newSettings}' WHERE TYPE = 'RedactionVerificationSettings'");

                // This is a hacky solution for the unit test because actually assigning them via a user specific queue fam
                // will auotmatically create the user.
                fileProcessingDb.ExecuteCommandQuery(@"INSERT INTO dbo.FAMUser(UserName) VALUES ('jon_doe'), ('jane_doe')");
                fileProcessingDb.ExecuteCommandQuery($"UPDATE dbo.FileActionStatus SET UserID = 5");

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

                // Moving all documents to a different user.
                fileProcessingDb.ExecuteCommandQuery($"UPDATE dbo.FileActionStatus SET UserID = 2");
                result = controller.OpenDocument();
                
                openDocumentResult = result.AssertGoodResult<DocumentIdResult>();
                // Since the documents were moved to another user, we should not be able to get another from the queue.
                Assert.AreEqual(-1, openDocumentResult.Id);

                controller.Logout()
                    .AssertGoodResult<NoContentResult>();
            }
            finally
            {
                FileApiMgr.Instance.ReleaseAll();
                _testDbManager.RemoveDatabase(dbName);
            }
        }

        [Test]
        [Category("Automated")]
        public static void OpenDocument_NoID()
        {
            string dbName = "Test_AppBackendAPI_OpenDocument_NoID";

            try
            {
                var (fileProcessingDb, user, controller) = InitializeDBAndUser(dbName, _testFiles);

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
                FileApiMgr.Instance.ReleaseAll();
                _testDbManager.RemoveDatabase(dbName);
            }
        }

        [Test]
        [Category("Automated")]
        public static void OpenDocument_WithID()
        {
            string dbName = "Test_AppBackendAPI_OpenDocument_WithID";

            try
            {
                var (fileProcessingDb, user, controller) = InitializeDBAndUser(dbName, _testFiles);

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
                FileApiMgr.Instance.ReleaseAll();
                _testDbManager.RemoveDatabase(dbName);
            }
        }

        /// <summary>
        /// Tests that different controllers don't end up being assigned the same document
        /// when requesting the next open document repeatedly at about the same time.
        /// https://extract.atlassian.net/browse/ISSUE-16852
        /// </summary>
        [Test]
        [Category("Automated")]
        public static void OpenDocument_ConcurrentSessions()
        {
            string dbName = "Test_AppBackendAPI_OpenDocument_ConcurrentSessions";

            try
            {
                // This test uses two different controller instances (each with its own FAM session)
                // to repeatedly try to request the next open document within a fraction of a second of
                // each other (slightly altering the timing each time).

                // (User1 = jane_doe)
                var (fileProcessingDb, user1, controller1) = InitializeDBAndUser(dbName, _testFiles);
                LogInToWebApp(controller1, user1);

                User user2 = ApiTestUtils.CreateUser("jon_doe", "123");
                var controller2 = SetupController(user2);
                LogInToWebApp(controller2, user2);

                var rng = new Random();
                var totalDocCount = _testFileArray.Length;

                for (int i = 0; i < 100; i++)
                {
                    Func<AppBackendController, Task<(int FileID, int SessionID)>> openDoc = async (controller) =>
                    {
                        // Ask for next available document, and get the document (FileTask) session ID associated
                        // with each document. Use slightly varying delays before each call to ensure the order
                        // for which the calls are happening between the two controllers vary.
                        await Task.Delay(rng.Next(0, 100));
                        var idResult = controller.OpenDocument(-1, processSkipped: false)
                            .AssertGoodResult<DocumentIdResult>();
                        await Task.Delay(rng.Next(0, 100));
                        var sessionId = controller.GetActiveDocumentSessionId();
                        return (FileID: idResult.Id, SessionID: sessionId);
                    };

                    var openDoc1 = openDoc(controller1);
                    var openDoc2 = openDoc(controller2);
                    Task.WaitAll(openDoc1, openDoc2);

                    //  Prior to the fix for ISSUE-16852, the following check would fail within a few iterations
                    Assert.AreNotEqual(openDoc1.Result.FileID, openDoc2.Result.FileID,
                        "Same file grabbed by both controllers");
                    // This check was not failing, but good to confirm they are different nonetheless.
                    Assert.AreNotEqual(openDoc1.Result.SessionID, openDoc2.Result.SessionID,
                        "Same session ID being used by both controllers");

                    // Confirm there are now 2 fewer documents indicated as pending than originally.
                    var queueStatus = controller1.GetQueueStatus()
                        .AssertGoodResult<QueueStatusResult>();
                    Assert.AreEqual(totalDocCount - 2, queueStatus.PendingDocuments);

                    // Close the documents without committing to return them to the queue.
                    controller1.CloseDocument(openDoc1.Result.FileID, commit: false)
                        .AssertGoodResult<NoContentResult>();
                    controller2.CloseDocument(openDoc2.Result.FileID, commit: false)
                        .AssertGoodResult<NoContentResult>();
                }

                controller1.Logout()
                    .AssertGoodResult<NoContentResult>();
                controller2.Logout()
                    .AssertGoodResult<NoContentResult>();
            }
            finally
            {
                FileApiMgr.Instance.ReleaseAll();
                _testDbManager.RemoveDatabase(dbName);
            }
        }

        [Test]
        [Category("Automated")]
        public static void GetQueueStatus()
        {
            string dbName = "Test_AppBackendAPI_GetQueueStatus";

            try
            {
                var (fileProcessingDb, user, controller) = InitializeDBAndUser(dbName, _testFiles);
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
                var controller2 = SetupController(user2);

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
                FileApiMgr.Instance.ReleaseAll();
                _testDbManager.RemoveDatabase(dbName);
            }
        }

        [Test]
        [Category("Automated")]
        public static void ReOpenDocument()
        {
            string dbName = "Test_AppBackendAPI_ReOpenDocument";

            try
            {
                var (fileProcessingDb, user, controller) = InitializeDBAndUser(dbName, _testFiles);

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
                result = controller.OpenDocument(2);
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
                FileApiMgr.Instance.ReleaseAll();
                _testDbManager.RemoveDatabase(dbName);
            }
        }

        [Test]
        [Category("Automated")]
        public static void LogoutClosesDocument()
        {
            string dbName = "Test_AppBackendAPI_LogoutClosesDocument";

            try
            {
                var (fileProcessingDb, user, controller) = InitializeDBAndUser(dbName, _testFiles);

                LogInToWebApp(controller, user);
                OpenDocument(controller, 1);

                Assert.AreEqual(EActionStatus.kActionProcessing, fileProcessingDb.GetFileStatus(1, _VERIFY_ACTION, false));

                controller.Logout()
                    .AssertGoodResult<NoContentResult>();

                Assert.AreEqual(EActionStatus.kActionPending, fileProcessingDb.GetFileStatus(1, _VERIFY_ACTION, false));
            }
            finally
            {
                FileApiMgr.Instance.ReleaseAll();
                _testDbManager.RemoveDatabase(dbName);
            }
        }

        [Test]
        [Category("Automated")]
        public static void AbandonedSession()
        {
            string dbName = "Test_AppBackendAPI_AbandonedSession";

            try
            {
                var (fileProcessingDb, user, controller) = InitializeDBAndUser(dbName, _testFiles);

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
                FileApiMgr.Instance.ReleaseAll();

                // Reset the default context to ensure no crossover of session IDs.
                ApiTestUtils.SetDefaultApiContext(ApiContext.CURRENT_VERSION, dbName);
                var controller2 = SetupController(user);

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
                FileApiMgr.Instance.ReleaseAll();
                _testDbManager.RemoveDatabase(dbName);
            }
        }

        [Test]
        [Category("Automated")]
        public static void GetPageInfo_NoUSS()
        {
            string dbName = "Test_AppBackendAPI_GetPageInfo_NoUSS";

            try
            {
                var (fileProcessingDb, user, controller) = InitializeDBAndUser(dbName, _testFiles);
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
                FileApiMgr.Instance.ReleaseAll();
                _testDbManager.RemoveDatabase(dbName);
            }
        }

        [Test]
        [Category("Automated")]
        public static void ProcessAnnotation()
        {
            string dbName = "Test_AppBackendAPI_ProcessAnnotation";

            try
            {
                var (fileProcessingDb, user, controller) = InitializeDBAndUser(dbName, _testFiles);

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
                FileApiMgr.Instance.ReleaseAll();
                _testDbManager.RemoveDatabase(dbName);
            }
        }

        [Test]
        [Category("Automated")]
        [TestCaseSource("_attributeNames")]
        public static void ProcessAnnotationConfirmShrink(string attributeName)
        {
            string dbName = "Test_AppBackendAPI_ProcessAnnotation";

            try
            {
                var (fileProcessingDb, user, controller) = InitializeDBAndUser(dbName, _testFiles);

                LogInToWebApp(controller, user);
                var openDocumentResult = OpenDocument(controller, 3);
                Assert.AreEqual(3, openDocumentResult.Id);

                string voaFile = _testFileNames[2] + ".voa";
                var voa = new IUnknownVectorClass();
                voa.LoadFrom(voaFile, false);
                var mapper = new AttributeMapper(voa, EWorkflowType.kExtraction);
                var documentAttribute = mapper.MapAttribute((IAttribute)voa.At(1), true);
                documentAttribute.Name = attributeName;

                // Increase height so that the zone encloses some empty page
                var heightBefore = 500;
                documentAttribute.SpatialPosition.LineInfo[0].SpatialLineZone.Height = heightBefore;

                var processAnnotationResult = controller.ProcessAnnotation(openDocumentResult.Id, 1, new ProcessAnnotationParameters() { Annotation = documentAttribute, Definition = "{ AutoShrinkRedactionZones: {} }", OperationType = "modify" });
                processAnnotationResult.AssertResultCode(200, "ProcessAnnotation is failing");

                var modifiedAttribute = ((OkObjectResult)processAnnotationResult).Value as DocumentAttribute;
                var heightAfter = modifiedAttribute.SpatialPosition.LineInfo[0].SpatialLineZone.Height;

                // Ensure the attribute is significantly smaller
                Assert.Less(heightAfter, 400);

                controller.Logout().AssertGoodResult<NoContentResult>();
            }
            finally
            {
                FileApiMgr.Instance.ReleaseAll();
                _testDbManager.RemoveDatabase(dbName);
            }
        }

        [Test]
        [Category("Automated")]
        public static void GetPageInfo_WithUSS()
        {
            string dbName = "Test_AppBackendAPI_GetPageInfo_WithUSS";

            try
            {
                var (fileProcessingDb, user, controller) = InitializeDBAndUser(dbName, _testFiles);
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
                FileApiMgr.Instance.ReleaseAll();
                _testDbManager.RemoveDatabase(dbName);
            }
        }

        /// <summary>
        /// GET GetDocumentPage/{Id}/{Page}
        /// </summary>
        [Test]
        [Category("Automated")]
        public static void GetPageImage()
        {
            string dbName = "Test_AppBackendAPI_GetPageImage";

            try
            {
                var (fileProcessingDb, user, controller) = InitializeDBAndUser(dbName, _testFiles);

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
                FileApiMgr.Instance.ReleaseAll();
                _testDbManager.RemoveDatabase(dbName);

                _testFiles.RemoveFile(_TEST_FILE3);
            }
        }

        [Test]
        [Category("Automated")]
        public static void GetDocumentData()
        {
            string dbName = "Test_AppBackendAPI_GetDocumentData";

            try
            {
                var (fileProcessingDb, user, controller) = InitializeDBAndUser(dbName, _testFiles);

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
                FileApiMgr.Instance.ReleaseAll();
                _testDbManager.RemoveDatabase(dbName);
            }
        }

        [Test]
        [Category("Automated")]
        public static void UpdateDocumentData()
        {
            string dbName = "Test_AppBackendAPI_GetDocumentData";
            string attributeSetName = "Attr";
            int docID = 3;

            try
            {
                var (db, user, controller) = InitializeDBAndUser(dbName, _testFiles);

                LogInToWebApp(controller, user);
                OpenDocument(controller, docID);

                var result = controller.GetDocumentData(3);
                var attributeSet = result.AssertGoodResult<DocumentDataResult>();

                var originalAttributeIDs = attributeSet.Attributes
                    .Select(attribute => new Guid(attribute.ID))
                    .ToList();

                var page1Data = new List<DocumentAttribute>(attributeSet.Attributes
                    .Where(attribute => attribute.HasPositionInfo == true
                        && attribute.SpatialPosition.Pages.SequenceEqual(new[] { 1 })));
                Assert.AreEqual(2, page1Data.Count);

                // Tests the following changes by editing data per-page, then committing:
                // 1) Delete the 1st of the two attributes on page 1
                // 2) Edit the name and type of the remaining attribute
                // 3) Add a new attribute on page 3

                page1Data.RemoveAt(0);

                var attributeToEdit = page1Data.Single();
                attributeToEdit.Value = dbName;
                attributeToEdit.Type = dbName;

                var updatedAttributeIDs = new List<Guid>();
                updatedAttributeIDs.Add(new Guid(attributeToEdit.ID));

                controller.EditPageData(docID, 1, page1Data)
                    .AssertGoodResult<NoContentResult>();

                var page3Data = new List<DocumentAttribute>()
                {
                    ApiTestUtils.CreateDocumentAttribute(dbName, dbName, 3, 100, 100, 200, 100, 100)
                };
                updatedAttributeIDs.Add(new Guid(page3Data.Single().ID));

                controller.EditPageData(docID, 3, page3Data)
                    .AssertGoodResult<NoContentResult>();

                var attributeMgr = db.GetAttributeDBMgr();
                var storedAttributes = attributeMgr.GetAttributeSetForFile(
                        docID, attributeSetName, -1, true)
                    .ToIEnumerable<IAttribute>()
                    .ToList();

                var storedAttributeIDs = storedAttributes
                    .OfType<IAttribute>()
                    .Where(attribute => attribute.Value.HasSpatialInfo())
                    .OfType<IIdentifiableObject>()
                    .Select(storedAttribute => storedAttribute.InstanceGUID);

                // At this point official attribute set in db should still be un-edited attributes.
                Assert.IsTrue(storedAttributeIDs.SequenceEqual(originalAttributeIDs));

                controller.CommitDocumentData(docID)
                    .AssertGoodResult<NoContentResult>();

                storedAttributes = attributeMgr.GetAttributeSetForFile(
                        docID, attributeSetName, -1, true)
                    .ToIEnumerable<IAttribute>()
                    .ToList();

                storedAttributeIDs = storedAttributes
                    .OfType<IAttribute>()
                    .Where(attribute => attribute.Value.HasSpatialInfo())
                    .OfType<IIdentifiableObject>()
                    .Select(storedAttribute => storedAttribute.InstanceGUID);

                // Following CommitDocumentData, we should instead find the updatedAttributeIDs in the database.
                Assert.IsTrue(storedAttributeIDs.SequenceEqual(updatedAttributeIDs));

                // Ensure there is only one attribute on the first page and that the edits are there.
                var storedPage1Attribute = storedAttributes
                    .Where(attribute => attribute.Value.HasSpatialInfo()
                        && attribute.Value.GetFirstPageNumber() == 1)
                    .Single();
                Assert.AreEqual(dbName, storedPage1Attribute.Value.String);
                Assert.AreEqual(dbName, storedPage1Attribute.Type);

                // Ensure we find the attribute added to the 3rd page with the correct spatial info.
                var storedPage3Attribute = storedAttributes
                    .Where(attribute => attribute.Value.HasSpatialInfo()
                        && attribute.Value.GetFirstPageNumber() == 3)
                    .Single();
                var rasterZone = storedPage3Attribute.Value.GetOCRImageRasterZones()
                    .ToIEnumerable<IRasterZone>()
                    .Single();
                Assert.AreEqual(100, rasterZone.StartX);
                Assert.AreEqual(100, rasterZone.StartY);
                Assert.AreEqual(200, rasterZone.EndX);
                Assert.AreEqual(100, rasterZone.EndY);
                Assert.AreEqual(100, rasterZone.Height);

                controller.Logout()
                    .AssertGoodResult<NoContentResult>();
            }
            finally
            {
                FileApiMgr.Instance.ReleaseAll();
                _testDbManager.RemoveDatabase(dbName);
            }
        }

        [Test]
        [Category("Automated")]
        public static void UncommittedData()
        {
            string dbName = "Test_AppBackendAPI_UncommittedData";
            string attributeSetName = "Attr";
            int docID = 3;

            try
            {
                var (db, user, controller) = InitializeDBAndUser(dbName, _testFiles);

                // Tests several scenarios involving pages updated via EditDocumentData where data
                // is not applied via CommitDocumentData before the document is closed:
                // 1) Edit page 1 and 3 with out committing. Confirm edits not applied to result of
                //      GetDocumentData, but edits can be retrieved via GetUncommittedDocumentData
                // 2) Confirm uncommitted edits can be deleted via DeleteUncommittedDocumentData
                // 3) Confirm uncommitted edits retrieved from GetUncommittedDocumentData
                //      can be applied/committed later.
                // 4) Confirm GetUncommittedDocumentData ignores edits made prior to a new
                //      attribute set being stored for the document.

                LogInToWebApp(controller, user);
                OpenDocument(controller, docID);

                var uncommittedData = controller.GetUncommittedDocumentData(docID)
                    .AssertGoodResult<UncommittedDocumentDataResult>();
                Assert.AreEqual(0, uncommittedData.UncommittedPagesOfAttributes.Count);

                var result = controller.GetDocumentData(docID);
                var attributeSet = result.AssertGoodResult<DocumentDataResult>();

                var page1Data = new List<DocumentAttribute>(attributeSet.Attributes
                    .Where(attribute => attribute.HasPositionInfo == true
                        && attribute.SpatialPosition.Pages.SequenceEqual(new[] { 1 })));
                Assert.AreEqual(2, page1Data.Count);

                page1Data.RemoveAt(0);

                var attributeToEdit = page1Data.Single();
                attributeToEdit.Value = dbName;
                attributeToEdit.Type = dbName;

                controller.EditPageData(docID, 1, page1Data)
                    .AssertGoodResult<NoContentResult>();

                var page3Data = new List<DocumentAttribute>()
                {
                    ApiTestUtils.CreateDocumentAttribute(dbName, dbName, 3, 100, 100, 200, 100, 100)
                };

                controller.EditPageData(docID, 3, page3Data)
                    .AssertGoodResult<NoContentResult>();

                controller.CloseDocument(docID, false)
                    .AssertGoodResult<NoContentResult>();
                controller.Logout()
                    .AssertGoodResult<NoContentResult>();

                User user2 = ApiTestUtils.CreateUser("jon_doe", "123");
                var controller2 = SetupController(user2);
                LogInToWebApp(controller2, user2);
                OpenDocument(controller2, docID);

                var documentData = controller2.GetDocumentData(docID)
                    .AssertGoodResult<DocumentDataResult>();
                Assert.AreEqual(2, documentData.Attributes.Count);
                Assert.AreNotEqual(dbName, documentData.Attributes.First().Value);

                uncommittedData = controller2.GetUncommittedDocumentData(docID)
                    .AssertGoodResult<UncommittedDocumentDataResult>();
                Assert.AreEqual(2, uncommittedData.UncommittedPagesOfAttributes.Count);
                var firstUncommittedPage = uncommittedData.UncommittedPagesOfAttributes[0];
                Assert.AreEqual(1, firstUncommittedPage.PageNumber);
                Assert.AreEqual(dbName, firstUncommittedPage.Attributes.Single().Value);
                var secondUncommittedPage = uncommittedData.UncommittedPagesOfAttributes[1];
                Assert.AreEqual(3, secondUncommittedPage.PageNumber);
                Assert.AreEqual(page3Data.Single().ID, secondUncommittedPage.Attributes.Single().ID);

                // 2) Confirm uncommitted edits can be deleted via DeleteUncommittedDocumentData

                controller2.DeleteOldCacheData(docID)
                    .AssertGoodResult<NoContentResult>();

                uncommittedData = controller2.GetUncommittedDocumentData(docID)
                    .AssertGoodResult<UncommittedDocumentDataResult>();
                Assert.AreEqual(0, uncommittedData.UncommittedPagesOfAttributes.Count);

                // 3) Confirm uncommitted edits retrieved from GetUncommittedDocumentData
                //      can be applied/committed later.

                controller2.EditPageData(docID, 1, firstUncommittedPage.Attributes)
                    .AssertGoodResult<NoContentResult>();

                controller2.CommitDocumentData(docID)
                    .AssertGoodResult<NoContentResult>();

                controller2.CloseDocument(docID, false);
                OpenDocument(controller2, docID);

                documentData = controller2.GetDocumentData(docID)
                    .AssertGoodResult<DocumentDataResult>();
                Assert.AreEqual(dbName, documentData.Attributes.Single().Value);

                // 3) Confirm uncommitted edits retrieved from GetUncommittedDocumentData
                //      can be applied/committed later.

                controller2.EditPageData(docID, 3, secondUncommittedPage.Attributes)
                    .AssertGoodResult<NoContentResult>();

                controller2.CloseDocument(docID, false);

                LogInToWebApp(controller, user);
                OpenDocument(controller, docID);

                documentData = controller.GetDocumentData(docID)
                    .AssertGoodResult<DocumentDataResult>();
                Assert.AreEqual(dbName, documentData.Attributes[0].Value);

                // 4) Confirm GetUncommittedDocumentData ignores edits made prior to a new
                //      attribute set being stored for the document.

                uncommittedData = controller.GetUncommittedDocumentData(docID)
                    .AssertGoodResult<UncommittedDocumentDataResult>();
                Assert.AreEqual(3, uncommittedData.UncommittedPagesOfAttributes.Single().PageNumber);

                var attributeMgr = db.GetAttributeDBMgr();
                var actionId = db.GetActionID(_COMPUTE_ACTION);
                db.RecordFAMSessionStart("", _COMPUTE_ACTION, false, true);
                int sessionId = db.StartFileTaskSession(_STORE_ATTRIBUTE_GUID, docID, actionId);
                var storedAttributes = attributeMgr.GetAttributeSetForFile(docID, attributeSetName, -1, true);
                attributeMgr.CreateNewAttributeSetForFile(sessionId, attributeSetName, storedAttributes, false, false, false, false);
                db.EndFileTaskSession(sessionId, 0, 0, false);
                db.RecordFAMSessionStop();

                uncommittedData = controller.GetUncommittedDocumentData(docID)
                    .AssertGoodResult<UncommittedDocumentDataResult>();
                Assert.AreEqual(0, uncommittedData.UncommittedPagesOfAttributes.Count);

                controller.Logout()
                   .AssertGoodResult<NoContentResult>();
                controller2.Logout()
                   .AssertGoodResult<NoContentResult>();
            }
            finally
            {
                FileApiMgr.Instance.ReleaseAll();
                _testDbManager.RemoveDatabase(dbName);
            }
        }

        /// <summary>
        /// Tests that on refresh, only updated data is retrieved by GetUncommittedData
        /// (whether on initial load or a simulated refresh of the browser)
        /// https://extract.atlassian.net/browse/ISSUE-16827
        /// </summary>
        [Test]
        [Category("Automated")]
        public static void UncommittedDataWhenRefreshed()
        {
            string dbName = "Test_AppBackendAPI_UncommittedDataWhenRefreshed";

            try
            {
                var (db, user, controller) = InitializeDBAndUser(dbName, _testFiles);
                var docID = 3;
                var page = 1;

                LogInToWebApp(controller, user);

                // Test steps
                // 1) Open a document, update an attribute, close document without committing
                // 2) Re-open the document; confirm modification from #1 is available via UncommittedData but not DocumentData
                // 3) Update the same attribute with different modification
                // 4) Without first closing the document call GetDocumentData again (as a browser refresh would do) and confirm that
                //    the edit from #3 has not been overwritten in the UncommittedData result.

                controller.OpenDocument(docID)
                    .AssertGoodResult<DocumentIdResult>();

                var docDataResult = controller.GetDocumentData(docID)
                    .AssertGoodResult<DocumentDataResult>();
                var pageAttribute = docDataResult.Attributes
                    .First(attribute => attribute.HasPositionInfo == true
                        && attribute.SpatialPosition.Pages.SequenceEqual(new[] { page }));
                string attributeID = pageAttribute.ID;
                string originalValue = pageAttribute.Value;

                var uncommittedData = controller.GetUncommittedDocumentData(docID)
                    .AssertGoodResult<UncommittedDocumentDataResult>();
                Assert.AreEqual(0, uncommittedData.UncommittedPagesOfAttributes.Count);
                pageAttribute.Value = "First Modification";
                controller.EditPageData(docID, page, new[] { pageAttribute }.ToList())
                    .AssertGoodResult<NoContentResult>();

                controller.CloseDocument(docID, false)
                    .AssertGoodResult<NoContentResult>();

                // 2) Re-open the document; confirm modification from #1 is available via UncommittedData but not DocumentData
                controller.OpenDocument(docID)
                    .AssertGoodResult<DocumentIdResult>();
                var docDataResult2 = controller.GetDocumentData(docID)
                    .AssertGoodResult<DocumentDataResult>();
                var pageAttribute2 = docDataResult2.Attributes
                    .First(attribute => attribute.ID == pageAttribute.ID
                        && attribute.SpatialPosition.Pages.SequenceEqual(new[] { page }));
                Assert.AreEqual(originalValue, pageAttribute2.Value);

                var uncommittedData2 = controller.GetUncommittedDocumentData(docID)
                    .AssertGoodResult<UncommittedDocumentDataResult>();
                Assert.AreEqual("First Modification",
                    uncommittedData2
                    .UncommittedPagesOfAttributes.Single()
                    .Attributes.Single()
                    .Value);

                // 3) Update the same attribute with different modification
                pageAttribute2.Value = "Second Modification";
                controller.EditPageData(docID, page, new[] { pageAttribute2 }.ToList())
                    .AssertGoodResult<NoContentResult>();

                // 4) Without first closing the document call GetDocumentData again (as a browser refresh would do) and confirm that
                //    the edit from #3 has not been overwritten in the UncommittedData result.
                controller.GetDocumentData(docID)
                    .AssertGoodResult<DocumentDataResult>();
                var uncommittedData3 = controller.GetUncommittedDocumentData(docID)
                    .AssertGoodResult<UncommittedDocumentDataResult>();
                Assert.AreEqual("Second Modification",
                    uncommittedData3
                    .UncommittedPagesOfAttributes.Single()
                    .Attributes.Single()
                    .Value);

                controller.Logout().AssertGoodResult<NoContentResult>();
            }
            finally
            {
                FileApiMgr.Instance.ReleaseAll();
                _testDbManager.RemoveDatabase(dbName);
            }
        }

        [Test]
        [Category("Automated")]
        public static void GetDocumentWordZones()
        {
            string dbName = "Test_AppBackendAPI_GetDocumentWordZones";

            try
            {
                var (fileProcessingDb, user, controller) = InitializeDBAndUser(dbName, _testFiles);

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
                FileApiMgr.Instance.ReleaseAll();
                _testDbManager.RemoveDatabase(dbName);
            }
        }

        [Test]
        [Category("Automated")]
        public static void AddGetComment()
        {
            string dbName = "Test_AppBackendAPI_AddGetComment";

            try
            {
                var (fileProcessingDb, user, controller) = InitializeDBAndUser(dbName, _testFiles);

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
                FileApiMgr.Instance.ReleaseAll();
                _testDbManager.RemoveDatabase(dbName);
            }
        }

        [Test]
        [Category("Automated")]
        public static void SkipDocument()
        {
            string dbName = "Test_AppBackendAPI_SkipDocument";

            try
            {
                var (fileProcessingDb, user, controller) = InitializeDBAndUser(dbName, _testFiles);

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
                var stats = fileProcessingDb.GetVisibleFileStats(actionId, true, true);
                Assert.AreEqual(1, stats.NumDocumentsSkipped, "There should be 1 skipped document.");

                IActionStatistics userStats = fileProcessingDb.GetFileStatsForUser(user.Username, actionId, false);
                Assert.AreEqual(1, userStats.NumDocumentsSkipped, "Skipped count for user should be 1");

                var skippedForThisUser = controller.GetSkippedFiles("").AssertGoodResult<QueuedFilesResult>();
                Assert.AreEqual(1, skippedForThisUser.QueuedFiles.Count(), "There should be 1 skipped document for user");

                string comment = fileProcessingDb.GetFileActionComment(openDocumentResult.Id, actionId);
                Assert.AreEqual(skipDocumentData.Comment, comment, "Retrieved Comment should equal the saved comment.");

                controller.CloseDocument(openDocumentResult.Id, true);

                // Mark the file as deleted and confirm that it doesn't show up in the stats anymore
                var workflowID = fileProcessingDb.GetWorkflowID(fileProcessingDb.ActiveWorkflow);
                var fileID = openDocumentResult.Id;
                fileProcessingDb.MarkFileDeleted(fileID, workflowID);

                stats = fileProcessingDb.GetVisibleFileStats(actionId, true, true);
                Assert.AreEqual(0, stats.NumDocumentsSkipped, "There should be no skipped documents visible");

                userStats = fileProcessingDb.GetFileStatsForUser(user.Username, actionId, false);
                Assert.AreEqual(0, userStats.NumDocumentsSkipped, "Skipped count for user should be 0");

                skippedForThisUser = controller.GetSkippedFiles("").AssertGoodResult<QueuedFilesResult>();
                Assert.AreEqual(0, skippedForThisUser.QueuedFiles.Count(), "There should be 0 skipped documents for user");

                controller.Logout();
            }
            finally
            {
                FileApiMgr.Instance.ReleaseAll();
                _testDbManager.RemoveDatabase(dbName);
            }
        }

        [Test, Category("Automated")]
        public static void FailDocument()
        {
            string dbName = "Test_AppBackendAPI_FailDocument";

            try
            {
                var (fileProcessingDb, user, controller) = InitializeDBAndUser(dbName, _testFiles);

                LogInToWebApp(controller, user);
                var openDocumentResult = OpenDocument(controller, 1);

                var result = controller.FailDocument(openDocumentResult.Id);
                result.AssertGoodResult<NoContentResult>();

                var actionId = fileProcessingDb.GetActionID(_VERIFY_ACTION);
                var stats = fileProcessingDb.GetVisibleFileStats(actionId, true, true);
                Assert.AreEqual(1, stats.NumDocumentsFailed, "There should be 1 failed document.");

                controller.CloseDocument(openDocumentResult.Id, true);
                controller.Logout();
            }
            finally
            {
                FileApiMgr.Instance.ReleaseAll();
                _testDbManager.RemoveDatabase(dbName);
            }
        }

        /// <summary>
        /// Test search results on first IDShield document. Combined what _ought_ to be multiple tests to save run time (setup/teardown are costly because of DB)
        /// </summary>
        [Test]
        [Category("Automated")]
        public static void PostSearch()
        {
            string dbName = "Test_AppBackendAPI_PostSearch";

            try
            {
                var (fileProcessingDb, user, controller) = InitializeDBAndUser(dbName, _testFiles);
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
                FileApiMgr.Instance.ReleaseAll();
                _testDbManager.RemoveDatabase(dbName);
            }
        }

        [Test]
        [Category("Automated")]
        public static void PostSearchNoUSS()
        {
            string dbName = "Test_AppBackendAPI_PostSearch";

            try
            {
                var (fileProcessingDb, user, controller) = InitializeDBAndUser(dbName, _testFiles);
                var docID = 2;

                LogInToWebApp(controller, user);
                OpenDocument(controller, docID);

                // Literal, ignore case
                var result = controller.PostSearch(docID, new SearchParameters { Query = "DOE", QueryType = QueryType.Literal, CaseSensitive = false, PageNumber = null, ResultType = "Name" });
                result.AssertResultCode(404, "An error is expected with no uss file");
            }
            finally
            {
                FileApiMgr.Instance.ReleaseAll();
                _testDbManager.RemoveDatabase(dbName);
            }
        }

        /// <summary>
        /// Test search results on bad page number
        /// </summary>
        [Test]
        [Category("Automated")]
        public static void PostSearchBadPage()
        {
            string dbName = "Test_AppBackendAPI_PostSearch_BadPage";

            try
            {
                var (fileProcessingDb, user, controller) = InitializeDBAndUser(dbName, _testFiles);
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
                FileApiMgr.Instance.ReleaseAll();
                _testDbManager.RemoveDatabase(dbName);
            }
        }

        /// <summary>
        /// Test search results on bad page number
        /// </summary>
        [Test]
        [Category("Automated")]
        public static void PostSearchBadResultType()
        {
            string dbName = "Test_AppBackendAPI_PostSearch_BadResultType";

            try
            {
                var (fileProcessingDb, user, controller) = InitializeDBAndUser(dbName, _testFiles);
                var docID = 1;

                LogInToWebApp(controller, user);
                OpenDocument(controller, docID);

                var result = controller.PostSearch(docID, new SearchParameters { Query = "DOE", QueryType = QueryType.Literal, CaseSensitive = false, PageNumber = 1, ResultType = "123" });
                result.AssertResultCode(400, "Searching with invalid result type should be a 400 error. See ISSUE-16673");

                controller.Logout().AssertGoodResult<NoContentResult>();
            }
            finally
            {
                FileApiMgr.Instance.ReleaseAll();
                _testDbManager.RemoveDatabase(dbName);
            }
        }

        /// <summary>
        /// Test search results with bad query
        /// </summary>
        [Test]
        [Category("Automated")]
        public static void PostSearchBadQuery()
        {
            string dbName = "Test_AppBackendAPI_PostSearch_BadQuery";

            try
            {
                var (fileProcessingDb, user, controller) = InitializeDBAndUser(dbName, _testFiles);
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
                FileApiMgr.Instance.ReleaseAll();
                _testDbManager.RemoveDatabase(dbName);
            }
        }

        /// <summary>
        /// Test search results on 1000 page document
        /// </summary>
        [Test]
        [Category("Automated")]
        public static void PostSearch1kPage()
        {
            string dbName = "Test_AppBackendAPI_PostSearch_1k";

            try
            {
                var (db, user, controller) = InitializeDBAndUser(dbName, _testFiles);
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
                FileApiMgr.Instance.ReleaseAll();
                _testDbManager.RemoveDatabase(dbName);
            }
        }

        /// <summary>
        /// Tests that page data can be cached and that the cached data is used.
        /// </summary>
        [Test]
        [Category("Automated")]
        public static void CachePageData()
        {
            CachePageDataAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public static async Task CachePageDataAsync()
        {
            string dbName = "Test_AppBackendAPI_CachePageData";
            TestFileManager<TestBackendAPI> testSpecficFiles = new TestFileManager<TestBackendAPI>(dbName);

            try
            {
                var (db, user, controller) = InitializeDBAndUser(dbName, testSpecficFiles);
                int docID = 3;
                // NOTE: GetDocumentPage will trigger caching of page 1 (when openend) as well as any
                // subsequent page. To isoloate behavior of CachePageDataAsync, all testing here will
                // be for page 2.
                int page = 2;

                LogInToWebApp(controller, user);
                OpenDocument(controller, docID);

                // Currently GetDocumentPage and GetPageWordZones will utilize cached data.
                // Retrieve data before caching for later comparison.
                var pageData1 = new PageData(controller, docID, page);

                // Request for page 2 data to be cached; and allow up to 3 seconds.
                var cacheTask = controller.CachePageDataAsync(docID, page);
                if (await Task.WhenAny(cacheTask, Task.Delay(3000)).ConfigureAwait(false) == cacheTask)
                {
                    var result = await cacheTask.ConfigureAwait(false);
                    result.AssertGoodResult<NoContentResult>();
                }
                else
                {
                    Assert.Fail("Failed to cache data");
                }

                // Replace image + uss file to confirm API is retrieving cached data from DB instead of from the files on disk.
                string targetFileName = _testFileNames[docID - 1];
                string targetUssFileName = targetFileName + ".uss";
                string differentFileName = _testFileNames[docID + 1];
                string differentUssFileName = differentFileName + ".uss";
                File.Copy(differentFileName, targetFileName, true);
                File.Copy(differentUssFileName, targetUssFileName, true);

                var pageData2 = new PageData(controller, docID, page);
                Assert.IsTrue(pageData1.Equals(pageData2), "Page data is different");

                // Closing a document will clear the cached data. Confirm this is the case
                controller.CloseDocument(docID, false);

                Assert.IsFalse(controller.GetCachedImagePageNumbers(db).Any(), "Cache was not cleared");

                // Since the files on disk were replaced, trying again to get the document image and word zones should
                // now yield different results than we got on the initial read or from cache.
                OpenDocument(controller, docID);
                var pageData3 = new PageData(controller, docID, page);
                Assert.IsFalse(pageData1.Equals(pageData3), "Page data was expected to differ");

                controller.Logout().AssertGoodResult<NoContentResult>();
            }
            finally
            {
                testSpecficFiles.Dispose();
                FileApiMgr.Instance.ReleaseAll();
                _testDbManager.RemoveDatabase(dbName);
            }
        }

        /// <summary>
        /// Test that trying to cache a non-existant page fails with an appropriate 404 error.
        /// </summary>
        [Test]
        [Category("Automated")]
        public static void CacheNonExistentPage()
        {
            CacheNonExistentPageAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public static async Task CacheNonExistentPageAsync()
        {
            string dbName = "Test_AppBackendAPI_CacheNonExistentPage";

            try
            {
                var (db, user, controller) = InitializeDBAndUser(dbName, _testFiles);
                var docID = 1;

                LogInToWebApp(controller, user);
                OpenDocument(controller, docID);

                var result = await controller.CachePageDataAsync(docID, 2).ConfigureAwait(false);
                result.AssertResultCode(404, "Expected page not found.");

                controller.Logout().AssertGoodResult<NoContentResult>();
            }
            finally
            {
                FileApiMgr.Instance.ReleaseAll();
                _testDbManager.RemoveDatabase(dbName);
            }
        }

        /// <summary>
        /// Test that trying to cache a corrupt uss file generates a 500 error that is logged to the
        /// cache table.
        /// </summary>
        [Test]
        [Category("Automated")]
        public static void CacheCorruptUssFile()
        {
            CacheCorruptUssFileAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public static async Task CacheCorruptUssFileAsync()
        {
            string dbName = "Test_AppBackendAPI_CacheCorruptUssFile";
            TestFileManager<TestBackendAPI> testSpecficFiles = new TestFileManager<TestBackendAPI>(dbName);

            try
            {
                var (db, user, controller) = InitializeDBAndUser(dbName, testSpecficFiles);
                var docID = 1;

                string targetFileName = _testFileNames[docID - 1];
                string targetUssFileName = targetFileName + ".uss";

                File.Copy(targetFileName, targetUssFileName, true);

                LogInToWebApp(controller, user);
                OpenDocument(controller, docID);

                var result = await controller.CachePageDataAsync(docID, 1).ConfigureAwait(false);
                result.AssertResultCode(500, "Expected error caching corrupt uss");

                // Confirm that the cache exception has been recorded in the db for the page.
                Assert.IsFalse(controller.IsPageDataCached(db, 1, ECacheDataType.kImage | ECacheDataType.kWordZone));

                controller.Logout().AssertGoodResult<NoContentResult>();
            }
            finally
            {
                testSpecficFiles.Dispose();
                FileApiMgr.Instance.ReleaseAll();
                _testDbManager.RemoveDatabase(dbName);
            }
        }

        /// <summary>
        /// Test that retrieving a document page automatically triggers data for the next page to be cached.
        /// </summary>
        [Test]
        [Category("Automated")]
        public static void AutoPageCache()
        {
            string dbName = "Test_AppBackendAPI_AutoPageCache";

            try
            {
                var (db, user, controller) = InitializeDBAndUser(dbName, _testFiles);
                var docID = 3;

                LogInToWebApp(controller, user);
                OpenDocument(controller, docID);

                // GetDocumentPage should trigger caching of the subsequent page in all cases and
                // also the current page only if the current page is 1
                var result = controller.GetDocumentPage(docID, 1);
                result.AssertGoodResult<FileContentResult>();

                // Wait for up to 3 seconds for page 1 and 2 to be cached (page 2 should be cached first)
                bool dataIsCached = false;
                for (int i = 0; !dataIsCached && i < 30; i++)
                {
                    if (controller.GetCachedImagePageNumbers(db).SequenceEqual(new[] { 1, 2 }))
                    {
                        dataIsCached = true;
                    }
                    else
                    {
                        Thread.Sleep(100);
                    }
                }

                Assert.IsTrue(dataIsCached, "Failed to cache data");

                result = controller.GetDocumentPage(docID, 3);
                result.AssertGoodResult<FileContentResult>();

                // Wait for up to 3 seconds for page 4 to be cached (page 3 should not be cached)
                dataIsCached = false;
                for (int i = 0; !dataIsCached && i < 30; i++)
                {
                    if (controller.GetCachedImagePageNumbers(db).SequenceEqual(new[] { 1, 2, 4 }))
                    {
                        dataIsCached = true;
                    }
                    else
                    {
                        Thread.Sleep(100);
                    }
                }

                controller.Logout().AssertGoodResult<NoContentResult>();
            }
            finally
            {
                FileApiMgr.Instance.ReleaseAll();
                _testDbManager.RemoveDatabase(dbName);
            }
        }

        /// <summary>
        /// Test that closing a session in the midst of caching does not cause errors.
        /// </summary>
        [Test]
        [Category("Automated")]
        public static void CacheSessionCloseWhileInProgress()
        {
            string dbName = "Test_AppBackendAPI_CacheSessionCloseWhileInProgress";

            try
            {
                var (db, user, controller) = InitializeDBAndUser(dbName, _testFiles);
                var docID = 3;

                // Looping this test originally caused cursor errors in CacheFileTaskSessionData;
                // While CacheSimultaneousOperations has been added to test still more complex
                // scenarios, confirm the issue discovered looping here remains resolved.
                for (int i = 0; i < 100; i++)
                {
                    LogInToWebApp(controller, user);
                
                    OpenDocument(controller, docID);
                    var documentSessionId = controller.GetActiveDocumentSessionId();

                    var cacheTasks = new Task<IActionResult>[] {
                        controller.CachePageDataAsync(docID, 1),
                        controller.CachePageDataAsync(docID, 2),
                        controller.CachePageDataAsync(docID, 3),
                        controller.CachePageDataAsync(docID, 4)
                    };

                    var completedIndex = Task.WaitAny(cacheTasks);
                    cacheTasks[completedIndex].Result.AssertGoodResult<NoContentResult>();

                    // Caching should be continuing asynchronously and should not have cached
                    // all pages by this point.
                    var cachedPageCount = db.GetCachedImagePageNumbers(documentSessionId).Length;
                    Assert.True(cachedPageCount >= 1 && cachedPageCount < 4,
                        "Unexpected number of cached pages.");

                    controller.CloseDocument(docID, false).AssertGoodResult<NoContentResult>();

                    // Closing the document should wipe out all existing cache rows.
                    Assert.AreEqual(0, db.GetCachedImagePageNumbers(documentSessionId).Length);

                    Task.WaitAll(cacheTasks);
                    foreach (var task in cacheTasks)
                    {
                        task.Result.AssertGoodResult<NoContentResult>();
                    }

                    controller.Logout().AssertGoodResult<NoContentResult>();
                }
            }
            finally
            {
                FileApiMgr.Instance.ReleaseAll();
                _testDbManager.RemoveDatabase(dbName);
            }
        }

        /// <summary>
        /// Test caching and retrieving page data from multiple documents simultaneously under
        /// a number of different circumstances including cases where sessions are closed before
        /// async cache operations complete.
        /// </summary>
        [Test]
        [Category("Automated")]
        public static void CacheSimultaneousOperations()
        {
            string dbName = "Test_AppBackendAPI_CacheSimultaneousOperations";

            try
            {
                var (db, user, controller) = InitializeDBAndUser(dbName, _testFiles);

                using (var testFiles1 = new TestFileManager<TestBackendAPI>(dbName + "_1"))
                using (var testFiles2 = new TestFileManager<TestBackendAPI>(dbName + "_2"))
                using (var testFiles3 = new TestFileManager<TestBackendAPI>(dbName + "_3"))
                using (var testFiles4 = new TestFileManager<TestBackendAPI>(dbName + "_4"))
                {
                    var set1FileIDs = AddFilesToDB(testFiles1, db, 1, addAdditionalSet: true);
                    var set2FileIDs = AddFilesToDB(testFiles2, db, 1, addAdditionalSet: true);
                    var set3FileIDs = AddFilesToDB(testFiles3, db, 1, addAdditionalSet: true);
                    var set4FileIDs = AddFilesToDB(testFiles4, db, 1, addAdditionalSet: true);

                    // 10 loops of letting each specific type of test run concurrently
                    for (int i = 0; i < 10; i++)
                    {
                        var testTasks = new[]
                        {
                            // Each of the following tests opens all 5 documents in the test set,
                            // initializes caching of the first 10 pages of each doc (only one has > 4 pages),
                            // then checks that data was properly cached for either all the pages, or just the first.
                            // The LongRunning flag used for each task below is to help ensure each task be run
                            // on a separate thread.

                            // The first two tests use separate sessions to load all documents in parallel.
                            Task.Factory.StartNew(() =>
                                TestCacheWithParallelSessions(db, set1FileIDs,
                                maxPages: 10,
                                waitAll: true), // Check that all pages were cached correctly
                                TaskCreationOptions.LongRunning),
                            Task.Factory.StartNew(() =>
                                TestCacheWithParallelSessions(db, set2FileIDs,
                                maxPages: 10,
                                waitAll: false), // Check that the first page was cached correctly, close doc before caching completes.
                                TaskCreationOptions.LongRunning),

                            // The second two tests use a single session to test each document sequentially.
                            Task.Factory.StartNew(() =>
                                TestCacheSequentiallySameSession(db, set3FileIDs,
                                maxPages: 10,
                                waitAll: true), // Check that all pages were cached correctly
                                TaskCreationOptions.LongRunning),
                            Task.Factory.StartNew(() =>
                                TestCacheSequentiallySameSession(db, set4FileIDs,
                                maxPages: 10,
                                waitAll: false), // Check that the first page was cached correctly, close doc before caching completes.
                                TaskCreationOptions.LongRunning)
                        };

                        Task.WaitAll(testTasks);
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractLog("ELI49466");
                throw ex;
            }
            finally
            {
                FileApiMgr.Instance.ReleaseAll();
                _testDbManager.RemoveDatabase(dbName);
            }
        }

        /// <summary>
        /// <para><b>Helper method for CacheSimultaneousOperations.</b></para>
        /// Uses separate sessions to test each document in <see paramref="fileSet"/> in parallel.
        /// </summary>
        /// <param name="db">The <see cref="FileProcessingDB"/> being used for the test.</param>
        /// <param name="fileSet">The IDs of the files to test.</param>
        /// <param name="maxPages">The number of pages to try caching (except for documents with
        /// fewer pages than specified here.</param>
        /// <param name="waitAll"><c>true</c> to wait until all pages have been cached and to
        /// verify data was correctly cached for each; <c>false</c> to cancel all further caching
        /// that was started after verifying data was cached correctly for the first page.</param>
        static void TestCacheWithParallelSessions(FileProcessingDB db, IEnumerable<int> fileSet, int maxPages, bool waitAll)
        {
            Parallel.ForEach(fileSet, docID =>
            {
                try
                {
                    TestCacheWithParallelSessions(db, docID, maxPages, waitAll);
                }
                catch (Exception ex)
                {
                    ex.ExtractLog("ELI49488");
                    throw ex;
                }
            });
        }

        /// <summary>
        /// <para><b>Helper method for CacheSimultaneousOperations.</b></para>
        /// Uses separate session to test the specified <see paramref="docID"/>.
        /// </summary>
        /// <param name="db">The <see cref="FileProcessingDB"/> being used for the test.</param>
        /// <param name="docID">The ID of the file to test.</param>
        /// <param name="maxPages">The number of pages to try caching (except for documents with
        /// fewer pages than specified here.</param>
        /// <param name="waitAll"><c>true</c> to wait until all pages have been cached and to
        /// verify data was correctly cached for each; <c>false</c> to cancel all further caching
        /// that was started after verifying data was cached correctly for the first page.</param>
        static void TestCacheWithParallelSessions(FileProcessingDB db, int docID, int maxPages, bool waitAll)
        {
            AppBackendController controller = null;

            try
            {
                User user = ApiTestUtils.CreateUser("jon_doe", "123");
                controller = SetupController(user);
                LogInToWebApp(controller, user);
                OpenDocument(controller, docID);

                Assert.AreEqual(0, controller.GetCachedImagePageNumbers(db).Length, "Document pages not cleared from cache");

                bool fileHasUSS = _testFileHasUSS[(docID - 1) % _testFileHasUSS.Length];

                // Currently GetDocumentPage and GetPageWordZones will utilize cached data.
                // Retrieve data before caching for later comparison.
                var pageData = new PageData(controller, docID, 1, getWordZoneData: fileHasUSS);
                var cacheTasks = StartCachingPages(controller, docID, maxPages, expectedFirstPageData: pageData);
                if (waitAll)
                {
                    var dataToCheck = ECacheDataType.kImage;
                    if (fileHasUSS)
                    {
                        dataToCheck |= ECacheDataType.kWordZone;
                    }
                    CheckAllPages(controller, db, cacheTasks, dataToCheck);
                }
            }
            finally
            {
                controller?.Logout().AssertGoodResult<NoContentResult>();
            }
        }

        /// <summary>
        /// <para><b>Helper method for CacheSimultaneousOperations.</b></para>
        /// Use single session to test each document sequentially.
        /// </summary>
        /// <param name="db">The <see cref="FileProcessingDB"/> being used for the test.</param>
        /// <param name="fileSet">The IDs of the files to test.</param>
        /// <param name="maxPages">The number of pages to try caching (except for documents with
        /// fewer pages than specified here.</param>
        /// <param name="waitAll"><c>true</c> to wait until all pages have been cached and to
        /// verify data was correctly cached for each; <c>false</c> to cancel all further caching
        /// that was started after verifying data was cached correctly for the first page.</param>
        static void TestCacheSequentiallySameSession(FileProcessingDB db, IEnumerable<int> fileSet, int maxPages, bool waitAll)
        {
            AppBackendController controller = null;
            User user = ApiTestUtils.CreateUser("jon_doe", "123");
            controller = SetupController(user);
            LogInToWebApp(controller, user);

            try
            {
                foreach(int docID in fileSet)
                {
                    OpenDocument(controller, docID);

                    Assert.AreEqual(0, controller.GetCachedImagePageNumbers(db).Length, "Document pages not cleared from cache");

                    bool hasUssFile = _testFileHasUSS[(docID - 1) % _testFileHasUSS.Length];
                    
                    // Currently GetDocumentPage and GetPageWordZones will utilize cached data.
                    // Retrieve data before caching for later comparison.
                    var pageData = new PageData(controller, docID, 1, getWordZoneData: hasUssFile);
                    var cacheTasks = StartCachingPages(controller, docID, maxPages, expectedFirstPageData: pageData);
                    if (waitAll)
                    {
                        var dataToCheck = ECacheDataType.kImage;
                        if (hasUssFile)
                        {
                            dataToCheck |= ECacheDataType.kWordZone;
                        }
                        CheckAllPages(controller, db, cacheTasks, dataToCheck);
                    }

                    controller.CloseDocument(docID, false).AssertGoodResult<NoContentResult>();
                }
            }
            catch (Exception ex)
            {
                ex.ExtractLog("ELI49489");
                throw ex;
            }
            finally
            {
                controller?.Logout().AssertGoodResult<NoContentResult>();
            }
        }

        /// <summary>
        /// <para><b>Helper method for CacheSimultaneousOperations.</b></para>
        /// Starts asynchronous calls to cache the pages of the specified <see paramref="docID"/>.
        /// </summary>
        /// <param name="controller">The <see cref="AppBackendController"/> being tested.</param>
        /// <param name="docID">The document ID for which pages should be cached.</param>
        /// <param name="maxPages">This method will start caching pages from the beginning of the
        /// document up to this value unless the document does not have this many pages.</param>
        /// <param name="expectedFirstPageData">Specifies the expected data for the first page.
        /// The cached data for the first page will be compared against this before returning.</param>
        /// <returns>An array of tasks for each page's cache operation.</returns>
        static Task<IActionResult>[] StartCachingPages(AppBackendController controller, int docID, int maxPages, PageData expectedFirstPageData)
        {
            var result = controller.GetPageInfo(docID);
            var pagesInfo = result.AssertGoodResult<PagesInfoResult>();

            var pages = Enumerable.Range(1, Math.Min(pagesInfo.PageCount, maxPages));

            var documentSessionId = controller.GetActiveDocumentSessionId();

            var cacheTasks = pages.Select(page =>
                controller.CachePageDataAsync(docID, page))
                .ToArray();

            var completedIndex = Task.WaitAny(cacheTasks);
            result = cacheTasks[completedIndex].Result;
            result.AssertGoodResult<NoContentResult>();

            var pageData = new PageData(controller, docID, 1,
                getWordZoneData: !string.IsNullOrWhiteSpace(expectedFirstPageData.WordZoneJson));
            Assert.IsTrue(pageData.Equals(expectedFirstPageData), "Page data is different");

            return cacheTasks;
        }

        /// <summary>
        /// <para><b>Helper method for CacheSimultaneousOperations.</b></para>
        /// Validates that each of the specified <see paramref="cacheTasks"/> completes successfully
        /// and that corresponding cached data can be found in the database.
        /// </summary>
        /// <param name="controller">The <see cref="AppBackendController"/> being tested.</param>
        /// <param name="db">The <see cref="FileProcessingDB"/> into which data is being cached.</param>
        /// <param name="cacheTasks">The tasks for each cache operation to check.</param>
        /// <param name="dataType">This call will check for the existence of all types of data indicated
        /// by the <see cref="ECacheDataType"/> flag values specified here.</param>
        static void CheckAllPages(AppBackendController controller, FileProcessingDB db,
            Task<IActionResult>[] cacheTasks, ECacheDataType dataType)
        {
            Task.WaitAll(cacheTasks);
            for (int page = 1; page <= cacheTasks.Length; page++)
            {
                var task = cacheTasks[page - 1];
                task.Result.AssertGoodResult<NoContentResult>();
                Assert.IsTrue(controller.IsPageDataCached(db, page, dataType),
                    "Page was not cached");
            }
        }

        [Test]
        [Category("Automated")]
        public static void GetUserSpecificQueueStatus()
        {
            string dbName = "Test_AppBackendAPI_GetUserSpecificQueueStatus";

            try
            {
                var (fileProcessingDb, user, controller) = InitializeDBAndUser(dbName, _testFiles);
                var newSettings = JsonConvert.SerializeObject(new WebAppSettingsResult { EnableAllPendingQueue = false });

                fileProcessingDb.ExecuteCommandQuery($"UPDATE [dbo].[WebAppConfig] SET SETTINGS = '{newSettings}' WHERE TYPE = 'RedactionVerificationSettings'");

                // This is a hacky solution for the unit test because actually assigning them via a user specific queue fam
                // Will auotmatically create the user.
                fileProcessingDb.ExecuteCommandQuery(@"INSERT INTO dbo.FAMUser(UserName) VALUES ('jon_doe'), ('jane_doe')");
                fileProcessingDb.ExecuteCommandQuery($"UPDATE dbo.FileActionStatus SET UserID = 5");

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
                var controller2 = SetupController(user2);

                result = controller2.Login(user2);
                token = result.AssertGoodResult<JwtSecurityToken>();
                controller2.ApplyTokenClaimPrincipalToContext(token);

                sessionResult = controller2.SessionLogin();
                sessionToken = sessionResult.AssertGoodResult<JwtSecurityToken>();

                result = controller.GetQueueStatus();
                queueStatus = result.AssertGoodResult<QueueStatusResult>();
                Assert.AreEqual(2, queueStatus.ActiveUsers);
                // There are no new documents assigned to the new user.
                Assert.AreEqual(pendingDocuments, 4);

                controller.CloseDocument(openDocumentResult.Id, commit: true)
                    .AssertGoodResult<NoContentResult>();

                result = controller.OpenDocument();
                result.AssertGoodResult<DocumentIdResult>();
                pendingDocuments--;

                result = controller2.GetQueueStatus();
                queueStatus = result.AssertGoodResult<QueueStatusResult>();
                Assert.AreEqual(2, queueStatus.ActiveUsers);
                Assert.AreEqual(0, queueStatus.PendingDocuments);

                controller2.ApplyTokenClaimPrincipalToContext(sessionToken);
                result = controller2.OpenDocument();
                result.AssertGoodResult<DocumentIdResult>();
                pendingDocuments--;

                result = controller2.GetQueueStatus();
                queueStatus = result.AssertGoodResult<QueueStatusResult>();
                Assert.AreEqual(2, queueStatus.ActiveUsers);
                Assert.AreEqual(0, queueStatus.PendingDocuments);

                controller.Logout()
                    .AssertGoodResult<NoContentResult>();
                pendingDocuments++;

                result = controller2.GetQueueStatus();
                queueStatus = result.AssertGoodResult<QueueStatusResult>();
                Assert.AreEqual(1, queueStatus.ActiveUsers);
                Assert.AreEqual(0, queueStatus.PendingDocuments);

                controller2.Logout()
                    .AssertGoodResult<NoContentResult>();
            }
            finally
            {
                FileApiMgr.Instance.ReleaseAll();
                _testDbManager.RemoveDatabase(dbName);
            }
        }

        [Test]
        [Category("Automated")]
        public static void GetQueuedFiles()
        {
            string dbName = "Test_AppBackendAPI_GetQueuedFiles";

            try
            {
                var (fileProcessingDb, user, controller) = InitializeDBAndUser(dbName, _testFiles);

                var result = controller.Login(user);
                var token = result.AssertGoodResult<JwtSecurityToken>();
                controller.ApplyTokenClaimPrincipalToContext(token);

                result = controller.GetQueuedFiles("");
                var queuedFilesResult = result.AssertGoodResult<QueuedFilesResult>();

                Assert.Greater(queuedFilesResult.QueuedFiles.Count(), 0);

                controller.Logout()
                    .AssertGoodResult<NoContentResult>();
            }
            finally
            {
                FileApiMgr.Instance.ReleaseAll();
                _testDbManager.RemoveDatabase(dbName);
            }
        }
        #endregion Public Test Functions

        #region Private Members

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

        private static (FileProcessingDB, User, AppBackendController) InitializeDBAndUser(string dbName, 
            TestFileManager<TestBackendAPI> testFiles,
            string username = "jane_doe", string password = "123")
        {
            var (fileProcessingDb, user, controller) =
                _testDbManager.InitializeEnvironment(CreateController(),
                    ApiContext.CURRENT_VERSION, "Resources.Demo_IDShield.bak", dbName, username, password);

            var actionID = fileProcessingDb.GetActionIDForWorkflow(_VERIFY_ACTION, fileProcessingDb.GetWorkflowID("CourtOffice"));
            AddFilesToDB(testFiles, fileProcessingDb, actionID);

            return (fileProcessingDb, user, controller);
        }

        private static List<int> AddFilesToDB(TestFileManager<TestBackendAPI> testFiles, FileProcessingDB fileProcessingDb, int actionID,
            bool addAdditionalSet = false)
        {
            var fileIDs = new List<int>();

            for (int i = 0; i < _testFileArray.Length; i++)
            {
                int fileID = -1;
                string testFileName = testFiles.GetFile(_testFileArray[i]);

                if (!addAdditionalSet)
                {
                    _testFileNames[i] = testFileName;

                    if (_testFileInDB[i])
                    {
                        fileID = fileProcessingDb.GetFileID(_testFileOriginalNames[i]);
                        fileProcessingDb.RenameFile(new FileRecordClass { ActionID = actionID, FileID = fileID, Name = _testFileOriginalNames[i] }, testFileName);
                        fileIDs.Add(fileID);
                    }
                }

                if (fileID == -1)
                {
                    FileRecord fileRecord = fileProcessingDb.AddFile(
                        testFileName, _VERIFY_ACTION, 1, EFilePriority.kPriorityNormal, false, false,
                        EActionStatus.kActionPending, false, out _, out _);

                    fileIDs.Add(fileRecord.FileID);
                }

                if (_testFileHasUSS[i])
                {
                    testFiles.GetFile(_testFileArray[i] + ".uss", testFileName + ".uss");
                }

                if (_testFileHasVOA[i])
                {
                    testFiles.GetFile(_testFileArray[i] + ".voa", testFileName + ".voa");
                }
            }

            return fileIDs;
        }

        private static AppBackendController SetupController(User user)
        {
            return user.SetupController(CreateController());
        }

        private static AppBackendController CreateController()
        {
            return new AppBackendController(new DocumentDataFactory(FileApiMgr.Instance));
        }

        #endregion Private Members
    }

    /// <summary>
    /// Helper class to assist in validating cached page data matches expected page data.
    /// </summary>
    public class PageData
    {
        /// <summary>
        /// Reads page data for the specified document page. Will read from cache if data has been
        /// cached; otherwise data will be read from the source files.
        /// </summary>
        /// <param name="controller">The <see cref="AppBackendController"/> being tested.</param>
        /// <param name="docID">The document ID for which data should be read.</param>
        /// <param name="page">The page number for which data should be read.</param>
        /// <param name="getWordZoneData"><c>true</c> to read word zone data; <c>false</c> to
        /// read image data only.</param>
        public PageData(AppBackendController controller, int docID, int page, bool getWordZoneData = true)
        {
            try
            {
                var result = controller.GetDocumentPage(docID, page);
                var fileContentResult = result.AssertGoodResult<FileContentResult>();
                Assert.Greater(fileContentResult.FileContents.Length, 0);
                ImageSize = fileContentResult.FileContents.Length;

                if (getWordZoneData)
                {
                    result = controller.GetPageWordZones(docID, page);
                    var wordZoneResult = result.AssertGoodResult<WordZoneDataResult>();
                    Assert.Greater(wordZoneResult.Zones.Count, 0);
                    WordZoneJson = JsonConvert.SerializeObject(wordZoneResult);
                }
            }
            catch (Exception ex)
            {
                var ee = new ExtractException("ELI49454", "Error getting page data", ex);
                ee.AddDebugData("DocID", docID, false);
                ee.AddDebugData("Page", page, false);
                throw ee;
            }
        }

        public int ImageSize { get; private set; }
        public string WordZoneJson { get; private set; } 

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns><c>true</c> if the specified object is equal to the current object; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            if (base.Equals(obj))
            {
                return true;
            }

            if (!(obj is PageData other))
            {
                return false;
            }

            // It appears the PDF image generated by GetDocumentPage can end up producing different bytes at the end of the
            // file that sometimes cause the files to be a byte or two different in length. I don't see any evidence in any
            // case that the images are invalid. Allow for some difference in file length.
            int imageDataLenDiff = Math.Abs(ImageSize - other.ImageSize);

            return (imageDataLenDiff < 5) && WordZoneJson == other.WordZoneJson;
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode()
        {
            return
                ImageSize.GetHashCode()
                ^ WordZoneJson.GetHashCode();
        }
    }
}
