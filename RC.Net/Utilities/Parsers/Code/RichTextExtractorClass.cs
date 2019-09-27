using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using UCLID_COMUTILSLib;

namespace Extract.Utilities.Parsers
{
    /// <summary>
    /// COM class to expose RichTextExtractor methods
    /// </summary>
    [ComVisible(true)]
    [Guid("3D1D9CFA-DBE6-4924-8D1A-259EC97587A8")]
    [ProgId("Extract.Utilities.Parsers.RichTextExtractorClass")]
    public class RichTextExtractorClass: IRichTextExtractor
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="RichTextExtractorClass"/> class.
        /// </summary>
        public RichTextExtractorClass()
        {
        }

        #endregion Constructors

        #region Public Methods

        /// <summary>
        /// Parse RTF code and return the plain text (not including destinations) and the indexes and lengths of the characters into the original code,
        /// encoded as ten bytes per character as a string (1st byte is char, next 8 bytes are the index as hex string, 10th is the length as hex)
        /// </summary>
        /// <param name="input">The rich text code to parse</param>
        /// <param name="sourceDocName">The name to use as debug data for any exceptions</param>
        /// <param name="bThrowParseExceptions">Whether to throw or only log parse exceptions</param>
        public string GetIndexedTextFromFile(string strFileName, bool bThrowParseExceptions)
        {
            try
            {
                // Allow opening of files that are also open in Word, e.g. (ReadAllText fails)
                using (var fileStream = new FileStream(strFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var textReader = new StreamReader(fileStream))
                {
                    string rtf = textReader.ReadToEnd();
                    return Encoding
                        .GetEncoding("windows-1252")
                        .GetString(RichTextExtractor.GetIndexedText(rtf, strFileName, bThrowParseExceptions));
                }
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI48358", ex.Message);
            }
        }

        #endregion Public Methods
    }
}
