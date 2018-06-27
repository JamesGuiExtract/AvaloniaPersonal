using Extract.Code.Attributes;
using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Runtime.Serialization;
using System.Threading;
using System.Transactions;
using System.Windows.Forms;

namespace Extract.ETL
{
    /// <summary>
    /// Class to implement the DocumentVerificationRates database service to populate the ReportingDocumentVerificationRates table
    /// </summary>
    [DataContract]
    [KnownType(typeof(ScheduledEvent))]
    [ExtractCategory("DatabaseService", "Document verification rates" )]
    public class DocumentVerificationRates : DatabaseService, IConfigSettings
    {
        #region Internal classes

        /// <summary>
        /// Class for the DocumentVerificationTimes status stored in the DatabaseService record
        /// </summary>
        [DataContract]
        public class DocumentVerificationStatus : DatabaseServiceStatus
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

        const int CURRENT_VERSION = 1;

        /// <summary>
        /// Query to select the records to be used for updating ReportingVerificationRates table
        /// Requires the following parameters
        ///     @LastFileTaskSessionID INT 
        ///     
        /// This string is to be used in a string.Format statement with {0} either (0) if no previous ID's 
        ///     or (<comma separated list of previously active id's>)
        /// </summary>
        static readonly string _QUERY_FOR_SOURCE_RECORDS = @"
           SELECT [FileTaskSession].[ID]
			  ,CASE 
                    WHEN ([ActiveFAM].FAMSessionID) IS NULL THEN 0
                    WHEN ([Duration] IS NULL AND [OverheadTime] IS NULL) THEN 1
			        ELSE 0 
			   END  ActiveFAM
              ,[FileTaskSession].[ActionID]
              ,[TaskClassID]
              ,[FileID]
              , COALESCE([Duration], 0.0) [Duration]
              , COALESCE([OverheadTime], 0.0) [OverheadTime]
              , COALESCE([ActivityTime], 0.0) [ActivityTime] 
          FROM [dbo].[FileTaskSession] 
            INNER JOIN [dbo].[TaskClass] ON [TaskClass].[ID] = [FileTaskSession].[TaskClassID]
            LEFT JOIN [dbo].[ActiveFAM] ON [FileTaskSession].[FAMSessionID] = [ActiveFAM].[FAMSessionID]
          WHERE ([TaskClass].GUID IN 
                    ('FD7867BD-815B-47B5-BAF4-243B8C44AABB', 
                     '59496DF7-3951-49B7-B063-8C28F4CD843F', 
                     'AD7F3F3F-20EC-4830-B014-EC118F6D4567' )) 
                AND (([FileTaskSession].[ID] > @LastFileTaskSessionID 
				AND [FileTaskSession].[Duration] IS NOT NULL 
				AND [FileTaskSession].OverheadTime IS NOT NULL)
				OR [ActiveFAM].FAMSessionID IS NOT NULL
				OR ([FileTaskSession].[ID] IN ({0})))
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

                var status = GetLastOrCreateStatus(() => new DocumentVerificationStatus());

                using (var connection = NewSqlDBConnection())
                {
                    connection.Open();

                    var sourceCmd = connection.CreateCommand();
                    sourceCmd.CommandText = string.Format(CultureInfo.InvariantCulture,
                        _QUERY_FOR_SOURCE_RECORDS, 
                        (status.SetOfActiveFileTaskIds.Count == 0) ? "0": string.Join(",", status.SetOfActiveFileTaskIds));
                    sourceCmd.Parameters.Add("@LastFileTaskSessionID", SqlDbType.Int).Value = status.LastFileTaskSessionIDProcessed;

                    sourceCmd.CommandTimeout = 0;
                    var readerTask = sourceCmd.ExecuteReaderAsync(cancelToken);

                    using (var sourceReader = readerTask.Result)
                    {
                        while (sourceReader.Read())
                        {
                            cancelToken.ThrowIfCancellationRequested();

                            Int32 fileTaskSessionID = sourceReader.GetInt32(sourceReader.GetOrdinal("ID"));
                            bool activeFAM = sourceReader.GetInt32(sourceReader.GetOrdinal("ActiveFAM")) != 0;
                            Int32 actionID = sourceReader.GetInt32(sourceReader.GetOrdinal("ActionID"));
                            Int32 taskClassID = sourceReader.GetInt32(sourceReader.GetOrdinal("TaskClassID"));
                            Int32 fileID = sourceReader.GetInt32(sourceReader.GetOrdinal("FileID"));
                            Double duration = sourceReader.GetDouble(sourceReader.GetOrdinal("Duration"));
                            Double overhead = sourceReader.GetDouble(sourceReader.GetOrdinal("OverheadTime"));
                            Double activityTime = sourceReader.GetDouble(sourceReader.GetOrdinal("ActivityTime"));

                            status.LastFileTaskSessionIDProcessed = fileTaskSessionID;

                            // Update the list of active file task ids for status
                            if (activeFAM && !status.SetOfActiveFileTaskIds.Contains(fileTaskSessionID))
                            {
                                status.SetOfActiveFileTaskIds.Add(fileTaskSessionID);
                            }
                            if (!activeFAM && status.SetOfActiveFileTaskIds.Contains(fileTaskSessionID))
                            {
                                status.SetOfActiveFileTaskIds.Remove(fileTaskSessionID);
                            }

                            try
                            {
                                using (var trans = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                                using (var saveConnection = NewSqlDBConnection())
                                {
                                    saveConnection.Open();
                                    using (var saveCmd = saveConnection.CreateCommand())
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
                                        task.Wait(cancelToken);

                                        status.SaveStatus(saveConnection, DatabaseServiceID);

                                        trans.Complete();
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                throw ex.AsExtract("ELI45472");
                            }
                        }
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
            catch(Exception ex)
            {
                ex.ExtractDisplay("ELI45685");
            }
            return false;
        }

        public bool IsConfigured()
        {
            return !string.IsNullOrWhiteSpace(Description);   
        }

        #endregion
    }
}
