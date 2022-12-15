using Extract.Testing.Utilities;
using NUnit.Framework;
using UCLID_COMUTILSLib;

namespace Extract.FileActionManager.FileProcessing.Test
{
    [TestFixture]
    [Category("TagExpansion")]
    public class TestRelativePathPartsFunction
    {
        /// <summary>
        /// Performs initialization needed for the entire test run.
        /// </summary>
        [OneTimeSetUp]
        public static void Setup()
        {
            GeneralMethods.TestSetup();
        }

        /// <summary>
        /// Performs tear down needed after entire test run.
        /// </summary>
        [OneTimeTearDown]
        public static void FinalCleanup()
        {
        }

        /// <summary>
        /// Happy cases for $RelativePathParts with no specified part numbers
        /// </summary>
        [Category("Automated")]
        [TestCase(
            @"D:\Workflow\Input",
            @"D:\Workflow\Input\SubFolder1\Subfolder2\FileName.tif",
            ExpectedResult = @"SubFolder1\Subfolder2\FileName.tif",
            TestName = "RelativePathParts Good(Simple paths with drive letter)")]
        [TestCase(
            @"D:\\\\\WOrkflow\InPut\",
            @"D:\WorKflow\\Input\SubFolder1\Subfolder2\\FileName.tif",
            ExpectedResult = @"SubFolder1\Subfolder2\FileName.tif",
            TestName = "RelativePathParts Good(Extra back-slashes and mismatched letter cases in paths)")]
        [TestCase(
            @"\\Server\Workflow\Input",
            @"\\Server\Workflow\Input\SubFolder1\Subfolder2\FileName.tif",
            ExpectedResult = @"SubFolder1\Subfolder2\FileName.tif",
            TestName = "RelativePathParts Good(UNC path)")]
        [TestCase(
            @"\\Server\\Workflow\Input\\\",
            @"\\Server\Workflow\\Input\SubFolder1\Subfolder2\\FileName.tif",
            ExpectedResult = @"SubFolder1\Subfolder2\FileName.tif",
            TestName = "RelativePathParts Good(UNC path with extra back-slashes in paths)")]
        [TestCase(
            @"\\Server\Workflow\Input\OtherSubfolder1\..\OtherSubfolder2\..",
            @"\\Server\Workflow\Input\SubFolder1\Subfolder2\Subfolder3\..\FileName.tif",
            ExpectedResult = @"SubFolder1\Subfolder2\FileName.tif",
            TestName = "RelativePathParts Good(Relative components in paths)")]
        [TestCase(
            @"\\Server",
            @"\\Server\Workflow\Output\FileName.tif",
            ExpectedResult = @"Workflow\Output\FileName.tif",
            TestName = "RelativePathParts Good(Base is only a server name)")]
        [TestCase(
            "",
            @"\\Server\Workflow\Output\FileName.tif",
            ExpectedResult = @"\\Server\Workflow\Output\FileName.tif",
            TestName = "RelativePathParts Good(Base is empty)")]
        [TestCase(
            @"\\Server\Workflow\Input",
            @"\\Server\Workflow\Input\Folder1\Folder2\Folder3\..\..\FileName.tif",
            TestName = "RelativePathParts Good(Target is absolute but has parent folder operations)",
            ExpectedResult = @"Folder1\FileName.tif")]
        public static string ValidCases(string basePath, string targetPath)
        {
            try
            {
                // Arrange
                MiscUtilsClass miscUtils = new();
                miscUtils.AddTag("<BasePath>", basePath);
                miscUtils.AddTag("<TargetPath>", targetPath);

                // Act
                string result = miscUtils.ExpandTagsAndFunctions("$RelativePathParts(<BasePath>,<TargetPath>)", "", null);

                // Assert
                return result;
            }
            catch (System.Runtime.InteropServices.COMException ex)
            {
                throw ex.AsExtract("ELI53569");
            }
        }

        /// <summary>
        /// Unhappy cases for $RelativePathParts with no specified part numbers
        /// </summary>
        [Category("Automated")]
        [TestCase(
            @"\\Server1\Workflow\Input",
            @"\\Server2\Workflow\Output\FileName.tif",
            ExpectedResult = "Runtime error: Base path is not a prefix of target path!",
            TestName = "RelativePathParts Bad(Target on a different server than base)")]
        [TestCase(
            @"\\Server\Workflow\Input",
            @"D:\Workflow\Output\FileName.tif",
            ExpectedResult = "Runtime error: Base path is not a prefix of target path!",
            TestName = "RelativePathParts Bad(Base is UNC but target is not)")]
        [TestCase(
            @"C:\Workflow\Input",
            @"\\Server\Workflow\Output\FileName.tif",
            ExpectedResult = "Runtime error: Base path is not a prefix of target path!",
            TestName = "RelativePathParts Bad(Target is UNC but base is not)")]
        [TestCase(
            @"\\Server\Workflow\Input\SubFolder1\SubFolder2",
            @"\\Server\Workflow\FileName.tif",
            ExpectedResult = "Runtime error: Target path cannot be shorter than base path!",
            TestName = "RelativePathParts Bad(Target in parent folder of base)")]
        [TestCase(
            @"\\Server\Workflow\Input",
            @"\\Server\Workflow\Output\FileName.tif",
            ExpectedResult = "Runtime error: Base path is not a prefix of target path!",
            TestName = "RelativePathParts Bad(Target in a different branch than base)")]
        [TestCase(
            @"Workflow\Input",
            @"Workflow\Input\Folder1\Folder2\..\..\FileName.tif",
            ExpectedResult = "Runtime error: Could not make relative path!",
            TestName = "RelativePathParts Bad(Target is not absolute but has parent folder operations)")]
        public static string InvalidCases(string basePath, string targetPath)
        {
            // Arrange
            MiscUtilsClass miscUtils = new();
            miscUtils.AddTag("<BasePath>", basePath);
            miscUtils.AddTag("<TargetPath>", targetPath);

            // Act
            string result = null;
            string message = null;
            ExtractException uex = null;
            try
            {
                result = miscUtils.ExpandTagsAndFunctions("$RelativePathParts(<BasePath>,<TargetPath>)", "", null);
            }
            catch (System.Runtime.InteropServices.COMException ex)
            {
                uex = ex.AsExtract("ELI53574");
                message = uex.Message;
            }

            // Assert
            Assert.Multiple(() =>
            {
                Assert.NotNull(uex);
                Assert.IsNull(result);
            });

            return message;
        }

        /// <summary>
        /// Happy cases for $RelativePathParts with specified path part numbers
        /// </summary>
        [TestCase(
            @"\\Server",
            @"\\Server\Workflow\Output\FileName.tif",
            "1,3",
            ExpectedResult = @"Workflow\FileName.tif",
            TestName = "RelativePathParts Good(Two specified parts)")]
        [TestCase(
            @"\\Server",
            @"\\Server\Workflow\Output\FileName.tif",
            "1,3,1,2",
            ExpectedResult = @"Workflow\FileName.tif\Workflow\Output",
            TestName = "RelativePathParts Good(Out of order and repeated numbers)")]
        [TestCase(
            "",
            @"\\Server\Workflow\Output\FileName.tif",
            "2,4",
            ExpectedResult = @"Workflow\FileName.tif",
            TestName = "RelativePathParts Good(Path parts with empty base)")]
        [TestCase(
            "",
            @"\\Server\Workflow\Output\Processed\Sub\FileName.tif",
            "6..1",
            ExpectedResult = @"FileName.tif\Sub\Processed\Output\Workflow\\\Server",
            TestName = "RelativePathParts Good(Reverse range)")]
        [TestCase(
            "",
            @"\\Server\Workflow\Output\Processed\Sub\FileName.tif",
            "<PartsVar>",
            ExpectedResult = @"\\Server\Workflow\Sub\FileName.tif",
            TestName = "RelativePathParts Good(Path parts specified as a tag)")]
        public static string SpecificParts(string basePath, string targetPath, string specifiedParts)
        {
            try
            {
                // Arrange
                MiscUtilsClass miscUtils = new();
                miscUtils.AddTag("<BasePath>", basePath);
                miscUtils.AddTag("<TargetPath>", targetPath);
                miscUtils.AddTag("<PartsVar>", "..2,-2.."); // Used to confirm that the code will expand a list even if it is supplied as one param

                // Act
                string result = miscUtils.ExpandTagsAndFunctions($"$RelativePathParts(<BasePath>,<TargetPath>,{specifiedParts})", "", null);

                // Assert
                return result;
            }
            catch (System.Runtime.InteropServices.COMException ex)
            {
                throw ex.AsExtract("ELI53585");
            }
        }

        /// <summary>
        /// Unhappy cases for $RelativePathParts with specified part numbers
        /// </summary>
        [TestCase(
            "",
            @"\\Server\Workflow\Output\FileName.tif",
            "2,Last",
            ExpectedResult = "Could not parse range!",
            TestName = "RelativePathParts Bad(Non-numeric path part)")]
        [TestCase(
            "",
            @"\\Server\Workflow\Output\FileName.tif",
            "2,0",
            ExpectedResult = "Specified start number is out of range!",
            TestName = "RelativePathParts Bad(Zero is a bad path part number)")]
        [TestCase(
            "",
            @"\\Server\Workflow\Output\FileName.tif",
            "1,5",
            ExpectedResult = "Specified start number is out of range!",
            TestName = "RelativePathParts Bad(Five is too high of a path part number)")]
        [TestCase(
            "",
            @"\\Server\Workflow\Output\FileName.tif",
            "4..-5",
            ExpectedResult = "Specified end number is out of range!",
            TestName = "RelativePathParts Bad(Negative five is too low of a path part number)")]
        [Category("Automated")]
        public static string InvalidSpecificParts(string basePath, string targetPath, string specifiedParts)
        {
            // Arrange
            MiscUtilsClass miscUtils = new();
            miscUtils.AddTag("<BasePath>", basePath);
            miscUtils.AddTag("<TargetPath>", targetPath);

            // Act
            string result = null;
            string message = null;
            ExtractException uex = null;
            try
            {
                result = miscUtils.ExpandTagsAndFunctions($"$RelativePathParts(<BasePath>,<TargetPath>,{specifiedParts})", "", null);
            }
            catch (System.Runtime.InteropServices.COMException ex)
            {
                uex = ex.AsExtract("ELI53841");
                message = uex.Message;
            }

            // Assert
            Assert.Multiple(() =>
            {
                Assert.NotNull(uex);
                Assert.IsNull(result);
            });

            return message;
        }
    }
}
