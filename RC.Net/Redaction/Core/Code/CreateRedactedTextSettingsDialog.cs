using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Extract.Redaction
{
    /// <summary>
    /// A <see cref="Form"/> to view and modify <see cref="CreateRedactedTextSettings"/> instances.
    /// </summary>
    public partial class CreateRedactedTextSettingsDialog : Form
    {
        #region Fields

        /// <summary>
        /// The settings to display.
        /// </summary>
        CreateRedactedTextSettings _settings;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateRedactedTextSettingsDialog"/> class.
        /// </summary>
        /// <param name="settings">TThe settings to display.</param>
        public CreateRedactedTextSettingsDialog(CreateRedactedTextSettings settings)
        {
            try
            {
                InitializeComponent();

                _settings = settings ?? new CreateRedactedTextSettings();

                _outputPathTagsButton.PathTags = new FileActionManagerPathTags();

                // Configure all check boxes to enable/disable dependent controls when their check
                // state is changed.
                _redactSpecificTypesRadioButton.CheckedChanged += ((sender, e) =>
                    {
                        bool enable = _redactSpecificTypesRadioButton.Checked;
                        _highConfidenceDataCheckBox.Enabled = enable;
                        _mediumConfidenceDataCheckBox.Enabled = enable;
                        _lowConfidenceDataCheckBox.Enabled = enable;
                        _manualDataCheckBox.Enabled = enable;
                        _otherDataCheckBox.Enabled = enable;
                        _dataTypesTextBox.Enabled = enable && _otherDataCheckBox.Checked;
                    });

                _otherDataCheckBox.CheckedChanged += ((sender, e) =>
                    _dataTypesTextBox.Enabled = _otherDataCheckBox.Checked);

                _replaceCharactersRadioButton.CheckedChanged += ((sender, e) =>
                    {
                        _charsToReplaceComboBox.Enabled = _replaceCharactersRadioButton.Checked;
                        _replacementValueTextBox.Enabled = _replaceCharactersRadioButton.Checked;
                    });

                _surroundTextRadioButton.CheckedChanged += ((sender, e) =>
                    _xmlElementTextBox.Enabled = _surroundTextRadioButton.Checked);

                _okButton.Click += HandleOkButtonClick;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI31633", ex);
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets the specified <see cref="CreateRedactedTextSettings"/>.
        /// </summary>
        public CreateRedactedTextSettings CreateRedactedTextSettings
        {
            get
            {
                return _settings;
            }
        }

        #endregion Properties

        #region Overrides

        /// <summary>
        /// Raises the <see cref="Form.Load"/> event.
        /// </summary>
        /// <param name="e">The event data associated with the <see cref="Form.Load"/> event.</param>
        protected override void OnLoad(EventArgs e)
        {
            try
            {
                base.OnLoad(e);

                // Initialize the values and enabled states of the controls based on _settings.
                _redactAllTypesRadioButton.Checked = _settings.RedactAllTypes;
                _redactSpecificTypesRadioButton.Checked = !_settings.RedactAllTypes;

                bool enableDataCheckboxes = _redactSpecificTypesRadioButton.Checked;

                _highConfidenceDataCheckBox.Enabled = enableDataCheckboxes;
                _highConfidenceDataCheckBox.Checked =
                    _settings.DataTypes.Contains(Constants.HCData, StringComparer.OrdinalIgnoreCase);

                _mediumConfidenceDataCheckBox.Enabled = enableDataCheckboxes;
                _mediumConfidenceDataCheckBox.Checked =
                    _settings.DataTypes.Contains(Constants.MCData, StringComparer.OrdinalIgnoreCase);

                _lowConfidenceDataCheckBox.Enabled = enableDataCheckboxes;
                _lowConfidenceDataCheckBox.Checked =
                    _settings.DataTypes.Contains(Constants.LCData, StringComparer.OrdinalIgnoreCase);

                _manualDataCheckBox.Enabled = enableDataCheckboxes;
                _manualDataCheckBox.Checked =
                    _settings.DataTypes.Contains(Constants.Manual, StringComparer.OrdinalIgnoreCase);

                _dataTypesTextBox.Text = string.Join(", ", GetOtherDataTypesFromSettings());
                _dataTypesTextBox.Enabled = enableDataCheckboxes && _otherDataCheckBox.Checked;

                _otherDataCheckBox.Enabled = enableDataCheckboxes;
                _otherDataCheckBox.Checked =
                    _settings.RedactOtherTypes && !string.IsNullOrEmpty(_dataTypesTextBox.Text);

                _replaceCharactersRadioButton.Checked = _settings.ReplaceCharacters;
                _surroundTextRadioButton.Checked = !_settings.ReplaceCharacters;

                _charsToReplaceComboBox.SetSelectedText(
                    (_settings.CharactersToReplace == CharacterClass.All) ? "all" : "alpha numeric");

                _replacementValueTextBox.Text = _settings.ReplacementValue;
                _replacementValueTextBox.Enabled = _replaceCharactersRadioButton.Checked;

                _xmlElementTextBox.Text = _settings.XmlElementName;
                _xmlElementTextBox.Enabled = _surroundTextRadioButton.Checked;

                _dataFileControl.DataFile = _settings.DataFile;

                _outputLocationTextBox.Text = _settings.OutputFileName;
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI31650", ex);
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

                // Apply the new settings.
                _settings = GetCreateRedactedTextSettings();
                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI31649", ex);
            }
        }

        #endregion Event Handlers

        #region Private Members

        /// <summary>
        /// Gets the <see cref="GetCreateRedactedTextSettings"/> specified via the UI.
        /// </summary>
        /// <returns>The <see cref="GetCreateRedactedTextSettings"/> specified via the UI.</returns>
        CreateRedactedTextSettings GetCreateRedactedTextSettings()
        {
            try
            {
                bool replaceAllTypes = _redactAllTypesRadioButton.Checked;
                bool specifyOtherTypes = _otherDataCheckBox.Checked;
                string[] dataTypes = GetDataTypes();
                bool replaceCharacters = _replaceCharactersRadioButton.Checked;
                CharacterClass charactersToReplace = (_charsToReplaceComboBox.Text == "all")
                    ? CharacterClass.All
                    : CharacterClass.Alphanumeric;
                string replacementValue = _replacementValueTextBox.Text;
                string xmlElementName = _xmlElementTextBox.Text;
                string dataFile = _dataFileControl.DataFile;
                string outputFileName = _outputLocationTextBox.Text;

                return new CreateRedactedTextSettings(replaceAllTypes, specifyOtherTypes, dataTypes,
                    replaceCharacters, charactersToReplace, replacementValue, xmlElementName,
                    dataFile, outputFileName);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI31652");
            }
        }

        /// <summary>
        /// Gets the user specified data types.
        /// <para><b>Note</b></para>
        /// These data types are not necessarily configured for redaction.
        /// </summary>
        /// <returns>The user specified data types.</returns>
        string[] GetDataTypes()
        {
            HashSet<string> dataTypes = new HashSet<string>();
            if (_highConfidenceDataCheckBox.Checked)
            {
                dataTypes.Add(Constants.HCData);
            }
            if (_mediumConfidenceDataCheckBox.Checked)
            {
                dataTypes.Add(Constants.MCData);
            }
            if (_lowConfidenceDataCheckBox.Checked)
            {
                dataTypes.Add(Constants.LCData);
            }
            if (_manualDataCheckBox.Checked)
            {
                dataTypes.Add(Constants.Manual);
            }

            foreach (string dataType in _dataTypesTextBox.Text.Split(new char[] { ',', ' ' },
                StringSplitOptions.RemoveEmptyEntries))
            {
                dataTypes.Add(dataType);
            }

            return dataTypes.ToArray();
        }

        /// <summary>
        /// Retrieves and array of non-standard ID Shield data types contained in _settings.
        /// </summary>
        /// <returns>The non-standard ID Shield data types.</returns>
        string[] GetOtherDataTypesFromSettings()
        {
            HashSet<string> otherDataTypes = new HashSet<string>(_settings.DataTypes);
            otherDataTypes.RemoveWhere(type =>
                CreateRedactedTextSettings.StandardDataTypes.Contains(
                    type, StringComparer.OrdinalIgnoreCase));

            return otherDataTypes.ToArray();
        }

        /// <summary>
        /// Displays a warning message if the user specified settings are invalid.
        /// </summary>
        /// <returns><see langword="true"/> if the settings are invalid; <see langword="false"/> if 
        /// the settings are valid.</returns>
         bool WarnIfInvalid()
        {
            // Must have at least one data type specified
            if (_redactSpecificTypesRadioButton.Checked)
            {
                string[] dataTypes = GetDataTypes();
                if (dataTypes.Length == 0)
                {
                    _dataTypesTextBox.Focus();
                    MessageBox.Show("Please specify data types to replace.", "Invalid data types",
                        MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button1,
                        0);
                    return true;
                }
            }

            if (_surroundTextRadioButton.Checked &&
                string.IsNullOrWhiteSpace(_xmlElementTextBox.Text))
            {
                _xmlElementTextBox.Focus();
                MessageBox.Show("Please specify the name of the XML element used to surround sensitive text",
                    "Specify XML element name", MessageBoxButtons.OK, MessageBoxIcon.None,
                    MessageBoxDefaultButton.Button1, 0);
                return true;
            }

            if (string.IsNullOrWhiteSpace(_outputLocationTextBox.Text))
            {
                _outputLocationTextBox.Focus();
                MessageBox.Show("Please specify the output filename.", "Missing output location",
                    MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button1,
                    0);
                return true;
            }
            

            return false;
        }

        #endregion Private Members
    }
}
