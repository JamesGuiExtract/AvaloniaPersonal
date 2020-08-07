using Extract.Imaging;
using Extract.Licensing;
using Extract.Utilities.Forms;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using UCLID_AFCORELib;
using UCLID_COMUTILSLib;

using ComRasterZone = UCLID_RASTERANDOCRMGMTLib.RasterZone;
using SpatialString = UCLID_RASTERANDOCRMGMTLib.SpatialString;

namespace Extract.DataEntry
{
    /// <summary>
    /// A base of common code needed by any <see cref="IDataEntryControl"/> that extends
    /// <see cref="DataGridView"/>.
    /// </summary>
    public abstract partial class DataEntryTableBase : DataGridView, IDataEntryControl, ISupportInitialize, IDataEntryAutoCompleteControl
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(DataEntryTableBase).ToString();

        /// <summary>
        /// The color that should be used to indicate table's current selection when the table is
        /// not the active data control.</summary>
        static readonly Color _INACTIVE_SELECTION_COLOR = Color.LightGray;

        #endregion Constants

        #region Fields

        /// <summary>
        /// Specifies whether the current instance is running in design mode.
        /// </summary>
        readonly bool _inDesignMode;

        /// <summary>
        /// The name used to identify the <see cref="IAttribute"/> to be associated with the table.
        /// </summary>
        string _attributeName;

        /// <summary>
        /// The <see cref="DataEntryControlHost"/> to which this control belongs.
        /// </summary>
        DataEntryControlHost _dataEntryControlHost;

        /// <summary>
        /// Used to specify the data entry control which is mapped to the parent of the attribute(s) 
        /// to which the current table is to be mapped.
        /// </summary>
        IDataEntryControl _parentDataEntryControl;

        /// <summary>
        /// Specifies the color which should be used to indicate active status.
        /// </summary>
        Color _color;

        /// <summary>
        /// Specifies whether the control should remain disabled at all times.
        /// </summary>
        bool _disabled;

        /// <summary>
        /// Specifies whether the clipboard contents should be cleared after pasting into the
        /// control.
        /// </summary>
        bool _clearClipboardOnPaste;

        /// <summary>
        /// Specifies whether descendant attributes in other controls should be highlighted.
        /// </summary>
        bool _highlightSelectionInChildControls = true;

        /// <summary>
        /// Specifies whether the table will attempt to generate a hint by indicating the other
        /// attributes sharing the same row.
        /// </summary>
        bool _rowHintsEnabled = true;

        /// <summary>
        /// Specifies whether the table will attempt to generate a hint by indicating the other 
        /// attributes sharing the same column.
        /// </summary>
        bool _columnHintsEnabled = true;

        /// <summary>
        /// Used to generate "smart" hints for attributes missing spatial info.
        /// </summary>
        readonly SpatialHintGenerator _spatialHintGenerator = new SpatialHintGenerator();

        /// <summary>
        /// Indicates whether the spatial information has changed such that hints need to be
        /// recalculated.
        /// </summary>
        bool _hintsAreDirty;

        /// <summary>
        /// Indicates whether the table is currently active.
        /// </summary>
        bool _isActive;

        /// <summary>
        /// Keeps track of which table element is mapped to each attribute represented in the table.
        /// </summary>
        readonly Dictionary<IAttribute, object> _attributeMap = new Dictionary<IAttribute, object>();

        /// <summary>
        /// The attribute that is currently propagated by this table (if any)
        /// </summary>
        IAttribute _currentlyPropagatedAttribute;

        /// <summary>
        /// A reference count of the methods that have requested to temporarily prevent processing
        /// of selection changes so that one or more programmatic selection changes can be made 
        /// before the table is in its intended state and ready to process selection events.
        /// </summary>
        int _suppressSelectionProcessingReferenceCount;

        /// <summary>
        /// A reference count of the methods that have requested to temporarily prevent refreshes of
        /// the table data so that all data to be applied can be applied before
        /// RefreshAttributes(bool, IAttribute) can be executed.
        /// </summary>
        int _suppressRefreshReferenceCount;

        /// <summary>
        /// The set of attributes that are pending to be refreshed.
        /// </summary>
        HashSet<IAttribute> _pendingRefreshAttributes = new HashSet<IAttribute>();

        /// <summary>
        /// Whether attribute spatial info needs to be refreshed once the suppression of attribute
        /// refreshes is released.
        /// </summary>
        bool _pendingSpatialRefresh;

        /// <summary>
        /// Indicates whether data is currently being dragged over the table.
        /// </summary>
        bool _dragOverInProgress;

        /// <summary>
        /// The control currently being used to update the data in the active cell.
        /// </summary>
        Control _editingControl;

        /// <summary>
        /// A regular style font to indicate viewed fields.
        /// </summary>
        Font _regularFont;

        /// <summary>
        /// A bold style font to indicate unviewed fields.
        /// </summary>
        Font _boldFont;

        /// <summary>
        /// The default style to use for cells.
        /// </summary>
        DataGridViewCellStyle _defaultCellStyle;

        /// <summary>
        /// The style to use for cells currently being edited.
        /// </summary>
        DataGridViewCellStyle _editModeCellStyle;

        /// <summary>
        /// The style to use for selected cells whose fields have been viewed in the active table.
        /// </summary>
        DataGridViewCellStyle _regularActiveCellStyle;

        /// <summary>
        /// The style to use for selected cells whose fields have been viewed and are not in the
        /// active table.
        /// </summary>
        DataGridViewCellStyle _regularInactiveCellStyle;

        /// <summary>
        /// The style to use for selected cells whose fields have not been viewed in the active table.
        /// </summary>
        DataGridViewCellStyle _boldActiveCellStyle;

        /// <summary>
        /// The style to use for selected cells whose fields have not been viewed and are not in the
        /// active table.
        /// </summary>
        DataGridViewCellStyle _boldInactiveCellStyle;

        /// <summary>
        /// The style to use for selected cells whose contents have been viewed and that are
        /// currently being dragged.
        /// </summary>
        DataGridViewCellStyle _regularDraggedCellStyle;

        /// <summary>
        /// The style to use for selected cells whose contents have not been viewed and that are
        /// currently being dragged.
        /// </summary>
        DataGridViewCellStyle _boldDraggedCellStyle;

        /// <summary>
        /// The style to use for table cells when the table is disabled.
        /// </summary>
        DataGridViewCellStyle _disabledCellStyle;

        /// <summary>
        /// [DataEntry:920] We need to prevent programmatic calls to EndEdit in some circumstances
        /// to prevent the table from getting into a bad state.
        /// </summary>
        bool _preventProgrammaticEndEdit;

        LuceneAutoSuggest _luceneAutoSuggest;

        #endregion Fields

        #region Delegates

        /// <summary>
        /// Signature to use for invoking methods that accept one <see cref="EventArgs"/> parameter.
        /// </summary>
        /// <param name="e">An <see cref="EventArgs"/> parameter.</param>
        delegate void EventArgsDelegate(EventArgs e);

        #endregion Delegates

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
            readonly DataEntryTableBase _dataEntryTable;

            /// <summary>
            /// Indicates whether or not the object instance has been disposed.
            /// </summary>
            bool _disposed;

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

        #region RefreshSuppressor

        /// <summary>
        /// A class to manage requests to suppress processing of the
        /// RefreshAttributes(bool, IAttribute[]) method.
        /// Created in response to DataEntry:1188 in which refreshes in the middle of applying
        /// attribute data caused problems.
        /// </summary>
        protected class RefreshSuppressor : IDisposable
        {
            /// <summary>
            /// The <see cref="DataEntryTable"/> whose RefreshAttributes method is to be suppressed.
            /// </summary>
            readonly DataEntryTableBase _dataEntryTable;

            /// <summary>
            /// Indicates whether or not the object instance has been disposed.
            /// </summary>
            bool _disposed;

            /// <summary>
            /// Initializes a new <see cref="RefreshSuppressor"/> instance.
            /// </summary>
            /// <param name="dataEntryTable">The <see cref="DataEntryTable"/> whose RefreshAttributes
            /// method is to be suppressed. RefreshAttributes will continue to be suppressed until
            /// this instance is disposed of and there are no other instances of
            /// <see cref="RefreshSuppressor"/> currently active.</param>
            public RefreshSuppressor(DataEntryTableBase dataEntryTable)
            {
                try
                {
                    ExtractException.Assert("ELI35382", "Null argument exception!",
                        dataEntryTable != null);

                    _dataEntryTable = dataEntryTable;
                    _dataEntryTable._suppressRefreshReferenceCount++;
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI35383", ex);
                }
            }

            /// <summary>
            /// Relinquishes suppression of the RefreshAttributes method.
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
                    ExtractException.Display("ELI35384", ex);
                }
            }

            /// <overloads>Relinquishes suppression of the RefreshAttributes method.</overloads>
            /// <summary>
            /// Relinquishes suppression of the RefreshAttributes method.
            /// </summary>
            /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged 
            /// resources; <see langword="false"/> to release only unmanaged resources.</param>        
            protected virtual void Dispose(bool disposing)
            {
                if (disposing && !_disposed)
                {
                    // Dispose of managed objects
                    _dataEntryTable._suppressRefreshReferenceCount--;
                    _disposed = true;
                }

                // Dispose of unmanaged resources
            }
        }

        #endregion RefreshSuppressor

        #region Constructors

        /// <summary>
        /// Initializes <see cref="DataEntryTableBase"/> instance as part of a derived class.
        /// </summary>
        protected DataEntryTableBase()
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
                LicenseUtilities.ValidateLicense(
                    LicenseIdName.DataEntryCoreComponents, "ELI24495", _OBJECT_NAME);

                // Initialize the fonts used by the DataGridViewCellStyle objects.
                _regularFont = new Font(DefaultCellStyle.Font, FontStyle.Regular);
                _boldFont = new Font(DefaultCellStyle.Font, FontStyle.Bold);

                InitializeCellStyles();

                // Use a DataEntryTableRow instance as the row template.
                base.RowTemplate = new DataEntryTableRow();

                EditMode = DataGridViewEditMode.EditOnKeystrokeOrF2;

                ClipboardCopyMode = DataGridViewClipboardCopyMode.EnableWithoutHeaderText;

                AttributeStatusInfo.ValidationStateChanged += HandleValidationStateChanged;
                AttributeStatusInfo.AttributeInitialized += HandleAttributeStatusInfo_AttributeInitialized;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI24283", ex);
            }
        }

        /// <summary>
        /// A table element that is currently being initialized. This is used to be able to map any
        /// attribute that is initialized to the table element before data queries that may need to
        /// access the element are applied (per ISSUE-13193).
        /// </summary>
        protected object InitializingTableElement
        {
            get;
            set;
        }

        /// <summary>
        /// Handles the AttributeInitialized event in order to to map the attribute to any
        /// <see cref="InitializingTableElement"/> currently defined.
        /// </summary>
        void HandleAttributeStatusInfo_AttributeInitialized(object sender, AttributeInitializedEventArgs e)
        {
            try
            {
                if (e.DataEntryControl == this
                        && InitializingTableElement != null
                        && !_attributeMap.ContainsKey(e.Attribute))
                {
                    MapAttribute(e.Attribute, InitializingTableElement);
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI50232");
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets a value indicating whether the current instance is running in design mode.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if in design mode; otherwise, <see langword="false"/>.
        /// </value>
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool InDesignMode
        {
            get
            {
                return _inDesignMode;
            }
        }

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
                    return (PropagateAttributes != null);
                }
                catch (Exception ex)
                {
                    throw ExtractException.AsExtractException("ELI24464", ex);
                }
            }
        }

        /// <summary>
        /// Gets whether the table is currently active.
        /// </summary>
        /// <returns><see langword="true"/> if the table is currently active (in terms of the data
        /// entry framework); <see langword="false"/> otherwise.</returns>
        protected bool IsActive
        {
            get
            {
                return _isActive;
            }
        }

        /// <summary>
        /// Gets or set the <see cref="DataGridViewCell"/> for which an auto-complete list is
        /// currently displayed, or <see langword="null"/> if no auto-complete list is currently
        /// displayed.
        /// </summary>
        /// <value>The <see cref="DataGridViewCell"/> for which an auto-complete list is currently
        /// displayed, or <see langword="null"/> if no auto-complete list is currently displayed.
        /// </value>
        protected DataGridViewCell AutoCompleteCell
        {
            get;
            set;
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

        /// <summary>
        /// Gets whether data is currently being dragged over the table.
        /// </summary>
        /// <returns><see langword="true"/> if data is currently being dragged over the table;
        /// <see langword="false"/> otherwise.</returns>
        protected bool DragOverInProgress
        {
            get
            {
                return _dragOverInProgress;
            }
        }

        /// <summary>
        /// Gets or sets the currently active cell.
        /// <para><b>NOTE</b></para>
        /// This property hides and re-implements <see cref="DataGridView.CurrentCell"/>. This
        /// re-implementation should be used instead because of processing that occurs for
        /// selection changes. Using the base <see cref="DataGridView.CurrentCell"/>, the
        /// <see cref="DataGridView.SelectionChanged"/> event will sometimes be raised prior to
        /// selection changing when a new <see cref="CurrentCell"/> value being assigned.
        /// This causes problems in code that counts on the <see cref="DataGridView.SelectionChanged"/>
        /// event to occur after the new selection is in place.
        /// </summary>
        /// <value>The <see cref="DataGridViewCell"/> to make active.</value>
        /// <returns>The currently active <see cref="DataGridViewCell"/>.</returns>
        new protected DataGridViewCell CurrentCell
        {
            get
            {
                return base.CurrentCell;
            }

            set
            {
                using (new SelectionProcessingSuppressor(this))
                {
                    // https://extract.atlassian.net/browse/ISSUE-702
                    // To prevent exceptions when changing selection in a table which has vertical
                    // sizing which cuts off a row, call PerformLayout first.
                    PerformLayout();

                    base.CurrentCell = value;
                }
                OnSelectionChanged(new EventArgs());
            }
        }

        /// <summary>
        /// The <see cref="DataEntryAutoCompleteMode"/> to use for text box editing controls
        /// </summary>
        [Category("Data Entry Table")]
        public DataEntryAutoCompleteMode AutoCompleteMode { get; set; } = DataEntryAutoCompleteMode.SuggestLucene;

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
        internal void ValidateCell(IDataEntryTableCell dataEntryCell, bool throwException)
        {
            try
            {
                IAttribute attribute = dataEntryCell.Attribute;

                DataGridViewCell cell = dataEntryCell.AsDataGridViewCell;
                DataValidity dataValidity;

                if (attribute == null || AttributeStatusInfo.GetOwningControl(attribute).Disabled)
                {
                    // Nothing to do except clear any existing validation errors/warnings.
                    cell.ErrorText = "";
                    return;
                }

                // [DataEntry:913]
                // If someone is typing in a text box cell, we don't want to auto-correct as they
                // are typing, but if a comboBox cell is in edit mode, we need to auto-correct right
                // away to exactly match the combo-box value, otherwise an error will result.
                if (EditingControl == null || dataEntryCell is DataEntryComboBoxCell)
                {
                    string correctedValue;
                    dataValidity =
                        AttributeStatusInfo.Validate(attribute, throwException, out correctedValue);
                    if (dataValidity == DataValidity.Valid && !string.IsNullOrEmpty(correctedValue))
                    {
                        cell.Value = correctedValue;
                    }
                }
                else
                {
                    dataValidity = AttributeStatusInfo.Validate(attribute, throwException);
                }

                if (dataValidity != DataValidity.Valid)
                {
                    // If validation fails on a combo box cell, clear the data in the cell since it
                    // wouldn't be displayed in the cell but would be displayed as a tooltip.
                    if (dataValidity != DataValidity.ValidationWarning && cell is DataEntryComboBoxCell)
                    {
                        // Only clear the value if it is not already empty to avoid an infinite
                        // recursion loop by triggering cell validation.
                        if (!string.IsNullOrEmpty(cell.Value.ToString()))
                        {
                            cell.Value = "";
                        }
                    }
                    
                    // Display an error icon to indicate the data is invalid.
                    IDataEntryValidator validator =
                            AttributeStatusInfo.GetStatusInfo(attribute).Validator;
                    ExtractException.Assert("ELI29215", "Null validator exception!",
                        validator != null);
                    cell.ErrorText = validator.ValidationErrorMessage;
                }
                else
                {
                    // If validation was successful, make sure any existing error icon is cleared and
                    // apply the data.
                    cell.ErrorText = "";
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

                // Check if we need the disabled style
                if (!Enabled)
                {
                    style = _disabledCellStyle;
                }
                // Check if we need edit mode style
                else if (dataEntryCell.AsDataGridViewCell.IsInEditMode)
                {
                    style = _editModeCellStyle;
                }
                // Otherwise, the style needs to be based on _isActive and whether the field has
                // been viewed.
                else
                {
                    bool hasBeenViewed =
                        AttributeStatusInfo.HasBeenViewedOrIsNotViewable(dataEntryCell.Attribute, false);

                    if (dataEntryCell.IsBeingDragged)
                    {
                        style = hasBeenViewed ? _regularDraggedCellStyle : _boldDraggedCellStyle;
                    }
                    else if (_isActive)
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
                // If the cell is a data entry cell with a valid attribute, use the other overload.
                IDataEntryTableCell dataEntryCell = cell as IDataEntryTableCell;
                if (dataEntryCell != null && dataEntryCell.Attribute != null)
                {
                    UpdateCellStyle(dataEntryCell);
                    return;
                }

                DataGridViewCellStyle style;

                // Check if we need the disabled style
                if (!Enabled)
                {
                    style = _disabledCellStyle;
                }
                // Check if we need edit mode style.
                else if (cell.IsInEditMode)
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

                    // Don't show cells that aren't in DataEntryTableColumns as active. This check
                    // is being added to prevent a greed background behind OrderPickerTableColumn
                    // buttons, but in general this seems like a reasonable way to differentiate
                    // table elements that are not mapped into the attribute hierarchy.
                    if (_isActive && dataEntryCell != null)
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
                Rows[e.RowIndex].Cells[e.ColumnIndex].Style = _editModeCellStyle;
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI24312", ex);
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

                        // Dispose the auto-suggest control here to prevent it from popping up
                        // twice when a new cell is edited
                        _luceneAutoSuggest?.Dispose();
                        _luceneAutoSuggest = null;
                    }
                }

                _editingControl = null;

                DataGridViewCell cell = Rows[e.RowIndex].Cells[e.ColumnIndex];

                // [DataEntry:905]
                // Validate the cell to ensure the cell value is auto-corrected for capitalization,
                // whitespace.
                IDataEntryTableCell dataEntryCell = cell as IDataEntryTableCell;
                if (dataEntryCell != null)
                {
                    ValidateCell(dataEntryCell, false);
                }

                // Update the style now that the cell is out of edit mode
                UpdateCellStyle(cell);
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
                    foreach (DataGridViewRow row in Rows)
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
                // If a delete/cut/copy/paste shortcut is being used on a single cell,
                // force the current cell into edit mode and re-send the keys to allow the edit
                // control to handle them.
                if (!e.Handled && SelectedCells.Count == 1 && 
                         (e.KeyCode == Keys.Delete ||
                            (e.Modifiers == Keys.Control && 
                                (e.KeyCode == Keys.C || e.KeyCode == Keys.X || e.KeyCode == Keys.V))))

                {
                    // In case of delete key, call DeleteSelectedCellContents so that any associated
                    // spatial info is deleted as well.
                    if (e.KeyCode == Keys.Delete)
                    {
                        DeleteSelectedCellContents();
                    }
                    else
                    {
                        base.BeginEdit(true);

                        // Ensure the editing control has focus before calling SendKeys to ensure
                        // this won't trigger recursion.
                        if (_editingControl != null && _editingControl.Focused)
                        {
                            switch (e.KeyCode)
                            {
                                case Keys.C: SendKeys.Send("^c"); break;
                                case Keys.X: SendKeys.Send("^x"); break;
                                case Keys.V: SendKeys.Send("^v"); break;
                                default: ExtractException.ThrowLogicException("ELI25452"); break;
                            }
                        }
                    }

                    e.Handled = true;
                }
                // Otherwise, allow the base class to handle the key
                else
                {
                    // If in edit mode but the edit control does not have focus, send keyboard input
                    // to the editing control.
                    if (_editingControl != null && !_editingControl.Focused)
                    {
                        KeyMethods.SendKeyToControl(e.KeyValue, e.Shift, e.Control, e.Alt, _editingControl);
                        e.Handled = true;
                    }
                }

                base.OnKeyDown(e);
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
        protected override bool ProcessKeyPreview(ref Message m)
        {
            try
            {
                DataGridViewTextBoxEditingControl textBoxEditingControl = 
                    _editingControl as DataGridViewTextBoxEditingControl;

                if (textBoxEditingControl != null && m.Msg == WindowsMessage.KeyDown)
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
        /// Raises the <see cref="Control.DragEnter"/> event.
        /// </summary>
        /// <param name="drgevent">The <see cref="DragEventArgs"/> associated with the event.
        /// </param>
        protected override void OnDragEnter(DragEventArgs drgevent)
        {
            try
            {
                _dragOverInProgress = true;

                base.OnDragEnter(drgevent);
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI26666", ex);
                ee.AddDebugData("Event Data", drgevent, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Raises the <see cref="Control.DragEnter"/> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs"/> associated with the event.
        /// </param>
        protected override void OnDragLeave(EventArgs e)
        {
            try
            {
                _dragOverInProgress = false;

                base.OnDragLeave(e);
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI26670", ex);
                ee.AddDebugData("Event Data", e, false);
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
                _dragOverInProgress = false;

                base.OnDragDrop(drgevent);

                // [DataEntry:490-492]
                // Update attribute selection to redisplay the tooltip and to make the DEP aware of
                // the new selection.
                ProcessSelectionChange();
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI26706", ex);
                ee.AddDebugData("Event Data", drgevent, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.DataGridView.CurrentCellChanged"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.
        /// </param>
        protected override void OnCurrentCellChanged(EventArgs e)
        {
            try
            {
                // Whenever the current cell initially changes, auto-complete will no longer be
                // active.
                if (AutoCompleteCell != null)
                {
                    AutoCompleteCell = null;
                }

                base.OnCurrentCellChanged(e);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI36100");
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
                // Iterate both data entry and non-data entry cells that are selected both prior to
                // and after the selection change to make sure the background color is updated
                // correctly for all cells..
                HashSet<DataGridViewCell> cellsToUpdate =
                    new HashSet<DataGridViewCell>(SelectedCells.Cast<DataGridViewCell>());

                base.OnSelectionChanged(e);

                foreach (DataGridViewCell cell in SelectedCells)
                {
                    cellsToUpdate.Add(cell);
                }

                foreach (DataGridViewCell cell in cellsToUpdate)
                {
                    UpdateCellStyle(cell);
                }

                // Command extensions of DataEntryTableBase to process the selection change unless
                // processing is temporarily suppressed.
                if (_suppressSelectionProcessingReferenceCount == 0)
                {
                    ProcessSelectionChange();
                }

                Invalidate();
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI24458", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Raises the <see cref="Control.EnabledChanged"/> event.
        /// </summary>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        protected override void OnEnabledChanged(EventArgs e)
        {
            try
            {
                base.OnEnabledChanged(e);

                if (!_inDesignMode)
                {
                    // Change the table's default cell style base on the enabled status
                    DefaultCellStyle = Enabled ? _defaultCellStyle : _disabledCellStyle;

                    // Update the style of all cells currently displayed.
                    foreach (DataGridViewRow row in Rows)
                    {
                        foreach (DataGridViewCell cell in row.Cells)
                        {
                            UpdateCellStyle(cell);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI27326", ex);
            }
        }

        /// <summary>
        /// Raises the <see cref="DataGridView.EditingControlShowing"/> to display the control to 
        /// edit the data in a cell.
        /// </summary>
        /// <param name="e">An <see cref="DataGridViewEditingControlShowingEventArgs"/> that
        /// contains the event data.</param>
        protected override void OnEditingControlShowing(DataGridViewEditingControlShowingEventArgs e)
        {
            try
            {
                base.OnEditingControlShowing(e);

                // If the current selection does not result in a propagated attribute when dependent
                // controls are present (i.e., selection across multiple rows), don't allow edit mode
                // since that dependent triggers in the dependent controls wouldn't be updated.
                if (PropagateAttributes != null && _currentlyPropagatedAttribute == null &&
                    CurrentRow.Index != NewRowIndex)
                {
                    EndEdit();
                    return;
                }

                // [DataEntry:415]
                // Limit the selection to just the cell being edited to prevent unexpected behavior
                // when tabbing after the edit.
                // [DataEntry:664]
                // Also ensure that the currently selected cell is the CurrentCell (the cell that is
                // in edit mode).
                if (SelectedCells.Count > 1 || !SelectedCells.Contains(base.CurrentCell))
                {
                    // Clear all selections first since sometimes trying to select an individual cell 
                    // in a selected row doesn't clear the row selection otherwise.
                    ClearSelection();
                    ClearSelection(base.CurrentCell.ColumnIndex, base.CurrentCell.RowIndex,
                        true);
                }

                IDataEntryTableCell dataEntryCell = base.CurrentCell as IDataEntryTableCell;
                DataGridViewTextBoxEditingControl textEditingControl = null;

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
                    else if (dataEntryCell.Attribute != null)
                    {

                        textEditingControl = (DataGridViewTextBoxEditingControl)_editingControl;
                        textEditingControl.TextChanged += HandleCellTextChanged;
                        IDataEntryValidator validator =
                            AttributeStatusInfo.GetStatusInfo(dataEntryCell.Attribute).Validator;

                        if (AutoCompleteMode == DataEntryAutoCompleteMode.SuggestLucene && validator != null)
                        {
                            var autoCompleteValues = validator.AutoCompleteValuesWithSynonyms;
                            _luceneAutoSuggest = new LuceneAutoSuggest(textEditingControl, this);
                            _luceneAutoSuggest.SetDataEntryControlHost(DataEntryControlHost);
                            _luceneAutoSuggest.SetListBackColor(_color);
                            _luceneAutoSuggest.UpdateAutoCompleteList(autoCompleteValues);

                        }
                        else
                        {
                            AutoCompleteMode autoCompleteMode = textEditingControl.AutoCompleteMode;
                            AutoCompleteSource autoCompleteSource =
                                textEditingControl.AutoCompleteSource;
                            AutoCompleteStringCollection autoCompleteList =
                                textEditingControl.AutoCompleteCustomSource;
                            if (DataEntryMethods.UpdateAutoCompleteList(validator, ref autoCompleteMode,
                                    ref autoCompleteSource, ref autoCompleteList, out string[] autoCompleteValues))
                            {
                                // If auto-complete has been turned on/off from its previous state,
                                // update the registration for the EditingControlPreviewKeyDown event.
                                if (textEditingControl.AutoCompleteMode != autoCompleteMode)
                                {
                                    if (autoCompleteMode == System.Windows.Forms.AutoCompleteMode.None)
                                    {
                                        textEditingControl.PreviewKeyDown -=
                                            HandleEditingControlPreviewKeyDown;
                                    }
                                    else
                                    {
                                        textEditingControl.PreviewKeyDown +=
                                            HandleEditingControlPreviewKeyDown;
                                    }
                                }

                                textEditingControl.AutoCompleteMode = autoCompleteMode;
                                textEditingControl.AutoCompleteSource = autoCompleteSource;
                                textEditingControl.AutoCompleteCustomSource = autoCompleteList;
                            }
                        }
                    }
                }
                
                // [DataEntry:1109]
                // If textEditingControl has not been set it means the field is not in a
                // DataEntryTableRow or DataEntryTableColumn or it is in the new row of a table.
                // In either case, at this time an auto-complete list should not be displayed.
                // Displaying it now while in the new row can cause behavioral issues in the
                // auto-complete box when it is re-displayed once the new row is initialized
                // via DataEntryTable.BeginEdit.
                if (textEditingControl == null)
                {
                    textEditingControl = _editingControl as DataGridViewTextBoxEditingControl;
                    if (textEditingControl != null)
                    {
                        textEditingControl.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.None;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI24986", ex);
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.Validating"/> event to prevent edit mode from ending as
        /// a result of the DEP losing focus.
        /// </summary>
        /// <param name="e">A <see cref="CancelEventArgs"/> that contains the event data.</param>
        protected override void OnValidating(CancelEventArgs e)
        {
            try
            {
                // If in edit mode and the DEP doesn't contain focus, eat the OnValidating event.
                // This will prevent edit mode from ending.
                if (EditingControl == null ||
                    (DataEntryControlHost != null && DataEntryControlHost.ContainsFocus))
                {
                    // [DataEntry:920]
                    // When in the process of validating, prevent IndicateActive(false) from
                    // programmatically ending edit mode as this will result in null object
                    // reference exceptions
                    _preventProgrammaticEndEdit = true;
                    base.OnValidating(e);
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI29181", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
            finally
            {
                _preventProgrammaticEndEdit = false;
            }
        }

        /// <summary>
        /// Raises the <see cref="DataGridView.CellPainting"/> event.
        /// </summary>
        /// <param name="e">A <see cref="DataGridViewCellPaintingEventArgs"/> that contains the
        /// event data. </param>
        protected override void OnCellPainting(DataGridViewCellPaintingEventArgs e)
        {
            try
            {
                bool paintWarningIcon = false;
                IDataEntryTableCell dataEntryCell = null;

                // If the error icon is set to be drawn
                if (((e.PaintParts & DataGridViewPaintParts.ErrorIcon) != 0) &&
                    !string.IsNullOrEmpty(e.ErrorText))
                {
                    dataEntryCell = Rows[e.RowIndex].Cells[e.ColumnIndex] as IDataEntryTableCell;

                    // If the cell in question has a validation warning associated with it, paint
                    // the warning icon instead of the error icon.
                    if (dataEntryCell != null && dataEntryCell.Attribute != null &&
                        (AttributeStatusInfo.GetDataValidity(dataEntryCell.Attribute) ==
                            DataValidity.ValidationWarning))
                    {
                        paintWarningIcon = true;
                    }
                }

                if (paintWarningIcon)
                {
                    // Allow the base class to paint everything except the error icon.
                    e.Paint(e.ClipBounds, e.PaintParts ^ DataGridViewPaintParts.ErrorIcon);

                    // Calculate the position of the error icon to be drawn.
                    Rectangle bounds = dataEntryCell.AsDataGridViewCell.ErrorIconBounds;
                    bounds.Offset(e.CellBounds.Location);

                    // The warning icon does not have a recognizable exclamation point if it is
                    // not drawn somewhat bigger than ErrorIconBounds.
                    bounds.Inflate(2, 2);

                    // Set high-quality settings.
                    GraphicsState origState = e.Graphics.Save();
                    e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    e.Graphics.CompositingQuality = CompositingQuality.HighQuality;
                    e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
                    e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                    e.Graphics.DrawIcon(SystemIcons.Warning, bounds);

                    // Restore previous settings.
                    e.Graphics.Restore(origState);

                    // Indicate to base class that the painting has been taken care of.
                    e.Handled = true;
                }

                base.OnCellPainting(e);
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI28982", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the case that the font has changed in order to update the pre-defined cell
        /// styles the DataEntry cells use.
        /// </summary>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        protected override void OnFontChanged(EventArgs e)
        {
            try
            {
                base.OnFontChanged(e);

                // Re-create the fonts used by the cell styles.
                _regularFont.Dispose();
                _regularFont = new Font(DefaultCellStyle.Font, FontStyle.Regular);
                
                _boldFont.Dispose();
                _boldFont = new Font(DefaultCellStyle.Font, FontStyle.Bold);

                // Update the styles to use the new font.
                InitializeCellStyles();
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI29643", ex);
            }
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
                // Delete the contents of all selected cells if the delete key was pressed
                if (e.KeyCode == Keys.Delete)
                {
                    // [DataEntry:641]
                    // Clear the contents as well as the spatial info of all selected cells.
                    DeleteSelectedCellContents();

                    return true;
                }

                keyProcessed = base.ProcessDataGridViewKey(e);
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI29663", ex);
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
                // If the delete key is pressed while a combo cell is selected or all text
                // is selected in a text box cell, delete spatial info.
                if (keyData == Keys.Delete && EditingControl != null)
                {
                    IDataEntryTableCell dataEntryCell = CurrentCell as IDataEntryTableCell;

                    if (dataEntryCell != null)
                    {
                        DataGridViewTextBoxEditingControl textBoxEditingControl =
                            EditingControl as DataGridViewTextBoxEditingControl;

                        if (textBoxEditingControl == null ||
                            textBoxEditingControl.SelectionLength == textBoxEditingControl.Text.Length)
                        {
                            DeleteSelectedCellContents();
                        }
                    }
                }

                keyProcessed = base.ProcessDialogKey(keyData);
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI29662", ex);
            }

            return keyProcessed;
        }

        /// <summary>
        /// Handles the case that there was an error displaying data in a table cell. Any error that
        /// would display is converted to and displayed as an <see cref="ExtractException"/>.
        /// </summary>
        /// <param name="displayErrorDialogIfNoHandler">true to display an error dialog box if there
        /// is no handler for the <see cref="E:System.Windows.Forms.DataGridView.DataError"/> event.
        /// </param>
        /// <param name="e">A <see cref="T:System.Windows.Forms.DataGridViewDataErrorEventArgs"/>
        /// that contains the event data.</param>
        protected override void OnDataError(bool displayErrorDialogIfNoHandler, DataGridViewDataErrorEventArgs e)
        {
            try
            {
                // Call the base OnDataError with false to prevent the error from displaying.
                base.OnDataError(false, e);

                // Create and display an ExtractExceptions using the data from the event args.
                var ee = e.Exception.AsExtract("ELI35074");
                DataGridViewCell cell = Rows[e.RowIndex].Cells[e.ColumnIndex];
                ee.AddDebugData("Value", cell.Value.ToString(), false);
                ee.AddDebugData("Row index", e.RowIndex, false);
                ee.AddDebugData("Column index", e.ColumnIndex, false);
                ee.AddDebugData("Context", e.Context.ToString(), false);
                
                // [DataEntry:1290]
                // For the 9.6 release only, this is being changed from a display into a log. For
                // any subsequent release, an as-yet-to-be-implemented system that allows for
                // exceptions to be displayed internally but not at customer sites should be used.
                ee.Log();
            }
            catch (Exception ex)
            {
                // [DataEntry:1290]
                // For the 9.6 release only, this is being changed from a display into a log. For
                // any subsequent release, an as-yet-to-be-implemented system that allows for
                // exceptions to be displayed internally but not at customer sites should be used.
                ex.ExtractLog("ELI35075");
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.DataGridView.ColumnStateChanged"/> event.
        /// </summary>
        /// <param name="e">A <see cref="DataGridViewColumnStateChangedEventArgs"/> that contains
        /// the event data.</param>
        /// <exception cref="T:System.InvalidCastException">The column changed from read-only to
        /// read/write, enabling the current cell to enter edit mode, but the
        /// <see cref="P:DataGridViewCell.EditType"/> property of the current cell does not indicate
        /// a class that derives from <see cref="T:Control"/> and implements
        /// <see cref="T:IDataGridViewEditingControl"/>.</exception>
        protected override void OnColumnStateChanged(DataGridViewColumnStateChangedEventArgs e)
        {
            try
            {
                base.OnColumnStateChanged(e);

                // https://extract.atlassian.net/browse/ISSUE-12812
                // If the visible status of a column is changed after document load, the viewable
                // status of the affected attributes needs to be updated so that highlights and
                // validation are applied correctly.
                if (e.StateChanged == DataGridViewElementStates.Visible &&
                    DataEntryControlHost != null && !DataEntryControlHost.ChangingData)
                {
                    foreach (IAttribute attribute in Rows.OfType<DataGridViewRow>()
                        .Select(row => row.Cells[e.Column.Index]).OfType<IDataEntryTableCell>()
                        .Select(cell => cell.Attribute)
                        .Where(attribute => attribute != null))
                    {
                        bool viewable = Visible && e.Column.Visible;
                        AttributeStatusInfo.MarkAsViewable(attribute, viewable);
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI37913");
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Control.VisibleChanged"/> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.
        /// </param>
        protected override void OnVisibleChanged(EventArgs e)
        {
            try
            {
                base.OnVisibleChanged(e);

                // https://extract.atlassian.net/browse/ISSUE-12812
                // If the visibility of the table is changed after document load, the viewable
                // status of the attributes needs to be updated so that highlights and validation
                // are applied correctly.
                if (DataEntryControlHost != null && !DataEntryControlHost.ChangingData)
                {
                    foreach (IDataEntryTableCell cell in Rows.OfType<DataGridViewRow>()
                        .SelectMany(row => row.Cells.OfType<IDataEntryTableCell>())
                        .Where(cell => cell.Attribute != null))
                    {
                        bool viewable = Visible && cell.AsDataGridViewCell.OwningColumn.Visible;
                        AttributeStatusInfo.MarkAsViewable(cell.Attribute, viewable);
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI37914");
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
        /// <see cref="IAttribute"/> having been modified (i.e., via a swipe or loading a document) or
        /// the result of a new selection in a multi-attribute table. The event will provide the 
        /// updated <see cref="IAttribute"/>(s) to registered listeners.
        /// </summary>
        /// <seealso cref="IDataEntryControl"/>
        public event EventHandler<AttributesEventArgs> PropagateAttributes;

        /// <summary>
        /// Fired when the table has been manipulated in such a way that swiping should be
        /// either enabled or disabled.
        /// </summary>
        /// <seealso cref="IDataEntryControl"/>
        public event EventHandler<SwipingStateChangedEventArgs> SwipingStateChanged;

        /// <summary>
        /// Raised whenever data is being dragged to query dependent controls on whether they would
        /// be able to handle the dragged data if it was dropped.
        /// </summary>
        /// <seealso cref="IDataEntryControl"/>
        public event EventHandler<QueryDraggedDataSupportedEventArgs> QueryDraggedDataSupported;

        /// <summary>
        /// Indicates that a control has begun an update and that the
        /// <see cref="DataEntryControlHost"/> should not redraw highlights, etc, until the update
        /// is complete.
        /// <para><b>NOTE:</b></para>
        /// This event should only be raised for updates that initiated via user interaction with
        /// the control. It should not be raised for updates triggered by the
        /// <see cref="DataEntryControlHost"/> such as <see cref="ProcessSwipedText"/>.
        /// </summary>
        public event EventHandler<EventArgs> UpdateStarted;

        /// <summary>
        /// Indicates that a control has ended an update and actions that needs to be taken by the
        /// <see cref="DataEntryControlHost"/> such as re-drawing highlights can now proceed.
        /// </summary>
        public event EventHandler<EventArgs> UpdateEnded;

        /// <summary>
        /// Gets or sets the <see cref="DataEntryControlHost"/> to which this control belongs
        /// </summary>
        /// <value>The <see cref="DataEntryControlHost"/> to which this control belongs.</value>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public DataEntryControlHost DataEntryControlHost
        {
            get
            {
                return _dataEntryControlHost;
            }
            set
            {
                try
                {
                    if (_dataEntryControlHost != value)
                    {
                        _dataEntryControlHost = value;
                        _luceneAutoSuggest?.SetDataEntryControlHost(value);
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI50225");
                }
            }
        }

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
        public virtual IDataEntryControl ParentDataEntryControl
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
        /// Gets or sets whether the control should remain disabled at all times.
        /// <para><b>Note</b></para>
        /// If disabled, mapped data will not be validated.
        /// </summary>
        /// <value><see langword="true"/> if the control should remain disabled,
        /// <see langword="false"/> otherwise.</value>
        /// <returns><see langword="true"/> if the control will remain disabled,
        /// <see langword="false"/> otherwise.</returns>
        [Category("Data Entry Control")]
        [DefaultValue(false)]
        public bool Disabled
        {
            get
            {
                return _disabled;
            }

            set
            {
                _disabled = value;
            }
        }

        /// <summary>
        /// Gets or sets whether the clipboard contents should be cleared after pasting into the
        /// control.
        /// </summary>
        /// <value><see langword="true"/> if the clipboard should be cleared after pasting,
        /// <see langword="false"/> otherwise.</value>
        [Category("Data Entry Control")]
        [DefaultValue(false)]
        public bool ClearClipboardOnPaste
        {
            get
            {
                return _clearClipboardOnPaste;
            }

            set
            {
                _clearClipboardOnPaste = value;
            }
        }

        /// <summary>
        /// Gets or sets whether descendant attributes in other controls should be highlighted.
        /// </summary>
        /// <value><see langword="true"/> if descendant attributes should be highlighted when this
        /// table is selected; <see langword="false"/> otherwise.</value>
        [Category("Data Entry Control")]
        [DefaultValue(true)]
        public bool HighlightSelectionInChildControls
        {
            get
            {
                return _highlightSelectionInChildControls;
            }

            set
            {
                _highlightSelectionInChildControls = value;
            }
        }

        /// <summary>
        /// Activates or inactivates the <see cref="DataEntryTable"/>.
        /// <para><b>Requirements:</b></para>
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
                // Ensure edit mode ends once this control is no longer active.
                if (!setActive && !_preventProgrammaticEndEdit && EditingControl != null)
                {
                    EndEditNoFocus();
                }

                // The table should be displayed as active only if it is editable.
                _isActive = setActive && !ReadOnly;
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
                foreach (DataGridViewCell cell in SelectedCells)
                {
                    UpdateCellStyle(cell);
                }

                // [DataEntry:148]
                // Processing needs to take place to mark selected cells as read and derived classes
                // likely will need to raise events such as SwipingStateChanged based on the current
                // selection. OnSelectionChanged encompass the processing that needs to happen,
                // however the windows events are ordered such that a click in the table will trigger
                // GotFocus (and, thus, IndicateActive) before the mouse click is processed which
                // may change the current selection. This means doing the processing now may
                // inappropriately mark cells as read. Therefore, use BeginInvoke to place a call to
                // OnSelectionChange on the control's message queue that will only be called after
                // the other events already on the queue (such as a mouse click) are processed.
                if (setActive)
                {
                    BeginInvoke(new EventArgsDelegate(OnSelectionChanged),
                        new object[] { new EventArgs() });
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI24209", ex);
            }
        }

        /// <summary>
        /// Commands a multi-selection control to propagate the specified <see cref="IAttribute"/>
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
        /// <param name="selectTabGroup">If <see langword="true"/> all <see cref="IAttribute"/>s in
        /// the specified <see cref="IAttribute"/>'s tab group are to be selected,
        /// <see langword="false"/> otherwise.</param>
        /// <seealso cref="IDataEntryControl"/>
        public virtual void PropagateAttribute(IAttribute attribute, bool selectAttribute,
            bool selectTabGroup)
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

                // https://extract.atlassian.net/browse/ISSUE-13005
                // In the case of the DataEntryTwoColumnTable, the primary attribute may not end up
                // getting mapped to a row in which it will be mapped to this control in general. In
                // this case, there is no particular selection to make.
                if (_attributeMap[attribute] == this)
                {
                    ProcessSelectionChange();
                    return;
                }

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
                                foreach (var cell in row.Cells.OfType<IDataEntryTableCell>())
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
                                    ClearSelection(-1, row.Index, true);
                                }
                                else
                                {
                                    // Select the cell containing
                                    ClearSelection(newCurrentCell.ColumnIndex, row.Index, true);
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
                        DataGridViewCell newCurrentCell = (DataGridViewCell)_attributeMap[attribute];

                        // Clear all selections first since sometimes trying to select an individual cell 
                        // in a selected row doesn't clear the row selection otherwise.
                        ClearSelection();

                        // Make sure the cell is visible before making it the current cell
                        if (newCurrentCell.Visible)
                        {
                            base.CurrentCell = newCurrentCell;

                            // Update the selection to just the current cell
                            ClearSelection(base.CurrentCell.ColumnIndex, base.CurrentCell.RowIndex, true);
                        }
                        else
                        {
                            ClearSelection();

                            // Update the selection to include the entire row.
                            ClearSelection(-1, newCurrentCell.RowIndex, true);
                        }

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
        /// Refreshes the specified <see cref="IAttribute"/>s' values to the table.
        /// </summary>
        /// <param name="spatialInfoUpdated"><see langword="true"/> if the attribute's spatial info
        /// has changed so that hints can be updated; <see langword="false"/> if the attribute's
        /// spatial info has not changed.</param>
        /// <param name="attributes">The <see cref="IAttribute"/>s whose values should be refreshed.
        /// </param>
        public virtual void RefreshAttributes(bool spatialInfoUpdated, params IAttribute[] attributes)
        {
            try
            {
                bool refreshedAttribute = false;

                foreach (IAttribute attribute in attributes)
                {
                    // If attribute refreshes are being suppressed, don't refresh now but keep track
                    // of which attributes should be refreshed later.
                    if (_suppressRefreshReferenceCount > 0)
                    {
                        _pendingRefreshAttributes.Add(attribute);
                        _pendingSpatialRefresh |= spatialInfoUpdated;
                        continue;
                    }

                    object tableElement;
                    if (_attributeMap.TryGetValue(attribute, out tableElement))
                    {
                        DataGridViewCell cell = tableElement as DataGridViewCell;
                        if (cell != null)
                        {
                            // Don't refresh the value if the value hasn't actually changed. Doing so is not
                            // only in-efficient but it can cause un-intended side effects if an
                            // auto-complete list is active.
                            if (cell.Value.ToString() != attribute.Value.String)
                            {
                                cell.Value = attribute.Value.String;
                            }
                            else
                            {
                                // While the value hasn't changed, it could now be invalid due to a
                                // change in another field that affects a validation query for this one.
                                ValidateCell((IDataEntryTableCell)cell, false);
                            }

                            // If the cell is in edit mode, the value needs to be applied to the edit
                            // control as well.
                            if (cell.IsInEditMode && _editingControl != null &&
                                _editingControl.Text != attribute.Value.String)
                            {
                                cell.DataGridView.RefreshEdit();
                            }
                            
                            // Consider the attribute refreshed even if the text value didn't change.
                            refreshedAttribute = true;
                        }
                    }
                }

                // [DataEntry:547] Update hints if the spatial info has changed.
                if (refreshedAttribute && spatialInfoUpdated)
                {
                    UpdateHints(true);
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI26121", ex);
            }
        }

        /// <summary>
        /// Gets the UI element associated with the specified <see paramref="attribute"/>. This may
        /// be a type of <see cref="Control"/> or it may also be <see cref="DataGridViewElement"/>
        /// such as a <see cref="DataGridViewCell"/> if the <see paramref="attribute"/>'s owning
        /// control is a table control.
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> for which the UI element is needed.
        /// </param>
        /// <returns>The UI element</returns>
        public object GetAttributeUIElement(IAttribute attribute)
        {
            try
            {
                ExtractException.Assert("ELI37289", "Null argument exception", attribute != null);

                object tableElement;
                if (_attributeMap.TryGetValue(attribute, out tableElement))
                {
                    DataGridViewCell cell = tableElement as DataGridViewCell;
                    if (cell != null)
                    {
                        return cell;
                    }

                    // Attempt to interpret the item as a DataEntryTableRow to get its Attribute.
                    DataEntryTableRow row = tableElement as DataEntryTableRow;
                    if (row != null)
                    {
                        return row;
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI37291");
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
        /// <param name="e">An <see cref="AttributesEventArgs"/> that contains the event data.
        /// </param>
        /// <seealso cref="IDataEntryControl"/>
        public virtual void HandlePropagateAttributes(object sender, AttributesEventArgs e)
        {
            try
            {
                // An attribute can be mapped if there is one and only one attribute to be
                // propagated.
                if (e.Attributes != null && e.Attributes.Size() == 1)
                {
                    Enabled = !_disabled;

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

        /// <summary>
        /// Applies the selection state represented by <see paramref="selectionState"/> to the
        /// control.
        /// </summary>
        /// <param name="selectionState">The <see cref="SelectionState"/> to apply.</param>
        public void ApplySelection(Extract.DataEntry.SelectionState selectionState)
        {
            try
            {
                SelectionState tableSelectionState =
                    selectionState as DataEntryTableBase.SelectionState;

                if (tableSelectionState != null)
                {
                    using (new SelectionProcessingSuppressor(this))
                    {
                        EndEdit();
                        ClearSelection();

                        // Set the current cell before setting selection, otherwise setting the
                        // current cell my undo intended selections.
                        if (tableSelectionState.CurrentCellPosition != null)
                        {
                            CurrentCell = Rows[tableSelectionState.CurrentCellPosition.Item1].
                                Cells[tableSelectionState.CurrentCellPosition.Item2];
                        }

                        foreach (Tuple<int, int> cellIndexes in tableSelectionState.SelectedCellLocations)
                        {
                            Rows[cellIndexes.Item1].Cells[cellIndexes.Item2].Selected = true;
                        }

                        foreach (int rowIndex in tableSelectionState.SelectedRowIndexes)
                        {
                            Rows[rowIndex].Selected = true;
                        }

                        foreach (int columnIndex in tableSelectionState.SelectedColumnIndexes)
                        {
                            Columns[columnIndex].Selected = true;
                        }
                    }

                    ProcessSelectionChange();
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI31011", ex);
            }
        }

        /// <summary>
        /// Creates a <see cref="BackgroundFieldModel"/> for representing this control during
        /// a background data load.
        /// </summary>
        public virtual BackgroundFieldModel GetBackgroundFieldModel()
        {
            return null;
        }

        #endregion IDataEntryControl Members

        #region Abstract IDataEntryControl Members

        /// <summary>
        /// Gets or sets whether the table accepts as input the <see cref="SpatialString"/>
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
        /// <returns><see langword="true"/> if the control was able to use the swiped text;
        /// <see langword="false"/> if it could not be used.</returns>
        /// <seealso cref="IDataEntryControl"/>
        public abstract bool ProcessSwipedText(SpatialString swipedText);

        /// <summary>
        /// Any data that was cached should be cleared;  This is called when a document is unloaded.
        /// If controls fail to clear COM objects, errors may result if that data is accessed when
        /// a subsequent document is loaded.
        /// </summary>
        public abstract void ClearCachedData();

        /// <summary>
        /// Requests that the <see cref="IDataEntryControl"/> refresh all <see cref="IAttribute"/>
        /// values to the screen.
        /// </summary>
        public abstract void RefreshAttributes();

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
                try
                {
                    // https://extract.atlassian.net/browse/ISSUE-12527
                    // Based on exception encountered in the FFI to do with edit mode, adding same code
                    // here to protect against a crash when disposing.
                    if (!IsDisposed && IsCurrentCellInEditMode)
                    {
                        EndEdit();
                    }

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

                    if (_luceneAutoSuggest != null)
                    {
                        _luceneAutoSuggest.Dispose();
                        _luceneAutoSuggest = null;
                    }

                } catch { }
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
                // Only mark a cell is viewed if  it is the only selected cell and that cell is a
                // IDataEntryTableCell.
                if (_isActive && SelectedCells.Count == 1)
                {
                    IDataEntryTableCell cell = SelectedCells[0] as IDataEntryTableCell;
                    if (cell != null && cell.Attribute != null)
                    {
                        if (!_dragOverInProgress)
                        {
                            AttributeStatusInfo.MarkAsViewed(cell.Attribute, true);
                        }

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
        /// Deletes the attributes that aren't needed anymore (via AttributeStatusInfo), and ensures
        /// the deleted attribute is no longer referenced by any table member.
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> to be deleted.</param>
        protected virtual void DeleteAttributeData(IAttribute attribute)
        {
            // If the attribute being deleted is the currently propagated attribute, propagate null
            // to clear dependent controls.
            if (_currentlyPropagatedAttribute == attribute)
            {
                OnPropagateAttributes(null);
            }

            AttributeStatusInfo.DeleteAttribute(attribute);
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
        /// <param name="selectedGroupAttribute">The <see cref="IAttribute"/> representing a
        /// currently selected row or group. <see langword="null"/> if no row or group is selected.
        /// </param>
        protected void OnAttributesSelected(IUnknownVector attributes,
            bool includeSubAttributes, bool displayTooltips, IAttribute selectedGroupAttribute)
        {
            if (AttributesSelected != null)
            {
                var selectionState = new SelectionState(this, attributes, includeSubAttributes,
                    displayTooltips, selectedGroupAttribute);
                AttributesSelected(this, new AttributesSelectedEventArgs(selectionState));
            }
        }

        /// <summary>
        /// Raises the <see cref="PropagateAttributes"/> event.
        /// </summary>
        /// <param name="attributes">The set of attributes that have been updated.</param>
        protected void OnPropagateAttributes(IUnknownVector attributes)
        {
            if (PropagateAttributes != null)
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

                    PropagateAttributes(this, new AttributesEventArgs(attributes));
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
            if (SwipingStateChanged != null)
            {
                SwipingStateChanged(this, new SwipingStateChangedEventArgs(SupportsSwiping));
            }
        }

        /// <summary>
        /// Raises the <see cref="QueryDraggedDataSupported"/> supported event to query dependent
        /// controls on whether they would be able to handle the dragged data if it was dropped.
        /// The initial Effect parameter of DragEventArgs should be set to
        /// <see cref="DragDropEffects.None"/> before this method is called. If any dependent
        /// controls are able to handle dropped data, the Effect parameter will be updated with the
        /// drop options currently supported by one or more dependent controls.
        /// </summary>
        /// <param name="e">The <see cref="DragEventArgs"/> associated with the drag event.</param>
        protected void OnQueryDraggedDataSupported(DragEventArgs e)
        {
            if (QueryDraggedDataSupported != null)
            {
                QueryDraggedDataSupported(this, new QueryDraggedDataSupportedEventArgs(e));
            }
        }

        /// <summary>
        /// Raises the <see cref="UpdateStarted"/> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs"/> associated with the drag event.</param>
        protected void OnUpdateStarted(EventArgs e)
        {
            if (UpdateStarted != null)
            {
                UpdateStarted(this, e);
            }
        }

        /// <summary>
        /// Raises the <see cref="UpdateEnded"/> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs"/> associated with the drag event.</param>
        protected void OnUpdateEnded(EventArgs e)
        {
            if (UpdateEnded != null)
            {
                UpdateEnded(this, e);
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

                object mappedElement = null;
                if (!_attributeMap.TryGetValue(attribute, out mappedElement) || mappedElement != tableElement)
                {
                    if (mappedElement != null)
                    {
                        UnMapAttribute(attribute, clearCellAttributes: true);
                    }

                    // Since the spatial information for this table has likely changed, refresh all
                    // spatial hints for this table.
                    _hintsAreDirty = true;

                    _attributeMap[attribute] = tableElement;

                    // Register for the AttributeDeleted event so the attribute is removed from the
                    // map once it is deleted. It is important to do this because FinalReleaseComObject
                    // will be called on the attribute after deletion to prevent handle leaks.
                    AttributeStatusInfo statusInfo = AttributeStatusInfo.GetStatusInfo(attribute);
                    statusInfo.AttributeDeleted += HandleAttributeDeleted;

                    // Only attributes mapped to a IDataEntryTableCell will be viewable.
                    IDataEntryTableCell dataEntryCell = tableElement as IDataEntryTableCell;

                    // Mark the attribute as visible if the table is visible and the table element
                    // is a cell (as opposed to a row which isn't visible on its own)
                    if (dataEntryCell != null)
                    {
                        // Register to receive notification that the spatial info for the cell has
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
        /// Un-maps a <see cref="IAttribute"/> from a table element.
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> to un-map.</param>
        /// <param name="clearCellAttributes"><see langword="true"/> to remove the reference
        /// to each cell's <see cref="IAttribute"/>, <see langword="false"/> otherwise.</param>
        // I can't find a way to case "UnMap" in a way that makes FX cop happy.
        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Un")]
        protected void UnMapAttribute(IAttribute attribute, bool clearCellAttributes)
        {
            try
            {
                object tableElement;
                if (_attributeMap.TryGetValue(attribute, out tableElement))
                {
                    _attributeMap.Remove(attribute);

                    // Unregister from the HandleAttributeDeleted event.
                    AttributeStatusInfo statusInfo = AttributeStatusInfo.GetStatusInfo(attribute);
                    statusInfo.AttributeDeleted -= HandleAttributeDeleted;

                    // If the attribute was mapped to a dataEntryCell, clear the cell's attribute
                    // property to ensure it is no longer referenced and unregister the 
                    // HandleCellSpatialInfoChanged that was previously registered.
                    IDataEntryTableCell dataEntryCell = tableElement as IDataEntryTableCell;
                    if (dataEntryCell != null)
                    {
                        if (clearCellAttributes)
                        {
                            dataEntryCell.Attribute = null;
                        }

                        dataEntryCell.CellSpatialInfoChanged -= HandleCellSpatialInfoChanged;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI27848", ex);
            }
        }

        /// <summary>
        /// Clears <see cref="DataEntryTableBase"/>'s internal attribute map and resets selection
        /// to the first displayed table cell.
        /// </summary>
        /// <param name="clearCellAttributes"><see langword="true"/> to remove the reference
        /// to each cell's <see cref="IAttribute"/>, <see langword="false"/> otherwise.</param>
        protected void ClearAttributeMappings(bool clearCellAttributes)
        {
            try
            {
                // Ensure hints are recalculated next time.
                _hintsAreDirty = true;

                // Clear the attribute map
                List<IAttribute> attributeList = new List<IAttribute>(_attributeMap.Keys);
                foreach (IAttribute attribute in attributeList)
                {
                    UnMapAttribute(attribute,clearCellAttributes);
                }

                // Reset selection back to the first displayed cell.
                base.CurrentCell = FirstDisplayedCell;
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
        /// <param name="forceRecalculation">If <see langword="true"/>, re-calculate all hints.
        /// If <see langword="false"/>, hints will only be recalculated if they are dirty.</param>
        protected void UpdateHints(bool forceRecalculation)
        {
            List<IAttribute> autoPopulatedAttributes;
            UpdateHints(forceRecalculation, null, out autoPopulatedAttributes);
        }

        /// <summary>
        /// As long as spatial info associated with the table has changed since the last time hints
        /// were generated, spatial hints will be generated according to the table properties for
        /// all <see cref="IAttribute"/>s in the table that are lacking spatial information.
        /// </summary>
        /// <param name="forceRecalculation">If <see langword="true"/>, re-calculate all hints.
        /// If <see langword="false"/>, hints will only be recalculated if they are dirty.</param>
        /// <param name="allowAutoPopulateForRow">If not <see langword="null"/>, any OCR text found
        /// within a smart hint region will be used to populate attributes in the specified row
        /// number only.</param>
        /// <param name="autoPopulatedAttributes">Returns whether or not any attribute values were auto-
        /// populated in the row indicated by <see paramref="allowAutoPopulateForRow"/>.</param>
        // Out parameter used instead of return value since it would otherwise not be obvious
        // that the return value is associated only with whether data was auto-populated.
        [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "2#")]
        protected void UpdateHints(bool forceRecalculation, int? allowAutoPopulateForRow,
            out List<IAttribute> autoPopulatedAttributes)
        {
            try
            {
                autoPopulatedAttributes = new List<IAttribute>();

                if (forceRecalculation || _hintsAreDirty)
                {
                    _spatialHintGenerator.ClearHintCache();

                    // Compile all rows that are to be used to generate SmartHints
                    List<DataGridViewRow> smartHintRows = new List<DataGridViewRow>();
                    foreach (DataGridViewRow row in Rows)
                    {
                        DataEntryTableRow dataEntryRow = row as DataEntryTableRow;
                        if (dataEntryRow != null && dataEntryRow.SmartHintsEnabled)
                        {
                            smartHintRows.Add(row);
                        }
                    }

                    // Compile all columns that are to be used to generate SmartHints
                    List<DataGridViewColumn> smartHintColumns = new List<DataGridViewColumn>();
                    foreach (DataGridViewColumn column in Columns)
                    {
                        DataEntryTableColumn dataEntryColumn = column as DataEntryTableColumn;
                        if (dataEntryColumn != null && dataEntryColumn.SmartHintsEnabled)
                        {
                            smartHintColumns.Add(column);
                        }
                    }

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
                            bool autoPopulatedData;
                            CreateSpatialHint(attribute, cell, smartHintRows, smartHintColumns,
                                allowAutoPopulateForRow.HasValue && allowAutoPopulateForRow.Value == cell.RowIndex,
                                out autoPopulatedData);

                            if (autoPopulatedData)
                            {
                                autoPopulatedAttributes.Add(attribute);
                            }
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

        /// <summary>
        /// Clears the contents as well as the spatial info of all selected cells.
        /// </summary>
        protected void DeleteSelectedCellContents()
        {
            try
            {
                // Attributes whose spatial info has been cleared.
                List<IAttribute> clearedAttributes = new List<IAttribute>();

                // Delay screen redraw until all cells are processed.
                OnUpdateStarted(new EventArgs());

                foreach (DataGridViewCell cell in SelectedCells)
                {
                    // Clear the value of each selected cell.
                    cell.Value = "";

                    // Remove the spatial info of each selected cell.
                    IDataEntryTableCell dataEntryCell = cell as IDataEntryTableCell;
                    if (dataEntryCell != null && dataEntryCell.Attribute != null)
                    {
                        AttributeStatusInfo.RemoveSpatialInfo(dataEntryCell.Attribute);
                        clearedAttributes.Add(dataEntryCell.Attribute);
                    }
                }

                // While AttributeStatusInfo.SetValue() is called as a consequence of the
                // cell.Value = "" call above, it will register only as an incremental change that
                // will not trigger query execution. This call should constitute an end-of-edit.
                AttributeStatusInfo.EndEdit();

                // As long as spatial info was removed for at least one attribute,
                // refresh those attributes.
                if (clearedAttributes.Count > 0)
                {
                    RefreshAttributes(true, clearedAttributes.ToArray());
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI27322", ex);
            }
            finally
            {
                OnUpdateEnded(new EventArgs());
            }
        }

        /// <summary>
        /// Executes a refresh of all attributes whose refreshes had been suppressed.
        /// Created in response to DataEntry:1188 in which refreshes in the middle of applying
        /// attribute data caused problems.
        /// </summary>
        protected void ExecutePendingRefresh()
        {
            try
            {
                // If refreshes are still being suppressed, there is nothing yet to do.
                if (_suppressRefreshReferenceCount > 0)
                {
                    return;
                }

                IAttribute[] attributesToRefresh = _pendingRefreshAttributes
                        .Where(attribute => _attributeMap.Keys.Contains(attribute))
                        .ToArray();

                RefreshAttributes(_pendingSpatialRefresh, attributesToRefresh);

                _pendingRefreshAttributes.Clear();
                _pendingSpatialRefresh = false;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI35385");
            }
        }

        /// <summary>
        /// Achieves same result as calling <see cref="EndEdit"/>, but without grabbing focus.
        /// </summary>
        protected void EndEditNoFocus()
        {
            try
            {
                // Using reflector I was able to find a codepath that called the private
                // DataGridView.EndEdit with the "keepFocus" parameter as false if the current cell
                // is updated while EditMode is EditOnEnter. 
                var editMode = EditMode;
                var currentCell = CurrentCell;
                EditMode = DataGridViewEditMode.EditOnEnter;
                CurrentCell = null;
                CurrentCell = currentCell;
                EditMode = editMode;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI44709");
            }
        }

        #endregion Protected Members

        #region ISupportInitialize

        /// <summary>
        /// Signals the object that initialization is complete.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        void ISupportInitialize.EndInit()
        {
            try
            {
                // Especially for tables that single row, make rows fit within table nicely.
                if (ClientSize.Height - 2 < RowTemplate.Height)
                {
                    RowTemplate.Height = ClientSize.Height - 2;
                }

                InterfaceMapping interfaceMapping = typeof(DataGridView).GetInterfaceMap(typeof(ISupportInitialize));
                MethodInfo endInitMethod = interfaceMapping.TargetMethods.Single(
                    m => m.Name.EndsWith("EndInit", StringComparison.Ordinal));
                endInitMethod.Invoke(this, new object[0]);
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI41646");
            }
        }

        #endregion ISupportInitialize

        #region Event Handlers

        /// <summary>
        /// Handles the case that text has been modified via the active editing control.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
        void HandleCellTextChanged(object sender, EventArgs e)
        {
            try
            {
                // Only IDataEntryTableCells need to be monitored for edits.
                IDataEntryTableCell dataEntryCell = base.CurrentCell as IDataEntryTableCell;
                if (dataEntryCell != null && dataEntryCell.Attribute != null &&
                    base.CurrentCell.Value.ToString() != _editingControl.Text)
                {
                    if (AutoCompleteMode == DataEntryAutoCompleteMode.SuggestLucene)
                    {
                        _luceneAutoSuggest?.SetText(_editingControl.Text, ignoreNextTextChangedEvent: false);
                    }
                    // Since DataGridViewCells are not normally modified in real-time as text is
                    // changed, apply changes from the editing control to the cell here.
                    base.CurrentCell.Value = _editingControl.Text;
                }

                // Set AutoCompleteCell depending on whether the auto-complete is active for the
                // current cell.
                if (AutoCompleteCell == null && FormsMethods.IsAutoCompleteDisplayed())
                {
                    AutoCompleteCell = CurrentCell;
                }
                else if (AutoCompleteCell != null && !FormsMethods.IsAutoCompleteDisplayed())
                {
                    AutoCompleteCell = null;
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
        void HandleCellSelectedIndexChanged(object sender, EventArgs e)
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
        void HandleCellSpatialInfoChanged(object sender, CellSpatialInfoChangedEventArgs e)
        {
            try
            {
                if (Visible)
                {
                    // Since the spatial information for this table has changed, refresh all spatial
                    // hints for this table.
                    _hintsAreDirty = true;
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI25231", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles any <see cref="Control.PreviewKeyDown"/> events raised by an active
        /// <see cref="DataGridViewTextBoxEditingControl"/> in order to prevent a situation
        /// where the up and down arrows can apparently cause buffer overrun problems.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">An <see cref="PreviewKeyDownEventArgs"/> that contains the event data.
        /// </param>
        void HandleEditingControlPreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            try
            {
                // [DataEntry:385]
                // If the up or down arrow keys are pressed while the cursor is at the end of the
                // text and the auto-complete list is not currently displayed, end edit mode and
                // manually change the selected cell based on the arrow key as should happen. This
                // prevents apparent memory issues with auto-complete that can otherwise cause
                // garbage characters to appear at the end of the field.
                if ((e.KeyCode == Keys.Up || e.KeyCode == Keys.Down) && EditingControl != null)
                {
                    DataGridViewTextBoxEditingControl textEditingControl =
                                (DataGridViewTextBoxEditingControl)EditingControl;

                    if (textEditingControl.AutoCompleteMode != System.Windows.Forms.AutoCompleteMode.None &&
                        textEditingControl.SelectionStart == textEditingControl.Text.Length &&
                        !FormsMethods.IsAutoCompleteDisplayed())
                    {
                        // It is important to turn off auto-complete here. Though ending the edit
                        // mode prevents the user from seeing corrupted characters, gflags indicates
                        // that memory has still been stomped on. If auto-complete is disabled
                        // first, gflags does not detect any memory corruption.
                        textEditingControl.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.None;

                        // Ensure the arrow key changes cell selection in this circumstance by first
                        // ending edit mode...
                        EndEdit();

                        // ... and then manually specifying the new CurrentCell based on the arrow
                        // key pressed.
                        if (e.KeyCode == Keys.Down)
                        {
                            if (CurrentCell.RowIndex < Rows.Count - 1)
                            {
                                CurrentCell = Rows[CurrentCell.RowIndex + 1]
                                    .Cells[CurrentCell.ColumnIndex];
                            }
                        }
                        else if (e.KeyCode == Keys.Up)
                        {
                            if (CurrentCell.RowIndex > 0)
                            {
                                CurrentCell = Rows[CurrentCell.RowIndex - 1]
                                    .Cells[CurrentCell.ColumnIndex];
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI27646", ex);
                ee.AddDebugData("Event data", e, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Handles the <see cref="AttributeStatusInfo.AttributeDeleted"/> event.
        /// </summary>
        /// <param name="sender">The object that sent the event.</param>
        /// <param name="e">An <see cref="AttributeDeletedEventArgs"/> that contains the event data.
        /// </param>
        void HandleAttributeDeleted(object sender, AttributeDeletedEventArgs e)
        {
            try
            {
                // Remove any deleted attributes from the _attributeMap and that the attribute is
                // no longer reference by the cell that contains it.
                // Don't clear the cell attribute here-- in some cases in the process of adding a
                // row, an attribute that has already been replaced in the table will be deleted
                // and clearing the cell attribute would result in the newly placed attribute being
                // deleted. There is no need to clear the cell attribute on delete since there is
                // not any current situation where a deleted attribute doesn't also result in either
                // the removal of its associated cell or in the replacement of the cell's attribute.
                UnMapAttribute(e.DeletedAttribute, false);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI27847", ex);
            }
        }

        /// <summary>
        /// Handles the <see cref="AttributeStatusInfo.ValidationStateChanged"/> event.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="Extract.DataEntry.ValidationStateChangedEventArgs"/>
        /// instance containing the event data.</param>
        void HandleValidationStateChanged(object sender, ValidationStateChangedEventArgs e)
        {
            try
            {
                // https://extract.atlassian.net/browse/ISSUE-13153
                // There seem to be cases where the cell doesn't get redrawn properly after the
                // validation state changes-- as a way to try to ensure it does get drawn properly
                // in the end, invoke an InvalidateCell call for any attributes whose validation
                // state changed in the table.
                if (_attributeMap.ContainsKey(e.Attribute))
                {
                    this.SafeBeginInvoke("ELI38419", () =>
                    {
                        object tableElement = null;
                        if (_attributeMap.TryGetValue(e.Attribute, out tableElement))
                        {
                            var cell = tableElement as DataGridViewCell;
                            if (cell != null)
                            {
                                InvalidateCell(cell);
                            }
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI38420");
            }
        }

        #endregion Event Handlers

        #region Private Members

        /// <summary>
        /// Compiles a list of <see cref="RasterZone"/>s that describe the specified
        /// <see cref="IAttribute"/>'s location.
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> whose 
        /// <see cref="RasterZone"/>(s) are needed.</param>
        /// <returns>A list of <see cref="RasterZone"/>s that describe the specified
        /// <see cref="IAttribute"/>'s location.</returns>
        static List<RasterZone> GetAttributeRasterZones(IAttribute attribute)
        {
            // Initialize the return value.
            List<RasterZone> zones = new List<RasterZone>();

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
                    ComRasterZone comRasterZone = (ComRasterZone)comRasterZones.At(j);

                    RasterZone rasterZone = new RasterZone(comRasterZone);

                    zones.Add(rasterZone);
                }
            }

            return zones;
        }

        /// <summary>
        /// Retrieves a list of <see cref="RasterZone"/>s that describe the location
        /// of other <see cref="IAttribute"/>s in the same row as the specified cell.
        /// </summary>
        /// <param name="targetCell">The <see cref="DataGridViewCell"/> for which raster zones of
        /// <see cref="IAttribute"/>s sharing the same row are needed.</param>
        /// <param name="columnsToUse">The set of <see cref="DataGridViewColumn"/>s from which
        /// spatial info will be used.</param>
        /// <returns>A list of <see cref="RasterZone"/>s describe the location
        /// of other <see cref="IAttribute"/>s in the same row.</returns>
        List<RasterZone> GetRowRasterZones(DataGridViewCell targetCell,
            IEnumerable columnsToUse)
        {
            try
            {
                List<RasterZone> rowZones = new List<RasterZone>(Columns.Count - 1);

                // Ensure the target cell is a IDataEntryTableCell; otherwise return an empty list
                if (!(targetCell is IDataEntryTableCell))
                {
                    return rowZones;
                }

                // Iterate through every column except the column containing the target
                // attribute.
                foreach (DataGridViewColumn column in columnsToUse)
                {
                    if (column.Index != targetCell.ColumnIndex)
                    {
                        IDataEntryTableCell cell =
                            Rows[targetCell.RowIndex].Cells[column.Index] as IDataEntryTableCell;

                        // If the cell in the current column is a DataEntry cell, compile
                        // the RasterZones that describe it's attribute location.
                        if (cell != null && cell.Attribute != null)
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
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI27603", ex);
            }
        }

        /// <summary>
        /// Retrieves a list of <see cref="RasterZone"/>s that describe the location
        /// of other <see cref="IAttribute"/>s in the same column as the specified cell.
        /// </summary>
        /// <param name="targetCell">The <see cref="DataGridViewCell"/> for which raster zones of
        /// <see cref="IAttribute"/>s sharing the same column are needed.</param>
        /// <param name="rowsToUse">The set of <see cref="DataGridViewRow"/>s from which spatial
        /// info will be used.</param>
        /// <returns>A list of <see cref="RasterZone"/>s describe the location
        /// of other <see cref="IAttribute"/>s in the same column.</returns>
        List<RasterZone> GetColumnRasterZones(DataGridViewCell targetCell,
            IEnumerable rowsToUse)
        {
            try
            {
                List<RasterZone> columnZones =
                    new List<RasterZone>(Rows.Count - 1);

                // Ensure the target cell is a IDataEntryTableCell; otherwise return an empty list
                if (!(targetCell is IDataEntryTableCell))
                {
                    return columnZones;
                }

                // Iterate through every row except the row containing the target attribute
                // and the "new" row (if present).
                foreach (DataGridViewRow row in rowsToUse)
                {
                    if (row.Index != targetCell.RowIndex)
                    {
                        IDataEntryTableCell cell =
                            Rows[row.Index].Cells[targetCell.ColumnIndex] as IDataEntryTableCell;

                        // If the cell in the current row is a DataEntry cell, compile
                        // the RasterZones that describe it's attribute location.
                        if (cell != null && cell.Attribute != null)
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
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI27604", ex);
            }
        }

        /// <summary>
        /// Attempts to provide a hint as to where the data for the specified
        /// <see cref="IAttribute"/> might appear.
        /// </summary>
        /// <param name="attribute">The <see cref="IAttribute"/> for which a spatial hint is wanted.</param>
        /// <param name="targetCell">The <see cref="DataGridViewCell"/> for which a spatial hint
        /// is wanted.</param>
        /// <param name="smartHintRows">The rows for which smart hints are enabled.</param>
        /// <param name="smartHintColumns">The columns for which smart hints are enabled.</param>
        /// <param name="allowAutoPopulation">if set to <see langword="true"/> [allow auto population].</param>
        /// <param name="autoPopulated"><see langword="true"/> if auto-population using OCR text
        /// that exists within a smart-hint region should occur; <see langword="false"/> if
        /// auto-population should not be allowed.</param>
        // Use lists to have access to the count.
        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", MessageId = "5#")]
        protected void CreateSpatialHint(IAttribute attribute, DataGridViewCell targetCell,
            List<DataGridViewRow> smartHintRows, List<DataGridViewColumn> smartHintColumns,
            bool allowAutoPopulation, out bool autoPopulated)
        {
            autoPopulated = false;

            try
            {
                ExtractException.Assert("ELI25232", "Null argument exception!", attribute != null);
                ExtractException.Assert("ELI25233", "Null argument exception!", targetCell != null);
                ExtractException.Assert("ELI27631", "Null argument exception!",
                    smartHintRows != null);
                ExtractException.Assert("ELI27632", "Null argument exception!",
                    smartHintColumns != null);

                // Initialize a list of raster zones for hint locations.
                List<RasterZone> spatialHints = null;

                // A list to keep track of RasterZones for attributes sharing the same column as the
                // target attribute.
                List<RasterZone> columnZones;

                // Attempt to generate a hint based on the intersection of row and column data
                // if both the row and column containing the target cell have smart hints enabled.
                if (smartHintRows.Count > 1 && smartHintColumns.Count > 1 &&
                    smartHintRows.Contains(Rows[targetCell.RowIndex]) &&
                    smartHintColumns.Contains(Columns[targetCell.ColumnIndex]))
                {
                    // Compile a set of raster zones representing the other attributes sharing
                    // the same row and column.
                    List<RasterZone> rowZones = GetRowRasterZones(targetCell, smartHintColumns);
                    columnZones = GetColumnRasterZones(targetCell, smartHintRows);

                    // Attempt to generate a spatial hint using the spatial intersection of the
                    // row and column zones.
                    RasterZone rasterZone =
                        _spatialHintGenerator.GetRowColumnIntersectionSpatialHint(
                            targetCell.RowIndex, rowZones, targetCell.ColumnIndex, columnZones);

                    // If a smart hint was able to be generated, use it.
                    if (rasterZone != null)
                    {
                        if (allowAutoPopulation && attribute.Value.IsEmpty())
                        {
                            // GetTextFromZone will include only words whose center points lie
                            // within the specified rasterZone. In cases where there is some
                            // document skew and the width of the zone is small to begin with (such
                            // as for a LabDE flag value), this can end up missing text. Add a bit
                            // of padding to the zone for which OCR text is to be retrieved.
                            RasterZone paddedZone = new RasterZone(
                                rasterZone.Start, rasterZone.End, rasterZone.Height, rasterZone.PageNumber);
                            paddedZone.ExpandRasterZone(6, 6);

                            var wordsFromZone =
                                DataEntryControlHost.ImageViewer.GetOcrTextFromZone(paddedZone);

                            if (wordsFromZone != null)
                            {
                                AttributeStatusInfo.SetValue(attribute, wordsFromZone, false, false);
                                autoPopulated = true;
                                return;
                            }
                        }

                        spatialHints = new List<RasterZone>();
                        spatialHints.Add(rasterZone);

                        AttributeStatusInfo.SetHintType(attribute, HintType.Direct);
                        AttributeStatusInfo.SetHintRasterZones(attribute, spatialHints);

                        return;
                    }
                }

                // If a "smart" hint was not generated, check to see if row and/or column hints can
                // and should be generated.
                // If row hints are enabled and there are other attributes sharing the row.
                if (_rowHintsEnabled && Columns.Count > 1)
                {
                    // Compile the raster zones of other attributes sharing the row
                    spatialHints = GetRowRasterZones(targetCell, Columns);
                }

                // If column hints are enabled and there are other attributes sharing the column.
                if (_columnHintsEnabled && Rows.Count > 1)
                {
                    // Compile the raster zones of other attributes sharing the column
                    columnZones = GetColumnRasterZones(targetCell, Rows);

                    // Assign or append the column zones to the return value.
                    if (spatialHints == null)
                    {
                        spatialHints = new List<RasterZone>(columnZones);
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

        /// <summary>
        /// Initializes the cell styles used by DataEntry cells using _regularFont and _boldFont.
        /// _regularFont and _boldFont must already be defined prior to calling
        /// InitializeCellStyles.
        /// </summary>
        void InitializeCellStyles()
        {
            // Initialize the various cell styles (modifying existing cell styles on-the-fly
            // causes poor performance. Microsoft recommends sharing DataGridViewCellStyle
            // instances as much as possible.

            _defaultCellStyle = DefaultCellStyle;

            // The style to use for cells currently being edited.
            _editModeCellStyle = new DataGridViewCellStyle(_defaultCellStyle);
            _editModeCellStyle.Font = _regularFont;
            _editModeCellStyle.SelectionForeColor = Color.Black;

            // The style to use for selected cells whose fields have been viewed in the active 
            // table.
            _regularActiveCellStyle = new DataGridViewCellStyle(_defaultCellStyle);
            _regularActiveCellStyle.Font = _regularFont;
            _regularActiveCellStyle.SelectionForeColor = Color.Black;

            // The style to use for selected cells whose fields have been viewed and are not
            // in the active table. A data entry table is going to distinguish between 
            // "selected-and-active" and "selected-but-inactive".  Initialize the selection 
            // color for "inactive" with a more subtle color than the default (blue).
            _regularInactiveCellStyle = new DataGridViewCellStyle(_defaultCellStyle);
            _regularInactiveCellStyle.Font = _regularFont;
            _regularInactiveCellStyle.SelectionForeColor = Color.Black;
            _regularInactiveCellStyle.SelectionBackColor = _INACTIVE_SELECTION_COLOR;

            // The style to use for selected cells whose fields have not been viewed in the
            // active table.
            _boldActiveCellStyle = new DataGridViewCellStyle(_defaultCellStyle);
            _boldActiveCellStyle.Font = _boldFont;
            _boldActiveCellStyle.SelectionForeColor = Color.Black;

            // The style to use for selected cells whose fields have not been viewed and are not
            // in the active table. A data entry table is going to distinguish between 
            // "selected-and-active" and "selected-but-inactive".  Initialize the selection
            // color for "inactive" with a more subtle color than the default (blue).
            _boldInactiveCellStyle = new DataGridViewCellStyle(_defaultCellStyle);
            _boldInactiveCellStyle.Font = _boldFont;
            _boldInactiveCellStyle.SelectionForeColor = Color.Black;
            _boldInactiveCellStyle.SelectionBackColor = _INACTIVE_SELECTION_COLOR;

            // The style to use for selected cells whose contents have been viewed and that are
            // currently being dragged. The inactive selection color is used for the background
            // to delineate the dragged cells.
            _regularDraggedCellStyle = new DataGridViewCellStyle(_defaultCellStyle);
            _regularDraggedCellStyle.Font = _regularFont;
            _regularDraggedCellStyle.SelectionForeColor = Color.Black;
            _regularDraggedCellStyle.BackColor = _INACTIVE_SELECTION_COLOR;
            _regularDraggedCellStyle.SelectionBackColor = _INACTIVE_SELECTION_COLOR;

            // The style to use for selected cells whose contents have not been viewed and that
            // are currently being dragged. The inactive selection color is used for the
            // background to delineate the dragged cells.
            _boldDraggedCellStyle = new DataGridViewCellStyle(_defaultCellStyle);
            _boldDraggedCellStyle.Font = _boldFont;
            _boldDraggedCellStyle.SelectionForeColor = Color.Black;
            _boldDraggedCellStyle.BackColor = _INACTIVE_SELECTION_COLOR;
            _boldDraggedCellStyle.SelectionBackColor = _INACTIVE_SELECTION_COLOR;

            // The style to use for table cells when the table is disabled.
            _disabledCellStyle = new DataGridViewCellStyle(_defaultCellStyle);
            _disabledCellStyle.SelectionForeColor = SystemColors.GrayText;
            _disabledCellStyle.ForeColor = SystemColors.GrayText;
            _disabledCellStyle.SelectionBackColor = SystemColors.Control;
            _disabledCellStyle.BackColor = SystemColors.Control;
        }

        #endregion Private Members
    }
}
