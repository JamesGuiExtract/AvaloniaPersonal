using Extract.Database;
using Extract.Database.Sqlite;
using Extract.Utilities;
using Extract.Utilities.Forms;
using System;
using System.Data;
using System.Data.Common;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Extract.SQLCDBEditor
{
    /// <summary>
    /// Form used for importing data from an input file into a database.
    /// </summary>
    public partial class ImportTableForm : Form
    {
        #region Fields

        /// <summary>
        /// The data adapter to populate <see cref="_resultsTable"/> for database tables.
        /// </summary>
        DbDataAdapter _adapter = null;

        /// <summary>
        /// The data table representing the table contents or query results.
        /// </summary>
        DataTable _resultsTable = null;

        /// <summary>
        /// Allow caller to get the modified table name, iff the append/replace was successful.
        /// </summary>
        public string ModifiedTableName { get; private set; }

        /// <summary>
        /// The database filename, passed from parent form.
        /// </summary>
        string _databaseFilename;

        /// <summary>
        /// The currently selected table name.
        /// </summary>
        string _tablename;

        /// <summary>
        /// The currently selected data file name (containing data to apply to the selected table)
        /// </summary>
        string _dataFilename;

        /// <summary>
        /// The active, open connection passed from the parent form.
        /// </summary>
        DbConnection _connection;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Default CTOR
        /// </summary>
        public ImportTableForm(string databaseFileName, string[] tableNames, DbConnection connection)
        {
            ExtractException.Assert("ELI39099", "Database file name is empty", !String.IsNullOrEmpty(databaseFileName));
            _databaseFilename = databaseFileName;
            _connection = connection;

            InitializeComponent();

            PopulateDatabaseTableNames(tableNames);

            // Set the maximum form size according to the screen resolution, 
            // to ensure that the form will never resize beyond the window bounds.
            int height = Screen.PrimaryScreen.Bounds.Height;
            int width = Screen.PrimaryScreen.Bounds.Width;
            this.MaximumSize = new System.Drawing.Size(width, height);
        }

        #endregion Constructors

        #region Overrides

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (null != _resultsTable)
                {
                    _resultsTable.Dispose();
                    _resultsTable = null;
                }

                if (null != _adapter)
                {
                    _adapter.Dispose();
                    _adapter = null;
                }

                if (components != null)
                {
                    components.Dispose();
                }
            }

            base.Dispose(disposing);
        }

        #endregion Overrides

        #region Event Handlers

        /// <summary>
        /// Select data file button handler.
        /// </summary>
        void HandleSelectDataFileButton(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog ofd = new OpenFileDialog();
                ofd.Title = "Select data file";
                ofd.Filter = "CSV Files (.csv)|*.csv|All files (*.*)|*.*";
                ofd.InitialDirectory = Path.GetDirectoryName(_databaseFilename);
                var result = ofd.ShowDialog();
                if (result == DialogResult.OK)
                {
                    _dataFilename = ofd.FileName;
                    SelectDataFileTextBox.Text = _dataFilename;
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI39132", ex);
            }
        }

        /// <summary>
        /// Event handler for the Import (data into the database) button
        /// </summary>
        void HandleImportDataToDatabaseClick(object sender, EventArgs e)
        {
            try
            {
                if (String.IsNullOrWhiteSpace(_dataFilename))
                {
                    // User may have typed the file name in directly.
                    _dataFilename = SelectDataFileTextBox.Text;
                }

                ExtractException.Assert("ELI39100", "Table name is empty", !String.IsNullOrEmpty(_tablename));
                ExtractException.Assert("ELI39101", "Data file name is empty", !String.IsNullOrEmpty(_dataFilename));

                bool replaceData = ReplaceRadioButton.Checked;
                const bool useTransaction = true;
                ImportSettings importSettings = new ImportSettings(_tablename,
                                                                   _dataFilename,
                                                                   useTransaction,
                                                                   replaceData);
                Tuple<int, string[]> result = null;
                using (new TemporaryWaitCursor())
                {
                    result = ImportTable.ImportFromFile(importSettings, _connection);
                }

                var countOfFailedInsertRows = result.Item1;
                if (countOfFailedInsertRows == 0)
                {
                    DisplayResultMessage(replaceData, rowMessages: result.Item2);

                    ModifiedTableName = _tablename;

                    this.Close();
                    return;
                }

                // Here to show user what failed
                var messages = result.Item2;
                using (DisplayImportErrors errors = new DisplayImportErrors(messages))
                {
                    var ret = errors.ShowDialog();
                    errors.Dispose();

                    // This implements re-submit with transactional integrity turned off, 
                    // so any rows that don't cause an error will be updated into table.
                    if (DialogResult.OK == ret)
                    {
                        importSettings.UseTransaction = false;
                        using (new TemporaryWaitCursor())
                        {
                            result = ImportTable.ImportFromFile(importSettings, _connection);
                        }

                        var failedRows = result.Item1;
                        var rowMessages = result.Item2;
                        DisplayResultMessage(replaceData, rowMessages, failedRows);

                        ModifiedTableName = importSettings.ReplaceData || failedRows != rowMessages.Count() ? _tablename : "";
                        this.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI39134", ex);
            }
        }


        /// <summary>
        /// Event handler for select data file text box, so that text changes can trigger
        /// an update and show a new data set in the grid.
        /// </summary>
        void HandleSelectDataFileTextBox_TextChanged(object sender, EventArgs e)
        {
            try
            {
                _dataFilename = SelectDataFileTextBox.Text;
                if (!File.Exists(_dataFilename))
                    return;

                AutoSelectTableForMatchingFile();
                UpdateDataIntoGrid();
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI39137", ex);
            }
        }

        /// <summary>
        /// Event handler for select table combo box. When the dropdown type is
        /// dropdownlist, the selection committed event is the one to use.
        /// </summary>
        void HandleSelectTableToImportDataInto_SelectionChangeCommitted(object sender, EventArgs e)
        {
            try
            {
                _tablename = (string)SelectTableToImportDataIntoComboBox.Text;
                UpdateDataIntoGrid();
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI39133", ex);
            }
        }

        /// <summary>
        /// Both the Append and replace radio buttons need to do the same thing when
        /// the check state changes - update the data grid view.
        /// </summary>
        void HandleRadioButton_CheckChanged(object sender, EventArgs e)
        {
            try
            {
                if (sender == ReplaceRadioButton && ReplaceRadioButton.Checked)
                {
                    using (CustomizableMessageBox cmb = new CustomizableMessageBox())
                    {
                        cmb.Caption = "Warning";
                        cmb.Text = "The replace option will clear all data from the selected table before " +
                                   "importing the new data.\r\n\r\n" +
                                   "Please review any foreign key relationships with this table to assess impacts.\r\n " +
                                   "Be aware that any foreign key relationships using cascade deletes may " +
                                   "cause data in other tables to be deleted.\r\n\r\n" +
                                   "Are you sure you want to use replace instead of append?";
                        cmb.StandardIcon = MessageBoxIcon.Warning;

                        CustomizableMessageBoxButton yesButton = new CustomizableMessageBoxButton();
                        yesButton.Text = "Yes";
                        yesButton.Value = "Yes";
                        yesButton.IsCancelButton = false;
                        cmb.AddButton(yesButton, defaultButton: false);

                        CustomizableMessageBoxButton noButton = new CustomizableMessageBoxButton();
                        noButton.Text = "No";
                        noButton.Value = "No";
                        noButton.IsCancelButton = true;
                        cmb.AddButton(noButton, defaultButton: true);

                        var result = cmb.Show(this);
                        if (result == "No")
                        {
                            AppendRadioButton.Checked = true;
                            return;
                        }
                    }
                }

                UpdateDataIntoGrid();
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI39169", ex);
            }
        }

        #endregion Event Handlers

        #region Private Methods

        /// <summary>
        /// Set row numbers into the column of row headers in the grid
        /// </summary>
        void SetRowNumber()
        {
            foreach (DataGridViewRow row in ResultsGridView.Rows)
            {
                row.HeaderCell.Value = String.Format(CultureInfo.InvariantCulture, "{0}", row.Index + 1);
            }

            ResultsGridView.AutoResizeRowHeadersWidth(DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders);
        }

        /// <summary>
        /// Remove any leading and trailing double quote (") characters.
        /// </summary>
        /// <param name="value">input string to remove quotes from</param>
        /// <returns>Returns string with double quote characters at start and 
        /// end removed iff quotes existed</returns>
        static string RemoveDoubleQuotes(string value)
        {
            string text = value.Trim('"');
            return text;
        }

        /// <summary>
        /// Using the input file, load the data into the data table.
        /// </summary>
        /// <param name="textRows"></param>
        void LoadImportData()
        {
            try
            {
                bool appendMode = AppendRadioButton.Checked;
                SqliteColumnCollection tci = new SqliteColumnCollection(_tablename, _connection);

                using var csvReader = new Microsoft.VisualBasic.FileIO.TextFieldParser(_dataFilename);
                csvReader.Delimiters = new[] { "," };
                while (!csvReader.EndOfData)
                {
                    string[] elements = csvReader.ReadFields();

                    DataRow dr = _resultsTable.NewRow();
                    int countOfColumns = _resultsTable.Columns.Count;
                    int index = 0;
                    foreach (var item in elements)
                    {
                        // In append mode, values in auto-increment columns are not relevant, so display 
                        // empty value in that column to give the user a clue that the auto-increment column
                        // will not be imported into.
                        if (appendMode)
                        {
                            if (tci[index].IsAutoIncrement)
                            {
                                ++index;
                                if (index >= countOfColumns)
                                {
                                    break;
                                }

                                continue;
                            }
                        }

                        dr[index] = RemoveDoubleQuotes(item);

                        ++index;
                        if (index >= countOfColumns)
                        {
                            break;
                        }
                    }

                    _resultsTable.Rows.Add(dr);
                }

                ColumnNotImported(makeVisible: appendMode);
            }
            catch (Exception ex)
            {
                // There may be a column type mismatch or more columns in the 
                // import data than exist in the table. In any case, report the 
                // error into the grid and continue.
                if (_resultsTable != null)
                {
                    _resultsTable.Dispose();
                }

                _resultsTable = new DataTable();
                _resultsTable.Locale = CultureInfo.CurrentCulture;

                _resultsTable.Columns.Add("Error", typeof(string));
                _resultsTable.Rows.Add(ex.Message);
            }
        }

        /// <summary>
        /// Applies selected data file and table name, fetching data and performing update in grid view.
        /// </summary>
        void UpdateDataIntoGrid()
        {
            using (new TemporaryWaitCursor())
            {
                if (String.IsNullOrWhiteSpace(_dataFilename))
                    return;
                if (String.IsNullOrWhiteSpace(_tablename))
                    return;

                // Now populate the data grid. First clear any existing data from the data table and grid view.
                ResultsGridView.DataSource = null;

                _resultsTable = new DataTable();
                _resultsTable.Locale = CultureInfo.CurrentCulture;

                // Load data from the DB table, so that the schema is set. Clearing retains columns, not data.
                LoadOneRowIntoTable();
                _resultsTable.Clear();

                LoadImportData();

                ResultsGridView.DataSource = _resultsTable;
                SetRowNumber();
            }
        }


        /// <summary>
        /// Add the database table names to the combo box.
        /// </summary>
        /// <param name="tableNames"></param>
        void PopulateDatabaseTableNames(string[] tableNames)
        {
            try
            {
                ExtractException.Assert("ELI39104",
                                        "There are no table names in the current database",
                                        tableNames.Length > 0);
                foreach (var tableName in tableNames)
                {
                    SelectTableToImportDataIntoComboBox.Items.Add(tableName);
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI39093", ex);
            }
        }

        /// <summary>
        /// Loads the table from database, so that the table structure is present.
        /// </summary>
        void LoadOneRowIntoTable()
        {
            if (_adapter != null)
            {
                _adapter.Dispose();
                _adapter = null;
            }

            DbProviderFactory providerFactory = DBMethods.GetDBProvider(_connection);
            _adapter = providerFactory.CreateDataAdapter();
            _adapter.SelectCommand = DBMethods.CreateDBCommand(_connection, 
                UtilityMethods.FormatInvariant($"SELECT * FROM [{_tablename}] LIMIT 1"), null);
            _adapter.Fill(_resultsTable);
        }

        /// <summary>
        /// Notify the user that an auto-increment column will not be imported (or
        /// clear the message depending on the makeVisible flag).
        /// </summary>
        /// <param name="makeVisible">true to display the text box and message, false otherwise</param>
        void ColumnNotImported(bool makeVisible)
        {
            ColumnNotImportedTextBox.Visible = makeVisible;
            if (makeVisible)
            {
                SqliteColumnCollection tci = new SqliteColumnCollection(_tablename, _connection);

                int countOfAutoIncrementColumns = tci.Count(c => c.IsAutoIncrement);
                if (countOfAutoIncrementColumns < 1)
                {
                    return;
                }

                var name = tci.Where(column => column.IsAutoIncrement == true)
                              .Select(column => column.ColumnName)
                              .Aggregate((current, next) => current + ", " + next);

                ColumnNotImportedTextBox.Text = String.Format(CultureInfo.CurrentCulture,
                                                              "The column: {0}, will not be imported because it is an auto-increment column",
                                                              name);
                ColumnNotImportedTextBox.ForeColor = Color.Red;
            }
        }

        /// <summary>
        /// automatically select a table based on the selected file name - convenience for user
        /// </summary>
        void AutoSelectTableForMatchingFile()
        {
            if (String.IsNullOrWhiteSpace(_dataFilename))
            {
                return;
            }

            var name = Path.GetFileNameWithoutExtension(_dataFilename);
            if (String.IsNullOrWhiteSpace(name))
            {
                return;
            }

            int index = SelectTableToImportDataIntoComboBox.FindStringExact(name);
            if (index < 0)
            {
                return;
            }

            SelectTableToImportDataIntoComboBox.SelectedIndex = index;
            _tablename = (string)SelectTableToImportDataIntoComboBox.Text;
        }

        /// <summary>
        /// display the result of the import append|replace operation
        /// </summary>
        /// <param name="replaceData">true to replace, false to append</param>
        /// <param name="rowMessages">string[] of row insert operation outcomes</param>
        /// <param name="countOfFailedRows">number of rows that failed to be imported</param>
        void DisplayResultMessage(bool replaceData, string[] rowMessages, int countOfFailedRows = 0)
        {
            string message;
            int rowsImported = rowMessages.Count() - countOfFailedRows;
            if (replaceData)
            {
                message = String.Format(CultureInfo.CurrentCulture,
                                        "The table data has been replaced with {0} rows of imported data",
                                        rowsImported);
            }
            else
            {
                message = String.Format(CultureInfo.CurrentCulture,
                                        "{0} rows of data have been appended to the table\n" +
                                        "{1} rows of data FAILED to be appended",
                                        rowsImported,
                                        countOfFailedRows);
            }

            using (CustomizableMessageBox cmb = new CustomizableMessageBox())
            {
                cmb.Caption = "Table update status";
                cmb.Text = message;
                cmb.AddStandardButtons(MessageBoxButtons.OK);
                cmb.StandardIcon = MessageBoxIcon.Information;

                cmb.Show(this);
            }
        }

        #endregion Private Methods
    }
}
