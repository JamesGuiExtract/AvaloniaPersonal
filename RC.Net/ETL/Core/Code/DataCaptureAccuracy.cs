using Extract.AttributeFinder;
using Extract.Code.Attributes;
using Extract.DataCaptureStats;
using Extract.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Transactions;
using System.Windows.Forms;
using UCLID_AFCORELib;
using UCLID_COMUTILSLib;

namespace Extract.ETL
{
    /// <summary>
    /// Class to implement the Database service for Data Capture accuracy stats
    /// </summary>
    [DataContract]
    [KnownType(typeof(ScheduledEvent))]
    [ExtractCategory("DatabaseService", "Data capture accuracy")]
    [SuppressMessage("Microsoft.Naming", "CA1709: CorrectCasingInTypeName")]
    public class DataCaptureAccuracy : DatabaseService, IConfigSettings, IHasConfigurableDatabaseServiceStatus
    {
        #region Constants

        /// <summary>
        /// Current version
        /// </summary>
        const int CURRENT_VERSION = 1;

        /// <summary>
        /// The number of FileTaskSession rows to process in a single transaction.
        /// </summary>
        const int PROCESS_BATCH_SIZE = 100;

        /// <summary>
        /// Query to get the affected files for the batch
        /// Parameters:
        ///     @FoundSetName - Name of the Attribute set for found values
        ///     @ExpectedSetName - Name of the Attribute set for Expected values
        ///     @StartFileTaskSessionSetID - The first FileTaskSession row used to define the affected files.
        ///     @EndFileTaskSessionSetID - The last FileTaskSession row used to define the affected files.
        /// </summary>
        static readonly string AFFECTED_FILES =
            @"
                DECLARE @affectedFiles TABLE (FileID INT)
				INSERT INTO @affectedFiles
				SELECT DISTINCT FileID
					FROM AttributeSetForFile
					INNER JOIN AttributeSetName ON AttributeSetForFile.AttributeSetNameID = AttributeSetName.ID
					INNER JOIN FileTaskSession ON AttributeSetForFile.FileTaskSessionID = FileTaskSession.ID
					WHERE AttributeSetName.Description IN (@FoundSetName, @ExpectedSetName)
                    AND FileTaskSession.DateTimeStamp IS NOT NULL
					AND FileTaskSession.ID >= @StartFileTaskSessionSetID AND FileTaskSession.ID <= @EndFileTaskSessionSetID
            ";

        /// <summary>
        /// Query used to get the data used to create the records in ReportingDataCaptureAccuracy table
        /// This query will collect a list of all files affected by FileTaskSession rows in the range
        /// @StartFileTaskSessionSetID to @EndFileTaskSessionSetID, delete all related rows from 
        /// ReportingDataCaptureAccuracy, then provide the necessary data for all comparisons to be
        /// executed on those affected files given the data that currently exists in the DB (may include
        /// pulling data from a FileTaskSession row not in the provided range of FileTaskSession rows.
        /// Parameters:
        ///     @FoundSetName - Name of the Attribute set for found values
        ///     @ExpectedSetName - Name of the Attribute set for Expected values
        ///     @StartFileTaskSessionSetID - The first FileTaskSession row used to define the affected files.
        ///     @EndFileTaskSessionSetID - The last FileTaskSession row used to define the affected files.
        /// </summary>
        static readonly string UPDATE_ACCURACY_DATA_SQL = AFFECTED_FILES +
            @"
                
				;WITH ExpectedFTS AS (
				SELECT DISTINCT affectedFiles.FileID, MAX(AttributeSetForFile.FileTaskSessionID) AS ID
					FROM @affectedFiles affectedFiles
					INNER JOIN FileTaskSession ON affectedFiles.FileID = FileTaskSession.FileID
                        AND FileTaskSession.ID <= @EndFileTaskSessionSetID AND FileTaskSession.ActionID IS NOT NULL
					INNER JOIN AttributeSetForFile ON FileTaskSession.ID = AttributeSetForFile.FileTaskSessionID
					INNER JOIN AttributeSetName ON AttributeSetNameID = AttributeSetName.ID
					WHERE AttributeSetName.Description = @ExpectedSetName
					GROUP BY affectedFiles.FileID
				),
				FoundAndExpectedFTS AS (
				SELECT DISTINCT
					RANK() OVER (PARTITION BY ExpectedFTS.FileID ORDER BY FileTaskSession.ID DESC, Pagination.ID DESC) AS Rank
					,ExpectedFTS.ID AS ExpectedFTSID
					,FileTaskSession.ID AS FoundFTSID
					,Pagination.ID AS PaginationID
				FROM ExpectedFTS
				LEFT JOIN Pagination ON (ExpectedFTS.FileID = Pagination.DestFileID 
					AND Pagination.SourceFileID <> Pagination.DestFileID 
					AND Pagination.DestPage IS NOT NULL)
				LEFT JOIN FileTaskSession ON
					(FileTaskSession.FileID = ExpectedFTS.FileID OR FileTaskSession.FileID = Pagination.OriginalFileID)
                     AND FileTaskSession.ActionID IS NOT NULL
				INNER JOIN AttributeSetForFile ON FileTaskSession.ID = AttributeSetForFile.FileTaskSessionID
				INNER JOIN AttributeSetName ON AttributeSetNameID = AttributeSetName.ID
				LEFT JOIN FAMFile ON FileTaskSession.FileID = FAMFile.ID
				WHERE AttributeSetName.Description = @FoundSetName
				)

				SELECT FoundAttributeSet.ID AS FoundAttributeSetFileID
						,FoundAttributeSet.VOA FoundVOA
						,ExpectedAttributeSet.ID AS ExpectedAttributeSetFileID
						,ExpectedAttributeSet.VOA AS ExpectedVOA
						,expectedFTS.FileID as ExpectedFileID
						,foundFTS.FileID AS OriginalFileID
						,COALESCE(Pagination.OriginalPage, -1) AS FirstPageFromOriginal
						,COALESCE(FAMFile.Pages, 1) AS FoundPageCount
						,foundFTS.DateTimeStamp FoundDateTimeStamp
						,foundFTS.ActionID FoundActionID
						,foundFS.FAMUserID FoundFAMUserID
						,expectedFTS.DateTimeStamp ExpectedDateTimeStamp
						,expectedFTS.ActionID ExpectedActionID
						,expectedFS.FAMUserID ExpectedFAMUserID
				FROM FoundAndExpectedFTS
					INNER JOIN FileTaskSession foundFTS ON FoundFTSID = foundFTS.ID
					INNER JOIN FAMSession foundFS 
						ON foundFTS.FAMSessionID = foundFS.ID
					INNER JOIN FileTaskSession expectedFTS ON ExpectedFTSID = expectedFTS.ID
					INNER JOIN FAMSession expectedFS 
						ON expectedFTS.FAMSessionID = expectedFS.ID
					INNER JOIN AttributeSetForFile FoundAttributeSet ON FoundAttributeSet.FileTaskSessionID = FoundFTSID
					INNER JOIN AttributeSetForFile ExpectedAttributeSet ON ExpectedAttributeSet.FileTaskSessionID = ExpectedFTSID
					LEFT JOIN Pagination ON Pagination.ID = FoundAndExpectedFTS.PaginationID
					INNER JOIN FAMFile ON foundFTS.FileID = FAMFile.ID
					WHERE RANK = 1
            ";

        /// <summary>
        /// Query to delete old data
        /// Parameters:
        ///     @FoundSetName - Name of the Attribute set for found values
        ///     @ExpectedSetName - Name of the Attribute set for Expected values
        ///     @StartFileTaskSessionSetID - The first FileTaskSession row used to define the affected files.
        ///     @EndFileTaskSessionSetID - The last FileTaskSession row used to define the affected files.
        /// </summary>
        static readonly string DELETE_OLD_DATA = AFFECTED_FILES +
            @"
                DELETE FROM ReportingDataCaptureAccuracy
                    WHERE DatabaseServiceID = @DatabaseServiceID
                        AND FileID IN(SELECT FileID FROM @affectedFiles)
            ";

        #endregion

        #region Fields

        /// <summary>
        /// Indicates whether the Process method is currently executing.
        /// </summary>
        bool _processing;

        /// <summary>
        /// The ID of the last file task session row processed by this service.
        /// </summary>
        int lastFileTaskSessionIDProcessed = -1;

        /// <summary>
        /// The current status info for this service.
        /// </summary>
        DataCaptureAccuracyStatus _status;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Default constructor for DataCaptureAccuracy
        /// </summary>
        public DataCaptureAccuracy()
        {
        }

        #endregion Constructors

        #region DatabaseService Properties

        /// <summary>
        /// Gets a value indicating whether this instance is processing.
        /// </summary>
        /// <value>
        ///   <c>true</c> if processing; otherwise, <c>false</c>.
        /// </value>
        public override bool Processing
        {
            get
            {
                return _processing;
            }
        }

        #endregion DatabaseService Properties

        #region DatabaseService Methods

        /// <summary>
        /// Performs the processing needed for the records in ReportingDataCaptureAccuracy table
        /// </summary>
        /// <param name="cancelToken">Cancel token to indicate that processing should stop</param>
        public override void Process(CancellationToken cancelToken)
        {
            try
            {
                _processing = true;

                RefreshStatus();

                int maxReportableFileTaskSession = MaxReportableFileTaskSessionId();

                while (LastFileTaskSessionIDProcessed < maxReportableFileTaskSession)
                {
                    cancelToken.ThrowIfCancellationRequested();
                    var endFileTaskSessionID =
                        Math.Min(LastFileTaskSessionIDProcessed + PROCESS_BATCH_SIZE, maxReportableFileTaskSession);

                    ProcessBatch(cancelToken, endFileTaskSessionID);
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45366");
            }
            finally
            {
                _processing = false;
            }
        }

        /// <summary>
        /// Processes a batch of FileTaskSession table rows (starting with
        /// <see cref="LastFileTaskSessionIDProcessed"/> + 1 and ending with
        /// <see paramref="endFileTaskSessionID"/>.
        /// </summary>
        /// <param name="cancelToken">Cancel token to indicate that processing should stop</param>
        /// <param name="endFileTaskSessionID">The ID of the last file task session row to process.</param>
        void ProcessBatch(CancellationToken cancelToken, int endFileTaskSessionID)
        {
            var queriesToRunInBatch = new ConcurrentQueue<string>();

            using (var connection = NewSqlDBConnection())
            {
                // Open the connection
                connection.Open();

                SqlCommand cmd = connection.CreateCommand();
                // Set the timeout so that it waits indefinitely
                cmd.CommandTimeout = 0;

                // This command gets the data to work with in this batch
                cmd.CommandText = UPDATE_ACCURACY_DATA_SQL;

                addParametersToCommand(cmd, endFileTaskSessionID);
                
				// Keep track of active threads
                CountdownEvent threadCountdown = new CountdownEvent(1);

                Semaphore threadSemaphore = new Semaphore(NumberOfProcessingThreads, NumberOfProcessingThreads);

                using (SqlDataReader ExpectedAndFoundReader = cmd.ExecuteReader())
                {
                    // Get VOA and other relevant data for each file needed to calculate capture statistics.
                    while (ReadDataAccuracyQueryData(ExpectedAndFoundReader, cancelToken, out var queryResultRow))
                    {
                        // Increment the number of pending threads
                        threadCountdown.AddCount();

                        // Get Semaphore before creating the thread
                        threadSemaphore.WaitOne();

                        // Create a thread pool thread to do the comparison
                        ThreadPool.QueueUserWorkItem(delegate
                        {
                            try
                            {
                                // Put the expected and found streams in usings so they will be disposed
                                using (queryResultRow.ExpectedStream)
                                using (queryResultRow.FoundStream)
                                {
                                    // Get the VOAs from the streams
                                    IUnknownVector expectedAttributes = AttributeMethods.GetVectorOfAttributesFromSqlBinary(queryResultRow.ExpectedStream);
                                    IUnknownVector foundAttributes = AttributeMethods.GetVectorOfAttributesFromSqlBinary(queryResultRow.FoundStream);

                                    // If the original file ID differs from the expected file ID, search the
                                    // original file's attribute hierarchy to find the comparison attributes
                                    // in a proposed document pagination hierarchy.
                                    // We know to be searching the pagination source document data because
                                    // if manual pagination had triggered rule execution on the expected file
                                    // (or even rules had been run after-the-fact on the expected file), the
                                    // more recent storage of the found attribute set would trigger that to
                                    // be used as the comparison.
                                    if (queryResultRow.ExpectedFileID != queryResultRow.OriginalFileID)
                                    {
                                        foundAttributes = GetFoundAttributesFromPaginationSource(foundAttributes, queryResultRow);
                                    }

                                    // Compare the VOAs
                                    var output = AttributeTreeComparer.CompareAttributes(expectedAttributes,
                                    foundAttributes, XPathOfAttributesToIgnore, XPathOfContainerOnlyAttributes, cancelToken)
                                    .ToList();

                                    // Add the comparison results to the Results
                                    var statsToStore = output.AggregateStatistics(cancelToken).ToList();

                                    queriesToRunInBatch.Enqueue(AddAccuracyDataQueryToList(statsToStore, queryResultRow));
                                }
                            }
                            catch (Exception ex)
                            {
                                ex.AsExtract("ELI41544").Log();
                            }
                            finally
                            {
                                // Decrement the number of pending threads
                                threadCountdown.Signal();

                                // Release semaphore after thread has been created
                                threadSemaphore.Release();
                            }
                        });
                    }
                }
                threadCountdown.Signal();
                WaitHandle.WaitAny(new WaitHandle[] { threadCountdown.WaitHandle, cancelToken.WaitHandle });
            }

            AddTheDataToTheDatabase(queriesToRunInBatch, endFileTaskSessionID, cancelToken);
        }


        /// <summary>
        /// Deletes the old records and adds the new data by executing the queries in queriesToRunInBatch
        /// </summary>
        /// <param name="endFileTaskSessionID">The last fileTaskSessionID in the batch</param>
        /// <param name="cancelToken">Cancel token</param>
        void AddTheDataToTheDatabase(ConcurrentQueue<string> queriesToRunInBatch, int endFileTaskSessionID,
            CancellationToken cancelToken)
        {
            using (TransactionScope scope = new TransactionScope(
               TransactionScopeOption.Required,
               new TransactionOptions()
               {
                   IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead,
                   Timeout = TransactionManager.MaximumTimeout,
               },
               TransactionScopeAsyncFlowOption.Enabled))
            {
                using (var connection = NewSqlDBConnection())
                {
                    connection.Open();

                    // delete the old records
                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandTimeout = 0;
                        cmd.CommandText = DELETE_OLD_DATA;
                        addParametersToCommand(cmd, endFileTaskSessionID);
                        var deleteTask = cmd.ExecuteNonQueryAsync();
                        deleteTask.Wait(cancelToken);
                    }

                    // Run the queries to add the accuracy records
                    foreach (var q in queriesToRunInBatch)
                    {
                        try
                        {
                            using (var cmd = connection.CreateCommand())
                            {
                                cmd.CommandTimeout = 0;
                                cmd.CommandText = q;
                                var addTask = cmd.ExecuteNonQueryAsync();
                                addTask.Wait(cancelToken);
                            }
                        }
                        catch (Exception ex)
                        {
                            var ee = ex.AsExtract("ELI46166");
                            ee.AddDebugData("Query", q, false);
                            throw ee;
                        }
                    }

                    // If Canceled, there will have been an exception everything but saving the status is done 
                    LastFileTaskSessionIDProcessed = endFileTaskSessionID;

                    Status.SaveStatus(connection, DatabaseServiceID);
                }
                scope.Complete();
            }
        }

        /// <summary>
        /// Reads the next row from the data accuracy query's results.
        /// </summary>
        /// <param name="dataAccuracyQueryReader">THe <see cref="SqlDataReader"/> for the query's results.</param>
        /// <param name="cancelToken">Cancel token to indicate that processing should stop</param>
        /// <param name="queryResultRow">The <see cref="UpdateQueryResultRow"/> instance to store the data.</param>
        /// <returns><c>true</c> if the next row was read successfully; <c>false</c> if the process was cancelled
        /// or there were no more rows in the result.</returns>
        static bool ReadDataAccuracyQueryData(SqlDataReader dataAccuracyQueryReader, CancellationToken cancelToken,
            out UpdateQueryResultRow queryResultRow)
        {
            // Get the ordinal for the FoundVOA and ExpectedVOA columns
            int foundVOAColumn = dataAccuracyQueryReader.GetOrdinal("FoundVOA");
            int expectedVOAColumn = dataAccuracyQueryReader.GetOrdinal("ExpectedVOA");
            int foundAttributeForFileSetColumn = dataAccuracyQueryReader.GetOrdinal("FoundAttributeSetFileID");
            int expectedAttributeForFileSetColumn = dataAccuracyQueryReader.GetOrdinal("ExpectedAttributeSetFileID");
            int expectedfileIDColumn = dataAccuracyQueryReader.GetOrdinal("ExpectedFileID");
            int originalFileIDColumn = dataAccuracyQueryReader.GetOrdinal("OriginalFileID");
            int firstPageFromOriginalColumn = dataAccuracyQueryReader.GetOrdinal("FirstPageFromOriginal");
            int foundPageCountColumn = dataAccuracyQueryReader.GetOrdinal("FoundPageCount");
            int foundDateTimeStampColumn = dataAccuracyQueryReader.GetOrdinal("FoundDateTimeStamp");
            int foundActionIDColumn = dataAccuracyQueryReader.GetOrdinal("FoundActionID");
            int foundFAMUserIDColumn = dataAccuracyQueryReader.GetOrdinal("FoundFAMUserID");
            int expectedDateTimeStampColumn = dataAccuracyQueryReader.GetOrdinal("ExpectedDateTimeStamp");
            int expectedActionIDColumn = dataAccuracyQueryReader.GetOrdinal("ExpectedActionID");
            int expectedFAMUserIDColumn = dataAccuracyQueryReader.GetOrdinal("ExpectedFAMUserID");

            // Process the found records
            if (dataAccuracyQueryReader.Read())
            {
                cancelToken.ThrowIfCancellationRequested();
                queryResultRow = new UpdateQueryResultRow()
                {
                    // Get the streams for the expected and found voa data (the thread will read the voa from the stream
                    ExpectedStream = dataAccuracyQueryReader.GetStream(expectedVOAColumn),
                    FoundStream = dataAccuracyQueryReader.GetStream(foundVOAColumn),
                    FoundID = dataAccuracyQueryReader.GetInt64(foundAttributeForFileSetColumn),
                    ExpectedID = dataAccuracyQueryReader.GetInt64(expectedAttributeForFileSetColumn),
                    ExpectedFileID = dataAccuracyQueryReader.GetInt32(expectedfileIDColumn),
                    OriginalFileID = dataAccuracyQueryReader.GetInt32(originalFileIDColumn),
                    FirstPageFromOriginal = dataAccuracyQueryReader.GetInt32(firstPageFromOriginalColumn),
                    FoundPageCount = dataAccuracyQueryReader.GetInt32(foundPageCountColumn),
                    FoundDateTime = dataAccuracyQueryReader.GetDateTime(foundDateTimeStampColumn),
                    FoundActionID = dataAccuracyQueryReader.GetInt32(foundActionIDColumn),
                    FoundFAMUserID = dataAccuracyQueryReader.GetInt32(foundFAMUserIDColumn),
                    ExpectedDateTime = dataAccuracyQueryReader.GetDateTime(expectedDateTimeStampColumn),
                    ExpectedActionID = dataAccuracyQueryReader.GetInt32(expectedActionIDColumn),
                    ExpectedFAMUserID = dataAccuracyQueryReader.GetInt32(expectedFAMUserIDColumn)
                };

                return true;
            }
            else
            {
                queryResultRow = new UpdateQueryResultRow();

                return false;
            }
        }

        /// <summary>
        /// Retrieves the "Found" attribute set from a pagination source file's attribute hierarchy
        /// to find the comparison attributes that correspond to this output document.
        /// </summary>
        /// <param name="foundAttributes">The "found" attribute hierarchy which is expected to have
        /// data for multiple documents under parallel root-level "Document" attributes.</param>
        /// <param name="queryResultRow">The <see cref="UpdateQueryResultRow"/> providing the data
        /// for this file's comparison.</param>
        /// <returns></returns>
        static IUnknownVector GetFoundAttributesFromPaginationSource(
            IUnknownVector foundAttributes, UpdateQueryResultRow queryResultRow)
        {
            var foundContext = new XPathContext(foundAttributes);
            var documentIterator = foundContext.GetIterator("/*/Document");
            while (documentIterator.MoveNext())
            {
                var pagesAttribute = (foundContext.Evaluate(documentIterator, "Pages") as List<object>)
                    ?.OfType<IAttribute>()
                    .SingleOrDefault();
                var pagesString = pagesAttribute?.Value.String;

                // If we find a Document node containing the first page, use this as a comparison.
                // We can know the document wasn't manually paginated, lest the output document would
                // have a more found data set.
                if (UtilityMethods.GetPageNumbersFromString(pagesString, queryResultRow.FoundPageCount, true)
                        .Contains(queryResultRow.FirstPageFromOriginal))
                {
                    var foundAttributesFromSource = (foundContext.Evaluate(documentIterator, "DocumentData") as List<object>)
                        ?.OfType<IAttribute>()
                        .SingleOrDefault()
                        ?.SubAttributes;
                    if (foundAttributesFromSource != null)
                    {
                        return foundAttributesFromSource;
                    }
                }
            }

            return foundAttributes;
        }

        /// <summary>
        /// Creates and adds the accuracy data query to add the data in <see paramref="statsToStore"/> to the
        /// ReportingDataCaptureAccuracy table.
        /// </summary>
        /// <param name="statsToStore">The <see cref="AccuracyDetail"/> instances to store.</param>
        /// <param name="queryResultRow">The <see cref="UpdateQueryResultRow"/> used to generate the stats.
        /// </param>
        string AddAccuracyDataQueryToList(List<AccuracyDetail> statsToStore,
            UpdateQueryResultRow queryResultRow)
        {
            statsToStore = statsToStore.Where(c => c.Label != AccuracyDetailLabel.ContainerOnly).ToList();

            // This is needed so a row gets put in for every file that has expected and found voa's saved
            if (!statsToStore.Any())
            {
                List<AccuracyDetail> list = new List<AccuracyDetail>();
                list.Add(new AccuracyDetail(AccuracyDetailLabel.Correct, string.Empty, 0));
                list.Add(new AccuracyDetail(AccuracyDetailLabel.Incorrect, string.Empty, 0));
                list.Add(new AccuracyDetail(AccuracyDetailLabel.Expected, string.Empty, 0));

                statsToStore = list;
            }
            var lookup = statsToStore.ToLookup(a => new { a.Path, a.Label });

            var attributePaths = statsToStore
                .Select(a => a.Path)
                .Distinct()
                .OrderBy(p => p)
                .ToList();

            List<string> valuesToAdd = new List<string>();
            foreach (var path in attributePaths)
            {
                int correct = lookup[new { Path = path, Label = AccuracyDetailLabel.Correct }].Sum(a => a.Value);
                int incorrect = lookup[new { Path = path, Label = AccuracyDetailLabel.Incorrect }].Sum(a => a.Value);
                int expected = lookup[new { Path = path, Label = AccuracyDetailLabel.Expected }].Sum(a => a.Value);

                valuesToAdd.Add(string.Format(CultureInfo.InvariantCulture,
                    @"({0}, {1}, {2}, {3}, '{4}', {5}, {6}, {7}, '{8:s}', {9}, {10}, '{11:s}', {12}, {13})"
                    , DatabaseServiceID
                    , queryResultRow.FoundID
                    , queryResultRow.ExpectedID
                    , queryResultRow.ExpectedFileID
                    , path
                    , correct
                    , expected
                    , incorrect
                    , queryResultRow.FoundDateTime
                    , queryResultRow.FoundActionID
                    , queryResultRow.FoundFAMUserID
                    , queryResultRow.ExpectedDateTime
                    , queryResultRow.ExpectedActionID
                    , queryResultRow.ExpectedFAMUserID
                    ));
            }

            var queryToAdd =
                string.Format(CultureInfo.InvariantCulture,
                    @"INSERT INTO [dbo].[ReportingDataCaptureAccuracy]
                        ([DatabaseServiceID]
                        ,[FoundAttributeSetForFileID]
                        ,[ExpectedAttributeSetForFileID]
                        ,[FileID]
                        ,[Attribute]
                        ,[Correct]
                        ,[Expected]
                        ,[Incorrect]
                        ,[FoundDateTimeStamp]
                        ,[FoundActionID]
                        ,[FoundFAMUserID]
                        ,[ExpectedDateTimeStamp]
                        ,[ExpectedActionID]
                        ,[ExpectedFAMUserID]
                        )
                    VALUES
                        {0};", string.Join(",\r\n", valuesToAdd));

            return queryToAdd;
        }

        #endregion DatabaseService Methods

        #region IConfigSettings implementation

        /// <summary>
        /// Method returns the state of the configuration
        /// </summary>
        /// <returns>Returns <see langword="true"/> if configuration is valid, otherwise false</returns>
        public bool IsConfigured()
        {
            try
            {
                bool configured = true;
                configured = configured && !string.IsNullOrWhiteSpace(XPathOfAttributesToIgnore);
                configured = configured && !string.IsNullOrWhiteSpace(XPathOfContainerOnlyAttributes);
                configured = configured && !string.IsNullOrWhiteSpace(ExpectedAttributeSetName);
                configured = configured && !string.IsNullOrWhiteSpace(FoundAttributeSetName);
                configured = configured && (FoundAttributeSetName != ExpectedAttributeSetName);
                return configured;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45656");
            }
        }

        /// <summary>
        /// Displays a form to Configures the DataCaptureAccuracy service
        /// </summary>
        /// <returns><see langword="true"/> if configuration was ok'd. if configuration was canceled returns 
        /// <see langword="false"/></returns>
        public bool Configure()
        {
            try
            {
                DataCaptureAccuracyForm captureAccuracyForm = new DataCaptureAccuracyForm(this);
                return captureAccuracyForm.ShowDialog() == DialogResult.OK;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45657");
            }
            return false;
        }

        #endregion IConfigSettings implementation

        #region DataCaptureAccuracy Properties

        /// <summary>
        /// XPath query of attributes the be ignored when comparing attributes
        /// </summary>
        [DataMember]
        public string XPathOfAttributesToIgnore { get; set; } = string.Empty;

        /// <summary>
        /// XPath of Attributes that are only considered containers when comparing attributes
        /// </summary>
        [DataMember]
        public string XPathOfContainerOnlyAttributes { get; set; } = string.Empty;

        /// <summary>
        /// The set name of the expected attributes when doing the comparison
        /// </summary>
        [DataMember]
        public string ExpectedAttributeSetName { get; set; } = "DataSavedByOperator";

        /// <summary>
        /// The set name of the found attributes when doing the comparison
        /// </summary>
        [DataMember]
        public string FoundAttributeSetName { get; set; } = "DataFoundByRules";

        /// <summary>
        /// The average F1Score from the last time the testing command was successfully executed
        /// </summary>
        public int LastFileTaskSessionIDProcessed
        {
            get
            {
                if (_status != null)
                {
                    return _status.LastFileTaskSessionIDProcessed;
                }
                else
                {
                    return lastFileTaskSessionIDProcessed;
                }
            }
            set
            {
                if (_status != null)
                {
                    _status.LastFileTaskSessionIDProcessed = value;
                }
                else
                {
                    lastFileTaskSessionIDProcessed = value;
                }
            }
        }

        [DataMember]
        public override int Version { get; protected set; } = CURRENT_VERSION;

        #endregion DataCaptureAccuracy Properties

        #region IHasConfigurableDatabaseServiceStatus

        /// <summary>
        /// The <see cref="DatabaseServiceStatus"/> for this instance
        /// </summary>
        public DatabaseServiceStatus Status
        {
            get => _status = _status ?? GetLastOrCreateStatus(() => new DataCaptureAccuracyStatus()
            {
                LastFileTaskSessionIDProcessed = -1
            });

            set => _status = value as DataCaptureAccuracyStatus;
        }

        /// <summary>
        /// Refreshes the <see cref="DatabaseServiceStatus"/> by loading from the database, creating a new instance,
        /// or setting it to null (if <see cref="DatabaseServiceID"/>, <see cref="DatabaseServer"/> and
        /// <see cref="DatabaseName"/> are not configured)
        /// </summary>
        public void RefreshStatus()
        {
            try
            {
                if (DatabaseServiceID > 0
                    && !string.IsNullOrEmpty(DatabaseServer)
                    && !string.IsNullOrEmpty(DatabaseName))
                {
                    _status = GetLastOrCreateStatus(() => new DataCaptureAccuracyStatus());
                }
                else
                {
                    _status = null;
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI46065");
            }
        }

        #endregion IHasConfigurableDatabaseServiceStatus

        #region Private Methods

        /// <summary>
        /// Called after this instance is deserialized.
        /// </summary>
        [OnDeserialized]
        void OnDeserialized(StreamingContext context)
        {
            if (Version > CURRENT_VERSION)
            {
                ExtractException ee = new ExtractException("ELI45382", "Settings were saved with a newer version.");
                ee.AddDebugData("SavedVersion", Version, false);
                ee.AddDebugData("CurrentVersion", CURRENT_VERSION, false);
                throw ee;
            }

            Version = CURRENT_VERSION;
        }

        /// <summary>
        /// Adds the following parameters using the configured properties
        ///     @FoundSetName - Set to FoundAttributeSetName
        ///     @ExpectedSetName - Set to ExpectedAttributeSetName
        ///     @DatabaseServiceID - Set to ID
        /// </summary>
        /// <param name="cmd">The SqlCommand that needs the parameters added</param>
        void addParametersToCommand(SqlCommand cmd, int endFileTaskSessionId)
        {
            cmd.Parameters.Add("@DatabaseServiceID", SqlDbType.Int);
            cmd.Parameters.Add("@FoundSetName", SqlDbType.NVarChar);
            cmd.Parameters.Add("@ExpectedSetName", SqlDbType.NVarChar);
            cmd.Parameters.Add("@StartFileTaskSessionSetID", SqlDbType.Int);
            cmd.Parameters.Add("@EndFileTaskSessionSetID", SqlDbType.Int);
            cmd.Parameters["@DatabaseServiceID"].Value = DatabaseServiceID;
            cmd.Parameters["@FoundSetName"].Value = FoundAttributeSetName;
            cmd.Parameters["@ExpectedSetName"].Value = ExpectedAttributeSetName;
            cmd.Parameters["@StartFileTaskSessionSetID"].Value = LastFileTaskSessionIDProcessed + 1;
            cmd.Parameters["@EndFileTaskSessionSetID"].Value = endFileTaskSessionId;
        }

        #endregion Private Methods

        #region Private Classes

        /// <summary>
        /// Class for the DataCaptureAccuracyStatus stored in the DatabaseService record
        /// </summary>
        [DataContract]
        public class DataCaptureAccuracyStatus : DatabaseServiceStatus, IFileTaskSessionServiceStatus
        {
            const int _CURRENT_VERSION = 1;

            [DataMember]
            public override int Version { get; protected set; } = _CURRENT_VERSION;

            /// <summary>
            /// The ID of the last MLData record processed
            /// </summary>
            public int LastFileTaskSessionIDProcessed { get; set; }

            /// <summary>
            /// Called after this instance is deserialized.
            /// </summary>
            [OnDeserialized]
            void OnDeserialized(StreamingContext context)
            {
                if (Version > _CURRENT_VERSION)
                {
                    ExtractException ee = new ExtractException("ELI46064", "Settings were saved with a newer version.");
                    ee.AddDebugData("SavedVersion", Version, false);
                    ee.AddDebugData("CurrentVersion", _CURRENT_VERSION, false);
                    throw ee;
                }

                Version = _CURRENT_VERSION;
            }
        }

        /// <summary>
        /// Stores the data from a row of output from UPDATE_ACCURACY_DATA_SQL
        /// </summary>
        struct UpdateQueryResultRow
        {
            public Stream ExpectedStream;
            public Stream FoundStream;
            public Int64 FoundID;
            public Int64 ExpectedID;
            public Int32 ExpectedFileID;
            public Int32 OriginalFileID;
            public Int32 FirstPageFromOriginal;
            public Int32 FoundPageCount;
            public DateTime FoundDateTime;
            public Int32 FoundActionID;
            public Int32 FoundFAMUserID;
            public DateTime ExpectedDateTime;
            public Int32 ExpectedActionID;
            public Int32 ExpectedFAMUserID;
        }

        #endregion Private Classes
    }
}
