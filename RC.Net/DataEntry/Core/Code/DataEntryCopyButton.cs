using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using UCLID_AFCORELib;
using UCLID_COMUTILSLib;

namespace Extract.DataEntry
{
    /// <summary>
    /// A <see cref="Button"/> and <see cref="IDataEntryControl"/> extension intended to copy the
    /// text of another control to the clipboard and report its "data" as invalid until it is
    /// pressed.
    /// </summary>
    public partial class DataEntryCopyButton : Button, IDataEntrySingleAttributeControl, IRequiresErrorProvider,
        IDataEntryValidator
    {
        #region Fields

        /// <summary>
        /// The name used to identify the <see cref="IAttribute"/> to be associated with the button.
        /// </summary>
        string _attributeName;

        /// <summary>
        /// The attribute mapped to this control.
        /// </summary>
        IAttribute _attribute;

        /// <summary>
        /// The <see cref="DataEntryControlHost"/> to which this control belongs
        /// </summary>
        DataEntryControlHost _dataEntryControlHost;

        /// <summary>
        /// Used to specify the data entry control which is mapped to the parent of the attribute 
        /// to which this button is to be mapped.
        /// </summary>
        IDataEntryControl _parentDataEntryControl;

        /// <summary>
        /// Specifies under what circumstances the control's attribute should serve as a tab stop.
        /// </summary>
        TabStopMode _tabStopMode = TabStopMode.Always;

        /// <summary>
        /// Specifies whether the control should remain disabled at all times.
        /// </summary>
        bool _disabled;

        /// <summary>
        /// The error provider to be used to indicate data validation problems to the user.
        /// </summary>
        ErrorProvider _errorProvider;

        /// <summary>
        /// The error message that should be displayed until the button has been clicked.
        /// </summary>
        string _validationErrorMessage =
            "Press the copy button to copy the image name to the clipboard.";

        /// <summary>
        /// The control that contains the text to be copied.
        /// </summary>
        Control _copySourceControl;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// 
        /// </summary>
        public DataEntryCopyButton()
            : base()
        {
            try
            {
                InitializeComponent();

                // Create a unique name to use for the attribute.  It will not be persisted, it
                // serves only as a placeholder for the control in the tab order.
                _attributeName = "PlaceholderAttribute_" + this.Name;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI26972", ex);
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the error message that should be displayed until the button has been clicked.
        /// </summary>
        /// <value>The tooltip text that should be displayed for an error provider icon until the
        /// button has been clicked.</value>
        /// <returns>The tooltip text that is be displayed for an error provider icon until the
        /// button has been clicked.</returns>
        [Category("Data Entry Copy Button")]
        public string ValidationErrorMessage
        {
            get
            {
                return _validationErrorMessage;
            }

            set
            {
                _validationErrorMessage = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="Control"/> that contains the text to be copied.
        /// </summary>
        /// <value>The <see cref="Control"/> that contains the text to be copied.</value>
        /// <returns>The <see cref="Control"/> that contains the text to be copied.</returns>
        [Category("Data Entry Copy Button")]
        public Control CopySourceControl
        {
            get
            {
                return _copySourceControl;
            }

            set
            {
                _copySourceControl = value;
            }
        }

        #endregion Properties

        #region Overrides

        /// <summary>
        /// Handles the <see cref="Control.Click"/> event.
        /// </summary>
        /// <param name="e">A <see cref="EventArgs"/> that contains the event data.</param>
        protected override void OnClick(EventArgs e)
        {
            try
            {
                ExtractException.Assert("ELI26980", "Cannot find control to copy from!",
                    _copySourceControl != null);

                try
                {
                    Clipboard.SetText(_copySourceControl.Text);
                }
                catch (Exception ex)
                {
                    // From time to time (especially in the case of a lot of successive clicks),
                    // the SetText call will throw an exception. I have found that even when an
                    // exception is thrown, the text seems to have been applied to the clipboard.
                    // Even if it hasn't, just log the exception. The user can re-press the button
                    // if need be.
                    ExtractException ee = ExtractException.AsExtractException("ELI27955", ex);
                    ee.AddDebugData("Copied text", _copySourceControl.Text, false);
                    ee.Log();
                    return;
                }

                _errorProvider.SetError(this, "");
                AttributeStatusInfo.SetDataValidity(_attribute, DataValidity.Valid);

                base.OnClick(e);
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI26979", ex);
            }
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
                if (_attribute != null &&
                    DataEntryControlHost != null && !DataEntryControlHost.ChangingData)
                {
                    AttributeStatusInfo.MarkAsViewable(_attribute, Visible);
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI37917");
            }
        }

        #endregion Overrides

        #region IDataEntryValidator Members

        #region Events

        /// <summary>
        /// Raised to indicate the validation list has been updated.
        /// </summary>
        public event EventHandler<EventArgs> ValidationListChanged
        {
            // Since this event is not currently used by this class but is needed by the 
            // IDataEntryValidator interface, define it with an empty implementation to prevent
            // "unused" warnings during compile.
            add { }
            remove { }
        }

        /// <summary>
        /// Raised to indicate the auto-complete values have changed.
        /// </summary>
        public event EventHandler<EventArgs> AutoCompleteValuesChanged
        {
            // Since this event is not currently used by this class but is needed by the 
            // IDataEntryValidator interface, define it with an empty implementation to prevent
            // "unused" warnings during compile.
            add { }
            remove { }
        }

        #endregion Events

        /// <overloads>
        /// Tests to see if the provided <see cref="IAttribute"/> meets all specified 
        /// validation requirements the implementing class has.
        /// </overloads>
        /// <summary>
        /// Tests to see if the provided <see cref="IAttribute"/> meets all specified 
        /// validation requirements the implementing class has.
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> whose data is to be validated.
        /// </param>
        /// <param name="throwException">If <see langword="true"/> if the method will throw an
        /// exception if the provided value does not meet validation requirements.</param>
        /// <returns>A <see cref="DataValidity"/>value indicating whether 
        /// <see paramref="attribute"/>'s value is currently valid.
        /// </returns>
        /// <throws><see cref="DataEntryValidationException"/> if the <see cref="IAttribute"/>'s 
        /// data fails to match any validation requirements it has and
        /// <see paramref="throwException"/> is <see langword="true"/></throws>
        public DataValidity Validate(IAttribute attribute, bool throwException)
        {
            try
            {
                DataValidity dataValidity = AttributeStatusInfo.GetDataValidity(attribute);

                if (throwException && dataValidity != DataValidity.Valid)
                {
                    throw new DataEntryValidationException("ELI27081", _validationErrorMessage,
                        this);
                }
                else
                {
                    return dataValidity;
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI27079", ex);
            }
        }

        /// <summary>
        /// Tests to see if the provided <see cref="IAttribute"/> meets all specified 
        /// validation requirements the implementing class has.
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> whose data is to be validated.
        /// </param>
        /// <param name="throwException">If <see langword="true"/> if the method will throw an
        /// exception if the provided value does not meet validation requirements.</param>
        /// <param name="correctedValue">If the value is valid but has extra whitespace or has
        /// different casing, this parameter will be populated with a corrected version of the
        /// value that has already been applied to the attribute.</param>
        /// <returns>A <see cref="DataValidity"/>value indicating whether 
        /// <see paramref="attribute"/>'s value is currently valid.
        /// </returns>
        /// <throws><see cref="DataEntryValidationException"/> if the <see cref="IAttribute"/>'s 
        /// data fails to match any validation requirements it has and
        /// <see paramref="throwException"/> is <see langword="true"/></throws>
        [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "2#")]
        public DataValidity Validate(IAttribute attribute, bool throwException, out string correctedValue)
        {
            try
            {
                correctedValue = null;
                DataValidity dataValidity = AttributeStatusInfo.GetDataValidity(attribute);

                if (throwException && dataValidity != DataValidity.Valid)
                {
                    throw new DataEntryValidationException("ELI29213", _validationErrorMessage,
                        this);
                }
                else
                {
                    return dataValidity;
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI29214", ex);
            }
        }

        /// <summary>
        /// Unused. The copy button does not provide any auto-complete list.
        /// </summary>
        /// <returns><see langword="null"/></returns>
        public string[] GetAutoCompleteValues()
        {
            return null;
        }

        /// <summary>
        /// Unused. The copy button does not provide any auto-complete list.
        /// </summary>
        /// <returns><see <c>null</c></returns>
        public IEnumerable<KeyValuePair<string, List<string>>> AutoCompleteValuesWithSynonyms => null;

        /// <summary>
        /// The copy button's validation is not specific to any given attribute; the same instance
        /// can be used to validate multiple attributes.
        /// </summary>
        /// <returns>The current instance.</returns>
        public IDataEntryValidator GetPerAttributeInstance()
        {
            return this;
        }

        #endregion IDataEntryValidator Members

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
            _errorProvider = validationErrorProvider;
        }

        #endregion IRequiresErrorProvider Members

        #region IDataEntryControl Members

        /// <summary>
        /// Fired whenever the set of selected or active <see cref="IAttribute"/>(s) for a control
        /// changes. This can occur as part of the <see cref="PropagateAttributes"/> event, when
        /// new attribute(s) are created via a swipe or when a new element of the control becomes 
        /// active.
        /// </summary>
        public event EventHandler<AttributesSelectedEventArgs> AttributesSelected;

        /// <summary>
        /// Gets or sets the <see cref="DataEntryControlHost"/> to which this copy button belongs
        /// </summary>
        /// <value>The <see cref="DataEntryControlHost"/> to which this copy button belongs.</value>
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
        /// If the <see cref="DataEntryCopyButton"/> is not intended to operate on root-level data, 
        /// this property must be used to specify the <see cref="IDataEntryControl"/> which is 
        /// mapped to the parent of the <see cref="IAttribute"/>(s) to which the current 
        /// <see cref="DataEntryCopyButton"/> is to be mapped.  The specified 
        /// <see cref="IDataEntryControl"/> must be contained in the same 
        /// <see cref="DataEntryControlHost"/> as this <see cref="DataEntryCopyButton"/>. If the 
        /// <see cref="DataEntryCopyButton"/> is to be mapped to a root-level <see cref="IAttribute"/>, 
        /// this property must be set to <see langref="null"/>.</summary>
        /// <seealso cref="IDataEntryControl"/>
        [Category("Data Entry Copy Button")]
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
        /// Gets the selection mode to use to find the mapped attribute for the
        /// <see cref="DataEntryCopyButton"/>.
        /// </summary>
        /// <value>Cannot be changed from MultipleMatchSelectionMode.First.</value>
        /// <returns><see cref="DataEntryCopyButton"/> will always use
        /// MultipleMatchSelectionMode.First.</returns>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public MultipleMatchSelectionMode MultipleMatchSelectionMode
        {
            get
            {
                return MultipleMatchSelectionMode.First;
            }

            set
            {
                ExtractException.Assert("ELI26981", "DataEntryCopyButton must use " +
                    "MultipleMatchSelectionMode.First", value == MultipleMatchSelectionMode.First);
            }
        }

        /// <summary>
        /// Specifies under what circumstances the <see cref="DataEntryCopyButton"/>'s
        /// <see cref="IAttribute"/> should serve as a tab stop.
        /// </summary>
        /// <value>A <see cref="TabStopMode"/> value indicating when the attribute should serve as a
        /// tab stop.</value>
        /// <returns>A <see cref="TabStopMode"/> value indicating when the attribute will serve as a
        /// tab stop.</returns>
        [Category("Data Entry Copy Button")]
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
        /// Gets or sets whether the control should remain disabled at all times.
        /// <para><b>Note</b></para>
        /// If disabled, mapped data will not be validated.
        /// </summary>
        /// <value><see langword="true"/> if the control should remain disabled,
        /// <see langword="false"/> otherwise.</value>
        /// <returns><see langword="true"/> if the control will remain disabled,
        /// <see langword="false"/> otherwise.</returns>
        [Category("Data Entry Copy Button")]
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
        /// Specifies the domain of <see cref="IAttribute"/>s from which the 
        /// <see cref="IDataEntryControl"/> should find the <see cref="IAttribute"/> 
        /// to which it should be mapped. <see cref="DataEntryCopyButton"/> will always use the
        /// first match (if available).
        /// </summary>
        /// <param name="sourceAttributes">The <see cref="IUnknownVector"/> instance of 
        /// <see cref="IAttribute"/>s from which the <see cref="DataEntryCopyButton"/> 
        /// should find its corresponding <see cref="IAttribute"/>.  Can be an empty vector, but 
        /// must not be <see langword="null"/>.</param>
        /// <seealso cref="IDataEntryControl"/>
        public void SetAttributes(IUnknownVector sourceAttributes)
        {
            try
            {
                // [DataEntry:298]
                // If the copy button has nothing to propagate, disable it since any data entered
                // would not be mapped into the attribute hierarchy.
                // Also, prevent it from being enabled if explicitly disabled via the
                // IDataEntryControl interface.
                Enabled = (sourceAttributes != null && !_disabled);

                if (!Enabled)
                {
                    // If not enabled, ensure the error icon is reset.
                    _errorProvider.SetError(this, "");
                }

                if (sourceAttributes == null)
                {
                    // If the source attribute is null, clear existing data and do not attempt to
                    // map to a new attribute.
                    _attribute = null;
                }
                else
                {
                    // Attempt to find a mapped attribute from the provided vector.  Create a new 
                    // attribute if no such attribute can be found.
                    _attribute = DataEntryMethods.InitializeAttribute(_attributeName,
                        MultipleMatchSelectionMode.First, !string.IsNullOrEmpty(_attributeName),
                        sourceAttributes, null, this, null, false, _tabStopMode, this, null, null);

                    if (base.Visible)
                    {
                        AttributeStatusInfo.MarkAsViewable(_attribute, true);

                        // If the attribute has not yet been marked as not persistable, this is the
                        // first time it has been displayed-- initialize the attribute as invalid.
                        if (!_disabled && !string.IsNullOrEmpty(ValidationErrorMessage)
                            && AttributeStatusInfo.IsAttributePersistable(_attribute))
                        {
                            _errorProvider.SetError(this, _validationErrorMessage);
                            AttributeStatusInfo.SetDataValidity(_attribute, DataValidity.Invalid);
                        }
                    }

                    // Don't persist placeholder attributes in output-- the attribute itself is only
                    // a placeholder for the control in the tab order.
                    AttributeStatusInfo.SetAttributeAsPersistable(_attribute, false);

                    // Consider the attribute fully propagated as it should never have children.
                    AttributeStatusInfo.MarkAsPropagated(_attribute, true, true);
                }

                // Raise the AttributesSelected event to notify the host of the currently selected
                // attribute.
                OnAttributesSelected();
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI26976", ex);
            }
        }

        /// <summary>
        /// Handles the case that this <see cref="DataEntryCopyButton"/>'s 
        /// <see cref="ParentDataEntryControl"/> has requested that a new <see cref="IAttribute"/> 
        /// be propagated.  The <see cref="DataEntryCopyButton"/> will re-map its control appropriately.
        /// The <see cref="DataEntryCopyButton"/> will mark the <see cref="AttributeStatusInfo"/> instance 
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
                    // If there is more than one parent attribute or no parent attributes, the group
                    // box cannot be mapped and should propagate null so all dependent controls are
                    // unmapped.
                    SetAttributes(null);
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI26977", ex);
            }
        }

        /// <summary>
        /// This method has no effect for a <see cref="DataEntryCopyButton"/> except to validate that
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
            ExtractException.Assert("ELI26978", "Unexpected attribute!",
                attribute == null || attribute == _attribute);
        }

        /// <summary>
        /// The single attribute mapped to this control
        /// </summary>
        public IAttribute Attribute => _attribute;

        /// <summary>
        /// The attributes mapped to this control.
        /// (In the case of this control, there will only be one)
        /// </summary>
        public IEnumerable<IAttribute> Attributes => new[] { _attribute };

        #endregion IDataEntryControl Members

        #region Unused IDataEntryControl Members

        /// <summary>
        /// This event is not raised by <see cref="DataEntryCopyButton"/>.
        /// </summary>
        public event EventHandler<AttributesEventArgs> PropagateAttributes
        {
            // Since this event is not currently used by this class but is needed by the 
            // IDataEntryControl interface, define it with an empty implementation to prevent
            // "unused" warnings during compile.
            add { }
            remove { }
        }

        /// <summary>
        /// This event is not raised by <see cref="DataEntryCopyButton"/>.
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
        /// This event is not raised by <see cref="DataEntryCopyButton"/>.
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
        /// This event is not raised by <see cref="DataEntryCopyButton"/>.
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
        /// This event is not raised by <see cref="DataEntryCopyButton"/>.
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
        /// <see cref="DataEntryCopyButton"/> does not support swiping; the value of this property
        /// will always be <see langword="false"/>.
        /// </summary>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool SupportsSwiping
        {
            get
            {
                return false;
            }
            set
            {
                if (value)
                {
                    throw new ExtractException("ELI26973",
                        "DataEntryCopyButton does not support swiping!");
                }
            }
        }

        /// <summary>
        /// <see cref="DataEntryCopyButton"/> does not support pasting; the value of this property
        /// will always be <see langword="false"/>.
        /// </summary>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ClearClipboardOnPaste
        {
            get
            {
                return false;
            }

            set
            {
                throw new ExtractException("ELI29011",
                    "DataEntryCopyButton does not support pasting!");
            }
        }

        /// <summary>
        /// <see cref="DataEntryCopyButton"/> does not support selection; the value of this property
        /// will always be <see langword="false"/>.
        /// </summary>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool HighlightSelectionInChildControls
        {
            get
            {
                return false;
            }

            set
            {
                throw new ExtractException("ELI34990",
                    "DataEntryCopyButton does not support selection!");
            }
        }

        /// <summary>
        /// <see cref="DataEntryCopyButton"/> does not have any implementation for this method.
        /// </summary>
        /// <param name="setActive">Unused.</param>
        /// <param name="color">Unused.</param>
        public void IndicateActive(bool setActive, Color color)
        {
        }

        /// <summary>
        /// <see cref="DataEntryCopyButton"/> does not have any implementation for this method.
        /// </summary>
        /// <throws><see cref="ExtractException"/> if called.</throws>
        /// <param name="swipedText">Unused.</param>
        public bool ProcessSwipedText(UCLID_RASTERANDOCRMGMTLib.SpatialString swipedText)
        {
            throw new ExtractException("ELI27008", "DataEntryCopyButton does not support swiping!");
        }

        /// <summary>
        /// <see cref="DataEntryCopyButton"/> does not have any implementation for this method.
        /// </summary>
        public virtual void RefreshAttributes()
        {
        }

        /// <summary>
        /// <see cref="DataEntryCopyButton"/> does not have any implementation for this method.
        /// </summary>
        /// <param name="spatialInfoUpdated">Unused.</param>
        /// <param name="attributes">Unused.</param>
        public void RefreshAttributes(bool spatialInfoUpdated,  params IAttribute[] attributes)
        {
        }

        /// <summary>
        /// Gets the UI element associated with the specified <see paramref="attribute"/>. This may
        /// be a type of <see cref="Control"/> or it may also be <see cref="DataGridViewElement"/>
        /// such as a <see cref="DataGridViewCell"/> if the <see paramref="attribute"/>'s owning
        /// control is a table control.
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> for which the UI element is needed.
        /// </param>
        /// <param name="elementName">If a related element is required, the property name of an
        /// object relative to the object mapped directly to the attribute. Multiple references may
        /// be chained by separating with a period. E.g., While a specific attribute may be mapped
        /// to a DataGridViewRow, an elementName of "DataGridView.VerticalScrollBar" could be used
        /// to refer to the scroll bar for the grid.</param>
        /// <returns>The UI element.</returns>
        public object GetAttributeUIElement(IAttribute attribute, string elementName)
        {
            try
            {
                return string.IsNullOrEmpty(elementName)
                    ? this
                    : this.GetProperty(elementName);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI50323");
            }
        }

        /// <summary>
        /// <see cref="DataEntryCopyButton"/> doesn't cache any data.
        /// </summary>
        public void ClearCachedData()
        {
        }

        /// <summary>
        /// <see cref="DataEntryCopyButton"/> does not have any implementation for this method.
        /// <param name="selectionState">Unused.</param>
        /// </summary>
        public void ApplySelection(SelectionState selectionState)
        {
        }

        /// <summary>
        /// Creates a <see cref="BackgroundFieldModel"/> for representing this control during
        /// a background data load.
        /// </summary>
        public BackgroundFieldModel GetBackgroundFieldModel()
        {
            try
            {
                return new BackgroundFieldModel()
                {
                    Name = _attributeName,
                    OwningControl = this,
                    OwningControlModel = new BackgroundControlModel(this),
                    DisplayOrder = DataEntryMethods.GetTabIndices(this),
                    IsViewable = Visible,
                    PersistAttribute = false,
                };
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45503");
            }
        }

        #endregion Unused IDataEntryControl Members

        #region Private Members

        /// <summary>
        /// Raises the <see cref="AttributesSelected"/> event.
        /// </summary>
        void OnAttributesSelected()
        {
            if (this.AttributesSelected != null)
            {
                var selectionState = SelectionState.Create(this);
                AttributesSelected(this, new AttributesSelectedEventArgs(selectionState));
            }
        }

        #endregion Private Members
    }
}
