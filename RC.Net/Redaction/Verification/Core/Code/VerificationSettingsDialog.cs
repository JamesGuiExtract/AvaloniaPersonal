using Extract.Utilities.Forms;
using System;
using System.Windows.Forms;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;

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
        /// The slideshow settings.
        /// </summary>
        SlideshowSettings _slideshowSettings;

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

			FileProcessingDB database = new FileProcessingDB();
			database.ConnectLastUsedDBThisProcess();

            StrToStrMap actionNameToId = database.GetActions();
            VariantVector actionNames = actionNameToId.GetKeys();
            int size = actionNames.Size;
            for (int i = 0; i < size; i++)
            {
                _actionNameComboBox.Items.Add(actionNames[i]);
            }
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
            bool useBackdropImage = _backdropImageCheckBox.Checked;
            string backdropImage = _backdropImageTextBox.Text;
            SetFileActionStatusSettings action = GetActionStatusSettings();
            bool enableInputTracking = _enableInputEventTrackingCheckBox.Checked;
            bool launchInFullScreenMode = _launchFullScreenCheckBox.Checked;
            SlideshowSettings slideshowSettings = GetSlideshowSettings();

            return new VerificationSettings(general, feedback, dataFile, useBackdropImage,
                backdropImage, action, enableInputTracking, launchInFullScreenMode,
                slideshowSettings);
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
        /// Gets the <see cref="SetFileActionStatusSettings"/> from the user interface.
        /// </summary>
        /// <returns>The <see cref="SetFileActionStatusSettings"/> from the user interface.
        /// </returns>
        SetFileActionStatusSettings GetActionStatusSettings()
        {
            // Get the settings
            bool enabled = _fileActionCheckBox.Checked;
            string actionName = _actionNameComboBox.Text;
            EActionStatus actionStatus = GetActionStatusFromString(_actionStatusComboBox.Text);

            return new SetFileActionStatusSettings(enabled, actionName, actionStatus);
        }

        /// <summary>
        /// Gets the <see cref="SlideshowSettings"/> from the user interface.
        /// </summary>
        /// <returns>The <see cref="SlideshowSettings"/> from the user interface.
        /// </returns>
        SlideshowSettings GetSlideshowSettings()
        {
            bool enabled = _enableSlideshowCheckBox.Checked;
            bool applyAutoAdvanceTag = _slideshowSettings.ApplyAutoAdvanceTag;
            string autoAdvanceTag = _slideshowSettings.AutoAdvanceTag;
            bool applyAutoAdvanceActionStatus = _slideshowSettings.ApplyAutoAdvanceActionStatus;
            string autoAdvanceActionName = _slideshowSettings.AutoAdvanceActionName;
            EActionStatus autoAdvanceActionStatus = _slideshowSettings.AutoAdvanceActionStatus;
            bool checkDocTypeConditon = _slideshowSettings.CheckDocumentCondition;
            ObjectWithDescription documentCondition = _slideshowSettings.DocumentCondition;

            return new SlideshowSettings(enabled, applyAutoAdvanceTag, autoAdvanceTag,
                applyAutoAdvanceActionStatus, autoAdvanceActionName, autoAdvanceActionStatus,
                checkDocTypeConditon, documentCondition);
        }

        /// <summary>
        /// Gets the action status with the specified name.
        /// </summary>
        /// <param name="name">The name of the action status to return.</param>
        /// <returns>The action status with the specified <paramref name="name"/>.</returns>
        static EActionStatus GetActionStatusFromString(string name)
        {
            string upperCaseName = name.ToUpperInvariant();
            switch (upperCaseName)
            {
                case "PENDING":
                    return EActionStatus.kActionPending;
                case "UNATTEMPTED":
                    return EActionStatus.kActionUnattempted;
                case "COMPLETED":
                    return EActionStatus.kActionCompleted;
                case "FAILED":
                    return EActionStatus.kActionFailed;
                case "SKIPPED":
                    return EActionStatus.kActionSkipped;
            }

            ExtractException ee = new ExtractException("ELI29171",
                "Unexpected action status.");
            ee.AddDebugData("Action status", name, false);
            throw ee;
        }

        /// <summary>
        /// Gets the name of the specified action status.
        /// </summary>
        /// <param name="status">Action status whose name should be retrieved.</param>
        /// <returns>The name of the specified action <paramref name="status"/>.</returns>
        static string GetStringFromActionStatus(EActionStatus status)
        {
            switch (status)
            {
                case EActionStatus.kActionCompleted:
                    return "Completed";
                case EActionStatus.kActionFailed:
                    return "Failed";
                case EActionStatus.kActionPending:
                    return "Pending";
                case EActionStatus.kActionSkipped:
                    return "Skipped";
                case EActionStatus.kActionUnattempted:
                    return "Unattempted";
            }

            ExtractException ee = new ExtractException("ELI29172",
                "Unexpected action status");
            ee.AddDebugData("Action status", status, false);
            throw ee;
        }

        /// <summary>
        /// Gets the name of the action to select when the form is first displayed.
        /// </summary>
        /// <returns>The name of the action to select when the form is first displayed.</returns>
        string GetInitialActionName()
        {
            string actionName = _settings.ActionStatusSettings.ActionName;
            if (string.IsNullOrEmpty(actionName) && _actionNameComboBox.Items.Count > 0)
            {
                // If no action is selected, select the first action in the combo box
                object item = _actionNameComboBox.Items[0];
                actionName = _actionNameComboBox.GetItemText(item);
            }

            return actionName;
        }

        /// <summary>
        /// Updates the enabled state of the controls.
        /// </summary>
        void UpdateControls()
        {
            // Enable or disable feedback settings
            _feedbackSettingsButton.Enabled = _collectFeedbackCheckBox.Checked;
            _slideshowSettingsButton.Enabled = _enableSlideshowCheckBox.Checked;

            // Enable or disable settings
            bool enabled = _backdropImageCheckBox.Checked;
            _backdropImageTextBox.Enabled = enabled;
            _backdropImagePathTagsButton.Enabled = enabled;
            _backdropImageBrowseButton.Enabled = enabled;

            // Enable or disable action status settings
            enabled = _fileActionCheckBox.Checked;
            _actionNameComboBox.Enabled = enabled;
            _actionNamePathTagsButton.Enabled = enabled;
            _actionStatusComboBox.Enabled = enabled;
        }

        /// <summary>
        /// Displays a warning message if the user specified settings are invalid.
        /// </summary>
        /// <returns><see langword="true"/> if the settings are invalid; <see langword="false"/> if 
        /// the settings are valid.</returns>
        bool WarnIfInvalid()
        {
            // ID Shield data file must be specified
            if (string.IsNullOrEmpty(_dataFileControl.DataFile))
            {
                MessageBox.Show("Please enter the ID Shield data file", "Invalid ID Shield data file",
                    MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button1, 0);
                _dataFileControl.Focus();
                return true;
            }

            // Backdrop image must be specified if checked
            if (_backdropImageCheckBox.Checked)
            {
                if (string.IsNullOrEmpty(_backdropImageTextBox.Text))
                {
                    MessageBox.Show("Please enter a backdrop image", "Invalid backdrop image",
                        MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button1, 0);
                    _backdropImageTextBox.Focus();
                    return true;
                }
                if (new FAMTagManager().StringContainsInvalidTags(_backdropImageTextBox.Text))
                {
                    MessageBox.Show("Backdrop image contains invalid tags", "Invalid backdrop image",
                        MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button1, 0);
                    _backdropImageTextBox.Focus();
                    return true;
                }
            }

            return false;
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

                // Backdrop image
                _backdropImageCheckBox.Checked = _settings.UseBackdropImage;
                _backdropImageTextBox.Text = _settings.BackdropImage;

                // Action status settings
                _fileActionCheckBox.Checked = _settings.ActionStatusSettings.Enabled;
                _actionNameComboBox.Text = GetInitialActionName();
                _actionStatusComboBox.Text = GetStringFromActionStatus(_settings.ActionStatusSettings.ActionStatus);
                
                // Input tracking
                _enableInputEventTrackingCheckBox.Checked = _settings.EnableInputTracking;

                // Full screen mode
                _launchFullScreenCheckBox.Checked = _settings.LaunchInFullScreenMode;

                // Slideshow settings
                _slideshowSettings = _settings.SlideshowSettings;
                _enableSlideshowCheckBox.Checked = _slideshowSettings.SlideshowEnabled;

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
        /// Handles the <see cref="CheckBox.CheckedChanged"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="CheckBox.CheckedChanged"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="CheckBox.CheckedChanged"/> event.</param>
        void HandleFileActionCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            try
            {
                UpdateControls();
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI29173", ex);
            }
        }

        /// <summary>
        /// Handles the <see cref="CheckBox.CheckedChanged"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="CheckBox.CheckedChanged"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="CheckBox.CheckedChanged"/> event.</param>
        void HandleBackdropImageCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            try
            {
                UpdateControls();
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI29671", ex);
            }
        }

        /// <summary>
        /// Handles the <see cref="BrowseButton.PathSelected"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="BrowseButton.PathSelected"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="BrowseButton.PathSelected"/> event.</param>
        void HandleBackdropImageBrowseButtonPathSelected(object sender, PathSelectedEventArgs e)
        {
            try
            {
                _backdropImageTextBox.Text = e.Path;
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI29670", ex);
            }
        }

        /// <summary>
        /// Handles the <see cref="PathTagsButton.TagSelected"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="PathTagsButton.TagSelected"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="PathTagsButton.TagSelected"/> event.</param>
        void HandleActionNamePathTagsButtonTagSelected(object sender, TagSelectedEventArgs e)
        {
            try
            {
                _actionNameComboBox.SetSelectedText(e.Tag);
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI29162", ex);
            }
        }

        /// <summary>
        /// Handles the slideshow settings button click.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleEnableSlideshowCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            try
            {
                UpdateControls();
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI31039", ex);
            }
        }

        /// <summary>
        /// Handles the slideshow settings button click.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleSlideshowSettingsButtonClick(object sender, EventArgs e)
        {
            try
            {
                using (SlideshowSettingsDialog dialog =
                    new SlideshowSettingsDialog(_slideshowSettings))
                {
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        _slideshowSettings = dialog.SlideshowSettings;
                    }
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI31037", ex);
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