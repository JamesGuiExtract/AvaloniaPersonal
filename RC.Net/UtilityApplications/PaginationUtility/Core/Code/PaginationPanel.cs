using ADODB;
using Extract.AttributeFinder;
using Extract.DataEntry.LabDE;
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
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using UCLID_AFCORELib;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;
using UCLID_RASTERANDOCRMGMTLib;
using static System.FormattableString;
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

        /// <summary>
        /// A string representation of the GUID for <see cref="AttributeStorageManagerClass"/> 
        /// </summary>
        static readonly string _ATTRIBUTE_STORAGE_MANAGER_GUID =
            typeof(AttributeStorageManagerClass).GUID.ToString("B");

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
        /// The set of all <see cref="OutputDocument"/>s currently displayed in the UI.
        /// </summary>
        ObservableCollection<OutputDocument> _displayedDocuments =
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
        /// Indicated a manually overridden value for <see cref="PendingChanges"/> or
        /// <see langword="null"/> if there is no manually overridden value
        /// (<see cref="PendingChanges"/> will return a calculated value).
        /// </summary>
        bool? _pendingChangesOverride;

        /// <summary>
        /// Indicates whether pending pagination changes are in the process of being committed.
        /// </summary>
        bool _committingChanges;

        /// <summary>
        /// The <see cref="ImageViewer"/> to be used by this instance.
        /// </summary>
        ImageViewer _imageViewer;

        /// <summary>
        /// The <see cref="ShortcutsManager"/> managing all keyboard shortcuts for this instance.
        /// </summary>
        ShortcutsManager _shortcuts = new ShortcutsManager();

        /// <summary>
        /// Keeps track of image viewer shortcuts that have been disabled.
        /// </summary>
        Dictionary<Keys, ShortcutHandler> _disabledImageViewerShortcuts =
            new Dictionary<Keys, ShortcutHandler>();

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

        /// <summary>
        /// Indicates whether _documentDataPanel is currently focused.
        /// </summary>
        bool _dataPanelFocused;

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

                _displayedDocuments.CollectionChanged += HandleDisplayedDocuments_CollectionChanged;
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
        /// Occurs when saving data for a file
        /// </summary>
        public event EventHandler<SavingDataEventArgs> SavingData;

        /// <summary>
        /// Occurs when done saving data for all files
        /// </summary>
        public event EventHandler<SavingDataEventArgs> DoneSavingData;

        /// <summary>
        /// Raised when a newly paginated document is generated.
        /// </summary>
        public event EventHandler<CreatingOutputDocumentEventArgs> CreatingOutputDocument;

        /// <summary>
        /// Raised when the newly created document is fully created (moved from temp dir to final destination)
        /// </summary>
        public event EventHandler<CreatingOutputDocumentEventArgs> OutputDocumentCreated;

        /// <summary>
        /// Raised when a proposed output document is deleted (applied with all pages deleted).
        /// </summary>
        public event EventHandler<OutputDocumentDeletedEventArgs> OutputDocumentDeleted;

        /// <summary>
        /// Raised when a proposed output document applied without adding/deleting/re-ordering/rotating
        /// any pages from the source document.
        /// </summary>
        public event EventHandler<AcceptedSourcePaginationEventArgs> AcceptedSourcePagination;

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

        /// <summary>
        /// Raised to indicate this panel is being re-initialized.
        /// </summary>
        public event EventHandler<EventArgs> PanelResetting;

        /// <summary>
        /// Raised to indicate the panel content is being reverted (either to original or suggested
        /// pagination).
        /// </summary>
        public event EventHandler<EventArgs> RevertedChanges;

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
        /// Gets or sets a value indicating whether the indicator to show whether documents will be
        /// queued for reprocessing should be hidden.
        /// </summary>
        /// <value><see langword="true"/> if the reprocessing indicator should be hidden; otherwise,
        /// <see langword="false"/>.
        /// </value>
        [DefaultValue(true)]
        public bool AutoSelectForReprocess
        {
            get;
            set;
        } = true;

        /// <summary>
        /// Gets or sets a value indicating whether document pages for newly loaded documents should
        /// be collapsed by default.
        /// </summary>
        public bool DefaultToCollapsed
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether pages should automatically be oriented to match
        /// the orientation of the text (per OCR).
        /// </summary>
        /// <value>
        ///   <c>true</c> if pages should automatically be oriented to match the orientation of the
        ///   text; otherwise, <c>false</c>.
        /// </value>
        public bool AutoRotateImages
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the select all check box should be visible.
        /// </summary>
        public bool SelectAllCheckBoxVisible
        {
            get;
            set;
        } = true;

        /// <summary>
        /// Gets or sets a value indicating whether the save button visible (to save changes without committing).
        /// </summary>
        public bool SaveButtonVisible
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance should cache images with the
        /// <see cref="ImageViewer"/> when loading files or <see langword="false"/> if it should not
        /// (such as if the application is managing caching externally).
        /// </summary>
        /// <value><see langword="true"/> if this instance should cache images; otherwise,
        /// <see langword="false"/>.
        /// </value>
        public bool CacheImages
        {
            get;
            set;
        }

        /// <summary>
        /// <c>true</c> to indicate the verification task will create pagination output documents
        /// itself; <c>false</c> if a server-side task will create the output documents.
        /// </summary>
        public bool CreateDocumentOnOutput
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

        /// <summary>
        /// Gets or sets whether the user should view all pages before applying/accepting pagination.
        /// </summary>
        public bool RequireAllPagesToBeViewed
        {
            get;
            set;
        }

        #endregion Configuration Properties

        #region Runtime Properties

        /// <summary>
        /// Gets whenther the panel is currently commiting changes.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public bool IsCommittingChanges
        {
            get
            {
                return _committingChanges;
            }
        }

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
                    if (_committingChanges)
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
                        return (PendingDocuments != null) &&
                            PendingDocuments.Any(document => !document.InOriginalForm ||
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
        /// Gets the number of proposed output documents displayed.
        /// </summary>
        public int OutputDocumentCount
        {
            get
            {
                try
                {
                    return _primaryPageLayoutControl.Documents
                        .Count(doc => doc.PageControls.Any(page => !page.Deleted));
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI44683");
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
                        if (_documentDataPanel != null)
                        {
                            _documentDataPanel.PageLoadRequest -= HandleDocumentDataPanel_PageLoadRequest;
                            if (_documentDataPanel.PanelControl != null)
                            {
                                _documentDataPanel.PanelControl.Enter -= HandlePanelControl_Enter;
                                _documentDataPanel.PanelControl.Leave -= HandlePanelControl_Leave;
                                _documentDataPanel.DataPanelChanged -= HandleDataPanel_DataPanelChanged;
                            }
                        }

                        _documentDataPanel = value;

                        if (_documentDataPanel != null)
                        {
                            _documentDataPanel.PageLoadRequest += HandleDocumentDataPanel_PageLoadRequest;
                            if (_documentDataPanel.PanelControl != null)
                            {
                                _documentDataPanel.PanelControl.Enter += HandlePanelControl_Enter;
                                _documentDataPanel.PanelControl.Leave += HandlePanelControl_Leave;
                                _documentDataPanel.DataPanelChanged += HandleDataPanel_DataPanelChanged;
                            }
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
        public bool RevertToSuggestedEnabled
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

        /// <summary>
        /// Gets a value indicating whether this instance's data panel is open.
        /// </summary>
        /// <value><see langword="true"/> if this instance's data panel is open; otherwise,
        /// <see langword="false"/>.
        /// </value>
        public bool IsDataPanelOpen
        {
            get
            {
                try
                {
                    // The DocumentDataPanel member of this class will remain set even when it is
                    // not open. However, the DocumentDataPanel member of PageLayoutControl will
                    // only be assigned when the panel is actually open.
                    return _primaryPageLayoutControl?.DocumentDataPanel != null;
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI44684");
                }
            }
        }

        /// <summary>
        /// Gets the current number of pages contained in this panel (across all documents).
        /// </summary>
        public int PageCount
        {
            get
            {
                return _primaryPageLayoutControl.PageCount;
            }
        }

        /// <summary>
        /// Gets a value indicating whether all pages (in all documents) in this panel have been viewed.
        /// </summary>
        public bool AllPagesViewed
        {
            get
            {
                return _primaryPageLayoutControl
                    .PageControls
                    .All(pageControl => pageControl.Viewed);
            }
        }

        /// <summary>
        /// Gets a value indicating whether this <see cref="PaginationPanel"/> is parked.
        /// </summary>
        public bool Parked
        {
            get;
            private set;
        }

        #endregion Runtime Properties

        #region Internal Properties

        #endregion Internal Properties

        #region Methods

        /// <summary>
        /// Loads the specified documents pages into the pane.
        /// </summary>
        /// <param name="fileName">Name of the file to load.</param>
        /// <param name="position">The position at which a document should be loaded. 0 = Load at
        /// the front (top), -1 = load at the end (bottom). Any other value should be a value
        /// passed via <see cref="CreatingOutputDocumentEventArgs"/> and not a value the caller
        /// should expect to be able to calculate.</param>
        /// <param name="selectDocument"><see langword="true"/> to select the document; otherwise,
        /// <see langword="false"/>.</param>
        public void LoadFile(string fileName, int fileID, int position, bool selectDocument)
        {
            LoadFile(fileName, fileID, position, null, null, null, false, null, selectDocument);
        }

        /// <summary>
        /// Loads the specified documents pages into the pane.
        /// </summary>
        /// <param name="fileName">Name of the file to load.</param>
        /// <param name="fileID">The file ID.</param>
        /// <param name="position">The position at which a document should be loaded. 0 = Load at
        /// the front (top), -1 = load at the end (bottom). Any other value should be a value
        /// passed via <see cref="CreatingOutputDocumentEventArgs"/> and not a value the caller
        /// should expect to be able to calculate.</param>
        /// <param name="pages">The page numbers from <see paramref="fileName"/> to be loaded or
        /// <see langword="null"/> to load all pages.</param>
        /// <param name="deletedPages">The page numbers from <see paramref="fileName"/> to be
        /// loaded but shown as deleted.</param>
        /// <param name="viewedPages">The page numbers from <see paramref="fileName"/> to be
        /// loaded but shown as viewed.</param>
        /// <param name="paginationSuggested"><see langword="true"/> if pagination has been
        /// suggested for this document; <see langword="false"/> if it has not been.</param>
        /// <param name="documentData">The VOA file data associated with <see paramref="fileName"/>.
        /// </param>
        /// <param name="selectDocument"><see langword="true"/> to select the document; otherwise,
        /// <see langword="false"/>.</param>
        public void LoadFile(string fileName, int fileID, int position, IEnumerable<int> pages,
            IEnumerable<int> deletedPages, IEnumerable<int> viewedPages, bool paginationSuggested,
            PaginationDocumentData documentData, bool selectDocument)
        {
            try
            {
                if (InvokeRequired)
                {
                    this.Invoke((MethodInvoker)(() => LoadFile(
                        fileName, fileID, position, pages, deletedPages, viewedPages, 
                        paginationSuggested, documentData, selectDocument)));
                    return;
                }

                _pendingChangesOverride = null;

                SuspendUIUpdatesForOperation();

                // Selecting the new document causes unexepected UI behavior and, in some cases
                // exceptions if new documents are being loaded collapsed or a DEP is currently
                // open for another document.
                // https://extract.atlassian.net/browse/ISSUE-15337
                selectDocument &= !DefaultToCollapsed && !IsDataPanelOpen;

                // The OutputDocument doesn't get created directly by this method. Use
                // _documentData to be able to pass this info to when it is needed in
                // IPaginationUtility.CreateOutputDocument.
                if (paginationSuggested || documentData != null)
                {
                    _documentData[fileName] = new Tuple<bool, PaginationDocumentData>(
                        paginationSuggested, documentData);
                }

                var sourceDocument = OpenDocument(fileName, fileID);

                if (sourceDocument != null)
                {
                    // This will call Application.DoEvents in the midst of loading a document to
                    // keep the UI responsive as pages are loaded. This allows an opportunity
                    // for there to be multiple calls into LoadNextDocument at the same time.
                    var outputDocument = (documentData?.PaginationRequest == null)
                        ? _primaryPageLayoutControl.CreateOutputDocument(sourceDocument, pages, deletedPages, viewedPages, position, true)
                        : _primaryPageLayoutControl.CreateOutputDocument(sourceDocument, documentData.PaginationRequest, position, true);

                    _originalDocuments.Add(outputDocument);
                    var setOutputDocs = _sourceToOriginalDocuments.GetOrAdd(
                        sourceDocument, _ => new HashSet<OutputDocument>());
                    setOutputDocs.Add(outputDocument);

                    _primaryPageLayoutControl.GetDocumentPosition(outputDocument);

                    if (selectDocument)
                    {
                        _primaryPageLayoutControl.SelectDocument(outputDocument);
                    }
                }

                ApplyOrderOfLoadedSourceDocuments();

                // https://extract.atlassian.net/browse/ISSUE-15351
                // In the process of loading a document, controls are added and removed. In some
                // cases, such as with the removal and re-addition of the load next document button,
                // this seems leave focus in the DEP when it should really be with the newly loaded
                // document. Re-evaluate current focus any time a new file has been loaded.
                ProcessFocusChange(forceUpdate: false);
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
        /// Adds any pages from <see paramref="sourceDocumentName"/> not already included in the
        /// panel into a separate output document where all pages are marked as deleted by default.
        /// It is possible such pages had been moved into documents from other sources or into a
        /// document that has since been reverted such that it no longer includes the page.
        /// This ensures the page is there to be added to a new output document if needed.
        /// </summary>
        /// <param name="sourceDocumentName">The name of a source document already represented in
        /// the UI for which any missing pages should be added.</param>
        public void AddOrphanedPages(string sourceDocumentName)
        {
            try
            {
                var sourceDocument = _sourceDocuments.Single(s => s.FileName == sourceDocumentName);

                var orphanedPages = sourceDocument.Pages
                    .Except(_primaryPageLayoutControl.PageControls.Select(c => c.Page));
                if (orphanedPages.Any())
                {
                    _primaryPageLayoutControl.CreateOutputDocument(
                        sourceDocument, null, orphanedPages.Select(p => p.OriginalPageNumber), null, -1, true);
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI47248");
            }
        }

        /// <summary>
        /// Gets the data associated with the specified <see paramref="sourceFileName"/>.
        /// </summary>
        /// <param name="sourceFileName">The name of the document for which data should be retrieved.</param>
        /// <returns>The <see cref="PaginationDocumentData"/> instance associated with
        /// <see paramref="sourceFileName"/> or <see langword="null"/> if it has no data.</returns>
        public PaginationDocumentData GetDocumentData(string sourceFileName)
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
                    return document.DocumentData;
                }

                return null;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI40272");
            }
        }

        /// <summary>
        /// Updates the data associated with the specified <see paramref="sourceFileName"/>.
        /// <para><b>Note:</b></para>
        /// The document's data can be updated only in the case that the currently represented
        /// pagination does not differ at all with how this document currently exists on disk.
        /// </summary>
        /// <param name="sourceFileName">The name of the document for which data should be updated.</param>
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
                    document.DocumentData = documentData;

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
        public bool IsInSourceDocForm(string sourceFileName)
        {
            try
            {
                var matchingDocuments = _primaryPageLayoutControl.Documents
                    .Where(doc => doc.InSourceDocForm &&
                        doc.PageControls.First().Page.OriginalDocumentName.Equals(sourceFileName,
                            StringComparison.OrdinalIgnoreCase));

                return (matchingDocuments.Count() == 1) &&
                    matchingDocuments.Single().InSourceDocForm;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI40277");
            }
        }

        /// <summary>
        /// Gets whether the pagination pane depicts the document in the same form as it was
        /// initially loaded.
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
                    .Where(doc =>
                        doc.PageControls
                            .First()
                            .Page.OriginalDocumentName.Equals(
                                sourceFileName,StringComparison.OrdinalIgnoreCase));

                return matchingDocuments.All(doc => doc.InOriginalForm);
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

                foreach (var document in PendingDocuments)
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
        /// Gets whether there are any manual changes to either pagination or pagination data.
        /// </summary>
        /// <param name="paginationModified"><see langword="true"/> if there has been manual changes to pagination.</param>
        /// <param name="dataModified"><see langword="true"/> if there has been manual changes to data.</param>
        [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "0#")]
        [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "1#")]
        public void CheckForChanges(out bool paginationModified, out bool dataModified)
        {
            try 
            {	        
                paginationModified = PendingDocuments.Any(document =>
                    !document.InOriginalForm);
                dataModified = PendingDocuments.Any(document =>
                    document.DataModified && !document.DocumentData.DataSharedInVerification);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI40281");
            }
        }

        /// <summary>
        /// Saves the current state of pagination and index data for all source documents.
        /// NOTE: An exception will be thrown if the pages from multiple source documents
        /// are currently combined into a single proposed output document.
        /// </summary>
        /// <returns><c>true</c> if the data was saved; otherwise, <c>false</c>.</returns>
        public bool Save()
        {
            TemporaryWaitCursor waitCursor = new TemporaryWaitCursor();

            try
            {
                bool result = SaveDocumentData(selectedDocumentsOnly: false, validateData: false);

                if (result)
                {
                    result = OutputSourceVoas(selectedDocsOnly: false, displayMessageOnFailure: true);
                }

                return result;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45736");
            }
            finally
            {
                waitCursor.Dispose();
            }
        }

        /// <summary>
        /// Outputs the data pagination and index data for all source documents without
        /// first updating the index data from each document.
        /// NOTE: A message will be displayed if the pages from multiple source documents
        /// are currently combined into a single proposed output document and the method will return false.
        /// </summary>
        /// <param name="selectedDocsOnly"><c>true</c> if only the source documents relating to selected
        /// output documents should be saved. <c>false</c> to save all source documents represented in
        /// the UI.</param>
        /// <param name="displayMessageOnFailure"><c>true</c> to display a message and return false
        /// if there are merges not compatible to be saved. <c>false</c> if an exception should
        /// be thrown instead.</param>
        /// <returns><c>true</c> if the data was saved; otherwise, <c>false</c>.</returns>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Voas")]
        public bool OutputSourceVoas(bool selectedDocsOnly, bool displayMessageOnFailure)
        {
            try
            {
                var affectedDocs = selectedDocsOnly
                    ? _displayedDocuments.Where(doc => doc.Selected)
                    : _displayedDocuments;

                var affectedSources = new HashSet<SourceDocument>(
                    affectedDocs.SelectMany(doc => doc.PageControls
                        .Select(pageControl => pageControl.Page.SourceDocument)
                        .Where(sourceDoc => sourceDoc != null)
                        .Distinct()));

                // List affected source docs => each output doc
                // (for all output documents involved with affectedSources)
                var sourcesToOutputMapping = _displayedDocuments
                    .Where(doc => doc.PageControls.Any(pageControl => 
                        affectedSources.Contains(pageControl.Page.SourceDocument)))
                    .Select(doc => 
                    (sourceDocs:
                        doc.PageControls
                            .Select(pageControl => pageControl.Page.SourceDocument)
                            .Where(sourceDoc => sourceDoc != null)
                            .Distinct()
                            .ToList(),
                    outputDocument: doc)
                ).ToList();

                // Cannot output source voa in the case of merges for any document that has not been
                // applied (pages will not be available when sources are reloaded)
                var unsupportedMergeSourceDocuments = sourcesToOutputMapping
                    .Where(x => !x.outputDocument.OutputProcessed && x.sourceDocs.Count() > 1)
                    .SelectMany(x => x.sourceDocs.Select(y => y.FileName))
                    .Distinct()
                    .ToList();

                if (unsupportedMergeSourceDocuments.Any())
                {
                    if (displayMessageOnFailure)
                    {
                        UtilityMethods.ShowMessageBox("It is not possible to save progress when multiple " +
                            "source documents have been combined into a single output document.",
                            "Unable to save", true);
                    }
                    else
                    {
                        var ee = new ExtractException("ELI47247", "Failed to save merged source documents");
                        unsupportedMergeSourceDocuments.ForEach(sourceDoc => ee.AddDebugData("Merged Source", sourceDoc, false));
                        throw ee;
                    }
                    return false;
                }

                var documentsToSave = new Dictionary<SourceDocument, List<OutputDocument>>();
                foreach (var (sourceDocs, pendingDocument) in sourcesToOutputMapping.Where(entry => entry.sourceDocs.Any()))
                {
                    foreach (var sourceDoc in sourceDocs)
                    {
                        var outputDocs = documentsToSave.GetOrAdd(sourceDoc, _ => new List<OutputDocument>());
                        outputDocs.Add(pendingDocument);
                    }
                }

                var orderedDocuments = _primaryPageLayoutControl.Documents.ToList();

                foreach (var sourceEntry in documentsToSave)
                {
                    var sourceDocName = sourceEntry.Key.FileName;
                    var documentAttributes = new IUnknownVector();

                    foreach (var outputDoc in sourceEntry.Value.OrderBy(doc => orderedDocuments.IndexOf(doc)))
                    {
                        var documentAttribute = GetDocumentAttribute(outputDoc, sourceDocName);
                        documentAttributes.PushBack(documentAttribute);
                    }

                    if (documentAttributes.Size() > 0)
                    {
                        IUnknownVector saveFileData = new IUnknownVector();
                        saveFileData.Append(documentAttributes);
                        saveFileData.SaveTo(sourceEntry.Key.FileName + ".voa", false, _ATTRIBUTE_STORAGE_MANAGER_GUID);
                        saveFileData.ReportMemoryUsage();
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45736");
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
            TemporaryWaitCursor waitCursor = new TemporaryWaitCursor();

            try
            {
                _committingChanges = true;

                if (!CanSelectedDocumentsBeCommitted())
                {
                    return false;
                }

                // Don't suspend UI updates until CanSelectedDocumentsBeCommitted has been checked--
                // there is not flickering to prevent at this point and moving windows in front of
                // a suspended UI can cause artifacts.
                SuspendUIUpdates = true;

                // Prevent the UI from trying to load/close pages in the image viewer as the
                // operation is taking place.
                _primaryPageLayoutControl.ClearSelection();

                // Depending upon the manipulations that occurred, there may be some displayed
                // documents that have been left without any pages. There could also be rare
                // circumstances where all none of the pages in a previously processed document
                // are from a different document. In either case, these documents don't represent
                // an active source document. Disregard these.
                var documentsToRemove = _displayedDocuments
                    .Where(doc => !doc.PageControls.Any(c => c.Page.SourceDocument != null))
                    .ToArray();
                foreach (var document in documentsToRemove)
                {
                    _primaryPageLayoutControl.DeleteOutputDocument(document);
                    _displayedDocuments.Remove(document);
                }

                var documentsToCommit = new HashSet<OutputDocument>(PendingDocuments
                    .Where(document => !CommitOnlySelection || document.Selected));
                var documentsInSourceForm = new HashSet<OutputDocument>(documentsToCommit
                    .Where(document => document.InSourceDocForm));
                var documentsNotInSourceForm = new HashSet<OutputDocument>(documentsToCommit
                    .Except(documentsInSourceForm));

                var copies = new HashSet<OutputDocument>(documentsInSourceForm
                    .GroupBy(doc =>
                        doc.PageControls
                           // Documents can be in source form even with deleted pages from some other document
                           .First(c => !c.Deleted)
                           .Page.OriginalDocumentName)
                    .SelectMany(g => g.Skip(1)));

                // Remove copies from source form collection since any copy will need to be created
                documentsInSourceForm.ExceptWith(copies);

                // Documents not in source form and copies of source-form document will need to be created
                var outputDocuments = documentsNotInSourceForm.Union(copies);

                _outputDocumentPositions = outputDocuments
                    .ToDictionary(
                        doc => doc, doc => _primaryPageLayoutControl.GetDocumentPosition(doc));

                // https://extract.atlassian.net/browse/ISSUE-14372
                // Compile any source documents that will not present in the UI anymore after this
                // call. This may include documents from documentsToRemove for which pages haven't
                // been copied to any other document.
                var documentsToRemain = PendingDocuments.Except(documentsToCommit);
                var sourcesOfDocumentsToRemain = documentsToRemain
                    .SelectMany(outputDoc => outputDoc.PageControls)
                    .Select(pageControl => pageControl.Page.SourceDocument)
                    .ToList();
                var missingSourceDocuments = _sourceDocuments.Except(sourcesOfDocumentsToRemain);

                // Calculate source documents that are only source documents, i.e., that are not also expected
                // to continue through the work-flow unmodified
                // These are any sources of documents that are not in source form...
                var sourceDocumentsNotOutput = new HashSet<string>(documentsNotInSourceForm
                    .SelectMany(doc => doc.PageControls)
                    .Select(c => c.Page.OriginalDocumentName));

                // ...plus sources of documents with no pages left
                // https://extract.atlassian.net/browse/ISSUE-13998
                sourceDocumentsNotOutput.UnionWith(missingSourceDocuments
                    .Select(p => p.FileName));

                // ...minus any sources of documents that are in source form
                var sourceFormSourceDocuments = new HashSet<string>(
                    documentsInSourceForm
                        .Select(doc => doc.PageControls
                        // Documents can be in source form even with deleted pages from some other document
                        .First(c => !c.Deleted)
                        .Page.OriginalDocumentName));
                sourceDocumentsNotOutput.ExceptWith(sourceFormSourceDocuments);
                // As it is no longer required for all pages from a given source document to be committed
                // at the same time, don't remove any source documents with pages that will remain.
                sourceDocumentsNotOutput.ExceptWith(sourcesOfDocumentsToRemain.Select(doc => doc.FileName));

                // Generate the paginated output.
                foreach (var outputDocument in outputDocuments)
                {
                    if (CreateDocumentOnOutput && outputDocument.PageControls.Any(c => !c.Deleted))
                    {
                        // Create the physical output document only if configured to do so and there is at least
                        // one non-deleted page.
                        outputDocument.Output();
                    }
                    else
                    {
                        // Even if a physical document was not created, call ProcessOutputDocument
                        // to assign the output document name, write pagination history and queue
                        // the output file.
                        ProcessOutputDocument(outputDocument);

                        // https://extract.atlassian.net/browse/ISSUE-16571
                        // https://extract.atlassian.net/browse/ISSUE-16642
                        // After applying partial pagination, this document should no longer be
                        // reverted from this state via the "Restore as originally loaded" button.
                        outputDocument.SetOriginalForm();
                        outputDocument.DocumentData.SetOriginalForm();

                        // https://extract.atlassian.net/browse/ISSUE-16581
                        // Upon committing a document, add it to the original document list if it is not
                        // there already. This is needed for the output to be represented in the case
                        // of "Restore as originally loaded".
                        foreach (var originalDocSet in outputDocument.PageControls
                            .Select(c => c.Page.SourceDocument)
                            .Distinct()
                            .Select(s => _sourceToOriginalDocuments[s]))
                        {
                            originalDocSet.Add(outputDocument);
                        }
                    }

                    outputDocument.Collapsed = true;
                }

                // https://extract.atlassian.net/browse/ISSUE-16645
                // Ensure any documents with unmodified pagination are handled so that pagination history, etc can be recorded.
                foreach (var document in documentsToCommit.Except(outputDocuments))
                {
                    OnAcceptedSourcePagination(new AcceptedSourcePaginationEventArgs(document.SourcePageInfo, document.DocumentData));
                }

                LinkFilesWithRecordIDs(documentsToCommit);

                var disregardedPagination = documentsInSourceForm
                    .Where(doc => doc.PaginationSuggested)
                    .Select(doc => doc.PageControls
                        // Documents can be in source form even with deleted pages from some other document
                        .First(c => !c.Deleted)
                        .Page.OriginalDocumentName);

                var unmodifiedPagination = documentsInSourceForm
                    .Select(doc => new KeyValuePair<string, PaginationDocumentData>(
                        doc.PageControls
                        // Documents can be in source form even with deleted pages from some other document
                        .First(c => !c.Deleted)
                        .Page.OriginalDocumentName, doc.DocumentData));

                OnPaginated(sourceDocumentsNotOutput,
                    disregardedPagination,
                    unmodifiedPagination);

                // For any documents did not have either manual or suggested pagination, reset all
                // data about these documents so that nothing about the document is marked dirty.
                foreach (var document in PendingDocuments
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
                    _committingChanges = false;
                    SuspendUIUpdates = false;
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

                waitCursor.Dispose();
            }
        }

        /// <summary>
        /// Links <see cref="documentsToCommit"/> to whatever order/encounter numbers are reported.
        /// </summary>
        /// <param name="documentsToCommit">The documents to commit.</param>
        void LinkFilesWithRecordIDs(HashSet<OutputDocument> documentsToCommit)
        {
            using (var famData = new FAMData(FileProcessingDB))
            {
                foreach (var document in documentsToCommit
                    .Where(doc => doc.DocumentData.Orders?.Any() == true
                        && doc.PageControls.Any(c => !c.Deleted)))
                {
                    if (document.FileID == -1)
                    {
                        new ExtractException("ELI45615", "Failed to link record ID to document.").Display();
                    }

                    foreach (var order in document.DocumentData.Orders)
                    {
                        famData.LinkFileWithOrder(document.FileID, order.OrderNumber, order.OrderDate);
                    }
                }

                foreach (var document in documentsToCommit
                   .Where(doc => doc.DocumentData.Encounters?.Any() == true
                        && doc.PageControls.Any(c => !c.Deleted)))
                {
                    if (document.FileID == -1)
                    {
                        new ExtractException("ELI45616", "Failed to link record ID to document.").Display();
                    }

                    foreach (var encounter in document.DocumentData.Encounters)
                    {
                        famData.LinkFileWithEncounter(document.FileID, encounter.EncounterNumber, encounter.EncounterDate);
                    }
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
            if (RequireAllPagesToBeViewed && !AllPagesViewed)
            {
                if (MessageBox.Show(null,
                    "There are pages that have not been viewed. Proceed anyway?",
                    "Unviewed pages", MessageBoxButtons.YesNo, MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button2, 0) == DialogResult.No)
                {
                    return false;
                }
            }

            // Ensure that if any source documents are being merged, all pages from the respective
            // sources are selected.
            var unsupportedMerges = GetUnselectedPagesAffectedByMerge();

            if (unsupportedMerges.unselectedPagesControls.Any())
            {
                using (var msgBox = new CustomizableMessageBox())
                {
                    msgBox.Caption = "Commit error";
                    msgBox.StandardIcon = MessageBoxIcon.Error;
                    msgBox.Text = string.Format(CultureInfo.CurrentCulture,
                        "Cannot commit because the following document(s) were merged but not fully selected: {0}\r\n\r\n" +
                        "Expand selection to include all affected source document pages?",
                        string.Join(", ", unsupportedMerges.mergedSourceDocuments));
                    msgBox.AddButton("Expand selection", "expand", false);
                    msgBox.AddButton("Cancel", "cancel", false);
                    if (msgBox.Show(this) == "expand")
                    {
                        foreach (var separator in unsupportedMerges.unselectedPagesControls
                            .Select(c => c.Document.PaginationSeparator)
                            .Distinct())
                        {
                            separator.DocumentSelectedToCommit = true;
                        }
                    }
                }

                return false;
            }

            // If document data could not be saved, abort the commit operation.
            if (!SaveDocumentData(selectedDocumentsOnly: CommitOnlySelection, validateData: true))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Identifies pages not included in the current selection that are affected by output document(s)
        /// that merge two or more source documents. Such merges cannot be processed if the sources
        /// documents are not entirely selected.
        /// </summary>
        /// <returns></returns>
        (HashSet<string> mergedSourceDocuments, List<PageThumbnailControl> unselectedPagesControls)
        GetUnselectedPagesAffectedByMerge()
        {
            // All merged source documents in the UI.
            var allMergedSourceDocuments = new HashSet<string>(
                PendingDocuments
                    .Select(doc => doc.PageControls
                        .Select(c => c.Page.OriginalDocumentName)
                        .Distinct())
                    .Where(set => set.Count() > 1)
                    .SelectMany(set => set));

            // Only the merged documents affected by the current selection
            var mergedSourceDocuments = new HashSet<string>(
                PendingDocuments
                    .Where(doc => doc.Selected &&
                        doc.PageControls.Any(c => allMergedSourceDocuments.Contains(c.Page.OriginalDocumentName)))
                    .SelectMany(doc => doc.PageControls
                        .Select(c => c.Page.OriginalDocumentName))
                        .Distinct());

            var unselectedPagesControls = new List<PageThumbnailControl>(
                PendingDocuments
                    .Where(doc => !doc.Selected)
                    .SelectMany(doc => doc.PageControls
                        .Where(c =>
                            mergedSourceDocuments.Contains(c.Page.OriginalDocumentName))));

            return (mergedSourceDocuments, unselectedPagesControls);
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

                foreach (var outputDocument in _displayedDocuments.ToArray())
                {
                    _primaryPageLayoutControl.DeleteOutputDocument(outputDocument);
                    _displayedDocuments.Remove(outputDocument);
                }

                SourceDocument[] sourceDocArray;
                lock (_sourceDocumentLock)
                {
                    sourceDocArray = _sourceDocuments.ToArray();
                }

                foreach (var page in sourceDocArray.SelectMany(doc => doc.Pages))
                {
                    page.ImageOrientation = 0;
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
                            outputDocument.Collapsed = false;
                            outputDocument.Selected = false;
                            _primaryPageLayoutControl.LoadOutputDocument(
                                outputDocument, outputDocument.OriginalPages, null, null, false);
                            if (outputDocument.DocumentData != null)
                            {
                                outputDocument.DocumentData.Revert();
                                if (_documentDataPanel != null)
                                {
                                    _documentDataPanel.UpdateDocumentData(outputDocument.DocumentData,
                                        statusOnly: true, displayValidationErrors: false);
                                }
                            }
                            _displayedDocuments.Add(outputDocument);
                        }
                        // Reverting a document for which there was suggested pagination. Rather than
                        // reverting document data, a new document data instance will be needed.
                        else
                        {
                            // CreateOutputDocument will add the document to _displayedDocuments.
                            var outputDocument =
                                ((IPaginationUtility)this).CreateOutputDocument(sourceDocument.FileName);
                            outputDocument.SetOriginalForm();
                            outputDocument.PaginationSuggested = true;
                            outputDocument.Collapsed = false;
                            outputDocument.Selected = false;

                            var args = new DocumentDataRequestEventArgs(sourceDocument.FileName);
                            OnDocumentDataRequest(args);
                            outputDocument.DocumentData = args.DocumentData;

                            _primaryPageLayoutControl.LoadOutputDocument(
                                outputDocument, sourceDocument.Pages, null, null, false);
                        }
                    }
                }
                else
                {
                    foreach (var sourceDocument in sourceDocArray)
                    {
                        var sourcePagesReverted = new List<int>();

                        foreach (var outputDocument in _sourceToOriginalDocuments[sourceDocument]
                            .Where(doc => doc.OriginalPages.Any())
                            .OrderBy(doc =>
                                doc.OriginalPages.Select(page => page.OriginalPageNumber).Min()))
                        {
                            outputDocument.Collapsed = false;
                            outputDocument.Selected = false;

                            _primaryPageLayoutControl.LoadOutputDocument(outputDocument,
                                outputDocument.OriginalPages, outputDocument.OriginalDeletedPages,
                                outputDocument.OriginalViewedPages, true);
                            if (!_displayedDocuments.Contains(outputDocument))
                            {
                                _displayedDocuments.Add(outputDocument);
                            }

                            if (outputDocument.DocumentData != null)
                            {
                                outputDocument.DocumentData.Revert();
                                if (_documentDataPanel != null)
                                {
                                    _documentDataPanel.UpdateDocumentData(outputDocument.DocumentData,
                                        statusOnly: true, displayValidationErrors: false);
                                }
                            }

                            sourcePagesReverted.AddRange(
                                outputDocument.OriginalPages
                                    .Union(outputDocument.OriginalDeletedPages)
                                    .Select(page => page.OriginalPageNumber));
                        }

                        // https://extract.atlassian.net/browse/ISSUE-16571
                        // It is possible that pages had been moved from applied documents to pending documents
                        // and now that the pending documents are reverted have not been included in any
                        // _displayedDocuments added thus far. In this case, create a separate document for these
                        // pages in which all the pages will show as deleted.
                        AddOrphanedPages(sourceDocument.FileName);
                    }
                }

                SuspendUIUpdates = false;
                _primaryPageLayoutControl.PerformFullLayout();
                _primaryPageLayoutControl.SelectFirstPage();
                _primaryPageLayoutControl.Focus();

                UpdateCommandStates();

                OnRevertedChanges();
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
        /// <param name="acceptingPagination">Use <c>true</c> if the file is being committed,
        /// <c>false</c> if the file is being skipped or if processing is stopping.</param>
        /// <returns>Returns the index at which the document existed.
        /// <para><b>Note</b></para>
        /// This is not a document index. The caller should not try to interpret this value;
        /// it's use should be limited to passing as the position argument of
        /// PaginationPanel.LoadFile or SelectDocumentAtPosition.
        /// </returns>
        public int RemoveSourceFile(string fileName, bool acceptingPagination)
        {
            try
            {
                int position = -1;

                // https://extract.atlassian.net/browse/ISSUE-13955
                // I am unclear as to why, but using a direct invoke here to remove the pages rather
                // than ExecuteInUIThread (which BeginInvokes, then waits on the result) caused
                // momentary hangs when closing the combined DE verification task and, in at least
                // one case, an exception.
                FormsMethods.ExecuteInUIThread(this, () =>
                {
                    lock (_sourceDocumentLock)
                    {
                        var sourceDocument = _sourceDocuments
                            .SingleOrDefault(doc => doc.FileName == fileName);
                        if (sourceDocument != null)
                        {
                            var documentsToDelete = _displayedDocuments
                                .Where(doc =>
                                    doc.PageControls
                                    .Any(c => c.Page.SourceDocument == sourceDocument))
                                .ToArray();

                            foreach (var outputDocument in documentsToDelete)
                            {
                                // If the document's data is open for editing, close the panel.
                                if (_primaryPageLayoutControl.DocumentInDataEdit == outputDocument)
                                {
                                    CloseDataPanel(validateData: false);
                                }
                                int docPosition =
                                    _primaryPageLayoutControl.DeleteOutputDocument(outputDocument);
                                if (docPosition != -1)
                                {
                                    position = (position == -1)
                                        ? docPosition
                                        : Math.Min(position, docPosition);
                                }
                                _displayedDocuments.Remove(outputDocument);
                            }

                            // Output expected voa if so configured
                            if (OutputExpectedPaginationAttributesFile && acceptingPagination)
                            {
                                var pathTags = new SourceDocumentPathTags(fileName);
                                var outputFileName = pathTags.Expand(ExpectedPaginationAttributesPath);
                                WriteExpectedAttributes(fileName, outputFileName);
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

                            if (CacheImages)
                            {
                                _imageViewer.UnloadImage(fileName);
                            }
                        }

                        ApplyOrderOfLoadedSourceDocuments();
                    }
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
        /// Opens the data panel for the singly selected document or the first document.
        /// </summary>
        public void OpenDataPanel()
        {
            try
            {
                var selectedDocs = _primaryPageLayoutControl.PartiallySelectedDocuments;

                var targetDocument = selectedDocs.Count() == 1
                    ? selectedDocs.Single()
                    : _primaryPageLayoutControl
                        .Documents
                        .FirstOrDefault();

                if (targetDocument != null
                    && !targetDocument.OutputProcessed
                    && targetDocument != _primaryPageLayoutControl.DocumentInDataEdit)
                {
                    targetDocument.Collapsed = false;

                    // https://extract.atlassian.net/browse/ISSUE-14886
                    // Ensure separator has been assigned to the target document
                    if (targetDocument.PaginationSeparator == null)
                    {
                        _primaryPageLayoutControl.UpdateDocumentSeparator(targetDocument);
                    }
                    targetDocument.PaginationSeparator.OpenDataPanel();

                    ProcessFocusChange(forceUpdate: true);
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI44685");
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
                SaveDocumentData(selectedDocumentsOnly: false, validateData: false);
                EnablePageDisplay = false;
                ImageViewerPageNavigationEnabled = true;
                _primaryPageLayoutControl.ClearSelection();
                Parked = true;
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
                ImageViewerPageNavigationEnabled = false;
                _primaryPageLayoutControl.SelectFirstPage();
                Parked = false;
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
                var outputDocument = new OutputDocument(outputDocumentName, AutoSelectForReprocess);
                outputDocument.DocumentOutputting += HandleOutputDocument_DocumentOutputting;
                outputDocument.DocumentOutput += HandleOutputDocument_DocumentOutput;

                Tuple<bool, PaginationDocumentData> documentData;
                if (_documentData.TryGetValue(originalDocName, out documentData))
                {
                    outputDocument.PaginationSuggested = documentData.Item1;
                    outputDocument.DocumentData = documentData.Item2;
                    if (outputDocument.DocumentData != null)
                    {
                        outputDocument.DocumentStateChanged += HandleDocument_DocumentStateChanged;
                    }
                }
                else
                {
                    var args = new DocumentDataRequestEventArgs(originalDocName);
                    OnDocumentDataRequest(args);
                    outputDocument.DocumentData = args.DocumentData;
                }

                _displayedDocuments.Add(outputDocument);

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
                try
                {
                    if (value != _imageViewer)
                    {
                        if (_imageViewer != null)
                        {
                            _imageViewer.ImageFileClosing -= HandleImageViewer_ImageFileClosing;
                            _imageViewer.Enter -= HandleImageViewer_Enter;
                            _imageViewer.Leave -= HandleImageViewer_Leave;
                            _imageViewer.PreviewKeyDown -= HandleImageViewer_PreviewKeyDown;
                        }

                        _imageViewer = value;

                        if (_imageViewer != null)
                        {
                            _imageViewer.ImageFileClosing += HandleImageViewer_ImageFileClosing;
                            _imageViewer.Enter += HandleImageViewer_Enter;
                            _imageViewer.Leave += HandleImageViewer_Leave;
                            _imageViewer.PreviewKeyDown += HandleImageViewer_PreviewKeyDown;
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI41359");
                }
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

                _saveToolStripButton.Visible = SaveButtonVisible;

                ResetPrimaryPageLayoutControl();
            }
            catch (Exception ex)
            {
                // https://extract.atlassian.net/browse/ISSUE-13885
                // Throw here because this panel will be housed in a parent application that should
                // have it's own try/catch/display and that will need to know of any errors loading
                // this panel.
                throw ex.AsExtract("ELI39561");
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
                if (!_primaryPageLayoutControl.IgnoreShortcutKey && _shortcuts.ProcessKey(keyData))
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
                // If CreateDocumentOnOutput is true, the file has been output at this point
                // according to the PageLayoutControl, but this is actually only to a temporary 
                // file in order to give the owner of this panel the opportunity to determine
                // where it is going before the file is written to the final location.
                outputDocument = (OutputDocument)sender;
                tempFile = _tempFiles[outputDocument];
                ProcessOutputDocument(outputDocument, tempFile.FileName);
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
        /// All processing that should occur after a new pagination output document has been added
        /// to the database (whether or not the physical file has been created yet).
        /// </summary>
        /// <param name="outputDocument">The <see cref="OutputDocument"/> for which output is being
        /// created.</param>
        /// <param name="tempFileName">If the physical output has been created to a temporary filename,
        /// the filename to copy to the permanent output location.</param>
        void ProcessOutputDocument(OutputDocument outputDocument, string tempFileName = null)
        {
            var originalPages = outputDocument.OriginalPages;
            var currentPages = outputDocument.PageControls
                .Where(c => !c.Deleted)
                .Select(c => c.Page);

            bool pagesEqual = Page.PagesAreEqual(originalPages, currentPages);

            var sourcePageInfo = outputDocument.SourcePageInfo;

            int pageCount = currentPages.Count();

            if (pageCount > 0)
            {
                long fileSize = File.Exists(tempFileName)
                    ? new FileInfo(tempFileName).Length
                    : 0; // Default to 0 if output file size is TBD

                bool? suggestedPaginationAccepted = outputDocument.PaginationSuggested
                    ? (bool?)(outputDocument.InOriginalForm ? true : false)
                    : null;

                int position = _outputDocumentPositions[outputDocument];

                var sourcePages = outputDocument.PageControls
                    .Where(c => !c.Deleted)
                    .Select(c => (documentName: c.Page.OriginalDocumentName,
                                  page: c.Page.OriginalPageNumber,
                                  rotation: c.Page.ImageOrientation));
                var rotatedPages = sourcePages
                    .Where(page => page.rotation != 0)
                    .Select(page =>
                        (page.documentName,
                            page.page,
                            page.rotation))
                    .ToList().AsReadOnly();

                bool pagesEqualButRotated = pagesEqual && rotatedPages.Count() > 0;

                var eventArgs = new CreatingOutputDocumentEventArgs(
                    sourcePageInfo, pageCount, fileSize, suggestedPaginationAccepted, position,
                    outputDocument.DocumentData, rotatedPages, pagesEqualButRotated);

                // CreatingOutputDocument will trigger the output document ID and pagination history to
                // be added to the DB (and prevent it from being queued from a watching supplier)
                // https://extract.atlassian.net/browse/ISSUE-13760
                OnCreatingOutputDocument(eventArgs);
                outputDocument.FileID = eventArgs.FileID;
                string outputFileName = eventArgs.OutputFileName;

                if (CreateDocumentOnOutput)
                {
                    ExtractException.Assert("ELI39588",
                        "No filename has been specified for pagination output.",
                        !string.IsNullOrWhiteSpace(outputFileName));

                    File.Copy(tempFileName, outputFileName);
                }
                else
                {
                    IUnknownVector saveFileData = new IUnknownVector();
                    saveFileData.PushBack(GetDocumentAttribute(outputDocument, outputFileName));
                    saveFileData.SaveTo(outputFileName + ".voa", false, _ATTRIBUTE_STORAGE_MANAGER_GUID);
                    saveFileData.ReportMemoryUsage();
                }

                // Will queue the output file per task settings.
                OnOutputDocumentCreated(eventArgs);
            }
            else
            {
                // All pages in the document are deleted; raise OutputDocumentDeleted.
                OnOutputDocumentDeleted(new OutputDocumentDeletedEventArgs(sourcePageInfo, outputDocument.DocumentData));
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
        /// Handles the <see cref="Control.Click"/> event of the <see cref="_saveToolStripButton"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        void HandleSaveToolStripButton_Click(object sender, EventArgs e)
        {
            try
            {
                Save();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45740");
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
                var response = MessageBox.Show(this,
                    "Restore all pages and extracted data to the state at which they existed when " +
                    "first displayed (except for any documents that have already been processed). " +
                    "This may represent the original state or the state saved by a prior user.",
                    "Restore as originally loaded?", MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2, 0);

                if (response == DialogResult.Yes)
                {
                    RevertPendingChanges(revertToSource: false);
                }
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
                var response = MessageBox.Show(this,
                    "Discarding all changes will display all source documents as they were before " +
                    "being processed by the software and will discard all data extracted from " +
                    "those documents.", "Discard all changes?", MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2, 0);

                if (response == DialogResult.Yes)
                {
                    RevertPendingChanges(revertToSource: true);
                }
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
                    if (!CloseDataPanel(validateData: false))
                    {
                        _primaryPageLayoutControl.SelectDocument(_primaryPageLayoutControl.DocumentInDataEdit);
                        return;
                    }

                    // Data is not loaded into the panel until PaginationSeparator.OpenDataPanel
                    // so that the control handles are instantiated first.
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
        /// <see cref="_displayedDocuments"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="NotifyCollectionChangedEventArgs"/> instance containing
        /// the event data.</param>
        void HandleDisplayedDocuments_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            try
            {
                if (e.NewItems != null)
                {
                    foreach (var newDocument in e.NewItems.Cast<OutputDocument>())
                    {
                        newDocument.DocumentStateChanged += HandleDocument_DocumentStateChanged;
                    }
                }

                if (e.OldItems != null)
                {
                    foreach (var oldDocument in e.OldItems.Cast<OutputDocument>())
                    {
                        oldDocument.DocumentStateChanged -= HandleDocument_DocumentStateChanged;
                        var disposableData = oldDocument.DocumentData as IDisposable;
                        if (disposableData != null)
                        {
                            disposableData.Dispose();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39793");
            }
        }

        /// <summary>
        /// Handles the <see cref="OutputDocument.DocumentStateChanged"/> event for all
        /// <see cref="_displayedDocuments"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleDocument_DocumentStateChanged(object sender, EventArgs e)
        {
            try
            {
                var document = (OutputDocument)sender;

                // Prevent unnecessary updates of separators as document data is being prepared for commit.
                if (_committingChanges && _displayedDocuments.Contains(document))
                {
                    return;
                }

                UpdateCommandStates();
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

        /// <summary>
        /// Handles the <see cref="IPaginationDocumentDataPanel.PageLoadRequest"/> event of the
        /// <see cref="_documentDataPanel"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void HandleDocumentDataPanel_PageLoadRequest(object sender, PageLoadRequestEventArgs e)
        {
            try
            {
                // Only select the page if the DEP is currently intended to have focus. This prevents
                // unexpected page changes when focused is changed between different panels.
                if (_dataPanelFocused)
                {
                    var pageToSelect =
                        (_primaryPageLayoutControl.DocumentInDataEdit?.PageControls ?? _primaryPageLayoutControl.PageControls)
                            .FirstOrDefault(c => e.PageNumber == c.Page.OriginalPageNumber &&
                                c.Page.OriginalDocumentName
                                    .Equals(e.SourceDocName, StringComparison.OrdinalIgnoreCase));

                    if (pageToSelect != null)
                    {
                        // https://extract.atlassian.net/browse/ISSUE-14343
                        // Don't allow the selection of a page scroll away from the DEP that requested the
                        // page change.
                        _primaryPageLayoutControl.SelectPage(pageToSelect, scrollToPage: false);
                    }

                    e.Handled = true;
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI44702");
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.Enter"/> event of the
        /// <see cref="_documentDataPanel.PanelControl"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        void HandlePanelControl_Enter(object sender, EventArgs e)
        {
            try
            {
                ProcessFocusChange(forceUpdate: false);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI44703");
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.Leave"/> event of the
        /// <see cref="_documentDataPanel.PanelControl"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        void HandlePanelControl_Leave(object sender, EventArgs e)
        {
            try
            {
                ProcessFocusChange(forceUpdate: false);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI41667");
            }
        }

        /// <summary>
        /// Handles the <see cref="IPaginationDocumentDataPanel.DataPanelChanged"/> event of the
        /// <see cref="_documentDataPanel"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        void HandleDataPanel_DataPanelChanged(object sender, EventArgs e)
        {
            try
            {
                _primaryPageLayoutControl.SnapDataPanelToTop();

                ProcessFocusChange(forceUpdate: true);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI44809");
            }
        }

        /// <summary>
        /// Handles the <see cref="ImageViewer.ImageFileClosing"/> event of the
        /// <see cref="ImageViewer"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void HandleImageViewer_ImageFileClosing(object sender, ImageFileClosingEventArgs e)
        {
            try
            {
                // ISSUE-14163 Pagination: DE verification swiping tool inappropriately disabled.
                // Only set AllowHighlight false when the pagination panel is the active pane, otherwise
                // this prevents the data panel from correctly enabling the word highlight tool (it enables
                // but highlights are disabled and don't show on the image).
                if (Visible)
                {
                    // Whenever a DEP is closed, ensure the swiping tools are disabled until another
                    // DEP specifically enables them.
                    ImageViewer.AllowHighlight = false;
                    ImageViewer.CursorTool = CursorTool.None;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI41341");
            }
        }

        /// <summary>
        /// Handles the <see cref="Enter"/> event of the <see cref="_imageViewer"/> control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PreviewKeyDownEventArgs"/> instance containing the event data.</param>
        void HandleImageViewer_Enter(object sender, EventArgs e)
        {
            try
            {
                ProcessFocusChange(forceUpdate: false);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI44700");
            }
        }

        /// <summary>
        /// Handles the <see cref="Leave"/> event of the <see cref="_imageViewer"/> control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PreviewKeyDownEventArgs"/> instance containing the event data.</param>
        void HandleImageViewer_Leave(object sender, EventArgs e)
        {
            try
            {
                ProcessFocusChange(forceUpdate: false);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI44701");
            }
        }

        /// <summary>
        /// Handles the <see cref="PreviewKeyDown"/> event of the <see cref="_imageViewer"/> control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PreviewKeyDownEventArgs"/> instance containing the event data.</param>
        void HandleImageViewer_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            try
            {
                if (!_primaryPageLayoutControl.IgnoreShortcutKey && !_dataPanelFocused)
                {
                    _shortcuts.ProcessKey(e.KeyCode);
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI44699");
            }
        }

        #endregion Event Handlers

        #region Private Members

        /// <summary>
        /// The subset of <see cref="_displayedDocuments"/> that are still pending to be applied.
        /// </summary>
        IEnumerable<OutputDocument> PendingDocuments
        {
            get
            {
                return _displayedDocuments?.Where(document => !document.OutputProcessed);
            }
        }

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
                return _displayedDocuments.All(doc => doc.Collapsed
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
            foreach (var document in _displayedDocuments)
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
                    PendingDocuments.Any() &&
                    PendingDocuments.All(doc => doc.PageControls.Count == 0 || doc.Selected);
            }
        }

        /// <summary>
        /// Selects or un-selects all documents for committal.
        /// </summary>
        /// <param name="select"><see langword="true"/> to select all documents for committal or
        /// <see langword="false"/> to clear all the check boxes.</param>
        void SelectAllToCommit(bool select)
        {
            // If selecting, select only documents pending. If clearing selection, ensure already
            // committed documents are de-selected as well.
            foreach (var document in (select ? PendingDocuments: _displayedDocuments))
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
            OnPanelResetting();

            if (_selectAllToCommitCheckBox != null)
            {
                _selectAllToCommitCheckBox.Visible = CommitOnlySelection && SelectAllCheckBoxVisible;
            }

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
            _primaryPageLayoutControl.ExternalOutputOnly = true;
            _primaryPageLayoutControl.ImageViewer = _imageViewer;
            _primaryPageLayoutControl.AutoRotateImages = AutoRotateImages;
            _primaryPageLayoutControl.CommitOnlySelection = CommitOnlySelection;
            _primaryPageLayoutControl.LoadNextDocumentVisible = LoadNextDocumentVisible;
            _primaryPageLayoutControl.DefaultToCollapsed = DefaultToCollapsed;
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
        /// <param name="fileID">The File ID</param>
        /// <returns>A <see cref="SourceDocument"/> representing <see paramref="inputFileName"/> or
        /// <see langword="null"/> if the file is missing or could not be opened.</returns>
        SourceDocument OpenDocument(string inputFileName, int fileID)
        {
            SourceDocument sourceDocument = null;

            lock (_sourceDocumentLock)
            {
                ExtractException.Assert("ELI40278", "Cannot find file", File.Exists(inputFileName),
                    "Filename", inputFileName);

                sourceDocument = _sourceDocuments.SingleOrDefault(doc =>
                    doc.FileName.Equals(inputFileName, StringComparison.OrdinalIgnoreCase));

                if (sourceDocument == null)
                {
                    sourceDocument = new SourceDocument(inputFileName, fileID);
                    if (!sourceDocument.Pages.Any())
                    {
                        sourceDocument.Dispose();
                        return null;
                    }

                    if (sourceDocument.Pages.Count > PageLayoutControl._MAX_LOADED_PAGES)
                    {
                        var ee = new ExtractException("ELI40384",
                            Invariant($"Unable to load more than {PageLayoutControl._MAX_LOADED_PAGES} pages for pagination."));
                        ee.AddDebugData("Filename", inputFileName, false);
                        ee.AddDebugData("Pages", sourceDocument.Pages.Count, false);

                        sourceDocument.Dispose();

                        throw ee;
                    }

                    if (CacheImages)
                    {
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
                    }

                    _sourceDocuments.Add(sourceDocument);
                }
            }

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
        /// Gets a root level "Document" attribute representing the current state of the
        /// <see paramref="outputDoc"/> in the UI. This includes the document pages as well
        /// as the document's data.
        /// </summary>
        /// <param name="outputDoc">The <see cref="OutputDocument"/> for which the document attribute
        /// is required.</param>
        /// <param name="sourceDocName">Name of the source document.</param>
        /// <returns>The root-level "Document" attribute </returns>
        static IAttribute GetDocumentAttribute(OutputDocument outputDoc, string sourceDocName)
        {
            var documentAttribute = new AttributeClass() { Name = "Document" };

            void addPagesSubattribute(string name)
            {
                var pages = outputDoc.PageControls
                    .Where(pageControl =>
                           name == "Pages"
                        || name == "DeletedPages" && pageControl.Deleted
                        || name == "ViewedPages" && pageControl.Viewed
                    )
                    .Select(pageControl => pageControl.Page.OriginalPageNumber);
                if (pages.Any())
                {
                    documentAttribute.AddSubAttribute(name,
                        UtilityMethods.GetPageNumbersAsString(pages),
                        sourceDocName);
                }

            }
            addPagesSubattribute("Pages");
            addPagesSubattribute("DeletedPages");
            addPagesSubattribute("ViewedPages");

            // https://extract.atlassian.net/browse/ISSUE-16631
            // Pagination requests will be used to indicate "applied" documents in the UI even when
            // all pages have been deleted. However, such a pagination request should not be persisted
            // to disk in the case that no output document was actually created.
            if (outputDoc.DocumentData.PaginationRequest != null
                && outputDoc.DocumentData.PaginationRequest.ImagePages.Any())
            {
                documentAttribute.SubAttributes.PushBack(
                    outputDoc.DocumentData.PaginationRequest.GetAsAttribute());
            }

            documentAttribute.SubAttributes.PushBack(outputDoc.DocumentData.DocumentDataAttribute);

            return documentAttribute;
        }

        /// <summary>
        /// Saves all the <see cref="PaginationDocumentData"/> for the documents.
        /// </summary>
        /// <param name="selectedDocumentsOnly"><see langword="true"/> if data should be saved only if
        /// the document is selected for commit, <see langword="false"/> if the data should be saved
        /// regardless.</param>
        /// <param name="validateData"><see langword="true"/> if the document's data should
        /// be validated for errors when saving; otherwise, <see langwor="false"/>.</param>
        /// <returns><see langword="true"/> if the data was saved or <see langword="false"/> if the
        /// data could not be saved and needs to be corrected.
        /// </returns>
        bool SaveDocumentData(bool selectedDocumentsOnly, bool validateData)
        {
            if (_documentDataPanel == null)
            {
                return true;
            }

            var documentsToSave = _displayedDocuments.Where(document =>
                document.PageControls.Any(c => !c.Deleted)
                && !document.OutputProcessed
                && (!selectedDocumentsOnly || document.Selected))
                .ToArray();

            if (IsDataPanelOpen && documentsToSave.Contains(_primaryPageLayoutControl.DocumentInDataEdit))
            {
                if (!CloseDataPanel(validateData))
                {
                    // It is the responsibility of the DocumentDataPanel to inform the user of
                    // any data issues that need correction if the panel is open.
                    return false;
                }
            }

            if (documentsToSave.Length > 0)
            {
                string sourceFileName = null;
                foreach (var document in documentsToSave)
                {
                    sourceFileName = document.PageControls
                        .Select(c => c.Page.SourceDocument?.FileName)
                        .FirstOrDefault(n => n != null);

                    if (sourceFileName != null)
                    {
                        SavingData?.Invoke(this, new SavingDataEventArgs(sourceFileName));
                        _documentDataPanel.UpdateDocumentData(document.DocumentData, statusOnly: false,
                            displayValidationErrors: validateData);
                    }
                }

                if (sourceFileName != null)
                {
                    _documentDataPanel.WaitForDocumentStatusUpdates();

                    DoneSavingData?.Invoke(this, new SavingDataEventArgs(sourceFileName));
                }
            }

            if (validateData)
            {
                try
                {
                    SuspendUIUpdates = false;

                    var erroredDocument = documentsToSave.FirstOrDefault(document => document.DocumentData.DataError);

                    if (erroredDocument != null)
                    {
                        this.SafeBeginInvoke("ELI41669", () =>
                        {
                            _primaryPageLayoutControl.SelectDocument(erroredDocument);
                            erroredDocument.PaginationSeparator.OpenDataPanel();
                            erroredDocument.Collapsed = false;
                            ProcessFocusChange(forceUpdate: true);
                        });

                        return false;
                    }

                    if (!ConfirmRecordNumberReuse(
                            documentsToSave.Where(document => document.DocumentData.PromptForDuplicateOrders),
                            document => document.DocumentData.Orders?.Select(order => order.OrderNumber),
                            "LabDEOrderFile", "OrderNumber", "order number"))
                    {
                        return false;
                    }

                    if (!ConfirmRecordNumberReuse(
                            documentsToSave.Where(document => document.DocumentData.PromptForDuplicateEncounters),
                            document => document.DocumentData.Encounters?.Select(encounter => encounter.EncounterNumber),
                            "LabDEEncounterFile", "EncounterID", "encounter number"))
                    {
                        return false;
                    }
                }
                finally
                {
                    SuspendUIUpdates = true;
                }
            }

            return true;
        }

        /// <summary>
        /// Checks for duplicate order/encounter numbers in <see cref="documents"/> and also checks
        /// to see if any of the order/encounter numbers have been previously linked. If so, a prompt
        /// will be displayed and if the user choses not to save, the documents with used/duplicate
        /// numbers will be indicated.
        /// </summary>
        /// <returns><c>true</c> if there are no used/duplicate numbers or if the user chose to
        /// to commit anyway; <c>false</c> if the user chose to abort the commit.</returns>
        bool ConfirmRecordNumberReuse(IEnumerable<OutputDocument> documents,
                    Func<OutputDocument, IEnumerable<string>> numberSelector,
                    string tableName, string fieldName, string numberLabel)
        {
            var docsWithRecordNumbers = documents
                .Where(document => numberSelector(document)
                    ?.Any(numbers => !string.IsNullOrWhiteSpace(numbers)) == true);

            if (docsWithRecordNumbers.Any())
            {
                var allOrderNumbers = docsWithRecordNumbers
                    .SelectMany(document => numberSelector(document));

                var docsWithDuplicateRecordNumbers = docsWithRecordNumbers
                    .Select(document =>
                        (Document: document,
                         DuplicateNumbers: numberSelector(document)
                                .Where(x => allOrderNumbers.Count(y => (x == y)) >= 2)
                        ))
                    .Where(item => item.DuplicateNumbers.Any());

                if (docsWithDuplicateRecordNumbers.Any())
                {
                    var message = string.Join("\r\n",
                    docsWithDuplicateRecordNumbers.Select(item =>
                        Invariant($"{item.Document.Summary} ({string.Join(", ", item.DuplicateNumbers)})")));

                    message =
                        Invariant($"The following {numberLabel}(s) have been used multiple times:\r\n\r\n") +
                        message +
                        "\r\n\r\nSubmit documents anyway?";

                    using (CustomizableMessageBox messageBox = new CustomizableMessageBox())
                    {
                        messageBox.Caption = "Warning";
                        messageBox.StandardIcon = MessageBoxIcon.Exclamation;
                        messageBox.Text = message;
                        messageBox.AddStandardButtons(MessageBoxButtons.YesNo);
                        if (messageBox.Show(this) == "Yes")
                        {
                            foreach (var documentData in documents
                                .Select(document => document.DocumentData as DataEntryPaginationDocumentData))
                            {
                                documentData.SetDataError(false);
                            }
                            // Clear any error icons added for duplicate record numbers.
                            Refresh();
                        }
                        else
                        {
                            foreach (var documentData in docsWithDuplicateRecordNumbers
                                .Select(item => item.Document.DocumentData as DataEntryPaginationDocumentData))
                            {
                                documentData.SetDataError(true);
                                documentData.SetDataErrorMessage(
                                    Invariant($"This document has a duplicate {numberLabel}."));
                            }

                            return false;
                        }
                    }
                }

                var quotedNumbers = allOrderNumbers.Select(n => "'" + n.Replace("'", "''") + "'");

                string query = Invariant($"Select DISTINCT [{fieldName}] FROM [{tableName}] ") +
                    Invariant($"WHERE [{fieldName}] IN ({string.Join(",", quotedNumbers)})");

                HashSet<string> usedNumbers;
                using (var usedNumberTable = GetResultsForQuery(query))
                {
                    usedNumbers = new HashSet<string>(
                        usedNumberTable.Rows
                            .OfType<DataRow>()
                            .Select(r => (string)r[0]));
                }

                var docsWithUsedRecordNumbers = docsWithRecordNumbers
                    .Select(document =>
                        (Document: document,
                         UsedNumbers: numberSelector(document)
                            .Where(x => usedNumbers.Contains(x)))
                        )
                    .Where(item => item.UsedNumbers?.Any() == true);

                if (docsWithUsedRecordNumbers.Any())
                {
                    var message = string.Join("\r\n",
                    docsWithUsedRecordNumbers.Select(item =>
                        Invariant($"{item.Document.Summary} ({string.Join(", ", item.UsedNumbers)})")));

                    message =
                        Invariant($"Documents have already been filed against the following {numberLabel}(s):\r\n\r\n") +
                        message +
                        "\r\n\r\nSubmit document(s) anyway?";

                    using (CustomizableMessageBox messageBox = new CustomizableMessageBox())
                    {
                        messageBox.Caption = "Warning";
                        messageBox.StandardIcon = MessageBoxIcon.Exclamation;
                        messageBox.Text = message;
                        messageBox.AddStandardButtons(MessageBoxButtons.YesNo);
                        if (messageBox.Show(this) == "Yes")
                        {
                            foreach (var documentData in documents
                                .Select(document => document.DocumentData as DataEntryPaginationDocumentData))
                            {
                                documentData.SetDataError(false);
                                // Clear any error icons added for duplicate record numbers.
                                Refresh();
                            }
                        }
                        else
                        {
                            foreach (var documentData in docsWithUsedRecordNumbers
                                .Select(item => item.Document.DocumentData as DataEntryPaginationDocumentData))
                            {
                                documentData.SetDataError(true);
                                documentData.SetDataErrorMessage(
                                    Invariant($"This document is using an {numberLabel} that previous document(s) have been filed against."));
                            }

                            return false;
                        }
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Closes the <see cref="DocumentDataPanel"/> (if visible), applying any changed data in
        /// the process.
        /// </summary>
        /// <param name="validateData"><see langword="true"/> if the document's data should
        /// be validated for errors when saving; otherwise, <see langword="false"/>.</param>
        /// <returns><see langword="true"/> if the data panel was successfully closed,
        /// <see langword="false"/> if it could not be closed due to an error in the data.</returns>
        public bool CloseDataPanel(bool validateData)
        {
            try
            {
                if (DocumentDataPanel != null)
                {
                    var panelControl = (Control)DocumentDataPanel;
                    var separator = panelControl.GetAncestors()
                        .OfType<PaginationSeparator>()
                        .FirstOrDefault();
                    if (separator != null)
                    {
                        bool closed = separator.CloseDataPanel(saveData: true, validateData: validateData);
                        ProcessFocusChange(forceUpdate: true);
                        return closed;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI41699");
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
                if (!SuspendUIUpdates)
                {
                    _updatingCommandStates = true;

                    var nonEmptyDocs = PendingDocuments.Where(doc =>
                        doc.PageControls.Any()).ToList();
                    var docsWithNonDeletedPages = nonEmptyDocs.Where(doc =>
                        doc.PageControls.Any(c => !c.Deleted));
                    bool isDocDataEdited = docsWithNonDeletedPages.Any(doc =>
                        doc.DocumentData != null && doc.DocumentData.Modified);
                    bool documentCopyExists = nonEmptyDocs.Count > _originalDocuments.Count();

                    // Removed check of doc.PaginationSuggested here so that auto-rotated images can also
                    // register as if it is a pagination suggestion in terms of reverting.
                    RevertToSuggestedEnabled =
                        nonEmptyDocs.Any(doc => isDocDataEdited || documentCopyExists || !doc.InOriginalForm);
                    _revertToOriginalToolStripButton.Enabled = RevertToSuggestedEnabled;

                    RevertToSourceEnabled = !_displayedDocuments.Any(doc => doc.OutputProcessed) &&
                        (isDocDataEdited || documentCopyExists || nonEmptyDocs.Any(doc => !doc.InSourceDocForm));
                    _revertToSourceToolStripButton.Enabled = RevertToSourceEnabled;

                    _saveToolStripButton.Enabled = PendingDocuments.Any();
                    _applyToolStripButton.Enabled = CommitEnabled;
                    _collapseAllToolStripButton.Image =
                        AllDocumentsCollapsed
                            ? Properties.Resources.Expand
                            : Properties.Resources.Collapse;
                    if (CommitOnlySelection)
                    {
                        _selectAllToCommitCheckBox.Enabled = PendingDocuments.Any();
                        _selectAllToCommitCheckBox.Checked = AllDocumentsSelected;
                    }
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
                        // Document loads that being in the midst committing documents was turning off
                        // SuspendUIUpdates and allowing flickering to occur.
                        else if (!_committingChanges)
                        {
                            _uiUpdatesSuspended = false;
                            EnablePageDisplay = true;
                            _primaryPageLayoutControl.UIUpdatesSuspended = false;
                            UpdateCommandStates();
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
        /// Gets or sets whether page navigation via ImageViewer shortcuts should be enabled.
        /// </summary>
        bool ImageViewerPageNavigationEnabled
        {
            get
            {
                return _disabledImageViewerShortcuts.Count == 0;
            }

            set
            {
                if (ImageViewerPageNavigationEnabled != value)
                {
                    if (value)
                    {
                        ImageViewer.Shortcuts[Keys.PageUp] = _disabledImageViewerShortcuts[Keys.PageUp];
                        ImageViewer.Shortcuts[Keys.PageDown] = _disabledImageViewerShortcuts[Keys.PageDown];
                        ImageViewer.Shortcuts[Keys.Control | Keys.Home] = _disabledImageViewerShortcuts[Keys.Control | Keys.Home];
                        ImageViewer.Shortcuts[Keys.Control | Keys.End] = _disabledImageViewerShortcuts[Keys.Control | Keys.End];
                        _disabledImageViewerShortcuts.Clear();
                    }
                    else
                    {
                        _disabledImageViewerShortcuts[Keys.PageUp] = ImageViewer.Shortcuts[Keys.PageUp];
                        _disabledImageViewerShortcuts[Keys.PageDown] = ImageViewer.Shortcuts[Keys.PageDown];
                        _disabledImageViewerShortcuts[Keys.Control | Keys.Home] = ImageViewer.Shortcuts[Keys.Control | Keys.Home];
                        _disabledImageViewerShortcuts[Keys.Control | Keys.End] = ImageViewer.Shortcuts[Keys.Control | Keys.End];
                        ImageViewer.Shortcuts[Keys.PageUp] = null;
                        ImageViewer.Shortcuts[Keys.PageDown] = null;
                        ImageViewer.Shortcuts[Keys.Control | Keys.Home] = null;
                        ImageViewer.Shortcuts[Keys.Control | Keys.End] = null;
                    }
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
                // Select records for any files that were paginated
                string query = string.Format(CultureInfo.CurrentCulture,
                    @"SELECT [Pages],[SourcePage],[DestFileID],[DestPage],[FileTaskSessionID] " +
                     "FROM [FAMFile] JOIN [Pagination] " +
                     "ON [Pagination].[SourceFileID] = [FAMFile].[ID] " +
                     "WHERE [FileName] = '{0}' " +
                     // Do not select records that represent multiple generations of pagination
                     // (i.e., the source is also a destination)
                     // https://extract.atlassian.net/browse/ISSUE-14923
                     "AND [SourceFileID] = [OriginalFileID]"
                     , sourceDocName.Replace("'", "''"));

                var records = GetResultsForQuery(query).Rows.Cast<DataRow>();

                if (!records.Any())
                {
                    return;
                }

                var documents = new List<(string pages, string deletedPages)>();
                var lastSession = records.Max(r => r["FileTaskSessionID"] as int?);
                var flags = new List<string>();

                // If no pagination occurred, then the expected data should be a single document for
                // the entire range
                if (lastSession == null)
                {
                    int numberOfPages = (int)records.First()["Pages"];
                    documents.Add((Enumerable.Range(1, numberOfPages).ToRangeString(), ""));
                }
                // Otherwise, output one document for each destination file
                // Output each page, deleted or not, that appeared as part of the output document
                // and also collect deleted pages (they will be counted twice)
                else
                {
                    var outputDocuments = records
                        .Where(r => r["FileTaskSessionID"] as int? == lastSession)
                        .GroupBy(r => r["DestFileID"] as int?,
                                 r => (sourcePage: (int)r["SourcePage"], // Select only the source pages for each group
                                       destPage: r["DestPage"] as int?));

                    foreach (var documentPages in outputDocuments)
                    {
                        var allPages = documentPages
                            .Where(x => x.destPage != null)
                            .OrderBy(x => x.destPage)
                            .Select(x => x.sourcePage)
                            .ToList();
                        var deletedPages = documentPages
                            .Where(x => x.destPage == null)
                            .OrderBy(x => x.sourcePage)
                            .Select(x => x.sourcePage)
                            .ToList();
                        // Insert deleted pages where they make the most sense (to make ranges clean)
                        foreach(var deletedPage in deletedPages)
                        {
                            var i = allPages.FindIndex(page => page > deletedPage);
                            if (i >= 0)
                            {
                                allPages.Insert(i, deletedPage);
                            }
                            else
                            {
                                allPages.Add(deletedPage);
                            }
                        }
                        documents.Add((allPages.ToRangeString(), deletedPages.ToRangeString()));

                        // Generate flags for unusual situations that won't be handled
                        // well by a pagination LM
                        // https://extract.atlassian.net/browse/ISSUE-14923
                        var sourceOrdering = allPages.OrderBy(p => p).ToList();
                        if (!allPages.SequenceEqual(sourceOrdering))
                        {
                            flags.Add("Pages have been rearranged");
                        }
                    }

                    // Generate a flag for the case where a page appears multiple times
                    // in the output
                    // https://extract.atlassian.net/browse/ISSUE-14923
                    var sourcePages = outputDocuments
                        .SelectMany(g => g.Select(x => x.sourcePage))
                        .ToList();
                    if (sourcePages.Distinct().Count() < sourcePages.Count)
                    {
                        flags.Add("Pages have been duplicated");
                    }
                }

                // Create the hierarchy
                documents.Select(doc =>
                {
                    var ss = new SpatialStringClass();
                    ss.CreateNonSpatialString(_DOCUMENT_PLACEHOLDER_TEXT, sourceDocName);
                    var documentAttribute = new ComAttribute { Name = "Document", Value = ss };

                    // Add a Pages attribute to denote the range(s) of pages in this document
                    if (!string.IsNullOrEmpty(doc.pages))
                    {
                        ss = new SpatialStringClass();
                        ss.CreateNonSpatialString(doc.pages, sourceDocName);
                        documentAttribute.SubAttributes.PushBack(new ComAttribute { Name = "Pages", Value = ss });
                    }

                    // Add a DeletedPages attribute to denote the range(s) of deleted pages in this document
                    if (!string.IsNullOrEmpty(doc.deletedPages))
                    {
                        ss = new SpatialStringClass();
                        ss.CreateNonSpatialString(doc.deletedPages, sourceDocName);
                        documentAttribute.SubAttributes.PushBack(new ComAttribute { Name = "DeletedPages", Value = ss });
                    }
                    return documentAttribute;
                })

                // Add flags for unusual situations
                // https://extract.atlassian.net/browse/ISSUE-14923
                .Concat(flags.Distinct().Select(flag =>
                    {
                        var ss = new SpatialStringClass();
                        ss.CreateNonSpatialString(flag, sourceDocName);
                        return new ComAttribute
                        {
                            Name = LearningMachineDataEncoder.IncompatibleWithPaginationTrainingAttributeName,
                            Value = ss
                        };
                    }
                ))
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
        /// Handles focus changes between the DEP, PageLayoutControl and ImageViewer to ensure
        /// current focus is properly indicated.
        /// </summary>
        /// <param name="forceUpdate"><c>true</c> to force the focus status to be updated in cases
        /// where the panel itself may have changed without interaction from this class; <c>false</c>
        /// if the focus status should be updated only if known to have changed.</param>
        void ProcessFocusChange(bool forceUpdate)
        {
            // If neither the DEP nor _primaryPageLayoutControl conclusively has focus (i.e., image
            // viewer likely has focus), leave _dataPanelFocused as it currently is.
            bool focusIsKnown = !IsDataPanelOpen
                || ContainsFocus
                || (_documentDataPanel?.PanelControl?.ContainsFocus == true);

            if (focusIsKnown)
            {
                bool dataPanelFocused = IsDataPanelOpen
                    && (_documentDataPanel?.PanelControl?.ContainsFocus == true);

                if (forceUpdate || dataPanelFocused != _dataPanelFocused)
                {
                    _dataPanelFocused = dataPanelFocused;

                    var dataEntryContainer = _documentDataPanel as DataEntryPanelContainer;
                    _primaryPageLayoutControl.IndicateFocus = !_dataPanelFocused;
                    var activePanel = dataEntryContainer.ActiveDataEntryPanel;
                    if (activePanel != null)
                    {
                        // Invoke rather than call directly so that the control that was clicked has the
                        // opportunity to register the click before we ask the panel to indicate focus.
                        // If we set IndicateFocus before the clicked control can register the click, the
                        // it may not trigger the proper page to be opened in the image viewer.
                        this.SafeBeginInvoke("ELI44704", () => activePanel.IndicateFocus = _dataPanelFocused);
                    }
                }
            }
        }

        /// <summary>
        /// Raises the <see cref="DocumentDataRequest"/> event.
        /// </summary>
        /// <param name="eventArgs">The <see cref="DocumentDataRequestEventArgs"/> instance containing
        /// the event data.</param>
        void OnDocumentDataRequest(DocumentDataRequestEventArgs eventArgs)
        {
            DocumentDataRequest?.Invoke(this, eventArgs);
        }

        /// <summary>
        /// Raises the <see cref="LoadNextDocument"/> event.
        /// </summary>
        void OnLoadNextDocument()
        {
            LoadNextDocument?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Raises the <see cref="CreatingOutputDocument"/> event.
        /// </summary>
        /// <param name="eventArgs">The <see cref="CreatingOutputDocumentEventArgs"/> instance to
        /// use when raising the event.</param>
        void OnCreatingOutputDocument(CreatingOutputDocumentEventArgs eventArgs)
        {
            CreatingOutputDocument?.Invoke(this, eventArgs);
        }

        /// <summary>
        /// Raises the <see cref="OutputDocumentCreated"/> event.
        /// </summary>
        /// <param name="eventArgs">The <see cref="CreatingOutputDocumentEventArgs"/> instance to
        /// use when raising the event.</param>
        private void OnOutputDocumentCreated(CreatingOutputDocumentEventArgs eventArgs)
        {
            OutputDocumentCreated?.Invoke(this, eventArgs);
        }

        /// <summary>
        /// Raises the <see cref="OutputDocumentDeleted"/> event.
        /// </summary>
        /// <param name="eventArgs">The <see cref="OutputDocumentDeletedEventArgs"/> instance to
        /// use when raising the event.</param>
        private void OnOutputDocumentDeleted(OutputDocumentDeletedEventArgs eventArgs)
        {
            OutputDocumentDeleted?.Invoke(this, eventArgs);
        }

        /// <summary>
        /// Raises the <see cref="AcceptedSourcePagination"/> event.
        /// </summary>
        /// <param name="eventArgs">The <see cref="AcceptedSourcePaginationEventArgs"/> instance to
        /// use when raising the event.</param>
        private void OnAcceptedSourcePagination(AcceptedSourcePaginationEventArgs eventArgs)
        {
            AcceptedSourcePagination?.Invoke(this, eventArgs);
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
        /// <param name="unmodifiedPaginationSources">All documents names and associated
        /// <see cref="PaginationDocumentData"/> where the document pages have not been modified.
        /// </param>
        void OnPaginated(IEnumerable<string> paginatedDocumentSources,
            IEnumerable<string> disregardedPaginationSources,
            IEnumerable<KeyValuePair<string, PaginationDocumentData>> unmodifiedPaginationSources)
        {
            Paginated?.Invoke(this,
                new PaginatedEventArgs(
                    paginatedDocumentSources, disregardedPaginationSources,
                    unmodifiedPaginationSources));
        }

        /// <summary>
        /// Raises the <see cref="PaginationError"/> event.
        /// </summary>
        /// <param name="ee">The exception that has been thrown from pagination.
        /// </param>
        void OnPaginationError(ExtractException ee)
        {
            PaginationError?.Invoke(this, new ExtractExceptionEventArgs(ee));
        }

        /// <summary>
        /// Raises the <see cref="StateChanged"/> event.
        /// </summary>
        void OnStateChanged()
        {
            StateChanged?.Invoke(this, new EventArgs());
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

        /// <summary>
        /// Raises the <see cref="PanelResetting"/> event.
        /// </summary>
        void OnPanelResetting()
        {
            PanelResetting?.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Raises the <see cref="RevertedChanges"/> event.
        /// </summary>
        void OnRevertedChanges()
        {
            RevertedChanges?.Invoke(this, new EventArgs());
        }

        #endregion Private Members
    }
}
