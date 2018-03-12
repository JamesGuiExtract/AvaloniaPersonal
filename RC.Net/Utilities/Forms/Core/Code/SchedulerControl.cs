using Extract.Licensing;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;

namespace Extract.Utilities.Forms
{
    public partial class SchedulerControl : UserControl
    {
        #region Constants

        /// <summary>
        /// Object name used in licensing calls
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(SchedulerControl).ToString();

        /// <summary>
        /// Dictionary with selections for the Duration Unit
        /// </summary>
        static readonly Dictionary<DateTimeUnit, string> _durationUnits = new Dictionary<DateTimeUnit, string>()
        {
            { DateTimeUnit.Minute, DateTimeUnit.Minute.ToString() },
            { DateTimeUnit.Hour, DateTimeUnit.Hour.ToString() },
            { DateTimeUnit.Day, DateTimeUnit.Day.ToString() },
            { DateTimeUnit.Week, DateTimeUnit.Week.ToString() }
        };

        #endregion Constants

        #region Constructors

        /// <summary>
        /// Initializes SchedulerControl
        /// </summary>
        public SchedulerControl()
            : base()
        {
            try
            {
                // Only validate the license at run time
                if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
                {
                    LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());
                }

                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects,
                    "ELI45623", _OBJECT_NAME);

                InitializeComponent();
                _recurrenceUnitComboBox.DataSource = _durationUnits.ToList();
                _recurrenceUnitComboBox.DisplayMember = "Value";
                _recurrenceUnitComboBox.ValueMember = "Key";
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45624");
            }
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Value for the control
        /// </summary>
        public ScheduledEvent Value
        {
            get
            {
                try
                {
                    return new ScheduledEvent()
                    {
                        Start = new DateTime(
                            _startDatePicker.Value.Year,
                            _startDatePicker.Value.Month,
                            _startDatePicker.Value.Day,
                            _startTimePicker.Value.Hour,
                            _startTimePicker.Value.Minute,
                            _startTimePicker.Value.Second
                         ),
                        End = _untilCheckBox.Checked ?
                             new DateTime(
                                 _endDatePicker.Value.Year,
                                 _endDatePicker.Value.Month,
                                 _endDatePicker.Value.Day,
                                 _endTimePicker.Value.Hour,
                                 _endTimePicker.Value.Minute,
                                 _endTimePicker.Value.Second
                             ) : (DateTime?)null,
                        RecurrenceUnit = _recurEveryRadioButton.Checked ? GetRecuranceUnit() : (DateTimeUnit?)null,
                        Duration = _scheduleDurationCheckBox.Checked ? GetDuration() : (TimeSpan?)null,
                        Enabled = true,
                        Exclusions = new List<ScheduledEvent>()
                    };
                }
                catch (Exception ex)
                {

                    throw ex.AsExtract("ELI45628");
                }
            }

            set
            {
                try
                {
                    if (value == null)
                    {
                        value = new ScheduledEvent();
                    }
                    _startDatePicker.Value = value.Start;
                    _startTimePicker.Value = value.Start;
                    if (value.End != null)
                    {
                        _endDatePicker.Value = (DateTime)value.End;
                        _endTimePicker.Value = (DateTime)value.End;
                        _untilCheckBox.Checked = true;
                    }
                    SetRecurring(value);
                    SetDuration(value);
                }
                catch (Exception ex)
                {

                    throw ex.AsExtract("ELI45629");
                }
            }
        }
        #endregion

        #region Helper Methods

        void SetDuration(ScheduledEvent value)
        {
            if (value.Duration != null)
            {
                _scheduleDurationCheckBox.Checked = true;
                TimeSpan ts = (TimeSpan)value.Duration;
                _durationDaysNumericUpDown.Value = (Decimal)ts.Days;
                _durationHoursNumericUpDown.Value = (Decimal)ts.Hours;
                _durationMinutesNumericUpDown.Value = (Decimal)ts.Minutes;
            }
            else
            {
                _scheduleDurationCheckBox.Checked = false;
            }
        }

        void SetRecurring(ScheduledEvent value)
        {
            bool recurring = value.RecurrenceUnit != null;

            _specifiedTimeRadioButton.Checked = !recurring;
            _recurEveryRadioButton.Checked = recurring;
            if (recurring)
            {
                _recurrenceUnitComboBox.SelectedValue = value.RecurrenceUnit;
            }
        }

        TimeSpan? GetDuration()
        {
            if (_scheduleDurationCheckBox.Checked)
            {
                return new TimeSpan(
                    (int)_durationDaysNumericUpDown.Value,
                    (int)_durationHoursNumericUpDown.Value,
                    (int)_durationMinutesNumericUpDown.Value,
                    0);
            }
            return (TimeSpan?)null;
        }

        DateTimeUnit? GetRecuranceUnit()
        {
            return _recurEveryRadioButton.Checked ?
                (DateTimeUnit)_recurrenceUnitComboBox.SelectedValue : (DateTimeUnit?)null;
        }

        #endregion

        #region Event handlers

        void HandleUntilCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            try
            {
                bool enable = ((CheckBox)sender).Checked;
                _endDatePicker.Enabled = enable;
                _endTimePicker.Enabled = enable;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45630");
            }
        }

        void HandleRecurEveryRadioButtonCheckedChanged(object sender, EventArgs e)
        {
            try
            {
                bool enable = ((RadioButton)sender).Checked;
                _recurrenceUnitComboBox.Enabled = enable;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45631");
            }
        }

        #endregion

        void HandleScheduleDurationCheckBoxCheckedChanged(object sender, EventArgs e)
        {
            try
            {
                bool enable = ((CheckBox)sender).Checked;
                _durationDaysNumericUpDown.Enabled = enable;
                _durationHoursNumericUpDown.Enabled = enable;
                _durationMinutesNumericUpDown.Enabled = enable;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45634");
            }
        }
    }
}
