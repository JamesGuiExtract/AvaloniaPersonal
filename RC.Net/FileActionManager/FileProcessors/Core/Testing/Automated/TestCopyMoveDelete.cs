using Extract.FileActionManager.Database.Test;
using Extract.Testing.Utilities;
using Extract.Utilities;
using NUnit.Framework;
using System.IO;
using System.IO.Compression;
using System.Linq;
using UCLID_FILEPROCESSINGLib;
using UCLID_FILEPROCESSORSLib;

namespace Extract.FileActionManager.FileProcessors.Test
{
    /// <summary>
    /// Provides unit test cases for the <see cref="ICopyMoveDeleteFileProcessor"/>.
    /// </summary>
    [TestFixture]
    [Category("TestCopyMoveDelete")]
    public class TestCopyMoveDelete
    {
        #region Constants

        static readonly string _FIRST_ACTION = "first";

        #endregion Constants

        #region Fields

        static TestFileManager<TestCopyMoveDelete> _testFiles;
        static FAMTestDBManager<TestCopyMoveDelete> _testDbManager;
        static string _testDirectory;
        static string _sourceFileDirectory;
        static string _basicTestSourceFileDirectory;
        static string _destFileDirectory;

        #endregion Fields

        #region Overhead Methods

        /// <summary>
        /// Setup method to initialize the testing environment.
        /// </summary>
        [TestFixtureSetUp]
        public static void Setup()
        {
            GeneralMethods.TestSetup();
            _testFiles = new TestFileManager<TestCopyMoveDelete>();
            _testDbManager = new FAMTestDBManager<TestCopyMoveDelete>();
        }

        [SetUp]
        public static void PerTestSetup()
        {
            string testFileZip = _testFiles.GetFile("Resources.CopyMoveDeleteFiles.zip");
            _testDirectory = FileSystemMethods.GetTemporaryFolderName(Path.GetDirectoryName(testFileZip));
            _sourceFileDirectory = Path.Combine(_testDirectory, "SourceFiles");
            _basicTestSourceFileDirectory = Path.Combine(_sourceFileDirectory, "BasicTests");
            _destFileDirectory = Path.Combine(_testDirectory, "DestFiles");
            ZipFile.ExtractToDirectory(testFileZip, _sourceFileDirectory);
        }

        [TearDown]
        public static void PerTestTeardown()
        {
            try
            {
                Directory.Delete(_testDirectory, recursive: true);
            }
            catch (DirectoryNotFoundException)
            { }
        }

        /// <summary>
        /// Cleanup after all tests have run.
        /// </summary>
        [TestFixtureTearDown]
        public static void FinalCleanup()
        {
            try
            {
                Directory.Delete(_testDirectory, recursive: true);
            }
            catch (DirectoryNotFoundException)
            { }

            if (_testFiles != null)
            {
                _testFiles.Dispose();
                _testFiles = null;
            }

            if (_testDbManager != null)
            {
                _testDbManager.Dispose();
                _testDbManager = null;
            }
        }

        #endregion Overhead Methods

        #region Tests

        /// <summary>
        /// Basic copy operation
        /// </summary>
        [Test]
        public static void TestCopyFile()
        {
            var copyMoveDeleteTask = new CopyMoveDeleteFileProcessorClass();
            copyMoveDeleteTask.SetCopyFiles(
                "<SourceDocName>"
                , $@"{_destFileDirectory}\$FileOf(<SourceDocName>)");

            var sourceDocName = Path.Combine(_basicTestSourceFileDirectory, "TestImage001.tif");
            var destFileName = Path.Combine(_destFileDirectory, Path.GetFileName(sourceDocName));
            copyMoveDeleteTask.Execute(sourceDocName);

            Assert.IsTrue(File.Exists(sourceDocName));
            Assert.IsTrue(File.Exists(destFileName));
            Assert.AreEqual(1, Directory.GetFiles(_destFileDirectory, "*").Length);
        }

        /// <summary>
        /// Test SourceMissingType settings for copy operations
        /// </summary>
        [Test]
        public static void TestCopyMissingFile()
        {
            var copyMoveDeleteTask = new CopyMoveDeleteFileProcessorClass();
            copyMoveDeleteTask.SetCopyFiles(
                "<SourceDocName>"
                , $@"{_destFileDirectory}\$FileOf(<SourceDocName>)");
            copyMoveDeleteTask.SourceMissingType = ECMDSourceMissingType.kCMDSourceMissingSkip;

            var sourceDocName = Path.Combine(_basicTestSourceFileDirectory, "TestImage001.missing.tif");
            var destFileName = Path.Combine(_destFileDirectory, Path.GetFileName(sourceDocName));
            copyMoveDeleteTask.Execute(sourceDocName);
            
            Assert.IsFalse(File.Exists(destFileName));

            copyMoveDeleteTask.SourceMissingType = ECMDSourceMissingType.kCMDSourceMissingError;

            Assert.Throws<ExtractException>(() => copyMoveDeleteTask.Execute(sourceDocName));
            Assert.IsFalse(File.Exists(destFileName));
        }

        /// <summary>
        /// Test DestinationPresentType settings for copy operations
        /// </summary>
        [Test]
        public static void TestCopyFileOverwrite()
        {
            var copyMoveDeleteTask = new CopyMoveDeleteFileProcessorClass();
            copyMoveDeleteTask.SetCopyFiles(
                "<SourceDocName>"
                , $@"{_destFileDirectory}\$FileOf(<SourceDocName>)");
            copyMoveDeleteTask.DestinationPresentType = ECMDDestinationPresentType.kCMDDestinationPresentError;

            var sourceDocName = Path.Combine(_basicTestSourceFileDirectory, "TestImage001.tif");
            var destFileName = Path.Combine(_destFileDirectory, Path.GetFileName(sourceDocName));

            Directory.CreateDirectory(_destFileDirectory);
            File.Create(destFileName).Dispose();

            Assert.Throws<ExtractException>(() => copyMoveDeleteTask.Execute(sourceDocName));

            copyMoveDeleteTask.DestinationPresentType = ECMDDestinationPresentType.kCMDDestinationPresentSkip;
            copyMoveDeleteTask.Execute(sourceDocName);
            var fileInfo = new FileInfo(destFileName);
            Assert.AreEqual(0, fileInfo.Length);

            copyMoveDeleteTask.DestinationPresentType = ECMDDestinationPresentType.kCMDDestinationPresentOverwrite;
            copyMoveDeleteTask.Execute(sourceDocName);
            var sourceFileLength = new FileInfo(sourceDocName).Length;
            var destFileLength = new FileInfo(destFileName).Length;
            Assert.AreEqual(sourceFileLength, destFileLength);
        }

        /// <summary>
        /// Test related file setting when no related files exist for copy operations
        /// </summary>
        [Test]
        public static void TestCopyRelatedFilesNoRelated()
        {
            var copyMoveDeleteTask = new CopyMoveDeleteFileProcessorClass();
            copyMoveDeleteTask.SetCopyFiles(
                "<SourceDocName>.uss"
                , $@"{_destFileDirectory}\$FileOf(<SourceDocName>).uss");
            copyMoveDeleteTask.IncludeRelatedFiles = true;

            var sourceDocName = Path.Combine(_basicTestSourceFileDirectory, "TestImage001.tif");
            var destFileName = Path.Combine(_destFileDirectory, Path.GetFileName(sourceDocName));
            copyMoveDeleteTask.Execute(sourceDocName);

            Assert.IsTrue(File.Exists(destFileName + ".uss"));
            Assert.AreEqual(1, Directory.GetFiles(_destFileDirectory, "*").Length);
        }

        /// <summary>
        /// Test related file setting properly applies logic for expected source doc extentions for copy operations
        /// </summary>
        [Test]
        public static void TestCopyRelatedFilesExpectedExtension()
        {
            var copyMoveDeleteTask = new CopyMoveDeleteFileProcessorClass();
            copyMoveDeleteTask.SetCopyFiles(
                "<SourceDocName>"
                , $@"{_destFileDirectory}\$FileOf(<SourceDocName>)");
            copyMoveDeleteTask.IncludeRelatedFiles = true;

            var sourceDocName = Path.Combine(_basicTestSourceFileDirectory, "TestImage001.tif");
            var destFileRoot = Path.Combine(_destFileDirectory, Path.GetFileNameWithoutExtension(sourceDocName));
            copyMoveDeleteTask.Execute(sourceDocName);

            // All files get copied because is handled under rules for common extension.
            Assert.IsTrue(File.Exists(destFileRoot + ".tif"));
            Assert.IsTrue(File.Exists(destFileRoot + ".tif.uss"));
            Assert.IsTrue(File.Exists(destFileRoot + ".pdf.uss"));
            Assert.IsTrue(File.Exists(destFileRoot + ".redacted.pdf"));
        }

        /// <summary>
        /// Test related file setting does not copy files based on file root when the specified
        /// document itself does not exist.
        /// </summary>
        [Test]
        public static void TestCopyRelatedFilesUnQualifiedRoot()
        {
            var copyMoveDeleteTask = new CopyMoveDeleteFileProcessorClass();
            copyMoveDeleteTask.SetCopyFiles(
                "<SourceDocName>"
                , $@"{_destFileDirectory}\$FileOf(<SourceDocName>)");
            copyMoveDeleteTask.IncludeRelatedFiles = true;
            copyMoveDeleteTask.SourceMissingType = ECMDSourceMissingType.kCMDSourceMissingSkip;

            var sourceDocName = Path.Combine(_basicTestSourceFileDirectory, "TestImage001.pdf");
            var destFileRoot = Path.Combine(_destFileDirectory, Path.GetFileNameWithoutExtension(sourceDocName));
            copyMoveDeleteTask.Execute(sourceDocName);

            // Since the pdf file itself is missing, only filenames that build off "TestImage001.pdf" are considered.
            Assert.IsTrue(File.Exists(destFileRoot + ".pdf.uss"));
            Assert.AreEqual(1, Directory.GetFiles(_destFileDirectory, "*").Length);
        }

        /// <summary>
        /// Test that the DestinationPresentType setting applies to related files for copy operations
        /// </summary>
        [Test]
        public static void TestCopyRelatedFilesExistingError()
        {
            var copyMoveDeleteTask = new CopyMoveDeleteFileProcessorClass();
            copyMoveDeleteTask.SetCopyFiles(
                "<SourceDocName>"
                , $@"{_destFileDirectory}\$FileOf(<SourceDocName>)");
            copyMoveDeleteTask.IncludeRelatedFiles = true;
            copyMoveDeleteTask.DestinationPresentType = ECMDDestinationPresentType.kCMDDestinationPresentError;

            var sourceDocName = Path.Combine(_basicTestSourceFileDirectory, "TestImage001.tif");
            var destFileRoot = Path.Combine(_destFileDirectory, Path.GetFileNameWithoutExtension(sourceDocName));
            var destUssFile = destFileRoot + ".tif.uss";
            var destPdfUssFile = destFileRoot + ".pdf.uss";
            var destRedactedPdfFile = destFileRoot + ".redacted.pdf";

            Directory.CreateDirectory(_destFileDirectory);
            File.Create(destUssFile).Dispose();
            File.Create(destPdfUssFile).Dispose();

            // Error because DestinationPresentType dictates the task should fail (even if the error is for a related file)
            Assert.Throws<ExtractException>(() => copyMoveDeleteTask.Execute(sourceDocName));

            // destRedactedPdfFile still gets copied because all related files should be attempted even if some fail.
            Assert.IsTrue(File.Exists(destFileRoot + ".tif"));
            Assert.IsTrue(File.Exists(destRedactedPdfFile));
            Assert.AreEqual(0, new FileInfo(destUssFile).Length);
            Assert.AreEqual(0, new FileInfo(destPdfUssFile).Length);
        }

        /// <summary>
        /// Test that the DestinationPresentType setting applies to related files for copy operations
        /// </summary>
        [Test]
        public static void TestCopyRelatedFilesExistingSkip()
        {
            var copyMoveDeleteTask = new CopyMoveDeleteFileProcessorClass();
            copyMoveDeleteTask.SetCopyFiles(
                "<SourceDocName>"
                , $@"{_destFileDirectory}\$FileOf(<SourceDocName>)");
            copyMoveDeleteTask.IncludeRelatedFiles = true;
            copyMoveDeleteTask.DestinationPresentType = ECMDDestinationPresentType.kCMDDestinationPresentSkip;

            var sourceDocName = Path.Combine(_basicTestSourceFileDirectory, "TestImage001.tif");
            var destFileRoot = Path.Combine(_destFileDirectory, Path.GetFileNameWithoutExtension(sourceDocName));
            var destUssFile = destFileRoot + ".tif.uss";
            var destPdfUssFile = destFileRoot + ".pdf.uss";
            var destRedactedPdfFile = destFileRoot + ".redacted.pdf";

            Directory.CreateDirectory(_destFileDirectory);
            File.Create(destUssFile).Dispose();
            File.Create(destPdfUssFile).Dispose();

            // Skip setting means while uss files won't be copied, there won't be errors either
            copyMoveDeleteTask.Execute(sourceDocName);

            // destRedactedPdfFile still gets copied because all related files should be attempted even if some fail.
            Assert.IsTrue(File.Exists(destFileRoot + ".tif"));
            Assert.IsTrue(File.Exists(destRedactedPdfFile));
            Assert.AreEqual(0, new FileInfo(destUssFile).Length);
            Assert.AreEqual(0, new FileInfo(destPdfUssFile).Length);
        }

        /// <summary>
        /// Test that the DestinationPresentType setting applies to related files for copy operations
        /// </summary>
        [Test]
        public static void TestCopyRelatedFilesExistingOverwrite()
        {
            var copyMoveDeleteTask = new CopyMoveDeleteFileProcessorClass();
            copyMoveDeleteTask.SetCopyFiles(
                "<SourceDocName>"
                , $@"{_destFileDirectory}\$FileOf(<SourceDocName>)");
            copyMoveDeleteTask.IncludeRelatedFiles = true;
            copyMoveDeleteTask.DestinationPresentType = ECMDDestinationPresentType.kCMDDestinationPresentOverwrite;

            var sourceDocName = Path.Combine(_basicTestSourceFileDirectory, "TestImage001.tif");
            var destFileRoot = Path.Combine(_destFileDirectory, Path.GetFileNameWithoutExtension(sourceDocName));
            var destUssFile = destFileRoot + ".tif.uss";
            var sourceUssFileLength = new FileInfo(sourceDocName + ".uss").Length;
            var destPdfUssFile = destFileRoot + ".pdf.uss";
            var sourcePdfUssFileLength = new FileInfo(sourceDocName.Replace(".tif", ".pdf.uss")).Length;
            var destRedactedPdfFile = destFileRoot + ".redacted.pdf";

            Directory.CreateDirectory(_destFileDirectory);
            File.Create(destUssFile).Dispose();
            File.Create(destPdfUssFile).Dispose();

            // Skip setting means while uss files won't be copied, there won't be errors either
            copyMoveDeleteTask.Execute(sourceDocName);

            // destRedactedPdfFile still gets copied because all related files should be attempted even if some fail.
            Assert.IsTrue(File.Exists(destFileRoot + ".tif"));
            Assert.IsTrue(File.Exists(destRedactedPdfFile));
            Assert.AreEqual(sourceUssFileLength, new FileInfo(destUssFile).Length);
            Assert.AreEqual(sourcePdfUssFileLength, new FileInfo(destPdfUssFile).Length);
        }

        /// <summary>
        /// Basic move operation
        /// </summary>
        [Test]
        public static void TestMoveFile()
        {
            var copyMoveDeleteTask = new CopyMoveDeleteFileProcessorClass();
            copyMoveDeleteTask.SetMoveFiles(
                "<SourceDocName>"
                , $@"{_destFileDirectory}\$FileOf(<SourceDocName>)");

            var sourceDocName = Path.Combine(_basicTestSourceFileDirectory, "TestImage001.tif");
            var destFileName = Path.Combine(_destFileDirectory, Path.GetFileName(sourceDocName));
            copyMoveDeleteTask.Execute(sourceDocName);

            Assert.IsFalse(File.Exists(sourceDocName));
            Assert.IsTrue(File.Exists(destFileName));
            Assert.AreEqual(1, Directory.GetFiles(_destFileDirectory, "*").Length);

            // Ensure remaining files have not been deleted
            Assert.AreEqual(3, Directory.GetFiles(_basicTestSourceFileDirectory, "*").Length);
        }

        /// <summary>
        /// Test SourceMissingType settings for move operations
        /// </summary>
        [Test]
        public static void TestMoveMissingFile()
        {
            var copyMoveDeleteTask = new CopyMoveDeleteFileProcessorClass();
            copyMoveDeleteTask.SetMoveFiles(
                "<SourceDocName>"
                , $@"{_destFileDirectory}\$FileOf(<SourceDocName>)");
            copyMoveDeleteTask.SourceMissingType = ECMDSourceMissingType.kCMDSourceMissingSkip;

            var sourceDocName = Path.Combine(_basicTestSourceFileDirectory, "TestImage001.missing.tif");
            var destFileName = Path.Combine(_destFileDirectory, Path.GetFileName(sourceDocName));
            copyMoveDeleteTask.Execute(sourceDocName);

            Assert.IsFalse(File.Exists(destFileName));

            copyMoveDeleteTask.SourceMissingType = ECMDSourceMissingType.kCMDSourceMissingError;

            Assert.Throws<ExtractException>(() => copyMoveDeleteTask.Execute(sourceDocName));
            Assert.IsFalse(File.Exists(destFileName));
        }

        /// <summary>
        /// Test DestinationPresentType settings for move operations
        /// </summary>
        [Test]
        public static void TestMoveFileOverwrite()
        {
            var copyMoveDeleteTask = new CopyMoveDeleteFileProcessorClass();
            copyMoveDeleteTask.SetMoveFiles(
                "<SourceDocName>"
                , $@"{_destFileDirectory}\$FileOf(<SourceDocName>)");
            copyMoveDeleteTask.DestinationPresentType = ECMDDestinationPresentType.kCMDDestinationPresentError;

            var sourceDocName = Path.Combine(_basicTestSourceFileDirectory, "TestImage001.tif");
            var sourceFileLength = new FileInfo(sourceDocName).Length;
            var destFileName = Path.Combine(_destFileDirectory, Path.GetFileName(sourceDocName));

            Directory.CreateDirectory(_destFileDirectory);
            File.Create(destFileName).Dispose();

            Assert.Throws<ExtractException>(() => copyMoveDeleteTask.Execute(sourceDocName));

            Assert.IsTrue(File.Exists(sourceDocName));

            copyMoveDeleteTask.DestinationPresentType = ECMDDestinationPresentType.kCMDDestinationPresentSkip;
            copyMoveDeleteTask.Execute(sourceDocName);
            var fileInfo = new FileInfo(destFileName);
            Assert.AreEqual(0, fileInfo.Length);
            Assert.IsTrue(File.Exists(sourceDocName));

            copyMoveDeleteTask.DestinationPresentType = ECMDDestinationPresentType.kCMDDestinationPresentOverwrite;
            copyMoveDeleteTask.Execute(sourceDocName);
            var destFileLength = new FileInfo(destFileName).Length;
            Assert.AreEqual(sourceFileLength, destFileLength);
            Assert.IsFalse(File.Exists(sourceDocName));

            // Ensure remaining files have not been deleted
            Assert.AreEqual(3, Directory.GetFiles(_basicTestSourceFileDirectory, "*").Length);
        }

        /// <summary>
        /// Test related file setting when no related files exist for move operations
        /// </summary>
        [Test]
        public static void TestMoveRelatedFilesNoRelated()
        {
            var copyMoveDeleteTask = new CopyMoveDeleteFileProcessorClass();
            copyMoveDeleteTask.SetMoveFiles(
                "<SourceDocName>.uss"
                , $@"{_destFileDirectory}\$FileOf(<SourceDocName>).uss");
            copyMoveDeleteTask.IncludeRelatedFiles = true;

            var sourceDocName = Path.Combine(_basicTestSourceFileDirectory, "TestImage001.tif");
            var destFileName = Path.Combine(_destFileDirectory, Path.GetFileName(sourceDocName));
            copyMoveDeleteTask.Execute(sourceDocName);

            Assert.IsFalse(File.Exists(sourceDocName + ".uss"));
            Assert.IsTrue(File.Exists(destFileName + ".uss"));
            Assert.AreEqual(1, Directory.GetFiles(_destFileDirectory, "*").Length);
        }

        /// <summary>
        /// Test related file setting properly applies logic for expected source doc extentions for move operations
        /// </summary>
        [Test]
        public static void TestMoveRelatedFilesExpectedExtension()
        {
            var copyMoveDeleteTask = new CopyMoveDeleteFileProcessorClass();
            copyMoveDeleteTask.SetMoveFiles(
                "<SourceDocName>"
                , $@"{_destFileDirectory}\$FileOf(<SourceDocName>)");
            copyMoveDeleteTask.IncludeRelatedFiles = true;

            var sourceDocName = Path.Combine(_basicTestSourceFileDirectory, "TestImage001.tif");
            var sourceDocRoot = Path.Combine(_basicTestSourceFileDirectory, "TestImage001");
            var destFileRoot = Path.Combine(_destFileDirectory, Path.GetFileNameWithoutExtension(sourceDocName));
            copyMoveDeleteTask.Execute(sourceDocName);

            // All files get moved because is handled under rules for common extension.
            Assert.IsTrue(File.Exists(destFileRoot + ".tif"));
            Assert.IsTrue(File.Exists(destFileRoot + ".tif.uss"));
            Assert.IsTrue(File.Exists(destFileRoot + ".pdf.uss"));
            Assert.IsTrue(File.Exists(destFileRoot + ".redacted.pdf"));
            Assert.IsFalse(File.Exists(sourceDocRoot + ".tif"));
            Assert.IsFalse(File.Exists(sourceDocRoot + ".tif.uss"));
            Assert.IsFalse(File.Exists(sourceDocRoot + ".pdf.uss"));
            Assert.IsFalse(File.Exists(sourceDocRoot + ".redacted.pdf"));
        }

        /// <summary>
        /// Test related file setting does not move files based on file root when the specified
        /// document itself does not exist.
        /// </summary>
        [Test]
        public static void TestMoveRelatedFilesUnQualifiedRoot()
        {
            var copyMoveDeleteTask = new CopyMoveDeleteFileProcessorClass();
            copyMoveDeleteTask.SetMoveFiles(
                "<SourceDocName>"
                , $@"{_destFileDirectory}\$FileOf(<SourceDocName>)");
            copyMoveDeleteTask.IncludeRelatedFiles = true;
            copyMoveDeleteTask.SourceMissingType = ECMDSourceMissingType.kCMDSourceMissingSkip;

            var sourceDocName = Path.Combine(_basicTestSourceFileDirectory, "TestImage001.pdf");
            var destFileRoot = Path.Combine(_destFileDirectory, Path.GetFileNameWithoutExtension(sourceDocName));
            copyMoveDeleteTask.Execute(sourceDocName);

            // Since the pdf file itself is missing, only filenames that build off "TestImage001.pdf" are considered.
            Assert.IsFalse(File.Exists(sourceDocName + ".uss"));
            Assert.IsTrue(File.Exists(destFileRoot + ".pdf.uss"));
            Assert.AreEqual(1, Directory.GetFiles(_destFileDirectory, "*").Length);
        }

        /// <summary>
        /// Test that the DestinationPresentType setting applies to related files for move operations
        /// </summary>
        [Test]
        public static void TestMoveRelatedFilesExistingError()
        {
            var copyMoveDeleteTask = new CopyMoveDeleteFileProcessorClass();
            copyMoveDeleteTask.SetMoveFiles(
                "<SourceDocName>"
                , $@"{_destFileDirectory}\$FileOf(<SourceDocName>)");
            copyMoveDeleteTask.IncludeRelatedFiles = true;
            copyMoveDeleteTask.DestinationPresentType = ECMDDestinationPresentType.kCMDDestinationPresentError;

            var sourceDocName = Path.Combine(_basicTestSourceFileDirectory, "TestImage001.tif");
            var destFileRoot = Path.Combine(_destFileDirectory, Path.GetFileNameWithoutExtension(sourceDocName));
            var destUssFile = destFileRoot + ".tif.uss";
            var destPdfUssFile = destFileRoot + ".pdf.uss";
            var destRedactedPdfFile = destFileRoot + ".redacted.pdf";

            Directory.CreateDirectory(_destFileDirectory);
            File.Create(destUssFile).Dispose();
            File.Create(destPdfUssFile).Dispose();

            // Error because DestinationPresentType dictates the task should fail (even if the error is for a related file)
            Assert.Throws<ExtractException>(() => copyMoveDeleteTask.Execute(sourceDocName));

            // destRedactedPdfFile still gets moved because all related files should be attempted even if some fail.
            Assert.IsTrue(File.Exists(destFileRoot + ".tif"));
            Assert.IsTrue(File.Exists(destRedactedPdfFile));
            Assert.AreEqual(0, new FileInfo(destUssFile).Length);
            Assert.AreEqual(0, new FileInfo(destPdfUssFile).Length);
            Assert.IsFalse(File.Exists(sourceDocName));  // Should be moved
            Assert.IsFalse(File.Exists(sourceDocName.Replace(".tif",".redacted.pdf"))); // Should be moved
            Assert.IsTrue(File.Exists(sourceDocName + ".uss")); // Move should have failed
            Assert.IsTrue(File.Exists(sourceDocName.Replace(".tif", ".pdf.uss"))); // Move should have failed
        }

        /// <summary>
        /// Test that the DestinationPresentType setting applies to related files for move operations
        /// </summary>
        [Test]
        public static void TestMoveRelatedFilesExistingSkip()
        {
            var copyMoveDeleteTask = new CopyMoveDeleteFileProcessorClass();
            copyMoveDeleteTask.SetMoveFiles(
                "<SourceDocName>"
                , $@"{_destFileDirectory}\$FileOf(<SourceDocName>)");
            copyMoveDeleteTask.IncludeRelatedFiles = true;
            copyMoveDeleteTask.DestinationPresentType = ECMDDestinationPresentType.kCMDDestinationPresentSkip;

            var sourceDocName = Path.Combine(_basicTestSourceFileDirectory, "TestImage001.tif");
            var destFileRoot = Path.Combine(_destFileDirectory, Path.GetFileNameWithoutExtension(sourceDocName));
            var destUssFile = destFileRoot + ".tif.uss";
            var destPdfUssFile = destFileRoot + ".pdf.uss";
            var destRedactedPdfFile = destFileRoot + ".redacted.pdf";

            Directory.CreateDirectory(_destFileDirectory);
            File.Create(destUssFile).Dispose();
            File.Create(destPdfUssFile).Dispose();

            // Skip setting means while uss files won't be copied, there won't be errors either
            copyMoveDeleteTask.Execute(sourceDocName);

            // destRedactedPdfFile still gets copied because all related files should be attempted even if some fail.
            Assert.IsTrue(File.Exists(destFileRoot + ".tif"));
            Assert.IsTrue(File.Exists(destRedactedPdfFile));
            Assert.AreEqual(0, new FileInfo(destUssFile).Length);
            Assert.AreEqual(0, new FileInfo(destPdfUssFile).Length);
            Assert.IsFalse(File.Exists(sourceDocName));  // Should be moved
            Assert.IsFalse(File.Exists(sourceDocName.Replace(".tif", ".redacted.pdf"))); // Should be moved
            Assert.IsTrue(File.Exists(sourceDocName + ".uss")); // Move should be skipped
            Assert.IsTrue(File.Exists(sourceDocName.Replace(".tif", ".pdf.uss"))); // Move should be skipped
        }

        /// <summary>
        /// Test that the DestinationPresentType setting applies to related files for move operations
        /// </summary>
        [Test]
        public static void TestMoveRelatedFilesExistingOverwrite()
        {
            var copyMoveDeleteTask = new CopyMoveDeleteFileProcessorClass();
            copyMoveDeleteTask.SetMoveFiles(
                "<SourceDocName>"
                , $@"{_destFileDirectory}\$FileOf(<SourceDocName>)");
            copyMoveDeleteTask.IncludeRelatedFiles = true;
            copyMoveDeleteTask.DestinationPresentType = ECMDDestinationPresentType.kCMDDestinationPresentOverwrite;

            var sourceDocName = Path.Combine(_basicTestSourceFileDirectory, "TestImage001.tif");
            var destFileRoot = Path.Combine(_destFileDirectory, Path.GetFileNameWithoutExtension(sourceDocName));
            var destUssFile = destFileRoot + ".tif.uss";
            var sourceUssFileLength = new FileInfo(sourceDocName + ".uss").Length;
            var destPdfUssFile = destFileRoot + ".pdf.uss";
            var sourcePdfUssFileLength = new FileInfo(sourceDocName.Replace(".tif", ".pdf.uss")).Length;
            var destRedactedPdfFile = destFileRoot + ".redacted.pdf";

            Directory.CreateDirectory(_destFileDirectory);
            File.Create(destUssFile).Dispose();
            File.Create(destPdfUssFile).Dispose();

            // Skip setting means while uss files won't be copied, there won't be errors either
            copyMoveDeleteTask.Execute(sourceDocName);

            // destRedactedPdfFile still gets copied because all related files should be attempted even if some fail.
            Assert.IsTrue(File.Exists(destFileRoot + ".tif"));
            Assert.IsTrue(File.Exists(destRedactedPdfFile));
            Assert.AreEqual(sourceUssFileLength, new FileInfo(destUssFile).Length);
            Assert.AreEqual(sourcePdfUssFileLength, new FileInfo(destPdfUssFile).Length);
            Assert.IsFalse(File.Exists(sourceDocName));  // Should be moved
            Assert.IsFalse(File.Exists(sourceDocName.Replace(".tif", ".redacted.pdf"))); // Should be moved
            Assert.IsFalse(File.Exists(sourceDocName + ".uss")); // Move should have overwritten
            Assert.IsFalse(File.Exists(sourceDocName.Replace(".tif", ".pdf.uss"))); // Move should have overwritten
        }

        /// <summary>
        /// Basic delet operation
        /// </summary>
        [Test]
        public static void TestDeleteFile()
        {
            var copyMoveDeleteTask = new CopyMoveDeleteFileProcessorClass();
            copyMoveDeleteTask.SetDeleteFiles("<SourceDocName>");

            var sourceDocName = Path.Combine(_basicTestSourceFileDirectory, "TestImage001.tif");
            var destFileName = Path.Combine(_destFileDirectory, Path.GetFileName(sourceDocName));
            copyMoveDeleteTask.Execute(sourceDocName);

            Assert.IsFalse(File.Exists(sourceDocName));

            // Ensure remaining files have not been deleted
            Assert.AreEqual(3, Directory.GetFiles(_basicTestSourceFileDirectory, "*").Length);
        }

        /// <summary>
        /// Test SourceMissingType settings for delete operations
        /// </summary>
        [Test]
        public static void TestDeleteMissingFile()
        {
            var copyMoveDeleteTask = new CopyMoveDeleteFileProcessorClass();
            copyMoveDeleteTask.SetDeleteFiles("<SourceDocName>");
            copyMoveDeleteTask.SourceMissingType = ECMDSourceMissingType.kCMDSourceMissingSkip;

            var sourceDocName = Path.Combine(_basicTestSourceFileDirectory, "TestImage001.missing.tif");
            var destFileName = Path.Combine(_destFileDirectory, Path.GetFileName(sourceDocName));

            copyMoveDeleteTask.Execute(sourceDocName);

            copyMoveDeleteTask.SourceMissingType = ECMDSourceMissingType.kCMDSourceMissingError;

            Assert.Throws<ExtractException>(() => copyMoveDeleteTask.Execute(sourceDocName));
        }

        /// <summary>
        /// Test related file setting when no related files exist for delete operations
        /// </summary>
        [Test]
        public static void TestDeleteRelatedFilesNoRelated()
        {
            var copyMoveDeleteTask = new CopyMoveDeleteFileProcessorClass();
            copyMoveDeleteTask.SetDeleteFiles(
                "<SourceDocName>.uss");
            copyMoveDeleteTask.IncludeRelatedFiles = true;

            var sourceDocName = Path.Combine(_basicTestSourceFileDirectory, "TestImage001.tif");
            copyMoveDeleteTask.Execute(sourceDocName);

            // No only uss should have been deleted; no other files count as related
            Assert.AreEqual(3, Directory.GetFiles(_basicTestSourceFileDirectory, "*").Length);
        }

        /// <summary>
        /// Test related file setting properly applies logic for expected source doc extentions for delete operations
        /// </summary>
        [Test]
        public static void TestDeleteRelatedFilesExpectedExtension()
        {
            var copyMoveDeleteTask = new CopyMoveDeleteFileProcessorClass();
            copyMoveDeleteTask.SetDeleteFiles("<SourceDocName>");
            copyMoveDeleteTask.IncludeRelatedFiles = true;

            var sourceDocName = Path.Combine(_basicTestSourceFileDirectory, "TestImage001.tif");
            copyMoveDeleteTask.Execute(sourceDocName);

            // All files get deleted because is handled under rules for common extension.
            Assert.AreEqual(0, Directory.GetFiles(_basicTestSourceFileDirectory, "*").Length);
        }

        /// <summary>
        /// Test related file setting does not delete files based on file root when the specified
        /// document itself does not exist.
        /// </summary>
        [Test]
        public static void TestDeleteRelatedFilesUnQualifiedRoot()
        {
            var copyMoveDeleteTask = new CopyMoveDeleteFileProcessorClass();
            copyMoveDeleteTask.SetDeleteFiles("<SourceDocName>");
            copyMoveDeleteTask.IncludeRelatedFiles = true;
            copyMoveDeleteTask.SourceMissingType = ECMDSourceMissingType.kCMDSourceMissingSkip;

            var sourceDocName = Path.Combine(_basicTestSourceFileDirectory, "TestImage001.pdf");
            copyMoveDeleteTask.Execute(sourceDocName);

            // Since the pdf file itself is missing, only filenames that build off "TestImage001.pdf" are considered.
            Assert.IsFalse(File.Exists(sourceDocName + ".uss"));
        }

        /// <summary>
        /// For related files, test that the list of extentions to be treated under rules for expected extenaions
        /// is applied correctly.
        /// </summary>
        [Test]
        public static void TestRelatedFilesExpectedExtensions()
        {
            Assert.IsTrue(IsExpectedExtension(".tif"));
            Assert.IsTrue(IsExpectedExtension(".tiff"));
            Assert.IsTrue(IsExpectedExtension(".pdf"));
            Assert.IsTrue(IsExpectedExtension(".bmp"));
            Assert.IsTrue(IsExpectedExtension(".jpg"));
            Assert.IsTrue(IsExpectedExtension(".jpeg"));
            Assert.IsTrue(IsExpectedExtension(".rtf"));
            Assert.IsFalse(IsExpectedExtension(".001"));
            Assert.IsFalse(IsExpectedExtension(".gif"));
            Assert.IsFalse(IsExpectedExtension(".txt"));
            Assert.IsFalse(IsExpectedExtension(".xml"));
        }

        /// <summary>
        /// For related files, test that handling of files with various permutations of multiple extensions are
        /// handled correctly.
        /// </summary>
        [Test]
        public static void TestRelatedFilesMultipleExtensions()
        {
            var copyMoveDeleteTask = new CopyMoveDeleteFileProcessorClass();
            copyMoveDeleteTask.SetCopyFiles("<SourceDocName>"
                , $@"{_destFileDirectory}\$FileOf(<SourceDocName>)");
            copyMoveDeleteTask.IncludeRelatedFiles = true;

            var sourceDocName = Path.Combine(_sourceFileDirectory, "TestImage003", "TestImage003.0001.pdf.tif");
            copyMoveDeleteTask.Execute(sourceDocName);

            var destFileRoot = Path.Combine(_destFileDirectory, Path.GetFileNameWithoutExtension(sourceDocName));

            // Ensure neither TestImage001 nor TestImage001_Paginated_30 are included
            Assert.IsTrue(File.Exists(Path.Combine(_destFileDirectory, "TestImage003.0001.pdf.tif")));
            Assert.IsTrue(File.Exists(Path.Combine(_destFileDirectory, "TestImage003.0001.pdf.tif.xml")));
            Assert.IsTrue(File.Exists(Path.Combine(_destFileDirectory, "TestImage003.0001.tif")));
            Assert.IsTrue(File.Exists(Path.Combine(_destFileDirectory, "TestImage003.0001.pdf")));

            Assert.IsFalse(File.Exists(Path.Combine(_destFileDirectory, "TestImage001.tif")));
            Assert.IsFalse(File.Exists(Path.Combine(_destFileDirectory, "TestImage001.pdf")));
            Assert.IsFalse(File.Exists(Path.Combine(_destFileDirectory, "TestImage001.pdf.tif")));
        }

        /// <summary>
        /// For related files, test if the qualified root of the filename matches the name of a directory,
        /// files in that directory are not included.
        /// </summary>
        [Test]
        public static void TestRelatedFilesRootFileNameIsDirectory()
        {
            var copyMoveDeleteTask = new CopyMoveDeleteFileProcessorClass();
            copyMoveDeleteTask.SetCopyFiles("<SourceDocName>"
                ,$@"{_destFileDirectory}\$FileOf(<SourceDocName>)");
            copyMoveDeleteTask.IncludeRelatedFiles = true;

            var sourceDocName = Path.Combine(_sourceFileDirectory, "TestImage003.tif");
            copyMoveDeleteTask.Execute(sourceDocName);

            var destFileRoot = Path.Combine(_destFileDirectory, Path.GetFileNameWithoutExtension(sourceDocName));
            // Ensure all 5 files that qualify are copies, but that nothing from the directories TestImage003 or TestImage003.1 are.
            Assert.IsTrue(File.Exists(destFileRoot + ".tif"));
            Assert.IsTrue(File.Exists(destFileRoot + ".tif.uss"));
            Assert.IsTrue(File.Exists(destFileRoot + ".tif.voa"));
            Assert.IsTrue(File.Exists(destFileRoot + ".tif.xml"));
            Assert.IsTrue(File.Exists(destFileRoot + ".bak"));
            Assert.AreEqual(5, Directory.GetFiles(_destFileDirectory, "*").Length);
            Assert.IsFalse(Directory.Exists(Path.Combine(_destFileDirectory, "TestImage003")));
            Assert.IsFalse(Directory.Exists(Path.Combine(_destFileDirectory, "TestImage003.01")));
        }

        /// <summary>
        /// For related files, test that the filename pattern used for paginated files do not
        /// cause operations to be applied to the source batch.
        /// </summary>
        [Test]
        public static void TestRelatedFilesPaginatedFile()
        {
            var copyMoveDeleteTask = new CopyMoveDeleteFileProcessorClass();
            copyMoveDeleteTask.SetCopyFiles("<SourceDocName>"
                , $@"{_destFileDirectory}\$FileOf(<SourceDocName>)");
            copyMoveDeleteTask.IncludeRelatedFiles = true;

            var sourceDocName = Path.Combine(_sourceFileDirectory, "PaginationTest", "TestImage001_Paginated_3.tif");
            copyMoveDeleteTask.Execute(sourceDocName);

            var destFileRoot = Path.Combine(_destFileDirectory, Path.GetFileNameWithoutExtension(sourceDocName));

            // Ensure neither TestImage001 nor TestImage001_Paginated_30 are included
            Assert.IsTrue(File.Exists(Path.Combine(_destFileDirectory, "TestImage001_Paginated_3.tif")));
            Assert.IsTrue(File.Exists(Path.Combine(_destFileDirectory, "TestImage001_Paginated_3.tif.DataAfterLastVerify.voa")));
            Assert.IsFalse(File.Exists(Path.Combine(_destFileDirectory, "TestImage001.pdf.tif.voa")));
            Assert.IsFalse(File.Exists(Path.Combine(_destFileDirectory, "TestImage001_Paginated_30.tif")));
        }

        /// <summary>
        /// For related files, test that the related files are calculated prior to any operartion that
        /// would affect which files are deemed related and what their resulting names will be.
        /// </summary>
        [Test]
        public static void TestRelatedFileIdentificationSequence()
        {
            var copyMoveDeleteTask = new CopyMoveDeleteFileProcessorClass();
            // Specify that files be renamed to new files in the same directory, but with a random character
            // sequence inserted before the extension
            copyMoveDeleteTask.SetMoveFiles("<SourceDocName>"
                , $@"{_basicTestSourceFileDirectory}\$InsertBeforeExtension($FileOf(<SourceDocName>),$RandomAlphaNumeric(5))");
            copyMoveDeleteTask.IncludeRelatedFiles = true;

            var sourceDocName = Path.Combine(_basicTestSourceFileDirectory, "TestImage001.tif");
            copyMoveDeleteTask.Execute(sourceDocName);

            string destinationFile =
                Directory.GetFiles(_basicTestSourceFileDirectory, "TestImage001?????.tif")
                .Single();

            // Moving the files to new names in the same directory should still result in the same
            // four files.
            Assert.AreEqual(4, Directory.GetFiles(_basicTestSourceFileDirectory, "*").Length);

            // Parse out the random sequence of digits added; this sequence should be shared by all
            // files deemed related to the source doc.
            string randomSequence = destinationFile.Substring(destinationFile.Length - 9, 5);
            
            var destinationFileBase = Path.Combine(_basicTestSourceFileDirectory, "TestImage001" + randomSequence);
            Assert.IsTrue(File.Exists(destinationFileBase + ".tif.uss"));
            Assert.IsTrue(File.Exists(destinationFileBase + ".pdf.uss"));
            Assert.IsTrue(File.Exists(destinationFileBase + ".redacted.pdf"));

            // Running a delete on the new destination file should properly delete the relate files
            // Deletion of TestImage001?????.tif first should not affect the identification of
            // TestImage001?????.pdf.uss and TestImage001?????.redacted.pdf as related files.
            copyMoveDeleteTask.SetDeleteFiles("<SourceDocName>");
            copyMoveDeleteTask.Execute(destinationFile);

            Assert.AreEqual(0, Directory.GetFiles(_basicTestSourceFileDirectory, "*").Length);
        }

        /// <summary>
        /// Test that neither a rename is performed nor related files process if the primary file
        /// operation does not succeed.
        /// </summary>
        [Test]
        public static void TestRelatedFilesPrimaryOperationError()
        {
            string dbName = "Test_RelatedFilesPrimaryOperationError";
            IFileProcessingDB fileProcessingDb = null;

            try
            {
                var copyMoveDeleteTask = new CopyMoveDeleteFileProcessorClass();
                copyMoveDeleteTask.SetMoveFiles("<SourceDocName>"
                    , $@"{_destFileDirectory}\$FileOf(<SourceDocName>)");
                copyMoveDeleteTask.SourceMissingType = ECMDSourceMissingType.kCMDSourceMissingError;
                copyMoveDeleteTask.IncludeRelatedFiles = true;
                copyMoveDeleteTask.ModifySourceDocName = true;

                var sourceDocName = Path.Combine(_basicTestSourceFileDirectory, "TestImage001.pdf.tif");
                fileProcessingDb = RunAgainstDB(dbName, copyMoveDeleteTask, sourceDocName);

                var stats = fileProcessingDb.GetStats(1, true);
                Assert.AreEqual(1, stats.NumDocumentsFailed);

                // Ensure rename did not occur
                Assert.AreEqual(fileProcessingDb.GetFileNameFromFileID(1), sourceDocName);
                // Ensure related files not processed
                Assert.AreEqual(0, Directory.GetFiles(_destFileDirectory, "*").Length);
            }
            finally
            {
                fileProcessingDb?.CloseAllDBConnections();
                _testDbManager.RemoveDatabase(dbName);
            }
        }

        /// <summary>
        /// Test that related files do not process if a rename fails.
        /// </summary>
        [Test]
        public static void TestRelatedFilesRenameOperationError()
        {
            string dbName = "Test_RelatedFilesRenameOperationError";
            IFileProcessingDB fileProcessingDb = null;

            try
            {
                var copyMoveDeleteTask = new CopyMoveDeleteFileProcessorClass();
                copyMoveDeleteTask.SetMoveFiles("<SourceDocName>"
                    , $@"{_destFileDirectory}\$FileOf(<SourceDocName>)");
                copyMoveDeleteTask.SourceMissingType = ECMDSourceMissingType.kCMDSourceMissingError;
                copyMoveDeleteTask.IncludeRelatedFiles = true;
                copyMoveDeleteTask.ModifySourceDocName = true;

                var sourceDocName = Path.Combine(_basicTestSourceFileDirectory, "TestImage001.tif");
                var destFileName = Path.Combine(_destFileDirectory, "TestImage001.tif");
                fileProcessingDb = RunAgainstDB(dbName, copyMoveDeleteTask, new[] { sourceDocName, destFileName });

                // Though we only are really testing sourceDocName fails because destFileName exists, destFileName
                // will fail as well.
                var stats = fileProcessingDb.GetStats(1, true);
                Assert.AreEqual(2, stats.NumDocumentsFailed);

                // Ensure file was moved
                Assert.IsTrue(File.Exists(destFileName));
                Assert.IsFalse(File.Exists(sourceDocName));

                // After rename error, ensure related files not processed
                Assert.AreEqual(1, Directory.GetFiles(_destFileDirectory, "*").Length);
            }
            finally
            {
                fileProcessingDb?.CloseAllDBConnections();
                _testDbManager.RemoveDatabase(dbName);
            }
        }

        /// <summary>
        /// Test that if some related files fail, other are still processed if they can be.
        /// </summary>
        [Test]
        public static void TestRelatedFilesRelatedFilesError()
        {
            string dbName = "Test_RelatedFilesRelatedFilesError";
            IFileProcessingDB fileProcessingDb = null;

            try
            {
                var copyMoveDeleteTask = new CopyMoveDeleteFileProcessorClass();
                copyMoveDeleteTask.SetMoveFiles("<SourceDocName>"
                    , $@"{_destFileDirectory}\$FileOf(<SourceDocName>)");
                copyMoveDeleteTask.SourceMissingType = ECMDSourceMissingType.kCMDSourceMissingError;
                copyMoveDeleteTask.IncludeRelatedFiles = true;
                copyMoveDeleteTask.ModifySourceDocName = true;
                copyMoveDeleteTask.DestinationPresentType = ECMDDestinationPresentType.kCMDDestinationPresentError;

                Directory.CreateDirectory(_destFileDirectory);
                var previouslyExistingRelatedFile = Path.Combine(_destFileDirectory, "TestImage001.pdf.uss");
                File.Create(previouslyExistingRelatedFile).Dispose();

                var sourceDocName = Path.Combine(_basicTestSourceFileDirectory, "TestImage001.tif");
                fileProcessingDb = RunAgainstDB(dbName, copyMoveDeleteTask, sourceDocName);

                var stats = fileProcessingDb.GetStats(1, true);
                Assert.AreEqual(1, stats.NumDocumentsFailed);

                // Ensure rename occurred.
                Assert.AreEqual(fileProcessingDb.GetFileNameFromFileID(1), Path.Combine(_destFileDirectory, "TestImage001.tif"));

                // Ensure all files except TestImage001.pdf.uss were moved
                Assert.IsFalse(File.Exists(Path.Combine(_basicTestSourceFileDirectory, "TestImage001.tif")));
                Assert.IsFalse(File.Exists(Path.Combine(_basicTestSourceFileDirectory, "TestImage001.tif.uss")));
                Assert.IsFalse(File.Exists(Path.Combine(_basicTestSourceFileDirectory, "TestImage001.redacted.pdf")));
                Assert.IsTrue(File.Exists(Path.Combine(_basicTestSourceFileDirectory, "TestImage001.pdf.uss")));

                // All files should be in dest directory, but failed file should still be zero len.
                Assert.AreEqual(4, Directory.GetFiles(_destFileDirectory, "*").Length);
                Assert.AreEqual(0, new FileInfo(previouslyExistingRelatedFile).Length);
            }
            finally
            {
                fileProcessingDb?.CloseAllDBConnections();
                _testDbManager.RemoveDatabase(dbName);
            }
        }

        #endregion Tests

        #region Helper Methods

        /// <summary>
        /// Tests if the specified extension is handled under rules for expected extensions of source doc names.
        /// </summary>
        public static bool IsExpectedExtension(string extension)
        {
            try
            {
                var copyMoveDeleteTask = new CopyMoveDeleteFileProcessorClass();
                copyMoveDeleteTask.SetDeleteFiles("<SourceDocName>");
                copyMoveDeleteTask.IncludeRelatedFiles = true;

                if (!Directory.Exists(_destFileDirectory))
                {
                    Directory.CreateDirectory(_destFileDirectory);
                }

                var sourceDocName = Path.Combine(_destFileDirectory, "TestImage001" + extension);
                var backupFileName = Path.Combine(_destFileDirectory, "TestImage001.bak");
                File.Copy(
                    Path.Combine(_basicTestSourceFileDirectory, "TestImage001.tif"),
                    sourceDocName);
                File.Create(backupFileName).Dispose();

                copyMoveDeleteTask.Execute(sourceDocName);

                // If backup was deleted, extension has been treated as an expected extension. 
                return !File.Exists(backupFileName);
            }
            finally
            {
                Directory.Delete(_destFileDirectory, recursive: true);
            }
        }

        /// <summary>
        /// Creates a database and runs the specified Copy/Move/Delete task against the specified files in the database.
        /// </summary>
        /// <returns>The IFileProcessingDB against which the task was run.</returns>
        public static IFileProcessingDB RunAgainstDB(string testName, CopyMoveDeleteFileProcessorClass task, params string[] files)
        {
            IFileProcessingDB fileProcessingDb = _testDbManager.GetNewDatabase(testName);
            fileProcessingDb.DefineNewAction(_FIRST_ACTION);

            foreach (var file in files)
            {
                int id = fileProcessingDb.AddFileNoQueue(file, 0, 0, EFilePriority.kPriorityNormal, -1);
                fileProcessingDb.SetFileStatusToPending(id, _FIRST_ACTION, true);
            }

            // Press the file
            using (var famSession = new FAMProcessingSession(
                fileProcessingDb, _FIRST_ACTION, "", task))
            {
                famSession.WaitForProcessingToComplete();
            }

            return fileProcessingDb;
        }

        #endregion Helper Methods
    }
}
