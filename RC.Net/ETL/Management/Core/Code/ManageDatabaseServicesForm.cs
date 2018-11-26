using Extract.Utilities;
using Extract.Utilities.Forms;
using System;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Transactions;
using System.Windows.Forms;
using UCLID_FILEPROCESSINGLib;

namespace Extract.ETL.Management
{
    public partial class ManageDatabaseServicesForm : Form
    {
        #region DatabaseServiceData class definition

        /// <summary>
        /// Class for the Database service data used for binding data to a DataGridView
        /// </summary>
        public class DatabaseServiceData : INotifyPropertyChanged
        {
            #region DatabaseServiceData Fields

            Int32 _id;
            string _description;
            string _serviceType;
            DatabaseService _service;
            bool _enabled;

            #endregion

            #region DatabaseServiceData Properties

            /// <summary>
            /// ID property that raises PropertyChanged event when value changes 
            /// </summary>
            public Int32 ID
            {
                get { return _id; }
                set
                {
                    if (value != _id)
                    {
                        _id = value;
                        NotifyPropertyChanged();
                    }
                }
            }

            /// <summary>
            /// Description property that raises PropertyChanged event when value changes
            /// </summary>
            public string Description
            {
                get { return _description; }
                set
                {
                    if (value != _description)
                    {
                        _description = value;
                        NotifyPropertyChanged();
                    }
                }
            }

            /// <summary>
            /// Description of the Database server type
            /// </summary>
            public string ServiceType
            {
                get { return _serviceType; }
                set
                {
                    if (value != _serviceType)
                    {
                        _serviceType = value;
                        NotifyPropertyChanged();
                    }
                }
            }

            /// <summary>
            /// Service property that raises PropertyChanged event when value changes
            /// </summary>
            public DatabaseService Service
            {
                get { return _service; }
                set
                {
                    if (value != _service)
                    {
                        _service = value;
                        NotifyPropertyChanged();
                    }
                }
            }

            /// <summary>
            /// Indicates that the Database service is enabled
            /// </summary>
            public bool Enabled
            {
                get { return _enabled; }
                set
                {
                    if (value != _enabled)
                    {
                        _enabled = value;
                        NotifyPropertyChanged();
                    }
                }
            }

            #endregion

            #region DatabaseServiceData Events

            public event PropertyChangedEventHandler PropertyChanged;

            #endregion

            #region DatabaseServiceData Event handlers

            /// <summary>
            /// Called by each of the property Set accessors when property changes
            /// </summary>
            /// <param name="propertyName">Name of the property changed</param>
            protected virtual void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }

            #endregion
        }

        #endregion

        #region Fields

        readonly string _DatabaseServiceSql = "SELECT ID, Description, Settings, Status, Enabled FROM dbo.DatabaseService";

        /// <summary>
        /// Binding list of data being displayed in data grid
        /// </summary>
        BindingList<DatabaseServiceData> _listOfDataToDisplay;

        /// <summary>
        /// The Current value of the ETLRestart field from the DBInfo table
        /// </summary>
        DateTime _EtlRestartTime;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes empty form
        /// </summary>
        public ManageDatabaseServicesForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Initializes and loads the data grid with data from the given database
        /// </summary>
        /// <param name="serverName">Name of server to connect to</param>
        /// <param name="databaseName">Name of database to connect to</param>
        public ManageDatabaseServicesForm(string serverName, string databaseName)
        {
            try
            {
                InitializeComponent();
                DatabaseServer = serverName;
                DatabaseName = databaseName;

                LoadDataGrid();
                EnableButtons();
            }
            catch (Exception ex)
            {

                ex.ExtractDisplay("ELI45589");
            }
        }

        #endregion

        #region Private Properties

        string DatabaseServer { get; set; }

        string DatabaseName { get; set; }

        #endregion

        #region Private Methods

        /// <summary>
        /// Loads the DatabaseService data into the data grid
        /// </summary>
        void LoadDataGrid()
        {
            try
            {
                var table = new DataTable();
                using (var connection = NewSqlDBConnection())
                {
                    connection.Open();

                    using (var command = connection.CreateCommand())
                    {
                        command.CommandText = _DatabaseServiceSql;

                        table.Load(command.ExecuteReader());

                        var dataToDisplay = table.AsEnumerable()
                            .Select(r =>
                            {
                                var service = DatabaseService.FromJson(r.Field<string>("Settings"));

                            // An empty settings string results in a null service object
                            return service == null
                                ? null
                                : new DatabaseServiceData
                                {
                                    ID = r.Field<Int32>("ID"),
                                    Description = r.Field<string>("Description"),
                                    Service = service,
                                    ServiceType = service.ExtractCategoryType,
                                    Enabled = r.Field<bool>("Enabled")
                                };
                            })
                            .Where(s => s != null)
                            .ToList();

                        _listOfDataToDisplay = new BindingList<DatabaseServiceData>(dataToDisplay);
                        BindingSource bindingSource = new BindingSource();
                        bindingSource.DataSource = _listOfDataToDisplay;

                        _databaseServicesDataGridView.DataSource = bindingSource;

                        // Hide the ID column
                        _databaseServicesDataGridView.Columns["ID"].Visible = false;
                        _databaseServicesDataGridView.Columns["Service"].Visible = false;
                        _databaseServicesDataGridView.Columns["ServiceType"].HeaderText = "Service Type";
                        _databaseServicesDataGridView.Columns["ServiceType"].MinimumWidth = 150;
                        _databaseServicesDataGridView.Columns["Enabled"].AutoSizeMode = DataGridViewAutoSizeColumnMode.ColumnHeader;
                        _databaseServicesDataGridView.Columns["Enabled"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
                        _databaseServicesDataGridView.Columns["Description"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                    }

                    // This is not a field or property because ManagerDatabaseServicesForm is used by FAMDBAdmin and
                    // if FileProcessingDB is a property or field FAMDBAdmin doesn't always compile(C++ CLR project)
                    FileProcessingDB famDB = new FileProcessingDB();
                    famDB.DatabaseServer = DatabaseServer;
                    famDB.DatabaseName = DatabaseName;

                    // Get the ETLRestart
                    string etlRestartString = famDB.GetDBInfoSetting("ETLRestart", false);
                    //  if it is empty set it to current datetime or if it doesn't parse as a date
                    if (string.IsNullOrEmpty(etlRestartString) || !DateTime.TryParse(etlRestartString, out _EtlRestartTime))
                    {
                        _EtlRestartTime = DateTime.Now;
                        famDB.SetDBInfoSetting("ETLRestart", _EtlRestartTime.ToString("yyyy-MM-ddTHH:mm:ss.fffzzz"), true, false);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45588");
            }
        }

        /// <summary>
        /// Gets a new <see cref="SqlConnection"/> using the configured server and database
        /// </summary>
        /// <returns></returns>
        SqlConnection NewSqlDBConnection()
        {
            // Build the connection string from the settings
            SqlConnectionStringBuilder sqlConnectionBuild = new SqlConnectionStringBuilder();
            sqlConnectionBuild.DataSource = DatabaseServer;
            sqlConnectionBuild.InitialCatalog = DatabaseName;
            sqlConnectionBuild.IntegratedSecurity = true;
            sqlConnectionBuild.NetworkLibrary = "dbmssocn";
            sqlConnectionBuild.MultipleActiveResultSets = true;
            sqlConnectionBuild.Encrypt = false;
            return new SqlConnection(sqlConnectionBuild.ConnectionString);
        }

        /// <summary>
        /// Gets the row data from the database if there is no matching ID in the DB the row will be deleted from the bound data
        /// </summary>
        /// <param name="row">Row to retrieve from the database</param>
        /// <returns><see langword="true"> if the row was found in the database, <see langword="false"/>
        /// is returned if the row is no longer in the database</see></returns>
        bool GetCurrentRowDataFromDB(DataGridViewRow row)
        {
            DatabaseServiceData currentData = row.DataBoundItem as DatabaseServiceData;

            // Get current data from the database
            using (var connection = NewSqlDBConnection())
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = _DatabaseServiceSql + " WHERE ID = @DatabaseServiceID";
                command.Parameters.AddWithValue("@DatabaseServiceID", currentData.ID);

                var data = command.ExecuteReader();
                if (!data.Read())
                {
                    MessageBox.Show("Database Service no longer exists.");
                    _listOfDataToDisplay.Remove((DatabaseServiceData)row.DataBoundItem);
                    return false;
                }
                currentData.Description = data.GetString(data.GetOrdinal("Description"));
                currentData.Service = DatabaseService.FromJson(data.GetString(data.GetOrdinal("Settings")));
                return true;
            }
        }

        /// <summary>
        /// Edits the given DatabaseService 
        /// </summary>
        /// <param name="service">DatabaseService to be edited</param>
        /// <param name="caption">Caption to use for editing service</param>
        /// <param name="id">The DatabaseService.ID of the service
        /// (needed to show the status fields when modifying a service)</param>
        /// <returns>configured service or null if not configured</returns>
        DatabaseService EditService(DatabaseService service, string caption, int id = 0)
        {
            // make a clone to work with
            var tmpService = service.Clone() as DatabaseService;
            tmpService.DatabaseName = DatabaseName;
            tmpService.DatabaseServer = DatabaseServer;
            tmpService.DatabaseServiceID = id;
            bool configured = false;
            IConfigSettings configService = tmpService as IConfigSettings;
            if (configService != null)
            {
                configured = configService.Configure();
            }
            else
            {
                DatabaseServiceEditForm serviceEditForm =
                    new DatabaseServiceEditForm(tmpService);

                serviceEditForm.Text = String.Format(CultureInfo.InvariantCulture,
                    caption, tmpService.ExtractCategoryType);
                configured = serviceEditForm.ShowDialog(this) == DialogResult.OK;
                tmpService = serviceEditForm.Service;

            }
            return (configured) ? tmpService : null;
        }

        /// <summary>
        /// Enables or Disables buttons on the form
        /// </summary>
        void EnableButtons()
        {
            bool enable = _databaseServicesDataGridView.Rows.Count > 0;
            _modifyButton.Enabled = enable;
            _deleteButton.Enabled = enable;
        }

        #endregion

        #region Event Handlers

        void HandleDatabaseServicesDataGridViewCellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                var grid = (DataGridView)sender;

                if (e.RowIndex >= 0 && grid.Columns[e.ColumnIndex].Name == "Enabled")
                {
                    // Get the current row selected
                    var row = grid.CurrentRow;
                    if (row is null)
                    {
                        return;
                    }

                    DatabaseServiceData currentData = row.DataBoundItem as DatabaseServiceData;
                    bool newEnabledValue = !currentData.Enabled;

                    // Update the value in the database
                    using (var trans = new TransactionScope())
                    using (var connection = NewSqlDBConnection())
                    {
                        connection.Open();
                        var cmd = connection.CreateCommand();
                        cmd.CommandText = @"
                            UPDATE DatabaseService 
                            SET [Enabled] = @Enabled
                            WHERE ID = @DatabaseServiceID";
                        cmd.Parameters.AddWithValue("@Enabled", newEnabledValue);
                        cmd.Parameters.AddWithValue("@DatabaseServiceID", currentData.ID);
                        cmd.ExecuteNonQuery();
                        trans.Complete();

                        // update the data for the row
                        currentData.Enabled = newEnabledValue;
                    }
                }
                EnableButtons();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45643");
            }
        }

        void HandleModifyButtonClick(object sender, EventArgs e)
        {
            try
            {
                // Get the current row selected
                var row = _databaseServicesDataGridView.CurrentRow;
                if (row is null)
                {
                    return;
                }

                if (!GetCurrentRowDataFromDB(row))
                {
                    return;
                }

                DatabaseServiceData currentData = row.DataBoundItem as DatabaseServiceData;

                var service = EditService(currentData.Service, "Modify {0} database service.", currentData.ID);

                if (service != null)
                {
                    service.DatabaseServer = DatabaseServer;
                    service.DatabaseName = DatabaseName;

                    // Update the database service record in the database
                    service.UpdateDatabaseServiceSettings();

                    // update the data for the row
                    currentData.Description = service.Description;
                    currentData.Service = service;

                    EnableButtons();
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45591");
            }
        }

        void HandleAddButtonClick(object sender, EventArgs e)
        {
            try
            {
                SelectTypeByExtractCategoryForm<DatabaseService> typeForm =
                    new SelectTypeByExtractCategoryForm<DatabaseService>("DatabaseService");
                if (typeForm.ShowDialog() == DialogResult.OK)
                {
                    DatabaseService service = typeForm.TypeSelected;

                    service = EditService(service, "Add {0} database service.");

                    if (service != null)
                    {
                        int id = service.AddToDatabase(DatabaseServer, DatabaseName);

                        var newRcd = new DatabaseServiceData
                        {
                            ID = id,
                            Description = service.Description,
                            Service = service,
                            ServiceType = service.ExtractCategoryType,
                            Enabled = true
                        };
                        _listOfDataToDisplay.Add(newRcd);
                        int rowAdded = _databaseServicesDataGridView.Rows.GetLastRow(DataGridViewElementStates.None);
                        _databaseServicesDataGridView.CurrentCell = _databaseServicesDataGridView.Rows[rowAdded].Cells["Enabled"];

                        EnableButtons();
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45599");
            }
        }

        void HandleDeleteButtonClick(object sender, EventArgs e)
        {
            try
            {
                // Get the current row selected
                var row = _databaseServicesDataGridView.CurrentRow;
                if (row is null)
                {
                    return;
                }

                if (MessageBox.Show(
                    "Delete the current row?",
                    "Delete database service.",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Exclamation) == DialogResult.Yes)
                {
                    Int32 ID = (Int32)row.Cells["ID"].Value;

                    using (var trans = new TransactionScope())
                    using (var connection = NewSqlDBConnection())
                    {
                        connection.Open();
                        var cmd = connection.CreateCommand();
                        cmd.CommandText = @"
                                DELETE FROM DatabaseService 
                                WHERE ID = @DatabaseServiceID";
                        cmd.Parameters.AddWithValue("@DatabaseServiceID", ID);
                        cmd.ExecuteNonQuery();
                        trans.Complete();

                        _listOfDataToDisplay.Remove((DatabaseServiceData)row.DataBoundItem);
                    }
                }
                EnableButtons();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45608");
            }
        }

        void HandleRefreshButtonClick(object sender, EventArgs e)
        {
            try
            {
                LoadDataGrid();
                EnableButtons();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45609");
            }
        }

        void HandleDatabaseServicesDataGridViewCellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex >= 0)
                {
                    HandleModifyButtonClick(sender, e);
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45664");
            }
        }

        void HandleRestartETLButtonClick(object sender, EventArgs e)
        {
            try
            {
                _EtlRestartTime = DateTime.Now;

                // This is not a field or property because ManagerDatabaseServicesForm is used by FAMDBAdmin and
                // if FileProcessingDB is a property or field FAMDBAdmin doesn't always compile(C++ CLR project)
                FileProcessingDB famDB = new FileProcessingDB();
                famDB.DatabaseServer = DatabaseServer;
                famDB.DatabaseName = DatabaseName;

                famDB.SetDBInfoSetting("ETLRestart", _EtlRestartTime.ToString("yyyy-MM-ddTHH:mm:ss.fffzzz"), true, false);
                
                EnableButtons();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI46311");
            }

        }

        #endregion
    }
}
