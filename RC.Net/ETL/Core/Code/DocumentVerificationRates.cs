using Extract.Code.Attributes;
using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using System.Windows.Forms;

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
            [DataMember]
            public int LastFileTaskSessionIDProcessed { get; set; } = 0;

            /// <summary>
            /// Set of FileTaskSession Ids that where associated with an active FAM in the last run
            /// NOTE: This is no longer used and if it has values the ReportingVerificationRates Table will be cleared
            /// </summary>
            [DataMember]
            public HashSet<Int32> SetOfActiveFileTaskIds { get; protected set; } = new HashSet<int>();

            #endregion

            #region DatabaseVerificationStatus Serialization

            /// <summary>
            /// Called after this instance is deserialized.
            /// </summary>
            [OnDeserialized]
            void OnDeserialized(StreamingContext context)
            {
                if (Version > CURRENT_VERSION)
                {
                    ExtractException ee = new ExtractException("ELI45475", "Settings were saved with a newer version.");
                    ee.AddDebugData("SavedVersion", Version, false);
                    ee.AddDebugData("CurrentVersion", CURRENT_VERSION, false);
                    throw ee;
                }

                Version = CURRENT_VERSION;
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
        ///     or (<comma separated list of previously active id's>)
        /// </summary>
        static readonly string _QUERY_FOR_SOURCE_RECORDS = @"
            SELECT [FileTaskSession].[ID]
                     ,[FileID]
            		 ,[FileTaskSession].[ActionID]
                     ,[TaskClassID]
                     , COALESCE([Duration], 0.0) [Duration]
                     , COALESCE([OverheadTime], 0.0) [OverheadTime]
                     , COALESCE([ActivityTime], 0.0) [ActivityTime] 
                 FROM [dbo].[FileTaskSession] 
                   INNER JOIN [dbo].[TaskClass] ON [TaskClass].[ID] = [FileTaskSession].[TaskClassID]
                   
                 WHERE ([TaskClass].GUID IN 
                           ('FD7867BD-815B-47B5-BAF4-243B8C44AABB', 
                            '59496DF7-3951-49B7-B063-8C28F4CD843F', 
                            'AD7F3F3F-20EC-4830-B014-EC118F6D4567' )) 
                       AND (([FileTaskSession].[ID] > @LastProcessedFileTaskSessionID 
            			AND [FileTaskSession].[ID] < = @EndOfBatch
            AND [FileTaskSession].[Duration] IS NOT NULL 
            AND [FileTaskSession].OverheadTime IS NOT NULL))
            ORDER BY ID ASC";

        /// <summary>
        /// Query to add or update the ReportingVerificationRates table
        /// Requires the following parameters
        ///     @FileID INT
        ///     @ActionID INT
        ///     @TaskClassID INT
        ///     @Duration FLOAT
        ///     @Overhead FLOAT
        ///     @ActivityTime FLOAT
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
                    [LastFileTaskSessionID] = @LastFileTaskSessionID
                WHERE [FileID] = @FileID AND [ActionID] = @ActionID AND [TaskClassID] = @TaskClassID AND [DatabaseServiceID] = @DatabaseServiceID
            END ELSE BEGIN
                /* Insert New records only if the duration and overhead is not 0 */
                IF (ABS(@Duration) > 0.00001 AND ABS(@Overhead) > 0.00001) 
                BEGIN
                    INSERT INTO [ReportingVerificationRates]
                        ([DatabaseServiceID], [FileID], [ActionID], [TaskClassID], [LastFileTaskSessionID], [Duration], [OverheadTime], [ActivityTime])
                    VALUES 
                        (@DatabaseServiceID, @FileID,  @ActionID, @TaskClassID,  @LastFileTaskSessionID,  @Duration,  @Overhead,  @ActivityTime)
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

                // Reset the Stats on the following conditions
                if (_status.LastFileTaskSessionIDProcessed <= 0 || _status.SetOfActiveFileTaskIds.Count > 0)
                {
                    ResetStatistics();
                }
                int maxFileTaskSession = MaxReportableFileTaskSessionId();


                while (_status.LastFileTaskSessionIDProcessed < maxFileTaskSession)
                {
                    using (var scope = GetNewTransactionScope())
                    {
                        using (var connection = NewSqlDBConnection())
                        {
                            connection.Open();

                            using (var sourceCmd = connection.CreateCommand())
                            {
                                int endOfBatch = Math.Min(_status.LastFileTaskSessionIDProcessed + PROCESS_BATCH_SIZE, maxFileTaskSession);

                                sourceCmd.CommandTimeout = 0;
                                sourceCmd.CommandText = _QUERY_FOR_SOURCE_RECORDS;

                                sourceCmd.Parameters.AddWithValue("@LastProcessedFileTaskSessionID", _status.LastFileTaskSessionIDProcessed);
                                sourceCmd.Parameters.AddWithValue("@EndOfBatch", endOfBatch);

                                sourceCmd.CommandTimeout = 0;
                                using (var readerTask = sourceCmd.ExecuteReaderAsync(cancelToken))
                                {
                                    ProcessBatch(connection, readerTask);
                                }
                                _status.LastFileTaskSessionIDProcessed = endOfBatch;

                            }
                        }
                        scope.Complete();
                    }

                    // There is a chance that this status will get out of sync with the ReportingVerificationRates
                    try
                    {
                        SaveStatus();
                    }
                    catch (Exception saveException )
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
            _status.LastFileTaskSessionIDProcessed = -1;
            _status.SetOfActiveFileTaskIds.Clear();

            SaveStatus();

            using (var scope = GetNewTransactionScope())
            using (var connection = NewSqlDBConnection())
            {
                connection.Open();
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandTimeout = 0;
                    cmd.CommandText = "DELETE FROM [ReportingVerificationRates]";
                    var deleteTask = cmd.ExecuteNonQueryAsync();
                    deleteTask.Wait(_cancelToken);

                    scope.Complete();
                }
            }
        }

        /// <summary>
        /// Processes the current batch
        /// </summary>
        /// <param name="readerTask">Reader task that contains the records to be processed</param>
        /// <param name="connection">Connection to use to process the batch</param>
        void ProcessBatch(SqlConnection connection,  Task<SqlDataReader> readerTask)
        {
            using (var sourceReader = readerTask.Result)
            {
                while (sourceReader.Read())
                {
                    _cancelToken.ThrowIfCancellationRequested();

                    Int32 fileTaskSessionID = sourceReader.GetInt32(sourceReader.GetOrdinal("ID"));
                    Int32 actionID = sourceReader.GetInt32(sourceReader.GetOrdinal("ActionID"));
                    Int32 taskClassID = sourceReader.GetInt32(sourceReader.GetOrdinal("TaskClassID"));
                    Int32 fileID = sourceReader.GetInt32(sourceReader.GetOrdinal("FileID"));
                    Double duration = sourceReader.GetDouble(sourceReader.GetOrdinal("Duration"));
                    Double overhead = sourceReader.GetDouble(sourceReader.GetOrdinal("OverheadTime"));
                    Double activityTime = sourceReader.GetDouble(sourceReader.GetOrdinal("ActivityTime"));

                    try
                    {
                        using (var saveCmd = connection.CreateCommand())
                        {
                            saveCmd.CommandText = _QUERY_TO_ADD_UPDATE_REPORTING_VERIFICATION;
                            saveCmd.Parameters.Add("@FileID", SqlDbType.Int).Value = fileID;
                            saveCmd.Parameters.Add("@ActionID", SqlDbType.Int).Value = actionID;
                            saveCmd.Parameters.Add("@TaskClassID", SqlDbType.Int).Value = taskClassID;
                            saveCmd.Parameters.Add("@LastFileTaskSessionID", SqlDbType.Int).Value = fileTaskSessionID;
                            saveCmd.Parameters.Add("@Duration", SqlDbType.Float).Value = duration;
                            saveCmd.Parameters.Add("@Overhead", SqlDbType.Float).Value = overhead;
                            saveCmd.Parameters.Add("@ActivityTime", SqlDbType.Float).Value = activityTime;
                            saveCmd.Parameters.Add("@DatabaseServiceID", SqlDbType.Int).Value = DatabaseServiceID;

                            var task = saveCmd.ExecuteNonQueryAsync();
                            task.Wait(_cancelToken);
                        }
                    }
                    catch (Exception ex)
                    {
                        throw ex.AsExtract("ELI45472");
                    }
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
        void SaveStatus()
        {
            SaveStatus(_status);
        }

        /// <summary>
        /// Gets a <see cref="TransactionScope"/> that has been configured 
        /// </summary>
        /// <returns>Configured <see cref="TransactionScope"/></returns>
        TransactionScope GetNewTransactionScope()
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
            get => _status ?? new DocumentVerificationStatus
            {
                LastFileTaskSessionIDProcessed = -1
            };

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
