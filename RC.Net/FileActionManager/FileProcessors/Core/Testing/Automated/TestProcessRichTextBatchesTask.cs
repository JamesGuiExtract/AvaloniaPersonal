using Extract.FileActionManager.Database.Test;
using Extract.Redaction.Davidson;
using Extract.Testing.Utilities;
using Extract.Utilities;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using UCLID_AFCORELib;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;

namespace Extract.FileActionManager.FileProcessors.Test
{
    /// <summary>
    /// Provides unit test cases for the <see cref="ProcessRichTextBatchesTask"/>.
    /// </summary>
    [TestFixture]
    [Category("ProcessRichTextBatchesTask")]
    public class TestProcessRichTextBatchesTask
    {
        #region Constants

        const string RTF_HEADER = @"\rtf1\ansi\ansicpg1252\deff0\nouicompat\deflang1033";

        static readonly Encoding _encoding = Encoding.GetEncoding("windows-1252");

        #endregion Constants

        #region Fields

        static TestFileManager<TestProcessRichTextBatchesTask> _testFiles;
        static FAMTestDBManager<TestProcessRichTextBatchesTask> _testDbManager;
        static List<string> _outputFolders = new List<string>();

        #endregion Fields

        #region Overhead Methods

        /// <summary>
        /// Setup method to initialize the testing environment.
        /// </summary>
        [OneTimeSetUp]
        public static void Setup()
        {
            GeneralMethods.TestSetup();
            _testFiles = new TestFileManager<TestProcessRichTextBatchesTask>();
            _testDbManager = new FAMTestDBManager<TestProcessRichTextBatchesTask>();
        }

        /// <summary>
        /// Cleanup after all tests have run.
        /// </summary>
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
            }
            // Delete temp dir
            foreach(var dir in _outputFolders.Where(dir => Directory.Exists(dir)))
            {
                Directory.Delete(dir, true);
            }
        }

        #endregion Overhead Methods

        #region Tests

        /// <summary>
        /// Tests splitting a batch that contains two items with multiple files in the content and one with empty content
        /// </summary>
        [Test]
        public static void SplitBatch()
        {
            string inputFile = _testFiles.GetFile("Resources.BatchWithSimpleContent.txt");
            string[] inp = File.ReadAllLines(inputFile, _encoding);

            var afDoc = new AFDocumentClass();
            afDoc.Text.SourceDocName = inputFile;
            var pathTags = new AttributeFinder.AttributeFinderPathTags(afDoc);
            var batchItems = RichTextFormatBatchProcessor.DivideBatch(inp, "$DirOf(<SourceDocName>)", pathTags).ToList();

            Assert.AreEqual(10, batchItems.Count);
            Assert.AreEqual(6, batchItems.OfType<OutputFileData>().Count());
        }


        /// <summary>
        /// Tests splitting a batch that contains a '|' character in RTF file contents
        /// </summary>
        [Test]
        public static void SplitBatchWithPipeInContent()
        {
            string inputFile = _testFiles.GetFile("Resources.BatchWithPipeInContent.txt");
            string[] inp = File.ReadAllLines(inputFile, _encoding);

            var afDoc = new AFDocumentClass();
            afDoc.Text.SourceDocName = inputFile;
            var pathTags = new AttributeFinder.AttributeFinderPathTags(afDoc);
            var batchItems = RichTextFormatBatchProcessor.DivideBatch(inp, "$DirOf(<SourceDocName>)", pathTags).ToList();

            Assert.AreEqual(9, batchItems.Count);
            Assert.AreEqual(6, batchItems.OfType<OutputFileData>().Count());
            Assert.That(batchItems.OfType<OutputFileData>().All(x => x.FileType == OutputFileType.RichTextFile));
        }

        /// <summary>
        /// Tests splitting a batch that has an invalid RTF file in it
        /// </summary>
        [Test]
        public static void SplitBatchWithInvalidContent()
        {
            string inputFile = _testFiles.GetFile("Resources.BatchWithInvalidContent.txt");
            string[] inp = File.ReadAllLines(inputFile, _encoding);

            var afDoc = new AFDocumentClass();
            afDoc.Text.SourceDocName = inputFile;
            var pathTags = new AttributeFinder.AttributeFinderPathTags(afDoc);
            var batchItems = RichTextFormatBatchProcessor.DivideBatch(inp, "$DirOf(<SourceDocName>)", pathTags).ToList();

            Assert.AreEqual(10, batchItems.Count);
            Assert.AreEqual(8, batchItems.OfType<OutputFileData>().Count());
            Assert.AreEqual(7, batchItems.OfType<OutputFileData>().Where(x => x.FileType == OutputFileType.RichTextFile).Count());
            Assert.AreEqual(1, batchItems.OfType<OutputFileData>().Where(x => x.FileType == OutputFileType.TextFile).Count());
        }

        /// <summary>
        /// Tests round-trip equality for Text -> DivideBatch -> ToDavidsonFormat
        /// </summary>
        [Test]
        public static void DavidsonFormatHomomorphism()
        {
            string[] inputFiles = new string[3];
            inputFiles[0] = _testFiles.GetFile("Resources.BatchWithSimpleContent.txt");
            inputFiles[1] = _testFiles.GetFile("Resources.BatchWithPipeInContent.txt");
            inputFiles[2] = _testFiles.GetFile("Resources.BatchWithInvalidContent.txt");
            foreach (var inputFile in inputFiles)
            {
                string original = File.ReadAllText(inputFile, _encoding);
                string[] inp = File.ReadAllLines(inputFile, _encoding);

                var afDoc = new AFDocumentClass();
                afDoc.Text.SourceDocName = inputFile;
                var pathTags = new AttributeFinder.AttributeFinderPathTags(afDoc);
                var batchItems = RichTextFormatBatchProcessor.DivideBatch(inp, "$DirOf(<SourceDocName>)", pathTags);

                string reconstituted = null;
                using (var stream = new MemoryStream())
                {
                    foreach (var item in batchItems)
                    {
                        item.ToDavidsonFormat(stream);
                    }

                    stream.Flush();
                    stream.Position = 0;
                    reconstituted = new StreamReader(stream, _encoding).ReadToEnd();
                }

                Assert.AreEqual(original, reconstituted);
            }
        }

        /// <summary>
        /// Tests round-trip equality for Text -> DivideBatch -> ToProtobuf -> FromProtobuf -> ToDavisonFormat
        /// </summary>
        [Test]
        public static void ProtobufHomomorphism()
        {
            string[] inputFiles = new string[3];
            inputFiles[0] = _testFiles.GetFile("Resources.BatchWithSimpleContent.txt");
            inputFiles[1] = _testFiles.GetFile("Resources.BatchWithPipeInContent.txt");
            inputFiles[2] = _testFiles.GetFile("Resources.BatchWithInvalidContent.txt");
            foreach (var inputFile in inputFiles)
            {
                string original = File.ReadAllText(inputFile, _encoding);
                string[] inp = File.ReadAllLines(inputFile, _encoding);

                var afDoc = new AFDocumentClass();
                afDoc.Text.SourceDocName = inputFile;
                var pathTags = new AttributeFinder.AttributeFinderPathTags(afDoc);
                IEnumerable<BatchFileItem> batchItems = RichTextFormatBatchProcessor.DivideBatch(inp, "$DirOf(<SourceDocName>)", pathTags);

                // Write/read to protocol buffer stream
                List<BatchFileItem> deserialized = new List<BatchFileItem>();
                using (var stream = new MemoryStream())
                {
                    foreach (var item in batchItems)
                    {
                        item.ToProtobuf(stream);
                    }

                    stream.Flush();
                    stream.Position = 0;

                    while (stream.ReadByte() > 0)
                    {
                        stream.Position--;
                        deserialized.Add(BatchFileItem.FromProtobuf(stream));
                    }
                }

                // Write back to a string
                string reconstituted = null;
                using (var stream = new MemoryStream())
                {
                    foreach (var item in deserialized)
                    {
                        item.ToDavidsonFormat(stream);
                    }

                    stream.Flush();
                    stream.Position = 0;
                    reconstituted = new StreamReader(stream, _encoding).ReadToEnd();
                }

                Assert.AreEqual(original, reconstituted);
            }
        }

        /// <summary>
        /// Tests using ProcessFile method to divide the input
        /// </summary>
        [Test]
        public static void ProcessFile_Divide()
        {
            const string DB_NAME = "B6C4A57E-6834-40D1-BECB-CFF55999AD7F";
            var db = _testDbManager.GetNewDatabase(DB_NAME);
            try
            {
                db.DefineNewAction("a");

                string[] inputFiles = new string[3];
                inputFiles[0] = _testFiles.GetFile("Resources.BatchWithSimpleContent.txt");
                inputFiles[1] = _testFiles.GetFile("Resources.BatchWithPipeInContent.txt");
                inputFiles[2] = _testFiles.GetFile("Resources.BatchWithInvalidContent.txt");

                List<FileRecord> records = QueueFiles(db, inputFiles);

                _outputFolders.Add(FileSystemMethods.GetTemporaryFolderName());

                var divideTask = new ProcessRichTextBatchesTask
                {
                    DivideBatchIntoRichTextFiles = true,
                    OutputDirectory = _outputFolders.Last() + "\\" + RichTextFormatBatchProcessor.SubBatchNumber
                };
                divideTask.Init(1, null, db, null);

                db.RecordFAMSessionStart("DUMMY", "a", true, true);
                foreach (var fileRecord in records)
                {
                    divideTask.ProcessFile(fileRecord, 1, null, db, null, false);
                }

                var expectedFiles = new string[] {
                    @"001\BatchWithInvalidContent.txt.line-000001.label-1048308.sub-001.sublabel-HEADER0001.rtf",
                    @"001\BatchWithInvalidContent.txt.line-000001.label-1048308.sub-002.sublabel-DETAIL0001.rtf",
                    @"001\BatchWithInvalidContent.txt.line-000001.label-1048308.sub-003.sublabel-Extract_Invalid_RTF.txt",
                    @"001\BatchWithInvalidContent.txt.line-000001.label-1048308.sub-004.sublabel-DETAIL0001.rtf",
                    @"001\BatchWithInvalidContent.txt.line-000001.label-1048308.sub-005.sublabel-Extract_No_Label.rtf",
                    @"001\BatchWithInvalidContent.txt.line-000042.label-1048332.sub-001.sublabel-HEADER0001.rtf",
                    @"001\BatchWithInvalidContent.txt.line-000042.label-1048332.sub-002.sublabel-DETAIL0001.rtf",
                    @"001\BatchWithInvalidContent.txt.line-000042.label-1048332.sub-003.sublabel-Extract_No_Label.rtf",
                    @"001\BatchWithPipeInContent.txt.line-000002.label-1086935.sub-001.sublabel-HEADER0001.rtf",
                    @"001\BatchWithPipeInContent.txt.line-000002.label-1086935.sub-002.sublabel-DETAIL0001.rtf",
                    @"001\BatchWithPipeInContent.txt.line-000002.label-1086935.sub-003.sublabel-Extract_No_Label.rtf",
                    @"001\BatchWithPipeInContent.txt.line-000032.label-1185038.sub-001.sublabel-HEADER0001.rtf",
                    @"001\BatchWithPipeInContent.txt.line-000032.label-1185038.sub-002.sublabel-DETAIL0001.rtf",
                    @"001\BatchWithPipeInContent.txt.line-000032.label-1185038.sub-003.sublabel-Extract_No_Label.rtf",
                    @"001\BatchWithSimpleContent.txt.line-000002.label-1116458.sub-001.sublabel-HEADER0001.rtf",
                    @"001\BatchWithSimpleContent.txt.line-000002.label-1116458.sub-002.sublabel-DETAIL0001.rtf",
                    @"001\BatchWithSimpleContent.txt.line-000002.label-1116458.sub-003.sublabel-Extract_No_Label.rtf",
                    @"001\BatchWithSimpleContent.txt.line-000032.label-1086935.sub-001.sublabel-HEADER0001.rtf",
                    @"001\BatchWithSimpleContent.txt.line-000032.label-1086935.sub-002.sublabel-DETAIL0001.rtf",
                    @"001\BatchWithSimpleContent.txt.line-000032.label-1086935.sub-003.sublabel-Extract_No_Label.rtf",
                };

                var outputFiles = Directory.GetFiles(_outputFolders.Last(), "*.*", SearchOption.AllDirectories)
                    .Select(file => Path.Combine(Path.GetFileName(Path.GetDirectoryName(file)), Path.GetFileName(file)))
                    .ToList();

                CollectionAssert.AreEqual(expectedFiles, outputFiles);
            }
            finally
            {
                _testDbManager.RemoveDatabase(DB_NAME);
            }
        }

        /// <summary>
        /// Tests using ProcessFile method to update the input
        /// </summary>
        [Test]
        public static void ProcessFile_Update()
        {
            const string DB_NAME = "354C291E-C4FC-400E-8B8E-20234AEC4FB9";
            var db = _testDbManager.GetNewDatabase(DB_NAME);
            try
            {
                db.DefineNewAction("a");

                string[] inputFiles = new string[3];
                inputFiles[0] = _testFiles.GetFile("Resources.BatchWithSimpleContent.txt");
                inputFiles[1] = _testFiles.GetFile("Resources.BatchWithPipeInContent.txt");
                inputFiles[2] = _testFiles.GetFile("Resources.BatchWithInvalidContent.txt");

                List<FileRecord> records = QueueFiles(db, inputFiles);

                _outputFolders.Add(FileSystemMethods.GetTemporaryFolderName());

                var task = new ProcessRichTextBatchesTask
                {
                    DivideBatchIntoRichTextFiles = true,
                    OutputDirectory = _outputFolders.Last()
                };
                task.Init(1, null, db, null);

                db.RecordFAMSessionStart("DUMMY", "a", true, true);
                foreach (var fileRecord in records)
                {
                    task.ProcessFile(fileRecord, 1, null, db, null, false);
                }

                var outputFiles = Directory.GetFiles(_outputFolders.Last(), "*.rtf", SearchOption.AllDirectories);

                var johnDoeCounts = outputFiles.Select(f => Regex.Matches(File.ReadAllText(f), @"(?inx)\bDoe\b").Count).Sum();
                Assert.AreEqual(32, johnDoeCounts);

                // Redact "Doe"
                foreach (var file in outputFiles)
                {
                    var text = File.ReadAllText(file, _encoding);
                    text = Regex.Replace(text, @"(?inx)\bDoe\b", "Joe");

                    var redactedFile = Path.ChangeExtension(file, ".redacted.rtf");
                    File.WriteAllText(redactedFile, text, _encoding);
                }

                task.DivideBatchIntoRichTextFiles = false;
                task.RedactedFile = "$InsertBeforeExt(<SourceDocName>,.redacted)";
                task.UpdatedBatchFile = _outputFolders.Last() + @"\$FileOf(<SourceDocName>).updated";

                foreach (var fileRecord in records)
                {
                    task.ProcessFile(fileRecord, 1, null, db, null, false);
                }

                var redactedBatches = Directory.GetFiles(_outputFolders.Last(), "*.updated");
                var redactedJohnDoeCounts = redactedBatches.Select(f => Regex.Matches(File.ReadAllText(f), @"(?inx)\bDoe\b").Count).Sum();
                var redactedJohnJoeCounts = redactedBatches.Select(f => Regex.Matches(File.ReadAllText(f), @"(?inx)\bJoe\b").Count).Sum();

                Assert.AreEqual(0, redactedJohnDoeCounts);
                Assert.AreEqual(johnDoeCounts, redactedJohnJoeCounts);

                for (int i = 0; i < inputFiles.Length; i++)
                {
                    var inputText = File.ReadAllText(inputFiles[i], _encoding);
                    var inputTextReplaced = Regex.Replace(inputText, @"(?inx)\bDoe\b", "Joe");

                    var redactedBatch = Path.Combine(_outputFolders.Last(), Path.GetFileName(inputFiles[i]) + ".updated");
                    var redactedText = File.ReadAllText(redactedBatch, _encoding);

                    Assert.AreEqual(inputTextReplaced, redactedText);
                }
            }
            finally
            {
                _testDbManager.RemoveDatabase(DB_NAME);
            }
        }

        /// <summary>
        /// Tests that RTF files must be complete before update will work
        /// </summary>
        [Test]
        public static void ProcessFile_NoWorkflow_DoNotUpdateIfNotComplete()
        {
            const string DB_NAME = "BD05D688-E9AE-42F2-85B0-E1647E1B6193";
            var db = _testDbManager.GetNewDatabase(DB_NAME);
            try
            {
                db.DefineNewAction("a");
                db.DefineNewAction("b");
                db.DefineNewAction("c");

                string inputFile = _testFiles.GetFile("Resources.BatchWithSimpleContent.txt");

                FileRecord fileRecord = QueueFiles(db, new[] { inputFile })[0];

                _outputFolders.Add(FileSystemMethods.GetTemporaryFolderName());

                // Divide
                var task = new ProcessRichTextBatchesTask
                {
                    DivideBatchIntoRichTextFiles = true,
                    OutputDirectory = _outputFolders.Last(),
                    OutputAction = "b"
                };
                task.Init(1, null, db, null);

                db.RecordFAMSessionStart("DUMMY", "a", true, true);
                task.ProcessFile(fileRecord, 1, null, db, null, false);

                // Attempt to update when output files are unattempted
                task.DivideBatchIntoRichTextFiles = false;
                task.UpdatedBatchFile = _outputFolders.Last() + @"\$FileOf(<SourceDocName>).updated";
                task.RedactedAction = "c";

                task.ProcessFile(fileRecord, 1, null, db, null, false);
                var status = db.GetFileStatus(fileRecord.FileID, "a", false);

                // File gets set to skipped
                Assert.AreEqual(EActionStatus.kActionSkipped, status);

                // Updated file not created
                var updatedFile = Path.Combine(_outputFolders.Last(), Path.GetFileName(fileRecord.Name) + ".updated");
                Assert.That(!File.Exists(updatedFile));

                // Reset file to pending so the after status can be measured again
                db.SetFileStatusToPending(fileRecord.FileID, "a", false);

                // Attempt to update when output files are pending
                task.RedactedAction = "b";

                task.ProcessFile(fileRecord, 1, null, db, null, false);
                status = db.GetFileStatus(fileRecord.FileID, "a", false);
                Assert.AreEqual(EActionStatus.kActionSkipped, status);
                Assert.That(!File.Exists(updatedFile));

                // Reset file to pending so the after status can be measured again
                db.SetFileStatusToPending(fileRecord.FileID, "a", false);

                // Set files to failed and confirm that the file now fails
                var outputFiles = Directory.GetFiles(_outputFolders.Last(), "*.rtf", SearchOption.AllDirectories);
                for (int i = 0; i < outputFiles.Length; i++)
                {
                    db.SetStatusForFile(i + 2, "b", -1, EActionStatus.kActionFailed, false, false, out _);
                }
                task.ProcessFile(fileRecord, 1, null, db, null, false);
                status = db.GetFileStatus(fileRecord.FileID, "a", false);
                Assert.AreEqual(EActionStatus.kActionFailed, status);
                Assert.That(!File.Exists(updatedFile));


                // Reset file to pending so the after status can be measured again
                db.SetFileStatusToPending(fileRecord.FileID, "a", false);

                // Set all the output files to complete and confirm that the batch will now update
                for (int i = 0; i < outputFiles.Length; i++)
                {
                    db.SetStatusForFile(i + 2, "b", -1, EActionStatus.kActionCompleted, false, false, out _);
                }
                task.ProcessFile(fileRecord, 1, null, db, null, false);
                status = db.GetFileStatus(fileRecord.FileID, "a", false);
                Assert.AreEqual(EActionStatus.kActionPending, status); // Tasks don't set files to complete so assert that the state is still pending
                Assert.That(File.Exists(updatedFile));
            }
            finally
            {
                _testDbManager.RemoveDatabase(DB_NAME);
            }
        }

        /// <summary>
        /// Tests that RTF files must be complete before update will work
        /// </summary>
        [Test]
        public static void ProcessFile_Workflow_DoNotUpdateIfNotComplete()
        {
            const string DB_NAME = "2F4B0F3C-D03C-42A4-A6AF-DE25D4D56A01";
            var db = _testDbManager.GetNewDatabase(DB_NAME);
            try
            {
                var actions = new[] { "a", "b", "c" };
                var wfActions = actions.Select(a =>
                {
                    db.DefineNewAction(a);
                    var vv = new VariantVectorClass();
                    vv.PushBack(a);
                    vv.PushBack(true);
                    return vv;
                }).ToIUnknownVector();

                var wfID = db.AddWorkflow("Workflow1", EWorkflowType.kUndefined);
                db.SetWorkflowActions(wfID, wfActions);
                WorkflowDefinition workflowDefinition = new WorkflowDefinition
                {
                    ID = wfID,
                    Name = "Workflow1",
                    Type = EWorkflowType.kUndefined,
                    StartAction = "a",
                    EditAction = "b",
                    EndAction = "c"
                };
                db.SetWorkflowDefinition(workflowDefinition);

                // Set the active workflow
                db.ActiveWorkflow = workflowDefinition.Name;
                var actionIDofA = db.GetActionIDForWorkflow("a", wfID);

                string inputFile = _testFiles.GetFile("Resources.BatchWithSimpleContent.txt");

                FileRecord fileRecord = QueueFiles(db, new[] { inputFile }, wfID)[0];

                _outputFolders.Add(FileSystemMethods.GetTemporaryFolderName());

                // Divide
                var task = new ProcessRichTextBatchesTask
                {
                    DivideBatchIntoRichTextFiles = true,
                    OutputDirectory = _outputFolders.Last(),
                    OutputAction = "b"
                };
                task.Init(1, null, db, null);

                db.RecordFAMSessionStart("DUMMY", "a", true, true);
                task.ProcessFile(fileRecord, 1, null, db, null, false);

                // Attempt to update when output files are unattempted
                task.DivideBatchIntoRichTextFiles = false;
                task.RedactedAction = "c";
                task.UpdatedBatchFile = _outputFolders.Last() + @"\$FileOf(<SourceDocName>).updated";

                task.ProcessFile(fileRecord, actionIDofA, null, db, null, false);
                var status = db.GetFileStatus(fileRecord.FileID, "a", false);

                // File gets set to skipped
                Assert.AreEqual(EActionStatus.kActionSkipped, status);

                // No updated file is created
                var updatedFile = Path.Combine(_outputFolders.Last(), Path.GetFileName(fileRecord.Name) + ".updated");
                Assert.That(!File.Exists(updatedFile));

                // Reset file to pending so the after-status can be measured again
                db.SetFileStatusToPending(fileRecord.FileID, "a", false);

                // Attempt to update when output files are pending
                task.RedactedAction = "b";
                var outputFiles = Directory.GetFiles(_outputFolders.Last(), "*.rtf", SearchOption.AllDirectories);
                for (int i = 0; i < outputFiles.Length; i++)
                {
                    Assert.AreEqual(EActionStatus.kActionPending, db.GetFileStatus(i + 2, "b", false));
                }

                task.ProcessFile(fileRecord, actionIDofA, null, db, null, false);
                status = db.GetFileStatus(fileRecord.FileID, "a", false);
                Assert.AreEqual(EActionStatus.kActionSkipped, status);
                Assert.That(!File.Exists(updatedFile));

                // Reset file to pending so the after status can be measured again
                db.SetFileStatusToPending(fileRecord.FileID, "a", false);

                // Set files to failed and confirm that the file now fails
                for (int i = 0; i < outputFiles.Length; i++)
                {
                    db.SetStatusForFile(i + 2, "b", wfID, EActionStatus.kActionFailed, false, false, out _);
                }
                task.ProcessFile(fileRecord, actionIDofA, null, db, null, false);
                status = db.GetFileStatus(fileRecord.FileID, "a", false);
                Assert.AreEqual(EActionStatus.kActionFailed, status);
                Assert.That(!File.Exists(updatedFile));

                // Reset file to pending so the after status can be measured again
                db.SetFileStatusToPending(fileRecord.FileID, "a", false);

                // Set files to complete and confirm that the file now processes
                for (int i = 0; i < outputFiles.Length; i++)
                {
                    db.SetStatusForFile(i + 2, "b", wfID, EActionStatus.kActionCompleted, false, false, out _);
                }
                task.ProcessFile(fileRecord, actionIDofA, null, db, null, false);
                status = db.GetFileStatus(fileRecord.FileID, "a", false);
                Assert.AreEqual(EActionStatus.kActionPending, status); // Tasks don't set files to complete so assert that the state is still pending
                Assert.That(File.Exists(updatedFile));
            }
            finally
            {
                _testDbManager.RemoveDatabase(DB_NAME);
            }
        }

        /// <summary>
        /// Tests the IMustBeConfigured interface
        /// </summary>
        [Test]
        public static void IMustBeConfiguredObject_Interface()
        {
            var task = new ProcessRichTextBatchesTask();
            var configured = (IMustBeConfiguredObject)task;
            Assert.That(configured.IsConfigured());

            task.OutputDirectory = "";
            Assert.That(!configured.IsConfigured());

            task.DivideBatchIntoRichTextFiles = false;
            Assert.That(!configured.IsConfigured());
            task.OutputDirectory = @"D:\temp";
            Assert.That(configured.IsConfigured());

            task.UpdatedBatchFile = "";
            Assert.That(!configured.IsConfigured());
            task.UpdatedBatchFile = "<SourceDocName>";
            Assert.That(configured.IsConfigured());

            task.RedactedFile = "";
            Assert.That(!configured.IsConfigured());
            task.RedactedFile = "<SourceDocName>";
            Assert.That(configured.IsConfigured());
        }

        #endregion Tests

        #region Helper Methods

        private static List<FileRecord> QueueFiles(FileProcessingDB db, string[] inputFiles, int workflowID = -1)
        {
            return inputFiles.Select(file =>
                db.AddFile(file, "a", workflowID, EFilePriority.kPriorityNormal, false, false, EActionStatus.kActionPending, false, out _, out _)
            ).ToList();
        }

        #endregion
    }
}
