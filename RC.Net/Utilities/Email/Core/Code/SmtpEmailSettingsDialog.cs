using Extract.Utilities.Email.Properties;
using System;
using System.Windows.Forms;

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

                _emailSettingsControl.LoadSettings(_settings);
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
                if (_emailSettingsControl.ValidateSettings())
                {
                    _emailSettingsControl.ApplySettings(_settings);
                    _settings.SaveSettings();

                    DialogResult = DialogResult.OK;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI32272");
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
                _emailSettingsControl.SendTestEmail();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI32276");
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
                _buttonTest.Enabled = _emailSettingsControl.HasSettings;
                _buttonOk.Enabled = _emailSettingsControl.HasSettings;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35926");
            }
        }

        #endregion Event Handlers
    }
}
