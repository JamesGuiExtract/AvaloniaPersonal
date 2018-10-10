using DevExpress.DashboardCommon;
using DevExpress.DashboardCommon.ViewerData;
using DevExpress.DashboardWin;
using DevExpress.XtraEditors;
using Extract;
using Extract.Dashboard.Utilities;
using Extract.Utilities.Forms;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;

namespace DashboardViewer
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
                        using (var connection = NewSqlDBConnection())
                        {
                            connection.Open();
                            var command = connection.CreateCommand();
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

        /// <summary>
        /// Gets the active dashboard from the underlying control
        /// </summary>
        public Dashboard Dashboard
        {
            get
            {
                return dashboardViewerMain.Dashboard;
            }
        }

        /// <summary>
        /// Dictionary to track drill down level for Dashboard controls
        /// </summary>
        public Dictionary<string, int> DrillDownLevelForItem { get; } = new Dictionary<string, int>();

        /// <summary>
        /// Tracks if the Drill down level has increased for the control
        /// </summary>
        public Dictionary<string, bool> DrillDownLevelIncreased { get; } = new Dictionary<string, bool>();

        /// <summary>
        /// The server name to use for the Dashboard
        /// </summary>
        public string ServerName
        {
            get
            {
                return IsDatabaseOverridden ? _serverName : ConfiguredServerName; ;
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
                return IsDatabaseOverridden ? _databaseName : ConfiguredDatabaseName;
            }

            set
            {
                _databaseName = value;
            }
        }

        /// <summary>
        /// The Server configured in the Dashboard
        /// </summary>
        public string ConfiguredServerName { get; set; }

        /// <summary>
        /// The Database configured in the Dashboard
        /// </summary>
        public string ConfiguredDatabaseName { get; set; }

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
        public IEnumerable<string> CurrentFilteredFiles { get; set; } = new List<string>();

        /// <summary>
        /// Since this has a <see cref="DevExpress.DashboardWin.DashboardViewer"/> return the instance of the viewer
        /// </summary>
        public DevExpress.DashboardWin.DashboardViewer Viewer => dashboardViewerMain;

        /// <summary>
        /// Since this does not have <see cref="DevExpress.DashboardWin.DashboardDesigner"/> return null
        /// </summary>
        public DashboardDesigner Designer => null;

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

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public DashboardViewerForm()
        {
            InitializeComponent();

            _dashboardShared = new DashboardShared<DashboardViewerForm>(this, true);
            dashboardToolStripMenuItem.Visible = IsDatabaseOverridden;
        }

        /// <summary>
        /// Constructs DashboardViewerform and opening the given file
        /// </summary>
        /// <param name="fileName">File containing dashboard to open</param>
        public DashboardViewerForm(string fileName)
        {
            try
            {
                InitializeComponent();

                _dashboardShared = new DashboardShared<DashboardViewerForm>(this, true);

                _dashboardName = fileName;
                dashboardViewerMain.DashboardSource = fileName;
                dashboardToolStripMenuItem.Visible = IsDatabaseOverridden;
                UpdateMainTitle();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45310");
            }
        }

        /// <summary>
        /// Constructs DashboardViewerform and opening indicated dashboard using the server and database.
        /// </summary>
        /// <param name="dashboard">File or name of dashboard in database to open</param>
        /// <param name="inDatabase">Specifies that the dashboard is to be loaded from the database</param>
        /// <param name="serverName">Server name to use when opening a dashboard.</param>
        /// <param name="databaseName">Database name to use when opening a dashboard</param>
        public DashboardViewerForm(string dashboard, bool inDatabase, string serverName, string databaseName)
        {
            try
            {
                InitializeComponent();

                _dashboardShared = new DashboardShared<DashboardViewerForm>(this, true);

                ServerName = serverName;
                DatabaseName = databaseName;
                _dashboardName = dashboard;

                if (inDatabase)
                {
                    LoadDashboardFromDatabase(dashboard);
                }
                else
                {
                    dashboardViewerMain.DashboardSource = dashboard;
                }

                dashboardToolStripMenuItem.Visible = AllowDatabaseDashboardSelection;
                fileToolStripMenuItem.Visible = !inDatabase || !AllowDatabaseDashboardSelection;

                UpdateMainTitle();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45310");
            }
        }

        /// <summary>
        /// Constructs DashboardViewerform that allows dashboards to be opened from the database
        /// </summary>
        /// <param name="serverName">Server name to use when opening a dashboard.</param>
        /// <param name="databaseName">Database name to use when opening a dashboard</param>
        public DashboardViewerForm(string serverName, string databaseName)
        {
            try
            {
                InitializeComponent();

                _dashboardShared = new DashboardShared<DashboardViewerForm>(this, true);

                ServerName = serverName;
                DatabaseName = databaseName;
                dashboardToolStripMenuItem.Visible = AllowDatabaseDashboardSelection;
                fileToolStripMenuItem.Visible = !AllowDatabaseDashboardSelection;

                UpdateMainTitle();
                dashboardFlyoutPanel.ShowPopup();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45754");
            }
        }

        #endregion

        #region Event Handlers

        void DashboardViewerForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                e.Cancel = !_dashboardShared.RequestDashboardClose();
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

        void HandleToolStripButtonRefresh_Click(object sender, EventArgs e)
        {
            try
            {
                if (Dashboard != null)
                {
                    dashboardViewerMain.ReloadData(false);
                    _toolStripTextBoxlastRefresh.Text = DateTime.Now.ToString();
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
                    LoadDashboardFromDatabase(_dashboardsInDBListBoxControl.SelectedValue as string);
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
                _dashboardName = string.Empty;
                dashboardViewerMain.DashboardSource = string.Empty;
                _toolStripTextBoxlastRefresh.Text = "";
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
                string filename;
                if (DashboardHelper.SelectDashboardFile(out filename))
                {
                    dashboardViewerMain.DashboardSource = string.Empty;
                    dashboardViewerMain.DashboardSource = filename;
                    _dashboardName = filename;
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

        void HandleDashboardViewerMainDrillDownPerformed(object sender, DrillActionEventArgs e)
        {
            try
            {
                DrillDownLevelForItem[e.DashboardItemName] = e.DrillDownLevel;
                DrillDownLevelIncreased[e.DashboardItemName] = true;
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
                DrillDownLevelForItem[e.DashboardItemName] = e.DrillDownLevel;
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
                UpdateMainTitle();
                _dashboardShared.GridConfigurationsFromXML(Dashboard?.UserData);
                _toolStripTextBoxlastRefresh.Text = DateTime.Now.ToString();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45311");
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

            _dashboardName = dashboardName;

            using (var connection = NewSqlDBConnection())
            {
                connection.Open();

                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "SELECT Definition from Dashboard where DashboardName = @DashboardName";
                    cmd.Parameters.AddWithValue("@DashboardName", dashboardName);
                    var reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        var xdoc = XDocument.Load(reader.GetXmlReader(0), LoadOptions.None);
                        dashboardViewerMain.Dashboard = new Dashboard();
                        dashboardViewerMain.Dashboard.LoadFromXDocument(xdoc);
                    }
                }
            }
            UpdateMainTitle();
        }

        /// <summary>
        /// Update the main title to show the loaded dashboard and database and server 
        /// </summary>
        void UpdateMainTitle()
        {
            if (Dashboard is null)
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
                    "\"{0}\" - Using {1} on {2}", _dashboardName, DatabaseName, ServerName);
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
                using (var connection = NewSqlDBConnection())
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = "SELECT DashboardName FROM Dashboard ";

                    DataTable dataTable = new DataTable();
                    dataTable.Locale = CultureInfo.CurrentCulture;
                    dataTable.Load(command.ExecuteReader());

                    var dashboardList = dataTable.AsEnumerable().ToList().Select(dr => dr.Field<string>(0));
                    _dashboardsInDBListBoxControl.DataSource = dashboardList;
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45765");
            }
        }

        /// <summary>
        /// Returns a connection to the configured database. 
        /// </summary>
        /// <returns>SqlConnection that connects to the <see cref="DatabaseServer"/> and <see cref="DatabaseName"/></returns>
        SqlConnection NewSqlDBConnection()
        {
            // Build the connection string from the settings
            SqlConnectionStringBuilder sqlConnectionBuild = new SqlConnectionStringBuilder();
            sqlConnectionBuild.DataSource = ServerName;
            sqlConnectionBuild.InitialCatalog = DatabaseName;
            sqlConnectionBuild.IntegratedSecurity = true;
            sqlConnectionBuild.NetworkLibrary = "dbmssocn";
            sqlConnectionBuild.MultipleActiveResultSets = true;
            return new SqlConnection(sqlConnectionBuild.ConnectionString);
        }

        #endregion
    }
}
