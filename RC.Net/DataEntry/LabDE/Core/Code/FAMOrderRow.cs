using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
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

        /// <summary>
        /// A cached list of FAM file IDs that have already been submitted against each order in 
        /// <see cref="_matchingOrders"/>.
        /// </summary>
        Dictionary<string, List<int>> _correspondingFileIds = new Dictionary<string, List<int>>();

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
        public event EventHandler<EventArgs> DataUpdated;

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

                        ClearCachedData();
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

                        ClearCachedData();
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
                    _matchingOrders =
                        FAMData.GetMatchingOrders(this, out _correspondingFileIds);
                }

                return _matchingOrders;
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
                return _correspondingFileIds[orderNumber];
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI38162");
            }
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
                    ClearCachedData();
                    OnDataUpdated();
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
                if (e.DeletedAttribute == PatientMRNAttribute)
                {
                    PatientMRNAttribute = null;
                }
                else if (e.DeletedAttribute == OrderCodeAttribute)
                {
                    OrderCodeAttribute = null;
                }

                ClearCachedData();
                OnDataUpdated();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI38164");
            }
        }
        
        #region Private Members

        /// <summary>
        /// Clears all data currently cached to force it to be re-retrieved from the FAM DB next
        /// time it is needed.
        /// </summary>
        void ClearCachedData()
        {
            if (_matchingOrders != null)
            {
                _matchingOrders.Dispose();
                _matchingOrders = null;
            }
            _correspondingFileIds.Clear();
            _statusColor = null;
        }

        /// <summary>
        /// Raises the <see cref="DataUpdated"/> event.
        /// </summary>
        void OnDataUpdated()
        {
            if (DataUpdated != null)
            {
                DataUpdated(this, new EventArgs());
            }
        }

        #endregion Private Members
    }
}
