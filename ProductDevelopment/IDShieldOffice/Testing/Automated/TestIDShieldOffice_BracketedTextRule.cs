using Extract.Rules;
using NUnit.Framework;

namespace IDShieldOffice.Test
{
    public partial class TestIDShieldOffice
    {
        #region Square Brackets

        /// <summary>
        /// Test the ability to find pairs of matching square brackets.
        /// </summary>
        [Test, Category("Automated"), Category("Bracketed Text")]
        public static void Automated_FindSquareBracketsAllMatched()
        {
            // Set the test string and expected results
            string testString = "[There should be ] [three matches] for [this string]";
            string[] expectedResults = new string[] {
                "[There should be ]",
                "[three matches]",
                "[this string]" };

            BracketedTextRule bracketedTextRule = new BracketedTextRule(true, false, false);

            Assert.That(TestFindingRuleOnText(bracketedTextRule, testString, expectedResults));
        }

        /// <summary>
        /// Test the ability to find pairs of matching square brackets.
        /// </summary>
        [Test, Category("Automated"), Category("Bracketed Text")]
        public static void Automated_FindSquareBracketsUnmatchedOpening()
        {
            // Set the test string and expected results
            string testString = "[There should be ] [two matches for [this string]";
            string[] expectedResults = new string[] {
                "[There should be ]",
                "[this string]" };

            BracketedTextRule bracketedTextRule = new BracketedTextRule(true, false, false);

            Assert.That(TestFindingRuleOnText(bracketedTextRule, testString, expectedResults));
        }

        /// <summary>
        /// Test the ability to find pairs of matching square brackets.
        /// </summary>
        [Test, Category("Automated"), Category("Bracketed Text")]
        public static void Automated_FindSquareBracketsUnmatchedClosing()
        {
            // Set the test string and expected results
            string testString = "[There should be ] two matches] for [this string]";
            string[] expectedResults = new string[] {
                "[There should be ]",
                "[this string]" };

            BracketedTextRule bracketedTextRule = new BracketedTextRule(true, false, false);

            Assert.That(TestFindingRuleOnText(bracketedTextRule, testString, expectedResults));
        }

        /// <summary>
        /// Test the ability to find pairs of matching square brackets.
        /// </summary>
        [Test, Category("Automated"), Category("Bracketed Text")]
        public static void Automated_FindSquareBracketsMultilineAllMatched()
        {
            // Set the test string and expected results
            string testString = "[There should " + _LINE_BREAK + "be ] [three " + _LINE_BREAK
                + "matches] for [this string]";
            string[] expectedResults = new string[] {
                "[There should be ]",
                "[three matches]",
                "[this string]" };

            BracketedTextRule bracketedTextRule = new BracketedTextRule(true, false, false);

            Assert.That(TestFindingRuleOnText(bracketedTextRule, testString, expectedResults));
        }

        /// <summary>
        /// Test the ability to find pairs of matching square brackets.
        /// </summary>
        [Test, Category("Automated"), Category("Bracketed Text")]
        public static void Automated_FindSquareBracketsMultilineUnmatchedOpening()
        {
            // Set the test string and expected results
            string testString = "[There should " + _LINE_BREAK + "be ] [two matches "
                + _LINE_BREAK + "for [this string]";
            string[] expectedResults = new string[] {
                "[There should be ]",
                "[this string]" };

            BracketedTextRule bracketedTextRule = new BracketedTextRule(true, false, false);

            Assert.That(TestFindingRuleOnText(bracketedTextRule, testString, expectedResults));
        }

        /// <summary>
        /// Test the ability to find pairs of matching square brackets.
        /// </summary>
        [Test, Category("Automated"), Category("Bracketed Text")]
        public static void Automated_FindSquareBracketsMultilineUnmatchedClosing()
        {
            // Set the test string and expected results
            string testString = "[There should " + _LINE_BREAK + "be ] two matches"
                + _LINE_BREAK + "] for [this string]";
            string[] expectedResults = new string[] {
                "[There should be ]",
                "[this string]" };

            BracketedTextRule bracketedTextRule = new BracketedTextRule(true, false, false);

            Assert.That(TestFindingRuleOnText(bracketedTextRule, testString, expectedResults));
        }

        /// <summary>
        /// Test the ability to find pairs of matching square brackets.
        /// </summary>
        [Test, Category("Automated"), Category("Bracketed Text")]
        public static void Automated_FindSquareBracketsMultiplePagesAllMatched()
        {
            // Set the test string and expected results
            string testString = "[There should " + _LINE_BREAK + "be ] [three " + _PAGE_BREAK
                + "matches] for [this " + _PAGE_BREAK + "string]";
            string[] expectedResults = new string[] {
                "[There should be ]",
                "[three matches]",
                "[this string]" };

            BracketedTextRule bracketedTextRule = new BracketedTextRule(true, false, false);

            Assert.That(TestFindingRuleOnText(bracketedTextRule, testString, expectedResults));
        }

        /// <summary>
        /// Test the ability to find pairs of matching square brackets.
        /// </summary>
        [Test, Category("Automated"), Category("Bracketed Text")]
        public static void Automated_FindSquareBracketsMultiplePagesUnmatchedOpening()
        {
            // Set the test string and expected results
            string testString = "[There should " + _PAGE_BREAK + "be ] [two matches "
                + _PAGE_BREAK + "for [this " + _LINE_BREAK + "string]";
            string[] expectedResults = new string[] {
                "[There should be ]",
                "[this string]" };

            BracketedTextRule bracketedTextRule = new BracketedTextRule(true, false, false);

            Assert.That(TestFindingRuleOnText(bracketedTextRule, testString, expectedResults));
        }

        /// <summary>
        /// Test the ability to find pairs of matching square brackets.
        /// </summary>
        [Test, Category("Automated"), Category("Bracketed Text")]
        public static void Automated_FindSquareBracketsMultiplePagesUnmatchedClosing()
        {
            // Set the test string and expected results
            string testString = "[There should " + _PAGE_BREAK + "be ] two matches"
                + _PAGE_BREAK + "] for [this" + _LINE_BREAK + " string]";
            string[] expectedResults = new string[] {
                "[There should be ]",
                "[this string]" };

            BracketedTextRule bracketedTextRule = new BracketedTextRule(true, false, false);

            Assert.That(TestFindingRuleOnText(bracketedTextRule, testString, expectedResults));
        }

        /// <summary>
        /// Test the ability to find nested pairs of matching square brackets.
        /// </summary>
        [Test, Category("Automated"), Category("Bracketed Text")]
        public static void Automated_FindNestedSquareBracketsAllMatched()
        {
            // Set the test string and expected results
            string testString = "[There should [be [three matches] for this] string]";
            string[] expectedResults = new string[] {
                "[There should [be [three matches] for this] string]",
                "[be [three matches] for this]",
                "[three matches]" };

            BracketedTextRule bracketedTextRule = new BracketedTextRule(true, false, false);

            Assert.That(TestFindingRuleOnText(bracketedTextRule, testString, expectedResults));
        }

        /// <summary>
        /// Test the ability to find nested pairs of matching square brackets.
        /// </summary>
        [Test, Category("Automated"), Category("Bracketed Text")]
        public static void Automated_FindNestedSquareBracketsUnmatchedOpening()
        {
            // Set the test string and expected results
            string testString = "[There should [be [two matches for this] string]";
            string[] expectedResults = new string[] {
                "[be [two matches for this] string]",
                "[two matches for this]" };

            BracketedTextRule bracketedTextRule = new BracketedTextRule(true, false, false);

            Assert.That(TestFindingRuleOnText(bracketedTextRule, testString, expectedResults));
        }

        /// <summary>
        /// Test the ability to find nested pairs of matching square brackets.
        /// </summary>
        [Test, Category("Automated"), Category("Bracketed Text")]
        public static void Automated_FindNestedSquareBracketsUnmatchedClosing()
        {
            // Set the test string and expected results
            string testString = "[There should be [two matches for] this] string]";
            string[] expectedResults = new string[] {
                "[There should be [two matches for] this]",
                "[two matches for]" };

            BracketedTextRule bracketedTextRule = new BracketedTextRule(true, false, false);

            Assert.That(TestFindingRuleOnText(bracketedTextRule, testString, expectedResults));
        }

        /// <summary>
        /// Test the ability to find nested pairs of matching square brackets across multiple lines.
        /// </summary>
        [Test, Category("Automated"), Category("Bracketed Text")]
        public static void Automated_FindNestedSquareBracketsMultilineAllMatched()
        {
            // Set the test string and expected results
            string testString = "[There " + _LINE_BREAK + "should [be " + _LINE_BREAK + "[three "
                + _LINE_BREAK + "matches] for " + _LINE_BREAK + "this] string]";
            string[] expectedResults = new string[] {
                "[There should [be [three matches] for this] string]",
                "[be [three matches] for this]",
                "[three matches]" };

            BracketedTextRule bracketedTextRule = new BracketedTextRule(true, false, false);

            Assert.That(TestFindingRuleOnText(bracketedTextRule, testString, expectedResults));
        }

        /// <summary>
        /// Test the ability to find nested pairs of matching square brackets across multiple lines.
        /// </summary>
        [Test, Category("Automated"), Category("Bracketed Text")]
        public static void Automated_FindNestedSquareBracketsMultilineUnmatchedOpening()
        {
            // Set the test string and expected results
            string testString = "[There " + _LINE_BREAK + "should [be " + _LINE_BREAK 
                + "[two matches " + _LINE_BREAK + "for this] " + _LINE_BREAK + "string]";
            string[] expectedResults = new string[] {
                "[be [two matches for this] string]",
                "[two matches for this]" };

            BracketedTextRule bracketedTextRule = new BracketedTextRule(true, false, false);

            Assert.That(TestFindingRuleOnText(bracketedTextRule,
                testString, expectedResults));
        }

        /// <summary>
        /// Test the ability to find nested pairs of matching square brackets across multiple lines.
        /// </summary>
        [Test, Category("Automated"), Category("Bracketed Text")]
        public static void Automated_FindNestedSquareBracketsMultilineUnmatchedClosing()
        {
            // Set the test string and expected results
            string testString = "[There " + _LINE_BREAK + "should be " + _LINE_BREAK 
                + "[two " + _LINE_BREAK + "matches for] " + _LINE_BREAK 
                + "this] " + _LINE_BREAK + "string]";
            string[] expectedResults = new string[] {
                "[There should be [two matches for] this]",
                "[two matches for]" };

            BracketedTextRule bracketedTextRule = new BracketedTextRule(true, false, false);

            Assert.That(TestFindingRuleOnText(bracketedTextRule,
                testString, expectedResults));
        }

        /// <summary>
        /// Test the ability to find nested pairs of matching square brackets across multiple lines.
        /// </summary>
        [Test, Category("Automated"), Category("Bracketed Text")]
        public static void Automated_FindNestedSquareBracketsMultiplePagesAllMatched()
        {
            // Set the test string and expected results
            string testString = "[There should [be " + _LINE_BREAK + "[three " + _PAGE_BREAK
                + "matches] for " + _LINE_BREAK + "this] string]";
            string[] expectedResults = new string[] {
                "[There should [be [three matches] for this] string]",
                "[be [three matches] for this]",
                "[three matches]" };

            BracketedTextRule bracketedTextRule = new BracketedTextRule(true, false, false);

            Assert.That(TestFindingRuleOnText(bracketedTextRule, testString, expectedResults));
        }

        /// <summary>
        /// Test the ability to find nested pairs of matching square brackets across multiple lines.
        /// </summary>
        [Test, Category("Automated"), Category("Bracketed Text")]
        public static void Automated_FindNestedSquareBracketsMultiplePagesUnmatchedOpening()
        {
            // Set the test string and expected results
            string testString = "[There " + _LINE_BREAK + "should [be " + _LINE_BREAK 
                + "[two " + _PAGE_BREAK + "matches " + _LINE_BREAK + "for this] " 
                + _LINE_BREAK + "string]";
            string[] expectedResults = new string[] {
                "[be [two matches for this] string]",
                "[two matches for this]" };

            BracketedTextRule bracketedTextRule = new BracketedTextRule(true, false, false);

            Assert.That(TestFindingRuleOnText(bracketedTextRule,
                testString, expectedResults));
        }

        /// <summary>
        /// Test the ability to find nested pairs of matching square brackets across multiple lines.
        /// </summary>
        [Test, Category("Automated"), Category("Bracketed Text")]
        public static void Automated_FindNestedSquareBracketsMultiplePagesUnmatchedClosing()
        {
            // Set the test string and expected results
            string testString = "[There " + _LINE_BREAK + "should be " + _LINE_BREAK 
                + "[two " + _PAGE_BREAK + "matches for] " + _LINE_BREAK
                + "this] " + _LINE_BREAK + "string]";
            string[] expectedResults = new string[] {
                "[There should be [two matches for] this]",
                "[two matches for]" };

            BracketedTextRule bracketedTextRule = new BracketedTextRule(true, false, false);

            Assert.That(TestFindingRuleOnText(bracketedTextRule,
                testString, expectedResults));
        }

        #endregion Square Brackets

        #region Curved Brackets

        /// <summary>
        /// Test the ability to find pairs of matching curved brackets.
        /// </summary>
        [Test, Category("Automated"), Category("Bracketed Text")]
        public static void Automated_FindCurvedBracketsAllMatched()
        {
            // Set the test string and expected results
            string testString = "(There should be ) (three matches) for (this string)";
            string[] expectedResults = new string[] {
                "(There should be )",
                "(three matches)",
                "(this string)" };

            BracketedTextRule bracketedTextRule = new BracketedTextRule(false, true, false);

            Assert.That(TestFindingRuleOnText(bracketedTextRule, testString, expectedResults));
        }

        /// <summary>
        /// Test the ability to find pairs of matching curved brackets.
        /// </summary>
        [Test, Category("Automated"), Category("Bracketed Text")]
        public static void Automated_FindCurvedBracketsUnmatchedOpening()
        {
            // Set the test string and expected results
            string testString = "(There should be ) (two matches for (this string)";
            string[] expectedResults = new string[] {
                "(There should be )",
                "(this string)" };

            BracketedTextRule bracketedTextRule = new BracketedTextRule(false, true, false);

            Assert.That(TestFindingRuleOnText(bracketedTextRule, testString, expectedResults));
        }

        /// <summary>
        /// Test the ability to find pairs of matching curved brackets.
        /// </summary>
        [Test, Category("Automated"), Category("Bracketed Text")]
        public static void Automated_FindCurvedBracketsUnmatchedClosing()
        {
            // Set the test string and expected results
            string testString = "(There should be ) two matches) for (this string)";
            string[] expectedResults = new string[] {
                "(There should be )",
                "(this string)" };

            BracketedTextRule bracketedTextRule = new BracketedTextRule(false, true, false);

            Assert.That(TestFindingRuleOnText(bracketedTextRule, testString, expectedResults));
        }

        /// <summary>
        /// Test the ability to find pairs of matching curved brackets.
        /// </summary>
        [Test, Category("Automated"), Category("Bracketed Text")]
        public static void Automated_FindCurvedBracketsMultilineAllMatched()
        {
            // Set the test string and expected results
            string testString = "(There should " + _LINE_BREAK + "be ) (three " + _LINE_BREAK
                + "matches) for (this string)";
            string[] expectedResults = new string[] {
                "(There should be )",
                "(three matches)",
                "(this string)" };

            BracketedTextRule bracketedTextRule = new BracketedTextRule(false, true, false);

            Assert.That(TestFindingRuleOnText(bracketedTextRule, testString, expectedResults));
        }

        /// <summary>
        /// Test the ability to find pairs of matching curved brackets.
        /// </summary>
        [Test, Category("Automated"), Category("Bracketed Text")]
        public static void Automated_FindCurvedBracketsMultilineUnmatchedOpening()
        {
            // Set the test string and expected results
            string testString = "(There should " + _LINE_BREAK + "be ) (two matches "
                + _LINE_BREAK + "for (this string)";
            string[] expectedResults = new string[] {
                "(There should be )",
                "(this string)" };

            BracketedTextRule bracketedTextRule = new BracketedTextRule(false, true, false);

            Assert.That(TestFindingRuleOnText(bracketedTextRule, testString, expectedResults));
        }

        /// <summary>
        /// Test the ability to find pairs of matching curved brackets.
        /// </summary>
        [Test, Category("Automated"), Category("Bracketed Text")]
        public static void Automated_FindCurvedBracketsMultilineUnmatchedClosing()
        {
            // Set the test string and expected results
            string testString = "(There should " + _LINE_BREAK + "be ) two matches"
                + _LINE_BREAK + ") for (this string)";
            string[] expectedResults = new string[] {
                "(There should be )",
                "(this string)" };

            BracketedTextRule bracketedTextRule = new BracketedTextRule(false, true, false);

            Assert.That(TestFindingRuleOnText(bracketedTextRule, testString, expectedResults));
        }

        /// <summary>
        /// Test the ability to find pairs of matching curved brackets.
        /// </summary>
        [Test, Category("Automated"), Category("Bracketed Text")]
        public static void Automated_FindCurvedBracketsMultiplePagesAllMatched()
        {
            // Set the test string and expected results
            string testString = "(There should " + _LINE_BREAK + "be ) (three " + _PAGE_BREAK
                + "matches) for (this " + _PAGE_BREAK + "string)";
            string[] expectedResults = new string[] {
                "(There should be )",
                "(three matches)",
                "(this string)" };

            BracketedTextRule bracketedTextRule = new BracketedTextRule(false, true, false);

            Assert.That(TestFindingRuleOnText(bracketedTextRule, testString, expectedResults));
        }

        /// <summary>
        /// Test the ability to find pairs of matching curved brackets.
        /// </summary>
        [Test, Category("Automated"), Category("Bracketed Text")]
        public static void Automated_FindCurvedBracketsMultiplePagesUnmatchedOpening()
        {
            // Set the test string and expected results
            string testString = "(There should " + _PAGE_BREAK + "be ) (two matches "
                + _PAGE_BREAK + "for (this " + _LINE_BREAK + "string)";
            string[] expectedResults = new string[] {
                "(There should be )",
                "(this string)" };

            BracketedTextRule bracketedTextRule = new BracketedTextRule(false, true, false);

            Assert.That(TestFindingRuleOnText(bracketedTextRule, testString, expectedResults));
        }

        /// <summary>
        /// Test the ability to find pairs of matching curved brackets.
        /// </summary>
        [Test, Category("Automated"), Category("Bracketed Text")]
        public static void Automated_FindCurvedBracketsMultiplePagesUnmatchedClosing()
        {
            // Set the test string and expected results
            string testString = "(There should " + _PAGE_BREAK + "be ) two matches"
                + _PAGE_BREAK + ") for (this" + _LINE_BREAK + " string)";
            string[] expectedResults = new string[] {
                "(There should be )",
                "(this string)" };

            BracketedTextRule bracketedTextRule = new BracketedTextRule(false, true, false);

            Assert.That(TestFindingRuleOnText(bracketedTextRule, testString, expectedResults));
        }

        /// <summary>
        /// Test the ability to find nested pairs of matching curved brackets.
        /// </summary>
        [Test, Category("Automated"), Category("Bracketed Text")]
        public static void Automated_FindNestedCurvedBracketsAllMatched()
        {
            // Set the test string and expected results
            string testString = "(There should (be (three matches) for this) string)";
            string[] expectedResults = new string[] {
                "(There should (be (three matches) for this) string)",
                "(be (three matches) for this)",
                "(three matches)" };

            BracketedTextRule bracketedTextRule = new BracketedTextRule(false, true, false);

            Assert.That(TestFindingRuleOnText(bracketedTextRule, testString, expectedResults));
        }

        /// <summary>
        /// Test the ability to find nested pairs of matching curved brackets.
        /// </summary>
        [Test, Category("Automated"), Category("Bracketed Text")]
        public static void Automated_FindNestedCurvedBracketsUnmatchedOpening()
        {
            // Set the test string and expected results
            string testString = "(There should (be (two matches for this) string)";
            string[] expectedResults = new string[] {
                "(be (two matches for this) string)",
                "(two matches for this)" };

            BracketedTextRule bracketedTextRule = new BracketedTextRule(false, true, false);

            Assert.That(TestFindingRuleOnText(bracketedTextRule, testString, expectedResults));
        }

        /// <summary>
        /// Test the ability to find nested pairs of matching curved brackets.
        /// </summary>
        [Test, Category("Automated"), Category("Bracketed Text")]
        public static void Automated_FindNestedCurvedBracketsUnmatchedClosing()
        {
            // Set the test string and expected results
            string testString = "(There should be (two matches for) this) string)";
            string[] expectedResults = new string[] {
                "(There should be (two matches for) this)",
                "(two matches for)" };

            BracketedTextRule bracketedTextRule = new BracketedTextRule(false, true, false);

            Assert.That(TestFindingRuleOnText(bracketedTextRule, testString, expectedResults));
        }

        /// <summary>
        /// Test the ability to find nested pairs of matching curved brackets across multiple lines.
        /// </summary>
        [Test, Category("Automated"), Category("Bracketed Text")]
        public static void Automated_FindNestedCurvedBracketsMultilineAllMatched()
        {
            // Set the test string and expected results
            string testString = "(There " + _LINE_BREAK + "should (be " + _LINE_BREAK 
                + "(three " + _LINE_BREAK + "matches) for " + _LINE_BREAK + "this) string)";
            string[] expectedResults = new string[] {
                "(There should (be (three matches) for this) string)",
                "(be (three matches) for this)",
                "(three matches)" };

            BracketedTextRule bracketedTextRule = new BracketedTextRule(false, true, false);

            Assert.That(TestFindingRuleOnText(bracketedTextRule, testString, expectedResults));
        }

        /// <summary>
        /// Test the ability to find nested pairs of matching curved brackets across multiple lines.
        /// </summary>
        [Test, Category("Automated"), Category("Bracketed Text")]
        public static void Automated_FindNestedCurvedBracketsMultilineUnmatchedOpening()
        {
            // Set the test string and expected results
            string testString = "(There " + _LINE_BREAK + "should (be " + _LINE_BREAK 
                + "(two matches " + _LINE_BREAK + "for this) " + _LINE_BREAK + "string)";
            string[] expectedResults = new string[] {
                "(be (two matches for this) string)",
                "(two matches for this)" };

            BracketedTextRule bracketedTextRule = new BracketedTextRule(false, true, false);

            Assert.That(TestFindingRuleOnText(bracketedTextRule,
                testString, expectedResults));
        }

        /// <summary>
        /// Test the ability to find nested pairs of matching curved brackets across multiple lines.
        /// </summary>
        [Test, Category("Automated"), Category("Bracketed Text")]
        public static void Automated_FindNestedCurvedBracketsMultilineUnmatchedClosing()
        {
            // Set the test string and expected results
            string testString = "(There " + _LINE_BREAK + "should be " + _LINE_BREAK + "(two " 
                + _LINE_BREAK + "matches for) " + _LINE_BREAK + "this) " + _LINE_BREAK + "string)";
            string[] expectedResults = new string[] {
                "(There should be (two matches for) this)",
                "(two matches for)" };

            BracketedTextRule bracketedTextRule = new BracketedTextRule(false, true, false);

            Assert.That(TestFindingRuleOnText(bracketedTextRule,
                testString, expectedResults));
        }

        #endregion Curved Brackets

        #region Curly Brackets

        /// <summary>
        /// Test the ability to find pairs of matching curly brackets.
        /// </summary>
        [Test, Category("Automated"), Category("Bracketed Text")]
        public static void Automated_FindCurlyBracketsAllMatched()
        {
            // Set the test string and expected results
            string testString = "{There should be } {three matches} for {this string}";
            string[] expectedResults = new string[] {
                "{There should be }",
                "{three matches}",
                "{this string}" };

            BracketedTextRule bracketedTextRule = new BracketedTextRule(false, false, true);

            Assert.That(TestFindingRuleOnText(bracketedTextRule, testString, expectedResults));
        }

        /// <summary>
        /// Test the ability to find pairs of matching curly brackets.
        /// </summary>
        [Test, Category("Automated"), Category("Bracketed Text")]
        public static void Automated_FindCurlyBracketsUnmatchedOpening()
        {
            // Set the test string and expected results
            string testString = "{There should be } {two matches for {this string}";
            string[] expectedResults = new string[] {
                "{There should be }",
                "{this string}" };

            BracketedTextRule bracketedTextRule = new BracketedTextRule(false, false, true);

            Assert.That(TestFindingRuleOnText(bracketedTextRule, testString, expectedResults));
        }

        /// <summary>
        /// Test the ability to find pairs of matching curly brackets.
        /// </summary>
        [Test, Category("Automated"), Category("Bracketed Text")]
        public static void Automated_FindCurlyBracketsUnmatchedClosing()
        {
            // Set the test string and expected results
            string testString = "{There should be } two matches} for {this string}";
            string[] expectedResults = new string[] {
                "{There should be }",
                "{this string}" };

            BracketedTextRule bracketedTextRule = new BracketedTextRule(false, false, true);

            Assert.That(TestFindingRuleOnText(bracketedTextRule, testString, expectedResults));
        }

        /// <summary>
        /// Test the ability to find pairs of matching curly brackets.
        /// </summary>
        [Test, Category("Automated"), Category("Bracketed Text")]
        public static void Automated_FindCurlyBracketsMultilineAllMatched()
        {
            // Set the test string and expected results
            string testString = "{There should " + _LINE_BREAK + "be } {three " + _LINE_BREAK
                + "matches} for {this string}";
            string[] expectedResults = new string[] {
                "{There should be }",
                "{three matches}",
                "{this string}" };

            BracketedTextRule bracketedTextRule = new BracketedTextRule(false, false, true);

            Assert.That(TestFindingRuleOnText(bracketedTextRule, testString, expectedResults));
        }

        /// <summary>
        /// Test the ability to find pairs of matching curly brackets.
        /// </summary>
        [Test, Category("Automated"), Category("Bracketed Text")]
        public static void Automated_FindCurlyBracketsMultilineUnmatchedOpening()
        {
            // Set the test string and expected results
            string testString = "{There should " + _LINE_BREAK + "be } {two matches "
                + _LINE_BREAK + "for {this string}";
            string[] expectedResults = new string[] {
                "{There should be }",
                "{this string}" };

            BracketedTextRule bracketedTextRule = new BracketedTextRule(false, false, true);

            Assert.That(TestFindingRuleOnText(bracketedTextRule, testString, expectedResults));
        }

        /// <summary>
        /// Test the ability to find pairs of matching curly brackets.
        /// </summary>
        [Test, Category("Automated"), Category("Bracketed Text")]
        public static void Automated_FindCurlyBracketsMultilineUnmatchedClosing()
        {
            // Set the test string and expected results
            string testString = "{There should " + _LINE_BREAK + "be } two matches"
                + _LINE_BREAK + "} for {this string}";
            string[] expectedResults = new string[] {
                "{There should be }",
                "{this string}" };

            BracketedTextRule bracketedTextRule = new BracketedTextRule(false, false, true);

            Assert.That(TestFindingRuleOnText(bracketedTextRule, testString, expectedResults));
        }

        /// <summary>
        /// Test the ability to find pairs of matching curly brackets.
        /// </summary>
        [Test, Category("Automated"), Category("Bracketed Text")]
        public static void Automated_FindCurlyBracketsMultiplePagesAllMatched()
        {
            // Set the test string and expected results
            string testString = "{There should " + _LINE_BREAK + "be } {three " + _PAGE_BREAK
                + "matches} for {this " + _PAGE_BREAK + "string}";
            string[] expectedResults = new string[] {
                "{There should be }",
                "{three matches}",
                "{this string}" };

            BracketedTextRule bracketedTextRule = new BracketedTextRule(false, false, true);

            Assert.That(TestFindingRuleOnText(bracketedTextRule, testString, expectedResults));
        }

        /// <summary>
        /// Test the ability to find pairs of matching curly brackets.
        /// </summary>
        [Test, Category("Automated"), Category("Bracketed Text")]
        public static void Automated_FindCurlyBracketsMultiplePagesUnmatchedOpening()
        {
            // Set the test string and expected results
            string testString = "{There should " + _PAGE_BREAK + "be } {two matches "
                + _PAGE_BREAK + "for {this " + _LINE_BREAK + "string}";
            string[] expectedResults = new string[] {
                "{There should be }",
                "{this string}" };

            BracketedTextRule bracketedTextRule = new BracketedTextRule(false, false, true);

            Assert.That(TestFindingRuleOnText(bracketedTextRule, testString, expectedResults));
        }

        /// <summary>
        /// Test the ability to find pairs of matching curly brackets.
        /// </summary>
        [Test, Category("Automated"), Category("Bracketed Text")]
        public static void Automated_FindCurlyBracketsMultiplePagesUnmatchedClosing()
        {
            // Set the test string and expected results
            string testString = "{There should " + _PAGE_BREAK + "be } two matches"
                + _PAGE_BREAK + "} for {this" + _LINE_BREAK + " string}";
            string[] expectedResults = new string[] {
                "{There should be }",
                "{this string}" };

            BracketedTextRule bracketedTextRule = new BracketedTextRule(false, false, true);

            Assert.That(TestFindingRuleOnText(bracketedTextRule, testString, expectedResults));
        }

        /// <summary>
        /// Test the ability to find nested pairs of matching curly brackets.
        /// </summary>
        [Test, Category("Automated"), Category("Bracketed Text")]
        public static void Automated_FindNestedCurlyBracketsAllMatched()
        {
            // Set the test string and expected results
            string testString = "{There should {be {three matches} for this} string}";
            string[] expectedResults = new string[] {
                "{There should {be {three matches} for this} string}",
                "{be {three matches} for this}",
                "{three matches}" };

            BracketedTextRule bracketedTextRule = new BracketedTextRule(false, false, true);

            Assert.That(TestFindingRuleOnText(bracketedTextRule, testString, expectedResults));
        }

        /// <summary>
        /// Test the ability to find nested pairs of matching curly brackets.
        /// </summary>
        [Test, Category("Automated"), Category("Bracketed Text")]
        public static void Automated_FindNestedCurlyBracketsUnmatchedOpening()
        {
            // Set the test string and expected results
            string testString = "{There should {be {two matches for this} string}";
            string[] expectedResults = new string[] {
                "{be {two matches for this} string}",
                "{two matches for this}" };

            BracketedTextRule bracketedTextRule = new BracketedTextRule(false, false, true);

            Assert.That(TestFindingRuleOnText(bracketedTextRule, testString, expectedResults));
        }

        /// <summary>
        /// Test the ability to find nested pairs of matching curly brackets.
        /// </summary>
        [Test, Category("Automated"), Category("Bracketed Text")]
        public static void Automated_FindNestedCurlyBracketsUnmatchedClosing()
        {
            // Set the test string and expected results
            string testString = "{There should be {two matches for} this} string}";
            string[] expectedResults = new string[] {
                "{There should be {two matches for} this}",
                "{two matches for}" };

            BracketedTextRule bracketedTextRule = new BracketedTextRule(false, false, true);

            Assert.That(TestFindingRuleOnText(bracketedTextRule, testString, expectedResults));
        }

        /// <summary>
        /// Test the ability to find nested pairs of matching curly brackets across multiple lines.
        /// </summary>
        [Test, Category("Automated"), Category("Bracketed Text")]
        public static void Automated_FindNestedCurlyBracketsMultilineAllMatched()
        {
            // Set the test string and expected results
            string testString = "{There " + _LINE_BREAK + "should {be " + _LINE_BREAK + "{three " 
                + _LINE_BREAK + "matches} for " + _LINE_BREAK + "this} string}";
            string[] expectedResults = new string[] {
                "{There should {be {three matches} for this} string}",
                "{be {three matches} for this}",
                "{three matches}" };

            BracketedTextRule bracketedTextRule = new BracketedTextRule(false, false, true);

            Assert.That(TestFindingRuleOnText(bracketedTextRule, testString, expectedResults));
        }

        /// <summary>
        /// Test the ability to find nested pairs of matching curly brackets across multiple lines.
        /// </summary>
        [Test, Category("Automated"), Category("Bracketed Text")]
        public static void Automated_FindNestedCurlyBracketsMultilineUnmatchedOpening()
        {
            // Set the test string and expected results
            string testString = "{There " + _LINE_BREAK + "should {be " + _LINE_BREAK 
                + "{two matches " + _LINE_BREAK + "for this} " + _LINE_BREAK + "string}";
            string[] expectedResults = new string[] {
                "{be {two matches for this} string}",
                "{two matches for this}" };

            BracketedTextRule bracketedTextRule = new BracketedTextRule(false, false, true);

            Assert.That(TestFindingRuleOnText(bracketedTextRule,
                testString, expectedResults));
        }

        /// <summary>
        /// Test the ability to find nested pairs of matching curly brackets across multiple lines.
        /// </summary>
        [Test, Category("Automated"), Category("Bracketed Text")]
        public static void Automated_FindNestedCurlyBracketsMultilineUnmatchedClosing()
        {
            // Set the test string and expected results
            string testString = "{There " + _LINE_BREAK + "should be " + _LINE_BREAK + "{two " 
                + _LINE_BREAK + "matches for} " + _LINE_BREAK + "this} " + _LINE_BREAK + "string}";
            string[] expectedResults = new string[] {
                "{There should be {two matches for} this}",
                "{two matches for}" };

            BracketedTextRule bracketedTextRule = new BracketedTextRule(false, false, true);

            Assert.That(TestFindingRuleOnText(bracketedTextRule,
                testString, expectedResults));
        }

        #endregion Curly Brackets
    }
}
