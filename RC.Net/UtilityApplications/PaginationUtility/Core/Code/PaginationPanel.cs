using Extract.Imaging;
using Extract.Imaging.Forms;
using Extract.Utilities;
using Extract.Utilities.Forms;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Extract.UtilityApplications.PaginationUtility
{
    /// <summary>
    /// Pagination functionality encapsulated in a panel to allow for pagination as part of an
    /// application other than PaginationUtility.exe.
    /// </summary>
    public partial class PaginationPanel : UserControl, IPaginationUtility, IImageViewerControl
    {
        #region Fields

        /// <summary>
        /// The <see cref="PageLayoutControl"/> into which input documents are initially loaded.
        /// </summary>
        PageLayoutControl _primaryPageLayoutControl;

        /// <summary>
        /// The <see cref="IDocumentDataPanel"/> used to display document specific fields
        /// corresponding to the currently selected document.
        /// </summary>
        IDocumentDataPanel _documentDataPanel;

        /// <summary>
        /// All <see cref="SourceDocument"/>s currently active in the UI in the order they appeared
        /// before any user edits.
        /// </summary>
        List<SourceDocument> _sourceDocuments = new List<SourceDocument>();

        /// <summary>
        /// Synchronizes access to <see cref="_sourceDocuments"/>
        /// </summary>
        object _sourceDocumentLock = new object();

        /// <summary>
        /// The set of all <see cref="ExtendedOutputDocument"/>s currently active in the UI.
        /// </summary>
        ObservableCollection<ExtendedOutputDocument> _pendingDocuments =
            new ObservableCollection<ExtendedOutputDocument>();

        /// <summary>
        /// The <see cref="ExtendedOutputDocument"/>s that were originally present as a result of
        /// the last LoadFile or RemoveSourceFile operation.
        /// </summary>
        List<ExtendedOutputDocument> _originalDocuments = new List<ExtendedOutputDocument>();

        /// <summary>
        /// All <see cref="ExtendedOutputDocument"/> that relate to each
        /// <see cref="SourceDocument"/>.
        /// </summary>
        Dictionary<SourceDocument, HashSet<ExtendedOutputDocument>> _sourceToOriginalDocuments =
            new Dictionary<SourceDocument, HashSet<ExtendedOutputDocument>>();

        /// <summary>
        /// Maintains a temporary files to which <see cref="OutputDocument"/>s should be written
        /// until the owner of this panel has the opportunity to determine where it is going.
        /// </summary>
        Dictionary<ExtendedOutputDocument, TemporaryFile> _tempFiles =
            new Dictionary<ExtendedOutputDocument, TemporaryFile>();

        /// <summary>
        /// Keeps track of the names of documents that have been output as part of the current
        /// <see cref="CommitPendingChanges"/> call.
        /// </summary>
        List<string> _outputDocumentNames = new List<string>();

        /// <summary>
        /// While in the process of committing pagination output, keeps track of the control index
        /// of the first page in the panel for each document being output. This allows the new
        /// document to be loaded into the same position in the panel via the position parameter
        /// of LoadFile.
        /// </summary>
        Dictionary<OutputDocument, int> _outputDocumentPositions;

        /// <summary>
        /// Keeps track of whether pagination is suggested for a particular source file name and the
        /// VOA file data that needs to be assigned.
        /// </summary>
        Dictionary<string, Tuple<bool, IDocumentData>> _documentData =
            new Dictionary<string, Tuple<bool, IDocumentData>>();

        /// <summary>
        /// Indicates the actively selected document. (the document reflected in
        /// <see cref="DocumentDataPanel"/>)
        /// </summary>
        ExtendedOutputDocument _activeDocument;

        /// <summary>
        /// Indicates a document for which data could not be successfully saved. This data will need
        /// to be corrected before doing anything else.
        /// <see cref="DocumentDataPanel"/>)
        /// </summary>
        ExtendedOutputDocument _invalidDocumentData;        
        
        /// <summary>
        /// Indicated a manually overridden value for <see cref="PendingChanges"/> or
        /// <see langword="null"/> if there is no manually overridden value
        /// (<see cref="PendingChanges"/> will return a calculated value).
        /// </summary>
        bool? _pendingChangesOverride;

        /// <summary>
        /// Indicates whether pending pagination changes are in the process of being committed.
        /// </summary>
        bool _commitingChanges;

        /// <summary>
        /// The <see cref="ImageViewer"/> to be used by this instance.
        /// </summary>
        ImageViewer _imageViewer;

        /// <summary>
        /// The <see cref="ShortcutsManager"/> managing all keyboard shortcuts for this instance.
        /// </summary>
        ShortcutsManager _shortcuts = new ShortcutsManager();

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PaginationPanel"/> class.
        /// </summary>
        public PaginationPanel()
        {
            try
            {
                InitializeComponent();

                _pendingDocuments.CollectionChanged += HandlePendingDocuments_CollectionChanged;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39554");
            }
        }

        #endregion Constructors

        #region Events

        /// <summary>
        /// Raised when a new prospective document has been defined in the panel and
        /// <see cref="IDocumentData"/> should be assigned if needed.
        /// </summary>
        public event EventHandler<DocumentDataRequestEventArgs> DocumentDataRequest;

        /// <summary>
        /// Raised when the Load Next Document button is pressed.
        /// </summary>
        public event EventHandler<EventArgs> LoadNextDocument;

        /// <summary>
        /// Raised when a newly paginated document is generated.
        /// </summary>
        public event EventHandler<CreatingOutputDocumentEventArgs> CreatingOutputDocument;

        /// <summary>
        /// Raised when a pagination operation is complete. May follow multiple
        /// <see cref="CreatingOutputDocument"/> events if a single pagination event produced
        /// multiple documents.
        /// </summary>
        public event EventHandler<PaginatedEventArgs> Paginated;

        /// <summary>
        /// Raised when the state of any pages, documents or data has changed.
        /// </summary>
        public event EventHandler<EventArgs> StateChanged;

        #endregion Events

        #region Properties

        /// <summary>
        /// Gets a value indicating whether there are any uncommitted changes to document pagination.
        /// </summary>
        /// <value><see langword="true"/> if there are uncommitted changes to document pagination;
        /// otherwise, <see langword="false"/>.
        /// </value>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public bool PendingChanges
        {
            get
            {
                try
                {
                    // If in the process of committing changes, don't consider changes as pending.
                    if (_commitingChanges)
                    {
                        return false;
                    }
                    // If a PendingChanges status has been manually applied, use that value rather
                    // than calculating whether there have been changes.
                    else if (_pendingChangesOverride.HasValue)
                    {
                        return _pendingChangesOverride.Value;
                    }
                    else
                    {
                        return (_pendingDocuments != null) &&
                            _pendingDocuments.Any(document => !document.InOriginalForm ||
                                (document.DocumentData != null && document.DocumentData.Modified));
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI39676");
                }
            }

            set
            {
                // Allow the calculated value of PendingChanges to be overridden.
                _pendingChangesOverride = value;
            }
        }

        /// <summary>
        /// Gets the filename of all source documents currently active in the UI in the order they
        /// appeared before any user edits.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public ReadOnlyCollection<string> SourceDocuments
        {
            get
            {
                try
                {
                    return _sourceDocuments
                        .Select(doc => doc.FileName)
                        .ToList()
                        .AsReadOnly();
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI39596");
                }
            }
        }

        /// <summary>
        /// Gets or sets whether the primarily selected <see cref="PageThumbnailControl"/> should
        /// update the displayed page in the <see cref="ImageViewer"/>.
        /// </summary>
        /// <value><see langword="true"/> if the primarily selected page should update the displayed
        /// page in the <see cref="ImageViewer"/>; otherwise, <see langword="false"/>.</value>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public bool EnablePageDisplay
        {
            get
            {
                return _primaryPageLayoutControl.EnablePageDisplay;
            }

            set
            {
                _primaryPageLayoutControl.EnablePageDisplay = value;
            }
        }

        /// <summary>
        /// Gets whether pagination has been suggested for the specified
        /// <see paramref="sourceFileName"/>.
        /// </summary>
        /// <param name="sourceFileName">Name of the source file to which this query relates.</param>
        /// <returns><see langword="true"/> if pagination has been suggested; otherwise, 
        /// <see langword="false"/>.
        /// </returns>
        public bool IsPaginationSuggested(string sourceFileName)
        {
            try
            {
                return _primaryPageLayoutControl.Documents
                    .Cast<ExtendedOutputDocument>()
                    .Where(doc => doc.PageControls.Any(
                        c => c.Page.OriginalDocumentName == sourceFileName))
                    .Any(doc => doc.PaginationSuggested);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39665");
            }
        }

        /// <summary>
        /// Gets whether the pagination pane depicts the document in the same form it exists on disk.
        /// <see paramref="sourceFileName"/>.
        /// </summary>
        /// <param name="sourceFileName">Name of the source file to which this query relates.</param>
        /// <returns><see langword="true"/> if the document is unmodified compared to how it exists
        /// on disk; otherwise, <see langword="false"/>.
        /// </returns>
        public bool IsInOriginalForm(string sourceFileName)
        {
            try
            {
                var matchingDocuments = _primaryPageLayoutControl.Documents
                    .Where(doc => doc.InOriginalForm &&
                        doc.PageControls.First().Page.OriginalDocumentName.Equals(sourceFileName,
                            StringComparison.OrdinalIgnoreCase));

                return (matchingDocuments.Count() == 1) &&
                    matchingDocuments.Single().InOriginalForm;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39816");
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="IDocumentDataPanel"/> to display and allow editing
        /// of data associated with any singly selected document or <see langword="null"/> if no
        /// such panel is available.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public IDocumentDataPanel DocumentDataPanel
        {
            get
            {
                return _documentDataPanel;
            }

            set
            {
                try
                {
                    if (value != _documentDataPanel)
                    {
                        if (_documentDataPanel != null)
                        {
                            _tableLayoutPanel.Controls.Remove(_documentDataPanel.Control);
                        }

                        _documentDataPanel = value;

                        if (_documentDataPanel != null)
                        {
                            _tableLayoutPanel.Controls.Add(_documentDataPanel.Control, 0, 0);
                            _documentDataPanel.Control.Dock = DockStyle.Fill;
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI39716");
                }
            }
        }

        /// <summary>
        /// Gets the fully selected source documents. These are documents that are in their source
        /// doc forms.
        /// </summary>
        public ReadOnlyCollection<string> FullySelectedSourceDocuments
        {
            get
            {
                try
                {
                    var fullySelectedDocuments = _primaryPageLayoutControl
                        .FullySelectedDocuments
                        .Where(doc => doc.InSourceDocForm)
                        .ToArray();
                    var partiallySelectedDocuments = _primaryPageLayoutControl
                        .PartiallySelectedDocuments
                        .ToArray();
                    if (!fullySelectedDocuments.SequenceEqual(partiallySelectedDocuments))
                    {
                        return new List<string>().AsReadOnly();
                    }

                    return fullySelectedDocuments
                        .Select(doc =>
                            doc.PageControls
                                .First()
                                .Page.SourceDocument.FileName)
                        .ToList()
                        .AsReadOnly();
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI40148");
                }
            }
        }

        /// <summary>
        /// Gets or sets whether the panel's toolbar button should be displayed.
        /// </summary>
        public bool ToolBarVisible
        {
            get
            {
                return _toolStripContainer.TopToolStripPanelVisible;
            }

            set
            {
                _toolStripContainer.TopToolStripPanelVisible = value;
            }
        }

        /// <summary>
        /// Gets whether there are pending changes available for <see cref="CommitPendingChanges"/>.
        /// </summary>
        public bool CommitEnabled
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets whether there are any documents that differ from their original loaded form
        /// available for <see cref="RevertPendingChanges"/>.
        /// </summary>
        public bool RevertToOriginalEnabled
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets whether there are any documents that differ from their source form available for
        /// available for <see cref="RevertPendingChanges"/>.
        /// </summary>
        public bool RevertToSourceEnabled
        {
            get;
            private set;
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Loads the specified documents pages into the pane.
        /// </summary>
        /// <param name="fileName">Name of the file to load.</param>
        /// <param name="position">The position at which a document should be loaded. 0 = Load at
        /// the front (top), -1 = load at the end (bottom). Any other value should be a value
        /// passed via <see cref="CreatingOutputDocumentEventArgs"/> and not a value the caller
        /// should expect to be able to calculate.</param>
        public void LoadFile(string fileName, int position)
        {
            LoadFile(fileName, position, null, false, null);
        }

        /// <summary>
        /// Loads the specified documents pages into the pane.
        /// </summary>
        /// <param name="fileName">Name of the file to load.</param>
        /// <param name="position">The position at which a document should be loaded. 0 = Load at
        /// the front (top), -1 = load at the end (bottom). Any other value should be a value
        /// passed via <see cref="CreatingOutputDocumentEventArgs"/> and not a value the caller
        /// should expect to be able to calculate.</param>
        /// <param name="pages">The page numbers from <see paramref="fileName"/> to be loaded or
        /// <see langword="null"/> to load all pages.</param>
        /// <param name="paginationSuggested"><see langword="true"/> if pagination has been
        /// suggested for this document; <see langword="false"/> if it has not been.</param>
        /// <param name="documentData">The VOA file data associated with <see paramref="fileName"/>.
        /// </param>
        public void LoadFile(string fileName, int position, IEnumerable<int> pages,
            bool paginationSuggested, IDocumentData documentData)
        {
            try
            {
                FormsMethods.ExecuteInUIThread(this, () =>
                {
                    // The ExtendedOutputDocument doesn't get created directly by this method. Use
                    // _documentData to be able to pass this info to when it is needed in
                    // IPaginationUtility.CreateOutputDocument.
                    if (paginationSuggested || documentData != null)
                    {
                        _documentData[fileName] = new Tuple<bool, IDocumentData>(
                            paginationSuggested, documentData);
                    }

                    var sourceDocument = OpenDocument(fileName);

                    if (sourceDocument != null)
                    {
                        // This will call Application.DoEvents in the midst of loading a document to
                        // keep the UI responsive as pages are loaded. This allows an opportunity
                        // for there to be multiple calls into LoadNextDocument at the same time.
                        var outputDocument = 
                            (ExtendedOutputDocument)_primaryPageLayoutControl.CreateOutputDocument(
                                sourceDocument, pages, position, true);

                        _originalDocuments.Add(outputDocument);
                        var setOutputDocs = _sourceToOriginalDocuments.GetOrAdd(
                            sourceDocument, () => new HashSet<ExtendedOutputDocument>());
                        setOutputDocs.Add(outputDocument);
                    }

                    ApplyOrderOfLoadedSourceDocuments();
                });
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39555");
            }
            finally
            {
                _documentData.Remove(fileName);
            }
        }

        /// <summary>
        /// Updates the data associated with the specified <see paramref="sourceFileName"/>.
        /// <para><b>Note:</b></para>
        /// The document's data can be updated only in the case that the currently represented
        /// pagination does not differ at all with how this document currently exists on disk.
        /// </summary>
        /// <param name="sourceFileName">The name of the document for</param>
        /// <param name="documentData">The data to associate with <see paramref="sourceFileName"/>.
        /// (replaces any previously assigned data)</param>
        /// <returns><see langword="true"/> if the document data was updated,
        /// <see langword="false"/> if it was not because the represented pagination differs with
        /// the document on disk.</returns>
        public bool UpdateDocumentData(string sourceFileName, IDocumentData documentData)
        {
            try
            {
                var matchingDocuments = _primaryPageLayoutControl.Documents
                    .Where(doc => doc.InOriginalForm &&
                        doc.PageControls.First().Page.OriginalDocumentName.Equals(sourceFileName,
                            StringComparison.OrdinalIgnoreCase));

                if (matchingDocuments.Count() == 1)
                {
                    var document = (ExtendedOutputDocument)matchingDocuments.Single();
                    UpdateDocumentData(document, documentData);

                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39720");
            }
        }

        /// <summary>
        /// Redefines the current state of all documents as the original form.
        /// </summary>
        public void SetOriginalDocumentForm()
        {
            try
            {
                _pendingChangesOverride = null;

                foreach (var document in _pendingDocuments)
                {
                    document.SetOriginalForm();
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI40149");
            }
        }

        /// <summary>
        /// Commits any pending document pagination changes.
        /// </summary>
        /// <returns><see langword="true"/> if the currently processing file needs to be released
        /// following the next call to delay file; <see langword="false"/> if the currently
        /// processing file should remain in processing.</returns>
        public void CommitPendingChanges()
        {
            LockControlUpdates controlUpdateLock = null;

            try
            {
                // If document data could not be saved, abort the commit operation.
                if (_invalidDocumentData != null || !SaveActiveDocumentData())
                {
                    return;
                }

                _commitingChanges = true;

                // Prevent the UI from trying to load/close pages in the image viewer as the
                // operation is taking place.
                _primaryPageLayoutControl.ClearSelection();
                EnablePageDisplay = false;
                controlUpdateLock = new LockControlUpdates(_primaryPageLayoutControl);

                _outputDocumentNames.Clear();

                // Depending upon the manipulations that occurred, there may be some pending
                // documents that have been left without any pages. Disregard these.
                var documentsToRemove = _pendingDocuments
                    .Where(doc => !doc.PageControls.Any(c => !c.Deleted))
                    .ToArray();
                foreach (var document in documentsToRemove)
                {
                    _pendingDocuments.Remove(document);
                }

                var outputDocuments = _pendingDocuments
                    .Where(document => !document.InSourceDocForm)
                    .ToList();

                _outputDocumentPositions = _primaryPageLayoutControl.Documents
                    .ToDictionary(
                        doc => doc, doc => _primaryPageLayoutControl.GetDocumentPosition(doc));

                var sourceDocuments =
                    outputDocuments.SelectMany(doc => doc.PageControls.Where(c => !c.Deleted))
                        .Select(c => c.Page.SourceDocument)
                        .Distinct()
                        .ToArray();

                // Generate the paginated output. _outputDocumentNames will maintain the names files
                // that have been output.
                foreach (var outputDocument in outputDocuments)
                {
                    outputDocument.Output();
                    _pendingDocuments.Remove(outputDocument);
                }

                var disregardedPagination = _pendingDocuments
                    .Where(doc => doc.InSourceDocForm &&
                        doc.PaginationSuggested &&
                        !doc.InOriginalForm)
                    .Select(doc => doc.PageControls.First().Page.OriginalDocumentName);

                var sourcesWithModifiedData = _pendingDocuments
                    .Where(doc => doc.InSourceDocForm &&
                        doc.DocumentData != null && doc.DocumentData.Modified)
                    .Select(doc => new KeyValuePair<string, IDocumentData>(
                        doc.PageControls.First().Page.OriginalDocumentName, doc.DocumentData));

                OnPaginated(sourceDocuments.Select(doc => doc.FileName),
                    disregardedPagination,
                    sourcesWithModifiedData);

                foreach (var document in _pendingDocuments)
                {
                    document.PaginationSuggested = false;
                    document.SetOriginalForm();
                    foreach (var pageControl in document.PageControls)
                    {
                        pageControl.Invalidate();
                    }
                }

                ApplyOrderOfLoadedSourceDocuments();

                _pendingChangesOverride = null;

                UpdateCommandStates();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39571");
            }
            finally
            {
                if (controlUpdateLock != null)
                {
                    controlUpdateLock.Dispose();
                }
                EnablePageDisplay = true;
                _commitingChanges = false;
                _outputDocumentPositions.Clear();
            }
        }

        /// <summary>
        /// Reverts any uncommitted document pagination changes.
        /// </summary>
        /// <param name="revertToSource"><see langword="true"/> to revert to the state of the source
        /// documents on disk; <see langword="false"/> to revert to the state that was originally
        /// presented (will differ from source state if pagination was suggested).</param>
        public void RevertPendingChanges(bool revertToSource)
        {
            LockControlUpdates controlUpdateLock = null;

            try
            {
                // Prevent the UI from trying to load/close pages in the image viewer as the
                // operation is taking place.
                _primaryPageLayoutControl.ClearSelection();
                controlUpdateLock = new LockControlUpdates(_primaryPageLayoutControl);
                EnablePageDisplay = false;

                foreach (var outputDocument in _pendingDocuments.ToArray())
                {
                    _primaryPageLayoutControl.DeleteOutputDocument(outputDocument);
                    _pendingDocuments.Remove(outputDocument);
                }

                SourceDocument[] sourceDocArray;
                lock (_sourceDocumentLock)
                {
                    sourceDocArray = _sourceDocuments.ToArray();
                }

                if (revertToSource)
                {
                    foreach (SourceDocument sourceDocument in sourceDocArray)
                    {
                        // Reverting a document for which there was not suggested pagination
                        HashSet<ExtendedOutputDocument> outputDocuments = null;
                        if (_sourceToOriginalDocuments.TryGetValue(sourceDocument, out outputDocuments) &&
                            outputDocuments.Count == 1)
                        {
                            var outputDocument = outputDocuments.Single();
                            _primaryPageLayoutControl.LoadOutputDocument(
                                outputDocument, outputDocument.OriginalPages, null);
                            if (outputDocument.DocumentData != null)
                            {
                                outputDocument.DocumentData.Revert();
                            }
                            _pendingDocuments.Add(outputDocument);
                        }
                        // Reverting a document for which there was suggested pagination. Rather than
                        // reverting document data, a new document data instance will be needed.
                        else
                        {
                            // CreateOutputDocument will add the document to _pendingDocuments.
                            var outputDocument = (ExtendedOutputDocument)
                                ((IPaginationUtility)this).CreateOutputDocument(sourceDocument.FileName);
                            outputDocument.SetOriginalForm();
                            outputDocument.PaginationSuggested = true;

                            var args = new DocumentDataRequestEventArgs(sourceDocument.FileName);
                            OnDocumentDataRequest(args);
                            outputDocument.DocumentData = args.DocumentData;

                            _primaryPageLayoutControl.LoadOutputDocument(
                                outputDocument, sourceDocument.Pages, null);
                        }
                    }
                }
                else
                {
                    foreach (var outputDocument in sourceDocArray
                        .SelectMany(source => _sourceToOriginalDocuments[source]
                            .OrderBy(doc => doc.OriginalPages.Select(
                                page => page.OriginalPageNumber).Min())))
                    {
                        _primaryPageLayoutControl.LoadOutputDocument(outputDocument,
                            outputDocument.OriginalPages, outputDocument.OriginalDeletedPages);
                        if (!_pendingDocuments.Contains(outputDocument))
                        {
                            _pendingDocuments.Add(outputDocument);
                        }

                        if (outputDocument.DocumentData != null)
                        {
                            outputDocument.DocumentData.Revert();
                        }
                    }
                }

                _primaryPageLayoutControl.SelectFirstPage();
                _primaryPageLayoutControl.Focus();

                UpdateCommandStates();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI40018");
            }
            finally
            {
                if (controlUpdateLock != null)
                {
                    controlUpdateLock.Dispose();
                }
                EnablePageDisplay = true;
            }
        }

        /// <summary>
        /// Removes any pages or pending output documents associated with the specified
        /// <see paramref="fileName"/> from the panel.
        /// </summary>
        /// <param name="fileName">The name of the file for which pages should be removed.</param>
        /// <returns>Returns the index at which the document existed.
        /// <para><b>Note</b></para>
        /// This is not a document index. The caller should not try to interpret this value;
        /// it's use should be limited to passing as the position argument of
        /// PaginationPanel.LoadFile or SelectDocumentAtPosition.
        /// </returns>
        public int RemoveSourceFile(string fileName)
        {
            try
            {
                int position = -1;

                FormsMethods.ExecuteInUIThread(this, () =>
                {
                    lock (_sourceDocumentLock)
                    {
                        var sourceDocument = _sourceDocuments.Single(doc => doc.FileName == fileName);

                        var documentsToDelete = _pendingDocuments
                            .Where(doc =>
                                doc.PageControls.Any(c =>
                                    c.Page.SourceDocument == sourceDocument))
                            .ToArray();

                        foreach (var outputDocument in documentsToDelete)
                        {
                            int docPosition =
                                _primaryPageLayoutControl.DeleteOutputDocument(outputDocument);
                            if (docPosition != -1)
                            {
                                position = (position == -1)
                                    ? docPosition
                                    : Math.Min(position, docPosition);
                            }
                            _pendingDocuments.Remove(outputDocument);
                        }

                        var referencedOriginalDocuments = _originalDocuments
                            .Where(doc => doc.OriginalPages.Any(page => page.OriginalDocumentName == fileName))
                            .ToArray();

                        foreach (var outputDocument in referencedOriginalDocuments)
                        {
                            _originalDocuments.Remove(outputDocument);
                        }

                        _sourceDocuments.Remove(sourceDocument);
                        _sourceToOriginalDocuments.Remove(sourceDocument);
                    }

                    ApplyOrderOfLoadedSourceDocuments();
                });

                return position;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39556");
            }
        }

        /// <summary>
        /// Selects the document currently as <see paramref="position"/>.
        /// </summary>
        /// <param name="position">This value should be a value retrieved via
        /// <see cref="PageLayoutControl.GetDocumentPosition"/> and not a value the caller should
        /// expect to be able to calculate.</param>
        public bool SelectDocumentAtPosition(int position)
        {
            try
            {
                return _primaryPageLayoutControl.SelectDocumentAtPosition(position);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI40150");
            }
        }

        /// <summary>
        /// Prepares for application focus to be moved to another control (such as verification) by
        /// saving pending data and disabling interaction with the <see cref="ImageViewer"/>.
        /// </summary>
        public void Park()
        {
            try
            {
                SaveActiveDocumentData();
                EnablePageDisplay = false;
                _primaryPageLayoutControl.ClearSelection();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39787");
            }
        }

        /// <summary>
        /// Prepares for application focus to be given to this panel by enabling interaction with
        /// the <see cref="ImageViewer"/>.
        /// </summary>
        public void Resume()
        {
            try
            {
                EnablePageDisplay = true;
                _primaryPageLayoutControl.SelectFirstPage();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39788");
            }
        }

        #endregion Methods

        #region IPaginationUtility

        /// <summary>
        /// Creates a new <see cref="OutputDocument"/> based on the specified
        /// <see paramref="originalDocName"/>.
        /// </summary>
        /// <param name="originalDocName">The name that should be used as the basis for the new
        /// document's filename.</param>
        /// <returns>
        /// The new <see cref="OutputDocument"/>.
        /// </returns>
        OutputDocument IPaginationUtility.CreateOutputDocument(string originalDocName)
        {
            try
            {
                string outputDocumentName = GenerateOutputDocumentName(originalDocName);
                var outputDocument = new ExtendedOutputDocument(outputDocumentName);
                outputDocument.DocumentOutputting += HandleOutputDocument_DocumentOutputting;
                outputDocument.DocumentOutput += HandleOutputDocument_DocumentOutput;

                Tuple<bool, IDocumentData> documentData;
                if (_documentData.TryGetValue(originalDocName, out documentData))
                {
                    outputDocument.PaginationSuggested = documentData.Item1;
                    outputDocument.DocumentData = documentData.Item2;
                    if (outputDocument.DocumentData != null)
                    {
                        outputDocument.DocumentData.ModifiedChanged += DocumentData_ModifiedChanged;
                    }
                }
                else
                {
                    var args = new DocumentDataRequestEventArgs(originalDocName);
                    OnDocumentDataRequest(args);
                    ((ExtendedOutputDocument)outputDocument).DocumentData = args.DocumentData;
                }

                _pendingDocuments.Add(outputDocument);

                return outputDocument;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39558");
            }
        }

        /// <summary>
        /// Generates a new name an <see cref="OutputDocument"/> based on the specified
        /// <see paramref="originalDocName"/> that is unique compared to any pending output
        /// document, any existing document or any document that has been output by this instance
        /// whether or not the file still exists.
        /// </summary>
        /// <param name="originalDocName">The filename that should serve as the basis for the new
        /// document name.</param>
        /// <returns>The unique document name.</returns>
        public string GenerateOutputDocumentName(string originalDocName)
        {
            try
            {
                string baseFilename = originalDocName;
                string directory = Path.GetDirectoryName(baseFilename);
                string extension = Path.GetExtension(baseFilename);
                baseFilename = Path.GetFileNameWithoutExtension(baseFilename);

                return Path.Combine(directory, baseFilename + "_" +
                    UtilityMethods.GetRandomString(10, true, false, true) +
                    extension);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39559");
            }
        }

        /// <summary>
        /// Gets the <see cref="Page"/> instance(s) that represent the page(s) of the specified
        /// <see paramref="fileName"/>.
        /// </summary>
        /// <param name="fileName">The image file for which <see cref="Page"/> instance(s) should be
        /// retrieved.</param>
        /// <param name="pageNumber">If <see langword="null"/>, all pages will be retrieved;
        /// otherwise only the specified page will be retrieved.</param>
        /// <returns>The <see cref="Page"/> instance(s) that represent the page(s) of the specified
        /// <see paramref="fileName"/>.</returns>
        IEnumerable<Page> IPaginationUtility.GetDocumentPages(string fileName, int? pageNumber)
        {
            // See if the document indicated is already open as a SourceDocument.
            SourceDocument document = null;

            lock (_sourceDocumentLock)
            {
                document = _sourceDocuments
                    .Where(doc => doc.FileName.Equals(fileName, StringComparison.OrdinalIgnoreCase))
                    .SingleOrDefault();
            }

            // If not, open it as a SourceDocument.
            if (document == null)
            {
                document = OpenDocument(fileName);
            }

            // If unable to open then document, don't throw an exception, just act as though
            // the data was not on the clipboard in the first place.
            if (document == null)
            {
                yield break;
            }

            ExtractException.Assert("ELI39560", "Cannot find source page(s).",
                document != null && (pageNumber == null || document.Pages.Count >= pageNumber),
                "Filename", fileName,
                "Page number", pageNumber);

            // Return the correct page from the SourceDocument.
            if (pageNumber != null)
            {
                yield return document.Pages[pageNumber.Value - 1];
            }
            else
            {
                foreach (Page page in document.Pages)
                {
                    yield return page;
                }
            }
        }

        /// <summary>
        /// A <see cref="ToolStripItem"/> intended to trigger a cut operation or
        /// <see langword="null"/> if no such item is available.
        /// </summary>
        public ToolStripItem CutMenuItem
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// A <see cref="ToolStripItem"/> intended to trigger a copy operation or 
        /// <see langword="null"/> if no such item is available.
        /// </summary>
        public ToolStripItem CopyMenuItem
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// A <see cref="ToolStripItem"/> intended to trigger a insert copied data operation or
        /// <see langword="null"/> if no such item is available.
        /// </summary>
        public ToolStripItem PasteMenuItem
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// A <see cref="ToolStripItem"/> intended to trigger a delete operation or
        /// <see langword="null"/> if no such item is available.
        /// </summary>
        public ToolStripItem DeleteMenuItem
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// A <see cref="ToolStripItem"/> intended to trigger an un-delete operation or
        /// <see langword="null"/> if no such item is available.
        /// </summary>
        public ToolStripItem UnDeleteMenuItem
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// A <see cref="ToolStripItem"/> intended to trigger a print operation or
        /// <see langword="null"/> if no such item is available.
        /// </summary>
        public ToolStripItem PrintMenuItem
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// A <see cref="ToolStripItem"/> intended to toggle a document separator ahead of the
        /// currently selected page or <see langword="null"/> if no such item is available.
        /// </summary>
        public ToolStripItem ToggleDocumentSeparatorMenuItem
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// A <see cref="ToolStripButton"/> intended to trigger output the currently selected
        /// document(s) or <see langword="null"/> if no such item is available.
        /// </summary>
        public ToolStripButton OutputDocumentToolStripButton
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// A <see cref="ToolStripItem"/> intended to trigger output the currently selected
        /// document(s) or <see langword="null"/> if no such item is available.
        /// </summary>
        public ToolStripMenuItem OutputSelectedDocumentsMenuItem
        {
            get
            {
                return null;
            }
        }

        #endregion IPaginationUtility

        #region IImageViewerControl Members

        /// <summary>
        /// Gets or sets the <see cref="ImageViewer"/> this instance should use.
        /// </summary>
        /// <value>The <see cref="ImageViewer"/> this instance should use.</value>
        [Browsable(false)]
        public ImageViewer ImageViewer
        {
            get
            {
                return _imageViewer;
            }
            set
            {
                _imageViewer = value;
            }
        }

        #endregion IImageViewerControl Members

        #region Overrides

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Form.Load"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.
        /// </param>
        protected override void OnLoad(EventArgs e)
        {
            try
            {
                base.OnLoad(e);

                if (_imageViewer != null)
                {
                    _imageViewer.EstablishConnections(this);
                }

                ResetPrimaryPageLayoutControl();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI39561");
            }
        }

        /// <summary>
        /// Processes a command key.
        /// </summary>
        /// <param name="msg">The window message to process.</param>
        /// <param name="keyData">The key to process.</param>
        /// <returns><see langword="true"/> if the character was processed by the control; 
        /// <see langword="false"/> if the character was not processed.</returns>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            try
            {
                if (_shortcuts.ProcessKey(keyData))
                {
                    return true;
                }

                // This key was not processed, bubble it up to the base class.
                return base.ProcessCmdKey(ref msg, keyData);
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI39562", ex);
                return false;
            }
        }

        #endregion Overrides

        #region Event Handlers

        /// <summary>
        /// Handles the <see cref="OutputDocument.DocumentOutputting"/> event.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.ComponentModel.CancelEventArgs"/> instance
        /// containing the event data.</param>
        void HandleOutputDocument_DocumentOutputting(object sender, CancelEventArgs e)
        {
            try
            {
                var outputDocument = (ExtendedOutputDocument)sender;

                // Don't output the document to its final location at first since a running file
                // supplier would be likely to queue it before this process has a chance to grab it.
                // Only after the file has been added to the DB and checked out for processing by
                // this instance will it be moved to its final location.
                var tempFile = new TemporaryFile(true);
                _tempFiles[outputDocument] = tempFile;

                outputDocument.FileName = tempFile.FileName;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39564");
            }
        }

        /// <summary>
        /// Handles the <see cref="OutputDocument.DocumentOutput"/> event.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void HandleOutputDocument_DocumentOutput(object sender, EventArgs e)
        {
            ExtendedOutputDocument outputDocument = null;
            TemporaryFile tempFile = null;

            try
            {
                // The file has been output at this point according to the PageLayoutControl, but
                // this is actually only to a temporary in order to give the owner of this panel the
                // opportunity to determine where it is going before the file is written to the
                // final location.
                outputDocument = (ExtendedOutputDocument)sender;
                tempFile = _tempFiles[outputDocument];

                var sourcePageInfo = outputDocument.PageControls
                    .Select(c => new PageInfo()
                    {
                        DocumentName = c.Page.OriginalDocumentName,
                        Page = c.Page.OriginalPageNumber
                    });

                int pageCount = 0;
                using (var codecs = new ImageCodecs())
                using (var reader = codecs.CreateReader(tempFile.FileName))
                {
                    pageCount = reader.PageCount;
                }
                long fileSize = new FileInfo(tempFile.FileName).Length;

                bool? suggestedPaginationAccepted = outputDocument.PaginationSuggested
                    ? (bool ?)(outputDocument.InOriginalForm ? true : false)
                    : null;

                int position = _outputDocumentPositions[outputDocument];

                var eventArgs = new CreatingOutputDocumentEventArgs(
                    sourcePageInfo, pageCount, fileSize, suggestedPaginationAccepted, position,
                    outputDocument.DocumentData);
                OnCreatingOutputDocument(eventArgs);
                string outputFileName = eventArgs.OutputFileName;

                ExtractException.Assert("ELI39588",
                    "No filename has been specified for pagination output.",
                    !string.IsNullOrWhiteSpace(outputFileName));

                File.Copy(tempFile.FileName, outputFileName);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39565");
            }
            finally
            {
                if (tempFile != null)
                {
                    _tempFiles.Remove(outputDocument);
                    tempFile.Dispose();
                }
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event of the <see cref="_applyToolStripButton"/>
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleApplyToolStripButton_Click(object sender, EventArgs e)
        {
            try
            {
                CommitPendingChanges();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI39566");
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event of the
        /// <see cref="_revertToOriginalToolStripButton"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleRevertToOriginalToolStripButton_Click(object sender, EventArgs e)
        {
            try
            {
                RevertPendingChanges(revertToSource: false);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI39664");
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event of the
        /// <see cref="_revertToSourceToolStripButton"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleRevertToSourceToolStripButton_Click(object sender, EventArgs e)
        {
            try
            {
                RevertPendingChanges(revertToSource: true);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI39567");
            }
        }

        /// <summary>
        /// Handles the <see cref="PageLayoutControl.StateChanged"/> event.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandlePageLayoutControl_StateChanged(object sender, EventArgs e)
        {
            try
            {
                UpdateCommandStates();

                OnStateChanged();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39568");
            }
        }

        /// <summary>
        /// Handles the <see cref="IDocumentData.ModifiedChanged"/> event for the document data of
        /// any of the <see cref="_pendingDocuments"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void DocumentData_ModifiedChanged(object sender, EventArgs e)
        {
            try
            {
                UpdateCommandStates();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39771");
            }
        }

        /// <summary>
        /// Handles the <see cref="PageLayoutControl.SelectionChanged"/> event.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandlePageLayoutControl_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                ExtendedOutputDocument newActiveDocument = null;
                if (_primaryPageLayoutControl.PartiallySelectedDocuments.Count() == 1)
                {
                    newActiveDocument = (ExtendedOutputDocument)
                        _primaryPageLayoutControl.PartiallySelectedDocuments.Single();
                }

                if (_invalidDocumentData != null && newActiveDocument != _invalidDocumentData)
                {
                    return;
                }

                if (newActiveDocument != _activeDocument)
                {
                    if (DocumentDataPanel != null)
                    {
                        if (!SaveActiveDocumentData())
                        {
                            return;
                        }

                        _activeDocument = newActiveDocument;

                        DocumentDataPanel.LoadData(
                            (_activeDocument == null) 
                                ? null 
                                : _activeDocument.DocumentData);
                    }

                    _activeDocument = newActiveDocument;
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39714");
            }
        }

        /// <summary>
        /// Handles the DocumentSplit event of the _primaryPageLayoutControl control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DocumentSplitEventArgs"/> instance containing the event data.</param>
        void HandlePrimaryPageLayoutControl_DocumentSplit(object sender, DocumentSplitEventArgs e)
        {
            try
            {
                _activeDocument = null;

                var args = new DocumentDataRequestEventArgs(
                    e.OriginalDocument.PageControls
                        .Where(c => !c.Deleted)
                        .Select(c => c.Page.OriginalDocumentName)
                        .Distinct()
                        .ToArray());
                OnDocumentDataRequest(args);

                UpdateDocumentData((ExtendedOutputDocument)e.OriginalDocument, args.DocumentData);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39817");
            }
        }

        /// <summary>
        /// Handles the DocumentsMerged event of the _primaryPageLayoutControl control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DocumentsMergedEventArgs"/> instance containing the event data.</param>
        void HandlePrimaryPageLayoutControl_DocumentsMerged(object sender, DocumentsMergedEventArgs e)
        {
            try
            {
                _activeDocument = null;

                var args = new DocumentDataRequestEventArgs(
                    e.ResultingDocument.PageControls
                        .Where(c => !c.Deleted)
                        .Select(c => c.Page.OriginalDocumentName)
                        .Distinct()
                        .ToArray());
                OnDocumentDataRequest(args);

                UpdateDocumentData((ExtendedOutputDocument)e.ResultingDocument, args.DocumentData);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39818");
            }
        }

        /// <summary>
        /// Handles the <see cref="PageLayoutControl.LoadNextDocumentRequest"/> event of the
        /// <see cref="_primaryPageLayoutControl"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandlePrimaryPageLayoutControl_LoadNextDocumentRequest(object sender, EventArgs e)
        {
            try
            {
                OnLoadNextDocument();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI40151");
            }
        }

        /// <summary>
        /// Handles the ObservableCollection.CollectionChanged event of
        /// <see cref="_pendingDocuments"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="NotifyCollectionChangedEventArgs"/> instance containing
        /// the event data.</param>
        void HandlePendingDocuments_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            try
            {
                if (e.NewItems != null)
                {
                    foreach (var newDocument in e.NewItems.Cast<ExtendedOutputDocument>())
                    {
                        newDocument.DocumentDataChanged += HandleDocument_DocumentDataChanged;
                    }
                }

                if (e.OldItems != null)
                {
                    foreach (var oldDocument in e.OldItems.Cast<ExtendedOutputDocument>())
                    {
                        oldDocument.DocumentDataChanged -= HandleDocument_DocumentDataChanged;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39793");
            }
        }

        /// <summary>
        /// Handles the <see cref="ExtendedOutputDocument.DocumentDataChanged"/> event for all
        /// <see cref="_pendingDocuments"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleDocument_DocumentDataChanged(object sender, EventArgs e)
        {
            try
            {
                var document = (ExtendedOutputDocument)sender;

                foreach (var pageControl in document.PageControls)
                {
                    pageControl.Invalidate();
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39794");
            }
        }

        #endregion Event Handlers

        #region Private Members

        /// <summary>
        /// Clears and disposes of any existing <see cref="_primaryPageLayoutControl"/> and
        /// initializes a new one.
        /// </summary>
        void ResetPrimaryPageLayoutControl()
        {
            if (_primaryPageLayoutControl != null)
            {
                _primaryPageLayoutControl.StateChanged -= HandlePageLayoutControl_StateChanged;
                _primaryPageLayoutControl.SelectionChanged -= HandlePageLayoutControl_SelectionChanged;
                _primaryPageLayoutControl.DocumentSplit -= HandlePrimaryPageLayoutControl_DocumentSplit;
                _primaryPageLayoutControl.DocumentsMerged -= HandlePrimaryPageLayoutControl_DocumentsMerged;
                // If the control contains a lot of pagination controls, it can take a long time to
                // remove-- I am unclear why (it has to do with more than just whether thumbnails
                // are still being loaded. Disposing of the control first allows it to be quickly
                // removed.
                _primaryPageLayoutControl.Dispose();
                _tableLayoutPanel.Controls.Remove(_primaryPageLayoutControl);

                // Perform a GC to force a cleanup of everything before loading the new
                // _primaryPageLayoutControl
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }

            _primaryPageLayoutControl = new PageLayoutControl(this);
            _primaryPageLayoutControl.Shortcuts = _shortcuts;
            _primaryPageLayoutControl.Dock = DockStyle.Fill;
            _primaryPageLayoutControl.ImageViewer = _imageViewer;
            _primaryPageLayoutControl.StateChanged += HandlePageLayoutControl_StateChanged;
            _primaryPageLayoutControl.SelectionChanged += HandlePageLayoutControl_SelectionChanged;
            _primaryPageLayoutControl.DocumentSplit += HandlePrimaryPageLayoutControl_DocumentSplit;
            _primaryPageLayoutControl.DocumentsMerged += HandlePrimaryPageLayoutControl_DocumentsMerged;
            _primaryPageLayoutControl.LoadNextDocumentRequest += HandlePrimaryPageLayoutControl_LoadNextDocumentRequest;
            _tableLayoutPanel.Controls.Add(_primaryPageLayoutControl, 0, 1);
            _primaryPageLayoutControl.Focus();
        }

        /// <summary>
        /// Opens the specified <see paramref="inputFileName"/> as a <see cref="SourceDocument"/>
        /// instance.
        /// </summary>
        /// <param name="inputFileName">Name of the input file.</param>
        /// <returns>A <see cref="SourceDocument"/> representing <see paramref="inputFileName"/> or
        /// <see langword="null"/> if the file is missing or could not be opened.</returns>
        SourceDocument OpenDocument(string inputFileName)
        {
            SourceDocument sourceDocument = null;

            lock (_sourceDocumentLock)
            {
                if (!File.Exists(inputFileName))
                {
                    return null;
                }

                sourceDocument = _sourceDocuments.SingleOrDefault(doc =>
                    doc.FileName.Equals(inputFileName, StringComparison.OrdinalIgnoreCase));

                if (sourceDocument == null)
                {
                    sourceDocument = new SourceDocument(inputFileName);
                    if (!sourceDocument.Pages.Any())
                    {
                        sourceDocument.Dispose();
                        return null;
                    }

                    _sourceDocuments.Add(sourceDocument);
                }
            }

            ThreadingMethods.RunInBackgroundThread("ELI39570", () =>
            {
                lock (_sourceDocumentLock)
                {
                    if (_sourceDocuments.Contains(sourceDocument))
                    {
                        _imageViewer.CacheImage(inputFileName);
                    }
                }
            });

            return sourceDocument;
        }

        /// <summary>
        /// Orders _sourceDocuments so that it matches the order the corresponding pages currently
        /// appear in the panel.
        /// </summary>
        void ApplyOrderOfLoadedSourceDocuments()
        {
            lock (_sourceDocumentLock)
            {
                // Get the order of the first appearance of SourceDocuments in the pane.
                var orderedFileNames =
                    _primaryPageLayoutControl
                    .Documents
                    .SelectMany(doc => doc.PageControls)
                    .Select(pageControl => pageControl.Page.SourceDocument)
                    .Distinct()
                    .ToList();

                _sourceDocuments = _sourceDocuments
                    .OrderBy(doc => orderedFileNames.IndexOf(doc))
                    .ToList();
            }
        }

        /// <summary>
        /// Saves the data from any defined <see cref="DocumentDataPanel"/> into the
        /// <see cref=" _activeDocument"/> (if there is one).
        /// </summary>
        /// <returns><see langword="true"/> if the data was save or <see langword="false"/> if the
        /// data could not be saved and needs to be corrected.
        /// <para><b>Note</b></para>
        /// It is the responsibility of the DocumentDataPanel to inform the user of any data issues
        /// that need correction.
        /// </returns>
        bool SaveActiveDocumentData()
        {
            if (DocumentDataPanel == null || _activeDocument == null)
            {
                return true;
            }
            else if (DocumentDataPanel.SaveData(_activeDocument.DocumentData))
            {
                _invalidDocumentData = null;
                return true;
            }
            else
            {
                _invalidDocumentData = _activeDocument;
                this.SafeBeginInvoke("ELI39721", () =>
                {
                    try
                    {
                        _primaryPageLayoutControl.SelectDocument(_activeDocument);
                    }
                    finally
                    {
                        _invalidDocumentData = null;
                    }
                });
                return false;
            }
        }

        /// <summary>
        /// Updates the data associated with the specified <see paramref="sourceFileName"/>.
        /// </summary>
        /// <param name="outputDocument"></param>
        /// <param name="documentData">The data to associate with <see paramref="sourceFileName"/>.
        /// (replaces any previously assigned data)</param>
        void UpdateDocumentData(ExtendedOutputDocument outputDocument, IDocumentData documentData)
        {
            if (outputDocument.DocumentData != null)
            {
                outputDocument.DocumentData.ModifiedChanged -= DocumentData_ModifiedChanged;
            }
            outputDocument.DocumentData = documentData;
            if (outputDocument.DocumentData != null)
            {
                outputDocument.DocumentData.ModifiedChanged += DocumentData_ModifiedChanged;
            }
        }

        /// <summary>
        /// Updates the states of the pagination command buttons.
        /// </summary>
        void UpdateCommandStates()
        {
            var nonEmptyDocs = _pendingDocuments.Where(doc =>
                doc.PageControls.Any(c => !c.Deleted));
            bool isDocDataEdited = nonEmptyDocs.Any(doc =>
                doc.DocumentData != null && doc.DocumentData.Modified);

            RevertToOriginalEnabled =
                nonEmptyDocs.Any(doc => !doc.InOriginalForm || isDocDataEdited);
            _revertToOriginalToolStripButton.Enabled = RevertToOriginalEnabled;
            RevertToSourceEnabled =
                nonEmptyDocs.Any(doc => !doc.InSourceDocForm || isDocDataEdited);
            _revertToSourceToolStripButton.Enabled = RevertToSourceEnabled;
            CommitEnabled =
                PendingChanges || // Value can be overridden; need to enabled apply whenever true.
                _revertToOriginalToolStripButton.Enabled ||
                _revertToSourceToolStripButton.Enabled;
            _applyToolStripButton.Enabled = CommitEnabled;
        }

        /// <summary>
        /// Raises the <see cref="DocumentDataRequest"/> event.
        /// </summary>
        /// <param name="eventArgs">The <see cref="DocumentDataRequestEventArgs"/> instance containing
        /// the event data.</param>
        void OnDocumentDataRequest(DocumentDataRequestEventArgs eventArgs)
        {
            var eventHandler = DocumentDataRequest;
            if (eventHandler != null)
            {
                eventHandler(this, eventArgs);
            }
        }

        /// <summary>
        /// Raises the <see cref="LoadNextDocument"/> event.
        /// </summary>
        void OnLoadNextDocument()
        {
            var eventHandler = LoadNextDocument;
            if (eventHandler != null)
            {
                eventHandler(this, new EventArgs());
            }
        }

        /// <summary>
        /// Raises the <see cref="CreatingOutputDocument"/> event.
        /// </summary>
        /// <param name="eventArgs">The <see cref="CreatingOutputDocumentEventArgs"/> instance to
        /// use when raising the event.</param>
        void OnCreatingOutputDocument(CreatingOutputDocumentEventArgs eventArgs)
        {
            var eventHandler = CreatingOutputDocument;
            if (eventHandler != null)
            {
                eventHandler(this, eventArgs);
            }
        }

        /// <summary>
        /// Raises the <see cref="Paginated"/> event.
        /// </summary>
        /// <param name="paginatedDocumentSources">The source documents that were used to generate
        /// the paginated output. These documents will no longer be referenced by the
        /// <see cref="PaginationPanel"/>.</param>
        /// <param name="disregardedPaginationSources">All documents applied as they exist on disk
        /// but for which there was differing suggested pagination.</param>
        /// <param name="modifiedDocumentData">All documents names and associated
        /// <see cref="IDocumentData"/> where data was modified, but the document pages have not
        /// been modified compared to pagination on disk.</param>   
        void OnPaginated(IEnumerable<string> paginatedDocumentSources,
            IEnumerable<string> disregardedPaginationSources,
            IEnumerable<KeyValuePair<string, IDocumentData>> modifiedDocumentData)
        {
            var eventHandler = Paginated;
            if (eventHandler != null)
            {
                eventHandler(this, new PaginatedEventArgs(
                    paginatedDocumentSources, disregardedPaginationSources, modifiedDocumentData));
            }
        }

        /// <summary>
        /// Raises the <see cref="StateChanged"/> event.
        /// </summary>
        void OnStateChanged()
        {
            var eventHandler = StateChanged;
            if (eventHandler != null)
            {
                eventHandler(this, new EventArgs());
            }
        }

        #endregion Private Members
    }
}
