using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Linq;
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
    public partial class DataEntryComboBox : LuceneComboBox, IDataEntryTextControl
    {
        // The selection mode to use when multiple attributes are found which match the attribute 
        // name for this control.
        MultipleMatchSelectionMode _multipleMatchSelectionMode =
            MultipleMatchSelectionMode.None;

        // Specifies whether this control should allow input via image swipe.
        bool _supportsSwiping = true;

        // The filename of the rule file to be used to parse swiped data.
        string _formattingRuleFileName;

        // The template object to be used as a model for per-attribute validation objects.
        readonly DataEntryValidator _validatorTemplate = new DataEntryValidator();

        // Indicates whether the combo box is currently active.
        bool _isActive;

        // The active FontStyle for the control.
        FontStyle _fontStyle;

        /// <summary>
        /// Initializes a new <see cref="DataEntryComboBox"/> instance.
        /// </summary>
        public DataEntryComboBox() : base()
        {
            try
            {
                InitializeComponent();

                _fontStyle = Font.Style;

                LastSelectionStart = SelectionStart;
                LastSelectionLength = SelectionLength;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI25535", ex);
            }
        }

        /// <summary>
        /// Gets or sets the name identifying the <see cref="IAttribute"/> to be associated with 
        /// the <see cref="DataEntryComboBox"/>.</summary>
        /// <value>Sets the name identifying the <see cref="IAttribute"/> to be associated with 
        /// the <see cref="DataEntryComboBox"/>. Specifying <see langword="null"/> will make the
        /// text box a "dependent sibling" to its <see cref="ParentDataEntryControl"/> meaning 
        /// its attribute will share the same name as the control it is dependent upon, but 
        /// the specific attribute it displays will be dependent on the current selection in the 
        /// <see cref="ParentDataEntryControl"/>.
        /// </value>
        /// <returns>The name identifying the <see cref="IAttribute"/> to be associated with the 
        /// <see cref="DataEntryComboBox"/>.</returns>
        [Category("Data Entry Combo Box")]
        public string AttributeName { get; set; }

        /// <summary>
        /// Gets or sets the selection mode to use to find the mapped attribute for the
        /// <see cref="DataEntryComboBox"/>.
        /// </summary>
        /// <value>The selection mode to use to find the mapped attribute for the
        /// <see cref="DataEntryComboBox"/>.</value>
        /// <returns>The selection mode to use to find the mapped attribute for the
        /// <see cref="DataEntryComboBox"/>.</returns>
        [Category("Data Entry Combo Box")]
        [DefaultValue(MultipleMatchSelectionMode.None)]
        public MultipleMatchSelectionMode MultipleMatchSelectionMode
        {
            get
            {
                return _multipleMatchSelectionMode;
            }

            set
            {
                try
                {
                    if (value != _multipleMatchSelectionMode)
                    {
                        ExtractException.Assert("ELI37369",
                            "Invalid MultipleMatchSelectionMode for a combo box.",
                            value != MultipleMatchSelectionMode.All);

                        _multipleMatchSelectionMode = value;
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI37372");
                }
            }
        }

        /// <summary>
        /// Specifies the filename of an <see cref="IRuleSet"/> that should be used to reformat or
        /// split <see cref="SpatialString"/> content passed into <see cref="ProcessSwipedText"/>.
        /// </summary>
        /// <value>The filename of the <see cref="IRuleSet"/> to be used.</value>
        /// <returns>The filename of the <see cref="IRuleSet"/> to be used.</returns>
        [Category("Data Entry Combo Box")]
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
        /// </summary>
        /// <value>A regular expression the data entered in a control must match prior to being 
        /// saved. <see langword="null"/> to remove any existing validation pattern requirement.</value>
        /// <returns>A regular expression the data entered in a control must match prior to being 
        /// saved. <see langword="null"/> if there is no validation pattern set.</returns>
        [Category("Data Entry Combo Box")]
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
                    throw ExtractException.AsExtractException("ELI25579", ex);
                }
            }

            set
            {
                try
                {
                    _validatorTemplate.ValidationPattern = value;
                }
                catch (ExtractException ex)
                {
                    throw ExtractException.AsExtractException("ELI25580", ex);
                }
            }
        }

        /// <summary>
        /// Gets or sets a query which will cause the combo box's validation list to be automatically
        /// updated using values from other <see cref="IAttribute"/>'s and/or a database query.
        /// Every time an attribute specified in the query is modified, this query will be 
        /// re-evaluated and used to update the validation list.
        /// </summary>
        /// <value>A query which will cause the validation list to be automatically updated using
        /// values from other <see cref="IAttribute"/>'s and/or a database query.</value>
        /// <returns>A query being used to automatically update the validation list using values
        /// from other <see cref="IAttribute"/>'s and/or a database query.</returns>
        [Category("Data Entry Combo Box")]
        [Editor("System.ComponentModel.Design.MultilineStringEditor, System.Design", typeof(UITypeEditor)), Localizable(true)]
        [DefaultValue(null)]
        public string ValidationQuery { get; set; }

        /// <summary>
        /// Gets or sets whether a value that matches a validation list item case-insensitively but
        /// not case-sensitively will be changed to match the validation list value.
        /// </summary>
        /// <value><see langword="true"/> if values should be modified to match the case of list items,
        /// <see langword="false"/> if case-insensitive matches should be left as-is.</value>
        /// <returns><see langword="true"/> if values will be modified to match the case of list items,
        /// <see langword="false"/> if case-insensitive matches will be left as-is.</returns>
        [Category("Data Entry Combo Box")]
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
            }
        }

        /// <summary>
        /// Gets or sets whether validation lists will be checked for matches case-sensitively.
        /// </summary>
        /// <value><see langword="true"/> to validate against a validation list case-sensitively;
        /// <see langword="false"/> otherwise.</value>
        /// <returns><see langword="true"/> if validating against a validation list
        /// case-sensitively; <see langword="false"/> otherwise.</returns>
        [Category("Data Entry Combo Box")]
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
                    return _validatorTemplate.ValidationErrorMessage;
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
                    _validatorTemplate.ValidationErrorMessage = value;
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI25542", ex);
                }
            }
        }

        /// <summary>
        /// Gets or sets a query which will cause the text box's value to automatically be updated
        /// using values from other <see cref="IAttribute"/>'s and/or a database query". Every time
        /// an attribute specified in the query is modified, this query will be re-evaluated and
        /// used to update the value.
        /// </summary>
        /// <value>A query which will cause value to automatically be updated using values
        /// from other <see cref="IAttribute"/>'s and/or a database query.</value>
        /// <returns>A query which will cause value to automatically be updated using values
        /// from other <see cref="IAttribute"/>'s and/or a database query.</returns>
        [Category("Data Entry Combo Box")]
        [Editor("System.ComponentModel.Design.MultilineStringEditor, System.Design", typeof(UITypeEditor)), Localizable(true)]
        [DefaultValue(null)]
        public string AutoUpdateQuery { get; set; }

        /// <summary>
        /// Specifies under what circumstances the <see cref="DataEntryComboBox"/>'s
        /// <see cref="IAttribute"/> should serve as a tab stop.
        /// </summary>
        /// <value>A <see cref="TabStopMode"/> value indicating when the attribute should serve as a
        /// tab stop.</value>
        /// <returns>A <see cref="TabStopMode"/> value indicating when the attribute will serve as a
        /// tab stop.</returns>
        [Category("Data Entry Combo Box")]
        [DefaultValue(TabStopMode.Always)]
        public TabStopMode TabStopMode { get; set; } = TabStopMode.Always;

        /// <summary>
        /// Gets or sets whether the mapped <see cref="IAttribute"/>'s value should be saved.
        /// </summary>
        /// <value><see langword="true"/> to save the attribute's value; <see langword="false"/>
        /// otherwise.
        /// </value>
        /// <returns><see langword="true"/> if the attribute's value will be saved;
        /// <see langword="false"/> otherwise.</returns>
        [Category("Data Entry Combo Box")]
        [DefaultValue(true)]
        public bool PersistAttribute { get; set; } = true;

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
        [Category("Data Entry Combo Box")]
        [DefaultValue(true)]
        public bool RemoveNewLineChars { get; set; } = true;

        /// <summary>
        /// The attribute mapped to this control.
        /// </summary>
        public IAttribute Attribute { get; private set; }

        /// <summary>
        /// The last noted selection start.
        /// </summary>
        public int LastSelectionStart { get; private set; }

        /// <summary>
        /// The last noted selection length.
        /// </summary>
        public int LastSelectionLength { get; private set; }

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

                // Only apply data if the combo box is currently mapped
                if (Attribute != null)
                {
                    AttributeStatusInfo.SetValue(Attribute, Text, true, false);
                }

                // Check for selection change as a result of the text change.
                ProcessSelectionChange();

                // Display a validation error icon if needed.
                UpdateValidation(Validate(false, out var _));
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
                base.OnSelectedIndexChanged(e);

                // Only apply data if the combo box is currently mapped.
                if (Attribute != null)
                {
                    AttributeStatusInfo.SetValue(Attribute, Text, true, true);
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI25551", ex);
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
                // If the delete key is pressed while all text is selected or when using
                // DropDownList, delete spatial info.
                if (e.KeyCode == Keys.Delete
                    && Attribute != null
                    && (DroppedDown || SelectionLength == base.Text.Length))
                {
                    AttributeStatusInfo.RemoveSpatialInfo(Attribute);
                }

                base.OnKeyDown(e);

                // Check for selection change.
                ProcessSelectionChange();
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI25987", ex);
            }
        }

        /// <summary>
        /// Adds back newlines if not permanently replaced
        /// </summary>
        protected override string FormatForTextGetter(string text)
        {
            // Add carriage returns back so that caller will not know they were replaced
            // internally.
            if (!RemoveNewLineChars && !string.IsNullOrEmpty(text))
            {
                return text.Replace(DataEntryMethods._CRLF_REPLACEMENT, "\r\n");
            }

            return text;
        }

        /// <summary>
        /// Replaces newlines in the value
        /// </summary>
        protected override string FormatForTextSetter(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                // Permanently replace newline chars with spaces if specified.
                if (RemoveNewLineChars &&
                    text.IndexOf("\r\n", StringComparison.Ordinal) >= 0)
                {
                    // Replace a group of CRLFs with just a single space.
                    text = Regex.Replace(text, "(\r\n)+", " ");
                }
                // Temporarily replace carriage returns or line feeds with no break spaces
                // to prevent the unprintable "boxes" from appearing.
                else if (!RemoveNewLineChars)
                {
                    text = text.Replace("\r\n", DataEntryMethods._CRLF_REPLACEMENT);
                }
            }
            return text;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.VisibleChanged"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.
        /// </param>
        protected override void OnVisibleChanged(EventArgs e)
        {
            try
            {
                base.OnVisibleChanged(e);

                // https://extract.atlassian.net/browse/ISSUE-12812
                // If the visibility of this control is changed after document load, the viewable
                // status of the attribute needs to be updated so that highlights and validation
                // are applied correctly.
                if (Attribute != null &&
                    DataEntryControlHost != null && !DataEntryControlHost.ChangingData)
                {
                    AttributeStatusInfo.MarkAsViewable(Attribute, Visible);
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI37916");
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.Leave" /> event.
        /// Sets the fontstyle to indicate that this control has been viewed.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
        protected override void OnLeave(EventArgs e)
        {
            try
            {
                base.OnLeave(e);

                if (_fontStyle == FontStyle.Bold &&
                    Attribute != null &&
                    AttributeStatusInfo.HasBeenViewedOrIsNotViewable(Attribute, false))
                {
                    SetFontStyle(FontStyle.Regular);
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI41677");
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.MouseUp"/> event.
        /// </summary>
        /// <param name="e">The event data.</param>
        protected override void OnMouseUp(MouseEventArgs e)
        {
            try
            {
                base.OnMouseUp(e);

                // Check for selection change as a result of any mouse click.
                ProcessSelectionChange();
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI50228", ex);
            }
        }


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
        /// <see cref="IAttribute"/> having been modified (i.e., via a swipe or loading a document). 
        /// The event will provide the updated <see cref="IAttribute"/>(s) to registered listeners.
        /// </summary>
        public event EventHandler<AttributesEventArgs> PropagateAttributes;

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

        /// <summary>
        /// Indicates that a control has begun an update and that the
        /// <see cref="DataEntryControlHost"/> should not redraw highlights, etc, until the update
        /// is complete.
        /// <para><b>NOTE:</b></para>
        /// This event should only be raised for updates that initiated via user iteration with the
        /// control. It should not be raised for updates triggered by the
        /// <see cref="DataEntryControlHost"/> such as <see cref="ProcessSwipedText"/>.
        /// </summary>
        public event EventHandler<EventArgs> UpdateStarted
        {
            // Since this event is not currently used by this class but is needed by the 
            // IDataEntryControl interface, define it with an empty implementation to prevent
            // "unused" warnings during compile.
            add { }
            remove { }
        }

        /// <summary>
        /// Indicates that a control has ended an update and actions that needs to be taken by the
        /// <see cref="DataEntryControlHost"/> such as re-drawing highlights can now proceed.
        /// </summary>
        public event EventHandler<EventArgs> UpdateEnded
        {
            // Since this event is not currently used by this class but is needed by the 
            // IDataEntryControl interface, define it with an empty implementation to prevent
            // "unused" warnings during compile.
            add { }
            remove { }
        }

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
        [DefaultValue(null)]
        public IDataEntryControl ParentDataEntryControl { get; set; }

        /// <summary>
        /// If <see langword="true"/>, the text box will accept as input the 
        /// <see cref="SpatialString"/> associated with an image swipe.  If <see langword="false"/>, 
        /// the text box will accept swiped input and the swiping tool should be disabled while the 
        /// text box is active.
        /// </summary>
        [Category("Data Entry Control")]
        [DefaultValue(true)]
        public bool SupportsSwiping
        {
            get
            {
                if (InDesignMode)
                {
                    return _supportsSwiping;
                }
                else
                {
                    // If control isn't enabled or no attribute has been mapped, swiping is not
                    // currently supported.
                    return (_supportsSwiping && base.Enabled && Attribute != null);
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
        [DefaultValue(false)]
        public bool Disabled { get; set; }

        /// <summary>
        /// Gets or sets whether the clipboard contents should be cleared after pasting into the
        /// control.
        /// </summary>
        /// <value><see langword="true"/> if the clipboard should be cleared after pasting,
        /// <see langword="false"/> otherwise.</value>
        [Category("Data Entry Control")]
        [DefaultValue(false)]
        public bool ClearClipboardOnPaste { get; set; }

        /// <summary>
        /// Gets or sets whether descendant attributes in other controls should be highlighted when
        /// this attribute is selected.
        /// </summary>
        /// <value><see langword="true"/> if descendant attributes in other controls should be
        /// highlighted when this attribute is selected; <see langword="false"/> otherwise.</value>
        [Category("Data Entry Control")]
        [DefaultValue(false)]
        public bool HighlightSelectionInChildControls { get; set; }



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
                base.Enabled = (sourceAttributes != null && !Disabled);

                if (sourceAttributes == null)
                {
                    // If the source attribute is null, clear existing data and do not attempt to
                    // map to a new attribute.
                    Attribute = null;
                    SelectedIndex = -1;
                }
                else
                {
                    // Stop listening to the previous active validator (if there is one)
                    if (ActiveValidator != null)
                    {
                        ActiveValidator.AutoCompleteValuesChanged -= HandleAutoCompleteValuesChanged;
                        ActiveValidator = null;
                    }

                    // Attempt to find a mapped attribute from the provided vector.  Create a new 
                    // attribute if no such attribute can be found.
                    Attribute = DataEntryMethods.InitializeAttribute(AttributeName,
                        _multipleMatchSelectionMode, !string.IsNullOrEmpty(AttributeName),
                        sourceAttributes, null, this, null, false, TabStopMode, _validatorTemplate,
                        AutoUpdateQuery, ValidationQuery);

                    // Update the combo box using the new attribute's validator (if there is one).
                    if (Attribute != null)
                    {
                        ActiveValidator = AttributeStatusInfo.GetStatusInfo(Attribute).Validator;

                        if (ActiveValidator != null)
                        {
                            UpdateComboBoxItems();

                            ActiveValidator.AutoCompleteValuesChanged +=
                                HandleAutoCompleteValuesChanged;
                        }
                    }

                    if (base.Visible)
                    {
                        // Mark the attribute as visible if the combobox is visible
                        AttributeStatusInfo.MarkAsViewable(Attribute, true);

                        // [DataEntry:327] If this control is active, ensure the attribute is marked as viewed.
                        if (_isActive)
                        {
                            AttributeStatusInfo.MarkAsViewed(Attribute, true);
                        }
                    }

                    // If not persisting the attribute, mark the attribute accordingly.
                    if (!PersistAttribute)
                    {
                        AttributeStatusInfo.SetAttributeAsPersistable(Attribute, false);
                    }

                    // If the attribute has not been viewed, apply bold font. Otherwise, use
                    // regular font.
                    bool hasBeenViewed = AttributeStatusInfo.HasBeenViewedOrIsNotViewable(Attribute, false);
                    if ((_fontStyle == FontStyle.Bold) == hasBeenViewed)
                    {
                        SetFontStyle(hasBeenViewed ? FontStyle.Regular : FontStyle.Bold);
                    }
                }

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
                // The combo box should be displayed as active only if it is editable and mapped to
                // an attribute.
                _isActive = (setActive && Attribute != null);

                // Change the background color to display active status.
                if (_isActive)
                {
                    BackColor = color;

                    // Mark the attribute as having been viewed and update the attribute's status
                    // info accordingly.
                    AttributeStatusInfo.MarkAsViewed(Attribute, true);

                    if (_fontStyle == FontStyle.Bold)
                    {
                        SetFontStyle(FontStyle.Regular);
                    }
                }
                else
                {
                    ResetBackColor();
                }

                Invalidate();
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
        /// <param name="selectTabGroup">If <see langword="true"/> all <see cref="IAttribute"/>s in
        /// the specified <see cref="IAttribute"/>'s tab group are to be selected,
        /// <see langword="false"/> otherwise.</param>
        /// <seealso cref="IDataEntryControl"/>
        public void PropagateAttribute(IAttribute attribute, bool selectAttribute,
            bool selectTabGroup)
        {
            ExtractException.Assert("ELI25547", "Unexpected attribute!",
                attribute == null || attribute == Attribute);
        }

        /// <summary>
        /// Requests that the <see cref="IDataEntryControl"/> refresh all <see cref="IAttribute"/>
        /// values to the screen.
        /// </summary>
        public virtual void RefreshAttributes()
        {
            RefreshAttributes(true, Attribute);
        }

        /// <summary>
        /// Refreshes the <see cref="IAttribute"/>'s value to the combo box.
        /// </summary>
        /// <param name="spatialInfoUpdated"><see langword="true"/> if the attribute's spatial info
        /// has changed so that hints can be updated; <see langword="false"/> if the attribute's
        /// spatial info has not changed.</param>
        /// <param name="attributes">The <see cref="IAttribute"/>s whose values should be refreshed.
        /// </param>
        public virtual void RefreshAttributes(bool spatialInfoUpdated,
            params IAttribute[] attributes)

        {
            try
            {
                if (Attribute != null && attributes.Contains(Attribute))
                {
                    string newValue = Attribute.Value?.String ?? "";
                    if (newValue != Text || spatialInfoUpdated)
                    {
                        Text = (Attribute.Value != null) ? Attribute.Value.String : "";
                    }

                    // In case the value itself didn't change but the validation list did,
                    // explicitly check validation here.
                    UpdateValidation(Validate(false, out var _));

                    // Raise the AttributesSelected event to re-notify the host of the spatial
                    // information associated with the attribute in case the spatial info has
                    // changed.
                    OnAttributesSelected();

                    // Update the font according to the viewed status.
                    bool hasBeenViewed = AttributeStatusInfo.HasBeenViewedOrIsNotViewable(Attribute, false);
                    if ((_fontStyle == FontStyle.Bold) == hasBeenViewed)
                    {
                        SetFontStyle(hasBeenViewed ? FontStyle.Regular : FontStyle.Bold);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI26126", ex);
            }
        }

        /// <summary>
        /// Gets the UI element associated with the specified <see paramref="attribute"/>. This may
        /// be a type of <see cref="Control"/> or it may also be <see cref="DataGridViewElement"/>
        /// such as a <see cref="DataGridViewCell"/> if the <see paramref="attribute"/>'s owning
        /// control is a table control.
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> for which the UI element is needed.
        /// </param>
        /// <returns>The UI element</returns>
        public object GetAttributeUIElement(IAttribute attribute)
        {
            return this;
        }

        /// <summary>
        /// Processes the supplied <see cref="SpatialString"/> as input.
        /// </summary>
        /// <param name="swipedText">The <see cref="SpatialString"/> representing the
        /// recognized text in the swiped image area.</param>
        /// <returns><see langword="true"/> if the control was able to use the swiped text;
        /// <see langword="false"/> if it could not be used.</returns>
        /// <seealso cref="IDataEntryControl"/>
        public bool ProcessSwipedText(SpatialString swipedText)
        {
            try
            {
                // If control isn't enabled or no attribute has been mapped, swiping is not
                // currently supported.
                if (!base.Enabled || Attribute == null)
                {
                    return false;
                }

                if (!string.IsNullOrWhiteSpace(FormattingRuleFile))
                {
                    // Find the appropriate attribute (if there is one) from the rule's output.
                    IAttribute attribute = DataEntryMethods.RunFormattingRule(
                        FormattingRuleFile, swipedText, AttributeName, _multipleMatchSelectionMode);

                    if (attribute != null && !string.IsNullOrEmpty(attribute.Value.String))
                    {
                        // Use the resulting attribute's value if one was found.
                        AttributeStatusInfo.SetValue(Attribute, attribute.Value, false, true);
                    }
                    else
                    {
                        // If the rules did not find anything, go ahead and use the swiped text
                        // as the attribute value.
                        AttributeStatusInfo.SetValue(Attribute, swipedText, false, true);
                    }
                }
                else
                {
                    // If there is no formatting rule specified, simply assign the swiped text
                    // as the current attribute's value.
                    AttributeStatusInfo.SetValue(Attribute, swipedText, false, true);
                }

                // Consider the attribute un-accepted after a swipe.
                AttributeStatusInfo.AcceptValue(Attribute, false);

                // Update text value of this control and raise the events that need to be raised
                // in conjunction with an attribute change.
                OnAttributeChanged();

                return true;
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

        /// <summary>
        /// Applies the selection state represented by <see paramref="selectionState"/> to the
        /// control.
        /// </summary>
        /// <param name="selectionState">The <see cref="SelectionState"/> to apply.</param>
        public void ApplySelection(SelectionState selectionState)
        {
            try
            {
                if (selectionState is TextControlSelectionState textBoxSelectionState)
                {
                    SelectionStart = textBoxSelectionState.SelectionStart;
                    SelectionLength = textBoxSelectionState.SelectionLength;
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI50227", ex);
            }
        }

        /// <summary>
        /// Creates a <see cref="BackgroundFieldModel"/> for representing this control during
        /// a background data load.
        /// </summary>
        public BackgroundFieldModel GetBackgroundFieldModel()
        {
            try
            {
                var fieldModel = new BackgroundFieldModel()
                {
                    Name = AttributeName,
                    ParentAttributeControl = ParentDataEntryControl,
                    AutoUpdateQuery = AutoUpdateQuery,
                    ValidationQuery = ValidationQuery,
                    DisplayOrder = DataEntryMethods.GetTabIndices(this),
                    IsViewable = Visible,
                    PersistAttribute = PersistAttribute,
                    ValidationErrorMessage = ValidationErrorMessage,
                    ValidationPattern = ValidationPattern,
                    ValidationCorrectsCase = ValidationCorrectsCase
                };

                return fieldModel;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45504");
            }
        }



        /// <summary>
        /// Handles the case that this <see cref="IDataEntryControl"/>'s 
        /// <see cref="ParentDataEntryControl"/> has requested that a new <see cref="IAttribute"/> 
        /// be propagated.  The <see cref="DataEntryComboBox"/> will re-map its control appropriately.
        /// The <see cref="DataEntryComboBox"/> will mark the <see cref="AttributeStatusInfo"/> instance 
        /// associated with any <see cref="IAttribute"/> it propagates as propagated.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">An <see cref="AttributesEventArgs"/> that contains the event data.
        /// </param>
        /// <seealso cref="IDataEntryControl"/>
        public void HandlePropagateAttributes(object sender, AttributesEventArgs e)
        {
            try
            {
                // An attribute can be mapped if there is one and only one attribute to be
                // propagated.
                if (e.Attributes != null && e.Attributes.Size() == 1)
                {
                    base.Enabled = !Disabled;

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
                    if (!string.IsNullOrEmpty(AttributeName))
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


        /// <summary>
        /// Updates the text value of this control and raise the events that need to be raised
        /// in conjunction with an attribute change (<see cref="AttributesSelected"/> and
        /// <see cref="PropagateAttributes"/>).
        /// </summary>
        void OnAttributeChanged()
        {
            Text = Attribute?.Value?.String ?? "";

            // Display the attribute text.
            UpdateValidation(Validate(false, out var _));

            // Raise the AttributesSelected event to signal that the spatial information
            // associated with this control has changed.
            OnAttributesSelected();

            // Raise the PropagateAttributes event so that any descendants of this data control can
            // re-map themselves.
            OnPropagateAttributes();
        }

        /// <summary>
        /// Raises the <see cref="AttributesSelected"/> event.
        /// </summary>
        void OnAttributesSelected()
        {
            if (AttributesSelected != null)
            {
                var selectionState = SelectionState.Create(this);
                AttributesSelected(this, new AttributesSelectedEventArgs(selectionState));
            }
        }

        /// <summary>
        /// Raises the <see cref="PropagateAttributes"/> event.
        /// </summary>
        void OnPropagateAttributes()
        {
            if (PropagateAttributes != null)
            {
                if (Attribute == null)
                {
                    // If the combo box is not currently mapped to an attribute, it should propagate
                    // null so all dependent controls are unmapped.
                    PropagateAttributes(this,
                        new AttributesEventArgs(null));
                }
                else
                {
                    // Propagate the mapped attribute.
                    PropagateAttributes(this,
                        new AttributesEventArgs(
                            DataEntryMethods.AttributeAsVector(Attribute)));
                }
            }
            else if (Attribute != null)
            {
                // If there are no dependent controls registered to receive this event, consider
                // the attribute propagated.
                AttributeStatusInfo.MarkAsPropagated(Attribute, true, true);
            }
        }

        /// <summary>
        /// Handles the case that the validation list was changed so that the combo box items
        /// can be updated to reflect the new list.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        void HandleAutoCompleteValuesChanged(object sender, EventArgs e)
        {
            try
            {
                UpdateComboBoxItems();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI26161");
            }
        }

        // Determines validity of _attribute and optionally creates a corrected value
        DataValidity Validate(bool correctValue, out string correctedValue)
        {
            if (ActiveValidator == null || Attribute == null)
            {
                correctedValue = null;
                return DataValidity.Valid;
            }
            DataValidity dataValidity;
            if (correctValue)
            {
                dataValidity = ActiveValidator.Validate(Attribute, false, out correctedValue);
            }
            else
            {
                dataValidity = ActiveValidator.Validate(Attribute, false);
                correctedValue = null;
            }

            return dataValidity;
        }

        // Applies a new font style to the control.
        void SetFontStyle(FontStyle fontStyle)
        {
            _fontStyle = fontStyle;
            Font = new Font(Font, fontStyle);
        }

        /// <summary>
        /// Checks for a change in the current selection. Needed to add
        /// <see cref="DataEntrySelectionMemento"/>s to the <see cref="Extract.Utilities.UndoManager"/>
        /// stack since there is no SelectionChanged event for the <see cref="TextBox"/> class.
        /// </summary>
        void ProcessSelectionChange()
        {
            if (SelectionStart != LastSelectionStart || SelectionLength != LastSelectionLength)
            {
                AttributeStatusInfo.UndoManager.AddMemento(
                    new DataEntrySelectionMemento(DataEntryControlHost, SelectionState.Create(this)));

                LastSelectionStart = SelectionStart;
                LastSelectionLength = SelectionLength;
            }
        }
    }
}
