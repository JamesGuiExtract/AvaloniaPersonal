using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UCLID_AFCORELib;
using UCLID_COMUTILSLib;

namespace Extract.DataEntry
{
    /// <summary>
    /// Represents the current selection within the data entry framework. This includes both the
    /// selected <see cref="IAttribute"/>s as well as the indicated selection within the related
    /// <see cref="IDataEntryControl"/>.
    /// </summary>
    public class SelectionState
    {
        /// <summary>
        /// The <see cref="IDataEntryControl"/> the selection pertains to.
        /// </summary>
        readonly IDataEntryControl _dataEntryControl;

        /// <summary>
        /// The selected <see cref="IAttribute"/>s.
        /// </summary>
        readonly ReadOnlyCollection<IAttribute> _attributes;

        /// <summary>
        /// Indicates whether tooltips should be displayed for the attribute(s)
        /// </summary>
        readonly bool _displayToolTips;

        /// <summary>
        /// The <see cref="IAttribute"/> representing a currently selected row or group.
        /// </summary>
        readonly IAttribute _selectedGroupAttribute;

        /// <summary>
        /// Initializes a new <see cref="SelectionState"/> instance for which a single-attribute
        /// control is selected.
        /// </summary>
        /// <param name="dataEntryControl">The <see cref="IDataEntryControl"/> the selection
        /// pertains to.</param>
        public SelectionState(IDataEntrySingleAttributeControl dataEntryControl)
            : this(dataEntryControl, new[] { dataEntryControl.Attribute }, false, true, null)
        { 
        }

        /// <summary>
        /// Initializes a new <see cref="SelectionState"/> instance.
        /// </summary>
        /// <param name="dataEntryControl">The <see cref="IDataEntryControl"/> the selection
        /// pertains to.</param>
        /// <param name="attributes">The selected <see cref="IAttribute"/>s.</param>
        /// <param name="includeSubAttributes">If <see langword="true"/>, all descendents 
        /// of the <see cref="IAttribute"/>s should be included in the selection,
        /// <see langword="false"/> otherwise.</param>
        /// <param name="displayToolTips"><see langword="true"/> if tooltips should be displayed
        /// for the <see paramref="attributes"/>.</param>
        /// <param name="selectedGroupAttribute">The <see cref="IAttribute"/> representing a
        /// currently selected row or group. <see langword="null"/> if no row or group is selected.
        /// </param>
        protected SelectionState(IDataEntryControl dataEntryControl, IEnumerable<IAttribute> attributes,
            bool includeSubAttributes, bool displayToolTips, IAttribute selectedGroupAttribute)
            : this(dataEntryControl, 
                DataEntryMethods.ToAttributeEnumerable(attributes, includeSubAttributes),
                displayToolTips, selectedGroupAttribute)
        {
        }

        /// <summary>
        /// Initializes a new <see cref="SelectionState"/> instance.
        /// </summary>
        /// <param name="dataEntryControl">The <see cref="IDataEntryControl"/> the selection
        /// pertains to.</param>
        /// <param name="attributes">The selected <see cref="IAttribute"/>s.</param>
        /// <param name="displayToolTips"><see langword="true"/> if tooltips should be displayed
        /// for the <see paramref="attributes"/>.</param>
        /// <param name="selectedGroupAttribute">The <see cref="IAttribute"/> representing a
        /// currently selected row or group. <see langword="null"/> if no row or group is selected.
        /// </param>
        protected SelectionState(IDataEntryControl dataEntryControl, IEnumerable<IAttribute> attributes,
            bool displayToolTips, IAttribute selectedGroupAttribute)
        {
            _dataEntryControl = dataEntryControl;
            _attributes = new ReadOnlyCollection<IAttribute>(new List<IAttribute>(attributes));
            _displayToolTips = displayToolTips;
            _selectedGroupAttribute = selectedGroupAttribute;
        }

        /// <summary>
        /// Gets the <see cref="IDataEntryControl"/> the selection pertains to.
        /// </summary>
        /// <value>The <see cref="IDataEntryControl"/> the selection pertains to.</value>
        public IDataEntryControl DataControl
        {
            get
            {
                return _dataEntryControl;
            }
        }

        /// <summary>
        /// Gets the selected <see cref="IAttribute"/>s.
        /// </summary>
        /// <value>The selected <see cref="IAttribute"/>s.</value>
        public ReadOnlyCollection<IAttribute> Attributes
        {
            get
            {
                return _attributes;
            }
        }

        /// <summary>
        /// Gets whether tooltips should be displayed for the <see cref="Attributes"/>.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if tooltips should be displayed for the <see cref="Attributes"/>.
        /// </value>
        public bool DisplayToolTips
        {
            get
            {
                return _displayToolTips;
            }
        }

        /// <summary>
        /// Gets the <see cref="IAttribute"/> representing a currently selected row or group.
        /// <see langword="null"/> if no row or group is selected.
        /// </summary>
        /// <value>The <see cref="IAttribute"/> representing a currently selected row or group.
        /// <see langword="null"/> if no row or group is selected.</value>
        public IAttribute SelectedGroupAttribute
        {
            get
            {
                return _selectedGroupAttribute;
            }
        }

        /// <summary>
        /// Create a <see cref="ButtonControlSelectionState"/> from an <see cref="IDataEntryControl/>
        /// </summary>
        public static SelectionState Create(IDataEntrySingleAttributeControl control)
        {
            return new SelectionState(control);
        }

        /// <summary>
        /// Create a <see cref="TextControlSelectionState"/> from an <see cref="IDataEntryTextControl/>
        /// </summary>
        public static TextControlSelectionState Create(IDataEntryTextControl control)
        {
            return new TextControlSelectionState(control);
        }
    }

    /// <summary>
    /// Represents the current selection within the data entry framework for an editable text control. This includes both the
    /// selected <see cref="IAttribute"/>s as well as the indicated selection within the related
    /// <see cref="IDataEntryControl"/>.
    /// </summary>
    public class TextControlSelectionState : SelectionState
    {
        /// <summary>
        /// Gets the starting index of the <see cref="DataEntryTextBox"/> selection.
        /// </summary>
        /// <value>The starting index of the <see cref="DataEntryTextBox"/> selection.</value>
        public int SelectionStart { get; }

        /// <summary>
        /// Gets the length of the <see cref="DataEntryTextBox"/> selection.
        /// </summary>
        /// <value>The length of the <see cref="DataEntryTextBox"/> selection.</value>
        public int SelectionLength { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SelectionState"/> class.
        /// </summary>
        /// <param name="textControl">The <see cref="IDataEntryTextControl"/> the selection state applies to.</param>
        public TextControlSelectionState(IDataEntryTextControl dataEntryControl)
            : base(dataEntryControl, new[] { dataEntryControl.Attribute }, false, true, null)
        {
            try
            {
                SelectionStart = dataEntryControl.LastSelectionStart;
                SelectionLength = dataEntryControl.LastSelectionLength;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI50229", ex);
            }
        }
    }
}
