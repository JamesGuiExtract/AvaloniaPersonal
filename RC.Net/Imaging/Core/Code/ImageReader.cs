using Extract.Imaging.Utilities;
using Extract.Licensing;
using Extract.Utilities;
using Leadtools;
using Leadtools.Codecs;
using Leadtools.ImageProcessing;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace Extract.Imaging
{
    /// <summary>
    /// A thread-safe reader that can read image files.
    /// </summary>
    public sealed class ImageReader : IDisposable
    {
        #region Constants

        static readonly string _OBJECT_NAME = typeof(ImageReader).ToString();

        #endregion Constants

        #region Fields

        /// <summary>
        /// The name of the file to read.
        /// </summary>
        readonly string _fileName;

        /// <summary>
        /// Used to decode the image file.
        /// </summary>
        RasterCodecs _codecs;

        /// <summary>
        /// Stream that reads <see cref="_fileName"/>.
        /// </summary>
        FileStream _stream;

        /// <summary>
        /// The page count of the image being read.
        /// </summary>
        readonly int _pageCount;

        /// <summary>
        /// The file format of the image being read.
        /// </summary>
        readonly RasterImageFormat _format;

        /// <summary>
        /// Whether pdf should load as bitonal or not
        /// </summary>
        readonly bool _loadPdfAsBitonal;

        /// <summary>
        /// <see langword="true"/> if the image is a portable document format (PDF) file;
        /// <see langword="false"/> if the image is not a PDF file.
        /// </summary>
        public bool IsPdf { get; private set; }

        /// <summary>
        /// The command used to modify PDF files back to 1 bit per pixel after loading
        /// </summary>
        ColorResolutionCommand _bitonalConversionCommand;

        /// <summary>
        /// Image pages that have been cached for this reader.
        /// </summary>
        Dictionary<int, RasterImage> _loadedImages = new Dictionary<int, RasterImage>();

        /// <summary>
        /// Mutex used to synchronize access to the <see cref="ImageReader"/>.
        /// </summary>
        object _lock = new object();

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageReader"/> class.
        /// </summary>
        /// <param name="fileName">The name of the file to read.</param>
        /// <param name="codecs">Used when decoding the image file.</param>
        /// <param name="loadPdfAsBitonal">Whether or not the PDF should be loaded as a bitonal
        /// image or not.</param>
        internal ImageReader(string fileName, RasterCodecs codecs, bool loadPdfAsBitonal)
        {
            try
            {
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI28486",
                    _OBJECT_NAME);

                _fileName = fileName;
                _codecs = codecs;
                _loadPdfAsBitonal = loadPdfAsBitonal;

                // Prevent write access while reading
                FileShare sharing = RegistryManager.LockFiles
                                        ? FileShare.Read : FileShare.ReadWrite | FileShare.Delete;
                _stream = File.Open(fileName, FileMode.Open, FileAccess.Read, sharing);

                // Log that the image reader was created if necessary
                if (RegistryManager.LogFileLocking)
                {
                    ExtractException ee = new ExtractException("ELI29941",
                        "Application trace: Image reader created");
                    ee.Log();
                }

                using (new PdfLock())
                {
                    // Get file information
                    using (CodecsImageInfo info = _codecs.GetInformation(_stream, true))
                    {
                        _pageCount = info.TotalPages;
                        _format = info.Format;
                    }
                }

                IsPdf = ImageMethods.IsPdf(_format);
            }
            catch (Exception ex)
            {
                if (_stream != null)
                {
                    _stream.Dispose();
                    _stream = null;
                }
                if (_codecs != null)
                {
                    _codecs.Dispose();
                    _codecs = null;
                }

                ExtractException ee = new ExtractException("ELI28623",
                    "Unable to create image reader.", ex);
                ee.AddDebugData("File name", fileName, false);
                throw ee;
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets the number of pages of the image being read.
        /// </summary>
        /// <value>The number of pages of the image being read.</value>
        public int PageCount
        {
            get 
            { 
                return _pageCount;
            }
        }

        /// <summary>
        /// Gets the file format of the image file being read.
        /// </summary>
        /// <value>The file format of the image file being read.</value>
        public RasterImageFormat Format
        {
            get 
            { 
                return _format;
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Loads a page into the image cache.
        /// </summary>
        /// <param name="pageNumber">The page to load.</param>
        public void CachePage(int pageNumber)
        {
            RasterImage image = null;

            try
            {
                lock (_lock)
                using (new PdfLock(IsPdf))
                {
                    if (!_loadedImages.TryGetValue(pageNumber, out image))
                    {
                        image = _codecs.Load(_stream, 0, CodecsLoadByteOrder.BgrOrGray,
                            pageNumber, pageNumber);

                        // If loading PDF as bitonal and this file is a PDF, set bits per pixel to 1
                        if (_loadPdfAsBitonal && IsPdf)
                        {
                            ConvertToBitonalImage(image);
                        }

                        _loadedImages[pageNumber] = image;
                    }
                }
            }
            catch (Exception ex)
            {
                if (image != null)
                {
                    image.Dispose();
                }

                ExtractException ee = new ExtractException("ELI30720",
                    "Unable to read page.", ex);
                ee.AddDebugData("File name", _fileName, false);
                ee.AddDebugData("Page number", pageNumber, false);
                throw ee;
            }
        }

        /// <summary>
        /// Creates a <see cref="RasterImage"/> for the specified page.
        /// </summary>
        /// <param name="pageNumber">The 1-based page number to read.</param>
        /// <returns>A <see cref="RasterImage"/> for <paramref name="pageNumber"/>.</returns>
        public RasterImage ReadPage(int pageNumber)
        {
            try
            {
                lock (_lock)
                {
                    RasterImage image;
                    if (_loadedImages.TryGetValue(pageNumber, out image))
                    {
                        return image.Clone();
                    }
                    else
                    {
                        using (new PdfLock(IsPdf))
                        {
                            image = _codecs.Load(_stream, 0, CodecsLoadByteOrder.BgrOrGray,
                                pageNumber, pageNumber);

                            // If loading PDF as bitonal and this file is a PDF, set bits per pixel
                            // to 1
                            if (_loadPdfAsBitonal && IsPdf)
                            {
                                ConvertToBitonalImage(image);
                            }

                            return image;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = new ExtractException("ELI28481",
                    "Unable to read page.", ex);
                ee.AddDebugData("File name", _fileName, false);
                ee.AddDebugData("Page number", pageNumber, false);
                throw ee;
            }
        }

        /// <summary>
        /// Converts the specified <see cref="RasterImage"/> to a bitonal image.
        /// </summary>
        /// <param name="image">The <see cref="RasterImage"/> to convert to a bitonal
        /// image.</param>
        void ConvertToBitonalImage(RasterImage image)
        {
            if (_bitonalConversionCommand == null)
            {
                _bitonalConversionCommand = new ColorResolutionCommand();
                _bitonalConversionCommand.Mode = ColorResolutionCommandMode.InPlace;
                _bitonalConversionCommand.BitsPerPixel = 1;
                _bitonalConversionCommand.DitheringMethod = RasterDitheringMethod.FloydStein;
                _bitonalConversionCommand.PaletteFlags = ColorResolutionCommandPaletteFlags.Fixed;
                _bitonalConversionCommand.Colors = 0;
            }

            _bitonalConversionCommand.Run(image);
        }

        /// <summary>
        /// Creates a <see cref="RasterImage"/> for the specified page, sized to fit within the 
        /// specified dimensions.
        /// </summary>
        /// <param name="pageNumber">The 1-based page number to read.</param>
        /// <param name="fitWithin">The dimensions in pixels in which the resultant image will 
        /// fit while maintaining its aspect ratio.</param>
        /// <returns>A <see cref="RasterImage"/> for <paramref name="pageNumber"/>, sized to fit 
        /// within the specified <paramref name="fitWithin"/>.</returns>
        public RasterImage ReadPageAsThumbnail(int pageNumber, Size fitWithin)
        {
            try
            {
                lock (_lock)
                using (new PdfLock(IsPdf))
                {
                    // Get the dimensions of this page.
                    ImagePageProperties properties = GetPageProperties(pageNumber);

                    // Calculate how far from the desired size the original image is
                    double scale = Math.Min(fitWithin.Width / (double)properties.Width,
                        fitWithin.Height / (double)properties.Height);

                    // Set the desired width and height, maintaining aspect ratio
                    int width = (int)(properties.Width * scale);
                    int height = (int)(properties.Height * scale);

                    return _codecs.Load(_fileName, width, height, 24, RasterSizeFlags.Bicubic,
                        CodecsLoadByteOrder.BgrOrGray, pageNumber, pageNumber); 
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = new ExtractException("ELI28453",
                    "Unable to read page as thumbnail.", ex);
                ee.AddDebugData("Image file", _fileName, false);
                ee.AddDebugData("Page", pageNumber, false);
                throw ee;
            }
        }

        /// <summary>
        /// Reads the <see cref="ImagePageProperties"/> from the specified page.
        /// </summary>
        /// <param name="pageNumber">The 1-based page from which the properties should be read.
        /// </param>
        /// <returns>The <see cref="ImagePageProperties"/> from the specified 
        /// <paramref name="pageNumber"/>.</returns>
        public ImagePageProperties ReadPageProperties(int pageNumber)
        {
            try
            {
                using (new PdfLock(IsPdf))
                {
                    return GetPageProperties(pageNumber); 
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = new ExtractException("ELI28827",
                    "Unable to read page properties.", ex);
                ee.AddDebugData("Image file", _fileName, false);
                ee.AddDebugData("Page", pageNumber, false);
                throw ee;
            }
        }

        /// <summary>
        /// Reads the <see cref="ImagePageProperties"/> from the specified page. Unlike 
        /// <see cref="ReadPageProperties"/>, this method does NOT lock for exclusive PDF reading.
        /// </summary>
        /// <param name="pageNumber">The 1-based page from which the properties should be read.
        /// </param>
        /// <returns>The <see cref="ImagePageProperties"/> from the specified 
        /// <paramref name="pageNumber"/>.</returns>
        ImagePageProperties GetPageProperties(int pageNumber)
        {
            // TODO: Cache image info?
            lock (_lock)
            using (CodecsImageInfo info = _codecs.GetInformation(_stream, false, pageNumber))
            {
                return new ImagePageProperties(info);
            }
        }

        /// <summary>
        /// Reads the annotations tag from the specified page.
        /// </summary>
        /// <param name="pageNumber">The 1-page number from which to read.</param>
        /// <returns>The annotations tag on the specified <paramref name="pageNumber"/>.</returns>
        public RasterTagMetadata ReadTagOnPage(int pageNumber)
        {
            try
            {
                lock (_lock)
                {
                    return IsPdf ? null :
                        _codecs.ReadTag(_fileName, pageNumber, RasterTagMetadata.AnnotationTiff);
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = new ExtractException("ELI28482",
                    "Unable to read tiff annotations.", ex);
                ee.AddDebugData("File name", _fileName, false);
                ee.AddDebugData("Page number", pageNumber, false);
                throw ee;
            }
        }

        /// <summary>
        /// Creates a <see cref="PixelProbe"/> class for the specified page.
        /// </summary>
        /// <param name="pageNumber">The 1-based page number of the page whose pixels should be 
        /// tested.</param>
        public PixelProbe CreatePixelProbe(int pageNumber)
        {
            RasterImage image = null;
            try
            {
                lock(_lock)
                using (new PdfLock(IsPdf))
                {
                    image = _codecs.Load(_stream, 1, CodecsLoadByteOrder.BgrOrGray, pageNumber, pageNumber); 
                }

                return new PixelProbe(image);
            }
            catch (Exception ex)
            {
                // Dispose of the image
                if (image != null)
                {
                    image.Dispose();
                }

                // Wrap as ExtractException
                ExtractException ee = new ExtractException("ELI28612",
                    "Unable to create pixel probe.", ex);
                ee.AddDebugData("Page number", pageNumber, false);
                throw ee;
            } 
        }

        #endregion Methods

        #region IDisposable Members

        /// <summary>
        /// Releases all resources used by the <see cref="ImageReader"/>.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <overloads>Releases resources used by the <see cref="ImageReader"/>.</overloads>
        /// <summary>
        /// Releases all unmanaged resources used by the <see cref="ImageReader"/>.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged 
        /// resources; <see langword="false"/> to release only unmanaged resources.</param>		
        void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose of managed objects
                if (_loadedImages != null)
                {
                    CollectionMethods.ClearAndDispose(_loadedImages);
                    _loadedImages = null;
                }
                if (_stream != null)
                {
                    _stream.Dispose();
                    _stream = null;
                }
                if (_codecs != null)
                {
                    _codecs.Dispose();
                    _codecs = null;
                }

                // Log that the lock was released if necessary
                if (RegistryManager.LogFileLocking)
                {
                    ExtractException ee = new ExtractException("ELI29944",
                        "Application trace: Image reader disposed");
                    ee.Log();
                }
            }

            // Dispose of unmanaged resources
        }

        #endregion IDisposable Members
    }
}