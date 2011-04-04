using Extract.Utilities.Email.Properties;
using Extract.Utilities.Forms;
using System;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using UCLID_COMUTILSLib;

namespace Extract.Utilities.Email
{
    internal partial class SmtpEmailSettingsDialog : Form
    {
        #region Fields

        /// <summary>
        /// The email settings object for initializing the UI.
        /// </summary>
        SmtpEmailSettings _settings;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SmtpEmailSettingsDialog"/> class.
        /// </summary>
        public SmtpEmailSettingsDialog() : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SmtpEmailSettingsDialog"/> class.
        /// </summary>
        /// <param name="settings">The settings to configure the UI with.
        /// If <see langword="null"/> then the current global settings will be loaded.</param>
        public SmtpEmailSettingsDialog(SmtpEmailSettings settings)
        {
            try
            {
                InitializeComponent();

                _settings = settings;
                if (_settings == null)
                {
                    _settings = new SmtpEmailSettings();
                    _settings.LoadSettings(false);
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI32279");
            }
        }

        #endregion Constructors

        #region Event Handlers

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Form.Load"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            try
            {
                Icon = Resources.EmailSettings;

                _textSmtpServer.Text = _settings.Server;
                _textPort.Text = _settings.Port.ToString(CultureInfo.CurrentCulture);
                string userName = _settings.UserName;
                if (!string.IsNullOrWhiteSpace(userName))
                {
                    _checkRequireAuthentication.Checked = true;
                    _textUserName.Text = userName;
                    _textPassword.Text = _settings.Password;
                    _checkUseSsl.Checked = _settings.UseSsl;
                }

                _textSenderName.Text = _settings.SenderName;
                _textSenderEmail.Text = _settings.SenderAddress;
                _textEmailSignature.Text = _settings.EmailSignature;

                UpdateEnabledStates();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI32271");
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
                var settings = ValidateAndGetSettings();
                if (settings != null)
                {
                    // Set the correct settings level before saving.
                    settings.UserSettings = _settings.UserSettings;
                    settings.SaveSettings();
                }
                else
                {
                    return;
                }

                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI32272");
            }
        }

        /// <summary>
        /// Handles the text box text changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void HandleTextBoxTextChanged(object sender, EventArgs e)
        {
            try
            {
                UpdateEnabledStates();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI32274");
            }
        }

        /// <summary>
        /// Handles the require authentication check changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void HandleRequireAuthenticationCheckChanged(object sender, EventArgs e)
        {
            try
            {
                UpdateEnabledStates();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI32275");
            }
        }

        /// <summary>
        /// Handles the test email click.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void HandleTestEmailClick(object sender, EventArgs e)
        {
            try
            {
                var settings = ValidateAndGetSettings();
                if (settings != null)
                {
                    string addressList = string.Empty;
                    if (InputBox.Show(this, "Please enter email addresses separated by ';'",
                        "Send Test Email", ref addressList) == DialogResult.OK
                        && !string.IsNullOrWhiteSpace(addressList))
                    {
                        var addresses = addressList
                            .Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(s => s.Trim())
                            .Where(s => !string.IsNullOrEmpty(s))
                            .ToList();

                        VariantVector recipients = new VariantVector();
                        foreach (var address in addresses)
                        {
                            recipients.PushBack(address);
                        }

                        // Show wait cursor
                        using (new TemporaryWaitCursor())
                        {
                            var message = new ExtractEmailMessage();
                            message.EmailSettings = settings;
                            message.Recipients = recipients;
                            message.Body = "Test message from email configuration window.";
                            message.Subject = "This is a test!";
                            message.Send();
                        }


                        UtilityMethods.ShowMessageBox("Test message sent successfully.",
                            "Test Message Sent", false);
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI32276");
            }
        }

        #endregion Event Handlers

        #region Methods

        /// <summary>
        /// Updates the enables state of all controls
        /// </summary>
        void UpdateEnabledStates()
        {
            bool enableUserAndPass = _checkRequireAuthentication.Checked;
            _textUserName.Enabled = enableUserAndPass;
            _textPassword.Enabled = enableUserAndPass;
            _checkUseSsl.Enabled = enableUserAndPass;

            bool enableOkAndTest = !string.IsNullOrWhiteSpace(_textSmtpServer.Text)
                && !string.IsNullOrWhiteSpace(_textPort.Text)
                && !string.IsNullOrWhiteSpace(_textSenderName.Text)
                && !string.IsNullOrWhiteSpace(_textSenderEmail.Text)
                && !(enableUserAndPass && string.IsNullOrWhiteSpace(_textUserName.Text)
                    && string.IsNullOrWhiteSpace(_textPassword.Text));

            _buttonOk.Enabled = enableOkAndTest;
            _buttonTest.Enabled = enableOkAndTest;
        }

        /// <summary>
        /// Validates and gets the settings.
        /// </summary>
        /// <returns>The settings from the dialog if they are valid, otherwise
        /// <see langword="null"/>.</returns>
        SmtpEmailSettings ValidateAndGetSettings()
        {
            SmtpEmailSettings settings = new SmtpEmailSettings();

            if (string.IsNullOrWhiteSpace(_textSmtpServer.Text))
            {
                    UtilityMethods.ShowMessageBox("SMTP server must be specified.",
                        "No Server", true);
                    _textSmtpServer.Focus();
                    return null;
            }
            settings.Server = _textSmtpServer.Text;

            // Get the port
            settings.Port = _textPort.Int32Value;

            if (_checkRequireAuthentication.Checked)
            {
                // Ensure there is a username and password
                if (string.IsNullOrWhiteSpace(_textUserName.Text))
                {
                    UtilityMethods.ShowMessageBox("Use name must be specified.",
                        "No User Name", true);
                    _textUserName.Focus();
                    return null;
                }
                settings.UserName = _textUserName.Text;

                if (string.IsNullOrWhiteSpace(_textPassword.Text))
                {
                    UtilityMethods.ShowMessageBox("Password must be specified.",
                        "No Password", true);
                    _textPassword.Focus();
                    return null;
                }
                settings.Password = _textPassword.Text;

                settings.UseSsl = _checkUseSsl.Checked;
            }

            if (string.IsNullOrWhiteSpace(_textSenderName.Text))
            {
                    UtilityMethods.ShowMessageBox("Sender name must be specified.",
                        "No Sender Name", true);
                    _textSenderName.Focus();
                    return null;
            }
            settings.SenderName = _textSenderName.Text;

            string senderAddress = _textSenderEmail.Text;
            if (string.IsNullOrWhiteSpace(senderAddress))
            {
                    UtilityMethods.ShowMessageBox("Sender email address must be specified.",
                        "No Sender", true);
                    _textSenderEmail.Focus();
                    return null;
            }
            else if (!UtilityMethods.IsValidEmailAddress(senderAddress))
            {
                if (MessageBox.Show("The specified email address does not appear to conform to a valid email address form. Are you sure you want to use this address?",
                    "Possible Invalid Email", MessageBoxButtons.YesNo, MessageBoxIcon.Warning,
                    MessageBoxDefaultButton.Button1, 0) == DialogResult.No)
                {
                    _textSenderEmail.Focus();
                    return null;
                }
            }
            settings.SenderAddress = senderAddress;
            settings.EmailSignature = _textEmailSignature.Text;

            return settings;
        }

        #endregion Methods
    }
}
