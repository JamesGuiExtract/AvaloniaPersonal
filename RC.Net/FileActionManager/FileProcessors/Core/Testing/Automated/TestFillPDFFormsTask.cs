using Extract.FileActionManager.Database.Test;
using Extract.Interop;
using Extract.Testing.Utilities;
using NUnit.Framework;
using System.IO;
using UCLID_FILEPROCESSINGLib;

namespace Extract.FileActionManager.FileProcessors.Test
{
    [TestFixture]
    [Category("FillPdfFormsTask"), Category("Automated")]
    public class TestFillPdfFormsTask
    {
        const string basicPDFForm = "Resources.Form_Original2.pdf";

        TestFileManager<TestFillPdfFormsTask> _testFiles;
        FAMTestDBManager<TestFillPdfFormsTask> _testDbManager;

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
        public void TestSerialization()
        {
            // TODO: Fill in values to test serialization.
            // Arrange
            FillPdfFormsTask task = new()
            {
            };
            using var stream = new MemoryStream();

            // Act
            task.Save(new IStreamWrapper(stream), false);

            stream.Position = 0;
            FillPdfFormsTask loadedTask = new();
            loadedTask.Load(new IStreamWrapper(stream));

            // Assert
        }

        // Confirm that clone works correctly
        [Test]
        public void TestCopy()
        {
            // TODO: Add in values to test copy properly.

            // Arrange
            FillPdfFormsTask task = new()
            {
            };

            // Act
            FillPdfFormsTask clonedTask = (FillPdfFormsTask)task.Clone();

            // Assert
        }


        /// <summary>
        /// Ensures a PDF Form can be processed.
        /// </summary>
        [Test]
        public void ProcessFile_FillInForm()
        {
            // Arrange
            string databaseName = "Test ProcessFile_FillInForm";
            using var db = _testDbManager.GetDisposableDatabase(databaseName, actionNames: new[] { "a_input" });

            FileRecord record = db.FileProcessingDB.AddFile(_testFiles.GetFile(basicPDFForm), "a_input", -1, EFilePriority.kPriorityNormal, false, false, EActionStatus.kActionPending, false, out _, out _);


            FillPdfFormsTask task = new()
            {
            };
            task.Init(1, null, db.FileProcessingDB, null);

            // Act
            task.ProcessFile(record, 1, null, db.FileProcessingDB, null, false);

            // Assert
            Assert.True(File.Exists(record.Name + "filled.pdf"));
        }

        #endregion
    }
}
