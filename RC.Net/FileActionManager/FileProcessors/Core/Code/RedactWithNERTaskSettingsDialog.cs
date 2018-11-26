using Extract.Licensing;
using System;
using System.Windows.Forms;

namespace Extract.FileActionManager.FileProcessors
{
    /// <summary>
    /// A <see cref="Form"/> to view and modify settings for an
    /// <see cref="RedactWithNERTask"/> instance.
    /// </summary>
    public partial class RedactWithNERTaskSettingsDialog : Form
    {
        #region Constants

        /// <summary>
        /// The object name used for license validation.
        /// </summary>
        static readonly string _OBJECT_NAME =
            typeof(RedactWithNERTaskSettingsDialog).ToString();

        #endregion Constants

        #region Fields

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="RedactWithNERTaskSettingsDialog"/> class.
        /// </summary>
        /// <param name="settings">The <see cref="RedactWithNERTask"/> to configure</param>
        public RedactWithNERTaskSettingsDialog(RedactWithNERTask settings)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.FileActionManagerObjects, "ELI46519",
                    _OBJECT_NAME);

                Settings = settings;

                InitializeComponent();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI46520");
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Property to return the configured settings
        /// </summary>
        public RedactWithNERTask Settings
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

                _nerModelTextBox.Text = Settings.NERModelPath;
                _outputImageTextBox.Text = Settings.OutputImagePath;
                _outputVOATextBox.Text = Settings.OutputVOAPath ?? "";
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI46521");
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

                Settings.NERModelPath = _nerModelTextBox.Text;
                Settings.OutputImagePath = _outputImageTextBox.Text;
                Settings.OutputVOAPath = _outputVOATextBox.Text;

                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI46522", ex);
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
            if (string.IsNullOrWhiteSpace(_nerModelTextBox.Text))
            {
                return true;
            }
            if (string.IsNullOrWhiteSpace(_outputImageTextBox.Text))
            {
                return true;
            }
            return false;
        }

        #endregion Private Members
    }
}
