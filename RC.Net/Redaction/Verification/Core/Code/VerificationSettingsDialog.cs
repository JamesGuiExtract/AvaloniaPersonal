using Extract.Utilities.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using UCLID_FILEPROCESSINGLib;

namespace Extract.Redaction.Verification
{
    /// <summary>
    /// Represents a dialog that allows the user to select settings for 
    /// <see cref="VerificationTask"/>.
    /// </summary>
    public partial class VerificationSettingsDialog : Form
    {
        #region VerificationSettingsDialog Fields

        /// <summary>
        /// The verification feedback settings.
        /// </summary>
        FeedbackSettings _feedback;

        /// <summary>
        /// The verification settings.
        /// </summary>
        VerificationSettings _settings;

        #endregion VerificationSettingsDialog Fields

        #region VerificationSettingsDialog Constructors

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

        #endregion VerificationSettingsDialog Constructors

        #region VerificationSettingsDialog Properties

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

        #endregion VerificationSettingsDialog Properties

        #region VerificationSettingsDialog Methods

        /// <summary>
        /// Gets the <see cref="VerificationSettings"/> from the user interface.
        /// </summary>
        /// <returns>The <see cref="VerificationSettings"/> from the user interface.</returns>
        VerificationSettings GetVerificationSettings()
        {
            // Get the settings
            GeneralVerificationSettings general = GetGeneralSettings();
            FeedbackSettings feedback = GetFeedbackSettings();
            string dataFile = _dataFileTextBox.Text;
            MetadataSettings metadata = GetMetaDataSettings();

            return new VerificationSettings(general, feedback, dataFile, metadata);
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
        /// Gets the <see cref="MetadataSettings"/> from the user interface.
        /// </summary>
        /// <returns>The <see cref="MetadataSettings"/> from the user interface.</returns>
        MetadataSettings GetMetaDataSettings()
        {
            // Get the settings
            bool alwaysOutputMetadata = _alwaysOutputMetadataRadioButton.Checked;
            string metadataFile = _metadataFileTextBox.Text;

            return new MetadataSettings(alwaysOutputMetadata, metadataFile);
        }

        /// <summary>
        /// Updates the enabled state of the controls.
        /// </summary>
        void UpdateControls()
        {
            _feedbackSettingsButton.Enabled = _collectFeedbackCheckBox.Checked;
        }

        #endregion VerificationSettingsDialog Methods

        #region VerificationSettingsDialog Overrides

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
                _dataFileTextBox.Text = _settings.InputFile;

                // Metadata settings
                if (_settings.Metadata.AlwaysOutputMetadata)
                {
                    _alwaysOutputMetadataRadioButton.Checked = true;
                }
                else
                {
                    _onlyRedactionsRadioButton.Checked = true;
                }
                _metadataFileTextBox.Text = _settings.Metadata.MetadataFile;

                // Update the UI
                UpdateControls();
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI26328", ex);
            }
        }

        #endregion VerificationSettingsDialog Overrides

        #region VerificationSettingsDialog Event Handlers

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
        /// Handles the <see cref="PathTagsButton.TagSelected"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="PathTagsButton.TagSelected"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="PathTagsButton.TagSelected"/> event.</param>
        void HandleDataFilePathTagsButtonTagSelected(object sender, TagSelectedEventArgs e)
        {
            try
            {
                _dataFileTextBox.SelectedText = e.Tag;
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI26306", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the <see cref="BrowseButton.PathSelected"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="BrowseButton.PathSelected"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="BrowseButton.PathSelected"/> event.</param>
        void HandleDataFileBrowseButtonPathSelected(object sender, PathSelectedEventArgs e)
        {
            try
            {
                _dataFileTextBox.Text = e.Path;
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI26307", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the <see cref="PathTagsButton.TagSelected"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="PathTagsButton.TagSelected"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="PathTagsButton.TagSelected"/> event.</param>
        void HandleMetadataPathTagsButtonTagSelected(object sender, TagSelectedEventArgs e)
        {
            try
            {
                _metadataFileTextBox.SelectedText = e.Tag;
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI26308", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the <see cref="BrowseButton.PathSelected"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="BrowseButton.PathSelected"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="BrowseButton.PathSelected"/> event.</param>
        void HandleMetadataBrowseButtonPathSelected(object sender, PathSelectedEventArgs e)
        {
            try
            {
                _metadataFileTextBox.Text = e.Path;
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI26309", ex);
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
                // Ensure the ID Shield data and metadata paths are specified
                bool isEmpty = WarnIfEmpty(_dataFileTextBox, "ID Shield data file") ||
                    WarnIfEmpty(_metadataFileTextBox, "metadata output file");
                if (isEmpty)
                {
                    return;
                }

                // Store settings
                _settings = GetVerificationSettings();
                this.DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI26310", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Displays a warning message to the user if the specified textbox is empty.
        /// </summary>
        /// <param name="textBox">The textbox to check.</param>
        /// <param name="name">Short description of the text that the textbox should contain.
        /// </param>
        /// <returns><see langword="true"/> if <paramref name="textBox"/> is empty; 
        /// <see langword="false"/> if <paramref name="textBox"/> contains any text.</returns>
        static bool WarnIfEmpty(TextBox textBox, string name)
        {
            bool isEmpty = string.IsNullOrEmpty(textBox.Text);
            if (isEmpty)
            {
                MessageBox.Show("Please enter the " + name, "Invalid " + name, 
                    MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button1, 0);
                textBox.Focus();
            }
            return isEmpty;
        }

        #endregion VerificationSettingsDialog Event Handlers
    }
}