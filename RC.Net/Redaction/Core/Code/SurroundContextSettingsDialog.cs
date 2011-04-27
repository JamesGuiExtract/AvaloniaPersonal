using Extract.Utilities;
using System;
using System.Windows.Forms;

namespace Extract.Redaction
{
    /// <summary>
    /// Represents a dialog that allows the user to select settings for the <see cref="SurroundContextTask"/>.
    /// </summary>
    public partial class SurroundContextSettingsDialog : Form
    {
        #region Fields
		
        /// <summary>
        /// Settings for the <see cref="SurroundContextTask"/>.
        /// </summary>
        SurroundContextSettings _settings;
 
        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new <see cref="SurroundContextSettingsDialog"/> class.
        /// </summary>
        public SurroundContextSettingsDialog() 
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SurroundContextSettingsDialog"/> class.
        /// </summary>
        /// <param name="settings">Settings for the <see cref="SurroundContextTask"/>.</param>
        public SurroundContextSettingsDialog(SurroundContextSettings settings)
        {
            InitializeComponent();

            _settings = settings ?? new SurroundContextSettings();
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the settings for the <see cref="SurroundContextTask"/>.
        /// </summary>
        /// <value>Settings for the <see cref="SurroundContextTask"/>.</value>
        public SurroundContextSettings SurroundContextSettings
        {
            get
            {
                return _settings;
            }
            set
            {
                _settings = value;
            }
        }
		 
        #endregion Properties

        #region Methods

        /// <summary>
        /// Gets the <see cref="SurroundContextSettings"/> from the user interface.
        /// </summary>
        /// <returns>The <see cref="SurroundContextSettings"/> from the user interface.</returns>
        SurroundContextSettings GetSurroundContextSettings()
        {
            bool extendAllTypes = _extendAllTypesRadioButton.Checked;
            string[] dataTypes = GetDataTypes();
            bool redactWords = _redactWordsCheckBox.Checked;
            int maxWords = (int) _maxWordsNumericUpDown.Value;
            bool extendHeight = _extendHeightCheckBox.Checked;
            string dataFile = _dataFileControl.DataFile;

            return new SurroundContextSettings(extendAllTypes, dataTypes, redactWords, maxWords, 
                extendHeight, dataFile);
        }

        /// <summary>
        /// Gets the user specified data types to extend.
        /// </summary>
        /// <returns>The user specified data type to extend.</returns>
        string[] GetDataTypes()
        {
            return _dataTypesTextBox.Text.Split(new char[] {',', ' '},
                StringSplitOptions.RemoveEmptyEntries);
        }

        /// <summary>
        /// Displays a warning message if the user specified settings are invalid.
        /// </summary>
        /// <returns><see langword="true"/> if the settings are invalid; <see langword="false"/> if 
        /// the settings are valid.</returns>
        bool WarnIfInvalid()
        {
            // Must have at least one data type specified
            if (_extendSpecificTypesRadioButton.Checked)
            {
                string[] dataTypes = GetDataTypes();
                if (dataTypes.Length == 0)
                {
                    _dataTypesTextBox.Focus();
                    MessageBox.Show("Please specify data types to extend.", "Invalid data types",
                        MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button1,
                        0);
                    return true;
                }
                else
                {
                    // Need to validate that the specified data types are valid identifiers
                    if (!UtilityMethods.IsValidIdentifier(dataTypes))
                    {
                        _dataTypesTextBox.Focus();
                        MessageBox.Show("Data types must be valid identifiers. "
                            + @"Valid identifers must match the pattern ^[_a-zA-Z]\w*$",
                            "Invalid data types", MessageBoxButtons.OK, MessageBoxIcon.None,
                            MessageBoxDefaultButton.Button1, 0);
                        return true;
                    }
                }
            }

            // Must have one of the context checkboxes checked.
            if (!_redactWordsCheckBox.Checked && !_extendHeightCheckBox.Checked)
            {
                _redactWordsCheckBox.Focus();
                MessageBox.Show("Please select context to redact.", 
                    "No context selected for redaction.", MessageBoxButtons.OK, 
                    MessageBoxIcon.None, MessageBoxDefaultButton.Button1, 0);
                return true;
            }

            return false;
        }
		 
        #endregion Methods

        #region Overrides

        /// <summary>
        /// Raises the <see cref="Form.Load"/> event.
        /// </summary>
        /// <param name="e">The event data associated with the <see cref="Form.Load"/> event.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            try
            {
                if (_settings.ExtendAllTypes)
                {
                    _extendAllTypesRadioButton.Checked = true;
                }
                else
                {
                    _extendSpecificTypesRadioButton.Checked = true;
                }

                _dataTypesTextBox.Text = string.Join(", ", _settings.GetDataTypes());
                _redactWordsCheckBox.Checked = _settings.RedactWords;
                _maxWordsNumericUpDown.Value = _settings.MaxWords;
                _maxWordsNumericUpDown.ValueChanged += HandleMaxWordsNumericUpDown_ValueChanged;
                _extendHeightCheckBox.Checked = _settings.ExtendHeight;
                _dataFileControl.DataFile = _settings.DataFile;
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI29513", ex);
            }
        }

        /// <summary>
        /// Handles the case that _maxWordsNumericUpDown's value has changed.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">The event data associated with the event.</param>
        void HandleMaxWordsNumericUpDown_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                // [FlexIDSCore:4046]
                // If a decimal value is typed in, the control's text will be updated to reflect the
                // NumericUpDown settings (resulting in rounding), but the control's actual value
                // will not change. Depending upon how the value is stored, this can result in a
                // different value being stored than is displayed. Therefore, take matters into our
                // own hands and programatically round any non-interger value.
                decimal roundedValue = decimal.Round(_maxWordsNumericUpDown.Value);
                if (roundedValue != _maxWordsNumericUpDown.Value)
                {
                    _maxWordsNumericUpDown.Value = roundedValue;
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI29894", ex);
            }
        }

        #endregion Overrides

        #region Event Handlers

        /// <summary>
        /// Handles the <see cref="RadioButton.CheckedChanged"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="RadioButton.CheckedChanged"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="RadioButton.CheckedChanged"/> event.</param>
        void HandleExtendSpecificTypesRadioButtonCheckedChanged(object sender, EventArgs e)
        {
            try
            {
                _dataTypesTextBox.Enabled = _extendSpecificTypesRadioButton.Checked;
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI29522", ex);
            }
        }

        /// <summary>
        /// Handles the <see cref="CheckBox.CheckedChanged"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="CheckBox.CheckedChanged"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="CheckBox.CheckedChanged"/> event.</param>
        void HandleRedactWordsCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            try
            {
                _maxWordsNumericUpDown.Enabled = _redactWordsCheckBox.Checked;
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI29523", ex);
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="Control.Click"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="Control.Click"/> event.</param>
        void HandleOkButtonClick(object sender, EventArgs e)
        {
            try
            {
                if (WarnIfInvalid())
                {
                    return;
                }

                // Store settings
                _settings = GetSurroundContextSettings();
                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI29514", ex);
            }
        }
		 
        #endregion Event Handlers
    }
}