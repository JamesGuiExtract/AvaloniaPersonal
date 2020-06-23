using Extract.Licensing;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;

namespace Extract.Reporting
{
    /// <summary>
    /// A user control for managing a <see cref="ValueListParameter"/>.
    /// </summary>
    public partial class ValueListParameterControl : UserControl, IExtractParameterControl
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        private static readonly string _OBJECT_NAME = typeof(ValueListParameterControl).ToString();

        #endregion Constants

        #region Fields

        /// <summary>
        /// The <see cref="ValueListParameter"/> associated with this control.
        /// </summary>
        ValueListParameter _valueListParameter;

        #endregion Fields

        #region Constructors

        /// <overload>Initializes a new <see cref="ValueListParameterControl"/> class.</overload>
        /// <summary>
        /// Initializes a new <see cref="ValueListParameterControl"/> class.
        /// </summary>
        public ValueListParameterControl()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new <see cref="ValueListParameterControl"/> class.
        /// </summary>
        /// <param name="valueListParameter">The <see cref="ValueListParameter"/>
        /// associated with this control.</param>
        public ValueListParameterControl(ValueListParameter valueListParameter)
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
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI23826",
					_OBJECT_NAME);

                InitializeComponent();

                if (valueListParameter != null)
                {
                    _valueListParameter = valueListParameter;
                    UpdateComboList();

                    _parameterName.Text = _valueListParameter.ParameterName;
                    _parameterValue.Text = _valueListParameter.ParameterValue;
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI23827", ex);
            }
        }

        #endregion Constructors

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

                if (_valueListParameter != null)
                {
                    // Set the drop down style based on whether other values are allowed
                    _parameterValue.DropDownStyle = _valueListParameter.AllowOtherValues ?
                        ComboBoxStyle.DropDown : ComboBoxStyle.DropDownList;

                    if (_parameterValue.Items.Count > 0)
                    {
                        // Attempt to select the default value (if there is one)
                        int index = -1;
                        if (!string.IsNullOrEmpty(_valueListParameter.ParameterValue))
                        {
                            index = _parameterValue.FindStringExact(_valueListParameter.ParameterValue);
                        }

                        _parameterValue.SelectedIndex = index != -1 ? index : 0;
                    }
                }
                else
                {
                    // Do not allow other values in drop down list since there is no
                    // value list parameter associated with the control at this time
                    _parameterValue.DropDownStyle = ComboBoxStyle.DropDownList;
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI23854", ex);
            }
        }

        #endregion Event Handlers

        #region Properties

        /// <summary>
        /// Gets or sets the current parameter value combo box.  If setting, will also
        /// update the underlying <see cref="Extract.ReportViewer.ValueListParameter.ParameterValue"/>.
        /// </summary>
        /// <returns>The current value in the parameter value combo box.</returns>
        /// <value>The current value in the parameter value combo box and also
        /// updates the underlying <see cref="Extract.ReportViewer.ValueListParameter.ParameterValue"/>.
        /// </value>
        public string ParameterValue
        {
            get
            {
                return _parameterValue.Text;
            }
            set
            {
                try
                {
                    // Check if the new value is contained in the list of values
                    if (!_parameterValue.Items.Contains(value))
                    {
                        if (_valueListParameter != null)
                        {
                            _valueListParameter.ParameterValue = value;
                            UpdateComboList();
                        }
                        else
                        {
                            _parameterValue.Items.Add(value);
                        }
                    }
                    else
                    {
                        if (_valueListParameter != null)
                        {
                            _valueListParameter.ParameterValue = value;
                        }
                    }

                    // Update the currently selected item
                    _parameterValue.SelectedIndex = _parameterValue.Items.IndexOf(value);
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI26507", ex);
                }
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="ValueListParameter"/> underlying this control.
        /// </summary>
        /// <returns>The <see cref="ValueListParameter"/> underlying the control.</returns>
        /// <value>The <see cref="ValueListParameter"/> underlying the control.</value>
        public ValueListParameter ValueListParameter
        {
            get
            {
                return _valueListParameter;
            }
            set
            {
                try
                {
                    _valueListParameter = value;

                    // Update the combo list
                    UpdateComboList();

                    // Set the appropriate parameter name
                    if (_valueListParameter != null)
                    {
                        _parameterName.Text = _valueListParameter.ParameterName;

                        // Adjust the drop down style
                        _parameterValue.DropDownStyle = _valueListParameter.AllowOtherValues ?
                            ComboBoxStyle.DropDown : ComboBoxStyle.DropDownList;
                    }
                    else
                    {
                        // No value list parameter associated with the control, reset items
                        // to default state
                        _parameterName.Text = "Parameter name";
                        _parameterValue.DropDownStyle = ComboBoxStyle.DropDownList;
                    }
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI26508", ex);
                }
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Updates the combo list with the value from the <see cref="ValueListParameter"/>.
        /// </summary>
        private void UpdateComboList()
        {
            // First clear the collection of items
            _parameterValue.Items.Clear();

            if (_valueListParameter != null)
            {
                // Now add the values to the collection
                _parameterValue.Items.AddRange(
                    new List<string>(_valueListParameter.Values).ToArray());
            }
        }

        #endregion Methods

        #region IExtractParameterControl Members

        /// <summary>
        /// Applies the current control value to the underlying <see cref="TextParameter"/>.
        /// </summary>
        public void Apply()
        {
            try
            {
                string parameterName = _parameterName.Text;

                ExtractException.Assert("ELI23849", "Cannot leave " + parameterName + " empty!",
                    !string.IsNullOrEmpty(_parameterValue.Text));

                if (_valueListParameter != null)
                {
                    _valueListParameter.ParameterValue = _parameterValue.Text;
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI23828", ex);
            }
        }

        #endregion
    }
}
