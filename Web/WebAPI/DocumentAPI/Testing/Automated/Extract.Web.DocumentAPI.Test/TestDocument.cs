using DocumentAPI.Models;
using Extract.FileActionManager.Database.Test;
using Extract.Testing.Utilities;
using NUnit.Framework;
using System;
using System.Diagnostics;
using System.IO;
using ApiUtils = DocumentAPI.Utils;

namespace Extract.Web.DocumentAPI.Test
{
    [TestFixture]
    [NUnit.Framework.Category("WebAPI")]
    public class TestDocument
    {
        #region Constants

        static readonly string DbLabDE = "Demo_LabDE_Temp";
        const int MaxDemo_LabDE_FileId = 18;

        #endregion Constants

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
        /// Tests both DocumentData.SubmitFile, and DocumentData.GetSourceFile
        /// </summary>
        [Test, Category("Automated")]
        public static void Test_SubmitFile()
        {
            UCLID_FILEPROCESSINGLib.FileProcessingDB db = null;

            try
            {
                _testDbManager.GetDatabase("Resources.Demo_LabDE.bak", DbLabDE);

                var c = Utils.SetDefaultApiContext();
                var inf = FileApiMgr.GetInterface(c);
                inf.InUse = false;
                db = inf.Interface;

                var filename = _testFiles.GetFile("Resources.A418.tif");
                var stream = new FileStream(filename, FileMode.Open);

                var result = DocumentData.SubmitFile(filename, stream).Result;
                Assert.IsTrue(result != null, "Null result returned from submitfile");

                var fileId = DocumentData.ConvertIdToFileId(result.Id);
                Assert.IsTrue(fileId > 0, "Bad value for fileId: {0}", fileId);
                Assert.IsTrue(result.Error.ErrorOccurred == false, "An error has been signaled");

                // It is OK to re-submit - the web service writes a unique filename, based on the submitted filename,
                // so test this as well.
                var result2 = DocumentData.SubmitFile(filename, stream).Result;
                Assert.IsTrue(result2 != null, "Null result returned from second submitfile");

                var (sourceFilename, errMsg, err) = DocumentData.GetSourceFileName(result.Id);
                Assert.IsTrue(err != true, "Error signaled by GetSourceFileName, fileId: {0}", result.Id);
                Assert.IsTrue(String.IsNullOrEmpty(errMsg), "An error message was returned: {0}", errMsg);

                // Can't directly test that filenames are equivalent, because the web service takes the base filename
                // and adds a GUID to it, so test that the returned filename contains the original filename.
                var originalFilename = Path.GetFileNameWithoutExtension(filename);
                Assert.IsTrue(sourceFilename.Contains(originalFilename),
                              "source filename: {0}, not equal to original filename: {1}",
                              sourceFilename,
                              filename);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception reported: {0}", ex.Message);
                throw;
            }
            finally
            {
                db.CloseAllDBConnections();
                _testDbManager.RemoveDatabase(DbLabDE);
            }
        }

        /// <summary>
        /// Tests both DocumentData.SubmitText and DocumentData.GetSourceFileName
        /// </summary>
        [Test, Category("Automated")]
        public static void Test_SubmitText()
        {
            UCLID_FILEPROCESSINGLib.FileProcessingDB db = null;

            try
            {
                _testDbManager.GetDatabase("Resources.Demo_LabDE.bak", DbLabDE);

                var c = Utils.SetDefaultApiContext();
                var inf = FileApiMgr.GetInterface(c);
                inf.InUse = false;
                db = inf.Interface;

                var result = DocumentData.SubmitText("Document 1, SSN: 111-22-3333, DOB: 10-04-1999").Result;
                var fileId = DocumentData.ConvertIdToFileId(result.Id);
                Assert.IsTrue(fileId > 0, "Bad value for fileId: {0}", fileId);
                Assert.IsTrue(result.Error.ErrorOccurred == false, "An error has been signaled");

                var (sourceFilename, errMsg, err) = DocumentData.GetSourceFileName(result.Id);
                Assert.IsTrue(err != true, "Error signaled by GetSourceFileName, fileId: {0}", result.Id);
                Assert.IsTrue(String.IsNullOrEmpty(errMsg), "An error message was returned: {0}", errMsg);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception reported: {0}", ex.Message);
                throw;
            }
            finally
            {
                db.CloseAllDBConnections();
                _testDbManager.RemoveDatabase(DbLabDE);
            }
        }

        [Test, Category("Automated")]
        public static void Test_GetStatus()
        {
            UCLID_FILEPROCESSINGLib.FileProcessingDB db = null;

            try
            {
                _testDbManager.GetDatabase("Resources.Demo_LabDE.bak", DbLabDE);

                var c = Utils.SetDefaultApiContext();
                var inf = FileApiMgr.GetInterface(c);
                inf.InUse = false;
                db = inf.Interface;

                for (int i = 1; i <= MaxDemo_LabDE_FileId; ++i)
                {
                    var result = DocumentData.GetStatus(stringId: i.ToString());
                    Assert.IsTrue(result.Count > 0, "Empty result was returned");
                    var status = result[0];
                    Assert.IsTrue(status.DocumentStatus == DocumentProcessingStatus.Done, "Unexpected processing state");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception reported: {0}", ex.Message);
                throw;
            }
            finally
            {
                db.CloseAllDBConnections();
                _testDbManager.RemoveDatabase(DbLabDE);
            }
        }


        /// <summary>
        /// Test GetFileResulot - note that this tests the internal GetResult() function
        /// </summary>
        [Test, Category("Automated")]
        public static void Test_GetFileResult()
        {
            UCLID_FILEPROCESSINGLib.FileProcessingDB db = null;

            try
            {
                _testDbManager.GetDatabase("Resources.Demo_LabDE.bak", DbLabDE);

                var c = Utils.SetDefaultApiContext();
                var inf = FileApiMgr.GetInterface(c);
                inf.InUse = false;
                db = inf.Interface;
                
                for (int i = 1; i <= MaxDemo_LabDE_FileId; ++i)
                {
                    var (filename, isError, errMessage) = DocumentData.GetResult(id: i.ToString());
                    Assert.IsTrue(!String.IsNullOrEmpty(filename), "Empty filename returned");
                    Assert.IsTrue(!isError, "An error was signaled");
                    Assert.IsTrue(String.IsNullOrEmpty(errMessage), "An error message was returned");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception reported: {0}", ex.Message);
                throw;
            }
            finally
            {
                db.CloseAllDBConnections();
                _testDbManager.RemoveDatabase(DbLabDE);
            }
        }

        [Test, Category("Automated")]
        public static void Test_GetTextResult()
        {
            UCLID_FILEPROCESSINGLib.FileProcessingDB db = null;

            try
            {
                _testDbManager.GetDatabase("Resources.Demo_LabDE.bak", DbLabDE);

                var c = Utils.SetDefaultApiContext();
                var inf = FileApiMgr.GetInterface(c);
                inf.InUse = false;
                db = inf.Interface;

                for (int i = 1; i <= MaxDemo_LabDE_FileId; ++i)
                {
                    var result = DocumentData.GetTextResult(textId: i.ToString()).Result;
                    Assert.IsTrue(result.Error.ErrorOccurred == false, "error is indicated");
                    Assert.IsTrue(String.IsNullOrEmpty(result.Error.Message), "error, message: {0}", result.Error.Message);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception reported: {0}", ex.Message);
                throw;
            }
            finally
            {
                db.CloseAllDBConnections();
                _testDbManager.RemoveDatabase(DbLabDE);
            }
        }

        [Test, Category("Automated")]
        public static void Test_GetDocumentType()
        {
            UCLID_FILEPROCESSINGLib.FileProcessingDB db = null;

            try
            {
                _testDbManager.GetDatabase("Resources.Demo_LabDE.bak", DbLabDE);

                var c = Utils.SetDefaultApiContext();
                var inf = FileApiMgr.GetInterface(c);
                inf.InUse = false;
                db = inf.Interface;

                for (int i = 1; i <= MaxDemo_LabDE_FileId; ++i)
                {
                    using (var data = new DocumentData(ApiUtils.CurrentApiContext))
                    {
                        var docType = data.GetDocumentType(id: i.ToString());
                        Assert.IsTrue(!String.IsNullOrEmpty(docType));

                        switch (i)
                        {
                            case 6:
                            case 8:
                                Assert.IsTrue(docType.IsEquivalent("NonLab"), "Document type expected to be NonLab, is: {0}", docType);
                                break;

                            default:
                                Assert.IsTrue(docType.IsEquivalent("Unknown"), "Document type expected to be Unknown, is: {0}", docType);
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception reported: {0}", ex.Message);
                throw;
            }
            finally
            {
                db.CloseAllDBConnections();
                _testDbManager.RemoveDatabase(DbLabDE);
            }
        }


        #endregion Public Test Functions
    }
}