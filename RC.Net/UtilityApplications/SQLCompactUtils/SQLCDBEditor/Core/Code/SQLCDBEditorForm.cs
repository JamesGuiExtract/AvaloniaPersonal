using Extract.Database;
using Extract.Licensing;
using Extract.Utilities;
using Extract.Utilities.Forms;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlServerCe;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Extract.SQLCDBEditor
{
    /// <summary>
    /// This is the main form used by the SQLCDBEditor application. It will open a 
    /// SQL Compact Database and display the tables in a list in the left pane and will
    /// display records from the currently selected table in the pane on the right.  Changes
    /// can be made to the data in the displayed records and can be saved when the save button
    /// is clicked or when prompted before closing or opening another database.
    /// </summary>
    public partial class SQLCDBEditorForm : Form
    {
        #region Internal Class

        /// <summary>
        /// Internal class that manages a data table, adapter and commandbuilder for each
        /// table in the compact database
        /// </summary>
        class DataTableInformation : IDisposable
        {
            #region Fields

            /// <summary>
            /// The data table associated with this information
            /// </summary>
            DataTable _table;

            /// <summary>
            /// The data adapter for the data table
            /// </summary>
            SqlCeDataAdapter _adapter;

            /// <summary>
            /// The command builder that enables insert/update/deletes to be run
            /// </summary>
            SqlCeCommandBuilder _command;

            #endregion Fields

            #region Constructor

            /// <summary>
            /// Initializes a new instance of the <see cref="DataTableInformation"/> class
            /// with an emtpy table.
            /// </summary>
            [SuppressMessage("Microsoft.Globalization", "CA1306:SetLocaleForDataTypes")]
            public DataTableInformation()
            {
                _table = new DataTable();
            }

            /// <summary>
            /// Initializes a new instance of the <see cref="DataTableInformation"/> class.
            /// </summary>
            /// <param name="table">The table.</param>
            /// <param name="adapter">The adapter.</param>
            public DataTableInformation(DataTable table, SqlCeDataAdapter adapter)
            {
                _table = table;
                _adapter = adapter;
                _command = new SqlCeCommandBuilder(adapter);
            }

            #endregion Constructor

            #region Methods

            /// <summary>
            /// Updates the underlying data table using the cached adapter
            /// </summary>
            public void UpdateTable()
            {
                _adapter.Update(_table);
            }

            #endregion Methods

            #region Properties

            /// <summary>
            /// Gets the data table.
            /// </summary>
            /// <value>The data table.</value>
            public DataTable DataTable
            {
                get
                {
                    return _table;
                }
            }

            #endregion Properties

            #region IDisposable Members

            /// <summary>
            /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
            /// </summary>
            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            /// <summary>
            /// Releases unmanaged and - optionally - managed resources
            /// </summary>
            /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged resources; <see langword="false"/> to release only unmanaged resources.</param>
            void Dispose(bool disposing)
            {
                if (disposing)
                {
                    if (_command != null)
                    {
                        _command.Dispose();
                        _command = null;
                    }
                    if (_adapter != null)
                    {
                        _adapter.Dispose();
                        _adapter = null;
                    }
                    if (_table != null)
                    {
                        _table.Dispose();
                        _table = null;
                    }
                }
            }

            #endregion
        }

        #endregion Internal Class

        #region Constants

        /// <summary>
        /// The name of this object.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(SQLCDBEditorForm).ToString();

        /// <summary>
        /// The default title for this form.
        /// </summary>
        const string _DEFAULT_TITLE = "SQLCDBEditor";

        #endregion Constants

        #region Fields

        /// <summary>
        /// File name of the currently opened database.
        /// </summary>
        String _databaseFileName = string.Empty;

        /// <summary>
        /// Opened connection to the current database.
        /// </summary>
        SqlCeConnection _connection;

        /// <summary>
        /// Dictionary mapping each table name to a class containing the table, adapter and
        /// commandbuilder
        /// </summary>
        Dictionary<string, DataTableInformation> _dictionaryOfTableInformation;

        /// <summary>
        /// Flag used to indicate that there are changes to the database that need to be saved.
        /// </summary>
        bool _dirty;

        /// <summary>
        /// Binding source used as dataGrid source, changes to the data display will be made
        /// directly to this object.
        /// </summary>
        BindingSource bindingSource = new BindingSource();

        /// <summary>
        /// Indicates whether this is running as a standalone app or as a dialog.
        /// </summary>
        bool _standAlone;

        /// <summary>
        /// Indicates whether the file that was open has been saved.
        /// </summary>
        bool _fileSaved;

        /// <summary>
        /// A custom value to display in the title bar.
        /// </summary>
        string _customTitle;

        /// <summary>
        /// The database schema updater used to update the current schema
        /// </summary>
        IDatabaseSchemaUpdater _schemaUpdater;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes the SQLCDBEditorForm
        /// </summary>
        public SQLCDBEditorForm()
            : this(null, true)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SQLCDBEditorForm"/> class.
        /// </summary>
        /// <param name="databaseFileName">Name of the database file.</param>
        public SQLCDBEditorForm(string databaseFileName)
            : this(databaseFileName, true)
        {
        }

        /// <summary>
        /// Initializes the SQLCDBEditorForm and allows specification of a database file to open
        /// after loading.
        /// </summary>
        /// <param name="databaseFileName">Name of the database file to load.</param>
        /// <param name="standAlone">Whether the form is being used as a standalone application
        /// or is being used as a dialog box.</param>
        public SQLCDBEditorForm(string databaseFileName, bool standAlone)
        {
            try
            {
                InitializeComponent();
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects,
                    "ELI29538", _OBJECT_NAME);
                _databaseFileName = databaseFileName;
                _standAlone = standAlone;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI30830", ex);
            }
        }

        #endregion Constructors

        #region Overrides

        /// <summary>
        /// Handles the <see cref="Form.Load"/> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs"/> associated with this event.</param>
        protected override void OnLoad(EventArgs e)
        {
            try
            {
                base.OnLoad(e);

                // Set the dataGridView datasource to the bindingSource
                _dataGridView.DataSource = bindingSource;

                // Initialize the dirty flag
                _dirty = false;

                if (!_standAlone)
                {
                    _closeToolStripMenuItem.Visible = false;
                    _openToolStripButton.Visible = false;
                    _openToolStripMenuItem.Visible = false;
                }

                // If there is a database file set open the database.
                if (!string.IsNullOrEmpty(_databaseFileName))
                {
                    // Invoking so that the Form completes loading before the database
                    // is opened. This way if the user is prompted about a database
                    // needing to be updated, the form will already be opened.
                    BeginInvoke((MethodInvoker)(() => { OpenDatabase(_databaseFileName); }));
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI29495", ex);
            }
            finally
            {
                EnableCommands();
            }
        }

        #endregion Overrides

        #region Event Handlers

        /// <summary>
        /// Handles the Form Closing event.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">The event data associated with the event.</param>
        void SQLCDBEditorForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                // Check for unsaved changes.
                if (CheckForSaveAndConfirm())
                {
                    // User canceled
                    e.Cancel = true;
                    return;
                }
                CloseDatabase();
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI29526", ex);

                if (MessageBox.Show("Do you wish to exit without saving?", "Close", MessageBoxButtons.YesNo,
                     MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, 0) != DialogResult.Yes)
                {
                    e.Cancel = true;
                }
            }
        }

        /// <summary>
        /// Handles the Open event from either the menu or the tool strip.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">The event data associated with the event.</param>
        void HandleOpenClick(object sender, EventArgs e)
        {
            try
            {
                // Check for unsaved changes.
                if (CheckForSaveAndConfirm())
                {
                    // User selected cancel to save confirmation, so nothing to do.
                    return;
                }

                // Setup OpenFileDialog to get the database to open
                using (OpenFileDialog openDatabaseFile = new OpenFileDialog())
                {
                    openDatabaseFile.DefaultExt = "sdf";
                    openDatabaseFile.Filter = "Database files (*.sdf)|*.sdf|All files (*.*)|*.*";

                    // Get the database to open
                    if (openDatabaseFile.ShowDialog() == DialogResult.OK)
                    {
                        // Open the selected database.
                        OpenDatabase(openDatabaseFile.FileName);
                    }
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI29496", ex);
            }
        }

        /// <summary>
        /// Handles the Save event from either the menu or the tool strip.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">The event data associated with the event.</param>
        void HandleSaveClick(object sender, EventArgs e)
        {
            try
            {
                // Save the changes.
                SaveDatabaseChanges();
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI29497", ex);
            }
        }

        /// <summary>
        /// Handles the Exit event from the menu.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">The event data associated with the event.</param>
        void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Close();
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI29626", ex);
            }
        }

        /// <summary>
        /// Handles the Close event from the menu and will close the currently open database.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">The event data associated with the event.</param>
        void CloseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (CheckForSaveAndConfirm())
                {
                    // Operation was canceled by user when saving.
                    return;
                }
                CloseDatabase();
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI29560", ex);
            }
        }

        /// <summary>
        /// Displays the about box.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">The event data associated with the event.</param>
        void AboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                AboutBox aboutBox = new AboutBox();
                aboutBox.ShowDialog();
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI29565", ex);
            }
        }

        /// <summary>
        /// Handles the SelectedValueChanged event on the listbox with table names.  This event
        /// is only active when the list box has been filed with table names.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">The event data associated with the event.</param>
        private void ListBoxTables_SelectedValueChanged(object sender, EventArgs e)
        {
            try
            {
                // Load the selected table into the dataGridView
                LoadTable(_listBoxTables.Text);
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI29498", ex);
            }
        }

        /// <summary>
        /// Handles the CurrentCellDirtyStateChanged event.  This is used to set the _dirty flag
        /// if when the current cell in the dataGridView is changed.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">The event data associated with the event.</param>
        void DataGridView_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            try
            {
                // Check if the current cell in the grid is dirty
                if (_dataGridView.IsCurrentCellDirty)
                {
                    // Set the dirty flag
                    _dirty = true;

                    // Update menu and tool strip
                    EnableCommands();

                    // Update the caption for the window to indicate that the database has been changed.
                    SetWindowTitle();
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI29512", ex);
            }
        }

        /// <summary>
        /// Handles the UserDeletedRow event of the dataGridView. This is used to set the _dirty flag
        /// if a record is deleted from the datagrid.  
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">The event data associated with the event.</param>
        void DataGridView_UserDeletedRow(object sender, DataGridViewRowEventArgs e)
        {
            try
            {
                // Set the dirty flag
                _dirty = true;

                // Update menu and tool strip
                EnableCommands();

                // Update the caption for the window to indicate that the database has been changed.
                SetWindowTitle();
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI29527", ex);
            }
        }

        /// <summary>
        /// Handles the <see cref="DataGridView.DataError"/> event.
        /// </summary>
        /// <param name="sender">The object which sent the event.</param>
        /// <param name="e">The data associated with the event.</param>
        void HandleDataGridViewError(object sender, DataGridViewDataErrorEventArgs e)
        {
            try
            {
                // Set throw exception to false so that the exception is not rethrown
                // after it has been displayed here
                e.ThrowException = false;

                // Wrap the exception as an ExtractException and display it
                ExtractException ee = ExtractException.AsExtractException("ELI30291", e.Exception);
                ee.AddDebugData("Data Row", e.RowIndex, false);
                ee.AddDebugData("Data Column", e.ColumnIndex, false);
                ee.Display();
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI30292", ex);
            }
        }

        /// <summary>
        /// Handles the update to current schema menu item click.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void HandleUpdateToCurrentSchemaClick(object sender, EventArgs e)
        {
            try
            {
                if (_schemaUpdater == null || !_schemaUpdater.IsUpdateRequired)
                {
                    return;
                }

                UpdateToCurrentSchema();
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI31163", ex);
            }
        }

        #endregion Event Handlers

        #region Private Methods

        /// <summary>
        /// Method loads the given table into the dataGridView.
        /// </summary>
        /// <param name="tableName">Name of the table to load.</param>
        [SuppressMessage("Microsoft.Globalization", "CA1306:SetLocaleForDataTypes")]
        void LoadTable(string tableName)
        {
            DataTableInformation info;

            // Check if the table to be loaded has been loaded before
            if (!string.IsNullOrWhiteSpace(tableName))
            {
                info = LoadTableFromDatabase(tableName);

                // Check for unique constraints
                bool _hasUniqueContraint = false;
                foreach (Constraint c in info.DataTable.Constraints)
                {
                    // Check if current constraint is unique					
                    if (c is UniqueConstraint)
                    {
                        // set Flag to true;
                        _hasUniqueContraint = true;
                        break;
                    }
                }

                // Set the ReadOnly and AllowUserToAddRows so that if no unique constraint
                // the grid cannot be modified and rows cannot be added
                _dataGridView.ReadOnly = !_hasUniqueContraint;
                _dataGridView.AllowUserToAddRows = _hasUniqueContraint;
            }
            else
            {
                // If the table has not been created create an empty table

                info = new DataTableInformation();
            }

            var table = info.DataTable;

            // Set the bindingSoure dataSource to the table
            bindingSource.DataSource = table;

            using (var graphics = _dataGridView.CreateGraphics())
            {
                var font = _dataGridView.ColumnHeadersDefaultCellStyle.Font;
                for (int i = 0; i < table.Columns.Count; i++)
                {
                    var column = table.Columns[i];
                    var type = column.DataType;
                    float fillWeight = 200;
                    if (type == typeof(bool))
                    {
                        fillWeight = 100;
                    }
                    else if (type == typeof(string))
                    {
                        if (column.MaxLength > 500)
                        {
                            fillWeight = 450;
                        }
                        else if (column.MaxLength > 300)
                        {
                            fillWeight = 400;
                        }
                        else if (column.MaxLength > 150)
                        {
                            fillWeight = 350;
                        }
                        else
                        {
                            fillWeight = 250;
                        }
                    }

                    // Get the head text and measure out the minimum width to display the string
                    // Pad the width by 4 pixels (2 each side)
                    var temp = _dataGridView.Columns[i].HeaderText;
                    var size = graphics.MeasureString(temp, font);
                    _dataGridView.Columns[i].MinimumWidth = (int)(size.Width + 0.5) + 4;
                    _dataGridView.Columns[i].FillWeight = fillWeight;
                    _dataGridView.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                }
            }

            // Reset the current cell
            _dataGridView.CurrentCell = null;
        }

        /// <summary>
        /// Loads the table from database.
        /// </summary>
        /// <param name="tableName">Name of the table to load.</param>
        /// <returns>The loaded table.</returns>
        [SuppressMessage("Microsoft.Globalization", "CA1306:SetLocaleForDataTypes")]
        DataTableInformation LoadTableFromDatabase(string tableName)
        {
            // Check if this table has been loaded already
            DataTableInformation info = null;
            if (_dictionaryOfTableInformation.TryGetValue(tableName, out info))
            {
                // Return the loaded table
                return info;
            }

            // Table has not been loaded before so load it
            var table = new DataTable();

            // Setup dataAdapter to get the data
            SqlCeDataAdapter dataAdapter = new SqlCeDataAdapter("SELECT * FROM " + tableName, _connection);

            // Fill the table with the data from the dataAdapter
            dataAdapter.Fill(table);

            // Fill the schema for the table for the database
            dataAdapter.FillSchema(table, SchemaType.Source);

            // Check for auto increment fields and default column values
            foreach (DataColumn c in table.Columns)
            {
                // Get the information for the current column
                using (SqlCeCommand sqlcmd = new SqlCeCommand(
                    "SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '" +
                    tableName + "' AND COLUMN_NAME = '" + c.ColumnName + "'", _connection))
                {
                    using (SqlCeResultSet columnsResult =
                        sqlcmd.ExecuteResultSet(ResultSetOptions.Scrollable))
                    {
                        int colPos;

                        // Get the first record in the result set - should only be one
                        if (columnsResult.ReadFirst())
                        {
                            // If the column is an auto increment column set the seed value to next
                            // auto increment value for the column
                            if (c.AutoIncrement)
                            {
                                // Get the position of the AUTOINC_NEXT field
                                colPos = columnsResult.GetOrdinal("AUTOINC_NEXT");

                                // Set the seed to the value in the AUTOINC_NEXT field
                                c.AutoIncrementSeed = (long)columnsResult.GetValue(colPos);
                            }

                            // Set the default for a column if one is defined
                            colPos = columnsResult.GetOrdinal("COLUMN_HASDEFAULT");
                            if (columnsResult.GetBoolean(colPos))
                            {
                                // Set the default value for the column
                                colPos = columnsResult.GetOrdinal("COLUMN_DEFAULT");
                                c.DefaultValue = columnsResult.GetValue(colPos);
                            }
                        }
                    }
                }
            }

            info = new DataTableInformation(table, dataAdapter);
            _dictionaryOfTableInformation.Add(tableName, info);

            return info;
        }

        /// <summary>
        /// Method loads the names of the tables in the opened database into the list box. This
        /// method does not save any changes to the database that was previously loaded. If the 
        /// database should be saved it should be done before calling this method.
        /// </summary>
        [SuppressMessage("Microsoft.Globalization", "CA1306:SetLocaleForDataTypes")]
        HashSet<string> LoadTableNames()
        {
            // Remove the handler for the SelectedValueChanged event while loading the list box
            _listBoxTables.SelectedValueChanged -= ListBoxTables_SelectedValueChanged;

            SqlCeDataAdapter tableListDataAdapter = null;
            try
            {
                // If dictionary of table information already exist clear the values it contains.
                if (_dictionaryOfTableInformation != null)
                {
                    CollectionMethods.ClearAndDispose(_dictionaryOfTableInformation);
                }

                // Create a new dictionary of table information
                _dictionaryOfTableInformation = new Dictionary<string, DataTableInformation>();

                // Create adapter for the list of tables.
                tableListDataAdapter =
                    new SqlCeDataAdapter("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES", _connection);

                // Create a new DataTable for the table names in this database and fill it
                var tableNameTable = new DataTable();
                tableListDataAdapter.Fill(tableNameTable);

                // Set the member to display from the table
                _listBoxTables.DisplayMember = "TABLE_NAME";

                // Set the listbox datasource to the tableNameTable
                _listBoxTables.DataSource = tableNameTable;

                // Reset the _dirty flag
                _dirty = false;

                // Update menu and tool strip
                EnableCommands();

                var tableNames = new HashSet<string>();
                foreach (DataRow row in tableNameTable.Rows)
                {
                    tableNames.Add(row["TABLE_NAME"].ToString());
                }

                return tableNames;
            }
            catch (Exception ex)
            {
                throw new ExtractException("ELI29627", "Unable to load tables.", ex);
            }
            finally
            {
                if (tableListDataAdapter != null)
                {
                    tableListDataAdapter.Dispose();
                }

                // Activate the SelectedValueChanged event handler
                _listBoxTables.SelectedValueChanged += ListBoxTables_SelectedValueChanged;
            }
        }

        /// <summary>
        /// Method creates a transaction and saves all of the changes to the database and if there 
        /// were no errors on save, displays a dialog indicating a successful save.
        /// </summary>
        void SaveDatabaseChanges()
        {
            // Check for a loaded database file.
            if (string.IsNullOrEmpty(_databaseFileName))
            {
                return;
            }
            // Display wait cursor while saving the changes
            using (new TemporaryWaitCursor())
            {
                // Check for an edit in progress on the current row in the datagrid
                if (_dataGridView.IsCurrentRowDirty)
                {
                    // Change the current cell to get the changes to the current cell saved to 
                    // the datasource
                    _dataGridView.CurrentCell = null;
                }

                // Begin a transaction so all or none of the changes are commited
                SqlCeTransaction transaction = _connection.BeginTransaction();
                try
                {
                    // Go through all of the table information objects in the dictionary and
                    // update them.
                    foreach (var tableinformation in _dictionaryOfTableInformation.Values)
                    {
                        tableinformation.UpdateTable();
                    }

                    // Commit the transaction
                    transaction.Commit();

                    _fileSaved = true;
                }
                catch (Exception ex)
                {
                    // Rollback the transaction
                    transaction.Rollback();

                    // Throw exception that there was a problem saving the database
                    ExtractException ee = new ExtractException("ELI29529", "Error saving database", ex);
                    throw ee;
                }
                finally
                {
                    transaction.Dispose();
                }

                // Clear _dirty flag
                _dirty = false;

                // Update window text
                SetWindowTitle();

                // Update menu and tool strip
                EnableCommands();

                // Display message that save was successful.
                MessageBox.Show("Database was saved successfully.", "Database save",
                    MessageBoxButtons.OK, MessageBoxIcon.Information,
                    MessageBoxDefaultButton.Button1, 0);

                // Open database to refresh the tables.
                OpenDatabase(_databaseFileName);
            }
        }

        /// <summary>
        /// Method opens a connection to the database.
        /// </summary>
        /// <param name="databaseToOpen">SQL CE Database file to open.</param>
        void OpenDatabase(string databaseToOpen)
        {
            try
            {
                // If same database is open need to save the current table
                string currentlyOpenTable = "";
                if (databaseToOpen == _databaseFileName)
                {
                    // reopening the current database.
                    currentlyOpenTable = _listBoxTables.Text;
                }

                // Reset the schema updater
                _schemaUpdater = null;

                // Close the database
                CloseDatabase();

                // Set the _databaseFileName
                _databaseFileName = databaseToOpen;

                // Create the connection to the database
                _connection = new SqlCeConnection(
                    SqlCompactMethods.BuildDBConnectionString(_databaseFileName, true));

                // Open the connection.
                _connection.Open();

                // Load the table names into the listBox
                var tableNames = LoadTableNames();

                // If this database was previously open reopen the same table
                if (!string.IsNullOrEmpty(currentlyOpenTable))
                {
                    _listBoxTables.SelectedIndex = _listBoxTables.FindStringExact(currentlyOpenTable);
                }

                // Load the currently selected table into the dataGridView
                LoadTable(_listBoxTables.Text);

                SetWindowTitle();

                _fileSaved = false;

                CheckSchemaVersionAndPromptForUpdate(tableNames);
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI31165", ex);
            }
            finally
            {
                // Make sure the commands are enabled/disabled appropriately
                EnableCommands();
            }
        }

        /// <summary>
        /// Checks the schema version and prompts the user if an update is required.
        /// <para><b>Note:</b></para>
        /// This will also update the status bar in the UI to indicate known schema,
        /// unknown schema, update needed.
        /// </summary>
        /// <param name="tableNames">The table names.</param>
        void CheckSchemaVersionAndPromptForUpdate(HashSet<string> tableNames)
        {
            _schemaUpdater = GetSchemaUpdater(tableNames);
            string statusText = "Database schema is current.";
            Color textColor = Color.Green;
            bool promptForUpdate = false;
            bool canUpdateSchema = false;
            if (_schemaUpdater != null)
            {
                _schemaUpdater.SetDatabaseConnection(_connection);
                if (_schemaUpdater.IsUpdateRequired)
                {
                    promptForUpdate = true;
                    canUpdateSchema = true;
                    textColor = Color.Black;
                    statusText = "Database schema requires updating.";
                }
                else if (_schemaUpdater.IsNewerVersion)
                {
                    statusText = "Database was created in a newer version.";
                    textColor = Color.Red;
                    canUpdateSchema = false;
                }
            }
            else
            {
                textColor = Color.Red;
                statusText = "Unknown Database Schema";
            }

            _updateToCurrentSchemaToolStripMenuItem.Enabled = canUpdateSchema;
            _statusLabelSchemaInfo.ForeColor = textColor;
            _statusLabelSchemaInfo.Text = statusText;

            if (promptForUpdate)
            {
                var result = MessageBox.Show(
                    "Database schema requires an update, would you like to update now?",
                    "Schema out of date", MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, 0);
                if (result == DialogResult.Yes)
                {
                    // Invoking the method here so that the update can be started after
                    // this method finishes execution
                    BeginInvoke((MethodInvoker)(() => { UpdateToCurrentSchema(); }));
                }
            }
        }

        /// <summary>
        /// Method sets the windows title as SQLCDBEditor if no database file is loaded and if a 
        /// database file is loaded appends the filename preceeded by an * if the changes have been
        /// made since it was loaded.
        /// </summary>
        void SetWindowTitle()
        {
            if (!string.IsNullOrEmpty(_customTitle))
            {
                Text = _customTitle + (_dirty ? "*" : "");
            }
            else if (!string.IsNullOrEmpty(_databaseFileName))
            {
                Text = _DEFAULT_TITLE + " - " + _databaseFileName + ((_dirty) ? "*" : "");
            }
            else
            {
                Text = _DEFAULT_TITLE;
            }
        }

        /// <summary>
        /// Method checks if the database is dirty and if it is will ask the user if they want to
        /// save changes. 
        /// </summary>
        /// <returns>Return value is <see langword="true"/> if user selected cancel in the dialog.</returns>
        [SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions")]
        bool CheckForSaveAndConfirm()
        {
            // Before opening a new database check to see if there are changes to be saved
            if (_dirty)
            {
                // Ask user if changes should be saved
                DialogResult result = MessageBox.Show("Save changes?", "Save",
                    MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

                // If yes save the changes
                if (result == DialogResult.Yes)
                {
                    SaveDatabaseChanges();
                }
                else if (result == DialogResult.Cancel)
                {
                    // If cancel was selected there is nothing further to do
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Method enables or disables commands based on the _dirty flag and whether _databaseFileName 
        /// is an empty string indicating there is no database loaded.
        /// </summary>
        void EnableCommands()
        {
            _closeToolStripMenuItem.Enabled = !string.IsNullOrEmpty(_databaseFileName);
            _saveToolStripButton.Enabled = _dirty;
            _saveToolStripMenuItem.Enabled = _dirty;
            _updateToCurrentSchemaToolStripMenuItem.Enabled =
                _schemaUpdater != null
                && _schemaUpdater.IsUpdateRequired;
        }

        /// <summary>
        /// Method closes the database
        /// </summary>
        void CloseDatabase()
        {
            // Remove the handler for the SelectedValueChanged event.
            _listBoxTables.SelectedValueChanged -= ListBoxTables_SelectedValueChanged;

            // Clear the listbox
            IDisposable d = _listBoxTables.DataSource as IDisposable;
            if (d != null)
            {
                d.Dispose();
            }
            _listBoxTables.DataSource = null;

            // Clear the data grid binding source
            d = bindingSource.DataSource as IDisposable;
            if (d != null)
            {
                d.Dispose();
            }
            bindingSource.DataSource = null;


            // Clear table information
            if (_dictionaryOfTableInformation != null)
            {
                CollectionMethods.ClearAndDispose(_dictionaryOfTableInformation);
                _dictionaryOfTableInformation = null;
            }

            // Clear database file name
            _databaseFileName = "";

            // Clear Dirty Flag
            _dirty = false;

            _statusLabelSchemaInfo.Text = "";

            // Update menu and tool strip
            EnableCommands();

            // Close an open connection
            if (_connection != null)
            {
                // Close the connection
                _connection.Close();

                // Dispose of the connection
                _connection.Dispose();

                // Set to null
                _connection = null;
            }

            SetWindowTitle();
        }

        /// <summary>
        /// Attempts to get the <see cref="IDatabaseSchemaUpdater"/> for known schemas.
        /// </summary>
        /// <param name="tableNames">The table names for the current database.</param>
        IDatabaseSchemaUpdater GetSchemaUpdater(HashSet<string> tableNames)
        {
            IDatabaseSchemaUpdater updater = null;
            if (tableNames.Contains("Settings"))
            {
                var info = LoadTableFromDatabase("Settings");

                // Look for the schema manager
                var result = info.DataTable.Select("Name = '"
                    + DatabaseHelperMethods.DatabaseSchemaManagerKey + "'");
                if (result.Length == 1)
                {
                    // Build the name to the assembly containing the manager
                    var className = result[0]["Value"].ToString();
                    updater =
                        UtilityMethods.CreateTypeFromTypeName(className) as IDatabaseSchemaUpdater;
                    if (updater == null)
                    {
                        var ee = new ExtractException("ELI31154",
                            "Database contained an entry for schema manager, "
                        + "but it does not contain a schema updater.");
                        ee.AddDebugData("Class Name", className, false);
                        throw ee;
                    }
                }
                else
                {
                    // No schema updater defined. Check for FPSFile table
                    if (tableNames.Contains("FPSFile"))
                    {
                        updater = (IDatabaseSchemaUpdater)UtilityMethods.CreateTypeFromTypeName(
                            "Extract.FileActionManager.Database.FAMServiceDatabaseManager");
                    }
                }
            }
            else
            {
                // Check for expected LabDE order mapper tables
                if (tableNames.Contains("LabOrder") && tableNames.Contains("LabTest")
                    && tableNames.Contains("LabOrderTest") && tableNames.Contains("AlternateTestName"))
                {
                    updater = (IDatabaseSchemaUpdater)UtilityMethods.CreateTypeFromTypeName(
                        "Extract.LabResultsCustomComponents.OrderMapperDatabaseSchemaManager");
                }
            }

            return updater;
        }

        /// <summary>
        /// Updates to current schema.
        /// </summary>
        void UpdateToCurrentSchema()
        {
            try
            {
                // Store the db name and close the open database
                string tempName = _databaseFileName;
                CloseDatabase();

                using (var eventHandle = new ManualResetEvent(false))
                using (var waitForm = new PleaseWaitForm("Please wait while the database schema is updated.",
                    eventHandle, 3))
                {
                    var tempTask = Task.Factory.StartNew(() =>
                    {
                        using (var connection = new SqlCeConnection(
                            SqlCompactMethods.BuildDBConnectionString(tempName, false)))
                        {
                            _schemaUpdater.SetDatabaseConnection(connection);
                            try
                            {
                                var task =
                                    _schemaUpdater.BeginUpdateToLatestSchema(null,
                                    new CancellationTokenSource());
                                task.Wait();
                            }
                            finally
                            {
                                eventHandle.Set();
                            }
                        }
                    });

                    waitForm.ShowDialog();

                    // Perform wait (this forces any aggregate exceptions
                    // from the task to be thrown)
                    try
                    {
                        tempTask.Wait();
                    }
                    catch (AggregateException ex)
                    {
                        throw ExtractException.AsExtractException("ELI31164", ex.Flatten());
                    }
                }

                // Invoking so that the update method completes and then the prompt is displayed
                // before the database is reopened.
                BeginInvoke((MethodInvoker)(() =>
                {
                    MessageBox.Show(this, "Database has been updated to current schema.",
                        "Database Updated", MessageBoxButtons.OK, MessageBoxIcon.Information,
                        MessageBoxDefaultButton.Button1, 0);

                    OpenDatabase(tempName);
                }));
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI31167", ex);
            }
        }

        #endregion Private Methods

        #region Properties

        /// <summary>
        /// Gets the default title for the window.
        /// </summary>
        /// <value>The default title.</value>
        public static string DefaultTitle
        {
            get
            {
                return _DEFAULT_TITLE;
            }
        }

        /// <summary>
        /// Gets or sets the custom title for the <see cref="Form"/>.
        /// </summary>
        /// <value>The custom title.</value>
        public string CustomTitle
        {
            get
            {
                return _customTitle;
            }
            set
            {
                _customTitle = value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the open file has been saved.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the open file has been saved; otherwise, <see langword="false"/>.
        /// </value>
        public bool FileSaved
        {
            get
            {
                return _fileSaved;
            }
        }

        #endregion Properties
    }
}