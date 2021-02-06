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
        [OneTimeSetUp]
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
            regexParser.Pattern = @"(??timeout=00:01:00)^(([0-9a-fA-F]{1,4}:)*([0-9a-fA-F]{1,4}))*(::)$";

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
        public static void SpecifiedNotLongEnoughTimeout_LeadingWhiteSpace()
        {
            var regexParser = new DotNetRegexParser();
            regexParser.Pattern = @"
                                    (??timeout=00:00:2)^(([0-9a-fA-F]{1,4}:)*([0-9a-fA-F]{1,4}))*(::)$";

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

        [Test]
        public static void RightToLeft()
        {
            var regexParser = new DotNetRegexParser();

            // First match is the leftmost occurrence of the pattern
            regexParser.Pattern = @"\d.*?E";
            Assert.AreEqual("51:4:1DB:9E", regexParser.Regex.Match(_INPUT).Value);

            // First match is the rightmost occurrence of the pattern
            regexParser.Pattern = @"(??RightToLeft=true)\d.*?E";
            Assert.AreEqual("4:cd:E", regexParser.Regex.Match(_INPUT).Value);

            // First match is the leftmost occurrence of the pattern
            regexParser.Pattern = @"(??RightToLeft=false)\d.*?E";
            Assert.AreEqual("51:4:1DB:9E", regexParser.Regex.Match(_INPUT).Value);
        }

        [Test]
        public static void OutputUnderscoreGroupsForDebugging_FindNamedGroups()
        {
            var regexParser = new DotNetRegexParser();

            regexParser.Pattern = @"(?'_test'.*)";
            Assert.AreEqual(0, regexParser.FindNamedGroups(_INPUT, true).Size());

            regexParser.Pattern = @"(??OutputUnderScoreGroups=true)(?'_test'.*)";
            Assert.AreEqual(1, regexParser.FindNamedGroups(_INPUT, true).Size());

            regexParser.Pattern = @"(??OutputUnderScoreGroups=false)(?'_test'.*)";
            Assert.AreEqual(0, regexParser.FindNamedGroups(_INPUT, true).Size());
        }

        [Test]
        public static void OutputUnderscoreGroupsForDebugging_Find()
        {
            var regexParser = new DotNetRegexParser();

            regexParser.Pattern = @"(?'_test'.*)";
            var result = regexParser.Find(_INPUT, true, true, false);
            IUnknownVector namedGroupData = (IUnknownVector)((ObjectPair)result.At(0)).Object2;
            Assert.AreEqual(0, namedGroupData.Size());

            regexParser.Pattern = @"(??OutputUnderScoreGroups=true)(?'_test'.*)";
            result = regexParser.Find(_INPUT, true, true, false);
            namedGroupData = (IUnknownVector)((ObjectPair)result.At(0)).Object2;
            Assert.AreEqual(1, namedGroupData.Size());

            regexParser.Pattern = @"(??OutputUnderScoreGroups=false)(?'_test'.*)";
            result = regexParser.Find(_INPUT, true, true, false);
            namedGroupData = (IUnknownVector)((ObjectPair)result.At(0)).Object2;
            Assert.AreEqual(0, namedGroupData.Size());
        }

        [Test]
        public static void FindIfXOf_Find()
        {
            var regexParser = new DotNetRegexParser();

            string input = "this cerlificate of live dirth";
            regexParser.Pattern = @"(??FindIfXOf=1)
                                    (
                                      ((?x)(?'certificate'
                                          certificate
                                        | (?'fuzzy'(?~<method=better_fit>certificate))
                                      ))
                                      ((?'birth'birth))
                                    )(\w+)";
            var result = regexParser.Find(input, bFindFirstMatchOnly: false, bReturnNamedMatches: true, bDoNotStopAtEmptyMatch: false);

            // Test succeeded and five words are found
            Assert.AreEqual(5, result.Size());

            // Each has result has the named group data for the matching test pattern
            foreach (var pair in result.ToIEnumerable<ObjectPair>())
            {
                IUnknownVector namedGroupData = (IUnknownVector)pair.Object2;
                Assert.AreEqual(2, namedGroupData.Size());
            }
        }

        [Test]
        public static void FindIfXOf_Find_MoreGroups()
        {
            var regexParser = new DotNetRegexParser();

            string input = "this cerlificate of live dirth";
            regexParser.Pattern = @"(??FindIfXOf=1)
                                    (
                                      ((?x)(?'certificate'
                                          certificate
                                        | (?'fuzzy'(?~<method=better_fit>certificate))
                                      ))
                                      ((?'birth'birth))
                                    )((?'sub'\w)\w+)";
            var result = regexParser.Find(input, bFindFirstMatchOnly: false, bReturnNamedMatches: true, bDoNotStopAtEmptyMatch: false);

            // Test succeeded and five words are found
            Assert.AreEqual(5, result.Size());

            // Each has result has the named group data for the matching test pattern first
            // and the named group from the main, finding pattern second
            foreach (var pair in result.ToIEnumerable<ObjectPair>())
            {
                IUnknownVector namedGroupData = (IUnknownVector)pair.Object2;
                Assert.AreEqual(3, namedGroupData.Size());
                Assert.AreEqual("certificate", ((Token)namedGroupData.At(0)).Name);
                Assert.AreEqual("fuzzy", ((Token)namedGroupData.At(1)).Name);
                Assert.AreEqual("sub", ((Token)namedGroupData.At(2)).Name);
            }
        }

        [Test]
        public static void FindIfXOf_FindNamedGroups()
        {
            var regexParser = new DotNetRegexParser();

            string input = "this cerlificate of live dirth";
            regexParser.Pattern = @"(??FindIfXOf=1)
                                    (
                                      ((?x)(?'certificate'
                                          certificate
                                        | (?'fuzzy'(?~<method=better_fit>certificate))
                                      ))
                                      ((?'birth'birth))
                                    )(\w+)";
            var result = regexParser.FindNamedGroups(input, bFindFirstMatchOnly: false);

            // Test succeeded, but no named groups exist in the main pattern
            Assert.AreEqual(0, result.Size());
        }

        [Test]
        public static void FindIfXOf_FindNamedGroups_MoreGroups()
        {
            var regexParser = new DotNetRegexParser();

            string input = "this cerlificate of live dirth";
            regexParser.Pattern = @"(??FindIfXOf=1)
                                    (
                                      ((?x)(?'certificate'
                                          certificate
                                        | (?'fuzzy'(?~<method=better_fit>certificate))
                                      ))
                                      ((?'birth'birth))
                                    )((?'sub'\w)\w+)";
            var result = regexParser.FindNamedGroups(input, bFindFirstMatchOnly: false);

            // Test succeeded and five matches are found
            Assert.AreEqual(5, result.Size());
        }

        [Test]
        public static void FindIfXOf_Recursive()
        {
            var regexParser = new DotNetRegexParser();

            string input = "this cerlificate of live dirth mother's place of residence";
            regexParser.Pattern = @"(??FindIfXOf=2, timeout=00:00:30)
                                    (
                                      ((?x)(?'testpattern1'
                                          (?~<method=better_fit>certificate)
                                      ))
                                      ((??FindIfXOf=2)
                                        (((?'nestedtestpattern_not_output'(?~<>birth)))(child)(live)(death)(vital))
                                        ((?'findpattern_of_testpattern_is_output'.))
                                      )
                                    )
                                    ((??FindIfXOf=1)
                                      (
                                        ((?x:(?'testpattern_of_findpattern_is_output'
                                            residence
                                        )))
                                      )
                                      ((?'findpattern_is_output'[\S\s]+))
                                    )";
            var result = regexParser.Find(input, bFindFirstMatchOnly: false, bReturnNamedMatches: true, bDoNotStopAtEmptyMatch: false);

            // Test succeeded, one match
            Assert.AreEqual(1, result.Size());

            // Result has the named group data for the matching test patterns with defined group names
            // and the finding pattern
            var pair = (ObjectPair)result.At(0);
            IUnknownVector namedGroupData = (IUnknownVector)pair.Object2;
            Assert.AreEqual(4, namedGroupData.Size());
            CollectionAssert.AreEqual(new[]
                { "testpattern1",
                  "findpattern_of_testpattern_is_output",
                  "testpattern_of_findpattern_is_output",
                  "findpattern_is_output" },
                namedGroupData.ToIEnumerable<Token>().Select(t => t.Name).ToArray());
        }

        [Test]
        public static void FindIfXOf_FindReplacements()
        {
            var regexParser = new DotNetRegexParser();

            string input = "this cerlificate of live dirth";
            regexParser.Pattern = @"(??FindIfXOf=1)
                                    (
                                      ((?x)(?'certificate'
                                          certificate
                                        | (?'fuzzy'(?~<method=better_fit>certificate))
                                      ))
                                      ((?'birth'birth))
                                    )((?~<method=better_fit>certificate))";
            var result = regexParser.FindReplacements(input, "certificate", true);

            // Test succeeded and one replacement found
            Assert.AreEqual(1, result.Size());
        }

        [Test]
        public static void FindIfXOf_ReplaceMatches()
        {
            var regexParser = new DotNetRegexParser();

            string input = "this cerlificate of live dirth";
            regexParser.Pattern = @"(??FindIfXOf=1)
                                    (
                                      ((?x)(?'certificate'
                                          certificate
                                        | (?'fuzzy'(?~<method=better_fit>certificate))
                                      ))
                                      ((?'birth'birth))
                                    )((?~<method=better_fit>certificate))";

            // This method doesn't support FindIfXOf patterns
            Assert.Throws<ExtractException>(() => regexParser.ReplaceMatches(input, "certificate", true));
        }

        [Test]
        public static void FindIfXOf_StringContainsPatterns()
        {
            var regexParser = new DotNetRegexParser();

            string input = "this cerlificate of live dirth";
            string pattern = @"(??FindIfXOf=1)
                               (
                                 ((?x)(?'certificate'
                                     certificate
                                   | (?'fuzzy'(?~<method=better_fit>certificate))
                                 ))
                                 ((?'birth'birth))
                               )((?~<method=better_fit>certificate))";

            var patterns = new VariantVector();
            patterns.PushBack(pattern);

            // This method doesn't support FindIfXOf patterns
            Assert.Throws<ExtractException>(() => regexParser.StringContainsPatterns(input, patterns, true));
        }

        [Test]
        public static void FindIfXOf_SyntaxErrorWrongNumberOfGroups()
        {
            var regexParser = new DotNetRegexParser();

            // Too few groups
            regexParser.Pattern = @"(??FindIfXOf=2)
                                    (
                                      (certificate)
                                      (birth)
                                    )";
            var exception = Assert.Throws<ExtractException>(() => regexParser.Find("", false, false, false));
            var inner = ExtractException.FromStringizedByteStream("", exception.Message).InnerException;

            Assert.AreEqual("Could not parse FindIfXOf pattern", inner.Message);
            Assert.That(inner.Data.Contains("Hint"));

            // Too many groups
            regexParser.Pattern = @"(??FindIfXOf=2)
                                    (?x)
                                    (
                                      (certificate)
                                      (birth)
                                    )
                                    (birth\scertificate)";
            exception = Assert.Throws<ExtractException>(() => regexParser.Find("", false, false, false));
            inner = ExtractException.FromStringizedByteStream("", exception.Message).InnerException;
            Assert.AreEqual("Could not parse FindIfXOf pattern", inner.Message);
            Assert.That(inner.Data.Contains("Hint"));
        }

        [Test]
        public static void FindIfXOf_SyntaxErrorExtraStuff()
        {
            var regexParser = new DotNetRegexParser();

            // Extra stuff
            regexParser.Pattern = @"(??FindIfXOf=2)
                                    (
                                      (certificate)
                                      (birth)
                                    )
                                    (birth\scertificate)
                                    extra stuff";
            var exception = Assert.Throws<ExtractException>(() => regexParser.Find("", false, false, false));
            var inner = ExtractException.FromStringizedByteStream("", exception.Message).InnerException;
            Assert.AreEqual("Could not parse FindIfXOf pattern", inner.Message);
            Assert.That(inner.Data.Contains("Hint"));
        }

        [Test]
        public static void FindIfXOf_SyntaxErrorTestPatterns()
        {
            var regexParser = new DotNetRegexParser();

            // No test patterns
            regexParser.Pattern = @"(??FindIfXOf=2)
                                    ()
                                    (birth\scertificate)";
            var exception = Assert.Throws<ExtractException>(() => regexParser.Find("", false, false, false));
            var inner = ExtractException.FromStringizedByteStream("", exception.Message).InnerException;
            Assert.AreEqual("Could not parse FindIfXOf pattern", inner.Message);
            Assert.That(inner.Data.Contains("Hint"));

            // No test patterns 2
            regexParser.Pattern = @"(??FindIfXOf=2)
                                    (
                                      no parens
                                    )
                                    (birth\scertificate)";
            exception = Assert.Throws<ExtractException>(() => regexParser.Find("", false, false, false));
            inner = ExtractException.FromStringizedByteStream("", exception.Message).InnerException;
            Assert.AreEqual("Could not parse FindIfXOf pattern", inner.Message);
            Assert.That(inner.Data.Contains("Hint"));

            // Missing closing paren
            regexParser.Pattern = @"(??FindIfXOf=2)
                                    (
                                      (missing paren
                                    )
                                    (birth\scertificate)";
            exception = Assert.Throws<ExtractException>(() => regexParser.Find("", false, false, false));
            inner = ExtractException.FromStringizedByteStream("", exception.Message).InnerException;
            Assert.AreEqual("Could not parse FindIfXOf pattern", inner.Message);
            Assert.That(inner.Data.Contains("Hint"));

            // Too many closing paren
            regexParser.Pattern = @"(??FindIfXOf=2)
                                    (
                                      (testpattern))
                                    )
                                    (birth\scertificate)";
            exception = Assert.Throws<ExtractException>(() => regexParser.Find("", false, false, false));
            inner = ExtractException.FromStringizedByteStream("", exception.Message).InnerException;
            Assert.AreEqual("Could not parse FindIfXOf pattern", inner.Message);
            Assert.That(inner.Data.Contains("Hint"));

            // Extra stuff
            regexParser.Pattern = @"(??FindIfXOf=2)
                                    (
                                      (certificate)
                                      extra stuff
                                      (birth)
                                    )
                                    (birth\scertificate)";
            exception = Assert.Throws<ExtractException>(() => regexParser.Find("", false, false, false));
            inner = ExtractException.FromStringizedByteStream("", exception.Message).InnerException;
            Assert.AreEqual("Could not parse FindIfXOf pattern", inner.Message);
            Assert.That(inner.Data.Contains("Hint"));
        }

        #endregion Tests
    }
}
