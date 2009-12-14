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

namespace Extract.Imaging
{
    /// <summary>
    /// Represents the property page for the format of Bates numbers.
    /// </summary>
    public partial class BatesNumberFormatPropertyPage : UserControl, IPropertyPage
    {
        #region BatesNumberManagerFormatPropertyPage Constants

        static readonly string _NEXT_NUMBER_FILE_TYPES =
                "TXT files (*.txt)|*.txt*|" +
                "All files (*.*)|*.*||";

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        private static readonly string _OBJECT_NAME =
            typeof(BatesNumberFormatPropertyPage).ToString();

        #endregion BatesNumberManagerFormatPropertyPage Constants

        #region BatesNumberManagerFormatPropertyPage Fields

        /// <summary>
        /// Whether or not the settings on the property page have been modified.
        /// </summary>
        private bool _dirty;

        /// <summary>
        /// A dialog that allows the user to select the next number file to use.
        /// </summary>
        OpenFileDialog _nextNumberFileDialog;

        /// <summary>
        /// The format object that will be modified by this dialog
        /// </summary>
        BatesNumberFormat _format;

        /// <summary>
        /// The <see cref="IBatesNumberGenerator"/> object to use.
        /// </summary>
        IBatesNumberGenerator _generator;

        /// <summary>
        /// The list of values for the counter combo box
        /// </summary>
        List<string> _counters;

        #endregion BatesNumberManagerFormatPropertyPage Fields

        #region BatesNumberManagerFormatPropertyPage Events

        /// <summary>
        /// Occurs when the property page is modified.
        /// </summary>
        public event EventHandler PropertyPageModified;

        #endregion BatesNumberManagerFormatPropertyPage Events

        #region BatesNumberManagerFormatPropertyPage Constructors

        /// <summary>
        /// Initializes a new <see cref="BatesNumberFormatPropertyPage"/> class.
        /// </summary>
        /// <param name="format">The <see cref="BatesNumberFormat"/> to save the settings to.</param>
        /// <param name="generator">The <see cref="IBatesNumberGenerator"/> object to use
        /// when updating the sample bates number.</param>
        public BatesNumberFormatPropertyPage(BatesNumberFormat format,
            IBatesNumberGenerator generator) : this(format, generator, null)
        {
        }

        /// <summary>
        /// Initializes a new <see cref="BatesNumberFormatPropertyPage"/> class.
        /// </summary>
        /// <param name="format">The <see cref="BatesNumberFormat"/> to save the settings to.</param>
        /// <param name="generator">The <see cref="IBatesNumberGenerator"/> to use when
        /// updating the sample boxes on the property page.</param>
        /// <param name="counters">Specifies the values to show in the counters combo box.
        /// <para><b>Note:</b></para>
        /// If <paramref name="counters"/> is <see langword="null"/> then the counters combo
        /// box will be hidden. If <paramref name="counters"/> is not <see lanword="null"/>
        /// then the radion buttons for specifying a file or next number value will be hidden.</param>
        // Don't fight with auto-generated code.
        //[SuppressMessage("Microsoft.Performance", "CA1805:DoNotInitializeUnnecessarily")]
        public BatesNumberFormatPropertyPage(BatesNumberFormat format,
            IBatesNumberGenerator generator, IEnumerable<string> counters)
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
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI23186",
					_OBJECT_NAME);

                ExtractException.Assert("ELI27842", "Bates number generator must not be NULL.",
                    generator != null);

                InitializeComponent();

                // If counters has been specified, load the list
                if (counters != null)
                {
                    _counters = new List<string>();
                    _counters.AddRange(counters);
                }

                _generator = generator;

                _format = format;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI23187", ex);
            }
        }

        #endregion BatesNumberManagerFormatPropertyPage Constructors

        #region BatesNumberManagerFormatPropertyPage Methods

        /// <summary>
        /// Resets all the values to the values stored in <see cref="_format"/> and 
        /// resets the dirty flag to <see langword="false"/>.
        /// </summary>
        private void RefreshSettings()
        {
            _zeroPadCheckBox.Checked = _format.ZeroPad;
            _digitsUpDown.Value = _format.Digits;

            // Update the values based on the counter list
            if (_counters != null)
            {
                // Select the appropriate item (if it exists) in the combo box
                // otherwise just select the first item in the list
                int index = -1;
                if (!string.IsNullOrEmpty(_format.DatabaseCounter))
                {
                    index = _counterToUseCombo.FindString(_format.DatabaseCounter);
                    if (index == -1)
                    {
                        // Prompt the user about non-existent counter
                        MessageBox.Show("Counter: " + _format.DatabaseCounter + " was not found.",
                            "Counter Not Found", MessageBoxButtons.OK, MessageBoxIcon.Information,
                            MessageBoxDefaultButton.Button1, 0);

                        // Set the index to 0 so that the first counter is selected (if
                        // no items in the list then use -1 as the index)
                        index = _counters.Count > 0 ? 0 : -1;
                    }
                }

                // Just select the first counter
                _counterToUseCombo.SelectedIndex = index;
            }
            else
            {
                // Next number UI elements
                if (_format.UseNextNumberFile)
                {
                    _nextNumberFileRadioButton.Checked = true;
                }
                else
                {
                    _nextNumberSpecifiedRadioButton.Checked = true;
                }
                _nextNumberSpecifiedTextBox.Text =
                    _format.NextNumber.ToString(CultureInfo.CurrentCulture);
                _nextNumberFileTextBox.Text = _format.NextNumberFile;
            }

            // Bates number format UI elements
            if (_format.AppendPageNumber)
            {
                _usePageNumberRadioButton.Checked = true;
            }
            else
            {
                _useBatesForEachPageRadioButton.Checked = true;
            }
            _zeroPadPageNumberCheckBox.Checked = _format.ZeroPadPage;
            _pageDigitsUpDown.Value = _format.PageDigits;
            _pageNumberSeparatorTextBox.Text = _format.PageNumberSeparator;

            // Prefixes and suffixes UI elements
            _prefixTextBox.Text = _format.Prefix;
            _suffixTextBox.Text = _format.Suffix;

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
            using (BatesNumberFormat format = GetSettingsAsFormat())
            {
                UpdateSampleNextNumber(format);
            }
        }

        /// <summary>
        /// Updates the <see cref="_sampleNextNumberTextBox"/> with the next Bates number. Does 
        /// not increment the file.
        /// </summary>
        /// <param name="format">The format object to use to create the sample number.</param>
        private void UpdateSampleNextNumber(BatesNumberFormat format)
        {
            long nextNumber = BatesNumberGenerator.PeekNextNumberFromFile(format);

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
            using (BatesNumberFormat format = GetSettingsAsFormat())
            {
                // Store the current format object
                BatesNumberFormat originalFormat = _generator.Format;
                try
                {
                    // Set the generator to use the new format
                    _generator.Format = format;

                    // Update the number
                    _sampleBatesNumberTextBox.Text =
                        _generator.PeekNextNumberString(1);
                }
                finally
                {
                    // Reset generator to old format
                    _generator.Format = originalFormat;
                }
            }
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
        /// Stores the user specified settings to the <see cref="BatesNumberFormat"/> object.
        /// </summary>
        private void SaveSettingsToFormat()
        {
            // Set whether the format should be using a database counter
            _format.UseDatabaseCounter = _counterToUseCombo.Visible;
            _format.DatabaseCounter = _counterToUseCombo.Visible ? _counterToUseCombo.Text : "";

            // Store the next number settings
            _format.UseNextNumberFile = _nextNumberFileRadioButton.Checked;
            _format.NextNumber = GetNextSpecifiedNumber();
            _format.NextNumberFile = _nextNumberFileTextBox.Text;
            _format.ZeroPad = _zeroPadCheckBox.Checked;
            _format.Digits = (int)_digitsUpDown.Value;

            // Store the Bates number format settings
            _format.AppendPageNumber = _usePageNumberRadioButton.Checked;
            _format.ZeroPadPage = _zeroPadPageNumberCheckBox.Checked;
            _format.PageDigits = (int)_pageDigitsUpDown.Value;
            _format.PageNumberSeparator = _pageNumberSeparatorTextBox.Text;

            // Store the prefixes and suffixes settings
            _format.Prefix = _prefixTextBox.Text;
            _format.Suffix = _suffixTextBox.Text;
        }

        /// <summary>
        /// Retrieves the user specified settings as a <see cref="BatesNumberFormat"/> object.
        /// </summary>
        /// <returns>The user specified settings as a <see cref="BatesNumberFormat"/> object.</returns>
        BatesNumberFormat GetSettingsAsFormat()
        {
            BatesNumberFormat format = new BatesNumberFormat();

            // Set whether the format should be using a database counter
            format.UseDatabaseCounter = _counterToUseCombo.Visible;
            format.DatabaseCounter = _counterToUseCombo.Visible ? _counterToUseCombo.Text : "";

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

        /// <summary>
        /// Moves the zero pad check box and control and shrinks the next number
        /// group box as well as the entire property page when using the
        /// database counter as opposed to a file or specified counter value.
        /// </summary>
        private void MoveZeroPadAndResizeForDatabase()
        {
            // Move the controls up (store the distance moved)
            int distance = _zeroPadCheckBox.Top - (_counterToUseCombo.Bottom
                + _counterToUseLabel.Margin.Bottom + _zeroPadCheckBox.Margin.Top);
            _zeroPadCheckBox.Location = new Point(_zeroPadCheckBox.Left, _zeroPadCheckBox.Top - distance);
            _digitsUpDown.Location = new Point(_digitsUpDown.Left, _digitsUpDown.Top - distance);
            _digitsLabel.Location = new Point(_digitsLabel.Left, _digitsLabel.Top - distance);

            // Resize the group box accordingly
            _nextNumberGroupBox.Size = new Size(_nextNumberGroupBox.Size.Width,
                _nextNumberGroupBox.Size.Height - distance);

            // Move each group box
            _formatGroupBox.Location = new Point(_formatGroupBox.Left,
                _formatGroupBox.Top - distance);
            _prefixesGroupBox.Location = new Point(_prefixesGroupBox.Left,
                _prefixesGroupBox.Top - distance);
            _sampleGroupBox.Location = new Point(_sampleGroupBox.Left,
                _sampleGroupBox.Top - distance);

            // Resize the property page accordingly
            Size = new Size(Size.Width, Size.Height - distance);
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

            // Show/hide controls based on the counters list
            if (_counters != null)
            {
                // Fill the combo box with the counters list
                _counterToUseCombo.Items.AddRange(_counters.ToArray());

                // Hide the other controls and resize the group box
                _nextNumberFileRadioButton.Visible = false;
                _nextNumberFileTextBox.Visible = false;
                _nextNumberFileButton.Visible = false;
                _nextNumberSpecifiedRadioButton.Visible = false;
                _nextNumberSpecifiedTextBox.Visible = false;
                _nextNumberLabel.Visible = false;
                _sampleNextNumberTextBox.Visible = false;

                MoveZeroPadAndResizeForDatabase();
            }
            else
            {
                _counterToUseLabel.Visible = false;
                _counterToUseCombo.Visible = false;
            }

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
                using (BatesNumberFormat format = GetSettingsAsFormat())
                {
                    if (BatesNumberGenerator.PeekNextNumberFromFile(format) < 0)
                    {
                        MessageBox.Show("File does not contain a valid Bates number!",
                            "Invalid Bates Number File", MessageBoxButtons.OK,
                            MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, 0);
                    }

                    // Update the sample number
                    UpdateSampleNextNumber(format);
                }
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

        /// <summary>
        /// Handles <see cref="ComboBox.SelectedIndexChanged"/> event.
        /// </summary>
        /// <param name="sender">The object which sent the event.</param>
        /// <param name="e">The data associated with the event.</param>
        private void HandleUserCounterComboChanged(object sender, EventArgs e)
        {
            // Get the new name
            OnPropertyPageModified(new EventArgs());
        }

        #endregion BatesNumberManagerFormatPropertyPage Event Handlers

        #region IPropertyPage Members

        /// <summary>
        /// Applies the changes to the <see cref="BatesNumberFormat"/>.
        /// </summary>
        public void Apply()
        {
            try
            {
                // Ensure the settings are valid
                if (!this.IsValid)
                {
                    MessageBox.Show("Cannot apply changes. Settings are invalid.", "Invalid settings",
                        MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button1, 0);
                    return;
                }

                SaveSettingsToFormat();

                // Reset the dirty flag
                _dirty = false;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI27840", ex);
            }
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
                if (_nextNumberFileRadioButton.Visible && _nextNumberFileRadioButton.Checked)
                {
                    // Ensure the file is valid and contains a valid next number [IDSD #201 - JDS]
                    if (File.Exists(_nextNumberFileTextBox.Text))
                    {
                        using (BatesNumberFormat format = GetSettingsAsFormat())
                        {
                            // Get the next Bates number
                            long nextNumber =
                                BatesNumberGenerator.PeekNextNumberFromFile(format);

                            // Check that the number from the file is valid
                            return nextNumber >= 0;
                        }
                    }

                    return false;
                }
                else if (_nextNumberSpecifiedRadioButton.Visible && _nextNumberSpecifiedRadioButton.Checked)
                {
                    // Ensure the next specified number is a non-negative integer
                    return GetNextSpecifiedNumber() >= 0;
                }
                else
                {
                    // Ensure a counter is selected
                    return _counterToUseCombo.SelectedIndex >= 0;
                }
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
