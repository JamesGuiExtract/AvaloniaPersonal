using Extract.Licensing;
using Extract.Utilities.Forms;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using UCLID_AFCORELib;
using UCLID_COMUTILSLib;
using UCLID_RASTERANDOCRMGMTLib;

namespace Extract.DataEntry
{
    /// <summary>
    /// A multiple-attribute control intended to manage two levels of <see cref="IAttribute"/>s 
    /// where each row in the table contains <see cref="IAttribute"/>s of the table's specified 
    /// <see cref="DataEntryTableBase.AttributeName"/> and each column represents a sub-attribute
    /// of its row's <see cref="IAttribute"/>.</summary>
    public partial class DataEntryTable : DataEntryTableBase
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(DataEntryTable).ToString();

        #endregion Constants

        #region Fields

        /// <summary>
        /// Indicates whether swiping should be allowed when an individual cell is selected.
        /// </summary>
        bool _cellSwipingEnabled = true;

        /// <summary>
        /// Indicates whether swiping should be allowed when a complete row is selected.
        /// </summary>
        bool _rowSwipingEnabled;

        /// <summary>
        /// Indicates whether data for a row can be automatically populated via row hints.
        /// </summary>
        bool _rowAutoPopulationEnabled = true;

        /// <summary>
        /// The filename of the rule file to be used to parse swiped data into rows.
        /// </summary>
        string _rowFormattingRuleFileName;

        /// <summary>
        /// Specifies the minimum number of rows a DataEntryTable must have.  If the specified
        /// number of attributes are not found, new, blank ones are created as necessary.
        /// </summary>
        int _minimumNumberOfRows;

        /// <summary>
        /// Specifies names of attributes that can be mapped into this control by renaming them.
        /// The purpose is to be able to copy data out of one control and paste it into another.
        /// </summary>
        readonly Collection<string> _compatibleAttributeNames = new Collection<string>();

        /// <summary>
        /// Specifies whether the table will have a context menu option to allow rows to be sorted
        /// spatially.
        /// </summary>
        bool _allowSpatialRowSorting;

        /// <summary>
        /// Context MenuItem that allows a row to be inserted at the current location.
        /// </summary>
        readonly ToolStripMenuItem _rowInsertMenuItem = new ToolStripMenuItem("Insert row");

        /// <summary>
        /// Context MenuItem that allows the selected row(s) to be deleted.
        /// </summary>
        readonly ToolStripMenuItem _rowDeleteMenuItem = new ToolStripMenuItem("Delete row(s)");

        /// <summary>
        /// Context MenuItem that allows the selected row(s) to be copied to the clipboard.
        /// </summary>
        readonly ToolStripMenuItem _rowCopyMenuItem = new ToolStripMenuItem("Copy row(s)");

        /// <summary>
        /// Context MenuItem that allows the selected row(s) to be copied to the clipboard and
        /// deleted from the table.
        /// </summary>
        readonly ToolStripMenuItem _rowCutMenuItem = new ToolStripMenuItem("Cut row(s)");

        /// <summary>
        /// Context MenuItem that allows the selected row(s) to be pasted into the current
        /// selection from the clipboard.
        /// </summary>
        readonly ToolStripMenuItem _rowPasteMenuItem = new ToolStripMenuItem("Paste copied row(s)");

        /// <summary>
        /// Context MenuItem that allows the selected row(s) to be inserted into from the clipboard.
        /// </summary>
        readonly ToolStripMenuItem _rowInsertCopiedMenuItem = new ToolStripMenuItem("Insert copied row(s)");

        /// <summary>
        /// Context MenuItem that sorts the all rows in the table spatially.
        /// </summary>
        readonly ToolStripMenuItem _sortAllRowsMenuItem = new ToolStripMenuItem("Sort rows to match document");

        /// <summary>
        /// The domain of attributes to which this control's attribute(s) belong.
        /// </summary>
        IUnknownVector _sourceAttributes;

        /// <summary>
        /// The attributes used to direct focus to the first cell of the "new" row of the table before
        /// focus leaves the control.  These attributes will not store any data useful as output.
        /// </summary>
        Dictionary<IUnknownVector, IAttribute> _tabOrderPlaceholderAttributes =
            new Dictionary<IUnknownVector, IAttribute>();

        /// <summary>
        /// A cache of DataEntryTableRows that have been populated so that when parent control
        /// propagates between different attributes, the propagated attribute does not need to be
        /// re-initialized as long as it had previously been initialized.
        /// </summary>
        readonly Dictionary<IUnknownVector, Dictionary<IAttribute, DataEntryTableRow>> _cachedRows =
            new Dictionary<IUnknownVector, Dictionary<IAttribute, DataEntryTableRow>>();

        /// <summary>
        /// The cached rows that pertain to the currently propagated attributes from the table's
        /// parent.
        /// </summary>
        Dictionary<IAttribute, DataEntryTableRow> _activeCachedRows;

        /// <summary>
        /// Any rows that are currently being dragged.
        /// </summary>
        List<DataEntryTableRow> _draggedRows;

        /// <summary>
        /// The point where a <see cref="Control.MouseDown"/> event took place. While the mouse
        /// remains down, this value will remain set and the base class mouse down event will not
        /// be raised until the mouse is released in order to maintain row selection for drag and
        /// drop operations.
        /// </summary>
        Point? _rowMouseDownPoint;

        /// <summary>
        /// A lazily instantiated <see cref="MiscUtils"/> instance to use for converting 
        /// IPersistStream implementations to/from a stringized byte stream.
        /// </summary>
        MiscUtils _miscUtils;

        /// <summary>
        /// Indicates which column pressing the active cell should be in after the enter key is
        /// pressed.
        /// </summary>
        int _carriageReturnColumn;

        /// <summary>
        /// Indicates whether selection is being reset manually while a mouse button is depressed.
        /// </summary>
        bool _selectionIsBeingReset;

        /// <summary>
        /// Indicates whether the control supports tabbing a row at a time.
        /// </summary>
        bool _allowTabbingByRow;

        /// <summary>
        /// Keeps track of the <see cref="DataGridViewRow"/> instances that have been initialized
        /// into the table.
        /// </summary>
        HashSet<DataGridViewRow> _initializedRows = new HashSet<DataGridViewRow>();

        /// <summary>
        /// Keeps track of the <see cref="DataGridViewRow"/> instances that have been modified
        /// since being initialized into the table.
        /// </summary>
        HashSet<DataGridViewRow> _modifiedRows = new HashSet<DataGridViewRow>();

        #endregion Fields

        #region Delegates

        /// <summary>
        /// Signature to use for invoking methods that accept one <see langword="string"/> parameter.
        /// </summary>
        /// <param name="value">An <see langword="string"/> parameter.</param>
        delegate void StringDelegate(string value);

        #endregion Delegates

        #region Constructors

        /// <summary>
        /// Initializes a new <see cref="DataEntryTable"/> instance.
        /// </summary>
        public DataEntryTable()
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(
                    LicenseIdName.DataEntryCoreComponents, "ELI24490", _OBJECT_NAME);

                // Enable smart hints on the rows so that smart hints will work for any column with
                // smart hints enabled (smart hints need both the row and column to have smart hints
                // enabled).
                ((DataEntryTableRow)RowTemplate).SmartHintsEnabled = true;

                InitializeComponent();
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI24223", ex);
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Indicates whether swiping should be allowed when an individual cell is selected.
        /// </summary>
        /// <value><see langword="true"/> if the table should allow swiping when an individual cell
        /// is selected, <see langword="false"/> if it should not.</value>
        /// <returns><see langword="true"/> if the table allows swiping when an individual cell
        /// is selected, <see langword="false"/> if it does not.</returns>
        [Category("Data Entry Table")]
        [DefaultValue(true)]
        public bool CellSwipingEnabled
        {
            get
            {
                return _cellSwipingEnabled;
            }

            set
            {
                _cellSwipingEnabled = value;
            }
        }

        /// <summary>
        /// Indicates whether swiping should be allowed when a complete row is selected.
        /// </summary>
        /// <value><see langword="true"/> if the table should allow swiping when a complete row
        /// is selected, <see langword="false"/> if it should not.</value>
        /// <returns><see langword="true"/> if the table allows swiping when a complete row
        /// is selected, <see langword="false"/> if it does not.</returns>
        [Category("Data Entry Table")]
        [DefaultValue(false)]
        public bool RowSwipingEnabled
        {
            get
            {
                return _rowSwipingEnabled;
            }

            set
            {
                _rowSwipingEnabled = value;
            }
        }

        /// <summary>
        /// Specifies the filename of an <see cref="IRuleSet"/> that should be used to reformat or
        /// split <see cref="SpatialString"/> content passed into <see cref="ProcessSwipedText"/>
        /// that is intended to populate the entire table.
        /// </summary>
        /// <value>The filename of the <see cref="IRuleSet"/> to be used.</value>
        /// <returns>The filename of the <see cref="IRuleSet"/> to be used.</returns>
        [Category("Data Entry Table")]
        [DefaultValue(null)]
        public string RowFormattingRuleFile
        {
            get
            {
                return _rowFormattingRuleFileName;
            }

            set
            {
                _rowFormattingRuleFileName = value;
            }
        }

        /// <summary>
        /// Specifies whether GetSpatialHint will attempt to generate a hint by indicating the 
        /// other <see cref="IAttribute"/>s sharing the same row.
        /// </summary>
        /// <value><see langword="true"/> if the table should attempt to generate row hints when
        /// possible; <see langword="false"/> if the table should never attempt to generate row
        /// hints.</value>
        /// <returns><see langword="true"/> if the table is configured to generate row hints when
        /// possible; <see langword="false"/> if the table is not configured to generate row
        /// hints.</returns>
        [Category("Data Entry Table")]
        [DefaultValue(true)]
        public new bool RowHintsEnabled
        {
            get
            {
                return base.RowHintsEnabled;
            }

            set
            {
                base.RowHintsEnabled = value;
            }
        }

        /// <summary>
        /// Specifies whether data for a row can be automatically populated via row hints.
        /// </summary>
        /// <value><see langword="true"/> if data for a row can be automatically populated;
        /// otherwise, <see langword="false"/>.</value>
        [Category("Data Entry Table")]
        [DefaultValue(true)]
        public bool RowAutoPopulationEnabled
        {
            get
            {
                return _rowAutoPopulationEnabled;
            }

            set
            {
                _rowAutoPopulationEnabled = value;
            }
        }

        /// <summary>
        /// Specifies whether GetSpatialHint will attempt to generate a hint by indicating the 
        /// other <see cref="IAttribute"/>s sharing the same column.
        /// </summary>
        /// <value><see langword="true"/> if the table should attempt to generate column hints when
        /// possible; <see langword="false"/> if the table should never attempt to generate column
        /// hints.</value>
        /// <returns><see langword="true"/> if the table is configured to generate column hints when
        /// possible; <see langword="false"/> if the table is not configured to generate column
        /// hints.</returns>
        [Category("Data Entry Table")]
        [DefaultValue(true)]
        public new bool ColumnHintsEnabled
        {
            get
            {
                return base.ColumnHintsEnabled;
            }

            set
            {
                base.ColumnHintsEnabled = value;
            }
        }

        /// <summary>
        /// Specifies the minimum number of rows the <see cref="DataEntryTable"/> must have. If the
        /// specified number of attributes are not found, new, blank ones are created as necessary.
        /// </summary>
        /// <value>The minimum number of rows the <see cref="DataEntryTable"/> must have.</value>
        /// <returns>The minimum number of rows the <see cref="DataEntryTable"/> must have.
        /// </returns>
        [Category("Data Entry Table")]
        [DefaultValue(0)]
        public int MinimumNumberOfRows
        {
            get
            {
                return _minimumNumberOfRows;
            }

            set
            {
                _minimumNumberOfRows = value;
            }
        }

        /// <summary>
        /// Specifies names of <see cref="IAttribute"/>s that can be mapped into this control by
        /// renaming them. The purpose is to be able to copy data out of controls mapped to the
        /// specified attribute names and paste it into this control.
        /// </summary>
        /// <value>The names of <see cref="IAttribute"/>s that may be mapped into this control via
        /// a paste operation.</value>
        [Category("Data Entry Table")] 
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public Collection<string> CompatibleAttributeNames
        {
            get
            {
                return _compatibleAttributeNames;
            }
        }

        /// <summary>
        /// Gets or sets whether the table will have a context menu option to allow rows to be
        /// sorted spatially.
        /// </summary>
        /// <value><see langword="true"/> if there should be a context menu option to allow rows to
        /// be sorted spatially.</value>
        [Category("Data Entry Table")]
        [DefaultValue(false)]
        public bool AllowSpatialRowSorting
        {
            get
            {
                return _allowSpatialRowSorting;
            }
            set
            {
                _allowSpatialRowSorting = value;
            }
        }

        /// <summary>
        /// Gets or sets whether the control supports tabbing by a group (row) of
        /// <see cref="IAttribute"/>s at a time.
        /// </summary>
        /// <value><see langword="true"/> if the control supports tabbing a group of
        /// <see cref="IAttribute"/>s at a time; <see langword="false"/> otherwise.</value>
        [Category("Data Entry Table")]
        [DefaultValue(false)]
        public bool AllowTabbingByRow
        {
            get
            {
                return _allowTabbingByRow;
            }

            set
            {
                _allowTabbingByRow = value;
            }
        }

        #endregion Properties

        #region Overrides

        /// <summary>
        /// Gets or sets the <see cref="IDataEntryControl"/> which is mapped to the parent of the 
        /// <see cref="IAttribute"/>(s) to which the current table is to be mapped.  The specified 
        /// <see cref="IDataEntryControl"/> must be contained in the same 
        /// <see cref="DataEntryControlHost"/> as this table.</summary>
        /// <value>The <see cref="IDataEntryControl"/> that is to act as the parent for this
        /// control's data or <see langword="null"/> if this control is to be mapped to a root-level 
        /// <see cref="IAttribute"/>.</value>
        /// <returns>The <see cref="IDataEntryControl"/> that acts as the parent for this control's
        /// data or <see langword="null"/> if this control is mapped to a root-level 
        /// <see cref="IAttribute"/>.</returns>
        /// <seealso cref="IDataEntryControl"/>
        [Category("Data Entry Control")]
        public override IDataEntryControl ParentDataEntryControl
        {
            get
            {
                return base.ParentDataEntryControl;
            }

            set
            {
                try
                {
                    if (value != base.ParentDataEntryControl)
                    {
                        // Unregister the last parent control from any drag and drop events.
                        if (base.AllowDrop && base.ParentDataEntryControl != null)
                        {
                            base.ParentDataEntryControl.QueryDraggedDataSupported -=
                                HandleQueryDraggedDataSupported;
                            ((Control)base.ParentDataEntryControl).DragDrop -= HandleParentDragDrop;
                        }

                        // Register the new parent control for any drag and drop events if AllowDrop is
                        // true.
                        if (base.AllowDrop && value != null)
                        {
                            value.QueryDraggedDataSupported += HandleQueryDraggedDataSupported;
                            ((Control)value).DragDrop += HandleParentDragDrop;
                        }
                    }

                    base.ParentDataEntryControl = value;
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI26784", ex);
                }
            }
        }

        /// <summary>
        /// Raises the <see cref="Control.HandleCreated"/> event in order to verify that the table
        /// has been properly configured and to create the context menu before data entry commences.
        /// </summary>
        /// <param name="e">A <see cref="EventArgs"/> that contains the event data.</param>
        protected override void OnHandleCreated(EventArgs e)
        {
            var autoSizeMode = ColumnHeadersHeightSizeMode;

            try
            {
                // https://extract.atlassian.net/browse/ISSUE-14222
                // The sequence of events when this form sometimes results in an
                // InvalidOperationException related to column sizing during layout. The call stack
                // at the time of the exception is outside of Extract code. However, since the
                // situation seems to be related to enforcing ColumnHeadersHeightSizeMode,
                // temporarily disable any column header resizing while changing visibility.
                ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;

                base.OnHandleCreated(e);

                // If we are not in design mode and the control is being made visible for the first
                // time, verify the table is properly configured and create the context menu.
                if (!InDesignMode)
                {
                    ExtractException.Assert("ELI24222",
                        "A DataEntryTable must have an AttributeName specified!",
                        !string.IsNullOrEmpty(AttributeName));

                    ExtractException.Assert("ELI24249",
                        "Row swiping is enabled, but no row formatting rule was specified!",
                        !RowSwipingEnabled || !string.IsNullOrWhiteSpace(RowFormattingRuleFile));

                    // Create a context menu for the table
                    base.ContextMenuStrip = new ContextMenuStrip();

                    // Insert row menu option
                    if (AllowUserToAddRows)
                    {
                        _rowInsertMenuItem.Enabled = false;
                        _rowInsertMenuItem.Click += HandleInsertMenuItemClick;
                        base.ContextMenuStrip.Items.Add(_rowInsertMenuItem);
                    }

                    // Delete row(s) menu option
                    if (AllowUserToDeleteRows)
                    {
                        _rowDeleteMenuItem.Enabled = false;
                        _rowDeleteMenuItem.Click += HandleDeleteMenuItemClick;
                        base.ContextMenuStrip.Items.Add(_rowDeleteMenuItem);
                    }

                    // Add a separator (if necessary)
                    if (base.ContextMenuStrip.Items.Count > 0)
                    {
                        base.ContextMenuStrip.Items.Add(new ToolStripSeparator());
                    }

                    // Copy row(s) menu option
                    _rowCopyMenuItem.Enabled = false;
                    _rowCopyMenuItem.Click += HandleRowCopyMenuItemClick;
                    base.ContextMenuStrip.Items.Add(_rowCopyMenuItem);

                    // Cut row(s) menu option
                    if (AllowUserToDeleteRows)
                    {
                        _rowCutMenuItem.Enabled = false;
                        _rowCutMenuItem.Click += HandleRowCutMenuItemClick;
                        base.ContextMenuStrip.Items.Add(_rowCutMenuItem);
                    }

                    // Add a separator
                    base.ContextMenuStrip.Items.Add(new ToolStripSeparator());

                    // Paste copied row(s) menu option
                    _rowPasteMenuItem.Enabled = false;
                    _rowPasteMenuItem.Click += HandleRowPasteMenuItemClick;
                    base.ContextMenuStrip.Items.Add(_rowPasteMenuItem);

                    // Insert copied row(s) menu option
                    if (AllowUserToAddRows)
                    {
                        _rowInsertCopiedMenuItem.Enabled = false;
                        _rowInsertCopiedMenuItem.Click += HandleRowInsertMenuItemClick;
                        base.ContextMenuStrip.Items.Add(_rowInsertCopiedMenuItem);
                    }

                    // Insert sort row option if specified
                    if (_allowSpatialRowSorting)
                    {
                        base.ContextMenuStrip.Items.Add(new ToolStripSeparator());
                        _sortAllRowsMenuItem.Enabled = false;
                        _sortAllRowsMenuItem.Click += HandleSortAllRowsMenuItemClick;
                        base.ContextMenuStrip.Items.Add(_sortAllRowsMenuItem);
                    }
                  
                    // Handle the opening of the context menu so that the available options and selection 
                    // state can be finalized.
                    base.ContextMenuStrip.Opening += HandleContextMenuOpening;

                    // Handle the case that rows collection has been changed so that
                    // _sourceAttributes can be updated as appropriate.
                    Rows.CollectionChanged += HandleRowsCollectionChanged;

                    // Disable column header wrap mode, otherwise resizing columns widths can cause
                    // wrapped text to increase the height of the header column thereby upsetting
                    // control sizing and possibly hiding data.
                    ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.False;

                    // Prevent drag operations from accidentally resizing rows.
                    AllowUserToResizeRows = false;
                }
            }
            catch (Exception ex)
            {
                ExtractException.AsExtractException("ELI24250", ex).Display();
            }
            finally
            {
                try
                {
                    ColumnHeadersHeightSizeMode = autoSizeMode;
                }
                catch (Exception ex)
                {
                    ex.ExtractLog("ELI41647");
                }
            }
        }

        /// <summary>
        /// Highlights the currently selected <see cref="IAttribute"/>s in the image viewer and
        /// propagates the selected row(s) as appropriate.
        /// </summary>
        protected override void ProcessSelectionChange()
        {
            base.ProcessSelectionChange();

            // If a selection change has resulted in a single cell being selected in a column less
            // than the current _carriageReturnColumn, the current column is now the
            // _carriageReturnColumn regardless of how selection arrived here.
            if (SelectedCells.Count == 1 && CurrentCell != null && 
                CurrentCell.ColumnIndex < _carriageReturnColumn)
            {
                _carriageReturnColumn = CurrentCell.ColumnIndex;
            }

            // Create a vector to store the attributes in the currently selected row(s).
            IUnknownVector selectedAttributes = new IUnknownVectorClass();

            // [DataEntry:645]
            // Process selection as a row selection only if there are no cells outside
            // the selected rows that are selected.
            bool rowSelectionMode = false;
            if (SelectedRows.Count > 0)
            {
                rowSelectionMode = true;

                foreach (DataGridViewCell cell in SelectedCells)
                {
                    if (!SelectedRows.Contains(Rows[cell.RowIndex]))
                    {
                        rowSelectionMode = false;
                        break;
                    }
                }
            }

            if (rowSelectionMode)
            {
                // [DataEntry:934]
                // If an entire row is selected, end edit mode to avoid confusion about what
                // happens when attempting to navigate while edit mode is still active.
                if (EditingControl != null)
                {
                    EndEdit();
                }

                // Indicates whether the only selected row is the new row.
                bool newRowSelection = false;

                // Loop through each selected row and add the attribute corresponding to each
                // to the set of selected attributes;
                foreach (DataGridViewRow selectedRow in SelectedRows)
                {
                    IAttribute attribute = GetAttribute(selectedRow);
                    if (attribute != null)
                    {
                        selectedAttributes.PushBack(attribute);
                    }
                    else if (selectedRow.Index == NewRowIndex && SelectedRows.Count == 1)
                    {
                        newRowSelection = true;
                    }
                }

                // Notify listeners new attribute(s) needs to be propagated
                OnPropagateAttributes(selectedAttributes);

                if (newRowSelection && TabOrderPlaceholderAttribute != null)
                {
                    // [DataEntry:346]
                    // If the new row is the only row selected, report 
                    // TabOrderPlaceholderAttribute as the selected attribute so that the
                    // control host will be able to correctly direct tab order forward and
                    // backward from this point.
                    selectedAttributes.PushBack(TabOrderPlaceholderAttribute);
                }

                // Allow all selected cells to be viewed with tooltips only if exactly one row
                // attribute is selected.
                // [DataEntry:491] Don't show tooltips during a drag & drop.
                bool rowView = selectedAttributes.Size() == 1 && !DragOverInProgress;

                // If the whole row is being viewed, mark the row's attributes as viewed.
                if (rowView && IsActive)
                {
                    foreach (DataGridViewCell cell in SelectedRows[0].Cells)
                    {
                        IDataEntryTableCell dataEntryCell = cell as IDataEntryTableCell;

                        if (dataEntryCell != null && dataEntryCell.Attribute != null)
                        {
                            AttributeStatusInfo.MarkAsViewed(dataEntryCell.Attribute, true);

                            UpdateCellStyle(dataEntryCell);
                        }
                    }
                }

                // Notify listeners that the spatial info to be associated with the table has
                // changed (include all subattributes to the row(s)'s attribute(s) in the 
                // spatial info).
                OnAttributesSelected(selectedAttributes, true, rowView,
                    (_allowTabbingByRow && !Disabled && selectedAttributes.Size() > 0) 
                        ? (IAttribute)selectedAttributes.At(0)
                        : null);
            }
            else if (SelectedCells.Count > 0)
            {
                // TabOrderPlaceholderAttribute may represent an attribute group if selected.
                IAttribute selectedGroupAttribute = null;

                // Create a collection to keep track of the attributes for each row in which a cell
                // is selected.
                IUnknownVector selectedRowAttributes = new IUnknownVectorClass();

                // If the selection is on a cell-by-cell basis rather, we need to compile the
                // attributes corresponding to each cell for the AttributesSelected event,
                // then find the attribute for the current row for the PropagateAttributes
                // event.
                foreach (IDataEntryTableCell selectedDataEntryCell in SelectedCells
                    .OfType<IDataEntryTableCell>())
                {
                    // Keep track of the cells' attributes for OnAttributesSelected
                    IAttribute attribute = GetAttribute(selectedDataEntryCell);
                    if (attribute != null)
                    {
                        selectedAttributes.PushBack(attribute);
                    }

                    DataGridViewCell selectedCell = (DataGridViewCell)selectedDataEntryCell;

                    // Keep track of the rows' attributes for OnPropagateAttributes
                    DataEntryTableRow row = Rows[selectedCell.RowIndex] as DataEntryTableRow;
                    if (row != null && row.Attribute != null)
                    {
                        selectedRowAttributes.PushBackIfNotContained(row.Attribute);
                    }
                }

                // Raises the PropagateAttributes event to notify dependent controls that new
                // attribute(s) need to be propagated.
                OnPropagateAttributes(selectedRowAttributes);

                // If a cell of the "new" row is selected, report 
                // TabOrderPlaceholderAttribute as the selected attribute so that the control host
                // will be able to correctly direct tab order forward and backward from this point.
                if (TabOrderPlaceholderAttribute != null &&
                    selectedRowAttributes.Size() == 0 &&
                    CurrentCell != null && CurrentCell.RowIndex == NewRowIndex)
                {
                    selectedAttributes.PushBack(TabOrderPlaceholderAttribute);

                    // If the only attribute selected is TabOrderPlaceholderAttribute, it should be
                    // considered a selected group. (The new row as a whole will not be selected
                    // when tabbing by row... only the first cell).
                    if (SelectedCells.Count == 1)
                    {
                        selectedGroupAttribute = TabOrderPlaceholderAttribute;
                    }
                }

                // Allow all selected cells to be viewed with tooltips only if exactly one row
                // attribute is selected.
                // [DataEntry:491] Don't show tooltips during a drag & drop.
                bool rowView = selectedRowAttributes.Size() == 1 && !DragOverInProgress;

                // If the whole row is being viewed, mark the row's attributes as viewed.
                if (rowView && IsActive)
                {
                    int selectedCount = selectedAttributes.Size();
                    for (int i = 0; i < selectedCount; i++)
                    {
                        AttributeStatusInfo.MarkAsViewed((IAttribute)selectedAttributes.At(i), true);
                    }

                    foreach (DataGridViewCell cell in SelectedCells)
                    {
                        UpdateCellStyle(cell);
                    }
                }

                // Include all the attributes for the specifically selected cells in the 
                // spatial info, not any children of those attributes.
                OnAttributesSelected(selectedAttributes, false, rowView, selectedGroupAttribute);
            }
            else
            {
                // Raise the PropagateAttributes event to notify dependent controls that there
                // is no active attribute (and to clear any existing mappings).
                OnPropagateAttributes(null);

                // Raise AttributesSelected to update the control's highlight.
                OnAttributesSelected(new IUnknownVectorClass(), false, false, null);
            }

            // Update the swiping state based on the current selection.
            OnSwipingStateChanged();
        }

        /// <summary>
        /// Raises the <see cref="DataGridView.UserAddedRow"/> event. Overridden in order to map new
        /// <see cref="IAttribute"/>s to a newly added row.
        /// </summary>
        /// <param name="e">A <see cref="DataGridViewRowEventArgs"/> that contains the event 
        /// data.</param>
        protected override void OnUserAddedRow(DataGridViewRowEventArgs e)
        {
            string initialValue = null;

            try
            {
                OnUpdateStarted(new EventArgs());

                base.OnUserAddedRow(e);

                // Re-enter edit mode so that any changes to the validation list based on triggers
                // are put into effect.
                if (EditingControl != null)
                {
                    EndEdit();
                }

                // Obtain the initial value before calling ApplyAttributeToRow which may trigger
                // auto-update queries and cause the value to change.
                initialValue = CurrentCell.Value.ToString();

                CurrentCell.Value = "";

                // Add a new attribute for the specified row.
                ApplyAttributeToRow(e.Row.Index - 1, null, null);
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI24244", ex);
                ee.AddDebugData("Event Data", e, false);
                ee.Display();
            }
            finally
            {
                // [DataEntry:1027]
                // Since the re-initialization of edit mode is being done via a begin invoke,
                // the call to OnUpdateEnded needs to be invoked as well so that it happens
                // after the re-initialization of edit mode.
                this.SafeBeginInvoke("ELI34408", () => OnUpdateEnded(new EventArgs()));
                this.SafeBeginInvoke("ELI34410", () =>
                    AttributeStatusInfo.UndoManager.ExtendCurrentOperation());

                // If the initial value was null or empty (can this happen?) there is no need to
                // call BeginEdit
                if (!string.IsNullOrEmpty(initialValue))
                {
                    // https://extract.atlassian.net/browse/ISSUE-12554
                    // With the addition of PauseQueries for ISSUE-12453, queries will now be
                    // scheduled for execution in the above OnUpdateEnded call. If BeginEdit is
                    // scheduled before those queries, the queries may override initialValue.
                    // Therefore, the scheduling of BeginEdit has been moved down here.
                    this.SafeBeginInvoke("ELI34449", () => BeginEdit(initialValue));
                }
            }
        }

        /// <summary>
        /// Raises the <see cref="DataGridView.RowsRemoved"/> event. Overridden in order to un-map 
        /// the <see cref="IAttribute"/>s that were mapped to the specified rows and to remove
        /// them from the overall <see cref="IAttribute"/> structure.
        /// </summary>
        /// <param name="e">A <see cref="DataGridViewRowsRemovedEventArgs"/> that contains the
        /// event data.</param>
        protected override void OnRowsRemoved(DataGridViewRowsRemovedEventArgs e)
        {
            try
            {
                base.OnRowsRemoved(e);

                // Unless the selection is updated, a single cell instead of an entire row will 
                // be selected following a delete.  It is more natural to leave an entire row
                // selected. But this needs to be done in a fashion so that the selected row matches
                // the "current" row.
                if (Rows.Count > 0)
                {
                    if ((e.RowIndex < Rows.Count && e.RowIndex > 0 && e.RowIndex == NewRowIndex) ||
                        (e.RowIndex == Rows.Count))
                    {
                        // If the last row is selected (except if there is only one row selected in a table
                        // that allows new rows), select the previous row from the row that was deleted.
                        ClearSelection(-1, e.RowIndex - 1, true);
                    }
                    else if (e.RowIndex < Rows.Count)
                    {
                        // If this is not the last row, select the same row index as was previous selected.
                        ClearSelection(-1, e.RowIndex, true);
                    }
                }
                else
                {
                    // An empty vector is a cue to clear mappings and reset the selection to the first
                    // cell.
                    ClearAttributeMappings(false);
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI24245", ex);
                ee.AddDebugData("Event Data", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Raises the <see cref="Control.KeyDown"/> event to check for keyboard shortcuts.
        /// </summary>
        /// <param name="e">A <see cref="KeyEventArgs"/> that contains the event data.</param>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            bool inUpdate = false;

            try
            {
                // Default handled flag to false;
                e.Handled = false;

                // Check for Ctrl + C, Ctrl + X, Ctrl + V or Ctrl + I
                if (e.Modifiers == Keys.Control &&
                    (e.KeyCode == Keys.C || e.KeyCode == Keys.X || e.KeyCode == Keys.V ||
                     e.KeyCode == Keys.I))
                {
                    // Check to see if row task options should be available based on the current
                    // table cell.
                    bool enableRowOptions = AllowRowTasks(
                        CurrentCell.RowIndex, CurrentCell.ColumnIndex);

                    switch (e.KeyCode)
                    {
                        // Copy selected rows
                        case Keys.C:

                            inUpdate = true;
                            OnUpdateStarted(new EventArgs());

                            if (enableRowOptions)
                            {
                                CopySelectedRows();

                                e.Handled = true;
                            }
                            else if (CopySelectedCells())
                            {
                                e.Handled = true;
                            }
                            break;

                        // Cut selected rows
                        case Keys.X:

                            inUpdate = true;
                            OnUpdateStarted(new EventArgs());

                            if (enableRowOptions && AllowUserToDeleteRows)
                            {
                                CopySelectedRows();

                                DeleteSelectedRows();

                                e.Handled = true;
                            }
                            else if (CopySelectedCells())
                            {
                                foreach (DataGridViewCell cell in SelectedCells)
                                {
                                    cell.Value = null;
                                }

                                e.Handled = true;
                            }
                            break;

                        // Paste selected rows
                        case Keys.V:

                            inUpdate = true;
                            OnUpdateStarted(new EventArgs());
                            IDataObject clipboardData = DataEntryMethods.GetClipboardData();

                            if (enableRowOptions && GetDataFormatName(clipboardData) != null)
                            {
                                PasteRowData(clipboardData);

                                e.Handled = true;
                            }
                            else if (PasteCellData(clipboardData))
                            {
                                e.Handled = true;
                            }
                            break;

                        // Insert new row
                        case Keys.I:
                            {
                                inUpdate = true;
                                OnUpdateStarted(new EventArgs());

                                if (AllowUserToAddRows)
                                {
                                    InsertNewRow(true);

                                    e.Handled = true;
                                }
                            }
                            break;
                    }
                }

                base.OnKeyDown(e);
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI24773", ex);
                ee.AddDebugData("Event Data", e, false);
                ee.Display();
            }
            finally
            {
                if (inUpdate)
                {
                    OnUpdateEnded(new EventArgs());
                }
            }
        }

        /// <summary>
        /// Previews a keyboard message. 
        /// </summary>
        /// <param name="m">A <see cref="Message"/>, passed by reference, that represents the window
        /// message to process.</param>
        /// <returns><see langword="true"/> if the message was processed; otherwise, 
        /// <see langword="false"/>.</returns>
        protected override bool ProcessKeyPreview(ref Message m)
        {
            try
            {
                if (EditingControl != null && m.Msg == WindowsMessage.KeyDown && 
                    m.WParam == (IntPtr)Keys.V && (ModifierKeys & Keys.Control) != 0)
                {
                    try
                    {
                        OnUpdateStarted(new EventArgs());

                        if (PasteCellData(DataEntryMethods.GetClipboardData()))
                        {
                            return true;
                        }
                    }
                    finally
                    {
                        OnUpdateEnded(new EventArgs());
                    }
                }
                
                return base.ProcessKeyPreview(ref m);
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI28788", ex);
                ee.AddDebugData("Message", m, false);
                ee.Display();
            }

            return base.ProcessKeyPreview(ref m);
        }

        /// <summary>
        /// Raises or delays the <see cref="Control.MouseDown"/> event depending on the current
        /// selection state of the table. If one or more rows are currently selected, the
        /// <see cref="Control.MouseDown"/> event may be postponed until the mouse button is
        /// released.
        /// </summary>
        /// <param name="e">The <see cref="MouseEventArgs"/> associated with the event.</param>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            try
            {
                _rowMouseDownPoint = null;
                HitTestInfo hit = HitTest(e.X, e.Y);

                // If row operations are supported for the current selection and shift or control
                // keys are not pressed, suppress the MouseDown event until the mouse button is
                // released to maintain the current selection for any potential drag and drop
                // operation unless the shift or control modifiers are being used.
                if ((ModifierKeys & Keys.Shift) == 0 &&
                    (ModifierKeys & Keys.Control) == 0 &&
                    AllowRowTasks(hit.RowIndex, hit.ColumnIndex))
                {
                    _rowMouseDownPoint = e.Location;
                }
                else
                {
                    base.OnMouseDown(e);
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI26695", ex);
                ee.AddDebugData("Event Data", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Raises the <see cref="Control.MouseUp"/> event. If a <see cref="Control.MouseDown"/>
        /// event has been postponed, it will be raised first.
        /// </summary>
        /// <param name="e">The <see cref="MouseEventArgs"/> associated with the event(s).</param>
        protected override void OnMouseUp(MouseEventArgs e)
        {
            try
            {
                // Once a mouse button is released, we no longer need special processing for
                // manually reset selection.
                if (_selectionIsBeingReset)
                {
                    _selectionIsBeingReset = false;
                }

                if (_rowMouseDownPoint != null)
                {
                    _rowMouseDownPoint = null;

                    base.OnMouseDown(e);
                }

                base.OnMouseUp(e);
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI26696", ex);
                ee.AddDebugData("Event Data", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Raises the <see cref="Control.MouseLeave"/> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs"/> associated with the event(s).</param>
        protected override void OnMouseLeave(EventArgs e)
        {
            try
            {
                // [DataEntry:707]
                // If the mouse leaves the control while _selectionIsBeingReset is true, we no
                // longer want the scrolling to be locked.
                if (_selectionIsBeingReset)
                {
                    _selectionIsBeingReset = false;
                }

                base.OnMouseLeave(e);
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI29013", ex);
                ee.AddDebugData("Event Data", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Raises the <see cref="Control.MouseMove"/> event.
        /// </summary>
        /// <param name="e">The <see cref="MouseEventArgs"/> associated with the event.</param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            try
            {
                // If a drag event is not currently in progress, the left mouse button is down and
                // the mouse is in a different location than when the mouse button was pressed and
                // row tasks are allowed given the current selection, begin a drag and drop
                // operation.
                if (!DragOverInProgress && _rowMouseDownPoint != null &&
                    e.Location != _rowMouseDownPoint.Value &&
                    (MouseButtons & MouseButtons.Left) != 0)
                {
                    HitTestInfo hit = HitTest(e.X, e.Y);

                    if (AllowRowTasks(hit.RowIndex, hit.ColumnIndex))
                    {
                        DoRowDragDrop();
                    }
                }

                base.OnMouseMove(e);
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI26667", ex);
                ee.AddDebugData("Event Data", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Raises the <see cref="Control.DragOver"/> event.
        /// </summary>
        /// <param name="drgevent">The <see cref="DragEventArgs"/> associated with the event.
        /// </param>
        protected override void OnDragOver(DragEventArgs drgevent)
        {
            try
            {
                // Initialize the currently supported drag/drop action to none.
                drgevent.Effect = DragDropEffects.None;

                // Determine if a drop is supported for the data being dragged.
                string dataType = GetDataFormatName(drgevent.Data);

                // If the dragged data is compatible with the rows of this table.
                if (dataType != null)
                {
                    Point location = PointToClient(new Point(drgevent.X, drgevent.Y));
                    HitTestInfo hit = HitTest(location.X, location.Y);

                    // If row tasks are allowed given the current selection.
                    if (AllowRowTasks(hit.RowIndex, hit.ColumnIndex))
                    {
                        // Give focus to the table if a drop is supported.
                        if (!base.Focused)
                        {
                            Focus();
                        }

                        // Started with all allowed operations from the drag source.
                        drgevent.Effect = drgevent.AllowedEffect;

                        // If dragging into the same control and table
                        if (RowsBeingDraggedFromCurrentTable)
                        {
                            // Don't allow rows to be dragged onto themselves.
                            if (_draggedRows.Contains((DataEntryTableRow)CurrentRow))
                            {
                                drgevent.Effect &= DragDropEffects.None;
                            }
                            // Otherwise, allow the data to be moved to the drag position.
                            else
                            {
                                drgevent.Effect &= DragDropEffects.Move;
                            }
                        }
                        // If dragging into the same control, but a different table the data should
                        // be moved to that table.
                        else if (_draggedRows != null)
                        {
                            drgevent.Effect &= DragDropEffects.Move;
                        }
                    }
                }
                // If the dragged data is not compatible with this table, but is compatible with a
                // dependent control.
                else
                {
                    Point location = PointToClient(new Point(drgevent.X, drgevent.Y));
                    HitTestInfo hit = HitTest(location.X, location.Y);

                    // If row tasks are allowed given the current selection.
                    if (AllowRowTasks(hit.RowIndex, hit.ColumnIndex))
                    {
                        OnQueryDraggedDataSupported(drgevent);
                    }
                }

                // Ensure no drop operations have been specified that aren't allowed.
                drgevent.Effect &= drgevent.AllowedEffect;

                // If both copy and move are supported, use move instead of copy.
                if (drgevent.Effect == (DragDropEffects.Copy | DragDropEffects.Move))
                {
                    drgevent.Effect = DragDropEffects.Move;
                }

                // [DataEntry:626]
                // If dragging more than one row, ensure all dragged rows are selected whenever
                // the cursor is over any of the dragged rows so that dragging behavior doesn't
                // change after dragging out of the dragged rows and then back into them.
                if (_draggedRows != null && _draggedRows.Count > 1 &&
                    _draggedRows.Contains(CurrentRow as DataEntryTableRow))
                {
                    bool changedSelection = false;

                    // Don't update the selection until all the dragged rows have been selected.
                    using (new SelectionProcessingSuppressor(this))
                    {
                        foreach (DataEntryTableRow row in _draggedRows)
                        {
                            if (!row.Selected)
                            {
                                row.Selected = true;
                                changedSelection = true;
                            }
                        }
                    }

                    // As long as selection occurred, update the selection now.
                    if (changedSelection)
                    {
                        base.OnSelectionChanged(new EventArgs());
                    }
                }

                base.OnDragOver(drgevent);
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI26668", ex);
                ee.AddDebugData("Event Data", drgevent, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Raises the <see cref="Control.DragDrop"/> event.
        /// </summary>
        /// <param name="drgevent">The <see cref="DragEventArgs"/> associated with the event.
        /// </param>
        protected override void OnDragDrop(DragEventArgs drgevent)
        {
            try
            {
                OnUpdateStarted(new EventArgs());

                // If this table supports the data type being dragged
                if (GetDataFormatName(drgevent.Data) != null)
                {
                    // When dropping data into a compatible table control that allows rows to be
                    // added, insert it before the selected row, not over top of it.
                    if (AllowUserToAddRows)
                    {
                        InsertNewRow(true);
                    }

                    // Paste the dropped data in.
                    PasteRowData(drgevent.Data, true);

                    Focus();
                }
                // If a dependent control has indicated it supports the dragged data and the new
                // row is currently selected.
                else if (drgevent.Effect != DragDropEffects.None && 
                    CurrentCell.RowIndex == NewRowIndex)
                {
                    // Create a new entry for the data that will be propagated to dependent controls.
                    InsertNewRow(true);

                    Focus();
                }

                base.OnDragDrop(drgevent);
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI26967", ex);
                ee.AddDebugData("Event Data", drgevent, false);
                ee.Display();
            }
            finally
            {
                OnUpdateEnded(new EventArgs());
            }
        }

        /// <summary>
        /// Raises the <see cref="ScrollableControl.Scroll"/> event.
        /// </summary>
        /// <param name="e">The <see cref="ScrollEventArgs"/> associated with the event.</param>
        protected override void OnScroll(ScrollEventArgs e)
        {
            try
            {
                // If a drag and drop operation is in progress or a manual selection reset is in
                // progress, prevent the table from being scrolled as the results are usually
                // undesirable.
                if (_selectionIsBeingReset || DragOverInProgress)
                {
                    e.NewValue = e.OldValue;
                }

                base.OnScroll(e);
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI26694", ex);
                ee.AddDebugData("Event Data", e, false);
                ee.Display();
            }

            base.OnScroll(e);
        }

        /// <summary>
        /// Raises the <see cref="E:DataGridView.CellValueChanged"/> event.
        /// </summary>
        /// <param name="e">A <see cref="T:DataGridViewCellEventArgs"/> that contains the event
        /// data.</param>
        /// <exception cref="T:ArgumentOutOfRangeException">The value of the
        /// <see cref="P:DataGridViewCellEventArgs.ColumnIndex"/> property of <paramref name="e"/>
        /// is greater than the number of columns in the control minus one.
        /// -or-
        /// The value of the <see cref="P:DataGridViewCellEventArgs.RowIndex"/> property of
        /// <paramref name="e"/> is greater than the number of rows in the control minus one.
        /// </exception>
        protected override void OnCellValueChanged(DataGridViewCellEventArgs e)
        {
            try
            {
                base.OnCellValueChanged(e);

                if (e.RowIndex >= 0 && e.RowIndex < Rows.Count)
                {
                    var row = Rows[e.RowIndex];

                    // Consider a row modified if a cell value changes for any row that has already
                    // been initialized into the table.
                    if (_initializedRows.Contains(row))
                    {
                        _modifiedRows.Add(row);
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI39199");
            }
        }

        /// <summary>
        /// Releases all unmanaged resources used by the <see cref="DataEntryTable"/>.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged 
        /// resources; <see langword="false"/> to release only unmanaged resources.</param>        
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose of managed objects
                if (components != null)
                {
                    components.Dispose();
                }

                ClearCachedData();
            }

            // Dispose of unmanaged resources

            // Dispose of base class
            base.Dispose(disposing);
        }

        /// <summary>
        /// Processes keys used for navigating in the <see cref="DataEntryTable"/>.
        /// <para><b>Note</b></para>
        /// This is called when the <see cref="DataGridView"/> is not in edit mode.
        /// </summary>
        /// <param name="e">Contains information about the key that was pressed.</param>
        /// <returns><see langword="true"/> if the key was processed; otherwise,
        /// <see langword="false"/>.</returns>
        //[SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers", MessageId = "0#")]
        protected override bool ProcessDataGridViewKey(KeyEventArgs e)
        {
            bool keyProcessed = false;

            try
            {
                // [DataEntry:486]
                // Handle the enter key manually in order to mimic Excel's behavior.
                if (e.KeyCode == Keys.Enter && ProcessEnterKey())
                {
                    return true;
                }

                // [DataEntry:745]
                // Don't allow left/right arrow keys to exit a cell in edit mode.
                if (EditingControl != null && (e.KeyCode == Keys.Left || e.KeyCode == Keys.Right))
                {
                    return true;
                }

                // If the delete key was pressed while not in edit mode and where it won't result in
                // any rows being deleted, instead delete the contents of all selected cells.
                if (e.KeyCode == Keys.Delete)
                {
                    if (SelectedRows.Count > 0 && AllowUserToDeleteRows)
                    {
                        // [DataEntry:899]
                        // Due to code to delete the contents of selected cells, a delete key won't
                        // make it all the way through to the underlying DataGridView class-- we
                        // need to explicitly delete selected rows.
                        DeleteSelectedRows();
                    }   
                    else
                    {
                        // [DataEntry:641]
                        // Clear the contents as well as the spatial info of all selected cells.
                        DeleteSelectedCellContents();
                    }

                    return true;
                }

                keyProcessed = base.ProcessDataGridViewKey(e);

                if (keyProcessed)
                {
                    // If DataGridViewKey was processed for any key other than Enter or Delete,
                    // reset the _carriageReturnColumn to the current column as the current
                    // selection is likely not the result of tabbing.
                    _carriageReturnColumn = CurrentCell == null ? 0 : CurrentCell.ColumnIndex;
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI27274", ex);
                ee.AddDebugData("Key data", e, false);
                ee.Display();
            }

            return keyProcessed;
        }

        /// <summary>
        /// Processes a dialog key.
        /// <para><b>Note</b></para>
        /// This is called when the <see cref="DataGridView"/> is in edit mode.
        /// </summary>
        /// <param name="keyData">One of the <see cref="Keys"/> values that represents the key to
        /// process.</param>
        /// <returns><see langword="true"/> if the key was processed by the control; otherwise,
        /// <see langword="false"/>.</returns>
        protected override bool ProcessDialogKey(Keys keyData)
        {
            bool keyProcessed = false;

            try
            {
                // [DataEntry:486]
                // Handle the enter key manually in order to mimic Excel's behavior.
                if (keyData == Keys.Enter && ProcessEnterKey())
                {
                    return true;
                }

                if (IsCurrentCellInEditMode && keyData == (Keys.Shift | Keys.Space))
                {
                    // [DataEntry:958, 959]
                    // Unhandled, Shift+Space will trigger full row selection which will, in turn,
                    // trigger an EndEdit call in ProcessSelectionChange. This sequence seems to
                    // cause a couple different problems that both seem related to the editing
                    // control handling the space before the grid does. I cannot find a solution
                    // that involves the grid handling the row selection "naturally". Forcing the
                    // selection at this point, however, works.
                    ClearSelection(-1, CurrentCell.RowIndex, true);
                    return true;
                }
                
                keyProcessed = base.ProcessDialogKey(keyData);

                // If any other dialog key was processed, reset the _carriageReturnColumn to the
                // current column as the current selection is likely not the result of tabbing.
                if (keyProcessed)
                {
                    _carriageReturnColumn = CurrentCell == null ? 0 : CurrentCell.ColumnIndex;
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI27275", ex);
                ee.AddDebugData("Key data", keyData.ToString(), false);
                ee.Display();
            }

            return keyProcessed;
        }

        /// <summary>
        /// Raises the <see cref="Control.Click"/> event.
        /// </summary>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        protected override void OnClick(EventArgs e)
        {
            base.OnClick(e);

            // In the event of a click, the selected column is now the _carriageReturnColumn.
            _carriageReturnColumn = CurrentCell == null ? 0 : CurrentCell.ColumnIndex;
        }

        /// <summary>
        /// Deletes the attributes that aren't needed anymore (via AttributeStatusInfo), and ensures
        /// the deleted attribute is no longer referenced by any table member.
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> to be deleted.</param>
        protected override void DeleteAttributeData(IAttribute attribute)
        {
            foreach (Dictionary<IAttribute, DataEntryTableRow> cachedSet in
                _cachedRows.Values)
            {
                if (cachedSet.ContainsKey(attribute))
                {
                    cachedSet.Remove(attribute);
                }
            }

            base.DeleteAttributeData(attribute);
        }

        #endregion Overrides

        #region Event Handlers

        /// <summary>
        /// Handles the <see cref="DataGridViewRowCollection.CollectionChanged"/> event in order to
        /// remove from _sourceAttributes any attributes that have been deleted from the table.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">An <see cref="CollectionChangeEventArgs"/> that contains the event data.
        /// </param>
        void HandleRowsCollectionChanged(object sender, CollectionChangeEventArgs e)
        {
            try
            {
                if (e.Action == CollectionChangeAction.Remove)
                {
                    // Remove the row's attribute from _sourceAttributes and raise the 
                    // AttributesDeleted event.
                    IAttribute attributeToRemove = GetAttribute(e.Element);
                    if (attributeToRemove != null)
                    {
                        ExtractException.Assert("ELI26142", "Uninitialized data!",
                            _sourceAttributes != null);

                        DeleteAttributeData(attributeToRemove);
                    }

                    var row = (DataGridViewRow)e.Element;
                    _initializedRows.Remove(row);
                    _modifiedRows.Remove(row);
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI24313", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        #endregion Event Handlers

        #region IDataEntryControl Properties

        /// <summary>
        /// Indicates whether the <see cref="DataEntryTable"/> will currently accept as input the
        /// <see cref="SpatialString"/> associated with an image swipe. 
        /// <para><b>Note:</b></para>
        /// The <see cref="CellSwipingEnabled"/> and <see cref="RowSwipingEnabled"/> properties 
        /// should be used to configure swiping on the <see cref="DataEntryTable"/> in place of the 
        /// <see cref="SupportsSwiping"/> and is why this property is not browsable. If this 
        /// property is set to <see langword="true"/>, both of the above properties will be set to 
        /// <see langword="true"/>; likewise setting <see cref="SupportsSwiping"/> to 
        /// <see langword="false"/> will set both properties to <see langword="false"/>.
        /// </summary>
        /// <value><see langword="true"/> if the table will not currently accept input via a
        /// swipe, <see langword="false"/> otherwise.</value>
        /// <returns><see langword="true"/> if the table will not currently accept input via a
        /// swipe, <see langword="false"/> otherwise.</returns>
        /// <seealso cref="IDataEntryControl"/>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override bool SupportsSwiping
        {
            get
            {
                // If the control isn't enabled or no attributes have been mapped, swiping is not
                // currently supported.
                if (!Enabled || _sourceAttributes == null)
                {
                    return false;
                }
                else if (SelectedRows.Count > 0)
                {
                    // Row selection
                    return _rowSwipingEnabled;
                }
                else if (SelectedCells.Count == 1 && CurrentCell != null)
                {
                    // Cell selection
                    if (_cellSwipingEnabled)
                    {
                        var column = Columns[CurrentCell.ColumnIndex] as DataEntryTableColumn;
                        if (column != null && column.SupportsSwiping)
                        {
                            return true;
                        }
                    }
                }

                return false;
            }

            set
            {
                _rowSwipingEnabled = value;
                _cellSwipingEnabled = value;
            }
        }

        #endregion IDataEntryControl Properties

        #region IDataEntryControl Methods

        /// <summary>
        /// Specifies the domain of <see cref="IAttribute"/>s from which the 
        /// <see cref="DataEntryTable"/> should find the <see cref="IAttribute"/>(s) 
        /// to which it should be mapped (based on the 
        /// <see cref="DataEntryTableBase.AttributeName"/> property). 
        /// </summary>
        /// <param name="sourceAttributes">The <see cref="IUnknownVector"/> instance of 
        /// <see cref="IAttribute"/>s from which the <see cref="DataEntryTable"/> 
        /// should find its corresponding <see cref="IAttribute"/>(s). Can be an empty vector, but 
        /// must not be <see langword="null"/>.</param>
        /// <seealso cref="IDataEntryControl"/>
        public override void SetAttributes(IUnknownVector sourceAttributes)
        {
            try
            {
                // [DataEntry:298]
                // If the table isn't assigned any data, disable it since any data entered would
                // not be mapped into the attribute hierarchy.
                // Also, prevent it from being enabled if explicitly disabled via the
                // IDataEntryControl interface.
                Enabled = sourceAttributes != null && !Disabled;

                _sourceAttributes = sourceAttributes;

                _initializedRows.Clear();
                _modifiedRows.Clear();

                // Retrieve the cached rows that correspond to sourceAttributes (if available).
                if (sourceAttributes == null)
                {
                    _activeCachedRows = null;
                }
                else
                {
                    // If necessary, create a row cache for sourceAttributes.
                    if (!_cachedRows.TryGetValue(sourceAttributes, out _activeCachedRows))
                    {
                        _activeCachedRows = new Dictionary<IAttribute, DataEntryTableRow>();
                        _cachedRows[sourceAttributes] = _activeCachedRows;
                    }
                }

                // Clear all existing entries from the table.
                Rows.Clear();

                if (_sourceAttributes == null)
                {
                    // If no data is being assigned, clear the existing attribute mappings and do not
                    // attempt to map a new attribute.
                    ClearAttributeMappings(false);
                }
                else
                {
                    // Attempt to find mapped attribute(s) from the provided vector.  
                    IUnknownVector mappedAttributes = DataEntryMethods.InitializeAttributes(
                        AttributeName, MultipleMatchSelectionMode.All, _sourceAttributes, null,
                        this, null, false, null, null, null, null);

                    using (new RefreshSuppressor(this))
                    {
                        // Create arrays to store the attributes associated with each row & cell.
                        int count = mappedAttributes.Size();

                        using (new SelectionProcessingSuppressor(this))
                        {
                            // Pre-populate the appropriate number of rows (otherwise the "new" row may not
                            // be visible)
                            if (count > 0)
                            {
                                Rows.Add(count);
                            }
                        }

                        // Loop through all attributes to populate the members of the row and cell attribute
                        // arrays and to add the values of each into the table.
                        for (int i = 0; i < count; i++)
                        {
                            ApplyAttributeToRow(i, (IAttribute)mappedAttributes.At(i), null);
                        }

                        // [DataEntry:393]
                        // If the minimum number of rows are not present, add new blank rows as necessary.
                        int hasNewRow = (NewRowIndex == -1 ? 0 : 1);
                        while (Rows.Count - hasNewRow < _minimumNumberOfRows)
                        {
                            int addedRowIndex = Rows.Add();

                            // Apply the new attribute to the added row.
                            ApplyAttributeToRow(addedRowIndex, null, null);
                        }
                    }

                    // [DataEntry:263]
                    // If the TabOrderPlaceholderAttribute has not yet been added, do it now.
                    if (!_tabOrderPlaceholderAttributes.ContainsKey(sourceAttributes))
                    {
                        IAttribute attribute = TabOrderPlaceholderAttribute;
                        if (attribute != null)
                        {
                            DataEntryMethods.ReorderAttributes(_sourceAttributes,
                                    DataEntryMethods.AttributeAsVector(attribute));
                        }
                    }
                }

                // Any refreshes suppressed above can now occur.
                ExecutePendingRefresh();

                // Since the spatial information for this cell has likely changed, spatial hints need 
                // to be updated.
                UpdateHints(false);

                // Highlights the specified attributes in the image viewer and propagates the 
                // current selection to dependent controls (if appropriate)
                ProcessSelectionChange();

                Invalidate();
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI24227", ex);
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
        public override bool ProcessSwipedText(SpatialString swipedText)
        {
            try
            {
                // Swiping not supported if control isn't enabled or data isn't loaded.
                if (!Enabled || _sourceAttributes == null)
                {
                    return false;
                }

                if (SelectedRows.Count > 0)
                {
                    return ProcessRowSwipe(swipedText);
                }
                else if (SelectedCells.Count > 0)
                {
                    return ProcessCellSwipe(swipedText);
                }

                return false;
            }
            catch (Exception ex)
            {
                try
                {
                    // If an exception was thrown while processing a swipe, refresh hints for the
                    // table since the hints may not be valid at this point.
                    UpdateHints(true);
                }
                catch (Exception ex2)
                {
                    ExtractException.Log("ELI27098", ex2);
                }

                throw ExtractException.AsExtractException("ELI24240", ex);
            }
        }

        /// <summary>
        /// Commands the specified <see cref="IAttribute"/> to be propagated onto dependent 
        /// <see cref="IDataEntryControl"/>s.
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> whose sub-attributes should be
        /// propagated to any child controls. If <see langword="null"/>, the currently selected
        /// <see cref="IAttribute"/> should be repropagated if there is a single attribute.
        /// <para><b>Requirements</b></para>If non-<see langword="null"/>, the specified 
        /// <see cref="IAttribute"/> must be known to be mapped to the 
        /// <see cref="IDataEntryControl"/>.</param>
        /// <param name="selectAttribute">If <see langword="true"/>, the specified 
        /// <see cref="IAttribute"/> will also be selected within the table.  If 
        /// <see langword="false"/>, the previous selection will remain even if a different
        /// <see cref="IAttribute"/> was propagated.</param>
        /// <param name="selectTabGroup">If <see langword="true"/> all <see cref="IAttribute"/>s in
        /// the specified <see cref="IAttribute"/>'s tab group are to be selected,
        /// <see langword="false"/> otherwise.</param>
        public override void PropagateAttribute(IAttribute attribute, bool selectAttribute,
            bool selectTabGroup)
        {
            try
            {
                if (attribute == null)
                {
                    base.PropagateAttribute(attribute, selectAttribute, selectAttribute);
                    return;
                }

                // Needed if selecting a row (selectTabGroup)
                DataEntryTableRow selectedRow;

                // Special handling is needed for the case where the TabOrderPlaceholderAttribute
                // is to be propagated.
                if (attribute == TabOrderPlaceholderAttribute)
                {
                    // If selectAttribute is specified, activate the first visible cell in the   new row.
                    if (selectAttribute)
                    {
                        ClearSelection();
                        CurrentCell = GetFirstVisibleCellInRow(Rows[NewRowIndex]);
                        CurrentCell.Selected = true;

                        OnAttributesSelected(
                            DataEntryMethods.AttributeAsVector(TabOrderPlaceholderAttribute),
                            false, false, TabOrderPlaceholderAttribute);
                    }
                    // Otherwise, propagate null since TabOrderPlaceholderAttribute will
                    // never have children.
                    else
                    {
                        OnPropagateAttributes(null);
                    }
                }
                // If selecting an entire row, ensure the specified attribute is a row attribute,
                // then select the row.
                else if (selectTabGroup &&
                    _activeCachedRows.TryGetValue(attribute, out selectedRow))
                {
                    ClearSelection(-1, selectedRow.Index, true);
                    CurrentCell = GetFirstVisibleCellInRow(Rows[selectedRow.Index]);
                }
                // If TabOrderPlaceholderAttribute is not the attribute to propagate, allow the
                // base class to handle the propagation.
                else
                {
                    base.PropagateAttribute(attribute, selectAttribute, selectTabGroup);
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI25445", ex);
            }
        }

        /// <summary>
        /// Any data that was cached should be cleared;  This is called when a document is unloaded.
        /// If controls fail to clear COM objects, errors may result if that data is accessed when
        /// a subsequent document is loaded.
        /// </summary>
        public override void ClearCachedData()
        {
            try
            {
                // Ensure all rows are removed from the table before disposing of the cached rows.
                Rows.Clear();

                // Dispose of all cached rows.
                foreach (Dictionary<IAttribute, DataEntryTableRow> cachedSet in _cachedRows.Values)
                {
                    foreach (DataEntryTableRow row in cachedSet.Values)
                    {
                        row.Dispose();
                    }
                }
                _cachedRows.Clear();

                _activeCachedRows = null;

                _tabOrderPlaceholderAttributes.Clear();

                // https://extract.atlassian.net/browse/ISSUE-16596
                // Trying to access the clipboard in a background context can cause a exceptions which
                // spam the log file.
                if (DataEntryControlHost?.DataEntryApplication?.RunningInBackground == false)
                {
                    try
                    {
                        // [DataEntry:378]
                        // Prevent copying and pasting table data between different documents.
                        string rowDataType = GetDataFormatName(Clipboard.GetDataObject());
                        if (!string.IsNullOrEmpty(rowDataType) && rowDataType != "System.String")
                        {
                            DataEntryMethods.ClearClipboardData();
                        }
                    }
                    catch (Exception ex)
                    {
                        // https://extract.atlassian.net/browse/ISSUE-14294
                        // Clipboard operations being finicky has long been an issue. Don't allow a
                        // failure checking clipboard data to cause a larger issue.
                        ex.ExtractLog("ELI41660");
                    }
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI26631", ex);
            }
        }

        /// <summary>
        /// Requests that the <see cref="IDataEntryControl"/> refresh all <see cref="IAttribute"/>
        /// values to the screen.
        /// </summary>
        public override void RefreshAttributes()
        {
            try
            {
                SetAttributes(_sourceAttributes);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI31010", ex);
            }
        }

        /// <summary>
        /// Creates a <see cref="BackgroundFieldModel"/> for representing this control during
        /// a background data load.
        /// </summary>
        public override BackgroundFieldModel GetBackgroundFieldModel()
        {
            try
            {
                var displayOrder = DataEntryMethods.GetTabIndices(this);
                var fieldModel = new BackgroundFieldModel()
                {
                    Name = AttributeName,
                    ParentAttributeControl = ParentDataEntryControl,
                    DisplayOrder = displayOrder,
                    AutoCreate = (MinimumNumberOfRows > 0)
                };

                var identityColumn = Columns
                    .OfType<DataEntryTableColumn>()
                    .SingleOrDefault(c => c.AttributeName == ".");
                if (identityColumn != null)
                {
                    fieldModel.AutoUpdateQuery = identityColumn.AutoUpdateQuery;
                    fieldModel.ValidationQuery = identityColumn.ValidationQuery;
                    fieldModel.IsViewable = Visible && identityColumn.Visible;
                    fieldModel.PersistAttribute = identityColumn.PersistAttribute;
                    fieldModel.ValidationErrorMessage = identityColumn.ValidationErrorMessage;
                    fieldModel.ValidationPattern = identityColumn.ValidationPattern;
                    fieldModel.ValidationCorrectsCase = identityColumn.ValidationCorrectsCase;
                }

                foreach (var dataEntryColumn in Columns.OfType<DataEntryTableColumn>())
                {
                    if (dataEntryColumn.AttributeName != ".")
                    {
                        var childFieldModel = new BackgroundFieldModel()
                        {
                            Name = dataEntryColumn.AttributeName,
                            AutoUpdateQuery = dataEntryColumn.AutoUpdateQuery,
                            ValidationQuery = dataEntryColumn.ValidationQuery,
                            DisplayOrder = displayOrder.Concat(new[] { dataEntryColumn.Index + 1 }),
                            IsViewable = Visible && dataEntryColumn.Visible,
                            PersistAttribute = dataEntryColumn.PersistAttribute,
                            ValidationErrorMessage = dataEntryColumn.ValidationErrorMessage,
                            ValidationPattern = dataEntryColumn.ValidationPattern,
                            ValidationCorrectsCase = dataEntryColumn.ValidationCorrectsCase
                        };

                        fieldModel.Children.Add(childFieldModel);
                    }
                }

                return fieldModel;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45506");
            }
        }

        #endregion IDataEntryControl Methods

        #region Event Handlers

        /// <summary>
        /// When data is being dragged over a parent data control, handles the
        /// <see cref="IDataEntryControl.QueryDraggedDataSupported"/> event to indicate to the
        /// parent control whether this table would be able to use the data if it were dropped.
        /// </summary>
        /// <param name="sender">The control that sent the event.</param>
        /// <param name="e">A <see cref="QueryDraggedDataSupportedEventArgs"/> that contains the
        /// event data.</param>
        /// <seealso cref="IDataEntryControl"/>
        void HandleQueryDraggedDataSupported(object sender, QueryDraggedDataSupportedEventArgs e)
        {
            try
            {
                // If the dragged data matches this tables row data type and either new rows
                // can be added to the table or there is only one possible row the data could go.
                // (Don't use any compatible type since that data should be handled by either
                // the parent or the control associated with the data type).
                if (GetDataFormatName(e.DragDropEventArgs.Data) == RowDataFormatName &&
                    (AllowUserToAddRows || Rows.Count == 0))
                {
                    // As long as this table is from the same table from which the data was
                    // dragged, support the data being moved into active table.
                    if (_draggedRows == null || _activeCachedRows == null ||
                        !_activeCachedRows.ContainsKey(_draggedRows[0].Attribute))
                    {
                        e.DragDropEventArgs.Effect |= DragDropEffects.Move;
                    }
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI26665", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the case that data was dropped into a parent data entry control.
        /// </summary>
        /// <param name="sender">The control that sent the event.</param>
        /// <param name="e">A <see cref="DragEventArgs"/> that contains the event data.</param>
        void HandleParentDragDrop(object sender, DragEventArgs e)
        {
            try
            {
                // If the dropped data is the data type used by this table's rows.
                // (Don't use any compatible type since that data should be handled by either
                // the parent or the control associated with the data type).
                if (GetDataFormatName(e.Data) == RowDataFormatName)
                {
                    if (AllowUserToAddRows)
                    {
                        // Highlight the table's new row.
                        ClearSelection(-1, NewRowIndex, true);
                        CurrentCell = GetFirstVisibleCellInRow(Rows[NewRowIndex]);

                        // Paste the dropped data in.
                        PasteRowData(e.Data);

                        Focus();
                    }
                    // If rows are not allowed to be added, but there is only a single attribute
                    // it seems pretty clear that this attribute should be replaced.
                    else if (Rows.Count == 1)
                    {
                        // Highlight the table's one and only row.
                        ClearSelection(-1, 0, true);
                        CurrentCell = GetFirstVisibleCellInRow(Rows[0]);

                        // Paste the dropped data in.
                        PasteRowData(e.Data);

                        Focus();
                    }
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI26671", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the case the user requested to insert a new row.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        /// <seealso cref="IDataEntryControl"/>
        void HandleInsertMenuItemClick(object sender, EventArgs e)
        {
            try
            {
                OnUpdateStarted(new EventArgs());

                InsertNewRow(true);
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI24317", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
            finally
            {
                OnUpdateEnded(new EventArgs());
            }
        }

        /// <summary>
        /// Handles the case the user requested to delete the selected row(s).
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        void HandleDeleteMenuItemClick(object sender, EventArgs e)
        {
            try
            {
                DeleteSelectedRows();
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI24318", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the case the user requested to copy the selected row(s).
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        void HandleRowCopyMenuItemClick(object sender, EventArgs e)
        {
            try
            {
                CopySelectedRows();                
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI24761", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the case the user requested to cut the selected row(s).
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        void HandleRowCutMenuItemClick(object sender, EventArgs e)
        {
            try
            {
                CopySelectedRows();

                DeleteSelectedRows();
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI24770", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the case the user requested to paste the row(s) currently in the clipboard
        /// into the currently selected rows.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        void HandleRowPasteMenuItemClick(object sender, EventArgs e)
        {
            try
            {
                PasteRowData(DataEntryMethods.GetClipboardData());
            }
            catch (Exception ex)
            {
                var ee = new ExtractException("ELI24762", "Error pasting clipboard data.", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the case the user requested to insert the row(s) in the clipboard.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        void HandleRowInsertMenuItemClick(object sender, EventArgs e)
        {
            try
            {
                OnUpdateStarted(new EventArgs());

                InsertNewRow(true);

                PasteRowData(DataEntryMethods.GetClipboardData());
            }
            catch (Exception ex)
            {
                ExtractException ee = new ExtractException("ELI24763",
                    "Unable to insert copied rows!", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
            finally
            {
                OnUpdateEnded(new EventArgs());
            }
        }

        /// <summary>
        /// Handles the case the user requested to insert the row(s) in the clipboard.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        void HandleSortAllRowsMenuItemClick(object sender, EventArgs e)
        {
            try
            {
                // Keep track of the original selection so it can be restored after sorting.
                // [DataEntry:776]
                // Don't include the selection new row in the selection since clearing
                // the table will result in a different new row being added.
                List<DataGridViewCell> selectedCells = new List<DataGridViewCell>();
                foreach (DataGridViewCell cell in SelectedCells)
                {
                    if (!Rows[cell.RowIndex].IsNewRow)
                    {
                        selectedCells.Add(cell);
                    }
                }
                List<DataGridViewRow> selectedRows = new List<DataGridViewRow>();
                foreach (DataGridViewRow row in SelectedRows)
                {
                    if (!row.IsNewRow)
                    {
                        selectedRows.Add(row);
                    }
                }

                // Compile the list of row attributes.
                List<IAttribute> rowAttributes = new List<IAttribute>();
                foreach (DataEntryTableRow row in Rows)
                {
                    if (row.IsNewRow)
                    {
                        break;
                    }

                    rowAttributes.Add(row.Attribute);
                }

                // Sort the row attributes spatially.
                rowAttributes =
                    DataEntryControlHost.SortAttributesSpatially(rowAttributes);

                using (new RefreshSuppressor(this))
                using (new SelectionProcessingSuppressor(this))
                {
                    // Remove all rows so they can be re-added from the cache.
                    // Note: Calling clear does not trigger attribute removal to be processed, they
                    // are still in _sourceAttributes and still in the row cache.
                    Rows.Clear();

                    Rows.Add(rowAttributes.Count);

                    // Add back each row at the appropriate position. By allowing the row cache to
                    // remain intact, this will have a minimal performance hit as opposed to
                    // re-adding the attributes from scratch. Work from bottom to top so
                    // insertBeforeAttribute can be used to ensure attributes are being added to
                    // _sourceAttributes in the correct order.
                    IAttribute insertBeforeAttribute = null;
                    for (int i = rowAttributes.Count - 1; i >= 0; i--)
                    {
                        IAttribute rowAttribute = rowAttributes[i];

                        ApplyAttributeToRow(i, rowAttribute, insertBeforeAttribute);
                        insertBeforeAttribute = rowAttribute;
                    }

                    // Reset the original selection.
                    ClearSelection();
                    foreach (DataGridViewCell cell in selectedCells)
                    {
                        cell.Selected = true;
                    }
                    foreach (DataGridViewRow row in selectedRows)
                    {
                        row.Selected = true;
                    }
                }

                // Any refreshes suppressed above can now occur.
                ExecutePendingRefresh();
            }
            catch (Exception ex)
            {
                ExtractException ee = new ExtractException("ELI28768", "Unable to sort rows!", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the context menu opening event so that the available options and selection
        /// state can be finalized.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">An <see cref="CancelEventArgs"/> that contains the event data.</param>
        void HandleContextMenuOpening(object sender, CancelEventArgs e)
        {
            try
            {
                // [DataEntry:587]
                // Ensure this table is active before performing any sort of operation that can be
                // started via a context menu to prevent confusion with the current selection.
                if (!IsActive)
                {
                    Focus();
                }

                // Determine the location where the context menu was opened.
                Point mouseLocation = PointToClient(MousePosition);
                HitTestInfo hit = HitTest(mouseLocation.X, mouseLocation.Y);

                // Check to see if row and/or row task options should be available based on
                // the current table cell.
                bool enableRowOptions = AllowRowTasks(hit.RowIndex, hit.ColumnIndex);
                bool enablePasteOptions = false;

                // If getting the data from the clipboard throws an exception don't enable 
                // the paste button
		        // Only interested in enabling or disabling the paste menu and don't want an exception displayed
		        // so added the try catch block to remove the display of the exception when displaying the
		        // context menu
		        // https://extract.atlassian.net/browse/ISSUE-13155
                try
                {
                    if (enableRowOptions &&
                        GetDataFormatName(DataEntryMethods.GetClipboardData()) != null)
                    {
                        enablePasteOptions = true;
                    }
                }
                catch (Exception)
                {
                    enablePasteOptions = false;
                }

                // Enable/disable the context menu options as appropriate.
                _rowInsertMenuItem.Enabled = enableRowOptions;
                _rowDeleteMenuItem.Enabled = enableRowOptions;
                _rowCopyMenuItem.Enabled = enableRowOptions;
                _rowCutMenuItem.Enabled = enableRowOptions;
                _rowPasteMenuItem.Enabled = enablePasteOptions;
                _rowInsertCopiedMenuItem.Enabled = enablePasteOptions;

                if (_allowSpatialRowSorting)
                {
                    _sortAllRowsMenuItem.Enabled = enableRowOptions;
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI24319", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the case that a TabOrderPlaceholderAttribute has been deleted.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">An <see cref="AttributeDeletedEventArgs"/> that contains the event data.
        /// </param>
        void HandlePlaceholderAttributeDeleted(object sender, AttributeDeletedEventArgs e)
        {
            // Ensure that wherever a TabOrderPlaceholderAttribute is deleted from, it is no longer
            // referenced.
            foreach (KeyValuePair<IUnknownVector, IAttribute> pair in _tabOrderPlaceholderAttributes)
            {
                if (e.DeletedAttribute == pair.Value)
                {
                    _tabOrderPlaceholderAttributes.Remove(pair.Key);
                    break;
                }
            }
        }

        #endregion Event Handlers

        #region Private Members

        /// <summary>
        /// Gets the name of a <see cref="DataFormats.Format"/> value for placing items on the
        /// clipboard or into a <see cref="DataObject"/> class.
        /// </summary>
        /// <returns>
        /// The name of a <see cref="DataFormats.Format"/> value for placing items on the clipboard
        /// or into a <see cref="DataObject"/> class.
        /// </returns>
        string RowDataFormatName
        {
            get
            {
                DataFormats.Format format = DataFormats.GetFormat(_OBJECT_NAME + AttributeName);

                return format.Name;
            }
        }

        /// <summary>
        /// Gets the name of a  <see langword="DataFormats.Format"/> value to identify multiple
        /// columns of data on the clipboard that was copied by this control.
        /// </summary>
        /// <returns>The name of a <see langword="DataFormats.Format"/> value to identify multiple
        /// columns of data on the clipboard that was copied from this control.</returns>
        string MultiColumnDataFormatName
        {
            get
            {
                DataFormats.Format format = DataFormats.GetFormat(
                    _OBJECT_NAME + base.AttributeName + "<MultiColumnSelection>");

                return format.Name;
            }
        }
        
        /// <summary>
        /// Indicates the type of data on the clipboard as long as it is usable by this 
        /// <see cref="DataEntryTable"/>.
        /// </summary>
        /// <param name="dataObject">The <see cref="IDataObject"/> instance whose data type is to
        /// be checked for compatibility.</param>
        /// <returns>A <see langword="string"/> indicating the type of data on the clipboard
        /// as long as it is usable by this <see cref="DataEntryTable"/>. If the data is not usable
        /// by this table, <see langword="null"/> is returned.</returns>
        string GetDataFormatName(IDataObject dataObject)
        {
            if (dataObject != null)
            {
                // Check for data from this table.
                if (dataObject.GetDataPresent(RowDataFormatName))
                {
                    return RowDataFormatName;
                }
                // Check for data from compatible tables.
                else
                {
                    foreach (string compatibleAttribute in _compatibleAttributeNames)
                    {
                        if (dataObject.GetDataPresent(_OBJECT_NAME + compatibleAttribute))
                        {
                            return _OBJECT_NAME + compatibleAttribute;
                        }
                    }
                }

                // Check for string data that can be processed with the row formatting rule.
                if (!string.IsNullOrWhiteSpace(RowFormattingRuleFile)
                    && dataObject.GetDataPresent("System.String"))
                {
                    return "System.String";
                }
            }

            return null;
        }

        /// <summary>
        /// Performs a drag and drop operation using the currently selected cells as the data
        /// source for the operation.
        /// </summary>
        void DoRowDragDrop()
        {
            try
            {
                OnUpdateStarted(new EventArgs());

                IDataObject rowDataObject;
                string rowData = GetSelectedRowData();

                // Row data is available from the current selection.
                if (!string.IsNullOrEmpty(rowData))
                {
                    // Create a dataObject containing the rows' data.
                    rowDataObject = new DataObject(RowDataFormatName, rowData);
                   
                    // Maintain an internal list of the rows being dragged.
                    _draggedRows = new List<DataEntryTableRow>();
                    foreach (DataGridViewRow row in SelectedRows)
                    {
                        if (row.Index != NewRowIndex)
                        {
                            _draggedRows.Add((DataEntryTableRow)row);

                            // Update the style of each row being dragged.
                            foreach (var cell in row.Cells.OfType<IDataEntryTableCell>())
                            {
                                cell.IsBeingDragged = true;
                                UpdateCellStyle(cell);
                            }
                        }
                    }
                }
                else
                {
                    // TODO: Add support for drag/drop of cells in the future.
                    return;
                }

                DragDropEffects allowedEffects = AllowUserToDeleteRows ?
                    (DragDropEffects.Move | DragDropEffects.Copy) : DragDropEffects.Copy;

                // Begin the drag-and-drop operation.
                DragDropEffects operationPerformed = DoDragDrop(rowDataObject, allowedEffects);

                // After the drag-and-drop has ended (whether or not the data was dropped), remove
                // the special style applied to the dragged rows.
                foreach (DataEntryTableRow row in _draggedRows)
                {
                    foreach (var cell in row.Cells.OfType<IDataEntryTableCell>())
                    {
                        cell.IsBeingDragged = false;
                        UpdateCellStyle(cell);
                    }
                }

                // If the rows were moved to a different location, they need to be removed from this
                // table.
                if ((operationPerformed & DragDropEffects.Move) != 0)
                {
                    RemoveDraggedRows();
                }
                else if (operationPerformed == 0)
                {
                    // Restore row selection to the dragged rows if no drag operation ended up being
                    // performed.
                    if (_activeCachedRows != null &&
                        _activeCachedRows.ContainsKey(_draggedRows[0].Attribute))
                    {
                        using (new SelectionProcessingSuppressor(this))
                        {
                            DataGridViewRow lastSelectedRow = null;

                            ClearSelection();
                            foreach (DataEntryTableRow row in _draggedRows)
                            {
                                lastSelectedRow = row;
                            }
                            
                            // Select the currentCell before selecting the rows otherwise the row
                            // selection(s) may be undone.
                            if (lastSelectedRow != null)
                            {
                                CurrentCell = GetFirstVisibleCellInRow(lastSelectedRow);
                            }

                            foreach (DataEntryTableRow row in _draggedRows)
                            {
                                row.Selected = true;
                            }
                        }

                        Focus();
                    }
                }

                // [DataEntry:490-492]
                // Update attribute selection to redisplay the tooltip and to make the DEP aware of
                // the new selection.
                ProcessSelectionChange();
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI26674", ex);
            }
            finally
            {
                _draggedRows = null;
                OnUpdateEnded(new EventArgs());
            }
        }

        /// <summary>
        /// Removes the rows that were dragged from the table.
        /// </summary>
        void RemoveDraggedRows()
        {
            // Find the cached set of rows the dragged data came from.
            foreach (Dictionary<IAttribute, DataEntryTableRow> cachedSet in
                _cachedRows.Values)
            {
                if (cachedSet.ContainsKey(_draggedRows[0].Attribute))
                {
                    // Preserve the row selection that existed prior to removing the rows.
                    DataGridViewRow[] selectedRows = new DataGridViewRow[SelectedRows.Count];
                    SelectedRows.CopyTo(selectedRows, 0);
                    List<DataGridViewRow> initialSelectedRows =
                        new List<DataGridViewRow>(selectedRows);

                    // Remove each row
                    foreach (DataEntryTableRow draggedRow in _draggedRows)
                    {
                        // If the dragged rows are in the table currently displayed, simply
                        // removing the row will trigger all necessary actions.
                        if (cachedSet == _activeCachedRows)
                        {
                            Rows.Remove(draggedRow);
                        }
                        // If the rows are from a table not currently displayed,
                        // DeleteAttribute needs to be called and the cached row set needs
                        // to be updated.
                        else
                        {
                            DataEntryTableRow row = cachedSet[draggedRow.Attribute];
                            row.Dispose();

                            DeleteAttributeData(draggedRow.Attribute);
                        }
                    }

                    // Restore the initial row selection as long as the selection does not
                    // include the source drag rows.
                    if (cachedSet == _activeCachedRows && initialSelectedRows.Count > 0)
                    {
                        using (new SelectionProcessingSuppressor(this))
                        {
                            DataGridViewRow lastSelectedRow = null;

                            ClearSelection();
                            foreach (DataGridViewRow row in initialSelectedRows)
                            {
                                lastSelectedRow = row;
                            }

                            // Select the currentCell before selecting the rows otherwise the row
                            // selection(s) may be undone.
                            if (lastSelectedRow != null &&
                                !_draggedRows.Contains(lastSelectedRow as DataEntryTableRow))
                            {
                                CurrentCell = GetFirstVisibleCellInRow(lastSelectedRow);
                            }

                            foreach (DataGridViewRow row in initialSelectedRows)
                            {
                                if (!_draggedRows.Contains(row as DataEntryTableRow))
                                {
                                    row.Selected = true;
                                }
                            }
                        }
                    }

                    break;
                }
            }
        }

        /// <summary>
        /// Gets a lazily instantiate <see cref="MiscUtils"/> instance to use for converting 
        /// IPersistStream implementations to/from a stringized byte stream.
        /// </summary>
        /// <returns>A <see cref="MiscUtils"/> instance.</returns>
        MiscUtils MiscUtils
        {
            get
            {
                if (_miscUtils == null)
                {
                    _miscUtils = new MiscUtils();
                }

                return _miscUtils;
            }
        }

        /// <summary>
        /// Compares two rows by their index position
        /// </summary>
        /// <param name="row1">The first <see cref="DataGridViewRow"/> to compare</param>
        /// <param name="row2">The second <see cref="DataGridViewRow"/> to compare</param>
        /// <returns>
        /// Less than 0 if row1's index is less than row2's
        /// 0 if row1's index is the same as row2's
        /// Greater than 0 if row1's index is greater than row2's
        /// </returns>
        static int CompareRowsByIndex(DataGridViewRow row1, DataGridViewRow row2)
        {
            return (row1.Index - row2.Index);
        }

        /// <summary>
        /// Uses the supplied attribute for the value of the specified row. If appropriate, a new
        /// row is added.
        /// </summary>
        /// <param name="rowIndex">The row where the attribute is to be applied.</param>
        /// <param name="attribute">The attribute to store in the specified row. If 
        /// <see langword="null"/>, a new attribute is created and used.</param>
        /// <param name="insertBeforeAttribute">Specifies the position in the _sourceAttributes
        /// vector to place the attribute. If <see langword="null"/>, the attribute will be placed
        /// kept at the location of the attribute if it is replacing or at the end of the vector
        /// if it is a new attribute. This value is ignored (assumed <see langword="null"/>) if the 
        /// provided attribute is being applied into the "new" row.</param>
        void ApplyAttributeToRow(int rowIndex, IAttribute attribute,
            IAttribute insertBeforeAttribute)
        {
            ExtractException.Assert("ELI26143", "Uninitialized data!", _sourceAttributes != null);

            bool newAttributeCreated = false;

            // If no attribute was supplied, create a new one.
            if (attribute == null)
            {
                attribute = AttributeStatusInfo.Initialize(AttributeName, _sourceAttributes, this);
                newAttributeCreated = true;
            }

            // Keep track of any attribute we are replacing.
            IAttribute attributeToReplace = GetAttribute(Rows[rowIndex]);

            using (new RefreshSuppressor(this))
            using (new SelectionProcessingSuppressor(this))
            {
                // Add a new row to the table if necessary
                if (rowIndex == Rows.Count || Rows[rowIndex].IsNewRow)
                {
                    Rows.Add();
                }
            }

            // If a row has already been cached for the attribute, simply swap out the current row
            // for the cached row and re-map all attributes in the row.
            DataEntryTableRow cachedRow;
            if (_activeCachedRows.TryGetValue(attribute, out cachedRow))
            {
                using (new RefreshSuppressor(this))
                using (new SelectionProcessingSuppressor(this))
                {
                    Rows.RemoveAt(rowIndex);
                    Rows.Insert(rowIndex, cachedRow);

                    // Start by mapping the attribute to the row itself.
                    MapAttribute(attribute, Rows[rowIndex]);

                    // If insertBeforeAttribute as specified, ensure the attribute is added at the
                    // the specified position.
                    if (insertBeforeAttribute != null)
                    {
                        DataEntryMethods.InsertOrReplaceAttribute(_sourceAttributes, attribute,
                            null, insertBeforeAttribute);
                    }

                    // Remap each cell's attribute in the row (may cause the parent row's attribute to
                    // be remapped).
                    foreach (DataGridViewCell cell in cachedRow.Cells)
                    {
                        IDataEntryTableCell dataEntryCell = cell as IDataEntryTableCell;
                        if (dataEntryCell != null)
                        {
                            MapAttribute(dataEntryCell.Attribute, dataEntryCell);

                            // Set the value in case the attribute has been modified since it was
                            // last displayed. This will trigger the value to be re-validated as
                            // well in case validation status changed while the cell was not
                            // displayed.
                            cell.Value = dataEntryCell.Attribute;
                        }

                        // The cell style may need to be updated in case the enabled status of the
                        // table has changed since the cell was last displayed.
                        UpdateCellStyle(cell);
                    }
                }
            }
            // If a cached row didn't exist for the incoming attribute, initialize it.
            else
            {
                // Map the attribute to the row itself.
                ((DataEntryTableRow)Rows[rowIndex]).Attribute = attribute;
                MapAttribute(attribute, Rows[rowIndex]);

                bool parentAttributeIsMapped = false;

                // Keep track of all attributes in the row so they can be assigned to a row tab
                // group if necessary.
                List<IAttribute> rowAttributes = new List<IAttribute>();

                // Loop through each column to populate the values.
                foreach (DataGridViewColumn column in Columns)
                {
                    DataEntryTableColumn dataEntryTableColumn = column as DataEntryTableColumn;

                    // If the column is a data entry column, we need to populate it as appropriate.
                    if (dataEntryTableColumn != null)
                    {
                        IDataEntryTableCell dataEntryCell = (IDataEntryTableCell)
                            Rows[rowIndex].Cells[column.Index];

                        var displayOrder = DataEntryMethods.GetTabIndices(this).Concat(new[] { column.Index + 1 });

                        IAttribute subAttribute;
                        if (dataEntryTableColumn.AttributeName == ".")
                        {
                            // "." indicates that the value of the row's attribute should be used in
                            // this column.
                            subAttribute = attribute;
                            parentAttributeIsMapped = true;

                            // It is important to call initialize here to ensure AttributeStatusInfo
                            // raises the AttributeInitialized event for all attributes mapped into
                            // the table.
                            AttributeStatusInfo.Initialize(attribute, _sourceAttributes, this,
                                displayOrder, false, dataEntryTableColumn.TabStopMode,
                                dataEntryCell.ValidatorTemplate, dataEntryTableColumn.AutoUpdateQuery,
                                dataEntryTableColumn.ValidationQuery);
                        }
                        else
                        {
                            // Select the appropriate subattribute to use and create an new (empty)
                            // attribute if no such attribute can be found.
                            subAttribute = DataEntryMethods.InitializeAttribute(
                                dataEntryTableColumn.AttributeName,
                                dataEntryTableColumn.MultipleMatchSelectionMode,
                                true, attribute.SubAttributes, null, this, displayOrder, true,
                                dataEntryTableColumn.TabStopMode, dataEntryCell.ValidatorTemplate,
                                dataEntryTableColumn.AutoUpdateQuery,
                                dataEntryTableColumn.ValidationQuery);

                            if (_allowTabbingByRow && !Disabled)
                            {
                                AttributeStatusInfo.SetAttributeTabGroup(subAttribute,
                                    new List<IAttribute>());
                            }
                        }

                        rowAttributes.Add(subAttribute);

                        // If not persisting the attribute, mark the attribute accordingly.
                        if (!dataEntryTableColumn.PersistAttribute)
                        {
                            AttributeStatusInfo.SetAttributeAsPersistable(subAttribute, false);
                        }

                        // If the attribute being applied is a hint, it has been copied from elsewhere
                        // and the hint shouldn't apply here-- remove the hint.
                        if (AttributeStatusInfo.GetHintType(subAttribute) != HintType.None)
                        {
                            AttributeStatusInfo.SetHintType(subAttribute, HintType.None);
                            AttributeStatusInfo.SetHintRasterZones(subAttribute, null);
                        }

                        // Map the column's attribute and set the attribute's validator. NOTE: This
                        // may be remapping the row's attribute; this is intentional so that 
                        // PropagateAttribute will select the specified cell for the row's attribute.
                        MapAttribute(subAttribute, dataEntryCell);

                        // Apply the subAttribute as the cell's value.
                        Rows[rowIndex].Cells[column.Index].Value = subAttribute;
                    }
                }

                // If the value of the row's attribute itself is not displayed in one of the columns,
                // consider the row's attribute as valid and viewed.
                if (!parentAttributeIsMapped)
                {
                    // It is important to call initialize here to ensure AttributeStatusInfo
                    // raises the AttributeInitialized event for all attributes mapped into the
                    // table.
                    AttributeStatusInfo.Initialize(attribute, _sourceAttributes, this, null, false,
                        TabStopMode.Never, null, null, null);
                }

                // Swap out the existing attribute in the overall attribute hierarchy (either
                // keeping attribute ordering the same as it was or explicitly inserting it at the
                // specified position).
                if (DataEntryMethods.InsertOrReplaceAttribute(_sourceAttributes, attribute,
                    attributeToReplace, insertBeforeAttribute))
                {
                    DeleteAttributeData(attributeToReplace);
                }

                if (HasDependentControls)
                {
                    // [DataEntry:679]
                    // Propagate the attribute right away; otherwise they will not be marked
                    // as viewable and will not have highlights created for them.
                    OnPropagateAttributes(DataEntryMethods.AttributeAsVector(attribute));
                }
                else
                {
                    // If this control does not have any dependent controls, consider each row
                    // propagated.
                    AttributeStatusInfo.MarkAsPropagated(attribute, true, true);
                }

                // If TabOrderPlaceholderAttribute is being used, make sure it remains the last
                // attribute from this control in _sourceAttributes.
                if (TabOrderPlaceholderAttribute != null)
                {
                    DataEntryMethods.ReorderAttributes(_sourceAttributes,
                        DataEntryMethods.AttributeAsVector(TabOrderPlaceholderAttribute));
                }

                // If a new attribute was created for this row, ensure that it propagates all its
                // sub-attributes to keep all status info's in the attribute hierarchy up-to-date.
                if (newAttributeCreated)
                {
                    ProcessSelectionChange();
                }

                // Assign the row attributes to a tab group if _allowTabbingByRow is true.
                if (_allowTabbingByRow && !Disabled)
                {
                    AttributeStatusInfo.SetAttributeTabGroup(attribute, rowAttributes);
                }

                // Cache the initialized attribute's row for later use.
                _activeCachedRows[attribute] = (DataEntryTableRow)Rows[rowIndex];
            }

            // Any refreshes suppressed above can now occur.
            ExecutePendingRefresh();

            _initializedRows.Add(Rows[rowIndex]);
        }

        /// <summary>
        /// Process swiped text as input into the currently selected row(s).
        /// <list type="bullet">
        /// <bullet>If no attributes are found, no action will be taken.</bullet>
        /// <bullet>If the number of attributes found in the swiped text equals the number of rows
        /// selected, the selected rows will be replaced with the swiped attributes.</bullet>
        /// <bullet>If the number of attributes found is greater than the number of rows selected,
        /// if <see cref="DataGridView.AllowUserToAddRows"/> is <see langword="true"/>, the selected 
        /// rows will be replaced with the swiped attributes, then new rows will be inserted 
        /// immediately following the last selected row as needed.  If 
        /// <see cref="DataGridView.AllowUserToAddRows"/> is <see langword="false"/>, at least one
        /// attribute found in the swipe will not be added to the table.</bullet>
        /// <bullet>If the number of attributes found is less than the number of rows selected then 
        /// the found attributes will replace the selected rows until there are no more found 
        /// attributes. If <see cref="DataGridView.AllowUserToDeleteRows"/> is 
        /// <see langword="true"/>, the remaining selected rows will then be deleted (otherwise,
        /// they will remain.</bullet>
        /// </list>
        /// </summary>
        /// <param name="swipedText">The OCR'd text from the image swipe.</param>
        bool ProcessRowSwipe(SpatialString swipedText)
        {
            // Row selection mode. The swipe can only be applied via the results of
            // a row formatting rule.
            IUnknownVector formattedData = DataEntryMethods.RunFormattingRule(RowFormattingRuleFile,
                swipedText, AttributeName);

            // Find all attributes which apply to this control.
            IUnknownVector formattedAttributes =
                DataEntryMethods.AFUtility.QueryAttributes(formattedData, AttributeName, false);

            // If no attributes were returned from the rule, return false to indicate formatting
            // was not successful.
            if (formattedAttributes == null || formattedAttributes.Size() == 0)
            {
                return false;
            }

            // Apply the found attributes into the currently selected rows of the table.
            ApplyAttributesToSelectedRows(formattedAttributes);

            return true;
        }

        /// <summary>
        /// Process swiped text as input into the currently selected cell.
        /// <para><b>Requirements:</b></para>
        /// One (and only one) cell may be selected.
        /// </summary>
        /// <param name="swipedText">The OCR'd text from the image swipe.</param>
        /// <throws><see cref="ExtractException"/> if more than one cell is selected.</throws>
        bool ProcessCellSwipe(SpatialString swipedText)
        {
            ExtractException.Assert("ELI24239",
                "Cell swiping is supported only for one cell at a time!", 
                SelectedCells.Count == 1);

            // Obtain the row and column where the swipe occurred. (One or both may not
            // apply depending on the selection type).
            int rowIndex = CurrentCell.RowIndex;
            int columnIndex = CurrentCell.ColumnIndex;

            bool allowAutoPop = RowAutoPopulationEnabled;

            // Cell selection mode. The swipe can be applied either via the results of a
            // column formatting rule or the swiped text value can be applied directly to
            // the cell's mapped attribute.
            DataEntryTableColumn dataEntryColumn = (DataEntryTableColumn)Columns[columnIndex];

            // Process the swiped text with a formatting rule (if available).
            if (dataEntryColumn.FormattingRule != null)
            {
                // Select the attribute name to look for from the rule results. (Could be based on
                // a sub-attribute name or the name of the table's primary attribute).
                string attributeName = (dataEntryColumn.AttributeName == ".")
                    ? AttributeName : dataEntryColumn.AttributeName;

                IAttribute attribute = DataEntryMethods.RunFormattingRule(
                    dataEntryColumn.FormattingRuleFile, swipedText, attributeName,
                    dataEntryColumn.MultipleMatchSelectionMode);
                
                // Use the value of the found attribute only if the found attribute has a non-empty
                // value.
                if (attribute != null && !string.IsNullOrEmpty(attribute.Value.String))
                {
                    swipedText = attribute.Value;
                }
            }

            // If swiping into the "new" row, create & map a new attribute as necessary.
            if (CurrentRow.IsNewRow)
            {
                // NOTE: SelectionProcessingSuppressor does not work here since ApplyAttributeToRow
                // calls ProcessSelectionChange directly.
                ApplyAttributeToRow(rowIndex, null, null);

                // [DataEntry:288] Keep cell selection on the cell that was swiped.
                CurrentCell = Rows[rowIndex].Cells[columnIndex];
            }
            else if (_modifiedRows.Contains(Rows[rowIndex]))
            {
                // https://extract.atlassian.net/browse/ISSUE-13549
                // Do not allow auto-population of values via smart hints if any modification of the
                // row has occurred.
                allowAutoPop = false;
            }

            // If there is an active text box editing control, swipe into the current
            // selection rather than replacing the entire value.
            DataGridViewTextBoxEditingControl textBoxEditingControl =
                EditingControl as DataGridViewTextBoxEditingControl;
            int selectionStart = -1;
            int selectionLength = -1;
            if (textBoxEditingControl != null)
            {
                // Keep track of what the final selection should be.
                selectionStart = textBoxEditingControl.SelectionStart;
                selectionLength = swipedText.Size;

                IDataEntryTableCell dataEntryCell =
                    Rows[rowIndex].Cells[columnIndex] as IDataEntryTableCell;

                if (dataEntryCell != null)
                {
                    swipedText = DataEntryMethods.InsertSpatialStringIntoSelection(
                        textBoxEditingControl, dataEntryCell.Attribute.Value, swipedText);
                }
            }
            
            // Apply the new value directly to the mapped attribute (Don't replace the entire 
            // attribute).
            Rows[rowIndex].Cells[columnIndex].Value = swipedText;

            // If an editing control is active, update it to reflect the result of the swipe.
            if (EditingControl != null)
            {
                RefreshEdit();

                if (textBoxEditingControl != null)
                {
                    // Select the newly swiped text.
                    textBoxEditingControl.Select(selectionStart, selectionLength);
                }

                // Forces the caret position to be updated appropriately.
                EditingControl.Focus();
            }

            // Since the spatial information for this cell has changed, spatial hints need to be
            // updated.
            // https://extract.atlassian.net/browse/ISSUE-12826
            // Allow any OCR text existing within any smart hints generated for this row to be used
            // to auto-populate the attribute values.
            List<IAttribute> autoPopulatedAttributes;
            UpdateHints(false, allowAutoPop ? (int?)rowIndex : null, out autoPopulatedAttributes);

            if (autoPopulatedAttributes.Count > 0)
            {
                RefreshAttributes(true, autoPopulatedAttributes.ToArray());
                // If any data was auto-populated, select the whole row to make it clear data
                // was added beyond just the cell where the swipe occurred.
                ClearSelection();
                Rows[rowIndex].Selected = true;
            }
            else
            {
                // Raise AttributesSelected to update the control's highlight.
                OnAttributesSelected(
                    DataEntryMethods.AttributeAsVector(
                        GetAttribute(CurrentCell)), false, true, null);
            }

            return true;
        }

        /// <summary>
        /// Inserts a new row into the table before the current row.
        /// </summary>
        /// <param name="selectNewRow"><see langword="true"/>to select the new row after pasting the
        /// data, <see langword="false"/> to leave the current selection</param>
        void InsertNewRow(bool selectNewRow)
        {
            if (_sourceAttributes != null)
            {
                // Obtain the current row and its associated attribute.
                int rowIndex = CurrentRow.Index;
                IAttribute insertBeforeAttribute = GetAttribute(CurrentRow);

                // Insert the new row and adjust the existing attribute mappings accordingly.
                Rows.Insert(rowIndex, 1);

                // Apply the new attribute to the inserted row.
                ApplyAttributeToRow(rowIndex, null, insertBeforeAttribute);

                if (selectNewRow)
                {
                    // Highlight the newly inserted row.
                    ClearSelection(-1, rowIndex, true);
                    CurrentCell = GetFirstVisibleCellInRow(Rows[rowIndex]);
                }
                else
                {
                    foreach (DataGridViewCell cell in SelectedCells)
                    {
                        if (cell.RowIndex == NewRowIndex)
                        {
                            cell.Selected = false;
                            Rows[NewRowIndex - 1].Cells[cell.ColumnIndex].Selected = true;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Deletes the currently selected row(s).
        /// </summary>
        void DeleteSelectedRows()
        {
            try
            {
                // Delay processing of changes in the control host until all rows have been deleted.
                OnUpdateStarted(new EventArgs());

                // Delete each row in the current selection.
                foreach (DataGridViewRow row in SelectedRows)
                {
                    if (!row.IsNewRow)
                    {
                        Rows.RemoveAt(row.Index);
                    }
                }
            }
            finally
            {
                OnUpdateEnded(new EventArgs());
            }
        }

        /// <summary>
        /// Copies the currently selected row(s) to the clipboard
        /// </summary>
        void CopySelectedRows()
        {
            string rowData = GetSelectedRowData();

            // If at least one row was copied, add the copied attributes to the clipboard.
            if (rowData != null)
            {
                // Add the copied attributes to the clipboard
                DataEntryMethods.SetClipboardData(RowDataFormatName, rowData);
            }
        }

        /// <summary>
        /// Attempts to copy data from the selected cells to the clipboard.
        /// <para><b>Note</b></para>
        /// Only data within a single row will be able to be copied.
        /// </summary>
        /// <returns><see langword="true"/> if data was successfully copied, <see langword="false"/>
        /// if data was not able to be copied to the clipboard.</returns>
        bool CopySelectedCells()
        {
            try
            {
                // Treat a singly selected cell as plain text.
                if (SelectedCells.Count == 1)
                {
                    // [DataEntry:1241]
                    // Just ignore the copy if null or empty
                    string value = SelectedCells[0].Value.ToString();
                    if (!string.IsNullOrEmpty(value))
                    {
                        Clipboard.SetText(value);
                    }
                    return true;
                }

                // Iterate all selected cells to ensure they are in the same row.
                List<int> selectedColumns = new List<int>();
                int selectedRow = -1;

                foreach (DataGridViewCell cell in SelectedCells)
                {
                    if (!selectedColumns.Contains(cell.ColumnIndex))
                    {
                        selectedColumns.Add(cell.ColumnIndex);
                    }

                    if (selectedRow == -1)
                    {
                        selectedRow = cell.RowIndex;
                    }
                    else if (selectedRow != cell.RowIndex)
                    {
                        selectedRow = -1;
                        break;
                    }
                }

                // If all the cell are in the same row, compile the data to copy to the clipboard
                if (selectedRow >= 0)
                {
                    // Sort the columns so that are in index order
                    string[][] data = new string[selectedColumns.Count][];
                    selectedColumns.Sort();

                    // Build an array containing each column name with its associated value.
                    for (int i = 0; i < selectedColumns.Count; i++)
                    {
                        DataEntryTableColumn dataEntryColumn =
                            Columns[selectedColumns[i]] as DataEntryTableColumn;
                        if (dataEntryColumn != null)
                        {
                            data[i] = new string[2];
                            data[i][0] = dataEntryColumn.AttributeName;
                            data[i][1] =
                                Rows[selectedRow].Cells[dataEntryColumn.Index].Value.ToString();
                        }
                        else
                        {
                            return false;
                        }
                    }

                    DataEntryMethods.SetClipboardData(MultiColumnDataFormatName, data);

                    return true;
                } 
                
                return false;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI28786", ex);
            }
        }

        /// <summary>
        /// Gets the data from the currently selected rows as a stringized byte stream.
        /// </summary>
        /// <returns>A stringized byte stream representing the <see cref="IAttribute"/>s for each
        /// of the currently selected rows.</returns>
        string GetSelectedRowData()
        {
            if (_sourceAttributes != null)
            {
                IUnknownVector copiedAttributes = new IUnknownVectorClass();

                // Get a sorted (top to bottom) list of currently selected rows.
                DataGridViewRow[] selectedRows = new DataGridViewRow[SelectedRows.Count];
                SelectedRows.CopyTo(selectedRows, 0);
                List<DataGridViewRow> selectedRowList = new List<DataGridViewRow>(selectedRows);
                selectedRowList.Sort(CompareRowsByIndex);

                // Add the attribute mapped to each row in the selection to a copied rows vector.
                foreach (DataEntryTableRow row in selectedRowList)
                {
                    // Don't copy the "new" row.
                    if (row.IsNewRow)
                    {
                        continue;
                    }

                    copiedAttributes.PushBack(row.Attribute);
                }

                // If at least one row with data was selected, return its data.
                if (copiedAttributes.Size() > 0)
                {
                    // Convert to a stringized byte stream so that the data can be preserved in the
                    // clipboard (this also removes the need to clone attributes before adding).
                    return MiscUtils.GetObjectAsStringizedByteStream(copiedAttributes);
                }
            }

            return null;
        }

        /// <overloads>
        /// Pastes attributes currently in the clipboard into the currently selected rows.
        /// </overloads>
        /// <summary>
        /// Pastes attributes currently in the clipboard into the currently selected rows.
        /// <list type="bullet">
        /// <bullet>If no attributes are in the clipboard, no action will be taken.</bullet>
        /// <bullet>If the number of attributes in the clipboard equals the number of rows
        /// selected, the selected rows will be replaced with the clipboard attributes.</bullet>
        /// <bullet>If the number of attributes in the clipboard is greater than the number of rows
        /// selected, if <see cref="DataGridView.AllowUserToAddRows"/> is <see langword="true"/>, 
        /// the selected rows will be replaced with the clipboard attributes, then new rows will be 
        /// inserted immediately following the last selected row as needed.  If 
        /// <see cref="DataGridView.AllowUserToAddRows"/> is <see langword="false"/>, at least one
        /// attribute found in the clipboard will not be added to the table.</bullet>
        /// <bullet>If the number of attributes in the clipboard is less than the number of rows 
        /// selected then the clipboard attributes will replace the selected rows until there are no 
        /// more clipboard attributes. If <see cref="DataGridView.AllowUserToDeleteRows"/> is 
        /// <see langword="true"/>, the remaining selected rows will then be deleted (otherwise,
        /// they will remain.</bullet>
        /// </list>
        /// </summary>
        /// <param name="dataObject">The <see cref="IDataObject"/> containing the data to be pasted
        /// into the currently selected row(s).</param>
        void PasteRowData(IDataObject dataObject)
        {
            PasteRowData(dataObject, false);
        }

        /// <summary>
        /// Pastes attributes currently in the clipboard into the currently selected rows.
        /// <list type="bullet">
        /// <bullet>If no attributes are in the clipboard, no action will be taken.</bullet>
        /// <bullet>If the number of attributes in the clipboard equals the number of rows
        /// selected, the selected rows will be replaced with the clipboard attributes.</bullet>
        /// <bullet>If the number of attributes in the clipboard is greater than the number of rows
        /// selected, if <see cref="DataGridView.AllowUserToAddRows"/> is <see langword="true"/>, 
        /// the selected rows will be replaced with the clipboard attributes, then new rows will be 
        /// inserted immediately following the last selected row as needed.  If 
        /// <see cref="DataGridView.AllowUserToAddRows"/> is <see langword="false"/>, at least one
        /// attribute found in the clipboard will not be added to the table.</bullet>
        /// <bullet>If the number of attributes in the clipboard is less than the number of rows 
        /// selected then the clipboard attributes will replace the selected rows until there are no 
        /// more clipboard attributes. If <see cref="DataGridView.AllowUserToDeleteRows"/> is 
        /// <see langword="true"/>, the remaining selected rows will then be deleted (otherwise,
        /// they will remain.</bullet>
        /// </list>
        /// </summary>
        /// <param name="dataObject">The <see cref="IDataObject"/> containing the data to be pasted
        /// into the currently selected row(s).</param>
        /// <param name="dragAndDrop">If <see langword="true"/> then the paste is being called
        /// from a drag and drop event.</param>
        void PasteRowData(IDataObject dataObject, bool dragAndDrop)
        {
            try
            {
                // Delay processing of changes in the control host until PasteRowData is complete.
                OnUpdateStarted(new EventArgs());

                string dataType = GetDataFormatName(dataObject);

                // If the data on the clipboard is a string, use the row formatting rule to parse the
                // text into the row.
                if (!string.IsNullOrWhiteSpace(RowFormattingRuleFile) && dataType == "System.String")
                {
                    SpatialString spatialString = new SpatialStringClass();
                    spatialString.CreateNonSpatialString((string)dataObject.GetData(dataType), "");

                    ProcessRowSwipe(spatialString);
                }
                // Otherwise the data is attributes from this table or a compatible one.
                else if (!string.IsNullOrEmpty(dataType))
                {
                    // Retrieve the data from the clipboard and convert it back into an attribute vector.
                    string stringizedAttributes = (string)dataObject.GetData(dataType);

                    IUnknownVector attributesToPaste = (IUnknownVector)
                        MiscUtils.GetObjectFromStringizedByteStream(stringizedAttributes);

                    // Make a copy of the attributes before using them
                    ICopyableObject ipCopy = attributesToPaste;
                    if (ipCopy != null)
                    {
                        attributesToPaste = (IUnknownVector)ipCopy.Clone();
                    }

                    bool renameAttributesFromOtherTable = dataType != RowDataFormatName;
                    int count = attributesToPaste.Size();
                    for (int i = 0; i < count; i++)
                    {
                        IAttribute attribute = (IAttribute)attributesToPaste.At(i);

                        // If the attributes are not from this table, rename them.
                        if (renameAttributesFromOtherTable)
                        {
                            attribute.Name = AttributeName;
                        }
                    }

                    // Apply the attributes into the currently selected rows of the table.
                    ApplyAttributesToSelectedRows(attributesToPaste);
                }

                try
                {
                    // [DataEntry:757]
                    // DataEntryControlHost will be unable to determine when clipboard data needs to be
                    // cleared for row pasting. Clear the clipboard if specified.
                    if (!dragAndDrop && ClearClipboardOnPaste && Clipboard.ContainsText())
                    {
                        DataEntryMethods.ClearClipboardData();
                    }
                }
                catch (Exception ex)
                {
                    // https://extract.atlassian.net/browse/ISSUE-14294
                    // Clipboard operations being finicky has long been an issue. Don't allow a
                    // failure checking clipboard data to cause a larger issue.
                    ex.ExtractLog("ELI41661");
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI27271", ex);
            }
            finally
            {
                OnUpdateEnded(new EventArgs());
            }
        }

        /// <summary>
        /// Attempts to paste data from the clipboard into multiple selected cells. This includes
        /// the case that multiple cells are selected for pasting, or that the currently selected
        /// cell matches the first column of multicolumn data on the clipboard.
        /// </summary>
        /// <param name="dataObject">The <see cref="IDataObject"/> containing the data to be pasted
        /// into the currently selected cells(s).</param>
        /// <returns><see langword="true"/> if data as successfully pasted, <see langword="false"/>
        /// if it could not be pasted.</returns>
        bool PasteCellData(IDataObject dataObject)
        {
            try
            {
                if (dataObject == null)
                {
                    return false;
                }

                // If multicolumn data from this table is on the clipboard, try to paste it into the
                // current selection.
                if (dataObject.GetDataPresent(MultiColumnDataFormatName))
                {
                    // Extract the pasted data
                    string[][] data = (string[][])dataObject.GetData(MultiColumnDataFormatName);
                    bool pastedData = false;

                    // Prevent edit control from handling paste.
                    if (EditingControl != null)
                    {
                        EndEdit();
                    }

                    // If a single cell is selected, check to see if it matches the first column in
                    // the selected data.
                    if (SelectedCells.Count == 1)
                    {
                        // Get the row and column of the selected cell
                        DataGridViewCell cell = SelectedCells[0];
                        DataGridViewRow row = Rows[cell.RowIndex];
                        DataEntryTableColumn dataEntryColumn =
                            Columns[cell.ColumnIndex] as DataEntryTableColumn;

                        // If the name of the column the selected cell is in matches that of the
                        // first column of clipboard data, paste all columns from the clipboard.
                        if (dataEntryColumn != null &&
                            dataEntryColumn.AttributeName == data[0][0])
                        {
                            if (cell.RowIndex == NewRowIndex)
                            {
                                // [DataEntry:787]
                                // If pasting into the new row, manually add the new row and
                                // initialize it before settings the cell data.
                                InsertNewRow(false);
                                row = Rows[NewRowIndex - 1];
                                cell = row.Cells[cell.ColumnIndex];

                                // Reset CurrentCell to the manually added row, otherwise the
                                // DataGridView will try to automatically add another row.
                                CurrentCell = row.Cells[cell.ColumnIndex];  
                            }

                            // Paste the data into successive visible cells starting with the
                            // selected cell.
                            for (int i = 0; i < data.Length; i++)
                            {
                                cell.Value = data[i][1];

                                int nextVisibleColumn;
                                for (nextVisibleColumn = cell.ColumnIndex + 1;
                                     nextVisibleColumn < Columns.Count &&
                                        !Columns[nextVisibleColumn].Visible;
                                     nextVisibleColumn++) { }

                                if (nextVisibleColumn >= Columns.Count)
                                {
                                    break;
                                }

                                cell = row.Cells[nextVisibleColumn];
                            }

                            pastedData = true;
                        }
                        else if (data.Length == 1)
                        {
                            // If a single cell was copied, allow a single cell to be pasted no
                            // matter the column.
                            row.Cells[cell.ColumnIndex].Value = data[0][1];

                            pastedData = true;
                        }
                    }
                    // If multiple cells are selected, paste data into any cells whose column
                    // is included in the clipboard data.
                    else
                    {
                        // Create a dictionary mapping clipboard data column names to the
                        // associated value.
                        Dictionary<string, string> columnData = new Dictionary<string, string>();
                        for (int i = 0; i < data.Length; i++)
                        {
                            columnData[data[i][0]] = data[i][1];
                        }

                        int newRowIndex = -1;

                        // Cycle through each selected cell looking for cells in the clipboard
                        // columns.
                        foreach (DataGridViewCell cell in SelectedCells)
                        {
                            DataEntryTableColumn dataEntryColumn =
                                Columns[cell.ColumnIndex] as DataEntryTableColumn;
                            string value;
                            if (dataEntryColumn != null &&
                                columnData.TryGetValue(dataEntryColumn.AttributeName, out value))
                            {
                                // Add new row if necessary.
                                if (cell.RowIndex == NewRowIndex)
                                {
                                    if (newRowIndex == -1)
                                    {
                                        newRowIndex = cell.RowIndex;
                                        InsertNewRow(false);
                                    }

                                    Rows[newRowIndex].Cells[cell.ColumnIndex].Value = value;
                                }
                                // Otherwise, just set the cell's value.
                                else
                                {
                                    cell.Value = value;
                                }

                                pastedData = true;
                            }
                        }

                        if (pastedData)
                        {
                            CurrentCell = SelectedCells.OfType<DataGridViewCell>().Last();
                        }
                    }

                    if (!pastedData)
                    {
                        MessageBox.Show(this, "Unable to paste into the current selection.",
                            "Paste error", MessageBoxButtons.OK, MessageBoxIcon.Information,
                            MessageBoxDefaultButton.Button1, 0);
                    }

                    return true;
                }
                // Allow pasting of plain text into multiple cells as long as the cells are in the
                // same column and we are not in edit mode (in which case the paste should be
                // treated as a normal text paste into the active cell).
                else if (dataObject.GetDataPresent("System.String") && EditingControl == null)
                {
                    // Ensure all cells are in the same column.
                    int columnIndex = -1;
                    foreach (DataGridViewCell cell in SelectedCells)
                    {
                        if (columnIndex == -1)
                        {
                            columnIndex = cell.ColumnIndex;
                        }
                        else
                        {
                            if (columnIndex != cell.ColumnIndex)
                            {
                                columnIndex = -1;
                                break;
                            }
                        }
                    }

                    // If all cells are in the same column, apply the pasted text to all selected
                    // cells.
                    if (columnIndex >= 0)
                    {
                        foreach (DataGridViewCell cell in SelectedCells)
                        {
                            // Add new row if necessary.
                            if (cell.RowIndex == NewRowIndex)
                            {
                                InsertNewRow(false);
                                Rows[NewRowIndex - 1].Cells[cell.ColumnIndex].Value =
                                    Clipboard.GetText();
                            }
                            // Otherwise, just set the cell's value.
                            else
                            {
                                cell.Value = Clipboard.GetText();
                            }
                        }

                        CurrentCell = SelectedCells.OfType<DataGridViewCell>().Last();

                        return true;
                    }

                    if (SelectedCells.Count > 1)
                    {
                        MessageBox.Show(this, "Pasting a single value into multiple cells is " +
                            "allowed only when all cells are in the same column.",
                            "Paste error", MessageBoxButtons.OK, MessageBoxIcon.Information,
                            MessageBoxDefaultButton.Button1, 0);
                    }
                }
                else if (!dataObject.GetDataPresent("System.String") && EditingControl != null)
                {
                    // If there is no data that can be pasted into the active edit control
                    // cycle the edit mode to prevent data in the selected cell from being cleared.
                    EndEdit();
                    BeginEdit(true);
                }

                return false;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI28787", ex);
            }
        }

        /// <summary>
        /// Applies the specified <see cref="IAttribute"/>s to the currently selected rows.
        /// <list type="bullet">
        /// <bullet>If no attributes are specified, no action will be taken.</bullet>
        /// <bullet>If the number of attributes specified equals the number of rows
        /// selected, the selected rows will be replaced with the specified attributes.</bullet>
        /// <bullet>If the number of attributes specified is greater than the number of rows
        /// selected, if <see cref="DataGridView.AllowUserToAddRows"/> is <see langword="true"/>, 
        /// the selected rows will be replaced with the specified attributes, then new rows will be 
        /// inserted immediately following the last selected row as needed.  If 
        /// <see cref="DataGridView.AllowUserToAddRows"/> is <see langword="false"/>, at least one
        /// specified attribute will not be added to the table.</bullet>
        /// <bullet>If the number of attributes specified is less than the number of rows 
        /// selected then the specified attributes will replace the selected rows until there are no 
        /// more specified attributes. If <see cref="DataGridView.AllowUserToDeleteRows"/> is 
        /// <see langword="true"/>, the remaining selected rows will then be deleted (otherwise,
        /// they will remain.</bullet>
        /// </list>
        /// </summary>
        /// <param name="attributes">An <see cref="IUnknownVector"/> of 
        /// <see cref="IAttribute"/>s to be applied </param>
        void ApplyAttributesToSelectedRows(IUnknownVector attributes)
        {
            try
            {
                ExtractException.Assert("ELI26146", "Uninitialized data!",
                    _sourceAttributes != null);

                int count = attributes.Size();

                // If no attributes were found, don't do anything.
                if (count == 0)
                {
                    return;
                }

                // Temporarily suppress selection changes as several will occur programmatically in
                // in the course of populating the table. In addition to making the code more
                // efficient, this insures hints are updated before the control host processes the
                // selection change and re-draws the highlights.
                // [DataEntry:1188]
                // Also suppress all calls to RefreshAttributes(bool, IAttribute[]) while data is
                // still being applied to the rows to prevent cases where spatial info can be
                // incorrectly cleared by refreshing on incomplete data.
                using (new SelectionProcessingSuppressor(this))
                using (new RefreshSuppressor(this))
                {
                    // Get a sorted (top to bottom) list of currently selected rows.
                    DataGridViewRow[] selectedRows = new DataGridViewRow[SelectedRows.Count];
                    SelectedRows.CopyTo(selectedRows, 0);
                    List<DataGridViewRow> destinationRows = new List<DataGridViewRow>(selectedRows);
                    destinationRows.Sort(CompareRowsByIndex);

                    // Add the found attributes to the table.
                    int rowsAdded;
                    IAttribute insertBeforeAttribute = null;
                    for (rowsAdded = 0; rowsAdded < count; rowsAdded++)
                    {
                        // Check to see if there were more found attributes than there were
                        // selected rows in the table.
                        if (rowsAdded == destinationRows.Count)
                        {
                            // If the table doesn't allow new rows, break without adding the 
                            // remaining attributes.
                            if (AllowUserToAddRows == false)
                            {
                                break;
                            }

                            // Add a new row for the next attribute and add it to the
                            // destinationRows list.
                            int nextIndex = destinationRows[destinationRows.Count - 1].Index + 1;
                            insertBeforeAttribute = GetAttribute(Rows[nextIndex]);
                            Rows.Insert(nextIndex, 1);
                            destinationRows.Add(Rows[nextIndex]);
                        }

                        IAttribute attribute = (IAttribute)attributes.At(rowsAdded);

                        // [DataEntry:426]
                        // Ensure the parent attribute is initialized before applying the attribute
                        // to the row, otherwise the parentAttribute property will not be set
                        // correctly for column attributes. 
                        if (AttributeStatusInfo.GetParentAttribute(attribute) == null)
                        {
                            AttributeStatusInfo.Initialize(attribute, _sourceAttributes, this);
                        }

                        ApplyAttributeToRow(destinationRows[rowsAdded].Index, attribute,
                            insertBeforeAttribute);

                        // If the attribute was added into the "new" row, the current entry in 
                        // destinationRows will now represent the new "new" row.  Therefore, adjust
                        // the entry to reflect previous row which is the one that was just added.
                        if (destinationRows[rowsAdded].IsNewRow)
                        {
                            destinationRows[rowsAdded] = Rows[NewRowIndex - 1];
                        }
                    }

                    // If deletions are allowed, delete all selected rows that were not replaced with
                    // attribute from the swiped text.
                    if (AllowUserToDeleteRows)
                    {
                        while (rowsAdded < destinationRows.Count)
                        {
                            if (destinationRows[rowsAdded].Index != NewRowIndex)
                            {
                                Rows.RemoveAt(destinationRows[rowsAdded].Index);
                            }

                            destinationRows.RemoveAt(rowsAdded);
                        }
                    }

                    // Adjust the current selection to encapsulate all rows populated via swiped text
                    // with the current row being the last row populated.
                    ClearSelection(-1, destinationRows[destinationRows.Count - 1].Index, true);
                    CurrentCell = GetFirstVisibleCellInRow(destinationRows[destinationRows.Count - 1]);
                    foreach (DataGridViewRow row in destinationRows)
                    {
                        row.Selected = true;
                    }

                    // Since the spatial information for this cell has likely changed, spatial hints
                    // need to be updated.
                    UpdateHints(false);

                    // If TabOrderPlaceholderAttribute is being used, make sure it remains the last
                    // attribute from this control in _sourceAttributes.
                    if (TabOrderPlaceholderAttribute != null)
                    {
                        DataEntryMethods.ReorderAttributes(_sourceAttributes,
                            DataEntryMethods.AttributeAsVector(TabOrderPlaceholderAttribute));
                    }
                }

                // Any refreshes suppressed above can now occur.
                ExecutePendingRefresh();

                // Highlights the results of the swipe in the image viewer and propagates the
                // selection to dependent controls (if appropriate)
                ProcessSelectionChange();
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI25503", ex);
            }
        }

        /// <summary>
        /// Checks to see if row and/or row paste tasks should be available based on an event
        /// in the specified row and column index where -1 indicates a row or column header.
        /// </summary>
        /// <param name="originRowIndex">The index of the row from which an event originated.
        /// </param>
        /// <param name="originColumnIndex">The index of the column from which an event originated.
        /// </param>
        /// <returns><see langword="true"/> if general row tasks should be available for execution; 
        /// <see langword="false"/> otherwise.
        /// </returns>
        bool AllowRowTasks(int originRowIndex, int originColumnIndex)
        {
            // If the table is not currently mapped, row tasks are not allowed.
            if (_sourceAttributes == null)
            {
                return false;
            }

            // [DataEntry:653]
            // If the current selection is currently being reset manually and a mouse button
            // is down, disallow any scrolling until the mouse button is released. Otherwise
            // the mouse release may occur over a different cell due to scrolling and result
            // in unintended cell selection.
            if (MouseButtons != MouseButtons.None)
            {
                _selectionIsBeingReset = true;
            }

            // Determine if row-level options are to be available based on the clicked location
            // and the current selection.
            bool enableRowOptions = (originRowIndex >= 0 &&
                                     (originColumnIndex == -1 || Rows[originRowIndex].Selected));

            if (!enableRowOptions && originRowIndex >= 0 && originColumnIndex >= 0)
            {
                // If the menu was opened via a data entry cell, check to see if the clicked
                // cell is currently selected.  If not, select this cell instead to prevent
                // confusion about which cell(s) will be acted upon.
                DataGridViewCell clickedCell = Rows[originRowIndex].Cells[originColumnIndex];

                if (!clickedCell.Selected)
                {
                    CurrentCell = clickedCell;
                }
            }
            else if (originRowIndex >= 0)
            {
                // If the menu was opened via a row header, checked to see if the menu was
                // opened within a currently selected row.
                DataGridViewRow clickedRow = Rows[originRowIndex];
                if (clickedRow.Selected && CurrentRow != clickedRow)
                {
                    // If the clicked row was selected, but not the "current" row, we want to make
                    // it the current row so it is clear which row is being used as the basis for
                    // for a row insertion operation.
                    DataGridViewRow[] selectedRows = new DataGridViewRow[SelectedRows.Count];
                    SelectedRows.CopyTo(selectedRows, 0);

                    // Suppress selection while changing the selection to prevent undesirable
                    // flickering of tests while adjusting the selection.
                    using (new SelectionProcessingSuppressor(this))
                    {
                        // Set the current cell to the first visible cell in the clicked row
                        CurrentCell = GetFirstVisibleCellInRow(clickedRow);

                        // Changing the current cell will have cleared the previously selected rows.
                        // Re-select them now.
                        foreach (DataGridViewRow row in selectedRows)
                        {
                            row.Selected = true;
                        }
                    }
                }
                else if (CurrentRow != clickedRow)
                {
                    ClearSelection(-1, originRowIndex, true);

                    // If the clicked row is not the current row, make the click row current
                    // and selected.
                    CurrentCell = GetFirstVisibleCellInRow(clickedRow);
                }
                else if (!CurrentRow.Selected)
                {
                    ClearSelection(-1, originRowIndex, true);
                }
            }

            return enableRowOptions;
        }

        /// <summary>
        /// Manually handles the enter key to mimic navigation as in Excel.
        /// </summary>
        /// <returns><see langword="true"/> if the key was processed by the
        /// <see cref="DataEntryTable"/>; otherwise, <see langword="false"/>.</returns>
        bool ProcessEnterKey()
        {
            if (SelectedCells.Count == 1 && CurrentCell != null)
            {
                int newRowIndex = CurrentCell.RowIndex;
                if (newRowIndex < (RowCount - 1))
                {
                    newRowIndex++;
                }

                if (AutoCompleteCell == null)
                {
                    CurrentCell = Rows[newRowIndex].Cells[_carriageReturnColumn];
                }
                // [DataEntry:759, 1281]
                // If an auto-complete list is currently displayed, do not advance the selection to
                // the next row if the enter key is not actually down (a mouse click on the
                // auto-complete results in an enter keystroke being processed even thought the
                // enter key was not actually pressed).
                else if (!DataEntryControlHost.IsKeyDown(Keys.Enter))
                {
                    // Since enter will close the auto-complete list, AutoCompleteCell should be
                    // set back to null.
                    AutoCompleteCell = null;
                }
                else
                {
                    // If the enter key was pressed while the auto-complete list was displayed,
                    // allow the table to process the keystroke as it normally would.
                    return false;
                }

                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Gets whether there are rows currently being dragged from this table and control.
        /// </summary>
        /// <returns><see langword="true"/> if there are rows currently being dragged from this
        /// table and control; <see langword="false"/> otherwise.</returns>
        bool RowsBeingDraggedFromCurrentTable
        {
            get
            {
                return (_draggedRows != null && _activeCachedRows != null &&
                        _activeCachedRows.ContainsValue(_draggedRows[0]));
            }
        }

        /// <summary>
        /// Begins edit mode using the specified value as the initial value and initializes the
        /// auto-complete list if appropriate.
        /// </summary>
        /// <param name="initialValue">The initial value the cell should have after entering edit
        /// mode. If the specified value is a single character, the auto-complete list will be
        /// displayed as appropriate.</param>
        void BeginEdit(string initialValue)
        {
            try
            {
                base.BeginEdit(false);

                if (initialValue.Length == 1)
                {
                    // If the initial value is a single character, assume it is typed and re-send it
                    // as WindowsMessage.Character message via SendCharacterToControl to trigger
                    // auto-complete to display.
                    KeyMethods.SendCharacterToControl(initialValue[0], EditingControl);
                }
                else if (initialValue.Length > 1)
                {
                    // For initial values > 1 char (i.e., pasted text), simply re-apply the initial
                    // value and refresh the attribute.
                    IDataEntryTableCell dataEntryCell = base.CurrentCell as IDataEntryTableCell;
                    if (dataEntryCell != null)
                    {
                        base.CurrentCell.Value = initialValue;
                        base.RefreshAttributes(false, dataEntryCell.Attribute);
                    }
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI29988", ex);
            }
        }

        /// <summary>
        /// Gets a tab order place-holder attribute for the currently propagated _sourceAttributes.
        /// </summary>
        /// <returns>An <see cref="IAttribute"/> to use as a placeholder for tab order. May be
        /// <see langword="null"/> if no attributes are currently propagated, the table doesn't have
        /// a "new" row or the table is not visible.</returns>
        IAttribute TabOrderPlaceholderAttribute
        {
            get
            {
                // A TabOrderPlaceholderAttribute is not needed if there is no currently propagated
                // attributes, the table does not support new rows.
                // https://extract.atlassian.net/browse/ISSUE-13005
                // I have removed the check that is preventing the placeholder attribute creation in
                // cases where the control is not visible. To support the case that controls may
                // programmatically shown and hidden, do not assume that because a control is not
                // currently visible that it never will be.
                if (_sourceAttributes == null || !AllowUserToAddRows)
                {
                    return null;
                }

                // Look up a previously created placeholder attribute for the current
                // _sourceAttributes set, if one is available.
                IAttribute tabOrderPlaceholderAttribute;
                if (!_tabOrderPlaceholderAttributes.TryGetValue(
                    _sourceAttributes, out tabOrderPlaceholderAttribute))
                {
                    // This was added to make sure that the place holder will be after the last attribute
                    // in the set. 
                    // https://extract.atlassian.net/browse/ISSUE-1163
                    var displayOrder = DataEntryMethods.GetTabIndices(this).Concat(new[] { _sourceAttributes.Size() });

                    // If no placeholder attribute has been created for this set, create one now.
                    tabOrderPlaceholderAttribute = DataEntryMethods.InitializeAttribute(
                        "PlaceholderAttribute_" + Name, MultipleMatchSelectionMode.First,
                        true, _sourceAttributes, null, this, displayOrder, true, TabStopMode.Always,
                        null, null, null);

                    AttributeStatusInfo placeholderStatusInfo =
                        AttributeStatusInfo.GetStatusInfo(tabOrderPlaceholderAttribute);
                    placeholderStatusInfo.AttributeDeleted += HandlePlaceholderAttributeDeleted;

                    // Don't persist placeholder attributes in output.
                    AttributeStatusInfo.SetAttributeAsPersistable(
                        tabOrderPlaceholderAttribute, false);

                    // Mark this attribute as viewable even though it will not be used to store any
                    // useful data so that focus will be directed to it.
                    AttributeStatusInfo.MarkAsViewable(tabOrderPlaceholderAttribute, true);

                    // If this table supports tabbing by row, assign the row attribute a group
                    // consisting of all column attributes.
                    if (_allowTabbingByRow && !Disabled)
                    {
                        List<IAttribute> tabGroup = new List<IAttribute>(
                            new IAttribute[] { tabOrderPlaceholderAttribute });
                        AttributeStatusInfo.SetAttributeTabGroup(
                            tabOrderPlaceholderAttribute, tabGroup);
                    }

                    _tabOrderPlaceholderAttributes[_sourceAttributes] = tabOrderPlaceholderAttribute;
                }

                return tabOrderPlaceholderAttribute;
            }
        }

        /// <summary>
        /// Returns the first visible cell in the given row
        /// </summary>
        /// <param name="row">Row to find the first visible cell</param>
        /// <returns>The first visible cell in the row</returns>
        static DataGridViewCell GetFirstVisibleCellInRow(DataGridViewRow row)
        {
            return row.Cells.Cast<DataGridViewCell>().First(c => c.Visible == true);
        }

        #endregion Private Members
    }
}
