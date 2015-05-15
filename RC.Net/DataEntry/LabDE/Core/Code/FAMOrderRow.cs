using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using System.Linq;
using UCLID_AFCORELib;

namespace Extract.DataEntry.LabDE
{
    /// <summary>
    /// Provides access to data pertaining to a row in a LabDE order table.
    /// </summary>
    internal class FAMOrderRow : IDisposable
    {
        #region Fields

        /// <summary>
        /// The <see cref="IAttribute"/> currently representing the MRN associated with the LabDE
        /// order.
        /// </summary>
        IAttribute _patientMRNAttribute;

        /// <summary>
        /// The <see cref="IAttribute"/> currently representing the order code associated with the
        /// LabDE order.
        /// </summary>
        IAttribute _orderCodeAttribute;

        /// <summary>
        /// The <see cref="IAttribute"/> currently representing the order number associated with the
        /// LabDE order.
        /// </summary>
        IAttribute _orderNumberAttribute;

        /// <summary>
        /// A HashSet of potentially matching order numbers for the current LabDE order.
        /// </summary>
        HashSet<string> _matchingOrderIds;

        /// <summary>
        /// A cached <see cref="DataTable"/> of potential matching FAM DB orders for the current
        /// LabDE order.
        /// </summary>
        DataTable _matchingOrders;

        /// <summary>
        /// A cached <see cref="Color"/> indicating the availability of matching FAM DB orders for
        /// the current LabDE order; <see langword="null"/> if there is no color to indicate the
        /// current status.
        /// </summary>
        Color? _statusColor;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FAMOrderRow"/> class.
        /// </summary>
        /// <param name="famData">The <see cref="FAMData"/> instance that is managing this instance.
        /// </param>
        /// <param name="dataEntryTableRow">The <see cref="DataEntryTableRow"/> representing the
        /// order in the LabDE DEP to which this instance pertains.</param>
        public FAMOrderRow(FAMData famData, DataEntryTableRow dataEntryTableRow)
        {
            try
            {
                FAMData = famData;
                DataEntryTableRow = dataEntryTableRow;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI38157");
            }
        }

        #endregion Constructors

        #region Events

        /// <summary>
        /// Raised to indicate data associated with this instance has changed.
        /// </summary>
        public event EventHandler<RowDataUpdatedArgs> RowDataUpdated;

        #endregion Events

        #region Properties

        /// <summary>
        /// The <see cref="FAMData"/> instance that is managing this instance.
        /// </summary>
        FAMData FAMData
        {
            get;
            set;
        }

        /// <summary>
        /// The <see cref="DataEntryTableRow"/> representing the order in the LabDE DEP to which
        /// this instance pertains.
        /// </summary>
        public DataEntryTableRow DataEntryTableRow
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the <see cref="IAttribute"/> currently representing the MRN associated with
        /// the LabDE order.
        /// </summary>
        /// <value>
        /// The <see cref="IAttribute"/> currently representing the MRN associated with the LabDE
        /// order.
        /// </value>
        public IAttribute PatientMRNAttribute
        {
            get
            {
                return _patientMRNAttribute;
            }

            set
            {
                try
                {
                    if (value != _patientMRNAttribute)
                    {
                        if (_patientMRNAttribute != null)
                        {
                            var statusInfo = AttributeStatusInfo.GetStatusInfo(_patientMRNAttribute);
                            statusInfo.AttributeValueModified -= Handle_AttributeValueModified;
                            statusInfo.AttributeDeleted -= Handle_AttributeDeleted;
                        }

                        _patientMRNAttribute = value;

                        if (value != null)
                        {
                            var statusInfo = AttributeStatusInfo.GetStatusInfo(value);
                            statusInfo.AttributeValueModified += Handle_AttributeValueModified;
                            statusInfo.AttributeDeleted += Handle_AttributeDeleted;
                        }

                        ClearCachedData(true);
                        // If the FAMData instance is updating this property, it knows of the
                        // changing data (it is in the process of querying row data). No need to
                        // raise DataUpdated.
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI38158");
                }
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="IAttribute"/> representing this order in LabDE
        /// </summary>
        /// <value>
        /// The <see cref="IAttribute"/> representing this order in LabDE.
        /// </value>
        public IAttribute OrderAttribute
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the <see cref="IAttribute"/> currently representing the order number
        /// associated with the LabDE order.
        /// </summary>
        /// <value>
        /// The <see cref="IAttribute"/> currently representing the order number associated with the
        /// LabDE order.
        /// </value>
        public IAttribute OrderNumberAttribute
        {
            get
            {
                return _orderNumberAttribute;
            }

            set
            {
                try
                {
                    if (value != _orderNumberAttribute)
                    {
                        if (_orderNumberAttribute != null)
                        {
                            var statusInfo = AttributeStatusInfo.GetStatusInfo(_orderNumberAttribute);
                            statusInfo.AttributeValueModified -= Handle_AttributeValueModified;
                            statusInfo.AttributeDeleted -= Handle_AttributeDeleted;
                        }

                        _orderNumberAttribute = value;

                        if (value != null)
                        {
                            var statusInfo = AttributeStatusInfo.GetStatusInfo(value);
                            statusInfo.AttributeValueModified += Handle_AttributeValueModified;
                            statusInfo.AttributeDeleted += Handle_AttributeDeleted;
                        }

                        // Changes to the order number don't require the database to be re-queried.
                        ClearCachedData(false);
                        // If the FAMData instance is updating this property, it knows of the
                        // changing data (it is in the process of querying row data). No need to
                        // raise DataUpdated.
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI38232");
                }
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="IAttribute"/> currently representing the order code
        /// associated with the LabDE order.
        /// </summary>
        /// <value>
        /// The <see cref="IAttribute"/> currently representing the order code associated with the
        /// LabDE order.
        /// </value>
        public IAttribute OrderCodeAttribute
        {
            get
            {
                return _orderCodeAttribute;
            }

            set
            {
                try
                {
                    if (value != _orderCodeAttribute)
                    {
                        if (_orderCodeAttribute != null)
                        {
                            var statusInfo = AttributeStatusInfo.GetStatusInfo(_orderCodeAttribute);
                            statusInfo.AttributeValueModified -= Handle_AttributeValueModified;
                            statusInfo.AttributeDeleted -= Handle_AttributeDeleted;
                        }

                        _orderCodeAttribute = value;

                        if (value != null)
                        {
                            var statusInfo = AttributeStatusInfo.GetStatusInfo(value);
                            statusInfo.AttributeValueModified += Handle_AttributeValueModified;
                            statusInfo.AttributeDeleted += Handle_AttributeDeleted;
                        }

                        ClearCachedData(true);
                        // If the FAMData instance is updating this property, it knows of the
                        // changing data (it is in the process of querying row data). No need to
                        // raise DataUpdated.
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI38159");
                }
            }
        }

        /// <summary>
        /// Gets the MRN associated with the LabDE order.
        /// </summary>
        public string PatientMRN
        {
            get
            {
                try
                {
                    return (PatientMRNAttribute != null)
                                ? PatientMRNAttribute.Value.String
                                : "";
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI38160");
                }
            }
        }

        /// <summary>
        /// Gets the order code associated with the LabDE order.
        /// </summary>
        public string OrderCode
        {
            get
            {
                try
                {
                    return (OrderCodeAttribute != null)
                                ? OrderCodeAttribute.Value.String
                                : "";
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI38161");
                }
            }
        }

        /// <summary>
        /// Gets the order number associated with the LabDE order.
        /// </summary>
        public string OrderNumber
        {
            get
            {
                try
                {
                    return (OrderNumberAttribute != null)
                            ? OrderNumberAttribute.Value.String
                            : "";
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI38234");
                }
            }
        }

        /// <summary>
        /// Gets or sets a HashSet of potentially matching order numbers for the current LabDE order.
        /// </summary>
        /// <value>
        /// A HashSet of potentially matching order numbers for the current LabDE order. 
        /// </value>
        public HashSet<string> MatchingOrderIDs
        {
            get
            {
                return _matchingOrderIds;
            }

            set
            {
                _matchingOrderIds = value;
            }
        }

        /// <summary>
        /// Gets a <see cref="DataTable"/> of potential matching FAM DB orders for the current LabDE
        /// order.
        /// </summary>
        /// <value>
        /// A <see cref="DataTable"/> of potential matching FAM DB orders for the current LabDE order.
        /// </value>
        public DataTable MatchingOrders
        {
            get
            {
                if (_matchingOrders == null)
                {
                    _matchingOrders = FAMData.GetMatchingOrders(this);
                }

                return _matchingOrders;
            }
        }

        /// <summary>
        /// Gets the matching orders that haven't already been mapped to another
        /// <see cref="FAMOrderRow"/>.
        /// </summary>
        /// <returns>A <see cref="DataView"/> representing matching orders that haven't already been
        /// mapped to another <see cref="FAMOrderRow"/>.</returns>
        public DataView UnmappedMatchingOrders
        {
            get
            {
                try
                {
                    // Put the unmapped orders in to a string list excluding this row's currently
                    // mapped order number. (order numbers will be quoted for SQL).
                    string otherAlreadyMappedOrders = string.Join(",",
                        FAMData.AlreadyMappedOrderNumbers
                            .Except(new[] { "'" + OrderNumber + "'" }, 
                                StringComparer.OrdinalIgnoreCase));

                    // If there are any already mapped orders to be excluded, add a row filter for
                    // the view.
                    DataView unmappedOrdersView = new DataView(MatchingOrders);
                    if (!string.IsNullOrWhiteSpace(otherAlreadyMappedOrders))
                    {
                        string orderNumberColumnName = (string)FAMData.OrderQueryColumns
                            .OfType<DictionaryEntry>()
                            .First()
                            .Key;

                        unmappedOrdersView.RowFilter = string.Format(CultureInfo.InvariantCulture,
                            "[{0}] NOT IN ({1})", orderNumberColumnName, otherAlreadyMappedOrders);
                    }

                    return unmappedOrdersView;
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI38233");
                }
            }
        }

        /// <summary>
        /// Gets a <see cref="Color"/> indicating the availability of matching FAM DB orders for the
        /// current LabDE order.
        /// </summary>
        /// <value>
        /// A <see cref="Color"/> indicating the availability of matching FAM DB orders for the
        /// current LabDE order; <see langword="null"/> if there is no color to indicate the current
        /// status.
        /// </value>
        public Color? StatusColor
        {
            get
            {
                if (!_statusColor.HasValue)
                {
                    _statusColor = FAMData.GetOrdersStatusColor(this);
                }

                return _statusColor.Value;
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Gets the file IDs for files that have already been filed against
        /// <see paramref="orderNumber"/> in LabDE.
        /// </summary>
        /// <param name="orderNumber">The order number.</param>
        /// <returns>The file IDs for files that have already been filed against the order number.
        /// </returns>
        public IEnumerable<int> GetCorrespondingFileIds(string orderNumber)
        {
            try
            {
                return FAMData.GetCorrespondingFileIDs(orderNumber);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI38162");
            }
        }

        /// <summary>
        /// Clears all data currently cached to force it to be re-retrieved next time it is needed.
        /// </summary>
        /// <param name="clearDatabaseData"><see langword="true"/> to clear data cached from the
        /// database; <see langword="false"/> to clear only data obtained from the UI.</param>
        public void ClearCachedData(bool clearDatabaseData)
        {
            if (clearDatabaseData)
            {
                if (_matchingOrders != null)
                {
                    _matchingOrders.Dispose();
                    _matchingOrders = null;
                }

                _matchingOrderIds = null;
            }

            // Status color depends both on DB and UI data... it must be cleared for all calls into
            // ClearCachedData.
            _statusColor = null;
        }

        #endregion Methods

        #region IDisposable Members

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged 
        /// resources; <see langword="false"/> to release only unmanaged resources.</param>        
        void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Unregisters events for attribute modification/deletes.
                PatientMRNAttribute = null;
                OrderCodeAttribute = null;

                if (_matchingOrders != null)
                {
                    _matchingOrders.Dispose();
                    _matchingOrders = null;
                }
            }

            // Dispose of unmanaged resources
        }

        #endregion IDisposable Members

        /// <summary>
        /// Handles the <see cref="AttributeStatusInfo.AttributeValueModified"/> event for one of
        /// the <see cref="IAttribute"/>s containing key data used in determining potential matching
        /// order codes.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Extract.DataEntry.AttributeValueModifiedEventArgs"/>
        /// instance containing the event data.</param>
        public void Handle_AttributeValueModified(object sender, AttributeValueModifiedEventArgs e)
        {
            try
            {
                if (!e.IncrementalUpdate)
                {
                    bool orderNumberUpdated = (e.Attribute == OrderNumberAttribute);
                    ClearCachedData(!orderNumberUpdated);

                    OnRowDataUpdated(orderNumberUpdated);
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI38163");
            }
        }

        /// <summary>
        /// Handles the <see cref="AttributeStatusInfo.AttributeValueModified"/> event for one of
        /// the <see cref="IAttribute"/>s containing key data used in determining potential matching
        /// order codes.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Extract.DataEntry.AttributeValueModifiedEventArgs"/>
        /// instance containing the event data.</param>
        public void Handle_AttributeDeleted(object sender, AttributeDeletedEventArgs e)
        {
            try
            {
                var statusInfo = AttributeStatusInfo.GetStatusInfo(e.DeletedAttribute);
                statusInfo.AttributeValueModified -= Handle_AttributeValueModified;
                statusInfo.AttributeDeleted -= Handle_AttributeDeleted;

                if (e.DeletedAttribute == PatientMRNAttribute)
                {
                    PatientMRNAttribute = null;
                }
                else if (e.DeletedAttribute == OrderCodeAttribute)
                {
                    OrderCodeAttribute = null;
                }

                ClearCachedData(true);
                OnRowDataUpdated(e.DeletedAttribute == OrderNumberAttribute);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI38164");
            }
        }
        
        #region Private Members

        /// <summary>
        /// Raises the <see cref="RowDataUpdated"/> event.
        /// </summary>
        /// <param name="orderNumberUpdated"><see langword="true"/> if the order number has been
        /// changed; otherwise, <see langword="false"/>.</param>
        void OnRowDataUpdated(bool orderNumberUpdated)
        {
            if (RowDataUpdated != null)
            {
                RowDataUpdated(this, new RowDataUpdatedArgs(this, orderNumberUpdated));
            }
        }

        #endregion Private Members
    }
}
