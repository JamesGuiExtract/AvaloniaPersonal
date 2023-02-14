using Extract.Code.Attributes;
using Extract.SqlDatabase;
using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Globalization;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Extract.ETL
{
    [DataContract]
    [KnownType(typeof(ScheduledEvent))]
    [ExtractCategory("DatabaseService", "Database cleanup")]
    public class DatabaseCleanup : DatabaseService, IConfigSettings, IHasConfigurableDatabaseServiceStatus
    {
        const int CURRENT_VERSION = 2;
        DatabaseCleanupStatus _status;
        bool _processing;

        public Dictionary<string, int> RowDeletedFromTables { get; } = new();
        public Collection<ExtractException> Exceptions { get; } = new();

        [DataMember]
        public int PurgeRecordsOlderThanDays { get; set; } = 425;

        [DataMember]
        public int MaxDaysToProcessPerRun { get; set; } = 30;

        #region SQLQueries
        private readonly string CalculateRowsToDeleteQuery = @"
SELECT
	'AttributeSetForFile'
	, COUNT(*)
FROM
	dbo.FileTaskSession
		INNER JOIN dbo.AttributeSetForFile
			ON dbo.AttributeSetForFile.FileTaskSessionID = dbo.FileTaskSession.ID
WHERE
	dbo.FileTaskSession.DateTimeStamp < @Date


UNION

SELECT
	'DashboardAttributeFields'
	, COUNT(*)
FROM
	dbo.DashboardAttributeFields

        INNER JOIN dbo.AttributeSetForFile
            ON dbo.DashboardAttributeFields.AttributeSetForFileID = dbo.AttributeSetForFile.ID
            
            INNER JOIN dbo.FileTaskSession
                ON dbo.AttributeSetForFile.FileTaskSessionID = dbo.FileTaskSession.ID
                AND dbo.FileTaskSession.DateTimeStamp < @Date
UNION
	
SELECT
	'ReportingRedactionAccuracy'
	, COUNT(*)
FROM
	dbo.ReportingRedactionAccuracy

        INNER JOIN dbo.AttributeSetForFile
            ON (dbo.ReportingRedactionAccuracy.ExpectedAttributeSetForFileID = dbo.AttributeSetForFile.ID
                OR dbo.ReportingRedactionAccuracy.FoundAttributeSetForFileID = dbo.AttributeSetForFile.ID)
            
            INNER JOIN dbo.FileTaskSession
                ON dbo.AttributeSetForFile.FileTaskSessionID = dbo.FileTaskSession.ID
                AND dbo.FileTaskSession.DateTimeStamp < @Date
	
UNION
	
SELECT
	'ReportingDataCaptureAccuracy'
	, COUNT(*)
FROM
	dbo.ReportingDataCaptureAccuracy

        INNER JOIN dbo.AttributeSetForFile
            ON (dbo.ReportingDataCaptureAccuracy.ExpectedAttributeSetForFileID = dbo.AttributeSetForFile.ID
                OR dbo.ReportingDataCaptureAccuracy.FoundAttributeSetForFileID = dbo.AttributeSetForFile.ID)
            
            INNER JOIN dbo.FileTaskSession
                ON dbo.AttributeSetForFile.FileTaskSessionID = dbo.FileTaskSession.ID
                AND dbo.FileTaskSession.DateTimeStamp < @Date

UNION

SELECT
	'QueueEvent'
	, COUNT(*)
FROM
	dbo.QueueEvent
WHERE
	dbo.QueueEvent.DateTimeStamp < @Date

UNION

SELECT
	'SourceDocChangeHistory'
	, COUNT(*)
FROM
		dbo.SourceDocChangeHistory
WHERE
		dbo.SourceDocChangeHistory.[TimeStamp] < @Date

UNION

SELECT
	'FileActionStateTransition'
	, COUNT(*)
FROM
		dbo.FileActionStateTransition
WHERE
		dbo.FileActionStateTransition.DateTimeStamp < @Date

UNION

SELECT
	'LabDEOrder'
	, COUNT(*)
FROM
		dbo.LabDEOrder
WHERE
		dbo.LabDEOrder.ReceivedDateTime < @Date

UNION

SELECT
	'LabDEEncounter'
	, COUNT(DISTINCT dbo.LabDEEncounter.CSN)
	
FROM
		dbo.LabDEEncounter
			LEFT OUTER JOIN dbo.LabDEOrder
				ON dbo.LabDEOrder.EncounterID = dbo.LabDEEncounter.CSN
WHERE
	dbo.LabDEEncounter.EncounterDateTime < @Date
	AND
	dbo.LabDEOrder.EncounterID IS NULL

UNION

SELECT 
    'Attribute'
    , COUNT(DISTINCT dbo.Attribute.ID)
FROM
    dbo.Attribute
        INNER JOIN dbo.AttributeSetForFile 
		    ON dbo.Attribute.AttributeSetForFileID = dbo.AttributeSetForFile.ID
										  
			INNER JOIN dbo.FileTaskSession 
			    ON dbo.FileTaskSession.ID = dbo.AttributeSetForFile.FileTaskSessionID
                AND dbo.FileTaskSession.DateTimeStamp < @Date
";

        private readonly string CalculateFurthestIDToProcessTo =
@"
SELECT
	MAX(ID) AS FileTaskSessionID
FROM
	dbo.FileTaskSession
WHERE
    (
		dbo.FileTaskSession.DateTimeStamp < (SELECT DATEADD(DAY, @MaxDaysToProcessPerRun, DateTimeStamp) FROM dbo.FileTaskSession WHERE ID = @LastFileTaskSessionIDProcessed)
		OR
		(
			@LastFileTaskSessionIDProcessed = -1
			AND
			dbo.FileTaskSession.DateTimeStamp < (SELECT TOP(1) DATEADD(DAY, @MaxDaysToProcessPerRun, DateTimeStamp) FROM dbo.FileTaskSession ORDER BY dbo.FileTaskSession.ID)
		)
	)
	AND
	dbo.FileTaskSession.DateTimeStamp < @MaxDateToProcessTo";

        private readonly Dictionary<string, string> TableDeletionQueries = new()
        {
            { "QueueEvent", @"
DELETE FROM
	dbo.QueueEvent
WHERE
	dbo.QueueEvent.DateTimeStamp < @Date;

SELECT @@ROWCOUNT" },
            { "SourceDocChangeHistory", @"
DELETE FROM
		dbo.SourceDocChangeHistory
WHERE
		dbo.SourceDocChangeHistory.[TimeStamp] < @Date;

SELECT @@ROWCOUNT" },
            { "LabDEOrder", @"
DELETE FROM
		dbo.LabDEOrder
WHERE
		dbo.LabDEOrder.ReceivedDateTime < @Date;

SELECT @@ROWCOUNT" },
            { "LabDEEncounter", @"
DELETE
		dbo.LabDEEncounter
FROM
		dbo.LabDEEncounter encounter
			LEFT OUTER JOIN dbo.LabDEOrder
				ON dbo.LabDEOrder.EncounterID = encounter.CSN
WHERE
	encounter.EncounterDateTime < @Date
	AND
	dbo.LabDEOrder.EncounterID IS NULL;

SELECT @@ROWCOUNT" },
            { "FileActionStateTransition", @"
DELETE FROM
		dbo.FileActionStateTransition
WHERE
		dbo.FileActionStateTransition.DateTimeStamp < @Date;

SELECT @@ROWCOUNT" },
            { "Attribute", @"
DELETE 
    dbo.Attribute
FROM
    dbo.Attribute a
        INNER JOIN dbo.AttributeSetForFile 
		    ON a.AttributeSetForFileID = dbo.AttributeSetForFile.ID
										  
			INNER JOIN dbo.FileTaskSession 
			    ON dbo.FileTaskSession.ID = dbo.AttributeSetForFile.FileTaskSessionID
                AND dbo.FileTaskSession.DateTimeStamp < @Date;

SELECT @@ROWCOUNT" },
            { "ReportingDataCaptureAccuracy", @"
DELETE 
	dbo.ReportingDataCaptureAccuracy 
FROM 
	dbo.ReportingDataCaptureAccuracy

        INNER JOIN dbo.AttributeSetForFile
            ON (dbo.ReportingDataCaptureAccuracy.ExpectedAttributeSetForFileID = dbo.AttributeSetForFile.ID
                OR dbo.ReportingDataCaptureAccuracy.FoundAttributeSetForFileID = dbo.AttributeSetForFile.ID)
            
            INNER JOIN dbo.FileTaskSession
                ON dbo.AttributeSetForFile.FileTaskSessionID = dbo.FileTaskSession.ID
                AND dbo.FileTaskSession.DateTimeStamp < @Date;

SELECT @@ROWCOUNT" },
            { "ReportingRedactionAccuracy", @"
DELETE 
	dbo.ReportingRedactionAccuracy 
FROM 
	dbo.ReportingRedactionAccuracy

        INNER JOIN dbo.AttributeSetForFile
            ON (dbo.ReportingRedactionAccuracy.ExpectedAttributeSetForFileID = dbo.AttributeSetForFile.ID
                OR dbo.ReportingRedactionAccuracy.FoundAttributeSetForFileID = dbo.AttributeSetForFile.ID)
            
            INNER JOIN dbo.FileTaskSession
                ON dbo.AttributeSetForFile.FileTaskSessionID = dbo.FileTaskSession.ID
                AND dbo.FileTaskSession.DateTimeStamp < @Date;

SELECT @@ROWCOUNT" },
            { "DashboardAttributeFields", @"
DELETE 
	dbo.DashboardAttributeFields 
FROM 
	dbo.DashboardAttributeFields

        INNER JOIN dbo.AttributeSetForFile
            ON dbo.DashboardAttributeFields.AttributeSetForFileID = dbo.AttributeSetForFile.ID
            
            INNER JOIN dbo.FileTaskSession
                ON dbo.AttributeSetForFile.FileTaskSessionID = dbo.FileTaskSession.ID
                AND dbo.FileTaskSession.DateTimeStamp < @Date;

SELECT @@ROWCOUNT" },
            { "AttributeSetForFile", @"
DELETE 
	dbo.AttributeSetForFile 
FROM 
	dbo.FileTaskSession
		INNER JOIN dbo.AttributeSetForFile
			ON dbo.AttributeSetForFile.FileTaskSessionID = dbo.FileTaskSession.ID
WHERE
	dbo.FileTaskSession.DateTimeStamp < @Date;

SELECT @@ROWCOUNT" }
        };
        #endregion SQLQueries

        /// <summary>
        /// The <see cref="DatabaseServiceStatus"/> for this instance
        /// </summary>
        public DatabaseServiceStatus Status
        {
            get => _status ??= GetLastOrCreateStatus(() => new DatabaseCleanupStatus());

            set => _status = value as DatabaseCleanupStatus;
        }

        public DatabaseCleanup()
        {
            Schedule = new ScheduledEvent
            {
                Start = DateTime.Today
            };
        }

        /// <summary>
        /// Gets a value indicating whether this instance is processing.
        /// </summary>
        /// <value>
        ///   <c>true</c> if processing; otherwise, <c>false</c>.
        /// </value>
        public override bool Processing => _processing;

        [DataMember]
        public override int Version { get; protected set; } = CURRENT_VERSION;

        public bool Configure()
        {
            try
            {
                DatabaseCleanupForm captureAccuracyForm = new(this);
                return captureAccuracyForm.ShowDialog() == DialogResult.OK;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI51944");
            }
            return false;
        }

        public bool IsConfigured()
        {
            return true;
        }

        public override void Process(CancellationToken cancelToken)
        {
            try
            {
                _processing = true;
                BeginProcessing(cancelToken)
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();
            }
            catch (Exception ex)
            {
                var ee = ex.AsExtract("ELI51951");
                if (cancelToken.IsCancellationRequested)
                {
                    ee.AddDebugData("Info", "The service was canceled.");
                }
                throw ee;
            }
            finally
            {
                _processing = false;
            }
        }

        private async Task BeginProcessing(CancellationToken cancelToken)
        {

            new ExtractException("ELI53983", "Application Trace: The database cleanup service has started.").Log();
            this.Exceptions.Clear();
            this.RowDeletedFromTables.Clear();
            using ExtractRoleConnection connection = new(DatabaseServer, DatabaseName);
            connection.Open();
            try
            {
                this.RefreshStatus();
                DateTime start = DateTime.Now;

                var startingFileTaskSessionID = ((DatabaseCleanupStatus)this.Status).LastFileTaskSessionIDProcessed;

                var startFileTaskSessionDate = startingFileTaskSessionID == -1 ? GetMinFileTaskSessionDate(connection) : GetFileTaskSessionDateFromID(startingFileTaskSessionID, connection);
                var maxFileTaskSessionIDToProcess = GetMaxFileTaskSessionToProcessTo(connection);
                if (maxFileTaskSessionIDToProcess == null || startFileTaskSessionDate == null)
                {
                    new ExtractException("ELI53985", "Application Trace: The database cleanup service has aborted." +
                        $"{(maxFileTaskSessionIDToProcess == null ? " The last file task session ID could not be determined." : string.Empty)}" +
                        $"{(startFileTaskSessionDate == null ? " The file task session start date could not be determined." : string.Empty)}").Log();
                    return;
                }

                var lastFileTaskSessionDate = GetFileTaskSessionDateFromID((int)maxFileTaskSessionIDToProcess, connection);

                await DeleteRowsOneDayAtATime((DateTime)startFileTaskSessionDate, lastFileTaskSessionDate, cancelToken, connection);

                LogRuntimeInformation(start, (DateTime)startFileTaskSessionDate, lastFileTaskSessionDate);

                if (Exceptions.Count > 0)
                {
                    throw ExtractException.AsAggregateException(Exceptions);
                }
            }
            finally
            {
                connection.Dispose();
            }
        }

        private async Task DeleteRowsOneDayAtATime(DateTime startFileTaskSessionDate, DateTime lastFileTaskSessionDate, CancellationToken cancelToken, ExtractRoleConnection connection)
        {
            for (DateTime indexDate = startFileTaskSessionDate.Date; indexDate < lastFileTaskSessionDate; indexDate = indexDate.AddDays(1))
            {
                cancelToken.ThrowIfCancellationRequested();

                await ExecuteDeleteQueries(indexDate, cancelToken, connection);

                ((DatabaseCleanupStatus)this.Status).LastFileTaskSessionIDProcessed = GetLargestFileTaskSessionIDFromDate(indexDate, connection);
                this.Status.SaveStatus(connection, this.DatabaseServiceID);
                string cleanedUpDate = indexDate.Date.ToString("d", CultureInfo.InvariantCulture);
                new ExtractException("ELI53986", $"Application Trace: The database cleanup service has deleted rows for this date: {cleanedUpDate}").Log();
            }
        }

        private async Task ExecuteDeleteQueries(DateTime nextDateToProcessTo, CancellationToken cancelToken, ExtractRoleConnection connection)
        {
            foreach (var query in TableDeletionQueries)
            {
                cancelToken.ThrowIfCancellationRequested();
                try
                {
                    using var cmd = connection.CreateCommand();
                    cmd.CommandTimeout = 0;
                    // A single day needs to be addded for the casts in the query (bounds case).
                    cmd.Parameters.AddWithValue("@Date", nextDateToProcessTo.AddDays(1));
                    cmd.CommandText = query.Value;

                    var rowsDeleted = await cmd.ExecuteScalarAsync(cancelToken);
                    if (RowDeletedFromTables.ContainsKey(query.Key))
                    {
                        RowDeletedFromTables[query.Key] += (int)rowsDeleted;
                    }
                    else
                    {
                        RowDeletedFromTables.Add(query.Key, (int)rowsDeleted);
                    }
                }
                catch (Exception ex)
                {
                    Exceptions.Add(ex.AsExtract("ELI51954"));
                }
            }
        }

        private void LogRuntimeInformation(DateTime processingStarted, DateTime firstFileTaskSession, DateTime lastFileTaskSession)
        {
            ExtractException ee = new("ELI53014", "Application Trace: The database cleanup service finished");
            ee.AddDebugData("RuntimeInMinutes", Math.Round((DateTime.Now - processingStarted).TotalMinutes));
            ee.AddDebugData("Service ID", this.DatabaseServiceID);
            ee.AddDebugData("Purge records older than", this.PurgeRecordsOlderThanDays);
            ee.AddDebugData("Days to run", this.MaxDaysToProcessPerRun);

            if (RowDeletedFromTables.Count > 0)
            {
                foreach (var row in RowDeletedFromTables)
                {
                    ee.AddDebugData(row.Key + " rows deleted", row.Value);
                }

                ee.AddDebugData("Start file task session date", firstFileTaskSession);
                ee.AddDebugData("End file task session date", lastFileTaskSession);
                ee.ExtractLog("ELI53015");
            }
            else
            {
                ee.AddDebugData("No rows deleted", "");
                ee.ExtractLog("ELI53987");
            }
        }

        public void RefreshStatus()
        {
            try
            {
                if (DatabaseServiceID > 0
                    && !string.IsNullOrEmpty(DatabaseServer)
                    && !string.IsNullOrEmpty(DatabaseName))
                {
                    _status = GetLastOrCreateStatus(() => new DatabaseCleanupStatus());
                }
                else
                {
                    _status = null;
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI51946");
            }
        }

        private static DateTime GetFileTaskSessionDateFromID(int fileTaskSessionIDToProcess, ExtractRoleConnection connection)
        {
            using var cmd = connection.CreateCommand();
            cmd.Parameters.AddWithValue("@FileTaskSessionIDToProcess", fileTaskSessionIDToProcess);
            cmd.CommandText = "SELECT [DateTimeStamp] FROM [FileTaskSession] WHERE [ID] = @FileTaskSessionIDToProcess";
            var result = cmd.ExecuteScalar();
            if (result == null)
            {
                throw new ExtractException("ELI53100", "Invalid file task session ID");
            }
            return (DateTime)result;
        }

        private static DateTime? GetMinFileTaskSessionDate(ExtractRoleConnection connection)
        {
            using var cmd = connection.CreateCommand();
            cmd.CommandText = "SELECT MIN([DateTimeStamp]) FROM [FileTaskSession]";
            var result = cmd.ExecuteScalar();
            return result as DateTime?;
        }

        private static int GetLargestFileTaskSessionIDFromDate(DateTime date, ExtractRoleConnection connection)
        {
            // Strip the time from the datetime, and add one day for the query.
            date = date.Date.AddDays(1);
            using var cmd = connection.CreateCommand();
            cmd.Parameters.AddWithValue("@Date", date);
            cmd.CommandText = "SELECT MAX(ID) FROM [FileTaskSession] WHERE DateTimeStamp < @Date";
            var result = cmd.ExecuteScalar();
            return (int)result;
        }

        private int? GetMaxFileTaskSessionToProcessTo(ExtractRoleConnection connection)
        {
            using var cmd = connection.CreateCommand();

            cmd.Parameters.Add(new SqlParameter("@MaxDateToProcessTo", System.Data.SqlDbType.DateTime) { Value = DateTime.Today.AddDays(-1 * PurgeRecordsOlderThanDays) });
            cmd.Parameters.Add(new SqlParameter("@MaxDaysToProcessPerRun", System.Data.SqlDbType.Int) { Value = this.MaxDaysToProcessPerRun });
            cmd.Parameters.Add(new SqlParameter("@LastFileTaskSessionIDProcessed", System.Data.SqlDbType.Int) { Value = ((DatabaseCleanupStatus)this.Status).LastFileTaskSessionIDProcessed });
            cmd.CommandText = CalculateFurthestIDToProcessTo;
            var result = cmd.ExecuteScalar();
            return result as int?;
        }

        public void CalculateNumberOfRowsToDelete(int purgeRecordsOlderThanDays)
        {
            try
            {
                using var connection = new ExtractRoleConnection(DatabaseServer, DatabaseName);
                connection.Open();

                using var cmd = connection.CreateCommand();
                cmd.CommandTimeout = 500;
                cmd.CommandText = CalculateRowsToDeleteQuery;
                cmd.Parameters.AddWithValue("@Date", DateTime.Today.AddDays(-purgeRecordsOlderThanDays));

                using SqlDataReader reader = cmd.ExecuteReader();

                string message = "The total number of records that will be deleted (may require several runs):\n";
                while (reader.Read())
                {
                    message += reader.GetString(0) + ": " + reader.GetInt32(1).ToString(CultureInfo.InvariantCulture) + "\n";
                }

                UtilityMethods.ShowMessageBox(message, "Table cleanup stats.", false);
            }
            catch (SqlException)
            {

            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI51947");
            }
        }
    }

    public class DatabaseCleanupStatus : DatabaseServiceStatus, IFileTaskSessionServiceStatus
    {
        const int _CURRENT_VERSION = 1;

        [DataMember]
        public override int Version { get; protected set; } = _CURRENT_VERSION;

        /// <summary>
        /// Contains the last file task session ID processed.
        /// </summary>
        public int LastFileTaskSessionIDProcessed { get; set; } = -1;

        /// <summary>
        /// Called after this instance is deserialized.
        /// </summary>
        [OnDeserialized]
        void OnDeserialized(StreamingContext context)
        {
            if (Version > _CURRENT_VERSION)
            {
                ExtractException ee = new("ELI51910", "Settings were saved with a newer version.");
                ee.AddDebugData("SavedVersion", Version, false);
                ee.AddDebugData("CurrentVersion", _CURRENT_VERSION, false);
                throw ee;
            }

            Version = _CURRENT_VERSION;
        }
    }
}
