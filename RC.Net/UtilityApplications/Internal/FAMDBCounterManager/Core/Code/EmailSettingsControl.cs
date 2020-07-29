using Extract.Licensing.Internal;
using System;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace Extract.FAMDBCounterManager
{
    /// <summary>
    /// A <see cref="UserControl"/> that allows configuration and persistence of email settings.
    /// <para><b>Note</b></para>
    /// This class is a modified copy of Extract.Utilities.Email.EmailSettingsControl. This project
    /// is not linked to Extract.Utilities.Email to avoid COM dependencies.
    /// </summary>
    internal partial class EmailSettingsControl : UserControl
    {
        #region Fields

        /// <summary>
        /// Used to validate email addresses.
        /// </summary>
        static Regex _emailValidator;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="EmailSettingsControl"/> class.
        /// </summary>
        public EmailSettingsControl()
        {
            InitializeComponent();
        }

        #endregion Constructors

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
        public void LoadSettings(EmailSettingsManager settings)
        {
            UtilityMethods.Assert(settings != null, "Null argument exception");

            _textSmtpServer.Text = settings.Server;
            _textPort.Text = settings.Port.ToString(CultureInfo.CurrentCulture);
            _checkRequireAuthentication.Checked = !string.IsNullOrEmpty(settings.UserName);
            _textUserName.Text = settings.UserName;
            _textPassword.Text = settings.Password;
            _checkUseSsl.Checked = settings.UseSsl;
            _textSenderName.Text = settings.SenderName;
            _textSenderEmail.Text = settings.SenderAddress;
            _textSubjectTemplate.Text = settings.SubjectTemplate;
            _editableBodyTextBox.Text = settings.EditableBodyTemplate;
            _readOnlyBodyTextBox.Text = settings.ReadonlyBodyTemplate;

            UpdateEnabledStates();
        }

        /// <summary>
        /// Validates that the settings in the UI are valid.
        /// </summary>
        /// <returns><see langword="true"/> if the current values in the UI controls are valid;
        /// otherwise, <see langword="false"/>.</returns>
        public bool ValidateSettings()
        {
            if (string.IsNullOrWhiteSpace(_textSmtpServer.Text))
            {
                UtilityMethods.ShowMessageBox("SMTP server must be specified.", "No Server", true);
                _textSmtpServer.Focus();
                return false;
            }

            if (_checkRequireAuthentication.Checked)
            {
                // Ensure there is a username and password
                if (string.IsNullOrWhiteSpace(_textUserName.Text))
                {
                    UtilityMethods.ShowMessageBox("User name must be specified.", "No User Name", true);
                    _textUserName.Focus();
                    return false;
                }

                if (string.IsNullOrWhiteSpace(_textPassword.Text))
                {
                    UtilityMethods.ShowMessageBox("Password must be specified.", "No Password", true);
                    _textPassword.Focus();
                    return false;
                }
            }

            if (string.IsNullOrWhiteSpace(_textSenderName.Text))
            {
                UtilityMethods.ShowMessageBox("Sender name must be specified.", "No Sender Name", true);
                _textSenderName.Focus();
                return false;
            }

            string senderAddress = _textSenderEmail.Text;
            if (string.IsNullOrWhiteSpace(senderAddress))
            {
                UtilityMethods.ShowMessageBox("Sender email address must be specified.", "No Sender", true);
                _textSenderEmail.Focus();
                return false;
            }
            else if (!IsValidEmailAddress(senderAddress))
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

        /// <summary>
        /// Applies the settings from the UI controls to <see paramref="settings"/>.
        /// </summary>
        public void ApplySettings(EmailSettingsManager settings)
        {
            settings.Server = _textSmtpServer.Text;
            settings.Port = _textPort.Int32Value;
            // There is legacy code that uses the presence or absence of a username setting to
            // determine whether authentication is required rather than persisting a separate
            // boolean.
            settings.UserName = _checkRequireAuthentication.Checked ? _textUserName.Text : "";
            settings.Password = _checkRequireAuthentication.Checked ? _textPassword.Text : "";
            settings.UseSsl = _checkRequireAuthentication.Checked ? _checkUseSsl.Checked : false;
            settings.SenderName = _textSenderName.Text;
            settings.SenderAddress = _textSenderEmail.Text;
            settings.SubjectTemplate = _textSubjectTemplate.Text;
            settings.EditableBodyTemplate = _editableBodyTextBox.Text;
            settings.ReadonlyBodyTemplate = _readOnlyBodyTextBox.Text;
        }

        /// <summary>
        /// Attempts to send a test email.
        /// </summary>
        public void SendTestEmail()
        {
            if (!ValidateSettings())
            {
                return;
            }

            EmailSettingsManager settings = new EmailSettingsManager();
            ApplySettings(settings);

            string addressList = string.Empty;
            if (InputBox.Show(this, "Please enter email addresses separated by ';'",
                "Send Test Email", ref addressList) == DialogResult.OK
                && !string.IsNullOrWhiteSpace(addressList))
            {
                var recipients = addressList
                    .Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim())
                    .Where(s => !string.IsNullOrEmpty(s))
                    .ToArray();

                settings.DatabaseInfo = new DatabaseInfo
                {
                    DatabaseID = Guid.Empty,
                    DatabaseServer = "[Database Server]",
                    DatabaseName = "[Database Name]",
                    CreationTime = DateTime.Now,
                    RestoreTime = DateTime.Now,
                    LastCounterUpdateTime = DateTime.Now,
                    DateTimeStamp = DateTime.Now
                };

                settings.CounterOperationInfo = new CounterOperationInfo
                {
                    Customer = "[Customer goes here]",
                    Comment = "[Comment goes here]",
                    Description = "[Description goes here]",
                    CodeType = "[update/unlock]",
                    Code = "[Code goes here]"
                };

                string subject = settings.GetSubject();
                string body = settings.GetEditableBody() + "\r\n" + settings.GetReadonlyBody();

                var message = new EmailMessage();
                message.EmailSettings = settings;
                message.Recipients = recipients;
                message.Subject = subject;
                message.Body = body;
                message.Send();

                UtilityMethods.ShowMessageBox(
                    "Test message sent successfully.", "Test Message Sent", false);
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
                ex.ShowMessageBox();
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
                ex.ShowMessageBox();
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
        }

        /// <summary>
        /// Determines whether <paramref name="emailAddress"/> is a valid email address.
        /// <para>
        /// The email validation regex is a modified form of the RFC 2822 standard for internet
        /// email addresses from http://www.regular-expressions.info/email.html
        /// </para>
        /// </summary>
        /// <param name="emailAddress">The email address to validate.</param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="emailAddress"/> is a valid email address;
        /// <see langword="false"/> otherwise.
        /// </returns>
        static bool IsValidEmailAddress(string emailAddress)
        {
            if (_emailValidator == null)
            {
                if (_emailValidator == null)
                {
                    _emailValidator = new Regex(@"^[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?$",
                        RegexOptions.IgnoreCase);
                }
            }

            return _emailValidator.IsMatch(emailAddress);
        }

        #endregion Private Members
    }
}
