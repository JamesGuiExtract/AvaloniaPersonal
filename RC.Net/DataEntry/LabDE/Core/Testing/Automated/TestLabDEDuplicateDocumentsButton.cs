using Extract.FileActionManager.Database.Test;
using Extract.Imaging;
using Extract.Testing.Utilities;
using Extract.Utilities;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;

namespace Extract.DataEntry.LabDE.Test
{
    /// <summary>
    /// Class to test <see cref="DuplicateDocumentsButton"/>.
    /// </summary>
    [Category("TestLabDEDuplicateDocumentsButton")]
    [TestFixture]
    public class TestLabDEDuplicateDocumentsButton
    {
        #region Constants

        const string Demo_LabDE_With_Data_DB = "Resources.Demo_LabDE_WithData.bak";
        const string FILE_1_FILENAME = @"C:\Demo_LabDE\Input\F003.tif";
        const string FILE_2_FILENAME = @"C:\Demo_LabDE\Input\H350.tif";
        const string FILE_3_FILENAME = @"C:\Demo_LabDE\Input\J057.tif";
        const string VERIFY_ACTION = "A02_Verify";
        const string CLEANUP_ACTION = "Z_AdminAction";
        const string IGNORE_TAG = "User_IgnoreDocument";
        const string STAPLED_TAG = "User_MultiplePatients";
        const string STAPLED_INTO_METADATA_FIELD = "StapledInto";

        readonly string[] ORIGINAL_IMAGE_PATHS = new[]
        {
            FILE_1_FILENAME,
            FILE_2_FILENAME,
            FILE_3_FILENAME,
        };


        #endregion

        #region Fields

        /// <summary>
        /// Manages test FAM DBs
        /// </summary>
        static FAMTestDBManager<TestLabDEDuplicateDocumentsButton> _testDbManager;
        static TestFileManager<TestLabDEDuplicateDocumentsButton> _testFileManager;

        #endregion Fields

        #region Overhead

        /// <summary>
        /// Initializes the test fixture.
        /// </summary>
        [OneTimeSetUp]
        public static void Setup()
        {
            GeneralMethods.TestSetup();

            _testDbManager = new FAMTestDBManager<TestLabDEDuplicateDocumentsButton>();
            _testFileManager = new TestFileManager<TestLabDEDuplicateDocumentsButton>("A53F8802-DB9D-4B23-9865-D1A3DE33AB55");
        }

        /// <summary>
        /// Cleanup after all tests have run.
        /// </summary>
        [OneTimeTearDown]
        public static void FinalCleanup()
        {
            if (_testDbManager != null)
            {
                _testDbManager.Dispose();
                _testDbManager = null;
            }
            _testFileManager.Dispose();
        }

        #endregion Overhead

        #region Test Methods

        /// <summary>
        /// Tests basic behavior of the <see cref="DuplicateDocumentsButton"/> (including basic
        /// behavior of <see cref="DuplicateDocumentsFFIColumn"/>).
        /// </summary>
        [Test, Category("Automated")]
        public void DuplicateDocumentBasicsTest()
        {
            string testDBName = "Test_DuplicateDocumentBasicsTest";
            FileProcessingDB _famDB = null;

            try
            {
                // DB contains 3 files:
                // File ID 1: F003.tif, Initial status = Complete
                // File ID 2: H350.tif, Initial status = Pending
                // File ID 3: J057.tif, Initial status = Pending
                // Metadata for File ID 3:
                // PatientFirstName = "Bert"
                // PatientLastName = "Doe"
                // PatientDOB = "08/08/2000"
                // CollectionDate = "08/08/2008";
                _famDB = _testDbManager.GetDatabase(Demo_LabDE_With_Data_DB, testDBName);
                var updatedFilePaths = GetAndRenameFilesInDB(_famDB);
                using (var famSession = new FAMProcessingSession(_famDB, VERIFY_ACTION, "", new NullFileProcessingTask()))
                using (var testButton = InitializeDuplicateDocumentButton(famSession, updatedFilePaths[FILE_2_FILENAME]))
                using (testButton.DataEntryControlHost)
                {
                    // Confirm file ID 3 is pending for now.
                    var doc3Status = _famDB.GetFileStatus(3, VERIFY_ACTION, false);
                    Assert.AreEqual(EActionStatus.kActionPending, doc3Status);

                    var testFFIColumn = testButton.ActionColumn;

                    // Populate info matching ID 3
                    Assert.IsFalse(testButton.Flash);
                    testButton.FirstName = "Bert";
                    Assert.IsFalse(testButton.Flash);
                    testButton.LastName = "Doe";
                    Assert.IsFalse(testButton.Flash);
                    testButton.DOB = "08/08/2000";
                    Assert.IsFalse(testButton.Flash);
                    testButton.CollectionDate = "08/08/2008";

                    // Button should flash once all matching data is set.
                    Assert.IsTrue(testButton.Flash);

                    // AutoCheckoutDuplicateFiles was false, so file ID 3 should remain pending.
                    doc3Status = _famDB.GetFileStatus(3, VERIFY_ACTION, false);
                    Assert.AreEqual(EActionStatus.kActionPending, doc3Status);

                    using (var ffiForm = testButton.GetTestFAMFileInspector())
                    {
                        // File 2 was already processing via the FAMProcessingSession, file 3
                        // should list the status it was before the FFI was activated (pending)
                        Assert.AreEqual(EActionStatus.kActionProcessing, testFFIColumn.GetPreviousStatus(2));
                        Assert.AreEqual(EActionStatus.kActionPending, testFFIColumn.GetPreviousStatus(3));

                        // GetValue should have triggered file 3 to be pulled into processing status
                        doc3Status = _famDB.GetFileStatus(3, VERIFY_ACTION, false);
                        Assert.AreEqual(EActionStatus.kActionProcessing, doc3Status);
                    }
                }
            }
            finally
            {
                _famDB?.CloseAllDBConnections();
                _testDbManager.RemoveDatabase(testDBName);
            }
        }

        /// <summary>
        /// Tests that the button can handle one of multiple comma-delimited dates in the
        /// CollectionDate metadata field.
        /// </summary>
        [Test, Category("Automated")]
        public void DuplicateDocumentMultipleCollectionDatesTest()
        {
            string testDBName = "Test_DuplicateDocumentMultipleCollectionDatesTest";
            FileProcessingDB _famDB = null;

            try
            {
                // DB contains 3 files:
                // File ID 1: F003.tif, Initial status = Complete
                // File ID 2: H350.tif, Initial status = Pending
                // File ID 3: J057.tif, Initial status = Pending
                // Metadata for File ID 3:
                // PatientFirstName = "Bert"
                // PatientLastName = "Doe"
                // PatientDOB = "08/08/2000"
                // CollectionDate = "08/08/2008";
                _famDB = _testDbManager.GetDatabase(Demo_LabDE_With_Data_DB, testDBName);
                var updatedFilePaths = GetAndRenameFilesInDB(_famDB);
                using (var famSession = new FAMProcessingSession(_famDB, VERIFY_ACTION, "", new NullFileProcessingTask()))
                using (var testButton = InitializeDuplicateDocumentButton(famSession, updatedFilePaths[FILE_2_FILENAME]))
                using (testButton.DataEntryControlHost)
                {
                    // Populate info matching ID 3
                    Assert.IsFalse(testButton.Flash);
                    testButton.FirstName = "Bert";
                    Assert.IsFalse(testButton.Flash);
                    testButton.LastName = "Doe";
                    Assert.IsFalse(testButton.Flash);
                    testButton.DOB = "08/08/2000";
                    Assert.IsFalse(testButton.Flash);

                    // Middle date of several dates should be recognized as a matching date
                    testButton.CollectionDate = "08/07/2008,08/08/2008,08/10/2008";
                    Assert.IsTrue(testButton.Flash);

                    // First date of several dates should be recognized as a matching date
                    testButton.CollectionDate = "08/08/2008,08/10/2008";
                    Assert.IsTrue(testButton.Flash);

                    // Last date of several dates should be recognized as a matching date
                    testButton.CollectionDate = "08/07/2008,08/08/2008";
                    Assert.IsTrue(testButton.Flash);

                    // Matching date should be recognized only when proper comma delimiting is used.
                    testButton.CollectionDate = "08/07/200808/08/2008";
                    Assert.IsFalse(testButton.Flash);
                }
            }
            finally
            {
                _famDB?.CloseAllDBConnections();
                _testDbManager.RemoveDatabase(testDBName);
            }
        }

        /// <summary>
        /// Tests that <see cref="DuplicateDocumentsButton.AutoCheckoutDuplicateFiles"/> has intended
        /// effect of automatically checking out matching files.
        /// </summary>
        [Test, Category("Automated")]
        public void DuplicateDocumentAutoCheckoutTest()
        {
            string testDBName = "Test_DuplicateDocumentAutoCheckoutTest";
            FileProcessingDB _famDB = null;

            try
            {
                // DB contains 3 files:
                // File ID 1: F003.tif, Initial status = Complete
                // File ID 2: H350.tif, Initial status = Pending
                // File ID 3: J057.tif, Initial status = Pending
                // Metadata for File ID 3:
                // PatientFirstName = "Bert"
                // PatientLastName = "Doe"
                // PatientDOB = "08/08/2000"
                // CollectionDate = "08/08/2008";
                _famDB = _testDbManager.GetDatabase(Demo_LabDE_With_Data_DB, testDBName);
                var updatedFilePaths = GetAndRenameFilesInDB(_famDB);
                using (var dataEntryPanel = new DataEntryControlHost())
                using (var famSession = new FAMProcessingSession(_famDB, VERIFY_ACTION))
                {
                    var dataEntryApp = new BackgroundDataEntryApp(famSession.FileProcessingManager.FileProcessingMgmtRole.FPDB);
                    dataEntryApp.DatabaseActionName = VERIFY_ACTION;
                    dataEntryApp.FileRequestHandler = (IFileRequestHandler)famSession.FileProcessingManager.FileProcessingMgmtRole;

                    dataEntryPanel.DataEntryApplication = dataEntryApp;
                    DuplicateDocumentsButton testButton = new DuplicateDocumentsButton();
                    testButton.AutoCheckoutDuplicateFiles = true;
                    dataEntryPanel.Controls.Add(testButton);
                    testButton.DataEntryControlHost = dataEntryPanel;

                    dataEntryPanel.LoadData(new IUnknownVector(), updatedFilePaths[FILE_2_FILENAME],
                        forEditing: false, initialSelection: FieldSelection.DoNotReset);

                    var doc3Status = _famDB.GetFileStatus(3, VERIFY_ACTION, false);
                    Assert.AreEqual(EActionStatus.kActionPending, doc3Status);

                    // Populate info matching ID 3
                    Assert.IsFalse(testButton.Flash);
                    testButton.FirstName = "Bert";
                    testButton.LastName = "Doe";
                    testButton.DOB = "08/08/2000";
                    testButton.CollectionDate = "08/08/2008";
                    Assert.IsTrue(testButton.Flash);

                    // AutoCheckoutDuplicateFiles = true, so File ID 3 should now be checked-out (processing)
                    doc3Status = _famDB.GetFileStatus(3, VERIFY_ACTION, false);
                    Assert.AreEqual(EActionStatus.kActionProcessing, doc3Status);
                }
            }
            finally
            {
                _famDB?.CloseAllDBConnections();
                _testDbManager.RemoveDatabase(testDBName);
            }
        }

        /// <summary>
        /// Tests applying changes via the duplicate documents FFI to potential displayed documents
        /// using typical tag/action settings.
        /// NOTE: For the moment any tests that would result in any other file besides the original
        /// "current" file are not testable; to do so would require a way to integrate
        /// GetTestFAMFileInspector with an active data entry task.
        /// </summary>
        [Test, Category("Automated")]
        public void DuplicateDocumentActionsWithSettingsTest()
        {
            string testDBName = "Test_DuplicateDocumentActionsWithSettingsTest";
            FileProcessingDB _famDB = null;

            try
            {
                // DB contains 3 files:
                // File ID 1: F003.tif, Initial status = Complete
                // File ID 2: H350.tif, Initial status = Pending
                // File ID 3: J057.tif, Initial status = Pending
                // Metadata for File ID 3:
                // PatientFirstName = "Bert"
                // PatientLastName = "Doe"
                // PatientDOB = "08/08/2000"
                // CollectionDate = "08/08/2008";
                _famDB = _testDbManager.GetDatabase(Demo_LabDE_With_Data_DB, testDBName);
                var updatedFilePaths = GetAndRenameFilesInDB(_famDB);
                using (var famSession = new FAMProcessingSession(_famDB, VERIFY_ACTION, "", new NullFileProcessingTask()))
                using (var testButton = InitializeDuplicateDocumentButton(famSession, updatedFilePaths[FILE_2_FILENAME]))
                using (testButton.DataEntryControlHost)
                using (var stapledOutputTempFile = new TemporaryFile(".tif", false))
                {
                    // Confirm file ID 3 is pending for now.
                    var doc3Status = _famDB.GetFileStatus(3, VERIFY_ACTION, false);
                    Assert.AreEqual(EActionStatus.kActionPending, doc3Status, message: "Precondition not met: FileID 3 expected to be pending in the verify action");
                    Assert.IsFalse(DoesFileHaveTag(_famDB, 3, IGNORE_TAG), message: "Precondition not met: FileID 3 should not have an ignore tag");

                    // Confirm file IDs 1, 2, 3 are unattempted in the cleanup action.
                    var doc1CleanupStatus = _famDB.GetFileStatus(1, CLEANUP_ACTION, false);
                    Assert.AreEqual(EActionStatus.kActionUnattempted, doc1CleanupStatus,
                        message: "Precondition not met: FileID 1 expected to be unattempted in the cleanup action");
                    var doc2CleanupStatus = _famDB.GetFileStatus(2, CLEANUP_ACTION, false);
                    Assert.AreEqual(EActionStatus.kActionUnattempted, doc2CleanupStatus,
                        message: "Precondition not met: FileID 2 expected to be unattempted in the cleanup action");
                    var doc3CleanupStatus = _famDB.GetFileStatus(3, CLEANUP_ACTION, false);
                    Assert.AreEqual(EActionStatus.kActionUnattempted, doc3CleanupStatus,
                        message: "Precondition not met: FileID 3 expected to be unattempted in the cleanup action");

                    // Configure typical settings
                    var testFFIColumn = testButton.ActionColumn;
                    testButton.TagForIgnore = IGNORE_TAG;
                    testButton.TagForStaple = STAPLED_TAG;
                    testButton.StapledIntoMetadataFieldName = STAPLED_INTO_METADATA_FIELD;
                    testButton.StapledDocumentOutput = stapledOutputTempFile.FileName;
                    testButton.CleanupAction = CLEANUP_ACTION;

                    // Populate info matching ID 3
                    Assert.IsFalse(testButton.Flash, message: "Precondition not met: Button is expected to be not flashing");
                    testButton.FirstName = "Bert";
                    testButton.LastName = "Doe";
                    testButton.DOB = "08/08/2000";
                    testButton.CollectionDate = "08/08/2008";
                    Assert.IsTrue(testButton.Flash, message: "Postcondition not met: Button is expected to be flashing");

                    // [SkipOption]: Skip the file (no other metadata or action changes should occur)
                    using (var ffiFormSkip = testButton.GetTestFAMFileInspector())
                    {
                        CollectionAssert.AreEqual(new[] { 2, 3 }, ffiFormSkip.DisplayedFileIds, message: "Precondition not met for SkipOption");

                        // GetValue should have triggered file 3 to be pulled into processing status
                        doc3Status = _famDB.GetFileStatus(3, VERIFY_ACTION, false);
                        Assert.AreEqual(EActionStatus.kActionProcessing, doc3Status,
                            message: "SkipOption: FileID 3 is expected to be processing in the verify action");

                        testFFIColumn.SetValue(3, testFFIColumn.SkipOption.Action);
                        testFFIColumn.Apply();

                        doc3Status = _famDB.GetFileStatus(3, VERIFY_ACTION, false);
                        Assert.AreEqual(EActionStatus.kActionSkipped, doc3Status,
                            message: "SkipOption: FileID 3 is expected to be skipped in verify action");

                        doc2CleanupStatus = _famDB.GetFileStatus(2, CLEANUP_ACTION, false);
                        Assert.AreEqual(EActionStatus.kActionUnattempted, doc2CleanupStatus,
                            message: "SkipOption: FileID 2 is expected to be unattempted in cleanup action");
                        doc3CleanupStatus = _famDB.GetFileStatus(3, CLEANUP_ACTION, false);
                        Assert.AreEqual(EActionStatus.kActionUnattempted, doc3CleanupStatus,
                            message: "SkipOption: FileID 3 is expected to be unattempted in cleanup action");
                    }

                    // Okay to leave completed for VERIFY_ACTION to ensure it gets pulled back into processing.

                    // [IgnoreOption]: Complete files, apply IGNORE_TAG
                    using (var ffiFormIgnore = testButton.GetTestFAMFileInspector())
                    {
                        CollectionAssert.AreEqual(new[] { 2, 3 }, ffiFormIgnore.DisplayedFileIds, message: "Precondition not met for IgnoreOption");

                        // GetValue should have triggered file 3 to be pulled into processing status
                        doc3Status = _famDB.GetFileStatus(3, VERIFY_ACTION, false);
                        Assert.AreEqual(EActionStatus.kActionProcessing, doc3Status,
                            message: "IgnoreOption: FileID 3 is expected to be processing in the verify action");

                        // [IgnoreOption]: Complete file, apply IGNORE_TAG
                        testFFIColumn.SetValue(3, testFFIColumn.IgnoreOption.Action);
                        testFFIColumn.Apply();

                        doc3Status = _famDB.GetFileStatus(3, VERIFY_ACTION, false);
                        Assert.AreEqual(EActionStatus.kActionCompleted, doc3Status,
                            message: "IgnoreOption: FileID 3 is expected to be completed in the verify action");
                        Assert.IsTrue(DoesFileHaveTag(_famDB, 3, IGNORE_TAG),
                            message: "IgnoreOption: FileID 3 is expected to have the ignore tag set");

                        doc2CleanupStatus = _famDB.GetFileStatus(2, CLEANUP_ACTION, false);
                        Assert.AreEqual(EActionStatus.kActionUnattempted, doc2CleanupStatus,
                            message: "IgnoreOption: FileID 2 is expected to be unattempted in cleanup action");
                        doc3CleanupStatus = _famDB.GetFileStatus(3, CLEANUP_ACTION, false);
                        Assert.AreEqual(EActionStatus.kActionPending, doc3CleanupStatus,
                            message: "IgnoreOption: FileID 3 is expected to be pending in cleanup action");
                    }

                    // Reset cleanup for next test. Okay to leave file 3 completed for VERIFY_ACTION to ensure it gets
                    // pulled back into processing.
                    _famDB.SetFileStatusToPending(3, CLEANUP_ACTION, false);

                    // Update metadata for file 1 to trigger it to show as a duplicate of file 2 as well.
                    _famDB.SetMetadataFieldValue(1, "PatientFirstName", "Bert");
                    _famDB.SetMetadataFieldValue(1, "PatientLastName", "Doe");
                    _famDB.SetMetadataFieldValue(1, "PatientDOB", "08/08/2000");
                    _famDB.SetMetadataFieldValue(1, "CollectionDate", "08/08/2008");

                    // [StapleOption]: Create stapled output and confirm the correct number of pages.
                    // Source files should be completed and TagForStaple and StapledIntoMetadataFieldName
                    // should be applied.
                    using (var ffiFormStaple = testButton.GetTestFAMFileInspector())
                    {
                        CollectionAssert.AreEqual(new[] { 1, 2, 3 }, ffiFormStaple.DisplayedFileIds, message: "Precondition not met for StapleOption");

                        testFFIColumn.SetValue(1, testFFIColumn.StapleOption.Action);
                        testFFIColumn.SetValue(3, testFFIColumn.StapleOption.Action);
                        testFFIColumn.Apply();

                        int totalPages =
                            _famDB.GetFileRecord(updatedFilePaths[FILE_1_FILENAME], VERIFY_ACTION).Pages
                            + _famDB.GetFileRecord(updatedFilePaths[FILE_3_FILENAME], VERIFY_ACTION).Pages;
                        Assert.AreEqual(totalPages, NuanceImageMethods.GetPageCount(stapledOutputTempFile.FileName),
                            message: "Stapled output has the wrong number of pages");

                        var doc1Status = _famDB.GetFileStatus(1, VERIFY_ACTION, false);
                        Assert.AreEqual(EActionStatus.kActionCompleted, doc1Status,
                            message: "StapleOption: DocID 1 is expected to be completed in the verify action");
                        Assert.IsTrue(DoesFileHaveTag(_famDB, 1, STAPLED_TAG), message: "StapleOption: DocID 1 is expected to have the stapled tag");
                        Assert.AreEqual(stapledOutputTempFile.FileName,
                            _famDB.GetMetadataFieldValue(1, STAPLED_INTO_METADATA_FIELD),
                            message: "StapleOption: Stapled-into metadata field is expected to be set for DocID 1");

                        doc3Status = _famDB.GetFileStatus(3, VERIFY_ACTION, false);
                        Assert.AreEqual(EActionStatus.kActionCompleted, doc3Status,
                            message: "StapleOption: DocID 3 is expected to be completed in the verify action");
                        Assert.IsTrue(DoesFileHaveTag(_famDB, 3, STAPLED_TAG), message: "StapleOption: DocID 3 is expected to have the stapled tag");
                        Assert.AreEqual(stapledOutputTempFile.FileName,
                            _famDB.GetMetadataFieldValue(3, STAPLED_INTO_METADATA_FIELD),
                            message: "StapleOption: Stapled-into metadata field is expected to be set for DocID 3");

                        doc1CleanupStatus = _famDB.GetFileStatus(1, CLEANUP_ACTION, false);
                        Assert.AreEqual(EActionStatus.kActionPending, doc1CleanupStatus,
                            message: "StapleOption: DocID 1 is expected to be pending in the cleanup action");
                        doc2CleanupStatus = _famDB.GetFileStatus(2, CLEANUP_ACTION, false);
                        Assert.AreEqual(EActionStatus.kActionUnattempted, doc2CleanupStatus,
                            message: "StapleOption: DocID 2 is expected to be unattempted in the cleanup action");
                        doc2CleanupStatus = _famDB.GetFileStatus(3, CLEANUP_ACTION, false);
                        Assert.AreEqual(EActionStatus.kActionPending, doc3CleanupStatus,
                            message: "StapleOption: DocID 3 is expected to be pending in the cleanup action");
                    }
                }
            }
            finally
            {
                _famDB?.CloseAllDBConnections();
                _testDbManager.RemoveDatabase(testDBName);
            }
        }

        /// <summary>
        /// Tests applying changes via the duplicate documents FFI to potential displayed documents
        /// using without tag/action settings (confirm basic operations occur as expected without
        /// applying changes to actions/tags)
        /// NOTE: For the moment any tests that would result in any other file besides the original
        /// "current" file are not testable; to do so would require a way to integrate
        /// GetTestFAMFileInspector with an active data entry task.
        /// </summary>
        [Test, Category("Automated")]
        public void DuplicateDocumentActionsWithoutSettingsTest()
        {
            string testDBName = "Test_DuplicateDocumentActionsWithoutSettingsTest";
            FileProcessingDB _famDB = null;

            try
            {
                // DB contains 3 files:
                // File ID 1: F003.tif, Initial status = Complete
                // File ID 2: H350.tif, Initial status = Pending
                // File ID 3: J057.tif, Initial status = Pending
                // Metadata for File ID 3:
                // PatientFirstName = "Bert"
                // PatientLastName = "Doe"
                // PatientDOB = "08/08/2000"
                // CollectionDate = "08/08/2008";
                _famDB = _testDbManager.GetDatabase(Demo_LabDE_With_Data_DB, testDBName);
                var updatedFilePaths = GetAndRenameFilesInDB(_famDB);
                using (var famSession = new FAMProcessingSession(_famDB, VERIFY_ACTION, "", new NullFileProcessingTask()))
                using (var testButton = InitializeDuplicateDocumentButton(famSession, updatedFilePaths[FILE_2_FILENAME]))
                using (testButton.DataEntryControlHost)
                using (var stapledOutputTempFile = new TemporaryFile(".tif", false))
                {
                    // Confirm file ID 3 is pending for now.
                    var doc3Status = _famDB.GetFileStatus(3, VERIFY_ACTION, false);
                    Assert.AreEqual(EActionStatus.kActionPending, doc3Status, message: "Precondition not met: FileID 3 expected to be pending in the verify action");
                    Assert.IsFalse(DoesFileHaveTag(_famDB, 3, IGNORE_TAG), message: "Precondition not met: FileID 3 should not have an ignore tag");

                    // Confirm file IDs 1, 2, 3 are unattempted in the cleanup action.
                    var doc1CleanupStatus = _famDB.GetFileStatus(1, CLEANUP_ACTION, false);
                    Assert.AreEqual(EActionStatus.kActionUnattempted, doc1CleanupStatus,
                        message: "Precondition not met: FileID 1 expected to be unattempted in the cleanup action");
                    var doc2CleanupStatus = _famDB.GetFileStatus(2, CLEANUP_ACTION, false);
                    Assert.AreEqual(EActionStatus.kActionUnattempted, doc2CleanupStatus,
                        message: "Precondition not met: FileID 2 expected to be unattempted in the cleanup action");
                    var doc3CleanupStatus = _famDB.GetFileStatus(3, CLEANUP_ACTION, false);
                    Assert.AreEqual(EActionStatus.kActionUnattempted, doc3CleanupStatus,
                        message: "Precondition not met: FileID 3 expected to be unattempted in the cleanup action");

                    // Omit typical settings (aside from StapledDocumentOutput)
                    var testFFIColumn = testButton.ActionColumn;
                    testButton.TagForIgnore = null;
                    testButton.TagForStaple = null;
                    testButton.StapledDocumentOutput = stapledOutputTempFile.FileName;
                    testButton.StapledIntoMetadataFieldName = null;
                    testButton.CleanupAction = null;

                    // Populate info matching ID 3
                    Assert.IsFalse(testButton.Flash, message: "Precondition not met: Button is expected to be not flashing");
                    testButton.FirstName = "Bert";
                    testButton.LastName = "Doe";
                    testButton.DOB = "08/08/2000";
                    testButton.CollectionDate = "08/08/2008";
                    Assert.IsTrue(testButton.Flash, message: "Postcondition not met: Button is expected to be flashing");

                    // [SkipOption]: Skip the file. (no other metadata or action changes should occur)
                    using (var ffiFormSkip = testButton.GetTestFAMFileInspector())
                    {
                        CollectionAssert.AreEqual(new[] { 2, 3 }, ffiFormSkip.DisplayedFileIds, message: "Precondition not met for SkipOption");

                        // GetValue should have triggered file 3 to be pulled into processing status
                        doc3Status = _famDB.GetFileStatus(3, VERIFY_ACTION, false);
                        Assert.AreEqual(EActionStatus.kActionProcessing, doc3Status,
                            message: "SkipOption: FileID 3 is expected to be processing in the verify action");

                        testFFIColumn.SetValue(3, testFFIColumn.SkipOption.Action);
                        testFFIColumn.Apply();

                        doc3Status = _famDB.GetFileStatus(3, VERIFY_ACTION, false);
                        Assert.AreEqual(EActionStatus.kActionSkipped, doc3Status,
                            message: "SkipOption: FileID 3 is expected to be skipped in verify action");

                        doc2CleanupStatus = _famDB.GetFileStatus(2, CLEANUP_ACTION, false);
                        Assert.AreEqual(EActionStatus.kActionUnattempted, doc2CleanupStatus,
                            message: "SkipOption: FileID 2 is expected to be unattempted in cleanup action");
                        doc3CleanupStatus = _famDB.GetFileStatus(3, CLEANUP_ACTION, false);
                        Assert.AreEqual(EActionStatus.kActionUnattempted, doc3CleanupStatus,
                            message: "SkipOption: FileID 3 is expected to be unattempted in cleanup action");
                    }

                    // [IgnoreOption]: Complete the file; TagForIgnore has not been specified and should not be applied.
                    using (var ffiFormIgnore = testButton.GetTestFAMFileInspector())
                    {
                        CollectionAssert.AreEqual(new[] { 2, 3 }, ffiFormIgnore.DisplayedFileIds, message: "Precondition not met for IgnoreOption");

                        // GetValue should have triggered file 3 to be pulled into processing status
                        doc3Status = _famDB.GetFileStatus(3, VERIFY_ACTION, false);
                        Assert.AreEqual(EActionStatus.kActionProcessing, doc3Status,
                            message: "IgnoreOption: FileID 3 is expected to be processing in the verify action");

                        // [IgnoreOption]: Complete file, apply IGNORE_TAG
                        testFFIColumn.SetValue(3, testFFIColumn.IgnoreOption.Action);
                        testFFIColumn.Apply();

                        doc3Status = _famDB.GetFileStatus(3, VERIFY_ACTION, false);
                        Assert.AreEqual(EActionStatus.kActionCompleted, doc3Status,
                            message: "IgnoreOption: FileID 3 is expected to be completed in the verify action");
                        Assert.IsFalse(DoesFileHaveTag(_famDB, 3, IGNORE_TAG),
                            message: "IgnoreOption: FileID 3 is not expected to have the ignore tag set");

                        doc2CleanupStatus = _famDB.GetFileStatus(2, CLEANUP_ACTION, false);
                        Assert.AreEqual(EActionStatus.kActionUnattempted, doc2CleanupStatus,
                            message: "IgnoreOption: FileID 2 is expected to be unattempted in cleanup action");
                        doc3CleanupStatus = _famDB.GetFileStatus(3, CLEANUP_ACTION, false);
                        Assert.AreEqual(EActionStatus.kActionUnattempted, doc3CleanupStatus,
                            message: "IgnoreOption: FileID 3 is expected to be unattempted in cleanup action");
                    }

                    // Update metadata for file 1 to trigger it to show as a duplicate of file 2 as well.
                    _famDB.SetMetadataFieldValue(1, "PatientFirstName", "Bert");
                    _famDB.SetMetadataFieldValue(1, "PatientLastName", "Doe");
                    _famDB.SetMetadataFieldValue(1, "PatientDOB", "08/08/2000");
                    _famDB.SetMetadataFieldValue(1, "CollectionDate", "08/08/2008");

                    // [StapleOption]: Create stapled output and confirm the correct number of pages.
                    // Source files should be completed. TagForStaple and StapledIntoMetadataFieldName
                    // are not specified and should not be applied.
                    using (var ffiFormStaple = testButton.GetTestFAMFileInspector())
                    {
                        CollectionAssert.AreEqual(new[] { 1, 2, 3 }, ffiFormStaple.DisplayedFileIds, message: "Precondition not met for StapleOption");

                        testFFIColumn.SetValue(1, testFFIColumn.StapleOption.Action);
                        testFFIColumn.SetValue(3, testFFIColumn.StapleOption.Action);
                        testFFIColumn.Apply();

                        int totalPages =
                            _famDB.GetFileRecord(updatedFilePaths[FILE_1_FILENAME], VERIFY_ACTION).Pages
                            + _famDB.GetFileRecord(updatedFilePaths[FILE_3_FILENAME], VERIFY_ACTION).Pages;
                        Assert.AreEqual(totalPages, NuanceImageMethods.GetPageCount(stapledOutputTempFile.FileName),
                            message: "Stapled output has the wrong number of pages");

                        var doc1Status = _famDB.GetFileStatus(1, VERIFY_ACTION, false);
                        Assert.AreEqual(EActionStatus.kActionCompleted, doc1Status,
                            message: "StapleOption: DocID 1 is expected to be completed in the verify action");
                        Assert.IsFalse(DoesFileHaveTag(_famDB, 1, STAPLED_TAG), message: "StapleOption: DocID 1 is not expected to have the stapled tag");
                        Assert.IsTrue(string.IsNullOrEmpty(_famDB.GetMetadataFieldValue(1, STAPLED_INTO_METADATA_FIELD)),
                            message: "StapleOption: Stapled-into metadata field is not expected to be set for DocID 1");

                        doc3Status = _famDB.GetFileStatus(3, VERIFY_ACTION, false);
                        Assert.AreEqual(EActionStatus.kActionCompleted, doc3Status,
                            message: "StapleOption: DocID 3 is expected to be completed in the verify action");
                        Assert.IsFalse(DoesFileHaveTag(_famDB, 3, STAPLED_TAG), message: "StapleOption: DocID 3 is not expected to have the stapled tag");
                        Assert.IsTrue(string.IsNullOrEmpty(_famDB.GetMetadataFieldValue(3, STAPLED_INTO_METADATA_FIELD)),
                            message: "StapleOption: Stapled-into metadata field is not expected to be set for DocID 3");

                        doc1CleanupStatus = _famDB.GetFileStatus(1, CLEANUP_ACTION, false);
                        Assert.AreEqual(EActionStatus.kActionUnattempted, doc1CleanupStatus,
                            message: "StapleOption: DocID 1 is expected to be unattempted in the cleanup action");
                        doc2CleanupStatus = _famDB.GetFileStatus(2, CLEANUP_ACTION, false);
                        Assert.AreEqual(EActionStatus.kActionUnattempted, doc2CleanupStatus,
                            message: "StapleOption: DocID 2 is expected to be unattempted in the cleanup action");
                        doc2CleanupStatus = _famDB.GetFileStatus(3, CLEANUP_ACTION, false);
                        Assert.AreEqual(EActionStatus.kActionUnattempted, doc3CleanupStatus,
                            message: "StapleOption: DocID 3 is expected to be unattempted in the cleanup action");
                    }
                }
            }
            finally
            {
                _famDB?.CloseAllDBConnections();
                _famDB = null;
                _testDbManager.RemoveDatabase(testDBName);
            }
        }

        #endregion Test Methods

        #region Helper Methods

        /// <summary>
        /// Creates and initializes a <see cref="DuplicateDocumentsButton"/> for use against the specified
        /// <see cref="FAMProcessingSession"/> and <see cref="fileName"/>.
        /// NOTE: Both the <see cref="DuplicateDocumentsButton"/> returned and its DataEntryControlHost member
        /// are disposable and should be disposed of when done.
        /// </summary>
        DuplicateDocumentsButton InitializeDuplicateDocumentButton(FAMProcessingSession famSession, string fileName)
        {
            var dataEntryPanel = new DataEntryControlHost();
            var dataEntryApp = new BackgroundDataEntryApp(famSession.FileProcessingManager.FileProcessingMgmtRole.FPDB);
            dataEntryApp.DatabaseActionName = VERIFY_ACTION;
            dataEntryApp.FileRequestHandler = (IFileRequestHandler)famSession.FileProcessingManager.FileProcessingMgmtRole;

            dataEntryPanel.DataEntryApplication = dataEntryApp;
            DuplicateDocumentsButton testButton = new DuplicateDocumentsButton();
            testButton.AutoCheckoutDuplicateFiles = false;
            dataEntryPanel.Controls.Add(testButton);
            testButton.DataEntryControlHost = dataEntryPanel;

            dataEntryPanel.LoadData(new IUnknownVector(), fileName,
                forEditing: false, initialSelection: FieldSelection.DoNotReset);

            return testButton;
        }

        /// <summary>
        /// Tests if the specified tag has been applied to the specified file.
        /// </summary>
        /// <returns></returns>
        bool DoesFileHaveTag(FileProcessingDB famDB, int fileID, string tagName)
        {
            var fileTags = famDB.GetTagsOnFile(fileID).ToIEnumerable<string>();
            return fileTags.Contains(tagName);
        }

        Dictionary<string, string> GetAndRenameFilesInDB(FileProcessingDB famDB)
        {
            return ORIGINAL_IMAGE_PATHS
                .Select(origPath =>
                {
                    var fileRecord = famDB.GetFileRecord(origPath, VERIFY_ACTION);
                    var resourceName = "Resources.DemoImages." + Path.GetFileName(origPath);
                    var newPath = _testFileManager.GetFile(resourceName);
                    famDB.RenameFile(fileRecord, newPath);
                    return (origPath, newPath);
                })
                .ToDictionary(t => t.origPath, t => t.newPath);
        }

        #endregion Helper Methods
    }
}
