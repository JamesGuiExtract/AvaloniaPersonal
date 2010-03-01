using Extract.Licensing;
using Extract.Drawing;
using Extract.Imaging;
using Extract.Utilities.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using UCLID_FILEPROCESSINGLib;
using UCLID_COMUTILSLib;

namespace Extract.FileActionManager.FileProcessors
{
    /// <summary>
    /// The dialog for configuring the settings of an <see cref="ApplyBatesNumberTask"/>.
    /// </summary>
    internal partial class ApplyBatesNumberSettingsDialog : Form
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        private static readonly string _OBJECT_NAME =
            typeof(ApplyBatesNumberSettingsDialog).ToString();

        /// <summary>
        /// The file filter string for the file browse button.
        /// </summary>
        private static readonly string _FILE_FILTERS =
            "BMP files (*.bmp;*.rle;*.dib)|*.bmp*;*.rle*;*.dib*|"
            + "GIF files (*.gif)|*.gif*|JFIF files (*.jpg)|*.jpg*|PCX files (*.pcx)|*.pcx*|"
            + "PICT files (*.pct)|*.pct*|PNG files (*.png)|*.png*|TIFF files (*.tif)|*.tif*|"
            + "PDF files (*.pdf)|*.pdf*|All image files|*.bmp*;*.rle*;*.dib*;*.rst*;*.gp4*;"
            + "*.mil*;*.cal*;*.cg4*;*.flc*;*.fli*;*.gif*;*.jpg*;*.pcx*;*.pct*;*.png*;*.tga*;"
            + "*.tif*;*.pdf*|All files (*.*)|*.*||";

        #endregion Constants

        #region Fields

        /// <summary>
        /// The <see cref="BatesNumberGeneratorWithDatabase"/> used to set the format and
        /// update the sample edit controls.
        /// </summary>
        BatesNumberGeneratorWithDatabase _generator;

        /// <summary>
        /// The file that will receive the Bates numbers
        /// </summary>
        string _fileName;

        /// <summary>
        /// The <see cref="BatesNumberFormatPropertyPage"/> for setting the format of
        /// the Bates number.
        /// </summary>
        BatesNumberFormatPropertyPage _batesFormat;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplyBatesNumberSettingsDialog"/> class.
        /// </summary>
        /// <param name="generator">The <see cref="BatesNumberGeneratorWithDatabase"/>
        /// to use when getting the sample next bates number.</param>
        /// <param name="fileName">The file that the bates number will be applied to.</param>
        public ApplyBatesNumberSettingsDialog(BatesNumberGeneratorWithDatabase generator,
            string fileName)
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
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI27879",
					_OBJECT_NAME);

                InitializeComponent();

                ExtractException.Assert("ELI27880", "BatesNumberGenerator must not be NULL.",
                    generator != null);

                // Clone the format and create a new BatesNumberGenerator
                BatesNumberFormat format = generator.Format.Clone();
                _generator = new BatesNumberGeneratorWithDatabase(format, generator.DatabaseManager);

                // Copy the file name
                _fileName = fileName;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI27881", ex);
            }
        }

        #endregion Constructors

        #region Event Handlers

        /// <summary>
        /// Raises the <see cref="Form.OnLoad"/> event.
        /// </summary>
        /// <param name="e">The data associated with the event.</param>
        protected override void OnLoad(EventArgs e)
        {
            try
            {
                base.OnLoad(e);

                // Add the bates number format control
                AddBatesNumberFormatControl();

                // Add the file name to the text box
                if (!string.IsNullOrEmpty(_fileName))
                {
                    _fileNameTextBox.Text = _fileName;

                    // Set the cursor to the end of the file name
                    _fileNameTextBox.Select(_fileNameTextBox.Text.Length, 0);
                }

                // Add the filter string to the browse button
                _browseButton.FileFilter = _FILE_FILTERS;

                // Update the description for the appearance
                UpdateAppearanceSummary();
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI27882", ex);
                ee.AddDebugData("Event Data", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Raises the <see cref="Form.Closing"/> event.
        /// </summary>
        /// <param name="e">The data associated with the event.</param>
        protected override void OnClosing(CancelEventArgs e)
        {
            try
            {
                if (DialogResult == DialogResult.OK)
                {
                    // Ensure a file name has been specified
                    if (string.IsNullOrEmpty(_fileNameTextBox.Text))
                    {
                        // Prompt the user, set the focus to the file name control and
                        // set cancel to true (stops the closing of the form)
                        MessageBox.Show("The file name field may not be left blank.",
                            "Invalid Settings", MessageBoxButtons.OK, MessageBoxIcon.Information,
                            MessageBoxDefaultButton.Button1, 0);
                        _fileNameTextBox.Select();
                        e.Cancel = true;
                    }
                    // Check if the bates format is valid
                    else if (!_batesFormat.IsValid)
                    {
                        // Invalid format, prompt the user, set focus to the bates format control
                        // and set cancel to true (stops the closing of the form)
                        MessageBox.Show("Bates format settings are not valid, please check.",
                            "Invalid Settings", MessageBoxButtons.OK, MessageBoxIcon.Information,
                            MessageBoxDefaultButton.Button1, 0);
                        _batesFormat.SetFocusToFirstControl();
                        e.Cancel = true;
                    }
                    else
                    {
                        // Save the format settings
                        _batesFormat.Apply();

                        // Store the file name
                        _fileName = _fileNameTextBox.Text;
                    }
                }

                base.OnClosing(e);
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI27985", ex);
                ee.AddDebugData("Event Data", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event for the appearance button.
        /// Displayes the configuration property page for setting the position and
        /// appearance.
        /// </summary>
        /// <param name="sender">The object which sent the event.</param>
        /// <param name="e">The data associated with the event.</param>
        private void HandleChangeAppearanceButtonClick(object sender, EventArgs e)
        {
            try
            {
                // Create the property page and display it
                PropertyPageForm appearanceForm = new PropertyPageForm(
                    "Change Bates Number Default Position And Appearance",
                    new BatesNumberAppearancePropertyPage(_generator.Format));

                // Show the dialog
                DialogResult result = appearanceForm.ShowDialog(this);

                if (result == DialogResult.OK)
                {
                    // Update the description for the appearance
                    UpdateAppearanceSummary();
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI27883", ex);
                ee.AddDebugData("Event Data", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event for the <see cref="PathTagsButton"/>.
        /// </summary>
        /// <param name="sender">The object which sent the event.</param>
        /// <param name="e">The data associated with the event.</param>
        private void HandlePathTagsButtonClick(object sender, TagSelectedEventArgs e)
        {
            try
            {
                _fileNameTextBox.SelectedText = e.Tag;
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI27884", ex);
                ee.AddDebugData("Event Data", e, false);
                ee.Display();
            }
        }

        #endregion Event Handlers

        #region Methods

        /// <summary>
        /// Computes a user readable string representation of the page anchor alignment
        /// for the Bates number.
        /// </summary>
        /// <param name="batesNumberFormat">The format object containing the PageAnchorAlignment.</param>
        /// <returns>A string representation of the anchor point.</returns>
        static string GetPositionOfFont(BatesNumberFormat batesNumberFormat)
        {
            StringBuilder sb = new StringBuilder();
            switch (batesNumberFormat.PageAnchorAlignment)
            {
                case AnchorAlignment.LeftBottom:
                    sb.Append("Bottom left");
                    break;

                case AnchorAlignment.RightBottom:
                    sb.Append("Bottom right");
                    break;

                case AnchorAlignment.LeftTop:
                    sb.Append("Top left");
                    break;

                case AnchorAlignment.RightTop:
                    sb.Append("Top right");
                    break;

                default:
                    ExtractException ee = new ExtractException("ELI27885",
                        "Unexpected page anchor alignment.");
                    ee.AddDebugData("Alignment", batesNumberFormat.PageAnchorAlignment, false);
                    throw ee;
            }

            sb.Append(" of page");

            return sb.ToString();
        }

        /// <summary>
        /// Adds the <see cref="BatesNumberFormatPropertyPage"/> user control to the
        /// settings dialog.
        /// </summary>
        void AddBatesNumberFormatControl()
        {
            // Get the list of database counters
            VariantVector names = _generator.DatabaseManager.GetUserCounterNames();

            // Ensure there is at least 1 item in the list
            int size = names.Size;
            if (size == 0)
            {
                ExtractException ee = new ExtractException("ELI27886",
                    "User counter list was empty.");
                ee.Display();
            }

            // Fill a list of strings with the counter names
            List<string> counterNames = new List<string>(size);
            for (int i = 0; i < size; i++)
            {
                // Get the name as a string
                string name = names[i] as string;
                if (name == null)
                {
                    throw new ExtractException("ELI27887", "User counter list was invalid.");
                }

                counterNames.Add(name);
            }

            // Sort the list
            counterNames.Sort();

            // Create the bates number format control and add it to the form
            _batesFormat = new BatesNumberFormatPropertyPage(_generator.Format,
                _generator, counterNames);
            _batesFormat.Location = new Point(_fileNameGroupBox.Left - _batesFormat.Margin.Left,
                _fileNameGroupBox.Bottom + _fileNameGroupBox.Margin.Top);
            Controls.Add(_batesFormat);
            _batesFormat.TabIndex = 1;

            // Compute the new size of the form
            int width = _batesFormat.Width + (2 * _batesFormat.Location.X)
                + _batesFormat.Margin.Horizontal;
            int height = Height + Margin.Vertical + _batesFormat.Height +
                _batesFormat.Margin.Vertical;

            // Resize the form appropriately
            Size = new Size(width, height);
        }

        /// <summary>
        /// Updates the readonly edit control associated with the Bates number appearance
        /// settings.
        /// </summary>
        void UpdateAppearanceSummary()
        {
            _appearanceSummaryText.Text = "Font: "
                + FontMethods.GetUserFriendlyFontString(_generator.Format.Font)
                + Environment.NewLine + "Position: " + GetPositionOfFont(_generator.Format)
                + Environment.NewLine + "Alignment: "
                + AnchorAlignmentHelper.GetAlignmentAsString(_generator.Format.AnchorAlignment);
        }

        #endregion Methods

        #region Properties

        /// <summary>
        /// Gets the <see cref="BatesNumberGenerator"/> object that is being used by
        /// the settings dialog.
        /// </summary>
        public BatesNumberGeneratorWithDatabase BatesNumberGenerator
        {
            get
            {
                return _generator;
            }
        }

        /// <summary>
        /// Returns the file name that has been set by the dialog.
        /// </summary>
        public string FileName
        {
            get
            {
                return _fileName;
            }
        }

        #endregion Properties
    }
}