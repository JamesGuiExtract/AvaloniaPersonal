using System;
using System.Windows.Forms;
using Extract.Licensing;
using Extract.Utilities;
using UCLID_AFCORELib;
using UCLID_AFSELECTORSLib;
using UCLID_COMUTILSLib;

namespace Extract.AttributeFinder.Rules
{
    /// <summary>
    /// Allows configuration of a <see cref="SetDocumentTags"/> instance.
    /// </summary>
    [CLSCompliant(false)]
    public partial class SetDocumentTagsSettingsDialog : Form
    {
        #region Constants

        /// <summary>
        /// The object name.
        /// </summary>
        static readonly string _OBJECT_NAME =
            typeof(SetDocumentTagsSettingsDialog).ToString();

        #endregion Constants

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SetDocumentTagsSettingsDialog"/> class.
        /// </summary>
        /// <param name="settings">The settings.</param>
        public SetDocumentTagsSettingsDialog(SetDocumentTags settings)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.RuleSetEditorUIObject,
                    "ELI38560", _OBJECT_NAME);

                Settings = settings;

                InitializeComponent();

                _stringTagPathTagsButton.PathTags = new AttributeFinderPathTags();
                _objectTagPathTagsButton.PathTags = new AttributeFinderPathTags();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI38561");
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the <see cref="SetDocumentTags"/> to configure.
        /// </summary>
        /// <value>The <see cref="SetDocumentTags"/> to configure.</value>
        public SetDocumentTags Settings
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
                    // Set values
                    _stringTagName.Text = Settings.StringTagName;
                    _delimiter.Text = ParamEscape(Settings.Delimiter);
                    _useSpecifiedValueForStringTag.Checked = Settings.UseSpecifiedValueForStringTag;
                    _specifiedValueForStringTag.Text = Settings.SpecifiedValueForStringTag;
                    _useTagValueForStringTag.Checked = Settings.UseTagValueForStringTag;
                    _tagNameForStringTagValue.Text = Settings.TagNameForStringTagValue;
                    _useSelectedAttributesForStringTag.Checked = Settings.UseSelectedAttributesForStringTagValue;
                    _stringTagAttributeSelector.ConfigurableObject = 
                        (ICategorizedComponent)Settings.StringTagAttributeSelector;

                    _objectTagName.Text = Settings.ObjectTagName;
                    _useSpecifiedValueForObjectTag.Checked = Settings.UseSpecifiedValueForObjectTag;
                    _specifiedValueForObjectTag.Text = Settings.SpecifiedValueForObjectTag;
                    _useSelectedAttributesForObjectTag.Checked = Settings.UseSelectedAttributesForObjectTagValue;
                    _objectTagAttributeSelector.ConfigurableObject = 
                        (ICategorizedComponent)Settings.ObjectTagAttributeSelector;

                    // Set state of main check boxes
                    _setStringTagCheckBox.Checked = Settings.SetStringTag;
                    _setObjectTagCheckBox.Checked = Settings.SetObjectTag;

                    // Enable/disable all controls based on checked states
                    SetEnabledStates();
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI38562");
            }
        }

        #endregion Overrides

        #region Event Handlers

        /// <summary>
        /// Handles the <see cref="T:CheckBox.CheckChanged"/> event for
        /// various <see cref="System.Windows.Forms.CheckBox"/>s.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleCheckChanged(object sender, System.EventArgs e)
        {
            try
            {
                SetEnabledStates();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI38563");
            }
        }

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

                Settings.SetStringTag = _setStringTagCheckBox.Checked;
                Settings.Delimiter = ParamUnescape(_delimiter.Text);
                Settings.StringTagName = _stringTagName.Text;
                Settings.UseSpecifiedValueForStringTag = _useSpecifiedValueForStringTag.Checked;
                Settings.SpecifiedValueForStringTag = _specifiedValueForStringTag.Text;
                Settings.UseTagValueForStringTag = _useTagValueForStringTag.Checked;
                Settings.TagNameForStringTagValue = _tagNameForStringTagValue.Text;
                Settings.UseSelectedAttributesForStringTagValue =
                    _useSelectedAttributesForStringTag.Checked;
                Settings.StringTagAttributeSelector =
                    (IAttributeSelector)_stringTagAttributeSelector.ConfigurableObject;

                Settings.SetObjectTag = _setObjectTagCheckBox.Checked;
                Settings.ObjectTagName = _objectTagName.Text;
                Settings.UseSpecifiedValueForObjectTag = _useSpecifiedValueForObjectTag.Checked;
                Settings.SpecifiedValueForObjectTag = _specifiedValueForObjectTag.Text;
                Settings.UseSelectedAttributesForObjectTagValue =
                    _useSelectedAttributesForObjectTag.Checked;
                Settings.ObjectTagAttributeSelector =
                    (IAttributeSelector)_objectTagAttributeSelector.ConfigurableObject;

                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI38564");
            }
        }

        /// <summary>
        /// Handles the click event for the <see cref="_configureToSetDocType"/> button.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleConfigureToSetDocTypeClick(object sender, EventArgs e)
        {
            try
            {
                _setStringTagCheckBox.Checked = true;
                _stringTagName.Text = "DocProbability";
                _useSpecifiedValueForStringTag.Checked = true;
                _specifiedValueForStringTag.Text = "2";

                _setObjectTagCheckBox.Checked = true;
                _objectTagName.Text = "DocType";
                _useSelectedAttributesForObjectTag.Checked = true;
                _objectTagAttributeSelector.ConfigurableObject =
                    (ICategorizedComponent)(new QueryBasedAS { QueryText = "DocumentType"});
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI38565");
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
            if (!_setStringTagCheckBox.Checked && !_setObjectTagCheckBox.Checked)
            {
                UtilityMethods.ShowMessageBox("You must specify at least one tag to set.",
                    "Specify a tag to set", false);
                return true;
            }

            if (_setStringTagCheckBox.Checked)
            {
                if (string.IsNullOrWhiteSpace(_stringTagName.Text))
                {
                    _stringTagName.Focus();
                    UtilityMethods.ShowMessageBox("Please specify a tag name.",
                        "Specify a tag name", false);
                    return true;
                }

                if (_useTagValueForStringTag.Checked
                         && string.IsNullOrWhiteSpace(_tagNameForStringTagValue.Text))
                {
                    _tagNameForStringTagValue.Focus();
                    UtilityMethods.ShowMessageBox("Please specify a tag name to use for the value.",
                        "Specify a tag name for value", false);
                    return true;
                }
                else if (_useSelectedAttributesForStringTag.Checked)
                {
                    if (_stringTagAttributeSelector.ConfigurableObject == null)
                    {
                        _stringTagAttributeSelector.Focus();
                        UtilityMethods.ShowMessageBox("Please specify an attribute selector to use.",
                            "Specify attribute selector", false);
                        return true;
                    }
                    else
                    {
                        IMustBeConfiguredObject configurable =
                            _stringTagAttributeSelector.ConfigurableObject as IMustBeConfiguredObject;
                        if (configurable != null && !configurable.IsConfigured())
                        {
                            _stringTagAttributeSelector.Focus();
                            UtilityMethods.ShowMessageBox("The selected attribute selector has not been " +
                                "properly configured.",
                                "Attribute selector not configured", false);
                            return true;
                        }
                    }
                }
            }

            if (_setObjectTagCheckBox.Checked)
            {
                if (string.IsNullOrWhiteSpace(_objectTagName.Text))
                {
                    _objectTagName.Focus();
                    UtilityMethods.ShowMessageBox("Please specify a tag name.",
                        "Specify a tag name", false);
                    return true;
                }

                if (_useSelectedAttributesForObjectTag.Checked)
                {
                    if (_objectTagAttributeSelector.ConfigurableObject == null)
                    {
                        _objectTagAttributeSelector.Focus();
                        UtilityMethods.ShowMessageBox("Please specify an attribute selector to use.",
                            "Specify attribute selector", false);
                        return true;
                    }
                    else
                    {
                        IMustBeConfiguredObject configurable =
                            _objectTagAttributeSelector.ConfigurableObject as IMustBeConfiguredObject;
                        if (configurable != null && !configurable.IsConfigured())
                        {
                            _objectTagAttributeSelector.Focus();
                            UtilityMethods.ShowMessageBox("The selected attribute selector has not been " +
                                "properly configured.",
                                "Attribute selector not configured", false);
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Sets the enabled states of all controls
        /// </summary>
        void SetEnabledStates()
        {
            // Set enabled states based on main check boxes
            _stringTagName.Enabled =
            _useSpecifiedValueForStringTag.Enabled =
            _specifiedValueForStringTag.Enabled =
            _stringTagPathTagsButton.Enabled =
            _useTagValueForStringTag.Enabled =
            _tagNameForStringTagValue.Enabled =
            _delimiter.Enabled =
            _useSelectedAttributesForStringTag.Enabled =
            _stringTagAttributeSelector.Enabled = _setStringTagCheckBox.Checked;

            _objectTagName.Enabled =
            _useSpecifiedValueForObjectTag.Enabled =
            _specifiedValueForObjectTag.Enabled =
            _objectTagPathTagsButton.Enabled =
            _useSelectedAttributesForObjectTag.Enabled =
            _objectTagAttributeSelector.Enabled = _setObjectTagCheckBox.Checked;

            // Set enabled states based on specific settings
            if (_setStringTagCheckBox.Checked)
            {
                _specifiedValueForStringTag.Enabled = _useSpecifiedValueForStringTag.Checked;
                _stringTagPathTagsButton.Enabled = _useSpecifiedValueForStringTag.Checked;
                _tagNameForStringTagValue.Enabled = _useTagValueForStringTag.Checked;
                _stringTagAttributeSelector.Enabled = _useSelectedAttributesForStringTag.Checked;
                _delimiter.Enabled = _useSelectedAttributesForStringTag.Checked || _useTagValueForStringTag.Checked;
            }

            if (_setObjectTagCheckBox.Checked)
            {
                _specifiedValueForObjectTag.Enabled = _useSpecifiedValueForObjectTag.Checked;
                _objectTagPathTagsButton.Enabled = _useSpecifiedValueForObjectTag.Checked;
                _objectTagAttributeSelector.Enabled = _useSelectedAttributesForObjectTag.Checked;
            }
        }

        /// <summary>
        /// Replaces carriage returns, line feeds and tab characters
        /// with escaped versions.
        /// </summary>
        /// <param name="parameter">The parameter to escape.</param>
        /// <returns>The escaped parameter.</returns>
        static string ParamEscape(string parameter)
        {
            if (parameter == null)
            {
                return null;
            }
            else
            {
                parameter = parameter.Replace("\r", "\\r");
                parameter = parameter.Replace("\n", "\\n");
                parameter = parameter.Replace("\t", "\\t");

                return parameter;
            }
        }

        /// <summary>
        /// Replaces printable escape sequences for carriage returns, line feeds and tab characters
        /// with the characters themselves.
        /// </summary>
        /// <param name="parameter">The parameter to unescape.</param>
        /// <returns>The unescaped parameter.</returns>
        static string ParamUnescape(string parameter)
        {
            if (parameter == null)
            {
                return null;
            }
            else
            {
                parameter = parameter.Replace("\\r", "\r");
                parameter = parameter.Replace("\\n", "\n");
                parameter = parameter.Replace("\\t", "\t");

                return parameter;
            }
        }

        #endregion Private Members
    }
}
