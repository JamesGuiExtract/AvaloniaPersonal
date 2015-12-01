using Extract.Database;
using Extract.Utilities.Forms;
using System;
using System.Data;
using System.Data.SqlServerCe;
using System.Diagnostics.CodeAnalysis;
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
        SqlCeDataAdapter _adapter = null;

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
        SqlCeConnection _connection;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Default CTOR
        /// </summary>
        public ImportTableForm(string databaseFileName, string[] tableNames, SqlCeConnection connection)
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
        [SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions")]
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
                ImportSettings importSettings = new ImportSettings(_databaseFilename,
                                                                   _tablename,
                                                                   _dataFilename,
                                                                   useTransaction,
                                                                   replaceData);
                importSettings.ColumnDelimiter = ",";

                Tuple<int, string[]> result = null;
                using (new TemporaryWaitCursor())
                {
                    result = ImportTable.ImportFromFile(importSettings, _connection);
                }

                var countOfFailedInsertRows = result.Item1;
                if (countOfFailedInsertRows == 0)
                {
                    int rowsImported = result.Item2.Count();
                    string message;
                    if (replaceData)
                    {
                        message = String.Format(CultureInfo.CurrentCulture,
                                                "The table data has been replaced with {0} rows of imported data",
                                                rowsImported);
                    }
                    else
                    {
                        message = String.Format(CultureInfo.CurrentCulture,
                                                "{0} rows of data have been appended to the table",
                                                rowsImported);
                    }

                    MessageBox.Show(text: message, caption: "Table update status");
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
                        ImportTable.ImportFromFile(importSettings, _connection);

                        ModifiedTableName = _tablename;
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
        /// Using the specified string array (from input file), load the data into the data table.
        /// </summary>
        /// <param name="textRows"></param>
        void LoadImportData(string[] textRows)
        {
            try
            {
                bool appendMode = AppendRadioButton.Checked;
                SqlCeTableColumnInfo tci = new SqlCeTableColumnInfo(_tablename, _connection);
                foreach (var line in textRows)
                {
                    if (String.IsNullOrWhiteSpace(line))
                        continue;

                    string[] elements = ImportTable.MakeColumns(line,
                                                                delimiter: ",",
                                                                useAdvancedSplitter: true);
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
                                continue;
                            }
                        }

                        dr[index] = RemoveDoubleQuotes(item);
                        ++index;

                        if (index >= countOfColumns)
                            break;
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

            LoadImportData(ReadFile());

            ResultsGridView.DataSource = _resultsTable;
            SetRowNumber();
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
                                        "There are no table names in the curent database",
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
        /// Read in the selected text file, and return an array of text 
        /// corresponding to lines of text in the file.
        /// </summary>
        /// <returns>array of strings corresponding to lines of text in the file</returns>
        string[] ReadFile()
        {
            ExtractException.Assert("ELI39136", "An import filename must be specified", !String.IsNullOrWhiteSpace(_dataFilename));

            try
            {
                using (StreamReader sr = new StreamReader(_dataFilename))
                {
                    var text = sr.ReadToEnd();
                    sr.Close();

                    string[] textRows = text.Split(new String[] {"\r\n"}, StringSplitOptions.RemoveEmptyEntries);
                    return textRows;
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI39121", ex);
                return new string[0];
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

            _adapter = new SqlCeDataAdapter("SELECT TOP (1) * FROM " + _tablename, _connection);
            _adapter.Fill(_resultsTable);
        }

        /// <summary>
        /// Notify the user that an auto-increment columnm will not be imported (or
        /// clear the message depending on the makeVisible flag).
        /// </summary>
        /// <param name="makeVisible">true to display the text box and message, false otherwise</param>
        void ColumnNotImported(bool makeVisible)
        {
            ColumnNotImportedTextBox.Visible = makeVisible;
            if (makeVisible)
            {
                SqlCeTableColumnInfo tci = new SqlCeTableColumnInfo(_tablename, _connection);

                int countOfAutoIncrementColumns = tci.Count(c => c.IsAutoIncrement);
                if (countOfAutoIncrementColumns < 1)
                {
                    return;
                }

                var name = tci.Where(column => column.IsAutoIncrement == true)
                              .Select(column => column.ColumnName)
                              .Aggregate((current, next) => current + ", " + next);

                ColumnNotImportedTextBox.Text = String.Format(CultureInfo.CurrentCulture,
                                                              "The column: {0}, will not be exported because it is an auto-increment column",
                                                              name);
                ColumnNotImportedTextBox.ForeColor = Color.Red;
            }
        }

        #endregion Private Methods
    }
}
