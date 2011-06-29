using Extract.Licensing;
using Extract.Utilities;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms;

namespace Extract.FileActionManager.Conditions
{
    /// <summary>
    /// A <see cref="Form"/> that allows for configuration of an <see cref="FileTimestampCondition"/>
    /// instance.
    /// </summary>
    public partial class FileTimestampConditionSettingsDialog : Form
    {
        #region Constants

        /// <summary>
        /// The object name.
        /// </summary>
        static readonly string _OBJECT_NAME =
            typeof(FileTimestampConditionSettingsDialog).ToString();

        #endregion Constants

        #region Constructors

        /// <summary>
        /// Initializes static data for the <see cref="FileTimestampConditionSettingsDialog"/>
        /// class.
        /// </summary>
        // FXCop seems to believe this is here to initialize static fields. That is not the case.
        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static FileTimestampConditionSettingsDialog()
        {
            try
            {
                // Assign readable values for enums to be displayed in combo boxes.
                FileTimestampProperty.Created.SetReadableValue("created");
                FileTimestampProperty.Accessed.SetReadableValue("accessed");
                FileTimestampProperty.Modified.SetReadableValue("modified");

                TimeSpanUnit.Seconds.SetReadableValue("seconds");
                TimeSpanUnit.Minutes.SetReadableValue("minutes");
                TimeSpanUnit.Hours.SetReadableValue("hours");
                TimeSpanUnit.Days.SetReadableValue("days");
                TimeSpanUnit.Weeks.SetReadableValue("weeks");
                TimeSpanUnit.Months.SetReadableValue("months");
                TimeSpanUnit.Years.SetReadableValue("years");

                DateComparisonOperator.Equal.SetReadableValue("equal to");
                DateComparisonOperator.NotEqual.SetReadableValue("not equal to");
                DateComparisonOperator.After.SetReadableValue("after");
                DateComparisonOperator.OnOrAfter.SetReadableValue("equal to or after");
                DateComparisonOperator.Before.SetReadableValue("before");
                DateComparisonOperator.OnOrBefore.SetReadableValue("equal to or before");

                TimePeriod.ThisMinute.SetReadableValue("this minute");
                TimePeriod.LastMinute.SetReadableValue("last minute");
                TimePeriod.ThisHour.SetReadableValue("this hour");
                TimePeriod.LastHour.SetReadableValue("last hour");
                TimePeriod.Today.SetReadableValue("today");
                TimePeriod.Yesterday.SetReadableValue("yesterday");
                TimePeriod.ThisWeek.SetReadableValue("this week");
                TimePeriod.LastWeek.SetReadableValue("last week");
                TimePeriod.ThisMonth.SetReadableValue("this month");
                TimePeriod.LastMonth.SetReadableValue("last month");
                TimePeriod.ThisYear.SetReadableValue("this year");
                TimePeriod.LastYear.SetReadableValue("last year");
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI32799");
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FileTimestampConditionSettingsDialog"/> class.
        /// </summary>
        public FileTimestampConditionSettingsDialog(FileTimestampCondition settings)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI32800",
                    _OBJECT_NAME);

                Settings = settings;

                InitializeComponent();

                _file1PathTags.PathTags = new FileActionManagerPathTags();
                _file2PathTags.PathTags = _file1PathTags.PathTags;

                // Specify reactions to changes in controls.
                _rangeRadioButton.CheckedChanged += ((sender, args) =>
                    {
                        _rangeStartDateTimePicker.Enabled = _rangeRadioButton.Checked;
                        _rangeEndDateTimePicker.Enabled = _rangeRadioButton.Checked;
                    });

                _compareRadioButton.CheckedChanged += ((sender, args) =>
                    {
                        try
                        {
                            bool enable = _compareRadioButton.Checked;

                            _comparisonComboBox.Enabled = enable;

                            _staticTimeCompareRadioButton.Enabled = enable;
                            _relativeTimeCompareRadioButton.Enabled = enable;
                            _relativeTimePeriodCompareRadioButton.Enabled = enable;
                            _fileCompareRadioButton.Enabled = enable;

                            _comparisonDateTimePicker.Enabled =
                                enable && _staticTimeCompareRadioButton.Checked;
                            _numberAgoUpDown.Enabled =
                                enable && _relativeTimeCompareRadioButton.Checked;
                            _unitsAgoComboBox.Enabled =
                                enable && _relativeTimeCompareRadioButton.Checked;
                            _timePeriodComboBox.Enabled =
                                enable && _relativeTimePeriodCompareRadioButton.Checked;
                            _file2PropertyComboBox.Enabled =
                                enable && _fileCompareRadioButton.Checked;
                            _file2TextBox.Enabled = enable && _fileCompareRadioButton.Checked;
                            _file2PathTags.Enabled = enable && _fileCompareRadioButton.Checked;
                            _file2Browse.Enabled = enable && _fileCompareRadioButton.Checked;
                        }
                        catch (Exception ex)
                        {
                            ex.ExtractDisplay("ELI32801");
                        }
                    });

                _staticTimeCompareRadioButton.CheckedChanged += ((sender, args) =>
                    {
                        _comparisonDateTimePicker.Enabled =
                            _compareRadioButton.Checked && _staticTimeCompareRadioButton.Checked;
                    });

                _relativeTimeCompareRadioButton.CheckedChanged += ((sender, args) =>
                    {
                        _numberAgoUpDown.Enabled =
                            _compareRadioButton.Checked && _relativeTimeCompareRadioButton.Checked;
                        _unitsAgoComboBox.Enabled =
                            _compareRadioButton.Checked && _relativeTimeCompareRadioButton.Checked;
                    });

                _relativeTimePeriodCompareRadioButton.CheckedChanged += ((sender, args) =>
                    {
                        try
                        {
                            // Allow the UI configuration phrase to read correctly whether comparing
                            // against a static time or a time period.
                            if (_relativeTimePeriodCompareRadioButton.Checked)
                            {
                                _timePeriodComboBox.Enabled = _compareRadioButton.Checked;
                                _comparisonComboBox.RenameEnumValue(DateComparisonOperator.Equal, "from");
                                _comparisonComboBox.RenameEnumValue(DateComparisonOperator.NotEqual, "not from");
                                _comparisonComboBox.RenameEnumValue(DateComparisonOperator.OnOrAfter, "from or after");
                                _comparisonComboBox.RenameEnumValue(DateComparisonOperator.OnOrBefore, "from or before");
                            }
                            else
                            {
                                _timePeriodComboBox.Enabled = false;
                                _comparisonComboBox.RenameEnumValue(DateComparisonOperator.Equal, "equal to");
                                _comparisonComboBox.RenameEnumValue(DateComparisonOperator.NotEqual, "not equal to");
                                _comparisonComboBox.RenameEnumValue(DateComparisonOperator.OnOrAfter, "equal to or after");
                                _comparisonComboBox.RenameEnumValue(DateComparisonOperator.OnOrBefore, "equal to or before");
                            }
                        }
                        catch (Exception ex)
                        {
                            ex.ExtractDisplay("ELI32802");
                        }
                    });

                _fileCompareRadioButton.CheckedChanged += ((sender, args) =>
                    {
                        bool enable = _compareRadioButton.Checked && _fileCompareRadioButton.Checked;
                        _file2PropertyComboBox.Enabled = enable;
                        _file2TextBox.Enabled = enable;
                        _file2PathTags.Enabled = enable;
                        _file2Browse.Enabled = enable;
                    });
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI32803");
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the settings.
        /// </summary>
        /// <value>
        /// The settings.
        /// </value>
        public FileTimestampCondition Settings { get; set; }

        #endregion Properties

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

                // Load the combo boxes with the readable values of their associates enums.
                _file1PropertyComboBox.InitializeWithReadableEnum<FileTimestampProperty>(false);
                _comparisonComboBox.InitializeWithReadableEnum<DateComparisonOperator>(false);
                _unitsAgoComboBox.InitializeWithReadableEnum<TimeSpanUnit>(false);
                _file2PropertyComboBox.InitializeWithReadableEnum<FileTimestampProperty>(false);
                _timePeriodComboBox.InitializeWithReadableEnum<TimePeriod>(false);

                // Apply Settings values to the UI.
                if (Settings != null)
                {
                    _metComboBox.Text = Settings.MetIfTrue ? "met" : "not met";
                    _file1TextBox.Text = Settings.File1Name;
                    _file1PropertyComboBox.SelectEnumValue(Settings.File1Property);
                    
                    _rangeStartDateTimePicker.Value = Settings.RangeStartDateTime;
                    _rangeEndDateTimePicker.Value = Settings.RangeEndDateTime;
                    _comparisonComboBox.SelectEnumValue(Settings.ComparisonOperator);
                    
                    switch (Settings.ComparisonMethod)
                    {
                        case TimestampComparisonMethod.RangeComparison:
                            _rangeRadioButton.Checked = true;
                            break;

                        case TimestampComparisonMethod.StaticComparison:
                            _compareRadioButton.Checked = true;
                            _staticTimeCompareRadioButton.Checked = true;
                            break;

                        case TimestampComparisonMethod.RelativeComparison:
                            _compareRadioButton.Checked = true;
                            _relativeTimeCompareRadioButton.Checked = true;
                            break;

                        case TimestampComparisonMethod.RelativeTimePeriod:
                            _compareRadioButton.Checked = true;
                            _relativeTimePeriodCompareRadioButton.Checked = true;
                            break;

                        case TimestampComparisonMethod.FileComparison:
                            _compareRadioButton.Checked = true;
                            _fileCompareRadioButton.Checked = true;
                            break;

                        default:
                            throw new ExtractException("ELI32804",
                                "Unexepected timestamp comparison method.");
                    }

                    _comparisonDateTimePicker.Value = Settings.ComparisonDateTime;
                    _timePeriodComboBox.SelectEnumValue(Settings.RelativeTimePeriod);
                    _numberAgoUpDown.Value = Settings.RelativeNumberAgo;
                    _unitsAgoComboBox.SelectEnumValue(Settings.RelativeTimeSpanUnit);
                    _file2PropertyComboBox.SelectEnumValue(Settings.File2Property);
                    _file2TextBox.Text = Settings.File2Name;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI32805");
            }
        }

        #region Event Handlers

        /// <summary>
        /// In the case that the OK button is clicked, validates the settings, applies them, and
        /// closes.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleOkButtonClick(object sender, EventArgs e)
        {
            try
            {
                // If there are invalid settings, prompt and return without closing.
                if (WarnIfInvalid())
                {
                    return;
                }

                // Apply the UI values to the Settings instance.
                Settings.MetIfTrue = _metComboBox.Text == "met";
                Settings.File1Name = _file1TextBox.Text;
                Settings.File1Property =
                    _file1PropertyComboBox.ToEnumValue<FileTimestampProperty>();

                Settings.ComparisonOperator =
                    _comparisonComboBox.ToEnumValue<DateComparisonOperator>();
                if (_rangeRadioButton.Checked)
                {
                    Settings.ComparisonMethod = TimestampComparisonMethod.RangeComparison;
                }
                else if (_staticTimeCompareRadioButton.Checked)
                {
                    Settings.ComparisonMethod = TimestampComparisonMethod.StaticComparison;
                }
                else if (_relativeTimeCompareRadioButton.Checked)
                {
                    Settings.ComparisonMethod = TimestampComparisonMethod.RelativeComparison;
                }
                else if (_relativeTimePeriodCompareRadioButton.Checked)
                {
                    Settings.ComparisonMethod = TimestampComparisonMethod.RelativeTimePeriod;
                }
                else if (_fileCompareRadioButton.Checked)
                {
                    Settings.ComparisonMethod = TimestampComparisonMethod.FileComparison;
                }
                else
                {
                    throw new ExtractException("ELI32807", "Unexpected timestamp comparison method.");
                }

                Settings.RangeStartDateTime = _rangeStartDateTimePicker.Value;
                Settings.RangeEndDateTime = _rangeEndDateTimePicker.Value;
                Settings.ComparisonDateTime = _comparisonDateTimePicker.Value;
                Settings.RelativeTimePeriod = _timePeriodComboBox.ToEnumValue<TimePeriod>();
                Settings.RelativeNumberAgo = (int)_numberAgoUpDown.Value;
                Settings.RelativeTimeSpanUnit = _unitsAgoComboBox.ToEnumValue<TimeSpanUnit>();
                Settings.File2Property = _file2PropertyComboBox.ToEnumValue<FileTimestampProperty>();
                Settings.File2Name = _file2TextBox.Text;

                DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI32808");
            }
        }

        #endregion Event Handlers

        #region Private Members

        /// <summary>
        /// Displays a warning message if the user specified settings are invalid.
        /// </summary>
        /// <returns><see langword="true"/> if the settings are invalid; <see langword="false"/> if
        /// the settings are valid.</returns>
        bool WarnIfInvalid()
        {
            ExtractException.Assert("ELI32809",
                "File timestamp condition settings have not been provided.", Settings != null);

            if (string.IsNullOrWhiteSpace(_file1TextBox.Text))
            {
                UtilityMethods.ShowMessageBox("You must specify a file.",
                    "Invalid configuration", true);
                _file1TextBox.Focus();

                return true;
            }


            if (_rangeRadioButton.Checked)
            {
                if (_rangeStartDateTimePicker.Value > _rangeEndDateTimePicker.Value)
                {
                    UtilityMethods.ShowMessageBox(
                        "The range end must not fall before the range start.",
                        "Invalid configuration", true);
                    _rangeStartDateTimePicker.Focus();

                    return true;
                }
            }
            else // If using a comparison
            {
                if (_comparisonComboBox.ToEnumValue<DateComparisonOperator>()
                    == DateComparisonOperator.After)
                {
                    if (_relativeTimeCompareRadioButton.Checked && _numberAgoUpDown.Value == 0)
                    {
                        UtilityMethods.ShowMessageBox("There will never be a valid timestamp after '0 " +
                            _unitsAgoComboBox.Text + " ago'. Please specify a valid condition.",
                            "Invalid configuration", true);
                        _unitsAgoComboBox.Focus();

                        return true;
                    }
                    else if (_relativeTimePeriodCompareRadioButton.Checked)
                    {
                        TimePeriod timePeriod = _timePeriodComboBox.ToEnumValue<TimePeriod>();
                        if (timePeriod == TimePeriod.ThisMinute ||
                            timePeriod == TimePeriod.ThisHour ||
                            timePeriod == TimePeriod.Today ||
                            timePeriod == TimePeriod.ThisWeek ||
                            timePeriod == TimePeriod.ThisMonth ||
                            timePeriod == TimePeriod.ThisYear)
                        {
                            UtilityMethods.ShowMessageBox("There will never be a valid timestamp after '" +
                                _timePeriodComboBox.Text + "'. Please specify a valid condition.",
                                "Invalid configuration", true);
                            _timePeriodComboBox.Focus();

                            return true;
                        }
                    }
                }

                if (_fileCompareRadioButton.Checked)
                {
                    if (string.IsNullOrWhiteSpace(_file2TextBox.Text))
                    {
                        UtilityMethods.ShowMessageBox(
                            "You must specify a file to compare against.",
                            "Invalid configuration", true);
                        _file2TextBox.Focus();

                        return true;
                    }
                }
            }

            return false;
        }

        #endregion Private Members
    }
}
