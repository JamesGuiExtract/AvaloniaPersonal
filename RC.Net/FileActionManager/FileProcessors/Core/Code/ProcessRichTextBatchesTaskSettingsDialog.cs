using Extract.FileActionManager.Forms;
using Extract.Licensing;
using Extract.Redaction.Davidson;
using Extract.Utilities;
using Extract.Utilities.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using UCLID_FILEPROCESSINGLib;

namespace Extract.FileActionManager.FileProcessors
{
    /// <summary>
    /// A <see cref="Form"/> to view and modify settings for a
    /// <see cref="ProcessRichTextBatchesTask"/> instance.
    /// </summary>
    [CLSCompliant(false)]
    public partial class ProcessRichTextBatchesTaskSettingsDialog : Form
    {
        #region Constants

        /// <summary>
        /// The object name used for license validation.
        /// </summary>
        static readonly string _OBJECT_NAME =
            typeof(ProcessRichTextBatchesTaskSettingsDialog).ToString();

        const string _NO_ACTION = "<None>";

        #endregion Constants

        #region Fields

        private IFileProcessingDB _fileProcessingDB;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessRichTextBatchesTaskSettingsDialog"/> class.
        /// </summary>
        /// <param name="settings">The <see cref="ProcessRichTextBatchesTask"/> to configure</param>
        /// <param name="fileProcessingDB">The <see cref="IFileProcessingDB"/> this instance is
        /// associated with.</param>
        public ProcessRichTextBatchesTaskSettingsDialog(ProcessRichTextBatchesTask settings,
            IFileProcessingDB fileProcessingDB)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.FileActionManagerObjects, "ELI48376",
                    _OBJECT_NAME);

                InitializeComponent();

                var outputPathTags = new FileActionManagerPathTags();
                outputPathTags.AddTag(RichTextFormatBatchProcessor.SubBatchNumber, "");

                _outputDirPathTags.PathTags = outputPathTags;

                Settings = settings;
                _fileProcessingDB = fileProcessingDB;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI48377");
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Property to return the configured settings
        /// </summary>
        public ProcessRichTextBatchesTask Settings
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

                var actionNames = new List<string> { _NO_ACTION };
                actionNames.AddRange(
                    _fileProcessingDB
                    .GetActions()
                    .GetKeys()
                    .ToIEnumerable<string>());

                foreach (var actionName in actionNames)
                {
                    _redactionActionComboBox.Items.Add(actionName);
                    _sourceActionComboBox.Items.Add(actionName);
                    _outputActionComboBox.Items.Add(actionName);
                }

                if (Settings.DivideBatchIntoRichTextFiles)
                {
                    _divideBatchIntoFilesRadioButton.Checked = true;
                }
                else
                {
                    _updateBatchWithRedactedFilesRadioButton.Checked = true;
                }
                _outputDirTextBox.Text = Settings.OutputDirectory;
                _redactionActionComboBox.Text = Settings.RedactedAction;
                _redactedOutputFileTextBox.Text = Settings.RedactedFile;
                _updatedBatchFileTextBox.Text = Settings.UpdatedBatchFile;
                _sourceActionComboBox.Text = Settings.SourceAction;
                _outputActionComboBox.Text = Settings.OutputAction;

                UpdateEnabledStates();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI48378");
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

                Settings.OutputDirectory = _outputDirTextBox.Text;
                Settings.SourceAction = _sourceActionComboBox.Text;
                Settings.OutputAction = _outputActionComboBox.Text;
                Settings.DivideBatchIntoRichTextFiles = _divideBatchIntoFilesRadioButton.Checked;
                Settings.RedactedAction = _redactionActionComboBox.Text;
                Settings.UpdatedBatchFile = _updatedBatchFileTextBox.Text;
                Settings.RedactedFile = _redactedOutputFileTextBox.Text;

                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI48379", ex);
            }
        }
       
        /// <summary>
        /// Handles the SelectedIndexChanged event in order to clear the combo if &lt;NONE&gt; is selected.
        /// </summary>
        void HandleActionComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                ComboBox comboBox = (ComboBox)sender;
                if (comboBox.Text == _NO_ACTION)
                {
                    // If "<None>" was selected, interpret that to mean the text should be cleared.
                    this.SafeBeginInvoke("ELI48393", () => comboBox.Text = "");
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI48394");
            }
        }

        private void _divideBatchIntoFilesRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            UpdateEnabledStates();
        }

        #endregion Event Handlers

        #region Private Members

        /// <summary>
        /// Displays a warning message if the user specified settings are invalid.
        /// </summary>
        /// <returns><c>true</c> if the settings are invalid; <c>false</c> if
        /// the settings are valid.</returns>
        bool WarnIfInvalid()
        {
            if (string.IsNullOrWhiteSpace(_outputDirTextBox.Text))
            {
                UtilityMethods.ShowMessageBox(
                    "The output path must be specified.",
                    "Invalid configuration", true);
                _outputDirTextBox.Focus();

                return true;
            }

            if (_redactedOutputFileTextBox.Enabled &&
                string.IsNullOrWhiteSpace(_redactedOutputFileTextBox.Text))
            {
                UtilityMethods.ShowMessageBox(
                    "The redacted file path must be specified.",
                    "Invalid configuration", true);
                _redactedOutputFileTextBox.Focus();

                return true;
            }

            if (_sourceActionComboBox.Enabled &&
                !string.IsNullOrWhiteSpace(_sourceActionComboBox.Text) &&
                !_sourceActionComboBox.Items.Contains(_sourceActionComboBox.Text))
            {
                UtilityMethods.ShowMessageBox(
                    "The action to queue source, batch, files to is not valid.",
                    "Invalid configuration", true);
                _sourceActionComboBox.Focus();

                return true;
            }

            if (_outputActionComboBox.Enabled &&
                !string.IsNullOrWhiteSpace(_outputActionComboBox.Text) &&
                !_outputActionComboBox.Items.Contains(_outputActionComboBox.Text))
            {
                UtilityMethods.ShowMessageBox(
                    "The action to queue output, RTF, files to is not valid.",
                    "Invalid configuration", true);
                _outputActionComboBox.Focus();

                return true;
            }

            return false;
        }

        private void UpdateEnabledStates()
        {
            if (_updateBatchWithRedactedFilesRadioButton.Checked)
            {
                _redactionGroupBox.Enabled = true;
                _queueGroupBox.Enabled = false;
            }
            else
            {
                _redactionGroupBox.Enabled = false;
                _queueGroupBox.Enabled = true;
            }
        }

        #endregion Private Members
    }
}
