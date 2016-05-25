using Extract.Licensing;
using Extract.Utilities;
using System;
using System.Windows.Forms;
using UCLID_AFCORELib;
using UCLID_COMUTILSLib;

namespace Extract.AttributeFinder.Rules
{
    /// <summary>
    /// Allows configuration of a <see cref="LearningMachineOutputHandler"/> instance.
    /// </summary>
    [CLSCompliant(false)]
    public partial class LearningMachineOutputHandlerSettingsDialog : Form
    {
        #region Constants

        /// <summary>
        /// The object name.
        /// </summary>
        static readonly string _OBJECT_NAME =
            typeof(LearningMachineOutputHandlerSettingsDialog).ToString();

        #endregion Constants

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="LearningMachineOutputHandlerSettingsDialog"/> class.
        /// </summary>
        /// <param name="settings">The settings.</param>
        public LearningMachineOutputHandlerSettingsDialog(LearningMachineOutputHandler settings)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.RuleSetEditorUIObject,
                    "ELI39912", _OBJECT_NAME);

                Settings = settings;

                InitializeComponent();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39913");
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the <see cref="LearningMachineOutputHandler"/> to configure.
        /// </summary>
        /// <value>The <see cref="LearningMachineOutputHandler"/> to configure.</value>
        public LearningMachineOutputHandler Settings
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

                _savedMachinePathTagsButton.PathTags = new AttributeFinderPathTags();

                // Apply Settings values to the UI.
                if (Settings != null)
                {
                    _savedMachineTextBox.Text = Settings.SavedMachinePath;
                    _preserveInputAttributesCheckBox.Checked = Settings.PreserveInputAttributes;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI39914");
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

                Settings.SavedMachinePath = _savedMachineTextBox.Text;
                Settings.PreserveInputAttributes = _preserveInputAttributesCheckBox.Checked;

                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI39915");
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
            if (string.IsNullOrWhiteSpace(_savedMachineTextBox.Text))
            {
                _savedMachineTextBox.Focus();
                UtilityMethods.ShowMessageBox("Specify path to saved learning machine.",
                    "Specify learning machine", false);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Handles the OnValidating event
        /// </summary>
        /// <param name="sender">The sender</param>
        /// <param name="e">The args</param>
        private void HandleSavedMachineTextBox_Validating(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = string.IsNullOrWhiteSpace(_savedMachineTextBox.Text);
        }

        #endregion Private Members
    }
}