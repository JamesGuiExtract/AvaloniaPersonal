using Extract.Interop;
using Extract.Licensing;
using System;
using System.ComponentModel;
using System.Collections.Generic;
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
        private static readonly string _OBJECT_NAME = typeof(DataEntryTable).ToString();

        #endregion Constants

        #region Fields

        /// <summary>
        /// Indicates whether swiping should be allowed when an individual cell is selected.
        /// </summary>
        private bool _cellSwipingEnabled;

        /// <summary>
        /// Indicates whether swiping should be allowed when a complete row is selected.
        /// </summary>
        private bool _rowSwipingEnabled;

        /// <summary>
        /// The filename of the rule file to be used to parse swiped data into rows.
        /// </summary>
        private string _rowFormattingRuleFileName;

        /// <summary>
        /// The formatting rule to be used when processing text from imaging swipes for a row at a 
        /// time.
        /// </summary>
        private IRuleSet _rowFormattingRule;

        /// <summary>
        /// Specifies the minimum number of rows a DataEntryTable must have.  If the specified
        /// number of attributes are not found, new, blank ones are created as necessary.
        /// </summary>
        private int _minimumNumberOfRows;

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
        private IUnknownVector _sourceAttributes;

        /// <summary>
        /// An attribute used to direct focus to the first cell of the "new" row of the table before
        /// focus leaves the control.  This attribute will not store any data useful as output.
        /// </summary>
        private IAttribute _tabOrderPlaceholderAttribute;

        /// <summary>
        /// A set of attributes mapped to row(s) in the table which have been copied.
        /// </summary>
        private IUnknownVector _copiedRowAttributes;

        /// <summary>
        /// Specifies whether the current instance is running in design mode.
        /// </summary>
        private bool _inDesignMode;

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

        #endregion Properties

        #region Overrides

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
                            if (enableRowOptions && (_copiedRowAttributes != null))
                            {
                                PasteCopiedRows();

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

        #endregion Overrides

        #region Event Handlers

        /// <summary>
        /// Handles the <see cref="DataGridViewRowCollection.CollectionChanged"/> event in order to
        /// remove from _sourceAttributes any attributes that have been deleted from the table.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">An <see cref="CollectionChangeEventArgs"/> that contains the event data.
        /// </param>
        private void HandleRowsCollectionChanged(object sender, CollectionChangeEventArgs e)
        {
            try
            {
                if (e.Action == CollectionChangeAction.Remove)
                {
                    // Remove the row's attribute from _sourceAttributes and raise the 
                    // AttributesDeleted event.
                    // IMPORTANT: Remove the attribute from _sourceAttributes before calling 
                    // DeleteAttribute since the control host may attempt to verify the attribute
                    // is missing (as part of a check invalid or unviewed items)
                    IAttribute attributeToRemove = DataEntryTableBase.GetAttribute(e.Element);
                    if (attributeToRemove != null)
                    {
                        ExtractException.Assert("ELI26142", "Uninitialized data!",
                            _sourceAttributes != null);

                        _sourceAttributes.RemoveValue(attributeToRemove);
                        AttributeStatusInfo.DeleteAttribute(attributeToRemove);
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
                base.Enabled = (sourceAttributes != null);

                _sourceAttributes = sourceAttributes;

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

        #endregion IDataEntryControl Methods

        #region Event Handlers

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
                PasteCopiedRows();
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

                PasteCopiedRows();
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
        private void HandleContextMenuOpening(object sender, CancelEventArgs e)
        {
            try
            {
                // Determine the location where the context menu was opened.
                Point mouseLocation = base.PointToClient(Control.MousePosition);
                HitTestInfo hit = base.HitTest(mouseLocation.X, mouseLocation.Y);

                // Check to see if row and/or row task options should be available based on
                // the current table cell.
                bool enableRowOptions = AllowRowTasks(hit.RowIndex, hit.ColumnIndex);

                // Enable/disable the context menu options as appropriate.
                _rowInsertMenuItem.Enabled = enableRowOptions;
                _rowDeleteMenuItem.Enabled = enableRowOptions;
                _rowCopyMenuItem.Enabled = enableRowOptions;
                _rowCutMenuItem.Enabled = enableRowOptions;
                _rowPasteMenuItem.Enabled = enableRowOptions && (_copiedRowAttributes != null);
                _rowInsertCopiedMenuItem.Enabled = 
                    enableRowOptions && (_copiedRowAttributes != null);
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
        /// Gets a <see langword="string"/> value to identify data on the clipboard that can be 
        /// used by this table. Data will only be able to be shared between the same table as 
        /// identified by control name and attribute name.
        /// </summary>
        /// <returns>A <see langword="string"/> value to identify data on the clipboard that can be
        /// used by this table.</returns>
        private string ClipboardDataType
        {
            get
            {
                return _OBJECT_NAME + base.Name + base.AttributeName;
            }
        }

        /// <summary>
        /// Gets a lazily instantiate <see cref="MiscUtils"/> instance to use for converting 
        /// <see cref="IPersistStream"/> implementations to/from a stringized byte stream.
        /// </summary>
        /// <returns>A <see cref="MiscUtils"/> instance.</returns>
        private MiscUtils MiscUtils
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
        private static int CompareRowsByIndex(DataGridViewRow row1, DataGridViewRow row2)
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
        private void ApplyAttributeToRow(int rowIndex, IAttribute attribute,
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
                    attribute, _sourceAttributes, this, 0, false, false, null, null, null);
                newAttributeCreated = true;
            }

            // Swap out the existing attribute in the overall attribute heirarchy (either keeping 
            // attribute ordering the same as it was or explicitly inserting it at the specified
            // position).
            IAttribute attributeToReplace = DataEntryTableBase.GetAttribute(base.Rows[rowIndex]);
            if (DataEntryMethods.InsertOrReplaceAttribute(_sourceAttributes, attribute,
                attributeToReplace, insertBeforeAttribute))
            {
                AttributeStatusInfo.DeleteAttribute(attributeToReplace);
            }

            using (new SelectionProcessingSuppressor(this))
            {
                // Add a new row to the table if necessary
                if (rowIndex == base.Rows.Count || base.Rows[rowIndex].IsNewRow)
                {
                    base.Rows.Add();
                }
            }

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
                        // raises the AttributeInitialized event for all attributes mapped into the
                        // table.
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
                            dataEntryTableColumn.AutoUpdateQuery, dataEntryTableColumn.ValidationQuery);
                    }

                    // If the attribute being applied is a hint, it has been copied from elsewhere
                    // and the hint shouldn't apply here-- remove the hint.
                    if (AttributeStatusInfo.GetHintType(subAttribute) != HintType.None)
                    {
                        AttributeStatusInfo.SetHintType(subAttribute, HintType.None);
                        AttributeStatusInfo.SetHintRasterZones(subAttribute, null);
                    }

                    // Map the column's attribute and set the attribute's validator. NOTE: This may 
                    // be remapping the row's attribute; this is intentional so that 
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

            // If this control does not have any dependent controls, consider each row propagated.
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
        private void ProcessRowSwipe(SpatialString swipedText)
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
        private void ProcessCellSwipe(SpatialString swipedText)
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
                if (!string.IsNullOrEmpty(attribute.Value.String))
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
        private void InsertNewRow()
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
        private void DeleteSelectedRows()
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
        private void CopySelectedRows()
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

                // If at least one row was copied, add the copied attributes to the internal
                // "clipboard".
                if (copiedAttributes.Size() > 0)
                {
                    _copiedRowAttributes = copiedAttributes;
                }
            }
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
        private void PasteCopiedRows()
        {
            if (_sourceAttributes != null && _copiedRowAttributes != null)
            {
                IUnknownVector attributesToPaste = (IUnknownVector)new IUnknownVectorClass();

                // The AttributeStatusInfo for each attribute needs to be re-initialized
                // to set the owning control (which isn't persisted).
                int count = _copiedRowAttributes.Size();
                for (int i = 0; i < count; i++)
                {
                    // Clone the attribute (the original may still exist elsewhere)
                    ICopyableObject copyableAttribute = (ICopyableObject)_copiedRowAttributes.At(i);
                    attributesToPaste.PushBack(copyableAttribute.Clone());
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
        private void ApplyAttributesToSelectedRows(IUnknownVector attributes)
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

                        ApplyAttributeToRow(destinationRows[rowsAdded].Index,
                            (IAttribute)attributes.At(rowsAdded), insertBeforeAttribute);

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
        private bool AllowRowTasks(int originRowIndex, int originColumnIndex)
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
