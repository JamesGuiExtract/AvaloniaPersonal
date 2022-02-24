using Extract.FileActionManager.Database.Test;
using Extract.Interop;
using Extract.Testing.Utilities;
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
    [Category("ProcessRichTextBatchesTask"), Category("Automated")]
    public class TestMimeFileSplitterTask
    {
        const string _HTML_EMAIL_WITH_ATTACHMENTS = "Resources.Emails.HtmlBodyWithAttachments.eml";
        const string _HTML_EMAIL_WITH_INLINE_IMAGE = "Resources.Emails.HtmlBodyWithInlineImage.eml";
        const string _TEXT_EMAIL_WITH_ATTACHMENTS = "Resources.Emails.TextBodyWithAttachments.eml";

        static TestFileManager<TestMimeFileSplitterTask> _testFiles;
        static FAMTestDBManager<TestMimeFileSplitterTask> _testDbManager;

        #region Overhead

        [OneTimeSetUp]
        public static void Setup()
        {
            GeneralMethods.TestSetup();
            _testFiles = new();
            _testDbManager = new();
        }

        [OneTimeTearDown]
        public static void FinalCleanup()
        {
            _testFiles.Dispose();
            _testDbManager.Dispose();
        }

        #endregion Overhead

        #region Tests

        // Confirm that save/load works correctly
        [Test]
        [Pairwise]
        public static void TestSerialization(
            [Values(null, "", @"C:\OutDir")] string outputDir,
            [Values(null, "", "SA", "Cleanup")] string sourceAction,
            [Values(null, "", "OA", "Attachments")] string outputAction)
        {
            // Arrange
            SplitMimeFileTask task = new() { OutputDirectory = outputDir, SourceAction = sourceAction, OutputAction = outputAction};
            using var stream = new MemoryStream();

            // Act
            task.Save(new IStreamWrapper(stream), false);

            stream.Position = 0;
            SplitMimeFileTask loadedTask = new();
            loadedTask.Load(new IStreamWrapper(stream));

            // Assert
            Assert.AreEqual(outputDir, loadedTask.OutputDirectory);
            Assert.AreEqual(sourceAction, loadedTask.SourceAction);
            Assert.AreEqual(outputAction, loadedTask.OutputAction);
        }

        // Confirm that clone works correctly
        [Test]
        [Pairwise]
        public static void TestCopy(
            [Values(null, "", @"C:\OutDir")] string outputDir,
            [Values(null, "", "SA", "Cleanup")] string sourceAction,
            [Values(null, "", "OA", "Attachments")] string outputAction)
        {
            // Arrange
            SplitMimeFileTask task = new() { OutputDirectory = outputDir, SourceAction = sourceAction, OutputAction = outputAction};

            // Act
            SplitMimeFileTask clonedTask = (SplitMimeFileTask)task.Clone();

            // Assert
            Assert.AreEqual(outputDir, clonedTask.OutputDirectory);
            Assert.AreEqual(sourceAction, clonedTask.SourceAction);
            Assert.AreEqual(outputAction, clonedTask.OutputAction);
        }

        /// <summary>
        /// Tests the IMustBeConfigured interface
        /// </summary>
        [Test]
        public static void IMustBeConfiguredObject_Interface()
        {
            var task = new SplitMimeFileTask();
            Assert.That(task.IsConfigured());

            task.OutputDirectory = "";
            Assert.That(task.IsConfigured(), Is.False);
        }

        /// <summary>
        /// Tests using the task's ProcessFile method to divide the input
        /// </summary>
        [Test]
        public static void ProcessFile_ConfirmOutputFilesExist()
        {
            // Arrange
            string databaseName = _testDbManager.GenerateDatabaseName();
            FileProcessingDB db = _testDbManager.GetNewDatabase(databaseName);

            try
            {
                db.DefineNewAction("a_input");
                db.DefineNewAction("b_output");
                db.DefineNewAction("c_source");

                string[] inputFiles = new string[3];
                inputFiles[0] = _testFiles.GetFile(_HTML_EMAIL_WITH_INLINE_IMAGE);
                inputFiles[1] = _testFiles.GetFile(_TEXT_EMAIL_WITH_ATTACHMENTS);
                inputFiles[2] = _testFiles.GetFile(_HTML_EMAIL_WITH_ATTACHMENTS);

                List<FileRecord> records = QueueFiles(db, inputFiles, "a_input");

                SplitMimeFileTask task = new()
                {
                    OutputDirectory = @"$DirOf(<SourceDocName>)\$FileNoExtOf(<SourceDocName>)",
                    OutputAction = "b_output",
                    SourceAction = "c_source"
                };
                task.Init(1, null, db, null);

                db.RecordFAMSessionStart("DUMMY", "a_input", true, true);

                // Act
                foreach (var fileRecord in records)
                {
                    task.ProcessFile(fileRecord, 1, null, db, null, false);
                }

                // Assert
                string rootOutputDir = Path.GetDirectoryName(inputFiles[0]);
                var expectedFiles = new string[]
                {
                    // Input files
                    Path.Combine(rootOutputDir, Path.GetFileName(inputFiles[0])),
                    Path.Combine(rootOutputDir, Path.GetFileName(inputFiles[1])),
                    Path.Combine(rootOutputDir, Path.GetFileName(inputFiles[2])),

                    // Output files
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

                var actualFiles = Directory.GetFiles(rootOutputDir, "*.*", SearchOption.AllDirectories);

                CollectionAssert.AreEquivalent(expectedFiles, actualFiles);
            }
            finally
            {
                try
                {
                    db.RecordFAMSessionStop();
                }
                catch { /**/ }
                _testDbManager.RemoveDatabase(databaseName);
            }
        }

        /// <summary>
        /// Tests that the source file is set to pending when using the task's ProcessFile method to divide the input
        /// </summary>
        [Test]
        public static void ProcessFile_ConfirmSourceFilesAreSetToPending([Values] bool setActionStatusForSource)
        {
            // Arrange
            string databaseName = _testDbManager.GenerateDatabaseName();
            FileProcessingDB db = _testDbManager.GetNewDatabase(databaseName);

            try
            {
                db.DefineNewAction("a_input");
                db.DefineNewAction("b_output");
                db.DefineNewAction("c_source");

                string[] inputFiles = new string[3];
                inputFiles[0] = _testFiles.GetFile(_HTML_EMAIL_WITH_INLINE_IMAGE);
                inputFiles[1] = _testFiles.GetFile(_TEXT_EMAIL_WITH_ATTACHMENTS);
                inputFiles[2] = _testFiles.GetFile(_HTML_EMAIL_WITH_ATTACHMENTS);

                List<FileRecord> records = QueueFiles(db, inputFiles, "a_input");

                SplitMimeFileTask task = new()
                {
                    OutputDirectory = @"$DirOf(<SourceDocName>)\$FileNoExtOf(<SourceDocName>)",
                    OutputAction = "b_output",
                    SourceAction = setActionStatusForSource ? "c_source" : null
                };
                task.Init(1, null, db, null);

                db.RecordFAMSessionStart("DUMMY", "a_input", true, true);

                // Act
                foreach (var fileRecord in records)
                {
                    task.ProcessFile(fileRecord, 1, null, db, null, false);
                }

                // Assert
                var pendingFilesInSourceAction = inputFiles
                    .Select((fileName, i) => new { fileName, status = db.GetFileStatus(i + 1, "c_source", false) })
                    .Where(o => o.status == EActionStatus.kActionPending)
                    .Select(o => o.fileName)
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
            finally
            {
                try
                {
                    db.RecordFAMSessionStop();
                }
                catch { /**/ }
                _testDbManager.RemoveDatabase(databaseName);
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
