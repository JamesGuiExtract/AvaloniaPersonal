using Extract.Code.Attributes;
using Extract.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Threading;

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
        bool _enabled = true;
        int _databaseServiceID;
        string _databaseService = string.Empty;
        string _databaseName = string.Empty;
        string _description = string.Empty;

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
                return _databaseService;
            }
            set
            {
                if (_databaseService != value)
                {
                    _databaseService = value;

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
                return JsonConvert.SerializeObject(this,
                    new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Objects, Formatting = Formatting.Indented });
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
                return (DatabaseService)JsonConvert.DeserializeObject(settings,
                    new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Objects });
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45387");
            }
        }

        /// <summary>
        /// Saves <see paramref="status"/> to the DB for this <see cref="DatabaseServiceID"/>
        /// </summary>
        /// <param name="status">The <see cref="DatabaseServiceStatus"/> instance to save</param>
        public void SaveStatus(DatabaseServiceStatus status)
        {
            if (DatabaseServiceID <= 0)
            {
                return;
            }

            // Save status to the DB
            using (var connection = NewSqlDBConnection())
            {
                connection.Open();
                try
                {
                    var fileTaskSessionStatus = status as IFileTaskSessionServiceStatus;
                    int? lastFileTaskSession = fileTaskSessionStatus?.LastFileTaskSessionIDProcessed;

                    if (status == null)
                    {
                        using (var cmd = connection.CreateCommand())
                        {
                            cmd.CommandText = @"
                                UPDATE [DatabaseService]
                                SET [Status] = NULL,
                                [LastFileTaskSessionIDProcessed] = @LastFileTaskSession
                                WHERE ID = @DatabaseServiceID";
                            cmd.Parameters.Add("@DatabaseServiceID", SqlDbType.Int).Value = DatabaseServiceID;
                            if (lastFileTaskSession is null)
                            {
                                cmd.Parameters.AddWithValue("@LastFileTaskSession", DBNull.Value);
                            }
                            else
                            {
                                cmd.Parameters.AddWithValue("@LastFileTaskSession", lastFileTaskSession);
                            }
                            cmd.ExecuteNonQuery();
                        }
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
            if (DatabaseServiceID <= 0)
            {
                return creator();
            }
            using (var connection = NewSqlDBConnection(enlist: false))
            {
                connection.Open();

                // need to get the previous status
                var statusCmd = connection.CreateCommand();
                statusCmd.CommandText = "SELECT Status, LastFileTaskSessionIDProcessed FROM DatabaseService WHERE ID = @DatabaseServiceID";
                statusCmd.Parameters.AddWithValue("@DatabaseServiceID", DatabaseServiceID);
                using (var statusResult = statusCmd.ExecuteReader())
                {
                    if (!statusResult.HasRows)
                    {
                        ExtractException ee = new ExtractException("ELI45479", "Invalid DatabaseServiceID.");
                        ee.AddDebugData("DatabaseServiceID", DatabaseServiceID, false);
                        throw ee;
                    }

                    string jsonStatus = string.Empty;
                    Int32? lastFileTaskSessionID = null;
                    if (statusResult.Read() && !statusResult.IsDBNull(statusResult.GetOrdinal("Status")))
                    {
                        jsonStatus = statusResult.GetString(statusResult.GetOrdinal("Status"));
                        if (!statusResult.IsDBNull(statusResult.GetOrdinal("LastFileTaskSessionIDProcessed")))
                        {
                            lastFileTaskSessionID = statusResult.GetInt32(statusResult.GetOrdinal("LastFileTaskSessionIDProcessed"));
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
                                        using (var saveConnection = NewSqlDBConnection())
                                        {
                                            saveConnection.Open();
                                            using (var saveCmd = saveConnection.CreateCommand())
                                            {
                                                lastFileTaskSessionID = lastFileTaskSessionFromStatus.Value<int>();
                                                saveCmd.CommandText =
                                                    "UPDATE DatabaseService Set LastFileTaskSessionIDProcessed = @LastFileTaskSessionID WHERE ID = @DatabaseServiceID";
                                                saveCmd.Parameters.AddWithValue("@LastFileTaskSessionID", lastFileTaskSessionID);
                                                saveCmd.Parameters.AddWithValue("@DatabaseServiceID", DatabaseServiceID);
                                                saveCmd.ExecuteNonQuery();
                                            }
                                        }
                                    }
                                }
                                fileTaskSessionServiceStatus.LastFileTaskSessionIDProcessed = lastFileTaskSessionID ??
                                    fileTaskSessionServiceStatus.LastFileTaskSessionIDProcessed;
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
            }
        }

        /// <summary>
        /// Returns the maximum FileTaskSession row ID that should be used for ETL service processing.
        /// This is either 1 less than the minimum row belonging to an ActiveFAM that does not have
        /// a DateTimeStamp assigned, or the maximum FileTaskSession ID if no active session exists.
        /// </summary>
        /// <returns></returns>
        protected int MaxReportableFileTaskSessionId(bool storeTaskOnly = false)
        {
            try
            {
                using (var connection = NewSqlDBConnection())
                {
                    connection.Open();

                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = @"
                            SELECT COALESCE(MIN(CASE WHEN ActiveFAM.ID IS NOT NULL AND DateTimeStamp IS NULL THEN FileTaskSession.ID END) - 1,
                                    MAX(FileTaskSession.ID))
	                            FROM FileTaskSession
								INNER JOIN TaskClass ON TaskClass.ID = FileTaskSession.TaskClassID
	                            LEFT JOIN FAMSession ON FAMSessionID = FAMSession.ID
	                            LEFT JOIN ActiveFAM ON FAMSession.ID = ActiveFAM.FAMSessionID 
                        ";

                        if (storeTaskOnly)
                        {
                            cmd.CommandText += "WHERE TaskClass.GUID = 'B25D64C0-6FF6-4E0B-83D4-0D5DFEB68006'";
                        }

                        return (int)(cmd.ExecuteScalar() ?? -1);
                    }
                }
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
        /// Returns a connection to the configured database
        /// </summary>
        /// <param name="enlist">Whether to enlist in a transaction scope if there is one</param>
        /// <returns>SqlConnection that connects to the <see cref="DatabaseServer"/> and <see cref="DatabaseName"/></returns>
        protected virtual SqlConnection NewSqlDBConnection(bool enlist = true)
        {
            // Build the connection string from the settings
            SqlConnectionStringBuilder sqlConnectionBuild = new SqlConnectionStringBuilder();
            sqlConnectionBuild.DataSource = DatabaseServer;
            sqlConnectionBuild.InitialCatalog = DatabaseName;
            sqlConnectionBuild.IntegratedSecurity = true;
            sqlConnectionBuild.NetworkLibrary = "dbmssocn";
            sqlConnectionBuild.MultipleActiveResultSets = true;
            sqlConnectionBuild.Enlist = enlist;
            return new SqlConnection(sqlConnectionBuild.ConnectionString);
        }

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
