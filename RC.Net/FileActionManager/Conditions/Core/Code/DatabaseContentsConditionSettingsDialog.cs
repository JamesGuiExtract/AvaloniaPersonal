using Extract.Database;
using Extract.FileActionManager.Forms;
using Extract.Licensing;
using Extract.SqlDatabase;
using Extract.Utilities;
using Extract.Utilities.Forms;
using Microsoft.Data.ConnectionUI;
using System;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;

namespace Extract.FileActionManager.Conditions
{
    /// <summary>
    /// A <see cref="Form"/> that allows for configuration of an <see cref="DatabaseContentsCondition"/>
    /// instance.
    /// </summary>
    public partial class DatabaseContentsConditionSettingsDialog : Form
    {
        #region Constants

        /// <summary>
        /// The object name.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(DatabaseContentsConditionSettingsDialog).ToString();

        /// <summary>
        /// The number of seconds to allow for a DB connection to be established or a query to be
        /// executed for purposes of update UI auto-complete based on schema info from the specified
        /// database table or query.
        /// </summary>
        static readonly int _SCHEMA_UPDATE_TIMEOUT = 2;

        /// <summary>
        /// A <see cref="Regex"/> instance that identifies an existing "top" clause in an SQL query
        /// or, if not present, the location where the top clause should go.
        /// </summary>
        static readonly Regex _topClauseRegex =
            new Regex(@"(?<=^\s*SELECT(\s+DISTINCT)?)\s+(TOP\s+\(?\d+\)?\s+)?",
                RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.RightToLeft);

        /// <summary>
        /// A <see cref="Regex"/> instance identifying the "where" clause of an SQL query.
        /// </summary>
        static readonly Regex _whereClauseRegex = new Regex(@"\s+WHERE\s[\s\S]+",
            RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.RightToLeft);

        #endregion Constants

        #region Private classes

        /// <summary>
        /// Represents the <see cref="EventArgs"/> to be passed to a background thread tasked with
        /// updating the schema info of the configured database table or query.
        /// </summary>
        class SchemaInfoWorkerArgs
        {

            /// <summary>
            /// Initializes a new instance of the <see cref="SchemaInfoWorkerArgs"/> class.
            /// <para><b>Note</b></para>
            /// The arguments <see paramref="tableName"/> or <see paramref="query"/> cannot both be
            /// specified (non-<see langword="null"/>) at the same time.
            /// </summary>
            /// <param name="connectionInfo">A <see cref="DatabaseConnectionInfo"/> instance
            /// representing the database to use.</param>
            /// <param name="resetConnection"><see langword="true"/> if an existing
            /// <see cref="DbConnection"/> should be reset because <see cref="ConnectionInfo"/> may
            /// have changed since the last call to update the schema.</param>
            /// <param name="tableName">The name of the table for which field names are needed.</param>
            /// <param name="query">The query for which field names are needed.</param>
            public SchemaInfoWorkerArgs(DatabaseConnectionInfo connectionInfo, bool resetConnection,
                string tableName, string query)
            {
                ExtractException.Assert("ELI36959", "Invalid schema update arguments.",
                    string.IsNullOrEmpty(tableName) || string.IsNullOrEmpty(query));

                ConnectionInfo = connectionInfo;
                ResetConnection = resetConnection;
                TableName = tableName;
                Query = query;
            }

            /// <summary>
            /// Gets a <see cref="DatabaseConnectionInfo"/> instance representing the database to
            /// use.
            /// </summary>
            public DatabaseConnectionInfo ConnectionInfo
            {
                get;
                private set;
            }

            /// <summary>
            /// Gets whether an existing <see cref="DbConnection"/> should be reset because
            /// <see cref="ConnectionInfo"/> may have changed since the last call to update the
            /// schema.
            /// </summary>
            public bool ResetConnection
            {
                get;
                private set;
            }

            /// <summary>
            /// Gets the name of the table for which field names are needed. Must be
            /// <see langword="null"/> if <see cref="Query"/> is not <see langword="null"/>.
            /// </summary>
            public string TableName
            {
                get;
                private set;
            }

            /// <summary>
            /// Gets the query for which field names are needed. Must be <see langword="null"/> if
            /// <see cref="TableName"/> is not <see langword="null"/>.
            /// </summary>
            public string Query
            {
                get;
                private set;
            }
        }

        /// <summary>
        /// Represents the results of a background thread operation to obtain schema info of the
        /// configured database table or query.
        /// </summary>
        class SchemaInfoWorkerResult
        {
            /// <summary>
            /// The names of the tables in the configured database.
            /// </summary>
            public string[] TableNames = new string[0];

            /// <summary>
            /// The names of the fields in the configured table or returned by the specified query.
            /// </summary>
            public string[] FieldNames = new string[0];
        }

        #endregion Private classes

        #region Fields

        /// <summary>
        /// The default style to use for text cells.
        /// </summary>
        DataGridViewCellStyle _defaultTextCellStyle;

        /// <summary>
        /// The style to use for text cells when the table is disabled.
        /// </summary>
        DataGridViewCellStyle _disabledTextCellStyle;

        /// <summary>
        /// The default style to use for check box cells.
        /// </summary>
        DataGridViewCellStyle _defaultCheckBoxCellStyle;

        /// <summary>
        /// The style to use for checkbox cells when the table is disabled.
        /// </summary>
        DataGridViewCellStyle _disabledCheckBoxCellStyle;

        /// <summary>
        /// A <see cref="BackgroundWorker"/> to use to obtain schema info from the configured
        /// database and table/query.
        /// </summary>
        BackgroundWorker _schemaInfoBackgroundWorker = new BackgroundWorker();

        /// <summary>
        /// Indicates whether any previously started schema info update operations are complete.
        /// </summary>
        ManualResetEvent _schemaInfoUpdateComplete = new ManualResetEvent(true);

        /// <summary>
        /// The <see cref="DbConnection"/> being used to obtain schema info.
        /// </summary>
        DbConnection _schemaInfoDbConnection;

        /// <summary>
        /// Used to create and open <see cref="_schemaInfoDbConnection"/> while managing local
        /// SQL CE database copies.
        /// </summary>
        DatabaseConnectionInfo _schemaInfoDbConnectionInfo;

        /// <summary>
        /// Indicates that either the selected database table has changed or the query has changed
        /// so that previously obtained field names may no longer be correct.
        /// </summary>
        bool _queryOrTableHasChanged;

        /// <summary>
        /// If non-<see langword="null"/> indicates that a new schema update operation should be
        /// started as soon as the current one is complete; 
        /// </summary>
        bool? _schemaUpdateResetConnection;

        /// <summary>
        /// The field names that were able to be loaded by the background schema info update or
        /// <see param="null"/> if fields names have not been able to be loaded.
        /// </summary>
        string[] _fieldNames;

        /// <summary>
        /// The selection in the last <see cref="_fieldsDataGridView"/> row when edit mode was ended
        /// in the value column.
        /// </summary>
        Tuple<int, int> _fieldValueControlTextSelection;

        /// <summary>
        /// Indicates whether this form has been loaded.
        /// </summary>
        bool _loaded;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes static data for the <see cref="DatabaseContentsConditionSettingsDialog"/>
        /// class.
        /// </summary>
        // FXCop seems to believe this is here to initialize static fields. That is not the case.
        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static DatabaseContentsConditionSettingsDialog()
        {
            try
            {
                DatabaseContentsConditionSearchModifier.All.SetReadableValue("all");
                DatabaseContentsConditionSearchModifier.Any.SetReadableValue("any");
                DatabaseContentsConditionSearchModifier.None.SetReadableValue("none");

                DatabaseContentsConditionRowCount.Zero.SetReadableValue("zero rows");
                DatabaseContentsConditionRowCount.ExactlyOne.SetReadableValue("exactly one row");
                DatabaseContentsConditionRowCount.AtLeastOne.SetReadableValue("at least one row");
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI36960");
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseContentsConditionSettingsDialog"/>
        /// class.
        /// </summary>
        /// <param name="settings">The <see cref="DatabaseContentsCondition"/> instance to be
        /// configured.</param>
        public DatabaseContentsConditionSettingsDialog(DatabaseContentsCondition settings)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI36961",
                    _OBJECT_NAME);

                Settings = settings;

                InitializeComponent();

                // Initialize styles that allow the table to appear disabled when it is disabled.
                InitializeCellStyles();

                var pathTags = new FileActionManagerPathTags();
                pathTags.AlwaysShowDatabaseTags = true;
                _databaseConnectionControl.PathTags = pathTags;
                _queryPathTagsButton.PathTags = pathTags;
                _fieldsPathTagsButton.PathTags = pathTags;

                // Register for focus change notifications so that _fieldValueControlTextSelection
                // can be cleared when appropriate.
                foreach (Control control in this.GetAllControls())
                {
                    control.Enter += new EventHandler(HandleControl_Enter);
                }

                // Initialize background worker that will obtain schema info as settings are
                // configured/modified.
                _schemaInfoBackgroundWorker.WorkerSupportsCancellation = true;
                _schemaInfoBackgroundWorker.DoWork += HandleSchemaInfoBackgroundWorker_DoWork;
                _schemaInfoBackgroundWorker.RunWorkerCompleted +=
                    HandleSchemaInfoBackgroundWorker_RunWorkerCompleted;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI36962");
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the <see cref="DatabaseContentsCondition"/> instance to configure.
        /// </summary>
        /// <value>
        /// The <see cref="DatabaseContentsCondition"/> instance to configure.
        /// </value>
        public DatabaseContentsCondition Settings
        { 
            get;
            set;
        }

        #endregion Properties

        #region Overrides

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Form.Load"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.
        /// </param>
        protected override void OnLoad(EventArgs e)
        {
            try
            {
                base.OnLoad(e);

                // Load the combo boxes with the readable values of their associates enums.
                _searchModifierComboBox.InitializeWithReadableEnum<DatabaseContentsConditionSearchModifier>(false);
                _checkFieldsRowCountComboBox.InitializeWithReadableEnum<DatabaseContentsConditionRowCount>(false);
                _doNotCheckFieldsRowCountComboBox.InitializeWithReadableEnum<DatabaseContentsConditionRowCount>(false);

                // Apply Settings values to the UI.
                if (Settings != null)
                {
                    _useFAMDbRadioButton.Checked = Settings.UseFAMDBConnection;
                    _specifiedDbRadioButton.Checked = !Settings.UseFAMDBConnection;
                    _databaseConnectionControl.DatabaseConnectionInfo =
                        new DatabaseConnectionInfo(Settings.DatabaseConnectionInfo);
                    _tableOrQueryComboBox.SelectedIndex = Settings.UseQuery ? 1 : 0;
                    _tableComboBox.Text = Settings.Table;
                    _queryTextBox.Text = Settings.Query;
                    _doNotCheckFieldsRadioButton.Checked = !Settings.CheckFields;
                    _checkFieldsRadioButton.Checked = Settings.CheckFields;
                    _doNotCheckFieldsRowCountComboBox.SelectEnumValue(Settings.RowCountCondition);
                    _checkFieldsRowCountComboBox.SelectEnumValue(Settings.RowCountCondition);
                    _searchModifierComboBox.SelectEnumValue(Settings.SearchModifier);
                    foreach (IVariantVector fieldVector in
                        Settings.SearchFields.ToIEnumerable<IVariantVector>())
                    {
                        _fieldsDataGridView.Rows.Add(fieldVector.ToIEnumerable<object>().ToArray());
                    }
                }

                _loaded = true;

                UpdateDbSchemaInfo(true);
                UpdateUI();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI36963");
            }
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.
        /// </param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                    components = null;
                }

                if (_schemaInfoBackgroundWorker != null)
                {
                    _schemaInfoBackgroundWorker.Dispose();
                    _schemaInfoBackgroundWorker = null;
                }

                if (_schemaInfoUpdateComplete != null)
                {
                    _schemaInfoUpdateComplete.Dispose();
                    _schemaInfoUpdateComplete = null;
                }

                if (_schemaInfoDbConnectionInfo != null)
                {
                    _schemaInfoDbConnectionInfo.Dispose();
                    _schemaInfoDbConnectionInfo = null;
                }

                _schemaInfoDbConnection?.Dispose();
                _schemaInfoDbConnection = null;
            }
            base.Dispose(disposing);
        }

        #endregion Overrides

        #region Event Handlers

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event of the <see cref="_advancedButton"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleAdvancedButtonClick(object sender, EventArgs e)
        {
            try
            {
                using (var dialog = new DatabaseContentsConditionAdvancedDialog(Settings))
                {
                    dialog.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI36964");
            }
        }

        /// <summary>
        /// In the case that the <see cref="Control.Click"/> event of the <see cref="_okButton"/>.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleOkButtonClick(object sender, EventArgs e)
        {
            try
            {
                bool schemaAvailable = _schemaInfoUpdateComplete.WaitOne(0);

                if (schemaAvailable)
                {
                    ApplySettings(true);
                }
                else
                {
                    // If schema info is in the process of being updated, wait for the update to
                    // complete so that we can use it for validation of the settings. This wait must
                    // happen on another thread which invokes ApplySettings on this UI thread when
                    // complete because _schemaInfoUpdateComplete is set in the BackgroundWorker
                    // RunWorkerCompleted event handler which also needs to run in the UI thread
                    // (and thus would be blocked if we waited on this thread).
                    ThreadingMethods.RunInBackgroundThread("ELI36998", () =>
                    {
                        // Convert seconds to milliseconds for the wait
                        bool updateCompleted =
                            _schemaInfoUpdateComplete.WaitOne(_SCHEMA_UPDATE_TIMEOUT * 1000);
                        this.SafeBeginInvoke("ELI36999", () => ApplySettings(updateCompleted));
                    });

                    // Disable the UI while waiting for the schema info update to complete so that
                    // the user doesn't change anything or click OK again before ApplySettings gets
                    // invoked.
                    Enabled = false;
                    return;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI36997");
            }
        }

        /// <summary>
        /// Handles the <see cref="ComboBox.SelectedIndexChanged"/> event of the
        /// <see cref="_tableOrQueryComboBox"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleTableOrQueryComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                UpdateDbSchemaInfo(false);
                UpdateUI();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI36966");
            }
        }

        /// <summary>
        /// Handles the <see cref="ComboBox.SelectedIndexChanged"/> event of the
        /// <see cref="_tableComboBox"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleTableComboBox_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            try
            {
                UpdateDbSchemaInfo(false);
                UpdateUI();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI36967");
            }
        }

        /// <summary>
        /// Handles the <see cref="RadioButton.CheckedChanged"/> event of the
        /// <see cref="_checkFieldsRadioButton"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleCheckFieldsRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                UpdateUI();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI36968");
            }
        }

        /// <summary>
        /// Handles the <see cref="RadioButton.CheckedChanged"/> event of the
        /// <see cref="_specifiedDbRadioButton"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleSpecifiedDbRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                UpdateDbSchemaInfo(true);
                UpdateUI();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI36969");
            }
        }

        /// <summary>
        /// Handles the <see cref="DatabaseConnectionControl.ConnectionChanged"/> event of the
        /// <see cref="_databaseConnectionControl"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleDatabaseConnectionControl_ConnectionChanged(object sender, EventArgs e)
        {
            try
            {
                UpdateDbSchemaInfo(true);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI36970");
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.TextChanged"/> event of the <see cref="_queryTextBox"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleQueryOrTable_TextChanged(object sender, EventArgs e)
        {
            try
            {
                // Don't update the schema as the user is typing, but keep of the fact that it has
                // changed so that as soon as the query or table box loses focus, the schema can be
                // updated.
                _queryOrTableHasChanged = true;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI36971");
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.Leave"/> event of the <see cref="_queryTextBox"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleQueryOrTable_Leave(object sender, EventArgs e)
        {
            try
            {
                // If the table/query box has lost focus and had been edited while it had focus,
                // trigger a schema update for the new table/query.
                if (_queryOrTableHasChanged)
                {
                    _queryOrTableHasChanged = false;
                    UpdateDbSchemaInfo(false);
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI36972");
            }
        }

        /// <summary>
        /// Handles the <see cref="DataGridView.EditingControlShowing"/> event of the
        /// <see cref="_fieldsDataGridView"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.DataGridViewEditingControlShowingEventArgs"/>
        /// instance containing the event data.</param>
        void HandleFieldsDataGridView_EditingControlShowing(object sender, 
            DataGridViewEditingControlShowingEventArgs e)
        {
            try
            {
                // When the editing control is displayed for the field name column, initialize
                // auto-complete using _fieldNames.
                var editingControl = e.Control as DataGridViewTextBoxEditingControl;
                if (editingControl != null)
                {
                    if (_fieldsDataGridView.CurrentCell.ColumnIndex != 0 || 
                        _fieldNames == null || _fieldNames.Length == 0)
                    {
                        editingControl.AutoCompleteMode = AutoCompleteMode.None;
                    }
                    else
                    {
                        editingControl.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                        editingControl.AutoCompleteSource = AutoCompleteSource.CustomSource;
                        editingControl.AutoCompleteCustomSource = new AutoCompleteStringCollection();
                        // Add an extra copy of every value with a pre-pended space char so that
                        // pressing space will bring up a list with all available field names.
                        editingControl.AutoCompleteCustomSource.AddRange(
                            _fieldNames
                                .Select(name => " " + name)
                                .Union(_fieldNames)
                                .ToArray());
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI36973");
            }
        }

        /// <summary>
        /// Handles the <see cref="DataGridView.CellEndEdit"/> event of the
        /// <see cref="_fieldsDataGridView"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.DataGridViewCellEventArgs"/>
        /// instance containing the event data.</param>
        void HandleFieldsDataGridView_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                // Remove pre-pended spaces added to allow the space bar to bring up the full
                // auto-complete list.
                if (e.ColumnIndex == 0 && _fieldNames != null)
                {
                    DataGridViewCell cell = _fieldsDataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex];
                    if (cell.Value != null)
                    {
                        cell.Value = (cell.Value.ToString()).Trim();
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI36974");
            }
        }

        /// <summary>
        /// Handles the <see cref="BackgroundWorker.DoWork"/> event of the
        /// <see cref="_schemaInfoBackgroundWorker"/> to do work on the background thread.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.ComponentModel.DoWorkEventArgs"/> instance
        /// containing the event data.</param>
        void HandleSchemaInfoBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            var result = new SchemaInfoWorkerResult();
            e.Result = result;

            try
            {
                if (worker.CancellationPending)
                {
                    return;
                }

                var arguments = (SchemaInfoWorkerArgs)e.Argument;

                // Path tags may not be able to properly expand any path tags in the connection
                // string with a lot of the info not available at this point, but give it a shot.
                IFileProcessingDB fileProcessingDB = new FileProcessingDB();
                var pathTags = new FileActionManagerPathTags();
                pathTags.AlwaysShowDatabaseTags = true;

                DbConnection lastConnection = _schemaInfoDbConnection;
                DbConnection dbConnection = GetDbConnectionForSchemaUpdater(
                    arguments.ResetConnection, arguments.ConnectionInfo, fileProcessingDB, pathTags);

                if (worker.CancellationPending)
                {
                    return;
                }

                // Setting table names to null indicates that the current table list should be left
                // alone. Only update the table list if the connection has changed.
                result.TableNames = (lastConnection != dbConnection) 
                    ? GetTableNames(dbConnection) 
                    : null;

                if (worker.CancellationPending)
                {
                    return;
                }

                result.FieldNames = GetResultFields(dbConnection, fileProcessingDB, pathTags,
                    arguments.TableName, arguments.Query);
            }
            // If unable to connect to the DB or parse the query to obtain schema info, just ignore
            // the error and return nothing. It may not be possible to parse the data at config time
            // due to path tags/functions queries that can only be evaluated at run time.
            catch { }
        }

        /// <summary>
        /// Handles the <see cref="BackgroundWorker.RunWorkerCompleted"/> event of the
        /// <see cref="_schemaInfoBackgroundWorker"/> to apply results of the operation in the UI
        /// thread.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.ComponentModel.RunWorkerCompletedEventArgs"/>
        /// instance containing the event data.</param>
        void HandleSchemaInfoBackgroundWorker_RunWorkerCompleted(object sender,
            RunWorkerCompletedEventArgs e)
        {
            try
            {
                if (_schemaUpdateResetConnection.HasValue)
                {
                    // If _schemaUpdateResetConnection is not null, another schema update operation
                    // was requested while this one was still running. Ignore the results of the
                    // last operation and kick off another.
                    UpdateDbSchemaInfo(_schemaUpdateResetConnection.Value);
                }
                else
                {
                    // There are no pending update operations. Apply the results of the operation as
                    // long as the update didn't fail. The table name and field list will have
                    // already been cleared when the operation was started. Populate the results
                    // only if the operation was successful.
                    if (!e.Cancelled && e.Error == null)
                    {
                        var result = (SchemaInfoWorkerResult)e.Result;

                        if (result.TableNames != null)
                        {
                            UpdateTableNames(result.TableNames);
                        }
                        UpdateFieldNames(result.FieldNames);
                    }

                    // In the case that there was a timeout in WarnIfInvalid waiting on this event
                    // it may already be disposed of. Check for null before setting the event.
                    if (_schemaInfoUpdateComplete != null)
                    {
                        _schemaInfoUpdateComplete.Set();
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI36975");
            }
        }

        /// <summary>
        /// Handles the <see cref="DataGridView.CurrentCellChanged"/> event of the
        /// <see cref="_fieldsDataGridView"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleFieldsDataGridView_CurrentCellChanged(object sender, EventArgs e)
        {
            try
            {
                _fieldValueControlTextSelection = null;
                UpdateUI();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI36991");
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.Enter"/> event of any control in this form.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleControl_Enter(object sender, EventArgs e)
        {
            try
            {
                // Don't persist selection for use by _fieldsPathTagsButton after any control other
                // that _fieldsPathTagsButton is activated.
                if (sender != _fieldsPathTagsButton)
                {
                    _fieldValueControlTextSelection = null;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI36993");
            }
        }

        /// <summary>
        /// Handles the <see cref="DataGridView.CellValidating"/> event of the
        /// <see cref="_fieldsDataGridView"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.DataGridViewCellValidatingEventArgs"/>
        /// instance containing the event data.</param>
        void HandleFieldsDataGridView_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            try
            {
                // Use of the path tags button will cause edit mode to end and the editing control
                // to close. So that tags/functions can be inserted using the current selection,
                // store the selection as the editing control closes as long as the column being
                // edited is the value column.
                if (e.ColumnIndex != 1)
                {
                    _fieldValueControlTextSelection = null;
                    return;
                }

                var editingControl = _fieldsDataGridView.EditingControl as DataGridViewTextBoxEditingControl;
                if (editingControl != null)
                {
                    _fieldValueControlTextSelection = new Tuple<int, int>(
                        editingControl.SelectionStart, editingControl.SelectionLength);
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI36990");
            }
        }

        /// <summary>
        /// Handles the <see cref="PathTagsButton.TagSelecting "/>event of the
        /// <see cref="_fieldsPathTagsButton"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Extract.Utilities.Forms.TagSelectingEventArgs"/> instance
        /// containing the event data.</param>
        void HandleFieldsPathTagsButton_TagSelecting(object sender, TagSelectingEventArgs e)
        {
            try
            {
                // Use of the path tags button will have caused edit mode in the data grid view to
                // have ended. Re-enter edit mode and restore the last known selection before
                // applying the tag selection.
                _fieldsDataGridView.BeginEdit(true);

                var editingControl = _fieldsDataGridView.EditingControl as DataGridViewTextBoxEditingControl;
                if (editingControl != null)
                {
                    _fieldsPathTagsButton.TextControl = editingControl;
                    if (_fieldValueControlTextSelection != null)
                    {
                        editingControl.Select(
                            _fieldValueControlTextSelection.Item1, _fieldValueControlTextSelection.Item2);
                    }
                }
                else
                {
                    e.Cancel = true;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI36995");
            }
        }

        #endregion Event Handlers

        #region Private Members

        /// <summary>
        /// Initializes styles that allow <see cref="_fieldsDataGridView"/> to appear disabled when
        /// it is disabled.
        /// </summary>
        void InitializeCellStyles()
        {
            _defaultTextCellStyle = _fieldsDataGridView.DefaultCellStyle;
            _defaultCheckBoxCellStyle = _fieldsDataGridView.Columns["_fuzzyColumn"].DefaultCellStyle;

            // The style to use for table text cells when the table is disabled.
            _disabledTextCellStyle = new DataGridViewCellStyle(_defaultTextCellStyle);
            _disabledTextCellStyle.ForeColor = SystemColors.GrayText;
            _disabledTextCellStyle.BackColor = SystemColors.Control;

            // The style to use for table check box cells when the table is disabled.
            _disabledCheckBoxCellStyle = new DataGridViewCellStyle(_defaultCheckBoxCellStyle);
            _disabledCheckBoxCellStyle.ForeColor = SystemColors.GrayText;
            _disabledCheckBoxCellStyle.BackColor = SystemColors.Control;
        }

        /// <summary>
        /// Updates the status of various UI controls based on the currently selected options.
        /// </summary>
        void UpdateUI()
        {
            bool useQuery = _tableOrQueryComboBox.SelectedIndex == 1;
            _tableComboBox.Visible = !useQuery;
            _queryTextBox.Enabled = useQuery;
            _queryPathTagsButton.Enabled = useQuery;
            _checkFieldsRadioButton.Text = useQuery ? "Returns" : "Contains";
            _doNotCheckFieldsRadioButton.Text = useQuery ? "Returns" : "Contains";

            bool useFields = _checkFieldsRadioButton.Checked;
            _doNotCheckFieldsRowCountComboBox.Enabled = !useFields;
            _checkFieldsRowCountComboBox.Enabled = useFields;
            _searchModifierComboBox.Enabled = useFields;
            _fieldsDataGridView.Enabled = useFields;
            _fieldsPathTagsButton.Enabled = useFields;

            _databaseConnectionControl.Enabled = _specifiedDbRadioButton.Checked;

            // Update the style of cells in _fieldsDataGridView so that when disabled it appears
            // disabled.
            foreach (DataGridViewRow row in _fieldsDataGridView.Rows)
            {
                foreach (DataGridViewCell cell in row.Cells)
                {
                    if (_fieldsDataGridView.Columns[cell.ColumnIndex] is DataGridViewCheckBoxColumn)
                    {
                        cell.Style = _fieldsDataGridView.Enabled
                            ? _defaultCheckBoxCellStyle
                            : _disabledCheckBoxCellStyle;
                    }
                    else
                    {
                        cell.Style = _fieldsDataGridView.Enabled
                            ? _defaultTextCellStyle
                            : _disabledTextCellStyle;
                    }
                }
            }

            // The _fieldsDataGridView doesn't appear disabled if it has an active cell.
            if (!_fieldsDataGridView.Enabled && _fieldsDataGridView.CurrentCell != null)
            {
                // Invoke to avoid an exception about re-entry into the selected cell event handler.
                // There is no infinite loop risk here because we are checking for null before
                // setting to null.
                this.SafeBeginInvoke("ELI36992", () => _fieldsDataGridView.CurrentCell = null);
            }

            _fieldsPathTagsButton.Enabled =
                _fieldsDataGridView.CurrentCell != null &&
                _fieldsDataGridView.CurrentCell.ColumnIndex == 1;
        }

        /// <summary>
        /// Updates the known table and field names based on the currently configured settings.
        /// </summary>
        /// <param name="resetConnection"><see langword="true"/> if database connection settings
        /// have changed since the last UpdateDbSchema call and should be re-initialized; otherwise,
        /// <see langword="false"/>.</param>
        void UpdateDbSchemaInfo(bool resetConnection)
        {
            // Don't allow any updates to be triggered as the controls are being initialized during
            // load or after the form has been disposed.
            if (_loaded && _schemaInfoUpdateComplete != null)
            {
                // If a schema update is already in progress, cancel the existing update and
                // schedule another to run as soon as the current one is canceled.
                if (_schemaInfoBackgroundWorker.IsBusy)
                {
                    _schemaUpdateResetConnection = (_schemaUpdateResetConnection.HasValue)
                        ? _schemaUpdateResetConnection.Value | resetConnection
                        : resetConnection;
                    _schemaInfoBackgroundWorker.CancelAsync();
                }
                else
                {
                    // Indicate that an update is in progress and reset the flag that would triggers
                    // another update on completion.
                    _schemaInfoUpdateComplete.Reset();
                    _schemaUpdateResetConnection = null;

                    if (resetConnection)
                    {
                        // Before kicking off the background process, clear the loaded table names if
                        // the list of table names may possibly have changed.
                        UpdateTableNames(null);
                    }
                    // The field names should be cleared before the background process is started
                    // because they are subject to change in any call to UpdateDbSchema.
                    UpdateFieldNames(null);

                    // Use the current control values to initialize an SchemaInfoWorkerArgs instance
                    // for the background operation.
                    DatabaseConnectionInfo connectionInfo = _useFAMDbRadioButton.Checked
                        ? null
                        : _databaseConnectionControl.DatabaseConnectionInfo;
                    string tableName = (_tableOrQueryComboBox.SelectedIndex == 0)
                        ? _tableComboBox.Text
                        : null;
                    string query = (_tableOrQueryComboBox.SelectedIndex == 1)
                        ? _queryTextBox.Text
                        : null;
                    var schemaInfoWorkerArgs =
                        new SchemaInfoWorkerArgs(connectionInfo, resetConnection, tableName, query);

                    // Kick off the operation in the background (let the UI continue to be
                    // manipulated in the meantime)
                    _schemaInfoBackgroundWorker.RunWorkerAsync(schemaInfoWorkerArgs);
                }
            }
        }

        /// <summary>
        /// Updates the table name auto-complete list.
        /// </summary>
        /// <param name="tableNames">The table names to apply or <see langword="null"/> for a blank
        /// list.</param>
        void UpdateTableNames(string[] tableNames)
        {
            _tableComboBox.Items.Clear();

            if (tableNames == null || tableNames.Length == 0)
            {
                _tableComboBox.AutoCompleteMode = AutoCompleteMode.None;
            }
            else
            {
                _tableComboBox.Items.AddRange(tableNames);
                _tableComboBox.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                _tableComboBox.AutoCompleteSource = AutoCompleteSource.CustomSource;
                _tableComboBox.AutoCompleteCustomSource = new AutoCompleteStringCollection();
                _tableComboBox.AutoCompleteCustomSource.AddRange(tableNames);
            }
        }

        /// <summary>
        /// Updates the field names auto-complete list.
        /// </summary>
        /// <param name="fieldNames">The field names to apply or <see langword="null"/> for a blank
        /// list.</param>
        void UpdateFieldNames(string[] fieldNames)
        {
            if (fieldNames == null || fieldNames.Length == 0)
            {
                _fieldNames = null;
            }
            else
            {
                _fieldNames = fieldNames;
            }
        }

        /// <summary>
        /// Gets the table names from the database (will work only on Microsoft databases; an empty
        /// array will be returned for other databases).
        /// </summary>
        /// <param name="dbConnection">The <see cref="DbConnection"/> for which the table list is
        /// needed.</param>
        /// <returns>An array of the table names.</returns>
        static string[] GetTableNames(DbConnection dbConnection)
        {
            try
            {
                return DBMethods.GetQueryResultsAsStringArray(dbConnection,
                    "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES ORDER BY TABLE_NAME");
            }
            catch
            {
                return new string[0];
            }
        }

        /// <summary>
        /// Gets the field names from the specified <see paramref="tableName"/> or returned from the
        /// <see paramref="query"/>.
        /// </summary>
        /// <param name="dbConnection">The <see cref="DbConnection"/> that is the target for the
        /// <see paramref="tableName"/> or <see paramref="query"/>.</param>
        /// <param name="fileProcessingDB">The <see cref="IFileProcessingDB"/> that may be needed to
        /// expand some path tags.</param>
        /// <param name="pathTags">The <see cref="FileActionManagerPathTags"/> used to
        /// expand any path tags/functions in the <see paramref="query"/>.</param>
        /// <param name="tableName">The name of the database table for which the field names are
        /// needed. Must be <see langword="null"/> if <see paramref="query"/> is specified.</param>
        /// <param name="query">The query for which the field names are needed. Must be
        /// <see langword="null"/> if <see paramref="tableName"/> is specified.</param>
        /// <returns>An array of the field names.</returns>
        string[] GetResultFields(DbConnection dbConnection, IFileProcessingDB fileProcessingDB,
            FileActionManagerPathTags pathTags, string tableName, string query)
        {
            ExtractException.Assert("ELI36976", "Invalid text expansion parameters.",
                string.IsNullOrEmpty(tableName) || string.IsNullOrEmpty(query));

            string sqlQuery = null;
            if (tableName != null)
            {
                sqlQuery = "SELECT * FROM [" + tableName + "]";
            }
            else
            {
                // Expand any embedded DataEntryQueries.
                FileRecord fileRecord = new FileRecord();
                bool isDataEntryQueryOnly = false;
                string[] expandedQuery = _useFAMDbRadioButton.Checked
                    ? Settings.ExpandText(
                        query, fileRecord, pathTags, null, fileProcessingDB, "MISSING_VALUE",
                        out isDataEntryQueryOnly)
                    : Settings.ExpandText(
                        query, fileRecord, pathTags, _databaseConnectionControl.DatabaseConnectionInfo,
                        null, "MISSING_VALUE", out isDataEntryQueryOnly);

                if (!isDataEntryQueryOnly)
                {
                    sqlQuery = expandedQuery[0];
                    // To get the columns, we don't actually need any data. Top 0 prevents
                    // waiting for the query to gather a lot of data. Since _topClauseRegex
                    // will actually find an unwanted hit for "DISTINCT" if present, replace
                    // only the first hit which should always be in the right spot.
                    sqlQuery = _topClauseRegex.Replace(sqlQuery, " TOP (0) ", 1);
                    // Remove everything from WHERE clause on as that won't affect the
                    // columns returned. Remove only the last WHERE clause so as not to
                    // break the query by cutting off within a nested query.
                    sqlQuery = _whereClauseRegex.Replace(sqlQuery, "", 1);
                }
            }

            // If a specified query isDataEntryQueryOnly, sqlQuery will not be populated; there will
            // not be field names available.
            if (!string.IsNullOrWhiteSpace(sqlQuery))
            {
                using (DbTransaction transaction = dbConnection.BeginTransaction())
                using (var dbCommand = DBMethods.CreateDBCommand(dbConnection, sqlQuery, null))
                {
                    // Command timeout is not supported for some DBs (such as SQL CE).
                    // Ignore any errors applying a command timeout.
                    try
                    {
                        dbCommand.CommandTimeout = _SCHEMA_UPDATE_TIMEOUT;
                    }
                    catch { }

                    dbCommand.Transaction = transaction;
                    try
                    {
                        using (DataTable queryResults = DBMethods.ExecuteDBQuery(dbCommand))
                        {
                            return queryResults.Columns
                                .OfType<DataColumn>()
                                .Select(column => column.ColumnName)
                                .Where(name => !string.IsNullOrWhiteSpace(name))
                                .ToArray();
                        }
                    }
                    finally
                    {
                        // Ensure the query does not result in the database being modified.
                        transaction.Rollback();
                    }
                }
            }
            else
            {
                // If no SQL query is specified, there are no field name that could be expanded.
                return new string[0];
            }
        }

        /// <summary>
        /// Gets the <see cref="DbConnection"/> instance to use to obtain schema info for
        /// auto-complete and settings validation in the UI.
        /// </summary>
        /// <param name="resetConnection"><see langword="true"/> if the connection should be
        /// re-established even if there is an existing open connection; <see langword="false"/> to
        /// re-use an existing connection when possible.</param>
        /// <param name="connectionInfo">The connection info.</param>
        /// <param name="fileProcessingDB"></param>
        /// <param name="pathTags">The <see cref="FileActionManagerPathTags"/>.</param>
        /// <returns></returns>
        DbConnection GetDbConnectionForSchemaUpdater(bool resetConnection, DatabaseConnectionInfo connectionInfo,
            IFileProcessingDB fileProcessingDB, FileActionManagerPathTags pathTags)
        {
            // Re-use a previously obtained connection if possible.
            if (!resetConnection && _schemaInfoDbConnection != null &&
                _schemaInfoDbConnection.State == ConnectionState.Open)
            {
                return _schemaInfoDbConnection;
            }

            if (_schemaInfoDbConnection != null)
            {
                _schemaInfoDbConnection?.Dispose();
                _schemaInfoDbConnection = null;
            }

            if (connectionInfo == null)
            {
                fileProcessingDB.ConnectLastUsedDBThisProcess();
                var connectionStringBuilder = new DbConnectionStringBuilder();
                connectionStringBuilder.ConnectionString = fileProcessingDB.ConnectionString;

                connectionStringBuilder.Add("Timeout", _SCHEMA_UPDATE_TIMEOUT);
                SqlConnectionStringBuilder sqlConnectionStringBuilder = new SqlConnectionStringBuilder(                
                     SqlUtil.CreateConnectionString(fileProcessingDB.DatabaseServer, fileProcessingDB.DatabaseName));
                sqlConnectionStringBuilder.ConnectTimeout = _SCHEMA_UPDATE_TIMEOUT;

                _schemaInfoDbConnection = new ExtractRoleConnection(connectionStringBuilder.ConnectionString);
                _schemaInfoDbConnection.Open();
            }
            else
            {
                // Use a DatabaseConnectionInfo instance to open manually specified connections so
                // that local SQL CE database copies are automatically managed.
                _schemaInfoDbConnectionInfo = new DatabaseConnectionInfo(connectionInfo);
                var connectionStringBuilder = new DbConnectionStringBuilder();
                connectionStringBuilder.ConnectionString = pathTags.Expand(connectionInfo.ConnectionString);
                if (connectionInfo.DataSource.Name.Equals(DataSource.SqlDataSource.Name, StringComparison.Ordinal))
                {
                    // Timeout parameter is generally not accepted by non-microsoft T-SQL databases
                    // (at least by this name). Most of the time we will be dealing with either
                    // T-SQL DBs or SQL CE DBs where connection timeouts are not going to be an
                    // issue.
                    connectionStringBuilder.Add("Timeout", _SCHEMA_UPDATE_TIMEOUT);
                }
                _schemaInfoDbConnectionInfo.ConnectionString = connectionStringBuilder.ConnectionString;
                _schemaInfoDbConnection = _schemaInfoDbConnectionInfo.OpenConnection();
            }

            return _schemaInfoDbConnection;
        }

        /// <summary>
        /// Applies the options configured in the UI to the <see cref="Settings"/>.
        /// </summary>
        /// <param name="schemaAvailable"><see langword="true"/> if schema info from the DB/query
        /// have been gathered and should be used to validate the settings; otherwise,
        /// <see langword="false"/></param>
        void ApplySettings(bool schemaAvailable)
        {
            try
            {
                // In case we had been waiting on a schema update operation, the UI would have been
                // disabled. Re-enable it now so that if there are validation errors that prevent
                // applying the settings they are able to use the UI.
                Enabled = true;

                // If there are invalid settings, prompt and return without closing.
                if (WarnIfInvalid(schemaAvailable))
                {
                    return;
                }

                // Apply the UI values to the Settings instance.
                Settings.UseFAMDBConnection = _useFAMDbRadioButton.Checked;
                Settings.DatabaseConnectionInfo =
                    new DatabaseConnectionInfo(_databaseConnectionControl.DatabaseConnectionInfo);
                Settings.UseQuery = _tableOrQueryComboBox.SelectedIndex == 1;
                Settings.Table = _tableComboBox.Text;
                Settings.Query = _queryTextBox.Text;
                Settings.CheckFields = _checkFieldsRadioButton.Checked;
                if (Settings.CheckFields)
                {
                    Settings.RowCountCondition = _checkFieldsRowCountComboBox.ToEnumValue<
                        DatabaseContentsConditionRowCount>();
                }
                else
                {
                    Settings.RowCountCondition = _doNotCheckFieldsRowCountComboBox.ToEnumValue<
                        DatabaseContentsConditionRowCount>();
                }
                Settings.SearchModifier =
                    _searchModifierComboBox.ToEnumValue<DatabaseContentsConditionSearchModifier>();

                IUnknownVector searchFields = new IUnknownVector();
                foreach (var row in _fieldsDataGridView.Rows.OfType<DataGridViewRow>()
                    .Where(row => !row.IsNewRow))
                {
                    IVariantVector fieldVector = new VariantVector();
                    fieldVector.PushBack((row.Cells[0].Value ?? "").ToString());
                    fieldVector.PushBack((row.Cells[1].Value ?? "").ToString());
                    fieldVector.PushBack((bool)(row.Cells[2].Value ?? false));
                    fieldVector.PushBack((bool)(row.Cells[3].Value ?? false));

                    searchFields.PushBack(fieldVector);
                }

                Settings.SearchFields = searchFields;

                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI36965");
            }
        }

        /// <summary>
        /// Displays a warning message if the user specified settings are invalid.
        /// </summary>
        /// <param name="schemaAvailable"><see langword="true"/> if schema info from the DB/query
        /// have been gathered and should be used to validate the settings; otherwise,
        /// <see langword="false"/></param>
        /// <returns><see langword="true"/> if the settings are invalid; <see langword="false"/> if
        /// the settings are valid.</returns>
        bool WarnIfInvalid(bool schemaAvailable)
        {
            ExtractException.Assert("ELI36977",
                "Query condition settings have not been provided.", Settings != null);

            // Validate than a database has been specified.
            if (_specifiedDbRadioButton.Checked &&
                (_databaseConnectionControl.DatabaseConnectionInfo.DataProvider == null ||
                 string.IsNullOrWhiteSpace(_databaseConnectionControl.DatabaseConnectionInfo.ConnectionString)))
            {
                _databaseConnectionControl.Focus();
                UtilityMethods.ShowMessageBox(
                        "The database connection has not been specified.",
                        "Invalid configuration", true);

                return true;
            }

            bool useQuery = _tableOrQueryComboBox.SelectedIndex == 1;

            // Validate than a query has been specified.
            if (useQuery && string.IsNullOrWhiteSpace(_queryTextBox.Text))
            {
                _queryTextBox.Focus();
                UtilityMethods.ShowMessageBox(
                    "The query has not been specified", "Invalid configuration", true);

                return true;
            }

            // Validate than a table has been specified.
            if (!useQuery && string.IsNullOrWhiteSpace(_tableComboBox.Text))
            {
                _tableComboBox.Focus();
                UtilityMethods.ShowMessageBox(
                    "The table has not been specified", "Invalid configuration", true);

                return true;
            }

            // Validate than a valid table name has been specified (if possible).
            if (schemaAvailable && !useQuery && _tableComboBox.Items.Count > 0 &&
                !_tableComboBox.Items
                    .OfType<string>()
                    .Any(name =>
                        _tableComboBox.Text.Equals(name, StringComparison.OrdinalIgnoreCase)))
            {
                _tableComboBox.Focus();
                UtilityMethods.ShowMessageBox(
                    "The specified table does not exist.", "Invalid configuration", true);

                return true;
            }

            if (_checkFieldsRadioButton.Checked)
            {
                // Remove any empty rows before checking that the remaining rows properly specify
                // fields to compare in the query results.
                var emptyRows = _fieldsDataGridView.Rows.OfType<DataGridViewRow>()
                    .Where(row => !row.IsNewRow &&
                        (row.Cells[0].Value == null || string.IsNullOrWhiteSpace(row.Cells[0].Value.ToString())) &&
                        (row.Cells[1].Value == null || string.IsNullOrWhiteSpace(row.Cells[1].Value.ToString())))
                    .ToArray();

                foreach (DataGridViewRow row in emptyRows)
                {
                    _fieldsDataGridView.Rows.Remove(row);
                }

                // Ensure at least one field has been specified.
                if (_fieldsDataGridView.RowCount <= 1)
                {
                    _fieldsDataGridView.Focus();
                    UtilityMethods.ShowMessageBox(
                        "The field(s) to compare the table or query results against have not been specified.",
                        "Invalid configuration", true);

                    return true;
                }

                // Perform validation on each row of the settings table.
                foreach (var cell in _fieldsDataGridView.Rows.OfType<DataGridViewRow>()
                    .Where(row => !row.IsNewRow)
                    .Select(row => row.Cells[0]))
                {
                    // Ensure a field name/ordinal has been specified.
                    if (cell.Value == null || string.IsNullOrWhiteSpace(cell.Value.ToString()))
                    {
                        _fieldsDataGridView.CurrentCell = cell;
                        _fieldsDataGridView.Focus();
                        UtilityMethods.ShowMessageBox(
                            "All specified fields to compare in the table or query results must have a name specified.",
                            "Invalid configuration", true);

                        return true;
                    }

                    if (schemaAvailable && _fieldNames != null && _fieldNames.Length > 0)
                    {
                        string stringValue = cell.Value.ToString();
                        int ordinal;
                        if (!Int32.TryParse(stringValue, out ordinal))
                        {
                            ordinal = -1;
                        }

                        // Validate that an invalid column ordinal was not used.
                        if (ordinal > 0)
                        {
                            if (ordinal > _fieldNames.Length)
                            {
                                _fieldsDataGridView.CurrentCell = cell;
                                _fieldsDataGridView.Focus();
                                UtilityMethods.ShowMessageBox(
                                    "The specified field number is greater than the number of columns available.",
                                    "Invalid configuration", true);

                                return true;
                            }
                        }
                        // Validate the specified field name.
                        else if (!_fieldNames
                            .OfType<string>()
                            .Any(name =>
                                stringValue.Equals(name, StringComparison.OrdinalIgnoreCase)))
                        {
                            _fieldsDataGridView.CurrentCell = cell;
                            _fieldsDataGridView.Focus();
                            UtilityMethods.ShowMessageBox(
                                "The specified field does not exist.",
                                "Invalid configuration", true);

                            return true;
                        }
                    }
                }
            }

            return false;
        }

        #endregion Private Members
    }
}
