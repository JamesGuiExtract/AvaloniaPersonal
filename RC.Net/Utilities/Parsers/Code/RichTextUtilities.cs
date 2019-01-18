using System;
using System.Text.RegularExpressions;

namespace Extract.Utilities.Parsers
{
    public static class RichTextUtilities
    {
        // Test whether a particular index occurs immediately after a special code
        static readonly Regex _specialCodeEndsHere = new Regex(@"\\[a-z0-9]+\G", RegexOptions.RightToLeft);

        /// <summary>
        /// Determines whether it might be possible to change the meaning of the preceding rich text by making a replacement
        /// </summary>
        /// <remarks>This logic is based on my reading of the RTF specification listed at http://www.biblioscape.com/rtf15_spec.htm </remarks>
        /// <param name="sourceText">The text to be modified</param>
        /// <param name="redactionStartIndex">The index where the replacement will start</param>
        /// <param name="redactionFirstCharacter">The first character of the replacement</param>
        /// <returns><c>true</c> if the replacement could possibly affect the meaning of the preceding code. <c>false</c> if the meaning of preceding text would definitely not be changed</returns>
        public static bool CouldRichTextRedactionChangePrecedingCode(string sourceText, int redactionStartIndex, char redactionFirstCharacter)
        {
            bool replacementTextCanDefinitelyBeDelimiter = !Char.IsLetterOrDigit(redactionFirstCharacter) && redactionFirstCharacter != '-';
            if (replacementTextCanDefinitelyBeDelimiter)
            {
                return false;
            }

            bool followsSpecialCode = _specialCodeEndsHere.IsMatch(sourceText, redactionStartIndex);
            if (followsSpecialCode)
            {
                char lastChar = sourceText[redactionStartIndex - 1];
                if (Char.IsDigit(lastChar) && Char.IsDigit(redactionFirstCharacter))
                {
                    return true;
                }
                if (Char.IsLetter(lastChar) && (Char.IsLetterOrDigit(redactionFirstCharacter) || redactionFirstCharacter == '-'))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
