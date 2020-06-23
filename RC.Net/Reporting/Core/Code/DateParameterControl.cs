using Extract.Licensing;
using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms;

namespace Extract.Reporting
{
    /// <summary>
    /// A user control for managing a <see cref="DateParameter"/>.
    /// </summary>
    public partial class DateParameterControl : UserControl, IExtractParameterControl
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        private static readonly string _OBJECT_NAME = typeof(DateParameterControl).ToString();

        #endregion Constants

        #region Fields

        /// <summary>
        /// The <see cref="DateParameter"/> associated with this control.
        /// </summary>
        DateParameter _dateParameter;

        #endregion Fields

        #region Constructors

        /// <overloads>Initializes a new <see cref="DateParameterControl"/> class.</overloads>
        /// <summary>
        /// Initializes a new <see cref="DateParameterControl"/> class.
        /// </summary>
        public DateParameterControl()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new <see cref="DateParameterControl"/> class.
        /// </summary>
        /// <param name="dateParameter">The <see cref="DateParameter"/> that this
        /// control is associated with.</param>
        public DateParameterControl(DateParameter dateParameter)
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
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI23802",
					_OBJECT_NAME);

                InitializeComponent();

                if (dateParameter != null)
                {
                    _dateParameter = dateParameter;
                    _parameterName.Text = _dateParameter.ParameterName;
                    _parameterValue.Value = _dateParameter.ParameterValue;
                    if (!_dateParameter.ShowTime)
                    {
                        _parameterValue.CustomFormat =
                            _parameterValue.CustomFormat.Replace(" HH:mm", "");
                    }
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI23803", ex);
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the current parameter value date picker.  If setting, will also
        /// update the underlying <see cref="DateParameter"/>.
        /// </summary>
        /// <returns>The current value in the parameter value date picker.</returns>
        /// <value>The current value in the parameter value date picker and also
        /// updates the underlying <see cref="DateParameter"/>.
        /// </value>
        // This property is just setting a string, it should never throw an exception
        [SuppressMessage("ExtractRules", "ES0001:PublicMethodsContainTryCatch")]
        public DateTime ParameterValue
        {
            get
            {
                try
                {
                    return _parameterValue.Value;
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI23696", ex);
                }
            }
            set
            {
                _parameterValue.Value = value;

                if (_dateParameter != null)
                {
                    _dateParameter.ParameterValue = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="DateParameter"/> underlying this control.
        /// </summary>
        /// <returns>The <see cref="DateParameter"/> underlying the control.</returns>
        /// <value>The <see cref="DateParameter"/> underlying the control.</value>
        public DateParameter DateParameter
        {
            get
            {
                return _dateParameter;
            }
            set
            {
                try
                {
                    _dateParameter = value;

                    if (_dateParameter != null)
                    {
                        _parameterName.Text = _dateParameter.ParameterName;
                        _parameterValue.Value = _dateParameter.ParameterValue;
                    }
                    else
                    {
                        _parameterName.Text = "Parameter name";
                        _parameterValue.Value = DateTime.Now;
                    }
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI26501", ex);
                }
            }
        }

        #endregion Properties

        #region Event Handlers

        /// <summary>
        /// Handles the <see cref="DateTimePicker.ValueChanged"/>
        /// </summary>
        /// <param name="sender">The object which sent the event.</param>
        /// <param name="e">The data associated with the event.</param>
        private void HandleValueChanged(object sender, EventArgs e)
        {
            try
            {
                if (_dateParameter != null)
                {
                    _dateParameter.ParameterValue = _parameterValue.Value;
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI23697", ex);
                ee.AddDebugData("Event Arguments", e, false);
                ee.Display();
            }
        }

        #endregion Event Handlers

        #region IExtractParameterControl Members

        /// <summary>
        /// Applies the current control value to the underlying <see cref="DateParameter"/>.
        /// </summary>
        public void Apply()
        {
            try
            {
                if (_dateParameter != null)
                {
                    _dateParameter.ParameterValue = _parameterValue.Value;
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI23804", ex);
            }
        }

        #endregion
    }
}
