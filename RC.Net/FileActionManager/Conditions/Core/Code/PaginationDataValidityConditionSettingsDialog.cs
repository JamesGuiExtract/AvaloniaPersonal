using Extract.Licensing;
using Extract.Utilities;
using System;
using System.Windows.Forms;

namespace Extract.FileActionManager.Conditions
{
    /// <summary>
    /// A <see cref="Form"/> that allows for configuration of an <see cref="PageCountCondition"/>
    /// instance.
    /// </summary>
    public partial class PaginationDataValidityConditionSettingsDialog : Form
    {
        #region Constants

        /// <summary>
        /// The object name.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(PaginationDataValidityConditionSettingsDialog).ToString();

        #endregion Constants

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="PaginationDataValidityConditionSettingsDialog"/> class.
        /// </summary>
        public PaginationDataValidityConditionSettingsDialog(PaginationDataValidityCondition settings)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI47145",
                    _OBJECT_NAME);

                Settings = settings;

                InitializeComponent();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI47146");
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the settings.
        /// </summary>
        /// <value>
        /// The settings.
        /// </value>
        public PaginationDataValidityCondition Settings { get; set; }

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

                // Apply Settings values to the UI.
                if (Settings != null)
                {
                    _ifNoErrorsCheckBox.Checked = Settings.IfNoErrors;
                    _ifNoWarningsCheckBox.Checked = Settings.IfNoWarnings;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI47147");
            }
        }

        #endregion Overrides

        #region Event Handlers

        /// <summary>
        /// In the case that the OK button is clicked, validates the settings, applies them, and
        /// closes.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleOkButtonClick(object sender, EventArgs e)
        {
            try
            {
                // If there are invalid settings, prompt and return without closing.
                if (WarnIfInvalid())
                {
                    return;
                }

                Settings.IfNoErrors = _ifNoErrorsCheckBox.Checked;
                Settings.IfNoWarnings = _ifNoWarningsCheckBox.Checked;

                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI47148");
            }
        }

        #endregion Event Handlers

        #region Private Members

        /// <summary>
        /// Displays a warning message if the user specified settings are invalid.
        /// </summary>
        /// <returns><see langword="true"/> if the settings are invalid; <see langword="false"/> if
        /// the settings are valid.</returns>
        bool WarnIfInvalid()
        {
            ExtractException.Assert("ELI47149",
                "Page count condition settings have not been provided.", Settings != null);

            if (!_ifNoErrorsCheckBox.Checked
                && !_ifNoWarningsCheckBox.Checked)
            {
                UtilityMethods.ShowMessageBox(
                    "At least one data validity restriction must be selected.", "Invalid configuration", true);

                return true;
            }

            return false;
        }

        #endregion Private Members
    }
}
