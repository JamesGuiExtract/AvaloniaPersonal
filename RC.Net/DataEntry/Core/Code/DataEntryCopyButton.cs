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
    public partial class DataEntryCopyButton : Button, IDataEntryControl, IRequiresErrorProvider
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

                Clipboard.SetText(_copySourceControl.Text);

                _errorProvider.SetError(this, "");
                AttributeStatusInfo.MarkDataAsValid(_attribute, true);

                base.OnClick(e);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI26979", ex);
            }
        }

        #endregion Overrides

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

        #region IDataEntryControl Members

        /// <summary>
        /// Fired whenever the set of selected or active <see cref="IAttribute"/>(s) for a control
        /// changes. This can occur as part of the <see cref="PropagateAttributes"/> event, when
        /// new attribute(s) are created via a swipe or when a new element of the control becomes 
        /// active.
        /// </summary>
        public event EventHandler<AttributesSelectedEventArgs> AttributesSelected;

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
                base.Enabled = (sourceAttributes != null && !_disabled);

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
                        sourceAttributes, null, this, 0, false, _tabStopMode, null, null, null);

                    if (base.Visible)
                    {
                        AttributeStatusInfo.MarkAsViewable(_attribute, true);

                        // If the attribute has not yet been marked as not persistable, this is the
                        // first time it has been displayed-- initialize the attribute as invalid.
                        if (!_disabled && AttributeStatusInfo.IsAttributePersistable(_attribute))
                        {
                            _errorProvider.SetError(this, _validationErrorMessage);
                            AttributeStatusInfo.MarkDataAsValid(_attribute, false);
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
        /// <seealso cref="IDataEntryControl"/>
        public void PropagateAttribute(IAttribute attribute, bool selectAttribute)
        {
            ExtractException.Assert("ELI26978", "Unexpected attribute!",
                attribute == null || attribute == _attribute);
        }

        #endregion IDataEntryControl Members

        #region Unused IDataEntryControl Members

        /// <summary>
        /// This event is not raised by <see cref="DataEntryCopyButton"/>.
        /// </summary>
        public event EventHandler<PropagateAttributesEventArgs> PropagateAttributes
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
                        "DataEntryCopyButton box does not support swiping!");
                }
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
        public void ProcessSwipedText(UCLID_RASTERANDOCRMGMTLib.SpatialString swipedText)
        {
            throw new ExtractException("ELI27008", "DataEntryCopyButton does not support swiping!");
        }

        /// <summary>
        /// <see cref="DataEntryCopyButton"/> does not have any implementation for this method.
        /// </summary>
        /// <param name="attribute">Unused.</param>
        public void RefreshAttribute(UCLID_AFCORELib.IAttribute attribute)
        {
        }

        /// <summary>
        /// <see cref="DataEntryCopyButton"/> doesn't cache any data.
        /// </summary>
        public void ClearCachedData()
        {
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
                AttributesSelected(this,
                    new AttributesSelectedEventArgs(DataEntryMethods.AttributeAsVector(_attribute),
                        false, true));
            }
        }

        #endregion Private Members
    }
}
