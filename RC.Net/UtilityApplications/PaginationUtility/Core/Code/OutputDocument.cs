using Extract.Imaging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;

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

        /// <summary>
        /// The <see cref="Page"/>s from <see cref="_originalPages"/> that were originally in a
        /// viewed state.
        /// </summary>
        HashSet<Page> _originalViewedPages = null;

        /// <summary>
        /// The document data that is associated with this instance (VOA file data, for instance).
        /// </summary>
        PaginationDocumentData _documentData;

        /// <summary>
        /// Indicates whether the document should appear collapsed (with only header showing).
        /// </summary>
        bool _collapsed;

        /// <summary>
        /// The file ID.
        /// </summary>
        int _fileID = -1;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="OutputDocument"/> class.
        /// </summary>
        /// <param name="fileName">The filename that the document is to be saved as.</param>
        /// <param name="AutoSelectForReprocess"><c>true</c> if it should be automatically
        /// determined whether to return the document for reprocessing based upon pagination
        /// changes made in the UI; otherwise, <c>false</c>.</param>
        public OutputDocument(string fileName, bool autoSelectForReprocess = false)
        {
            try
            {
                FileName = fileName;
                AutoSelectForReprocess = autoSelectForReprocess;
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

        /// <summary>
        /// Raised to indicate the document has been altered and UI indications may need to change.
        /// </summary>
        public event EventHandler<EventArgs> Invalidated;

        /// <summary>
        /// Raised when the <see cref="DocumentData"/> or page states for this instance have been
        /// modified (currently not raised when pages are added/removed to avoid adding excess
        /// event handling).
        /// </summary>
        public event EventHandler<EventArgs> DocumentStateChanged;

        /// <summary>
        /// The <see cref="PaginationSeparator"/> representing this document in the UI.
        /// </summary>
        private PaginationSeparator _paginationSeparator;

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
        /// Gets the <see cref="PaginationUtility.PageInfo"/>s that represent the source document
        /// pages for the document to output.
        /// </summary>
        public ReadOnlyCollection<PageInfo> SourcePageInfo
        {
            get
            {
                return _pageControls.Select(c => new PageInfo
                    {
                        DocumentName = c.Page.OriginalDocumentName,
                        Page = c.Page.OriginalPageNumber,
                        Deleted = c.Deleted,
                        Orientation = c.Page.ImageOrientation
                    }).ToList().AsReadOnly();
            }
        }

        /// <summary>
        /// The <see cref="PaginationSeparator"/> representing this document in the UI.
        /// </summary>
        public PaginationSeparator PaginationSeparator
        {
            get
            {
                return _paginationSeparator;
            }
            set
            {
                if (value != _paginationSeparator)
                {
                    _paginationSeparator = value;
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
        /// Gets or sets the file ID, or -1 it has both not been assigned and the document is not
        /// currently InSourceDocForm (meaning it will need to be created).
        /// </summary>
        public int FileID
        {
            get
            {
                if (_fileID == -1 && InSourceDocForm)
                {
                    return PageControls.First().Page.SourceDocument?.FileID ?? -1;
                }

                return _fileID;
            }

            set
            {
                _fileID = value;
            }
        }

        /// <summary>
        /// Gets or sets whether the document should appear collapsed (with only header showing).
        /// </summary>
        public bool Collapsed
        {
            get
            {
                return _collapsed;
            }

            set
            {
                try
                {
                    if (value != _collapsed)
                    {
                        _collapsed = value;

                        // Update the separator's state as well
                        // https://extract.atlassian.net/browse/ISSUE-13974
                        if (PaginationSeparator != null)
                        {
                            PaginationSeparator.Collapsed = value;
                        }

                        Control parentControl = null;
                        if (PageControls.Any())
                        {
                            parentControl = PageControls.First().Parent;
                            parentControl.SuspendLayout();
                        }

                        try
                        {
                            foreach (var pageControl in PageControls)
                            {
                                pageControl.Visible = !value;
                            }
                        }
                        finally
                        {
                            if (parentControl != null)
                            {
                                parentControl.ResumeLayout(true);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI40172");
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="OutputDocument"/> is selected
        /// to be committed.
        /// </summary>
        /// <value><see langword="true"/> if selected; otherwise, <see langword="false"/>.
        /// </value>
        public bool Selected
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
                    // If this document has been processed, it may contain merged pages from sources that
                    // can't now be referenced.
                    // Consider processed documents in a original form.
                    if (OutputProcessed)
                    {
                        return true;
                    }

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

                    if (currentPages.Any(page => page.ImageOrientation != page.ProposedOrientation))
                    {
                        return false;
                    }

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
                    // If this document has been processed, it may contain merged pages from sources that
                    // can't now be referenced.
                    // Consider processed documents in source form.
                    if (OutputProcessed)
                    {
                        return true;
                    }

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

                    if (pages.Any(page => page.ImageOrientation != 0))
                    {
                        return false;
                    }

                    var sourceDocPages = pages.First().SourceDocument.Pages;

                    // Ensure the same sequence of and orientation of current pages compared to
                    // sourceDocPages.
                    return Page.PagesAreEqual(pages, sourceDocPages);
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
                    if (_originalPages == null)
                    {
                        return new ReadOnlyCollection<Page>(new Page[0]);
                    }
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
                    if (_originalDeletedPages == null)
                    {
                        return new ReadOnlyCollection<Page>(new Page[0]);
                    }
                    return _originalDeletedPages.ToList().AsReadOnly();
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI40047");
                }
            }
        }

        /// <summary>
        /// Gets the <see cref="Page"/>s that were in a viewed state when
        /// <see cref="SetOriginalForm"/> was called.
        /// </summary>
        public ReadOnlyCollection<Page> OriginalViewedPages
        {
            get
            {
                try
                {
                    if (_originalViewedPages == null)
                    {
                        return new ReadOnlyCollection<Page>(new Page[0]);
                    }
                    return _originalViewedPages.ToList().AsReadOnly();
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI46671");
                }
            }
        }

        /// <summary>
        /// Gets or sets document data that is associated with this instance (VOA file data,
        /// for instance).
        /// </summary>
        /// <value>
        /// The document data that is associated with this instance.
        /// </value>
        public PaginationDocumentData DocumentData
        {
            get
            {
                return _documentData;
            }

            set
            {
                try
                {
                    if (value != _documentData)
                    {
                        if (_documentData != null)
                        {
                            _documentData.DocumentDataStateChanged -= HandleDocumentDataState_Changed;
                        }

                        _documentData = value;

                        if (_documentData != null)
                        {
                            _documentData.DocumentDataStateChanged += HandleDocumentDataState_Changed;
                        }

                        OnDocumentStateChanged();
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI39795");
                }
            }
        }

        /// <summary>
        /// Gets whether the data for this document has been modified by the user.
        /// </summary>
        public bool DataModified
        {
            get
            {
                return (_documentData != null) && _documentData.Modified;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the document's data currently has an error
        /// </summary>
        /// <value><see langword="true"/> if the document data contains an error; otherwise,
        /// <see langword="false"/>.
        /// </value>
        public bool DataError
        {
            get
            {
                return PageControls.Any(c => !c.Deleted) &&
                    (_documentData != null) &&
                    _documentData.AllowDataEdit &&
                    _documentData.DataError;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating the there was pagination suggested for the current
        /// document.
        /// </summary>
        /// <value> <see langword="true"/> if pagination was suggested for the current document;
        /// otherwise, <see langword="false"/>.
        /// </value>
        public bool PaginationSuggested
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a brief summary of the document.
        /// </summary>
        public string Summary
        {
            get
            {
                if (DocumentData == null)
                {
                    return "";
                }
                else
                {
                    return DocumentData.Summary;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether it should be automatically determined whether to
        /// return the document for reprocessing based upon pagination changes made in the UI.
        /// </summary>
        /// <value><c>true</c> reprocessing requirement should be automatically determined;
        /// otherwise, <c>false</c>.
        /// </value>
        public bool AutoSelectForReprocess
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a value indicating whether this document is to be returned for reprocessing.
        /// </summary>
        /// <value> <c>true</c> if the document is to be reprocessed; otherwise, <c>false</c>.
        /// </value>
        public bool SendForReprocessing
        {
            get
            {
                try
                {
                    if (DocumentData?.SendForReprocessing == true)
                    {
                        return true;
                    }
                    else if (AutoSelectForReprocess)
                    {
                        return
                            !InOriginalForm &&
                            PageControls.Any(c => !c.Deleted) &&
                            (DocumentData?.DataSharedInVerification != true);
                    }

                    return false;
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI44682");
                }
            }
        }

        /// <summary>
        /// <c>true</c> if the document has already been processed (output or deleted).
        /// <c>false</c> if the document is still pending for output.
        /// </summary>
        public bool OutputProcessed
        {
            get
            {
                return _documentData?.PaginationRequest != null;
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

                _originalViewedPages = new HashSet<Page>(_pageControls
                    .Where(c => c.Viewed)
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
                    pageControl.PageStateChanged += HandlePageControl_PageStateChanged;
                }

                if (pageIndex < _pageControls.Count)
                {
                    _pageControls.Insert(pageIndex, pageControl);
                }
                else
                {
                    _pageControls.Add(pageControl);
                }

                pageControl.AddStylist(new NewOutputPageStylist(pageControl),
                    replaceExistingTypeInstances: true);

                pageControl.Visible = !_collapsed;

                pageControl.Document = this;

                Invalidate();

                OnDocumentStateChanged();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35552");
            }
        }

        /// <summary>
        /// Handles the PageStateChanged event of the pageControl control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void HandlePageControl_PageStateChanged(object sender, EventArgs e)
        {
            try
            {
                Invalidate();

                OnDocumentStateChanged();
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
                    pageControl.PageStateChanged -= HandlePageControl_PageStateChanged;
                }
    
                pageControl.Document = null;

                Invalidate();

                OnDocumentStateChanged();
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

        /// <summary>
        /// Use to indicate the document has been altered in some way and related UI indications may
        /// need to change.
        /// </summary>
        public void Invalidate()
        {
            try
            {
                foreach (var pageControl in _pageControls)
                {
                    pageControl.Invalidate();
                }

                OnInvalidated();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI40170");
            }
        }

        #endregion Methods

        #region Event Handlers

        /// <summary>
        /// Handles the <see cref="PaginationDocumentData.DocumentDataChanged"/>,
        /// <see cref="PaginationDocumentData.SummaryChanged"/> or
        /// <see cref="PaginationDocumentData.SendForReprocessingChanged"/> events for
        /// <see cref="DocumentData"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleDocumentDataState_Changed(object sender, EventArgs e)
        {
            try
            {
                OnDocumentStateChanged();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39796");
            }
        }

        #endregion Event Handlers

        #region Private Members

        /// <summary>
        /// Raises the <see cref="DocumentOutputting"/> event.
        /// </summary>
        /// <param name="eventArgs">The <see cref="System.ComponentModel.CancelEventArgs"/> instance
        /// containing the event data.</param>
        void OnDocumentOutputting(CancelEventArgs eventArgs)
        {
            DocumentOutputting?.Invoke(this, eventArgs);
        }

        /// <summary>
        /// Raises the <see cref="DocumentOutput"/> event.
        /// </summary>
        void OnDocumentOutput()
        {
            DocumentOutput?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Raises the <see cref="Invalidated"/> event.
        /// </summary>
        void OnInvalidated()
        {
            Invalidated?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Raises the <see cref="DocumentStateChanged"/> event.
        /// <para><b>Note</b></para>
        /// Currently not raised when pages are added/removed to avoid adding excess event handling.
        /// </summary>
        void OnDocumentStateChanged()
        {
            var eventHandler = DocumentStateChanged;
            if (eventHandler != null)
            {
                eventHandler(this, new EventArgs());
            }
        }


        #endregion Private Members
    }
        
}
