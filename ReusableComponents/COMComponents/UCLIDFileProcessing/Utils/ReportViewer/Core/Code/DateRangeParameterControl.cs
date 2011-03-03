using Extract.Licensing;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Data;
using System.Globalization;
using System.Text;
using System.Windows.Forms;

namespace Extract.ReportViewer
{
    /// <summary>
    /// A user control for managing a <see cref="DateRangeParameter"/>.
    /// </summary>
    public partial class DateRangeParameterControl : UserControl, IExtractParameterControl
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        private static readonly string _OBJECT_NAME = typeof(DateRangeParameterControl).ToString();

        #endregion Constants

        #region Fields

        /// <summary>
        /// The <see cref="DateRangeParameter"/> associated with this control.
        /// </summary>
        DateRangeParameter _dateRangeParameter;

        #endregion Fields

        #region Constructors

        /// <overloads>Initializes a new <see cref="DateRangeParameterControl"/> class.</overloads>
        /// <summary>
        /// Initializes a new <see cref="DateRangeParameterControl"/> class.
        /// </summary>
        public DateRangeParameterControl()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new <see cref="DateRangeParameterControl"/> class.
        /// </summary>
        /// <param name="dateRangeParameter">The <see cref="DateRangeParameter"/>
        /// associated with this control.</param>
        public DateRangeParameterControl(DateRangeParameter dateRangeParameter)
        {
            try
            {
                // Load licenses if in design mode
                if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
                {
                    // Load the license files from folder
                    LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());
                }

                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI23805",
                    _OBJECT_NAME);

                InitializeComponent();

                // Fill the range combo box with the enum values as human readable strings
                _rangeValues.Items.AddRange(DateRangeValueTypeHelper.GetHumanReadableStrings());

                if (dateRangeParameter != null)
                {
                    _dateRangeParameter = dateRangeParameter;
                    _parameterName.Text = _dateRangeParameter.ParameterName;

                    _rangeValues.SelectedIndex =
                        _rangeValues.Items.IndexOf(
                        DateRangeValueTypeHelper.GetHumanReadableString(_dateRangeParameter.ParameterValue));
                    _parameterValueBegin.Value = _dateRangeParameter.Minimum;
                    _parameterValueEnd.Value = _dateRangeParameter.Maximum;

                    // Enable/disable the date time picker based on whether the range is
                    // custom or not
                    bool custom = _dateRangeParameter.ParameterValue == DateRangeValue.Custom;
                    _parameterValueBegin.Enabled = custom;
                    _parameterValueEnd.Enabled = custom;
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI23806", ex);
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the begin date in the begin date picker control.  If setting will also
        /// update the underlying <see cref="DateRangeParameter"/> value.
        /// </summary>
        /// <returns>The value in the begin date picker control.</returns>
        /// <value>The value in the begin date picker control. Will also update
        /// the underlying <see cref="DateRangeParameter"/> value.</value>
        // This property is just setting a date, it should never throw an exception
        [SuppressMessage("ExtractRules", "ES0001:PublicMethodsContainTryCatch")]
        public DateTime ParameterValueBegin
        {
            get
            {
                try
                {
                    return _parameterValueBegin.Value;
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI23698", ex);
                }
            }
            set
            {
                _parameterValueBegin.Value = value;

                if (_dateRangeParameter != null)
                {
                    _dateRangeParameter.Minimum = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the end date in the begin date picker control.  If setting will also
        /// update the underlying <see cref="DateRangeParameter"/> value.
        /// </summary>
        /// <returns>The value in the end date picker control.</returns>
        /// <value>The value in the end date picker control. Will also update
        /// the underlying <see cref="DateRangeParameter"/> value.</value>
        // This property is just setting a date, it should never throw an exception
        [SuppressMessage("ExtractRules", "ES0001:PublicMethodsContainTryCatch")]
        public DateTime ParameterValueEnd
        {
            get
            {
                try
                {
                    return _parameterValueEnd.Value;
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI23699", ex);
                }
            }
            set
            {
                _parameterValueEnd.Value = value;

                if (_dateRangeParameter != null)
                {
                    _dateRangeParameter.Maximum = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="DateRangeParameter"/> underlying this control.
        /// </summary>
        /// <returns>The <see cref="DateRangeParameter"/> underlying the control.</returns>
        /// <value>The <see cref="DateRangeParameter"/> underlying the control.</value>
        public DateRangeParameter DateRangeParameter
        {
            get
            {
                return _dateRangeParameter;
            }
            set
            {
                try
                {
                    _dateRangeParameter = value;

                    if (_dateRangeParameter != null)
                    {
                        _parameterName.Text = _dateRangeParameter.ParameterName;
                        _rangeValues.SelectedIndex =
                            _rangeValues.Items.IndexOf(
                            DateRangeValueTypeHelper.GetHumanReadableString(
                            _dateRangeParameter.ParameterValue));

                        _parameterValueBegin.Value = _dateRangeParameter.Minimum;
                        _parameterValueEnd.Value = _dateRangeParameter.Maximum;

                        // Enable/disable the date time picker based on whether the range is
                        // custom or not
                        bool custom = _dateRangeParameter.ParameterValue == DateRangeValue.Custom;
                        _parameterValueBegin.Enabled = custom;
                        _parameterValueEnd.Enabled = custom;
                    }
                    else
                    {
                        _parameterName.Text = "Parameter name";
                        _parameterValueBegin.Value = DateTime.Now;
                        _parameterValueEnd.Value = _parameterValueBegin.Value;
                    }
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI26502", ex);
                }
            }
        }

        #endregion Properties

        #region Event Handlers

        /// <summary>
        /// Handles the <see cref="Form.Load"/> event.
        /// </summary>
        /// <param name="e">The data associated with the event.</param>
        protected override void OnLoad(EventArgs e)
        {
            try
            {
                base.OnLoad(e);

                // Get the enum value from the control
                DateRangeValue range = DateRangeValueTypeHelper.ParseString(_rangeValues.Text);
                bool custom = range == DateRangeValue.Custom;

                _parameterValueBegin.Enabled = custom;
                _parameterValueEnd.Enabled = custom;
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI23807", ex);
            }

        }
        /// <summary>
        /// Handles the <see cref="DateTimePicker.ValueChanged"/> event for the
        /// beginning date control.
        /// </summary>
        /// <param name="sender">The object which sent the event.</param>
        /// <param name="e">The data associated with the event.</param>
        private void HandleBeginDateValueChanged(object sender, EventArgs e)
        {
            try
            {
                // DNRCAU #531 - 0 the seconds
                var dateTime = _parameterValueBegin.Value;
                _dateRangeParameter.Minimum = dateTime.AddSeconds(-dateTime.Second);
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI23700", ex);
                ee.AddDebugData("Event Arguments", e, false);
                ee.AddDebugData("Begin Text", _parameterValueBegin.Text ?? "null", false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the <see cref="DateTimePicker.ValueChanged"/> event for the
        /// ending date control.
        /// </summary>
        /// <param name="sender">The object which sent the event.</param>
        /// <param name="e">The data associated with the event.</param>
        private void HandleEndDateValueChanged(object sender, EventArgs e)
        {
            try
            {
                // DNRCAU #531 - 0 the seconds
                var dateTime = _parameterValueEnd.Value;
                _dateRangeParameter.Maximum = dateTime.AddSeconds(-dateTime.Second);
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI23701", ex);
                ee.AddDebugData("Event Arguments", e, false);
                ee.AddDebugData("End Text", _parameterValueEnd.Text ?? "null", false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the <see cref="ComboBox.SelectedIndexChanged"/> event.
        /// </summary>
        /// <param name="sender">The object which sent the event.</param>
        /// <param name="e">The data associated with the event.</param>
        private void HandleRangeComboChanged(object sender, EventArgs e)
        {
            try
            {
                // Get the enum value from the control
                DateRangeValue newRange = DateRangeValueTypeHelper.ParseString(_rangeValues.Text);

                // Update the date range parameter with the new enum value
                _dateRangeParameter.ParameterValue = newRange;

                // Check whether the new range is custom
                bool custom = newRange == DateRangeValue.Custom;

                // Enable/disable the date time pickers based on whether the user specified custom
                _parameterValueBegin.Enabled = custom;
                _parameterValueEnd.Enabled = custom;

                // Set the date/time display in the date time values
                _parameterValueBegin.Value = _dateRangeParameter.Minimum;
                _parameterValueEnd.Value = _dateRangeParameter.Maximum;
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI23702", ex);
                ee.AddDebugData("Event Arguments", e, false);
                ee.Display();
            }
        }

        #endregion Event Handlers

        #region IExtractParameterControl Members

        /// <summary>
        /// Applies the current control value to the underlying <see cref="DateRangeParameter"/>.
        /// </summary>
        public void Apply()
        {
            try
            {
                if (_dateRangeParameter != null)
                {
                    // Get the enum value from the control
                    DateRangeValue range = DateRangeValueTypeHelper.ParseString(_rangeValues.Text);

                    // Set the range
                    _dateRangeParameter.ParameterValue = range;

                    // Check for custom
                    if (range == DateRangeValue.Custom)
                    {
                        _dateRangeParameter.Minimum = _parameterValueBegin.Value;
                        _dateRangeParameter.Maximum = _parameterValueEnd.Value;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI23808", ex);
            }
        }

        #endregion
    }
}
