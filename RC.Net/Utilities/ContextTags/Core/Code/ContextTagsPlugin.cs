using Extract.Database.Sqlite;
using Extract.Licensing;
using Extract.SQLCDBEditor;
using Extract.Utilities.ContextTags.SqliteModels.Version3;
using Extract.Utilities.Forms;
using LinqToDB;
using LinqToDB.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;

namespace Extract.Utilities.ContextTags
{
    /// <summary>
    /// A <see cref="SQLCDBEditorPlugin"/> implementation that allows for editing of a
    /// <see cref="CustomTagsDB"/> in a convenient, editable view.
    /// </summary>
    public class ContextTagsPlugin : SQLCDBEditorPlugin
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(ContextTagsPlugin).ToString();

        /// <summary>
        /// Database server tag name.
        /// </summary>
        const string _DATABASE_SERVER_TAG = "<DatabaseServer>";

        /// <summary>
        /// Database name tag name.
        /// </summary>
        const string _DATABASE_NAME_TAG = "<DatabaseName>";

        /// <summary>
        /// Constant string for the default values tag
        /// </summary>
        const string DefaultValuesTag = "<DefaultValues>";

        #endregion Constants

        #region Fields

        /// <summary>
        /// The path of the database to be edited.
        /// </summary>
        string _databasePath;
        
        /// <summary>
        /// A <see cref="CustomTagsDB"/> instance representing the database.
        /// </summary>
        CustomTagsDB _database;

#pragma warning disable CA2213 // Disposable fields should be disposed (These fields are expected to be managed by the plugin manager)

        /// <summary>
        /// A <see cref="ContextTagsEditorViewCollection"/> providing the editable view used in the
        /// plugin.
        /// </summary>
        ContextTagsEditorViewCollection _contextTagsView;

        /// <summary>
        /// A <see cref="Button"/> that can be used to edit the available contexts.
        /// </summary>
        Button _editContextsButton;

        /// <summary>
        /// A <see cref="Button"/> that can be used to edit the available contexts.
        /// </summary>
        Button _addDatabaseTagsButton;

        /// <summary>
        /// Combo box to select workflow values to display/edit
        /// </summary>
        ComboBox _workflowComboBox;

        /// <summary>
        /// Label for the workflow combo
        /// </summary>
        Label _workflowLabel;

#pragma warning restore CA2213 // Disposable fields should be disposed

        /// <summary>
        /// Indicates if the host is in design mode or not.
        /// </summary>
        readonly bool _inDesignMode;

        /// <summary>
        /// Dictionary to hold the workflows for the combo box
        /// </summary>
        List<String> _workflows;

        /// <summary>
        /// Used for HashSet to contain workflow load failures
        /// </summary>
        struct DatabaseData
        {
            public string Server;
            public string Database;
        }

        /// <summary>
        /// Used to contain the databases that could not load workflows
        /// </summary>
        HashSet<DatabaseData> _workflowLoadFailedDBs;

        /// <summary>
        /// Set of databases currently in progress of loading
        /// </summary>
        HashSet<DatabaseData> _workflowLoadInProgressDBs;

        /// <summary>
        /// Used for synchronization on _workflowLoadFailedDBs and _workflowLoadInProgressDBs
        /// </summary>
        Object _lockFor_workflowLoadDBs = new Object();

        /// <summary>
        /// Owning plugin manager
        /// </summary>
        ISQLCDBEditorPluginManager _pluginManager;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ContextTagsPlugin"/> class.
        /// </summary>
        public ContextTagsPlugin()
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
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI38032",
                    _OBJECT_NAME);

                _workflowLoadFailedDBs = new HashSet<DatabaseData>();
                _workflowLoadInProgressDBs = new HashSet<DatabaseData>();

                _workflows = new List<string>();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI38033");
            }
        }

        #endregion Constructors

        #region Overrides

        /// <summary>
        /// Gets the display name.
        /// </summary>
        public override string DisplayName
        {
            get
            {
                return "CustomTags";
            }
        }

        /// <summary>
        /// If not <see langword="null"/>, results of this query are displayed in a pane above the
        /// plugin control.
        /// </summary>
        public override string Query
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="BindingSource"/> property should be used
        /// to populate the results grid rather that the results of <see cref="Query"/>.
        /// </summary>
        /// <value><see langword="true"/> if the <see cref="BindingSource"/> property should be used
        /// to populate the results grid; otherwise, <see langword="false"/>.</value>
        public override bool ProvidesBindingSource
        {
            get
            {
                return true;
            }
        }

        /// Returns true
        public override bool DataIsValid => true;

        /// <summary>
        /// Allows plugin to initialize.
        /// </summary>
        /// <param name="pluginManager">The <see cref="ISQLCDBEditorPluginManager"/> manager for
        /// this plugin.</param>
        /// <param name="connection">The <see cref="DbConnection"/> for use by the plugin.</param>
        /// <remarks>The connection parameter must be a <see cref="System.Data.SQLite.SQLiteConnection"/></remarks>
        public override void LoadPlugin(ISQLCDBEditorPluginManager pluginManager, DbConnection connection)
        {
            try
            {
                if (connection is not System.Data.SQLite.SQLiteConnection sqliteConnection)
                {
                    throw new ArgumentException("Connection must be a SQLiteConnection", nameof(connection));
                }
                _databasePath = sqliteConnection.FileName;

                _pluginManager = pluginManager ?? throw new ArgumentNullException(nameof(pluginManager));
                _pluginManager.AllowUserToAddRows = true;
                _pluginManager.AllowUserToDeleteRows = true;

                _editContextsButton = pluginManager.GetNewButton();
                _editContextsButton.Text = "Edit Contexts";
                _editContextsButton.Click += HandleEditContextsButton_Click;
                _editContextsButton.HandleCreated += HandleEditContextsButton_HandleCreated;

                _addDatabaseTagsButton = pluginManager.GetNewButton();
                _addDatabaseTagsButton.Text = "Add Database Tags";
                _addDatabaseTagsButton.Click += HandleAddDatabaseTagsButton_Click;

                _workflowComboBox = new ComboBox();
                _workflowComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
                _workflowComboBox.Sorted = true;
                _workflowLabel = new Label();
                _workflowLabel.Text = "Workflow:";
                _workflowLabel.Visible = false;
                pluginManager.AddControlToPluginToolStrip(_workflowLabel);
                _workflowLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
                _workflowComboBox.Width += _workflowComboBox.Width;
                _workflowComboBox.Visible = false;
                pluginManager.AddControlToPluginToolStrip(_workflowComboBox);

                pluginManager.DataChanged += HandlePluginManager_DataChanged;
                pluginManager.DataGridViewCellFormatting += HandlePluginManager_DataGridViewCellFormatting;
                pluginManager.DataGridViewCellContextMenuStripNeeded += HandlePluginManager_DataGridViewCellContextMenuStripNeeded;

                RefreshData();

                UpdateWorkflowCombo();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI38035");
            }
        }

        /// <summary>
        /// Gets the <see cref="BindingSource"/> to use for the results grid data if
        /// <see cref="ProvidesBindingSource"/> is <see langword="true"/>.
        /// </summary>
        public override BindingSource BindingSource
        {
            get
            {
                return _contextTagsView;
            }
        }

        /// <summary>
        /// Performs any custom refresh logic needed by the plugin. Generally a plugin where
        /// <see cref="ProvidesBindingSource"/> is <see langword="true"/> will need to perform the
        /// refresh of the data here.
        /// </summary>
        public override void RefreshData()
        {
            try
            {
                base.RefreshData();

                ExtractException.Assert("ELI38036", "Database path is missing", !String.IsNullOrWhiteSpace(_databasePath));

                if (_database == null)
                {
                    _database = new CustomTagsDB(SqliteMethods.BuildConnectionOptions(_databasePath));
                }

				// Initialize or refresh data in the context tags view
				// Previously there was an issue with refresh that caused exception
				// the ContextTagsEditorViewCollection Refresh method has been modified to 
				// fix this problem
                if (_contextTagsView == null)
                {
                    _contextTagsView = new ContextTagsEditorViewCollection(_database, _workflowComboBox?.Text ?? "");
                    _contextTagsView.DataChanged += HandleContextTagsView_DataChanged;
                }
                else
                {
                    _contextTagsView.ActiveWorkflow = _workflowComboBox?.Text ?? "";
                    _contextTagsView.Refresh();
                }

                // The AllowNew set needs to come after Refresh; if AllowNew is set before the data
                // is initialized via the refresh call, errors will result as a the DataGridView is
                // initialized. 
                _contextTagsView.AllowNew = true;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI38037");
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
                if (_database != null)
                {
                    _database.Dispose();
                    _database = null;
                }
            }

            base.Dispose(disposing);
        }

        #endregion Overrides

        #region Event Handlers

        /// <summary>
        /// Handles the <see cref="Control.HandleCreated"/> event of <see cref="_editContextsButton"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleEditContextsButton_HandleCreated(object sender, EventArgs e)
        {
            try
            {
                // To ensure the plugin UI has been loaded and displayed before prompting to create
                // a context if necessary, used the handle creation of _editContextsButton as a
                // queue to invoke the check and prompt.
                _editContextsButton.SafeBeginInvoke("ELI38038", () =>
                {
                    PromptToCreateCurrentContext();
                });
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI38039");
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event of <see cref="_editContextsButton"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleEditContextsButton_Click(object sender, EventArgs e)
        {
            try
            {
                using ContextEditingForm contextEditingForm = new(_databasePath);
                if (contextEditingForm.ShowDialog(this) == DialogResult.OK)
                {
                    OnDataChanged(true, true);
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI38040");
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event of <see cref="_addDatabaseTagsButton"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleAddDatabaseTagsButton_Click(object sender, EventArgs e)
        {
            try
            {
                string[] tagsToAdd = new[] { _DATABASE_SERVER_TAG, _DATABASE_NAME_TAG };
                tagsToAdd = tagsToAdd.Except(_database.CustomTags.Select(tag => tag.Name)).ToArray();

                if (!tagsToAdd.Any())
                {
                    UtilityMethods.ShowMessageBox("The database tags have already been added.",
                        "Database tags already exist.", false);
                    return;
                }

                _database.BulkCopy(tagsToAdd.Select(tag => new CustomTag { Name = tag }));

                OnDataChanged(true, true);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI38093");
            }
        }

        /// <summary>
        /// Handles the <see cref="ContextTagsEditorViewCollection.DataChanged"/> event of the
        /// <see cref="_contextTagsView"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleContextTagsView_DataChanged(object sender, EventArgs e)
        {
            try
            {
                OnDataChanged(true, false);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI38228");
            }
        }

        /// <summary>
        /// Handles the DataChanged event from Plugin manager
        /// </summary>
        /// <param name="sender">The source of the event</param>
        /// <param name="e">The <see cref="DataChangedEventArgs"/> instance containing the event data</param>
        void HandlePluginManager_DataChanged(object sender, DataChangedEventArgs e)
        {
            try
            {
                UpdateWorkflowCombo();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI43273");
            }
        }

        /// <summary>
        /// Handles the <see cref="ComboBox.SelectionChangeCommitted"/> event of the <see cref="_workflowComboBox"/>. 
        /// </summary>
        /// <param name="sender">The source of the event</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void HandleWorkflowComboBox_SelectionChangeCommitted(object sender, EventArgs e)
        {
            try
            {
                _pluginManager.AllowUserToAddRows = _workflowComboBox.SelectedIndex <= 0;
                _pluginManager.AllowUserToDeleteRows = _workflowComboBox.SelectedIndex <= 0;
                RefreshData();
            }
            catch (Exception ex)
            {

                ex.ExtractDisplay("ELI43268");
            }
        }

        /// <summary>
        /// Handles the <see cref="DataGridView.CellFormatting"/> event that is available
        /// through the <see cref="ISQLCDBEditorPluginManager.DataGridViewCellFormatting"/> event
        /// </summary>
        /// <param name="sender">The source of the event</param>
        /// <param name="e">The <see cref="DataGridViewCellFormattingEventArgs"/> instance 
        /// containing the event data</param>
        void HandlePluginManager_DataGridViewCellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            try
            {
                // Check the row index is withing the bounds of the _contextTagsView
                if (e.RowIndex >= _contextTagsView.Count || e.RowIndex < 0 || e.ColumnIndex < 0)
                {
                    return;
                }

                // Get the ContextTagsEditorViewRow for the current row
                var row = _contextTagsView[e.RowIndex] as ContextTagsEditorViewRow;
                if (row != null)
                {
                    // Make sure the column is within the bounds of the properties collection for the row
                    var properties = row.GetProperties();
                    if (e.ColumnIndex < 0 || e.ColumnIndex >= properties.Count)
                    {
                        return;
                    }

                    // get the ContextTagsEditorViewPropertyDescriptor for the current column
                    var col = row.GetProperties()[e.ColumnIndex] as ContextTagsEditorViewPropertyDescriptor;

                    // If the value in the column is a value for a workflow change the color
                    if (col != null && col.IsWorkflowValue(row))
                    {
                        e.CellStyle.ForeColor = System.Drawing.Color.Red;
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI43350");
            }
        }

        /// <summary>
        /// Handles the <see cref="DataGridView.CellFormatting"/> event that is available
        /// through the <see cref="ISQLCDBEditorPluginManager.DataGridViewCellFormatting"/> event
        /// </summary>
        /// <param name="sender">The source of the event</param>
        /// <param name="e">The <see cref="DataGridViewCellFormattingEventArgs"/> instance 
        /// containing the event data</param>
        void HandlePluginManager_DataGridViewCellContextMenuStripNeeded(object sender, 
            DataGridViewCellContextMenuStripNeededEventArgs e)
        {
            try
            {
                // Check the row index is within the bounds of the _contextTagsView
                if (e.RowIndex >= _contextTagsView.Count || e.RowIndex < 0 || e.ColumnIndex < 0) 
                {
                    return;
                }

                 var dg = sender as DataGridView;

                // Change the current cell to the cell in event args
                if (dg != null)
                {
                    dg.CurrentCell = dg.Rows[e.RowIndex].Cells[e.ColumnIndex];
                }

               // Get the ContextTagsEditorViewRow for the current row
                var row = _contextTagsView[e.RowIndex] as ContextTagsEditorViewRow;
                if (row != null && row.HasBeenCommitted)
                {
                    // Make sure the column is within the bounds of the properties collection for the row
                    var properties = row.GetProperties();
                    if (e.ColumnIndex < 0 || e.ColumnIndex >= properties.Count)
                    {
                        return;
                    }

                    // get the ContextTagsEditorViewPropertyDescriptor for the current column
                    var col = row.GetProperties()[e.ColumnIndex] as ContextTagsEditorViewPropertyDescriptor;

                    // If the value in the column is a value for a workflow change the color
                    if (col != null && col.IsWorkflowValue(row))
                    {
                        e.ContextMenuStrip = new ContextMenuStrip();
                        e.ContextMenuStrip.Items.Add("Restore default", null,
                            (s, a) =>
                            {
                                try
                                {
                                    row.DeleteWorkflowValue(col.DisplayName);
                                    OnDataChanged(true, false);
                                    dg?.Refresh();
                                }
                                catch(Exception ex)
                                {
                                    ex.ExtractDisplay("ELI43484");
                                }
                            });
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI43352");
            }
        }

        #endregion Event Handlers

        #region Private Members

        /// <summary>
        /// Checks to see if there appears to be a context defined for the database's directory. If
        /// not, prompt to allow creation of a context.
        /// </summary>
        void PromptToCreateCurrentContext()
        {
            try
            {
                // If there isn't a context matching the database's directory display a dialog that
                // allows creating of a context for the current directory.
                if (string.IsNullOrWhiteSpace(_database.GetContextNameForDatabaseDirectory()))
                {
                    using CreateContextForm createContextForm = new(_databasePath);
                    if (createContextForm.ShowDialog(this) == DialogResult.OK)
                    {
                        OnDataChanged(true, true);
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI38042");
            }
        }

        /// <summary>
        /// Adds the workflows to the _workflows list and to the combo box
        /// </summary>
        /// <param name="addedWorkflows">Workflow to add</param>
        void AddWorkflows(List<string> addedWorkflows)
        {
            var workflowsToAdd = addedWorkflows.Except(_workflows, StringComparer.CurrentCultureIgnoreCase);
            if (workflowsToAdd.Any())
            {
                _workflows.AddRange(workflowsToAdd);
                LoadWorkflowsIntoComboBox();
            }
        }

        /// <summary>
        /// Sets the _workflows member variable to contain a list of workflows obtained from
        /// all the databases that have been defined with the DatabaseServer and DatabaseName tags
        /// and also include any workflows that are currently used in the Context tags database file
        /// </summary>
        void GetWorkflows()
        {
            // Add the workflows that are already in the database
            var workflowsInContextDB = _database.TagValues
                .Where(w => w.Workflow.Length != 0)
                .Select(s => s.Workflow)
                .Distinct().AsEnumerable()
                .Except(_workflows, StringComparer.CurrentCultureIgnoreCase); 

            _workflows.AddRange(workflowsInContextDB);

            var DatabaseServerTagID = _database.CustomTags
                .Where(t => t.Name == _DATABASE_SERVER_TAG)
                .Select(s => s.ID)
                .SingleOrDefault();
            var DatabaseNameTagID = _database.CustomTags
                .Where(t => t.Name == _DATABASE_NAME_TAG)
                .Select(s => s.ID)
                .SingleOrDefault();

            // if either DatabaseServerTag or DatabaseNameTag are not defined there are no workflows
            if (DatabaseServerTagID == 0 || DatabaseNameTagID == 0)
            {
                return;
            }

            // Get the all the context id's
            var ContextIDs = _database.Contexts.Select(c => c.ID).ToList();

            // For each context ID get the database name and database Server (if it exists)
            foreach (var contextID in ContextIDs)
            {
                DatabaseData db = new DatabaseData();
                db.Server = _database.TagValues
                    .Where(t => t.TagID == DatabaseServerTagID && t.ContextID == contextID)
                    .Select(t => t.Value)
                    .SingleOrDefault();
                db.Database = _database.TagValues
                  .Where(t => t.TagID == DatabaseNameTagID && t.ContextID == contextID)
                  .Select(t => t.Value)
                  .SingleOrDefault();

                lock (_lockFor_workflowLoadDBs)
                {
                    if (String.IsNullOrWhiteSpace(db.Server) || String.IsNullOrWhiteSpace(db.Database) ||
                        _workflowLoadFailedDBs.Contains(db) || _workflowLoadInProgressDBs.Contains(db))
                    {
                        continue;
                    }
                    _workflowLoadInProgressDBs.Add(db);
                }


				// Gets the Workflows from a database
                Thread getWorkflowThread = new Thread(() =>
                {
                    bool connected = false;
                    // This adds the values from the database assigned to the context 
                    try
                    {
                        FileProcessingDB famDB = new FileProcessingDB();
                        famDB.DatabaseServer = db.Server;
                        famDB.DatabaseName = db.Database;
                        famDB.ConnectionRetryTimeout = 0;
                        famDB.NumberOfConnectionRetries = 0;
                        famDB.ResetDBConnection(false, false);
                        connected = true;
                        StrToStrMap workflowMap = famDB.GetWorkflows();

                        var workflowsToAdd = workflowMap.ComToDictionary()
                            .Select(w => w.Key)
                            .ToList();

                        if (workflowsToAdd.Count > 0)
                        {
                            _workflowComboBox.SafeBeginInvoke("ELI43446", () =>
                                 {
                                     AddWorkflows(workflowsToAdd);

                                 }, false);
                        }
                    }
                    catch (Exception ex)
                    {
                        ExtractException ee = new ExtractException("ELI42184", "Unable to get workflows from database.", ex);
                        ee.AddDebugData("Server", db.Server, false);
                        ee.AddDebugData("Database", db.Database, false);
                        ee.Log();
                        if (!connected)
                        {
                            try
                            {
                                lock (_lockFor_workflowLoadDBs)
                                {
                                    _workflowLoadFailedDBs.Add(db);
                                }
                            }
                            catch (Exception) { }
                        }
                    }
                    finally
                    {
                        try
                        {
                            lock (_lockFor_workflowLoadDBs)
                            {
                                _workflowLoadInProgressDBs.Remove(db);
                            }
                        }
                        catch (Exception) { }
                    }
                });
                getWorkflowThread.SetApartmentState(ApartmentState.MTA);
                getWorkflowThread.Start();
            }
        }

        /// <summary>
        /// Updates the workflow combo for the current values in _workflows if the list is empty 
		/// the label and ComboBox will be hidden otherwise will be made visible and contain
		/// the values that are in _workflows
        /// </summary>
        void UpdateWorkflowCombo()
        {
            if (_workflowComboBox == null)
            {
                return;
            }
            
            GetWorkflows();

            LoadWorkflowsIntoComboBox();
        }

        /// <summary>
        /// Loads the workflows in _workflows list into the WorkflowCombo
        /// </summary>
        void LoadWorkflowsIntoComboBox()
        {
            _workflowComboBox.SelectionChangeCommitted -= HandleWorkflowComboBox_SelectionChangeCommitted;
            if (_workflows.Count > 0)
            {
                string selected = _workflowComboBox.SelectedItem as string;
                _workflowComboBox.Items.Clear();
                _workflowLabel.Visible = true;
                _workflowComboBox.Visible = true;
                _workflowComboBox.Items.Insert(0, DefaultValuesTag);
                _workflowComboBox.Items.AddRange(_workflows.ToArray());
                int indexToSelect = 0;
                if (!String.IsNullOrEmpty(selected))
                {
                    indexToSelect = _workflowComboBox.FindStringExact(selected);
                    indexToSelect = (indexToSelect < 0) ? 0 : indexToSelect;
                }
                _workflowComboBox.SelectedIndex = indexToSelect;
                _workflowComboBox.SelectionChangeCommitted += HandleWorkflowComboBox_SelectionChangeCommitted;
            }
            else
            {
                _workflowComboBox.Items.Clear();
                _workflowComboBox.Items.Insert(0, DefaultValuesTag);
                _workflowComboBox.Visible = false;
                _workflowLabel.Visible = false;
            }
        }

        #endregion Private Members
    }
}
