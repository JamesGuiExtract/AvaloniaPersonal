using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Reflection;
using System.Text;
using UCLID_AFCORELib;
using UCLID_COMUTILSLib;
using UCLID_RASTERANDOCRMGMTLib;

namespace Extract.DataEntry
{
    public enum DataEntryAutoCompleteMode
    {
        /// <summary>
        /// Disables the automatic completion feature for the System.Windows.Forms.ComboBox
        /// and System.Windows.Forms.TextBox controls.
        /// </summary>
        None = 0,

        /// <summary>
        /// Displays the auxiliary drop-down list associated with the edit control. This
        /// drop-down is populated with one or more suggested completion strings.
        /// </summary>
        Suggest = 1,

        /// <summary>
        /// Appends the remainder of the most likely candidate string to the existing characters,
        /// highlighting the appended characters.
        /// </summary>
        Append = 2,

        /// <summary>
        /// Applies both Suggest and Append options.
        /// </summary>
        SuggestAppend = 3,

        /// <summary>
        /// Use <see cref="LuceneAutoSuggest"/> to show a suggestion list
        /// </summary>
        SuggestLucene = 4,
    }

    public enum AutoDropDownMode
    {
        /// <summary>
        /// Don't automatically show the suggestion list when the control gets focus
        /// </summary>
        Never = 0,

        /// <summary>
        /// Automatically show the suggestion list when the control gets focus
        /// </summary>
        Always = 1,

        /// <summary>
        /// Automatically show the suggestion list when the control gets focus if Text is empty or is only whitespace
        /// </summary>
        WhenEmpty = 2,
    }

    /// <summary>
    /// Interface to support use of <see cref="DataEntryAutoCompleteMode"/>
    /// </summary>
    public interface IDataEntryAutoCompleteControl
    {
        /// <summary>
        /// The <see cref="DataEntryAutoCompleteMode"/> to use for text box editing controls
        /// </summary>
        DataEntryAutoCompleteMode AutoCompleteMode
        {
            get;
            set;
        }

        /// <summary>
        /// Determines whether to show the list as soon as this control gets focus
        /// </summary>
        AutoDropDownMode AutoDropDownMode { get; set; }

        /// <summary>
        /// When <c>true</c> the best match will be automatically selected in the list while typing.
        /// When <c>false</c> arrow keys or the mouse must be used to select an item.
        /// </summary>
        bool AutomaticallySelectBestMatchingItem { get; set; }
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
        event EventHandler<AttributesEventArgs> PropagateAttributes;

        /// <summary>
        /// Fired when a <see cref="IDataEntryControl"/> has been manipulated in such a way that 
        /// swiping should be either enabled or disabled.
        /// </summary>
        event EventHandler<SwipingStateChangedEventArgs> SwipingStateChanged;

        /// <summary>
        /// Raised by a control whenever data is being dragged to query dependent controls on whether
        /// they would be able to handle the dragged data if it was dropped.
        /// </summary>
        event EventHandler<QueryDraggedDataSupportedEventArgs> QueryDraggedDataSupported;

        /// <summary>
        /// Indicates that a control has begun an update and that the
        /// <see cref="DataEntryControlHost"/> should not redraw highlights, etc, until the update
        /// is complete.
        /// <para><b>NOTE:</b></para>
        /// This event should only be raised for updates that initiated via user interation with the
        /// control. It should not be raised for updates triggered by the
        /// <see cref="DataEntryControlHost"/> such as <see cref="ProcessSwipedText"/>.
        /// </summary>
        event EventHandler<EventArgs> UpdateStarted;

        /// <summary>
        /// Indicates that a control has ended an update and actions that needs to be taken by the
        /// <see cref="DataEntryControlHost"/> such as re-drawing highlights can now proceed.
        /// </summary>
        event EventHandler<EventArgs> UpdateEnded;

        #endregion Events

        #region Properties

        /// <summary>
        /// Gets or sets the <see cref="DataEntryControlHost"/> to which this control belongs
        /// </summary>
        /// <value>The <see cref="DataEntryControlHost"/> to which this control belongs.</value>
        DataEntryControlHost DataEntryControlHost
        {
            get;
            set;
        }

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
        /// Disabled is a DataEntry framework property that should be used in place of Control.Enabled
        /// The Enabled status of a field will be dynamically changed when data is loaded/unloaded or
        /// when data is propagated from parent controls; thus Enabled should be left to the DE
        /// framework to control.
        /// Disabled controls will not perform validation on mapped data.
        /// </summary>
        bool Disabled
        {
            get;
            set;
        }

        /// <summary>
        /// Indicates whether the control is visible.
        /// </summary>
        bool Visible
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets whether the clipboard contents should be cleared after pasting into the
        /// control.
        /// </summary>
        /// <value><see langword="true"/> if the clipboard should be cleared after pasting,
        /// <see langword="false"/> otherwise.</value>
        bool ClearClipboardOnPaste
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets whether descendant attributes in other controls should be highlighted.
        /// </summary>
        /// <value><see langword="true"/> if descendant attributes in other controls;
        /// <see langword="false"/> otherwise.</value>
        bool HighlightSelectionInChildControls
        {
            get;
            set;
        }

        /// <summary>
        /// Indicates whether validation is enabled for the control. If <c>false</c>, validation
        /// queries will continue to provide auto-complete lists and alter case if
        /// ValidationCorrectsCase is set for any field, but it will not show any data errors or
        /// warnings or prevent saving of the document.
        /// </summary>
        bool ValidationEnabled
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
        /// <param name="selectTabGroup">If <see langword="true"/> all <see cref="IAttribute"/>s in
        /// the specified <see cref="IAttribute"/>'s tab group are to be selected,
        /// <see langword="false"/> otherwise.</param>
        void PropagateAttribute(IAttribute attribute, bool selectAttribute, bool selectTabGroup);

        /// <summary>
        /// Requests that the <see cref="IDataEntryControl"/> process the supplied 
        /// <see cref="SpatialString"/> as input.
        /// </summary>
        /// <param name="swipedText">The <see cref="SpatialString"/> representing the
        /// recognized text in the swiped image area.</param>
        /// <returns><see langword="true"/> if the control was able to use the swiped text;
        /// <see langword="false"/> if it could not be used.</returns>
        bool ProcessSwipedText(SpatialString swipedText);

        /// <summary>
        /// Requests that the <see cref="IDataEntryControl"/> refresh all <see cref="IAttribute"/>
        /// values to the screen.
        /// </summary>
        void RefreshAttributes();

        /// <summary>
        /// Requests that the <see cref="IDataEntryControl"/> refresh the specified
        /// <see cref="IAttribute"/>s' values to the screen.
        /// </summary>
        /// <param name="spatialInfoUpdated"><see langword="true"/> if the attributes spatial info
        /// has changed so that hints can be updated; <see langword="false"/> if the attributes'
        /// spatial info has not changed.</param>
        /// <param name="attributes">The <see cref="IAttribute"/>s whose values should be refreshed
        /// in the UI.</param>
        void RefreshAttributes(bool spatialInfoUpdated, params IAttribute[] attributes);

        /// <summary>
        /// Gets the UI element associated with the specified <see paramref="attribute"/>. This may
        /// be a type of <see cref="T:Control"/> or it may also be
        /// <see cref="T:DataGridViewElement"/> such as a <see cref="T:DataGridViewCell"/> if the
        /// <see paramref="attribute"/>'s owning control is a table control.
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> for which the UI element is needed.
        /// </param>
        /// <param name="elementName">If a related element is required, the property name of an
        /// object relative to the object mapped directly to the attribute. Multiple references may
        /// be chained by separating with a period. E.g., While a specific attribute may be mapped
        /// to a DataGridViewRow, an elementName of "DataGridView.VerticalScrollBar" could be used
        /// to refer to the scroll bar for the grid.</param>
        object GetAttributeUIElement(IAttribute attribute, string elementName);

        /// <summary>
        /// Any data that was cached should be cleared;  This is called when a document is unloaded.
        /// If controls fail to clear COM objects, errors may result if that data is accessed when
        /// a subsequent document is loaded.
        /// </summary>
        void ClearCachedData();

        /// <summary>
        /// Applies the selection state represented by <see paramref="selectionState"/> to the
        /// control.
        /// </summary>
        /// <param name="selectionState">The <see cref="SelectionState"/> to apply.</param>
        void ApplySelection(SelectionState selectionState);

        /// <summary>
        /// Creates a <see cref="BackgroundFieldModel"/> for representing this control during
        /// a background data load.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        BackgroundFieldModel GetBackgroundFieldModel();

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
        /// <param name="e">An <see cref="AttributesEventArgs"/> that contains the event data.
        /// </param>
        void HandlePropagateAttributes(object sender, AttributesEventArgs e);

        #endregion EventHandlers
    }

    /// <summary>
    /// Interface for <see cref="IDataEntryControl"/>s that have an attribute property
    /// </summary>
    public interface IDataEntrySingleAttributeControl : IDataEntryControl
    {
        /// <summary>
        /// The single attribute mapped to this control
        /// </summary>
        IAttribute Attribute { get; }
    }

    /// <summary>
    /// Interface for <see cref="IDataEntryControl"/>s that have selection start and length properties
    /// </summary>
    public interface IDataEntryTextControl : IDataEntrySingleAttributeControl
    {
        int LastSelectionStart { get; }

        int LastSelectionLength { get; }
    }

}
