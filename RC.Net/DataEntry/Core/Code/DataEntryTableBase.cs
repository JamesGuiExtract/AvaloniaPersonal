using Extract.Drawing;
using Extract.Licensing;
using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Security.Permissions;
using System.Text;
using System.Windows.Forms;
using UCLID_AFCORELib;
using UCLID_COMUTILSLib;
using UCLID_RASTERANDOCRMGMTLib;

namespace Extract.DataEntry
{
    /// <summary>
    /// A base of common code needed by any <see cref="IDataEntryControl"/> that extends
    /// <see cref="DataGridView"/>.
    /// </summary>
    public abstract class DataEntryTableBase : DataGridView, IDataEntryControl, IDisposable
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        private static readonly string _OBJECT_NAME = typeof(DataEntryTableBase).ToString();

        /// <summary>
        /// The color that should be used to indicate table's current selection when the table is
        /// not the active data control.</summary>
        private static readonly Color _INACTIVE_SELECTION_COLOR = Color.LightGray;

        /// <summary>
        /// The value associated with a window's key down message.
        /// </summary>
        private const int _WM_KEYDOWN = 0x100;

        #endregion Constants

        #region Fields

        /// <summary>
        /// The name used to identify the <see cref="IAttribute"/> to be associated with the table.
        /// </summary>
        private string _attributeName;

        /// <summary>
        /// Used to specify the data entry control which is mapped to the parent of the attribute(s) 
        /// to which the current table is to be mapped.
        /// </summary>
        private IDataEntryControl _parentDataEntryControl;

        /// <summary>
        /// Specifies the color which should be used to indicate active status.
        /// </summary>
        private Color _color;

        /// <summary>
        /// Specifies whether the table will attempt to generate a hint using the intersection of 
        /// the row and column occupied by the specified attribute.
        /// </summary>
        private bool _smartHintsEnabled = true;

        /// <summary>
        /// Specifies whether the table will attempt to generate a hint by indicating the other
        /// attributes sharing the same row.
        /// </summary>
        private bool _rowHintsEnabled = true;

        /// <summary>
        /// Specifies whether the table will attempt to generate a hint by indicating the other 
        /// attributes sharing the same column.
        /// </summary>
        private bool _columnHintsEnabled = true;

        /// <summary>
        /// Used to generate "smart" hints for attributes missing spatial info.
        /// </summary>
        private SpatialHintGenerator _spatialHintGenerator = new SpatialHintGenerator();

        /// <summary>
        /// Indicates whether the spatial information has changed such that hints need to be
        /// recalculated.
        /// </summary>
        private bool _hintsAreDirty;

        /// <summary>
        /// Indicates whether the table is currently active.
        /// </summary>
        private bool _isActive;

        /// <summary>
        /// Keeps track of which table element is mapped to each attribute represented in the table.
        /// </summary>
        private Dictionary<IAttribute, object> _attributeMap =
            new Dictionary<IAttribute, object>();

        /// <summary>
        /// The attribute that is currently propagated by this table (if any)
        /// </summary>
        private IAttribute _currentlyPropagatedAttribute;

        /// <summary>
        /// A reference count of the methods that have requested to temporarily prevent processing
        /// of selection changes so that one or more programatic selection changes can be made 
        /// before the table is in its intended state and ready to process selection events.
        /// </summary>
        private int _suppressSelectionProcessingReferenceCount;

        /// <summary>
        /// The control currently being used to update the data in the active cell.
        /// </summary>
        private Control _editingControl;

        /// <summary>
        /// A regular style font to indicate viewed fields.
        /// </summary>
        private Font _regularFont;

        /// <summary>
        /// A bold style font to indicate unviewed fields.
        /// </summary>
        private Font _boldFont;

        /// <summary>
        /// The style to use for cells currently being edited.
        /// </summary>
        private DataGridViewCellStyle _editModeCellStyle;

        /// <summary>
        /// The style to use for selected cells whose fields have been viewed in the active table.
        /// </summary>
        private DataGridViewCellStyle _regularActiveCellStyle;

        /// <summary>
        /// The style to use for selected cells whose fields have been viewed and are not in the
        /// active table.
        /// </summary>
        private DataGridViewCellStyle _regularInactiveCellStyle;

        /// <summary>
        /// The style to use for selected cells whose fields have not been viewed in the active table.
        /// </summary>
        private DataGridViewCellStyle _boldActiveCellStyle;

        /// <summary>
        /// The style to use for selected cells whose fields have not been viewed and are not in the
        /// active table.
        /// </summary>
        private DataGridViewCellStyle _boldInactiveCellStyle;

        #endregion Fields

        #region Delegates

        /// <summary>
        /// Signature to use for invoking methods that accept one <see cref="EventArgs"/> parameter.
        /// </summary>
        /// <param name="e">An <see cref="EventArgs"/> parameter.</param>
        private delegate void EventArgsDelegate(EventArgs e);

        #endregion

        #region SelectionProcessingSuppressor

        /// <summary>
        /// A class to manage requests to suppress processing of the 
        /// <see cref="DataGridView.SelectionChanged"/> event.
        /// </summary>
        protected class SelectionProcessingSuppressor : IDisposable
        {
            /// <summary>
            /// The <see cref="DataEntryTable"/> whose SelectionChanged handling is to be suppressed.
            /// </summary>
            private DataEntryTableBase _dataEntryTable;

            /// <summary>
            /// Indicates whether or not the object instance has been disposed.
            /// </summary>
            private bool _disposed;

            /// <summary>
            /// Initializes a new <see cref="SelectionProcessingSuppressor"/> instance.
            /// </summary>
            /// <param name="dataEntryTable">The <see cref="DataEntryTable"/> whose SelectionChanged
            /// handling is to be suppressed. SelectionChanged event handling will continue to be
            /// suppressed until this instance is disposed of and there are no other instances of
            /// <see cref="SelectionProcessingSuppressor"/> currently active.</param>
            public SelectionProcessingSuppressor(DataEntryTableBase dataEntryTable)
            {
                try
                {
                    ExtractException.Assert("ELI25622", "Null argument exception!", 
                        dataEntryTable != null);

                    _dataEntryTable = dataEntryTable;
                    _dataEntryTable._suppressSelectionProcessingReferenceCount++;
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI25621", ex);
                }
            }

            /// <summary>
            /// Relinquishes suppression of <see cref="DataGridView.SelectionChanged"/> event
            /// handling.
            /// </summary>
            public void Dispose()
            {
                try
                {
                    Dispose(true);
                    GC.SuppressFinalize(this);
                }
                catch (Exception ex)
                {
                    ExtractException.Display("ELI25623", ex);
                }
            }

            /// <overloads>Relinquishes suppression of <see cref="DataGridView.SelectionChanged"/>
            /// event.</overloads>
            /// <summary>
            /// Relinquishes suppression of <see cref="DataGridView.SelectionChanged"/> event.
            /// </summary>
            /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged 
            /// resources; <see langword="false"/> to release only unmanaged resources.</param>        
            protected virtual void Dispose(bool disposing)
            {
                if (disposing && !_disposed)
                {
                    // Dispose of managed objects
                    _dataEntryTable._suppressSelectionProcessingReferenceCount--;
                    _disposed = true;
                }

                // Dispose of unmanaged resources
            }
        }

        #endregion SelectionProcessingSuppressor

        #region Constructors

        /// <summary>
        /// Initializes <see cref="DataEntryTableBase"/> instance as part of a derived class.
        /// </summary>
        protected DataEntryTableBase()
            : base()
        {
            try
            {
                // Load licenses in design mode
                if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
                {
                    // Load the license files from folder
                    LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());
                }

                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.FlexIndexCoreObjects, "ELI24495",
                    _OBJECT_NAME);

                // Initialize the various cell styles (modifying existing cell styles on-the-fly
                // causes poor performance. Microsoft recommends sharing DataGridViewCellStyle
                // instances as much as possible.

                // Initialize the fonts used by the DataGridViewCellStyle objects.
                _regularFont = new Font(base.DefaultCellStyle.Font, FontStyle.Regular);
                _boldFont = new Font(base.DefaultCellStyle.Font, FontStyle.Bold);

                // The style to use for cells currently being edited.
                _editModeCellStyle = new DataGridViewCellStyle(base.DefaultCellStyle);
                _editModeCellStyle.Font = _regularFont;
                _editModeCellStyle.SelectionForeColor = Color.Black;

                // The style to use for selected cells whose fields have been viewed in the active 
                // table.
                _regularActiveCellStyle = new DataGridViewCellStyle(base.DefaultCellStyle);
                _regularActiveCellStyle.Font = _regularFont;
                _regularActiveCellStyle.SelectionForeColor = Color.Black;

                // The style to use for selected cells whose fields have been viewed and are not
                // in the active table. A data entry table is going to distinguish between 
                // "selected-and-active" and "selected-but-inactive".  Initialize the selection 
                // color for "inactive" with a more subtle color than the default (blue).
                _regularInactiveCellStyle = new DataGridViewCellStyle(base.DefaultCellStyle);
                _regularInactiveCellStyle.Font = _regularFont;
                _regularInactiveCellStyle.SelectionForeColor = Color.Black;
                _regularInactiveCellStyle.SelectionBackColor = _INACTIVE_SELECTION_COLOR;

                // The style to use for selected cells whose fields have not been viewed in the
                // active table.
                _boldActiveCellStyle = new DataGridViewCellStyle(base.DefaultCellStyle);
                _boldActiveCellStyle.Font = _boldFont;
                _boldActiveCellStyle.SelectionForeColor = Color.Black;

                // The style to use for selected cells whose fields have not been viewed and are not
                // in the active table. A data entry table is going to distinguish between 
                // "selected-and-active" and "selected-but-inactive".  Initialize the selection
                // color for "inactive" with a more subtle color than the default (blue).
                _boldInactiveCellStyle = new DataGridViewCellStyle(base.DefaultCellStyle);
                _boldInactiveCellStyle.Font = _boldFont;
                _boldInactiveCellStyle.SelectionForeColor = Color.Black;
                _boldInactiveCellStyle.SelectionBackColor = _INACTIVE_SELECTION_COLOR;

                // Use a DataEntryTableRow instance as the row template.
                base.RowTemplate = new DataEntryTableRow();

                base.EditMode = DataGridViewEditMode.EditOnKeystrokeOrF2;

                base.ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithoutHeaderText;

                base.EditingControlShowing += HandleEditingControlShowing;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI24283", ex);
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Indicates whether the table has any dependent <see cref="IDataEntryControl"/>s.
        /// </summary>
        /// <returns><see langword="true"/> if the table has dependent controls, 
        /// <see langword="false"/> otherwise.</returns>
        protected bool HasDependentControls
        {
            get
            {
                try
                {
                    return (this.PropagateAttributes != null);
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI24464", ex);
                }
            }
        }

        /// <summary>
        /// Gets or sets the name identifying the <see cref="IAttribute"/>(s) to be associated with 
        /// the table.</summary>
        /// <value>Sets the name identifying the <see cref="IAttribute"/>(s) to be associated with 
        /// the table.</value>
        /// <returns>The name identifying the <see cref="IAttribute"/>(s) to be associated with the 
        /// table.</returns>
        [Category("Data Entry Table")]
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

        /// <summary>
        /// Gets or sets a row object that represents the template for all the rows in the control.
        /// Replaces <see cref="DataGridView.RowTemplate"/> to hide it and discourage its use since
        /// a <see cref="DataEntryTableRow"/> object must be used.
        /// <para><b>Requirements</b></para>
        /// Must implement <see cref="DataEntryTableRow"/>
        /// </summary>
        /// <value>A row object that represents the template for all the rows in the control.
        /// </value>
        /// <returns>A row object that represents the template for all the rows in the control.
        /// </returns>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new DataGridViewRow RowTemplate
        {
            get
            {
                return base.RowTemplate;
            }

            set
            {
                ExtractException.Assert("ELI24284", "RowTemplate must implement DataEntryTableRow",
                    value.GetType().IsSubclassOf(typeof(DataEntryTableRow)));

                base.RowTemplate = value;
            }
        }

        /// <summary>
        /// Specifies whether the table will attempt to generate a hint using the intersection of
        /// the row and column occupied by the specified.
        /// <see cref="IAttribute"/>.
        /// </summary>
        /// <value><see langword="true"/> if the table should attempt to generate smart hints when
        /// possible; <see langword="false"/> if the table should never attempt to generate smart
        /// hints.</value>
        /// <returns><see langword="true"/> if the table is configured to generate smart hints when
        /// possible; <see langword="false"/> if the table is not configured to generate smart
        /// hints.</returns>
        [Category("Data Entry Table")]
        protected bool SmartHintsEnabled
        {
            get
            {
                return _smartHintsEnabled;
            }

            set
            {
                _smartHintsEnabled = value;
            }
        }

        /// <summary>
        /// Specifies whether the table will attempt to generate a hint by indicating the other 
        /// <see cref="IAttribute"/>s sharing the same row.
        /// </summary>
        /// <value><see langword="true"/> if the table should attempt to generate row hints when
        /// possible; <see langword="false"/> if the table should never attempt to generate row
        /// hints.</value>
        /// <returns><see langword="true"/> if the table is configured to generate row hints when
        /// possible; <see langword="false"/> if the table is not configured to generate row
        /// hints.</returns>
        [Category("Data Entry Table")]
        protected bool RowHintsEnabled
        {
            get
            {
                return _rowHintsEnabled;
            }

            set
            {
                _rowHintsEnabled = value;
            }
        }

        /// <summary>
        /// Specifies whether the table will attempt to generate a hint by indicating the other
        /// <see cref="IAttribute"/>s sharing the same column.
        /// </summary>
        /// <value><see langword="true"/> if the table should attempt to generate column hints when
        /// possible; <see langword="false"/> if the table should never attempt to generate column
        /// hints.</value>
        /// <returns><see langword="true"/> if the table is configured to generate column hints when
        /// possible; <see langword="false"/> if the table is not configured to generate column
        /// hints.</returns>
        [Category("Data Entry Table")]
        protected bool ColumnHintsEnabled
        {
            get
            {
                return _columnHintsEnabled;
            }

            set
            {
                _columnHintsEnabled = value;
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Tests to see if the provided <see cref="IDataEntryTableCell"/> meets any validation 
        /// requirements the row has.
        /// <para><b>Note</b></para>
        /// If the validation list for the cell's <see cref="DataEntryValidator"/> has been
        /// configured and the cell's value matches a value in the supplied list case-insensitively
        /// but not case-sensitively, the cell's value will be modified to match the casing in the
        /// supplied list.
        /// </summary>
        /// <param name="dataEntryCell">The <see cref="IDataEntryTableCell"/> whose data is to be
        /// validated.</param>
        /// <param name="throwException"><see langword="true"/> to throw an
        /// <see cref="DataEntryValidationException"/> if the cell fails validation or
        /// <see langword="false"/> to display an error icon in the cell.</param>
        /// <throws><see cref="DataEntryValidationException"/> if the 
        /// <see cref="IDataEntryTableCell"/>'s data fails to match any validation requirements it 
        /// has.</throws>
        internal static void ValidateCell(IDataEntryTableCell dataEntryCell, bool throwException)
        {
            try
            {
                if (dataEntryCell.Validator == null || dataEntryCell.Attribute == null)
                {
                    // Nothing to do.
                    return;
                }

                DataGridViewCell cell = dataEntryCell.AsDataGridViewCell;

                // Before validating the value, replace the CRLF display substitution with actual
                // CR/LFs.
                string data = cell.Value.ToString();
                data = data.Replace(DataEntryMethods._CRLF_REPLACEMENT, "\r\n");

                if (!dataEntryCell.Validator.Validate(
                        ref data, dataEntryCell.Attribute, throwException))
                {
                    // If validation fails on a combo box cell, clear the data in the cell since it
                    // displayed wouldn't be displayed in the cell but would be displayed as a tooltip.
                    if (cell is DataEntryComboBoxCell && !string.IsNullOrEmpty(data))
                    {
                        cell.Value = "";
                        cell.ErrorText = "";
                    }
                    // Otherwise, display an error icon to indicate the data is invalid.
                    else
                    {
                        cell.ErrorText = dataEntryCell.Validator.ValidationErrorMessage;
                    }
                }
                else
                {
                    // If validation was successful, make sure any existing error icon is cleared and
                    // apply the data.
                    cell.ErrorText = "";

                    if (cell.Value.ToString() !=
                        data.Replace("\r\n", DataEntryMethods._CRLF_REPLACEMENT))
                    {
                        cell.Value = data;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI25592", ex);
            }
        }

        /// <overloads>Updates the style being used by the specified cell based on the current
        /// state of the table and cell.</overloads>
        /// <summary>
        /// Updates the style being used by the specified <see cref="IDataEntryTableCell"/> based
        /// on the current state of the table and cell.
        /// </summary>
        /// <param name="dataEntryCell">The <see cref="IDataEntryTableCell"/> whose style is to be 
        /// updated.</param>
        internal void UpdateCellStyle(IDataEntryTableCell dataEntryCell)
        {
            try
            {
                // If an attribute has not been specified, treat the cell as if it were a non-
                // data entry cell.
                if (dataEntryCell.Attribute == null)
                {
                    UpdateCellStyle(dataEntryCell.AsDataGridViewCell);
                    return;
                }

                DataGridViewCellStyle style;

                // Check if we need edit mode style
                if (dataEntryCell.AsDataGridViewCell.IsInEditMode)
                {
                    style = _editModeCellStyle;
                }
                // Otherwise, the style needs to be based on _isActive and whether the field has
                // been viewed.
                else
                {
                    bool hasBeenViewed =
                        AttributeStatusInfo.HasBeenViewed(dataEntryCell.Attribute, false);

                    if (_isActive)
                    {
                        style = hasBeenViewed ? _regularActiveCellStyle : _boldActiveCellStyle;
                    }
                    else
                    {
                        style = hasBeenViewed ? _regularInactiveCellStyle : _boldInactiveCellStyle;
                    }
                }

                DataGridViewCell cell = dataEntryCell.AsDataGridViewCell;
                ExtractException.Assert("ELI25629", "Unexpected cell state!", cell != null);

                // Update the cell's style if necessary.
                if (!cell.HasStyle || cell.Style != style)
                {
                    cell.Style = style;
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI25627", ex);
            }
        }

        /// <overloads>Updates the style being used by the specified cell based on the current
        /// state of the table and cell.</overloads>
        /// <summary>
        /// Updates the style being used by the specified <see cref="DataGridViewCell"/> based on
        /// the current state of the table and cell.
        /// </summary>
        /// <param name="cell">The <see cref="DataGridViewCell"/> whose style is to be
        /// updated.</param>
        internal void UpdateCellStyle(DataGridViewCell cell)
        {
            try
            {
                DataGridViewCellStyle style;

                // Check if we need edit mode style.
                if (cell.IsInEditMode)
                {
                    style = _editModeCellStyle;
                }
                // Otherwise, the style needs to be based on _isActive and whether the field has
                // been viewed.
                else
                {
                    bool bold = false;
                    if (cell.HasStyle)
                    {
                        bold = cell.Style.Font.Bold;
                    }

                    if (_isActive)
                    {
                        style = bold ? _boldActiveCellStyle : _regularActiveCellStyle;
                    }
                    else
                    {
                        style = bold ? _boldInactiveCellStyle : _regularInactiveCellStyle;
                    }
                }

                // Update the cell's style if necessary.
                if (!cell.HasStyle || cell.Style != style)
                {
                    cell.Style = style;
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI25630", ex);
            }
        }

        #endregion Methods

        #region Overrides

        /// <summary>
        /// Handles the case that the user has begun updating data in a cell.
        /// </summary>
        /// <param name="e">A <see cref="DataGridViewCellCancelEventArgs"/> that contains the event 
        /// data.</param>
        protected override void OnCellBeginEdit(DataGridViewCellCancelEventArgs e)
        {
            try
            {
                base.OnCellBeginEdit(e);

                // Update the style now that the cell is in edit mode
                base.Rows[e.RowIndex].Cells[e.ColumnIndex].Style = _editModeCellStyle;
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI24312", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the case that an editing control has been displayed to edit the data in a cell
        /// in order that the table can register to recieve <see cref="Control.TextChanged"/> events
        /// as the data is modified.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">An <see cref="DataGridViewEditingControlShowingEventArgs"/> that
        /// contains the event data.</param>
        private void HandleEditingControlShowing(object sender, 
            DataGridViewEditingControlShowingEventArgs e)
        {
            try
            {
                IDataEntryTableCell dataEntryCell = base.CurrentCell as IDataEntryTableCell;

                if (e.Control != null && dataEntryCell != null)
                {
                     _editingControl = e.Control;

                    // If a combo box is being used, register for the SelectedIndexChanged event
                    // to handle changes to the value.
                    if (base.CurrentCell is DataEntryComboBoxCell)
                    {
                        DataGridViewComboBoxEditingControl comboEditingControl =
                            (DataGridViewComboBoxEditingControl)_editingControl;

                        comboEditingControl.SelectedIndexChanged += 
                            HandleCellSelectedIndexChanged; 
                    }
                    // If a text box is being used, initialize the auto-complete values (if
                    // applicable) and register for the TextChanged event to handle
                    // changes to the value.
                    else
                    {
                        DataGridViewTextBoxEditingControl textEditingControl =
                            (DataGridViewTextBoxEditingControl)_editingControl;

                        textEditingControl.TextChanged += HandleCellTextChanged;

                        DataEntryValidator validator = dataEntryCell.Validator;

                        // If available, use the validation list values to initialize the
                        // auto-complete values.
                        if (validator != null)
                        {
                            string[] validationListValues = validator.GetValidationListValues();
                            if (validationListValues != null)
                            {
                                textEditingControl.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                                textEditingControl.AutoCompleteSource = AutoCompleteSource.CustomSource;
                                textEditingControl.AutoCompleteCustomSource.Clear();
                                textEditingControl.AutoCompleteCustomSource.AddRange(
                                    validationListValues);
                            }
                            else
                            {
                                textEditingControl.AutoCompleteMode = AutoCompleteMode.None;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI24986", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the case that the user has finished updating data in a cell.
        /// </summary>
        /// <param name="e">A <see cref="DataGridViewCellEventArgs"/> that contains the event 
        /// data.</param>
        protected override void OnCellEndEdit(DataGridViewCellEventArgs e)
        {
            try
            {
                base.OnCellEndEdit(e);

                if (_editingControl != null)
                {
                    DataGridViewComboBoxEditingControl comboEditingControl =
                        _editingControl as DataGridViewComboBoxEditingControl;
                  
                    // If the editing control was a combo box, unregister from the
                    // SelectionIndexChanged event.
                    if (comboEditingControl != null)
                    {
                        comboEditingControl.SelectedIndexChanged -= HandleCellSelectedIndexChanged;
                    }
                    // Otherwise, unregister from the TextChanged event.
                    else
                    {
                        _editingControl.TextChanged -= HandleCellTextChanged;
                    }
                }

                _editingControl = null;

                // Update the style now that the cell is out of edit mode
                UpdateCellStyle(base.Rows[e.RowIndex].Cells[e.ColumnIndex]);
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI24311", ex);
                ee.AddDebugData("Event Data", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="Font"/> used by the table.
        /// </summary>
        /// <value>The <see cref="Font"/> the table should use.</value>
        /// <returns>The <see cref="Font"/> the table is using.</returns>
        public override Font Font
        {
            get
            {
                return base.Font;
            }

            set
            {
                try
                {
                    base.Font = value;

                    // In case the table has existing cells, update the font in those cells.
                    foreach (DataGridViewRow row in base.Rows)
                    {
                        foreach (DataGridViewCell cell in row.Cells)
                        {
                            cell.Style.Font = value;
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI25220", ex);
                }
            }
        }

        /// <summary>
        /// Raises the <see cref="Control.KeyDown"/> event. Overridden in order to make the current
        /// cell the first cell in the next row rather than the cell below the current one and to
        /// allow delete/cut/copy/paste shortcuts to work even when edit mode is not currently active.
        /// </summary>
        /// <param name="e">A <see cref="KeyEventArgs"/> that contains the event data.</param>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            try
            {
                // If the enter key was pressed and there is only a single cell currently selected,
                // set the current cell as the first cell in the next row.
                if (e.KeyCode == Keys.Enter && base.SelectedCells.Count == 1 &&
                    base.CurrentCell.RowIndex < (base.RowCount - 1))
                {
                    base.CurrentCell = base.Rows[base.CurrentCell.RowIndex + 1].Cells[0];

                    e.Handled = true;
                }
                // Otherwise if a delete/cut/copy/paste shortcut is being used on a single cell,
                // force the current cell into edit mode and re-send the keys to allow the edit
                // control to handle them.
                else if (base.SelectedCells.Count == 1 && 
                         (e.KeyCode == Keys.Delete ||
                            (e.Modifiers == Keys.Control && 
                                (e.KeyCode == Keys.C || e.KeyCode == Keys.X || e.KeyCode == Keys.V))))

                {
                    base.BeginEdit(true);

                    // Ensure the editing control has focus before calling SendKeys to ensure
                    // this won't trigger recursion.
                    if (_editingControl != null && _editingControl.Focused)
                    {
                        switch (e.KeyCode)
                        {
                            case Keys.Delete:   SendKeys.Send("{DEL}"); break;
                            case Keys.C:        SendKeys.Send("^c"); break;
                            case Keys.X:        SendKeys.Send("^x"); break;
                            case Keys.V:        SendKeys.Send("^v"); break;
                            default: ExtractException.ThrowLogicException("ELI25452"); break;
                        }
                    }

                    e.Handled = true;
                }
                // Otherwise, allow the base class to handle the key
                else
                {
                    base.OnKeyDown(e);
                }
            }
            catch (Exception ex)
            {
                ExtractException.AsExtractException("ELI25451", ex).Display();
            }
        }

        /// <summary>
        /// Previews a keyboard message. 
        /// </summary>
        /// <param name="m">A <see cref="Message"/>, passed by reference, that represents the window
        /// message to process.</param>
        /// <returns><see langword="true"/> if the message was processed; otherwise, 
        /// <see langword="false"/>.</returns>
        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        protected override bool ProcessKeyPreview(ref Message m)
        {
            try
            {
                DataGridViewTextBoxEditingControl textBoxEditingControl = 
                    _editingControl as DataGridViewTextBoxEditingControl;

                if (textBoxEditingControl != null && m.Msg == _WM_KEYDOWN)
                {
                    // [DataEntry:267]
                    // When in edit mode of a textbox cell, home and end should go to the beginning
                    // or end of the active cell's text and not exit edit mode as the default
                    // behavior seems to be if all contents are selected.
                    if (textBoxEditingControl.SelectionLength == textBoxEditingControl.Text.Length)
                    {
                        if (m.WParam == (IntPtr)Keys.Home)
                        {
                            textBoxEditingControl.Select(0, 0);
                            return true;
                        }
                        else if (m.WParam == (IntPtr)Keys.End)
                        {
                            textBoxEditingControl.Select(textBoxEditingControl.Text.Length, 0);
                            return true;
                        }
                    }

                    // [DataEntry:275]
                    // When in edit mode of a textbox cell shift + space should not exit edit mode
                    // to select the whole row, but should simply insert a space.
                    else if (m.WParam == (IntPtr)Keys.Space && Control.ModifierKeys == Keys.Shift)
                    {
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI25599", ex);
                ee.AddDebugData("Message", m, false);
                ee.Display();
            }

            return base.ProcessKeyPreview(ref m);
        }

        /// <summary>
        /// Raises the <see cref="Control.KeyUp"/> event. Overridden in order to make the current
        /// cell the first cell in the next row rather than the cell below the current one.
        /// </summary>
        /// <param name="e">A <see cref="KeyEventArgs"/> that contains the event data.</param>
        protected override void OnKeyUp(KeyEventArgs e)
        {
            try
            {
                // If the enter key is being released, there is only a single cell currently selected
                // set the current cell as the first cell in the row.
                if (e.KeyCode == Keys.Enter && base.SelectedCells.Count == 1)
                {
                    // If the current cell isn't in the first column, move it there now (This
                    // situation will occur when the last cell was in edit mode when the enter key
                    // was pressed which prevents the OnKeyDown override from being called).
                    if (base.CurrentCell.ColumnIndex > 0)
                    {
                        base.CurrentCell = base.Rows[base.CurrentCell.RowIndex].Cells[0];
                    }

                    // Ensure ProcessSelectionChange is always called after manually changing
                    // the current selection.
                    ProcessSelectionChange();

                    e.Handled = true;
                }
                // Otherwise, allow the base class to handle the key
                else
                {
                    base.OnKeyUp(e);
                }
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI25441", ex);
            }
        }

        #endregion Overrides

        #region IDataEntryControl Members

        /// <summary>
        /// Fired whenever the set of selected or active <see cref="IAttribute"/>(s) for a control
        /// changes. This can occur as part of the <see cref="PropagateAttributes"/> event, when
        /// new attribute(s) are created via a swipe or when a new element of the control becomes 
        /// active.
        /// </summary>
        /// <seealso cref="IDataEntryControl"/>
        public event EventHandler<AttributesSelectedEventArgs> AttributesSelected;

        /// <summary>
        /// Fired to request that the and <see cref="IAttribute"/> or <see cref="IAttribute"/>(s) be
        /// propagated to any dependent controls.  This can be in response to an 
        /// <see cref="IAttribute"/> having been modified (ie, via a swipe or loading a document) or
        /// the result of a new selection in a multi-attribute table. The event will provide the 
        /// updated <see cref="IAttribute"/>(s) to registered listeners.
        /// </summary>
        /// <seealso cref="IDataEntryControl"/>
        public event EventHandler<PropagateAttributesEventArgs> PropagateAttributes;

        /// <summary>
        /// Fired when the table has been manipulated in such a way that swiping should be
        /// either enabled or disabled.
        /// </summary>
        /// <seealso cref="IDataEntryControl"/>
        public event EventHandler<SwipingStateChangedEventArgs> SwipingStateChanged;

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
        /// Handles the case that the current selection in the table has changed so that the 
        /// background color of the cell can be changed to the "active" or "inactive" color
        /// as appropriate.
        /// </summary>
        /// <param name="e">A <see cref="EventArgs"/> that contains the event data.</param>
        protected override void OnSelectionChanged(EventArgs e)
        {
            try
            {
                base.OnSelectionChanged(e);

                ExtractException.Assert("ELI25375", "Invalid table state!", _color != null);

                // Loop throught the cells to update the styles using IDataEntryTableCell so that
                // the attribute's status can be used to determine viewed state and thus font style.
                foreach (IDataEntryTableCell dataEntryCell in base.SelectedCells)
                {
                    // Set the background color depending on whether the table is currently active.
                    UpdateCellStyle(dataEntryCell);
                }

                // Repeat the iteration for non-data entry cells to make sure the background color
                // is updated for all cells in the table.
                foreach (DataGridViewCell cell in base.SelectedCells)
                {
                    UpdateCellStyle(cell);
                }

                // Command extensions of DataEntryTableBase to process the selection change unless
                // processing is temporarily suppressed.
                if (_suppressSelectionProcessingReferenceCount == 0)
                {
                    ProcessSelectionChange();
                }

                base.Invalidate();
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI24458", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Activates or inactivates the <see cref="DataEntryTable"/>.
        /// <para><b>Requirments:</b></para>
        /// This method must be called with setActive <see langword="true"/> before the user is
        /// able to edit data in the table.
        /// </summary>
        /// <param name="setActive">If <see langref="true"/>, the <see cref="DataEntryTable"/>
        /// should visually indicate that it is active. If <see langref="false"/> the 
        /// <see cref="DataEntryTable"/> should not visually indicate that it is active.</param>
        /// <param name="color">The <see cref="Color"/> that should be used to indicate active 
        /// status (unused if setActive is <see langword="false"/>).</param>
        /// <seealso cref="IDataEntryControl"/>
        public virtual void IndicateActive(bool setActive, Color color)
        {
            try
            {
                _isActive = setActive;
                _color = color;

                // Update the color of the cell styles if necessary.
                if (_editModeCellStyle.BackColor != _color)
                {
                    _editModeCellStyle.BackColor = _color;
                }
                if (_regularActiveCellStyle.SelectionBackColor != _color)
                {
                    _regularActiveCellStyle.SelectionBackColor = _color;
                }
                if (_boldActiveCellStyle.SelectionBackColor != _color)
                {
                    _boldActiveCellStyle.SelectionBackColor = _color;
                }

                // Update the style of the selected cells in the newly activated/deactivated table.
                foreach (DataGridViewCell cell in base.SelectedCells)
                {
                    UpdateCellStyle(cell);
                }

                // [DataEntry:148]
                // Processing needs to take place to mark selected cells as read and derived classes
                // likely will need to raise events such as SwipingStateChanged based on the current
                // selection. OnSelectionChanged encompases the processing that needs to happen,
                // however the windows events are ordered such that a click in the table will trigger
                // GotFocus (and, thus, IndicateActive) before the mouse click is processed which
                // may change the current selection. This means doing the processing now may
                // inappropriately mark cells as read. Therefore, use BeginInvoke to place a call to
                // OnSelectionChange on the control's message queue that will only be called after
                // the other events already on the queue (such as a mouse click) are processed.
                if (setActive)
                {
                    base.BeginInvoke(new EventArgsDelegate(OnSelectionChanged),
                        new object[] { new EventArgs() });
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI24209", ex);
            }
        }

        /// <summary>
        /// Commands a multi-selection control to propogate the specified <see cref="IAttribute"/>
        /// onto dependent <see cref="IDataEntryControl"/>s.
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
        /// <seealso cref="IDataEntryControl"/>
        public virtual void PropagateAttribute(IAttribute attribute, bool selectAttribute)
        {
            try
            {
                // If null is specified, just call ProcessSelectionChange to propagate the currently
                // selected attribute and return;
                if (attribute == null)
                {
                    ProcessSelectionChange();
                    return;
                }

                ExtractException.Assert("ELI24737", "Unexpected attribute!",
                    _attributeMap.ContainsKey(attribute));

                // Within this method, prevent derived classes from processing selection changes
                // until specifically commanded with a ProcessSelectionChange call.
                using (new SelectionProcessingSuppressor(this))
                {
                    // Attempt to cast the table element as a row
                    DataEntryTableRow row = _attributeMap[attribute] as DataEntryTableRow;
                    if (row != null)
                    {
                        // If the mapped table element is a row, it needs to be propagated (and possibly 
                        // selected) unless the specified attribute is already propagated.
                        if (_currentlyPropagatedAttribute != attribute)
                        {
                            DataGridViewCell newCurrentCell = null;

                            if (selectAttribute)
                            {
                                // See if the row's value is also mapped to a column so that it can be 
                                // selected.
                                foreach (IDataEntryTableCell cell in row.Cells)
                                {
                                    if (cell.Attribute == attribute)
                                    {
                                        newCurrentCell = (DataGridViewCell)cell;
                                        break;
                                    }
                                }
                            }

                            // Select the row, if required by selectAttribute
                            if (selectAttribute)
                            {
                                // If the row's attribute was not mapped to a column, just select the
                                // first cell in the row.
                                if (newCurrentCell == null)
                                {
                                    newCurrentCell = row.Cells[0];

                                    // Update the selection to include the entire row.
                                    base.ClearSelection(-1, row.Index, true);
                                }
                                else
                                {
                                    // Select the cell containing
                                    base.ClearSelection(newCurrentCell.ColumnIndex, row.Index, true);
                                }

                                base.CurrentCell = newCurrentCell;

                                // Propagate the selected attribute
                                ProcessSelectionChange();
                            }
                            // Don't select the row, just propagate its attribute.
                            else
                            {
                                OnPropagateAttributes(DataEntryMethods.AttributeAsVector(attribute));
                            }
                        }

                        return;
                    }

                    if (selectAttribute)
                    {
                        // If not a row, the mapped element must be a cell.
                        base.CurrentCell = (DataGridViewCell)_attributeMap[attribute];

                        // Clear all selections first since sometimes trying to select an individual cell 
                        // in a selected row doesn't clear the row selection otherwise.
                        base.ClearSelection();
                        base.ClearSelection(base.CurrentCell.ColumnIndex, base.CurrentCell.RowIndex, true);

                        ProcessSelectionChange();
                    }
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI24629", ex);
            }
        }

        /// <summary>
        /// Refreshes the specified <see cref="IAttribute"/>'s value to the text box.
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> whose value should be refreshed.
        /// </param>
        public virtual void RefreshAttribute(IAttribute attribute)
        {
            try
            {
                object tableElement = null;
                if (_attributeMap.TryGetValue(attribute, out tableElement))
                {
                    DataGridViewCell cell = tableElement as DataGridViewCell;
                    if (cell != null)
                    {
                        cell.Value = attribute.Value.String;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI26121", ex);
            }
        }

        /// <summary>
        /// Handles the case that this table's <see cref="ParentDataEntryControl"/> has requested 
        /// that a new <see cref="IAttribute"/> be propagated.  The <see cref="DataEntryTextBox"/> 
        /// will re-map its control appropriately. The <see cref="AttributeStatusInfo"/> instance 
        /// associated with any <see cref="IAttribute"/> propagated by this handler will be marked
        /// as propagated.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">An <see cref="PropagateAttributesEventArgs"/> that contains the event data.
        /// </param>
        /// <seealso cref="IDataEntryControl"/>
        public virtual void HandlePropagateAttributes(object sender, PropagateAttributesEventArgs e)
        {
            try
            {
                // An attribute can be mapped if there is one and only one attribute to be
                // propagated.
                if (e.Attributes != null && e.Attributes.Size() == 1)
                {
                    base.Enabled = true;

                    // Mark the attribute as propagated.
                    IAttribute parentAttribute = (IAttribute)e.Attributes.At(0);
                    AttributeStatusInfo.MarkAsPropagated(parentAttribute, true, false);

                    // If an attribute name is specified, this is a "child" control intended to act on
                    // a sub-attribute of the provided attribute. If no attribute name is specified,
                    // this is a "sibling" control likely intended to display details of the currently
                    // selected attribute in a multi-selection control.
                    if (!string.IsNullOrEmpty(_attributeName))
                    {
                        // This is a dependent child to the sender. Re-map this control using the
                        // updated attribute's children.
                        SetAttributes(parentAttribute.SubAttributes);
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
                    // If there is more than one parent attribute or no parent attributes, the table
                    // cannot be mapped and should propagate null so all dependent controls are
                    // unmapped.
                    SetAttributes(null);
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI24206", ex);
                ee.AddDebugData("Event data", e, false);
                throw ee;
            }
        }

        #endregion IDataEntryControl Members

        #region Abstract IDataEntryControl Members

        /// <summary>
        /// Gets or sets whether the table accepts as input input the <see cref="SpatialString"/>
        /// associated with an image swipe.
        /// </summary>
        /// <value><see langword="true"/> to configure the table to accept swiped input, 
        /// <see langword="false"/> if it should not accept swiped input.</value>
        /// <returns>If <see langword="true"/>, the table currently accepts input via swiping.
        /// If <see langword="false"/>, the control does not and the swiping tool should be 
        /// disabled.</returns>
        /// <seealso cref="IDataEntryControl"/>
        public abstract bool SupportsSwiping
        {
            get;
            set;
        }

        /// <summary>
        /// Specifies the domain of attributes from which the table should find its mapping. 
        /// </summary>
        /// <param name="sourceAttributes">An <see cref="IUnknownVector"/> vector of 
        /// <see cref="IAttribute"/>s in which the control should find its mapping(s).</param>
        /// <seealso cref="IDataEntryControl"/>
        public abstract void SetAttributes(IUnknownVector sourceAttributes);


        /// <summary>
        /// Requests that the table process the supplied <see cref="SpatialString"/> as input.
        /// </summary>
        /// <param name="swipedText">The <see cref="SpatialString"/> representing the
        /// recognized text in the swiped image area.</param>
        /// <seealso cref="IDataEntryControl"/>
        public abstract void ProcessSwipedText(SpatialString swipedText);

        #endregion Abstract IDataEntryControl Members

        #region IDisposable Members

        /// <summary>
        /// Releases all unmanaged resources used by the <see cref="DataEntryTableBase"/>.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged 
        /// resources; <see langword="false"/> to release only unmanaged resources.</param>        
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose of managed objects
                if (_regularFont != null)
                {
                    _regularFont.Dispose();
                    _regularFont = null;
                }

                if (_boldFont != null)
                {
                    _boldFont.Dispose();
                    _boldFont = null;
                }
            }

            // Dispose of unmanaged resources

            // Dispose of base class
            base.Dispose(disposing);
        }

        #endregion IDisposable Members

        #region Protected Members

        /// <summary>
        /// Ensure's individually viewed cells of the table are marked as viewed. Also commands 
        /// <see cref="DataEntryTableBase"/> extensions to perform any processing that needs to 
        /// happen on a selection change (such as propagating the currently selected attribute).
        /// </summary>
        protected virtual void ProcessSelectionChange()
        {
            try
            {
                // Only mark a cell is viewed if it is the only selected cell and that cell is a
                // IDataEntryTableCell.
                if (_isActive && base.SelectedCells.Count == 1)
                {
                    IDataEntryTableCell cell = base.SelectedCells[0] as IDataEntryTableCell;
                    if (cell != null && cell.Attribute != null)
                    {
                        AttributeStatusInfo.MarkAsViewed(cell.Attribute, true);

                        UpdateCellStyle(cell);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI24935", ex);
            }
        }

        /// <summary>
        /// Raises the <see cref="AttributesSelected"/> event.
        /// </summary>
        /// <param name="attributes">The set of attributes whose spatial information should
        /// be associated with the table.</param>
        /// <param name="includeSubAttributes">Indicates whether the spatial information of the
        /// specified attributes' subattributes should be included as well.</param>
        /// <param name="displayTooltips"><see langword="true"/> if tooltips should be displayed
        /// for the <see paramref="attributes"/>.</param>
        protected void OnAttributesSelected(IUnknownVector attributes, bool includeSubAttributes,
            bool displayTooltips)
        {
            if (this.AttributesSelected != null)
            {
                AttributesSelected(this, new AttributesSelectedEventArgs(attributes,
                    includeSubAttributes, displayTooltips));
            }
        }

        /// <summary>
        /// Raises the <see cref="PropagateAttributes"/> event.
        /// </summary>
        /// <param name="attributes">The set of attributes that have been updated.</param>
        protected void OnPropagateAttributes(IUnknownVector attributes)
        {
            if (this.PropagateAttributes != null)
            {
                IAttribute attributeToPropagate = null;

                // Keep track of any single attribute that is currently propagated so that
                // PropagateAttribute doesn't need to re-propagate attributes that are already
                // propagated.
                if (attributes != null && attributes.Size() == 1)
                {
                    attributeToPropagate = (IAttribute)attributes.At(0);
                }

                // If the attribute needing propagation is not already propagated, propagate it now.
                // (Always propagate an empty selection so that dependent controls are disabled.).
                if (attributeToPropagate == null || 
                    attributeToPropagate != _currentlyPropagatedAttribute)
                {
                    _currentlyPropagatedAttribute = attributeToPropagate;

                    PropagateAttributes(this, new PropagateAttributesEventArgs(attributes));
                }
            }
            else if (attributes != null)
            {
                // If there are no dependent controls registered to receive this event, consider
                // all attributes that would otherwise have been propagated via this event as
                // propagated.
                int attributeCount = attributes.Size();
                for (int i = 0; i < attributeCount; i++)
                {
                    IAttribute attribute = (IAttribute)attributes.At(i);
                    ExtractException.Assert("ELI24452", "Missing attribute data!", attribute != null);

                    AttributeStatusInfo.MarkAsPropagated(attribute, true, true);
                }
            }
        }

        /// <summary>
        /// Raises the <see cref="SwipingStateChanged"/> event.
        /// </summary>
        protected void OnSwipingStateChanged()
        {
            if (this.SwipingStateChanged != null)
            {
                SwipingStateChanged(this, new SwipingStateChangedEventArgs(this.SupportsSwiping));
            }
        }

        /// <summary>
        /// Gets the <see cref="IAttribute"/> associated with the provided row or cell.
        /// </summary>
        /// <param name="item">The row or cell for which the corresponding
        /// <see cref="IAttribute"/> is needed. Must implement either <see cref="IDataEntryTableCell"/>
        /// or <see cref="DataEntryTableRow"/>.</param>
        /// <returns>The <see cref="IAttribute"/> associated with the item.</returns>
        protected static IAttribute GetAttribute(object item)
        {
            try
            {
                // Attempt to interpret the item as a IDataEntryTableCell to get its Attribute.
                IDataEntryTableCell cell = item as IDataEntryTableCell;
                if (cell != null)
                {
                    return cell.Attribute;
                }

                // Attempt to interpret the item as a DataEntryTableRow to get its Attribute.
                DataEntryTableRow row = item as DataEntryTableRow;
                if (row != null)
                {
                    return row.Attribute;
                }

                ExtractException ee = new ExtractException("ELI24287",
                    "GetAttribute called on invalid item type!");
                ee.AddDebugData("Type", item.GetType().Name, false);
                throw ee;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI24938", ex);
            }
        }

        /// <summary>
        /// Links an <see cref="IAttribute"/> to a table element for
        /// efficient lookup in the future. Use of this method is required if the
        /// <see cref="DataEntryTableBase.PropagateAttribute"/> implementation is to be used.
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> to map.</param>
        /// <param name="tableElement">The table element to be mapped.
        /// <para><b>Requirements</b></para>
        /// Must be either a <see cref="DataEntryTableRow"/> or a <see cref="IDataEntryTableCell"/>.
        /// </param>
        protected void MapAttribute(IAttribute attribute, object tableElement)
        {
            try
            {
                ExtractException.Assert("ELI24639", "Missing attribute!", attribute != null);

                // Since the spatial information for this table has likely changed, refresh all
                // spatial hints for this table.
                _hintsAreDirty = true;

                _attributeMap[attribute] = tableElement;

                if (base.Visible)
                {
                    // Only attributes mapped to a IDataEntryTableCell will be viewable.
                    IDataEntryTableCell dataEntryCell = tableElement as IDataEntryTableCell;
                    
                    // Mark the attribute as visible if the table is visible and the table element
                    // is a cell (as opposed to a row which isn't visible on its own)
                    if (dataEntryCell != null)
                    {
                        // Register to recieve notification that the spatial info for the cell has
                        // changed.
                        dataEntryCell.CellSpatialInfoChanged += HandleCellSpatialInfoChanged;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI24638", ex);
            }
        }

        /// <summary>
        /// Clears <see cref="DataEntryTableBase"/>'s internal attribute map and resets selection
        /// to the first displayed table cell.
        /// </summary>
        protected void ClearAttributeMappings()
        {
            try
            {
                // Ensure hints are recalculated next time.
                _hintsAreDirty = true;

                // Clear the attribute map
                _attributeMap.Clear();

                // Reset selection back to the first displayed cell.
                base.CurrentCell = base.FirstDisplayedCell;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI24860", ex);
            }
        }

        /// <summary>
        /// As long as spatial info associated with the table has changed since the last time hints
        /// were generated, spatial hints will be generated according to the table properties for
        /// all <see cref="IAttribute"/>s in the table that are lacking spatial information.
        /// </summary>
        protected void UpdateHints()
        {
            try
            {
                if (_hintsAreDirty && 
                    (_smartHintsEnabled || _rowHintsEnabled || _columnHintsEnabled))
                {
                    _spatialHintGenerator.ClearHintCache();

                    // Loop through all mapped attributes looking for ones that need a spatial hint.
                    foreach (IAttribute attribute in _attributeMap.Keys)
                    {
                        // Find the cell associated with the attribute.
                        DataGridViewCell cell = _attributeMap[attribute] as DataGridViewCell;
                        IDataEntryTableCell dataEntryCell =
                            _attributeMap[attribute] as IDataEntryTableCell;

                        // If the cell is still in the table and that attribute is empty or lacking
                        // spatial info, generate a hint (if possible).
                        if (cell != null && dataEntryCell != null && 
                            cell.RowIndex >= 0 && cell.ColumnIndex >= 0 &&
                            (!attribute.Value.HasSpatialInfo() || 
                             string.IsNullOrEmpty(attribute.Value.String)))
                        {
                            CreateSpatialHint(attribute, cell);
                        }
                    }

                    _hintsAreDirty = false;
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI25148", ex);
            }
        }

        #endregion Protected Members

        #region Event Handlers

        /// <summary>
        /// Handles the case that text has been modified via the active editing control.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        private void HandleCellTextChanged(object sender, EventArgs e)
        {
            try
            {
                // Only IDataEntryTableCells need to be monitored for edits.
                IDataEntryTableCell dataEntryCell = base.CurrentCell as IDataEntryTableCell;
                if (dataEntryCell != null && dataEntryCell.Attribute != null)
                {
                    // Since DataGridViewCells are not normally modified in real-time as text is
                    // changed, apply changes from the editing control to the cell here.
                    if (base.CurrentCell.Value.ToString() != _editingControl.Text)
                    {
                        base.CurrentCell.Value = _editingControl.Text;
                    }
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI24979", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles <see cref="ListBox.SelectedIndexChanged"/> events associated with edits via a
        /// combo box cell.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        private void HandleCellSelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                // Only IDataEntryTableCells need to be monitored for edits.
                IDataEntryTableCell dataEntryCell = base.CurrentCell as IDataEntryTableCell;
                if (dataEntryCell != null && dataEntryCell.Attribute != null)
                {
                    // Since DataGridViewCells are not normally modified in real-time as text is
                    // changed, apply changes from the editing control to the cell here.
                    if (base.CurrentCell.Value.ToString() != _editingControl.Text)
                    {
                        base.CurrentCell.Value = _editingControl.Text;
                    }
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI25566", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// In the case that spatial information relating to a <see cref="IDataEntryTableCell"/> has
        /// changed, spatial hints for the table will be flagged as dirty and data needed to
        /// generate hints will be cached.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">A <see cref="CellSpatialInfoChangedEventArgs"/> that contains the event
        /// data.</param>
        private void HandleCellSpatialInfoChanged(object sender, CellSpatialInfoChangedEventArgs e)
        {
            try
            {
                // Since the spatial information for this table has changed, refresh all spatial
                // hints for this table.
                _hintsAreDirty = true;
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI25231", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        #endregion Event Handlers

        #region Private Members

        /// <summary>
        /// Compiles a list of <see cref="Extract.Imaging.RasterZone"/>s that describe the specified
        /// <see cref="IAttribute"/>'s location.
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> whose 
        /// <see cref="Extract.Imaging.RasterZone"/>(s) are needed.</param>
        /// <returns>A list of <see cref="Extract.Imaging.RasterZone"/>s that describe the specified
        /// <see cref="IAttribute"/>'s location.</returns>
        private static List<Extract.Imaging.RasterZone> GetAttributeRasterZones(IAttribute attribute)
        {
            // Initialize the return value.
            List<Extract.Imaging.RasterZone> zones = new List<Extract.Imaging.RasterZone>();

            // If the specified attribute has a text value and spatial information, proceed to
            // find its location.
            if (!string.IsNullOrEmpty(attribute.Value.String) && attribute.Value.HasSpatialInfo())
            {
                // Add each raster zone from the attribute's value to the raster zone 
                // result list.
                IUnknownVector comRasterZones = attribute.Value.GetOriginalImageRasterZones();
                int rasterZoneCount = comRasterZones.Size();
                for (int j = 0; j < rasterZoneCount; j++)
                {
                    UCLID_RASTERANDOCRMGMTLib.RasterZone comRasterZone =
                        (UCLID_RASTERANDOCRMGMTLib.RasterZone)comRasterZones.At(j);

                    Extract.Imaging.RasterZone rasterZone =
                        new Extract.Imaging.RasterZone(comRasterZone);

                    zones.Add(rasterZone);
                }
            }

            return zones;
        }

        /// <summary>
        /// Retrieves a list of <see cref="Extract.Imaging.RasterZone"/>s that describe the location
        /// of other <see cref="IAttribute"/>s in the same row as the specified cell.
        /// </summary>
        /// <param name="targetCell">The <see cref="DataGridViewCell"/> for which raster zones of
        /// <see cref="IAttribute"/>s sharing the same row are needed.</param>
        /// <returns>A list of <see cref="Extract.Imaging.RasterZone"/>s describe the location
        /// of other <see cref="IAttribute"/>s in the same row.</returns>
        private List<Extract.Imaging.RasterZone> GetRowRasterZones(DataGridViewCell targetCell)
        {
            List<Extract.Imaging.RasterZone> rowZones =
                new List<Extract.Imaging.RasterZone>(base.Columns.Count - 1);

            // Ensure the target cell is a IDataEntryTableCell; otherwise return an empty list
            if (!(targetCell is IDataEntryTableCell))
            {
                return rowZones;
            }

            // Iterate through every column except the column containing the target
            // attribute.
            for (int i = 0; i < base.Columns.Count; i++)
            {
                if (i != targetCell.ColumnIndex)
                {
                    IDataEntryTableCell cell =
                        base.Rows[targetCell.RowIndex].Cells[i] as IDataEntryTableCell;

                    // If the cell in the current column is a DataEntry cell, compile
                    // the RasterZones that describe it's attribute location.
                    if (cell != null)
                    {
                        // But don't use the raster zones from hints 
                        if (AttributeStatusInfo.GetHintType(cell.Attribute) == HintType.None)
                        {
                            rowZones.AddRange(GetAttributeRasterZones(cell.Attribute));
                        }
                    }
                }
            }

            return rowZones;
        }

        /// <summary>
        /// Retrieves a list of <see cref="Extract.Imaging.RasterZone"/>s that describe the location
        /// of other <see cref="IAttribute"/>s in the same column as the specified cell.
        /// </summary>
        /// <param name="targetCell">The <see cref="DataGridViewCell"/> for which raster zones of
        /// <see cref="IAttribute"/>s sharing the same column are needed.</param>
        /// <returns>A list of <see cref="Extract.Imaging.RasterZone"/>s describe the location
        /// of other <see cref="IAttribute"/>s in the same column.</returns>
        private List<Extract.Imaging.RasterZone> GetColumnRasterZones(DataGridViewCell targetCell)
        {
            List<Extract.Imaging.RasterZone> columnZones =
                new List<Extract.Imaging.RasterZone>(base.Rows.Count - 1);

            // Ensure the target cell is a IDataEntryTableCell; otherwise return an empty list
            if (!(targetCell is IDataEntryTableCell))
            {
                return columnZones;
            }

            // Iterate through every row except the row containing the target attribute
            // and the "new" row (if present).
            for (int i = 0; i < base.Rows.Count; i++)
            {
                if (i != targetCell.RowIndex && i != base.NewRowIndex)
                {
                    IDataEntryTableCell cell =
                        base.Rows[i].Cells[targetCell.ColumnIndex] as IDataEntryTableCell;

                    // If the cell in the current row is a DataEntry cell, compile
                    // the RasterZones that describe it's attribute location.
                    if (cell != null)
                    {
                        // But don't use the raster zones from hints 
                        if (AttributeStatusInfo.GetHintType(cell.Attribute) == HintType.None)
                        {
                            columnZones.AddRange(GetAttributeRasterZones(cell.Attribute));
                        }
                    }
                }
            }

            return columnZones;
        }

        /// <summary>
        /// Attempts to provide a hint as to where the data for the specified 
        /// <see cref="IAttribute"/> might appear.
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> for which a spatial hint is wanted.
        /// </param>
        /// <param name="targetCell">The <see cref="DataGridViewCell"/> for which a spatial hint
        /// is wanted.</param>
        protected void CreateSpatialHint(IAttribute attribute, DataGridViewCell targetCell)
        {
            try
            {
                ExtractException.Assert("ELI25232", "Null argument exception!", attribute != null);
                ExtractException.Assert("ELI25233", "Null argument exception!", targetCell != null);

                // Initialize a list of raster zones for hint locations.
                List<Extract.Imaging.RasterZone> spatialHints = null;

                // A list to keep track of RasterZones for attributes sharing the same row as the
                // target attribute.
                List<Extract.Imaging.RasterZone> rowZones = null;

                // A list to keep track of RasterZones for attributes sharing the same column as the
                // target attribute.
                List<Extract.Imaging.RasterZone> columnZones = null;

                // Attempt to generate a hint based on the intersection of row and column data
                // if there are multiple rows and columns in the table.
                if (_smartHintsEnabled && base.Rows.Count > 1 && base.Columns.Count > 1)
                {
                    // Compile a set of raster zones representing the other attributes sharing
                    // the same row and column.
                    rowZones = GetRowRasterZones(targetCell);
                    columnZones = GetColumnRasterZones(targetCell);

                    // Attempt to generate a spatial hint using the spatial intersection of the
                    // row and column zones.
                    Extract.Imaging.RasterZone rasterZone =
                        _spatialHintGenerator.GetRowColumnIntersectionSpatialHint(
                            targetCell.RowIndex, rowZones, targetCell.ColumnIndex, columnZones);

                    // If a smart hint was able to be generated, use it.
                    if (rasterZone != null)
                    {
                        spatialHints = new List<Extract.Imaging.RasterZone>();
                        spatialHints.Add(rasterZone);

                        AttributeStatusInfo.SetHintType(attribute, HintType.Direct);
                        AttributeStatusInfo.SetHintRasterZones(attribute, spatialHints);

                        return;
                    }
                }

                // If a "smart" hint was not generated, check to see if row and/or column hints can
                // and should be generated.
                // If row hints are enabled and there are other attributes sharing the row.
                if (_rowHintsEnabled && base.Columns.Count > 1)
                {
                    // Compile the raster zones of other attributes sharing the row if they
                    // haven't already been compiled.
                    if (rowZones == null)
                    {
                        spatialHints = GetRowRasterZones(targetCell);
                    }
                    // Or just use the raster zones that were already compiled.
                    else
                    {
                        spatialHints = rowZones;
                    }
                }

                // If column hints are enabled and there are other attributes sharing the column.
                if (_columnHintsEnabled && base.Rows.Count > 1)
                {
                    // Compile the raster zones of other attributes sharing the column if they
                    // haven't already been compiled.
                    if (columnZones == null)
                    {
                        columnZones = GetColumnRasterZones(targetCell);
                    }

                    // Assign or append the column zones to the return value.
                    if (spatialHints == null)
                    {
                        spatialHints = new List<Extract.Imaging.RasterZone>(columnZones);
                    }
                    else
                    {
                        spatialHints.AddRange(columnZones);
                    }
                }

                // As long as some kind of spatial hint was generated, return an indirect hint
                if (spatialHints != null)
                {
                    AttributeStatusInfo.SetHintType(attribute, HintType.Indirect);
                    AttributeStatusInfo.SetHintRasterZones(attribute, spatialHints);
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI25001", ex);
            }
        }

        #endregion Private Members
    }
}
