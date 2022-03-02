using Extract.FileActionManager.Database.Test;
using Extract.Interop;
using Extract.Testing.Utilities;
using Extract.Utilities;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UCLID_FILEPROCESSINGLib;

namespace Extract.FileActionManager.FileProcessors.Test
{
    /// <summary>
    /// Test FAM task behavior. More tests are in TestMimeFileSplitter
    /// </summary>
    [TestFixture]
    [Category("ConvertEmailToPdfTask"), Category("Automated")]
    public class TestConvertEmailToPdfTask
    {
        const string _HTML_EMAIL_WITH_ATTACHMENTS = "Resources.Emails.HtmlBodyWithAttachments.eml";
        const string _HTML_EMAIL_WITH_INLINE_IMAGE = "Resources.Emails.HtmlBodyWithInlineImage.eml";
        const string _TEXT_EMAIL_WITH_ATTACHMENTS = "Resources.Emails.TextBodyWithAttachments.eml";

        TestFileManager<TestConvertEmailToPdfTask> _testFiles;
        FAMTestDBManager<TestConvertEmailToPdfTask> _testDbManager;

        #region Overhead

        [OneTimeSetUp]
        public static void Setup()
        {
            GeneralMethods.TestSetup();
        }

        [SetUp]
        public void PerTestSetup()
        {
            _testFiles = new();
            _testDbManager = new();
        }

        [TearDown]
        public void PerTestTearDown()
        {
            _testFiles.Dispose();
            _testDbManager.Dispose();
        }

        #endregion Overhead

        #region Tests

        // Confirm that save/load works correctly
        [Test]
        [Pairwise]
        public void TestSerialization(
            [Values] ConvertEmailProcessingMode processingMode,
            [Values(null, "", @"C:\OutDir\$FileOf(<SourceDocName>)")] string outputFile,
            [Values(null, "", @"C:\OutDir")] string outputDir,
            [Values(null, "", "SA", "Cleanup")] string sourceAction,
            [Values(null, "", "OA", "Attachments")] string outputAction)
        {
            // Arrange
            ConvertEmailToPdfTask task = new()
            {
                ProcessingMode = processingMode,
                OutputFilePath = outputFile,
                OutputDirectory = outputDir,
                SourceAction = sourceAction,
                OutputAction = outputAction
            };
            using var stream = new MemoryStream();

            // Act
            task.Save(new IStreamWrapper(stream), false);

            stream.Position = 0;
            ConvertEmailToPdfTask loadedTask = new();
            loadedTask.Load(new IStreamWrapper(stream));

            // Assert
            Assert.AreEqual(processingMode, loadedTask.ProcessingMode);
            Assert.AreEqual(outputFile, loadedTask.OutputFilePath);
            Assert.AreEqual(outputDir, loadedTask.OutputDirectory);
            Assert.AreEqual(sourceAction, loadedTask.SourceAction);
            Assert.AreEqual(outputAction, loadedTask.OutputAction);
        }

        // Confirm that clone works correctly
        [Test]
        [Pairwise]
        public void TestCopy(
            [Values] ConvertEmailProcessingMode processingMode,
            [Values(null, "", @"C:\OutDir\$FileOf(<SourceDocName>)")] string outputFile,
            [Values(null, "", @"C:\OutDir")] string outputDir,
            [Values(null, "", "SA", "Cleanup")] string sourceAction,
            [Values(null, "", "OA", "Attachments")] string outputAction)
        {
            // Arrange
            ConvertEmailToPdfTask task = new()
            {
                ProcessingMode = processingMode,
                OutputFilePath = outputFile,
                OutputDirectory = outputDir,
                SourceAction = sourceAction,
                OutputAction = outputAction
            };

            // Act
            ConvertEmailToPdfTask clonedTask = (ConvertEmailToPdfTask)task.Clone();

            // Assert
            Assert.AreEqual(processingMode, clonedTask.ProcessingMode);
            Assert.AreEqual(outputFile, clonedTask.OutputFilePath);
            Assert.AreEqual(outputDir, clonedTask.OutputDirectory);
            Assert.AreEqual(sourceAction, clonedTask.SourceAction);
            Assert.AreEqual(outputAction, clonedTask.OutputAction);
        }

        /// <summary>
        /// Tests the IMustBeConfigured interface for the splitter mode
        /// </summary>
        [Test]
        public void IMustBeConfiguredObject_SplitMode()
        {
            var task = new ConvertEmailToPdfTask { ProcessingMode = ConvertEmailProcessingMode.Split };
            Assert.That(task.IsConfigured());

            // Output file is not required
            task.OutputFilePath = "";
            Assert.That(task.IsConfigured());

            // Output directory is required
            task.OutputDirectory = "";
            Assert.That(task.IsConfigured(), Is.False);
        }

        /// <summary>
        /// Tests the IMustBeConfigured interface for the combo mode
        /// </summary>
        [Test]
        public void IMustBeConfiguredObject_ComboMode()
        {
            var task = new ConvertEmailToPdfTask { ProcessingMode = ConvertEmailProcessingMode.Combo };
            Assert.That(task.IsConfigured());

            // Output directory isn't required
            task.OutputDirectory = "";
            Assert.That(task.IsConfigured());

            // Output file is required for Combo mode
            task.OutputFilePath = "";
            Assert.That(task.IsConfigured(), Is.False);
        }

        /// <summary>
        /// Tests using the task's ProcessFile method to divide the input
        /// </summary>
        [Test]
        public void ProcessFile_SplitMode_ConfirmOutputFiles([Values] bool setActionStatusForOutput)
        {
            // Arrange
            string databaseName = _testDbManager.GenerateDatabaseName();
            using var db = _testDbManager.GetDisposableDatabase(databaseName, actionNames: new[] { "a_input", "b_output", "c_source" });

            string[] inputFiles = new string[3];
            inputFiles[0] = _testFiles.GetFile(_HTML_EMAIL_WITH_INLINE_IMAGE);
            inputFiles[1] = _testFiles.GetFile(_TEXT_EMAIL_WITH_ATTACHMENTS);
            inputFiles[2] = _testFiles.GetFile(_HTML_EMAIL_WITH_ATTACHMENTS);

            List<FileRecord> records = QueueFiles(db.FileProcessingDB, inputFiles, "a_input");

            ConvertEmailToPdfTask task = new()
            {
                ProcessingMode = ConvertEmailProcessingMode.Split,
                OutputDirectory = @"$DirOf(<SourceDocName>)\$FileNoExtOf(<SourceDocName>)",
                OutputAction = setActionStatusForOutput ? "b_output" : null,
                SourceAction = "c_source"
            };
            task.Init(1, null, db.FileProcessingDB, null);

            // Act
            foreach (var fileRecord in records)
            {
                task.ProcessFile(fileRecord, 1, null, db.FileProcessingDB, null, false);
            }

            // Assert
            string rootOutputDir = Path.GetDirectoryName(inputFiles[0]);
            var expectedOutputFiles = new string[]
            {
                    Path.Combine(rootOutputDir,
                        Path.GetFileNameWithoutExtension(inputFiles[0]),
                        Path.GetFileNameWithoutExtension(inputFiles[0]) + "_body_text.html"),
                    Path.Combine(
                        rootOutputDir,
                        Path.GetFileNameWithoutExtension(inputFiles[1]),
                        Path.GetFileNameWithoutExtension(inputFiles[1]) + "_body_text.txt"),
                    Path.Combine(
                        rootOutputDir,
                        Path.GetFileNameWithoutExtension(inputFiles[1]),
                        Path.GetFileNameWithoutExtension(inputFiles[1]) + "_attachment_001_Nat 1-28, 2-4, and 2-7-2022.pdf"),
                    Path.Combine(
                        rootOutputDir,
                        Path.GetFileNameWithoutExtension(inputFiles[2]),
                        Path.GetFileNameWithoutExtension(inputFiles[2]) + "_body_text.html"),
                    Path.Combine(
                        rootOutputDir,
                        Path.GetFileNameWithoutExtension(inputFiles[2]),
                        Path.GetFileNameWithoutExtension(inputFiles[2]) + "_attachment_001_LEADTOOLS v22 Developer DLI.pdf"),
            };

            // ----------------------------
            // Confirm that the expected files are on disk
            // ----------------------------
            var expectedFiles = new string[][]
            {
                    inputFiles, // Both the original and output files will be in the folder being checked
                    expectedOutputFiles
            }
            .SelectMany(x => x)
            .ToList();

            var actualFiles = Directory.GetFiles(rootOutputDir, "*.*", SearchOption.AllDirectories);
            CollectionAssert.AreEquivalent(expectedFiles, actualFiles);

            // ----------------------------
            // Confirm the database status of the output files
            // ----------------------------
            var pendingFilesInOutputAction = db.FileProcessingDB.GetFilesToProcess("b_output", int.MaxValue, false, null)
                .ToIEnumerable<IFileRecord>()
                .Select(fileRecord => fileRecord.Name)
                .ToList();

            if (setActionStatusForOutput)
            {
                CollectionAssert.AreEquivalent(expectedOutputFiles, pendingFilesInOutputAction);
            }
            else
            {
                CollectionAssert.IsEmpty(pendingFilesInOutputAction);
            }
        }

        /// <summary>
        /// Tests using the task's ProcessFile method to convert the input to a PDF file
        /// </summary>
        [Test]
        [Pairwise]
        public void ProcessFile_ComboMode_ConfirmOutputFiles(
            [Values] bool setActionStatusForOutput,
            [Values] bool modifySourceDocNameInDatabase)
        {
            // Arrange
            string databaseName = _testDbManager.GenerateDatabaseName();
            using var db = _testDbManager.GetDisposableDatabase(databaseName, actionNames: new[] { "a_input", "b_output", "c_source" });

            string[] inputFiles = new string[3];
            inputFiles[0] = _testFiles.GetFile(_HTML_EMAIL_WITH_INLINE_IMAGE);
            inputFiles[1] = _testFiles.GetFile(_TEXT_EMAIL_WITH_ATTACHMENTS);
            inputFiles[2] = _testFiles.GetFile(_HTML_EMAIL_WITH_ATTACHMENTS);

            List<FileRecord> records = QueueFiles(db.FileProcessingDB, inputFiles, "a_input");

            ConvertEmailToPdfTask task = new()
            {
                ProcessingMode = ConvertEmailProcessingMode.Combo,
                ModifySourceDocName = modifySourceDocNameInDatabase,
                OutputFilePath = @"$ChangeExt(<SourceDocName>,pdf)", // Test non-default path function
                OutputAction = setActionStatusForOutput ? "b_output" : null,
                SourceAction = "c_source"
            };
            task.Init(1, null, db.FileProcessingDB, null);

            // Act
            foreach (var fileRecord in records)
            {
                task.ProcessFile(fileRecord, 1, null, db.FileProcessingDB, null, false);
            }

            // Assert
            string rootOutputDir = Path.GetDirectoryName(inputFiles[0]);

            var expectedOutputFiles = new string[]
            {
                    Path.Combine(rootOutputDir, Path.GetFileNameWithoutExtension(inputFiles[0]) + ".pdf"),
                    Path.Combine(rootOutputDir, Path.GetFileNameWithoutExtension(inputFiles[1]) + ".pdf"),
                    Path.Combine(rootOutputDir, Path.GetFileNameWithoutExtension(inputFiles[2]) + ".pdf"),
            };

            // ----------------------------
            // Confirm that the expected files are on disk
            // ----------------------------
            var expectedFiles = new string[][]
            {
                    inputFiles, // Both the original and output files will be in the folder being checked
                    expectedOutputFiles
            }
            .SelectMany(x => x)
            .ToList();

            var actualFiles = Directory.GetFiles(rootOutputDir, "*.*", SearchOption.AllDirectories);
            CollectionAssert.AreEquivalent(expectedFiles, actualFiles);

            // ----------------------------
            // Confirm the database status of the output files
            // ----------------------------
            var pendingFilesInOutputAction = db.FileProcessingDB.GetFilesToProcess("b_output", int.MaxValue, false, null)
                .ToIEnumerable<IFileRecord>()
                .Select(fileRecord => fileRecord.Name)
                .ToList();

            // The output action is ignored if modifying the source name to be the output name
            if (setActionStatusForOutput && !modifySourceDocNameInDatabase)
            {
                var expectedPending = inputFiles.Select(f => Path.ChangeExtension(f, ".pdf")).ToList();
                CollectionAssert.AreEquivalent(expectedPending, pendingFilesInOutputAction);
            }
            else
            {
                CollectionAssert.IsEmpty(pendingFilesInOutputAction);
            }

            // ----------------------------
            // Confirm behavior of modifySourceDocName
            // ----------------------------
            var currentInputFileNamesInDatabase = inputFiles
                .Select((_, i) => db.FileProcessingDB.GetFileNameFromFileID(i + 1))
                .ToList();

            if (modifySourceDocNameInDatabase)
            {
                CollectionAssert.AreEquivalent(expectedOutputFiles, currentInputFileNamesInDatabase);
            }
            else
            {
                CollectionAssert.AreEquivalent(inputFiles, currentInputFileNamesInDatabase);
            }
        }

        /// <summary>
        /// Tests that the source file is set to pending when using the task's ProcessFile method to divide the input
        /// </summary>
        [Test]
        [Pairwise]
        public void ProcessFile_ConfirmSourceFilesAreSetToPending(
            [Values] ConvertEmailProcessingMode processingMode,
            [Values] bool setActionStatusForSource)
        {
            // Arrange
            string databaseName = _testDbManager.GenerateDatabaseName();
            using var db = _testDbManager.GetDisposableDatabase(databaseName, actionNames: new[] { "a_input", "b_output", "c_source" });

            string[] inputFiles = new string[3];
            inputFiles[0] = _testFiles.GetFile(_HTML_EMAIL_WITH_INLINE_IMAGE);
            inputFiles[1] = _testFiles.GetFile(_TEXT_EMAIL_WITH_ATTACHMENTS);
            inputFiles[2] = _testFiles.GetFile(_HTML_EMAIL_WITH_ATTACHMENTS);

            List<FileRecord> records = QueueFiles(db.FileProcessingDB, inputFiles, "a_input");

            ConvertEmailToPdfTask task = new()
            {
                ProcessingMode = processingMode,
                ModifySourceDocName = false,
                OutputFilePath = @"<SourceDocName>.pdf",
                OutputDirectory = @"$DirOf(<SourceDocName>)\$FileNoExtOf(<SourceDocName>)",
                OutputAction = "b_output",
                SourceAction = setActionStatusForSource ? "c_source" : null
            };
            task.Init(1, null, db.FileProcessingDB, null);

            // Act
            foreach (var fileRecord in records)
            {
                task.ProcessFile(fileRecord, 1, null, db.FileProcessingDB, null, false);
            }

            // Assert

            // List the files that are pending in the source action
            var pendingFilesInSourceAction = db.FileProcessingDB.GetFilesToProcess("c_source", int.MaxValue, false, null)
                .ToIEnumerable<IFileRecord>()
                .Select(fileRecord => fileRecord.Name)
                .ToList();

            if (setActionStatusForSource)
            {
                CollectionAssert.AreEquivalent(inputFiles, pendingFilesInSourceAction);
            }
            else
            {
                CollectionAssert.IsEmpty(pendingFilesInSourceAction);
            }
        }

        #endregion

        #region Helper Methods

        private static List<FileRecord> QueueFiles(FileProcessingDB db, string[] inputFiles, string action, int workflowID = -1)
        {
            return inputFiles.Select(file =>
                db.AddFile(file, action, workflowID, EFilePriority.kPriorityNormal, false, false, EActionStatus.kActionPending, false, out _, out _)
            ).ToList();
        }

        #endregion
    }
}
