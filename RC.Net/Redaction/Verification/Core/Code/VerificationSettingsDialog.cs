using System;
using System.Windows.Forms;

namespace Extract.Redaction.Verification
{
    /// <summary>
    /// Represents a dialog that allows the user to select settings for 
    /// <see cref="VerificationTask"/>.
    /// </summary>
    public partial class VerificationSettingsDialog : Form
    {
        #region Fields

        /// <summary>
        /// The verification feedback settings.
        /// </summary>
        FeedbackSettings _feedback;

        /// <summary>
        /// The verification settings.
        /// </summary>
        VerificationSettings _settings;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new <see cref="VerificationSettingsDialog"/> class.
        /// </summary>
        // Don't fight with auto-generated code.
        //[SuppressMessage("Microsoft.Performance", "CA1805:DoNotInitializeUnnecessarily")]
        public VerificationSettingsDialog() : this(null)
        {
        }

        /// <summary>
	    /// Initializes a new instance of the <see cref="VerificationSettingsDialog"/> class.
	    /// </summary>
	    public VerificationSettingsDialog(VerificationSettings settings)
        {
            InitializeComponent();

            _settings = settings ?? new VerificationSettings();
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the verification settings.
        /// </summary>
        /// <value>The verification settings.</value>
        /// <returns>The verification settings.</returns>
        public VerificationSettings VerificationSettings
        {
            get
            {
                return _settings;
            }
            set
            {
                _settings = value ?? new VerificationSettings();
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Gets the <see cref="VerificationSettings"/> from the user interface.
        /// </summary>
        /// <returns>The <see cref="VerificationSettings"/> from the user interface.</returns>
        VerificationSettings GetVerificationSettings()
        {
            // Get the settings
            GeneralVerificationSettings general = GetGeneralSettings();
            FeedbackSettings feedback = GetFeedbackSettings();
            string dataFile = _dataFileControl.DataFile;

            return new VerificationSettings(general, feedback, dataFile);
        }

        /// <summary>
        /// Gets the <see cref="GeneralVerificationSettings"/> from the user interface.
        /// </summary>
        /// <returns>The <see cref="GeneralVerificationSettings"/> from the user interface.
        /// </returns>
        GeneralVerificationSettings GetGeneralSettings()
        {
            // Get the settings
            bool verifyAllPages = _verifyAllPagesCheckBox.Checked;
            bool requireTypes = _requireTypeCheckBox.Checked;
            bool requireExemptions = _requireExemptionsCheckBox.Checked;

            return new GeneralVerificationSettings(verifyAllPages, requireTypes, requireExemptions);
        }

        /// <summary>
        /// Gets the <see cref="FeedbackSettings"/> from the user interface.
        /// </summary>
        /// <returns>The <see cref="FeedbackSettings"/> from the user interface.
        /// </returns>
        FeedbackSettings GetFeedbackSettings()
        {
            // Get the settings
            bool collectFeedback = _collectFeedbackCheckBox.Checked;
            string dataFolder = _feedback.DataFolder;
            bool collectOriginalDocument = _feedback.CollectOriginalDocument;
            bool useOriginalFileNames = _feedback.UseOriginalFileNames;
            CollectionTypes collectionTypes = _feedback.CollectionTypes;

            return new FeedbackSettings(collectFeedback, dataFolder, collectOriginalDocument,
                useOriginalFileNames, collectionTypes);
        }

        /// <summary>
        /// Updates the enabled state of the controls.
        /// </summary>
        void UpdateControls()
        {
            _feedbackSettingsButton.Enabled = _collectFeedbackCheckBox.Checked;
        }

        /// <summary>
        /// Displays a warning message if the user specified settings are invalid.
        /// </summary>
        /// <returns><see langword="true"/> if the settings are invalid; <see langword="false"/> if 
        /// the settings are valid.</returns>
        bool WarnIfInvalid()
        {
            bool isEmpty = string.IsNullOrEmpty(_dataFileControl.DataFile);
            if (isEmpty)
            {
                MessageBox.Show("Please enter the ID Shield data file", "Invalid ID Shield data file",
                    MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button1, 0);
                _dataFileControl.Focus();
            }

            return isEmpty;
        }

        #endregion Methods

        #region Overrides

        /// <summary>
        /// Raises the <see cref="Form.Load"/> event.
        /// </summary>
        /// <param name="e">The event data associated with the <see cref="Form.Load"/> 
        /// event.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            try
            {
                // General settings
                _verifyAllPagesCheckBox.Checked = _settings.General.VerifyAllPages;
                _requireTypeCheckBox.Checked = _settings.General.RequireTypes;
                _requireExemptionsCheckBox.Checked = _settings.General.RequireExemptions;

                // Feedback settings
                _feedback = _settings.Feedback;
                _collectFeedbackCheckBox.Checked = _feedback.Collect;

                // ID Shield data file
                _dataFileControl.DataFile = _settings.InputFile;

                // Update the UI
                UpdateControls();
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI26328", ex);
            }
        }

        #endregion Overrides

        #region Event Handlers

        /// <summary>
        /// Handles the <see cref="CheckBox.CheckedChanged"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="CheckBox.CheckedChanged"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="CheckBox.CheckedChanged"/> event.</param>
        void HandleCollectFeedbackCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            try
            {
                UpdateControls();
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI26299", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="Control.Click"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="Control.Click"/> event.</param>
        void HandleFeedbackSettingsButtonClick(object sender, EventArgs e)
        {
            try
            {
                using (FeedbackSettingsDialog dialog = new FeedbackSettingsDialog(_feedback))
                {
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        _feedback = dialog.FeedbackSettings;
                    }
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI26305", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="Control.Click"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="Control.Click"/> event.</param>
        void HandleOkButtonClick(object sender, EventArgs e)
        {
            try
            {
                // Ensure the settings are valid
                if (WarnIfInvalid())
                {
                    return;
                }

                // Store settings
                _settings = GetVerificationSettings();
                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI26310", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        #endregion Event Handlers
    }
}