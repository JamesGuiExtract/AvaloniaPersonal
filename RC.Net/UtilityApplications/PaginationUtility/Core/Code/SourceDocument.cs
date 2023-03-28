using Extract.Imaging;
using Extract.Imaging.Forms;
using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;

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
        /// <param name="fileID">The ID of the file in the FAMDB</param>
        /// <param name="autoRotatePages"><c>true</c> if pages should automatically be rotated to
        /// match the orientation of the text; otherwise, <c>false</c>.</param>
        public SourceDocument(string fileName, int fileID, bool autoRotatePages)
        {
            try
            {
                ExtractException.Assert("ELI35545", "Missing File", File.Exists(fileName),
                    "Filename", fileName);

                FileName = fileName;
                FileID = fileID;

                // Retrieve spatialPageInfos to obtain page orientation if autoRotatePages is true.
                var spatialPageInfos = autoRotatePages
                    ? ImageMethods.GetSpatialPageInfos(fileName)
                    : null;

                _thumbnailWorker = new ThumbnailWorker(FileName, PageThumbnailControl.ThumbnailSize, true);

                // Initialize a Page instance for each page of the document with a placeholder
                // thumbnail that will be replaced as the _thumbnailWorker loads the thumbnails.
                for (int pageNumber = 1; pageNumber <= _thumbnailWorker.PageCount; pageNumber++)
                {
                    var page = new Page(this, pageNumber);

                    if (spatialPageInfos != null)
                    {
                        var orientation = ImageMethods.GetPageRotation(spatialPageInfos, pageNumber);
                        if (orientation != null)
                        {
                            page.ProposedOrientation = orientation.Value;
                            page.ImageOrientation = orientation.Value;
                        }
                    }

                    page.ThumbnailRequested += HandlePage_ThumbnailRequested;

                    _pages.Add(page);
                    _loadingPages[pageNumber] = page;
                }

                _thumbnailWorker.ThumbnailLoaded += HandleThumbnailWorker_ThumbnailLoaded;
            }
            catch (Exception ex)
            {
                var ee = new ExtractException("ELI35546", "Unable to open document", ex);
                ee.AddDebugData("Filename", fileName, false);
                throw ee;
            }
        }

        #endregion Constructors

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

        /// <summary>
        /// Gets the file ID.
        /// </summary>
        public int FileID
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
        }

        #endregion IDisposable Members

        #region Event Handlers

        /// <summary>
        /// Handles the <see cref="ThumbnailRequested"/> event of a <see cref="Page"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        void HandlePage_ThumbnailRequested(object sender, EventArgs e)
        {
            try
            {
                var page = (Page)sender;

                _thumbnailWorker.GetThumbnail(page.OriginalPageNumber);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI43390");
            }
        }

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
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35547");
            }
        }

        #endregion Event Handlers
    }
}
