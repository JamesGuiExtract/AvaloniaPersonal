using Extract.Imaging.Forms;
using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;

namespace Extract.UtilityApplications.PaginationUtility
{
    /// <summary>
    /// Represents a document loaded as input in its original form.
    /// </summary>
    internal class SourceDocument : IDisposable
    {
        #region Fields

        /// <summary>
        /// The <see cref="Page"/> instances that represent the pages of the original document.
        /// </summary>
        List<Page> _pages = new List<Page>();

        /// <summary>
        /// Keeps track of the pages for which thumbnails have yet to be loaded.
        /// </summary>
        Dictionary<int, Page> _loadingPages = new Dictionary<int, Page>();

        /// <summary>
        /// Works in a background thread to load thumbnails for this document.
        /// </summary>
        ThumbnailWorker _thumbnailWorker;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SourceDocument"/> class.
        /// </summary>
        /// <param name="fileName">Name of the file to load.</param>
        public SourceDocument(string fileName)
        {
            try
            {
                ExtractException.Assert("ELI35545", "Missing File", File.Exists(fileName),
                    "Filename", fileName);

                FileName = fileName;

                _thumbnailWorker = new ThumbnailWorker(FileName, PageThumbnailControl.ThumbnailSize);

                // Initialize a Page instance for each page of the document with a placeholder
                // thumbnail that will be replaced as the _thumbnailWorker loads the thumbnails.
                for (int pageNumber = 1; pageNumber <= _thumbnailWorker.PageCount; pageNumber++)
                {
                    var page = new Page(this, pageNumber);
                    _pages.Add(page);
                    _loadingPages[pageNumber] = page;
                }

                _thumbnailWorker.ThumbnailLoaded += HandleThumbnailWorker_ThumbnailLoaded;
                _thumbnailWorker.BeginLoading();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35546");
            }
        }

        #endregion Constructors

        #region Events
        
        /// <summary>
        /// Raised when this instance is disposed of.
        /// </summary>
        internal event EventHandler<EventArgs> Disposed;

        #endregion Events

        #region Properties

        /// <summary>
        /// Gets the <see cref="Page"/>s that comprise this document.
        /// </summary>
        public ReadOnlyCollection<Page> Pages
        {
            get
            {
                return _pages.AsReadOnly();
            }
        }

        /// <summary>
        /// Gets the filename of the document.
        /// </summary>
        public string FileName
        {
            get;
            private set;
        }

        #endregion Properties

        #region IDisposable Members

        /// <summary>
        /// Releases all resources used by the <see cref="SourceDocument"/>.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <overloads>Releases resources used by the <see cref="SourceDocument"/>.</overloads>
        /// <summary>
        /// Releases all unmanaged resources used by the <see cref="SourceDocument"/>.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged 
        /// resources; <see langword="false"/> to release only unmanaged resources.</param>        
        void Dispose(bool disposing)
        {
            if (disposing)
            {
                try
                {
                    // Dispose of managed resources

                    // Get local reference since _thumbnailWorker can be disposed of on a background
                    // thread.
                    var thumbnailWorker = _thumbnailWorker;
                    if (thumbnailWorker != null)
                    {
                        thumbnailWorker.Dispose();
                        _thumbnailWorker = null;
                    }

                    CollectionMethods.ClearAndDispose(_pages);
                }
                catch { }
            }

            // Dispose of ummanaged resources
            try
            {
                OnDisposed();
            }
            catch { }
        }

        #endregion IDisposable Members

        #region Event Handlers

        /// <summary>
        /// Handles the <see cref="ThumbnailWorker.ThumbnailLoaded"/> event of the
        /// <see cref="_thumbnailWorker"/> control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Extract.Imaging.Forms.ThumbnailLoadedEventArgs"/>
        /// instance containing the event data.</param>
        void HandleThumbnailWorker_ThumbnailLoaded(object sender, ThumbnailLoadedEventArgs e)
        {
            try
            {
                var page = _loadingPages[e.PageNumber];
                if (!page.IsDisposed)
                {
                    page.ThumbnailImage = e.ThumbnailImage;
                }

                _loadingPages.Remove(e.PageNumber);

                if (_loadingPages.Count == 0)
                {
                    Task.Factory.StartNew(() =>
                    {
                        // Thumbnail worker could also be disposed from UI thread, so invoke the
                        // call to dispose so that is it not disposed of while the UI thread is
                        // using it.
                        var thumbnailWorker = _thumbnailWorker;
                        if (thumbnailWorker != null)
                        {
                            thumbnailWorker.Dispose();
                            _thumbnailWorker = null;
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35547");
            }
        }

        #endregion Event Handlers

        #region Private Members

        /// <summary>
        /// Raises the <see cref="Disposed"/> event.
        /// </summary>
        void OnDisposed()
        {
            var eventHandler = Disposed;
            if (eventHandler != null)
            {
                eventHandler(this, new EventArgs());
            }
        }

        #endregion Private Members
    }
}
