using Extract.Licensing.Internal;
using System;
using System.Windows.Forms;

namespace Extract.FAMDBCounterManager
{
    /// <summary>
    /// A <see cref="Form"/> that allows configuration and persistence of email settings.
    /// <para><b>Note</b></para>
    /// This class is a modified copy of Extract.Utilities.Email.SmtpEmailSettingsDialog. This
    /// project is not linked to Extract.Utilities.Email to avoid COM dependencies.
    /// </summary>
    internal partial class EmailSettingsDialog : Form
    {
        #region Fields

        /// <summary>
        /// The email settings object for initializing the UI.
        /// </summary>
        EmailSettingsManager _settings;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="EmailSettingsDialog"/> class.
        /// </summary>
        public EmailSettingsDialog() : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="EmailSettingsDialog"/> class.
        /// </summary>
        /// <param name="settings">The settings to configure the UI with.
        /// If <see langword="null"/> then the current global settings will be loaded.</param>
        public EmailSettingsDialog(EmailSettingsManager settings)
        {
            InitializeComponent();

            _settings = settings;
            if (_settings == null)
            {
                _settings = new EmailSettingsManager();
                _settings.LoadSettings();
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
            try
            {
                base.OnLoad(e);
            
                _emailSettingsControl.LoadSettings(_settings);
            }
            catch (Exception ex)
            {
                ex.ShowMessageBox();
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
                if (_emailSettingsControl.ValidateSettings())
                {
                    _emailSettingsControl.ApplySettings(_settings);
                    _settings.SaveSettings();

                    DialogResult = DialogResult.OK;
                }
            }
            catch (Exception ex)
            {
                ex.ShowMessageBox();
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event of the <see cref="_testSendButton"/>.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void HandleTestEmailClick(object sender, EventArgs e)
        {
            try
            {
                _emailSettingsControl.SendTestEmail();
            }
            catch (Exception ex)
            {
                ex.ShowMessageBox();
            }
        }

        /// <summary>
        /// Handles the <see cref="EmailSettingsControl.SettingsChanged"/> event of the
        /// <see cref="_emailSettingsControl"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleEmailSettingsControl_SettingsStateChanged(object sender, EventArgs e)
        {
            try
            {
                _testSendButton.Enabled = _emailSettingsControl.HasSettings;
                _buttonOk.Enabled = _emailSettingsControl.HasSettings;
            }
            catch (Exception ex)
            {
                ex.ShowMessageBox();
            }
        }

        #endregion Event Handlers
    }
}
