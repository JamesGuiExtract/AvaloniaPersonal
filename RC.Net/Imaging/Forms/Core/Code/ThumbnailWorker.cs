using Leadtools;
using Leadtools.Codecs;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Threading;

namespace Extract.Imaging.Forms
{
    /// <summary>
    /// Represents a background thread that loads thumbnails for the pages of an image.
    /// </summary>
    public class ThumbnailWorker : BackgroundWorker
    {
        #region ThumbnailWorker Constants

        /// <summary>
        /// The maximum amount of time to wait in the Dispose method for the worker to cancel.
        /// </summary>
        const int _DISPOSE_TIMEOUT = 60000;

        /// <summary>
        /// The maximum interval to wait before checking the time out status.
        /// </summary>
        const int _TIMEOUT_INTERVAL = 250;

        #endregion ThumbnailWorker Constants

        #region ThumbnailWorker Fields

        /// <summary>
        /// The name of the file from which to load thumbnails.
        /// </summary>
        readonly string _fileName;

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
        RasterCodecs _codecs;

        /// <summary>
        /// An array of loaded thumbnail images. A null element indicates the thumbnail has not 
        /// yet been loaded.
        /// </summary>
        RasterImage[] _thumbnails;

        /// <summary>
        /// The number of pages successfully loaded.
        /// </summary>
        int _pagesComplete;

        /// <summary>
        /// <see langword="true"/> if loading thumbnails has been cancelled; 
        /// <see langword="false"/> if loading thumbnails has not been cancelled.
        /// </summary>
        volatile bool _cancelled;

        /// <summary>
        /// Protects access to shared resources.
        /// </summary>
        readonly object _lock = new object();

        #endregion ThumbnailWorker Fields

        #region ThumbnailWorker Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ThumbnailWorker"/> class.
        /// </summary>
        public ThumbnailWorker(string fileName, int pageCount, Size thumbnailSize)
        {
            _fileName = fileName;
            _pageCount = pageCount;
            _thumbnailSize = thumbnailSize;
            _thumbnails = new RasterImage[_pageCount];
            _priorityEndPage = _pageCount;

            RasterCodecs.Startup();
            _codecs = new RasterCodecs();
            _codecs.Options.Tiff.Load.IgnoreViewPerspective = true;
        }

        #endregion ThumbnailWorker Constructors

        #region ThumbnailWorker Methods

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
                CancelAsync();

                // Wait up to the specified time out for the worker to stop
                if (IsBusy && !_cancelled)
                {
                    int remaining = timeout;
                    while (remaining > _TIMEOUT_INTERVAL)
                    {
                        Thread.Sleep(_TIMEOUT_INTERVAL);
                        remaining -= _TIMEOUT_INTERVAL;

                        if (_cancelled)
                        {
                            return;
                        }
                    }

                    if (remaining > 0)
                    {
                        Thread.Sleep(_TIMEOUT_INTERVAL);

                        if (!_cancelled)
                        {
                            throw new ExtractException("ELI27934",
                                "Thumbnail thread cancellation timed out.");
                        }
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
            if (_pagesComplete >= _pageCount)
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
        /// Creates a high quality thumbnail for the specified page of the image.
        /// </summary>
        /// <param name="page">The page from which to create a thumbnail.</param>
        /// <returns>A high quality thumbnail for the specified page of the image.</returns>
        RasterImage CreateThumbnail(int page)
        {
            // Get the dimensions of this page.
            CodecsImageInfo info = _codecs.GetInformation(_fileName, false, page);
            int width = info.Width;
            int height = info.Height;

            // Calculate how far from the desired size the original image is
            double scale = Math.Min(_thumbnailSize.Width / (double)width,
                _thumbnailSize.Height / (double)height);

            // Set the desired width and height, maintaining aspect ratio
            width = (int)(width * scale);
            height = (int)(height * scale);

            return _codecs.Load(_fileName, width, height, 24, RasterSizeFlags.Bicubic, 
                CodecsLoadByteOrder.BgrOrGray, page, page);
        }

        #endregion ThumbnailWorker Methods

        /// <summary>
        /// Raises the <see cref="BackgroundWorker.DoWork"/> event.
        /// </summary>
        /// <param name="e">The event data associated with the 
        /// <see cref="BackgroundWorker.DoWork"/> event.</param>
        protected override void OnDoWork(DoWorkEventArgs e)
        {
            _cancelled = false;

            base.OnDoWork(e);

            // Iterate through each unloaded thumbnail
            int page = GetNextPageToLoad();
            while (page > 0 && !CancellationPending)
            {
                // Load the thumbnail for this page
                RasterImage thumbnail = CreateThumbnail(page);
                lock (_lock)
                {
                    _thumbnails[page - 1] = thumbnail;
                }
                _pagesComplete++;
                
                // Report progress as necessary
                if (WorkerReportsProgress)
                {
                    if (CancellationPending)
                    {
                        break;
                    }

                    ThumbnailWorkerProgress progress = new ThumbnailWorkerProgress(page, thumbnail);
                    ReportProgress(_pagesComplete * 100 / _pageCount, progress);
                }

                page = GetNextPageToLoad();
            }

            if (CancellationPending)
            {
                // The loading has been cancelled
                e.Cancel = true;
                _cancelled = true;
            }
            else
            {
                // Loading completed successfully
                e.Result = _thumbnails;
            }
        }

        #region IDisposable Members

        /// <overloads>Releases resources used by the <see cref="ThumbnailWorker"/>.</overloads>
        /// <summary>
        /// Releases all unmanaged resources used by the <see cref="ThumbnailWorker"/>.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged 
        /// resources; <see langword="false"/> to release only unmanaged resources.</param>        
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Stop processing before disposing of resources
                if (IsBusy)
                {
                    try
                    {
                        Cancel(_DISPOSE_TIMEOUT);
                    }
                    catch (Exception ex)
                    {
                        ExtractException.Log("ELI27935", ex);
                    }
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
                if (_codecs != null)
                {
                    _codecs.Dispose();
                    _codecs = null;
                    RasterCodecs.Shutdown();
                }
            }

            // Dispose of unmanaged resources

            // Dispose of base class
            base.Dispose(disposing);
        }

        #endregion IDisposable Members
    }
}
