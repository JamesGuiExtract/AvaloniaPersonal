using ADODB;
using Extract.AttributeFinder;
using Extract.Imaging;
using Extract.Imaging.Forms;
using Extract.Utilities;
using Extract.Utilities.Forms;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using UCLID_RASTERANDOCRMGMTLib;
using ComAttribute = UCLID_AFCORELib.Attribute;

namespace Extract.UtilityApplications.PaginationUtility
{
    /// <summary>
    /// Pagination functionality encapsulated in a panel to allow for pagination as part of an
    /// application other than PaginationUtility.exe.
    /// </summary>
    public partial class PaginationPanel : UserControl, IPaginationUtility, IImageViewerControl
    {
        #region Constants

        /// <summary>
        /// Text to use for otherwise empty Document attribute values in pagination hierarchy
        /// </summary>
        private static readonly string _DOCUMENT_PLACEHOLDER_TEXT = "N/A";

        #endregion Constants

        #region Fields

        /// <summary>
        /// The <see cref="PageLayoutControl"/> into which input documents are initially loaded.
        /// </summary>
        PageLayoutControl _primaryPageLayoutControl;

        /// <summary>
        /// The <see cref="IPaginationDocumentDataPanel"/> used to display document specific fields
        /// corresponding to the currently selected document.
        /// </summary>
        IPaginationDocumentDataPanel _documentDataPanel;

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
        /// The set of all <see cref="OutputDocument"/>s currently active in the UI.
        /// </summary>
        ObservableCollection<OutputDocument> _pendingDocuments =
            new ObservableCollection<OutputDocument>();

        /// <summary>
        /// The <see cref="OutputDocument"/>s that were originally present as a result of
        /// the last LoadFile or RemoveSourceFile operation.
        /// </summary>
        List<OutputDocument> _originalDocuments = new List<OutputDocument>();

        /// <summary>
        /// All <see cref="OutputDocument"/> that relate to each
        /// <see cref="SourceDocument"/>.
        /// </summary>
        Dictionary<SourceDocument, HashSet<OutputDocument>> _sourceToOriginalDocuments =
            new Dictionary<SourceDocument, HashSet<OutputDocument>>();

        /// <summary>
        /// Maintains a temporary files to which <see cref="OutputDocument"/>s should be written
        /// until the owner of this panel has the opportunity to determine where it is going.
        /// </summary>
        Dictionary<OutputDocument, TemporaryFile> _tempFiles =
            new Dictionary<OutputDocument, TemporaryFile>();

        /// <summary>
        /// Keeps track of the names of documents that have been output as part of the current
        /// <see cref="CommitChanges"/> call.
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
        Dictionary<string, Tuple<bool, PaginationDocumentData>> _documentData =
            new Dictionary<string, Tuple<bool, PaginationDocumentData>>();

        /// <summary>
        /// Indicates the <see cref="OutputDocument"/> whose <see cref="PaginationDocumentData"/>
        /// is reflected in <see cref="DocumentDataPanel"/>.
        /// </summary>
        OutputDocument _documentWithDataInEdit;

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

        /// <summary>
        /// A <see cref="CheckBox"/> to set or clear the
        /// <see cref="PaginationSeparator.DocumentSelectedToCommit"/> flag for all documents.
        /// Added via code since toolstrips to not natively support checkboxes.
        /// </summary>
        CheckBox _selectAllToCommitCheckBox;

        /// <summary>
        /// Indicates whether the <see cref="UpdateCommandStates"/> is in the process of being
        /// called so that controls modifications aren't mistaken for user input.
        /// </summary>
        bool _updatingCommandStates;

        /// <summary>
        /// Indicates whether UI updates are currently suspended for performance reasons during an
        /// operation.
        /// </summary>
        bool _uiUpdatesSuspended;

        /// <summary>
        /// Indicates whether the <see cref="CommittingChanges"/> event is currently being raised in
        /// order to prevent recursive raising of the event when a handler calls back into
        /// <see cref="CommitChanges"/>.
        /// </summary>
        bool _raisingCommittingChanges;

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
        /// <see cref="PaginationDocumentData"/> should be assigned if needed.
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
        /// Raised when a pagination operation has failed.
        /// </summary>
        public event EventHandler<ExtractExceptionEventArgs> PaginationError;

        /// <summary>
        /// Raised when the state of any pages, documents or data has changed.
        /// </summary>
        public event EventHandler<EventArgs> StateChanged;

        /// <summary>
        /// Raised as selected <see cref="OutputDocument"/>s are being committed.
        /// </summary>
        public event EventHandler<CommittingChangesEventArgs> CommittingChanges;

        #endregion Events

        #region Configuration Properties

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
        /// Gets or sets whether the load next document button should be available.
        /// </summary>
        public bool LoadNextDocumentVisible
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets whether changes should be committed only for the selected documents.
        /// </summary>
        /// <value><see langword="true"/> if changes should be committed only for the selected
        /// document(s) or <see langword="false"/> if changes should be committed for all documents
        /// loaded into this instance.</value>
        public bool CommitOnlySelection
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets whether to output expected pagination attributes
        /// </summary>
        public bool OutputExpectedPaginationAttributesFile
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the expected pagination attributes path.
        /// </summary>
        public string ExpectedPaginationAttributesPath
        {
            get;
            set;
        }

        #endregion Configuration Properties

        #region Runtime Properties

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
        /// Gets or sets the <see cref="IPaginationDocumentDataPanel"/> to display and allow editing
        /// of data associated with any singly selected document or <see langword="null"/> if no
        /// such panel is available.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public IPaginationDocumentDataPanel DocumentDataPanel
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
                        _documentDataPanel = value;
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
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
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
        /// Gets whether there are pending changes available for <see cref="CommitChanges"/>.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public bool CommitEnabled
        {
            get
            {
                if (CommitOnlySelection)
                {
                    return _primaryPageLayoutControl != null &&
                        _primaryPageLayoutControl.Documents.Any(doc => doc.Selected);
                }
                else
                {
                    return PendingChanges || // Value can be overridden; need to enabled apply whenever true.
                        _revertToOriginalToolStripButton.Enabled ||
                        _revertToSourceToolStripButton.Enabled;
                }
            }
        }

        /// <summary>
        /// Gets whether there are any documents that differ from their original loaded form
        /// available for <see cref="RevertPendingChanges"/>.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public bool RevertToOriginalEnabled
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets whether there are any documents that differ from their source form available for
        /// available for <see cref="RevertPendingChanges"/>.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public bool RevertToSourceEnabled
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the file processing database.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public UCLID_FILEPROCESSINGLib.FileProcessingDB FileProcessingDB
        {
            get;
            set;
        }

        #endregion Runtime Properties

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
            LoadFile(fileName, position, null, null, false, null);
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
        /// <param name="deletedPages">The page numbers from <see paramref="fileName"/> to be
        /// loaded but shown as deleted.</param>
        /// <param name="paginationSuggested"><see langword="true"/> if pagination has been
        /// suggested for this document; <see langword="false"/> if it has not been.</param>
        /// <param name="documentData">The VOA file data associated with <see paramref="fileName"/>.
        /// </param>
        public void LoadFile(string fileName, int position, IEnumerable<int> pages,
            IEnumerable<int> deletedPages, bool paginationSuggested,
            PaginationDocumentData documentData)
        {
            try
            {
                FormsMethods.ExecuteInUIThread(this, () =>
                {
                    SuspendUIUpdatesForOperation();

                    // The OutputDocument doesn't get created directly by this method. Use
                    // _documentData to be able to pass this info to when it is needed in
                    // IPaginationUtility.CreateOutputDocument.
                    if (paginationSuggested || documentData != null)
                    {
                        _documentData[fileName] = new Tuple<bool, PaginationDocumentData>(
                            paginationSuggested, documentData);
                    }

                    var sourceDocument = OpenDocument(fileName);

                    if (sourceDocument != null)
                    {
                        // This will call Application.DoEvents in the midst of loading a document to
                        // keep the UI responsive as pages are loaded. This allows an opportunity
                        // for there to be multiple calls into LoadNextDocument at the same time.
                        var outputDocument = _primaryPageLayoutControl.CreateOutputDocument(
                            sourceDocument, pages, deletedPages, position, true);

                        _originalDocuments.Add(outputDocument);
                        var setOutputDocs = _sourceToOriginalDocuments.GetOrAdd(
                            sourceDocument, () => new HashSet<OutputDocument>());
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
        public bool UpdateDocumentData(string sourceFileName, PaginationDocumentData documentData)
        {
            try
            {
                var matchingDocuments = _primaryPageLayoutControl.Documents
                    .Where(doc => doc.InOriginalForm &&
                        doc.PageControls.First().Page.OriginalDocumentName.Equals(sourceFileName,
                            StringComparison.OrdinalIgnoreCase));

                if (matchingDocuments.Count() == 1)
                {
                    var document = matchingDocuments.Single();
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
        /// Commits changes (either for all document or just selected documents depending on the
        /// value of <see cref="CommitOnlySelection"/>.
        /// </summary>
        /// <returns><see langword="true"/> if changes were able to be committed; otherwise,
        /// <see langword="false"/>.</returns>
        public bool CommitChanges()
        {
            try
            {
                SuspendUIUpdatesForOperation();

                if (!CanSelectedDocumentsBeCommitted())
                {
                    return false;
                }

                _commitingChanges = true;

                // Prevent the UI from trying to load/close pages in the image viewer as the
                // operation is taking place.
                _primaryPageLayoutControl.ClearSelection();

                _outputDocumentNames.Clear();

                // Depending upon the manipulations that occurred, there may be some pending
                // documents that have been left without any pages. Disregard these.
                var documentsToRemove = _pendingDocuments
                    .Where(doc => !doc.PageControls.Any())
                    .ToArray();
                foreach (var document in documentsToRemove)
                {
                    _pendingDocuments.Remove(document);
                }

                var documentsToCommit = _pendingDocuments
                    .Where(document => !CommitOnlySelection || document.Selected);

                var outputDocuments = documentsToCommit
                    .Where(document => !document.InSourceDocForm)
                    .ToList();

                _outputDocumentPositions = outputDocuments
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
                    if (outputDocument.PageControls.Any(c => !c.Deleted))
                    {
                        outputDocument.Output();
                    }
                    else
                    {
                        // There are no undeleted pages to output; simply remove the document.
                        _primaryPageLayoutControl.DeleteOutputDocument(outputDocument);
                        _pendingDocuments.Remove(outputDocument);
                    }
                }

                var disregardedPagination = documentsToCommit
                    .Where(doc => documentsToCommit.Contains(doc) && doc.InSourceDocForm &&
                        doc.PaginationSuggested &&
                        !doc.InOriginalForm)
                    .Select(doc => doc.PageControls.First().Page.OriginalDocumentName);

                var sourcesWithModifiedData = documentsToCommit
                    .Where(doc => documentsToCommit.Contains(doc) && doc.InSourceDocForm &&
                        doc.DocumentData != null && doc.DocumentData.Modified)
                    .Select(doc => new KeyValuePair<string, PaginationDocumentData>(
                        doc.PageControls.First().Page.OriginalDocumentName, doc.DocumentData));

                var unmodifiedPagination = documentsToCommit
                    .Where(doc => documentsToCommit.Contains(doc) && doc.InSourceDocForm)
                    .Select(doc => doc.PageControls.First().Page.OriginalDocumentName);

                OnPaginated(sourceDocuments.Select(doc => doc.FileName),
                    disregardedPagination,
                    sourcesWithModifiedData,
                    unmodifiedPagination);

                // For any documents did not have either manual or suggested pagination, reset all
                // data about these documents so that nothing about the document is marked dirty.
                foreach (var document in _pendingDocuments
                    .Where(doc => documentsToCommit.Contains(doc)))
                {
                    document.PaginationSuggested = false;
                    document.SetOriginalForm();
                    foreach (var pageControl in document.PageControls)
                    {
                        pageControl.Invalidate();
                    }
                }

                _pendingChangesOverride = null;

                if (CommitOnlySelection)
                {
                    SelectAllToCommit(false);
                }

                return true;
            }
            catch (Exception ex)
            {
                var ee = ex.AsExtract("ELI39571");

                try
                {
                    OnPaginationError(ee);
                }
                catch (Exception ex2)
                {
                    ex2.ExtractLog("ELI40231");
                }

                throw ee;
            }
            finally
            {
                try
                {
                    _commitingChanges = false;
                    if (_outputDocumentPositions != null)
                    {
                        _outputDocumentPositions.Clear();
                    }

                    ApplyOrderOfLoadedSourceDocuments();

                    UpdateCommandStates();
                }
                catch (Exception ex)
                {
                    ex.ExtractLog("ELI40210");
                }
            }
        }

        /// <summary>
        /// Checks if changes can be committed.
        /// </summary>
        /// <returns><see langword="true"/> if the documents can be submitted or
        /// <see langword="false"/> if there is a reason they cannot be. This method will have
        /// displayed an necessary message to the user explaining why the document could not be
        /// submitted.
        /// </returns>
        bool CanSelectedDocumentsBeCommitted()
        {
            // If document data could not be saved, abort the commit operation.
            if (!SaveDocumentData(true))
            {
                return false;
            }

            if (_pendingDocuments.Any(document => document.Selected && document.DataError))
            {
                UtilityMethods.ShowMessageBox("Data errors must be corrected before saving", "Data Error", true);
                return false;
            }

            if (!CommitOnlySelection)
            {
                return true;
            }

            var affectedSourceDocuments = new HashSet<string>(
                _pendingDocuments
                    .Where(doc => doc.Selected)
                    .SelectMany(doc => doc.PageControls
                        .Select(c => c.Page.OriginalDocumentName)));

            var unIncludedPagesControls = 
                _pendingDocuments
                    .Where(doc => !doc.Selected)
                    .SelectMany(doc => doc.PageControls
                        .Where(c => 
                            affectedSourceDocuments.Contains(c.Page.OriginalDocumentName)));

            if (unIncludedPagesControls.Any())
            {
                var unselectedSourceDocuments = unIncludedPagesControls
                    .Select(c => Path.GetFileName(c.Page.OriginalDocumentName))
                    .Distinct();

                using (var msgBox = new CustomizableMessageBox())
                {
                    msgBox.Caption = "Commit error";
                    msgBox.StandardIcon = MessageBoxIcon.Error;
                    msgBox.Text = string.Format(CultureInfo.CurrentCulture,
                        "Cannot commit because the following document(s) were not fully selected: {0}\r\n\r\n" +
                        "Expand selection to include all affected source document pages?",
                        string.Join(", ", unselectedSourceDocuments));
                    msgBox.AddButton("Expand selection", "expand", false);
                    msgBox.AddButton("Cancel", "cancel", false);
                    if (msgBox.Show(this) == "expand")
                    {
                        foreach (var separator in unIncludedPagesControls
                            .Select(c => c.Document.PaginationSeparator)
                            .Distinct())
                        {
                            separator.DocumentSelectedToCommit = true;
                        }
                    }
                }

                return false;
            }

            return true;
        }

        /// <summary>
        /// Reverts any uncommitted document pagination changes.
        /// </summary>
        /// <param name="revertToSource"><see langword="true"/> to revert to the state of the source
        /// documents on disk; <see langword="false"/> to revert to the state that was originally
        /// presented (will differ from source state if pagination was suggested).</param>
        public void RevertPendingChanges(bool revertToSource)
        {
            try
            {
                SuspendUIUpdates = true;

                _primaryPageLayoutControl.ClearSelection();

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
                        HashSet<OutputDocument> outputDocuments = null;
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
                            var outputDocument =
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
                        outputDocument.Collapsed = false;
                        outputDocument.Selected = false;

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

                SuspendUIUpdates = false;
                _primaryPageLayoutControl.PerformFullLayout();
                _primaryPageLayoutControl.SelectFirstPage();
                _primaryPageLayoutControl.Focus();

                UpdateCommandStates();
            }
            catch (Exception ex)
            {
                SuspendUIUpdates = false;

                throw ex.AsExtract("ELI40018");
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
                            // If the document's data is open for editing, close the panel.
                            if (_documentWithDataInEdit == outputDocument)
                            {
                                if (!CloseDataPanel())
                                {
                                    _primaryPageLayoutControl.SelectDocument(outputDocument);

                                    throw new ExtractException("ELI40211",
                                        "Failed to save document data");
                                }
                            }

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

                // Output expected voa if so configured
                if (OutputExpectedPaginationAttributesFile)
                {
                    var pathTags = new SourceDocumentPathTags(fileName);
                    var outputFileName = pathTags.Expand(ExpectedPaginationAttributesPath);
                    WriteExpectedAttributes(fileName, outputFileName);
                }

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
                SaveDocumentData(false);
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
                var outputDocument = new OutputDocument(outputDocumentName);
                outputDocument.DocumentOutputting += HandleOutputDocument_DocumentOutputting;
                outputDocument.DocumentOutput += HandleOutputDocument_DocumentOutput;

                Tuple<bool, PaginationDocumentData> documentData;
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
                    outputDocument.DocumentData = args.DocumentData;
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

                if (CommitOnlySelection)
                {
                    // Since toolstrips to not natively support checkboxes, add the select all checkbox
                    // manually.
                    _selectAllToCommitCheckBox = new CheckBox();
                    var host = new ToolStripControlHost(_selectAllToCommitCheckBox);
                    // Pad a couple of pixels to align with separator check boxes.
                    host.Margin = new Padding(0, 0, 2, 0);
                    _topToolStrip.Items.Insert(1, host);
                    _selectAllToCommitCheckBox.CheckedChanged += HandleSelectAllToCommitCheckBox_CheckedChanged;
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
                var outputDocument = (OutputDocument)sender;

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
            OutputDocument outputDocument = null;
            TemporaryFile tempFile = null;

            try
            {
                // The file has been output at this point according to the PageLayoutControl, but
                // this is actually only to a temporary in order to give the owner of this panel the
                // opportunity to determine where it is going before the file is written to the
                // final location.
                outputDocument = (OutputDocument)sender;
                tempFile = _tempFiles[outputDocument];

                var sourcePageInfo = outputDocument.PageControls
                    .Where(c => !c.Deleted)
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
                // Allow ability of external event handler to handle the apply action.
                var committingChangesArgs = new CommittingChangesEventArgs();
                OnCommittingChanges(committingChangesArgs);
                if (!committingChangesArgs.Handled)
                {
                    CommitChanges();
                }
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
        /// Handles the <see cref="PaginationDocumentData.ModifiedChanged"/> event for the document
        /// data of any of the <see cref="_pendingDocuments"/>.
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
        /// Handles the DocumentDataPanelRequest event of the _primaryPageLayoutControl control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DocumentDataPanelRequestEventArgs"/> instance containing
        /// the event data.</param>
        void HandlePrimaryPageLayoutControl_DocumentDataPanelRequest(object sender,
            DocumentDataPanelRequestEventArgs e)
        {
            try
            {
                if (DocumentDataPanel != null)
                {
                    if (!CloseDataPanel())
                    {
                        _primaryPageLayoutControl.SelectDocument(_documentWithDataInEdit);

                        throw new ExtractException("ELI40212", "Failed to save document data");
                    }

                    _documentWithDataInEdit = e.OutputDocument;
                    DocumentDataPanel.LoadData(_documentWithDataInEdit.DocumentData);
                    e.DocumentDataPanel = DocumentDataPanel;
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI40177");
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
                    foreach (var newDocument in e.NewItems.Cast<OutputDocument>())
                    {
                        newDocument.DocumentDataChanged += HandleDocument_DocumentDataChanged;
                    }
                }

                if (e.OldItems != null)
                {
                    foreach (var oldDocument in e.OldItems.Cast<OutputDocument>())
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
        /// Handles the <see cref="OutputDocument.DocumentDataChanged"/> event for all
        /// <see cref="_pendingDocuments"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleDocument_DocumentDataChanged(object sender, EventArgs e)
        {
            try
            {
                var document = (OutputDocument)sender;

                foreach (var pageControl in document.PageControls)
                {
                    pageControl.Invalidate();
                }

                if (document.PaginationSeparator != null)
                {
                    document.PaginationSeparator.Invalidate();
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39794");
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event of the
        /// <see cref="_collapseAllToolStripButton"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleCollapseAllToolStripButton_Click(object sender, EventArgs e)
        {
            try
            {
                SuspendUIUpdatesForOperation();

                bool collapse = !AllDocumentsCollapsed;

                CollapseAll(collapse);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI40216");
            }
        }

        /// <summary>
        /// Handles the <see cref="CheckBox.CheckedChanged"/> event of the
        /// <see cref="_selectAllToCommitCheckBox"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void HandleSelectAllToCommitCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (CommitOnlySelection && !_updatingCommandStates)
                {
                    SuspendUIUpdatesForOperation();

                    bool select = _selectAllToCommitCheckBox.Checked;

                    SelectAllToCommit(select);
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI40217");
            }
        }

        #endregion Event Handlers

        #region Private Members

        /// <summary>
        /// Gets a value indicating whether all documents are currently collapsed (pages hidden).
        /// </summary>
        /// <value><see langword="true"/> if all documents are collapsed; otherwise,
        /// <see langword="false"/>.
        /// </value>
        bool AllDocumentsCollapsed
        {
            get
            {
                return _pendingDocuments.All(doc => doc.Collapsed
                    || !doc.PageControls.Any());
            }
        }

        /// <summary>
        /// Collapses or expands all documents. (shows/hides all document pages).
        /// </summary>
        /// <param name="collapse"><see langword="true"/> to collapse all documents to hide their
        /// pages of <see langword="false"/> to expand them to display all pages.</param>
        void CollapseAll(bool collapse)
        {
            foreach (var document in _pendingDocuments)
            {
                var separator = document.PaginationSeparator;
                if (separator != null)
                {
                    separator.Collapsed = collapse;
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether all documents are currently selected for committal.
        /// </summary>
        /// <value><see langword="true"/> if all documents selected for committal; otherwise,
        /// <see langword="false"/>.
        /// </value>
        bool AllDocumentsSelected
        {
            get
            {
                return CommitOnlySelection &&
                    _pendingDocuments.All(doc => doc.PageControls.Count == 0 || doc.Selected);
            }
        }

        /// <summary>
        /// Selects or un-selects all documents for committal.
        /// </summary>
        /// <param name="select"><see langword="true"/> to select all documents for committal or
        /// <see langword="false"/> to clear all the check boxes.</param>
        void SelectAllToCommit(bool select)
        {
            foreach (var document in _pendingDocuments)
            {
                var separator = document.PaginationSeparator;
                if (separator != null)
                {
                    separator.DocumentSelectedToCommit = select;
                }
            }
        }

        /// <summary>
        /// Clears and disposes of any existing <see cref="_primaryPageLayoutControl"/> and
        /// initializes a new one.
        /// </summary>
        void ResetPrimaryPageLayoutControl()
        {
            if (_primaryPageLayoutControl != null)
            {
                _primaryPageLayoutControl.StateChanged -= HandlePageLayoutControl_StateChanged;
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
            _primaryPageLayoutControl.CommitOnlySelection = CommitOnlySelection;
            _primaryPageLayoutControl.LoadNextDocumentVisible = LoadNextDocumentVisible;
            _primaryPageLayoutControl.StateChanged += HandlePageLayoutControl_StateChanged;
            _primaryPageLayoutControl.LoadNextDocumentRequest += HandlePrimaryPageLayoutControl_LoadNextDocumentRequest;
            _primaryPageLayoutControl.DocumentDataPanelRequest += HandlePrimaryPageLayoutControl_DocumentDataPanelRequest;
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
        /// Saves the data from any defined <see cref="DocumentDataPanel"/> if currently active.
        /// </summary>
        /// <param name="onlyIfSelected"></param>
        /// <returns><see langword="true"/> if the data was saved or <see langword="false"/> if the
        /// data could not be saved and needs to be corrected.
        /// <para><b>Note</b></para>
        /// It is the responsibility of the DocumentDataPanel to inform the user of any data issues
        /// that need correction.
        /// </returns>
        bool SaveDocumentData(bool onlyIfSelected)
        {
            if (_documentWithDataInEdit == null || 
                (onlyIfSelected && !_documentWithDataInEdit.Selected) ||
                CloseDataPanel())
            {
                return true;
            }
            else
            {
                this.SafeBeginInvoke("ELI39721", () =>
                {
                     _primaryPageLayoutControl.SelectDocument(_documentWithDataInEdit);
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
        void UpdateDocumentData(OutputDocument outputDocument, PaginationDocumentData documentData)
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
        /// Closes the <see cref="DocumentDataPanel"/> (if visible), applying any changed data in
        /// the process.
        /// </summary>
        bool CloseDataPanel()
        {
            if (DocumentDataPanel != null)
            {
                var panelControl = (Control)DocumentDataPanel;
                if (panelControl.Parent != null)
                {
                    if (_documentWithDataInEdit == null ||
                        (DocumentDataPanel.SaveData(_documentWithDataInEdit.DocumentData) &&
                         !_documentWithDataInEdit.DataError))
                    {
                        panelControl.Parent.Controls.Remove(panelControl);
                        _documentWithDataInEdit = null;
                    }
                    else
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Updates the states of the pagination command buttons.
        /// </summary>
        void UpdateCommandStates()
        {
            try
            {
                _updatingCommandStates = true;

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
                _applyToolStripButton.Enabled = CommitEnabled;
                _collapseAllToolStripButton.Image =
                    AllDocumentsCollapsed
                        ? Properties.Resources.Expand
                        : Properties.Resources.Collapse;
                if (CommitOnlySelection)
                {
                    _selectAllToCommitCheckBox.Checked = AllDocumentsSelected;
                }
            }
            finally
            {
                _updatingCommandStates = false;
            }
        }

        /// <summary>
        /// Gets or sets whether UI updates are currently suspended for performance reasons during
        /// an operation.
        /// </summary>
        public bool SuspendUIUpdates
        {
            get
            {
                return _uiUpdatesSuspended;
            }

            set
            {
                try
                {
                    if (value != _uiUpdatesSuspended)
                    {
                        if (value)
                        {
                            _uiUpdatesSuspended = true;
                            SuspendLayout();
                            _primaryPageLayoutControl.UIUpdatesSuspended = true;
                            EnablePageDisplay = false;
                        }
                        else
                        {
                            _uiUpdatesSuspended = false;
                            EnablePageDisplay = true;
                            _primaryPageLayoutControl.UIUpdatesSuspended = false;
                            ResumeLayout(true);
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI40226");
                }
            }
        }

        /// <summary>
        /// Temporarily suspends UI updates for the context of the current message handler. UI
        /// updates will automatically be resumed with the next windows message.
        /// </summary>
        public void SuspendUIUpdatesForOperation()
        {
            try
            {
                if (!_uiUpdatesSuspended)
                {
                    SuspendUIUpdates = true;

                    this.SafeBeginInvoke("ELI40218", () =>
                    {
                        SuspendUIUpdates = false;
                    });
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI40227");
            }
        }

        /// <summary>
        /// Creates the expected attributes.
        /// </summary>
        /// <param name="sourceDocName">Path of the source image file</param>
        /// <param name="outputFileName">Path to write the expected attributes to</param>
        private void WriteExpectedAttributes(string sourceDocName, string outputFileName)
        {
            try
            {
                // Select records for the file only if the file was not created as a result of pagination
                string query = string.Format(CultureInfo.CurrentCulture,
                    @"SELECT [Pages],[SourcePage],[DestFileID],[DestPage],[FileTaskSessionID] " +
                     "FROM [FAMFile] LEFT OUTER JOIN [Pagination] " +
                     "ON [Pagination].[SourceFileID] = [FAMFile].[ID] " +
                     "WHERE [FileName] = '{0}' " +
                     "AND [FamFile].[ID] NOT IN (SELECT [DestFileID] FROM [Pagination])",
                     sourceDocName.Replace("'", "''"));

                var records = GetResultsForQuery(query).Rows.Cast<DataRow>();

                if (!records.Any())
                {
                    return;
                }

                var documents = new List<string>();
                var lastSession = records.Max(r => r["FileTaskSessionID"] as int?);

                // If no pagination occurred, then the expected data should be a single document for
                // the entire range
                if (lastSession == null)
                {
                    int numberOfPages = (int)records.First()["Pages"];
                    documents.Add(Enumerable.Range(1, numberOfPages).ToRangeString());
                }
                // Otherwise, output one document for each destination file
                else
                {
                    var outputDocuments = records
                        .Where(r => r["FileTaskSessionID"] as int? == lastSession
                            && r["DestPage"] as int? != null)
                        .OrderBy(r => (int)r["DestPage"])
                        .GroupBy(r => r["DestFileID"] as int?,
                                 r => (int)r["SourcePage"]);

                    foreach (var documentPages in outputDocuments)
                    {
                        documents.Add(documentPages.ToRangeString());
                    }
                }

                // Create the hierarchy
                documents.Select(pages =>
                {
                    var ss = new SpatialStringClass();
                    ss.CreateNonSpatialString(_DOCUMENT_PLACEHOLDER_TEXT, sourceDocName);
                    var documentAttribute = new ComAttribute { Name = "Document", Value = ss };

                    // Add a Pages attribute to denote the range of pages in this document
                    ss = new SpatialStringClass();
                    ss.CreateNonSpatialString(pages, sourceDocName);
                    documentAttribute.SubAttributes.PushBack(new ComAttribute { Name = "Pages", Value = ss });
                    return documentAttribute;
                })
                    .SaveToIUnknownVector(outputFileName);

            }
            catch (Exception e)
            {
                throw e.AsExtract("ELI40159");
            }
        }


        /// <summary>
        /// Gets the results for the specified query from <see cref="FileProcessingDB"/>.
        /// <para><b>NOTE</b></para>
        /// The caller is responsible for disposing of the returned <see cref="DataTable"/>.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>A <see cref="DataTable"/> with the results of the specified query.</returns>
        private DataTable GetResultsForQuery(string query)
        {
            DataTable queryResults = new DataTable();
            queryResults.Locale = CultureInfo.CurrentCulture;
            Recordset adoRecordset = null;

            try
            {
                // Populate all pre-defined search terms from the database's FieldSearch table.
                adoRecordset = FileProcessingDB.GetResultsForQuery(query);

                // Convert the ADODB Recordset to a DataTable.
                using (var adapter = new System.Data.OleDb.OleDbDataAdapter())
                {
                    adapter.Fill(queryResults, adoRecordset);
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI40160");
            }
            finally
            {
                // If the recordset is not currently closed, close it. Some queries that do not
                // produce results will potentially result in a recordset that is closed right away.
                if (adoRecordset != null && adoRecordset.State != 0)
                {
                    adoRecordset.Close();
                }
            }

            return queryResults;
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
        /// <see cref="PaginationDocumentData"/> where data was modified, but the document pages
        /// have not been modified compared to pagination on disk.</param>
        /// <param name="unmodifiedPaginationSources"></param>
        void OnPaginated(IEnumerable<string> paginatedDocumentSources,
            IEnumerable<string> disregardedPaginationSources,
            IEnumerable<KeyValuePair<string, PaginationDocumentData>> modifiedDocumentData,
            IEnumerable<string> unmodifiedPaginationSources)
        {
            var eventHandler = Paginated;
            if (eventHandler != null)
            {
                eventHandler(this, 
                    new PaginatedEventArgs(
                        paginatedDocumentSources, disregardedPaginationSources,
                        modifiedDocumentData, unmodifiedPaginationSources));
            }
        }

        /// <summary>
        /// Raises the <see cref="PaginationError"/> event.
        /// </summary>
        /// <param name="ee">The exception that has been thrown from pagination.
        /// </param>
        void OnPaginationError(ExtractException ee)
        {
            var eventHandler = PaginationError;
            if (eventHandler != null)
            {
                eventHandler(this, new ExtractExceptionEventArgs(ee));
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

        /// <summary>
        /// Raises the <see cref="CommittingChanges"/> event.
        /// </summary>
        /// <param name="e"></param>
        void OnCommittingChanges(CommittingChangesEventArgs e)
        {
            if (_raisingCommittingChanges)
            {
                return;
            }

            try
            {
                var eventHandler = CommittingChanges;
                if (eventHandler != null)
                {
                    _raisingCommittingChanges = true;
                    eventHandler(this, e);
                }
            }
            finally
            {
                _raisingCommittingChanges = false;
            }
        }

        #endregion Private Members
    }
}
