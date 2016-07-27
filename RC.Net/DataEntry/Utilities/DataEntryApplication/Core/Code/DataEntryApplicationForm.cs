using Extract.AttributeFinder;
using Extract.Database;
using Extract.DataEntry.Utilities.DataEntryApplication.Properties;
using Extract.FileActionManager.Forms;
using Extract.Imaging;
using Extract.Imaging.Forms;
using Extract.Licensing;
using Extract.Utilities;
using Extract.Utilities.Forms;
using Extract.UtilityApplications.PaginationUtility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;
using TD.SandDock;
using UCLID_AFCORELib;
using UCLID_AFUTILSLib;
using UCLID_COMUTILSLib;
using UCLID_DATAENTRYCUSTOMCOMPONENTSLib;
using UCLID_FILEPROCESSINGLib;

namespace Extract.DataEntry.Utilities.DataEntryApplication
{
    /// <summary>
    /// The generic application used to run all data entry forms.  The application consists of two
    /// panes:
    /// <list type="bullet">
    /// <item>The Data Entry Panel (DEP) will display the content from a document and allow for the content
    /// to be verified/corrected.  The DEP consists of a <see cref="DataEntryControlHost"/> instance 
    /// populated by controls which implement <see cref="IDataEntryControl"/>.</item>
    /// <item>The image viewer will display the document image itself and allow for interaction with the
    /// DEP such as highlighting the image area associated with the content currently selected in the DEP
    /// or allowing DEP controls to be populated via OCR "swipes" in the image viewer.</item>
    /// </list>
    /// </summary>
    public partial class DataEntryApplicationForm : Form, IVerificationForm, IDataEntryApplication
    {
        #region Constants

        /// <summary>
        /// The license string for the SandDock manager
        /// </summary>
        static readonly string _SANDDOCK_LICENSE_STRING = @"1970|siE7SnF/jzINQg1AOTIaCXLlouA=";

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(DataEntryApplicationForm).ToString();

        /// <summary>
        /// The number of pixels to pad around the DEP that is loaded.
        /// </summary>
        const int _DATA_ENTRY_PANEL_PADDING = 3;

        /// <summary>
        /// "Save and commit"
        /// </summary>
        const string _SAVE_AND_COMMIT = "Save and commit";

        /// <summary>
        /// "Next document"
        /// </summary>
        const string _NEXT_DOCUMENT = "Next document";

        /// <summary>
        /// A string representation of the GUID of the data entry verification task.
        /// </summary>
        static readonly string _VERIFICATION_TASK_GUID = typeof(ComClass).GUID.ToString("B");

        /// <summary>
        /// A string representation of the GUID for <see cref="AttributeStorageManagerClass"/> 
        /// </summary>
        static readonly string _ATTRIBUTE_STORAGE_MANAGER_GUID =
            typeof(AttributeStorageManagerClass).GUID.ToString("B");

        #endregion Constants

        #region DataEntryConfiguration

        /// <summary>
        /// Represents a data entry configuration used to display document data.
        /// </summary>
        class DataEntryConfiguration : IDisposable
        {
            /// <summary>
            /// The configuration settings specified via config file.
            /// </summary>
            ConfigSettings<Extract.DataEntry.Properties.Settings> _config;

            /// <summary>
            /// The <see cref="DataEntryControlHost"/> instance associated with the configuration.
            /// </summary>
            DataEntryControlHost _dataEntryControlHost;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="config">The configuration settings specified via config file.</param>
            /// <param name="dataEntryControlHost">The <see cref="DataEntryControlHost"/> instance
            /// associated with the configuration.</param>
            public DataEntryConfiguration(
                ConfigSettings<Extract.DataEntry.Properties.Settings> config,
                DataEntryControlHost dataEntryControlHost)
            {
                _config = config;
                _dataEntryControlHost = dataEntryControlHost;
            }

            /// <summary>
            /// The configuration settings specified via config file.
            /// </summary>
            public ConfigSettings<Extract.DataEntry.Properties.Settings> Config
            {
                get
                {
                    return _config;
                }
            }

            /// <summary>
            /// The <see cref="DataEntryControlHost"/> instance associated with the configuration.
            /// </summary>
            public DataEntryControlHost DataEntryControlHost
            {
                get
                {
                    return _dataEntryControlHost;
                }
            }

            /// <overloads>Releases resources used by the <see cref="DataEntryConfiguration"/>.
            /// </overloads>
            /// <summary>
            /// Releases all resources used by the <see cref="DataEntryConfiguration"/>.
            /// </summary>
            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            /// <summary>
            /// Releases all unmanaged resources used by the <see cref="DataEntryConfiguration"/>.
            /// </summary>
            /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged
            /// resources; <see langword="false"/> to release only unmanaged resources.</param>        
            void Dispose(bool disposing)
            {
                if (disposing)
                {
                    // Dispose of managed resources
                    if (_dataEntryControlHost != null)
                    {
                        _dataEntryControlHost.Dispose();
                        _dataEntryControlHost = null;
                    }
                }

                // Dispose of unmanaged resources
            }
        }

        #endregion DataEntryConfiguration

        #region Fields

        /// <summary>
        /// The settings for this application.
        /// </summary>
        ConfigSettings<Properties.Settings> _applicationConfig;

        /// <summary>
        /// The user settings to be persisted to the registry for this application.
        /// </summary>
        RegistrySettings<Properties.Settings> _registry;

        /// <summary>
        /// The verification task settings.
        /// </summary>
        VerificationSettings _settings;

        /// <summary>
        /// Provides the resources used to brand the DataEntryApplication as a specific product.
        /// </summary>
        BrandingResourceManager _brandingResources;
        
        /// <summary>
        /// A map of defined document types to the configuration to be used.
        /// </summary>
        Dictionary<string, DataEntryConfiguration> _documentTypeConfigurations;

        /// <summary>
        /// If not <see langword="null"/> this configuration should be used for documents with
        /// missing or undefined document types.
        /// </summary>
        DataEntryConfiguration _defaultDataEntryConfig;

        /// <summary>
        /// The configuration that is currently loaded.
        /// </summary>
        DataEntryConfiguration _activeDataEntryConfig;

        /// <summary>
        /// The current document type
        /// </summary>
        string _activeDocumentType;

        /// <summary>
        /// Indicates whether the document type is in the process of being changed.
        /// </summary>
        bool _changingDocumentType;

        /// <summary>
        /// An undefined document type that should temporarily be made available in
        /// _documentTypeComboBox so that the document can be saved with its original DocumentType.
        /// </summary>
        string _temporaryDocumentType;

        /// <summary>
        /// The <see cref="IAttribute"/> that contains the DocumentType value.
        /// </summary>
        IAttribute _documentTypeAttribute;

        /// <summary>
        /// The <see cref="AttributeStatusInfo"/> associated with _documentTypeAttribute
        /// </summary>
        AttributeStatusInfo _documentTypeAttributeStatusInfo;

        /// <summary>
        /// The data entry panel control host implementation to be used by the application.
        /// </summary>
        DataEntryControlHost _dataEntryControlHost;

        /// <summary>
        /// A <see cref="Form"/> to display the <see cref="ImageViewer"/> in a separate window.
        /// </summary>
        Form _imageViewerForm;

        /// <summary>
        /// Processes shortcuts when the image viewer is being displayed in a separate form.
        /// </summary>
        ShortcutsMessageFilter _imageWindowShortcutsMessageFilter;

        /// <summary>
        /// Indicates whether the <see cref="DataEntryApplicationForm"/> is in standalone mode 
        /// (<see langref="true"/>) or whether another application has launched 
        /// <see cref="DataEntryApplicationForm"/> via the COM interface (<see langref="false"/>).
        /// </summary>
        readonly bool _standAloneMode;

        /// <summary>
        /// Indicates whether the form has finished loading.
        /// </summary>
        bool _isLoaded;

        /// <summary>
        /// The <see cref="FileProcessingDB"/> in use.
        /// </summary>
        FileProcessingDB _fileProcessingDb;

        /// <summary>
        /// The <see cref="IFileRequestHandler"/> that can be used by the verification task to carry
        /// out requests for files to be checked out, released or re-ordered in the queue.
        /// </summary>
        IFileRequestHandler _fileRequestHandler;

        /// <summary>
        /// The ID of the file being processed.
        /// </summary>
        int _fileId;

        /// <summary>
        /// The name of the file being processed.
        /// </summary>
        string _fileName;

        /// <summary>
        /// The ID of the action being processed.
        /// </summary>
        int _actionId;

        /// <summary>
        /// The name of the action being processed.
        /// </summary>
        string _actionName;

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
        /// Allows data entry specific database operations.
        /// </summary>
        DataEntryProductDBMgr _dataEntryDatabaseManager;

        /// <summary>
        /// Specifies whether counts will be recorded for the defined data entry counters (only if
        /// enabled both in the task and in the database);
        /// </summary>
        bool _countersEnabled;
        
        /// <summary>
        /// Tracks user input in the file processing database.
        /// </summary>
        InputEventTracker _inputEventTracker;

        /// <summary>
        /// Keeps track of the time spent verifying a file.
        /// </summary>
        Stopwatch _fileProcessingStopwatch;

        /// <summary>
        /// The time in seconds between the previous document being saved/closed and the current
        /// document being displayed. If this is the first document displayed (or the first since
        /// the last call to<see cref="Standby"/>, this time will be 0.
        /// </summary>
        double? _overheadElapsedTime;

        /// <summary>
        /// Indicates whether an entry has been added to the DataEntryData table for the current
        /// document.
        /// </summary>
        bool _recordedDatabaseData;

        /// <summary>
        /// The close file command
        /// </summary>
        ApplicationCommand _closeFileCommand;

        /// <summary>
        /// The save and commit file command
        /// </summary>
        ApplicationCommand _saveAndCommitFileCommand;

        /// <summary>
        /// The goto next invalid item command.
        /// </summary>
        ApplicationCommand _gotoNextInvalidCommand;

        /// <summary>
        /// The goto next unviewed item command.
        /// </summary>
        ApplicationCommand _gotoNextUnviewedCommand;

        /// <summary>
        /// The toggle highlight tool command.
        /// </summary>
        ApplicationCommand _toggleHighlightCommand;

        /// <summary>
        /// The angular highlight tool command.
        /// </summary>
        ApplicationCommand _selectAngularHighlightCommand;

        /// <summary>
        /// The rectangular highlight tool command.
        /// </summary>
        ApplicationCommand _selectRectangularHighlightCommand;

        /// <summary>
        /// The word highlight tool command.
        /// </summary>
        ApplicationCommand _selectWordHighlightCommand;
         
        /// <summary>
        /// The hide tooltips command.
        /// </summary>
        ApplicationCommand _hideToolTipsCommand;

        /// <summary>
        /// The toggle show all data highlights command
        /// </summary>
        ApplicationCommand _toggleShowAllHighlightsCommand;

        /// <summary>
        /// The accept spatial info command
        /// </summary>
        ApplicationCommand _acceptSpatialInfoCommand;

        /// <summary>
        /// The remove spatial info command
        /// </summary>
        ApplicationCommand _removeSpatialInfoCommand;

        /// <summary>
        /// The undo command.
        /// </summary>
        ApplicationCommand _undoCommand;

        /// <summary>
        /// The redo command.
        /// </summary>
        ApplicationCommand _redoCommand;

        /// <summary>
        /// DatabaseConnectionInfo instances to be used for any validation or auto-update queries
        /// requiring a database; The key is the connection name (blank for default connection).
        /// </summary>
        Dictionary<string, DatabaseConnectionInfo> _dbConnections =
            new Dictionary<string, DatabaseConnectionInfo>();

        /// <summary>
        /// The user-specified settings for the data entry application.
        /// </summary>
        readonly UserPreferences _userPreferences;

        /// <summary>
        /// The dialog for setting user preferences.
        /// </summary>
        PropertyPageForm _userPreferencesDialog;

        /// <summary>
        /// Tool strip menu item for opening a new image.
        /// </summary>
        OpenImageToolStripMenuItem _openImageToolStripMenuItem;

        /// <summary>
        /// Tool strip menu item for closing the currently open image.
        /// </summary>
        CloseImageToolStripMenuItem _closeImageToolStripMenuItem;

        /// <summary>
        /// Tool strip menu item for saving and committing the current image.
        /// </summary>
        ToolStripMenuItem _saveAndCommitMenuItem = CreateSaveAndCommitMenuItem();

        /// <summary>
        /// Tool strip menu item for saving the current image.
        /// </summary>
        ToolStripMenuItem _saveMenuItem = CreateDisabledMenuItem("Save");

        /// <summary>
        /// Tool strip menu item for printing the current image.
        /// </summary>
        PrintImageToolStripMenuItem _printMenuItem = CreatePrintImageMenuItem();

        /// <summary>
        /// Tool strip menu item to skip processing the current document.
        /// </summary>
        ToolStripMenuItem _skipProcessingMenuItem;

        /// <summary>
        /// Tool strip menu item to exit the application or stop processing.
        /// </summary>
        ToolStripMenuItem _exitToolStripMenuItem = new ToolStripMenuItem("E&xit");

        /// <summary>
        /// During image load events, updates to the scroll panel will be locked to prevent excess
        /// scrolling. This flag tracks the fact that the panel needs to be scrolled to the top
        /// on the next control selection change that occurs (via ItemSelectionChanged).
        /// </summary>
        bool _scrollToTopRequired;

        /// <summary>
        /// Used to invoke methods on this control.
        /// </summary>
        readonly ControlInvoker _invoker;

        /// <summary>
        /// The number of document that have been loaded in this session of the
        /// <see cref="DataEntryApplicationForm"/>.
        /// </summary>
        uint _documentLoadCount;

        /// <summary>
        /// Indicates whether all highlights are currently being shown.
        /// </summary>
        bool _showAllHighlights;

        /// <summary>
        /// Indicates whether tabbing by row/group is currently enabled.
        /// </summary>
        bool _allowTabbingByGroup = true;

        /// <summary>
        /// The comment loaded or to be stored the file processing database.
        /// </summary>
        string _fileProcessingDBComment;

        /// <summary>
        /// Indicates if the form should be invisible (for the purposes of loading data instead of
        /// verification).
        /// </summary>
        bool _invisible;

        /// <summary>
        /// https://extract.atlassian.net/browse/ISSUE-13573
        /// Used to prevent simultaneous modification of a voa file from multiple Extract Systems
        /// processes.
        /// </summary>
        ExtractFileLock _voaFileLock = new ExtractFileLock();

        /// <summary>
        /// Maps document names to VOA file data that as been cached for use for the file.
        /// </summary>
        Dictionary<string, IUnknownVector> _cachedVOAData = new Dictionary<string, IUnknownVector>();

        /// <summary>
        /// This checks the user.config file to make sure it is not corrupt when the each instance 
        /// is created and when each instance is destroyed and if it is good makes a backup and 
        /// if corrupted will replace with the backup if available otherwise the config file will be deleted
        /// https://extract.atlassian.net/browse/ISSUE-12830
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        UserConfigChecker _userConfigChecker = new UserConfigChecker();

        /// <summary>
        /// A panel that is available to view/edit key data fields associated with either physical
        /// or proposed paginated documents.
        /// </summary>
        IPaginationDocumentDataPanel _paginationDocumentDataPanel;

        /// <summary>
        /// Attributes that have been modified in the _paginationDocumentDataPanel and need to be
        /// refreshed in the DEP.
        /// </summary>
        HashSet<IAttribute> _paginagionAttributesToRefresh = new HashSet<IAttribute>();

        /// <summary>
        /// Keeps track of suggested pagination output that was accepted and should be loaded ahead
        /// of any documents currently processing in this instance.
        /// </summary>
        Queue<string> _paginationOutputOrder = new Queue<string>();

        /// <summary>
        /// Indicates whether a pagination event from the <see cref="_paginationPanel"/> is
        /// currently being processed.
        /// </summary>
        bool _paginating;

        #endregion Fields

        #region Constructors

        /// <overloads>Initializes a new instance of the <see cref="DataEntryApplicationForm"/> 
        /// class.</overloads>
        /// <summary>
        /// Initializes a new instance of the <see cref="DataEntryApplicationForm"/> class in 
        /// stand alone mode.
        /// </summary>
        /// <param name="settings">The <see cref="VerificationSettings"/>.</param>
        public DataEntryApplicationForm(VerificationSettings settings)
            : this(settings, true, null, 0, null, null)
        {
        }

        /// <summary>
        /// Initializes a new <see cref="DataEntryApplicationForm"/> class.
        /// </summary>
        /// <param name="settings">The <see cref="VerificationSettings"/>.</param>
        /// <param name="standAloneMode"><see langref="true"/> if the created as a standalone 
        /// application; <see langref="false"/> if launched via the COM interface.</param>
        /// <param name="fileProcessingDB">The <see cref="FileProcessingDB"/> in use or
        /// <see langword="null"/> if no file processing database is being used.</param>
        /// <param name="actionId">The ID of the file processing action currently being used.
        /// </param>
        /// <param name="tagManager">The <see cref="FAMTagManager"/> used to expand path tags and
        /// functions.</param>
        /// <param name="fileRequestHandler">The <see cref="IFileRequestHandler"/> that can be used
        /// by the verification task to carry out requests for files to be checked out, released or
        /// re-ordered in the queue.</param>
        public DataEntryApplicationForm(VerificationSettings settings, bool standAloneMode,
            FileProcessingDB fileProcessingDB, int actionId, FAMTagManager tagManager,
            IFileRequestHandler fileRequestHandler)
        {
            try
            {
                // Load licenses in design mode
                if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
                {
                    // Load the license files from folder
                    LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());
                }

                // Validate the license
                LicenseUtilities.ValidateLicense(
                    LicenseIdName.DataEntryCoreComponents, "ELI23668", _OBJECT_NAME);

                // License SandDock before creating the form
                SandDockManager.ActivateProduct(_SANDDOCK_LICENSE_STRING);

                _settings = settings;

                if (tagManager == null)
                {
                    // A FAMTagManager without path tags is better than no tag manager (still can
                    // be used to expand path functions).
                    tagManager = new FAMTagManager();
                }

                _tagUtility = (ITagUtility)tagManager;

                string expandedConfigFileName = 
                    tagManager.ExpandTagsAndFunctions(settings.ConfigFileName, null);

                // Initialize the root directory the DataEntry framework should use when resolving
                // relative paths.
                DataEntryMethods.SolutionRootDirectory =
                    Path.GetDirectoryName(expandedConfigFileName);

                // Initialize the application settings.
                _applicationConfig = new ConfigSettings<Properties.Settings>(
                    expandedConfigFileName, false, false, _tagUtility);

                // Initialize the user registry settings.
                _registry = new RegistrySettings<Properties.Settings>(
                    @"Software\Extract Systems\DataEntry");

                _brandingResources = new BrandingResourceManager(
                    DataEntryMethods.ResolvePath(_applicationConfig.Settings.ApplicationResourceFile));

                AttributeStatusInfo.DisableAutoUpdateQueries =
                    _applicationConfig.Settings.DisableAutoUpdateQueries;

                AttributeStatusInfo.DisableValidationQueries =
                    _applicationConfig.Settings.DisableValidationQueries;

                // Since SpotIR compatibility is not required for data entry applications, avoid the
                // performance hit it exacts.
                Highlight.SpotIRCompatible = false;

                _standAloneMode = standAloneMode;
                _fileProcessingDb = fileProcessingDB;
                _actionId = actionId;
                _fileRequestHandler = fileRequestHandler;

                if (settings.InputEventTrackingEnabled || settings.CountersEnabled)
                {
                    ExtractException.Assert("ELI29828", "Cannot enable " +
                        ((settings.InputEventTrackingEnabled && settings.CountersEnabled)
                            ? "input tracking or data counters"
                            : settings.InputEventTrackingEnabled
                                ? "input tracking"
                                : "counters") +
                        " without access to a file processing database!", _fileProcessingDb != null);
                }

                // Whether to enable data entry counters depends upon the DBInfo setting as
                // well as the task configuration.
                if (_settings.CountersEnabled)
                {
                    _countersEnabled =
                        _fileProcessingDb.GetDBInfoSetting("EnableDataEntryCounters", true)
                            .Equals("1", StringComparison.OrdinalIgnoreCase);
                }

                // Get the action name if there is an associated action ID.
                if (_fileProcessingDb != null)
                {
                    _actionName = _fileProcessingDb.GetActionName(_actionId);
                }
                
                InitializeComponent();

                if (_brandingResources.ApplicationIcon != null)
                {
                    Icon = (Icon)_brandingResources.ApplicationIcon.Clone();
                }

                if (string.IsNullOrEmpty(_brandingResources.HelpFilePath))
                {
                    _appHelpMenuItem.Visible = false;
                }
                else
                {
                    _appHelpMenuItem.Text = _brandingResources.ApplicationTitle + " &help...";
                }

                _aboutMenuItem.Text = "&About " + _brandingResources.ApplicationTitle + "...";


                // Need to hide _openFileToolStripButton in FAM mode by searching for it.
                if (!_standAloneMode)
                {
                    foreach (ToolStripItem item in _fileCommandsToolStrip.Items)
                    {
                        if (item is OpenImageToolStripSplitButton)
                        {
                            item.Visible = false;

                            break;
                        }
                    }

                    if (_settings.AllowTags)
                    {
                        _tagFileToolStripButton.TagSettings = _settings.TagSettings;
                    }
                    else
                    {
                        _tagFileToolStripButton.Visible = false;
                    }
                }
                else
                {
                    // If in stand-alone mode, the _tagFileToolStripButton isn't useful.
                    _tagFileToolStripButton.Visible = false;
                }

                // Add the file tool strip items
                AddFileToolStripItems(_fileToolStripMenuItem.DropDownItems);

                // Read the user preferences object from the registry
                _userPreferences = UserPreferences.FromRegistry();

                // Load all configurations defined in configFileName
                LoadDataEntryConfigurations(expandedConfigFileName);

                // If a default configuration exists, use it to begin with.
                _activeDataEntryConfig = _defaultDataEntryConfig;

                if (!_standAloneMode)
                {
                    _exitToolStripMenuItem.Text = "Stop processing";
                    _imageViewer.DefaultStatusMessage = "Waiting for next document...";
                }

                // Apply the persisted auto-OCR settings.
                _imageViewer.AutoOcr = _registry.Settings.AutoOcr;
                _imageViewer.OcrTradeoff = _registry.Settings.OcrTradeoff;

                // For pre-fetching purposes, allow the ImageViewer to cache images.
                _imageViewer.CacheImages = true;

                if (_paginationPanel != null)
                {
                    _paginationPanel.ImageViewer = _imageViewer;
                    _paginationPanel.ExpectedPaginationAttributesPath =
                        _settings.PaginationSettings.ExpectedPaginationAttributesOutputPath;
                    _paginationPanel.OutputExpectedPaginationAttributesFile =
                        _settings.PaginationSettings.OutputExpectedPaginationAttributesFiles;
                    _paginationPanel.FileProcessingDB = FileProcessingDB;
                }

                _invoker = new ControlInvoker(this);
            }
            catch (Exception ex)
            {
                // [LegacyRCAndUtils:6190]
                // If an exception is thrown from the constructor and, as a result, the form is
                // not disposed of, this seems to lead to a crash when the application is closed.
                Dispose();

                throw ExtractException.AsExtractException("ELI23669", ex);
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
                // [DataEntry:614]
                // Don't use the Window's theme for Windows Vista or later since the Aero theme
                // hides the color applied to the active control when the active control is a
                // drop-list combo box.
                if (Environment.OSVersion.Platform == PlatformID.Win32NT &&
                    Environment.OSVersion.Version.Major >= 6)
                {
                    return false;
                }

                // The Windows XP theme doesn't cause any problems
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
        /// Gets the active <see cref="DataEntryControlHost"/>.
        /// </summary>
        public DataEntryControlHost ActiveDataEntryControlHost
        {
            get
            {
                return (_activeDataEntryConfig == null)
                    ? null
                    : _activeDataEntryConfig.DataEntryControlHost;
            }
        }

        #endregion Properties

        #region Public Methods

        /// <summary>
        /// Specifies that the form should be invisible (for the purposes of loading data instead of
        /// verification).
        /// </summary>
        public void MakeInvisible()
        {
            try
            {
                if (!_invisible)
                {
                    this.MakeFormInvisible();

                    if (_magnifierDockableWindow.IsOpen)
                    {
                        _magnifierDockableWindow.Close();
                    }

                    if (_thumbnailDockableWindow.IsOpen)
                    {
                        _thumbnailDockableWindow.Close();
                    }
                    if (_imageViewerForm != null)
                    {
                        CloseSeparateImageWindow();
                    }

                    _invisible = true;
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35080");
            }
        }

        /// <summary>
        /// Loads the type of the correct config for document.
        /// </summary>
        /// <param name="attributes">The attributes representing the document data.</param>
        /// <returns><see langword="true"/> if the configuration was changed based on the data;
        /// otherwise, <see langword="false"/>.</returns>
        public bool LoadCorrectConfigForData(IUnknownVector attributes)
        {
            try
            {
                // If there were document type specific configurations defined, apply the
                // appropriate configuration now.
                bool changedDocumentType, changedDataEntryConfig;
                if (_documentTypeConfigurations != null)
                {
                    string documentType = GetDocumentType(attributes, false);

                    // If there is a default configuration, add the original document type to the
                    // document type combo and allow the document to be saved with the undefined
                    // document type.
                    if (_defaultDataEntryConfig != null &&
                        _documentTypeComboBox.FindStringExact(documentType) == -1)
                    {
                        _temporaryDocumentType = documentType;
                        _documentTypeComboBox.Items.Insert(0, documentType);
                    }

                    SetActiveDocumentType(documentType, true,
                        out changedDocumentType, out changedDataEntryConfig);
                }
                else
                {
                    changedDocumentType = false;
                    changedDataEntryConfig = false;
                }

                return changedDataEntryConfig;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35079");
            }
        }

        #endregion Public Methods

        #region Methods

        /// <summary>
        /// Adds the file tool strip menu items to the specified tool strip item collection.
        /// </summary>
        /// <param name="items">The collection to which the file tool strip menu item should be 
        /// added.</param>
        void AddFileToolStripItems(ToolStripItemCollection items)
        {
            // Only open or close images in stand alone mode
            if (_standAloneMode)
            {
                _openImageToolStripMenuItem = CreateOpenImageMenuItem();
                _closeImageToolStripMenuItem = CreateCloseImageMenuItem();
                ToolStripSeparator separator = new ToolStripSeparator();

                items.AddRange(new ToolStripItem[] 
                    { _openImageToolStripMenuItem, _closeImageToolStripMenuItem, separator });
            }

            // Add the save and commit menu item (created in constructor)
            items.Add(_saveAndCommitMenuItem);

            // Only add the save menu item if not running in stand alone
            if (!_standAloneMode)
            {
                _saveMenuItem = CreateDisabledMenuItem("Save");
                items.Add(_saveMenuItem);
            }

            // Add the print image menu item
            items.AddRange(new ToolStripItem[] { _printMenuItem, new ToolStripSeparator() });

            // Only add skip processing if not in stand alone mode and _fileProcessingDb is
            // available.
            if (!_standAloneMode && _fileProcessingDb != null)
            {
                _skipProcessingMenuItem = CreateDisabledMenuItem("Skip document");
                items.Add(_skipProcessingMenuItem);
            }

            // Add the exit menu item
            items.Add(_exitToolStripMenuItem);
        }

        /// <summary>
        /// Creates a menu item to open images.
        /// </summary>
        /// <returns>A menu item to open images.</returns>
        static OpenImageToolStripMenuItem CreateOpenImageMenuItem()
        {
            OpenImageToolStripMenuItem menuItem = new OpenImageToolStripMenuItem();
            menuItem.Enabled = false;
            menuItem.Text = "&Open...";
            return menuItem;
        }

        /// <summary>
        /// Creates a menu item to close images.
        /// </summary>
        /// <returns>A menu item to close images.</returns>
        static CloseImageToolStripMenuItem CreateCloseImageMenuItem()
        {
            CloseImageToolStripMenuItem menuItem = new CloseImageToolStripMenuItem();
            menuItem.Enabled = false;
            menuItem.Text = "&Close";
            return menuItem;
        }

        /// <summary>
        /// Creates a menu item to print images.
        /// </summary>
        /// <returns>A menu item to print images.</returns>
        static PrintImageToolStripMenuItem CreatePrintImageMenuItem()
        {
            PrintImageToolStripMenuItem menuItem = new PrintImageToolStripMenuItem();
            menuItem.Enabled = false;
            menuItem.Text = "&Print...";
            return menuItem;
        }

        /// <summary>
        /// Creates a menu item to commit and save images.
        /// </summary>
        static ToolStripMenuItem CreateSaveAndCommitMenuItem()
        {
            ToolStripMenuItem menuItem = CreateDisabledMenuItem("&Save");
            menuItem.Image = Resources.SaveImageButtonSmall;
            menuItem.ShortcutKeyDisplayString = "Ctrl+S";
            return menuItem;
        }

        /// <summary>
        /// Creates a disabled tool strip menu item.
        /// </summary>
        /// <param name="text">The text of the tool strip menu item.</param>
        /// <returns>A disabled tool strip menu item with <paramref name="text"/>.</returns>
        static ToolStripMenuItem CreateDisabledMenuItem(string text)
        {
            ToolStripMenuItem menuItem = new ToolStripMenuItem(text);
            menuItem.Enabled = false;
            return menuItem;
        }

        /// <summary>
        /// A thread-safe method that opens the specified document.
        /// </summary>
        /// <param name="fileName">The filename of the document to open.</param>
        /// <param name="fileID">The ID of the file being processed.</param>
        /// <param name="actionID">The ID of the action being processed.</param>
        /// <param name="tagManager">The <see cref="FAMTagManager"/> to use if needed.</param>
        /// <param name="fileProcessingDB">The <see cref="FileProcessingDB"/> in use or
        /// <see langword="null"/> if no file processing database is being used.</param>
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
                _cachedVOAData.Remove(fileName);

                // In order to keep the order documents are displayed in sync with the
                // _paginationPanel, swap out the loading file for another if necessary.
                if (_paginationPanel != null && ReorderAccordingToPagination(fileName))
                {
                    AbortProcessing(EFileProcessingResult.kProcessingDelayed, false);
                    return;
                }

                ExtractException.Assert("ELI29830", "Unexpected file processing database!",
                    _fileProcessingDb == fileProcessingDB);
                ExtractException.Assert("ELI29831", "Unexpected database action ID!",
                    _fileProcessingDb == null || _actionId == actionID);

                _fileId = fileID;
                _fileName = fileName;
                _fileTaskSessionID = null;
                if (_paginationPanel != null)
                {
                    _paginationPanel.FileTaskSessionID = null;
                }

                StartFileTaskSession();

                _tagFileToolStripButton.Database = fileProcessingDB;
                _tagFileToolStripButton.FileId = fileID;
                // For consistency with other buttons, keep disabled until the file is loaded.
                _tagFileToolStripButton.Enabled = false;

                if (_settings.InputEventTrackingEnabled && _inputEventTracker == null)
                {
                    _inputEventTracker = new InputEventTracker(fileProcessingDB, actionID);
                }

                if (_dataEntryDatabaseManager == null && fileProcessingDB != null)
                {
                    FAMDBUtils famDBUtils = new FAMDBUtils();
                    Type mgrType = Type.GetTypeFromProgID(famDBUtils.GetDataEntryDBProgId());

                    _dataEntryDatabaseManager = (DataEntryProductDBMgr)Activator.CreateInstance(mgrType);
                    _dataEntryDatabaseManager.Initialize(fileProcessingDB);
                }

                if (_paginationPanel != null)
                {
                    LoadDocumentForPagination(fileName);

                    if (_paginationPanel.IsPaginationSuggested(fileName))
                    {
                        // When jumping straight into pagination, still update the title bar to
                        // reflect the active document.
                        string imageName = Path.GetFileName(fileName);
                        base.Text = imageName + " - " + _brandingResources.ApplicationTitle;
                        if (_imageViewerForm != null)
                        {
                            _imageViewerForm.Text = imageName + " - " +
                                _brandingResources.ApplicationTitle + " Image Window";
                        }

                        _tabControl.SelectedTab = _paginationTab;
                        _paginationPanel.PendingChanges = true;

                        // If pagination has been suggested, don't bother loading the data for the
                        // current document; either the suggestion will be accepted trigger new
                        // files to be loaded or the suggestion will be rejected triggering the
                        // document to be sent back to the rules.
                        return;
                    }
                    else
                    {
                        _tabControl.SelectedTab = _dataTab;
                    }
                }

                _imageViewer.OpenImage(fileName, false);

                if (_fileProcessingDb != null && _dataEntryControlHost != null)
                {
                    _fileProcessingDBComment =
                        _fileProcessingDb.GetFileActionComment(_fileId, _actionId);
                }
                else
                {
                    _fileProcessingDBComment = null;
                }
            }
            catch (Exception ex)
            {
                RaiseVerificationException("ELI23871", ex, true);
            }
            finally
            {
                // Cached VOA file data should only be used within the context of the Open call.
                _cachedVOAData.Remove(fileName);
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
                _imageViewer.CacheImage(fileName);

                // [DataEntry:1151]
                // It appears in some cases the image viewer ends up trying to display highlights
                // being loaded via prefetch. I don't feel comfortable that I can make a low-risk
                // fix for this issue right now. Therefore I am disabling highlight loading as part
                // of pre-fetch.
                //_dataEntryControlHost.Prefetch(fileName);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI34533");
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

                // Do not count time in standby toward overhead time.
                if (_fileProcessingStopwatch != null && _fileProcessingStopwatch.IsRunning)
                {
                    _fileProcessingStopwatch.Stop();
                }

                return true;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI33953");
            }
        }

        #endregion Methods

        #region Overrides

        /// <summary>
        /// Initializes the application by establishing connections for all 
        /// <see cref="IImageViewerControl"/>s.
        /// </summary>
        /// <param name="e">A <see cref="EventArgs"/> that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            try
            {
                // Before Loading the state make sure the config is still valid
				// https://extract.atlassian.net/browse/ISSUE-12830
                UserConfigChecker.EnsureValidUserConfigFile();

                base.OnLoad(e);

                if (!_standAloneMode && _settings.PaginationEnabled)
                {
                    ValidationPaginationActions();

                    LoadPaginationDocumentDataPanel();

                    // Show pagination tab before showing the data tab to trigger the pagination
                    // control to be created and loaded.
                    _paginationTab.Show();
                    _dataTab.Show();
                }
                else
                {
                    // If pagination is not needed, move the panel that houses the DEP out of the
                    // tab control and directly into the left side of _splitContainer. 
                    _dataTab.Controls.Remove(_scrollPanel);
                    _splitContainer.Panel1.Controls.Remove(_tabControl);
                    _splitContainer.Panel1.Controls.Add(_scrollPanel);

                    _dataTab = null;
                    _tabControl = null;
                    _paginationTab = null;
                    _paginationPanel = null;
                }

                // Set the application name
                if (string.IsNullOrEmpty(_actionName))
                {
                    base.Text = _brandingResources.ApplicationTitle;
                }
                else
                {
                    // [DataEntry:740] Show the name of the current action in the title bar.
                    base.Text = "Waiting - " + _brandingResources.ApplicationTitle +
                        " (" + _actionName + ")";
                }

                _magnifierToolStripButton.DockableWindow = _magnifierDockableWindow;
                if (_paginationPanel == null)
                {
                    _thumbnailsToolStripButton.DockableWindow = _thumbnailDockableWindow;
                }
                else
                {
                    // If pagination is available, the thumbnail pane is redundant and does not
                    // currently play well with the pagination panel. Disallow use of the thumbnail
                    // window in this configuration.
                    _thumbnailsToolStripButton.Visible = false;
                }

                // Establish shortcut keys

                // Open an image
                if (_standAloneMode)
                {
                    _imageViewer.Shortcuts[Keys.O | Keys.Control] = _imageViewer.SelectOpenImage;
                }

                // Close an image
                _closeFileCommand = new ApplicationCommand(_imageViewer.Shortcuts,
                    new Keys[] { Keys.Control | Keys.F4 }, _imageViewer.CloseImage,
                    GetCloseFileToolStripItems(), false, _standAloneMode, _standAloneMode);

                // Save
                _saveAndCommitFileCommand = new ApplicationCommand(_imageViewer.Shortcuts,
                    new Keys[] { Keys.S | Keys.Control }, SaveAndCommit,
                    new ToolStripItem[] { _saveAndCommitButton, _saveAndCommitMenuItem },
                    false, true, false);

                // Print an image
                _imageViewer.Shortcuts[Keys.P | Keys.Control] = _imageViewer.SelectPrint;

                // Goto next invalid
                _gotoNextInvalidCommand = new ApplicationCommand(_imageViewer.Shortcuts,
                    new Keys[] { Keys.F3 }, null,
                    new ToolStripItem[] { _nextInvalidToolStripButton, 
                        _nextInvalidToolStripMenuItem }, true, true, false);

                // Goto next unviewed
                _gotoNextUnviewedCommand = new ApplicationCommand(_imageViewer.Shortcuts,
                    new Keys[] { Keys.F4 }, null,
                    new ToolStripItem[] { _nextUnviewedToolStripButton, 
                        _nextUnviewedToolStripMenuItem }, true, true, false);

                // Zoom tool
                _imageViewer.Shortcuts[Keys.Alt | Keys.Z] =
                    _imageViewer.SelectZoomWindowTool;

                // Pan tool
                _imageViewer.Shortcuts[Keys.Alt | Keys.A] = _imageViewer.SelectPanTool;

                // Review and select tool
                _imageViewer.Shortcuts[Keys.Alt | Keys.R] = _imageViewer.SelectSelectLayerObjectsTool;

                // Toggle highlight tool
                _toggleHighlightCommand = new ApplicationCommand(_imageViewer.Shortcuts,
                    new Keys[] {Keys.Alt | Keys.S}, _imageViewer.ToggleHighlightTool, null, false, 
                    true, false);

                // Swipe angular zone tool
                _selectAngularHighlightCommand = new ApplicationCommand(_imageViewer.Shortcuts, 
                    null, _imageViewer.SelectAngularHighlightTool, 
                    new ToolStripItem[] { _angularHighlightToolStripMenuItem, 
                        _angularHighlightToolStripButton}, false, true, false);

                // Swipe rectangular zone tool
                _selectRectangularHighlightCommand = new ApplicationCommand(_imageViewer.Shortcuts, 
                    null, _imageViewer.SelectRectangularHighlightTool, 
                    new ToolStripItem[] { _rectangularHighlightToolStripMenuItem, 
                        _rectangularHighlightToolStripButton }, false, true, false);

                // Swipe word tool
                _selectWordHighlightCommand = new ApplicationCommand(_imageViewer.Shortcuts,
                    null, _imageViewer.SelectWordHighlightTool,
                    new ToolStripItem[] { _wordHighlightToolStripMenuItem,
                        _wordHighlightToolStripButton }, false, true, false);

                // Fit to page
                _imageViewer.Shortcuts[Keys.Alt | Keys.P] = _imageViewer.ToggleFitToPageMode;

                // Fit to width
                _imageViewer.Shortcuts[Keys.Alt | Keys.W] = _imageViewer.ToggleFitToWidthMode;

                // Go to first page
                _imageViewer.Shortcuts[Keys.Control | Keys.Home] = _imageViewer.GoToFirstPage;

                // Go to the next page
                _imageViewer.Shortcuts[Keys.PageDown] = _imageViewer.GoToNextPage;

                // Go to the previous page
                _imageViewer.Shortcuts[Keys.PageUp] = _imageViewer.GoToPreviousPage;

                // Go to last page
                _imageViewer.Shortcuts[Keys.Control | Keys.End] = _imageViewer.GoToLastPage;

                // Zoom in
                _imageViewer.Shortcuts[Keys.F7] = _imageViewer.SelectZoomIn;
                _imageViewer.Shortcuts[Keys.Add | Keys.Control] = _imageViewer.SelectZoomIn;
                _imageViewer.Shortcuts[Keys.Oemplus | Keys.Control] = _imageViewer.SelectZoomIn;

                // Zoom out
                _imageViewer.Shortcuts[Keys.F8] = _imageViewer.SelectZoomOut;
                _imageViewer.Shortcuts[Keys.Subtract | Keys.Control] = _imageViewer.SelectZoomOut;
                _imageViewer.Shortcuts[Keys.OemMinus | Keys.Control] = _imageViewer.SelectZoomOut;

                // Zoom previous
                _imageViewer.Shortcuts[Keys.Alt | Keys.Left] = _imageViewer.SelectZoomPrevious;

                // Zoom next
                _imageViewer.Shortcuts[Keys.Alt | Keys.Right] = _imageViewer.SelectZoomNext;

                // Rotate clockwise
                _imageViewer.Shortcuts[Keys.R | Keys.Control] = _imageViewer.SelectRotateClockwise;

                // Rotate counterclockwise
                _imageViewer.Shortcuts[Keys.R | Keys.Control | Keys.Shift] =
                    _imageViewer.SelectRotateCounterclockwise;

                // Toggle tab by row/group mode
                _imageViewer.Shortcuts[Keys.F9] = ToggleAllowTabbingByGroup;
                _allowTabbingByGroupToolStripMenuItem.CheckState =
                    _allowTabbingByGroup ? CheckState.Checked : CheckState.Unchecked;

                // Toggle showing image viewer in a separate window.
                _imageViewer.Shortcuts[Keys.F11] = ToggleSeparateImageWindow;
                _separateImageWindowToolStripMenuItem.CheckState =
                    _registry.Settings.ShowSeparateImageWindow
                        ? CheckState.Checked
                        : CheckState.Unchecked;

                // Toggle showing magnifier pane.
                _imageViewer.Shortcuts[Keys.F12] = (() => _magnifierToolStripButton.PerformClick());

               // Hide any visible toolTips
                _hideToolTipsCommand = new ApplicationCommand(_imageViewer.Shortcuts,
                    new Keys[] { Keys.Escape }, null,
                    new ToolStripItem[] { _hideToolTipsMenuItem }, false, true, false);

                // Toggle show all data highlights
                _toggleShowAllHighlightsCommand = new ApplicationCommand(_imageViewer.Shortcuts,
                    new Keys[] { Keys.F10 }, ToggleShowAllHighlights,
                    new ToolStripItem[] { _toggleShowAllHighlightsButton, 
                        _toggleShowAllHighlightsMenuItem }, false, true, false);

                // Accept spatial info command
                _acceptSpatialInfoCommand = new ApplicationCommand(_imageViewer.Shortcuts,
                    new Keys[] { Keys.T | Keys.Control }, null,
                    new ToolStripItem[] { _acceptImageHighlightMenuItem }, false, true, false);

                // Remove spatial info command
                _removeSpatialInfoCommand = new ApplicationCommand(_imageViewer.Shortcuts,
                    new Keys[] { Keys.D | Keys.Control }, null,
                    new ToolStripItem[] { _removeImageHighlightMenuItem }, false, true, false);

                // Undo command
                _undoCommand = new ApplicationCommand(_imageViewer.Shortcuts,
                    new Keys[] { Keys.Z | Keys.Control }, null,
                    new ToolStripItem[] { _undoToolStripButton, _undoToolStripMenuItem },
                    false, true, false);

                // Redo command
                _redoCommand = new ApplicationCommand(_imageViewer.Shortcuts,
                    new Keys[] { Keys.Y | Keys.Control }, null,
                    new ToolStripItem[] { _redoToolStripButton, _redoToolStripMenuItem },
                    false, true, false);

                _imageViewer.Shortcuts[Keys.F6] = ToggleAutoZoom;

                // Disable the OpenImageToolStripSplitButton if this is not stand alone mode
                if (!_standAloneMode)
                {
                    foreach (ToolStripItem item in _fileCommandsToolStrip.Items)
                    {
                        if (item is OpenImageToolStripSplitButton)
                        {
                            item.Enabled = false;
                            break;
                        }
                    }
                }

                // Register for events.
                _imageViewer.ImageFileChanged += HandleImageFileChanged;
                _imageViewer.ImageFileClosing += HandleImageFileClosing;
                _imageViewer.LoadingNewImage += HandleLoadingNewImage;
                _saveAndCommitMenuItem.Click += HandleSaveAndCommitClick;
                _saveAndCommitButton.Click += HandleSaveAndCommitClick;
                if (!_standAloneMode)
                {
                    _saveMenuItem.Click += HandleSaveClick;

                    if (_fileProcessingDb != null)
                    {
                        _skipProcessingMenuItem.Click += HandleSkipFileClick;
                    }
                }
                _exitToolStripMenuItem.Click += HandleExitToolStripMenuItemClick;
                _nextUnviewedToolStripButton.Click += HandleGoToNextUnviewedClick;
                _nextUnviewedToolStripMenuItem.Click += HandleGoToNextUnviewedClick;
                _nextInvalidToolStripButton.Click += HandleGoToNextInvalidClick;
                _nextInvalidToolStripMenuItem.Click += HandleGoToNextInvalidClick;
                _toggleShowAllHighlightsButton.Click += HandleToggleShowAllHighlightsClick;
                _toggleShowAllHighlightsMenuItem.Click += HandleToggleShowAllHighlightsClick;
                _hideToolTipsMenuItem.Click += HandleHideToolTipsClick;
                _acceptImageHighlightMenuItem.Click += HandleAcceptImageHighlightClick;
                _removeImageHighlightMenuItem.Click += HandleRemoveImageHighlightClick;
                _splitContainer.SplitterMoved += HandleSplitterMoved;
                _aboutMenuItem.Click += HandleAboutMenuItemClick;
                _appHelpMenuItem.Click += HandleHelpMenuItemClick;
                _optionsToolStripMenuItem.Click += HandleOptionsMenuItemClick;
                _allowTabbingByGroupToolStripMenuItem.Click += HandleToggleAllowTabbingByGroup;
                _separateImageWindowToolStripMenuItem.Click += HandleSeparateImageWindow;
                _undoToolStripButton.Click += HandleUndoClick;
                _undoToolStripMenuItem.Click += HandleUndoClick;
                _redoToolStripButton.Click += HandleRedoClick;
                _redoToolStripMenuItem.Click += HandleRedoClick;
                AttributeStatusInfo.UndoManager.UndoAvailabilityChanged += HandleUndoAvailabilityChanged;
                AttributeStatusInfo.UndoManager.RedoAvailabilityChanged += HandleRedoAvailabilityChanged;

                // [DataEntry:195] Open the form with the position and size set per the registry 
                // settings. Do this regardless of whether the window will be maximized so that it
                // will restore to the size used the last time the window was in the "normal" state.
                Rectangle defaultBounds = new Rectangle(
                    new Point(_registry.Settings.WindowPositionX,
                              _registry.Settings.WindowPositionY),
                    new Size(_registry.Settings.WindowWidth,
                             _registry.Settings.WindowHeight));

                Rectangle workingArea = Screen.GetWorkingArea(defaultBounds);
                if (workingArea.IntersectsWith(defaultBounds))
                {
                    DesktopBounds = defaultBounds;

                    if (_registry.Settings.WindowMaximized)
                    {
                        // Maximize the window if the registry setting indicates the application should
                        // launch maximized.
                        WindowState = FormWindowState.Maximized;
                    }
                }

                // Load the DEP into the left-hand panel or separate image window and position and
                // sizes it correctly.
                LoadDataEntryControlHostPanel();

                // Handle scroll in order to update the panel position while a scroll is in
                // progress.
                _scrollPanel.Scroll += HandleScrollPanelScroll;

                // Establish connections between the image viewer and all image viewer controls.
                _imageViewer.EstablishConnections(this);

                // Don't allow the image viewer to be show in a separate window or the magnifier to
                // be shown if the form is in invisible mode.
                if (!_invisible)
                {
                    // Not sure how to default the magnifier window to open floating by default except
                    // to manually open it as floating, then close it prior to loading any saved layout
                    // (which will override the default situation, if present).
                    _magnifierDockableWindow.OpenFloating();
                    _thumbnailDockableWindow.OpenFloating();

                    if (_registry.Settings.ShowSeparateImageWindow)
                    {
                        OpenSeparateImageWindow();
                    }

                    _magnifierDockableWindow.Close();
                    _thumbnailDockableWindow.Close();

                    try
                    {
                        // Commenting this LOC removes a docstyle exception, see JIRA ISSUE-13833
                        // TODO: look into this after 10.4 release!
                        //_sandDockManager.LoadLayout();
                    }
                    catch (Exception ex)
                    {
                        // In the case of an error loading the layout, save the layout to replace
                        // the bad configuration with the default configuration.
                        _sandDockManager.SaveLayout();

                        ex.ExtractLog("ELI40391");
                    }

                    // If pagination is available, the thumbnail pane is redundant and does not
                    // currently play well with the pagination panel. Disallow use of the thumbnail
                    // window in this configuration.
                    if (_paginationPanel != null && _thumbnailDockableWindow.IsOpen)
                    {
                        _thumbnailDockableWindow.Close();
                    }
                }

                // Adjust UI elements to reflect the current configuration.
                SetUIConfiguration(_activeDataEntryConfig);

                // Load the DEP associated with the active configuration.
                SetDataEntryControlHost((_activeDataEntryConfig == null)
                    ? null : _activeDataEntryConfig.DataEntryControlHost);

                if (_applicationConfig.Settings.EnableLogging)
                {
                    AttributeStatusInfo.Logger = Logger.CreateLogger(
                        _applicationConfig.Settings.LogToFile,
                        _applicationConfig.Settings.LogFilter,
                        _applicationConfig.Settings.InputEventFilter,
                        this);
                }

                _isLoaded = true;
                OnInitialized();
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI23670", ex);
                ee.AddDebugData("Event Arguments", e, false);
                RaiseVerificationException(ee, false);
            }
        }

        /// <summary>
        /// Gets the tool strip items the close files.
        /// </summary>
        /// <returns>The tool strip items that close files.</returns>
        ToolStripItem[] GetCloseFileToolStripItems()
        {
            return _standAloneMode ?
                new ToolStripItem[] { _closeImageToolStripMenuItem } :
                new ToolStripItem[0];
        }

        /// <summary>
        /// Raises the <see cref="Form.OnResize"/> event.
        /// </summary>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        protected override void OnResize(EventArgs e)
        {
            try
            {
                base.OnResize(e);

                if (_isLoaded && WindowState != FormWindowState.Minimized)
                {
                    if (WindowState == FormWindowState.Maximized)
                    {
                        // If the user maximized the form, set the form to default to maximized,
                        // but don't adjust the default form size to use in normal mode.
                        _registry.Settings.WindowMaximized = true;
                    }
                    else if (WindowState == FormWindowState.Normal)
                    {
                        // If the user restored or moved the form in normal mode, store
                        // the new size as the default size.
                        _registry.Settings.WindowMaximized = false;
                        _registry.Settings.WindowWidth = Size.Width;
                        _registry.Settings.WindowHeight = Size.Height;
                    }

                    // If there is an image open in the image viewer then restore the previous
                    // scroll position - [DNRCAU #262 - JDS]
                    if (_imageViewer.IsImageAvailable)
                    {
                        _imageViewer.RestoreScrollPosition();
                    }
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI25071", ex);
                ee.AddDebugData("Event arguments", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Raises the <see cref="Control.OnMove"/> event.
        /// </summary>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        protected override void OnMove(EventArgs e)
        {
            try
            {
                base.OnMove(e);

                if (_isLoaded && WindowState == FormWindowState.Normal)
                {
                    // If the user moved the form, store the new position.
                    _registry.Settings.WindowPositionX = DesktopLocation.X;
                    _registry.Settings.WindowPositionY = DesktopLocation.Y;
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI25072", ex);
                ee.AddDebugData("Event arguments", e, false);
                ee.Display();
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
                // [DataEntry:316]
                // Don't allow any shortcuts or menu navigation via keys while an image viewer
                // tracking event is in progress.
                if (_imageViewer.Capture)
                {
                    return true;
                }

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
                ExtractException.Display("ELI24060", ex);
                return false;
            }
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
                // Before closing windows make sure the config is still valid.               
				// https://extract.atlassian.net/browse/ISSUE-12830
                UserConfigChecker.EnsureValidUserConfigFile();

                if (!PreventSaveOfDirtyData)
                {
                    // Check for unsaved data and cancel the close if necessary.
                    if (AttemptSave(false) == DialogResult.Cancel)
                    {
                        e.Cancel = true;
                    }
                    else
                    {
                        if (_paginationPanel != null)
                        {
                            _paginationPanel.RemoveSourceFile(_fileName);
                        }

                        // Record statistics to database that needs to happen when a file is closed.
                        if (!_standAloneMode && _dataEntryDatabaseManager != null)
                        {
                            RecordCounts(onLoad: false, attributes: null);
                            EndFileTaskSession();
                        }
                    }
                }

                if (!e.Cancel && _dataEntryControlHost != null)
                {
                    // Clear data to give the host a chance to clear any static COM objects that will
                    // not be accessible from a different thread due to the single apartment threading
                    // model.
                    _dataEntryControlHost.ClearData();
                }

                if (_isLoaded && !_invisible)
                {
                    _sandDockManager.SaveLayout();
                }

                // Don't call base.OnFormClosing until we know if the close is being canceled
                // (if VerificationForm receives a FormClosing event, it expects that the form will
                // indeed close).
                base.OnFormClosing(e);
            }
            catch (Exception ex)
            {
                RaiseVerificationException("ELI24858", ex, false);
            }
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                try
                {
                    if (_brandingResources != null)
                    {
                        _brandingResources.Dispose();
                        _brandingResources = null;
                    }

                    if (_documentTypeConfigurations != null)
                    {
                        CollectionMethods.ClearAndDispose(_documentTypeConfigurations);
                        _documentTypeConfigurations = null;
                    }

                    if (_defaultDataEntryConfig != null)
                    {
                        _defaultDataEntryConfig.Dispose();
                        _defaultDataEntryConfig = null;
                    }

                    if (_inputEventTracker != null)
                    {
                        _inputEventTracker.Dispose();
                        _inputEventTracker = null;
                    }

                    if (AttributeStatusInfo.Logger != null)
                    {
                        AttributeStatusInfo.Logger.Dispose();
                        AttributeStatusInfo.Logger = null;
                    }

                    if (_imageWindowShortcutsMessageFilter != null)
                    {
                        _imageWindowShortcutsMessageFilter.Dispose();
                        _imageWindowShortcutsMessageFilter = null;
                    }

                    if (_imageViewerForm != null)
                    {
                        _imageViewerForm.Dispose();
                        _imageViewerForm = null;
                    }

                    if (_imageViewer != null)
                    {
                        _imageViewer.Dispose();
                        _imageViewer = null;
                    }

                    if (_magnifierDockableWindow != null)
                    {
                        _magnifierDockableWindow.Dispose();
                        _magnifierDockableWindow = null;
                    }

                    if (_thumbnailDockableWindow != null)
                    {
                        _thumbnailDockableWindow.Dispose();
                        _thumbnailDockableWindow = null;
                    }

                    if (_thumbnailViewer != null)
                    {
                        _thumbnailViewer.Dispose();
                        _thumbnailViewer = null;
                    }

                    if (components != null)
                    {
                        components.Dispose();
                        components = null;
                    }

                    if (_dataEntryControlHost != null)
                    {
                        // Will cause the control host to be disposed of.
                        SetDataEntryControlHost(null);
                    }

                    if (_dbConnections != null)
                    {
                        CollectionMethods.ClearAndDispose(_dbConnections);
                        _dbConnections = null;
                    }

                    // Dispose of menu items
                    if (_closeImageToolStripMenuItem != null)
                    {
                        _closeImageToolStripMenuItem.Dispose();
                        _closeImageToolStripMenuItem = null;
                    }
                    if (_saveAndCommitMenuItem != null)
                    {
                        _saveAndCommitMenuItem.Dispose();
                        _saveAndCommitMenuItem = null;
                    }
                    if (_saveMenuItem != null)
                    {
                        _saveMenuItem.Dispose();
                        _saveMenuItem = null;
                    }
                    if (_printMenuItem != null)
                    {
                        _printMenuItem.Dispose();
                        _printMenuItem = null;
                    }
                    if (_skipProcessingMenuItem != null)
                    {
                        _skipProcessingMenuItem.Dispose();
                        _skipProcessingMenuItem = null;
                    }
                    if (_exitToolStripMenuItem != null)
                    {
                        _exitToolStripMenuItem.Dispose();
                        _exitToolStripMenuItem = null;
                    }
                    if (_voaFileLock != null)
                    {
                        _voaFileLock.Dispose();
                        _voaFileLock = null;
                    }
                }
                catch { }
            }

            base.Dispose(disposing);
        }

        #endregion Overrides

        #region Events

        /// <summary>
        /// This event indicates the verification form has been initialized and is ready to load a
        /// document.
        /// </summary>
        public event EventHandler<EventArgs> Initialized;

        /// <summary>
        /// This event indicates that the current document is done processing.
        /// </summary>
        public event EventHandler<FileCompleteEventArgs> FileComplete;

        /// <summary>
        /// Raised when the task requests that a specific file be provided ahead of the files
        /// currently waiting in the task from different threads (prefetched).
        /// </summary>
        public event EventHandler<FileRequestedEventArgs> FileRequested;

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

        /// <summary>
        /// This event indicates the value of <see cref="ShowAllHighlights"/> has changed.
        /// </summary>
        public event EventHandler<EventArgs> ShowAllHighlightsChanged;

        #endregion Events

        #region Event Handlers

        /// <summary>
        /// Handles the case that the user requested that the data be saved and committed.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">The event data associated with the event.</param>
        void HandleSaveAndCommitClick(object sender, EventArgs e)
        {
            try
            {
                SaveAndCommit();
            }
            catch (Exception ex)
            {
                ExtractException ee = new ExtractException("ELI23908",
                    "Failed to output document data!", ex);
                ee.AddDebugData("Event data", e, false);
                RaiseVerificationException(ee, true);
            }
        }

        /// <summary>
        /// Handles the case that the user requested that the data be saved (but not committed).
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">The event data associated with the event.</param>
        void HandleSaveClick(object sender, EventArgs e)
        {
            try
            {
                ExtractException.Assert("ELI29142", "Saving is disabled!",
                    _activeDataEntryConfig == null ||
                    !_activeDataEntryConfig.Config.Settings.PreventSave);

                SaveData(false);
            }
            catch (Exception ex)
            {
                ExtractException ee = new ExtractException("ELI26948",
                    "Failed to output document data!", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the case that the user requested to skip the current document
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">The event data associated with the event.</param>
        void HandleSkipFileClick(object sender, EventArgs e)
        {
            try
            {
                SkipFile();
            }
            catch (Exception ex)
            {
                ExtractException ee = new ExtractException("ELI26943",
                    "Failed to output document data!", ex);
                ee.AddDebugData("Event data", e, false);
                RaiseVerificationException(ee, true);
            }
        }
        /// <summary>
        /// Handles the case that a new image was loaded into the image viewer.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">The event data associated with the event.</param>
        void HandleImageFileChanged(object sender, ImageFileChangedEventArgs e)
        {
            try
            {
                // If a _temporaryDocumentType was added to the _documentTypeMenu to match the
                // original type of the last document loaded, remove it now that the image has
                // changed.
                if (_temporaryDocumentType != null)
                {
                    _documentTypeComboBox.Items.Remove(_temporaryDocumentType);
                    _temporaryDocumentType = null;
                }

                // Ensure the data in the controls is cleared when a document is closed or prior to
                // loading any new data.
                if (_dataEntryControlHost != null)
                {
                    _dataEntryControlHost.ClearData();
                }

                if (!_imageViewer.IsImageAvailable)
                {
                    _voaFileLock.ReleaseLock();

                    // The goto next invalid and unviewed buttons and menu options will be enabled via
                    // the DataEntryControlHost.UnviewedItemsFound and InvalidItemsFound events.
                    _gotoNextInvalidCommand.Enabled = false;
                    _gotoNextUnviewedCommand.Enabled = false;

                    // This highlight commands will be enabled via HandleSwipingStateChanged.
                    _toggleHighlightCommand.Enabled = false;
                    _selectAngularHighlightCommand.Enabled = false;
                    _selectRectangularHighlightCommand.Enabled = false;
                    _selectWordHighlightCommand.Enabled = false;
                }

                base.Text = _brandingResources.ApplicationTitle;
                if (_imageViewerForm != null)
                {
                    _imageViewerForm.Text = _brandingResources.ApplicationTitle + " Image Window";
                }

                if (_imageViewer.IsImageAvailable)
                {
                    // https://extract.atlassian.net/browse/ISSUE-12265
                    // Ensure the dimensions (in pixels) of each page as reported by the OCR engine
                    // match the page dimensions to be used by the image viewer so that redactions
                    // appear where they are supposed to appear.
                    if (_imageViewer.OcrData != null)
                    {
                        _imageViewer.OcrData.SpatialString.ValidatePageDimensions();
                    }

                    _documentLoadCount++;

                    // [DataEntry:693]
                    // On WindowsXP, resources associated with the auto-complete list (
                    // particularly GDI objects) do not seem to be cleaned up and eventually 
                    // "Error creating window handle" exceptions will result. Calling GC.Collect
                    // cleans up these resources. (GCFrequency default == 1)
                    if (_imageViewer.IsImageAvailable && _activeDataEntryConfig != null &&
                        _activeDataEntryConfig.Config.Settings.GCFrequency > 0 &&
                        _documentLoadCount % _activeDataEntryConfig.Config.Settings.GCFrequency == 0)
                    {
                        GC.Collect();
                    }

                    string imageName = Path.GetFileName(_imageViewer.ImageFile);
                    base.Text = imageName + " - " + base.Text;
                    if (_imageViewerForm != null)
                    {
                        _imageViewerForm.Text = imageName + " - " + _imageViewerForm.Text;
                    }

                    IUnknownVector attributes = GetVOAData(_imageViewer.ImageFile);
                    if (!LoadCorrectConfigForData(attributes) && _dataEntryControlHost != null)
                    {
                        // [DataEntry:729]
                        // If the data entry config didn't change, still need to call
                        // GetDatabaseConnection to get the latest version of the database (in case
                        // it has been updated).
                        _dataEntryControlHost.SetDatabaseConnections(GetDatabaseConnections());
                    }

                    // Record counts on load
                    if (!_standAloneMode && _fileProcessingDb != null)
                    {
                        RecordCounts(onLoad: true, attributes: attributes);
                    }

                    // If a DEP is being used, load the data into it
                    if (_dataEntryControlHost != null)
                    {
                        _dataEntryControlHost.LoadData(attributes);

                        if (_documentTypeConfigurations != null)
                        {
                            // Monitor for changes to the document type from the DEP.
                            GetDocumentType(attributes, true);
                        }

                        // Now that the data has been loaded into the DEP, update the document data
                        // in the pagination panel so that it is sharing the same attributes
                        // hierarchy with the DEP (rather than a separately loaded copy).
                        if (_paginationPanel != null && _paginationDocumentDataPanel != null)
                        {
                            PaginationDocumentData documentData = GetAsPaginationDocumentData(attributes);
                            documentData.DataSharedInVerification = true;

                            // The AttributeValueChanged event only needs to be registered in
                            // conjunction with the currently active document in verification in
                            // order to synchronize data between the two.
                            documentData.AttributeValueChanged += HandleDocumentData_AttributeValueChanged;

                            _paginationPanel.UpdateDocumentData(_fileName, documentData);
                        }
                    }
                }
                else if (!string.IsNullOrEmpty(_actionName))
                {
                    base.Text = "Waiting - " + base.Text;

                    if (_dataEntryControlHost != null)
                    {
                        _dataEntryControlHost.LoadData(null);
                    }
                }

                if (!string.IsNullOrEmpty(_actionName))
                {
                    // [DataEntry:740] Show the name of the current action in the title bar.
                    base.Text += " (" + _actionName + ")";
                }

                // If in standalone mode, no need to enable/disable _saveAndCommitFileCommand
                if (!_standAloneMode)
                {
                    // [DataEntry:1108]
                    // Enable/disable the save and commit command without respect to whether
                    // PreventSave is set. If PreventSave is set, the option should still be
                    // available; its behavior will just be different.
                    _saveAndCommitFileCommand.Enabled = _imageViewer.IsImageAvailable &&
                        _activeDataEntryConfig != null;
                }

                _hideToolTipsCommand.Enabled = _imageViewer.IsImageAvailable;
                _toggleShowAllHighlightsCommand.Enabled = _imageViewer.IsImageAvailable;
                
                // Undo/redo command should be unavailable until a change is actually made.
                _undoCommand.Enabled = false;
                _redoCommand.Enabled = false;

                if (!_standAloneMode)
                {
                    // Saving the document should be allowed as long as a document is available,
                    // a data entry configuration is loaded, and PreventSave is not specified.
                    _saveMenuItem.Enabled = _imageViewer.IsImageAvailable &&
                        _activeDataEntryConfig != null &&
                        !_activeDataEntryConfig.Config.Settings.PreventSave;

                    if (_fileProcessingDb != null)
                    {
                        _skipProcessingMenuItem.Enabled = _imageViewer.IsImageAvailable;
                        _tagFileToolStripButton.Enabled = _imageViewer.IsImageAvailable;
                    }
                }

                // [DataEntry:414]
                // A document should only be allowed to be closed in FAM mode
                _closeFileCommand.Enabled = (_standAloneMode && _imageViewer.IsImageAvailable);

                _documentTypeComboBox.Enabled = _imageViewer.IsImageAvailable;

                // Ensure the DEP is scrolled back to the top when a document is loaded, but delay
                // the call to scroll until the next control selection change since the scroll panel
                // may currently be locked.
                _scrollToTopRequired = true;
            }
            catch (Exception ex)
            {
                ExtractException ee = new ExtractException("ELI24228",
                    "Failed to " + 
                    (_imageViewer.IsImageAvailable ? "load" : "clear") + 
                    " document data!", ex);
                ee.AddDebugData("Event data", e, false);
                RaiseVerificationException(ee, true);
            }
        }

        /// <summary>
        /// Handles the case that the current image is closing so that the user can be prompted
        /// about unsaved data.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">The <see cref="ImageFileClosingEventArgs"/> containing the event data.
        /// </param>
        void HandleImageFileClosing(object sender, ImageFileClosingEventArgs e)
        {
            try
            {
                // Make sure ImageViewer activity in pagination UI or switching tabs doesn't get
                // interpreted as a file getting closed in verification.
                if (_paginating ||
                    (_dataEntryControlHost != null && !_dataEntryControlHost.IsDocumentLoaded))
                {
                    return;
                }

                if (!PreventSaveOfDirtyData)
                {
                    // Check for unsaved data and cancel the close if necessary.
                    if (AttemptSave(false) == DialogResult.Cancel)
                    {
                        e.Cancel = true;
                    }
                    else
                    {
                        // Record statistics to database that need to happen when a file is closed.
                        if (!_standAloneMode && _dataEntryDatabaseManager != null)
                        {
                            RecordCounts(onLoad: false, attributes: null);
                            EndFileTaskSession();
                        }

                        // https://extract.atlassian.net/browse/ISSUE-13051
                        // Consider removing code that would cancel processing in response to the
                        // ImageFileClosing event.
                        OnFileComplete(EFileProcessingResult.kProcessingCancelled);
                    }
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI24982", ex);
                ee.AddDebugData("Event data", e, false);
                RaiseVerificationException(ee, false);
            }
        }

        /// <summary>
        /// Handles the <see cref="Extract.Imaging.Forms.ImageViewer.LoadingNewImage"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the
        /// <see cref="Extract.Imaging.Forms.ImageViewer.LoadingNewImage"/> event.</param>
        /// <param name="e">The event data associated with the
        /// <see cref="Extract.Imaging.Forms.ImageViewer.LoadingNewImage"/> event.</param>
        void HandleLoadingNewImage(object sender, LoadingNewImageEventArgs e)
        {
            try
            {
                // Set the application title to reflect the name of the document being opened.
                base.Text = "Loading document - " + _brandingResources.ApplicationTitle;
                if (!string.IsNullOrEmpty(_actionName))
                {
                    // [DataEntry:740] Show the name of the current action in the title bar.
                    base.Text += " (" + _actionName + ")";
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI29180", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the case that the <see cref="DataEntryControlHost"/> is indicating that swiping
        /// support has either been enabled or disabled.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">The event data associated with the event.</param>
        void HandleSwipingStateChanged(object sender, SwipingStateChangedEventArgs e)
        {
            try
            {
                // If an image is not available, the active cursor tool will be disabled
                // automatically. Don't programmatically change the cursor tool-- that way the last
                // cursor tool will be remembered when the next image is loaded.
                if (_imageViewer.IsImageAvailable)
                {
                    // Enable/disable and deselect the highlight cursor tools as needed.
                    if (!e.SwipingEnabled &&
                        (_imageViewer.CursorTool == CursorTool.AngularHighlight ||
                         _imageViewer.CursorTool == CursorTool.RectangularHighlight ||
                         _imageViewer.CursorTool == CursorTool.WordHighlight))
                    {
                        _imageViewer.CursorTool = CursorTool.None;
                    }

                    // Enable or disable highlight commands as appropriate.
                    _toggleHighlightCommand.Enabled = e.SwipingEnabled;
                    _selectAngularHighlightCommand.Enabled = e.SwipingEnabled;
                    _selectRectangularHighlightCommand.Enabled = e.SwipingEnabled;
                    _selectWordHighlightCommand.Enabled = e.SwipingEnabled;
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI24063", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the case that the <see cref="DataEntryControlHost"/> now reports unviewed items
        /// to either be present or not present so that the goto next unviewed button and menu item
        /// can be enabled/disabled.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">An <see cref="UnviewedItemsFoundEventArgs"/> instance containing the
        /// event data.</param>
        void HandleUnviewedItemsFound(object sender, UnviewedItemsFoundEventArgs e)
        {
            try
            {
                _gotoNextUnviewedCommand.Enabled = e.UnviewedItemsFound;
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI24933", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the case that the <see cref="DataEntryControlHost"/> now reports unviewed items
        /// to either be present or not present so that the goto next unviewed button and menu item
        /// can be enabled/disabled.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">An <see cref="InvalidItemsFoundEventArgs"/> instance containing the
        /// event data.</param>
        void HandleInvalidItemsFound(object sender, InvalidItemsFoundEventArgs e)
        {
            try
            {
                _gotoNextInvalidCommand.Enabled = e.InvalidItemsFound;
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI24916", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the case that new items have been selected by updating the enabled status of the
        /// confirm and remove spatial info commands accordingly.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">An <see cref="ItemSelectionChangedEventArgs"/> instance containing the
        /// event data.</param>
        void HandleItemSelectionChanged(object sender, ItemSelectionChangedEventArgs e)
        {
            try
            {
                if (_scrollToTopRequired)
                {
                    // [DataEntry:200]
                    // Execute any request to scroll the panel back to the top now since scroll
                    // panel updates should not be locked during a selection change.
                    _scrollPanel.AutoScrollPosition = new Point(_scrollPanel.AutoScrollPosition.X, 0);
                    _scrollToTopRequired = false;
                }

                _acceptSpatialInfoCommand.Enabled = e.SelectedItemsWithUnacceptedHighlights > 0;

                _removeSpatialInfoCommand.Enabled =
                    (e.SelectedItemsWithAcceptedHighlights > 0 ||
                     e.SelectedItemsWithUnacceptedHighlights > 0 ||
                     e.SelectedItemsWithDirectHints > 0 || e.SelectedItemsWithIndirectHints > 0);
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI25981", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the case that the user selected exit from the file menu
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">The event data associated with the event.</param>
        void HandleExitToolStripMenuItemClick(object sender, EventArgs e)
        {
            try
            {
                Close();
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI24059", ex);
                ee.AddDebugData("Event arguments", e, false);
                RaiseVerificationException(ee, false);
            }
        }

        /// <summary>
        /// Handles the case that the user selected the "Goto next unviewed" button or menu item.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">The event data associated with the event.</param>
        void HandleGoToNextUnviewedClick(object sender, EventArgs e)
        {
            try
            {
                if (_dataEntryControlHost != null)
                {
                    _dataEntryControlHost.GoToNextUnviewed();
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI24643", ex);
                ee.AddDebugData("Event arguments", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the case that the user selected the "Goto next invalid" button or menu item.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">The event data associated with the event.</param>
        void HandleGoToNextInvalidClick(object sender, EventArgs e)
        {
            try
            {
                if (_dataEntryControlHost != null)
                {
                    _dataEntryControlHost.GoToNextInvalid();
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI24644", ex);
                ee.AddDebugData("Event arguments", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the case that the user selected the "Highlight all data in image" button or menu 
        /// item.
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
                ExtractException ee = ExtractException.AsExtractException("ELI25159", ex);
                ee.AddDebugData("Event arguments", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the case that the user selected the "Hide tooltips" menu item.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">The event data associated with the event.</param>
        void HandleHideToolTipsClick(object sender, EventArgs e)
        {
            try
            {
                if (_dataEntryControlHost != null)
                {
                    _dataEntryControlHost.ToggleHideTooltips();
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI25994", ex);
                ee.AddDebugData("Event arguments", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the case that the user selected the "Accept highlight" menu item.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">The event data associated with the event.</param>
        void HandleAcceptImageHighlightClick(object sender, EventArgs e)
        {
            try
            {
                if (_dataEntryControlHost != null)
                {
                    _dataEntryControlHost.AcceptSpatialInfo();
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI25992", ex);
                ee.AddDebugData("Event arguments", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the case that the user selected the "Remove highlight" menu item.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">The event data associated with the event.</param>
        void HandleRemoveImageHighlightClick(object sender, EventArgs e)
        {
            try
            {
                if (_dataEntryControlHost != null)
                {
                    _dataEntryControlHost.RemoveSpatialInfo();
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI25993", ex);
                ee.AddDebugData("Event arguments", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the <see cref="ScrollBar.Scroll"/> event from the _scrollPanel's vertical
        /// scrollbar in order to update the window position while a scroll is in progress.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">The <see cref="ScrollEventArgs"/> that contains the event data.</param>
        void HandleScrollPanelScroll(object sender, ScrollEventArgs e)
        {
            _scrollPanel.AutoScrollPosition = new Point(_scrollPanel.AutoScrollPosition.X, e.NewValue);
        }

        /// <summary>
        /// Handles the <see cref="SplitContainer.SplitterMoved"/> event in order to update the
        /// default splitter position in the registry.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">The event data associated with the event.</param>
        void HandleSplitterMoved(object sender, SplitterEventArgs e)
        {
            try
            {
                _registry.Settings.SplitterPosition = _splitContainer.SplitterDistance;
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI25073", ex);
                ee.AddDebugData("Event arguments", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the <see cref="ToolStripItem.Click"/> event for the help menu item.
        /// </summary>
        /// <param name="sender">The object that sent the <see cref="ToolStripItem.Click"/> event.
        /// </param>
        /// <param name="e">The event data associated with the <see cref="ToolStripItem.Click"/> 
        /// event.</param>
        void HandleHelpMenuItemClick(object sender, EventArgs e)
        {
            try
            {
                Help.ShowHelp(this,
                    DataEntryMethods.ResolvePath(_brandingResources.HelpFilePath));
            }
            catch (Exception ex)
            {
                ExtractException ee =
                    new ExtractException("ELI26958", "The help file failed to load!", ex);
                ee.AddDebugData("Help file", _brandingResources.HelpFilePath, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the <see cref="ToolStripItem.Click"/> event for the about menu item.
        /// </summary>
        /// <param name="sender">The object that sent the <see cref="ToolStripItem.Click"/> event.
        /// </param>
        /// <param name="e">The event data associated with the <see cref="ToolStripItem.Click"/> 
        /// event.</param>
        void HandleAboutMenuItemClick(object sender, EventArgs e)
        {
            try
            {
                // Show the about dialog
                using (AboutForm aboutForm = new AboutForm(_brandingResources))
                {
                    aboutForm.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI26957", ex);
            }
        }

        /// <summary>
        /// Handles the case that the user selected the Tools | Options menu item.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">The event data associated with the event.</param>
        void HandleOptionsMenuItemClick(object sender, EventArgs e)
        {
            try
            {
                // Create the preferences dialog if not already created
                if (_userPreferencesDialog == null)
                {
                    _userPreferencesDialog = new PropertyPageForm("Options",
                        (IPropertyPage)_userPreferences.PropertyPage);

                    _userPreferencesDialog.Icon = Icon;
                }

                // Display the dialog
                DialogResult result = _userPreferencesDialog.ShowDialog();
                if (result == DialogResult.OK)
                {
                    // Apply the new auto-OCR settings to the image viewer.
                    _imageViewer.AutoOcr = _userPreferences.AutoOcr;
                    _imageViewer.OcrTradeoff = _userPreferences.OcrTradeoff;

                    // If the user applied settings, store them to the registry and update the
                    // dataEntryControlHost's settings.
                    _userPreferences.WriteToRegistry();
                }
                else
                {
                    _userPreferences.ReadFromRegistry();
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI27020", ex);
            }
        }

        /// <summary>
        /// Handles the case that the user selected the "Allow tabbing by group" menu item.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">The event data associated with the event.</param>
        void HandleToggleAllowTabbingByGroup(object sender, EventArgs e)
        {
            try
            {
                ToggleAllowTabbingByGroup();
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI28811", ex);
                ee.AddDebugData("Event arguments", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the case that the user selected the "Show image in separate Window" menu item.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">The event data associated with the event.</param>
        void HandleSeparateImageWindow(object sender, EventArgs e)
        {
            try
            {
                ToggleSeparateImageWindow();
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI28852", ex);
                ee.AddDebugData("Event arguments", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles an image viewer form <see cref="Control.Resize"/> event so that the new size
        /// can be applied as the new default size in the registry.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">The event data associated with the event.</param>
        void HandleImageViewerFormResize(object sender, EventArgs e)
        {
            try
            {
                if (_imageViewerForm.WindowState == FormWindowState.Maximized)
                {
                    // If the user maximized the form, set the form to default to maximized,
                    // but don't adjust the default form size to use in normal mode.
                    _registry.Settings.ImageWindowMaximized = true;
                }
                else if (_imageViewerForm.WindowState == FormWindowState.Normal)
                {
                    // If the user restored or moved the form in normal mode, store
                    // the new size as the default size.
                    _registry.Settings.ImageWindowMaximized = false;
                    _registry.Settings.ImageWindowWidth = _imageViewerForm.Size.Width;
                    _registry.Settings.ImageWindowHeight = _imageViewerForm.Size.Height;
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI28848", ex);
                ee.AddDebugData("Event arguments", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles an image viewer form <see cref="Control.Move"/> event so that the new position
        /// can be applied as the new default position in the registry.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">The event data associated with the event.</param>
        void HandleImageViewerFormMove(object sender, EventArgs e)
        {
            try
            {
                if (_imageViewerForm.WindowState == FormWindowState.Normal)
                {
                    // If the user moved the form, store the new position.
                    _registry.Settings.ImageWindowPositionX = _imageViewerForm.DesktopLocation.X;
                    _registry.Settings.ImageWindowPositionY = _imageViewerForm.DesktopLocation.Y;
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI28849", ex);
                ee.AddDebugData("Event arguments", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles an image viewer form <see cref="Form.Activate"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">The event data associated with the event.</param>
        void HandleImageViewerFormActivated(object sender, EventArgs e)
        {
            try
            {
                // Ensure the image viewer gets focus whenever the image viewer form is brought up.
                _imageViewer.Focus();
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI28856", ex);
                ee.AddDebugData("Event arguments", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles an image viewer form <see cref="Form.Closing"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">The event data associated with the event.</param>
        void HandleImageViewerFormFormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                // If the user is closes the image viewer form, go back to single-window mode.
                _registry.Settings.ShowSeparateImageWindow = false;
                _separateImageWindowToolStripMenuItem.CheckState = CheckState.Unchecked;
                CloseSeparateImageWindow();
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI28841", ex);
                ee.AddDebugData("Event arguments", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the <see cref="MessageFilterBase.MessageHandled"/> or 
        /// <see cref="DataEntryControlHost"/> MessageHandled event.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">The event data associated with the event.</param>
        void HandleMessageFilterMessageHandled(object sender, MessageHandledEventArgs e)
        {
            try
            {
                // If we are recording input event statistics, notify the input tracker of any
                // events that have been intercepted.
                if (CurrentlyRecordingStatistics && _inputEventTracker != null)
                {
                    _inputEventTracker.NotifyOfInputEvent();
                }

                if (AttributeStatusInfo.IsLoggingEnabled(LogCategories.InputEvent))
                {
                    AttributeStatusInfo.Logger.NotifyMessageHandled(e);
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI29133", ex);
                ee.AddDebugData("Event arguments", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the <see cref="AttributeStatusInfo.AttributeValueModified"/> event for the
        /// <see cref="IAttribute"/> containing the document type.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">The event data associated with the event.</param>
        void HandleDocumentTypeAttributeValueModified(object sender,
                AttributeValueModifiedEventArgs e)
        {
            try
            {
                // Update the active document type, but don't allow the current configuration to be
                // changed.
                ChangeActiveDocumentType(e.Attribute.Value.String, false);
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI30653", ex);
                ee.AddDebugData("Event data", e, false);
                // Debated on whether this should be displayed instead, since this may be something
                // that happens in the midst of verification not but this could happen during
                // document load and even it if doesn't, it could indicate that the document will
                // not be able to be correctly saved.
                RaiseVerificationException(ee, true);
            }
        }

        /// <summary>
        /// Handles the <see cref="ComboBox.SelectedIndexChanged"/> event for the
        /// _documentTypeComboBox.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">The event data associated with the event.</param>
        void HandleDocumentTypeSelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                // Update the active document type, changing the current configuration if
                // appropriate.
                ChangeActiveDocumentType(_documentTypeComboBox.Text, true);
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI30616", ex);
                ee.AddDebugData("Event arguments", e, false);
                // Debated on whether this should be displayed instead, since this may be something
                // that happens in the midst of verification not but this could happen during
                // document load and even it if doesn't, it could indicate that the document will
                // not be able to be correctly saved.
                RaiseVerificationException(ee, true);
            }
        }

        /// <summary>
        /// Handles the case that the user selected the "Undo" button or menu item.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">The event data associated with the event.</param>
        void HandleUndoClick(object sender, EventArgs e)
        {
            try
            {
                if (_dataEntryControlHost != null)
                {
                    _dataEntryControlHost.Undo();
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI31002", ex);
                ee.AddDebugData("Event arguments", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the case that the user selected the "Redo" button or menu item.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">The event data associated with the event.</param>
        void HandleRedoClick(object sender, EventArgs e)
        {
            try
            {
                if (_dataEntryControlHost != null)
                {
                    _dataEntryControlHost.Redo();
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI34438");
            }
        }

        /// <summary>
        /// Handles the <see cref="UndoManager.UndoAvailabilityChanged"/> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleUndoAvailabilityChanged(object sender, EventArgs e)
        {
            try
            {
                _undoCommand.Enabled = _imageViewer.IsImageAvailable &&
                    AttributeStatusInfo.UndoManager.UndoOperationAvailable;
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI31014", ex);
            }
        }

        /// <summary>
        /// Handles the <see cref="UndoManager.RedoAvailabilityChanged"/> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void HandleRedoAvailabilityChanged(object sender, EventArgs e)
        {
            try
            {
                _redoCommand.Enabled = _imageViewer.IsImageAvailable &&
                    AttributeStatusInfo.UndoManager.RedoOperationAvailable;
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI34437", ex);
            }
        }

        /// <summary>
        /// Handles the TabControl.Selecting event of the <see cref="_tabControl"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="TabControlCancelEventArgs"/> instance containing the
        /// event data.</param>
        void HandleTabControl_Selecting(object sender, System.Windows.Forms.TabControlCancelEventArgs e)
        {
            try
            {
                if (!_paginating)
                {
                    if (e.TabPage != _paginationTab && _paginationPanel.PendingChanges)
                    {
                        UtilityMethods.ShowMessageBox(_paginationPanel.IsPaginationSuggested(_fileName)
                            ? "You must apply desired pagination for this file."
                            : "You must apply or revert pagination changes.",
                            "Uncommitted pagination", false);
                        e.Cancel = true;
                    }
                    // Though not easily reproducible, switching to the pagination tab before a
                    // document is fully loaded can cause exceptions for a null _imageViewer in
                    // FinalizeDocumentLoad. Prevent tab changes until a document fully loaded,
                    // though allow for tab changes for form setup before any files are loaded.                    
                    else if (e.TabPage == _paginationTab && _imageViewer.IsImageAvailable &&
                        _dataEntryControlHost != null && !_dataEntryControlHost.IsDocumentLoaded)
                    {
                        e.Cancel = true;
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI39669");
            }
        }

        /// <summary>
        /// Handles the TabControl.SelectedIndexChanged event of the <see cref="_tabControl"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleTabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (!_isLoaded)
                {
                    return;
                }

                if (_tabControl.SelectedTab == _dataTab && _dataEntryControlHost.ImageViewer == null)
                {
                    // Ensure the _paginationPanel won't try to do anything with the image viewer
                    // while the data tab has focus.
                    _paginationPanel.Park();

                    // The pagination control may have closed the document; re-open it if necessary.
                    if (!string.IsNullOrWhiteSpace(_fileName) &&
                        _imageViewer.ImageFile != _fileName)
                    {
                        _imageViewer.OpenImage(_fileName, false);
                    }

                    _dataEntryControlHost.ImageViewer = _imageViewer;
                    _dataEntryControlHost.DisableKeyboardInput = false;
                    _imageViewer.AllowHighlight = true;
                    _imageViewer.ImageFileChanged += HandleImageFileChanged;
                    _imageViewer.ImageFileClosing += HandleImageFileClosing;
                    _imageViewer.LoadingNewImage += HandleLoadingNewImage;

                    _saveAndCommitFileCommand.Enabled = _imageViewer.IsImageAvailable;
                    _saveMenuItem.Enabled = _imageViewer.IsImageAvailable;
                    _skipProcessingMenuItem.Enabled = _imageViewer.IsImageAvailable;
                    _printMenuItem.Enabled = _imageViewer.IsImageAvailable;
                    _pageNavigationToolStripMenuItem.Enabled = _imageViewer.IsImageAvailable;
                    _pageNavigationImageViewerToolStrip.Enabled = _imageViewer.IsImageAvailable;
                    if (_imageViewer.IsImageAvailable)
                    {
                        // This restores the page number to the page _pageNavigationImageViewerToolStrip.
                        _imageViewer.PageNumber = _imageViewer.PageNumber;
                    }
                    _gotoNextInvalidCommand.Enabled = _imageViewer.IsImageAvailable;
                    _gotoNextUnviewedCommand.Enabled = _imageViewer.IsImageAvailable;
                    _fileCommandsToolStrip.Enabled = _imageViewer.IsImageAvailable;
                    _hideToolTipsCommand.Enabled = _imageViewer.IsImageAvailable;
                    _acceptSpatialInfoCommand.Enabled = _imageViewer.IsImageAvailable;
                    _removeSpatialInfoCommand.Enabled = _imageViewer.IsImageAvailable;
                    _undoCommand.Enabled = _imageViewer.IsImageAvailable;
                    _redoCommand.Enabled = _imageViewer.IsImageAvailable;
                    _selectAngularHighlightCommand.Enabled = _imageViewer.IsImageAvailable;
                    _selectRectangularHighlightCommand.Enabled = _imageViewer.IsImageAvailable;
                    _selectWordHighlightCommand.Enabled = _imageViewer.IsImageAvailable;
                    _toggleHighlightCommand.Enabled = _imageViewer.IsImageAvailable;
                    _toggleShowAllHighlightsCommand.Enabled = _imageViewer.IsImageAvailable;

                    // The select layer object tool becomes available based on the ImageViewer state.
                    // The only way to control the enabled state is by changing the ImageViewer property.
                    _selectLayerObjectMenuItem.ImageViewer = _imageViewer;
                    _selectLayerObjectToolStripButton.ImageViewer = _imageViewer;
                    _printMenuItem.ImageViewer = _imageViewer;

                    // Refresh any attributes that were modified in the _paginationDocumentDataPanel
                    // so that it's value stays in sync.
                    foreach (IAttribute attribute in _paginagionAttributesToRefresh)
                    {
                        AttributeStatusInfo.SetValue(attribute, attribute.Value, true, true);
                        var owningControl = AttributeStatusInfo.GetOwningControl(attribute);
                        if (owningControl != null)
                        {
                            owningControl.RefreshAttributes(true, attribute);
                        }
                    }

                    _paginagionAttributesToRefresh.Clear();

                    _dataEntryControlHost.EnsureFieldSelection();

                    // Table controls don't always seem to be drawn correctly after switching tabs.
                    _dataEntryControlHost.Refresh();
                }
                else if (_tabControl.SelectedTab != _dataTab && _dataEntryControlHost.ImageViewer != null)
                {
                    // These events are handled in the context of data entry verification. If
                    // pagination is active these events should be ignored.
                    _imageViewer.AllowHighlight = false;
                    _imageViewer.ImageFileChanged -= HandleImageFileChanged;
                    _imageViewer.ImageFileClosing -= HandleImageFileClosing;
                    _imageViewer.LoadingNewImage -= HandleLoadingNewImage;
                    // Setting ImageViewer to null will unregister image viewer events we shouldn't
                    // handle while in pagination.
                    _dataEntryControlHost.DisableKeyboardInput = true;
                    _dataEntryControlHost.ImageViewer = null;

                    _saveAndCommitFileCommand.Enabled = false;
                    _saveMenuItem.Enabled = false;
                    
                    // Skip should always be enabled when on the pagination tab if a document is available
                    _skipProcessingMenuItem.Enabled = _paginationPanel.SourceDocuments.Any();
                    
                    _pageNavigationToolStripMenuItem.Enabled = false;
                    _pageNavigationImageViewerToolStrip.Enabled = false;
                    _gotoNextInvalidCommand.Enabled = false;
                    _gotoNextUnviewedCommand.Enabled = false;
                    _fileCommandsToolStrip.Enabled = false;
                    _hideToolTipsCommand.Enabled = false;
                    _acceptSpatialInfoCommand.Enabled = false;
                    _removeSpatialInfoCommand.Enabled = false;
                    _undoCommand.Enabled = false;
                    _redoCommand.Enabled = false;
                    _selectAngularHighlightCommand.Enabled = false;
                    _selectRectangularHighlightCommand.Enabled = false;
                    _selectWordHighlightCommand.Enabled = false;
                    _toggleHighlightCommand.Enabled = false;
                    _toggleShowAllHighlightsCommand.Enabled = false;

                    _imageViewer.CursorTool = CursorTool.None;

                    // The select layer object tool becomes available based on the ImageViewer state.
                    // The only way to control the enabled state is by changing the ImageViewer property.
                    _selectLayerObjectMenuItem.ImageViewer = null;
                    _selectLayerObjectToolStripButton.ImageViewer = null;
                    _printMenuItem.ImageViewer = null;

                    _paginationPanel.Resume();
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI39546");
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
                throw ex.AsExtract("ELI39792");
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

                bool sendForReprocessing =
                    (e.DocumentData != null && !e.DocumentData.SendForReprocessing != null)
                    ? e.DocumentData.SendForReprocessing.Value
                    : (!e.SuggestedPaginationAccepted.HasValue || !e.SuggestedPaginationAccepted.Value);

                EFilePriority priority = GetPriorityForFile(e);

                // Add the file to the DB and check it out for this process before actually writing
                // it to outputPath to prevent a running file supplier from grabbing it and another
                // process from getting it.
                int fileID = _paginationPanel.AddFileWithNameConflictResolve(e, priority);

                // Produce a uss file for the paginated document using the uss data from the
                // source documents
                int pageCounter = 1;
                var pageMap = new Dictionary<Tuple<string, int>, List<int>>();
                foreach (var pageInfo in e.SourcePageInfo)
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

                var newSpatialPageInfos = PaginationPanel.CreateUSSForPaginatedDocument(e.OutputFileName, pageMap);

                // Only grab the file back into the current verification session if suggested
                // pagination boundaries were accepted (meaning the rules should have already found
                // everything we would expect them to find for this document).
                if (sendForReprocessing)
                {
                    // Produce a voa file for the paginated document using the data the rules suggested.
                    var documentData = e.DocumentData as PaginationDocumentData;
                    if (documentData != null)
                    {
                        var copyThis = (ICopyableObject)documentData.Attributes;
                        var attributesCopy = (IUnknownVector)copyThis.Clone();
                        attributesCopy.ReportMemoryUsage();

                        if (e.SourcePageInfo.Any(pageInfo => pageInfo.DocumentName == _fileName))
                        {
                            DataEntryControlHost.PruneNonPersistingAttributes(attributesCopy);
                        }

                        AttributeMethods.TranslateAttributesToNewDocument(
                            attributesCopy, e.OutputFileName, pageMap, newSpatialPageInfos);

                        attributesCopy.SaveTo(e.OutputFileName + ".voa", false,
                            _ATTRIBUTE_STORAGE_MANAGER_GUID);
                        attributesCopy.ReportMemoryUsage();
                    }

                    if (!string.IsNullOrWhiteSpace(_settings.PaginationSettings.PaginationOutputAction))
                    {
                        EActionStatus oldStatus;
                        FileProcessingDB.SetStatusForFile(fileID,
                            _settings.PaginationSettings.PaginationOutputAction,
                            EActionStatus.kActionPending, false, false, out oldStatus);
                    }
                }
                else if (!string.IsNullOrWhiteSpace(_settings.PaginationSettings.PaginationOutputAction))
                {
                    GrabDocumentForVerification(fileID, e, pageMap, newSpatialPageInfos);                    
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39595");
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
                _paginating = true;

                // HandlePaginationPanel_CreatingOutputDocument will have already paused the
                // queue in most cases, but not if the applied pagination matched source doc form.
                FileRequestHandler.PauseProcessingQueue();

                foreach (var item in e.ModifiedDocumentData)
                {
                    string sourceFileName = item.Key;
                    PaginationDocumentData documentData = (PaginationDocumentData)item.Value;

                    string dataFileName = sourceFileName + ".voa";
                    documentData.Attributes.SaveTo(dataFileName, false, _ATTRIBUTE_STORAGE_MANAGER_GUID);
                    documentData.SetOriginalForm();
                }

                // If suggested pagination was disregarded, the source document should be
                // treated as essentially new output that needs to be reprocessed by rules
                // since what the rules originally found doesn't apply to the source
                // document as a whole.
                if (e.DisregardedPaginationSources.SingleOrDefault() == _fileName)
                {
                    if (!string.IsNullOrWhiteSpace(_settings.PaginationSettings.PaginationOutputAction))
                    {
                        EActionStatus oldStatus;
                        FileProcessingDB.SetStatusForFile(_fileId,
                            _settings.PaginationSettings.PaginationOutputAction,
                            EActionStatus.kActionPending, false, false, out oldStatus);
                    }
                }
                // PaginationSourceAction allows a paginated source to be moved into a
                // cleanup action even when it is completed for this action without actually
                // having completed all FAM tasks.
                else if (!string.IsNullOrWhiteSpace(_settings.PaginationSettings.PaginationSourceAction))
                {
                    EActionStatus oldStatus;
                    FileProcessingDB.SetStatusForFile(_fileId,
                        _settings.PaginationSettings.PaginationSourceAction,
                        EActionStatus.kActionPending, false, true, out oldStatus);
                }

                // In order for files to be completed (and any prompting or other UI tasks
                // accomplished by the DEP), the data tab must be given back focus.
                _tabControl.SelectedTab = _dataTab;

                _paginationPanel.RemoveSourceFile(_fileName);

                var success = FileRequestHandler.SetFallbackStatus(
                    _fileId, EActionStatus.kActionCompleted);
                ExtractException.Assert("ELI39623", "Failed to set fallback status", success,
                    "FileID", _fileId);

                ReleaseFile(_fileId);
                DelayFile();

                _paginating = false;
            }
            catch (Exception ex)
            {
                _paginating = false;

                throw ex.AsExtract("ELI39547");
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
                _paginating = false;
                _paginationOutputOrder.Clear();
                FileRequestHandler.ResumeProcessingQueue();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI40232");
            }
        }

        /// <summary>
        /// Handles the <see cref="PaginationDocumentData.AttributeValueChanged"/> event.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="AttributeValueChangedEventArgs"/> instance containing the
        /// event data.</param>
        void HandleDocumentData_AttributeValueChanged(object sender, AttributeValueChangedEventArgs e)
        {
            try
            {
                _paginagionAttributesToRefresh.Add(e.ModifiedAttribute);

                // This event is registered only for the active document's data an not for any other
                // document displayed in the pagination panel this will not block the modified
                // status for any document except the active one.
                e.MarkAsModified = !_paginationPanel.IsInOriginalForm(_fileName);
                _paginationDocumentDataPanel.ShowMessage("You are modifying data for the document being verified.");
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39752");
            }
        }

        #endregion Event Handlers

        #region IDataEntryApplication Members

        /// <summary>
        /// Gets the title of the application.
        /// </summary>
        public string ApplicationTitle
        {
            get
            {
                return (_brandingResources == null) ? "" : _brandingResources.ApplicationTitle;
            }
        }   

        /// <summary>
        /// Gets how the image viewer zoom/view is adjusted when new fields are selected.
        /// </summary>
        public AutoZoomMode AutoZoomMode
        {
            get
            {
                return _userPreferences.AutoZoomMode;
            }
        }

        /// <summary>
        /// Gets the page space (context) that should be shown around an object selected when
        /// AutoZoom mode is active. 0 indicates no context space should be shown around the current
        /// selection where 1 indicates the maximum context space should be shown.
        /// </summary>
        public double AutoZoomContext
        {
            get 
            {
                return _userPreferences.AutoZoomContext; 
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
        /// Gets whether tabbing should allow groups (rows) of attributes to be selected at a time
        /// for controls in which group tabbing is enabled.
        /// </summary>
        public bool AllowTabbingByGroup
        {
            get
            {
                return _allowTabbingByGroup;
            }
        }

        /// <summary>
        /// Gets the <see cref="FileProcessingDB"/> this instance is currently being run against.
        /// </summary>
        public FileProcessingDB FileProcessingDB
        {
            get
            {
                return _fileProcessingDb;
            }
        }

        /// <summary>
        /// Gets the name of the action in <see cref="FileProcessingDB"/> this instance is currently
        /// being run against.
        /// </summary>
        public string DatabaseActionName
        {
            get
            {
                return _actionName;
            }
        }

        /// <summary>
        /// Gets or sets the comment for the current file that is stored in the file processing
        /// database.
        /// </summary>
        public string DatabaseComment
        {
            get
            {
                return _fileProcessingDBComment;
            }

            set
            {
                _fileProcessingDBComment = value;
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

        /// <summary>
        /// Gets a value indicating whether this <see cref="IDataEntryApplication"/> is dirty.
        /// </summary>
        /// <value>
        /// 	<see langword="true"/> if dirty; otherwise, <see langword="false"/>.
        /// </value>
        public bool Dirty
        {
            get
            {
                return _dataEntryControlHost != null && _dataEntryControlHost.Dirty;
            }
        }

        /// <summary>
        /// Saves the data currently displayed to disk.
        /// </summary>
        /// <param name="validateData"><see langword="true"/> to ensure the data is conforms to the
        /// DataEntryControlHost InvalidDataSaveMode before saving, <see langword="false"/> to save
        /// data without validating.</param>
        /// <returns><see langword="true"/> if the data was saved, <see langword="false"/> if it was
        /// not.</returns>
        public bool SaveData(bool validateData)
        {
            if (InvokeRequired)
            {
                bool saved = false;

                _invoker.Invoke((MethodInvoker)(() =>
                {
                    try
                    {
                        saved = SaveData(validateData);
                    }
                    catch (Exception ex)
                    {
                        throw ex.AsExtract("ELI37506");
                    }
                }));

                return saved;
            }

            try
            {
                if (_activeDataEntryConfig == null ||
                        _dataEntryControlHost == null ||
                        _activeDataEntryConfig.Config.Settings.PreventSave)
                {
                    return false;
                }
                else
                {
                    string dataFilename = _imageViewer.ImageFile + ".voa";
                    _voaFileLock.GetLock(dataFilename,
                        _standAloneMode ? "" : "Data entry verification");

                    bool saved = _dataEntryControlHost.SaveData(validateData);

                    if (saved && !_standAloneMode && _fileProcessingDb != null)
                    {
                        _fileProcessingDb.SetFileActionComment(_fileId, _actionId,
                                _fileProcessingDBComment);
                    }

                    return saved;
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI37507");
            }
        }

        /// <summary>
        /// Delays processing of the current file allowing the next file in the queue to be brought
        /// up in its place, though if there are no more files in the queue this will cause the same
        /// file to be re-displayed.
        /// <para><b>Note</b></para>
        /// If there are changes in the currently loaded document, they will be disregarded. To
        /// check for changes and save, use the <see cref="Dirty"/> and <see cref="SaveData"/>
        /// members first.
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
                        ex.ExtractDisplay("ELI37508");
                    }
                }));

                return;
            }

            try
            {
                ExtractException.Assert("ELI37451", "Invalid operation.",
                    !_standAloneMode && _fileProcessingDb != null);

                // If is no image loaded, there is no file to delay. (While paginating the normal
                // document load may have been short-circuited; assume there is a document to
                // delay).
                if (_paginating || _imageViewer.IsImageAvailable)
                {
                    AbortProcessing(EFileProcessingResult.kProcessingDelayed, false);
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI37452");
            }
        }

        /// <summary>
        /// Skips processing for the current file. This is the same as pressing the skip button in
        /// the UI.
        /// <para><b>Note</b></para>
        /// If there are changes in the currently loaded document, they will be disregarded. To
        /// check for changes and save, use the <see cref="Dirty"/> and <see cref="SaveData"/>
        /// members first.
        /// </summary>
        public void SkipFile()
        {
            if (InvokeRequired)
            {
                _invoker.Invoke((MethodInvoker)(() =>
                {
                    try
                    {
                        SkipFile();
                    }
                    catch (Exception ex)
                    {
                        ex.ExtractDisplay("ELI37509");
                    }
                }));

                return;
            }

            try
            {
                ExtractException.Assert("ELI37453", "Invalid operation.",
                    !_standAloneMode && _fileProcessingDb != null);

                if (_fileId != -1)
                {
                    AbortProcessing(EFileProcessingResult.kProcessingSkipped, true);
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI37454");
            }
        }

        /// <summary>
        /// Requests the specified <see paramref="fileID"/> to be the next file displayed. The file
        /// should be allowed to jump ahead of any other files currently "processing" in the
        /// verification task on other threads (prefetch).
        /// </summary>
        /// <param name="fileID">The file ID.</param>
        /// <returns><see langword="true"/> if the file is currently processing in the verification
        /// task and confirmed to be available, <see langword="false"/> if the task is not currently
        /// holding the file; the requested file will be expected to be the next file in the queue.
        /// </returns>
        public bool RequestFile(int fileID)
        {
            try
            {
                ExtractException.Assert("ELI37493", "Invalid operation.", FileProcessingDB != null);

                // Inform the verification form that we need this file.
                var eventArgs = new FileRequestedEventArgs(fileID);
                OnFileRequested(eventArgs);
                bool requestSucceeded = eventArgs.FileIsAvailable;

                // If the specified file is not actively "processing" in the task (prefetch), move the
                // file to the front of the FPRecordManager's queue to ensure it is the next file in.
                if (!requestSucceeded)
                {
                    requestSucceeded = FileRequestHandler.MoveToFrontOfProcessingQueue(fileID);
                }

                return requestSucceeded;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI37494");
            }
        }

        /// <summary>
        /// Releases the specified file from the current process's internal queue of files checked
        /// out for processing. The file will be treated as if processing has been canceled/stopped
        /// and returned to the current fallback status (status before lock by default).
        /// <para><b>Note</b></para>
        /// The requested file will not be shown until the currently displayed file is closed. If
        /// the requested file needs to replace the currently displayed file immediately,
        /// <see cref="DelayFile()"/> should be called after RequestFile.
        /// </summary>
        /// <param name="fileID">The ID of the file to release.</param>
        /// <returns><see langword="true"/> if the file is currently processing in the verification
        /// task and confirmed to be available,<see langword="false"/> if the task is not currently
        /// holding the file; the requested file will be expected to the next file in the queue or
        /// an exception will result.</returns>
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
                throw ex.AsExtract("ELI37500");
            }
        }

        #endregion IDataEntryApplication Members

        #region Private Members

        /// <summary>
        /// Gets whether statistics are currently being tracked (data entry file duration, counters,
        /// or input events).
        /// </summary>
        bool CurrentlyRecordingStatistics
        {
            get
            {
                return _fileProcessingStopwatch != null;
            }
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
        /// <param name="fileProcessingResult">Specifies under what circumstances
        /// verification of the file completed.</param>
        void OnFileComplete(EFileProcessingResult fileProcessingResult)
        {
            var eventHandler = FileComplete;
            if (eventHandler != null)
            {
                eventHandler(this, new FileCompleteEventArgs(fileProcessingResult));
            }
        }

        /// <summary>
        /// Raises the <see cref="FileRequested"/> event.
        /// </summary>
        /// <param name="eventArgs">The <see cref="FileRequestedEventArgs"/> instance containing
        /// the event data.</param>
        void OnFileRequested(FileRequestedEventArgs eventArgs)
        {
            var eventHandler = FileRequested;
            if (eventHandler != null)
            {
                eventHandler(this, eventArgs);
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
        /// <param name="ee">The <see cref="VerificationExceptionGeneratedEventArgs"/> for the event.
        /// </param>
        void OnExceptionGenerated(VerificationExceptionGeneratedEventArgs ee)
        {
            var eventHandler = ExceptionGenerated;
            if (eventHandler != null)
            {
                eventHandler(this, ee);
            }
        }

        /// <summary>
        /// Raises the <see cref="ShowAllHighlightsChanged"/> event.
        /// </summary>
        protected void OnShowAllHighlightsChanged()
        {
            var eventHandler = ShowAllHighlightsChanged;
            if (eventHandler != null)
            {
                eventHandler(this, new EventArgs());
            }
        }

        /// <summary>
        /// Saves and commits current file in FAM (!_standAloneMode).
        /// </summary>
        void SaveAndCommit()
        {
            try
            {
                // Treat SaveAndCommit like "Save" for stand-alone mode
                if (_standAloneMode)
                {
                    SaveData(false);
                }
                // AttemptSave will return Cancel if there was invalid data in the DEP.
                else if (AttemptSave(true) != DialogResult.Cancel)
                {
                    ExtractException.Assert("ELI30677",
                        "No controls are loaded from which to save data.",
                        _dataEntryControlHost != null);

                    // If running in FAM mode, close the document until the next one is loaded so it
                    // is clear that the last document has been committed.

                    // Since data has been saved, prevent any other attempts that might be triggered
                    // by events raised during this process.
                    PreventSaveOfDirtyData = true;

                    if (_paginationPanel != null)
                    {
                        _paginationPanel.RemoveSourceFile(_fileName);
                    }

                    _imageViewer.CloseImage();

                    if (_fileProcessingDb != null)
                    {
                        // Record statistics to database that need to happen when a file is closed.
                        RecordCounts(onLoad: false,
                            attributes: _dataEntryControlHost.MostRecentlySavedAttributes);
                        EndFileTaskSession();

                        _fileId = -1;
                        _fileName = null;
                    }

                    OnFileComplete(EFileProcessingResult.kProcessingSuccessful);
                }
            }
            catch (Exception ex)
            {
                RaiseVerificationException("ELI26945", ex, true);
            }
        }

        /// <summary>
        /// Checks for unsaved data and prompts the user to save as necessary.
        /// </summary>
        /// <param name="commitData"><see langword="true"/> if data is being committed and therefore
        /// it should be validated in the DEP or <see langword="false"/> if the purpose is to give
        /// the user a chance to save the data without committing in which case the user will be
        /// prompted whether to save or not.</param>
        /// <returns><see cref="DialogResult.Yes"/> if the document was successfully saved, 
        /// <see cref="DialogResult.No"/> if the user elected not to save or
        /// <see cref="DialogResult.Cancel"/> if the user elected to cancel the operation which
        /// triggered the save attempt or the data in the DEP failed validation for a commit.
        /// </returns>
        DialogResult AttemptSave(bool commitData)
        {
            DialogResult response = DialogResult.Yes;

            // If preventing save, don't save, but also return "Yes" so the application behaves as
            // if it did save correctly.
            if (_activeDataEntryConfig == null || 
                _dataEntryControlHost == null ||
                _activeDataEntryConfig.Config.Settings.PreventSave)
            {
                return response;
            }

            if (_paginationTab != null)
            {
                bool paginationModified = false;
                bool dataModified = false;

                _paginationPanel.CheckForChanges(out paginationModified, out dataModified);

                if (paginationModified || dataModified)
                {
                    if (DialogResult.No == MessageBox.Show(this,
                    "You have uncommitted changes to " +
                        ((paginationModified && dataModified)
                            ? "pagination and data"
                            : paginationModified ? "pagination" : "pagination data") +
                    " that will be lost.\r\n\r\n" +
                    "Discard changes?",
                    "Uncommitted changes", MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2, 0))
                    {
                        return DialogResult.Cancel;
                    }
                }
            }

            // When saving a document or processing a document change, we need to ensure that the
            // data entry verification tab is again made active so that all events that need to be
            // processed for a document change in verification are registered.
            if (_dataTab != null)
            {
                // https://extract.atlassian.net/browse/ISSUE-13838
                // Prevent dialog insisting on committing pagination changes if data is not being
                // committed.
                if (!commitData)
                {
                    _paginationPanel.Park();
                    _paginationPanel.PendingChanges = false;
                }
                _tabControl.SelectedTab = _dataTab;
            }

            if (_imageViewer.IsImageAvailable && (commitData || _dataEntryControlHost.Dirty))
            {
                if (commitData)
                {
                    // [DataEntry:805]
                    // commitData flag determines if data will be validated on save.
                    // Turn off commitData if an applied tag matches the SkipValidationIfDocTaggedAs
                    // setting (and save without prompting)
                    if (!_standAloneMode && _fileProcessingDb != null && !string.IsNullOrEmpty(
                            _activeDataEntryConfig.Config.Settings.SkipValidationIfDocTaggedAs))
                    {
                        // [DataEntry:1294]
                        // Support multiple tags to allow validation to be skipped delimited by
                        // a comma or semi-colon.
                        IEnumerable<string> tagsToBeSkipped =
                            _activeDataEntryConfig.Config.Settings.SkipValidationIfDocTaggedAs
                                .Split(new[] { ',', ';' })
                                .Select(tagName => tagName.Trim())
                                .Where(tagName => !string.IsNullOrWhiteSpace(tagName));

                        VariantVector appliedTags = _fileProcessingDb.GetTagsOnFile(_fileId);
                        int tagCount = appliedTags.Size;
                        for (int i = 0; i < tagCount; i++)
                        {
                            if (tagsToBeSkipped.Any(tagName => tagName.Equals(
                                (string)appliedTags[i], StringComparison.OrdinalIgnoreCase)))
                            {
                                commitData = false;
                                break;
                            }
                        }
                    }
                }
                // Prompt if the data is not being committed.
                else
                {
                    var buttons = PreventCloseCancel
                        ? MessageBoxButtons.YesNo
                        : MessageBoxButtons.YesNoCancel;

                    response = MessageBox.Show(this,
                        "Data has not been saved, would you like to save now?",
                        "Data Not Saved", buttons, MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button1, 0);

                    // If the user chose not to save, prevent any subsequent attempts to save this
                    // document.
                    if (response == DialogResult.No)
                    {
                        _dataEntryControlHost.Dirty = false;
                    }
                }

                // If committing data or the user elected to save, attempt the save.
                if (response == DialogResult.Yes && !SaveData(commitData))
                {
                    // Return cancel if the data in the DEP failed validation.
                    response = DialogResult.Cancel;
                }
            }

            return response;
        }

        /// <summary>
        /// Aborts processing of the current file and returns the specified
        /// <see paramref="processingResult"/>. Will prompt for modified data as applicable.
        /// </summary>
        /// <param name="processingResult">The <see cref="EFileProcessingResult"/> to return for the
        /// current file.</param>
        /// <param name="promptToSave"><see langword="true"/> if a prompt should be displayed about
        /// whether to save any unsaved changes; otherwise, <see langword="false"/>.</param>
        void AbortProcessing(EFileProcessingResult processingResult, bool promptToSave)
        {
            ExtractException.Assert("ELI37455", "Invalid processing result",
                processingResult != EFileProcessingResult.kProcessingSuccessful);

            if (!promptToSave || AttemptSave(false) != DialogResult.Cancel)
            {
                // Since data has been saved, prevent any other attempts that might be triggered
                // by events raised during this process.
                PreventSaveOfDirtyData = true;

                // When processing a document change, we need to ensure that the data entry
                // verification tab is again made active so that all events that need to be
                // processed for a document change in verification are registered.
                if (_paginationPanel != null)
                {
                    _paginationPanel.Park();
                    _paginationPanel.PendingChanges = false;
                    _tabControl.SelectedTab = _dataTab;
                    _paginationPanel.RemoveSourceFile(_fileName);
                }

                _imageViewer.CloseImage();

                // Record statistics to database that need to happen when a file is closed.
                RecordCounts(onLoad: false, attributes: null);
                EndFileTaskSession();

                _fileId = -1;
                _fileName = null;

                OnFileComplete(processingResult);
            }
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
            if (_standAloneMode)
            {
                // In stand-alone mode, there is no FAM process or VerificationForm to deal with the
                // exception.
                ee.Display();
            }
            else
            {
                // https://extract.atlassian.net/browse/ISSUE-13048
                // In case of error, we don't want to let HandleImageFileClosing to cancel the image
                // if the user doesn't choose to stop. Set PreventSaveOfDirtyData now and close the
                // image so the image isn't later closed when PreventSaveOfDirtyData has been reset.
                PreventSaveOfDirtyData = true;
                try
                {
                    if (_imageViewer.IsImageAvailable)
                    {
                        if (_paginationPanel != null)
                        {
                            _paginationPanel.RemoveSourceFile(_fileName);
                        }

                        _imageViewer.CloseImage();
                    }
                }
                catch (Exception ex)
                {
                    var ee2 = new ExtractException("ELI38287", 
                        "Image did not properly close after error.", ex);
                    ee2.Log();
                }

                var verificationException =
                    new VerificationExceptionGeneratedEventArgs(ee, canProcessingContinue);
                OnExceptionGenerated(verificationException);
            }
        }

        /// <summary>
        /// Loads all defined document types and their associated configurations from the specified
        /// master config file.
        /// </summary>
        /// <param name="masterConfigFileName">The config file which will specify document type
        /// configurations (if available).</param>
        void LoadDataEntryConfigurations(string masterConfigFileName)
        {
            // Retrieve the documentTypeConfigurations XML section if it exists
            IXPathNavigable documentTypeConfiguration =
                _applicationConfig.GetSectionXml("documentTypeConfigurations");
           
            XPathNavigator configurationNode = null;
            if (documentTypeConfiguration != null)
            {
                configurationNode = documentTypeConfiguration.CreateNavigator();
            }

            // If unable to find the documentTypeConfigurations or find a defined configuration,
            // use the master config file as the one and only configuration.
            if (configurationNode == null || !configurationNode.MoveToFirstChild())
            {
                _defaultDataEntryConfig = LoadDataEntryConfiguration(masterConfigFileName, null);

                // Hide the document type combo.
                if (_dataTab != null)
                {
                    _dataTab.Controls.Remove(_documentTypePanel);
                }
                else
                {
                    _splitContainer.Panel1.Controls.Remove(_documentTypePanel);
                }
                _scrollPanel.Dock = DockStyle.Fill;
                return;
            }

            // Document type configurations have been defined.
            _documentTypeConfigurations = new Dictionary<string, DataEntryConfiguration>();

            // Load each configuration.
            do
            {
                if (!configurationNode.Name.Equals("configuration", StringComparison.OrdinalIgnoreCase))
                {
                    ExtractException ee = new ExtractException("ELI30617",
                        "Config file error: Unknown DocumentTypeConfiguration element.");
                    ee.AddDebugData("Name", configurationNode.Name, false);
                    throw ee;
                }

                XPathNavigator attribute = configurationNode.Clone();
                if (!attribute.MoveToFirstAttribute())
                {
                    throw new ExtractException("ELI30618",
                        "Config file error: Missing required DocumentTypeConfiguration elements.");
                }

                // Load the configurations element's attributes
                string configFileName = null;
                bool defaultConfiguration = false;
                do
                {
                    if (attribute.Name.Equals("configFile", StringComparison.OrdinalIgnoreCase))
                    {
                        configFileName = attribute.Value;
                    }
                    else if (attribute.Name.Equals("default", StringComparison.OrdinalIgnoreCase))
                    {
                        defaultConfiguration = attribute.ValueAsBoolean;
                        if (defaultConfiguration && _defaultDataEntryConfig != null)
                        {
                            throw new ExtractException("ELI30664", 
                                "Only one document type configuration may be set as the default.");
                        }
                    }
                    else
                    {
                        ExtractException ee = new ExtractException("ELI30619",
                            "Config file error: Unknown attribute in Configuration node.");
                        ee.AddDebugData("Name", attribute.Name, false);
                        throw ee;
                    }
                }
                while (attribute.MoveToNextAttribute());

                ExtractException.Assert("ELI30620",
                    "Config file error: Missing configFile attribute in Configuration node.",
                    !string.IsNullOrEmpty(configFileName));

                configFileName = DataEntryMethods.ResolvePath(configFileName);

                DataEntryConfiguration config =
                    LoadDataEntryConfiguration(configFileName, masterConfigFileName);
                if (defaultConfiguration)
                {
                    _defaultDataEntryConfig = config;
                }

                XPathNavigator documentTypeNode = configurationNode.Clone();
                if (!documentTypeNode.MoveToFirstChild())
                {
                    throw new ExtractException("ELI30621",
                        "Config file error: At least one DocumentType element is required for each Configuration.");
                }

                // Load all document types that will use this configuration.
                do
                {
                    if (!documentTypeNode.Name.Equals("DocumentType",
                            StringComparison.OrdinalIgnoreCase))
                    {
                        ExtractException ee = new ExtractException("ELI30622",
                            "Unknown DocumentTypeConfiguration element.");
                        ee.AddDebugData("Name", documentTypeNode.Name, false);
                        throw ee;
                    }

                    string documentType = documentTypeNode.Value;
                    string documentTypeUpper = documentType.ToUpper(CultureInfo.CurrentCulture);
                    if (_documentTypeConfigurations.ContainsKey(documentTypeUpper))
                    {
                        ExtractException ee = new ExtractException("ELI30623",
                            "Config file error: Duplicate documentType element.");
                        ee.AddDebugData("DocumentType", documentType, false);
                        throw ee;
                    }

                    _documentTypeComboBox.Items.Add(documentType);
                    _documentTypeConfigurations[documentTypeUpper] = config;
                }
                while (documentTypeNode.MoveToNext());
            }
            while (configurationNode.MoveToNext());

            // Register to be notified when the user selects a new document type.
            _documentTypeComboBox.SelectedIndexChanged += HandleDocumentTypeSelectedIndexChanged;
        }

        /// <summary>
        /// Loads the data entry configuration defined by the specified config file.
        /// </summary>
        /// <param name="configFileName">The configuration file that defines the configuration to
        /// be loaded.</param>
        /// <param name="masterConfigFileName">If not <see langword="null"/>, the configuration that
        /// may provide defaults for DataEntry and objectSettings config file values.</param>
        /// <returns>The loaded <see cref="DataEntryConfiguration"/>.</returns>
        DataEntryConfiguration LoadDataEntryConfiguration(string configFileName,
            string masterConfigFileName)
        {
            try
            {
                // Load the configuration settings from file.
                ConfigSettings<Extract.DataEntry.Properties.Settings> config =
                    new ConfigSettings<Extract.DataEntry.Properties.Settings>(
                        configFileName, masterConfigFileName, false, false, _tagUtility);

                // Retrieve the name of the DEP assembly
                string dataEntryPanelFileName = DataEntryMethods.ResolvePath(
                    config.Settings.DataEntryPanelFileName);

                // Create the data entry control host from the specified assembly
                DataEntryControlHost dataEntryControlHost =
                    CreateDataEntryControlHost(dataEntryPanelFileName);

                DataEntryConfiguration configuration =
                    new DataEntryConfiguration(config, dataEntryControlHost);

                // Tie the newly created DEP to this application and its ImageViewer.
                dataEntryControlHost.DataEntryApplication = (IDataEntryApplication)this;
                dataEntryControlHost.Config = config;
                dataEntryControlHost.ImageViewer = _imageViewer;

                QueryNode.QueryCacheLimit = _applicationConfig.Settings.QueryCacheLimit;

                // If HighlightConfidenceBoundary settings has been specified in the config file and
                // the controlHost has exactly two confidence tiers, use the provided value as the
                // minimum OCR confidence value in order to highlight text as confidently OCR'd
                if (!string.IsNullOrEmpty(config.Settings.HighlightConfidenceBoundary)
                    && dataEntryControlHost.HighlightColors.Length == 2)
                {
                    int confidenceBoundary = Convert.ToInt32(
                        config.Settings.HighlightConfidenceBoundary,
                        CultureInfo.CurrentCulture);

                    ExtractException.Assert("ELI25684", "HighlightConfidenceBoundary settings must " +
                        "be a value between 1 and 100",
                        confidenceBoundary >= 1 && confidenceBoundary <= 100);

                    HighlightColor[] highlightColors = dataEntryControlHost.HighlightColors;
                    highlightColors[0].MaxOcrConfidence = confidenceBoundary - 1;
                    dataEntryControlHost.HighlightColors = highlightColors;
                }

                dataEntryControlHost.DisabledControls = config.Settings.DisabledControls;
                dataEntryControlHost.DisabledValidationControls =
                    config.Settings.DisabledValidationControls;

                // Apply settings from the config file that pertain to the DEP.
                if (!string.IsNullOrEmpty(masterConfigFileName))
                {
                    _applicationConfig.ApplyObjectSettings(dataEntryControlHost);
                }
                config.ApplyObjectSettings(dataEntryControlHost);

                return configuration;
            }
            catch (Exception ex)
            {
                ExtractException ee = new ExtractException("ELI30539",
                    "Failed to load data entry configuration", ex);
                ee.AddDebugData("Config file", configFileName, false);
                throw ee;
            }
        }

        /// <summary>
        /// Applies the specified document type and loads the DEP associated with the document
        /// types configuration if necessary.
        /// </summary>
        /// <param name="documentType">The new document type.</param>
        /// <param name="allowConfigurationChange"><see langword="true"/> if the configuration
        /// should be changed if the new document type calls for it, <see langword="false"/> if
        /// the current configuration should not be changed.</param>
        /// <param name="changedDocumentType"><see langword="true"/> if the active document type
        /// was changed, <see langword="false"/> otherwise.</param>
        /// <param name="changedDataEntryConfig"><see langword="true"/> if the active configuration
        /// was changed, <see langword="false"/> otherwise</param>
        void SetActiveDocumentType(string documentType, bool allowConfigurationChange,
            out bool changedDocumentType, out bool changedDataEntryConfig)
        {
            try
            {
                changedDocumentType = false;
                changedDataEntryConfig = false;
                DataEntryConfiguration newDataEntryConfig = _defaultDataEntryConfig;

                if (!documentType.Equals(_activeDocumentType, StringComparison.OrdinalIgnoreCase))
                {
                    changedDocumentType = true;
                    bool blockedConfigurationChange = false;

                    // Search for the configuration to use for the new document type.
                    if (_documentTypeConfigurations != null)
                    {
                        string documentTypeUpper = documentType.ToUpper(CultureInfo.CurrentCulture);
                        if (!_documentTypeConfigurations.TryGetValue(
                                documentTypeUpper, out newDataEntryConfig))
                        {
                            newDataEntryConfig = _defaultDataEntryConfig;
                        }
                    }

                    // If a configuration was found and it differs from the active one, load it.
                    if (newDataEntryConfig != _activeDataEntryConfig)
                    {
                        if (!allowConfigurationChange)
                        {
                            // The document type calls for the configuration to be changed, but
                            // configuration changes are disallowed. This change is to be blocked.
                            blockedConfigurationChange = true;
                        }
                        else if (AttemptSave(false) == DialogResult.Cancel)
                        {
                            // If the user cancelled the change, restore the _activeDocumentType
                            // selection in the document type combo box.
                            _documentTypeComboBox.Text = _activeDocumentType;
                            changedDocumentType = false;
                        }
                        else
                        {
                            // Apply the new configuration and load its DEP.
                            changedDataEntryConfig = true;
                            _activeDataEntryConfig = newDataEntryConfig;
                            SetDataEntryControlHost((_activeDataEntryConfig == null) 
                                ? null :_activeDataEntryConfig.DataEntryControlHost);
                        }
                    }

                    if (changedDocumentType)
                    {
                        changedDocumentType = !blockedConfigurationChange;

                        if (blockedConfigurationChange ||
                            _documentTypeComboBox.FindStringExact(documentType) == -1)
                        {
                            // The new documentType is not valid.
                            _documentTypeComboBox.SelectedIndex = -1;
                        }
                        else
                        {
                            // Assign the new document type.
                            _activeDocumentType = documentType;
                            _documentTypeComboBox.Text = documentType;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI30655", ex);
            }
        }

        /// <summary>
        /// Instantiates the one and only <see cref="DataEntryControlHost"/> implemented by the
        /// specified assembly.
        /// </summary>
        /// <param name="assemblyFileName">The filename of the assembly to use.</param>
        /// <returns>A <see cref="DataEntryControlHost"/> instantiated from the specified assembly.
        /// </returns>
        static DataEntryControlHost CreateDataEntryControlHost(string assemblyFileName)
        {
            try
            {
                // A variable to store the return value
                DataEntryControlHost controlHost =
                    UtilityMethods.CreateTypeFromAssembly<DataEntryControlHost>(assemblyFileName);

                ExtractException.Assert("ELI23676",
                    "Failed to find data entry control host implementation!", controlHost != null);

                return controlHost;
            }
            catch (Exception ex)
            {
                ExtractException ee = new ExtractException("ELI23677",
                    "Unable to initialize data entry control host!", ex);
                ee.AddDebugData("Assembly Name", assemblyFileName, false);
                throw ee;
            }
        }

        /// <summary>
        /// Sets the control which implements the data entry panel (DEP).
        /// </summary>
        /// <param name="dataEntryControlHost">The control which implements the data entry panel
        /// (DEP). <see langword="null"/> is allowed, but results in a blank DEP.</param>
        void SetDataEntryControlHost(DataEntryControlHost dataEntryControlHost)
        {
            try
            {
                if (dataEntryControlHost != _dataEntryControlHost)
                {
                    if (_dataEntryControlHost != null)
                    {
                        // Set Active = false for the old DEP so that it no longer tracks image
                        // viewer events.
                        _dataEntryControlHost.Active = false;

                        // Unregister for events and disengage shortcut handlers for the previous DEP
                        _dataEntryControlHost.SwipingStateChanged -= HandleSwipingStateChanged;
                        _dataEntryControlHost.InvalidItemsFound -= HandleInvalidItemsFound;
                        _dataEntryControlHost.UnviewedItemsFound -= HandleUnviewedItemsFound;
                        _dataEntryControlHost.ItemSelectionChanged -= HandleItemSelectionChanged;
                        if (_settings.InputEventTrackingEnabled)
                        {
                            _dataEntryControlHost.MessageHandled -= HandleMessageFilterMessageHandled;
                        }

                        _gotoNextInvalidCommand.ShortcutHandler = null;
                        _gotoNextUnviewedCommand.ShortcutHandler = null;
                        _hideToolTipsCommand.ShortcutHandler = null;
                        _acceptSpatialInfoCommand.ShortcutHandler = null;
                        _removeSpatialInfoCommand.ShortcutHandler = null;

                        _dataEntryControlHost.ClearData();

                        _dataEntryControlHost.SetDatabaseConnections(null);
                    }

                    _dataEntryControlHost = dataEntryControlHost;

                    if (_dataEntryControlHost != null)
                    {
                        // If there's database(s) available, let the control host know about it.
                        _dataEntryControlHost.SetDatabaseConnections(GetDatabaseConnections());

                        // Register for events and engage shortcut handlers for the new DEP
                        _dataEntryControlHost.SwipingStateChanged += HandleSwipingStateChanged;
                        _dataEntryControlHost.InvalidItemsFound += HandleInvalidItemsFound;
                        _dataEntryControlHost.UnviewedItemsFound += HandleUnviewedItemsFound;
                        _dataEntryControlHost.ItemSelectionChanged += HandleItemSelectionChanged;
                        if (_settings.InputEventTrackingEnabled)
                        {
                            _dataEntryControlHost.MessageHandled += HandleMessageFilterMessageHandled;
                        }

                        _gotoNextInvalidCommand.ShortcutHandler =
                            _dataEntryControlHost.GoToNextInvalid;
                        _gotoNextUnviewedCommand.ShortcutHandler =
                            _dataEntryControlHost.GoToNextUnviewed;
                        _hideToolTipsCommand.ShortcutHandler =
                            _dataEntryControlHost.ToggleHideTooltips;
                        _acceptSpatialInfoCommand.ShortcutHandler =
                            _dataEntryControlHost.AcceptSpatialInfo;
                        _removeSpatialInfoCommand.ShortcutHandler =
                            _dataEntryControlHost.RemoveSpatialInfo;
                        _undoCommand.ShortcutHandler = _dataEntryControlHost.Undo;
                        _redoCommand.ShortcutHandler = _dataEntryControlHost.Redo;

                        // Set Active = true for the new DEP so that it tracks image viewer events.
                        _dataEntryControlHost.Active = true;
                    }

                    // Load the panel into the _scrollPane
                    LoadDataEntryControlHostPanel();
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = new ExtractException("ELI23884",
                    "Failed to set DataEntryControlHost.", ex);
                ee.AddDebugData("DataEntryControlHost", dataEntryControlHost, false);
                throw ee;
            }
        }

        /// <summary>
        /// Changes the current document's document type to the specified value.
        /// </summary>
        /// <param name="documentType">The new document type</param>
        /// <param name="allowConfigurationChange"><see langword="true"/> if the configuration
        /// should be changed if the new document type calls for it, <see langword="false"/> if
        /// the current configuration should not be changed.</param>
        void ChangeActiveDocumentType(string documentType, bool allowConfigurationChange)
        {
            if (_changingDocumentType)
            {
                return;
            }

            try
            {
                _changingDocumentType = true;
                ExtractException.Assert("ELI30624", "Document type configurations not defined.",
                    _documentTypeConfigurations != null);

                bool changedDocumentType, changedDataEntryConfig;
                SetActiveDocumentType(documentType, allowConfigurationChange,
                    out changedDocumentType, out changedDataEntryConfig);
                if (changedDocumentType)
                {
                    if (changedDataEntryConfig)
                    {
                        // Adjust UI elements to reflect the new configuration.
                        SetUIConfiguration(_activeDataEntryConfig);

                        // If a DEP is being used, load the data into it
                        if (_dataEntryControlHost != null)
                        {
                            IUnknownVector attributes = GetVOAData(_imageViewer.ImageFile);
                            _dataEntryControlHost.LoadData(attributes);

                            // Undo/redo command should be unavailable until a change is actually made.
                            _undoCommand.Enabled = false;
                            _redoCommand.Enabled = false;
                        }
                    }

                    if (_dataEntryControlHost != null)
                    {
                        // Apply the new document type to the DocumentType attribute.
                        AssignNewDocumentType(documentType, changedDataEntryConfig);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI30663", ex);
            }
            finally
            {
                _changingDocumentType = false;
            }
        }

        /// <summary>
        /// Adjusts UI elements to reflect the specified configuration.
        /// </summary>
        /// <param name="config">The <see cref="DataEntryConfiguration"/> the UI should reflect.
        /// </param>
        void SetUIConfiguration(DataEntryConfiguration config)
        {
            // Change the text on certain controls if not running in stand alone mode
            if (!_standAloneMode)
            {
                bool enableSave = config != null && !config.Config.Settings.PreventSave;

                if (enableSave)
                {
                    _saveAndCommitMenuItem.Text = "&" + _SAVE_AND_COMMIT;
                    _saveAndCommitMenuItem.Image = Resources.SaveImageButtonSmall;
                    _saveAndCommitButton.Text = _SAVE_AND_COMMIT;
                    _saveAndCommitButton.ToolTipText = _SAVE_AND_COMMIT + " (Ctrl+S)";
                    _saveAndCommitButton.Image = Resources.SaveImageButton;
                }
                else
                {
                    _saveAndCommitMenuItem.Text = "&" + _NEXT_DOCUMENT;
                    _saveAndCommitMenuItem.Image = Resources.NextDocumentSmall;
                    _saveAndCommitButton.Text = _NEXT_DOCUMENT;
                    _saveAndCommitButton.ToolTipText = _NEXT_DOCUMENT + " (Ctrl+S)";
                    _saveAndCommitButton.Image = Resources.NextDocument;
                }

                if (_isLoaded)
                {
                    enableSave &= _imageViewer.IsImageAvailable;

                    _saveMenuItem.Enabled = enableSave;
                }
                else
                {
                    QueryNode.PerformanceTesting = config.Config.Settings.PerformanceTesting;
                }

                _saveAndCommitFileCommand.Enabled = (config != null && _isLoaded);
            }
        }

        /// <summary>
        /// Assigns a new document type to the DocumentType <see cref="IAttribute"/>.
        /// </summary>
        /// <param name="newDocumentType">The new document type.</param>
        /// <param name="reloadDocumentTypeAttribute">Whether to re-find the DocumentType attribute
        /// rather than use one that has already been found.</param>
        void AssignNewDocumentType(string newDocumentType, bool reloadDocumentTypeAttribute)
        {
            // If reloading the DocumentType attribute, remove event registration from the last
            // DocumentType attribute we found.
            if (reloadDocumentTypeAttribute)
            {
                _documentTypeAttribute = null;

                if (_documentTypeAttributeStatusInfo != null)
                {
                    _documentTypeAttributeStatusInfo.AttributeValueModified -=
                        HandleDocumentTypeAttributeValueModified;
                    _documentTypeAttributeStatusInfo = null;
                }
            }

            // Attempt to find a new DocumentType attribute if we don't currently have one.
            if (_documentTypeAttribute == null && _dataEntryControlHost != null)
            {
                IUnknownVector matchingAttributes =
                DataEntryMethods.AFUtility.QueryAttributes(
                    _dataEntryControlHost.Attributes, "DocumentType", false);

                int matchingAttributeCount = matchingAttributes.Size();
                if (matchingAttributeCount > 0)
                {
                    _documentTypeAttribute = (IAttribute)matchingAttributes.At(0);
                }
                else
                {
                    // Create a new DocumentType attribute if necessary.
                    _documentTypeAttribute = (IAttribute)new AttributeClass();
                    _documentTypeAttribute.Name = "DocumentType";

                    AttributeStatusInfo.Initialize(_documentTypeAttribute,
                        _dataEntryControlHost.Attributes, null);
                }

                // Register to be notified of changes to the attribute.
                _documentTypeAttributeStatusInfo =
                    AttributeStatusInfo.GetStatusInfo(_documentTypeAttribute);
                _documentTypeAttributeStatusInfo.AttributeValueModified +=
                    HandleDocumentTypeAttributeValueModified;
            }

            // If the DocumentType value was changed, refresh any DEP control that displays the
            // DocumentType.
            if (!_documentTypeAttribute.Value.String.Equals(
                    newDocumentType, StringComparison.OrdinalIgnoreCase))
            {
                AttributeStatusInfo.SetValue(_documentTypeAttribute, newDocumentType, false, true);
                IDataEntryControl dataEntryControl =
                    AttributeStatusInfo.GetOwningControl(_documentTypeAttribute);
                if (dataEntryControl != null)
                {
                    dataEntryControl.RefreshAttributes(false, _documentTypeAttribute);
                }
            }
        }

        /// <summary>
        /// Gets the document type for the specified data using the first root-level DocumentType
        /// <see cref="IAttribute"/>.
        /// </summary>
        /// <param name="attributes">The attributes representing the document's data.</param>
        /// <param name="listenForChanges"><see langword="true"/> to watch for changes to the
        /// <see cref="IAttribute"/>'s value, <see langword="false"/> otherwise.</param>
        string GetDocumentType(IUnknownVector attributes, bool listenForChanges)
        {
            string documentType = "";
            _documentTypeAttribute = null;

            // Remove event registration from the last DocumentType attribute we found.
            if (_documentTypeAttributeStatusInfo != null)
            {
                _documentTypeAttributeStatusInfo.AttributeValueModified -=
                    HandleDocumentTypeAttributeValueModified;
                _documentTypeAttributeStatusInfo = null;
            }

            // Search for the DocumentType attribute.
            IUnknownVector matchingAttributes =
                DataEntryMethods.AFUtility.QueryAttributes(
                    attributes, "DocumentType", false);

            int matchingAttributeCount = matchingAttributes.Size();
            if (matchingAttributeCount > 0)
            {
                _documentTypeAttribute = (IAttribute)matchingAttributes.At(0);
            }

            // If one was found, retrieve the document type and register to be notified of changes
            // if specified.
            if (_documentTypeAttribute != null)
            {
                documentType = _documentTypeAttribute.Value.String;

                if (listenForChanges)
                {
                    _documentTypeAttributeStatusInfo =
                        AttributeStatusInfo.GetStatusInfo(_documentTypeAttribute);
                    _documentTypeAttributeStatusInfo.AttributeValueModified +=
                        HandleDocumentTypeAttributeValueModified;
                }
            }

            return documentType;
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
            _toggleShowAllHighlightsMenuItem.CheckState =
                _showAllHighlights ? CheckState.Checked : CheckState.Unchecked;

            OnShowAllHighlightsChanged();
        }

        /// <summary>
        /// Toggles whether the view is auto-zoomed to the current selection.
        /// </summary>
        void ToggleAutoZoom()
        {
            if (_dataEntryControlHost != null)
            {
                _dataEntryControlHost.ToggleZoomToSelection();
            }
        }

        /// <summary>
        /// Toggles whether tabbing should allow groups (rows) of attributes to be
        /// selected at a time for controls in which group tabbing is enabled.
        /// </summary>
        void ToggleAllowTabbingByGroup()
        {
            _allowTabbingByGroup = !_allowTabbingByGroup;

            _allowTabbingByGroupToolStripMenuItem.CheckState =
                 _allowTabbingByGroup ? CheckState.Checked : CheckState.Unchecked;
        }

        /// <summary>
        /// Toggles whether the image viewer is displayed in a separate window.
        /// </summary>
        void ToggleSeparateImageWindow()
        {
            if (_imageViewerForm == null)
            {
                OpenSeparateImageWindow();
            }
            else
            {
                CloseSeparateImageWindow();
            }
        }

        /// <summary>
        /// Moves the <see cref="ImageViewer"/> and associated <see cref="ToolStrip"/>s to a
        /// separate <see cref="Form"/>.
        /// </summary>
        void OpenSeparateImageWindow()
        {
            // Do not allow the image window to be moved to a separate form if invisible.
            if (_invisible)
            {
                return;
            }

            // Store mode in registry so the next time it will re-open in the same state.
            _registry.Settings.ShowSeparateImageWindow = true;
            _separateImageWindowToolStripMenuItem.CheckState = CheckState.Checked;

            if (_imageViewerForm == null)
            {
                // Create the form.
                _imageViewerForm = new Form();
                _imageViewerForm.Text = _brandingResources.ApplicationTitle + " Image Window";
                _imageViewerForm.Icon = Icon;
                _imageViewerForm.SuspendLayout();

                try
                {
                    // Create a shortcut filter to handle shortcuts when the image viewer is
                    // displayed in a separate window to prevent the shortcut keys from being passed
                    // back to the main window (thereby activating it and dropping the image window
                    // to the background).
                    _imageWindowShortcutsMessageFilter = new ShortcutsMessageFilter(
                        ShortcutsEnabled, _imageViewer.Shortcuts, this);

                    if (_settings.InputEventTrackingEnabled)
                    {
                        _imageWindowShortcutsMessageFilter.MessageHandled +=
                            HandleMessageFilterMessageHandled;
                    }

                    // Move the image viewer to the new form.
                    _splitContainer.Panel2.MoveControls(_imageViewerForm, _imageViewer);

                    // Create a new SandDockManager and DockContainer for the new form, and move the
                    // _magnifierDockableWindow over to it.
                    _sandDockManager = _sandDockManager.MoveSandDockToNewForm(_imageViewerForm);

                    // Create a toolstrip container to hold the image viewer related toolstrips.
                    ToolStripContainer imageViewerFormToolStripContainer = new ToolStripContainer();
                    imageViewerFormToolStripContainer.Name = "_toolStripContainer";
                    imageViewerFormToolStripContainer.BottomToolStripPanelVisible = false;
                    imageViewerFormToolStripContainer.Dock = DockStyle.Top;
                    // Initialize to a very large size to ensure the toolstrips do not get added in
                    // in multiple rows.
                    imageViewerFormToolStripContainer.Size = new Size(9999, 9999);

                    // Move the toolstrips.
                    _toolStripContainer.TopToolStripPanel.MoveControls(
                        imageViewerFormToolStripContainer.TopToolStripPanel, _miscImageToolStrip,
                        _basicCommandsImageViewerToolStrip, _pageNavigationImageViewerToolStrip,
                        _viewCommandsImageViewerToolStrip);
                    // Size the toolstrip container correctly.
                    imageViewerFormToolStripContainer.TopToolStripPanel.AutoSize = true;
                    imageViewerFormToolStripContainer.Size =
                        imageViewerFormToolStripContainer.TopToolStripPanel.Size;

                    // Add the toolstrip container to the new form and show it.
                    _imageViewerForm.Controls.Add(imageViewerFormToolStripContainer);
                    _imageViewerForm.Show();

                    // Initialize the position using previously stored registry settings or
                    // the previous desktop location of the image viewer.
                    Point location = new Point(_registry.Settings.ImageWindowPositionX,
                        _registry.Settings.ImageWindowPositionY);
                    Rectangle workingArea = Screen.GetWorkingArea(location);
                    if (location.X != -1 && location.Y != -1 && workingArea.Contains(location))
                    {
                        _imageViewerForm.DesktopLocation = location;
                    }
                    else
                    {
                        location = new Point(DesktopLocation.X + _splitContainer.SplitterDistance,
                            DesktopLocation.Y + 10);
                        _imageViewerForm.DesktopLocation = location;
                    }

                    // Initialize the size using previously stored registry settings or
                    // the previous size of the image viewer.
                    Size size = new Size(_registry.Settings.ImageWindowWidth,
                        _registry.Settings.ImageWindowHeight);
                    if (size.Width > 0 && size.Height > 0 && workingArea.Contains(location))
                    {
                        _imageViewerForm.Size = size;
                    }
                    else
                    {
                        Size clientSize = _splitContainer.Panel2.Size;
                        clientSize.Height += imageViewerFormToolStripContainer.Height;
                        _imageViewerForm.ClientSize = clientSize;
                    }

                    // Collapse the pane the image viewer was removed from.
                    _splitContainer.Panel2Collapsed = true;

                    if (_registry.Settings.ImageWindowMaximized)
                    {
                        _imageViewerForm.WindowState = FormWindowState.Maximized;
                    }
                }
                finally
                {
                    _imageViewerForm.ResumeLayout(true);
                }

                // Ensure the image viewer gets focus.
                _imageViewer.Focus();

                _imageViewerForm.FormClosing += HandleImageViewerFormFormClosing;
                _imageViewerForm.Activated += HandleImageViewerFormActivated;
                _imageViewerForm.Resize += HandleImageViewerFormResize;
                _imageViewerForm.Move += HandleImageViewerFormMove;
            }
        }

        /// <summary>
        /// Moves the <see cref="ImageViewer"/> and associated <see cref="ToolStrip"/>s from a
        /// separate <see cref="Form"/> back into the main application form.
        /// </summary>
        void CloseSeparateImageWindow()
        {
            // Store mode in registry so the next time it will re-open in the same state.
            _registry.Settings.ShowSeparateImageWindow = false;
            _separateImageWindowToolStripMenuItem.CheckState = CheckState.Unchecked;

            if (_imageViewerForm != null)
            {
                if (_settings.InputEventTrackingEnabled)
                {
                    _imageWindowShortcutsMessageFilter.MessageHandled -=
                        HandleMessageFilterMessageHandled;
                }

                _imageWindowShortcutsMessageFilter.Dispose();
                _imageWindowShortcutsMessageFilter = null;

                Form imageViewerForm = _imageViewerForm;
                _imageViewerForm = null;

                imageViewerForm.FormClosing -= HandleImageViewerFormFormClosing;
                imageViewerForm.Activated -= HandleImageViewerFormActivated;
                imageViewerForm.Resize -= HandleImageViewerFormResize;
                imageViewerForm.Move -= HandleImageViewerFormMove;

                SuspendLayout();

                try
                {
                    // Restore the pane for the image viewer, and move the image viewer back into it.
                    _splitContainer.Panel2Collapsed = false;
                    imageViewerForm.MoveControls(_splitContainer.Panel2, _imageViewer);

                    _sandDockManager = _sandDockManager.MoveSandDockToNewForm(_splitContainer.Panel2);

                    // Move the image viewer toolstrips back into the main application window.
                    ToolStripContainer imageViewerFormToolStripContainer =
                        (ToolStripContainer)imageViewerForm.Controls["_toolStripContainer"];

                    imageViewerFormToolStripContainer.TopToolStripPanel.MoveControls(
                        _toolStripContainer.TopToolStripPanel, _miscImageToolStrip,
                        _basicCommandsImageViewerToolStrip, _pageNavigationImageViewerToolStrip,
                        _viewCommandsImageViewerToolStrip);

                    // Get rid of the separate image viewer form.
                    imageViewerForm.Close();
                    imageViewerForm.Dispose();
                }
                finally
                {
                    ResumeLayout(true);
                }
            }
        }

        /// <summary>
        /// Represents a delegate that determines whether shortcuts are enabled in _imageViewerForm.
        /// </summary>
        /// <returns><see langword="true"/> since shortcuts should always be enabled in the image
        /// window.</returns>
        static public bool ShortcutsEnabled()
        {
            return true;
        }
        
        /// <summary>
        /// Attempts to open database connection(s) for use by the DEP for validation and
        /// auto-updates if connection information is specified in the config settings.
        /// </summary>
        /// <returns>A dictionary of <see cref="DbConnection"/>(s) where the key is the connection
        /// name (blank for default). If no database connection is currently configured, any open
        /// connection will be closed and <see langword="null"/> will returned.
        /// </returns>
        Dictionary<string, DbConnection> GetDatabaseConnections()
        {
            try
            {
                if (_activeDataEntryConfig == null)
                {
                    CollectionMethods.ClearAndDispose(_dbConnections);
                }
                else
                {
                    // Retrieve the databaseConnections XML section from the active configuration if
                    // it exists
                    IXPathNavigable databaseConnections =
                        _activeDataEntryConfig.Config.GetSectionXml("databaseConnections");

                    bool loadedDefaultConnection = false;

                    // Parse and create/update each specified connection.
                    XPathNavigator databaseConnectionsNode = null;
                    if (databaseConnections != null)
                    {
                        databaseConnectionsNode = databaseConnections.CreateNavigator();

                        if (databaseConnectionsNode.MoveToFirstChild())
                        {
                            do
                            {
                                bool isDefaultConnection = false;
                                var connectionInfo =
                                    LoadDatabaseConnection(databaseConnectionsNode, out isDefaultConnection);

                                ExtractException.Assert("ELI37781",
                                    "Multiple default connections are defined.",
                                    !isDefaultConnection || !loadedDefaultConnection);

                                loadedDefaultConnection |= isDefaultConnection;

                                // https://extract.atlassian.net/browse/ISSUE-13385
                                // If this is the default connection, use the FKB version (if
                                // specified) to be able to expand the <ComponentDataDir> using
                                // _tagUtility from this point forward (including for any subsequent
                                // connection definitions).
                                if (isDefaultConnection && _tagUtility != null)
                                {
                                    AddComponentDataDirTag(connectionInfo);
                                }
                            }
                            while (databaseConnectionsNode.MoveToNext());
                        }
                    }

                    // If there was no default database connection specified via the
                    // databaseConnections section, attempt to load it via the legacy DB properties.
                    if (!loadedDefaultConnection && _activeDataEntryConfig != null)
                    {
                        SetDatabaseConnection("",
                            _activeDataEntryConfig.Config.Settings.DatabaseType,
                            _activeDataEntryConfig.Config.Settings.LocalDataSource,
                            _activeDataEntryConfig.Config.Settings.DatabaseConnectionString);
                    }
                }

                // This class keeps track of DatabaseConfigurationInfo objects for ease of
                // management, but the DataEntryControlHost only cares about the DbConnections
                // themselves; return the managed connection for each.
                return _dbConnections.ToDictionary(
                    (conn) => conn.Key, (conn) => conn.Value.ManagedDbConnection);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI26159");
            }
        }

        /// <summary>
        /// Given the specified <see paramref="databaseConnectionsNode"/>, creates or updates the
        /// specified database connection for use in the <see cref="DataEntryControlHost"/>.
        /// </summary>
        /// <param name="databaseConnectionsNode">A <see cref="XPathNavigator"/> instance from a
        /// data entry config file defining the connection.</param>
        /// <param name="isDefaultConnection"><see langword="true"/> if the connection is defined as
        /// the default database connection; otherwise, <see langword="false"/>.</param>
        /// <returns>A <see cref="DatabaseConnectionInfo"/> representing the
        /// <see cref="DbConnection"/>.</returns>
        DatabaseConnectionInfo LoadDatabaseConnection(XPathNavigator databaseConnectionsNode,
            out bool isDefaultConnection)
        {
            isDefaultConnection = false;
            string connectionName = "";
            string databaseType = "";
            string localDataSource = "";
            string databaseConnectionString = "";

            XPathNavigator attribute = databaseConnectionsNode.Clone();
            if (attribute.MoveToFirstAttribute())
            {
                do
                {
                    if (attribute.Name.Equals("name", StringComparison.OrdinalIgnoreCase))
                    {
                        connectionName = attribute.Value;
                    }
                    else if (attribute.Name.Equals("default", StringComparison.OrdinalIgnoreCase))
                    {
                        isDefaultConnection = attribute.Value.ToBoolean();
                    }
                }
                while (attribute.MoveToNextAttribute());
            }

            XPathNavigator connectionProperty = databaseConnectionsNode.Clone();
            if (connectionProperty.MoveToFirstChild())
            {
                // Load all properties of the defined connection.
                do
                {
                    // Use GetNodeValue extension method for XmlNode to allow for expansion of path
                    // tags in the config file.
                    var xmlNode = (XmlNode)connectionProperty.UnderlyingObject;

                    if (connectionProperty.Name.Equals("databaseType",
                        StringComparison.OrdinalIgnoreCase))
                    {
                        databaseType = xmlNode.GetNodeValue(_tagUtility, false, true);
                    }
                    else if (connectionProperty.Name.Equals("localDataSource",
                        StringComparison.OrdinalIgnoreCase))
                    {
                        localDataSource = xmlNode.GetNodeValue(_tagUtility, false, true);
                    }
                    else if (connectionProperty.Name.Equals("databaseConnectionString",
                        StringComparison.OrdinalIgnoreCase))
                    {
                        databaseConnectionString = xmlNode.GetNodeValue(_tagUtility, false, true);
                    }
                }
                while (connectionProperty.MoveToNext());

                if (string.IsNullOrWhiteSpace(connectionName))
                {
                    // If the connection is not named, assume it to be the default.
                    isDefaultConnection = true;
                }
                else
                {
                    SetDatabaseConnection(connectionName,
                        databaseType, localDataSource, databaseConnectionString);
                }

                // If this is the default connection, add the connection under a blank name as well
                // (blank in _dbConnections indicates the default connection.) 
                if (isDefaultConnection && !string.IsNullOrEmpty(connectionName))
                {
                    _dbConnections[""] = _dbConnections[connectionName];
                }

                return _dbConnections[connectionName];
            }

            return null;
        }

        /// <summary>
        /// Adds or updates <see cref="_dbConnections"/> with a <see cref="DatabaseConnectionInfo"/>
        /// instance under the specified <see paramref="name"/>.
        /// </summary>
        /// <param name="name">The name of the connection ("" for the default connection)</param>
        /// <param name="databaseType">The qualified type name of the database connection.</param>
        /// <param name="localDataSource">If using an SQL CE DB, the name of the DB file can be
        /// specified here in lieu of the <see paramref="connectionString"/>.</param>
        /// <param name="connectionString">The connection string to use when connecting to the DB.
        /// </param>
        void SetDatabaseConnection(string name, string databaseType, string localDataSource,
            string connectionString)
        {
            if (!string.IsNullOrWhiteSpace(localDataSource))
            {
                ExtractException.Assert("ELI37778", "Either a database connection string " +
                    "can be specified, or a local datasource-- not both.",
                    string.IsNullOrEmpty(connectionString), "Local data source", localDataSource,
                    "Connection string", connectionString);

                connectionString = SqlCompactMethods.BuildDBConnectionString(
                    DataEntryMethods.ResolvePath(localDataSource).ToLower(CultureInfo.CurrentCulture));
            }

            DatabaseConnectionInfo dbConnInfo = null;
            if (_dbConnections.TryGetValue(name, out dbConnInfo))
            {
                // If there is an existing connection by this name that differs from the newly
                // specified one (or there is no newly specified connection) close the existing
                // connection.
                if (string.IsNullOrWhiteSpace(connectionString) ||
                    Type.GetType(databaseType) != dbConnInfo.TargetConnectionType ||
                    connectionString != dbConnInfo.ConnectionString)
                {
                    _dbConnections.Remove(name);
                    if (dbConnInfo != null)
                    {
                        dbConnInfo.Dispose();
                        dbConnInfo = null;
                    }
                }
            }

            // Create the DatabaseConnectionInfo instance if needed.
            if (dbConnInfo == null && !string.IsNullOrWhiteSpace(connectionString))
            {
                dbConnInfo = new DatabaseConnectionInfo(databaseType, connectionString);
                dbConnInfo.UseLocalSqlCeCopy = true;
                _dbConnections[name] = dbConnInfo;
            }
        }

        /// <summary>
        /// If an FKBVersion value is available in the Settings table of the specified
        /// <see paramref="connectionInfo"/>, adds the &lt;ComponentDataDir&gt; tag to
        /// <see cref="_tagUtility"/>.
        /// </summary>
        /// <param name="connectionInfo">A <see cref="DatabaseConnectionInfo"/> that is expected to
        /// represent the default customer OrderMappingDB database. The tag will only be added if
        /// this database has a Settings table and that table has a populated FKBVersion setting.
        /// </param>
        void AddComponentDataDirTag(DatabaseConnectionInfo connectionInfo)
        {
            if (_tagUtility != null &&
                DBMethods.GetQueryResultsAsStringArray(connectionInfo.ManagedDbConnection,
                "SELECT COUNT(*) FROM [INFORMATION_SCHEMA].[TABLES] WHERE [TABLE_NAME] = 'Settings'")
                .Single() == "1")
            {
                string FKBVersion = DBMethods.GetQueryResultsAsStringArray(
                    connectionInfo.ManagedDbConnection,
                    "SELECT [Value] FROM [Settings] WHERE [Name] = 'FKBVersion'")
                    .SingleOrDefault();

                if (!string.IsNullOrWhiteSpace(FKBVersion))
                {
                    var ruleExecutionEnv = new RuleExecutionEnv();
                    ruleExecutionEnv.PushRSDFileName("");
                    try
                    {
                        ruleExecutionEnv.FKBVersion = FKBVersion;
                        if (FileProcessingDB != null)
                        {
                            ruleExecutionEnv.AlternateComponentDataDir =
                                FileProcessingDB.GetDBInfoSetting("AlternateComponentDataDir", false);
                        }

                        var afUtility = new AFUtility();
                        _tagUtility.AddTag("<ComponentDataDir>", afUtility.GetComponentDataFolder());
                    }
                    finally
                    {
                        ruleExecutionEnv.PopRSDFileName();
                    }
                }
            }
        }

        /// <summary>
        /// Create a new FileTaskSession table row and starts a timer for the session.
        /// </summary>
        void StartFileTaskSession()
        {
            try
            {
                _recordedDatabaseData = false;

                _fileTaskSessionID = FileProcessingDB.StartFileTaskSession(_VERIFICATION_TASK_GUID, _fileId);

                if (_paginationPanel != null)
                {
                    _paginationPanel.FileTaskSessionID = _fileTaskSessionID;
                }

                // If the timer is currently running, its current time will be the overhead time
                // (time since the previous document was saved. Restart the timer to track the
                // screen time of this document.
                if (_fileProcessingStopwatch != null && _fileProcessingStopwatch.IsRunning)
                {
                    _overheadElapsedTime = _fileProcessingStopwatch.ElapsedMilliseconds / 1000.0;
                    _fileProcessingStopwatch.Restart();
                }
                // The timer will need to be started for the first document. 
                else
                {
                    _overheadElapsedTime = 0;
                    _fileProcessingStopwatch = new Stopwatch();
                    _fileProcessingStopwatch.Start();
                }

                // Enable input event tracking.
                if (_inputEventTracker != null)
                {
                    _inputEventTracker.RegisterControl(this);
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39702");
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
                if (CurrentlyRecordingStatistics)
                {
                    // Don't count input when a document is not open.
                    if (_inputEventTracker != null)
                    {
                        _inputEventTracker.UnregisterControl(this);
                    }

                    double elapsedSeconds = _fileProcessingStopwatch.ElapsedMilliseconds / 1000.0;
                    _fileProcessingStopwatch.Restart();

                    _fileProcessingDb.UpdateFileTaskSession(
                        _fileTaskSessionID.Value, elapsedSeconds, _overheadElapsedTime.Value);
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39703");
            }
        }

        /// <summary>
        /// Records counts for the current document (represented by <see paramref="attributes"/>) to
        /// the DataEntryCounterValue table.
        /// </summary>
        /// <param name="onLoad"><see langword="true"/> if a new file is being opened,
        /// <see langword="false"/> if processing of a file is ending.</param>
        /// <param name="attributes">The attributes to be used to generate counts.</param>
        void RecordCounts(bool onLoad, IUnknownVector attributes)
        {
            try
            {
                if (_countersEnabled)
                {
                    ExtractException.Assert("ELI29829", "Cannot record database statistics since the " +
                        "database manager has not been initialized!", _dataEntryDatabaseManager != null);

                    ExtractException.Assert("ELI39701",
                        "Cannot record counts; there is not active FileTaskSession.",
                        _fileTaskSessionID != null);

                    if (onLoad)
                    {
                        _recordedDatabaseData = false;
                    }

                    if (onLoad || !_recordedDatabaseData)
                    {
                        _dataEntryDatabaseManager.RecordCounterValues(
                            onLoad, _fileTaskSessionID.Value, attributes);

                        // Ensure we never inappropriately record an entry for a file.
                        _recordedDatabaseData = !onLoad;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39704");
            }
        }

        /// <summary>
        /// Loads the DEP into the left-hand panel or separate window and positions and sizes it
        /// correctly.
        /// </summary>
        void LoadDataEntryControlHostPanel()
        {
            if (_dataEntryControlHost == null && _scrollPanel.Controls.Count > 0)
            {
                _scrollPanel.Controls.Clear();
            }
            else if (_dataEntryControlHost != null && (_scrollPanel.Controls.Count == 0 || 
                        !_scrollPanel.Controls.Contains(_dataEntryControlHost)))
            {
                if (_scrollPanel.Controls.Count > 0)
                {
                    _scrollPanel.Controls.Clear();
                }

                Control mainDataControl = (Control)_dataTab ?? _dataEntryControlHost;

                // Pad by _DATA_ENTRY_PANEL_PADDING around DEP content
                mainDataControl.Location
                    = new Point(_DATA_ENTRY_PANEL_PADDING, _DATA_ENTRY_PANEL_PADDING);
                _splitContainer.SplitterWidth = _DATA_ENTRY_PANEL_PADDING;
                if (_registry.Settings.SplitterPosition > 0)
                {
                    _splitContainer.SplitterDistance = _registry.Settings.SplitterPosition;
                }
                else
                {
                    _splitContainer.SplitterDistance = mainDataControl.Size.Width +
                        _DATA_ENTRY_PANEL_PADDING + _scrollPanel.AutoScrollMargin.Width;
                }

                _dataEntryControlHost.Anchor = AnchorStyles.Left | AnchorStyles.Top |
                    AnchorStyles.Right;

                int horizontalPadding =
                    (2 * _DATA_ENTRY_PANEL_PADDING) + _scrollPanel.AutoScrollMargin.Width;
                if (_dataTab != null)
                {
                    horizontalPadding += _tabControl.Margin.Horizontal;
                    horizontalPadding += _dataTab.Margin.Horizontal + _dataTab.Padding.Horizontal;
                }

                int minPanel1Width = _dataEntryControlHost.MinimumSize.Width;
                if (_paginationPanel != null)
                {
                    minPanel1Width = Math.Max(minPanel1Width, _paginationPanel.MinimumSize.Width);
                }

                // The splitter should respect the minimum size of the DEP.
                _splitContainer.Panel1MinSize = minPanel1Width + horizontalPadding +
                    SystemInformation.VerticalScrollBarWidth;

                // [DataEntry:3770]
                // If the image viewer is open in a separate window, the scroll panel & DEP don't
                // need to be confined to the area left of the splitter position.
                if (_imageViewerForm == null)
                {
                    // Set the width of the scroll panel and DEP based on the resulting position of the
                    // splitter to ensure the DEP is sized so that all the controls are properly
                    // visible.
                    _scrollPanel.Width = _splitContainer.SplitterDistance;
                }

                mainDataControl.Width = _scrollPanel.Width - horizontalPadding;
                if (_dataTab != null)
                {
                    _dataEntryControlHost.Width = _dataTab.ClientRectangle.Width;
                    _scrollPanel.Width = _dataTab.ClientRectangle.Width;
                }

                // Add the DEP to an auto-scroll pane to allow scrolling if the DEP is too
                // long. (The scroll pane is sized to allow the full width of the DEP to 
                // display initially) 
                _scrollPanel.Controls.Add(_dataEntryControlHost);
            }
        }

        /// <summary>
        /// Validates that the action settings for pagination are valid.
        /// </summary>
        void ValidationPaginationActions()
        {
            if (!string.IsNullOrWhiteSpace(_settings.PaginationSettings.PaginationSourceAction))
            {
                int sourceActionID = FileProcessingDB.GetActionID(
                    _settings.PaginationSettings.PaginationSourceAction);

                ExtractException.Assert("ELI40385",
                    "Cannot set pagination sources back to pending in same action",
                    sourceActionID != _actionId);
            }

            if (!string.IsNullOrWhiteSpace(_settings.PaginationSettings.PaginationOutputAction))
            {
                int outputActionID = FileProcessingDB.GetActionID(
                    _settings.PaginationSettings.PaginationOutputAction);

                ExtractException.Assert("ELI40386",
                    "Cannot set pagination output back to pending in same action",
                    outputActionID != _actionId);
            }
        }

        /// <summary>
        /// Loads any <see cref="IPaginationDocumentDataPanel"/> that has been defined in the DEP
        /// assembly.
        /// </summary>
        void LoadPaginationDocumentDataPanel()
        {
            string dataEntryPanelFileName = DataEntryMethods.ResolvePath(
                _activeDataEntryConfig.Config.Settings.DataEntryPanelFileName);

            // May be null if the DEP assembly does not define an IPaginationDocumentDataPanel.
            _paginationDocumentDataPanel =
                UtilityMethods.CreateTypeFromAssembly<IPaginationDocumentDataPanel>(
                    dataEntryPanelFileName);
            _paginationPanel.DocumentDataPanel = _paginationDocumentDataPanel;

            if (_paginationDocumentDataPanel != null)
            {
                // Allows config file to be able to update _paginationDocumentDataPanel.
                _activeDataEntryConfig.Config.ApplyObjectSettings(
                    _paginationDocumentDataPanel.Control);

                _paginationPanel.DocumentDataRequest += HandlePaginationPanel_DocumentDataRequest;
            }
        }

        /// <summary>
        /// Ensures the <see paramref="fileName"/> being loaded is in sync with the next file
        /// displayed in the pagination tab.
        /// </summary>
        /// <param name="fileName">The name of the file that is currently loading</param>
        /// <returns><see langword="true"/> if the currently loading document needs to be swapped
        /// out to correspond with the order of documents in pagination.</returns>
        bool ReorderAccordingToPagination(string fileName)
        {
            // As suggested pagination is accepted, processes the generated output in order ahead
            // of any other document in the queue
            // the file it wants to come up.
            if (_paginationOutputOrder.Any())
            {
                if (_paginationOutputOrder.Peek() != fileName)
                {
                    try
                    {
                        FileRequestHandler.PauseProcessingQueue();
                        int neededFileId = FileProcessingDB.GetFileID(_paginationOutputOrder.Peek());
                        RequestFile(neededFileId);
                    }
                    catch (Exception ex)
                    {
                        _paginationOutputOrder.Clear();

                        throw ex.AsExtract("ELI40234");
                    }
                    finally
                    {
                        FileRequestHandler.ResumeProcessingQueue();
                    }

                    return true;
                }

                _paginationOutputOrder.Dequeue();

                // To help prevent that we might need to swap out the next file that attempts to
                // load, request the subsequent document so that it is what comes into
                // verification after this file.
                if (_paginationOutputOrder.Any())
                {
                    try
                    {
                        int nextFileID = FileProcessingDB.GetFileID(_paginationOutputOrder.Peek());
                        RequestFile(nextFileID);
                    }
                    catch (Exception ex)
                    {
                        _paginationOutputOrder.Clear();

                        throw ex.AsExtract("ELI40235");
                    }
                }
            }

            return false;
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
            if (_paginationPanel == null || _paginationPanel.SourceDocuments.Contains(fileName))
            {
                return;
            }

            // Look in the voa file for rules-suggested pagination.
            // TODO: Warn if the document was previously paginated.
            string dataFilename = fileName + ".voa";
            if (File.Exists(dataFilename))
            {
                IUnknownVector attributes = GetVOAData(fileName);
                // Allow DEP to use the data loaded here rather than separately re-loading it.
                _cachedVOAData[fileName] = attributes;

                PaginationDocumentData documentData = GetAsPaginationDocumentData(attributes);

                var attributeArray = attributes
                    .ToIEnumerable<IAttribute>()
                    .ToArray();
                var rootAttributeNames = attributeArray
                    .Select(attribute => attribute.Name)
                    .Distinct();

                // If only "Document" attributes exist at the root of the VOA file, there is
                // rules-suggested pagination.
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
                        // There are three attributes that may be under the root Document attribute:
                        // Pages- The range/list specification of pages to be included.
                        // DeletedPages- The range/list specification of pages to be shown as deleted.
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
                                !deletedPages.Any() &&
                                pages.Count() == pageCount)
                            {
                                suggestedPagination = false;

                                // If there is only one virtual document associated with this
                                // document, treat the voa file as if it only contained the data
                                // for verification (and not the Document/DocumentData hierarchy
                                // used for pagination).
                                _cachedVOAData[fileName] = documentAttributes;
                            }
                            else
                            {
                                suggestedPagination = true;
                            }
                        }

                        _paginationPanel.LoadFile(
                            fileName, -1, pages, deletedPages, suggestedPagination.Value, documentData, false);
                    }

                    return;
                }

                // There was a VOA file, just not with suggested pagination. Pass on the VOA data.
                _paginationPanel.LoadFile(fileName, -1, null, null, false, documentData, false);
                return;
            }

            // If there was no rules-suggested pagination, go ahead and load the physical document
            // into the _paginationPanel
            _paginationPanel.LoadFile(fileName, -1, false);
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
                ? _paginationDocumentDataPanel.GetDocumentData(
                    attributes, FileProcessingDB, _imageViewer)
                : null;
        }

        /// <summary>
        /// Loads the data from the specified VOA file or retrieve data that has already been
        /// cached for the file.
        /// </summary>
        /// <param name="fileName">The file for which VOA data should be loaded.</param>
        /// <returns>The VOA data.</returns>
        IUnknownVector GetVOAData(string fileName)
        {
            IUnknownVector attributes = null;

            if (_cachedVOAData.TryGetValue(fileName, out attributes))
            {
                return attributes;
            }

            attributes = (IUnknownVector)new IUnknownVectorClass();

            // If an image was loaded, look for and attempt to load corresponding data.
            string dataFilename = fileName + ".voa";
            if (File.Exists(dataFilename))
            {
                _voaFileLock.GetLock(dataFilename,
                    _standAloneMode ? "" : "Data entry verification");

                attributes.LoadFrom(dataFilename, false);
            }
            return attributes;
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
                string outputDocPath = _settings.PaginationSettings.PaginationOutputPath;
                string sourceDocName = e.SourcePageInfo.First().DocumentName;
                var pathTags = new FileActionManagerPathTags((FAMTagManager)_tagUtility, sourceDocName);
                if (outputDocPath.Contains(PaginationSettings.SubDocIndexTag))
                {
                    string query = string.Format(CultureInfo.InvariantCulture,
                        "SELECT COUNT(DISTINCT([DestFileID])) AS [PreviouslyOutput] " +
                        "   FROM [Pagination] " +
                        "   INNER JOIN [FAMFile] ON [Pagination].[SourceFileID] = [FAMFile].[ID] " +
                        "   WHERE [FileName] = '{0}'",
                        sourceDocName.Replace("'", "''"));

                    var recordset = FileProcessingDB.GetResultsForQuery(query);
                    int subDocIndex = e.SubDocIndex + (int)recordset.Fields["PreviouslyOutput"].Value;
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

                return pathTags.Expand(_settings.PaginationSettings.PaginationOutputPath);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39891");
            }
        }

        /// <summary>
        /// Gets the priority to assign a paginated output file that needs to be sent back for
        /// processing.
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
                    "   AND [FileName] IN ({1})", _actionId, sourceDocNames);

                var recordset = FileProcessingDB.GetResultsForQuery(query);
                var priority = (EFilePriority)recordset.Fields["MaxPriority"].Value;
                recordset.Close();

                priority = (EFilePriority)
                    Math.Max((int)priority,
                        (int)_settings.PaginationSettings.PaginatedOutputPriority);

                return priority;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39892");
            }
        }

        /// <summary>
        /// Grabs the specified document for immediate verification in the current session. Any
        /// indexing data included in <see paramref="e"/> will be saved to VOA and used by
        /// verification.
        /// </summary>
        /// <param name="fileID">The FAM file ID of the document.</param>
        /// <param name="e">The <see cref="CreatingOutputDocumentEventArgs"/> instance containing
        /// data about the <see cref="PaginationPanel.CreatingOutputDocument"/> event for which this
        /// call is being made.</param>
        /// <param name="pageMap">Each key represents a tuple of the old document name and page
        /// number while the value represents the new page number(s) in 
        /// <see paramref="newDocumentName"/> associated with that source page.</param>
        /// <param name="newSpatialPageInfos">The new spatial page infos to be associated with produced VOA.</param>
        void GrabDocumentForVerification(int fileID, CreatingOutputDocumentEventArgs e,
            Dictionary<Tuple<string, int>, List<int>> pageMap, LongToObjectMap newSpatialPageInfos)
        {
            try
            {
                // Produce a voa file for the paginated document using the data the rules suggested.
                var documentData = e.DocumentData as PaginationDocumentData;
                if (documentData != null)
                {
                    var copyThis = (ICopyableObject)documentData.Attributes;
                    var attributesCopy = (IUnknownVector)copyThis.Clone();
                    attributesCopy.ReportMemoryUsage();

                    AttributeMethods.TranslateAttributesToNewDocument(
                        attributesCopy, e.OutputFileName, pageMap, newSpatialPageInfos);

                    attributesCopy.SaveTo(e.OutputFileName + ".voa", false,
                        _ATTRIBUTE_STORAGE_MANAGER_GUID);
                    attributesCopy.ReportMemoryUsage();

                    // Though saved out to file, it is this object that will be used for the
                    // newly paginated document; it should not be marked dirty at this point.
                    documentData.SetOriginalForm();
                }

                EActionStatus previousStatus;
                bool success =
                    FileRequestHandler.CheckoutForProcessing(fileID, false, out previousStatus);
                ExtractException.Assert("ELI39621", "Failed to check out file", success,
                    "FileID", fileID);
                success = FileRequestHandler.SetFallbackStatus(fileID, EActionStatus.kActionPending);
                ExtractException.Assert("ELI39622", "Failed to set fallback status", success,
                    "FileID", fileID);
                _paginationOutputOrder.Enqueue(e.OutputFileName);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39889");
            }
        }

        #endregion Private Members
    }
}
