using Extract.Licensing;
using Extract.Utilities;
using Extract.Utilities.Forms;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlServerCe;
using System.Diagnostics.CodeAnalysis;
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
        /// Dictionary that stores the DataTable using the table name as a key.
        /// This keeps the table that was set as the DataSource of the grid if another
        /// table was selected.
        /// </summary>
        Dictionary<string, DataTable> _dictionaryOfTables;

        /// <summary>
        /// Dictionary that stores the adapter used to fill the DataTable object for the table name.
        /// The key is the table name.
        /// This keeps the DataAdapter the will need to be used to call update to save changes. 
        /// </summary>
        Dictionary<string, SqlCeDataAdapter> _dictionaryOfAdapters;

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
                dataGridView.DataSource = bindingSource;

                // Initialize the dirty flag
                _dirty = false;

                // If there is a database file set open the database.
                if (!string.IsNullOrEmpty(_databaseFileName))
                {
                    OpenDatabase(_databaseFileName);
                }

                if (!_standAlone)
                {
                    closeToolStripMenuItem.Visible = false;
                    openToolStripButton.Visible = false;
                    openToolStripMenuItem.Visible = false;
                }

                EnableCommands();
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI29495", ex);
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
                LoadTable(listBoxTables.Text);
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
                if (dataGridView.IsCurrentCellDirty)
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

        #endregion Event Handlers

        #region Private Methods

        /// <summary>
        /// Method loads the given table into the dataGridView.
        /// </summary>
        /// <param name="tableName">Name of the table to load.</param>
        [SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "commandBuilder")]
        [SuppressMessage("Microsoft.Globalization", "CA1306:SetLocaleForDataTypes")]
        void LoadTable(string tableName)
        {
            DataTable table;

            // Check if the table to be loaded has been loaded before
            if (!String.IsNullOrEmpty(tableName))
            {
                if (!_dictionaryOfTables.TryGetValue(tableName, out table))
                {
                    // Table has not been loaded before so load it
                    table = new DataTable();

                    // Setup dataAdapter to get the data
                    SqlCeDataAdapter dataAdapter = new SqlCeDataAdapter("SELECT * FROM " + tableName, _connection);

                    // Setup of the commands for Select, Update, and Delete
                    SqlCeCommandBuilder commandBuilder = new SqlCeCommandBuilder(dataAdapter);

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

                    // Add the new adapter to the list of adapters
                    _dictionaryOfAdapters.Add(tableName, dataAdapter);

                    // Add the new table to the list of tables
                   _dictionaryOfTables.Add(tableName, table);
                }

                // Check for unique constraints
                bool _hasUniqueContraint = false;
                foreach (Constraint c in table.Constraints)
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
                dataGridView.ReadOnly = !_hasUniqueContraint;
                dataGridView.AllowUserToAddRows = _hasUniqueContraint;
            }
            else
            {
               // If the table has not been created create an empty table
                table = new DataTable();
            }
            // Set the bindingSoure dataSource to the table
            bindingSource.DataSource = table;
            
            // Reset the current cell
            dataGridView.CurrentCell = null;
        }

        /// <summary>
        /// Method loads the names of the tables in the opened database into the list box. This
        /// method does not save any changes to the database that was previously loaded. If the 
        /// database should be saved it should be done before calling this method.
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "commandBuilder")]
        [SuppressMessage("Microsoft.Globalization", "CA1306:SetLocaleForDataTypes")]
        void LoadTableNames()
        {
            // Remove the handler for the SelectedValueChanged event while loading the list box
            listBoxTables.SelectedValueChanged -= ListBoxTables_SelectedValueChanged;

            try
            {
                // If dictionary of tables already exist clear the values it contains.
                if (_dictionaryOfTables != null)
                {
                    CollectionMethods.ClearAndDispose(_dictionaryOfTables);
                }

                // Create a new dictionary of Tables
                _dictionaryOfTables = new Dictionary<string, DataTable>();

                // If dictionary of Adapters already exists clear the values it contains
                if (_dictionaryOfAdapters != null)
                {
                    CollectionMethods.ClearAndDispose(_dictionaryOfAdapters);
                }

                // Create a new dictionary of adapters
                _dictionaryOfAdapters = new Dictionary<string, SqlCeDataAdapter>();

                // Create a new DataTable for the table names in this database
                DataTable tableNameTable = new DataTable();

                // Create adapter for the list of tables.
                SqlCeDataAdapter tableListDataAdapter =
                    new SqlCeDataAdapter("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES", _connection);

                // Create the commands for the tableListDataAdapter 
                SqlCeCommandBuilder commandBuilder = new SqlCeCommandBuilder(tableListDataAdapter);

                // Fill the tableNameTable with data from the adapter
                tableListDataAdapter.Fill(tableNameTable);

                // Set the member to display from the table
                listBoxTables.DisplayMember = "TABLE_NAME";

                // Set the listbox datasource to the tableNameTable
                listBoxTables.DataSource = tableNameTable;

                // Reset the _dirty flag
                _dirty = false;

                // Update menu and tool strip
                EnableCommands();
            }
            catch (Exception ex)
            {
                throw new ExtractException("ELI29627", "Unable to load tables.", ex);
            }
            finally
            {
                // Activate the SelectedValueChanged event handler
                listBoxTables.SelectedValueChanged += ListBoxTables_SelectedValueChanged;
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
                if (dataGridView.IsCurrentRowDirty)
                {
                    // Change the current cell to get the changes to the current cell saved to 
                    // the datasource
                    dataGridView.CurrentCell = null;
                }

                // Begin a transaction so all or non of the changes are commited
                SqlCeTransaction transaction = _connection.BeginTransaction();
                try
                {
                    // Go through all of the tables in the ditionary of tables
                    foreach (KeyValuePair<string, DataTable> kv in _dictionaryOfTables)
                    {
                        // Get the DataTable
                        DataTable dt = kv.Value;

                        // Try to get the adapter for that table
                        SqlCeDataAdapter da;
                        if (_dictionaryOfAdapters.TryGetValue(kv.Key, out da))
                        {
                            // Update the the database
                            da.Update(dt);
                        }
                    }

                    // Commit the transaction
                    transaction.Commit();

                    // Clear _dirty flag
                    _dirty = false;

                    // Update window text
                    SetWindowTitle();

                    // Update menu and tool strip
                    EnableCommands();

                    // Open database to refresh the tables.
                    OpenDatabase(_databaseFileName);

                    // Display message that save was successful.
                    MessageBox.Show("Database was saved successfully.", "Database save",
                        MessageBoxButtons.OK, MessageBoxIcon.Information,
                        MessageBoxDefaultButton.Button1, 0);

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
            }
       }

        /// <summary>
        /// Method opens a connection to the database.
        /// </summary>
        /// <param name="databaseToOpen">SQL CE Database file to open.</param>
        void OpenDatabase(string databaseToOpen)
        {
            // If same database is open need to save the current table
            string currentlyOpenTable = "";
            if (databaseToOpen == _databaseFileName)
            {
                // reopening the current database.
                currentlyOpenTable = listBoxTables.Text;
            }

            // Close the database
            CloseDatabase();

            // Set the _databaseFileName
            _databaseFileName = databaseToOpen;

            // Create the connection to the database
            _connection = new SqlCeConnection(@"Data Source=" + _databaseFileName + ";File Mode=Exclusive;");

            // Open the connection.
            _connection.Open();

            // Load the table names into the listBox
            LoadTableNames();

            // If this database was previously open reopen the same table
            if (!string.IsNullOrEmpty(currentlyOpenTable))
            {
                listBoxTables.SelectedIndex = listBoxTables.FindStringExact(currentlyOpenTable);
            }

            // Load the currently selected table into the dataGridView
            LoadTable(listBoxTables.Text);

            SetWindowTitle();

            _fileSaved = false;
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
            closeToolStripMenuItem.Enabled = !string.IsNullOrEmpty(_databaseFileName);
            saveToolStripButton.Enabled = _dirty;
            saveToolStripMenuItem.Enabled = _dirty;
        }

        /// <summary>
        /// Method closes the database
        /// </summary>
        void CloseDatabase()
        {
            // Remove the handler for the SelectedValueChanged event.
            listBoxTables.SelectedValueChanged -= ListBoxTables_SelectedValueChanged;

            // Clear the listbox
            IDisposable d = (IDisposable)listBoxTables.DataSource;
            if (d != null)
            {
                d.Dispose();
            }
            listBoxTables.DataSource = null;

            // Clear the data grid binding source
            d = (IDisposable)bindingSource.DataSource;
            if (d != null)
            {
                d.Dispose();
            }
            bindingSource.DataSource = null;


            // Clear dictionary of adapters
            if (_dictionaryOfAdapters != null)
            {
                CollectionMethods.ClearAndDispose(_dictionaryOfAdapters);
                _dictionaryOfAdapters = null;
            }

            // Clear dictionary of tables
            if (_dictionaryOfTables != null)
            {
                CollectionMethods.ClearAndDispose(_dictionaryOfTables);
                _dictionaryOfTables = null;
            }

            // Clear database file name
            _databaseFileName = "";

            // Clear Dirty Flag
            _dirty = false;

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