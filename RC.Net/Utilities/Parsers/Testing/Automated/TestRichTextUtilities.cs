using Extract.Testing.Utilities;
using NUnit.Framework;
using System.Linq;
using System.Windows.Forms;
using UCLID_COMUTILSLib;

namespace Extract.Utilities.Parsers.Test
{
    /// <summary>
    /// Class for testing the ConfigSettings class
    /// </summary>
    [TestFixture]
    [Category("Automatic")]
    public class TestRichTextUtilities
    {
        #region Constants

        const string HEADER = @"\rtf1\ansi\ansicpg1252\deff0\nouicompat\deflang1033";
        const string COLOR_TABLE =
            @"{\colortbl;" + 
            @"\red0\green0\blue0;" + 
            @"\red0\green0\blue255;" + 
            @"\red0\green255\blue255;" + 
            @"\red0\green255\blue0;" + 
            @"\red255\green0\blue255;" + 
            @"\red255\green0\blue0;" + 
            @"\red255\green255\blue0;" + 
            @"\red255\green255\blue255;" + 
            @"\red0\green0\blue128;" + 
            @"\red0\green128\blue128;" + 
            @"\red0\green128\blue0;" + 
            @"\red128\green0\blue128;" + 
            @"\red128\green0\blue0;" + 
            @"\red128\green128\blue0;" + 
            @"\red128\green128\blue128;" + 
            @"\red192\green192\blue192;" + 
            @"}";

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

        /// <summary>
        /// This tests that the utility method (RichTextUtilities.CouldRichTextRedactionChangePrecedingCode)
        /// prevents alpha redactions from changing the meaning of preceding control words that end in alpha chars
        /// </summary>
        [Test]
        public static void AlphaRedactionStartsAfterControlWordEndingInLetter()
        {
            string header = @"\rtf1\ansi\ansicpg1252\deff0\nouicompat\deflang1033";
            string input = @"{" + header + @"\par[RedactMe]}";

            string parsed = GetVisibleText(input);

            // Confirm expected meaning
            Assert.AreEqual("\n[RedactMe]", parsed);

            int destinationStart = header.Length + 1; // there is a leading '{' that isn't in the header length
            int redactionStart = destinationStart + 4;

            // Confirm changed meaning if redaction with alpha chars were to be done unsafely (RTF is now one unrecognized control word and thus will display as an empty string)
            string redacted = PerformRedaction(input, redactionStart, 10, 'X', false);
            string expectedCode = @"{" + header + @"\parXXXXXXXXXX}";
            Assert.AreEqual(expectedCode, redacted);

            parsed = GetVisibleText(redacted);
            Assert.AreEqual("", parsed);

            // Confirm that the rich text utility method can be used to prevent this
            redacted = PerformRedaction(input, redactionStart, 10, 'X', true);
            expectedCode = @"{" + header + @"\par XXXXXXXXXX}"; // Extra space delimits and becomes part of the preceding control word
            Assert.AreEqual(expectedCode, redacted);

            parsed = GetVisibleText(redacted);
            Assert.AreEqual("\nXXXXXXXXXX", parsed); // Extra space is not shown because it is part of the preceding control word
        }

        /// <summary>
        /// This tests that the utility method (RichTextUtilities.CouldRichTextRedactionChangePrecedingCode)
        /// isn't needed/has no effect when the preceding control word is already delimited with a space or other char
        /// </summary>
        [Test]
        public static void AlphaRedactionStartsAfterDelimiter()
        {
            string header = @"\rtf1\ansi\ansicpg1252\deff0\nouicompat\deflang1033";
            string input = @"{" + header + @"\par [RedactMe]}";

            string parsed = GetVisibleText(input);

            // Confirm expected meaning
            Assert.AreEqual("\n[RedactMe]", parsed); // Space is not shown because it is part of the preceding control word

            int destinationStart = header.Length + 1; // there is a leading '{' that isn't in the header length
            int redactionStart = destinationStart + 5;

            // Confirm that meaning is unchanged even if redaction with alpha chars were to be done unsafely
            string redacted = PerformRedaction(input, redactionStart, 10, 'X', false);
            string expectedCode = @"{" + header + @"\par XXXXXXXXXX}";
            Assert.AreEqual(expectedCode, redacted);

            parsed = GetVisibleText(redacted);
            Assert.AreEqual("\nXXXXXXXXXX", parsed); // Space is not shown because it is part of the preceding control word

            // Confirm that the rich text utility method has no effect
            redacted = PerformRedaction(input, redactionStart, 10, 'X', true);
            expectedCode = @"{" + header + @"\par XXXXXXXXXX}";
            Assert.AreEqual(expectedCode, redacted);
        }

        /// <summary>
        /// This tests that the utility method (RichTextUtilities.CouldRichTextRedactionChangePrecedingCode)
        /// prevents numeric redactions from changing the meaning of preceding control words that end in alpha chars
        /// </summary>
        [Test]
        public static void NumericRedactionStartsAfterControlWordEndingInLetter()
        {
            string header = @"\rtf1\ansi\ansicpg1252\deff0\nouicompat\deflang1033";
            string input = @"{" + header + @"\par[RedactMe]}";

            string parsed = GetVisibleText(input);

            // Confirm expected meaning
            Assert.AreEqual("\n[RedactMe]", parsed);

            int destinationStart = header.Length + 1; // there is a leading '{' that isn't in the header length
            int redactionStart = destinationStart + 4;

            // Confirm changed meaning if redaction with numeric chars were to be done unsafely (RTF is now one unrecognized control word and thus will display as an empty string)
            string redacted = PerformRedaction(input, redactionStart, 10, '0', false);
            string expectedCode = @"{" + header + @"\par0000000000}";
            Assert.AreEqual(expectedCode, redacted);

            parsed = GetVisibleText(redacted);
            Assert.AreEqual("", parsed);

            // Confirm that the rich text utility method can be used to prevent this
            redacted = PerformRedaction(input, redactionStart, 10, '0', true);
            expectedCode = @"{" + header + @"\par 0000000000}"; // Extra space delimits and becomes part of the preceding control word
            Assert.AreEqual(expectedCode, redacted);

            parsed = GetVisibleText(redacted);
            Assert.AreEqual("\n0000000000", parsed); // Extra space is not shown because it is part of the preceding control word
        }

        /// <summary>
        /// This tests that the utility method (RichTextUtilities.CouldRichTextRedactionChangePrecedingCode)
        /// prevents dash redactions from being shortened when following control words ending in alpha chars
        /// </summary>
        [Test]
        public static void DashRedactionStartsAfterControlWordEndingInLetter()
        {
            string header = @"\rtf1\ansi\ansicpg1252\deff0\nouicompat\deflang1033";
            string input = @"{" + header + @"\par[RedactMe]}";

            string parsed = GetVisibleText(input);

            // Confirm expected meaning
            Assert.AreEqual("\n[RedactMe]", parsed);

            int destinationStart = header.Length + 1; // there is a leading '{' that isn't in the header length
            int redactionStart = destinationStart + 4;

            // Confirm changed meaning if redaction with numeric chars were to be done unsafely (text is missing a char)
            string redacted = PerformRedaction(input, redactionStart, 10, '-', false);
            string expectedCode = @"{" + header + @"\par----------}";
            Assert.AreEqual(expectedCode, redacted);

            parsed = GetVisibleText(redacted);
            Assert.AreEqual("\n---------", parsed);

            // Confirm that the rich text utility method can be used to prevent this
            redacted = PerformRedaction(input, redactionStart, 10, '-', true);
            expectedCode = @"{" + header + @"\par ----------}"; // Extra space delimits and becomes part of the preceding control word
            Assert.AreEqual(expectedCode, redacted);

            parsed = GetVisibleText(redacted);
            Assert.AreEqual("\n----------", parsed); // Extra space is not shown because it is part of the preceding control word
        }

        /// <summary>
        /// This tests that the utility method (RichTextUtilities.CouldRichTextRedactionChangePrecedingCode)
        /// isn't needed/has no effect for dash redactions when the preceding control word ends in a number
        /// </summary>
        [Test]
        public static void DashRedactionStartsAfterControlWordEndingInNumber()
        {
            string input = @"{" + HEADER + COLOR_TABLE + @"\f1\cb1\cf2[RedactMe]}";

            string parsed = GetVisibleText(input);

            // Confirm expected meaning
            Assert.AreEqual("[RedactMe]", parsed);

            int destinationStart = HEADER.Length + COLOR_TABLE.Length + 1; // there is a leading '{' that isn't in the header length
            int redactionStart = destinationStart + 11;

            // Confirm that meaning is unchanged even if redaction with alpha chars were to be done unsafely
            string redacted = PerformRedaction(input, redactionStart, 10, '-', false);
            string expectedCode = @"{" + HEADER + COLOR_TABLE + @"\f1\cb1\cf2----------}";
            Assert.AreEqual(expectedCode, redacted);

            parsed = GetVisibleText(redacted);
            Assert.AreEqual("----------", parsed);

            // Confirm that the rich text utility method has no effect
            redacted = PerformRedaction(input, redactionStart, 10, '-', true);
            expectedCode = @"{" + HEADER + COLOR_TABLE + @"\f1\cb1\cf2----------}";
            Assert.AreEqual(expectedCode, redacted);
        }

        /// <summary>
        /// This tests that the utility method (RichTextUtilities.CouldRichTextRedactionChangePrecedingCode)
        /// prevents numeric redactions from changing the meaning of preceding control words that end in numeric chars
        /// </summary>
        [Test]
        public static void NumericRedactionStartsAfterControlWordEndingInNumber()
        {
            string input = @"{" + HEADER + COLOR_TABLE + @"\f1\cb1\cf2[RedactMe]}";

            string parsed = GetVisibleText(input);

            // Confirm expected meaning
            Assert.AreEqual("[RedactMe]", parsed);

            int destinationStart = HEADER.Length + COLOR_TABLE.Length + 1; // there is a leading '{' that isn't in the header length
            int redactionStart = destinationStart + 11;

            // Confirm changed meaning when redaction starts with a number (redaction is now part of a color table index)
            string redacted = PerformRedaction(input, redactionStart, 10, '0', false);
            string expectedCode = @"{" + HEADER + COLOR_TABLE + @"\f1\cb1\cf20000000000}";
            Assert.AreEqual(expectedCode, redacted);

            parsed = GetVisibleText(redacted);
            Assert.AreEqual("", parsed);

            // Confirm that the rich text utility method can be used to prevent this
            redacted = PerformRedaction(input, redactionStart, 10, '0', true);
            expectedCode = @"{" + HEADER + COLOR_TABLE + @"\f1\cb1\cf2 0000000000}"; // Extra space delimits and becomes part of the preceding control word
            Assert.AreEqual(expectedCode, redacted);

            parsed = GetVisibleText(redacted);
            Assert.AreEqual("0000000000", parsed); // Extra space is not shown because it is part of the preceding control word

            // Confirm that we don't need to separate the redaction if it starts with a letter
            redacted = PerformRedaction(input, redactionStart, 10, 'X', true);
            expectedCode = @"{" + HEADER + COLOR_TABLE + @"\f1\cb1\cf2XXXXXXXXXX}";
            Assert.AreEqual(expectedCode, redacted);

            parsed = GetVisibleText(redacted);
            Assert.AreEqual("XXXXXXXXXX", parsed);
        }

        #endregion Tests

        #region Helper Methods

        static string GetVisibleText(string rtf)
        {
            using (var rtfBox = new RichTextBox())
            {
                rtfBox.Rtf = rtf;
                return rtfBox.Text;
            }
        }

        static string PerformRedaction(string input, int redactionStartIndex, int redactionLength, char redactionChar, bool safe)
        {
            bool precedingCodeNeedsDelimiter = safe && RichTextUtilities.CouldRichTextRedactionChangePrecedingCode(input, redactionStartIndex, redactionChar);
            string redacted = new string(
                input
                .Take(redactionStartIndex)
                .Concat(precedingCodeNeedsDelimiter ? Enumerable.Repeat(' ', 1) : Enumerable.Empty<char>())
                .Concat(Enumerable.Repeat(redactionChar, redactionLength))
                .Concat(input.Skip(redactionStartIndex + redactionLength))
                .ToArray());

            return redacted;
        }

        #endregion Helper Methods

    }
}
