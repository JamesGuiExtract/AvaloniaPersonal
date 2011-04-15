using Extract.Encryption;
using Extract.Licensing;
using Extract.Utilities;
using System;
using System.ComponentModel;
using System.Windows.Forms;
using UCLID_FILEPROCESSINGLib;

namespace Extract.FileActionManager.FileProcessors
{
    internal partial class EncryptDecryptFileTaskSettingsDialog : Form
    {
        #region Fields

        /// <summary>
        /// Indicates that the password field has changed.
        /// </summary>
        bool _passwordChanged;

        /// <summary>
        /// Indicates whether or not the passowrd was set before the UI was displayed.
        /// If the password was set then the user cannot change between encrypt and
        /// decrypt actions without changing the password.
        /// </summary>
        bool _passwordSet;

        #endregion Fields

        /// <summary>
        /// Initializes a new instance of the <see cref="EncryptDecryptFileTaskSettingsDialog"/> class.
        /// </summary>
        public EncryptDecryptFileTaskSettingsDialog()
            : this(new EncryptDecryptFileTask())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EncryptDecryptFileTaskSettingsDialog"/> class.
        /// </summary>
        /// <param name="task">The task to copy the initial settings from.</param>
        public EncryptDecryptFileTaskSettingsDialog(EncryptDecryptFileTask task)
        {
            InitializeComponent();
            var tagManager = new FileActionManagerPathTags();
            _pathTagsInput.PathTags = tagManager;
            _pathTagsDestination.PathTags = tagManager;

            InputFile = task.InputFile;
            DestinationFile = task.DestinationFile;
            OverwriteDestination = task.OverwriteDestination;
            EncryptFile = task.EncryptFile;
            _passwordSet = task.PasswordSet;
            var password = _passwordSet ? "The password has been set" : string.Empty;
            _textPassword.Text = password;
            _textPasswordConfirm.Text = password;
        }

        #region Properties

        /// <summary>
        /// Gets the input file.
        /// </summary>
        public string InputFile { get; private set; }

        /// <summary>
        /// Gets the destination file.
        /// </summary>
        public string DestinationFile { get; private set; }

        /// <summary>
        /// Gets a value indicating whether to overwrite destination file or not.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the destination file can be overwritten;
        /// otherwise, <see langword="false"/>.
        /// </value>
        public bool OverwriteDestination { get; private set; }

        /// <summary>
        /// Gets a value indicating whether to encrypt or decrupt the file.
        /// </summary>
        /// <value>
        ///	<see langword="true"/> if the input file should be encrypted;
        ///	<see langword="false"/> if the input file should be decrypted.
        /// </value>
        public bool EncryptFile { get; private set; }

        /// <summary>
        /// Gets the password.
        /// </summary>
        public byte[] Password { get; private set; }

        #endregion Properties

        #region Event Handlers

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Form.Load"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            try
            {
                base.OnLoad(e);
                _textInputFile.Text = InputFile;
                _textDestination.Text = DestinationFile;
                _checkOverwriteDestination.Checked = OverwriteDestination;
                _radioEncrypt.Checked = EncryptFile;
                _radioDecrypt.Checked = !EncryptFile;

                // Password has not changed when first loaded
                _passwordChanged = false;

                UpdateControlStates();

                // Add text changed event handlers
                // (Do not add the handler until after the first update control state is called)
                _textPassword.TextChanged += HandlePasswordTextChanged;
                _textPasswordConfirm.TextChanged += HandlePasswordTextChanged;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI32343");
            }
        }

        /// <summary>
        /// Handles the ok button clicked.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void HandleOkButtonClicked(object sender, EventArgs e)
        {
            try
            {
                if (!ValidateSettings())
                {
                    return;
                }

                InputFile = _textInputFile.Text;
                DestinationFile = _textDestination.Text;
                OverwriteDestination = _checkOverwriteDestination.Checked;
                EncryptFile = _radioEncrypt.Checked;
                if (_passwordChanged)
                {
                    Password = ExtractEncryption.GetHashedBytes(_textPassword.Text, 1, new MapLabel());
                }
                else
                {
                    Password = null;
                }

                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI32344");
            }
        }

        /// <summary>
        /// Handles the password text changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void HandlePasswordTextChanged(object sender, EventArgs e)
        {
            try
            {
                _passwordChanged = true;
                _passwordSet = false;
                bool valid = _textPassword.Text.Equals(
                    _textPasswordConfirm.Text, StringComparison.Ordinal);
                _labelPasswordError.Text = valid ? "" : "Password mismatch";
                _errorPassword.SetError(_labelPasswordError, _labelPasswordError.Text);

                UpdateControlStates();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI32345");
            }
        }

        /// <summary>
        /// Handles the input and destination text changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void HandleInputAndDestinationTextChanged(object sender, EventArgs e)
        {
            try
            {
                UpdateControlStates();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI32348");
            }
        }

        #endregion Event Handlers

        #region Methods

        /// <summary>
        /// Validates the settings.
        /// </summary>
        /// <returns><see langword="true"/> if the settings are valid.</returns>
        bool ValidateSettings()
        {
            var tagManager = new FAMTagManager();

            if (string.IsNullOrWhiteSpace(_textInputFile.Text))
            {
                UtilityMethods.ShowMessageBox("Input file must be specified.",
                    "No Input File", true);
                _textInputFile.Focus();
                return false;
            }
            else if (tagManager.StringContainsInvalidTags(_textInputFile.Text))
            {
                UtilityMethods.ShowMessageBox("Input file contains invalid tags.",
                    "Invalid Tags", true);
                _textInputFile.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(_textDestination.Text))
            {
                UtilityMethods.ShowMessageBox("Destination file must be specified.",
                    "No Destination File", true);
                _textDestination.Focus();
                return false;
            }
            else if (tagManager.StringContainsInvalidTags(_textDestination.Text))
            {
                UtilityMethods.ShowMessageBox("Destination file contains invalid tags.",
                    "Invalid Tags", true);
                _textDestination.Focus();
                return false;
            }

            if (string.IsNullOrEmpty(_textPassword.Text))
            {
                UtilityMethods.ShowMessageBox("You must specify a password.",
                    "No Password", true);
                _textPassword.Focus();
                return false;
            }
            else if (_passwordChanged
                && !_textPassword.Text.Equals(_textPasswordConfirm.Text, StringComparison.Ordinal))
            {
                UtilityMethods.ShowMessageBox("Passwords do not match.",
                    "Password Mismatch", true);
                _textPassword.Focus();
                return false;
            }

            return true;
        }

        /// <summary>
        /// Updates the enabled/disabled state of the controls
        /// </summary>
        void UpdateControlStates()
        {
            if (_passwordSet)
            {
                _radioEncrypt.Enabled = EncryptFile;
                _labelEnableEncrypt.Visible = !EncryptFile;
                _radioDecrypt.Enabled = !EncryptFile;
                _labelEnableDecrypt.Visible = EncryptFile;
            }
            else
            {
                _radioEncrypt.Enabled = true;
                _radioDecrypt.Enabled = true;
                _labelEnableEncrypt.Visible = false;
                _labelEnableDecrypt.Visible = false;
            }

            _buttonOk.Enabled = !string.IsNullOrWhiteSpace(_textInputFile.Text)
                && !string.IsNullOrWhiteSpace(_textDestination.Text)
                && !string.IsNullOrEmpty(_textPassword.Text)
                && _textPassword.Text.Equals(_textPasswordConfirm.Text, StringComparison.Ordinal);
        }

        #endregion Methods
    }
}
