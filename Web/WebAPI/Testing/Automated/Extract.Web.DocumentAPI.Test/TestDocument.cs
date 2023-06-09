﻿using DynamicData.Kernel;
using Extract.FileActionManager.Database.Test;
using Extract.SqlDatabase;
using Extract.Testing.Utilities;
using Extract.Utilities;
using Extract.Web.ApiConfiguration.Models;
using Extract.Web.ApiConfiguration.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using org.apache.pdfbox.pdmodel;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using UCLID_AFCORELib;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;
using UCLID_RASTERANDOCRMGMTLib;
using WebAPI;
using WebAPI.Controllers;
using WebAPI.Models;

using ComRasterZone = UCLID_RASTERANDOCRMGMTLib.RasterZone;

namespace Extract.Web.WebAPI.Test
{
    [TestFixture]
    [NUnit.Framework.Category("DocumentAPI")]
    public class TestDocument
    {
        // VERSIONS:
        // 2.0 = LEGACY_VERSION
        // 3.0 = First version after introducing versioning
        // 4.0 = CURRENT_VERSION; Added GetMetadataField and SetMetadataField

        #region Constants

        const int MaxDemo_LabDE_FileId = 18;

        const int OutputFileMetadataFieldID = 6;

        static readonly string _IDS_VERIFY_ACTION = "Verify";
        static readonly string _LABDE_VERIFY_ACTION = "A02_Verify";
        static readonly string _TEST_FILE_TESTIMAGE001 = "Resources.TestImage001.tif";
        static readonly string _TEST_FILE_TESTIMAGE001_USS = "Resources.TestImage001.tif.uss";
        static readonly string _TEST_FILE_C413 = "Resources.C413.tif";
        static readonly string _TEST_FILE_C413_USS = "Resources.C413.tif.uss";

        static readonly string _WBC_ATTRIBUTE_GUID = "7b493421-299a-423c-80b2-b37043597081";
        static readonly string _HEMOGLOBIN_ATTRIBUTE_GUID = "0ec90152-3c89-4d22-84a0-17322b17b888";
        static readonly string _PATIENT_NAME_ATTRIBUTE_GUID = "edd589e3-fe6f-4b5c-a7c1-5879610e9c07";

        // Index for FulllText node, always the zeroeth child node of parent nodes.
        const int FullTextIndex = 0;

        // Index for AverageCharConfidence attribute in FullText node
        const int AvgCharConf = 0;

        // Indexes for SpatialLineZone attributes
        const int StartX = 0;
        const int StartY = 1;
        const int EndX = 2;
        const int EndY = 3;

        // Indexes for SpatialLineBounds attributes
        const int Top = 0;
        const int Left = 1;
        const int Bottom = 2;
        const int Right = 3;
        private static readonly DocumentApiConfiguration _idShieldDefaultConfiguration = new(
            configurationName: "DocumentAPITesting",
            isDefault: true,
            workflowName: "CourtOffice",
            attributeSet: "Attr",
            processingAction: "Verify",
            postProcessingAction: "Output",
            documentFolder: @"c:\temp\DocumentFolder",
            startAction: "Compute",
            endAction: "Output",
            postWorkflowAction: "",
            outputFileNameMetadataField: "Outputfile",
            outputFileNameMetadataInitialValueFunction: "<SourceDocName>.result.tif");

        private static readonly DocumentApiConfiguration _flexIndexDefaultConfiguration = new(
            configurationName: "DocumentAPITesting",
            isDefault: true,
            workflowName: "CourtOffice",
            attributeSet: "Attr",
            processingAction: "Verify",
            postProcessingAction: "Output",
            documentFolder: @"c:\temp\DocumentFolder",
            startAction: "ExtractData",
            endAction: "Output",
            postWorkflowAction: "",
            outputFileNameMetadataField: "Outputfile",
            outputFileNameMetadataInitialValueFunction: "<SourceDocName>.result.tif");

        private static readonly DocumentApiConfiguration _labDEDefaultConfiguration = new(
            configurationName: "DocumentAPITesting",
            isDefault: true,
            workflowName: "CourtOffice",
            attributeSet: "DataFoundByRules",
            processingAction: "A02_Verify",
            postProcessingAction: "Z_AdminAction",
            documentFolder: @"c:\temp\DocumentFolder",
            startAction: "A01_ExtractData",
            endAction: "Z_AdminAction",
            postWorkflowAction: "",
            outputFileNameMetadataField: "Outputfile",
            outputFileNameMetadataInitialValueFunction: "<SourceDocName>.result.tif");

        #endregion Constants

        /// <summary>
        /// DTO to simplify writing the test
        /// </summary>
        class FileInfo
        {
            /// <summary>
            /// the resource-embedded xml file name to test DocumentAtributeSet against
            /// </summary>
            public string XmlFile { get; set; }

            /// <summary>
            /// The database name to fetch the DocumentAttributeSet from. Note that this
            /// value is used to set the web server Document API database name.
            /// </summary>
            public string DatabaseName { get; set; }

            /// <summary>
            /// The AttributeSetName to specify to retrieve the DocumentAttributeSet
            /// Note that this value is used to set the Web API Document API AttributeSetName.
            /// </summary>
            public string AttributeSetName { get; set; }
        }

        /// <summary>
        /// These dictionaries use fileId as the key for a FileInfo object that describes the
        /// parameters necessary to get an associated DocumentAttributeSet.
        /// </summary>
        static readonly Dictionary<int, FileInfo> LabDEFileIdToFileInfo = new()
        {
            {1, new FileInfo {XmlFile = "Resources.A418.tif.restored.xml", AttributeSetName = "DataFoundByRules" } },
            {12, new FileInfo {XmlFile = "Resources.K151.tif.restored.xml", AttributeSetName = "DataFoundByRules" } }
        };

        static readonly Dictionary<int, FileInfo> IDShieldFileIdToFileInfo = new()
        {
            {2, new FileInfo {XmlFile = "Resources.TestImage002.tif.restored.xml", AttributeSetName = "Attr" } },
            {3, new FileInfo {XmlFile = "Resources.TestImage003.tif.restored.xml", AttributeSetName = "Attr" } }
        };

        static readonly Dictionary<int, FileInfo> FlexIndexFileIdToFileInfo = new()
        {
            {1, new FileInfo {XmlFile = "Resources.Example01.tif.restored.xml", AttributeSetName = "Attr"} },
            {3, new FileInfo {XmlFile = "Resources.Example03.tif.restored.xml", AttributeSetName = "Attr"} }
        };

        #region Fields
        /// <summary>
        /// test DB Manager, used to extract a database backup file from the resource, and the attach/detach it
        /// to the local database server. 
        /// </summary>
        static FAMTestDBManager<TestDocument> _testDbManager;

        static TestFileManager<TestDocument> _testFiles;

        #endregion Fields

        [OneTimeSetUp]
        public static void Setup()
        {
            GeneralMethods.TestSetup();
            _testDbManager = new FAMTestDBManager<TestDocument>();

            _testFiles = new TestFileManager<TestDocument>("452534BF-684A-446C-9C69-057489BDEC23");
        }

        [OneTimeTearDown]
        public static void FinalCleanup()
        {
            if (_testDbManager != null)
            {
                _testDbManager.Dispose();
                _testDbManager = null;
            }

            if (_testFiles != null)
            {
                _testFiles.Dispose();
                _testFiles = null;
            }
        }

        #region Public Test Functions

        /// <summary>
        /// Tests both DocumentController.PostDocument
        /// </summary>
        [Test, Category("Automated")]
        [TestCase(ApiContext.LEGACY_VERSION)]
        [TestCase(ApiContext.CURRENT_VERSION)]
        public static void Test_PostDocument(string apiVersion)
        {
            string dbName = $"Test_DocumentAPI_PostDocument_{apiVersion}";

            try
            {
                (FileProcessingDB fileProcessingDb, User user, DocumentController controller) =
                    InitializeAndLogin(apiVersion, "Resources.Demo_LabDE.bak", dbName, "jon_doe", "123");

                var filename = _testFiles.GetFile("Resources.A418.tif");
                using (var stream = new FileStream(filename, FileMode.Open))
                {
                    var formFile = new FormFile(stream, 0, stream.Length, filename, filename);

                    var result = controller.PostDocument(formFile);
                    var submitResult1 = result.AssertGoodResult<DocumentIdResult>();

                    // It is OK to re-submit - the web service writes a unique filename, based on the 
                    // submitted filename, so test this as well.
                    result = controller.PostDocument(formFile);
                    var submitResult2 = result.AssertGoodResult<DocumentIdResult>();

                    var fileName1 = fileProcessingDb.GetFileNameFromFileID(submitResult1.Id);
                    var fileName2 = fileProcessingDb.GetFileNameFromFileID(submitResult2.Id);

                    Assert.AreNotEqual(fileName1, fileName2,
                        "source filename: {0}, not equal to original filename: {1}",
                        fileName1,
                        fileName2);

                    // Can't directly test that filenames are equivalent, because the web service takes the base filename
                    // and adds a GUID to it, so test that the returned filename contains the original filename.
                    var originalFilename = Path.GetFileNameWithoutExtension(filename);
                    Assert.IsTrue(fileName1.Contains(originalFilename),
                        "source filename: {0}, not equal to original filename: {1}",
                        fileName1,
                        filename);
                }
            }
            finally
            {
                FileApiMgr.Instance.ReleaseAll();
                _testDbManager.RemoveDatabase(dbName);
            }
        }

        /// <summary>
        /// Tests that metadata is set properly when the appropriate metadata fields exist.
        /// Tests https://extract.atlassian.net/browse/ISSUE-16872
        /// </summary>
        [Test, Category("Automated")]
        [TestCase(ApiContext.LEGACY_VERSION)]
        [TestCase(ApiContext.CURRENT_VERSION)]
        public static void Test_PostDocumentMetadata(string apiVersion)
        {
            string dbName = $"Test_DocumentAPI_PostDocumentMetadata_{apiVersion}";

            Mock<IConfigurationDatabaseService> mock = new();
            mock.Setup(x => x.DocumentAPIWebConfigurations).Returns(new List<IDocumentApiWebConfiguration>() { _labDEDefaultConfiguration });
            mock.Setup(x => x.Configurations).Returns(new List<ICommonWebConfiguration>() { _labDEDefaultConfiguration });

            try
            {
                (FileProcessingDB fileProcessingDb, User user, UsersController userController) =
                _testDbManager.InitializeEnvironment(
                    controller: () => new UsersController(mock.Object, _labDEDefaultConfiguration)
                    , apiVersion: apiVersion
                    , dbResource: "Resources.Demo_LabDE.bak"
                    , dbName: dbName
                    , username: "jon_doe"
                    , password: "123"
                    , webConfiguration: _labDEDefaultConfiguration);

                // Add metadata fields to the DB before the DocumentData class gets around to caching which metadata
                // fields exist.
                fileProcessingDb.AddMetadataField("OriginalFileName");
                fileProcessingDb.AddMetadataField("UploaderReportedFileName");

                var result = userController.Login(user);
                var token = result.AssertGoodResult<JwtSecurityToken>();

                var controller = user.SetupController(new DocumentController(mock.Object));

                var filename = _testFiles.GetFile("Resources.A418.tif");
                using (var stream = new FileStream(filename, FileMode.Open))
                {
                    var formFile = new FormFile(stream, 0, stream.Length, filename, filename);

                    result = controller.PostDocument(formFile);
                    var submitResult1 = result.AssertGoodResult<DocumentIdResult>();

                    Assert.AreEqual(Path.GetFileName(filename), fileProcessingDb.GetMetadataFieldValue(submitResult1.Id, "OriginalFileName"));
                    Assert.AreEqual(filename, fileProcessingDb.GetMetadataFieldValue(submitResult1.Id, "UploaderReportedFileName"));
                }
            }
            finally
            {
                FileApiMgr.Instance.ReleaseAll();
                _testDbManager.RemoveDatabase(dbName);
            }
        }

        /// <summary>
        /// Tests corner cases related to document metadata fields:
        /// - Missing metadata field in the in the database
        /// - Filename only specified (not path)
        /// - Invalid file paths specified
        /// Tests https://extract.atlassian.net/browse/ISSUE-16872
        /// </summary>
        [Test, Category("Automated")]
        [TestCase(ApiContext.LEGACY_VERSION)]
        [TestCase(ApiContext.CURRENT_VERSION)]
        public static void Test_PostDocumentMetadataCornerCases(string apiVersion)
        {
            string dbName = $"Test_DocumentAPI_PostDocumentMetadataCornerCases_{apiVersion}";

            try
            {
                Mock<IConfigurationDatabaseService> mock = new();
                mock.Setup(x => x.DocumentAPIWebConfigurations).Returns(new List<IDocumentApiWebConfiguration>() { _labDEDefaultConfiguration });
                mock.Setup(x => x.Configurations).Returns(new List<ICommonWebConfiguration>() { _labDEDefaultConfiguration });

                (FileProcessingDB fileProcessingDb, User user, UsersController userController) =
                _testDbManager.InitializeEnvironment(
                    controller: () => new UsersController(mock.Object, _labDEDefaultConfiguration)
                    , apiVersion: apiVersion
                    , dbResource: "Resources.Demo_LabDE.bak"
                    , dbName: dbName
                    , username: "jon_doe"
                    , password: "123"
                    , webConfiguration: _labDEDefaultConfiguration);

                // Add metadata field to the DB before the DocumentData class gets around to caching which metadata
                // fields exist.
                fileProcessingDb.AddMetadataField("OriginalFileName");
                // TEST: Missing UploaderReportedFileName metadata field

                var result = userController.Login(user);
                var token = result.AssertGoodResult<JwtSecurityToken>();

                var controller = user.SetupController(new DocumentController(mock.Object));

                var filename = _testFiles.GetFile("Resources.A418.tif");
                using (var stream = new FileStream(filename, FileMode.Open))
                {
                    // TEST: Filename only specified (not path)
                    string fileNameOnly = Path.GetFileName(filename);

                    var formFile = new FormFile(stream, 0, stream.Length, filename, fileNameOnly);

                    result = controller.PostDocument(formFile);
                    var submitResult = result.AssertGoodResult<DocumentIdResult>();

                    Assert.AreEqual(fileNameOnly, fileProcessingDb.GetMetadataFieldValue(submitResult.Id, "OriginalFileName"));
                }

                using (var stream = new FileStream(filename, FileMode.Open))
                {
                    // TEST: Invalid file paths specified
                    string badFileName = "Garbage;path!Name/";

                    var formFile = new FormFile(stream, 0, stream.Length, filename, badFileName);

                    result = controller.PostDocument(formFile);
                    var submitResult = result.AssertGoodResult<DocumentIdResult>();

                    Assert.IsTrue(string.IsNullOrWhiteSpace(fileProcessingDb.GetMetadataFieldValue(submitResult.Id, "OriginalFileName")));
                }
            }
            finally
            {
                FileApiMgr.Instance.ReleaseAll();
                _testDbManager.RemoveDatabase(dbName);
            }
        }

        /// <summary>
        /// Tests both DocumentData.SubmitText and DocumentData.GetSourceFileName
        /// </summary>
        [Test, Category("Automated")]
        [TestCase(ApiContext.LEGACY_VERSION)]
        [TestCase(ApiContext.CURRENT_VERSION)]
        public static void Test_PostText(string apiVersion)
        {
            string dbName = $"Test_DocumentAPI_PostText_{apiVersion}";

            try
            {
                (FileProcessingDB fileProcessingDb, User user, DocumentController controller) =
                    InitializeAndLogin(apiVersion, "Resources.Demo_LabDE.bak", dbName, "jon_doe", "123");

                var result = controller.PostText("Document 1, SSN: 111-22-3333, DOB: 10-04-1999");
                var submitResult = result.AssertGoodResult<DocumentIdResult>();

                var sourceFilename = fileProcessingDb.GetFileNameFromFileID(submitResult.Id);
                Assert.IsTrue(!string.IsNullOrEmpty(sourceFilename), "Failed to post text");
            }
            finally
            {
                FileApiMgr.Instance.ReleaseAll();
                _testDbManager.RemoveDatabase(dbName);
            }
        }

        [Test, Category("Automated")]
        [TestCase(ApiContext.LEGACY_VERSION)]
        [TestCase(ApiContext.CURRENT_VERSION)]
        public static void Test_UTF8Text(string apiVersion)
        {
            string dbName = $"Test_DocumentAPI_UTF8Text_{apiVersion}";

            try
            {
                (FileProcessingDB fileProcessingDb, User user, DocumentController controller) =
                    InitializeAndLogin(apiVersion, "Resources.Demo_LabDE.bak", dbName, "jon_doe", "123");

                var testText = "¡Buenos días! ¿Cómo estás? It's 212°!";

                var result = controller.PostText(testText);
                var submitResult = result.AssertGoodResult<DocumentIdResult>();

                var sourceFilename = fileProcessingDb.GetFileNameFromFileID(submitResult.Id);
                Assert.IsTrue(!string.IsNullOrEmpty(sourceFilename), "Failed to post text");

                result = controller.GetText(submitResult.Id);
                var textResult = result.AssertGoodResult<PageTextResult>();

                Assert.AreEqual(testText, textResult.Pages.Single().Text);
            }
            finally
            {
                FileApiMgr.Instance.ReleaseAll();
                _testDbManager.RemoveDatabase(dbName);
            }
        }

        [Test, Category("Automated")]
        [TestCase(ApiContext.LEGACY_VERSION)]
        [TestCase(ApiContext.CURRENT_VERSION)]
        public static void Test_GetStatus(string apiVersion)
        {
            string dbName = $"Test_DocumentAPI_GetStatus_{apiVersion}";
            Mock<IConfigurationDatabaseService> mock = new();
            mock.Setup(x => x.DocumentAPIWebConfigurations).Returns(new List<IDocumentApiWebConfiguration>() { _labDEDefaultConfiguration });
            mock.Setup(x => x.Configurations).Returns(new List<ICommonWebConfiguration>() { _labDEDefaultConfiguration });

            try
            {
                (FileProcessingDB fileProcessingDb
                , User user
                , UsersController usersController
                , Dictionary<int, DocumentProcessingStatus> expectedStatuses) =
                    ApiTestUtils.CreateStatusTestEnvironment(
                        _testDbManager,
                        configService => new UsersController(configService, _labDEDefaultConfiguration),
                        apiVersion: apiVersion,
                        dbName: dbName,
                        username: "jon_doe",
                        password: "123");

                var token = usersController.Login(user).AssertGoodResult<JwtSecurityToken>();
                var controller = user.SetupController(new DocumentController(mock.Object));

                foreach ((int fileId, DocumentProcessingStatus expectedStatus) in expectedStatuses)
                {
                    var statusResult = controller.GetStatus(fileId).AssertGoodResult<ProcessingStatusResult>();
                    Assert.AreEqual(expectedStatus, statusResult.DocumentStatus,
                        $"Incorrect status for file {fileId}");
                }
            }
            finally
            {
                FileApiMgr.Instance.ReleaseAll();
                _testDbManager.RemoveDatabase(dbName);
            }
        }

        [Test, Category("Automated")]
        [TestCase(ApiContext.LEGACY_VERSION)]
        [TestCase(ApiContext.CURRENT_VERSION)]
        public static void Test_GetOutputFile(string apiVersion)
        {
            string dbName = $"Test_DocumentAPI_GetOutputFile_{apiVersion}";
            var tempFiles = new List<string>();

            try
            {
                (FileProcessingDB fileProcessingDb, User user, DocumentController controller) =
                    InitializeAndLogin(apiVersion, "Resources.Demo_LabDE.bak", dbName, "jon_doe", "123");

                var config = ApiTestUtils.CurrentApiContext.WebConfiguration.ValueOrDefault() as IDocumentApiWebConfiguration;
                Assert.NotNull(config);
                var workflowId = fileProcessingDb.GetWorkflowID(config.WorkflowName);
                var workflow = fileProcessingDb.GetWorkflowDefinition(workflowId);

                for (int i = 1; i <= MaxDemo_LabDE_FileId; ++i)
                {
                    string outputFileName = FileSystemMethods.GetTemporaryFileName();
                    File.WriteAllText(outputFileName, fileProcessingDb.GetFileNameFromFileID(i));
                    tempFiles.Add(outputFileName);

                    fileProcessingDb.SetMetadataFieldValue(i, config.OutputFileNameMetadataField, outputFileName);

                    var result = controller.GetOutputFile(i);
                    var fileResult = result.AssertGoodResult<PhysicalFileResult>();

                    Assert.AreEqual(Path.GetFileName(outputFileName), fileResult.FileDownloadName,
                        "Output file path incorrect");
                }
            }
            finally
            {
                FileApiMgr.Instance.ReleaseAll();
                _testDbManager.RemoveDatabase(dbName);

                foreach (string tempFile in tempFiles)
                {
                    File.Delete(tempFile);
                }
            }
        }

        [Test, Category("Automated")]
        [TestCase(ApiContext.LEGACY_VERSION)]
        [TestCase(ApiContext.CURRENT_VERSION)]
        public static void Test_PostGetDeleteFile(string apiVersion)
        {
            string dbName = $"Test_DocumentAPI_PostGetDeleteFile_{apiVersion}";

            try
            {
                (FileProcessingDB fileProcessingDb, User user, DocumentController controller) =
                    InitializeAndLogin(apiVersion, "Resources.Demo_LabDE.bak", dbName, "jon_doe", "123");

                var filename = _testFiles.GetFile("Resources.A418.tif");
                using (var stream = new FileStream(filename, FileMode.Open))
                {
                    var formFile = new FormFile(stream, 0, stream.Length, filename, filename);

                    var submitResult = controller.PostDocument(formFile)
                        .AssertGoodResult<DocumentIdResult>();

                    var documentResult = controller.GetDocument(submitResult.Id)
                        .AssertGoodResult<PhysicalFileResult>();

                    controller.DeleteDocument(submitResult.Id)
                        .AssertGoodResult<NoContentResult>();

                    // Ensure guid is removed from suggested download filename.
                    Assert.AreEqual(Path.GetFileName(filename), documentResult.FileDownloadName,
                        "Invalid download filename format");

                    // Ensure error is returned if we attempt to get document after deletion.
                    controller.GetDocument(submitResult.Id)
                        .AssertResultCode(StatusCodes.Status404NotFound);
                }
            }
            finally
            {
                FileApiMgr.Instance.ReleaseAll();
                _testDbManager.RemoveDatabase(dbName);
            }
        }

        [Test, Category("Automated")]
        [TestCase(ApiContext.LEGACY_VERSION)]
        [TestCase(ApiContext.CURRENT_VERSION)]
        public static void Test_GetPageInfoAndText(string apiVersion)
        {
            string dbName = $"Test_DocumentAPI_GetPageInfoAndText_{apiVersion}";

            try
            {
                (FileProcessingDB fileProcessingDb, User user, DocumentController controller) =
                    InitializeAndLogin(apiVersion, "Resources.Demo_LabDE.bak", dbName, "jon_doe", "123");

                var testFilename = _testFiles.GetFile(_TEST_FILE_C413);
                var ussFilename = _testFiles.GetFile(_TEST_FILE_C413_USS);

                int fileId = 4; // A 5 page document.
                string fileName = fileProcessingDb.GetFileNameFromFileID(fileId);
                var fileRecord = fileProcessingDb.GetFileRecord(fileName, _LABDE_VERIFY_ACTION);
                fileProcessingDb.RenameFile(fileRecord, testFilename);

                var ussData = new SpatialString();
                ussData.LoadFrom(ussFilename, false);

                var expectedPages = ussData.GetPages(true, "");
                int expectedPageCount = expectedPages.Size();

                var pagesInfoResult = controller.GetPageInfo(fileId)
                    .AssertGoodResult<PagesInfoResult>();

                Assert.AreEqual(expectedPageCount, pagesInfoResult.PageCount,
                    "PageInfo page count incorrect.");

                var pageTextResult = controller.GetText(fileId)
                    .AssertGoodResult<PageTextResult>();

                Assert.AreEqual(expectedPageCount, pageTextResult.Pages.Count,
                    "PageText page count incorrect.");
                for (int page = 1; page <= expectedPageCount; page++)
                {
                    var expectedPage = (SpatialString)expectedPages.At(page - 1);
                    var expectedPageInfo = expectedPage.GetPageInfo(page);
                    var pageInfo = pagesInfoResult.PageInfos[page - 1];
                    Assert.AreEqual(page, pageInfo.Page, "Incorrect PageInfo page number");
                    Assert.AreEqual(expectedPageInfo.Height, pageInfo.Height, "Incorrect page height");
                    Assert.AreEqual(expectedPageInfo.Width, pageInfo.Width, "Incorrect page width");
                    // Last page is upside down
                    Assert.AreEqual(page == 5 ? 180 : 0, pageInfo.DisplayOrientation,
                        "Incorrect page orientation");

                    var pageText = pageTextResult.Pages[page - 1];
                    Assert.AreEqual(page, pageText.Page, "Incorrect PageText page number");
                    Assert.AreEqual(expectedPage.String, pageText.Text, "Incorrect page text");

                    var singlePageResult = controller.GetPageText(fileId, page)
                        .AssertGoodResult<PageTextResult>();

                    Assert.AreEqual(page, singlePageResult.Pages.Single().Page,
                        "Incorrect single page number");
                    Assert.AreEqual(expectedPage.String, singlePageResult.Pages.Single().Text,
                        "Incorrect single page text");
                }
            }
            finally
            {
                FileApiMgr.Instance.ReleaseAll();
                _testDbManager.RemoveDatabase(dbName);

                _testFiles.RemoveFile(_TEST_FILE_C413);
                _testFiles.RemoveFile(_TEST_FILE_C413_USS);
            }
        }

        [Test, Category("Automated")]
        [TestCase(ApiContext.LEGACY_VERSION)]
        [TestCase(ApiContext.CURRENT_VERSION)]
        public static void Test_GetDocumentWordZones(string apiVersion)
        {
            string dbName = $"Test_DocumentAPI_GetDocumentWordZones_{apiVersion}";

            try
            {
                (FileProcessingDB fileProcessingDb, User user, DocumentController controller) =
                    InitializeAndLogin(apiVersion, "Resources.Demo_LabDE.bak", dbName, "jon_doe", "123");

                string testFileName = _testFiles.GetFile(_TEST_FILE_C413);
                string ussFileName = _testFiles.GetFile(_TEST_FILE_C413_USS);

                int fileId = 4; // A 5 page document.
                string fileName = fileProcessingDb.GetFileNameFromFileID(fileId);
                var fileRecord = fileProcessingDb.GetFileRecord(fileName, _LABDE_VERIFY_ACTION);
                fileProcessingDb.RenameFile(fileRecord, testFileName);

                var ussData = new SpatialString();
                ussData.LoadFrom(ussFileName, false);
                var pageCount = ussData.GetLastPageNumber();

                for (int page = 1; page <= pageCount; page++)
                {
                    var pageText = ussData.GetSpecifiedPages(page, page);
                    List<ComRasterZone> pageWords =
                        pageText.GetLines().ToIEnumerable<SpatialString>()
                            .SelectMany(line => line.GetWords().ToIEnumerable<SpatialString>()
                                .Where(word => word.HasSpatialInfo())
                                .Select(word => (ComRasterZone)word.GetOriginalImageRasterZones().At(0)))
                            .ToList();
                    int wordCount = pageWords.Count;

                    var wordZoneData = controller.GetPageWordZones(fileId, page)
                        .AssertGoodResult<WordZoneDataResult>()
                        .Zones
                        .SelectMany(line => line)
                        .ToList();
                    Assert.AreEqual(wordCount, wordZoneData.Count, "Unexpected number of words");

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
            }
            finally
            {
                FileApiMgr.Instance.ReleaseAll();
                _testDbManager.RemoveDatabase(dbName);

                _testFiles.RemoveFile(_TEST_FILE_C413);
                _testFiles.RemoveFile(_TEST_FILE_C413_USS);
            }
        }

        [Test, Category("Automated")]
        [TestCase(ApiContext.LEGACY_VERSION)]
        [TestCase(ApiContext.CURRENT_VERSION)]
        public static void Test_GetTextResult(string apiVersion)
        {
            string dbName = $"Test_DocumentAPI_GetTextResult_{apiVersion}";

            try
            {
                (FileProcessingDB fileProcessingDb, User user, DocumentController controller) =
                    InitializeAndLogin(apiVersion, "Resources.Demo_LabDE.bak", dbName, "jon_doe", "123");

                var filename = _testFiles.GetFile("Resources.ResultText.txt");
                SetupMetadataFieldValue(1, filename, OutputFileMetadataFieldID, dbName);

                var result = controller.GetOutputText(1);
                var textResult = result.AssertGoodResult<PageTextResult>();

                Assert.AreEqual(File.ReadAllText(filename), textResult.Pages.First().Text,
                    "Incorrect text result");
            }
            finally
            {
                FileApiMgr.Instance.ReleaseAll();
                _testDbManager.RemoveDatabase(dbName);
            }
        }

        [Test, Category("Automated")]
        [TestCase(ApiContext.LEGACY_VERSION)]
        [TestCase(ApiContext.CURRENT_VERSION)]
        public static void Test_GetDocumentType(string apiVersion)
        {
            string dbName = $"Test_DocumentAPI_GetDocumentType_{apiVersion}";

            try
            {
                (FileProcessingDB fileProcessingDb, User user, DocumentController controller) =
                    InitializeAndLogin(apiVersion, "Resources.Demo_LabDE.bak", dbName, "jon_doe", "123");

                for (int i = 1; i <= MaxDemo_LabDE_FileId; ++i)
                {
                    var textResult = controller.GetDocumentType(i)
                        .AssertGoodResult<TextData>();
                    Assert.IsTrue(!string.IsNullOrEmpty(textResult.Text), "Document type result is empty");

                    switch (i)
                    {
                        case 6:
                        case 8:
                            Assert.IsTrue(Utils.IsEquivalent(textResult.Text, "NonLab"),
                                "Document type expected to be NonLab, is: {0}", textResult.Text);
                            break;

                        default:
                            Assert.IsTrue(Utils.IsEquivalent(textResult.Text, "Unknown"),
                                "Document type expected to be Unknown, is: {0}", textResult.Text);
                            break;
                    }
                }
            }
            finally
            {
                FileApiMgr.Instance.ReleaseAll();
                _testDbManager.RemoveDatabase(dbName);
            }
        }

        /// <summary>
        /// test IDShield documents
        /// </summary>
        [Test, Category("Automated")]
        [TestCase(ApiContext.LEGACY_VERSION)]
        [TestCase(ApiContext.CURRENT_VERSION)]
        public static void TestIDShield_GetDocumentData(string apiVersion)
        {
            string dbName = $"Test_DocumentAPI_IDShield_GetDocumentData_{apiVersion}";
            Mock<IConfigurationDatabaseService> mock = new();
            mock.Setup(x => x.DocumentAPIWebConfigurations).Returns(new List<IDocumentApiWebConfiguration>() { _idShieldDefaultConfiguration });
            mock.Setup(x => x.Configurations).Returns(new List<ICommonWebConfiguration>() { _idShieldDefaultConfiguration });

            try
            {
                (FileProcessingDB fileProcessingDb, User user, UsersController userController) =
                _testDbManager.InitializeEnvironment(
                    controller: () =>
                    {
                        var configurationDatabaseService = new ConfigurationDatabaseService(new FileProcessingDBClass() { DatabaseName = dbName, DatabaseServer = "(local)" });
                        return new UsersController(configurationDatabaseService, _idShieldDefaultConfiguration);
                    }
                    , apiVersion: apiVersion
                    , dbResource: "Resources.Demo_IDShield.bak"
                    , dbName: dbName
                    , username: "jon_doe"
                    , password: "123"
                    , webConfiguration: _idShieldDefaultConfiguration);

                var controller = user.SetupController(new DocumentController(mock.Object));

                foreach (var kvpFileInfo in IDShieldFileIdToFileInfo)
                {
                    var fileId = kvpFileInfo.Key;
                    var fileInfo = kvpFileInfo.Value;

                    string xmlFileName = _testFiles.GetFile(fileInfo.XmlFile);
                    var documentDataResult = controller.GetDocumentData(fileId).AssertGoodResult<DocumentDataResult>();

                    bool success = CompareXmlToDocumentAttributeSet(xmlFileName, documentDataResult);
                }
            }
            finally
            {
                FileApiMgr.Instance.ReleaseAll();
                _testDbManager.RemoveDatabase(dbName);
            }
        }

        /// <summary>
        /// Test IDShield documents
        /// </summary>
        [Test, Category("Automated")]
        [TestCase(ApiContext.LEGACY_VERSION)]
        [TestCase(ApiContext.CURRENT_VERSION)]
        public static void Test_PutDocumentData(string apiVersion)
        {
            string dbName = $"Test_DocumentAPI_PutDocumentData_{apiVersion}";
            Mock<IConfigurationDatabaseService> mock = new();
            mock.Setup(x => x.DocumentAPIWebConfigurations).Returns(new List<IDocumentApiWebConfiguration>() { _idShieldDefaultConfiguration });
            mock.Setup(x => x.Configurations).Returns(new List<ICommonWebConfiguration>() { _idShieldDefaultConfiguration });

            try
            {
                (FileProcessingDB fileProcessingDb, User user, UsersController userController) =
                _testDbManager.InitializeEnvironment(
                    controller: () =>
                    {
                        var configurationDatabaseService = new ConfigurationDatabaseService(new FileProcessingDBClass() { DatabaseName = dbName, DatabaseServer = "(local)" });
                        return new UsersController(configurationDatabaseService, _idShieldDefaultConfiguration);
                    }
                    , apiVersion: apiVersion
                    , dbResource: "Resources.Demo_IDShield.bak"
                    , dbName: dbName
                    , username: "jon_doe"
                    , password: "123"
                    , webConfiguration: _idShieldDefaultConfiguration);

                var controller = user.SetupController(new DocumentController(mock.Object));

                var testFilename = _testFiles.GetFile(_TEST_FILE_TESTIMAGE001);
                var ussFilename = _testFiles.GetFile(_TEST_FILE_TESTIMAGE001_USS);

                int fileId = 1;
                string fileName = fileProcessingDb.GetFileNameFromFileID(fileId);
                var fileRecord = fileProcessingDb.GetFileRecord(fileName, _IDS_VERIFY_ACTION);
                fileProcessingDb.RenameFile(fileRecord, testFilename);

                var data = controller.GetDocumentData(fileId)
                    .AssertGoodResult<DocumentDataResult>();

                // Delete the first attribute
                var attributeToDelete = data.Attributes.First();
                data.Attributes.Remove(attributeToDelete);
                var deletedID = attributeToDelete.ID;

                // Update the the second attribute, change spatial area via SpatialLineBounds
                var attributeToUpdate = data.Attributes.First();
                var updatedID = attributeToUpdate.ID;
                attributeToUpdate.Value = "Updated";
                attributeToUpdate.SpatialPosition = new Position
                {
                    LineInfo = new List<SpatialLine>(new[]
                    {
                        new SpatialLine
                        {
                            SpatialLineBounds = new SpatialLineBounds
                            {
                                PageNumber = 1,
                                Left = 0,
                                Top = 0,
                                Right = 100,
                                Bottom = 50
                            }
                        }
                    })
                };

                // Add a new attribute using SpatialLineZone to specify location.
                var newAttribute = new DocumentAttribute
                {
                    Name = "Data",
                    ConfidenceLevel = "Manual",
                    HasPositionInfo = true,
                    Value = "Test",
                    SpatialPosition = new Position
                    {
                        LineInfo = new List<SpatialLine>(new[]
                            {
                                new SpatialLine
                                {
                                    SpatialLineZone = new SpatialLineZone
                                    {
                                        PageNumber = 1,
                                        StartX = 1000,
                                        StartY = 1500,
                                        EndX = 2000,
                                        EndY = 1500,
                                        Height = 101
                                    }
                                }
                            })
                    }
                };

                var addedAttributeID = newAttribute.ID;

                var newData = new DocumentDataInput
                {
                    Attributes = data.Attributes
                };

                newData.Attributes.Add(newAttribute);

                controller.PutDocumentData(fileId, newData)
                    .AssertGoodResult<NoContentResult>();

                data = controller.GetDocumentData(fileId)
                    .AssertGoodResult<DocumentDataResult>();

                Assert.AreEqual(2, data.Attributes.Count,
                    "Incorrect number of remaining attributes");

                Assert.AreEqual(0, data.Attributes.Count(a => a.ID == deletedID),
                    "Attribute failed to delete");

                var updatedAttribute = data.Attributes.Single(a => a.ID == updatedID);
                Assert.AreEqual("Updated", updatedAttribute.Value,
                    "Attribute failed to update");
                // Ensure zone of updated attribute was set correctly based on the supplied SpatialLineZone
                var zone = updatedAttribute.SpatialPosition.LineInfo
                    .Single()
                    .SpatialLineZone;
                Assert.AreEqual(1, zone.PageNumber, "Incorrect page");
                Assert.AreEqual(0, zone.StartX, "Incorrect StartX");
                Assert.AreEqual(25, zone.StartY, "Incorrect StartY");
                Assert.AreEqual(100, zone.EndX, "Incorrect EndX");
                Assert.AreEqual(25, zone.EndY, "Incorrect EndY");
                Assert.AreEqual(51, zone.Height, "Incorrect height");

                var addedAttribute = data.Attributes.Single(a => a.ID == addedAttributeID);
                // Ensure bounds were set correctly based on the supplied SpatialLineZone
                var bounds = addedAttribute.SpatialPosition.LineInfo
                    .Single()
                    .SpatialLineBounds;
                Assert.AreEqual(1, bounds.PageNumber, "Incorrect page");
                Assert.AreEqual(1000, bounds.Left, "Incorrect left boundary");
                Assert.AreEqual(1450, bounds.Top, "Incorrect top boundary");
                Assert.AreEqual(2000, bounds.Right, "Incorrect right boundary");
                Assert.AreEqual(1550, bounds.Bottom, "Incorrect bottom boundary");
            }
            finally
            {
                FileApiMgr.Instance.ReleaseAll();
                _testDbManager.RemoveDatabase(dbName);
            }
        }

        /// <summary>
        /// Tests putting an attibute with minimal data and, specifically, with no childAttributes field specified in the json.
        /// Tests https://extract.atlassian.net/browse/ISSUE-16747
        /// </summary>
        [Test, Category("Automated")]
        [TestCase(ApiContext.LEGACY_VERSION)]
        [TestCase(ApiContext.CURRENT_VERSION)]
        public static void Test_PutMinimalDocumentData(string apiVersion)
        {
            string dbName = $"Test_DocumentAPI_PutMinimalDocumentData_{apiVersion}";
            Mock<IConfigurationDatabaseService> mock = new();
            mock.Setup(x => x.DocumentAPIWebConfigurations).Returns(new List<IDocumentApiWebConfiguration>() { _idShieldDefaultConfiguration });
            mock.Setup(x => x.Configurations).Returns(new List<ICommonWebConfiguration>() { _idShieldDefaultConfiguration });

            try
            {
                (FileProcessingDB fileProcessingDb, User user, UsersController userController) =
                _testDbManager.InitializeEnvironment(
                    controller: () =>
                    {
                        var configurationDatabaseService = new ConfigurationDatabaseService(new FileProcessingDBClass() { DatabaseName = dbName, DatabaseServer = "(local)" });
                        return new UsersController(configurationDatabaseService, _idShieldDefaultConfiguration);
                    }
                    , apiVersion: apiVersion
                    , dbResource: "Resources.Demo_IDShield.bak"
                    , dbName: dbName
                    , username: "jon_doe"
                    , password: "123"
                    , webConfiguration: _idShieldDefaultConfiguration);

                var controller = user.SetupController(new DocumentController(mock.Object));

                var testFilename = _testFiles.GetFile(_TEST_FILE_TESTIMAGE001);
                var ussFilename = _testFiles.GetFile(_TEST_FILE_TESTIMAGE001_USS);

                int fileId = 1;

                var minimalAttributeData = JsonConvert.DeserializeObject<DocumentDataInput>(
                    "{\"attributes\": [ {\"name\": \"DocumentType\",\"value\": \"Minimal\"}]}");

                controller.PutDocumentData(fileId, minimalAttributeData)
                    .AssertGoodResult<NoContentResult>();

                var data = controller.GetDocumentData(fileId)
                    .AssertGoodResult<DocumentDataResult>();

                Assert.AreEqual("Minimal", data.Attributes
                    .Single()
                    .Value,
                    "Incorrect attribute value");
            }
            finally
            {
                FileApiMgr.Instance.ReleaseAll();
                _testDbManager.RemoveDatabase(dbName);
            }
        }

        /// <summary>
        /// Tests that document data can be updated via put and patch when documents are in P/S, C and F.
        /// Tests https://extract.atlassian.net/browse/ISSUE-16748
        [Test, Category("Automated")]
        [TestCase(ApiContext.LEGACY_VERSION)]
        [TestCase(ApiContext.CURRENT_VERSION)]

        public static void Test_PutPatchNonPendingFile(string apiVersion)
        {
            string dbName = $"Test_DocumentAPI_PutPatchNonPendingFile_{apiVersion}";
            Mock<IConfigurationDatabaseService> mock = new();
            mock.Setup(x => x.DocumentAPIWebConfigurations).Returns(new List<IDocumentApiWebConfiguration>() { _idShieldDefaultConfiguration });
            mock.Setup(x => x.Configurations).Returns(new List<ICommonWebConfiguration>() { _idShieldDefaultConfiguration });

            try
            {
                (FileProcessingDB fileProcessingDb, User user, UsersController userController) =
                _testDbManager.InitializeEnvironment(
                    controller: () =>
                    {
                        var configurationDatabaseService = new ConfigurationDatabaseService(new FileProcessingDBClass() { DatabaseName = dbName, DatabaseServer = "(local)" });
                        return new UsersController(configurationDatabaseService, _idShieldDefaultConfiguration);
                    }
                    , apiVersion: apiVersion
                    , dbResource: "Resources.Demo_IDShield.bak"
                    , dbName: dbName
                    , username: "jon_doe"
                    , password: "123"
                    , webConfiguration: _idShieldDefaultConfiguration);

                var controller = user.SetupController(new DocumentController(mock.Object));

                var config = ApiTestUtils.CurrentApiContext.WebConfiguration.ValueOrDefault();
                Assert.NotNull(config);
                var workflowId = fileProcessingDb.GetWorkflowID(config.WorkflowName);
                var workflow = fileProcessingDb.GetWorkflowDefinition(workflowId);

                var testFilename = _testFiles.GetFile(_TEST_FILE_TESTIMAGE001);
                var ussFilename = _testFiles.GetFile(_TEST_FILE_TESTIMAGE001_USS);

                int fileId = 1;

                // First try using PutDocumentData for a document in various documents states
                var testInput = new DocumentDataInput
                {
                    Attributes = new[]
                    {
                        new DocumentAttribute
                        {
                            Name = "Data",
                            Value = "Pending",
                            HasPositionInfo = false
                        }
                    }.ToList()
                };

                var testAttribute = testInput.Attributes.Single();

                controller.PutDocumentData(fileId, testInput)
                    .AssertGoodResult<NoContentResult>();

                var data = controller.GetDocumentData(fileId)
                    .AssertGoodResult<DocumentDataResult>();
                Assert.AreEqual("Pending", data.Attributes
                    .Single()
                    .Value,
                    "Incorrect attribute value");

                testAttribute.Value = "Complete";
                controller.PutDocumentData(fileId, testInput)
                    .AssertGoodResult<NoContentResult>();

                data = controller.GetDocumentData(fileId)
                    .AssertGoodResult<DocumentDataResult>();
                Assert.AreEqual("Complete", data.Attributes
                    .Single()
                    .Value,
                    "Incorrect attribute value");

                fileProcessingDb.SetStatusForFile(
                    fileId, _IDS_VERIFY_ACTION, workflowId, EActionStatus.kActionFailed, false, false, out _);
                var status = fileProcessingDb.GetFileStatus(fileId, _IDS_VERIFY_ACTION, false);
                Assert.AreEqual(EActionStatus.kActionFailed, status);

                testAttribute.Value = "Failed";
                controller.PutDocumentData(fileId, testInput)
                    .AssertGoodResult<NoContentResult>();

                data = controller.GetDocumentData(fileId)
                    .AssertGoodResult<DocumentDataResult>();
                Assert.AreEqual("Failed", data.Attributes
                    .Single()
                    .Value,
                    "Incorrect attribute value");

                fileProcessingDb.SetFileStatusToUnattempted(fileId, _IDS_VERIFY_ACTION, false);
                status = fileProcessingDb.GetFileStatus(fileId, _IDS_VERIFY_ACTION, false);
                Assert.AreEqual(EActionStatus.kActionUnattempted, status);

                testAttribute.Value = "Unattempted";
                controller.PutDocumentData(fileId, testInput)
                    .AssertResultCode(StatusCodes.Status423Locked);

                fileProcessingDb.SetFileStatusToPending(fileId, _IDS_VERIFY_ACTION, false);
                status = fileProcessingDb.GetFileStatus(fileId, _IDS_VERIFY_ACTION, false);
                Assert.AreEqual(EActionStatus.kActionPending, status);


                // Next try using PatchDocumentData for a document in various documents states
                var testPatch = new DocumentDataPatch
                {
                    Attributes = new[]
                    {
                        new DocumentAttributePatch(PatchOperation.Update)
                        {
                            ID = testAttribute.ID,
                            Value = "PatchPending"
                        }
                    }.ToList()
                };

                var patchAttribute = testPatch.Attributes.Single();

                controller.PatchDocumentData(fileId, testPatch)
                    .AssertGoodResult<NoContentResult>();

                data = controller.GetDocumentData(fileId)
                    .AssertGoodResult<DocumentDataResult>();
                Assert.AreEqual("PatchPending", data.Attributes
                    .Single()
                    .Value,
                    "Incorrect attribute value");

                patchAttribute.Value = "PatchComplete";
                controller.PatchDocumentData(fileId, testPatch)
                    .AssertGoodResult<NoContentResult>();

                data = controller.GetDocumentData(fileId)
                    .AssertGoodResult<DocumentDataResult>();
                Assert.AreEqual("PatchComplete", data.Attributes
                    .Single()
                    .Value,
                    "Incorrect attribute value");

                fileProcessingDb.SetStatusForFile(
                    fileId, _IDS_VERIFY_ACTION, workflowId, EActionStatus.kActionFailed, false, false, out _);
                status = fileProcessingDb.GetFileStatus(fileId, _IDS_VERIFY_ACTION, false);
                Assert.AreEqual(EActionStatus.kActionFailed, status);

                patchAttribute.Value = "Failed";
                controller.PatchDocumentData(fileId, testPatch)
                    .AssertGoodResult<NoContentResult>();

                data = controller.GetDocumentData(fileId)
                    .AssertGoodResult<DocumentDataResult>();
                Assert.AreEqual("Failed", data.Attributes
                    .Single()
                    .Value,
                    "Incorrect attribute value");

                fileProcessingDb.SetFileStatusToUnattempted(fileId, _IDS_VERIFY_ACTION, false);
                status = fileProcessingDb.GetFileStatus(fileId, _IDS_VERIFY_ACTION, false);
                Assert.AreEqual(EActionStatus.kActionUnattempted, status);

                patchAttribute.Value = "Unattempted";
                controller.PatchDocumentData(fileId, testPatch)
                    .AssertResultCode(StatusCodes.Status423Locked);
            }
            finally
            {
                FileApiMgr.Instance.ReleaseAll();
                _testDbManager.RemoveDatabase(dbName);
            }
        }

        /// <summary>
        /// Test IDShield documents
        /// </summary>
        [Test, Category("Automated")]
        [TestCase(ApiContext.LEGACY_VERSION)]
        [TestCase(ApiContext.CURRENT_VERSION)]
        public static void Test_PatchDocumentData(string apiVersion)
        {
            string dbName = $"Test_DocumentAPI_PatchDocumentData_{apiVersion}";

            try
            {
                (FileProcessingDB fileProcessingDb, User user, DocumentController controller) =
                    InitializeAndLogin(apiVersion, "Resources.Demo_LabDE.bak", dbName, "jon_doe", "123");

                var testFilename = _testFiles.GetFile(_TEST_FILE_C413);
                var ussFilename = _testFiles.GetFile(_TEST_FILE_C413_USS);

                int fileId = 4;
                string fileName = fileProcessingDb.GetFileNameFromFileID(fileId);
                var fileRecord = fileProcessingDb.GetFileRecord(fileName, _LABDE_VERIFY_ACTION);
                fileProcessingDb.RenameFile(fileRecord, testFilename);

                var data = controller.GetDocumentData(fileId)
                    .AssertGoodResult<DocumentDataResult>();

                var attributeToAdd = new DocumentAttributePatch(PatchOperation.Create)
                {
                    Value = "At Root",
                    HasPositionInfo = false
                };

                var attributeToAdd2 = new DocumentAttributePatch(PatchOperation.Create)
                {
                    ParentAttributeID = _HEMOGLOBIN_ATTRIBUTE_GUID,
                    HasPositionInfo = true,
                    Value = "Under Hemoglobin",
                    SpatialPosition = new Position
                    {
                        LineInfo = new List<SpatialLine>(new[]
                            {
                                new SpatialLine
                                {
                                    SpatialLineZone = new SpatialLineZone
                                    {
                                        PageNumber = 2,
                                        StartX = 1000,
                                        StartY = 1500,
                                        EndX = 2000,
                                        EndY = 1500,
                                        Height = 101
                                    }
                                }
                            })
                    }
                };

                var attributeToUpdate = data.Attributes.SelectMany(a => a.ChildAttributes)
                    .Single(b => b.ID == _WBC_ATTRIBUTE_GUID);
                var attributeUpdate = new DocumentAttributePatch(attributeToUpdate, PatchOperation.Update);
                attributeUpdate.Value = "Edited";

                var attributeToDelete = new DocumentAttributePatch(PatchOperation.Delete)
                {
                    ID = _PATIENT_NAME_ATTRIBUTE_GUID
                };

                var patchData = new DocumentDataPatch
                {
                    Attributes = new List<DocumentAttributePatch>(new[]
                    {
                        attributeToAdd,
                        attributeToAdd2,
                        attributeUpdate,
                        attributeToDelete
                    })
                };

                controller.PatchDocumentData(fileId, patchData)
                    .AssertGoodResult<NoContentResult>();

                var data2 = controller.GetDocumentData(fileId)
                    .AssertGoodResult<DocumentDataResult>();

                Assert.AreEqual(data.Attributes.Count + 1, data2.Attributes.Count,
                    "Incorrect number of remaining attribute");

                var addedAttribute = data2.Attributes
                    .SingleOrDefault(b => b.ID == attributeToAdd.ID);
                Assert.IsNotNull(addedAttribute, "Failed to create attribute");
                Assert.AreEqual("At Root", addedAttribute.Value,
                    "Added attribute value incorrect");

                var addedAttribute2 = data2.Attributes
                    .SingleOrDefault(b => b.ID == attributeToAdd2.ID);
                Assert.IsNull(addedAttribute2, "Attribute incorrectly create at root");
                var parentAttribute = data2.Attributes
                    .SelectMany(a => a.ChildAttributes)
                    .SingleOrDefault(b => b.ID == _HEMOGLOBIN_ATTRIBUTE_GUID);
                addedAttribute2 = parentAttribute.ChildAttributes
                    .SingleOrDefault(a => a.ID == attributeToAdd2.ID);
                Assert.IsNotNull(addedAttribute2, "Failed to create attribute");
                var bounds = addedAttribute2.SpatialPosition.LineInfo
                    .Single()
                    .SpatialLineBounds;
                Assert.AreEqual(2, bounds.PageNumber, "Incorrect page number");
                Assert.AreEqual(1000, bounds.Left, "Incorrect left boundary");
                Assert.AreEqual(1450, bounds.Top, "Incorrect top boundary");
                Assert.AreEqual(2000, bounds.Right, "Incorrect right boundary");
                Assert.AreEqual(1550, bounds.Bottom, "Incorrect bottom boundary");

                var editedAttribute = data2.Attributes.SelectMany(a => a.ChildAttributes)
                    .Single(b => b.ID == _WBC_ATTRIBUTE_GUID);
                Assert.AreEqual("Edited", editedAttribute.Value, "Incorrect edited value");
                Assert.AreEqual(attributeToUpdate.ChildAttributes.Count, editedAttribute.ChildAttributes.Count,
                    "Edited attribute children have been modified");

                Assert.IsFalse(data2.Attributes.Any(a => a.ID == attributeToDelete.ID),
                    "Failed to delete attribute");
            }
            finally
            {
                FileApiMgr.Instance.ReleaseAll();
                _testDbManager.RemoveDatabase(dbName);

                _testFiles.RemoveFile(_TEST_FILE_C413);
                _testFiles.RemoveFile(_TEST_FILE_C413_USS);
            }
        }

        [Test, Category("Automated")]
        [TestCase(ApiContext.LEGACY_VERSION)]
        [TestCase(ApiContext.CURRENT_VERSION)]
        public static void TestLabDE_GetDocumentData(string apiVersion)
        {
            string dbName = $"Test_DocumentAPI_LabDE_GetDocumentData_{apiVersion}";

            try
            {
                (FileProcessingDB fileProcessingDb, User user, DocumentController controller) =
                    InitializeAndLogin(apiVersion, "Resources.Demo_LabDE.bak", dbName, "jon_doe", "123");

                foreach (var kvpFileInfo in LabDEFileIdToFileInfo)
                {
                    var fileId = kvpFileInfo.Key;
                    var fileInfo = kvpFileInfo.Value;

                    string xmlFileName = _testFiles.GetFile(fileInfo.XmlFile);
                    var documentDataResult = controller.GetDocumentData(fileId).AssertGoodResult<DocumentDataResult>();

                    bool success = CompareXmlToDocumentAttributeSet(xmlFileName, documentDataResult);

                    Assert.IsTrue(success);
                }
            }
            finally
            {
                FileApiMgr.Instance.ReleaseAll();
                _testDbManager.RemoveDatabase(dbName);
            }
        }

        /// <summary>
        /// test FlexIndex documents
        /// </summary>
        [Test, Category("Automated")]
        [TestCase(ApiContext.LEGACY_VERSION)]
        [TestCase(ApiContext.CURRENT_VERSION)]
        public static void TestFlexIndex_GetDocumentData(string apiVersion)
        {
            string dbName = $"Test_DocumentAPI_FlexIndex_GetDocumentData_{apiVersion}";
            Mock<IConfigurationDatabaseService> mock = new();
            mock.Setup(x => x.DocumentAPIWebConfigurations).Returns(new List<IDocumentApiWebConfiguration>() { _flexIndexDefaultConfiguration });
            mock.Setup(x => x.Configurations).Returns(new List<ICommonWebConfiguration>() { _flexIndexDefaultConfiguration });

            try
            {
                (FileProcessingDB fileProcessingDb, User user, UsersController userController) =
                _testDbManager.InitializeEnvironment(
                    controller: () =>
                    {
                        var configurationDatabaseService = new ConfigurationDatabaseService(new FileProcessingDBClass() { DatabaseName = dbName, DatabaseServer = "(local)" });
                        return new UsersController(configurationDatabaseService, _flexIndexDefaultConfiguration);
                    }
                    , apiVersion: apiVersion
                    , dbResource: "Resources.Demo_FlexIndex.bak"
                    , dbName: dbName
                    , username: "jon_doe"
                    , password: "123"
                    , webConfiguration: _flexIndexDefaultConfiguration);

                var controller = user.SetupController(new DocumentController(mock.Object));

                foreach (var kvpFileInfo in FlexIndexFileIdToFileInfo)
                {
                    var fileId = kvpFileInfo.Key;
                    var fileInfo = kvpFileInfo.Value;

                    string xmlFileName = _testFiles.GetFile(fileInfo.XmlFile);
                    var documentDataResult = controller.GetDocumentData(fileId).AssertGoodResult<DocumentDataResult>();

                    bool success = CompareXmlToDocumentAttributeSet(xmlFileName, documentDataResult);

                    Assert.IsTrue(success);
                }
            }
            finally
            {
                FileApiMgr.Instance.ReleaseAll();
                _testDbManager.RemoveDatabase(dbName);
            }
        }

        [Test, Category("Automated")]
        public static void TestManyLineAttribute()
        {
            var voa = new IUnknownVector();
            var ss = new SpatialString();
            string tempUssFileName = _testFiles.GetFile("Resources.Image1.pdf.uss");
            Assert.That(File.Exists(tempUssFileName));

            ss.LoadFrom(tempUssFileName, bSetDirtyFlagToTrue: false);
            var attr = new AttributeClass { Name = "Manual", Value = ss };
            voa.PushBack(attr);

            var mapper = new AttributeMapper(voa, UCLID_FILEPROCESSINGLib.EWorkflowType.kExtraction);
            var docAttr = mapper.MapAttributesToDocumentAttributeSet(
                includeNonSpatial: true, verboseSpatialData: true, splitMultiPageAttributes: false);
            Assert.IsFalse(docAttr.Attributes.Any(a =>
                a.SpatialPosition.Pages.Count != a.SpatialPosition.Pages.Distinct().Count()));
        }

        [Test, Category("Automated")]
        [TestCase(ApiContext.CURRENT_VERSION)]
        public static void Test_GetMetadataField(string apiVersion)
        {
            var dbName = $"Test_DocumentAPI_GetMetadataField_{apiVersion}";

            try
            {
                (FileProcessingDB fileProcessingDb, User user, DocumentController controller) =
                    InitializeAndLogin(apiVersion, "Resources.Demo_LabDE.bak", dbName, "jon_doe", "123");

                fileProcessingDb.SetMetadataFieldValue(1, "PatientFirstName", "TestName");

                var metadataFieldResult = controller.GetMetadataField(1, "PatientFirstName").AssertGoodResult<MetadataFieldResult>();

                Assert.AreEqual("TestName", metadataFieldResult.Value);
            }
            finally
            {
                FileApiMgr.Instance.ReleaseAll();
                _testDbManager.RemoveDatabase(dbName);
            }
        }

        [Test, Category("Automated")]
        [TestCase(ApiContext.CURRENT_VERSION)]
        public static void Test_SetMetadataField(string apiVersion)
        {
            var dbName = $"Test_DocumentAPI_SetMetadataField_{apiVersion}";

            try
            {
                (FileProcessingDB fileProcessingDb, User user, DocumentController controller) =
                    InitializeAndLogin(apiVersion, "Resources.Demo_LabDE.bak", dbName, "jon_doe", "123");

                controller.SetMetadataField(1, "PatientFirstName", "TestName").AssertGoodResult<NoContentResult>();

                string dbValue = fileProcessingDb.GetMetadataFieldValue(1, "PatientFirstName");

                Assert.AreEqual("TestName", dbValue);
            }
            finally
            {
                FileApiMgr.Instance.ReleaseAll();
                _testDbManager.RemoveDatabase(dbName);
            }
        }

        /// <summary>
        /// Tests that informative errors are returned when the appropriate actions for API calls are not
        /// set and that operations are recorded under the appropriate action in the database given the operation.
        /// https://extract.atlassian.net/browse/ISSUE-16745
        /// </summary>
        [Test, Category("Broken")]
        [TestCase(ApiContext.LEGACY_VERSION)]
        [TestCase(ApiContext.CURRENT_VERSION)]
        public static void Test_ProperWorkflowActionNamesUsed(string apiVersion)
        {
            string dbName = $"Test_DocumentAPI_ProperWorkflowActionNamesUsed_{apiVersion}";
            DocumentApiConfiguration properWorkflowActionNamesUsed = new(
            configurationName: "DocumentAPITesting",
            isDefault: true,
            workflowName: "CourtOffice",
            attributeSet: "Attr",
            processingAction: "",
            postProcessingAction: "Output",
            documentFolder: @"c:\temp\DocumentFolder",
            startAction: "Compute",
            endAction: "Output",
            postWorkflowAction: "",
            outputFileNameMetadataField: "Outputfile",
            outputFileNameMetadataInitialValueFunction: "<SourceDocName>.result.tif");

            Mock<IConfigurationDatabaseService> mock = new();
            mock.Setup(x => x.DocumentAPIWebConfigurations).Returns(new List<IDocumentApiWebConfiguration>() { properWorkflowActionNamesUsed });
            mock.Setup(x => x.Configurations).Returns(new List<ICommonWebConfiguration>() { properWorkflowActionNamesUsed });

            // This test is failing due to behavior changes in the web app.

            // This test will:
            // 1) Set the workflow start action and clear the verify/edit action
            // 2) Confirm file can be posted and it does so under the start action
            // 3) Confirm descriptive error when trying to put data without the verify/edit action set.
            // 4) Clear the workflow start action and set the verify/edit action
            // 5) Confirm descriptive error when trying to post a file without a start action set.
            // 6) Confirm data can be posted and it does so under the verify/edit action.

            try
            {
                (FileProcessingDB fileProcessingDb, User user, UsersController userController) =
                _testDbManager.InitializeEnvironment(
                    controller: () =>
                    {
                        var configurationDatabaseService = new ConfigurationDatabaseService(new FileProcessingDBClass() { DatabaseName = dbName, DatabaseServer = "(local)" });
                        return new UsersController(configurationDatabaseService, properWorkflowActionNamesUsed);
                    }
                    , apiVersion: apiVersion
                    , dbResource: "Resources.Demo_IDShield.bak"
                    , dbName: dbName
                    , username: "jon_doe"
                    , password: "123"
                    , webConfiguration: properWorkflowActionNamesUsed);
                user.ConfigurationName = properWorkflowActionNamesUsed.ConfigurationName;
                var controller = user.SetupController(new DocumentController(mock.Object));

                var filename = _testFiles.GetFile(_TEST_FILE_TESTIMAGE001);

                using var stream = new FileStream(filename, FileMode.Open);
                // 1) Set the workflow start action. The processing action was not set.
                var workflowId = fileProcessingDb.GetWorkflowID(properWorkflowActionNamesUsed.WorkflowName);
                var workflow = fileProcessingDb.GetWorkflowDefinition(workflowId);

                // 2) Confirm file can be posted and it does so under the start action
                var formFile = new FormFile(stream, 0, stream.Length, filename, filename);
                var documentIdResult = controller.PostDocument(formFile)
                    .AssertGoodResult<DocumentIdResult>();

                var recordSet = fileProcessingDb.GetResultsForQuery(
                    $@"SELECT [ASCName]
	                        FROM [QueueEvent]
	                        INNER JOIN [Action] ON [ActionID] = [Action].[ID]
	                        WHERE [FileID] = {documentIdResult.Id}");
                recordSet.MoveFirst();
                Assert.AreEqual("Compute", recordSet.Fields["ASCName"].Value);
                recordSet.MoveNext();
                Assert.IsTrue(recordSet.EOF);

                // 3) Confirm descriptive error when trying to put data without the verify/edit action set.
                var badResult = controller.PutDocumentData(documentIdResult.Id, new DocumentDataInput());
                badResult.AssertResultCode(500);
                Assert.That(((ErrorResult)((ObjectResult)badResult).Value).Error.Message,
                    Contains.Substring("verify/update action"),
                    "Expected error citing verify/update action configuration");

                // 4) Clear the workflow start action and set the verify/edit action

                // One thing to note here, the configuration name is different. The FileAPI manager will try to re-use a cached configuration
                // so the name MUST be changed.
                properWorkflowActionNamesUsed = new(
                configurationName: "DocumentAPITesting2",
                isDefault: true,
                workflowName: "CourtOffice",
                attributeSet: "Attr",
                processingAction: "Verify",
                postProcessingAction: "Output",
                documentFolder: @"c:\temp\DocumentFolder",
                startAction: "",
                endAction: "Output",
                postWorkflowAction: "",
                outputFileNameMetadataField: "Outputfile",
                outputFileNameMetadataInitialValueFunction: "<SourceDocName>.result.tif");

                user.ConfigurationName = properWorkflowActionNamesUsed.ConfigurationName;
                mock = new();
                mock.Setup(x => x.DocumentAPIWebConfigurations).Returns(new List<IDocumentApiWebConfiguration>() { properWorkflowActionNamesUsed });
                mock.Setup(x => x.Configurations).Returns(new List<ICommonWebConfiguration>() { properWorkflowActionNamesUsed });
                var controller2 = user.SetupController(new DocumentController(mock.Object), properWorkflowActionNamesUsed);

                // 5) Confirm descriptive error when trying to post a file without a start action set.
                badResult = controller2.PostDocument(formFile);
                badResult.AssertResultCode(500);
                Assert.That(((ErrorResult)((ObjectResult)badResult).Value).Error.Message,
                    Contains.Substring("start action"),
                    "Expected error citing start action configuration");

                // Files must be pending in verify/edit action to allow for data to be posted.
                fileProcessingDb.SetFileStatusToPending(documentIdResult.Id, "Verify", false);

                // 6) Confirm data can be posted and it does so under the verify/edit action.
                controller2.PutDocumentData(documentIdResult.Id, new DocumentDataInput() { Attributes = new List<DocumentAttribute>() })
                    .AssertGoodResult<NoContentResult>();

                recordSet = fileProcessingDb.GetResultsForQuery(
                    $@"SELECT [ASCName]
	                        FROM [FileTaskSession]
	                        INNER JOIN [Action] ON [ActionID] = [Action].[ID]
	                        WHERE [FileID] = {documentIdResult.Id}");
                recordSet.MoveFirst();
                Assert.AreEqual("Verify", recordSet.Fields["ASCName"].Value);
                recordSet.MoveNext();
                Assert.IsTrue(recordSet.EOF);
            }
            finally
            {
                FileApiMgr.Instance.ReleaseAll();
                _testDbManager.RemoveDatabase(dbName);
            }
        }

        [Test, Category("Automated"), Category("TryGetPage"), Parallelizable(ParallelScope.Self)]
        public static void Test_GetImagePageFromTIF()
        {
            using var testFiles = new TestFileManager<TestDocument>();

            var tifPath = testFiles.GetFile("Resources.TestImage003.tif");

            Assert.False(DocumentData.TryGetPageFromAssociatedPdf(tifPath, 1, out var _));

            Assert.True(DocumentData.TryGetPageWithConversionToPdf(tifPath, 1, out byte[] imageDataFromConversion));
            Assert.True(imageDataFromConversion.Length > 0);
        }

        [Test, Category("Automated"), Category("TryGetPage"), Parallelizable(ParallelScope.Self)]
        public static void Test_GetImagePageFromPDF()
        {
            using var testFiles = new TestFileManager<TestDocument>();

            var pdfPath = testFiles.GetFile("Resources.TestImage003.pdf");
            Assert.True(DocumentData.TryGetPageFromAssociatedPdf(pdfPath, 1, out byte[] imageDataFromPDF));
            Assert.True(imageDataFromPDF.Length > 0);
            RemoveCachedPDF(pdfPath);
        }

        [Test, Category("Automated"), Category("TryGetPage"), Parallelizable(ParallelScope.Self)]
        public static void Test_GetImagePageFromAssociatedPDF()
        {
            using var testFiles = new TestFileManager<TestDocument>();

            var tifPath = testFiles.GetFile("Resources.TestImage003.tif");
            Assert.False(DocumentData.TryGetPageFromAssociatedPdf(tifPath, 1, out var _));

            var pdfPath = testFiles.GetFile("Resources.TestImage003.pdf");
            Assert.True(DocumentData.TryGetPageFromAssociatedPdf(tifPath, 1, out byte[] imageDataFromPDF));
            RemoveCachedPDF(pdfPath);

            var pdfPathOld = pdfPath;
            pdfPath = testFiles.GetFile("Resources.TestImage003.pdf", tifPath + ".PDF");
            Assert.False(File.Exists(pdfPathOld));
            Assert.True(DocumentData.TryGetPageFromAssociatedPdf(tifPath, 1, out imageDataFromPDF));
            RemoveCachedPDF(pdfPath);

            tifPath = testFiles.GetFile("Resources.TestImage003.tif", Path.ChangeExtension(tifPath, ".pdf.tif"));
            pdfPathOld = pdfPath;
            pdfPath = testFiles.GetFile("Resources.TestImage003.pdf", Path.ChangeExtension(tifPath, null));
            Assert.False(File.Exists(pdfPathOld));
            Assert.True(DocumentData.TryGetPageFromAssociatedPdf(tifPath, 1, out imageDataFromPDF));
            RemoveCachedPDF(pdfPath);
        }

        // I used nuance to convert this TIF to PDF so the data is the same for both methods
        // The only differences are in the prefix and suffix of the documents
        [TestCase(1, Description = "Page 1"), Category("Automated"), Category("TryGetPage")]
        [TestCase(2, Description = "Page 2"), Category("Automated"), Category("TryGetPage")]
        [TestCase(3, Description = "Page 3"), Category("Automated"), Category("TryGetPage")]
        [TestCase(4, Description = "Page 4"), Category("Automated"), Category("TryGetPage")]
        [Parallelizable(ParallelScope.All)]
        public static void Test_ComparePDFPages(int pageNumber)
        {
            using var testFiles = new TestFileManager<TestDocument>();

            // Hack to pull out the image from the PDF by looking for null bytes to indicate beginning
            // and then searching for 'endstream' text
            byte[] getMainStream(byte[] pdfData)
            {
                int find(int start, byte[] pattern)
                {
                    int matchLength = 0;
                    int matchStart = 0;
                    for (int i = start; i < pdfData.Length && matchLength < pattern.Length; i++)
                    {
                        if (pdfData[i] == pattern[matchLength])
                        {
                            if (matchLength == 0)
                            {
                                matchStart = i;
                            }
                            matchLength++;
                        }
                        else
                        {
                            matchLength = 0;
                        }
                    }

                    return matchLength == pattern.Length
                        ? matchStart
                        : -1;
                }

                int startStream = find(0, new byte[] { 0, 0, 0, 0 });
                int endStream = find(startStream, ASCIIEncoding.GetEncoding("windows-1252").GetBytes("endstream"));

                // Remove newline chars b/c the two libraries use differerent line endings
                while (pdfData[--endStream] == '\r' || pdfData[endStream] == '\n') ;

                return pdfData.Skip(startStream).Take(endStream - startStream).ToArray();
            }

            string pdfPath = testFiles.GetFile("Resources.TestImage003.pdf");
            Assert.True(DocumentData.TryGetPageFromAssociatedPdf(pdfPath, pageNumber, out byte[] imageDataFromPDF));
            RemoveCachedPDF(pdfPath);

            string tifPath = testFiles.GetFile("Resources.TestImage003.tif");
            Assert.True(DocumentData.TryGetPageWithConversionToPdf(tifPath, pageNumber, out byte[] imageDataFromConversion));

            // Sanity check--these should be a little different
            CollectionAssert.AreNotEqual(imageDataFromPDF, imageDataFromConversion);

            byte[] middleNoConversion = getMainStream(imageDataFromPDF);
            byte[] middleConverted = getMainStream(imageDataFromConversion);

            // Sanity check--this is most of the data, right?
            Assert.Greater(middleConverted.Length, 0.9 * imageDataFromConversion.Length);

            CollectionAssert.AreEqual(middleConverted, middleNoConversion);
        }

        // Ensure the method fails gracefully (returns false) for a bad PDF file
        [Test, Category("Automated"), Category("TryGetPage"), Parallelizable(ParallelScope.Self)]
        public static void Test_GetImagePageFromBadPDF()
        {
            using var testFiles = new TestFileManager<TestDocument>();

            var resource = "Resources.TestImage003.tif";

            var tifPath = testFiles.GetFile(resource);

            // Move to a path ending in .pdf
            var pdfPath = testFiles.GetFile(resource, tifPath + ".not.a.pdf");

            Assert.False(DocumentData.TryGetPageFromAssociatedPdf(pdfPath, 1, out byte[] imageDataFromPDF));
        }

        #endregion Public Test Functions

        static (FileProcessingDB fileProcessingDb, User user, DocumentController DocumentController)
        InitializeAndLogin(string apiVersion, string dbResource, string dbName, string username, string password)
        {
            Mock<IConfigurationDatabaseService> mock = new();

            IDocumentApiWebConfiguration webConfig = null;
            if (dbResource.Equals("Resources.Demo_LabDE.bak"))
            {
                webConfig = _labDEDefaultConfiguration;
            }
            else
            {
                webConfig = _idShieldDefaultConfiguration;
            }
            mock.Setup(x => x.DocumentAPIWebConfigurations).Returns(new List<IDocumentApiWebConfiguration>() { webConfig });
            mock.Setup(x => x.Configurations).Returns(new List<ICommonWebConfiguration>() { webConfig });

            (FileProcessingDB fileProcessingDb, User user, UsersController userController) =
                _testDbManager.InitializeEnvironment(
                    controller: () => new UsersController(mock.Object, webConfig)
                    , apiVersion: apiVersion
                    , dbResource: "Resources.Demo_LabDE.bak"
                    , dbName: dbName
                    , username: "jon_doe"
                    , password: "123"
                    , dbResource.Equals("Resources.Demo_LabDE.bak") ? _labDEDefaultConfiguration : _idShieldDefaultConfiguration);

            var result = userController.Login(user);
            var token = result.AssertGoodResult<JwtSecurityToken>();

            return (fileProcessingDb, user, user.SetupController(new DocumentController(mock.Object)));
        }

        static void SetupMetadataFieldValue(int fileId, string value, int fieldId, string dbName)
        {
            string command = Utils.Inv(
                $"UPDATE [dbo].[FileMetadataFieldValue] SET Value='{value}' WHERE [FileID]={fileId} AND [MetadataFieldID]={fieldId};");
            ModifyTable(dbName, command);
        }

        public static void ModifyTable(string dbName, string command)
        {
            // NOTE: "Pooling=false;" keeps the connection from being pooled, and 
            // allows the connection to REALLY close, so the DB can be removed later.
            string connectionString = $"Server=(local);Database={dbName};Trusted_Connection=True;Pooling=false;";
            using (var connection = new ExtractRoleConnection(connectionString))
            {
                connection.Open();
                using var cmd = connection.CreateCommand();
                cmd.CommandText = command;
                cmd.ExecuteNonQuery();
            }
        }

        static bool CompareXmlToDocumentAttributeSet(string xmlFile, DocumentDataResult documentAttributes)
        {
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(xmlFile);
            var root = xmlDoc.FirstChild;

            int nonMetadataAttributes = 0;
            for (int i = 0; i < root.ChildNodes.Count; ++i)
            {
                var childNode = root.ChildNodes[i];
                if (!childNode.Name.StartsWith("_"))
                {
                    var attr = documentAttributes.Attributes[i];
                    CompareParentNodeToAttribute(childNode, attr);
                    nonMetadataAttributes++;
                }
            }

            Assert.IsTrue(nonMetadataAttributes == documentAttributes.Attributes.Count,
                "xml attributes count: {0}, != documentAttributes count: {1}",
                nonMetadataAttributes,
                documentAttributes.Attributes.Count);

            return true;
        }

        static void CompareParentNodeToAttribute(XmlNode parentNode, DocumentAttribute docAttr, bool topLevel = true)
        {
            if (topLevel)
            {
                Debug.WriteLine($"Node: {parentNode.Name}");
            }
            else
            {
                Debug.WriteLine($"\tNode: {parentNode.Name}");
            }

            var fullTextChildNode = TestParentNode(parentNode, docAttr);
            if (fullTextChildNode == null)
            {
                return;     // only get here on an exception, don't throw any more...
            }

            var lastNode = fullTextChildNode;

            if ((bool)docAttr.HasPositionInfo)
            {
                XmlNode nextNode = null;

                for (int i = 0; i < docAttr.SpatialPosition.LineInfo.Count; ++i)
                {
                    // process SpatialLine, and then it's nested child element LineText,
                    // and then the SpatialLineZone and SpatialLinebounds elements.
                    // if there node siblings, then continue the iteration with 
                    // the next pair(s) of SpatialLineZone and SpatialLineBounds 
                    // elements, otherwise restart the sequence testing.
                    if (nextNode == null || nextNode.NextSibling == null)
                    {

                        var spatialLineNode = lastNode.NextSibling;
                        AssertNodeName(spatialLineNode, "SpatialLine");

                        // last node at the nesting level of the spatialLine node - not the sub elements
                        lastNode = spatialLineNode;

                        TestSpatialLinePageNumber(spatialLineNode, docAttr.SpatialPosition.LineInfo[i].SpatialLineZone.PageNumber, i);

                        if (spatialLineNode.ChildNodes.Count == 0)
                        {
                            continue;
                        }

                        nextNode = spatialLineNode.ChildNodes[0];
                        AssertNodeName(nextNode, "LineText");

                        TestLineText(nextNode, docAttr.SpatialPosition.LineInfo[i].SpatialLineZone.Text, i);
                    }

                    nextNode = nextNode.NextSibling;
                    AssertNodeName(nextNode, "SpatialLineZone");

                    TestSpatialLineZone(nextNode, docAttr.SpatialPosition.LineInfo[i].SpatialLineZone, i);

                    nextNode = nextNode.NextSibling;
                    AssertNodeName(nextNode, "SpatialLineBounds");

                    TestSpatialLineBounds(nextNode, docAttr.SpatialPosition.LineInfo[i].SpatialLineBounds, i);
                }
            }

            // Now process all the childAttribute items.
            for (int i = 0; i < docAttr.ChildAttributes.Count; ++i)
            {
                var childDocAttr = docAttr.ChildAttributes[i];

                Assert.IsTrue(lastNode != null, "last node is empty, child attribute index: {0}", i);
                lastNode = lastNode.NextSibling;

                CompareParentNodeToAttribute(lastNode, childDocAttr, topLevel: false);
            }
        }

        static void TestSpatialLineBounds(XmlNode spatialLineBounds, SpatialLineBounds bounds, int i)
        {
            //< SpatialLineBounds Top = "92" Left = "1858" Bottom = "3136" Right = "2283" />
            var attributes = spatialLineBounds.Attributes;
            Assert.IsTrue(attributes != null, "null spatialLinebounds attrubtes, index: {0}", i);

            var top = attributes[Top];
            AssertSame(top.Name, top.Value, $"LineInfo[{i}].SpatialLineBounds.Top", bounds.Top.ToString());

            var left = attributes[Left];
            AssertSame(left.Name, left.Value, $"LineInfo[{i}].SpatialLineBounds.Left", bounds.Left.ToString());

            var bottom = attributes[Bottom];
            AssertSame(bottom.Name, bottom.Value, $"LineInfo[{i}].SpatialLineBounds.Bottom", bounds.Bottom.ToString());

            var right = attributes[Right];
            AssertSame(right.Name, right.Value, $"LineInfo[{i}].SpatialLineBounds.Right", bounds.Right.ToString());
        }

        static XmlNode TestParentNode(XmlNode parentNode, DocumentAttribute docAttr)
        {
            // Test Name first - this is unusual because the xml attribute name is the tag name of the root attribute,
            // and the value of the ES_attribute is in the 0th child element Fulltext as the value.
            // Get AverageCharConfidence
            //<LabInfo>
            //    <FullText AverageCharConfidence = "86">N/A</FullText>
            var childNode = parentNode.ChildNodes[FullTextIndex];
            AssertNodeName(childNode, "FullText");

            string nodeName = GetParentNodeName(parentNode);
            AssertSame(nodeName, childNode.InnerText, docAttr.Name, docAttr.Value);

            // e.g. LabDE.PhysicianInfo has a child element FullText that doesn't have a AverageCharacterConfidence
            // attribute.
            if (childNode.Attributes.Count > 0)
            {
                string xmlCharConfidence = childNode.Attributes[AvgCharConf].Value;
                AssertSame(childNode.Name, xmlCharConfidence, "averageCharacterConfidence", docAttr.AverageCharacterConfidence.ToString());
            }

            return childNode;
        }

        static string GetParentNodeName(XmlNode parentNode)
        {
            var name = parentNode.Name;
            if (name.IsEquivalent("HCData") ||
                name.IsEquivalent("MCData") ||
                name.IsEquivalent("LCData") ||
                name.IsEquivalent("Manual"))
            {
                return "Data";
            }

            return name;
        }

        /// <summary>
        /// verify that the spatial page line number matches, if it exists
        /// </summary>
        /// <param name="spatialLineNode"></param>
        /// <param name="pageNumber"></param>
        /// <param name="index"></param>
        static void TestSpatialLinePageNumber(XmlNode spatialLineNode, int? pageNumber, int index)
        {
            if (pageNumber == null)
            {
                if (spatialLineNode.Attributes != null)
                {
                    Assert.IsTrue(spatialLineNode.Attributes["PageNumber"] == null,
                                    "pageNumber is null but xml spatialLineNode has a PageNumber attribute");
                }

                return;
            }

            var xmlPageNumber = spatialLineNode.Attributes[0];
            AssertSame(spatialLineNode.Name,
                        xmlPageNumber.Value,
                        $"docAttr.SpatialPosition.Pages[{index}]",
                        pageNumber.ToString());
        }

        static void TestLineText(XmlNode lineTextNode, string text, int index)
        {
            AssertSame(lineTextNode.Name,
                        lineTextNode.InnerText,
                        $"docAttr.SpatialPosition.LineInfo[{index}].SpatialLineZone.Text",
                        text);
        }

        //<SpatialLineZone StartX = "1878" StartY="1612" EndX="2264" EndY="1617" Height="3041"/>
        static void TestSpatialLineZone(XmlNode spatialLineZoneNode, SpatialLineZone zone, int i)
        {
            //< SpatialLineZone StartX = "1878" StartY = "1612" EndX = "2264" EndY = "1617" Height = "3041" />
            var attributes = spatialLineZoneNode.Attributes;
            Assert.IsTrue(attributes != null, "null spatialLineZoneNode attributes, index: {0}", i);

            var startX = attributes[StartX];
            AssertSame(startX.Name, startX.Value, $"LineInfo[{i}].SpatialLineZone.StartX", zone.StartX.ToString());

            var startY = attributes[StartY];
            AssertSame(startY.Name, startY.Value, $"LineInfo[{i}].SpatialLineZone.StartY", zone.StartY.ToString());

            var endX = attributes[EndX];
            AssertSame(endX.Name, endX.Value, $"LineInfo[{i}].SpatialLineZone.EndX", zone.EndX.ToString());

            var endY = attributes[EndY];
            AssertSame(endY.Name, endY.Value, $"LineInfo[{i}].SpatialLineZone.EndY", zone.EndY.ToString());
        }

        static void AssertSame(string xmlName, string xmlValue, string attrName, string attrValue)
        {
            Assert.IsTrue(attrValue.IsEquivalent(xmlValue),
                            "{0} value: {1}, is not equivalent to: {2} value: {3}",
                            xmlName,
                            xmlValue,
                            attrName,
                            attrValue);
        }

        static void AssertNodeName(XmlNode node, string name)
        {
            string nodeName = node.Name;
            Assert.IsTrue(nodeName.IsEquivalent(name),
                            "Node name test failed: expected: {0}, node is actually named: {1}",
                            name,
                            nodeName);
        }

        // Remove the PDDocument from the cache and close it so that the file can be deleted
        static void RemoveCachedPDF(string pdfPath)
        {
            Utils.RemoveCachedObject<PDDocument>(new[] { pdfPath })?.close();
        }
    }
}