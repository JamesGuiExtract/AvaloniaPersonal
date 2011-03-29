using Extract.Utilities;
using Extract.Utilities.Forms;
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
        #region Constants

        /// <summary>
        /// The label for the option to add a random number of characters to redacted text.
        /// </summary>
        static readonly string _ADD_CHARACTERS_LABEL =
            "\"X\" characters to obscure length of sensitive text.";

        #endregion Constants

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

                foreach (CharacterClass characterClass in Enum.GetValues(typeof(CharacterClass)))
                {
                    _charsToReplaceComboBox.Items.Add(characterClass.ToReadableString());
                }

                _settings = settings ?? new CreateRedactedTextSettings();

                _outputPathTagsButton.PathTags = new FileActionManagerPathTags();

                // Configure all controls to enable/disable dependent controls as appropriate when
                // their state changes.
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
                    _dataTypesTextBox.Enabled = 
                        _redactSpecificTypesRadioButton.Checked && _otherDataCheckBox.Checked);

                _replaceCharactersRadioButton.CheckedChanged += ((sender, e) =>
                    {
                        try
                        {
                            _charsToReplaceComboBox.Enabled = _replaceCharactersRadioButton.Checked;
                            _replacementCharTextBox.Enabled = _replaceCharactersRadioButton.Checked;
                            SetAddCharactersOptionEnabledStatus();
                        }
                        catch (Exception ex)
                        {
                            ex.ExtractDisplay("ELI31696");
                        }
                    });

                _charsToReplaceComboBox.SelectedIndexChanged += ((sender, e) =>
                    {
                        try
                        {
                            SetAddCharactersOptionEnabledStatus();
                        }
                        catch (Exception ex)
                        {
                            ex.ExtractDisplay("ELI31698");
                        }
                    });

                _replacementCharTextBox.TextChanged += ((sender, e) =>
                    {
                        try
                        {
                            _addCharactersLabel2.Text =
                                _ADD_CHARACTERS_LABEL.Replace("X", _replacementCharTextBox.Text);
                            SetAddCharactersOptionEnabledStatus();
                        }
                        catch (Exception ex)
                        {
                            ex.ExtractDisplay("ELI31699");
                        }
                    });

                _addCharactersCheckBox.CheckedChanged += ((sender, e) =>
                    {
                        try
                        {
                            SetAddCharactersOptionEnabledStatus();
                        }
                        catch (Exception ex)
                        {
                            ex.ExtractDisplay("ELI31700");
                        }
                    });

                _addCharactersUpDown.UserTextCorrected += ((sender, e) =>
                    {
                        try
                        {
                            MessageBox.Show("The maximum number of charaters to add to each " +
                                "redaction must be between 1 and 99", "Invalid number of characters.",
                                MessageBoxButtons.OK, MessageBoxIcon.None,
                                MessageBoxDefaultButton.Button1, 0);
                            _addCharactersUpDown.Focus();
                        }
                        catch (Exception ex)
                        {
                            ex.ExtractDisplay("ELI31701");
                        }
                    });

                _replaceTextRadioButton.CheckedChanged += ((sender, e) =>
                    _replacementTextTextBox.Enabled = _replaceTextRadioButton.Checked);

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

                switch (_settings.RedactionMethod)
                {
                    case RedactionMethod.ReplaceCharacters:
                        {
                            _replaceCharactersRadioButton.Checked = true;
                        }
                        break;

                    case RedactionMethod.ReplaceText:
                        {
                            _replaceTextRadioButton.Checked = true;
                        }
                        break;

                    case RedactionMethod.SurroundWithXml:
                        {
                            _surroundTextRadioButton.Checked = true;
                        }
                        break;
                }

                _charsToReplaceComboBox.SelectedIndex = (int)_settings.CharactersToReplace;
                _charsToReplaceComboBox.Enabled = _replaceCharactersRadioButton.Checked;

                _replacementCharTextBox.Text = _settings.ReplacementCharacter;
                _replacementCharTextBox.Enabled = _replaceCharactersRadioButton.Checked;

                _addCharactersCheckBox.Checked = _settings.AddCharactersToRedaction;
                _addCharactersUpDown.Value = _settings.MaxNumberAddedCharacters;
                _addCharactersLabel2.Text =
                    _ADD_CHARACTERS_LABEL.Replace("X", _replacementCharTextBox.Text);

                SetAddCharactersOptionEnabledStatus();

                _replacementTextTextBox.Text = _settings.ReplacementText;
                _replacementTextTextBox.Enabled = _replaceTextRadioButton.Checked;

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
                RedactionMethod redactionMethod;
                if (_replaceCharactersRadioButton.Checked)
                {
                    redactionMethod = RedactionMethod.ReplaceCharacters;
                }
                else if (_replaceTextRadioButton.Checked)
                {
                    redactionMethod = RedactionMethod.ReplaceText;
                }
                else
                {
                    redactionMethod = RedactionMethod.SurroundWithXml;
                }
                CharacterClass charactersToReplace =
                    (CharacterClass)_charsToReplaceComboBox.SelectedIndex;
                string replacementChar = _replacementCharTextBox.Text;
                bool addCharsToRedaction = _addCharactersCheckBox.Checked;
                int maxNumberAddedCharacters = (int)_addCharactersUpDown.Value;
                string replacementText = _replacementTextTextBox.Text;
                string xmlElementName = _xmlElementTextBox.Text;
                string dataFile = _dataFileControl.DataFile;
                string outputFileName = _outputLocationTextBox.Text;

                return new CreateRedactedTextSettings(replaceAllTypes, specifyOtherTypes, dataTypes,
                    redactionMethod, charactersToReplace, replacementChar, addCharsToRedaction,
                    maxNumberAddedCharacters, replacementText, xmlElementName, dataFile, outputFileName);
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

            if (_surroundTextRadioButton.Checked)
            {
                try
                {
                    UtilityMethods.ValidateXmlElementName(_xmlElementTextBox.Text);
                }
                catch
                {
                    _xmlElementTextBox.Focus();
                    MessageBox.Show("The XML element name is missing or invalid.",
                        "Invalid XML element name", MessageBoxButtons.OK, MessageBoxIcon.None,
                        MessageBoxDefaultButton.Button1, 0);
                    return true;
                }
            }

            if (string.IsNullOrWhiteSpace(_dataFileControl.DataFile))
            {
                _outputLocationTextBox.Focus();
                MessageBox.Show("Please specify the data file containing the redaction data.",
                    "Missing data filename", MessageBoxButtons.OK, MessageBoxIcon.None,
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

         /// <summary>
         /// Sets the enabled state of all <see cref="Control"/>s associated with the option to add
         /// characters to redacted items to obscure the length of the original data.
         /// </summary>
        void SetAddCharactersOptionEnabledStatus()
        {
            bool enable = _replaceCharactersRadioButton.Checked &&
                          _charsToReplaceComboBox.SelectedIndex == (int)CharacterClass.All &&
                          !string.IsNullOrEmpty(_replacementCharTextBox.Text);

            _addCharactersCheckBox.Enabled = enable;
            enable &= _addCharactersCheckBox.Checked;

            _addCharactersLabel1.Enabled = enable && enable;
            _addCharactersLabel2.Enabled = enable && enable;
            _addCharactersUpDown.Enabled = enable && enable;
        }
        
        #endregion Private Members
    }
}
