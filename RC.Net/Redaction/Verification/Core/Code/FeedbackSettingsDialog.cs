using Extract.Utilities.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace Extract.Redaction.Verification
{
    /// <summary>
    /// Represents a dialog that allows the user to select <see cref="FeedbackSettings"/>.
    /// </summary>
    public partial class FeedbackSettingsDialog : Form
    {
        #region FeedbackSettingsDialog Fields

        /// <summary>
        /// Verification feedback settings.
        /// </summary>
        FeedbackSettings _settings;

        #endregion FeedbackSettingsDialog Fields

        #region FeedbackSettingsDialog Constructors

        /// <summary>
        /// Initializes a new <see cref="FeedbackSettingsDialog"/> class.
        /// </summary>
        // Don't fight with auto-generated code.
        //[SuppressMessage("Microsoft.Performance", "CA1805:DoNotInitializeUnnecessarily")]
        public FeedbackSettingsDialog() : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FeedbackSettingsDialog"/> class.
        /// </summary>
        public FeedbackSettingsDialog(FeedbackSettings settings)
        {
            InitializeComponent();

            _settings = settings ?? new FeedbackSettings();
        }

        #endregion FeedbackSettingsDialog Constructors

        #region FeedbackSettingsDialog Properties

        /// <summary>
        /// Gets or sets the verification feedback settings.
        /// </summary>
        /// <value>The verification feedback settings.</value>
        /// <returns>The verification feedback settings.</returns>
        public FeedbackSettings FeedbackSettings
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
        
        #endregion FeedbackSettingsDialog Properties

        #region FeedbackSettingsDialog Methods

        /// <summary>
        /// Gets the <see cref="FeedbackSettings"/> from the user interface.
        /// </summary>
        /// <returns>The <see cref="FeedbackSettings"/> from the user interface.</returns>
        FeedbackSettings GetFeedbackSettings()
        {
            // Get the settings
            string dataFolder = _dataFolderTextBox.Text;
            bool collectOriginalDocument = _collectOriginalDocumentCheckBox.Checked;
            bool useOriginalFileNames = _originalFileNamesRadioButton.Checked;
            CollectionTypes collectionTypes = GetCollectionTypes();

            return new FeedbackSettings(true, dataFolder, collectOriginalDocument, 
                useOriginalFileNames, collectionTypes);
        }

        /// <summary>
        /// Gets the <see cref="CollectionTypes"/> from the user interface.
        /// </summary>
        /// <returns>The <see cref="CollectionTypes"/> from the user interface.</returns>
        CollectionTypes GetCollectionTypes()
        {
            // Check if all documents are being collected
            if (_collectAllCheckBox.Checked)
            {
                return CollectionTypes.All;
            }

            // Check which individual types are being collected
            CollectionTypes collectionTypes = CollectionTypes.None;
            if (_collectRedactedCheckBox.Checked)
            {
                collectionTypes |= CollectionTypes.Redacted;
            }
            if (_collectCorrectedCheckBox.Checked)
            {
                collectionTypes |= CollectionTypes.Corrected;
            }

            return collectionTypes;
        }

        /// <summary>
        /// Updates the enabled state of the controls.
        /// </summary>
        void UpdateControls()
        {
            // If collect all is selected, check all other checkboxes
            bool collectAll = _collectAllCheckBox.Checked;
            if (collectAll)
            {
                _collectRedactedCheckBox.Checked = true;
                _collectCorrectedCheckBox.Checked = true;
            }

            // Enable checkboxes iff not collecting all documents
            _collectRedactedCheckBox.Enabled = !collectAll;
            _collectCorrectedCheckBox.Enabled = !collectAll;
        }

        /// <summary>
        /// Displays a warning message if the user specified settings are invalid.
        /// </summary>
        /// <returns><see langword="true"/> if the settings are invalid; <see langword="false"/> if 
        /// the settings are valid.</returns>
        bool WarnIfInvalid()
        {
            // The data folder cannot be empty
            if (string.IsNullOrEmpty(_dataFolderTextBox.Text))
            {
                MessageBox.Show("Please enter a feedback data folder", "Invalid data folder", 
                    MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button1, 0);
                _dataFolderTextBox.Focus();
                return true;
            }

            // At least one document type must be collected
            if (!_collectAllCheckBox.Checked && !_collectRedactedCheckBox.Checked && 
                !_collectCorrectedCheckBox.Checked)
            {
                MessageBox.Show(
                    "Please specify at least one document type from which to collect feedback", 
                    "Invalid settings", MessageBoxButtons.OK, MessageBoxIcon.None, 
                    MessageBoxDefaultButton.Button1, 0);
                _collectAllCheckBox.Focus();
                return true;
            }

            return false;
        }

        #endregion FeedbackSettingsDialog Methods

        #region FeedbackSettingsDialog Overrides

        /// <summary>
        /// Raises the <see cref="Form.Load"/> event.
        /// </summary>
        /// <param name="e">The event data associated with the <see cref="Form.Load"/> event.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            try
            {
                // Data storage and options
                _dataFolderTextBox.Text = _settings.DataFolder;
                _collectOriginalDocumentCheckBox.Checked = _settings.CollectOriginalDocument;

                // Filenames to use for feedback data
                if (_settings.UseOriginalFileNames)
                {
                    _originalFileNamesRadioButton.Checked = true;
                }
                else
                {
                    _uniqueFileNamesRadioButton.Checked = true;
                }

                // Collect feedback for
                CollectionTypes types = _settings.CollectionTypes;
                _collectAllCheckBox.Checked = types == CollectionTypes.All;
                _collectRedactedCheckBox.Checked = (types & CollectionTypes.Redacted) > 0;
                _collectCorrectedCheckBox.Checked = (types & CollectionTypes.Corrected) > 0;

                UpdateControls();
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI26336", ex);
            }
        }

        #endregion FeedbackSettingsDialog Overrides

        #region FeedbackSettingsDialog Event Handlers

        /// <summary>
        /// Handles the <see cref="BrowseButton.PathSelected"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="BrowseButton.PathSelected"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="BrowseButton.PathSelected"/> event.</param>
        void HandleDataFolderBrowseButtonPathSelected(object sender, PathSelectedEventArgs e)
        {
            try
            {
                _dataFolderTextBox.Text = e.Path;
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI26333", ex);
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
        void HandleCollectAllCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            try
            {
                UpdateControls();
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI26334", ex);
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
                if (WarnIfInvalid())
                {
                    return;
                }

                // Store settings
                _settings = GetFeedbackSettings();
                this.DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI26335", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        #endregion FeedbackSettingsDialog Event Handlers
    }
}