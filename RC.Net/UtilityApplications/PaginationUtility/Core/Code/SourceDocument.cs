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
    /// 
    /// </summary>
    internal class SourceDocument : IDisposable
    {
        #region Fields

        /// <summary>
        /// 
        /// </summary>
        Dictionary<int, Page> _loadingPages = new Dictionary<int, Page>();

        /// <summary>
        /// 
        /// </summary>
        List<Page> _pages = new List<Page>();

        /// <summary>
        /// 
        /// </summary>
        ThumbnailWorker _thumbnailWorker;

        /// <summary>
        /// 
        /// </summary>
        string _fileName;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SourceDocument"/> class.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        public SourceDocument(string fileName)
        {
            try
            {
                ExtractException.Assert("ELI35545", "Missing File", File.Exists(fileName),
                    "Filename", fileName);

                _fileName = fileName;

                _thumbnailWorker = new ThumbnailWorker(_fileName, PageThumbnailControl.ThumbnailSize);

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
        /// 
        /// </summary>
        internal event EventHandler<EventArgs> Disposed;

        #endregion Events

        #region Properties

        /// <summary>
        /// Gets the pages.
        /// </summary>
        public ReadOnlyCollection<Page> Pages
        {
            get
            {
                return _pages.AsReadOnly();
            }
        }

        /// <summary>
        ///
        /// </summary>
        public string FileName
        {
            get
            {
                return _fileName;
            }
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
        /// Handles the ThumbnailLoaded event of the HandleThumbnailWorker control.
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
                        // Thumbnail worker could also be disposed from main thread.
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
        /// 
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
