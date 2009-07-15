using Extract.Licensing;
using Extract.Imaging.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Data;
using System.Drawing;
using System.Drawing.Design;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using UCLID_AFCORELib;
using UCLID_COMUTILSLib;
using UCLID_RASTERANDOCRMGMTLib;

namespace Extract.DataEntry
{
    /// <summary>
    /// A <see cref="IDataEntryControl"/> which allows the <see langref="string"/> value associated
    /// with an <see cref="IAttribute"/> to be viewed and changed to one of list of pre-defined
    /// values.
    /// </summary>
    public partial class DataEntryComboBox : ComboBox, IDataEntryControl, IRequiresErrorProvider
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        private static readonly string _OBJECT_NAME = typeof(DataEntryComboBox).ToString();

        #endregion Constants

        #region Fields

        /// <summary>
        /// The name used to identify the <see cref="IAttribute"/> to be  associated with the text 
        /// box.
        /// </summary>
        private string _attributeName;

        /// <summary>
        /// Used to specify the data entry control which is mapped to the parent of the attribute 
        /// to which the current combobox is to be mapped.
        /// </summary>
        private IDataEntryControl _parentDataEntryControl;

        /// <summary>
        /// The selection mode to use when multiple attributes are found which match the attribute 
        /// name for this control.
        /// </summary>
        private MultipleMatchSelectionMode _multipleMatchSelectionMode = 
            MultipleMatchSelectionMode.First;

        /// <summary>
        /// Specifies whether this control should allow input via image swipe.
        /// </summary>
        private bool _supportsSwiping = true;

        /// <summary>
        /// Specifies whether the control should remain disabled at all times.
        /// </summary>
        private bool _disabled;

        /// <summary>
        /// The attribute mapped to this control.
        /// </summary>
        private IAttribute _attribute;

        /// <summary>
        /// The domain of attributes to which this control's attribute belongs.
        /// </summary>
        private IUnknownVector _sourceAttributes;

        /// <summary>
        /// The filename of the rule file to be used to parse swiped data.
        /// </summary>
        private string _formattingRuleFileName;

        /// <summary>
        /// The formatting rule to be used when processing text from image swipes.
        /// </summary>
        private IRuleSet _formattingRule;

        /// <summary>
        /// The object which will provide validation for cell data.
        /// </summary>
        private DataEntryValidator _validator = new DataEntryValidator();

        /// <summary>
        /// The error provider to be used to indicate data validation problems to the user.
        /// </summary>
        ErrorProvider _errorProvider;

        /// <summary>
        /// A query which will cause value to automatically be updated using the values from other
        /// <see cref="IAttribute"/>'s and/or a database query.
        /// </summary>
        private string _autoUpdateQuery;

        /// <summary>
        /// A query which will cause the validation list to be updated using the values from other
        /// <see cref="IAttribute"/>'s and/or a database query.
        /// </summary>
        private string _validationQuery;

        /// <summary>
        /// Specifies whether tab should always stop on the combo box or whether it can be skipped
        /// if empty and valid.
        /// </summary>
        private bool _tabStopRequired = true;

        /// <summary>
        /// Specifies whether the current instance is running in design mode
        /// </summary>
        private bool _inDesignMode;

        /// <summary>
        /// Indicates whether the combo box is currently active.
        /// </summary>
        private bool _isActive;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new <see cref="DataEntryComboBox"/> instance.
        /// </summary>
        public DataEntryComboBox()
        {
            try
            {
                _inDesignMode = (LicenseManager.UsageMode == LicenseUsageMode.Designtime);

                // Load licenses in design mode
                if (_inDesignMode)
                {
                    // Load the license files from folder
                    LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());
                }

                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.FlexIndexCoreObjects, "ELI25534",
                    _OBJECT_NAME);

                InitializeComponent();

                _validator.ValidationListChanged += HandleValidationListChanged;

                // Initialize auto-complete mode if not using a drop-list.
                if (base.DropDownStyle != ComboBoxStyle.DropDownList)
                {
                    base.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                    base.AutoCompleteSource = AutoCompleteSource.CustomSource;
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI25535", ex);
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the name identifying the <see cref="IAttribute"/> to be associated with 
        /// the <see cref="DataEntryComboBox"/>.</summary>
        /// <value>Sets the name indentifying the <see cref="IAttribute"/> to be associated with 
        /// the <see cref="DataEntryComboBox"/>. Specifying <see langword="null"/> will make the
        /// text box a "dependent sibling" to its <see cref="ParentDataEntryControl"/> meaning 
        /// its attribute will share the same name as the control it is dependent upon, but 
        /// the specific attribute it displays will be dependent on the current selection in the 
        /// <see cref="ParentDataEntryControl"/>.
        /// </value>
        /// <returns>The name indentifying the <see cref="IAttribute"/> to be associated with the 
        /// <see cref="DataEntryComboBox"/>.</returns>
        [Category("Data Entry Combo Box")]
        public string AttributeName
        {
            get
            {
                return _attributeName;
            }

            set
            {
                // TODO: Assert that either ParentDataEntryControl or AttributeName must be non-null.

                _attributeName = value;
            }
        }

        /// <summary>
        /// Gets or sets the selection mode to use to find the mapped attribute for the
        /// <see cref="DataEntryComboBox"/>.
        /// </summary>
        /// <value>The selection mode to use to find the mapped attribute for the
        /// <see cref="DataEntryComboBox"/>.</value>
        /// <returns>The selection mode to use to find the mapped attribute for the
        /// <see cref="DataEntryComboBox"/>.</returns>
        [Category("Data Entry Combo Box")]
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
        /// split <see cref="SpatialString"/> content passed into <see cref="ProcessSwipedText"/>.
        /// </summary>
        /// <value>The filename of the <see cref="IRuleSet"/> to be used.</value>
        /// <returns>The filename of the <see cref="IRuleSet"/> to be used.</returns>
        [Category("Data Entry Combo Box")]
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
                    // If not in design mode and a formatting rule is specified, attempt to load an
                    // attribute finding rule.
                    if (!_inDesignMode && !string.IsNullOrEmpty(value))
                    {
                        _formattingRule = (IRuleSet) new RuleSetClass();
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
                    throw ExtractException.AsExtractException("ELI25536", ex);
                }
            }
        }

        /// <summary>
        /// Gets or set a regular expression the data entered in a control must match prior to being 
        /// saved.
        /// <para><b>Requirements</b></para>
        /// Cannot be specified at the same time <see cref="ValidationListFileName"/> is specified.
        /// </summary>
        /// <value>A regular expression the data entered in a control must match prior to being 
        /// saved. <see langword="null"/> to remove any existing validation pattern requirement.</value>
        /// <returns>A regular expression the data entered in a control must match prior to being 
        /// saved. <see langword="null"/> if there is no validation pattern set.</returns>
        [Category("Data Entry Combo Box")]
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
                    throw ExtractException.AsExtractException("ELI25579", ex);
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
                    throw ExtractException.AsExtractException("ELI25580", ex);
                }
            }
        }

        /// <summary>
        /// Gets or sets the name of a file containing a list of possible values the data entered in
        /// the <see cref="DataEntryComboBox"/> must match prior to being saved. 
        /// <para><b>Requirements</b></para>
        /// Cannot be specified at the same time <see cref="ValidationPattern"/> is specified.
        /// <para><b>Note</b></para>
        /// If the <see cref="DataEntryComboBox"/>'s value matches a value in the supplied list
        /// case-insensitively but not case-sensitively, the value will be modified to match the 
        /// casing in the list. If a value is specified in the list multiple times, the casing
        /// of the last entry will be used.
        /// </summary>
        /// <value>The name of a file containing list of values. <see langword="null"/> to remove
        /// any existing validation list requirement.</value>
        /// <returns>The name of a file containing list of values. <see langword="null"/> if there 
        /// is no validation list set.</returns>
        [Category("Data Entry Combo Box")]
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
                    throw ExtractException.AsExtractException("ELI25539", ex);
                }
            }

            set
            {
                try
                {
                    _validator.ValidationListFileName = value;

                    // If the item list wasn't explicitly specified via Items, initialize it using
                    // the members of the validation list.
                    if (!_inDesignMode && value != null && base.Items.Count == 0)
                    {
                        UpdateComboBoxItemsViaValidationList();
                    }
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI25540", ex);
                }
            }
        }

        /// <summary>
        /// Gets or sets a query which will cause the combo box's validation list to be automatically
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
        [Category("Data Entry Combo Box")]
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
        [Category("Data Entry Combo Box")]
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
        /// Gets or set the error message that should be displayed for an error provider icon on
        /// validation failure. If unspecified, a default of "Bad value" will be used.
        /// </summary>
        /// <value>The tooltip text that should be displayed for an error provider icon on
        /// validation failure.</value>
        /// <returns>The tooltip text that should be displayed for an error provider icon on
        /// validation failure.</returns>
        [Category("Data Entry Combo Box")]
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
                    throw ExtractException.AsExtractException("ELI25541", ex);
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
                    throw ExtractException.AsExtractException("ELI25542", ex);
                }
            }
        }

        /// <summary>
        /// Gets or sets a query which will cause the combo box's value to automatically be updated
        /// using values from other <see cref="IAttribute"/>'s and/or a database query. Values
        /// to be used from other <see cref="IAttribute"/>'s values should be inserted into the
        /// query using curly braces. For example, to have the value reflect the value of a
        /// sibling attribute named "Source", the query would be specified as "{../Source}".
        /// If the query matches SQL syntax it will be executed against the
        /// <see cref="DataEntryControlHost"/>'s database. Every time an attribute specified in the
        /// query is modified, this query will be re-evaluated and used to update the value.
        /// </summary>
        /// <value>A query which will cause value to automatically be updated using values
        /// from other <see cref="IAttribute"/>'s and/or a database query.</value>
        /// <returns>A query which will cause value to automatically be updated using values
        /// from other <see cref="IAttribute"/>'s and/or a database query.</returns>
        [Category("Data Entry Combo Box")]
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
        /// Specifies whether tab should always stop on this field or whether it can be skipped
        /// if it is empty and valid.
        /// </summary>
        /// <value><see langword="true"/> if the field should always be a tabstop,
        /// <see langword="false"/> if the field can be skipped if empty and valid</value>
        /// <returns><see langword="true"/> if the field is always be a tabstop,
        /// <see langword="false"/> if the field will be skipped if empty and valid</returns>
        [Category("Data Entry Combo Box")]
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

        #endregion Properties

        #region Overrides

        /// <summary>
        /// Handles changes to the text of the control by updating the underlying
        /// <see cref="IAttribute"/>.
        /// </summary>
        /// <param name="e">A <see cref="EventArgs"/> that contains the event data.</param>
        protected override void OnTextChanged(EventArgs e)
        {
            try
            {
                base.OnTextChanged(e);

                // Only apply data if the combo box is currently mapped.
                if (_attribute != null)
                {
                    AttributeStatusInfo.SetValue(_attribute, this.Text, true, false);
                }

                // Display a validation error icon if needed.
                if (_errorProvider != null)
                {
                    _errorProvider.SetError(this,
                        Validate(false) ? "" : _validator.ValidationErrorMessage);
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI23921", ex);
            }
        }

        /// <summary>
        /// Raises the SelectedIndexChanged event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs"/> associated with the event.</param>
        protected override void OnSelectedIndexChanged(EventArgs e)
        {
            try
            {
                base.OnSelectedItemChanged(e);

                // Only apply data if the combo box is currently mapped.
                if (_attribute != null)
                {
                    AttributeStatusInfo.SetValue(_attribute, this.Text, true, true);
                }

                // Display or clear a validation error icon if needed.
                if (_errorProvider != null)
                {
                    _errorProvider.SetError(this,
                        Validate(false) ? "" : _validator.ValidationErrorMessage);
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI25551", ex);
            }
        }

        /// <summary>
        /// Handles the case the focus has changed to another control.  At this point, attempt to
        /// use validation to correct capitialization differences with a validation list.
        /// </summary>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        protected override void OnLostFocus(EventArgs e)
        {
            try
            {
                base.OnLostFocus(e);

                // If a validation list is supplied, this will correct capitalization differences
                // with a list item.
                Validate(true);
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI25543", ex);
            }
        }

        /// <summary>
        /// Raises the <see cref="Control.KeyDown"/> event.
        /// </summary>
        /// <param name="e">A <see cref="KeyEventArgs"/> containing the event data.</param>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            try
            {
                // [DataEntry:295]
                // Ctrl + A is not implemented by the base ComboBox class.
                if (e.KeyCode == Keys.A && e.Control)
                {
                    base.SelectAll();

                    e.Handled = true;
                }

                base.OnKeyDown(e);
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI25987", ex);
            }
        }

        /// <summary>
        /// Gets or sets the text associated with this <see cref="DataEntryTextBox"/>.
        /// </summary>
        /// <value>The text associated with this <see cref="DataEntryTextBox"/>.</value>
        /// <returns>The text associated with this <see cref="DataEntryTextBox"/>.</returns>
        public override string Text
        {
            get
            {
                try
                {
                    // Add carriage returns back so that caller will not know they were replaced
                    // internally.
                    return base.Text.Replace(DataEntryMethods._CRLF_REPLACEMENT, "\r\n");
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI26011", ex);
                }
            }

            set
            {
                try
                {
                    // Internally replace carriage returns or line feeds with no break spaces to
                    // prevent the unprintable "boxes" from appearing.
                    value = value.Replace("\r\n", DataEntryMethods._CRLF_REPLACEMENT);

                    base.Text = value;
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI26012", ex);
                }
            }
        }

        #endregion Overrides

        #region IDataEntryControl Events

        /// <summary>
        /// Fired whenever the set of selected or active <see cref="IAttribute"/>(s) for a control
        /// changes. This can occur as part of the <see cref="PropagateAttributes"/> event, when
        /// new attribute(s) are created via a swipe or when a new element of the control becomes 
        /// active.
        /// </summary>
        public event EventHandler<AttributesSelectedEventArgs> AttributesSelected;

        /// <summary>
        /// Fired to request that the and <see cref="IAttribute"/> or <see cref="IAttribute"/>(s) be
        /// propagated to any dependent controls.  This will be in response to an 
        /// <see cref="IAttribute"/> having been modified (ie, via a swipe or loading a document). 
        /// The event will provide the updated <see cref="IAttribute"/>(s) to registered listeners.
        /// </summary>
        public event EventHandler<PropagateAttributesEventArgs> PropagateAttributes;

        /// <summary>
        /// Fired when the text box has been manipulated in such a way that swiping should be
        /// either enabled or disabled.
        /// </summary>
        public event EventHandler<SwipingStateChangedEventArgs> SwipingStateChanged
        {
            // Since this event is not currently used by this class but is needed by the 
            // IDataEntryControl interface, define it with an empty implementation to prevent
            // "unused" warnings during compile.
            add { }
            remove { }
        }

        /// <summary>
        /// Raised by a control whenever data is being dragged to query dependent controls on whether
        /// they would be able to handle the dragged data if it was dropped.
        /// </summary>
        public event EventHandler<QueryDraggedDataSupportedEventArgs> QueryDraggedDataSupported
        {
            // Since this event is not currently used by this class but is needed by the 
            // IDataEntryControl interface, define it with an empty implementation to prevent
            // "unused" warnings during compile.
            add { }
            remove { }
        }

        #endregion IDataEntryControl Events

        #region IDataEntryControl Properties

        /// <summary>
        /// If the <see cref="DataEntryComboBox"/> is not intended to operate on root-level
        /// data, this property must be used to specify the <see cref="IDataEntryControl"/> which is
        /// mapped to the parent of the <see cref="IAttribute"/>(s) to which the current 
        /// <see cref="DataEntryComboBox"/> is to be mapped.  The specified 
        /// <see cref="IDataEntryControl"/> must be contained in the same 
        /// <see cref="DataEntryControlHost"/> as this <see cref="DataEntryComboBox"/>. If the 
        /// <see cref="DataEntryComboBox"/> is to be mapped to a root-level
        /// <see cref="IAttribute"/>, this property must be set to <see langref="null"/>.</summary>
        /// <seealso cref="IDataEntryControl"/>
        [Category("Data Entry Control")]
        public IDataEntryControl ParentDataEntryControl
        {
            get
            {
                return _parentDataEntryControl;
            }

            set
            {
                _parentDataEntryControl = value;
            }
        }

        /// <summary>
        /// If <see langword="true"/>, the text box will accept as input the 
        /// <see cref="SpatialString"/> associated with an image swipe.  If <see langword="false"/>, 
        /// the text box will accept swiped input and the swiping tool should be disabled while the 
        /// text box is active.
        /// </summary>
        [Category("Data Entry Control")]
        public bool SupportsSwiping
        {
            get
            {
                if (_inDesignMode)
                {
                    return _supportsSwiping;
                }
                else
                {
                    // If control isn't enabled or no attribute has been mapped, swiping is not
                    // currently supported.
                    return (_supportsSwiping && base.Enabled && _attribute != null);
                }
            }

            set
            {
                _supportsSwiping = value;
            }
        }

        /// <summary>
        /// Gets or sets whether the control should remain disabled at all times.
        /// <para><b>Note</b></para>
        /// If disabled, mapped data will not be validated.
        /// </summary>
        /// <value><see langword="true"/> if the control should remain disabled,
        /// <see langword="false"/> otherwise.</value>
        /// <returns><see langword="true"/> if the control will remain disabled,
        /// <see langword="false"/> otherwise.</returns>
        [Category("Data Entry Control")]
        public bool Disabled
        {
            get
            {
                return _disabled;
            }

            set
            {
                _disabled = value;
            }
        }

        #endregion IDataEntryControl Properties

        #region IDataEntryControl Methods

        /// <summary>
        /// Specifies the domain of <see cref="IAttribute"/>s from which the 
        /// <see cref="IDataEntryControl"/> should find the <see cref="IAttribute"/> 
        /// to which it should be mapped (based on the <see cref="AttributeName"/> property). 
        /// If there are multiple attributes, the <see cref="MultipleMatchSelectionMode"/> property
        /// will be used to decide to which <see cref="IAttribute"/> it should map 
        /// itself.
        /// </summary>
        /// <param name="sourceAttributes">The <see cref="IUnknownVector"/> instance of 
        /// <see cref="IAttribute"/>s from which the <see cref="DataEntryComboBox"/> 
        /// should find its corresponding <see cref="IAttribute"/>.  Can be an empty vector, but 
        /// must not be <see langword="null"/>.</param>
        /// <seealso cref="IDataEntryControl"/>
        public void SetAttributes(IUnknownVector sourceAttributes)
        {
            try
            {
                // [DataEntry:298]
                // If the combo box has nothing to propagate, disable it since any data entered
                // would not be mapped into the attribute hierarchy.
                // Also, prevent it from being enabled if explicitly disabled via the
                // IDataEntryControl interface.
                base.Enabled = (sourceAttributes != null && !_disabled);

                if (sourceAttributes == null)
                {
                    // If the source attribute is null, clear existing data and do not attempt to
                    // map to a new attribute.
                    _attribute = null;
                    this.Text = "";
                }
                else
                {
                    // Attempt to find a mapped attribute from the provided vector.  Create a new 
                    // attribute if no such attribute can be found.
                    _attribute = DataEntryMethods.InitializeAttribute(_attributeName,
                        _multipleMatchSelectionMode, !string.IsNullOrEmpty(_attributeName),
                        sourceAttributes, null, this, 0, false, _tabStopRequired, _validator, 
                        _autoUpdateQuery, _validationQuery);

                    if (base.Visible)
                    {
                        // Mark the attribute as visible if the combobox is visible
                        AttributeStatusInfo.MarkAsViewable(_attribute, true);

                        // [DataEntry:327] If this control is active, ensure the attribute is marked as viewed.
                        if (_isActive)
                        {
                            AttributeStatusInfo.MarkAsViewed(_attribute, true);
                        }
                    }

                    // If the attribute has not been viewed, apply bold font.  Otherwise, use
                    // regular font.
                    bool hasBeenViewed = AttributeStatusInfo.HasBeenViewed(_attribute, false);
                    if (base.Font.Bold == hasBeenViewed)
                    {
                        base.Font = new Font(base.Font,
                            hasBeenViewed ? FontStyle.Regular : FontStyle.Bold);
                    }

                    // If the text value is not editable (DropDownList style), use the first value in
                    // the list as the default value.  Otherwise, allow the value to initialize to
                    // blank.
                    if (base.Items.Count > 0 && base.DropDownStyle == ComboBoxStyle.DropDownList &&
                        base.FindStringExact(_attribute.Value.String) == ListBox.NoMatches)
                    {
                        base.SelectedIndex = 0;
                    }
                }

                _sourceAttributes = sourceAttributes;

                // Update text value of this control and raise the events that need to be raised
                // in conjunction with an attribute change.
                OnAttributeChanged();
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI25545", ex);
            }
        }

        /// <summary>
        /// Activates or inactivates the <see cref="DataEntryComboBox"/>.
        /// </summary>
        /// <param name="setActive">If <see langref="true"/>, the <see cref="DataEntryComboBox"/>
        /// should visually indicate that it is active. If <see langref="false"/> the 
        /// <see cref="DataEntryComboBox"/> should not visually indicate that it is active.
        /// </param>
        /// <param name="color">The <see cref="Color"/> that should be used to indicate active 
        /// status (unused if setActive is <see langword="false"/>).</param>
        /// <seealso cref="IDataEntryControl"/>
        public void IndicateActive(bool setActive, Color color)
        {
            try
            {
                // The combo box can be active only if it is mapped to an attribute.
                _isActive = (setActive && _attribute != null);

                // Change the background color to display active status.
                if (_isActive)
                {
                    base.BackColor = color;

                    // Mark the attribute as having been viewed and update the attribute's status
                    // info accordingly.
                    AttributeStatusInfo.MarkAsViewed(_attribute, true);
                    if (base.Font.Bold)
                    {
                        base.Font = new Font(base.Font, FontStyle.Regular);
                    }
                }
                else
                {
                    base.ResetBackColor();
                }

                this.Invalidate();
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI25546", ex);
            }
        }

        /// <summary>
        /// This method has no effect for a <see cref="DataEntryComboBox"/> except to validate that
        /// the specified <see cref="IAttribute"/> is mapped.
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> whose sub-attributes should be
        /// propagated to any child controls. If <see langword="null"/>, the currently selected
        /// <see cref="IAttribute"/> should be repropagated if there is a single attribute.
        /// <para><b>Requirements</b></para>If non-<see langword="null"/>, the specified 
        /// <see cref="IAttribute"/> must be known to be mapped to the 
        /// <see cref="IDataEntryControl"/>.</param>
        /// <param name="selectAttribute">If <see langword="true"/>, the specified 
        /// <see cref="IAttribute"/> will also be selected within the <see cref="IDataEntryControl"/>.
        /// If <see langword="false"/>, the previous selection will remain even if a different
        /// <see cref="IAttribute"/> was propagated.
        /// </param>
        /// <seealso cref="IDataEntryControl"/>
        public void PropagateAttribute(IAttribute attribute, bool selectAttribute)
        {
            ExtractException.Assert("ELI25547", "Unexpected attribute!",
                attribute == null || attribute == _attribute);
        }

        /// <summary>
        /// Refreshes the specified <see cref="IAttribute"/>'s value to the text box.
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> whose value should be refreshed.
        /// </param>
        public virtual void RefreshAttribute(IAttribute attribute)
        {
            try
            {
                if (_attribute == attribute)
                {
                    this.Text = (_attribute.Value != null) ? _attribute.Value.String : "";

                    // In case the value itself didn't change but the validation list did, explicitly
                    // check validation here.
                    if (_errorProvider != null)
                    {
                        _errorProvider.SetError(this,
                            Validate(false) ? "" : _validator.ValidationErrorMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI26126", ex);
            }
        }

        /// <summary>
        /// Processes the supplied <see cref="SpatialString"/> as input.
        /// </summary>
        /// <param name="swipedText">The <see cref="SpatialString"/> representing the
        /// recognized text in the swiped image area.</param>
        /// <seealso cref="IDataEntryControl"/>
        public void ProcessSwipedText(SpatialString swipedText)
        {
            try
            {
                // If control isn't enabled or no attribute has been mapped, swiping is not
                // currently supported.
                if (!base.Enabled || _attribute == null)
                {
                    return;
                }

                if (_formattingRule != null)
                {
                    // Format the data into attribute(s) using the rule.
                    IUnknownVector formattedData = DataEntryMethods.RunFormattingRule(
                        _formattingRule, swipedText);

                    if (formattedData.Size() > 0)
                    {
                        // Find the appropriate attribute (if there is one) from the rule's output.
                        IAttribute attribute = DataEntryMethods.InitializeAttribute(_attributeName,
                            _multipleMatchSelectionMode, !string.IsNullOrEmpty(_attributeName),
                            formattedData, null, this, 0, false, _tabStopRequired, _validator, 
                            _autoUpdateQuery, _validationQuery);

                        // [DataEntry:251] Swap out the existing attribute in the overall attribute
                        // heirarchy (keeping attribute ordering the same as it was).
                        if (DataEntryMethods.InsertOrReplaceAttribute(
                            _sourceAttributes, attribute, _attribute, null))
                        {
                            AttributeStatusInfo.DeleteAttribute(_attribute);
                        }

                        _attribute = attribute;

                        // [DataEntry:258] The new attribute needs to be marked as viewable.
                        AttributeStatusInfo.MarkAsViewable(_attribute, true);

                        // [DataEntry:327] If this control is active, ensure the attribute is marked as viewed.
                        if (_isActive)
                        {
                            AttributeStatusInfo.MarkAsViewed(_attribute, true);
                        }
                    }
                    else
                    {
                        // If the rules did not find anything, go ahead and use the swiped text
                        // as the attribute value.
                        _attribute.Value = swipedText;

                        // Consider the attribute un-accepted after a swipe.
                        AttributeStatusInfo.AcceptValue(_attribute, false);
                    }
                }
                else
                {
                    // If there is no formatting rule specified, simply assign the swiped text
                    // as the current attribute's value.
                    _attribute.Value = swipedText;

                    // Consider the attribute un-accepted after a swipe.
                    AttributeStatusInfo.AcceptValue(_attribute, false);
                }

                // Update text value of this control and raise the events that need to be raised
                // in conjunction with an attribute change.
                OnAttributeChanged();
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI25548", ex);
            }
        }

        /// <summary>
        /// Any data that was cached should be cleared;  This is called when a document is unloaded.
        /// If controls fail to clear COM objects, errors may result if that data is accessed when
        /// a subsequent document is loaded.
        /// </summary>
        public void ClearCachedData()
        {
            // Nothing to do.
        }

        #endregion IDataEntryControl Methods

        #region IDataEntryControl Event Handlers

        /// <summary>
        /// Handles the case that this <see cref="IDataEntryControl"/>'s 
        /// <see cref="ParentDataEntryControl"/> has requested that a new <see cref="IAttribute"/> 
        /// be propagated.  The <see cref="DataEntryComboBox"/> will re-map its control appropriately.
        /// The <see cref="DataEntryComboBox"/> will mark the <see cref="AttributeStatusInfo"/> instance 
        /// associated with any <see cref="IAttribute"/> it propagates as propagated.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">An <see cref="PropagateAttributesEventArgs"/> that contains the event data.
        /// </param>
        /// <seealso cref="IDataEntryControl"/>
        public void HandlePropagateAttributes(object sender, PropagateAttributesEventArgs e)
        {
            try
            {
                // An attribute can be mapped if there is one and only one attribute to be
                // propagated.
                if (e.Attributes != null && e.Attributes.Size() == 1)
                {
                    base.Enabled = !_disabled;

                    // This is a dependent child to the sender. Re-map this control using the
                    // updated attribute's children.
                    IAttribute propagatedAttribute = (IAttribute)e.Attributes.At(0);

                    // If we found a single attribute to use for mapping, mark the attribute
                    // as propagated.
                    AttributeStatusInfo.MarkAsPropagated(propagatedAttribute, true, false);

                    // If an attribute name is specified, this is a "child" control intended to act on
                    // a sub-attribute of the provided attribute. If no attribute name is specified,
                    // this is a "sibling" control likely intended to display details of the currently
                    // selected attribute in a multi-selection control.
                    if (!string.IsNullOrEmpty(_attributeName))
                    {
                        SetAttributes(propagatedAttribute.SubAttributes);
                    }
                    else
                    {
                        // This is a dependent sibling to the sender. Re-map this control using the
                        // updated attribute itself.
                        SetAttributes(e.Attributes);
                    }
                }
                else
                {
                    // If there is more than one parent attribute or no parent attributes, the combo
                    // box cannot be mapped and should propagate null so all dependent controls are
                    // unmapped.
                    SetAttributes(null);
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI25549", ex);
                ee.AddDebugData("Event data", e, false);
                throw ee;
            }
        }

        #endregion IDataEntryControl Event Handlers

        #region IRequiresErrorProvider Members

        /// <summary>
        /// Specifies the standard <see cref="ErrorProvider"/> that should be used to 
        /// display data validation errors.
        /// </summary>
        /// <param name="errorProvider">The standard <see cref="ErrorProvider"/> that should be 
        /// used to display data validation errors.</param>
        public void SetErrorProvider(ErrorProvider errorProvider)
        {
            _errorProvider = errorProvider;
        }

        #endregion IRequiresErrorProvider Members

        #region Private Members

        /// <summary>
        /// Updates the text value of this control and raise the events that need to be raised
        /// in conjunction with an attribute change (<see cref="AttributesSelected"/> and
        /// <see cref="PropagateAttributes"/>).
        /// </summary>
        private void OnAttributeChanged()
        {
            // Display the attribute text.
            this.Text = (_attribute != null && _attribute.Value != null)
                ? _attribute.Value.String : "";

            // Display a validation error icon if needed.
            if (_errorProvider != null)
            {
                _errorProvider.SetError(this,
                    Validate(true) ? "" : _validator.ValidationErrorMessage);
            }

            // Raise the AttributesSelected event to signal that the spatial information
            // associated with this control has changed.
            OnAttributesSelected();

            // Raise the PropagateAttributes event so that any descendents of this data controlc can
            // re-map themselves.
            OnPropagateAttributes();
        }

        /// <summary>
        /// Raises the <see cref="AttributesSelected"/> event.
        /// </summary>
        private void OnAttributesSelected()
        {
            if (this.AttributesSelected != null)
            {
                AttributesSelected(this,
                    new AttributesSelectedEventArgs(DataEntryMethods.AttributeAsVector(_attribute),
                        false, true));
            }
        }

        /// <summary>
        /// Raises the <see cref="PropagateAttributes"/> event.
        /// </summary>
        private void OnPropagateAttributes()
        {
            if (this.PropagateAttributes != null)
            {
                if (_attribute == null)
                {
                    // If the combo box is not currently mapped to an attribute, it should propagate
                    // null so all dependent controls are unmapped.
                    PropagateAttributes(this,
                        new PropagateAttributesEventArgs(null));
                }
                else
                {
                    // Propagate the mapped attribute.
                    PropagateAttributes(this,
                        new PropagateAttributesEventArgs(
                            DataEntryMethods.AttributeAsVector(_attribute)));
                }
            }
            else if (_attribute != null)
            {
                // If there are no dependent controls registered to receive this event, consider
                // the attribute propagated.
                AttributeStatusInfo.MarkAsPropagated(_attribute, true, true);
            }
        }

        /// <summary>
        /// Tests to see if the control's data fails to match any validation requirements it has.
        /// <para><b>NOTE:</b></para>
        /// If the items used to populate the combo box are not specified explicitly via the
        /// Items property, the items from the validation list (if
        /// provided) will be used to populate the combo box.
        /// </summary>
        /// <param name="correctCase">If <see langword="true"/> and a 
        /// <see cref="ValidationListFileName"/> has been configured, if the 
        /// <see cref="DataEntryComboBox"/>'s value matches a value in the supplied list  
        /// case-insensitively but not case-sensitively, the value will be modified to match 
        /// the casing in the supplied list.</param>
        /// <returns>If throwException is <see langword="false"/> the method will return
        /// <see langword="true"/> if the control either has no validation requirements or 
        /// the data it contains meets the requirements or <see langword="false"/>
        /// otherwise.</returns>
        private bool Validate(bool correctCase)
        {
            // If there is no mapped attribute or the control is disabled, the data cannot be invalid.
            if (_disabled || _attribute == null)
            {
                return true;
            }

            string value = this.Text;

            // Test to see if the data is valid.
            bool dataIsValid = _validator.Validate(ref value, _attribute, false);

            // If the data is valid, correctCase is true, and the validator updated the value with
            // new casing, apply the updated value to both the control and underlying attribute.
            if (dataIsValid && correctCase && value != this.Text)
            {
                this.Text = value;
                AttributeStatusInfo.SetValue(_attribute, value, false, false);
            }

            return dataIsValid;
        }

        /// <summary>
        /// Handles the case that the validation list was changed so that the combo box items
        /// can be updated to reflect the new list.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        private void HandleValidationListChanged(object sender, EventArgs e)
        {
            try
            {
                UpdateComboBoxItemsViaValidationList();
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI26161", ex);
                ee.AddDebugData("Event data", e, false);
                throw ee;
            }
        }

        /// <summary>
        /// Updates the items in the ComboBox list to reflect the current values in the
        /// validation list.
        /// </summary>
        private void UpdateComboBoxItemsViaValidationList()
        {
            string[] validationListValues = null;

            validationListValues = _validator.GetValidationListValues();

            if (validationListValues != null)
            {
                // Reseting the item list will clear the value. Preserve the original value.
                string originalValue = this.Text;

                // Auto-complete is supported unless the DropDownStyle is DropDownList
                if (base.DropDownStyle != ComboBoxStyle.DropDownList)
                {
                    base.AutoCompleteCustomSource.Clear();
                    base.AutoCompleteCustomSource.AddRange(validationListValues);
                }

                base.Items.Clear();
                base.Items.AddRange(validationListValues);

                // Restore the original value
                this.Text = originalValue;
            }
        }

        #endregion Private Members
    }
}
