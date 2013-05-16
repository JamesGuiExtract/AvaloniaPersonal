using ADODB;
using Extract.Drawing;
using Extract.Imaging;
using Extract.Imaging.Forms;
using Extract.Licensing;
using Extract.Utilities;
using Extract.Utilities.Forms;
using Extract.Utilities.Parsers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
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
            FileSystemMethods.ApplicationDataPath, "FAMFileInspector", "FAMFileInspector.xml");

        /// <summary>
        /// Name for the mutex used to serialize persistance of the control and form layout.
        /// </summary>
        static readonly string _FORM_PERSISTENCE_MUTEX_STRING =
            "24440334-DE0C-46C1-920F-45D064A10DBF";

        /// <summary>
        /// The column from <see cref="_fileListDataGridView"/> that represents the results of the
        /// most recent search.
        /// </summary>
        internal static int _FILE_LIST_MATCH_COLUMN_INDEX = 2;

        /// <summary>
        /// The color of highlights to show found search terms in documents.
        /// </summary>
        static readonly Color _HIGHLIGHT_COLOR = Color.LimeGreen;

        /// <summary>
        /// The color of the dashed border used to indicate the currently selected match.
        /// </summary>
        static readonly Color _SELECTION_BORDER_COLOR = Color.Red;

        /// <summary>
        /// The maximum number of files to display at once.
        /// </summary>
        public static readonly int MaxFilesToDisplay = 1000;

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
        /// Represents an entry from the FAM DB's AppLaunch table
        /// </summary>
        struct AppLaunchItem
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
        /// Associates pre-defined search terms from the database's FieldSearch table with their
        /// associated queries.
        /// </summary>
        Dictionary<string, string> _dataSearchQueries = new Dictionary<string, string>();

        /// <summary>
        /// All application launch items that should be available as context menu options in
        /// <see cref="_fileListDataGridView"/> and the <see cref="AppLaunchItem"/> that defines the
        /// option's behavior.
        /// </summary>
        Dictionary<ToolStripMenuItem, AppLaunchItem> _appLaunchItems =
            new Dictionary<ToolStripMenuItem, AppLaunchItem>();

        /// <summary>
        /// A <see cref="Task"/> that performs database query operations on a background thread.
        /// </summary>
        volatile Task _queryTask;

        /// <summary>
        /// Allows the <see cref="_queryTask"/> to be canceled.
        /// </summary>
        volatile CancellationTokenSource _queryCanceler;

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
        /// Indicates whether the last match (instead of the first) should be selected by default
        /// when a new image is opened.
        /// </summary>
        bool _selectLastMatchByDefault;

        /// <summary>
        /// Indicates if the host is in design mode or not.
        /// </summary>
        readonly bool _inDesignMode;

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

                // License SandDock before creating the form.
                SandDockManager.ActivateProduct(_SANDDOCK_LICENSE_STRING);

                FileProcessingDB = new FileProcessingDB();
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

                // Settings PopuSize to 1 for the dockable window prevents it from popuping over
                // other windows when hovering while collapsed. (I found this behavior to be
                // confusing)
                _searchDockableWindow.PopupSize = 1;

                LayerObject.SelectionPen = ExtractPens.GetThickDashedPen(_SELECTION_BORDER_COLOR);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35712");
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the <see cref="FileProcessingDB"/> whose files are being inspected.
        /// </summary>
        /// <value>
        /// The <see cref="FileProcessingDB"/> whose files are being inspected.
        /// </value>
        [CLSCompliant(false)]
        public FileProcessingDB FileProcessingDB
        {
            get;
            set;
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
        /// Gets or sets the name database server being used.
        /// </summary>
        /// <value>
        /// The name of the database server being used.
        /// </value>
        public string DatabaseServer
        {
            get
            {
                return FileProcessingDB.DatabaseServer;
            }

            set
            {
                FileProcessingDB.DatabaseServer = value;
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
                return FileProcessingDB.DatabaseName;
            }

            set
            {
                FileProcessingDB.DatabaseName = value;
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Resets all changes to file selection back to the default (no conditions, top 1000 files).
        /// </summary>
        public void ResetFileSelectionSettings()
        {
            try
            {
                FileSelector.Reset();
                FileSelector.LimitToSubset(false, false, MaxFilesToDisplay);
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
            Recordset queryResults = null;

            try
            {
                ClearSearchResults();

                _searchModifierComboBox.SelectEnumValue(SearchModifier.Any);
                _textSearchTermsDataGridView.Rows.Clear();
                _dataSearchTermsDataGridView.Rows.Clear();

                // Populate all pre-defined search terms from the database's FieldSearch table.
                queryResults = FileProcessingDB.GetResultsForQuery(
                    "SELECT [FieldName], [AttributeQuery] FROM [FieldSearch] WHERE [Enabled] = 1 " +
                    "ORDER BY [FieldName]");
                while (!queryResults.EOF)
                {
                    string fieldName = (string)queryResults.Fields["FieldName"].Value;
                    string attributeQuery = (string)queryResults.Fields["AttributeQuery"].Value;

                    _dataSearchQueries[fieldName] = attributeQuery;

                    int index = _dataSearchTermsDataGridView.Rows.Add(fieldName, "");
                    _dataSearchTermsDataGridView.Rows[index].Cells[0].ReadOnly = true;

                    queryResults.MoveNext();
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35772");
            }
            finally
            {
                if (queryResults != null)
                {
                    queryResults.Close();
                }
            }
        }

        /// <summary>
        /// Generates the displayed file list based on the <see cref="FileSelector"/>'s current
        /// settings.
        /// </summary>
        public void GenerateFileList()
        {
            try
            {
                // Ensure any previous background operation is canceled first.
                CancelBackgroundOperation();

                if (!FileProcessingDB.IsConnected)
                {
                    FileProcessingDB.ResetDBConnection();
                }

                UpdateFileSelectionSummary();

                _fileListDataGridView.Rows.Clear();

                // If generating a new file list, the previous search results don't apply anymore.
                // Unheck show search results until a new search is run.
                _showOnlyMatchesCheckBox.Checked = false;
                _showOnlyMatchesCheckBox.Enabled = false;

                string query = FileSelector.BuildQuery(FileProcessingDB,
                    "[FAMFile].[ID], [FAMFile].[FileName], [FAMFile].[Pages]",
                    " ORDER BY [FAMFile].[ID]");

                // Run the query on a background thread so the UI remains responsive as rows are loaded.
                StartBackgroundOperation(() => RunDatabaseQuery(query, _queryCanceler.Token));
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35792");
            }
        }

        /// <summary>
        /// Initializes the context menu of the file list based on the current
        /// <see cref="FileProcessingDB"/>'s AppLaunch table.
        /// </summary>
        public void InitializeContextMenu()
        {
            Recordset queryResults = null;

            try
            {
                // Dispose of any previous context menu option.
                if (_fileListDataGridView.ContextMenuStrip != null)
                {
                    _fileListDataGridView.ContextMenuStrip.Dispose();
                    _fileListDataGridView.ContextMenuStrip = null;
                }

                var newContextMenuStrip = new ContextMenuStrip();

                // Populate context menu options for all enabled items from the database's AppLaunch
                // table.
                queryResults = FileProcessingDB.GetResultsForQuery(
                    "SELECT [AppName], [ApplicationPath], [Arguments], [AllowMultipleFiles], " +
                        "[SupportsErrorHandling], [Blocking] " +
                    "FROM [LaunchApp] WHERE [Enabled] = 1 ORDER BY [AppName]");
                while (!queryResults.EOF)
                {
                    // Create an AppLaunchItem instance representing the settings of this item.
                    var appLaunchItem = new AppLaunchItem();
                    appLaunchItem.Name = (string)queryResults.Fields["AppName"].Value;
                    appLaunchItem.ApplicationPath =
                        (string)queryResults.Fields["ApplicationPath"].Value;
                    if (!(queryResults.Fields[2].Value is System.DBNull))
                    {
                        appLaunchItem.Arguments =
                            (string)(queryResults.Fields["Arguments"].Value ?? string.Empty);
                    }
                    appLaunchItem.AllowMultipleFiles =
                        (bool)queryResults.Fields["AllowMultipleFiles"].Value;
                    appLaunchItem.SupportsErrorHandling =
                        (bool)queryResults.Fields["SupportsErrorHandling"].Value;
                    appLaunchItem.Blocking = (bool)queryResults.Fields["Blocking"].Value;

                    // Create a context menu option and add a handler for it.
                    var menuItem = new ToolStripMenuItem(appLaunchItem.Name);
                    menuItem.Click += HandleLaunchAppMenuItem_Click;

                    _appLaunchItems.Add(menuItem, appLaunchItem);
                    newContextMenuStrip.Items.Add(menuItem);

                    queryResults.MoveNext();
                }

                // If there is at least one enabled context menu option, attach the menu to
                // _fileListDataGridView
                if (newContextMenuStrip.Items.Count > 0)
                {
                    newContextMenuStrip.Items.Add(new ToolStripSeparator());
                    newContextMenuStrip.Items.Add(new ToolStripMenuItem("Cancel"));

                    _fileListDataGridView.ContextMenuStrip = newContextMenuStrip;

                    // Handle the opening of the context menu so that the available options can be
                    // enabled/disabled appropriately.
                    newContextMenuStrip.Opening += HandleContextMenuStrip_Opening;
                }
                else
                {
                    newContextMenuStrip.Dispose();
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35821");
            }
            finally
            {
                if (queryResults != null)
                {
                    queryResults.Close();
                }
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
                base.OnLoad(e);

#if DEBUG
                if (_searchSplitContainer.Location != Point.Empty)
                {
                    UtilityMethods.ShowMessageBox(
                        "For some reason with this form, if _searchSplitContainer is not " +
                        "initialized last in InitializeComponent, the components on the form " +
                        "get re-arranged when the form is displayed outside of the designer. " +
                        "Move the initialization of _searchSplitContainer and it's two panels " +
                        "to just above the FAMFileInspectorForm itself below after making " +
                        "changes in the designer.", "Incorrect layout detected", true);
                }
#endif

                // Search capability is available only as a separately licensed feature.
                if (!LicenseUtilities.IsLicensed(LicenseIdName.FileInspectorSearch))
                {
                    // Removing the entire dock container seems is the only sure-fire way I can find
                    // to prevent the tab from showing.
                    _searchSplitContainer.Panel2.Controls.Remove(_dockContainer);
                    _showOnlyMatchesCheckBox.Visible = false;
                    _fileListDataGridView.Dock = DockStyle.Fill;
                }
                else if (!_searchDockableWindow.IsOpen)
                {
                    // If this application was run without search licensed, the form state manager
                    // will remember that the search pane was closed. Force it open.
                    _searchDockableWindow.Open();
                }

                // Establish image viewer connections prior to calling base.OnLoad which will
                // potentially remove some IImageViewerControls.
                _imageViewer.EstablishConnections(this);

                _imageViewer.Shortcuts[Keys.Oemcomma] = HandlePreviousMatchCommand;
                _imageViewer.Shortcuts[Keys.OemPeriod] = HandleNextMatchCommand;

                // Initialize the search settings.
                _searchTypeComboBox.SelectEnumValue(SearchType.Text);
                ResetSearch();

                InitializeContextMenu();
                GenerateFileList();
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
            try
            {
                if (keyData.HasFlag(Keys.Tab))
                {
                    bool forward = !keyData.HasFlag(Keys.Shift);

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

                // This key was not processed, bubble it up to the base class.
                return base.ProcessCmdKey(ref msg, keyData);
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI35841", ex);
            }

            return true;
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
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Clean up managed objects
                if (_formStateManager != null)
                {
                    _formStateManager.Dispose();
                    _formStateManager = null;
                }

                if (_appLaunchItems != null)
                {
                    foreach (ToolStripMenuItem menuItem in _appLaunchItems.Keys)
                    {
                        menuItem.Dispose();
                    }

                    _appLaunchItems = null;
                }

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
            }

            // Clean up unmanaged objects

            base.Dispose(disposing);
        }

        #endregion Overrides

        #region Event Handlers

        /// <summary>
        /// Handles the <see cref="T:DockableWindow.Closing"/> event for all
        /// <see cref="DockableWindow"/>s on the form.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="TD.SandDock.DockControlClosingEventArgs"/> instance
        /// containing the event data.</param>
        void HandleDockWindow_Closing(object sender, DockControlClosingEventArgs e)
        {
            try
            {
                // In order to allow the close (X) button to be used to "close" the dockable window
                // but still have a tab available to re-open them, cancel the close and collapse the
                // pane instead.
                e.Cancel = true;
                e.DockControl.Collapsed = true;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35714");
            }
        }

        /// <summary>
        /// Handles the <see cref="T:DockableWindow.AutoHidePopupOpened"/> event for all
        /// <see cref="DockableWindow"/>s on the form.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleDockableWindow_AutoHidePopupOpened(object sender, EventArgs e)
        {
            try
            {
                var dockableWindow = (DockableWindow)sender;

                // If a collapsed window has been opened via mouse press, immediately un-collapse it
                // rather than allow it to temporarily popup over all other windows.
                // Ignore if the mouse isn't down (meaning SandDock timer related to hover is
                // triggering).
                if (dockableWindow.Collapsed &&
                    (Control.MouseButtons.HasFlag(System.Windows.Forms.MouseButtons.Left)))
                {
                    dockableWindow.Collapsed = false;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35715");
            }
        }

        /// <summary>
        /// Handles the <see cref="T:SandDockManager.DockControlActivated"/> event of the
        /// <see cref="_sandDockManager"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="TD.SandDock.DockControlEventArgs"/> instance containing
        /// the event data.</param>
        void HandleSandDockManager_DockControlActivated(object sender, DockControlEventArgs e)
        {
            try
            {
                // If a collapsed window has been activated, immediately un-collapse it rather than
                // allow it to temporarily popup over all other windows.
                if (e.DockControl.Collapsed)
                {
                    e.DockControl.Collapsed = false;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35716");
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
                if (FileSelector.Configure(FileProcessingDB, "Select the files to be listed",
                    "SELECT [Filename] FROM [FAMFile]"))
                {
                    ClearSearchResults();
                    GenerateFileList();
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35717");
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
                _dataSearchTermsDataGridView.Visible = !textSearch;
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
	        }
	        catch (Exception ex)
	        {
		        ex.ExtractDisplay("ELI35718");
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
        /// <see cref="_fileListDataGridView"/> control.
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

                // If the row under the right-click is not selected, do not present the menu to
                // avoid confusion as to whether any action taken should be applied to a
                // non-selected row beneath the context menu origin.
                if (hit.RowIndex < 0 || !_fileListDataGridView.Rows[hit.RowIndex].Selected)
                {
                    e.Cancel = true;
                    return;
                }

                int selectionCount = _fileListDataGridView.SelectedRows.Count;

                // Enable/disable each app launch item in the context menu based on the current
                // selection.
                foreach (KeyValuePair<ToolStripMenuItem, AppLaunchItem> app in _appLaunchItems)
                {
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
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35822");
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event of a <see cref="ToolStripMenuItem"/> for
        /// one of the app launch context menu options.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleLaunchAppMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                AppLaunchItem appLaunchItem = _appLaunchItems[(ToolStripMenuItem)sender];

                // Convert the selected files to an array so that the file list is a snapshot that
                // won't change after processing has begun.
                var fileNames = _fileListDataGridView.SelectedRows
                    .OfType<DataGridViewRow>()
                    .OrderBy(row => row.Index)
                    .Select(row => row.GetFileData().FileName)
                    .ToArray();

                if (appLaunchItem.Blocking)
                {
                    // If blocking, use a modal message box to block rather than calling
                    // RunApplication on this thread; the latter causes the for to report "not
                    // responding" in some circumstances.
                    CustomizableMessageBox messageBox = new CustomizableMessageBox();
                    messageBox.UseDefaultOkButton = false;
                    messageBox.Caption = _APPLICATION_TITLE;
                    messageBox.Text = "Running " + appLaunchItem.Name.Quote() + "...";

                    Task.Factory.StartNew(() =>
                    {
                        try
                        {
                            RunApplication(appLaunchItem, fileNames);
                            this.SafeBeginInvoke("ELI35825", () => messageBox.Close(""));
                        }
                        catch (Exception ex)
                        {
                            ex.ExtractDisplay("ELI35824");
                        }
                        finally
                        {
                            this.SafeBeginInvoke("ELI35826", () => messageBox.Dispose());
                        }
                    });

                    messageBox.Show(this);
                }
                else
                {
                    // If not blocking, run the application on a background thread; allow the UI thread
                    // to continue.
                    Task.Factory.StartNew(() => RunApplication(appLaunchItem, fileNames));

                    // Since a non-blocking app will continue to run in the background, use the
                    // status bar to indicate the application is being launched. After 5 seconds,
                    // revert to the normal status message.
                    _searchStatusLabel.Text = "Started " + appLaunchItem.Name.Quote() + "...";
                    Task.Factory.StartNew(() =>
                    {
                        Thread.Sleep(5000);
                        this.SafeBeginInvoke("ELI35818", () => UpdateStatusLabel());
                    });
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35823");
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
        /// Handles the UI command to select the previous search result.
        /// </summary>
        void HandlePreviousMatchCommand()
        {
            try
            {
                // If there is a previous search result in this document, go to it.
                if (_imageViewer.CanGoToPreviousLayerObject)
                {
                    _imageViewer.GoToPreviousLayerObject();
                }
                // Otherwise, go to the next previous document with any matches.
                else
                {
                    DataGridViewRow nextRow = GetNextRow(false, row => 
                        row.GetFileData().MatchCount > 0);
                    if (nextRow != null)
                    {
                        try
                        {
                            // Because we are navigating backward, we want the last match of the
                            // document selected. Set _selectLastMatchByDefault to true before
                            // selecting the next document.
                            _selectLastMatchByDefault = true;
                            _fileListDataGridView.CurrentCell = nextRow.Cells[0];
                        }
                        finally
                        {
                            _selectLastMatchByDefault = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35839");
            }
        }

        /// <summary>
        /// Handles the UI command to select the next search result.
        /// </summary>
        void HandleNextMatchCommand()
        {
            try
            {
                // If there is a subsequent search result in this document, go to it.
                if (_imageViewer.CanGoToNextLayerObject)
                {
                    _imageViewer.GoToNextLayerObject();
                }
                // Otherwise, go to the next document with any matches.
                else
                {
                    DataGridViewRow nextRow = GetNextRow(true, row =>
                        row.GetFileData().MatchCount > 0);
                    if (nextRow != null)
                    {
                        _fileListDataGridView.CurrentCell = nextRow.Cells[0];
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35840");
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

                    if (FileProcessingDB.ShowSelectDB("Select database", false, true))
                    {
                        // Checks schema
                        FileProcessingDB.ResetDBConnection();
                        ResetFileSelectionSettings();
                        ResetSearch();
                        Show();
                        InitializeContextMenu();
                        GenerateFileList();
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

        #endregion Event Handlers

        #region Private Members

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
        /// Runs a database query to build the file list on a background thread.
        /// </summary>
        /// <param name="query">The query used to generate the file list.</param>
        /// <param name="cancelToken">The <see cref="CancellationToken"/> that should be checked
        /// after adding each file to the list to ensure the operation hasn't been canceled.</param>
        void RunDatabaseQuery(string query, CancellationToken cancelToken)
        {
            Recordset queryResults = null;

            try
            {
                queryResults = FileProcessingDB.GetResultsForQuery(query);

                _fileSelectionCount = 0;

                // If there are any query results, populate _resultsDataGridView.
                while (!queryResults.EOF)
                {
                    // Abort if the user cancelled.
                    cancelToken.ThrowIfCancellationRequested();

                    // Populate up to MaxFilesToDisplay in the file list, but iterate all
                    // results to obtain the overall number of files selected.
                    if (_fileSelectionCount < MaxFilesToDisplay)
                    {
                        // Retrieve the fields necessary for the results table.
                        string fileName = (string)queryResults.Fields["FileName"].Value;
                        var fileData = new FAMFileData(fileName);

                        string directory = Path.GetDirectoryName(fileName);
                        fileName = Path.GetFileName(fileName);
                        int pageCount = (int)queryResults.Fields["Pages"].Value;

                        // Invoke the new row to be added on the UI thread.
                        this.SafeBeginInvoke("ELI35725", () =>
                        {
                            _fileListDataGridView.Rows.Add(fileName, pageCount, fileData,
                                directory);
                        });
                    }

                    queryResults.MoveNext();
                    _fileSelectionCount++;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35726");
            }
            finally
            {
                if (queryResults != null)
                {
                    queryResults.Close();
                }
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
            var regexParsers = new List<DotNetRegexParser>();
            foreach (string searchTerm in searchTerms)
            {
                DotNetRegexParser regexParser = new DotNetRegexParser();
                if (searchTerm.StartsWith("~", StringComparison.Ordinal))
                {
                    regexParser.Pattern = searchTerm.Substring(1);
                }
                else
                {
                    regexParser.Pattern = Regex.Escape(searchTerm);
                }
                regexParser.RegexOptions |= RegexOptions.Compiled;
                regexParsers.Add(regexParser);
            }

            int missingOcrCount = 0;

            // Search each file in the file list.
            foreach (DataGridViewRow row in _fileListDataGridView.Rows)
            {
                // Abort if the user cancelled.
                cancelToken.ThrowIfCancellationRequested();

                // Obtain the OCR text for the file.
                FAMFileData rowData = row.GetFileData();
                rowData.ShowTextResults = true;
                SpatialString ocrText = rowData.OcrText;
                if (ocrText == null)
                {
                    missingOcrCount++;
                }
                else
                {
                    string fileText = ocrText.String;
                    List<Match> allMatches = new List<Match>();

                    // Initialize FileMatchesSearch depending on whether we are looking for any term.
                    rowData.FileMatchesSearch = (searchModifier != SearchModifier.Any);

                    // Search the OCR text with the parser for each search term.
                    foreach (DotNetRegexParser parser in regexParsers)
                    {
                        var matches = parser.Regex.Matches(fileText)
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
            }

            this.SafeBeginInvoke("ELI35828", () =>
            {
                if (missingOcrCount == 0)
                {
                    _searchErrorStatusStripLabel.Text = "";
                }
                else
                {
                    _searchErrorStatusStripLabel.Text = string.Format(CultureInfo.CurrentCulture,
                        "{0:D} file(s) have not been OCRed", missingOcrCount);
                }
            });
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
            var regexParsers = new Dictionary<string, DotNetRegexParser>();
            foreach (KeyValuePair<string, string> searchTerm in searchTerms)
            {
                DotNetRegexParser regexParser = new DotNetRegexParser();
                if (searchTerm.Value.StartsWith("~", StringComparison.Ordinal))
                {
                    regexParser.Pattern = searchTerm.Value.Substring(1);
                }
                else
                {
                    regexParser.Pattern = Regex.Escape(searchTerm.Value);
                }
                regexParser.RegexOptions |= RegexOptions.Compiled;
                regexParsers.Add(searchTerm.Key, regexParser);
            }

            int missingDataCount = 0;

            // Search each file in the file list.
            foreach (DataGridViewRow row in _fileListDataGridView.Rows)
            {
                // Abort if the user cancelled.
                cancelToken.ThrowIfCancellationRequested();

                // Obtain the VOA data for the file.
                FAMFileData rowData = row.GetFileData();
                rowData.ShowTextResults = false;
                IUnknownVector attributes = rowData.Attributes;
                if (attributes == null)
                {
                    missingDataCount++;
                }
                else
                {
                    var allMatches = new List<ThreadSafeSpatialString>();

                    // Initialize FileMatchesSearch depending on whether we are looking for any term.
                    rowData.FileMatchesSearch = (searchModifier != SearchModifier.Any);

                    // Search the specified attributes with the parser for each search term.
                    foreach (KeyValuePair<string, DotNetRegexParser> parser in regexParsers)
                    {
                        IEnumerable<ThreadSafeSpatialString> matches =
                            _afUtils.QueryAttributes(attributes, parser.Key, false)
                            .ToIEnumerable<IAttribute>()
                            .Select(attribute => attribute.Value)
                            .SelectMany(value => parser.Value.Regex.Matches(value.String)
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
            }

            this.SafeBeginInvoke("ELI35829", () =>
            {
                if (missingDataCount == 0)
                {
                    _searchErrorStatusStripLabel.Text = "";
                }
                else
                {
                    _searchErrorStatusStripLabel.Text = string.Format(CultureInfo.CurrentCulture,
                        "Rules have not been run on {0:D} file(s)", missingDataCount);
                }
            });
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
                    this.SafeBeginInvoke("ELI35722",
                        () =>
                        {
                            OperationIsActive = false;

                            foreach (Exception ex in task.Exception.InnerExceptions)
                            {
                                ex.ExtractDisplay("ELI35723");
                            }
                        }),
                        TaskContinuationOptions.OnlyOnFaulted);

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
                if (_queryTask.Wait(0))
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
                    _queryCanceler.Cancel();
                    try
                    {
                        _queryTask.Wait();
                    }
                    catch { }  // Ignore any exceptions; we don't care about this task anymore.
                }
            }

            OperationIsActive = false;
        }

        /// <summary>
        /// Launches the specified <see paramref="fileNames"/> in the application defined by
        /// <see paramref="appLaunchItem"/>.
        /// </summary>
        /// <param name="appLaunchItem">The <see cref="AppLaunchItem"/> defining the application to
        /// be run.</param>
        /// <param name="fileNames">The files to be run in <see paramref="appLaunchItem"/></param>
        void RunApplication(AppLaunchItem appLaunchItem, IEnumerable<string> fileNames)
        {
            try
            {
                // Collects any exceptions that occur when processing the files.
                var exceptions = new List<ExtractException>();

                // Process each filename in sequence.
                foreach (string fileName in fileNames)
                {
                    try
                    {
                        // Expand the command line arguments using path tags/functions.
                        SourceDocumentPathTags pathTags = new SourceDocumentPathTags(fileName);
                        string arguments = pathTags.Expand(appLaunchItem.Arguments);

                        if (appLaunchItem.SupportsErrorHandling)
                        {
                            SystemMethods.RunExtractExecutable(
                                appLaunchItem.ApplicationPath, arguments);
                        }
                        else
                        {
                            SystemMethods.RunExecutable(
                                appLaunchItem.ApplicationPath, arguments, int.MaxValue);
                        }
                    }
                    catch (Exception ex)
                    {
                        exceptions.Add(ex.AsExtract("ELI35820"));
                    }
                }

                if (exceptions.Count > 0)
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
                        exceptions.Add(new ExtractException("ELI35819",
                            string.Format(CultureInfo.CurrentCulture,
                            "{0:D} file(s) failed {1}", exceptions.Count, appLaunchItem.Name.Quote())));
                        throw ExtractException.AsAggregateException(exceptions);
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35811");

                // If there was an error launching a non-blocking app, ensure the status label is
                // returned
                if (!appLaunchItem.Blocking)
                {
                    this.SafeBeginInvoke("ELI35815", () => UpdateStatusLabel());
                }
            }
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
            if (rowToUpdate.Visible && _fileListDataGridView.SelectedRows.Count == 1)
            {
                if (_fileListDataGridView.SelectedRows[0] == rowToUpdate &&
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
                // Open the image associated with fileData and highlight all search terms found int
                // it.
                OpenImage(fileData);
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
            .ContinueWith((task) => _pendingOpenImageCount--,
                TaskContinuationOptions.OnlyOnFaulted);
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
                        _imageViewer.OpenImage(fileData.FileName, false, false);

                        // Display highlights for all search terms found in the selected file.
                        ShowMatchHighlights(fileData);
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

                // Select the first or last search result in the document depending on
                // _selectLastMatchByDefault.
                if (_selectLastMatchByDefault)
                {
                    // Can't use GoToPreviousVisibleLayerObject, because it will select the last in
                    // the current view rather than the last in the document.
                    LayerObject lastMatch = _imageViewer.LayerObjects
                        .GetSortedCollection()
                        .LastOrDefault();
                    if (lastMatch != null)
                    {
                        _imageViewer.LayerObjects.Selection.Clear();
                        _imageViewer.CenterOnLayerObjects(lastMatch);
                        lastMatch.Selected = true;
                    }
                }
                else if (_imageViewer.CanGoToNextLayerObject)
                {
                    _imageViewer.GoToNextVisibleLayerObject(true);
                }
            }
        }

        /// <summary>
        /// Updates the file selection summary label.
        /// </summary>
        void UpdateFileSelectionSummary()
        {
            Text = _APPLICATION_TITLE;
            if (FileProcessingDB.IsConnected)
            {
                Text = DatabaseName + " on " + DatabaseServer + " - " + Text;
            }

            string summaryText = FileSelector.GetSummaryString();
            _selectFilesSummaryLabel.Text = "Listing ";
            _selectFilesSummaryLabel.Text +=
                summaryText.Substring(0, 1).ToLower(CultureInfo.CurrentCulture);
            _selectFilesSummaryLabel.Text += summaryText.Substring(1);
        }

        #endregion Private Members
    }
}
