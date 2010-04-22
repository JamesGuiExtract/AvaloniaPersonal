using Extract;
using Extract.Utilities;
using Extract.Testing.Utilities;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Extract.Utilities.Test
{
    /// <summary>
    /// Class for testing <see cref="FileActionManagerPathTags"/>
    /// </summary>
    [TestFixture]
    [Category("TestFileActionManagerPathTags")]
    public class TestFileActionManagerPathTags
    {
        #region Constants

        const string _TEST_DRIVE = @"C:\";
        const string _TEST_FOLDERS = @"temp1\temp2";
        const string _TEST_DIR = _TEST_DRIVE + _TEST_FOLDERS;
        const string _TEST_EXT = "tif";
        const string _FILE_NAME = @"filename";
        const string _TEST_FILE_PATH = _TEST_DIR + "\\" + _FILE_NAME + "." + _TEST_EXT;
        const string _TEST_FILE_NO_EXT = @"C:\temp1\temp2\filename";
        const string _FPS_FILE_DIR = @"C:\folder1\folder2\fpsFiles";

        #endregion Constants

        #region Fields

        /// <summary>
        /// A <see cref="FileActionManagerPathTags"/> object used to test tag expansion.
        /// </summary>
        FileActionManagerPathTags _fileTags =
            new FileActionManagerPathTags(_TEST_FILE_PATH, _FPS_FILE_DIR);

        #endregion Fields

        #region TestSetup

        /// <summary>
        /// Performs initialization needed for the entire test run.
        /// </summary>
        [TestFixtureSetUp]
        public static void Initialize()
        {
            GeneralMethods.TestSetup();
        }

        #endregion TestSetup

        #region Tests

        /// <summary>
        /// Tests that SourceDocName is expanded correctly.
        /// </summary>
        [Test]
        public void SourceDocName()
        {
            string tag = "<SourceDocName>";
            Assert.That(_fileTags.Expand(tag).Equals(_TEST_FILE_PATH));
        }

        /// <summary>
        /// Tests that FPSFileDir is expanded correctly.
        /// </summary>
        [Test]
        public void FpsFileDir()
        {
            string tag = "<FPSFileDir>";
            Assert.That(_fileTags.Expand(tag).Equals(_FPS_FILE_DIR));
        }

        /// <summary>
        /// Tests whether or not the ChangeExt function throws an exception
        /// when a file name has no extension.
        /// </summary>
        [Test]
        public void ChangeExtNoExt()
        {
            string tag = "$ChangeExt(" + _TEST_FILE_NO_EXT + ",pdf)";
            try
            {
                _fileTags.Expand(tag);
                Assert.Fail();
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOf<ExtractException>(ex);
            }
        }

        /// <summary>
        /// Tests the ChangeExt function.
        /// </summary>
        [Test]
        public void ChangeExtWithExt()
        {
            string tag = "$ChangeExt(" + _TEST_FILE_PATH + ",pdf)";
            Assert.That(_fileTags.Expand(tag).Equals(_TEST_FILE_NO_EXT + ".pdf"));
        }

        /// <summary>
        /// Tests the DirNoDriveOf function.
        /// </summary>
        [Test]
        public void DirNoDriveOf()
        {
            string tag = @"$DirNoDriveOf(" + _TEST_FILE_PATH + ")";
            Assert.That(_fileTags.Expand(tag).Equals(_TEST_FOLDERS));
        }

        /// <summary>
        /// Tests the DirOf function.
        /// </summary>
        [Test]
        public void DirOf()
        {
            string tag = @"$DirOf(" + _TEST_FILE_PATH + ")";
            Assert.That(_fileTags.Expand(tag).Equals(_TEST_DIR));
        }

        /// <summary>
        /// Tests the DriveOf function.
        /// </summary>
        [Test]
        public void DriveOf()
        {
            string tag = @"$DriveOf(" + _TEST_FILE_PATH + ")";
            Assert.That(_fileTags.Expand(tag).Equals(_TEST_DRIVE));
        }

        /// <summary>
        /// Tests the ExtOf function.
        /// </summary>
        [Test]
        public void ExtOf()
        {
            string tag = @"$ExtOf(" + _TEST_FILE_PATH + ")";
            Assert.That(_fileTags.Expand(tag).Equals(_TEST_EXT));
        }

        /// <summary>
        /// Tests the Env function.
        /// </summary>
        [Test]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
            MessageId = "Env")]
        public void Env()
        {
            string tag = @"$Env(PATH)";
            Console.WriteLine(_fileTags.Expand(tag));
            Console.WriteLine(Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Process));
            Assert.That(_fileTags.Expand(tag).Equals(
                Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Process),
                StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Tests whether or not the Env function throws an exception if the
        /// specified environment variable does not exist.
        /// </summary>
        [Test]
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly",
            MessageId = "Env")]
        public void EnvDoesNotExist()
        {
            try
            {
                string tag = @"$Env(THISVARIABLEDOESNOTEXISTASANENVIRONMENTVARIABLE)";
                _fileTags.Expand(tag);

                // If we get here then the test failed
                Assert.Fail();
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOf<ExtractException>(ex);
            }
        }

        /// <summary>
        /// Tests the FileNoExtOf function.
        /// </summary>
        [Test]
        public void FileNoExtOf()
        {
            string tag = @"$FileNoExtOf(" + _TEST_FILE_PATH + ")";
            Assert.That(_fileTags.Expand(tag).Equals(_FILE_NAME));
        }

        /// <summary>
        /// Tests the FileOf function.
        /// </summary>
        [Test]
        public void FileOf()
        {
            string tag = @"$FileOf(" + _TEST_FILE_PATH + ")";
            Assert.That(_fileTags.Expand(tag).Equals(_FILE_NAME + "." + _TEST_EXT));
        }

        /// <summary>
        /// Tests the FullUserName function.
        /// </summary>
        [Test]
        public void FullUserName()
        {
            string tag = @"$FullUserName()";
            try
            {
                _fileTags.Expand(tag);
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOf<ExtractException>(ex);
            }
        }

        /// <summary>
        /// Tests the InsertBeforeExt function.
        /// </summary>
        [Test]
        public void InsertBeforeExt()
        {
            string tag = @"$InsertBeforeExt(" + _TEST_FILE_PATH + ",.test)";
            Assert.That(_fileTags.Expand(tag).Equals(_TEST_FILE_NO_EXT + ".test." + _TEST_EXT));
        }

        /// <summary>
        /// Tests whether InsertBeforeExt throws an exception if the specified string has
        /// no file extension.
        /// </summary>
        [Test]
        public void InsertBeforeExtException()
        {
            try
            {
                string tag = @"$InsertBeforeExt(" + _TEST_FILE_NO_EXT + ",.test)";
                _fileTags.Expand(tag);
                Assert.Fail();
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOf<ExtractException>(ex);
            }
        }

        /// <summary>
        /// Tests the Now function with the default and a specified format.
        /// </summary>
        /// <param name="format">The format argument.</param>
        /// <param name="expression">The regular expression that the resulting date
        /// time string should be formatted like.</param>
        [Test, Sequential]
        public void Now([Values("", "%m/%d/%Y %H:%M")] string format,
            [Values(@"\d{4}-\d{2}-\d{2}-\d{2}-\d{2}-\d{2}-\d{3}",
                @"\d{2}/\d{2}/\d{4} \d{2}:\d{2}")] string expression)
        {
            string tag = @"$Now(" + format + ")";
            Assert.That(Regex.IsMatch(_fileTags.Expand(tag), expression));
        }

        /// <summary>
        /// Tests the RandomAlphaNumeric function that repeated execution produces a different
        /// string
        /// </summary>
        [Test]
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly",
            MessageId = "AlphaNumeric")]
        public void RandomAlphaNumeric()
        {
            string tag = @"$RandomAlphaNumeric(4)";
            string[] results = new string[10];
            for (int i = 0; i < results.Length; i++)
            {
                results[i] = _fileTags.Expand(tag);
            }

            for (int i = 0; i < results.Length; i++)
            {
                for (int j = i+1; j < results.Length; j++)
                {
                    Assert.That(!results[i].Equals(results[j]));
                }
            }
        }

        /// <summary>
        /// Tests the RandomAlphaNumeric function and checks whether the resulting string
        /// is the proper format.
        /// </summary>
        [Test]
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly",
            MessageId = "AlphaNumeric")]
        public void RandomAlphaNumericProperFormat()
        {
            string tag = @"$RandomAlphaNumeric(50)";
            Assert.That(Regex.IsMatch(_fileTags.Expand(tag), @"[A-Z\d]+"));
        }

        /// <summary>
        /// Tests the RandomAlphaNumeric function and checks whether the resulting string
        /// is the correct length.
        /// </summary>
        /// <param name="length">The length of the string to produce.</param>
        [Test]
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly",
            MessageId = "AlphaNumeric")]
        [CLSCompliant(false)]
        public void RandomAlphaNumericProperLength([Values(5, 7, 9, 11, 13)] int length)
        {
            string tag = @"$RandomAlphaNumeric(" +
                length.ToString(CultureInfo.InvariantCulture) + ")";
            Assert.That(_fileTags.Expand(tag).Length == length);
        }

        /// <summary>
        /// Tests whether the RandomAlphaNumeric function throws an exception with an invalid
        /// length string.
        /// </summary>
        [Test]
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly",
            MessageId = "AlphaNumeric")]
        public void RandomAlphaNumericException()
        {
            try
            {
                string tag = @"$RandomAlphaNumeric(h)";
                _fileTags.Expand(tag);
                Assert.Fail();
            }
            catch (Exception ex)
            {
                Assert.IsInstanceOf<ExtractException>(ex);
            }
        }

        /// <summary>
        /// Tests the Replace function.
        /// </summary>
        [Test]
        public void Replace()
        {
            string temp = @"C:\temp1\temp2\filename.tif";
            string tag = "$Replace(" + temp + ",te,bli)";
            Assert.That(_fileTags.Expand(tag).Equals(temp.Replace("te", "bli")));
        }

        /// <summary>
        /// Tests the UserName function.
        /// </summary>
        [Test]
        public void UserName()
        {
            string tag = @"$UserName()";
            Assert.That(_fileTags.Expand(tag).Equals(Environment.UserName));
        }

        #endregion Tests
    }
}
