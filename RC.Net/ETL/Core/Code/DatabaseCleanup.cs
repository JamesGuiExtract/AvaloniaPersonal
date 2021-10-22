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
        const int CURRENT_VERSION = 1;
        DatabaseCleanupStatus _status;
        bool _processing;

        private readonly int batchSize = 10_000;

        [DataMember]
        public int PurgeRecordsOlderThanDays { get; set; } = 365;

        [DataMember]
        public int MaximumNumberOfRecordsToProcessFromFileTaskSession { get; set; } = 50_000;

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

        private readonly string CalculateMostRecentFileTaskSessionID =
@"
SELECT
	MAX(ID) AS FileTaskSessionID
FROM
	dbo.FileTaskSession
WHERE
	dbo.FileTaskSession.ID <= (@LastFileTaskSessionProcess + @MaximumNumberOfRecordsToProcessFromFileTaskSession)
	AND
	dbo.FileTaskSession.DateTimeStamp < @Date";

        private readonly string[] TableDeletionQueries = {
@"
DELETE FROM
	dbo.QueueEvent
WHERE
	dbo.QueueEvent.DateTimeStamp < @Date;",
@"
DELETE FROM
		dbo.SourceDocChangeHistory
WHERE
		dbo.SourceDocChangeHistory.[TimeStamp] < @Date;",
@"
DELETE FROM
		dbo.LabDEOrder
WHERE
		dbo.LabDEOrder.ReceivedDateTime < @Date;",
@"
DELETE
		dbo.LabDEEncounter
FROM
		dbo.LabDEEncounter encounter
			LEFT OUTER JOIN dbo.LabDEOrder
				ON dbo.LabDEOrder.EncounterID = encounter.CSN
WHERE
	encounter.EncounterDateTime < @Date
	AND
	dbo.LabDEOrder.EncounterID IS NULL;",
@"
DELETE FROM
		dbo.FileActionStateTransition
WHERE
		dbo.FileActionStateTransition.DateTimeStamp < @Date;",
@"
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

ALTER TABLE dbo.Attribute WITH CHECK CHECK CONSTRAINT ALL;",
@"
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
	dbo.AttributeSetForFile.ID IN (SELECT MostRecentAttributeSets.ID FROM MostRecentAttributeSets WHERE RowNumber > 1 AND MostRecentAttributeSets.DateTimeStamp < @Date);"};
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
                Task.Run(() =>
                {
                    Collection<ExtractException> exceptions = new();
                    using var connection = new ExtractRoleConnection(DatabaseServer, DatabaseName);
                    connection.Open();

                    this.RefreshStatus();
                    var startingFileTaskSessionID = ((DatabaseCleanupStatus)this.Status).LastFileTaskSessionIDProcessed;
                    var MaxFileTaskSessionIDToProcess = GetLastFileTaskSessionID(connection);

                    for(int i = Math.Max(startingFileTaskSessionID,0); 
                    i < MaxFileTaskSessionIDToProcess && i - startingFileTaskSessionID <= MaximumNumberOfRecordsToProcessFromFileTaskSession;
                    i+= batchSize)
                    {
                        var NextDateToProcessTo = GetNextFileTaskSessionDate(connection, Math.Min(MaxFileTaskSessionIDToProcess,i + batchSize));
                        foreach (var query in TableDeletionQueries)
                        {
                            try
                            {
                                using var cmd = connection.CreateCommand();
                                cmd.CommandTimeout = 0;
                                cmd.Parameters.AddWithValue("@Date", NextDateToProcessTo);
                                cmd.CommandText = query;
                                cmd.ExecuteNonQueryAsync(cancelToken).Wait();
                            }
                            catch (Exception ex)
                            {
                                exceptions.Add(ex.AsExtract("ELI51954"));
                            }
                        }
                        ((DatabaseCleanupStatus)this.Status).LastFileTaskSessionIDProcessed = Math.Min(i + batchSize, MaxFileTaskSessionIDToProcess);
                        this.Status.SaveStatus(connection, this.DatabaseServiceID);
                    }

                    if (exceptions.Count > 0)
                    {
                        throw ExtractException.AsAggregateException(exceptions);
                    }

                }, cancelToken).Wait();
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

        private static DateTime GetNextFileTaskSessionDate(ExtractRoleConnection connection, int fileTaskSessionIDToProcess)
        {
            using var cmd = connection.CreateCommand();
            cmd.Parameters.AddWithValue("@FileTaskSessionIDToProcess", fileTaskSessionIDToProcess);
            cmd.CommandText = "SELECT [DateTimeStamp] FROM [FileTaskSession] WHERE [ID] = @FileTaskSessionIDToProcess";

            return (DateTime)cmd.ExecuteScalar();
        }

        private int GetLastFileTaskSessionID(ExtractRoleConnection connection)
        {
            using var cmd = connection.CreateCommand();
            cmd.Parameters.AddWithValue("@Date", DateTime.Today.AddDays(-1 * PurgeRecordsOlderThanDays));
            cmd.Parameters.AddWithValue("@MaximumNumberOfRecordsToProcessFromFileTaskSession", this.MaximumNumberOfRecordsToProcessFromFileTaskSession);
            cmd.Parameters.AddWithValue("@LastFileTaskSessionProcess", Math.Max(0,((DatabaseCleanupStatus)this.Status).LastFileTaskSessionIDProcessed));
            cmd.CommandText = CalculateMostRecentFileTaskSessionID;

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
