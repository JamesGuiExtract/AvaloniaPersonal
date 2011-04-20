using Extract.Utilities;
using Extract.Utilities.Forms;
using System;
using System.Drawing;
using System.Windows.Forms;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;

namespace Extract.Redaction.Verification
{
    /// <summary>
    /// A dialog to allow for the configuration of redaction verification slideshow settings.
    /// </summary>
    public partial class SlideshowSettingsDialog : Form
    {
        #region Constants

        /// <summary>
        /// The display name of the category for dialogs dealing with configuring the document
        /// condition.
        /// </summary>
        static readonly string _CONDITION_CATEGORY_NAME = "Condition";

        /// <summary>
        /// The COM category for dialogs dealing with configuring the document condition.
        /// </summary>
        static readonly string _CONDITION_CATEGORY = "Extract FAM Conditions";

        #endregion Constants

        #region Fields

        /// <summary>
        /// The verification slideshow task settings.
        /// </summary>
        SlideshowSettings _settings;

        /// <summary>
        /// A lazily instantiated <see cref="MiscUtils"/> instance to use configuring the
        /// _documentCondition.
        /// </summary>
        MiscUtils _miscUtils;

        /// <summary>
        /// Contains the <see cref="IFAMCondition"/> that can be used to determine whether the
        /// slideshow should be paused for a particular document.
        /// </summary>
        ObjectWithDescription _documentCondition;

        /// <summary>
        /// Indicates whether the form has been loaded.
        /// </summary>
        bool _loaded;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SlideshowSettingsDialog"/> class.
        /// </summary>
        public SlideshowSettingsDialog()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SlideshowSettingsDialog"/> class.
        /// </summary>
        /// <param name="settings">The <see cref="SlideshowSettings"/> representing the current
        /// slidehow settings.</param>
        public SlideshowSettingsDialog(SlideshowSettings settings)
        {
            try
            {
                InitializeComponent();

                _settings = settings ?? new SlideshowSettings();

                FileProcessingDB database = new FileProcessingDB();
                database.ConnectLastUsedDBThisProcess();

                // Populate the available actions for which the file action status may be set.
                StrToStrMap actionNameToId = database.GetActions();
                VariantVector actionNames = actionNameToId.GetKeys();
                int size = actionNames.Size;
                for (int i = 0; i < size; i++)
                {
                    _actionNameComboBox.Items.Add(actionNames[i]);
                }

                // Populate the list of tag names that may be applied.
                VariantVector tagNames = database.GetTagNames();
                size = tagNames.Size;
                for (int i = 0; i < size; i++)
                {
                    string tagName = (string)tagNames[i];
                    _tagNameComboBox.Items.Add(tagName);
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI31031", ex);
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets the <see cref="MiscUtils"/> instance.
        /// </summary>
        /// <value>The<see cref="MiscUtils"/> instance.</value>
        MiscUtils MiscUtils
        {
            get
            {
                if (_miscUtils == null)
                {
                    _miscUtils = new MiscUtilsClass();
                }

                return _miscUtils;
            }
        }

        /// <summary>
        /// Gets or sets the The <see cref="SlideshowSettings"/> representing the current slidehow
        /// settings.
        /// </summary>
        /// <value>The The <see cref="SlideshowSettings"/> representing the current slidehow
        /// settings.</value>
        public SlideshowSettings SlideshowSettings
        {
            get
            {
                return _settings;
            }

            set
            {
                _settings = value;
            }
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

                _applyDocumentTagCheckBox.Checked = _settings.ApplyAutoAdvanceTag;
                _tagNameComboBox.Text = _settings.AutoAdvanceTag;

                _setFileActionStatusCheckBox.Checked = _settings.ApplyAutoAdvanceActionStatus;
                _actionNameComboBox.Text = _settings.AutoAdvanceActionName;
                _actionStatusComboBox.Text = GetStringFromActionStatus(_settings.AutoAdvanceActionStatus);

                _pauseOnDocumentConditionCheckBox.Checked = _settings.CheckDocumentCondition;
                _documentCondition = _settings.DocumentCondition;

                _forceFitToPageModeCheckBox.Checked = _settings.ForceFitToPageMode;
                _promptRandomlyRadioButton.Checked = _settings.PromptRandomly;
                _promptIntervalUpDown.Value = _settings.PromptInterval;
                _requireRunKeyRadioButton.Checked = _settings.RequireRunKey;

                _loaded = true;

                UpdateControls();

                _promptRandomlyRadioButton.CheckedChanged += ((sender, eventArgs) =>
                    {
                        _promptIntervalUpDown.Enabled = _promptRandomlyRadioButton.Checked;
                    });

                _applyDocumentTagCheckBox.CheckedChanged += ((sender, eventArgs) =>
                    {
                        _tagNameComboBox.Enabled = _applyDocumentTagCheckBox.Checked;
                    });

                _setFileActionStatusCheckBox.CheckedChanged += ((sender, eventArgs) =>
                    {
                        bool enabled = _setFileActionStatusCheckBox.Checked;
                        _actionNameComboBox.Enabled = enabled;
                        _actionNamePathTagsButton.Enabled = enabled;
                        _actionStatusComboBox.Enabled = enabled;
                    });

                _pauseOnDocumentConditionCheckBox.CheckedChanged += ((sender, eventArgs) =>
                    {
                        _documentConditionButton.Enabled = _pauseOnDocumentConditionCheckBox.Checked;
                        _documentConditionTextBox.Enabled  = _pauseOnDocumentConditionCheckBox.Checked;
                    });
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI31032", ex);
            }
        }

        #endregion Overrides

        #region Event handlers

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
                ExtractException.Display("ELI31040", ex);
            }
        }

        /// <summary>
        /// Handles the ok button click.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleOkButtonClick(object sender, EventArgs e)
        {
            try
            {
                if (WarnIfInvalid())
                {
                    return;
                }

                // Apply the newly configured settings.
                _settings = new SlideshowSettings(true, _applyDocumentTagCheckBox.Checked,
                    _tagNameComboBox.Text, _setFileActionStatusCheckBox.Checked,
                    _actionNameComboBox.Text, GetActionStatusFromString(_actionStatusComboBox.Text),
                    _pauseOnDocumentConditionCheckBox.Checked, _documentCondition,
                    _forceFitToPageModeCheckBox.Checked, _promptRandomlyRadioButton.Checked,
                    (int)_promptIntervalUpDown.Value, _requireRunKeyRadioButton.Checked);

                this.DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI31043", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the document condition click.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void HandleDocumentConditionClick(object sender, EventArgs e)
        {
            try
            {
                // Use MiscUtils to configure the document condition COM object.
                Point menuLocationPoint = PointToScreen(ClientRectangle.Location);
                menuLocationPoint.Offset(_documentConditionButton.Right, _documentConditionButton.Top);

                Guid guid = new Guid();
                MiscUtils.HandlePlugInObjectCommandButtonClick(_documentCondition, _CONDITION_CATEGORY_NAME,
                    _CONDITION_CATEGORY, true, 0, ref guid, menuLocationPoint.X, menuLocationPoint.Y);

                UpdateControls();
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI31112", ex);
            }
        }

        /// <summary>
        /// Handles the document condition text box double click.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void HandleDocumentConditionTextBoxDoubleClick(object sender, EventArgs e)
        {
            try
            {
                Guid guid = new Guid();
                MiscUtils.HandlePlugInObjectDoubleClick(_documentCondition, _CONDITION_CATEGORY_NAME,
                    _CONDITION_CATEGORY, true, 0, guid);

                UpdateControls();
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI31125", ex);
            }
        }

        /// <summary>
        /// Handles the case that the user entered an invalid value which was corrected.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandlePromptIntervalTextCorrected(object sender, EventArgs e)
        {
            try
            {
                UtilityMethods.ShowMessageBox(
                    "The alertness prompt interval must be between 1 and 1000 pages.",
                    "Invalid alertness prompt interval", true);
                _promptIntervalUpDown.Focus();
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI32368", ex);
            }
        }

        #endregion Event handlers

        #region Private members

        /// <summary>
        /// Updates the enabled state of the controls dependent on the settings of others.
        /// </summary>
        void UpdateControls()
        {
            if (_loaded)
            {
                _tagNameComboBox.Enabled = _applyDocumentTagCheckBox.Checked;

                _actionNameComboBox.Enabled = _setFileActionStatusCheckBox.Checked;
                _actionNamePathTagsButton.Enabled = _setFileActionStatusCheckBox.Checked;
                _actionStatusComboBox.Enabled = _setFileActionStatusCheckBox.Checked;

                _documentConditionButton.Enabled = _pauseOnDocumentConditionCheckBox.Checked;
                if (_documentCondition.Object == null)
                {
                    _documentConditionTextBox.Text = String.Empty;
                }
                else
                {
                    _documentConditionTextBox.Text = _documentCondition.Description;
                }

                _promptIntervalUpDown.Enabled = _promptRandomlyRadioButton.Checked;
            }
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

            ExtractException ee = new ExtractException("ELI31113",
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

            ExtractException ee = new ExtractException("ELI31114",
                "Unexpected action status");
            ee.AddDebugData("Action status", status, false);
            throw ee;
        }

        /// <summary>
        /// Displays a warning message if the user specified settings are invalid.
        /// </summary>
        /// <returns><see langword="true"/> if the settings are invalid; <see langword="false"/> if 
        /// the settings are valid.</returns>
        bool WarnIfInvalid()
        {
            if (_applyDocumentTagCheckBox.Checked)
            {
                if (_tagNameComboBox.Items.Count == 0)
                {
                    MessageBox.Show("No tags exist in the database which can be applied.", "No tags available",
                        MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button1, 0);
                    _applyDocumentTagCheckBox.Focus();
                    return true;
                }

                if (string.IsNullOrEmpty(_tagNameComboBox.Text))
                {
                    MessageBox.Show("Specify which document tag should be applied.", "Tag not specified",
                        MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button1, 0);
                    _tagNameComboBox.Focus();
                    return true;
                }
            }

            if (_setFileActionStatusCheckBox.Checked)
            {
                if (!_actionNameComboBox.Items.Contains(_actionNameComboBox.Text) &&
                    _actionNameComboBox.Text.IndexOf('$') == -1)
                {
                    MessageBox.Show("Enter an existing action name if FAM tags are not being used.",
                        "Invalid action name", MessageBoxButtons.OK, MessageBoxIcon.None,
                        MessageBoxDefaultButton.Button1, 0);
                    _actionNameComboBox.Focus();
                    return true;
                }

                if (string.IsNullOrEmpty(_actionStatusComboBox.Text))
                {
                    MessageBox.Show("Specify the new status that should be applied for the action \"" +
                        _actionNameComboBox.Text + "\"", "Action status not specified",
                        MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button1, 0);
                    _actionStatusComboBox.Focus();
                    return true;
                }
            }

            if (_pauseOnDocumentConditionCheckBox.Checked && _documentCondition.Object == null)
            {
                MessageBox.Show("Specify document condition on which the slideshow should be paused.", 
                    "Document condition not specified", MessageBoxButtons.OK, MessageBoxIcon.None,
                    MessageBoxDefaultButton.Button1, 0);
                _documentConditionButton.Focus();
                return true;
            }

            return false;
        }

        #endregion Private members
    }
}
