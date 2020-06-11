using Extract.Testing.Utilities;
using NUnit.Framework;

namespace Extract.Utilities.Parsers.Test
{
    /// <summary>
    /// Class for testing the ConfigSettings class
    /// </summary>
    [TestFixture]
    [Category("FuzzyRegexes")]
    public class TestFuzzyRegexExpressions
    {
        #region Constants

        /// <summary>
        /// The input string to be used for testing fuzzy expressions
        /// </summary>
        const string _INPUT = "The quick  brown fox jumped over the lazy_dog.";

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
        public static void MethodFast()
        {
            var regexParser = new DotNetRegexParser();
            regexParser.Pattern = @"(?~<method=fast>brown)";

            Assert.That(regexParser.Regex.Match(_INPUT).Value == " brown");
        }

        [Test]
        public static void MethodBetterFit()
        {
            var regexParser = new DotNetRegexParser();
            regexParser.Pattern = @"(?~<method=better_fit>brown)";

            Assert.That(regexParser.Regex.Match(_INPUT).Value == "brown");
        }

        [Test]
        public static void MethodDefault()
        {
            var regexParser = new DotNetRegexParser();
            regexParser.Pattern = @"(?~<>brown)";

            Assert.That(regexParser.Regex.Match(_INPUT).Value == " brown");
        }

        [Test]
        public static void ErrorDiffPositive()
        {
            var regexParser = new DotNetRegexParser();
            regexParser.Pattern = @"(?~<error=2>ju__ed)";

            Assert.That(regexParser.Regex.IsMatch(_INPUT));
        }

        [Test]
        public static void ErrorDiffNegative()
        {
            var regexParser = new DotNetRegexParser();
            regexParser.Pattern = @"(?~<error=1>ju__ed)";

            Assert.That(!regexParser.Regex.IsMatch(_INPUT));
        }

        [Test]
        public static void ErrorMissingPositive()
        {
            var regexParser = new DotNetRegexParser();
            regexParser.Pattern = @"(?~<error=2>ju__mped)";

            Assert.That(regexParser.Regex.IsMatch(_INPUT));
        }

        [Test]
        public static void ErrorMissingDiffNegative()
        {
            var regexParser = new DotNetRegexParser();
            regexParser.Pattern = @"(?~<error=1>ju__mped)";

            Assert.That(!regexParser.Regex.IsMatch(_INPUT));
        }

        [Test]
        public static void ErrorAddedPositive()
        {
            var regexParser = new DotNetRegexParser();
            regexParser.Pattern = @"(?~<error=2>jued)";

            Assert.That(regexParser.Regex.IsMatch(_INPUT));
        }

        [Test]
        public static void ErrorAddedDiffNegative()
        {
            var regexParser = new DotNetRegexParser();
            regexParser.Pattern = @"(?~<error=1>jued)";

            Assert.That(!regexParser.Regex.IsMatch(_INPUT));
        }

        [Test]
        public static void ErrorDefault()
        {
            var regexParser = new DotNetRegexParser();
            regexParser.Pattern = @"(?~<>ju_ped)";

            Assert.That(regexParser.Regex.IsMatch(_INPUT));
        }

        [Test]
        public static void EscapeSpacesPositive()
        {
            var regexParser = new DotNetRegexParser();
            regexParser.Pattern = @"(?~<escape_space_chars=true>brown _ox)";

            Assert.That(regexParser.Regex.IsMatch(_INPUT));
        }

        [Test]
        public static void EscapeSpacesAltPositive()
        {
            var regexParser = new DotNetRegexParser();
            regexParser.Pattern = @"(?~<esc_s=true>brown _ox)";

            Assert.That(regexParser.Regex.IsMatch(_INPUT));
        }

        [Test]
        public static void EscapeSpacesNegative()
        {
            var regexParser = new DotNetRegexParser();
            regexParser.Pattern = @"(?~<escape_space_chars=false>brown _ox)";

            Assert.That(!regexParser.Regex.IsMatch(_INPUT));
        }

        [Test]
        public static void EscapeSpacesDefault()
        {
            var regexParser = new DotNetRegexParser();
            regexParser.Pattern = @"(?~<>brown _ox)";

            Assert.That(!regexParser.Regex.IsMatch(_INPUT));
        }

        [Test]
        public static void ExtraWhiteSpacePositive()
        {
            var regexParser = new DotNetRegexParser();
            regexParser.Pattern = @"(?~<extra_ws=1>quick\s_rown)";

            Assert.That(regexParser.Regex.IsMatch(_INPUT));
        }

        [Test]
        public static void ExtraWhiteSpaceAltPositive()
        {
            var regexParser = new DotNetRegexParser();
            regexParser.Pattern = @"(?~<xtra_ws=1>quick\s_rown)";

            Assert.That(regexParser.Regex.IsMatch(_INPUT));
        }

        [Test]
        public static void ExtraWhiteSpaceNegative()
        {
            var regexParser = new DotNetRegexParser();
            regexParser.Pattern = @"(?~<extra_ws=0>quick\s_rown)";

            Assert.That(!regexParser.Regex.IsMatch(_INPUT));
        }

        [Test]
        public static void ExtraWhiteSpaceDefault()
        {
            var regexParser = new DotNetRegexParser();
            regexParser.Pattern = @"(?~<>quick\s_rown)";

            Assert.That(!regexParser.Regex.IsMatch(_INPUT));
        }

        [Test]
        public static void Substitute1Positive()
        {
            var regexParser = new DotNetRegexParser();
            regexParser.Pattern = @"(?~<substitute_pattern=o>br_wn)";

            Assert.That(regexParser.Regex.IsMatch(_INPUT));
        }

        [Test]
        public static void Substitute1Negative()
        {
            var regexParser = new DotNetRegexParser();
            regexParser.Pattern = @"(?~<substitute_pattern=X>br_wn)";

            Assert.That(!regexParser.Regex.IsMatch(_INPUT));
        }

        [Test]
        public static void Substitute2Positive()
        {
            var regexParser = new DotNetRegexParser();
            regexParser.Pattern = @"(?~<sub=[a-z]>br_wn)";

            Assert.That(regexParser.Regex.IsMatch(_INPUT));
        }

        [Test]
        public static void Substitute2Negative()
        {
            var regexParser = new DotNetRegexParser();
            regexParser.Pattern = @"(?~<sub=[^a-z]>br_wn)";

            Assert.That(!regexParser.Regex.IsMatch(_INPUT));
        }

        [Test]
        public static void GlobalNamesPositive()
        {
            var regexParser = new DotNetRegexParser();
            regexParser.Pattern = @"(?~<use_global_names=true>quick)\s+\w+\s+(?~<use_global_names=true>f__)";

            Assert.That(regexParser.Regex.IsMatch(_INPUT));
        }

        [Test]
        public static void GlobalNamesAltPositive()
        {
            var regexParser = new DotNetRegexParser();
            regexParser.Pattern = @"(?~<global=true>quick)\s+\w+\s+(?~<global=true>f__)";

            Assert.That(regexParser.Regex.IsMatch(_INPUT));
        }

        [Test]
        public static void GlobalNamesAlt2Positive()
        {
            var regexParser = new DotNetRegexParser();
            regexParser.Pattern = @"(?~<g=true>quick)\s+\w+\s+(?~<g=true>f__)";

            Assert.That(regexParser.Regex.IsMatch(_INPUT));
        }

        [Test]
        public static void GlobalNamesNegative()
        {
            var regexParser = new DotNetRegexParser();
            regexParser.Pattern = @"(?~<g=true>qu_ck)\s+\w+\s+(?~<g=true>f__)";

            Assert.That(!regexParser.Regex.IsMatch(_INPUT));
        }

        [Test]
        public static void GlobalNamesNegative2()
        {
            var regexParser = new DotNetRegexParser();
            regexParser.Pattern = @"(?~<g=false>quick)\s+\w+\s+(?~<g=false>f__)";

            Assert.That(!regexParser.Regex.IsMatch(_INPUT));
        }

        [Test]
        public static void GlobalNamesNegative3()
        {
            var regexParser = new DotNetRegexParser();
            regexParser.Pattern = @"(?~<g=true>quick)\s+\w+\s+(?~<g=false>f__)";

            Assert.That(!regexParser.Regex.IsMatch(_INPUT));
        }

        [Test]
        public static void GlobalNamesNegative4()
        {
            var regexParser = new DotNetRegexParser();
            regexParser.Pattern = @"(?~<g=false>quick)\s+\w+\s+(?~<g=true>f__)";

            Assert.That(!regexParser.Regex.IsMatch(_INPUT));
        }

        [Test]
        public static void GlobalNamesDefault()
        {
            var regexParser = new DotNetRegexParser();
            regexParser.Pattern = @"(?~<>quick)\s+\w+\s+(?~<>f__)";

            Assert.That(!regexParser.Regex.IsMatch(_INPUT));
        }

        [Test]
        public static void WhiteSpacePatternPositive()
        {
            var regexParser = new DotNetRegexParser();
            regexParser.Pattern = @"(?~<error=0,xtra_ws=1,ws_pattern=_>lazydog)";

            Assert.That(regexParser.Regex.IsMatch(_INPUT));
        }

        [Test]
        public static void WhiteSpacePatternAltPositive()
        {
            var regexParser = new DotNetRegexParser();
            regexParser.Pattern = @"(?~<error=0,xtra_ws=1,ws=_>lazydog)";

            Assert.That(regexParser.Regex.IsMatch(_INPUT));
        }

        [Test]
        public static void WhiteSpacePatternNegative()
        {
            var regexParser = new DotNetRegexParser();
            regexParser.Pattern = @"(?~<error=0,xtra_ws=1,ws_pattern=\s>lazydog)";

            Assert.That(!regexParser.Regex.IsMatch(_INPUT));
        }

        [Test]
        public static void ReplacementsPositive()
        {
            var regexParser = new DotNetRegexParser();
            regexParser.Pattern = @"(?~<replacements=([^\sa-z]=>[a-z])>br_wn\sf_x)";

            Assert.That(regexParser.Regex.IsMatch(_INPUT));
        }

        [Test]
        public static void Replacements2Positive()
        {
            var regexParser = new DotNetRegexParser();
            regexParser.Pattern = @"(?~<replacements=([^\sa-z]=>[a-z])>brawn\sf_x)";

            Assert.That(regexParser.Regex.IsMatch(_INPUT));
        }

        [Test]
        public static void Replacements3Positive()
        {
            var regexParser = new DotNetRegexParser();
            regexParser.Pattern = @"(?~<replacements=([^\sa-z]=>[a-z])>br_wn\sfax)";

            Assert.That(regexParser.Regex.IsMatch(_INPUT));
        }

        [Test]
        public static void ReplacementsNegative()
        {
            var regexParser = new DotNetRegexParser();
            regexParser.Pattern = @"(?~<error=0,replacements=([^\sa-z]=>[a-z])>brawn\sfax)";

            Assert.That(!regexParser.Regex.IsMatch(_INPUT));
        }

        #endregion Tests
    }
}
