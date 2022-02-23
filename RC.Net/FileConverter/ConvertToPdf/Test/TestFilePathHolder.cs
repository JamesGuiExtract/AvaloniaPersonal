using Extract.Testing.Utilities;
using Extract.Utilities;
using NUnit.Framework;
using System;
using System.Linq;

namespace Extract.FileConverter.Test
{
    // Class for testing the FilePathHolder functionality
    [TestFixture]
    [Category("FilePathHolder")]
    public class TestFilePathHolder
    {
        // Performs initialization needed for the entire test run.
        [OneTimeSetUp]
        public static void Initialize()
        {
            GeneralMethods.TestSetup();
        }

        // Ensure proper wrapper types are generated for unknown file types
        [Test, Category("Automated")]
        public void TestCreate_Unknown
            ([Values(
            ".xsdhfshdfa"
            )] string extension)
        {
            // Arrange
            using TemporaryFile tempInputFile = new(extension, false);

            // Act
            FilePathHolder filePathHolder = FilePathHolder.Create(tempInputFile.FileName);

            // Assert
            Assert.AreEqual(FileType.Unknown, filePathHolder.FileType);
            Assert.AreEqual(typeof(UnknownFile), filePathHolder.GetType());
        }

        // Ensure proper wrapper types are generated for text documents
        [Test, Category("Automated")]
        public void TestCreate_Text
            ([Values(
            ".txt"
            )] string extension,
            [Values] bool funnyCase)
        {
            // Arrange
            if (funnyCase) extension = RandomizeCase(extension);
            using TemporaryFile tempInputFile = new(extension, false);

            // Act
            FilePathHolder filePathHolder = FilePathHolder.Create(tempInputFile.FileName);

            // Assert
            Assert.AreEqual(FileType.Text, filePathHolder.FileType);
            Assert.AreEqual(typeof(TextFile), filePathHolder.GetType());
        }

        // Ensure proper wrapper types are generated for word documents
        [Test, Category("Automated")]
        public void TestCreate_Word
            ([Values(
            ".rtf",
            ".doc",
            ".docx"
            )] string extension,
            [Values] bool funnyCase)
        {
            // Arrange
            if (funnyCase) extension = RandomizeCase(extension);
            using TemporaryFile tempInputFile = new(extension, false);

            // Act
            FilePathHolder filePathHolder = FilePathHolder.Create(tempInputFile.FileName);

            // Assert
            Assert.AreEqual(FileType.Word, filePathHolder.FileType);
            Assert.AreEqual(typeof(WordFile), filePathHolder.GetType());
        }

        // Ensure proper wrapper types are generated for html documents
        [Test, Category("Automated")]
        public void TestCreate_Html
            ([Values(
            ".html",
            ".htm"
            )] string extension,
            [Values] bool funnyCase)
        {
            // Arrange
            if (funnyCase) extension = RandomizeCase(extension);
            using TemporaryFile tempInputFile = new(extension, false);

            // Act
            FilePathHolder filePathHolder = FilePathHolder.Create(tempInputFile.FileName);

            // Assert
            Assert.AreEqual(FileType.Html, filePathHolder.FileType);
            Assert.AreEqual(typeof(HtmlFile), filePathHolder.GetType());
        }

        // Ensure proper wrapper types are generated for excel documents
        [Test, Category("Automated")]
        public void TestCreate_Excel
            ([Values(
            ".xls",
            ".xlsx",
            ".csv",
            ".tsv"
            )] string extension,
            [Values] bool funnyCase)
        {
            // Arrange
            if (funnyCase) extension = RandomizeCase(extension);
            using TemporaryFile tempInputFile = new(extension, false);

            // Act
            FilePathHolder filePathHolder = FilePathHolder.Create(tempInputFile.FileName);

            // Assert
            Assert.AreEqual(FileType.Excel, filePathHolder.FileType);
            Assert.AreEqual(typeof(ExcelFile), filePathHolder.GetType());
        }

        // Ensure proper wrapper types are generated for image documents
        [Test, Category("Automated")]
        public void TestCreate_Image
            ([Values(
            ".tif", ".tiff",
            ".jpg", ".jpeg", ".jpe", ".jif", ".jfif", ".jfi",
            ".jp2", ".j2k", ".jpf", ".jpx", ".jpm",
            ".gif",
            ".png",
            ".bmp", ".dib",
            ".001", ".012", ".123" 
            )] string extension,
            [Values] bool funnyCase)
        {
            // Arrange
            if (funnyCase) extension = RandomizeCase(extension);
            using TemporaryFile tempInputFile = new(extension, false);

            // Act
            FilePathHolder filePathHolder = FilePathHolder.Create(tempInputFile.FileName);

            // Assert
            Assert.AreEqual(FileType.Image, filePathHolder.FileType);
            Assert.AreEqual(typeof(ImageFile), filePathHolder.GetType());
        }

        // Ensure proper wrapper types are generated for pdf documents
        [Test, Category("Automated")]
        public void TestCreate_Pdf
            ([Values(
            ".pdf"
            )] string extension,
            [Values] bool funnyCase)
        {
            // Arrange
            if (funnyCase) extension = RandomizeCase(extension);
            using TemporaryFile tempInputFile = new(extension, false);

            // Act
            FilePathHolder filePathHolder = FilePathHolder.Create(tempInputFile.FileName);

            // Assert
            Assert.AreEqual(FileType.Pdf, filePathHolder.FileType);
            Assert.AreEqual(typeof(PdfFile), filePathHolder.GetType());
        }

        private static string RandomizeCase(string extension)
        {
            var rng = new Random();
            return new string(extension.Select(
                c => rng.Next(2) == 0
                    ? char.ToUpperInvariant(c)
                    : char.ToLowerInvariant(c)
                ).ToArray());
        }
    }
}