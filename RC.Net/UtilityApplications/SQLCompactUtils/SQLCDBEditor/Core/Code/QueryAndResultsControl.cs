﻿using Extract.Utilities;
using Extract.Utilities.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Data.SqlServerCe;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using TD.SandDock;

namespace Extract.SQLCDBEditor
{
    /// <summary>
    /// A control which allow for the viewing/editing of a database table or query as well as
    /// modification of a query.
    /// </summary>
    internal partial class QueryAndResultsControl : UserControl, INotifyPropertyChanged
    {
        #region Fields

        /// <summary>
        /// The connection to the database.
        /// </summary>
        SqlCeConnection _connection;

        /// <summary>
        /// The data table representing the table contents or query results.
        /// </summary>
        DataTable _resultsTable = new DataTable();

        /// <summary>
        /// The data adapter to populate <see cref="_resultsTable"/> for database tables.
        /// </summary>
        SqlCeDataAdapter _adapter;

        /// <summary>
        /// The <see cref="SqlCeCommandBuilder"/> used to facilitate database table editing.
        /// </summary>
        SqlCeCommandBuilder _commandBuilder;

        /// <summary>
        /// The query to evaluate. This does not contain any sub-queries for defining query
        /// parameters.
        /// </summary>
        string _masterQuery;

        /// <summary>
        /// The last query text saved or loaded from disk. Used to deterimine if the query is
        /// dirty.
        /// </summary>
        string _lastSavedQuery;

        /// <summary>
        /// <see cref="Control"/>s for specification of query parameters.
        /// </summary>
        List<Control> _queryParameterControls = new List<Control>();

        /// <summary>
        /// A list of the query parameter names and values last used so that parameters do not need
        /// to be re-entered on execution if the parameter definition hasn't changed.
        /// </summary>
        List<KeyValuePair<string, string>> _lastUsedParameters
            = new List<KeyValuePair<string, string>>();

        /// <summary>
        /// Indicates whether the query itself has changed since the last execution.
        /// </summary>
        bool _queryChanged;

        /// <summary>
        /// Indicates whether the result of query has changed due to data changing in the database
        /// since the last execution.
        /// </summary>
        bool _resultsChanged;

        /// <summary>
        /// Indicates the first row in a table containing invalid data, or -1 if there is no invalid
        /// data.
        /// </summary>
        List<DataGridViewRow> _invalidRows = new List<DataGridViewRow>();
        
        /// <summary>
        /// Indicates whether the query text is dirty.
        /// </summary>
        bool _isQueryDirty;

        /// <summary>
        /// Indicates if the host is in design mode or not.
        /// </summary>
        readonly bool _inDesignMode;

        #endregion Fields

        #region Constructors

        /// <overloads>
        /// Initializes a new instance of the <see cref="QueryAndResultsControl"/> class.
        /// </overloads>
        /// <summary>
        /// Initializes a new instance of the <see cref="QueryAndResultsControl"/> class.
        /// </summary>
        /// <param name="isTable"><see langword="true"/> if the control is to represent a database
        /// table, <see langword="false"/> if it is to respresent a query.</param>
        public QueryAndResultsControl(bool isTable)
        {
            try
            {
                _inDesignMode = (LicenseManager.UsageMode == LicenseUsageMode.Designtime);
               
                InitializeComponent();

                _resultsTable.Locale = CultureInfo.CurrentCulture;
                IsTable = isTable;
                if (!isTable)
                {
                    Name = "New Query";
                }
                _invalidRows.Clear();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI34573");
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryAndResultsControl"/> class.
        /// </summary>
        /// <param name="name">The name of the table or query.</param>
        /// <param name="fileName">Name of the database file if this control represents a
        /// database table or the query text otherwise.</param> 
        /// <param name="isTable"><see langword="true"/> if the control is to represent a database
        /// table, <see langword="false"/> if it is to respresent a query.</param>
        public QueryAndResultsControl(string name, string fileName, bool isTable)
            : base()
        {
            try
            {
                _inDesignMode = LicenseManager.UsageMode == LicenseUsageMode.Designtime;
  
                InitializeComponent();

                _resultsTable.Locale = CultureInfo.CurrentCulture;
                Name = name;
                FileName = fileName;
                IsTable = isTable;
                _invalidRows.Clear();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI34574");
            }
        }

        #endregion Constructors

        #region Events

        /// <summary>
        /// Raised when the user has modified data.
        /// </summary>
        public event EventHandler<EventArgs> DataChanged;

        /// <summary>
        /// Raised to indicate that the control should be opened in a separate tab.
        /// </summary>
        public event EventHandler<EventArgs> SentToSeparateTab;

        /// <summary>
        /// Raised to indicate a property value has changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raised to indicate a new <see cref="QueryAndResultsControl"/> instance has been created.
        /// </summary>
        public event EventHandler<QueryCreatedEventArgs> QueryCreated;

        /// <summary>
        /// Raised to indicate the query is being renamed.
        /// </summary>
        public event EventHandler<QueryRenamingEventArgs> QueryRenaming;

        /// <summary>
        /// Raised to indicate a query has been saved.
        /// </summary>
        public event EventHandler<EventArgs> QuerySaved;

        #endregion Events

        #region Properties

        /// <summary>
        /// The name of the table or query, suffixed by an asterix if a query is dirty.
        /// </summary>
        /// <returns></returns>
        public string DisplayName
        {
            get
            {
                if (IsQueryDirty)
                {
                    return Name + "*";
                }
                else
                {
                    return Name;
                }
            }
        }

        /// <summary>
        /// Gets or sets the name of the database file if this control represents a database table
        /// or the query text otherwise. 
        /// </summary>
        /// <value>
        /// The name of the database or query file.
        /// </value>
        public string FileName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a value indicating whether this control represents a database table.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if this instance represents a database table;
        /// <see langword="false"/> if it represents a database query.
        /// </value>
        public bool IsTable
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a value indicating whether this instance is read only.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if this instance is read only; otherwise, <see langword="false"/>.
        /// </value>
        public bool IsReadOnly
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a value indicating whether this instance is loaded.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if this instance is loaded; otherwise, <see langword="false"/>.
        /// </value>
        public bool IsLoaded
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a value indicating whether database table data is valid.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if this instance represents a database table whose data is
        /// invalid; otherwise, <see langword="false"/>.
        /// </value>
        public bool DataIsValid
        {
            get
            {
                try
                {
                    // If the grid is currently being edited, apply the active edit to force the
                    // data to be validated.
                    if (_resultsGrid.IsCurrentCellInEditMode)
                    {
                        _resultsGrid.EndEdit();
                    }

                    return (_invalidRows.Count == 0);
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI34634");
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the send to separate tab button should be shown.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if send to separate tab button should be shown; otherwise, 
        /// <see langword="false"/>.
        /// </value>
        public bool ShowSendToSeparateTabButton
        {
            get
            {
                return _sendToSeparateTabButton.Visible;
            }

            set
            {
                _sendToSeparateTabButton.Visible = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance represents a query and that query
        /// is dirty.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the query dirty; otherwise, <see langword="false"/>.
        /// </value>
        public bool IsQueryDirty
        {
            get
            {
                return _isQueryDirty;
            }

            set
            {
                try
                {
                    if (value != _isQueryDirty)
                    {
                        _isQueryDirty = value;

                        OnPropertyChanged("IsDirty");
                        OnPropertyChanged("DisplayName");
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI34576");
                }
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Loads the control data from a database table and populates the results grid.
        /// </summary>
        /// <param name="connection">The connection to the database.</param>
        /// <param name="tableName">Name of the database table.</param>
        public void LoadTable(SqlCeConnection connection, string tableName)
        {
            try
            {
                ExtractException.Assert("ELI34577", "Cannot load a query as a database table",
                    IsTable);

                _connection = connection;

                _showHideQueryButton.Visible = false;
                _renameButton.Visible = false;
                _saveButton.Visible = false;
                _executeQueryButton.Visible = false;

                LoadTableFromDatabase(connection, tableName);

                _resultsGrid.DataSource = _resultsTable;

                IsLoaded = true;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI34578");
            }
        }

        /// <overloads>
        /// Loads the control data using a query and populates the results grid.
        /// </overloads>
        /// <summary>
        /// Loads the control data using a query and populates the results grid.
        /// </summary>
        /// <param name="connection">The connection to the database.</param>
        public void LoadQuery(SqlCeConnection connection)
        {
            try
            {
                string query = File.ReadAllText(FileName);

                LoadQuery(connection, query);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI34579");
            }
        }

        /// <summary>
        /// Loads the control data using a query and populates the results grid.
        /// </summary>
        /// <param name="connection">The connection to the database.</param>
        /// <param name="query">The query.</param>
        public void LoadQuery(SqlCeConnection connection, string query)
        {
            try
            {
                ExtractException.Assert("ELI34580", "", !IsTable);

                _connection = connection;
                _lastUsedParameters.Clear();

                // If the query doesn't yet exist on disk, immediately treat it as dirty.
                if (!File.Exists(FileName))
                {
                    IsQueryDirty = true;
                    _saveButton.Enabled = true;
                    QueryPanelOpen = true;
                }

                // If the file on disk is read-only, prevent the query from being edited in this
                // control.
                if (File.Exists(FileName) && new FileInfo(FileName).IsReadOnly)
                {
                    IsReadOnly = true;
                    _renameButton.Visible = false;
                    _saveButton.Visible = false;
                }

                _queryScintillaBox.Text = query;
                _queryScintillaBox.IsReadOnly = IsReadOnly;
                _lastSavedQuery = query;
                _resultsGrid.AllowUserToAddRows = false;

                // Parse the query in order to define any parameters used by the query.
                ParseQuery(connection, query);

                // Populate the results grid.
                RefreshData(true);

                _queryScintillaBox.TextChanged += HandleQueryScintillaBoxTextChanged;

                _resultsGrid.DataSource = _resultsTable;

                IsLoaded = true;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI34581");
            }
        }

        /// <summary>
        /// Refreshes the data.
        /// </summary>
        /// <param name="updateQueryResult"><see langword="true"/> to update the results or a query
        /// <see langword="false"/> to just make the visible results as out-of-date.</param>
        public void RefreshData(bool updateQueryResult)
        {
            DataTable latestDataTable = null;

            try
            {
                // If the results table has not been created or it currently contains invalid data,
                // prevent a refresh.
                if (_resultsTable == null || !DataIsValid)
                {
                    return;
                }

                // Load the latest data into a latestDataTable so that we can check to see if the
                // data already in _resultsTable is out of date.
                latestDataTable = new DataTable();
                latestDataTable.Locale = CultureInfo.CurrentCulture;

                if (IsTable)
                {
                    _adapter.Fill(latestDataTable);
                }
                else
                {
                    // Don't bother running the query at all if it is blank.
                    if (!string.IsNullOrWhiteSpace(_masterQuery))
                    {
                        // Obtain any query parameters from the _queryParameterControls.
                        Dictionary<string, string> parameters = new Dictionary<string, string>();
                        for (int i = 0; i < _queryParameterControls.Count; i++)
                        {
                            string parameterKey = "@" + i.ToString(CultureInfo.InvariantCulture);
                            string parameterValue = _queryParameterControls[i].Text;

                            parameters[parameterKey] = parameterValue;
                            _lastUsedParameters[i] = new KeyValuePair<string, string>(
                                _lastUsedParameters[i].Key, parameterValue);
                        }

                        // Populate latestDataTable with the query results.
                        using (SqlCeCommand command = (SqlCeCommand)DBMethods.CreateDBCommand(
                            _connection, _masterQuery, parameters))
                        using (SqlCeDataAdapter adapter = new SqlCeDataAdapter(command))
                        {
                            adapter.Fill(latestDataTable);
                        }
                    }
                }

                // Check to see if the data in _resultsTable differs from the latestDataTable
                bool dataChanged = false;
                if (latestDataTable.Rows.Count != _resultsTable.Rows.Count)
                {
                    dataChanged = true;
                }
                else
                {
                    for (int i = 0; i < _resultsTable.Rows.Count; i++)
                    {
                        for (int j = 0; j < _resultsTable.Columns.Count; j++)
                        {
                            if (latestDataTable.Rows[i][j].ToString() != _resultsTable.Rows[i][j].ToString())
                            {
                                dataChanged = true;
                                break;
                            }
                        }
                    }
                }

                // If the data hasn't changed, there is nothing more to do.
                if (!dataChanged)
                {
                    return;
                }

                if (!IsTable && !updateQueryResult)
                {
                    // If a query and the query results shouldn't be automatically updated, mark
                    // them as out-of-date.
                    _resultsChanged = true;
                    UpdateResultsStatus(false);
                }
                else
                {
                    // If updating the results, keep track of the last scroll position so that we
                    // can keep the same data visiable (as much as is possible).
                    int scrollPos = _resultsGrid.FirstDisplayedScrollingRowIndex;

                    // Keep track of existing column widths so that the refresh doesn't re-size
                    // columns for database tables.
                    int[] columnWidths = _resultsGrid.Columns
                        .OfType<DataGridViewColumn>()
                        .Select(column => column.Width)
                        .ToArray();

                    // While refreshing, don't handle data changed events so that the changes are
                    // not interpreted as edits by the user.
                    if (IsTable)
                    {
                        _resultsTable.ColumnChanged -= HandleColumnChanged;
                        _resultsTable.RowChanged -= HandleRowChanged;
                        _resultsTable.RowDeleted -= HandleRowChanged;
                        _resultsTable.TableNewRow -= HandleTableNewRow;
                    }

                    // Apply the latest data to the grid.
                    _resultsGrid.DataSource = null;
                    _resultsTable.Dispose();
                    _resultsTable = latestDataTable;
                    latestDataTable = null;
                    _resultsGrid.DataSource = _resultsTable;

                    if (IsTable)
                    {
                        // Re-register to get data changed events.
                        _resultsTable.ColumnChanged += HandleColumnChanged;
                        _resultsTable.RowChanged += HandleRowChanged;
                        _resultsTable.RowDeleted += HandleRowChanged;
                        _resultsTable.TableNewRow += HandleTableNewRow;

                        
                    }

                    // If this is a table or a query has been re-executed without changing, restore
                    // the previous column sizes that had
                    if ((IsTable || !_queryChanged) &&
                        columnWidths.Length == _resultsGrid.Columns.Count)
                    {
                        // Re-apply the previous column widths.
                        foreach (DataGridViewColumn column in _resultsGrid.Columns)
                        {
                            column.Width = columnWidths[column.Index];
                        }
                    }
                    else
                    {
                        // Always auto-size columns if a query has changed since the last execution
                        // because the columns may no longer be the same.
                        AutoSizeColumns();
                    }

                    // Re-apply the previous vertical scroll position for tables to try to make it
                    // appear that the table was updated in-place.
                    if (IsTable)
                    {
                        if (scrollPos >= 0 && scrollPos < _resultsGrid.RowCount)
                        {
                            _resultsGrid.FirstDisplayedScrollingRowIndex = scrollPos;
                        }
                    }

                    _resultsGrid.Refresh();
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI34546");
            }
            finally
            {
                if (latestDataTable != null)
                {
                    latestDataTable.Dispose();
                }
            }
        }

        /// <summary>
        /// Saves the current text from the query edit box to the associated sqlce file.
        /// </summary>
        public void SaveQuery()
        {
            try
            {
                File.WriteAllText(FileName, _queryScintillaBox.Text);
                IsQueryDirty = false;
                _saveButton.Enabled = false;

                OnQuerySaved();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI34635");
            }
        }

        /// <summary>
        /// Renames a query.
        /// </summary>
        public void Rename()
        {
            try
            {
                ExtractException.Assert("ELI34583", "", !IsTable && !IsReadOnly);

                string originalName = Name;
                string newName = Name;

                while (InputBox.Show(this, "New query name", "Rename query", ref newName)
                    == DialogResult.OK)
                {
                    try
                    {
                        // Validate that the new query name is valid and unique.
                        ExtractException.Assert("ELI34584", "Invalid characters in query name",
                            FileSystemMethods.IsFileNameValid(newName));
                        string newFileName = Path.Combine(Path.GetDirectoryName(FileName),
                            newName + ".sqlce");
                        ExtractException.Assert("ELI34585", "No query name specified.",
                            !string.IsNullOrWhiteSpace(newFileName));
                        
                        var eventArgs = new QueryRenamingEventArgs(newName);
                        OnQueryRenaming(eventArgs);

                        // The rename operation has been canceled (name already exists).
                        ExtractException.Assert("ELI34586", eventArgs.CancelReason, !eventArgs.Cancel);

                        if (File.Exists(FileName))
                        {
                            FileSystemMethods.MoveFile(FileName, newFileName, false);
                        }

                        Name = newName;

                        OnPropertyChanged("DisplayName");

                        FileName = newFileName;

                        // If the rename was successful, return.
                        break;
                    }
                    catch (Exception ex)
                    {
                        // Restore the original name if the rename did not succeed.
                        Name = originalName;

                        // Display instead of throwing so that the rename prompt is re-displayed
                        // if the rename was not successful.
                        ex.ExtractDisplay("ELI34575");
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI34582");
            }
        }

        /// <summary>
        /// Creates a copy of this control. If this instance represents a database table, the copy
        /// with be a query selecting all data from the table.
        /// </summary>
        public void CreateQueryCopy()
        {
            try
            {
                var queryAndResultsControl = new QueryAndResultsControl(false);

                if (IsTable)
                {
                    // Create a query selecting all columns explicity.
                    string query = "SELECT ";

                    query += string.Join("\r\n\t,", _resultsTable.Columns
                        .OfType<DataColumn>()
                        .Select(column => "[" + column.ColumnName + "]"));
                    query += "\r\n\tFROM [" + Name + "]";

                    queryAndResultsControl.LoadQuery(_connection, query);
                }
                else
                {
                    queryAndResultsControl.LoadQuery(_connection, _queryScintillaBox.Text);
                }

                queryAndResultsControl.Name = Name;

                OnQueryCreated(queryAndResultsControl);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI34587");
            }
        }

        /// <summary>
        /// If there is any invalid data in the table, selects and displays that data.
        /// </summary>
        public void ShowInvalidData()
        {
            try
            {
                if (_invalidRows.Count > 0)
                {
                    _resultsGrid.ClearSelection();

                    DataGridViewRow firstInvalidRow = _invalidRows
                        .First(row => row != null && _resultsGrid.Rows.Contains(row));

                    firstInvalidRow.Selected = true;
                    int firstRowIndex = _resultsGrid.FirstDisplayedScrollingRowIndex;
                    int lastRowIndex = firstRowIndex + _resultsGrid.DisplayedRowCount(false) - 1;
                    if (firstInvalidRow.Index < firstRowIndex || firstInvalidRow.Index > lastRowIndex)
                    {
                        _resultsGrid.FirstDisplayedScrollingRowIndex = firstInvalidRow.Index;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI34588");
            }
        }

        #endregion Methods

        #region Overrides

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.UserControl.Load"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.
        /// </param>
        protected override void OnLoad(EventArgs e)
        {
            try
            {
                base.OnLoad(e);

                if (!_inDesignMode)
                {
                    Dock = DockStyle.Fill;
                    Size = Parent.ClientSize;
                    _queryScintillaBox.ConfigurationManager.Configure();
                    _queryScintillaBox.Lexing.Colorize();
                }

                // Check for unique constraints
                bool hasUniqueContraint = false;
                foreach (Constraint c in _resultsTable.Constraints)
                {
                    // Check if current constraint is unique					
                    if (c is UniqueConstraint)
                    {
                        // set Flag to true;
                        hasUniqueContraint = true;
                        break;
                    }
                }

                // Set the ReadOnly and AllowUserToAddRows so that if no unique constraint
                // the grid cannot be modified and rows cannot be added
                _resultsGrid.ReadOnly = !hasUniqueContraint;
                _resultsGrid.AllowUserToAddRows = hasUniqueContraint;

                AutoSizeColumns();

                _resultsTable.ColumnChanged += HandleColumnChanged;
                _resultsTable.RowChanged += HandleRowChanged;
                _resultsTable.RowDeleted += HandleRowChanged;
                _resultsTable.TableNewRow += HandleTableNewRow;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI34589");
            }
        }

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_adapter != null)
                {
                    _adapter.Dispose();
                    _adapter = null;
                }

                if (components != null)
                {
                    components.Dispose();
                    components = null;
                }

                if (_resultsTable != null)
                {
                    _resultsTable.Dispose();
                    _resultsTable = null;
                }

                if (_commandBuilder != null)
                {
                    _commandBuilder.Dispose();
                    _commandBuilder = null;
                }
            }
            base.Dispose(disposing);
        }

        #endregion Overrides

        #region Event Handlers

        /// <summary>
        /// Handles the execute button click.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleExecuteButtonClick(object sender, EventArgs e)
        {
            try
            {
                _queryAndResultsTableLayoutPanel.SuspendLayout();
                
                using (new LockControlUpdates(_queryAndResultsTableLayoutPanel))
                {
                    ParseQuery(_connection, _queryScintillaBox.Text);
                }

                _queryAndResultsTableLayoutPanel.ResumeLayout(true);

                RefreshData(true);

                _queryChanged = false;
                _resultsChanged = false;
                UpdateResultsStatus(true);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI34590");
            }
            finally
            {
                _queryAndResultsTableLayoutPanel.ResumeLayout();
                _queryAndResultsTableLayoutPanel.Refresh();
            }
        }

        /// <summary>
        /// Handles the case that the text of a parameter control changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleParameterControlTextChanged(object sender, EventArgs e)
        {
            try
            {
                RefreshData(true);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI34591");
            }
        }

        /// <summary>
        /// Handles the <see cref="T:DataGridView.CurrentCellDirtyStateChanged"/> event for the
        /// <see cref="_resultsGrid"/>.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleResultsGridCurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            try
            {
                OnDataChanged();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI34633");
            }
        }

        /// <summary>
        /// Handles the case that column data changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Data.DataRowChangeEventArgs"/> instance containing
        /// the event data.</param>
        void HandleColumnChanged(object sender, DataColumnChangeEventArgs e)
        {
            try
            {
                // In order that the column data be validated and committed right away, call EndEdit
                // on the row. This will trigger HandleRowChanged which will validate/commit the data.
                e.Row.EndEdit();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI34594");
            }
        }

        /// <summary>
        /// Handles the case that row data changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Data.DataRowChangeEventArgs"/> instance containing
        /// the event data.</param>
        void HandleRowChanged(object sender, DataRowChangeEventArgs e)
        {
            try
            {
                UpdateRow(e.Row);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI34592");
            }
        }

        /// <summary>
        /// Handles the case that a row was added.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Data.DataTableNewRowEventArgs"/> instance
        /// containing the event data.</param>
        void HandleTableNewRow(object sender, DataTableNewRowEventArgs e)
        {
            try
            {
                UpdateRow(e.Row);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI34593");
            }
        }

        /// <summary>
        /// Handles the case that the text in the query edit box has changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleQueryScintillaBoxTextChanged(object sender, EventArgs e)
        {
            try
            {
                // Disable the parameter controls until the query is re-executed since edits to the
                // query may re-define what the parameters are.
                foreach (Control control in _queryParameterControls)
                {
                    control.Enabled = false;
                }

                _queryChanged = true;
                IsQueryDirty = (_lastSavedQuery != null &&
                    !_lastSavedQuery.Equals(_queryScintillaBox.Text, StringComparison.Ordinal));
                _saveButton.Enabled = IsQueryDirty;

                // Indicate that the results no longer necessarily reflect what is in the query box.
                UpdateResultsStatus(false);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI34561");
            }
        }

        /// <summary>
        /// Handles the <see cref="T:Control.Click"/> event for the
        /// <see cref="_sendToSeparateTabButton"/>.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleSendToSeparateTabClick(object sender, EventArgs e)
        {
            try
            {
                OnSentToSeparateTab();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI34597");
            }
        }

        /// <summary>
        /// Handles the <see cref="T:Control.Click"/> event for the <see cref="_newQueryButton"/>.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void HandleCopyToNewQueryButtonClick(object sender, EventArgs e)
        {
            try
            {
                CreateQueryCopy();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI34598");
            }
        }

        /// <summary>
        /// Handles the <see cref="T:Control.Click"/> event for the
        /// <see cref="_showHideQueryButton"/>.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleShowHideQueryButtonClick(object sender, EventArgs e)
        {
            try
            {
                QueryPanelOpen = !QueryPanelOpen;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI34632");
            }
        }

        /// <summary>
        /// Handles the <see cref="T:Control.Click"/> event for the <see cref="_renameButton"/>.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleRenameButtonClick(object sender, EventArgs e)
        {
            try
            {
                Rename();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI34599");
            }
        }

        /// <summary>
        /// Handles the <see cref="T:Control.Click"/> event for the <see cref="_saveButton"/>.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleSaveButtonClick(object sender, EventArgs e)
        {
            try
            {
                SaveQuery();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI34600");
            }
        }

        /// <summary>
        /// Handles the <see cref="T:Control.Click"/> event for the <see cref="_resultsStatusLabel"/>.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleInvalidDataLabelClicked(object sender, EventArgs e)
        {
            try
            {
                if (!DataIsValid)
                {
                    // In case the user has just correct invalid data, commit any edits that are in
                    // progress.
                    _resultsGrid.EndEdit();

                    ShowInvalidData();
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI34601");
            }
        }

        #endregion Event Handlers

        #region Private Methods

        /// <summary>
        /// Gets or sets whether the panel containing the query edit control is open.
        /// </summary>
        /// <value><see langword="true"/> if the panel containing the query edit control is open;
        /// otherwise, <see langword="false"/>.
        /// </value>
        bool QueryPanelOpen
        {
            get
            {
                return !_resultsSplitContainer.Panel1Collapsed;
            }

            set
            {
                try
                {
                    if (value != QueryPanelOpen)
                    {
                        _resultsSplitContainer.Panel1Collapsed = !value;
                        _showHideQueryButton.Text = value ? "Hide query" : "Show query";
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI34631");
                }
            }
        }

        /// <summary>
        /// Loads the table from database.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="tableName">Name of the table to load.</param>
        /// <returns>The loaded table.</returns>
        [SuppressMessage("Microsoft.Globalization", "CA1306:SetLocaleForDataTypes")]
        void LoadTableFromDatabase(SqlCeConnection connection, string tableName)
        {
            // Setup dataAdapter to get the data
            _adapter = new SqlCeDataAdapter("SELECT * FROM " + tableName, connection);

            // Fill the table with the data from the dataAdapter
            _adapter.Fill(_resultsTable);

            // Fill the schema for the table for the database
            _adapter.FillSchema(_resultsTable, SchemaType.Source);

            // Check for auto increment fields and default column values
            foreach (DataColumn c in _resultsTable.Columns)
            {
                // Get the information for the current column
                using (SqlCeCommand sqlcmd = new SqlCeCommand(
                    "SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '" +
                    tableName + "' AND COLUMN_NAME = '" + c.ColumnName + "'", connection))
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

            // Create a command builder for the adapter that allow edits made in the _resultsGrid
            // to be applied back to the database.
            _commandBuilder = new SqlCeCommandBuilder();
            _commandBuilder.DataAdapter = _adapter;
        }

        /// <summary>
        /// Automatically adjusts the column sizes to try to take best advantage of the currently
        /// available space.
        /// </summary>
        void AutoSizeColumns()
        {
            // Generate the default column sizes.
            using (var graphics = _resultsGrid.CreateGraphics())
            {
                var font = _resultsGrid.ColumnHeadersDefaultCellStyle.Font;
                for (int i = 0; i < _resultsTable.Columns.Count; i++)
                {
                    var column = _resultsTable.Columns[i];
                    var type = column.DataType;

                    // Default fill weight will be 1 with a minimum width of 50 (~ 6 chars)
                    float fillWeight = 1.0F;
                    int minSize = 50;

                    if (type == typeof(bool))
                    {
                        // Bool fields do not need to scale larger with size as the table does and
                        // can have a smaller min width.
                        fillWeight = 0.01F;
                        minSize = 25;
                    }
                    else if (type == typeof(string))
                    {
                        // For text fields that can be > 10 chars but not unlimited, scale up from
                        // a fill weight of 1.0 logarithmically.
                        // MaxLength 10	 = 1.0
                        // MaxLength 50	 = 2.6
                        // MaxLength 500 = 4.9
                        if (column.MaxLength > 10)
                        {
                            fillWeight = (float)Math.Log((double)column.MaxLength / 3.7);
                        }

                        // But cap the max fill weight at 7 (MaxLength ~5000)
                        if (fillWeight > 7.0F || column.MaxLength == -1)
                        {
                            fillWeight = 7.0F;
                        }
                    }

                    // Get the head text and measure out the minimum width to display the string
                    // Pad the width by 4 pixels (2 each side)
                    var temp = _resultsGrid.Columns[i].HeaderText;
                    var size = graphics.MeasureString(temp, font);
                    minSize = Math.Max(minSize, (int)(size.Width + 0.5) + 4);
                    _resultsGrid.Columns[i].MinimumWidth = minSize;
                    _resultsGrid.Columns[i].FillWeight = fillWeight;
                    _resultsGrid.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                }

                // Perform a layout to apply the default column sizes.
                _resultsGrid.PerformLayout();

                // Once the default column sizes have been applied, turn off auto-sizing and set
                // min width to a small size to allow the user almost complete control over column
                // sizes.
                foreach (DataGridViewColumn column in _resultsGrid.Columns)
                {
                    column.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                    column.MinimumWidth = 25;
                }
            }
        }

        /// <summary>
        /// Parses the <see paramref="query"/> to extract any parameter definitions from the primary
        /// query.
        /// </summary>
        /// <param name="connection">The connection to the database to use if needed to define the
        /// available values for a query.</param>
        /// <param name="query">The query.</param>
        void ParseQuery(SqlCeConnection connection, string query)
        {
            // Remove any existing parameter controls.
            RemoveParameterControls();

            // Parse the overall query text into separate queries delemited by "GO" statements.
            Regex queryParserRegex = new Regex(@"[\r\n]+[\s]*GO[\s]*[\r\n]+");

            string[] queries = queryParserRegex.Split(query);
            _parametersTableLayoutPanel.RowCount = queries.Count();

            // Create a regex to parse out the prompt text for each parameter
            queryParserRegex = new Regex(@"(?<=--)[\s\S]+?(?=[\r\n]|$)");

            // All queries except the last one define parameters.
            for (int i = 0; i < queries.Length - 1; i++)
            {
                string prompt = queryParserRegex.Match(queries[i])
                    .ToString()
                    .Trim();

                string value = "";
                if (_lastUsedParameters.Count > i && _lastUsedParameters[i].Key == prompt)
                {
                    // The last time the query was executed, the prompt for this parameter was the
                    // same; attempt to re-use the last specified value.
                    value = _lastUsedParameters[i].Value;
                }
                else
                {
                    // The prompt for this parameter has changed since the last execution. Create
                    // a new entry in _lastUsedParameters.
                    if (_lastUsedParameters.Count == i)
                    {
                        _lastUsedParameters.Add(new KeyValuePair<string, string>(prompt, ""));
                    }
                    else
                    {
                        _lastUsedParameters[i] = new KeyValuePair<string, string>(prompt, "");
                    }
                }

                // Attempt to generate a list of available values for the parameter.
                string[] availableValues = null;
                if (!string.IsNullOrWhiteSpace(queries[i]))
                {
                    try
                    {
                        using (DbCommand parameterQuery = DBMethods.CreateDBCommand(
                            connection, queries[i], null))
                        {
                            availableValues = DBMethods.ExecuteDBQuery(parameterQuery, ",");
                        }
                    }
                    // Ignore any exceptions. A free-form edit box will be provided to enter the
                    // paramter value.
                    catch { }
                }

                // Add either a comboBox or textBox for the user to enter the parameter value
                // depending upon whether there are any availableValues.
                AddNewParameterControl(i, prompt, availableValues, value);
            }

            _masterQuery = queries.Last();
        }

        /// <summary>
        /// Add either a <see cref="ComboBox"/> or <see cref="TextBox"/> for the user to enter the
        /// parameter value depending upon whether there are any <see paramref="availableValues"/>.
        /// </summary>
        /// <param name="index">The index of the parameter</param>
        /// <param name="prompt">The prompt to display for the parameter</param>
        /// <param name="availableValues">The available values for the paramter. If
        /// <see langword="null"/>, the paramter control will be a <see cref="TextBox"/>, otherwise
        /// the control will be a <see cref="ComboBox"/> whose drop list will be populated with the
        /// available values.</param>
        /// <param name="value">The value.</param>
        void AddNewParameterControl(int index, string prompt, string[] availableValues, string value)
        {
            // Create a label with the specified prompt.
            Label label = new Label();
            label.AutoSize = true;
            label.Anchor = AnchorStyles.Left;
            label.Text = prompt;
            _parametersTableLayoutPanel.Controls.Add(label, 0, index);

            // If there are available values, create a ComboBox and set its auto-complete list to
            // the availableValues.
            if (availableValues != null && availableValues.Length > 0)
            {
                ComboBox comboBox = new ComboBox();
                comboBox.Anchor = AnchorStyles.Left;
                comboBox.Items.AddRange(availableValues);
                var autoCompleteList = new AutoCompleteStringCollection();
                autoCompleteList.AddRange(availableValues.Distinct().ToArray());
                comboBox.AutoCompleteCustomSource = autoCompleteList;
                comboBox.AutoCompleteSource = AutoCompleteSource.CustomSource;
                comboBox.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                comboBox.Text = value;
                comboBox.TextChanged += HandleParameterControlTextChanged;

                _parametersTableLayoutPanel.Controls.Add(comboBox, 1, index);
                _queryParameterControls.Add(comboBox);
            }
            // If there are no available values, create a TextBox to accept the parameter.
            else
            {
                TextBox textBox = new TextBox();
                textBox.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
                textBox.Text = value;
                textBox.TextChanged += HandleParameterControlTextChanged;

                _parametersTableLayoutPanel.Controls.Add(textBox, 1, index);
                _queryParameterControls.Add(textBox);
            }
        }

        /// <summary>
        /// Removes and disposes of the existing parameter controls (included labels).
        /// </summary>
        void RemoveParameterControls()
        {
            List<Control> oldControls =
                new List<Control>(_parametersTableLayoutPanel.Controls.OfType<Control>());

            _parametersTableLayoutPanel.Controls.Clear();
            _parametersTableLayoutPanel.RowCount = 1;
            _queryParameterControls.Clear();

            CollectionMethods.ClearAndDispose(oldControls);
        }

        /// <summary>
        /// Updates the controls that relate to the status of the currently displayed results.
        /// </summary>
        /// <param name="resultsAreCurrent"><see langword="true"/> if the displayed results are
        /// known to be up-to-date; <see langword="false"/> otherwise.</param>
        void UpdateResultsStatus(bool resultsAreCurrent)
        {
            if (resultsAreCurrent)
            {
                _executeQueryButton.Enabled = false;
                _resultsStatusLabel.Visible = false;
            }
            else
            {
                // If the results are not necessarily up-to-date, update the label indicating why.
                if (IsTable)
                {
                    ExtractException.Assert("ELI34602", "Unexpected table state.", !DataIsValid);
                    _resultsStatusLabel.Text = "Invalid has been entered. (click here to view)";
                }
                else if (_resultsChanged)
                {
                    _resultsStatusLabel.Text = "Query results are out of date";
                }
                else if (_queryChanged)
                {
                    _resultsStatusLabel.Text = "Query has been modified since the last execution";
                }
                else
                {
                    ExtractException.ThrowLogicException("ELI34630");
                }

                _executeQueryButton.Enabled = true;
                _resultsStatusLabel.Visible = true;
            }
        }

        /// <summary>
        /// Applies edits from the <see cref="_resultsGrid"/> to the specified <see paramref="row"/>.
        /// </summary>
        /// <param name="row">The <see cref="DataRow"/> for which edits should be applied.</param>
        void UpdateRow(DataRow row)
        {
            // Remove any rows from _invalidRows that no longer have errors.
            if (!row.HasErrors && _invalidRows.Count > 0)
            {
                int index = _resultsTable.Rows.IndexOf(row);
                if (index >= 0 && index < _resultsGrid.RowCount)
                {
                    var gridRow = _resultsGrid.Rows[index];
                    if (_invalidRows.Contains(gridRow))
                    {
                        _invalidRows.Remove(gridRow);
                    }
                }
            }

            // Attempt to commit the row data to the table.
            try
            {
                _adapter.Update(_resultsTable);
                RefreshData(true);
                OnDataChanged();

                // If the update was successful, all data is now valid. Ensure the _invalidRows
                // list is clear and update the result status controls.
                _invalidRows.Clear();
                UpdateResultsStatus(true);
            }
            catch
            {
                // Add the offending DataGridView row to _invalidRows.
                try
                {
                    int invalidRowIndex = _resultsTable.Rows.IndexOf(row);
                    if (invalidRowIndex >= 0)
                    {
                        var invalidRow = _resultsGrid.Rows[invalidRowIndex];
                        if (!_invalidRows.Contains(invalidRow))
                        {
                            _invalidRows.Add(invalidRow);
                        }
                    }
                    else
                    {
                        // If we could not resolve the offending row, add null so that _invalidRows
                        // is not empty which will cause the table's data to be flagged as invalid.
                        _invalidRows.Add(null);
                    }
                }
                catch
                {
                    // If we could not resolve the offending row, add null so that _invalidRows
                    // is not empty which will cause the table's data to be flagged as invalid.
                    _invalidRows.Add(null);
                }

                UpdateResultsStatus(false);

                // An error icon will be displayed to call attention to the problem; no need to
                // throw or display exception.
            }
        }

        /// <summary>
        /// Raises the <see cref="DataChanged"/> event.
        /// </summary>
        void OnDataChanged()
        {
            if (DataChanged != null)
            {
                DataChanged(this, new EventArgs());
            }
        }

        /// <summary>
        /// Raises the <see cref="SentToSeparateTab"/> event.
        /// </summary>
        void OnSentToSeparateTab()
        {
            if (SentToSeparateTab != null)
            {
                SentToSeparateTab(this, new EventArgs());
            }
        }

        /// <summary>
        /// Raises the <see cref="PropertyChanged"/> event.
        /// </summary>
        /// <param name="propertyName">The name of the property that has changed.</param>
        void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        /// <summary>
        /// Raises the <see cref="QueryCreated"/> event.
        /// </summary>
        /// <param name="queryAndResultsControl">The <see cref="QueryAndResultsControl"/>
        /// that has been created.</param>
        void OnQueryCreated(QueryAndResultsControl queryAndResultsControl)
        {
            if (QueryCreated != null)
            {
                QueryCreated(this, new QueryCreatedEventArgs(queryAndResultsControl));
            }
        }

        /// <summary>
        /// Raises the <see cref="QueryRenaming"/> event.
        /// </summary>
        /// <param name="eventArgs">The <see cref="Extract.SQLCDBEditor.QueryRenamingEventArgs"/>
        /// instance containing the event data.</param>
        void OnQueryRenaming(QueryRenamingEventArgs eventArgs)
        {
            if (QueryRenaming != null)
            {
                QueryRenaming(this, eventArgs);
            }
        }

        /// <summary>
        /// Raises the <see cref="QuerySaved"/> event.
        /// </summary>
        void OnQuerySaved()
        {
            if (QuerySaved != null)
            {
                QuerySaved(this, new EventArgs());
            }
        }

        #endregion Private Methods
    }
}
