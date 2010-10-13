using Extract.Licensing;
using Leadtools;
using System;
using System.Drawing;
using System.Threading;

namespace Extract.Imaging.Forms
{
    /// <summary>
    /// Represents a background thread that loads thumbnails for the pages of an image.
    /// </summary>
    public sealed class ThumbnailWorker : IDisposable
    {
        #region Constants

        /// <summary>
        /// The maximum amount of time to wait in the Dispose method for the worker to cancel.
        /// </summary>
        const int _DISPOSE_TIMEOUT = 10000;

        /// <summary>
        /// The maximum interval to wait before checking the time out status.
        /// </summary>
        const int _TIMEOUT_INTERVAL = 250;

        static readonly string _OBJECT_NAME = typeof(ThumbnailWorker).ToString();

        #endregion Constants

        #region Fields

        /// <summary>
        /// The thread in which the loading is performed.
        /// </summary>
        readonly Thread _thread;

        /// <summary>
        /// The number of the pages in the file from which to load thumbnails.
        /// </summary>
        readonly int _pageCount;

        /// <summary>
        /// The size of the thumbnails to load.
        /// </summary>
        readonly Size _thumbnailSize;

        /// <summary>
        /// The first page of high priority thumbnails to load.
        /// </summary>
        volatile int _priorityStartPage = 1;

        /// <summary>
        /// The last page of high priority thumbnails to load.
        /// </summary>
        volatile int _priorityEndPage;

        /// <summary>
        /// Codecs for loading thumbnails.
        /// </summary>
        ImageCodecs _codecs = new ImageCodecs();

        /// <summary>
        /// Decodes the image thumbnails from disk.
        /// </summary>
        ImageReader _reader;

        /// <summary>
        /// An array of loaded thumbnail images. A null element indicates the thumbnail has not 
        /// yet been loaded.
        /// </summary>
        RasterImage[] _thumbnails;

        /// <summary>
        /// <see langword="true"/> if thumbnails are being loaded;
        /// <see langword="false"/> if thumbnails are not being loaded.
        /// </summary>
        bool _running;

        /// <summary>
        /// Raised when the worker is un-paused. When paused, this event is reset.
        /// </summary>
        EventWaitHandle _unPausedEvent = new ManualResetEvent(true);

        /// <summary>
        /// Raised when the worker is cancelled.
        /// </summary>
        EventWaitHandle _cancelledEvent = new ManualResetEvent(false);

        /// <summary>
        /// Protects access to shared resources.
        /// </summary>
        readonly object _lock = new object();

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ThumbnailWorker"/> class.
        /// </summary>
        public ThumbnailWorker(string fileName, Size thumbnailSize)
        {
            LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI28489",
                _OBJECT_NAME);

            _reader = _codecs.CreateReader(fileName);
            _pageCount = _reader.PageCount;
            _thumbnailSize = thumbnailSize;
            _thumbnails = new RasterImage[_pageCount];
            _priorityEndPage = _pageCount;

            _thread = new Thread(LoadThumbnails);
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets whether the <see cref="ThumbnailWorker"/> is in the process of loading thumbnails.
        /// (The process may or may not be paused).
        /// </summary>
        /// <value><see langword="true"/> if the <see cref="ThumbnailWorker"/> is in the process of 
        /// loading thumbnails; <see langword="false"/> if it is not loading thumbnails.</value>
        public bool IsRunning
        {
            get
            {
                return _running;
            }
        }

        /// <summary>
        /// Gets or sets whether the worker's processing is paused.
        /// </summary>
        /// <value><see langword="true"/> if processing is paused, <see langword="false"/>
        /// otherwise.</value>
        public bool Paused
        {
            get
            {
                return !_unPausedEvent.WaitOne(0);
            }

            set
            {
                try
                {
                    if (value)
                    {
                        _unPausedEvent.Reset();
                    }
                    else
                    {
                        _unPausedEvent.Set();
                    }
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI30744", ex);
                }
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Cancels the pending background operation, waiting up to the specified number of 
        /// milliseconds. 
        /// </summary>
        /// <param name="timeout">The maximum number of milliseconds to wait for processing to 
        /// cancel.</param>
        public void Cancel(int timeout)
        {
            try
            {
                // Signal the worker to stop
                _cancelledEvent.Set();

                // Wait up to the specified time out for the worker to stop
                if (IsRunning)
                {
                    int remaining = timeout;
                    while (remaining > _TIMEOUT_INTERVAL)
                    {
                        Thread.Sleep(_TIMEOUT_INTERVAL);
                        remaining -= _TIMEOUT_INTERVAL;

                        if (!IsRunning)
                        {
                            return;
                        }
                    }

                    if (remaining > 0)
                    {
                        Thread.Sleep(remaining);
                    }

                    if (IsRunning)
                    {
                        _thread.Abort();
                        throw new ExtractException("ELI27934",
                            "Thumbnail thread cancellation timed out.");
                    }
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = new ExtractException("ELI27856",
                    "Unable to cancel thumbnail loading.", ex);
                ee.AddDebugData("Time out", timeout, false);
                throw ee;
            }
        }

        /// <summary>
        /// Specifies pages that should be loaded as thumbnails first.
        /// </summary>
        /// <param name="startPage">The 1-based first page of the high priority thumbnail pages.</param>
        /// <param name="endPage">The 1-based last page of the high priority thumbnail pages.</param>
        public void SetPriorityThumbnails(int startPage, int endPage)
        {
            try
            {
                lock (_lock)
                {
                    _priorityStartPage = startPage;
                    _priorityEndPage = endPage;
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = new ExtractException("ELI27923",
                    "Unable to set thumbnail page priorities.", ex);
                ee.AddDebugData("Start page", startPage, false);
                ee.AddDebugData("End page", endPage, false);
                throw ee;
            }
        }

        /// <summary>
        /// Gets the thumbnail for the specified page.
        /// </summary>
        /// <param name="page">The 1-based page number of the thumbnail to retrieve.</param>
        /// <returns>A thumbnail version of the image of <paramref name="page"/>; or 
        /// <see langword="null"/> if the thumbnail has not yet been created.</returns>
        public RasterImage GetThumbnail(int page)
        {
            try
            {
                if (page < 1 || page > _pageCount)
                {
                    throw new ExtractException("ELI27924", "Invalid page number.");
                }

                lock (_lock)
                {
                    return _thumbnails[page - 1];
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = new ExtractException("ELI27925",
                    "Unable to get thumbnail.", ex);
                ee.AddDebugData("Page", page, false);
                throw ee;
            }
        }

        /// <summary>
        /// Calculates the next 1-based page load. Priority pages are loaded first; then pages are 
        /// loaded sequentially starting at the beginning.
        /// </summary>
        /// <returns>The next 1-based page to load; or -1 if there are no more pages to load.
        /// </returns>
        int GetNextPageToLoad()
        {
            // Wait here if processing is paused but not cancelled.
            WaitHandle.WaitAny(new WaitHandle[] { _unPausedEvent, _cancelledEvent });

            if (_cancelledEvent.WaitOne(0))
            {
                return -1;
            }

            lock (_lock)
            {
                // Return the first priority unloaded page.
                int page = GetUnloadedPageInRange(_priorityStartPage, _priorityEndPage);
                if (page > 0)
                {
                    return page;
                }

                // Return the first page prior to the first priority page
                page = GetUnloadedPageInRange(1, _priorityStartPage - 1);
                if (page > 0)
                {
                    return page;
                }

                // Return the first page after the last priority page
                return GetUnloadedPageInRange(_priorityEndPage + 1, _pageCount);
            }
        }

        /// <summary>
        /// Gets the first unloaded 1-based page in the specified page range.
        /// </summary>
        /// <param name="startPage">The first 1-based page to check.</param>
        /// <param name="endPage">The last 1-based page to check.</param>
        /// <returns>The first unloaded 1-based between <paramref name="startPage"/> (inclusive) 
        /// and <paramref name="endPage"/> (inclusive).</returns>
        int GetUnloadedPageInRange(int startPage, int endPage)
        {
            for (int i = startPage - 1; i < endPage; i++)
            {
                if (_thumbnails[i] == null)
                {
                    return i + 1;
                }
            }

            return -1;
        }

        /// <summary>
        /// Asynchronously starts loading thumbnail images.
        /// </summary>
        public void BeginLoading()
        {
            _thread.Start();
        }

        /// <summary>
        /// Loads and stores thumbnail representation of the pages of the image.
        /// </summary>
        void LoadThumbnails()
        {
            try
            {
                _cancelledEvent.Reset();
                _unPausedEvent.Set();
                _running = true;

                // Iterate through each unloaded thumbnail
                int page = GetNextPageToLoad();
                while (page > 0)
                {
                    // Load the thumbnail for this page
                    RasterImage thumbnail = _reader.ReadPageAsThumbnail(page, _thumbnailSize);
                    lock (_lock)
                    {
                        _thumbnails[page - 1] = thumbnail;
                    }

                    page = GetNextPageToLoad();
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI27937", ex);
            }
            finally
            {
                _running = false;
            }
        }

        #endregion Methods

        #region IDisposable Members

        /// <summary>
        /// Releases all resources used by the <see cref="ThumbnailWorker"/>.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <overloads>Releases resources used by the <see cref="ThumbnailWorker"/>.</overloads>
        /// <summary>
        /// Releases all unmanaged resources used by the <see cref="ThumbnailWorker"/>.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged 
        /// resources; <see langword="false"/> to release only unmanaged resources.</param>        
        void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Stop processing before disposing of resources
                try
                {
                    Cancel(_DISPOSE_TIMEOUT);
                }
                catch (Exception ex)
                {
                    ExtractException.Log("ELI27935", ex);
                }

                // Dispose of managed resources
                if (_thumbnails != null)
                {
                    foreach (RasterImage image in _thumbnails)
                    {
                        if (image != null)
                        {
                            image.Dispose();
                        }
                    }
                    _thumbnails = null;
                }
                if (_reader != null)
                {
                    _reader.Dispose();
                    _reader = null;
                }
                if (_codecs != null)
                {
                    _codecs.Dispose();
                    _codecs = null;
                }
                if (_unPausedEvent != null)
                {
                    _unPausedEvent.Dispose();
                    _unPausedEvent = null;
                }
                if (_cancelledEvent != null)
                {
                    _cancelledEvent.Dispose();
                    _cancelledEvent = null;
                }
            }

            // Dispose of unmanaged resources
        }

        #endregion IDisposable Members
    }
}
