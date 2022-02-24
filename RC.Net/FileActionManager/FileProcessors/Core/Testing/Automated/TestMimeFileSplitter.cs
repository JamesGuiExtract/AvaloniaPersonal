using Extract.Testing.Utilities;
using Extract.Utilities;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;

namespace Extract.FileActionManager.FileProcessors.Test
{
    // Test behavior of the MimeFileSplitter class with mocked IFileProcessingDB
    // There is a test that uses a real database in TestMimeFileSplitterTask
    [TestFixture, Category("MimeFileSplitter"), Category("Automated")]
    public class TestMimeFileSplitter
    {
        const string _HTML_EMAIL = "Resources.Emails.HtmlBodyWithNoAttachments.eml";
        const string _HTML_EMAIL_BODY = "Resources.Emails.HtmlBodyWithNoAttachments_body_text.html";
        const string _TEXT_EMAIL_WITH_ATTACHMENTS = "Resources.Emails.TextBodyWithVariousAttachments.eml";
        const string _TEXT_EMAIL_WITH_UNNAMED_ATTACHMENT = "Resources.Emails.TextBodyWithUnnamedAttachment.eml";

        const int _FIRST_OUTPUT_FILE_ID = 100;

        private MockRepository _mockRepository;

        private Mock<IFileProcessingDB> _fileProcessingDBMock;
        private Mock<IFAMTagManager> _famTagManagerMock;

        string _outputDir;

        static TestFileManager<TestMimeFileSplitter> _testFileManager;

        #region Overhead

        [OneTimeSetUp]
        public static void Setup()
        {
            GeneralMethods.TestSetup();
            _testFileManager = new();
        }

        [OneTimeTearDown]
        public static void FinalCleanup()
        {
            _testFileManager.Dispose();
        }

        [SetUp]
        public void PerTestSetUp()
        {
            _outputDir = FileSystemMethods.GetTemporaryFolder().FullName;

            _mockRepository = new MockRepository(MockBehavior.Default);

            _fileProcessingDBMock = _mockRepository.Create<IFileProcessingDB>();

            _famTagManagerMock = _mockRepository.Create<IFAMTagManager>();
            _famTagManagerMock.Setup(x => x.ExpandTagsAndFunctions(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(_outputDir);
        }

        [TearDown]
        public void PerTestTearDown()
        {
            Directory.Delete(_outputDir, true);
        }

        #endregion

        // Verify that one output file is created when there are no attachments in the email
        [Test]
        public void SplitFile_HtmlEmailWithNoAttachments_CreatesOneOutputFile()
        {
            // Arrange
            var mimeFileSplitter = CreateMimeFileSplitter();

            string sourceDocName = _testFileManager.GetFile(_HTML_EMAIL);
            EmailFileRecord fileRecord = new(sourceDocName);

            SetupFileProcessingDBMock();

            // Act
            mimeFileSplitter.SplitFile(fileRecord);

            // Assert

            // Confirm that the expected output file exists
            string expectedOutputFileName = Path.Combine(_outputDir, Path.GetFileNameWithoutExtension(sourceDocName) + "_body_text.html");
            FileAssert.Exists(expectedOutputFileName);

            // Confirm that the file was added to the database
            long fileSize = new FileInfo(expectedOutputFileName).Length;
            _fileProcessingDBMock.Verify(x =>
                x.AddFileNoQueue(
                    expectedOutputFileName,
                    fileSize,
                    It.IsAny<int>(),
                    It.IsAny<EFilePriority>(),
                    It.IsAny<int>())
                , Times.Once());
        }

        // Verify the content of the body html output file
        [Test]
        public void SplitFile_HtmlEmailWithNoAttachments_VerifyOuputFileContent()
        {
            // Arrange
            var mimeFileSplitter = CreateMimeFileSplitter();

            string sourceDocName = _testFileManager.GetFile(_HTML_EMAIL);
            EmailFileRecord fileRecord = new(sourceDocName);

            SetupFileProcessingDBMock();

            // Act
            mimeFileSplitter.SplitFile(fileRecord);

            // Assert

            string expectedHtml = File.ReadAllText(_testFileManager.GetFile(_HTML_EMAIL_BODY));
            string outputFileName = Path.Combine(_outputDir, Path.GetFileNameWithoutExtension(sourceDocName) + "_body_text.html");
            string actualHtml = File.ReadAllText(outputFileName);

            Assert.That(actualHtml, Is.EqualTo(expectedHtml));
        }

        // Check that all the expected IFileProcessingDB calls happen with the right parameters
        [Test]
        [Pairwise]
        public void SplitFile_HtmlEmailWithNoAttachments_VerifyAllFileProcessingDBCalls(
            [Values(38, 199)] int fileTaskSession,
            [Values(1, 123)] int fileID,
            [Values(17, 24)] int actionID,
            [Values] EFilePriority filePriority,
            [Values(-1, 3)] int workflowID)
        {
            // Arrange
            var mimeFileSplitter = CreateMimeFileSplitter();

            string sourceDocName = _testFileManager.GetFile(_HTML_EMAIL);
            EmailFileRecord fileRecord = new(new FileRecordClass
            {
                Name = sourceDocName,
                FileID = fileID,
                ActionID = actionID,
                Priority = filePriority,
                WorkflowID = workflowID
            });

            List<IUnknownVector> paginationHistory = SetupFileProcessingDBMock(fileTaskSessionIDToUse: fileTaskSession);

            // Act
            mimeFileSplitter.SplitFile(fileRecord);

            // Assert

            // Confirm that a file task session was started and ended
            _fileProcessingDBMock
                .Verify(x => x.StartFileTaskSession(Constants.TaskClassSplitMimeFile, fileRecord.FileID, fileRecord.ActionID), Times.Once());
            _fileProcessingDBMock
                .Verify(x => x.EndFileTaskSession(fileTaskSession, It.IsAny<double>(), It.IsAny<double>(), false), Times.Once());

            // Confirm that the file was added to the database with the expected parameters
            string expectedOutputFileName = Path.Combine(_outputDir, Path.GetFileNameWithoutExtension(sourceDocName) + "_body_text.html");
            long fileSize = new FileInfo(expectedOutputFileName).Length;
            _fileProcessingDBMock.Verify(x => x.AddFileNoQueue(
                expectedOutputFileName,
                fileSize,
                0,
                filePriority,
                workflowID), Times.Once());

            _fileProcessingDBMock.Verify(x => x.AddPaginationHistory(_FIRST_OUTPUT_FILE_ID, It.IsAny<IUnknownVector>(), null, fileTaskSession), Times.Once());

            // Output documents are conceptually one page of the source email
            // so there should be one record added mapping source page 1 to the new document
            Assert.That(paginationHistory.Count, Is.EqualTo(1));
            Assert.That(paginationHistory[0].Size(), Is.EqualTo(1));
            var pair = (IStringPair)paginationHistory[0].At(0);
            Assert.That(pair.StringKey, Is.EqualTo(sourceDocName));
            Assert.That(pair.StringValue, Is.EqualTo("1"));

            _fileProcessingDBMock.VerifyNoOtherCalls();
        }

        // Verify that a file is created for a text body and each attachment
        [Test]
        public void SplitFile_TextEmailWithVariousAttachments_CreatesManyOutputFiles()
        {
            // Arrange
            var mimeFileSplitter = CreateMimeFileSplitter();

            string sourceDocName = _testFileManager.GetFile(_TEXT_EMAIL_WITH_ATTACHMENTS);
            EmailFileRecord fileRecord = new(sourceDocName);

            SetupFileProcessingDBMock();

            // Act
            mimeFileSplitter.SplitFile(fileRecord);

            // Assert

            // Confirm that the expected output files exist
            string baseOutputPath = Path.Combine(_outputDir, Path.GetFileNameWithoutExtension(sourceDocName));
            string[] expectedOutputFileNames = new[]
            {
                baseOutputPath + "_body_text.txt",
                baseOutputPath + "_attachment_001_French.pdf",
                baseOutputPath + "_attachment_002_NERF.png",
                baseOutputPath + "_attachment_003_text.html",
            };

            string[] actualOutputFileNames = Directory.GetFiles(_outputDir);
            CollectionAssert.AreEquivalent(expectedOutputFileNames, actualOutputFileNames);

            foreach (string expectedOutputFileName in expectedOutputFileNames)
            {
                // Confirm that the files were added to the database
                long fileSize = new FileInfo(expectedOutputFileName).Length;
                _fileProcessingDBMock.Verify(x =>
                    x.AddFileNoQueue(
                        expectedOutputFileName,
                        fileSize,
                        It.IsAny<int>(),
                        It.IsAny<EFilePriority>(),
                        It.IsAny<int>())
                    , Times.Once());
            }
        }

        // Verify that the output files are set to pending in the configured action
        [Test]
        public void SplitFile_TextEmailWithVariousAttachments_VerifyActionStatusOfOutputFiles()
        {
            // Arrange
            string outputAction = "A02_Attach";
            var mimeFileSplitter = CreateMimeFileSplitter(outputAction: outputAction);

            string sourceDocName = _testFileManager.GetFile(_TEXT_EMAIL_WITH_ATTACHMENTS);
            int fileID = 94;
            int workflowID = 183;

            EmailFileRecord fileRecord = new(new FileRecordClass
            {
                Name = sourceDocName,
                FileID = fileID,
                WorkflowID = workflowID
            });

            SetupFileProcessingDBMock(outputAction: outputAction);

            // Act
            mimeFileSplitter.SplitFile(fileRecord);

            // Assert
            int expectedNumberOfOutputFiles = 4;
            for (int i = 0; i < expectedNumberOfOutputFiles; i++)
            {
                _fileProcessingDBMock.Verify(x =>
                    x.SetStatusForFile(
                        _FIRST_OUTPUT_FILE_ID + i,
                        outputAction,
                        workflowID,
                        EActionStatus.kActionPending,
                        false,
                        false,
                        out It.Ref<EActionStatus>.IsAny),
                    Times.Once());
            }
        }

        // Check that all the expected IFileProcessingDB calls happen with the right parameters when there are multiple output files
        [Test]
        [Pairwise]
        public void SplitFile_TextEmailWithVariousAttachments_VerifyAllFileProcessingDBCalls(
            [Values(39, 201)] int fileTaskSession,
            [Values(419, 3)] int fileID,
            [Values(14, 30)] int actionID,
            [Values] EFilePriority filePriority,
            [Values(12, 0)] int workflowID)
        {
            // Arrange
            var mimeFileSplitter = CreateMimeFileSplitter();

            string sourceDocName = _testFileManager.GetFile(_TEXT_EMAIL_WITH_ATTACHMENTS);
            EmailFileRecord fileRecord = new(new FileRecordClass
            {
                Name = sourceDocName,
                FileID = fileID,
                ActionID = actionID,
                Priority = filePriority,
                WorkflowID = workflowID
            });

            List<IUnknownVector> paginationHistory = SetupFileProcessingDBMock(fileTaskSessionIDToUse: fileTaskSession);

            // Act
            mimeFileSplitter.SplitFile(fileRecord);

            // Assert

            // Confirm that a file task session was started and ended
            _fileProcessingDBMock
                .Verify(x => x.StartFileTaskSession(Constants.TaskClassSplitMimeFile, fileRecord.FileID, fileRecord.ActionID), Times.Once());
            _fileProcessingDBMock
                .Verify(x => x.EndFileTaskSession(fileTaskSession, It.IsAny<double>(), It.IsAny<double>(), false), Times.Once());

            // Confirm that the files were added to the database with the expected parameters
            string baseOutputPath = Path.Combine(_outputDir, Path.GetFileNameWithoutExtension(sourceDocName));
            string[] expectedOutputFileNames = new[]
            {
                baseOutputPath + "_body_text.txt",
                baseOutputPath + "_attachment_001_French.pdf",
                baseOutputPath + "_attachment_002_NERF.png",
                baseOutputPath + "_attachment_003_text.html",
            };
            int[] expectedPageCounts = new[] { 0, 226, 1, 0 };

            for (int i = 0; i < expectedOutputFileNames.Length; i++)
            {
                string expectedOutputFileName = expectedOutputFileNames[i];
                long fileSize = new FileInfo(expectedOutputFileName).Length;
                _fileProcessingDBMock.Verify(x => x.AddFileNoQueue(
                    expectedOutputFileName,
                    fileSize,
                    expectedPageCounts[i],
                    filePriority,
                    workflowID), Times.Once());

                _fileProcessingDBMock.Verify(x => x.AddPaginationHistory(_FIRST_OUTPUT_FILE_ID + i, It.IsAny<IUnknownVector>(), null, fileTaskSession), Times.Once());

                // Output documents are conceptually one page of the source email
                // so there should be one record added for each output document mapping source page (i + 1) to the new document
                Assert.That(paginationHistory[i].Size(), Is.EqualTo(1));
                var pair = (IStringPair)paginationHistory[i].At(0);
                Assert.That(pair.StringKey, Is.EqualTo(sourceDocName));
                Assert.That(pair.StringValue, Is.EqualTo(UtilityMethods.FormatInvariant($"{i + 1}")));
            }

            _fileProcessingDBMock.VerifyNoOtherCalls();
        }

        // Verify that the system can handle filename collisions
        [Test]
        public void SplitFile_TextEmailWithVariousAttachments_ConfirmFilenameCollisionHandling()
        {
            // Arrange
            var mimeFileSplitter = CreateMimeFileSplitter();

            string sourceDocName = _testFileManager.GetFile(_TEXT_EMAIL_WITH_ATTACHMENTS);

            EmailFileRecord fileRecord = new(sourceDocName);

            SetupFileProcessingDBMock();

            // Setup some special cases for database name collisions
            string baseOutputPath = Path.Combine(_outputDir, Path.GetFileNameWithoutExtension(sourceDocName));
            _fileProcessingDBMock
                .Setup(x => x.AddFileNoQueue(
                    It.Is<string>(path =>
                        path == baseOutputPath + "_body_text.txt"
                        || path == baseOutputPath + "_body_copy_001_text.txt"
                        || path == baseOutputPath + "_body_copy_002_text.txt"
                        || path == baseOutputPath + "_attachment_002_NERF.png"
                        || path == baseOutputPath + "_attachment_002_copy_001_NERF.png"
                        || path == baseOutputPath + "_attachment_003_text.html"
                        || path == baseOutputPath + "_attachment_003_copy_001_text.html"
                        || path == baseOutputPath + "_attachment_003_copy_002_text.html"),
                    It.IsAny<long>(),
                    It.IsAny<int>(),
                    It.IsAny<EFilePriority>(),
                    It.IsAny<int>()))
                .Throws(new Exception("Simulated file name collision"));

            // Setup non-empty record return value for GetResultsForQuery so that the error will be interpreted properly
            var recordsetMock = _mockRepository.Create<ADODB.Recordset>();
            recordsetMock.Setup(x => x.EOF).Returns(false);
            _fileProcessingDBMock.Setup(x => x.GetResultsForQuery(It.IsAny<string>()))
                .Returns(recordsetMock.Object);

            // Setup special cases for file system name collisions
            string fakeFile1 = baseOutputPath + "_attachment_001_French.pdf";
            File.WriteAllText(fakeFile1, "");
            string fakeFile2 = baseOutputPath + "_attachment_003_copy_003_text.html";
            File.WriteAllText(fakeFile2, "");

            // Act
            mimeFileSplitter.SplitFile(fileRecord);

            // Assert
            string[] expectedOutputFileNames = new[]
            {
                fakeFile1,
                fakeFile2,
                baseOutputPath + "_body_copy_003_text.txt",
                baseOutputPath + "_attachment_001_copy_001_French.pdf",
                baseOutputPath + "_attachment_002_copy_002_NERF.png",
                baseOutputPath + "_attachment_003_copy_004_text.html",
            };

            string[] actualOutputFileNames = Directory.GetFiles(_outputDir);
            CollectionAssert.AreEquivalent(expectedOutputFileNames, actualOutputFileNames);
        }

        // Verify that splitting will fail if AddFileNoQueue fails for another reason
        [Test]
        public void SplitFile_TextEmailWithVariousAttachments_ConfirmFailureIfNotAFilenameCollision()
        {
            // Arrange
            string outputAction = "C01_Output";
            var mimeFileSplitter = CreateMimeFileSplitter(outputAction: outputAction);

            string sourceDocName = _testFileManager.GetFile(_TEXT_EMAIL_WITH_ATTACHMENTS);

            EmailFileRecord fileRecord = new(sourceDocName);

            SetupFileProcessingDBMock(outputAction: outputAction);

            // Setup a special case for database name collision
            string baseOutputPath = Path.Combine(_outputDir, Path.GetFileNameWithoutExtension(sourceDocName));
            _fileProcessingDBMock
                .Setup(x => x.AddFileNoQueue(
                    It.Is<string>(path => path == baseOutputPath + "_body_text.txt"),
                    It.IsAny<long>(),
                    It.IsAny<int>(),
                    It.IsAny<EFilePriority>(),
                    It.IsAny<int>()))
                .Throws(new Exception("Simulated database error"));

            // Setup an empty record return value for GetResultsForQuery so that the error will be interpreted properly
            var recordsetMock = _mockRepository.Create<ADODB.Recordset>();
            recordsetMock.Setup(x => x.EOF).Returns(true);
            _fileProcessingDBMock.Setup(x => x.GetResultsForQuery(It.IsAny<string>())).Returns(recordsetMock.Object);

            // Act
            var exn = Assert.Throws<ExtractException>(() => mimeFileSplitter.SplitFile(fileRecord));

            // Assert
            Assert.AreEqual("Simulated database error", exn.Message);
        }

        // Verify that unnamed attachments can be output
        [Test]
        public void SplitFile_TextEmailWithUnnamedAttachment_CreatesExpectedOutputFiles()
        {
            // Arrange
            var mimeFileSplitter = CreateMimeFileSplitter();

            string sourceDocName = _testFileManager.GetFile(_TEXT_EMAIL_WITH_UNNAMED_ATTACHMENT);
            EmailFileRecord fileRecord = new(sourceDocName);

            SetupFileProcessingDBMock();

            // Act
            mimeFileSplitter.SplitFile(fileRecord);

            // Assert

            // Confirm that the expected output files exist
            string baseOutputPath = Path.Combine(_outputDir, Path.GetFileNameWithoutExtension(sourceDocName));
            string[] expectedOutputFileNames = new[]
            {
                baseOutputPath + "_body_text.txt",
                baseOutputPath + "_attachment_001_untitled",
                baseOutputPath + "_attachment_002_text.html",
            };
            int[] expectedPageCounts = new[] { 0, 0, 0 };

            string[] actualOutputFileNames = Directory.GetFiles(_outputDir);
            CollectionAssert.AreEquivalent(expectedOutputFileNames, actualOutputFileNames);

            // Confirm that the files were added to the database
            for (int i = 0; i < expectedOutputFileNames.Length; i++)
            {
                string expectedOutputFileName = expectedOutputFileNames[i];
                long fileSize = new FileInfo(expectedOutputFileName).Length;
                _fileProcessingDBMock.Verify(x =>
                    x.AddFileNoQueue(
                        expectedOutputFileName,
                        fileSize,
                        expectedPageCounts[i],
                        It.IsAny<EFilePriority>(),
                        It.IsAny<int>())
                    , Times.Once());
            }
        }

        #region Helper Methods

        // Create the SUT with mocked dependencies
        MimeFileSplitter CreateMimeFileSplitter(
            string outputAction = null,
            string outputDir = "$DirOf(<SourceDocName>)")
        {
            DatabaseClientForMimeFileSplitter databaseClient = new(_fileProcessingDBMock.Object, outputAction);

            return new(databaseClient, outputDir, _famTagManagerMock.Object);
        }

        // Setup a mock of the IFileProcessingDB dependency that will return the expected data for expected calls
        // Returns a list that will be populated with the source page info passed to each AddPaginationHistory call
        List<IUnknownVector> SetupFileProcessingDBMock(
            int fileTaskSessionIDToUse = 2776,
            string outputAction = null)
        {
            // First a file task session should be opened
            _fileProcessingDBMock
                .Setup(x => x.StartFileTaskSession(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                .Returns(fileTaskSessionIDToUse);

            // Each output document should be added to the database
            var outputFileIDs = new Queue<int>(Enumerable.Range(_FIRST_OUTPUT_FILE_ID, 100));
            _fileProcessingDBMock
                .Setup(x => x.AddFileNoQueue(
                    It.IsAny<string>(),
                    It.IsAny<long>(),
                    It.IsAny<int>(),
                    It.IsAny<EFilePriority>(),
                    It.IsAny<int>()))
                .Returns(outputFileIDs.Dequeue);

            // Each output file should be recorded in the pagination history table
            // Record the source page info so that it can be verified later
            List<IUnknownVector> paginationHistory = new();
            _fileProcessingDBMock
                .Setup(x => x.AddPaginationHistory(It.IsAny<int>(), It.IsAny<IUnknownVector>(), It.IsAny<IUnknownVector>(), fileTaskSessionIDToUse))
                .Callback<int, IUnknownVector, IUnknownVector, int>((_, sourcePageInfo, _, _) => paginationHistory.Add(sourcePageInfo));

            // The output documents should be queued if the output action is specified
            if (outputAction != null)
            {
                _fileProcessingDBMock
                    .Setup(x => x.SetStatusForFile(
                        It.IsAny<int>(),
                        outputAction,
                        It.IsAny<int>(),
                        EActionStatus.kActionPending,
                        false,
                        false,
                        out It.Ref<EActionStatus>.IsAny));
            }

            // The file task session should be closed
            _fileProcessingDBMock
                .Setup(x => x.EndFileTaskSession(It.IsAny<int>(), It.IsAny<double>(), It.IsAny<double>(), It.IsAny<bool>()));

            return paginationHistory;
        }

        #endregion
    }
}
