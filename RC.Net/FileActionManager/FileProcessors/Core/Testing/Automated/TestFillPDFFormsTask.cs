using Extract.FileActionManager.Database.Test;
using Extract.GdPicture;
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
        const string basicPDFFormVOA = "Resources.Form_Original2.pdf.voa";

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
            FileRecord record = db.FileProcessingDB.AddFile(_testFiles.GetFile(basicPDFForm), "a_input", -1, EFilePriority.kPriorityNormal, false, false, EActionStatus.kActionPending, false, out _, out _);

            FillPdfFormsTask task = new()
            {
                FieldsToAutoFill = new Dictionary<string, string>() { { "Yes", "no" }, { "Hello", "Goodbye" } }
            };
            task.Init(1, null, db.FileProcessingDB, null);

            // Act
            task.ProcessFile(record, 1, null, db.FileProcessingDB, null, false);
        }

        /// <summary>
        /// Ensures a PDF Form can be processed.
        /// </summary>
        [Test]
        public void ProcessFile_EnsureNewTextIsPresent()
        {
            // Arrange
            string databaseName = "Test ProcessFile_EnsureNewTextIsPresent";
            using var db = _testDbManager.GetDisposableDatabase(databaseName, actionNames: new[] { "a_input" });
            FileRecord record = db.FileProcessingDB.AddFile(_testFiles.GetFile(basicPDFForm), "a_input", -1, EFilePriority.kPriorityNormal, false, false, EActionStatus.kActionPending, false, out _, out _);

            string textToTest = "This really should not exist on a pdf form ;)";

            FillPdfFormsTask task = new()
            {
                FieldsToAutoFill = new Dictionary<string, string>() { { "Given Name Text Box", textToTest } }
            };
            task.Init(1, null, db.FileProcessingDB, null);

            // Act
            task.ProcessFile(record, 1, null, db.FileProcessingDB, null, false);

            using var pdf = new PDF(record.Name);

            var text = ImageUtils.GetText(pdf);

            pdf.Dispose();
            // Assert
            Assert.IsTrue(text.Contains(textToTest));
        }

        /// <summary>
        /// Test the ability to fill in metadata fields.
        /// </summary>
        [Test]
        public void ProcessFile_MetadataFill()
        {
            // Arrange
            string databaseName = "Test ProcessFile_MetadataFill";
            using var db = _testDbManager.GetDisposableDatabase(databaseName, actionNames: new[] { "a_input" });
            FileRecord record = db.FileProcessingDB.AddFile(_testFiles.GetFile(basicPDFForm), "a_input", -1, EFilePriority.kPriorityNormal, false, false, EActionStatus.kActionPending, false, out _, out _);

            string textToTest = "Can you rick roll a rick rolled roll?";

            db.FileProcessingDB.AddMetadataField("Memes");
            db.FileProcessingDB.SetMetadataFieldValue(1, "Memes", textToTest);

            FillPdfFormsTask task = new()
            {
                FieldsToAutoFill = new Dictionary<string, string>() { { "Given Name Text Box", "$Metadata(<SourceDocName>,Memes)" } }
            };
            task.Init(1, null, db.FileProcessingDB, null);

            // Act
            task.ProcessFile(record, 1, null, db.FileProcessingDB, null, false);

            using var pdf = new PDF(record.Name);

            var text = ImageUtils.GetText(pdf);

            pdf.Dispose();

            // Assert
            Assert.IsTrue(text.Contains(textToTest));
        }

        /// <summary>
        /// Test the ability to pull from a VOA file.
        /// </summary>
        [Test]
        public void ProcessFile_ValuesFromVOA()
        {
            // Arrange
            string databaseName = "Test ProcessFile_ValuesFromVOA";
            using var db = _testDbManager.GetDisposableDatabase(databaseName, actionNames: new[] { "a_input" });
            FileRecord record = db.FileProcessingDB.AddFile(_testFiles.GetFile(basicPDFForm), "a_input", -1, EFilePriority.kPriorityNormal, false, false, EActionStatus.kActionPending, false, out _, out _);

            _testFiles.GetFile(basicPDFFormVOA);

            FillPdfFormsTask task = new()
            {
                FieldsToAutoFill = new Dictionary<string, string>() { { "Given Name Text Box", "</TestAttribute>" } }
            };
            task.Init(1, null, db.FileProcessingDB, null);

            // Act
            task.ProcessFile(record, 1, null, db.FileProcessingDB, null, false);

            using var pdf = new PDF(record.Name);

            var text = ImageUtils.GetText(pdf);

            pdf.Dispose();

            // Assert
            Assert.IsTrue(text.Contains("Can you rick roll a rick rolled roll?"));
        }

        /// <summary>
        /// Test the ability to pull from a VOA file.
        /// </summary>
        [Test]
        public void ProcessFile_ValuesFromVOAStoredInDatabase()
        {
            // Arrange
            string databaseName = "Test ProcessFile_ValuesFromVOAStoredInDatabase";
            using var db = _testDbManager.GetDisposableDatabase(databaseName, actionNames: new[] { "a_input" });
            FileRecord record = db.FileProcessingDB.AddFile(_testFiles.GetFile(basicPDFForm), "a_input", -1, EFilePriority.kPriorityNormal, false, false, EActionStatus.kActionPending, false, out _, out _);
            db.FileProcessingDB.ExecuteCommandQuery("INSERT INTO dbo.AttributeSetName (Description) VALUES ('AttributesFoundByRules')");
            _testFiles.GetFile(basicPDFFormVOA);

            // Store the attribute set in the database.
            StoreAttributesInDBTask storeAttributesInDBTask = new()
            {
                StoreModeIsSet = true,
                VOAFileName = "<SourceDocName>.voa",
                AttributeSetName = "AttributesFoundByRules",
                StoreDiscreteData = true,
            };

            storeAttributesInDBTask.Init(1, null, db.FileProcessingDB, null);

            storeAttributesInDBTask.ProcessFile(record, 1, null, db.FileProcessingDB, null, false);

            db.FileProcessingDB.SetFileStatusToPending(1, "a_input", true);

            FillPdfFormsTask task = new()
            {
                FieldsToAutoFill = new Dictionary<string, string>() { { "Given Name Text Box", "$Attribute(<SourceDocName>,AttributesFoundByRules,TestAttribute)" } }
            };
            task.Init(1, null, db.FileProcessingDB, null);

            // Act
            task.ProcessFile(record, 1, null, db.FileProcessingDB, null, false);

            using var pdf = new PDF(record.Name);

            var text = ImageUtils.GetText(pdf);

            pdf.Dispose();

            // Assert
            Assert.IsTrue(text.Contains("Can you rick roll a rick rolled roll?"));
        }

        /// <summary>
        /// Ensures a PDF Form can be processed with multiple values.
        /// </summary>
        [Test]
        public void ProcessFile_TestMultipleValues()
        {
            // Arrange
            string databaseName = "Test ProcessFile_TestMultipleValues";
            using var db = _testDbManager.GetDisposableDatabase(databaseName, actionNames: new[] { "a_input" });
            FileRecord record = db.FileProcessingDB.AddFile(_testFiles.GetFile(basicPDFForm), "a_input", -1, EFilePriority.kPriorityNormal, false, false, EActionStatus.kActionPending, false, out _, out _);

            string textToTest = "This really should not exist on a pdf form ;)";
            string additionalTextToTest = "Beep boop, another text to test.";

            FillPdfFormsTask task = new()
            {
                FieldsToAutoFill = new Dictionary<string, string>() { { "Given Name Text Box", textToTest }, { "Family Name Text Box", additionalTextToTest } }
            };
            task.Init(1, null, db.FileProcessingDB, null);

            // Act
            task.ProcessFile(record, 1, null, db.FileProcessingDB, null, false);

            using var pdf = new PDF(record.Name);

            var text = ImageUtils.GetText(pdf);

            pdf.Dispose();
            // Assert
            Assert.IsTrue(text.Contains(textToTest));
            Assert.IsTrue(text.Contains(additionalTextToTest));
        }

        /// <summary>
        /// Test the ability to pull from a VOA file.
        /// This also ensures expand text does not throw errors from a already loaded voa file.
        /// </summary>
        [Test]
        public void ProcessFile_MultipleValuesFromVOA()
        {
            // Arrange
            string databaseName = "Test ProcessFile_MultipleValuesFromVOA";
            using var db = _testDbManager.GetDisposableDatabase(databaseName, actionNames: new[] { "a_input" });
            FileRecord record = db.FileProcessingDB.AddFile(_testFiles.GetFile(basicPDFForm), "a_input", -1, EFilePriority.kPriorityNormal, false, false, EActionStatus.kActionPending, false, out _, out _);

            _testFiles.GetFile(basicPDFFormVOA);

            FillPdfFormsTask task = new()
            {
                FieldsToAutoFill = new Dictionary<string, string>() { { "Given Name Text Box", "</TestAttribute>" }, { "Family Name Text Box", "</SecondTest>" } }
            };
            task.Init(1, null, db.FileProcessingDB, null);

            // Act
            task.ProcessFile(record, 1, null, db.FileProcessingDB, null, false);

            using var pdf = new PDF(record.Name);

            var text = ImageUtils.GetText(pdf);

            pdf.Dispose();

            // Assert
            Assert.IsTrue(text.Contains("Can you rick roll a rick rolled roll?"));
            Assert.IsTrue(text.Contains("Beep boop, another text to test."));
        }

        /// <summary>
        /// Test the ability to process a file with an invalid attribute
        /// </summary>
        [Test]
        public void ProcessFile_InvalidAttributeFromVOA()
        {
            // Arrange
            string databaseName = "Test ProcessFile_InvalidAttributeFromVOA";
            using var db = _testDbManager.GetDisposableDatabase(databaseName, actionNames: new[] { "a_input" });
            FileRecord record = db.FileProcessingDB.AddFile(_testFiles.GetFile(basicPDFForm), "a_input", -1, EFilePriority.kPriorityNormal, false, false, EActionStatus.kActionPending, false, out _, out _);

            _testFiles.GetFile(basicPDFFormVOA);

            FillPdfFormsTask task = new()
            {
                FieldsToAutoFill = new Dictionary<string, string>() { { "Given Name Text Box", "</NotAnAttribute>" } }
            };
            task.Init(1, null, db.FileProcessingDB, null);

            // Act
            task.ProcessFile(record, 1, null, db.FileProcessingDB, null, false);

            using var pdf = new PDF(record.Name);

            var text = ImageUtils.GetText(pdf);

            pdf.Dispose();

            // Assert
        }

        /// <summary>
        /// Test the ability to get text box form fields (others should be excluded).
        /// </summary>
        [Test]
        public void ProcessFile_GetPDFFormFields()
        {
            // Arrange
            using GdPictureUtility gdPictureUtilityForm = new();
            var testFile = _testFiles.GetFile(basicPDFForm);

            GdPictureUtility.ThrowIfStatusNotOK(gdPictureUtilityForm.PdfAPI.LoadFromFile(testFile, false),
                "ELI54300", "The PDF document can't be loaded", new(filePath: testFile));

            // Act

            var fields = FillPdfFormsTask.GetFormFieldValues(gdPictureUtilityForm.PdfAPI, testFile);

            // Assert

            // Note the name text box does not mean anything, its just the naming convention followed in this pdf.
            // What actually makes something a text box is determined by GDPictures API.
            Assert.IsTrue(fields.Count.Equals(8));
            Assert.IsTrue(fields.ContainsKey("Given Name Text Box"));
            Assert.IsTrue(fields.ContainsKey("Family Name Text Box"));
            Assert.IsTrue(fields.ContainsKey("House nr Text Box"));
            Assert.IsTrue(fields.ContainsKey("Address 2 Text Box"));
            Assert.IsTrue(fields.ContainsKey("Postcode Text Box"));
            Assert.IsTrue(fields.ContainsKey("Height Formatted Field"));
            Assert.IsTrue(fields.ContainsKey("City Text Box"));
            Assert.IsTrue(fields.ContainsKey("Address 1 Text Box"));
        }
        #endregion
    }
}
