using Extract.Testing.Utilities;
using NUnit.Framework;
using UCLID_COMUTILSLib;

namespace Extract.FileActionManager.FileProcessing.Test
{
    [TestFixture]
    [Category("TagExpansion")]
    public class TestNoExtOfFunction
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
        /// Happy cases for $NoExtOf
        /// </summary>
        [TestCase(
            @"C:\One\Two\Three\Four.pdf",
            ExpectedResult = @"C:\One\Two\Three\Four",
            TestName = "NoExtOf Good(Remove single extension)")]
        [TestCase(
            @"\\One\Two\Three\Four.docx",
            ExpectedResult = @"\\One\Two\Three\Four",
            TestName = "NoExtOf Good(Remove single extension from UNC path)")]
        [TestCase(
            @"\\One\Two\Three\Four.pdf.tif",
            ExpectedResult = @"\\One\Two\Three\Four.pdf",
            TestName = "NoExtOf Good(Remove one of two extensions)")]
        [TestCase(
            @"\\One\Two.Three\Four",
            ExpectedResult = @"\\One\Two.Three\Four",
            TestName = "NoExtOf Good(No extensions in file part to remove)")]
        public static string ValidCases(string path)
        {
            try
            {
                // Arrange
                MiscUtilsClass miscUtils = new();
                miscUtils.AddTag("<Path>", path);

                // Act
                string result = miscUtils.ExpandTagsAndFunctions($"$NoExtOf(<Path>)", "", null);

                // Assert
                return result;
            }
            catch (System.Runtime.InteropServices.COMException ex)
            {
                throw ex.AsExtract("ELI53596");
            }
        }
    }
}
