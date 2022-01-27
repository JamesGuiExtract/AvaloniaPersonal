using Extract.FileActionManager.Forms;
using Extract.Licensing;
using Extract.Redaction.Davidson;
using Extract.Utilities;
using Extract.Utilities.Forms;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using UCLID_FILEPROCESSINGLib;

namespace Extract.FileActionManager.FileProcessors
{
    /// <summary>
    /// A <see cref="Form"/> to view and modify settings for an
    /// <see cref="SplitMimeFileTask"/> instance.
    /// </summary>
    [CLSCompliant(false)]
    public partial class SplitMimeFileSettingsDialog : Form
    {
        #region Constants

        /// <summary>
        /// The object name used for license validation.
        /// </summary>
        static readonly string _OBJECT_NAME =
            typeof(SplitMimeFileSettingsDialog).ToString();

        const string _NO_ACTION = "<None>";

        #endregion Constants

        #region Fields

        readonly IFileProcessingDB _fileProcessingDB;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SplitMimeFileSettingsDialog"/> class.
        /// </summary>
        /// <param name="settings">The <see cref="SplitMimeFileTask"/> to configure</param>
        /// <param name="fileProcessingDB">The <see cref="IFileProcessingDB"/> this instance is
        /// associated with.</param>
        public SplitMimeFileSettingsDialog(SplitMimeFileTask settings,
            IFileProcessingDB fileProcessingDB)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.FileActionManagerObjects, "ELI53133",
                    _OBJECT_NAME);

                InitializeComponent();

                Settings = settings;
                _fileProcessingDB = fileProcessingDB;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53134");
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Property to return the configured settings
        /// </summary>
        public SplitMimeFileTask Settings
        {
            get;
            set;
        }

        #endregion Properties

        #region Overrides

        /// <summary>
        /// Raises the <see cref="Form.Load"/> event.
        /// </summary>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.
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
                    _sourceActionComboBox.Items.Add(actionName);
                    _outputActionComboBox.Items.Add(actionName);
                }

                _outputDirTextBox.Text = Settings.OutputDirectory;
                _sourceActionComboBox.Text = Settings.SourceAction;
                _outputActionComboBox.Text = Settings.OutputAction;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI53135");
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

                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI53136", ex);
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
                    this.SafeBeginInvoke("ELI53168", () => comboBox.Text = "");
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI53169");
            }
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

        #endregion Private Members
    }
}
