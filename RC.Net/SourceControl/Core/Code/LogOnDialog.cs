using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Extract.SourceControl
{
    /// <summary>
    /// Represents a dialog that allows the user to specify log on settings.
    /// </summary>
    public partial class LogOnDialog : Form
    {
        #region LogOnDialog Fields

        LogOnSettings _settings;

        #endregion LogOnDialog Fields

        #region LogOnDialog Constructors

        /// <summary>
        /// Initializes a new <see cref="LogOnDialog"/> class.
        /// </summary>
        // Don't fight with auto-generated code.
        //[SuppressMessage("Microsoft.Performance", "CA1805:DoNotInitializeUnnecessarily")]
        public LogOnDialog(LogOnSettings settings)
        {
            InitializeComponent();

            _settings = settings;
        }

        #endregion LogOnDialog Constructors

        #region LogOnDialog Properties

        /// <summary>
        /// Gets or sets the log on settings.
        /// </summary>
        /// <value>The log on settings.</value>
        /// <returns>The log on settings.</returns>
        public LogOnSettings LogOnSettings
        {
            get
            {
                return _settings;
            }
            set
            {
                _settings = value;
            }
        }

        #endregion LogOnDialog Properties

        #region LogOnDialog Methods

        LogOnSettings GetLogOnSettings()
        {
            return new LogOnSettings(GetServer(), _userNameTextBox.Text, _passwordTextBox.Text);
        }

        string GetServer()
        {
            string server = _serverTextBox.Text;
            if (server.Length > 0)
            {
                if (!server.StartsWith(@"http://", StringComparison.OrdinalIgnoreCase))
                {
                    server = @"http://" + server;
                }
                if (!server.EndsWith(@"/VaultService", StringComparison.OrdinalIgnoreCase))
                {
                    if (!server.EndsWith(@"/", StringComparison.OrdinalIgnoreCase))
                    {
                        server += @"/";
                    }
                    server += "VaultService";
                }
            }
            return server;
        }

        static void WarnSettingMissing(string missingSetting)
        {
            MessageBox.Show("Please enter a " + missingSetting, "Invalid " + missingSetting,
                MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button1, 0);
        }

        #endregion LogOnDialog Methods

        #region LogOnDialog Overrides

        /// <summary>
        /// Raises the <see cref="Form.Load"/> event.
        /// </summary>
        /// <param name="e">An <see cref="EventArgs"/> that contains event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            _userNameTextBox.Text = _settings.UserName ?? "";
            _passwordTextBox.Text = "";
            _serverTextBox.Text = _settings.Server ?? "";
        }

        #endregion LogOnDialog Overrides

        #region LogOnDialog Event Handlers

        void HandleOkButtonClick(object sender, EventArgs e)
        {
            LogOnSettings settings = GetLogOnSettings();

            bool settingsValid = true;
            if (string.IsNullOrEmpty(settings.UserName))
            {
                WarnSettingMissing("username");
                settingsValid = false;
            }
            else if (string.IsNullOrEmpty(settings.Password))
            {
                WarnSettingMissing("password");
                settingsValid = false;
            }
            else if (string.IsNullOrEmpty(settings.Server))
            {
                WarnSettingMissing("server");
                settingsValid = false;
            }

            if (settingsValid)
            {
                _settings = settings;
                this.DialogResult = DialogResult.OK;
                Close();
            }
        }

        #endregion LogOnDialog Event Handlers
    }
}