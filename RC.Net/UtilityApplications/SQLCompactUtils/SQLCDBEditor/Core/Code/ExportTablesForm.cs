using Extract.Database.Sqlite;
using Extract.Utilities.Forms;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Windows.Forms;

namespace Extract.SQLCDBEditor
{
    /// <summary>
    /// This form is used to select one or more tables to export.
    /// </summary>
    public partial class ExportTablesForm : Form
    {
        #region Fields

        /// <summary>
        /// The directory to write output files too.
        /// </summary>
        string _selectedOutputDirectory;

        /// <summary>
        /// The database filename passed from the parent form.
        /// </summary>
        string _databaseFilename;

        /// <summary>
        /// The active, open connection passed from the parent form.
        /// </summary>
        DbConnection _connection;

        #endregion Fields

        #region Constants

        /// <summary>
        /// Column index of the selectors - a column of checkbox controls embedded
        /// in the zeroth column of the grid.
        /// </summary>
        const int _selectColumnIndex = 0;

        /// <summary>
        /// The column index of the "table name" column.
        /// </summary>
        const int _tableNameIndex = 1;

        /// <summary>
        /// The column index of the "file name" column
        /// </summary>
        const int _filenameColumnIndex = 2;

        #endregion Constants

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="databaseFileName">The database file in use</param>
        /// <param name="tableNames">a list of table names, used to populate the grid view</param>
        /// <param name="connection">the current open connection that is in use</param>
        public ExportTablesForm(string databaseFileName, string[] tableNames, DbConnection connection)
        {
            InitializeComponent();

            _databaseFilename = databaseFileName;
            _connection = connection;

            PopulateGrid(tableNames);
            AddSelectAllCheckBox();
        }

        #endregion Constructors

        #region Event Handlers

        /// <summary>
        /// Apply state change of all check boxes in the grid view.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void HandleSelectAllCheckBox_Click(object sender, EventArgs e)
        {
            try
            {
                // This is done to work around an issue when the current selected cell 
                // is in the zeroth (select) column - the checkbox isn't updated.
                var cell = TablesGridView.CurrentCell;
                var startRow = cell.RowIndex;
                var startCol = cell.ColumnIndex;
                MoveGridViewSelection(0, 1);

                CheckBox cb = (CheckBox)sender;
                bool isChecked = cb.Checked;
                SetAllGridViewCheckBoxes(isChecked);

                MoveGridViewSelection(startRow, startCol);
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI39106", ex);
            }
        }

        /// <summary>
        /// Handles exporting tables, including checking to ensure that preconditions are valid
        /// </summary>
        void HandleExportTables_Click(object sender, EventArgs e)
        {
            try
            {
                // Need at least one selection, and the selected output directory must be set.
                ExtractException.Assert("ELI39111",
                                        "Selected output directory must be set",
                                        !String.IsNullOrWhiteSpace(SelectedOutputDirectoryTextBox.Text));

                ExtractException.Assert("ELI39112",
                                        "At least one table must be selected",
                                        TableIsSelected());

                List<string> results = new List<string>();

                foreach (DataGridViewRow row in TablesGridView.Rows)
                {
                    if (RowIsSelected(row))
                    {
                        var filename = GetFileName(row);
                        var outputFilename = MakeOutputFileName(filename);
                        if (File.Exists(outputFilename))
                        {
                            var msg = String.Format(CultureInfo.CurrentCulture,
                                                    "The file: {0}, already exists. Are you sure you want to overwrite this file?",
                                                    outputFilename);

                            using (CustomizableMessageBox cmb = new CustomizableMessageBox())
                            {
                                cmb.Caption = "File exists";
                                cmb.Text = msg;

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

                                cmb.StandardIcon = MessageBoxIcon.Warning;

                                var response = cmb.Show(this);
                                if (response == "No")
                                {
                                    continue;
                                }
                            }
                        }

                        var tableName = GetTableName(row);
                        var query = "SELECT * FROM [" + tableName + ']';
                        const bool addQuotes = true;
                        var exportSettings = new ExportSettings(_databaseFilename, query, outputFilename, tableName, addQuotes);
                        exportSettings.ColumnDelimiter = ",";

                        var result = ExportTable.ExportToFile(exportSettings, _connection, true);

                        var rowsWritten = result.Split(new Char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
                        var numberOfRows = rowsWritten.Length;
                        string notify = (0 == numberOfRows) ? "* " : " ";
                        results.Add(notify + filename + " (" + numberOfRows.ToString("N0", CultureInfo.CurrentCulture) + " rows exported)\r\n");
                    }
                }

                if (results.Count > 0)
                {
                    var resultsForm = new ExportResultsForm(results.ToArray());
                    resultsForm.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI39110", ex);
            }
        }


        /// <summary>
        /// Button click handler to select an output directory 
        /// </summary>
        void HandleSelectDirectoryButton_Click(object sender, EventArgs e)
        {
            try
            {
                FolderBrowserDialog fbd = new FolderBrowserDialog();
                fbd.SelectedPath = Path.GetDirectoryName(_databaseFilename);
                var ret = fbd.ShowDialog();
                if (ret != DialogResult.OK)
                    return;

                _selectedOutputDirectory = fbd.SelectedPath;
                SelectedOutputDirectoryTextBox.Text = _selectedOutputDirectory;
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI39105", ex);
            }
        }

        #endregion Event Handlers

        #region Private Methods

        /// <summary>
        /// Adds all the data in the grid; three columns: checkbox (not set), table name, and file name
        /// </summary>
        /// <param name="tableNames"></param>
        void PopulateGrid(string[] tableNames)
        {
            foreach (var name in tableNames)
            {
                var filename = name + ".csv";
                TablesGridView.Rows.Add(false, name, filename);
            }

        }

        /// <summary>
        /// Takes a value to apply to all check boxes, and set all of them accordingly
        /// </summary>
        void SetAllGridViewCheckBoxes(bool value)
        {
            foreach (DataGridViewRow row in TablesGridView.Rows)
            {
                row.Cells[_selectColumnIndex].Value = value;    
            }
        }

        /// <summary>
        /// Add a custom checkbox element into the header column.
        /// </summary>
        void AddSelectAllCheckBox()
        {
            CheckBox cb = new CheckBox();
            cb.Name = "SelectAll";
            cb.Size = new Size(14, 14);
            cb.Text = "Select";

            Rectangle rect = new Rectangle(0, 0, 50, 21);

            cb.Location = new Point((rect.Location.X + ((rect.Width - cb.Width) / 2)) + 1,
                                    (rect.Location.Y + ((rect.Height - cb.Height) / 2)) + 2);
            cb.BackColor = Color.Transparent;
            TablesGridView.Controls.Add(cb);

            // Handle header checkbox clicked event
            cb.Click += new EventHandler(HandleSelectAllCheckBox_Click);
        }

        /// <summary>
        /// Moves the gridview current selection.
        /// </summary>
        void MoveGridViewSelection(int row, int column)
        {
            if (TablesGridView.Rows.Count > row && TablesGridView.ColumnCount > column)
            {
                TablesGridView.CurrentCell = TablesGridView.Rows[row].Cells[column];
                TablesGridView.CurrentCell.Selected = true;
            }
        }


        /// <summary>
        /// Determines if a table is selected.
        /// </summary>
        /// <returns>return true if at one table is selected</returns>
        bool TableIsSelected()
        {
            foreach (DataGridViewRow row in TablesGridView.Rows)
            {
                if (true == (bool)row.Cells[_selectColumnIndex].Value)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Determines if the row is selected
        /// </summary>
        /// <param name="row">The row to check</param>
        /// <returns>true or false</returns>
        static bool RowIsSelected(DataGridViewRow row)
        {
            return (bool)row.Cells[_selectColumnIndex].Value;
        }

        /// <summary>
        /// Gets the file name from the specified row
        /// </summary>
        /// <param name="row">The row to operate on</param>
        /// <returns>file name of the row</returns>
        static string GetFileName(DataGridViewRow row)
        {
            return row.Cells[_filenameColumnIndex].Value.ToString();
        }

        /// <summary>
        /// Gets the table name from the specified row
        /// </summary>
        /// <param name="row">the row to operate on</param>
        /// <returns>table name of the row</returns>
        static string GetTableName(DataGridViewRow row)
        {
            return row.Cells[_tableNameIndex].Value.ToString();
        }

        /// <summary>
        /// Glues the output directory path onto the filename
        /// </summary>
        /// <returns>path + filename</returns>
        string MakeOutputFileName(string filename)
        {
            var path = SelectedOutputDirectoryTextBox.Text;
            if (!path.EndsWith("\\", StringComparison.OrdinalIgnoreCase))
            {
                path += '\\';
            }

            return path + filename;
        }

        #endregion Private Methods
    }
}
