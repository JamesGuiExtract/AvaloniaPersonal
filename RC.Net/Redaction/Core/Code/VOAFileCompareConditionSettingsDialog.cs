using Extract.Utilities;
using System;
using System.Windows.Forms;

namespace Extract.Redaction
{
    /// <summary>
    /// A <see cref="Form"/> used to display and configure
    /// <see cref="VOAFileCompareConditionSettings"/>.
    /// </summary>
    public partial class VOAFileCompareConditionSettingsDialog : Form
    {
        #region Fields

        /// <summary>
        /// The settings to display.
        /// </summary>
        VOAFileCompareConditionSettings _settings;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="VOAFileCompareConditionSettingsDialog"/> class.
        /// </summary>
        /// <value></value>
        public VOAFileCompareConditionSettingsDialog(VOAFileCompareConditionSettings settings)
        {
            try
            {
                InitializeComponent();

                _settings = settings ?? new VOAFileCompareConditionSettings();

                _dataFile1PathTagsButton.PathTags = new FileActionManagerPathTags();
                _dataFile2PathTagsButton.PathTags = new FileActionManagerPathTags();
                _outputFilePathTagsButton.PathTags = new FileActionManagerPathTags();

                _outputDataCheckBox.CheckedChanged += ((sender, e) =>
                    {
                        bool enable = _outputDataCheckBox.Checked;
                        _outputFileTextBox.Enabled = enable;
                        _outputFilePathTagsButton.Enabled = enable;
                        _outputConditionCheckBox.Enabled = enable;
                    });

                _overlapThresholdUpDown.UserTextCorrected += ((sender, e) =>
                    {
                        MessageBox.Show("The maximum number of characters to add to each " +
                            "redaction must be between 1 and 100", "Invalid number of characters.",
                            MessageBoxButtons.OK, MessageBoxIcon.None,
                            MessageBoxDefaultButton.Button1, 0);
                        _overlapThresholdUpDown.Focus();
                    });

                _okButton.Click += HandleOkButtonClick;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI31762");
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets the specified <see cref="VOAFileCompareConditionSettings"/>.
        /// </summary>
        public VOAFileCompareConditionSettings VOAFileCompareConditionSettings
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
        /// <param name="e">The event data associated with the <see cref="Form.Load"/> event.
        /// </param>
        protected override void OnLoad(EventArgs e)
        {
            try
            {
                base.OnLoad(e);

                _conditionMetComboBox.SelectedItem =
                    _settings.ConditionMetIfMatching ? "met" : "not met";
                _dataFile1TextBox.Text = _settings.DataFile1;
                _dataFile2TextBox.Text = _settings.DataFile2;
                _useMutualOverlapComboBox.SelectedIndex = _settings.UseMutualOverlap ? 0 : 1;
                _overlapThresholdUpDown.Value = _settings.OverlapThreshold;
                _outputDataCheckBox.Checked = _settings.CreateOutput;
                _outputFileTextBox.Text = _settings.OutputFile;
                _outputFileTextBox.Enabled = _outputDataCheckBox.Checked;
                _outputFilePathTagsButton.Enabled = _outputDataCheckBox.Checked;
                _outputConditionCheckBox.Checked = _settings.CreateOutputOnlyOnCondition;
                _outputConditionCheckBox.Enabled = _outputDataCheckBox.Checked;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI31763");
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
                _settings = GetVOAFileCompareConditionSettings();
                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI31764", ex);
            }
        }

        #endregion Event Handlers

        #region Private Members

        /// <summary>
        /// Gets the <see cref="VOAFileCompareConditionSettings"/> specified via the UI.
        /// </summary>
        /// <returns>The <see cref="VOAFileCompareConditionSettings"/> specified via the UI.
        /// </returns>
        VOAFileCompareConditionSettings GetVOAFileCompareConditionSettings()
        {
            try
            {
                bool conditionMetIfMatching =
                    _conditionMetComboBox.Text.Equals("met", StringComparison.OrdinalIgnoreCase);
                string dataFile1 = _dataFile1TextBox.Text;
                string dataFile2 = _dataFile2TextBox.Text;
                bool useMutualOverlap = (_useMutualOverlapComboBox.SelectedIndex == 0) ? true : false;
                int overlapThreshold = (int)_overlapThresholdUpDown.Value;
                bool createOutput = _outputDataCheckBox.Checked;
                string outputFile = _outputFileTextBox.Text;
                bool createOutputOnlyOnCondition = _outputConditionCheckBox.Checked;

                return new VOAFileCompareConditionSettings(conditionMetIfMatching, dataFile1,
                    dataFile2, useMutualOverlap, overlapThreshold, createOutput, outputFile,
                    createOutputOnlyOnCondition);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI31765");
            }
        }

        /// <summary>
        /// Displays a warning message if the user specified settings are invalid.
        /// </summary>
        /// <returns><see langword="true"/> if the settings are invalid; <see langword="false"/> if 
        /// the settings are valid.</returns>
        bool WarnIfInvalid()
        {
            if (string.IsNullOrWhiteSpace(_dataFile1TextBox.Text))
            {
                _dataFile1TextBox.Focus();
                MessageBox.Show("Please specify both ID Shield data (VOA) files to compare.",
                    "Missing data filename", MessageBoxButtons.OK, MessageBoxIcon.None,
                    MessageBoxDefaultButton.Button1, 0);
                return true;
            }

            if (string.IsNullOrWhiteSpace(_dataFile2TextBox.Text))
            {
                _dataFile1TextBox.Focus();
                MessageBox.Show("Please specify both ID Shield data (VOA) files to compare.",
                    "Missing data filename", MessageBoxButtons.OK, MessageBoxIcon.None,
                    MessageBoxDefaultButton.Button1, 0);
                return true;
            }

            if (_outputDataCheckBox.Checked && string.IsNullOrWhiteSpace(_outputFileTextBox.Text))
            {
                _outputFileTextBox.Focus();
                MessageBox.Show("Please specify the output ID Shield data (VOA) filename.",
                    "Missing output filename", MessageBoxButtons.OK, MessageBoxIcon.None,
                    MessageBoxDefaultButton.Button1, 0);
                return true;
            }

            return false;
        }

        #endregion Private Members
    }
}
