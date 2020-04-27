using ADODB;
using Extract.FileActionManager.Database.Test;
using Extract.Testing.Utilities;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;

namespace Extract.DataEntry.LabDE.Test
{
    /// <summary>
    /// Class to test <see cref="Extract.DataEntry.LabDE.DuplicateDocumentsButton"/>.
    /// </summary>
    [Category("TestLabDEDuplicateDocumentsButton")]
    [TestFixture]
    public class TestLabDEDuplicateDocumentsButton
    {
        #region Constants

        const string Demo_LabDE_With_Data_DB = "Resources.Demo_LabDE_WithData.bak";
        const string VERIFY_ACTION = "A02_Verify";
        const string FILE_2_FILENAME = @"C:\Demo_LabDE\Input\H350.tif";

        #endregion

        #region Fields

        /// <summary>
        /// Manages test FAM DBs
        /// </summary>
        static FAMTestDBManager<TestLabDEDuplicateDocumentsButton> _testDbManager;

        #endregion Fields

        #region Overhead

        /// <summary>
        /// Initializes the test fixture.
        /// </summary>
        [TestFixtureSetUp]
        public static void Setup()
        {
            GeneralMethods.TestSetup();

            _testDbManager = new FAMTestDBManager<TestLabDEDuplicateDocumentsButton>();
        }

        /// <summary>
        /// Cleanup after all tests have run.
        /// </summary>
        [TestFixtureTearDown]
        public static void FinalCleanup()
        {
            if (_testDbManager != null)
            {
                _testDbManager.Dispose();
                _testDbManager = null;
            }
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

                using (var dataEntryPanel = new DataEntryControlHost())
                using (var famSession = new FAMProcessingSession(_famDB, VERIFY_ACTION, "", new NullFileProcessingTask()))
                {
                    var dataEntryApp = new BackgroundDataEntryApp(famSession.FileProcessingManager.FileProcessingMgmtRole.FPDB);
                    dataEntryApp.DatabaseActionName = VERIFY_ACTION;
                    dataEntryApp.FileRequestHandler = (IFileRequestHandler)famSession.FileProcessingManager.FileProcessingMgmtRole;

                    dataEntryPanel.DataEntryApplication = dataEntryApp;
                    DuplicateDocumentsButton testButton = new DuplicateDocumentsButton();
                    testButton.AutoCheckoutDuplicateFiles = false;
                    dataEntryPanel.Controls.Add(testButton);
                    testButton.DataEntryControlHost = dataEntryPanel;

                    dataEntryPanel.LoadData(new IUnknownVector(), FILE_2_FILENAME, false);

                    // Confirm file ID 3 is pending for now.
                    var doc3Status = _famDB.GetFileStatus(3, VERIFY_ACTION, false);
                    Assert.AreEqual(EActionStatus.kActionPending, doc3Status);

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

                    // Simulate opening of FFI
                    var testFFIColumn = new DuplicateDocumentsFFIColumn();
                    testFFIColumn.Initialize(dataEntryApp);
                    testFFIColumn.GetValue(2);
                    testFFIColumn.GetValue(3);

                    // File 2 was already processing via the FAMProcessingSession, file 3
                    // should list the status it was before the FFI was activated (pending)
                    Assert.AreEqual(EActionStatus.kActionProcessing, testFFIColumn.GetPreviousStatus(2));
                    Assert.AreEqual(EActionStatus.kActionPending, testFFIColumn.GetPreviousStatus(3));

                    // GetValue should have triggered file 3 to be pulled into processing status
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

                using (var dataEntryPanel = new DataEntryControlHost())
                using (var famSession = new FAMProcessingSession(_famDB, VERIFY_ACTION))
                {
                    var dataEntryApp = new BackgroundDataEntryApp(famSession.FileProcessingManager.FileProcessingMgmtRole.FPDB);
                    dataEntryApp.DatabaseActionName = VERIFY_ACTION;
                    dataEntryApp.FileRequestHandler = (IFileRequestHandler)famSession.FileProcessingManager.FileProcessingMgmtRole;

                    dataEntryPanel.DataEntryApplication = dataEntryApp;
                    DuplicateDocumentsButton testButton = new DuplicateDocumentsButton();
                    testButton.AutoCheckoutDuplicateFiles = false;

                    dataEntryPanel.Controls.Add(testButton);
                    testButton.DataEntryControlHost = dataEntryPanel;
                    dataEntryPanel.LoadData(new IUnknownVector(), FILE_2_FILENAME, false);

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

                var cancellationTokenSource = new CancellationTokenSource();
                var nullTask = new NullFileProcessingTask();

                using (var dataEntryPanel = new DataEntryControlHost())
                using (var famSession = new FAMProcessingSession(_famDB, VERIFY_ACTION, "", nullTask))
                {
                    var dataEntryApp = new BackgroundDataEntryApp(famSession.FileProcessingManager.FileProcessingMgmtRole.FPDB);
                    dataEntryApp.DatabaseActionName = VERIFY_ACTION;
                    dataEntryApp.FileRequestHandler = (IFileRequestHandler)famSession.FileProcessingManager.FileProcessingMgmtRole;

                    dataEntryPanel.DataEntryApplication = dataEntryApp;
                    DuplicateDocumentsButton testButton = new DuplicateDocumentsButton();
                    testButton.AutoCheckoutDuplicateFiles = true;
                    dataEntryPanel.Controls.Add(testButton);
                    testButton.DataEntryControlHost = dataEntryPanel;

                    dataEntryPanel.LoadData(new IUnknownVector(), FILE_2_FILENAME, false);

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

        #endregion Test Methods
    }
}
