using Extract.Licensing;
using Extract.Testing.Utilities;
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
            using TestFileManager<TestConvertDocumentTask> testFiles = new();
            string fileName = testFiles.GetFile("UnitTestNoNamespaceChange.fps");
            FileProcessingManagerClass fileProcessingManager = new();
            fileProcessingManager.LoadFrom(fileName, false);
            fileProcessingManager.SaveTo(fileName, true);
        }
    }
}
