using System;
using System.Collections.Generic;
using System.Text;
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
    /// Argument for a <see cref="DataEntryControlHost.SwipingStateChanged"/> or 
    /// <see cref="IDataEntryControl.SwipingStateChanged"/> event used to specify that swiping
    /// should either be enabled or disabled.
    /// </summary>
    public class SwipingStateChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Specifies whether swiping is being enabled (true) or disabled (false)
        /// </summary>
        private bool _enabled;

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
        private readonly IAttribute _attribute;

        /// <summary>
        /// The <see cref="IUnknownVector"/> of <see cref="IAttribute"/>s from which the attribute
        /// is from.
        /// </summary>
        private readonly IUnknownVector _sourceAttributes;

        /// <summary>
        /// The <see cref="IDataEntryControl"/> associated with the attribute.
        /// </summary>
        private readonly IDataEntryControl _dataEntryControl;

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
                _dataEntryControl = dataEntryControl;
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
                return _dataEntryControl;
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
        private readonly IAttribute _attribute;

        /// <summary>
        /// Specifies whether the modification is part of an ongoing edit.
        /// </summary>
        private readonly bool _incrementalUpdate;

        /// <summary>
        /// Specifies whether the modification should trigger the attribute's spatial info to be
        /// accepted.
        /// </summary>
        private readonly bool _acceptSpatialInfo;

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
        public AttributeValueModifiedEventArgs(IAttribute attribute, bool incrementalUpdate, 
            bool acceptSpatialInfo)
        {
            try
            {
                _attribute = attribute;
                _incrementalUpdate = incrementalUpdate;
                _acceptSpatialInfo = acceptSpatialInfo;
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
        private IAttribute _deletedAttribute;

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
        private readonly IAttribute _attribute;

        /// <summary>
        /// Specifies whether the attribute has been marked viewed or unviewed.
        /// </summary>
        private readonly bool _dataIsViewed;

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
        private readonly IAttribute _attribute;

        /// <summary>
        /// Specifies whether the <see cref="IAttribute"/>'s data is now valid or invalid.
        /// </summary>
        private readonly bool _dataIsValid;

        /// <summary>
        /// Initializes a new <see cref="ValidationStateChangedEventArgs"/> instance.
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> for which the data validity has 
        /// changed.</param>
        /// <param name="dataIsValid"><see langword="true"/> if the <see cref="IAttribute"/>'s data
        /// has been marked as valid or <see langword="false"/> if the <see cref="IAttribute"/>'s 
        /// data has been marked as invalid.</param>
        public ValidationStateChangedEventArgs(IAttribute attribute, bool dataIsValid)
            : base()
        {
            _attribute = attribute;
            _dataIsValid = dataIsValid;
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
        /// Gets whether the <see cref="IAttribute"/>'s data is now valid or invalid.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if the <see cref="IAttribute"/>'s data has been marked
        /// as valid or <see langword="false"/> if the <see cref="IAttribute"/>'s data has been
        /// marked as invalid.
        /// </returns>
        public bool IsDataValid
        {
            get
            {
                return _dataIsValid;
            }
        }
    }

    /// <summary>
    /// Provides information about an event in which a search was made for unviewed
    /// <see cref="IAttribute"/>s.
    /// </summary>
    public class UnviewedItemsFoundEventArgs : EventArgs
    {
        /// <summary>
        /// Specifies whether any unviewed attributes were found in the search.
        /// </summary>
        private readonly bool _unviewedItemsFound;

        /// <summary>
        /// Initializes a new <see cref="UnviewedItemsFoundEventArgs"/> instance.
        /// </summary>
        /// <param name="unviewedItemsFound"><see langword="true"/> if unviewed 
        /// <see cref="IAttribute"/>s were found or <see langword="false"/> if no unviewed 
        /// <see cref="IAttribute"/>s were found.</param>
        public UnviewedItemsFoundEventArgs(bool unviewedItemsFound)
            : base()
        {
            _unviewedItemsFound = unviewedItemsFound;
        }

        /// <summary>
        /// Gets whether any unviewed <see cref="IAttribute"/>s were found in the search.
        /// </summary>
        /// <returns><see langword="true"/> if unviewed <see cref="IAttribute"/>s were found or 
        /// <see langword="false"/> if no unviewed <see cref="IAttribute"/>s were found.</returns>
        public bool UnviewedItemsFound
        {
            get
            {
                return _unviewedItemsFound;
            }
        }
    }

    /// <summary>
    /// Provides information about an event in which a search was made for 
    /// <see cref="IAttribute"/>s with invalid data.
    /// </summary>
    public class InvalidItemsFoundEventArgs : EventArgs
    {
        /// <summary>
        /// Specifies whether any <see cref="IAttribute"/>s with invalid data were found in the 
        /// search.
        /// </summary>
        private readonly bool _invalidItemsFound;

        /// <summary>
        /// Initializes a new <see cref="InvalidItemsFoundEventArgs"/> instance.
        /// </summary>
        /// <param name="invalidItemsFound"><see langword="true"/> if <see cref="IAttribute"/>s with
        /// invalid data were found in the search or <see langword="false"/> if all
        /// <see cref="IAttribute"/>s had valid data.</param>
        public InvalidItemsFoundEventArgs(bool invalidItemsFound)
            : base()
        {
            _invalidItemsFound = invalidItemsFound;
        }

        /// <summary>
        /// Gets whether any <see cref="IAttribute"/>s with invalid data were found in the search.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if <see cref="IAttribute"/>s with invalid data were 
        /// found in the search or <see langword="false"/> if all <see cref="IAttribute"/>s had 
        /// valid data.
        /// </returns>
        public bool InvalidItemsFound
        {
            get
            {
                return _invalidItemsFound;
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
        private readonly int _selectedItemsWithAcceptedHighlights;

        /// <summary>
        /// The number of items in the selection with highlights that have not been accepted by the
        /// user.
        /// </summary>
        private readonly int _selectedItemsWithUnacceptedHighlights;

        /// <summary>
        /// The number of items in the selection without spatial information but that have a direct
        /// hint indicating where the data may be (if present).
        /// </summary>
        private readonly int _selectedItemsWithDirectHints;

        /// <summary>
        /// The number of items in the selection without spatial information but that have an indirect
        /// hint indicating data related field.
        /// </summary>
        private readonly int _selectedItemsWithIndirectHints;

        /// <summary>
        /// The number of items in the selection without spatial information or hints.
        /// </summary>
        private readonly int _selectedItemsWithoutHighlights;

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
                ExtractException.AsExtractException("ELI25989", ex);
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
        private readonly IAttribute _attribute;


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
}
