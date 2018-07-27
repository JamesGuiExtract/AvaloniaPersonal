using Extract.FileActionManager.Forms;
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
        #region Constants
        static readonly string _PRESERVED_REDACTION_QA_TEXT = "(You will see viewed status of redaction items" +
                                                              " and pages as of the time the documentation most " +
                                                              " recently verified)";

        static readonly string _RESET_REDACTION_QA_TEXT = "(You will not see viewed status of redaction items" +
                                                              " and pages as of the time the documentation most " +
                                                              " recently verified)";
        #endregion

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
        /// Specifies which tags should be available to the users.
        /// </summary>
        FileTagSelectionSettings _tagSettings;

        /// <summary>
        /// The <see cref="FileProcessingDB"/>.
        /// </summary>
        FileProcessingDB _fileProcessingDB;

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

            FAMDBUtils dbUtils = new FAMDBUtils();
            Type mgrType = Type.GetTypeFromProgID(dbUtils.GetFAMDBProgId());
            _fileProcessingDB = (FileProcessingDB)Activator.CreateInstance(mgrType);

            _fileProcessingDB.ConnectLastUsedDBThisProcess();

            StrToStrMap actionNameToId = _fileProcessingDB.GetActions();
            VariantVector actionNames = actionNameToId.GetKeys();
            int size = actionNames.Size;
            for (int i = 0; i < size; i++)
            {
                _actionNameComboBox.Items.Add(actionNames[i]);
            }

            UpdateRedactionVerificationState(settings.RedactionVerificationMode.VerificationMode);
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
            bool launchInFullScreenMode = _launchFullScreenCheckBox.Checked;
            SlideshowSettings slideshowSettings = GetSlideshowSettings();
            bool allowTags = _allowTagsCheckBox.Checked;
            VerificationModeSetting verificationModeSetting = GetVerificationModeSetting();

            return new VerificationSettings(general, feedback, dataFile, useBackdropImage,
                backdropImage, action, launchInFullScreenMode,
                slideshowSettings, allowTags, _tagSettings, verificationModeSetting);
        }

        /// <summary>
        /// Gets the verification mode setting.
        /// </summary>
        /// <returns></returns>
        VerificationModeSetting GetVerificationModeSetting()
        {
            if (_redactionVerificationRadioButton.Checked)
            {
                return new VerificationModeSetting(VerificationMode.Verify);
            }

            const int preserveIndex = 0;
            if (preserveIndex == _redactionQaComboBox.SelectedIndex)
            {
                return new VerificationModeSetting(VerificationMode.QAModePreserveViewStatus);
            }
            else
            {
                return new VerificationModeSetting(VerificationMode.QAModeResetViewStatus);
            }
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
            bool verifyAllItems = _verifyAllItemsCheckBox.Checked;
            bool verifyFullPageCluesOnly = _verifyFullPageCluesCheckBox.Checked;
            bool requireTypes = _requireTypeCheckBox.Checked;
            bool requireExemptions = _requireExemptionsCheckBox.Checked;
            bool allowSeamlessNavigation = _seamlessNavigationCheckBox.Checked;
            bool promptForSaveUntilCommit = _promptForSaveUntilCommit.Checked;

            return new GeneralVerificationSettings(verifyAllPages, verifyAllItems, verifyFullPageCluesOnly, requireTypes,
                requireExemptions, allowSeamlessNavigation, promptForSaveUntilCommit);
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
            bool forcefitToPageMode = _slideshowSettings.ForceFitToPageMode;
            bool promptRandomly = _slideshowSettings.PromptRandomly;
            int promptInterval = _slideshowSettings.PromptInterval;
            bool requireRunKey = _slideshowSettings.RequireRunKey;

            return new SlideshowSettings(enabled, applyAutoAdvanceTag, autoAdvanceTag,
                applyAutoAdvanceActionStatus, autoAdvanceActionName, autoAdvanceActionStatus,
                checkDocTypeConditon, documentCondition, forcefitToPageMode, promptRandomly,
                promptInterval, requireRunKey);
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
            // Enable or disable nested settings buttons
            _feedbackSettingsButton.Enabled = _collectFeedbackCheckBox.Checked;
            _slideshowSettingsButton.Enabled = _enableSlideshowCheckBox.Checked;
            _tagSettingsButton.Enabled = _allowTagsCheckBox.Checked;

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

            // Enable or disable settings for verifyFullPageClues checkboax
            enabled = !_verifyAllItemsCheckBox.Checked && !_verifyAllPagesCheckBox.Checked;
            _verifyFullPageCluesCheckBox.Enabled = enabled;

            _verifyAllPagesCheckBox.Enabled = !(enabled && _verifyFullPageCluesCheckBox.Checked);
            _verifyAllItemsCheckBox.Enabled = !(enabled && _verifyFullPageCluesCheckBox.Checked);
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

        /// <summary>
        /// Updates the state of the redaction verification controls.
        /// </summary>
        /// <param name="mode">The mode.</param>
        void UpdateRedactionVerificationState(VerificationMode mode)
        {
            if (mode == VerificationMode.Verify)
            {
                _redactionVerificationRadioButton.Checked = true;
                _redactionQaComboBox.SelectedIndex = 0;
                _redactionQaExplanatoryLabel.Visible = false;
            }
            else
            {
                _redactionQaRadioButton.Checked = true;

                const int preserveIndex = 0;
                const int resetIndex = 1;

                _redactionQaComboBox.SelectedIndex =
                    mode == VerificationMode.QAModePreserveViewStatus ? preserveIndex : resetIndex;
            }
        }

        /// <summary>
        /// Changes the text to track redaction qa drop down.
        /// </summary>
        void ChangeTextToTrackRedactionQaDropDown()
        {
            try
            {
                const int preservedSelectedIndex = 0;
                if (preservedSelectedIndex == _redactionQaComboBox.SelectedIndex)
                {
                    _redactionQaExplanatoryLabel.Text = _PRESERVED_REDACTION_QA_TEXT;
                }
                else
                {
                    _redactionQaExplanatoryLabel.Text = _RESET_REDACTION_QA_TEXT;
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI39832");
            }
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
                _verifyAllItemsCheckBox.Checked = _settings.General.VerifyAllItems;
                _verifyFullPageCluesCheckBox.Checked = _settings.General.VerifyFullPageCluesOnly;
                _requireExemptionsCheckBox.Checked = _settings.General.RequireExemptions;
                _requireTypeCheckBox.Checked = _settings.General.RequireTypes;
                _seamlessNavigationCheckBox.Checked = _settings.General.AllowSeamlessNavigation;
                _feedback = _settings.Feedback;
                _collectFeedbackCheckBox.Checked = _feedback.Collect;
                _launchFullScreenCheckBox.Checked = _settings.LaunchInFullScreenMode;
                _promptForSaveUntilCommit.Checked = _settings.General.PromptForSaveUntilCommit;

                // ID Shield data file
                _dataFileControl.DataFile = _settings.InputFile;

                // Backdrop image
                _backdropImageCheckBox.Checked = _settings.UseBackdropImage;
                _backdropImageTextBox.Text = _settings.BackdropImage;

                // Slideshow settings
                _slideshowSettings = _settings.SlideshowSettings;
                _enableSlideshowCheckBox.Checked = _slideshowSettings.SlideshowEnabled;

                // Action status settings
                _fileActionCheckBox.Checked = _settings.ActionStatusSettings.Enabled;
                _actionNameComboBox.Text = GetInitialActionName();
                _actionStatusComboBox.Text = GetStringFromActionStatus(_settings.ActionStatusSettings.ActionStatus);

                // Tag settings
                _allowTagsCheckBox.Checked = _settings.AllowTags;
                _tagSettings = _settings.TagSettings;

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
        /// Handles the <see cref="CheckBox.CheckedChanged"/> event of the
        /// <see cref="_allowTagsCheckBox"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleAllowTagsCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                UpdateControls();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI37218");
            }
        }

        /// <summary>
        /// Handles the <see cref="_tagSettingsButton"/> <see cref="Control.Click"/> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleTagSettingsButtonClick(object sender, EventArgs e)
        {
            try
            {
                using (FileTagSelectionDialog dialog =
                    new FileTagSelectionDialog(_tagSettings, _fileProcessingDB))
                {
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        _tagSettings = dialog.Settings;
                    }
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI37219", ex);
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

        /// <summary>
        /// Handles the CheckedChanged event of the _redactionVerificationRadioButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        void _redactionVerificationRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                _redactionVerifyExplanatoryTextLabel.Visible = _redactionVerificationRadioButton.Checked;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI39829");
            }
        }

        /// <summary>
        /// Handles the CheckedChanged event of the _redactionQaRadioButton control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        void _redactionQaRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                bool checkd = _redactionQaRadioButton.Checked;

                _redactionQaExplanatoryLabel.Visible = checkd;
                _redactionQaComboBox.Enabled = checkd;

                if (checkd)
                {
                    ChangeTextToTrackRedactionQaDropDown();
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI39830");
            }
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the _redactionQaComboBox control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        void _redactionQaComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                ChangeTextToTrackRedactionQaDropDown();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI39831");
            }
        }

        /// <summary>
        /// Handles the CheckedChanged event of _verifyAllPagesCheckBox, _verifyFullPageCluesCheckBox and 
        /// _verifyAllItemsCheckbox controls
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        void HandleCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                UpdateControls();
            }
            catch (Exception ex_)
            {
                ex_.ExtractDisplay("ELI46172");
            }
        }

        #endregion Event Handlers
    }
}