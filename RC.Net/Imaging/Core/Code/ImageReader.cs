using System;
using System.Drawing;
using System.IO;
using Extract.Licensing;
using Leadtools;
using Leadtools.Codecs;

namespace Extract.Imaging
{
    /// <summary>
    /// Represents a reader that can read image files.
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
        /// <see langword="true"/> if the image is a portable document format (PDF) file;
        /// <see langword="false"/> if the image is not a PDF file.
        /// </summary>
        readonly bool _isPdf;

        /// <summary>
        /// Extract licensing.
        /// </summary>
        static readonly LicenseStateCache _license =
            new LicenseStateCache(LicenseIdName.ExtractCoreObjects, _OBJECT_NAME);

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageReader"/> class.
        /// </summary>
        /// <param name="fileName">The name of the file to read.</param>
        /// <param name="codecs">Used when decoding the image file.</param>
        internal ImageReader(string fileName, RasterCodecs codecs)
        {
            try
            {
                _license.Validate("ELI28486");

                _fileName = fileName;
                _codecs = codecs;

                // Prevent write access while reading
                _stream = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);

                using (new PdfLock())
                {
                    // Get file information
                    using (CodecsImageInfo info = _codecs.GetInformation(_stream, true))
                    {
                        _pageCount = info.TotalPages;
                        _format = info.Format;
                    }
                }

                _isPdf = ImageMethods.IsPdf(_format);
            }
            catch (Exception ex)
            {
                if (_stream != null)
                {
                    _stream.Dispose();
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
        /// Creates a <see cref="RasterImage"/> for the specified page.
        /// </summary>
        /// <param name="pageNumber">The 1-based page number to read.</param>
        /// <returns>A <see cref="RasterImage"/> for <paramref name="pageNumber"/>.</returns>
        public RasterImage ReadPage(int pageNumber)
        {
            try
            {
                using (new PdfLock(_isPdf))
                {
                    return _codecs.Load(_stream, 0, CodecsLoadByteOrder.BgrOrGray, pageNumber, pageNumber);
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
        /// Creates a <see cref="RasterImage"/> for the specified page, sized to fit within the 
        /// specified dimensions.
        /// </summary>
        /// <param name="pageNumber">The 1-based page number to read.</param>
        /// <param name="fitWithin">The dimensions in pixels in which the resultant image will 
        /// fit while maintaining its aspect ratio.</param>
        /// <returns>A <see cref="RasterImage"/> for  <paramref name="pageNumber"/>, sized to fit 
        /// within the specified <paramref name="fitWithin"/>.</returns>
        public RasterImage ReadPageAsThumbnail(int pageNumber, Size fitWithin)
        {
            try
            {
                using (new PdfLock(_isPdf))
                {
                    // TODO: Cache image info
                    // Get the dimensions of this page.
                    int width;
                    int height;
                    using (CodecsImageInfo info = _codecs.GetInformation(_stream, false, pageNumber))
                    {
                        width = info.Width;
                        height = info.Height;
                    } 

                    // Calculate how far from the desired size the original image is
                    double scale = Math.Min(fitWithin.Width / (double)width,
                        fitWithin.Height / (double)height);

                    // Set the desired width and height, maintaining aspect ratio
                    width = (int)(width * scale);
                    height = (int)(height * scale);

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
        /// Reads the annotations tag from the specified page.
        /// </summary>
        /// <param name="pageNumber">The 1-page number from which to read.</param>
        /// <returns>The annotations tag on the specified <paramref name="pageNumber"/>.</returns>
        public RasterTagMetadata ReadTagOnPage(int pageNumber)
        {
            try
            {
                return _isPdf ? null : 
                    _codecs.ReadTag(_fileName, pageNumber, RasterTagMetadata.AnnotationTiff);
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
                using (new PdfLock(_isPdf))
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
                if (_codecs != null)
                {
                    _codecs.Dispose();
                    _codecs = null;
                }
                if (_stream != null)
                {
                    _stream.Dispose();
                    _stream = null;
                }
            }

            // Dispose of unmanaged resources
        }

        #endregion IDisposable Members
    }
}