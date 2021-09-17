using Extract.Licensing;
using Extract.Utilities;
using Leadtools;
using Leadtools.Codecs;
using Leadtools.ImageProcessing;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Runtime.Caching;
using System.Threading;

using static System.FormattableString;

namespace Extract.Imaging
{
    /// <summary>
    /// A thread-safe reader that can read image files.
    /// </summary>
    public sealed class ImageReader : IDisposable
    {
        #region Constants

        static readonly string _OBJECT_NAME = typeof(ImageReader).ToString();

        /// <summary>
        /// Number of retries to perform if the GetInformation call fails with
        /// a BadPdfContent exception. This can be removed when [DNRCAU #662] is patched.
        /// </summary>
        const int _MAX_RETRIES = 3;

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
        /// The position in the stream at which the image begins
        /// </summary>
        long _startOfImage;

        /// <summary>
        /// The length in bytes of the image portion of the stream
        /// </summary>
        long _lengthOfImage;

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
        public bool IsPdf { get; private set; }

        /// <summary>
        /// Image pages that have been cached for this reader.
        /// </summary>
        Dictionary<int, RasterImage> _loadedImages = new Dictionary<int, RasterImage>();

        /// <summary>
        /// Mutex used to synchronize access to the <see cref="ImageReader"/>.
        /// </summary>
        object _lock = new object();

        /// <summary>
        /// Stores the last loaded pixel probe and its page number.
        /// </summary>
        Tuple<int, PixelProbe> _currentProbe = new Tuple<int,PixelProbe>(-1, null);

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
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI28486",
                    _OBJECT_NAME);

                _fileName = fileName;
                _codecs = codecs;

                // Prevent write access while reading
                FileShare sharing = RegistryManager.LockFiles
                                        ? FileShare.Read : FileShare.ReadWrite | FileShare.Delete;

                FileSystemMethods.PerformFileOperationWithRetry(() =>
                    _stream = File.Open(fileName, FileMode.Open, FileAccess.Read, sharing), true);

                _startOfImage = 0;
                _lengthOfImage = _stream.Length;

                // Find the start of the PDF, since LEADTOOLS has problems with some images
                // when there is a prefix before the start tag
                // https://extract.atlassian.net/browse/ISSUE-14637
                if (fileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                {
                    const int MAX_PREFIX = 1024;
                    var buf = new byte[MAX_PREFIX + 4];
                    int totalBytesRead = _stream.Read(buf, 0, buf.Length);
                    int maxStartIndex = totalBytesRead - 4;
                    bool foundStart = false;
                    for (int i = 0; i < maxStartIndex; ++i)
                    {
                        if (buf[i] == '%'
                            && buf[i + 1] == 'P'
                            && buf[i + 2] == 'D'
                            && buf[i + 3] == 'F')
                        {
                            foundStart = true;
                            _startOfImage = i;
                            _lengthOfImage -= _startOfImage;
                            break;
                        }
                    }

                    // If not able to find the start prefix, reset the stream to the beginning
                    // and hope for the best
                    if (!foundStart)
                    {
                        _startOfImage = 0;
                    }
                }

                // Log that the image reader was created if necessary
                if (RegistryManager.LogFileLocking)
                {
                    ExtractException ee = new ExtractException("ELI29941",
                        "Application trace: Image reader created");
                    ee.Log();
                }

                // Get file information
                // TODO: Remove this retry code once the bug in leadtools referenced by
                // [DNRCAU #662] is officially fixed.
                int retryCount = 0;
                do
                {
                    try
                    {
                        int pageCount = 0;
                        RasterImageFormat format = RasterImageFormat.Unknown;
                        // https://extract.atlassian.net/browse/ISSUE-11972
                        // To avoid problems with network hiccups, after the initial load of an
                        // image, allow retries for any windows error code, not just sharing
                        // violations.
                        FileSystemMethods.PerformFileOperationWithRetry(() =>
                        {
                            _stream.Position = _startOfImage;
                            using (CodecsImageInfo info = _codecs.GetInformation(_stream, true))
                            {
                                pageCount = info.TotalPages;
                                format = info.Format;
                            }
                        },
                        false);

                        _pageCount = pageCount;
                        _format = format;
                        break;
                    }
                    catch (RasterException re)
                    {
                        // If the raster exception is Bad PDF content, then just retry
                        // the check (after sleeping for a random amount of time). Any
                        // other exception just throw it out
                        if (re.Code != RasterExceptionCode.PdfBadContent
                            || (++retryCount) > _MAX_RETRIES)
                        {
                            throw;
                        }
                    }

                    Thread.Sleep((new Random().Next(100, 500)));
                } while (true);

                IsPdf = ImageMethods.IsPdf(_format);

                // Make sure to reset the stream position to the beginning so that
                // calls to _codecs.Load that pass in an offset work correctly
                _stream.Position = 0;
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
                {
                    if (!_loadedImages.TryGetValue(pageNumber, out image))
                    {
                        // https://extract.atlassian.net/browse/ISSUE-11972
                        // To avoid problems with network hiccups, after the initial load of an
                        // image, allow retries for any windows error code, not just sharing
                        // violations.
                        FileSystemMethods.PerformFileOperationWithRetry(() =>
                            image = _codecs.Load(_stream, _startOfImage, _lengthOfImage, 0, CodecsLoadByteOrder.BgrOrGray,
                                pageNumber, pageNumber)
                        , false);

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
                        // https://extract.atlassian.net/browse/ISSUE-11972
                        // To avoid problems with network hiccups, after the initial load of an
                        // image, allow retries for any windows error code, not just sharing
                        // violations.
                        FileSystemMethods.PerformFileOperationWithRetry(() =>
                            image = _codecs.Load(_stream, _startOfImage, _lengthOfImage, 0, CodecsLoadByteOrder.BgrOrGray,
                                pageNumber, pageNumber)
                        , false);

                        return image;
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
                // Get the dimensions of this page.
                ImagePageProperties properties = GetPageProperties(pageNumber);

                // Calculate how far from the desired size the original image is
                double scale = Math.Min(fitWithin.Width / (double)properties.Width,
                    fitWithin.Height / (double)properties.Height);

                // Set the desired width and height, maintaining aspect ratio
                int width = (int)(properties.Width * scale);
                int height = (int)(properties.Height * scale);

                // There is no overload that resizes _and_ loads from an offset but
                // setting the position of the stream seems to work
                _stream.Position = _startOfImage;
                return _codecs.Load(_stream, width, height, 24, RasterSizeFlags.Bicubic,
                    CodecsLoadByteOrder.BgrOrGray, pageNumber, pageNumber);
            }
            catch (Exception ex)
            {
                ExtractException ee = new ExtractException("ELI28453",
                    "Unable to read page as thumbnail.", ex);
                ee.AddDebugData("Image file", _fileName, false);
                ee.AddDebugData("Page", pageNumber, false);
                throw ee;
            }
            finally
            {
                // Make sure to reset the stream position to the beginning so that
                // calls to _codecs.Load that pass in an offset work correctly
                _stream.Position = 0;
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
                return GetPageProperties(pageNumber); 
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
            // Cache info so that correcting spatial data on large images works well
            // https://extract.atlassian.net/projects/ISSUE/issues/ISSUE-15299
            var key = Invariant($"ImagePageProperties{_fileName}:{pageNumber}");
            var cache = MemoryCache.Default;
            var result = cache.Get(key) as ImagePageProperties;
            if (result == null)
            {
                lock (_lock)
                {
                    result = cache.Get(key) as ImagePageProperties;
                    if (result == null)
                    {
                        try
                        {

                            _stream.Position = _startOfImage;
                            using (CodecsImageInfo info = _codecs.GetInformation(_stream, false, pageNumber))
                            {
                                result = new ImagePageProperties(info);
                            }
                        }
                        finally
                        {
                            // Make sure to reset the stream position to the beginning so that
                            // calls to _codecs.Load that pass in an offset work correctly
                            _stream.Position = 0;
                        }

                        CacheItemPolicy policy = new CacheItemPolicy
                        {
                            SlidingExpiration = TimeSpan.FromMinutes(30)
                        };
                        cache.Set(key, result, policy);
                    }
                }
            }

            return result;
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
                {
                    if (_currentProbe.Item1 != pageNumber)
                    {
                        if (_currentProbe.Item2 != null)
                        {
                            _currentProbe.Item2.Dispose();
                        }
                        // https://extract.atlassian.net/browse/ISSUE-11972
                        // To avoid problems with network hiccups, after the initial load of an
                        // image, allow retries for any windows error code, not just sharing
                        // violations.
                        FileSystemMethods.PerformFileOperationWithRetry(() =>
                            image = _codecs.Load(_stream, _startOfImage, _lengthOfImage, 1, CodecsLoadByteOrder.BgrOrGray,
                                pageNumber, pageNumber)
                        , false);
                        var probe = new PixelProbe(image);
                        image = null;
                        _currentProbe = new Tuple<int, PixelProbe>(pageNumber, probe);
                    }

                    return _currentProbe.Item2.Copy();
                }
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
                if (_currentProbe != null && _currentProbe.Item2 != null)
                {
                    _currentProbe.Item2.Dispose();
                    _currentProbe = null;
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