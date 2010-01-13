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
        static readonly string _OBJECT_NAME = typeof(DataEntryTableRow).ToString();

        #endregion Constants

        #region Fields

        /// <summary>
        /// The attribute whose value is associated with this row.
        /// </summary>
        IAttribute _attribute;

        /// <summary>
        /// The display name to be used for this row.
        /// </summary>
        string _name;

        /// <summary>
        /// The name subattributes to the table's primary attributes must have to be displayed in 
        /// this row.
        /// </summary>
        string _attributeName;

        /// <summary>
        /// The selection mode to use when multiple attributes are found which match the attribute 
        /// name for this row.
        /// </summary>
        MultipleMatchSelectionMode _multipleMatchSelectionMode =
            MultipleMatchSelectionMode.None;

        /// <summary>
        /// 
        /// </summary>
        string _formattingRuleFileName;

        /// <summary>
        /// The formatting rule to be used when processing text from image swipes in this row.
        /// </summary>
        IRuleSet _formattingRule;

        /// <summary>
        /// The object which will provide validation for cell data.
        /// </summary>
        DataEntryValidator _validator = new DataEntryValidator();

        /// <summary>
        /// A query which will cause value to automatically be updated using the values from other
        /// <see cref="IAttribute"/>'s and/or a database query.
        /// </summary>
        string _autoUpdateQuery;

        /// <summary>
        /// Specifies under what circumstances the row's attributes should serve as a tab stop.
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
        /// Specifies whether the cells in the row should be edited with a non-editable combo box.
        /// </summary>
        bool _useComboBoxCells;

        /// <summary>
        /// Specifies whether carriage return or new line characters will be replaced with spaces.
        /// </summary>
        bool _removeNewLineChars = true;

        /// <summary>
        /// Specifies whether the table will attempt to generate a hint using the intersection of 
        /// the row and column occupied by the specified attributes in this row.
        /// </summary>
        bool _smartHintsEnabled;

        /// <summary>
        /// Specifies whether the current instance is running in design mode
        /// </summary>
        bool _inDesignMode;

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
                LicenseUtilities.ValidateLicense(
                    LicenseIdName.DataEntryCoreComponents, "ELI24486", _OBJECT_NAME);
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
        /// <see cref="IDataEntryControl.ProcessSwipedText"/> for this row.
        /// </summary>
        /// <value>The filename of the <see cref="IRuleSet"/> to be used. Can be 
        /// <see langword="null"/> if no rule should be used.</value>
        /// <returns>The filename of the <see cref="IRuleSet"/> to be used. Can be 
        /// <see langword="null"/> if no rule has been specified.</returns>
        [Category("Data Entry Table Row")]
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
        /// </summary>
        /// <value>A regular expression the data entered in this row must match prior to being 
        /// saved. <see langword="null"/> to remove any existing validation pattern requirement.</value>
        /// <returns>A regular expression the data entered in this row must match prior to being 
        /// saved. <see langword="null"/> if there is no validation pattern set.</returns>
        [Category("Data Entry Table Row")]
        [DefaultValue(null)]
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
        [DefaultValue(null)]
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
        [DefaultValue(true)]
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
        /// Gets or sets whether validation lists will be checked for matches case-sensitively.
        /// </summary>
        /// <value><see langword="true"/> to validate against a validation list case-sensitively;
        /// <see langword="false"/> otherwise.</value>
        /// <returns><see langword="true"/> if validating against a validation list
        /// case-sensitively; <see langword="false"/> otherwise.</returns>
        [Category("Data Entry Table Row")]
        [DefaultValue(false)]
        public bool ValidationIsCaseSensitive
        {
            get
            {
                return _validator.CaseSensitive;
            }

            set
            {
                _validator.CaseSensitive = value;
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
        /// Specifies under what circumstances the <see cref="DataEntryTableRow"/>'s
        /// <see cref="IAttribute"/>s should serve as a tab stop.
        /// </summary>
        /// <value>A <see cref="TabStopMode"/> value indicating when the attributes should serve as a
        /// tab stop.</value>
        /// <returns>A <see cref="TabStopMode"/> value indicating when the attributes will serve as a
        /// tab stop.</returns>
        [Category("Data Entry Table Row")]
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
        [Category("Data Entry Table Row")]
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
                        MessageBox.Show(null, "Setting the parent attribute of a row as " +
                            " non-persistable will prevent all data in this table from being saved!",
                            "Warning", MessageBoxButtons.OK, MessageBoxIcon.Exclamation,
                            MessageBoxDefaultButton.Button1, 0);
                    }

                    _persistAttribute = value;
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI27089", ex);
                }
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
        [DefaultValue(false)]
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
        [Category("Data Entry Table Row")]
        [DefaultValue(true)]
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
        /// Specifies whether GetSpatialHint will attempt to generate a hint using the
        /// intersection of the row and column occupied by the specified <see cref="IAttribute"/>.
        /// </summary>
        /// <value><see langword="true"/> if the smart hints should be generated for this row
        /// when possible; <see langword="false"/> if the row should never attempt to generate
        /// smart hints.</value>
        /// <returns><see langword="true"/> if the row is configured to generate smart hints when
        /// possible; <see langword="false"/> if the row is not configured to generate smart
        /// hints.</returns>
        [Category("Data Entry Table Row")]
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
                row.ValidationQuery = this.ValidationQuery;
                row.ValidationErrorMessage = this.ValidationErrorMessage;
                row.UseComboBoxCells = this.UseComboBoxCells;
                row.AutoUpdateQuery = this.AutoUpdateQuery;
                row.TabStopMode = this.TabStopMode;
                row.PersistAttribute = this.PersistAttribute;
                row.ValidationCorrectsCase = this.ValidationCorrectsCase;
                row.ValidationIsCaseSensitive = this.ValidationIsCaseSensitive;
                row.RemoveNewLineChars = this.RemoveNewLineChars;
                row.SmartHintsEnabled = this.SmartHintsEnabled;

                return row;
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI25574", ex);

                return null;
            }
        }

        /// <summary>
        /// Called when the band is associated with a different <see cref="DataGridView"/>. This
        /// call initializes the list of items for the combo boxes, if necessary, using a 
        /// <see cref="DataEntryValidator"/>.</summary>
        protected override void OnDataGridViewChanged()
        {
            try
            {
                base.OnDataGridViewChanged();

                // If a valid grid has been specified and combo boxes are being used, initialize the
                // item list in each cell.
                if (base.DataGridView != null && _useComboBoxCells)
                {
                    string[] autoCompleteValues = _validator.GetAutoCompleteValues();

                    ExtractException.Assert("ELI25934", "Auto-complete query must be specified " +
                        "for ComboBox cells!", _inDesignMode || autoCompleteValues != null);

                    if (autoCompleteValues != null)
                    {
                        // Create a template to use in each cell.
                        DataEntryComboBoxCell template = new DataEntryComboBoxCell();
                        template.Items.AddRange(autoCompleteValues);

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
        void UpdateComboBoxItems()
        {
            // If an auto-complete list was specified, use it to populate the combo box items.
            string[] autoCompleteValues = _validator.GetAutoCompleteValues();
            if (autoCompleteValues != null)
            {
                foreach (DataEntryComboBoxCell cell in base.Cells)
                {
                    // Reseting the item list will clear the value. Preserve the original value.
                    string originalValue = cell.Value.ToString();

                    cell.Items.Clear();
                    cell.Items.AddRange(autoCompleteValues);

                    // Restore the original value
                    cell.Value = originalValue;
                }
            }
        }

        #endregion Private Members
    }
}
