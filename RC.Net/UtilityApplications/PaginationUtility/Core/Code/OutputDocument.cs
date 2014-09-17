using Extract.Imaging;
using Extract.Utilities;
using Leadtools;
using Leadtools.Codecs;
using Leadtools.ImageProcessing;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace Extract.UtilityApplications.PaginationUtility
{
    /// <summary>
    /// Represents a potential output document as represented by a collection of
    /// <see cref="PageThumbnailControl"/>s.
    /// </summary>
    internal class OutputDocument
    {
        #region Fields

        /// <summary>
        /// The <see cref="PageThumbnailControl"/>s that represent the pages that are to comprise
        /// the document.
        /// </summary>
        List<PageThumbnailControl> _pageControls = new List<PageThumbnailControl>();

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="OutputDocument"/> class.
        /// </summary>
        /// <param name="fileName">The filename that the document is to be saved as.</param>
        public OutputDocument(string fileName)
        {
            try
            {
                FileName = fileName;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35548");
            }
        }

        #endregion Constructors

        #region Events

        /// <summary>
        /// Raised when the document is about to be output.
        /// </summary>
        public event EventHandler<CancelEventArgs> DocumentOutputting;

        /// <summary>
        /// Raised when the document is output.
        /// </summary>
        public event EventHandler<EventArgs> DocumentOutput;

        #endregion Events

        #region Properties

        /// <summary>
        /// Gets the <see cref="PageThumbnailControl"/>s that comprise the document to output.
        /// </summary>
        public ReadOnlyCollection<PageThumbnailControl> PageControls
        {
            get
            {
                try
                {
                    return _pageControls.AsReadOnly();
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI35549");
                }
            }
        }

        /// <summary>
        /// Gets or sets the filename that the document is to be saved as.
        /// </summary>
        /// <value>
        /// The filename that the document is to be saved as.
        /// </value>
        public string FileName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the document has changed compared to the input
        /// document for all the pages.
        /// </summary>
        /// <value><see langword="true"/> if in its original form; otherwise, <see langword="false"/>.
        /// </value>
        public bool InOriginalForm
        {
            get;
            set;
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Adds the specified <see paramref="pageControl"/> as the last page of the document.
        /// </summary>
        /// <param name="pageControl">The <see cref="PageThumbnailControl"/> representing the page
        /// to be added.</param>
        public void AddPage(PageThumbnailControl pageControl)
        {
            try
            {
                InsertPage(pageControl, _pageControls.Count + 1);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35550");
            }
        }

        /// <summary>
        /// Inserts the specified <see paramref="pageControl"/> as <see paramref="pageNumber"/> of
        /// the document.
        /// </summary>
        /// <param name="pageControl">The <see cref="PageThumbnailControl"/> representing the page
        /// to be inserted.</param>
        /// <param name="pageNumber">The page number the new page should be inserted at.</param>
        public void InsertPage(PageThumbnailControl pageControl, int pageNumber)
        {
            try
            {
                ExtractException.Assert("ELI35551", "Invalid page number",
                    pageNumber > 0 && pageNumber <= _pageControls.Count + 1,
                    "Document", FileName, "Page", pageNumber);

                InOriginalForm = false;

                if (pageNumber <= _pageControls.Count)
                {
                    _pageControls.Insert(pageNumber - 1, pageControl);
                }
                else
                {
                    _pageControls.Add(pageControl);
                }

                pageControl.Document = this;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35552");
            }
        }

        /// <summary>
        /// Removes the specified <see paramref="pageControl"/> from the document.
        /// </summary>
        /// <param name="pageControl">The <see cref="PageThumbnailControl"/> that is to be removed.</param>
        public void RemovePage(PageThumbnailControl pageControl)
        {
            try
            {
                ExtractException.Assert("ELI35553", "Null argument exception.", pageControl != null);

                InOriginalForm = false;

                _pageControls.Remove(pageControl);

                pageControl.Document = null;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35554");
            }
        }

        /// <summary>
        /// Outputs the document to the current <see cref="FileName"/>.
        /// </summary>
        /// <returns><see langword="true"/> if the document was output; otherwise
        /// <see langword="false"/>.</returns>
        public bool Output()
        {
            try
            {
                CancelEventArgs eventArgs = new CancelEventArgs();
                OnDocumentOutputting(eventArgs);
                if (eventArgs.Cancel)
                {
                    return false;
                }

                // Ensure the destination directory exists.
                string directory = Path.GetDirectoryName(FileName);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                if (InOriginalForm)
                {
                    // [DotNetRCAndUtils:972]
                    // If the extension of the file has changed, it is likely the user is intending
                    // to output the document in a different format.
                    string extension = Path.GetExtension(FileName);
                    string originalExtension =
                        Path.GetExtension(_pageControls[0].Page.OriginalDocumentName);

                    if (!extension.Equals(originalExtension, StringComparison.OrdinalIgnoreCase))
                    {
                        // If the extension has changed, set InOriginalForm to false to trigger the
                        // document to be manually output.
                        InOriginalForm = false;
                    }
                }

                if (InOriginalForm)
                {
                    // If the document has not been changed from its original form, it can simply be
                    // copied to _fileName rather than require it to be re-assembled.
                    File.Copy(_pageControls[0].Page.OriginalDocumentName, FileName);
                }
                else
                {
                    // Otherwise, generate a new document using the current PageControls as the
                    // document's pages.
                    using (ImageCodecs codecs = new ImageCodecs())
                    {
                        ImageWriter writer = null;
                        TemporaryFile temporaryFile = null;
                        var readers = new Dictionary<string, ImageReader>();

                        try
                        {
                            // Determine the specifications and format the output document will be.
                            ColorResolutionCommand conversionCommand;
                            int outputBitsPerPixel;
                            RasterImageFormat outputFormat;
                            InitializeOutputFormat(
                                out conversionCommand, out outputBitsPerPixel, out outputFormat);

                            foreach (Page page in PageControls.Select(pageControl => pageControl.Page))
                            {
                                // Get an image reader for the current page.
                                ImageReader reader;
                                if (!readers.TryGetValue(page.OriginalDocumentName, out reader))
                                {
                                    reader = codecs.CreateReader(page.OriginalDocumentName);
                                    readers[page.OriginalDocumentName] = reader;
                                }

                                using (RasterImage imagePage = reader.ReadPage(page.OriginalPageNumber))
                                {
                                    // On the first page, generate a writer using the same format as the
                                    // first page.
                                    if (writer == null)
                                    {
                                        if (!ImageMethods.IsTiff(outputFormat) &&
                                            Path.GetExtension(FileName)
                                                .StartsWith(".tif", StringComparison.OrdinalIgnoreCase))
                                        {
                                            // Ensure the default output format can handle color if the
                                            // output is to be in color.
                                            RasterImageFormat defaultFormat = (outputBitsPerPixel == 1)
                                                ? RasterImageFormat.CcittGroup4
                                                : RasterImageFormat.TifLzw;

                                            // If the image format is not tif, but the output filename
                                            // has a tif extension, change the format to a tif.
                                            writer = codecs.CreateWriter(FileName, defaultFormat, false);
                                        }
                                        else if (ImageMethods.IsPdf(outputFormat) ||
                                                 Path.GetExtension(FileName)
                                                     .Equals(".pdf", StringComparison.OrdinalIgnoreCase))
                                        {
                                            // If the output format is PDF, first output a tif to a
                                            // temporary file, then we will convert to a PDF at the end.
                                            // Theoretically, this shouldn't be necessary, but I was
                                            // not otherwise able able to get the LeadTools .Net API
                                            // to output an acceptable quality color PDF that was not
                                            // too large (PdfCompressor tended to produce unsightly
                                            // blocks on areas with light shading and I could not get
                                            // a reasonably sized output doc without using PdfCompressor.
                                            temporaryFile = new TemporaryFile(true);
                                            writer = codecs.CreateWriter(temporaryFile.FileName, outputFormat, false);
                                        }
                                        else
                                        {
                                            writer = codecs.CreateWriter(FileName, outputFormat, false);
                                        }
                                    }

                                    // [DotNetRCAndUtils:969]
                                    // Ensure the format of imagePage is such that it can be
                                    // appended to the writer without error.
                                    SetImageFormat(conversionCommand, imagePage);

                                    // Image must be rotated with forceTrueRotation to true, otherwise
                                    // the output page is not rendered with the correct orientation
                                    // (unclear why).
                                    ImageMethods.RotateImageByDegrees(
                                        imagePage, page.ImageOrientation, true);

                                    writer.AppendImage(imagePage);
                                }
                            }

                            writer.Commit(true);

                            // If the final output is to be a pdf, convert the temporary tif to a
                            // pdf now.
                            if (temporaryFile != null)
                            {
                                ImageMethods.ConvertTifToPdf(temporaryFile.FileName, FileName, true);
                                temporaryFile.Dispose();
                            }
                        }
                        finally
                        {
                            try
                            {
                                if (writer != null)
                                {
                                    writer.Dispose();
                                }

                                CollectionMethods.ClearAndDispose(readers);

                                if (temporaryFile != null)
                                {
                                    temporaryFile.Dispose();
                                }
                            }
                            catch (Exception ex)
                            {
                                ex.ExtractLog("ELI35555");
                            }
                        }
                    }
                }

                OnDocumentOutput();

                return true;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35556");
            }
        }

        #endregion Methods

        #region Private Members

        /// <summary>
        /// Initializes the output format parameters based on the <see cref="PageControls"/>.
        /// </summary>
        /// <param name="conversionCommand">The <see cref="ColorResolutionCommand"/> command
        /// used to convert image pages to the output format.</param>
        /// <param name="outputBitsPerPixel">The bits per pixel the output should be.</param>
        /// <param name="outputFormat">The <see cref="RasterImageFormat"/> the output should be.
        /// </param>
        void InitializeOutputFormat(out ColorResolutionCommand conversionCommand,
            out int outputBitsPerPixel, out RasterImageFormat outputFormat)
        {
            conversionCommand = null;
            outputBitsPerPixel = 0;
            outputFormat = RasterImageFormat.CcittGroup4;

            using (ImageCodecs codecs = new ImageCodecs())
            {
                // This code was modified per https://extract.atlassian.net/browse/ISSUE-12470 to
                // look through all pages, not just the first page from each source document.

                // Create a dictionary of original document names of the pages in the output to the
                // pages from those document(s) that are being used.
                Dictionary<string, List<int>> documentPages = new Dictionary<string,List<int>>();
                foreach (PageThumbnailControl pageControl in PageControls)
                {
                    List<int> pageList = null;
                    if (!documentPages.TryGetValue(pageControl.Page.OriginalDocumentName, out pageList))
                    {
                        pageList = new List<int>();
                        documentPages[pageControl.Page.OriginalDocumentName] = pageList;
                    }

                    pageList.Add(pageControl.Page.OriginalPageNumber);
                }

                // Iterate through all source document pages and use the first page matching the
                // highest bitdepth as the format to use for the output document.
                foreach (KeyValuePair<string, List<int>> entry in documentPages)
                {
                    string sourceDocumentName = entry.Key;

                    using (ImageReader imageReader = codecs.CreateReader(sourceDocumentName))
                    {
                        foreach (int page in entry.Value)
                        {
                            using (RasterImage imagePage = imageReader.ReadPage(page))
                            {
                                int bitsPerPixel = imagePage.BitsPerPixel;

                                // The output format should be the first page or the page with the
                                // highest bit depth.
                                if (conversionCommand == null ||
                                    bitsPerPixel > conversionCommand.BitsPerPixel)
                                {
                                    conversionCommand = new ColorResolutionCommand();
                                    conversionCommand.Mode = ColorResolutionCommandMode.InPlace;
                                    conversionCommand.BitsPerPixel = bitsPerPixel;
                                    conversionCommand.DitheringMethod =
                                        RasterDitheringMethod.FloydStein;
                                    conversionCommand.PaletteFlags =
                                            ColorResolutionCommandPaletteFlags.UsePalette;

                                    conversionCommand.Order = imagePage.Order;
                                    conversionCommand.SetPalette(imagePage.GetPalette());

                                    outputBitsPerPixel = bitsPerPixel;
                                    outputFormat = imageReader.Format;
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Sets the format of <see paramref="imagePage"/> to match
        /// <see paramref="conversionCommand"/> if it does not already.
        /// </summary>
        /// <param name="conversionCommand">The <see cref="ColorResolutionCommand"/> used to apply
        /// the desired format.</param>
        /// <param name="imagePage">The <see cref="RasterImage"/> whose format should be set.</param>
        static void SetImageFormat(ColorResolutionCommand conversionCommand, RasterImage imagePage)
        {
            // Check to see if conversion is required.
            if (imagePage.BitsPerPixel == conversionCommand.BitsPerPixel &&
                imagePage.Order == conversionCommand.Order)
            {
                var targetPalette = conversionCommand.GetPalette();
                var imagePalette = imagePage.GetPalette();

                if (targetPalette == null && imagePalette == null)
                {
                    // No conversion is required.
                    return;
                }

                if (targetPalette != null && imagePalette != null &&
                    targetPalette.SequenceEqual(imagePalette))
                {
                    // No conversion is required.
                    return;
                }
            }

            conversionCommand.Run(imagePage);
        }

        /// <summary>
        /// Raises the <see cref="DocumentOutputting"/> event.
        /// </summary>
        /// <param name="eventArgs">The <see cref="System.ComponentModel.CancelEventArgs"/> instance
        /// containing the event data.</param>
        void OnDocumentOutputting(CancelEventArgs eventArgs)
        {
            var eventHandler = DocumentOutputting;
            if (eventHandler != null)
            {
                eventHandler(this, eventArgs);
            }
        }

        /// <summary>
        /// Raises the <see cref="DocumentOutput"/> event.
        /// </summary>
        void OnDocumentOutput()
        {
            var eventHandler = DocumentOutput;
            if (eventHandler != null)
            {
                eventHandler(this, new EventArgs());
            }
        }

        #endregion Private Members
    }
}
