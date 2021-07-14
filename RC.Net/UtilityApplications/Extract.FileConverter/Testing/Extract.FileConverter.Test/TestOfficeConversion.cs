using Extract.Licensing;
using Extract.Testing.Utilities;
using NUnit.Framework;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Extract.FileConverter.Test
{
    /// <summary>
    /// Provides test cases for the <see cref="OfficeConverter"/> class.
    /// </summary>
    [TestFixture]
    [Category("TestConverters")]
    public class TestOfficeConversion
    {
        /// <summary>
        /// Setup method to initialize the testing environment.
        /// </summary>
        [OneTimeSetUp]
        public static void Setup()
        {
            LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());
        }

        [Test, Category("Automated"), Category("OfficeRequired")]
        [Parallelizable(ParallelScope.All)]
        [TestCase("TestWordDocuments.VPNInstructions.docx", TestName = "Convert docx to pdf")]
        [TestCase("TestWordDocuments.VPNInstructions2003.doc", TestName = "Convert doc to pdf.")]
        [TestCase("TestWordDocuments.VPNInstructions2003.odt", TestName = "Convert odt to pdf.")]
        [TestCase("TestWordDocuments.VPNInstructions2003.rtf", TestName = "Convert rtf to pdf.")]
        [TestCase("TestWordDocuments.VPNInstructions2003.txt", TestName = "Convert txt to pdf.")]
        [TestCase("TestWordDocuments.VPNInstructions2003.wps", TestName = "Convert wps to pdf.")]
        [TestCase("TestPowerpointDocuments.Test.odp", TestName = "Convert odp to pdf.")]
        [TestCase("TestPowerpointDocuments.Test.pps", TestName = "Convert pps to pdf.")]
        [TestCase("TestPowerpointDocuments.Test.ppt", TestName = "Convert ppt to pdf.")]
        [TestCase("TestPowerpointDocuments.Test.pptx", TestName = "Convert pptx to pdf.")]
        [TestCase("TestExcelDocuments.Jan-March.csv", TestName = "Convert csv to pdf.")]
        [TestCase("TestExcelDocuments.Jan-March.ods", TestName = "Convert ods to pdf.")]
        [TestCase("TestExcelDocuments.Jan-March.prn", TestName = "Convert prn to pdf.")]
        [TestCase("TestExcelDocuments.Jan-March.xls", TestName = "Convert xls to pdf.")]
        [TestCase("TestExcelDocuments.Jan-March.xlsx", TestName = "Convert xlsx to pdf.")]
        public static void ConvertFilesToPdf(string resource)
        {
            using TestFileManager<TestOfficeConversion> testFiles = new();
            string fileName = testFiles.GetFile(resource);
            IConverter[] converters = { new OfficeConverter() { IsEnabled = true } };
            PerformConversion.Convert(converters, fileName, DestinationFileFormat.Pdf);
            Assert.IsTrue(File.Exists(fileName + ".pdf"));
        }

        [Test, Category("Automated"), Category("OfficeRequired")]
        [Parallelizable(ParallelScope.All)]
        [TestCase("TestWordDocuments.VPNInstructions.docx", TestName = "Convert docx to tiff")]
        [TestCase("TestWordDocuments.VPNInstructions2003.doc", TestName = "Convert doc to tiff.")]
        [TestCase("TestWordDocuments.VPNInstructions2003.odt", TestName = "Convert odt to tiff.")]
        [TestCase("TestWordDocuments.VPNInstructions2003.rtf", TestName = "Convert rtf to tiff.")]
        [TestCase("TestWordDocuments.VPNInstructions2003.txt", TestName = "Convert txt to tiff.")]
        [TestCase("TestWordDocuments.VPNInstructions2003.wps", TestName = "Convert wps to tiff.")]
        [TestCase("TestPowerpointDocuments.Test.odp", TestName = "Convert odp to tiff.")]
        [TestCase("TestPowerpointDocuments.Test.pps", TestName = "Convert pps to tiff.")]
        [TestCase("TestPowerpointDocuments.Test.ppt", TestName = "Convert ppt to tiff.")]
        [TestCase("TestPowerpointDocuments.Test.pptx", TestName = "Convert pptx to tiff.")]
        [TestCase("TestExcelDocuments.Jan-March.csv", TestName = "Convert csv to tiff.")]
        [TestCase("TestExcelDocuments.Jan-March.ods", TestName = "Convert ods to tiff.")]
        [TestCase("TestExcelDocuments.Jan-March.prn", TestName = "Convert prn to tiff.")]
        [TestCase("TestExcelDocuments.Jan-March.xls", TestName = "Convert xls to tiff.")]
        [TestCase("TestExcelDocuments.Jan-March.xlsx", TestName = "Convert xlsx to tiff.")]
        public static void ConvertToTiff(string resource)
        {
            using TestFileManager<TestOfficeConversion> testFiles = new();
            string fileName = testFiles.GetFile(resource);
            IConverter[] converters = { new OfficeConverter() { IsEnabled = true } };
            PerformConversion.Convert(converters, fileName, DestinationFileFormat.Tif);
            Assert.IsTrue(File.Exists(fileName + ".tif"));
        }

        [Test, Category("Automated"), Category("OfficeRequired")]
        public static void EnsureErrorForNonSupportedFormat()
        {
            using TestFileManager<TestOfficeConversion> testFiles = new();
            string fileName = testFiles.GetFile("TestNonExistantFormat.VPNInstructions2003.lol");
            IConverter[] converters = { new OfficeConverter() { IsEnabled = true } };
            Assert.Throws<ExtractException>(() => PerformConversion.Convert(converters, fileName, DestinationFileFormat.Tif));
        }
    }
}