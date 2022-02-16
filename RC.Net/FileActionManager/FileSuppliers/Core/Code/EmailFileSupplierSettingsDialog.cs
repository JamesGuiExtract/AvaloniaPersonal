using Extract.Email.GraphClient;
using Extract.Licensing;
using Extract.Utilities;
using System;
using System.Net;
using System.Windows.Forms;

namespace Extract.FileActionManager.FileSuppliers
{
    /// <summary>
    /// A <see cref="Form"/> to view and modify settings for an
    /// <see cref="EmailFileSupplier"/> instance.
    /// </summary>
    [CLSCompliant(false)]
    public partial class EmailFileSupplierSettingsDialog : Form
    {
        #region Constants

        /// <summary>
        /// The object name used for license validation.
        /// </summary>
        static readonly string _OBJECT_NAME =
            typeof(EmailFileSupplierSettingsDialog).ToString();

        #endregion Constants

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="EmailFileSupplierSettingsDialog"/> class.
        /// </summary>
        /// <param name="settings">The <see cref="EmailFileSupplier"/> to configure</param>
        public EmailFileSupplierSettingsDialog(EmailFileSupplier settings)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.FileActionManagerObjects, "ELI53209",
                    _OBJECT_NAME);

                Settings = settings;

                InitializeComponent();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53210");
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Property to return the configured settings
        /// </summary>
        public IEmailFileSupplier Settings
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

                _usernameTextBox.Text = Settings.UserName;
                _passwordTextBox.Text = Settings.Password.Unsecure();

                _sharedEmailAddressTextBox.Text = Settings.SharedEmailAddress;
                _inputFolderTextBox.Text = Settings.InputMailFolderName;
                _postDownloadFolderTextBox.Text = Settings.QueuedMailFolderName;

                _downloadDirectoryTextBox.Text = Settings.DownloadDirectory;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI53211");
            }
        }

        #endregion Overrides

        #region Event Handlers

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the <see cref="Control.Click"/> event.</param>
        /// <param name="e">The event data associated with the <see cref="Control.Click"/> event.
        /// </param>
        void HandleOkButtonClick(object sender, EventArgs e)
        {
            try
            {
                if (WarnIfInvalid())
                {
                    return;
                }

                Settings.UserName = _usernameTextBox.Text;
                Settings.Password = new NetworkCredential("", _passwordTextBox.Text).SecurePassword;

                Settings.SharedEmailAddress = _sharedEmailAddressTextBox.Text;
                Settings.InputMailFolderName = _inputFolderTextBox.Text;
                Settings.QueuedMailFolderName = _postDownloadFolderTextBox.Text;

                Settings.DownloadDirectory = _downloadDirectoryTextBox.Text;

                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI53212", ex);
            }
        }

        #endregion Event Handlers

        #region Private Members

        /// <summary>
        /// Displays a warning message if the user specified settings are invalid.
        /// </summary>
        /// <returns><c>true</c> if the settings are invalid; <c>false</c> if
        /// the settings are valid.</returns>
        bool WarnIfInvalid()
        {
            if (string.IsNullOrWhiteSpace(_usernameTextBox.Text))
            {
                UtilityMethods.ShowMessageBox(
                    "User name must be configured",
                    "Configuration error", true);
                _usernameTextBox.Focus();

                return true;
            }
            if (string.IsNullOrWhiteSpace(_passwordTextBox.Text))
            {
                UtilityMethods.ShowMessageBox(
                    "Password must be configured",
                    "Configuration error", true);
                _passwordTextBox.Focus();

                return true;
            }
            if (string.IsNullOrWhiteSpace(_sharedEmailAddressTextBox.Text))
            {
                UtilityMethods.ShowMessageBox(
                    "Email address must be configured",
                    "Configuration error", true);
                _sharedEmailAddressTextBox.Focus();

                return true;
            }
            if (string.IsNullOrWhiteSpace(_inputFolderTextBox.Text))
            {
                UtilityMethods.ShowMessageBox(
                    "Input folder must be configured",
                    "Configuration error", true);
                _inputFolderTextBox.Focus();

                return true;
            }
            if (string.IsNullOrWhiteSpace(_postDownloadFolderTextBox.Text))
            {
                UtilityMethods.ShowMessageBox(
                    "Post-download folder must be configured",
                    "Configuration error", true);
                _postDownloadFolderTextBox.Focus();

                return true;
            }
            if (string.IsNullOrWhiteSpace(_downloadDirectoryTextBox.Text))
            {
                UtilityMethods.ShowMessageBox(
                    "Download directory must be configured",
                    "Configuration error", true);
                _downloadDirectoryTextBox.Focus();

                return true;
            }
            return false;
        }

        #endregion Private Members
    }
}
