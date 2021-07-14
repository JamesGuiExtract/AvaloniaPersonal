using Extract.Licensing;
using Extract.Testing.Utilities;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Extract.FileConverter.Test
{
    /// <summary>
    /// Provides test cases for the <see cref="TestKofaxConverter"/> class.
    /// </summary>
    [TestFixture]
    [Category("TestConverters")]
    public class TestKofaxConverter
    {
        /// <summary>
        /// Setup method to initialize the testing environment.
        /// </summary>
        [OneTimeSetUp]
        public static void Setup()
        {
            LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());
        }

        public static IEnumerable<TestCaseData> TestCasesPdf => new[]
        {
             new TestCaseData(new KofaxModel() { Color = false, Compression = -1, PageNumber = -1, RemovePages = string.Empty, SpecifiedCompressionFormat = KofaxFileFormat.None }),
             new TestCaseData(new KofaxModel() { Color = true, Compression = -1, PageNumber = -1, RemovePages = string.Empty, SpecifiedCompressionFormat = KofaxFileFormat.None }),
             new TestCaseData(new KofaxModel() { Color = false, Compression = 1, PageNumber = -1, RemovePages = string.Empty, SpecifiedCompressionFormat = KofaxFileFormat.Pdf }),
             new TestCaseData(new KofaxModel() { Color = false, Compression = -1, PageNumber = 1, RemovePages = string.Empty, SpecifiedCompressionFormat = KofaxFileFormat.None }),
             new TestCaseData(new KofaxModel() { Color = false, Compression = -1, PageNumber = -1, RemovePages = string.Empty, SpecifiedCompressionFormat = KofaxFileFormat.None }),
             new TestCaseData(new KofaxModel() { Color = true, Compression = 1, PageNumber = -1, RemovePages = string.Empty, SpecifiedCompressionFormat = KofaxFileFormat.PdfMixedRasterContent }),
             new TestCaseData(new KofaxModel() { Color = true, Compression = 5, PageNumber = 1, RemovePages = string.Empty, SpecifiedCompressionFormat = KofaxFileFormat.Pdf })
        };

        public static IEnumerable<TestCaseData> TestCasesTiff => new[]
        {
             new TestCaseData(new KofaxModel() { Color = false, Compression = -1, PageNumber = -1, RemovePages = string.Empty, SpecifiedCompressionFormat = KofaxFileFormat.None }),
             new TestCaseData(new KofaxModel() { Color = true, Compression = -1, PageNumber = -1, RemovePages = string.Empty, SpecifiedCompressionFormat = KofaxFileFormat.None }),
             new TestCaseData(new KofaxModel() { Color = false, Compression = 1, PageNumber = -1, RemovePages = string.Empty, SpecifiedCompressionFormat = KofaxFileFormat.TifG31 }),
             new TestCaseData(new KofaxModel() { Color = false, Compression = -1, PageNumber = 1, RemovePages = string.Empty, SpecifiedCompressionFormat = KofaxFileFormat.None }),
             new TestCaseData(new KofaxModel() { Color = false, Compression = -1, PageNumber = -1, RemovePages = string.Empty, SpecifiedCompressionFormat = KofaxFileFormat.None }),
             new TestCaseData(new KofaxModel() { Color = true, Compression = 1, PageNumber = -1, RemovePages = string.Empty, SpecifiedCompressionFormat = KofaxFileFormat.TifG32 }),
             new TestCaseData(new KofaxModel() { Color = true, Compression = 5, PageNumber = 1, RemovePages = string.Empty, SpecifiedCompressionFormat = KofaxFileFormat.TifNo })
        };

        [Test, Category("Automated")]
        [Parallelizable(ParallelScope.All)]
        [TestCaseSource(nameof(TestCasesPdf))]
        public static void ConvertTifToPdf(KofaxModel kofaxModel)
        {
            using TestFileManager<TestKofaxConverter> testFiles = new();
            string fileName = testFiles.GetFile("TestTiffDocuments.0275pages.tif");
            IConverter[] converters = { new KofaxConverter()
                {
                    IsEnabled = true,
                    KofaxModel = kofaxModel
                } };
            PerformConversion.Convert(converters, fileName, DestinationFileFormat.Pdf);
            Assert.IsTrue(File.Exists(fileName + ".pdf"));
        }

        [Test, Category("Automated")]
        [Parallelizable(ParallelScope.All)]
        [TestCaseSource(nameof(TestCasesTiff))]
        public static void ConvertPdfToTif(KofaxModel kofaxModel)
        {
            using TestFileManager<TestKofaxConverter> testFiles = new();
            string fileName = testFiles.GetFile("TestPDFDocuments.0003.pdf");
            IConverter[] converters = { new KofaxConverter()
                {
                    IsEnabled = true,
                    KofaxModel = kofaxModel
                } };
            PerformConversion.Convert(converters, fileName, DestinationFileFormat.Tif);
            Assert.IsTrue(File.Exists(fileName + ".tif"));
        }
    }
}
