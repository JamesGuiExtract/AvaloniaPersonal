using Extract.Drawing;
using Extract.FileActionManager.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Design;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using UCLID_AFCORELib;
using UCLID_FILEPROCESSINGLib;

namespace Extract.DataEntry.LabDE
{
    /// <summary>
    /// Defines a column that can be added to a <see cref="DataEntryTable"/> in order to allow
    /// selection of the proper record ID from the records stored in the FAMDB. The column will
    /// provide a button that opens a UI to allow the user to view and select from the possible
    /// matching records.
    /// </summary>
    public abstract class RecordPickerTableColumn : DataGridViewButtonColumn
    {
        #region Constants

        /// <summary>
        /// The name of the property grid category for <see cref="RecordPickerTableColumn"/>
        /// specific properties.
        /// </summary>
        protected const string DesignerGridCategory = "Picker Configuration";

        #endregion Constants

        #region Fields

        /// <summary>
        /// The <see cref="DataEntryTableColumn"/> for the record ID field.
        /// </summary>
        DataEntryTableColumn _recordIDColumn;

        /// <summary>
        /// The current <see cref="DataEntryControlHost"/>.
        /// </summary>
        DataEntryControlHost _dataEntryControlHost;

        /// <summary>
        /// Provides access to the outstanding record data in the FAM database.
        /// </summary>
        FAMData _famData;

        /// <summary>
        /// Indicates whether there is a pending request to invalidate the column (to force a
        /// re-paint).
        /// </summary>
        bool _pendingInvalidate;

        /// <summary>
        /// Contains rows for which auto-population of the recordID should not be allowed. Used
        /// to enforce that auto-population of record IDs only happens regarding changes that
        /// directly affect the row rather than as a side-effect of edits to other rows.
        /// </summary>
        HashSet<DocumentDataRecord> _autoPopulationExemptions = new HashSet<DocumentDataRecord>();

        /// <summary>
        /// Specifies whether the current instance is running in design mode
        /// </summary>
        bool _inDesignMode;

        /// <summary>
        /// Specifies whether events have been registered
        /// </summary>
        bool _eventsRegistered;

        /// <summary>
        /// Safety check flag for assigning event handlers
        /// </summary>
        bool _isDisposed;

        #endregion Fields

        #region Properties

        /// <summary>
        /// Gets or sets the name of the <see cref="DataGridViewColumn"/> for the record ID field.
        /// </summary>
        /// <value>
        /// The name of the <see cref="DataGridViewColumn"/> for the record ID field.
        /// </value>
        // The RecordPickerColumnSelector class displays a drop-down list of the names of the columns
        // in the current table.
        [Editor(typeof(RecordPickerColumnSelector), typeof(UITypeEditor))]
        [Category(DesignerGridCategory)]
        public string RecordIdColumn
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the attribute path, relative to the main record attribute, of the attribute that
        /// represents the record ID.
        /// </summary>
        /// <value>The attribute path, relative to the main record attribute, of the attribute that
        /// represents the record ID.he name of the identifier field database.
        /// </value>
        [Category(DesignerGridCategory)]
        public string RecordIdAttribute
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a list of SQL clauses (to be used in a WHERE clause against
        /// <see cref="BaseSelectQuery"/>) that must all be true for a potential record to be matching.
        /// </summary>
        /// <value>
        /// A list of SQL clauses (to be used in a WHERE clause against <see cref="BaseSelectQuery"/>)
        /// that must all be true for a potential record to be matching.
        /// </value>
        [Editor(typeof(MultilineStringEditor), typeof(UITypeEditor))]
        [Category(DesignerGridCategory)]
        public string RecordMatchCriteria
        {
            get
            {
                try
                {
                    return string.Join("\r\n", DataConfiguration.RecordMatchCriteria);
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI38128");
                }
            }

            set
            {
                try
                {
                    DataConfiguration.RecordMatchCriteria = new List<string>(
                        value.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries));
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI38129");
                }
            }
        }

        /// <summary>
        /// Gets or sets the definitions of the columns for the record selection grid. Each column is
        /// specified via a separate line that starts with the column name followed by a colon.
        /// The remainder of the line should be an SQL query part that selects the appropriate data
        /// from the FAM DB table. 
        /// <para><b>Note</b></para>
        /// The RecordId field must be selected as the first column.
        /// </summary>
        /// <value>
        /// The definitions of the columns for the record selection grid.
        /// </value>
        [Editor(typeof(MultilineStringEditor), typeof(UITypeEditor))]
        [Category(DesignerGridCategory)]
        [SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.String.Format(System.String,System.Object)")]
        public string RecordQueryColumns
        {
            get
            {
                try
                {
                    return FromOrderedDictionary(DataConfiguration.RecordQueryColumns);
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI38130");
                }
            }

            set
            {
                try
                {
                    var newDictionary = ToOrderedDictionary(value);

                    ExtractException.Assert("ELI38131",
                        $"The first column must select {DataConfiguration.IdFieldDatabaseName}",
                        newDictionary.Count > 0 &&
                        newDictionary[0].ToString().IndexOf(
                            DataConfiguration.IdFieldDatabaseName, StringComparison.OrdinalIgnoreCase) >= 0);

                    DataConfiguration.RecordQueryColumns = newDictionary;
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI38132");
                }
            }
        }

        /// <summary>
        /// Gets or sets the different possible status colors for the buttons and their SQL query
        /// conditions. Each color is specified via a separate line that starts with the color name
        /// (from <see cref="System.Drawing.Color"/>) followed by a colon. The remainder of the line
        /// should be an SQL query expression that evaluates to true if the color should be used
        /// based to indicate available record status. If multiple expressions evaluate to true, the
        /// first (top-most) of the matching rows will be used.
        /// <value>
        /// The different possible status colors for the buttons and their SQL query conditions.
        /// </value>
        [Editor(typeof(MultilineStringEditor), typeof(UITypeEditor))]
        [Category(DesignerGridCategory)]
        public string ColorQueryConditions
        {
            get
            {
                try
                {
                    return FromOrderedDictionary(DataConfiguration.ColorQueryConditions);
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI38133");
                }
            }

            set
            {
                try
                {
                    DataConfiguration.ColorQueryConditions = ToOrderedDictionary(value);
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI38134");
                }
            }
        }

        /// <summary>
        /// Gets or sets the filter on the displayed matching rows that determine which rows are
        /// candidates for auto-selection when the picker UI is displayed. If <see langword="null"/>
        /// records will not be automatically selected (unless a record ID had already been
        /// assigned).
        /// </summary>
        /// <value>
        /// The filter on the displayed matching rows that determine which rows are candidates for
        /// auto-selection. The syntax is as specified for the DataColumn.Expression property.
        /// </value>
        [Category(DesignerGridCategory)]
        public string AutoSelectionFilter
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the record in which rows matching <see cref="AutoSelectionRecord"/> are to be
        /// considered for auto-selection where the first matching row is the row that is selected.
        /// <see langword="null"/> if a row should be auto-selected only if it is the only row
        /// matching <see cref="AutoSelectionFilter"/> (or only row period if
        /// <see cref="AutoSelectionFilter"/> is not specified).
        /// </summary>
        /// <value>
        /// The record in which rows matching <see cref="AutoSelectionRecord"/> are to be considered
        /// for auto-selection. The syntax is as described for the DataView.Sort property.
        /// </value>
        [Category(DesignerGridCategory)]
        public string AutoSelectionRecord
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the record ID should be auto-populated for a
        /// row when there is only a single candidate row that would appear in the record picker grid
        /// that also matches <see cref="AutoSelectionFilter"/> (and there are not other rows in the
        /// table for which this is also true).
        /// </summary>
        /// <value><see langword="true"/> to allow auto-population of the record ID; otherwise, 
        /// <see langword="false"/>.
        /// </value>
        [DefaultValue(false)]
        [Category(DesignerGridCategory)]
        public bool AutoPopulate
        {
            get;
            set;
        }

        #endregion Properties  

        #region Methods

        /// <summary>
        /// Gets the descriptions of all records on the active document that have previously been
        /// submitted via LabDE.
        /// </summary>
        /// <returns>The descriptions of all records on the active document that have previously been
        /// submitted.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public IEnumerable<string> GetPreviouslySubmittedRecords()
        {
            try
            {
                LoadDataForAllRows();

                return FAMData.GetPreviouslySubmittedRecords();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI38188");
            }
        }

        /// <summary>
        /// Links the record IDs currently in the database table with the active document in the
        /// FAM DB. If the link already exists, the existing row will be updated as necessary.
        /// </summary>
        public void LinkFileWithRecords()
        {
            try
            {
                LoadDataForAllRows();

                int fileId = FileProcessingDB.GetFileID(AttributeStatusInfo.SourceDocName);

                FAMData.LinkFileWithCurrentRecords(fileId);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI38182");
            }
        }

        /// <summary>
        /// Clears all data currently cached to force it to be re-retrieved next time it is needed.
        /// </summary>
        /// <param name="clearDatabaseData"><see langword="true"/> to clear data cached from the
        /// database; <see langword="false"/> to clear only data obtained from the UI.</param>
        public void ClearCachedData(bool clearDatabaseData)
        {
            try
            {
                FAMData.ClearCachedData(clearDatabaseData);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI38194");
            }
        }

        #endregion Methods

        #region Internal Members

        /// <summary>
        /// Gets the <see cref="FAMData"/> that allows access to the record data stored in the FAM
        /// database.
        /// </summary>
        internal FAMData FAMData
        {
            get
            {
                try
                {
                    if (_famData == null)
                    {
                        _famData = new FAMData(FileProcessingDB);
                        _famData.DataConfiguration = DataConfiguration;
                        _famData.RowDataUpdated += HandleFamData_RowDataUpdated;
                    }

                    _famData.FileProcessingDB = FileProcessingDB;

                    return _famData;
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI41554");
                }
            }
        }

        internal abstract IFAMDataConfiguration DataConfiguration
        {
            get;
        }

        #endregion Internal Members
        
        #region Protected Members

        /// <summary>
        /// Gets the <see cref="DataEntryControlHost"/>.
        /// </summary>
        protected DataEntryControlHost DataEntryControlHost
        {
            get
            {
                try
                {
                    if (_dataEntryControlHost == null)
                    {
                        var dataEntryTable = DataGridView as DataEntryTableBase;
                        if (dataEntryTable != null && dataEntryTable.DataEntryControlHost != null)
                        {
                            _dataEntryControlHost = dataEntryTable.DataEntryControlHost;
                            _dataEntryControlHost.UpdateEnded += HandleDataEntryControlHost_UpdateEnded;
                        }
                    }
                    return _dataEntryControlHost;
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI41552");
                }
            }
        }

        /// <summary>
        /// Gets the <see cref="FileProcessingDB"/>
        /// </summary>
        protected FileProcessingDB FileProcessingDB
        {
            get
            {
                try
                {
                    return _inDesignMode || (DataEntryControlHost == null)
                        ? null
                        : DataEntryControlHost.DataEntryApplication.FileProcessingDB;
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI41553");
                }
            }
        }

        #endregion Protected Members

        #region Overrides

        /// <summary>
        /// Called when the band is associated with a different 
        /// <see cref="T:System.Windows.Forms.DataGridView"/>.
        /// </summary>
        protected override void OnDataGridViewChanged()
        {
            try
            {
                base.OnDataGridViewChanged();

                // _inDesignMode does not seem to get set correctly at least some of the time for
                // DataEntryTableRow and DataEntryTableColumn. When assigned to a table, set 
                // _inDesignMode if the table's InDesignMode property is true (which does seem to be
                // reliable.
                if (!_inDesignMode && DataGridView != null)
                {
                    var parentDataEntryTable = DataGridView as DataEntryTableBase;

                    if (parentDataEntryTable != null)
                    {
                        _inDesignMode = parentDataEntryTable.InDesignMode;
                    }
                }

                // Register to be notified of relevant events in the DataGridView.
                // (The extra test for DataEntryTableBase is because of unreliability of the
                // _inDesignMode checks. DataGridView will not be a DataEntryTableBase when editing
                // the column in the designer.
                if (!_eventsRegistered && !_isDisposed && !_inDesignMode && Visible && DataGridView is DataEntryTableBase)
                {
                    DataGridView.HandleCreated += HandleDataGridView_HandleCreated;
                    DataGridView.HandleDestroyed += HandleDataGridView_HandleDestroyed;
                    DataGridView.CellContentClick += HandleDataGridView_CellContentClick;
                    DataGridView.CellPainting += HandleDataGridView_CellPainting;
                    DataGridView.Rows.CollectionChanged += HandleDataGridViewRows_CollectionChanged;

                    _eventsRegistered = true;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI38136");
            }
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged
        /// resources; <see langword="false"/> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                try
                {
                    if (_famData != null)
                    {
                        _famData.RowDataUpdated -= HandleFamData_RowDataUpdated;
                        _famData.Dispose();
                        _famData = null;
                    }

                    _recordIDColumn = null;
                    _dataEntryControlHost = null;

                    // This fixes a multiple registration problem, where event handlers were getting assigned
                    // many times (3 that I saw) - and fixes a memory leak as well.
                    // Done for Issue-14322, didn't fix the issue but is an improvement.
                    if (_eventsRegistered)
                    {
                        DataGridView.HandleCreated -= HandleDataGridView_HandleCreated;
                        DataGridView.HandleDestroyed -= HandleDataGridView_HandleDestroyed;
                        DataGridView.CellContentClick -= HandleDataGridView_CellContentClick;
                        DataGridView.CellPainting -= HandleDataGridView_CellPainting;
                        DataGridView.Rows.CollectionChanged -= HandleDataGridViewRows_CollectionChanged;

                        _eventsRegistered = false;
                        _isDisposed = true;
                    }
                } catch { }
            }

            base.Dispose(disposing);
        }

        #endregion Overrides

        #region Event Handlers

        /// <summary>
        /// Handles the <see cref="Control.HandleCreated"/> event of the <see cref="DataGridView"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleDataGridView_HandleCreated(object sender, EventArgs e)
        {
            try
            {
                // Once the grid is ready for display, resolve _recordIdColumn.
                _recordIDColumn = DataGridView.Columns
                    .OfType<DataEntryTableColumn>()
                    .SingleOrDefault(column => column.Name == RecordIdColumn);

                // This column may exist in thread where the UI has not been created (such as data
                // entry pre-loader OH or DataEntryDocumentPanel.UpdateDocumentStatus. UI updates
                // based on cleared cache isn't relevant (and will fail) if the UI is not created.
                AttributeStatusInfo.QueryCacheCleared += HandleAttributeStatusInfo_QueryCacheCleared;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI38137");
            }
        }

        /// <summary>
        /// Handles the <see cref="Control.HandleDestroyed"/> event of the <see cref="DataGridView"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void HandleDataGridView_HandleDestroyed(object sender, EventArgs e)
        {
            try
            {
                AttributeStatusInfo.QueryCacheCleared -= HandleAttributeStatusInfo_QueryCacheCleared;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI41581");
            }
        }

        /// <summary>
        /// Handles the <see cref="DataGridView.CellPainting"/> event of the
        /// <see cref="DataGridView"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.DataGridViewCellPaintingEventArgs"/>
        /// instance containing the event data.</param>
        void HandleDataGridView_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            try
            {
                // Make sure to respect Handled flag, if already handled there is nothing to do, get out
                if (e.Handled)
                {
                    return;
                }

                if (DataEntryControlHost != null && DataEntryControlHost.UpdateInProgress)
                {
                    // If the UI data is currently being updated, postpone the paint until the
                    // update is complete to prevent unnecessary cache thrashing and DB hits.
                    _pendingInvalidate = true;
                    e.Handled = true;
                    return;
                }

                // Paint the with the appropriate status color.
                if (e.RowIndex >= 0 && e.ColumnIndex == Index)
                {
                    var dataEntryRow = DataGridView.Rows[e.RowIndex] as DataEntryTableRow;
                    if (dataEntryRow != null)
                    {
                        // If this appears to be first row data is being loaded for, load the data
                        // for all rows right now. This is needed for
                        // FAMData.AlreadyMappedRecordIDs to be able to correctly reflect all
                        // rows whether or not they are currently visible.
                        if (!FAMData.HasRowData)
                        {
                            _autoPopulationExemptions.Clear();
                            LoadDataForAllRows();
                        }

                        var recordIdCell = dataEntryRow.Cells[_recordIDColumn.Index];
                        var recordId = recordIdCell.Value.ToString();
                        DocumentDataRecord rowData = FAMData.GetRowData(dataEntryRow);
                        if (string.IsNullOrWhiteSpace(recordId) &&
                            rowData != null && rowData.StatusColor.HasValue)
                        {
                            // This prevents a still mysterious "Parameter is not valid" exception. Apparently
                            // the Handled flag is set true somewhere inside LoadDataForAllRows, but even putting a 
                            // break into e.Handled.set() hasn't clarified this... e.Graphics has been disposed.
                            // https://extract.atlassian.net/browse/ISSUE-14322
                            if (e.Handled)
                            {
                                return;
                            }

                            try
                            {
                                e.PaintBackground(e.CellBounds, false);
                            }
                            catch (Exception ex)
                            {
                                // Just in case the above test doesn't prevent the exception - log it.
                                // https://extract.atlassian.net/browse/ISSUE-14322
                                ex.ExtractLog("ELI41678");

                                return;
                            }

                            // By making the brush color be translucent, some of the original button
                            // shading shows through giving a more polished appearance than a flat
                            // color.
                            Color color = Color.FromArgb(80, rowData.StatusColor.Value);
                            Brush brush = ExtractBrushes.GetSolidBrush(color);

                            // The button doesn't extend all the way to the edges of the cell...
                            // shrink the paint area vs the overall bounds.
                            e.Paint(e.CellBounds, DataGridViewPaintParts.All);
                            Rectangle fillRect = e.CellBounds;
                            fillRect.Inflate(-3, -3);
                            e.Graphics.FillRectangle(brush, fillRect);

                            // Now paint "..." to indicate this will open a form.
                            brush = ExtractBrushes.GetSolidBrush(Color.Black);
                            StringFormat stringFormat = new StringFormat();
                            stringFormat.Alignment = StringAlignment.Center;
                            stringFormat.LineAlignment = StringAlignment.Center;
                            e.Graphics.DrawString(
                                "...", DataGridView.Font, brush, e.CellBounds, stringFormat);
                            e.Handled = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI38138");
            }
        }

        /// <summary>
        /// Handles the <see cref="DataGridView.CellContentClick"/> event of the
        /// <see cref="DataGridView"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.DataGridViewCellEventArgs"/>
        /// instance containing the event data.</param>
        void HandleDataGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                // Activate the record-picker UI (FFI).
                if (e.RowIndex >= 0 && this == DataGridView.Columns[e.ColumnIndex])
                {
                    var tableRow = DataGridView.Rows[e.RowIndex];
                    var dataEntryRow = tableRow as DataEntryTableRow;
                    if (dataEntryRow != null)
                    {
                        // https://extract.atlassian.net/browse/ISSUE-13063
                        // Select the row associated with the button clicked so that selection is
                        // not lost (clearing the record details section).
                        tableRow.Selected = true;

                        var recordIdCell = tableRow.Cells[_recordIDColumn.Index];
                        var initialValue = recordIdCell.Value.ToString();

                        // If a selection was made, apply the new record ID.
                        string recordId = ShowPickerFFI(dataEntryRow, initialValue);
                        if (!string.IsNullOrWhiteSpace(recordId))
                        {
                            recordIdCell.Value = recordId;

                            // Setting the record ID cell as active helps indicate that a value
                            // was applied.
                            DataGridView.ClearSelection();
                            
                            // https://extract.atlassian.net/browse/ISSUE-15137
                            if (recordIdCell.Visible)
                            {
                                DataGridView.CurrentCell = recordIdCell;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI38139");
            }
        }

        /// <summary>
        /// Handles the <see cref="DataGridViewRowCollection.CollectionChanged"/> event of the
        /// <see cref="DataGridView"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.ComponentModel.CollectionChangeEventArgs"/>
        /// instance containing the event data.</param>
        void HandleDataGridViewRows_CollectionChanged(object sender, CollectionChangeEventArgs e)
        {
            try
            {
                // Notify FAMData not to track the deleted row anymore.
                if (e.Action == CollectionChangeAction.Remove)
                {
                    var dataEntryRow = e.Element as DataEntryTableRow;
                    if (dataEntryRow != null)
                    {
                        FAMData.DeleteRow(dataEntryRow);
                    }
                }
                else if (e.Action == CollectionChangeAction.Refresh)
                {
                    FAMData.ResetRowData();
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI38140");
            }
        }

        /// <summary>
        /// Handles the <see cref="E:FAMData.RowDataUpdated"/> event.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowDataUpdatedEventArgs"/> instance containing the
        /// event data.</param>
        void HandleFamData_RowDataUpdated(object sender, RowDataUpdatedEventArgs e)
        {
            try
            {
                var row = e.DocumentDataRecord.DataEntryTableRow;
                if (row != null)
                {
                    var cell = row.Cells[Index];
                    if (cell != null && cell.DataGridView == DataGridView)
                    {
                        ClearCachedData(false);

                        // If the record number has changed, update the tooltip text for the record
                        // ID and picker cells to be a summary of the currently selected record.
                        if (e.RecordIdUpdated)
                        {
                            // If the row number specifically is being changed, block
                            // auto-population to prevent a deleted record ID from being
                            // automatically replaced.
                            _autoPopulationExemptions.Add(e.DocumentDataRecord);

                            var dataEntryTable = DataGridView as DataEntryTableBase;
                            if (dataEntryTable != null)
                            {
                                IAttribute recordIdAttribute = e.DocumentDataRecord.IdField.Attribute;
                                IDataEntryTableCell recordIdCell =
                                    dataEntryTable.GetAttributeUIElement(recordIdAttribute, "")
                                        as IDataEntryTableCell;
                                if (recordIdCell != null)
                                {
                                    DataGridViewCell pickerButtonCell =
                                        recordIdCell.AsDataGridViewCell.OwningRow.Cells[Index];
                                    string recordDescription =
                                        FAMData.GetRecordDescription(e.DocumentDataRecord.IdField.Value);
                                    recordIdCell.AsDataGridViewCell.ToolTipText = recordDescription;
                                    pickerButtonCell.ToolTipText = recordDescription;
                                }
                            }
                        }
                        // If an edit is being made that directly affects this row and the record
                        // ID has not yet been filled in, allow the edit to auto-populate an
                        // record ID (if applicable).
                        else if (string.IsNullOrWhiteSpace(e.DocumentDataRecord.IdField.Value))
                        {
                            _autoPopulationExemptions.Remove(e.DocumentDataRecord);
                        }

                        if (!DataEntryControlHost.IsDocumentLoaded)
                        {
                            // https://extract.atlassian.net/browse/ISSUE-14303
                            // If the document is not yet loaded we don't need to invoke any invalidate.
                            // However, if auto-populating, go ahead and do that immediately so that the DEP
                            // does not depend on being invalidated in order to have a correctly populated
                            // record ID.
                            if (AutoPopulate)
                            {
                                DataEntryControlHost.Invoke((MethodInvoker)(() => AutoPopulateRecordNumbers()));
                            }
                        }

                        InvokeInvalidate();
                    }
                }
            }
            catch (Exception ex)
            {
                // Throw here since we know this event is triggered by our own UI event handler.
                throw ex.AsExtract("ELI38141");
            }
        }

        /// <summary>
        /// Handles the <see cref="E:DataEntryControlHost.UpdateEnded"/> event of
        /// <see cref="DataEntryControlHost"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleDataEntryControlHost_UpdateEnded(object sender, EventArgs e)
        {
            try
            {
                if (!DataEntryControlHost.IsDocumentLoaded)
                {
                    // https://extract.atlassian.net/browse/ISSUE-14303
                    // If the document is not yet loaded we don't need to invoke any invalidate.
                    // However, if auto-populating, go ahead and do that immediately so that the DEP
                    // does not depend on being invalidated in order to have a correctly populated
                    // record ID.
                    if (AutoPopulate)
                    {
                        DataEntryControlHost.Invoke((MethodInvoker)(() => AutoPopulateRecordNumbers()));
                    }
                }

                // At the end of an update, if a paint was postponed, invoke an invalidate now.
                if (_pendingInvalidate)
                {
                    // Set _pendingInvalidate first since it will be used as a flag in
                    // InvokeInvalidate to ensure multiple invalidates are not invoked at the same
                    // time.
                    _pendingInvalidate = false;
                    InvokeInvalidate();
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI38230");
            }
        }

        /// <summary>
        /// Handles the <see cref="AttributeStatusInfo.QueryCacheCleared" /> event.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        void HandleAttributeStatusInfo_QueryCacheCleared(object sender, EventArgs e)
        {
            try
            {
                ClearCachedData(true);
                _autoPopulationExemptions.Clear();
                
                InvokeInvalidate();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI41575");
            }
        }

        #endregion Event Handlers

        #region Private Members

        /// <summary>
        /// Ensures <see cref="FAMData"/> has loaded data from the DB for all rows in the
        /// <see cref="DataGridView"/>.
        /// </summary>
        void LoadDataForAllRows()
        {
            foreach (DataGridViewRow row in DataGridView.Rows)
            {
                FAMData.GetRowData(row as DataEntryTableRow);
            }
        }

        /// <summary>
        /// Displays the record selection UI FFI for the specified <see paramref="dataEntryRow"/>.
        /// </summary>
        /// <param name="dataEntryRow">The <see cref="DataEntryTableRow"/> to show the UI for.</param>
        /// <param name="defaultRecordId">The record ID that should be selected by default in
        /// the picker UI.</param>
        /// <returns>The record ID that was selected from the picker UI; <see langword="null"/> if
        /// the UI was cancelled without having chosen an record ID.
        /// </returns>
        string ShowPickerFFI(DataEntryTableRow dataEntryRow, string defaultRecordId)
        {
            DocumentDataRecord rowData = FAMData.GetRowData(dataEntryRow);
            if (rowData != null)
            {
                using (var fileInspectorForm = new FAMFileInspectorForm())
                using (var selectionPane = new RecordPickerSelectionPane())
                {
                    fileInspectorForm.UseDatabaseMode = true;
                    fileInspectorForm.FileProcessingDB.DuplicateConnection(FileProcessingDB);

                    fileInspectorForm.FileSelectorPane = selectionPane;
                    selectionPane.AutoSelectionFilter = AutoSelectionFilter;
                    selectionPane.AutoSelectionRecord = AutoSelectionRecord;
                    selectionPane.SelectedRecordId = defaultRecordId;
                    selectionPane.RowData = rowData;
                    selectionPane.UpdateRecordSelectionGrid();

                    if (fileInspectorForm.ShowDialog(DataEntryControlHost) == DialogResult.OK)
                    {
                        return selectionPane.SelectedRecordId;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Invoke an invalidate call to occur after any current UI events are complete.
        /// </summary>
        void InvokeInvalidate()
        {
            // Don't invoke the invalidate multiple times (multiple rows may all trigger this call
            // in response to the same UI event).
            if (!_pendingInvalidate && DataEntryControlHost != null && Visible)
            {
                if (DataEntryControlHost.UpdateInProgress)
                {
                    // Postpone until the current update is complete.
                    _pendingInvalidate = true;
                }
                else
                {
                    // Prevent simultaneous invokes of invalidate.
                    _pendingInvalidate = true;

                    DataEntryControlHost.ExecuteOnIdle("ELI38231", () => 
                    {
                        _pendingInvalidate = false;

                        if (AutoPopulate)
                        {
                            AutoPopulateRecordNumbers();
                        }
                        DataGridView.InvalidateColumn(Index);
                    });
                }
            }
        }

        /// <summary>
        /// Auto-populates all record IDs that qualify for auto population.
        /// </summary>
        void AutoPopulateRecordNumbers()
        {
            var pendingAutoPopulation = new Dictionary<string, DataGridViewCell>();

            foreach (DataGridViewRow tableRow in DataGridView.Rows)
            {
                DocumentDataRecord rowData = FAMData.GetRowData(tableRow as DataEntryTableRow);
                var recordIdCell = tableRow.Cells[_recordIDColumn.Index];

                // Only consider rows for which we have data, currently do not have an record ID
                // and haven't been exempted from auto-population.
                if (rowData != null && !_autoPopulationExemptions.Contains(rowData) &&
                    string.IsNullOrWhiteSpace(recordIdCell.Value as string))
                {
                    // Get a view for all un-mapped matching records that qualify under
                    // AutoSelectionFilter.
                    using (DataTable table = rowData.UnmappedMatchingRecords.ToTable())
                    using (DataView view = new DataView(table))
                    {
                        if (!string.IsNullOrWhiteSpace(AutoSelectionFilter))
                        {
                            view.RowFilter = AutoSelectionFilter;
                        }

                        // If there is only one qualifying row, add it to the list of rows to
                        // auto-populate as long as there are no other rows that qualify for the
                        // same record number.
                        if (view.Count == 1)
                        {
                            string recordId = (string)(view[0].Row.ItemArray[0]);
                            if (pendingAutoPopulation.ContainsKey(recordId))
                            {
                                pendingAutoPopulation[recordId] = null;
                            }
                            else
                            {
                                pendingAutoPopulation[recordId] = recordIdCell;

                                if (!DataEntryControlHost.IsDocumentLoaded)
                                {
                                    // As a work-around to issue of values applied during load of
                                    // being reverted, set LastAppliedStringValue.
                                    IAttribute recordIdAttribute = rowData.IdField.Attribute;
                                    AttributeStatusInfo.GetStatusInfo(recordIdAttribute).LastAppliedStringValue = recordId;
                                }
                            }
                        }
                    }

                    // Once a row has been tested for auto-population qualification, do not consider
                    // it again until explicitly allowed to do so.
                    _autoPopulationExemptions.Add(rowData);
                }
            }

            // Apply the qualifying auto-populated record IDs.
            foreach (KeyValuePair<string, DataGridViewCell> autoPopulation in pendingAutoPopulation)
            {
                bool trackingOperations = AttributeStatusInfo.UndoManager.TrackOperations;

                try
                {
                    // This isn't a user edit which should be tracked by the undo system.
                    AttributeStatusInfo.UndoManager.TrackOperations = false;

                    if (autoPopulation.Value != null)
                    {
                        autoPopulation.Value.Value = autoPopulation.Key;
                    }
                }
                finally
                {
                    if (trackingOperations)
                    {
                        AttributeStatusInfo.UndoManager.TrackOperations = true;
                    }
                }
            }

            // https://extract.atlassian.net/browse/ISSUE-15162
            // Ensure queries fire whenever auto-population occurs.
            if (pendingAutoPopulation.Any())
            {
                AttributeStatusInfo.EndEdit();
            }
        }

        /// <summary>
        /// Converts the specified <see paramref="dictionary"/> to a string where each line is an
        /// entry. The key is the text up to the first colon on the line and the value is everything
        /// after the colon.
        /// </summary>
        /// <returns>A string representation of <see paramref="dictionary"/></returns>
        static string FromOrderedDictionary(OrderedDictionary dictionary)
        {
            return string.Join("\r\n", dictionary
                .OfType<DictionaryEntry>()
                .Select(entry => entry.Key + ": " + entry.Value));
        }

        /// <summary>
        /// Converts the specified <see paramref="description"/> to an
        /// <see cref="OrderedDictionary"/>. The description is expected to have one line for each
        /// entry. The key should be the text up to the first colon on the line and the value should
        /// be everything after the colon.
        /// </summary>
        /// <returns>The <see cref="OrderedDictionary"/> represented by <see paramref="description"/>.
        /// </returns>
        static OrderedDictionary ToOrderedDictionary(string description)
        {
            string[] rows =
                description.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            var newDictionary = new OrderedDictionary();
            foreach (string columnDefintion in rows)
            {
                int colonIndex = columnDefintion.IndexOf(':');
                string key = (colonIndex >= 0)
                    ? columnDefintion.Substring(0, colonIndex).Trim()
                    : columnDefintion.Trim();
                string value = (colonIndex >= 0)
                    ? columnDefintion.Substring(colonIndex + 1).Trim()
                    : "";

                newDictionary.Add(key, value);
            }

            return newDictionary;
        }

        #endregion Private Members
    }

    /// <summary>
    /// A <see cref="UITypeEditor"/> intended for the
    /// <see cref="RecordPickerTableColumn.RecordIdColumn"/> field in a property grid. This
    /// control causes a drop-down list of the names of the columns in the current table.
    /// </summary>
    internal class RecordPickerColumnSelector : ObjectSelectorEditor
    {
        /// <summary>
        /// Fills a hierarchical collection of labeled items, with each item represented by a
        /// <see cref="T:System.Windows.Forms.TreeNode"/>.
        /// </summary>
        /// <param name="selector">A hierarchical collection of labeled items.</param>
        /// <param name="context">The context information for a component.</param>
        /// <param name="provider">The <see cref="M:System.IServiceProvider.GetService(System.Type)"/>
        /// method of this interface that obtains the object that provides the service.</param>
        protected override void FillTreeWithData(Selector selector, ITypeDescriptorContext context,
            IServiceProvider provider)
        {
            try
            {
                base.FillTreeWithData(selector, context, provider);

                // Need to use reflection to get at the members of context.Instance which will be
                // a non-public class that holds information about the column being edited here.
                RecordPickerTableColumn RecordPickerColumn =
                    context.Instance.GetType()
                    .GetField("column", BindingFlags.Instance | BindingFlags.NonPublic)
                    .GetValue(context.Instance)
                    as RecordPickerTableColumn;
                DataGridView dataGridView = RecordPickerColumn.DataGridView;

                // All column names except for the record picker column should be available for
                // selection.
                selector.Nodes.AddRange(
                    dataGridView.Columns
                        .OfType<DataGridViewColumn>()
                        .Where(column => column != RecordPickerColumn)
                        .Select(column => new SelectorNode(column.Name, column.Name))
                        .ToArray());

                // Select by default the column the is currently configured as the
                // RecordIdColumn.
                selector.SelectedNode =
                    selector.Nodes.OfType<SelectorNode>()
                    .Where(node => RecordPickerColumn.RecordIdColumn != null &&
                        RecordPickerColumn.RecordIdColumn.Equals(
                            (string)node.value, StringComparison.Ordinal))
                    .OfType<TreeNode>()
                    .SingleOrDefault();
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI38142");
            }
        }
    }
}
