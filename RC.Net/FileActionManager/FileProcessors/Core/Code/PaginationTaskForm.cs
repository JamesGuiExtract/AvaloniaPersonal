using Extract.AttributeFinder;
using Extract.DataEntry;
using Extract.FileActionManager.Forms;
using Extract.Imaging;
using Extract.Licensing;
using Extract.Utilities;
using Extract.Utilities.Forms;
using Extract.UtilityApplications.PaginationUtility;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using UCLID_AFCORELib;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;

namespace Extract.FileActionManager.FileProcessors
{
    /// <summary>
    /// Represents a task that displays a form allowing the user to paginate files.
    /// </summary>
    [CLSCompliant(false)]
    public sealed partial class PaginationTaskForm : Form, IVerificationForm, IDataEntryApplication
    {
        #region Constants

        /// <summary>
        /// The full path to the file that contains information about persisting the 
        /// <see cref="PaginationTaskForm"/>.
        /// </summary>
        static readonly string _FORM_PERSISTENCE_FILE = FileSystemMethods.PathCombine(
            FileSystemMethods.UserApplicationDataPath, "PaginateFiles", "PaginateFilesTaskForm.xml");

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
        /// The ID of the action being processed.
        /// </summary>
        int _actionID;

        /// <summary>
        /// Maps source file IDs to corresponding file task session IDs
        /// </summary>
        Dictionary<int, int> _fileTaskSessionMap = new Dictionary<int, int>();

        /// <summary>
        /// The <see cref="ITagUtility"/> interface of the <see cref="FAMTagManager"/> provided to
        /// expand path tags/functions.
        /// </summary>
        ITagUtility _tagUtility;

        /// <summary>
        /// Keeps track of the file ID(s) loaded; for use with the load next document button.
        /// </summary>
        List<int> _fileIDsLoaded = new List<int>();

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
        /// The _paginationForm index at which the last page was removed and the next file should
        /// be selected
        /// </summary>
        int _lastDocumentPosition = -1;

        /// <summary>
        /// Indicates when document are in the process of being swapped out to prevent unnecessary
        /// control updates during this process.
        /// </summary>
        bool _changingDocuments;

        /// <summary>
        /// Indicates whether all highlights are currently being shown.
        /// </summary>
        bool _showAllHighlights;

        /// <summary>
        /// The undo command.
        /// </summary>
        ApplicationCommand _undoCommand;

        /// <summary>
        /// The redo command.
        /// </summary>
        ApplicationCommand _redoCommand;

        /// <summary>
        /// Toggle show all data highlights
        /// </summary>
        ApplicationCommand _toggleShowAllHighlightsCommand;

        /// <summary>
        /// Indicates whether AttributeStatusInfo.DisposeThread is pending.
        /// </summary>
        bool _disposeThreadPending;

        /// <summary>
        /// Indicates whether a document selection is pending; rather than perform selections as
        /// each document is loaded (which can cause a lot of flickering), wait until all pending
        /// events are executed and only then perform the selection.
        /// </summary>
        bool _documentSelectionPending = true;

        /// <summary>
        /// Tracks user input in the file processing database.
        /// </summary>
        InputEventTracker _inputEventTracker;

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
        /// <param name="paginationDocumentDataPanelAssembly">The name of an assembly or DEP config
        /// file to be used to display any data panel for verifying/editing index data.</param>
        /// <param name="fileProcessingDB">The <see cref="FileProcessingDB"/> in use.</param>
        /// <param name="actionID">The ID of the action being processed.</param>
        /// <param name="tagManager">The <see cref="FAMTagManager"/> to use if needed.</param>
        /// <param name="fileRequestHandler">The <see cref="IFileRequestHandler"/> that can be used
        /// by the task to carry out requests for files to be checked out, released or re-ordered
        /// in the queue.</param>
        public PaginationTaskForm(PaginationTask settings,
            string paginationDocumentDataPanelAssembly, FileProcessingDB fileProcessingDB,
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
                _paginationPanel.DefaultToCollapsed = _settings.DefaultToCollapsed;
                _paginationPanel.AutoRotateImages = _settings.AutoRotateImages;
                _paginationPanel.SelectAllCheckBoxVisible = _settings.SelectAllCheckBoxVisible;
                _paginationPanel.LoadNextDocumentVisible = _settings.LoadNextDocumentVisible;
                _paginationPanel.SaveButtonVisible = true;

                if (!string.IsNullOrWhiteSpace(paginationDocumentDataPanelAssembly))
                {
                    var expandedAssemblyFileName = _tagUtility.ExpandTagsAndFunctions(
                        paginationDocumentDataPanelAssembly, null, null);

                    _paginationDocumentDataPanel = CreateDocumentDataPanel(expandedAssemblyFileName);
                    if (_paginationDocumentDataPanel.PanelControl != null)
                    {
                        _paginationDocumentDataPanel.PanelControl.ParentChanged +=
                            HandlePaginationDocumentDataPanel_ParentChanged;
                    }

                    if (_paginationDocumentDataPanel.AdvancedDataEntryOperationsSupported)
                    {
                        _advancedCommandsToolStrip.Visible = true;
                    }
                }

                Application.Idle += HandleApplicationIdle;
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
        /// Gets a value indicating whether the form supports displaying multiple documents
        /// simultaneously (one for each processing thread).
        /// </summary>
        /// <value><c>true</c> if the form supports multiple documents, <c>false</c> if only one
        /// document at a time can be loaded.</value>
        public bool SupportsMultipleDocuments
        {
            get
            {
                return !_settings.SingleSourceDocumentMode;
            }
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

        #region IDataEntryApplication

        /// <summary>
        /// This event indicates the value of <see cref="P:Extract.DataEntry.IDataEntryApplication.ShowAllHighlights" /> has
        /// changed.
        /// </summary>
        public event EventHandler<EventArgs> ShowAllHighlightsChanged;

        /// <summary>
        /// Saves the data currently displayed to disk.
        /// </summary>
        /// <param name="validateData"><see langword="true" /> to ensure the data is conforms to the
        /// DataEntryControlHost InvalidDataSaveMode before saving, <see langword="false" /> to save
        /// data without validating.</param>
        /// <returns>
        /// <see langword="true" /> if the data was saved, <see langword="false" /> if it was
        /// not.
        /// </returns>
        public bool SaveData(bool validateData)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Skips processing for the current file. This is the same as pressing the skip button in
        /// the UI.
        /// <para><b>Note</b></para>
        /// If there are changes in the currently loaded document, they will be disregarded. To
        /// check for changes and save, use the <see cref="P:Extract.DataEntry.IDataEntryApplication.Dirty" /> and <see cref="M:Extract.DataEntry.IDataEntryApplication.SaveData(System.Boolean)" />
        /// members first.
        /// </summary>
        public void SkipFile()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Requests the specified <see paramref="fileID" /> to be the next file displayed. The file
        /// should be allowed to jump ahead of any other files currently "processing" in the
        /// verification task on other threads (prefetch).
        /// <para><b>Note</b></para>
        /// The requested file will not be shown until the currently displayed file is closed. If
        /// the requested file needs to replace the currently displayed file immediately,
        /// <see cref="M:Extract.DataEntry.IDataEntryApplication.DelayFile" /> should be called after RequestFile.
        /// </summary>
        /// <param name="fileID">The file ID.</param>
        /// <returns>
        /// <see langword="true" /> if the file is currently processing in the verification
        /// task and confirmed to be available,<see langword="false" /> if the task is not currently
        /// holding the file; the requested file will be expected to the next file in the queue or
        /// an exception will result.
        /// </returns>
        public bool RequestFile(int fileID)
        {
            try
            {
                ExtractException.Assert("ELI44760", "Invalid operation.", FileProcessingDB != null);

                string fileName = FileProcessingDB.GetFileNameFromFileID(fileID);
                LoadDocumentForPagination(fileID, fileName, true);

                return true;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI44759");
            }
        }

        /// <summary>
        /// The title of the current DataEntry application.
        /// </summary>
        public string ApplicationTitle
        {
            get
            {
                return "";
            }
        }

        /// <summary>
        /// Specifies how the image viewer zoom/view is adjusted when new fields are selected.
        /// </summary>
        public AutoZoomMode AutoZoomMode
        {
            get
            {
                return AutoZoomMode.NoZoom;
            }
        }

        /// <summary>
        /// The page space (context) that should be shown around an object selected when AutoZoom
        /// mode is active. 0 indicates no context space should be shown around the current
        /// selection where 1 indicates the maximum context space should be shown.
        /// </summary>
        /// <exception cref="System.NotImplementedException"></exception>
        public double AutoZoomContext
        {
            get
            {
                return 0;
            }
        }

        /// <summary>
        /// Indicates whether tabbing should allow groups (rows) of attributes to be selected at a
        /// time for controls in which group tabbing is enabled.
        /// </summary>
        public bool AllowTabbingByGroup
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets whether highlights for all data mapped to an <see cref="IDataEntryControl"/>
        /// should be displayed in the <see cref="ImageViewer"/> or whether only highlights relating
        /// to the currently selected fields should be displayed.
        /// </summary>
        public bool ShowAllHighlights
        {
            get
            {
                return _showAllHighlights;
            }
        }

        /// <summary>
        /// Gets the name of the action in <see cref="P:Extract.DataEntry.FileProcessingDB" /> that the
        /// <see cref="T:Extract.DataEntry.IDataEntryApplication" /> is currently being run against.
        /// </summary>
        public string DatabaseActionName
        {
            get
            {
                return FileProcessingDB.GetActionName(_actionID);
            }
        }

        /// <summary>
        /// Gets or sets the comment for the current file that is stored in the file processing
        /// database.
        /// </summary>
        public string DatabaseComment
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a value indicating whether the currently loaded document is dirty.
        /// </summary>
        /// <value>
        /// <see langword="true" /> if dirty; otherwise, <see langword="false" />.
        /// </value>
        public bool Dirty
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the IDs of the files currently loaded in the application.
        /// </summary>
        /// <value>
        /// The IDs of the files currently loaded in the application.
        /// </value>
        public ReadOnlyCollection<int> FileIds
        {
            get
            {
                return _fileIDsLoaded.AsReadOnly();
            }
        }

        #endregion IDataEntryApplication

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

                ExtractException.Assert("ELI43205",
                    "Pagination cannot be performed for more than one workflow at a time.",
                    !FileProcessingDB.RunningAllWorkflows);

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
                _imageViewer.Shortcuts[Keys.Control | Keys.Home] = null;
                _imageViewer.Shortcuts[Keys.PageDown] = null;
                _imageViewer.Shortcuts[Keys.PageUp] = null;
                _imageViewer.Shortcuts[Keys.Control | Keys.End] = null;

                // Re-map image viewer shortcuts to used require modifier keys. Default shortcuts
                // that don't use modifier keys are not used so that data entry is not interfered
                // with.
                _imageViewer.Shortcuts[Keys.P | Keys.Control] = _imageViewer.SelectPrint;
                _imageViewer.Shortcuts[Keys.Alt | Keys.Z] = _imageViewer.SelectZoomWindowTool;
                _imageViewer.Shortcuts[Keys.Alt | Keys.A] = _imageViewer.SelectPanTool;
                _imageViewer.Shortcuts[Keys.Alt | Keys.R] = _imageViewer.SelectSelectLayerObjectsTool;
                _imageViewer.Shortcuts[Keys.Alt | Keys.P] = _imageViewer.ToggleFitToPageMode;
                _imageViewer.Shortcuts[Keys.Alt | Keys.W] = _imageViewer.ToggleFitToWidthMode;
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
                if (_paginationDocumentDataPanel != null && _paginationDocumentDataPanel.AdvancedDataEntryOperationsSupported)
                {
                    _imageViewer.Shortcuts[Keys.Escape] = _paginationDocumentDataPanel.ToggleHideTooltips;

                    // Undo command
                    _undoCommand = new ApplicationCommand(_imageViewer.Shortcuts,
                        new Keys[] { Keys.Z | Keys.Control }, PerformUndo,
                        new ToolStripItem[] { },
                        false, true, false);

                    // Redo command
                    _redoCommand = new ApplicationCommand(_imageViewer.Shortcuts,
                        new Keys[] { Keys.Y | Keys.Control }, PerformRedo,
                        new ToolStripItem[] { },
                        false, true, false);

                    // Toggle show all data highlights
                    _toggleShowAllHighlightsButton.Click += HandleToggleShowAllHighlightsClick;
                    _toggleShowAllHighlightsCommand = new ApplicationCommand(_imageViewer.Shortcuts,
                        new Keys[] { Keys.F10 }, ToggleShowAllHighlights,
                        new ToolStripItem[] { _toggleShowAllHighlightsButton }, false, true, false);
                }

                if (!string.IsNullOrWhiteSpace(_settings.SourceAction))
                {
                    int sourceActionID = FileProcessingDB.GetActionID(_settings.SourceAction);

                    ExtractException.Assert("ELI40385",
                        "Cannot set pagination sources back to pending in same action",
                        sourceActionID != _actionID);
                }

                if (!string.IsNullOrWhiteSpace(_settings.OutputAction))
                {
                    int outputActionID = FileProcessingDB.GetActionID(_settings.OutputAction);

                    ExtractException.Assert("ELI40386",
                        "Cannot set pagination output back to pending in same action",
                        outputActionID != _actionID);

                    // Register for OutputDocumentCreated event in order to set status for the
                    // OutputAction _after_ the document has been created
                    _paginationPanel.OutputDocumentCreated += HandlePaginationPanel_OutputDocumentCreated;
                }

                _paginationPanel.LoadNextDocument += HandlePaginationPanel_LoadNextDocument;
                _paginationPanel.FileTaskSessionIdRequest += HandlePaginationPanel_FileTaskSessionRequest;

                // May be null if the an IPaginationDocumentDataPanel is not specified to be used in
                // this workflow.
                _paginationPanel.DocumentDataPanel = _paginationDocumentDataPanel;
                if (_paginationDocumentDataPanel != null)
                {
                    _paginationPanel.DocumentDataRequest += HandlePaginationPanel_DocumentDataRequest;
                    _paginationDocumentDataPanel.UndoAvailabilityChanged += HandlePaginationDocumentDataPanel_UndoAvailabilityChanged;
                    _paginationDocumentDataPanel.RedoAvailabilityChanged += HandlePaginationDocumentDataPanel_RedoAvailabilityChanged;
                }

                // ComponentData directories referenced by configuration databases will be cached.
                // Clear any cached ComponentData directory each time the UI is opened.
                DataEntryConfiguration.ResetComponentDataDir();

                if (_splitContainer.Panel1MinSize < _paginationPanel.MinimumSize.Width)
                {
                    _splitContainer.Panel1MinSize = _paginationPanel.MinimumSize.Width;
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

        /// <summary>
        /// Raises the <see cref="Form.FormClosing"/> event in order to give the user an opportunity to save
        /// data prior to closing the application.
        /// </summary>
        /// <param name="e">The event data associated with the event.</param>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            try
            {
                e.Cancel = PreventClose();
                base.OnFormClosing(e);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI40280");
            }
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
                try
                {
                    Application.Idle -= HandleApplicationIdle;

                    // Close any open panels before disposing of anything
                    // https://extract.atlassian.net/browse/ISSUE-14377
                    _paginationPanel?.CloseDataPanel(validateData: false);

                    // Release managed resources
                    if (_inputEventTracker != null)
                    {
                        _inputEventTracker.Dispose();
                        _inputEventTracker = null;
                    }
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
                    if (_paginationDocumentDataPanel?.PanelControl != null)
                    {
                        _paginationDocumentDataPanel.PanelControl.Dispose();
                        _paginationDocumentDataPanel = null;
                    }
                    if (_disposeThreadPending)
                    {
                        AttributeStatusInfo.DisposeThread();
                        _disposeThreadPending = false;
                    }
                }
                catch
                { }
            }

            // Release unmanaged resources

            base.Dispose(disposing);
        }

        #endregion Overrides

        #region Event Handlers

        /// <summary>
        /// Handles the <see cref="PaginationPanel.PanelResetting"/> event of the
        /// <see cref="_paginationPanel"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        void HandlePaginationPanel_PanelResetting(object sender, EventArgs e)
        {
            try
            {
                if (_settings.SingleSourceDocumentMode)
                {
                    _paginationPanel.CommitOnlySelection = false;
                    _paginationPanel.LoadNextDocumentVisible = false;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI44675");
            }
        }

        /// <summary>
        /// Handles the <see cref="PaginationPanel.RevertedChanges"/> event of the
        /// <see cref="_paginationPanel"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        void HandlePaginationPanel_RevertedChanges(object sender, EventArgs e)
        {
            try
            {
                // SingleSourceDocumentMode should auto-open the DEP if there is no suggested pagination
                if (_settings.SingleSourceDocumentMode)
                { 
                    _paginationPanel.SafeBeginInvoke("ELI44676", () =>
                    {
                        if (!_paginationPanel.IsDataPanelOpen && _paginationPanel.OutputDocumentCount == 1)
                        {
                            _paginationPanel.OpenDataPanel();
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI44677");
            }
        }

        /// <summary>
        /// Handles the <see cref="Application.Idle"/> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        void HandleApplicationIdle(object sender, EventArgs e)
        {
            try
            {
                // Once application is idle, go ahead and perform any pending document selection.
                if (_documentSelectionPending)
                {
                    _documentSelectionPending = false;

                    if (_lastDocumentPosition != -1)
                    {
                        this.SafeBeginInvoke("ELI43370", () =>
                        {
                            _paginationPanel.SelectDocumentAtPosition(
                                (_lastDocumentPosition == -1)
                                    ? 0
                                    : _lastDocumentPosition);
                            _lastDocumentPosition = -1;
                        });
                    }

                    // SingleSourceDocumentMode should auto-open the DEP if there is no suggested pagination
                    if (_settings.SingleSourceDocumentMode && !_paginationPanel.IsDataPanelOpen &&
                        _paginationPanel.OutputDocumentCount == 1)
                    {
                        _paginationPanel.OpenDataPanel();
                    }

                    // Without this call, after committing the top document separator bar is not updating correctly after commit.
                    _paginationPanel.Invalidate();

                    if (!Focused)
                    {
                        FormsMethods.FlashWindow(this, true, true);
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI43371");
            }
        }

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
                // https://extract.atlassian.net/browse/ISSUE-13904
                // First check to see if there is an additional file that has been checked out for
                // processing that is not yet actively processing in the FAM.
                int fileID = FileRequestHandler.GetNextCheckedOutFile(
                    (_fileIDsLoaded.Count == 0) ? -1 : _fileIDsLoaded.Last());
                ExtractException.Assert("ELI41282", "Unexpected file",
                    !_fileIDsLoaded.Contains(fileID));

                // If not, retrieve the next file from the database.
                if (fileID == -1)
                {
                    fileID = FileRequestHandler.CheckoutNextFile(false);
                }
                if (fileID > 0)
                {
                    string fileName = FileProcessingDB.GetFileNameFromFileID(fileID);
                    _fileTaskSessionMap[fileID] = StartFileTaskSession(fileID);
                    LoadDocumentForPagination(fileID, fileName, true);
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
        /// Handles the <see cref="PaginationPanel.FileTaskSessionRequest"/> event of the <see cref="_paginationPanel"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="FileTaskSessionRequestEventArgs"/> instance containing the event data.</param>
        void HandlePaginationPanel_FileTaskSessionRequest(object sender, FileTaskSessionRequestEventArgs e)
        {
            try
            {
                if (_fileTaskSessionMap.TryGetValue(e.FileID, out int fileTaskSessionID))
                {
                    e.FileTaskSessionID = fileTaskSessionID;
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45597");
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
                string dataSourceDocName =
                    (e.SourceDocNames.Count() == 1)
                        ? e.SourceDocNames.Single()
                        : null;
                e.DocumentData = GetAsPaginationDocumentData(new IUnknownVector(), dataSourceDocName);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI40089");
            }
        }

        /// <summary>
        /// Handles the <see cref="PaginationPanel.CommittingChanges"/> event of the
        /// <see cref="_paginationPanel"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="CommittingChangesEventArgs"/> instance containing the event data.
        /// </param>
        void HandlePaginationPanel_CommittingChanges(object sender, CommittingChangesEventArgs e)
        {
            try
            {
                e.Handled = true;

                _paginationPanel.SuspendLayout();
                _changingDocuments = true;

                if (!_paginationPanel.CommitChanges())
                {
                    _changingDocuments = false;
                    UpdateControls();
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
                        _documentSelectionPending = true;
                    }
                }
                catch (Exception ex)
                {
                    ex.ExtractDisplay("ELI40168");
                }
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

                // Create directory if it doesn't exist
                Directory.CreateDirectory(Path.GetDirectoryName(e.OutputFileName));

                EFilePriority priority = GetPriorityForFile(e);

                // Add the file to the DB and check it out for this process before actually writing
                // it to outputPath to prevent a running file supplier from grabbing it and another
                // process from getting it.
                _paginationPanel.AddFileWithNameConflictResolve(e, priority);

                // Add pagination history before the image is created so that it does not
                // get queued by a watching supplier
                // https://extract.atlassian.net/browse/ISSUE-13760
                // Format source page info into an IUnknownVector of StringPairs (filename, page).
                var sourcePageInfo = e.SourcePageInfo
                    .Where(info => !info.Deleted)
                    .Select(info => new StringPairClass()
                    {
                        StringKey = info.DocumentName,
                        StringValue = info.Page.ToString(CultureInfo.InvariantCulture)
                    })
                    .ToIUnknownVector();
                var deletedSourcePageInfo = e.SourcePageInfo
                    .Where(info => info.Deleted)
                    .Select(info => new StringPairClass()
                    {
                        StringKey = info.DocumentName,
                        StringValue = info.Page.ToString(CultureInfo.InvariantCulture)
                    })
                    .ToIUnknownVector();

                int firstSourceDocID = e.SourcePageInfo
                    .Select(pageInfo => FileProcessingDB.GetFileID(pageInfo.DocumentName))
                    .First();

                ExtractException.Assert("ELI40090", "FileTaskSession was not started.",
                    _fileTaskSessionMap.TryGetValue(firstSourceDocID, out int sessionID));

                FileProcessingDB.AddPaginationHistory(
                    e.OutputFileName, sourcePageInfo, deletedSourcePageInfo, sessionID);

                // Produce a uss file for the paginated document using the uss data from the
                // source documents
                int pageCounter = 1;
                var pageMap = new Dictionary<Tuple<string, int>, List<int>>();
                foreach (var pageInfo in e.SourcePageInfo.Where(info => !info.Deleted))
                {
                    var sourcePage = new Tuple<string, int>(pageInfo.DocumentName, pageInfo.Page);
                    var destPages = new List<int>();
                    if (!pageMap.TryGetValue(sourcePage, out destPages))
                    {
                        destPages = new List<int>();
                        pageMap[sourcePage] = destPages;
                    }

                    destPages.Add(pageCounter++);
                }

                var newSpatialPageInfos = PaginationPanel.CreateUSSForPaginatedDocument(e.OutputFileName,
                                                                                        pageMap,
                                                                                        e.RotatedPages);

                var documentData = e.DocumentData as PaginationDocumentData;
                if (documentData != null && documentData.Attributes != null && documentData.Attributes.Size() > 0)
                {
                    AttributeMethods.TranslateAttributesToNewDocument(
                        documentData.Attributes, e.OutputFileName, pageMap, newSpatialPageInfos);

                    string dataFileName = e.OutputFileName + ".voa";
                    documentData.Attributes.SaveTo(dataFileName, false, _ATTRIBUTE_STORAGE_MANAGER_GUID);
                    documentData.Attributes.ReportMemoryUsage();
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

                foreach (var item in e.UnmodifiedPaginationSources
                    .Where(item => item.Value != null))
                {
                    string sourceFileName = item.Key;
                    PaginationDocumentData documentData = (PaginationDocumentData)item.Value;

                    string dataFileName = sourceFileName + ".voa";
                    documentData.Attributes.SaveTo(dataFileName, false, _ATTRIBUTE_STORAGE_MANAGER_GUID);
                    documentData.SetOriginalForm();
                }

                // SourceAction allows a paginated source to be moved into a cleanup action even
                // if it not moving forward in the primary workflow.
                ReleaseFiles(e.PaginatedDocumentSources, _settings.SourceAction);
                // OutputAction is for documents that should move forward in the primary workflow.
                ReleaseFiles(e.UnmodifiedPaginationSources.Select(source => source.Key), _settings.OutputAction);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI40092");
            }
            finally
            {
                FileRequestHandler.ResumeProcessingQueue();
            }
        }

        /// <summary>
        /// Handles the <see cref="PaginationPanel.PaginationError"/> event of the
        /// <see cref="_paginationPanel"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ExtractExceptionEventArgs"/> instance containing the
        /// event data.</param>
        void HandlePaginationPanel_PaginationError(object sender, ExtractExceptionEventArgs e)
        {
            try
            {
                // https://extract.atlassian.net/browse/ISSUE-13831
                // If an exception is thrown creating the output document, the Paginated event will
                // not be raised (which is ordinarily what would resume the processing queue).
                FileRequestHandler.ResumeProcessingQueue();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI40233");
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
        /// Handles the <see cref="UndoAvailabilityChanged"/> event of the
        /// <see cref="_paginationDocumentDataPanel"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        void HandlePaginationDocumentDataPanel_UndoAvailabilityChanged(object sender, EventArgs e)
        {
            try
            {
                _undoCommand.Enabled = _paginationDocumentDataPanel.UndoOperationAvailable;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI41426");
            }
        }

        /// <summary>
        /// Handles the <see cref="RedoAvailabilityChanged"/> event of the
        /// <see cref="_paginationDocumentDataPanel"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        void HandlePaginationDocumentDataPanel_RedoAvailabilityChanged(object sender, EventArgs e)
        {
            try
            {
                _redoCommand.Enabled = _paginationDocumentDataPanel.RedoOperationAvailable;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI41427");
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event of the <see cref="_undoToolStripButton"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        void HandleUndoToolStripButton_Click(object sender, EventArgs e)
        {
            try
            {
                _paginationDocumentDataPanel.Undo();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI41421");
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event of the <see cref="_redoToolStripButton"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        void HandleRedoToolStripButton_Click(object sender, EventArgs e)
        {
            try
            {
                _paginationDocumentDataPanel.Redo();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI41422");
            }
        }

        /// <summary>
        /// Handles the <see cref="ParentChanged"/> event of the <see cref="_paginationDocumentDataPanel"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        void HandlePaginationDocumentDataPanel_ParentChanged(object sender, EventArgs e)
        {
            try
            {
                if (_paginationDocumentDataPanel.AdvancedDataEntryOperationsSupported)
                {
                    if (_showAllHighlights)
                    {
                        ToggleShowAllHighlights();
                    }
                    _toggleShowAllHighlightsCommand.Enabled = _paginationDocumentDataPanel.PanelControl.Parent != null;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI41475");
            }
        }

        /// <summary>
        /// Handles the case that the user selected the "Highlight all data in image" button.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">The event data associated with the event.</param>
        void HandleToggleShowAllHighlightsClick(object sender, EventArgs e)
        {
            try
            {
                ToggleShowAllHighlights();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI41476");
            }
        }

        /// <summary>
        /// Sets a newly created file to pending for the OutputAction
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="CreatingOutputDocumentEventArgs"/> instance containing the
        /// FileID of the newly created file.</param>
        void HandlePaginationPanel_OutputDocumentCreated(object sender, CreatingOutputDocumentEventArgs e)
        {
            try
            {
                FileProcessingDB.SetStatusForFile(e.FileID,
                    _settings.OutputAction,
                    -1, // Current workflow
                    EActionStatus.kActionPending,
                    vbAllowQueuedStatusOverride: false,
                    vbQueueChangeIfProcessing: false,
                    poldStatus: out var _);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI43308");
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
                _actionID = actionID;
                _fileProcessingDB = fileProcessingDB;

                _fileTaskSessionMap[fileID] = StartFileTaskSession(fileID);

                if (_inputEventTracker == null)
                {
                    _inputEventTracker = new InputEventTracker(fileProcessingDB, actionID);
                    _inputEventTracker.RegisterControl(this);
                }
                _inputEventTracker.Active = true;

                if (_settings.SingleSourceDocumentMode)
                {
                    // In discussion with Rob for the 10.6 Hurley update (10.6.1.XX), he wanted a title
                    // bar that did not refer to "Paginate Files" since that isn't the primary purpose
                    // of this task, esp when in SingleSourceDocumentMode. More consistent changes to
                    // title bar text to be discussed later.
                    string titleText = "Extract - <Workflow> - $FileOf(<SourceDocName>)";

                    var pathTags = new FileActionManagerPathTags((FAMTagManager)_tagUtility, fileName);
                    titleText = pathTags.Expand(titleText);
                    Text = titleText.Replace("-  -", "-");
                }

                LoadDocumentForPagination(fileID, fileName, false);

                _documentSelectionPending = true;
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

                if (_settings.SingleSourceDocumentMode)
                {
                    // In discussion with Rob for the 10.6 Hurley update (10.6.1.XX), he wanted a title
                    // bar that did not refer to "Paginate Files" since that isn't the primary purpose
                    // of this task, esp when in SingleSourceDocumentMode. More consistent changes to
                    // title bar text to be discussed later.
                    string titleText = "Extract - <Workflow> - (Waiting for file)";

                    titleText = _tagUtility.ExpandTagsAndFunctions(titleText, "", null);
                    Text = titleText.Replace("-  -", "-");
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
        /// <param name="fileId">The ID of the file to delay (or -1 when there is only a single
        /// file to which this call could apply).</param>
        public void DelayFile(int fileId = -1)
        {
            if (InvokeRequired)
            {
                _invoker.Invoke((MethodInvoker)(() =>
                {
                    try
                    {
                        DelayFile(fileId);
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

                OnFileComplete(fileId, EFileProcessingResult.kProcessingDelayed);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI40140");
            }
        }

        /// <summary>
        /// Executes disposal of any thread-local or thread-static objects just prior to the UI
        /// thread closing.
        /// </summary>
        public void DisposeThread()
        {
            if (IsDisposed)
            {
                AttributeStatusInfo.DisposeThread();
            }
            else
            {
                _disposeThreadPending = true;
            }
        }

        #endregion IVerificationForm Members

        #region Private Members

        /// <summary>
        /// Creates the <see cref="IPaginationDocumentDataPanel"/> that should be used to edit data
        /// for documents in the pagination pane.
        /// </summary>
        /// <param name="paginationDocumentDataPanelAssembly"></param>
        /// <returns>The <see cref="IPaginationDocumentDataPanel"/> that should be used to edit data
        /// for documents in the pagination pane.</returns>
        IPaginationDocumentDataPanel CreateDocumentDataPanel(string paginationDocumentDataPanelAssembly)
        {
            if (paginationDocumentDataPanelAssembly.EndsWith(".config", StringComparison.OrdinalIgnoreCase))
            {
                return new DataEntryPanelContainer(paginationDocumentDataPanelAssembly, this, _tagUtility, _imageViewer);
            }
            else
            {
                // May be null if the an IPaginationDocumentDataPanel is not specified to be used in
                // this workflow.
                return UtilityMethods.CreateTypeFromAssembly<IPaginationDocumentDataPanel>(
                    paginationDocumentDataPanelAssembly);
            }
        }

        /// <summary>
        /// Updates the properties of the controls based on the currently open image.
        /// </summary>
        void UpdateControls()
        {
            if (!_settings.SingleSourceDocumentMode)
            {
                if (_paginationPanel.SourceDocuments.Any())
                {
                    Text = "Paginate Files";
                }
                else
                {
                    Text = "(Waiting for file) - Paginate Files";
                }
            }
        }

        /// <summary>
        /// Create a new FileTaskSession table row and starts a timer for the session.
        /// </summary>
        /// <param name="fileID">The ID of the file for which a session is to be started.</param>
        int StartFileTaskSession(int fileID)
        {
            try
            {
                return FileProcessingDB.StartFileTaskSession(_PAGINATION_TASK_GUID, fileID, _actionID);
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
        /// <param name="fileID">The ID of the file for which a session is to be ended.</param>
        void EndFileTaskSession(int fileID)
        {
            try
            {
                ExtractException.Assert("ELI45593", "No active session",
                    _fileTaskSessionMap.TryGetValue(fileID, out int sessionID));

                _fileProcessingDB.UpdateFileTaskSession(sessionID, 0, 0, 0);
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
        /// <param name="targetAction">The action the source files should be queued to.</param>
        /// <returns>The <see cref="_paginationPanel"/> index at which the first of the removed
        /// pages was at the time of release.</returns>
        int ReleaseFiles(IEnumerable<string> sourceFileNames, string targetAction)
        {
            int position = -1;

            foreach (string sourceFileName in sourceFileNames)
            {
                int docPosition = _paginationPanel.RemoveSourceFile(sourceFileName, acceptingPagination: true);
                position = (position == -1)
                    ? docPosition
                    : Math.Min(docPosition, position);

                int sourceFileID = FileProcessingDB.GetFileID(sourceFileName);
                _fileIDsLoaded.Remove(sourceFileID);

                var success = FileRequestHandler.SetFallbackStatus(
                    sourceFileID, EActionStatus.kActionCompleted);
                ExtractException.Assert("ELI40104", "Failed to set fallback status", success,
                    "FileID", sourceFileID);
                ReleaseFile(sourceFileID);

                DelayFile(sourceFileID);
                EndFileTaskSession(sourceFileID);

                if (!string.IsNullOrWhiteSpace(targetAction))
                {
                    EActionStatus oldStatus;
                    FileProcessingDB.SetStatusForFile(sourceFileID,
                        targetAction, -1, EActionStatus.kActionPending, false, true, out oldStatus);
                }

                if (_paginationPanel.OutputDocumentCount == 0)
                {
                    // Don't count input when a document is not open.
                    _inputEventTracker.Active = false;
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
        /// <param name="fileId">The ID of the file.</param>
        /// <param name="fileName">The name of the file.</param>
        /// <param name="selectDocument"><see langword="true"/> to select the first document
        /// resulting from this source document; otherwise, <see langword="false"/>.</param>
        void LoadDocumentForPagination(int fileId, string fileName, bool selectDocument)
        {
            try
            {
                _paginationPanel.SuspendUIUpdates = true;

                if (_paginationPanel == null || _paginationPanel.SourceDocuments.Contains(fileName))
                {
                    return;
                }

                _fileIDsLoaded.Add(fileId);

                // Look in the voa file for rules-suggested pagination.
                // TODO: Warn if the document was previously paginated.
                string dataFilename = fileName + ".voa";
                if (File.Exists(dataFilename))
                {
                    // If an image was loaded, look for and attempt to load corresponding data.
                    IUnknownVector attributes = new IUnknownVectorClass();
                    attributes.LoadFrom(dataFilename, false);
                    attributes.UpdateSourceDocNameOfAttributes(fileName);
                    attributes.ReportMemoryUsage();

                    var attributeArray = attributes
                        .ToIEnumerable<IAttribute>()
                        .ToArray();
                    var rootAttributeNames = new HashSet<string>(
                        attributeArray.Select(attribute => attribute.Name),
                        StringComparer.OrdinalIgnoreCase);

                    // If only "Document" attributes exist at the root of the VOA file, there is
                    // rules-suggested pagination.
                    // TODO: If there is only a single document, extract the document data and don't
                    // show suggested pagination.
                    if (rootAttributeNames.Count == 1 &&
                        rootAttributeNames.Contains("Document"))
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
                                    .Where(attribute => attribute.Name.Equals(
                                        "Pages", StringComparison.OrdinalIgnoreCase))
                                    .Select(attribute => attribute.Value.String)
                                    .SingleOrDefault() ?? "", pageCount, true);

                            var deletedPages =
                                UtilityMethods.GetPageNumbersFromString(
                                    documentAttribute.SubAttributes
                                    .ToIEnumerable<IAttribute>()
                                    .Where(attribute => attribute.Name.Equals(
                                        "DeletedPages", StringComparison.OrdinalIgnoreCase))
                                    .Select(attribute => attribute.Value.String)
                                    .SingleOrDefault() ?? "", pageCount, true);

                            var documentDataAttribute = documentAttribute.SubAttributes
                                .ToIEnumerable<IAttribute>()
                                .SingleOrDefault(attribute => attribute.Name.Equals(
                                    "DocumentData", StringComparison.OrdinalIgnoreCase));

                            PaginationDocumentData documentData =
                                GetAsPaginationDocumentData(documentDataAttribute, fileName);

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

                            _paginationPanel.LoadFile(fileName, fileId, -1, pages, deletedPages,
                                suggestedPagination.Value, documentData, selectDocument);
                            selectDocument = false;
                        }

                        return;
                    }

                    // There was a VOA file, just not with suggested pagination. Pass on the VOA data.
                    PaginationDocumentData rootDocumentData = GetAsPaginationDocumentData(attributes, fileName);
                    _paginationPanel.LoadFile(
                        fileName, fileId, -1, null, null, false, rootDocumentData, selectDocument);
                    return;
                }

                // If there was no rules-suggested pagination, go ahead and load the physical document
                // into the _paginationPanel
                _paginationPanel.LoadFile(fileName, fileId, -1, selectDocument);
            }
            finally
            {
                try
                {
                    // If in SingleSourceDocumentMode, the apply button should always be enabled whenever
                    // a document is loaded (rather than only being enabled when a document is selected)
                    if (_settings.SingleSourceDocumentMode && _paginationPanel.SourceDocuments.Any())
                    {
                        _paginationPanel.PendingChanges = true;
                    }
                }
                catch (Exception ex)
                {
                    ex.ExtractLog("ELI44678");
                }

                if (!_paginationPanel.IsCommittingChanges)
                {
                    _paginationPanel.SuspendUIUpdates = false;
                }
            }
        }

        /// <summary>
        /// Gets a <see cref="PaginationDocumentData"/> instance representing the specified
        /// <see paramref="attributes"/> if the <see cref="_paginationDocumentDataPanel"/> is
        /// available.
        /// </summary>
        /// <param name="documentDataAttribute">The attributes.</param>
        /// <param name="sourceDocName">The source document name to be associated the data is
        /// associated with or <see langword="null"/> if the data is not associated with a
        /// particular source document.</param>
        /// <returns>The <see cref="PaginationDocumentData"/> instance or <see langword="null"/> if
        /// the <see cref="_paginationDocumentDataPanel"/> is not available.</returns>
        PaginationDocumentData GetAsPaginationDocumentData(IAttribute documentDataAttribute, string sourceDocName)
        {
            return (_paginationDocumentDataPanel == null)
                ? new PaginationDocumentData(documentDataAttribute, sourceDocName)
                : _paginationDocumentDataPanel.GetDocumentData(
                    documentDataAttribute, sourceDocName, FileProcessingDB, _imageViewer);
        }

        /// <summary>
        /// Gets a <see cref="PaginationDocumentData"/> instance representing the specified
        /// <see paramref="attributes"/> if the <see cref="_paginationDocumentDataPanel"/> is
        /// available.
        /// </summary>
        /// <param name="documentDataAttribute">The attributes.</param>
        /// <param name="sourceDocName">The source document name to be associated the data is
        /// associated with or <see langword="null"/> if the data is not associated with a
        /// particular source document.</param>
        /// <returns>The <see cref="PaginationDocumentData"/> instance or <see langword="null"/> if
        /// the <see cref="_paginationDocumentDataPanel"/> is not available.</returns>
        PaginationDocumentData GetAsPaginationDocumentData(IUnknownVector attributes, string sourceDocName)
        {
            return (_paginationDocumentDataPanel == null)
                ? new PaginationDocumentData(attributes, sourceDocName)
                : _paginationDocumentDataPanel.GetDocumentData(
                    attributes, sourceDocName, FileProcessingDB, _imageViewer);
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
                var sourcePageInfo = e.SourcePageInfo.Where(info => !info.Deleted).ToList();
                string sourceDocName = sourcePageInfo.First().DocumentName;
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
                    int firstPageNum = sourcePageInfo
                        .Where(page => page.DocumentName == sourceDocName)
                        .Min(page => page.Page);

                    pathTags.AddTag(PaginationSettings.FirstPageTag,
                        firstPageNum.ToString(CultureInfo.InvariantCulture));
                }
                if (outputDocPath.Contains(PaginationSettings.LastPageTag))
                {
                    int lastPageNum = sourcePageInfo
                        .Where(page => page.DocumentName == sourceDocName)
                        .Max(page => page.Page);

                    pathTags.AddTag(PaginationSettings.LastPageTag,
                        lastPageNum.ToString(CultureInfo.InvariantCulture));
                }

                return Path.GetFullPath(pathTags.Expand(_settings.OutputPath));
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
                var sourcePageInfo = e.SourcePageInfo.Where(info => !info.Deleted).ToList();
                var sourceDocNames = string.Join(", ",
                        sourcePageInfo
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
        /// If there are any manual pagination changes, warns the user about the changes that will
        /// be lost if the pagination UI is closed.
        /// </summary>
        /// <returns><see langword="true"/> if the users doesn't want to lose their changes and the
        /// task should be kept open; <see langword="false"/> if there are no manual pagination
        /// changes or the user is okay losing them.</returns>
        bool PreventClose()
        {
            if (FileIds.Any())
            {
                // Because PaginationPanel.CheckForChanges is not very robust a determining when there
                // are actually changes to save, always prompt for save on close without also trying to
                // state whether changes have been correctly saved.
                var response = MessageBox.Show(this, "Would you like to save your work?",
                "Save progress?", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button1, 0);

                if (response == DialogResult.Yes)
                {
                    _paginationPanel.Save();
                    return false;
                }
                else if (response == DialogResult.Cancel)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Performs an undo operation.
        /// </summary>
        void PerformUndo()
        {
            try
            {
                _paginationDocumentDataPanel.Undo();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI41428");
            }
        }

        /// <summary>
        /// Performs a redo operation.
        /// </summary>
        void PerformRedo()
        {
            try
            {
                _paginationDocumentDataPanel.Redo();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI41429");
            }
        }

        /// <summary>
        /// Toggles whether all data is currently highlighted in the <see cref="ImageViewer"/> or
        /// whether only the currently selected data is highlighted.
        /// </summary>
        void ToggleShowAllHighlights()
        {
            _showAllHighlights = !_showAllHighlights;

            _toggleShowAllHighlightsButton.CheckState =
                _showAllHighlights ? CheckState.Checked : CheckState.Unchecked;

            OnShowAllHighlightsChanged();
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
            Initialized.Invoke(this, new EventArgs());
        }

        /// <summary>
        /// Raises the <see cref="FileComplete"/> event.
        /// </summary>
        /// <param name="fileId">The ID of the file that has completed.</param>
        /// <param name="processingResult">The <see cref="EFileProcessingResult"/> the file should
        /// be completed with.</param>
        void OnFileComplete(int fileId, EFileProcessingResult processingResult)
        {
            FileComplete.Invoke(this, new FileCompleteEventArgs(fileId, processingResult));
        }

        /// <summary>
        /// Raises the <see cref="FileDelayed"/> event.
        /// </summary>
        /// <param name="eventArgs">The <see cref="FileDelayedEventArgs"/> instance containing the
        /// event data.</param>
        void OnFileDelayed(FileDelayedEventArgs eventArgs)
        {
            FileDelayed.Invoke(this, eventArgs);
        }

        /// <summary>
        /// Raises the <see cref="ExceptionGenerated"/> event.
        /// </summary>
        /// <param name="ee">The <see cref="VerificationExceptionGeneratedEventArgs"/> that has been
        /// generated.</param>
        void OnExceptionGenerated(VerificationExceptionGeneratedEventArgs ee)
        {
            ExceptionGenerated?.Invoke(this, ee);
        }

        /// <summary>
        /// Raises the <see cref="ShowAllHighlightsChanged"/> event.
        /// </summary>
        void OnShowAllHighlightsChanged()
        {
            ShowAllHighlightsChanged?.Invoke(this, new EventArgs());
        }

        #endregion Private Members
    }
}
