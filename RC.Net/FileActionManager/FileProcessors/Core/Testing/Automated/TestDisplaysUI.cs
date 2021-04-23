using Extract.Interop;
using Extract.Testing.Utilities;
using Extract.Utilities;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;
using UCLID_FILEPROCESSORSLib;

namespace Extract.FileActionManager.FileProcessors.Test
{
    /// <summary>
    /// Provides unit test cases for the <see cref="ValidateXmlTask"/>.
    /// </summary>
    [TestFixture]
    [Category("DisplayUIPropertyForTasks")]
    public class TestDisplayUIPropertyForTasks
    {
        #region Constants

        /// <summary>
        /// A list of all the file processors, you can find your list at
        /// C:\ProgramData\Extract Systems\CategoryFiles\UCLID File Processors.lst
        /// </summary>
        private static readonly Dictionary<string,string> fileProcessors = new Dictionary<string, string>() {
            { "Core: Send email","Extract.FileActionManager.FileProcessors.SendEmailTask" },
            { "Core: Execute rules","AFFileProcessors.AFEngineFileProcessor.1" },
            { "Core: Clean up image", "FileProcessors.CleanupImageFileProcessor.1" },
            { "Redaction: Create redacted text", "Extract.Redaction.CreateRedactedTextTask" },
            { "Core: Convert to searchable PDF", "FileProcessors.ConvertToPDFTask.1" },
            { "Core: Convert VOA to XML", "AFFileProcessors.AFConvertVOAToXMLTask.1" },
            { "Core: Set Metadata", "Extract.FileActionManager.FileProcessors.SetMetadataTask" },
            { "Core: Validate XML", "Extract.FileActionManager.FileProcessors.ValidateXmlTask" },
            { "Core: Launch application", "FileProcessors.LaunchAppFileProcessor.1" },
            { "Core: Manage tags", "FileProcessors.ManageTagsTask.1" },
            { "Redaction: Create redacted image", "RedactionCustomComponents.RedactionTask.1" },
            { "Core: Set file priority", "Extract.FileActionManager.FileProcessors.SetFilePriorityTask" },
            { "Core: Modify source document name in database", "Extract.FileActionManager.FileProcessors.ModifySourceDocNameInDB" },
            { "Core: Create file", "Extract.FileActionManager.FileProcessors.CreateFileTask" },
            { "Redaction: Extend redactions to surround context", "Extract.Redaction.SurroundContextTask" },
            { "Redaction: Merge ID Shield data files", "Extract.Redaction.VOAFileMergeTask" },
            { "Data Entry: Verify extracted data", "Extract.DataEntry.Utilities.DataEntryApplication" },
            { "Core: OCR document with GCV", "Extract.FileActionManager.FileProcessors.CloudOCRTask" },
            { "Core: Archive or restore associated file", "FileProcessors.ArchiveRestoreTask.1" },
            { "Core: Modify PDF file", "Extract.FileActionManager.FileProcessors.ModifyPdfFileTask" },
            { "Core: Extract image area", "Extract.FileActionManager.FileProcessors.ExtractImageAreaTask" },
            { "Core: Encrypt/decrypt file", "Extract.FileActionManager.FileProcessors.EncryptDecryptFileTask" },
            { "RTF: Process batches", "Extract.FileActionManager.FileProcessors.ProcessRichTextBatchesTask" },
            { "Redaction: Create metadata XML", "Extract.Redaction.MetadataTask" },
            { "Redaction: Filter ID Shield data file", "RedactionCustomComponents.FilterIDShieldDataFileTask.1" },
            { "Core: Apply Bates number", "Extract.FileActionManager.FileProcessors.ApplyBatesNumberTask" },
            { "Core: Sleep", "FileProcessors.SleepTask.1" },
            { "Core: Transfer, rename or delete via FTP/SFTP", "Extract.FileActionManager.FileProcessors.FtpTask" },
            { "Core: Delete empty folder", "Extract.FileActionManager.FileProcessors.DeleteEmptyFolderTask" },
            { "Redaction: Verify sensitive data", "Extract.Redaction.Verification.VerificationTask" },
            { "Core: Store or retrieve attributes in database", "Extract.FileActionManager.FileProcessors.StoreAttributesInDbTask" },
            { "Pagination: Create output", "Extract.FileActionManager.CreatePaginatedOutputTask" },
            { "Core: View image", "Extract.FileActionManager.ViewImageTask" },
            { "Core: Rasterize PDF", "Extract.FileActionManager.FileProcessors.RasterizePdfTask" },
            { "Core: Set file-action status in database", "FileProcessors.SetActionStatusFileProcessor.1" },
            { "Core: Copy, move or delete file", "FileProcessors.CopyMoveDeleteFileProcessor.1" },
            { "Core: Enhance OCR", "AFFileProcessors.EnhanceOCRTask.1" },
            { "Pagination: Verify", "Extract.FileActionManager.PaginationTask" },
            { "Core: Add watermark", "FileProcessors.AddWatermarkTask.1" },
            { "Core: Split multi-page document", "Extract.FileActionManager.FileProcessors.SplitMultipageDocumentTask" },
            { "Core: OCR document", "FileProcessors.OCRFileProcessor.1" },
            { "Core: Conditionally execute task(s)", "FileProcessors.ConditionalTask.1" },
            { "Pagination: Auto-Paginate", "Extract.FileActionManager.AutoPaginateTask" },
            { "Core: Transform XML", "Extract.FileActionManager.FileProcessors.TransformXmlTask" },
            { "Core: Convert document", "Extract.FileConverter.ConverterFileProcessor" },
        };

        /// <summary>
        /// This list contains the text that will only be contained in descriptions of tasks
        /// that are Display a UI.
        /// </summary>
        static readonly List<string> _UI_SELECTION_LIST = new List<string> { "View", "Verify", "Paginate" };

        #endregion

        #region Overhead Methods

        /// <summary>
        /// Setup method to initialize the testing environment.
        /// </summary>
        [OneTimeSetUp]
        public static void Setup()
        {
            GeneralMethods.TestSetup();
        }

        /// <summary>
        /// Cleanup after all tests have run.
        /// </summary>
        [OneTimeTearDown]
        public static void FinalCleanup()
        {

        }

        #endregion Overhead Methods

        #region Unit Tests

        /// <summary>
        /// Tests that all the file processing tasks return appropriate values for DisplayUI
        /// </summary>
        [Test, Category("DisplayUI")]
        public static void TestDisplayUIProperty()
        {
            CategoryManager categoryManager = new CategoryManager();
            var fileProcessorsProgIDs = categoryManager.GetDescriptionToProgIDMap1(ExtractCategories.FileProcessorsName);
            var processorsProgIDs = fileProcessorsProgIDs.ComToDictionary();

            // Checks to see if any new licenses were created, if so it asks you to add them to this test.
            var licensedButNotInTest = processorsProgIDs.Keys.Except(fileProcessors.Keys).ToList();
            Assert.AreEqual(0, licensedButNotInTest.Count,
                "Please add the following lines to the fileProcessors dictionary in TestDisplaysUI.cs: "
                + String.Join("\r\n",
                    licensedButNotInTest.Select(description => "\"" + description + "\", \"" + processorsProgIDs[description] + "\"")));

            // Checks to see if any items are not licensed.
            var notLicensed = fileProcessors.Keys.Except(processorsProgIDs.Keys).ToList();
            Assert.AreEqual(0, notLicensed.Count,
                "You Need to license these (and/or fix dll registration) and then run 'check for new components' to update the cache file: "
                + String.Join("\r\n", notLicensed.Select(description => fileProcessors[description])));

            // Test the Tasks that have a UI           
            var uiTasks = processorsProgIDs.Where(t => _UI_SELECTION_LIST.Any(w => t.Key.Contains(w)));
            foreach (var k in uiTasks)
            {
                string progID = fileProcessorsProgIDs.GetValue(k.Key);
                Type t = Type.GetTypeFromProgID(progID);
                IFileProcessingTask task = Activator.CreateInstance(t) as IFileProcessingTask;

                Assert.IsNotNull(task, "Unable to create " + progID);
                Assert.That(task.DisplaysUI, "UI task has incorrect DisplaysUI value " + progID);
            }

            var nonUITasks = processorsProgIDs.Except(uiTasks);
            foreach (var k in nonUITasks)
            {
                string progID = fileProcessorsProgIDs.GetValue(k.Key);
                Type t = Type.GetTypeFromProgID(progID);
                IFileProcessingTask task = Activator.CreateInstance(t) as IFileProcessingTask;

                Assert.IsNotNull(task, "Unable to create " + progID);
                Assert.That(!task.DisplaysUI, "Non UI task has incorrect DisplaysUI value " + progID);
            }
        }

        /// <summary>
        /// Tests that the conditional task checks its true and false tasks when calling the DisplayUI property
        /// </summary>
        [Test, Category("DisplayUI")]
        public static void TestDisplayUIPropertyForConditionalTask()
        {
            ConditionalTask conditionalTask = new ConditionalTask();
            IFileProcessingTask task = conditionalTask as IFileProcessingTask;

            // Create an ObjectWithDiscription that has a UI object in it
            ObjectWithDescription UIObjectWithDescription = new ObjectWithDescription
            {
                Enabled = true,

                // Use the pagination task as the UI object to test
                Object = new PaginationTask()
            };

            // Create an ObjectWithdiscription that has a non UI object in it
            ObjectWithDescription NonUIObjectWithDiscription = new ObjectWithDescription
            {
                Enabled = true,

                // Use the copy move delete file processor as non UI object to test
                Object = new CopyMoveDeleteFileProcessor()
            };

            // Test true tasks with UI object
            conditionalTask.TasksForConditionTrue.PushBack(UIObjectWithDescription);

            Assert.That(!task.DisplaysUI, "Conditional task with UI object in True tasks should always return false.");

            // Test true tasks with non UI object
            conditionalTask.TasksForConditionTrue.Clear();
            conditionalTask.TasksForConditionTrue.PushBack(NonUIObjectWithDiscription);

            Assert.That(!task.DisplaysUI, "Conditional task with non UI object in True tasks");

            // Test false tasks with UI object
            conditionalTask.TasksForConditionTrue.Clear();
            conditionalTask.TasksForConditionFalse.Clear();
            conditionalTask.TasksForConditionFalse.PushBack(UIObjectWithDescription);

            Assert.That(!task.DisplaysUI, "Conditional task with UI object in False tasks should always return false");

            // Test false tasks with non UI object
            conditionalTask.TasksForConditionFalse.Clear();
            conditionalTask.TasksForConditionFalse.PushBack(NonUIObjectWithDiscription);

            Assert.That(!task.DisplaysUI, "Conditional task with non UI object in False tasks");
        }

        #endregion
    }
}