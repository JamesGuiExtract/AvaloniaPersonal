using AttributeDbMgrComponentsLib;
using Extract.Licensing;
using Extract.Utilities;
using System;
using System.Linq;
using System.Windows.Forms;
using UCLID_FILEPROCESSINGLib;

namespace Extract.FileActionManager.Conditions
{
    /// <summary>
    /// Allows for configuration of a <see cref="CompareAttributesCondition"/> instance.
    /// </summary>
    public partial class CompareAttributesConditionSettingsDialog : Form
    {
        #region Constants

        /// <summary>
        /// The object name.
        /// </summary>
        static readonly string _OBJECT_NAME =
            typeof(CompareAttributesConditionSettingsDialog).ToString();

        #endregion Constants

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CompareAttributesConditionSettingsDialog"/> class.
        /// </summary>
        /// <param name="settings">The condition object.</param>
        public CompareAttributesConditionSettingsDialog(CompareAttributesCondition settings)
            : this()
        {
            try
            {
                Settings = settings;

                // Apply Settings values to the UI.
                if (Settings != null)
                {
                    SetControlValues();
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45561");
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompareAttributesConditionSettingsDialog"/> class.
        /// </summary>
        public CompareAttributesConditionSettingsDialog()
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.FlexIndexIDShieldCoreObjects, "ELI45562",
                    _OBJECT_NAME);

                InitializeComponent();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45563");
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
        public CompareAttributesCondition Settings { get; set; }

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

                var fileProcessingDb = new FileProcessingDBClass();
                fileProcessingDb.ConnectLastUsedDBThisProcess();
                var attributeDBMgr = new AttributeDBMgrClass
                {
                    FAMDB = fileProcessingDb
                };
                var attributeSets = attributeDBMgr.GetAllAttributeSetNames().GetKeys().ToIEnumerable<string>().ToArray();
                _firstAttributeSetNameComboBox.Items.AddRange(attributeSets);
                _secondAttributeSetNameComboBox.Items.AddRange(attributeSets);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45564");
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
                    DialogResult = DialogResult.None;
                    return;
                }

                // Apply the UI values to the Settings instance.
                Settings.MetIfDifferent = _differentSameComboBox.Text == "different";
                Settings.FirstAttributeSetName = _firstAttributeSetNameComboBox.Text;
                Settings.SecondAttributeSetName = _secondAttributeSetNameComboBox.Text;
                if (_ignoreNoAttributesRadioButton.Checked)
                {
                    Settings.AttributesToIgnoreType = AttributeFilterType.None;
                }
                else if (_ignoreEmptyAttributesRadioButton.Checked)
                {
                    Settings.AttributesToIgnoreType = AttributeFilterType.FilterEmpty;
                }
                else
                {
                    Settings.AttributesToIgnoreType = AttributeFilterType.FilterByXPath;
                }

                Settings.XPathToIgnore = _xpathToIgnoreScintillaBox.Text;

                Settings.ValidateSettings();
            }
            catch (Exception ex)
            {
                DialogResult = DialogResult.None;
                ex.ExtractDisplay("ELI45565");
            }
        }

        /// <summary>
        /// Handles check changed events for the attribute filter radio buttons
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleIgnoreAttributesRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (_ignoreAttributesSelectedByXPathRadioButton.Checked)
                {
                    _xpathToIgnoreScintillaBox.Enabled = true;
                    _xpathToIgnoreScintillaBox.ForeColor = System.Drawing.SystemColors.WindowText;
                }
                else
                {
                    _xpathToIgnoreScintillaBox.Enabled = false;
                    _xpathToIgnoreScintillaBox.ForeColor = System.Drawing.SystemColors.GrayText;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45568");
            }
        }

        #endregion Event Handlers

        #region Private Members

        /// <summary>
        /// Sets the control values from the settings object.
        /// </summary>
        private void SetControlValues()
        {
            _differentSameComboBox.Text = Settings.MetIfDifferent ? "different" : "the same";
            _firstAttributeSetNameComboBox.Text = Settings.FirstAttributeSetName;
            _secondAttributeSetNameComboBox.Text = Settings.SecondAttributeSetName;
            switch (Settings.AttributesToIgnoreType)
            {
                case AttributeFilterType.None:
                    _ignoreNoAttributesRadioButton.Checked = true;
                    _xpathToIgnoreScintillaBox.Enabled = false;
                    _xpathToIgnoreScintillaBox.ForeColor = System.Drawing.SystemColors.GrayText;
                    break;
                case AttributeFilterType.FilterEmpty:
                    _ignoreEmptyAttributesRadioButton.Checked = true;
                    _xpathToIgnoreScintillaBox.Enabled = false;
                    _xpathToIgnoreScintillaBox.ForeColor = System.Drawing.SystemColors.GrayText;
                    break;
                case AttributeFilterType.FilterByXPath:
                    _ignoreAttributesSelectedByXPathRadioButton.Checked = true;
                    _xpathToIgnoreScintillaBox.Enabled = true;
                    _xpathToIgnoreScintillaBox.ForeColor = System.Drawing.SystemColors.WindowText;
                    break;
                default:
                    throw new ExtractException("ELI45567", "Unsupported attribute filter type: "
                        + Settings.AttributesToIgnoreType.ToString());
            }
            _xpathToIgnoreScintillaBox.Text = Settings.XPathToIgnore;
        }

        /// <summary>
        /// Displays a warning message if the user specified settings are invalid.
        /// </summary>
        /// <returns><see langword="true"/> if the settings are invalid; <see langword="false"/> if
        /// the settings are valid.</returns>
        bool WarnIfInvalid()
        {
            ExtractException.Assert("ELI45566", "CompareAttributesCondition settings have not been provided.",
                Settings != null);

            if (string.IsNullOrWhiteSpace(_firstAttributeSetNameComboBox.Text))
            {
                UtilityMethods.ShowMessageBox("First attribute set name not given",
                    "Missing attribute set name", true);
                _firstAttributeSetNameComboBox.Focus();
                return true;
            }

            if (string.IsNullOrWhiteSpace(_secondAttributeSetNameComboBox.Text))
            {
                UtilityMethods.ShowMessageBox("Second attribute set name not given",
                    "Missing attribute set name", true);
                _secondAttributeSetNameComboBox.Focus();
                return true;
            }

            if (string.Equals(_firstAttributeSetNameComboBox.Text,
                _secondAttributeSetNameComboBox.Text, StringComparison.OrdinalIgnoreCase))
            {
                UtilityMethods.ShowMessageBox("Attribute set names cannot be the same",
                    "Duplicate attribute set names", true);
                _secondAttributeSetNameComboBox.Focus();
                return true;
            }

            if (_ignoreAttributesSelectedByXPathRadioButton.Checked
                && !string.IsNullOrWhiteSpace(_xpathToIgnoreScintillaBox.Text)
                && !UtilityMethods.IsValidXPathExpression(_xpathToIgnoreScintillaBox.Text))
            {
                UtilityMethods.ShowMessageBox("Invalid XPath expression.",
                    "Invalid XPath", true);
                _xpathToIgnoreScintillaBox.Focus();
                return true;
            }

            return false;
        }

        #endregion Private Members
    }
}
