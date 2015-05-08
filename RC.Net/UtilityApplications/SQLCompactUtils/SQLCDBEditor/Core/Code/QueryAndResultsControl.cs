﻿using Extract.Database;
using Extract.Licensing;
using Extract.Utilities;
using Extract.Utilities.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlServerCe;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Extract.SQLCDBEditor
{
    /// <summary>
    /// The available types of <see cref="QueryAndResultsControl"/>s.
    /// </summary>
    internal enum QueryAndResultsType
    {
        /// <summary>
        /// Displays and allows for editing of database queries.
        /// </summary>
        Table,

        /// <summary>
        /// Allows for editing and execution of queries.
        /// </summary>
        Query,

        /// <summary>
        /// Provides custom behavior.
        /// </summary>
        Plugin
    }

    /// <summary>
    /// A control which allow for the viewing/editing of a database table or query as well as
    /// modification of a query.
    /// </summary>
    internal partial class QueryAndResultsControl : UserControl, INotifyPropertyChanged,
        ISQLCDBEditorPluginManager
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(QueryAndResultsControl).ToString();

        #endregion Constants

        #region Fields

        /// <summary>
        /// The connection to the database.
        /// </summary>
        SqlCeConnection _connection;

        /// <summary>
        /// The name of the loaded database table.
        /// </summary>
        string _tableName;

        /// <summary>
        /// The data table representing the table contents or query results.
        /// </summary>
        DataTable _resultsTable = new DataTable();

        /// <summary>
        /// The names of the columns that comprise the primary key for the _resultsTable;
        /// </summary>
        List<string> _primaryKeyColumnNames;

        /// <summary>
        /// The data adapter to populate <see cref="_resultsTable"/> for database tables.
        /// </summary>
        SqlCeDataAdapter _adapter;

        /// <summary>
        /// The <see cref="SqlCeCommandBuilder"/> used to facilitate database table editing.
        /// </summary>
        SqlCeCommandBuilder _commandBuilder;

        /// <summary>
        /// Stores the MaxLength for each column in the table for use in calculating default column
        /// sizes.
        /// </summary>
        Dictionary<int, int> _columnSizes = new Dictionary<int, int>();

        /// <summary>
        /// The query to evaluate. This does not contain any sub-queries for defining query
        /// parameters.
        /// </summary>
        string _masterQuery;

        /// <summary>
        /// The last query text saved or loaded from disk. Used to determine if the query is
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
        /// The queries used to generate lists of available values for the parameter(s).
        /// </summary>
        List<string> _parameterQueries = new List<string>();

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
        /// Indicates whether an error was encountered the last time the query was executed.
        /// </summary>
        bool _queryError;
        
        /// <summary>
        /// Indicates whether the query text is dirty.
        /// </summary>
        bool _isQueryDirty;

        /// <summary>
        /// Indicates if query execution is allowed. When <see cref="QueryModifiesData"/>, this is
        /// set to <see langword="false"/>, unless the user explicitly presses the execute button to
        /// prevent un-intended modification of data.
        /// </summary>
        bool _allowQueryExcecution = true;

        /// <summary>
        /// The <see cref="SQLCDBEditorPlugin"/> being hosted by this instance.
        /// </summary>
        SQLCDBEditorPlugin _plugin;

        /// <summary>
        /// Indicates whether the initial <see cref="Control.Layout"/> call (after loading) has
        /// completed. Used to determine the right time to compute the initial size of the columns
        /// or plugin.
        /// </summary>
        bool _initialLayoutComplete;

        /// <summary>
        /// Indicates whether this control's data is valid.
        /// </summary>
        bool _dataIsValid;

        /// <summary>
        /// A data error encountered for a table row that has yet to be resolved.
        /// </summary>
        DataGridViewDataErrorEventArgs _activeDataError;

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
        /// <param name="queryAndResultsType">The <see cref="QueryAndResultsType"/> of this
        /// instance.</param>
        public QueryAndResultsControl(QueryAndResultsType queryAndResultsType)
        {
            try
            {
                _inDesignMode = (LicenseManager.UsageMode == LicenseUsageMode.Designtime);

                if (_inDesignMode)
                {
                    // Load the license files from folder
                    LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());
                }

                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI38021",
                    _OBJECT_NAME);
               
                InitializeComponent();

                _resultsTable.Locale = CultureInfo.CurrentCulture;
                QueryAndResultsType = queryAndResultsType;
                if (QueryAndResultsType == QueryAndResultsType.Query)
                {
                    Name = "New Query";
                }
                DataIsValid = true;
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
        /// <param name="queryAndResultsType">The <see cref="QueryAndResultsType"/> of this
        /// instance.</param>
        public QueryAndResultsControl(string name, string fileName,
            QueryAndResultsType queryAndResultsType)
            : base()
        {
            try
            {
                _inDesignMode = LicenseManager.UsageMode == LicenseUsageMode.Designtime;
  
                InitializeComponent();

                _resultsTable.Locale = CultureInfo.CurrentCulture;
                Name = name;
                FileName = fileName;
                QueryAndResultsType = queryAndResultsType;
                DataIsValid = true;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI34574");
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryAndResultsControl"/> class.
        /// </summary>
        /// <param name="plugin">The <see cref="SQLCDBEditorPlugin"/> to host in this instance.
        /// </param>
        public QueryAndResultsControl(SQLCDBEditorPlugin plugin)
            : base()
        {
            try
            {
                _inDesignMode = LicenseManager.UsageMode == LicenseUsageMode.Designtime;

                InitializeComponent();

                _resultsTable.Locale = CultureInfo.CurrentCulture;
                Name = plugin.DisplayName;
                _plugin = plugin;
                _plugin.DataChanged += HandlePluginDataChanged;
                _plugin.StatusMessageChanged += HandlePluginStatusMessageChanged;
                QueryAndResultsType = QueryAndResultsType.Plugin;
                DataIsValid = true;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI34842");
            }
        }

        #endregion Constructors

        #region Events

        /// <summary>
        /// Raised when the user has modified data.
        /// </summary>
        public event EventHandler<DataChangedEventArgs> DataChanged;

        /// <summary>
        /// Raised when the plugin has a new status message to display (may be
        /// <see langword="null"/> to clear the existing status message.
        /// </summary>
        public event EventHandler<StatusMessageChangedEventArgs> StatusMessageChanged;

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

        /// <summary>
        /// Raised to indicate the selection in the query or results grid has changed.
        /// </summary>
        public event EventHandler<GridSelectionEventArgs> SelectionChanged;

        #endregion Events

        #region Properties

        /// <summary>
        /// Gets the name of the table or query, suffixed by an asterisk if a query is dirty.
        /// </summary>
        /// <returns>The name of the table or query, suffixed by an asterisk if a query is dirty.
        /// </returns>
        // Don't obfuscate because this un-obfuscated property name is needed by the PropertyChanged
        // event.
        [ObfuscationAttribute(Exclude = true)]
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
        /// Gets the <see cref="QueryAndResultsType"/> of this instance.
        /// </summary>
        public QueryAndResultsType QueryAndResultsType
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
        /// Gets a value indicating whether this control's data is valid.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the data represented by this instance is valid; otherwise,
        /// <see langword="false"/>.
        /// </value>
        public bool DataIsValid
        {
            get
            {
                return _dataIsValid;
            }

            private set
            {
                try
                {
                    if (value != _dataIsValid)
                    {
                        _dataIsValid = value;

                        if (value)
                        {
                            _activeDataError = null;

                            if (IsLoaded)
                            {
                                // Ensure ErrorText is cleared for all rows in the table.
                                foreach (var row in _resultsGrid.Rows
                                    .OfType<DataGridViewRow>()
                                    .Where(row => !string.IsNullOrEmpty(row.ErrorText)))
                                {
                                    row.ErrorText = null;
                                }
                            }
                        }

                        // As long as this instance is loaded (and the validity change is per user
                        // interaction) update control to reflect the current validity status.
                        if (IsLoaded)
                        {
                            UpdateResultsStatus(value);
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI38222");
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

                        OnPropertyChanged("DisplayName");
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI34576");
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the query exists on disk as an sqlce file.
        /// </summary>
        /// <value><see langword="true"/> if query exists on disk; otherwise,
        /// <see langword="false"/>.
        /// </value>
        public bool QueryFileExists
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a value indicating whether the query is currently blank.
        /// </summary>
        /// <value><see langword="true"/> if the query blank; otherwise, <see langword="false"/>.
        /// </value>
        public bool IsQueryBlank
        {
            get
            {
                return string.IsNullOrWhiteSpace(_queryScintillaBox.Text);
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
                ExtractException.Assert("ELI34577", "Cannot load as a database table",
                    QueryAndResultsType == QueryAndResultsType.Table);

                _connection = connection;
                _tableName = tableName;

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
                ExtractException.Assert("ELI34580", "Cannot load as a query",
                    QueryAndResultsType == QueryAndResultsType.Query);

                // Use ControlDark for the background color to provide a border around to query edit
                // box.
                _resultsSplitContainer.Panel1.BackColor = System.Drawing.SystemColors.ControlDark;

                QueryFileExists = File.Exists(FileName);

                // If the query doesn't yet exist on disk, the save button should be enabled and the
                // query panel should be open.
                if (!QueryFileExists)
                {
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

                LoadQueryCore(connection, query);

                if (_resultsGrid.RowCount > 0)
                {
                    _resultsGrid.Rows[0].Selected = true;
                }
            }
            catch (Exception ex)
            {
                _queryError = true;

                throw ex.AsExtract("ELI34581");
            }
        }

        /// <summary>
        /// Loads the plugin by providing it the <see paramref="connection"/> and using the 
        /// <see cref="SQLCDBEditorPlugin.BindingSource"/> it creates using the connection.
        /// </summary>
        /// <param name="connection">The <see cref="SqlCeConnection"/> to the database.</param>
        void LoadPluginViaBindingSource(SqlCeConnection connection)
        {
            _lastUsedParameters.Clear();

            _resultsGrid.AllowUserToAddRows = false;
            _resultsGrid.AllowUserToDeleteRows = false;
            _resultsGrid.AutoGenerateColumns = true;

            _connection = connection;
            _plugin.LoadPlugin(this, _connection);

            _resultsGrid.DataSource = _plugin.BindingSource;

            // For a plugin that provides its own data source, we will not be able to keep track of
            // more that one data error simultaneously. The RowValidating should be used to prevent
            // the user from moving on from any row until any error has been resolved.
            _resultsGrid.RowValidating += HandleResultsGrid_RowValidating;
            _resultsGrid.RowsRemoved += HandleResultsGrid_DataBoundRowsRemoved;

            IsLoaded = true;
        }

        /// <summary>
        /// Loads the requests grid using the <see paramref="query"/>. This is logic that is shared
        /// between query and plugin types.
        /// </summary>
        /// <param name="connection">The connection to the database.</param>
        /// <param name="query">The query.</param>
        void LoadQueryCore(SqlCeConnection connection, string query)
        {
            ExtractException.Assert("ELI34580", "No query can be loaded from this control type.",
                QueryAndResultsType == QueryAndResultsType.Query ||
                QueryAndResultsType == QueryAndResultsType.Plugin);

            _connection = connection;
            _lastUsedParameters.Clear();

            _queryScintillaBox.Text = query;
            _queryScintillaBox.IsReadOnly = IsReadOnly;
            _queryScintillaBox.TextChanged += HandleQueryScintillaBoxTextChanged;
            _lastSavedQuery = query;
            _resultsGrid.AllowUserToAddRows = false;
            _resultsGrid.AllowUserToDeleteRows = false;

            // If we have gotten this far, consider the query loaded even if the query cannot be
            // evaluated. The user will be able to edit the query to allow it to evaluate.
            IsLoaded = true;

            // Parse the query in order to define any parameters used by the query.
            ParseQuery(connection, query);

            if (QueryModifiesData)
            {
                // Query execution should be prevented except in the case that the execute button is
                // manually pressed.
                _allowQueryExcecution = false;
            }
            else
            {
                // Populate the results grid.
                RefreshData(true, true);
            }

            _resultsGrid.DataSource = _resultsTable;
        }

        /// <summary>
        /// Loads a plugin into this instance.
        /// </summary>
        /// <param name="connection">The connection to the database.</param>
        public void LoadPlugin(SqlCeConnection connection)
        {
            try
            {
                ExtractException.Assert("ELI34580", "Cannot load as a plugin",
                    QueryAndResultsType == QueryAndResultsType.Plugin);

                // Put the query results in Panel1 (if the plugin has a query) and the plugin
                // control in Panel2.
                _resultsSplitContainer.Panel1.Controls.Clear();
                _resultsSplitContainer.Panel2.Controls.Clear();

                if (!string.IsNullOrEmpty(_plugin.Query) || UsingPluginBindingSource)
                {
                    _resultsSplitContainer.Panel1.Controls.Add(_resultsPanel);
                    _resultsSplitContainer.Panel1Collapsed = false;
                }
                else
                {
                    _resultsSplitContainer.Panel1Collapsed = true;
                }

                if (_plugin.DisplayControl)
                {
                    _resultsSplitContainer.Panel2.Controls.Add(_plugin);
                    _plugin.Dock = DockStyle.Fill;
                }
                else
                {
                    _resultsSplitContainer.Panel2Collapsed = true;
                }

                // Hide the buttons used by the query control type.
                _newQueryButton.Visible = false;
                _showHideQueryButton.Visible = false;
                _renameButton.Visible = false;
                _saveButton.Visible = false;
                _executeQueryButton.Visible = false;

                // initialize the query results grid. 
                if (UsingPluginBindingSource)
                {
                    LoadPluginViaBindingSource(connection);
                }
                else
                {
                    LoadQueryCore(connection, _plugin.Query);
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI34823");
            }
        }

        /// <summary>
        /// Refreshes the data.
        /// </summary>
        /// <param name="updateQueryResult"><see langword="true"/> to update the results or a query
        /// <see langword="false"/> to just make the visible results as out-of-date.</param>
        /// <param name="forceQueryExcecution"><see langword="true"/> toForce the query to be
        /// re-executed and be updated; <see langword="false"/> otherwise.</param>
        public void RefreshData(bool updateQueryResult, bool forceQueryExcecution)
        {
            DataTable latestDataTable = null;

            try
            {
                // If the results table has not been created or it currently contains invalid data,
                // prevent a refresh.
                if ((_resultsTable == null && !UsingPluginBindingSource) || !DataIsValid)
                {
                    return;
                }

                // Load the latest data into a latestDataTable so that we can check to see if the
                // data already in _resultsTable is out of date.
                latestDataTable = new DataTable();
                latestDataTable.Locale = CultureInfo.CurrentCulture;

                // If table data is not valid prior to the merge, always consider the data changed
                // so that it forces invalid rows to be revalidated.
                bool dataChanged = !DataIsValid;

                if (QueryAndResultsType == QueryAndResultsType.Table)
                {
                    _adapter.Fill(latestDataTable);
                    ApplySchema(latestDataTable);
                    StoreColumnSizes(latestDataTable);

                    if (!DataIsValid)
                    {
                        // If there is currently any invalid data in the table, merge the data from
                        // the currently invalid rows into latestDataTable so that the user's edits
                        // are preserved even if invalid. Note that this call will clear any
                        // constraints on latestDataTable.
                        DataIsValid = MergeRowsIntoTable(latestDataTable, _resultsTable.Rows
                            .OfType<DataRow>()
                            .Where(row => row.HasErrors));
                    }
                    else
                    {
                        // [DotNetRCAndUtils:826]
                        // Before using latestDataTable, the constraints need to be cleared so that
                        // if new rows are added which violates a constraint, they will be flagged
                        // but allowed to continue to exist in the table until the user gets around
                        // to correcting the data.
                        RemoveConstraints(latestDataTable);
                    }
                }
                else if (QueryAndResultsType == QueryAndResultsType.Query)
                {
                    // [DotNetRCAndUtils:824]
                    // We already know the query needs to be re-run. The query may be in a partially
                    // dirty state and unable to run anyway.
                    if ((_queryChanged || _queryError || _resultsChanged) && !forceQueryExcecution)
                    {
                        return;
                    }

                    if (_allowQueryExcecution)
                    {
                        RefreshQuery(latestDataTable);
                    }
                    else
                    {
                        UpdateResultsStatus(false);
                    }
                }
                else if (QueryAndResultsType == QueryAndResultsType.Plugin)
                {
                    if (UsingPluginBindingSource)
                    {
                        // For the time being, just always assume a plugin's binding source has
                        // changed. Can later revisit making this code agnostic as to whether the
                        // data is provided via a DataTable or BindingSource.
                        dataChanged = true;
                    }
                    else
                    {
                        RefreshQuery(latestDataTable);
                        _plugin.RefreshData();
                    }
                }

                if (QueryAndResultsType == QueryAndResultsType.Query && forceQueryExcecution)
                {
                    // Always refresh the results if forceQueryExcecution is true.
                    dataChanged = true;
                }
                // Check to see if the data in _resultsTable differs from the latestDataTable
                else if (latestDataTable.Rows.Count != _resultsTable.Rows.Count ||
                    latestDataTable.Columns.Count != _resultsTable.Columns.Count)
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

                if (QueryAndResultsType == QueryAndResultsType.Query && !updateQueryResult)
                {
                    // If a query and the query results shouldn't be automatically updated, mark
                    // them as out-of-date.
                    _resultsChanged = true;
                    UpdateResultsStatus(false);
                }
                else
                {
                    UpdateResultsGrid(latestDataTable);

                    // After applying the results to the grid, they are now in use and we don't want
                    // to dispose of them.
                    latestDataTable = null;
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
        /// Applies the result data/results to the grid and resets the column sizes and scroll
        /// position as appropriate.
        /// </summary>
        /// <param name="latestDataTable">A <see cref="DataTable"/> containing the latest
        /// data/results.</param>
        void UpdateResultsGrid(DataTable latestDataTable)
        {
            // If updating the results, keep track of the sort order, last scroll position,
            // and column sized so that we can keep the same data visible.
            int sortedColumnIndex = (_resultsGrid.SortedColumn == null)
                ? -1
                : _resultsGrid.SortedColumn.Index;
            ListSortDirection sortOrder = (_resultsGrid.SortOrder == SortOrder.Descending)
                ? ListSortDirection.Descending
                : ListSortDirection.Ascending;
            int scrollPos = _resultsGrid.FirstDisplayedScrollingRowIndex;
            int[] columnWidths = _resultsGrid.Columns
                .OfType<DataGridViewColumn>()
                .Select(column => column.Width)
                .ToArray();

            // While refreshing, don't handle data changed events so that the changes are
            // not interpreted as edits by the user.
            if (QueryAndResultsType == QueryAndResultsType.Table)
            {
                _resultsTable.TableNewRow -= HandleTableNewRow;
                _resultsTable.ColumnChanged -= HandleColumnChanged;
                _resultsTable.RowChanged -= HandleRowChanged;
            }

            // Apply the latest data to the grid.
            _resultsGrid.DataSource = null;

            if (UsingPluginBindingSource)
            {
                _plugin.RefreshData();
                _resultsGrid.DataSource = _plugin.BindingSource;
            }
            else
            {
                _resultsTable.Dispose();
                _resultsTable = latestDataTable;
                _resultsGrid.DataSource = _resultsTable;
            }

            if (QueryAndResultsType == QueryAndResultsType.Table)
            {
                // Re-register to get data changed events.
                _resultsTable.TableNewRow += HandleTableNewRow;
                _resultsTable.ColumnChanged += HandleColumnChanged;
                _resultsTable.RowChanged += HandleRowChanged;
            }

            // If this is a table or a query has been re-executed without changing, restore
            // the previous column sizes that had
            if (((QueryAndResultsType != QueryAndResultsType.Query) || !_queryChanged) &&
                columnWidths.Length == _resultsGrid.Columns.Count)
            {
                // Re-apply the previous column widths.
                foreach (DataGridViewColumn column in _resultsGrid.Columns)
                {
                    column.Width = columnWidths[column.Index];
                }

                // Restore the previous sort order.
                if (sortedColumnIndex >= 0)
                {
                    _resultsGrid.Sort(_resultsGrid.Columns[sortedColumnIndex], sortOrder);
                }

                // Re-apply the previous vertical scroll position for tables to try to make it
                // appear that the table was updated in-place.
                if (QueryAndResultsType != QueryAndResultsType.Query)
                {
                    if (scrollPos >= 0 && scrollPos < _resultsGrid.RowCount)
                    {
                        _resultsGrid.FirstDisplayedScrollingRowIndex = scrollPos;
                    }
                }
            }
            else if (_initialLayoutComplete)
            {
                // Always auto-size columns if a query has changed since the last execution
                // because the columns may no longer be the same.
                AutoSizeColumns();
            }

            _resultsGrid.Refresh();
        }

        /// <summary>
        /// Re-executes the query and populates <see paramref="latestDataTable"/> with the results.
        /// </summary>
        /// <param name="latestDataTable">The <see cref="DataTable"/> that should be populated with
        /// the query results.</param>
        void RefreshQuery(DataTable latestDataTable)
        {
            // Update the list of available values for any ComboBox parameter controls.
            RefreshParameterControls();

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

                _queryError = false;

                if (QueryModifiesData)
                {
                    _allowQueryExcecution = false;

                    OnDataChanged(true, false);
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
                QueryFileExists = true;
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
                ExtractException.Assert("ELI34583", "Only editable queries may be renamed.",
                    (QueryAndResultsType == QueryAndResultsType.Query) && !IsReadOnly);

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
                var queryAndResultsControl = new QueryAndResultsControl(QueryAndResultsType.Query);

                switch (QueryAndResultsType)
                {
                    case QueryAndResultsType.Table:
                        {
                            // Create a query selecting all columns explicitly.
                            string query = "SELECT ";

                            query += string.Join("\r\n\t,", _resultsTable.Columns
                                .OfType<DataColumn>()
                                .Select(column => "[" + column.ColumnName + "]"));
                            query += "\r\n\tFROM [" + Name + "]";

                            queryAndResultsControl.LoadQuery(_connection, query);
                        }
                        break;

                    case QueryAndResultsType.Query:
                        {
                            queryAndResultsControl.LoadQuery(_connection, _queryScintillaBox.Text);
                        }
                        break;

                    default:
                        {
                            ExtractException.ThrowLogicException("ELI34824");
                        }
                        break;
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
        /// Ends any active editing of table data.
        /// </summary>
        public void EndDataEdit()
        {
            try
            {
                // If the grid is currently being edited, apply the active edit to force the
                // data to be validated.
                if (_resultsGrid.IsCurrentCellInEditMode)
                {
                    _resultsGrid.EndEdit();

                    if (QueryAndResultsType == QueryAndResultsType.Table)
                    {
                        DataRow row =
                            ((DataRowView)_resultsGrid.CurrentCell.OwningRow.DataBoundItem).Row;
                        if (row.RowState == DataRowState.Detached)
                        {
                            _resultsTable.Rows.Add(row);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI34662");
            }
        }

        /// <summary>
        /// If there is any invalid data in the table, selects and displays that data.
        /// </summary>
        public string ShowInvalidData()
        {
            try
            {
                DataGridViewRow firstInvalidRow = _resultsGrid.Rows
                    .OfType<DataGridViewRow>()
                    .FirstOrDefault(row => !string.IsNullOrEmpty(row.ErrorText));

                if (firstInvalidRow != null)
                {
                    _resultsGrid.ClearSelection();
                    firstInvalidRow.Selected = true;
                    int firstRowIndex = _resultsGrid.FirstDisplayedScrollingRowIndex;
                    int lastRowIndex = firstRowIndex + _resultsGrid.DisplayedRowCount(false) - 1;
                    if (firstInvalidRow.Index < firstRowIndex ||
                        firstInvalidRow.Index > lastRowIndex)
                    {
                        _resultsGrid.FirstDisplayedScrollingRowIndex = firstInvalidRow.Index;
                    }

                    return firstInvalidRow.ErrorText;
                }

                return null;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI34588");
            }
        }

        #endregion Methods

        #region ISQLCDBEditorPluginManager Members

        /// <summary>
        /// Creates a new <see cref="Button"/> in the plugin toolstrip for use by the plugin.
        /// </summary>
        /// <returns>The <see cref="Button"/>.</returns>
        public Button GetNewButton()
        {
            try
            {
                Button button = new Button();
                button.AutoSize = true;
                _buttonsFlowLayoutPanel.Controls.Add(button);

                return button;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI34825");
            }
        }

        /// <summary>
        /// Causes the results of the <see cref="SQLCDBEditorPlugin.Query"/> to be refreshed.
        /// </summary>
        public void RefreshQueryResults()
        {
            try
            {
                RefreshData(true, false);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI34826");
            }
        }

        #endregion ISQLCDBEditorPluginManager Members

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
                    _queryScintillaBox.ConfigurationManager.Configure();
                    _queryScintillaBox.Lexing.Colorize();
                }

                // [DataEntry:882, 885]
                // Make the table read only based BindingSource read-only status or on the presence
                // of a primary key for tables. Without a primary key, table rows cannot be deleted
                // and modified data cannot be merged with changes from other tables.
                _resultsGrid.ReadOnly = UsingPluginBindingSource
                    ? _plugin.BindingSource.IsReadOnly
                    : (_primaryKeyColumnNames == null || _primaryKeyColumnNames.Count() == 0);
                _resultsGrid.AllowUserToAddRows = !_resultsGrid.ReadOnly;
                _resultsGrid.AllowUserToDeleteRows = !_resultsGrid.ReadOnly;

                // If read-only, selected rows in the query control will be passed to a plugin a row
                // at a time, so use full row selection.
                _resultsGrid.SelectionMode = _resultsGrid.ReadOnly
                    ? DataGridViewSelectionMode.FullRowSelect
                    : DataGridViewSelectionMode.RowHeaderSelect;

                if (_resultsTable != null)
                {
                    // [DotNetRCAndUtils:826]
                    // Before using _resultsTable, the constraints need to be cleared so that if new
                    // rows are added which violates a constraint, they will be flagged but allowed to
                    // continue to exist in the table until.
                    RemoveConstraints(_resultsTable);
                }

                _resultsTable.TableNewRow += HandleTableNewRow;
                _resultsTable.ColumnChanged += HandleColumnChanged;
                _resultsTable.RowChanged += HandleRowChanged;

                UpdateResultsStatus(!QueryModifiesData && !_queryError);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI34589");
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.Layout"/> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.LayoutEventArgs"/> that contains the event data.</param>
        protected override void OnLayout(LayoutEventArgs e)
        {
            try
            {
                base.OnLayout(e);

                // If this is the first time Layout has been raised since loading, this control is now its final size.
                // It is now okay to auto-size the columns or plugin.
                if (IsLoaded && !_initialLayoutComplete)
                {
                    _initialLayoutComplete = true;

                    AutoSizeColumns();

                    if (QueryAndResultsType == QueryAndResultsType.Plugin)
                    {
                        // Plugins with binding sources will have been loaded already (when
                        // UsingPluginBindingSource, load is necessary to initialize
                        // _resultsGrid.DataSource).
                        if (!UsingPluginBindingSource)
                        {
                            _plugin.LoadPlugin(this, _connection);
                        }

                        // SelectionChanged will not have been registered by the plugin until
                        // _plugin.LoadPlugin is called, so fire a selection event now so that the
                        // plugin registers the initial selection.
                        OnSelectionChanged();
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI34844");
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

                // Query execution is allowed because the user explicitly requested it.
                _allowQueryExcecution = true;
                RefreshData(true, true);

                _queryChanged = false;
                _resultsChanged = false;
                UpdateResultsStatus(true);
            }
            catch (Exception ex)
            {
                _resultsTable.Clear();
                _queryError = true;
                _queryChanged = false;
                UpdateResultsStatus(false);

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
                RefreshData(true, false);
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
                OnDataChanged(false, false);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI34633");
            }
        }

        /// <summary>
        /// Handles the <see cref="DataTable.TableNewRow"/> event of <see cref="_resultsTable"/>.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Data.DataTableNewRowEventArgs"/> instance
        /// containing the event data.</param>
        void HandleTableNewRow(object sender, DataTableNewRowEventArgs e)
        {
            try
            {
                // [DotNetRCAndUtils:837]
                // Manually validate the correct auto-increment values with the DB to prevent these
                // values from getting out-of-sync with the database.
                SetAutoIncrementValues(e.Row);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI36044");
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
                // In order that the column data be validated and committed right away after
                // changing a cell value, call UpdateRow.
                UpdateRow(e.Row);
            }
            catch (Exception ex)
            {
                // Indicate that the table data is not valid.
                DataIsValid = false;

                // Flag the offending row.
                e.Row.RowError = ex.Message;
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
        /// Handles the <see cref="DataGridView.UserDeletingRow"/> event of the
        /// <see cref="_resultsGrid"/> control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.DataGridViewRowCancelEventArgs"/>
        /// instance containing the event data.</param>
        void HandleResultsGridUserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
        {
            try
            {
                // [DotNetRCAndUtils:837]
                // When the last row of a table with and auto-incrementing column is deleted, it can
                // lead to concurrency violation exceptions. I do not have a clear understanding of
                // the problem, but there are many threads to be found regarding this error and
                // various possible causes such as this one:
                // http://social.msdn.microsoft.com/Forums/en-US/winformsdatacontrols/thread/bfdb40a8-0e29-4897-8251-6368abe24516
                // I have been unable to directly solve the problem, but what seems an acceptable
                // alternative is that when the last row of a table is being deleted, to cancel the
                // handling of the event, then manually clear the table data instead.
                if (_resultsTable.Rows.Count == 1)
                {
                    ExtractException.Assert("ELI35343", "Data grid in unexpected state.",
                        _resultsTable.Rows[0] == ((DataRowView)e.Row.DataBoundItem).Row);

                    // The table is being programmatically cleared; no RowsRemoved event handling
                    // should occur.
                    _resultsGrid.RowsRemoved -= HandleResultsGridRowsRemoved;

                    // Cancel the automated handling of the grid row deletion.
                    e.Cancel = true;

                    try
                    {
                        // Manually clear the table's data
                        DBMethods.ExecuteDBQuery(_connection, "DELETE FROM [" + _tableName + "]");
                    }
                    catch (Exception ex)
                    {
                        ExtractException ee = ex.AsExtract("ELI35345");

                        // The inner exception will be likely to contain a much more appropriate
                        // message about the problem deleting the row.
                        if (ee.InnerException != null)
                        {
                            throw ee.InnerException;
                        }
                        else
                        {
                            throw ee;
                        }
                    }

                    OnDataChanged(true, false);

                    // Reload the empty table into the grid.
                    RefreshData(true, true);
                }
                else
                {
                    OnDataChanged(true, false);
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35344");
            }
            finally
            {
                if (e.Cancel)
                {
                    _resultsGrid.RowsRemoved += HandleResultsGridRowsRemoved;
                }
            }
        }

        /// <summary>
        /// Handles the results grid rows removed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.DataGridViewRowsRemovedEventArgs"/>
        /// instance containing the event data.</param>
        void HandleResultsGridRowsRemoved(object sender, DataGridViewRowsRemovedEventArgs e)
        {
            DataRow row = null;

            // [DotNetRCAndUtils:865]
            // Unlike with adds and edits which can be allowed to remain in an invalid state until
            // corrected, deletes must be committed immediately and an error must be displayed if
            // the delete is not valid as there is no way to flag as invalid a row that isn't there.
            try
            {
                // If this is not a table or is not loaded, don't attempt to process deletes that
                // may be triggered by low-level refreshes of the _resultsGrid. Even after IsLoaded
                // is true, in the process of adding this control into the editor, it can cause data
                // refreshes that should be ignored; ActiveControl being set is an indicator that
                // this control is loaded and is being modified by the user.
                if (QueryAndResultsType == QueryAndResultsType.Table &&
                    IsLoaded && ActiveControl != null && _resultsTable.Rows.Count > e.RowIndex)
                {
                    row = _resultsTable.Rows[e.RowIndex];
                    _adapter.Update(new[] { row });

                    // If the deleted row(s) had errors, the table as a whole may now be valid. Check.
                    ValidateTableData();
                }
            }
            catch (Exception ex)
            {
                try
                {
                    // If the deletion failed, the change needs to be undone so that the row
                    // re-appears in the grid. Otherwise the data in the grid will not match the
                    // underlying data.
                    if (row != null)
                    {
                        row.RejectChanges();
                    }

                    ex.ExtractDisplay("ELI34664");
                }
                catch (Exception ex2)
                {
                    // If the deletion could not be undone, the data is now out of sync; we are in
                    // a bad state.
                    var ee = new ExtractException("ELI35315",
                        "Error processing delete; table may not save correctly.", ex2);
                    ee.Display();
                }
            }
        }

        /// <summary>
        /// Handles the <see cref="T:DataGridView.RowError"/> event from <see cref="_resultsGrid"/>.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.DataGridViewDataErrorEventArgs"/>
        /// instance containing the event data.</param>
        void HandleResultsGridDataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            try
            {
                e.ThrowException = false;

                // Don't think it is possible to have a null exception, but in case of one, just
                // ignore the error.
                if (e.Exception == null)
                {
                    return;
                }

                // Apply the exception text to the row corresponding to the error.
                if (e.RowIndex >= 0 && e.RowIndex < _resultsGrid.RowCount)
                {
                    _resultsGrid.Rows[e.RowIndex].ErrorText = e.Exception.Message;
                }

                _activeDataError = e;
                DataIsValid = false;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI34658");
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
                _saveButton.Enabled = IsQueryDirty || !QueryFileExists;

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

        /// <summary>
        /// Handles the <see cref="T:DataGridView.SelectionChanged"/> event for the
        /// <see cref="_resultsGrid"/>.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleResultsGridSelectionChanged(object sender, EventArgs e)
        {
            try
            {
                OnSelectionChanged();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI34827");
            }
        }

        /// <summary>
        /// Handles the<see cref="T:SQLCDBEditorPlugin.DataChanged"/> event for the
        /// <see cref="_plugin"/>.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="Extract.SQLCDBEditor.DataChangedEventArgs"/> instance
        /// containing the event data.</param>
        void HandlePluginDataChanged(object sender, DataChangedEventArgs e)
        {
            try
            {
                // If binding data source is being used, any time data is changed re-check to see
                // if the data is now valid.
                if (!DataIsValid && UsingPluginBindingSource && _plugin.DataIsValid)
                {
                    DataIsValid = true;
                }

                OnDataChanged(e.DataCommitted, e.RefreshSource);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI34828");
            }
        }

        /// <summary>
        /// Handles the <see cref="SQLCDBEditorPlugin.StatusMessageChanged"/> event of the
        /// <see cref="_plugin"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Extract.SQLCDBEditor.StatusMessageChangedEventArgs"/>
        /// instance containing the event data.</param>
        void HandlePluginStatusMessageChanged(object sender, StatusMessageChangedEventArgs e)
        {
            try
            {
                OnStatusMessageChanged(e);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI37597");
            }
        }

        /// <summary>
        /// Handles the <see cref="DataGridView.RowValidating"/> event of the
        /// <see cref="_resultsGrid"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DataGridViewCellCancelEventArgs"/> instance containing
        /// the event data.</param>
        void HandleResultsGrid_RowValidating(object sender, DataGridViewCellCancelEventArgs e)
        {
            try
            {
                ExtractException.Assert("ELI38226","Internal logic error", UsingPluginBindingSource);

                // If there is an active data error for this row, do not allow the user to move on
                // from this row until the error has been corrected.
                if (_activeDataError != null && _activeDataError.RowIndex == e.RowIndex)
                {
                    e.Cancel = true;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI38223");
            }
        }

        /// <summary>
        /// Handles the <see cref="DataGridView.RowsRemoved"/> event of the
        /// <see cref="_resultsGrid"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="DataGridViewRowsRemovedEventArgs"/> instance containing
        /// the event data.</param>
        void HandleResultsGrid_DataBoundRowsRemoved(object sender, DataGridViewRowsRemovedEventArgs e)
        {
            try
            {
                // The DataGridView will automatically delete a new row that has not yet been
                // successfully committed into the database if it has data errors. I spent a great
                // deal of time trying to figure out how to prevent this behavior, but for now I am
                // settling on at least informing the user what has happened.
                if (_activeDataError != null &&
                    e.RowIndex <= _activeDataError.RowIndex &&
                    (e.RowIndex + e.RowCount) > _activeDataError.RowIndex)
                {
                    ExtractException ee = new ExtractException("ELI38224",
                        "Row could not be added because of an error in the data:\r\n" +
                        _activeDataError.Exception.Message, _activeDataError.Exception);
                    ee.Display();
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI38225");
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
        /// Gets a value indicating whether this control is hosting a query which may add, modify or
        /// delete data.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if this control is hosting a query which may add, modify or
        /// delete data.; otherwise, <see langword="false"/>.
        /// </value>
        bool QueryModifiesData
        {
            get
            {
                if (QueryAndResultsType == QueryAndResultsType && _masterQuery != null)
                {
                    return Regex.IsMatch(_masterQuery, @"(\bUPDATE\b)|(\bDELETE\b)|(\bINSERT\b)",
                        RegexOptions.IgnoreCase);
                }
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether this instance is using the <see cref="_plugin"/>'s
        /// <see cref="SQLCDBEditorPlugin.BindingSource"/> rather than _resultsTable as
        /// _resultsGrid.DataSource.
        /// </summary>
        /// <value><see langword="true"/> if this instance is using the plugin's BindingSource;
        /// otherwise, <see langword="false"/>.
        /// </value>
        bool UsingPluginBindingSource
        {
            get
            {
                return _plugin != null && _plugin.ProvidesBindingSource;
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
            if (_adapter != null)
            {
                _adapter.Dispose();
                _adapter = null;
            }

            if (_commandBuilder != null)
            {
                _commandBuilder.Dispose();
                _commandBuilder = null;
            }

            // Setup dataAdapter to get the data
            _adapter = new SqlCeDataAdapter("SELECT * FROM " + tableName, connection);

            // Fill the table with the data from the dataAdapter
            _adapter.Fill(_resultsTable);
            ApplySchema(_resultsTable);
            StoreColumnSizes(_resultsTable);

            // Create a command builder for the adapter that allow edits made in the _resultsGrid
            // to be applied back to the database.
            _commandBuilder = new SqlCeCommandBuilder();
            _commandBuilder.DataAdapter = _adapter;
        }

        /// <summary>
        /// Applies the schema of the specified <see paramref="table"/> to its
        /// <see cref="DataColumn"/>s.
        /// </summary>
        /// <param name="table">The <see cref="DataTable"/>.</param>
        void ApplySchema(DataTable table)
        {
            // Fill the schema for the table for the database
            _adapter.FillSchema(table, SchemaType.Source);

            // The above call does not seem to always set the primary key correctly. If a primary
            // key is not set, set it manually.
            if (table.PrimaryKey.Length == 0)
            {
                string[] primaryKeyColumns = DBMethods.GetQueryResultsAsStringArray (_connection,
                    "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.INDEXES WHERE TABLE_NAME = '"
                    + Name + "' AND PRIMARY_KEY = 1");

                table.PrimaryKey = primaryKeyColumns
                    .Select(columnName => table.Columns[columnName])
                    .ToArray();
            }

            // The constraints on the table (including primary key) will be removed before use.
            // Store which columns are the primary key separately.
            _primaryKeyColumnNames = new List<string>(table.PrimaryKey
                .Select(column => column.ColumnName));

            try
            {
                // Check for auto increment fields and default column values
                foreach (DataColumn c in table.Columns)
                {
                    // Get the information for the current column
                    using (SqlCeCommand sqlcmd = new SqlCeCommand(
                        "SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '" +
                        Name + "' AND COLUMN_NAME = '" + c.ColumnName + "'", _connection))
                    {
                        using (SqlCeResultSet columnsResult =
                            sqlcmd.ExecuteResultSet(ResultSetOptions.Scrollable))
                        {
                            // Get the first record in the result set - should only be one
                            if (columnsResult.ReadFirst())
                            {
                                SetColumnAutoIncrement(c, columnsResult);

                                SetColumnDefaultValue(c, columnsResult);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var ee = new ExtractException("ELI35351", "Error parsing table schema; " +
                    "auto-increment columns or columns with default values may not auto-populate.",
                    ex);
                ee.Display();
            }
        }

        /// <summary>
        /// Stores the MaxLength of all columns in <see paramref="dataTable"/>.
        /// </summary>
        /// <param name="dataTable">The <see cref="DataTable"/>.</param>
        void StoreColumnSizes(DataTable dataTable)
        {
            _columnSizes.Clear();
            for (int i = 0; i < dataTable.Columns.Count; i++)
            {
                _columnSizes[i] = dataTable.Columns[i].MaxLength;
            }
        }

        /// <summary>
        /// Applies any auto-increment settings from <see paramref="columnSchema"/> to
        /// <see paramref="dataColumn"/>.
        /// </summary>
        /// <param name="dataColumn">The <see cref="DataColumn"/> to which auto-increment settings
        /// should be applied.</param>
        /// <param name="columnSchema">The <see cref="SqlCeResultSet"/> containing schema info for
        /// the column.</param>
        static int SetColumnAutoIncrement(DataColumn dataColumn, SqlCeResultSet columnSchema)
        {
            int colPos = -1;
            try
            {
                // If the column is an auto increment column set the seed value to next
                // auto-increment value for the column.
                if (dataColumn.AutoIncrement)
                {
                    // Get the position of the AUTOINC_NEXT field
                    colPos = columnSchema.GetOrdinal("AUTOINC_NEXT");

                    // Set the seed to the value in the AUTOINC_NEXT field
                    dataColumn.AutoIncrementSeed = (long)columnSchema.GetValue(colPos);
                }
            }
            catch (Exception ex)
            {
                var ee = new ExtractException("ELI35352", "Error parsing table schema; " +
                    "column may not auto-increment with correct value.", ex);
                try
                {
                    ee.AddDebugData("Column Name", dataColumn.ColumnName, false);
                    if (colPos >= 0)
                    {
                        ee.AddDebugData("Seed", columnSchema.GetValue(colPos).ToString(), false);
                    }
                }
                catch { }

                ee.Display();
            }
            return colPos;
        }

        /// <summary>
        /// Applies any default value from <see paramref="columnSchema"/> to
        /// <see paramref="dataColumn"/>.
        /// </summary>
        /// <param name="dataColumn">The <see cref="DataColumn"/> to which any default value should
        /// be applied.</param>
        /// <param name="columnSchema">The <see cref="SqlCeResultSet"/> containing schema info for
        /// the column.</param>
        static void SetColumnDefaultValue(DataColumn dataColumn, SqlCeResultSet columnSchema)
        {
            int colPos = -1;
            try
            {
                // Set the default for a column if one is defined
                colPos = columnSchema.GetOrdinal("COLUMN_HASDEFAULT");
                if (columnSchema.GetBoolean(colPos))
                {
                    // Set the default value for the column
                    colPos = columnSchema.GetOrdinal("COLUMN_DEFAULT");
                    dataColumn.DefaultValue = columnSchema.GetValue(colPos);
                }
            }
            catch (Exception ex)
            {
                var ee = new ExtractException("ELI35353", "Error parsing table schema; " +
                    "column default value may not be applied.", ex);
                try
                {
                    ee.AddDebugData("Column Name", dataColumn.ColumnName, false);
                    if (colPos >= 0)
                    {
                        ee.AddDebugData("Default value", columnSchema.GetValue(colPos).ToString(),
                            false);
                    }
                }
                catch { }

                ee.Display();
            }
        }

        /// <summary>
        /// Removes table and column constraints for the <see paramref="dataTable"/> so that a
        /// DataGridView using it has as much freedom as possible to allow invalid data to exist
        /// until a user choses to correct it (needs to save).
        /// </summary>
        /// <param name="dataTable">The data table.</param>
        static void RemoveConstraints(DataTable dataTable)
        {
            dataTable.Constraints.Clear();
            foreach (DataColumn column in dataTable.Columns)
            {
                // Allow the user to enter as much text as they wish; it will be marked invalid if
                // it is too much.
                column.MaxLength = -1;
                column.AllowDBNull = true;
            }
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
                for (int i = 0; i < _resultsGrid.ColumnCount; i++)
                {
                    DataGridViewColumn column = _resultsGrid.Columns[i];

                    // Default fill weight will be 1 with a minimum width of 50 (~ 6 chars)
                    float fillWeight = 1.0F; 
                    int minSize = 50;

                    if (column.ValueType == typeof(bool))
                    {
                        // Bool fields do not need to scale larger with size as the table does and
                        // can have a smaller min width.
                        fillWeight = 0.01F;
                        minSize = 25;
                    }
                    else if (column.ValueType == typeof(string))
                    {
                        int maxLength = -1;

                        if (!UsingPluginBindingSource && _columnSizes.TryGetValue(i, out maxLength))
                        {
                            // For text fields that can be > 10 chars but not unlimited, scale up from
                            // a fill weight of 1.0 logarithmically.
                            // MaxLength 10	 = 1.0
                            // MaxLength 50	 = 2.6
                            // MaxLength 500 = 4.9
                            if (maxLength > 10)
                            {
                                fillWeight = (float)Math.Log((double)maxLength / 3.7);
                            }
                        }

                        // But cap the max fill weight at 7 (MaxLength ~5000)
                        if (fillWeight > 7.0F || maxLength == -1)
                        {
                            fillWeight = 7.0F;
                        }
                    }

                    // Get the head text and measure out the minimum width to display the string
                    // Pad the width by 4 pixels (2 each side)
                    var temp = _resultsGrid.Columns[i].HeaderText;
                    var size = graphics.MeasureString(temp, font);
                    minSize = Math.Max(minSize, (int)(size.Width + 0.5) + 4);
                    column.MinimumWidth = minSize;
                    column.FillWeight = fillWeight;
                    column.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
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
        /// Sets any auto-increment column values by querying the DB directly for the next
        /// auto-increment value. This avoids the auto-increment values from getting out-of-sync
        /// with the database.
        /// </summary>
        /// <param name="row">The <see cref="DataRow"/> for which auto-increment values should be
        /// set.</param>
        void SetAutoIncrementValues(DataRow row)
        {
            // Iterate all auto-increment columns in the DB.
            foreach (DataColumn dataColumn in _resultsTable.Columns
                .OfType<DataColumn>()
                .Where(column => column.AutoIncrement))
            {
                // Query the next auto-increment value for this column from the DB.
                var parameters = new Dictionary<string, string>();
                parameters["@0"] = _tableName;
                parameters["@1"] = dataColumn.ColumnName;
                using (var sqlCommand = DBMethods.CreateDBCommand(_connection,
                    "SELECT AUTOINC_NEXT FROM INFORMATION_SCHEMA.COLUMNS " +
                        "WHERE TABLE_NAME = @0 AND COLUMN_NAME = @1",
                    parameters))
                using (var queryResult = sqlCommand.ExecuteReader())
                {
                    // Get the first record in the result set - should only be one
                    if (queryResult.Read())
                    {
                        var nextValue = Convert.ChangeType(queryResult.GetValue(0),
                            dataColumn.DataType, CultureInfo.InvariantCulture);

                        // If the row's auto-increment value is out of sync with the DB, apply the
                        // correct value to the row.
                        if (!nextValue.Equals(row[dataColumn.Ordinal]))
                        {
                            // Since an auto-increment column is going to be read-only, apply
                            // the new auto-increment value via a separate array variable.
                            object[] rowData = row.ItemArray;
                            rowData[dataColumn.Ordinal] = nextValue;
                            row.ItemArray = rowData;
                        }
                    }
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

            // Parse the overall query text into separate queries delimited by "GO" statements.
            Regex queryParserRegex = new Regex(@"[\r\n]+[\s]*GO[\s]*[\r\n]+");

            string[] queries = queryParserRegex.Split(query);
            _parametersTableLayoutPanel.RowCount = queries.Count();

            // Create a regex to parse out the prompt text for each parameter
            queryParserRegex = new Regex(@"(?<=--)[\s\S]+?(?=[\r\n]|$)");

            // All queries except the last one define parameters.
            for (int i = 0; i < queries.Length - 1; i++)
            {
                _parameterQueries.Add(queries[i]);

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
                        availableValues = DBMethods.GetQueryResultsAsStringArray(
                            connection, queries[i], null, ",");
                    }
                    // Ignore any exceptions. A free-form edit box will be provided to enter the
                    // parameter value.
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
        /// <param name="availableValues">The available values for the parameter. If
        /// <see langword="null"/>, the parameter control will be a <see cref="TextBox"/>, otherwise
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
                comboBox.Sorted = false;
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
            _parameterQueries.Clear();

            CollectionMethods.ClearAndDispose(oldControls);
        }

        /// <summary>
        /// Updates the controls that relate to the status of the currently displayed results.
        /// </summary>
        /// <param name="resultsAreCurrent"><see langword="true"/> if the displayed results are
        /// known to be up-to-date; <see langword="false"/> otherwise.</param>
        void UpdateResultsStatus(bool resultsAreCurrent)
        {
            if (QueryModifiesData && !_queryError && !_queryChanged)
            {
                _executeQueryButton.Enabled = true;
                _resultsStatusLabel.Text = "Query has been executed";

                _resultsStatusLabel.Visible = resultsAreCurrent;
            }
            else if (resultsAreCurrent)
            {
                if (QueryAndResultsType == SQLCDBEditor.QueryAndResultsType.Table && _resultsGrid.ReadOnly)
                {
                    _resultsStatusLabel.Text = "Table is not editable because it lacks a primary key.";
                    _resultsStatusLabel.Visible = true;
                }
                else if (!QueryModifiesData)
                {
                    _executeQueryButton.Enabled = false;
                    _resultsStatusLabel.Visible = false;
                }
            }
            else
            {
                // If the results are not necessarily up-to-date, update the label indicating why.
                if (QueryAndResultsType == QueryAndResultsType.Table)
                {
                    ExtractException.Assert("ELI34602", "Unexpected table state.", !DataIsValid);
                    _resultsStatusLabel.Text = "Invalid has been entered. (click here to view)";
                }
                else if (UsingPluginBindingSource)
                {
                    ExtractException.Assert("ELI38227", "Invalid data.", !DataIsValid);
                    if (_activeDataError != null)
                    {
                        _resultsStatusLabel.Text =
                            "Invalid has been entered: " + _activeDataError.Exception.Message;
                    }
                    else
                    {
                        _resultsStatusLabel.Text = "Invalid has been entered.";
                    }
                }
                else if (_resultsChanged)
                {
                    _resultsStatusLabel.Text = "Query results are out of date";
                }
                else if (_queryChanged)
                {
                    _resultsStatusLabel.Text = "Query has been modified since the last execution";
                }
                else if (_queryError)
                {
                    _resultsStatusLabel.Text = "The query failed to execute.";
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
            Exception rowException = null;

            // Clear any errors already associated with the row and re-check for errors.
            row.ClearErrors();

            if (row.RowState == DataRowState.Detached)
            {
                // If the row is currently detached (i.e., new), errors such as constraint violations
                // will not be caught by calling  _adapter.Update(). Adding it to _resultsTable
                // right away can cause it to be sorted before the user is done entering the entire
                // row... so add it to a temporary copy of the table with a transaction that gets
                // rolled back to test if it would be able to be added to _resultsTable without error.
                using (DataTable tableCopy = row.Table.Copy())
                using (SqlCeTransaction transaction = _connection.BeginTransaction(IsolationLevel.Serializable))
                {
                    if (_adapter.InsertCommand == null)
                    {
                        _adapter.InsertCommand = _commandBuilder.GetInsertCommand();
                    }
                    if (_adapter.UpdateCommand == null)
                    {
                        _adapter.UpdateCommand = _commandBuilder.GetUpdateCommand();
                    }

                    _adapter.InsertCommand.Transaction = transaction;
                    _adapter.UpdateCommand.Transaction = transaction;

                    try
                    {
                        DataRow rowCopy = tableCopy.Rows.Add(row.ItemArray);
                        _adapter.Update(new[] { rowCopy });
                    }
                    catch (Exception ex)
                    {
                        rowException = ex;

                        // An error icon will be displayed to call attention to the problem; no need to
                        // throw or display exception.
                    }
                    finally
                    {
                        transaction.Rollback();
                        _adapter.InsertCommand.Transaction = null;
                        _adapter.UpdateCommand.Transaction = null;
                    }
                }
            }
            else
            {
                // Attempt to commit the row data to the table.
                try
                {
                    row.EndEdit();

                    // Update just the row first so that every row with invalid data gets a validation
                    // error icon and that after correcting the data, the error icon is cleared even if
                    // earlier rows have errors.
                    _adapter.Update(new DataRow[] { row });
                }
                catch (Exception ex)
                {
                    rowException = ex;

                    // An error icon will be displayed to call attention to the problem; no need to
                    // throw or display exception.
                }
            }

            if (rowException == null)
            {
                // Check to see if the table as a whole is now valid.
                ValidateTableData();
            }
            else
            {
                // The row update failed.
                if (!row.HasErrors)
                {
                    row.RowError = rowException.Message;
                }

                DataIsValid = false;
            }

            _resultsGrid.Invalidate();

            OnDataChanged(true, false);
        }

        /// <summary>
        /// Validates the table data and updates the result status controls appropriately.
        /// </summary>
        void ValidateTableData()
        {
            try
            {
                // Then see if the entire table can be successfully updated.
                _adapter.Update(_resultsTable);

                // If the update was successful, all data is now valid.
                DataIsValid = true;
            }
            // If there are any exceptions in this block, there is no need to display them. It
            // just means the table data will remain marked as invalid.
            catch { }
        }

        /// <summary>
        /// Merges the specified <see paramref="rowsToMerge"/> into <see paramref="dataTable"/>.
        /// <para><b>Note</b></para>
        /// If any of <see paramref="rowsToMerge"/> cannot be found in <see paramref="dataTable"/>
        /// using the primary key value, the row will not be merged.
        /// <para><b>Note</b></para>
        /// It seemed <see cref="T:DataTable.Merge"/> should have been able to serve the purpose I
        /// created this method for, but I couldn't get the desired results. Either it would update
        /// all data in the grid from the DB, or it would preserve all data in the grid.
        /// </summary>
        /// <requires><see paramref="dataTable"/> must have a primary key.</requires>
        /// <param name="dataTable">The <see cref="DataTable"/> into which the rows should be merged.
        /// </param>
        /// <param name="rowsToMerge">The rows to merge into <see paramref="dataTable"/>.</param>
        /// <returns><see langword="true"/> if the resulting data in <see paramref="dataTable"/> is
        /// valid; otherwise, <see langword="false"/>.</returns>
        bool MergeRowsIntoTable(DataTable dataTable, IEnumerable<DataRow> rowsToMerge)
        {
            bool dataIsValid = true;

            Dictionary<DataRow, DataRow> rowPairings = new Dictionary<DataRow, DataRow>();

            // Find the corresponding row in dataTable for each rowsToMerge.
            foreach (DataRow rowToMerge in rowsToMerge
                .Where(row => row.RowState != DataRowState.Added))
            {
                // Look up the row in dataTable using the original value of the primary key, not the
                // current version in which the primary key may have changed or may now be invalid.
                object[] primaryKeyValue = _primaryKeyColumnNames
                    .Select(columnName =>
                        rowToMerge[rowToMerge.Table.Columns[columnName], DataRowVersion.Original])
                    .ToArray();
                DataRow destinationRow = dataTable.Rows.Find(primaryKeyValue);

                // If we were un-able to find the existing row to merge into, proceeding with the
                // merge would cause that row to be duplicated. Abort the merge.
                ExtractException.Assert("ELI34661", "Failed to apply changes.",
                    destinationRow != null);

                rowPairings[rowToMerge] = destinationRow;
            }

            // Once we have found corresponding rows in dataTable for all rowsToMerge, clear the
            // constraints to allow the rows to be added even if they violate a constraint.
            RemoveConstraints(dataTable);

            // Apply each rowsToMerge to the dataTable.
            foreach (DataRow row in rowsToMerge)
            {
                DataRow destinationRow;
                if (rowPairings.TryGetValue(row, out destinationRow))
                {
                    // If there is a corresponding row in dataTable, update it's values.
                    for (int j = 0; j < row.ItemArray.Length; j++)
                    {
                        if (!row[j].Equals(destinationRow[j]))
                        {
                            destinationRow[j] = row[j];
                        }
                    }
                }
                else
                {
                    // If there was no corresponding row, add it as a new row.
                    destinationRow = dataTable.Rows.Add(row.ItemArray);
                }

                try
                {
                    // Attempt an update on each merged-in row. If the data is still
                    // invalid, this will throw an exception which we can ignore since
                    // the validation icon on the row will become set.
                    _adapter.Update(new DataRow[] { destinationRow });
                }
                catch
                {
                    dataIsValid = false;
                }
            }

            return dataIsValid;
        }

        /// <summary>
        /// Updates the list of available value for any ComboBox parameter fields to reflect the
        /// latest data in the database.
        /// </summary>
        void RefreshParameterControls()
        {
            for (int i = 0; i < _queryParameterControls.Count(); i++)
            {
                // Don't change the type of parameter control that already exists. Only ComboBoxes
                // will have lists of available values that need to be updated.
                ComboBox comboBox = _queryParameterControls[i] as ComboBox;
                if (comboBox == null)
                {
                    continue;
                }

                // Save the current available value list so it can be compared to the new list.
                string[] originalAvailableValuesList = comboBox.Items
                    .OfType<string>()
                    .ToArray();

                try
                {
                    // Get the up-to-date list.
                    string[] newAvailableValuesList = DBMethods.GetQueryResultsAsStringArray(
                        _connection, _parameterQueries[i], null, ",");

                    // If the list has changed, update the items and auto-complete list of the
                    // ComboBox.
                    if (!Enumerable.SequenceEqual(
                        originalAvailableValuesList, newAvailableValuesList))
                    {
                        comboBox.Items.Clear();
                        comboBox.Items.AddRange(newAvailableValuesList);
                        var autoCompleteList = new AutoCompleteStringCollection();
                        autoCompleteList.AddRange(newAvailableValuesList.Distinct().ToArray());
                        comboBox.AutoCompleteCustomSource = autoCompleteList;
                    }
                }
                // Ignore any exceptions and just use the previous list of available values.
                catch {}
            }
        }

        /// <summary>
        /// Raises the <see cref="DataChanged"/> event.
        /// </summary>
        /// <param name="dataCommitted"><see langword="true"/> if the changed data was committed;
        /// <see langword="false"/> if the change is in progress.</param>
        /// <param name="refreshSource"><see langword="true"/> if the
        /// <see cref="QueryAndResultsControl"/> that raised the event should be refreshed as well;
        /// otherwise, <see langword="false"/>.</param>
        void OnDataChanged(bool dataCommitted, bool refreshSource)
        {
            var eventHandler = DataChanged;
            if (eventHandler != null)
            {
                eventHandler(this, new DataChangedEventArgs(dataCommitted, refreshSource));
            }
        }

        /// <summary>
        /// Raises the <see cref="StatusMessageChanged"/> event.
        /// </summary>
        /// <param name="eventArgs">The <see cref="StatusMessageChangedEventArgs"/> instance
        /// containing the event data.</param>
        void OnStatusMessageChanged(StatusMessageChangedEventArgs eventArgs)
        {
            var eventHandler = StatusMessageChanged;
            if (eventHandler != null)
            {
                eventHandler(this, eventArgs);
            }
        }

        /// <summary>
        /// Raises the <see cref="SentToSeparateTab"/> event.
        /// </summary>
        void OnSentToSeparateTab()
        {
            var eventHandler = SentToSeparateTab;
            if (eventHandler != null)
            {
                eventHandler(this, new EventArgs());
            }
        }

        /// <summary>
        /// Raises the <see cref="PropertyChanged"/> event.
        /// </summary>
        /// <param name="propertyName">The name of the property that has changed.</param>
        void OnPropertyChanged(string propertyName)
        {
            var eventHandler = PropertyChanged;
            if (eventHandler != null)
            {
                eventHandler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        /// <summary>
        /// Raises the <see cref="QueryCreated"/> event.
        /// </summary>
        /// <param name="queryAndResultsControl">The <see cref="QueryAndResultsControl"/>
        /// that has been created.</param>
        void OnQueryCreated(QueryAndResultsControl queryAndResultsControl)
        {
            var eventHandler = QueryCreated;
            if (eventHandler != null)
            {
                eventHandler(this, new QueryCreatedEventArgs(queryAndResultsControl));
            }
        }

        /// <summary>
        /// Raises the <see cref="QueryRenaming"/> event.
        /// </summary>
        /// <param name="eventArgs">The <see cref="Extract.SQLCDBEditor.QueryRenamingEventArgs"/>
        /// instance containing the event data.</param>
        void OnQueryRenaming(QueryRenamingEventArgs eventArgs)
        {
            var eventHandler = QueryRenaming;
            if (eventHandler != null)
            {
                eventHandler(this, eventArgs);
            }
        }

        /// <summary>
        /// Raises the <see cref="QuerySaved"/> event.
        /// </summary>
        void OnQuerySaved()
        {
            var eventHandler = QuerySaved;
            if (eventHandler != null)
            {
                eventHandler(this, new EventArgs());
            }
        }

        /// <summary>
        /// Raises the <see cref="SelectionChanged"/> event.
        /// </summary>
        void OnSelectionChanged()
        {
            var eventHandler = SelectionChanged;
            if (eventHandler != null)
            {
                eventHandler(this,
                    new GridSelectionEventArgs(_resultsGrid.SelectedRows
                        .OfType<DataGridViewRow>()
                        .Select(gridRow => gridRow.DataBoundItem)
                        .OfType<DataRowView>()
                        .Select(dataRow => dataRow.Row)
                        .OfType<DataRow>()));
            }
        }

        #endregion Private Methods
    }
}
