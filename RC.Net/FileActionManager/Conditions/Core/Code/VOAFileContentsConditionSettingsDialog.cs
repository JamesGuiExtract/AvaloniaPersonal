using Extract.Licensing;
using Extract.Utilities;
using Extract.Utilities.Forms;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms;

namespace Extract.FileActionManager.Conditions
{
    /// <summary>
    /// Allows for configuration of a <see cref="VOAFileContentsCondition"/> instance.
    /// </summary>
    public partial class VOAFileContentsConditionSettingsDialog : Form
    {
        #region Constants

        /// <summary>
        /// The object name.
        /// </summary>
        static readonly string _OBJECT_NAME =
            typeof(VOAFileContentsConditionSettingsDialog).ToString();

        #endregion Constants

        #region Constructors

        /// <summary>
        /// Initializes static data for the <see cref="VOAFileContentsConditionSettingsDialog"/>
        /// class.
        /// </summary>
        // FXCop seems to believe this is here to initialize static fields. That is not the case.
        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static VOAFileContentsConditionSettingsDialog()
        {
            try
            {
                // Assign readable values for enums to be displayed in combo boxes.
                VOAContentsConditionRequirement.ContainsAtLeast.SetReadableValue("contains at least");
                VOAContentsConditionRequirement.ContainsAtMost.SetReadableValue("contains at most");
                VOAContentsConditionRequirement.ContainsExactly.SetReadableValue("contains exactly");
                VOAContentsConditionRequirement.DoesNotContainExactly.SetReadableValue("does not contain exactly");

                AttributeField.Name.SetReadableValue("name");
                AttributeField.Value.SetReadableValue("value");
                AttributeField.Type.SetReadableValue("type");

                ComparisonOperator.Equal.SetReadableValue("equals");
                ComparisonOperator.NotEqual.SetReadableValue("is not equal to");
                ComparisonOperator.GreaterThan.SetReadableValue("is greater than");
                ComparisonOperator.GreaterThanEqual.SetReadableValue("is greater than or equal to");
                ComparisonOperator.LessThan.SetReadableValue("is less than");
                ComparisonOperator.LessThanEqual.SetReadableValue("is less than or equal to");
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI32704");
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VOAFileContentsConditionSettingsDialog"/> class.
        /// </summary>
        /// <param name="settings">The condition object.</param>
        public VOAFileContentsConditionSettingsDialog(VOAFileContentsCondition settings)
            : this()
        {
            try
            {
                Settings = settings;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI32705");
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VOAFileContentsConditionSettingsDialog"/> class.
        /// </summary>
        public VOAFileContentsConditionSettingsDialog()
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.FlexIndexCoreObjects, "ELI32715",
                    _OBJECT_NAME);

                InitializeComponent();

                _voaFilePathTags.PathTags = new FileActionManagerPathTags();

                _comparisonRadioButton.CheckedChanged += ((sender, args) =>
                    {
                        _comparisonComboBox.Enabled = _comparisonRadioButton.Checked;
                        _comparisonValueTextBox.Enabled = _comparisonRadioButton.Checked;
                        _regexCheckBox.Enabled =
                            (_comparisonComboBox.Text == ComparisonOperator.Equal.ToReadableValue()) ||
                            (_comparisonComboBox.Text == ComparisonOperator.NotEqual.ToReadableValue());
                    });

                _comparisonComboBox.SelectedIndexChanged += ((sender, args) =>
                    {
                        _regexCheckBox.Enabled =
                            (_comparisonComboBox.Text == ComparisonOperator.Equal.ToReadableValue()) ||
                            (_comparisonComboBox.Text == ComparisonOperator.NotEqual.ToReadableValue());
                    });

                _rangeRadioButton.CheckedChanged += ((sender, args) =>
                    {
                        _rangeMinValueTextBox.Enabled = _rangeRadioButton.Checked;
                        _rangeMaxValueTextBox.Enabled = _rangeRadioButton.Checked;
                        _regexCheckBox.Enabled = false;
                    });

                _searchMatchRadioButton.CheckedChanged += ((sender, args) =>
                    {
                        _searchMatchTypeComboBox.Enabled = _searchMatchRadioButton.Checked;
                        _searchTextBox.Enabled = _searchMatchRadioButton.Checked;
                        _regexCheckBox.Enabled = true;
                    });

                _inListRadioButton.CheckedChanged += ((sender, args) =>
                    {
                        _matchListBox.Enabled = _inListRadioButton.Checked;
                        _addButton.Enabled = _inListRadioButton.Checked;
                        _removeButton.Enabled = _inListRadioButton.Checked;
                        _modifyButton.Enabled = _inListRadioButton.Checked;
                        _regexCheckBox.Enabled = true;
                    });
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI32706");
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
        public VOAFileContentsCondition Settings { get; set; }

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

                // Load the combo boxes with the readable values of their associates enums.
                _containsComboBox.InitializeWithReadableEnum<VOAContentsConditionRequirement>();
                _fieldNameComboBox.InitializeWithReadableEnum<AttributeField>();
                _comparisonComboBox.InitializeWithReadableEnum<ComparisonOperator>();

                // Apply Settings values to the UI.
                if (Settings != null)
                {
                    _metComboBox.Text = Settings.MetIfTrue ? "met" : "not met";
                    _voaFileTextBox.Text = Settings.VOAFileName;
                    _containsComboBox.Text = Settings.Requirement.ToReadableValue();
                    _attributeCountUpDown.Value = Settings.AttributeCount;
                    _attributeQueryTextBox.Text = Settings.AttributeQuery;
                    _fieldNameComboBox.Text = Settings.ComparisonField.ToReadableValue();
                    _comparisonRadioButton.Checked =
                        Settings.ComparisonMethod == AttributeComparisonMethod.Comparison;
                    _comparisonComboBox.Text = Settings.ComparisonOperator.ToReadableValue();
                    _comparisonValueTextBox.Text = Settings.ComparisonValue;
                    _rangeRadioButton.Checked =
                        Settings.ComparisonMethod == AttributeComparisonMethod.Range;
                    _rangeMinValueTextBox.Text = Settings.RangeMinValue;
                    _rangeMaxValueTextBox.Text = Settings.RangeMaxValue;
                    _searchMatchRadioButton.Checked =
                        Settings.ComparisonMethod == AttributeComparisonMethod.Search;
                    _searchMatchTypeComboBox.Text = Settings.SearchFullyMatches
                        ? "fully matches" : "contains a match for";
                    _searchTextBox.Text = Settings.SearchPattern;
                    _inListRadioButton.Checked =
                        Settings.ComparisonMethod == AttributeComparisonMethod.List;
                    _matchListBox.Items.Clear();
                    if (Settings.ListValues != null)
                    {
                        foreach (string value in Settings.ListValues)
                        {
                            _matchListBox.Items.Add(value);
                        }
                    }
                    _caseSensitiveCheckBox.Checked =
                        Settings.StringComparisonMode == StringComparison.Ordinal;
                    _regexCheckBox.Checked = Settings.UseRegex;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI32707");
            }
        }

        #endregion Overrides

        #region Event Handlers

        /// <summary>
        /// Handles the attribute count corrected.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleAttributeCountCorrected(object sender, EventArgs e)
        {
            try
            {
                UtilityMethods.ShowMessageBox( "The attribute count must be between 0 and 1000.",
                    "Invalid attribute count", true);
                _attributeCountUpDown.Focus();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI32708");
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

                // Apply the UI values to the Settings instance.
                Settings.MetIfTrue = _metComboBox.Text == "met";
                Settings.VOAFileName = _voaFileTextBox.Text;
                Settings.Requirement =
                    _containsComboBox.ToReadableEnumValue<VOAContentsConditionRequirement>();
                Settings.AttributeCount = (int)_attributeCountUpDown.Value;
                Settings.AttributeQuery = _attributeQueryTextBox.Text;
                Settings.ComparisonField = _fieldNameComboBox.ToReadableEnumValue<AttributeField>();
                if (_comparisonRadioButton.Checked)
                {
                    Settings.ComparisonMethod = AttributeComparisonMethod.Comparison;
                }
                else if (_rangeRadioButton.Checked)
                {
                    Settings.ComparisonMethod = AttributeComparisonMethod.Range;
                }
                else if (_searchMatchRadioButton.Checked)
                {
                    Settings.ComparisonMethod = AttributeComparisonMethod.Search;
                }
                else if (_inListRadioButton.Checked)
                {
                    Settings.ComparisonMethod = AttributeComparisonMethod.List;
                }
                else
                {
                    throw new ExtractException("ELI32709", "Unexcepted VOA condition comparison method.");
                }

                Settings.ComparisonValue = _comparisonValueTextBox.Text;
                Settings.ComparisonOperator =
                    _comparisonComboBox.ToReadableEnumValue<ComparisonOperator>();
                Settings.RangeMinValue = _rangeMinValueTextBox.Text;
                Settings.RangeMaxValue = _rangeMaxValueTextBox.Text;
                Settings.SearchFullyMatches = _searchMatchTypeComboBox.Text == "fully matches";
                Settings.SearchPattern = _searchTextBox.Text;
                Settings.ListValues = new string[_matchListBox.Items.Count];
                _matchListBox.Items.CopyTo(Settings.ListValues, 0);
                Settings.StringComparisonMode = _caseSensitiveCheckBox.Checked
                    ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
                Settings.UseRegex = _regexCheckBox.Checked;

                Settings.ValidateSettings();

                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI32710");
            }
        }

        /// <summary>
        /// Handles the list add button click.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleListAddButtonClick(object sender, EventArgs e)
        {
            try
            {
                // Prompt for a new value to add to the list.
                var value = string.Empty;
                if (InputBox.Show(this, "Enter a value:", "Add value", ref value) == DialogResult.OK
                    && !string.IsNullOrWhiteSpace(value))
                {
                    if (_matchListBox.SelectedIndex != -1)
                    {
                        // Insert the new value at the currently selected index if possible.
                        _matchListBox.Items.Insert(_matchListBox.SelectedIndex, value);
                        _matchListBox.SelectedIndex--;
                    }
                    else
                    {
                        // Otherwise, add the new value to the end of the list.
                        _matchListBox.Items.Add(value);
                        _matchListBox.SelectedIndex = _matchListBox.Items.Count - 1;
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI32711");
            }
        }

        /// <summary>
        /// Handles the list remove button click.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleListRemoveButtonClick(object sender, EventArgs e)
        {
            try
            {
                if (_matchListBox.SelectedIndex != -1)
                {
                    _matchListBox.Items.RemoveAt(_matchListBox.SelectedIndex);
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI32712");
            }
        }

        /// <summary>
        /// Handles the list modify button click.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleListModifyButtonClick(object sender, EventArgs e)
        {
            try
            {
                // If no item is selected, there is nothing to do.
                if (_matchListBox.SelectedIndex == -1)
                {
                    return;
                }

                // Load the currently selected value into an InputBox prompt and allow it to be edited.
                var value = _matchListBox.SelectedItem.ToString();
                do
                {
                    string.IsNullOrWhiteSpace(value);
                    if (InputBox.Show(this, "Enter a value:", "Modify value", ref value) ==
                            DialogResult.Cancel)
                    {
                        break;
                    }

                    // Apply the new item if it is not blank.
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        _matchListBox.Items[_matchListBox.SelectedIndex] = value;
                        break;
                    }
                }
                while (true);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI32713");
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
            ExtractException.Assert("ELI32714", "VOA contents conditions settings have not been provided.",
                Settings != null);

            if (string.IsNullOrWhiteSpace(_voaFileTextBox.Text))
            {
                UtilityMethods.ShowMessageBox("You must specify a VOA file.",
                    "VOA file not specified", true);
                _voaFileTextBox.Focus();

                return true;
            }

            if (_searchMatchRadioButton.Checked &&
                string.IsNullOrWhiteSpace(_searchTextBox.Text))
            {
                UtilityMethods.ShowMessageBox("You must specify a value to search for.",
                    "Search pattern not specified", true);
                _searchTextBox.Focus();

                return true;
            }

            if (_inListRadioButton.Checked && _matchListBox.Items.Count < 1)
            {
                UtilityMethods.ShowMessageBox("You must specify at least one value to compare",
                    "Empty list", true);
                _matchListBox.Focus();

                return true;
            }

            return false;
        }

        #endregion Private Members
    }
}
