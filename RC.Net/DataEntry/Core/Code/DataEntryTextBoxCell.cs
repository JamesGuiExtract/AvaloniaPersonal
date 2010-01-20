using Extract.Licensing;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using UCLID_AFCORELib;
using UCLID_COMUTILSLib;
using UCLID_RASTERANDOCRMGMTLib;

namespace Extract.DataEntry
{
    /// <summary>
    /// A text box cell for use in a <see cref="DataEntryTableBase"/>-derived class.
    /// </summary>
    public class DataEntryTextBoxCell : DataGridViewTextBoxCell, IDataEntryTableCell
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        private static readonly string _OBJECT_NAME = typeof(DataEntryTextBoxCell).ToString();

        #endregion Constants

        #region Fields

        /// <summary>
        /// The attribute whose value is associated with this cell.
        /// </summary>
        private IAttribute _attribute;

        /// <summary>
        /// The template object to be used as a model for per-attribute validation objects.
        /// </summary>
        private DataEntryValidator _validatorTemplate;

        /// <summary>
        /// Specifies whether carriage return or new line characters will be replaced with spaces.
        /// </summary>
        private bool _removeNewLineChars = true;

        /// <summary>
        /// The <see cref="DataEntryTable"/> the cell is associated with.
        /// </summary>
        private DataEntryTableBase _dataEntryTable;

        /// <summary>
        /// Indicates whether the cell is being dragged as part of a drag and drop operation.
        /// </summary>
        private bool _isBeingDragged;

        /// <summary>
        /// Indicates whether the cell's value is in the process of being initialized.
        /// </summary>
        private bool _initializingValue;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initiates a new <see cref="DataEntryTextBoxCell"/> instance.
        /// </summary>
        public DataEntryTextBoxCell()
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
                LicenseUtilities.ValidateLicense(
                    LicenseIdName.DataEntryCoreComponents, "ELI25584", _OBJECT_NAME);

                _dataEntryTable = DataGridView as DataEntryTableBase;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI25585", ex);
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
        public DataEntryValidator ValidatorTemplate
        {
            get
            {
                return _validatorTemplate;
            }

            set
            {
                _validatorTemplate = value;
            }
        }

        /// <summary>
        /// Gets or sets whether carriage return or new line characters will be replaced with
        /// spaces.
        /// <para><b>Note</b></para>
        /// If <see langword="false"/>, new line characters will be preserved only as long as the
        /// user does not delete the space in text that represents the new line's location.
        /// </summary>
        /// <value><see langword="true"/> to replace carriage return or new line characters;
        /// <see langword="false"/> otherwise.
        /// </value>
        /// <returns><see langword="true"/> if carriage return or new line characters will be
        /// replaced; <see langword="false"/> otherwise.</returns>
        public bool RemoveNewLineChars
        {
            get
            {
                return _removeNewLineChars;
            }

            set
            {
                _removeNewLineChars = value;
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

        /// <summary>
        /// Gets or sets whether the cell is being dragged as part of a drag and drop operation.
        /// </summary>
        /// <value>Sets whether the cell is being dragged as part of a drag and drop operation.
        /// </value>
        /// <returns>Gets whether the cell is being dragged as part of a drag and drop operation.
        /// </returns>
        public bool IsBeingDragged
        {
            get
            {
                return _isBeingDragged;
            }

            set
            {
                _isBeingDragged = value;
            }
        }

        #endregion IDataEntryTableCell Members

        #region Overrides

        /// <summary>
        /// Creates a copy of the <see cref="DataEntryTextBoxCell"/> instance.
        /// </summary>
        /// <returns>A copy of the <see cref="DataEntryTextBoxCell"/> instance.</returns>
        public override object Clone()
        {
            try
            {
                DataEntryTextBoxCell clone = (DataEntryTextBoxCell)base.Clone();

                clone.ValidatorTemplate = _validatorTemplate;
                clone.RemoveNewLineChars = _removeNewLineChars;

                return clone;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI26206", ex);
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

                _dataEntryTable = DataGridView as DataEntryTableBase;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI25668", ex);
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
                if (_initializingValue)
                {
                    return value;
                }

                string stringValue = (value == null) ? "" : value.ToString();

                // If specified, replace newline chars, and save the new value.
                if (_removeNewLineChars)
                {
                    if (stringValue.IndexOf("\r\n", StringComparison.Ordinal) >= 0)
                    {
                        // Replace a group of CRLFs with just a single space.
                        stringValue = Regex.Replace(stringValue, "(\r\n)+", " ");

                        // Store the new value so that the values are replaced without first
                        // requiring an edit to the cell's value. (set _initializingValue to
                        // prevent recursion)
                        _initializingValue = true;
                        base.Value = stringValue;
                        _initializingValue = false;
                    }

                    return stringValue;
                }
                // Otherwise replace CRLFs in the strings used for display with no break spaces to
                // prevent the unprintable "boxes" from appearing.
                else
                {
                    return stringValue.Replace("\r\n", DataEntryMethods._CRLF_REPLACEMENT);
                }
            }
            catch (Exception ex)
            {
                _initializingValue = false;

                throw ExtractException.AsExtractException("ELI26217", ex);
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
            bool result = false;

            try
            {
                bool spatialInfoChanged = false;

                // Test to see if the provided value is an attribute...
                IAttribute attribute = value as IAttribute;

                if (attribute != null)
                {
                    // [DataEntry:367]
                    // Don't consider spatial info changed if an attribute is being applied for the
                    // first time; this only happens during the initial load. Treating this case
                    // as spatialInfoChanged will erroneously clear accepted text.
                    if (_attribute != null && attribute.Value.HasSpatialInfo())
                    {
                        spatialInfoChanged = true;
                    }

                    // If so, use the provided attribute.
                    Attribute = attribute;                    
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
                            AttributeStatusInfo.SetValue(_attribute, spatialString, true, false);

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

                        // Ensure any leading spaces are trimmed off-- controls will use leading
                        // spaces in auto-complete lists to allow all possible entries to be
                        // displayed, but the space should not remain in the final value.
                        if (stringValue.Length > 1 && stringValue[0] == ' ')
                        {
                            stringValue = stringValue.Substring(1);
                        }

                        // If preserving newlines, add CRLFs back so that the replacement value used
                        // for display is never actually stored.
                        if (!_removeNewLineChars)
                        {
                            stringValue =
                                stringValue.Replace(DataEntryMethods._CRLF_REPLACEMENT, "\r\n");
                        }

                        // Otherwise, used the provided value as a string to replace the text of
                        // the associated attribute.
                        AttributeStatusInfo.SetValue(_attribute, stringValue, true, false);
                    }
                }

                // Using the stringized version of the provided value, set the base classes' value.
                if (_attribute != null)
                {
                    result = base.SetValue(rowIndex, _attribute.Value.String);

                    if (DataGridView == null)
                    {
                        // [DataEntry:765]
                        // The value can be updated with a null DataGridView as long as it has an
                        // already initialized attribute and the value is not being set with an
                        // attribute.
                        ExtractException.Assert("ELI29203",
                            "Failed to apply attribute to table cell!", attribute == null);
                    }
                    else
                    {
                        if (DataGridView.Visible && Visible)
                        {
                            // [DataEntry:243]
                            // Set viewable now rather than later, otherwise HasBeenViewed will return
                            // false on the basis that the attribute is not viewable.
                            AttributeStatusInfo.MarkAsViewable(_attribute, true);
                        }

                        // If a control is read-only, consider the attribute as viewed since it is
                        // unlikely to matter if a field that can't be changed was viewed.
                        if (DataGridView.ReadOnly || ReadOnly)
                        {
                            AttributeStatusInfo.MarkAsViewed(_attribute, true);
                        }

                        _dataEntryTable.UpdateCellStyle((IDataEntryTableCell)this);
                    }
                }
                else
                {
                    result = base.SetValue(rowIndex, value);
                }

                // If a value was applied, validate it.
                if (result && _dataEntryTable != null)
                {
                    _dataEntryTable.ValidateCell(this, false);
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
                ExtractException.Display("ELI25583", ex);
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
                    throw ExtractException.AsExtractException("ELI26216", ex);
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

        #endregion Private Members
    }
}
