using Extract.Utilities;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Transactions;
using System.Windows.Forms;
using System.Xml.Linq;


namespace Extract.Dashboard.Forms
{
    public partial class ManageDashboardsForm : Form
    {
        #region Fields

        /// <summary>
        /// Saves the cell value at the beginning of a cell edit
        /// </summary>
        string _originalCellValue;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a ManageDashboardsForm for the given server and database
        /// </summary>
        /// <param name="databaseServer">The Database server name</param>
        /// <param name="databaseName">The Database name to connect to</param>
        public ManageDashboardsForm(string databaseServer, string databaseName)
        {
            try
            {
                InitializeComponent();
                DatabaseName = databaseName;
                DatabaseServer = databaseServer;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45764");
            }
        }

        #endregion

        #region Public properties

        /// <summary>
        /// Database server to connect to
        /// </summary>
        public string DatabaseServer { get; set; }

        /// <summary>
        /// Database to connect to
        /// </summary>
        public string DatabaseName { get; set; }

        #endregion

        #region Event Handlers

        void HandleViewButtonClick(object sender, EventArgs e)
        {
            try
            {
                string dashboardName = dashboardDataGridView.CurrentCell?.Value as string;
                if (string.IsNullOrWhiteSpace(dashboardName))
                {
                    return;
                }
                string parameters = string.Format(CultureInfo.InvariantCulture,
                    "/s \"{0}\" /d \"{1}\" /b \"{2}\"", DatabaseServer, DatabaseName, dashboardName);

                string dashboardViewer = FileSystemMethods.PathCombine(
                    FileSystemMethods.CommonComponentsPath, "DashboardViewer.exe");

                SystemMethods.RunExecutable(dashboardViewer, parameters, 0, startAndReturnimmediately: true);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45778");
            }
        }

        void HandleDashboardGridViewCellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (dashboardDataGridView.CurrentCell.Value as string != _originalCellValue)
                {
                    using (var connection = NewSqlDBConnection())
                    using (var scope = new TransactionScope())
                    {
                        connection.Open();
                        var command = connection.CreateCommand();
                        command.CommandText =
                            "UPDATE Dashboard SET DashboardName = @NewDashboardName WHERE DashboardName = @OldDashboardName";
                        command.Parameters.AddWithValue("@OldDashboardName", _originalCellValue);
                        command.Parameters.AddWithValue("@NewDashboardName", dashboardDataGridView.CurrentCell.Value as string);
                        command.ExecuteNonQuery();
                        scope.Complete();
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45779");
            }
        }

        void HandleDashboardGridViewCellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            try
            {
                _originalCellValue = dashboardDataGridView.CurrentCell.Value as string;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45780");
            }
        }

        void HandleRenameDashboardButtonClick(object sender, EventArgs e)
        {
            try
            {
                dashboardDataGridView.BeginEdit(true);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45781");
            }
        }

        void HandleRemoveDashboardButtonClick(object sender, EventArgs e)
        {
            try
            {
                if (dashboardDataGridView.CurrentRow != null)
                {
                    string dashboardName = dashboardDataGridView.CurrentRow.Cells["DashboardName"].Value as string;
                    string message = string.Format(CultureInfo.InstalledUICulture, "Remove the {0} dashboard?", dashboardName);
                    if (MessageBox.Show(message, "Remove dashboard from database", MessageBoxButtons.YesNoCancel) == DialogResult.Yes)
                    {
                        using (var connection = NewSqlDBConnection())
                        using (var scope = new TransactionScope())
                        {
                            connection.Open();
                            var command = connection.CreateCommand();

                            command.CommandText = "DELETE FROM Dashboard WHERE DashboardName = @DashboardName";
                            command.Parameters.AddWithValue("@DashboardName", dashboardName);
                            command.ExecuteNonQuery();
                            scope.Complete();
                        }
                        // reload the grid
                        LoadDashboardGrid();
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45775");
            }

        }

        void HandleImportDashboardButtonClick(object sender, EventArgs e)
        {
            try
            {
                SelectDashboardToImportForm selectForm = new SelectDashboardToImportForm();
                if (selectForm.ShowDialog() == DialogResult.OK)
                {
                    var xDoc = XDocument.Load(selectForm.DashboardFile);
                    using (var connect = NewSqlDBConnection())
                    using (var scope = new TransactionScope())
                    {
                        connect.Open();
                        var command = connect.CreateCommand();

                        command.CommandText =
                            "INSERT INTO Dashboard ([DashboardName], [Definition]) " +
                            "VALUES ( @DashboardName, @Definition)";

                        command.Parameters.AddWithValue("@DashboardName", selectForm.DashboardName);
                        command.Parameters.Add("@Definition", SqlDbType.Xml).Value = xDoc.ToString();

                        command.ExecuteScalar();
                        scope.Complete();
                    }
                    // reload the grid
                    LoadDashboardGrid();
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45768");
            }
        }

        void HandleManageDashboardsFormLoad(object sender, EventArgs e)
        {
            try
            {
                LoadDashboardGrid();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45763");
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Load the Dashboard grid from the configured database
        /// </summary>
        void LoadDashboardGrid()
        {
            try
            {
                using (var connection = NewSqlDBConnection())
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = "SELECT DashboardName FROM Dashboard";
                    
					DataTable dataTable = new DataTable();
                    dataTable.Load(command.ExecuteReader());
                    dashboardDataGridView.DataSource = dataTable;
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
            sqlConnectionBuild.DataSource = DatabaseServer;
            sqlConnectionBuild.InitialCatalog = DatabaseName;
            sqlConnectionBuild.IntegratedSecurity = true;
            sqlConnectionBuild.NetworkLibrary = "dbmssocn";
            sqlConnectionBuild.MultipleActiveResultSets = true;
            return new SqlConnection(sqlConnectionBuild.ConnectionString);
        }

        #endregion

        private void dashboardDataGridView_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                // check to make sure the header wasn't double clicked
                if (e.RowIndex >= 0)
                {
                    HandleViewButtonClick(sender, e);
                }
            }
            catch(Exception ex)
            {
                ex.ExtractDisplay("ELI46129");
            }
        }
    }
}
