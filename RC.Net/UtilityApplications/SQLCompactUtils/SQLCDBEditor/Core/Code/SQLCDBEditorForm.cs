using Extract.Database;
using Extract.Database.Sqlite;
using Extract.Drawing;
using Extract.Licensing;
using Extract.SQLCDBEditor.Properties;
using Extract.Utilities;
using Extract.Utilities.Forms;
using Extract.Utilities.SqlCompactToSqliteConverter;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using TD.SandDock;

using static System.FormattableString;

namespace Extract.SQLCDBEditor
{
    /// <summary>
    /// This is the main form used by the SQLCDBEditor application. It will open a 
    /// SQLite database and display the tables and query lists in SandDock panes and will
    /// display records from the currently selected table in the pane on the right. Changes
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
        /// The license string for the SandDock manager
        /// </summary>
        static readonly string _SANDDOCK_LICENSE_STRING = @"1970|siE7SnF/jzINQg1AOTIaCXLlouA=";

        /// <summary>
        /// The default title for this form.
        /// </summary>
        const string _DEFAULT_TITLE = "ExtractSQLiteEditor";

        /// <summary>
        /// The full path to the file that contains information about persisting the 
        /// <see cref="SQLCDBEditorForm"/>.
        /// </summary>
        static readonly string _FORM_PERSISTENCE_FILE = FileSystemMethods.PathCombine(
            FileSystemMethods.UserApplicationDataPath, _DEFAULT_TITLE, _DEFAULT_TITLE + ".xml");

        /// <summary>
        /// Name for the mutex used to serialize persistence of the control and form layout.
        /// </summary>
        static readonly string _FORM_PERSISTENCE_MUTEX_STRING =
            "{DE40ED67-7A96-41F9-BDE8-20634534EF38";

        #endregion Constants

        #region Fields

        /// <summary>
        /// File name of the currently opened database.
        /// </summary>
        String _databaseFileName = string.Empty;

        /// <summary>
        /// The filename for a temporary copy of the database used while editing the database. This
        /// file will overwrite _databaseFileName when saved and will be deleted when the database
        /// is closed.
        /// </summary>
        string _databaseWorkingCopyFileName;

        /// <summary>
        /// The last modified time of _databaseFileName as of the time the database was last opened
        /// or saved by this instance. Used to determine if there may be a data conflict between
        /// edits made by this application and another application.
        /// </summary>
        DateTime _databaseLastModifiedTime;

        /// <summary>
        /// Opened connection to the current database.
        /// </summary>
        DbConnection _connection;

        /// <summary>
        /// The table names in the currently open database.
        /// </summary>
        HashSet<string> _tableNames;

        /// <summary>
        /// The list of table names in the currently open database.
        /// </summary>
        List<QueryAndResultsControl> _tableList = new List<QueryAndResultsControl>();

        /// <summary>
        /// The binding between <see cref="_tableList"/> and <see cref="_tablesListBox"/>
        /// </summary>
        BindingSource _tablesBindingSource = new BindingSource();

        /// <summary>
        /// The list of queries available for the open database.
        /// </summary>
        List<QueryAndResultsControl> _queryList = new List<QueryAndResultsControl>();

        /// <summary>
        /// The binding between <see cref="_queryList"/> and <see cref="_queriesListBox"/>
        /// </summary>
        BindingSource _queriesBindingSource = new BindingSource();

        /// <summary>
        /// The <see cref="QueryAndResultsControl"/> containing new queries that have not yet been
        /// saved to disk.
        /// </summary>
        List<QueryAndResultsControl> _pendingQueryList = new List<QueryAndResultsControl>();

        /// <summary>
        /// The list of plugins available for the open database.
        /// </summary>
        List<QueryAndResultsControl> _pluginList = new List<QueryAndResultsControl>();

        /// <summary>
        /// The last <see cref="StatusMessageChangedEventArgs"/> for each
        /// <see cref="QueryAndResultsControl"/> that was reported via the
        /// <see cref="QueryAndResultsControl.StatusMessageChanged"/> event.
        /// </summary>
        Dictionary<QueryAndResultsControl, StatusMessageChangedEventArgs> _activeStatusMessages =
            new Dictionary<QueryAndResultsControl, StatusMessageChangedEventArgs>();

        /// <summary>
        /// The binding between <see cref="_pluginList"/> and <see cref="_pluginsListBox"/>
        /// </summary>
        BindingSource _pluginsBindingSource = new BindingSource();

        /// <summary>
        /// The primary tab into which tables and queries will be opened unless otherwise specified.
        /// </summary>
        TabbedDocument _primaryTab;

        /// <summary>
        /// The most recently selected/active <see cref="QueryAndResultsControl"/>.
        /// </summary>
        QueryAndResultsControl _lastSelectedItem;

        /// <summary>
        /// The <see cref="QueryAndResultsControl"/> to be associated with a list box context menu.
        /// </summary>
        QueryAndResultsControl _contextMenuItem;

        /// <summary>
        /// Flag used to indicate that there are changes to the database that need to be saved.
        /// </summary>
        bool _dirty;

        /// <summary>
        /// Indicates whether this is running as a standalone app or as a dialog.
        /// </summary>
        bool _standAlone;

        /// <summary>
        /// Indicates whether or not a customerSchemaUpdateIsOccuring
        /// </summary>
        bool _customerSchemaUpdating = false;

        /// <summary>
        /// A custom value to display in the title bar.
        /// </summary>
        string _customTitle;

        /// <summary>
        /// The database schema manager used to update the current schema
        /// </summary>
        ISqliteDatabaseManager _schemaManager;

        /// <summary>
        /// Saves/restores window state info
        /// </summary>
        FormStateManager _formStateManager;

        /// <summary>
        /// Indicates if a UI replacement plugin is currently being used.
        /// </summary>
        bool _usingUIReplacement;

        /// <summary>
        /// Indicates if the host is in design mode or not.
        /// </summary>
        readonly bool _inDesignMode;

        /// <summary>
        /// Flag to indicate that a schema update is in progress
        /// </summary>
        bool _updatingSchema = false;

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
                _inDesignMode = (LicenseManager.UsageMode == LicenseUsageMode.Designtime);

                if (_inDesignMode)
                {
                    // Load the license files from folder
                    LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());
                }

                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects,
                    "ELI29538", _OBJECT_NAME);

                // License SandDock before creating the form
                SandDockManager.ActivateProduct(_SANDDOCK_LICENSE_STRING);

                InitializeComponent();

                _databaseFileName = databaseFileName;
                _standAlone = standAlone;

                // Establish the bindings for the table and query list boxes
                _tablesBindingSource.DataSource = _tableList;
                _tablesListBox.DataSource = _tablesBindingSource;
                _queriesBindingSource.DataSource = _queryList;
                _queriesListBox.DataSource = _queriesBindingSource;
                _pluginsBindingSource.DataSource = _pluginList;
                _pluginsListBox.DataSource = _pluginsBindingSource;

                if (LicenseManager.UsageMode != LicenseUsageMode.Designtime)
                {
                    _formStateManager = new FormStateManager(this, _FORM_PERSISTENCE_FILE,
                        _FORM_PERSISTENCE_MUTEX_STRING, _sandDockManager, false, null);
                }
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

                // Initialize the dirty flag
                _dirty = false;

                // The plugin list should be closed until a database is open with available plugins.
                if (!_inDesignMode && (_pluginDockableWindow.DockSituation != DockSituation.None))
                {
                    _pluginDockableWindow.Close();
                }

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
                    this.SafeBeginInvoke("ELI35346", () => { OpenDatabase(_databaseFileName); });
                }

                _primaryTab = new TabbedDocument();
                _primaryTab.AllowClose = false;
                _primaryTab.Manager = _sandDockManager;
                _primaryTab.Text = "";
                _primaryTab.TabImage = Resources.Star;
                _primaryTab.OpenDocument(WindowOpenMethod.OnScreen);
                _primaryTab.Closing += HandleTabbedDocumentClosing;
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

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Form.Closing"/> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.ComponentModel.CancelEventArgs"/> that contains
        /// the event data.</param>
        protected override void OnClosing(CancelEventArgs e)
        {
            try
            {
                // Make sure the user.config file is in a good state
                // https://extract.atlassian.net/browse/ISSUE-12830
                UserConfigChecker.EnsureValidUserConfigFile();

                base.OnClosing(e);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI38211");
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
                try
                {
                    if (_formStateManager != null)
                    {
                        _formStateManager.Dispose();
                        _formStateManager = null;
                    }

                    if (_connection != null)
                    {
                        _connection.Dispose();
                        _connection = null;
                    }

                    if (!string.IsNullOrEmpty(_databaseWorkingCopyFileName) &&
                        System.IO.File.Exists(_databaseWorkingCopyFileName))
                    {
                        FileSystemMethods.DeleteFile(_databaseWorkingCopyFileName);
                        _databaseWorkingCopyFileName = null;
                    }

                    CollectionMethods.ClearAndDispose(_tableList);
                    CollectionMethods.ClearAndDispose(_queryList);
                    CollectionMethods.ClearAndDispose(_pluginList);
                    CollectionMethods.ClearAndDispose(_pendingQueryList);

                    if (_tablesBindingSource != null)
                    {
                        _tablesListBox.DataSource = null;
                        _tablesBindingSource.Dispose();
                        _tablesBindingSource = null;
                    }

                    if (_queriesBindingSource != null)
                    {
                        _queriesListBox.DataSource = null;
                        _queriesBindingSource.Dispose();
                        _queriesBindingSource = null;
                    }

                    if (_pluginsBindingSource != null)
                    {
                        _pluginsListBox.DataSource = null;
                        _pluginsBindingSource.Dispose();
                        _pluginsBindingSource = null;
                    }

                    if (components != null)
                    {
                        components.Dispose();
                    }
                }
                catch (System.Exception ex)
                {
                    ex.ExtractLog("ELI34543");
                }
            }
            base.Dispose(disposing);
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
                // Don't allow any tab to abort the close. Specifically if a RowValidating event is
                // canceled for a plugin, that can end up triggering the cancel property to be true
                // here. But this shouldn't prevent the user from closing the application.
                e.Cancel = false;

                // Check for unsaved changes.
                if (!CheckForSaveAndConfirm())
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
                if (!CheckForSaveAndConfirm())
                {
                    // User selected cancel to save confirmation, so nothing to do.
                    return;
                }

                // Setup OpenFileDialog to get the database to open
                using (OpenFileDialog openDatabaseFile = new OpenFileDialog())
                {
                    openDatabaseFile.DefaultExt = "sqlite";
                    openDatabaseFile.Filter = "Database files (*.sqlite;*.sqlite3;*.db)|*.sqlite;*.sqlite3;*.db|All files (*.*)|*.*";

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
                if (!CheckForSaveAndConfirm())
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
        /// Handles the <see cref="T:ListBox.SelectionChanged"/> event from the
        /// <see cref="_tablesListBox"/> and <see cref="_queriesListBox"/>.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        private void HandleListSelectionChanged(object sender, EventArgs e)
        {
            try
            {
                ListBox listBox = (ListBox)sender;
                QueryAndResultsControl queryAndResultsControl =
                    listBox.SelectedItem as QueryAndResultsControl;
                if (queryAndResultsControl != null)
                {
                    // If the selected item was already selected, do nothing.
                    if (listBox.SelectedItem == _lastSelectedItem &&
                        listBox.SelectedItem == queryAndResultsControl)
                    {
                        return;
                    }

                    // Clear any selection from the other list box to make it clear what the
                    // currently selected item is.
                    switch (queryAndResultsControl.QueryAndResultsType)
                    {
                        case QueryAndResultsType.Table:
                            {
                                _queriesListBox.ClearSelected();
                                _pluginsListBox.ClearSelected();
                            }
                            break;

                        case QueryAndResultsType.Query:
                            {
                                _tablesListBox.ClearSelected();
                                _pluginsListBox.ClearSelected();
                            }
                            break;

                        case QueryAndResultsType.Plugin:
                            {
                                _tablesListBox.ClearSelected();
                                _queriesListBox.ClearSelected();
                            }
                            break;

                        default:
                            {
                                ExtractException.ThrowLogicException("ELI34829");
                            }
                            break;
                    }

                    Control previouslyActiveControl = ActiveControl;

                    OpenTableOrQuery(queryAndResultsControl, false, true);

                    // Return focus to the list box to prevent it from losing focus by clicking in it.
                    if (previouslyActiveControl == _navigationDockContainer)
                    {
                        listBox.Focus();
                    }
                }
                else
                {
                    // Clear _lastSelectedItem to ensure the if the same item is selected that was
                    // last selected, that it is re-displayed.
                    _lastSelectedItem = null;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI34603");
            }
        }

        /// <summary>
        /// Handles the <see cref="T:ContextMenuStrip.Opening"/> event from the
        /// <see cref="_contextMenuStrip"/>.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.ComponentModel.CancelEventArgs"/> instance
        /// containing the event data.</param>
        void HandleContextMenuStripOpening(object sender, CancelEventArgs e)
        {
            try
            {
                // If no context menu item has been identified, don't open the context menu.
                if (_contextMenuItem == null)
                {
                    e.Cancel = true;
                }
                else
                {
                    // The properties of the item may not be set correctly if it is not loaded.
                    // Load it in order to know what context menu options to display.
                    if (!_contextMenuItem.IsLoaded)
                    {
                        LoadTableOrQuery(_contextMenuItem);
                    }

                    bool isEditableQuery =
                        (_contextMenuItem.QueryAndResultsType == QueryAndResultsType.Query) &&
                        !_contextMenuItem.IsReadOnly;

                    _renameQueryMenuItem.Visible = isEditableQuery;
                    _deleteToolStripMenuItem.Visible = isEditableQuery;
                    _openInSeparateTabMenuItem.Visible =
                        _contextMenuItem.ShowSendToSeparateTabButton;
                    _copyToNewQueryToolStripMenuItem.Visible =
                        _contextMenuItem.QueryAndResultsType != QueryAndResultsType.Plugin;

                    // Invalidate the appropriate list box so that the focus rectangle gets drawn.
                    if (_tableList.Contains(_contextMenuItem))
                    {
                        _tablesListBox.Invalidate();
                    }
                    else if (_queryList.Contains(_contextMenuItem))
                    {
                        _queriesListBox.Invalidate();
                    }
                    else if (_pluginList.Contains(_contextMenuItem))
                    {
                        _pluginsListBox.Invalidate();
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI34604");
            }
        }

        /// <summary>
        /// Handles the <see cref="T:ContextMenuStrip.Closed"/> event from the
        /// <see cref="_contextMenuStrip"/>.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.ToolStripDropDownClosedEventArgs"/>
        /// instance containing the event data.</param>
        void HandleContextMenuStripClosed(object sender, ToolStripDropDownClosedEventArgs e)
        {
            try
            {
                // Invalidate the appropriate list box so that the focus rectangle gets cleared.
                if (_contextMenuItem != null)
                {
                    if (_tableList.Contains(_contextMenuItem))
                    {
                        _tablesListBox.Invalidate();
                    }
                    else if (_queryList.Contains(_contextMenuItem))
                    {
                        _queriesListBox.Invalidate();
                    }
                    else if (_pluginList.Contains(_contextMenuItem))
                    {
                        _pluginsListBox.Invalidate();
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI34605");
            }
        }

        /// <summary>
        /// Handles the <see cref="T:Control.Click"/> event for the
        /// <see cref="_openInSeparateTabMenuItem"/>.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleOpenInNewTabMenuItemClick(object sender, EventArgs e)
        {
            try
            {
                if (_contextMenuItem != null)
                {
                    OpenTableOrQuery(_contextMenuItem, true, true);
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI34606");
            }
        }

        /// <summary>
        /// Handles the <see cref="T:Control.Click"/> event for the
        /// <see cref="_renameQueryMenuItem"/>.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleRenameQueryMenuItemClick(object sender, EventArgs e)
        {
            try
            {
                if (_contextMenuItem != null)
                {
                    _contextMenuItem.Rename();
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI34607");
            }
        }

        /// <summary>
        /// Handles the <see cref="T:Control.Click"/> event for the
        /// <see cref="_copyToNewQueryToolStripMenuItem"/>.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleCopyToNewQueryMenuItemClick(object sender, EventArgs e)
        {
            try
            {
                // The query needs to be loaded before it can be used to create a copy.
                if (!_contextMenuItem.IsLoaded)
                {
                    LoadTableOrQuery(_contextMenuItem);
                }

                _contextMenuItem.CreateQueryCopy();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI34608");
            }
        }

        /// <summary>
        /// Handles the <see cref="T:Control.Click"/> event for the
        /// <see cref="_deleteToolStripMenuItem"/>.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleDeleteMenuItemClick(object sender, EventArgs e)
        {
            try
            {
                DeleteQuery(_contextMenuItem);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI34609");
            }
        }

        /// <summary>
        /// Handles the <see cref="T:QueryAndResultsControl.SentToSeparateTab"/> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleTableOrQuerySentToSeparateTab(object sender, EventArgs e)
        {
            try
            {
                OpenTableOrQuery((QueryAndResultsControl)sender, true, true);

                // Re-activate the primary tab make it clear to the user that the control was moved
                // and they may now open another table/query.
                if (_primaryTab != null)
                {
                    _primaryTab.Activate();
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI34611");
            }
        }

        /// <summary>
        /// Handles the <see cref="T:Control.Click"/> event for the
        /// <see cref="_newQueryToolStripButton"/>.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleNewQueryClick(object sender, EventArgs e)
        {
            try
            {
                var queryAndResultsControl = new QueryAndResultsControl(QueryAndResultsType.Query);

                InitializeNewQuery(queryAndResultsControl);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI34612");
            }
        }

        /// <summary>
        /// Handles the <see cref="T:QueryAndResultsControl.QueryCreated"/> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="Extract.SQLCDBEditor.QueryCreatedEventArgs"/> instance
        /// containing the event data.</param>
        void HandleQueryCreated(object sender, QueryCreatedEventArgs e)
        {
            try
            {
                InitializeNewQuery(e.QueryAndResultsControl);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI34613");
            }
        }

        /// <summary>
        /// Handles the <see cref="T:TabbedDocument.Closing"/> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="TD.SandDock.DockControlClosingEventArgs"/> instance
        /// containing the event data.</param>
        void HandleTabbedDocumentClosing(object sender, DockControlClosingEventArgs e)
        {
            try
            {
                if (!e.DockControl.AllowClose)
                {
                    // Prevent closing a UI replacement plugin or any other tab that has been
                    // programmatically set AllowClose = false.
                    e.Cancel = true;
                    return;
                }

                // When a tab is being closed, clear any data status messages.
                ApplyStatusMessage(null);

                // Don't allow the primary tab itself to be closed... only the control it contains.
                if (e.DockControl == _primaryTab)
                {
                    ClearPrimaryTab();

                    _tablesListBox.ClearSelected();
                    _queriesListBox.ClearSelected();
                    _pluginsListBox.ClearSelected();
                    _lastSelectedItem = null;

                    e.Cancel = true;
                    return;
                }

                if (e.DockControl.Controls.Count > 0)
                {
                    var queryAndResultsControl = (QueryAndResultsControl)e.DockControl.Controls[0];

                    if (_pendingQueryList.Contains(queryAndResultsControl))
                    {
                        // If the query is unsaved, prompt to save it before allowing the tab to be
                        // closed.
                        if (PromptAndSaveDirtyQueries(queryAndResultsControl))
                        {
                            _pendingQueryList.Remove(queryAndResultsControl);

                            // If the query was saved to disk, clear the QueryAndResultsControl out
                            // of the tabbed document so that it is not disposed of.
                            if (queryAndResultsControl.QueryFileExists)
                            {
                                e.DockControl.Controls.Clear();
                            }

                        }
                        else
                        {
                            e.Cancel = true;
                            return;
                        }
                    }
                    else
                    {
                        // Once a separate tab is closed, the send to separate tab button should be
                        // available again.
                        queryAndResultsControl.ShowSendToSeparateTabButton = true;

                        // Clear the QueryAndResultsControl out of the tabbed document so that it is
                        // not disposed of (even though the tab is closed, the QueryAndResultsControl
                        // will still be maintained to use in whatever tab it is next opened in.
                        e.DockControl.Controls.Clear();
                    }

                    // If this control was the current selection in one of the lists, clear that
                    // selection.
                    if (_tablesListBox.SelectedItem == queryAndResultsControl)
                    {
                        _tablesListBox.ClearSelected();
                    }
                    else if (_queriesListBox.SelectedItem == queryAndResultsControl)
                    {
                        _queriesListBox.ClearSelected();
                    }
                    else if (_pluginsListBox.SelectedItem == queryAndResultsControl)
                    {
                        _pluginsListBox.ClearSelected();
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI34614");
            }
        }

        /// <summary>
        /// Handles the <see cref="T:TabbedDocument.Closed"/> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        void HandleTabbedDocumentClosed(object sender, EventArgs e)
        {
            try
            {
                // When a tab is closed, ensure the tab that is now displayed (if any) is activated
                // to ensure the selection in the list boxes is up-to-date.
                if (_sandDockManager.ActiveTabbedDocument != null)
                {
                    _sandDockManager.ActiveTabbedDocument.Activate();
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI34639");
            }
        }

        /// <summary>
        /// Handles the <see cref="T:QueryAndResultsControl.DataChanged"/> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="DataChangedEventArgs"/> instance containing the event data.
        /// </param>
        void HandleDataChanged(object sender, DataChangedEventArgs e)
        {
            try
            {
                if (e.DataCommitted)
                {
                    // If data has been changed in one table, refresh the data in all other visible
                    // tables immediately.
                    foreach (var queryAndResultsControl in _sandDockManager.GetDockControls()
                        .OfType<TabbedDocument>()
                        .Where(tab => tab.Controls.Count == 1)
                        .Select(tab => (QueryAndResultsControl)tab.Controls[0])
                        .Where(control => e.RefreshSource || control != sender))
                    {
                        // Use false so that for queries, the results get marked as stale, but don't
                        // get automatically refreshed.
                        queryAndResultsControl.RefreshData(false, false);
                    }
                }

                _dirty = true;

                EnableCommands();

                SetWindowTitle();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI34615");
            }
        }

        /// <summary>
        /// Handles the <see cref="T:QueryAndResultsControl.StatusMessageChanged"/> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="Extract.SQLCDBEditor.StatusMessageChangedEventArgs"/>
        /// instance containing the event data.</param>
        void HandleStatusMessageChanged(object sender, StatusMessageChangedEventArgs e)
        {
            try
            {
                // Keep track of this status message for the case that this QueryAndResultsControl
                // loses then re-gains focus again later.
                _activeStatusMessages[(QueryAndResultsControl)sender] = e;

                _dataStatusLabel.Text = e.StatusMessage;
                _dataStatusLabel.ForeColor = (e.TextColor == Color.Empty)
                    ? Color.Black
                    : e.TextColor;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI37598");
            }
        }

        /// <summary>
        /// Handles the <see cref="T:QueryAndResultsControl.PropertyChanged"/> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.ComponentModel.PropertyChangedEventArgs"/>
        /// instance containing the event data.</param>
        void HandleQueryPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            try
            {
                // If the display name property has been modified, update the tab text to match it.
                if (e.PropertyName == "DisplayName")
                {
                    QueryAndResultsControl queryAndResultsControl = (QueryAndResultsControl)sender;
                    TabbedDocument documentWindow = queryAndResultsControl.Parent as TabbedDocument;
                    if (documentWindow != null)
                    {
                        documentWindow.TabText = queryAndResultsControl.DisplayName;
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI34616");
            }
        }

        /// <summary>
        /// Handles the <see cref="T:QueryAndResultsControl.Saved"/> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleQuerySaved(object sender, EventArgs e)
        {
            try
            {
                // If this query has not yet been saved to disk, add it to the queries list box.
                if (!_queryList.Contains(sender))
                {
                    var queryAndResultsControl = (QueryAndResultsControl)sender;

                    _pendingQueryList.Remove(queryAndResultsControl);
                    _queryList.Add(queryAndResultsControl);
                    _queriesBindingSource.ResetBindings(false);
                    _lastSelectedItem = null;
                    _queriesListBox.SelectedItem = sender;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI34617");
            }
        }

        /// <summary>
        /// Handles the query renaming.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="Extract.SQLCDBEditor.QueryRenamingEventArgs"/> instance
        /// containing the event data.</param>
        void HandleQueryRenaming(object sender, QueryRenamingEventArgs e)
        {
            try
            {
                if (ExistingQueryNames.Contains(e.NewName))
                {
                    e.Cancel = true;
                    e.CancelReason = "Query \"" + e.NewName + "\" already exists.";
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI34627");
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
                if (_schemaManager == null || !_schemaManager.IsUpdateRequired)
                {
                    return;
                }

                UpdateToCurrentSchema();
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI34618", ex);
            }
        }

        /// <summary>
        /// Handles the <see cref="T:DockableWindow.Closing"/> event for the
        /// <see cref="_tableDockWindow"/> and <see cref="_queryDockWindow"/>.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="TD.SandDock.DockControlClosingEventArgs"/> instance
        /// containing the event data.</param>
        void HandleDockWindowClosing(object sender, TD.SandDock.DockControlClosingEventArgs e)
        {
            try
            {
                // In order to allow the close (X) button to be used to "close" they query or table
                // list panes, but still have a tab available to re-open them, cancel the close and
                // collapse the pane instead.
                e.Cancel = true;
                e.DockControl.Collapsed = true;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI34619");
            }
        }

        /// <summary>
        /// Handles the <see cref="T:SandDockManager.DockControlActivated"/> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="TD.SandDock.DockControlEventArgs"/> instance containing
        /// the event data.</param>
        void HandleDockControlActivated(object sender, DockControlEventArgs e)
        {
            try
            {
                // If the activated dock control is a TabbedDocument, select the corresponding item
                // in either the table or query list box.
                TabbedDocument tabbedDocument = e.DockControl as TabbedDocument;
                if (tabbedDocument != null)
                {
                    // If the activated tab does not have an item for any list, clear any
                    // selection in all.
                    if (e.DockControl.Controls.Count == 0)
                    {
                        _tablesListBox.ClearSelected();
                        _queriesListBox.ClearSelected();
                        _pluginsListBox.ClearSelected();
                        return;
                    }

                    QueryAndResultsControl queryAndResultsControl =
                        (QueryAndResultsControl)e.DockControl.Controls[0];

                    if (_tableList.Contains(queryAndResultsControl))
                    {
                        _queriesListBox.ClearSelected();
                        _pluginsListBox.ClearSelected();

                        if (!_tablesListBox.SelectedItems.Contains(queryAndResultsControl))
                        {
                            _tablesListBox.ClearSelected();
                            _tablesListBox.SelectedItems.Add(queryAndResultsControl);
                        }
                    }
                    else if (_queryList.Contains(queryAndResultsControl))
                    {
                        _tablesListBox.ClearSelected();
                        _pluginsListBox.ClearSelected();

                        if (!_queriesListBox.SelectedItems.Contains(queryAndResultsControl))
                        {
                            _queriesListBox.ClearSelected();
                            _queriesListBox.SelectedItems.Add(queryAndResultsControl);
                        }
                    }
                    else if (_pluginList.Contains(queryAndResultsControl))
                    {
                        _tablesListBox.ClearSelected();
                        _queriesListBox.ClearSelected();

                        if (!_pluginsListBox.SelectedItems.Contains(queryAndResultsControl))
                        {
                            _pluginsListBox.ClearSelected();
                            _pluginsListBox.SelectedItems.Add(queryAndResultsControl);
                        }
                    }
                    else
                    {
                        // This does not yet belong to any list. Clear any selection in all lists.
                        _tablesListBox.ClearSelected();
                        _queriesListBox.ClearSelected();
                        _pluginsListBox.ClearSelected();
                    }
                }
                else
                {
                    // If the activated dock control is the table or query pane, in order to allow
                    // the pane to "uncollapse" with a single click (without requiring it to be
                    // pinned), set the Collapsed property to false.
                    e.DockControl.Collapsed = false;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI34620");
            }
        }

        /// <summary>
        /// Handles the <see cref="T:Control.MouseDown"/> event for the <see cref="_tablesListBox"/>
        /// and <see cref="_queriesListBox"/>.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.MouseEventArgs"/> instance
        /// containing the event data.</param>
        void HandleListMouseDown(object sender, MouseEventArgs e)
        {
            try
            {
                // If the right mouse button was clicked, the context menu will open and will need
                // to know which list item it is to be associated with. Assign _contextMenuItem for
                // it to use.
                if (e.Button == MouseButtons.Right)
                {
                    ListBox listBox = (ListBox)sender;
                    int index = listBox.IndexFromPoint(e.Location);
                    if (index >= 0)
                    {
                        _contextMenuItem = (QueryAndResultsControl)listBox.Items[index];
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI34621");
            }
        }

        /// <summary>
        /// Handles the <see cref="T:ListBox.DrawItem"/> event for the <see cref="_tablesListBox"/>
        /// and <see cref="_queriesListBox"/>.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.DrawItemEventArgs"/> instance containing the event data.</param>
        void HandleListBoxDrawItem(object sender, DrawItemEventArgs e)
        {
            try
            {
                // Manually draw the list box items so that the focus rectangle can be displayed for
                // the item associated with a context menu click.
                if (e.Index >= 0)
                {
                    ListBox listBox = (ListBox)sender;
                    object listItem = listBox.Items[e.Index];
                    string itemText = listBox.GetItemText(listItem);

                    e.DrawBackground();
                    e.Graphics.DrawString(itemText, e.Font, ExtractBrushes.GetSolidBrush(e.ForeColor),
                        e.Bounds, StringFormat.GenericDefault);

                    if (listItem == _contextMenuItem && _contextMenuStrip.Visible)
                    {
                        ControlPaint.DrawFocusRectangle(e.Graphics, e.Bounds);
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI34622");
            }
        }

        /// <summary>
        /// Handles the <see cref="T:Control.PreviewKeyDown"/> event for the
        /// <see cref="_tablesListBox"/> and <see cref="_queriesListBox"/>.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.PreviewKeyDownEventArgs"/> instance
        /// containing the event data.</param>
        void HandleQueryListPreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            try
            {
                if (e.KeyCode == Keys.Delete && _queriesListBox.SelectedItem != null)
                {
                    DeleteQuery((QueryAndResultsControl)_queriesListBox.SelectedItem);
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI34623");
            }
        }

        /// <summary>
        /// Handler for Import menu item.
        /// </summary>
        void HandleImportClick(object sender, EventArgs e)
        {
            try
            {
                using (ImportTableForm importForm = new ImportTableForm(_databaseWorkingCopyFileName,
                                                              _tableNames.ToArray<string>(),
                                                              _connection))
                {
                    importForm.ShowDialog();
                    var modifiedTableName = importForm.ModifiedTableName;
                    if (!String.IsNullOrWhiteSpace(modifiedTableName))
                    {
                        // If data has been changed in one table, refresh the data in all other visible
                        // tables immediately.
                        foreach (var queryAndResultsControl in _sandDockManager.GetDockControls()
                            .OfType<TabbedDocument>()
                            .Where(tab => tab.Controls.Count == 1)
                            .Select(tab => (QueryAndResultsControl)tab.Controls[0])
                            .Where(control => control != sender))
                        {
                            // Use true here so that queries are automatically refreshed.
                            queryAndResultsControl.RefreshData(updateQueryResult: true, forceQueryExecution: false);
                        }

                        _dirty = true;
                        EnableCommands();
                    }
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI39031", ex);
            }

        }

        /// <summary>
        /// Handler for export menu item.
        /// </summary>
        void HandleExportClick(object sender, EventArgs e)
        {
            try
            {
                using (ExportTablesForm exportTables = new ExportTablesForm(
                    _databaseWorkingCopyFileName, _tableNames.ToArray(), _connection))
                {
                    exportTables.ShowDialog();
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI39033", ex);
            }
        }

        #endregion Event Handlers

        #region Private Methods

        /// <summary>
        /// Gets or sets if a UI replacement plugin is currently being used.
        /// </summary>
        /// <value><see langword="true"/> if a UI replacement plugin is currently being used;
        /// otherwise, <see langword="false"/>.
        /// </value>
        bool UsingUIReplacement
        {
            get
            {
                return _usingUIReplacement;
            }

            set
            {
                if (value != _usingUIReplacement)
                {
                    _usingUIReplacement = value;
                    _newQueryToolStripButton.Visible = !value;
                    _newQueryToolStripMenuItem.Visible = !value;

                    if (_usingUIReplacement)
                    {
                        _navigationDockContainer.Hide();
                        if (_primaryTab != null)
                        {
                            _primaryTab.Close();
                            _primaryTab.Dispose();
                            _primaryTab = null;
                        }
                    }
                    else
                    {
                        if (_primaryTab == null)
                        {
                            _primaryTab = new TabbedDocument();
                            _primaryTab.AllowClose = false;
                            _primaryTab.Manager = _sandDockManager;
                            _primaryTab.Text = "";
                            _primaryTab.TabImage = Resources.Star;
                            _primaryTab.OpenDocument(WindowOpenMethod.OnScreen);
                            _primaryTab.Closing += HandleTabbedDocumentClosing;
                        }
                        _navigationDockContainer.Show();
                    }
                }
            }
        }

        /// <summary>
        /// Gets the table names in the currently open database.
        /// </summary>
        /// <returns>A <see cref="HashSet{T}"/> of table names in the currently open database.
        /// </returns>
        HashSet<string> TableNames
        {
            get
            {
                if (_tableNames == null)
                {
                    _tableNames = new HashSet<string>(DBMethods.GetQueryResultsAsStringArray(_connection,
                        "SELECT [name] FROM [sqlite_master] WHERE [type] = 'table' AND [name] NOT LIKE 'sqlite_%'"));
                }

                return _tableNames;
            }
        }

        /// <summary>
        /// Method loads the names of the tables in the opened database into the list box. This
        /// method does not save any changes to the database that was previously loaded. If the 
        /// database should be saved it should be done before calling this method.
        /// </summary>
        /// <returns>The names of the tables that were loaded.</returns>
        [SuppressMessage("Microsoft.Globalization", "CA1306:SetLocaleForDataTypes")]
        [SuppressMessage("Microsoft.Globalization", "CA1309:Use ordinal StringComparison", Justification = "Invariant culture is a more natural ordering than ordinal")]
        void LoadTableList()
        {
            // Remove the handler for the SelectedIndexChanged event while loading the list box
            _tablesListBox.SelectedIndexChanged -= HandleListSelectionChanged;

            try
            {
                _tableList.AddRange(TableNames
                    .OrderBy(tableName => tableName, StringComparer.InvariantCultureIgnoreCase)
                    .Select(tableName =>
                        new QueryAndResultsControl(tableName, _databaseFileName,
                            QueryAndResultsType.Table)));

                _tablesBindingSource.ResetBindings(false);

                _tablesListBox.ClearSelected();
            }
            catch (Exception ex)
            {
                throw new ExtractException("ELI29627", "Unable to load tables.", ex);
            }
            finally
            {
                // Re-activate the SelectedIndexChanged event handler
                _tablesListBox.SelectedIndexChanged += HandleListSelectionChanged;
            }
        }

        /// <summary>
        /// Loads any .sqlce files into the query list pane. The name in the pane will be the name
        /// of the file (without the extension).
        /// </summary>
        void LoadQueryList()
        {
            // Remove the handler for the SelectedIndexChanged event while loading the list box
            _queriesListBox.SelectedIndexChanged -= HandleListSelectionChanged;

            try
            {
                _queryList.AddRange(
                    Directory.EnumerateFiles(Path.GetDirectoryName(_databaseFileName), "*.sqlce")
                    .Select(fileName => new QueryAndResultsControl(
                        Path.GetFileNameWithoutExtension(fileName), fileName,
                            QueryAndResultsType.Query)));

                _queriesBindingSource.ResetBindings(false);

                _queriesListBox.ClearSelected();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI34624");
            }
            finally
            {
                // Re-activate the SelectedIndexChanged event handler
                _queriesListBox.SelectedIndexChanged += HandleListSelectionChanged;
            }
        }

        /// <summary>
        /// Loads any .plugin files into the plugin list pane.
        /// </summary>
        void LoadPluginList()
        {
            // Remove the handler for the SelectedIndexChanged event while loading the list box
            _pluginsListBox.SelectedIndexChanged -= HandleListSelectionChanged;

            try
            {
                IEnumerable<string> pluginAssemblies;
                if (_schemaManager?.UISupplementPluginAssemblies is IEnumerable<string> assemblies
                    && assemblies.Any())
                {
                    pluginAssemblies = assemblies;
                }
                else
                {
                    pluginAssemblies = Directory.EnumerateFiles(Path.GetDirectoryName(_databaseFileName), "*.plugin");
                }

                _pluginList.AddRange(
                    pluginAssemblies
                    .Select(fileName =>
                        UtilityMethods.CreateTypeFromAssembly<SQLCDBEditorPlugin>(fileName))
                    .Where(plugin => plugin != null)
                    .Select(plugin => new QueryAndResultsControl(plugin)));

                _pluginsBindingSource.ResetBindings(false);

                _pluginsListBox.ClearSelected();

                // The plugin pane should be visible only if there are available plugins.
                UpdatePluginPaneVisibility();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI38268");
            }
            finally
            {
                // Re-activate the SelectedIndexChanged event handler
                _pluginsListBox.SelectedIndexChanged += HandleListSelectionChanged;
            }
        }

        /// <summary>
        /// Opens and activates the specified <see paramref="queryAndResultsControl"/> into either
        /// the primary tab or a separate tab.
        /// </summary>
        /// <param name="queryAndResultsControl">The <see cref="QueryAndResultsControl"/> to open.
        /// </param>
        /// <param name="openInNewTab"><see langword="true"/> to open in a separate tab that will
        /// not be used to display any other table or query, or <see langword="false"/> to open
        /// into the primary tab.</param>
        /// <param name="activate"><see langword="true"/> if the tab should be activated; otherwise,
        /// <see langword="false"/>.</param>
        void OpenTableOrQuery(QueryAndResultsControl queryAndResultsControl, bool openInNewTab, bool activate)
        {
            // If an opened tab is not activated, it doesn't get sized correctly (even if
            // PerformLayout is called). Therefore, allow all tabs to activated when opened so that
            // the tab layout can initialize, then restore the last active tab if active is false.
            // Lock control updates during this time so that there is no flicker as tabs temporarily
            // activate.
            DockControl activeTab = _sandDockManager.ActiveTabbedDocument;
            FormsMethods.LockControlUpdate(this, true);

            try
            {
                queryAndResultsControl.SuspendLayout();
                SuspendLayout();

                OpenTableOrQueryCore(queryAndResultsControl, openInNewTab);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI34610");
            }
            finally
            {
                try
                {
                    ResumeLayout(true);
                    queryAndResultsControl.ResumeLayout(true);
                    if (!activate && activeTab != null)
                    {
                        activeTab.Activate();
                    }
                }
                catch (Exception ex)
                {
                    ex.ExtractLog("ELI39369");
                }

                FormsMethods.LockControlUpdate(this, false);
                Invalidate(true);
            }
        }

        /// <summary>
        /// A helper method for <see cref="OpenTableOrQuery"/> that activates the specified
        /// <see paramref="queryAndResultsControl"/> into either the primary tab or a separate tab.
        /// </summary>
        /// <param name="queryAndResultsControl">The <see cref="QueryAndResultsControl"/> to open.
        /// </param>
        /// <param name="openInNewTab"><see langword="true"/> to open in a separate tab that will
        /// not be used to display any other table or query, or <see langword="false"/> to open
        /// into the primary tab.</param>
        void OpenTableOrQueryCore(QueryAndResultsControl queryAndResultsControl, bool openInNewTab)
        {
            // When a queryAndResultsControl is displayed, update the status bar with this control's
            // active status message (if any).
            ApplyStatusMessage(queryAndResultsControl);

            TabbedDocument tabbedDocument = null;

            _lastSelectedItem = queryAndResultsControl;

            // Initialize the control if it has not already been.
            bool isAlreadyLoaded = queryAndResultsControl.IsLoaded;
            if (!isAlreadyLoaded)
            {
                LoadTableOrQuery(queryAndResultsControl);
            }

            // If the control's parent is not null, it is currently open.
            if (queryAndResultsControl.Parent != null)
            {
                tabbedDocument = (TabbedDocument)queryAndResultsControl.Parent;

                if (openInNewTab && tabbedDocument == _primaryTab)
                {
                    // If opening in a new tab the control that is open in the primary tab, first
                    // close it in the primary tab.
                    ClearPrimaryTab();
                    tabbedDocument = null;
                }
            }

            // If the control is not currently open, find or create a tab to open it in.
            if (tabbedDocument == null)
            {
                // Create a separate tab for it.
                if (openInNewTab)
                {
                    tabbedDocument = new TabbedDocument(_sandDockManager, queryAndResultsControl,
                        queryAndResultsControl.DisplayName);
                    if (_primaryTab != null)
                    {
                        tabbedDocument.OpenWith(_primaryTab);
                    }
                    else
                    {
                        tabbedDocument.Open(WindowOpenMethod.OnScreen);
                    }
                    tabbedDocument.Closing += HandleTabbedDocumentClosing;
                    tabbedDocument.Closed += HandleTabbedDocumentClosed;
                    tabbedDocument.AllowClose = !UsingUIReplacement;
                }
                // ... or close the control currently in the primary tab and open it there.
                else if (_primaryTab != null)
                {
                    if (_primaryTab.Controls.Count > 0)
                    {
                        _primaryTab.Controls.Clear();
                    }
                    _primaryTab.Controls.Add(queryAndResultsControl);
                    _primaryTab.TabText = queryAndResultsControl.DisplayName;
                    _primaryTab.AllowClose = true;
                    tabbedDocument = _primaryTab;
                }
            }

            // If the control is open in a separate tab, don't provide the option to send it to
            // another one.
            queryAndResultsControl.ShowSendToSeparateTabButton =
                !UsingUIReplacement && (tabbedDocument == _primaryTab);

            if (isAlreadyLoaded)
            {
                queryAndResultsControl.RefreshData(false, false);
            }

            tabbedDocument.Activate();
        }

        /// <summary>
        /// Loads and initializes the specified <see paramref="queryAndResultsControl"/>.
        /// </summary>
        /// <param name="queryAndResultsControl">The <see cref="QueryAndResultsControl"/> to load.
        /// </param>
        void LoadTableOrQuery(QueryAndResultsControl queryAndResultsControl)
        {
            if (queryAndResultsControl.IsLoaded)
            {
                return;
            }

            switch (queryAndResultsControl.QueryAndResultsType)
            {
                case QueryAndResultsType.Table:
                    {
                        queryAndResultsControl.LoadTable(_connection, queryAndResultsControl.DisplayName);
                    }
                    break;

                case QueryAndResultsType.Query:
                    {
                        try
                        {
                            queryAndResultsControl.LoadQuery(_connection);
                        }
                        catch (Exception ex)
                        {
                            // If this is a query, go ahead and allow it to be displayed (after displaying
                            // the exception) so that the query text may be corrected.
                            ex.ExtractDisplay("ELI34656");
                        }

                        queryAndResultsControl.PropertyChanged += HandleQueryPropertyChanged;
                        queryAndResultsControl.QuerySaved += HandleQuerySaved;
                        queryAndResultsControl.QueryRenaming += HandleQueryRenaming;
                    }
                    break;

                case QueryAndResultsType.Plugin:
                    {
                        queryAndResultsControl.LoadPlugin(_connection);
                    }
                    break;

                default:
                    {
                        ExtractException.ThrowLogicException("ELI34830");
                    }
                    break;
            }

            queryAndResultsControl.DataChanged += HandleDataChanged;
            queryAndResultsControl.StatusMessageChanged += HandleStatusMessageChanged;
            queryAndResultsControl.SentToSeparateTab += HandleTableOrQuerySentToSeparateTab;
            queryAndResultsControl.QueryCreated += HandleQueryCreated;
        }

        /// <summary>
        /// Closes the specified <see paramref="queryAndResultsControl"/> and deletes any associated
        /// sqlce file from disk.
        /// </summary>
        /// <param name="queryAndResultsControl">The <see cref="QueryAndResultsControl"/> to delete.
        /// </param>
        void DeleteQuery(QueryAndResultsControl queryAndResultsControl)
        {
            _queriesListBox.SelectedIndexChanged -= HandleListSelectionChanged;

            try
            {
                if (queryAndResultsControl.IsReadOnly)
                {
                    return;
                }

                // Only delete if they confirm when prompted.
                if (MessageBox.Show("Are you sure you want to delete the query \""
                        + queryAndResultsControl.Name + "\"?", "Delete query",
                            MessageBoxButtons.OKCancel, MessageBoxIcon.Warning,
                            MessageBoxDefaultButton.Button1, 0) == DialogResult.OK)
                {
                    if (File.Exists(queryAndResultsControl.FileName))
                    {
                        FileSystemMethods.DeleteFile(queryAndResultsControl.FileName);
                    }

                    // If the query is currently open, close it.
                    TabbedDocument documentWindow = queryAndResultsControl.Parent as TabbedDocument;
                    if (documentWindow != null)
                    {
                        if (documentWindow == _primaryTab)
                        {
                            ClearPrimaryTab();
                        }
                        else
                        {
                            documentWindow.Close();
                        }
                    }

                    // Remove the query control from the lists and dispose of it.
                    if (_contextMenuItem == queryAndResultsControl)
                    {
                        _contextMenuItem = null;
                    }
                    _queryList.Remove(queryAndResultsControl);
                    _pendingQueryList.Remove(queryAndResultsControl);
                    queryAndResultsControl.Dispose();
                    _queriesBindingSource.ResetBindings(false);

                    // Activate the primary tab to ensure the table/query list selection gets updated.
                    if (_primaryTab != null)
                    {
                        _primaryTab.Activate();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI34625");
            }
            finally
            {
                _queriesListBox.SelectedIndexChanged += HandleListSelectionChanged;
            }
        }

        /// <summary>
        /// Method creates a transaction and saves all of the changes to the database and if there 
        /// were no errors on save, displays a dialog indicating a successful save.
        /// </summary>
        /// <returns><see langword="true"/> if the database was successfully saved; otherwise,
        /// <see langword="false"/>.</returns>
        bool SaveDatabaseChanges()
        {
            // Check for a loaded database file.
            if (string.IsNullOrEmpty(_databaseFileName))
            {
                return true;
            }

            // Ensure any active edits are committed.
            foreach (var queryAndResultsControl in _tableList.Union(_pluginList))
            {
                queryAndResultsControl.EndDataEdit();
            }

            var invalidQueryAndResultsControl = _tableList.Union(_pluginList)
                .Where(control => control.IsLoaded && !control.DataIsValid)
                .FirstOrDefault();

            if (invalidQueryAndResultsControl != null)
            {
                OpenTableOrQuery(invalidQueryAndResultsControl, false, true);
                string errorText = invalidQueryAndResultsControl.ShowInvalidData();
                var ee = new ExtractException("ELI34626", "Unable to save; \"" +
                    invalidQueryAndResultsControl.DisplayName + "\" has invalid data.");
                ee.AddDebugData("Error", errorText, false);
                ee.Display();

                return false;
            }

            // Display wait cursor while saving the changes
            using (new TemporaryWaitCursor())
            {
                // Ensure that the main database file has not been edited since it was last
                // opened/saved. If it has, prompt for whether to overwrite the changes from
                // whatever else opened the database.
                if (File.GetLastWriteTime(_databaseFileName) > _databaseLastModifiedTime)
                {
                    using (CustomizableMessageBox messageBox = new CustomizableMessageBox())
                    {
                        messageBox.Caption = "Conflict";
                        messageBox.StandardIcon = MessageBoxIcon.Warning;
                        messageBox.Text = "The database has been modified by an another " +
                            "application. Do you want to overwrite those changes?";
                        messageBox.AddButton("Overwrite", "Overwrite", false);
                        messageBox.AddButton("Cancel", "Cancel", true);
                        if (messageBox.Show(this) == "Cancel")
                        {
                            return false;
                        }
                    }
                }

                // The database is opened without any sharing, so it needs to be closed before
                // copying the working copy back over the main database file.
                _connection.Close();

                FileAttributes originalFileAttributes = File.GetAttributes(_databaseFileName);
                FileSystemMethods.PerformFileOperationWithRetry(() =>
                    File.Copy(_databaseWorkingCopyFileName, _databaseFileName, true),
                    true);
                // Apply the same attributes the primary database had originally.
                File.SetAttributes(_databaseFileName, originalFileAttributes);

                // [DotNetRCAndUtils:825]
                ExecutePostSaveScript(_databaseFileName + ".bat");
                ExecutePostSaveScript(_databaseFileName + ".vbs");

                _databaseLastModifiedTime = File.GetLastWriteTime(_databaseFileName);

                _connection.Open();

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
            }

            return true;
        }

        /// <summary>
        /// Executes the specified <see paramref="scriptFileName"/>.
        /// </summary>
        /// <param name="scriptFileName">The name of the script file to execute. If there is no file
        /// by this name, this call has no effect.</param>
        void ExecutePostSaveScript(string scriptFileName)
        {
            try
            {
                if (File.Exists(scriptFileName))
                {
                    using (var process = new Process())
                    {
                        process.StartInfo = new ProcessStartInfo(scriptFileName);
                        process.StartInfo.WorkingDirectory = Path.GetDirectoryName(scriptFileName);
                        process.Start();
                        process.WaitForExit();
                    }

                    // [DotNetRCAndUtils:833]
                    // If the script launches a window of its own, it can result in another
                    // application being active after it finishes. Ensure this application is still
                    // active.
                    Activate();
                }
            }
            catch (Exception ex)
            {
                var ee = new ExtractException("ELI34654", "Failed to execute post-save script.", ex);
                ee.AddDebugData("Script filename", scriptFileName, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Method opens a connection to the database.
        /// </summary>
        /// <param name="databaseToOpen">The database file to open.</param>
        void OpenDatabase(string databaseToOpen)
        {
            try
            {
                // If same database is open need to save the current table
                string currentlyOpenTable = "";
                if (databaseToOpen == _databaseFileName)
                {
                    // reopening the current database.
                    currentlyOpenTable = _tablesListBox.Text;
                }

                // Reset the schema updater
                _schemaManager = null;

                // Close the database
                CloseDatabase();

                // Set the _databaseFileName
                _databaseFileName = databaseToOpen;

                // [DotNetRCAndUtils:808]
                // Copy the database file to a separate, temporary filename to ensure the primary
                // database file can be used by our applications at the same time it is being edited.
                _databaseWorkingCopyFileName = Path.Combine(
                    Path.GetDirectoryName(_databaseFileName),
                    "~" + Path.GetFileName(_databaseFileName));

                if (File.Exists(_databaseWorkingCopyFileName))
                {
                    try
                    {
                        // If there is a previous copy of the working copy, if it can be deleted
                        // the instance that created it is not still open and it can therefore
                        // be ignored.
                        FileSystemMethods.DeleteFileNoRetry(_databaseWorkingCopyFileName);
                    }
                    catch
                    {
                        // But if it couldn't be deleted, another instance of SQLCDBEditor likely
                        // already has the database open for editing; prevent it from being opened.
                        ExtractException ee = new ExtractException("ELI34542",
                            "This database is currently being edited by another process.");
                        ee.AddDebugData("Database Filename", _databaseFileName, false);

                        // Set _databaseWorkingCopyFileName to null to ensure this instance doesn't
                        // try to delete a working copy another instance created.
                        _databaseWorkingCopyFileName = null;

                        throw ee;
                    }
                }

                // If this is an SQL Compact version it needs to be converted or an existing sqlite version
                // needs to be opened instead.
                if (OpenSQLCompactAsSQLite())
                {
                    return;
                }

                // Create a new working copy.
                FileSystemMethods.PerformFileOperationWithRetry(() =>
                    File.Copy(_databaseFileName, _databaseWorkingCopyFileName),
                    true);
                File.SetAttributes(_databaseWorkingCopyFileName, FileAttributes.Hidden);
                _databaseLastModifiedTime = File.GetLastWriteTime(_databaseFileName);

                try
                {
                    _connection = new SQLiteConnection(
                        SqliteMethods.BuildConnectionString(_databaseWorkingCopyFileName));

                    // Open the connection.
                    _connection.Open();
                }
                catch (Exception ex)
                {
                    // If we failed to open a new database connection, delete the working database
                    // file and set _databaseWorkingCopyFileName to null to ensure this instance
                    // doesn't end up trying to delete a working copy created by another instance.
                    // Verify the file exists before attempting to delete it
                    if (File.Exists(_databaseWorkingCopyFileName))
                    {
                        FileSystemMethods.DeleteFile(_databaseWorkingCopyFileName);
                    }
                    _databaseWorkingCopyFileName = null;
                    throw ex.AsExtract("ELI39234");
                }

                CheckSchemaVersionAndPromptForUpdate();

                if (!_customerSchemaUpdating)
                {
                    _customerSchemaUpdating = true;
                    CheckCustomerSchemaVersionAndPromptForUpdate(databaseToOpen);
                    OpenDatabase(databaseToOpen);
                    _customerSchemaUpdating = false;
                    return;
                }

                UsingUIReplacement =
                    _schemaManager != null &&
                    _schemaManager.UIReplacementPlugins != null &&
                    _schemaManager.UIReplacementPlugins.Any();

                if (UsingUIReplacement)
                {
                    foreach (var plugin in _schemaManager.UIReplacementPlugins
                        .Cast<SQLCDBEditorPlugin>())
                    {
                        var pluginControl = new QueryAndResultsControl(plugin);

                        // If the schema is being updated don't load the tables
                        if (!_updatingSchema)
                        {
                            OpenTableOrQuery(pluginControl, true, false);
                        }
                        _pluginList.Add(pluginControl);
                    }
                }
                else
                {
                    LoadTableList();
                    LoadQueryList();
                    LoadPluginList();

                    // If this database was previously open reopen the same table
                    if (!string.IsNullOrEmpty(currentlyOpenTable))
                    {
                        _tablesListBox.SelectedIndex = _tablesListBox.FindStringExact(currentlyOpenTable);
                    }
                    else if (_tableList.Count > 0)
                    {
                        _tablesListBox.SelectedIndex = 0;
                    }

                    _tablesListBox.Focus();
                }

                // Reset the _dirty flag
                _dirty = false;

                SetWindowTitle();

                // Update menu and tool strip
                EnableCommands();
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

        /// Prompt to convert and open current SQL compact database to SQLite or open exisiting SQLite version instead.
        /// <returns>True if current database was SQL compact and SQLite version was opened instead; false otherwise.</returns>
        bool OpenSQLCompactAsSQLite()
        {
            if (Path.GetExtension(_databaseFileName).Equals(".sdf", StringComparison.OrdinalIgnoreCase))
            {
                var convertedDatabaseFileName = Path.ChangeExtension(_databaseFileName, ".sqlite");
                if (File.Exists(_databaseFileName) && !File.Exists(convertedDatabaseFileName))
                {
                    using CustomizableMessageBox messageBox = new()
                    {
                        Caption = "Convert database?",
                        Text = $"The database \"{_databaseFileName}\" is an SQL Compact database that needs to be converted to SQLite before use." +
                                "\r\n\r\nConvert now?",
                        StandardIcon = MessageBoxIcon.Warning
                    };
                    messageBox.AddStandardButtons(MessageBoxButtons.OKCancel);
                    ExtractException.Assert("ELI51883", "SQL Compact database conversion aborted",
                        messageBox.Show() == "Ok",
                        "Database", _databaseFileName);

                    DatabaseConverter.ConvertDatabaseIfNeeded(convertedDatabaseFileName).GetAwaiter().GetResult();
                    OpenDatabase(convertedDatabaseFileName);
                    return true;
                }
                else
                {
                    using CustomizableMessageBox messageBox = new()
                    {
                        Caption = "Open SQLite version?",
                        Text = "NOTICE: This is an unsupported SQLCompact database for which an SQLite database exists!" +
                            "\r\n\r\nOpen the SQLite version?",
                        StandardIcon = MessageBoxIcon.Warning
                    };
                    messageBox.AddStandardButtons(MessageBoxButtons.OKCancel);
                    ExtractException.Assert("ELI51884", "Unsupported SQL Compact database format",
                        messageBox.Show() == "Ok",
                        "Database", _databaseFileName);
                    OpenDatabase(convertedDatabaseFileName);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// This always assumes the settings table exists based on other code checks!
        /// </summary>
        /// <returns>Returns the current schema version.</returns>
        static int GetCustomerSchemaVersion(string databaseFileName)
        {
            int customerSchemaVersion = 0;

            using SQLiteConnection connection = new(
                SqliteMethods.BuildConnectionString(databaseFileName));
            connection.Open();
            
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT Value FROM Settings WHERE Name = 'CustomerSchemaVersion'";
            if (command.ExecuteScalar() is string versionString)
            {
                bool success = int.TryParse(versionString, out customerSchemaVersion);
                if (!success)
                {
                    customerSchemaVersion = 0;
                }
            }
            else
            {
                command.CommandText = "INSERT INTO Settings(Name, Value) VALUES('CustomerSchemaVersion', 0)";
                command.ExecuteNonQuery();
            }

            return customerSchemaVersion;
        }

        /// <summary>
        /// Updates the customers schema based on the schema files passed, and creates a backup.
        /// </summary>
        /// <param name="updateFiles">The files to run</param>
        static void UpdateCustomerSchema(string databaseFile, string[] updateFiles)
        {
            try
            {
                File.Copy(databaseFile, databaseFile + $"{DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss", CultureInfo.InvariantCulture)}.backup");
                using SQLiteConnection connection = new(
                    SqliteMethods.BuildConnectionString(databaseFile));
                connection.Open();

                foreach (string file in updateFiles)
                {
                    string version = GetNumberFromFileName(file).ToString(CultureInfo.InvariantCulture);
                    try
                    {
                        using var command = connection.CreateCommand();
                        command.CommandText = File.ReadAllText(file);
                        command.ExecuteNonQuery();

                        using var command2 = connection.CreateCommand();
                        command2.CommandText = $"UPDATE Settings SET Value = {version} WHERE Name = 'CustomerSchemaVersion'";
                        command2.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        throw new ExtractException("ELI54271", $"Failed to update customer schema to version {version}", ex);
                    }
                }
            }
            catch (Exception e)
            {
                throw new ExtractException("ELI47070", "Customer schema update failed", e);
            }
        }

        /// <summary>
        /// Returns the first number from a FilePath.
        /// </summary>
        /// <param name="toParse">The file path to get the number out of the file name.</param>
        /// <returns></returns>
        static int GetNumberFromFileName(string toParse)
        {
            return int.Parse(Regex.Match(Path.GetFileName(toParse), @"\d+").Value, CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Compares two strings.
        /// </summary>
        /// <param name="s1">The first string to compare</param>
        /// <param name="s2">The second string to compare</param>
        /// <returns>Returns an int representing the larger of the strings for sorting.</returns>
        static int CompareStrings(string s1, string s2)
        {
            return GetNumberFromFileName(s1) > GetNumberFromFileName(s2) ? 1 : GetNumberFromFileName(s1) < GetNumberFromFileName(s2) ? -1 : 0;
        }

        /// <summary>
        /// Checks to see if an integer array is sequential.
        /// </summary>
        /// <param name="array">The array to check</param>
        /// <returns>Returns true if the array is sequential, otherwise false.</returns>
        static bool IsSequential(int[] array)
        {
            return array.Zip(array.Skip(1), (a, b) => (a + 1) == b).All(x => x);
        }

        /// <summary>
        /// Checks the customer schema version, and updates the schema if files are present for an update.
        /// </summary>
        /// <param name="databaseFile">The database file to check for updates.</param>
        void CheckCustomerSchemaVersionAndPromptForUpdate(string databaseFile)
        {
            try
            {
                string directory = Path.GetDirectoryName(databaseFile) + "\\" + Path.GetFileNameWithoutExtension(databaseFile) + "_CustomerSchemaUpdates";
                if (!Directory.Exists(directory))
                {
                    return;
                }

                CreateSettingsTableIfMissing();
                CloseDatabase();
                int customerSchemaVersion = GetCustomerSchemaVersion(databaseFile);
                var regex = new Regex(@"([0-9])+.*\.sql(ce)?");
                string[] updateCustomerSchemaFiles = Directory.GetFiles(directory, "*.sql*")
                                                              .Where(m => regex.IsMatch(m) && GetNumberFromFileName(m) > customerSchemaVersion)
                                                              .ToArray<string>();
                Array.Sort(updateCustomerSchemaFiles, CompareStrings);
                int maxSchemaVersionFile = updateCustomerSchemaFiles.Length > 0 ? GetNumberFromFileName(updateCustomerSchemaFiles[updateCustomerSchemaFiles.Length - 1]) : 0;

                if (customerSchemaVersion < maxSchemaVersionFile)
                {
                    var result = MessageBox.Show("Database schema has an update, would you like to update now?"
                                                , "Customer Schema out of date"
                                                , MessageBoxButtons.YesNo
                                                , MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, 0);
                    if (result == DialogResult.Yes)
                    {
                        if (!IsSequential(new[] { customerSchemaVersion }.Concat(updateCustomerSchemaFiles.Select(m => GetNumberFromFileName(m))).ToArray()))
                        {
                            UtilityMethods.ShowMessageBox(
                                "You must have sequential update files." +
                                 Invariant($" Ensure you have the update files {customerSchemaVersion + 1} - {maxSchemaVersionFile}")
                                ,"Missing update files", true);
                            return;
                        }

                        UpdateCustomerSchema(databaseFile, updateCustomerSchemaFiles);
                    }
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI54270", ex);
            }
        }

        private void CreateSettingsTableIfMissing()
        {
            if (!TableNames.Contains("Settings"))
            {
                File.Copy(_databaseFileName, _databaseFileName + $"{DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss", CultureInfo.InvariantCulture)}.backup");

                using SQLiteConnection connection = new(
                    SqliteMethods.BuildConnectionString(_databaseFileName));
                connection.Open();

                using var command = connection.CreateCommand();
                command.CommandText = @"CREATE TABLE Settings (
                        Name  NVARCHAR (100) NOT NULL COLLATE NOCASE,
                        Value NVARCHAR (512) COLLATE NOCASE,
                        CONSTRAINT PK__Settings__00000000000000D9 PRIMARY KEY (Name))";
                command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Checks the schema version and prompts the user if an update is required.
        /// <para><b>Note:</b></para>
        /// This will also update the status bar in the UI to indicate known schema,
        /// unknown schema, update needed.
        /// </summary>
        void CheckSchemaVersionAndPromptForUpdate()
        {
            _schemaManager = GetSchemaUpdater();
            string statusText = "Database schema is current.";
            Color textColor = Color.Green;
            bool promptForUpdate = false;
            bool canUpdateSchema = false;
            if (_schemaManager != null)
            {
                _schemaManager?.SetDatabase(_databaseWorkingCopyFileName);
                if (_schemaManager.IsUpdateRequired)
                {
                    promptForUpdate = true;
                    canUpdateSchema = true;
                    textColor = Color.Black;
                    statusText = "Database schema requires updating.";
                }
                else if (_schemaManager.IsNewerVersion)
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
            _databaseStatusLabel.ForeColor = textColor;
            _databaseStatusLabel.Text = statusText;

            if (promptForUpdate)
            {
                var result = MessageBox.Show(
                    "Database schema requires an update, would you like to update now?",
                    "Schema out of date", MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, 0);
                if (result == DialogResult.Yes)
                {
                    _updatingSchema = true;
                    // Invoking the method here so that the update can be started after
                    // this method finishes execution
                    this.SafeBeginInvoke("ELI35348", () => { UpdateToCurrentSchema(); });
                }
            }
        }

        /// <summary>
        /// Method sets the windows title as SQLCDBEditor if no database file is loaded and if a 
        /// database file is loaded appends the filename preceded by an * if the changes have been
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
        /// <returns>Return value is <see langword="false"/> if user selected cancel in the dialog.</returns>
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
                    if (!SaveDatabaseChanges())
                    {
                        return false;
                    }
                }
                else if (result == DialogResult.Cancel)
                {
                    // If cancel was selected there is nothing further to do
                    return false;
                }
            }

            // Prompt for any dirty queries to be saved.
            return PromptAndSaveDirtyQueries(_queryList.Union(_pendingQueryList).ToArray());
        }

        /// <summary>
        /// If any of the specified <see paramref="queryControls"/> have unsaved changes to queries,
        /// prompt to save them.
        /// </summary>
        /// <returns>Return value is <see langword="false"/> if user selected cancel in the prompt.
        /// </returns>
        bool PromptAndSaveDirtyQueries(params QueryAndResultsControl[] queryControls)
        {
            IEnumerable<QueryAndResultsControl> dirtyQueryControls =
                queryControls.Where(query =>
                    query.QueryAndResultsType == QueryAndResultsType.Query && query.IsQueryDirty);

            if (dirtyQueryControls.Count() > 0)
            {
                string caption = (dirtyQueryControls.Count() == 1) ? "Save query?" : "Save queries?";
                string prompt = (dirtyQueryControls.Count() == 1)
                    ? "The query \"" + dirtyQueryControls.First().Name +
                        "\" has unsaved changes. Do you wish to save it?"
                    : "Multiple queries have unsaved changes. Do you wish to save them?";

                DialogResult result = MessageBox.Show(this, prompt, caption, MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question, MessageBoxDefaultButton.Button1, 0);
                switch (result)
                {
                    case DialogResult.Yes:
                        {
                            foreach (QueryAndResultsControl dirtyQueryControl in dirtyQueryControls)
                            {
                                dirtyQueryControl.SaveQuery();
                            }
                        }
                        break;

                    case DialogResult.No:
                        {
                            // Mark the query as not dirty to ensure multiple prompts aren't
                            // displayed for the queries as the database is closing.
                            foreach (QueryAndResultsControl dirtyQueryControl in dirtyQueryControls)
                            {
                                dirtyQueryControl.IsQueryDirty = false;
                            }
                        }
                        break;

                    case DialogResult.Cancel:
                        {
                            return false;
                        }
                }
            }

            return true;
        }

        /// <summary>
        /// Method enables or disables commands based on the _dirty flag and whether _databaseFileName 
        /// is an empty string indicating there is no database loaded.
        /// </summary>
        void EnableCommands()
        {
            bool databaseOpen = !string.IsNullOrEmpty(_databaseFileName);

            _closeToolStripMenuItem.Enabled = databaseOpen;
            _saveToolStripButton.Enabled = _dirty;
            _saveToolStripMenuItem.Enabled = _dirty;
            _newQueryToolStripButton.Enabled = databaseOpen;
            _newQueryToolStripMenuItem.Enabled = databaseOpen;
            _updateToCurrentSchemaToolStripMenuItem.Enabled =
                _schemaManager != null
                && _schemaManager.IsUpdateRequired;

            _importToolStripMenuItem.Enabled = databaseOpen;
            _exportToolStripMenuItem.Enabled = databaseOpen;
        }

        /// <summary>
        /// Method closes the database
        /// </summary>
        void CloseDatabase()
        {
            foreach (TabbedDocument tabbedDocument in _sandDockManager.GetDockControls()
                .OfType<TabbedDocument>())
            {
                if (tabbedDocument == _primaryTab)
                {
                    ClearPrimaryTab();
                }
                else
                {
                    tabbedDocument.AllowClose = true;
                    tabbedDocument.Close();
                }
            }

            _tableNames = null;
            CollectionMethods.ClearAndDispose(_tableList);
            CollectionMethods.ClearAndDispose(_queryList);
            CollectionMethods.ClearAndDispose(_pluginList);
            _tablesBindingSource.ResetBindings(false);
            _queriesBindingSource.ResetBindings(false);
            _pluginsBindingSource.ResetBindings(false);

            CollectionMethods.ClearAndDispose(_pendingQueryList);

            // Clear database file name
            _databaseFileName = "";

            // Clear Dirty Flag
            _dirty = false;

            _databaseStatusLabel.Text = "";

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

            if (!string.IsNullOrEmpty(_databaseWorkingCopyFileName) &&
                File.Exists(_databaseWorkingCopyFileName))
            {
                FileSystemMethods.DeleteFile(_databaseWorkingCopyFileName);
                _databaseWorkingCopyFileName = null;
            }

            UpdatePluginPaneVisibility();

            SetWindowTitle();
        }

        /// <summary>
        /// Attempts to get the <see cref="IDatabaseSchemaManager"/> for known schemas.
        /// </summary>
        ISqliteDatabaseManager GetSchemaUpdater()
        {
            ISqliteDatabaseManager updater = null;
            
            if (TableNames.Contains("Settings"))
            {
                // Setup dataAdapter to get the data
                DbProviderFactory providerFactory = DBMethods.GetDBProvider(_connection);
                using (var adapter = providerFactory.CreateDataAdapter())
                using (var table = new DataTable())
                {
                    adapter.SelectCommand = DBMethods.CreateDBCommand(_connection,
                        "SELECT * FROM Settings", null);
                    table.Locale = CultureInfo.CurrentCulture;

                    // Fill the table with the data from the dataAdapter
                    adapter.Fill(table);

                    // Look for the schema manager
                    var result = table.Select("Name = '"
                        + DatabaseHelperMethods.DatabaseSchemaManagerKey + "'");
                    if (result.Length == 1)
                    {
                        // Build the name to the assembly containing the manager
                        var className = result[0]["Value"].ToString();
                        updater = UtilityMethods.CreateTypeFromTypeName(className) as ISqliteDatabaseManager;

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
                        if (TableNames.Contains("FPSFile"))
                        {
                            updater = (ISqliteDatabaseManager)UtilityMethods.CreateTypeFromTypeName(
                                "Extract.FileActionManager.Database.FAMServiceDatabaseManager");
                        }
                    }
                }
            }
            else
            {
                // Check for expected LabDE order mapper tables
                if (TableNames.Contains("LabOrder") && TableNames.Contains("LabTest")
                    && TableNames.Contains("LabOrderTest") && TableNames.Contains("AlternateTestName"))
                {
                    updater = (ISqliteDatabaseManager)UtilityMethods.CreateTypeFromTypeName(
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
                        _schemaManager.SetDatabase(tempName);
                        try
                        {
                            var task = _schemaManager.UpdateToLatestSchema();

                            try
                            {
                                task.Wait();
                            }
                            finally
                            {
                                task.Dispose();
                            }
                        }
                        finally
                        {
                            eventHandle.Set();
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
                this.SafeBeginInvoke("ELI35349", () =>
                {
                    MessageBox.Show(this, "Database has been updated to current schema.",
                        "Database Updated", MessageBoxButtons.OK, MessageBoxIcon.Information,
                        MessageBoxDefaultButton.Button1, 0);
                    _updatingSchema = false;
                    OpenDatabase(tempName);
                });
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI31167", ex);
            }
        }

        /// <summary>
        /// Initializes the specified <see paramref="queryAndResultsControl"/> as a new query.
        /// </summary>
        /// <param name="queryAndResultsControl">The query and results control to initialize.</param>
        void InitializeNewQuery(QueryAndResultsControl queryAndResultsControl)
        {
            // Generate a new name based on the table or query it was copied from plus a suffixed
            // number.
            string databasePath = Path.GetDirectoryName(_databaseFileName);
            string queryNameBase = queryAndResultsControl.Name;

            // If queryNameBase is already suffixed with a digit, increment that number rather than
            // appending another one.
            int i = queryAndResultsControl.IsQueryBlank ? 1 : 2;
            Regex digitSuffixFinder = new Regex(@"\s*\d+\s*$");
            Match match = digitSuffixFinder.Match(queryNameBase);
            if (match.Success)
            {
                int num = int.Parse(match.ToString(), CultureInfo.InvariantCulture);
                if (num < 999)
                {
                    i = num + 1;
                    queryNameBase = digitSuffixFinder.Replace(queryNameBase, "");
                }
            }

            // Test and increment the number until a unique name is found.
            string queryName = "";
            do
            {
                ExtractException.Assert("ELI34628", "Unable to create new query name.", i < 1000);

                queryName = string.Join(" ", queryNameBase, i.ToString(CultureInfo.InvariantCulture));
                i++;
            }
            while (ExistingQueryNames.Contains(queryName));

            queryAndResultsControl.Name = queryName;
            queryAndResultsControl.FileName = Path.Combine(databasePath, queryName + ".sqlce");

            queryAndResultsControl.PropertyChanged += HandleQueryPropertyChanged;
            queryAndResultsControl.QuerySaved += HandleQuerySaved;
            queryAndResultsControl.QueryRenaming += HandleQueryRenaming;
            queryAndResultsControl.SentToSeparateTab += HandleTableOrQuerySentToSeparateTab;
            queryAndResultsControl.QueryCreated += HandleQueryCreated;
            queryAndResultsControl.DataChanged += HandleDataChanged;

            if (!queryAndResultsControl.IsLoaded)
            {
                queryAndResultsControl.LoadQuery(_connection, "");
            }

            _pendingQueryList.Add(queryAndResultsControl);
            
            OpenTableOrQuery(queryAndResultsControl, true, true);
        }

        /// <summary>
        /// Gets the existing query names (both saved and pending).
        /// </summary>
        HashSet<string> ExistingQueryNames
        {
            get
            {
                string databasePath = Path.GetDirectoryName(_databaseFileName);

                HashSet<string> existingNames = new HashSet<string>(
                    _queryList.Select(query => query.Name)
                    .Union(_pendingQueryList.Select(query => query.Name))
                    .Union(Directory.EnumerateFiles(databasePath, "*.sqlce")
                        .Select(file => Path.GetFileNameWithoutExtension(file))));

                return existingNames;
            }
        }

        /// <summary>
        /// Closes any control open in the primary tab.
        /// </summary>
        void ClearPrimaryTab()
        {
            if (_primaryTab != null)
            {
                _primaryTab.Controls.Clear();
                _primaryTab.TabText = "";
                _primaryTab.AllowClose = false;

                _tablesListBox.ClearSelected();
                _queriesListBox.ClearSelected();
            }
        }

        /// <summary>
        /// Updates the plugin pane visibility based upon whether there are any available plugins.
        /// </summary>
        void UpdatePluginPaneVisibility()
        {
            bool shouldBeVisible =
                !string.IsNullOrEmpty(_databaseFileName) && (_pluginList.Count > 0);

            if (shouldBeVisible != (_pluginDockableWindow.DockSituation != DockSituation.None))
            {
                if (shouldBeVisible)
                {
                    _pluginDockableWindow.Open();
                    _pluginDockableWindow.Closing += HandleDockWindowClosing;
                }
                else
                {
                    _pluginDockableWindow.Closing -= HandleDockWindowClosing;
                    _pluginDockableWindow.Close();
                }
            }
        }

        /// <summary>
        /// Applies the active status message for <see paramref="queryAndResultsControl"/> (if any)
        /// to <see cref="_dataStatusLabel"/>.
        /// </summary>
        /// <param name="queryAndResultsControl">The query and results control.</param>
        void ApplyStatusMessage(QueryAndResultsControl queryAndResultsControl)
        {
            StatusMessageChangedEventArgs statusEventArgs = null;
            if (queryAndResultsControl != null &&
                _activeStatusMessages.TryGetValue(queryAndResultsControl, out statusEventArgs))
            {
                _dataStatusLabel.Text = statusEventArgs.StatusMessage;
                _dataStatusLabel.ForeColor = statusEventArgs.TextColor;
            }
            else
            {
                _dataStatusLabel.Text = "";
                _dataStatusLabel.ForeColor = Color.Black;
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

        #endregion Properties

    }
}
