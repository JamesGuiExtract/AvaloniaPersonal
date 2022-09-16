using Extract.Licensing;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using UCLID_AFCORELib;
using UCLID_RASTERANDOCRMGMTLib;

namespace Extract.DataEntry
{
    internal class DataEntryCheckBoxCell : DataGridViewCheckBoxCell, IDataEntryTableCell, ICheckBoxObject
    {
        static readonly string _OBJECT_NAME = typeof(DataEntryCheckBoxCell).ToString();

        public event EventHandler<CellSpatialInfoChangedEventArgs> CellSpatialInfoChanged;

        // The DataEntryTable the cell is associated with
        private DataEntryTableBase _dataEntryTable;

        #region Properties

        /// <inheritdoc/>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IAttribute Attribute { get; set; }

        /// <inheritdoc/>
        public DataEntryValidator ValidatorTemplate { get; set; }

        /// <summary>
        /// Unused
        /// </summary>
        public bool RemoveNewLineChars { get; set; }

        /// <inheritdoc/>
        public DataGridViewCell AsDataGridViewCell => this;

        /// <inheritdoc/>
        public bool IsBeingDragged { get; set; }

        /// <inheritdoc/>
        public string CheckedValue
        {
            get => (string)TrueValue;
            set => TrueValue = value;
        }

        /// <inheritdoc/>
        public string UncheckedValue
        {
            get => (string)FalseValue;
            set => FalseValue = value;
        }

        /// <inheritdoc/>
        public bool DefaultCheckedState { get; set; }

        #endregion Properties

        public DataEntryCheckBoxCell() : base()
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
                    LicenseIdName.DataEntryCoreComponents, "ELI53624", _OBJECT_NAME);

                // Set default values
                ValueType = typeof(string);
                TrueValue = "True";
                FalseValue = "False";
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI53625", ex);
            }
        }

        #region Overrides

        /// <summary>
        /// Overridden to call SetValue so that changes take effect immediately
        /// </summary>
        protected override void OnContentClick(DataGridViewCellEventArgs e)
        {
            base.OnContentClick(e);

            string newValue = String.Equals((string)Value, CheckedValue, StringComparison.Ordinal)
                ? UncheckedValue
                : CheckedValue;

            SetValue(e.RowIndex, newValue);
        }

        /// <inheritdoc/>
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
                    // Also don't consider spatial info changed if the incoming attribute is the
                    // same as the existing one.
                    if (Attribute != null && Attribute != attribute &&
                        attribute.Value.HasSpatialInfo())
                    {
                        spatialInfoChanged = true;
                    }

                    // If so, use the provided attribute.
                    Attribute = attribute;                    
                    this.NormalizeValue(Attribute);
                }
                else
                {
                    // Otherwise, test to see if the provided value is a spatial string...
                    if (value is SpatialString spatialString)
                    {
                        if (Attribute != null)
                        {
                            spatialInfoChanged = true;

                            // If so, use it as the cell's value.
                            this.NormalizeValue(Attribute, spatialString.String);
                        }
                        else
                        {
                            // If the associated attribute has not been set, just use the text of 
                            // the spatial string
                            value = spatialString.String;
                        }
                    }
                    else if (Attribute != null)
                    {
                        this.NormalizeValue(Attribute, value?.ToString());
                    }
                }

                // Using the stringized version of the provided value, set the base classes' value.
                if (Attribute != null)
                {
                    result = base.SetValue(rowIndex, Attribute.Value.String);

                    if (DataGridView == null)
                    {
                        // [DataEntry:765]
                        // The value can be updated with a null DataGridView as long as it has an
                        // already initialized attribute and the value is not being set with an
                        // attribute.
                        ExtractException.Assert("ELI53626",
                            "Failed to apply attribute to table cell!", attribute == null);
                    }
                    else
                    {
                        if (DataGridView.Visible && Visible)
                        {
                            // [DataEntry:243]
                            // Set viewable now rather than later, otherwise HasBeenViewed will return
                            // false on the basis that the attribute is not viewable.
                            AttributeStatusInfo.MarkAsViewable(Attribute, true);
                        }

                        // If a control is read-only, consider the attribute as viewed since it is
                        // unlikely to matter if a field that can't be changed was viewed.
                        if (DataGridView.ReadOnly || ReadOnly)
                        {
                            AttributeStatusInfo.MarkAsViewed(Attribute, true);
                        }

                        _dataEntryTable.UpdateCellStyle((IDataEntryTableCell)this);
                    }
                }
                else
                {
                    string normalizedValue = this.NormalizeValue(value?.ToString() ?? "");
                    result = base.SetValue(rowIndex, normalizedValue);
                }

                // If a value was applied, validate it.
                if (result && _dataEntryTable != null)
                {
                    _dataEntryTable.ValidateCell(this, false);
                }

                if (spatialInfoChanged)
                {
                    // If spatial info has been set, a hint is no longer needed.
                    AttributeStatusInfo.SetHintType(Attribute, HintType.None);

                    // Consider the attribute un-accepted after a swipe.
                    AttributeStatusInfo.AcceptValue(Attribute, false);

                    OnCellSpatialInfoChanged(Attribute);
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI53627", ex);
            }

            return result;
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
                throw ExtractException.AsExtractException("ELI53628", ex);
            }
        }

        /// <summary>
        /// Creates a copy of the <see cref="DataEntryCheckBoxCell"/> instance.
        /// </summary>
        /// <returns>A copy of the <see cref="DataEntryCheckBoxCell"/> instance.</returns>
        public override object Clone()
        {
            try
            {
                DataEntryCheckBoxCell clone = (DataEntryCheckBoxCell)base.Clone();

                clone.ValidatorTemplate = ValidatorTemplate;

                return clone;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI53629", ex);
            }
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
                        ErrorText = "";
                    }
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI53630", ex);
                }
            }
        }

        #endregion Overrides

        #region Private Members

        /// <summary>
        /// Raises the <see cref="IDataEntryTableCell.CellSpatialInfoChanged"/> event
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> associated with the event.</param>
        void OnCellSpatialInfoChanged(IAttribute attribute)
        {
            if (this.CellSpatialInfoChanged != null)
            {
                CellSpatialInfoChanged(this, new CellSpatialInfoChangedEventArgs(attribute));
            }
        }

        #endregion Private Members
    }
}