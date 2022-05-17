using DevExpress.DashboardCommon;
using DevExpress.DashboardCommon.ViewerData;
using DevExpress.DashboardWin;
using DevExpress.DashboardWin.Native;
using DevExpress.XtraEditors;
using DevExpress.XtraGrid;
using DevExpress.XtraGrid.Views.Grid;
using Extract.Dashboard.Forms;
using Extract.Dashboard.Utilities;
using Extract.Licensing;
using Extract.SqlDatabase;
using Extract.Utilities;
using Extract.Utilities.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Extract.DashboardViewer
{
    public partial class DashboardViewerForm : XtraForm, IExtractDashboardCommon
    {
        #region Constants

        /// <summary>
        /// The database schema version that the Dashboard table was added
        /// </summary>
        int _VersionDashboardTableAdded = 164;

        #endregion

        #region Private fields

        /// <summary>
        /// Server name to override server name in dashboard config
        /// if empty or null dashboard will be opened with configured server
        /// </summary>
        string _serverName;

        /// <summary>
        /// Database name to override database name in dashboard config
        /// if empty or null dashboard will be opened with configured database
        /// </summary>
        string _databaseName;

        /// <summary>
        /// Schema version for the active database
        /// </summary>
        int _databaseVersion = 0;

        /// <summary>
        /// Instance of <see cref="DashboardShared{T}"/> that contains shared code between Creator and Viewer
        /// </summary>
        DashboardShared<DashboardViewerForm> _dashboardShared;

        /// <summary>
        /// Name of the dashboard being displayed from the database
        /// </summary>
        string _dashboardName = String.Empty;

        /// <summary>
        /// If <c>true</c> _dashboardName is from the database, if <c>false</c> represents a filename
        /// </summary>
        bool _inDatabase = false;

        // Set that contains all of the items that are filtered
        HashSet<string> _filteredItems = new HashSet<string>();

        // if true indicates that the dashboard is using definition that is using extracted data
        bool _usingCachedData = false;

        /// <summary>
        /// Manages the temporary files that are created for each extracted datasource file so that the original can be
        /// updated without blocking.
        /// </summary>
        TemporaryFileCopyManager _temporaryDatasourceFileCopyManager = new TemporaryFileCopyManager();

        /// <summary>
        /// Dictionary to map the datasource with the Original filename that the extracted datasource uses
        /// </summary>
        Dictionary<object, string> _dictionaryDataSourceToFileName = new Dictionary<object, string>();

        /// <summary>
        /// This will contain a copy of the dashboard as saved in the database. It will be null if not using cached data
        /// </summary>
        DevExpress.DashboardCommon.Dashboard _dashboardForExtractedFileUpdate;

        /// <summary>
        /// The modified times for the extracted data files
        /// </summary>
        DateTime _extractedDataFileModifiedTime;

        /// <summary>
        /// Background work that is used to update the cached data
        /// </summary>
        BackgroundWorker _backgroundWorkerForUpdate = new BackgroundWorker();

        /// <summary>
        /// Reset event used to signal when the update is finished which will close the update wait dialog
        /// </summary>
        ManualResetEvent _updateResetEvent = new ManualResetEvent(true);

        /// <summary>
        /// Form displayed when the cached data is being updated
        /// </summary>
        PleaseWaitForm _updateWaitForm;

        #endregion

        #region Private properties

        /// <summary>
        /// Property to indicate if dashboard can be selected from the active database
        /// </summary>
        bool AllowDatabaseDashboardSelection
        {
            get
            {
                try
                {
                    if (_databaseVersion == 0 && IsDatabaseOverridden)
                    {
                        using var connection = new ExtractRoleConnection(ServerName, DatabaseName);
                        connection.Open();

                        using var command = connection.CreateCommand();
                        command.CommandText = "SELECT [Value] FROM DBInfo WHERE [Name] = 'FAMDBSchemaVersion'";
                        int version;
                        if (int.TryParse(command.ExecuteScalar() as string, out version))
                        {
                            _databaseVersion = version;
                        }
                        else
                        {
                            _databaseVersion = 0;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _databaseVersion = 0;
                    throw ex.AsExtract("ELI45774");
                }
                return _databaseVersion >= _VersionDashboardTableAdded;
            }
        }

        #endregion

        #region IExtractDashboardCommon Implementation

        public string ApplicationName { get; } = "Extract Dashboard Viewer";

        /// <summary>
        /// Gets the active dashboard from the underlying control
        /// </summary>
        public DevExpress.DashboardCommon.Dashboard CurrentDashboard
        {
            get
            {
                return dashboardViewerMain.Dashboard;
            }
        }

        /// <summary>
        /// Dictionary to track drill down level for Dashboard controls
        /// </summary>
        public Dictionary<string, int> DrilldownLevelForItem { get; } = new Dictionary<string, int>();

        /// <summary>
        /// Tracks if the Drill down level has increased for the control
        /// </summary>
        public Dictionary<string, bool> DrilldownLevelIncreased { get; } = new Dictionary<string, bool>();

        /// <summary>
        /// The server name to use for the Dashboard
        /// </summary>
        public string ServerName
        {
            get
            {
                return IsDatabaseOverridden ? _serverName : ServerNameFromDefinition; ;
            }
            set
            {
                _serverName = value;
            }
        }

        /// <summary>
        /// The DatabaseName to use for the dashboard
        /// </summary>
        public string DatabaseName
        {
            get
            {
                return IsDatabaseOverridden ? _databaseName : DatabaseNameFromDefinition;
            }

            set
            {
                _databaseName = value;
            }
        }

        /// <summary>
        /// The Server configured in the Dashboard
        /// </summary>
        public string ServerNameFromDefinition { get; set; }

        /// <summary>
        /// The Database configured in the Dashboard
        /// </summary>
        public string DatabaseNameFromDefinition { get; set; }

        /// <summary>
        /// Indicates that the Server and DatabaseName have been overridden
        /// </summary>
        public bool IsDatabaseOverridden
        {
            get
            {
                return !(string.IsNullOrWhiteSpace(_serverName) || string.IsNullOrWhiteSpace(_databaseName));
            }
        }

        /// <summary>
        /// List of files that were selected in the control when the Popup was 
        /// displayed
        /// </summary>
        public HashSet<string> CurrentFilteredFiles { get; } = new HashSet<string>();

        /// <summary>
        /// Since this has a <see cref="DevExpress.DashboardWin.DashboardViewer"/> return the instance of the viewer
        /// </summary>
        public DevExpress.DashboardWin.DashboardViewer Viewer => dashboardViewerMain;

        /// <summary>
        /// Since this does not have <see cref="DevExpress.DashboardWin.DashboardDesigner"/> return null
        /// </summary>
        public DashboardDesigner Designer { get; } = null;

        /// <summary>
        /// The key value pairs for the currently filter dimension selected in the grid
        /// </summary>
        public Dictionary<string, object> CurrentFilteredDimensions { get; } = new Dictionary<string, object>();

        /// <summary>
        /// Gets the current filtered values for the named dashboard item
        /// </summary>
        /// <param name="dashboardItemName">Dashboard item name</param>
        /// <returns>List of current <see cref="AxisPointTuple"/>s for the named control</returns>
        public IList<AxisPointTuple> GetCurrentFilterValues(string dashboardItemName)
        {
            return dashboardViewerMain.GetCurrentFilterValues(dashboardItemName);
        }

        /// <summary>
        /// Calls the <see cref="FormsExtensionMethods.SafeBeginInvoke(Control, string, Action, bool, Action{Exception})"/> 
        /// the specified <see paramref="action"/> asynchronously within a try/catch handler
        /// that will display any exceptions.
        /// </summary>
        /// <param name="eliCode">The ELI code to associate with any exception.</param>
        /// <param name="action">The <see cref="Action"/> to be invoked.</param>
        /// <param name="displayExceptions"><see langword="true"/> to display any exception caught;
        /// <see langword="false"/> to log instead.</param>
        /// <param name="exceptionAction">A second action that should be executed in the case of an
        /// exception an exception in <see paramref="action"/>.</param>
        public void SafeBeginInvokeForShared(string eliCode, Action action,
            bool displayExceptions = true, Action<Exception> exceptionAction = null)
        {
            this.SafeBeginInvoke(eliCode, action, displayExceptions, exceptionAction);
        }

        /// <summary>
        /// Opens a dashboard viewer with the given dashboard name and the filter data
        /// </summary>
        /// <param name="dashboardName">This will be assumed another dashboard in the current database for the open dashboard </param>
        /// <param name="filterData">The dictionary contains the filter data</param>
        public void OpenDashboardForm(string dashboardName, Dictionary<string, object> filterData)
        {
            try
            {
                bool isFile = string.IsNullOrWhiteSpace(Path.GetExtension(dashboardName));
                DashboardViewerForm form = new DashboardViewerForm(dashboardName, isFile, ServerName, DatabaseName, filterData);
                form.Show();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI47066");
            }
        }

        #endregion

        #region IDisposable implementation

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            if (disposing)
            {
                _temporaryDatasourceFileCopyManager?.Dispose();
                _temporaryDatasourceFileCopyManager = null;
                _dashboardShared?.Dispose();
                _dashboardShared = null;
                _dashboardForExtractedFileUpdate?.Dispose();
                _dashboardForExtractedFileUpdate = null;
                _backgroundWorkerForUpdate?.Dispose();
                _backgroundWorkerForUpdate = null;
                _updateWaitForm?.Dispose();
                _updateWaitForm = null;
                _updateResetEvent?.Dispose();
                _updateResetEvent = null;
            }
            base.Dispose(disposing);
        }

        #endregion

        #region Public properties

        /// <summary>
        /// Parameter values that will be assigned to parameters that are setup in a dashboard being opened
        /// </summary>
        public Dictionary<string, object> ParameterValues { get; } = new Dictionary<string, object>();

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public DashboardViewerForm()
        {
            InitializeComponent();

            _dashboardShared = new DashboardShared<DashboardViewerForm>(this, true);
            this.Viewer.ValidateCustomSqlQuery += DashboardHelpers.HandleDashboardCustomSqlQuery;
        }

        /// <summary>
        /// Constructs DashboardViewerForm and opening the given file
        /// </summary>
        /// <param name="fileName">File containing dashboard to open</param>
        public DashboardViewerForm(string fileName)
            : this()
        {
            try
            {
                _dashboardName = fileName;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45310");
            }
        }

        /// <summary>
        /// Constructs DashboardViewerForm and opening indicated dashboard using the server and database.
        /// </summary>
        /// <param name="dashboard">File or name of dashboard in database to open</param>
        /// <param name="inDatabase">Specifies that the dashboard is to be loaded from the database</param>
        /// <param name="serverName">Server name to use when opening a dashboard.</param>
        /// <param name="databaseName">Database name to use when opening a dashboard</param>
        public DashboardViewerForm(string dashboard, bool inDatabase, string serverName, string databaseName)
            : this()
        {
            try
            {
                ServerName = serverName;
                DatabaseName = databaseName;
                _dashboardName = dashboard;
                _inDatabase = inDatabase;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI50063");
            }
        }

        /// <summary>
        /// Constructs DashboardViewerForm and opening indicated dashboard using the server and database.
        /// </summary>
        /// <param name="dashboard">File or name of dashboard in database to open</param>
        /// <param name="inDatabase">Specifies that the dashboard is to be loaded from the database</param>
        /// <param name="serverName">Server name to use when opening a dashboard.</param>
        /// <param name="databaseName">Database name to use when opening a dashboard</param>
        public DashboardViewerForm(string dashboard, bool inDatabase, string serverName, string databaseName, Dictionary<string, object> filterValues)
            : this()
        {
            try
            {
                ServerName = serverName;
                DatabaseName = databaseName;
                _dashboardName = dashboard;
                _inDatabase = inDatabase;
                ParameterValues.AddRange(filterValues);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI50064");
            }
        }

        /// <summary>
        /// Constructs DashboardViewerForm that allows dashboards to be opened from the database
        /// </summary>
        /// <param name="serverName">Server name to use when opening a dashboard.</param>
        /// <param name="databaseName">Database name to use when opening a dashboard</param>
        public DashboardViewerForm(string serverName, string databaseName)
            : this()
        {
            try
            {
                ServerName = serverName;
                DatabaseName = databaseName;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45754");
            }
        }

        #endregion

        #region Event Handlers
   
        private void HandleDashboardsInDBListBoxControl_CustomItemTemplate(object sender, CustomItemTemplateEventArgs e)
        {
            try
            {
                var data = e.Item as SourceLink;
                if (data?.IsFile == true)
                {
                    e.Template = e.Templates["CoreDashboardTemplate"];
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI49923");
            }
        }

        void DashboardViewerForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                e.Cancel = !_dashboardShared.RequestDashboardClose();
                if (!e.Cancel && _dictionaryDataSourceToFileName.Count > 0)
                {
                    // Dispose of the dashboard so any extracted data sources will be closed
                    DisposeOfDashboardsAndDereferenceTempFiles();
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI46220");
            }
        }

        void HandlePopupMenuShowing(object sender, DashboardPopupMenuShowingEventArgs e)
        {
            try
            {
                _dashboardShared.HandlePopupMenuShowing(sender, e);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI46206");
            }
        }

        #region Menu item event handlers
        void HandleToolStripButtonClearMasterFilterClick(object sender, EventArgs e)
        {
            try
            {
                foreach (var item in dashboardViewerMain.Dashboard.Items)
                {
                    if (dashboardViewerMain.CanClearMasterFilter(item.ComponentName))
                    {
                        dashboardViewerMain.ClearMasterFilter(item.ComponentName);
                    }

                    // Clear any column search filters
                    var grid = ((IUnderlyingControlProvider)dashboardViewerMain).GetUnderlyingControl(item.ComponentName) as GridControl;
                    var gridView = grid?.MainView as GridView;
                    if (gridView != null)
                    {
                        gridView.ActiveFilter.Clear();
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI46434");
            }
        }

        void HandleToolStripButtonRefresh_Click(object sender, EventArgs e)
        {
            try
            {
                if (CurrentDashboard != null) 
                {
                    bool reloadData = !_usingCachedData || _dictionaryDataSourceToFileName
                        .Any(entry => _temporaryDatasourceFileCopyManager.HasFileBeenModified(entry.Value));

                    if (_usingCachedData && !reloadData)
                    {
                        if (MessageBox.Show(string.Empty +
                            "Cached data source needs to be updated from the database. Updating from the database can take time. Refresh anyway?",
                            "Update Cached",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Exclamation,
                            MessageBoxDefaultButton.Button1,
                            (MessageBoxOptions)0) != DialogResult.Yes)
                        {
                            return;
                        }
                        SetupBackgroundWorkerForCacheUpdate();
                        _updateResetEvent.Reset();
                        _backgroundWorkerForUpdate.RunWorkerAsync();
                        _updateWaitForm.ShowDialog();
                    }
                    else if (reloadData)
                    {
                        ReloadData();
                        _toolStripTextBoxlastRefresh.Text = (_usingCachedData) ?
                            _extractedDataFileModifiedTime.ToString(CultureInfo.CurrentCulture)
                            : DateTime.Now.ToString(CultureInfo.CurrentCulture);
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI46235");
            }
        }

        void HandleDashboardsInDBListBoxControlMouseDoubleClick(object sender, MouseEventArgs e)
        {
            try
            {
                if (_dashboardsInDBListBoxControl.SelectedIndex >= 0)
                {
                    var selected = _dashboardsInDBListBoxControl.SelectedValue as string;
                    if (string.IsNullOrWhiteSpace(selected))
                        return;

                    // Set empty dashboard to clear previous parameters 
                    // https://extract.atlassian.net/browse/ISSUE-17169
                    DisposeOfDashboardsAndDereferenceTempFiles();

                    if (Path.GetExtension(selected).Equals(".esdx", StringComparison.OrdinalIgnoreCase))
                    {
                        _dashboardName = selected;
                        var xdoc = XDocument.Load(_dashboardName);
                        
                        dashboardViewerMain.Dashboard = LoadDashboardFromXDocument(xdoc);
                        UpdateMainTitle();
                    }
                    else
                    {
                        LoadDashboardFromDatabase(_dashboardsInDBListBoxControl.SelectedValue as string);
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45772");
            }
            finally
            {
                dashboardFlyoutPanel.HidePopup();
            }
        }

        void HandleDashboardToolStripMenuItemClick(object sender, EventArgs e)
        {
            try
            {
                // Toggle the dashboard fly-out panel - if it is displayed hide it if it is not displayed show it
                if (dashboardFlyoutPanel.IsPopupOpen)
                {
                    dashboardFlyoutPanel.HidePopup();
                }
                else
                {
                    LoadDashboardList();
                    if (_dashboardsInDBListBoxControl.ItemCount > 0)
                    {
                        dashboardFlyoutPanel.ShowPopup();
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45771");
            }
        }

        void HandleCloseToolStripMenuItemClick(object sender, EventArgs e)
        {
            try
            {
                // Clear the existing dashboard
                _dashboardName = string.Empty;
                dashboardViewerMain.DashboardSource = string.Empty;
                _toolStripTextBoxlastRefresh.Text = string.Empty;
                UpdateMainTitle();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45308");
            }
        }

        void HandleOpenToolStripMenuItemClick(object sender, EventArgs e)
        {
            try
            {
                FileBrowser fileBrowser = new FileBrowser();
                string selectedFile = fileBrowser.BrowseForFile("ESDX|*.esdx|XML|*.xml|All|*.*", string.Empty);
                if (!string.IsNullOrWhiteSpace(selectedFile) && File.Exists(selectedFile))
                {
                    DisposeOfDashboardsAndDereferenceTempFiles();
                    
                    _dashboardName = selectedFile;
                    var xdoc = XDocument.Load(_dashboardName);

                    dashboardViewerMain.Dashboard = LoadDashboardFromXDocument(xdoc);
                    UpdateMainTitle();
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45309");
            }
        }

        void HandleExitToolStripMenuItemClick(object sender, EventArgs e)
        {
            try
            {
                Close();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45641");
            }
        }

        #endregion

        #region DashboardViewer event handlers

        private void HandleDashboardViewerMain_DataLoadingError(object sender, DataLoadingErrorEventArgs e)
        {
            try
            {
                _dashboardShared.HandleDataLoadingError(sender, e);
            }
            catch (Exception ex)
            {
                ex.ExtractLog("ELI53009");
            }
        }

        private void HandleDashboardView_CustomParameters(object sender, CustomParametersEventArgs e)
        {
            try
            {
                var dataSources = dashboardViewerMain.Dashboard.DataSources.OfType<DashboardSqlDataSource>();
                foreach (var ds in dataSources)
                {
                    ds.ConnectionParameters = ds.ConnectionParameters
                        .CreateConnectionParametersForReadOnly(ServerName, DatabaseName, ApplicationName);
                    if (ds.Queries.Any(q => q.Name == ds.AppRoleQueryName()))
                    {
                        ds.Connection.Close();
                        ds.Fill(ds.AppRoleQueryName());
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractLog("ELI51915");
            }

        }

        void HandleDashboardViewerMainMasterFilterCleared(object sender, MasterFilterClearedEventArgs e)
        {
            try
            {
                if (_filteredItems.Contains(e.DashboardItemName))
                {
                    _filteredItems.Remove(e.DashboardItemName);
                }
                UpdateMainTitle();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI46436");
            }
        }

        void HandleDashboardViewerMainMasterFilterSet(object sender, MasterFilterSetEventArgs e)
        {
            try
            {
                if (!_filteredItems.Contains(e.DashboardItemName))
                {
                    _filteredItems.Add(e.DashboardItemName);
                }
                UpdateMainTitle();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI46435");
            }
        }

        void HandleDashboardViewerMainDrillDownPerformed(object sender, DrillActionEventArgs e)
        {
            try
            {
                DrilldownLevelForItem[e.DashboardItemName] = e.DrillDownLevel;
                DrilldownLevelIncreased[e.DashboardItemName] = true;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45728");
            }
        }
        void HandleDashboardViewerMainDrillUpPerformed(object sender, DrillActionEventArgs e)
        {
            try
            {
                DrilldownLevelForItem[e.DashboardItemName] = e.DrillDownLevel;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45729");
            }
        }
        void HandleDashboardViewerMainDashboardItemDoubleClick(object sender, DashboardItemMouseActionEventArgs e)
        {
            try
            {
                _dashboardShared.HandleGridDashboardItemDoubleClick(sender, e);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45696");
            }
        }

        void HandleDashboardViewerMainConfigureDataConnection(object sender, DashboardConfigureDataConnectionEventArgs e)
        {
            try
            { 
                _dashboardShared.HandleConfigureDataConnection(sender, e);

                UpdateMainTitle();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45640");
            }
        }

        void HandleDashboardViewerMainDashboardChanged(object sender, EventArgs e)
        {
            try
            {
                // Clear the filtered items list since they are no longer filtered
                _filteredItems.Clear();

                UpdateMainTitle();
                
                _toolStripTextBoxlastRefresh.Text = (_usingCachedData) ?
                        _extractedDataFileModifiedTime.ToString(CultureInfo.CurrentCulture)
                        : DateTime.Now.ToString(CultureInfo.CurrentCulture);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45311");
            }
        }

        protected override void OnLoad(EventArgs e)
        {
            try
            {
                base.OnLoad(e);
                
                if (_inDatabase)
                {
                    if (!string.IsNullOrEmpty(_dashboardName))
                    {
                        LoadDashboardFromDatabase(_dashboardName);
                    }
                }
                else
                {
                    if (!string.IsNullOrEmpty(_dashboardName))
                    {
                        DisposeOfDashboardsAndDereferenceTempFiles();

                        var xdoc = XDocument.Load(_dashboardName);

                        dashboardViewerMain.Dashboard = LoadDashboardFromXDocument(xdoc);
                    }
                }

                if (!string.IsNullOrWhiteSpace(DatabaseName) &&
                    !string.IsNullOrWhiteSpace(ServerName) &&
                    string.IsNullOrWhiteSpace(_dashboardName))
                {
                    // Toggle the dashboard fly-out panel - if it is displayed hide it if it is not displayed show it
                    if (!dashboardFlyoutPanel.IsPopupOpen)
                    {
                        LoadDashboardList();
                        if (_dashboardsInDBListBoxControl.ItemCount > 0)
                        {
                            dashboardFlyoutPanel.ShowPopup();
                        }
                    }
                }

                dashboardToolStripMenuItem.Visible = AllowDatabaseDashboardSelection;
                fileToolStripMenuItem.Visible = !_inDatabase || !AllowDatabaseDashboardSelection;

                UpdateMainTitle();

                this.Focus();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI46428");
            }
        }

        #endregion

        #endregion

        #region Helper methods

        /// <summary>
        /// Loads the given dashboard fr0m the database
        /// </summary>
        /// <param name="dashboardName">Name of dashboard to load</param>
        void LoadDashboardFromDatabase(string dashboardName)
        {
            if (string.IsNullOrWhiteSpace(dashboardName))
            {
                return;
            }

            // Clear the filtered items since they will no longer be filtered
            _filteredItems.Clear();

            using var connection = new ExtractRoleConnection(ServerName, DatabaseName);
            connection.Open();
            using var cmd = connection.CreateCommand();
            cmd.CommandText = @"
                SELECT 
                        CASE
                            WHEN [UseExtractedData] = 0
                                OR [ExtractedDataDefinition] IS NULL
                            THEN [Definition]
                            ELSE [ExtractedDataDefinition]
                        END
                        AS [Definition],
                        CASE
                            WHEN [UseExtractedData] = 0
                                OR [ExtractedDataDefinition] IS NULL
                            THEN 0
                            ELSE 1
                        END
                        AS UseExtractedData
                FROM [dbo].[Dashboard]
                WHERE [DashboardName] = @DashboardName";
            cmd.Parameters.AddWithValue("@DashboardName", dashboardName);
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                UpdateMainTitle();

                var xdoc = XDocument.Load(reader.GetXmlReader(0), LoadOptions.None);

                // Set empty dashboard to clear previous parameters 
                // https://extract.atlassian.net/browse/ISSUE-17169
                DisposeOfDashboardsAndDereferenceTempFiles();
                dashboardViewerMain.Dashboard = LoadDashboardFromXDocument(xdoc);
                _dashboardName = dashboardName;
            }
            UpdateMainTitle();
        }

        /// <summary>
        /// Load the dashboard from a XDocument
        /// </summary>
        /// <param name="xdoc">The XDocument that has the dashboard definition</param>
        /// <returns>The new dashboard object loaded from the <paramref name="xdoc"/></returns>
        DevExpress.DashboardCommon.Dashboard LoadDashboardFromXDocument(XDocument xdoc)
        {
            _dashboardShared.CustomData.AssignDataFromDashboardDefinition(xdoc);

            LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());
            bool isLicensed = LicenseUtilities.IsLicensed(LicenseIdName.DashboardViewer);
            
            if (!isLicensed && !_dashboardShared.CustomData.CoreLicensed)
            {
                ExtractException ee = new ExtractException("ELI49740", "Dashboards are not licensed.");
                throw ee;
            }

            var dashboard = new DevExpress.DashboardCommon.Dashboard();
            dashboard.LoadFromXDocument(xdoc);

            var ds = dashboard.DataSources.OfType<DashboardSqlDataSource>().FirstOrDefault();

            if (ds != null )
            {
                _dashboardShared.UpdateDatabaseFromDefinition(ds.ConnectionParameters);
            }

            if (!string.IsNullOrWhiteSpace(ServerName) && !string.IsNullOrWhiteSpace(DatabaseName))
            {
                using var appConfig = new AppRoleConfig(SqlUtil.CreateConnectionString(ServerName, DatabaseName));
                appConfig.AddAppRoleQuery(dashboard);
            }

            ApplyParameterValues(dashboard);
            ReplaceExtractDataSourceFileWithTemporary(dashboard);
            if (_dictionaryDataSourceToFileName.Count > 0)
            {
                 _dashboardForExtractedFileUpdate = new DevExpress.DashboardCommon.Dashboard();
                _dashboardForExtractedFileUpdate.LoadFromXDocument(xdoc);
                _usingCachedData = true;
            }

            return dashboard;
        }

        /// <summary>
        /// Sets the Dashboard parameter values to the values in the ParameterValues dictionary
        /// </summary>
        /// <param name="dashboard"></param>
        void ApplyParameterValues(DevExpress.DashboardCommon.Dashboard dashboard)
        {
            if (ParameterValues.Count == 0)
            {
                return;
            }
            foreach (var parameter in dashboard.Parameters)
            {
                if (ParameterValues.TryGetValue(parameter.Name, out object value))
                {
                    parameter.Value = value;
                }
            }
        }

        /// <summary>
        /// Update the main title to show the loaded dashboard and database and server 
        /// </summary>
        void UpdateMainTitle()
        {
            bool filtered = _filteredItems.Count > 0;
            toolStripButtonClearMasterFilter.Enabled = CurrentDashboard != null;
            if (CurrentDashboard is null)
            {
                if (!IsDatabaseOverridden)
                {
                    Text = "Dashboard viewer";
                }
                else
                {
                    Text = string.Format(CultureInfo.InvariantCulture,
                        "Dashboard viewer Using {0} on {1}", _databaseName, _serverName);
                }
            }
            else if (string.IsNullOrEmpty(_dashboardName))
            {
                Text = string.Format(CultureInfo.InvariantCulture,
                    "Using {0} on {1}", DatabaseName, ServerName);
            }
            else
            {
                Text = string.Format(CultureInfo.InvariantCulture,
                    "{4}\"{0}\" - Using {1} on {2}{3}",
                    _dashboardName,
                    DatabaseName,
                    ServerName,
                    (filtered) ? "-Filtered" : string.Empty,
                    (_usingCachedData) ? "Cached " : string.Empty);
            }
        }

        /// <summary>
        /// Loads the list displayed by the dashboard flyout from the Dashboard table of the database
        /// </summary>
        void LoadDashboardList()
        {
            try
            {
                if (!IsDatabaseOverridden || !AllowDatabaseDashboardSelection)
                {
                    return;
                }
                var filters = GetFiltersFromDatabase();
                _dashboardsInDBListBoxControl.DataSource = _dashboardShared.DashboardList()
                    .FilterWithRegex(filters.includeFilter, filters.excludeFilter, d => d.SourceName).ToList();

                _dashboardsInDBListBoxControl.DisplayMember = "DisplayName";
                _dashboardsInDBListBoxControl.ValueMember = "SourceName";
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI50075");
            }
        }

        /// <summary>
        /// Changes all the Extracted datasources
        /// </summary>
        /// <param name="dashboard"></param>
        void ReplaceExtractDataSourceFileWithTemporary(DevExpress.DashboardCommon.Dashboard dashboard)
        {
            try
            {
                // Find the existing data sources
                var existingExtractDataSources = dashboard.DataSources
                    .OfType<DashboardExtractDataSource>();

                foreach (var eds in existingExtractDataSources)
                {
                    _extractedDataFileModifiedTime = File.GetLastWriteTime(eds.FileName);

                    _dictionaryDataSourceToFileName.Add(eds, eds.FileName);
                    eds.FileName = _temporaryDatasourceFileCopyManager.GetCurrentTemporaryFileName(
                        eds.FileName, eds, true, false);
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI46917");
            }
        }

        /// <summary>
        /// Reload the data for the dashboard
        /// </summary>
        void ReloadData()
        {
            try
            {
	             // If there is cached data load the dashboard from the saved _dashboardForExtractedFileUpdate which
	             // is the original definition
	             if (_dashboardForExtractedFileUpdate != null)
	             {
	                 dashboardViewerMain.Dashboard = LoadDashboardFromXDocument(_dashboardForExtractedFileUpdate.SaveToXDocument());
	             }
	             else
	             {
	                 var sources = dashboardViewerMain.Dashboard?
	                     .DataSources?
	                     .OfType<DashboardSqlDataSource>();
	
	                 foreach (var ds in sources)
	                 {
	                     ds.Connection.Close();
	                     var approleQuery = ds.Queries.FirstOrDefault(q => q.Name == ds.AppRoleQueryName());
	                     if (approleQuery is null && string.IsNullOrWhiteSpace(ServerName) && string.IsNullOrWhiteSpace( DatabaseName))
	                     {
                            using var appConfig = new AppRoleConfig(SqlUtil.CreateConnectionString(ServerName, DatabaseName));
                            appConfig.AddAppRoleQuery(ds);
	                     }
	                 }
	
	                 // Just reload the data
	                 dashboardViewerMain.ReloadData(false);
	             }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI47187");
            }
        }

        /// <summary>
        /// Clear the dashboard that is currently being displayed and all associated data
        /// </summary>
        void DisposeOfDashboardsAndDereferenceTempFiles()
        {
            if (dashboardViewerMain != null)
            {
                dashboardViewerMain.Dashboard?.Parameters.Clear();
                dashboardViewerMain.DashboardSource = string.Empty;
                dashboardViewerMain.Dashboard?.Dispose();
                dashboardViewerMain.Dashboard = null;
            }
            if (_dashboardForExtractedFileUpdate != null)
            {
                _dashboardForExtractedFileUpdate.Parameters.Clear();
                _dashboardForExtractedFileUpdate.Dispose();
                _dashboardForExtractedFileUpdate = null;
            }

            foreach (var entry in _dictionaryDataSourceToFileName)
            {
                _temporaryDatasourceFileCopyManager.Dereference(entry.Value, entry.Key);
            }

            _dictionaryDataSourceToFileName.Clear();
            _usingCachedData = false;
        }

        /// <summary>
        /// Setup the background worker for updating the cached data
        /// </summary>
        void SetupBackgroundWorkerForCacheUpdate()
        {
            _backgroundWorkerForUpdate?.Dispose();
            _backgroundWorkerForUpdate = new BackgroundWorker();

            _updateWaitForm = new PleaseWaitForm("Updating cached data", _updateResetEvent);
            _backgroundWorkerForUpdate.DoWork += (object sender, DoWorkEventArgs workEventArgs) =>
            {
                DashboardDataConverter.UpdateExtractedDataSources(_dashboardForExtractedFileUpdate, CancellationToken.None);
            };

            _backgroundWorkerForUpdate.RunWorkerCompleted += (object sender, RunWorkerCompletedEventArgs completedArgs) =>
            {
                try
                {
                    // this should trigger the close of the dialog
                    _updateResetEvent.Set();
                    _updateWaitForm.Visible = false;

                    if (completedArgs.Error != null)
                    {
                        completedArgs.Error.ExtractDisplay("ELI47222");
                    }

                    this.Focus();
                    this.BringToFront();

                    ReloadData();
                    _toolStripTextBoxlastRefresh.Text = (_usingCachedData) ?
                        _extractedDataFileModifiedTime.ToString(CultureInfo.CurrentCulture)
                        : DateTime.Now.ToString(CultureInfo.CurrentCulture);
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI47225");
                }
                finally
                {
                    // this should trigger the close of the dialog
                    _updateResetEvent?.Set();
                    if (_updateWaitForm != null)
                    {
                        _updateWaitForm.Visible = false;
                    }
                    _backgroundWorkerForUpdate?.Dispose();
                    _backgroundWorkerForUpdate = null;
                }

            };
        }

        private (string includeFilter, string excludeFilter) GetFiltersFromDatabase()
        {
            try
            {
                using var connection = new ExtractRoleConnection(ServerName, DatabaseName);
                connection.Open();
                using var cmd = connection.CreateCommand();

                cmd.CommandText = "SELECT Value FROM DBInfo WHERE [Name] = @SettingName";
                cmd.Parameters.AddWithValue("@SettingName", "DashboardIncludeFilter");
                var includeFilter = cmd.ExecuteScalar() as string ?? "";
                includeFilter = string.IsNullOrWhiteSpace(includeFilter) ? "(?=)" : includeFilter;
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@SettingName", "DashboardExcludeFilter");
                var excludeFilter = cmd.ExecuteScalar() as string ?? "";
                excludeFilter = string.IsNullOrWhiteSpace(excludeFilter) ? "(?!)" : excludeFilter;

                return (includeFilter, excludeFilter);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI49940");
            }
        }

        #endregion
    }
}
