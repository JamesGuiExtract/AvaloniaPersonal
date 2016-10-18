using System;
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
    /// Provides access to data pertaining to a record to be mapped in a LabDE table.
    /// </summary>
    internal abstract class DocumentDataRecord: IDisposable
    {
        #region Fields

        /// <summary>
        /// A cached <see cref="DataTable"/> of potential matching FAM DB records for the current
        /// instance.
        /// </summary>
        DataTable _matchingRecords;

        /// <summary>
        /// A cached <see cref="Color"/> indicating the availability of matching FAM DB records for
        /// the current instance; <see langword="null"/> if there is no color to indicate the
        /// current status.
        /// </summary>
        Color? _statusColor;

        IAttribute _attribute;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentDataRecord"/> class.
        /// </summary>
        /// <param name="famData">The <see cref="FAMData"/> instance that is managing this instance.
        /// </param>
        /// <param name="dataEntryTableRow">The <see cref="DataEntryTableRow"/> representing the
        /// record in the LabDE DEP to which this instance pertains.</param>
        /// <param name="attribute">The <see cref="IAttribute"/> representing the current record</param>
        protected DocumentDataRecord(FAMData famData, DataEntryTableRow dataEntryTableRow, IAttribute attribute)
        {
            try
            {
                Fields = new HashSet<DocumentDataField>();
                FAMData = famData;
                DataEntryTableRow = dataEntryTableRow;
                Attribute = attribute;
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
        public FAMData FAMData
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the <see cref="IFAMDataConfiguration"/> for this record.
        /// </summary>
        /// <value>
        /// The <see cref="IFAMDataConfiguration"/> for this record.
        /// </value>
        public abstract IFAMDataConfiguration FAMDataConfiguration
        {
            get;
        }

        /// <summary>
        /// Gets the <see cref="IAttribute"/> representing this record.
        /// </summary>
        /// <value>
        /// The <see cref="IAttribute"/> representing this record.
        /// </value>
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public IAttribute Attribute
        {
            get
            {
                return _attribute;
            }

            set
            {
                try
                {
                    if (value != _attribute)
                    {
                        _attribute = value;

                        foreach (var field in Fields)
                        {
                            field.RecordAttribute = value;
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI41513");
                }
            }
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
        /// Gets or sets the <see cref="DocumentDataField"/> representing the ID.
        /// </summary>
        /// <value>
        /// The <see cref="DocumentDataField"/> representing the ID.
        /// </value>
        public abstract DocumentDataField IdField
        {
            get;
        }

        /// <summary>
        /// Gets all <see cref="DocumentDataField"/>s needed for the current record type.
        /// </summary>
        /// <value>
        /// The <see cref="DocumentDataField"/>s needed for the current record type.
        /// </value>
        protected HashSet<DocumentDataField> Fields
        {
            get;
            private set;
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
                    _statusColor = GetOrdersStatusColor();
                }

                return _statusColor.Value;
            }
        }

        /// <summary>
        /// Gets or sets a HashSet of potentially matching order numbers for the current LabDE order.
        /// </summary>
        /// <value>
        /// A HashSet of potentially matching order numbers for the current LabDE order. 
        /// </value>
        public HashSet<string> MatchingRecordIDs
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a <see cref="DataTable"/> of potential matching FAM DB records for the current instance.
        /// </summary>
        /// <value>
        /// A <see cref="DataTable"/> of potential matching FAM DB orders for the current instance.
        /// </value>
        public virtual DataTable MatchingRecords
        {
            get
            {
                if (_matchingRecords == null)
                {
                    _matchingRecords = FAMData.GetMatchingRecords(this);
                }

                return _matchingRecords;
            }
        }

        /// <summary>
        /// Gets the matching records that haven't already been mapped to another
        /// <see cref="DocumentDataRecord"/>.
        /// </summary>
        /// <returns>A <see cref="DataView"/> representing matching records that haven't already been
        /// mapped to another <see cref="DocumentDataRecord"/>.</returns>
        public virtual DataView UnmappedMatchingRecords
        {
            get
            {
                try
                {
                    // Put the unmapped orders in to a string list excluding this row's currently
                    // mapped order number. (order numbers will be quoted for SQL).
                    string otherAlreadyMappedOrders = string.Join(",",
                        FAMData.AlreadyMappedRecordIDs
                            .Except(new[] { "'" + IdField.Value + "'" }, 
                                StringComparer.OrdinalIgnoreCase));

                    // If there are any already mapped orders to be excluded, add a row filter for
                    // the view.
                    DataView unmappedOrdersView = new DataView(MatchingRecords);
                    if (!string.IsNullOrWhiteSpace(otherAlreadyMappedOrders))
                    {
                        unmappedOrdersView.RowFilter = string.Format(CultureInfo.InvariantCulture,
                            "[{0}] NOT IN ({1})", FAMDataConfiguration.IdFieldName, otherAlreadyMappedOrders);
                    }

                    return unmappedOrdersView;
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI38233");
                }
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Gets an SQL query that retrieves possibly matching order numbers for this instance.
        /// </summary>
        /// <returns>An SQL query that retrieves possibly matching order numbers for this instance.
        /// </returns>
        public abstract string GetSelectedRecordIDsQuery();

        /// <summary>
        /// Gets a <see cref="Color"/> that indicating availability of matching orders for this instance.
        /// </summary>
        /// <returns>A <see cref="Color"/> that indicating availability of matching orders for the
        /// provided <see cref="DocumentDataRecord"/> or <see langword="null"/> if there is no color that
        /// reflects the current status.</returns>
        public abstract Color? GetOrdersStatusColor();

        /// <summary>
        /// Links this record with the specified <see paramref="fileId"/>.
        /// </summary>
        /// <param name="fileId">The file ID to link to the <see paramref="orderNumber"/>.</param>
        public abstract void LinkFileWithRecord( int fileId);

        /// <summary>
        /// Gets the file IDs for files that have already been filed against
        /// <see paramref="orderNumber"/> in LabDE.
        /// </summary>
        /// <param name="recordNumber">The order number.</param>
        /// <returns>The file IDs for files that have already been filed against the order number.
        /// </returns>
        public virtual IEnumerable<int> GetCorrespondingFileIds(string recordNumber)
        {
            try
            {
                return FAMData.GetCorrespondingFileIDs(recordNumber);
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
        public virtual void ClearCachedData(bool clearDatabaseData)
        {
            try
            {
                if (clearDatabaseData)
                {
                    if (_matchingRecords != null)
                    {
                        _matchingRecords.Dispose();
                        _matchingRecords = null;
                    }

                    MatchingRecordIDs = null;
                }

                // Status color depends both on DB and UI data... it must be cleared for all calls into
                // ClearCachedData.
                _statusColor = null;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI41514");
            }
        }

        #endregion Methods

        #region Event Handlers

        /// <summary>
        /// Handles the <see cref="DocumentDataField.AttributeUpdated"/> event for one of
        /// the <see cref="IAttribute"/>s containing key data used in determining potential matching
        /// order codes.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/>
        /// instance containing the event data.</param>
        public void Handle_AttributeUpdated(object sender, EventArgs e)
        {
            try
            {
                bool idFieldUpdated = (sender == IdField);
                ClearCachedData(!idFieldUpdated);

                OnRowDataUpdated(idFieldUpdated);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI38163");
            }
        }

        #endregion Event Handlers

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
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_matchingRecords != null)
                {
                    _matchingRecords.Dispose();
                    _matchingRecords = null;
                }

                foreach (var field in Fields)
                {
                    field.Dispose();
                }
            }

            // Dispose of unmanaged resources
        }

        #endregion IDisposable Members

        #region Private Members

        /// <summary>
        /// Raises the <see cref="RowDataUpdated"/> event.
        /// </summary>
        /// <param name="recordIdUpdated"><see langword="true"/> if the order number has been
        /// changed; otherwise, <see langword="false"/>.</param>
        void OnRowDataUpdated(bool recordIdUpdated)
        {
            if (RowDataUpdated != null)
            {
                RowDataUpdated(this, new RowDataUpdatedArgs(this, recordIdUpdated));
            }
        }

        #endregion Private Members
    }
}
