﻿using Extract.FileActionManager.Database.Test;
using Extract.FileActionManager.FileProcessors;
using Extract.Imaging;
using Extract.Testing.Utilities;
using Extract.Utilities;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;
using UCLID_RASTERANDOCRMGMTLib;
using WebAPI.Controllers;
using WebAPI.Models;

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
                // Actually validating the token data looks tricky; verify that it looks right at least.
                Assert.IsTrue(loginToken.access_token.ToString().StartsWith("eyJhb", StringComparison.OrdinalIgnoreCase));

                // Login should register an active FAM session
                Assert.IsTrue(fileProcessingDb.IsAnyFAMActive());

                result = controller.Logout();
                result.AssertGoodResult<GenericResult>();

                // Logout should close the FAM session
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

                Assert.AreEqual(EActionStatus.kActionProcessing, fileProcessingDb.GetFileStatus(1, _VERIFY_ACTION, false));

                result = controller.CloseDocument(commit: false);
                result.AssertGoodResult<GenericResult>();

                Assert.AreEqual(EActionStatus.kActionPending, fileProcessingDb.GetFileStatus(1, _VERIFY_ACTION, false));

                result = controller.OpenDocument();
                result.AssertGoodResult<DocumentId>();

                Assert.AreEqual(EActionStatus.kActionProcessing, fileProcessingDb.GetFileStatus(1, _VERIFY_ACTION, false));

                result = controller.CloseDocument(commit: true);
                result.AssertGoodResult<GenericResult>();

                Assert.AreEqual(EActionStatus.kActionCompleted, fileProcessingDb.GetFileStatus(1, _VERIFY_ACTION, false));

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

                Assert.AreEqual(EActionStatus.kActionProcessing, fileProcessingDb.GetFileStatus(1, _VERIFY_ACTION, false));

                result = controller.CloseDocument(commit: false);
                result.AssertGoodResult<GenericResult>();

                Assert.AreEqual(EActionStatus.kActionPending, fileProcessingDb.GetFileStatus(1, _VERIFY_ACTION, false));

                result = controller.OpenDocument(2);
                openDocumentResult = result.AssertGoodResult<DocumentId>();
                Assert.AreEqual(2, openDocumentResult.Id);

                Assert.AreEqual(EActionStatus.kActionProcessing, fileProcessingDb.GetFileStatus(2, _VERIFY_ACTION, false));

                result = controller.CloseDocument(commit: true);
                result.AssertGoodResult<GenericResult>();

                Assert.AreEqual(EActionStatus.kActionCompleted, fileProcessingDb.GetFileStatus(2, _VERIFY_ACTION, false));

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

                Assert.AreEqual(EActionStatus.kActionPending, fileProcessingDb.GetFileStatus(2, _VERIFY_ACTION, false));

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

                // After document is closed, OpenDocument should open the first file in the queue,
                // not the 2nd file that had been specified originally.
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

                Assert.AreEqual(EActionStatus.kActionProcessing, fileProcessingDb.GetFileStatus(1, _VERIFY_ACTION, false));

                result = controller.Logout();
                result.AssertGoodResult<GenericResult>();

                Assert.AreEqual(EActionStatus.kActionPending, fileProcessingDb.GetFileStatus(1, _VERIFY_ACTION, false));
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
                    testFileName, _VERIFY_ACTION, 1, EFilePriority.kPriorityHigh, false, false,
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
                    // Page 2 is rotated 90 degrees to the right.
                    Assert.AreEqual((page == 2) ? 2200 : 1712, pageInfo.Width, "Unexpected page width");
                    Assert.AreEqual((page == 2) ? 1712 : 2200, pageInfo.Height, "Unexpected page height");
                    // With no USS, the DisplayOrientation will still be zero.
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
                    testFileName, _VERIFY_ACTION, 1, EFilePriority.kPriorityHigh, false, false,
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
                    // Page 2 is rotated 90 degrees to the right.
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
                    testFileName, _VERIFY_ACTION, 1, EFilePriority.kPriorityHigh, false, false,
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

        [Test, Category("Automated")]
        public static void Test_GetDocumentData()
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
                    // Per discussion with GGK, non-spatial attributes will not be sent.
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

        [Test, Category("Automated")]
        public static void Test_SaveDocumentData()
        {
            string dbName = "AppBackendAPI_Test_SaveDocumentData";

            try
            {
                (FileProcessingDB fileProcessingDb, User user, AppBackendController controller) =
                    _testDbManager.InitializeEnvironment<TestBackendAPI, AppBackendController>
                        ("Resources.Demo_IDShield.bak", dbName, "admin", "a");

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

                _testFiles.RemoveFile(_TEST_FILE1);
                _testFiles.RemoveFile(_TEST_FILE1_USS);
                _testFiles.RemoveFile(_TEST_FILE1_VOA);
            }
        }

        [Test, Category("Automated")]
        public static void Test_GetDocumentWordZones()
        {
            string dbName = "AppBackendAPI_Test_GetDocumentWordZones";

            try
            {
                (FileProcessingDB fileProcessingDb, User user, AppBackendController controller) =
                   _testDbManager.InitializeEnvironment<TestBackendAPI, AppBackendController>
                       ("Resources.Demo_IDShield.bak", dbName, "admin", "a");

                string testFileName = _testFiles.GetFile(_TEST_FILE1);
                string ussFileName = _testFiles.GetFile(_TEST_FILE1_USS);

                var fileRecord = fileProcessingDb.AddFile(
                    testFileName, _VERIFY_ACTION, 1, EFilePriority.kPriorityHigh, false, false,
                    EActionStatus.kActionPending, false, out bool t1, out EActionStatus t2);
                int fileId = fileRecord.FileID;

                var result = controller.Login(user);
                result.AssertGoodResult<LoginToken>();

                result = controller.OpenDocument();
                result.AssertGoodResult<DocumentId>();

                var documentText = new SpatialString();
                documentText.LoadFrom(ussFileName, false);

                var documentPages = documentText.GetPages(false, "");

                for (int page = 1; page <= documentPages.Size(); page++)
                {
                    var pageText = documentText.GetSpecifiedPages(page, page);
                    IUnknownVector pageWords = pageText.GetWords();

                    result = controller.GetPageWordZones(page);
                    var wordZoneData = result.AssertGoodResult<WordZoneData>();
                    Assert.AreEqual(pageWords.Size(), wordZoneData.Zones.Count(), "Unexpected number of words");

                    for (int i = 0; i < wordZoneData.Zones.Count(); i++)
                    {
                        var wordZone = wordZoneData.Zones[i];

                        SpatialString spatialStringWord = (SpatialString)pageWords.At(i);
                        var spatialStringZone = (ComRasterZone)spatialStringWord.GetOriginalImageRasterZones().At(0);

                        Assert.AreEqual(page, wordZone.PageNumber, "Incorrect page");
                        Assert.AreEqual(spatialStringZone.StartX, wordZone.StartX, "Incorrect StartX");
                        Assert.AreEqual(spatialStringZone.StartY, wordZone.StartY, "Incorrect StartY");
                        Assert.AreEqual(spatialStringZone.EndX, wordZone.EndX, "Incorrect EndX");
                        Assert.AreEqual(spatialStringZone.EndY, wordZone.EndY, "Incorrect EndY");
                        Assert.AreEqual(spatialStringZone.Height, wordZone.Height, "Incorrect height");
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
                _testFiles.RemoveFile(_TEST_FILE1_USS);
            }
        }

        #endregion Public Test Functions
    }
}
