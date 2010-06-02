using Extract.Licensing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;

namespace Extract.Utilities
{
    /// <summary>
    /// A class for reading a text containing comments and parsing it into a
    /// collection of lines with the comments removed.
    /// </summary>
    // This class does not implement a collection.  It allows a commented text file
    // to be parsed and implements the IEnumerable interface so that the non-commented
    // lines can be easily iterated over.
    [SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
    public class CommentedTextFileReader : IEnumerable<string>
    {
        #region Fields

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(CommentedTextFileReader).ToString();

        /// <summary>
        /// The collection of lines from the file.
        /// </summary>
        List<string> _lines = new List<string>();

        /// <summary>
        /// Whether blank lines should be ignored or not.
        /// </summary>
        bool _ignoreBlankLines;

        /// <summary>
        /// The strings which indicate the beginning of a single line comment.
        /// </summary>
        List<string> _singleLineComments = new List<string>();

        /// <summary>
        /// The string that indicates the beginning of a multi-line comment.
        /// </summary>
        string _multilineCommentStart;

        /// <summary>
        /// The string that indicates the end of a multi-line comment.
        /// </summary>
        string _multilineCommentEnd;

        #endregion Fields

        #region Constructors

        /// <overloads>
        /// Initializes a new instance of the <see cref="CommentedTextFileReader"/> class.
        /// </overloads>
        /// <summary>
        /// Initializes a new instance of the <see cref="CommentedTextFileReader"/> class
        /// with default encoding of <see cref="Encoding.ASCII"/>, a default single
        /// line comment specified as '//', and default multi-line comments
        /// delimited by '/*' and '*/'
        /// </summary>
        /// <param name="fileName">The file to open and read text from.</param>
        public CommentedTextFileReader(string fileName)
            : this(fileName, Encoding.ASCII, true, new string[] { "//" }, "/*", "*/")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommentedTextFileReader"/> class
        /// with a specified encoding, a default single line comment specified as '//',
        /// and default multi-line comments delimited by '/*' and '*/'
        /// </summary>
        /// <param name="fileName">The file to open and read text from.</param>
        /// <param name="encoding">The <see cref="Encoding"/> to use when reading
        /// the text file.</param>
        public CommentedTextFileReader(string fileName, Encoding encoding)
            : this(fileName, encoding, true, new string[] { "//" }, "/*", "*/")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommentedTextFileReader"/> class
        /// with a specified encoding, a default single line comment specified as '//',
        /// and default multi-line comments delimited by '/*' and '*/'
        /// </summary>
        /// <param name="fileName">The file to open and read text from.</param>
        /// <param name="encoding">The <see cref="Encoding"/> to use when reading
        /// the text file.</param>
        /// <param name="ignoreBlankLines">Whether empty lines should be ignored or not.</param>
        public CommentedTextFileReader(string fileName, Encoding encoding, bool ignoreBlankLines)
            : this(fileName, encoding, ignoreBlankLines, new string[] {"//"}, "/*", "*/")
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommentedTextFileReader"/> class
        /// </summary>
        /// <param name="fileName">The file to open and read text from.</param>
        /// <param name="encoding">The <see cref="Encoding"/> to use when reading
        /// the text file.</param>
        /// <param name="ignoreEmptyLines">Whether empty lines should be ignored or not.</param>
        /// <param name="singleLineComments">The collection of strings which specify the
        /// beginning of a single line comment.</param>
        /// <param name="multilineCommentStart">The string which indicates the beginning of
        /// a multi-line comment.</param>
        /// <param name="multilineCommentEnd">The string which indicates the end of a multi-line
        /// comment.</param>
        public CommentedTextFileReader(string fileName, Encoding encoding, bool ignoreEmptyLines,
            IEnumerable<string> singleLineComments, string multilineCommentStart,
            string multilineCommentEnd)
        {
            try
            {
                // Ensure the file name has been specified
                ExtractException.Assert("ELI30147", "File name cannot be null or empty string.",
                    !string.IsNullOrEmpty(fileName));

                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects,
                    "ELI30148", _OBJECT_NAME);

                _ignoreBlankLines = ignoreEmptyLines;
                _singleLineComments.AddRange(singleLineComments);
                _multilineCommentStart = multilineCommentStart;
                _multilineCommentEnd = multilineCommentEnd;

                // Read the contents from the file into the internal string list.
                ReadFileContents(fileName, encoding);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI30149", ex);
            }
        }

        /// <summary>
        /// Reads the contents of the text file into an internal list of lines.
        /// All comments will be removed. If empty lines are being ignored then
        /// these will be removed as well. 
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="encoding"></param>
        void ReadFileContents(string fileName, Encoding encoding)
        {
            string fileContents = File.ReadAllText(fileName, encoding);

            // Strip out all multiline comments from the file
            int multiLineStart = fileContents.IndexOf(_multilineCommentStart,
                StringComparison.CurrentCulture);
            while (multiLineStart != -1)
            {
                // Search for the ending
                int multiLineEnd = fileContents.IndexOf(_multilineCommentEnd,
                    StringComparison.CurrentCulture);
                if (multiLineEnd == -1)
                {
                    ExtractException ee = new ExtractException("ELI30150",
                        "Mismatched multi-line comment in file.");
                    ee.AddDebugData("File Name", fileName, false);
                    ee.AddDebugData("Multi-line comment start", _multilineCommentStart, false);
                    ee.AddDebugData("Multi-line comment end", _multilineCommentEnd, false);
                    ee.AddDebugData("Index Of Multi-line start", multiLineStart, false);
                    throw ee;
                }

                // Remove the comment text
                fileContents = fileContents.Remove(multiLineStart,
                    (multiLineEnd - multiLineStart) + _multilineCommentEnd.Length);

                // Search again
                multiLineStart = fileContents.IndexOf(_multilineCommentStart, multiLineStart,
                    StringComparison.CurrentCulture);
            }

            string[] lines = fileContents.Split(new string[] {Environment.NewLine},
                _ignoreBlankLines ? StringSplitOptions.RemoveEmptyEntries : StringSplitOptions.None);
            foreach(string line in lines)
            {
                // Trim the line
                string text = line.Trim();

                // Check for single line comment text
                int index = StringMethods.FindIndexOfAny(text, _singleLineComments,
                    StringComparison.CurrentCulture);
                if (index != -1)
                {
                    // Get the substring from 0 to the comment start (trim the resulting string)
                    string temp = text.Substring(0, index).Trim();
                    if (!string.IsNullOrEmpty(temp))
                    {
                        // Add the string into the collection if it is not empty
                        _lines.Add(temp);
                    }
                }
                // If either the string is not empty or blank lines are
                // not being ignored, add the line to the collection
                else if (!string.IsNullOrEmpty(text) || !_ignoreBlankLines)
                {
                    _lines.Add(text);
                }
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets the collection of lines read from the file.
        /// </summary>
        public ReadOnlyCollection<string> Lines
        {
            get
            {
                return _lines.AsReadOnly();
            }
        }

        #endregion Properties

        #region IEnumerable<string> Members

        /// <summary>
        /// Gets and enumerator for the <see cref="CommentedTextFileReader"/>.
        /// </summary>
        /// <returns>The next line from the text file.</returns>
        public IEnumerator<string> GetEnumerator()
        {
            foreach (string line in _lines)
            {
                yield return line;
            }
        }

        /// <summary>
        /// Gets the enumerator for the IEnumerable interface.
        /// </summary>
        /// <returns>The enumerator.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion IEnumerable<string> Members
    }
}
