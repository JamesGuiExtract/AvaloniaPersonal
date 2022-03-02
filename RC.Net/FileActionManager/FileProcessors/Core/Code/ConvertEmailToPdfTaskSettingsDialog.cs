using Extract.Licensing;
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
    /// A <see cref="Form"/> to view and modify settings for an
    /// <see cref="ConvertEmailToPdfTask"/> instance.
    /// </summary>
    [CLSCompliant(false)]
    public partial class ConvertEmailToPdfTaskSettingsDialog : Form
    {
        #region Constants

        /// <summary>
        /// The object name used for license validation.
        /// </summary>
        static readonly string _OBJECT_NAME =
            typeof(ConvertEmailToPdfTaskSettingsDialog).ToString();

        const string _NO_ACTION = "<None>";

        #endregion Constants

        #region Fields

        readonly IFileProcessingDB _fileProcessingDB;
        readonly List<ComboBox> _actionComboBoxes;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ConvertEmailToPdfTaskSettingsDialog"/> class.
        /// </summary>
        /// <param name="settings">The <see cref="ConvertEmailToPdfTask"/> to configure</param>
        /// <param name="fileProcessingDB">The <see cref="IFileProcessingDB"/> this instance is
        /// associated with.</param>
        public ConvertEmailToPdfTaskSettingsDialog(ConvertEmailToPdfTask settings,
            IFileProcessingDB fileProcessingDB)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.FileActionManagerObjects, "ELI53133",
                    _OBJECT_NAME);

                InitializeComponent();

                _actionComboBoxes = new List<ComboBox>
                {
                    _splitModeQueueSourceFileActionComboBox,
                    _splitModeQueueNewFilesActionComboBox,
                    _comboModeQueueSourceFileActionComboBox,
                    _comboModeQueueNewFileActionComboBox
                };


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
        public ConvertEmailToPdfTask Settings
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

                UpdateActionComboBoxes();

                // default all but one group of settings to hidden
                _splitModeGroupBox.Visible = false;

                _comboModeRadioButton.Checked = Settings.ProcessingMode == ConvertEmailProcessingMode.Combo;
                _splitModeRadioButton.Checked = Settings.ProcessingMode == ConvertEmailProcessingMode.Split;

                _splitModeOutputDirTextBox.Text = Settings.OutputDirectory;
                _splitModeQueueSourceFileActionComboBox.Text = Settings.SourceAction;
                _splitModeQueueNewFilesActionComboBox.Text = Settings.OutputAction;

                _comboModeOutputFileNameTextBox.Text = Settings.OutputFilePath;
                _comboModeModifySourceDocNameCheckBox.Checked = Settings.ModifySourceDocName;
                _comboModeQueueSourceFileActionComboBox.Text = Settings.SourceAction;
                _comboModeQueueNewFileActionComboBox.Text = Settings.OutputAction;
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

                if (_comboModeRadioButton.Checked)
                {
                    if (WarnIfInvalid_ComboMode())
                    {
                        return;
                    }

                    Settings.ProcessingMode = ConvertEmailProcessingMode.Combo;
                    Settings.OutputFilePath = _comboModeOutputFileNameTextBox.Text;
                    Settings.SourceAction = _comboModeQueueSourceFileActionComboBox.Text;
                    Settings.OutputAction = _comboModeQueueNewFileActionComboBox.Text;
                    Settings.ModifySourceDocName = _comboModeModifySourceDocNameCheckBox.Checked;
                }
                else if (_splitModeRadioButton.Checked)
                {
                    if (WarnIfInvalid_SplitMode())
                    {
                        return;
                    }

                    Settings.ProcessingMode = ConvertEmailProcessingMode.Split;
                    Settings.OutputDirectory = _splitModeOutputDirTextBox.Text;
                    Settings.SourceAction = _splitModeQueueSourceFileActionComboBox.Text;
                    Settings.OutputAction = _splitModeQueueNewFilesActionComboBox.Text;
                }

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

        /// <summary>
        /// Change which group of settings is visible
        /// </summary>
        void HandleProcessingModeRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            SuspendLayout();
            try
            {
                if (sender == _comboModeRadioButton && _comboModeRadioButton.Checked)
                {
                    _comboModeGroupBox.Visible = true;
                    _splitModeGroupBox.Visible = false;
                }
                else if (sender == _splitModeRadioButton && _splitModeRadioButton.Checked)
                {
                    _comboModeGroupBox.Visible = false;
                    _splitModeGroupBox.Visible = true;
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53234");
            }
            finally
            {
                ResumeLayout(true);
            }
        }


        /// <summary>
        /// Enable/disable the new file action based on the modify-source-doc checkbox state
        /// </summary>
        void HandleComboModeModifySourceDocNameCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                _comboModeQueueNewFileActionComboBox.Enabled = !_comboModeModifySourceDocNameCheckBox.Checked;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53235");
            }
        }

        #endregion Event Handlers

        #region Private Members

        /// <summary>
        /// Displays a warning message if the user specified settings are invalid.
        /// </summary>
        /// <returns><c>true</c> if the settings are invalid; <c>false</c> if
        /// the settings are valid.</returns>
        bool WarnIfInvalid_ComboMode()
        {
            // Make sure the list of valid actions is up to date
            UpdateActionComboBoxes();

            if (string.IsNullOrWhiteSpace(_comboModeOutputFileNameTextBox.Text))
            {
                UtilityMethods.ShowMessageBox("The output file name must be specified.",
                    "Invalid configuration", true);
                _comboModeOutputFileNameTextBox.Focus();

                return true;
            }

            if (!string.IsNullOrWhiteSpace(_comboModeQueueNewFileActionComboBox.Text) &&
                !_comboModeQueueNewFileActionComboBox.Items.Contains(_comboModeQueueNewFileActionComboBox.Text))
            {
                UtilityMethods.ShowMessageBox("The action to queue the new file to is not valid.",
                    "Invalid configuration", true);
                _comboModeQueueNewFileActionComboBox.Focus();

                return true;
            }

            if (_comboModeQueueSourceFileActionComboBox.Enabled &&
                !string.IsNullOrWhiteSpace(_comboModeQueueSourceFileActionComboBox.Text) &&
                !_comboModeQueueSourceFileActionComboBox.Items.Contains(_comboModeQueueSourceFileActionComboBox.Text))
            {
                UtilityMethods.ShowMessageBox("The action to queue the source file to is not valid.",
                    "Invalid configuration", true);
                _comboModeQueueSourceFileActionComboBox.Focus();

                return true;
            }

            return false;
        }

        /// <summary>
        /// Displays a warning message if the user specified settings are invalid.
        /// </summary>
        /// <returns><c>true</c> if the settings are invalid; <c>false</c> if
        /// the settings are valid.</returns>
        bool WarnIfInvalid_SplitMode()
        {
            // Make sure the list of valid actions is up to date
            UpdateActionComboBoxes();

            if (string.IsNullOrWhiteSpace(_splitModeOutputDirTextBox.Text))
            {
                UtilityMethods.ShowMessageBox("The output directory must be specified.",
                    "Invalid configuration", true);
                _splitModeOutputDirTextBox.Focus();

                return true;
            }

            if (!string.IsNullOrWhiteSpace(_splitModeQueueSourceFileActionComboBox.Text) &&
                !_splitModeQueueSourceFileActionComboBox.Items.Contains(_splitModeQueueSourceFileActionComboBox.Text))
            {
                UtilityMethods.ShowMessageBox("The action to queue the source file to is not valid.",
                    "Invalid configuration", true);
                _splitModeQueueSourceFileActionComboBox.Focus();

                return true;
            }

            if (_splitModeQueueNewFilesActionComboBox.Enabled &&
                !string.IsNullOrWhiteSpace(_splitModeQueueNewFilesActionComboBox.Text) &&
                !_splitModeQueueNewFilesActionComboBox.Items.Contains(_splitModeQueueNewFilesActionComboBox.Text))
            {
                UtilityMethods.ShowMessageBox("The action to queue new files to is not valid.",
                    "Invalid configuration", true);
                _splitModeQueueNewFilesActionComboBox.Focus();

                return true;
            }

            return false;
        }

        // Refresh the actions from the database
        void UpdateActionComboBoxes()
        {
            var actionNames = Enumerable.Repeat(_NO_ACTION, 1)
                .Concat(_fileProcessingDB
                    .GetActions()
                    .GetKeys()
                    .ToIEnumerable<string>())
                .ToArray();

            foreach (var box in _actionComboBoxes)
            {
                box.Items.Clear();
                box.Items.AddRange(actionNames);
            }
        }

        #endregion Private Members
    }
}
