using Extract.Licensing;
using Extract.Utilities;
using System;
using System.Windows.Forms;

namespace Extract.AttributeFinder.Rules
{
    /// <summary>
    /// Allows configuration of a <see cref="FSharpPreprocessor"/> instance.
    /// </summary>
    public partial class FSharpPreprocessorSettingsDialog : Form
    {
        #region Constants

        /// <summary>
        /// The object name.
        /// </summary>
        static readonly string _OBJECT_NAME =
            typeof(FSharpPreprocessorSettingsDialog).ToString();

        #endregion Constants

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FSharpPreprocessorSettingsDialog"/> class.
        /// </summary>
        /// <param name="settings">The settings.</param>
        [CLSCompliant(false)]
        public FSharpPreprocessorSettingsDialog(FSharpPreprocessor settings)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.RuleSetEditorUIObject,
                    "ELI46950", _OBJECT_NAME);

                Settings = settings;

                InitializeComponent();

                _pathToScriptFilePathTagsButton.PathTags = new AttributeFinderPathTags();
                string collectibleTooltip = "When checked, memory usage will be lower but some features will not work."
                    + "\n If you get an exception with 'collectible' in the message, try unchecking this box.";
                toolTip1.SetToolTip(_collectibleCheckBox, collectibleTooltip);
                toolTip1.SetToolTip(_collectibleInfoTip, collectibleTooltip);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI46951");
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the <see cref="FSharpPreprocessor"/> to configure.
        /// </summary>
        /// <value>The <see cref="FSharpPreprocessor"/> to configure.</value>
        [CLSCompliant(false)]
        public FSharpPreprocessor Settings
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
                    _pathToScriptFileTextBox.Text = Settings.ScriptPath;
                    _functionNameTextBox.Text = Settings.FunctionName;
                    _collectibleCheckBox.Checked = Settings.Collectible;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI46952");
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

                Settings.ScriptPath = _pathToScriptFileTextBox.Text;
                Settings.FunctionName = _functionNameTextBox.Text;
                Settings.Collectible = _collectibleCheckBox.Checked;

                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI46953");
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
            if (string.IsNullOrWhiteSpace(_pathToScriptFileTextBox.Text))
            {
                _pathToScriptFileTextBox.Focus();
                UtilityMethods.ShowMessageBox("Please specify a script file to use.",
                    "Specify script file", false);
                return true;
            }

            if (string.IsNullOrWhiteSpace(_functionNameTextBox.Text))
            {
                _functionNameTextBox.Focus();
                UtilityMethods.ShowMessageBox("Please specify a function name to call.",
                    "Specify function name", false);
                return true;
            }


            return false;
        }

        #endregion Private Members
    }
}
