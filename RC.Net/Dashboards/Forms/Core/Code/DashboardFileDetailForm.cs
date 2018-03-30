using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;


namespace Extract.Dashboard.Forms
{
    public partial class DashboardFileDetailForm : Form
    {
        #region Public Properties

        /// <summary>
        /// Database server name to connect to
        /// </summary>
        public string ServerName { get; set; }

        /// <summary>
        /// Database to connect to on the server
        /// </summary>
        public string DatabaseName { get; set; }

        #endregion

        #region Private fields

        /// <summary>
        /// The configuration data to use to display data
        /// </summary>
        GridDetailConfiguration _gridDetailConfiguration;

        /// <summary>
        /// Dictionary that contains name-value pairs for data from the grid row
        /// </summary>
        Dictionary<string, object> _columnValues;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes the DashboardFileDetailForm
        /// </summary>
        /// <param name="columnValues">Dictionary with name-value pairs containing the data that is on the grid</param>
        /// <param name="serverName">Database server name</param>
        /// <param name="databaseName">Database name</param>
        /// <param name="configuration">Configuration to use</param>
        public DashboardFileDetailForm(Dictionary<string, object> columnValues, string serverName,
            string databaseName, GridDetailConfiguration configuration)
        {
            try
            {
                InitializeComponent();
                if (columnValues.ContainsKey("FileName"))
                {
                    _imageViewer.OpenImage((string)columnValues["FileName"], false);
                    Text = "Data for " + (string)columnValues["FileName"];
                }


                ServerName = serverName;
                DatabaseName = databaseName;
                _gridDetailConfiguration = configuration;
                _columnValues = columnValues;


            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45723");
            }

        } 

        #endregion

        #region Event Overrides

        protected override void OnLoad(EventArgs e)
        {
            try
            {
                _imageViewer.EstablishConnections(this);

                base.OnLoad(e);

                LoadGridData();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45703");
            }
        }

        #endregion

        #region Helper members

        /// <summary>
        /// Loads the data grid using the RowQuery in the configuration data
        /// </summary>
        void LoadGridData()
        {
            try
            {
                using (var table = new DataTable())
                using (var connection = NewSqlDBConnection())
                {
                    connection.Open();

                    var command = connection.CreateCommand();
                    command.CommandText = _gridDetailConfiguration.RowQuery;

                    // add the column values as parameters for the query
                    foreach (var kp in _columnValues)
                    {
                        command.Parameters.AddWithValue("@" + kp.Key, kp.Value);
                    }


                    table.Load(command.ExecuteReader());

                    dataGridView.DataSource = table;
                }

            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45705");
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

        #region Event handlers

        void HandleDataGridViewCellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            try
            {
                DataGridViewRow row = dataGridView.Rows[e.RowIndex];
                if (row.DataGridView.Columns.Contains("ExpectedOrFound"))
                {
                    row.DefaultCellStyle.BackColor =
                        ((string)row.Cells["ExpectedOrFound"].Value == "Expected") ? Color.LightPink : Color.LightBlue;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45722");
            }
        } 

        #endregion
    }
}

