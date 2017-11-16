using Extract.Licensing;
using Extract.Utilities;
using System;
using System.Windows.Forms;
using UCLID_AFCORELib;
using UCLID_COMUTILSLib;

namespace Extract.AttributeFinder.Rules
{
    /// <summary>
    /// Allows configuration of a <see cref="TemplateFinder"/> instance.
    /// </summary>
    [CLSCompliant(false)]
    public partial class TemplateFinderSettingsDialog : Form
    {
        #region Constants

        /// <summary>
        /// The object name.
        /// </summary>
        static readonly string _OBJECT_NAME =
            typeof(TemplateFinderSettingsDialog).ToString();

        #endregion Constants

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="TemplateFinderSettingsDialog"/> class.
        /// </summary>
        /// <param name="settings">The settings.</param>
        public TemplateFinderSettingsDialog(TemplateFinder settings)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.RuleSetEditorUIObject,
                    "ELI45221", _OBJECT_NAME);

                Settings = settings;

                InitializeComponent();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45222");
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the <see cref="TemplateFinder"/> to configure.
        /// </summary>
        /// <value>The <see cref="TemplateFinder"/> to configure.</value>
        public TemplateFinder Settings
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
                    _templatesDirTextBox.Text = Settings.TemplatesDir ?? "";
                    _redactionPredictorOptionsTextBox.Text = Settings.RedactionPredictorOptions ?? "";
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45223");
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

                Settings.TemplatesDir = _templatesDirTextBox.Text;
                Settings.RedactionPredictorOptions = _redactionPredictorOptionsTextBox.Text;

                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45224");
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
            if (string.IsNullOrWhiteSpace(_templatesDirTextBox.Text))
            {
                _templatesDirTextBox.Focus();
                UtilityMethods.ShowMessageBox("Please specify a directory of templates (*.tpt files).",
                    "Specify templates dir", false);
                return true;
            }

            return false;
        }

        #endregion Private Members
    }
}
