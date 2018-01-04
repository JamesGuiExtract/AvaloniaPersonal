using Extract.Licensing;
using Extract.Utilities;
using System;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Extract.AttributeFinder.Rules
{
    /// <summary>
    /// Allows configuration of a <see cref="NERFinder"/> instance.
    /// </summary>
    [CLSCompliant(false)]
    public partial class NERFinderSettingsDialog : Form
    {
        #region Constants

        /// <summary>
        /// The object name.
        /// </summary>
        static readonly string _OBJECT_NAME =
            typeof(NERFinderSettingsDialog).ToString();

        #endregion Constants

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="NERFinderSettingsDialog"/> class.
        /// </summary>
        /// <param name="settings">The settings.</param>
        public NERFinderSettingsDialog(NERFinder settings)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.RuleSetEditorUIObject,
                    "ELI44755", _OBJECT_NAME);

                Settings = settings;

                InitializeComponent();

                _sentenceDetectorPathTextBox.Enabled = _splitIntoSentencesCheckBox.Checked;
                _tokenizerPathTextBox.Enabled = _learnableTokenizerRadioButton.Checked;
                _applyLogFunctionCheckBox.Enabled =
                    _convertToPercentCheckBox.Enabled =
                        _outputConfidenceCheckBox.Checked;
                _logBaseLabel.Enabled =
                _logBaseNumericUpDown.Enabled =
                _logSteepnessLabel.Enabled =
                _logSteepnessNumericUpDown.Enabled =
                _logXMidLabel.Enabled =
                _logXMidNumericUpDown.Enabled =
                    _applyLogFunctionCheckBox.Enabled
                    && _applyLogFunctionCheckBox.Checked;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI44756");
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the <see cref="NERFinder"/> to configure.
        /// </summary>
        /// <value>The <see cref="NERFinder"/> to configure.</value>
        public NERFinder Settings
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
                    if (Settings.NameFinderType == NamedEntityRecognizer.OpenNLP)
                    {
                        _openNlpRadioButton.Checked = true;
                    }
                    else
                    {
                        _stanfordNerRadioButton.Checked = true;
                    }
                    _splitIntoSentencesCheckBox.Checked = Settings.SplitIntoSentences;
                    _sentenceDetectorPathTextBox.Text = Settings.SentenceDetectorPath ?? "";
                    switch (Settings.TokenizerType)
                    {
                        case OpenNlpTokenizer.WhiteSpaceTokenizer:
                            _whitespaceTokenizerRadioButton.Checked = true;
                            break;
                        case OpenNlpTokenizer.SimpleTokenizer:
                            _simpleTokenizerRadioButton.Checked = true;
                            break;
                        case OpenNlpTokenizer.LearnableTokenizer:
                            _learnableTokenizerRadioButton.Checked = true;
                            break;
                    };
                    _tokenizerPathTextBox.Text = Settings.TokenizerPath ?? "";
                    _nameFinderPathTextBox.Text = Settings.NameFinderPath ?? "";
                    _typesToReturnTextBox.Text = Settings.EntityTypes ?? "";

                    _outputConfidenceCheckBox.Checked = Settings.OutputConfidenceSubAttribute;
                    _applyLogFunctionCheckBox.Checked = Settings.ApplyLogFunctionToConfidence;
                    _logBaseNumericUpDown.Value = (decimal)Settings.LogBase;
                    _logSteepnessNumericUpDown.Value = (decimal)Settings.LogSteepness;
                    _logXMidNumericUpDown.Value = (decimal)Settings.LogXValueOfMiddle;
                    _convertToPercentCheckBox.Checked = Settings.ConvertConfidenceToPercent;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI44757");
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

                if (_openNlpRadioButton.Checked)
                {
                    Settings.NameFinderType = NamedEntityRecognizer.OpenNLP;
                }
                else
                {
                    Settings.NameFinderType = NamedEntityRecognizer.Stanford;
                }

                Settings.SplitIntoSentences = _splitIntoSentencesCheckBox.Checked;
                Settings.SentenceDetectorPath = _sentenceDetectorPathTextBox.Text;
                if (_whitespaceTokenizerRadioButton.Checked)
                {
                    Settings.TokenizerType = OpenNlpTokenizer.WhiteSpaceTokenizer;
                }
                else if (_simpleTokenizerRadioButton.Checked)
                {
                    Settings.TokenizerType = OpenNlpTokenizer.SimpleTokenizer;
                }
                else if (_learnableTokenizerRadioButton.Checked)
                {
                    Settings.TokenizerType = OpenNlpTokenizer.LearnableTokenizer;
                }
                Settings.TokenizerPath = _tokenizerPathTextBox.Text;
                Settings.NameFinderPath = _nameFinderPathTextBox.Text;
                Settings.EntityTypes = _typesToReturnTextBox.Text;
                Settings.OutputConfidenceSubAttribute = _outputConfidenceCheckBox.Checked;
                Settings.ApplyLogFunctionToConfidence = _applyLogFunctionCheckBox.Checked;
                Settings.LogBase = (double)_logBaseNumericUpDown.Value;
                Settings.LogSteepness = (double)_logSteepnessNumericUpDown.Value;
                Settings.LogXValueOfMiddle = (double)_logXMidNumericUpDown.Value;
                Settings.ConvertConfidenceToPercent = _convertToPercentCheckBox.Checked;
                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI44758");
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
            if (_splitIntoSentencesCheckBox.Enabled
                && _splitIntoSentencesCheckBox.Checked
                && string.IsNullOrWhiteSpace(_sentenceDetectorPathTextBox.Text))
            {
                _sentenceDetectorPathTextBox.Focus();
                UtilityMethods.ShowMessageBox("Please specify the path to a sentence detector model file.",
                    "Specify sentence detector model path", false);
                return true;
            }

            if (_learnableTokenizerRadioButton.Enabled
                && _learnableTokenizerRadioButton.Checked
                && string.IsNullOrWhiteSpace(_tokenizerPathTextBox.Text))
            {
                _tokenizerPathTextBox.Focus();
                UtilityMethods.ShowMessageBox("Please specify the path to a tokenizer model file.",
                    "Specify tokenizer model path", false);
                return true;
            }

            if (string.IsNullOrWhiteSpace(_nameFinderPathTextBox.Text))
            {
                _nameFinderPathTextBox.Focus();
                UtilityMethods.ShowMessageBox("Please specify the path to a name finder model file.",
                    "Specify name finder model path", false);
                return true;
            }

            if (!string.IsNullOrWhiteSpace(_typesToReturnTextBox.Text)
                && !Regex.IsMatch(_typesToReturnTextBox.Text, @"(?inx)\A\s*[_A-Z]\w*(\s*,\s*[_A-Z]\w*)*\s*\z"))
            {
                _typesToReturnTextBox.Focus();
                UtilityMethods.ShowMessageBox("Please specify a comma-separated list of entity types to be returned. Leave empty for all types.",
                    "Specify types to return", false);
                return true;
            }

            return false;
        }

        private void SplitIntoSentencesCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            _sentenceDetectorPathTextBox.Enabled = _splitIntoSentencesCheckBox.Checked;
        }

        private void LearnableTokenizerRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            _tokenizerPathTextBox.Enabled = _learnableTokenizerRadioButton.Checked;
        }

        private void StanfordNerRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            _sentenceDetectorGroupBox.Enabled =
                _tokenizerGroupBox.Enabled =
                    _openNlpRadioButton.Checked;
        }

        private void OutputConfidenceCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            _applyLogFunctionCheckBox.Enabled =
                _convertToPercentCheckBox.Enabled =
                    _outputConfidenceCheckBox.Checked;

            _logBaseLabel.Enabled =
            _logBaseNumericUpDown.Enabled =
            _logSteepnessLabel.Enabled =
            _logSteepnessNumericUpDown.Enabled =
            _logXMidLabel.Enabled =
            _logXMidNumericUpDown.Enabled =
                _applyLogFunctionCheckBox.Enabled
                && _applyLogFunctionCheckBox.Checked;
        }

        private void ApplyLogFunctionCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            _logBaseLabel.Enabled =
            _logBaseNumericUpDown.Enabled =
            _logSteepnessLabel.Enabled =
            _logSteepnessNumericUpDown.Enabled =
            _logXMidLabel.Enabled =
            _logXMidNumericUpDown.Enabled =
                _applyLogFunctionCheckBox.Enabled
                && _applyLogFunctionCheckBox.Checked;
        }

        #endregion Private Members
    }
}
