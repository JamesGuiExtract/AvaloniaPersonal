using Extract.Imaging;
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

        /// <summary>
        /// The <see cref="Page"/>s that represent the state which <see cref="SetOriginalForm"/> was
        /// called.
        /// </summary>
        List<Page> _originalPages = null;

        /// <summary>
        /// The <see cref="Page"/>s from <see cref="_originalPages"/> that were originally in a
        /// deleted state.
        /// </summary>
        HashSet<Page> _originalDeletedPages = null;

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
        /// Gets or sets a value indicating whether the document has changed compared to the point
        /// at which <see cref="SetOriginalForm"/> was called.
        /// </summary>
        /// <value><see langword="true"/> if in the original form; otherwise, <see langword="false"/>.
        /// </value>
        public bool InOriginalForm
        {
            get
            {
                try
                {
                    // If there are no original pages specified, only in original form if there are no
                    // page controls.
                    if (_originalPages == null)
                    {
                        return !_pageControls.Any();
                    }

                    // Ensure the same sequence of and orientation of current pages compared to
                    // _originalPages.
                    var currentPages = _pageControls
                        .Where(c => !c.Deleted)
                        .Select(c => c.Page);
                    return Page.PagesAreEqual(currentPages,
                        _originalPages.Where(c => !_originalDeletedPages.Contains(c)));
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI39659");
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the document has changed compared to the input
        /// the source documents as they currently exist on disk.
        /// </summary>
        /// <value><see langword="true"/> if in the source document form; otherwise,
        /// <see langword="false"/>.
        /// </value>
        public bool InSourceDocForm
        {
            get
            {
                try
                {
                    var pages = _pageControls
                        .Where(c => !c.Deleted)
                        .Select(c => c.Page)
                        .ToArray();

                    // If there is not exactly one source document for this output document, the
                    // document cannot be in source document form.
                    if (pages.Select(page => page.SourceDocument)
                        .Distinct()
                        .Count() != 1)
                    {
                        return false;
                    }

                    var currentPages = _pageControls
                        .Where(c => !c.Deleted)
                        .Select(c => c.Page);
                    var sourceDocPages = pages.First().SourceDocument.Pages;

                    // Ensure the same sequence of and orientation of current pages compared to
                    // sourceDocPages.
                    return Page.PagesAreEqual(currentPages, sourceDocPages);
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI39660");
                }
            }
        }

        /// <summary>
        /// Gets the document's <see cref="Page"/>s as they existed when
        /// <see cref="SetOriginalForm"/> was called.
        /// </summary>
        public ReadOnlyCollection<Page> OriginalPages
        {
            get
            {
                try
                {
                    return _originalPages.AsReadOnly();
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI40046");
                }
            }
        }

        /// <summary>
        /// Gets the <see cref="Page"/>s that were in a deleted state when
        /// <see cref="SetOriginalForm"/> was called.
        /// </summary>
        public ReadOnlyCollection<Page> OriginalDeletedPages
        {
            get
            {
                try
                {
                    return _originalDeletedPages.ToList().AsReadOnly();
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI40047");
                }
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Defines the current document state as the one represented by <see cref="OriginalPages"/>
        /// and <see cref="InOriginalForm"/>.
        /// </summary>
        public void SetOriginalForm()
        {
            try
            {
                _originalPages = new List<Page>(_pageControls
                    .Select(c => c.Page));

                _originalDeletedPages = new HashSet<Page>(_pageControls
                    .Where(c => c.Deleted)
                    .Select(c => c.Page));
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39661");
            }
        }

        /// <summary>
        /// Adds the specified <see paramref="pageControl"/> as the last page of the document.
        /// </summary>
        /// <param name="pageControl">The <see cref="PageThumbnailControl"/> representing the page
        /// to be added.</param>
        public virtual void AddPage(PageThumbnailControl pageControl)
        {
            try
            {
                InsertPage(pageControl, _pageControls.Count);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35550");
            }
        }

        /// <summary>
        /// Inserts the specified <see paramref="pageControl"/> at the specified zero-based page
        /// index.
        /// <para><b>Note</b></para>
        /// index != page number; index may reflect preceding deleted pages that will not appear in
        /// the resulting document.
        /// </summary>
        /// <param name="pageControl">The <see cref="PageThumbnailControl"/> representing the page
        /// to be inserted.</param>
        /// <param name="pageIndex">The index the new page should be inserted at.</param>
        public virtual void InsertPage(PageThumbnailControl pageControl, int pageIndex)
        {
            try
            {
                ExtractException.Assert("ELI35551", "Invalid page index",
                    pageIndex >= 0 && pageIndex <= _pageControls.Count,
                    "Document", FileName, "Page", pageIndex);

                if (!_pageControls.Contains(pageControl))
                {
                    pageControl.DeletedStateChanged += HandlePageControl_DeletedStateChanged;
                }

                if (pageIndex < _pageControls.Count)
                {
                    _pageControls.Insert(pageIndex, pageControl);
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
        /// Handles the DeletedStateChanged event of the pageControl control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void HandlePageControl_DeletedStateChanged(object sender, EventArgs e)
        {
            try
            {
                foreach (var pageControl in _pageControls)
                {
                    pageControl.Invalidate();
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI40044");
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

                if (_pageControls.Remove(pageControl))
                {
                    pageControl.DeletedStateChanged -= HandlePageControl_DeletedStateChanged;
                }

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
                // If there are no un-deleted pages, there is nothing to output.
                if (!_pageControls.Any(c => !c.Deleted))
                {
                    return true;
                }

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

                bool copyOriginalDocument = InOriginalForm;
                Page firstPage = _pageControls
                    .Where(pageControl => !pageControl.Deleted)
                    .First()
                    .Page;
                if (copyOriginalDocument)
                {
                    // [DotNetRCAndUtils:972]
                    // If the extension of the file has changed, it is likely the user is intending
                    // to output the document in a different format.
                    string extension = Path.GetExtension(FileName);
                    string originalExtension =
                        Path.GetExtension(firstPage.OriginalDocumentName);

                    if (!extension.Equals(originalExtension, StringComparison.OrdinalIgnoreCase))
                    {
                        // If the extension has changed, manually output the document in order to
                        // ensure it is written into the intended format.
                        copyOriginalDocument = false;
                    }
                }

                if (copyOriginalDocument)
                {
                    // If the document has not been changed from its original form, it can simply be
                    // copied to _fileName rather than require it to be re-assembled.
                    File.Copy(firstPage.OriginalDocumentName, FileName);
                }
                else
                {
                    // Otherwise, generate a new document using the current PageControls as the
                    // document's pages.
                    var imagePages = PageControls
                        .Where(pageControl => !pageControl.Deleted)
                        .Select(pageControl => new ImagePage(pageControl.Page.OriginalDocumentName,
                            pageControl.Page.OriginalPageNumber,
                            pageControl.Page.ImageOrientation));

                    ImageMethods.StaplePagesAsNewDocument(imagePages, FileName);
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
