using Extract.Drawing;
using Extract.FileActionManager.Utilities;
using Extract.Licensing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Drawing.Design;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using UCLID_FILEPROCESSINGLib;

namespace Extract.DataEntry.LabDE
{
    /// <summary>
    /// Defines a column that can be added to a <see cref="DataEntryTable"/> in order to allow
    /// selection of the proper order number from the orders stored in the FAMDB. The column will
    /// provide a button that opens a UI to allow the user to view and select from the possible
    /// matching orders.
    /// </summary>
    public class OrderPickerTableColumn : DataGridViewButtonColumn
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(OrderPickerTableColumn).ToString();

        /// <summary>
        /// The name of the property grid category for <see cref="OrderPickerTableColumn"/>
        /// specific properties.
        /// </summary>
        const string _PROPERTY_GRID_CATEGORY = "Order Picker";

        #endregion Constants

        #region Fields

        /// <summary>
        /// The <see cref="DataEntryTableColumn"/> for the order number field.
        /// </summary>
        DataEntryTableColumn _orderNumberColumn;

        /// <summary>
        /// The current <see cref="DataEntryControlHost"/>.
        /// </summary>
        DataEntryControlHost _dataEntryControlHost;

        /// <summary>
        /// Provides access to the outstanding order data in the FAM database.
        /// </summary>
        FAMData _famData;

        /// <summary>
        /// Specifies whether the current instance is running in design mode
        /// </summary>
        bool _inDesignMode;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderPickerTableColumn"/> class.
        /// </summary>
        public OrderPickerTableColumn()
            : base()
        {
            try
            {
                // Because LicenseUsageMode.UsageMode isn't always accurate, this will be re-checked
                // in OnDataGridViewChanged.
                _inDesignMode = (LicenseManager.UsageMode == LicenseUsageMode.Designtime);

                // Load licenses in design mode
                if (_inDesignMode)
                {
                    // Load the license files from folder
                    LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());
                }

                // Validate the license
                LicenseUtilities.ValidateLicense(
                    LicenseIdName.LabDECoreObjects, "ELI38122", _OBJECT_NAME);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI38123");
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the name of the <see cref="DataGridViewColumn"/> for the order number field.
        /// </summary>
        /// <value>
        /// The name of the <see cref="DataGridViewColumn"/> for the order number field.
        /// </value>
        // The OrderNumberColumnEditor class displays a drop-down list of the names of the columns
        // in the current table.
        [Editor(typeof(OrderNumberColumnSelector), typeof(UITypeEditor))]
        [Category(_PROPERTY_GRID_CATEGORY)]
        public string OrderNumberColumn
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the attribute path for the attribute containing the patient MRN. The path
        /// should either be rooted or be relative to the LabDE Order attribute.
        /// </summary>
        /// <value>
        /// The attribute path for the attribute containing the patient MRN.
        /// </value>
        [DefaultValue(FAMData._DEFAULT_PATIENT_MRN_ATTRIBUTE)]
        [Category(_PROPERTY_GRID_CATEGORY)]
        public string PatientMRNAttribute
        {
            get
            {
                try
                {
                    return FAMData.PatientMRNAttribute;
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI38124");
                }
            }

            set
            {
                try
                {
                    FAMData.PatientMRNAttribute = value;
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI38125");
                }
            }
        }

        /// <summary>
        /// Gets or sets the attribute path for the attribute containing the order code. The path
        /// should either be rooted or be relative to the LabDE Order attribute.
        /// </summary>
        /// <value>
        /// The attribute path for the attribute containing the order code.
        /// </value>
        [DefaultValue(FAMData._DEFAULT_ORDER_CODE_ATTRIBUTE)]
        [Category(_PROPERTY_GRID_CATEGORY)]
        public string OrderCodeAttribute
        {
            get
            {
                try
                {
                    return FAMData.OrderCodeAttribute;
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI38126");
                }
            }

            set
            {
                try
                {
                    FAMData.OrderCodeAttribute = value;
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI38127");
                }
            }
        }

        /// <summary>
        /// Gets or sets the attribute path for the attribute containing the collection date. The
        /// path should either be rooted or be relative to the LabDE Order attribute.
        /// </summary>
        /// <value>
        /// The attribute path for the attribute containing the collection date.
        /// </value>
        [DefaultValue(FAMData._DEFAULT_COLLECTION_DATE_ATTRIBUTE)]
        [Category(_PROPERTY_GRID_CATEGORY)]
        public string CollectionDateAttribute
        {
            get
            {
                try
                {
                    return FAMData.CollectionDateAttribute;
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI38178");
                }
            }

            set
            {
                try
                {
                    FAMData.CollectionDateAttribute = value;
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI38179");
                }
            }
        }

        /// <summary>
        /// Gets or sets the attribute path for the attribute containing the collection time. The
        /// path should either be rooted or be relative to the LabDE Order attribute.
        /// </summary>
        /// <value>
        /// The attribute path for the attribute containing the collection time.
        /// </value>
        [DefaultValue(FAMData._DEFAULT_COLLECTION_TIME_ATTRIBUTE)]
        [Category(_PROPERTY_GRID_CATEGORY)]
        public string CollectionTimeAttribute
        {
            get
            {
                try
                {
                    return FAMData.CollectionTimeAttribute;
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI38180");
                }
            }

            set
            {
                try
                {
                    FAMData.CollectionTimeAttribute = value;
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI38181");
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether unavailable orders should be displayed in the
        /// picker UI.
        /// </summary>
        /// <value><see langword="true"/> if unavailable orders should be displayed; otherwise,
        /// <see langword="false"/>.
        /// </value>
        [DefaultValue(false)]
        [Category(_PROPERTY_GRID_CATEGORY)]
        public bool ShowUnavailableOrders
        {
            get
            {
                try
                {
                    return FAMData.ShowUnavailableOrders;
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI38171");
                }
            }

            set
            {
                try
                {
                    FAMData.ShowUnavailableOrders = value;
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI38172");
                }
            }
        }

        /// <summary>
        /// Gets or sets an SQL query that selects order numbers based on additional custom criteria
        /// above having a matching MRN and order code.
        /// </summary>
        /// <value>
        /// An SQL query that selects order numbers based on additional custom criteria
        /// </value>
        [Category(_PROPERTY_GRID_CATEGORY)]
        public string CustomOrderMatchCriteriaQuery
        {
            get
            {
                try
                {
                    return FAMData.CustomOrderMatchCriteriaQuery;
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
                    FAMData.CustomOrderMatchCriteriaQuery = value;
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI38129");
                }
            }
        }

        /// <summary>
        /// Gets or sets the definitions of the columns for the order selection grid. Each column is
        /// specified via a separate line that starts with the column name followed by a colon.
        /// The remainder of the line should be an SQL query part that selects the appropriate data
        /// from the [LabDEOrder] table. 
        /// <para><b>Note</b></para>
        /// The OrderNumber field must be selected as the first column.
        /// </summary>
        /// <value>
        /// The definitions of the columns for the order selection grid.
        /// </value>
        [Editor(typeof(MultilineStringEditor), typeof(UITypeEditor))]
        [Category(_PROPERTY_GRID_CATEGORY)]
        public string OrderQueryColumns
        {
            get
            {
                try
                {
                    return FromOrderedDictionary(FAMData.OrderQueryColumns);
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
                        "The first column must select [OrderNumber]",
                        newDictionary.Count > 0 &&
                        newDictionary[0].ToString().ToUpperInvariant().Contains("ORDERNUMBER"));

                    FAMData.OrderQueryColumns = newDictionary;
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
        /// based to indicate available order status. If multiple expressions evaluate to true, the
        /// first (top-most) of the matching rows will be used.
        /// <para><b>Note</b></para>
        /// The fields available to query against for each order are:
        /// OrderNumber:        The order number.
        /// OrderStatus:        The status of the order.
        /// FileCount:          The number of files that have been filed against this order.
        /// ReceivedDateTime:   The time the ORM-O01 HL7 message defining the order was received.
        /// ReferenceDateTime:  A configurable date/time extracted from the message (requested
        ///                     date/time is typical).
        /// </summary>
        /// <value>
        /// The different possible status colors for the buttons and their SQL query conditions.
        /// </value>
        [Editor(typeof(MultilineStringEditor), typeof(UITypeEditor))]
        [Category(_PROPERTY_GRID_CATEGORY)]
        public string ColorQueryConditions
        {
            get
            {
                try
                {
                    return FromOrderedDictionary(FAMData.ColorQueryConditions);
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
                    FAMData.ColorQueryConditions = ToOrderedDictionary(value);
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI38134");
                }
            }
        }

        #endregion Properties  

        #region Methods

        /// <summary>
        /// Gets the descriptions of all orders on the active document that have previously been
        /// submitted via LabDE.
        /// </summary>
        /// <returns>The descriptions of all orders on the active document that have previously been
        /// submitted.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public IEnumerable<string> GetPreviouslySubmittedOrders()
        {
            try
            {
                LoadDataForAllRows();

                return FAMData.GetPreviouslySubmittedOrders();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI38188");
            }
        }

        /// <summary>
        /// Links the order numbers currently in the order table with the active document in the
        /// FAM DB (via the LabDEOrderFile table). If the link already exists, the collection date
        /// will be modified if necessary to match the currently specified collection date.
        /// </summary>
        public void LinkFileWithOrders()
        {
            try
            {
                LoadDataForAllRows();

                string currentFileName = DataEntryControlHost.ImageViewer.ImageFile;
                int fileId = FileProcessingDB.GetFileID(currentFileName);

                FAMData.LinkFileWithCurrentOrders(fileId);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI38182");
            }
        }

        /// <summary>
        /// Clears all data currently cached to force it to be re-retrieved from the FAM DB next
        /// time it is needed.
        /// </summary>
        public void ClearCachedData()
        {
            try
            {
                FAMData.ClearCachedData();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI38194");
            }
        }

        #endregion Methods

        #region Overrides

        /// <summary>
        /// Creates an exact copy of the <see cref="DataEntryTableColumn"/> instance.
        /// </summary>
        /// <returns>An exact copy of the <see cref="DataEntryTableColumn"/> instance.</returns>
        public override object Clone()
        {
            try
            {
                OrderPickerTableColumn column = (OrderPickerTableColumn)base.Clone();

                // Copy OrderPickerTableColumn specific properties
                column.OrderNumberColumn = this.OrderNumberColumn;
                column.PatientMRNAttribute = this.PatientMRNAttribute;
                column.OrderCodeAttribute = this.OrderCodeAttribute;
                column.ShowUnavailableOrders = this.ShowUnavailableOrders;
                column.CustomOrderMatchCriteriaQuery = this.CustomOrderMatchCriteriaQuery;
                column.OrderQueryColumns = this.OrderQueryColumns;
                column.ColorQueryConditions = this.ColorQueryConditions;
                
                return column;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI38135", ex);
            }
        }

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
                if (!_inDesignMode && DataGridView is DataEntryTableBase)
                {
                    DataGridView.HandleCreated += DataGridView_HandleCreated;
                    DataGridView.CellContentClick += DataGridView_CellContentClick;
                    DataGridView.CellPainting += DataGridView_CellPainting;
                    DataGridView.Rows.CollectionChanged += HandleDataGridViewRows_CollectionChanged;
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
                if (_famData != null)
                {
                    _famData.RowDataUpdated -= HandleFamData_RowDataUpdated;
                    _famData.OrderAttributeValueModified -= HandleFamData_OrderAttributeValueModified;
                    _famData.Dispose();
                    _famData = null;
                }
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
        void DataGridView_HandleCreated(object sender, EventArgs e)
        {
            try
            {
                // Once the grid is ready for display, resolve _orderNumberColumn.
                _orderNumberColumn = DataGridView.Columns
                    .OfType<DataEntryTableColumn>()
                    .SingleOrDefault(column => column.Name == OrderNumberColumn);

                FAMData.OrderNumberAttribute = _orderNumberColumn.AttributeName;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI38137");
            }
        }

        /// <summary>
        /// Handles the <see cref="DataGridView.CellPainting"/> event of the
        /// <see cref="DataGridView"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.Forms.DataGridViewCellPaintingEventArgs"/>
        /// instance containing the event data.</param>
        void DataGridView_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            try
            {
                e.PaintBackground(e.CellBounds, false);

                // Paint the with the appropriate status color.
                if (e.RowIndex >= 0 && e.ColumnIndex == Index)
                {
                    var dataEntryRow = DataGridView.Rows[e.RowIndex] as DataEntryTableRow;
                    if (dataEntryRow != null)
                    {
                        FAMOrderRow rowData = FAMData.GetRowData(dataEntryRow);
                        if (rowData != null && rowData.StatusColor.HasValue)
                        {
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
        void DataGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                // Activate the order-picker UI (FFI).
                if (e.RowIndex >= 0 && this == DataGridView.Columns[e.ColumnIndex])
                {
                    var tableRow = DataGridView.Rows[e.RowIndex];
                    var dataEntryRow = tableRow as DataEntryTableRow;
                    if (dataEntryRow != null)
                    {
                        var orderNumberCell = tableRow.Cells[_orderNumberColumn.Index];
                        var initialValue = orderNumberCell.Value.ToString();

                        // If a selection was made, apply the new order number.
                        string orderNumber = ShowPickerFFI(dataEntryRow, initialValue);
                        if (!string.IsNullOrWhiteSpace(orderNumber))
                        {
                            orderNumberCell.Value = orderNumber;

                            // Setting the order number cell as active helps indicate that a value
                            // was applied.
                            DataGridView.CurrentCell = orderNumberCell;
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
        /// <see cref="DataGridView"/>..
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
        /// <param name="e">The <see cref="RowDataUpdatedArgs"/> instance containing the
        /// event data.</param>
        void HandleFamData_RowDataUpdated(object sender, RowDataUpdatedArgs e)
        {
            try
            {
                var row = e.FAMOrderRow.DataEntryTableRow;
                if (row != null)
                {
                    var cell = row.Cells[Index];
                    if (cell != null && cell.DataGridView == DataGridView)
                    {
                        DataGridView.InvalidateCell(cell);
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
        /// Handles the <see cref="E:FAMData.OrderAttributeValueModified"/> event of the
        /// <see cref="FAMData"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Extract.DataEntry.AttributeValueModifiedEventArgs"/>
        /// instance containing the event data.</param>
        void HandleFamData_OrderAttributeValueModified(object sender, AttributeValueModifiedEventArgs e)
        {
            try
            {
                var dataEntryTable = DataGridView as DataEntryTableBase;
                if (dataEntryTable != null)
                {
                    // Update the tooltip text for both the order number and picker cells of the row
                    // affected by the change to display a text summary of the currently selected
                    // order.
                    IDataEntryTableCell orderCell =
                        dataEntryTable.GetAttributeUIElement(e.Attribute) as IDataEntryTableCell;
                    DataGridViewCell pickerButtonCell =
                        orderCell.AsDataGridViewCell.OwningRow.Cells[Index];
                    string orderDescription = FAMData.GetOrderDescription(e.Attribute.Value.String);
                    orderCell.AsDataGridViewCell.ToolTipText = orderDescription;
                    pickerButtonCell.ToolTipText = orderDescription;
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI38169");
            }
        }

        #endregion Event Handlers

        #region Private Members

        /// <summary>
        /// Gets the <see cref="DataEntryControlHost"/>.
        /// </summary>
        DataEntryControlHost DataEntryControlHost
        {
            get
            {
                if (_dataEntryControlHost == null)
                {
                    var dataEntryTable = DataGridView as DataEntryTableBase;
                    if (dataEntryTable != null)
                    {
                        _dataEntryControlHost = dataEntryTable.DataEntryControlHost;
                    }
                }
                return _dataEntryControlHost;
            }
        }

        /// <summary>
        /// Gets the <see cref="FileProcessingDB"/>
        /// </summary>
        FileProcessingDB FileProcessingDB
        {
            get
            {
                return _inDesignMode || (DataEntryControlHost == null)
                    ? null
                    : DataEntryControlHost.DataEntryApplication.FileProcessingDB;
            }
        }
        
        /// <summary>
        /// Gets the <see cref="FAMData"/> that allows access to the order data stored in the FAM
        /// database.
        /// </summary>
        FAMData FAMData
        {
            get
            {
                if (_famData == null)
                {
                    _famData = new FAMData(FileProcessingDB);
                    _famData.RowDataUpdated += HandleFamData_RowDataUpdated;
                    _famData.OrderAttributeValueModified += HandleFamData_OrderAttributeValueModified;
                }

                _famData.FileProcessingDB = FileProcessingDB;

                return _famData;
            }
        }

        /// <summary>
        /// Ensures <see cref="FAMData"/> has loaded data from the DB for all rows in the
        /// <see cref="DataGridView"/>.
        /// </summary>
        void LoadDataForAllRows()
        {
            foreach (DataGridViewRow row in DataGridView.Rows)
            {
                FAMOrderRow rowData = FAMData.GetRowData(row as DataEntryTableRow);
            }
        }

        /// <summary>
        /// Displays the order selection UI FFI for the specified <see paramref="dataEntryRow"/>.
        /// </summary>
        /// <param name="dataEntryRow">The <see cref="DataEntryTableRow"/> to show the UI for.</param>
        /// <param name="defaultOrderNumber">The order number that should be selected by default in
        /// the picker UI.</param>
        /// <returns>The order number that was selected from the picker UI; <see langword="null"/> if
        /// the UI was cancelled without having chosen an order number.
        /// </returns>
        string ShowPickerFFI(DataEntryTableRow dataEntryRow, string defaultOrderNumber)
        {
            FAMOrderRow rowData = FAMData.GetRowData(dataEntryRow);
            if (rowData != null)
            {
                using (var fileInspectorForm = new FAMFileInspectorForm())
                using (var selectionPane = new OrderPickerSelectionPane())
                {
                    fileInspectorForm.UseDatabaseMode = true;
                    fileInspectorForm.FileProcessingDB.DuplicateConnection(FileProcessingDB);

                    fileInspectorForm.FileSelectorPane = selectionPane;
                    selectionPane.SelectedOrderNumber = defaultOrderNumber;
                    selectionPane.RowData = rowData;
                    selectionPane.UpdateOrderSelectionGrid();

                    if (fileInspectorForm.ShowDialog(DataEntryControlHost) == DialogResult.OK)
                    {
                        return selectionPane.SelectedOrderNumber;
                    }
                }
            }

            return null;
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
    /// <see cref="OrderPickerTableColumn.OrderNumberColumn"/> field in a property grid. This
    /// control causes a drop-down list of the names of the columns in the current table.
    /// </summary>
    internal class OrderNumberColumnSelector : ObjectSelectorEditor
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
                OrderPickerTableColumn orderPickerColumn =
                    context.Instance.GetType()
                    .GetField("column", BindingFlags.Instance | BindingFlags.NonPublic)
                    .GetValue(context.Instance)
                    as OrderPickerTableColumn;
                DataGridView dataGridView = orderPickerColumn.DataGridView;

                // All column names except for the order picker column should be available for
                // selection.
                selector.Nodes.AddRange(
                    dataGridView.Columns
                        .OfType<DataGridViewColumn>()
                        .Where(column => column != orderPickerColumn)
                        .Select(column => new SelectorNode(column.Name, column.Name))
                        .ToArray());

                // Select by default the column the is currently configured as the
                // OrderNumberColumn.
                selector.SelectedNode =
                    selector.Nodes.OfType<SelectorNode>()
                    .Where(node => orderPickerColumn.OrderNumberColumn != null &&
                        orderPickerColumn.OrderNumberColumn.Equals(
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
