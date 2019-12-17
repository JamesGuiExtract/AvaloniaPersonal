using Extract.Licensing;
using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using technology.tabula;
using UCLID_AFCORELib;
using UCLID_COMUTILSLib;
using UCLID_RASTERANDOCRMGMTLib;
using UCLID_SSOCRLib;
using OCRParam = Extract.Utilities.Union<(int key, int value), (int key, double value), (string key, int value), (string key, double value), (string key, string value)>;

namespace Extract.AttributeFinder.Tabula
{
    /// <summary>
    /// Utilities methods for finding tables using Tabula
    /// </summary>
    [CLSCompliant(false)]
    public static class TabulaUtils
    {
        #region Fields

        static readonly ThreadLocal<ScansoftOCR> _ocrEngine = new ThreadLocal<ScansoftOCR>(() =>
        {
            var engine = new ScansoftOCRClass();
            engine.InitPrivateLicense(LicenseUtilities.GetMapLabelValue(new MapLabel()));
            return engine;
        });

        #endregion Fields

        #region Factory Methods

        /// <summary>
        /// Create <see cref="TabulaUtility{TTable}"/> instance that outputs formatted <see cref="SpatialString"/> tables,
        /// using <see cref="TabulaTableFinderV1"/> for table finding
        /// </summary>
        /// <param name="tableProcessor">The <see cref="TablesToSpatialString"/> instance to use for formatting the tables</param>
        public static TabulaUtility<SpatialString> CreateTabulaUtility(TablesToSpatialString tableProcessor)
        {
            return CreateTabulaUtility(new TabulaTableFinderV1(), tableProcessor);
        }

        /// <summary>
        /// Create <see cref="TabulaUtility{TTable}"/> instance that outputs formatted <see cref="SpatialString"/> tables,
        /// using specified <see cref="ITabulaTableFinder"/> for table finding
        /// </summary>
        /// <param name="tableFinder">The <see cref="ITabulaTableFinder"/> implementation to find the tables</param>
        /// <param name="tableProcessor">The <see cref="TablesToSpatialString"/> instance to use for formatting the tables</param>
        public static TabulaUtility<SpatialString> CreateTabulaUtility(ITabulaTableFinder tableFinder, TablesToSpatialString tableProcessor)
        {
            return new TabulaUtility<SpatialString>(tableFinder, tableProcessor);
        }
 
        /// <summary>
        /// Create instance for finding spatial string matrices using <see cref="TabulaTableFinderV1"/> for table finding
        /// </summary>
        public static TabulaUtility<SpatialString[][]> CreateTabulaUtility()
        {
            return CreateTabulaUtility(new TabulaTableFinderV1());
        }

        /// <summary>
        /// Create instance for finding spatial string matrices using specified <see cref="ITabulaTableFinder"/> for table finding
        /// </summary>
        /// <param name="tableFinder">The <see cref="ITabulaTableFinder"/> implementation to find the tables</param>
        public static TabulaUtility<SpatialString[][]> CreateTabulaUtility(ITabulaTableFinder tableFinder)
        {
            return new TabulaUtility<SpatialString[][]>(tableFinder, new TablesToSpatialStringCells());
        }

        #endregion Factory Methods

        #region Public Methods

        public static IEnumerable<T> ToEnumerable<T>(this java.util.List list)
        {
            return list.iterator().ToEnumerable<T>();
        }

        public static IEnumerable<T> ToEnumerable<T>(this java.util.Iterator it)
        {
            while (it.hasNext())
            {
                yield return (T)it.next();
            }
        }

        /// <summary>
        /// Combines pages of tables as spatial strings so that there is one "Page" attribute per outer sequence
        /// </summary>
        /// <param name="pagesOfTables">An enumeration of enumeration of tables</param>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static IUnknownVector GetTablesAsOneAttributePerPage(this IEnumerable<IEnumerable<SpatialString>> pagesOfTables)
        {
            try
            {
                var pageAttributes =
                    pagesOfTables
                    .Select(tablesOnPage =>
                    {
                        var attr = new AttributeClass
                        {
                            Name = SpecialAttributeNames.Page,
                        };
                        var tableVector = tablesOnPage.ToIUnknownVector();
                        if (tableVector.Size() > 0)
                        {
                            attr.Value.CreateFromSpatialStrings(tableVector, false);
                        }
                        return attr;
                    })
                    .ToIUnknownVector();
                pageAttributes.ReportMemoryUsage();
                return pageAttributes;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI49520");
            }
        }

        /// <summary>
        /// Combines tables into a single spatial string
        /// </summary>
        /// <param name="pagesOfTables">An enumeration of tables</param>
        /// <returns>
        /// A <see cref="SpatialString"/>containing zero or more tables.
        /// </returns>
        public static SpatialString GetTablesAsSpatialString(this IEnumerable<SpatialString> tables)
        {
            try
            {
                var result = new SpatialStringClass();
                var tableVector = tables.Where(table => table.HasSpatialInfo()).ToIUnknownVector();
                if (tableVector.Size() > 0)
                {
                    result.CreateFromSpatialStrings(tableVector, false);
                }
                result.ReportMemoryUsage();

                return result;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI49507");
            }
        }

        /// <summary>
        /// Create a text-based PDF file from a PDF or image
        /// </summary>
        /// <param name="inputFile">The PDF or image to convert</param>
        /// <param name="pdfFile">The path to write the text-based PDF to</param>
        public static void CreateTextPdf(string inputFile, string pdfFile, IHasOCRParameters hasOCRParameters = null)
        {
            try
            {
                var ocrParams = new List<OCRParam>
                {
                    // Turn off external fonts to make the file size smaller
                    new OCRParam(("Converters.Text.PDF.AdditionalFonts", 0)),
                    // Use original DPI for pictures
                    new OCRParam(("Converters.Text.PDF.Pictures", 1)),

                    new OCRParam(((int)EOCRParameter.kTradeoff, (int)EOcrTradeOff.kAccurate)),
                    new OCRParam(("Kernel.Img.Max.Pix.X", 32000)),
                    new OCRParam(("Kernel.Img.Max.Pix.Y", 32000)),
                    new OCRParam(("Kernel.OcrMgr.PreferAccurateEngine", 1)),
                }
                .ToOCRParameters();

                if (hasOCRParameters?.OCRParameters is VariantVector specifiedParams)
                {
                    ((VariantVector)ocrParams).Append(specifiedParams);
                }

                _ocrEngine.Value.CreateOutputImage(inputFile, "PDF", pdfFile, ocrParams);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI49538");
            }
        }

        #endregion Public Methods

        #region Internal Methods

        // Sort and flatten list that sometimes has TextChunk and sometimes has TextElement elements
        internal static List<TextElement> GetTextElements(java.util.List textElementsOrChunks)
        {
            Utils.sort(textElementsOrChunks, Rectangle.ILL_DEFINED_ORDER);
            List<TextElement> textElements = new List<TextElement>();
            foreach (var textElementOrChunk in textElementsOrChunks.ToEnumerable<HasText>())
            {
                if (textElementOrChunk is TextElement character)
                {
                    textElements.Add(character);
                }
                else if (textElementOrChunk is TextChunk word)
                {
                    foreach(var textElement in word.getTextElements().ToEnumerable<TextElement>())
                    {
                        textElements.Add(textElement);
                    }
                    textElements.Add(new TextElement(0, 0, 0, 0, null, 0, " ", 0));
                }
            }

            return textElements;
        }

        internal static List<LetterStruct> GetLettersFromTextElements(List<TextElement> textElements, ushort pageNumber, float pdfToImageWidth, float pdfToImageHeight)
        {
            var newLine = GetNonSpatialLetters("\r\n");
            List<LetterStruct> letters = new List<LetterStruct>();
            int lastSpatialIndex = -1;
            foreach(var textElement in textElements)
            {
                LetterStruct letter = GetLetter(textElement, pageNumber, pdfToImageWidth, pdfToImageHeight);

                // Check for place where there should be \r\n instead of space chars
                if (letter.Spatial)
                {
                    if (lastSpatialIndex >= 0 && letters[lastSpatialIndex].IsNewLineBetween(ref letter))
                    {
                        int charsToReplace = letters.Count - lastSpatialIndex;
                        if (charsToReplace > 0)
                        {
                            letters.RemoveRange(lastSpatialIndex, charsToReplace);
                        }
                        letters.AddRange(newLine);
                    }

                    lastSpatialIndex = letters.Count;
                }

                letters.Add(letter);
            }

            // Trim trailing spaces
            while (letters.Any() && letters.Last().Guess1 == ' ')
            {
                letters.RemoveAt(letters.Count - 1);
            }

            return letters;
        }


        internal static LetterStruct GetLetter(TextElement element, ushort pageNumber, float pdfToImageWidth, float pdfToImageHeight)
        {
            string text = element.getText();
            var bounds = element.getBounds();
            var isSpatial = bounds.width > 0 && bounds.height > 0;
            return new LetterStruct(
                code: text[0],
                top: (uint)(bounds.y * pdfToImageHeight),
                right: (uint)((bounds.x + bounds.width) * pdfToImageWidth),
                bottom: (uint)((bounds.y + bounds.height) * pdfToImageHeight),
                left: (uint)(bounds.x * pdfToImageWidth),
                pageNumber: isSpatial ? pageNumber : (ushort)0,
                endOfParagraph: false,
                endOfZone: false,
                spatial: isSpatial,
                fontSize: (byte)element.getFontSize(),
                characterConfidence: 1,
                font: 0);
        }

        internal static List<LetterStruct> GetNonSpatialLetters(string letters)
        {
            return letters.Select(letter => new LetterStruct(letter)).ToList();
        }

        #endregion Internal Methods
    }
}
