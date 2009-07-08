using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Text;
using UCLID_AFCORELib;
using UCLID_COMUTILSLib;
using UCLID_RASTERANDOCRMGMTLib;

namespace Extract.DataEntry
{
    /// <summary>
    /// Provides the <see cref="IUnknownVector"/> of <see cref="IAttribute"/>s
    /// that represent the spatial info to be associated with an 
    /// <see cref="IDataEntryControl.AttributesSelected"/> event.
    /// </summary>
    public class AttributesSelectedEventArgs : EventArgs
    {
        /// <summary>
        /// The <see cref="IUnknownVector"/> of <see cref="IAttribute"/>s associated
        /// with the <see cref="IDataEntryControl.AttributesSelected"/> event.
        /// </summary>
        private readonly IUnknownVector _attributes;

        /// <summary>
        /// Indicates whether the firing control is to be associated only with the spatial info of
        /// the specific attributes provided, or whether it should be associated with the spatial
        /// info of all descendent attributes as well.
        /// </summary>
        private readonly bool _includeSubAttributes;

        /// <summary>
        /// Indicates whether tooltips should be displayed for the attribute(s)
        /// </summary>
        private readonly bool _displayToolTips;

        /// <summary>
        /// Initializes a new <see cref="AttributesSelectedEventArgs"/> instance.
        /// </summary>
        /// <param name="attributes">The <see cref="IUnknownVector"/> of <see cref="IAttribute"/>s 
        /// whose spatial information is to be associated with the <see cref="IDataEntryControl"/>.
        /// </param>
        /// <param name="includeSubAttributes">If <see langword="true"/>, the sender is to be
        /// associated with the spatial info of the supplied attributes as well as all descendents 
        /// of those <see cref="IAttribute"/>s. If <see langword="false"/>  the sender is to be
        /// associated with the spatial info of only the supplied <see cref="IAttribute"/>s 
        /// themselves.</param>
        /// <param name="displayToolTips"><see langword="true"/> if tooltips should be displayed
        /// for the <see paramref="attributes"/>.</param>
        public AttributesSelectedEventArgs(IUnknownVector attributes, bool includeSubAttributes,
            bool displayToolTips)
        {
            _attributes = attributes;
            _includeSubAttributes = includeSubAttributes;
            _displayToolTips = displayToolTips;
        }

        /// <summary>
        /// The <see cref="IUnknownVector"/> of <see cref="IAttribute"/>s associated 
        /// with the <see cref="IDataEntryControl.AttributesSelected"/> event.
        /// </summary>
        public IUnknownVector Attributes
        {
            get
            {
                return _attributes;
            }
        }

        /// <summary>
        /// If <see langword="true"/>, the sender is to be associated with the spatial info of the 
        /// supplied attributes as well as all descendents of those <see cref="IAttribute"/>s. If 
        /// <see langword="false"/>  the sender is to be associated with the spatial info of only
        /// the supplied <see cref="IAttribute"/>s themselves.
        /// </summary>
        public bool IncludeSubAttributes
        {
            get
            {
                return _includeSubAttributes;
            }
        }

        /// <summary>
        /// <see langword="true"/> if tooltips should be displayed for the <see cref="Attributes"/>.
        /// </summary>
        public bool DisplayToolTips
        {
            get
            {
                return _displayToolTips;
            }
        }
    }

    /// <summary>
    /// Provides the <see cref="IUnknownVector"/> of <see cref="IAttribute"/>s 
    /// associated with an <see cref="IDataEntryControl.PropagateAttributes"/> event.
    /// </summary>
    public class PropagateAttributesEventArgs : EventArgs
    {
        /// <summary>
        /// The <see cref="IUnknownVector"/> of <see cref="IAttribute"/>s associated with the
        /// <see cref="IDataEntryControl.PropagateAttributes"/> event.
        /// </summary>
        private readonly IUnknownVector _attributes;

        /// <summary>
        /// Initializes a new <see cref="PropagateAttributesEventArgs"/> instance.
        /// </summary>
        /// <param name="attributes">The <see cref="IUnknownVector"/> of <see cref="IAttribute"/>s 
        /// associated with the <see cref="IDataEntryControl.PropagateAttributes"/> event.</param>
        public PropagateAttributesEventArgs(IUnknownVector attributes)
        {
            _attributes = attributes;
        }

        /// <summary>
        /// The <see cref="IUnknownVector"/> of <see cref="IAttribute"/>s associated with the
        /// <see cref="IDataEntryControl.PropagateAttributes"/> event.
        /// </summary>
        public IUnknownVector Attributes
        {
            get
            {
                return _attributes;
            }
        }
    }

    /// <summary>
    /// Defines a interface that allows a control to act as a data entry control in the data
    /// entry control framework.
    /// <para><b>Requirements:</b></para>
    /// <para>Must be implemented by a class derived from <see cref="System.Windows.Forms.Control"/>.
    /// </para>
    /// </summary>
    // Cannot be CLSCompliant because SetAttributes takes an IAttribute parameter.
    public interface IDataEntryControl
    {
        #region Events

        /// <summary>
        /// Fired whenever the set of selected or active <see cref="IAttribute"/>(s) for a control
        /// changes. This can occur as part of the <see cref="PropagateAttributes"/> event, when
        /// new attribute(s) are created via a swipe or when a new element of the control becomes 
        /// active.
        /// </summary>
        event EventHandler<AttributesSelectedEventArgs> AttributesSelected;

        /// <summary>
        /// Fired to request that the and <see cref="IAttribute"/> or <see cref="IAttribute"/>(s) be
        /// propagated to any dependent controls.  This can be in response to an 
        /// <see cref="IAttribute"/> having been modified (ie, via a swipe or loading a document) or
        /// the result of a new selection in a multi-attribute control. The event will provide the 
        /// updated <see cref="IAttribute"/>(s) to registered listeners.
        /// </summary>
        event EventHandler<PropagateAttributesEventArgs> PropagateAttributes;

        /// <summary>
        /// Fired when a <see cref="IDataEntryControl"/> has been manipulated in such a way that 
        /// swiping should be either enabled or disabled.
        /// </summary>
        event EventHandler<SwipingStateChangedEventArgs> SwipingStateChanged;

        #endregion Events

        #region Properties

        /// <summary>
        /// If the <see cref="IDataEntryControl"/> is not intended to operate on root-level data, 
        /// this property must be used to specify the <see cref="IDataEntryControl"/> which is 
        /// mapped to the parent of the <see cref="IAttribute"/>(s) to which the current 
        /// <see cref="IDataEntryControl"/> is to be mapped.  The specified 
        /// <see cref="IDataEntryControl"/> must be contained in the same 
        /// <see cref="DataEntryControlHost"/> as this <see cref="IDataEntryControl"/>. If the 
        /// <see cref="IDataEntryControl"/> is to be mapped to a root-level 
        /// <see cref="IAttribute"/>, this property must be set to <see langref="null"/>.
        /// </summary>
        IDataEntryControl ParentDataEntryControl
        {
            get;
            set;
        }

        /// <summary>
        /// If <see langword="true"/>, the control will accept as input the <see cref="SpatialString"/> 
        /// associated with an image swipe.  If <see langword="false"/>, the control will not and the 
        /// swiping tool should be disabled while the control is active.
        /// </summary>
        bool SupportsSwiping
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets whether the control should remain disabled at all times.
        /// <para><b>Note</b></para>
        /// Disabled controls will not perform validation on mapped data.
        /// </summary>
        bool Disabled
        {
            get;
            set;
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Specifies the domain of attributes from which the control should find its mapping 
        /// <para><b>NOTE:</b></para>The control is responsible for removing any attributes from the 
        /// supplied vector that are not to be included in output at the time this method is called.
        /// If <see langword="null"/> is passed in, it indicates that the control is not currently
        /// mapped to the attribute hierarchy. The control should disable itself and refrain from
        /// raising events dealing with specific <see cref="IAttribute"/>s.
        /// </summary>
        /// <param name="sourceAttributes">An <see cref="IUnknownVector"/> vector of 
        /// <see cref="IAttribute"/>s in which the control should find its mapping.
        /// </param>
        void SetAttributes(IUnknownVector sourceAttributes);

        /// <summary>
        /// Activates or inactivates an <see cref="IDataEntryControl"/> instance.
        /// </summary>
        /// <param name="setActive">If <see langref="true"/>, the control should visually indicate 
        /// that it is active. If <see langref="false"/> the control should not visually indicate
        /// that it is active.</param>
        /// <param name="color">The <see cref="Color"/> that should be used to indicate active status (unused if
        /// setActive is <see langword="false"/>).</param>
        void IndicateActive(bool setActive, Color color);

        /// <summary>
        /// Commands a multi-selection control to propogate the specified <see cref="IAttribute"/>
        /// onto dependent <see cref="IDataEntryControl"/>s.
        /// <para><b>Note</b></para>
        /// For <see cref="IDataEntryControl"/>s controlling only a single <see cref="IAttribute"/> 
        /// this method has no effect.
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
        void PropagateAttribute(IAttribute attribute, bool selectAttribute);

        /// <summary>
        /// Requests that the <see cref="IDataEntryControl"/> process the supplied 
        /// <see cref="SpatialString"/> as input.
        /// </summary>
        /// <param name="swipedText">The <see cref="SpatialString"/> representing the
        /// recognized text in the swiped image area.</param>
        void ProcessSwipedText(SpatialString swipedText);

        /// <summary>
        /// Requests that the <see cref="IDataEntryControl"/> refresh the specified
        /// <see cref="IAttribute"/>'s value to the screen.
        /// <para><b>NOTE:</b></para>
        /// Since it is possible this will be called while attributes are being initialized,
        /// control's should not throw exceptions if it doesn't currently have the specified
        /// <see cref="IAttribute"/> available to refresh.
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> whose value should be refreshed
        /// in the UI.</param>
        void RefreshAttribute(IAttribute attribute);

        /// <summary>
        /// Any data that was cached should be cleared;  This is called when a document is unloaded.
        /// If controls fail to clear COM objects, errors may result if that data is accessed when
        /// a subsequent document is loaded.
        /// </summary>
        void ClearCachedData();

        #endregion Methods

        #region EventHandlers

        /// <summary>
        /// Handles the case that this <see cref="IDataEntryControl"/>'s 
        /// <see cref="ParentDataEntryControl"/> has requested that a new <see cref="IAttribute"/> 
        /// be propagated.  The receiving control should re-map its control appropriately.
        /// <para><b>Requirements</b></para>
        /// Any control implmenting this interface needs to mark the 
        /// <see cref="AttributeStatusInfo"/> instance associated with any 
        /// <see cref="IAttribute"/>(s) it propagates as propagated.
        /// <para><b>NOTE:</b></para>
        /// If the vector of attributes being propagated is <see langword="null"/>, empty, or can
        /// otherwise not be mapped to the control, the control should disable itself and refrain
        /// from raising events dealing with specific <see cref="IAttribute"/>s.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">An <see cref="PropagateAttributesEventArgs"/> that contains the event data.
        /// </param>
        void HandlePropagateAttributes(object sender, PropagateAttributesEventArgs e);

        #endregion EventHandlers
    }
}
