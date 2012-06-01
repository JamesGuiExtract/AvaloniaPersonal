using Extract.Licensing;
using Extract.Utilities;
using Extract.Utilities.Forms;
using Microsoft.Data.ConnectionUI;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Extract.Database
{
    /// <summary>
    /// A control that allows configuration of a database connection to a number of different data
    /// sources. Includes a path tags button and obscuring of passwords.
    /// </summary>
    [CLSCompliant(false)]
    public partial class DatabaseConnectionControl : UserControl
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(DatabaseConnectionControl).ToString();

        /// <summary>
        /// The character to use to mask passwords in the connection string box.
        /// </summary>
        static readonly char _PASSWORD_CHAR = '•';

        /// <summary>
        /// The string to use to mask passwords in the connection string box.
        /// </summary>
        static readonly string _PASSWORD_MASK = "=••••••••";

        /// <summary>
        /// A <see cref="Regex"/> to find passwords in a connection string.
        /// </summary>
        static readonly Regex _PASSWORD_REGEX =
            new Regex(@"(?<=(^|[;\s])(password|pwd))=[\s\S]+?(?=($|[;]))", RegexOptions.IgnoreCase);

        /// <summary>
        /// A <see cref="Regex"/> to find data file names in a connection string.
        /// </summary>
        static readonly Regex _DATAFILE_REGEX =
            new Regex(@"(?<=(^|[;\s])(data\ssource|attachdbfilename)=)[\s\S]+?(?=($|[;]))", RegexOptions.IgnoreCase);

        #endregion Constants

        #region Fields

        /// <summary>
        /// Indicates if the host is in design mode or not.
        /// </summary>
        bool _inDesignMode;

        /// <summary>
        /// Indicates whether the controls have been positioned yet.
        /// </summary>
        bool _positionedControls;

        /// <summary>
        /// The <see cref="DataConnectionDialog"/>
        /// </summary>
        DataConnectionDialog _connectionDialog = new DataConnectionDialog();

        /// <summary>
        /// The <see cref="ErrorProvider"/> which will display an error icon when the data source
        /// is not properly configured.
        /// </summary>
        ErrorProvider _errorProvider = new ErrorProvider();

        /// <summary>
        /// The <see cref="DatabaseConnectionInfo"/> representing the connection configuration.
        /// </summary>
        DatabaseConnectionInfo _databaseConnectionInfo;

        /// <summary>
        /// The <see cref="IPathTags"/> to use for the <see cref="_pathTagsButton"/>.
        /// </summary>
        IPathTags _pathTags;

        /// <summary>
        /// Indicates whether the <see cref="_pathTagsButton"/> should be shown.
        /// </summary>
        bool _showPathTagsButton;

        /// <summary>
        /// The connection string's password (if any) including the preceeding equal sign.
        /// </summary>
        string _password;

        /// <summary>
        /// The data file referenced in the current connection string (if any).
        /// </summary>
        string _dataFile;

        /// <summary>
        /// The last known value of the _connectionStringTextBox's text.
        /// </summary>
        string _lastConnectionStringValue;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseConnectionControl"/> class.
        /// </summary>
        public DatabaseConnectionControl()
        {
            try
            {
                _inDesignMode = (LicenseManager.UsageMode == LicenseUsageMode.Designtime);

                // Load licenses in design mode
                if (_inDesignMode)
                {
                    // Load the license files from folder
                    LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());
                }

                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI34760",
                    _OBJECT_NAME);

                InitializeComponent();

                ShowPathTagsButton = true;
                ShowOpenDataFileMenuOption = true;
                DatabaseConnectionInfo = new DatabaseConnectionInfo();
                _errorProvider.BlinkStyle = ErrorBlinkStyle.NeverBlink;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI34761");
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the database connection info.
        /// </summary>
        /// <value>
        /// The database connection info.
        /// </value>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DatabaseConnectionInfo DatabaseConnectionInfo
        {
            get
            {
                return _databaseConnectionInfo;
            }

            set
            {
                try
                {
                    if (value != _databaseConnectionInfo)
                    {
                        _databaseConnectionInfo = value;

                        if (_databaseConnectionInfo != null)
                        {
                            _databaseConnectionInfo.LoadConnectionDialogConfiguration(_connectionDialog);
                        }

                        UpdateUI();
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI34762");
                }
            }
        }

        /// <summary>
        /// Gets or sets the path tags.
        /// </summary>
        /// <value>
        /// The path tags.
        /// </value>
        public IPathTags PathTags
        {
            get
            {
                return _pathTags;
            }

            set
            {
                try
                {
                    if (value != _pathTags)
                    {
                        _pathTags = value;
                        _pathTagsButton.PathTags = value;

                        if (_pathTags == null)
                        {
                            _pathTagsButton.DisplayPathTags = false;
                            _pathTagsButton.DisplayFunctionTags = false;
                        }
                        else
                        {
                            _pathTagsButton.DisplayPathTags = true;
                            _pathTagsButton.DisplayFunctionTags = true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI34763");
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to show the path tags button.
        /// </summary>
        /// <value><see langword="true"/> to show the path tags button; otherwise,
        /// <see langword="false"/>.
        /// </value>
        [DefaultValue(true)]
        public bool ShowPathTagsButton
        {
            get
            {
                return _showPathTagsButton;
            }

            set
            {
                try
                {
                    if (value != _showPathTagsButton)
                    {
                        _showPathTagsButton = value;

                        // Reposition the controls if they have already been positioned.
                        _positionedControls = false;

                        UpdateUI();
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI34764");
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether to show the open data file path tags button
        /// menu option.
        /// </summary>
        /// <value><see langword="true"/> to show the open data file path tags button menu option;
        /// otherwise, <see langword="false"/>.
        /// </value>
        [DefaultValue(true)]
        public bool ShowOpenDataFileMenuOption
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether to show the copy connection type path tags
        /// button menu option.
        /// </summary>
        /// <value><see langword="true"/> to show the copy connection type path tags button menu
        /// option; otherwise, <see langword="false"/>.
        /// </value>
        [DefaultValue(false)]
        public bool ShowCopyConnectionTypeMenuOption
        {
            get;
            set;
        }

        #endregion Properties

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

                UpdateUI();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI34765");
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
                if (components != null)
                {
                    components.Dispose();
                    components = null;
                }

                if (_errorProvider != null)
                {
                    _errorProvider.Dispose();
                    _errorProvider = null;
                }
            }
            base.Dispose(disposing);
        }

        #endregion Overrides

        #region Event Handlers

        /// <summary>
        /// Handles the <see cref="T:TextBox.TextChanged"/> event from
        /// <see cref="_connectionStringTextBox"/>.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleConnectionStringTextChanged(object sender, EventArgs e)
        {
            try
            {
                // Do not allow any edits that remove some but not all of the password mask.
                // The password must be edited with the configure connection button.
                if (!string.IsNullOrEmpty(_password))
                {
                    if (!_connectionStringTextBox.Text.Contains(_PASSWORD_MASK) &&
                        _connectionStringTextBox.Text.Contains(_PASSWORD_CHAR))
                    {
                        int selectionStart = Math.Min(_connectionStringTextBox.SelectionStart,
                            _lastConnectionStringValue.Length);
                        int selectionLength = Math.Min(_connectionStringTextBox.SelectionLength,
                            _lastConnectionStringValue.Length - _connectionStringTextBox.SelectionStart);

                        _connectionStringTextBox.Text = _lastConnectionStringValue;

                        _connectionStringTextBox.Select(selectionStart, selectionLength);

                        UtilityMethods.ShowMessageBox(
                            "The password can be edited only within the Configure Connection dialog.",
                            "Cannot edit password", false);
                    }

                    _lastConnectionStringValue = _connectionStringTextBox.Text;
                    return;
                }
                
                _lastConnectionStringValue = _connectionStringTextBox.Text;

                // Update the official connection string by substituting the password for the
                // _PASSWORD_MASK.
                DatabaseConnectionInfo.ConnectionString =
                    _connectionStringTextBox.Text.Replace(_PASSWORD_MASK, _password);

                UpdateDataSourceValidationStatus();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI34766");
            }
        }

        /// <summary>
        /// Handles the <see cref="T:Control.Click"/> event of the
        /// <see cref="_configureConnectionButton"/>.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleConfigureConnectionClicked(object sender, EventArgs e)
        {
            try
            {
                ExtractException.Assert("ELI34767", "Database connection info has not been supplied.",
                    DatabaseConnectionInfo != null);

                try
                {
                    // Ensure the official connection string has been updated with the un-masked
                    // password.
                    DatabaseConnectionInfo.ConnectionString =
                        _connectionStringTextBox.Text.Replace(_PASSWORD_MASK, _password);

                    // Initialize the dialog.
                    // If the data source or provider are null, don't specify which would overwrite
                    // the default (last used source/type).
                    if (DatabaseConnectionInfo.DataSource != null)
                    {
                        _connectionDialog.SelectedDataSource = DatabaseConnectionInfo.DataSource;
                    }
                    if (DatabaseConnectionInfo.DataProvider != null)
                    {
                        _connectionDialog.SelectedDataProvider = DatabaseConnectionInfo.DataProvider;
                    }
                    _connectionDialog.ConnectionString = DatabaseConnectionInfo.ConnectionString;
                }
                catch (Exception ex)
                {
                    // Indicate the error initializing the connection dialog.
                    if (!string.IsNullOrWhiteSpace(DatabaseConnectionInfo.ConnectionString))
                    {
                        _errorProvider.SetError(_dataSourceTextBox, ex.Message);
                    }
                }

                if (DataConnectionDialog.Show(_connectionDialog) == DialogResult.OK)
                {
                    // Apply the settings from the connection dialog back to the current connection
                    // settings and update the text controls to match.
                    DatabaseConnectionInfo.DataSource = _connectionDialog.SelectedDataSource;
                    DatabaseConnectionInfo.DataProvider = _connectionDialog.SelectedDataProvider;
                    DatabaseConnectionInfo.ConnectionString = _connectionDialog.ConnectionString;
                    DatabaseConnectionInfo.SaveConnectionDialogConfiguration(_connectionDialog);

                    UpdateUI();
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI34768");
            }
        }

        /// <summary>
        /// Handles the <see cref="T:PathTagsButton.MenuOpening"/> event of the
        /// <see cref="_pathTagsButton"/>.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="PathTagsMenuOpeningEventArgs"/> instance containing the
        /// event data.</param>
        void HandlePathTagsMenuOpening(object sender, PathTagsMenuOpeningEventArgs e)
        {
            try
            {
                // Add the opened data file and copy connection type menu items to the path tags menu
                // if so configured.
                if (DatabaseConnectionInfo != null)
                {
                    int insertIndex = 0;

                    if (ShowOpenDataFileMenuOption)
                    {
                        var menuItem =
                            new ToolStripMenuItem("Open data file", null, HandleOpenDataFile);
                        e.ContextMenuStrip.Items.Insert(insertIndex++, menuItem);

                        var matches = _DATAFILE_REGEX.Matches(_connectionStringTextBox.Text);
                        _dataFile = matches
                            .Cast<Match>()
                            .Where(match => match.Success && File.Exists(match.Value))
                            .Select(match => match.Value)
                            .FirstOrDefault();
                        menuItem.Enabled = !string.IsNullOrWhiteSpace(_dataFile);
                    }

                    if (ShowCopyConnectionTypeMenuOption)
                    {
                        var menuItem =
                            new ToolStripMenuItem("Copy connection data type to clipboard", null,
                                HandleCopyConnectionDataType);
                        e.ContextMenuStrip.Items.Insert(insertIndex++, menuItem);

                        menuItem.Enabled = (DatabaseConnectionInfo.DataProvider != null);
                    }

                    // If either of the menu options were added, insert a separator before the rest
                    // of path tags menu items.
                    if (insertIndex > 0 && e.ContextMenuStrip.Items.Count > insertIndex)
                    {
                        e.ContextMenuStrip.Items.Insert(insertIndex, new ToolStripSeparator());
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI34769");
            }
        }

        /// <summary>
        /// Handles the selection of the open data file path tags menu option.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleOpenDataFile(object sender, EventArgs e)
        {
            try
            {
                Process.Start(_dataFile);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI34772");
            }
        }

        /// <summary>
        /// Handles the selection of the copy connection type path tags menu option.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleCopyConnectionDataType(object sender, EventArgs e)
        {
            try
            {
                Clipboard.SetText(
                    DatabaseConnectionInfo.DataProvider.TargetConnectionType.AssemblyQualifiedName);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI34770");
            }
        }

        #endregion Event Handlers

        #region Private Methods

        /// <summary>
        /// Updates the UI.
        /// </summary>
        void UpdateUI()
        {
            if (DatabaseConnectionInfo == null)
            {
                // If there is no connection info, clear the text boxes.
                _dataSourceTextBox.Text = "";
                _connectionStringTextBox.Text = "";
                _password = "";
                _dataFile = "";
            }
            else
            {
                // Set the text boxes using DatabaseConnectionInfo.
                _dataSourceTextBox.Text = DatabaseConnectionInfo.DataSourceName;

                string connectionString = DatabaseConnectionInfo.ConnectionString;
                if (connectionString != null)
                {
                    // Obsure any password in the connection string.
                    Match match = _PASSWORD_REGEX.Match(connectionString);
                    if (match.Success)
                    {
                        _password = "=" + match.Value;
                        connectionString = connectionString.Remove(match.Index, match.Length);
                        connectionString = connectionString.Insert(match.Index, _PASSWORD_MASK);
                    }
                }

                _connectionStringTextBox.Text = connectionString;
            }

            // Show or hide the path tags button per ShowPathTagsButton.
            if (!_positionedControls)
            {
                if (ShowPathTagsButton)
                {
                    _pathTagsButton.PathTags = _pathTags;
                    _dataSourceTextBox.Width = _pathTagsButton.Left - 8 - _dataSourceTextBox.Left;
                    _connectionStringTextBox.Width = _dataSourceTextBox.Width;
                    _pathTagsButton.Visible = true;
                }
                else
                {
                    _pathTagsButton.DisplayFunctionTags = false;
                    _dataSourceTextBox.Width = _pathTagsButton.Right - _dataSourceTextBox.Left;
                    _connectionStringTextBox.Width = _dataSourceTextBox.Width;
                    _pathTagsButton.Visible = false;
                }

                _positionedControls = true;
            }

            UpdateDataSourceValidationStatus();
        }

        /// <summary>
        /// Updates the data source validation status.
        /// </summary>
        void UpdateDataSourceValidationStatus()
        {
            if (!string.IsNullOrEmpty(_dataSourceTextBox.Text) ||
                string.IsNullOrWhiteSpace(_connectionStringTextBox.Text))
            {
                _errorProvider.SetError(_dataSourceTextBox, "");
            }
            else
            {
                _errorProvider.SetError(_dataSourceTextBox, "Data source has not been selected.");
            }
        }

        #endregion Private Methods
    }
}
