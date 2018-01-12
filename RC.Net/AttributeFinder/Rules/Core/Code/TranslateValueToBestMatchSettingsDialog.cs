using Extract.Licensing;
using Extract.Utilities;
using System;
using System.Windows.Forms;
using UCLID_AFCORELib;
using UCLID_COMUTILSLib;

namespace Extract.AttributeFinder.Rules
{
    /// <summary>
    /// Allows configuration of a <see cref="TranslateValueToBestMatch"/> instance.
    /// </summary>
    [CLSCompliant(false)]
    public partial class TranslateValueToBestMatchSettingsDialog : Form
    {
        #region Constants

        /// <summary>
        /// The object name.
        /// </summary>
        static readonly string _OBJECT_NAME =
            typeof(TranslateValueToBestMatchSettingsDialog).ToString();

        #endregion Constants

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="TranslateValueToBestMatchSettingsDialog"/> class.
        /// </summary>
        /// <param name="settings">The settings.</param>
        public TranslateValueToBestMatchSettingsDialog(TranslateValueToBestMatch settings)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.RuleSetEditorUIObject,
                    "ELI45484", _OBJECT_NAME);

                Settings = settings;

                InitializeComponent();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45485");
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the <see cref="TranslateValueToBestMatch"/> to configure.
        /// </summary>
        /// <value>The <see cref="TranslateValueToBestMatch"/> to configure.</value>
        public TranslateValueToBestMatch Settings
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

                // Apply Settings values to the UI.
                if (Settings != null)
                {
                    _attributeSelectorControl.ConfigurableObject =
                        (ICategorizedComponent)Settings.AttributeSelector;
                    _sourceListPathTextBox.Text = Settings.SourceListPath ?? "";
                    _synonymMapPathTextBox.Text = Settings.SynonymMapPath ?? "";
                    _minimumMatchScoreNumericUpDown.Value = (decimal)Settings.MinimumMatchScore;
                    switch (Settings.UnableToTranslateAction)
                    {
                        case NoGoodMatchAction.DoNothing:
                            _doNothingRadioButton.Checked = true;
                            break;
                        case NoGoodMatchAction.ClearValue:
                            _clearValueRadioButton.Checked = true;
                            break;
                        case NoGoodMatchAction.RemoveAttribute:
                            _removeAttributeRadioButton.Checked = true;
                            break;
                        case NoGoodMatchAction.SetTypeToUntranslated:
                            _setTypeToUntranslatedRadioButton.Checked = true;
                            break;
                    }
                    _createScoreSubattributeCheckBox.Checked = Settings.CreateBestMatchScoreSubAttribute;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45486");
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

                Settings.AttributeSelector =
                    (IAttributeSelector)_attributeSelectorControl.ConfigurableObject;
                Settings.SourceListPath = _sourceListPathTextBox.Text;
                Settings.SynonymMapPath = _synonymMapPathTextBox.Text;
                Settings.MinimumMatchScore = (double)_minimumMatchScoreNumericUpDown.Value;

                if (_doNothingRadioButton.Checked)                  Settings.UnableToTranslateAction = NoGoodMatchAction.DoNothing;
                else if (_clearValueRadioButton.Checked)            Settings.UnableToTranslateAction = NoGoodMatchAction.ClearValue;
                else if (_setTypeToUntranslatedRadioButton.Checked) Settings.UnableToTranslateAction = NoGoodMatchAction.SetTypeToUntranslated;
                else if (_removeAttributeRadioButton.Checked)       Settings.UnableToTranslateAction = NoGoodMatchAction.RemoveAttribute;

                Settings.CreateBestMatchScoreSubAttribute = _createScoreSubattributeCheckBox.Checked;

                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45487");
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
            if (_attributeSelectorControl.ConfigurableObject == null)
            {
                _attributeSelectorControl.Focus();
                UtilityMethods.ShowMessageBox("Please specify an attribute selector to use.",
                    "Specify attribute selector", false);
                return true;
            }

            if (_attributeSelectorControl.ConfigurableObject is IMustBeConfiguredObject configurable
                && !configurable.IsConfigured())
            {
                _attributeSelectorControl.Focus();
                UtilityMethods.ShowMessageBox("The selected attribute selector has not been " +
                    "properly configured.",
                    "Attribute selector not configured", false);
                return true;
            }

            if (string.IsNullOrWhiteSpace(_sourceListPathTextBox.Text))
            {
                _sourceListPathTextBox.Focus();
                UtilityMethods.ShowMessageBox("Please specify a list of translation targets.",
                    "Specify translate-to list", false);
                return true;
            }

            return false;
        }

        #endregion Private Members
    }
}
