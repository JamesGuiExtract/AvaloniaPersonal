﻿using Extract.FileActionManager.Database.Test;
using Extract.Imaging;
using Extract.Testing.Utilities;
using Extract.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.IO;
using System.Net;
using UCLID_FILEPROCESSINGLib;
using WebAPI.Controllers;
using WebAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Routing;
using Newtonsoft.Json.Linq;
using AttributeDbMgrComponentsLib;
using System.Linq;

namespace Extract.Web.WebAPI.Test
{
    [TestFixture]
    [NUnit.Framework.Category("AppBackendAPI")]
    public class TestBackendAPI
    {
        #region Constants

        static readonly string _TEST_FILE1 = "Resources.TestImage003.tif";
        static readonly string _TEST_FILE1_USS = "Resources.TestImage003.tif.uss";
        static readonly string _ACTION_NAME = "Verify";

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

        [Test, Category("Automated")]
        public static void Test_LoginLogout()
        {
            string dbName = "AppBackendAPI_Test_LoginLogout";

            try
            {
                (FileProcessingDB fileProcessingDb, User user, AppBackendController controller) =
                    _testDbManager.InitializeEnvironment<TestBackendAPI, AppBackendController>
                        ("Resources.Demo_IDShield.bak", dbName, "admin", "a");

                var result = controller.Login(user);
                var loginToken = result.AssertGoodResult<LoginToken>();
                Assert.IsTrue(loginToken.access_token.ToString().StartsWith("eyJhb", StringComparison.OrdinalIgnoreCase));

                // Login should register an active FAM session
                Assert.IsTrue(fileProcessingDb.IsAnyFAMActive());

                // Login should close the FAM session
                result = controller.Logout();
                result.AssertGoodResult<GenericResult>();

                Assert.IsFalse(fileProcessingDb.IsAnyFAMActive());
            }
            catch (Exception ex)
            {
                Assert.Fail("Failed: {0}", ex.Message);
            }
            finally
            {
                FileApiMgr.ReleaseAll();
                _testDbManager.RemoveDatabase(dbName);
            }
        }

        [Test, Category("Automated")]
        public static void Test_GetSettings()
        {
            string dbName = "AppBackendAPI_Test_GetSettings";

            try
            {
                (FileProcessingDB fileProcessingDb, User user, AppBackendController controller) =
                    _testDbManager.InitializeEnvironment<TestBackendAPI, AppBackendController>
                        ("Resources.Demo_IDShield.bak", dbName, "admin", "a");

                var result = controller.Login(user);
                result.AssertGoodResult<LoginToken>();

                result = controller.GetSettings();
                var settings = result.AssertGoodResult<WebAppSettings>();

                Assert.IsTrue(settings.RedactionTypes.SequenceEqual(
                    new[] { "DOB", "SSN",  "TestType" }), "Failed to retrieve redaction types");

                // Login should close the FAM session
                result = controller.Logout();
                result.AssertGoodResult<GenericResult>();
            }
            catch (Exception ex)
            {
                Assert.Fail("Failed: {0}", ex.Message);
            }
            finally
            {
                FileApiMgr.ReleaseAll();
                _testDbManager.RemoveDatabase(dbName);
            }
        }

        [Test, Category("Automated")]
        public static void Test_OpenDocument_NoID()
        {
            string dbName = "AppBackendAPI_Test_OpenDocument_NoID";

            try
            {
                (FileProcessingDB fileProcessingDb, User user, AppBackendController controller) =
                    _testDbManager.InitializeEnvironment<TestBackendAPI, AppBackendController>
                        ("Resources.Demo_IDShield.bak", dbName, "admin", "a");

                var result = controller.Login(user);
                result.AssertGoodResult<LoginToken>();

                result = controller.OpenDocument();
                var openDocumentResult = result.AssertGoodResult<DocumentId>();
                Assert.AreEqual(1, openDocumentResult.Id);

                Assert.AreEqual(EActionStatus.kActionProcessing, fileProcessingDb.GetFileStatus(1, _ACTION_NAME, false));

                result = controller.CloseDocument(commit: false);
                result.AssertGoodResult<GenericResult>();

                Assert.AreEqual(EActionStatus.kActionPending, fileProcessingDb.GetFileStatus(1, _ACTION_NAME, false));

                result = controller.OpenDocument();
                result.AssertGoodResult<DocumentId>();

                Assert.AreEqual(EActionStatus.kActionProcessing, fileProcessingDb.GetFileStatus(1, _ACTION_NAME, false));

                result = controller.CloseDocument(commit: true);
                result.AssertGoodResult<GenericResult>();

                Assert.AreEqual(EActionStatus.kActionCompleted, fileProcessingDb.GetFileStatus(1, _ACTION_NAME, false));

                result = controller.Logout();
                result.AssertGoodResult<GenericResult>();
            }
            finally
            {
                FileApiMgr.ReleaseAll();
                _testDbManager.RemoveDatabase(dbName);
            }
        }

        [Test, Category("Automated")]
        public static void Test_OpenDocument_WithID()
        {
            string dbName = "AppBackendAPI_Test_OpenDocument_WithID";

            try
            {
                (FileProcessingDB fileProcessingDb, User user, AppBackendController controller) =
                    _testDbManager.InitializeEnvironment<TestBackendAPI, AppBackendController>
                        ("Resources.Demo_IDShield.bak", dbName, "admin", "a");

                var result = controller.Login(user);
                result.AssertGoodResult<LoginToken>();

                result = controller.OpenDocument();
                var openDocumentResult = result.AssertGoodResult<DocumentId>();
                Assert.AreEqual(1, openDocumentResult.Id);

                Assert.AreEqual(EActionStatus.kActionProcessing, fileProcessingDb.GetFileStatus(1, _ACTION_NAME, false));

                result = controller.CloseDocument(commit: false);
                result.AssertGoodResult<GenericResult>();

                Assert.AreEqual(EActionStatus.kActionPending, fileProcessingDb.GetFileStatus(1, _ACTION_NAME, false));

                result = controller.OpenDocument(2);
                openDocumentResult = result.AssertGoodResult<DocumentId>();
                Assert.AreEqual(2, openDocumentResult.Id);

                Assert.AreEqual(EActionStatus.kActionProcessing, fileProcessingDb.GetFileStatus(2, _ACTION_NAME, false));

                result = controller.CloseDocument(commit: true);
                result.AssertGoodResult<GenericResult>();

                Assert.AreEqual(EActionStatus.kActionCompleted, fileProcessingDb.GetFileStatus(2, _ACTION_NAME, false));

                result = controller.Logout();
                result.AssertGoodResult<GenericResult>();
            }
            finally
            {
                FileApiMgr.ReleaseAll();
                _testDbManager.RemoveDatabase(dbName);
            }
        }

        [Test, Category("Automated")]
        public static void Test_ReOpenDocument()
        {
            string dbName = "AppBackendAPI_Test_ReOpenDocument";

            try
            {
                (FileProcessingDB fileProcessingDb, User user, AppBackendController controller) =
                    _testDbManager.InitializeEnvironment<TestBackendAPI, AppBackendController>
                        ("Resources.Demo_IDShield.bak", dbName, "admin", "a");

                var result = controller.Login(user);
                result.AssertGoodResult<LoginToken>();

                result = controller.OpenDocument();
                result = controller.CloseDocument(commit: false);
                result.AssertGoodResult<GenericResult>();

                Assert.AreEqual(EActionStatus.kActionPending, fileProcessingDb.GetFileStatus(2, _ACTION_NAME, false));

                result = controller.OpenDocument(2);
                var openDocumentResult = result.AssertGoodResult<DocumentId>();
                Assert.AreEqual(2, openDocumentResult.Id);

                // Per GGK request, allow second call to OpenDocument to not fail and return the ID of the already
                // open document.
                // https://extract.atlassian.net/browse/WEB-55
                result = controller.OpenDocument();
                openDocumentResult = result.AssertGoodResult<DocumentId>();
                Assert.AreEqual(2, openDocumentResult.Id);

                result = controller.CloseDocument(commit: false);
                result.AssertGoodResult<GenericResult>();

                // After document is closed, the first file should be the file opened, not the 2nd
                // file that had been specified originally.
                result = controller.OpenDocument();
                openDocumentResult = result.AssertGoodResult<DocumentId>();
                Assert.AreEqual(1, openDocumentResult.Id);

                result = controller.CloseDocument(commit: false);
                result.AssertGoodResult<GenericResult>();

                result = controller.Logout();
                result.AssertGoodResult<GenericResult>();
            }
            finally
            {
                FileApiMgr.ReleaseAll();
                _testDbManager.RemoveDatabase(dbName);
            }
        }

        [Test, Category("Automated")]
        public static void Test_LogoutClosesDocument()
        {
            string dbName = "AppBackendAPI_Test_LogoutClosesDocument";

            try
            {
                (FileProcessingDB fileProcessingDb, User user, AppBackendController controller) =
                    _testDbManager.InitializeEnvironment<TestBackendAPI, AppBackendController>
                        ("Resources.Demo_IDShield.bak", dbName, "admin", "a");

                var result = controller.Login(user);
                result.AssertGoodResult<LoginToken>();

                result = controller.OpenDocument();
                var openDocumentResult = result.AssertGoodResult<DocumentId>();
                Assert.AreEqual(1, openDocumentResult.Id);

                Assert.AreEqual(EActionStatus.kActionProcessing, fileProcessingDb.GetFileStatus(1, _ACTION_NAME, false));

                result = controller.Logout();
                result.AssertGoodResult<GenericResult>();

                Assert.AreEqual(EActionStatus.kActionPending, fileProcessingDb.GetFileStatus(1, _ACTION_NAME, false));
            }
            finally
            {
                FileApiMgr.ReleaseAll();
                _testDbManager.RemoveDatabase(dbName);
            }
        }

        [Test, Category("Automated")]
        public static void Test_GetPageInfo_NoUSS()
        {
            string dbName = "AppBackendAPI_Test_GetPageInfo_NoUSS";

            try
            {
                (FileProcessingDB fileProcessingDb, User user, AppBackendController controller) =
                    _testDbManager.InitializeEnvironment<TestBackendAPI, AppBackendController>
                        ("Resources.Demo_IDShield.bak", dbName, "admin", "a");

                string testFileName = _testFiles.GetFile(_TEST_FILE1);

                var fileRecord = fileProcessingDb.AddFile(
                    testFileName, _ACTION_NAME, 1, EFilePriority.kPriorityHigh, false, false,
                    EActionStatus.kActionPending, false, out bool t1, out EActionStatus t2);
                int fileId = fileRecord.FileID;

                var result = controller.Login(user);
                result.AssertGoodResult<LoginToken>();

                result = controller.OpenDocument();
                var openDocumentResult = result.AssertGoodResult<DocumentId>();
                Assert.AreEqual(fileId, openDocumentResult.Id);

                result = controller.GetPageInfo();
                var pagesInfo = result.AssertGoodResult<PagesInfo>();

                Assert.AreEqual(pagesInfo.PageCount, 4, "Unexpected page count");
                Assert.AreEqual(pagesInfo.PageInfos.Count, 4, "Unexpected page infos count");
                for (int page = 1; page <= 4; page++)
                {
                    var pageInfo = pagesInfo.PageInfos[page - 1];
                    Assert.AreEqual(page, pageInfo.Page, "Unexpected page number");
                    Assert.AreEqual((page == 2) ? 2200 : 1712, pageInfo.Width, "Unexpected page width");
                    Assert.AreEqual((page == 2) ? 1712 : 2200, pageInfo.Height, "Unexpected page height");
                    Assert.AreEqual(0, pageInfo.DisplayOrientation, "Unexpected page orientation");
                }

                result = controller.Logout();
                result.AssertGoodResult<GenericResult>();
            }
            finally
            {
                FileApiMgr.ReleaseAll();
                _testDbManager.RemoveDatabase(dbName);

                _testFiles.RemoveFile(_TEST_FILE1);
            }
        }

        [Test, Category("Automated")]
        public static void Test_GetPageInfo_WithUSS()
        {
            string dbName = "AppBackendAPI_Test_GetPageInfo_WithUSS";

            try
            {
                (FileProcessingDB fileProcessingDb, User user, AppBackendController controller) =
                    _testDbManager.InitializeEnvironment<TestBackendAPI, AppBackendController>
                        ("Resources.Demo_IDShield.bak", dbName, "admin", "a");

                string testFileName = _testFiles.GetFile(_TEST_FILE1);
                _testFiles.GetFile(_TEST_FILE1_USS);

                var fileRecord = fileProcessingDb.AddFile(
                    testFileName, _ACTION_NAME, 1, EFilePriority.kPriorityHigh, false, false,
                    EActionStatus.kActionPending, false, out bool t1, out EActionStatus t2);
                int fileId = fileRecord.FileID;

                var result = controller.Login(user);
                result.AssertGoodResult<LoginToken>();

                result = controller.OpenDocument();
                var openDocumentResult = result.AssertGoodResult<DocumentId>();
                Assert.AreEqual(fileId, openDocumentResult.Id);

                result = controller.GetPageInfo();
                var pagesInfo = result.AssertGoodResult<PagesInfo>();

                Assert.AreEqual(pagesInfo.PageCount, 4, "Unexpected page count");
                Assert.AreEqual(pagesInfo.PageInfos.Count, 4, "Unexpected page infos count");
                for (int page = 1; page <= 4; page++)
                {
                    var pageInfo = pagesInfo.PageInfos[page - 1];

                    Assert.AreEqual(page, pageInfo.Page, "Unexpected page number");
                    Assert.AreEqual((page == 2) ? 2200 : 1712, pageInfo.Width, "Unexpected page width");
                    Assert.AreEqual((page == 2) ? 1712 : 2200, pageInfo.Height, "Unexpected page height");
                    Assert.AreEqual((page == 2) ? 270 : 0, pageInfo.DisplayOrientation, "Unexpected page orientation");
                }

                result = controller.Logout();
                result.AssertGoodResult<GenericResult>();
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
        [Test, Category("Automated")]
        public static void Test_GetPageImage()
        {
            string dbName = "AppBackendAPI_Test_GetPageImage";

            try
            {
                (FileProcessingDB fileProcessingDb, User user, AppBackendController controller) =
                    _testDbManager.InitializeEnvironment<TestBackendAPI, AppBackendController>
                        ("Resources.Demo_IDShield.bak", dbName, "admin", "a");

                string testFileName = _testFiles.GetFile(_TEST_FILE1);

                var fileRecord = fileProcessingDb.AddFile(
                    testFileName, _ACTION_NAME, 1, EFilePriority.kPriorityHigh, false, false,
                    EActionStatus.kActionPending, false, out bool t1, out EActionStatus t2);

                var result = controller.Login(user);
                result.AssertGoodResult<LoginToken>();

                result = controller.OpenDocument();
                var openDocumentResult = result.AssertGoodResult<DocumentId>();

                using (var codecs = new ImageCodecs())
                    for (int page = 1; page <= 4; page++)
                    {
                        result = controller.GetDocumentPage(page);
                        var fileResult = result.AssertGoodResult<FileContentResult>();

                        using (var temporaryFile = new TemporaryFile(".pdf", false))
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

                result = controller.Logout();
                result.AssertGoodResult<GenericResult>();
            }
            finally
            {
                FileApiMgr.ReleaseAll();
                _testDbManager.RemoveDatabase(dbName);

                _testFiles.RemoveFile(_TEST_FILE1);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [Test, Category("Automated")]
        public static void AppBackendAPI_Test_GetDocumentData()
        {
            string dbName = "AppBackendAPI_Test_GetDocumentData";

            try
            {
                (FileProcessingDB fileProcessingDb, User user, AppBackendController controller) =
                    _testDbManager.InitializeEnvironment<TestBackendAPI, AppBackendController>
                        ("Resources.Demo_IDShield.bak", dbName, "admin", "a");

                var result = controller.Login(user);
                result.AssertGoodResult<LoginToken>();

                result = controller.OpenDocument();
                result.AssertGoodResult<DocumentId>();

                result = controller.GetDocumentData();
                var attributeSet = result.AssertGoodResult<DocumentAttributeSet>();

                Assert.IsTrue(attributeSet.Attributes.Count > 0);
                foreach (var attribute in attributeSet.Attributes)
                {
                    // Per discussion with GGK, non-spacial attributes will not be sent.
                    Assert.IsTrue(attribute.HasPositionInfo);
                }

                result = controller.Logout();
                result.AssertGoodResult<GenericResult>();
            }
            finally
            {
                FileApiMgr.ReleaseAll();
                _testDbManager.RemoveDatabase(dbName);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [Test, Category("Automated")]
        public static void AppBackendAPI_Test_SaveDocumentData()
        {
            string dbName = "AppBackendAPI_Test_SaveDocumentData";

            try
            {
                (FileProcessingDB fileProcessingDb, User user, AppBackendController controller) =
                    _testDbManager.InitializeEnvironment<TestBackendAPI, AppBackendController>
                        ("Resources.Demo_IDShield.bak", dbName, "admin", "a");

                var result = controller.Login(user);
                result.AssertGoodResult<LoginToken>();

                result = controller.OpenDocument();
                result.AssertGoodResult<DocumentId>();

                result = controller.GetDocumentData();
                var attributeSet = result.AssertGoodResult<DocumentAttributeSet>();

                var updatedAttributes = attributeSet.Attributes.Skip(1);
                Assert.AreEqual(updatedAttributes.Count() + 1, attributeSet.Attributes.Count());

                var updateAttributeSet = new BareDocumentAttributeSet()
                {
                    Attributes = new List<DocumentAttribute>(updatedAttributes)
                };

                result = controller.SaveDocumentData(updateAttributeSet);
                result.AssertGoodResult<GenericResult>();

                result = controller.GetDocumentData();
                attributeSet = result.AssertGoodResult<DocumentAttributeSet>();

                Assert.AreEqual(attributeSet.Attributes.Count(), updatedAttributes.Count());

                result = controller.Logout();
                result.AssertGoodResult<GenericResult>();
            }
            finally
            {
                FileApiMgr.ReleaseAll();
                _testDbManager.RemoveDatabase(dbName);
            }
        }

        #endregion Public Test Functions
    }
}
