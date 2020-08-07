using DevExpress.DataAccess.ConnectionParameters;
using DevExpress.DataAccess.Sql;
using DevExpress.XtraEditors;
using DevExpress.XtraReports.UserDesigner;
using Extract;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Windows.Forms;

namespace ReportDesigner
{
    public partial class ReportDesignerForm : XtraForm
    {
        /// <summary>
        /// ServerName to override the SQL server specified in the report definition
        /// Should be null or empty if not overridden
        /// </summary>
        public string ServerName { get; set; }

        /// <summary>
        /// DatabaseName to override the SQL database specified in the report definition
        /// Should be null or empty if not overridden
        /// </summary>
        public string DatabaseName { get; set; }


        /// <summary>
        /// Constructor that creates the ReportDesigner form by opening the <paramref name="reportFile"/>
        /// </summary>
        /// <param name="reportFile">FileName for the report to open.</param>
        /// <param name="reportName">Name of the report to open from <paramref name="databaseName"/></param>
        /// <param name="serverName">Server name to open <paramref name="databaseName"/></param>
        /// <param name="databaseName">Database to open on <paramref name="serverName"/></param>
        public ReportDesignerForm(string reportFile, string reportName, string serverName, string databaseName)
        {
            InitializeComponent();

            ServerName = serverName;
            DatabaseName = databaseName;

            if (!string.IsNullOrEmpty(reportFile))
            {
                DevExpress.XtraReports.Configuration.Settings.Default.StorageOptions.RootDirectory = Path.GetDirectoryName(reportFile);
                reportDesigner1.OpenReport(reportFile);
            }
        }

        private void reportDesigner1_DesignPanelLoaded(object sender, DesignerLoadedEventArgs e)
        {
            try
            {
                var designPanel = sender as XRDesignPanel;

                if (OverrideConnectionWithPrompt(designPanel))
                {
                    // Add the handler to the ReportStateChanged event to change the state to Changed after the report 
                    // is fully Opened current state in this method is Opening 
                    designPanel.ReportStateChanged += HandleXRDesignPanel_ReportStateChanged;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI49829");
            }
        }
        

        private void HandleXRDesignPanel_ReportStateChanged(object sender, ReportStateEventArgs e)
        {
            // if the Report Status is Opened change to State to Changed and remove handler
            if (e.ReportState == ReportState.Opened)
            {
                var designPanel = sender as XRDesignPanel;
                DevExpress.XtraReports.Configuration.Settings.Default.StorageOptions.RootDirectory = Path.GetDirectoryName(designPanel.FileName);
                designPanel.ReportState = ReportState.Changed;
                designPanel.ReportStateChanged -= HandleXRDesignPanel_ReportStateChanged;
            }
        }

        private bool OverrideConnectionWithPrompt(XRDesignPanel panel)
        {
            var report = panel.Report;
            bool changeConnection = false;
            bool promptedChangeConnection = false;

            foreach (var sqlConnection in report.ComponentStorage.OfType<SqlDataSource>())
            {
                if (string.IsNullOrEmpty(ServerName) || string.IsNullOrEmpty(DatabaseName))
                {
                    var connectionParameters = sqlConnection?.ConnectionParameters as MsSqlConnectionParameters;
                    if (connectionParameters != null &&
                        (connectionParameters?.ServerName != ServerName ||
                            connectionParameters?.DatabaseName != DatabaseName))
                    {
                        if (!promptedChangeConnection)
                        {
                            changeConnection = MessageBox.Show(
    $@"Change all SQL connections to:
        Server: {ServerName} 
        Database: {DatabaseName}?

    Note: This will change the report if saved.",
                                "Change data connection",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Question,
                                MessageBoxDefaultButton.Button1, 0) == DialogResult.Yes;

                            promptedChangeConnection = true;
                        }
                        if (changeConnection)
                        {
                            connectionParameters.ServerName = ServerName;
                            connectionParameters.DatabaseName = DatabaseName;
                        }
                    }
                }

                // change the Command timeout
                sqlConnection.ConnectionOptions.DbCommandTimeout = 0;
            }
            return changeConnection;
        }
    }
}
