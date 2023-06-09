﻿using Extract.Code.Attributes;
using Extract.SqlDatabase;
using Extract.Utilities;
using System;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using System.Threading;
using System.Transactions;
using System.Windows.Forms;
using static System.FormattableString;

namespace Extract.ETL
{
    [DataContract]
    [KnownType(typeof(ScheduledEvent))]
    [ExtractCategory("DatabaseService", "HIM stats service")]
    public class HIMStats : DatabaseService, IConfigSettings, IHasConfigurableDatabaseServiceStatus
    {
        #region Internal classes

        /// <summary>
        /// Class for the HIMStats status stored in the DatabaseService record
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1034:NestedTypesShouldNotBeVisible")]
        [DataContract]
        public class HIMStatsStatus : DatabaseServiceStatus, IFileTaskSessionServiceStatus
        {
            #region HIMStatsStatus constants

            const int _CURRENT_VERSION = 2;

            #endregion

            #region HIMStatsStatus Properties

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
                    ExtractException ee = new ExtractException("ELI46590", "Settings were saved with a newer version.");
                    ee.AddDebugData("SavedVersion", Version, false);
                    ee.AddDebugData("CurrentVersion", _CURRENT_VERSION, false);
                    throw ee;
                }
                if (Version < _CURRENT_VERSION)
                {
                    // Force recalculation all the stats
                    LastFileTaskSessionIDProcessed = 0;
                }

                Version = _CURRENT_VERSION;
            }

            #endregion
        }

        #endregion

        #region Constants

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
                        AND ([TaskClass].GUID IN (
                            '{Constants.TaskClassDocumentApi}', 
                            '{Constants.TaskClassDataEntryVerification}', 
                            '{Constants.TaskClassPaginationVerification}',
                            '{Constants.TaskClassAutoPaginate}'
                           ))
                )
                INSERT INTO @FilesTable
                    SELECT FileID FROM TouchedFiles; 
            ");


        readonly string _UpdateQuery = GET_TOUCHED_FILES + Invariant($@"
            WITH PaginationDataWithRank
                 AS (SELECT MAX(Pagination.ID) PaginationID, 
                            FAMSession.FAMUserID, 
                            SourceFileID, 
                            DestFileID, 
                            OriginalFileID, 
                            MAX(FileTaskSession.DateTimeStamp) DateTimeStamp, 
                            FileTaskSession.ActionID, 
                            ACTION.ASCName, 
                            MAX(Pagination.FileTaskSessionID) FileTaskSessionID
                     FROM [dbo].[Pagination] WITH (NOLOCK)
                          INNER JOIN FileTaskSession WITH (NOLOCK) ON FileTaskSession.ID =
                          Pagination.FileTaskSessionID
                                                        AND FileTaskSession.ID >
                                                        @LastProcessedID
                                                        AND FileTaskSession.ID <=
                                                        @LastInBatchID
                          INNER JOIN ACTION ON FileTaskSession.ActionID = ACTION.ID
                          INNER JOIN FAMSession WITH (NOLOCK) ON FAMSession.ID = FileTaskSession.
                          FAMSessionID
                     WHERE FileTaskSession.ActionID IS NOT NULL
                           AND FileTaskSession.DateTimeStamp IS NOT NULL
                     GROUP BY FAMUserID, 
                              SourceFileID, 
                              DestFileID, 
                              OriginalFileID, 
                              FileTaskSession.ActionID, 
                              FileTaskSessionID, 
                              ASCName),
                 ProcessedOutsidePagination
                 AS (SELECT NULL PaginationID, 
                            FamSession.FAMUserID, 
                            FileTaskSession.FileID SourceFileID, 
                            FileTaskSession.FileID DestFileID, 
                            FileTaskSession.FileID OriginalFileID, 
                            MAX(FileTaskSession.DateTimeStamp) [DateTimeStamp], 
                            FileTaskSession.ActionID, 
                            ACTION.ASCName, 
                            MAX(FileTaskSession.ID) [FileTaskSessionID]
                      FROM FileTaskSession WITH (NOLOCK)
                          INNER JOIN FAMSession WITH (NOLOCK) ON
                          FAMSession.ID = FileTaskSession.FAMSessionID
                          INNER JOIN ACTION ON
                          FileTaskSession.ActionID = ACTION.ID
                          INNER JOIN TaskClass ON
                          FileTaskSession.TaskClassID = TaskClass.ID
                       INNER JOIN FileActionStatus WITH (NOLOCK) ON FileActionStatus.FileID = FileTaskSession.FileID 
                       AND FileActionStatus.ActionID =  FileTaskSession.ActionID 
                       AND FileActionStatus.ActionStatus = 'C'
                     WHERE FileTaskSession.ID > @LastProcessedID
                           AND FileTaskSession.ID <= @LastInBatchID
                           AND [TaskClass].GUID IN (
                           '{Constants.TaskClassDocumentApi}',                          
                           '{Constants.TaskClassDataEntryVerification}',                           
                           '{Constants.TaskClassPaginationVerification}',
                           '{Constants.TaskClassAutoPaginate}'
                           )
                     AND FileTaskSession.DateTimeStamp IS NOT NULL
                     AND FileTaskSession.ActionID IS NOT NULL
                     AND (FileTaskSession.FileID NOT IN
                     (
                         SELECT SourceFileID
                         FROM Pagination WITH (NOLOCK)
                         UNION
                         SELECT DestFileID
                         FROM Pagination WITH (NOLOCK)
                     ))
                     GROUP BY FAMSession.FAMUserID, 
                              FileTaskSession.FileID, 
                              FileTaskSession.ActionID, 
                              ACTION.ASCName),
                 DataToInsert
                 AS (SELECT PaginationID, 
                            FAMUserID, 
                            SourceFileID, 
                            DestFileID, 
                            OriginalFileID, 
                            DateTimeStamp, 
                            ActionID, 
                            FileTaskSessionID, 
                            ASCName
                     FROM PaginationDataWithRank
                     UNION
                     SELECT PaginationID, 
                            FAMUserID, 
                            SourceFileID, 
                            DestFileID, 
                            OriginalFileID, 
                            DateTimeStamp, 
                            ActionID, 
                            FileTaskSessionID, 
                            ASCName
                     FROM ProcessedOutsidePagination)
            
                 INSERT INTO ReportingHIMStats
                 SELECT DISTINCT 
                        DataToInsert.PaginationID, 
                        DataToInsert.FAMUserID, 
                        DataToInsert.SourceFileID, 
                        DataToInsert.DestFileID, 
                        DataToInsert.OriginalFileID, 
                        CAST([DateTimeStamp] AS DATE) AS [DateProcessed], 
                        DataToInsert.ActionID, 
                        DataToInsert.FileTaskSessionID, 
                        DataToInsert.ASCName
                 FROM DataToInsert;");

        #endregion

        #region Fields

        /// <summary>
        /// Current version
        /// </summary>
        const int CURRENT_VERSION = 1;

        /// <summary>
        /// Indicates whether the Process method is currently executing.
        /// </summary>
        bool _processing;

        /// <summary>
        /// The current status info for this service.
        /// </summary>
        HIMStatsStatus _status;

        #endregion Fields

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

        public int BatchSize { get; set; } = _PROCESS_BATCH_SIZE;

        #endregion DatabaseService Properties

        #region HIMStats Properties

        [DataMember]
        public override int Version { get; protected set; } = CURRENT_VERSION;
        #endregion

        #region IHasConfigurableDatabaseServiceStatus
       
        /// <summary>
        /// The <see cref="DatabaseServiceStatus"/> for this instance
        /// </summary>
        public DatabaseServiceStatus Status
        {
            get => _status = _status ?? GetLastOrCreateStatus(() => new HIMStatsStatus()
            {
                LastFileTaskSessionIDProcessed = -1
            });

            set => _status = value as HIMStatsStatus;
        }


        public void RefreshStatus()
        {
            try
            {
                if (DatabaseServiceID > 0
                    && !string.IsNullOrEmpty(DatabaseServer)
                    && !string.IsNullOrEmpty(DatabaseName))
                {
                    _status = GetLastOrCreateStatus(() => new HIMStatsStatus());
                }
                else
                {
                    _status = null;
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI46591");
            };
        }

        #endregion

        #region IConfigSettings implementation

        /// <summary>
        /// Method returns the state of the configuration
        /// </summary>
        /// <returns>Returns <see langword="true"/> if configuration is valid, otherwise false</returns>

        public bool IsConfigured()
        {
            return !string.IsNullOrWhiteSpace(Description);
        }

        /// <summary>
        /// Displays a form to Configures the HIMStats service
        /// </summary>
        /// <returns><see langword="true"/> if configuration was ok'd. if configuration was canceled returns 
        /// <see langword="false"/></returns>
        public bool Configure()
        {
            try
            {
                HIMStatsForm form = new HIMStatsForm(this);
                if (form.ShowDialog() == DialogResult.OK)
                {
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI46056");
                return false;
            }
        }

        #endregion

        #region DatabaseService Methods

        /// <summary>
        /// Performs the process of populating the ReportingHIMStats table in the database
        /// </summary>
        /// <param name="cancelToken">Token that can cancel the processing</param>
        public override void Process(CancellationToken cancelToken)
        {
            try
            {
                _processing = true;
                cancelToken.ThrowIfCancellationRequested();

                RefreshStatus();
                ExtractException.Assert("ELI46592", "Status cannot be null", _status != null);

                if (_status.LastFileTaskSessionIDProcessed != 0)
                    ClearReportingHIMStats(cancelToken);

                // Get the maximum File task session id available
                Int32 maxFileTaskSession = MaxReportableFileTaskSessionId(false);

                cancelToken.ThrowIfCancellationRequested();

                // Process the entries in chunks of 100 file task session
                while (_status.LastFileTaskSessionIDProcessed < maxFileTaskSession)
                {
                    cancelToken.ThrowIfCancellationRequested();

                    int lastInBatchToProcess = Math.Min(_status.LastFileTaskSessionIDProcessed + BatchSize, maxFileTaskSession);

                    using var connection = new ExtractRoleConnection(DatabaseServer, DatabaseName);
                    connection.Open();

                    using var cmd = connection.CreateCommand();
                    using var scope = new TransactionScope(
                        TransactionScopeOption.Required,
                        new TransactionOptions()
                        {
                            IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead,
                            Timeout = TransactionManager.MaximumTimeout,
                        },
                        TransactionScopeAsyncFlowOption.Enabled);


                    cmd.CommandText = _UpdateQuery;
                    cmd.CommandTimeout = 0;
                    cmd.Parameters.AddWithValue("@LastProcessedID", _status.LastFileTaskSessionIDProcessed);
                    cmd.Parameters.AddWithValue("@LastInBatchID", lastInBatchToProcess);
                    var task = cmd.ExecuteNonQueryAsync();
                    task.Wait(cancelToken);

                    _status.LastFileTaskSessionIDProcessed = lastInBatchToProcess;
                    _status.SaveStatus(connection, DatabaseServiceID);

                    scope.Complete();
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI46059");
            }
            finally
            {
                _processing = false;
            }
        }

        private void ClearReportingHIMStats(CancellationToken cancelToken)
        {
            if (_status.LastFileTaskSessionIDProcessed != 0) return;

            var connection = new ExtractRoleConnection(DatabaseServer, DatabaseName);
            connection.Open();


            using (var deleteCmd = connection.CreateCommand())
            using (var scope = new TransactionScope(
                TransactionScopeOption.Required,
                new TransactionOptions()
                {
                    IsolationLevel = System.Transactions.IsolationLevel.RepeatableRead,
                    Timeout = TransactionManager.MaximumTimeout,
                },
                TransactionScopeAsyncFlowOption.Enabled))
            {
                deleteCmd.CommandTimeout = 0;
                deleteCmd.CommandText = "DELETE FROM ReportingHIMStats";
                var task = deleteCmd.ExecuteNonQueryAsync();
                task.Wait(cancelToken);
                scope.Complete();
            }
        }

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
                ExtractException ee = new ExtractException("ELI46058", "Settings were saved with a newer version.");
                ee.AddDebugData("SavedVersion", Version, false);
                ee.AddDebugData("CurrentVersion", CURRENT_VERSION, false);
                throw ee;
            }

            Version = CURRENT_VERSION;
        }

        #endregion

    }
}
