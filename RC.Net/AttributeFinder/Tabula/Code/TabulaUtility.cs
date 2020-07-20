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
using UCLID_COMUTILSLib;
using UCLID_IMAGEUTILSLib;
using UCLID_RASTERANDOCRMGMTLib;
using UCLID_SSOCRLib;


namespace Extract.AttributeFinder.Tabula
{
    [CLSCompliant(false)]
    public sealed class TabulaUtility<TTable> : IDisposable
    {
        #region Fields

        readonly ThreadLocal<ImageUtils> _imageUtils = new ThreadLocal<ImageUtils>(() => new ImageUtilsClass());

        readonly ThreadLocal<ScansoftOCR> _ocrEngine = new ThreadLocal<ScansoftOCR>(() =>
        {
            var engine = new ScansoftOCRClass();
            engine.InitPrivateLicense(LicenseUtilities.GetMapLabelValue(new MapLabel()));
            return engine;
        });

        readonly ITabulaTableFinder _tableFinder;
        readonly ITabulaTableProcessor<TTable> _tableProcessor;

        #endregion Fields

        #region Constructors

        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static TabulaUtility()
        {
            // The maven assembly plugin (used in the build process) doesn't properly register these two readers
            IIORegistry registry = IIORegistry.getDefaultInstance();
            registry.registerServiceProvider(new com.github.jaiimageio.jpeg2000.impl.J2KImageReaderSpi());
            registry.registerServiceProvider(new org.apache.pdfbox.jbig2.JBIG2ImageReaderSpi());

            UnlockLeadtools.UnlockLeadToolsSupport();
        }

        /// <summary>
        /// Create instance for finding <see cref="TTable"/>s using specified <see cref="ITabulaTableFinder"/> for table finding
        /// </summary>
        /// <param name="tableFinder">The <see cref="ITabulaTableFinder"/> implementation to find the tables</param>
        /// <param name="tableProcessor">The <see cref="ITabulaTableProcessor{TResult}"/> used for processing each table</param>
        public TabulaUtility(ITabulaTableFinder tableFinder, ITabulaTableProcessor<TTable> tableProcessor)
        {
            _tableFinder = tableFinder;
            _tableProcessor = tableProcessor;
        }

        #endregion Constructors

        #region Public Methods

        /// <summary>
        /// Find tables for each page
        /// </summary>
        /// <param name="inputFile">The PDF or image to search for tables</param>
        /// <param name="spatialInfoForPages">Optional page dimension and rotation information.
        /// If null then this information will be loaded from disc or created with OCR</param>
        /// <param name="pageNumbers">Optional set of pages to search (pass null for all pages)</param>
        /// <param name="pdfFile">Optional path to load or write the intermediate, text-based PDF file to.
        /// If null then a temp file will be used.</param>
        /// <returns>A nested collection of tables where the outer layer has one entry per page searched</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public IList<IList<TTable>> GetTablesOnSpecifiedPages(
            string inputFile,
            LongToObjectMap spatialInfoForPages = null,
            IEnumerable<int> pageNumbers = null,
            string pdfFile = null)
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
                    // Create text-based PDF file for input to Tabula unless one has already been supplied
                    if (!File.Exists(pdfFile) || new FileInfo(pdfFile).Length == 0)
                    {
                        TabulaUtils.CreateTextPdf(inputFile, pdfFile);
                    }
                    pdDoc = PDDocument.load(new java.io.File(pdfFile));
                    return GetTablesOnSpecifiedPages(pdDoc, inputFile, spatialInfoForPages, pageNumbers)
                        .ToList();
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
        /// Find tables for each page using the supplied <see cref="PDDocument"/>
        /// </summary>
        /// <param name="inputDocument">The <see cref="PDDocument"/> to be searched for tables</param>
        /// <param name="sourceDocName">The direct or indirect source of <see paramref="inputDocument"/>.
        /// Spatial page info will be loaded from this or its associated USS file</param>
        /// <param name="spatialInfoForPages">Optional page dimension and rotation information.
        /// If null then this information will be loaded from disc or created with OCR</param>
        /// <param name="pageNumbers">Optional set of pages to search (pass null for all pages)</param>
        /// <returns>A nested collection of tables where the outer layer has one entry per page searched</returns>
        /// <remarks>
        /// This method returns a lazily evaluated collection so the <see paramref="inputDocument"/> must not be
        /// closed before enumerating the results
        /// </remarks>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public IEnumerable<IList<TTable>> GetTablesOnSpecifiedPages(
            PDDocument inputDocument,
            string sourceDocName,
            LongToObjectMap spatialInfoForPages = null,
            IEnumerable<int> pageNumbers = null
            )
        {
            try
            {
                var pagesToSearch = pageNumbers?.ToList();
                IEnumerable<Page> pageIterator = GetPageIterator(inputDocument, pagesToSearch);
                LongToObjectMap spatialPageInfos = spatialInfoForPages ?? GetSpatialPageInfos(sourceDocName, pagesToSearch);

                var pagesOfTables = pageIterator.Select(page =>
                {
                    try
                    {
                        var tables = GetTablesFromPage(page, spatialPageInfos, sourceDocName);
                        return ProcessTables(tables, spatialPageInfos);
                    }
                    catch (Exception ex)
                    {
                        var uex = ex.AsExtract("ELI49585");
                        uex.AddDebugData("Input file", sourceDocName);
                        uex.AddDebugData("Page number", page.getPageNumber());
                        throw uex;
                    }
                });

                return pagesOfTables;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI49586");
            }
        }

        #endregion Public Methods

        #region Private Methods

        private TabulaTablesForPage GetTablesFromPage(Page pageArea, LongToObjectMap spatialPageInfos, string sourceDocName)
        {
            try
            {
                var pageNumber = pageArea.getPageNumber();
                ExtractException.Assert("ELI49504", "SpatialPageInfo is missing", spatialPageInfos.Contains(pageNumber), "Page number", pageNumber);

                var pageInfo = (SpatialPageInfo)spatialPageInfos.GetValue(pageNumber);
                var width = pageInfo.Width;
                var height = pageInfo.Height;
                var orientation = pageInfo.Orientation;

                if (orientation == EOrientation.kRotRight || orientation == EOrientation.kRotLeft)
                {
                    UtilityMethods.Swap(ref width, ref height);
                }
                float pdfToImageWidth = width / pageArea.width;
                float pdfToImageHeight = height / pageArea.height;

                var tables = _tableFinder.GetTablesFromPageArea(pageArea);
                return new TabulaTablesForPage
                (
                    sourceDocName: sourceDocName,
                    tables: tables,
                    pageNumber: pageNumber,
                    pdfToImageWidth: pdfToImageWidth,
                    pdfToImageHeight: pdfToImageHeight
                );
            }
            catch (Exception ex)
            {
                throw new ExtractException("ELI49508", "Exception extracting tables", ex);
            }
        }

        private List<TTable> ProcessTables(TabulaTablesForPage tables, LongToObjectMap spatialPageInfos)
        {
            try
            {
                return _tableProcessor.ProcessTables(tables, spatialPageInfos).ToList();
            }
            catch (Exception ex)
            {
                throw new ExtractException("ELI50090", "Exception processing tables", ex);
            }
        }

        private static IEnumerable<Page> GetPageIterator(PDDocument pdDoc, IEnumerable<int> pageNumbers)
        {
            bool searchAllPages = pageNumbers == null || !pageNumbers.Any();
            if (!searchAllPages)
            {
                ValidatePageNumbers(pdDoc, pageNumbers);
            }

            var objectExtractor = new ObjectExtractor(pdDoc);
            if (searchAllPages)
            {
                return objectExtractor.extract().ToEnumerable<Page>();
            }
            else
            {
                var javaPageNumbers = new java.util.ArrayList();
                foreach (var i in pageNumbers)
                {
                    javaPageNumbers.add(java.lang.Integer.valueOf(i));
                }
                return objectExtractor.extract(javaPageNumbers).ToEnumerable<Page>();
            }
        }

        private static void ValidatePageNumbers(PDDocument pdDoc, IEnumerable<int> pageNumbers)
        {
            var numberOfPages = pdDoc.getNumberOfPages();

            var specifiedPageNumbers = new HashSet<int>(pageNumbers);
            var availablePages = Enumerable.Range(1, numberOfPages);
            var intersect = availablePages.Intersect(specifiedPageNumbers).ToList();
            if (intersect.Count != specifiedPageNumbers.Count)
            {
                var missingPages = specifiedPageNumbers.Except(intersect);
                var uex = new ExtractException("ELI49537", "Page not found!");
                foreach (var page in missingPages)
                {
                    uex.AddDebugData("Missing page", page);
                }
                throw uex;
            }
        }

        private LongToObjectMap GetSpatialPageInfos(string imagePath, IEnumerable<int> pageNumbers)
        {
            if (pageNumbers == null || pageNumbers.Count() > 1)
            {
                return GetSpatialPageInfoForAllPages(imagePath);
            }
            else
            {
                return GetSpatialPageInfoForSinglePage(imagePath, pageNumbers.First());
            }
        }

        private LongToObjectMap GetSpatialPageInfoForSinglePage(string imagePath, int pageNumber)
        {
            string sourceUSSFile = imagePath + ".uss";

            // Check to be sure that orientation info is from OCR
            if (!File.Exists(sourceUSSFile))
            {
                // TODO: This is inefficient. Should add a method that just gets page info without doing OCR
                var sourceUSS = _ocrEngine.Value.RecognizeTextInImage(imagePath, pageNumber, pageNumber, EFilterCharacters.kNoFilter, null, EOcrTradeOff.kBalanced, true, null, null);
                sourceUSS.ReportMemoryUsage();

                if (sourceUSS.HasSpatialInfo() && sourceUSS.SpatialPageInfos.Contains(pageNumber))
                {
                    return sourceUSS.SpatialPageInfos;
                }
            }

            // Get at least the dimensions (will not have orientation info if USS file is missing)
            var pageInfo = _imageUtils.Value.GetSpatialPageInfo(imagePath, pageNumber);
            var pageInfoMap = new LongToObjectMapClass();
            pageInfoMap.Set(pageNumber, pageInfo);
            return pageInfoMap;
        }

        private LongToObjectMap GetSpatialPageInfoForAllPages(string imagePath)
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

        #endregion Private Methods

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        public void Dispose()
        {
            if (!disposedValue)
            {
                _imageUtils.Dispose();
                _ocrEngine.Dispose();

                disposedValue = true;
            }
        }
        #endregion
    }
}
