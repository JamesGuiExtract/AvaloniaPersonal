using Extract.Utilities;
using System;
using System.Data;
using System.Data.SqlClient;
using System.DirectoryServices.AccountManagement;
using System.Globalization;
using System.IO;
using System.Linq;
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
                FAMUserID = GetFAMUserID();
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

        /// <summary>
        /// FAMUserID for current session
        /// </summary>
        public int FAMUserID { get; set; }

        #endregion

        #region Event Handlers

        void HandleExportDashboardButton_Click(object sender, EventArgs e)
        {
            if (dashboardDataGridView.CurrentRow != null)
            {
                string dashboardName = dashboardDataGridView.CurrentRow.Cells["DashboardName"].Value as string;

                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "ESDX|*.esdx|All|*.*";
                saveFileDialog.DefaultExt = "esdx";
                saveFileDialog.FileName = dashboardName + ".esdx";
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string fileName = saveFileDialog.FileName;
                    if (File.Exists(fileName))
                    {
                        if (MessageBox.Show(
                            string.Format(CultureInfo.InstalledUICulture, "{0} exists. Overwrite?", fileName), "File exists", MessageBoxButtons.YesNo) != DialogResult.Yes)
                        {
                            return;
                        }
                    }

                    string dashboardDefinition = GetDashboardDefinition(dashboardName);
                    XDocument document = XDocument.Parse(dashboardDefinition);
                    document.Save(saveFileDialog.FileName, SaveOptions.None);
                }
            }
        }
        void HandleReplaceDashboardButton_Click(object sender, EventArgs e)
        {
            try
            {
                if (dashboardDataGridView.CurrentRow != null)
                {
                    string dashboardName = dashboardDataGridView.CurrentRow.Cells["DashboardName"].Value as string;
                    SelectDashboardToImportForm selectForm = new SelectDashboardToImportForm(true, dashboardName);
                    if (selectForm.ShowDialog() == DialogResult.OK)
                    {
                        var xDoc = XDocument.Load(selectForm.DashboardFile, LoadOptions.PreserveWhitespace);
                        using (var connect = NewSqlDBConnection())
                        {
                            connect.Open();
                            var command = connect.CreateCommand();
                            command.CommandText =
                                @"UPDATE Dashboard 
                                    SET [Definition] = @Definition,
                                        [FAMUserID] = @FAMUserID,
                                        [LastImportedDate] = GETDATE()
                                    WHERE [DashboardName] = @DashboardName
                                ";

                            command.Parameters.Add("@DashboardName", SqlDbType.NVarChar, 100).Value = dashboardName;
                            command.Parameters.AddWithValue("@FAMUserID", FAMUserID);
                            command.Parameters.Add("@Definition", SqlDbType.Xml).Value = xDoc.ToString(SaveOptions.None);
                            command.ExecuteScalar();
                        }
                        LoadDashboardGrid();
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI46178");
            }
        }

        void HandleDashboardDataGridView_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                // check to make sure the header wasn't double clicked
                if (e.RowIndex >= 0)
                {
                    HandleViewButtonClick(sender, e);
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI46129");
            }
        }

        void HandleViewButtonClick(object sender, EventArgs e)
        {
            try
            {
                string dashboardName = dashboardDataGridView.CurrentRow?.Cells["DashboardName"].Value as string;
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
                if (dashboardDataGridView.Columns[e.ColumnIndex].Name == "DashboardName")
                {
                    if (string.IsNullOrEmpty(dashboardDataGridView.CurrentCell.Value as string))
                    {
                        ExtractException exName = new ExtractException("ELI46458", "Dashboard name cannot be empty.");
                        dashboardDataGridView.CurrentCell.Value = _originalCellValue;
                        throw exName;
                    }

                    if (dashboardDataGridView.CurrentCell.Value as string != _originalCellValue)
                    {
                        using (var connection = NewSqlDBConnection())
                        {
                            connection.Open();
                            var command = connection.CreateCommand();
                            command.CommandText =
                                "UPDATE Dashboard SET DashboardName = @NewDashboardName WHERE DashboardName = @OldDashboardName";
                            command.Parameters.AddWithValue("@OldDashboardName", _originalCellValue);
                            command.Parameters.AddWithValue("@NewDashboardName", dashboardDataGridView.CurrentCell.Value as string);
                            command.ExecuteNonQuery();
                        }
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
                if ( dashboardDataGridView.CurrentRow is null)
                {
                    return;
                }
                dashboardDataGridView.CurrentCell = dashboardDataGridView.CurrentRow.Cells["DashboardName"];
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
                    if (MessageBox.Show(message, "Remove dashboard from database", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    {
                        using (var connection = NewSqlDBConnection())
                        {
                            connection.Open();
                            var command = connection.CreateCommand();

                            command.CommandText = "DELETE FROM Dashboard WHERE DashboardName = @DashboardName";
                            command.Parameters.AddWithValue("@DashboardName", dashboardName);
                            command.ExecuteNonQuery();
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
                    var xDoc = XDocument.Load(selectForm.DashboardFile, LoadOptions.PreserveWhitespace);
                    using (var connect = NewSqlDBConnection())
                    {
                        connect.Open();
                        var command = connect.CreateCommand();

                        command.CommandText =
                            "INSERT INTO Dashboard ([DashboardName], [Definition], [FAMUserID], [LastImportedDate]) " +
                            "VALUES ( @DashboardName, @Definition, @FAMUserID, GETDATE())";

                        command.Parameters.Add("@DashboardName", SqlDbType.NVarChar, 100).Value = selectForm.DashboardName;
                        command.Parameters.AddWithValue("@FAMUserID", FAMUserID);
                        command.Parameters.Add("@Definition", SqlDbType.Xml).Value = xDoc.ToString(SaveOptions.None);

                        command.ExecuteScalar();
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
                    command.CommandText =
                        "SELECT DashboardName, IsNull(FullUserName, UserName) FullUserName, LastImportedDate FROM Dashboard " +
                            "INNER JOIN FAMUser ON Dashboard.FAMUserID = FAMUser.ID";

                    DataTable dataTable = new DataTable();
                    dataTable.Load(command.ExecuteReader());
                    dashboardDataGridView.DataSource = dataTable;
                    dashboardDataGridView.Columns["FullUserName"].HeaderText = "User Imported";
                    dashboardDataGridView.Columns["LastImportedDate"].HeaderText = "Last Imported";
                    EnableButtons();
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

        /// <summary>
        /// Gets or creates a FAMUserId for the current user
        /// </summary>
        /// <returns>ID of the user in the FAMUser table</returns>
        int GetFAMUserID()
        {
            using (var connection = NewSqlDBConnection())
            {
                connection.Open();
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = @"
                        DECLARE @FAMUserName nvarchar(50)= SUBSTRING(SUSER_SNAME(), CHARINDEX('\',SUSER_SNAME()) +1, 50)
                        
                        DECLARE @FAMUserID INT
                        
                        SELECT @FAMUserID = ID FROM FAMUser WHERE UserName = @FAMUserName
                        
                        IF @FAMUserID IS NULL
                        BEGIN
                        	INSERT INTO FAMUser(UserName, FullUserName)
                        	VALUES ( @FAMUserName, @FullUserName)
                        
                        	SELECT @FAMUserID = ID FROM FAMUser WHERE UserName = @FAMUserName
                        END
                        
                        SELECT @FAMUserID AS FAMUserID";

                    cmd.Parameters.Add("@FullUserName", SqlDbType.NVarChar, 128).Value = UserPrincipal.Current.DisplayName;

                    var result = cmd.ExecuteScalar() as int?;
                    return result ?? 0;
                }
            }
        }

        /// <summary>
        /// Get the dashboard definition for the given dashboard name;
        /// </summary>
        /// <param name="dashboardName"></param>
        /// <returns></returns>
        string GetDashboardDefinition(string dashboardName)
        {
            using (var connection = NewSqlDBConnection())
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText =
                        "SELECT Definition FROM Dashboard WHERE DashboardName = @DashboardName";

                    command.Parameters.Add("@DashboardName", SqlDbType.NVarChar, 100).Value = dashboardName;
                    return command.ExecuteScalar() as string;
                }
            }
        }

        /// <summary>
        /// Enables or disables buttons based on the content of the dashbaordDataGridView
        /// </summary>
        void EnableButtons()
        {
            bool enable = dashboardDataGridView.CurrentCell?.RowIndex >= 0;
            _viewButton.Enabled = enable;
            _renameDashboardButton.Enabled = enable;
            _removeDashboardButton.Enabled = enable;
            _replaceDashboardButton.Enabled = enable;
            _exportDashboardButton.Enabled = enable;
        }

        #endregion
    }
}
