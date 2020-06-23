using Extract.Licensing;
using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Windows.Forms;

namespace Extract.Reporting
{
    /// <summary>
    /// A user control for managing a <see cref="NumberParameter"/>.
    /// </summary>
    public partial class NumberParameterControl : UserControl, IExtractParameterControl
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        private static readonly string _OBJECT_NAME = typeof(NumberParameterControl).ToString();

        #endregion Constants

        #region Fields

        /// <summary>
        /// Holds the <see cref="NumberParameter"/> associated with this control.
        /// </summary>
        NumberParameter _numberParameter;

        #endregion Fields

        #region Constructors

        /// <overloads>Initializes a new <see cref="NumberParameterControl"/> class.</overloads>
        /// <summary>
        /// Initializes a new <see cref="NumberParameterControl"/> class.
        /// </summary>
        public NumberParameterControl()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new <see cref="NumberParameterControl"/> class.
        /// </summary>
        /// <param name="numberParameter">The <see cref="NumberParameter"/> that this
        /// control is associated with.</param>
        public NumberParameterControl(NumberParameter numberParameter)
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
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI23818",
					_OBJECT_NAME);

                InitializeComponent();

                if (numberParameter != null)
                {
                    _numberParameter = numberParameter;
                    _parameterName.Text = _numberParameter.ParameterName;
                    _parameterValue.Text =
                        _numberParameter.ParameterValue.ToString(CultureInfo.CurrentCulture);
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI23819", ex);
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the current parameter value edit box.  If setting, will also
        /// update the underlying <see cref="NumberParameter"/>.
        /// </summary>
        /// <returns>The current value in the parameter value edit box.</returns>
        /// <value>The current value in the parameter value edit box and also
        /// updates the underlying <see cref="NumberParameter"/>.
        /// </value>
        // This property is just setting a double, it should never throw an exception
        [SuppressMessage("ExtractRules", "ES0001:PublicMethodsContainTryCatch")]
        public double ParameterValue
        {
            get
            {
                try
                {
                    return Convert.ToDouble(_parameterValue.Text, CultureInfo.CurrentCulture);
                }
                catch(Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI23733", ex);
                }
            }
            set
            {
                _parameterValue.Text = value.ToString(CultureInfo.CurrentCulture);

                if (_numberParameter != null)
                {
                    _numberParameter.ParameterValue = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="NumberParameter"/> underlying this control.
        /// </summary>
        /// <returns>The <see cref="NumberParameter"/> underlying the control.</returns>
        /// <value>The <see cref="NumberParameter"/> underlying the control.</value>
        public NumberParameter NumberParameter
        {
            get
            {
                return _numberParameter;
            }
            set
            {
                try
                {
                    _numberParameter = value;

                    // Check if the value is not null
                    if (_numberParameter != null)
                    {
                        // Update the label and edit box
                        _parameterName.Text = _numberParameter.ParameterName;
                        _parameterValue.Text =
                            _numberParameter.ParameterValue.ToString(CultureInfo.CurrentCulture);
                    }
                    else
                    {
                        // Update the label and edit box
                        _parameterName.Text = "Parameter name";
                        _parameterValue.Text = "";
                    }
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI26504", ex);
                }
            }
        }

        #endregion Properties

        #region Event Handlers

        /// <summary>
        /// Handles the <see cref="Control.KeyDown"/> event for the text box. This is
        /// used to restrict the text box entries to numbers (includes '-' and '.').
        /// </summary>
        /// <param name="sender">The object which sent the event.</param>
        /// <param name="e">The <see cref="KeyEventArgs"/> data associated with the event.</param>
        private void HandleKeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                int keyValue = e.KeyValue;
                Keys keyCode = e.KeyCode;

                // Allow non-alpha characters such as space and backspace, numbers, '-' and '.'
                if (IsNonPrintableKeypress(keyValue) || IsDigitKeypress(keyValue)
                    || keyCode == Keys.Subtract || keyCode == Keys.OemMinus
                    || keyCode == Keys.Decimal || keyCode == Keys.OemPeriod)
                {
                    // Allow the key to be pressed
                }
                else
                {
                    // Suppress the key press
                    e.SuppressKeyPress = true;
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI23734", ex);
                ee.AddDebugData("Event Arguments", e, false);
                ee.Display();
            }
        }

        #endregion Event Handlers

        #region Methods

        /// <summary>
        /// Checks whether the specified key value is one of the number keys.
        /// </summary>
        /// <param name="keyValue">The value of the key that was pressed.</param>
        /// <returns><see langword="true"/> then the key value is one of the digit
        /// keys; <see langword="false"/> otherwise.</returns>
        private static bool IsDigitKeypress(int keyValue)
        {
            // Check if the key value is either 0-9 on the main keyboard (48-57)
            // or 0-9 on the number pad (96-105)
            bool digitPress = (keyValue > 47 && keyValue < 58)
                || (keyValue > 95 && keyValue < 106);

            return digitPress;
        }

        /// <summary>
        /// Checks whether the specified key value is one of the number keys.
        /// </summary>
        /// <param name="keyValue">The value of the key that was pressed.</param>
        /// <returns><see langword="true"/> then the key value is one of the digit
        /// keys; <see langword="false"/> otherwise.</returns>
        private static bool IsNonPrintableKeypress(int keyValue)
        {
            // Check if the key press is a non printable character such as space/backspace, etc.
            // (0-31)
            bool digitPress = (keyValue > 0 && keyValue < 31);

            return digitPress;
        }

        #endregion Methods

        #region IExtractParameterControl Members

        /// <summary>
        /// Applies the current control value to the underlying <see cref="NumberParameter"/>.
        /// </summary>
        public void Apply()
        {
            try
            {
                string parameterName = _parameterName.Text;

                ExtractException.Assert("ELI23848", "Cannot leave " + parameterName + " empty!",
                    !string.IsNullOrEmpty(_parameterValue.Text));

                if (_numberParameter != null)
                {
                    _numberParameter.SetValueFromString(_parameterValue.Text);
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI23820", ex);
            }
        }

        #endregion
    }
}
