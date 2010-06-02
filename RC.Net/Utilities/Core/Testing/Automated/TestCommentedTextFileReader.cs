using Extract.Testing.Utilities;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Extract.Utilities.Test
{
    /// <summary>
    /// Class for testing the <see cref="CommentedTextFileReader"/> class
    /// </summary>
    [TestFixture]
    [Category("CommentedTextFileReader")]
    public static class TestCommentedTextFileReader
    {
        #region Constants

        /// <summary>
        /// The text that should exist after comments are removed.
        /// </summary>
        const string _EXISTING_LINE_TEXT = "This line should be in the collection.";

        #endregion Constants

        #region Fields

        /// <summary>
        /// A <see cref="TemporaryFile"/> used to test the commented text file reader.
        /// </summary>
        static TemporaryFile _asciiFile;

        /// <summary>
        /// A <see cref="TemporaryFile"/> used to test the commented text file reader.
        /// </summary>
        static TemporaryFile _unicodeFile;

        /// <summary>
        /// A <see cref="TemporaryFile"/> used to test the commented text file reader.
        /// </summary>
        static TemporaryFile _asciiNonDefaultDelimiterFile;

        /// <summary>
        /// A <see cref="TemporaryFile"/> used to test the commented text file reader.
        /// </summary>
        static TemporaryFile _asciiExtraWhitespace;

        #endregion Fields

        #region TestSetup

        /// <summary>
        /// Performs initialization needed for the entire test run.
        /// </summary>
        [TestFixtureSetUp]
        public static void Initialize()
        {
            GeneralMethods.TestSetup();
            PrepareFiles();
        }

        /// <summary>
        /// Performs tear down needed after entire test run.
        /// </summary>
        [TestFixtureTearDown]
        public static void Teardown()
        {
            if (_asciiFile != null)
            {
                _asciiFile.Dispose();
                _asciiFile = null;
            }
            if (_unicodeFile != null)
            {
                _unicodeFile.Dispose();
                _unicodeFile = null;
            }
            if (_asciiNonDefaultDelimiterFile != null)
            {
                _asciiNonDefaultDelimiterFile.Dispose();
                _asciiNonDefaultDelimiterFile = null;
            }
        }

        /// <summary>
        /// Gets test text with default delimiter characters.
        /// </summary>
        /// <returns>The test text with default delimiters.</returns>
        static string GetTestTextWithDefaultDelimiters()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("/* This is a multi-line comment in the text file");
            sb.AppendLine();
            sb.AppendLine(" This text is still inside of the multi-line comment.");
            sb.AppendLine("This line ends the multi-line comment */");
            sb.AppendLine();
            sb.AppendLine(_EXISTING_LINE_TEXT);
            sb.AppendLine(_EXISTING_LINE_TEXT);
            sb.AppendLine("// This line should not be in the collection.");
            sb.AppendLine(_EXISTING_LINE_TEXT + "// But this text should not");
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine("/* Multi-line comment again*/");
            sb.AppendLine();
            sb.AppendLine(_EXISTING_LINE_TEXT);
            sb.AppendLine();

            return sb.ToString();
        }

        /// <summary>
        /// Gets test text with default delimiter characters and extra whitespace at
        /// the beginning and ending of lines.
        /// </summary>
        /// <returns>The test text with default delimiters.</returns>
        static string GetTestTextWithDefaultDelimitersAndExtraWhitespace()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("/* This is a multi-line comment in the text file");
            sb.AppendLine();
            sb.AppendLine(" This text is still inside of the multi-line comment.");
            sb.AppendLine("This line ends the multi-line comment */");
            sb.AppendLine();
            sb.AppendLine("\t\t" + _EXISTING_LINE_TEXT);
            sb.AppendLine(_EXISTING_LINE_TEXT + "\t    \t");
            sb.AppendLine("\t// This line should not be in the collection.");
            sb.AppendLine(_EXISTING_LINE_TEXT + "    // But this text should not");
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine("/* Multi-line comment again*/");
            sb.AppendLine();
            sb.AppendLine(" " + _EXISTING_LINE_TEXT);
            sb.AppendLine();

            return sb.ToString();
        }

        /// <summary>
        /// Gets test text with non-default delimiter characters.
        /// </summary>
        /// <returns>The test text with non-default delimiters.</returns>
        static string GetTestTextWithNonDefaultDelimiters()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("%:% This is a multi-line comment in the text file");
            sb.AppendLine();
            sb.AppendLine(" This text is still inside of the multi-line comment.");
            sb.AppendLine("This line ends the multi-line comment :%:");
            sb.AppendLine();
            sb.AppendLine(_EXISTING_LINE_TEXT);
            sb.AppendLine(_EXISTING_LINE_TEXT);
            sb.AppendLine("# This line should not be in the collection.");
            sb.AppendLine(_EXISTING_LINE_TEXT + "' But this text should not");
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine("%:% Multi-line comment again:%:");
            sb.AppendLine();
            sb.AppendLine(_EXISTING_LINE_TEXT);
            sb.AppendLine();

            return sb.ToString();
        }

        /// <summary>
        /// Prepares the testing files.
        /// </summary>
        static void PrepareFiles()
        {
            PrepareAsciiFile();
            PrepareUnicodeFile();
            PrepareAsciiFileNonDefaultDelimeters();
            PrepareAsciiFileWithExtraWhitespace();
        }

        /// <summary>
        /// Prepares the basic ascii text file for testing.
        /// </summary>
        static void PrepareAsciiFile()
        {
            _asciiFile = new TemporaryFile(".txt");
            File.WriteAllText(_asciiFile.FileName, GetTestTextWithDefaultDelimiters(),
                Encoding.ASCII);
        }

        /// <summary>
        /// Prepares the basic unicode text file for testing.
        /// </summary>
        static void PrepareUnicodeFile()
        {
            _unicodeFile = new TemporaryFile(".txt");
            File.WriteAllText(_unicodeFile.FileName, GetTestTextWithDefaultDelimiters(),
                Encoding.Unicode);
        }

        /// <summary>
        /// Prepares the ascii text file with different comment characters for testing.
        /// <para><b>Note:</b></para>
        /// The multi-line comment delimeters are: %:% for start and :%: for end.
        /// The single line comment delimeters are # and '
        /// </summary>
        static void PrepareAsciiFileNonDefaultDelimeters()
        {
            _asciiNonDefaultDelimiterFile = new TemporaryFile(".txt");
            File.WriteAllText(_asciiNonDefaultDelimiterFile.FileName,
                GetTestTextWithNonDefaultDelimiters(), Encoding.ASCII);
        }

        /// <summary>
        /// Prepares ascii text file with default comment characters and extra whitespace
        /// for testing.
        /// </summary>
        static void PrepareAsciiFileWithExtraWhitespace()
        {
            _asciiExtraWhitespace = new TemporaryFile(".txt");
            File.WriteAllText(_asciiExtraWhitespace.FileName,
                GetTestTextWithDefaultDelimitersAndExtraWhitespace(), Encoding.ASCII);
        }

        #endregion TestSetup

        /// <summary>
        /// Tests reading an ascii file with the blank lines removed
        /// </summary>
        [Test, Category("Automated")]
        public static void AsciiWithBlankLinesRemoved()
        {
            // Create a commented text file reader for the ascii that ignores
            // blank lines and uses default comment characters
            CommentedTextFileReader reader = new CommentedTextFileReader(_asciiFile.FileName,
                Encoding.ASCII, true);

            // Iterate through each line
            foreach (string line in reader)
            {
                if (!line.Equals(_EXISTING_LINE_TEXT, StringComparison.CurrentCulture))
                {
                    Console.WriteLine("Non-matching: " + line);
                    Assert.Fail("Non-Matching line");
                }
            }
        }

        /// <summary>
        /// Tests reading an ascii file without removing the blank lines
        /// </summary>
        [Test, Category("Automated")]
        public static void AsciiWithBlankLinesRetained()
        {
            // Create a commented text file reader for the ascii that ignores
            // blank lines and uses default comment characters
            CommentedTextFileReader reader = new CommentedTextFileReader(_asciiFile.FileName,
                Encoding.ASCII, false);

            int blankLineCount = 0;

            // Iterate through each line
            foreach (string line in reader)
            {
                if (string.IsNullOrEmpty(line))
                {
                    blankLineCount++;
                }
                else if (!line.Equals(_EXISTING_LINE_TEXT, StringComparison.CurrentCulture))
                {
                    Console.WriteLine("Non-matching: " + line);
                    Assert.Fail("Non-Matching line");
                }
            }

            Assert.That(blankLineCount > 0);
        }

        /// <summary>
        /// Tests reading a unicode file with the blank lines removed
        /// </summary>
        [Test, Category("Automated")]
        public static void UnicodeWithBlankLinesRemoved()
        {
            // Create a commented text file reader for the ascii that ignores
            // blank lines and uses default comment characters
            CommentedTextFileReader reader = new CommentedTextFileReader(_unicodeFile.FileName,
                Encoding.Unicode, true);

            // Iterate through each line
            foreach (string line in reader)
            {
                if (!line.Equals(_EXISTING_LINE_TEXT, StringComparison.CurrentCulture))
                {
                    Console.WriteLine("Non-matching: " + line);
                    Assert.Fail("Non-Matching line");
                }
            }
        }

        /// <summary>
        /// Tests reading an unicode file without removing the blank lines
        /// </summary>
        [Test, Category("Automated")]
        public static void UnicodeWithBlankLinesRetained()
        {
            // Create a commented text file reader for the ascii that ignores
            // blank lines and uses default comment characters
            CommentedTextFileReader reader = new CommentedTextFileReader(_unicodeFile.FileName,
                Encoding.ASCII, false);

            int blankLineCount = 0;

            // Iterate through each line
            foreach (string line in reader)
            {
                if (string.IsNullOrEmpty(line))
                {
                    blankLineCount++;
                }
                else if (!line.Equals(_EXISTING_LINE_TEXT, StringComparison.CurrentCulture))
                {
                    Console.WriteLine("Non-matching: " + line);
                    Assert.Fail("Non-Matching line");
                }
            }

            Assert.That(blankLineCount > 0);
        }

        /// <summary>
        /// Tests reading an ascii file with the blank lines removed
        /// </summary>
        [Test, Category("Automated")]
        public static void AsciiDifferentDelimiterWithBlankLinesRemoved()
        {
            // Create a commented text file reader for the ascii that ignores
            // blank lines and has specified single line and multi-line comment
            // delimeters
            CommentedTextFileReader reader = new CommentedTextFileReader(
                _asciiNonDefaultDelimiterFile.FileName, Encoding.ASCII, true,
                new string[] { "#", "'" }, "%:%", ":%:");

            // Iterate through each line
            foreach (string line in reader)
            {
                if (!line.Equals(_EXISTING_LINE_TEXT, StringComparison.CurrentCulture))
                {
                    Console.WriteLine("Non-matching: " + line);
                    Assert.Fail("Non-Matching line");
                }
            }
        }

        /// <summary>
        /// Tests reading an ascii file without removing the blank lines
        /// </summary>
        [Test, Category("Automated")]
        public static void AsciiDifferentDelimiterWithBlankLinesRetained()
        {
            // Create a commented text file reader for the ascii that retains
            // blank lines and has specified single line and multi-line comment
            // delimeters
            CommentedTextFileReader reader = new CommentedTextFileReader(
                _asciiNonDefaultDelimiterFile.FileName, Encoding.ASCII, false,
                new string[] { "#", "'" }, "%:%", ":%:");

            int blankLineCount = 0;

            // Iterate through each line
            foreach (string line in reader)
            {
                if (string.IsNullOrEmpty(line))
                {
                    blankLineCount++;
                }
                else if (!line.Equals(_EXISTING_LINE_TEXT, StringComparison.CurrentCulture))
                {
                    Console.WriteLine("Non-matching: " + line);
                    Assert.Fail("Non-Matching line");
                }
            }

            Assert.That(blankLineCount > 0);
        }

        /// <summary>
        /// Tests reading an ascii file containing extra whitespace with the blank lines removed
        /// </summary>
        [Test, Category("Automated")]
        public static void AsciiExtraWhiteSpaceWithBlankLinesRemoved()
        {
            // Create a commented text file reader for the ascii that ignores
            // blank lines and uses default comment characters
            CommentedTextFileReader reader = new CommentedTextFileReader(
                _asciiExtraWhitespace.FileName, Encoding.ASCII, true);

            // Iterate through each line
            foreach (string line in reader)
            {
                if (!line.Equals(_EXISTING_LINE_TEXT, StringComparison.CurrentCulture))
                {
                    Console.WriteLine("Non-matching: " + line);
                    Assert.Fail("Non-Matching line");
                }
            }
        }

        /// <summary>
        /// Tests reading an ascii file containing extra whitespace without removing the blank lines
        /// </summary>
        [Test, Category("Automated")]
        public static void AsciiExtraWhitesSpaceWithBlankLinesRetained()
        {
            // Create a commented text file reader for the ascii that ignores
            // blank lines and uses default comment characters
            CommentedTextFileReader reader = new CommentedTextFileReader(
                _asciiExtraWhitespace.FileName, Encoding.ASCII, false);

            int blankLineCount = 0;

            // Iterate through each line
            foreach (string line in reader)
            {
                if (string.IsNullOrEmpty(line))
                {
                    blankLineCount++;
                }
                else if (!line.Equals(_EXISTING_LINE_TEXT, StringComparison.CurrentCulture))
                {
                    Console.WriteLine("Non-matching: " + line);
                    Assert.Fail("Non-Matching line");
                }
            }

            Assert.That(blankLineCount > 0);
        }

        /// <summary>
        /// Tests whether or not the enumerator returns the same lines as the collection.
        /// </summary>
        [Test, Category("Automated")]
        public static void EnumeratorReturnsCorrectLines()
        {
            CommentedTextFileReader reader = new CommentedTextFileReader(_asciiFile.FileName,
                Encoding.ASCII, false);
            int i = 0;
            foreach (string line in reader)
            {
                if (!line.Equals(reader.Lines[i], StringComparison.CurrentCulture))
                {
                    Console.WriteLine("Non-matching:");
                    Console.WriteLine("Enumerator: " + line);
                    Console.WriteLine("Collection: " + reader.Lines[i]);
                    Assert.Fail("Non-Matching line");
                }
                i++;
            }
        }
    }
}
