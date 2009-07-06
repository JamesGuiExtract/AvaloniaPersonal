using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;
using UCLID_AFCORELib;
using UCLID_COMUTILSLib;
using UCLID_RASTERANDOCRMGMTLib;

namespace Extract.DataEntry
{
    /// <summary>
    /// A <see cref="GroupBox"/> extension which can be to pass data to
    /// <see cref="IDataEntryControl"/>s which share a common parent attribute whose value does not
    /// need to be viewable.
    /// </summary>
    public partial class DataEntryGroupBox : GroupBox, IDataEntryControl
    {
        #region Fields

        /// <summary>
        /// The name used to identify the <see cref="IAttribute"/> to be associated with the text 
        /// box.
        /// </summary>
        private string _attributeName;

        /// <summary>
        /// Used to specify the data entry control which is mapped to the parent of the attribute 
        /// to which the current group box is to be mapped.
        /// </summary>
        private IDataEntryControl _parentDataEntryControl;

        /// <summary>
        /// The selection mode to use when multiple attributes are found which match the attribute 
        /// name for this control.
        /// </summary>
        private MultipleMatchSelectionMode _multipleMatchSelectionMode =
            MultipleMatchSelectionMode.First;

        /// <summary>
        /// The attribute mapped to this control.
        /// </summary>
        private IAttribute _attribute;

        #endregion Fields

        #region Contstructors

        /// <summary>
        /// Instantiates a new <see cref="DataEntryGroupBox"/> instance.
        /// </summary>
        public DataEntryGroupBox()
            : base()
        {
            try
            {
                InitializeComponent();
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI26031", ex);
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the name identifying the <see cref="IAttribute"/> to be associated with 
        /// the <see cref="DataEntryGroupBox"/>.</summary>
        /// <value>Sets the name identifying the <see cref="IAttribute"/> to be associated with 
        /// the <see cref="DataEntryGroupBox"/>. Specifying <see langword="null"/> will make the
        /// group box a "dependent sibling" to its <see cref="ParentDataEntryControl"/> meaning 
        /// its attribute will share the same name as the control it is dependent upon, but the
        /// specific attribute it is mapped to will be dependent on the current selection in the
        /// <see cref="ParentDataEntryControl"/>.
        /// </value>
        /// <returns>The name indentifying the <see cref="IAttribute"/> to be associated with the 
        /// <see cref="DataEntryGroupBox"/>.</returns>
        [Category("Data Entry Group Box")]
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

        #endregion Properties

        #region IDataEntryControl Members

        /// <summary>
        /// If the <see cref="DataEntryGroupBox"/> is not intended to operate on root-level data, 
        /// this property must be used to specify the <see cref="IDataEntryControl"/> which is 
        /// mapped to the parent of the <see cref="IAttribute"/>(s) to which the current 
        /// <see cref="DataEntryGroupBox"/> is to be mapped.  The specified 
        /// <see cref="IDataEntryControl"/> must be contained in the same 
        /// <see cref="DataEntryControlHost"/> as this <see cref="DataEntryGroupBox"/>. If the 
        /// <see cref="DataEntryGroupBox"/> is to be mapped to a root-level <see cref="IAttribute"/>, 
        /// this property must be set to <see langref="null"/>.</summary>
        /// <seealso cref="IDataEntryControl"/>
        [Category("Data Entry Group Box")]
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
        /// Gets or sets the selection mode to use to find the mapped attribute for the
        /// <see cref="DataEntryGroupBox"/>.
        /// </summary>
        /// <value>The selection mode to use to find the mapped attribute for the
        /// <see cref="DataEntryGroupBox"/>.</value>
        /// <returns>The selection mode to use to find the mapped attribute for the
        /// <see cref="DataEntryGroupBox"/>.</returns>
        [Category("Data Entry Group Box")]
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
        /// Specifies the domain of <see cref="IAttribute"/>s from which the 
        /// <see cref="IDataEntryControl"/> should find the <see cref="IAttribute"/> 
        /// to which it should be mapped (based on the <see cref="AttributeName"/> property). 
        /// If there are multiple attributes, the <see cref="MultipleMatchSelectionMode"/> property
        /// will be used to decide to which <see cref="IAttribute"/> it should map 
        /// itself.
        /// </summary>
        /// <param name="sourceAttributes">The <see cref="IUnknownVector"/> instance of 
        /// <see cref="IAttribute"/>s from which the <see cref="DataEntryGroupBox"/> 
        /// should find its corresponding <see cref="IAttribute"/>.  Can be an empty vector, but 
        /// must not be <see langword="null"/>.</param>
        /// <seealso cref="IDataEntryControl"/>
        public void SetAttributes(IUnknownVector sourceAttributes)
        {
            try
            {
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
                        sourceAttributes, null, this, 0, false, false, null, null, null);
                }

                OnPropagateAttributes();
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI26035", ex);
            }
        }

        /// <summary>
        /// Handles the case that this <see cref="DataEntryGroupBox"/>'s 
        /// <see cref="ParentDataEntryControl"/> has requested that a new <see cref="IAttribute"/> 
        /// be propagated.  The <see cref="DataEntryGroupBox"/> will re-map its control appropriately.
        /// The <see cref="DataEntryGroupBox"/> will mark the <see cref="AttributeStatusInfo"/> instance 
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
                throw ExtractException.AsExtractException("ELI26039", ex);
            }
        }

        /// <summary>
        /// This method has no effect for a <see cref="DataEntryGroupBox"/> except to validate that
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
            ExtractException.Assert("ELI26037", "Unexpected attribute!",
                attribute == null || attribute == _attribute);
        }

        /// <summary>
        /// Fired to request that the <see cref="IAttribute"/> or <see cref="IAttribute"/>(s) be
        /// propagated to any dependent controls.  This will be in response to the
        /// <see cref="DataEntryGroupBox"/>'s attribute being re-mapped.
        /// The event will provide the updated <see cref="IAttribute"/> to registered listeners.
        /// </summary>
        public event EventHandler<PropagateAttributesEventArgs> PropagateAttributes;

        #endregion IDataEntryControl Members

        #region Unused IDataEntryControl Members

        /// <summary>
        /// This event is not raised by <see cref="DataEntryGroupBox"/>.
        /// </summary>
        public event EventHandler<AttributesSelectedEventArgs> AttributesSelected
        {
            // Since this event is not currently used by this class but is needed by the 
            // IDataEntryControl interface, define it with an empty implementation to prevent
            // "unused" warnings during compile.
            add { }
            remove { }
        }

        /// <summary>
        /// This event is not raised by <see cref="DataEntryGroupBox"/>.
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
        /// <see cref="DataEntryGroupBox"/> does not support swiping; the value of this property
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
                    throw new ExtractException("ELI26033", 
                        "DataEntryGroup box does not support swiping!");
                }
            }
        }

        /// <summary>
        /// <see cref="DataEntryGroupBox"/> does not have any implementation for this method.
        /// </summary>
        /// <param name="setActive">Unused.</param>
        /// <param name="color">Unused.</param>
        public void IndicateActive(bool setActive, System.Drawing.Color color)
        {
        }

        /// <summary>
        /// <see cref="DataEntryGroupBox"/> does not have any implementation for this method.
        /// </summary>
        /// <throws><see cref="ExtractException"/> if called.</throws>
        /// <param name="swipedText">Unused.</param>
        public void ProcessSwipedText(SpatialString swipedText)
        {
            throw new ExtractException("ELI26038", "DataEntryGroupBox does not support swiping!");
        }

        /// <summary>
        /// <see cref="DataEntryGroupBox"/> does not have any implementation for this method.
        /// </summary>
        /// <param name="attribute">Unused.</param>
        public virtual void RefreshAttribute(IAttribute attribute)
        {
        }

        /// <summary>
        /// Any data that was cached should be cleared;  This is called when a document is unloaded.
        /// If controls fail to clear COM objects, errors may result if that data is accessed when
        /// a subsequent document is loaded.
        /// </summary>
        public void ClearCachedData()
        {
        }

        #endregion Unused IDataEntryControl Members

        #region Private Members

        /// <summary>
        /// Raises the <see cref="PropagateAttributes"/> event.
        /// </summary>
        private void OnPropagateAttributes()
        {
            if (this.PropagateAttributes != null)
            {
                if (_attribute == null)
                {
                    // If the group box is not currently mapped to an attribute, it should propagate
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

        #endregion Private Members
    }
}
