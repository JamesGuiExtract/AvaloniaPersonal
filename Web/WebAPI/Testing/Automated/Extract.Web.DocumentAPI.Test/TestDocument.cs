using Extract.FileActionManager.Database.Test;
using Extract.Testing.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
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
        #region Constants

        const int MaxDemo_LabDE_FileId = 18;

        const int OutputFileMetadataFieldID = 6;

        static readonly string _VERIFY_ACTION = "A02_Verify";
        static readonly string _TEST_FILE1 = "Resources.C413.tif";
        static readonly string _TEST_FILE1_USS = "Resources.C413.tif.uss";
        //static readonly string _TEST_FILE1_VOA = "Resources.TestImage003.tif.voa";

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
        static Dictionary<int, FileInfo> LabDEFileIdToFileInfo = new Dictionary<int, FileInfo>
        {
            {1, new FileInfo {XmlFile = "Resources.A418.tif.restored.xml", AttributeSetName = "DataFoundByRules" } },
            {12, new FileInfo {XmlFile = "Resources.K151.tif.restored.xml", AttributeSetName = "DataFoundByRules" } }
        };

        static Dictionary<int, FileInfo> IDShieldFileIdToFileInfo = new Dictionary<int, FileInfo>
        {
            {2, new FileInfo {XmlFile = "Resources.TestImage002.tif.restored.xml", AttributeSetName = "Attr" } },
            {3, new FileInfo {XmlFile = "Resources.TestImage003.tif.restored.xml", AttributeSetName = "Attr" } }
        };

        static Dictionary<int, FileInfo> FlexIndexFileIdToFileInfo = new Dictionary<int, FileInfo>
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

        [TestFixtureSetUp]
        public static void Setup()
        {
            GeneralMethods.TestSetup();
            _testDbManager = new FAMTestDBManager<TestDocument>();

            _testFiles = new TestFileManager<TestDocument>();
        }

        [TestFixtureTearDown]
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
        public static void Test_PostDocument()
        {
            string dbName = "DocumentAPI_Test_PostDocument";

            try
            {
                (FileProcessingDB fileProcessingDb, User user, DocumentController controller) =
                    InitializeAndLogin("Resources.Demo_LabDE.bak", dbName, "admin", "a");

                var filename = _testFiles.GetFile("Resources.A418.tif");
                using (var stream = new FileStream(filename, FileMode.Open))
                {
                    var formFile = new FormFile(stream, 0, stream.Length, filename, filename);

                    var result = controller.PostDocument(formFile).Result;
                    var submitResult1 = result.AssertGoodResult<DocumentIdResult>();

                    // It is OK to re-submit - the web service writes a unique filename, based on the 
                    // submitted filename, so test this as well.
                    result = controller.PostDocument(formFile).Result;
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

        /// <summary>
        /// Tests both DocumentData.SubmitText and DocumentData.GetSourceFileName
        /// </summary>
        [Test, Category("Automated")]
        public static void Test_PostText()
        {
            string dbName = "DocumentAPI_Test_PostText";

            try
            {
                (FileProcessingDB fileProcessingDb, User user, DocumentController controller) =
                    InitializeAndLogin("Resources.Demo_LabDE.bak", dbName, "admin", "a");

                var result = controller.PostText("Document 1, SSN: 111-22-3333, DOB: 10-04-1999").Result;
                var submitResult = result.AssertGoodResult<DocumentIdResult>();

                var sourceFilename = fileProcessingDb.GetFileNameFromFileID(submitResult.Id);
                Assert.IsNotNullOrEmpty(sourceFilename);
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
        public static void Test_GetStatus()
        {
            string dbName = "DocumentAPI_Test_GetStatus";

            try
            {
                (FileProcessingDB fileProcessingDb, User user, DocumentController controller) =
                    InitializeAndLogin("Resources.Demo_LabDE.bak", dbName, "admin", "a");

                for (int i = 1; i <= 10; ++i)
                {
                    var statusResult = controller.GetStatus(i).AssertGoodResult<ProcessingStatusResult>();
                    Assert.IsTrue(statusResult.DocumentStatus == DocumentProcessingStatus.Processing, 
                        "Unexpected processing state");
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

        /// <summary>
        /// Test GetFileResulot - note that this tests the internal GetResult() function
        /// </summary>
        [Test, Category("Automated")]
        public static void Test_GetOutputFile()
        {
            string dbName = "DocumentAPI_Test_GetOutputFile";

            try
            {
                (FileProcessingDB fileProcessingDb, User user, DocumentController controller) =
                    InitializeAndLogin("Resources.Demo_LabDE.bak", dbName, "admin", "a");

                for (int i = 1; i <= MaxDemo_LabDE_FileId; ++i)
                {
                    var result = controller.GetOutputFile(i);
                    var fileResult = result.AssertGoodResult<PhysicalFileResult>();

                    var workflowId = fileProcessingDb.GetWorkflowID(ApiTestUtils.CurrentApiContext.WorkflowName);
                    var workflow = fileProcessingDb.GetWorkflowDefinition(workflowId);
                    var metadataFieldName = workflow.OutputFileMetadataField;
                    string expectedFileName = fileProcessingDb.GetMetadataFieldValue(i, workflow.OutputFileMetadataField);

                    Assert.AreEqual(expectedFileName, fileResult.FileName, "Unexpected filename");
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
        public static void Test_PostGetDeleteFile()
        {
            string dbName = "DocumentAPI_Test_PostGetDeleteFile";

            try
            {
                (FileProcessingDB fileProcessingDb, User user, DocumentController controller) =
                    InitializeAndLogin("Resources.Demo_LabDE.bak", dbName, "admin", "a");

                var filename = _testFiles.GetFile("Resources.A418.tif");
                using (var stream = new FileStream(filename, FileMode.Open))
                {
                    var formFile = new FormFile(stream, 0, stream.Length, filename, filename);

                    var submitResult = controller.PostDocument(formFile).Result
                        .AssertGoodResult<DocumentIdResult>();

                    var documentResult = controller.GetDocument(submitResult.Id)
                        .AssertGoodResult<PhysicalFileResult>();

                    controller.DeleteDocument(submitResult.Id)
                        .AssertGoodResult<NoContentResult>();

                    // Ensure guid is removed from suggested download filename.
                    Assert.AreEqual(Path.GetFileName(filename), documentResult.FileDownloadName);

                    // Ensure error is returned if we attempt to get document after deletion.
                    controller.GetDocument(submitResult.Id)
                        .AssertResultCode(StatusCodes.Status404NotFound);
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
        public static void Test_GetPageInfoAndText()
        {
            string dbName = "DocumentAPI_Test_GetPageInfoAndText";

            try
            {
                (FileProcessingDB fileProcessingDb, User user, DocumentController controller) =
                    InitializeAndLogin("Resources.Demo_LabDE.bak", dbName, "admin", "a");

                var testFilename = _testFiles.GetFile(_TEST_FILE1);
                var ussFilename = _testFiles.GetFile(_TEST_FILE1_USS);

                int fileId = 4; // A 5 page document.
                string fileName = fileProcessingDb.GetFileNameFromFileID(fileId);
                var fileRecord = fileProcessingDb.GetFileRecord(fileName, _VERIFY_ACTION);
                fileProcessingDb.RenameFile(fileRecord, testFilename);

                var ussData = new SpatialString();
                ussData.LoadFrom(ussFilename, false);

                var expectedPages = ussData.GetPages(true, "");
                int expectedPageCount = expectedPages.Size();

                var pagesInfoResult = controller.GetPageInfo(fileId)
                    .AssertGoodResult<PagesInfoResult>();

                Assert.AreEqual(expectedPageCount, pagesInfoResult.PageCount);

                var pageTextResult = controller.GetText(fileId)
                    .AssertGoodResult<PageTextResult>();

                Assert.AreEqual(expectedPageCount, pageTextResult.Pages.Count);
                for (int page = 1; page <= expectedPageCount; page++)
                {
                    var expectedPage = (SpatialString)expectedPages.At(page - 1);
                    var expectedPageInfo = expectedPage.GetPageInfo(page);
                    var pageInfo = pagesInfoResult.PageInfos[page - 1];
                    Assert.AreEqual(page, pageInfo.Page);
                    Assert.AreEqual(expectedPageInfo.Height, pageInfo.Height);
                    Assert.AreEqual(expectedPageInfo.Width, pageInfo.Width);
                    // Last page is upside down
                    Assert.AreEqual(page == 5 ? 180 : 0, pageInfo.DisplayOrientation);

                    var pageText = pageTextResult.Pages[page - 1];
                    Assert.AreEqual(page, pageText.Page);
                    Assert.AreEqual(expectedPage.String, pageText.Text);

                    var singlePageResult = controller.GetPageText(fileId, page)
                        .AssertGoodResult<PageTextResult>();

                    Assert.AreEqual(page, singlePageResult.Pages.Single().Page);
                    Assert.AreEqual(expectedPage.String, singlePageResult.Pages.Single().Text);
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
        public static void Test_GetDocumentWordZones()
        {
            string dbName = "DocumentAPI_Test_GetDocumentWordZones";

            try
            {
                (FileProcessingDB fileProcessingDb, User user, DocumentController controller) =
                    InitializeAndLogin("Resources.Demo_LabDE.bak", dbName, "admin", "a");

                string testFileName = _testFiles.GetFile(_TEST_FILE1);
                string ussFileName = _testFiles.GetFile(_TEST_FILE1_USS);

                int fileId = 4; // A 5 page document.
                string fileName = fileProcessingDb.GetFileNameFromFileID(fileId);
                var fileRecord = fileProcessingDb.GetFileRecord(fileName, _VERIFY_ACTION);
                fileProcessingDb.RenameFile(fileRecord, testFileName);

                var ussData = new SpatialString();
                ussData.LoadFrom(ussFileName, false);
                var pageCount = ussData.GetLastPageNumber();

                for (int page = 1; page <= pageCount; page++)
                {
                    var pageText = ussData.GetSpecifiedPages(page, page);
                    IUnknownVector pageWords = pageText.GetWords();
                    int wordCount = pageWords.Size();

                    var wordZoneData = controller.GetPageWordZones(fileId, page)
                        .AssertGoodResult<WordZoneDataResult>();
                    Assert.AreEqual(wordCount, wordZoneData.Zones.Count(), "Unexpected number of words");

                    for (int i = 0; i < wordCount ; i++)
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
            }
            finally
            {
                FileApiMgr.ReleaseAll();
                _testDbManager.RemoveDatabase(dbName);

                _testFiles.RemoveFile(_TEST_FILE1);
                _testFiles.RemoveFile(_TEST_FILE1_USS);
            }
        }

        [Test, Category("Automated")]
        public static void Test_GetTextResult()
        {
            string dbName = "DocumentAPI_Test_GetTextResult";

            try
            {   
                (FileProcessingDB fileProcessingDb, User user, DocumentController controller) =
                    InitializeAndLogin("Resources.Demo_LabDE.bak", dbName, "admin", "a");

                var filename = _testFiles.GetFile("Resources.ResultText.txt");
                SetupMetadataFieldValue(1, filename, OutputFileMetadataFieldID, dbName);

                var result = controller.GetOutputText(1);
                var textResult = result.AssertGoodResult<PageTextResult>();

                Assert.AreEqual(File.ReadAllText(filename), textResult.Pages.First().Text);
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
        public static void Test_GetDocumentType()
        {
            string dbName = "DocumentAPI_Test_GetDocumentType";

            try
            {
                (FileProcessingDB fileProcessingDb, User user, DocumentController controller) =
                    InitializeAndLogin("Resources.Demo_LabDE.bak", dbName, "admin", "a");

                for (int i = 1; i <= MaxDemo_LabDE_FileId; ++i)
                {
                    var textResult = controller.GetDocumentType(i)
                        .AssertGoodResult<global::WebAPI.Models.TextData>();
                    Assert.IsTrue(!string.IsNullOrEmpty(textResult.Text));

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

        /// <summary>
        /// test IDShield documents
        /// </summary>
        [Test, Category("Automated")]
        public static void TestIDShield_GetDocumentData()
        {
            string dbName = "DocumentAPI_TestIDShield_GetDocumentData";

            try
            {
                (FileProcessingDB fileProcessingDb, User user, DocumentController controller) =
                    InitializeAndLogin("Resources.Demo_IDShield.bak", dbName, "admin", "a");

                foreach (var kvpFileInfo in IDShieldFileIdToFileInfo)
                {
                    var fileId = kvpFileInfo.Key;
                    var fileInfo = kvpFileInfo.Value;

                    string xmlFileName = _testFiles.GetFile(fileInfo.XmlFile);
                    var documentDataResult = controller.GetDocumentData(fileId).AssertGoodResult<DocumentDataResult>();

                    bool success = CompareXmlToDocumentAttributeSet(xmlFileName, documentDataResult);
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

        /// <summary>
        /// Test IDShield documents
        /// </summary>
        [Test, Category("Automated")]
        public static void Test_PutDocumentData()
        {
            string dbName = "DocumentAPI_Test_PutDocumentData";

            try
            {
                (FileProcessingDB fileProcessingDb, User user, DocumentController controller) =
                    InitializeAndLogin("Resources.Demo_IDShield.bak", dbName, "admin", "a");

                var data = controller.GetDocumentData(1)
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

                controller.PutDocumentData(1, newData)
                    .AssertGoodResult<NoContentResult>();

                data = controller.GetDocumentData(1)
                    .AssertGoodResult<DocumentDataResult>();

                Assert.AreEqual(2, data.Attributes.Count);

                Assert.AreEqual(0, data.Attributes.Count(a => a.ID == deletedID));

                var updatedAttribute = data.Attributes.Single(a => a.ID == updatedID);
                Assert.AreEqual("Updated", updatedAttribute.Value);
                // Ensure zone of updated attribute was set correctly based on the supplied SpatialLineZone
                var zone = updatedAttribute.SpatialPosition.LineInfo
                    .Single()
                    .SpatialLineZone;
                Assert.AreEqual(1, zone.PageNumber);
                Assert.AreEqual(0, zone.StartX);
                Assert.AreEqual(25, zone.StartY);
                Assert.AreEqual(100, zone.EndX);
                Assert.AreEqual(25, zone.EndY);
                Assert.AreEqual(51, zone.Height);

                var addedAttribute = data.Attributes.Single(a => a.ID == addedAttributeID);
                // Ensure bounds were set correctly based on the supplied SpatialLineZone
                var bounds = addedAttribute.SpatialPosition.LineInfo
                    .Single()
                    .SpatialLineBounds;
                Assert.AreEqual(1, bounds.PageNumber);
                Assert.AreEqual(1000, bounds.Left);
                Assert.AreEqual(1450, bounds.Top);
                Assert.AreEqual(2000, bounds.Right);
                Assert.AreEqual(1550, bounds.Bottom);
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

        /// <summary>
        /// Test IDShield documents
        /// </summary>
        [Test, Category("Automated")]
        public static void Test_PatchDocumentData()
        {
            string dbName = "DocumentAPI_Test_PatchDocumentData";

            try
            {
                (FileProcessingDB fileProcessingDb, User user, DocumentController controller) =
                    InitializeAndLogin("Resources.Demo_LabDE.bak", dbName, "admin", "a");

                int fileId = 4;
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

                Assert.AreEqual(data.Attributes.Count + 1, data2.Attributes.Count);

                var addedAttribute = data2.Attributes
                    .SingleOrDefault(b => b.ID == attributeToAdd.ID);
                Assert.IsNotNull(addedAttribute);
                Assert.AreEqual("At Root", addedAttribute.Value);

                var addedAttribute2 = data2.Attributes
                    .SingleOrDefault(b => b.ID == attributeToAdd2.ID);
                Assert.IsNull(addedAttribute2);
                var parentAttribute = data2.Attributes
                    .SelectMany(a => a.ChildAttributes)
                    .SingleOrDefault(b => b.ID == _HEMOGLOBIN_ATTRIBUTE_GUID);
                addedAttribute2 = parentAttribute.ChildAttributes
                    .SingleOrDefault(a => a.ID == attributeToAdd2.ID);
                Assert.IsNotNull(addedAttribute2);
                var bounds = addedAttribute2.SpatialPosition.LineInfo
                    .Single()
                    .SpatialLineBounds;
                Assert.AreEqual(2, bounds.PageNumber);
                Assert.AreEqual(1000, bounds.Left);
                Assert.AreEqual(1450, bounds.Top);
                Assert.AreEqual(2000, bounds.Right);
                Assert.AreEqual(1550, bounds.Bottom);

                var editedAttribute = data2.Attributes.SelectMany(a => a.ChildAttributes)
                    .Single(b => b.ID == _WBC_ATTRIBUTE_GUID);
                Assert.AreEqual("Edited", editedAttribute.Value);
                Assert.AreEqual(attributeToUpdate.ChildAttributes.Count, editedAttribute.ChildAttributes.Count);

                Assert.IsFalse(data2.Attributes.Any(a => a.ID == attributeToDelete.ID));
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
        public static void TestLabDE_GetDocumentData()
        {
            string dbName = "DocumentAPI_TestLabDE_GetDocumentData";

            try
            {
                (FileProcessingDB fileProcessingDb, User user, DocumentController controller) =
                    InitializeAndLogin("Resources.Demo_LabDE.bak", dbName, "admin", "a");

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

        /// <summary>
        /// test FlexIndex documents
        /// </summary>
        [Test, Category("Automated")]
        public static void TestFlexIndex_GetDocumentData()
        {
            string dbName = "DocumentAPI_TestFlexIndex_GetDocumentData";

            try
            {
                (FileProcessingDB fileProcessingDb, User user, DocumentController controller) =
                    InitializeAndLogin("Resources.Demo_FlexIndex.bak", dbName, "admin", "a");

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
        public static void TestManyLineAttribute()
        {
            try
            {
                var voa = new IUnknownVector();
                var ss = new SpatialString();
                string tempUssFileName = _testFiles.GetFile("Resources.Image1.pdf.uss");
                Assert.That(File.Exists(tempUssFileName));

                ss.LoadFrom(tempUssFileName, bSetDirtyFlagToTrue: false);
                var attr = new AttributeClass { Name = "Manual", Value = ss };
                voa.PushBack(attr);

                var mapper = new AttributeMapper(voa, UCLID_FILEPROCESSINGLib.EWorkflowType.kExtraction);
                var docAttrr = mapper.MapAttributesToDocumentAttributeSet(includeNonSpatial: true, verboseSpatialData: true);
                Assert.IsFalse(docAttrr.Attributes.Any(a =>
                    a.SpatialPosition.Pages.Count != a.SpatialPosition.Pages.Distinct().Count()));
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception reported: {0}", ex.Message);
                throw;
            }
        }

        #endregion Public Test Functions

        static (FileProcessingDB fileProcessingDb, User user, DocumentController DocumentController)
            InitializeAndLogin(string dbResource, string dbName, string username, string password)
        {
            (FileProcessingDB fileProcessingDb, User user, UsersController userController) =
                _testDbManager.InitializeEnvironment<TestDocument, UsersController>
                    (dbResource, dbName, username, password);

            var result = userController.Login(user);
            var token = result.AssertGoodResult<JwtSecurityToken>();

            return (fileProcessingDb, user, user.CreateController<DocumentController>());
        }

        static void SetupMetadataFieldValue(int fileId, string value, int fieldId, string dbName)
        {
            string command = Utils.Inv(
                $"UPDATE [dbo].[FileMetadataFieldValue] SET Value='{value}' WHERE [FileID]={fileId} AND [MetadataFieldID]={fieldId};");
            ModifyTable(dbName, command);
        }

        public static void ModifyTable(string dbName, string command)
        {
            try
            {
                using (var cmd = new SqlCommand())
                {
                    // NOTE: "Pooling=false;" keeps the connection from being pooled, and 
                    // allows the conneciton to REALLY close, so the DB can be removed later.
                    string connectionString = "Server=(local);Database=" + dbName + ";Trusted_Connection=True;Pooling=false;";
                    using (var conn = new SqlConnection(connectionString))
                    {
                        conn.Open();

                        cmd.Connection = conn;
                        cmd.CommandText = command;
                        cmd.ExecuteNonQuery();

                        conn.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error: {0}", ex.Message);
            }
        }

        static DocumentDataResult GetDocumentResultSet(int fileId, string dbName)
        {
            try
            {
                ApiTestUtils.SetDefaultApiContext(dbName);

                using (var data = new DocumentData(Utils.CurrentApiContext))
                {
                    Assert.IsTrue(data != null, "null DocumentData reference");
                    return data.GetDocumentData(fileId, includeNonSpatial: true, verboseSpatialData: true);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception: {ex.Message}, in method: {ApiTestUtils.GetMethodName()}");
                throw;
            }
            finally
            {
                FileApiMgr.ReleaseAll();
            }
        }

        static bool CompareXmlToDocumentAttributeSet(string xmlFile, DocumentDataResult documentAttributes)
        {
            try
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
            }
            catch (Exception ex)
            {
                Assert.Fail($"Exception: {ex.Message}, in method: {ApiTestUtils.GetMethodName()}");
            }

            return true;
        }

        static void CompareParentNodeToAttribute(XmlNode parentNode, DocumentAttribute docAttr, bool topLevel = true)
        {
            try
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
            catch (Exception ex)
            {
                Assert.Fail($"Exception: {ex.Message}, in method: {ApiTestUtils.GetMethodName()}");
            }
        }

        static void TestSpatialLineBounds(XmlNode spatialLineBounds, SpatialLineBounds bounds, int i)
        {
            try
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
            catch (Exception ex)
            {
                Assert.Fail($"Exception: {ex.Message}, in method: {ApiTestUtils.GetMethodName()}");
            }
        }

        static XmlNode TestParentNode(XmlNode parentNode, DocumentAttribute docAttr)
        {
            try
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
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception: {ex.Message}, in method: {ApiTestUtils.GetMethodName()}");
                throw;
            }
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
            try
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
            catch (Exception ex)
            {
                Assert.Fail($"Exception: {ex.Message}, in method: {ApiTestUtils.GetMethodName()}");
            }
        }

        static void TestLineText(XmlNode lineTextNode, string text, int index)
        {
            try
            {
                AssertSame(lineTextNode.Name,
                           lineTextNode.InnerText,
                           $"docAttr.SpatialPosition.LineInfo[{index}].SpatialLineZone.Text",
                           text);
            }
            catch (Exception ex)
            {
                Assert.Fail($"Exception: {ex.Message}, in method: {ApiTestUtils.GetMethodName()}");
            }
        }

        //<SpatialLineZone StartX = "1878" StartY="1612" EndX="2264" EndY="1617" Height="3041"/>
        static void TestSpatialLineZone(XmlNode spatialLineZoneNode, SpatialLineZone zone, int i)
        {
            try
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
            catch (Exception ex)
            {
                Assert.Fail($"Exception: {ex.Message}, in method: {ApiTestUtils.GetMethodName()}");
            }
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
    }
}