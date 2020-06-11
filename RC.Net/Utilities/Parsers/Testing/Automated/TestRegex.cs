using Extract.Testing.Utilities;
using NUnit.Framework;
using System.Linq;
using UCLID_COMUTILSLib;

namespace Extract.Utilities.Parsers.Test
{
    /// <summary>
    /// Class for testing the ConfigSettings class
    /// </summary>
    [TestFixture]
    [Category("Automatic")]
    public class TestRegex
    {
        #region Constants

        /// <summary>
        /// The input string to be used for testing fuzzy expressions
        /// </summary>
        const string _INPUT = "the quick brown fox jumped over the lazy dogs";

        #endregion Constants

        #region Overhead Methods

        /// <summary>
        /// Setup method to initialize the testing environment.
        /// </summary>
        [OneTimeSetUp]
        public static void Setup()
        {
            GeneralMethods.TestSetup();
        }

        #endregion Overhead Methods

        #region Tests

        [Test]
        // Regex parser index out of bounds error with groups named after large numbers
        // https://extract.atlassian.net/browse/ISSUE-13902
        public static void Issue_13902_HighNumberedGroup()
        {
            var regexParser = new DotNetRegexParser();
            regexParser.Pattern = @"(.)(?'20'.)";

            Assert.DoesNotThrow(() => regexParser.Regex.Match(_INPUT));
        }

        #endregion Tests
    }
}
