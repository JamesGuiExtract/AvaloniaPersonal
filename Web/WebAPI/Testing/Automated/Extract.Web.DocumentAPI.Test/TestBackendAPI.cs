using Extract.FileActionManager.Database.Test;
using Extract.Imaging;
using Extract.Testing.Utilities;
using Extract.Utilities;
using NUnit.Framework;
using System;
using System.IO;
using UCLID_FILEPROCESSINGLib;
using WebAPI.Models;

namespace Extract.Web.WebAPI.Test
{
    [TestFixture]
    [NUnit.Framework.Category("AppBackendAPI")]
    public class TestBackendAPI
    {
        #region Constants

        static readonly string _LABDE_TEST_FILE1 = "Resources.TestImage003.tif";
        static readonly string _LABDE_TEST_FILE1_USS = "Resources.TestImage003.tif.uss";
        static readonly string _ACTION_NAME = "Compute";

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
        public static void Test_Login()
        {
            string dbName = "AppBackendAPI_Test_Login";

            try
            {
                _testDbManager.GetDatabase("Resources.Demo_IDShield.bak", dbName);

                ApiTestUtils.SetDefaultApiContext(dbName);

                var user = new User()
                {
                    Username = "admin",
                    Password = "a"
                };

                using (var userData = new UserData(ApiTestUtils.GetCurrentApiContext))
                {
                    userData.LoginUser(user);
                }
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
        public static void Test_GetPageInfo_NoUSS()
        {
            string dbName = "AppBackendAPI_Test_GetPageInfo_NoUSS";

            try
            {
                var fileProcessingDb = _testDbManager.GetDatabase("Resources.Demo_IDShield.bak", dbName);
                string testFileName = _testFiles.GetFile(_LABDE_TEST_FILE1);

                ApiTestUtils.SetDefaultApiContext(dbName);

                using (var data = new DocumentData(ApiTestUtils.GetCurrentApiContext))
                {
                    var fileRecord = fileProcessingDb.AddFile(
                        testFileName, _ACTION_NAME, 1, EFilePriority.kPriorityNormal, false, false,
                        EActionStatus.kActionPending, false, out bool t1, out EActionStatus t2);
                    int fileId = fileRecord.FileID;

                    var pagesInfo = data.GetPagesInfo(fileId);
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
                }
            }
            finally
            {
                FileApiMgr.ReleaseAll();
                _testDbManager.RemoveDatabase(dbName);

                _testFiles.RemoveFile(_LABDE_TEST_FILE1);
            }
        }

        [Test, Category("Automated")]
        public static void Test_GetPageInfo_WithUSS()
        {
            string dbName = "AppBackendAPI_Test_GetPageInfo_NoUSS";

            try
            {
                var fileProcessingDb = _testDbManager.GetDatabase("Resources.Demo_IDShield.bak", dbName);
                string testFileName = _testFiles.GetFile(_LABDE_TEST_FILE1);
                _testFiles.GetFile(_LABDE_TEST_FILE1_USS);

                ApiTestUtils.SetDefaultApiContext(dbName);

                using (var data = new DocumentData(ApiTestUtils.GetCurrentApiContext))
                {
                    var fileRecord = fileProcessingDb.AddFile(
                        testFileName, _ACTION_NAME, 1, EFilePriority.kPriorityNormal, false, false,
                        EActionStatus.kActionPending, false, out bool t1, out EActionStatus t2);
                    int fileId = fileRecord.FileID;

                    var pagesInfo = data.GetPagesInfo(fileId);
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
                }
            }
            finally
            {
                FileApiMgr.ReleaseAll();
                _testDbManager.RemoveDatabase(dbName);

                _testFiles.RemoveFile(_LABDE_TEST_FILE1);
                _testFiles.RemoveFile(_LABDE_TEST_FILE1_USS);
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
                var fileProcessingDb = _testDbManager.GetDatabase("Resources.Demo_IDShield.bak", dbName);
                string testFileName = _testFiles.GetFile(_LABDE_TEST_FILE1);

                ApiTestUtils.SetDefaultApiContext(dbName);

                using (var data = new DocumentData(ApiTestUtils.GetCurrentApiContext))
                using (var imageCodecs = new ImageCodecs())
                {
                    var fileRecord = fileProcessingDb.AddFile(
                        testFileName, _ACTION_NAME, 1, EFilePriority.kPriorityNormal, false, false,
                        EActionStatus.kActionPending, false, out bool t1, out EActionStatus t2);
                    int fileId = fileRecord.FileID;

                    for (int page = 1; page <= 4; page++)
                    {
                        var pageImage = data.GetPageImage(fileId, page);
                        using (var temporaryFile = new TemporaryFile(".pdf", false))
                        using (var fileStream = File.OpenWrite(temporaryFile.FileName))
                        {
                            fileStream.Write(pageImage, 0, pageImage.Length);
                            fileStream.Flush();
                            fileStream.Close();

                            using (var imageReader = imageCodecs.CreateReader(temporaryFile.FileName))
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
                }
            }
            finally
            {
                FileApiMgr.ReleaseAll();
                _testDbManager.RemoveDatabase(dbName);

                _testFiles.RemoveFile(_LABDE_TEST_FILE1);
            }
        }
        
        #endregion Public Test Functions
    }
}
