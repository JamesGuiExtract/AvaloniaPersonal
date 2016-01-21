using Extract.Utilities.Forms;
using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using UCLID_COMUTILSLib;

namespace Extract.Utilities.Email
{
    /// <summary>
    /// A <see cref="UserControl"/> that allows configuration and persistence of email settings.
    /// </summary>
    public partial class EmailSettingsControl : UserControl
    {
        /// <summary>
        /// Disable the invalid email warning iff the user answered YES to warning. 
        /// This flag will be re-enabled iff the field is later modified.
        /// </summary>
        private bool _disableInvalidEmailWarning = false;

        /// <summary>
        /// Disable the invalid SMTP Server warning iff the user answered YES to warning. 
        /// This flag will be re-enabled iff the field is later modified.
        /// </summary>
        private bool _disableInvalidSmtpServerWarning = false;

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="EmailSettingsControl"/> class.
        /// </summary>
        public EmailSettingsControl()
        {
            try
            {
                InitializeComponent();

                // This is a kludge - replace the text from the groupbox label with just the right number of spaces
                // so that the label and the groupbox frame don't show through the checkbox control, because the
                // checkbox control background is transparent, and I can't find any way to make it opaque!
                _groupBox1.Text = "                                    ";

                SmtpServerNameError(String.Empty);
                SmtpPortError(String.Empty);
                AuthenticationUserNameError(String.Empty);
                AuthenticationPasswordError(String.Empty);
                SenderNameError(String.Empty);
                SenderEmailAddressError(String.Empty);

                _textSmtpServer.SetErrorGlyphPosition(_emailSettingsControlErrorProvider);
                _textPort.SetErrorGlyphPosition(_emailSettingsControlErrorProvider);
                _textUserName.SetErrorGlyphPosition(_emailSettingsControlErrorProvider);
                _textPassword.SetErrorGlyphPosition(_emailSettingsControlErrorProvider);
                _textSenderName.SetErrorGlyphPosition(_emailSettingsControlErrorProvider);
                _textSenderEmail.SetErrorGlyphPosition(_emailSettingsControlErrorProvider);
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
        /// Gets a value indicating whether ANY potentially valid email settings have been entered.
        /// Note that SMTP server port is special, because it has a default value, so it is treated
        /// as "set" iff it has been cleared!
        /// </summary>
        /// <value><see langword="true"/> if potentially valid email settings have been entered;
        /// <see langword="false"/> if all fields (except SMTP server port) are blank.
        /// </value>
        public bool HasAnySettings
        {
            get
            {
                return _enableEmailSettingsCheckBox.Checked;
            }
        }

        /// <summary>
        /// Indicates whether the Requires authentication checkbox is set.
        /// </summary>
        /// <value>true when checked, false when not checked</value>
        public bool RequiresAuthentication
        {
            get
            {
                return _checkRequireAuthentication.Checked;
            }
        }

        #endregion Properties

        #region Methods

        string GetErrorTextForField(TextBox textBox)
        {
            if (textBox == _textSmtpServer)
            {
                return "form: host-name.domain-name.top-level-domain, e.g. smtp.gmail.com";
            }
            else if (textBox == _textPort)
            {
                return "form: port must be > 0, and < 65536";
            }
            else if (textBox == _textSenderEmail)
            {
                return "form: user@domain-name.top-level-domain, e.g. support@extractsystems.com";
            }
            else
            {
                return "This field is required";
            }            
        }

        /// <summary>
        /// Set (ErrorProvider) error description for the SMTP server name textbox control
        /// </summary>
        /// <param name="msg">message to display, or empty string to hide error</param>
        /// <param name="doNotDisplayErrors">override that when true prevents display of errors.
        /// This is convenient when it is important to get the return value from ValidateSettings w/o
        /// the side effects.</param>
        public void SmtpServerNameError(string msg, bool doNotDisplayErrors = false)
        {
            try
            {
                if (null == msg || true == doNotDisplayErrors)
                    return;

                _textSmtpServer.SetError(_emailSettingsControlErrorProvider, msg);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39228");
            }
        }

        /// <summary>
        /// Set (ErrorProvider) error description for the User name textbox control
        /// </summary>
        /// <param name="msg">message to display</param>
        /// <param name="doNotDisplayErrors">override that when true prevents display of errors.
        /// This is convenient when it is important to get the return value from ValidateSettings w/o
        /// the side effects.</param>
        public void AuthenticationUserNameError(string msg, bool doNotDisplayErrors = false)
        {
            try
            {
                if (null == msg || true == doNotDisplayErrors)
                    return;

                _textUserName.SetError(_emailSettingsControlErrorProvider, msg);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39224");
            }
        }

        /// <summary>
        /// Set (ErrorProvider) error description for the password textbox control
        /// </summary>
        /// <param name="msg">message to display</param>
        /// <param name="doNotDisplayErrors">override that when true prevents display of errors.
        /// This is convenient when it is important to get the return value from ValidateSettings w/o
        /// the side effects.</param>
        public void AuthenticationPasswordError(string msg, bool doNotDisplayErrors = false)
        {
            try
            {
                if (null == msg || true == doNotDisplayErrors)
                    return;

                _textPassword.SetError(_emailSettingsControlErrorProvider, msg);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39223");
            }
        }

        /// <summary>
        /// Set (ErrorProvider) error description for the Sender name textbox control
        /// </summary>
        /// <param name="msg">message to display</param>
        /// <param name="doNotDisplayErrors">override that when true prevents display of errors.
        /// This is convenient when it is important to get the return value from ValidateSettings w/o
        /// the side effects.</param>
        public void SenderNameError(string msg, bool doNotDisplayErrors = false)
        {
            try
            {
                if (null == msg || true == doNotDisplayErrors)
                    return;

                _textSenderName.SetError(_emailSettingsControlErrorProvider, msg);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39226");
            }
        }

        /// <summary>
        /// Set (ErrorProvider) error description for the Sender address textbox control
        /// </summary>
        /// <param name="msg">message to display</param>
        /// <param name="doNotDisplayErrors">override that when true prevents display of errors.
        /// This is convenient when it is important to get the return value from ValidateSettings w/o
        /// the side effects.</param>
        public void SenderEmailAddressError(string msg, bool doNotDisplayErrors = false)
        {
            try
            {
                if (null == msg || true == doNotDisplayErrors)
                    return;

                _textSenderEmail.SetError(_emailSettingsControlErrorProvider, msg);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39225");
            }
        }

        /// <summary>
        /// Set (ErrorProvider) error description for the SMTP port name textbox control
        /// </summary>
        /// <param name="msg">message to display</param>
        /// <param name="doNotDisplayErrors">override that when true prevents display of errors.
        /// This is convenient when it is important to get the return value from ValidateSettings w/o
        /// the side effects.</param>
        public void SmtpPortError(string msg, bool doNotDisplayErrors = false)
        {
            try
            {
                if (null == msg || true == doNotDisplayErrors)
                    return;

                _textPort.SetError(_emailSettingsControlErrorProvider, msg);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39227");
            }
        }

        /// <summary>
        /// Sets the enabled state of all controls, to either true or false. Note that 
        /// "all controls" doesn't include the "Enable email alerts" checkbox.
        /// </summary>
        /// <param name="enabled">if set to <c>true</c> [enabled].</param>
        void SetAllControlsEnabledState(bool enabled)
        {
            _textSmtpServer.Enabled = enabled;
            _textPort.Enabled = enabled;
            _textSenderName.Enabled = enabled;
            _textSenderEmail.Enabled = enabled;

            if (_checkRequireAuthentication.Checked)
            {
                _checkRequireAuthentication.Enabled = enabled;
                _textUserName.Enabled = enabled;
                _textPassword.Enabled = enabled;
                _checkUseSsl.Enabled = enabled;
                if (enabled)
                {
                    _textUserName.SetRequiredMarker();
                    _textPassword.SetRequiredMarker();
                }
                else
                {
                    _textUserName.RemoveRequiredMarker();
                    _textPassword.RemoveRequiredMarker();
                }
            }
        }

        /// <summary>
        /// Loads <see paremref="settings"/> into the UI controls.
        /// </summary>
        /// <param name="settings">email settings from database to use</param>
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

                _enableEmailSettingsCheckBox.Checked = settings.EnableEmailSettings;
                SetAllControlsEnabledState(enabled: settings.EnableEmailSettings);

                _disableInvalidEmailWarning = settings.PossibleInvalidSenderAddress;
                _disableInvalidSmtpServerWarning = settings.PossibleInvalidServer;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35928");
            }
        }

        /// <summary>
        /// Sets the text box field once. This is a helper function for ValidateSettings(), which simplifies
        /// getting the first text box in an error condition (so that the focus can be set to that textbox).
        /// </summary>
        /// <param name="onlySetOnce">The only textbox control to set (once).</param>
        /// <param name="textBox">The text box to add, iff the onlySetOnce target textbox isn't already set.</param>
        /// <returns>Returns a textbox control, either the onlySetOnce, or textBox.</returns>
        static TextBox SetTextBoxFieldOnce(TextBox onlySetOnce, TextBox textBox)
        {
            if (null == onlySetOnce)
            {
                return textBox;
            }
            else
            {
                return onlySetOnce;
            }
        }

        /// <summary>
        /// Validates that the settings in the UI are valid.
        /// </summary>
        /// <param name="doNotDisplayErrors">There are two cases (both "send" button enable/disable) where 
        /// validation is needed, but it is important not to prompt user/display error states. In these
        /// cases, doNotDisplayErrors is set to true.</param>
        /// <returns><see langword="true"/> if the current values in the UI controls are valid;
        /// otherwise, <see langword="false"/>.</returns>
        public bool ValidateSettings(bool doNotDisplayErrors = false)
        {
            try
            {
                // Note that the order of validation here is the same as the way the controls are
                // laid out, from top to bottom. This makes it easy to position the focus on the 
                // first invalid field.
                bool result = true;
                TextBox firstInvalidField = null;

                if (_textSmtpServer.EmptyOrRequiredMarkerIsSet())
                {
                    SmtpServerNameError("Outgoing mail (SMTP) server is required.", doNotDisplayErrors);
                    firstInvalidField = SetTextBoxFieldOnce(firstInvalidField, _textSmtpServer);
                    result = false;
                }
                else
                {
                    var text = _textSmtpServer.Text;
                    string pattern = @"(?=^.{1,254}$)(^(?:(?!\d+\.)[a-zA-Z0-9_\-]{1,63}\.?)+(?:[a-zA-Z]{2,})$)";
                    bool isMatch = Regex.IsMatch(text, pattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
                    IPAddress unused;
                    if (!doNotDisplayErrors && 
                        !_disableInvalidSmtpServerWarning &&
                        !isMatch && 
                        !IPAddress.TryParse(text, out unused))
                    {
                        if (MessageBox.Show("The specified SMTP address does not appear to conform " +
                                            "to a valid SMTP address form. Are you sure you want to use this address?",
                                            "Possible Invalid SMTP Address",
                                            MessageBoxButtons.YesNo,
                                            MessageBoxIcon.Warning,
                                            MessageBoxDefaultButton.Button1,
                                            0) == DialogResult.No)
                        {
                            SmtpServerNameError(GetErrorTextForField(_textSmtpServer), doNotDisplayErrors);
                            firstInvalidField = SetTextBoxFieldOnce(firstInvalidField, _textSmtpServer);
                            result = false;
                        }
                        else
                        {
                            SmtpServerNameError(String.Empty);
                            _disableInvalidSmtpServerWarning = true;
                        }
                    }
                }

                if (_textPort.EmptyOrRequiredMarkerIsSet())
                {
                    SmtpPortError(GetErrorTextForField(_textPort), doNotDisplayErrors);
                    firstInvalidField = SetTextBoxFieldOnce(firstInvalidField, _textPort);
                    result = false;
                }
                else
                {
                    int portNumber = Convert.ToInt32(_textPort.Text, CultureInfo.InvariantCulture);
                    if (portNumber <= 0 || portNumber > 65535)
                    {
                        SmtpPortError(GetErrorTextForField(_textPort), doNotDisplayErrors);
                        firstInvalidField = SetTextBoxFieldOnce(firstInvalidField, _textPort);
                        result = false;
                    }
                }

                if (_checkRequireAuthentication.Checked)
                {
                    // Ensure there is a username and password
                    if (_textUserName.EmptyOrRequiredMarkerIsSet())
                    {
                        AuthenticationUserNameError(GetErrorTextForField(_textUserName), doNotDisplayErrors);
                        firstInvalidField = SetTextBoxFieldOnce(firstInvalidField, _textUserName);
                        result = false;
                    }

                    if (_textPassword.EmptyOrRequiredMarkerIsSet())
                    {
                        AuthenticationPasswordError(GetErrorTextForField(_textPassword), doNotDisplayErrors);
                        firstInvalidField = SetTextBoxFieldOnce(firstInvalidField, _textPassword);
                        result = false;
                    }
                }

                if (_textSenderName.EmptyOrRequiredMarkerIsSet())
                {
                    SenderNameError(GetErrorTextForField(_textSenderName), doNotDisplayErrors);
                    firstInvalidField = SetTextBoxFieldOnce(firstInvalidField, _textSenderName);
                    result = false;
                }

                if (_textSenderEmail.EmptyOrRequiredMarkerIsSet())
                {
                    SenderEmailAddressError(GetErrorTextForField(_textSenderEmail), doNotDisplayErrors);
                    firstInvalidField = SetTextBoxFieldOnce(firstInvalidField, _textSenderEmail);
                    result = false;
                }
                else if (!UtilityMethods.IsValidEmailAddress(_textSenderEmail.Text))
                {
                    if (!doNotDisplayErrors && !_disableInvalidEmailWarning)
                    {
                        if (MessageBox.Show("The specified email address does not appear to conform " +
                            "to a valid email address form. Are you sure you want to use this address?",
                            "Possible Invalid Email", MessageBoxButtons.YesNo, MessageBoxIcon.Warning,
                            MessageBoxDefaultButton.Button1, 0) == DialogResult.No)
                        {
                            SenderEmailAddressError(GetErrorTextForField(_textSenderEmail));
                            firstInvalidField = SetTextBoxFieldOnce(firstInvalidField, _textSenderEmail);
                            result = false;
                        }
                        else
                        {
                            SenderEmailAddressError(String.Empty);
                            _disableInvalidEmailWarning = true;
                        }
                    }
                }

                if (false == doNotDisplayErrors && null != firstInvalidField)
                {
                    firstInvalidField.Focus();
                    string errorText = GetErrorTextForField(firstInvalidField);
                    firstInvalidField.SetError(_emailSettingsControlErrorProvider, errorText);
                }

                return result;
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
                settings.EnableEmailSettings = _enableEmailSettingsCheckBox.Checked;
                settings.Server = _textSmtpServer.TextValue();
                settings.Port = _textPort.Int32Value;

                // There is legacy code that uses the presence or absense of a username setting to
                // determine whether authentication is required rather than persisting a separate
                // boolean.
                settings.UserName = _checkRequireAuthentication.Checked ? _textUserName.TextValue() : "";
                settings.Password = _checkRequireAuthentication.Checked ? _textPassword.TextValue() : "";
                settings.UseSsl = _checkRequireAuthentication.Checked ? _checkUseSsl.Checked : false;
                settings.SenderName = _textSenderName.TextValue();
                settings.SenderAddress = _textSenderEmail.TextValue();
                settings.EmailSignature = _textEmailSignature.TextValue();
                settings.PossibleInvalidSenderAddress = _disableInvalidEmailWarning;
                settings.PossibleInvalidServer = _disableInvalidSmtpServerWarning;
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

        /// <summary>
        /// Updates the required markers.
        /// </summary>
        /// <param name="displayMarkers">if set to <c>true</c> display markers, otherwise remove markers.</param>
        void UpdateRequiredMarkers(bool displayMarkers)
        {
            if (true == displayMarkers)
            {
                if (String.IsNullOrWhiteSpace(_textSmtpServer.Text))
                {
                    _textSmtpServer.SetRequiredMarker();
                }

                if (String.IsNullOrWhiteSpace(_textSenderName.Text))
                {
                    _textSenderName.SetRequiredMarker();
                }

                if (String.IsNullOrWhiteSpace(_textSenderEmail.Text))
                {
                    _textSenderEmail.SetRequiredMarker();
                }
            }
            else
            {
                if (_textSmtpServer.EmptyOrRequiredMarkerIsSet())
                {
                    _textSmtpServer.RemoveRequiredMarker();
                }

                if (_textSenderName.EmptyOrRequiredMarkerIsSet())
                {
                    _textSenderName.RemoveRequiredMarker();
                }

                if (_textSenderEmail.EmptyOrRequiredMarkerIsSet())
                {
                    _textSenderEmail.RemoveRequiredMarker();
                }
            }
        }

        /// <summary>
        /// On load method, sets required markers on the form
        /// </summary>
        public void DoLoad()
        {
            try
            {
                if (!_enableEmailSettingsCheckBox.Checked)
                {
                    return;
                }

                UpdateRequiredMarkers(displayMarkers: true);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39214");
            }
        }

        #endregion Methods

        #region Event Handlers

        /// <summary>
        /// Handles the OnLoad event to mark text fields as required. Note that this event is triggered
        /// from the property page when teh Email tab is selected.
        /// </summary>
        private void HandleOnLoad(object sender, EventArgs e)
        {
            try
            {
                DoLoad();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI39208");
            }
        }

        /// <summary>
        /// Handles (focus) enter event, to remove the required field marker while user is entering text.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandleFocusEnter(object sender, EventArgs e)
        {
            try
            {
                var textbox = (TextBox)sender;
                if (textbox.IsRequiredMarkerSet())
                {
                    textbox.RemoveRequiredMarker();

                    if (textbox == _textPassword)
                    {
                        // Re-enable password chars for input - disabled previously to set required marker
                        // as a plain-text string (not password chars).
                        _textPassword.UseSystemPasswordChar = true;
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI39206");
            }
        }

        /// <summary>
        /// Handles the leave (focus) event to mark the field as required iff no other text exists in the field.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HandleFocusLeave(object sender, EventArgs e)
        {
            try
            {
                var textbox = (TextBox)sender;
                var text = textbox.Text;
                if (String.IsNullOrWhiteSpace(text))
                {
                    if (textbox == _textPassword)
                    {
                        _textPassword.UseSystemPasswordChar = false;
                    }   
                 
                    textbox.SetRequiredMarker();
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI39207");
            }
        }

        /// <summary>
        /// Handles the text box text changed.
        /// NOTE: All the required textBoxes specify this event handler.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void HandleTextBoxTextChanged(object sender, EventArgs e)
        {
            try
            {
                UpdateEnabledStates();

                TextBox textbox = (TextBox)sender;
                textbox.SetError(_emailSettingsControlErrorProvider, String.Empty);

                if (textbox == _textSmtpServer && _disableInvalidSmtpServerWarning)
                {
                    _disableInvalidSmtpServerWarning = false;
                }
                else if (textbox == _textSenderEmail && _disableInvalidEmailWarning)
                {
                    _disableInvalidEmailWarning = false;
                }
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

                if (_checkRequireAuthentication.Checked)
                {
                    if (String.IsNullOrWhiteSpace(_textUserName.Text))
                    {
                        _textUserName.SetRequiredMarker();
                    }

                    // Turn off password chars to set cue so that it won't be shown as dots...
                    if (String.IsNullOrWhiteSpace(_textPassword.Text))
                    {
                        _textPassword.UseSystemPasswordChar = false;
                        _textPassword.SetRequiredMarker();
                    }
                }
                else
                {
                    if (_textUserName.EmptyOrRequiredMarkerIsSet())
                    {
                        _textUserName.RemoveRequiredMarker();
                    }

                    if (_textPassword.EmptyOrRequiredMarkerIsSet())
                    {
                        _textPassword.UseSystemPasswordChar = true;
                        _textPassword.RemoveRequiredMarker();
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI32275");
            }
        }

        /// <summary>
        /// Handles the CheckStateChanged event of the EnableEmailSettingsCheckBox control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void HandleEnableEmailSettingsCheckBox_CheckStateChanged(object sender, EventArgs e)
        {
            try
            {
                SetAllControlsEnabledState(_enableEmailSettingsCheckBox.Checked);
                UpdateRequiredMarkers(_enableEmailSettingsCheckBox.Checked);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI39220");
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

    