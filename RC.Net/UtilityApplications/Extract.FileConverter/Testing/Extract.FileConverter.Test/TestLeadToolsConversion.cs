using Extract.Licensing;
using NUnit.Framework;
using System.IO;
using System.Linq;

namespace Extract.FileConverter.Test
{
    /// <summary>
    /// Provides test cases for the <see cref="LeadtoolsConverter"/> class.
    /// </summary>
    [TestFixture]
    [Category("TestConverters")]
    public class TestLeadtoolsConversion
    {
        /// <summary>
        /// Setup method to initialize the testing environment.
        /// </summary>
        [OneTimeSetUp]
        public static void Setup()
        {
            LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());
        }

        [Test, Category("Automated")]
        [Parallelizable(ParallelScope.All)]
        [TestCase(-1, false, "", TestName = "Base case no arguments")]
        [TestCase(1, false, "", TestName = "PerspectiveID set")]
        [TestCase(-1, true, "", TestName = "Retain set")]
        [TestCase(-1, false, "-1", TestName = "RemovePages set")]
        [TestCase(1, true, "-1", TestName = "All three set")]
        [TestCase(1, true, "", TestName = "First two set")]
        [TestCase(-1, true, "-1", TestName = "Last two set")]
        public static void ConvertTifToPdf(int perspectiveID, bool retain, string removePages)
        {
            string testItem = "Extract.FileConverter.Test.TestTiffDocuments.0275pages.tif";
            string fileName = Path.GetTempFileName() + "." + testItem.Split('.').Last();
            try
            {
                Utility.WriteResourceToFile(testItem, fileName);
                IConverter[] converters = { new LeadtoolsConverter()
                {
                    IsEnabled = true,
                    LeadtoolsModel = new LeadtoolsModel()
                        {
                            PerspectiveID = perspectiveID,
                            Retain = retain,
                            RemovePages = removePages
                        }
                } };
                PerformConversion.Convert(converters, fileName, DestinationFileFormat.Pdf);
                Assert.IsTrue(File.Exists(fileName + ".pdf"));
            }
            finally
            {
                File.Delete(fileName);
                Assert.IsTrue(File.Exists(fileName + ".pdf"));
            }
        }

        [Test, Category("Automated")]
        [Parallelizable(ParallelScope.All)]
        [TestCase("Extract.FileConverter.Test.TestPDFDocuments.0003.pdf", TestName = "Convert pdf to tif 003")]
        [TestCase("Extract.FileConverter.Test.TestPDFDocuments.004.pdf", TestName = "Convert pdf to tif 004")]
        public static void ConvertPdfToTiff(string testItem)
        {
            string fileName = Path.GetTempFileName() + "." + testItem.Split('.').Last();
            try
            {
                Utility.WriteResourceToFile(testItem, fileName);
                IConverter[] converters = { new LeadtoolsConverter() { IsEnabled = true } };
                PerformConversion.Convert(converters, fileName, DestinationFileFormat.Tif);
                Assert.IsTrue(File.Exists(fileName + ".tif"));
            }
            finally
            {
                File.Delete(fileName);
                Assert.IsTrue(File.Exists(fileName + ".tif"));
            }
        }
    }
}