using Extract.Imaging.Utilities;
using Extract.Testing.Utilities;
using Extract.Utilities;
using NUnit.Framework;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static Extract.Testing.Utilities.ImageUtils;

namespace Extract.FileConverter.ConvertToPdf.Test
{
    // Class for testing the ConvertToPdf functionality
    [TestFixture]
    [Category("ConvertToPdf")]
    public class TestConvertToPdf
    {
        TestFileManager<TestConvertToPdf> _testFiles;

        // Performs initialization needed for the entire test run.
        [OneTimeSetUp]
        public static void Initialize()
        {
            GeneralMethods.TestSetup();
            UnlockLeadtools.UnlockPdfSupport(false);
        }

        // Performs initialization needed for each test
        [SetUp]
        public void Setup()
        {
            _testFiles = new TestFileManager<TestConvertToPdf>();
        }

        // Performs tear down needed after each test run.
        [TearDown]
        public void Teardown()
        {
            _testFiles?.Dispose();
            _testFiles = null;
        }

        // Compare the result of conversion to the expected document
        [Test, Category("Automated")]
        [Sequential]
        public void Convert_VerifyResultWithImageComparison
            ([Values(
            "MarketingEmail.odt",
            "LoremIpsum.txt",
            "EmailWithPDFAttachment.eml",
            "FW Test embedded HTML 3 EEZ 2022-04-26-23-16.eml"
            )] string inputResource,

            [Values(
            "MarketingEmail.odt.pdf",
            "LoremIpsum.txt.pdf",
            "EmailWithPDFAttachment.eml.pdf",
            "FW Test embedded HTML 3 EEZ 2022-04-26-23-16.eml.pdf"
            )] string expectedResource)
        {
            // Arrange
            string inputFile = _testFiles.GetFile("Resources." + inputResource);
            using var expected = new PDF(_testFiles.GetFile("Resources." + expectedResource));
            using TemporaryFile tempOutputFile = new(".pdf", false);
            using var actual = new PDF(tempOutputFile.FileName);

            // Act
            bool success = MimeKitEmailToPdfConverter.CreateDefault()
                .Convert(FilePathHolder.Create(inputFile), new PdfFile(tempOutputFile.FileName));

            // Assert
            Assert.That(success, Is.True);
            double errors = ComparePagesAsImages(expected, actual);
            Assert.That(errors, Is.LessThan(1E-6));
        }

        [Test, Category("Automated")]
        [Sequential]
        [Ignore("Need to fix comparison code, I think")]
        public void Convert_VerifyResultWithImageComparison_TemporarilyBroken
            ([Values(
            "MarketingEmail.html",
            "MarketingEmail.docx"
            )] string inputResource,

            [Values(
            "MarketingEmail.html.pdf",
            "MarketingEmail.docx.pdf"
            )] string expectedResource)
        {
            // Arrange
            string inputFile = _testFiles.GetFile("Resources." + inputResource);
            using var expected = new PDF(_testFiles.GetFile("Resources." + expectedResource));
            using TemporaryFile tempOutputFile = new(".pdf", false);
            using var actual = new PDF(tempOutputFile.FileName);

            // Act
            bool success = MimeKitEmailToPdfConverter.CreateDefault()
                .Convert(FilePathHolder.Create(inputFile), new PdfFile(tempOutputFile.FileName));

            // Assert
            Assert.That(success, Is.True);
            double errors = ComparePagesAsImages(expected, actual);
            Assert.That(errors, Is.LessThan(1E-6));
        }

        // Compare the result inconsistent conversions
        // (devexpress isn't consistent with the way it converts spreadsheets, e.g.)
        [Test, Category("Automated")]
        [Sequential]
        public void Convert_VerifyResult_HighVarianceConversions
            ([Values(
            "StatsData.xls"
            )] string inputResource,

            [Values(
            "StatsData.xls.pdf"
            )] string expectedResource)
        {
            // Arrange
            string inputFile = _testFiles.GetFile("Resources." + inputResource);
            using var expected = new PDF(_testFiles.GetFile("Resources." + expectedResource));
            using TemporaryFile tempOutputFile = new(".pdf", false);
            using var actual = new PDF(tempOutputFile.FileName);

            // Act
            bool success = FileToPdfConverter.CreateDefault().Convert(inputFile, tempOutputFile.FileName);

            // Assert
            Assert.That(success, Is.True);
            double errors = ComparePagesAsImages(expected, actual);
            Assert.That(errors, Is.LessThan(0.003));
        }

        // Verify the special doc and page num tags are added for each PDF page
        // (these are to be used for pagination)
        [Test, Category("Automated")]
        public void Convert_VerifyPaginationTags()
        {
            // Arrange
            string inputFile = _testFiles.GetFile("Resources.EmailWithPDFAttachment.eml");
            using TemporaryFile tempOutputFile = new(".pdf", false);

            // Act
            bool success = MimeKitEmailToPdfConverter.CreateDefault()
                .Convert(FilePathHolder.Create(inputFile), new PdfFile(tempOutputFile.FileName));

            // Assert
            Assert.That(success, Is.True);

            List<(int, int)> expected = new() { (1, 1), (2, 1), (2, 2) };
            List<(int, int)> actual = GetLogicalDocumentAndPageTags(tempOutputFile.FileName);

            CollectionAssert.AreEqual(expected, actual);

            static List<(int, int)> GetLogicalDocumentAndPageTags(string pdfFile)
            {
                using PdfDocument document = PdfReader.Open(pdfFile);

                return document.Pages.Cast<PdfPage>().Select(page =>
                    (((PdfInteger)page.Elements["/ExtractSystems.LogicalDocumentNumber"]).Value,
                    ((PdfInteger)page.Elements["/ExtractSystems.LogicalPageNumber"]).Value)
                ).ToList();
            }
        }

        // Compare the result of slightly lossy conversion to the source document
        [Test, Category("Automated")]
        [Sequential]
        public void Convert_VerifyResult_Lossy
            ([Values(
            "StatsData.xls.pdf.tif"
            )] string inputResource,

            [Values(
            "StatsData.xls.pdf"
            )] string expectedResource)
        {
            // Arrange
            string inputFile = _testFiles.GetFile("Resources." + inputResource);
            using var expected = new PDF(_testFiles.GetFile("Resources." + expectedResource));
            using TemporaryFile tempOutputFile = new(".pdf", false);
            using var actual = new PDF(tempOutputFile.FileName);

            // Act
            bool success = FileToPdfConverter.CreateDefault().Convert(inputFile, tempOutputFile.FileName);

            // Assert
            Assert.That(success, Is.True);
            double errors = ComparePagesAsImages(expected, actual);
            Assert.That(errors, Is.LessThan(0.06));
        }

        // Compare the result of GdPicture conversion to the source document
        [Test, Category("Automated")]
        [Sequential]
        public void Convert_GdPicture_VerifyResultWithImageComparison
            ([Values(
            "StatsData.xls.pdf.tif"
            )] string inputResource,

            [Values(
            "StatsData.xls.pdf"
            )] string expectedResource)
        {
            // Arrange
            string inputFile = _testFiles.GetFile("Resources." + inputResource);
            using var expected = new PDF(_testFiles.GetFile("Resources." + expectedResource));
            using TemporaryFile tempOutputFile = new(".pdf", false);
            using var actual = new PDF(tempOutputFile.FileName);

            // Act
            bool success = new FileToPdfConverter(new GdPictureImageToPdfConverter()).Convert(inputFile, tempOutputFile.FileName);

            // Assert
            Assert.That(success, Is.True);
            double errors = ComparePagesAsImages(expected, actual);
            Assert.That(errors, Is.LessThan(0.06));
        }

        // Test outlier row removing logic
        [Test, Category("Automated")]
        public void Convert_SpreadsheetWithOutlierRow()
        {
            // Arrange
            string inputFile = _testFiles.GetFile("Resources.OutlierRow.xlsx");
            using TemporaryFile tempOutputFile = new(".pdf", false);

            // Act
            bool success = FileToPdfConverter.CreateDefault().Convert(inputFile, tempOutputFile.FileName);

            // Assert
            Assert.That(success, Is.True);
            int pageCount = UtilityMethods.GetNumberOfPagesInImage(tempOutputFile.FileName);
            Assert.AreEqual(11, pageCount);
        }

        // Confirm that outlier row removing logic is very conservative
        [Test, Category("Automated")]
        public void Convert_SpreadsheetWithMultipleOutlierRowsFails()
        {
            // Arrange
            string inputFile = _testFiles.GetFile("Resources.MultipleOutlierRows.xlsx");
            using TemporaryFile tempOutputFile = new(".pdf", false);

            // Act
            bool success = FileToPdfConverter.CreateDefault().Convert(inputFile, tempOutputFile.FileName);

            // Assert
            Assert.That(success, Is.False);
        }

        // Confirm that an Unknown file can be converted
        [Test, Category("Automated")]
        public void Convert_UnknownFile()
        {
            // Arrange
            string inputFile = _testFiles.GetFile("Resources.bozo.svg");
            using var expected = new PDF(_testFiles.GetFile("Resources.bozo.svg.pdf"));
            using TemporaryFile tempOutputFile = new(".pdf", false);
            using var actual = new PDF(tempOutputFile.FileName);
            Assume.That(FilePathHolder.Create(inputFile), Is.AssignableTo(typeof(UnknownFile)));

            // Act
            bool success = FileToPdfConverter.CreateDefault().Convert(inputFile, tempOutputFile.FileName);

            // Assert
            Assert.That(success, Is.True);
            double errors = ComparePagesAsImages(expected, actual);
            Assert.That(errors, Is.LessThan(1E-6));
        }

        // Confirm that no conversion happens when the input is already a PDF file
        [Test, Category("Automated")]
        public void Convert_PdfInputReturnsFalse()
        {
            // Arrange
            string inputFile = _testFiles.GetFile("Resources.StatsData.xls.pdf");
            using TemporaryFile tempOutputFile = new(".pdf", false);
            File.Delete(tempOutputFile.FileName);

            // Act
            bool success = FileToPdfConverter.CreateDefault().Convert(inputFile, tempOutputFile.FileName);

            // Assert
            Assert.That(success, Is.False);
            FileAssert.DoesNotExist(tempOutputFile.FileName);
        }

        // Confirm the default configuration
        [Test, Category("Automated")]
        public void CreateDefault_ConverterIsConfiguredCorrectly()
        {
            // Arrange

            // The converters returned by EnumerateConverters are sorted by FileType, then precedence
            // FileType enum order is: Unknown, Image, Pdf, Text, Html, Word, Excel
            var expectedOrder = new[]
            {
                // The aggregate converter is returned first
                typeof(FileToPdfConverter),

                // Unknown
                typeof(KofaxImageToPdfConverter),
                typeof(LeadToolsImageToPdfConverter),
                typeof(GdPictureImageToPdfConverter),
             
                // Image
                typeof(KofaxImageToPdfConverter),
                typeof(LeadToolsImageToPdfConverter),
                typeof(GdPictureImageToPdfConverter),
             
                // Text
                typeof(WKHtmlToPdfConverter),
                typeof(DevExpressOfficeToPdfConverter),
             
                // Html
                typeof(WKHtmlToPdfConverter),
                typeof(DevExpressOfficeToPdfConverter),
             
                // Word
                typeof(DevExpressOfficeToPdfConverter),
             
                // Excel
                typeof(DevExpressOfficeToPdfConverter)
            };

            // Act
            var fileToPdfConverter = FileToPdfConverter.CreateDefault();

            // Assert
            var actualOrder = fileToPdfConverter.EnumerateConverters()
                .Select(c => c.GetType())
                .ToList();

            // The order is important
            CollectionAssert.AreEqual(expectedOrder, actualOrder);
        }

        // Confirm serialize/deserialize works correctly
        [Test, Category("Automated")]
        public void Serialize_ConfirmJsonRoundTrip()
        {
            // Arrange
            var origConverter = FileToPdfConverter.CreateDefault();
            var expectedOrder = origConverter.EnumerateConverters().Select(c => c.GetType()).ToList();

            // This serializer can handle any IDataTransferObject defined in the supplied assemblies
            var serializer = new DataTransferObjectSerializer(typeof(FileToPdfConverter).Assembly);

            // Act
            string json = serializer.Serialize(origConverter.CreateDataTransferObject());
            var deserialized = (FileToPdfConverter)serializer.Deserialize(json).CreateDomainObject();

            // Assert
            var actualOrder = deserialized.EnumerateConverters().Select(c => c.GetType()).ToList();

            // The order is important
            CollectionAssert.AreEqual(expectedOrder, actualOrder);
        }
    }
}