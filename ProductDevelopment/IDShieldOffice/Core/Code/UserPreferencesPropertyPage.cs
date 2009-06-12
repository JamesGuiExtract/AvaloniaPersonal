using Extract;
using Extract.Imaging;
using Extract.Imaging.Forms;
using Extract.Licensing;
using Extract.Utilities;
using Extract.Utilities.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Security.Permissions;
using System.Text;
using System.Windows.Forms;
using UCLID_COMUTILSLib;

namespace IDShieldOffice
{
    /// <summary>
    /// Represents the property page of the ID Shield Office user preferences.
    /// </summary>
    public partial class UserPreferencesPropertyPage : UserControl, IPropertyPage
    {
        #region UserPreferencesPropertyPage Constants

        /// <summary>
        /// The file name to use for the sample source document.
        /// </summary>
        static readonly string _SAMPLE_SOURCE_DOC_NAME = @"C:\Images\FolderA\Image001.tif";

        /// <summary>
        /// The default output path for IDSO data files
        /// </summary>
        static readonly string _DEFAULT_IDSO_OUTPUT_PATH = @"<SourceDocName>.idso";

        /// <summary>
        /// The default output path for TIFF files.
        /// </summary>
        static readonly string _DEFAULT_TIFF_OUTPUT_PATH =
            @"$DirOf(<SourceDocName>)\$FileNoExtOf(<SourceDocName>).redacted.tif";

        /// <summary>
        /// The default output path for PDF files.
        /// </summary>
        static readonly string _DEFAULT_PDF_OUTPUT_PATH =
            @"$DirOf(<SourceDocName>)\$FileNoExtOf(<SourceDocName>).redacted.pdf";

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        private static readonly string _OBJECT_NAME =
            typeof(UserPreferencesPropertyPage).ToString();

        #endregion UserPreferencesPropertyPage Constants

        #region UserPreferencesPropertyPage Fields

        /// <summary>
        /// The <see cref="UserPreferences"/> to which settings will be applied.
        /// </summary>
        private UserPreferences _userPreferences;

        /// <summary>
        /// Whether or not the settings on the property page have been modified.
        /// </summary>
        private bool _dirty;

        /// <summary>
        /// Miscellaneous utilities, used for tags management
        /// </summary>
        MiscUtils _miscUtils = new MiscUtils();

        /// <summary>
        /// The image viewer with which the <see cref="UserPreferencesPropertyPage"/> is associated.
        /// </summary>
        readonly ImageViewer _imageViewer;

        #endregion UserPreferencesPropertyPage Fields

        #region UserPreferencesPropertyPage Constructors

        /// <summary>
        /// Initializes a new <see cref="UserPreferencesPropertyPage"/> class.
        /// </summary>
        /// <param name="imageViewer">The image viewer for which the preferences apply.</param>
        /// <param name="userPreferences">The user preferences to be configured.</param>
        // Don't fight with auto-generated code.
        //[SuppressMessage("Microsoft.Performance", "CA1805:DoNotInitializeUnnecessarily")]
        public UserPreferencesPropertyPage(ImageViewer imageViewer, UserPreferences userPreferences)
        {
            try
            {
                // Load licenses in design mode
                if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
                {
                    // Load the license files from folder
                    LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());
                }

                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.IdShieldOfficeObject, "ELI23204",
                    _OBJECT_NAME);

                InitializeComponent();

                _imageViewer = imageViewer;

                // Store the user preferences
                _userPreferences = userPreferences;

                // Fill the fill color combo box from the redaction color enum
                _redactionFillColorComboBox.Items.Clear();
                _redactionFillColorComboBox.Items.AddRange(Enum.GetNames(typeof(RedactionColor)));

                // Dock the Bates number property page in the appropriate tab
                FormsMethods.DockControlIntoContainer(
                    (Control)_userPreferences.BatesNumberManager.PropertyPage, this, _batesNumberPage);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI23205", ex);
            }
        }
        
        #endregion UserPreferencesPropertyPage Constructors

        #region UserPreferencesPropertyPage Methods

        /// <summary>
        /// Resets all the values to the values stored in <see cref="_userPreferences"/> and 
        /// resets the dirty flag to <see langword="false"/>.
        /// </summary>
        private void RefreshSettings()
        {
            // Set the UI elements
            _ocrTradeoffTrackBar.Value = (int)_userPreferences.OcrTradeoff;
            _redactionFillColorComboBox.Text = _userPreferences.RedactionFillColor.ToString();
            _verifyAllPagesCheckBox.Checked = _userPreferences.VerifyAllPages;
            _saveIdsoWithImageCheckBox.Checked = _userPreferences.SaveIdsoWithImage;
            
            // Update the output file format and path
            switch (_userPreferences.OutputFormat)
            {
                case OutputFormat.Tif:
                    _outputFormatComboBox.Text = "TIF";
                    break;

                case OutputFormat.Pdf:
                    _outputFormatComboBox.Text = "PDF";
                    break;

                case OutputFormat.Idso:
                    _outputFormatComboBox.Text = "IDSO";
                    break;

                default:
                    ExtractException ee = new ExtractException("ELI22196", 
                        "Unexpected output format.");
                    ee.AddDebugData("Output format", _userPreferences.OutputFormat, false);
                    throw ee;
            }
            _useOutputPath.Checked = _userPreferences.UseOutputPath;
            _outputPathTextBox.Text = _userPreferences.OutputPath;

            // Update the sample output file label
            UpdateSampleOutputFile();

            // Reset the dirty flag
            _dirty = false;
        }

        /// <summary>
        /// Updates the text on the <see cref="_sampleOutputFileName"/>.
        /// </summary>
        void UpdateSampleOutputFile()
        {
            try
            {
                // Get the use output path checkbox state
                bool useOutputPath = _useOutputPath.Checked;

                // Update the labels visibility based on the check box state
                _sampleInputFileName.Visible = useOutputPath;
                _sampleOutputFileName.Visible = useOutputPath;
                _labelSampleInputFileName.Visible = useOutputPath;
                _labelSampleOutputFileName.Visible = useOutputPath;

                // Use the open document as the sample filename if an image is currently open.
                _sampleInputFileName.Text = _imageViewer.IsImageAvailable ? _imageViewer.ImageFile :
                    _SAMPLE_SOURCE_DOC_NAME;

                // Update the output file label based on the output path text box
                _sampleOutputFileName.Text = string.IsNullOrEmpty(_outputPathTextBox.Text) ? ""
                    : _miscUtils.GetExpandedTags(_outputPathTextBox.Text, _sampleInputFileName.Text);

                // In case the paths are bigger than the text boxes, show the ends of the paths
                // rather than the beginnings.
                EnsureEndsOfFilePathsAreVisible();
            }
            catch
            {
                _sampleOutputFileName.Text = "Invalid";
            }
        }

        /// <summary>
        /// In case the output file example paths are bigger than the text boxes that contain them, 
        /// ensure the ends of the paths are visible rather than the start
        /// </summary>
        void EnsureEndsOfFilePathsAreVisible()
        {
            if (this._useOutputPath.Checked)
            {
                // To get as much of the path visible as possible while showing the end of the path, 
                // move the caret to the start then end of each path, then call ScrollToCaret.
                // (NOTE: This only works if the controls are visible)
                _sampleOutputFileName.Select(0, 0);
                _sampleOutputFileName.Select(_sampleOutputFileName.Text.Length, 0);
                _sampleOutputFileName.ScrollToCaret();
                _sampleInputFileName.Select(0, 0);
                _sampleInputFileName.Select(_sampleInputFileName.Text.Length, 0);
                _sampleInputFileName.ScrollToCaret();
            }
        }

        /// <summary>
        /// Updates the path in the output path text box based on the
        /// currently selected output format.
        /// </summary>
        private void UpdateOutputPathTextBox()
        {
            // Disable the output path controls if the output format is IDSO
            bool outputIdso = _outputFormatComboBox.Text == "IDSO";
            _outputPathTextBox.Enabled = !outputIdso;
            _pathTagsButton.Enabled = !outputIdso;

            // Set the output path based on the output format
            if (outputIdso)
            {
                _outputPathTextBox.Text = _DEFAULT_IDSO_OUTPUT_PATH;
            }
            else if (_outputFormatComboBox.Text == "PDF")
            {
                _outputPathTextBox.Text = _DEFAULT_PDF_OUTPUT_PATH;
            }
            else
            {
                _outputPathTextBox.Text = _DEFAULT_TIFF_OUTPUT_PATH;
            }
        }

        #endregion UserPreferencesPropertyPage Methods

        #region UserPreferencesPropertyPage OnEvents

        /// <summary>
        /// Raises the <see cref="UserControl.Load"/> event.
        /// </summary>
        /// <param name="e"><see cref="EventArgs"/> that contain the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Update the sample input file text
            _sampleInputFileName.Text = _SAMPLE_SOURCE_DOC_NAME;

            // Refresh the UI elements
            RefreshSettings();
        }

        /// <summary>
        /// Raises the <see cref="Control.Resize"/> event.
        /// </summary>
        /// <param name="e"><see cref="EventArgs"/> that contain the event data.</param>
        protected override void OnResize(EventArgs e)
        {
            try
            {
                base.OnResize(e);

                // Reposition the sample output path within their TextBoxes to ensure
                // maximum visibility
                EnsureEndsOfFilePathsAreVisible();
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI23380", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        #endregion UserPreferencesPropertyPage OnEvents

        #region UserPreferencesPropertyPage Event Handlers

        /// <summary>
        /// Handles the <see cref="TrackBar.ValueChanged"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the <see cref="TrackBar.ValueChanged"/> 
        /// event.</param>
        /// <param name="e">The event data associated with the <see cref="TrackBar.ValueChanged"/> 
        /// event.</param>
        private void HandleOcrTradeoffTrackBarValueChanged(object sender, EventArgs e)
        {
            // Raise the property page modified event
            OnPropertyPageModified();
        }

        /// <summary>
        /// Handles the <see cref="ComboBox.SelectedIndexChanged"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="ComboBox.SelectedIndexChanged"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="ComboBox.SelectedIndexChanged"/> event.</param>
        private void HandleRedactionFillColorComboBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            // Raise the property page modified event
            OnPropertyPageModified();
        }

        /// <summary>
        /// Handles the <see cref="CheckBox.CheckedChanged"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the <see cref="CheckBox.CheckedChanged"/> 
        /// event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="CheckBox.CheckedChanged"/> event.</param>
        private void HandleSaveIdsoWithImageCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            // Raise the property page modified event
            OnPropertyPageModified();
        }

        /// <summary>
        /// Handles the <see cref="ComboBox.SelectedIndexChanged"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="ComboBox.SelectedIndexChanged"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="ComboBox.SelectedIndexChanged"/> event.</param>
        private void HandleOutputFormatComboBoxSelectedIndexChanged(object sender, EventArgs e)
        {
            // If the use output path check box is selected, then update the text box
            if (_useOutputPath.Checked)
            {
                UpdateOutputPathTextBox();
            }

            // Raise the property page modified event
            OnPropertyPageModified();

            // Update the sample output file label
            UpdateSampleOutputFile();
        }

        /// <summary>
        /// Handles the <see cref="Control.TextChanged"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="Control.TextChanged"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="Control.TextChanged"/> event.</param>
        private void HandleOutputPathTextBoxTextChanged(object sender, EventArgs e)
        {
            // Raise the property page modified event
            OnPropertyPageModified();

            // Update the sample output file label
            UpdateSampleOutputFile();
        }

        /// <summary>
        /// Handles the <see cref="PathTagsButton.TagSelected"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="PathTagsButton.TagSelected"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="PathTagsButton.TagSelected"/> event.</param>
        private void HandlePathTagsButtonTagSelected(object sender, TagSelectedEventArgs e)
        {
            _outputPathTextBox.SelectedText = e.Tag;
        }

        /// <summary>
        /// Handles the <see cref="CheckBox.Checked"/> event for the verify all pages
        /// check box.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="CheckBox.Checked"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="CheckBox.Checked"/> event.</param>
        void HandleVerifyAllPagesCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            try
            {
                // Raise the property page modified event
                OnPropertyPageModified();
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI23308", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the <see cref="CheckBox.Checked"/> event for the use output path
        /// check box.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="CheckBox.Checked"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="CheckBox.Checked"/> event.</param>
        void HandleUseOutputPathCheckBoxChanged(object sender, EventArgs e)
        {
            try
            {
                // If the check box is checked then enable the output path text box
                // and path tags button and fill in the default output path string
                if (_useOutputPath.Checked)
                {
                    _outputPathTextBox.Enabled = true;
                    _pathTagsButton.Enabled = true;
                    UpdateOutputPathTextBox();
                }
                else
                {
                    // Disable the text box and button and set the text to empty string
                    _outputPathTextBox.Enabled = false;
                    _pathTagsButton.Enabled = false;
                    _outputPathTextBox.Text = "";
                }

                // Update the sample output file
                UpdateSampleOutputFile();

                // Raise the property page changed event
                OnPropertyPageModified();
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI23356", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the <see cref="LinkLabel.LinkClicked"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="LinkLabel.LinkClicked"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="LinkLabel.LinkClicked"/> event.</param>
        private void HandleLinkLabelSeeExamplesClicked(object sender,
            LinkLabelLinkClickedEventArgs e)
        {
            IDShieldOfficeForm.ShowDocTagsHelp(this.TopLevelControl);
        }

        #endregion UserPreferencesPropertyPage Event Handlers

        #region IPropertyPage Members

        /// <summary>
        /// Event raised when the dirty flag is set.
        /// </summary>
        public event EventHandler PropertyPageModified;

        /// <summary>
        /// Raises the PropertyPageModified event.
        /// </summary>
        private void OnPropertyPageModified()
        {
            try
            {
                // Set the dirty flag
                _dirty = true;

                // If there is a listener for the event then raise it.
                if (PropertyPageModified != null)
                {
                    PropertyPageModified(this, null);
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI22215", ex);
            }
        }

        /// <summary>
        /// Applies the changes to the <see cref="UserPreferences"/>.
        /// </summary>
        public void Apply()
        {
            // Get the property page interface of the Bates Number Manager
            IPropertyPage propertyPage = 
                (IPropertyPage) _userPreferences.BatesNumberManager.PropertyPage;

            // Ensure the settings are valid
            if (!this.IsValid || !propertyPage.IsValid)
            {
                MessageBox.Show("Cannot apply changes. Settings are invalid.", "Invalid settings",
                    MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button1, 0);
                return;
            }

            // Apply the changes to the Bates number manager
            propertyPage.Apply();

            // Ensure the changes were applied
            if (propertyPage.IsDirty)
            {
                // No need to display a message. The Apply() method has already done this.
                return;
            }

            // Store the settings
            _userPreferences.OcrTradeoff = (OcrTradeoff) _ocrTradeoffTrackBar.Value;
            _userPreferences.RedactionFillColor = (RedactionColor)Enum.Parse(typeof(RedactionColor),
                _redactionFillColorComboBox.Text, true);
            _userPreferences.SaveIdsoWithImage = _saveIdsoWithImageCheckBox.Checked;
            _userPreferences.OutputPath = _outputPathTextBox.Text;
            _userPreferences.UseOutputPath = _useOutputPath.Checked;
            _userPreferences.VerifyAllPages = _verifyAllPagesCheckBox.Checked;

            if (_outputFormatComboBox.Text == "TIF")
            {
                _userPreferences.OutputFormat = OutputFormat.Tif;
            }
            else if (_outputFormatComboBox.Text == "PDF")
            {
                _userPreferences.OutputFormat = OutputFormat.Pdf;
            }
            else
            {
                _userPreferences.OutputFormat = OutputFormat.Idso;
            }

            // Reset the dirty flag
            _dirty = false;
        }

        /// <summary>
        /// Gets whether the settings on the property page have been modified.
        /// </summary>
        /// <return><see langword="true"/> if the settings on the property page have been modified;
        /// <see langword="false"/> if they have not been modified.</return>
        public bool IsDirty
        {
            get
            {
                // Get the property page interface of the Bates Number Manager
                IPropertyPage propertyPage = 
                    (IPropertyPage) _userPreferences.BatesNumberManager.PropertyPage;

                // Check if this or the Bates number property page are dirty
                return _dirty || propertyPage.IsDirty;
            }
        }

        /// <summary>
        /// Gets whether the user-specified settings on the property page are valid.
        /// </summary>
        /// <value><see langword="true"/> if the user-specified settings are valid; 
        /// <see langword="false"/> if the settings are not valid.</value>
        public bool IsValid
        {
            get
            {
                // TODO: implement IsValid
                return true;
            }
        }

        /// <summary>
        /// Sets the focus to the first control in the property page.
        /// </summary>
        public void SetFocusToFirstControl()
        {
            // Do nothing
        }

        #endregion
    }
}
