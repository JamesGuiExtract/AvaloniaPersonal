using Extract.Utilities;
using System;
using System.Windows.Forms;

namespace Extract.UtilityApplications.NERAnnotation
{
    public partial class FunctionConfigurationDialog : Form
    {
        #region Properties

        public NERAnnotatorSettings Settings { get; internal set; }

        #endregion Properties

        #region Constructors

        public FunctionConfigurationDialog(NERAnnotatorSettings settings)
        {
            try
            {
                Settings = settings;

                InitializeComponent();

                // Set tooltip text to indicate supported function names
                var requiredFunctions = UtilityMethods.FormatInvariant(
                    $@"Required functions (all of type EntitiesAndPage -> EntitiesAndPage): { string.Join(", ", NERAnnotator.EntityFilteringFunctionNames) }");
                toolTip1.SetToolTip(_runEntityFilteringFunctionsCheckBox, requiredFunctions);
                toolTip1.SetToolTip(_entityFilteringFunctionsGroupBox, requiredFunctions);
                toolTip1.SetToolTip(_entityFilteringScriptFileTextBox, requiredFunctions);
                toolTip1.SetToolTip(_runEntityFilteringFunctionsInfoTip, requiredFunctions);

                SetControlValues();
                UpdateButtonStates();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI46964");
            }
        }

        #endregion Constructors

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

                Settings.RunPreprocessingFunction = _runPreprocessingFunctionCheckBox.Checked;
                Settings.PreprocessingScript = _preprocessingScriptFileTextBox.Text;
                Settings.PreprocessingFunctionName = _preprocessingFunctionNameTextBox.Text;

                Settings.RunEntityFilteringFunctions = _runEntityFilteringFunctionsCheckBox.Checked;
                Settings.EntityFilteringScript = _entityFilteringScriptFileTextBox.Text;

                Settings.RunCharacterReplacingFunction = _runCharReplacingFunctionCheckBox.Checked;
                Settings.CharacterReplacingScript = _charReplacingScriptFileTextBox.Text;
                Settings.CharacterReplacingFunctionName = _charReplacingFunctionNameTextBox.Text;

                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI46961");
            }
        }

        private void CheckBox_CheckedChanged(object sender, EventArgs e)
        {
            UpdateButtonStates();
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
            if (_runPreprocessingFunctionCheckBox.Checked && string.IsNullOrWhiteSpace(_preprocessingScriptFileTextBox.Text))
            {
                _preprocessingScriptFileTextBox.Focus();
                UtilityMethods.ShowMessageBox("Please specify a script file to use.",
                    "Specify script file", false);
                return true;
            }

            if (_runPreprocessingFunctionCheckBox.Checked && string.IsNullOrWhiteSpace(_preprocessingFunctionNameTextBox.Text))
            {
                _preprocessingFunctionNameTextBox.Focus();
                UtilityMethods.ShowMessageBox("Please specify a function name to call.",
                    "Specify function name", false);
                return true;
            }

            if (_runEntityFilteringFunctionsCheckBox.Checked && string.IsNullOrWhiteSpace(_entityFilteringScriptFileTextBox.Text))
            {
                _entityFilteringScriptFileTextBox.Focus();
                UtilityMethods.ShowMessageBox("Please specify a script file to use.",
                    "Specify script file", false);
                return true;
            }

            if (_runCharReplacingFunctionCheckBox.Checked && string.IsNullOrWhiteSpace(_charReplacingScriptFileTextBox.Text))
            {
                _charReplacingScriptFileTextBox.Focus();
                UtilityMethods.ShowMessageBox("Please specify a script file to use.",
                    "Specify script file", false);
                return true;
            }

            if (_runCharReplacingFunctionCheckBox.Checked && string.IsNullOrWhiteSpace(_charReplacingFunctionNameTextBox.Text))
            {
                _charReplacingFunctionNameTextBox.Focus();
                UtilityMethods.ShowMessageBox("Please specify a function name to call.",
                    "Specify function name", false);
                return true;
            }

            return false;
        }

        void SetControlValues()
        {
            _runPreprocessingFunctionCheckBox.Checked = Settings.RunPreprocessingFunction;
            _preprocessingScriptFileTextBox.Text = Settings.PreprocessingScript;
            _preprocessingFunctionNameTextBox.Text = Settings.PreprocessingFunctionName;

            _runEntityFilteringFunctionsCheckBox.Checked = Settings.RunEntityFilteringFunctions;
            _entityFilteringScriptFileTextBox.Text = Settings.EntityFilteringScript;

            _runCharReplacingFunctionCheckBox.Checked = Settings.RunCharacterReplacingFunction;
            _charReplacingScriptFileTextBox.Text = Settings.CharacterReplacingScript;
            _charReplacingFunctionNameTextBox.Text = Settings.CharacterReplacingFunctionName;
        }

        void UpdateButtonStates()
        {
            try
            {
                _preprocessingFunctionGroupBox.Enabled = _runPreprocessingFunctionCheckBox.Checked;
                _entityFilteringFunctionsGroupBox.Enabled = _runEntityFilteringFunctionsCheckBox.Checked;
                _charReplacingFunctionGroupBox.Enabled = _runCharReplacingFunctionCheckBox.Checked;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI46963");
            }
        }

        #endregion Private Members
    }
}
