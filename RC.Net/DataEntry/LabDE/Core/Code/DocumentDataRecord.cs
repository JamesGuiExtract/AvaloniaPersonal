using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using UCLID_AFCORELib;
using static System.FormattableString;

namespace Extract.DataEntry.LabDE
{
    /// <summary>
    /// Provides access to data pertaining to a record to be mapped in a LabDE table.
    /// </summary>
    /// <seealso cref="System.IDisposable" />
    public abstract class DocumentDataRecord : IDisposable
    {
        #region Fields

        /// <summary>
        /// A <see cref="Regex"/> used to parse attribute paths from the
        /// <see cref="RecordMatchCriteria"/>.
        /// </summary>
        Regex _attributeRegex = new Regex(@"{(?<path>[\S\s]+)}", RegexOptions.Compiled);

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

        /// <summary>
        /// The <see cref="IAttribute"/> representing this record.
        /// </summary>
        IAttribute _attribute;

        /// <summary>
        /// The <see cref="IFAMDataConfiguration"/> to be able to retrieve data from a FAM database
        /// for a particular record type.
        /// </summary>
        IFAMDataConfiguration _configuration;

        /// <summary>
        /// The record identifier field
        /// </summary>
        DocumentDataField _idField;

        /// <summary>
        /// Gets the active fields.
        /// </summary>
        Dictionary<IAttribute, DocumentDataField> _activeFields =
            new Dictionary<IAttribute, DocumentDataField>();

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
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "fam")]
        protected DocumentDataRecord(FAMData famData, DataEntryTableRow dataEntryTableRow, IAttribute attribute)
        {
            try
            {
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
        public event EventHandler<RowDataUpdatedEventArgs> RowDataUpdated;

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
        /// Gets the <see cref="IAttribute"/> representing this record.
        /// </summary>
        /// <value>
        /// The <see cref="IAttribute"/> representing this record.
        /// </value>
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

                        ApplyConfiguration(_configuration);
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI41513");
                }
            }
        }

        /// <summary>
        /// The <see cref="DataEntryTableRow"/> representing the record in the LabDE DEP to which
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
        public virtual DocumentDataField IdField
        {
            get
            {
                return _idField;
            }
        }

        /// <summary>
        /// Gets a <see cref="Color"/> indicating the availability of matching FAM DB records for the
        /// current LabDE record.
        /// </summary>
        /// <value>
        /// A <see cref="Color"/> indicating the availability of matching FAM DB records for the
        /// current LabDE record; <see langword="null"/> if there is no color to indicate the current
        /// status.
        /// </value>
        public virtual Color? StatusColor
        {
            get
            {
                if (!_statusColor.HasValue)
                {
                    _statusColor = GetRecordsStatusColor();
                }

                return _statusColor.Value;
            }
        }

        /// <summary>
        /// Gets or sets a HashSet of potentially matching IDs for the current LabDE record.
        /// </summary>
        /// <value>
        /// A HashSet of potentially matching IDs for the current LabDE record. 
        /// </value>
        public virtual HashSet<string> MatchingRecordIds
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets a <see cref="DataTable"/> of potential matching FAM DB records for the current instance.
        /// </summary>
        /// <value>
        /// A <see cref="DataTable"/> of potential matching FAM DB records for the current instance.
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
                    // Put the unmapped records in to a string list excluding this row's currently
                    // mapped record ID. (record IDs will be quoted for SQL).
                    string otherAlreadyMappedRecords = string.Join(",",
                        FAMData.AlreadyMappedRecordIds
                            .Except(new[] { "'" + IdField.Value + "'" },
                                StringComparer.OrdinalIgnoreCase));

                    // If there are any already mapped records to be excluded, add a row filter for
                    // the view.
                    DataView unmappedRecordsView = new DataView(MatchingRecords);
                    if (!string.IsNullOrWhiteSpace(otherAlreadyMappedRecords))
                    {
                        unmappedRecordsView.RowFilter = string.Format(CultureInfo.InvariantCulture,
                            "[{0}] NOT IN ({1})", _configuration.IdFieldDisplayName, otherAlreadyMappedRecords);
                    }

                    return unmappedRecordsView;
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI38233");
                }
            }
        }

        /// <summary>
        /// Gets or sets the default sort.
        /// </summary>
        /// <value>
        /// The default sort.
        /// </value>
        public virtual Tuple<string, ListSortDirection> DefaultSort
        {
            get
            {
                return FAMData.DefaultSort;
            }
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Gets an SQL query that retrieves possibly matching record IDs for this instance.
        /// </summary>
        /// <returns>An SQL query that retrieves possibly matching record IDs for this instance.
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public virtual string GetSelectedRecordIdsQuery()
        {
            try
            {
                string recordIDsQuery = null;

                if (MatchingRecordIds != null)
                {
                    // If the matching order IDs have been cached, select them directly rather than
                    //querying for them.
                    if (MatchingRecordIds.Count > 0)
                    {
                        recordIDsQuery = string.Join("\r\nUNION\r\n",
                            MatchingRecordIds.Select(orderNum => "SELECT " + orderNum));
                    }
                    else
                    {
                        // https://extract.atlassian.net/browse/ISSUE-13003
                        // In the case that we've cached MatchingOrderIDs, but there are none, use a
                        // query of "SELECT NULL"-- without anything at all an SQL syntax error will
                        // result when recordIDsQuery is plugged into a larger query.
                        recordIDsQuery = "SELECT NULL";
                    }
                }
                else
                {
                    var resolvedCriteria = _configuration.RecordMatchCriteria
                        .Select(criteria => _attributeRegex.Replace(criteria, match =>
                            Invariant($"'{_activeFields.Values.Single(field => field.AttributePath == match.Groups["path"].Value).Value}'")));

                    recordIDsQuery = _configuration.BaseSelectQuery;
                    if (resolvedCriteria.Any())
                    {
                        recordIDsQuery += "WHERE " + string.Join("\r\nAND\r\n", resolvedCriteria);
                    }
                }

                return recordIDsQuery;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI41550");
            }
        }

        /// <summary>
        /// Gets a <see cref="Color"/> that indicating availability of matching records for this instance.
        /// </summary>
        /// <returns>A <see cref="Color"/> that indicating availability of matching records for the
        /// provided <see cref="DocumentDataRecord"/> or <see langword="null"/> if there is no color that
        /// reflects the current status.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public abstract Color? GetRecordsStatusColor();

        /// <summary>
        /// Links this record with the specified <see paramref="fileId"/>.
        /// </summary>
        /// <param name="fileId">The file ID to link to this record.</param>
        public abstract void LinkFileWithRecord(int fileId);

        /// <summary>
        /// Gets the file IDs for files that have already been filed against
        /// <see paramref="recordId"/> in LabDE.
        /// </summary>
        /// <param name="recordId">The record ID.</param>
        /// <returns>The file IDs for files that have already been filed against the record ID.
        /// </returns>
        public virtual IEnumerable<int> GetCorrespondingFileIds(string recordId)
        {
            try
            {
                return FAMData.GetCorrespondingFileIds(recordId);
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

                    MatchingRecordIds = null;
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

        /// <summary>
        /// Applies the specified <see paramref="configuration"/> to be used for this instance.
        /// </summary>
        /// <param name="configuration">The <see cref="IFAMDataConfiguration"/> to be able to
        /// retrieve data from a FAM database for a particular record type</param>
        protected void ApplyConfiguration(IFAMDataConfiguration configuration)
        {
            try
            {
                if (_idField != null)
                {
                    _idField.Dispose();
                    _idField = null;
                }
                CollectionMethods.ClearAndDispose(_activeFields);

                _configuration = configuration;

                if (_configuration != null)
                {
                    _idField = new DocumentDataField(Attribute, _configuration.IdFieldAttributePath, true);
                    _idField.AttributeUpdated += Handle_AttributeUpdated;

                    InitializeMatchCriteria(configuration);
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI41551");
            }
        }

        #endregion Methods

        #region Event Handlers

        /// <summary>
        /// Handles the <see cref="DocumentDataField.AttributeUpdated"/> event for one of
        /// the <see cref="IAttribute"/>s containing key data used in determining potential matches.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/>
        /// instance containing the event data.</param>
        void Handle_AttributeUpdated(object sender, EventArgs e)
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

                if (_idField != null)
                {
                    _idField.Dispose();
                    _idField = null;
                }

                CollectionMethods.ClearAndDispose(_activeFields);
            }

            // Dispose of unmanaged resources
        }

        #endregion IDisposable Members

        #region Protected Members

        /// <summary>
        /// Gets the active fields.
        /// </summary>
        protected Dictionary<IAttribute, DocumentDataField> ActiveFields
        {
            get
            {
                return _activeFields;
            }
        }

        #endregion Protected Members

        #region Private Members

        /// <summary>
        /// Parses the <see cref="IFAMDataConfiguration.RecordMatchCriteria"/> of the specified
        /// <see paramref="configuration"/> to be able to find and watch events for referenced
        /// <see cref="IAttribute"/>s.
        /// </summary>
        /// <param name="configuration">The <see cref="IFAMDataConfiguration"/> to be able to
        /// retrieve data from a FAM database for a particular record type</param>
        void InitializeMatchCriteria(IFAMDataConfiguration configuration)
        {
            foreach (string matchCriteria in configuration.RecordMatchCriteria)
            {
                foreach (var match in _attributeRegex.Matches(matchCriteria).OfType<Match>())
                {
                    var field = new DocumentDataField(
                        Attribute,
                        match.Groups["path"].Value,
                        true);
                    if (field.Attribute != null &&
                        !_activeFields.ContainsKey(field.Attribute))
                    {
                        _activeFields[field.Attribute] = field;
                        field.AttributeUpdated += Handle_AttributeUpdated;
                    }
                    else
                    {
                        field.Dispose();
                    }
                }
            }
        }

        /// <summary>
        /// Raises the <see cref="RowDataUpdated"/> event.
        /// </summary>
        /// <param name="recordIdUpdated"><see langword="true"/> if the record ID has been
        /// changed; otherwise, <see langword="false"/>.</param>
        protected virtual void OnRowDataUpdated(bool recordIdUpdated)
        {
            if (RowDataUpdated != null)
            {
                RowDataUpdated(this, new RowDataUpdatedEventArgs(this, recordIdUpdated));
            }
        }

        #endregion Private Members
    }
}
