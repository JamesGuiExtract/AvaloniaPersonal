using Extract;
using Extract.Licensing;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Globalization;
using Extract.Utilities.Forms;
using System.IO;

namespace IDShieldOffice
{
    /// <summary>
    /// Represents the property page for the format of Bates numbers.
    /// </summary>
    public partial class BatesNumberManagerFormatPropertyPage : UserControl, IPropertyPage
    {
        #region BatesNumberManagerFormatPropertyPage Constants

        static readonly string _NEXT_NUMBER_FILE_TYPES =
                "TXT files (*.txt)|*.txt*|" +
                "All files (*.*)|*.*||";

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        private static readonly string _OBJECT_NAME =
            typeof(BatesNumberManagerFormatPropertyPage).ToString();

        #endregion BatesNumberManagerFormatPropertyPage Constants

        #region BatesNumberManagerFormatPropertyPage Fields

        /// <summary>
        /// The <see cref="BatesNumberManager"/> to which settings will be applied.
        /// </summary>
        readonly BatesNumberManager _batesNumberManager;

        /// <summary>
        /// Whether or not the settings on the property page have been modified.
        /// </summary>
        private bool _dirty;

        /// <summary>
        /// A dialog that allows the user to select the next number file to use.
        /// </summary>
        OpenFileDialog _nextNumberFileDialog;

        #endregion BatesNumberManagerFormatPropertyPage Fields

        #region BatesNumberManagerFormatPropertyPage Events

        /// <summary>
        /// Occurs when the property page is modified.
        /// </summary>
        public event EventHandler PropertyPageModified;

        #endregion BatesNumberManagerFormatPropertyPage Events

        #region BatesNumberManagerFormatPropertyPage Constructors

        /// <summary>
        /// Initializes a new <see cref="BatesNumberManagerFormatPropertyPage"/> class.
        /// </summary>
        // Don't fight with auto-generated code.
        //[SuppressMessage("Microsoft.Performance", "CA1805:DoNotInitializeUnnecessarily")]
        internal BatesNumberManagerFormatPropertyPage(BatesNumberManager batesNumberManager)
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
                LicenseUtilities.ValidateLicense(LicenseIdName.IdShieldOfficeObject, "ELI23186",
                    _OBJECT_NAME);

                InitializeComponent();

                // Store the Bates number manager
                _batesNumberManager = batesNumberManager;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI23187", ex);
            }
        }

        #endregion BatesNumberManagerFormatPropertyPage Constructors

        #region BatesNumberManagerFormatPropertyPage Methods

        /// <summary>
        /// Resets all the values to the values stored in <see cref="_batesNumberManager"/> and 
        /// resets the dirty flag to <see langword="false"/>.
        /// </summary>
        private void RefreshSettings()
        {
            // Next number UI elements
            BatesNumberFormat format = _batesNumberManager.Format;
            if (format.UseNextNumberFile)
	        {
                _nextNumberFileRadioButton.Checked = true;
	        }
            else
            {
                _nextNumberSpecifiedRadioButton.Checked = true;
            }
            _nextNumberSpecifiedTextBox.Text = 
                format.NextNumber.ToString(CultureInfo.CurrentCulture);
            _nextNumberFileTextBox.Text = format.NextNumberFile;
            _zeroPadCheckBox.Checked = format.ZeroPad;
            _digitsUpDown.Value = format.Digits;

            // Update the sample next number
            UpdateSampleNextNumber();

            // Bates number format UI elements
            if (format.AppendPageNumber)
            {
                _usePageNumberRadioButton.Checked = true;
            }
            else
            {
                _useBatesForEachPageRadioButton.Checked = true;
            }
            _zeroPadPageNumberCheckBox.Checked = format.ZeroPadPage;
            _pageDigitsUpDown.Value = format.PageDigits;
            _pageNumberSeparatorTextBox.Text = format.PageNumberSeparator;

            // Prefixes and suffixes UI elements
            _prefixTextBox.Text = format.Prefix;
            _suffixTextBox.Text = format.Suffix;

            // Update the sample Bates number
            UpdateSampleBatesNumber();

            // Reset the dirty flag
            _dirty = false;
        }

        /// <summary>
        /// Updates the <see cref="_sampleNextNumberTextBox"/> with the next Bates number. Does 
        /// not increment the file.
        /// </summary>
        private void UpdateSampleNextNumber()
        {
            // Get the next Bates number
            long nextNumber = BatesNumberGenerator.PeekNextNumberFromFile(ToBatesNumberFormat());

            // Set the text to the next number if valid, else to the empty string
            _sampleNextNumberTextBox.Text = nextNumber >= 0 ? 
                nextNumber.ToString(CultureInfo.CurrentCulture) : "";
        }

        /// <summary>
        /// Updates the <see cref="_sampleBatesNumberTextBox"/> with the next Bates number. Does 
        /// not increment the file or registry value.
        /// </summary>
        private void UpdateSampleBatesNumber()
        {
            _sampleBatesNumberTextBox.Text = 
                BatesNumberGenerator.PeekNextNumberString(1, ToBatesNumberFormat());
        }

        /// <summary>
        /// Retrieves the next specified number or -1 if the next number is invalid.
        /// </summary>
        /// <returns>The next specified number or -1 if the next number is invalid.</returns>
        private long GetNextSpecifiedNumber()
        {
            long number;
            return long.TryParse(_nextNumberSpecifiedTextBox.Text, out number) && number >= 0 ?
                number : -1;
        }

        /// <summary>
        /// Retrieves the user specified settings as a Bates number format object.
        /// </summary>
        /// <returns>The user specified settings as a Bates number format object.</returns>
        private BatesNumberFormat ToBatesNumberFormat()
        {
            // Create the Bates number format object
            BatesNumberFormat format = new BatesNumberFormat();

            // Store the next number settings
            format.UseNextNumberFile = _nextNumberFileRadioButton.Checked;
            format.NextNumber = GetNextSpecifiedNumber();
            format.NextNumberFile = _nextNumberFileTextBox.Text;
            format.ZeroPad = _zeroPadCheckBox.Checked;
            format.Digits = (int)_digitsUpDown.Value;

            // Store the Bates number format settings
            format.AppendPageNumber = _usePageNumberRadioButton.Checked;
            format.ZeroPadPage = _zeroPadPageNumberCheckBox.Checked;
            format.PageDigits = (int)_pageDigitsUpDown.Value;
            format.PageNumberSeparator = _pageNumberSeparatorTextBox.Text;

            // Store the prefixes and suffixes settings
            format.Prefix = _prefixTextBox.Text;
            format.Suffix = _suffixTextBox.Text;

            return format;
        }

        #endregion BatesNumberManagerFormatPropertyPage Methods

        #region BatesNumberManagerFormatPropertyPage Overrides

        /// <summary>
        /// Raises the <see cref="UserControl.Load"/> event.
        /// </summary>
        /// <param name="e">The event data associated with the <see cref="UserControl.Load"/> 
        /// event.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Refresh the UI elements
            RefreshSettings();
        }

        #endregion BatesNumberManagerFormatPropertyPage Overrides

        #region BatesNumberManagerFormatPropertyPage OnEvents

        /// <summary>
        /// Raises the <see cref="PropertyPageModified"/> event.
        /// </summary>
        /// <param name="e">The event data associated with the <see cref="PropertyPageModified"/> 
        /// event.</param>
        protected virtual void OnPropertyPageModified(EventArgs e)
        {
            _dirty = true;

            UpdateSampleBatesNumber();

            if (PropertyPageModified != null)
            {
                PropertyPageModified(this, e);
            }
        }

        #endregion BatesNumberManagerFormatPropertyPage OnEvents

        #region BatesNumberManagerFormatPropertyPage Event Handlers

        /// <summary>
        /// Handles the <see cref="RadioButton.CheckedChanged"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="RadioButton.CheckedChanged"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="RadioButton.CheckedChanged"/> event.</param>
        private void HandleNextNumberSpecifiedRadioButtonCheckedChanged(object sender, EventArgs e)
        {
            // Check if the next number specified radio button is checked
            if (_nextNumberSpecifiedRadioButton.Checked)
            {
                // Enable the specified next number text box
                _nextNumberSpecifiedTextBox.Enabled = true;
                _nextNumberFileTextBox.Enabled = false;
                _nextNumberFileButton.Enabled = false;
            }
            else
            {
                // Enable the number file text box
                _nextNumberSpecifiedTextBox.Enabled = false;
                _nextNumberFileTextBox.Enabled = true;
                _nextNumberFileButton.Enabled = true;

                // Update the sample next number
                UpdateSampleNextNumber();
            }

            // Raise the PropertyPageModified event
            OnPropertyPageModified(new EventArgs());
        }

        /// <summary>
        /// Handles the <see cref="Control.TextChanged"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="Control.TextChanged"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="Control.TextChanged"/> event.</param>
        private void HandleNextNumberSpecifiedTextBoxTextChanged(object sender, EventArgs e)
        {
            // Raise the PropertyPageModified event
            OnPropertyPageModified(new EventArgs());
        }

        /// <summary>
        /// Handles the <see cref="Control.TextChanged"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="Control.TextChanged"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="Control.TextChanged"/> event.</param>
        private void HandleNextNumberFileTextBoxTextChanged(object sender, EventArgs e)
        {
            // Check for file existence and verify that it contains a valid bates number
            // [IDSD #201 - JDS]
            if (File.Exists(_nextNumberFileTextBox.Text))
            {
                // Check that the file contains a valid Bates number
                if (BatesNumberGenerator.PeekNextNumberFromFile(ToBatesNumberFormat()) < 0)
                {
                    MessageBox.Show("File does not contain a valid Bates number!",
                        "Invalid Bates Number File", MessageBoxButtons.OK,
                        MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, 0);
                }

                // Update the sample number
                UpdateSampleNextNumber();
            }

            // Raise the PropertyPageModified event
            OnPropertyPageModified(new EventArgs());
        }

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="Control.Click"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="Control.Click"/> event.</param>
        private void HandleNextNumberFileButtonClick(object sender, EventArgs e)
        {
            // Create the next number file dialog if not already created
            if (_nextNumberFileDialog == null)
            {
                _nextNumberFileDialog = new OpenFileDialog();
                _nextNumberFileDialog.Title = "Select Next Number File";
                _nextNumberFileDialog.AddExtension = true;
                _nextNumberFileDialog.DefaultExt = "txt";
                _nextNumberFileDialog.Filter = _NEXT_NUMBER_FILE_TYPES;
                _nextNumberFileDialog.FileOk += HandleFileOk;
                _nextNumberFileDialog.CheckFileExists = false;
            }
            _nextNumberFileDialog.FileName = _nextNumberFileTextBox.Text;

            // Show the dialog and bail if the user selects cancel
            if (_nextNumberFileDialog.ShowDialog() == DialogResult.OK)
            {
                // Clear the text box first to ensure the textbox text changed event is fired
                _nextNumberFileTextBox.Text = "";

                // Display the next number file selected
                _nextNumberFileTextBox.Text = _nextNumberFileDialog.FileName;

                // Update the sample next number
                UpdateSampleNextNumber();
            }
        }

        /// <summary>
        /// Handles the <see cref="FileDialog.FileOk"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the <see cref="FileDialog.FileOk"/> event.
        /// </param>
        /// <param name="e">The event data associated with the <see cref="FileDialog.FileOk"/> 
        /// event.</param>
        private void HandleFileOk(object sender, CancelEventArgs e)
        {
            // If the file exists, we are done
            if (File.Exists(_nextNumberFileDialog.FileName))
            {
                return;
            }

            // Prompt the user whether to create the file
            DialogResult result = MessageBox.Show(
                "File \"" + _nextNumberFileDialog.FileName + "\" does not exist." + 
                Environment.NewLine + "Do you want to create it?", "Select Next Number File", 
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button1, 0);

            // Create the file or cancel
            if (result == DialogResult.Yes)
            {
                File.WriteAllText(_nextNumberFileDialog.FileName, "1");
            }
            else
            {
                e.Cancel = true;
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.Leave"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="Control.Leave"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="Control.Leave"/> event.</param>
        private void HandleSampleNextNumberTextBoxLeave(object sender, EventArgs e)
        {
            // Update the sample next number
            UpdateSampleNextNumber();
        }

        /// <summary>
        /// Handles the <see cref="CheckBox.CheckedChanged"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="CheckBox.CheckedChanged"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="CheckBox.CheckedChanged"/> event.</param>
        private void HandleZeroPadCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            _digitsUpDown.Enabled = _zeroPadCheckBox.Checked;

            // Raise the PropertyPageModified event
            OnPropertyPageModified(new EventArgs());
        }

        /// <summary>
        /// Handles the <see cref="NumericUpDown.ValueChanged"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="NumericUpDown.ValueChanged"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="NumericUpDown.ValueChanged"/> event.</param>
        private void HandleDigitsUpDownValueChanged(object sender, EventArgs e)
        {
            // Raise the PropertyPageModified event
            OnPropertyPageModified(new EventArgs());
        }

        /// <summary>
        /// Handles the <see cref="RadioButton.CheckedChanged"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="RadioButton.CheckedChanged"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="RadioButton.CheckedChanged"/> event.</param>
        private void HandleUsePageNumberRadioButtonCheckedChanged(object sender, EventArgs e)
        {
            // Check if the page number radion button is checked
            if (_usePageNumberRadioButton.Checked)
            {
                // Enable related controls
                _zeroPadPageNumberCheckBox.Enabled = true;
                _pageDigitsUpDown.Enabled = _zeroPadPageNumberCheckBox.Checked;
                _pageNumberSeparatorTextBox.Enabled = true;
            }
            else
            {
                // Disable related controls
                _zeroPadPageNumberCheckBox.Enabled = false;
                _pageDigitsUpDown.Enabled = false;
                _pageNumberSeparatorTextBox.Enabled = false;
            }

            // Raise the PropertyPageModified event
            OnPropertyPageModified(new EventArgs());
        }

        /// <summary>
        /// Handles the <see cref="CheckBox.CheckedChanged"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="CheckBox.CheckedChanged"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="CheckBox.CheckedChanged"/> event.</param>
        private void HandleZeroPadPageNumberCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            // Enable the page digits up down based on the zero pad page number check box
            _pageDigitsUpDown.Enabled = _zeroPadPageNumberCheckBox.Checked;

            // Raise the PropertyPageModified event
            OnPropertyPageModified(new EventArgs());
        }

        /// <summary>
        /// Handles the <see cref="NumericUpDown.ValueChanged"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="NumericUpDown.ValueChanged"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="NumericUpDown.ValueChanged"/> event.</param>
        private void HandlePageDigitsUpDownValueChanged(object sender, EventArgs e)
        {
            // Raise the PropertyPageModified event
            OnPropertyPageModified(new EventArgs());
        }

        /// <summary>
        /// Handles the <see cref="Control.TextChanged"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="Control.TextChanged"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="Control.TextChanged"/> event.</param>
        private void HandlePageNumberSeparatorTextBoxTextChanged(object sender, EventArgs e)
        {
            // Raise the PropertyPageModified event
            OnPropertyPageModified(new EventArgs());
        }

        /// <summary>
        /// Handles the <see cref="Control.TextChanged"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="Control.TextChanged"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="Control.TextChanged"/> event.</param>
        private void HandlePrefixTextBoxTextChanged(object sender, EventArgs e)
        {
            // Raise the PropertyPageModified event
            OnPropertyPageModified(new EventArgs());
        }

        /// <summary>
        /// Handles the <see cref="Control.TextChanged"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the 
        /// <see cref="Control.TextChanged"/> event.</param>
        /// <param name="e">The event data associated with the 
        /// <see cref="Control.TextChanged"/> event.</param>
        private void HandleSuffixTextBoxTextChanged(object sender, EventArgs e)
        {
            // Raise the PropertyPageModified event
            OnPropertyPageModified(new EventArgs());
        }

        #endregion BatesNumberManagerFormatPropertyPage Event Handlers

        #region IPropertyPage Members

        /// <summary>
        /// Applies the changes to the <see cref="BatesNumberManager"/>.
        /// </summary>
        public void Apply()
        {
            // Ensure the settings are valid
            if (!this.IsValid)
            {
                MessageBox.Show("Cannot apply changes. Settings are invalid.", "Invalid settings",
                    MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button1, 0);
                return;
            }

            BatesNumberFormat format = ToBatesNumberFormat();

            // [IDSD #71] - Check that the Bates number will be on the page
            if (!BatesNumberManager.CheckValidBatesNumberLocation(_batesNumberManager.ImageViewer,
                _batesNumberManager.HorizontalInches, _batesNumberManager.VerticalInches,
                _batesNumberManager.AnchorAlignment, _batesNumberManager.PageAnchorAlignment,
                _batesNumberManager.Font, format))
            {
                return;
            }

            // Store the changes
            _batesNumberManager.Format = format;

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
                return _dirty;
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
                // Check whether a next number file is used
                if (_nextNumberFileRadioButton.Checked)
                {
                    // Ensure the file is valid and contains a valid next number [IDSD #201 - JDS]
                    if (File.Exists(_nextNumberFileTextBox.Text))
                    {
                        // Get the next Bates number
                        long nextNumber =
                            BatesNumberGenerator.PeekNextNumberFromFile(ToBatesNumberFormat());

                        // Check that the number from the file is valid
                        return nextNumber >= 0;
                    }

                    return false;
                }

                // Ensure the next specified number is a non-negative integer
                return GetNextSpecifiedNumber() >= 0;
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
