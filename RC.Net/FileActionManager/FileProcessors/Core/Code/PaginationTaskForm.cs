using Extract.AttributeFinder;
using Extract.FileActionManager.Forms;
using Extract.Imaging;
using Extract.Licensing;
using Extract.Utilities;
using Extract.Utilities.Forms;
using Extract.UtilityApplications.PaginationUtility;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using UCLID_AFCORELib;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;
using UCLID_RASTERANDOCRMGMTLib;

namespace Extract.FileActionManager.FileProcessors
{
    /// <summary>
    /// Represents a task that displays a form allowing the user to paginate files.
    /// </summary>
    [CLSCompliant(false)]
    public sealed partial class PaginationTaskForm : Form, IVerificationForm
    {
        #region Constants

        /// <summary>
        /// The full path to the file that contains information about persisting the 
        /// <see cref="PaginationTaskForm"/>.
        /// </summary>
        static readonly string _FORM_PERSISTENCE_FILE = FileSystemMethods.PathCombine(
            FileSystemMethods.UserApplicationDataPath, "PaginateFiles", "PaginateFilesTaskForm.xml");

        /// <summary>
        /// The title to display for the paginate files task form.
        /// </summary>
        const string _FORM_TASK_TITLE = "Paginate Files";

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(PaginationTaskForm).ToString();

        /// <summary>
        /// Name for the mutex used to serialize persistence of the control and form layout.
        /// </summary>
        static readonly string _MUTEX_STRING = "5502932C-C214-4256-A866-60F7C35A7A26";

        /// <summary>
        /// A string representation of the GUID of the data entry verification task.
        /// </summary>
        static readonly string _PAGINATION_TASK_GUID = typeof(PaginationTask).GUID.ToString("B");

        /// <summary>
        /// A string representation of the GUID for <see cref="AttributeStorageManagerClass"/> 
        /// </summary>
        static readonly string _ATTRIBUTE_STORAGE_MANAGER_GUID =
            typeof(AttributeStorageManagerClass).GUID.ToString("B");

        #endregion Constants

        #region Fields

        /// <summary>
        /// A <see cref="PaginationTask"/> representing the settings to use in the form.
        /// </summary>
        PaginationTask _settings;

        /// <summary>
        /// The file processing database.
        /// </summary>
        FileProcessingDB _fileProcessingDB;

        /// <summary>
        /// The <see cref="IFileRequestHandler"/> that can be used by the verification task to carry
        /// out requests for files to be checked out, released or re-ordered in the queue.
        /// </summary>
        IFileRequestHandler _fileRequestHandler;

        /// <summary>
        /// The ID of the file being processed.
        /// </summary>
        int _fileID;

        /// <summary>
        /// The name of the file being processed.
        /// </summary>
        string _fileName;

        /// <summary>
        /// The ID of the action being processed.
        /// </summary>
        int _actionID;

        /// <summary>
        /// The ID of the currently active FileTaskSession row. <see langword="null"/> if there is
        /// no such row.
        /// </summary>
        int? _fileTaskSessionID;

        /// <summary>
        /// The <see cref="ITagUtility"/> interface of the <see cref="FAMTagManager"/> provided to
        /// expand path tags/functions.
        /// </summary>
        ITagUtility _tagUtility;

        /// <summary>
        /// Used to invoke methods on this control.
        /// </summary>
        readonly ControlInvoker _invoker;

        /// <summary>
        /// Saves/restores window state info and provides full screen mode.
        /// </summary>
        FormStateManager _formStateManager;

        /// <summary>
        /// A panel that is available to view/edit key data fields associated with either physical
        /// or proposed paginated documents.
        /// </summary>
        IPaginationDocumentDataPanel _paginationDocumentDataPanel;

        /// <summary>
        /// During a pagination operation in the <see cref="_paginationPanel"/>, the name, position
        /// in the panel, and data of documents that have been output and are to be immediately
        /// loaded back in for verification.
        /// </summary>
        List<Tuple<string, int, PaginationDocumentData>> _paginationOutputToReload =
            new List<Tuple<string, int, PaginationDocumentData>>();

        /// <summary>
        /// Used to prevent simultaneous modification of a voa file from multiple Extract Systems
        /// processes.
        /// </summary>
        Dictionary<string, ExtractFileLock> _voaFileLocks = new Dictionary<string, ExtractFileLock>();

        /// <summary>
        /// The _paginationForm index at which the last page was removed and the next file should
        /// be selected
        /// </summary>
        int _lastDocumentPosition = -1;

        /// <summary>
        /// Indicates when document are in the process of being swapped out to prevent unnecessary
        /// control updates during this process.
        /// </summary>
        bool _changingDocuments;

        #endregion Fields

        #region Events

        /// <summary>
        /// This event indicates the verification form has been initialized and is ready to load a
        /// document.
        /// </summary>
        public event EventHandler<EventArgs> Initialized;

        /// <summary>
        /// Occurs when a file has been "completed" (pagination has been accepted in this case).
        /// </summary>
        public event EventHandler<FileCompleteEventArgs> FileComplete;

        /// <summary>
        /// This event is not raised by <see cref="PaginationTaskForm"/>.
        /// </summary>
        public event EventHandler<FileRequestedEventArgs> FileRequested
        {
            // Since this event is not currently used by this class but is needed by the 
            // IVerificationForm interface, define it with an empty implementation to prevent
            // "unused" warnings during compile.
            add { }
            remove { }
        }

        /// <summary>
        /// Raised when the task request that processing of a specific file be delayed (returned to
        /// the FPRecordManager queue).
        /// </summary>
        public event EventHandler<FileDelayedEventArgs> FileDelayed;

        /// <summary>
        /// Raised when exceptions are raised from the verification UI that should result in the
        /// document failing. Generally this will be raised as a result of errors loading or saving
        /// the document as opposed to interacting with a successfully loaded document.
        /// </summary>
        public event EventHandler<VerificationExceptionGeneratedEventArgs> ExceptionGenerated;

        #endregion Events

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PaginationTaskForm"/> class.
        /// </summary>
        /// <param name="settings">A <see cref="PaginationTask"/> representing the settings to use in
        /// the form.</param>
        /// <param name="paginationDocumentDataPanel">An <see cref="IPaginationDocumentDataPanel"/>
        /// to be used to view and edit data related to a particular document.</param>
        /// <param name="fileProcessingDB">The <see cref="FileProcessingDB"/> in use.</param>
        /// <param name="actionID">The ID of the action being processed.</param>
        /// <param name="tagManager">The <see cref="FAMTagManager"/> to use if needed.</param>
        /// <param name="fileRequestHandler">The <see cref="IFileRequestHandler"/> that can be used
        /// by the task to carry out requests for files to be checked out, released or re-ordered
        /// in the queue.</param>
        public PaginationTaskForm(PaginationTask settings,
            IPaginationDocumentDataPanel paginationDocumentDataPanel, FileProcessingDB fileProcessingDB,
            int actionID, FAMTagManager tagManager, IFileRequestHandler fileRequestHandler)
        {
            try
            {
                if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
                {
                    // Load the license files from folder
                    LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());
                }

                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI40081",
                    _OBJECT_NAME);

                InitializeComponent();

                _settings = settings;
                _paginationDocumentDataPanel = paginationDocumentDataPanel;

                if (tagManager == null)
                {
                    // A FAMTagManager without path tags is better than no tag manager (still can
                    // be used to expand path functions).
                    tagManager = new FAMTagManager();
                }

                _tagUtility = (ITagUtility)tagManager;

                _fileProcessingDB = fileProcessingDB;
                _actionID = actionID;
                _fileRequestHandler = fileRequestHandler;

                _invoker = new ControlInvoker(this);

                if (LicenseManager.UsageMode != LicenseUsageMode.Designtime)
                {
                    _formStateManager = new FormStateManager(
                        this, _FORM_PERSISTENCE_FILE, _MUTEX_STRING, false, null);
                }

                _imageViewer.CacheImages = true;
                _paginationPanel.ImageViewer = _imageViewer;
                _paginationPanel.OutputExpectedPaginationAttributesFile
                    = settings.OutputExpectedPaginationAttributesFiles;
                _paginationPanel.ExpectedPaginationAttributesPath = settings.ExpectedPaginationAttributesOutputPath;
                _paginationPanel.FileProcessingDB = FileProcessingDB;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI40082", ex);
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets whether the control styles of the current Windows theme should be used for the
        /// verification form.
        /// </summary>
        /// <returns><see langword="true"/> to use the control styles of the current Windows theme;
        /// <see langword="false"/> to use Window's classic theme to draw controls.</returns>
        public bool UseVisualStyles
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the verification form should prevent
        /// any attempts to save dirty data. This may be used after experiencing an error or
        /// when the form is being programmatically closed. (when prompts to save in response to
        /// events that occur are not appropriate)
        /// </summary>
        /// <value><see langword="true"/> if the verification form should prevent any
        /// attempts to save dirty data; otherwise, <see langword="false"/>.</value>
        public bool PreventSaveOfDirtyData
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether any cancellation of a form closing event should
        /// be disallowed. This is used to ensure that if the FAM requests a verification task to
        /// stop, that the user can't cancel via a save dirty prompt.
        /// </summary>
        /// <value><see langword="true"/> if cancellation of a form closing event should be
        /// disallowed; otherwise <see langword="false"/>.</value>
        public bool PreventCloseCancel
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the <see cref="FileProcessingDB"/> this instance is currently being run against.
        /// </summary>
        public FileProcessingDB FileProcessingDB
        {
            get
            {
                return _fileProcessingDB;
            }
        }

        /// <summary>
        /// Gets the <see cref="IFileRequestHandler"/> that can be used by the verification task to
        /// carry out requests for files to be checked out, released or re-ordered in the queue.
        /// </summary>
        public IFileRequestHandler FileRequestHandler
        {
            get
            {
                return _fileRequestHandler;
            }
        }

        #endregion Properties

        #region Overrides

        /// <summary>
        /// Raises the <see cref="Form.Load"/> event.
        /// </summary>
        /// <param name="e">The event data associated with the <see cref="Form.Load"/> 
        /// event.</param>
        protected override void OnLoad(EventArgs e)
        {
            try
            {
                // Before Loading the state make sure the config is still valid
                // https://extract.atlassian.net/browse/ISSUE-12830
                UserConfigChecker.EnsureValidUserConfigFile();

                base.OnLoad(e);

                // Establish image viewer connections prior to calling base.OnLoad which will
                // potentially remove some IImageViewerControls.
                _imageViewer.EstablishConnections(this);

                // Disable shortcuts for features or functionality that isn't available for this task.
                _imageViewer.Shortcuts[Keys.O | Keys.Control] = null;
                _imageViewer.Shortcuts[Keys.F4 | Keys.Control] = null;
                _imageViewer.Shortcuts[Keys.S | Keys.Control] = null;
                _imageViewer.Shortcuts[Keys.Escape] = null;
                _imageViewer.Shortcuts[Keys.H] = null;
                _imageViewer.Shortcuts[Keys.OemPeriod] = null;
                _imageViewer.Shortcuts[Keys.Oemcomma] = null;
                _imageViewer.Shortcuts[Keys.F3] = null;
                _imageViewer.Shortcuts[Keys.Control | Keys.OemPeriod] = null;
                _imageViewer.Shortcuts[Keys.F3 | Keys.Shift] = null;
                _imageViewer.Shortcuts[Keys.Control | Keys.Oemcomma] = null;

                // Re-map image viewer shortcuts to used require modifier keys. Default shortcuts
                // that don't use modifier keys are not used so that data entry is not interfered
                // with.
                _imageViewer.Shortcuts[Keys.P | Keys.Control] = _imageViewer.SelectPrint;
                _imageViewer.Shortcuts[Keys.Alt | Keys.Z] = _imageViewer.SelectZoomWindowTool;
                _imageViewer.Shortcuts[Keys.Alt | Keys.A] = _imageViewer.SelectPanTool;
                _imageViewer.Shortcuts[Keys.Alt | Keys.R] = _imageViewer.SelectSelectLayerObjectsTool;
                _imageViewer.Shortcuts[Keys.Alt | Keys.P] = _imageViewer.ToggleFitToPageMode;
                _imageViewer.Shortcuts[Keys.Alt | Keys.W] = _imageViewer.ToggleFitToWidthMode;
                _imageViewer.Shortcuts[Keys.Control | Keys.Home] = _imageViewer.GoToFirstPage;
                _imageViewer.Shortcuts[Keys.PageDown] = _imageViewer.GoToNextPage;
                _imageViewer.Shortcuts[Keys.PageUp] = _imageViewer.GoToPreviousPage;
                _imageViewer.Shortcuts[Keys.Control | Keys.End] = _imageViewer.GoToLastPage;
                _imageViewer.Shortcuts[Keys.F7] = _imageViewer.SelectZoomIn;
                _imageViewer.Shortcuts[Keys.Add | Keys.Control] = _imageViewer.SelectZoomIn;
                _imageViewer.Shortcuts[Keys.Oemplus | Keys.Control] = _imageViewer.SelectZoomIn;
                _imageViewer.Shortcuts[Keys.F8] = _imageViewer.SelectZoomOut;
                _imageViewer.Shortcuts[Keys.Subtract | Keys.Control] = _imageViewer.SelectZoomOut;
                _imageViewer.Shortcuts[Keys.OemMinus | Keys.Control] = _imageViewer.SelectZoomOut;
                _imageViewer.Shortcuts[Keys.Alt | Keys.Left] = _imageViewer.SelectZoomPrevious;
                _imageViewer.Shortcuts[Keys.Alt | Keys.Right] = _imageViewer.SelectZoomNext;
                _imageViewer.Shortcuts[Keys.R | Keys.Control] = _imageViewer.SelectRotateClockwise;
                _imageViewer.Shortcuts[Keys.R | Keys.Control | Keys.Shift] = _imageViewer.SelectRotateCounterclockwise;

                _paginationPanel.LoadNextDocument += HandlePaginationPanel_LoadNextDocument;

                // May be null if the an IPaginationDocumentDataPanel is not specified to be used in
                // this workflow.
                _paginationPanel.DocumentDataPanel = _paginationDocumentDataPanel;
                if (_paginationDocumentDataPanel != null)
                {
                    _paginationPanel.DocumentDataRequest += HandlePaginationPanel_DocumentDataRequest;
                }

                UpdateControls();

                OnInitialized();
            }
            catch (Exception ex)
            {
                RaiseVerificationException("ELI40083", ex, false);
            }
        }

        /// <summary>
        /// Processes a command key.
        /// </summary>
        /// <param name="msg">The window message to process.</param>
        /// <param name="keyData">The key to process.</param>
        /// <returns><see langword="true"/> if the character was processed by the control; 
        /// otherwise, <see langword="false"/>.</returns>
        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            try
            {
                // Allow the image viewer to handle keyboard input for shortcuts.
                if (_imageViewer.Shortcuts.ProcessKey(keyData))
                {
                    return true;
                }

                // This key was not processed, bubble it up to the base class.
                return base.ProcessCmdKey(ref msg, keyData);
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI40084", ex);
            }

            return true;
        }

        /// <overloads>Releases resources used by the <see cref="PaginationTaskForm"/>.</overloads>
        /// <summary>
        /// Releases all unmanaged resources used by the <see cref="PaginationTaskForm"/>.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged 
        /// resources; <see langword="false"/> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Release managed resources
                if (components != null)
                {
                    components.Dispose();
                    components = null;
                }
                if (_formStateManager != null)
                {
                    _formStateManager.Dispose();
                    _formStateManager = null;
                }
                if (_voaFileLocks != null)
                {
                    CollectionMethods.ClearAndDispose(_voaFileLocks);
                    _voaFileLocks = null;
                }
            }

            // Release unmanaged resources

            base.Dispose(disposing);
        }

        #endregion Overrides

        #region Event Handlers

        /// <summary>
        /// Handles the <see cref="ToolStripItem.Click"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="ToolStripItem.Click"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="ToolStripItem.Click"/> event.</param>
        void HandleStopProcessingToolStripMenuItemClick(object sender, EventArgs e)
        {
            try
            {
                Close();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI40087");
            }
        }

        /// <summary>
        /// Handles the LoadNextDocument event of the _paginationPanel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void HandlePaginationPanel_LoadNextDocument(object sender, EventArgs e)
        {
            try
            {
                int fileID = FileRequestHandler.CheckoutNextFile(false);
                if (fileID > 0)
                {
                    string fileName = FileProcessingDB.GetFileNameFromFileID(fileID);
                    LoadDocumentForPagination(fileName);
                }
                else
                {
                    UtilityMethods.ShowMessageBox("No more files are available.", "No more files", false);
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI40088");
            }
        }

        /// <summary>
        /// Handles the <see cref="PaginationPanel.DocumentDataRequest"/> of the
        /// <see cref="_paginationPanel"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DocumentDataRequestEventArgs"/> instance containing the
        /// event data.</param>
        void HandlePaginationPanel_DocumentDataRequest(object sender, DocumentDataRequestEventArgs e)
        {
            try
            {
                e.DocumentData = GetAsPaginationDocumentData(new IUnknownVector());
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI40089");
            }
        }

        /// <summary>
        /// Handles the <see cref="PaginationPanel.CreatingOutputDocument"/> of the
        /// <see cref="_paginationPanel"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="CreatingOutputDocumentEventArgs"/> instance containing
        /// the event data.</param>
        void HandlePaginationPanel_CreatingOutputDocument(object sender, CreatingOutputDocumentEventArgs e)
        {
            try
            {
                // Keep the processing queue paused until the Paginated event.
                FileRequestHandler.PauseProcessingQueue();

                e.OutputFileName = GetPaginatedDocumentFileName(e);

                EFilePriority priority = GetPriorityForFile(e);

                // Add the file to the DB and check it out for this process before actually writing
                // it to outputPath to prevent a running file supplier from grabbing it and another
                // process from getting it.
                int fileID = AddFileWithNameConflictResolve(e, priority);

                // Format source page info into an IUnknownVector of StringPairs (filename, page).
                var sourcePageInfo = e.SourcePageInfo
                    .Select(info => new StringPairClass()
                    {
                        StringKey = info.DocumentName,
                        StringValue = info.Page.ToString(CultureInfo.InvariantCulture)
                    })
                    .ToIUnknownVector();

                ExtractException.Assert("ELI40090", "FileTaskSession was not started.",
                    _fileTaskSessionID.HasValue);

                FileProcessingDB.AddPaginationHistory(
                    e.OutputFileName, sourcePageInfo, _fileTaskSessionID.Value);

                // PaginationSourceAction allows a paginated source to be moved into a
                // cleanup action even when it is completed for this action without actually
                // having completed all FAM tasks.
                if (!string.IsNullOrWhiteSpace(_settings.OutputAction))
                {
                    EActionStatus oldStatus;
                    FileProcessingDB.SetStatusForFile(fileID,
                        _settings.OutputAction, EActionStatus.kActionPending, false, true, out oldStatus);
                }

                // Produce a uss file for the paginated document using the uss data from the
                // source documents
                int pageCounter = 1;
                var pageMap = e.SourcePageInfo.ToDictionary(
                    pageInfo => new Tuple<string, int>(pageInfo.DocumentName, pageInfo.Page),
                    _ => pageCounter++);

                CreateUSSForPaginatedDocument(e.OutputFileName, pageMap);

                var documentData = e.DocumentData as PaginationDocumentData;
                if (documentData != null)
                {
                    AttributeMethods.TranslateAttributesToNewDocument(
                        documentData.Attributes, e.OutputFileName, pageMap);

                    string dataFileName = e.OutputFileName + ".voa";
                    documentData.Attributes.SaveTo(dataFileName, false, _ATTRIBUTE_STORAGE_MANAGER_GUID);
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI40091");
            }
        }

        /// <summary>
        /// Handles the <see cref="PaginationPanel.Paginated"/> event of the
        /// <see cref="_paginationPanel"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PaginatedEventArgs"/> instance containing the event data.
        /// </param>
        void HandlePaginationPanel_Paginated(object sender, PaginatedEventArgs e)
        {
            try
            {
                // HandlePaginationPanel_CreatingOutputDocument will have already paused the
                // queue in most cases, but not if the applied pagination matched source doc form.
                FileRequestHandler.PauseProcessingQueue();

                // Reload all output files back into pagination.
                foreach (var output in _paginationOutputToReload)
                {
                    string fileName = output.Item1;
                    // Position is the index of the first page of the controls out of all pagination
                    // controls, though what position means doesn't really need to be interpreted in
                    // this scope.
                    int position = output.Item2;
                    PaginationDocumentData documentData = output.Item3;
                    _paginationPanel.LoadFile(fileName, position, null, false, documentData);
                }

                foreach (var item in e.ModifiedDocumentData)
                {
                    string sourceFileName = item.Key;
                    PaginationDocumentData documentData = (PaginationDocumentData)item.Value;

                    string dataFileName = sourceFileName + ".voa";
                    documentData.Attributes.SaveTo(dataFileName, false, _ATTRIBUTE_STORAGE_MANAGER_GUID);
                    documentData.SetOriginalForm();
                }

                ReleaseFiles(e.PaginatedDocumentSources.Union(
                    e.ModifiedDocumentData.Select(i => i.Key).Union(
                    e.DisregardedPaginationSources)));
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI40092");
            }
            finally
            {
                _paginationOutputToReload.Clear();
                FileRequestHandler.ResumeProcessingQueue();
            }
        }

        /// <summary>
        /// Handles the StateChanged event of the HandlePaginationPanel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void HandlePaginationPanel_StateChanged(object sender, EventArgs e)
        {
            try
            {
                UpdateControls();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI40093");
            }
        }

        /// <summary>
        /// Handles the Click event of the HandleApplyToolStripButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void HandleApplyToolStripButton_Click(object sender, EventArgs e)
        {
            try
            {
                _paginationPanel.SuspendLayout();
                _changingDocuments = true;

                if (_paginationPanel.PendingChanges)
                {
                    if (!_paginationPanel.CommitPendingChanges(true))
                    {
                        _changingDocuments = false;
                        UpdateControls();
                    }
                }
                else
                {
                    _lastDocumentPosition = ReleaseFiles(_paginationPanel.FullySelectedSourceDocuments);
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI40094");
            }
            finally
            {
                try
                {
                    _paginationPanel.ResumeLayout(true);

                    if (_changingDocuments)
                    {
                        _changingDocuments = false;

                        // If a new document was loaded in place of the released document(s), document
                        // selection will already have been defaulted. Otherwise, go ahead and default now.
                        this.SafeBeginInvoke("ELI40141", () =>
                        {
                            if (_lastDocumentPosition != -1)
                            {
                                DefaultDocumentSelection();
                            }
                        });
                    }
                }
                catch (Exception ex)
                {
                    ex.ExtractDisplay("ELI40168");
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the _revertToSuggestedToolStripButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void RevertToSuggestedToolStripButton_Click(object sender, EventArgs e)
        {
            try
            {
                _paginationPanel.RevertPendingChanges(revertToSource: false);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI40095");
            }
        }

        /// <summary>
        /// Handles the Click event of the _revertToDiskToolStripButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void RevertToDiskToolStripButton_Click(object sender, EventArgs e)
        {
            try
            {
                _paginationPanel.RevertPendingChanges(revertToSource: true);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI40096");
            }
        }

        #endregion Event Handlers

        #region IVerificationForm Members

        /// <summary>
        /// A thread-safe method that opens a document for verification.
        /// </summary>
        /// <param name="fileName">The filename of the document to open.</param>
        /// <param name="fileID">The ID of the file being processed.</param>
        /// <param name="actionID">The ID of the action being processed.</param>
        /// <param name="tagManager">The <see cref="FAMTagManager"/> to use if needed.</param>
        /// <param name="fileProcessingDB">The <see cref="FileProcessingDB"/> in use.</param>
        public void Open(string fileName, int fileID, int actionID, FAMTagManager tagManager,
            FileProcessingDB fileProcessingDB)
        {
            if (InvokeRequired)
            {
                _invoker.Invoke(new VerificationFormOpen(Open),
                    new object[] { fileName, fileID, actionID, tagManager, fileProcessingDB });
                return;
            }

            try
            {
                _fileName = Path.GetFullPath(fileName);
                _fileID = fileID;
                _actionID = actionID;
                _fileProcessingDB = fileProcessingDB;
                
                _fileTaskSessionID = null;
                StartFileTaskSession();

                if (!Focused)
                {
                    FormsMethods.FlashWindow(this, true, true);
                }

                LoadDocumentForPagination(fileName);
            }
            catch (Exception ex)
            {
                ExtractException ee = new ExtractException("ELI40097", "Unable to open file.", ex);
                ee.AddDebugData("File name", fileName, false);
                RaiseVerificationException(ee, true);
            }
        }

        /// <summary>
        /// A thread-safe method that allows loading of data prior to the <see cref="Open"/> call
        /// so as to reduce the time the <see cref="Open"/> call takes once it is called.
        /// </summary>
        /// <param name="fileName">The filename of the document for which to prefetch data.</param>
        /// <param name="fileID">The ID of the file being prefetched.</param>
        /// <param name="actionID">The ID of the action being prefetched.</param>
        /// <param name="tagManager">The <see cref="FAMTagManager"/> to use if needed.</param>
        /// <param name="fileProcessingDB">The <see cref="FileProcessingDB"/> in use.</param>
        public void Prefetch(string fileName, int fileID, int actionID, FAMTagManager tagManager,
            FileProcessingDB fileProcessingDB)
        {
            try
            {
                // This method must be run in the UI thread since it is loading attributes for
                // use in the UI thread.
                FormsMethods.ExecuteInUIThread(this, () => LoadDocumentForPagination(fileName));
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI40098", ex);
            }
        }

        /// <summary>
        /// Called to notify the file processor that the pending document queue is empty, but
        ///	the processing tasks have been configured to remain running until the next document
        ///	has been supplied. If the processor will standby until the next file is supplied it
        ///	should return <see langword="true"/>. If the processor wants to cancel processing,
        ///	it should return <see langword="false"/>. If the processor does not immediately know
        ///	whether processing should be cancelled right away, it may block until it does know,
        ///	and return at that time.
        /// <para><b>Note</b></para>
        /// This call will be made on a different thread than the other calls, so the Standby call
        /// must be thread-safe. This allows the file processor to block on the Standby call, but
        /// it also means the form may be opened or closed while the Standby call is still occurring.
        /// If this happens, the return value of Standby will be ignored; however, Standby should
        /// promptly return in this case to avoid needlessly keeping a thread alive.
        /// </summary>
        /// <returns><see langword="true"/> to standby until the next file is supplied;
        /// <see langword="false"/> to cancel processing.</returns>
        public bool Standby()
        {
            try
            {
                if (InvokeRequired)
                {
                    return (bool)_invoker.Invoke(new VerificationFormStandby(Standby));
                }

                return true;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI40099");
            }
        }

        /// <summary>
        /// Delays processing of the current file allowing the next file in the queue to be brought
        /// up in its place, though if there are no more files in the queue this will cause the same
        /// file to be re-displayed.
        /// </summary>
        public void DelayFile()
        {
            if (InvokeRequired)
            {
                _invoker.Invoke((MethodInvoker)(() =>
                {
                    try
                    {
                        DelayFile();
                    }
                    catch (Exception ex)
                    {
                        ex.ExtractDisplay("ELI40138");
                    }
                }));

                return;
            }

            try
            {
                ExtractException.Assert("ELI40139", "Invalid operation.",
                    FileProcessingDB != null);

                if (!string.IsNullOrEmpty(_fileName))
                {
                    OnFileComplete(EFileProcessingResult.kProcessingDelayed);
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI40140");
            }
        }

        #endregion IVerificationForm Members

        #region Private Members

        /// <summary>
        /// Updates the properties of the controls based on the currently open image.
        /// </summary>
        void UpdateControls()
        {
            bool enableApply = false;

            if (_paginationPanel.SourceDocuments.Any())
            {
                Text = _FORM_TASK_TITLE;

                if (_paginationPanel.CommitEnabled ||
                    _paginationPanel.FullySelectedSourceDocuments.Any())
                    
                {
                    enableApply = true;
                }
            }
            else
            {
                Text = "(Waiting for file) - " + _FORM_TASK_TITLE;
            }

            _applyToolStripButton.Enabled = !_changingDocuments && enableApply;
            _revertToSuggestedToolStripButton.Enabled =
                !_changingDocuments && _paginationPanel.RevertToOriginalEnabled;
            _revertToDiskToolStripButton.Enabled =
                !_changingDocuments && _paginationPanel.RevertToSourceEnabled;
        }

        /// <summary>
        /// Create a new FileTaskSession table row and starts a timer for the session.
        /// </summary>
        void StartFileTaskSession()
        {
            try
            {
                _fileTaskSessionID = FileProcessingDB.StartFileTaskSession(
                    _PAGINATION_TASK_GUID, _fileID);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI40101");
            }
        }

        /// <summary>
        /// Ends the current FileTaskSession by recording the DateTimeStamp and duration to the
        /// FileTaskSession row.
        /// </summary>
        void EndFileTaskSession()
        {
            try
            {
                _fileProcessingDB.UpdateFileTaskSession(
                    _fileTaskSessionID.Value, 0, 0);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI40102");
            }
        }

        /// <summary>
        /// Releases the specified file from the current process's internal queue of files checked
        /// out for processing. 
        /// </summary>
        /// <param name="fileID">The ID of the file to release.</param>
        public void ReleaseFile(int fileID)
        {
            try
            {
                FileRequestHandler.ReleaseFile(fileID);

                var eventArgs = new FileDelayedEventArgs(fileID);
                OnFileDelayed(eventArgs);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI40103");
            }
        }

        /// <summary>
        /// Releases the specified files from the current process's internal queue of files checked
        /// out for processing. 
        /// </summary>
        /// <param name="sourceFileNames">The names of the files to release.</param>
        /// <returns>The <see cref="_paginationPanel"/> index at which the first of the removed
        /// pages was at the time of release.</returns>
        int ReleaseFiles(IEnumerable<string> sourceFileNames)
        {
            int position = -1;

            foreach (string sourceFileName in sourceFileNames)
            {
                string dataFileName = sourceFileName + ".voa";
                ExtractFileLock voaFileLock = null;
                if (_voaFileLocks.TryGetValue(dataFileName, out voaFileLock))
                {
                    voaFileLock.ReleaseLock();
                    _voaFileLocks.Remove(sourceFileName);
                }

                int docPosition = _paginationPanel.RemoveSourceFile(sourceFileName);
                position = (position == -1)
                    ? docPosition
                    : Math.Min(docPosition, position);

                int sourceFileID = FileProcessingDB.GetFileID(sourceFileName);
                var success = FileRequestHandler.SetFallbackStatus(
                    sourceFileID, EActionStatus.kActionCompleted);
                ExtractException.Assert("ELI40104", "Failed to set fallback status", success,
                    "FileID", sourceFileID);
                ReleaseFile(sourceFileID);

                if (sourceFileName == _fileName)
                {
                    DelayFile();
                    EndFileTaskSession();
                    _fileName = null;
                }

                // SourceAction allows a paginated source to be moved into a cleanup action even
                // when it is completed for this action without actually having completed all FAM
                // tasks.
                if (!string.IsNullOrWhiteSpace(_settings.SourceAction))
                {
                    EActionStatus oldStatus;
                    FileProcessingDB.SetStatusForFile(sourceFileID,
                        _settings.SourceAction, EActionStatus.kActionPending, false, true, out oldStatus);
                }
            }

            return position;
        }

        /// <summary>
        /// Loads <see paramref="fileName"/> into <see cref="_paginationPanel"/> if it is available
        /// and displays any rules-suggested pagination for the file.
        /// <para><b>NOTE</b></para>
        /// This method must be run in the UI thread since it is loading attributes for use in the
        /// UI thread.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        void LoadDocumentForPagination(string fileName)
        {
            try
            {
                _paginationPanel.SuspendLayout();

                if (_paginationPanel == null || _paginationPanel.SourceDocuments.Contains(fileName))
                {
                    return;
                }

                // Look in the voa file for rules-suggested pagination.
                // TODO: Warn if the document was previously paginated.
                string dataFilename = fileName + ".voa";
                if (File.Exists(dataFilename))
                {
                    // If an image was loaded, look for and attempt to load corresponding data.
                    var voaFileLock = new ExtractFileLock();
                    voaFileLock.GetLock(dataFilename, "Paginate files task");
                    _voaFileLocks[dataFilename] = voaFileLock;

                    IUnknownVector attributes = new IUnknownVectorClass();
                    attributes.LoadFrom(dataFilename, false);

                    PaginationDocumentData documentData = GetAsPaginationDocumentData(attributes);

                    var attributeArray = attributes
                        .ToIEnumerable<IAttribute>()
                        .ToArray();
                    var rootAttributeNames = attributeArray
                        .Select(attribute => attribute.Name)
                        .Distinct();

                    // If only "Document" attributes exist at the root of the VOA file, there is
                    // rules-suggested pagination.
                    // TODO: If there is only a single document, extract the document data and don't
                    // show suggested pagination.
                    if (rootAttributeNames.Count() == 1 &&
                        rootAttributeNames.Single().Equals("Document", StringComparison.OrdinalIgnoreCase))
                    {
                        int pageCount = 0;
                        using (var codecs = new ImageCodecs())
                        using (var reader = codecs.CreateReader(fileName))
                        {
                            pageCount = reader.PageCount;
                        }

                        bool? suggestedPagination = null;

                        // Iterate each virtual document suggested by the rules and add as a separate
                        // document as far as the _paginationPanel is concerned.
                        foreach (var documentAttribute in attributeArray)
                        {
                            // There should be two attributes under the root Document attribute:
                            // Pages- The range/list specification of pages to be included.
                            // DocumentData- The data (redaction or indexing() the rules found for the
                            //  virtual document.
                            var pages =
                                UtilityMethods.GetPageNumbersFromString(
                                    documentAttribute.SubAttributes
                                    .ToIEnumerable<IAttribute>()
                                    .Single(attribute => attribute.Name.Equals(
                                        "Pages", StringComparison.OrdinalIgnoreCase))
                                    .Value.String, pageCount, true);

                            var documentAttributes = documentAttribute.SubAttributes
                                .ToIEnumerable<IAttribute>()
                                .Where(attribute => attribute.Name.Equals(
                                    "DocumentData", StringComparison.OrdinalIgnoreCase))
                                .Select(data => data.SubAttributes)
                                .SingleOrDefault() ?? new IUnknownVector();

                            documentData = GetAsPaginationDocumentData(documentAttributes);

                            if (!suggestedPagination.HasValue)
                            {
                                if (attributeArray.Length == 1 &&
                                    pages.Count() == pageCount)
                                {
                                    suggestedPagination = false;
                                }
                                else
                                {
                                    suggestedPagination = true;
                                }
                            }

                            _paginationPanel.LoadFile(
                                fileName, -1, pages, suggestedPagination.Value, documentData);
                            DefaultDocumentSelection();
                        }

                        return;
                    }

                    // There was a VOA file, just not with suggested pagination. Pass on the VOA data.
                    _paginationPanel.LoadFile(fileName, -1, null, false, documentData);
                    DefaultDocumentSelection();
                    return;
                }

                // If there was no rules-suggested pagination, go ahead and load the physical document
                // into the _paginationPanel
                _paginationPanel.LoadFile(fileName, -1);
                DefaultDocumentSelection();
            }
            finally
            {
                _paginationPanel.ResumeLayout(true);
            }
        }

        /// <summary>
        /// Gets a <see cref="PaginationDocumentData"/> instance representing the specified
        /// <see paramref="attributes"/> if the <see cref="_paginationDocumentDataPanel"/> is
        /// available.
        /// </summary>
        /// <param name="attributes">The attributes.</param>
        /// <returns>The <see cref="PaginationDocumentData"/> instance or <see langword="null"/> if
        /// the <see cref="_paginationDocumentDataPanel"/> is not available.</returns>
        PaginationDocumentData GetAsPaginationDocumentData(IUnknownVector attributes)
        {
            return (_paginationDocumentDataPanel != null)
                ? _paginationDocumentDataPanel.GetDocumentData(attributes)
                : null;
        }

        /// <summary>
        /// Generates the filename for pagination output based on the current pagination settings
        /// for the specified argument <see paramref="e"/>.
        /// </summary>
        /// <param name="e">A <see cref="CreatingOutputDocumentEventArgs"/> relating to the
        /// pagination output that is being generated.</param>
        /// <returns>The filename that should be used for this pagination output.</returns>
        string GetPaginatedDocumentFileName(CreatingOutputDocumentEventArgs e)
        {
            try
            {
                string outputDocPath = _settings.OutputPath;
                string sourceDocName = e.SourcePageInfo.First().DocumentName;
                var pathTags = new FileActionManagerPathTags((FAMTagManager)_tagUtility, sourceDocName);
                if (outputDocPath.Contains(PaginationSettings.SubDocIndexTag))
                {
                    string query = string.Format(CultureInfo.InvariantCulture,
                        "SELECT COUNT(DISTINCT([DestFileID])) + 1 AS [SubDocIndex] " +
                        "   FROM [Pagination] " +
                        "   INNER JOIN [FAMFile] ON [Pagination].[SourceFileID] = [FAMFile].[ID] " +
                        "   WHERE [FileName] = '{0}'",
                        sourceDocName.Replace("'", "''"));

                    var recordset = FileProcessingDB.GetResultsForQuery(query);
                    int subDocIndex = (int)recordset.Fields["SubDocIndex"].Value;
                    recordset.Close();

                    pathTags.AddTag(PaginationSettings.SubDocIndexTag,
                        subDocIndex.ToString(CultureInfo.InvariantCulture));
                }
                if (outputDocPath.Contains(PaginationSettings.FirstPageTag))
                {
                    int firstPageNum = e.SourcePageInfo
                        .Where(page => page.DocumentName == sourceDocName)
                        .Min(page => page.Page);

                    pathTags.AddTag(PaginationSettings.FirstPageTag,
                        firstPageNum.ToString(CultureInfo.InvariantCulture));
                }
                if (outputDocPath.Contains(PaginationSettings.LastPageTag))
                {
                    int lastPageNum = e.SourcePageInfo
                        .Where(page => page.DocumentName == sourceDocName)
                        .Max(page => page.Page);

                    pathTags.AddTag(PaginationSettings.LastPageTag,
                        lastPageNum.ToString(CultureInfo.InvariantCulture));
                }

                return pathTags.Expand(_settings.OutputPath);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI40105");
            }
        }

        /// <summary>
        /// Gets the priority to assign a paginated output file.
        /// </summary>
        /// <param name="e">The <see cref="CreatingOutputDocumentEventArgs"/> instance relating to
        /// the <see cref="PaginationPanel.CreatingOutputDocument"/> event for which this call is
        /// being made.</param>
        EFilePriority GetPriorityForFile(CreatingOutputDocumentEventArgs e)
        {
            try
            {
                var sourceDocNames = string.Join(", ",
                        e.SourcePageInfo
                            .Select(page => "'" + page.DocumentName.Replace("'", "''") + "'")
                            .Distinct());

                string query = string.Format(CultureInfo.InvariantCulture,
                    "SELECT MAX([FAMFile].[Priority]) AS [MaxPriority] FROM [FileActionStatus]" +
                    "   INNER JOIN [FAMFile] ON [FileID] = [FAMFile].[ID]" +
                    "   WHERE [ActionID] = {0}" +
                    "   AND [FileName] IN ({1})", _actionID, sourceDocNames);

                var recordset = FileProcessingDB.GetResultsForQuery(query);
                var priority = (EFilePriority)recordset.Fields["MaxPriority"].Value;
                recordset.Close();

                return priority;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI40106");
            }
        }

        /// <summary>
        /// Attempts add the filename associates with the argument <see paramref="e"/> to the FAM DB
        /// with the FileProcessingDB AddFileNoQueue method. If the add fails because
        /// the file already exists in the DB, it will add 6 random chars before the extension and
        /// try one more time.</summary>
        /// <param name="e">The <see cref="CreatingOutputDocumentEventArgs"/> instance relating to
        /// the <see cref="PaginationPanel.CreatingOutputDocument"/> event for which this call is
        /// being made.</param>
        /// <param name="priority">The <see cref="EFilePriority"/> that should be assigned for the
        /// file.</param>
        /// <returns>The ID of the newly added filename in the FAMFile table.</returns>
        int AddFileWithNameConflictResolve(CreatingOutputDocumentEventArgs e, EFilePriority priority)
        {
            int fileID = -1;

            try
            {
                fileID = FileProcessingDB.AddFileNoQueue(
                    e.OutputFileName, e.FileSize, e.PageCount, priority);
            }
            catch (Exception ex)
            {
                // Query to see if the e.OutputFileName can be found in the database.
                string query = string.Format(CultureInfo.InvariantCulture,
                    "SELECT [ID] FROM [FAMFile] WHERE [FileName] = '{0}'",
                    e.OutputFileName.Replace("'", "''"));

                var recordset = FileProcessingDB.GetResultsForQuery(query);
                bool fileExistsInDB = !recordset.EOF;
                recordset.Close();
                if (fileExistsInDB)
                {
                    var pathTags = new SourceDocumentPathTags(e.OutputFileName);
                    e.OutputFileName = pathTags.Expand(
                        "$InsertBeforeExt(<SourceDocName>,_$RandomAlphaNumeric(6))");

                    fileID = FileProcessingDB.AddFileNoQueue(
                        e.OutputFileName, e.FileSize, e.PageCount, priority);
                }
                else
                {
                    // The file was not in the database, the call failed for another reason.
                    throw ex.AsExtract("ELI40107");
                }
            }

            return fileID;
        }

        /// <summary>
        /// Creates a new uss file for the specified <see paramref="newDocumentName"/> based upon
        /// the specified <see paramref="pageMap"/> that relates the source pages to the
        /// corresponding pages in <see paramref="newDocumentName"/>.
        /// </summary>
        /// <param name="newDocumentName">The name of the document for which the uss file is being
        /// created.</param>
        /// <param name="pageMap">Each key represents a tuple of the old document name and page
        /// number while the value represents the new page number in 
        /// <see paramref="newDocumentName"/>.</param>
        static void CreateUSSForPaginatedDocument(string newDocumentName,
            Dictionary<Tuple<string, int>, int> pageMap)
        {
            try
            {
                var sourceUSSData = pageMap.Keys
                    .Select(sourcePage => sourcePage.Item1)
                    .Distinct()
                    .Where(sourceFileName => File.Exists(sourceFileName + ".uss"))
                    .ToDictionary(sourceFileName => sourceFileName, sourceFileName =>
                    {
                        var ussData = new SpatialString();
                        ussData.LoadFrom(sourceFileName + ".uss", false);
                        return ussData;
                    });

                var newPageData = new IUnknownVector();
                foreach (var pageInfo in pageMap)
                {
                    string sourceDocName = pageInfo.Key.Item1;
                    SpatialString sourceDocData;
                    if (sourceUSSData.TryGetValue(sourceDocName, out sourceDocData) &&
                        sourceDocData.HasSpatialInfo())
                    {
                        int sourcePage = pageInfo.Key.Item2;
                        int destPage = pageInfo.Value;
                        var pageData = sourceDocData.GetSpecifiedPages(sourcePage, sourcePage);
                        if (pageData.HasSpatialInfo())
                        {
                            pageData.UpdatePageNumber(destPage);

                            newPageData.PushBack(pageData);
                        }
                    }
                }

                var newUSSData = new SpatialString();
                if (newPageData.Size() > 0)
                {
                    newUSSData.CreateFromSpatialStrings(newPageData);
                }
                newUSSData.SourceDocName = newDocumentName;
                newUSSData.SaveTo(newDocumentName + ".uss", true, false);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI40108");
            }
        }

        /// <summary>
        /// Selects the document that now exists at <see cref="_lastDocumentPosition"/> (or the
        /// first document if <see cref="_lastDocumentPosition"/> is not set.
        /// </summary>
        void DefaultDocumentSelection()
        {
            _paginationPanel.SelectDocumentAtPosition(
                (_lastDocumentPosition == -1)
                    ? 0
                    : _lastDocumentPosition);
            _lastDocumentPosition = -1;
        }

        /// <summary>
        /// Cancels the current document and stops processing in the FAM.
        /// </summary>
        void CancelFile()
        {
            _imageViewer.CloseImage();

            OnFileComplete(EFileProcessingResult.kProcessingCancelled);
        }

        /// <summary>
        /// Raises the <see cref="ExceptionGenerated"/> event for handling by
        /// <see cref="VerificationForm{T}"/>.
        /// </summary>
        /// <param name="eliCode">The ELI code to be associated with the exception.</param>
        /// <param name="ex">The <see cref="Exception"/> that being raised.</param>
        /// <param name="canProcessingContinue"><see langword="true"/> if the user should be given
        /// the option to continue verification on the next document; <see langword="false"/> if the
        /// error should prevent the possibility of continuing the verification session.</param>
        void RaiseVerificationException(string eliCode, Exception ex, bool canProcessingContinue)
        {
            RaiseVerificationException(ex.AsExtract(eliCode), canProcessingContinue);
        }

        /// <summary>
        /// Raises the <see cref="ExceptionGenerated"/> event for handling by
        /// <see cref="VerificationForm{T}"/>.
        /// </summary>
        /// <param name="ee">The <see cref="ExtractException"/> that being raised.</param>
        /// <param name="canProcessingContinue"><see langword="true"/> if the user should be given
        /// the option to continue verification on the next document; <see langword="false"/> if the
        /// error should prevent the possibility of continuing the verification session.</param>
        void RaiseVerificationException(ExtractException ee, bool canProcessingContinue)
        {
            var verificationException =
                new VerificationExceptionGeneratedEventArgs(ee, canProcessingContinue);
            OnExceptionGenerated(verificationException);
        }

        /// <summary>
        /// Raises the <see cref="Initialized"/> event.
        /// </summary>
        void OnInitialized()
        {
            var eventHandler = Initialized;
            if (eventHandler != null)
            {
                eventHandler(this, new EventArgs());
            }
        }

        /// <summary>
        /// Raises the <see cref="FileComplete"/> event.
        /// </summary>
        /// <param name="processingResult">The <see cref="EFileProcessingResult"/> the file should
        /// be completed with.</param>
        void OnFileComplete(EFileProcessingResult processingResult)
        {
            var eventHandler = FileComplete;
            if (eventHandler != null)
            {
                eventHandler(this, new FileCompleteEventArgs(processingResult));
            }
        }

        /// <summary>
        /// Raises the <see cref="FileDelayed"/> event.
        /// </summary>
        /// <param name="eventArgs">The <see cref="FileDelayedEventArgs"/> instance containing the
        /// event data.</param>
        void OnFileDelayed(FileDelayedEventArgs eventArgs)
        {
            var eventHandler = FileDelayed;
            if (eventHandler != null)
            {
                eventHandler(this, eventArgs);
            }
        }

        /// <summary>
        /// Raises the <see cref="ExceptionGenerated"/> event.
        /// </summary>
        /// <param name="ee">The <see cref="VerificationExceptionGeneratedEventArgs"/> that has been
        /// generated.</param>
        void OnExceptionGenerated(VerificationExceptionGeneratedEventArgs ee)
        {
            var eventHandler = ExceptionGenerated;
            if (eventHandler != null)
            {
                eventHandler(this, ee);
            }
        }

        #endregion Private Members
    }
}
