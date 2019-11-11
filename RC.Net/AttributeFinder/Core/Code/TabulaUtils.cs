using Extract.Imaging.Utilities;
using Extract.Licensing;
using Extract.Utilities;
using javax.imageio.spi;
using org.apache.pdfbox.pdmodel;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;
using technology.tabula;
using technology.tabula.detectors;
using technology.tabula.extractors;
using UCLID_AFCORELib;
using UCLID_COMUTILSLib;
using UCLID_IMAGEUTILSLib;
using UCLID_RASTERANDOCRMGMTLib;
using UCLID_SSOCRLib;
using OCRParam = Extract.Utilities.Union<(int key, int value), (int key, double value), (string key, int value), (string key, double value), (string key, string value)>;

namespace Extract.AttributeFinder
{
    /// <summary>
    /// Utilities for finding tables using Tabula
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

        static readonly ThreadLocal<ImageUtils> _imageUtils = new ThreadLocal<ImageUtils>(() => new ImageUtilsClass());
        static readonly ThreadLocal<DefaultTabulaTableFinder> _defaultTableFinder = new ThreadLocal<DefaultTabulaTableFinder>(() => new DefaultTabulaTableFinder());

        #endregion Fields

        #region Constructor

        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static TabulaUtils()
        {
            // The maven assembly plugin (used in the build process) doesn't properly register these two readers
            IIORegistry registry = IIORegistry.getDefaultInstance();
            registry.registerServiceProvider(new com.github.jaiimageio.jpeg2000.impl.J2KImageReaderSpi());
            registry.registerServiceProvider(new org.apache.pdfbox.jbig2.JBIG2ImageReaderSpi());

            UnlockLeadtools.UnlockLeadToolsSupport();
        }

        #endregion Constructor

        #region Public Methods

        /// <summary>
        /// Generic method to do boring stuff and delegate table finding/processing for each page to the supplied classes
        /// </summary>
        /// <typeparam name="TResult">The result of the table processor</typeparam>
        /// <param name="inputFile">The PDF or image to search for tables</param>
        /// <param name="tableProcessor">The <see cref="ITabulaTableProcessor{TResult}"/> used for processing each table</param>
        /// <param name="pageNumbers">Optional set of pages to search (pass null for all pages)</param>
        /// <param name="tableFinder">Optional <see cref="ITabulaTableFinder"/> implementation to find the tables</param>
        /// <param name="pdfFile">Optional path to load or write the intermediate, text-based PDF file to. If null then a temp file will be used.</param>
        /// <returns>A nested collection of tables where the outer layer has one entry per page searched</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public static IEnumerable<IEnumerable<TResult>> GetTablesOnEveryPage<TResult>(string inputFile, ITabulaTableProcessor<TResult> tableProcessor,
            IEnumerable<int> pageNumbers = null, ITabulaTableFinder tableFinder = null, string pdfFile = null)
        {
            TemporaryFile tmpFile = null;
            try
            {
                if (pdfFile == null)
                {
                    tmpFile = new TemporaryFile(".pdf", true);
                    pdfFile = tmpFile.FileName;
                }

                PDDocument pdDoc = null;
                try
                {
                    LongToObjectMap spatialPageInfos = GetSpatialPageInfos(inputFile);
                    var numberOfInfos = spatialPageInfos.Size;

                    // Create text-based PDF file for input to Tabula unless one has already been supplied
                    if (!File.Exists(pdfFile) || new FileInfo(pdfFile).Length == 0)
                    {
                        var ocrParams = new List<OCRParam>
                        {
                            // Turn off external fonts to make the file size smaller
                            new OCRParam(("Converters.Text.PDF.AdditionalFonts", 0)),
                            // Use original DPI for pictures
                            new OCRParam(("Converters.Text.PDF.Pictures", 1)),
                        }
                        .ToOCRParameters();

                        _ocrEngine.Value.CreateOutputImage(inputFile, "PDF", pdfFile, ocrParams);
                    }

                    pdDoc = PDDocument.load(new java.io.File(pdfFile));
                    var numberOfPages = pdDoc.getNumberOfPages();

                    ExtractException.Assert("ELI49503", "Page count discrepancy", numberOfInfos == numberOfPages,
                        "Page count from GetSpatialPageInfos", numberOfInfos,
                        "Page count from getNumberOfPages", numberOfPages);

                    IEnumerable<int> pagesToSearch = Enumerable.Range(1, numberOfPages);
                    if (pageNumbers != null)
                    {
                        pagesToSearch = pagesToSearch.Intersect(pageNumbers);
                    }
                    var pagesOfTables = pagesToSearch
                        .Select(pageNumber =>
                        {
                            TabulaTablesForPage tables;
                            try
                            {
                                tables = GetTablesFromPage(pdDoc, pageNumber, spatialPageInfos, inputFile, tableFinder);
                            }
                            catch (Exception ex)
                            {
                                var uex = new ExtractException("ELI49508", "Application trace: Exception encountered extracting tables", ex);
                                uex.AddDebugData("Input file", inputFile);
                                uex.AddDebugData("Page number", pageNumber);
                                uex.Log();
                                return Enumerable.Empty<TResult>();
                            }

                            try
                            {
                                return tableProcessor.ProcessTables(tables, spatialPageInfos);
                            }
                            catch (Exception ex)
                            {
                                var uex = new ExtractException("ELI49508", "Application trace: Exception encountered processing tables", ex);
                                uex.AddDebugData("Input file", inputFile);
                                uex.AddDebugData("Page number", pageNumber);
                                uex.Log();
                                return Enumerable.Empty<TResult>();
                            }
                        })
                        .ToList();

                    return pagesOfTables;
                }
                finally
                {
                    try
                    {
                        pdDoc?.close();
                    }
                    catch { }
                }
            }
            finally
            {
                tmpFile?.Dispose();
            }
        }


        /// <summary>
        /// Find tables and make one spatial string per page with each table on the page delimited by keywords
        /// </summary>
        /// <param name="inputFile">A PDF or image file to search for tables</param>
        /// <param name="byRow">If <c>true</c> then tables will be divided by row first then by column. If <c>false</c> then by column then row</param>
        /// <param name="tableFinder">Optional implementation of the ITabulaTableFinder interface. Default (when null) recreates the default Tabula cmdline app behavior</param>
        /// <param name="pdfFile">Optional path to load or write the intermediate, text-based PDF file to. If null then a temp file will be used.</param>
        /// <returns>
        /// A vector of "Page" <see cref="UCLID_AFCORELib.Attribute"/>s, one per page.
        /// Each page's value will contain zero or more tables, enclosed within "START-TABLE" and "END-TABLE" strings, on their own lines.
        /// Each table will be divided into columns or rows, enclosed within "START-COLUMN/ROW" and "END-COLUMN/ROW" strings, on their own lines.
        /// </returns>
        public static IUnknownVector GetTablesAsOneAttributePerPage(string inputFile, bool byRow, ITabulaTableFinder tableFinder = null,
            string pdfFile = null)
        {
            try
            {
                var pageAttributes = GetTablesOnEveryPage(inputFile, new TablesToSpatialString(inputFile, byRow, "\t"), null, tableFinder, pdfFile)
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
        /// Find tables and make one spatial string per cell of the table
        /// </summary>
        /// <param name="inputFile">A PDF or image file to search for tables</param>
        /// <param name="tableFinder">Optional implementation of the ITabulaTableFinder interface.
        /// if null then the default algorithm will be used to recreate the default Tabula cmdline app behavior</param>
        /// <param name="pdfFile">Optional path to load or write the intermediate, text-based PDF file to.
        /// If null then a temp file will be used.</param>
        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        public static List<SpatialString[][]> GetTableCellsAsSpatialStrings(string inputFile, IEnumerable<int> pageNumbers = null,
            ITabulaTableFinder tableFinder = null, string pdfFile = null)
        {
            try
            {
                return GetTablesOnEveryPage(inputFile, new TablesToSpatialStringCells(), pageNumbers, tableFinder, pdfFile)
                    .SelectMany(tablesOnPage => tablesOnPage.ToList())
                    .ToList();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI49506");
            }
        }

        /// <summary>
        /// Find tables on a page and make a spatial string with each table on the page delimited by keywords
        /// </summary>
        /// <param name="inputFile">A PDF or image file to search for tables</param>
        /// <param name="pageNumber">The page number to search on</param>
        /// <param name="byRow">If <c>true</c> then tables will be divided by row first then by column.
        /// If <c>false</c> then by column first then by row</param>
        /// <param name="secondarySeparator">Value to put between the column values in a row
        /// (or row values in column if <see paramref="byRow"/> is <c>false</c>)</param>
        /// <param name="tableFinder">Optional implementation of the ITabulaTableFinder interface.
        /// if null then the default algorithm will be used to recreate the default Tabula cmdline app behavior</param>
        /// <returns>
        /// A <see cref="SpatialString"/> containing zero or more tables, enclosed within "START-TABLE" and "END-TABLE" strings, on their own lines.
        /// Each table will be divided into rows or columns, enclosed within "START-ROW" and "END-ROW" or "START-COLUMN" and "END-COLUMN" strings, on their own lines.
        /// </returns>
        public static SpatialString GetTablesAsSpatialString(string inputFile, int pageNumber, bool byRow, ITabulaTableFinder tableFinder = null,
            string pdfFile = null)
        {
            try
            {
                var result = new SpatialStringClass();
                var tables = GetTablesOnEveryPage(inputFile, new TablesToSpatialString(inputFile, byRow, "\t"),
                    new [] { pageNumber }, tableFinder, pdfFile).First();
                var tableVector = tables.ToIUnknownVector();
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

        #endregion Public Methods

        #region Private Methods

        // Find tables on a page
        private static TabulaTablesForPage GetTablesFromPage(PDDocument doc, int pageNumber, LongToObjectMap spatialPageInfos,
            string sourceDocName, ITabulaTableFinder tableFinder = null)
        {
            ExtractException.Assert("ELI49504", "SpatialPageInfo is missing", spatialPageInfos.Contains(pageNumber), "Page number", pageNumber);

            var pageInfo = (SpatialPageInfo)spatialPageInfos.GetValue(pageNumber);
            var width = pageInfo.Width;
            var height = pageInfo.Height;
            var orientation = pageInfo.Orientation;

            // Calculate the pdf-to-image scale factors
            var pageArea = GetPage(doc, pageNumber);
            if (orientation == EOrientation.kRotRight || orientation == EOrientation.kRotLeft)
            {
                UtilityMethods.Swap(ref width, ref height);
            }
            float pdfToImageWidth = width / pageArea.width;
            float pdfToImageHeight = height / pageArea.height;

            tableFinder = tableFinder ?? _defaultTableFinder.Value;
            var tables = tableFinder.GetTablesFromPageArea(pageArea);
            return new TabulaTablesForPage
            (
                sourceDocName: sourceDocName,
                tables: tables,
                pageNumber: pageNumber,
                pdfToImageWidth: pdfToImageWidth,
                pdfToImageHeight: pdfToImageHeight
            );
        }

        // Extract page from PDF
        private static Page GetPage(PDDocument doc, int pageNumber)
        {
            var objectExtractor = new ObjectExtractor(doc);
            var pageArea = objectExtractor.extract(pageNumber);
            return pageArea;
        }

        // Load page info or create with OCR
        private static LongToObjectMap GetSpatialPageInfos(string imagePath)
        {
            // Get at least the dimensions for all pages (will not have orientation info if USS file is missing)
            var pageInfos = _imageUtils.Value.GetSpatialPageInfos(imagePath)
                .ToIEnumerable<SpatialPageInfo>()
                .Select((info, i) => new KeyValuePair<int, SpatialPageInfo>(i + 1, info));

            // Check to be sure that orientation info is from OCR
            string sourceUSSFile = imagePath + ".uss";
            if (!File.Exists(sourceUSSFile))
            {
                // TODO: This is inefficient. Should add a method that just gets page info without doing OCR
                var sourceUSS = _ocrEngine.Value.RecognizeTextInImage(imagePath, 1, -1, EFilterCharacters.kNoFilter, null, EOcrTradeOff.kBalanced, true, null, null);
                sourceUSS.ReportMemoryUsage();

                // Add this info to the end of the enumeration so that it overwrites the info from the image when added to the map
                pageInfos = pageInfos.Concat(sourceUSS.EnumerateSpatialPageInfos());
            }

            return pageInfos.ToLongToObjectMap();
        }

        // Flatten list that sometimes has TextChunk and sometimes has TextElement elements
        private static List<TextElement> GetTextElements(java.util.List textElementsOrChunks)
        {
            List<TextElement> textElements = new List<TextElement>();

            Utils.sort(textElementsOrChunks, Rectangle.ILL_DEFINED_ORDER);
            var textIt = textElementsOrChunks.iterator();
            while (textIt.hasNext())
            {
                object item = textIt.next();
                if (item is TextElement character)
                {
                    textElements.Add(character);
                }
                else if (item is TextChunk word)
                {
                    var textChunkTextElements = word.getTextElements().iterator();
                    while (textChunkTextElements.hasNext())
                    {
                        textElements.Add((TextElement)textChunkTextElements.next());
                    }
                    textElements.Add(new TextElement(0, 0, 0, 0, null, 0, " ", 0));
                }
            }
            return textElements;
        }

        // Create Letters from TextElements
        private static List<LetterStruct> GetLettersFromTextElements(List<TextElement> textElements, ushort pageNumber, float pdfToImageWidth, float pdfToImageHeight, bool addNewlines)
        {
            var newLine = GetNonSpatialLetters("\r\n");
            List<LetterStruct> letters = new List<LetterStruct>();
            LetterStruct lastSpatialLetter = default(LetterStruct);
            for (int i = 0, size = textElements.Count; i < size; i++)
            {
                LetterStruct letter = GetLetter(textElements[i], pageNumber, pdfToImageWidth, pdfToImageHeight);
                bool isLastElement = i == size - 1;

                if (isLastElement && letter.Guess1 == ' ')
                {
                    break;
                }

                if (addNewlines)
                {
                    if (letter.Guess1 == ' ')
                    {
                        if (i > 0 && !isLastElement && lastSpatialLetter.Spatial)
                        {
                            LetterStruct nextLetter = GetLetter(textElements[i + 1], pageNumber, pdfToImageWidth, pdfToImageHeight);

                            if (lastSpatialLetter.IsNewLineBetween(ref nextLetter))
                            {
                                letters.AddRange(newLine);
                                letter = nextLetter;
                                i++;
                            }
                        }
                    }
                    else if (i > 0 && lastSpatialLetter.IsNewLineBetween(ref letter))
                    {
                        letters.AddRange(newLine);
                    }
                }

                letters.Add(letter);

                if (letter.Spatial)
                {
                    lastSpatialLetter = letter;
                }
            }

            return letters;
        }

        private static LetterStruct GetLetter(TextElement element, ushort pageNumber, float pdfToImageWidth, float pdfToImageHeight)
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

        private static List<LetterStruct> GetNonSpatialLetters(string letters)
        {
            return letters.Select(letter => new LetterStruct(letter)).ToList();
        }

        #endregion Private Methods

        #region Private Classes

        /// <summary>
        /// Recreate algorithm of Tabula.IKVM.exe
        /// </summary>
        private class DefaultTabulaTableFinder : ITabulaTableFinder
        {
            readonly NurminenDetectionAlgorithm _detector = new NurminenDetectionAlgorithm();
            readonly SpreadsheetExtractionAlgorithm _spreadsheetExtractor = new SpreadsheetExtractionAlgorithm();
            readonly BasicExtractionAlgorithm _basicExtractor = new BasicExtractionAlgorithm();

            public IEnumerable<Table> GetTablesFromPageArea(Page pageArea)
            {
                // TODO: It seems like this test should be done once per guess but the cmdline app does it once/page...
                ExtractionAlgorithm extractionAlgorithm = _spreadsheetExtractor;
                try
                {
                    if (!_spreadsheetExtractor.isTabular(pageArea))
                    {
                        extractionAlgorithm = _basicExtractor;
                    }
                }
                catch { } // TODO: Fix java code (It looks like there would be an exception when the spreadsheet alg finds tables and the basic alg finds none)

                java.util.Iterator/*<Rectangle>*/ guesses = _detector.detect(pageArea).iterator();
                while (guesses.hasNext())
                {
                    Page guess = pageArea.getArea((Rectangle)guesses.next());
                    java.util.Iterator/*<Table>*/ tables = extractionAlgorithm.extract(guess).iterator();
                    while (tables.hasNext())
                    {
                        yield return (Table)tables.next();
                    }
                }
            }
        }

        private class TablesToLetters : ITabulaTableProcessor<List<LetterStruct>[][]>
        {
            #region ITabulaTableProcessor

            public IEnumerable<List<LetterStruct>[][]> ProcessTables(TabulaTablesForPage page, LongToObjectMap spatialPageInfos)
            {
                var result = new List<List<LetterStruct>[][]>();
                foreach (var table in page.Tables)
                {
                    var rowCount = table.getRowCount();
                    if (rowCount == 0)
                    {
                        continue;
                    }
                    var tableMatrix = new List<LetterStruct>[rowCount][];
                    var colCount = table.getColCount();

                    java.util.Iterator/*<List<RectangularTextContainer>>*/ byRows = table.getRows().iterator();
                    int rowIdx = 0;
                    while (byRows.hasNext())
                    {
                        tableMatrix[rowIdx] = new List<LetterStruct>[colCount];
                        int colIdx = 0;
                        java.util.Iterator/*<RectangularTextContainer>*/ byCols = ((java.util.List)byRows.next()).iterator();
                        while (byCols.hasNext())
                        {
                            var cell = (RectangularTextContainer)byCols.next();
                            java.util.List textElementsOrChunks = cell.getTextElements();
                            List<TextElement> text = GetTextElements(textElementsOrChunks);
                            tableMatrix[rowIdx][colIdx] = GetLettersFromTextElements(text, (ushort)page.PageNumber, page.PdfToImageWidth, page.PdfToImageHeight, false);
                            colIdx++;
                        }
                        rowIdx++;
                    }
                    result.Add(tableMatrix);
                }
                return result;
            }

            #endregion ITabulaTableProcessor
        }

        private class TablesToSpatialStringCells : ITabulaTableProcessor<SpatialString[][]>
        {
            #region ITabulaTableProcessor

            IEnumerable<SpatialString[][]> ITabulaTableProcessor<SpatialString[][]>.ProcessTables(TabulaTablesForPage page, LongToObjectMap spatialPageInfos)
            {
                var tables = new TablesToLetters().ProcessTables(page, spatialPageInfos)
                    .Select(table =>
                        table.Select(row =>
                            row.Select(column =>
                                SpatialStringMethods.CreateFromLetters(column, page.SourceDocName, spatialPageInfos))
                                .ToArray()
                            ).ToArray()
                        ).ToList();
                return tables;
            }

            #endregion ITabulaTableProcessor
        }

        private class TablesToSpatialString : ITabulaTableProcessor<SpatialString>
        {
            readonly string _sourceDocName;
            readonly bool _byRow;
            readonly List<LetterStruct> _newLine = GetNonSpatialLetters("\r\n");
            readonly List<LetterStruct> _tablePrefix = GetNonSpatialLetters("START-TABLE");
            readonly List<LetterStruct> _tableSuffix = GetNonSpatialLetters("END-TABLE");
            readonly List<LetterStruct> _colSeparator;
#pragma warning disable IDE0044 // Add readonly modifier
            List<LetterStruct> _rowPrefix = GetNonSpatialLetters("START-ROW");
            List<LetterStruct> _rowSuffix = GetNonSpatialLetters("END-ROW");
            List<LetterStruct> _colPrefix = GetNonSpatialLetters("START-COLUMN");
            List<LetterStruct> _colSuffix = GetNonSpatialLetters("END-COLUMN");
#pragma warning restore IDE0044 // Add readonly modifier

            #region Constructors

            public TablesToSpatialString(string sourceDocName, bool byRow, string secondarySeparator)
            {
                _sourceDocName = sourceDocName;
                _byRow = byRow;
                if (!_byRow)
                {
                    UtilityMethods.Swap(ref _rowPrefix, ref _colPrefix);
                    UtilityMethods.Swap(ref _rowSuffix, ref _colSuffix);
                }
                _colSeparator = GetNonSpatialLetters(secondarySeparator);
            }

            #endregion Constructors

            #region ITabulaTableProcessor

            public IEnumerable<SpatialString> ProcessTables(TabulaTablesForPage page, LongToObjectMap spatialPageInfos)
            {
                try
                {
                    IEnumerable<List<LetterStruct>[][]> tables = new TablesToLetters().ProcessTables(page, spatialPageInfos);
                    return tables.Select(table => ProcessTable(table, spatialPageInfos));
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI49505");
                }
            }

            #endregion ITabulaTableProcessor

            #region Private Methods

            private SpatialString ProcessTable(List<LetterStruct>[][] table, LongToObjectMap spatialPageInfos)
            {
                List<LetterStruct> letters = new List<LetterStruct>();
                var rowCount = table.Length;
                var colCount = table[0].Length;
                if (!_byRow)
                {
                    UtilityMethods.Swap(ref rowCount, ref colCount);
                }

                if (letters.Any())
                {
                    letters.AddRange(_newLine);
                }
                letters.AddRange(_tablePrefix);
                letters.AddRange(_newLine);

                for (int rowIdx = 0; rowIdx < rowCount; rowIdx++)
                {
                    letters.AddRange(_rowPrefix);
                    letters.AddRange(_newLine);
                    for (int colIdx = 0; colIdx < colCount; colIdx++)
                    {
                        if (colIdx > 0)
                        {
                            letters.AddRange(_colSeparator);
                        }
                        if (_byRow)
                        {
                            letters.AddRange(table[rowIdx][colIdx]);
                        }
                        else
                        {
                            letters.AddRange(table[colIdx][rowIdx]);
                        }
                    }
                    letters.AddRange(_newLine);
                    letters.AddRange(_rowSuffix);
                    letters.AddRange(_newLine);
                }
                letters.AddRange(_tableSuffix);
                return SpatialStringMethods.CreateFromLetters(letters, _sourceDocName, spatialPageInfos);
            }

            #endregion Private Methods
        }

        #endregion Private Classes
    }
}
