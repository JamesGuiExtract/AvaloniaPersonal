using Extract.Code.Attributes;
using Extract.SqlDatabase;
using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
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
        const int CURRENT_VERSION = 1;
        DatabaseCleanupStatus _status;
        bool _processing;
        private ExtractRoleConnection connection;
        private readonly int batchSize = 10_000;

        [DataMember]
        public int PurgeRecordsOlderThanDays { get; set; } = 365;

        [DataMember]
        public int MaxDaysToProcessPerRun { get; set; } = 30;

        #region SQLQueries
        private readonly string CalculateRowsToDeleteQuery = @"
WITH MostRecentAttributeSets AS(
	SELECT
		dbo.AttributeSetForFile.AttributeSetNameID
		, dbo.FileTaskSession.DateTimeStamp
		, dbo.FileTaskSession.FileID
		, dbo.AttributeSetForFile.ID
		, ROW_NUMBER() OVER (PARTITION BY dbo.FileTaskSession.FileID, dbo.AttributeSetForFile.AttributeSetNameID Order by dbo.FileTaskSession.DateTimeStamp DESC) AS RowNumber
	FROM
		dbo.FileTaskSession
			INNER JOIN dbo.AttributeSetForFile
				ON dbo.AttributeSetForFile.FileTaskSessionID = dbo.FileTaskSession.ID
)

SELECT
	'AttributeSetForFile'
	, COUNT(*)
FROM
	dbo.AttributeSetForFile
WHERE
	dbo.AttributeSetForFile.ID IN (SELECT MostRecentAttributeSets.ID FROM MostRecentAttributeSets WHERE RowNumber > 1  AND MostRecentAttributeSets.DateTimeStamp < @Date) 

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

        private readonly string CalculateFurthestDateToProcessTo =
@"
SELECT
	MAX(ID) AS FileTaskSessionID
FROM
	dbo.FileTaskSession
WHERE
    ((SELECT DATEADD(DAY, @MaxDaysToProcessPerRun, DateTimeStamp) FROM dbo.FileTaskSession WHERE ID = @LastFileTaskSessionProcess) IS NULL
    OR
	dbo.FileTaskSession.DateTimeStamp < (SELECT DATEADD(DAY, @MaxDaysToProcessPerRun, DateTimeStamp) FROM dbo.FileTaskSession WHERE ID = @LastFileTaskSessionProcess))
	AND
	dbo.FileTaskSession.DateTimeStamp < @MaxDateToProcessTo";

        private readonly Dictionary<string,string> TableDeletionQueries = new(){
{"QueueEvent", @"
DELETE FROM
	dbo.QueueEvent
WHERE
	dbo.QueueEvent.DateTimeStamp < @Date;

SELECT @@ROWCOUNT"},
{"SourceDocChangeHistory", @"
DELETE FROM
		dbo.SourceDocChangeHistory
WHERE
		dbo.SourceDocChangeHistory.[TimeStamp] < @Date;

SELECT @@ROWCOUNT" },
{"LabDEOrder", @"
DELETE FROM
		dbo.LabDEOrder
WHERE
		dbo.LabDEOrder.ReceivedDateTime < @Date;

SELECT @@ROWCOUNT" },
{"LabDEEncounter", @"
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
{"FileActionStateTransition", @"
DELETE FROM
		dbo.FileActionStateTransition
WHERE
		dbo.FileActionStateTransition.DateTimeStamp < @Date;

SELECT @@ROWCOUNT" },
{"Attribute", @"
ALTER TABLE dbo.Attribute NOCHECK CONSTRAINT ALL;
DELETE 
    dbo.Attribute
FROM
    dbo.Attribute a
        INNER JOIN dbo.AttributeSetForFile 
		    ON a.AttributeSetForFileID = dbo.AttributeSetForFile.ID
										  
			INNER JOIN dbo.FileTaskSession 
			    ON dbo.FileTaskSession.ID = dbo.AttributeSetForFile.FileTaskSessionID
                AND dbo.FileTaskSession.DateTimeStamp < @Date;

SELECT @@ROWCOUNT
ALTER TABLE dbo.Attribute WITH CHECK CHECK CONSTRAINT ALL;" },
{"AttributeSetForFile", @"
WITH MostRecentAttributeSets AS(
	SELECT
		dbo.AttributeSetForFile.AttributeSetNameID
		, dbo.FileTaskSession.DateTimeStamp
		, dbo.FileTaskSession.FileID
		, dbo.AttributeSetForFile.ID
		, ROW_NUMBER() OVER (PARTITION BY dbo.FileTaskSession.FileID, dbo.AttributeSetForFile.AttributeSetNameID Order by dbo.FileTaskSession.DateTimeStamp DESC) AS RowNumber
	FROM
		dbo.FileTaskSession
			INNER JOIN dbo.AttributeSetForFile
				ON dbo.AttributeSetForFile.FileTaskSessionID = dbo.FileTaskSession.ID
)

DELETE FROM
	dbo.AttributeSetForFile
WHERE
	dbo.AttributeSetForFile.ID IN (SELECT MostRecentAttributeSets.ID FROM MostRecentAttributeSets WHERE RowNumber > 1 AND MostRecentAttributeSets.DateTimeStamp < @Date);

SELECT @@ROWCOUNT"} };
        #endregion SQLQueries

        /// <summary>
        /// The <see cref="DatabaseServiceStatus"/> for this instance
        /// </summary>
        public DatabaseServiceStatus Status
        {
            get => _status ??= GetLastOrCreateStatus(() => new DatabaseCleanupStatus(){});

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
        public override bool Processing
        {
            get
            {
                return _processing;
            }
        }

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
                Task.Run(() => BeginProcessing(cancelToken), cancelToken).Wait();
            }
            catch(Exception ex)
            {
                var ee = ex.AsExtract("ELI51951");
                if (ex.Message.Equals("A task was canceled."))
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

        private void BeginProcessing(CancellationToken cancelToken)
        {
            this.connection = new ExtractRoleConnection(DatabaseServer, DatabaseName);
            connection.Open();
            this.RefreshStatus();
            DateTime start = DateTime.Now;
            Dictionary<string,string> rowsDeletedFromTables = new();
            Collection<ExtractException> exceptions = new();

            var startingFileTaskSessionID = ((DatabaseCleanupStatus)this.Status).LastFileTaskSessionIDProcessed;
            int maxFileTaskSessionIDtoProcess = GetMaxFileTaskSessionToProcessTo();

            for (int i = Math.Max(startingFileTaskSessionID, 0); i < maxFileTaskSessionIDtoProcess; i += batchSize)
            {
                var NextDateToProcessTo = GetFileTaskSessionDateFromID(Math.Min(maxFileTaskSessionIDtoProcess, i + batchSize));
                
                var result = ExecuteDeleteQueries(NextDateToProcessTo, cancelToken);
                rowsDeletedFromTables.AddRange(result.rowsDeletedFromTables);
                exceptions.Concat(result.exceptions);

                ((DatabaseCleanupStatus)this.Status).LastFileTaskSessionIDProcessed = Math.Min(i + batchSize, maxFileTaskSessionIDtoProcess);
                this.Status.SaveStatus(connection, this.DatabaseServiceID);
            }

            LogRuntimeInformation(rowsDeletedFromTables, start, startingFileTaskSessionID);

            if (exceptions.Count > 0)
            {
                throw ExtractException.AsAggregateException(exceptions);
            }
            connection.Close();
            connection.Dispose();
        }

        private (Dictionary<string,string> rowsDeletedFromTables, Collection<ExtractException> exceptions) ExecuteDeleteQueries(DateTime nextDateToProcessTo, CancellationToken cancelToken)
        {
            Dictionary<string, string> rowsDeletedFromTables = new();
            Collection<ExtractException> exceptions = new();
            foreach (var query in TableDeletionQueries)
            {
                try
                {
                    using var cmd = connection.CreateCommand();
                    cmd.CommandTimeout = 0;
                    cmd.Parameters.AddWithValue("@Date", nextDateToProcessTo);
                    cmd.CommandText = query.Value;

                    var reader = cmd.ExecuteScalarAsync(cancelToken);
                    reader.Wait();
                    rowsDeletedFromTables.Add(query.Key, reader.Result.ToString());
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex.AsExtract("ELI51954"));
                }
            }
            return (rowsDeletedFromTables, exceptions);
        }

        private void LogRuntimeInformation(Dictionary<string,string> rowsToDeleteFromTables, DateTime start, int startingFileTaskSessionID)
        {
            if (rowsToDeleteFromTables.Count > 0)
            {
                ExtractException ee = new("ELI53014", "Application Trace: The database cleanup service finished");
                foreach (var row in rowsToDeleteFromTables)
                {
                    ee.AddDebugData(row.Key + " rows deleted", row.Value);
                }

                ee.AddDebugData("RuntimeInMinutes", Math.Round((DateTime.Now - start).TotalMinutes));
                ee.AddDebugData("Start processing Date", GetFileTaskSessionDateFromID(Math.Max(1,startingFileTaskSessionID)));
                ee.AddDebugData("End processing Date", GetFileTaskSessionDateFromID(Math.Max(1,((DatabaseCleanupStatus)this.Status).LastFileTaskSessionIDProcessed)));
                ee.ExtractLog("ELI53015");
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

        private DateTime GetFileTaskSessionDateFromID(int fileTaskSessionIDToProcess)
        {
            using var cmd = connection.CreateCommand();
            cmd.Parameters.AddWithValue("@FileTaskSessionIDToProcess", fileTaskSessionIDToProcess);
            cmd.CommandText = "SELECT [DateTimeStamp] FROM [FileTaskSession] WHERE [ID] = @FileTaskSessionIDToProcess";
            return (DateTime)cmd.ExecuteScalar();
        }

        private int GetMaxFileTaskSessionToProcessTo()
        {
            using var cmd = connection.CreateCommand();
            
            cmd.Parameters.Add(new SqlParameter("@MaxDateToProcessTo", System.Data.SqlDbType.DateTime) { Value = DateTime.Today.AddDays(-1 * PurgeRecordsOlderThanDays) });
            cmd.Parameters.Add(new SqlParameter("@MaxDaysToProcessPerRun", System.Data.SqlDbType.Int) { Value = this.MaxDaysToProcessPerRun });
            cmd.Parameters.Add(new SqlParameter("@LastFileTaskSessionProcess", System.Data.SqlDbType.Int) { Value = ((DatabaseCleanupStatus)this.Status).LastFileTaskSessionIDProcessed });
            cmd.CommandText = CalculateFurthestDateToProcessTo;

            return (int)cmd.ExecuteScalar();
        }

        public void CalculateNumberOfRowsToDelete(int purgeRecordsOlderThanDays)
        {
            try
            {
                using var connection = new ExtractRoleConnection(DatabaseServer, DatabaseName);
                connection.Open();

                using var cmd = connection.CreateCommand();
                cmd.CommandTimeout = 0;
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
            catch(Exception ex)
            {
                throw ex.AsExtract("ELI51947");
            }
        }
    }

    class DatabaseCleanupStatus : DatabaseServiceStatus, IFileTaskSessionServiceStatus
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
