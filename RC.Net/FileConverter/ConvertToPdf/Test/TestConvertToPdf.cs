using ESConvertToPDF.Test;
using Extract.Imaging.Utilities;
using Extract.Testing.Utilities;
using Extract.Utilities;
using NUnit.Framework;
using System.IO;
using System.Linq;
using static ESConvertToPDF.Test.ImageUtils;

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
            "StatsData.xls",
            "MarketingEmail.html",
            "MarketingEmail.docx",
            "LoremIpsum.txt"
            )] string inputResource,

            [Values(
            "StatsData.xls.pdf",
            "MarketingEmail.html.pdf",
            "MarketingEmail.docx.pdf",
            "LoremIpsum.txt.pdf"
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
            Assert.That(errors, Is.LessThan(1E-6));
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
             
                // Image
                typeof(KofaxImageToPdfConverter),
                typeof(LeadToolsImageToPdfConverter),
             
                // Text
                typeof(WKHtmlToPdfConverter),
                typeof(DevExpressOfficeToPdfConverter),
             
                // Html
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