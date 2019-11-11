using Extract.Utilities;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Text;

namespace Extract.AttributeFinder
{
    /// <summary>
    /// C# version of Letter/CPPLetter for use with CSpatialString::GetOCRImageLetterArray and CSpatialString::CreateFromLetterArray
    /// Use same layout as the cpp Letter so they can be cast to each other
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    [CLSCompliant(false)]
    public struct LetterStruct
    {
        #region Constants

        const byte UNKNOWN_CHAR = (byte)'^';

        #endregion Constants

        #region Fields

        readonly ushort _guess1;
        readonly ushort _guess2;
        readonly ushort _guess3;

        // The spatialBoundaries of the letter
        readonly uint _top;
        readonly uint _left;
        readonly uint _right;
        readonly uint _bottom;

        // max number of pages per document is limited to 65535
        // The page on which this character lies
        readonly ushort _pageNumber;

        // true if this is the last character in a paragraph
        readonly bool _endOfParagraph;

        // true if this is the last character in a zone
        readonly bool _endOfZone;

        // True if this character has spatial information
        // i.e. is a "Spatial Letter"
        readonly bool _spatial;

        // This is the font size (in points) of the letter 
        readonly byte _fontSize;

        readonly byte _charConfidence;

        readonly byte _font;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Creates a non-spatial letter
        /// </summary>
        /// <param name="code">The character as a unicode point to be converted to windows-1252</param>
        public LetterStruct(char code)
            : this(ConvertUnicodeToWindows1252(code))
        {
        }

        /// <summary>
        /// Creates a non-spatial letter
        /// </summary>
        /// <param name="code">The code point in windows-1252 codepage</param>
        public LetterStruct(byte code)
        {
            _guess1 = _guess2 = _guess3 = code;
            _top = _bottom = _left = _right = 0;
            _pageNumber = 0;
            _endOfParagraph = _endOfZone = _spatial = false;
            _fontSize = 0;
            _charConfidence = 100;
            _font = 0;
        }

        /// <summary>
        /// Creates a letter with full set of parameters
        /// </summary>
        /// <param name="code">The character as a unicode point to be converted to windows-1252</param>
        /// <param name="top">The y coordinate of top of the letter</param>
        /// <param name="right">The x coordinate of right of the letter</param>
        /// <param name="bottom">The y coordinate of bottom of the letter</param>
        /// <param name="left">The x coordinate of left of the letter</param>
        /// <param name="pageNumber">The page number of the letter</param>
        /// <param name="endOfParagraph">Whether the letter is the end of a paragraph</param>
        /// <param name="endOfZone">Whether the letter is the end of a zone</param>
        /// <param name="spatial">Whether the letter is spatial</param>
        /// <param name="fontSize">The font size, in points</param>
        /// <param name="characterConfidence">The character confidence, 1-100</param>
        /// <param name="font">The font characteristics (flags)</param>
        public LetterStruct(char code, uint top, uint right, uint bottom, uint left,
            ushort pageNumber, bool endOfParagraph, bool endOfZone, bool spatial,
            byte fontSize, byte characterConfidence, byte font)
        {
            _guess1 = _guess2 = _guess3 = ConvertUnicodeToWindows1252(code);
            _top = top;
            _bottom = bottom;
            _left = left;
            _right = right;
            _pageNumber = pageNumber;
            _endOfParagraph = endOfParagraph;
            _endOfZone = endOfZone;
            _spatial = spatial;
            _fontSize = fontSize;
            _charConfidence = characterConfidence;
            _font = font;
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// The letter's value
        /// </summary>
        public ushort Guess1 => _guess1;

        /// <summary>
        /// Unused or same as <see cref="Guess1"/>
        /// </summary>
        public ushort Guess2 => _guess2;

        /// <summary>
        /// Unused or same as <see cref="Guess1"/>
        /// </summary>
        public ushort Guess3 => _guess3;

        /// <summary>
        /// The y coordinate of top of the letter
        /// </summary>
        public uint Top => _top;

        /// <summary>
        /// The x coordinate of the left of the letter
        /// </summary>
        public uint Left => _left;

        /// <summary>
        /// The x coordinate of the right of the letter
        /// </summary>
        public uint Right => _right;

        /// <summary>
        /// The y coordinate of the bottom of the letter
        /// </summary>
        public uint Bottom => _bottom;

        /// <summary>
        /// The page number of the letter
        /// </summary>
        public ushort PageNumber => _pageNumber;

        /// <summary>
        /// True if this is the last character in a paragraph
        /// </summary>
        public bool EndOfParagraph => _endOfParagraph;
        
        /// <summary>
        /// True if this is the last character in a zone
        /// </summary>
        public bool EndOfZone => _endOfZone;

        /// <summary>
        /// True if this character has spatial information, i.e. is a "Spatial Letter"
        /// </summary>
        public bool Spatial => _spatial;

        /// <summary>
        /// This is the font size (in points) of the letter 
        /// </summary>
        public byte FontSize => _fontSize;

        /// <summary>
        /// The confidence level, from 1-100
        /// </summary>
        public byte CharConfidence => _charConfidence;

        /// <summary>
        /// Font attributes (flags, see #defines in CPPLetter.h)
        /// </summary>
        public byte Font => _font;

        /// <summary>
        /// The y coordinate of the halfway point between <see cref="Top"/> and <see cref="Bottom"/>
        /// </summary>
        public uint MidY => Top + (Bottom - Top) / 2;

        #endregion Properties

        #region Methods

        /// <summary>
        /// Determine whether the following letter is separated by a new line from this letter
        /// </summary>
        /// <param name="letterAfterThis">The letter following this instance</param>
        [SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "0#")]
        public bool IsNewLineBetween(ref LetterStruct letterAfterThis)
        {
            return MidY < letterAfterThis.Top;
        }

        #endregion Static Methods

        #region Private Methods

        private static byte ConvertUnicodeToWindows1252(char unicode)
        {
            try
            {
                var unicodeBytes = Encoding.UTF8.GetBytes(new[] { unicode });
                var asciiBytes = Encoding.Convert(Encoding.UTF8, Encoding.GetEncoding("windows-1252"), unicodeBytes);

                if (asciiBytes.Length == 1)
                {
                    return asciiBytes[0];
                }
            }
            catch { }

            return UNKNOWN_CHAR;
        }

        #endregion Private Methods

        #region Overrides

        /// <summary>
        /// Implementation to make FXCop happy
        /// </summary>
        /// <param name="obj">The object to compare to</param>
        public override bool Equals(object obj)
        {
            if (!(obj is LetterStruct other))
            {
                return false;
            }

            // Don't check m_bIsEndOfZone or m_bIsEndOfParagraph, because that depends on the context of
            // the surrounding letters, not just this one.
            return _guess1 == other._guess1 &&
                   _guess2 == other._guess2 &&
                   _guess3 == other._guess3 &&
                   _top == other._top &&
                   _left == other._left &&
                   _right == other._right &&
                   _bottom == other._bottom &&
                   _pageNumber == other._pageNumber &&
                   _spatial == other._spatial &&
                   _fontSize == other._fontSize &&
                   _charConfidence == other._charConfidence &&
                   _font == other._font;
        }

        /// <summary>
        /// Implementation to make FXCopy happy
        /// </summary>
        public override int GetHashCode()
        {
            // Don't check m_bIsEndOfZone or m_bIsEndOfParagraph, because that depends on the context of
            // the surrounding letters, not just this one.
            return HashCode.Start
                .Hash(_guess1)
                .Hash(_guess2)
                .Hash(_guess3)
                .Hash(_top)
                .Hash(_left)
                .Hash(_right)
                .Hash(_bottom)
                .Hash(_pageNumber)
                .Hash(_spatial)
                .Hash(_fontSize)
                .Hash(_charConfidence)
                .Hash(_font);
        }

        public static bool operator ==(LetterStruct letter1, LetterStruct letter2)
        {
            return letter1.Equals(letter2);
        }

        public static bool operator !=(LetterStruct letter1, LetterStruct letter2)
        {
            return !(letter1 == letter2);
        }

        #endregion Overrides
    }
}
