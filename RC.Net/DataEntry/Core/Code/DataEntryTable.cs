using Extract.Interop;
using Extract.Licensing;
using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.ComponentModel.Design;
using System.Drawing;
using System.Text;
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
        bool _cellSwipingEnabled;

        /// <summary>
        /// Indicates whether swiping should be allowed when a complete row is selected.
        /// </summary>
        bool _rowSwipingEnabled;

        /// <summary>
        /// The filename of the rule file to be used to parse swiped data into rows.
        /// </summary>
        string _rowFormattingRuleFileName;

        /// <summary>
        /// The formatting rule to be used when processing text from imaging swipes for a row at a 
        /// time.
        /// </summary>
        IRuleSet _rowFormattingRule;

        /// <summary>
        /// Specifies the minimum number of rows a DataEntryTable must have.  If the specified
        /// number of attributes are not found, new, blank ones are created as necessary.
        /// </summary>
        int _minimumNumberOfRows;

        /// <summary>
        /// Specifies names of attributes that can be mapped into this control by renaming them.
        /// The purpose is to be able to copy data out of one control and paste it into another.
        /// </summary>
        Collection<string> _compatibleAttributeNames = new Collection<string>();

        /// <summary>
        /// Context MenuItem that allows a row to be inserted at the current location.
        /// </summary>
        ToolStripMenuItem _rowInsertMenuItem = new ToolStripMenuItem("Insert row");

        /// <summary>
        /// Context MenuItem that allows the selected row(s) to be deleted.
        /// </summary>
        ToolStripMenuItem _rowDeleteMenuItem = new ToolStripMenuItem("Delete row(s)");

        /// <summary>
        /// Context MenuItem that allows the selected row(s) to be copied to the clipboard.
        /// </summary>
        ToolStripMenuItem _rowCopyMenuItem = new ToolStripMenuItem("Copy row(s)");

        /// <summary>
        /// Context MenuItem that allows the selected row(s) to be copied to the clipboard and
        /// deleted from the table.
        /// </summary>
        ToolStripMenuItem _rowCutMenuItem = new ToolStripMenuItem("Cut row(s)");

        /// <summary>
        /// Context MenuItem that allows the selected row(s) to be pasted into the current
        /// selection from the clipboard.
        /// </summary>
        ToolStripMenuItem _rowPasteMenuItem = new ToolStripMenuItem("Paste copied row(s)");

        /// <summary>
        /// Context MenuItem that allows the selected row(s) to be inserted into from the clipboard.
        /// </summary>
        ToolStripMenuItem _rowInsertCopiedMenuItem = new ToolStripMenuItem("Insert copied row(s)");

        /// <summary>
        /// The domain of attributes to which this control's attribute(s) belong.
        /// </summary>
        IUnknownVector _sourceAttributes;

        /// <summary>
        /// An attribute used to direct focus to the first cell of the "new" row of the table before
        /// focus leaves the control.  This attribute will not store any data useful as output.
        /// </summary>
        IAttribute _tabOrderPlaceholderAttribute;

        /// <summary>
        /// A cache of DataEntryTableRows that have been populated so that when parent control
        /// propagates between different attributes, the propagated attribute does not need to be
        /// re-initialized as long as it had previously been initialized.
        /// </summary>
        Dictionary<IUnknownVector, Dictionary<IAttribute, DataEntryTableRow>> _cachedRows =
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
        /// Whether the <see cref="Control.MouseDown"/> event is currently being suppressed in
        /// order to maintain row selection for drag and drop operations.
        /// </summary>
        bool _suppressingMouseDown;

        /// <summary>
        /// Specifies whether the current instance is running in design mode.
        /// </summary>
        bool _inDesignMode;

        /// <summary>
        /// A lazily instantiated <see cref="MiscUtils"/> instance to use for converting 
        /// IPersistStream implementations to/from a stringized byte stream.
        /// </summary>
        MiscUtils _miscUtils;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new <see cref="DataEntryTable"/> instance.
        /// </summary>
        public DataEntryTable()
            : base()
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
                LicenseUtilities.ValidateLicense(LicenseIdName.FlexIndexCoreObjects, "ELI24490",
                    _OBJECT_NAME);

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
        public string RowFormattingRuleFile
        {
            get
            {
                return _rowFormattingRuleFileName;
            }

            set
            {
                try
                {
                    // If not in design mode and a formatting rule is specified, attempt to load
                    // the attribute finding rule.
                    if (!_inDesignMode && !string.IsNullOrEmpty(value))
                    {
                        _rowFormattingRule = (IRuleSet) new RuleSetClass();
                        _rowFormattingRule.LoadFrom(DataEntryMethods.ResolvePath(value), false);
                    }
                    else
                    {
                        _rowFormattingRule = null;
                    }
                    
                    _rowFormattingRuleFileName = value;
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI24237", ex);
                }
            }
        }

        /// <summary>
        /// Specifies whether GetSpatialHint will attempt to generate a hint using the
        /// intersection of the row and column occupied by the specified <see cref="IAttribute"/>.
        /// </summary>
        /// <value><see langword="true"/> if the table should attempt to generate smart hints when
        /// possible; <see langword="false"/> if the table should never attempt to generate smart
        /// hints.</value>
        /// <returns><see langword="true"/> if the table is configured to generate smart hints when
        /// possible; <see langword="false"/> if the table is not configured to generate smart
        /// hints.</returns>
        [Category("Data Entry Table")]
        public new bool SmartHintsEnabled
        {
            get
            {
                return base.SmartHintsEnabled;
            }

            set
            {
                base.SmartHintsEnabled = value;
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
        }

        /// <summary>
        /// Raises the <see cref="Control.HandleCreated"/> event in order to verify that the table
        /// has been properly configured and to create the context menu before data entry commences.
        /// </summary>
        /// <param name="e">A <see cref="EventArgs"/> that contains the event data.</param>
        protected override void OnHandleCreated(EventArgs e)
        {
            try
            {
                base.OnHandleCreated(e);

                // If we are not in design mode and the control is being made visible for the first
                // time, verify the table is properly configured and create the context menu.
                if (!_inDesignMode)
                {
                    ExtractException.Assert("ELI24222",
                        "A DataEntryTable must have an AttributeName specified!",
                        !string.IsNullOrEmpty(base.AttributeName));

                    ExtractException.Assert("ELI24249",
                        "Row swiping is enabled, but no row formatting rule was specified!",
                        !this.RowSwipingEnabled || _rowFormattingRule != null);

                    // Create a context menu for the table
                    base.ContextMenuStrip = new ContextMenuStrip();

                    // Insert row menu option
                    if (base.AllowUserToAddRows)
                    {
                        _rowInsertMenuItem.Enabled = false;
                        _rowInsertMenuItem.Click += HandleInsertMenuItemClick;
                        base.ContextMenuStrip.Items.Add(_rowInsertMenuItem);
                    }

                    // Delete row(s) menu option
                    if (base.AllowUserToDeleteRows)
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
                    if (base.AllowUserToDeleteRows)
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
                    if (base.AllowUserToAddRows)
                    {
                        _rowInsertCopiedMenuItem.Enabled = false;
                        _rowInsertCopiedMenuItem.Click += HandleRowInsertMenuItemClick;
                        base.ContextMenuStrip.Items.Add(_rowInsertCopiedMenuItem);
                    }
                  
                    // Handle the opening of the context menu so that the available options and selection 
                    // state can be finalized.
                    base.ContextMenuStrip.Opening += HandleContextMenuOpening;

                    // Handle the case that rows collection has been changed so that
                    // _sourceAttributes can be updated as appropriate.
                    base.Rows.CollectionChanged += HandleRowsCollectionChanged;
                }
            }
            catch (Exception ex)
            {
                ExtractException.AsExtractException("ELI24250", ex).Display();
            }
        }

        /// <summary>
        /// Highlights the currently selected <see cref="IAttribute"/>s in the image viewer and
        /// propagates the selected row(s) as appropriate.
        /// </summary>
        protected override void ProcessSelectionChange()
        {
            base.ProcessSelectionChange();

            // Create a vector to store the attributes in the currently selected row(s).
            IUnknownVector selectedAttributes = (IUnknownVector)new IUnknownVectorClass();

            if (base.SelectedRows.Count > 0)
            {
                // Indicates whether the only selected row is the new row.
                bool newRowSelection = false;

                // Loop through each selected row and add the attribute corresponding to each
                // to the set of selected attributes;
                foreach (DataGridViewRow selectedRow in base.SelectedRows)
                {
                    IAttribute attribute = DataEntryTableBase.GetAttribute(selectedRow);
                    if (attribute != null)
                    {
                        selectedAttributes.PushBack(attribute);
                    }
                    else if (selectedRow.Index == base.NewRowIndex && base.SelectedRows.Count == 1)
                    {
                        newRowSelection = true;
                    }
                }

                // Notify listeners new attribute(s) needs to be propagated
                OnPropagateAttributes(selectedAttributes);

                if (newRowSelection)
                {
                    // [DataEntry:346]
                    // If the new row is the only row selected, report 
                    // _tabOrderPlaceholderAttribute as the selected attribute so that the
                    // control host will be able to correctly direct tab order forward and
                    // backward from this point.
                    selectedAttributes.PushBack(this._tabOrderPlaceholderAttribute);
                }

                // Notify listeners that the spatial info to be associated with the table has
                // changed (include all subattributes to the row(s)'s attribute(s) in the 
                // spatial info).
                OnAttributesSelected(selectedAttributes, true, selectedAttributes.Size() == 1);
            }
            else if (base.SelectedCells.Count > 0)
            {
                // Create a collection to keep track of the attributes for each row in which a cell
                // is selected.
                IUnknownVector selectedRowAttributes = (IUnknownVector)new IUnknownVectorClass();

                // If the selection is on a cell-by-cell basis rather, we need to compile the
                // attributes corresponding to each cell for the AttributesSelected event,
                // then find the attribute for the current row for the PropagateAttributes
                // event.
                foreach (IDataEntryTableCell selectedDataEntryCell in base.SelectedCells)
                {
                    // Keep track of the cells' attributes for OnAttributesSelected
                    IAttribute attribute = DataEntryTableBase.GetAttribute(selectedDataEntryCell);
                    if (attribute != null)
                    {
                        selectedAttributes.PushBack(attribute);
                    }

                    DataGridViewCell selectedCell = (DataGridViewCell)selectedDataEntryCell;

                    // Keep track of the rows' attributes for OnPropagateAttributes
                    DataEntryTableRow row = base.Rows[selectedCell.RowIndex] as DataEntryTableRow;
                    if (row != null && row.Attribute != null)
                    {
                        selectedRowAttributes.PushBackIfNotContained(row.Attribute);
                    }
                }

                // Raises the PropagateAttributes event to notify dependent controls that new
                // attribute(s) need to be propagated.
                OnPropagateAttributes(selectedRowAttributes);

                // If the a cell of the "new" row is selected, report 
                // _tabOrderPlaceholderAttribute as the selected attribute so that the control host
                // will be able to correctly direct tab order forward and backward from this point.
                if (_tabOrderPlaceholderAttribute != null &&
                    selectedRowAttributes.Size() == 0 &&
                    base.CurrentCell != null && base.CurrentCell.RowIndex == base.NewRowIndex)
                {
                    selectedAttributes.PushBack(_tabOrderPlaceholderAttribute);
                }

                // Include all the attributes for the specifically selected cells in the 
                // spatial info, not any children of those attributes. Show tooltips only
                // if one attribute is selected.
                OnAttributesSelected(selectedAttributes, false, selectedRowAttributes.Size() == 1);
            }
            else
            {
                // Raise the PropagateAttributes event to notify dependent controls that there
                // is no active attribute (and to clear any existing mappings).
                OnPropagateAttributes(null);

                // Raise AttributesSelected to update the control's highlight.
                OnAttributesSelected((IUnknownVector)new IUnknownVectorClass(), false, false);
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
            try
            {
                base.OnUserAddedRow(e);

                // Add a new attribute for the specified row.
                ApplyAttributeToRow(e.Row.Index - 1, null, null);

                // Re-enter edit mode so that any changes to the validation list based on triggers
                // are put into effect. 
                base.EndEdit();

                // If the value of the control is a standard char that won't cause a problem with
                // SendKeys, refresh (clear) the value before restarting the edit, then resend the
                // key again after the edit has started to trigger any relavant auto-complete list.
                string value = base.CurrentCell.Value.ToString();
                if (value == null || value.Length != 1 || value[0] < '0' || value[0] > 'z' ||
                    value[0] == '^')
                {
                    value = "";
                }
                else
                {
                    base.CurrentCell.Value = "";
                }

                base.BeginEdit(false);

                if (!string.IsNullOrEmpty(value))
                {
                    SendKeys.Send(value);
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI24244", ex);
                ee.AddDebugData("Event Data", e, false);
                ee.Display();
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
                if (base.Rows.Count > 0)
                {
                    if ((e.RowIndex < base.Rows.Count && e.RowIndex > 0 && e.RowIndex == base.NewRowIndex) ||
                        (e.RowIndex == base.Rows.Count))
                    {
                        // If the last row is selected (except if there is only one row selected in a table
                        // that allows new rows), select the previous row from the row that was deleted.
                        base.ClearSelection(-1, e.RowIndex - 1, true);
                    }
                    else if (e.RowIndex < base.Rows.Count)
                    {
                        // If this is not the last row, select the same row index as was previous seleced.
                        base.ClearSelection(-1, e.RowIndex, true);
                    }
                }
                else
                {
                    // An empty vector is a cue to clear mappings and reset the selection to the first
                    // cell.
                    base.ClearAttributeMappings();
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
            try
            {
                // Default handled flag to false;
                e.Handled = false;

                // Check for Ctrl + C, Ctrl + X, or Ctrl + V
                if (e.Modifiers == Keys.Control &&
                    (e.KeyCode == Keys.C || e.KeyCode == Keys.X || e.KeyCode == Keys.V))
                {
                    // Check to see if row task options should be available based on the current
                    // table cell.
                    bool enableRowOptions = AllowRowTasks(
                        base.CurrentCell.RowIndex, base.CurrentCell.ColumnIndex);

                    switch (e.KeyCode)
                    {
                        // Copy selected rows
                        case Keys.C:
                            if (enableRowOptions)
                            {
                                CopySelectedRows();

                                e.Handled = true;
                            }
                            break;

                        // Cut selected rows
                        case Keys.X:
                            if (enableRowOptions && base.AllowUserToDeleteRows)
                            {
                                CopySelectedRows();

                                DeleteSelectedRows();

                                e.Handled = true;
                            }
                            break;

                        // Paste selected rows
                        case Keys.V:
                            if (enableRowOptions && GetDataType(Clipboard.GetDataObject()) != null)
                            {
                                PasteRowData(Clipboard.GetDataObject());

                                e.Handled = true;
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
                _suppressingMouseDown = false;
                HitTestInfo hit = base.HitTest(e.X, e.Y);

                // If row operations are supported for the current selection and shift or control
                // keys are not pressed, suppress the MouseDown event until the mouse button is
                // released to maintain the current selection for any potential drag and drop
                // operation. unless the shift or control modifiers are being used.
                if ((Control.ModifierKeys & Keys.Shift) == 0 &&
                    (Control.ModifierKeys & Keys.Control) == 0 &&
                    AllowRowTasks(hit.RowIndex, hit.ColumnIndex))
                {
                    _suppressingMouseDown = true;
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
                if (_suppressingMouseDown)
                {
                    _suppressingMouseDown = false;

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
        /// Raises the <see cref="Control.MouseMove"/> event.
        /// </summary>
        /// <param name="e">The <see cref="MouseEventArgs"/> associated with the event.</param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            try
            {
                // If a drag event is not currently in progress, the left mouse button is down and
                // row tasks are allowed given the current selection, begin a drag and drop
                // operation.
                if (!base.DragOverInProgress && (Control.MouseButtons & MouseButtons.Left) != 0)
                {
                    HitTestInfo hit = base.HitTest(e.X, e.Y);

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
                string dataType = GetDataType(drgevent.Data);

                // If the dragged data is compatible with the rows of this table.
                if (dataType != null)
                {
                    Point location = base.PointToClient(new Point(drgevent.X, drgevent.Y));
                    HitTestInfo hit = base.HitTest(location.X, location.Y);

                    // If row tasks are allowed given the current selection.
                    if (AllowRowTasks(hit.RowIndex, hit.ColumnIndex))
                    {
                        // Give focus to the table if a drop is supported.
                        if (!base.Focused)
                        {
                            base.Focus();
                        }

                        // Started with all allowed operations from the drag source.
                        drgevent.Effect = drgevent.AllowedEffect;

                        // If dragging into the same control and table
                        if (_draggedRows != null && _activeCachedRows != null &&
                            _activeCachedRows.ContainsValue(_draggedRows[0]))
                        {
                            // Don't allow rows to be dragged onto themselves.
                            if (_draggedRows.Contains((DataEntryTableRow)base.CurrentRow))
                            {
                                drgevent.Effect &= DragDropEffects.None;
                            }
                            // If the new row is selected, allow the source to be duplicated into it.
                            else if (base.CurrentRow.Index == base.NewRowIndex)
                            {
                                drgevent.Effect &= DragDropEffects.Copy;
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
                    Point location = base.PointToClient(new Point(drgevent.X, drgevent.Y));
                    HitTestInfo hit = base.HitTest(location.X, location.Y);

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
                // If this table supports the data type being dragged
                if (GetDataType(drgevent.Data) != null)
                {
                    // When dropping data into a compatible table control that allows rows to be
                    // added, insert it before the selected row, not over top of it.
                    if (base.AllowUserToAddRows)
                    { 
                        InsertNewRow();
                    }

                    // Paste the dropped data in.
                    PasteRowData(drgevent.Data);

                    base.Focus();
                }
                // If a dependent control has indicated it supports the dragged data and the new
                // row is currently selected.
                else if (drgevent.Effect != DragDropEffects.None &&
                    base.CurrentCell.RowIndex == base.NewRowIndex)
                {
                    // Create a new entry for the data that will be propagated to dependent controls.
                    InsertNewRow();

                    base.Focus();
                }

                base.OnDragDrop(drgevent);
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI26666", ex);
                ee.AddDebugData("Event Data", drgevent, false);
                ee.Display();
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
                // If a drag and drop operation is in progress, prevent the table from being
                // scrolled as the results are usually undesireable.
                if (base.DragOverInProgress)
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
                    IAttribute attributeToRemove = DataEntryTableBase.GetAttribute(e.Element);
                    if (attributeToRemove != null)
                    {
                        ExtractException.Assert("ELI26142", "Uninitialized data!",
                            _sourceAttributes != null);

                        AttributeStatusInfo.DeleteAttribute(attributeToRemove);
                        _activeCachedRows.Remove(attributeToRemove);
                    }
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
        /// <see cref="SupportsSwiping"/> and is why this property is not browseable. If this 
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
                if (!base.Enabled || _sourceAttributes == null)
                {
                    return false;
                }
                else if (base.SelectedRows.Count > 0)
                {
                    // Row selection
                    return _rowSwipingEnabled;
                }
                else if (base.SelectedCells.Count == 1 && base.CurrentCell != null)
                {
                    // Cell selection
                    return (_cellSwipingEnabled && 
                        base.Columns[base.CurrentCell.ColumnIndex] is DataEntryTableColumn);
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
                base.Enabled = (sourceAttributes != null && !base.Disabled);

                _sourceAttributes = sourceAttributes;

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
                base.Rows.Clear();

                if (_sourceAttributes == null)
                {
                    // If no data is being assigned, clear the existing attribute mappings and do not
                    // attempt to map a new attribute.
                    base.ClearAttributeMappings();
                }
                else
                {
                    // Attempt to find mapped attribute(s) from the provided vector.  
                    IUnknownVector mappedAttributes = DataEntryMethods.InitializeAttributes(
                        base.AttributeName, MultipleMatchSelectionMode.All, _sourceAttributes, null,
                        this, null, false, false, null, null, null);

                    // Create arrays to store the attributes associated with each row & cell.
                    int count = mappedAttributes.Size();

                    using (new SelectionProcessingSuppressor(this))
                    {
                        // Pre-populate the appropriate number of rows (otherwise the "new" row may not
                        // be visible)
                        if (count > 0)
                        {
                            base.Rows.Add(count);
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
                    while (base.Rows.Count < _minimumNumberOfRows)
                    {
                        int addedRowIndex = base.Rows.Add();

                        // Apply the new attribute to the added row.
                        ApplyAttributeToRow(addedRowIndex, null, null);
                    }

                    if (base.AllowUserToAddRows && base.Visible)
                    {
                        // If there is a "new" row, initialize the _tabOrderPlaceholderAttribute.
                        _tabOrderPlaceholderAttribute = DataEntryMethods.InitializeAttribute(
                            "PlaceholderAttribute_" + base.Name, MultipleMatchSelectionMode.First, true,
                            _sourceAttributes, null, this, null, true, true, null, null, null);

                        // Don't persist placeholder attributes in output.
                        AttributeStatusInfo.SetAttributeAsPersistable(
                            _tabOrderPlaceholderAttribute, false);

                        // Mark this attribute as viewable even thought it will not be used to store any
                        // useful data so that focus will be directed to it.
                        AttributeStatusInfo.MarkAsViewable(_tabOrderPlaceholderAttribute, true);
                    }
                }

                // Since the spatial information for this cell has likely changed, spatial hints need 
                // to be updated.
                base.UpdateHints();

                // Selecting all cells makes table look more "disabled".
                if (base.Disabled)
                {
                    base.SelectAll();
                }

                // Highlights the specified attributes in the image viewer and propagates the 
                // current selection to dependent controls (if appropriate)
                ProcessSelectionChange();

                this.Invalidate();
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
        /// /// <seealso cref="IDataEntryControl"/>
        public override void ProcessSwipedText(SpatialString swipedText)
        {
            try
            {
                // Swiping not supported if control isn't enabled or data isn't loaded.
                if (!base.Enabled || _sourceAttributes == null)
                {
                    return;
                }

                if (base.SelectedRows.Count > 0)
                {
                    ProcessRowSwipe(swipedText);
                }
                else if (base.SelectedCells.Count > 0)
                {
                    ProcessCellSwipe(swipedText);
                }
            }
            catch (Exception ex)
            {
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
        /// <see cref="IAttribute"/> was propagated.
        /// </param>
        public override void PropagateAttribute(IAttribute attribute, bool selectAttribute)
        {
            try
            {
                // Special handling is needed for the case where the _tabOrderPlaceholderAttribute
                // is to be propagated.
                if (attribute == _tabOrderPlaceholderAttribute)
                {
                    // If selectAttribute is specified, activate the first cell in the new row.
                    if (selectAttribute)
                    {
                        base.CurrentCell = base.Rows[base.NewRowIndex].Cells[0];

                        base.OnAttributesSelected(
                            DataEntryMethods.AttributeAsVector(_tabOrderPlaceholderAttribute),
                            false, false);
                    }
                    // Otherwise, propagate null since _tabOrderPlaceholderAttribute will
                    // never have children.
                    else
                    {
                        base.OnPropagateAttributes(null);
                    }
                }
                // If _tabOrderPlaceholderAttribute is not the attribute to propagate, allow the
                // base class to handle the propagation.
                else
                {
                    base.PropagateAttribute(attribute, selectAttribute);
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
                base.Rows.Clear();

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

                // [DataEntry:378]
                // Prevent copying and pasting table data between different documents.
                string rowDataType = GetDataType(Clipboard.GetDataObject());
                if (!string.IsNullOrEmpty(rowDataType) && rowDataType != "System.String")
                {
                    Clipboard.Clear();
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI26631", ex);
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
                if (GetDataType(e.DragDropEventArgs.Data) == this.RowDataType &&
                    (base.AllowUserToAddRows || base.Rows.Count == 0))
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
                if (GetDataType(e.Data) == this.RowDataType)
                {
                    if (base.AllowUserToAddRows)
                    {
                        // Highlight the table's new row.
                        base.ClearSelection(-1, base.NewRowIndex, true);
                        base.CurrentCell = base.Rows[base.NewRowIndex].Cells[0];

                        // Paste the dropped data in.
                        PasteRowData(e.Data);

                        base.Focus();
                    }
                    // If rows are not allowed to be added, but there is only a single attribute
                    // it seems pretty clear the that this attribute should be replaced.
                    else if (base.Rows.Count == 1)
                    {
                        // Highlight the table's one and only row.
                        base.ClearSelection(-1, 0, true);
                        base.CurrentCell = base.Rows[0].Cells[0];

                        // Paste the dropped data in.
                        PasteRowData(e.Data);

                        base.Focus();
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
        /// Handles the case the the user requested to insert a new row.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        /// <seealso cref="IDataEntryControl"/>
        void HandleInsertMenuItemClick(object sender, EventArgs e)
        {
            try
            {
                InsertNewRow();
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI24317", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the case the the user requested to delete the selected row(s).
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
        /// Handles the case the the user requested to copy the selected row(s).
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
        /// Handles the case the the user requested to cut the selected row(s).
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
        /// Handles the case the the user requested to paste the row(s) currently in the clipboard
        /// into the currently selected rows.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        void HandleRowPasteMenuItemClick(object sender, EventArgs e)
        {
            try
            {
                PasteRowData(Clipboard.GetDataObject());
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI24762", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the case the the user requested to insert the row(s) in the clipboard.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        void HandleRowInsertMenuItemClick(object sender, EventArgs e)
        {
            try
            {
                InsertNewRow();

                PasteRowData(Clipboard.GetDataObject());
            }
            catch (Exception ex)
            {
                ExtractException ee = new ExtractException("ELI24763", 
                    "Unable to insert copied rows!", ex);
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
                // Determine the location where the context menu was opened.
                Point mouseLocation = base.PointToClient(Control.MousePosition);
                HitTestInfo hit = base.HitTest(mouseLocation.X, mouseLocation.Y);

                // Check to see if row and/or row task options should be available based on
                // the current table cell.
                bool enableRowOptions = AllowRowTasks(hit.RowIndex, hit.ColumnIndex);
                bool enablePasteOptions = false;
                if (enableRowOptions && GetDataType(Clipboard.GetDataObject()) != null)
                {
                    enablePasteOptions = true;
                }

                // Enable/disable the context menu options as appropriate.
                _rowInsertMenuItem.Enabled = enableRowOptions;
                _rowDeleteMenuItem.Enabled = enableRowOptions;
                _rowCopyMenuItem.Enabled = enableRowOptions;
                _rowCutMenuItem.Enabled = enableRowOptions;
                _rowPasteMenuItem.Enabled = enablePasteOptions;
                _rowInsertCopiedMenuItem.Enabled = enablePasteOptions;
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI24319", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        #endregion Event Handlers

        #region Private Members

        /// <summary>
        /// Gets a <see langword="string"/> value to identify data on the clipboard that was copied
        /// by this control.
        /// </summary>
        /// <returns>A <see langword="string"/> value to identify data on the clipboard that was
        /// copied from this control.</returns>
        string RowDataType
        {
            get
            {
                return _OBJECT_NAME + base.AttributeName;
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
        string GetDataType(IDataObject dataObject)
        {
            if (dataObject != null)
            {
                // Check for data from this table.
                if (dataObject.GetDataPresent(this.RowDataType))
                {
                    return this.RowDataType;
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
                if (_rowFormattingRule != null && dataObject.GetDataPresent("System.String"))
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
                IDataObject rowDataObject = null;
                string rowData = GetSelectedRowData();

                // Row data is available from the current selection.
                if (!string.IsNullOrEmpty(rowData))
                {
                    // Create a dataObject containing the rows' data.
                    rowDataObject = new DataObject(this.RowDataType, rowData);
                   
                    // Maintain an internal list of the rows being dragged.
                    _draggedRows = new List<DataEntryTableRow>();
                    foreach (DataGridViewRow row in base.SelectedRows)
                    {
                        if (row.Index != base.NewRowIndex)
                        {
                            _draggedRows.Add((DataEntryTableRow)row);

                            // Update the style of each row being dragged.
                            foreach (DataGridViewCell cell in row.Cells)
                            {
                                ((IDataEntryTableCell)cell).IsBeingDragged = true;
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

                DragDropEffects allowedEffects = base.AllowUserToDeleteRows ?
                    (DragDropEffects.Move | DragDropEffects.Copy) : DragDropEffects.Copy;

                // Begin the drag-and-drop operation.
                DragDropEffects operationPerformed = base.DoDragDrop(rowDataObject, allowedEffects);

                // After the drag-and-drop has ended (whether or not the data was dropped), remove
                // the special style applied to the dragged rows.
                foreach (DataGridViewRow row in _draggedRows)
                {
                    foreach (DataGridViewCell cell in row.Cells)
                    {
                        ((IDataEntryTableCell)cell).IsBeingDragged = false;
                        UpdateCellStyle(cell);
                    }
                }

                // If the rows were moved to a different location, they need to be removed from this
                // table.
                if (_draggedRows != null && (operationPerformed & DragDropEffects.Move) != 0)
                {
                    RemoveDraggedRows();
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI26674", ex);
            }
            finally
            {
                _draggedRows = null;
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
                    DataGridViewRow[] selectedRows = new DataGridViewRow[base.SelectedRows.Count];
                    base.SelectedRows.CopyTo(selectedRows, 0);
                    List<DataGridViewRow> initialSelectedRows =
                        new List<DataGridViewRow>(selectedRows);

                    // Remove each row
                    foreach (DataEntryTableRow draggedRow in _draggedRows)
                    {
                        // If the dragged rows are in the table currently displayed, simply
                        // removing the row will trigger all necessary actions.
                        if (cachedSet == _activeCachedRows)
                        {
                            base.Rows.Remove(draggedRow);
                        }
                        // If the rows are from a table not currently displayed,
                        // DeleteAttribute needs to be called and the cached row set needs
                        // to be updated.
                        else
                        {
                            AttributeStatusInfo.DeleteAttribute(draggedRow.Attribute);
                            DataEntryTableRow row = cachedSet[draggedRow.Attribute];
                            cachedSet.Remove(draggedRow.Attribute);
                            row.Dispose();
                        }
                    }

                    // Restore the initial row selection as long as the selection does not
                    // include the source drag rows.
                    if (cachedSet == _activeCachedRows && initialSelectedRows.Count > 0)
                    {
                        using (new SelectionProcessingSuppressor(this))
                        {
                            DataGridViewRow lastSelectedRow = null;

                            base.ClearSelection();
                            foreach (DataGridViewRow row in initialSelectedRows)
                            {
                                if (!_draggedRows.Contains(row as DataEntryTableRow))
                                {
                                    row.Selected = true;
                                    lastSelectedRow = row;
                                }
                            }

                            if (lastSelectedRow != null)
                            {
                                base.CurrentCell = lastSelectedRow.Cells[0];
                            }
                        }
                    }

                    break;
                }
            }
        }

        /// <summary>
        /// Gets a lazily instantiate <see cref="MiscUtils"/> instance to use for converting 
        /// <see cref="IPersistStream"/> implementations to/from a stringized byte stream.
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
                attribute = (IAttribute)new AttributeClass();
                attribute.Name = base.AttributeName;
                AttributeStatusInfo.Initialize(
                    attribute, _sourceAttributes, this, null, false, false, null, null, null);
                newAttributeCreated = true;
            }

            // Keep track of any attribute we are replacing.
            IAttribute attributeToReplace = DataEntryTableBase.GetAttribute(base.Rows[rowIndex]);

            using (new SelectionProcessingSuppressor(this))
            {
                // Add a new row to the table if necessary
                if (rowIndex == base.Rows.Count || base.Rows[rowIndex].IsNewRow)
                {
                    base.Rows.Add();
                }
            }

            // If a row has already been cached for the attribute, simply swap out the current row
            // for the cached row and re-map all attributes in the row.
            DataEntryTableRow cachedRow = null;
            if (_activeCachedRows.TryGetValue(attribute, out cachedRow))
            {
                base.Rows.RemoveAt(rowIndex);
                base.Rows.Insert(rowIndex, cachedRow);

                // Start by mapping the attribute to the row itself.
                base.MapAttribute(attribute, base.Rows[rowIndex]);

                // Remap each cell's attribute in the row (may cause the parent row's attribute to
                // be remapped).
                foreach (DataGridViewCell cell in cachedRow.Cells)
                {
                    IDataEntryTableCell dataEntryCell = cell as IDataEntryTableCell;
                    if (dataEntryCell != null)
                    {
                        base.MapAttribute(dataEntryCell.Attribute, dataEntryCell);
                    }
                }
            }
            // If a cached row didn't exist for the incoming attribute, initialize it.
            else
            {
                // Map the attribute to the row itself.
                ((DataEntryTableRow)base.Rows[rowIndex]).Attribute = attribute;
                base.MapAttribute(attribute, base.Rows[rowIndex]);

                bool parentAttributeIsMapped = false;

                // Loop through each column to populate the values.
                foreach (DataGridViewColumn column in base.Columns)
                {
                    IAttribute subAttribute = null;
                    DataEntryTableColumn dataEntryTableColumn = column as DataEntryTableColumn;

                    // If the column is a data entry column, we need to populate it as appropriate.
                    if (dataEntryTableColumn != null)
                    {
                        IDataEntryTableCell dataEntryCell = (IDataEntryTableCell)
                            base.Rows[rowIndex].Cells[column.Index];

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
                                column.Index, false, dataEntryTableColumn.TabStopRequired,
                                dataEntryCell.Validator, dataEntryTableColumn.AutoUpdateQuery,
                                dataEntryTableColumn.ValidationQuery);
                        }
                        else
                        {
                            // Select the appropriate subattribute to use and create an new (empty)
                            // attribute if no such attribute can be found.
                            subAttribute = DataEntryMethods.InitializeAttribute(
                                dataEntryTableColumn.AttributeName,
                                dataEntryTableColumn.MultipleMatchSelectionMode,
                                true, attribute.SubAttributes, null, this, column.Index, true,
                                dataEntryTableColumn.TabStopRequired, dataEntryCell.Validator,
                                dataEntryTableColumn.AutoUpdateQuery,
                                dataEntryTableColumn.ValidationQuery);
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
                        base.MapAttribute(subAttribute, dataEntryCell);

                        // Apply the subAttribute as the cell's value.
                        base.Rows[rowIndex].Cells[column.Index].Value = subAttribute;
                    }
                }

                // If the value of the row's attribute itself is not displayed in one of the columns,
                // consider the row's attribute as valid and viewed.
                if (!parentAttributeIsMapped)
                {
                    // It is important to call initialize here to ensure AttributeStatusInfo
                    // raises the AttributeInitialized event for all attributes mapped into the
                    // table.
                    AttributeStatusInfo.Initialize(
                        attribute, _sourceAttributes, this, 0, false, false, null, null, null);
                }

                // Swap out the existing attribute in the overall attribute heirarchy (either
                // keeping attribute ordering the same as it was or explicitly inserting it at the
                // specified position).
                if (DataEntryMethods.InsertOrReplaceAttribute(_sourceAttributes, attribute,
                    attributeToReplace, insertBeforeAttribute))
                {
                    AttributeStatusInfo.DeleteAttribute(attributeToReplace);
                    _activeCachedRows.Remove(attributeToReplace);
                }

                // If this control does not have any dependent controls, consider each row
                // propagated.
                if (!base.HasDependentControls)
                {
                    AttributeStatusInfo.MarkAsPropagated(attribute, true, true);
                }

                // If a new attribute was created for this row, ensure that it propagates all its
                // sub-attributes to keep all status info's in the attribute heirarchy up-to-date.
                if (newAttributeCreated)
                {
                    // If _tabOrderPlaceholderAttribute is being used, make sure it remains the last
                    // attribute in _sourceAttributes.
                    if (_tabOrderPlaceholderAttribute != null)
                    {
                        _sourceAttributes.RemoveValue(_tabOrderPlaceholderAttribute);
                        _sourceAttributes.PushBack(_tabOrderPlaceholderAttribute);
                    }

                    ProcessSelectionChange();
                }

                // Cache the initialized attribute's row for later use.
                _activeCachedRows[attribute] = (DataEntryTableRow)base.Rows[rowIndex];
            }
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
        void ProcessRowSwipe(SpatialString swipedText)
        {
            // Row selecton mode. The swipe can only be applied via the results of
            // a row formatting rule.
            IUnknownVector formattedData = DataEntryMethods.RunFormattingRule(_rowFormattingRule,
                swipedText);

            // Find all attributes which apply to this control.
            IUnknownVector formattedAttributes = DataEntryMethods.InitializeAttributes(
                base.AttributeName, MultipleMatchSelectionMode.All, formattedData, null, this, null,
                false, false, null, null, null);

            // Apply the found attributes into the currently selected rows of the table.
            ApplyAttributesToSelectedRows(formattedAttributes);
        }

        /// <summary>
        /// Process swiped text as input into the currently selected cell.
        /// <para><b>Requirements:</b></para>
        /// One (and only one) cell may be selected.
        /// </summary>
        /// <param name="swipedText">The OCR'd text from the image swipe.</param>
        /// <throws><see cref="ExtractException"/> if more than one cell is selected.</throws>
        void ProcessCellSwipe(SpatialString swipedText)
        {
            ExtractException.Assert("ELI24239",
                        "Cell swiping is supported only for one cell at a time!", 
                        base.SelectedCells.Count == 1);

            // Obtain the row and column where the swipe occured. (One or both may not
            // apply depending on the selection type).
            int rowIndex = base.CurrentCell.RowIndex;
            int columnIndex = base.CurrentCell.ColumnIndex;

            // Cell selecton mode. The swipe can be applied either via the results of a
            // column formatting rule or the swiped text value can be applied directly to
            // the cell's mapped attribute.
            DataEntryTableColumn dataEntryColumn = (DataEntryTableColumn)base.Columns[columnIndex];

            // Process the swiped text with a formatting rule (if available).
            if (dataEntryColumn.FormattingRule != null)
            {
                IUnknownVector formattedData =
                    DataEntryMethods.RunFormattingRule(dataEntryColumn.FormattingRule, swipedText);

                // Determine the attribute name to use to find the result
                string attributeName = (dataEntryColumn.AttributeName == ".") 
                    ? base.AttributeName : dataEntryColumn.AttributeName;

                IAttribute attribute = DataEntryMethods.InitializeAttribute(attributeName,
                    dataEntryColumn.MultipleMatchSelectionMode, false, formattedData, null, this,
                    null, true, false, null, null, null);

                // Use the value of the found attribute only if the found attribute has a non-empty
                // value.
                if (attribute != null && !string.IsNullOrEmpty(attribute.Value.String))
                {
                    swipedText = attribute.Value;
                }
            }

            // If swiping into the "new" row, create & map a new attribute as necessary.
            if (base.CurrentRow.IsNewRow)
            {
                // Cell selection will switch to the new "new" row, and then will be manually
                // changed back... don't process the SelectionChanged events that occur as a result.
                using (new SelectionProcessingSuppressor(this))
                {
                    ApplyAttributeToRow(rowIndex, null, null);

                    // [DataEntry:288] Keep cell selection on the cell that was swiped.
                    base.CurrentCell = base.Rows[rowIndex].Cells[columnIndex];
                }  
            }

            // Apply the new value directly to the mapped attribute (Don't replace the entire 
            // attribute).
            base.Rows[rowIndex].Cells[columnIndex].Value = swipedText;

            // Since the spatial information for this cell has changed, spatial hints need to be
            // updated.
            base.UpdateHints();

            // Raise AttributesSelected to update the control's highlight.
            OnAttributesSelected(
                DataEntryMethods.AttributeAsVector(
                    DataEntryTableBase.GetAttribute(base.CurrentCell)), false, true);
        }

        /// <summary>
        /// Inserts a new row into the table before the current row.
        /// </summary>
        void InsertNewRow()
        {
            if (_sourceAttributes != null)
            {
                // Obtain the current row and its associated attribute.
                int rowIndex = base.CurrentRow.Index;
                IAttribute insertBeforeAttribute = DataEntryTableBase.GetAttribute(base.CurrentRow);

                // Insert the new row and adjust the existing attribute mappings accordingly.
                base.Rows.Insert(rowIndex, 1);

                // Apply the new attribute to the inserted row.
                ApplyAttributeToRow(rowIndex, null, insertBeforeAttribute);

                // Highlight the newly inserted row.
                base.ClearSelection(-1, rowIndex, true);
                base.CurrentCell = base.Rows[rowIndex].Cells[0];
            }
        }

        /// <summary>
        /// Deletes the currently selected row(s).
        /// </summary>
        void DeleteSelectedRows()
        {
            // Delete each row in the current selection.
            foreach (DataGridViewRow row in base.SelectedRows)
            {
                if (!row.IsNewRow)
                {
                    base.Rows.RemoveAt(row.Index);
                }
            }
        }

        /// <summary>
        /// Copies the currently selected row(s) to the clipboard
        /// </summary>
        void CopySelectedRows()
        {
            string rowData = GetSelectedRowData();

            // If at least one row was copied, add the copied attributes to the internal
            // "clipboard".
            if (rowData != null)
            {
                // Add the copied attributes to the clipboard
                Clipboard.SetData(this.RowDataType, rowData);
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
                IUnknownVector copiedAttributes = (IUnknownVector)new IUnknownVectorClass();

                // Get a sorted (top to bottom) list of currently selected rows.
                DataGridViewRow[] selectedRows = new DataGridViewRow[base.SelectedRows.Count];
                base.SelectedRows.CopyTo(selectedRows, 0);
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
                    return this.MiscUtils.GetObjectAsStringizedByteStream(copiedAttributes);
                }
            }

            return null;
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
        void PasteRowData(IDataObject dataObject)
        {
            string dataType = GetDataType(dataObject);

            // If the data on the clipboard is a string, use the row formatting rule to parse the
            // text into the row.
            if (_rowFormattingRule != null && dataType == "System.String")
            {
                SpatialString spatialString = (SpatialString)new SpatialStringClass();
                spatialString.CreateNonSpatialString((string)dataObject.GetData(dataType), "");

                ProcessRowSwipe(spatialString);
            }
            // Otherwise the data is attributes from this table or a compatible one.
            else if (!string.IsNullOrEmpty(dataType))
            {
                // Retrieve the data from the clipboard and convert it back into an attribute vector.
                string stringizedAttributes = (string)dataObject.GetData(dataType);

                IUnknownVector attributesToPaste = (IUnknownVector)
                    this.MiscUtils.GetObjectFromStringizedByteStream(stringizedAttributes);

                int count = attributesToPaste.Size();
                for (int i = 0; i < count; i++)
                {
                    IAttribute attribute = (IAttribute)attributesToPaste.At(i);

                    // If the attributes are not from this table, rename them.
                    if (dataType != this.RowDataType)
                    {
                        attribute.Name = base.AttributeName;
                    }
                }

                // Apply the attributes into the currently selected rows of the table.
                ApplyAttributesToSelectedRows(attributesToPaste);
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

                // Temporarily suppress selection changes as several will occur programatically in
                // in the course of populating the table. In addition to making the code more
                // efficient, this insures hints are updated before the control host processes the
                // selection change and re-draws the highlights.
                using (new SelectionProcessingSuppressor(this))
                {
                    // Get a sorted (top to bottom) list of currently selected rows.
                    DataGridViewRow[] selectedRows = new DataGridViewRow[base.SelectedRows.Count];
                    base.SelectedRows.CopyTo(selectedRows, 0);
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
                            if (base.AllowUserToAddRows == false)
                            {
                                break;
                            }

                            // Add a new row for the next attribute and add it to the
                            // destinationRows list.
                            int nextIndex = destinationRows[destinationRows.Count - 1].Index + 1;
                            insertBeforeAttribute =
                                DataEntryTableBase.GetAttribute(base.Rows[nextIndex]);
                            base.Rows.Insert(nextIndex, 1);
                            destinationRows.Add(base.Rows[nextIndex]);
                        }

                        IAttribute attribute = (IAttribute)attributes.At(rowsAdded);

                        // [DataEntry:426]
                        // Ensure the parent attribute is initialized before applying the attribute
                        // to the row, otherwise the parentAttribute property will not be set
                        // correctly for column attributes. 
                        if (AttributeStatusInfo.GetParentAttribute(attribute) == null)
                        {
                            AttributeStatusInfo.Initialize(attribute, _sourceAttributes, this, null,
                                false, false, null, null, null);
                        }

                        ApplyAttributeToRow(destinationRows[rowsAdded].Index, attribute,
                            insertBeforeAttribute);

                        // If the attribute was added into the "new" row, the current entry in 
                        // destinationRows will now represent the new "new" row.  Therefore, adjust
                        // the entry to reflect previous row which is the one that was just added.
                        if (destinationRows[rowsAdded].IsNewRow)
                        {
                            destinationRows[rowsAdded] = base.Rows[base.NewRowIndex - 1];
                        }
                    }

                    // If deletions are allowed, delete all selected rows that were not replaced with
                    // attribute from the swiped text.
                    if (base.AllowUserToDeleteRows)
                    {
                        while (rowsAdded < destinationRows.Count)
                        {
                            if (destinationRows[rowsAdded].Index != base.NewRowIndex)
                            {
                                base.Rows.RemoveAt(destinationRows[rowsAdded].Index);
                            }

                            destinationRows.RemoveAt(rowsAdded);
                        }
                    }

                    // Adjust the current selection to encapsulate all rows populated via swiped text
                    // with the current row being the last row populated.
                    base.ClearSelection(-1, destinationRows[destinationRows.Count - 1].Index, true);
                    base.CurrentCell = destinationRows[destinationRows.Count - 1].Cells[0];
                    foreach (DataGridViewRow row in destinationRows)
                    {
                        row.Selected = true;
                    }

                    // Since the spatial information for this cell has likely changed, spatial hints
                    // need to be updated.
                    base.UpdateHints();

                    // If _tabOrderPlaceholderAttribute is being used, make sure it remains the last
                    // attribute in _sourceAttributes.
                    if (_tabOrderPlaceholderAttribute != null)
                    {
                        _sourceAttributes.RemoveValue(_tabOrderPlaceholderAttribute);
                        _sourceAttributes.PushBack(_tabOrderPlaceholderAttribute);
                    }
                }

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
        /// Checks to see if row and/or row paste tasks should be avaliable based on an event
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

            // Determine if row-level options are to be available based on the clicked location
            // and the current selection.
            bool enableRowOptions = (originRowIndex >= 0 &&
                                     (originColumnIndex == -1 || base.Rows[originRowIndex].Selected));

            if (!enableRowOptions && originRowIndex >= 0 && originColumnIndex >= 0)
            {
                // If the menu was opened via a data entry cell, check to see if the clicked
                // cell is currently selected.  If not, select this cell instead to prevent
                // confusion about which cell(s) will be acted upon.
                DataGridViewCell clickedCell = base.Rows[originRowIndex].Cells[originColumnIndex];

                if (!clickedCell.Selected)
                {
                    base.CurrentCell = clickedCell;
                }
            }
            else if (originRowIndex >= 0)
            {
                // If the menu was opened via a row header, checked to see if the menu was
                // opened within a currently selected row.
                DataGridViewRow clickedRow = base.Rows[originRowIndex];
                DataGridViewRow[] selectedRows = null;
                if (clickedRow.Selected && base.CurrentRow != clickedRow)
                {
                    // If the clicked row was selected, but not the "current" row, we want to make
                    // it the current row so it is clear which row is being used as the basis for
                    // for a row insertion operation.
                    selectedRows = new DataGridViewRow[base.SelectedRows.Count];
                    base.SelectedRows.CopyTo(selectedRows, 0);

                    // Set the current cell to the first cell in the clicked row
                    base.CurrentCell = clickedRow.Cells[0];

                    // Changing the current cell will have cleared the previously selected rows.
                    // Re-select them now.
                    if (selectedRows != null)
                    {
                        foreach (DataGridViewRow row in selectedRows)
                        {
                            row.Selected = true;
                        }
                    }
                }
                else if (base.CurrentRow != clickedRow)
                {
                    base.ClearSelection(-1, originRowIndex, true);

                    // If the clicked row is not the current row, make the click row current
                    // and selected.
                    base.CurrentCell = clickedRow.Cells[0];
                }
                else if (!base.CurrentRow.Selected)
                {
                    base.ClearSelection(-1, originRowIndex, true);
                }
            }

            return enableRowOptions;
        }

        #endregion Private Members
    }
}
