using ADODB;
using Extract.Drawing;
using Extract.FileActionManager.Forms;
using Extract.FileActionManager.Utilities.Properties;
using Extract.Imaging;
using Extract.Imaging.Forms;
using Extract.Interop;
using Extract.Licensing;
using Extract.ReportViewer;
using Extract.Utilities;
using Extract.Utilities.Forms;
using Extract.Utilities.Parsers;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TD.SandDock;
using UCLID_AFCORELib;
using UCLID_AFUTILSLib;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;
using UCLID_RASTERANDOCRMGMTLib;

namespace Extract.FileActionManager.Utilities
{
    #region Enums

    /// <summary>
    /// Represents the various methods a limited subset can be selected from a larger pool of files.
    /// </summary>
    public enum SubsetType
    {
        /// <summary>
        /// The subset will start at the beginning of the overall set of files.
        /// </summary>
        Top = 0,

        /// <summary>
        /// The subset will come from the end of the overall set of files.
        /// </summary>
        Bottom = 1,

        /// <summary>
        /// The subset will be a random selection from the overall set of files.
        /// </summary>
        Random = 2
    }

    #endregion Enums

    /// <summary>
    /// A <see cref="Form"/> that allows searching and inspection of files in a File Action Manager database
    /// based on database conditions, OCR content and data content.
    /// </summary>
    public partial class FAMFileInspectorForm : Form
    {
        #region Constants

        /// <summary>
        /// The name of this object.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(FAMFileInspectorForm).ToString();

        /// <summary>
        /// The visible title of this application.
        /// </summary>
        static readonly String _APPLICATION_TITLE = "FAM File Inspector";

        /// <summary>
        /// The license string for the SandDock manager
        /// </summary>
        static readonly string _SANDDOCK_LICENSE_STRING = @"1970|siE7SnF/jzINQg1AOTIaCXLlouA=";

        /// <summary>
        /// The full path to the file that contains information about persisting the 
        /// <see cref="FAMFileInspectorForm"/>.
        /// </summary>
        static readonly string _FORM_PERSISTENCE_FILE = FileSystemMethods.PathCombine(
            FileSystemMethods.UserApplicationDataPath, "FAMFileInspector", "FAMFileInspector.xml");

        /// <summary>
        /// Name for the mutex used to serialize persistence of the control and form layout.
        /// </summary>
        static readonly string _FORM_PERSISTENCE_MUTEX_STRING =
            "24440334-DE0C-46C1-920F-45D064A10DBF";

        /// <summary>
        /// The column from <see cref="_fileListDataGridView"/> that represents whether the file is
        /// flagged.
        /// </summary>
        internal static int _FILE_LIST_FLAG_COLUMN_INDEX = 0;

        /// <summary>
        /// The column from <see cref="_fileListDataGridView"/> that represents the results of the
        /// most recent search.
        /// </summary>
        internal static int _FILE_LIST_MATCH_COLUMN_INDEX = 3;

        /// <summary>
        /// The color of highlights to show found search terms in documents.
        /// </summary>
        static readonly Color _HIGHLIGHT_COLOR = Color.LimeGreen;

        /// <summary>
        /// The color of the dashed border used to indicate the currently selected match.
        /// </summary>
        static readonly Color _SELECTION_BORDER_COLOR = Color.Red;

        /// <summary>
        /// The maximum number of threads that should be used for searching
        /// </summary>
        static int _MAX_THREADS_FOR_DATA_SEARCH = 4;

        /// <summary>
        /// The default <see cref="FileFilter"/> to use when displaying the content of a directory
        /// rather than the contents of a database.
        /// </summary>
        static readonly string _DEFAULT_FILE_FILTER =
            "*.bmp;*.rle;*.dib;*.rst;*.gp4;*.mil;*.cal;*.cg4;*.flc;*.fli;*.gif;*.jpg;*.jpeg;" +
            "*.pcx;*.pct;*.png;*.tga;*.tif;*.tiff;*.pdf";

        /// <summary>
        /// The default value for <see cref="MaxFilesToDisplay"/>.
        /// </summary>
        public static readonly int DefaultMaxFilesToDisplay = 5000;

        /// <summary>
        /// The method a limited subset should be selected from the overall set of files.
        /// </summary>
        public static readonly SubsetType DefaultSubsetType = SubsetType.Top;

        #endregion Constants

        #region Enums

        /// <summary>
        /// Represents the way in which the specified search terms will be used to determine
        /// matching files.
        /// </summary>
        enum SearchModifier
        {
            /// <summary>
            /// A file will be a match if any search term is found.
            /// </summary>
            Any = 0,

            /// <summary>
            /// A file will be a match if all search terms are found.
            /// </summary>
            All = 1,

            /// <summary>
            /// A file will be a match if none of the search terms are found.
            /// </summary>
            None = 2
        }

        /// <summary>
        /// Indicates which type of search is to be performed.
        /// </summary>
        enum SearchType
        {
            /// <summary>
            /// The OCR text of the files should be searched.
            /// </summary>
            Text = 0,

            /// <summary>
            /// The extracted <see cref="IAttribute"/> data of the files should be searched.
            /// </summary>
            Data = 1
        }

        #endregion Enums

        #region Structs

        /// <summary>
        /// Represents an entry from the FAM DB's FileHandler table
        /// </summary>
        struct FileHandlerItem
        {
            /// <summary>
            /// Gets or sets the name the application should be presented to the user as.
            /// </summary>
            /// <value>
            /// The name the application should be presented to the user as.
            /// </value>
            public string Name
            {
                get;
                set;
            }

            /// <summary>
            /// Gets or sets the full to the executable to run.
            /// </summary>
            /// <value>
            /// The full to the executable to run.
            /// </value>
            public string ApplicationPath
            {
                get;
                set;
            }

            /// <summary>
            /// Gets or sets the command-line arguments to use for the application. The
            /// SourceDocName path tag and path functions are supported.
            /// </summary>
            /// <value>
            /// The command-line arguments to use for the application.
            /// </value>
            public string Arguments
            {
                get;
                set;
            }

            /// <summary>
            /// Gets or sets a value indicating whether the application item should be available for
            /// multiple files at once.
            /// </summary>
            /// <value><see langword="true"/> if the application item should be available for
            /// multiple files at once; <see langword="false"/> if the application item should be
            /// allowed for only one file at a time.
            /// </value>
            public bool AllowMultipleFiles
            {
                get;
                set;
            }

            /// <summary>
            /// Gets or sets a value indicating whether the application supports the /ef
            /// command-line parameter.
            /// </summary>
            /// <value><see langword="true"/> if the application supports the /ef command-line
            /// parameter; otherwise, <see langword="false"/>.
            /// </value>
            public bool SupportsErrorHandling
            {
                get;
                set;
            }

            /// <summary>
            /// Gets or sets a value indicating whether the launched application should block until
            /// complete.
            /// </summary>
            /// <value><see langword="true"/> if application should block until
            /// complete; <see langword="false"/> if the application run in the background without
            /// blocking.</value>
            public bool Blocking
            {
                get;
                set;
            }

            /// <summary>
            /// Gets or sets the workflow.
            /// </summary>
            /// <value>
            /// The workflow.
            /// </value>
            public string Workflow
            {
                get;
                set;
            }
        }

        #endregion Structs

        #region Delegates

        /// <summary>
        /// Implements a search operation that occurs on a background thread via
        /// <see cref="StartBackgroundOperation"/>.
        /// </summary>
        /// <typeparam name="T">The data type used to represent the search terms for the operation.
        /// </typeparam>
        /// <param name="searchTerms">The search terms for the operation.</param>
        /// <param name="searchModifier">The <see cref="SearchModifier"/> that indicate how the
        /// <see paramref="searchTerms"/> are to be used to determine matching files.</param>
        /// <param name="cancelToken">A <see cref="CancellationToken"/> the operation should check
        /// after searching each file to see if the operation has been canceled.</param>
        delegate void SearchOperation<T>(T searchTerms, SearchModifier searchModifier,
            CancellationToken cancelToken);

        /// <summary>
        /// Indicates whether the <see paramref="row"/> qualifies for a particular need.
        /// </summary>
        /// <param name="row">The <see cref="DataGridViewRow"/> to check for qualification.</param>
        /// <returns><see langword="true"/> if the row qualifies, otherwise,
        /// <see langword="false"/>.</returns>
        delegate bool RowQualifier(DataGridViewRow row);

        #endregion Delegates

        #region Fields

        /// <summary>
        /// The <see cref="FileProcessingDB"/> whose files are being inspected when this
        /// instance is being used to inspect files from a database.
        /// </summary>
        FileProcessingDB _fileProcessingDB;

        /// <summary>
        /// A semicolon delimited list of file extensions that should be displayed when operating on
        /// the contents of <see cref="SourceDirectory"/> rather than <see cref="FileProcessingDB"/>.
        /// </summary>
        string _fileFilter = _DEFAULT_FILE_FILTER;

        /// <summary>
        /// An <see cref="ImageCodecs"/> instance to use to gather data from image files.
        /// </summary>
        ImageCodecs _codecs;

        /// <summary>
        /// Saves/restores window state info
        /// </summary>
        FormStateManager _formStateManager;

        /// <summary>
        /// The number of files currently selected by <see cref="FileSelector"/>. Not all may be
        /// displayed.
        /// </summary>
        volatile int _fileSelectionCount;

        /// <summary>
        /// An <see cref="IAFUtility"/> instance used to query for <see cref="IAttribute"/>s from
        /// VOA (data) files.
        /// </summary>
        IAFUtility _afUtils = new AFUtility();

        /// <summary>
        /// A map of column indices to the <see cref="IFAMFileInspectorColumn"/> at each index.
        /// </summary>
        Dictionary<int, IFAMFileInspectorColumn> _customColumns =
            new Dictionary<int, IFAMFileInspectorColumn>();

        /// <summary>
        /// Associates pre-defined search terms from the database's FieldSearch table with their
        /// associated queries.
        /// </summary>
        Dictionary<string, string> _dataSearchQueries = new Dictionary<string, string>();

        /// <summary>
        /// All custom file handlers that should be available as context menu options in
        /// <see cref="_fileListDataGridView"/> and the <see cref="FileHandlerItem"/> that defines the
        /// option's behavior.
        /// </summary>
        Dictionary<ToolStripMenuItem, FileHandlerItem> _fileHandlerItems;

        /// <summary>
        /// Maps context menu options to a DocumentName based <see cref="ExtractReport"/> for the
        /// currently selected file.
        /// </summary>
        Dictionary<ToolStripMenuItem, ExtractReport> _reportMenuItems;

        /// <summary>
        /// Context menu option that is a parent to all menu items in <see cref="_reportMenuItems"/>.
        /// </summary>
        ToolStripMenuItem _reportMainMenuItem;

        /// <summary>
        /// Context menu option to copy selected file names as text.
        /// </summary>
        ToolStripMenuItem _copyFileNamesMenuItem;

        /// <summary>
        /// Context menu option to copy selected files as files.
        /// </summary>
        ToolStripMenuItem _copyFilesMenuItem;

        /// <summary>
        /// Context menu option to copy selected files and associated data as files.
        /// </summary>
        ToolStripMenuItem _copyFilesAndDataMenuItem;

        /// <summary>
        /// Context menu option to open the file location in windows explorer.
        /// </summary>
        ToolStripMenuItem _openFileLocationMenuItem;

        /// <summary>
        /// Context menu option that allows rows in the file list to be flagged.
        /// </summary>
        ToolStripMenuItem _setFlagMenuItem;

        /// <summary>
        /// Context menu option that allows the flagged rows in the file list to be cleared.
        /// </summary>
        ToolStripMenuItem _clearFlagMenuItem;

        /// <summary>
        /// A map of column indices to the associated context sub-menu for specifying custom column
        /// values.
        /// </summary>
        Dictionary<ToolStripItem, int> _customColumnMenus;

        /// <summary>
        /// A separator to use between custom column context menu options and the rest of the
        /// context menu options.
        /// </summary>
        ToolStripSeparator _customMenuSeparator;

        /// <summary>
        /// A separator between file handler menu options and the rest of the context menu.
        /// </summary>
        ToolStripSeparator _fileHandlerSeparator;

        /// <summary>
        /// Indicates whether a refresh of custom column values is pending.
        /// </summary>
        bool _customColumnRefreshPending;

        /// <summary>
        /// A <see cref="Task"/> that performs database query operations on a background thread.
        /// </summary>
        volatile Task _queryTask;

        /// <summary>
        /// Allows the <see cref="_queryTask"/> to be canceled.
        /// </summary>
        volatile CancellationTokenSource _queryCanceler;

        /// <summary>
        /// Allows background non-blocking file handler operations to be cancelled before processing
        /// any additional files.
        /// </summary>
        CancellationTokenSource _fileHandlerCanceler = new CancellationTokenSource();

        /// <summary>
        /// Tracks how many file handler operations are currently executing.
        /// </summary>
        CountdownEvent _fileHandlerCountdownEvent = new CountdownEvent(0);

        /// <summary>
        /// Allows any overlay text on the image viewer to be canceled.
        /// </summary>
        volatile CancellationTokenSource _overlayTextCanceler;

        /// <summary>
        /// Indicates whether a background database query or file search is active.
        /// </summary>
        volatile bool _operationIsActive = true;

        /// <summary>
        /// Indicates how many calls to <see cref="OpenImageInvoked"/> are pending.
        /// </summary>
        volatile int _pendingOpenImageCount;

        /// <summary>
        /// The point where a <see cref="Control.MouseDown"/> event took place that may be the start
        /// of a drag/drop operation.
        /// </summary>
        Point? _dragDropMouseDownPoint;

        /// <summary>
        /// Indicates that a selection change on mouse down has been temporarily suppressed. This is
        /// to allow drag events to be started without clearing multi-row selection.
        /// </summary>
        bool _suppressedSelectionChange;

        /// <summary>
        /// The <see cref="DataGridViewRow"/>s that were last selected. Use to override selection
        /// changes in certain <see cref="Control.MouseDown"/> events.
        /// </summary>
        DataGridViewRow[] _lastSelectedRows;

        /// <summary>
        /// A map of file ids to the <see cref="DataGridViewRow"/> instance for that file ID.
        /// </summary>
        Dictionary<int, DataGridViewRow> _rowsByFileId = new Dictionary<int, DataGridViewRow>();

        /// <summary>
        /// Indicates whether the option to copy selected file names to be copied as text is
        /// enabled.
        /// </summary>
        bool? _copyFileNamesEnabled;

        /// <summary>
        /// Indicates whether the option to copy selected files to be copied as files is enabled.
        /// </summary>
        bool? _copyFilesEnabled;

        /// <summary>
        /// Indicates whether the option to copy selected files as well as associated data files
        /// to be copied as files is enabled.
        /// </summary>
        bool? _copyFilesAndDataEnabled;

        /// <summary>
        /// Indicates whether the option to open the file location of the selected file in windows
        /// explorer is enabled.
        /// </summary>
        bool? _openFileLocationEnabled;

        /// <summary>
        /// Indicates whether reports are available to be run as a context menu option.
        /// </summary>
        bool? _reportsEnabled;

        /// <summary>
        /// Keeps track of whether ProcessCmdKey is currently being handled to prevent recursion.
        /// </summary>
        bool _processingCmdKey;

        /// <summary>
        /// Indicates whether the search feature is licensed.
        /// </summary>
        bool _searchIsLicensed;

        /// <summary>
        /// Indicates whether the form is in the process of closing.
        /// </summary>
        bool _closing;

        /// <summary>
        /// Indicates if the host is in design mode or not.
        /// </summary>
        readonly bool _inDesignMode;


        /// <summary>
        /// A <see cref="IFFIFileSelectionPane"/> instance that should replace the
        /// standard file selection pane.
        /// </summary>
        private IFFIFileSelectionPane _fileSelectorPane;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes the <see cref="FAMFileInspectorForm"/> class.
        /// </summary>
        // FXCop believes static members are being initialized here.
        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static FAMFileInspectorForm()
        {
            try
            {
                SearchModifier.Any.SetReadableValue("any");
                SearchModifier.All.SetReadableValue("all");
                SearchModifier.None.SetReadableValue("none");

                SearchType.Text.SetReadableValue("words");
                SearchType.Data.SetReadableValue("extracted data");
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35737");
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FAMFileInspectorForm"/> class.
        /// </summary>
        public FAMFileInspectorForm()
        {
            try
            {
                _inDesignMode = (LicenseManager.UsageMode == LicenseUsageMode.Designtime);

                if (_inDesignMode)
                {
                    // Load the license files from folder
                    LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());
                }

                LicenseUtilities.ValidateLicense(LicenseIdName.FileActionManagerObjects,
                    "ELI35711", _OBJECT_NAME);

                _searchIsLicensed = LicenseUtilities.IsLicensed(LicenseIdName.FileInspectorSearch);

                // License SandDock before creating the form.
                SandDockManager.ActivateProduct(_SANDDOCK_LICENSE_STRING);

                // Do not initialize FileProcessingDB here. A value of null will be used to indicate
                // that the FFI is not operating in database mode.
                MaxFilesToDisplay = DefaultMaxFilesToDisplay;
                SubsetType = DefaultSubsetType;
                FileSelector = new FAMFileSelector();
                
                InitializeComponent();

                // Turn off the tab stop on the page navigation text box.
                foreach (var textBox in _navigationToolsImageViewerToolStrip.Items
                    .OfType<ToolStripTextBox>())
                {
                    textBox.TextBox.TabStop = false;
                }

                if (!_inDesignMode)
                {
                    // Loads/save UI state properties
                    _formStateManager = new FormStateManager(this, _FORM_PERSISTENCE_FILE,
                        _FORM_PERSISTENCE_MUTEX_STRING, _sandDockManager, null);
                }

                _searchModifierComboBox.InitializeWithReadableEnum<SearchModifier>(false);
                _searchTypeComboBox.InitializeWithReadableEnum<SearchType>(false);

                LayerObject.SelectionPen = ExtractPens.GetThickDashedPen(_SELECTION_BORDER_COLOR);

                // This prevents the "missing image" icon from showing up for rows that aren't flagged.
                _fileListDataGridView.Columns[_FILE_LIST_FLAG_COLUMN_INDEX].DefaultCellStyle
                    .NullValue = null;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35712");
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets whether this instance should display the files in a
        /// <see cref="FileProcessingDB"/>.
        /// </summary>
        /// <value><see langword="true"/> if this instance is to display the files in a database;
        /// <see langword="false"/> if it is to display the contents of a directory or file list.
        /// </value>
        public bool UseDatabaseMode
        {
            get
            {
                return FileProcessingDB != null;
            }

            set
            {
                try 
	            {	        
		            if (value != UseDatabaseMode)
                    {
                        // The existence of FileProcessingDB is what indicates if the application is
                        // being run in database mode.
                        if (value)
                        {
                            FileProcessingDB = new FileProcessingDB();
                        }
                        else
                        {
                            FileProcessingDB.CloseAllDBConnections();
                            FileProcessingDB = null;
                        }

                        // If the form had already been created, ensure all mode-related settings
                        // have been reset.
                        if (IsHandleCreated)
                        {
                            ResetFileSelectionSettings();
                            ResetSearch();
                            InitializeContextMenu();
                            GenerateFileList(false);
                        }
                    }
	            }
	            catch (Exception ex)
	            {
		            throw ex.AsExtract("ELI36783");
	            }
            }
        }

        /// <summary>
        /// Gets the custom columns currently present in the FFI.
        /// </summary>
        [CLSCompliant(false)]
        public IEnumerable<IFAMFileInspectorColumn> CustomColumns
        {
            get
            {
                return _customColumns.Values;
            }
        }

        /// <summary>
        /// Gets the <see cref="FileProcessingDB"/> whose files are being inspected when this
        /// instance is being used to inspect files from a database. Will be <see langword="null"/>
        /// if the instance is being used to inspect files from a directory or file list.
        /// </summary>
        /// <value>
        /// The <see cref="FileProcessingDB"/> whose files are being inspected.
        /// </value>
        [CLSCompliant(false)]
        public FileProcessingDB FileProcessingDB
        {
            get
            {
                return _fileProcessingDB;
            }

            private set
            {
                if (value != _fileProcessingDB)
                {
                    _fileProcessingDB = value;

                    // Ensure any custom columns are aware of the new DB.
                    foreach (IFAMFileInspectorColumn column in _customColumns.Values)
                    {
                        column.FileProcessingDB = value;
                    }
                }
            }
        }

        /// <summary>
        /// The <see cref="IFAMFileSelector"/> used to specify the domain of files being inspected.
        /// </summary>
        [CLSCompliant(false)]
        public IFAMFileSelector FileSelector
        {
            get;
            set;
        }

        /// <summary>
        /// Indicates if the provided <see cref="FileSelector"/> should not be changeable in the
        /// FFI.
        /// </summary>
        public bool LockFileSelector
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the summary message that should override the automatic filter summary
        /// message.
        /// <para><b>Note</b></para>
        /// Valid only when <see cref="LockFileSelector"/> is <see langword="true"/>.
        /// filter.
        /// </summary>
        /// <value>
        /// The summary message that should override the automatic filter summary message.
        /// </value>
        public string LockedFileSelectionSummary
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a <see cref="IFFIFileSelectionPane"/> instance that should replace the
        /// standard file selection pane.
        /// </summary>
        /// <value>
        /// The <see cref="IFFIFileSelectionPane"/> instance that should replace the standard file
        /// selection pane.
        /// </value>
        [CLSCompliant(false)]
        public IFFIFileSelectionPane FileSelectorPane
        { 
            get
            {
                return _fileSelectorPane;
            }
            set
            {
                _fileSelectorPane = value;
                if (_fileSelectorPane != null)
                {
                    _fileSelectorPane.AcceptFunction = (row, args) => HandleOkButton_Click(row, args);
                }
            }
        }

        /// <summary>
        /// Gets or sets the name database server being used.
        /// </summary>
        /// <value>
        /// The name of the database server being used.
        /// </value>
        public string DatabaseServer
        {
            get
            {
                return (FileProcessingDB == null) ? "" : FileProcessingDB.DatabaseServer;
            }

            set
            {
                try
                {
                    if (!string.IsNullOrWhiteSpace(value) && FileProcessingDB == null)
                    {
                        FileProcessingDB = new FileProcessingDB();
                    }

                    if (FileProcessingDB != null)
                    {
                        FileProcessingDB.DatabaseServer = value;
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI36784");
                }
            }
        }

        /// <summary>
        /// Gets or sets the name of the database being inspected.
        /// </summary>
        /// <value>
        /// The name of the database being inspected.
        /// </value>
        public string DatabaseName
        {
            get
            {
                return (FileProcessingDB == null) ? "" : FileProcessingDB.DatabaseName;
            }

            set
            {
                try
                {
                    if (!string.IsNullOrWhiteSpace(value) && FileProcessingDB == null)
                    {
                        FileProcessingDB = new FileProcessingDB();
                    }

                    if (FileProcessingDB != null)
                    {
                        FileProcessingDB.DatabaseName = value;
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI36785");
                }
            }
        }


        /// <summary>
        /// Gets or sets the name of the workflow being inspected.
        /// </summary>
        /// <value>
        /// The name of the workflow being inspected.
        /// </value>
        public string WorkflowName
        {
            get
            {
                return FileProcessingDB?.ActiveWorkflow ?? "";
            }

            set
            {
                try
                {
                    if (!string.IsNullOrWhiteSpace(value) && FileProcessingDB == null)
                    {
                        FileProcessingDB = new FileProcessingDB();
                    }

                    if (FileProcessingDB != null)
                    {
                        FileProcessingDB.ActiveWorkflow = value;
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI43369");
                }
            }
        }

        /// <summary>
        /// Gets or sets the directory being inspected by this instance. Used when
        /// <see cref="FileProcessingDB"/> is <see langword="null"/>.
        /// </summary>
        /// <value>
        /// The directory being inspected by this instance.
        /// </value>
        public string SourceDirectory
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets whether files in subdirectories of <see cref="SourceDirectory"/> are to be
        /// included.
        /// </summary>
        /// <value><see langword="true"/> if files in subdirectories of
        /// <see cref="SourceDirectory"/> are to be included; otherwise, <see langword="false"/>.
        /// </value>
        public bool Recursive
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a semicolon delimited list of file extensions that should be displayed when
        /// inspecting the contents of <see cref="SourceDirectory"/> rather than
        /// <see cref="FileProcessingDB"/>.
        /// </summary>
        /// <value>
        /// A semicolon delimited list of file extensions that should be displayed when inspecting
        /// the contents of <see cref="SourceDirectory"/> rather than <see cref="FileProcessingDB"/>.
        /// </value>
        public string FileFilter
        {
            get
            {
                return _fileFilter;
            }

            set
            {
                _fileFilter = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum number of files to display at once.
        /// </summary>
        /// <value>
        /// The maximum number of files to display at once.
        /// </value>
        public int MaxFilesToDisplay
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the method a limited subset should be selected from the overall set of
        /// files.
        /// </summary>
        /// <value>
        /// The <see cref="T:SubsetType"/> specifying the method a limited subset should be
        /// selected from the overall set of files.
        /// </value>
        public SubsetType SubsetType
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the name of the file that contains a list of files to display
        /// </summary>
        public string FileListFileName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the path tag expression that determines which associated attribute
        /// data file will be used for each file
        /// https://extract.atlassian.net/browse/ISSUE-12702
        /// </summary>
        public string VOAPathExpression
        {
            get;
            set;
        } = "<SourceDocName>.voa";

        #endregion Properties

        #region Methods

        /// <summary>
        /// Adds the specified <see paremref="column"/> to the file list table in the FFI.
        /// </summary>
        /// <param name="column">An <see cref="IFAMFileInspectorColumn"/> defining the column.
        /// </param>
        [CLSCompliant(false)]
        public void AddCustomColumn(IFAMFileInspectorColumn column)
        {
            try
            {
                // This could be changed in the future, but currently the IFAMFileInspectorColumn
                // interface uses file ids to get and set values.
                ExtractException.Assert("ELI37428", "Custom FFI columns can only be used when " +
                    "the FFI is running against a FAM database.", UseDatabaseMode);

                // Custom columns will be inserted just before the folder column (which is
                // auto-fill).
                int nextCustomColumnIndex = (_customColumns.Count == 0)
                    ? _fileListDataGridView.ColumnCount - 1
                    : _customColumns.Keys.Max() + 1;

                _customColumns[nextCustomColumnIndex] = column;
                column.FileProcessingDB = FileProcessingDB;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI37429");
            }
        }

        /// <summary>
        /// Shows a dialog allowing the user to select which database to log into.
        /// </summary>
        /// <param name="requireAdminLogOn"><see langword="true"/> if admin credentials are to be
        /// required to connect to the database; otherwise, <see langword="false"/>.</param>
        /// <returns><see langword="true"/> if the user logged into a selected database;
        /// otherwise, <see langword="false"/>.</returns>
        public bool ShowSelectDB(bool requireAdminLogOn)
        {
            try
            {
                if (FileProcessingDB == null)
                {
                    FileProcessingDB = new FileProcessingDB();
                }

                return FileProcessingDB.ShowSelectDB("Select database", false, requireAdminLogOn);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI36786");
            }
        }

        /// <summary>
        /// Resets all changes to file selection back to the default (no conditions, top 5000 files).
        /// </summary>
        public void ResetFileSelectionSettings()
        {
            try
            {
                FileSelector.Reset();
                FileSelector.LimitToSubset(SubsetType == SubsetType.Random,
                    SubsetType == SubsetType.Top, false, MaxFilesToDisplay, -1);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35768");
            }
        }

        /// <summary>
        /// Resets the search settings back to the default and clears all matches indicated in
        /// <see cref="_fileListDataGridView"/>.
        /// </summary>
        public void ResetSearch()
        {
            try
            {
                ClearSearchResults();

                _searchModifierComboBox.SelectEnumValue(SearchModifier.Any);
                _textSearchTermsDataGridView.Rows.Clear();
                _dataSearchTermsDataGridView.Rows.Clear();
                _caseSensitiveSearchCheckBox.Checked = false;
                _regexSearchCheckBox.Checked = false;
                _fuzzySearchCheckBox.Checked = false;

                // Populate all pre-defined search terms from the database's FieldSearch table.
                using (var queryResults = GetResultsForQuery(
                    "SELECT [FieldName], [AttributeQuery] FROM [FieldSearch] WHERE [Enabled] = 1 " +
                    "ORDER BY [FieldName]"))
                {
                    foreach (var row in queryResults.Rows.OfType<DataRow>())
                    {
                        string fieldName = (string)row["FieldName"];
                        string attributeQuery = (string)row["AttributeQuery"];

                        _dataSearchQueries[fieldName] = attributeQuery;

                        int index = _dataSearchTermsDataGridView.Rows.Add(fieldName, "");
                        _dataSearchTermsDataGridView.Rows[index].Cells[0].ReadOnly = true;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35772");
            }
        }

        /// <summary>
        /// Generates the displayed file list based on the <see cref="FileSelector"/>'s current
        /// settings.
        /// </summary>
        /// <param name="preserveFlags"><see langword="true"/> if the state of the flag column of
        /// files should be maintained after the refresh; <see langword="false"/> if the flag value
        /// should be reset for all rows after the reset.</param>
        [SuppressMessage("Microsoft.Naming", "CA1726:UsePreferredTerms", MessageId = "Flags")]
        public void GenerateFileList(bool preserveFlags)
        {
            try
            {
                // Ensure any previous background operation is canceled first.
                CancelBackgroundOperation();

                if (UseDatabaseMode && !FileProcessingDB.IsConnected)
                {
                    FileProcessingDB.ResetDBConnection(false, false);
                }

                // After connecting, make sure the context menu options match whether the
                // connection is now in admin mode.
                InitializeContextMenu();

                UpdateFileSelectionSummary();

                // [DotNetRCAndUtils:1095]
                // If specified, preserve any flags so that they are not cleared when the file list
                // is refreshed.
                Dictionary<string, FAMFileData> preservedData = preserveFlags
                    ?  _fileListDataGridView.Rows
                        .OfType<DataGridViewRow>()
                        .Select(row => row.GetFileData())
                        .ToDictionary(data => data.FileName)
                    : null;

                _fileListDataGridView.Rows.Clear();
                _fileSelectionCount = 0;

                // If generating a new file list, the previous search results don't apply anymore.
                // Uncheck show search results until a new search is run.
                _showOnlyMatchesCheckBox.Checked = false;
                _showOnlyMatchesCheckBox.Enabled = false;

                if (UseDatabaseMode)
                {
                    string query;
                    if (FileSelectorPane != null)
                    {
                        query = "SELECT [ID], [FileName], [Pages]" +
                            "   FROM [FAMFile]" +
                            (FileSelectorPane.SelectedFileIds.Any()
                                ? " WHERE [ID] IN (" + string.Join(",", FileSelectorPane.SelectedFileIds) + ")"
                                : " WHERE 1 = 0");
                    }
                    else
                    {
                        query = FileSelector.BuildQuery(FileProcessingDB,
                            "[FAMFile].[ID], [FAMFile].[FileName], [FAMFile].[Pages]",
                            " ORDER BY [FAMFile].[ID]", false);
                    }

                    // Run the query on a background thread so the UI remains responsive as rows are
                    // loaded.
                    StartBackgroundOperation(() =>
                        RunDatabaseQuery(query, preservedData, _queryCanceler.Token));
                }
                else if (!string.IsNullOrWhiteSpace(SourceDirectory))
                {
                    var fileFilter = string.IsNullOrWhiteSpace(FileFilter)
                        ? null
                        : new Interfaces.FileFilter(null, FileFilter, false);

                    var fileEnumerable = Directory.EnumerateFiles(SourceDirectory, "*",
                        Recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly)
                        .Where(fileName => fileFilter == null || fileFilter.FileMatchesFilter(fileName));

                    // Run the enumeration on a background thread so the UI remains responsive as
                    // rows are loaded.
                    StartBackgroundOperation(() =>
                        RunFileEnumeration(fileEnumerable, preservedData, _queryCanceler.Token));
                }
                else
                {
                    CommentedTextFileReader listOfFiles = new CommentedTextFileReader(FileListFileName);
                    // Run the enumeration on a background thread so the UI remains responsive as
                    // rows are loaded.
                    StartBackgroundOperation(() =>
                        RunFileEnumeration(listOfFiles.AsEnumerable(), preservedData, _queryCanceler.Token));
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35792");
            }
        }

        /// <summary>
        /// Initializes the context menu of the file list based on the current
        /// <see cref="FileProcessingDB"/>'s FileHandler table.
        /// </summary>
        public void InitializeContextMenu()
        {
            try
            {
                // Dispose of the previous context menu.
                DisposeContextMenu();

                // Re-create new context menu items.
                _customColumnMenus = new Dictionary<ToolStripItem, int>();
                _fileHandlerItems = new Dictionary<ToolStripMenuItem, FileHandlerItem>();
                _copyFileNamesMenuItem = new ToolStripMenuItem("Copy filename(s)");
                _copyFilesMenuItem = new ToolStripMenuItem("Copy file(s)");
                _copyFilesAndDataMenuItem = new ToolStripMenuItem("Copy file(s) and data");
                _openFileLocationMenuItem = new ToolStripMenuItem("Open file location");
                _reportMenuItems = new Dictionary<ToolStripMenuItem, ExtractReport>();
                _reportMainMenuItem = new ToolStripMenuItem("Reports");
                _setFlagMenuItem = new ToolStripMenuItem("Set flag");
                _clearFlagMenuItem = new ToolStripMenuItem("Clear flag");

                var newContextMenuStrip = new ContextMenuStrip();

                if (!BasicMenuOptionsOnly)
                {
                    AddCustomFileHandlerOptions(newContextMenuStrip);
                }

                // Add feature menu options
                var featureMenuItems = new List<ToolStripItem>();

                CheckAvailableFeatures();

                if (CopyFileNamesEnabled)
                {
                    featureMenuItems.Add(_copyFileNamesMenuItem);
                    _copyFileNamesMenuItem.Click += HandleCopyFileNames;
                    _copyFileNamesMenuItem.ShortcutKeyDisplayString = "Ctrl + C";
                }

                if (CopyFilesEnabled)
                {
                    featureMenuItems.Add(_copyFilesMenuItem);
                    _copyFilesMenuItem.Click += HandleCopyFiles;
                }

                if (CopyFilesAndDataEnabled)
                {
                    featureMenuItems.Add(_copyFilesAndDataMenuItem);
                    _copyFilesAndDataMenuItem.Click += HandleCopyFilesAndData;
                }

                if (OpenFileLocationEnabled)
                {
                    featureMenuItems.Add(_openFileLocationMenuItem);
                    _openFileLocationMenuItem.Click += HandleOpenFileLocation;
                }

                if (featureMenuItems.Count > 0)
                {
                    if (newContextMenuStrip.Items.Count > 0)
                    {
                        _fileHandlerSeparator = new ToolStripSeparator();
                        newContextMenuStrip.Items.Add(_fileHandlerSeparator);
                    }

                    newContextMenuStrip.Items.AddRange(featureMenuItems.ToArray());
                }

                // Add report menu items.
                if (ReportsEnabled)
                {
                    _reportMenuItems = CreateReportMenuItems();
                    if (_reportMenuItems.Count > 0)
                    {
                        newContextMenuStrip.Items.Add(new ToolStripSeparator());
                        _reportMainMenuItem.DropDownItems.AddRange(_reportMenuItems.Keys.ToArray());
                        newContextMenuStrip.Items.Add(_reportMainMenuItem);
                    }
                }

                // Add set/clear flag menu options
                if (newContextMenuStrip.Items.Count > 0)
                {
                    newContextMenuStrip.Items.Add(new ToolStripSeparator());
                }

                newContextMenuStrip.Items.Add(_setFlagMenuItem);
                newContextMenuStrip.Items.Add(_clearFlagMenuItem);

                _setFlagMenuItem.Click += HandleSetFlagMenuItem_Click;
                _clearFlagMenuItem.Click += HandleClearFlagMenuItem_Click;

                // Add cancel menu option.
                newContextMenuStrip.Items.Add(new ToolStripSeparator());
                newContextMenuStrip.Items.Add(new ToolStripMenuItem("Cancel"));

                // Adds any context menu options to be able to set value(s) in custom columns for
                // the currently selected row(s).
                AddCustomColumnMenus(newContextMenuStrip);

                _fileListDataGridView.ContextMenuStrip = newContextMenuStrip;

                // Handle the opening of the context menu so that the available options can be
                // enabled/disabled appropriately.
                newContextMenuStrip.Opening += HandleContextMenuStrip_Opening;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35821");
            }
        }

        /// <summary>
        /// Gets information about the specified <see paramref="fileId"/>.
        /// </summary>
        /// <param name="fileId">The file id.</param>
        /// <param name="fileName">The filename of the file.</param>
        /// <param name="pageCount">The page count of the file.</param>
        [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "1#")]
        [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "2#")]        
        public void GetFileInfo(int fileId, out string fileName, out int pageCount)
        {
            try
            {
                var fileData = _rowsByFileId[fileId].GetFileData();
                fileName = fileData.FileName;
                pageCount = fileData.PageCount;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI37554");
            }
        }

        #endregion Methods

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
                // Establish image viewer connections prior to calling base.OnLoad which will
                // potentially remove some IImageViewerControls.
                _imageViewer.EstablishConnections(this);

                base.OnLoad(e);

                // https://extract.atlassian.net/browse/ISSUE-12159
                // Temporary hack to fix situation where the thumbnail pane would obscure the image
                // viewer on load.
                if (!_thumbnailDockableWindow.Collapsed)
                {
                    this.SafeBeginInvoke("ELI36818", () =>
                        {
                            _thumbnailViewerToolStripButton.PerformClick();
                            _thumbnailViewerToolStripButton.PerformClick();
                        }, false);
                }

                // If not using a FAM DB, the admin mode and logout options are not applicable.
                if (FileProcessingDB == null)
                {
                    _databaseToolStripMenuItem.Text = "File";
                    _adminModeToolStripMenuItem.Visible = false;
                    _logoutToolStripMenuItem.Visible = false;
                    _databaseMenuToolStripSeparator.Visible = false;
                }

                // Search capability is available only as a separately licensed feature.
                // If search is not licensed hide the pane and remove the control to display it.
                if (!_searchIsLicensed)
                {
                    _resultsSplitContainer.Panel1Collapsed = true;
                    _resultsTableLayoutPanel.RowCount = 1;
                    _resultsTableLayoutPanel.Controls.Remove(_collapsedSearchPanel);
                    _resultsTableLayoutPanel.RowStyles[0] = new RowStyle(SizeType.AutoSize);
                }
                // Ensure the pane used to allow the user to open the search pane is not displayed
                // if the search pane is visible.
                else if (!_inDesignMode &&  SearchPaneVisible && 
                    (_resultsTableLayoutPanel.RowCount == 2))
                {
                    _resultsTableLayoutPanel.RowCount = 1;
                    _resultsTableLayoutPanel.Controls.Remove(_collapsedSearchPanel);
                    _resultsTableLayoutPanel.RowStyles[0] = new RowStyle(SizeType.AutoSize);
                }

                _imageViewer.Shortcuts[Keys.F3] = _nextLayerObjectToolStripButton.PerformClick;
                _imageViewer.Shortcuts[Keys.Shift | Keys.F3] =
                    _previousLayerObjectToolStripButton.PerformClick;

                // The OK and Cancel buttons should be present only in the case that there is at
                // least one custom control that implements IFFIDataManager.
                if (!CustomDataManagers.Any())
                {
                    _mainLayoutPanel.Controls.Remove(_okCancelPanel);
                    _mainLayoutPanel.RowCount = 1;
                    _okCancelPanel.Dispose();
                }

                if (BasicMenuOptionsOnly)
                {
                    _menuStrip.Visible = false;
                }

                InitializeCustomColumns();

                // Initialize the search settings.
                _searchTypeComboBox.SelectEnumValue(SearchType.Text);
                ResetSearch();

                InitializeContextMenu();

                InitializeFileSelectionElements();

                // Set label to reflect the VOA that will be used when doing a data search
                // https://extract.atlassian.net/browse/ISSUE-12702
                _voaPathExpressionLabel.Text = VOAPathExpression;

                GenerateFileList(false);

                EnsureFileListColumnSizes();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35713");
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
            // Prevent recursion via PreProcessControlMessage call below.
            if (_processingCmdKey)
            {
                return false;
            }

            try
            {
                _processingCmdKey = true;

                if (keyData == Keys.Tab || keyData == (Keys.Shift | Keys.Tab))
                {
                    bool forward = keyData == Keys.Tab;

                    // Use the tab key to navigate rows as long as either the file list has focus or
                    // none of the other tab stops do.
                    if (ProcessFileListTabKey(forward))
                    {
                        return true;
                    }

                    // [DotNetRCAndUtils:1005]
                    // There are issues with the "natural" tab navigation of this form. Calculate
                    // what the next tab stop control should be so that we can use it to override
                    // the "natural" behavior.
                    Control lastfocusedControl = this.GetFocusedControl();
                    // If the focused control is a DataGridViewEditingControl, consider the grid as
                    // the focused control and not the editing control.
                    var editingControl = lastfocusedControl as IDataGridViewEditingControl;
                    if (editingControl != null)
                    {
                        lastfocusedControl = editingControl.EditingControlDataGridView;
                    }
                    Control expectedFocusControl =
                        FormsMethods.FindNextControl(this, lastfocusedControl, forward, true,
                        c => c.TabStop && c.Visible && c.Enabled && !(c is ContainerControl));

                    // If after processing the tab key, the focus has moved to another control (i.e.,
                    // it wasn't handled by the control, such as a data grid view), and the control
                    // that now has focus is not the one we expect, override the ActiveControl.
                    this.SafeBeginInvoke("ELI35849", () =>
                    {
                        Control focusedControl = this.GetFocusedControl();
                        if (focusedControl != lastfocusedControl &&
                            focusedControl != editingControl &&
                            focusedControl != expectedFocusControl)
                        {
                            ActiveControl = expectedFocusControl;
                        }
                    }, false);
                }

                // Allow the image viewer to handle keyboard input for shortcuts.
                if (_imageViewer.Shortcuts.ProcessKey(keyData))
                {
                    return true;
                }

                // [DotNetRCAndUtils:1117]
                // To prevent against the Ctrl + C shortcut to copy file names not working when the
                // user expects it to, check to see if the currently focused control is either a
                // TextBox control or wants to handle the Ctrl + C message. If not, treat the
                // shortcut as a copy of filenames from the file list.
                if (CopyFileNamesEnabled && keyData.HasFlag(Keys.C) && keyData.HasFlag(Keys.Control))
                {
                    var focusedControl = this.GetFocusedControl();

                    if (focusedControl != null && !(focusedControl is TextBoxBase))
                    {
                        PreProcessControlState state = focusedControl.PreProcessControlMessage(ref msg);
                        if (state == PreProcessControlState.MessageNotNeeded)
                        {
                            CopySelectedFileNames();
                            return true;
                        }
                    }
                }

                // This key was not processed, bubble it up to the base class.
                return base.ProcessCmdKey(ref msg, keyData);
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI35841", ex);
            }
            finally
            {
                _processingCmdKey = false;
            }

            return true;
        }

        /// <summary>
        /// Sets the control to the specified visible state.
        /// </summary>
        /// <param name="value"><see langword="true"/> to make the control visible; otherwise,
        /// <see langword="false"/>.</param>
        protected override void SetVisibleCore(bool value)
        {
            var autoSizeMode = _fileListDataGridView.ColumnHeadersHeightSizeMode;

            try
            {
                // https://extract.atlassian.net/browse/ISSUE-12964
                // The sequence of events that occurs when displaying the FFI via
                // Extract.DataEntry.LabDE.OrderPicker sometimes results in an
                // InvalidOperationException related to column sizing during layout. The call stack
                // at the time of the exception is completely outside of Extract code. However,
                // since the situation seems to be related to enforcing ColumnHeadersHeightSizeMode,
                // temporarily disable any column header resizing while changing visibility.
                _fileListDataGridView.ColumnHeadersHeightSizeMode =
                    DataGridViewColumnHeadersHeightSizeMode.DisableResizing;

                base.SetVisibleCore(value);

                _fileListDataGridView.ColumnHeadersHeightSizeMode = autoSizeMode;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI38210");
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Form.Closing"/> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.ComponentModel.CancelEventArgs"/> that contains
        /// the event data.</param>
        protected override void OnClosing(CancelEventArgs e)
        {
            try
            {
                // Check to see if any file handler operation is still running.
                if (!_fileHandlerCountdownEvent.Wait(0))
                {
                    if (DialogResult.OK == MessageBox.Show(
                        "One or more operations are still running.\r\n\r\n" +
                        "Stop the operation(s) before the next file is processed " +
                        "and close the application?",
                        "Stop operation?", MessageBoxButtons.OKCancel,
                        MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2, 0))
                    {
                        _fileHandlerCanceler.Cancel();
                        try
                        {
                            // While waiting for the background process(es) to stop, display a modal
                            // message box.
                            ShowMessageBoxWhileBlocking("Waiting for operation(s) to stop...",
                                () => _fileHandlerCountdownEvent.Wait());
                        }
                        catch { }
                    }
                    else
                    {
                        // Cancel the close to keep the background operation running.
                        e.Cancel = true;
                    }
                }

                // Ensure any modified data from custom columns is applied or explicitly disregarded
                // by the user before allowing the form to close.
                if (!PromptToApplyCustomChanges(true))
                {
                    e.Cancel = true;
                }

                if (!e.Cancel)
                {
                    _closing = true;

                    CancelBackgroundOperation();

                    CancelCustomData();
                }

                base.OnClosing(e);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35864");
            }
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> if managed resources should be disposed;
        /// otherwise, <see langword="false"/>.
        /// </param>
        // _queryCanceler and _overlayTextCanceler are not disposed of per
        // http://stackoverflow.com/questions/6960520/when-to-dispose-cancellationtokensource
        [SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "_queryCanceler")]
        [SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "_overlayTextCanceler")]
        [SuppressMessage("Microsoft.Usage", "CA2213:DisposableFieldsShouldBeDisposed", MessageId = "_fileHandlerCanceler")]
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                try
                {
                    // https://extract.atlassian.net/browse/ISSUE-12527
                    // It seems that disposing while a DataGridView is in edit mode causes an
                    // exception. Ending the edit mode first avoids the exception.
                    if (_dataSearchTermsDataGridView != null &&
                        !_dataSearchTermsDataGridView.IsDisposed &&
                        _dataSearchTermsDataGridView.IsCurrentCellInEditMode)
                    {
                        _dataSearchTermsDataGridView.EndEdit();
                    }

                    if (_textSearchTermsDataGridView != null &&
                        !_textSearchTermsDataGridView.IsDisposed &&
                        _textSearchTermsDataGridView.IsCurrentCellInEditMode)
                    {
                        _textSearchTermsDataGridView.EndEdit();
                    }

                    if (_fileListDataGridView != null &&
                        !_fileListDataGridView.IsDisposed &&
                        _fileListDataGridView.IsCurrentCellInEditMode)
                    {
                        _fileListDataGridView.EndEdit();
                    }

                    // Clean up managed objects
                    if (_formStateManager != null)
                    {
                        _formStateManager.Dispose();
                        _formStateManager = null;
                    }

                    if (_codecs != null)
                    {
                        _codecs.Dispose();
                        _codecs = null;
                    }

                    DisposeContextMenu();

                    if (components != null)
                    {
                        components.Dispose();
                        components = null;
                    }

                    if (_queryTask != null)
                    {
                        _queryTask.Dispose();
                        _queryTask = null;
                    }

                    if (_fileHandlerCountdownEvent != null)
                    {
                        _fileHandlerCountdownEvent.Dispose();
                        _fileHandlerCountdownEvent = null;
                    }
                }
                catch { }
            }

            // Clean up unmanaged objects

            base.Dispose(disposing);
        }

        #endregion Overrides

        #region Event Handlers

        /// <summary>
        /// Handles the <see cref="DockControl.DockSituationChanged"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="DockControl.DockSituationChanged"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="DockControl.DockSituationChanged"/> event.</param>
        void HandleThumbnailDockableWindowDockSituationChanged(object sender, EventArgs e)
        {
            try
            {
                // Don't keep loading thumbnails if _thumbnailDockableWindow is closed.
                // [FlexIDSCore:5015]
                // If the window is collapsed it means it is set to auto-hide. It is likely the user
                // will want to check on them in this configuration, so go ahead and load them.
                _thumbnailViewer.Active = _thumbnailDockableWindow.IsOpen ||
                    _thumbnailDockableWindow.Collapsed;
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI36787", ex);
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event of the <see cref="_selectFilesButton"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleSelectFilesButton_Click(object sender, EventArgs e)
        {
            try
            {
                // Indicates if the user confirmed their selection in the appropriate file
                // selection dialog.
                bool selectionConfirmed = false;

                if (UseDatabaseMode)
                {
                    if (FileSelector.Configure(FileProcessingDB, "Select the files to be listed",
                        "SELECT [Filename] FROM [FAMFile]", false))
                    {
                        selectionConfirmed = true;
                    }
                }
                else if (!string.IsNullOrWhiteSpace(SourceDirectory))
                {
                    using (var directoryForm = new DirectorySelectionForm(this))
                    {
                        if (directoryForm.ShowDialog(this) == DialogResult.OK)
                        {
                            selectionConfirmed = true;
                        }
                    }
                }
                else if (!string.IsNullOrWhiteSpace(FileListFileName))
                {
                    using (var fileListfileNameFrom = new FileListFileNameForm(this))
                    {
                        if (fileListfileNameFrom.ShowDialog(this) == DialogResult.OK)
                        {
                            selectionConfirmed = true;
                        }
                    }
                }

                // If the file selection was confirmed, update the file list and clear any existing
                // search results.
                if (selectionConfirmed)
                {
                    ClearSearchResults();
                    GenerateFileList(false);
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35717");
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event of the <see cref="_refreshFileListButton"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleRefreshFileListButton_Click(object sender, EventArgs e)
        {
            try
            {
                GenerateFileList(true);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI36039");
            }
        }

        /// <summary>
        /// Handles the <see cref="ComboBox.SelectedIndexChanged"/> event of the
        /// <see cref="_searchTypeComboBox"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleSearchTypeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                // Show the appropriate search term data grid view based on the selected search type.
                bool textSearch = _searchTypeComboBox.ToEnumValue<SearchType>() == SearchType.Text;
                _textSearchTermsDataGridView.Visible = textSearch;
                _dataSearchTermsDataGridView.Visible = 
                    _voaPathExpressionLabel.Visible = !textSearch;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35738");
            }
        }

        /// <summary>
        /// Handles the <see cref="DataGridView.CurrentCellChanged"/> event of the
        /// <see cref="_fileListDataGridView"/> control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleResultsDataGridView_CurrentCellChanged(object sender, EventArgs e)
        {
            try 
	        {
                DataGridViewRow row = _fileListDataGridView.CurrentRow;

                UpdateImageViewerDisplay(row);

                // If a combo box cell in a custom column is activated, drop it down right away.
                // Otherwise two clicks are required to drop down the menu (one to give focus, and
                // one to expand the dropdown).
                if (_fileListDataGridView.CurrentCell != null &&
                    !_fileListDataGridView.CurrentCell.OwningColumn.ReadOnly)
                {
                    _fileListDataGridView.BeginEdit(true);

                    var combo =
                        _fileListDataGridView.EditingControl as DataGridViewComboBoxEditingControl;

                    if (combo != null && combo.DropDownStyle == ComboBoxStyle.DropDownList)
                    {
                        combo.DroppedDown = true;
                    }
                }
	        }
	        catch (Exception ex)
	        {
		        ex.ExtractDisplay("ELI35718");
	        }
        }

        /// <summary>
        /// Handles the CellPainting event of the _fileListDataGridView control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.DataGridViewCellPaintingEventArgs"/>
        /// instance containing the event data.</param>
        void HandleResultsDataGridView_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            try
            {
                // If this is the column header for the flag column and the foreground is to be
                // drawn.
                if (e.ColumnIndex == _FILE_LIST_FLAG_COLUMN_INDEX && e.RowIndex == -1 &&
                    e.PaintParts.HasFlag(DataGridViewPaintParts.ContentForeground))
                {
                    // First paint the column header as it would normally be painted except for the
                    // content background which paints the sort order arrow that would overlap with
                    // the column header flag image.
                    var paintParts = e.PaintParts & ~DataGridViewPaintParts.ContentBackground;
                    e.Paint(e.CellBounds, paintParts);

                    // Then draw the flag icon on top.
                    GraphicsUnit pageUnit = e.Graphics.PageUnit;
                    RectangleF bounds = Resources.FlagImage.GetBounds(ref pageUnit);
                    bounds.Offset(e.CellBounds.X + (e.CellBounds.Width - bounds.Width) / 2F,
                                  e.CellBounds.Y + (e.CellBounds.Height - bounds.Height) / 2F);
                    e.Graphics.DrawImage(Resources.FlagImage, bounds);

                    e.Handled = true;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI36040");
            }
        }

        /// <summary>
        /// Handles the <see cref="CheckBox.CheckedChanged"/> event of the
        /// <see cref="_fuzzySearchCheckBox"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleFuzzySearchCheckBox_CheckedChanged(object sender, System.EventArgs e)
        {
            try
            {
                // Fuzzy search is not supported for regular expressions in general.
                if (_fuzzySearchCheckBox.Checked)
                {
                    _regexSearchCheckBox.Checked = false;
                    _regexSearchCheckBox.Enabled = false;
                }
                else
                {
                    _regexSearchCheckBox.Enabled = true;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI36093");
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event of the <see cref="_searchButton"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleSearchButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (OperationIsActive)
                {
                    CancelBackgroundOperation();
                }
                else if (_searchTypeComboBox.ToEnumValue<SearchType>() == SearchType.Text)
                {
                    StartTextSearch();
                }
                else
                {
                    StartDataSearch();
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35719");
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event of the <see cref="_clearButton"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleClearButton_Click(object sender, EventArgs e)
        {
            try
            {
                ResetSearch();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35720");
            }
        }

        /// <summary>
        /// Handles the <see cref="DataGridView.SortCompare"/> event of the
        /// <see cref="_fileListDataGridView"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.DataGridViewSortCompareEventArgs"/>
        /// instance containing the event data.</param>
        void HandleFileListDataGridView_SortCompare(object sender, DataGridViewSortCompareEventArgs e)
        {
            try
            {
                // If sorting based on the "Matches" column use custom sorting which is numerical
                // except for where "(No OCR)" or "(No Data)" is displayed.
                if (e.Column.Index == _FILE_LIST_MATCH_COLUMN_INDEX)
                {
                    e.SortResult = ((FAMFileData)e.CellValue1).CompareTo((FAMFileData)e.CellValue2);
                    e.Handled = true;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35739");
            }
        }

        /// <summary>
        /// Handles the <see cref="DataGridView.CellContentDoubleClick"/> event of the
        /// <see cref="_fileListDataGridView"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.DataGridViewCellEventArgs"/>
        /// instance containing the event data.</param>
        void HandleFileListDataGridView_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                // If a flag cell is double clicked, toggle the flagged status.
                if (e.ColumnIndex == _FILE_LIST_FLAG_COLUMN_INDEX && e.RowIndex != -1 &&
                    _fileListDataGridView.Rows.Count > 0)
                {
                    var row = _fileListDataGridView.Rows[e.RowIndex];

                    SetRowFlag(row, !row.GetFileData().Flagged);
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI36041");
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.KeyDown"/> event of the
        /// <see cref="_fileListDataGridView"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.KeyEventArgs"/> instance containing
        /// the event data.</param>
        void HandleFileListDataGridView_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            try
            {
                if (e.Modifiers == Keys.Control && e.KeyCode == Keys.C)
                {
                    if (CopyFileNamesEnabled)
                    {
                        CopySelectedFileNames();
                    }

                    // Whether or not copy file names is enabled, consider the event handled so that
                    // Ctrl + C doesn't copy test from the grid row using the DataGridView default
                    // handler.
                    e.Handled = true;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI36052");
            }
        }

        /// <summary>
        /// Handles the <see cref="DataGridView.SelectionChanged"/> event of the
        /// <see cref="_fileListDataGridView"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleFileListDataGridView_SelectionChanged(object sender, System.EventArgs e)
        {
            try
            {
                // Keep track of each selection change that occurs when the left mouse button is not
                // down so that selection changes on MouseDown can be "suppressed" in some cases by
                // restoring the previous selection.
                if (!MouseButtons.HasFlag(MouseButtons.Left))
                {
                    _lastSelectedRows = SelectedRows.ToArray();
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI36053");
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.SizeChanged"/> event of the
        /// <see cref="_fileListDataGridView"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleFileListDataGridView_SizeChanged(object sender, System.EventArgs e)
        {
            try
            {
                base.OnSizeChanged(e);

                EnsureFileListColumnSizes();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI37587");
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.MouseDown "/> event of the
        /// <see cref="_fileListDataGridView"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.MouseEventArgs"/> instance
        /// containing the event data.</param>
        void HandleFileListDataGridView_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            try
            {
                // If the left mouse button was pressed, prepare for a possible drag operation.
                // https://extract.atlassian.net/browse/ISSUE-12468
                // Do this regardless of whether CopyFilesEnabled is true because a side effect of
                // activating a drag and drop operation is that it prevents dragging from selecting
                // multiple rows. Actual data will be associate with the drag and drop operation
                // only in the case that CopyFilesEnabled is true.
                if (e.Button.HasFlag(MouseButtons.Left))
                {
                    DataGridView.HitTestInfo hit = _fileListDataGridView.HitTest(e.X, e.Y);
                    
                    // If a table row was clicked
                    if (hit.RowIndex >= 0)
                    {
                        // If the clicked row is already selected and shift or control keys are not
                        // pressed, suppress the MouseDown event until the mouse button is released
                        // to maintain the current selection for any potential drag and drop
                        // operation.
                        if (_lastSelectedRows != null &&
                            _fileListDataGridView.Rows[hit.RowIndex].Selected == true &&
                            !ModifierKeys.HasFlag(Keys.Shift) && !ModifierKeys.HasFlag(Keys.Control))
                        {
                            _suppressedSelectionChange = true;

                            // Invoke selection of the rows in _lastSelectedRows on the message
                            // queue so that immediately after clearing the last selection, it gets
                            // restored (transparent to user).
                            _fileListDataGridView.SafeBeginInvoke("ELI36054", () =>
                            {
                                foreach (var row in _lastSelectedRows)
                                {
                                    row.Selected = true;
                                }
                            });
                        }

                        // Keep track of the mouse-down point for use in determining whether to
                        // start drag/drop operations.
                        _dragDropMouseDownPoint = e.Location;
                        return;
                    }
                }

                // If a row was not clicked, _tableMouseDownPoint can be reset as a drag an drop
                // operation cannot be started from this location.
                _dragDropMouseDownPoint = null;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI36055");
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.MouseUp"/> event of the
        /// <see cref="_fileListDataGridView"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.MouseEventArgs"/> instance
        /// containing the event data.</param>
        void HandleFileListDataGridView_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            try
            {
                // If the left mouse button is being released.
                if (e.Button.HasFlag(MouseButtons.Left))
                {
                    _dragDropMouseDownPoint = null;

                    // If selection change had been suppressed on the corresponding MouseDown, apply
                    // the selection change that would have occurred now.
                    if (_suppressedSelectionChange)
                    {
                        _suppressedSelectionChange = false;

                        System.Windows.Forms.DataGridView.HitTestInfo hit =
                            _fileListDataGridView.HitTest(e.X, e.Y);

                        if (hit.RowIndex >= 0)
                        {
                            _fileListDataGridView.ClearSelection();
                            _fileListDataGridView.Rows[hit.RowIndex].Selected = true;
                        }
                    }

                    // Keep track of the current selection so that selection changes on the next
                    // MouseDown can be suppressed if necessary.
                    _lastSelectedRows = SelectedRows.ToArray();
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI36056");
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.MouseMove"/> event of the
        /// <see cref="_fileListDataGridView"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.MouseEventArgs"/> instance
        /// containing the event data.</param>
        void HandleFileListDataGridView_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            try
            {
                // If the left mouse button is down and the mouse is in a different location than when
                // the mouse button was pressed begin a drag and drop operation.
                if (_dragDropMouseDownPoint != null && e.Location != _dragDropMouseDownPoint.Value)
                {
                    _dragDropMouseDownPoint = null;
                    DataObject dragData = new DataObject();

                    if (CopyFileNamesEnabled)
                    {
                        StringCollection fileCollection = new StringCollection();
                        fileCollection.AddRange(GetSelectedFileNames());
                        dragData.SetFileDropList(fileCollection);

                        DoDragDrop(dragData, DragDropEffects.Copy);
                    }
                    else
                    {
                        DoDragDrop(dragData, DragDropEffects.None);
                    }

                    // If a drag/drop event was started, go ahead and consider the current selection
                    // permanent and eligible to use in the next selection "suppression".
                    _lastSelectedRows = SelectedRows.ToArray();
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI36057");
            }
        }

        /// <summary>
        /// Handles the <see cref="DataGridView.CurrentCellDirtyStateChanged"/> event of the
        /// <see cref="_fileListDataGridView"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleFileListDataGridView_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            try
            {
                // If a combo box with DropDownList style is modified, a new item has been selected.
                // Apply the value right away rather than requiring that the cell lose focus first.
                var comboControl =
                    _fileListDataGridView.EditingControl as DataGridViewComboBoxEditingControl;

                if (comboControl != null && comboControl.DropDownStyle == ComboBoxStyle.DropDownList)
                {
                    _fileListDataGridView.CommitEdit(DataGridViewDataErrorContexts.Commit);
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI37431");
            }
        }

        /// <summary>
        /// Handles the <see cref="DataGridView.CellValueChanged"/> event of the
        /// <see cref="_fileListDataGridView"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.DataGridViewCellEventArgs"/>
        /// instance containing the event data.</param>
        void HandleFileListDataGridView_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                // If a value for a custom column has changed, commit the new value.
                IFAMFileInspectorColumn customColumn = null;
                if (_customColumns.TryGetValue(e.ColumnIndex, out customColumn))
                {
                    if (e.RowIndex != -1)
                    {
                        var row = _fileListDataGridView.Rows[e.RowIndex];
                        var cell = row.Cells[e.ColumnIndex];
                        FAMFileData fileData = row.GetFileData();
                        string stringValue = cell.Value.ToString();

                        customColumn.SetValue(fileData.FileID, stringValue);

                        // When a new column value is applied, the column's implementation may
                        // require that other values be updated as a result. Check for values
                        // that need a refresh.
                        RefreshCustomColumns();
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI37432");
            }
        }

        /// <summary>
        /// Handles the <see cref="DataGridView.EditingControlShowing"/> event of the
        /// <see cref="_fileListDataGridView"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DataGridViewEditingControlShowingEventArgs"/> instance
        /// containing the event data.</param>
        void HandleFileListDataGridView_EditingControlShowing(object sender,
            DataGridViewEditingControlShowingEventArgs e)
        {
            try
            {
                int fileID = -1;

                // Check to see if the editing control is being displayed in a custom column.
                // If so, load the valid value choices for that column.
                string[] valueChoices = null;
                IFAMFileInspectorColumn columnDefinition = null;
                if (_customColumns.TryGetValue(_fileListDataGridView.CurrentCell.ColumnIndex,
                    out columnDefinition))
                {
                    fileID = _fileListDataGridView.CurrentCell.OwningRow.GetFileData().FileID;
                    valueChoices = columnDefinition
                        .GetValueChoices(fileID)
                        .ToIEnumerable<string>()
                        .ToArray();
                }

                // If the column is a combo box column, populate the value choices as the combo
                // box's drop items.
                var comboBox = e.Control as DataGridViewComboBoxEditingControl;
                if (comboBox != null)
                {
                    comboBox.Items.Clear();
                    if (valueChoices != null)
                    {
                        comboBox.Items.AddRange(valueChoices);
                    }
                    // Clearing combo box items will have cleared the value; restore the proper
                    // value.
                    comboBox.Text = columnDefinition.GetValue(fileID);

                    return;
                }

                // Future: Populate the auto-complete list for text box columns.
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI37555");
            }
        }

        /// <summary>
        /// Handles the case that the option to copy selected file names as text was invoked.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleCopyFileNames(object sender, EventArgs e)
        {
            try
            {
                CopySelectedFileNames();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI36049");
            }
        }

        /// <summary>
        /// Handles the case that the option to copy selected files as files was invoked.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleCopyFiles(object sender, EventArgs e)
        {
            try
            {
                CopySelectedFiles();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI36050");
            }
        }

        /// <summary>
        /// Handles the case that the option to copy selected files and associated data to the
        /// clipboard as files was invoked.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void HandleCopyFilesAndData(object sender, EventArgs e)
        {
            try
            {
                CopySelectedFilesAndData();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI36051");
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event of the <see cref="_openFileLocationMenuItem"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleOpenFileLocation(object sender, EventArgs e)
        {
            try
            {
                OpenFileLocation();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI37137");
            }
        }

        /// <summary>
        /// Handles the <see cref="ToolStripItem.Click"/> event of the
        /// <see cref="_setFlagMenuItem"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleSetFlagMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                SetRowFlag(SelectedRows, true);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI36042");
            }
        }

        /// <summary>
        /// Handles the <see cref="ToolStripItem.Click"/> event of the
        /// <see cref="_clearFlagMenuItem"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleClearFlagMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                SetRowFlag(SelectedRows, false);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI36043");
            }
        }

        /// <summary>
        /// Handles the <see cref="CheckBox.CheckedChanged"/> event of the
        /// <see cref="_showOnlyMatchesCheckBox"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleShowOnlyMatchesCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                // Update the visibility of each row based upon the search results and whether the
                // user has selected to see only matching files.
                foreach (DataGridViewRow row in _fileListDataGridView.Rows)
                {
                    FAMFileData fileData = row.GetFileData();
                    row.Visible = fileData.FileMatchesSearch || !_showOnlyMatchesCheckBox.Checked;
                }

                // After re-displaying all files from having been displaying only search results,
                // the last currently active row may still be selected though it is no longer the
                // current row. Clear the selection if this is the case.
                if (_fileListDataGridView.CurrentRow == null)
                {
                    _fileListDataGridView.ClearSelection();
                }

                UpdateImageViewerDisplay(_fileListDataGridView.CurrentRow);

                UpdateStatusLabel();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35740");
            }
        }

        /// <summary>
        /// Handles the <see cref="T:ContextMenuStripItem.Opening"/> event of the
        /// <see cref="_fileListDataGridView"/>'s <see cref="ContextMenuStrip"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.ComponentModel.CancelEventArgs"/> instance
        /// containing the event data.</param>
        void HandleContextMenuStrip_Opening(object sender, CancelEventArgs e)
        {
            try
            {
                // Determine the location where the context menu was opened.
                Point mouseLocation = _fileListDataGridView.PointToClient(MousePosition);
                System.Windows.Forms.DataGridView.HitTestInfo hit =
                    _fileListDataGridView.HitTest(mouseLocation.X, mouseLocation.Y);
                
                // If the click was not on a row, don't present the context menu.
                if (hit.RowIndex < 0)
                {
                    return;
                }

                // [DotNetRCAndUtils:1093]
                // If the row under the right-click is not selected, clear the current selection
                // and select the clicked row instead.
                DataGridViewRow clickedRow = _fileListDataGridView.Rows[hit.RowIndex];
                if (!clickedRow.Selected)
                {
                    _fileListDataGridView.ClearSelection();
                    _fileListDataGridView.CurrentCell = clickedRow.Cells[0];
                }

                int selectionCount = SelectedRows.Count();
                bool areAnyVisible = false;

                // Enable/disable each file handler item in the context menu based on the current
                // selection.
                foreach (KeyValuePair<ToolStripMenuItem, FileHandlerItem> app in _fileHandlerItems)
                {
                    bool isVisible =
                        string.IsNullOrWhiteSpace(app.Value.Workflow) ||
                        app.Value.Workflow.Equals(FileProcessingDB.ActiveWorkflow, StringComparison.OrdinalIgnoreCase);
                    app.Key.Visible = isVisible;
                    areAnyVisible |= isVisible;

                    if (selectionCount == 0)
                    {
                        // If there is no selection, no options should be enabled.
                        app.Key.Enabled = false;
                    }
                    else if (selectionCount > 1 && !app.Value.AllowMultipleFiles)
                    {
                        // Disable if multiple files are selected by the option is valid for only
                        // one.
                        app.Key.Enabled = false;
                    }
                    else
                    {
                        app.Key.Enabled = true;
                    }
                }

                if (_fileHandlerSeparator != null)
                {
                    _fileHandlerSeparator.Visible = areAnyVisible;
                }

                // Open file location should be available for single selection only.
                if (_openFileLocationMenuItem != null)
                {
                    _openFileLocationMenuItem.Enabled = (selectionCount == 1);
                }

                // Report context menu options should be available for single selection only.
                if (_reportMainMenuItem != null)
                {
                    _reportMainMenuItem.Enabled = (selectionCount == 1);
                }

                // Enable/disable custom column value choices based on the current selection.
                UpdateCustomColumnMenuEnabledStates();

                // Enable/disable the set/clear flag options depending on the flag status of the
                // selected rows.
                _setFlagMenuItem.Enabled = SelectedRows.Any(row => !row.GetFileData().Flagged);

                _clearFlagMenuItem.Enabled = SelectedRows.Any(row => row.GetFileData().Flagged);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35822");
            }
        }

        /// <summary>
        /// Handles the <see cref="ToolStripItem.Click"/> event of a
        /// <see cref="ToolStripMenuItem"/> to apply a value to a custom column
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleCustomColumnContextMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                ToolStripMenuItem menuItem = sender as ToolStripMenuItem;

                if (menuItem != null)
                {
                    // All selected rows that are visible should have the text of the selected menu
                    // option applied as the new cell value.
                    int columnIndex = _customColumnMenus[menuItem.OwnerItem];
                    foreach (var row in _fileListDataGridView.SelectedRows
                        .OfType<DataGridViewRow>()
                        .Where(row => row.Visible))
                    {
                        row.Cells[columnIndex].Value = menuItem.Text;
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI37438");
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event of a <see cref="ToolStripMenuItem"/> for
        /// one of the custom file handler context menu options.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleFileHandlerMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                FileHandlerItem fileHandlerItem = _fileHandlerItems[(ToolStripMenuItem)sender];

                // [DotNetRCAndUtils:1064]
                // Flag any rows for which a file handler was run to make it easier for the user
                // to keep track of for which files an action was taken.
                SetRowFlag(SelectedRows, true);

                // Get the selected files to an array so that the file list is a snapshot that won't
                // change after processing has begun.
                string[] fileNames = GetSelectedFileNames();

                if (fileHandlerItem.Blocking)
                {
                    // If blocking, use a modal message box to block rather than calling
                    // RunApplication on this thread; the latter causes the for to report "not
                    // responding" in some circumstances.
                    ShowMessageBoxWhileBlocking("Running " + fileHandlerItem.Name.Quote() + "...",
                        () => RunApplication(fileHandlerItem, fileNames, _fileHandlerCanceler.Token));
                }
                else
                {
                    // If not blocking, run the application on a background thread; allow the UI
                    // thread to continue.
                    try
                    {
                        // Increment the current _fileHandlerCountdownEvent to prevent the form from
                        // closing while the file handler item is still running. (AddCount cannot be
                        // called when the CurrentCount is already zero.)
                        if (!_fileHandlerCountdownEvent.TryAddCount())
                        {
                            _fileHandlerCountdownEvent.Reset(1);
                        }
                        
                        Task.Factory.StartNew(() =>
                            RunApplication(fileHandlerItem, fileNames, _fileHandlerCanceler.Token),
                                _fileHandlerCanceler.Token)
                        .ContinueWith((task) =>
                            {
                                // Regardless of whether there was a failure, indicate that the file
                                // handler process has finished.
                                _fileHandlerCountdownEvent.Signal();

                                // Handle any failure launching the operation to prevent unhandled
                                // exceptions from crashing the application.
                                if (task.IsFaulted)
                                {
                                    Exception[] exceptions = task.Exception.InnerExceptions.ToArray();
                                    this.SafeBeginInvoke("ELI35879", () =>
                                    {
                                        foreach (Exception ex in exceptions)
                                        {
                                            ex.ExtractDisplay("ELI35872");
                                        }
                                    });
                                }
                            });
                    }
                    catch
                    {
                        // In case the file handler task was never started, return
                        // _fileHandlerCountdownEvent to its previous value.
                        _fileHandlerCountdownEvent.Signal();
                        throw;
                    }

                    // Since a non-blocking app will continue to run in the background, use the
                    // status bar to indicate the application is being launched. After 5 seconds,
                    // revert to the normal status message.
                    _searchStatusLabel.Text = "Started " + fileHandlerItem.Name.Quote() + "...";
                    Task.Factory.StartNew(() =>
                    {
                        Thread.Sleep(5000);
                        this.SafeBeginInvoke("ELI35818", () => UpdateStatusLabel());
                    })
                    .ContinueWith((task) =>
                    {
                        // Handle any failure performing the status update to prevent unhandled
                        // exceptions from crashing the application.
                        foreach (Exception ex in task.Exception.InnerExceptions)
                        {
                            ex.ExtractLog("ELI35873");
                        }
                    }, TaskContinuationOptions.OnlyOnFaulted);
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35823");
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event of a <see cref="ToolStripMenuItem"/> for
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleReportMenuItem_Click(object sender, EventArgs e)
        {
            try 
	        {
                using (new TemporaryWaitCursor())
                {
                    ExtractReport report = _reportMenuItems[(ToolStripMenuItem)sender];

                    string fileName = GetSelectedFileNames().Single();
                    report.ParametersCollection["DocumentName"].SetValueFromString(fileName);

                    // If the report takes any parameters in addition to DocumentName, display a
                    // prompt so the user can specify them.
                    bool promptForParameters = report.ParametersCollection.Count > 1;
                    report.Initialize(DatabaseServer, DatabaseName, WorkflowName, promptForParameters);

                    // Show report on another thread so that the report is not modal to the FFI. The
                    // thread needs to be STA
                    ThreadingMethods.RunInBackgroundThread("ELI36059", () =>
                    {
                        using (ReportViewerForm reportViewer = new ReportViewerForm(report))
                        {
                            reportViewer.ShowDialog();
                        }
                    }, true, ApartmentState.STA);
                }
	        }
	        catch (Exception ex)
	        {
		        ex.ExtractDisplay("ELI36060");
	        }
        }

        /// <summary>
        /// If either the file list has focus or none of the other tab stops do, navigates the rows
        /// of <see cref="_fileListDataGridView"/>.
        /// </summary>
        /// <param name="forward"><see langword="true"/> to navigate forward, otherwise,
        /// <see langword="false"/>.</param>
        /// <returns><see langword="true"/> if the current row in <see cref="_fileListDataGridView"/>
        /// was changed, otherwise <see langword="false"/>.</returns>
        bool ProcessFileListTabKey(bool forward)
        {
            try
            {
                // Manually override navigation of the file list if either the file list has focus
                // or none of the other tab stops do.
                Control focusedControl = this.GetFocusedControl();
                bool isFocusTabStopControl = (focusedControl != null) && focusedControl.TabStop &&
                    focusedControl.Visible && focusedControl.Enabled &&
                    !(focusedControl is ContainerControl);
                isFocusTabStopControl |= (focusedControl is IDataGridViewEditingControl);
                if (_fileListDataGridView.Focused || !isFocusTabStopControl)
                {
                    // Reset focus to the file list, if it doesn't already have it.
                    ActiveControl = _fileListDataGridView;

                    // If we can find the next row to be selected, select it.
                    DataGridViewRow nextRow = GetNextRow(forward, null);
                    if (nextRow != null)
                    {
                        _fileListDataGridView.CurrentCell = nextRow.Cells[0];

                        return true;
                    }

                    // The current row (if any) is the last in the current navigation direction. Tab
                    // should now navigate away from the file list. To ensure that it does, make
                    // sure the current cell is the last in the row in the navigation direction.
                    DataGridViewCell currentCell = _fileListDataGridView.CurrentCell;
                    if (currentCell != null)
                    {
                        // If forward, set to last cell in row.
                        int lastColumnIndex = _fileListDataGridView.ColumnCount - 1;
                        if (forward && currentCell.ColumnIndex < lastColumnIndex)
                        {
                            _fileListDataGridView.CurrentCell =
                                _fileListDataGridView.Rows[currentCell.RowIndex].Cells[lastColumnIndex];
                        }
                        // If backward, set to first cell in row.
                        else if (!forward && currentCell.ColumnIndex > 0)
                        {
                            _fileListDataGridView.CurrentCell =
                                _fileListDataGridView.Rows[currentCell.RowIndex].Cells[0];
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35847");
            }

            return false;
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event of the
        /// <see cref="_adminModeToolStripMenuItem"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void HandleAdminModeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Repeatedly show login dialog until successful or the user cancels.
            while (true)
            {
                bool cancelled;
                if (FileProcessingDB.ShowLogin(true, out cancelled))
                {
                    ShowConnectionState();

                    // Checks schema
                    Show();
                    InitializeContextMenu();
                    break;
                }
                else if (cancelled)
                {
                    break;
                }
                else
                {
                    UtilityMethods.ShowMessageBox("The specified credentials were not valid.",
                        "Login failed", true);
                }
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event of the
        /// <see cref="_logoutToolStripMenuItem"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleLogoutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Repeatedly show login dialog until successful or the user exits the app.
            while (true)
            {
                try
                {
                    // Hide the main form until the user connects to a database.
                    Hide();

                    // Explicitly close the previous DB connection to clear the credentials.
                    // Otherwise if reconnecting to the same DB it may be in admin mode though the
                    // has not been re-prompted
                    FileProcessingDB.CloseAllDBConnections();

                    if (FileProcessingDB.ShowSelectDB("Select database", false, false))
                    {
                        // Checks schema
                        FileProcessingDB.ResetDBConnection(true, false);
                        ResetFileSelectionSettings();
                        ResetSearch();
                        Show();
                        InitializeContextMenu();
                        GenerateFileList(false);
                    }
                    else
                    {
                        // If the user chose to exit from the database selection prompt, exit.
                        Close();
                    }

                    break;
                }
                catch (Exception ex)
                {
                    ex.ExtractDisplay("ELI35756");
                }
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event of the <see cref="_exitToolStripMenuItem"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Close();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35753");
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event of the
        /// <see cref="_aboutToolStripMenuItem"/> control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleAboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                AboutBox aboutBox = new AboutBox();
                aboutBox.ShowDialog();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35831");
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event of the
        /// <see cref="_openSearchPaneButton"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleOpenSearchPaneButton_Click(object sender, EventArgs e)
        {
            try
            {
                SearchPaneVisible = true;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI36775");
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event of the
        /// <see cref="_closeSearchPaneButton"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleCloseSearchPaneButton_Click(object sender, EventArgs e)
        {
            try
            {
                SearchPaneVisible = false;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI36776");
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event of the <see cref="_okButton"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleOkButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (PromptToApplyCustomChanges(false))
                {
                    DialogResult = DialogResult.OK;
                    Close();
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI37439");
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event of the <see cref="_cancelButton"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleCancelButton_Click(object sender, EventArgs e)
        {
            try
            {
                Close();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI37440");
            }
        }

        /// <summary>
        /// Handles the <see cref="IFFIFileSelectionPane.RefreshRequired"/> event of the
        /// <see cref="FileSelectorPane"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleFileSelectorPane_RefreshRequired(object sender, EventArgs e)
        {
            try
            {
                GenerateFileList(false);
            }
            catch (Exception ex)
            {
                // Throw here since we know this event is triggered our own UI event handler.
                throw ex.AsExtract("ELI38121");
            }
        }

        #endregion Event Handlers

        #region Private Members

        /// <summary>
        /// Gets a value indicating whether this instance in currently running in admin mode.
        /// </summary>
        /// <value><see langword="true"/> if this instance in currently running in admin mode;
        /// otherwise, <see langword="false"/>.
        /// </value>
        bool InAdminMode
        {
            get
            {
                return FileProcessingDB != null && FileProcessingDB.LoggedInAsAdmin;
            }
        }

        /// <summary>
        /// Gets the codecs used to encode and decode images.
        /// </summary>
        /// <value>The codecs used to encode and decode images.</value>
        ImageCodecs Codecs
        {
            get
            {
                if (_codecs == null)
                {
                    _codecs = new ImageCodecs();
                }

                return _codecs;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the search is pane visible.
        /// </summary>
        /// <value><see langword="true"/> if the search pane visible; otherwise, 
        /// <see langword="false"/>.
        /// </value>
        bool SearchPaneVisible
        {
            get
            {
                return !_resultsSplitContainer.Panel1Collapsed;
            }

            set
            {
                try
                {
                    if (_searchIsLicensed)
                    {
                        _resultsSplitContainer.Panel1Collapsed = !value;

                        // If the visibility of the _collapsedSearchPanel does not coincide with
                        // the current SearchPaneVisible state, display/hide it as appropriate.
                        if (value != (_resultsTableLayoutPanel.RowCount == 1))
                        {
                            if (value)
                            {
                                // Hide _collapsedSearchPanel
                                _resultsTableLayoutPanel.RowCount = 1;
                                _resultsTableLayoutPanel.Controls.Remove(_collapsedSearchPanel);
                                _resultsTableLayoutPanel.RowStyles[0] = new RowStyle(SizeType.AutoSize);
                            }
                            else
                            {
                                // Show _collapsedSearchPanel
                                _resultsTableLayoutPanel.RowCount = 2;
                                _resultsTableLayoutPanel.Controls.Add(_collapsedSearchPanel);
                                _resultsTableLayoutPanel.Controls.SetChildIndex(_collapsedSearchPanel, 0);
                                _resultsTableLayoutPanel.RowStyles[0] = new RowStyle(SizeType.Absolute, 21);
                                _resultsTableLayoutPanel.RowStyles[1] = new RowStyle(SizeType.AutoSize);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI36777");
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether the option to copy selected file names to be copied
        /// as text is enabled.
        /// </summary>
        /// <value><see langword="true"/> if the option to copy selected file names to be copied
        /// as text is enabled; otherwise, <see langword="false"/>.</value>
        bool CopyFileNamesEnabled
        {
            get
            {
                if (!_copyFileNamesEnabled.HasValue)
                {
                    if (FileProcessingDB != null)
                    {
                        _copyFileNamesEnabled =
                            FileProcessingDB.IsFeatureEnabled(ExtractFeatures.FileHandlerCopyNames);
                    }
                    else
                    {
                        _copyFileNamesEnabled = true;
                    }
                }

                return _copyFileNamesEnabled.Value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the option to copy selected files to be copied as
        /// files is enabled.
        /// </summary>
        /// <value><see langword="true"/> if the option to copy selected files to be copied as
        /// files is enabled; otherwise, <see langword="false"/>.
        /// </value>
        bool CopyFilesEnabled
        {
            get
            {
                if (!_copyFilesEnabled.HasValue)
                {
                    if (FileProcessingDB != null)
                    {
                        _copyFilesEnabled =
                            FileProcessingDB.IsFeatureEnabled(ExtractFeatures.FileHandlerCopyFiles);
                    }
                    else
                    {
                        _copyFilesEnabled = true;
                    }
                }

                return _copyFilesEnabled.Value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the option to copy selected files as well as
        /// associated data files to be copied as files is enabled.
        /// </summary>
        /// <value><see langword="true"/> if the option to copy selected files as well as
        /// associated data files to be copied as files is enabled.; otherwise,
        /// <see langword="false"/>.</value>
        bool CopyFilesAndDataEnabled
        {
            get
            {
                if (!_copyFilesAndDataEnabled.HasValue)
                {
                    if (FileProcessingDB != null)
                    {
                        _copyFilesAndDataEnabled =
                            FileProcessingDB.IsFeatureEnabled(ExtractFeatures.FileHandlerCopyFilesAndData);
                    }
                    else
                    {
                        _copyFilesAndDataEnabled = true;
                    }
                }

                return _copyFilesAndDataEnabled.Value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the option to open a file location in Windows file
        /// explorer is enabled.
        /// </summary>
        /// <value><see langword="true"/> if the option to open a file location in Windows file
        /// explorer is enabled.; otherwise, <see langword="false"/>.</value>
        bool OpenFileLocationEnabled
        {
            get
            {
                if (!_openFileLocationEnabled.HasValue)
                {
                    if (FileProcessingDB != null)
                    {
                        _openFileLocationEnabled =
                            FileProcessingDB.IsFeatureEnabled(ExtractFeatures.FileHandlerOpenFileLocation);
                    }
                    else
                    {
                        _openFileLocationEnabled = true;
                    }
                }

                return _openFileLocationEnabled.Value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether reports are available to be run as a context menu
        /// option.
        /// </summary>
        /// <value><see langword="true"/> if reports are available to be run as a context menu
        /// option; otherwise, <see langword="false"/>.</value>
        bool ReportsEnabled
        {
            get
            {
                if (!_reportsEnabled.HasValue)
                {
                    _reportsEnabled = UseDatabaseMode &&
                        FileProcessingDB.IsFeatureEnabled(ExtractFeatures.RunDocumentSpecificReports);
                }

                return _reportsEnabled.Value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether a background database query or file search is
        /// currently active.
        /// </summary>
        /// <value><see langword="true"/> if a background database query or file search is currently
        /// active; otherwise, <see langword="false"/>.
        /// </value>
        bool OperationIsActive
        {
            get
            {
                return _operationIsActive;
            }

            set
            {
                _operationIsActive = value;

                // Update UI elements to reflect the current search state.
                _searchButton.Text = value ? "Cancel" : "Search";
                _selectFilesButton.Enabled = !value;
                _searchModifierComboBox.Enabled = !value;
                _searchTypeComboBox.Enabled = !value;
                _textSearchTermsDataGridView.Enabled = !value;
                _clearButton.Enabled = !value;
                if (!value)
                {
                    _searchProgressBar.Visible = false;
                    _searchErrorStatusStripLabel.Visible = true;
                }
                
                UpdateStatusLabel();
                Update();
            }
        }

        /// <summary>
        /// Gets the currently visible and selected <see cref="DataGridViewRow"/>s from
        /// <see cref="_fileListDataGridView"/>.
        /// </summary>
        // [DotNetRCAndUtils:1114]
        // Hidden rows should not be included in the rows considered selected.
        IEnumerable<DataGridViewRow> SelectedRows
        {
            get
            {
                return _fileListDataGridView.SelectedRows
                    .OfType<DataGridViewRow>()
                    .Where(row => row.Visible);
            }
        }

        /// <summary>
        /// Initializes the configure UI elements for file selection.
        /// </summary>
        void InitializeFileSelectionElements()
        {
            bool openTopDockableWindow = false;

            // _formToolStripContainer = the toolstrip container for the form as a whole
            // _mainToolStripContainer = the toolstrip container for the left pane
            //                           (non-image viewer controls).
            // _customSearchTopDockableWindow = pane across the top that FileSelectorPane can be
            //                           configured to occupy, hidden otherwise.

            if (FileSelectorPane != null)
            {
                _dataSplitContainer.Panel1.Controls.Clear();

                if (FileSelectorPane.PanePosition == SelectionPanePosition.Top)
                {
                    _mainToolStripContainer.TopToolStripPanel.Controls.Remove(_menuStrip);
                    _mainToolStripContainer.TopToolStripPanelVisible = false;
                    _formToolStripContainer.TopToolStripPanel.Visible = true;
                    _formToolStripContainer.TopToolStripPanel.Controls.Add(_menuStrip);

                    _dataSplitContainer.Panel1Collapsed = true;

                    _customSearchTopDockableWindow.Controls.Add(FileSelectorPane.Control);
                    _customSearchTopDockableWindow.PrimaryControl = FileSelectorPane.Control;
                    _customSearchTopDockableWindow.Text = FileSelectorPane.Title;
                    openTopDockableWindow = true;
                }
                else if (FileSelectorPane.PanePosition == SelectionPanePosition.Default)
                {
                    _dataSplitContainer.Panel1.Controls.Add(FileSelectorPane.Control);
                }
                else
                {
                    ExtractException.ThrowLogicException("ELI38165");
                }

                FileSelectorPane.Control.Dock = DockStyle.Fill;
                FileSelectorPane.RefreshRequired += HandleFileSelectorPane_RefreshRequired;
            }
            else
            {
                // Set the visibility of the _selectFilesButton according to LockFileSelector.
                if (LockFileSelector)
                {
                    ExtractException.Assert("ELI37441", "No file selector has been specified.",
                        FileSelector != null);

                    // If the file filter is locked, hide the "Change..." button and move the
                    // refresh button up.
                    _selectFilesButton.Visible = false;
                    _refreshFileListButton.Location = _selectFilesButton.Location;
                }
            }

            if (openTopDockableWindow)
            {
                _customSearchTopDockableWindow.Open();
            }
            else
            {
                // Despite being specified as collapsed in the designer, it seems to default to
                // being displayed unless explicitly closed here.
                _customSearchTopDockableWindow.Close();
            }
        }

        /// <summary>
        /// Gets the currently selected file names.
        /// </summary>
        /// <returns>The currently selected file names.</returns>
        string[] GetSelectedFileNames()
        {
            var fileNames = SelectedRows
                .OrderBy(row => row.Index)
                .Select(row => row.GetFileData().FileName)
                .ToArray();
            return fileNames;
        }

        /// <summary>
        /// Copies the selected file names to the clipboard as text.
        /// </summary>
        /// <param name="filenames">The filenames to place on the clipboard if already known;
        /// otherwise <see langword="null"/> to used the currently selected filenames.</param>
        /// <param name="attempt">The current attempt number. After 3 failed attempts an exception
        /// will be displayed.</param>
        void CopySelectedFileNames(string filenames = null, int attempt = 0)
        {
            filenames = filenames ?? string.Join("\r\n", GetSelectedFileNames()); 

            Clipboard.SetText(filenames);

            if (Clipboard.GetText() != filenames)
            {
                ExtractException.Assert("ELI36183", "Failed to set clipboard text.", attempt < 3);

                // Retry copying the file list to the clipboard in a subsequent message handler
                // after a slight delay.
                Thread.Sleep(100);
                this.SafeBeginInvoke("ELI36182",() => CopySelectedFileNames(filenames, attempt + 1));
            }
        }

        /// <summary>
        /// Copies the selected files to the clipboard as files.
        /// </summary>
        void CopySelectedFiles()
        {
            StringCollection fileCollection = new StringCollection();
            fileCollection.AddRange(GetSelectedFileNames());
            Clipboard.SetFileDropList(fileCollection);
        }

        /// <summary>
        /// Copies the selected files and associated data to the clipboard as files.
        /// </summary>
        void CopySelectedFilesAndData()
        {
            StringCollection fileCollection = new StringCollection();
            fileCollection.AddRange(GetSelectedFileNames()
                .SelectMany(fileName => new[] { fileName }
                    .Concat(Directory.EnumerateFiles(
                        Path.GetDirectoryName(fileName),
                        Path.GetFileName(fileName) + "*")
                        .Where(dataFileName =>
                            dataFileName.StartsWith(fileName + ".", StringComparison.OrdinalIgnoreCase))))
                .ToArray());
            Clipboard.SetFileDropList(fileCollection);
        }

        /// <summary>
        /// Opens the specified file location in windows explorer and selects the file if it exists.
        /// If neither the file nor the directory it is supposed to reside in are found, an error is
        /// displayed.
        /// </summary>
        void OpenFileLocation()
        {
            string fileName = GetSelectedFileNames().Single();
            string argument = null;

            if (File.Exists(fileName))
            {
                argument = "/select," + fileName.Quote();
            }
            else
            {
                string directory = Path.GetDirectoryName(fileName);
                if (Directory.Exists(directory))
                {
                    argument = "/root," + directory.Quote();
                }
                else
                {
                    UtilityMethods.ShowMessageBox(
                        "Neither the file nor its containing directory could be found.",
                        "File not found.", true);
                    return;
                }
            }
            
            using (var process = new Process())
            {
                process.StartInfo = new ProcessStartInfo("explorer.exe", argument);
                process.StartInfo.UseShellExecute = true;
                process.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
                process.Start();
            }
        }

        /// <summary>
        /// Clears results from a previous search.
        /// </summary>
        void ClearSearchResults()
        {
            _showOnlyMatchesCheckBox.Checked = false;
            _showOnlyMatchesCheckBox.Enabled = false;

            foreach (DataGridViewRow row in _fileListDataGridView.Rows)
            {
                row.GetFileData().ClearSearchResults();
            }
            _fileListMatchesColumn.Visible = false;
            _fileListDataGridView.Invalidate();

            if (_imageViewer.IsImageAvailable)
            {
                _imageViewer.LayerObjects.Clear();
            }

            _layerObjectSelectionStatusLabel.Text = "";
            _searchErrorStatusStripLabel.Text = "";
            _imageViewerErrorStripStatusLabel.Text = "";
            UpdateStatusLabel();
        }

        /// <summary>
        /// Updates the status label text to reflect the current state of
        /// <see cref="_fileListDataGridView"/>.
        /// </summary>
        void UpdateStatusLabel()
        {
            if (OperationIsActive)
            {
                _searchStatusLabel.Text = "Searching...";
            }
            else if (_showOnlyMatchesCheckBox.Checked)
            {
                int resultCount = _fileListDataGridView.Rows
                    .OfType<DataGridViewRow>()
                    .Count(row => row.Visible);
                _searchStatusLabel.Text = string.Format(CultureInfo.CurrentCulture,
                    "Showing {0:D} search results", resultCount);
            }
            else
            {
                if (_fileSelectionCount > _fileListDataGridView.Rows.Count)
                {
                    _searchStatusLabel.Text = string.Format(CultureInfo.CurrentCulture,
                        "Showing {0:D} of {1:D} files", _fileListDataGridView.Rows.Count,
                        _fileSelectionCount);
                }
                else
                {
                    _searchStatusLabel.Text = string.Format(CultureInfo.CurrentCulture,
                        "Showing {0:D} files", _fileListDataGridView.Rows.Count);
                }
            }
        }

        /// <summary>
        /// Creates a <see cref="ToolStripMenuItem"/> for every available report with a DocumentName
        /// parameter.
        /// </summary>
        Dictionary<ToolStripMenuItem, ExtractReport> CreateReportMenuItems()
        {
            Dictionary<ToolStripMenuItem, ExtractReport> reportMenuItems =
                new Dictionary<ToolStripMenuItem, ExtractReport>();

            // Search all standard and saved reports
            foreach (string reportFileName in
                Directory.EnumerateFiles(ExtractReport.StandardReportFolder, "*.rpt", SearchOption.AllDirectories)
                .Union(Directory.EnumerateFiles(ExtractReport.SavedReportFolder, "*.rpt", SearchOption.AllDirectories)))
            {
                var report = new ExtractReport(reportFileName);

                // If the report takes a "DocumentName" parameter that is not specified by default,
                // this report is eligible to be available via a context menu option.
                IExtractReportParameter documentNameParameter = null;
                if (report.ParametersCollection.TryGetValue("DocumentName", out documentNameParameter) &&
                    !documentNameParameter.HasValueSet())
                {
                    // Create a context menu option and add a handler for it.
                    string reportName = Path.GetFileNameWithoutExtension(reportFileName);
                    var menuItem = new ToolStripMenuItem(reportName);
                    menuItem.Click += HandleReportMenuItem_Click;

                    reportMenuItems[menuItem] = report;
                }
            }

            return reportMenuItems;
        }

        /// <summary>
        /// Gets the results for the specified query from <see cref="FileProcessingDB"/> if
        /// <see cref="UseDatabaseMode"/> or from an alternate settings database otherwise.
        /// <para><b>NOTE</b></para>
        /// The caller is responsible for disposing of the returned <see cref="DataTable"/>.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>A <see cref="DataTable"/> with the results of the specified query.</returns>
        DataTable GetResultsForQuery(string query)
        {
            DataTable queryResults = new DataTable();
            queryResults.Locale = CultureInfo.CurrentCulture;
            Recordset adoRecordset = null;

            try
            {
                if (UseDatabaseMode)
                {
                    // Populate all pre-defined search terms from the database's FieldSearch table.
                    adoRecordset = FileProcessingDB.GetResultsForQuery(query);

                    // Convert the ADODB Recordset to a DataTable.
                    using (OleDbDataAdapter adapter = new System.Data.OleDb.OleDbDataAdapter())
                    {
                        adapter.Fill(queryResults, adoRecordset);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI36788");
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
        /// Runs a database query to build the file list on a background thread.
        /// </summary>
        /// <param name="query">The query used to generate the file list.</param>
        /// <param name="preservedData">If not <see langword="null"/>, indicates files for which
        /// should be marked as flagged in the newly populated list.</param>
        /// <param name="cancelToken">The <see cref="CancellationToken"/> that should be checked
        /// after adding each file to the list to ensure the operation hasn't been canceled.</param>
        void RunDatabaseQuery(string query, Dictionary<string, FAMFileData> preservedData, CancellationToken cancelToken)
        {
            try
            {
                // If there are any query results, populate _resultsDataGridView.
                using (var queryResults = GetResultsForQuery(query))
                {
                    foreach (var row in queryResults.Rows.OfType<DataRow>())
                    {
                        // Abort if the user cancelled.
                        cancelToken.ThrowIfCancellationRequested();

                        // Populate up to MaxFilesToDisplay in the file list, but iterate all
                        // results to obtain the overall number of files selected.
                        if (_fileSelectionCount < MaxFilesToDisplay)
                        {
                            // Retrieve the fields necessary for the results table.
                            string fileName = (string)row["FileName"];

                            // Retrieve the data for the results table.
                            var fileData = (preservedData != null && preservedData.ContainsKey(fileName))
                                ? preservedData[fileName]
                                : new FAMFileData(fileName, VOAPathExpression);

                            Bitmap flagValue = fileData.Flagged ? Resources.FlagImage : null;

                            string directory = Path.GetDirectoryName(fileName);
                            fileName = Path.GetFileName(fileName);
                            fileData.FileID = (int)row["ID"];
                            fileData.PageCount = (int)row["Pages"];

                            var rowValues = new List<object>(new object[] 
                                { flagValue, fileName, fileData.PageCount, fileData });

                            // Add any the row values for any custom columns.
                            foreach (int index in _customColumns.Keys)
                            {
                                string value = GetCustomColumnValue(fileData.FileID, index);

                                rowValues.Add(value);
                            }

                            // Since the directory column size mode is set to fill, it should be last.
                            rowValues.Add(directory);

                            // Invoke the new row to be added on the UI thread.
                            this.SafeBeginInvoke("ELI35725", () =>
                            {
                                int index = _fileListDataGridView.Rows.Add(rowValues.ToArray());
                                _rowsByFileId[fileData.FileID] = _fileListDataGridView.Rows[index];
                            });
                        }

                        _fileSelectionCount++;
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // If canceled, the user didn't want to wait around for the operation to complete;
                // they don't need to see an exception about the operation being canceled.
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35726");
            }
        }

        /// <summary>
        /// Gets a file's value for a custom column. Ensures in the processes that if the column is
        /// a combo box column that the item is part of the columns list of possible choices so as
        /// to avoid errors when applying the column's value.
        /// </summary>
        /// <param name="fileId">The file ID for which the value is needed.</param>
        /// <param name="columnIndex">The column index for which the value is needed.</param>
        /// <returns>The file's value for the custom column.</returns>
        string GetCustomColumnValue(int fileId, int columnIndex)
        {
            IFAMFileInspectorColumn columnDefinition = _customColumns[columnIndex];
            string value = columnDefinition.GetValue(fileId);

            if (columnDefinition.FFIColumnType == FFIColumnType.Combo)
            {
                var dataGridViewColumn = (DataGridViewComboBoxColumn)
                    _fileListDataGridView.Columns[columnIndex];
                if (!dataGridViewColumn.Items.Contains(value))
                {
                    dataGridViewColumn.Items.Add(value);
                }
            }

            return value;
        }

        /// <summary>
        /// Enumerates the files in <see paramref="fileEnumerable"/> to build the file list on a
        /// background thread.
        /// </summary>
        /// <param name="fileEnumerable">The <see cref="IEnumerable{T}"/> of <see cref="string"/>s
        /// containing the files to be in the file list.</param>
        /// <param name="preservedData">If not <see langword="null"/>, indicates files for which
        /// should be marked as flagged in the newly populated list.</param>
        /// <param name="cancelToken">The <see cref="CancellationToken"/> that should be checked
        /// after adding each file to the list to ensure the operation hasn't been canceled.</param>
        void RunFileEnumeration(IEnumerable<string> fileEnumerable, Dictionary<string, FAMFileData> preservedData,
            CancellationToken cancelToken)
        {
            try
            {
                foreach (var filePath in fileEnumerable)
                {
                    // Abort if the user cancelled.
                    cancelToken.ThrowIfCancellationRequested();

                    // Populate up to MaxFilesToDisplay in the file list, but iterate all
                    // results to obtain the overall number of files selected.
                    if (_fileSelectionCount < MaxFilesToDisplay)
                    {
                        // Retrieve the data for the results table.
                        var fileData = (preservedData != null && preservedData.ContainsKey(filePath))
                            ? preservedData[filePath]
                            : new FAMFileData(filePath, VOAPathExpression);

                        Bitmap flagValue = fileData.Flagged ? Resources.FlagImage : null;

                        string directory = Path.GetDirectoryName(filePath);
                        string fileName = Path.GetFileName(filePath);
                        fileData.PageCount = GetPageCount(filePath);

                        // Invoke the new row to be added on the UI thread.
                        this.SafeBeginInvoke("ELI36789", () =>
                        {
                            _fileListDataGridView.Rows.Add(
                                flagValue, fileName, fileData.PageCount, fileData, directory);
                        });
                    }

                    _fileSelectionCount++;
                }
            }
            catch (OperationCanceledException)
            {
                // If canceled, the user didn't want to wait around for the operation to complete;
                // they don't need to see an exception about the operation being canceled.
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI36790");
            }
        }

        /// <summary>
        /// Starts a text search.
        /// </summary>
        void StartTextSearch()
        {
            var searchTerms = _textSearchTermsDataGridView.Rows
                .OfType<DataGridViewRow>()
                .Select(row => (string)row.Cells[0].Value)
                .Where(term => !string.IsNullOrWhiteSpace(term));
            ExtractException.Assert("ELI35741", "No search terms specified", searchTerms.Count() > 0);

            StartSearch(RunTextSearch, searchTerms);
        }

        /// <summary>
        /// Runs a text search on a background thread.
        /// </summary>
        /// <param name="searchTerms">The terms to search for in each file's OCR data.</param>
        /// <param name="searchModifier">The <see cref="SearchModifier"/> that indicate how the
        /// <see paramref="searchTerms"/> are to be used to determine matching files.</param>
        /// <param name="cancelToken">A <see cref="CancellationToken"/> the operation should check
        /// after searching each file to see if the operation has been canceled.</param>
        void RunTextSearch(IEnumerable<string> searchTerms, SearchModifier searchModifier,
            CancellationToken cancelToken)
        {
            // Create a compiled DotNetRegexParser for every search term using an escaped version of
            // the search term to allow any term to be search as a regular expression.
            var regexParsers = new List<Regex>();
            foreach (string searchTerm in searchTerms)
            {
                DotNetRegexParser regexParser = PrepareSearchRegex(searchTerm);
                regexParsers.Add(regexParser.Regex);
            }

            int notSearchedCount = 0;
            var exceptions = new ConcurrentQueue<ExtractException>();

            // Search each file in the file list.
            var parallelOptions = new ParallelOptions();
            parallelOptions.CancellationToken = cancelToken;
            parallelOptions.MaxDegreeOfParallelism = System.Environment.ProcessorCount;
            Parallel.ForEach(_fileListDataGridView.Rows.OfType<DataGridViewRow>(), parallelOptions, 
                row =>
            {
                // Abort if the user cancelled.
                cancelToken.ThrowIfCancellationRequested();

                FAMFileData rowData = row.GetFileData();

                try
                {
                    // Obtain the OCR text for the file.
                    rowData.ShowTextResults = true;
                    SpatialString ocrText = rowData.OcrText;
                    if (ocrText == null)
                    {
                        Interlocked.Add(ref notSearchedCount, 1);
                    }
                    else
                    {
                        string fileText = ocrText.String;
                        List<Match> allMatches = new List<Match>();

                        // Initialize FileMatchesSearch depending on whether we are looking for any term.
                        rowData.FileMatchesSearch = (searchModifier != SearchModifier.Any);

                        // Search the OCR text with the parser for each search term.
                        foreach (Regex parser in regexParsers)
                        {
                            var matches = parser.Matches(fileText)
                                .OfType<Match>()
                                .Where(match => match.Length > 0);

                            // Update FileMatchesSearch as appropriate given the results
                            switch (searchModifier)
                            {
                                case SearchModifier.Any: rowData.FileMatchesSearch |= matches.Any();
                                    break;
                                case SearchModifier.All: rowData.FileMatchesSearch &= matches.Any();
                                    break;
                                case SearchModifier.None: rowData.FileMatchesSearch &= !matches.Any();
                                    break;
                            }

                            // Compile all the matches regardless of whether the file is a match
                            // overall.
                            allMatches.AddRange(matches);
                        }

                        rowData.TextMatches = allMatches;
                    }
                }
                catch (Exception ex)
                {
                    Interlocked.Add(ref notSearchedCount, 1);
                    rowData.Exception = ex.AsExtract("ELI35852");
                    exceptions.Enqueue(ex.AsExtract("ELI35857"));
                }

                // Use a separate variable in the below, invoked call to update the row in the UI,
                // because by the time that update happens, row may have been re-assigned to another
                // row.
                var rowToUpdate = row;

                // Update the row in the UI.
                this.SafeBeginInvoke("ELI35742", () =>
                {
                    UpdateRowVisibility(rowData, rowToUpdate);

                    // Update the progress bar
                    _searchProgressBar.Value++;
                });
            });

            // [DotNetRCAndUtils:1029]
            // Exceptions should be displayed on the UI thread, blocking it which prevents the form
            // from being closed which can lead to a crash. Using BeginInvoke does not always block
            // the UI thread (not sure why) so use Invoke instead to guarantee the UI thread is
            // blocked.
            this.Invoke((MethodInvoker)(() =>
            {
                int exceptionCount = exceptions.Count;

                if (notSearchedCount == 0)
                {
                    _searchErrorStatusStripLabel.Text = "";
                }
                else
                {
                    _searchErrorStatusStripLabel.Text = string.Format(CultureInfo.CurrentCulture,
                        (exceptionCount == 0) 
                        ? "{0:D} file(s) have not been OCRed"
                        : "{0:D} file(s) could not be searched.", notSearchedCount);
                }

                if (exceptionCount > 0)
                {
                    // Aggregating a large number of exceptions can bog down, potentially
                    // making the quickly making the app appear hung. Aggregate a maximum of 10
                    // exceptions.
                    var exceptionsToAggregate = exceptions.Take(10).Union(new [] {
                        new ExtractException("ELI35853",
                        string.Format(CultureInfo.CurrentCulture,
                        "There were error(s) searching {0:D} file(s).", exceptionCount)) });

                    ExtractException.AsAggregateException(exceptionsToAggregate).Display();
                }

                EnsureFileListColumnSizes();
            }));
        }

        /// <summary>
        /// Starts a search of VOA file <see cref="IAttribute"/> data.
        /// </summary>
        void StartDataSearch()
        {
            // Compile the search terms into a dictionary where the attribute query is the key and
            // the value to search for in the attribute is the value.
            var searchTerms = new Dictionary<string,string>();
            foreach (KeyValuePair<string, string> pair in
                _dataSearchTermsDataGridView.Rows
                    .OfType<DataGridViewRow>()
                    .Select(row => new KeyValuePair<string, string>((string)row.Cells[0].Value, (string)row.Cells[1].Value))
                    .Where(pair => !string.IsNullOrWhiteSpace(pair.Key) &&
                        !string.IsNullOrWhiteSpace(pair.Value)))
            {
                // If this is a pre-defined search term, look up the associated attribute query
                string attributeQuery;
                if (!_dataSearchQueries.TryGetValue(pair.Key, out attributeQuery))
                {
                    // Otherwise treat the term itself as the attribute query.
                    attributeQuery = pair.Key;
                }

                searchTerms.Add(attributeQuery, pair.Value);
            }
            ExtractException.Assert("ELI35743", "No search terms specified", searchTerms.Count() > 0);

            StartSearch(RunDataSearch, searchTerms);
        }

        /// <summary>
        /// Runs a data search.
        /// </summary>
        /// <param name="searchTerms">The search terms as a <see cref="T:Dictionary(string,string)"/>
        /// where the attribute query is the key and the value to search for in the
        /// <see cref="IAttribute"/> is the value.</param>
        /// <param name="searchModifier">The <see cref="SearchModifier"/> that indicate how the
        /// <see paramref="searchTerms"/> are to be used to determine matching files.</param>
        /// <param name="cancelToken">A <see cref="CancellationToken"/> the operation should check
        /// after searching each file to see if the operation has been canceled.</param>
        void RunDataSearch(Dictionary<string,string> searchTerms, SearchModifier searchModifier,
            CancellationToken cancelToken)
        {
            // Create a dictionary for every attribute query where the value is a compiled
            // DotNetRegexParser for the corresponding search term using an escaped version of the
            // search term to allow any term to be search as a regular expression.
            var regexParsers = new Dictionary<string, Regex>();
            foreach (KeyValuePair<string, string> searchTerm in searchTerms)
            {
                DotNetRegexParser regexParser = PrepareSearchRegex(searchTerm.Value);
                regexParsers.Add(searchTerm.Key, regexParser.Regex);
            }

            int notSearchedCount = 0;
            var exceptions = new ConcurrentQueue<ExtractException>();

            // Search each file in the file list.
            var parallelOptions = new ParallelOptions();
            parallelOptions.CancellationToken = cancelToken;
            parallelOptions.MaxDegreeOfParallelism = Math.Min(_MAX_THREADS_FOR_DATA_SEARCH,
                System.Environment.ProcessorCount);
            Parallel.ForEach(_fileListDataGridView.Rows.OfType<DataGridViewRow>(), parallelOptions,
                row =>
            {
                // Abort if the user cancelled.
                cancelToken.ThrowIfCancellationRequested();

                FAMFileData rowData = row.GetFileData();

                try
                {
                    // Obtain the VOA data for the file.
                    rowData.ShowTextResults = false;
                    IUnknownVector attributes = rowData.Attributes;
                    if (attributes == null)
                    {
                        Interlocked.Add(ref notSearchedCount, 1);
                    }
                    else
                    {
                        var allMatches = new List<ThreadSafeSpatialString>();

                        // Initialize FileMatchesSearch depending on whether we are looking for any term.
                        rowData.FileMatchesSearch = (searchModifier != SearchModifier.Any);

                        // Search the specified attributes with the parser for each search term.
                        foreach (KeyValuePair<string, Regex> parser in regexParsers)
                        {
                            IEnumerable<ThreadSafeSpatialString> matches =
                                _afUtils.QueryAttributes(attributes, parser.Key, false)
                                .ToIEnumerable<IAttribute>()
                                .Select(attribute => attribute.Value)
                                .SelectMany(value => parser.Value.Matches(value.String)
                                    .OfType<Match>()
                                    .Where(match => match.Length > 0)
                                    .Select(match => new ThreadSafeSpatialString(this,
                                        value.GetSubString(match.Index, match.Index + match.Length - 1))));

                            // Update FileMatchesSearch as appropriate given the results
                            switch (searchModifier)
                            {
                                case SearchModifier.Any: rowData.FileMatchesSearch |= matches.Any();
                                    break;
                                case SearchModifier.All: rowData.FileMatchesSearch &= matches.Any();
                                    break;
                                case SearchModifier.None: rowData.FileMatchesSearch &= !matches.Any();
                                    break;
                            }

                            // Compile all the matches regardless of whether the file is a match
                            // overall.
                            allMatches.AddRange(matches);
                        }

                        rowData.DataMatches = allMatches;
                    }
                }
                catch (Exception ex)
                {
                    Interlocked.Add(ref notSearchedCount, 1);
                    rowData.Exception = ex.AsExtract("ELI35855");
                    exceptions.Enqueue(ex.AsExtract("ELI35858"));
                }

                // Use a separate variable in the below, invoked call to update the row in the UI,
                // because by the time that update happens, row may have been re-assigned to another
                // row.
                var rowToUpdate = row;

                // Update the row in the UI.
                this.SafeBeginInvoke("ELI35744", () =>
                {
                    UpdateRowVisibility(rowData, rowToUpdate);

                    // Update the progress bar
                    _searchProgressBar.Value++;
                });
            });

            // [DotNetRCAndUtils:1029]
            // Exceptions should be displayed on the UI thread, blocking it which prevents the form
            // from being closed which can lead to a crash. Using BeginInvoke does not always block
            // the UI thread (not sure why) so use Invoke instead to guarantee the UI thread is
            // blocked.
            this.Invoke((MethodInvoker)(() =>
            {
                int exceptionCount = exceptions.Count;

                if (notSearchedCount == 0)
                {
                    _searchErrorStatusStripLabel.Text = "";
                }
                else
                {
                    _searchErrorStatusStripLabel.Text = string.Format(CultureInfo.CurrentCulture,
                        (exceptionCount == 0) 
                        ? "Rules have not been run on {0:D} file(s)"
                        : "{0:D} file(s) could not be searched.", notSearchedCount);
                }

                if (exceptionCount > 0)
                {
                    // Aggregating a large number of exceptions can bog down, potentially
                    // making the quickly making the app appear hung. Aggregate a maximum of 10
                    // exceptions.
                    var exceptionsToAggregate = exceptions.Take(10).Union(new [] {
                       new ExtractException("ELI35856",
                        string.Format(CultureInfo.CurrentCulture,
                        "There were error(s) searching {0:D} file(s).", exceptionCount)) });

                    ExtractException.AsAggregateException(exceptionsToAggregate).Display();
                }

                EnsureFileListColumnSizes();
            }));
        }

        /// <summary>
        /// Prepares a <see cref="DotNetRegexParser"/> instance to search for the
        /// <see paramref="searchTerm"/> based on the currently selected search options.
        /// </summary>
        /// <param name="searchTerm">The search term.</param>
        /// <returns>A <see cref="DotNetRegexParser"/> instance to search for the
        /// <see paramref="searchTerm"/>.
        /// </returns>
        DotNetRegexParser PrepareSearchRegex(string searchTerm)
        {
            DotNetRegexParser regexParser = new DotNetRegexParser();

            // If the search term is not a regex, escape the search term for use as a regex.
            regexParser.Pattern = _regexSearchCheckBox.Checked 
                ? searchTerm
                : Regex.Escape(searchTerm);

            if (_fuzzySearchCheckBox.Checked)
            {
                // Allow one wrong char per 3 chars in the search term and 1 extra whitespace char
                // per 2 chars in the search term.
                int errorAllowance = searchTerm.Length / 3;
                int whiteSpaceAllowance = searchTerm.Length / 2;

                regexParser.Pattern = string.Format(CultureInfo.InvariantCulture,
                    "(?~<method=better_fit,error={0:D},xtra_ws={1:D}>{2})",
                    errorAllowance, whiteSpaceAllowance, regexParser.Pattern);
            }

            regexParser.IgnoreCase = !_caseSensitiveSearchCheckBox.Checked;
            regexParser.RegexOptions |= RegexOptions.Compiled;
            return regexParser;
        }

        /// <summary>
        /// Starts the specified <see paramref="searchOperation"/>.
        /// </summary>
        /// <typeparam name="T">The data type used to represent the search terms for the operation.
        /// </typeparam>
        /// <param name="searchOperation">The <see cref="SearchOperation{T}"/> to be performed.
        /// </param>
        /// <param name="searchTerms">The search terms for the operation.</param>
        void StartSearch<T>(SearchOperation<T> searchOperation, T searchTerms)
        {
            // Ensure any previous background operation is canceled first.
            CancelBackgroundOperation();

            foreach (DataGridViewRow row in _fileListDataGridView.Rows)
            {
                row.GetFileData().ClearSearchResults();
                // All rows should be hidden until they are determined to be a match.
                row.Visible = false;
            }
            _fileListMatchesColumn.Visible = true;
            EnsureFileListColumnSizes();
            _fileListDataGridView.Invalidate();

            _searchProgressBar.Value = 0;
            _searchProgressBar.Maximum = _fileListDataGridView.Rows.Count;
            _searchProgressBar.Visible = true;
            _searchErrorStatusStripLabel.Visible = false;

            // Start by showing only the matching files.
            _showOnlyMatchesCheckBox.Enabled = true;
            _showOnlyMatchesCheckBox.Checked = true;

            var searchModifier = _searchModifierComboBox.ToEnumValue<SearchModifier>();

            // Run the search on a background thread so that the UI remains responsive while the
            // operation is running.
            StartBackgroundOperation(() =>
                searchOperation(searchTerms, searchModifier, _queryCanceler.Token));
        }

        /// <summary>
        /// Starts a background query or search operation.
        /// </summary>
        /// <param name="operation">The <see cref="Action"/> to be performed in the background.
        /// </param>
        void StartBackgroundOperation(Action operation)
        {
            try
            {
                // If any operations were invoked to start after closing the form, ignore them.
                if (_closing)
                {
                    return;
                }

                // Update UI to reflect an ongoing operation.
                OperationIsActive = true;

                // Generate a background task which will perform the search.
                _queryCanceler = new CancellationTokenSource();
                _queryTask = new Task(() => operation(), _queryCanceler.Token);
                _queryTask.ContinueWith((task) =>
                    this.SafeBeginInvoke("ELI35721",
                        () => OperationIsActive = false),
                        TaskContinuationOptions.NotOnFaulted);
                _queryTask.ContinueWith((task) =>
                {
                    Exception[] exceptions = task.Exception.InnerExceptions.ToArray();
                    this.SafeBeginInvoke("ELI35722", () =>
                    {
                        OperationIsActive = false;
                        foreach (Exception ex in exceptions)
                        {
                            ex.ExtractDisplay("ELI35874");
                        }
                    });
                }, TaskContinuationOptions.OnlyOnFaulted);

                // Kick off the background search and return.
                _queryTask.Start();
            }
            catch (Exception ex)
            {
                // If there was and error starting the query, be sure to reset the UI to reflect
                // the fact that the query is not active.
                OperationIsActive = false;

                throw ex.AsExtract("ELI35745");
            }
        }

        /// <summary>
        /// Cancels the any actively running background operation.
        /// </summary>
        void CancelBackgroundOperation()
        {
            if (_queryTask != null)
            {
                if (_queryTask.IsCompleted)
                {
                    // The task has already ended; dispose of it.
                    _queryTask.Dispose();
                    _queryTask = null;
                }
                else
                {
                    // The task is still running; since we are going to cancel it, update the
                    // continue with so that it disposes of the task without displaying any
                    // exceptions about being cancelled.
                    _queryTask.ContinueWith((task) =>
                    {
                        task.Dispose();
                        _queryTask = null;
                    });

                    // Cancel _queryTask and wait for it to finish.
                    // https://extract.atlassian.net/browse/ISSUE-13633
                    // Moved the Cancel call into the try block as well.
                    try
                    {
                        _queryCanceler.Cancel();
                        _queryTask.Wait();
                    }
                    catch { }  // Ignore any exceptions; we don't care about this task anymore.
                }
            }

            OperationIsActive = false;
        }

        /// <summary>
        /// Launches the specified <see paramref="fileNames"/> in the application defined by
        /// <see paramref="fileHanderItem"/>.
        /// </summary>
        /// <param name="fileHanderItem">The <see cref="FileHandlerItem"/> defining the application to
        /// be run.</param>
        /// <param name="fileNames">The files to be run in <see paramref="appLaunchItem"/></param>
        /// <param name="cancelToken">A <see cref="CancellationToken"/> the that should be checked
        /// before each file to see if the operation has been canceled.</param>
        void RunApplication(FileHandlerItem fileHanderItem, IEnumerable<string> fileNames,
            CancellationToken cancelToken)
        {
            try
            {
                // Collects any exceptions that occur when processing the files.
                var exceptions = new List<ExtractException>();

                // Process each filename in sequence.
                foreach (string fileName in fileNames)
                {
                    cancelToken.ThrowIfCancellationRequested();

                    try
                    {
                        // Expand the command line arguments using path tags/functions.
                        FileActionManagerPathTags pathTags = new FileActionManagerPathTags(null, fileName);
                        if (UseDatabaseMode)
                        {
                            pathTags.DatabaseServer = _fileProcessingDB.DatabaseServer;
                            pathTags.DatabaseName = _fileProcessingDB.DatabaseName;
                            pathTags.Workflow = FileProcessingDB.ActiveWorkflow;
                        }

                        string applicationPath = fileHanderItem.ApplicationPath;
                        if (!string.IsNullOrEmpty(applicationPath))
                        {
                            applicationPath = pathTags.Expand(applicationPath);
                        }

                        string arguments = fileHanderItem.Arguments;
                        if (!string.IsNullOrEmpty(arguments))
                        {
                            arguments = pathTags.Expand(arguments);
                        }

                        if (fileHanderItem.SupportsErrorHandling)
                        {
                            SystemMethods.RunExtractExecutable(applicationPath, arguments);
                        }
                        else
                        {
                            SystemMethods.RunExecutable(applicationPath, arguments, int.MaxValue);
                        }
                    }
                    catch (Exception ex)
                    {
                        exceptions.Add(ex.AsExtract("ELI35820"));
                    }
                }

                int exceptionCount = exceptions.Count;
                if (exceptionCount > 0)
                {
                    // If there was only a single file selected, just throw the exception as-is.
                    if (fileNames.Count() == 1)
                    {
                        throw exceptions.First();
                    }
                    // If more than one file was selected report all exceptions in one aggregate
                    // exception after processing.
                    else
                    {
                        // Aggregating a large number of exceptions can bog down, potentially
                        // making the quickly making the app appear hung. Aggregate a maximum of 10
                        // exceptions.
                        var exceptionsToAggregate = exceptions.Take(10).Union(new [] {
                            new ExtractException("ELI35819",
                                string.Format(CultureInfo.CurrentCulture,
                                "{0:D} file(s) failed {1}", exceptionCount, fileHanderItem.Name.Quote())) });

                        throw ExtractException.AsAggregateException(exceptionsToAggregate);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // If canceled, the user didn't want to wait around for the operation to complete;
                // they don't need to see an exception about the operation being canceled.
            }
            catch (Exception ex)
            {
                // [DotNetRCAndUtils:1029]
                // Exceptions should be displayed on the UI thread, blocking it which prevents the
                // form from being closed which can lead to a crash. Using BeginInvoke does not
                // always block the UI thread (not sure why) so use Invoke instead to guarantee the
                // UI thread is blocked.
                this.Invoke((MethodInvoker)(() =>
                {
                    ex.ExtractDisplay("ELI35811");
                    
                    // If there was an error launching a non-blocking app, ensure the status label is
                    // returned
                    if (!fileHanderItem.Blocking)
                    {
                        UpdateStatusLabel();
                    }
                }));
            }
        }

        /// <summary>
        /// Shows a modal, non-closeable message box with the specified <see param="messageText"/>
        /// while the specified <see param="action"/> runs on a background thread.
        /// </summary>
        /// <param name="messageText">The message to be displayed.</param>
        /// <param name="action">The action to run on a background thread.</param>
        void ShowMessageBoxWhileBlocking(string messageText, Action action)
        {
            CustomizableMessageBox messageBox = new CustomizableMessageBox();
            messageBox.UseDefaultOkButton = false;
            messageBox.Caption = _APPLICATION_TITLE;
            messageBox.Text = messageText;

            Task.Factory.StartNew(() =>
            {
                try
                {
                    action();
                    this.SafeBeginInvoke("ELI35875", () => messageBox.Close(""));
                }
                catch (Exception ex)
                {
                    ex.ExtractDisplay("ELI35876");
                }
                finally
                {
                    this.SafeBeginInvoke("ELI35877", () => messageBox.Dispose());
                }
            })
            .ContinueWith((task) =>
            {
                // Handle any exceptions to prevent unhandled exceptions from crashing the
                // application.
                foreach (Exception ex in task.Exception.InnerExceptions)
                {
                    ex.ExtractLog("ELI35878");
                }
            }, TaskContinuationOptions.OnlyOnFaulted);

            messageBox.Show(this);
        }

        /// <summary>
        /// Gets the next <see cref="DataGridViewRow"/> with that matches the specified
        /// <see paramref="rowQualifier"/>.
        /// </summary>
        /// <param name="down"><see langword="true"/> to find the next row below the current one;
        /// <see langword="false"/> to fine the next row above the current one.</param>
        /// <param name="rowQualifier">This <see cref="RowQualifier"/> must be
        /// <see langword="true"/> for any return value.</param>
        DataGridViewRow GetNextRow(bool down, RowQualifier rowQualifier)
        {
            var rows = _fileListDataGridView.Rows.OfType<DataGridViewRow>();
            // If navigating backward, reverse the row enumeration.
            if (!down)
            {
                rows = rows.Reverse();
            }

            // Find the current row.
            DataGridViewRow currentRow = _fileListDataGridView.CurrentRow;
            if (currentRow == null)
            {
                // If there was no current row, use the first.
                currentRow = rows.FirstOrDefault();
                if (rowQualifier == null || rowQualifier(currentRow))
                {
                    // If the first row qualifies, return it.
                    return currentRow;
                }
            }

            // Take the first row after current row.
            DataGridViewRow nextRow =
                rows.SkipWhile(row => currentRow != null && row != currentRow)
                    .Skip(1)
                    .Where(row => row.Visible && (rowQualifier == null || rowQualifier(row)))
                    .FirstOrDefault();

            return nextRow;
        }

        /// <summary>
        /// Updates the row visibility based on the row's search result and whether only search
        /// results are being displayed.
        /// </summary>
        /// <param name="rowData">The row's <see cref="FAMFileData"/>.</param>
        /// <param name="rowToUpdate">The <see cref="DataGridViewRow"/> to update.</param>
        void UpdateRowVisibility(FAMFileData rowData, DataGridViewRow rowToUpdate)
        {
            rowToUpdate.Visible =
                rowData.FileMatchesSearch || !_showOnlyMatchesCheckBox.Checked;
            if (rowToUpdate.Visible && SelectedRows.Count() == 1)
            {
                if (SelectedRows.First() == rowToUpdate &&
                    _fileListDataGridView.CurrentRow != rowToUpdate)
                {
                    _fileListDataGridView.CurrentCell = rowToUpdate.Cells[0];
                }
            }
            _fileListDataGridView.InvalidateRow(rowToUpdate.Index);
        }

        /// <summary>
        /// Updates the image displayed in the <see cref="_imageViewer"/> based on the
        /// <see pararef="currentRow"/>.
        /// </summary>
        /// <param name="currentRow">The <see cref="DataGridViewRow"/> that is currently active.
        /// </param>
        void UpdateImageViewerDisplay(DataGridViewRow currentRow)
        {
            _layerObjectSelectionStatusLabel.Text = "";
            _imageViewerErrorStripStatusLabel.Text = "";
            if (_overlayTextCanceler != null)
            {
                _overlayTextCanceler.Cancel();
                _overlayTextCanceler = null;
            }

            FAMFileData fileData = (currentRow == null) ? null : currentRow.GetFileData();

            if (fileData != null && File.Exists(fileData.FileName))
            {
                try
                {
                    // Open the image associated with fileData and highlight all search terms found int
                    // it.
                    OpenImage(fileData);
                }
                catch (Exception ex)
                {
                    ex.ExtractLog("ELI36791");

                    // https://extract.atlassian.net/browse/ISSUE-12142
                    // Just display text stating the image could not be opened rather than popping
                    // up an exception.
                    _overlayTextCanceler = OverlayText.ShowText(_imageViewer, "Failed to open image",
                        Font, Color.FromArgb(100, Color.Red), null, 0);
                }
            }
            else // either there no current row or no file available, close any open image.
            {
                if (_imageViewer.IsImageAvailable)
                {
                    _imageViewer.CloseImage();
                }

                // If there is a current row, display "File not found"
                if (currentRow != null)
                {
                    _overlayTextCanceler = OverlayText.ShowText(_imageViewer, "File not found",
                        Font, Color.FromArgb(100, Color.Red), null, 0);
                }
            }
        }

        /// <summary>
        /// Opens the image associated with <see paramref="fileData"/> via an asynchronous call.
        /// </summary>
        /// <param name="fileData">The <see cref="FAMFileData"/> associated with the image to be
        /// opened.</param>
        void OpenImage(FAMFileData fileData)
        {
            // [DotNetRCAndUtils:1012]
            // Queue the open image call from a background thread. This will allow UI events to be
            // handled in the meantime. If at the time the OpenImageInvoked call is made, additional
            // open image requests have been queued up, all but the most recent will be ignored.
            _pendingOpenImageCount++;
            Task.Factory.StartNew(() =>
            {
                // Sleep a trivial amount of time to encourage the application context to allow the
                // UI thread to handle messages in the meantime.
                Thread.Sleep(1);
                this.SafeBeginInvoke("ELI35844", () => OpenImageInvoked(fileData));
            })
            // In case of error invoking the image open, return _pendingOpenImageCount to its
            //previous value.
            .ContinueWith((task) =>
            {
                _pendingOpenImageCount--;

                // Handle any exceptions to prevent unhandled exceptions from crashing the
                // application.
                foreach (Exception ex in task.Exception.InnerExceptions)
                {
                    ex.ExtractLog("ELI35870");
                }
            }, TaskContinuationOptions.OnlyOnFaulted);
        }

        /// <summary>
        /// Helper function for <see cref="OpenImage"/>. Opens the image associated with 
        /// <see paramref="fileData"/>.
        /// </summary>
        /// <param name="fileData">The <see cref="FAMFileData"/> associated with the image to be
        /// opened.</param>
        void OpenImageInvoked(FAMFileData fileData)
        {
            try
            {
                ExtractException.Assert("ELI35846", "Unexpected open image operation",
                    _pendingOpenImageCount > 0);
                
                // If subsequent file open requests have been queued, ignore this one.
                _pendingOpenImageCount--;
                if (_pendingOpenImageCount > 0)
                {
                    return;
                }

                using (new TemporaryWaitCursor())
                {
                    // To prevent flicker of the image viewer tool strips while loading a new image,
                    // if we can find a parent ToolStripContainer, lock it until the new image is
                    // loaded.
                    // [DotNetRCAndUtils:931]
                    // This, and the addition of a parameter on OpenImage to prevent an initial
                    // refresh is in place of locking the entire form which can cause the form to
                    // fall behind other open applications when clicked.
                    LockControlUpdates toolStripLocker = null;
                    for (Control control = this; control != null; control = control.Parent)
                    {
                        var toolStripContainer = control as ToolStripContainer;
                        if (toolStripContainer != null)
                        {
                            toolStripLocker = new LockControlUpdates(toolStripContainer);
                            break;
                        }
                    }

                    try
                    {
                        // Because the ImageViewer has code to open the image associated with uss
                        // files, which can be confusing in context of the FFI.
                        if (Path.GetExtension(fileData.FileName).Equals(
                            ".uss", StringComparison.OrdinalIgnoreCase))
                        {
                            _imageViewer.CloseImage();
                            var ee = new ExtractException("ELI36792", "USS files cannot be opened as images.");
                            ee.AddDebugData("Filename", fileData.FileName, false);
                            throw ee;
                        }

                        _imageViewer.OpenImage(fileData.FileName, false, false);

                        // Maintain the ImagePageData when changing documents so that the image
                        // orientations are remembered if the document is re-opened.
                        if (fileData.ImagePageData == null)
                        {
                            fileData.ImagePageData = _imageViewer.ImagePageData;
                        }
                        else
                        {
                            _imageViewer.ImagePageData = fileData.ImagePageData;
                        }

                        // Display highlights for all search terms found in the selected file.
                        ShowMatchHighlights(fileData);
                    }
                    catch (Exception ex)
                    {
                        ex.ExtractLog("ELI36793");

                        // https://extract.atlassian.net/browse/ISSUE-12142
                        // Just display text stating the image could not be opened rather than popping
                        // up an exception.
                        _overlayTextCanceler = OverlayText.ShowText(_imageViewer, 
                            "Failed to open image", Font, Color.FromArgb(100, Color.Red), null, 0);
                    }
                    finally
                    {
                        if (toolStripLocker != null)
                        {
                            toolStripLocker.Dispose();
                        }
                    }
                }

                Refresh();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35845");
            }
        }

        /// <summary>
        /// Gets the page count.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns></returns>
        int GetPageCount(string fileName)
        {
            int pageCount = 0;
            try
            {
                using (var reader = Codecs.CreateReader(fileName))
                {
                    pageCount = reader.PageCount;
                }
            }
            // If we fail to get the page count for a file, just ignore the exception.
            // This is not a critical error (the file may not even be an image).
            catch { }

            return pageCount;
        }

        /// <summary>
        /// Sets the flag on the specified row(s) of the <see cref="_fileListDataGridView"/>.
        /// </summary>
        /// <param name="rows">The <see cref="DataGridViewRow"/>s on which the flag should be set or
        /// cleared.</param>
        /// <param name="setFlag"><see langword="true"/> to set the flag; otherwise,
        /// <see langword="false"/>.</param>
        void SetRowFlag(IEnumerable<DataGridViewRow> rows, bool setFlag)
        {
            foreach (var row in rows)
            {
                SetRowFlag(row, setFlag);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="row"></param>
        /// <param name="setFlag"></param>
        void SetRowFlag(DataGridViewRow row, bool setFlag)
        {
            row.GetFileData().Flagged = setFlag;
            var cell = row.Cells[_FILE_LIST_FLAG_COLUMN_INDEX];
            cell.Value = setFlag ? Resources.FlagImage : null;
            _fileListDataGridView.InvalidateCell(cell);
        }

        /// <summary>
        /// Displays highlights in the <see cref="_imageViewer"/> representing all search terms
        /// found in the corresponding file.
        /// </summary>
        /// <param name="fileData">The <see cref="FAMFileData"/> for which highlights are to be
        /// displayed.</param>
        void ShowMatchHighlights(FAMFileData fileData)
        {
            if (fileData.ShowTextResults.HasValue)
            {
                int nonSpatialCount = 0;

                // If showing the results of a text search
                if (fileData.ShowTextResults.Value)
                {
                    IEnumerable<Match> matches = fileData.TextMatches;
                    if (matches != null)
                    {
                        // Get the OCR text for the file; to save memory, TextMatches does not store
                        // the SpatialString representing the match. The SpatialStrings are created
                        // here using the each match against the OCR text.
                        SpatialString ocrText = fileData.OcrText;
                        if (ocrText != null)
                        {
                            foreach (Match match in matches)
                            {
                                // Create a SpatialString representing the match.
                                SpatialString resultValue =
                                    ocrText.GetSubString(match.Index, match.Index + match.Length - 1);
                                if (resultValue.HasSpatialInfo())
                                {
                                    foreach (CompositeHighlightLayerObject highlight in
                                        _imageViewer.CreateHighlights(resultValue, _HIGHLIGHT_COLOR))
                                    {
                                        _imageViewer.LayerObjects.Add(highlight);
                                    }
                                }
                                else
                                {
                                    nonSpatialCount++;
                                }
                            }
                        }
                    }
                }
                else // If showing the results of a data search
                {
                    IEnumerable<ThreadSafeSpatialString> matches = fileData.DataMatches;
                    if (matches != null)
                    {
                        foreach (ThreadSafeSpatialString match in matches)
                        {
                            if (match.SpatialString.HasSpatialInfo())
                            {
                                foreach (CompositeHighlightLayerObject highlight in
                                    _imageViewer.CreateHighlights(match.SpatialString, _HIGHLIGHT_COLOR))
                                {
                                    _imageViewer.LayerObjects.Add(highlight);
                                }
                            }
                            else
                            {
                                nonSpatialCount++;
                            }
                        }
                    }
                }

                _imageViewer.Invalidate();

                if (nonSpatialCount == 0)
                {
                    _imageViewerErrorStripStatusLabel.Text = "";
                }
                else
                {
                    _imageViewerErrorStripStatusLabel.Text = string.Format(CultureInfo.CurrentCulture,
                        "{0:D} matches have no spatial data and cannot be displayed",
                        nonSpatialCount);
                }

                if (_imageViewer.CanGoToNextLayerObject)
                {
                    _imageViewer.GoToNextVisibleLayerObject(true);
                }
            }
        }

        /// <summary>
        /// Causes the availability of features to be re-checked against the database.
        /// </summary>
        void CheckAvailableFeatures()
        {
            _copyFileNamesEnabled = null;
            _copyFilesEnabled = null;
            _copyFilesAndDataEnabled = null;
            _openFileLocationEnabled = null;
            _reportsEnabled = null;
        }

        /// <summary>
        /// Gets a value indicating whether FFI menu main and context menu options should be limited
        /// to basic non-custom options. The main database menu and custom file handlers context
        /// menu options will not be shown.
        /// </summary>
        /// <value><see langword="true"/> to limit menu options to basic options only; otherwise,
        /// <see langword="false"/>.
        /// </value>
        bool BasicMenuOptionsOnly
        {
            get
            {
                bool basicMenuOptionsOnly =
                    _customColumns.Values.Any(column => column.BasicMenuOptionsOnly) ||
                    (FileSelectorPane != null && FileSelectorPane.BasicMenuOptionsOnly);
                
                return basicMenuOptionsOnly;
            }
        }

        /// <summary>
        /// Updates the file selection summary label.
        /// </summary>
        void UpdateFileSelectionSummary()
        {
            ShowConnectionState();

            if (FileProcessingDB != null)
            {
                string summaryText =
                    (LockFileSelector && !string.IsNullOrWhiteSpace(LockedFileSelectionSummary))
                        ? LockedFileSelectionSummary
                        : FileSelector.GetSummaryString(FileProcessingDB, false);
                _selectFilesSummaryLabel.Text = "Listing ";
                _selectFilesSummaryLabel.Text +=
                    summaryText.Substring(0, 1).ToLower(CultureInfo.CurrentCulture);
                _selectFilesSummaryLabel.Text += summaryText.Substring(1);
            }
            else if (!string.IsNullOrWhiteSpace(SourceDirectory))
            {
                _selectFilesSummaryLabel.Text =
                    "Showing files in the directory: " + SourceDirectory;
                _selectFilesSummaryLabel.Text += Recursive
                    ? "\r\nFiles in subdirectories are included."
                    : "\r\nFiles in subdirectories are not included.";
                _selectFilesSummaryLabel.Text += string.IsNullOrWhiteSpace(FileFilter)
                    ? "\r\nShowing all files regardless of type."
                    : "\r\nShowing files of type: " + FileFilter;
            }
            else if (!string.IsNullOrWhiteSpace(FileListFileName))
            {
                _selectFilesSummaryLabel.Text =
                    "Showing files listed in file: " + FileListFileName;
            }
        }

        /// <summary>
        /// Updates UI elements that reflect the current connection state.
        /// </summary>
        void ShowConnectionState()
        {
            Text = _APPLICATION_TITLE;

            if (UseDatabaseMode)
            {
                if (FileProcessingDB.IsConnected)
                {
                    Text = DatabaseName + " on " + DatabaseServer + " - " + Text;
                    if (InAdminMode)
                    {
                        Text += " (Admin mode)";
                    }
                }

                _adminModeToolStripMenuItem.Enabled = FileProcessingDB.IsConnected && !InAdminMode;
            }
        }

        /// <summary>
        /// Initializes any custom columns that have been specified.
        /// </summary>
        void InitializeCustomColumns()
        {
            // The _fileListDataGridView needs to be editable if there are any custom columns that
            // are not read-only.
            if (_customColumns.Any(column => !column.Value.ReadOnly))
            {
                _fileListDataGridView.EditMode = DataGridViewEditMode.EditOnKeystroke;
            }

            // Loop through each custom column definition.
            foreach (KeyValuePair<int, IFAMFileInspectorColumn> customColumn in _customColumns)
            {
                int columnIndex = customColumn.Key;
                IFAMFileInspectorColumn columnDefinition = customColumn.Value;
                DataGridViewColumn column = null;
                if (columnDefinition.FFIColumnType == FFIColumnType.Text)
                {
                    column = new DataGridViewTextBoxColumn();
                }
                else if (columnDefinition.FFIColumnType == FFIColumnType.Combo)
                {
                    // Combo box columns should be populated with all possible value options across
                    // all circumstances. The ones that are relevant for a specific row will be
                    // queried at the time the value is edited in order to remove disallowed
                    // values for that row.
                    var comboBoxColumn = new DataGridViewComboBoxColumn();
                    var options = columnDefinition.GetValueChoices(-1).ToIEnumerable<object>();
                    if (options != null)
                    {
                        comboBoxColumn.Items.AddRange(options.ToArray());
                    }

                    column = comboBoxColumn;
                }
                else
                {
                    ExtractException.ThrowLogicException("ELI37430");
                }

                column.HeaderText = columnDefinition.HeaderText;
                column.ReadOnly = columnDefinition.ReadOnly;
                column.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;

                _fileListDataGridView.Columns.Insert(columnIndex, column);

                column.Width = columnDefinition.DefaultWidth;
            }
        }

        /// <summary>
        /// Adds sub menus for setting values in custom columns. These sub menus will be added
        /// dynamically with each display of the context menu such that a custom column could change
        /// what options should be available within and FFI session depending on changing data.
        /// </summary>
        /// <param name="contextMenu">The <see cref="ContextMenuStrip"/> to which the menus should
        /// be added.</param>
        void AddCustomColumnMenus(ContextMenuStrip contextMenu)
        {
            int menuIndex = 0;

            // Loop through all custom columns that are not read-only.
            foreach (KeyValuePair<int, IFAMFileInspectorColumn> customColumn in _customColumns
                .Where(customColumn => !customColumn.Value.ReadOnly))
            {
                int columnIndex = customColumn.Key;
                IFAMFileInspectorColumn column = customColumn.Value;
                
                // Retrieve all options that may be valid across all possible selections and add
                // them to a context sub-menu. Options that are not valid given a specific selection
                // will be disabled at the time the context menu is displayed.
                var options = column.GetContextMenuChoices(null).ToIEnumerable<string>();
                if (options != null && options.Any())
                {
                    ToolStripMenuItem columnMenu = new ToolStripMenuItem(column.HeaderText);

                    foreach (string option in options)
                    {
                        var subMenuItem = (ToolStripMenuItem)columnMenu.DropDownItems.Add(option);
                        subMenuItem.Click += HandleCustomColumnContextMenuItem_Click;
                    }

                    contextMenu.Items.Insert(menuIndex, columnMenu);
                    _customColumnMenus[columnMenu] = columnIndex;
                    menuIndex++;
                }
            }

            // If at least one context menu was added, add a separator between the custom column
            // context menu options and the standard context menu options.
            if (menuIndex > 0)
            {
                _customMenuSeparator = new ToolStripSeparator();
                contextMenu.Items.Insert(menuIndex, _customMenuSeparator);
            }
        }

        /// <summary>
        /// Enables/disables custom column value choices based on the current selection.
        /// </summary>
        void UpdateCustomColumnMenuEnabledStates()
        {
            HashSet<int> selectedFileIDs = new HashSet<int>(_fileListDataGridView.SelectedRows
                .OfType<DataGridViewRow>()
                .Where(row => row.Visible)
                .Select(row => row.GetFileData().FileID));

            // Loop through each custom column context menu to enable/disable the value choices
            // based on the current selection,
            foreach (KeyValuePair<ToolStripItem, int> customMenu in _customColumnMenus)
            {
                int columnIndex = customMenu.Value;
                ToolStripMenuItem customToolStripMenu = (ToolStripMenuItem)customMenu.Key;

                IFAMFileInspectorColumn column = _customColumns[columnIndex];
                var enabledOptions = column.GetContextMenuChoices(selectedFileIDs);

                foreach (ToolStripItem item in customToolStripMenu.DropDownItems)
                {
                    item.Enabled = (enabledOptions != null && enabledOptions.Contains(item.Text));
                }
            }
        }

        /// <summary>
        /// Refreshes all values in custom columns that may have been modified programmatically by
        /// the <see cref="IFAMFileInspectorColumn"/> implementation.
        /// </summary>
        void RefreshCustomColumns()
        {
            // So that multiple column refreshes don't occur as a result of the same action in the
            // UI use BeginInvoke to schedule the refresh via the message queue.
            if (!_customColumnRefreshPending)
            {
                _customColumnRefreshPending = true;
                this.SafeBeginInvoke("ELI37435", () =>
                {
                    _customColumnRefreshPending = false;
                    
                    // Loop through every custom column
                    foreach (KeyValuePair<int, IFAMFileInspectorColumn> customColumn in _customColumns)
                    {
                        RefreshCustomColumn(customColumn.Key, customColumn.Value);
                    }
                });
            }
        }

        /// <summary>
        /// Refreshes all values in the specified <see paramref="column"/> that may have been
        /// modified programmatically by the <see cref="IFAMFileInspectorColumn"/> implementation.
        /// </summary>
        /// <param name="columnIndex">Index of the column.</param>
        /// <param name="column">The <see cref="IFAMFileInspectorColumn"/> defining the column.
        /// </param>
        void RefreshCustomColumn(int columnIndex, IFAMFileInspectorColumn column)
        {
            // Refresh each value in the column that requires a refresh.
            foreach (int fileId in column.GetValuesToRefresh())
            {
                DataGridViewRow row = null;
                if (_rowsByFileId.TryGetValue(fileId, out row))
                {
                    string value = GetCustomColumnValue(fileId, columnIndex);
                    var cell = row.Cells[columnIndex];
                    cell.Value = value;

                    // Refresh the active editing control if it is currently displayed.
                    if (cell == _fileListDataGridView.CurrentCell &&
                        _fileListDataGridView.EditingControl != null)
                    {
                        _fileListDataGridView.EditingControl.Text = value;
                    }
                }
            }
        }

        /// <summary>
        /// Gets all custom components implementing <see cref="IFFIDataManager"/>.
        /// </summary>
        IEnumerable<IFFIDataManager> CustomDataManagers
        {
            get
            {
                var dataManagers = _customColumns.Values.OfType<IFFIDataManager>();
                var dataManagerPane = FileSelectorPane as IFFIDataManager;
                if (dataManagerPane != null)
                {
                    dataManagers = dataManagers.Union(new[] {dataManagerPane});
                }

                return dataManagers;
            }
        }

        /// <summary>
        /// Gets any prompts <see cref="CustomDataManagers"/> need to display before committing
        /// data.
        /// </summary>
        IEnumerable<string> CustomApplyPrompts
        {
            get
            {
                return CustomDataManagers.Where(manager =>
                    manager.Dirty && !string.IsNullOrWhiteSpace(manager.ApplyPrompt))
                    .Select(column => column.ApplyPrompt);
            }
        }

        /// <summary>
        /// Gets any prompts <see cref="CustomDataManagers"/> need to display before canceling
        /// changes.
        /// </summary>
        IEnumerable<string> CustomCancelPrompts
        {
            get
            {
                return CustomDataManagers.Where(manager =>
                    manager.Dirty && !string.IsNullOrWhiteSpace(manager.CancelPrompt))
                    .Select(column => column.CancelPrompt);
            }
        }

        /// <summary>
        /// Prompts to apply custom control changes.
        /// </summary>
        /// <param name="canceling"><see langword="true"/> if the prompt is being displayed in the
        /// context of canceling, <see langword="false"/> if it is being displayed in the context of
        /// applying.</param>
        /// <returns><see langword="true"/> if any prompting or applying of custom control data has
        /// occurred and the form can be closed; <see langword="false"/> if the form should not be
        /// allowed to close.</returns>
        bool PromptToApplyCustomChanges(bool canceling)
        {
            bool allowClose = true;

            IEnumerable<string> customPrompts = canceling
                ? CustomCancelPrompts
                : CustomApplyPrompts;

            // JIRA ISSUE-13058 - Order picker doesn't display Apply changes dialog when closing
            // JIRA ISSUE-13164 - Modify FFI IDataManager Cancel/Close button behavior
            // The desired behavior is:
            //      1) The Close Button and the go-away box should act the same
            //      2) Both close methods should not prompt, even if the user has made changes that will be lost.
            //
            if (customPrompts.Any())
            {
                using (var messageBox = new CustomizableMessageBox())
                {
                    StringBuilder message = new StringBuilder(
                        canceling
                            ? "There are uncommitted changes."
                            : "You have selected to:");

                    message.AppendLine();
                    message.AppendLine();
                    message.AppendLine(string.Join("\r\n\r\n", customPrompts));
                    message.AppendLine();

                    message.Append("Apply changes?");
                    messageBox.Caption = "Apply changes?";
                    messageBox.Text = message.ToString();
                    if (canceling)
                    {
                        messageBox.AddStandardButtons(MessageBoxButtons.YesNoCancel);
                    }
                    else
                    {
                        messageBox.AddStandardButtons(MessageBoxButtons.OKCancel);
                    }
                    string response = messageBox.Show(this);
                    if (response == "Cancel")
                    {
                        allowClose = false;
                    }
                    else if (response == "No") // An option only if canceling.
                    {
                        allowClose = true;
                    }
                    else  // Yes or OK depending on if canceling.
                    {
                        allowClose = ApplyCustomData();
                    }
                }
            }
            else if (canceling)
            {
                CancelCustomData();
            }
            else
            {
                allowClose = ApplyCustomData();
            }

            return allowClose;
        }

        /// <summary>
        /// Applies uncommitted data in all <see cref="CustomDataManagers"/>.
        /// </summary>
        /// <returns><see langword="true"/> if the changes were successfully applied; otherwise,
        /// <see langword="false"/>.</returns>
        bool ApplyCustomData()
        {
            bool success = true;
            bool changesWereApplied = false;

            foreach (IFFIDataManager manager in CustomDataManagers)
            {
                success &= manager.Apply();
                changesWereApplied |= success;
            }

            // Even if the FFI close was initiated via the form close button (red X), if they chose
            // to commit changes, consider the dialog result OK.
            if (success && changesWereApplied)
            {
                DialogResult = DialogResult.OK;
            }

            return success;
        }

        /// <summary>
        /// Cancels uncommitted data changes in all <see cref="CustomDataManagers"/>.
        /// </summary>
        void CancelCustomData()
        {
            foreach (IFFIDataManager manager in CustomDataManagers)
            {
                manager.Cancel();
            }
        }

        /// <summary>
        /// For the most part, the column sizes are fixed with the folder column using all remaining
        /// space. However, if the left pane is sized such that not even the fixed columns have
        /// enough room to be displayed, reduce the width of the filename column as well until all
        /// the remaining columns fit properly within the grid.
        /// </summary>
        void EnsureFileListColumnSizes()
        {
            int widthOfOtherColumns = _fileListDataGridView.Columns
                .OfType<DataGridViewColumn>()
                .Where(column => column.Visible)
                .Except(new[] { _fileListNameColumn })
                .Sum(column => column.Width);

            int scrollBarWidth = _fileListDataGridView.Controls
                .OfType<VScrollBar>()
                .Where(scrollBar => scrollBar.Visible)
                .Sum(scrollBar => scrollBar.Width);

            int availableWidth = _fileListDataGridView.ClientSize.Width
                - _fileListDataGridView.RowHeadersWidth
                - widthOfOtherColumns
                - scrollBarWidth
                - 2; // Seem to need to subtract a couple more pixels to avoid a horizontal scroll bar.

            if (_fileListNameColumn.Width > availableWidth)
            {
                _fileListNameColumn.Width = availableWidth;
            }
        }

        /// <summary>
        /// Disposes the context menu items.
        /// </summary>
        void DisposeContextMenu()
        {
            if (_fileListDataGridView.ContextMenuStrip != null)
            {
                _fileListDataGridView.ContextMenuStrip.Dispose();
                _fileListDataGridView.ContextMenuStrip = null;
            }

            if (_customColumnMenus != null)
            {
                foreach (var menuItem in _customColumnMenus.Keys.OfType<ToolStripMenuItem>())
                {
                    foreach (ToolStripItem subMenuItem in menuItem.DropDownItems)
                    {
                        subMenuItem.Dispose();
                    }
                    menuItem.Dispose();
                }
                _customColumnMenus = null;
            }

            if (_customMenuSeparator != null)
            {
                _customMenuSeparator.Dispose();
                _customMenuSeparator = null;
            }

            if (_fileHandlerItems != null)
            {
                foreach (ToolStripMenuItem menuItem in _fileHandlerItems.Keys)
                {
                    menuItem.Dispose();
                }

                _fileHandlerItems = null;
            }

            if (_copyFileNamesMenuItem != null)
            {
                _copyFileNamesMenuItem.Dispose();
                _copyFileNamesMenuItem = null;
            }

            if (_copyFilesMenuItem != null)
            {
                _copyFilesMenuItem.Dispose();
                _copyFilesMenuItem = null;
            }

            if (_copyFilesAndDataMenuItem != null)
            {
                _copyFilesAndDataMenuItem.Dispose();
                _copyFilesAndDataMenuItem = null;
            }

            if (_openFileLocationMenuItem != null)
            {
                _openFileLocationMenuItem.Dispose();
                _openFileLocationMenuItem = null;
            }

            if (_reportMenuItems != null)
            {
                CollectionMethods.ClearAndDisposeKeysAndValues(_reportMenuItems);
                _reportMenuItems = null;
            }

            if (_reportMainMenuItem != null)
            {
                _reportMainMenuItem.Dispose();
                _reportMainMenuItem = null;
            }

            if (_setFlagMenuItem != null)
            {
                _setFlagMenuItem.Dispose();
                _setFlagMenuItem = null;
            }

            if (_clearFlagMenuItem != null)
            {
                _clearFlagMenuItem.Dispose();
                _clearFlagMenuItem = null;
            }
        }

        /// <summary>
        /// Adds any custom FileHandler options from the FAM DB to the
        /// <see paramref="contextMenuStrip"/>.
        /// </summary>
        /// <param name="contextMenuStrip">The <see cref="ContextMenuStrip"/> to which the file
        /// handlers should be added.</param>
        void AddCustomFileHandlerOptions(ContextMenuStrip contextMenuStrip)
        {
            // Populate context menu options for all enabled items available with the current
            // log-on mode from the database's FileHandler table.
            using (var queryResults = GetResultsForQuery(
                "SELECT [AppName], [ApplicationPath], [Arguments], [AllowMultipleFiles], " +
                    "[SupportsErrorHandling], [Blocking], [WorkflowName] " +
                "FROM [FileHandler] WHERE [Enabled] = 1 " +
                (InAdminMode ? "" : "AND [AdminOnly] = 0 ") +
                "ORDER BY [AppName]"))
            {
                foreach (var row in queryResults.Rows.OfType<DataRow>())
                {
                    // Create an FileHandlerItem instance representing the settings of this item.
                    var fileHandlerItem = new FileHandlerItem();
                    fileHandlerItem.Name = (string)row["AppName"];
                    fileHandlerItem.ApplicationPath = (string)row["ApplicationPath"].ToString();
                    fileHandlerItem.Arguments = (string)row["Arguments"].ToString();
                    fileHandlerItem.AllowMultipleFiles = (bool)row["AllowMultipleFiles"];
                    fileHandlerItem.SupportsErrorHandling = (bool)row["SupportsErrorHandling"];
                    fileHandlerItem.Blocking = (bool)row["Blocking"];
                    var workflowValue = row["WorkflowName"];
                    fileHandlerItem.Workflow = (workflowValue == DBNull.Value)
                        ? "" : 
                        (string)workflowValue;

                    // Create a context menu option and add a handler for it.
                    var menuItem = new ToolStripMenuItem(fileHandlerItem.Name);
                    menuItem.Click += HandleFileHandlerMenuItem_Click;

                    _fileHandlerItems.Add(menuItem, fileHandlerItem);
                    contextMenuStrip.Items.Add(menuItem);
                }
            }
        }

        #endregion Private Members
    }
}
