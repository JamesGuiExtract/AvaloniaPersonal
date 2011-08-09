using Extract.Licensing;
using Extract.Utilities;
using System;
using System.Windows.Forms;

namespace Extract.FileActionManager.FileSuppliers
{
    /// <summary>
    /// Allows configuration of a <see cref="ContextMenuFileSupplier"/> instance.
    /// </summary>
    public partial class ContextMenuFileSupplierSettingsDialog : Form
    {
        #region Constants

        /// <summary>
        /// The object name.
        /// </summary>
        static readonly string _OBJECT_NAME =
            typeof(ContextMenuFileSupplierSettingsDialog).ToString();

        #endregion Constants

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ContextMenuFileSupplierSettingsDialog"/>
        /// class.
        /// </summary>
        /// <param name="settings">The <see cref="ContextMenuFileSupplier"/> instance to configure.</param>
        public ContextMenuFileSupplierSettingsDialog(ContextMenuFileSupplier settings)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.FileActionManagerObjects, "ELI33157",
                    _OBJECT_NAME);

                Settings = settings;

                InitializeComponent();

                _limitRootCheckBox.CheckedChanged += ((sender, args) =>
                    {
                        _rootPathTextBox.Enabled = _limitRootCheckBox.Checked;
                        _pathBrowse.Enabled = _limitRootCheckBox.Checked;
                    });
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI33158");
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the <see cref="ContextMenuFileSupplier"/> to configure.
        /// </summary>
        /// <value>
        /// The <see cref="ContextMenuFileSupplier"/> to configure.
        /// </value>
        public ContextMenuFileSupplier Settings
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

                // Apply Settings values to the UI.
                if (Settings != null)
                {
                    _menuOptionNameTextBox.Text = Settings.MenuOptionName;
                    _fileFilterComboBox.Text = Settings.FileFilter;
                    _limitRootCheckBox.Checked = Settings.LimitPathRoot;
                    _rootPathTextBox.Text = Settings.PathRoot;
                    _rootPathTextBox.Enabled = Settings.LimitPathRoot;
                    _pathBrowse.Enabled = Settings.LimitPathRoot;
                    _inclueSubfoldersCheckBox.Checked = Settings.IncludeFolders;
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI33159");
            }
        }

        #endregion Overrides

        #region Event Handlers

        /// <summary>
        /// In the case that the OK button is clicked, validates the settings, applies them, and
        /// closes.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleOkButtonClick(object sender, EventArgs e)
        {
            try
            {
                // If there are invalid settings, prompt and return without closing.
                if (WarnIfInvalid())
                {
                    return;
                }

                Settings.MenuOptionName = _menuOptionNameTextBox.Text;
                Settings.FileFilter = _fileFilterComboBox.Text;
                Settings.LimitPathRoot = _limitRootCheckBox.Checked;
                Settings.PathRoot = _rootPathTextBox.Text;
                Settings.IncludeFolders = _inclueSubfoldersCheckBox.Checked;

                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI33160");
            }
        }

        #endregion Event Handlers

        #region Private Members

        /// <summary>
        /// Displays a warning message if the user specified settings are invalid.
        /// </summary>
        /// <returns><see langword="true"/> if the settings are invalid; <see langword="false"/> if
        /// the settings are valid.</returns>
        bool WarnIfInvalid()
        {
            ExtractException.Assert("ELI33161",
                "Context menu file supplier settings have not been provided.", Settings != null);

            if (string.IsNullOrWhiteSpace(_menuOptionNameTextBox.Text))
            {
                _menuOptionNameTextBox.Focus();
                UtilityMethods.ShowMessageBox("Please specify a name for the context menu option.",
                    "Specify menu option name", false);
                return true;
            }

            if (string.IsNullOrWhiteSpace(_fileFilterComboBox.Text))
            {
                _menuOptionNameTextBox.Focus();
                UtilityMethods.ShowMessageBox("Please specify a file filter indicating for which " +
                    "files the context menu should be available.",
                    "Specify file filter", false);
                return true;
            }

            if (_limitRootCheckBox.Checked && string.IsNullOrWhiteSpace(_rootPathTextBox.Text))
            {
                _rootPathTextBox.Focus();
                UtilityMethods.ShowMessageBox(
                    "Please specify root folder for which this context menu should be available.",
                    "Specify Path Root", false);
                return true;
            }

            return false;
        }

        #endregion Private Members
    }
}
