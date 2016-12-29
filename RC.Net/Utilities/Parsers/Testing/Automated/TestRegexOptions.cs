using Extract.Testing.Utilities;
using NUnit.Framework;

namespace Extract.Utilities.Parsers.Test
{
    /// <summary>
    /// Class for testing the ConfigSettings class
    /// </summary>
    [TestFixture]
    [Category("Automatic")]
    public class TestRegexOptions
    {
        #region Constants

        /// <summary>
        /// The input string to be used for testing fuzzy expressions
        /// </summary>
        const string _INPUT = "b51:4:1DB:9EE1:5:27d60:f44:D4:cd:E:5:0A5:4a:D24:41Ad:";

        #endregion Constants

        #region Overhead Methods

        /// <summary>
        /// Setup method to initialize the testing environment.
        /// </summary>
        [TestFixtureSetUp]
        public static void Setup()
        {
            GeneralMethods.TestSetup();
        }

        #endregion Overhead Methods

        #region Tests

        [Test]
        public static void DefaultTimeout()
        {
            var regexParser = new DotNetRegexParser();
            regexParser.Pattern = @"^(([0-9a-fA-F]{1,4}:)*([0-9a-fA-F]{1,4}))*(::)$";

            Assert.DoesNotThrow(() => regexParser.Regex.Match(_INPUT));
        }

        [Test]
        public static void SpecifiedLongEnoughTimeout()
        {
            var regexParser = new DotNetRegexParser();
            regexParser.Pattern = @"(??timeout=00:00:20)^(([0-9a-fA-F]{1,4}:)*([0-9a-fA-F]{1,4}))*(::)$";

            Assert.DoesNotThrow(() => regexParser.Regex.Match(_INPUT));
        }

        [Test]
        public static void SpecifiedNotLongEnoughTimeout()
        {
            var regexParser = new DotNetRegexParser();
            regexParser.Pattern = @"(??timeout=00:00:2)^(([0-9a-fA-F]{1,4}:)*([0-9a-fA-F]{1,4}))*(::)$";

            Assert.Throws<System.Text.RegularExpressions.RegexMatchTimeoutException>
                (() => regexParser.Regex.Match(_INPUT));
        }

        [Test]
        public static void SpecifiedTimeoutMixedCase()
        {
            var regexParser = new DotNetRegexParser();
            regexParser.Pattern = @"(??tiMeoUt=00:00:20)^(?>([0-9a-fA-F]{1,4}:)*([0-9a-fA-F]{1,4}))*(::)$";

            Assert.DoesNotThrow(() => regexParser.Regex.Match(_INPUT));
        }

        [Test]
        public static void BadlyFormedOption()
        {
            var regexParser = new DotNetRegexParser();
            regexParser.Pattern = @"(??timeout=)^(([0-9a-fA-F]{1,4}:)*([0-9a-fA-F]{1,4}))*(::)$";

            Assert.Throws<ExtractException>(() => regexParser.Regex.Match(_INPUT));
        }

        [Test]
        public static void BadlyFormedOption2()
        {
            var regexParser = new DotNetRegexParser();
            regexParser.Pattern = @"(??timeout)^(([0-9a-fA-F]{1,4}:)*([0-9a-fA-F]{1,4}))*(::)$";

            Assert.Throws<ExtractException>(() => regexParser.Regex.Match(_INPUT));
        }

        [Test]
        public static void NonexistentOption()
        {
            var regexParser = new DotNetRegexParser();
            regexParser.Pattern = @"(??bingaliboop=123)^(([0-9a-fA-F]{1,4}:)*([0-9a-fA-F]{1,4}))*(::)$";

            Assert.Throws<ExtractException>(() => regexParser.Regex.Match(_INPUT));
        }

        #endregion Tests
    }
}
