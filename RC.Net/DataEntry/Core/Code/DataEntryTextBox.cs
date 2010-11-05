using Extract.Licensing;
using Extract.Imaging.Forms;
using Extract.Utilities.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Design;
using System.Globalization;
using System.IO;
using System.Security.Permissions;
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
    /// with an <see cref="IAttribute"/> to be viewed and edited.
    /// </summary>
    public partial class DataEntryTextBox : TextBox, IDataEntryControl, IRequiresErrorProvider
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(DataEntryTextBox).ToString();

        #endregion Constants

        #region Fields

        /// <summary>
        /// The name used to identify the <see cref="IAttribute"/> to be  associated with the text 
        /// box.
        /// </summary>
        string _attributeName;

        /// <summary>
        /// The <see cref="DataEntryControlHost"/> to which this control belongs
        /// </summary>
        DataEntryControlHost _dataEntryControlHost;

        /// <summary>
        /// Used to specify the data entry control which is mapped to the parent of the attribute 
        /// to which the current textbox is to be mapped.
        /// </summary>
        IDataEntryControl _parentDataEntryControl;

        /// <summary>
        /// The selection mode to use when multiple attributes are found which match the attribute 
        /// name for this control.
        /// </summary>
        MultipleMatchSelectionMode _multipleMatchSelectionMode =
            MultipleMatchSelectionMode.None;

        /// <summary>
        /// Specifies whether this control should allow input via image swipe.
        /// </summary>
        bool _supportsSwiping = true;

        /// <summary>
        /// Specifies whether the control should remain disabled at all times.
        /// </summary>
        bool _disabled;

        /// <summary>
        /// Specifies whether the clipboard contents should be cleared after pasting into the
        /// control.
        /// </summary>
        bool _clearClipboardOnPaste;

        /// <summary>
        /// The attribute mapped to this control.
        /// </summary>
        IAttribute _attribute;

        /// <summary>
        /// The filename of the rule file to be used to parse swiped data.
        /// </summary>
        string _formattingRuleFileName;

        /// <summary>
        /// The formatting rule to be used when processing text from image swipes.
        /// </summary>
        IRuleSet _formattingRule;

        /// <summary>
        /// The template object to be used as a model for per-attribute validation objects.
        /// </summary>
        DataEntryValidator _validatorTemplate = new DataEntryValidator();

        /// <summary>
        /// The validator currently being used to validate the control's attribute.
        /// </summary>
        IDataEntryValidator _activeValidator;

        /// <summary>
        /// The error provider to be used to indicate data validation problems to the user.
        /// </summary>
        ErrorProvider _validationErrorProvider;

        /// <summary>
        /// The error provider to be used to indicate data validation warnings to the user.
        /// </summary>
        ErrorProvider _validationWarningErrorProvider;

        /// <summary>
        /// A query which will cause value to automatically be updated using the values from other
        /// <see cref="IAttribute"/>'s and/or a database query.
        /// </summary>
        string _autoUpdateQuery;

        /// <summary>
        /// A query which will cause the validation list to be updated using the values from other
        /// <see cref="IAttribute"/>'s and/or a database query.
        /// </summary>
        string _validationQuery;

        /// <summary>
        /// Specifies under what circumstances the control's attribute should serve as a tab stop.
        /// </summary>
        TabStopMode _tabStopMode = TabStopMode.Always;

        /// <summary>
        /// Specifies whether the mapped attribute's value should be saved.
        /// </summary>
        bool _persistAttribute = true;

        /// <summary>
        /// Specifies whether carriage return or new line characters will be replaced with spaces.
        /// </summary>
        bool _removeNewLineChars = true;

        /// <summary>
        /// Specifies whether the current instance is running in design mode
        /// </summary>
        bool _inDesignMode;

        /// <summary>
        /// Indicates whether the text box is currently active.
        /// </summary>
        bool _isActive;

        /// <summary>
        /// Indicates whether an update of the auto-complete list was requested, but the update was
        /// postponed because <see cref="DataEntryControlHost"/> UpdateInProgress returned.
        /// <see langword="true"/>.
        /// </summary>
        bool _autoCompleteUpdatePending;

        /// <summary>
        /// The active <see cref="FontStyle"/> for the control.
        /// </summary>
        FontStyle _fontStyle;

        #endregion Fields

        #region Delegates

        /// <summary>
        /// Signature to use for invoking parameterless methods.
        /// </summary>
        delegate void ParameterlessDelegate();

        /// <summary>
        /// Signature to use for invoking methods that accept one <see cref="FontStyle"/> parameter.
        /// </summary>
        /// <param name="fontStyle">A <see cref="FontStyle"/> parameter.</param>
        delegate void FontStyleDelegate(FontStyle fontStyle);

        #endregion Delegates

        #region Constructors

        /// <summary>
        /// Initializes a new <see cref="DataEntryTextBox"/> instance.
        /// </summary>
        public DataEntryTextBox()
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
                LicenseUtilities.ValidateLicense(
                    LicenseIdName.DataEntryCoreComponents, "ELI23689", _OBJECT_NAME);

                InitializeComponent();

                _fontStyle = Font.Style;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI23690", ex);
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the name identifying the <see cref="IAttribute"/> to be associated with 
        /// the <see cref="DataEntryTextBox"/>.</summary>
        /// <value>Sets the name indentifying the <see cref="IAttribute"/> to be associated with 
        /// the <see cref="DataEntryTextBox"/>. Specifying <see langword="null"/> will make the
        /// text box a "dependent sibling" to its <see cref="ParentDataEntryControl"/> meaning 
        /// its attribute will share the same name as the control it is dependent upon, but 
        /// the specific attribute it displays will be dependent on the current selection in the 
        /// <see cref="ParentDataEntryControl"/>.
        /// </value>
        /// <returns>The name indentifying the <see cref="IAttribute"/> to be associated with the 
        /// <see cref="DataEntryTextBox"/>.</returns>
        [Category("Data Entry Text Box")]
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
        /// <see cref="DataEntryTextBox"/>.
        /// </summary>
        /// <value>The selection mode to use to find the mapped attribute for the
        /// <see cref="DataEntryTextBox"/>.</value>
        /// <returns>The selection mode to use to find the mapped attribute for the
        /// <see cref="DataEntryTextBox"/>.</returns>
        [Category("Data Entry Text Box")]
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
        /// split <see cref="SpatialString"/> content passed into <see cref="ProcessSwipedText"/>.
        /// </summary>
        /// <value>The filename of the <see cref="IRuleSet"/> to be used.</value>
        /// <returns>The filename of the <see cref="IRuleSet"/> to be used.</returns>
        [Category("Data Entry Text Box")]
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
                    throw ExtractException.AsExtractException("ELI24111", ex);
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
        [Category("Data Entry Text Box")]
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
                    throw ExtractException.AsExtractException("ELI24184", ex);
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
                    throw ExtractException.AsExtractException("ELI24185", ex);
                }
            }
        }

        /// <summary>
        /// Gets or sets a query which will cause the text box's validation list to be automatically
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
        [Category("Data Entry Text Box")]
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
        [Category("Data Entry Text Box")]
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
        [Category("Data Entry Text Box")]
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
        [Category("Data Entry Text Box")]
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
                    throw ExtractException.AsExtractException("ELI24308", ex);
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
                    throw ExtractException.AsExtractException("ELI24309", ex);
                }
            }
        }

        /// <summary>
        /// Gets or sets a query which will cause the text box's value to automatically be updated
        /// using values from other <see cref="IAttribute"/>'s and/or a database query. Values
        /// to be used from other <see cref="IAttribute"/>'s values should be inserted into the
        /// query using curly braces. For example, to have the value reflect the value of a
        /// sibling attribute named "Source", the query would be specified as "{../Source}".
        /// Text contained withing &lt;SQL&gt; tags will be used as SQL queries against
        /// <see cref="DataEntryControlHost"/>'s database (after substituting in attribute values).
        /// Every time an attribute specified in the query is modified, this query will be
        /// re-evaluated and used to update the value.
        /// </summary>
        /// <value>A query which will cause value to automatically be updated using values
        /// from other <see cref="IAttribute"/>'s and/or a database query.</value>
        /// <returns>A query which will cause value to automatically be updated using values
        /// from other <see cref="IAttribute"/>'s and/or a database query.</returns>
        [Category("Data Entry Text Box")]
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
        /// Specifies under what circumstances the <see cref="DataEntryTextBox"/>'s
        /// <see cref="IAttribute"/> should serve as a tab stop.
        /// </summary>
        /// <value>A <see cref="TabStopMode"/> value indicating when the attribute should serve as a
        /// tab stop.</value>
        /// <returns>A <see cref="TabStopMode"/> value indicating when the attribute will serve as a
        /// tab stop.</returns>
        [Category("Data Entry Text Box")]
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
        /// Gets or sets whether the mapped <see cref="IAttribute"/>'s value should be saved.
        /// </summary>
        /// <value><see langword="true"/> to save the attribute's value; <see langword="false"/>
        /// otherwise.
        /// </value>
        /// <returns><see langword="true"/> if the attribute's value will be saved;
        /// <see langword="false"/> otherwise.</returns>
        [Category("Data Entry Text Box")]
        [DefaultValue(true)]
        public bool PersistAttribute
        {
            get
            {
                return _persistAttribute;
            }

            set
            {
                _persistAttribute = value;
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
        [Category("Data Entry Text Box")]
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
                    if (!_inDesignMode || Visible)
                    {
                        ExtractException.Assert("ELI27300",
                            "RemoveNewLineChars property must be false for multi-line controls.",
                            (!Multiline || !value));
                    }

                    _removeNewLineChars = value;
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI27310", ex);
                }
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

                // Only apply data if the text box is currently mapped.
                if (_attribute != null)
                {
                    AttributeStatusInfo.SetValue(_attribute, Text, true, false);
                }

                // Display a validation error icon if needed.
                UpdateValidation();
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI25582", ex);
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
                ExtractException.Display("ELI24198", ex);
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
                // Ctrl + A is not implemented by the base TextBox class.
                if (e.KeyCode == Keys.A && e.Control)
                {
                    SelectAll();

                    e.Handled = true;
                }
                // If the delete key is pressed while all text is selected, delete spatial info.
                else if (e.KeyCode == Keys.Delete && _attribute != null &&
                    SelectionLength == base.Text.Length)
                {
                    AttributeStatusInfo.RemoveSpatialInfo(_attribute);
                }

                base.OnKeyDown(e);
                
                // [DataEntry:443]
                // When an auto-complete list entry is selected (whether via clicking on an item or
                // pressing enter) an enter key press will be registered. If an auto-complete list
                // is active, an enter key press was registered, all text is selected an the text
                // leads off with a space, trim off the space that is very likely the special space
                // that was added for all entries in the auto-complete list.
                if (e.KeyCode == Keys.Return && AutoCompleteCustomSource != null &&
                    AutoCompleteCustomSource.Count > 0 && TextLength > 1 && 
                    SelectionLength == TextLength && Text[0] == ' ')
                {
                    Text = Text.Substring(1);
                    SelectAll();
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI25986", ex);
            }
        }

        /// <summary>
        /// Gets or sets whether this is a multi-line text box.
        /// </summary>
        /// <value><see langword="true"/> if the text box should be multi-line,
        /// <see langword="false"/> otherwise.</value>
        /// <returns><see langword="true"/> if the text box is multi-line,
        /// <see langword="false"/> otherwise.</returns>
        public override bool Multiline
        {
            get
            {
                return base.Multiline;
            }

            set
            {
                if (value)
                {
                    _removeNewLineChars = false;
                }

                base.Multiline = value;
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
                    string value = base.Text;

                    // Add carriage returns back so that caller will not know they were replaced
                    // internally.
                    if (!_removeNewLineChars && !string.IsNullOrEmpty(value))
                    {
                        return value.Replace(DataEntryMethods._CRLF_REPLACEMENT, "\r\n");
                    }

                    return value;
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI26009", ex);
                }
            }

            set
            {
                try
                {
                    if (!string.IsNullOrEmpty(value))
                    {
                        // Permanently replace newline chars with spaces if specified.
                        if (_removeNewLineChars &&
                            value.IndexOf("\r\n", StringComparison.Ordinal) >= 0)
                        {
                            // Replace a group of CRLFs with just a single space.
                            value = Regex.Replace(value, "(\r\n)+", " ");
                        }
                        // Temporarily replace carriage returns or line feeds with no break spaces
                        // to prevent the unprintable "boxes" from appearing.
                        else if (!_removeNewLineChars && !Multiline)
                        {
                            value = value.Replace("\r\n", DataEntryMethods._CRLF_REPLACEMENT);
                        }
                    }

                    base.Text = value;
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI26010", ex);
                }
            }
        }

        /// <summary>
        /// Raises the <see cref="Control.PreviewKeyDown"/> event.
        /// </summary>
        /// <param name="e">A <see cref="PreviewKeyDownEventArgs"/> containing the event data.
        /// </param>
        // This event handler has undergone a security review.
        //[SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers", MessageId = "0#")]
        protected override void OnPreviewKeyDown(PreviewKeyDownEventArgs e)
        {
            try
            {
                // [DataEntry:385]
                // If the the up or down arrow keys are pressed while the cursor is at the end
                // of the text and the auto-complete list is not currently displayed, temporarily
                // disable auto-complete to prevent some apparent memory issues with auto-complete
                // that can otherwise cause garbage characters to appear at the end of the field.
                if ((e.KeyCode == Keys.Up || e.KeyCode == Keys.Down) &&
                    SelectionStart == Text.Length && !FormsMethods.IsAutoCompleteDisplayed())
                {
                    AutoCompleteMode = AutoCompleteMode.None;
                }

                base.OnPreviewKeyDown(e);
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI27654", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Raises the <see cref="Control.KeyUp"/> event.
        /// </summary>
        /// <param name="e">A <see cref="KeyEventArgs"/> containing the event data.</param>
        protected override void OnKeyUp(KeyEventArgs e)
        {
            try
            {
                base.OnKeyUp(e);

                // If auto-complete was temporarily disabled to prevent arrow keys from exposing
                // memory issues with auto-complete, re-enable autocomplete now.
                if (AutoCompleteMode == AutoCompleteMode.None && AutoCompleteCustomSource.Count > 0)
                {
                    AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI27650", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
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
        /// This event should only be raised for updates that initiated via user interation with the
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

        #endregion IDataEntryControl Events

        #region IDataEntryControl Properties

        /// <summary>
        /// Gets or sets the <see cref="DataEntryControlHost"/> to which this control belongs
        /// </summary>
        /// <value>The <see cref="DataEntryControlHost"/> to which this control belongs.</value>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DataEntryControlHost DataEntryControlHost
        {
            get
            {
                return _dataEntryControlHost;
            }
            set
            {
                _dataEntryControlHost = value;
            }
        }

        /// <summary>
        /// If the <see cref="DataEntryTextBox"/> is not intended to operate on root-level data, 
        /// this property must be used to specify the <see cref="IDataEntryControl"/> which is 
        /// mapped to the parent of the <see cref="IAttribute"/>(s) to which the current 
        /// <see cref="DataEntryTextBox"/> is to be mapped.  The specified 
        /// <see cref="IDataEntryControl"/> must be contained in the same 
        /// <see cref="DataEntryControlHost"/> as this <see cref="DataEntryTextBox"/>. If the 
        /// <see cref="DataEntryTextBox"/> is to be mapped to a root-level <see cref="IAttribute"/>, 
        /// this property must be set to <see langref="null"/>.</summary>
        /// <seealso cref="IDataEntryControl"/>
        [Category("Data Entry Control")]
        [DefaultValue(null)]
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
        [DefaultValue(true)]
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
                    return (_supportsSwiping && Enabled && _attribute != null);
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

        /// <summary>
        /// Gets or sets whether the clipboard contents should be cleared after pasting into the
        /// control.
        /// </summary>
        /// <value><see langword="true"/> if the clipboard should be cleared after pasting,
        /// <see langword="false"/> otherwise.</value>
        [Category("Data Entry Control")]
        [DefaultValue(false)]
        public bool ClearClipboardOnPaste
        {
            get
            {
                return _clearClipboardOnPaste;
            }

            set
            {
                _clearClipboardOnPaste = value;
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
        /// <see cref="IAttribute"/>s from which the <see cref="DataEntryTextBox"/> 
        /// should find its corresponding <see cref="IAttribute"/>.  Can be an empty vector, but 
        /// must not be <see langword="null"/>.</param>
        /// <seealso cref="IDataEntryControl"/>
        public void SetAttributes(IUnknownVector sourceAttributes)
        {
            try
            {
                // [DataEntry:298]
                // If the text box has nothing to propagate, disable it since any data entered
                // would not be mapped into the attribute hierarchy.
                // Also, prevent it from being enabled if explicitly disabled via the
                // IDataEntryControl interface.
                Enabled = (sourceAttributes != null && !_disabled);

                if (sourceAttributes == null)
                {
                    // If the source attribute is null, clear existing data and do not attempt to
                    // map to a new attribute.
                    _attribute = null;
                    Text = "";
                }
                else
                {
                    // Stop listening to the previous active validator (if there is one)
                    if (_activeValidator != null)
                    {
                        _activeValidator.AutoCompleteValuesChanged -= HandleAutoCompleteValuesChanged;
                        _activeValidator = null;
                    }

                    // Attempt to find a mapped attribute from the provided vector.  Create a new 
                    // attribute if no such attribute can be found.
                    _attribute = DataEntryMethods.InitializeAttribute(_attributeName,
                        _multipleMatchSelectionMode, !string.IsNullOrEmpty(_attributeName),
                        sourceAttributes, null, this, 0, false, _tabStopMode, _validatorTemplate,
                        _autoUpdateQuery, _validationQuery);

                    // Update the combo box using the new attribute's validator (if there is one).
                    if (_attribute != null)
                    {
                        _activeValidator = AttributeStatusInfo.GetStatusInfo(_attribute).Validator;

                        if (_activeValidator != null)
                        {
                            UpdateAutoCompleteValues();

                            _activeValidator.AutoCompleteValuesChanged += HandleAutoCompleteValuesChanged;
                        }
                    }

                    if (Visible)
                    {
                        // Mark the attribute as visible if the textbox is visible
                        AttributeStatusInfo.MarkAsViewable(_attribute, true);

                        // [DataEntry:327] If this control is active, ensure the attribute is
                        // marked as viewed.
                        if (_isActive)
                        {
                            AttributeStatusInfo.MarkAsViewed(_attribute, true);
                        }
                    }

                    // If a control is read-only, consider the attribute as viewed since it is
                    // unlikely to matter if a field that can't be changed was viewed.
                    if (ReadOnly)
                    {
                        AttributeStatusInfo.MarkAsViewed(_attribute, true);
                    }

                    // If not persisting the attribute, mark the attribute accordingly.
                    if (!_persistAttribute)
                    {
                        AttributeStatusInfo.SetAttributeAsPersistable(_attribute, false);
                    }

                    // If the attribute has not been viewed, apply bold font.  Otherwise, use
                    // regular font. Compare against _fontStyle instead of the current font since
                    // a font change could be pending in the message queue.
                    bool hasBeenViewed = AttributeStatusInfo.HasBeenViewed(_attribute, false);
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
                throw ExtractException.AsExtractException("ELI24022", ex);
            }
        }

        /// <summary>
        /// Activates or inactivates the <see cref="DataEntryTextBox"/>.
        /// </summary>
        /// <param name="setActive">If <see langref="true"/>, the <see cref="DataEntryTextBox"/>
        /// should visually indicate that it is active. If <see langref="false"/> the 
        /// <see cref="DataEntryTextBox"/> should not visually indicate that it is active.</param>
        /// <param name="color">The <see cref="Color"/> that should be used to indicate active 
        /// status (unused if setActive is <see langword="false"/>).</param>
        /// <seealso cref="IDataEntryControl"/>
        public void IndicateActive(bool setActive, Color color)
        {
            try
            {
                // The text box should be displayed as active only if it is editable and mapped to
                // an attribute.
                _isActive = (setActive && !ReadOnly && _attribute != null);

                // Change the background color to display active status.
                if (_isActive)
                {
                    BackColor = color;

                    // Mark the attribute as having been viewed and update the attribute's status
                    // info accordingly. Compare against _fontStyle instead of the current font
                    // since a font change could be pending in the message queue.
                    AttributeStatusInfo.MarkAsViewed(_attribute, true);
                    if (_fontStyle == FontStyle.Bold)
                    {
                        // [DataEntry:795]
                        // Select all text unless the mouse cursor is within the control, in which
                        // case place at the position in text the mouse was before changing fonts.
                        Point mousePosition = PointToClient(Control.MousePosition);
                        if (ClientRectangle.Contains(mousePosition))
                        {
                            SelectionStart = GetCharIndexFromPosition(mousePosition);

                            if (SelectionStart == (Text.Length - 1) &&
                                mousePosition.X > GetPositionFromCharIndex(SelectionStart).X)
                            {
                                SelectionStart++;
                            }
                        }

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
                throw ExtractException.AsExtractException("ELI24021", ex);
            }
        }

        /// <summary>
        /// This method has no effect for a <see cref="DataEntryTextBox"/> except to validate that
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
        /// <param name="selectTabGroup">If <see langword="true"/> all <see cref="IAttribute"/>s in
        /// the specified <see cref="IAttribute"/>'s tab group are to be selected,
        /// <see langword="false"/> otherwise.</param>
        /// </param>
        /// <seealso cref="IDataEntryControl"/>
        public void PropagateAttribute(IAttribute attribute, bool selectAttribute,
            bool selectTabGroup)
        {
            ExtractException.Assert("ELI24736", "Unexpected attribute!",
                attribute == null || attribute == _attribute);
        }

        /// <summary>
        /// Requests that the <see cref="IDataEntryControl"/> refresh all <see cref="IAttribute"/>
        /// values to the screen.
        /// </summary>
        public virtual void RefreshAttributes()
        {
            RefreshAttributes(true, _attribute);
        }

        /// <summary>
        /// Refreshes the <see cref="IAttribute"/>'s value to the text box.
        /// </summary>
        /// <param name="spatialInfoUpdated"><see langword="true"/> if the attribute's spatial info
        /// has changed so that hints can be updated; <see langword="false"/> if the attribute's
        /// spatial info has not changed.</param>
        /// <param name="attributes">The <see cref="IAttribute"/>s whose values should be refreshed.
        /// </param>
        public virtual void RefreshAttributes(bool spatialInfoUpdated, params IAttribute[] attributes)
        {
            try
            {
                if (_attribute == null)
                {
                    return;
                }

                foreach (IAttribute attribute in attributes)
                {
                    if (_attribute == attribute)
                    {
                        Text = (_attribute.Value != null) ? _attribute.Value.String : "";

                        // In case the value itself didn't change but the validation list did, explicitly
                        // check validation here.
                        UpdateValidation();

                        // Raise the AttributesSelected event to re-notify the host of the spatial
                        // information associated with the attribute in case the spatial info has
                        // changed.
                        OnAttributesSelected();

                        // Update the font according to the viewed status. Perform the font change
                        // asynchronously othersize the it results in memory corruption related to
                        // auto-complete lists (probably related to the window handle recreation it
                        // triggers). Compare against _fontStyle instead of the current font since
                        // a font change could be pending in the message queue.
                        bool hasBeenViewed = AttributeStatusInfo.HasBeenViewed(_attribute, false);
                        if ((_fontStyle == FontStyle.Bold) == hasBeenViewed)
                        {
                            SetFontStyle(hasBeenViewed ? FontStyle.Regular : FontStyle.Bold);
                        }

                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI26219", ex);
            }
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
                if (!Enabled || _attribute == null)
                {
                    return false;
                }

                if (_formattingRule != null)
                {
                    // Find the appropriate attribute (if there is one) from the rule's output.
                    IAttribute attribute = DataEntryMethods.RunFormattingRule(
                        _formattingRule, swipedText, _attributeName, _multipleMatchSelectionMode);

                    if (attribute != null && !string.IsNullOrEmpty(attribute.Value.String))
                    {
                        // Use the resulting attribute's value if one was found.
                        swipedText = attribute.Value;
                    }
                }

                // Keep track of what the final selection should be.
                int selectionStart = SelectionStart;
                int selectionLength = swipedText.Size;

                // Insert the swiped text into the current selection.
                swipedText = DataEntryMethods.InsertSpatialStringIntoSelection(
                    this, _attribute.Value, swipedText);

                // Apply the new value.
                AttributeStatusInfo.SetValue(_attribute, swipedText, false, true);

                // Consider the attribute un-accepted after a swipe.
                AttributeStatusInfo.AcceptValue(_attribute, false);

                // Update text value of this control and raise the events that need to be raised
                // in conjunction with an attribute change.
                OnAttributeChanged();

                // Select the newly swiped text.
                Select(selectionStart, selectionLength);

                return true;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI24020", ex);
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
            // Nothing to do.
        }

        #endregion IDataEntryControl Methods

        #region IDataEntryControl Event Handlers

        /// <summary>
        /// Handles the case that this <see cref="IDataEntryControl"/>'s 
        /// <see cref="ParentDataEntryControl"/> has requested that a new <see cref="IAttribute"/> 
        /// be propagated.  The <see cref="DataEntryTextBox"/> will re-map its control appropriately.
        /// The <see cref="DataEntryTextBox"/> will mark the <see cref="AttributeStatusInfo"/> instance 
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
                    Enabled = !_disabled;

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
                    // If there is more than one parent attribute or no parent attributes, the text
                    // box cannot be mapped and should propagate null so all dependent controls are
                    // unmapped.
                    SetAttributes(null);
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI24190", ex);
                ee.AddDebugData("Event data", e, false);
                throw ee;
            }
        }

        #endregion IDataEntryControl Event Handlers

        #region IRequiresErrorProvider Members

        /// <summary>
        /// Specifies the standard <see cref="ErrorProvider"/>s that should be used to 
        /// display data validation errors.
        /// </summary>
        /// <param name="validationErrorProvider">The standard <see cref="ErrorProvider"/> that
        /// should be used to display data validation errors.</param>
        /// <param name="validationWarningErrorProvider">The <see cref="ErrorProvider"/> that should
        /// be used to display data validation warnings.</param>
        public void SetErrorProviders(ErrorProvider validationErrorProvider,
            ErrorProvider validationWarningErrorProvider)
        {
            _validationErrorProvider = validationErrorProvider;
            _validationWarningErrorProvider = validationWarningErrorProvider; 
        }

        #endregion IRequiresErrorProvider Members

        #region Private Members

        /// <summary>
        /// Updates the text value of this control and raise the events that need to be raised
        /// in conjunction with an attribute change (<see cref="AttributesSelected"/> and
        /// <see cref="PropagateAttributes"/>).
        /// </summary>
        void OnAttributeChanged()
        {
            // Display the attribute text.
            Text = (_attribute != null && _attribute.Value != null)
                ? _attribute.Value.String : "";

            // Display a validation error icon if needed.
            UpdateValidation();

            // Raise the AttributesSelected event to signal that the spatial information
            // associated with this control has changed.
            OnAttributesSelected();

            // Raise the PropagateAttributes event so that any descendents of this data control can
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
                var selectionState = new SelectionState(this, 
                    DataEntryMethods.AttributeAsVector(_attribute), false, true, null);
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
                if (_attribute == null)
                {
                    // If the text box is not currently mapped to an attribute, it should propagate
                    // null so all dependent controls are unmapped.
                    PropagateAttributes(this,
                        new AttributesEventArgs(null));
                }
                else
                {
                    // Propagate the mapped attribute.
                    PropagateAttributes(this,
                        new AttributesEventArgs(
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
        /// </summary>
        /// <param name="correctValue">If <see langword="true"/> and a 
        /// <see cref="DataEntryQuery"/> has been configured, if the 
        /// <see cref="DataEntryTextBox"/>'s value matches a value in the supplied list  
        /// case-insensitively but not case-sensitively or the value is different only due to
        /// leading or trailing spaces, the value will be modified to match the value in the
        /// supplied list.</param>
        /// <returns>If the data is valid or <see paramref="throwException"/> is
        /// <see langword="false"/> a <see cref="DataValidity"/> value indicating the validity of
        /// the data.
        /// </returns>
        DataValidity Validate(bool correctValue)
        {
            // If there is no mapped attribute, the control is disabled or there is not active
            // validator, the data cannot be validated.
            if (_disabled || _attribute == null || _activeValidator == null)
            {
                return DataValidity.Valid;
            }

            DataValidity dataValidity;

            if (correctValue)
            {
                // If the attribute's value should be removed of extra whitespace and made to match
                // the casing in the validator, provide a variable to retrieve a corrected value.
                string correctedValue;
                dataValidity = _activeValidator.Validate(_attribute, false, out correctedValue);

                // If a corrected value was returned, apply it to the text box.
                if (!string.IsNullOrEmpty(correctedValue))
                {
                    Text = correctedValue;
                }
            }
            else
            {
                // Do not auto-correct whitespace, casing... just validate.
                dataValidity = _activeValidator.Validate(_attribute, false);
            }

            return dataValidity;
        }

        /// <summary>
        /// Handles the case that the validation list was changed so that the auto-complete values
        /// can be updated to reflect the new list.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        void HandleAutoCompleteValuesChanged(object sender, EventArgs e)
        {
            try
            {
                // Add the new validation list items as auto-complete values
                UpdateAutoCompleteValues();
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI26220", ex);
                ee.AddDebugData("Event data", e, false);
                throw ee;
            }
        }

        /// <summary>
        /// Updates the auto-complete values to reflect the validation list values.
        /// </summary>
        void UpdateAutoCompleteValues()
        {
            // Post a message to update the autocomplete values on the message queue. This way we
            // can be sure that the list will not be updated as part of a key event handler (which
            // can lead to access violations.
            BeginInvoke(new ParameterlessDelegate(UpdateAutoCompleteValuesDirect));
        }

        /// <summary>
        /// Helper method for UpdateAutoCompleteValues. UpdateAutoCompleteValuesDirect should not be
        /// called directly to ensure auto-complete lists are never updated during a keystroke event.
        /// </summary>
        void UpdateAutoCompleteValuesDirect()
        {
            // If the host reports that an update is in progress, delay updating the auto-complete
            // list since the update may otherwise result in the auto-complete list being changed
            // multiple times before the update is over.
            if (DataEntryControlHost != null && DataEntryControlHost.UpdateInProgress)
            {
                if (!_autoCompleteUpdatePending)
                {
                    _autoCompleteUpdatePending = true;
                    DataEntryControlHost.UpdateEnded += HandleDataEntryControlHostUpdateEnded;
                }

                return;
            }
            else if (_autoCompleteUpdatePending)
            {
                DataEntryControlHost.UpdateEnded -= HandleDataEntryControlHostUpdateEnded;
                _autoCompleteUpdatePending = false;
            }

            // Get updated values for the auto-complete fields if an update is required.
            AutoCompleteMode autoCompleteMode = AutoCompleteMode;
            AutoCompleteSource autoCompleteSource = AutoCompleteSource;
            AutoCompleteStringCollection autoCompleteList = AutoCompleteCustomSource;
            string[] autoCompleteValues;
            if (DataEntryMethods.UpdateAutoCompleteList(_activeValidator, ref autoCompleteMode,
                    ref autoCompleteSource, ref autoCompleteList, out autoCompleteValues))
            {
                AutoCompleteMode = autoCompleteMode;
                AutoCompleteSource = autoCompleteSource;
                AutoCompleteCustomSource = autoCompleteList;
            }
            else if (base.AutoCompleteMode != AutoCompleteMode.None)
            {
                base.AutoCompleteMode = AutoCompleteMode.None;
            }
        }

        /// <summary>
        /// Handles the case the a significant update of control data as reported by
        /// <see cref="DataEntryControlHost"/> UpdateInProgress has ended.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        void HandleDataEntryControlHostUpdateEnded(object sender, EventArgs e)
        {
            try
            {
                DataEntryControlHost.UpdateEnded -= HandleDataEntryControlHostUpdateEnded;
                _autoCompleteUpdatePending = false;

                UpdateAutoCompleteValues();
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI30116", ex);
            }
        }

        /// <summary>
        /// Re-validates the control's data and updates validation error icon as appropriate.
        /// </summary>
        void UpdateValidation()
        {
            if (_validationErrorProvider != null || _validationWarningErrorProvider != null)
            {
                DataValidity dataValidity = Validate(false);

                if (_validationErrorProvider != null)
                {
                    _validationErrorProvider.SetError(this, dataValidity == DataValidity.Invalid ?
                        _activeValidator.ValidationErrorMessage : "");
                }

                if (_validationWarningErrorProvider != null)
                {
                    _validationWarningErrorProvider.SetError(this, 
                        dataValidity == DataValidity.ValidationWarning ? 
                            _activeValidator.ValidationErrorMessage : "");
                }
            }
        }

        /// <summary>
        /// Applies a new font style to the control.
        /// </summary>
        /// <param name="fontStyle">The new <see cref="FontStyle"/> to apply.</param>
        void SetFontStyle(FontStyle fontStyle)
        {
            _fontStyle = fontStyle;

            // Perform the font change asynchronously othersize it results in memory corruption
            // related to auto-complete lists (probably related to the window handle recreation it
            // triggers).
            base.BeginInvoke(new FontStyleDelegate(SetFontStyleDirect),
                        new object[] { fontStyle });
        }

        /// <summary>
        /// Applies a new font style to the control. This method is a helper for SetFontStyle and
        /// should not be called directly.
        /// </summary>
        /// <param name="fontStyle">The new <see cref="FontStyle"/> to apply.</param>
        void SetFontStyleDirect(FontStyle fontStyle)
        {
            try
            {
                Font = new Font(Font, fontStyle);
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI29998", ex);
            }
        }

        #endregion Private Members
    }
}
