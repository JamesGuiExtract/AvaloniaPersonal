using ESConvertToPDF.Test;
using Extract.Imaging.Utilities;
using Extract.Testing.Utilities;
using Extract.Utilities;
using NUnit.Framework;
using System.IO;
using System.Linq;
using static Extract.Testing.Utilities.ImageUtils;

namespace Extract.Imaging.Test
{
    [TestFixture, Category("ImageMethods")]
    public class ImageMethodsTest
    {
        TestFileManager<ImageMethodsTest> _testFiles;

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
            _testFiles = new();
        }

        // Performs tear down needed after each test run.
        [TearDown]
        public void Teardown()
        {
            _testFiles?.Dispose();
            _testFiles = null;
        }

        static ImagePage[] _copyFirstPage_0_90 = new ImagePage[]
        {
            new ImagePage("MarketingEmail.html.pdf", 1, 0),
            new ImagePage("MarketingEmail.html.pdf", 1, 90)
        };

        [Test, Category("Automated")]
        public void StaplePagesAsNewDocument_VerifyOutputWithImageComparison_PDF()
        {
            // Arrange
            var inputPageSpec = _copyFirstPage_0_90
                .Select(page => new ImagePage(_testFiles.GetFile("Resources." + page.DocumentName), page.PageNumber, page.ImageOrientation))
                .ToList();

            using PDF expected = new(_testFiles.GetFile("Resources.MarketingEmail_0_90.pdf"));
            using TemporaryFile tempOutputFile = new(".pdf", false);

            // Act
            ImageMethods.StaplePagesAsNewDocument(inputPageSpec, tempOutputFile.FileName);

            // Assert
            using PDF actual = new(tempOutputFile.FileName);
            Assert.That(new FileInfo(actual.Path).Length, Is.LessThan(100_000));

            double errors = ComparePagesAsImages(expected, actual);
            Assert.That(errors, Is.LessThan(1E-6));
        }

        [Test, Category("Automated")]
        public void StaplePagesAsNewDocument_VerifyOutputWithImageComparison_PDF_DoubleRotation()
        {
            // Arrange
            ImagePage[] _copyPages_0_90plus180 = new ImagePage[]
            {
                new ImagePage("MarketingEmail_0_90.pdf", 1, 0),
                new ImagePage("MarketingEmail_0_90.pdf", 2, 180)
            };

            var inputPageSpec = _copyPages_0_90plus180
                .Select(page => new ImagePage(_testFiles.GetFile("Resources." + page.DocumentName), page.PageNumber, page.ImageOrientation))
                .ToList();

            using PDF expected = new(_testFiles.GetFile("Resources.MarketingEmail_0_270.pdf"));
            using TemporaryFile tempOutputFile = new(".pdf", false);

            // Act
            ImageMethods.StaplePagesAsNewDocument(inputPageSpec, tempOutputFile.FileName);

            // Assert
            using PDF actual = new(tempOutputFile.FileName);
            Assert.That(new FileInfo(actual.Path).Length, Is.LessThan(100_000));

            double errors = ComparePagesAsImages(expected, actual);
            Assert.That(errors, Is.LessThan(1E-6));
        }


        [Test, Category("Automated")]
        public void StaplePagesAsNewDocument_VerifyOutputWithImageComparison_TIF()
        {
            // Arrange
            var inputPageSpec = _copyFirstPage_0_90
                .Select(page => new ImagePage(_testFiles.GetFile("Resources." + page.DocumentName), page.PageNumber, page.ImageOrientation))
                .ToList();

            using TIF expected = new(_testFiles.GetFile("Resources.MarketingEmail_0_90.tif"));
            using TemporaryFile tempOutputFile = new(".tif", false);

            // Act
            ImageMethods.StaplePagesAsNewDocument(inputPageSpec, tempOutputFile.FileName);

            // Assert
            using TIF actual = new(tempOutputFile.FileName);
            double errors = ComparePagesAsImages(expected, actual);
            Assert.That(errors, Is.LessThan(1E-3));
        }
    }
}