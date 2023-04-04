using Extract.GoogleCloud.Dto;
using GdPicture14;
using System;
using System.Text;

namespace Extract.GdPicture
{
    internal class OcrIterator: IRecognizedCharactersIterator
    {
        readonly GdPictureOCR api;
        readonly string resultID;
        readonly int characterCount;
        int currentCharacter;
        int currentWord;
        int currentLine;
        int currentParagraph;
        int currentBlock;

        /// <summary>
        /// Create a new instance that references the specified GdPictureImaging API instance
        /// </summary>
        public OcrIterator(GdPictureOCR api, string resultID)
        {
            this.api = api;
            this.resultID = resultID;

            characterCount = api.GetCharacterCount(resultID);
        }

        /// <summary>
        /// Create a new instance that copies the state of the specified source and references the same GdPictureImaging API instance
        /// </summary>
        public OcrIterator(OcrIterator source)
        {
            api = source.api;
            resultID = source.resultID;

            characterCount = source.characterCount;

            currentCharacter = source.currentCharacter;
            currentWord = source.currentWord;
            currentLine = source.currentLine;
            currentParagraph = source.currentParagraph;
            currentBlock = source.currentBlock;
        }

        /// <summary>
        /// Attempt to move the current position to the start of the next instance of the specified level
        /// </summary>
        /// <param name="level">The level to search for the next instance of</param>
        /// <returns>True if there is another instance of the specified level after the current position, else false</returns>
        public bool Next(PageIteratorLevel level)
        {
            return Next(level, level);
        }

        /// <summary>
        /// Attempt to move the current position to the start of the next instance of the specified level
        /// </summary>
        /// <param name="parentLevel">The scope that determines when to stop searching for the next instance of the specified level</param>
        /// <param name="level">The level to search for the next instance of</param>
        /// <returns>True if there is another instance of the specified level after the current position, else false</returns>
        public bool Next(PageIteratorLevel parentLevel, PageIteratorLevel level)
        {
            return Next(parentLevel, level, true);
        }

        /// <summary>
        /// Attempt to move the current position to the start of the previous instance of the specified level
        /// </summary>
        /// <param name="parentLevel">The scope that determines when to stop searching for the previous instance of the specified level</param>
        /// <param name="level">The level to search for the previous instance of</param>
        /// <returns>True if there is another instance of the specified level before the current position, else false</returns>
        public bool Prev(PageIteratorLevel parentLevel, PageIteratorLevel level)
        {
            return Next(parentLevel, level, false);
        }

        /// <summary>
        /// Whether the current position is pointing to the first symbol of the specified level
        /// </summary>
        /// <param name="level">The scope to be checked for the beginning of</param>
        public bool IsAtBeginningOf(PageIteratorLevel level)
        {
            return level switch
            {
                PageIteratorLevel.Block => IsAtBeginningOfBlock(),
                PageIteratorLevel.Para => IsAtBeginningOfParagraph(),
                PageIteratorLevel.TextLine => IsAtBeginningOfLine(),
                PageIteratorLevel.Word => IsAtBeginningOfWord(),
                PageIteratorLevel.Symbol => IsAtCharacter(),
                _ => false
            };
        }

        /// <summary>
        /// Whether there is not another level in the scope of the specified parent level
        /// </summary>
        /// <param name="parentLevel">The scope that determines when to stop searching for the next instance of the specified level</param>
        /// <param name="level">The level to search for the next instance of</param>
        /// <returns>False if there is another instance of the specified level after the current position, else true</returns>
        public bool IsAtFinalOf(PageIteratorLevel parentLevel, PageIteratorLevel level)
        {
            return !HasNext(parentLevel, level);
        }

        /// <summary>
        /// Whether there is another level in the scope of the specified parent level
        /// </summary>
        /// <param name="parentLevel">The scope that determines when to stop searching for the next instance of the specified level</param>
        /// <param name="level">The level to search for the next instance of</param>
        /// <returns>True if there is another instance of the specified level after the current position, else false</returns>
        public bool HasNext(PageIteratorLevel parentLevel, PageIteratorLevel level)
        {
            var clone = new OcrIterator(this);
            return clone.Next(parentLevel, level);
        }

        
        /// <summary>
        /// Calculates a bounding box for the symbols in contained in the specified level
        /// </summary>
        /// <param name="level">The scope that determines when to stop including symbols in the calculation</param>
        /// <param name="bounds">The bounds if successful or else an empty struct</param>
        /// <returns>True if the current position is at the beginning of the specified level, else false</returns>
        public bool TryGetBoundingBox(PageIteratorLevel level, out Rect bounds)
        {
            if (!IsAtBeginningOf(level))
            {
                bounds = default;
                return false;
            }

            bounds = level switch
            {
                PageIteratorLevel.Block => GetBlockBounds(),
                PageIteratorLevel.Para => GetParagraphBounds(),
                PageIteratorLevel.TextLine => GetLineBounds(),
                PageIteratorLevel.Word => GetWordBounds(),
                PageIteratorLevel.Symbol => GetCharacterBounds(),
                _ => default
            };

            return true;
        }

        /// <summary>
        /// Average the confidence, starting with the current symbol until the end of the current, specified level
        /// </summary>
        /// <param name="level">The scope that determines when to stop including symbols in the calculation</param>
        /// <returns>The average confidence level (in the range of 0-100)</returns>
        public float GetConfidence(PageIteratorLevel level)
        {
            var (conf, count) = Fold(level, (0f, 0), (charIdx, acc) =>
            {
                var (totalConf, count) = acc;
                var charConf = api.GetCharacterConfidence(resultID, charIdx);
                return (totalConf + charConf, count + 1);
            });
            if (count == 0)
            {
                return 0;
            }
            return conf / count;
        }

        /// <summary>
        /// Concatenate the symbol strings, starting with the current symbol until the end of the current, specified level
        /// </summary>
        /// <param name="level">The scope that determines when to stop including symbols in the concatenation</param>
        /// <returns>The average confidence level (in the range of 0-100)</returns>
        public string GetText(PageIteratorLevel level)
        {
            var text = Fold(level, new StringBuilder(), (charIdx, acc) =>
            {
                return acc.Append(api.GetCharacterValue(resultID, charIdx));
            });
            return text.ToString();
        }

        Rect GetBlockBounds()
        {
            return new Rect
            (
                left: api.GetBlockLeft(resultID, currentBlock),
                right: api.GetBlockRight(resultID, currentBlock),
                top: api.GetBlockTop(resultID, currentBlock),
                bottom: api.GetBlockBottom(resultID, currentBlock)
            );
        }

        Rect GetParagraphBounds()
        {
            return new Rect
            (
                left: api.GetParagraphLeft(resultID, currentParagraph),
                right: api.GetParagraphRight(resultID, currentParagraph),
                top: api.GetParagraphTop(resultID, currentParagraph),
                bottom: api.GetParagraphBottom(resultID, currentParagraph)
            );
        }

        Rect GetLineBounds()
        {
            return new Rect
            (
                left: api.GetTextLineLeft(resultID, currentLine),
                right: api.GetTextLineRight(resultID, currentLine),
                top: api.GetTextLineTop(resultID, currentLine),
                bottom: api.GetTextLineBottom(resultID, currentLine)
            );
        }

        Rect GetWordBounds()
        {
            return new Rect
            (
                left: api.GetWordLeft(resultID, currentWord),
                right: api.GetWordRight(resultID, currentWord),
                top: api.GetWordTop(resultID, currentWord),
                bottom: api.GetWordBottom(resultID, currentWord)
            );
        }

        Rect GetCharacterBounds()
        {
            return new Rect
            (
                left: api.GetCharacterLeft(resultID, currentCharacter),
                right: api.GetCharacterRight(resultID, currentCharacter),
                top: api.GetCharacterTop(resultID, currentCharacter),
                bottom: api.GetCharacterBottom(resultID, currentCharacter)
            );
        }

        /// <summary>
        /// Advance or retreat the current position, if possible. Return true if the position has changed.
        /// </summary>
        /// <param name="parentLevel">The parent level determines where to stop the iteration
        /// (iteration stops when a new parent level is encountered)</param>
        /// <param name="level">The level to advance by (the step unit)</param>
        /// <param name="forward">Whether to advance the position (when true) or retreat it (when false)</param>
        bool Next(PageIteratorLevel parentLevel, PageIteratorLevel level, bool forward)
        {
            // Whether moving is possible or not
            bool HasNextPrevSymbol()
            {
                return forward && HasNextCharacter() || !forward && currentCharacter > 0;
            }

            // If no movement is possible then return false
            if (!HasNextPrevSymbol())
            {
                return false;
            }

            // Move the current position to the next symbol and check to see if a new item of the specified level has been reached
            // TODO: optimize this method. This implementation is based on MicrIterator, which used a poorer API.
            // The GdPictureOCR API might allow for a faster method of iterating words or paragraphs, e.g.
            bool foundNext;
            bool foundNextParent;
            do
            {
                currentCharacter = forward ? currentCharacter + 1 : currentCharacter - 1;
                UpdateCurrentPositionFromCurrentCharacter();
                foundNextParent = level != parentLevel && IsAtBeginningOf(parentLevel);
                foundNext = IsAtBeginningOf(level);
            }
            while (!foundNextParent && !foundNext && HasNextPrevSymbol());

            // Backup one position if next parent was found (no movement possible)
            if (foundNextParent)
            {
                currentCharacter = forward ? currentCharacter - 1 : currentCharacter + 1;
                UpdateCurrentPositionFromCurrentCharacter();
                return false;
            }
            return foundNext;
        }

        // Set all current markers based on the current character pointer
        void UpdateCurrentPositionFromCurrentCharacter()
        {
            currentWord = api.GetCharacterWordIndex(resultID, currentCharacter);
            currentLine = api.GetWordLineIndex(resultID, currentWord);
            currentParagraph = api.GetTextLineParagraphIndex(resultID, currentLine);
            currentBlock = api.GetParagraphBlockIndex(resultID, currentParagraph);
        }

        // Reduce all symbols in the scope of the current specified level by repeatedly running func,
        // passing the returned accumulator to the next invocation
        T Fold<T>(PageIteratorLevel level, T accumulator, Func<int, T, T> func)
        {
            if (!IsAtCharacter())
            {
                return accumulator;
            }

            if (level == PageIteratorLevel.Symbol)
            {
                return func(currentCharacter, accumulator);
            }

            var iter = new OcrIterator(this);
            if (!iter.IsAtBeginningOf(level))
            {
                while (iter.Prev(level, PageIteratorLevel.Symbol))
                { }

                if (!iter.IsAtBeginningOf(level))
                {
                    return accumulator;
                }
            }

            do
            {
                accumulator = func(iter.currentCharacter, accumulator);
            }
            while (iter.Next(level, PageIteratorLevel.Symbol));

            return accumulator;
        }

        // Whether there is another character after the current character (forward direction)
        bool HasNextCharacter()
        {
            return characterCount > 0 && currentCharacter < (characterCount - 1);
        }

        // Whether the current character pointer is valid
        bool IsAtCharacter()
        {
            return currentCharacter >= 0 && currentCharacter < characterCount;
        }

        // Whether the current character is the first character of a block
        bool IsAtBeginningOfBlock()
        {
            return IsAtCharacter()
                && currentParagraph == api.GetBlockFirstParagraphIndex(resultID, currentBlock)
                && IsAtBeginningOfParagraph();
        }

        // Whether the current character is the first character of a paragraph
        bool IsAtBeginningOfParagraph()
        {
            return IsAtCharacter()
                && currentLine == api.GetParagraphFirstTextLineIndex(resultID, currentParagraph)
                && IsAtBeginningOfLine();
        }

        // Whether the current character is the first character of a line
        bool IsAtBeginningOfLine()
        {
            return IsAtCharacter()
                && currentWord == api.GetTextLineFirstWordIndex(resultID, currentLine)
                && IsAtBeginningOfWord();
        }

        // Whether the current symbol is the first symbol of a word
        bool IsAtBeginningOfWord()
        {
            return IsAtCharacter()
                && currentCharacter == api.GetWordFirstCharacterIndex(resultID, currentWord);
        }
    }
}
