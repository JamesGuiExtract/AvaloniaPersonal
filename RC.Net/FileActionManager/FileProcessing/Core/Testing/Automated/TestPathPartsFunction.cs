using Extract.Testing.Utilities;
using NUnit.Framework;
using UCLID_COMUTILSLib;

namespace Extract.FileActionManager.FileProcessing.Test
{
    [TestFixture]
    [Category("TagExpansion")]
    public class TestPathPartsFunction
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
        /// Happy cases for $PathParts
        /// </summary>
        [TestCase(
            @"\\One\Two\Three\Four.pdf.tif",
            "2,4",
            ExpectedResult = @"Two\Four.pdf.tif",
            TestName = "PathParts Good(Two specified parts)")]
        [TestCase(
            @"\\One\Two\Three\Four.tif",
            "2,3,2,4",
            ExpectedResult = @"Two\Three\Two\Four.tif",
            TestName = "PathParts Good(Out of order and repeated numbers)")]
        [TestCase(
            @"\\One\Two\Three\Four\Five\Six\Seven.tif",
            "2..4,7",
            ExpectedResult = @"Two\Three\Four\Seven.tif",
            TestName = "PathParts Good(Part range)")]
        [TestCase(
            @"\\One\Two\Three\Four\Five\Six\Seven.tif",
            "-3",
            ExpectedResult = @"Five",
            TestName = "PathParts Good(Part number relative to end)")]
        [TestCase(
            @"\\One\Two\Three\Four\Five\Six\Seven.tif",
            "<PartsVar>",
            ExpectedResult = @"\\One\Two\Six\Seven.tif",
            TestName = "PathParts Good(multiple part numbers specified as a tag)")]
        public static string ValidCases(string path, string specifiedParts)
        {
            try
            {
                // Arrange
                MiscUtilsClass miscUtils = new();
                miscUtils.AddTag("<Path>", path);
                miscUtils.AddTag("<PartsVar>", "..2,-2.."); // Used to confirm that the code will expand a list even if it is supplied as one param

                // Act
                string result = miscUtils.ExpandTagsAndFunctions($"$PathParts(<Path>,{specifiedParts})", "", null);

                // Assert
                return result;
            }
            catch (System.Runtime.InteropServices.COMException ex)
            {
                throw ex.AsExtract("ELI53586");
            }
        }

        /// <summary>
        /// Unhappy cases for $PathParts
        /// </summary>
        [TestCase(
            @"\\One\Two\Three\Four.tif",
            "2,Last",
            ExpectedResult = "Could not parse range!",
            TestName = "PathParts Bad(Non-numeric path part)")]
        [TestCase(
            @"\\One\Two\Three\Four.tif",
            "..",
            ExpectedResult = "Could not parse range!",
            TestName = "PathParts Bad(Range without any numbers)")]
        [TestCase(
            @"\\One\Two\Three\Four.tif",
            "1...3",
            ExpectedResult = "Could not parse range!",
            TestName = "PathParts Bad(Full range with too many periods)")]
        [TestCase(
            @"\\One\Two\Three\Four.tif",
            "1...",
            ExpectedResult = "Could not parse range!",
            TestName = "PathParts Bad(Left range with too many periods)")]
        [TestCase(
            @"\\One\Two\Three\Four.tif",
            "...3",
            ExpectedResult = "Could not parse range!",
            TestName = "PathParts Bad(Right range with too many periods)")]
        [TestCase(
            @"\\One\Two\Three\Four.tif",
            "2,0",
            ExpectedResult = "Specified start number is out of range!",
            TestName = "PathParts Bad(Zero is a bad path part number)")]
        [TestCase(
            @"\\One\Two\Three\Four.tif",
            "0..3",
            ExpectedResult = "Specified start number is out of range!",
            TestName = "PathParts Bad(Zero is a bad start number)")]
        [TestCase(
            @"\\One\Two\Three\Four.tif",
            "2..0",
            ExpectedResult = "Specified end number is out of range!",
            TestName = "PathParts Bad(Zero is a bad end number)")]
        [TestCase(
            @"\\One\Two\Three\Four.tif",
            "1,5",
            ExpectedResult = "Specified start number is out of range!",
            TestName = "PathParts Bad(Five is too high of a path part number)")]
        [TestCase(
            @"\\One\Two\Three\Four.tif",
            "4..-5",
            ExpectedResult = "Specified end number is out of range!",
            TestName = "PathParts Bad(Negative five is too low of a path part number)")]
        [Category("Automated")]
        public static string InvalidSpecificParts(string path, string specifiedParts)
        {
            // Arrange
            MiscUtilsClass miscUtils = new();
            miscUtils.AddTag("<Path>", path);

            // Act
            string result = null;
            string message = null;
            ExtractException uex = null;
            try
            {
                result = miscUtils.ExpandTagsAndFunctions($"$PathParts(<Path>,{specifiedParts})", "", null);
            }
            catch (System.Runtime.InteropServices.COMException ex)
            {
                uex = ex.AsExtract("ELI53587");
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