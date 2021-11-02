using Extract.Code.Attributes;
using Extract.SqlDatabase;
using Extract.Utilities;
using System;
using System.Data.SqlClient;
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

        [DataMember]
        public int PurgeRecordsOlderThanDays { get; set; } = 120;

        [DataMember]
        public int MaxFilesToSelect { get; set; } = 1000;

        private readonly string _deleteRecordsQuery = @"
BEGIN TRANSACTION [Transaction];
BEGIN TRY
		DECLARE @FileIDsToRemove TABLE (FileID INT)
		DECLARE @AttributesToRemove TABLE (AttributeID INT)

		INSERT INTO 
			@FileIDsToRemove (FileID) 
		SELECT TOP (@MaxFilesToSelect)
			MaxDateTimeForFile.FileID
		FROM
			(
				SELECT
					dbo.FileActionStateTransition.FileID
					, MAX(dbo.FileActionStateTransition.DateTimeStamp) AS DateTime
				FROM
					dbo.FileActionStateTransition
				GROUP BY
					dbo.FileActionStateTransition.FileID
			) AS MaxDateTimeForFile
		WHERE
			MaxDateTimeForFile.DateTime < CAST(@Date AS DATE)

		INSERT INTO 
			@AttributesToRemove
		SELECT
			dbo.Attribute.ID
		FROM
			@FileIDsToRemove AS FileIDsToRemove 
				LEFT OUTER JOIN dbo.FileTaskSession
					ON dbo.FileTaskSession.FileID = FileIDsToRemove.FileID

					LEFT OUTER JOIN dbo.AttributeSetForFile
						ON dbo.AttributeSetForFile.FileTaskSessionID = dbo.FileTaskSession.ID

						LEFT OUTER JOIN dbo.Attribute
							ON dbo.Attribute.AttributeSetForFileID = dbo.AttributeSetForFile.ID

		DELETE FROM 
			dbo.FileActionStateTransition
		WHERE
			dbo.FileActionStateTransition.FileID IN (SELECT FileID FROM @FileIDsToRemove);

		DELETE FROM
			dbo.QueueEvent
		WHERE
			dbo.QueueEvent.FileID IN (SELECT FileID FROM @FileIDsToRemove);

		DELETE FROM
			dbo.SourceDocChangeHistory
		WHERE
			dbo.SourceDocChangeHistory.FileID IN (SELECT FileID FROM @FileIDsToRemove);

		DELETE FROM 
			dbo.Attribute
		WHERE
			dbo.Attribute.ID IN (SELECT AttributeID FROM @AttributesToRemove);

		COMMIT TRANSACTION[Transaction];
END TRY
BEGIN CATCH
	ROLLBACK TRANSACTION[Transaction]
END CATCH
";

        /// <summary>
        /// The <see cref="DatabaseServiceStatus"/> for this instance
        /// </summary>
        public DatabaseServiceStatus Status
        {
            get => _status ??= GetLastOrCreateStatus(() => new DatabaseCleanupStatus()
            {
                LastFileTaskSessionIDProcessed = -1
            });

            set => _status = value as DatabaseCleanupStatus;
        }

        public DatabaseCleanup()
        {
            this.Schedule = new ScheduledEvent();
            this.Schedule.Start = DateTime.Today;
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
                DatabaseCleanupForm captureAccuracyForm = new DatabaseCleanupForm(this);
                return captureAccuracyForm.ShowDialog() == DialogResult.OK;
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI45657");
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

                    using var connection = new ExtractRoleConnection(DatabaseServer, DatabaseName);
                    connection.Open();
                    
                    using var cmd = connection.CreateCommand();
                    cmd.CommandTimeout = 0;
                    cmd.CommandText = _deleteRecordsQuery;
                    cmd.Parameters.AddWithValue("@Date", DateTime.Today.AddDays(-1 * this.PurgeRecordsOlderThanDays));
                    cmd.Parameters.AddWithValue("@MaxFilesToSelect", this.MaxFilesToSelect);
                    cmd.ExecuteNonQueryAsync(cancelToken).Wait();
                }, cancelToken).Wait();
            }
            catch(Exception ex)
            {
                throw ex.AsExtract("ELI51914");
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
                throw ex.AsExtract("ELI51913");
            }
        }
    }

    class DatabaseCleanupStatus : DatabaseServiceStatus
    {
        const int _CURRENT_VERSION = 1;

        [DataMember]
        public override int Version { get; protected set; } = _CURRENT_VERSION;

        public int LastFileTaskSessionIDProcessed { get; set; }

        /// <summary>
        /// Called after this instance is deserialized.
        /// </summary>
        [OnDeserialized]
        void OnDeserialized(StreamingContext context)
        {
            if (Version > _CURRENT_VERSION)
            {
                ExtractException ee = new ExtractException("ELI51910", "Settings were saved with a newer version.");
                ee.AddDebugData("SavedVersion", Version, false);
                ee.AddDebugData("CurrentVersion", _CURRENT_VERSION, false);
                throw ee;
            }

            Version = _CURRENT_VERSION;
        }
    }
}
