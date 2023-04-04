using Extract.GoogleCloud.Dto;
using Extract.Utilities;
using GdPicture14;
using System;
using System.Collections.Generic;
using System.Text;

namespace Extract.GdPicture
{
    internal class MicrIterator: IRecognizedCharactersIterator
    {
        public GdPictureImaging api;
        readonly int symbolsCount;
        int currentSymbol = 1;
        int currentLine;
        Dictionary<int, float>? averageCharacterWidthByLine;

        /// <summary>
        /// Create a new instance that references the specified GdPictureImaging API instance
        /// </summary>
        public MicrIterator(GdPictureImaging api)
        {
            this.api = api;
            symbolsCount = api.MICRGetSymbolsCount();
        }

        /// <summary>
        /// Create a new instance that copies the state of the specified source and references the same GdPictureImaging API instance
        /// </summary>
        public MicrIterator(MicrIterator source)
        {
            api = source.api;
            currentSymbol = source.currentSymbol;
            currentLine = source.currentLine;
            symbolsCount = source.symbolsCount;
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
                PageIteratorLevel.Block => currentSymbol == 1,
                PageIteratorLevel.Para => IsAtBeginningOfParagraph(),
                PageIteratorLevel.TextLine => IsAtBeginningOfLine(),
                PageIteratorLevel.Word => IsAtBeginningOfWord(),
                PageIteratorLevel.Symbol => IsAtSymbol(),
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
            var clone = new MicrIterator(this);
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

            bounds = new Rect
            (
                left: api.MICRGetSymbolLeft(currentSymbol),
                right: api.MICRGetSymbolRight(currentSymbol),
                top: api.MICRGetSymbolTop(currentSymbol),
                bottom: api.MICRGetSymbolBottom(currentSymbol)
            );

            return true;
        }

        /// <summary>
        /// Average the confidence, starting with the current symbol until the end of the current, specified level
        /// </summary>
        /// <param name="level">The scope that determines when to stop including symbols in the calculation</param>
        /// <returns>The average confidence level (in the range of 0-100)</returns>
        public float GetConfidence(PageIteratorLevel level)
        {
            var (conf, count) = Fold(level, (0f, 0), (symbolNum, acc) =>
            {
                var (totalConf, count) = acc;
                var symbolConf = api.MICRGetSymbolConfidence(symbolNum, 1);
                return (totalConf + symbolConf, count + 1);
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
            var text = Fold(level, new StringBuilder(), (symbolNum, acc) =>
            {
                return acc.Append(api.MICRGetSymbolValue(symbolNum, 1));
            });
            return text.ToString();
        }

        /// <summary>
        /// Advance or retreat the current position, if possible. Return true if the position has changed.
        /// </summary>
        /// <param name="parentLevel">The parent level determines where to stop the iteration
        /// (iteration stops when a new parent level is encountered)</param>
        /// <param name="level">The level to advance by (the step unit)</param>
        /// <param name="forward">Whether to advance the position (when true) or retreat it (when false)</param>
        /// <returns></returns>
        bool Next(PageIteratorLevel parentLevel, PageIteratorLevel level, bool forward)
        {
            // Whether moving is possible or not
            bool HasNextPrevSymbol()
            {
                return forward && HasNextSymbol() || !forward && currentSymbol > 1;
            }

            // If no movement is possible then return false
            if (!HasNextPrevSymbol())
            {
                return false;
            }

            // Move the current position to the next symbol and check to see if a new item of the specified level has been reached
            bool foundNext;
            bool foundNextParent;
            do
            {
                currentSymbol = forward ? currentSymbol + 1 : currentSymbol - 1;
                currentLine = api.MICRGetSymbolLine(currentSymbol);
                foundNextParent = level != parentLevel && IsAtBeginningOf(parentLevel);
                foundNext = IsAtBeginningOf(level);
            }
            while (!foundNextParent && !foundNext && HasNextPrevSymbol());

            // Backup one position if next parent was found (no movement possible)
            if (foundNextParent)
            {
                currentSymbol = forward ? currentSymbol - 1 : currentSymbol + 1;
                currentLine = api.MICRGetSymbolLine(currentSymbol);
                return false;
            }
            return foundNext;
        }

        // Reduce all symbols in the scope of the current specified level by repeatedly running func,
        // passing the returned accumulator to the next invocation
        T Fold<T>(PageIteratorLevel level, T accumulator, Func<int, T, T> func)
        {
            if (!IsAtSymbol())
            {
                return accumulator;
            }

            if (level == PageIteratorLevel.Symbol)
            {
                return func(currentSymbol, accumulator);
            }

            var iter = new MicrIterator(this);
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
                accumulator = func(iter.currentSymbol, accumulator);
            }
            while (iter.Next(level, PageIteratorLevel.Symbol));

            return accumulator;
        }

        // Whether there is another symbol after the current symbol (forward direction)
        bool HasNextSymbol()
        {
            return symbolsCount > 0 && currentSymbol < symbolsCount;
        }

        // Whether the current symbol pointer is valid
        bool IsAtSymbol()
        {
            return currentSymbol > 0 && currentSymbol <= symbolsCount;
        }

        // Whether the current symbol is the first symbol of a paragraph (where paragraph = GdPicture's MICR line numbering scheme)
        bool IsAtBeginningOfParagraph()
        {
            if (IsAtSymbol())
            {
                if (currentSymbol == 1)
                {
                    return true;
                }
                int prevLine = api.MICRGetSymbolLine(currentSymbol - 1);
                return currentLine != prevLine;
            }
            return false;
        }

        // Whether the current symbol is the first symbol of a line
        // (where a new line = the first symbol after a change in GdPicture's MICR line number
        // or a symbol that is more than 10 char-widths to the right of the previous symbol)
        bool IsAtBeginningOfLine()
        {
            if (IsAtSymbol())
            {
                if (IsAtBeginningOfParagraph())
                {
                    return true;
                }

                int prevSymbolRight = api.MICRGetSymbolRight(currentSymbol - 1);
                int thisSymbolLeft = api.MICRGetSymbolLeft(currentSymbol);
                int gapWidth = thisSymbolLeft - prevSymbolRight;

                // Gaps of 6, 8 and 10 avg char width were tested using MICR examples from TN - Davidson.
                // A gap of 8 gave the best results.
                return gapWidth > 8 * GetAverageCharacterWidth(currentLine);
            }
            return false;
        }

        // Whether the current symbol is the first symbol of a line
        // (where a new line = the first symbol after a change in GdPicture's MICR line number
        // or a symbol that is more than 2 char-widths to the right of the previous symbol)
        bool IsAtBeginningOfWord()
        {
            if (IsAtSymbol())
            {
                if (IsAtBeginningOfParagraph())
                {
                    return true;
                }

                int prevSymbolRight = api.MICRGetSymbolRight(currentSymbol - 1);
                int thisSymbolLeft = api.MICRGetSymbolLeft(currentSymbol);
                int gapWidth = thisSymbolLeft - prevSymbolRight;

                // TODO: Figure out what the optimal gap is and/or make this configurable
                return gapWidth > 2 * GetAverageCharacterWidth(currentLine);
            }
            return false;
        }

        // Get the average symbol width of the specified line
        // (calculates the average width for every MICR line on the first call)
        float GetAverageCharacterWidth(int lineNum)
        {
            // Build a dictionary of line-to-avg confidence so that the symbols only need to be iterated once
            if (averageCharacterWidthByLine == null)
            {
                Dictionary<int, (int, float)> totalCharWidths = new();
                for (int symbolNum = 1; symbolNum <= symbolsCount; symbolNum++)
                {
                    int line = api.MICRGetSymbolLine(symbolNum);
                    int left = api.MICRGetSymbolLeft(symbolNum);
                    int right = api.MICRGetSymbolRight(symbolNum);
                    (int count, float total) = totalCharWidths.GetOrAdd(line, _ => (0, 0));
                    count++;
                    total += right - left;
                    totalCharWidths[line] = (count, total);
                }

                averageCharacterWidthByLine = new();
                foreach (var kv in totalCharWidths)
                {
                    int line = kv.Key;
                    (int count, float total) = kv.Value;
                    if (count == 0)
                    {
                        averageCharacterWidthByLine[line] = 0;
                    }
                    else
                    {
                        averageCharacterWidthByLine[line] = total / count;
                    }
                }
            }

            if (averageCharacterWidthByLine.TryGetValue(lineNum, out var characterWidth))
            {
                return characterWidth;
            }

            return 0;
        }
    }
}
