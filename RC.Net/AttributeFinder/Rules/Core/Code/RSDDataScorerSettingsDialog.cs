using Extract.Licensing;
using Extract.Utilities;
using System;
using System.Windows.Forms;
using UCLID_AFUTILSLib;

namespace Extract.AttributeFinder.Rules
{
    /// <summary>
    /// Allows configuration of a <see cref="RSDDataScorer"/> instance.
    /// </summary>
    [CLSCompliant(false)]    
    public partial class RSDDataScorerSettingsDialog : Form
    {
        #region Constants

        /// <summary>
        /// The object name.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(RSDDataScorerSettingsDialog).ToString();

        /// <summary>
        /// The default <see cref="RSDDataScorer.ScoreExpression"/> to use if one has not yet been
        /// configured.
        /// </summary>
        const string _DEFAULT_EXPRESSION = "#ScorePart.sum()";

        #endregion Constants

        #region Fields

        /// <summary>
        /// Used to validate <see cref="RSDDataScorer.RSDFileName"/> as an explicit path.
        /// </summary>
        AFUtility _afUtility = new AFUtility();

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="RSDDataScorerSettingsDialog"/>
        /// class.
        /// </summary>
        /// <param name="settings">The <see cref="RSDDataScorer"/> instance to configure.</param>
        public RSDDataScorerSettingsDialog(RSDDataScorer settings)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.RuleWritingCoreObjects, "ELI33836",
                    _OBJECT_NAME);

                Settings = settings;

                InitializeComponent();

                _rsdFileNamePathTagsButton.PathTags = new AttributeFinderPathTags();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI33837");
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the <see cref="RSDDataScorer"/> to configure.
        /// </summary>
        /// <value>The <see cref="RSDDataScorer"/> to configure.</value>
        public RSDDataScorer Settings
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
                    _rsdFileNameTextBox.Text = Settings.RSDFileName;

                    if (string.IsNullOrWhiteSpace(Settings.ScoreExpression))
                    {
                        _scoreExpressionTextBox.Text = _DEFAULT_EXPRESSION;
                    }
                    else
                    {
                        _scoreExpressionTextBox.Text = Settings.ScoreExpression;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI33838");
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

                Settings.RSDFileName = _rsdFileNameTextBox.Text;
                Settings.ScoreExpression = _scoreExpressionTextBox.Text;

                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI33840");
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
            if (string.IsNullOrWhiteSpace(_rsdFileNameTextBox.Text))
            {
                _rsdFileNameTextBox.Focus();
                UtilityMethods.ShowMessageBox("Please specify an explicity path RSD file to use.",
                    "Specify RSD file", false);
                return true;
            }

            try
            {
                _afUtility.ValidateAsExplicitPath("ELI33851", _rsdFileNameTextBox.Text);
            }
            catch
            {
                _rsdFileNameTextBox.Focus();
                UtilityMethods.ShowMessageBox(
                    "The RSD file path must be absolute or based on a path tag.", 
                    "Invalid RSD file path", false);
                return true;
            }

            if (string.IsNullOrWhiteSpace(_scoreExpressionTextBox.Text))
            {
                _scoreExpressionTextBox.Focus();
                UtilityMethods.ShowMessageBox("Please specify the expression used to use.",
                    "Specify expression.", false);
                return true;
            }

            return false;
        }

        #endregion Private Members
    }
}
