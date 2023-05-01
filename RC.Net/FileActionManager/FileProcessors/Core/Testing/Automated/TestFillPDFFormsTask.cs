using Extract.FileActionManager.Database.Test;
using Extract.Interop;
using Extract.Testing.Utilities;
using J2N.Collections.Generic;
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
        const string randomPDF = "Resources.C413.pdf";

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
            // Arrange
            FillPdfFormsTask task = new()
            {
                FieldsToAutoFill = new Dictionary<string, string>() { { "Yes", "no" }, { "Hello", "Goodbye" } }
            };
            using var stream = new MemoryStream();

            // Act
            task.Save(new IStreamWrapper(stream), false);

            stream.Position = 0;
            FillPdfFormsTask loadedTask = new();
            loadedTask.Load(new IStreamWrapper(stream));

            // Assert
            Assert.AreEqual(task.FieldsToAutoFill, loadedTask.FieldsToAutoFill);
        }

        // Confirm that clone works correctly
        [Test]
        public void TestCopy()
        {
            // Arrange
            FillPdfFormsTask task = new()
            {
                FieldsToAutoFill = new Dictionary<string, string>() { { "Yes", "no" }, { "Hello", "Goodbye" } }
            };

            // Act
            FillPdfFormsTask clonedTask = (FillPdfFormsTask)task.Clone();

            // Assert
            Assert.AreEqual(task.FieldsToAutoFill, clonedTask.FieldsToAutoFill);
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
            string pdfForm = _testFiles.GetFile(basicPDFForm);
            FileRecord record = db.FileProcessingDB.AddFile(_testFiles.GetFile(randomPDF), "a_input", -1, EFilePriority.kPriorityNormal, false, false, EActionStatus.kActionPending, false, out _, out _);

            File.WriteAllText(Path.GetDirectoryName(record.Name) + "\\test.json", @"{""Given Name Text Box"": ""Test"", ""Family Name Text Box"": ""Yes""}");

            FillPdfFormsTask task = new()
            {
                FieldsToAutoFill = new Dictionary<string, string>() { { "Yes", "no" }, { "Hello", "Goodbye" } }
            };
            task.Init(1, null, db.FileProcessingDB, null);

            // Act
            task.ProcessFile(record, 1, null, db.FileProcessingDB, null, false);

            // Assert
        }

        #endregion
    }
}
