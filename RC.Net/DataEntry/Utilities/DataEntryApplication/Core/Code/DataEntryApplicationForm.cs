using Extract.AttributeFinder;
using Extract.DataEntry.Utilities.DataEntryApplication.Properties;
using Extract.FileActionManager.Forms;
using Extract.Imaging;
using Extract.Imaging.Forms;
using Extract.Imaging.Utilities;
using Extract.Licensing;
using Extract.Utilities;
using Extract.Utilities.Forms;
using Extract.UtilityApplications.PaginationUtility;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using TD.SandDock;
using UCLID_AFCORELib;
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
        /// Manages all <see cref="DataEntryConfiguration"/>s currently available. Multiple
        /// configurations will exist when there are multiple DEPs defined where the one used depends
        /// on doc-type.
        /// </summary>
        DataEntryConfigurationManager<Properties.Settings> _configManager;

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
        /// Indicates whether the an image is currently open for verification in the main data entry
        /// panel (data tab if pagination is enabled).
        /// </summary>
        bool _imageOpened;

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
        /// Indicates when the initial load for a document is occuring.
        /// </summary>
        bool _loading;

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
        HashSet<IAttribute> _paginationAttributesToRefresh = new HashSet<IAttribute>();

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

        /// <summary>
        /// Indicates whether AttributeStatusInfo.DisposeThread is pending.
        /// </summary>
        bool _disposeThreadPending;

        /// <summary>
        /// The InputActivityTimeout from database DBInfo, default is 30 sec
        /// </summary>
        int _inputActivityTimeout = 30;

        /// <summary>
        /// Utility methods to generalte new paginated output files and record them into in the FAM database.
        /// </summary>
        PaginatedOutputCreationUtility _paginatedOutputCreationUtility;

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

                UnlockLeadtools.UnlockLeadToolsSupport();

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

                QueryNode.QueryCacheLimit = _applicationConfig.Settings.QueryCacheLimit;

                // Since SpotIR compatibility is not required for data entry applications, avoid the
                // performance hit it exacts.
                Highlight.SpotIRCompatible = false;

                _standAloneMode = standAloneMode;
                _fileProcessingDb = fileProcessingDB;
                _actionId = actionId;
                _fileRequestHandler = fileRequestHandler;

                if (settings.CountersEnabled)
                {
                    ExtractException.Assert("ELI29828", "Cannot enable data counters" +
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

                if (_fileProcessingDb != null)
                {
                    // Get the InputActivityTimeout from the database
                    string activityTimeout = _fileProcessingDb.GetDBInfoSetting("InputActivityTimeout", false);
                    if (!string.IsNullOrWhiteSpace(activityTimeout))
                    {
                        _inputActivityTimeout = int.Parse(activityTimeout, CultureInfo.InvariantCulture);
                    }
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

                if (_applicationConfig.Settings.EnableLogging)
                {
                    AttributeStatusInfo.Logger = Logger.CreateLogger(
                        _applicationConfig.Settings.LogToFile,
                        _applicationConfig.Settings.LogFilter,
                        _applicationConfig.Settings.InputEventFilter,
                        this);
                }

                _configManager = new DataEntryConfigurationManager<Properties.Settings>(this, _tagUtility,
                    _applicationConfig, _imageViewer, _documentTypeComboBox);

                // ComponentData directories referenced by configuration databases will be cached.
                // Clear any cached ComponentData directory each time the UI is opened.
                DataEntryConfiguration.ResetComponentDataDir();

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
                // https://extract.atlassian.net/browse/ISSUE-15445
                // Removed code that disabled visual styles in order to allow active control
                // selection for combo boxes be indicated (ISSUE-473). This fix is suspected of
                // causing a crash. Also, this setting is inconsistent with the pagination task
                // that does use visual styles.
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
                return false;
            }
        }

        /// <summary>
        /// Gets the active <see cref="DataEntryControlHost"/>.
        /// </summary>
        public DataEntryControlHost ActiveDataEntryControlHost
        {
            get
            {
                return DataEntryControlHost;
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
                if (_applicationConfig.Settings.DisableSkip)
                {
                    _skipProcessingMenuItem.Enabled = false;
                }
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

                ExtractException.Assert("ELI29830", "Unexpected file processing database!",
                    _fileProcessingDb == fileProcessingDB);
                ExtractException.Assert("ELI29831", "Unexpected database action ID!",
                    _fileProcessingDb == null || _actionId == actionID);

                // These variables should be initialized here before the potential call to
                // AbortProcessing, otherwise AbortProcessing will report a bad file ID is being
                // aborted.
                // https://extract.atlassian.net/browse/ISSUE-15316
                _fileId = fileID;
                _fileName = fileName;
                _fileTaskSessionID = null;

                // In order to keep the order documents are displayed in sync with the
                // _paginationPanel, swap out the loading file for another if necessary.
                if (_paginationPanel != null && ReorderAccordingToPagination(fileName))
                {
                    AbortProcessing(EFileProcessingResult.kProcessingDelayed, false);
                    return;
                }

                _tagFileToolStripButton.Database = fileProcessingDB;
                _tagFileToolStripButton.FileId = fileID;
                // For consistency with other buttons, keep disabled until the file is loaded.
                _tagFileToolStripButton.Enabled = false;

                // Create input event tracker before file task session is started
                // because StartFileTaskSession() registers this form with the tracker
                if (_inputEventTracker == null && fileProcessingDB != null)
                {
                    _inputEventTracker = new InputEventTracker(fileProcessingDB, actionID);
                }

                StartFileTaskSession();

                if (_dataEntryDatabaseManager == null && fileProcessingDB != null)
                {
                    FAMDBUtils famDBUtils = new FAMDBUtils();
                    Type mgrType = Type.GetTypeFromProgID(famDBUtils.GetDataEntryDBProgId());

                    _dataEntryDatabaseManager = (DataEntryProductDBMgr)Activator.CreateInstance(mgrType);
                    _dataEntryDatabaseManager.Initialize(fileProcessingDB);
                }

                if (_paginationPanel != null)
                {
                    LoadDocumentForPagination(fileName, fileID);

                    bool paginationSuggested = _paginationPanel.IsPaginationSuggested(fileName);
                    // https://extract.atlassian.net/browse/ISSUE-15805
                    // In the case that we are configured to require viewing all pages for pagination,
                    // anytime there is more than one page, go straight to the pagination tab.
                    bool pagesShouldBeViewed =
                        _paginationPanel.RequireAllPagesToBeViewed && _paginationPanel.PageCount > 1;

                    if (paginationSuggested || pagesShouldBeViewed)
                    {
                        // When jumping straight into pagination, still update the title bar to
                        // reflect the active document.
                        string imageName = Path.GetFileName(fileName);
                        base.Text = imageName + " - " + _brandingResources.ApplicationTitle + " (" + _actionName + ")";
                        if (_imageViewerForm != null)
                        {
                            _imageViewerForm.Text = imageName + " - " +
                                _brandingResources.ApplicationTitle + " Image Window";
                        }

                        _tabControl.SelectedTab = _paginationTab;

                        // https://extract.atlassian.net/browse/ISSUE-16026
                        // Setting PendingChanges explicity prevents the panel from calculating on
                        // its own whether changes are pending. Set to true if pagination is
                        // suggested, but don't set to false if pagination is not suggested.
                        if (paginationSuggested)
                        {
                            _paginationPanel.PendingChanges = true;
                        }

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

                if (_fileProcessingDb != null && DataEntryControlHost != null)
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

                // https://extract.atlassian.net/browse/ISSUE-15728
                // Ensure the file task session for this file is ended if it was started.
                if (_fileTaskSessionID != null)
                {
                    EndFileTaskSession();
                }
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
                // DataEntryControlHost.Prefetch(fileName);
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

                _paginatedOutputCreationUtility = new PaginatedOutputCreationUtility(
                    _settings.PaginationSettings.PaginationOutputPath, FileProcessingDB, _actionId, workflowID: -1);

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
                    else
                    {
                        WindowState = FormWindowState.Normal;
                    }
                }

                _configManager.ConfigurationChanged += HandleConfigManager_ConfigurationChanged;
                _configManager.ConfigurationChangeError += HandleConfigManager_ConfigurationChangeError;
                _configManager.DocumentTypeChanged += HandleConfigManager_DocumentTypeChanged;

                string expandedConfigFileName =
                    _tagUtility.ExpandTagsAndFunctions(_settings.ConfigFileName, null, null);
                _configManager.LoadDataEntryConfigurations(expandedConfigFileName);

                if (!_standAloneMode && _settings.PaginationEnabled)
                {
                    ExtractException.Assert("ELI43208",
                        "Pagination cannot be performed for more than one workflow at a time.",
                        !FileProcessingDB.RunningAllWorkflows);

                    _paginationPanel.ImageViewer = _imageViewer;
                    _paginationPanel.ExpectedPaginationAttributesPath =
                        _settings.PaginationSettings.ExpectedPaginationAttributesOutputPath;
                    _paginationPanel.OutputExpectedPaginationAttributesFile =
                        _settings.PaginationSettings.OutputExpectedPaginationAttributesFiles;
                    _paginationPanel.RequireAllPagesToBeViewed =
                        _settings.PaginationSettings.RequireAllPagesToBeViewed;
                    _paginationPanel.FileProcessingDB = FileProcessingDB;
                    _paginationPanel.SaveButtonVisible = false;

                    ValidatePaginationActions();

                    LoadPaginationDocumentDataPanel();

                    // Register for OutputDocumentCreated event in order to set status for the
                    // PaginationOutputAction _after_ the document has been created
                    _paginationPanel.OutputDocumentCreated += HandlePaginationPanel_OutputDocumentCreated;

                    if (_configManager.RegisteredDocumentTypes.Any())
                    {
                        _scrollPanel.Top = _documentTypePanel.Bottom + 1;
                        _scrollPanel.Height -= _scrollPanel.Top;
                        _documentTypePanel.Visible = true;
                    }
                    else
                    {
                        _dataTab.Controls.Remove(_documentTypePanel);
                        _scrollPanel.Dock = DockStyle.Fill;
                    }

                    // Show pagination tab before showing the data tab to trigger the pagination
                    // control to be created and loaded.
                    _paginationTab.Show();
                    _dataTab.Show();

                    // https://extract.atlassian.net/browse/ISSUE-17541
                    // Default pagination tab to the parked state to prevent the panel from trying to
                    // handle shortcut keys until it is activated.
                    _paginationPanel.Park();
                }
                else
                {
                    // If pagination is not needed, move the panel that houses the DEP out of the
                    // tab control and directly into the left side of _splitContainer. 
                    _dataTab.Controls.Remove(_documentTypePanel);
                    _dataTab.Controls.Remove(_scrollPanel);
                    _splitContainer.Panel1.Controls.Remove(_tabControl);
                    _splitContainer.Panel1.Controls.Add(_documentTypePanel);
                    _splitContainer.Panel1.Controls.Add(_scrollPanel);

                    _dataTab = null;
                    _tabControl = null;
                    _paginationTab = null;
                    _paginationPanel = null;

                    if (_configManager.RegisteredDocumentTypes.Any())
                    {
                        _scrollPanel.Top = _documentTypePanel.Bottom + 1;
                        _scrollPanel.Height -= _scrollPanel.Top;
                        _documentTypePanel.Visible = true;
                    }
                    else
                    {
                        _splitContainer.Panel1.Controls.Remove(_documentTypePanel);
                        _scrollPanel.Dock = DockStyle.Fill;
                    }
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
                SetUIConfiguration(ActiveDataEntryConfig);

                // This needs to be done after the window handle has been created
                if (_documentTypeComboBox.Visible)
                {
                    _documentTypeComboBox.SetAutoCompleteValues(_configManager.RegisteredDocumentTypes);
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
                            _paginationPanel.RemoveSourceFile(_fileName, acceptingPagination: false);
                        }

                        // Record statistics to database that needs to happen when a file is closed.
                        if (!_standAloneMode && _dataEntryDatabaseManager != null)
                        {
                            RecordCounts(onLoad: false, attributes: null);
                            EndFileTaskSession();
                        }
                    }
                }

                if (!e.Cancel && DataEntryControlHost != null)
                {
                    // Clear data to give the host a chance to clear any static COM objects that will
                    // not be accessible from a different thread due to the single apartment threading
                    // model.
                    DataEntryControlHost.ClearData();
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

                    if (_configManager != null)
                    {
                        _configManager.Dispose();
                        _configManager = null;
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
                    if (_disposeThreadPending)
                    {
                        AttributeStatusInfo.DisposeThread();
                        _disposeThreadPending = false;
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
        /// Handles the <see cref="DataEntryConfigurationManager.ConfigurationChanged"/> event of the
        /// <see cref="_configManager"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ConfigurationChangedEventArgs"/> instance containing the
        /// event data.</param>
        void HandleConfigManager_ConfigurationChanged(object sender, ConfigurationChangedEventArgs e)
        {
            // Adjust UI elements to reflect the new configuration.
            SetUIConfiguration(ActiveDataEntryConfig);

            // If a DEP is being used, load the data into it
            if (DataEntryControlHost != null)
            {
                var oldDataEntryControlHost = e.OldDataEntryConfiguration?.DataEntryControlHost;
                var newDataEntryControlHost = e.NewDataEntryConfiguration?.DataEntryControlHost;

                if (oldDataEntryControlHost != newDataEntryControlHost)
                {
                    // Undo/redo command should be unavailable until a change is actually made.
                    _undoCommand.Enabled = false;
                    _redoCommand.Enabled = false;

                    if (oldDataEntryControlHost != null)
                    {
                        // Make sure to update the attribute collection that the config manager is using
                        // so that the correct document type attribute is updated.
                        // https://extract.atlassian.net/browse/ISSUE-14347
                        // Ignore the attributes if none were ever loaded (the default config's control
                        // host will have an empty attribute vector initially)
                        if (oldDataEntryControlHost.IsDocumentLoaded)
                        {
                            _configManager.Attributes = oldDataEntryControlHost.GetData(
                                validateData: false, pruneUnmappedAttributes: false);
                        }

                        // Set Active = false for the old DEP so that it no longer tracks image
                        // viewer events.
                        oldDataEntryControlHost.Active = false;

                        // Unregister for events and disengage shortcut handlers for the previous DEP
                        oldDataEntryControlHost.SwipingStateChanged -= HandleSwipingStateChanged;
                        oldDataEntryControlHost.DataValidityChanged -= HandleDataValidityChanged;
                        oldDataEntryControlHost.UnviewedDataStateChanged -= HandleUnviewedDataStateChanged;
                        oldDataEntryControlHost.ItemSelectionChanged -= HandleItemSelectionChanged;
                        oldDataEntryControlHost.MessageHandled -= HandleMessageFilterMessageHandled;

                        _gotoNextInvalidCommand.ShortcutHandler = null;
                        _gotoNextUnviewedCommand.ShortcutHandler = null;
                        _hideToolTipsCommand.ShortcutHandler = null;
                        _acceptSpatialInfoCommand.ShortcutHandler = null;
                        _removeSpatialInfoCommand.ShortcutHandler = null;

                        oldDataEntryControlHost.ClearData();
                        
                        // Don't preserve undo state between DEPs
                        // https://extract.atlassian.net/browse/ISSUE-14335
                        AttributeStatusInfo.UndoManager.ClearHistory();
                    }

                    if (newDataEntryControlHost != null)
                    {
                        // Load the panel into the _scrollPane
                        LoadDataEntryControlHostPanel();

                        // https://extract.atlassian.net/browse/ISSUE-17258
                        // If the configuration change is happening as part of loading a document,
                        // don't load the data into the panel now as it will be done by HandleImageFileChanged.
                        // This prevents data from being loaded twice and ensures initial field selection
                        // occurs correctly.
                        if (!_loading)
                        {
                            newDataEntryControlHost.LoadData(_configManager.Attributes, _fileName,
                                forEditing: true, initialSelection: FieldSelection.First);
                        }

                        // Register for events and engage shortcut handlers for the new DEP
                        newDataEntryControlHost.SwipingStateChanged += HandleSwipingStateChanged;
                        newDataEntryControlHost.DataValidityChanged += HandleDataValidityChanged;
                        newDataEntryControlHost.UnviewedDataStateChanged += HandleUnviewedDataStateChanged;
                        newDataEntryControlHost.ItemSelectionChanged += HandleItemSelectionChanged;
                        newDataEntryControlHost.MessageHandled += HandleMessageFilterMessageHandled;

                        _gotoNextInvalidCommand.ShortcutHandler =
                            newDataEntryControlHost.GoToNextInvalidWithPromptIfNone;
                        _gotoNextUnviewedCommand.ShortcutHandler =
                            newDataEntryControlHost.GoToNextUnviewed;
                        _hideToolTipsCommand.ShortcutHandler =
                            newDataEntryControlHost.ToggleHideTooltips;
                        _acceptSpatialInfoCommand.ShortcutHandler =
                            newDataEntryControlHost.AcceptSpatialInfo;
                        _removeSpatialInfoCommand.ShortcutHandler =
                            newDataEntryControlHost.RemoveSpatialInfo;
                        _undoCommand.ShortcutHandler = newDataEntryControlHost.Undo;
                        _redoCommand.ShortcutHandler = newDataEntryControlHost.Redo;

                        // Set Active = true for the new DEP so that it tracks image viewer events.
                        newDataEntryControlHost.Active = true;

                        // The combo box registers an IMessageFilter with the active DataEntryControlHost to intercept keyboard and mouse events
                        _documentTypeComboBox.DataEntryControlHost = newDataEntryControlHost;
                    }
                }
            }
        }

        /// <summary>
        /// Handles the <see cref="DataEntryConfigurationManager.DocumentTypeChanged"/> event of the
        /// <see cref="_configManager"/>.
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        void HandleConfigManager_DocumentTypeChanged(object sender, EventArgs e)
        {
            if (ActiveDataEntryControlHost != null)
            {
                ActiveDataEntryControlHost.Dirty = true;
            }
        }

        /// <summary>
        /// Handles the <see cref="DataEntryConfigurationManager.ConfigurationChangeError"/> event
        /// of the <see cref="_configManager"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="VerificationExceptionGeneratedEventArgs"/> instance containing the event data.</param>
        void HandleConfigManager_ConfigurationChangeError(object sender, VerificationExceptionGeneratedEventArgs e)
        {
            RaiseVerificationException(e.Exception, e.CanProcessingContinue);
        }

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
                    ActiveDataEntryConfig == null ||
                    !_applicationConfig.Settings.PreventSave);

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
                // Ensure the data in the controls is cleared when a document is closed or prior to
                // loading any new data.
                _configManager.ClearData();

                if (!_imageViewer.IsImageAvailable)
                {
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

                    _loading = true;
                    _documentLoadCount++;

                    // [DataEntry:693]
                    // On WindowsXP, resources associated with the auto-complete list (
                    // particularly GDI objects) do not seem to be cleaned up and eventually 
                    // "Error creating window handle" exceptions will result. Calling GC.Collect
                    // cleans up these resources. (GCFrequency default == 1)
                    if (_imageViewer.IsImageAvailable && ActiveDataEntryConfig != null &&
                        ActiveDataEntryConfig.Config.Settings.GCFrequency > 0 &&
                        _documentLoadCount % ActiveDataEntryConfig.Config.Settings.GCFrequency == 0)
                    {
                        GC.Collect();
                    }

                    string imageName = Path.GetFileName(_imageViewer.ImageFile);
                    base.Text = imageName + " - " + _brandingResources.ApplicationTitle + " (" + _actionName + ")";
                    if (_imageViewerForm != null)
                    {
                        _imageViewerForm.Text = imageName + " - " + _imageViewerForm.Text;
                    }

                    IUnknownVector attributes = GetVOAData(_imageViewer.ImageFile);
                    _configManager.LoadCorrectConfigForData(attributes);
                    attributes = _configManager.Attributes;
                    // Record counts on load
                    if (!_standAloneMode && _fileProcessingDb != null)
                    {
                        RecordCounts(onLoad: true, attributes: attributes);
                    }

                    // If a DEP is being used, load the data into it
                    if (DataEntryControlHost != null)
                    {
                        DataEntryControlHost.LoadData(attributes, _fileName, forEditing: true, 
                            initialSelection: FieldSelection.First);

                        // Now that the data has been loaded into the DEP, update the document data
                        // in the pagination panel so that it is sharing the same attributes
                        // hierarchy with the DEP (rather than a separately loaded copy).
                        if (_paginationPanel != null && _paginationDocumentDataPanel != null)
                        {
                            PaginationDocumentData documentData =
                                GetAsPaginationDocumentData(attributes);
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
                    base.Text = "Waiting - " + _brandingResources.ApplicationTitle + " (" + _actionName + ")";

                    if (DataEntryControlHost != null)
                    {
                        DataEntryControlHost.LoadData(null, null, forEditing: true,
                            initialSelection: FieldSelection.First);
                    }
                }
                else
                {
                    base.Text = _brandingResources.ApplicationTitle;
                }

                // If in standalone mode, no need to enable/disable _saveAndCommitFileCommand
                if (!_standAloneMode)
                {
                    // [DataEntry:1108]
                    // Enable/disable the save and commit command without respect to whether
                    // PreventSave is set. If PreventSave is set, the option should still be
                    // available; its behavior will just be different.
                    _saveAndCommitFileCommand.Enabled = _imageViewer.IsImageAvailable &&
                        ActiveDataEntryConfig != null;
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
                        ActiveDataEntryConfig != null &&
                        !_applicationConfig.Settings.PreventSave;

                    if (_fileProcessingDb != null)
                    {
                        _skipProcessingMenuItem.Enabled = 
                            !_applicationConfig.Settings.DisableSkip && _imageViewer.IsImageAvailable;
                        _tagFileToolStripButton.Enabled = _imageViewer.IsImageAvailable;
                    }
                }

                // [DataEntry:414]
                // A document should only be allowed to be closed in FAM mode
                _closeFileCommand.Enabled = (_standAloneMode && _imageViewer.IsImageAvailable);

                _documentTypeComboBox.Enabled = _imageViewer.IsImageAvailable;
                _imageOpened = _imageViewer.IsImageAvailable;

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
            finally
            {
                _loading = false;
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
                    (DataEntryControlHost != null && !DataEntryControlHost.IsDocumentLoaded))
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
                        OnFileComplete(_fileId, EFileProcessingResult.kProcessingCancelled);
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
        /// <param name="e">An <see cref="EventArgs"/> instance containing the
        /// event data.</param>
        void HandleUnviewedDataStateChanged(object sender, EventArgs e)
        {
            try
            {
                _gotoNextUnviewedCommand.Enabled = DataEntryControlHost.IsDataUnviewed;
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
        /// <param name="e">An <see cref="EventArgs"/> instance containing the event data.</param>
        void HandleDataValidityChanged(object sender, EventArgs e)
        {
            try
            {
                _gotoNextInvalidCommand.Enabled =
                    DataEntryControlHost.DataValidity != DataValidity.Valid;
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
                if (DataEntryControlHost != null)
                {
                    DataEntryControlHost.GoToNextUnviewed();
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
                if (DataEntryControlHost != null)
                {
                    DataEntryControlHost.GoToNextInvalidWithPromptIfNone();
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
                if (DataEntryControlHost != null)
                {
                    DataEntryControlHost.ToggleHideTooltips();
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
                if (DataEntryControlHost != null)
                {
                    DataEntryControlHost.AcceptSpatialInfo();
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
                if (DataEntryControlHost != null)
                {
                    DataEntryControlHost.RemoveSpatialInfo();
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
        /// Handles the case that the user selected the "Undo" button or menu item.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">The event data associated with the event.</param>
        void HandleUndoClick(object sender, EventArgs e)
        {
            try
            {
                if (DataEntryControlHost != null)
                {
                    DataEntryControlHost.Undo();
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
                if (DataEntryControlHost != null)
                {
                    DataEntryControlHost.Redo();
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
                    // Parked check is to see if the tab change is a result of verification closing
                    // (in which case the pagination tab will already be parked)
                    else if (e.TabPage != _paginationTab && !_paginationPanel.Parked &&
                        _paginationPanel.RequireAllPagesToBeViewed && !_paginationPanel.AllPagesViewed)
                    {
                        if (MessageBox.Show(null, 
                            "There are pages that have not been viewed. Proceed anyway?",
                            "Unviewed pages", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, 
                            MessageBoxDefaultButton.Button2, 0) == DialogResult.No)
                        {
                            e.Cancel = true;
                        }
                    }
                    // Though not easily reproducible, switching to the pagination tab before a
                    // document is fully loaded can cause exceptions for a null _imageViewer in
                    // FinalizeDocumentLoad. Prevent tab changes until a document fully loaded,
                    // though allow for tab changes for form setup before any files are loaded.                    
                    else if (e.TabPage == _paginationTab && _imageViewer.IsImageAvailable &&
                        DataEntryControlHost != null && !DataEntryControlHost.IsDocumentLoaded)
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

                if (_tabControl.SelectedTab == _dataTab && DataEntryControlHost.ImageViewer == null)
                {
                    // Ensure the _paginationPanel won't try to do anything with the image viewer
                    // while the data tab has focus.
                    _paginationPanel.Park();

                    // https://extract.atlassian.net/browse/ISSUE-15805
                    // In order to force a load of a document that has not yet been loaded due to the pagination
                    // tab being defaulted for a document we need to call OpenImage after registering for
                    // the events below. But to prevent the ImageClose that ImageOpen may trigger from
                    // triggering verification to close, close the image before registering for the
                    // events.
                    if (!_imageOpened && !_paginating)
                    {
                        _imageViewer.CloseImage();
                    }
                    // The pagination control may have closed the document; re-open it if necessary.
                    else if (!string.IsNullOrWhiteSpace(_fileName) &&
                        _imageViewer.ImageFile != _fileName)
                    {
                        _imageViewer.OpenImage(_fileName, false);
                    }

                    DataEntryControlHost.ImageViewer = _imageViewer;
                    DataEntryControlHost.DisableKeyboardInput = false;
                    _imageViewer.AllowHighlight = true;
                    _imageViewer.ImageFileChanged += HandleImageFileChanged;
                    _imageViewer.ImageFileClosing += HandleImageFileClosing;
                    _imageViewer.LoadingNewImage += HandleLoadingNewImage;

                    // https://extract.atlassian.net/browse/ISSUE-15805
                    // Force a load of a document that has not yet been loaded due to the pagination
                    // tab being defaulted for a document.
                    if (!_imageOpened && !_paginating && !string.IsNullOrWhiteSpace(_fileName))
                    {
                        _imageViewer.OpenImage(_fileName, false);
                    }

                    _saveAndCommitFileCommand.Enabled = _imageViewer.IsImageAvailable;
                    _saveMenuItem.Enabled = _imageViewer.IsImageAvailable;
                    _skipProcessingMenuItem.Enabled =
                            !_applicationConfig.Settings.DisableSkip && _imageViewer.IsImageAvailable;
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
                    _undoCommand.Enabled = _imageViewer.IsImageAvailable &&
                        AttributeStatusInfo.UndoManager.UndoOperationAvailable;
                    _redoCommand.Enabled = _imageViewer.IsImageAvailable &&
                        AttributeStatusInfo.UndoManager.RedoOperationAvailable;
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
                    foreach (IAttribute attribute in _paginationAttributesToRefresh
                        .Where(attribute => attribute != null))
                    {
                        AttributeStatusInfo.SetValue(attribute, attribute.Value, true, true);
                        var owningControl = AttributeStatusInfo.GetOwningControl(attribute);
                        if (owningControl != null)
                        {
                            owningControl.RefreshAttributes(true, attribute);
                        }
                    }

                    _paginationAttributesToRefresh.Clear();

                    // https://extract.atlassian.net/browse/ISSUE-14265
                    // If the current image was never opened in the main date entry verification
                    // panel (data tab), we will not be able to select a field.
                    if (_imageOpened)
                    {
                        DataEntryControlHost.EnsureFieldSelection(
                            targetField: FieldSelection.DoNotReset, viaTabKey: false);
                    }

                    // Table controls don't always seem to be drawn correctly after switching tabs.
                    DataEntryControlHost.Refresh();
                }
                else if (_tabControl.SelectedTab != _dataTab && DataEntryControlHost.ImageViewer != null)
                {
                    // These events are handled in the context of data entry verification. If
                    // pagination is active these events should be ignored.
                    _imageViewer.AllowHighlight = false;
                    _imageViewer.ImageFileChanged -= HandleImageFileChanged;
                    _imageViewer.ImageFileClosing -= HandleImageFileClosing;
                    _imageViewer.LoadingNewImage -= HandleLoadingNewImage;
                    // Setting ImageViewer to null will unregister image viewer events we shouldn't
                    // handle while in pagination.
                    DataEntryControlHost.DisableKeyboardInput = true;
                    DataEntryControlHost.ImageViewer = null;

                    _saveAndCommitFileCommand.Enabled = false;
                    _saveMenuItem.Enabled = false;
                    
                    // Skip should always be enabled when on the pagination tab if a document is available
                    _skipProcessingMenuItem.Enabled =
                        !_applicationConfig.Settings.DisableSkip && _paginationPanel.SourceDocuments.Any();

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

                bool sendForReprocessing = false;
                if (e.DocumentData != null && !e.DocumentData.SendForReprocessing != null)
                {
                    sendForReprocessing = e.DocumentData.SendForReprocessing.Value;
                }
                else if (e.PagesEqualButRotated)
                {
                    sendForReprocessing = false;
                }
                else
                {
                    sendForReprocessing = !e.SuggestedPaginationAccepted.HasValue ||
                                          !e.SuggestedPaginationAccepted.Value;
                }

                ExtractException.Assert("ELI41280", "FileTaskSession was not started.",
                    _fileTaskSessionID.HasValue);

                // Add the file to the DB and check it out for this process before actually writing
                // it to outputPath to prevent a running file supplier from grabbing it and another
                // process from getting it.
                var newFileInfo = _paginatedOutputCreationUtility.AddFileWithNameConflictResolve(
                    e.SourcePageInfo, (FAMTagManager)_tagUtility, _fileTaskSessionID.Value,
                    _settings.PaginationSettings.PaginatedOutputPriority);
                e.FileID = newFileInfo.FileID;
                e.OutputFileName = newFileInfo.FileName;

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

                FileProcessingDB.AddPaginationHistory(
                    e.FileID, sourcePageInfo, deletedSourcePageInfo, _fileTaskSessionID.Value);

                var imagePages = e.SourcePageInfo
                    .Where(p => !p.Deleted)
                    .Select(p => new ImagePage(p.DocumentName, p.Page, p.Orientation));

                var newSpatialPageInfos = AttributeMethods.CreateUSSForPaginatedDocument(e.OutputFileName, imagePages);

                // Only grab the file back into the current verification session if suggested
                // pagination boundaries were accepted (meaning the rules should have already found
                // everything we would expect them to find for this document).
                if (sendForReprocessing)
                {
                    // Produce a voa file for the paginated document using the data the rules suggested.
                    var documentData = e.DocumentData as PaginationDocumentData;
                    if (documentData != null && documentData.Attributes != null && documentData.Attributes.Size() > 0)
                    {
                        var copyThis = (ICopyableObject)documentData.Attributes;
                        var attributesCopy = (IUnknownVector)copyThis.Clone();
                        attributesCopy.ReportMemoryUsage();

                        if (e.SourcePageInfo.Any(
                            pageInfo => !pageInfo.Deleted && pageInfo.DocumentName == _fileName))
                        {
                            DataEntryMethods.PruneNonPersistingAttributes(attributesCopy);
                        }

                        AttributeMethods.TranslateAttributesToNewDocument(
                            attributesCopy, e.OutputFileName, imagePages, newSpatialPageInfos);

                        attributesCopy.SaveTo(e.OutputFileName + ".voa", false,
                            _ATTRIBUTE_STORAGE_MANAGER_GUID);
                        attributesCopy.ReportMemoryUsage();
                    }
                }
                else
                {
                    GrabDocumentForVerification(newFileInfo.FileID, e, imagePages, newSpatialPageInfos);                    
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39595");
            }
        }

        /// <summary>
        /// Handles the <see cref="PaginationPanel.AcceptedSourcePagination"/> of the <see cref="_paginationPanel"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="AcceptedSourcePaginationEventArgs"/> instance containing the event data.</param>
        void HandlePaginationPanel_AcceptedSourcePagination(object sender, AcceptedSourcePaginationEventArgs e)
        {
            try
            {
                ExtractException.Assert("ELI47283", "FileTaskSession was not started.", _fileTaskSessionID.HasValue);

                _paginatedOutputCreationUtility.WritePaginationHistory(
                    e.PageInfo, _fileId, _fileTaskSessionID.Value);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI47284");
            }
        }

        /// <summary>
        /// Handles the <see cref="PaginationPanel.OutputDocumentDeleted"/> of the <see cref="_paginationPanel"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="OutputDocumentDeletedEventArgs"/> instance containing the event data.</param>
        void HandlePaginationPanel_OutputDocumentDeleted(object sender, OutputDocumentDeletedEventArgs e)
        {
            try
            {
                ExtractException.Assert("ELI47217", "FileTaskSession was not started.", _fileTaskSessionID.HasValue);

                _paginatedOutputCreationUtility.WritePaginationHistory(
                    e.DeletedPageInfo, -1, _fileTaskSessionID.Value);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI47218");
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

                foreach (var item in e.UnmodifiedPaginationSources
                    .Where(item => item.Value != null))
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
                // If there is an unmodified source (along with modified versions) then the
                // unmodified source document needs to follow the same path as the new documents
                // (and definitely should not be set pending for the source cleanup action)
                if (e.DisregardedPaginationSources.SingleOrDefault() == _fileName
                    || e.UnmodifiedPaginationSources
                        .Select(source => source.Key)
                        .SingleOrDefault() == _fileName)
                {
                    if (!string.IsNullOrWhiteSpace(_settings.PaginationSettings.PaginationOutputAction))
                    {
                        EActionStatus oldStatus;
                        FileProcessingDB.SetStatusForFile(_fileId,
                            _settings.PaginationSettings.PaginationOutputAction, -1,
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
                        _settings.PaginationSettings.PaginationSourceAction, -1,
                        EActionStatus.kActionPending, false, true, out oldStatus);
                }

                // In order for files to be completed (and any prompting or other UI tasks
                // accomplished by the DEP), the data tab must be given back focus.
                _tabControl.SelectedTab = _dataTab;

                _paginationPanel.RemoveSourceFile(_fileName, acceptingPagination: true);

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
                _paginationAttributesToRefresh.Add(e.ModifiedAttribute);

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

        /// <summary>
        /// Sets a newly created file to pending for the PaginationOutputAction, if one is specified
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="CreatingOutputDocumentEventArgs"/> instance containing the
        /// FileID of the newly created file.</param>
        void HandlePaginationPanel_OutputDocumentCreated(object sender, CreatingOutputDocumentEventArgs e)
        {
            try
            {
                bool sendForReprocessing = false;
                if (e.DocumentData != null && !e.DocumentData.SendForReprocessing != null)
                {
                    sendForReprocessing = e.DocumentData.SendForReprocessing.Value;
                }
                else if (e.PagesEqualButRotated)
                {
                    sendForReprocessing = false;
                }
                else
                {
                    sendForReprocessing = !e.SuggestedPaginationAccepted.HasValue ||
                                          !e.SuggestedPaginationAccepted.Value;
                }

                if (sendForReprocessing
                    && !string.IsNullOrWhiteSpace(_settings.PaginationSettings.PaginationOutputAction))
                {
                    FileProcessingDB.SetStatusForFile(e.FileID,
                        _settings.PaginationSettings.PaginationOutputAction,
                        -1, // Current workflow
                        EActionStatus.kActionPending,
                        vbAllowQueuedStatusOverride: false,
                        vbQueueChangeIfProcessing: false,
                        poldStatus: out var _);
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI43307");
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
                return DataEntryControlHost != null && DataEntryControlHost.Dirty;
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
                return ((_fileId > 0) ? new[] { _fileId } : new int[0])
                    .ToList().AsReadOnly();
            }
        }

        /// <summary>
        /// <c>true</c> would indicated this application is running the in the background; 
        /// This form is visible to user, visible so return <c>false</c>.
        /// </summary>
        public bool RunningInBackground
        {
            get
            {
                return false;
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
                if (ActiveDataEntryConfig == null ||
                        DataEntryControlHost == null ||
                        _applicationConfig.Settings.PreventSave)
                {
                    return false;
                }
                else
                {
                    bool saved = DataEntryControlHost.SaveData(validateData);

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
                        ex.ExtractDisplay("ELI37508");
                    }
                }));

                return;
            }

            try
            {
                ExtractException.Assert("ELI37451", "Invalid operation.",
                    !_standAloneMode && _fileProcessingDb != null);

                ExtractException.Assert("ELI44741", "Cannot delay file that is not open.",
                    fileId == -1 || fileId == _fileId);

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

        #endregion IDataEntryApplication Members

        #region Private Members

        /// <summary>
        /// Gets the active <see cref="DataEntryConfiguration"/>.
        /// </summary>
        /// <value>
        /// The active <see cref="DataEntryConfiguration"/>.
        /// </value>
        DataEntryConfiguration ActiveDataEntryConfig
        {
            get
            {
                return _configManager?.ActiveDataEntryConfiguration;
            }
        }

        /// <summary>
        /// Gets the <see cref="DataEntryControlHost"/>.
        /// </summary>
        /// <value>
        /// The <see cref="DataEntryControlHost"/>.
        /// </value>
        DataEntryControlHost DataEntryControlHost
        {
            get
            {
                return ActiveDataEntryConfig?.DataEntryControlHost;
            }
        }

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
        /// <param name="fileId">The ID of the file that completed.</param>
        /// <param name="fileProcessingResult">Specifies under what circumstances
        /// verification of the file completed.</param>
        void OnFileComplete(int fileId, EFileProcessingResult fileProcessingResult)
        {
            var eventHandler = FileComplete;
            if (eventHandler != null)
            {
                eventHandler(this, new FileCompleteEventArgs(fileId, fileProcessingResult));
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
                    return;
                }

                // https://extract.atlassian.net/browse/ISSUE-14495
                // If the save+commit button is not disabled immediately, another SaveAndCommit
                // can be triggered before the first one is complete (BeginInvokes allow for
                // processing another event in the midst of the initial handling).
                _saveAndCommitFileCommand.Enabled = false;

                // AttemptSave will return Cancel if there was invalid data in the DEP.
                if (AttemptSave(true) == DialogResult.Cancel)
                {
                    // If the same attempt was cancelled, the save and commit button needs to be
                    // re-enabled. Otherwise, it will be re-enabled when the next file is loaded.
                    _saveAndCommitFileCommand.Enabled = true;
                }
                else
                {
                    ExtractException.Assert("ELI30677",
                        "No controls are loaded from which to save data.",
                        DataEntryControlHost != null);

                    // If running in FAM mode, close the document until the next one is loaded so it
                    // is clear that the last document has been committed.

                    // Since data has been saved, prevent any other attempts that might be triggered
                    // by events raised during this process.
                    PreventSaveOfDirtyData = true;

                    if (_paginationPanel != null)
                    {
                        _paginationPanel.RemoveSourceFile(_fileName, acceptingPagination: true);
                    }

                    _imageViewer.CloseImage();

                    if (_fileProcessingDb != null)
                    {
                        // Record statistics to database that need to happen when a file is closed.
                        RecordCounts(onLoad: false,
                            attributes: DataEntryControlHost.MostRecentlySavedAttributes);
                        EndFileTaskSession();
                    }

                    OnFileComplete(_fileId, EFileProcessingResult.kProcessingSuccessful);

                    _fileId = -1;
                    _fileName = null;
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
            if (ActiveDataEntryConfig == null ||
                DataEntryControlHost == null ||
                _applicationConfig.Settings.PreventSave)
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

                    // User as selected to discard changes; prevent additional save attempts that
                    // will re-prompt.
                    // https://extract.atlassian.net/browse/ISSUE-14904
                    PreventSaveOfDirtyData = true;
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

            if (_imageViewer.IsImageAvailable && (commitData || DataEntryControlHost.Dirty))
            {
                if (commitData)
                {
                    // [DataEntry:805]
                    // commitData flag determines if data will be validated on save.
                    // Turn off commitData if an applied tag matches the SkipValidationIfDocTaggedAs
                    // setting (and save without prompting)
                    if (!_standAloneMode && _fileProcessingDb != null && !string.IsNullOrEmpty(
                            ActiveDataEntryConfig.Config.Settings.SkipValidationIfDocTaggedAs))
                    {
                        // [DataEntry:1294]
                        // Support multiple tags to allow validation to be skipped delimited by
                        // a comma or semi-colon.
                        IEnumerable<string> tagsToBeSkipped =
                            ActiveDataEntryConfig.Config.Settings.SkipValidationIfDocTaggedAs
                                .Split(',', ';')
                                .Select(tagName => tagName.Trim())
                                .Where(tagName => !string.IsNullOrWhiteSpace(tagName))
                                .ToList();

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
                        DataEntryControlHost.Dirty = false;
                    }
                }

                // If committing data or the user elected to save, attempt the save.
                if (response == DialogResult.Yes && !SaveData(commitData))
                {
                    // Return cancel if the data in the DEP failed validation.
                    response = DialogResult.Cancel;
                }
            }

            if (response == DialogResult.Cancel)
            {
                // Ensure that if the operation that triggered the same attempt has been cancelled
                // that PreventSaveOfDirtyData is reset.
                PreventSaveOfDirtyData = false;
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
                    _paginationPanel.RemoveSourceFile(_fileName, acceptingPagination: false);
                }

                _imageViewer.CloseImage();

                // Record statistics to database that need to happen when a file is closed.
                // Null check because ReorderAccordingToPagination may triggered an abort of processing of a document
                // before a FileTaskSession was started.
                if (_fileTaskSessionID != null)
                {
                    RecordCounts(onLoad: false, attributes: null);
                }
                EndFileTaskSession();

                OnFileComplete(_fileId, processingResult);

                _fileId = -1;
                _fileName = null;
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
                            _paginationPanel.RemoveSourceFile(_fileName, acceptingPagination: false);
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
        /// Adjusts UI elements to reflect the specified configuration.
        /// </summary>
        /// <param name="config">The <see cref="DataEntryConfiguration"/> the UI should reflect.
        /// </param>
        void SetUIConfiguration(DataEntryConfiguration config)
        {
            // Change the text on certain controls if not running in stand alone mode
            if (!_standAloneMode && IsHandleCreated)
            {
                bool enableSave = !_applicationConfig.Settings.PreventSave;

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
                    AttributeStatusInfo.PerformanceTesting = _applicationConfig.Settings.PerformanceTesting;
                }

                _saveAndCommitFileCommand.Enabled = (config != null && _isLoaded);
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
            _toggleShowAllHighlightsMenuItem.CheckState =
                _showAllHighlights ? CheckState.Checked : CheckState.Unchecked;

            OnShowAllHighlightsChanged();
        }

        /// <summary>
        /// Toggles whether the view is auto-zoomed to the current selection.
        /// </summary>
        void ToggleAutoZoom()
        {
            if (DataEntryControlHost != null)
            {
                DataEntryControlHost.ToggleZoomToSelection();
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

                    _imageWindowShortcutsMessageFilter.MessageHandled +=
                            HandleMessageFilterMessageHandled;

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

                _imageWindowShortcutsMessageFilter.MessageHandled -=
                    HandleMessageFilterMessageHandled;


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
        /// Create a new FileTaskSession table row and starts a timer for the session.
        /// </summary>
        void StartFileTaskSession()
        {
            try
            {
                _recordedDatabaseData = false;

                _fileTaskSessionID = FileProcessingDB.StartFileTaskSession(_VERIFICATION_TASK_GUID, _fileId, _actionId);

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
                    _inputEventTracker.StartActivityTimer(_inputActivityTimeout);// Change timeout to get it from dbinfo table
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
                // https://extract.atlassian.net/browse/ISSUE-15686
                // ReorderAccordingToPagination may triggered an abort of processing of a document
                // before a FileTaskSession was started.
                if (CurrentlyRecordingStatistics && _fileTaskSessionID != null)
                {
                    // Don't count input when a document is not open.
                    if (_inputEventTracker != null)
                    {
                        _inputEventTracker.UnregisterControl(this);
                    }

                    double elapsedSeconds = _fileProcessingStopwatch.ElapsedMilliseconds / 1000.0;
                    _fileProcessingStopwatch.Restart();

                    double activityTime = _inputEventTracker?.StopActivityTimer() ?? 0.0;

                    _fileProcessingDb.EndFileTaskSession(
                        _fileTaskSessionID.Value, elapsedSeconds, _overheadElapsedTime.Value, activityTime);
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
                        "Cannot record counts; there is not an active FileTaskSession.",
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
            if (DataEntryControlHost == null && _scrollPanel.Controls.Count > 0)
            {
                _scrollPanel.Controls.Clear();
            }
            else if (DataEntryControlHost != null && (_scrollPanel.Controls.Count == 0 || 
                        !_scrollPanel.Controls.Contains(DataEntryControlHost)))
            {
                if (_scrollPanel.Controls.Count > 0)
                {
                    _scrollPanel.Controls.Clear();
                }

                Control mainDataControl = (Control)_dataTab ?? DataEntryControlHost;

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

                DataEntryControlHost.Anchor = AnchorStyles.Left | AnchorStyles.Top |
                    AnchorStyles.Right;

                int horizontalPadding =
                    (2 * _DATA_ENTRY_PANEL_PADDING) + _scrollPanel.AutoScrollMargin.Width;
                if (_dataTab != null)
                {
                    horizontalPadding += _tabControl.Margin.Horizontal;
                    horizontalPadding += _dataTab.Margin.Horizontal + _dataTab.Padding.Horizontal;
                }

                int minPanel1Width = DataEntryControlHost.MinimumSize.Width;
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
                    DataEntryControlHost.Width = _dataTab.ClientRectangle.Width;
                    _scrollPanel.Width = _dataTab.ClientRectangle.Width;
                }

                // Add the DEP to an auto-scroll pane to allow scrolling if the DEP is too
                // long. (The scroll pane is sized to allow the full width of the DEP to 
                // display initially) 
                _scrollPanel.Controls.Add(DataEntryControlHost);
            }
        }

        /// <summary>
        /// Validates that the action settings for pagination are valid.
        /// </summary>
        void ValidatePaginationActions()
        {
            if (!string.IsNullOrWhiteSpace(_settings.PaginationSettings.PaginationSourceAction))
            {
                int sourceActionID = -1;

                try
                {
                    sourceActionID = FileProcessingDB.GetActionID(
                        _settings.PaginationSettings.PaginationSourceAction);
                }
                catch (Exception)
                {
                    var ee = new ExtractException("ELI44807", "Action for pagination sources is not valid.");
                    ee.AddDebugData("Action", _settings.PaginationSettings.PaginationSourceAction, false);
                    throw ee;
                }

                ExtractException.Assert("ELI49988",
                    "Cannot set pagination sources back to pending in same action",
                    sourceActionID != _actionId);
            }

            if (!string.IsNullOrWhiteSpace(_settings.PaginationSettings.PaginationOutputAction))
            {
                int outputActionID = -1;

                try
                {
                    outputActionID = FileProcessingDB.GetActionID(
                        _settings.PaginationSettings.PaginationOutputAction);
                }
                catch (Exception)
                {
                    var ee = new ExtractException("ELI44808", "Action for paginated output is not valid.");
                    ee.AddDebugData("Action", _settings.PaginationSettings.PaginationOutputAction, false);
                    throw ee;
                }

                ExtractException.Assert("ELI49989",
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
                _configManager.DefaultDataEntryConfiguration.Config.Settings.DataEntryPanelFileName);

            // May be null if the DEP assembly does not define an IPaginationDocumentDataPanel.
            _paginationDocumentDataPanel =
                UtilityMethods.CreateTypeFromAssembly<IPaginationDocumentDataPanel>(
                    dataEntryPanelFileName);
            _paginationPanel.DocumentDataPanel = _paginationDocumentDataPanel;

            if (_paginationDocumentDataPanel != null)
            {
                // Allows config file to be able to update _paginationDocumentDataPanel.
                _configManager.DefaultDataEntryConfiguration.Config.ApplyObjectSettings(
                    _paginationDocumentDataPanel.PanelControl);
            }

            _paginationPanel.DocumentDataRequest += HandlePaginationPanel_DocumentDataRequest;
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
        /// <param name="fileID">File ID</param>
        void LoadDocumentForPagination(string fileName, int fileID)
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
                var rootAttributeNames = new HashSet<string>(
                    attributeArray.Select(attribute => attribute.Name),
                    StringComparer.OrdinalIgnoreCase);

                // If only "Document" attributes exist at the root of the VOA file, there is
                // rules-suggested pagination.
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

                        var viewedPages =
                            UtilityMethods.GetPageNumbersFromString(
                                documentAttribute.SubAttributes
                                .ToIEnumerable<IAttribute>()
                                .Where(attribute => attribute.Name.Equals(
                                    "ViewedPages", StringComparison.OrdinalIgnoreCase))
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
                            fileName, fileID, -1, pages, deletedPages, viewedPages,
                            suggestedPagination.Value, documentData, false);
                    }

                    return;
                }

                // There was a VOA file, just not with suggested pagination. Pass on the VOA data.
                _paginationPanel.LoadFile(fileName, fileID, -1, null, null, null, false, documentData, false);
                return;
            }

            // If there was no rules-suggested pagination, go ahead and load the physical document
            // into the _paginationPanel
            _paginationPanel.LoadFile(fileName, fileID, -1, false);
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
            return (_paginationDocumentDataPanel == null)
                ? new PaginationDocumentData(attributes, _fileName)
                : _paginationDocumentDataPanel.GetDocumentData(
                    attributes, _fileName, FileProcessingDB, _imageViewer);
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
                attributes.LoadFrom(dataFilename, false);
                attributes.UpdateSourceDocNameOfAttributes(fileName);
            }

            // The below code handles https://extract.atlassian.net/browse/ISSUE-16323
            if(attributes.Size() == 1)
            {
                var firstAttribute = attributes.ToIEnumerable<IAttribute>().First();
                if (firstAttribute.Name.Equals("Document"))
                {
                    attributes = firstAttribute
                        .SubAttributes
                        .ToIEnumerable<IAttribute>()
                        .SingleOrDefault(y => y.Name.Equals("DocumentData"))
                        ?.SubAttributes
                        ?? new IUnknownVector();
                }
            }

            return attributes;
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
        /// <param name="imagePages">Represents the <see cref="ImagePage"/>s of the old document for
        /// each successive page of the <see paramref="newDocumentName"/>.</param>
        /// <param name="newSpatialPageInfos">The new spatial page infos to be associated with produced VOA.</param>
        void GrabDocumentForVerification(int fileID, CreatingOutputDocumentEventArgs e,
            IEnumerable<ImagePage> imagePages, LongToObjectMap newSpatialPageInfos)
        {
            try
            {
                // Produce a voa file for the paginated document using the data the rules suggested.
                var documentData = e.DocumentData as PaginationDocumentData;
                if (documentData != null && documentData.Attributes != null && documentData.Attributes.Size() > 0)
                {
                    var copyThis = (ICopyableObject)documentData.Attributes;
                    var attributesCopy = (IUnknownVector)copyThis.Clone();
                    attributesCopy.ReportMemoryUsage();

                    AttributeMethods.TranslateAttributesToNewDocument(
                        attributesCopy, e.OutputFileName, imagePages, newSpatialPageInfos);

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
