using Extract.Licensing;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Design;
using System.Text;
using System.Windows.Forms;
using UCLID_AFCORELib;
using UCLID_RASTERANDOCRMGMTLib;

namespace Extract.DataEntry
{
    /// <summary>
    /// An extension of <see cref="DataGridViewRow"/> that allows for Extract Systems data entry
    /// framework specific properties and behavior.
    /// </summary>
    public class DataEntryTableRow : DataGridViewRow
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        private static readonly string _OBJECT_NAME = typeof(DataEntryTableRow).ToString();

        #endregion Constants

        #region Fields

        /// <summary>
        /// The attribute whose value is associated with this row.
        /// </summary>
        private IAttribute _attribute;

        /// <summary>
        /// The display name to be used for this row.
        /// </summary>
        private string _name;

        /// <summary>
        /// The name subattributes to the table's primary attributes must have to be displayed in 
        /// this row.
        /// </summary>
        private string _attributeName;

        /// <summary>
        /// The selection mode to use when multiple attributes are found which match the attribute 
        /// name for this row.
        /// </summary>
        private MultipleMatchSelectionMode _multipleMatchSelectionMode =
            MultipleMatchSelectionMode.First;

        /// <summary>
        /// 
        /// </summary>
        private string _formattingRuleFileName;

        /// <summary>
        /// The formatting rule to be used when processing text from image swipes in this row.
        /// </summary>
        private IRuleSet _formattingRule;

        /// <summary>
        /// The object which will provide validation for cell data.
        /// </summary>
        private DataEntryValidator _validator = new DataEntryValidator();

        /// <summary>
        /// A query which will cause value to automatically be updated using the values from other
        /// <see cref="IAttribute"/>'s and/or a database query.
        /// </summary>
        private string _autoUpdateQuery;

        /// <summary>
        /// Specifies whether tab should always stop on the row or whether it can be skipped
        /// if empty and valid.
        /// </summary>
        private bool _tabStopRequired = true;

        /// <summary>
        /// A query which will cause the validation list to be updated using the values from other
        /// <see cref="IAttribute"/>'s and/or a database query.
        /// </summary>
        private string _validationQuery;

        /// <summary>
        /// Specifies whether the cells in the row should be edited with a non-editable combo box.
        /// </summary>
        private bool _useComboBoxCells;

        /// <summary>
        /// Specifies whether the current instance is running in design mode
        /// </summary>
        private bool _inDesignMode;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new <see cref="DataEntryTableRow"/> instance.
        /// </summary>
        public DataEntryTableRow()
            : base()
        {
            try
            {
                // Use a ProcessName check for design mode because LicenseUsageMode.UsageMode 
                // isn't always accruate.
                _inDesignMode = Process.GetCurrentProcess().ProcessName.Equals(
                    "devenv", StringComparison.CurrentCultureIgnoreCase);

                // Load licenses in design mode
                if (_inDesignMode)
                {
                    // Load the license files from folder
                    LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());
                }

                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.FlexIndexCoreObjects, "ELI24486",
                    _OBJECT_NAME);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI24487", ex);
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// The <see cref="IAttribute"/> whose value is associated with this row.
        /// </summary>
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
        /// Gets or sets the display name for the row (the label that will appear in the row header).
        /// </summary>
        /// <value>The display name for the row.</value>
        /// <returns>The display name for the row.</returns>
        [Category("Data Entry Table Row")]
        public string Name
        {
            get
            {
                return _name;
            }

            set
            {
                _name = value;
            }
        }

        /// <summary>
        /// Gets or sets the name subattributes to the table's primary attribute must have to be 
        /// displayed in this row.
        /// </summary>
        /// <value>The name subattributes to the table's primary attribute must have to be 
        /// displayed in this row.</value>
        /// <returns>The name subattributes to the table's primary attribute must have to be 
        /// displayed in this row.</returns>
        [Category("Data Entry Table Row")]
        public string AttributeName
        {
            get
            {
                return _attributeName;
            }

            set
            {
                _attributeName = value;
            }
        }

        /// <summary>
        /// Gets or sets the selection mode to use to find the mapped attribute for this
        /// <see cref="DataEntryTableRow"/>
        /// </summary>
        /// <value>The selection mode to use to find the mapped attribute for this
        /// <see cref="DataEntryTableRow"/>.</value>
        /// <returns>The selection mode to use to find the mapped attribute for this
        /// <see cref="DataEntryTableRow"/>.</returns>
        [Category("Data Entry Table Row")]
        public MultipleMatchSelectionMode MultipleMatchSelectionMode
        {
            get
            {
                return _multipleMatchSelectionMode;
            }

            set
            {
                _multipleMatchSelectionMode = value;
            }
        }

        /// <summary>
        /// Specifies the filename of an <see cref="IRuleSet"/> that should be used to reformat or
        /// split <see cref="SpatialString"/> content passed into 
        /// <see cref="IDataEntryControl.ProcessSwipedText"/> for this row.
        /// </summary>
        /// <value>The filename of the <see cref="IRuleSet"/> to be used. Can be 
        /// <see langword="null"/> if no rule should be used.</value>
        /// <returns>The filename of the <see cref="IRuleSet"/> to be used. Can be 
        /// <see langword="null"/> if no rule has been specified.</returns>
        [Category("Data Entry Table Row")]
        public string FormattingRuleFile
        {
            get
            {
                return _formattingRuleFileName;
            }

            set
            {
                try
                {
                    // If the a formatting rule is specified, attempt to load
                    // an attribute finding rule.
                    if (!_inDesignMode && !string.IsNullOrEmpty(value))
                    {
                        _formattingRule = (IRuleSet)new RuleSetClass();
                        _formattingRule.LoadFrom(DataEntryMethods.ResolvePath(value), false);
                    }
                    else
                    {
                        _formattingRule = null;
                    }

                    _formattingRuleFileName = value;
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI24267", ex);
                }
            }
        }

        /// <summary>
        /// Gets the <see cref="IRuleSet"/> that should be used to reformat or
        /// split <see cref="SpatialString"/> content passed into 
        /// <see cref="IDataEntryControl.ProcessSwipedText"/> for this row.
        /// </summary>
        /// <returns>The <see cref="IRuleSet"/> that should be used. Can be <see langword="null"/>
        /// if no formatting rule has been specified.</returns>
        [Browsable(false)]
        public IRuleSet FormattingRule
        {
            get
            {
                return _formattingRule;
            }
        }

        /// <summary>
        /// Gets or set a regular expression the data entered in this row must match prior to being 
        /// saved.
        /// <para><b>Requirements</b></para>
        /// Must be specified if <see cref="UseComboBoxCells"/> is <see langword="true"/> since the 
        /// values from this list will be used to populate the values of the combo boxes in this column.
        /// Cannot be specified at the same time <see cref="ValidationListFileName"/> is specified.
        /// </summary>
        /// <value>A regular expression the data entered in this row must match prior to being 
        /// saved. <see langword="null"/> to remove any existing validation pattern requirement.</value>
        /// <returns>A regular expression the data entered in this row must match prior to being 
        /// saved. <see langword="null"/> if there is no validation pattern set.</returns>
        [Category("Data Entry Table Row")]
        public string ValidationPattern
        {
            get
            {
                try
                {
                    return _validator.ValidationPattern;
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI24302", ex);
                }
            }

            set
            {
                try
                {
                    _validator.ValidationPattern = value;
                }
                catch (ExtractException ex)
                {
                    throw ExtractException.AsExtractException("ELI24303", ex);
                }
            }
        }

        /// <summary>
        /// Gets or sets the name of a file containing a list of possible values the data entered in
        /// this row must match prior to being saved. 
        /// <para><b>Requirements</b></para>
        /// Cannot be specified at the same time <see cref="ValidationPattern"/> is specified.
        /// <para><b>Note</b></para>
        /// If the row's value matches a value in the supplied list case-insensitively but not 
        /// case-sensitively, the value will be modified to match the casing in the list. If a value 
        /// is specified in the list multiple times, the casing of the last entry will be used.
        /// </summary>
        /// <value>The name of a file containing list of values. <see langword="null"/> to remove
        /// any existing validation list requirement.</value>
        /// <returns>The name of a file containing list of values. <see langword="null"/> if there 
        /// is no validation list set.</returns>
        [Category("Data Entry Table Row")]
        public string ValidationListFileName
        {
            get
            {
                try
                {
                    return _validator.ValidationListFileName;
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI24304", ex);
                }
            }

            set
            {
                try
                {
                    _validator.ValidationListFileName = value;

                    // Update combo box list items using the validation list.
                    if (_useComboBoxCells)
                    {
                        UpdateComboBoxItems();
                    }
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI24305", ex);
                }
            }
        }

        /// <summary>
        /// Gets or sets a query which will cause the row's validation list to be automatically
        /// updated using values from other <see cref="IAttribute"/>'s and/or a database query.
        /// Values to be used from other <see cref="IAttribute"/>'s values should be inserted into
        /// the query using curly braces. For example, to have the validation list reflect the value
        /// of a sibling attribute named "Source", the query would be specified as "{../Source}".
        /// Text contained withing &lt;SQL&gt; tags will be used as SQL queries against
        /// <see cref="DataEntryControlHost"/>'s database (after substituting in attribute values).
        /// Every time an attribute specified in the query is modified, this query will be 
        /// re-evaluated and used to update the validation list.
        /// </summary>
        /// <value>A query which will cause the validation list to be automatically updated using
        /// values from other <see cref="IAttribute"/>'s and/or a database query.</value>
        /// <returns>A query being used to automatically update the validation list using values
        /// from other <see cref="IAttribute"/>'s and/or a database query.</returns>
        [Category("Data Entry Table Row")]
        [Editor("System.ComponentModel.Design.MultilineStringEditor, System.Design", typeof(UITypeEditor)), Localizable(true)]
        public string ValidationQuery
        {
            get
            {
                return _validationQuery;
            }

            set
            {
                _validationQuery = value;
            }
        }

        /// <summary>
        /// Gets or sets whether a value that matches a validation list item case-insensitively but
        /// not case-sensitively will be changed to match the validation list value.
        /// </summary>
        /// <value><see langword="true"/> if values should be modified to match the case of list items,
        /// <see langword="false"/> if case-insensitive matches should be left as-is.</value>
        /// <returns><see langword="true"/> if values will be modified to match the case of list items,
        /// <see langword="false"/> if case-insensitive matches will be left as-is.</returns>
        [Category("Data Entry Table Row")]
        public bool ValidationCorrectsCase
        {
            get
            {
                return _validator.CorrectCase;
            }

            set
            {
                _validator.CorrectCase = value;
            }
        }

        /// <summary>
        /// Gets or set the error message that should be presented to the user upon validation 
        /// failure. If unspecified, a default of "Bad value" will be used.
        /// </summary>
        /// <value>The error message that should be presented to the user upon validation failure.
        /// </value>
        /// <returns>The error message that should be presented to the user upon validation failure.
        /// </returns>
        [Category("Data Entry Table Row")]
        public string ValidationErrorMessage
        {
            get
            {
                try
                {
                    return _validator.ValidationErrorMessage;
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI24306", ex);
                }
            }

            set
            {
                try
                {
                    _validator.ValidationErrorMessage = value;
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI24307", ex);
                }
            }
        }

        /// <summary>
        /// Gets or sets a query which will cause a contained cell's value to automatically be
        /// updated using values from other <see cref="IAttribute"/>'s and/or a database query.
        /// Values to be used from other <see cref="IAttribute"/>'s values should be inserted into
        /// the query using curly braces. For example, to have the value reflect the value
        /// of a sibling attribute named "Source", the query would be specified as
        /// "{../Source}". If the query matches SQL syntax it will be executed against the
        /// <see cref="DataEntryControlHost"/>'s database. Every time an attribute specified in the
        /// query is modified, this query will be re-evaluated and used to update the value.
        /// </summary>
        /// <value>A query which will cause value to automatically be updated using values
        /// from other <see cref="IAttribute"/>'s and/or a database query.</value>
        /// <returns>A query which will cause value to automatically be updated using values
        /// from other <see cref="IAttribute"/>'s and/or a database query.</returns>
        [Category("Data Entry Table Row")]
        [Editor("System.ComponentModel.Design.MultilineStringEditor, System.Design", typeof(UITypeEditor)), Localizable(true)]
        public string AutoUpdateQuery
        {
            get
            {
                return _autoUpdateQuery;
            }

            set
            {
                _autoUpdateQuery = value;
            }
        }

        /// <summary>
        /// Specifies whether tab should always stop on this row or whether it can be skipped
        /// if the next cell is empty and valid.
        /// </summary>
        /// <value><see langword="true"/> if the row should always be a tabstop,
        /// <see langword="false"/> if the row can be skipped if empty and valid</value>
        /// <returns><see langword="true"/> if the row is always be a tabstop,
        /// <see langword="false"/> if the row will be skipped if empty and valid</returns>
        [Category("Data Entry Table Row")]
        public bool TabStopRequired
        {
            get
            {
                return _tabStopRequired;
            }

            set
            {
                _tabStopRequired = value;
            }
        }

        /// <summary>
        /// Specifies whether the cells in the row should be edited with a non-editable combo box.
        /// </summary>
        /// <value><see langword="true"/> if a <see cref="DataEntryComboBoxCell"/> should be used to
        /// edit values in this row, <see langword="false"/> if values should be edited with a
        /// <see cref="DataEntryTextBoxCell"/>.</value>
        /// <returns><see langword="true"/> if a <see cref="DataEntryComboBoxCell"/> are used to
        /// edit values in this row, <see langword="false"/> if values are edited with a
        /// <see cref="DataEntryTextBoxCell"/>.</returns>
        [Category("Data Entry Table Row")]
        public bool UseComboBoxCells
        {
            get
            {
                return _useComboBoxCells;
            }

            set
            {
                _useComboBoxCells = value;
            }
        }

        #endregion Properties

        #region Overrides

        /// <summary>
        /// Creates an exact copy of this <see cref="DataEntryTableRow"/>.
        /// </summary>
        /// <returns>An exact copy of this <see cref="DataEntryTableRow"/>.</returns>
        public override object Clone()
        {
            try
            {
                DataEntryTableRow row = (DataEntryTableRow)base.Clone();

                // Copy DataEntryTableRow specific properties
                row.AttributeName = this.AttributeName;
                row.FormattingRuleFile = this.FormattingRuleFile;
                row.Name = this.Name;
                row.MultipleMatchSelectionMode = this.MultipleMatchSelectionMode;
                row.ValidationPattern = this.ValidationPattern;
                row.ValidationListFileName = this.ValidationListFileName;
                row.ValidationQuery = this.ValidationQuery;
                row.ValidationErrorMessage = this.ValidationErrorMessage;
                row.UseComboBoxCells = this.UseComboBoxCells;
                row.AutoUpdateQuery = this.AutoUpdateQuery;
                row.TabStopRequired = this.TabStopRequired;
                row.ValidationCorrectsCase = this.ValidationCorrectsCase;

                return row;
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI25574", ex);

                return null;
            }
        }

        /// <summary>
        /// Called when the band is associated with a different <see cref="DataGridView"/>.  This
        /// call initializes the list of items for the combo boxes, if necessary, using 
        /// <see cref="ValidationListFileName"/>.</summary>
        protected override void OnDataGridViewChanged()
        {
            try
            {
                base.OnDataGridViewChanged();

                // If a valid grid has been specified and combo boxes are being used, initialize the
                // item list in each cell.
                if (base.DataGridView != null && _useComboBoxCells)
                {
                    string[] validationListValues = _validator.GetValidationListValues();

                    ExtractException.Assert("ELI25934", "ValidationListFileName must be specified " +
                        "for ComboBox cells", _inDesignMode || validationListValues != null);

                    if (validationListValues != null)
                    {
                        // Create a template to use in each cell.
                        DataEntryComboBoxCell template = new DataEntryComboBoxCell();
                        template.Items.AddRange(validationListValues);

                        // Loop through the cell in each column of the table, and replace the
                        // cell with a clone of the template.
                        for (int i = 0; i < base.DataGridView.ColumnCount; i++)
                        {
                            base.Cells[i] = (DataGridViewCell)template.Clone();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI25581", ex);
            }
        }

        #endregion Overrides

        #region Private Members

        /// <summary>
        /// Updates the items in the combo box's list using the current validation list.
        /// </summary>
        private void UpdateComboBoxItems()
        {
            // If a validation list was specified, use it to populate the combo box items.
            string[] validationListValues = _validator.GetValidationListValues();
            if (validationListValues != null)
            {
                foreach (DataEntryComboBoxCell cell in base.Cells)
                {
                    // Reseting the item list will clear the value. Preserve the original value.
                    string originalValue = cell.Value.ToString();

                    cell.Items.Clear();
                    cell.Items.AddRange(validationListValues);

                    // Restore the original value
                    cell.Value = originalValue;
                }
            }
        }

        #endregion Private Members
    }
}
