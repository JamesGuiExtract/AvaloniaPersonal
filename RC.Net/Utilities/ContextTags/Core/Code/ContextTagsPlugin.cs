using Extract.Licensing;
using Extract.SQLCDBEditor;
using Extract.Utilities.Forms;
using System;
using System.ComponentModel;
using System.Data.Common;
using System.Data.SqlServerCe;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace Extract.Utilities.ContextTags
{
    /// <summary>
    /// A <see cref="SQLCDBEditorPlugin"/> implementation that allows for editing of a
    /// <see cref="ContextTagDatabase"/> in a convenient, editable view.
    /// </summary>
    public class ContextTagsPlugin : SQLCDBEditorPlugin
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(ContextTagsPlugin).ToString();

        #endregion Constants

        #region Fields

        /// <summary>
        /// The <see cref="SqlCeConnection"/> for the database to be edited.
        /// </summary>
        SqlCeConnection _connection;
        
        /// <summary>
        /// A <see cref="ContextTagDatabase"/> instance representing the database.
        /// </summary>
        ContextTagDatabase _database;

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
        /// Indicates if the host is in design mode or not.
        /// </summary>
        readonly bool _inDesignMode;

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

        /// <summary>
        /// Allows plugin to initialize.
        /// </summary>
        /// <param name="pluginManager">The <see cref="ISQLCDBEditorPluginManager"/> manager for
        /// this plugin.</param>
        /// <param name="connection">The <see cref="SqlCeConnection"/> for use by the plugin.</param>
        public override void LoadPlugin(ISQLCDBEditorPluginManager pluginManager,
            SqlCeConnection connection)
        {
            try
            {
                ExtractException.Assert("ELI38034", "Null argument exception", connection != null);

                _connection = connection;

                RefreshData();

                _editContextsButton = pluginManager.GetNewButton();
                _editContextsButton.Text = "Edit Contexts";
                _editContextsButton.Click += HandleEditContextsButton_Click;
                _editContextsButton.HandleCreated += HandleEditContextsButton_HandleCreated;
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

                ExtractException.Assert("ELI38036", "Database connection is missing",
                    _connection != null);

                if (_database == null)
                {
                    _database = new ContextTagDatabase((SqlCeConnection)_connection);
                }

                if (_contextTagsView == null)
                {
                    _contextTagsView = new ContextTagsEditorViewCollection(_database);
                }

                _contextTagsView.Refresh();

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
                using (var contextEditingForm = new ContextEditingForm(_connection))
                {
                    if (contextEditingForm.ShowDialog(this) == DialogResult.OK)
                    {
                        OnDataChanged(true, true);
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI38040");
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
                // Retrieve the database's current directory, as a UNC path if possible.
                var connectionStringBuilder = new DbConnectionStringBuilder();
                connectionStringBuilder.ConnectionString = _connection.ConnectionString;

                object databaseFile = null;
                if (!connectionStringBuilder.TryGetValue("Data Source", out databaseFile) &&
                    !connectionStringBuilder.TryGetValue("DataSource", out databaseFile))
                {
                    ExtractException.ThrowLogicException("ELI38041");
                }

                string filename = (string)databaseFile;
                FileSystemMethods.ConvertToNetworkPath(ref filename, false);
                string fpsFileDir = Path.GetDirectoryName(filename);

                // Check to see if there is any context matching this directory.
                if (!_database.Context.Any(context => context.FPSFileDir == fpsFileDir))
                {
                    // If not, display a dialog that allows creating of a context for the current
                    // directory.
                    using (var createContextForm = new CreateContextForm(_connection, fpsFileDir))
                    {
                        if (createContextForm.ShowDialog(this) == DialogResult.OK)
                        {
                            OnDataChanged(true, true);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI38042");
            }
        }

        #endregion Private Members
    }
}
