using Extract.Code.Attributes;
using Extract.SqlDatabase;
using Extract.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Threading;
using System.Transactions;
using static System.FormattableString;

namespace Extract.ETL
{
    /// <summary>
    /// Defines the base class for processes that will be performed by a service
    /// </summary>
    [DataContract]
    [KnownType(typeof(ScheduledEvent))]
    [DatabaseService]
    public abstract class DatabaseService : IDisposable, ICloneable, INotifyPropertyChanged
    {
        internal static JsonSerializerSettings _serializeSettings =
            new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Objects, Formatting = Formatting.Indented };

        internal static JsonSerializerSettings _deserializeSettings =
            new JsonSerializerSettings {
                TypeNameHandling = TypeNameHandling.Objects,
                SerializationBinder = new NamespaceMappingSerializationBinder(
                  new List<KeyValuePair<string, string>> {
                      new KeyValuePair<string, string>(
                          "Extract.UtilityApplications.TrainingCoordinator", "Extract.UtilityApplications.MachineLearning"),
                      new KeyValuePair<string, string>(
                          "Extract.UtilityApplications.TrainingDataCollector", "Extract.UtilityApplications.MachineLearning"),
                      new KeyValuePair<string, string>(
                          "Extract.UtilityApplications.MLModelTrainer", "Extract.UtilityApplications.MachineLearning")
                  })};

        bool _enabled = true;
        int _databaseServiceID;
        string _databaseServer = string.Empty;
        string _databaseName = string.Empty;
        string _description = string.Empty;
        int _activeFAMID;

        static readonly string limitToTaskStoring = Invariant($@"TaskClassGUID in (
                                    '{Constants.TaskClassStoreRetrieveAttributes}',
                                    '{Constants.TaskClassDocumentApi}',
                                    '{Constants.TaskClassWebVerification}')");
        static readonly string queryForMaxReportableStoringTasks = $@"
                    DECLARE @MaxFileTaskSession INT, @MaxReportable INT;
                    SELECT @MaxFileTaskSession = MAX(FileTaskSession.ID)
                    FROM FileTaskSession WITH (NOLOCK)
                    WHERE {limitToTaskStoring}
                    

                    SELECT @MaxReportable = MIN(FileTaskSession.ID) - 1
                    FROM FileTaskSession WITH (NOLOCK)
                    WHERE {limitToTaskStoring} AND 

                    FileTaskSession.FAMSessionID IN
                    (
                        SELECT FAMSessionID
                        FROM ActiveFAM WITH (NOLOCK)
        
                    )
                    AND DateTimeStamp IS NULL;

                    SELECT ISNULL(@MaxReportable, @MaxFileTaskSession);";

        static readonly string queryForMaxReportableAllTasks = $@"
                    DECLARE @MaxFileTaskSession INT, @MaxReportable INT;
                    SELECT @MaxFileTaskSession = MAX(FileTaskSession.ID)
                    FROM FileTaskSession WITH (NOLOCK)
                    

                    SELECT @MaxReportable = MIN(FileTaskSession.ID) - 1
                    FROM FileTaskSession WITH (NOLOCK)

                    WHERE FileTaskSession.FAMSessionID IN
                    (
                        SELECT FAMSessionID
                        FROM ActiveFAM WITH (NOLOCK)
        
                    )
                    AND DateTimeStamp IS NULL;

                    SELECT ISNULL(@MaxReportable, @MaxFileTaskSession);";

        /// <summary>
        /// Description of the database service item
        /// </summary>
        [DataMember]
        public string Description
        {
            get
            {
                return _description;
            }
            set
            {
                if (value != _description)
                {
                    _description = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Name of the database. This value is not included in the settings
        /// </summary>
        /// <remarks>If this instance is an IHasConfigurableDatabaseServiceStatus then changing this value
        /// will result in a call to <see cref="RefreshStatus"/></remarks>
        public virtual string DatabaseName
        {
            get
            {
                return _databaseName;
            }
            set
            {
                if (_databaseName != value)
                {
                    _databaseName = value;

                    if (this is IHasConfigurableDatabaseServiceStatus hasStatus)
                    {
                        try
                        {
                            hasStatus.RefreshStatus();
                        }
                        catch (Exception ex)
                        {
                            ex.AsExtract("ELI45730").Log();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Name of the Server. This value is not included in the settings
        /// </summary>
        /// <remarks>If this instance is an IHasConfigurableDatabaseServiceStatus then changing this value
        /// will result in a call to <see cref="RefreshStatus"/></remarks>
        public virtual string DatabaseServer
        {
            get
            {
                return _databaseServer;
            }
            set
            {
                if (_databaseServer != value)
                {
                    _databaseServer = value;

                    if (this is IHasConfigurableDatabaseServiceStatus hasStatus)
                    {
                        try
                        {
                            hasStatus.RefreshStatus();
                        }
                        catch (Exception ex)
                        {
                            ex.AsExtract("ELI45731").Log();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// This is the id from the DatabaseService table.  This value is not included in the settings
        /// </summary>
        /// <remarks>If this instance is an IHasConfigurableDatabaseServiceStatus then changing this value
        /// will result in a call to <see cref="RefreshStatus"/></remarks>
        public int DatabaseServiceID
        {
            get
            {
                return _databaseServiceID;
            }
            set
            {
                if (_databaseServiceID != value)
                {
                    _databaseServiceID = value;

                    if (this is IHasConfigurableDatabaseServiceStatus hasStatus)
                    {
                        try
                        {
                            hasStatus.RefreshStatus();
                        }
                        catch (Exception ex)
                        {
                            ex.AsExtract("ELI45725").Log();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// If the DatabaseService can use multiple threads this is the max number of threads the service should use for processing
        /// </summary>
        public int NumberOfProcessingThreads { get; set; } = 1;

        [DataMember]
        public ScheduledEvent Schedule { get; set; }

        /// <summary>
        /// Whether enabled
        /// </summary>
        public virtual bool Enabled
        {
            get
            {
                return _enabled;
            }

            set
            {
                try
                {
                    if (value != _enabled)
                    {
                        _enabled = value;
                        if (Schedule != null)
                        {
                            Schedule.Enabled = value;
                        }
                        NotifyPropertyChanged();
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI45418");
                }
            }
        }

        /// <summary>
        /// Whether processing
        /// </summary>
        public abstract bool Processing { get; }

        /// <summary>
        /// The version
        /// </summary>
        [DataMember]
        public abstract int Version { get; protected set; }

        /// <summary>
        /// The type description set for the ExtractCategory attribute for the DatabaseService category
        /// </summary>
        public string ExtractCategoryType
        {
            get
            {
                return UtilityMethods.GetExtractCategoryTypeDescription("DatabaseService", GetType());
            }
        }

        /// <summary>
        /// Performs the processing defined the database service record
        /// </summary>
        /// <param name="cancelToken">Token that will cancel the processing</param>
        public abstract void Process(CancellationToken cancelToken);

        /// <summary>
        /// Returns the settings in a JSON string
        /// </summary>
        public string ToJson()
        {
            try
            {
                return JsonConvert.SerializeObject(this, _serializeSettings);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45368");
            }
        }

        /// <summary>
        /// Deserializes a <see cref="DatabaseService"/> instance from a JSON string
        /// </summary>
        /// <param name="settings">The JSON string to which a <see cref="DatabaseService"/> was previously saved</param>
        public static DatabaseService FromJson(string settings)
        {
            try
            {
                return (DatabaseService)JsonConvert.DeserializeObject(settings, _deserializeSettings);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45387");
            }
        }

        /// <summary>
        /// Sets the MachineID, StartTime and EndTime in the DatabaseService record representing the start of a process
        /// </summary>
        public void RecordProcessStart()
        {
            try
            {
                using var connection = new ExtractRoleConnection(DatabaseServer, DatabaseName);
                connection.Open();

                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText =
                        @"DECLARE @MachineID INT;
                        SELECT @MachineID = ID
                        FROM Machine
                        WHERE MachineName = @MachineName;

                        UPDATE DatabaseService 
                        SET StartTime = GetDate(),
                            EndTime = NULL,
                            NextScheduledRunTime = @NextScheduledRunTime,
	                        MachineID = @MachineID
                        WHERE ID = @DatabaseServiceID AND ActiveFAMID = @ActiveFAMID";

                    cmd.Parameters.Add("@MachineName", SqlDbType.NVarChar, 50).Value = Environment.MachineName;
                    cmd.Parameters.AddWithValue("@DatabaseServiceID", DatabaseServiceID);
                    cmd.Parameters.AddWithValue("@ActiveFAMID", _activeFAMID);
                    if (Schedule?.GetNextOccurrence() is DateTime nextOccurrence)
                    {
                        cmd.Parameters.AddWithValue("@NextScheduledRunTime", nextOccurrence);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@NextScheduledRunTime", DBNull.Value);
                    }
                    int rowsAffected = cmd.ExecuteNonQuery();
                    if (rowsAffected < 1)
                    {
                        ExtractException ee = new ExtractException("ELI46294", "Unable to Record process start");
                        ee.AddDebugData("SQL", cmd.CommandText, false);
                        ee.AddDebugData("@DatabaseServiceID", DatabaseServiceID, false);
                        ee.AddDebugData("@ActiveFAMID", _activeFAMID, false);
                        ee.AddDebugData("@MachineName", Environment.MachineName, false);
                        throw ee;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI46293");
            }
        }

        /// <summary>
        /// Sets the EndTime and if there is an exception the Exception fields in the DatabaseService record
        /// for the given dbService
        /// </summary>
        /// <param name="stringizedException">Stringized representation of the exception that was thrown</param>
        public void RecordProcessComplete(string stringizedException = null)
        {
            try
            {
                using var connection = new ExtractRoleConnection(DatabaseServer, DatabaseName);
                connection.Open();

                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText =
                        @"UPDATE DatabaseService 
                                SET     EndTime = GetDate(),
                                        NextScheduledRunTime = @NextScheduledRunTime,
                                        Exception = @Exception
                              WHERE ID = @DatabaseServiceID AND ActiveFAMID = @ActiveFAMID";

                    if (string.IsNullOrEmpty(stringizedException))
                    {
                        cmd.Parameters.AddWithValue("@Exception", DBNull.Value);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@Exception", stringizedException);
                    }
                    cmd.Parameters.AddWithValue("@DatabaseServiceID", DatabaseServiceID);
                    cmd.Parameters.AddWithValue("@ActiveFAMID", _activeFAMID);
                    if (Schedule?.GetNextOccurrence() is DateTime nextOccurrence)
                    {
                        cmd.Parameters.AddWithValue("@NextScheduledRunTime", nextOccurrence);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@NextScheduledRunTime", DBNull.Value);
                    }
                    int rowsAffected = cmd.ExecuteNonQuery();
                    if (rowsAffected < 1)
                    {
                        ExtractException ee = new ExtractException("ELI50081", "Unable to Record process start");
                        ee.AddDebugData("SQL", cmd.CommandText, false);
                        ee.AddDebugData("@DatabaseServiceID", DatabaseServiceID, false);
                        ee.AddDebugData("@ActiveFAMID", _activeFAMID, false);
                        throw ee;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI46295");
            }
        }

        /// <summary>
        /// Starts a FAMSession and ActiveFAM record for the DatabaseService 
        /// </summary>
        /// <returns><c>true</c> if the Schedule is active with activeFAMID. Otherwise <c>false</c></returns>
        public bool StartActiveSchedule(int activeFAMId)
        {
            try
            {
                using var connection = new ExtractRoleConnection(DatabaseServer, DatabaseName);
                connection.Open();

                using var scope = new TransactionScope(TransactionScopeOption.Required,
                    new TransactionOptions
                    {
                        IsolationLevel = System.Transactions.IsolationLevel.Serializable
                    });

                // check the existing value - since this is done within a transaction another process will not be able to change
                // it after this process reads it
                using (var cmdExisting = connection.CreateCommand())
                {
                    cmdExisting.CommandText = "SELECT [ActiveFAMID] FROM DatabaseService WHERE ID = @DatabaseServiceID";
                    cmdExisting.Parameters.AddWithValue("@DatabaseServiceID", DatabaseServiceID);

                    var result = cmdExisting.ExecuteScalar();
                    if (result != DBNull.Value)
                    {
                        return (int?)result == _activeFAMID;
                    }
                }
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText =
                        @"DECLARE @MachineID INT;
                              SELECT @MachineID = ID
                              FROM Machine
                              WHERE MachineName = @MachineName;
                              UPDATE [DatabaseService] 
                              SET     ActiveFAMID = @ActiveFAMID,
                                      NextScheduledRunTime = @NextScheduledRunTime,
                                      ActiveServiceMachineID = @MachineID
                              WHERE ID = @DatabaseServiceID";
                    cmd.Parameters.AddWithValue("@ActiveFAMID", activeFAMId);
                    cmd.Parameters.AddWithValue("@DatabaseServiceID", DatabaseServiceID);
                    cmd.Parameters.Add("@MachineName", SqlDbType.NVarChar, 50).Value = Environment.MachineName;
                    if (Schedule?.GetNextOccurrence() is DateTime nextOccurrence)
                    {
                        cmd.Parameters.AddWithValue("@NextScheduledRunTime", nextOccurrence);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@NextScheduledRunTime", DBNull.Value);
                    }
                    cmd.ExecuteNonQuery();
                    _activeFAMID = activeFAMId;
                }
                scope.Complete();

                return true;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI46281");
            }
        }

        /// <summary>
        /// Stops and Active FAMSession for the service 
        /// </summary>
        public void StopActiveSchedule()
        {
            try
            {
                // I only want to stop the active schedule if the current process is the one that started it
                using var connection = new ExtractRoleConnection(DatabaseServer, DatabaseName);
                connection.Open();
                using var cmd = connection.CreateCommand();

                cmd.CommandText =
                    @"UPDATE [DatabaseService] 
                        SET     ActiveFAMID = NULL,
                                ActiveServiceMachineID = NULL,
                                NextScheduledRunTime = NULL
                    WHERE ID = @DatabaseServiceID AND ActiveFAMID = @ActiveFAMID";
                cmd.Parameters.AddWithValue("@ActiveFAMID", _activeFAMID);
                cmd.Parameters.AddWithValue("@DatabaseServiceID", DatabaseServiceID);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI46282");
            }
        }

        /// <summary>
        /// Saves <see paramref="status"/> to the DB for this <see cref="DatabaseServiceID"/>
        /// </summary>
        /// <param name="connection">Connection to use for saving the status</param>
        /// <param name="status">The <see cref="DatabaseServiceStatus"/> instance to save</param>
        public void SaveStatus(SqlAppRoleConnection connection, DatabaseServiceStatus status)
        {
            try
            {
                if (DatabaseServiceID <= 0)
                {
                    return;
                }

                if (status == null)
                {
                    using var cmd = connection.CreateCommand();
                    cmd.CommandText = @"
                            UPDATE [DatabaseService]
                            SET [Status] = NULL,
                            [LastFileTaskSessionIDProcessed] = NULL
                            WHERE ID = @DatabaseServiceID";
                    cmd.Parameters.Add("@DatabaseServiceID", SqlDbType.Int).Value = DatabaseServiceID;
                    cmd.ExecuteNonQuery();
                }
                else
                {
                    status.SaveStatus(connection, DatabaseServiceID);
                }

            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45716");
            }
        }

        /// <summary>
        /// Inserts a record into the DatabaseService table for this service and sets the DatabaseServer,
        /// DatabaseName and DatabaseServiceID fields of this instance
        /// </summary>
        /// <param name="databaseServer">The server to add to</param>
        /// <param name="databaseName">The database to add to</param>
        /// <returns>The ID of the new record</returns>
        public int AddToDatabase(string databaseServer, string databaseName)
        {
            try
            {
                _databaseServer = databaseServer;
                _databaseName = databaseName;

                using var connection = new ExtractRoleConnection(DatabaseServer, DatabaseName);
                connection.Open();
                using var trans = new TransactionScope();

                using var cmd = connection.CreateCommand();
                cmd.CommandText = @"
                        INSERT INTO [dbo].[DatabaseService]
                                    ([Description]
                                    ,[Settings]
                                    ,[Enabled]
                                    )
                        OUTPUT inserted.id
                        VALUES (
                            @Description,
                            @Settings,
                            @Enabled)";

                cmd.Parameters.AddWithValue("@Description", Description);
                cmd.Parameters.AddWithValue("@Settings", ToJson());
                cmd.Parameters.AddWithValue("@Enabled", true);

                // Some services allow editing of initial status values so save 
                if (this is IHasConfigurableDatabaseServiceStatus hasStatus)
                {
                    cmd.CommandText = @"
                            INSERT INTO [dbo].[DatabaseService]
                                        ([Description]
                                        ,[Settings]
                                        ,[Enabled]
                                        ,[Status]
                                        ,[LastFileTaskSessionIDProcessed]
                                        )
                            OUTPUT inserted.id
                            VALUES (
                                @Description,
                                @Settings,
                                @Enabled,
                                @Status,
                                @LastFileTaskSession)";

                    string status = hasStatus.Status?.ToJson();
                    if (status != null)
                    {
                        cmd.Parameters.AddWithValue("@Status", status);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@Status", DBNull.Value);
                    }
                    var fileTaskSessionStatus = hasStatus.Status as IFileTaskSessionServiceStatus;
                    int? lastFileTaskSession = fileTaskSessionStatus?.LastFileTaskSessionIDProcessed;
                    if (lastFileTaskSession is null)
                    {
                        cmd.Parameters.AddWithValue("@LastFileTaskSession", DBNull.Value);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@LastFileTaskSession", lastFileTaskSession);
                    }
                }

                _databaseServiceID = (int)cmd.ExecuteScalar();
                trans.Complete();

                return _databaseServiceID;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI46489");
            }
        }

        /// <summary>
        /// Updates the settings stored in the configured database
        /// </summary>
        public void UpdateDatabaseServiceSettings()
        {
            try
            {
                ExtractException.Assert("ELI46505", "Database Server must be set.", !string.IsNullOrWhiteSpace(DatabaseServer));
                ExtractException.Assert("ELI46506", "Database Name must be set.", !string.IsNullOrWhiteSpace(DatabaseName));

                using var connection = new ExtractRoleConnection(DatabaseServer, DatabaseName);
                connection.Open();
                using var trans = new TransactionScope();

                using var cmd = connection.CreateCommand();
                cmd.CommandText = @"
                                UPDATE DatabaseService 
                                SET [Description] = @Description,
                                    [Settings]    = @Settings
                                WHERE ID = @DatabaseServiceID";
                cmd.Parameters.AddWithValue("@Description", Description);
                cmd.Parameters.AddWithValue("@Settings", ToJson());
                cmd.Parameters.AddWithValue("@DatabaseServiceID", DatabaseServiceID);

                // Some services allow editing of initial status values so update the status column in that case
                if (this is IHasConfigurableDatabaseServiceStatus hasStatus)
                {
                    cmd.CommandText = @"
                                    UPDATE DatabaseService 
                                    SET [Description] = @Description,
                                        [Settings]    = @Settings,
                                        [Status]      = @Status
                                    WHERE ID = @DatabaseServiceID";

                    string status = hasStatus.Status?.ToJson();
                    if (status != null)
                    {
                        cmd.Parameters.AddWithValue("@Status", status);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@Status", DBNull.Value);
                    }
                }

                cmd.ExecuteNonQuery();
                trans.Complete();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI46504");
            }
        }

        /// <summary>
        /// Gets the status object for this service from the DB or creates an instance using the supplied function
        /// </summary>
        /// <typeparam name="T">The type of the status object to return/create</typeparam>
        /// <returns>Either the last status saved to the DB for this ID or a new instance
        /// if the status column for this <see cref="DatabaseServiceID"/> is null or if the
        /// <see cref="DatabaseServiceID"/> is invalid (is less than 1)</returns>
        /// <param name="creator">The function used to create a new instance, if needed</param>
        protected T GetLastOrCreateStatus<T>(Func<T> creator) where T : DatabaseServiceStatus
        {
            if (string.IsNullOrWhiteSpace(DatabaseServer) || string.IsNullOrWhiteSpace(DatabaseName))
            {
                ExtractException ee = new ExtractException("ELI51835", "Server or Database not specified.");
                ee.AddDebugData("Server", DatabaseServer);
                ee.AddDebugData("Database", DatabaseName);
                throw ee;
            }

            if (DatabaseServiceID <= 0)
            {
                return creator();
            }
            string jsonStatus = string.Empty;
            Int32? lastFileTaskSessionID = null;

            using (var connection = new ExtractRoleConnection(DatabaseServer, DatabaseName))
            {
                connection.Open();

                using var statusCmd = connection.CreateCommand();

                // need to get the previous status
                statusCmd.CommandText = "SELECT Status, LastFileTaskSessionIDProcessed FROM DatabaseService WHERE ID = @DatabaseServiceID";
                statusCmd.Parameters.AddWithValue("@DatabaseServiceID", DatabaseServiceID);
                using var statusResult = statusCmd.ExecuteReader();

                if (!statusResult.HasRows)
                {
                    ExtractException ee = new ExtractException("ELI45479", "Invalid DatabaseServiceID.");
                    ee.AddDebugData("DatabaseServiceID", DatabaseServiceID, false);
                    throw ee;
                }

                if (statusResult.Read() && !statusResult.IsDBNull(statusResult.GetOrdinal("Status")))
                {
                    jsonStatus = statusResult.GetString(statusResult.GetOrdinal("Status"));
                    if (!statusResult.IsDBNull(statusResult.GetOrdinal("LastFileTaskSessionIDProcessed")))
                    {
                        lastFileTaskSessionID = statusResult.GetInt32(statusResult.GetOrdinal("LastFileTaskSessionIDProcessed"));
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(jsonStatus))
            {
                return creator();
            }
            else
            {
                if (DatabaseServiceStatus.FromJson(jsonStatus) is T status)
                {
                    var fileTaskSessionServiceStatus = status as IFileTaskSessionServiceStatus;
                    if (fileTaskSessionServiceStatus != null)
                    {
                        // if the DatabaseService record doesn't have a value for LastFileTaskSessionIDProcessed set
                        // it to the one in the status
                        if (lastFileTaskSessionID is null)
                        {
                            // Check for the Old value of LastFileTaskSessionIDProcessed in the json
                            JObject search = JObject.Parse(jsonStatus);
                            JToken lastFileTaskSessionFromStatus = search.SelectToken("LastFileTaskSessionIDProcessed");

                            // If there was a value in the json set the DatabaseService.LastFileTaskSessionIDProcessed column
                            // to that value
                            if (lastFileTaskSessionFromStatus != null)
                            {
                                // The LastFileTaskSessionIDProcessed in the DatabaseService record was null so
                                // set it to the value in the status record
                                using var saveConnection = new ExtractRoleConnection(DatabaseServer, DatabaseName);
                                saveConnection.Open();

                                using var saveCmd = saveConnection.CreateCommand();
                                lastFileTaskSessionID = lastFileTaskSessionFromStatus.Value<int>();
                                saveCmd.CommandText =
                                    "UPDATE DatabaseService Set LastFileTaskSessionIDProcessed = @LastFileTaskSessionID WHERE ID = @DatabaseServiceID";
                                saveCmd.Parameters.AddWithValue("@LastFileTaskSessionID", lastFileTaskSessionID);
                                saveCmd.Parameters.AddWithValue("@DatabaseServiceID", DatabaseServiceID);
                                saveCmd.ExecuteNonQuery();
                            }   
                        }
                        if (lastFileTaskSessionID != null)
                        {
                            fileTaskSessionServiceStatus.LastFileTaskSessionIDProcessed = (int)lastFileTaskSessionID;
                        }
                    }
                    return status;
                }
                else
                {
                    ExtractException statusException = new ExtractException("ELI45710", "Service could not determine previous status.");
                    statusException.AddDebugData("DatabaseServiceID", DatabaseServiceID, false);
                    statusException.AddDebugData("jsonStatusString", jsonStatus, false);
                    throw statusException;
                }
            }
        }

        /// <summary>
        /// Returns the maximum FileTaskSession row ID that should be used for ETL service processing.
        /// This is either 1 less than the minimum row belonging to an ActiveFAM that does not have
        /// a DateTimeStamp assigned, or the maximum FileTaskSession ID if no active session exists.
        /// </summary>
        /// <param name="onlyTasksStoringAttributes">This will restrict the file task sessions to those for the Store/Retrieve task and web verification</param>
        /// <returns></returns>
        protected int MaxReportableFileTaskSessionId(bool onlyTasksStoringAttributes)
        {
            try
            {
                using var connection = new ExtractRoleConnection(DatabaseServer, DatabaseName);
                connection.Open();


                using var cmd = connection.CreateCommand();
                cmd.CommandText = (onlyTasksStoringAttributes) ?
                    queryForMaxReportableStoringTasks :
                    queryForMaxReportableAllTasks;

                var result = cmd.ExecuteScalar();

                // The value returned could be a DBNull value so check
                return (int)((result == DBNull.Value) ? -1 : result);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI46066");
            }
        }

        #region IDisposable Members

        /// <summary>
        /// Releases all resources
        /// the temporary file being managed by this class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases all unmanaged resources
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged
        /// resources; <see langword="false"/> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose of managed resources
                Schedule?.Dispose();
                Schedule = null;
            }

            // Dispose of unmanaged resources
        }

        #endregion IDisposable Members

        #region IClonable members

        /// <summary>
        /// Clones the object - only properties that are serialized are copied to the new object
        /// </summary>
        /// <returns>A new instance of the object with the same serializable settings</returns>
        public virtual object Clone()
        {
            try
            {
                return FromJson(ToJson());
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45662");
            }
        }

        #endregion

        #region INotifyPropertyChanged

        /// <summary>
        /// Property changed event
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion INotifyPropertyChanged

        #region Helper members

        /// <summary>
        /// This method is called by the Set accessor of properties that support notification
        /// </summary>
        /// <param name="propertyName">Optional name of property that changed</param>
        protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
