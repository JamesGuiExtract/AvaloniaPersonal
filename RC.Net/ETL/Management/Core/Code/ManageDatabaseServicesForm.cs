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
                PropertyChangedEventHandler handler = PropertyChanged;
                if (handler != null)
                {
                    handler(this, new PropertyChangedEventArgs(propertyName));
                }
            }

            #endregion
        }

        #endregion

        #region Fields

        /// <summary>
        /// Binding list of data being displayed in data grid
        /// </summary>
        BindingList<DatabaseServiceData> _listOfDataToDisplay;

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
                loadDataGrid();
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
        void loadDataGrid()
        {
            try
            {
                var table = new DataTable();
                using (var connection = NewSqlDBConnection())
                {
                    connection.Open();

                    var command = connection.CreateCommand();
                    command.CommandText = "SELECT ID, Description, Settings, Status, Enabled FROM dbo.DatabaseService";

                    table.Load(command.ExecuteReader());

                    var dataToDisplay = table.AsEnumerable()
                        .Select(r => new DatabaseServiceData
                        {
                            ID = r.Field<Int32>("ID"),
                            Description = r.Field<string>("Description"),
                            Service = DatabaseService.FromJson(r.Field<string>("Settings")),
                            Enabled = r.Field<bool>("Enabled")
                        }).ToList();

                    _listOfDataToDisplay = new BindingList<DatabaseServiceData>(dataToDisplay);
                    BindingSource bindingSource = new BindingSource();
                    bindingSource.DataSource = _listOfDataToDisplay;

                    _databaseServicesDataGridView.DataSource = bindingSource;

                    // Hide the ID column
                    _databaseServicesDataGridView.Columns["ID"].Visible = false;
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
        /// Gets the row data from the database if there is not matching ID in the DB the row will be deleted from the bound data
        /// </summary>
        /// <param name="row">Row to retrieve from the database</param>
        /// <returns></returns>
        bool GetCurrentRowDataFromDB(DataGridViewRow row)
        {
            DatabaseServiceData currentData = row.DataBoundItem as DatabaseServiceData;

            // Get current data from the database
            using (var connection = NewSqlDBConnection())
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText =
                    "SELECT ID, Description, Settings, Status, Enabled FROM dbo.DatabaseService WHERE ID = @DatabaseServiceID";
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

                DatabaseServiceEditForm serviceEditForm =
                    new DatabaseServiceEditForm(currentData.Description, currentData.Service);

                serviceEditForm.Text = String.Format(CultureInfo.InvariantCulture,
                    "Modify {0} database service.", currentData.Service.GetType().Name);


                if (serviceEditForm.ShowDialog(this) == DialogResult.OK)
                {
                    string newDescription = serviceEditForm.Description;
                    DatabaseService newService = serviceEditForm.Service;

                    if (newDescription != currentData.Description || currentData.Service != newService)
                    {
                        using (var trans = new TransactionScope())
                        using (var connection = NewSqlDBConnection())
                        {
                            connection.Open();
                            var cmd = connection.CreateCommand();
                            cmd.CommandText = @"
                                UPDATE DatabaseService 
                                SET [Description] = @Description,
                                    [Settings]    = @Settings
                                WHERE ID = @DatabaseServiceID";
                            cmd.Parameters.AddWithValue("@Description", newDescription);
                            cmd.Parameters.AddWithValue("@Settings", newService.ToJson());
                            cmd.Parameters.AddWithValue("@DatabaseServiceID", currentData.ID);
                            cmd.ExecuteNonQuery();
                            trans.Complete();

                            // update the data for the row
                            currentData.Description = newDescription;
                            currentData.Service = newService;
                        }
                    }
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
                SelectTypeByExtractCategoryForm typeForm = new SelectTypeByExtractCategoryForm("DatabaseService");
                if (typeForm.ShowDialog() == DialogResult.OK)
                {
                    DatabaseService service = (DatabaseService)typeForm.TypeSelected;
                    DatabaseServiceEditForm serviceEditForm =
                        new DatabaseServiceEditForm(string.Empty, service);

                    serviceEditForm.Text = String.Format(CultureInfo.InvariantCulture,
                        "Add {0} database service.", service.GetType().Name);

                    if (serviceEditForm.ShowDialog(this) == DialogResult.OK)
                    {
                        using (var trans = new TransactionScope())
                        using (var connection = NewSqlDBConnection())
                        {
                            connection.Open();
                            var cmd = connection.CreateCommand();
                            cmd.CommandText = @"
                                INSERT INTO [dbo].[DatabaseService]
                                            ([Description]
                                            ,[Settings]
                                            ,[Enabled]
                                            )
	                            OUTPUT inserted.id
                                VALUES (
                                    @Description,
                                    @Settings,
                                    @Enabled)";

                            cmd.Parameters.AddWithValue("@Description", serviceEditForm.Description);
                            cmd.Parameters.AddWithValue("@Settings", service.ToJson());
                            cmd.Parameters.AddWithValue("@Enabled", true);
                            Int32 id = (Int32)cmd.ExecuteScalar();
                            trans.Complete();

                            var newRcd = new DatabaseServiceData()
                            {
                                ID = id,
                                Description = serviceEditForm.Description,
                                Service = serviceEditForm.Service,
                                Enabled = true
                            };
                            _listOfDataToDisplay.Add(newRcd);
                        }
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
                loadDataGrid();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45609");
            }
        }

        #endregion
    }
}
