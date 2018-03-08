using DevExpress.DashboardCommon;
using DevExpress.DataAccess.ConnectionParameters;
using DevExpress.XtraEditors;
using Extract;
using System;
using System.Globalization;
using System.Windows.Forms;

namespace DashboardViewer
{
    public partial class DashboardViewerForm : XtraForm
    {
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

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor
        /// </summary>
        public DashboardViewerForm()
        {
            InitializeComponent();
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
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45310");
            }
        }

        /// <summary>
        /// Constructs DashboardViewerform and opening the given file
        /// </summary>
        /// <param name="fileName">File containing dashboard to open</param>
        /// <param name="serverName">Server name to use when opening a dashboard.</param>
        /// <param name="databaseName">Database name to use when opening a dashboard</param>
        public DashboardViewerForm(string fileName, string serverName, string databaseName)
        {
            try
            {
                InitializeComponent();

                _serverName = serverName;
                _databaseName = databaseName;

                dashboardViewerMain.DashboardSource = fileName;

                UpdateMainTitle();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45310");
            }
        }

        #endregion

        #region Event Handlers

        #region Menu item event handlers

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
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "ESDX|*.esdx|XML|*.xml|All|*.*";
                openFileDialog.DefaultExt = "esdx";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    dashboardViewerMain.DashboardSource = string.Empty;
                    dashboardViewerMain.DashboardSource = openFileDialog.FileName;
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
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45311");
            }
        } 
        #endregion

        #endregion

        #region Helper methods

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

        #endregion
    }
}
