using Extract.AttributeFinder;
using Extract.Code.Attributes;
using Extract.DataCaptureStats;
using Extract.SqlDatabase;
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
using UCLID_COMUTILSLib;
using static System.FormattableString;

namespace Extract.ETL
{
    /// <summary>
    /// Class to implement the Database service for Redaction accuracy stats
    /// </summary>
    [DataContract]
    [KnownType(typeof(ScheduledEvent))]
    [SuppressMessage("Microsoft.Naming", "CA1709: CorrectCasingInTypeName")]
    [ExtractCategory("DatabaseService", "Redaction accuracy")]
    public class RedactionAccuracy : DatabaseService, IConfigSettings, IHasConfigurableDatabaseServiceStatus
    {
        #region Internal classes

        /// <summary>
        /// Class for the DocumentVerificationTimes status stored in the DatabaseService record
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
        [DataContract]
        public class RedactionAccuracyStatus : DatabaseServiceStatus, IFileTaskSessionServiceStatus
        {
            #region RedactionAccuracyStatus constants

            const int _CURRENT_VERSION = 1;

            #endregion

            #region RedactionAccuracyStatus Properties

            [DataMember]
            public override int Version { get; protected set; } = _CURRENT_VERSION;

            /// <summary>
            /// Maintains the last FileTaskSessionID that was processed successfully
            /// </summary>
            public int LastFileTaskSessionIDProcessed { get; set; } = 0;


            #endregion

            #region RedactionAccuracyStatus Serialization

            /// <summary>
            /// Called after this instance is deserialized.
            /// </summary>
            [OnDeserialized]
            void OnDeserialized(StreamingContext context)
            {
                if (Version > _CURRENT_VERSION)
                {
                    ExtractException ee = new ExtractException("ELI46063", "Settings were saved with a newer version.");
                    ee.AddDebugData("SavedVersion", Version, false);
                    ee.AddDebugData("CurrentVersion", _CURRENT_VERSION, false);
                    throw ee;
                }

                Version = _CURRENT_VERSION;
            }

            #endregion
        }

        #endregion

        #region Constants

        /// <summary>
        /// Current version
        /// </summary>
        const int CURRENT_VERSION = 1;

        /// <summary>
        /// Number of files to process at a time
        /// </summary>
        const int _PROCESS_BATCH_SIZE = 100;

        /// <summary>
        /// Query that is common to both UPDATE_ACCURACY_DATA_SQL and DELETE_OLD_DATA queries
        /// This query requires 
        ///     @LastInBatchID - last file task session id in the batch
        ///     @LastProcessedID - Last processed file task session
        /// </summary>
        static readonly string GET_TOUCHED_FILES =
            Invariant($@"
                DECLARE @FilesTable TABLE (
                	FileID INT
                )
                
                ;WITH TouchedFiles AS (
                	SELECT FileTaskSession.ID FileTaskSessionID, FileID, FileTaskSession.DateTimeStamp
                	FROM FileTaskSession WITH (NOLOCK) INNER JOIN TaskClass ON FileTaskSession.TaskClassID = TaskClass.ID
                	WHERE FileTaskSession.ID > @LastProcessedID AND FileTaskSession.ID <= @LastInBatchID
                        AND TaskClass.GUID IN (
                            '{Constants.TaskClassStoreRetrieveAttributes}',
                            '{Constants.TaskClassWebVerification}',
                            '{Constants.TaskClassDocumentApi}')
                )
                
                INSERT INTO @FilesTable
                    SELECT FileID FROM TouchedFiles 
            ");

        /// <summary>
        /// Query used to get the data used to create the records in ReportingRedactionAccuracy table
        /// it uses the GET_TOUCHED_FILES query
        /// This query requires values for the following parameters
        ///     @LastInBatchID - last file task session id in the batch
        ///     @LastProcessedID - Last processed file task session
        ///     @FoundSetName - Name of the Attribute set for found values
        ///     @ExpectedSetName - Name of the Attribute set for Expected values
        /// </summary>
        static readonly string UPDATE_ACCURACY_DATA_SQL = GET_TOUCHED_FILES +
            @"
                ; WITH
                 MostRecent AS (
                    SELECT AttributeSetName.Description
                     ,MAX(AttributeSetForFile.FileTaskSessionID) AS MostRecentFileTaskSession
                     ,AttributeSetForFile.AttributeSetNameID
                     ,FileTaskSession.FileID
                    
                    FROM AttributeSetForFile
                    INNER JOIN AttributeSetName ON AttributeSetForFile.AttributeSetNameID = AttributeSetName.ID
                    INNER JOIN FileTaskSession WITH (NOLOCK) ON AttributeSetForFile.FileTaskSessionID = FileTaskSession.ID 
                    	AND FileTaskSession.ID <= @LastInBatchID
                    INNER JOIN @FilesTable FT ON FT.FileID = FileTaskSession.FileID 
                    GROUP BY AttributeSetName.Description
                     ,AttributeSetForFile.AttributeSetNameID
                     ,FileTaskSession.FileID
                    HAVING AttributeSetName.Description in (@FoundSetName,@ExpectedSetName)
                )
                
                SELECT FoundAttributeSet.ID AS FoundAttributeSetFileID
                   ,FoundAttributeSet.VOA FoundVOA
                   ,ExpectedAttributeSet.ID AS ExpectedAttributeSetFileID
                   ,ExpectedAttributeSet.VOA AS ExpectedVOA
                   ,found.FileID
                      ,foundFTS.DateTimeStamp FoundDateTimeStamp
                      ,foundFTS.ActionID FoundActionID
                      ,foundFS.FAMUserID FoundFAMUserID
                      ,expectedFTS.DateTimeStamp ExpectedDateTimeStamp
                      ,expectedFTS.ActionID ExpectedActionID
                      ,expectedFS.FAMUserID ExpectedFAMUserID
                FROM MostRecent found 
                 INNER JOIN MostRecent expected 
                  ON found.Description = @FoundSetName AND Expected.Description = @ExpectedSetName 
                   AND found.FileID = expected.FileID
                    INNER JOIN FileTaskSession foundFTS WITH (NOLOCK)
                            ON found.MostRecentFileTaskSession = foundFTS.ID
                    INNER JOIN FAMSession foundFS  WITH (NOLOCK)
                        ON foundFTS.FAMSessionID = foundFS.ID
                    INNER JOIN FileTaskSession expectedFTS  WITH (NOLOCK)
                            ON expected.MostRecentFileTaskSession = expectedFTS.ID
                    INNER JOIN FAMSession expectedFS  WITH (NOLOCK)
                        ON expectedFTS.FAMSessionID = expectedFS.ID
                 INNER JOIN AttributeSetForFile FoundAttributeSet 
                  ON FoundAttributeSet.FileTaskSessionID = found.MostRecentFileTaskSession 
                   AND FoundAttributeSet.AttributeSetNameID = found.AttributeSetNameID
                 INNER JOIN AttributeSetForFile ExpectedAttributeSet 
                  ON ExpectedAttributeSet.FileTaskSessionID = expected.MostRecentFileTaskSession 
                   AND ExpectedAttributeSet.AttributeSetNameID = expected.AttributeSetNameID
        ";

        /// <summary>
        /// Query used to Delete the old data from ReportingRedactionAccuracy
        /// it uses the GET_TOUCHED_FILES query
        /// This query requires values for the following parameters
        ///     @LastInBatchID - last file task session id in the batch
        ///     @LastProcessedID - Last processed file task session
        ///     @DatabaseServiceID - Id of the record in the DatabaseService table for this service instance
        /// </summary>
        static readonly string DELETE_OLD_DATA = GET_TOUCHED_FILES +
            @"
                DELETE FROM ReportingRedactionAccuracy 
                    WHERE DatabaseServiceID = @DatabaseServiceID
                        AND FileID IN (SELECT FileID FROM @FilesTable)
            ";

        #endregion

        #region Fields

        /// <summary>
        /// Indicates whether the Process method is currently executing.
        /// </summary>
        bool _processing;


        /// <summary>
        /// The current status info for this service.
        /// </summary>
        RedactionAccuracyStatus _status;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Default constructor for RedactionAccuracy
        /// </summary>
        public RedactionAccuracy()
        {
        }

        #endregion Constructors

        #region DatabaseService implementation

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
        /// Performs the processing needed for the records in ReportingRedactionAccuracy table
        /// </summary>
        /// <param name="cancelToken">Token that can cancel the processing</param>
        public override void Process(CancellationToken cancelToken)
        {
            try
            {
                _processing = true;

                RefreshStatus();
                ExtractException.Assert("ELI46587", "Status cannot be null", _status != null);

                ClearReportingReportingRedactionAccuracy(cancelToken);

                // Get the maximum File task session id available
                Int32 maxFileTaskSession = MaxReportableFileTaskSessionId(true);

                // Process the entries in chunks of 100 file task session
                while (_status.LastFileTaskSessionIDProcessed < maxFileTaskSession)
                {
                    ConcurrentQueue<string> queriesToRunInBatch = new ConcurrentQueue<string>();

                    cancelToken.ThrowIfCancellationRequested();

                    int lastInBatchToProcess = Math.Min(_status.LastFileTaskSessionIDProcessed + _PROCESS_BATCH_SIZE, maxFileTaskSession);

                    using (var connection = new ExtractRoleConnection(DatabaseServer, DatabaseName))
                    {
                        connection.Open();
                        using var cmd = connection.CreateCommand();

                        // Set the timeout so that it waits indefinitely
                        cmd.CommandTimeout = 0;
                        cmd.CommandText = UPDATE_ACCURACY_DATA_SQL;

                        cmd.Parameters.AddWithValue("@FoundSetName", FoundAttributeSetName);
                        cmd.Parameters.AddWithValue("@ExpectedSetName", ExpectedAttributeSetName);
                        cmd.Parameters.AddWithValue("@DatabaseServiceID", DatabaseServiceID);
                        cmd.Parameters.AddWithValue("@LastProcessedID", _status.LastFileTaskSessionIDProcessed);
                        cmd.Parameters.AddWithValue("@LastInBatchID", lastInBatchToProcess);

                        queriesToRunInBatch.Enqueue(DELETE_OLD_DATA);

                        // Get VOA data for each file
                        SaveAccuracy(cmd, queriesToRunInBatch, cancelToken);
                    }

                    // Records to calculate stats
                    using var saveConnection = new ExtractRoleConnection(DatabaseServer, DatabaseName);
                    saveConnection.Open();

                    using var scope = new TransactionScope(TransactionScopeOption.Required,
                        new TransactionOptions()
                        {
                            IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead,
                            Timeout = TransactionManager.MaximumTimeout
                        },
                        TransactionScopeAsyncFlowOption.Enabled);

                    foreach (var q in queriesToRunInBatch)
                    {
                        using var saveCmd = saveConnection.CreateCommand();
                        saveCmd.CommandTimeout = 0;
                        saveCmd.CommandText = q;
                        saveCmd.Parameters.AddWithValue("@FoundSetName", FoundAttributeSetName);
                        saveCmd.Parameters.AddWithValue("@ExpectedSetName", ExpectedAttributeSetName);
                        saveCmd.Parameters.AddWithValue("@DatabaseServiceID", DatabaseServiceID);
                        saveCmd.Parameters.AddWithValue("@LastProcessedID", _status.LastFileTaskSessionIDProcessed);
                        saveCmd.Parameters.AddWithValue("@LastInBatchID", lastInBatchToProcess);
                        var saveTask = saveCmd.ExecuteNonQueryAsync();
                        saveTask.Wait(cancelToken);

                    }

                    _status.LastFileTaskSessionIDProcessed = lastInBatchToProcess;

                    _status.SaveStatus(saveConnection, DatabaseServiceID);
                    scope.Complete();
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45384");
            }
            finally
            {
                _processing = false;
            }
        }

        private void ClearReportingReportingRedactionAccuracy(CancellationToken cancelToken)
        {
            var connection = new ExtractRoleConnection(DatabaseServer, DatabaseName);
            connection.Open();

            // Clear the ReportingRedactionAccuracy table if the LastFileTaskSessionIDProcessed is 0
            if (_status.LastFileTaskSessionIDProcessed == 0)
            {
                using var deleteCmd = connection.CreateCommand();
                using var scope = new TransactionScope(TransactionScopeOption.Required,
                    new TransactionOptions()
                    {
                        IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead,
                        Timeout = TransactionManager.MaximumTimeout
                    },
                    TransactionScopeAsyncFlowOption.Enabled);

                deleteCmd.CommandTimeout = 0;
                deleteCmd.CommandText = "DELETE FROM ReportingRedactionAccuracy WHERE DatabaseServiceID = @DatabaseServiceID";
                deleteCmd.Parameters.AddWithValue("@DatabaseServiceID", DatabaseServiceID);
                var task = deleteCmd.ExecuteNonQueryAsync();
                task.Wait(cancelToken);
                scope.Complete();
            }
        }
        #endregion

        #endregion

        #region IConfigSettings implementation

        public bool IsConfigured()
        {
            try
            {
                return !string.IsNullOrWhiteSpace(Description) &&
                    !string.IsNullOrWhiteSpace(FoundAttributeSetName) &&
                    !string.IsNullOrWhiteSpace(ExpectedAttributeSetName);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45684");
            }
        }
        public bool Configure()
        {
            try
            {
                RedactionAccuracyForm form = new RedactionAccuracyForm(this);
                return form.ShowDialog() == DialogResult.OK;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45681");
            }
            return false;
        }

        #endregion


        #region RedactionAccuracy Properties

        /// <summary>
        /// XPath query of attributes to be compared
        /// </summary>
        [DataMember]
        public string XPathOfSensitiveAttributes { get; set; } = @"                    
                    /*/HCData
                  | /*/MCData
                  | /*/LCData
                  | /*/Manual";

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

        [DataMember]
        public override int Version { get; protected set; } = CURRENT_VERSION;
        #endregion

        #region IHasConfigurableDatabaseServiceStatus

        /// <summary>
        /// The <see cref="DatabaseServiceStatus"/> for this instance
        /// </summary>
        public DatabaseServiceStatus Status
        {
            get => _status = _status ?? GetLastOrCreateStatus(() => new RedactionAccuracyStatus()
            {
                LastFileTaskSessionIDProcessed = -1
            });

            set => _status = value as RedactionAccuracyStatus;
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
                    _status = GetLastOrCreateStatus(() => new RedactionAccuracyStatus());
                }
                else
                {
                    _status = null;
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI46583");
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Calculates the stats for each of the records returned by the command and saves the results to 
        /// ReportingRedactionAccuracy table
        /// </summary>
        /// <param name="cmd">Command to get the data needed to calculate the stats for the current block of data being processed</param>
        /// <param name="cancelToken"></param>
        void SaveAccuracy(AppRoleCommand cmd, ConcurrentQueue<string> queriesToRunInBatch, CancellationToken cancelToken)
        {
            // keep track of number of active threads
            using (CountdownEvent threadCountDown = new CountdownEvent(1))
            using (Semaphore threadSemaphore = new Semaphore(NumberOfProcessingThreads, NumberOfProcessingThreads))
            using (SqlDataReader ExpectedAndFoundReader = cmd.ExecuteReader())
            using (CancellationTokenSource exceptionCancel = new CancellationTokenSource())
            using (CancellationTokenSource multipleCancel = CancellationTokenSource.CreateLinkedTokenSource(exceptionCancel.Token, cancelToken))
            {
                try
                {
                    // Get the ordinal for the FoundVOA and ExpectedVOA columns
                    int foundVOAColumn = ExpectedAndFoundReader.GetOrdinal("FoundVOA");
                    int expectedVOAColumn = ExpectedAndFoundReader.GetOrdinal("ExpectedVOA");
                    int foundAttributeForFileSetColumn = ExpectedAndFoundReader.GetOrdinal("FoundAttributeSetFileID");
                    int expectedAttributeForFileSetColumn = ExpectedAndFoundReader.GetOrdinal("ExpectedAttributeSetFileID");
                    int fileIDColumn = ExpectedAndFoundReader.GetOrdinal("FileID");
                    int foundDateTimeStampColumn = ExpectedAndFoundReader.GetOrdinal("FoundDateTimeStamp");
                    int foundActionIDColumn = ExpectedAndFoundReader.GetOrdinal("FoundActionID");
                    int foundFAMUserIDColumn = ExpectedAndFoundReader.GetOrdinal("FoundFAMUserID");
                    int expectedDateTimeStampColumn = ExpectedAndFoundReader.GetOrdinal("ExpectedDateTimeStamp");
                    int expectedActionIDColumn = ExpectedAndFoundReader.GetOrdinal("ExpectedActionID");
                    int expectedFAMUserIDColumn = ExpectedAndFoundReader.GetOrdinal("ExpectedFAMUserID");

                    // Process the found records
                    while (ExpectedAndFoundReader.Read())
                    {
                        multipleCancel.Token.ThrowIfCancellationRequested();

                        // Get the streams for the expected and found voa data (the thread will read the voa from the stream
                        Stream expectedStream = ExpectedAndFoundReader.GetStream(expectedVOAColumn);
                        Stream foundStream = ExpectedAndFoundReader.GetStream(foundVOAColumn);
                        Int64 foundID = ExpectedAndFoundReader.GetInt64(foundAttributeForFileSetColumn);
                        Int64 expectedID = ExpectedAndFoundReader.GetInt64(expectedAttributeForFileSetColumn);
                        Int32 fileID = ExpectedAndFoundReader.GetInt32(fileIDColumn);
                        DateTime foundDateTime = ExpectedAndFoundReader.GetDateTime(foundDateTimeStampColumn);
                        Int32 foundActionID = ExpectedAndFoundReader.GetInt32(foundActionIDColumn);
                        Int32 foundFAMUserID = ExpectedAndFoundReader.GetInt32(foundFAMUserIDColumn);
                        DateTime expectedDateTime = ExpectedAndFoundReader.GetDateTime(expectedDateTimeStampColumn);
                        Int32 expectedActionID = ExpectedAndFoundReader.GetInt32(expectedActionIDColumn);
                        Int32 expectedFAMUserID = ExpectedAndFoundReader.GetInt32(expectedFAMUserIDColumn);

                        // Get Semaphore before creating the thread
                        threadSemaphore.WaitOne();
                        threadCountDown.AddCount();

                        // Create a thread pool thread to do the comparison
                        ThreadPool.QueueUserWorkItem(delegate
                        {
                            try
                            {
                                // Put the expected and found streams in usings so they will be disposed
                                using (expectedStream)
                                using (foundStream)
                                {
                                    // Get the VOAs from the streams
                                    IUnknownVector ExpectedAttributes = AttributeMethods.GetVectorOfAttributesFromSqlBinary(expectedStream);
                                    ExpectedAttributes.ReportMemoryUsage();
                                    IUnknownVector FoundAttributes = AttributeMethods.GetVectorOfAttributesFromSqlBinary(foundStream);
                                    FoundAttributes.ReportMemoryUsage();

                                    // Compare the VOAs
                                    var output = IDShieldAttributeComparer.CompareAttributes(ExpectedAttributes, FoundAttributes,
                                                XPathOfSensitiveAttributes, multipleCancel.Token).ToList();

                                    // process output for each page
                                    foreach (var pageKeyPair in output)
                                    {
                                        int page = pageKeyPair.Key;

                                        // Add the comparison results to the Results
                                        var statsToSave = pageKeyPair.Value.AggregateStatistics(multipleCancel.Token).ToList();

                                        var lookup = statsToSave.ToLookup(a => new { a.Path, a.Label });

                                        var attributePaths = statsToSave
                                            .Select(a => a.Path)
                                            .Distinct()
                                            .OrderBy(p => p)
                                            .ToList();

                                        List<string> valuesToAdd = new List<string>();
                                        foreach (var p in attributePaths)
                                        {
                                            int expected = lookup[new { Path = p, Label = AccuracyDetailLabel.Expected }].Sum(a => a.Value);
                                            int found = lookup[new { Path = p, Label = AccuracyDetailLabel.Found }].Sum(a => a.Value);
                                            int correct = lookup[new { Path = p, Label = AccuracyDetailLabel.Correct }].Sum(a => a.Value);
                                            int falsePositives = lookup[new { Path = p, Label = AccuracyDetailLabel.FalsePositives }].Sum(a => a.Value);
                                            int overRedacted = lookup[new { Path = p, Label = AccuracyDetailLabel.OverRedacted }].Sum(a => a.Value);
                                            int underRedacted = lookup[new { Path = p, Label = AccuracyDetailLabel.UnderRedacted }].Sum(a => a.Value);
                                            int missed = lookup[new { Path = p, Label = AccuracyDetailLabel.Missed }].Sum(a => a.Value);

                                            valuesToAdd.Add(string.Format(CultureInfo.InvariantCulture,
                                                @"({0}, {1}, {2}, {3}, {4}, '{5}', {6}, {7}, {8}, {9}, {10}, {11}, {12}, '{13:s}', {14}, {15}, '{16:s}', {17}, {18} )"
                                                , DatabaseServiceID
                                                , foundID
                                                , expectedID
                                                , fileID
                                                , page
                                                , p
                                                , expected
                                                , found
                                                , correct
                                                , falsePositives
                                                , overRedacted
                                                , underRedacted
                                                , missed
                                                , foundDateTime
                                                , foundFAMUserID
                                                , foundActionID
                                                , expectedDateTime
                                                , expectedFAMUserID
                                                , expectedActionID
                                                ));
                                        }

                                        queriesToRunInBatch.Enqueue(string.Format(CultureInfo.InvariantCulture,
                                            @"
                                        INSERT INTO [dbo].[ReportingRedactionAccuracy]
                                                ([DatabaseServiceID]
                                                ,[FoundAttributeSetForFileID]
                                                ,[ExpectedAttributeSetForFileID]
                                                ,[FileID]
                                                ,[Page]
                                                ,[Attribute]
                                                ,[Expected]
                                                ,[Found]
                                                ,[Correct]
                                                ,[FalsePositives]
                                                ,[OverRedacted]
                                                ,[UnderRedacted]
                                                ,[Missed]
                                                ,[FoundDateTimeStamp]
                                                ,[FoundFAMUserID]
                                                ,[FoundActionID]
									,[ExpectedDateTimeStamp]
                                                ,[ExpectedFAMUserID]
                                                ,[ExpectedActionID])
                                                VALUES
                                                    {3};", DatabaseServiceID, fileID, page, string.Join(",\r\n", valuesToAdd)));
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                ex.AsExtract("ELI45383").Log();
                            }
                            finally
                            {
                                // Release semaphore after thread has been created
                                threadSemaphore.Release();

                                // Decrement the number of pending threads
                                threadCountDown.Signal();
                            }
                        });
                    }
                }
                catch
                {
                    if (!exceptionCancel.IsCancellationRequested)
                    {
                        exceptionCancel.Cancel();
                    }
                    throw;
                }
                finally
                {
                    threadCountDown.Signal();
                    threadCountDown.Wait();
                }
            }
        }

        /// <summary>
        /// Called after this instance is deserialized.
        /// </summary>
        /// <param name="context">The context.</param>
        [OnDeserialized]
        void OnDeserialized(StreamingContext context)
        {
            if (Version > CURRENT_VERSION)
            {
                ExtractException ee = new ExtractException("ELI45385", "Settings were saved with a newer version.");
                ee.AddDebugData("SavedVersion", Version, false);
                ee.AddDebugData("CurrentVersion", CURRENT_VERSION, false);
                throw ee;
            }

            Version = CURRENT_VERSION;
        }

        #endregion
    }
}
