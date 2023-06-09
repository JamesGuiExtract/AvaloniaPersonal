﻿using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Windows.Forms;

namespace Extract.Utilities.Forms
{
    /// <summary>
    /// An enhanced <see cref="TextBox"/> control that restricts the data entered to be
    /// numeric input
    /// </summary>
    public partial class NumericEntryTextBox : TextBox
    {
        #region Fields

        /// <summary>
        /// Indicates whether exceptions should be displayed in the key handler or just thrown.
        /// </summary>
        bool _displayExceptions;

        /// <summary>
        /// Indicates whether the control will allow the decimal point to be specified.
        /// </summary>
        bool _allowDecimal;

        /// <summary>
        /// Indicates whether the control will allow negative numbers.
        /// </summary>
        bool _allowNegative;

        /// <summary>
        /// The minimum value for the text control
        /// </summary>
        double _minimumValue = double.MinValue;

        /// <summary>
        /// The maximum value for the text control
        /// </summary>
        double _maximumValue = double.MaxValue;

        #endregion Fields

        #region Events

        /// <summary>
        /// Raised when the value entered into the control is outside the specified range.
        /// </summary>
        public event EventHandler<ValueOutOfRangeEventArgs> ValueOutOfRange;

        #endregion Events

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="NumericEntryTextBox"/> class.
        /// </summary>
        public NumericEntryTextBox() : this(false, true, true)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NumericEntryTextBox"/> class.
        /// </summary>
        /// <param name="allowDecimal">if set to <see langword="true"/> then decimal points
        /// are allowed.</param>
        /// <param name="displayExceptions">if set to <see langword="true"/> exceptions will
        /// be displayed in event handlers.</param>
        /// <param name="allowNegative">if set to <see langword="true"/> will allow negative
        /// values to be entered.</param>
        public NumericEntryTextBox(bool allowDecimal, bool displayExceptions, bool allowNegative)
        {
            try
            {
                InitializeComponent();
                _allowDecimal = allowDecimal;
                _displayExceptions = displayExceptions;
                _allowNegative = allowNegative;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI31049", ex);
            }
        }

        #endregion Constructor

        #region Overrides

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.KeyPress"/> event.
        /// </summary>
        /// <param name="e">A <see cref="T:System.Windows.Forms.KeyPressEventArgs"/> that contains the event data.</param>
        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            try
            {
                if (!KeyAllowed(e.KeyChar))
                {
                    // Set handled to true so that keystroke is ignored
                    e.Handled = true;
                }

                base.OnKeyPress(e);
            }
            catch (Exception ex)
            {
                var ee = ExtractException.AsExtractException("ELI31050", ex);
                if (_displayExceptions)
                {
                    ee.Display();
                }
                else
                {
                    throw ee;
                }
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.Leave"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
        protected override void OnLeave(EventArgs e)
        {
            try
            {
                base.OnLeave(e);

                if (!string.IsNullOrWhiteSpace(Text))
                {
                    // Get the current value
                    var value = double.Parse(Text, CultureInfo.CurrentCulture);

                    // Validate the range
                    if (value < _minimumValue || value > _maximumValue)
                    {
                        // Raise the value out of range event
                        OnValueOutOfRange(new ValueOutOfRangeEventArgs(value,
                            _minimumValue, _maximumValue));
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI32601");
            }
        }

        #endregion Overrides

        #region Methods

        /// <summary>
        /// Keys the allowed.
        /// </summary>
        /// <param name="key">The key.</param>
        /// <returns><see langword="true"/> if the specified key is allowed in the current control
        /// configuration.</returns>
        bool KeyAllowed(char key)
        {
            return char.IsControl(key)
                || char.IsNumber(key)
                || (_allowDecimal && key == '.' && !Text.Contains(".")
                || (_allowNegative && key == '-' && !Text.Contains("-") && (SelectionStart == 0 && SelectionLength == 0))
                );
        }

        /// <summary>
        /// Raises the <see cref="E:ValueOutOfRange"/> event.
        /// </summary>
        /// <param name="e">The <see cref="Extract.Utilities.ValueOutOfRangeEventArgs"/> instance containing the event data.</param>
        void OnValueOutOfRange(ValueOutOfRangeEventArgs e)
        {
            if (ValueOutOfRange != null)
            {
                ValueOutOfRange(this, e);
            }
        }

        #endregion Methods

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether exceptions should be displayed or thrown
        /// from the message handlers in the control.
        /// </summary>
        /// <value>
        /// 	<see langword="true"/> if exceptions should be displayed; otherwise, <see langword="false"/>.
        /// </value>
        [DefaultValue(true)]
        public bool DisplayExceptions
        {
            get
            {
                return _displayExceptions;
            }
            set
            {
                _displayExceptions = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this control supports decimal digits.
        /// </summary>
        /// <value>
        /// 	<see langword="true"/> if decimals are allowed; otherwise, <see langword="false"/>.
        /// </value>
        [DefaultValue(false)]
        public bool AllowDecimal
        {
            get
            {
                return _allowDecimal;
            }
            set
            {
                _allowDecimal = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this control supports negative numbers.
        /// </summary>
        /// <value>
        /// 	<see langword="true"/> if negative numbers are allowed; otherwise, <see langword="false"/>.
        /// </value>
        [DefaultValue(true)]
        public bool AllowNegative
        {
            get
            {
                return _allowNegative;
            }
            set
            {
                _allowNegative = value;
            }
        }
        /// <summary>
        /// Gets the <see cref="Control.Text"/> value as an <see cref="Int32"/>
        /// <para><b>Note:</b></para>
        /// Calls into <seealso cref="Int32.Parse(string, IFormatProvider)"/>,
        /// if <see cref="AllowDecimal"/> is <see langword="true"/> and the
        /// specified value is in R then the number will be rounded.
        /// </summary>
        /// <value>The int32 value.</value>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Int32 Int32Value
        {
            get
            {
                try
                {
                    Int32 val = 0;
                    if (!string.IsNullOrWhiteSpace(Text))
                    {
                        if (_allowDecimal && Text.Contains("."))
                        {
                            var temp = double.Parse(Text, CultureInfo.CurrentCulture);
                            val = (Int32)Math.Round(temp);
                        }
                        else
                        {
                            val = Int32.Parse(Text, CultureInfo.CurrentCulture);
                        }
                    }

                    return val;
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI31058", ex);
                }
            }
            set
            {
                var val = _allowNegative ? value : Math.Abs(value);
                Text = val.ToString("G", CultureInfo.CurrentCulture);
            }
        }

        /// <summary>
        /// Gets the <see cref="Control.Text"/> value as an <see cref="Int64"/>
        /// <para><b>Note:</b></para>
        /// Calls into <seealso cref="Int64.Parse(string, IFormatProvider)"/>,
        /// if <see cref="AllowDecimal"/> is <see langword="true"/> and the
        /// specified value is in R then the number will be rounded.
        /// </summary>
        /// <value>The int64 value.</value>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Int64 Int64Value
        {
            get
            {
                try
                {
                    Int64 val = 0;
                    if (!string.IsNullOrWhiteSpace(Text))
                    {
                        if (_allowDecimal && Text.Contains("."))
                        {
                            var temp = double.Parse(Text, CultureInfo.CurrentCulture);
                            val = (Int64)Math.Round(temp);
                        }
                        else
                        {
                            val = Int64.Parse(Text, CultureInfo.CurrentCulture);
                        }
                    }

                    return val;
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI32600", ex);
                }
            }
            set
            {
                var val = _allowNegative ? value : Math.Abs(value);
                Text = val.ToString("G", CultureInfo.CurrentCulture);
            }
        }

        /// <summary>
        /// Gets the <see cref="Control.Text"/> value as an <see cref="Double"/>
        /// <para><b>Note:</b></para>
        /// <seealso cref="Double.Parse(string, IFormatProvider)"/>
        /// </summary>
        /// <value>The double value.</value>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public double DoubleValue
        {
            get
            {
                try
                {
                    var val = 0.0;
                    if (!string.IsNullOrWhiteSpace(Text))
                    {
                        val = double.Parse(Text, CultureInfo.CurrentCulture);
                    }

                    return val;
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI31059", ex);
                }
            }
            set
            {
                try
                {
                    var val = _allowNegative ? value : Math.Abs(value);
                    if (!_allowDecimal)
                    {
                        int temp = (int)Math.Round(val);
                        Text = temp.ToString("G", CultureInfo.CurrentCulture);
                    }
                    else
                    {
                        Text = val.ToString("F", CultureInfo.CurrentCulture);
                    }
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI31056", ex);
                }
            }
        }

        /// <summary>
        /// Gets or sets the minimum value allowed in the control.
        /// </summary>
        /// <value>
        /// The minimum value allowed in the control.
        /// </value>
        [SuppressMessage("ExtractRules", "ES0001:PublicMethodsContainTryCatch")]
        public double MinimumValue
        {
            get
            {
                return _minimumValue;
            }
            set
            {
                if (value > _maximumValue)
                {
                    throw new ArgumentOutOfRangeException("value",
                        "Must be less than or equal to the maximum value.");
                }

                _minimumValue = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum value allowed in the control.
        /// </summary>
        /// <value>
        /// The maximum value allowed in the control.
        /// </value>
        [SuppressMessage("ExtractRules", "ES0001:PublicMethodsContainTryCatch")]
        public double MaximumValue
        {
            get
            {
                return _maximumValue;
            }
            set
            {
                if (value < _minimumValue)
                {
                    throw new ArgumentOutOfRangeException("value",
                        "Must be greater  than or equal to the minimum value.");
                }

                _maximumValue = value;
            }
        }

        #endregion Properties
    }
}
