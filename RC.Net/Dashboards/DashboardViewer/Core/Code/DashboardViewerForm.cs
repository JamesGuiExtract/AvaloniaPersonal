using DevExpress.DashboardCommon;
using DevExpress.DashboardWin;
using DevExpress.DataAccess.ConnectionParameters;
using DevExpress.XtraEditors;
using Extract;
using Extract.Dashboard.Forms;
using Extract.Dashboard.Utilities;
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
    public partial class DashboardViewerForm : XtraForm
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
        /// Server name configured for loaded dashboard
        /// </summary>
        string _serverConfiguredInDashboard;

        /// <summary>
        /// Database name configured for loaded dashboard
        /// </summary>
        string _databaseConfiguredInDashboard;

        /// <summary>
        /// The key used is the control name
        /// </summary>
        Dictionary<string, GridDetailConfiguration> _customGridValues = new Dictionary<string, GridDetailConfiguration>();

        /// <summary>
        /// Dictionary to track drill down level for dashboard items
        /// </summary>
        Dictionary<string, int> _drillDownLevelForItem = new Dictionary<string, int>();

        /// <summary>
        /// Schema version for the active database
        /// </summary>
        int _databaseVersion = 0;

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
                    if (_databaseVersion == 0 && IsDatabaseOverridden())
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
                    ex.ExtractDisplay("ELI45774");
                }
                return _databaseVersion >= _VersionDashboardTableAdded;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public DashboardViewerForm()
        {
            InitializeComponent();
            dashboardToolStripMenuItem.Visible = IsDatabaseOverridden();
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

                dashboardViewerMain.DashboardSource = fileName;
                dashboardToolStripMenuItem.Visible = IsDatabaseOverridden();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45310");
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

                _serverName = serverName;
                _databaseName = databaseName;

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
                ex.ExtractDisplay("ELI45310");
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

                _serverName = serverName;
                _databaseName = databaseName;
                dashboardToolStripMenuItem.Visible = AllowDatabaseDashboardSelection;
                fileToolStripMenuItem.Visible = !AllowDatabaseDashboardSelection;

                UpdateMainTitle();
                dashboardFlyoutPanel.ShowPopup();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45754");
            }
        }

        #endregion

        #region Event Handlers

        #region Menu item event handlers

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
                // Toggle the dashboard flyout panel - if it is displayed hide it if it is not displayed show it
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
                dashboardViewerMain.DashboardSource = string.Empty;
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
                _drillDownLevelForItem[e.DashboardItemName] = e.DrillDownLevel;
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
                _drillDownLevelForItem[e.DashboardItemName] = e.DrillDownLevel;
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
                if (sender is DevExpress.DashboardWin.DashboardViewer dashboardViewer)
                {
                    Dashboard dashboard = dashboardViewer.Dashboard;
                    GridDashboardItem gridItem = dashboard.Items[e.DashboardItemName] as GridDashboardItem;

                    int drillLevel;
                    _drillDownLevelForItem.TryGetValue(e.DashboardItemName, out drillLevel);

                    if (!gridItem.InteractivityOptions.IsDrillDownEnabled || gridItem.GetDimensions().Count - 1 == drillLevel)
                    {
                        DashboardHelper.DisplayDashboardDetailForm(gridItem, e, _customGridValues[e.DashboardItemName]);
                    }
                }
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
                SqlServerConnectionParametersBase sqlParameters = e.ConnectionParameters as SqlServerConnectionParametersBase;

                // Only override SQL Server connection
                if (sqlParameters == null)
                {
                    return;
                }

                _serverConfiguredInDashboard = sqlParameters.ServerName;
                _databaseConfiguredInDashboard = sqlParameters.DatabaseName;

                if (string.IsNullOrWhiteSpace(_serverName) || string.IsNullOrWhiteSpace(_databaseName))
                {
                    return;
                }

                sqlParameters.ServerName = _serverName;
                sqlParameters.DatabaseName = _databaseName;
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
                _customGridValues = DashboardHelper.GridConfigurationsFromXML(dashboardViewerMain.Dashboard?.UserData);
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
        }

        /// <summary>
        /// Update the main title to show the loaded dashboard and database and server 
        /// </summary>
        void UpdateMainTitle()
        {
            if (dashboardViewerMain.Dashboard is null)
            {
                if (string.IsNullOrWhiteSpace(_serverName) && string.IsNullOrWhiteSpace(_databaseName))
                {
                    Text = "Dashboard viewer";
                }
                else
                {
                    Text = string.Format(CultureInfo.InvariantCulture,
                        "Dashboard viewer Using {0} on {1}", _databaseName, _serverName);
                }
            }
            else
            {
                if (string.IsNullOrWhiteSpace(_serverName) && string.IsNullOrWhiteSpace(_databaseName))
                {
                    Text = string.Format(CultureInfo.InvariantCulture,
                       "{0} Using {1} on {2}", dashboardViewerMain.Dashboard.Title.Text, _databaseConfiguredInDashboard,
                       _serverConfiguredInDashboard);

                }
                else
                {
                    Text = string.Format(CultureInfo.InvariantCulture,
                        "{0} Using {1} on {2}", dashboardViewerMain.Dashboard.Title.Text, _databaseName, _serverName);
                }
            }
        }

        /// <summary>
        /// Indicates if the server and database are being overridden.
        /// </summary>
        /// <returns><c>true</c> if the server and database are being overridden, <c>false</c> otherwise"/></returns>
        bool IsDatabaseOverridden()
        {
            return !(string.IsNullOrWhiteSpace(_serverName) || string.IsNullOrWhiteSpace(_databaseName));
        }

        /// <summary>
        /// Loads the list displayed by the dashboard flyout from the Dashboard table of the database
        /// </summary>
        void LoadDashboardList()
        {
            try
            {
                if (!IsDatabaseOverridden() || !AllowDatabaseDashboardSelection)
                {
                    return;
                }
                using (var connection = NewSqlDBConnection())
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = "SELECT DashboardName FROM Dashboard ";

                    DataTable dataTable = new DataTable();
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
            sqlConnectionBuild.DataSource = _serverName;
            sqlConnectionBuild.InitialCatalog = _databaseName;
            sqlConnectionBuild.IntegratedSecurity = true;
            sqlConnectionBuild.NetworkLibrary = "dbmssocn";
            sqlConnectionBuild.MultipleActiveResultSets = true;
            return new SqlConnection(sqlConnectionBuild.ConnectionString);
        }

        #endregion
    }
}
