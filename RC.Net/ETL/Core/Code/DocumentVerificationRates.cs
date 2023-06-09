﻿using Extract.Code.Attributes;
using Extract.SqlDatabase;
using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using System.Windows.Forms;
using static System.FormattableString;

namespace Extract.ETL
{
    /// <summary>
    /// Class to implement the DocumentVerificationRates database service to populate the ReportingDocumentVerificationRates table
    /// </summary>
    [DataContract]
    [KnownType(typeof(ScheduledEvent))]
    [ExtractCategory("DatabaseService", "Document verification rates")]
    public class DocumentVerificationRates : DatabaseService, IConfigSettings, IHasConfigurableDatabaseServiceStatus
    {
        #region Internal classes

        /// <summary>
        /// Class for the DocumentVerificationTimes status stored in the DatabaseService record
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
        [DataContract]
        public class DocumentVerificationStatus : DatabaseServiceStatus, IFileTaskSessionServiceStatus
        {
            #region DatabaseVerificationStatus constants

            const int _CURRENT_VERSION = 1;

            #endregion

            #region DatabaseVerificationStatus Properties

            [DataMember]
            public override int Version { get; protected set; } = _CURRENT_VERSION;

            /// <summary>
            /// Maintains the last FileTaskSessionID that was processed successfully
            /// </summary>
            public int LastFileTaskSessionIDProcessed { get; set; } = 0;

            /// <summary>
            /// Set of FileTaskSession Ids that where associated with an active FAM in the last run
            /// NOTE: This is no longer used and if it has values the ReportingVerificationRates Table will be cleared
            /// </summary>
            [DataMember]
            public HashSet<Int32> SetOfActiveFileTaskIds { get; } = new HashSet<int>();

            #endregion

            #region DatabaseVerificationStatus Serialization

            /// <summary>
            /// Called after this instance is deserialized.
            /// </summary>
            [OnDeserialized]
            void OnDeserialized(StreamingContext context)
            {
                if (Version > _CURRENT_VERSION)
                {
                    ExtractException ee = new ExtractException("ELI45475", "Settings were saved with a newer version.");
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
        /// The number of FileTaskSession rows to process in a single transaction.
        /// </summary>
        const int PROCESS_BATCH_SIZE = 100;

        /// <summary>
        /// Query to select the records to be used for updating ReportingVerificationRates table
        /// Requires the following parameters
        ///     @LastProcessedFileTaskSessionID INT 
        ///     @EndOfBatch INT
        ///     
        /// This string is to be used in a string.Format statement with {0} either (0) if no previous ID's 
        ///     or (<comma separated list of previously active id''s>)
        /// </summary>
        static readonly string _QUERY_FOR_SOURCE_RECORDS = Invariant($@"
            SELECT [FileTaskSession].[ID]
	            ,[FileID]
	            ,[FileTaskSession].[ActionID]
	            ,[TaskClassID]
	            ,COALESCE([Duration], 0.0) [Duration]
	            ,COALESCE([OverheadTime], 0.0) [OverheadTime]
	            ,COALESCE([ActivityTime], 0.0) [ActivityTime]
                ,COALESCE([DurationMinusTimeout], 0.0) [DurationMinusTimeout]
            FROM [dbo].[FileTaskSession] WITH (NOLOCK)
            INNER JOIN [dbo].[TaskClass]
	            ON [TaskClass].[ID] = [FileTaskSession].[TaskClassID]
            WHERE [TaskClass].GUID IN (
		            '{Constants.TaskClassWebVerification}'
		            ,'{Constants.TaskClassDataEntryVerification}'
		            ,'{Constants.TaskClassRedactionVerification}'
		            ,'{Constants.TaskClassPaginationVerification}'
		            )
	            AND [FileTaskSession].[ID] > @LastProcessedFileTaskSessionID
	            AND [FileTaskSession].[ID] <= @EndOfBatch
	            AND [FileTaskSession].[Duration] IS NOT NULL
	            AND [FileTaskSession].OverheadTime IS NOT NULL
	            AND ActionID IS NOT NULL
            ORDER BY ID ASC");
        // skip null ActionID for https://extract.atlassian.net/browse/ISSUE-16932

        /// <summary>
        /// Query to add or update the ReportingVerificationRates table
        /// Requires the following parameters
        ///     @FileID INT
        ///     @ActionID INT
        ///     @TaskClassID INT
        ///     @Duration FLOAT
        ///     @Overhead FLOAT
        ///     @ActivityTime FLOAT
        ///     @DurationMinusTimeout FLOAT
        ///     @LastFileTaskSessionID INT
        ///     @DatabaseServiceID INT
        ///     
        /// </summary>
        static readonly string _QUERY_TO_ADD_UPDATE_REPORTING_VERIFICATION = @"
            IF EXISTS (SELECT 1 FROM [ReportingVerificationRates] 
                WHERE [FileID] = @FileID AND [ActionID] = @ActionID AND [TaskClassID] = @TaskClassID AND [DatabaseServiceID] = @DatabaseServiceID) 
            BEGIN
                /* Update existing record */
                UPDATE [ReportingVerificationRates] 
                SET 
                    [Duration] = [Duration] + @Duration,
                    [OverheadTime] = [OverheadTime] + @Overhead,
                    [ActivityTime] = [ActivityTime] + @ActivityTime,
                    [DurationMinusTimeout] = [DurationMinusTimeout] + @DurationMinusTimeout,
                    [LastFileTaskSessionID] = @LastFileTaskSessionID
                WHERE [FileID] = @FileID AND [ActionID] = @ActionID AND [TaskClassID] = @TaskClassID AND [DatabaseServiceID] = @DatabaseServiceID
            END ELSE BEGIN
                /* Insert New records only if the duration is not 0 */
                IF (ABS(@Duration) > 0.00001) 
                BEGIN
                    INSERT INTO [ReportingVerificationRates]
                        ([DatabaseServiceID], [FileID], [ActionID], [TaskClassID], [LastFileTaskSessionID], [Duration], [OverheadTime], [ActivityTime], [DurationMinusTimeout])
                    VALUES 
                        (@DatabaseServiceID, @FileID,  @ActionID, @TaskClassID,  @LastFileTaskSessionID,  @Duration,  @Overhead,  @ActivityTime, @DurationMinusTimeout)
                END
            END
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
        DocumentVerificationStatus _status;

        /// <summary>
        /// <see cref="CancellationToken"/> that was passed into the <see cref="Process(CancellationToken)"/> method
        /// </summary>
        CancellationToken _cancelToken = CancellationToken.None;

        #endregion

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

        [DataMember]
        public override int Version { get; protected set; } = CURRENT_VERSION;

        #endregion DatabaseService Properties

        #region DatabaseService Methods
        public override void Process(CancellationToken cancelToken)
        {
            try
            {
                _processing = true;

                _cancelToken = cancelToken;

                RefreshStatus();
                ExtractException.Assert("ELI46588", "Status cannot be null", _status != null);

                // Reset the Stats on the following conditions
                if (_status.LastFileTaskSessionIDProcessed <= 0 || _status.SetOfActiveFileTaskIds.Count > 0)
                {
                    ResetStatistics();
                }
                int maxFileTaskSession = MaxReportableFileTaskSessionId(false);

                while (_status.LastFileTaskSessionIDProcessed < maxFileTaskSession)
                {
                    using var connection = new ExtractRoleConnection(DatabaseServer, DatabaseName);
                    connection.Open();
                    using var scope = GetNewTransactionScope();

                    using var sourceCmd = connection.CreateCommand();

                    int endOfBatch = Math.Min(_status.LastFileTaskSessionIDProcessed + PROCESS_BATCH_SIZE, maxFileTaskSession);

                    sourceCmd.CommandTimeout = 0;
                    sourceCmd.CommandText = _QUERY_FOR_SOURCE_RECORDS;

                    sourceCmd.Parameters.AddWithValue("@LastProcessedFileTaskSessionID", _status.LastFileTaskSessionIDProcessed);
                    sourceCmd.Parameters.AddWithValue("@EndOfBatch", endOfBatch);

                    sourceCmd.CommandTimeout = 0;
                    ProcessBatch(connection, sourceCmd.ExecuteReaderAsync(cancelToken))
                        .GetAwaiter().GetResult(); // Use GetResult() instead of Result for better exceptions
                    _status.LastFileTaskSessionIDProcessed = endOfBatch;


                    scope.Complete();

                    // There is a chance that this status will get out of sync with the ReportingVerificationRates
                    try
                    {
                        SaveStatus(connection);
                    }
                    catch (Exception saveException)
                    {
                        ExtractException saveStatusException = new ExtractException("ELI46124", "There was a problem saving status", saveException);
                        saveStatusException.AddDebugData("DatabaseService", Description, false);
                        throw saveStatusException;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45449");
            }
            finally
            {
                _processing = false;
            }
        }
        #endregion

        #endregion

        #region Private Methods

        /// <summary>
        /// Clears the ReportingVerificationRates table
        /// </summary>
        void ResetStatistics()
        {
            using var connection = new ExtractRoleConnection(DatabaseServer, DatabaseName);
            connection.Open();

            using var scope = GetNewTransactionScope();

            _status.LastFileTaskSessionIDProcessed = -1;
            _status.SetOfActiveFileTaskIds.Clear();
            SaveStatus(connection);
            
            using var cmd = connection.CreateCommand();
            cmd.CommandTimeout = 0;
            cmd.CommandText = "DELETE FROM [ReportingVerificationRates]";
            var deleteTask = cmd.ExecuteNonQueryAsync();
            deleteTask.Wait(_cancelToken);

            scope.Complete();
        }

        /// <summary>
        /// Processes the current batch
        /// </summary>
        /// <param name="readerTask">Reader task that contains the records to be processed</param>
        /// <param name="connection">Connection to use to process the batch</param>
        async Task ProcessBatch(SqlAppRoleConnection connection,  Task<SqlDataReader> readerTask)
        {
            // As we can't use MARS on database connections established via application role
            // authentication, we need to compile as list of verifcation session data to be
            // processed rather processing each session as they are read.
            List<(
                Int32 fileTaskSessionID,
                Int32 actionID,
                Int32 taskClassID,
                Int32 fileID,
                Double duration,
                Double overhead,
                Double activityTime,
                Double durationMinusTimeout
                )> verificationSessionDataList = new();

            using (var sourceReader = await readerTask.ConfigureAwait(false))
            {
                while (sourceReader.Read())
                {
                    _cancelToken.ThrowIfCancellationRequested();

                    verificationSessionDataList.Add((
                        fileTaskSessionID: sourceReader.GetInt32(sourceReader.GetOrdinal("ID")),
                        actionID: sourceReader.GetInt32(sourceReader.GetOrdinal("ActionID")),
                        taskClassID: sourceReader.GetInt32(sourceReader.GetOrdinal("TaskClassID")),
                        fileID: sourceReader.GetInt32(sourceReader.GetOrdinal("FileID")),
                        duration: sourceReader.GetDouble(sourceReader.GetOrdinal("Duration")),
                        overhead: sourceReader.GetDouble(sourceReader.GetOrdinal("OverheadTime")),
                        activityTime: sourceReader.GetDouble(sourceReader.GetOrdinal("ActivityTime")),
                        durationMinusTimeout: sourceReader.GetDouble(sourceReader.GetOrdinal("DurationMinusTimeout"))));
                }
            }

            foreach (var sessionData in verificationSessionDataList)
            {
                try
                {
                    using var saveCmd = connection.CreateCommand();
                    saveCmd.CommandText = _QUERY_TO_ADD_UPDATE_REPORTING_VERIFICATION;
                    saveCmd.Parameters.Add("@FileID", SqlDbType.Int).Value = sessionData.fileID;
                    saveCmd.Parameters.Add("@ActionID", SqlDbType.Int).Value = sessionData.actionID;
                    saveCmd.Parameters.Add("@TaskClassID", SqlDbType.Int).Value = sessionData.taskClassID;
                    saveCmd.Parameters.Add("@LastFileTaskSessionID", SqlDbType.Int).Value = sessionData.fileTaskSessionID;
                    saveCmd.Parameters.Add("@Duration", SqlDbType.Float).Value = sessionData.duration;
                    saveCmd.Parameters.Add("@Overhead", SqlDbType.Float).Value = sessionData.overhead;
                    saveCmd.Parameters.Add("@ActivityTime", SqlDbType.Float).Value = sessionData.activityTime;
                    saveCmd.Parameters.Add("@DurationMinusTimeout", SqlDbType.Float).Value = sessionData.durationMinusTimeout;
                    saveCmd.Parameters.Add("@DatabaseServiceID", SqlDbType.Int).Value = DatabaseServiceID;

                    await saveCmd.ExecuteNonQueryAsync(_cancelToken).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI45472");
                }
            }
        }

        /// <summary>
        /// Called after this instance is deserialized.
        /// </summary>
        [OnDeserialized]
        void OnDeserialized(StreamingContext context)
        {
            if (Version > CURRENT_VERSION)
            {
                ExtractException ee = new ExtractException("ELI45448", "Settings were saved with a newer version.");
                ee.AddDebugData("SavedVersion", Version, false);
                ee.AddDebugData("CurrentVersion", CURRENT_VERSION, false);
                throw ee;
            }

            Version = CURRENT_VERSION;
        }

        public bool Configure()
        {
            try
            {
                DocumentVerificationRatesForm configForm = new DocumentVerificationRatesForm(this);
                return configForm.ShowDialog() == DialogResult.OK;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45685");
            }
            return false;
        }


        public bool IsConfigured()
        {
            return !string.IsNullOrWhiteSpace(Description);
        }

        /// <summary>
        /// Saves the current <see cref="DatabaseServiceStatus"/> to the DB
        /// </summary>
        /// <param name="connection">Connection to use to save status</param>
        void SaveStatus(SqlAppRoleConnection connection)
        {
            SaveStatus(connection, _status);
        }

        /// <summary>
        /// Gets a <see cref="TransactionScope"/> that has been configured 
        /// </summary>
        /// <returns>Configured <see cref="TransactionScope"/></returns>
        static TransactionScope GetNewTransactionScope()
        {
            return new TransactionScope(
                TransactionScopeOption.Required,
                new TransactionOptions()
                {
                    IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead,
                    Timeout = TransactionManager.MaximumTimeout,
                },
                TransactionScopeAsyncFlowOption.Enabled);
        }

        #endregion

        #region IHasConfigurableDatabaseServiceStatus

        /// <summary>
        /// The <see cref="DatabaseServiceStatus"/> for this instance
        /// </summary>
        public DatabaseServiceStatus Status
        {
            get => _status = _status ?? GetLastOrCreateStatus(() => new DocumentVerificationStatus()
            {
                LastFileTaskSessionIDProcessed = -1
            });

            set => _status = value as DocumentVerificationStatus;
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
                    _status = GetLastOrCreateStatus(() => new DocumentVerificationStatus());
                }
                else
                {
                    _status = null;
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI46119");
            }
        }

        #endregion IHasConfigurableDatabaseServiceStatus
    }
}
