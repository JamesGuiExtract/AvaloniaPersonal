using Extract.Licensing;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;

namespace Extract.Reporting
{
    /// <summary>
    /// A user control for managing a <see cref="ValueListParameter"/>.
    /// </summary>
    public partial class MultipleSelectValueListParameterControl : UserControl, IExtractParameterControl
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        private static readonly string _OBJECT_NAME = typeof(MultipleSelectValueListParameterControl).ToString();

        #endregion Constants

        #region Fields

        /// <summary>
        /// The <see cref="ValueListParameter"/> associated with this control.
        /// </summary>
        ValueListParameter _valueListParameter;

        #endregion Fields

        #region Constructors

        /// <overload>Initializes a new <see cref="MultipleSelectValueListParameterControl"/> class.</overload>
        /// <summary>
        /// Initializes a new <see cref="MultipleSelectValueListParameterControl"/> class.
        /// </summary>
        public MultipleSelectValueListParameterControl()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new <see cref="MultipleSelectValueListParameterControl"/> class.
        /// </summary>
        /// <param name="valueListParameter">The <see cref="ValueListParameter"/>
        /// associated with this control.</param>
        public MultipleSelectValueListParameterControl(ValueListParameter valueListParameter)
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
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI50096",
					_OBJECT_NAME);

                // Verify that MultiSelect is true
                if (!valueListParameter.MultipleSelect)
                {
                    ExtractException ee = new ExtractException("ELI50097", "Not a MultiSelect ValueListParameter.");
                    throw ee;
                }

                InitializeComponent();

                if (valueListParameter != null)
                {
                    _valueListParameter = valueListParameter;
                    UpdateListBox();

                    _parameterName.Text = _valueListParameter.ParameterName;
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI50098", ex);
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
                    CheckSelectedValues(_valueListParameter.ParameterValue);
                }

            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI50099", ex);
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
                return string.Join(",", parameterValuesListBox.CheckedItems.OfType<string>());
            }
            set
            {
                try
                {
                    VerifySelectedItemsInList(value);

                    // Update the currently selected item
                    CheckSelectedValues(value);
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI50100", ex);
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
                    UpdateListBox();

                    // Set the appropriate parameter name
                    if (_valueListParameter != null)
                    {
                        _parameterName.Text = _valueListParameter.ParameterName;
                        CheckSelectedValues(_valueListParameter.ParameterValue);
                    }
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI50101", ex);
                }
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Updates the combo list with the value from the <see cref="ValueListParameter"/>.
        /// </summary>
        private void UpdateListBox()
        {
            // First clear the collection of items
            parameterValuesListBox.Items.Clear();

            if (_valueListParameter != null)
            {
                // Now add the values to the collection
                parameterValuesListBox.Items.AddRange(
                    new List<string>(_valueListParameter.Values).ToArray());
            }
        }

        private void CheckSelectedValues(string selections)
        {
            if (string.IsNullOrWhiteSpace(selections))
                return;
            var selectedItems = selections
                .Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .ToList();
            foreach (var s in selectedItems)
            {
                parameterValuesListBox.SetItemChecked(parameterValuesListBox.FindStringExact(s), true);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="selections">String containing , separated list of selections</param>
        private void VerifySelectedItemsInList(string selections)
        {
            var selectedItems = selections.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .ToList();
            var missingItems = selectedItems.Except(_valueListParameter.Values).ToList();
            if (missingItems.Count > 0)
            {
                ExtractException ee = new ExtractException("ELI50104",
                                                           "All values must be in value list!");
                ee.AddDebugData("MissingItems", string.Join(",", missingItems));
                throw ee;
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

                if (_valueListParameter != null)
                {
                    _valueListParameter.ParameterValue = ParameterValue;
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI50103", ex);
            }
        }

        #endregion
    }
}
