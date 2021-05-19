using Extract.Licensing;
using NUnit.Framework;
using System.IO;
using System.Linq;
using UCLID_FILEPROCESSINGLib;

namespace Extract.FileConverter.Test
{
    /// <summary>
    /// Provides test cases for the <see cref="TestKofaxConverter"/> class.
    /// </summary>
    [TestFixture]
    [Category("TestConverters")]
    public class TestConvertDocumentTask
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
        public static void CheckForNamespaceChange()
        {
            string testItem = "Extract.FileConverter.Test.UnitTestNoNamespaceChange.fps";
            string fileName = Path.GetTempFileName() + "." + testItem.Split('.').Last();
            try
            {
                Utility.WriteResourceToFile(testItem, fileName);
                FileProcessingManagerClass fileProcessingManager = new();
                fileProcessingManager.LoadFrom(fileName, false);
                fileProcessingManager.SaveTo(fileName, true);
            }
            finally
            {
                File.Delete(fileName);
            }
        }
    }
}
