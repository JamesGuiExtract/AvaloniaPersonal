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
        static readonly string _OBJECT_NAME = typeof(DataEntryTableColumn).ToString();

        #endregion Constants

        #region Fields

        /// <summary>
        /// The name subattributes to the table's primary attributes must have to be displayed in 
        /// this column.
        /// </summary>
        string _attributeName;

        /// <summary>
        /// The selection mode to use when multiple attributes are found which match the attribute 
        /// name for this column.
        /// </summary>
        MultipleMatchSelectionMode _multipleMatchSelectionMode =
            MultipleMatchSelectionMode.None;

        /// <summary>
        /// 
        /// </summary>
        string _formattingRuleFileName;

        /// <summary>
        /// The formatting rule to be used when processing text from image swipes in this column.
        /// </summary>
        IRuleSet _formattingRule;
        
        /// <summary>
        /// The template object to be used as a model for per-attribute validation objects.
        /// </summary>
        DataEntryValidator _validatorTemplate = new DataEntryValidator();

        /// <summary>
        /// A query which will cause value to automatically be updated using the values from other
        /// <see cref="IAttribute"/>'s and/or a database query.
        /// </summary>
        string _autoUpdateQuery;

        /// <summary>
        /// Specifies under what circumstances the column's attributes should serve as a tab stop.
        /// </summary>
        TabStopMode _tabStopMode = TabStopMode.Always;

        /// <summary>
        /// Specifies whether the attribute's value should be saved.
        /// </summary>
        bool _persistAttribute = true;

        /// <summary>
        /// A query which will cause the validation list to be updated using the values from other
        /// <see cref="IAttribute"/>'s and/or a database query.
        /// </summary>
        string _validationQuery;

        /// <summary>
        /// Specifies whether the cells in the column should be edited with a non-editable combo box.
        /// </summary>
        bool _useComboBoxCells;

        /// <summary>
        /// Specifies whether carriage return or new line characters will be replaced with spaces.
        /// </summary>
        bool _removeNewLineChars = true;

        /// <summary>
        /// Specifies whether the table will attempt to generate a hint using the intersection of 
        /// the row and column occupied by the specified attributes in this column.
        /// </summary>
        bool _smartHintsEnabled;

        /// <summary>
        /// Specifies whether if the cells in the column will accept as input the
        /// <see cref="SpatialString"/> associated with an image swipe.
        /// </summary>
        bool _supportsSwiping = true;

        /// <summary>
        /// Specifies whether the current instance is running in design mode
        /// </summary>
        bool _inDesignMode;

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
                LicenseUtilities.ValidateLicense(
                    LicenseIdName.DataEntryCoreComponents, "ELI24497", _OBJECT_NAME);

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
        [DefaultValue(MultipleMatchSelectionMode.None)]
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
        [DefaultValue(null)]
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
        [DefaultValue(null)]
        public string ValidationPattern
        {
            get
            {
                try
                {
                    return _validatorTemplate.ValidationPattern;
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
                    _validatorTemplate.ValidationPattern = value;

                    UpdateCellTemplate();
                }
                catch (ExtractException ex)
                {
                    throw ExtractException.AsExtractException("ELI24277", ex);
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
        [DefaultValue(null)]
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
        /// Gets or sets whether a value that matches a validation list item case-insensitively but
        /// not case-sensitively will be changed to match the validation list value.
        /// </summary>
        /// <value><see langword="true"/> if values should be modified to match the case of list items,
        /// <see langword="false"/> if case-insensitive matches should be left as-is.</value>
        /// <returns><see langword="true"/> if values will be modified to match the case of list items,
        /// <see langword="false"/> if case-insensitive matches will be left as-is.</returns>
        [Category("Data Entry Table Column")]
        [DefaultValue(true)]
        public bool ValidationCorrectsCase
        {
            get
            {
                return _validatorTemplate.CorrectCase;
            }

            set
            {
                _validatorTemplate.CorrectCase = value;

                UpdateCellTemplate();
            }
        }

        /// <summary>
        /// Gets or sets whether validation lists will be checked for matches case-sensitively.
        /// </summary>
        /// <value><see langword="true"/> to validate against a validation list case-sensitively;
        /// <see langword="false"/> otherwise.</value>
        /// <returns><see langword="true"/> if validating against a validation list
        /// case-sensitively; <see langword="false"/> otherwise.</returns>
        [Category("Data Entry Table Column")]
        [DefaultValue(false)]
        public bool ValidationIsCaseSensitive
        {
            get
            {
                return _validatorTemplate.CaseSensitive;
            }

            set
            {
                _validatorTemplate.CaseSensitive = value;

                UpdateCellTemplate();
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
                    return _validatorTemplate.ValidationErrorMessage;
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
                    _validatorTemplate.ValidationErrorMessage = value;

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
        [Editor("System.ComponentModel.Design.MultilineStringEditor, System.Design", typeof(UITypeEditor)), Localizable(true)]
        [DefaultValue(null)]
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
        /// Specifies under what circumstances the <see cref="DataEntryTableColumn"/>'s
        /// <see cref="IAttribute"/>s should serve as a tab stop.
        /// </summary>
        /// <value>A <see cref="TabStopMode"/> value indicating when the attributes should serve as a
        /// tab stop.</value>
        /// <returns>A <see cref="TabStopMode"/> value indicating when the attributes will serve as a
        /// tab stop.</returns>
        [Category("Data Entry Table Column")]
        [DefaultValue(TabStopMode.Always)]
        public TabStopMode TabStopMode
        {
            get
            {
                return _tabStopMode;
            }

            set
            {
                _tabStopMode = value;
            }
        }

        /// <summary>
        /// Gets or sets whether the <see cref="IAttribute"/>'s value should be saved.
        /// </summary>
        /// <value><see langword="true"/> to save the attribute's value; <see langword="false"/>
        /// otherwise.
        /// </value>
        /// <returns><see langword="true"/> if the attribute's value will be saved;
        /// <see langword="false"/> otherwise.</returns>
        [Category("Data Entry Table Column")]
        [DefaultValue(true)]
        public bool PersistAttribute
        {
            get
            {
                return _persistAttribute;
            }

            set
            {
                try
                {
                    if (!value && _persistAttribute != value && _inDesignMode && 
                        base.DataGridView != null && _attributeName == ".")
                    {
                        MessageBox.Show(null, "Setting the parent attribute of a column as " +
                            " non-persistable will prevent all data in this table from being saved!",
                            "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation,
                            MessageBoxDefaultButton.Button1, 0);
                    }

                    _persistAttribute = value;
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI27112", ex);
                }
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
        [DefaultValue(false)]
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
                    if (_useComboBoxCells != value)
                    {
                        _useComboBoxCells = value;

                        UpdateCellTemplate();
                    }
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI26513", ex);
                }
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
        [Category("Data Entry Table Column")]
        [DefaultValue(true)]
        public bool RemoveNewLineChars
        {
            get
            {
                return _removeNewLineChars;
            }

            set
            {
                try
                {
                    if (_removeNewLineChars != value)
                    {
                        _removeNewLineChars = value;

                        UpdateCellTemplate();
                    }
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI27298", ex);
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

        /// <summary>
        /// Specifies whether GetSpatialHint will attempt to generate a hint using the
        /// intersection of the row and column occupied by the specified <see cref="IAttribute"/>.
        /// </summary>
        /// <value><see langword="true"/> if the smart hints should be generated for this column
        /// when possible; <see langword="false"/> if the column should never attempt to generate
        /// smart hints.</value>
        /// <returns><see langword="true"/> if the column is configured to generate smart hints when
        /// possible; <see langword="false"/> if the column is not configured to generate smart
        /// hints.</returns>
        [Category("Data Entry Table Column")]
        [DefaultValue(false)]
        public bool SmartHintsEnabled
        {
            get
            {
                return _smartHintsEnabled;
            }

            set
            {
                _smartHintsEnabled = value;
            }
        }

        /// <summary>
        /// Specifies whether if the cells in the column will accept as input the
        /// <see cref="SpatialString"/> associated with an image swipe.
        /// </summary>
        /// <value><see langword="true"/> if the cells in the column will accept as input the
        /// <see cref="SpatialString"/> associated with an image swipe; otherwise,
        /// <see langword="false"/>.
        /// </value>
        [Category("Data Entry Table Column")]
        [DefaultValue(true)]
        public bool SupportsSwiping
        {
            get
            {
                return _supportsSwiping;
            }

            set
            {
                _supportsSwiping = value;
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
                column.ValidationQuery = this.ValidationQuery;
                column.ValidationErrorMessage = this.ValidationErrorMessage;
                column.UseComboBoxCells = this.UseComboBoxCells;
                column.AutoUpdateQuery = this.AutoUpdateQuery;
                column.TabStopMode = this.TabStopMode;
                column.PersistAttribute = this.PersistAttribute;
                column.ValidationCorrectsCase = this.ValidationCorrectsCase;
                column.ValidationIsCaseSensitive = this.ValidationIsCaseSensitive;
                column.RemoveNewLineChars = this.RemoveNewLineChars;
                column.SmartHintsEnabled = this.SmartHintsEnabled;
                column.SupportsSwiping = this.SupportsSwiping;

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
                    string[] autoCompleteValues = _validatorTemplate.GetAutoCompleteValues();

                    ExtractException.Assert("ELI25578", "Auto-complete query must be specified " +
                        "for ComboBox cells!", _inDesignMode || autoCompleteValues != null);
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
        void UpdateCellTemplate()
        {
            // Use a combo box template or text box template depending on the value of 
            // _useComboBoxCells.
            if (_useComboBoxCells)
            {
                DataEntryComboBoxCell cellTemplate = new DataEntryComboBoxCell();
                cellTemplate.ValidatorTemplate = (DataEntryValidator)_validatorTemplate.Clone();
                cellTemplate.RemoveNewLineChars = _removeNewLineChars;

                string[] autoCompleteValues = _validatorTemplate.GetAutoCompleteValues();
                if (autoCompleteValues != null)
                {
                    cellTemplate.Items.AddRange(autoCompleteValues);
                }

                base.CellTemplate = cellTemplate;
            }
            else
            {
                DataEntryTextBoxCell cellTemplate = new DataEntryTextBoxCell();
                cellTemplate.ValidatorTemplate = (DataEntryValidator)_validatorTemplate.Clone();
                cellTemplate.RemoveNewLineChars = _removeNewLineChars;

                base.CellTemplate = cellTemplate;
            }
        }
    }
}
