using Extract.Utilities.Forms;
using System;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using UCLID_COMUTILSLib;

namespace Extract.Utilities.Email
{
    /// <summary>
    /// A <see cref="UserControl"/> that allows configuratoin and persistence of email settings.
    /// </summary>
    public partial class EmailSettingsControl : UserControl
    {
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="EmailSettingsControl"/> class.
        /// </summary>
        public EmailSettingsControl()
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35927");
            }
        }

        #endregion Constructors

        #region Events

        /// <summary>
        /// Raised when any of the email settings have changed.
        /// </summary>
        public event EventHandler<EventArgs> SettingsChanged;

        #endregion Events

        #region Properties

        /// <summary>
        /// Gets a value indicating whether potentially valid email settings have been entered.
        /// </summary>
        /// <value><see langword="true"/> if potentially valid email settings have been entered;
        /// <see langword="false"/> if one or more required fields have not been populated.
        /// </value>
        public bool HasSettings
        {
            get;
            private set;
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Loads <see paremref="settings"/> into the UI controls.
        /// </summary>
        public void LoadSettings(SmtpEmailSettings settings)
        {
            try
            {
                ExtractException.Assert("ELI35931", "Null argument exception", settings != null);

                _textSmtpServer.Text = settings.Server;
                _textPort.Text = settings.Port.ToString(CultureInfo.CurrentCulture);
                _checkRequireAuthentication.Checked = !string.IsNullOrEmpty(settings.UserName);
                _textUserName.Text = settings.UserName;
                _textPassword.Text = settings.Password;
                _checkUseSsl.Checked = settings.UseSsl;
                _textSenderName.Text = settings.SenderName;
                _textSenderEmail.Text = settings.SenderAddress;
                _textEmailSignature.Text = settings.EmailSignature;

                UpdateEnabledStates();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35928");
            }
        }

        /// <summary>
        /// Validates that the settings in the UI are valid.
        /// </summary>
        /// <returns><see langword="true"/> if the current values in the UI controls are valid;
        /// otherwise, <see langword="false"/>.</returns>
        public bool ValidateSettings()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_textSmtpServer.Text))
                {
                    UtilityMethods.ShowMessageBox("SMTP server must be specified.",
                        "No Server", true);
                    _textSmtpServer.Focus();
                    return false;
                }

                if (_checkRequireAuthentication.Checked)
                {
                    // Ensure there is a username and password
                    if (string.IsNullOrWhiteSpace(_textUserName.Text))
                    {
                        UtilityMethods.ShowMessageBox("User name must be specified.",
                            "No User Name", true);
                        _textUserName.Focus();
                        return false;
                    }

                    if (string.IsNullOrWhiteSpace(_textPassword.Text))
                    {
                        UtilityMethods.ShowMessageBox("Password must be specified.",
                            "No Password", true);
                        _textPassword.Focus();
                        return false;
                    }
                }

                if (string.IsNullOrWhiteSpace(_textSenderName.Text))
                {
                    UtilityMethods.ShowMessageBox("Sender name must be specified.",
                        "No Sender Name", true);
                    _textSenderName.Focus();
                    return false;
                }

                string senderAddress = _textSenderEmail.Text;
                if (string.IsNullOrWhiteSpace(senderAddress))
                {
                    UtilityMethods.ShowMessageBox("Sender email address must be specified.",
                        "No Sender", true);
                    _textSenderEmail.Focus();
                    return false;
                }
                else if (!UtilityMethods.IsValidEmailAddress(senderAddress))
                {
                    if (MessageBox.Show("The specified email address does not appear to conform " +
                        "to a valid email address form. Are you sure you want to use this address?",
                        "Possible Invalid Email", MessageBoxButtons.YesNo, MessageBoxIcon.Warning,
                        MessageBoxDefaultButton.Button1, 0) == DialogResult.No)
                    {
                        _textSenderEmail.Focus();
                        return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35937");
            }
        }

        /// <summary>
        /// Applies the settings from the UI controls to <see paramref="settings"/>.
        /// </summary>
        public void ApplySettings(SmtpEmailSettings settings)
        {
            try
            {
                settings.Server = _textSmtpServer.Text;
                settings.Port = _textPort.Int32Value;
                // There is legacy code that uses the presence or absense of a username setting to
                // determine whether authentication is required rather than persisting a separate
                // boolean.
                settings.UserName = _checkRequireAuthentication.Checked ? _textUserName.Text : "";
                settings.Password = _checkRequireAuthentication.Checked ? _textPassword.Text : "";
                settings.UseSsl = _checkRequireAuthentication.Checked ? _checkUseSsl.Checked : false;
                settings.SenderName = _textSenderName.Text;
                settings.SenderAddress = _textSenderEmail.Text;
                settings.EmailSignature = _textEmailSignature.Text;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35924");
            }
        }

        /// <summary>
        /// Attempts to send a test email.
        /// </summary>
        public void SendTestEmail()
        {
            try
            {
                if (!ValidateSettings())
                {
                    return;
                }

                SmtpEmailSettings settings = new SmtpEmailSettings();
                ApplySettings(settings);

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
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35925");
            }
        }

        #endregion Methods

        #region Event Handlers

        /// <summary>
        /// Handles the text box text changed.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void HandleTextBoxTextChanged(object sender, EventArgs e)
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
        void HandleRequireAuthenticationCheckChanged(object sender, EventArgs e)
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

        #endregion Event Handlers

        #region Private Members

        /// <summary>
        /// Updates the enables state of all controls
        /// </summary>
        void UpdateEnabledStates()
        {
            bool enableUserAndPass = _checkRequireAuthentication.Checked;
            _textUserName.Enabled = enableUserAndPass;
            _textPassword.Enabled = enableUserAndPass;
            _checkUseSsl.Enabled = enableUserAndPass;

            HasSettings = !string.IsNullOrWhiteSpace(_textSmtpServer.Text)
                && !string.IsNullOrWhiteSpace(_textPort.Text)
                && !string.IsNullOrWhiteSpace(_textSenderName.Text)
                && !string.IsNullOrWhiteSpace(_textSenderEmail.Text)
                && !(enableUserAndPass && string.IsNullOrWhiteSpace(_textUserName.Text)
                    && string.IsNullOrWhiteSpace(_textPassword.Text));

            OnSettingsChanged();
        }

        /// <summary>
        /// Raises the <see cref="SettingsChanged"/> event.
        /// </summary>
        void OnSettingsChanged()
        {
            var eventHandler = SettingsChanged;
            if (eventHandler != null)
            {
                eventHandler(this, new EventArgs());
            }
        }

        #endregion Private Members
    }
}
