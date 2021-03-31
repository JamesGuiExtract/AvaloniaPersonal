using Extract.FileConverter.Converters;
using NUnit.Framework;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Extract.FileConverter.Test
{
    /// <summary>
    /// Provides test cases for the <see cref="DataEntryQuery"/> class.
    /// </summary>
    [TestFixture]
    [Category("TestLaunchArguments")]
    public class TestOfficeConversion
    {
        [Test, Category("Automated")]
        [Parallelizable(ParallelScope.All)]
        [TestCase("Extract.FileConverter.Test.TestWordDocuments.VPNInstructions.docx", TestName = "Convert docx to pdf")]
        [TestCase("Extract.FileConverter.Test.TestWordDocuments.VPNInstructions2003.doc", TestName = "Convert doc to pdf.")]
        [TestCase("Extract.FileConverter.Test.TestWordDocuments.VPNInstructions2003.odt", TestName = "Convert odt to pdf.")]
        [TestCase("Extract.FileConverter.Test.TestWordDocuments.VPNInstructions2003.rtf", TestName = "Convert rtf to pdf.")]
        [TestCase("Extract.FileConverter.Test.TestWordDocuments.VPNInstructions2003.txt", TestName = "Convert txt to pdf.")]
        [TestCase("Extract.FileConverter.Test.TestWordDocuments.VPNInstructions2003.wps", TestName = "Convert wps to pdf.")]
        [TestCase("Extract.FileConverter.Test.TestPowerpointDocuments.Test.odp", TestName = "Convert odp to pdf.")]
        [TestCase("Extract.FileConverter.Test.TestPowerpointDocuments.Test.pps", TestName = "Convert pps to pdf.")]
        [TestCase("Extract.FileConverter.Test.TestPowerpointDocuments.Test.ppt", TestName = "Convert ppt to pdf.")]
        [TestCase("Extract.FileConverter.Test.TestPowerpointDocuments.Test.pptx", TestName = "Convert pptx to pdf.")]
        [TestCase("Extract.FileConverter.Test.TestExcelDocuments.Jan-March.csv", TestName = "Convert csv to pdf.")]
        [TestCase("Extract.FileConverter.Test.TestExcelDocuments.Jan-March.ods", TestName = "Convert ods to pdf.")]
        [TestCase("Extract.FileConverter.Test.TestExcelDocuments.Jan-March.prn", TestName = "Convert prn to pdf.")]
        [TestCase("Extract.FileConverter.Test.TestExcelDocuments.Jan-March.xls", TestName = "Convert xls to pdf.")]
        [TestCase("Extract.FileConverter.Test.TestExcelDocuments.Jan-March.xlsx", TestName = "Convert xlsx to pdf.")]
        public static void ConvertDOCXToPdf(string testItem)
        {
            string fileName = Path.GetTempFileName() + "." + testItem.Split('.').Last();
            try
            {
                WriteResourceToFile(testItem, fileName);
                IConverter[] converters = { new OfficeConverter() };
                PerformConversion.Convert(converters, fileName, FileFormat.Pdf);
                Assert.IsTrue(File.Exists(Path.ChangeExtension(fileName, ".pdf")));
            }
            finally
            {
                File.Delete(fileName);
                File.Delete(Path.ChangeExtension(fileName, ".pdf"));
            }
        }

        [Test, Category("Automated")]
        [Parallelizable(ParallelScope.All)]
        [TestCase("Extract.FileConverter.Test.TestWordDocuments.VPNInstructions.docx", TestName = "Convert docx to tiff")]
        [TestCase("Extract.FileConverter.Test.TestWordDocuments.VPNInstructions2003.doc", TestName = "Convert doc to tiff.")]
        [TestCase("Extract.FileConverter.Test.TestWordDocuments.VPNInstructions2003.odt", TestName = "Convert odt to tiff.")]
        [TestCase("Extract.FileConverter.Test.TestWordDocuments.VPNInstructions2003.rtf", TestName = "Convert rtf to tiff.")]
        [TestCase("Extract.FileConverter.Test.TestWordDocuments.VPNInstructions2003.txt", TestName = "Convert txt to tiff.")]
        [TestCase("Extract.FileConverter.Test.TestWordDocuments.VPNInstructions2003.wps", TestName = "Convert wps to tiff.")]
        [TestCase("Extract.FileConverter.Test.TestPowerpointDocuments.Test.odp", TestName = "Convert odp to tiff.")]
        [TestCase("Extract.FileConverter.Test.TestPowerpointDocuments.Test.pps", TestName = "Convert pps to tiff.")]
        [TestCase("Extract.FileConverter.Test.TestPowerpointDocuments.Test.ppt", TestName = "Convert ppt to tiff.")]
        [TestCase("Extract.FileConverter.Test.TestPowerpointDocuments.Test.pptx", TestName = "Convert pptx to tiff.")]
        [TestCase("Extract.FileConverter.Test.TestExcelDocuments.Jan-March.csv", TestName = "Convert csv to tiff.")]
        [TestCase("Extract.FileConverter.Test.TestExcelDocuments.Jan-March.ods", TestName = "Convert ods to tiff.")]
        [TestCase("Extract.FileConverter.Test.TestExcelDocuments.Jan-March.prn", TestName = "Convert prn to tiff.")]
        [TestCase("Extract.FileConverter.Test.TestExcelDocuments.Jan-March.xls", TestName = "Convert xls to tiff.")]
        [TestCase("Extract.FileConverter.Test.TestExcelDocuments.Jan-March.xlsx", TestName = "Convert xlsx to tiff.")]
        public static void ConvertToTiff(string testItem)
        {
            string fileName = Path.GetTempFileName() + "." + testItem.Split('.').Last();
            try
            {
                WriteResourceToFile(testItem, fileName);
                IConverter[] converters = { new OfficeConverter() };
                PerformConversion.Convert(converters, fileName, FileFormat.Tiff);
                Assert.IsTrue(File.Exists(Path.ChangeExtension(fileName, ".tiff")));
            }
            finally
            {
                File.Delete(fileName);
                File.Delete(Path.ChangeExtension(fileName, ".tiff"));
            }
        }

        [Test, Category("Automated")]
        public static void EnsureErrorForNonSupportedFormat()
        {
            string fileName = Path.GetTempFileName() + ".lol";
            try
            {
                WriteResourceToFile("Extract.FileConverter.Test.TestNonExistantFormat.VPNInstructions2003.lol", fileName);
                IConverter[] converters = { new OfficeConverter() };
                Assert.Throws<ExtractException>(() => PerformConversion.Convert(converters, fileName, FileFormat.Tiff));
            }
            finally
            {
                File.Delete(fileName);
            }
        }

        public static void WriteResourceToFile(string resourceName, string fileName)
        {
            using var resource = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
            using var file = new FileStream(fileName, FileMode.Create, FileAccess.Write);
            resource.CopyTo(file);
        }
    }
}