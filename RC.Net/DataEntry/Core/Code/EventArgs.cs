using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Forms;
using UCLID_AFCORELib;
using UCLID_COMUTILSLib;

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
        /// The <see cref="SelectionState"/> representing the new selection.
        /// </summary>
        SelectionState _selectionState;

        /// <summary>
        /// Initializes a new <see cref="AttributesSelectedEventArgs"/> instance.
        /// </summary>
        /// <param name="selectionState">The <see cref="SelectionState"/> representing the new
        /// selection.</param>
        public AttributesSelectedEventArgs(SelectionState selectionState)
        {
            _selectionState = selectionState;
        }

        /// <summary>
        /// Gets the <see cref="SelectionState"/> representing the new selection.
        /// </summary>
        /// <value>The <see cref="SelectionState"/> representing the new selection.</value>
        public SelectionState SelectionState
        {
            get
            {
                return _selectionState;
            }
        }
    }

    /// <summary>
    /// Provides the <see cref="IUnknownVector"/> of <see cref="IAttribute"/>s 
    /// associated with an event.
    /// </summary>
    public class AttributesEventArgs : EventArgs
    {
        /// <summary>
        /// The <see cref="IUnknownVector"/> of <see cref="IAttribute"/>s associated with the event.
        /// </summary>
        readonly IUnknownVector _attributes;

        /// <summary>
        /// Initializes a new <see cref="AttributesEventArgs"/> instance.
        /// </summary>
        /// <param name="attributes">The <see cref="IUnknownVector"/> of <see cref="IAttribute"/>s 
        /// associated with the event.</param>
        public AttributesEventArgs(IUnknownVector attributes)
        {
            _attributes = attributes;
        }

        /// <summary>
        /// The <see cref="IUnknownVector"/> of <see cref="IAttribute"/>s associated with the event.
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
    /// Argument for a <see cref="DataEntryControlHost.SwipingStateChanged"/> or 
    /// <see cref="IDataEntryControl.SwipingStateChanged"/> event used to specify that swiping
    /// should either be enabled or disabled.
    /// </summary>
    public class SwipingStateChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Specifies whether swiping is being enabled (true) or disabled (false)
        /// </summary>
        bool _enabled;

        /// <summary>
        /// Initialized a new <see cref="SwipingStateChangedEventArgs"/> instance.
        /// </summary>
        /// <param name="enabled"><see langword="true"/> if swiping is being enabled,
        /// <see langword="false"/> if swiping is being disabled.</param>
        public SwipingStateChangedEventArgs(bool enabled)
        {
            _enabled = enabled;
        }

        /// <summary>
        /// Indicates whether swiping is being enabled or disabled.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if swiping is being enabled, <see langword="false"/> if swiping 
        /// is being disabled.
        /// </returns>
        public bool SwipingEnabled
        {
            get
            {
                return _enabled;
            }
        }
    }

    /// <summary>
    /// Argument for a <see cref="IDataEntryControl.QueryDraggedDataSupported"/> event.
    /// </summary>
    public class QueryDraggedDataSupportedEventArgs : EventArgs
    {
        /// <summary>
        /// The <see cref="DragEventArgs"/> associated with drag event for which the
        /// <see cref="IDataEntryControl.QueryDraggedDataSupported"/> event is being raised.
        /// </summary>
        readonly DragEventArgs _dragDropEventArgs;

        /// <summary>
        /// Initialized a new <see cref="QueryDraggedDataSupportedEventArgs"/> instance.
        /// </summary>
        /// <param name="dragDropEventArgs">The <see cref="DragEventArgs"/> associated with
        /// drag event for which the <see cref="IDataEntryControl.QueryDraggedDataSupported"/>
        /// event is being raised.</param>
        public QueryDraggedDataSupportedEventArgs(DragEventArgs dragDropEventArgs)
        {
            _dragDropEventArgs = dragDropEventArgs;
        }

        /// <summary>
        /// Gets the <see cref="DragEventArgs"/> associated with drag event for which the
        /// <see cref="IDataEntryControl.QueryDraggedDataSupported"/> event is being raised.
        /// </summary>
        /// <returns>The <see cref="DragEventArgs"/> associated with drag event for which the
        /// <see cref="IDataEntryControl.QueryDraggedDataSupported"/> event is being raised.
        /// </returns>
        public DragEventArgs DragDropEventArgs
        {
            get
            {
                return _dragDropEventArgs;
            }
        }
    }

    /// <summary>
    /// Provides the <see cref="IAttribute"/> being initialized as part of an 
    /// <see cref="AttributeStatusInfo.AttributeInitialized"/> event and the source 
    /// <see cref="IUnknownVector"/> of <see cref="IAttribute"/>s from which the attribute is from.
    /// </summary>
    public class AttributeInitializedEventArgs : EventArgs
    {
        /// <summary>
        /// The <see cref="IAttribute"/> being initialized.
        /// </summary>
        readonly IAttribute _attribute;

        /// <summary>
        /// The <see cref="IUnknownVector"/> of <see cref="IAttribute"/>s from which the attribute
        /// is from.
        /// </summary>
        readonly IUnknownVector _sourceAttributes;

        /// <summary>
        /// The <see cref="IDataEntryControl"/> associated with the attribute.
        /// </summary>
        readonly IDataEntryControl _dataControl;

        /// <summary>
        /// Initializes a new <see cref="AttributeInitializedEventArgs"/> instance.
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> being initialized.</param>
        /// <param name="sourceAttributes">The <see cref="IUnknownVector"/> of 
        /// <see cref="IAttribute"/>s from which the attribute is from.</param>
        /// <param name="dataEntryControl">The <see cref="IDataEntryControl"/> associated with the 
        /// attribute.</param>
        public AttributeInitializedEventArgs(IAttribute attribute, IUnknownVector sourceAttributes,
            IDataEntryControl dataEntryControl)
        {
            try
            {
                _attribute = attribute;
                _sourceAttributes = sourceAttributes;
                _dataControl = dataEntryControl;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI24779", ex);
            }
        }

        /// <summary>
        /// The <see cref="IAttribute"/> being initialized.
        /// </summary>
        public IAttribute Attribute
        {
            get
            {
                return _attribute;
            }
        }

        /// <summary>
        /// The <see cref="IUnknownVector"/> of <see cref="IAttribute"/>s from which the attribute
        /// is from.
        /// </summary>
        public IUnknownVector SourceAttributes
        {
            get
            {
                return _sourceAttributes;
            }
        }

        /// <summary>
        /// Gets the <see cref="IDataEntryControl"/> associated with the attribute.
        /// </summary>
        /// <returns>The <see cref="IDataEntryControl"/> associated with the attribute.</returns>
        public IDataEntryControl DataEntryControl
        {
            get
            {
                return _dataControl;
            }
        }
    }

    /// <summary>
    /// Provides information about an event in which an <see cref="IAttribute"/>'s value was
    /// modified.
    /// </summary>
    public class AttributeValueModifiedEventArgs : EventArgs
    {
        /// <summary>
        /// The <see cref="IAttribute"/> whose value was modified.
        /// </summary>
        readonly IAttribute _attribute;

        /// <summary>
        /// Specifies whether the modification is part of an ongoing edit.
        /// </summary>
        readonly bool _incrementalUpdate;

        /// <summary>
        /// Specifies whether the modification should trigger the attribute's spatial info to be
        /// accepted.
        /// </summary>
        readonly bool _acceptSpatialInfo;

        /// <summary>
        /// Specifies whether the spatial info for the <see cref="IAttribute"/> has changed.
        /// </summary>
        readonly bool _spatialInfoChanged;

        /// <summary>
        /// A list of all attributes whose values have been updated via an auto-update query as a
        /// result of this event.
        /// </summary>
        List<IAttribute> _autoUpdatedAttributes = new List<IAttribute>();

        /// <summary>
        /// Initializes a new <see cref="AttributeValueModifiedEventArgs"/> instance.
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> whose value was modified.</param>
        /// <param name="incrementalUpdate"><see langword="true"/>if the modification is part of an
        /// ongoing edit; <see langword="false"/> if the edit has finished.</param>
        /// <param name="acceptSpatialInfo"><see langword="true"/> if the modification should
        /// trigger the <see cref="IAttribute"/>'s spatial info to be accepted,
        /// <see langword="false"/> if the spatial info acceptance state should be left as is.
        /// </param>
        /// <param name="spatialInfoChanged"><see langword="true"/> if the spatial info for the
        /// <see cref="IAttribute"/> has changed, <see langword="false"/> if only the text has
        /// changed.</param>
        public AttributeValueModifiedEventArgs(IAttribute attribute, bool incrementalUpdate,
            bool acceptSpatialInfo, bool spatialInfoChanged)
        {
            try
            {
                _attribute = attribute;
                _incrementalUpdate = incrementalUpdate;
                _acceptSpatialInfo = acceptSpatialInfo;
                _spatialInfoChanged = spatialInfoChanged;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI26129", ex);
            }
        }

        /// <summary>
        /// Gets the <see cref="IAttribute"/> that was modified.
        /// </summary>
        /// <returns>
        /// The <see cref="IAttribute"/> that was modified.
        /// </returns>
        public IAttribute Attribute
        {
            get
            {
                return _attribute;
            }
        }

        /// <summary>
        /// Gets whether the modification is part of an ongoing edit.
        /// </summary>
        /// <returns><see langword="true"/>if the modification is part of an
        /// ongoing edit; <see langword="false"/> if the edit has finished.</returns>
        public bool IncrementalUpdate
        {
            get
            {
                return _incrementalUpdate;
            }
        }

        /// <summary>
        /// Gets whether the modification should trigger the <see cref="IAttribute"/>'s spatial
        /// info to be accepted.
        /// </summary>
        /// <returns><see langword="true"/> if the modification should trigger the
        /// <see cref="IAttribute"/>'s spatial info to be accepted, <see langword="false"/> if the
        /// spatial info acceptance state should be left as is.</returns>
        public bool AcceptSpatialInfo
        {
            get
            {
                return _acceptSpatialInfo;
            }
        }

        /// <summary>
        /// Gets whether the spatial info for the <see cref="IAttribute"/> has changed.
        /// </summary>
        /// <returns><see langword="true"/> if the spatial info for the <see cref="IAttribute"/>
        /// has changed, <see langword="false"/> if only the text has changed.</returns>
        public bool SpatialInfoChanged
        {
            get
            {
                return _spatialInfoChanged;
            }
        }

        /// <summary>
        /// Gets a list of all <see cref="IAttribute"/>s whose values have been updated via an
        /// auto-update query as a result of this event.
        /// </summary>
        /// <returns>A list of all <see cref="IAttribute"/>s whose values have been updated via an
        /// auto-update query as a result of this event.</returns>
        // Using List for its "Contains" method.
        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        public List<IAttribute> AutoUpdatedAttributes
        {
            get
            {
                return _autoUpdatedAttributes;
            }
        }
    }

    /// <summary>
    /// Provides information about an event in which an <see cref="IAttribute"/>'s value was
    /// modified.
    /// </summary>
    public class QueryValueModifiedEventArgs : EventArgs
    {
        /// <summary>
        /// Specifies whether the modification is part of an ongoing edit.
        /// </summary>
        readonly bool _incrementalUpdate;

        /// <summary>
        /// Initializes a new <see cref="QueryValueModifiedEventArgs"/> instance.
        /// </summary>
        /// <param name="incrementalUpdate"><see langword="true"/>if the modification is part of an
        /// ongoing edit; <see langword="false"/> if the edit has finished.</param>
        public QueryValueModifiedEventArgs(bool incrementalUpdate)
        {
            try
            {
                _incrementalUpdate = incrementalUpdate;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI38255", ex);
            }
        }

        /// <summary>
        /// Gets whether the modification is part of an ongoing edit.
        /// </summary>
        /// <returns><see langword="true"/>if the modification is part of an
        /// ongoing edit; <see langword="false"/> if the edit has finished.</returns>
        public bool IncrementalUpdate
        {
            get
            {
                return _incrementalUpdate;
            }
        }
    }

    /// <summary>
    /// Provides information about an event in which an <see cref="IAttribute"/> was delete from an
    /// <see cref="IDataEntryControl"/>.
    /// </summary>
    public class AttributeDeletedEventArgs : EventArgs
    {
        /// <summary>
        /// The <see cref="IAttribute"/> that was deleted.
        /// </summary>
        IAttribute _deletedAttribute;

        /// <summary>
        /// Initializes a new <see cref="AttributeDeletedEventArgs"/> instance.
        /// </summary>
        /// <param name="deletedAttribute">The <see cref="IAttribute"/> that was deleted.</param>
        public AttributeDeletedEventArgs(IAttribute deletedAttribute)
        {
            _deletedAttribute = deletedAttribute;
        }

        /// <summary>
        /// Gets the <see cref="IAttribute"/> that was deleted.
        /// </summary>
        /// <returns>
        /// The <see cref="IAttribute"/> that was deleted.
        /// </returns>
        public IAttribute DeletedAttribute
        {
            get
            {
                return _deletedAttribute;
            }
        }
    }

    /// <summary>
    /// Provides information about an event in which viewed/unviewed status of an
    /// <see cref="IAttribute"/> changed.
    /// </summary>
    public class ViewedStateChangedEventArgs : EventArgs
    {
        /// <summary>
        /// The <see cref="IAttribute"/> for which the viewed/unviewed status has changed.
        /// </summary>
        readonly IAttribute _attribute;

        /// <summary>
        /// Specifies whether the attribute has been marked viewed or unviewed.
        /// </summary>
        readonly bool _dataIsViewed;

        /// <summary>
        /// Initializes a new <see cref="ViewedStateChangedEventArgs"/> instance.
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> for which the viewed/unviewed 
        /// status has changed.</param>
        /// <param name="dataIsViewed"><see langword="true"/> if the <see cref="IAttribute"/>
        /// has been marked as viewed, <see langword="false"/> if the <see cref="IAttribute"/>
        /// has been marked as unviewed.</param>
        public ViewedStateChangedEventArgs(IAttribute attribute, bool dataIsViewed)
            : base()
        {
            _attribute = attribute;
            _dataIsViewed = dataIsViewed;
        }

        /// <summary>
        /// Gets the <see cref="IAttribute"/> for which the viewed/unviewed status has changed.
        /// </summary>
        /// <returns>The <see cref="IAttribute"/> for which the viewed/unviewed status has changed.
        /// </returns>
        public IAttribute Attribute
        {
            get
            {
                return _attribute;
            }
        }

        /// <summary>
        /// Gets whether the attribute has been marked viewed or unviewed.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if the <see cref="IAttribute"/> has been marked as viewed, 
        /// <see langword="false"/> if the <see cref="IAttribute"/> has been marked as unviewed.
        /// </returns>
        public bool IsDataViewed
        {
            get
            {
                return _dataIsViewed;
            }
        }
    }

    /// <summary>
    /// Provides information about an event in which an <see cref="IAttribute"/>'s data has been
    /// marked as valid or invalid.
    /// </summary>
    public class ValidationStateChangedEventArgs : EventArgs
    {
        /// <summary>
        /// The <see cref="IAttribute"/> for which the data validity has changed.
        /// </summary>
        readonly IAttribute _attribute;

        /// <summary>
        /// Specifies whether the <see cref="IAttribute"/>'s data is now valid or invalid.
        /// </summary>
        readonly DataValidity _dataValidity;

        /// <summary>
        /// Initializes a new <see cref="ValidationStateChangedEventArgs"/> instance.
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> for which the data validity has 
        /// changed.</param>
        /// <param name="dataValidity">An <see cref="DataValidity"/>value indicating whether
        /// <see paramref="attribute"/>'s value is now valid.</param>
        public ValidationStateChangedEventArgs(IAttribute attribute, DataValidity dataValidity)
            : base()
        {
            _attribute = attribute;
            _dataValidity = dataValidity;
        }

        /// <summary>
        /// Get the <see cref="IAttribute"/> for which the data validity has changed.
        /// </summary>
        /// <returns>The <see cref="IAttribute"/> for which the data validity has changed.</returns>
        public IAttribute Attribute
        {
            get
            {
                return _attribute;
            }
        }

        /// <summary>
        /// Gets whether the <see cref="IAttribute"/>'s data is now valid.
        /// </summary>
        /// <returns>
        /// A <see cref="DataValidity"/> value indicating whether the data is now valid.
        /// </returns>
        public DataValidity DataValidity
        {
            get
            {
                return _dataValidity;
            }
        }
    }

    /// <summary>
    /// Provides information about an event in which new items have been selected.
    /// </summary>
    public class ItemSelectionChangedEventArgs : EventArgs
    {
        /// <summary>
        /// The number of items in the selection with highlights that have been accepted by the
        /// user.
        /// </summary>
        readonly int _selectedItemsWithAcceptedHighlights;

        /// <summary>
        /// The number of items in the selection with highlights that have not been accepted by the
        /// user.
        /// </summary>
        readonly int _selectedItemsWithUnacceptedHighlights;

        /// <summary>
        /// The number of items in the selection without spatial information but that have a direct
        /// hint indicating where the data may be (if present).
        /// </summary>
        readonly int _selectedItemsWithDirectHints;

        /// <summary>
        /// The number of items in the selection without spatial information but that have an indirect
        /// hint indicating data related field.
        /// </summary>
        readonly int _selectedItemsWithIndirectHints;

        /// <summary>
        /// The number of items in the selection without spatial information or hints.
        /// </summary>
        readonly int _selectedItemsWithoutHighlights;

        /// <summary>
        /// Initializes a new <see cref="ItemSelectionChangedEventArgs"/> instance.
        /// </summary>
        /// <param name="selectedItemsWithAcceptedHighlights">The number of items in the selection
        /// with highlights that have been accepted by the user.</param>
        /// <param name="selectedItemsWithUnacceptedHighlights">The number of items in the
        /// selection with highlights that have not been accepted by the user.</param>
        /// <param name="selectedItemsWithDirectHints">The number of items in the selection without
        /// spatial information but that have a direct hint indicating where the data may be (if
        /// present).</param>
        /// <param name="selectedItemsWithIndirectHints">The number of items in the selection
        /// without spatial information but that have an indirect hint indicating data related
        /// field.</param>
        /// <param name="selectedItemsWithoutHighlights">The number of items in the selection
        /// without spatial information or hints.</param>
        public ItemSelectionChangedEventArgs(int selectedItemsWithAcceptedHighlights,
            int selectedItemsWithUnacceptedHighlights, int selectedItemsWithDirectHints,
            int selectedItemsWithIndirectHints, int selectedItemsWithoutHighlights)
            : base()
        {
            try
            {
                _selectedItemsWithAcceptedHighlights = selectedItemsWithAcceptedHighlights;
                _selectedItemsWithUnacceptedHighlights = selectedItemsWithUnacceptedHighlights;
                _selectedItemsWithDirectHints = selectedItemsWithDirectHints;
                _selectedItemsWithIndirectHints = selectedItemsWithIndirectHints;
                _selectedItemsWithoutHighlights = selectedItemsWithoutHighlights;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI25989", ex);
            }
        }

        /// <summary>
        /// Gets the number of items in the selection with highlights that have been accepted by
        /// the user.
        /// </summary>
        /// <returns>The number of items in the selection with highlights that have been accepted
        /// by the user.</returns>
        public int SelectedItemsWithAcceptedHighlights
        {
            get
            {
                return _selectedItemsWithAcceptedHighlights;
            }
        }

        /// <summary>
        /// Gets the number of items in the selection with highlights that have not been accepted
        /// by the user.
        /// </summary>
        /// <returns>The number of items in the selection with highlights that have not been
        /// accepted by the user.</returns>
        public int SelectedItemsWithUnacceptedHighlights
        {
            get
            {
                return _selectedItemsWithUnacceptedHighlights;
            }
        }

        /// <summary>
        /// Gets the number of items in the selection without spatial information but that have a
        /// direct hint indicating where the data may be (if present).
        /// </summary>
        /// <returns> The number of items in the selection without spatial information but that
        /// have a direct hint indicating where the data may be (if present).</returns>
        public int SelectedItemsWithDirectHints
        {
            get
            {
                return _selectedItemsWithDirectHints;
            }
        }

        /// <summary>
        /// Gets the number of items in the selection without spatial information but that have an
        /// indirect hint indicating data related field.
        /// </summary>
        /// <returns>The number of items in the selection without spatial information but that have
        /// an indirect hint indicating data related field.</returns>
        public int SelectedItemsWithIndirectHints
        {
            get
            {
                return _selectedItemsWithIndirectHints;
            }
        }

        /// <summary>
        /// The number of items in the selection without spatial information or hints.
        /// </summary>
        /// <returns>The number of items in the selection without spatial information or hints.
        /// </returns>
        public int SelectedItemsWithoutHighlights
        {
            get
            {
                return _selectedItemsWithoutHighlights;
            }
        }
    }

    /// <summary>
    /// Provides information about an event in which the spatial info of an attribute in a table
    /// has changed.
    /// </summary>
    public class CellSpatialInfoChangedEventArgs : EventArgs
    {
        /// <summary>
        /// The <see cref="IAttribute"/> for which the viewed/unviewed status has changed.
        /// </summary>
        readonly IAttribute _attribute;

        /// <summary>
        /// Initializes a new <see cref="ViewedStateChangedEventArgs"/> instance.
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> for which the viewed/unviewed 
        /// status has changed.</param>
        public CellSpatialInfoChangedEventArgs(IAttribute attribute)
            : base()
        {
            _attribute = attribute;
        }

        /// <summary>
        /// Gets the <see cref="IAttribute"/> for which the viewed/unviewed status has changed.
        /// </summary>
        /// <returns>The <see cref="IAttribute"/> for which the viewed/unviewed status has changed.
        /// </returns>
        public IAttribute Attribute
        {
            get
            {
                return _attribute;
            }
        }
    }

    /// <summary>
    /// Event arguments for the <see cref="DataEntryConfigurationManager.ConfigurationChanged"/> event.
    /// </summary>
    /// <seealso cref="EventArgs" />
    public class ConfigurationInitializedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationChangedEventArgs"/> class.
        /// </summary>
        /// <param name="dataEntryConfig">The initialized data entry configuration.</param>
        public ConfigurationInitializedEventArgs(DataEntryConfiguration dataEntryConfig)
        {
            DataEntryConfiguration = dataEntryConfig;
        }

        /// <summary>
        /// Gets the initialized data entry configuration.
        /// </summary>
        /// <value>
        /// The initialized data entry configuration.
        /// </value>
        public DataEntryConfiguration DataEntryConfiguration
        {
            get;
            private set;
        }
    }

    /// <summary>
    /// Event arguments for the <see cref="DataEntryConfigurationManager.ConfigurationChanged"/> event.
    /// </summary>
    /// <seealso cref="EventArgs" />
    public class ConfigurationChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConfigurationChangedEventArgs"/> class.
        /// </summary>
        /// <param name="oldDataEntryConfig">The old data entry configuration.</param>
        /// <param name="newDataEntryConfig">The new data entry configuration.</param>
        public ConfigurationChangedEventArgs(DataEntryConfiguration oldDataEntryConfig,
            DataEntryConfiguration newDataEntryConfig)
        {
            OldDataEntryConfiguration = oldDataEntryConfig;
            NewDataEntryConfiguration = newDataEntryConfig;
        }

        /// <summary>
        /// Gets the old data entry configuration.
        /// </summary>
        /// <value>
        /// The old data entry configuration.
        /// </value>
        public DataEntryConfiguration OldDataEntryConfiguration
        {
            get;
        }

        /// <summary>
        /// Gets the new data entry configuration.
        /// </summary>
        /// <value>
        /// The new data entry configuration.
        /// </value>
        public DataEntryConfiguration NewDataEntryConfiguration
        {
            get;
        }
    }

    /// <summary>
    /// Event arguments for the events that relate to data entry controls.
    /// </summary>
    public class DataEntryControlEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new <see cref="DataEntryControlEventArgs"/> instance.
        /// </summary>
        /// <param name="dataEntryControl">The <see cref="IDataEntryControl"/> the selection
        /// pertains to.</param>
        /// </param>
        public DataEntryControlEventArgs(IDataEntryControl dataEntryControl)
            : base()
        {
            DataEntryControl = dataEntryControl;
        }

        /// <summary>
        /// Gets the <see cref="IDataEntryControl"/> the event pertains to.
        /// </summary>
        /// <value>The <see cref="IDataEntryControl"/> the event  pertains to.</value>
        public IDataEntryControl DataEntryControl
        {
            get;
        }
    }

    /// <summary>
    /// Event arguments for <see cref="DataEntryControlHost.TabNavigation"/>
    /// </summary>
    public class TabNavigationEventArgs : EventArgs
    {
        public TabNavigationEventArgs(TabNavigationEventArgs eventArgs)
        {
            Forward = eventArgs.Forward;
            LastStop = eventArgs.LastStop;
        }

        /// <summary>
        /// Initializes a new <see cref="TabNavigationEventArgs"/> instance.
        /// </summary>
        /// <param name="forward"><c>true</c> if navigating forward out of the DEP; <c>false</c> if navigating backward.</param>
        /// <param name="lastStop"><c>true</c> if the tab navigation is leaving (or has already left) the sending control.
        /// <c>false</c> if navigation is advancing to another field in the control.</param>
        public TabNavigationEventArgs(bool forward, bool lastStop)
        {
            Forward = forward;
            LastStop = lastStop;
        }

        /// <summary>
        /// <c>true</c> if navigating forward out of the DEP; <c>false</c> if navigating backward.
        /// </summary>
        public bool Forward
        {
            get;
        }

        /// <summary>
        /// <c>true</c> if the tab navigation is leaving (or has already left) the sending control.
        /// <c>false</c> if navigation is advancing to another field in the control.
        /// </summary>
        public bool LastStop
        {
            get;
        }

        /// <summary>
        /// An event handler should set this value to <c>true</c> to indicate it has handled the event
        /// to indicate to the sending control that it should not process with its own navigation
        /// implementation.
        /// </summary>
        public bool Handled
        {
            get;
            set;
        }
    }
}
