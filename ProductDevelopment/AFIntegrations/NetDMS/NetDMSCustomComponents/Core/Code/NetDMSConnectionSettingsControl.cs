using Extract;
using Extract.Licensing;
using Extract.Utilities;
using Extract.Utilities.Forms;
using Microsoft.Win32;
using System;
using System.Globalization;
using System.Windows.Forms;

namespace Extract.NetDMSCustomComponents
{
    /// <summary>
    /// A <see cref="UserControl"/> that allows configuration of an <see cref="NetDMSClassBase"/>
    /// instance.
    /// </summary>
    internal partial class NetDMSConnectionSettingsControl : UserControl
    {
        #region Constants

        /// <summary>
        /// The object name.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(NetDMSConnectionSettingsControl).ToString();

        /// <summary>
        /// The NetDMS registery key that contains the last-used dispatcher.
        /// </summary>
        static readonly string _NETDMS_USER_CONSOLE_REG_KEY =
            "Software\\VistaSG\\WorkFlow User Console\\";

        /// <summary>
        /// The NetDMS registery key that contains data on recently used dispatchers.
        /// </summary>
        static readonly string _NETDMS_DISPATCHERS_REG_KEY =
            "Software\\VistaSG\\WorkFlow User Console\\Dispatchers\\";

        #endregion Constants

        #region Fields

        /// <summary>
        /// The <see cref="ErrorProvider"/> data entry controls should used to display data
        /// validation errors.
        /// </summary>
        ErrorProvider _errorProvider = new ErrorProvider();

        /// <summary>
        /// Indicates whether the user has changed the password for the NetDMS login.
        /// </summary>
        bool _passwordChanged;

        /// <summary>
        /// Indicates whether the NetDMS connection settings have been loaded into the UI.
        /// </summary>
        bool _connectionSettingsLoaded;

        /// <summary>
        /// Registry settings used to persist the last used NetDMS connection info.
        /// </summary>
        RegistrySettings<Properties.Settings> _registry =
            new RegistrySettings<Properties.Settings>(@"Software\Extract Systems\NetDMS");

        /// <summary>
        /// The ID of the last NetDMS dispatcher to be used (per the NetDMS registry node).
        /// </summary>
        int _lastNetDMSDispatcherRegistryNumber;

        /// <summary>
        /// The <see cref="NetDMSClassBase"/> instance that supplied the original connection info
        /// for this control.
        /// </summary>
        NetDMSClassBase _connectionSettingsSource;

        /// <summary>
        /// Indicates keyboard input into the password field when the password has not yet been
        /// changed and, therefore, is still the "dummy" value.
        /// </summary>
        string _passwordInput;

        #endregion Fields

        #region Contructors

        /// <summary>
        /// Initializes a new instance of the <see cref="NetDMSConnectionSettingsControl"/> class.
        /// </summary>
        public NetDMSConnectionSettingsControl()
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.FileActionManagerObjects, "ELI34929",
                    _OBJECT_NAME);

                InitializeComponent();

                _errorProvider.BlinkStyle = ErrorBlinkStyle.NeverBlink;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI34930");
            }
        }

        #endregion Contructors

        #region Methods

        /// <summary>
        /// Loads the connection settings.
        /// </summary>
        /// <param name="connectionSettings"></param>
        public void LoadConnectionSettings(NetDMSClassBase connectionSettings)
        {
            try
            {
                _connectionSettingsSource = connectionSettings;
                _serverTextBox.Text = connectionSettings.Server;
                Port = connectionSettings.Port;
                _userTextBox.Text = connectionSettings.User;
                // To be as secure as possible in handling the NetDMS password, the UI will not hold
                // the password except for the case where the user is entering/changing it. If the
                // password has previously been configured, simply assign a dummy string to indicate
                // that it has been entered. If the value is changed, the dummy value will be
                // cleared and the user will need to re-enter the entire password from scratch.
                if (connectionSettings.HasPassword)
                {
                    _passwordTextBox.Text = "********";
                }

                if (string.IsNullOrWhiteSpace(connectionSettings.Server) &&
                    connectionSettings.Port == 0 &&
                    string.IsNullOrWhiteSpace(connectionSettings.User))
                {
                    LoadLastConnectionInfo();
                }

                _connectionSettingsLoaded = true;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI34931");
            }
        }

        /// <summary>
        /// Applies the connection settings from the UI controls to the specified
        /// <see paramref="connectionSettings"/> instance.
        /// </summary>
        /// <param name="connectionSettings">The <see cref="NetDMSClassBase"/> instance to apply the
        /// settings to.</param>
        public void ApplyConnectionSettings(NetDMSClassBase connectionSettings)
        {
            try
            {
                connectionSettings.Server = _serverTextBox.Text;
                connectionSettings.Port = Port;
                connectionSettings.User = _userTextBox.Text;
                if (_passwordChanged)
                {
                    connectionSettings.SetPassword(_passwordTextBox.Text);
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI34932");
            }
        }

        /// <summary>
        /// Displays a warning message if the user specified settings are invalid.
        /// </summary>
        /// <returns><see langword="true"/> if the settings are invalid; <see langword="false"/> if
        /// the settings are valid.</returns>
        public bool WarnIfInvalid()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_serverTextBox.Text))
                {
                    _serverTextBox.Focus();
                    UtilityMethods.ShowMessageBox("Please specify the NetDMS server.",
                        "NetDMS server not specified", false);
                    return true;
                }

                if (Port <= 0)
                {
                    _portTextBox.Focus();
                    UtilityMethods.ShowMessageBox("Please specify a valid port number.",
                        "Port not specified", false);
                    return true;
                }

                if (string.IsNullOrWhiteSpace(_userTextBox.Text))
                {
                    _userTextBox.Focus();
                    UtilityMethods.ShowMessageBox("Please specify a NetDMS user.",
                        "NetDMS user not specified", false);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI34933");
            }
        }

        #endregion Methods

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

                // Disable the context menu on the password text box to prevent the user from being
                // able to cut/copy.
                _passwordTextBox.ContextMenu = new ContextMenu();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI34934");
            }
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.
        /// </param>
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
        /// Handles the <see cref="Control.TextChanged"/> event of the <see cref="_portTextBox"/>
        /// control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandlePortTextChanged(object sender, EventArgs e)
        {
            try
            {
                ParsePortText();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI34872");
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.KeyDown"/> event of <see cref="_passwordTextBox"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.KeyEventArgs"/> instance
        /// containing the event data.</param>
        void HandlePasswordTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                // To be as secure as possible in handling the NetDMS password, the UI will not hold
                // the password except for the case where the user is entering/changing it. To
                // detect if the user is modifying the password, first clear on key down. Later we
                // will check the KeyPress event to determine if this key event resulted in an event
                // that will change the text in the password box.
                _passwordInput = null;

                // The delete and backspace key should result in the entire "dummy" password being
                // cleared. The password will be cleared if _passwordInput is non-null.
                if (!_passwordChanged && e.KeyCode == Keys.Delete || e.KeyCode == Keys.Back)
                {
                    _passwordInput = "";
                }

                // Disable copy/cut shortcut keys to prevent someone from attempting to retrieve
                // the password.
                if (e.Control &&
                    (e.KeyCode == Keys.X || e.KeyCode == Keys.C || e.KeyCode == Keys.Insert))
                {
                    e.SuppressKeyPress = true;
                }
                // If the user is attempting to paste, allow it... but the paste will replace
                // all text.
                else if (!_passwordChanged && Clipboard.ContainsText() &&
                    ((e.Control && e.KeyCode == Keys.V) || (e.Shift && e.KeyCode == Keys.Insert)))
                {
                    e.SuppressKeyPress = true;
                    _passwordInput = Clipboard.GetText();
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI34936");
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.KeyPress"/> event of <see cref="_passwordTextBox"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.KeyPressEventArgs"/> instance
        /// containing the event data.</param>
        void HandlePasswordTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            try
            {
                // Check to see if the key press resulted in any input that will change the
                // contents of the password text box. If so, set _passwordInput to cause the dummy
                // password to be cleared.
                if (!_passwordChanged)
                {
                    if (Char.IsLetterOrDigit(e.KeyChar) ||
                        Char.IsWhiteSpace(e.KeyChar) ||
                        Char.IsPunctuation(e.KeyChar) ||
                        Char.IsSymbol(e.KeyChar))
                    {
                        _passwordInput = e.KeyChar.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI34935");
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.KeyUp"/> event of the <see cref="_passwordTextBox"/>
        /// control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.KeyEventArgs"/> instance containing
        /// the event data.</param>
        void HandlePasswordTextBox_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                // If the key press resulted in any kind of value that can change the contents of
                // the password box, clear the dummy password and replace it with the new input.
                if (_connectionSettingsLoaded && !_passwordChanged && _passwordInput != null)
                {
                    _passwordTextBox.Text = _passwordInput;
                    _passwordTextBox.Select(_passwordTextBox.Text.Length, 0);
                    _passwordChanged = true;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI34937");
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event of the
        /// <see cref="_loadLastConnectionButton"/> control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void HandleLoadLastConnectionButton_Click(object sender, EventArgs e)
        {
            try
            {
                LoadLastConnectionInfo();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI34938");
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event of the
        /// <see cref="_testConnectionButton"/> control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void HandleTestConnectionButton_Click(object sender, EventArgs e)
        {
            try
            {
                using (new TemporaryCursor(Cursors.WaitCursor))
                using (NetDMSClassBase connectionTester = new NetDMSClassBase())
                {
                    connectionTester.Server = _serverTextBox.Text;
                    connectionTester.Port = Int32.Parse(_portTextBox.Text,
                        CultureInfo.InvariantCulture);
                    connectionTester.User = _userTextBox.Text;
                    if (_passwordChanged)
                    {
                        connectionTester.SetPassword(_passwordTextBox.Text);
                    }
                    else
                    {
                        connectionTester.CopyPassword(_connectionSettingsSource);
                    }

                    connectionTester.Connect();
                    connectionTester.Disconnect();

                    UtilityMethods.ShowMessageBox(
                        "Successfully connected to the NetDMS dispatcher!", "Success", false);
                }
            }
            catch (Exception ex)
            {
                new ExtractException("ELI34939", "Connection test failed: " + ex.Message, ex).Display();
            }
        }

        #endregion Event Handlers

        #region Private Members

        /// <summary>
        /// Gets or sets the port.
        /// </summary>
        /// <value>
        /// The port.
        /// </value>
        int Port
        {
            get
            {
                return ParsePortText();
            }

            set
            {
                try
                {
                    _portTextBox.Text = value.ToString(CultureInfo.InvariantCulture);
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI34874");
                }
            }
        }

        /// <summary>
        /// Parses the port text.
        /// </summary>
        /// <returns></returns>
        int ParsePortText()
        {
            try
            {
                int port = Int32.Parse(_portTextBox.Text, CultureInfo.InvariantCulture);

                ExtractException.Assert("ELI34875",
                    "Port number must be greater than zero", port > 0);

                _errorProvider.SetError(_portTextBox, "");

                return port;
            }
            catch (Exception ex)
            {
                if (string.IsNullOrWhiteSpace(_portTextBox.Text))
                {
                    _errorProvider.SetError(_portTextBox, "Port must be specified.");
                }
                else
                {
                    _errorProvider.SetError(_portTextBox, "Invalid port number: " + ex.Message);
                }

                return 0;
            }
        }

        /// <summary>
        /// Loads the last NetDMS connection info used by NetDMSCustomeComponents, if possible, or
        /// if not, by NetDMS. In the case of the latter, the password will not be loaded since it
        /// is encrypted by the NetDMS in the registry.
        /// </summary>
        void LoadLastConnectionInfo()
        {
            try
            {
                _connectionSettingsLoaded = false;

                // If NetDMSCustomComponents doesn't have any last connection info itself, load the
                // last connection info from NetDMS's registry.
                if (string.IsNullOrWhiteSpace(_registry.Settings.LastServer))
                {
                    _lastNetDMSDispatcherRegistryNumber = GetLastNetDMSDispatcherRegistryId();

                    if (_lastNetDMSDispatcherRegistryNumber > 0)
                    {
                        _registry.Settings.LastServer = GetLastNetDMSDispatcherProperty("Host");
                        _registry.Settings.LastPort = GetLastNetDMSDispatcherProperty("Port");
                        _registry.Settings.LastUser = GetLastNetDMSDispatcherProperty("LastUser");
                        // The password is encrypted by NetDMS; we can't use it. Clear any existing
                        // password so that the user doesn't think that we have set the password.
                        _registry.Settings.LastPassword = "";
                    }
                }

                // If we have any info on the last connection, use it to populate the control.
                if (!string.IsNullOrWhiteSpace(_registry.Settings.LastServer))
                {
                    _serverTextBox.Text = _registry.Settings.LastServer;
                    _portTextBox.Text = _registry.Settings.LastPort;
                    _userTextBox.Text = _registry.Settings.LastUser;
                    _passwordTextBox.Text = string.IsNullOrWhiteSpace(_registry.Settings.LastPassword)
                        ? ""
                        : NetDMSClassBase._USE_LAST_PASSWORD;
                    if (!string.IsNullOrEmpty(_passwordTextBox.Text))
                    {
                        _passwordChanged = true;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI34940");
            }
            finally
            {
                _connectionSettingsLoaded = true;
            }
        }

        /// <summary>
        /// Gets the last used dispatcher ID from the NetDMS registry node.
        /// </summary>
        /// <returns>The last used dispatcher ID.</returns>
        static int GetLastNetDMSDispatcherRegistryId()
        {
            using (RegistryKey userConsoleKey =
                Registry.CurrentUser.OpenSubKey(_NETDMS_USER_CONSOLE_REG_KEY))
            {
                if (userConsoleKey != null)
                {
                    object lastDispatcher = userConsoleKey.GetValue("CurrentDispatcher");
                    if (lastDispatcher != null)
                    {
                        string dispatcherName = (string)lastDispatcher;

                        using (RegistryKey dispatchersKey =
                            Registry.CurrentUser.OpenSubKey(_NETDMS_DISPATCHERS_REG_KEY))
                        {
                            // Iterate through ID 1 to 100... if there is a dispatcher with ID > 100
                            // this code won't find it.
                            for (int i = 1; i < 100; i++)
                            {
                                object name = dispatchersKey.GetValue("Name" +
                                    i.ToString(CultureInfo.InvariantCulture));
                                if (name == null)
                                {
                                    break;
                                }

                                if (dispatcherName.Equals((string)name, StringComparison.OrdinalIgnoreCase))
                                {
                                    return i;
                                }
                            }
                        }
                    }
                }
            }

            return 0;
        }

        /// <summary>
        /// Gets the last net DMS dispatcher property.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns>The <see langword="string"/> value of the property.</returns>
        string GetLastNetDMSDispatcherProperty(string propertyName)
        {
            try
            {
                if (_lastNetDMSDispatcherRegistryNumber == 0)
                {
                    return "";
                }
                else
                {
                    using (RegistryKey dispatchersKey =
                        Registry.CurrentUser.OpenSubKey(_NETDMS_DISPATCHERS_REG_KEY))
                    {
                        return dispatchersKey.GetValue(
                            propertyName + _lastNetDMSDispatcherRegistryNumber
                                .ToString(CultureInfo.InvariantCulture))
                            .ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractLog("ELI34941");
                return "";
            }
        }

        #endregion Private Members
    }
}
