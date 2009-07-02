using Extract.Licensing;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing.Design;
using System.Text;
using System.Windows.Forms;
using UCLID_AFCORELib;
using UCLID_RASTERANDOCRMGMTLib;

namespace Extract.DataEntry
{
    /// <summary>
    /// An extension of <see cref="DataGridViewColumn"/> that allows for Extract Systems data entry
    /// specific properties and behavior.
    /// </summary>
    public class DataEntryTableColumn : DataGridViewColumn
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        private static readonly string _OBJECT_NAME = typeof(DataEntryTableColumn).ToString();

        #endregion Constants

        #region Fields

        /// <summary>
        /// The name subattributes to the table's primary attributes must have to be displayed in 
        /// this column.
        /// </summary>
        private string _attributeName;

        /// <summary>
        /// The selection mode to use when multiple attributes are found which match the attribute 
        /// name for this column.
        /// </summary>
        private MultipleMatchSelectionMode _multipleMatchSelectionMode =
            MultipleMatchSelectionMode.First;

        /// <summary>
        /// 
        /// </summary>
        private string _formattingRuleFileName;

        /// <summary>
        /// The formatting rule to be used when processing text from image swipes in this column.
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
        /// Specifies whether tab should always stop on the column or whether it can be skipped
        /// if empty and valid.
        /// </summary>
        private bool _tabStopRequired = true;

        /// <summary>
        /// A query which will cause the validation list to be updated using the values from other
        /// <see cref="IAttribute"/>'s and/or a database query.
        /// </summary>
        private string _validationQuery;

        /// <summary>
        /// Specifies whether the cells in the column should be edited with a non-editable combo box.
        /// </summary>
        private bool _useComboBoxCells;

        /// <summary>
        /// Specifies whether the current instance is running in design mode
        /// </summary>
        private bool _inDesignMode;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new <see cref="DataEntryTableColumn"/> instance.
        /// </summary>
        public DataEntryTableColumn()
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
                LicenseUtilities.ValidateLicense(LicenseIdName.FlexIndexCoreObjects, "ELI24497",
                    _OBJECT_NAME);

                // [DataEntry:407]
                // The DataEntryTable cannot currently support sorting.  Initialize as NotSortable
                // and hide the SortMode property.
                base.SortMode = DataGridViewColumnSortMode.NotSortable;

                UpdateCellTemplate();
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI24484", ex);
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the name subattributes to the table's primary attribute must have to be
        /// displayed in this column.
        /// </summary>
        [Category("Data Entry Table Column")]
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
        /// Gets or sets the selection mode to use when choosing between multiple candidate
        /// attributes for a cell in the <see cref="DataEntryTableColumn"/>
        /// </summary>
        /// <value>The selection mode to use to when choosing between multiple candidate attributes
        /// for a cell in a <see cref="DataEntryTableColumn"/>.</value>
        /// <returns>The selection mode to use to when choosing between multiple candidate attributes
        /// for a cell in a <see cref="DataEntryTableColumn"/>.</returns>
        [Category("Data Entry Table Column")]
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
        /// <see cref="IDataEntryControl.ProcessSwipedText"/> for this column.
        /// </summary>
        /// <value>The filename of the <see cref="IRuleSet"/> to be used. Can be 
        /// <see langword="null"/> if no rule should be used.</value>
        /// <returns>The filename of the <see cref="IRuleSet"/> to be used. Can be 
        /// <see langword="null"/> if no rule has been specified.</returns>
        [Category("Data Entry Table Column")]
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
                    throw ExtractException.AsExtractException("ELI24242", ex);
                }
            }
        }

        /// <summary>
        /// Gets the <see cref="IRuleSet"/> that should be used to reformat or split
        /// <see cref="SpatialString"/> content passed into 
        /// <see cref="IDataEntryControl.ProcessSwipedText"/> for this column.
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
        /// Gets or set a regular expression the data entered in a cell must match prior to being 
        /// saved.
        /// <para><b>Requirements</b></para>
        /// Cannot be specified at the same time <see cref="ValidationPattern"/> is specified.
        /// <para><b>Note</b></para>
        /// If the columns's value matches a value in the supplied list case-insensitively but not 
        /// case-sensitively, the value will be modified to match the casing in the list. If a value 
        /// is specified in the list multiple times, the casing of the last entry will be used.
        /// </summary>
        /// <value>A regular expression the data entered in a cell must match prior to being 
        /// saved. <see langword="null"/> to remove any existing validation pattern requirement.</value>
        /// <returns>A regular expression the data entered in a cell must match prior to being 
        /// saved. <see langword="null"/> if there is no validation pattern set.</returns>
        [Category("Data Entry Table Column")]
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
                    throw ExtractException.AsExtractException("ELI24299", ex);
                }
            }

            set
            {
                try
                {
                    _validator.ValidationPattern = value;

                    UpdateCellTemplate();
                }
                catch (ExtractException ex)
                {
                    throw ExtractException.AsExtractException("ELI24277", ex);
                }
            }
        }

        /// <summary>
        /// Gets or sets the name of a file containing a list of possible values the data entered in
        /// a cell must match prior to being saved. 
        /// <para><b>Requirements</b></para>
        /// Must be specified if <see cref="UseComboBoxCells"/> is <see langword="true"/> since the 
        /// values from this list will be used to populate the values of the combo boxes in this row.
        /// Cannot be specified at the same time <see cref="ValidationPattern"/> is specified.
        /// <para><b>Note</b></para>
        /// If a cell's value matches a value in the supplied list case-insensitively but not 
        /// case-sensitively, the value will be modified to match the casing in the list. If a value 
        /// is specified in the list multiple times, the casing of the last entry will be used.
        /// </summary>
        /// <value>The name of a file containing list of values. <see langword="null"/> to remove
        /// any existing validation list requirement.</value>
        /// <returns>The name of a file containing list of values. <see langword="null"/> if there 
        /// is no validation list set.</returns>
        [Category("Data Entry Table Column")]
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
                    throw ExtractException.AsExtractException("ELI24278", ex);
                }
            }

            set
            {
                try
                {
                    _validator.ValidationListFileName = value;

                    UpdateCellTemplate();
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI24280", ex);
                }
            }
        }

        /// <summary>
        /// Gets or sets a query which will cause the column's validation list to be automatically
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
        [Category("Data Entry Table Column")]
        [Editor("System.ComponentModel.Design.MultilineStringEditor, System.Design", typeof(UITypeEditor)), Localizable(true)]
        public string ValidationQuery
        {
            get
            {
                return _validationQuery;
            }

            set
            {
                try
                {
                    _validationQuery = value;

                    UpdateCellTemplate();
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI26512", ex);
                }
            }
        }

        /// <summary>
        /// Gets or set the error message that should be displayed for an error provider icon on
        /// validation failure. If unspecified, a default of "Bad value" will be used.
        /// </summary>
        /// <value>The tooltip text that should be displayed for an error provider icon on
        /// validation failure.</value>
        /// <returns>The tooltip text that should be displayed for an error provider icon on
        /// validation failure.</returns>
        [Category("Data Entry Table Column")]
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
                    throw ExtractException.AsExtractException("ELI24300", ex);
                }
            }

            set
            {
                try
                {
                    _validator.ValidationErrorMessage = value;

                    UpdateCellTemplate();
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI24301", ex);
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
        [Category("Data Entry Table Column")]
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
        /// Specifies whether tab should always stop on this column or whether it can be skipped
        /// if the next cell is empty and valid.
        /// </summary>
        /// <value><see langword="true"/> if the column should always be a tabstop,
        /// <see langword="false"/> if the column can be skipped if empty and valid</value>
        /// <returns><see langword="true"/> if the column is always be a tabstop,
        /// <see langword="false"/> if the column will be skipped if empty and valid</returns>
        [Category("Data Entry Table Column")]
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
        /// Specifies whether the cells in the column should be edited with a non-editable combo box.
        /// </summary>
        /// <value><see langword="true"/> if a <see cref="DataEntryComboBoxCell"/> should be used to
        /// edit values in this column, <see langword="false"/> if values should be edited with a
        /// <see cref="DataEntryTextBoxCell"/>.</value>
        /// <returns><see langword="true"/> if a <see cref="DataEntryComboBoxCell"/> are used to
        /// edit values in this column, <see langword="false"/> if values are edited with a
        /// <see cref="DataEntryTextBoxCell"/>.</returns>
        [Category("Data Entry Table Column")]
        public bool UseComboBoxCells
        {
            get
            {
                return _useComboBoxCells;
            }

            set
            {
                try
                {
                    _useComboBoxCells = value;

                    UpdateCellTemplate();
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI26513", ex);
                }
            }
        }

        /// <summary>
        /// SortMode is not supported by <see cref="DataEntryTable"/>. Hide the base class property
        /// as best as possible
        /// </summary>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public new DataGridViewColumnSortMode SortMode
        {
            get
            {
                return DataGridViewColumnSortMode.NotSortable;
            }

            set
            {
                if (value != DataGridViewColumnSortMode.NotSortable)
                {
                    throw new ExtractException("ELI26630", "SortMode is not supported!");
                }
            }
        }

        #endregion Properties

        #region Overrides

        /// <summary>
        /// Creates an exact copy of the <see cref="DataEntryTableColumn"/> instance.
        /// </summary>
        /// <returns>An exact copy of the <see cref="DataEntryTableColumn"/> instance.</returns>
        public override object Clone()
        {
            try
            {
                DataEntryTableColumn column = (DataEntryTableColumn)base.Clone();

                // Copy DataEntryTableColumn specific properties
                column.AttributeName = this.AttributeName;
                column.FormattingRuleFile = this.FormattingRuleFile;
                column.MultipleMatchSelectionMode = this.MultipleMatchSelectionMode;
                column.ValidationPattern = this.ValidationPattern;
                column.ValidationListFileName = this.ValidationListFileName;
                column.ValidationQuery = this.ValidationQuery;
                column.ValidationErrorMessage = this.ValidationErrorMessage;
                column.UseComboBoxCells = this.UseComboBoxCells;
                column.AutoUpdateQuery = this.AutoUpdateQuery;
                column.TabStopRequired = this.TabStopRequired;

                return column;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI24221", ex);
            }
        }

        /// <summary>
        /// Called when the band is associated with a different <see cref="DataGridView"/>.
        /// </summary>
        protected override void OnDataGridViewChanged()
        {
            try
            {
                base.OnDataGridViewChanged();

                // If a valid grid has been specified and combo boxes are being used, ensure a
                // validation list has been specified.
                if (base.DataGridView != null && _useComboBoxCells)
                {
                    string[] validationListValues = _validator.GetValidationListValues();

                    ExtractException.Assert("ELI25578", "ValidationListFileName must be specified " +
                        "for ComboBox cells!", _inDesignMode || validationListValues != null);
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI25935", ex);
            }
        }

        #endregion Overrides

        /// <summary>
        /// Updates the cell template to reflect the current _useComboBoxCells and validation
        /// settings.
        /// </summary>
        private void UpdateCellTemplate()
        {
            // Use a combo box template or text box template depending on the value of 
            // _useComboBoxCells.
            if (_useComboBoxCells)
            {
                DataEntryComboBoxCell cellTemplate = new DataEntryComboBoxCell();
                cellTemplate.Validator = (DataEntryValidator)_validator.Clone();

                string[] validationList = _validator.GetValidationListValues();
                if (validationList != null)
                {
                    cellTemplate.Items.AddRange(_validator.GetValidationListValues());
                }

                base.CellTemplate = cellTemplate;
            }
            else
            {
                DataEntryTextBoxCell cellTemplate = new DataEntryTextBoxCell();
                cellTemplate.Validator = (DataEntryValidator)_validator.Clone();
                base.CellTemplate = cellTemplate;
            }
        }
    }
}
