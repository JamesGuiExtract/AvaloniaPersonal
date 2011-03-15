using Extract.Utilities;
using System;
using System.Windows.Forms;

namespace Extract.Redaction
{
    /// <summary>
    /// A <see cref="Form"/> used to display and configure <see cref="VOAFileMergeTaskSettings"/>.
    /// </summary>
    public partial class VOAFileMergeTaskSettingsDialog : Form
    {
        #region Fields

        /// <summary>
        /// The settings to display.
        /// </summary>
        VOAFileMergeTaskSettings _settings;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="VOAFileMergeTaskSettingsDialog"/> class.
        /// </summary>
        /// <value></value>
        public VOAFileMergeTaskSettingsDialog(VOAFileMergeTaskSettings settings)
        {
            try
            {
                InitializeComponent();

                _settings = settings ?? new VOAFileMergeTaskSettings();

                _dataFile1PathTagsButton.PathTags = new FileActionManagerPathTags();
                _dataFile2PathTagsButton.PathTags = new FileActionManagerPathTags();
                _outputFilePathTagsButton.PathTags = new FileActionManagerPathTags();

                _overlapThresholdUpDown.UserTextCorrected += ((sender, e) =>
                    {
                        MessageBox.Show("The maximum number of charaters to add to each " +
                            "redaction must be between 1 and 100", "Invalid number of characters.",
                            MessageBoxButtons.OK, MessageBoxIcon.None,
                            MessageBoxDefaultButton.Button1, 0);
                        _overlapThresholdUpDown.Focus();
                    });

                _okButton.Click += HandleOkButtonClick;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI32055");
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets the specified <see cref="VOAFileMergeTaskSettings"/>.
        /// </summary>
        public VOAFileMergeTaskSettings VOAFileMergeTaskSettings
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

                _dataFile1TextBox.Text = _settings.DataFile1;
                _dataFile2TextBox.Text = _settings.DataFile2;
                _overlapThresholdUpDown.Value = _settings.OverlapThreshold;
                _outputFileTextBox.Text = _settings.OutputFile;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI32056");
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
                _settings = GetVOAFileMergeTaskSettings();
                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI32057", ex);
            }
        }

        #endregion Event Handlers

        #region Private Members

        /// <summary>
        /// Gets the <see cref="VOAFileMergeTaskSettings"/> specified via the UI.
        /// </summary>
        /// <returns>The <see cref="VOAFileMergeTaskSettings"/> specified via the UI.
        /// </returns>
        VOAFileMergeTaskSettings GetVOAFileMergeTaskSettings()
        {
            try
            {
                string dataFile1 = _dataFile1TextBox.Text;
                string dataFile2 = _dataFile2TextBox.Text;
                int overlapThreshold = (int)_overlapThresholdUpDown.Value;
                string outputFile = _outputFileTextBox.Text;

                return new VOAFileMergeTaskSettings(dataFile1, dataFile2, overlapThreshold, outputFile);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI32058");
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
                MessageBox.Show("Please specify both ID Shield data (VOA) files to merge.",
                    "Missing data filename", MessageBoxButtons.OK, MessageBoxIcon.None,
                    MessageBoxDefaultButton.Button1, 0);
                return true;
            }

            if (string.IsNullOrWhiteSpace(_dataFile2TextBox.Text))
            {
                _dataFile1TextBox.Focus();
                MessageBox.Show("Please specify both ID Shield data (VOA) files to merge.",
                    "Missing data filename", MessageBoxButtons.OK, MessageBoxIcon.None,
                    MessageBoxDefaultButton.Button1, 0);
                return true;
            }

            if (string.IsNullOrWhiteSpace(_outputFileTextBox.Text))
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
