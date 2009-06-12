using Extract.Licensing;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using UCLID_AFCORELib;
using UCLID_RASTERANDOCRMGMTLib;

namespace Extract.DataEntry
{    
    /// <summary>
    /// A combo box cell for use in a <see cref="DataEntryTableBase"/>-derived class.
    /// </summary>
    public class DataEntryComboBoxCell : DataGridViewComboBoxCell, IDataEntryTableCell
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        private static readonly string _OBJECT_NAME = typeof(DataEntryComboBoxCell).ToString();

        #endregion Constants

        #region Fields

        /// <summary>
        /// The attribute whose value is associated with this cell.
        /// </summary>
        private IAttribute _attribute;

        /// <summary>
        /// The object that is to provided validation of this cell's data.
        /// </summary>
        private DataEntryValidator _validator;

        /// <summary>
        /// The <see cref="DataEntryTable"/> the cell is associated with.
        /// </summary>
        private DataEntryTableBase _dataEntryTable;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initiates a new <see cref="DataEntryComboBoxCell"/> instance.
        /// </summary>
        public DataEntryComboBoxCell()
            : base()
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
                LicenseUtilities.ValidateLicense(LicenseIdName.FlexIndexCoreObjects, "ELI24488",
                    _OBJECT_NAME);

                _dataEntryTable = base.DataGridView as DataEntryTableBase;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI24489", ex);
            }
        }

        #endregion Constructors

        #region IDataEntryTableCell Members

        /// <summary>
        /// Gets or sets the <see cref="IAttribute"/> whose value is associated with this cell.
        /// </summary>
        /// <value>The <see cref="IAttribute"/> whose value is associated with this cell.</value>
        /// <returns>The <see cref="IAttribute"/> whose value is associated with this cell.
        /// </returns>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IAttribute Attribute
        {
            get
            {
                return _attribute;
            }

            set
            {
                _attribute = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="DataEntryValidator"/> to be used to validate this
        /// cell's data.
        /// </summary>
        /// <value>The <see cref="DataEntryValidator"/> to be used to validate this cell's
        /// data.</value>
        /// <returns>The <see cref="DataEntryValidator"/> used to validate this cell's data.
        /// </returns>
        public DataEntryValidator Validator
        {
            get
            {
                return _validator;
            }

            set
            {
                try
                {
                    if (value != _validator)
                    {
                        if (_validator != null)
                        {
                            _validator.ValidationListChanged -= HandleValidationListChanged;
                        }

                        _validator = value;

                        _validator.ValidationListChanged += HandleValidationListChanged;
                    }
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI26208", ex);
                }
            }
        }

        /// <summary>
        /// Provides access to the object as an <see cref="DataGridViewCell"/>.
        /// </summary>
        /// <returns>The object as an <see cref="DataGridViewCell"/>.</returns>
        public DataGridViewCell AsDataGridViewCell
        {
            get
            {
                return this;
            }
        }

        /// <summary>
        /// Raised when the spatial info associated with the <see cref="DataEntryComboBoxCell"/>'s
        /// <see cref="IAttribute"/> has changed.
        /// </summary>
        public event EventHandler<CellSpatialInfoChangedEventArgs> CellSpatialInfoChanged;

        #endregion IDataEntryTableCell Members

        #region Overrides

        /// <summary>
        /// Creates a copy of the <see cref="DataEntryComboBoxCell"/> instance.
        /// </summary>
        /// <returns>A copy of the <see cref="DataEntryComboBoxCell"/> instance.</returns>
        public override object Clone()
        {
            try
            {
                DataEntryComboBoxCell clone = (DataEntryComboBoxCell)base.Clone();

                if (_validator != null)
                {
                    clone.Validator = (DataEntryValidator)_validator.Clone();
                }

                return clone;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI26207", ex);
            }
        }

        /// <summary>
        /// Called when the <see cref="DataGridView"/> property of the cell changes.
        /// </summary>
        protected override void OnDataGridViewChanged()
        {
            try
            {
                base.OnDataGridViewChanged();

                _dataEntryTable = base.DataGridView as DataEntryTableBase;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI25669", ex);
            }
        }

        /// <summary>
        /// Gets the value of the cell.
        /// </summary>
        /// <param name="rowIndex">The index of the cell's parent row.</param>
        /// <returns>The cell's value.</returns>
        protected override object GetValue(int rowIndex)
        {
            try
            {
                // Replace CRLFs in the strings used for display with no break spaces to prevent
                // the unprintable "boxes" from appearing.
                object value = base.GetValue(rowIndex);
                string stringValue = (value == null) ? "" : value.ToString();
                return stringValue.Replace("\r\n", DataEntryMethods._CRLF_REPLACEMENT);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI26013", ex);
            }
        }


        /// <summary>
        /// Sets the value of the cell.
        /// </summary>
        /// <param name="rowIndex">The index of the cell's parent row.</param>
        /// <param name="value">The cell value to set. This value can be an <see cref="IAttribute"/>,
        /// a <see cref="SpatialString"/> or the value's <see cref="Object.ToString"/> value will
        /// be used.</param>
        /// <returns><see langword="true"/> if the value has been set; otherwise, 
        /// <see langword="false"/></returns>
        protected override bool SetValue(int rowIndex, object value)
        {
            bool result;

            try
            {
                bool spatialInfoChanged = false;

                // Test to see if the provided value is an attribute...
                IAttribute attribute = value as IAttribute;

                if (attribute != null)
                {
                    // If so, use the provided attribute.
                    _attribute = attribute;

                    if (attribute.Value.HasSpatialInfo())
                    {
                        spatialInfoChanged = true;
                    }
                }
                else
                {
                    // Otherwise, test to see if the provided value is a spatial string...
                    SpatialString spatialString = value as SpatialString;

                    if (spatialString != null)
                    {
                        if (_attribute != null)
                        {
                            // If so, use it as the cell's value.
                            AttributeStatusInfo.SetValue(_attribute, spatialString, true, true);

                            spatialInfoChanged = true;
                        }
                        else
                        {
                            // If the associated attribute has not been set, just use the text of 
                            // the attribute
                            value = spatialString.String;
                        }
                    }
                    else if (_attribute != null)
                    {
                        // If the provided value is null, convert it to an empty string.
                        string stringValue = (value == null) ? "" : value.ToString();

                        // Add CRLFs back so that the replacement value used for display is never
                        // actually stored.
                        stringValue =
                            stringValue.Replace(DataEntryMethods._CRLF_REPLACEMENT, "\r\n");

                        // Otherwise, used the provided value as a string to replace the text of
                        // the associated attribute.
                        AttributeStatusInfo.SetValue(_attribute, stringValue, true, true);
                    }
                }

                // Using the stringized version of the provided value, set the base classes' value.
                if (_attribute != null)
                {
                    result = base.SetValue(rowIndex, _attribute.Value.String);

                    if (base.DataGridView.Visible)
                    {
                        // [DataEntry:243]
                        // If an attribute is being mapped into a cell, it is viewable.  Set viewable
                        // now rather than later, otherwise HasBeenViewed will return false on the basis
                        // that the attribute is not viewable.
                        AttributeStatusInfo.MarkAsViewable(_attribute, true);
                    }

                    _dataEntryTable.UpdateCellStyle((IDataEntryTableCell)this);
                }
                else
                {
                    result = base.SetValue(rowIndex, value);
                }

                // If a value was applied, validate it.
                if (result)
                {
                    DataEntryTableBase.ValidateCell(this, false);
                }

                if (spatialInfoChanged)
                {
                    // If spatial info has been set, a hint is no longer needed.
                    AttributeStatusInfo.SetHintType(_attribute, HintType.None);

                    // Consider the attribute un-accepted after a swipe.
                    AttributeStatusInfo.AcceptValue(_attribute, false);

                    OnCellSpatialInfoChanged(_attribute);
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI24288", ex);
            }

            return result;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the cell's value can be edited.
        /// </summary>
        /// <value><see langword="true"/> to make the cell's value read-only;
        /// <see langword="false"/> to enable editing.</value>
        /// <returns><see langword="true"/> if the cell's value is read-only;
        /// <see langword="false"/> if it is editable.</returns>
        public override bool ReadOnly
        {
            get
            {
                return base.ReadOnly;
            }
            set
            {
                try
                {
                    base.ReadOnly = value;

                    // [DataEntry:176]
                    // If the control cell is disabled (such as when a document is closed), ensure
                    // there are no validation warnings displayed which the user can't do anything
                    // about.
                    if (value)
                    {
                        base.ErrorText = "";
                    }
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI25988", ex);
                }
            }
        }

        #endregion Overrides

        #region Private Members

        /// <summary>
        /// Raises the <see cref="IDataEntryTableCell.CellSpatialInfoChanged"/> event
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> associated with the event.</param>
        private void OnCellSpatialInfoChanged(IAttribute attribute)
        {
            if (this.CellSpatialInfoChanged != null)
            {
                CellSpatialInfoChanged(this, new CellSpatialInfoChangedEventArgs(attribute));
            }
        }

        /// <summary>
        /// Handles the case that the validation list was changed so that the combo box list items
        /// can be updated to reflect the new list.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        private void HandleValidationListChanged(object sender, EventArgs e)
        {
            try
            {
                // Reseting the item list will clear the value. Preserve the original
                // value.
                string originalValue = base.Value.ToString();

                string[] validationListValues = _validator.GetValidationListValues();

                base.Items.Clear();

                if (validationListValues != null)
                {
                    base.Items.AddRange(validationListValues);

                    // Restore the original value
                    base.Value = originalValue;
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI26209", ex);
                ee.AddDebugData("Event data", e, false);
                throw ee;
            }
        }

        #endregion Private Members
    }
}
