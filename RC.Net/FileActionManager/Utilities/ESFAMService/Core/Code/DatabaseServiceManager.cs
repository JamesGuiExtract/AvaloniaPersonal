using Extract.Database;
using Extract.ETL;
using Extract.Utilities;
using System;
using System.Collections.Concurrent;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UCLID_FILEPROCESSINGLib;

namespace Extract.FileActionManager.Utilities
{
    // Manages the execution of all DatabaseService instances defined in a FAM database.
    class DatabaseServiceManager : IDisposable
    {
        #region Fields

        // Map of the ScheduledEvent for each service to the corresponding service to execute.
        ConcurrentDictionary<ScheduledEvent, DatabaseService> _databaseServices =
            new ConcurrentDictionary<ScheduledEvent, DatabaseService>();

        // Map for indicating if the scheduledEvent is in the EventStarted event so that 
        // if it is already waiting another will not be queued
        ConcurrentDictionary<ScheduledEvent, bool> _inEventStarted = new ConcurrentDictionary<ScheduledEvent, bool>();

        // Lock for accessing _inEventStarted
        object _inEventStartedLock = new object();

        /// <summary>
        /// Indicates whether the manager is currently running.
        /// </summary>
        bool _running;

        /// <summary>
        /// Event indicating the service has been stopped.
        /// </summary>
        ManualResetEvent _stoppedEvent = new ManualResetEvent(false);

        /// <summary>
        /// Used to abort TryStart in the event ESFAMService is stopped before this manager was
        /// started.
        /// </summary>
        CancellationTokenSource _canceller = new CancellationTokenSource();

        /// <summary>
        /// An OLE database connection to the FAM database for the purpose of initializing the
        /// services.
        /// </summary>
        OleDbConnection _oleDbConnection;

        /// <summary>
        /// A <see cref="FileProcessingDB"/> used to create FAMSession rows such that only one
        /// service instance will try to run DatabaseServices if ESFAMServices are running.
        /// </summary>
        FileProcessingDB _fileProcessingDb;

        /// <summary>
        /// Synchronizes access to fields for thread safety.
        /// </summary>
        object _lock = new object();

        /// <summary>
        /// Synchronizes execution so that only one service is active at a time.
        /// </summary>
        object _processingLock = new object();

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseServiceManager"/> class.
        /// </summary>
        /// <param name="connectionString">The connection string for the FAM database.</param>
        public DatabaseServiceManager(string connectionString)
        {
            try
            {
                _oleDbConnection = new OleDbConnection(connectionString);
                _oleDbConnection.Open();

                _fileProcessingDb = new FileProcessingDB();
                _fileProcessingDb.DatabaseServer = _oleDbConnection.DataSource;
                _fileProcessingDb.DatabaseName = _oleDbConnection.Database;

                TryStart();
            }
            catch (Exception ex)
            {
                _stoppedEvent.Set();
                throw ex.AsExtract("ELI45412");
            }
        }

        #endregion Constructors

        #region Properties

        public EventWaitHandle StoppedWaitHandle
        {
            get
            {
                return _stoppedEvent;
            }
        }

        #endregion Properties

        #region IDisposable Members

        /// <summary>
        /// Releases all resources used by the <see cref="DatabaseServiceManager"/>. Also deletes
        /// the temporary file being managed by this class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases all unmanaged resources used by the <see cref="DatabaseServiceManager"/>. Also
        /// deletes the temporary file being managed by this class.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged
        /// resources; <see langword="false"/> to release only unmanaged resources.</param>
        void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose of managed resources
                try
                {
                    if (_oleDbConnection != null)
                    {
                        _oleDbConnection.Dispose();
                        _oleDbConnection = null;
                    }

                    try
                    {
                        Stop();
                    }
                    catch { }

                    _canceller.Dispose();
                    _stoppedEvent.Dispose();
                }
                catch { }
            }

            // Dispose of unmanaged resources
            if (_fileProcessingDb != null)
            {
                try
                {
                    _fileProcessingDb.CloseAllDBConnections();
                }
                catch { }
            }
        }

        #endregion IDisposable Members

        #region Event Handlers

        void HandleScheduleEvent_EventStarted(object sender, EventArgs e)
        {
            try
            {
                DatabaseService dbService = _databaseServices[(ScheduledEvent)sender];

                lock (_inEventStartedLock)
                {
                    // check if the ScheduledEvent is already in this method
                    if (_inEventStarted[(ScheduledEvent)sender])
                    {
                        return;
                    }

                    // Set the flag to true to indicate that the 
                    _inEventStarted[(ScheduledEvent)sender] = true;
                }

                lock (_processingLock)
                {
                    try
                    {
                        if (_running &&
                            !_stoppedEvent.WaitOne(0) &&
                            dbService.Enabled &&
                            !dbService.Schedule.GetIsInExcludedTime())
                        {
                            var fileProcessingDB = new FileProcessingDB();
                            try
                            {
                                fileProcessingDB.DatabaseServer = _fileProcessingDb.DatabaseServer;
                                fileProcessingDB.DatabaseName = _fileProcessingDb.DatabaseName;

                                fileProcessingDB.RecordFAMSessionStart("ETL: " + dbService.Description, string.Empty, false, false);
                                RecordProcessStart(dbService);
                                try
                                {
                                    dbService.Process(_canceller.Token);
                                }
                                catch (ExtractException processExtractException)
                                {
                                    RecordProcessComplete(dbService, processExtractException.AsStringizedByteStream());
                                    throw processExtractException;
                                }
                                catch (Exception processException)
                                {
                                    ExtractException ee = processException.AsExtract("ELI46237");
                                    RecordProcessComplete(dbService, ee.AsStringizedByteStream());
                                    throw ee;
                                }
                                finally
                                {
                                    fileProcessingDB.RecordFAMSessionStop();
                                }
                                RecordProcessComplete(dbService);
                            }
                            finally
                            {
                                fileProcessingDB.CloseAllDBConnections();
                            }
                        }
                    }
                    finally
                    {
                        lock (_inEventStartedLock)
                        {
                            _inEventStarted[(ScheduledEvent)sender] = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log exception unless the service is stopping
                if (!_canceller.IsCancellationRequested)
                {
                    new ExtractException("ELI45414", "ETL process failure", ex).Log();
                }
            }
        }

        #endregion Event Handlers

        #region Public Members

        /// <summary>
        /// Stops this instance.
        /// </summary>
        public void Stop()
        {
            try
            {
                lock (_lock)
                {
                    _canceller.Cancel();

                    if (!_running)
                    {
                        return;
                    }

                    _running = false;

                    _fileProcessingDb.UnregisterActiveFAM();
                    _fileProcessingDb.RecordFAMSessionStop();
                    _fileProcessingDb.CloseAllDBConnections();

                    foreach (var dbService in _databaseServices)
                    {
                        dbService.Value.Enabled = false;
                    }
                }

                Task.Run(() =>
                {
                    foreach (var dbService in _databaseServices)
                    {
                        try
                        {
                            while (dbService.Value.Processing)
                            {
                                Thread.Sleep(1000);
                            }

                            dbService.Value.Dispose();
                        }
                        catch (Exception ex)
                        {
                            new ExtractException("ELI45395",
                                "Failed waiting for DB service to complete.", ex).Log();
                        }
                    }

                    _databaseServices.Clear();

                    _stoppedEvent.Set();
                });
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45416");
            }
        }

        #endregion Public Members

        #region Private Members

        /// <summary>
        /// Sets the MachineID, StartTime and EndTime in the DatabaseService record representing the start of a process
        /// </summary>
        /// <param name="dbService">The <see cref="DatabaseService"/> to record values for</param>
        void RecordProcessStart(DatabaseService dbService)
        {
            using (var cmd = _oleDbConnection.CreateCommand())
            {
                cmd.CommandText =
                    @"DECLARE @MachineID INT;
                      DECLARE @MachineName NVARCHAR(MAX) = ?
                        SELECT @MachineID = ID
                        FROM Machine
                        WHERE MachineName = @MachineName;
                        IF(@MachineID is NULL)
                            INSERT INTO Machine (MachineName) Select(@MachineName)
                        
                        SELECT @MachineID = ID
                        FROM Machine
                        WHERE MachineName = @MachineName;

                        UPDATE DatabaseService 
                        SET StartTime = GetDate(),
                            EndTime = NULL,
	                        MachineID = @MachineID
                        WHERE ID = ?";

                cmd.Parameters.AddWithValue("@MachineNameParameter", Environment.MachineName);
                cmd.Parameters.AddWithValue("@DatabaseServiceID", dbService.DatabaseServiceID);
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Sets the EndTime and if there is an exception the Exception fields in the DatabaseService record
        /// for the given dbService
        /// </summary>
        /// <param name="dbService">The <see cref="DatabaseService"/> to save data for</param>
        /// <param name="stringizedException">Stringized representation of the exception that was thrown</param>
        void RecordProcessComplete(DatabaseService dbService, string stringizedException = null)
        {
            using (var cmd = _oleDbConnection.CreateCommand())
            {
                cmd.CommandText = "UPDATE DatabaseService SET EndTime = GetDate()";

                if (string.IsNullOrEmpty(stringizedException))
                {
                    cmd.CommandText += ", Exception = ?";
                    cmd.Parameters.AddWithValue("@Exception", DBNull.Value);
                }
                else
                {
                    cmd.CommandText += ", Exception = ?";
                    cmd.Parameters.Add("@Exception", OleDbType.VarChar, stringizedException.Length + 1).Value = stringizedException;
                }
                cmd.CommandText += " WHERE ID = ?";
                cmd.Parameters.AddWithValue("@DatabaseServiceID", dbService.DatabaseServiceID);
                cmd.ExecuteNonQuery();
            }
        }

        void TryStart()
        {
            try
            {
                // Trigger to clean up timed out ActiveFAM instances.
                _fileProcessingDb.IsAnyFAMActive();

                if (DBMethods.GetQueryResultsAsStringArray(_oleDbConnection,
                    "SELECT [ActiveFAM].[ID] FROM [ActiveFAM] " +
                    "   INNER JOIN [FAMSession] ON [FAMSessionID] = [FAMSession].[ID]" +
                    "   INNER JOIN [FPSFile] ON [FPSFileID] = [FPSFile].[ID]" +
                    "   WHERE [FPSFileName] LIKE 'ETL%'").Any())
                {
                    // There is already another service executing ETL on this database;
                    // check every 5 minutes to see if other ETL services are still running.
                    var task = Task.Delay(300000, _canceller.Token);
                    task.ContinueWith(t => TryStart(), TaskContinuationOptions.OnlyOnRanToCompletion);
                    task.ContinueWith(t => _stoppedEvent.Set(), TaskContinuationOptions.NotOnRanToCompletion);
                    return;
                }
            }
            catch (Exception ex)
            {
                _stoppedEvent.Set();
                new ExtractException("ELI45413", "ETL failed before start", ex).Log();
                return;
            }

            Start();
        }

        /// <summary>
        /// Starts running the <see cref="DatabaseService"/> instances in the database.
        /// </summary>
        void Start()
        {
            try
            {
                lock (_lock)
                {
                    _fileProcessingDb.RecordFAMSessionStart("ETL Manager", string.Empty, false, false);
                    _fileProcessingDb.RegisterActiveFAM();

                    _running = true;

                    using (DataTable dbServiceDefinitions = DBMethods.ExecuteDBQuery(
                                _oleDbConnection, "SELECT [ID], [Settings] FROM [DatabaseService] WHERE Enabled = 1"))
                    {
                        foreach (DataRow dbServiceRow in dbServiceDefinitions.Rows)
                        {
                            var dbService = DatabaseService.FromJson((string)dbServiceRow["Settings"]);

                            if (_databaseServices.TryAdd(dbService.Schedule, dbService))
                            {
                                dbService.DatabaseServiceID = (int)dbServiceRow["ID"];
                                dbService.DatabaseServer = _oleDbConnection.DataSource;
                                dbService.DatabaseName = _oleDbConnection.Database;
                                _inEventStarted[dbService.Schedule] = false;
                                dbService.Schedule.EventStarted += HandleScheduleEvent_EventStarted;
                            }
                        }
                    }
                }

                if (!_databaseServices.Any())
                {
                    Stop();
                }
            }
            catch (Exception ex)
            {
                new ExtractException("ELI45415", "ETL failed to start", ex).Log();
            }
        }

        #endregion Private Members
    }
}
