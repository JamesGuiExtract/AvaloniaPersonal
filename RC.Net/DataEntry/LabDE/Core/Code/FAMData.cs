using Extract.Database;
using Extract.SqlDatabase;
using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using UCLID_FILEPROCESSINGLib;

namespace Extract.DataEntry.LabDE
{
    /// <summary>
    /// Provides access to the LabDE related data in a FAM database.
    /// </summary>
    public class FAMData : IDisposable
    {
        #region Fields

        /// <summary>
        /// An <see cref="ExtractRoleConnection"/> for use when querying against the specified database
        /// <see cref="FileProcessingDB"/>.
        /// </summary>
        ExtractRoleConnection _extractRole;

        /// <summary>
        /// Maps each <see cref="DataEntryTableRow"/> in a LabDE table to an
        /// <see cref="DocumentDataRecord"/> instance to manage access to related FAM DB data.
        /// </summary>
        Dictionary<DataEntryTableRow, DocumentDataRecord> _rowData =
            new Dictionary<DataEntryTableRow, DocumentDataRecord>();

        /// <summary>
        /// Cached info for the most recently accessed records from the FAM DB.
        /// </summary>
        DataCache<string, DocumentDataRecordInfo> _recordInfoCache = new DataCache<string, DocumentDataRecordInfo>(100, null);

        /// <summary>
        /// A single-quoted list of record IDs that are currently assigned in the UI.
        /// </summary>
        List<string> _alreadyMappedRecordIDs = null;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="FAMData"/> class.  
        /// </summary>
        /// <param name="fileProcessingDB">The <see cref="FileProcessingDB"/> containing the LabDE
        /// data.</param>
        public FAMData(FileProcessingDB fileProcessingDB)
        {
            try
            {
                FileProcessingDB = fileProcessingDB;

                AttributeStatusInfo.DataReset += HandleAttributeStatusInfo_DataReset;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI38150");
            }
        }

        #endregion Constructors

        #region Events

        /// <summary>
        /// Raised to indicate data associated with one of the table rows being managed has changed.
        /// </summary>
        public event EventHandler<RowDataUpdatedEventArgs> RowDataUpdated;

        #endregion Events

        #region Properties

        /// <summary>
        /// Gets or sets the <see cref="FileProcessingDB"/> containing the LabDE data.
        /// </summary>
        /// <value>
        /// The <see cref="FileProcessingDB"/> containing the LabDE data.
        /// </value>
        public FileProcessingDB FileProcessingDB
        {
            get;
            set;
        }

        /// <summary>
        /// The data configuration
        /// </summary>
        public virtual IFAMDataConfiguration DataConfiguration
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a value indicating whether this instance currently has any
        /// <see cref="DocumentDataRecord"/> instances.
        /// </summary>
        /// <value><see langword="true"/> if this instance has any
        /// <see cref="DocumentDataRecord"/> instance; otherwise, <see langword="false"/>.
        /// </value>
        public virtual bool HasRowData
        {
            get
            {
                return (_rowData.Count > 0);
            }
        }

        /// <summary>
        /// Gets the currently loaded <see cref="DocumentDataRecord"/> for the table's current rows.
        /// </summary>
        public virtual IEnumerable<DocumentDataRecord> LoadedRecords
        {
            get
            {
                return _rowData.Values;
            }
        }

        /// <summary>
        /// Gets a single-quoted list of record IDs that are currently assigned in the UI.
        /// </summary>
        public virtual ReadOnlyCollection<string> AlreadyMappedRecordIds
        {
            get
            {
                try
                {
                    if (_alreadyMappedRecordIDs == null)
                    {
                        _alreadyMappedRecordIDs = _rowData.Values
                            .Select(row => row.IdField.Value)
                            .Where(id => !string.IsNullOrWhiteSpace(id))
                            .Select(id => "'" + id.Replace("'", "''") + "'")
                            .ToList();
                    }

                    return _alreadyMappedRecordIDs.AsReadOnly();
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI41516");
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
            get;
            private set;
        }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Gets a <see cref="DataTable"/> with the records specified by <see paramref="recordIDs"/>.
        /// Info for the records is cached for subsequent calls to <see cref="GetRecordDescription"/> or
        /// <see cref="GetCorrespondingFileIds"/>.
        /// </summary>
        /// <param name="recordIds">A comma delimited list of record IDs to be returned or an SQL
        /// query that selects the record numbers to be returned.</param>
        /// <param name="queryForRecordIds"><see langword="true"/> if
        /// <see paramref="recordIDs"/> represents an SQL query to select the appropriate record IDs,
        /// <see langword="false"/> if the record IDs are specified via a comma separated list (in
        /// which case the record IDs should be single-quoted for proper string comparisons).
        /// </param>
        /// <returns>A <see cref="DataTable"/> listing specified records.</returns>
        public virtual DataTable LoadRecordInfo(string recordIds, bool queryForRecordIds)
        {
            try
            {
                var recordInfoQuery = DataConfiguration.GetRecordInfoQuery(recordIds, queryForRecordIds);

                string lastRecordID = null;
                using (DataTable results = DBMethods.ExecuteDBQuery(SqlDbAppRoleConnection, recordInfoQuery))
                {
                    // The results will contain both an extra column (FileID) not expected by the
                    // caller, as well as potentially multiple rows per record for records for which
                    // multiple files have been submitted. Generate the eventual return value as a
                    // copy of results to initialize the columns, but we will then manually copy only
                    // the rows expected by the caller.
                    DataTable matchingRecords = results.Copy();
                    matchingRecords.Rows.Clear();

                    // Remove the FileID column that was not specified by recordInfoQuery, but rather
                    // used behind the scenes to compile correspondingFileIds.
                    matchingRecords.Columns.Remove("FileID");

                    var sortedColumn = matchingRecords.Columns
                        .OfType<DataColumn>()
                        .Select(column =>
                            new Tuple<DataColumn, Match>(
                                column, Regex.Match(column.ColumnName, @"\s(ASC|DESC)")))
                        .SingleOrDefault(item => item.Item2.Success);

                    if (sortedColumn != null)
                    {
                        sortedColumn.Item1.Caption =
                            sortedColumn.Item1.ColumnName.Substring(0, sortedColumn.Item2.Index);
                        DefaultSort = new Tuple<string, ListSortDirection>(sortedColumn.Item1.ColumnName,
                            (sortedColumn.Item2.Value == " ASC")
                                ? ListSortDirection.Ascending
                                : ListSortDirection.Descending);
                    }
                    else
                    {
                        DefaultSort = null;
                    }

                    DocumentDataRecordInfo recordInfo = null;

                    // Loop to copy a single row per distinct record into matchingRecords.
                    foreach (DataRow row in results.Rows
                        .OfType<DataRow>()
                        .OrderBy(row => row.ItemArray[0].ToString()))
                    {
                        string recordID = row.ItemArray[0].ToString();
                        if (recordID != lastRecordID)
                        {
                            matchingRecords.ImportRow(row);
                            lastRecordID = recordID;

                            // Cache the record info for GetRecordDescription or GetCorrespondingFileIds.
                            recordInfo = new DocumentDataRecordInfo();
                            recordInfo.Description = string.Join("\r\n", matchingRecords.Columns
                                .OfType<DataColumn>()
                                .Select(column => column.Caption + ": " +
                                    row.ItemArray[column.Ordinal].ToString()));
                            recordInfo.CorrespondingFileIds = new List<int>();

                            _recordInfoCache.CacheData(recordID, recordInfo);
                        }

                        // Collect all associated file IDs for the current record.
                        object fileIdValue = row.ItemArray.Last();
                        if (!(fileIdValue is System.DBNull))
                        {
                            recordInfo.CorrespondingFileIds.Add((int)fileIdValue);
                        }
                    }

                    return matchingRecords;
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI41515");
            }
        }

        /// <summary>
        /// Gets a <see cref="DocumentDataRecord"/> instance that provides access to FAM database data for
        /// the specified <see paramref="dataEntryRow"/>.
        /// </summary>
        /// <param name="dataEntryRow">The <see cref="DataEntryTableRow"/> for which data is needed.
        /// </param>
        /// <returns>a <see cref="DocumentDataRecord"/> instance that provides access to FAM database data.
        /// </returns>
        public virtual DocumentDataRecord GetRowData(DataEntryTableRow dataEntryRow)
        {
            try
            {
                if (dataEntryRow == null || dataEntryRow.Attribute == null)
                {
                    return null;
                }

                // Create a new DocumentDataRecord instance if one does not already exist for this row.
                DocumentDataRecord rowData = null;
                if (!_rowData.TryGetValue(dataEntryRow, out rowData))
                {
                    rowData = DataConfiguration.CreateDocumentDataRecord(
                        this, dataEntryRow, dataEntryRow.Attribute);
                    _rowData[dataEntryRow] = rowData;
                    rowData.RowDataUpdated += HandleRowData_DataUpdated;

                    // After initially creating a new DocumentDataRecord, raise OnRowDataUpdated so
                    // the record picker column will know of the record ID's initial value (if
                    // the record ID exists).
                    OnRowDataUpdated(new RowDataUpdatedEventArgs(rowData,
                        !string.IsNullOrWhiteSpace(rowData.IdField.Value)));
                }
                else
                {
                    rowData.Attribute = dataEntryRow.Attribute;
                }

                return rowData;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI38153");
            }
        }

        /// <summary>
        /// Gets a text description for the specified <see paramref="recordID"/>. 
        /// </summary>
        /// <param name="recordID">The record ID for which a description is needed.</param>
        /// <returns>A text description of the record.</returns>
        public virtual string GetRecordDescription(string recordID)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(recordID))
                {
                    DocumentDataRecordInfo recordInfo = GetRecordInfo(recordID);
                    if (recordInfo != null)
                    {
                        return recordInfo.Description;
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI38166");
            }
        }

        /// <summary>
        /// Gets a <see cref="DataTable"/> listing the possibly matching FAM DB records for
        /// the provided <see cref="DocumentDataRecord"/> with the columns being those defined by
        /// the query returned by <see cref="IFAMDataConfiguration.GetRecordInfoQuery"/>
        /// </summary>
        /// <param name="record">The <see cref="DocumentDataRecord"/> for which matching records are to be
        /// retrieved.</param>
        /// <returns>A <see cref="DataTable"/> listing the possibly matching records.</returns>
        public virtual DataTable GetMatchingRecords(DocumentDataRecord record)
        {
            try
            {
                string selectedRecordIDsQuery = record.GetSelectedRecordIdsQuery();

                // LoadRecordInfo has the side-effect of caching data from selectedRecordIDsQuery into
                // _recordInfoCache
                DataTable matchingRecordTable = LoadRecordInfo(selectedRecordIDsQuery, true);

                // Cache the list of records if they have not yet been.
                if (record.MatchingRecordIds == null)
                {
                    record.MatchingRecordIds = new HashSet<string>(
                        matchingRecordTable.Rows
                            .OfType<DataRow>()
                            .Select(row => "'" + row.ItemArray[0].ToString() + "'"));
                }

                return matchingRecordTable;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI38176");
            }
        }

        /// <summary>
        /// Gets the file IDs for files that have already been filed against
        /// <see paramref="recordID"/> in LabDE.
        /// </summary>
        /// <param name="recordId">The record ID.</param>
        /// <returns>The file IDs for files that have already been filed against the record ID.
        /// </returns>
        public virtual IEnumerable<int> GetCorrespondingFileIds(string recordId)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(recordId))
                {
                    DocumentDataRecordInfo recordInfo = GetRecordInfo(recordId);
                    if (recordInfo != null)
                    {
                        return recordInfo.CorrespondingFileIds;
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI38187");
            }
        }

        /// <summary>
        /// Deletes any <see cref="DocumentDataRecord"/> being managed for the specified
        /// <see paramref="dataEntryRow"/>.
        /// </summary>
        /// <param name="dataEntryRow">The <see cref="DataEntryTableRow"/> for which data should be
        /// deleted.</param>
        public virtual void DeleteRow(DataEntryTableRow dataEntryRow)
        {
            try
            {
                if (dataEntryRow == null)
                {
                    return;
                }

                DocumentDataRecord rowData = null;
                if (_rowData.TryGetValue(dataEntryRow, out rowData))
                {
                    rowData.RowDataUpdated -= HandleRowData_DataUpdated;
                    rowData.Dispose();
                    _rowData.Remove(dataEntryRow);
                }

                ClearCachedData(true);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI38154");
            }
        }

        /// <summary>
        /// Resets the row data.
        /// </summary>
        public virtual void ResetRowData()
        {
            try
            {
                var deletedRows = _rowData.ToArray();
                _rowData.Clear();

                foreach (var row in deletedRows)
                {
                    _rowData.Remove(row.Key);
                    row.Value.RowDataUpdated -= HandleRowData_DataUpdated;
                    row.Value.Dispose();
                }

                ClearCachedData(true);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI43540");
            }
        }

        /// <summary>
        /// Gets the descriptions of all records currently in <see cref="_rowData"/> that have
        /// previously been submitted via LabDE.
        /// </summary>
        /// <returns>The descriptions of all records on the active document that have previously
        /// been submitted.</returns>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public virtual IEnumerable<string> GetPreviouslySubmittedRecords()
        {
            foreach (var documentDataRecord in _rowData.Values)
            {
                string id = documentDataRecord.IdField.Value;

                DocumentDataRecordInfo recordInfo = GetRecordInfo(id);
                if (recordInfo != null && recordInfo.CorrespondingFileIds.Any())
                {
                    yield return recordInfo.Description;
                }
            }
        }

        /// <summary>
        /// Links the records currently in <see cref="_rowData"/> with <see paramref="fileId"/>
        /// in the FAM DB. If the link already exists, the link will be updated as appropriate for
        /// the new record.
        /// </summary>
        /// <param name="fileId">The fileId the records should be linked to.</param>
        public virtual void LinkFileWithCurrentRecords(int fileId)
        {
            try
            {
                foreach (var rowData in _rowData)
                {
                    if (string.IsNullOrWhiteSpace(rowData.Value.IdField.Value))
                    {
                        continue;
                    }

                    rowData.Value.LinkFileWithRecord(fileId);
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI38183");
            }
        }

        /// <summary>
        /// Links a file with the specified order.
        /// </summary>
        /// <param name="fileId">The file identifier.</param>
        /// <param name="orderNumber">The order number.</param>
        /// <param name="dateTime">The collection date/time for the order or <c>null</c> if not known.</param>
        public virtual void LinkFileWithOrder(int fileId, string orderNumber, DateTime? dateTime)
        {
            try
            {
                string query = string.Format(CultureInfo.CurrentCulture,
                    "DECLARE @linkExists INT \r\n" +
                    "   SELECT @linkExists = COUNT([OrderNumber]) \r\n" +
                    "       FROM [LabDEOrderFile] WHERE [OrderNumber] = {0} AND [FileID] = {1} \r\n" +
                    "IF @linkExists = 1 \r\n" +
                    "BEGIN \r\n" +
                    ((dateTime != null)
                        ? "    UPDATE [LabDEOrderFile] SET [CollectionDate] = '{2}' \r\n"
                        : "    UPDATE [LabDEOrderFile] SET [CollectionDate] = NULL \r\n") +
                    "        WHERE [OrderNumber] = {0} AND [FileID] = {1} \r\n" +
                    "END \r\n" +
                    "ELSE \r\n" +
                    "BEGIN \r\n" +
                    "    IF {0} IN (SELECT [OrderNumber] FROM [LabDEOrder]) \r\n" +
                    "    BEGIN \r\n" +
                    ((dateTime != null)
                        ? "        INSERT INTO [LabDEOrderFile] ([OrderNumber], [FileID], [CollectionDate]) \r\n" +
                            "            VALUES ({0}, {1}, '{2}') \r\n"
                        : "        INSERT INTO [LabDEOrderFile] ([OrderNumber], [FileID]) \r\n" +
                            "            VALUES ({0}, {1}) \r\n") +
                    "   END \r\n" +
                    "END",
                    "'" + orderNumber + "'", fileId, dateTime);

                // The query has no results-- immediately dispose of the DataTable returned.
                DBMethods.ExecuteDBQuery(SqlDbAppRoleConnection, query).Dispose();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45617");
            }
        }

        /// <summary>
        /// Links a file with the specified encounter.
        /// </summary>
        /// <param name="fileId">The file identifier.</param>
        /// <param name="orderNumber">The encounter identifier.</param>
        /// <param name="dateTime">The encounter date/time or <c>null</c> if not known.</param>
        public virtual void LinkFileWithEncounter(int fileId, string encounterID, DateTime? dateTime)
        {
            try
            {
                string query = string.Format(CultureInfo.CurrentCulture,
                    "DECLARE @linkExists INT \r\n" +
                    "   SELECT @linkExists = COUNT([EncounterID]) \r\n" +
                    "       FROM [LabDEEncounterFile] WHERE [EncounterID] = {0} AND [FileID] = {1} \r\n" +
                    "IF @linkExists = 1 \r\n" +
                    "BEGIN \r\n" +
                    ((dateTime != null)
                        ? "    UPDATE [LabDEEncounterFile] SET [DateTime] = '{2}' \r\n"
                        : "    UPDATE [LabDEEncounterFile] SET [DateTime] = NULL \r\n") +
                    "        WHERE [EncounterID] = {0} AND [FileID] = {1} \r\n" +
                    "END \r\n" +
                    "ELSE \r\n" +
                    "BEGIN \r\n" +
                    "    IF {0} IN (SELECT [CSN] FROM [LabDEEncounter]) \r\n" +
                    "    BEGIN \r\n" +
                    ((dateTime != null)
                        ? "        INSERT INTO [LabDEEncounterFile] ([EncounterID], [FileID], [DateTime]) \r\n" +
                            "            VALUES ({0}, {1}, '{2}') \r\n"
                        : "        INSERT INTO [LabDEEncounterFile] ([EncounterID], [FileID]) \r\n" +
                            "            VALUES ({0}, {1}) \r\n") +
                    "   END \r\n" +
                    "END",
                    "'" + encounterID + "'", fileId, dateTime);

                // The query has no results-- immediately dispose of the DataTable returned.
                DBMethods.ExecuteDBQuery(SqlDbAppRoleConnection, query).Dispose();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45618");
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
                    _recordInfoCache.Clear();
                }

                // _alreadyMappedRecordNumbers is generated from date in the UI, not the DB.
                _alreadyMappedRecordIDs = null;

                foreach (DocumentDataRecord row in _rowData.Values)
                {
                    row.ClearCachedData(clearDatabaseData);
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI38193");
            }
        }

        /// <summary>
        /// Executes the database query.
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public DataTable ExecuteDBQuery(string query)
        {
            return DBMethods.ExecuteDBQuery(SqlDbAppRoleConnection, query);
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
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                AttributeStatusInfo.DataReset -= HandleAttributeStatusInfo_DataReset;

                if (_rowData != null)
                {
                    CollectionMethods.ClearAndDispose(_rowData);
                    _rowData = null;
                }

                _extractRole?.Dispose();
                _extractRole = null;
            }

            // Dispose of unmanaged resources
        }

        #endregion IDisposable Members

        #region Event Handlers

        /// <summary>
        /// Handles the <see cref="DocumentDataRecord.RowDataUpdated"/> event of one of the
        /// <see cref="DocumentDataRecord"/>s from <see cref="_rowData"/>.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleRowData_DataUpdated(object sender, RowDataUpdatedEventArgs e)
        {
            try
            {
                OnRowDataUpdated(e);
            }
            catch (Exception ex)
            {
                // Throw here since we know this event is triggered by our own UI event handler.
                throw ex.AsExtract("ELI38155");
            }
        }

        /// <summary>
        /// Handles the <see cref="AttributeStatusInfo.DataReset"/> event.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.
        /// </param>
        void HandleAttributeStatusInfo_DataReset(object sender, EventArgs e)
        {
            try
            {
                // If AttributeStatusInfo data is being reset, we can no longer use any of the
                // attributes associated with the rows. Dispose of them.
                if (_rowData != null)
                {
                    CollectionMethods.ClearAndDispose(_rowData);
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI38202");
            }
        }

        #endregion Event Handlers

        #region Private Members

        /// <summary>
        /// Gets a lazily instantiated <see cref="SqlConnection"/> instance against the currently
        /// specified <see cref="FileProcessingDB"/>.
        /// </summary>
        /// <value>
        /// A <see cref="SqlConnection"/> against the currently specified
        /// <see cref="FileProcessingDB"/>.
        /// </value>
        /// Using the same casing as MS's own type name for crissakes
        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Db")]
        protected SqlAppRoleConnection SqlDbAppRoleConnection
        {
            get
            {
                if (_extractRole == null)
                {
                    ExtractException.Assert("ELI38156", "Missing database connection.",
                        FileProcessingDB != null);
                    OleDbConnectionStringBuilder oleDbConnectionStringBuilder
                            = new OleDbConnectionStringBuilder(FileProcessingDB.ConnectionString);

                    string server = oleDbConnectionStringBuilder.DataSource;
                    string database = (string)oleDbConnectionStringBuilder["Database"];

                    _extractRole = new ExtractRoleConnection(SqlUtil.CreateConnectionString(server, database));
                }

                return _extractRole;
            }
        }

        /// <summary>
        /// Retrieves the <see cref="DocumentDataRecordInfo"/> for the specified <see paramref="recordID"/>.
        /// </summary>
        /// <param name="recordID">The record ID for which info should be retrieved.</param>
        /// <returns>
        /// The <see cref="DocumentDataRecordInfo"/> for the specified <see paramref="recordID"/>.
        /// </returns>
        protected virtual DocumentDataRecordInfo GetRecordInfo(string recordID)
        {
            DocumentDataRecordInfo recordInfo = null;

            // First attempt to get the record info from the cache.
            if (!string.IsNullOrWhiteSpace(recordID) &&
                !_recordInfoCache.TryGetData(recordID, out recordInfo))
            {
                // If there's not cached info on-hand for this record, load it from the DB now.
                // The resulting DataTable is not needed; immediately dispose of it.
                // Record ID will be a string field; single quotes should be used for SQL string
                // comparisons.
                LoadRecordInfo("'" + recordID.Replace("'", "''") + "'", false).Dispose();

                // The record info should now be cached if the record exists.
                _recordInfoCache.TryGetData(recordID, out recordInfo);
            }

            return recordInfo;
        }

        /// <summary>
        /// Raises the <see cref="RowDataUpdated"/> event.
        /// </summary>
        /// <param name="e">The <see cref="RowDataUpdatedEventArgs"/> representing this event.</param>
        protected virtual void OnRowDataUpdated(RowDataUpdatedEventArgs e)
        {
            try
            {
                RowDataUpdated?.Invoke(this, e);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI43447");
            }
        }

        #endregion Private Members
    }

    /// <summary>
    /// Information regarding a record in the FAM DB.
    /// </summary>
    public class DocumentDataRecordInfo
    {
        /// <summary>
        /// A description of the record.
        /// </summary>
        public string Description
        {
            get;
            set;
        }

        /// <summary>
        /// The FAM file IDs for files that have already been filed against this record.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public List<int> CorrespondingFileIds
        {
            get;
            set;
        }
    }

    /// <summary>
    /// An <see cref="EventArgs"/> that represents data for the <see cref="FAMData.RowDataUpdated"/>
    /// event.
    /// </summary>
    public class RowDataUpdatedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RowDataUpdatedEventArgs"/> class.
        /// </summary>
        /// <param name="rowData">The <see cref="DocumentDataRecord"/> for which data was updated.</param>
        /// <param name="recordIdUpdated"><see langword="true"/> if the record ID has been
        /// changed; otherwise, <see langword="false"/>.</param>
        public RowDataUpdatedEventArgs(DocumentDataRecord rowData, bool recordIdUpdated)
            : base()
        {
            DocumentDataRecord = rowData;
            RecordIdUpdated = recordIdUpdated;
        }

        /// <summary>
        /// The <see cref="DocumentDataRecord"/> for which data was updated.
        /// </summary>
        public DocumentDataRecord DocumentDataRecord
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the record ID has been changed.
        /// </summary>
        /// <value><see langword="true"/> if the record ID has been changed; otherwise, 
        /// <see langword="false"/>.</value>
        public bool RecordIdUpdated
        {
            get;
            private set;
        }
    }
}
