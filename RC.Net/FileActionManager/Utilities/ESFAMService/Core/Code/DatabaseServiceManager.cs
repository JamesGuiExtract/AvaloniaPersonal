using Extract.Database;
using Extract.ETL;
using Extract.Interfaces;
using Extract.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UCLID_FILEPROCESSINGLib;

namespace Extract.FileActionManager.Utilities
{
    // Manages the execution of all DatabaseService instances defined in a FAM database.
    internal class DatabaseServiceManager : IDisposable
    {
        #region Fields

        // Map the of the ScheduledEvent for each service to the corresponding service to execute.
        ConcurrentDictionary<ScheduledEvent, DatabaseService> _databaseServices =
            new ConcurrentDictionary<ScheduledEvent, DatabaseService>();

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
        }

        #endregion IDisposable Members

        #region Event Handlers

        void HandleScheduleEvent_EventStarted(object sender, EventArgs e)
        {
            try
            {
                DatabaseService dbService = _databaseServices[(ScheduledEvent)sender];
                lock (_processingLock)
                {
                    if (_running &&
                        !_stoppedEvent.WaitOne(0) &&
                        dbService.Enabled &&
                        !dbService.Schedule.GetIsInExcludedTime())
                    {
                        dbService.Process(_canceller.Token);
                    }
                }
            }
            catch (Exception ex)
            {
                new ExtractException("ELI45414", "ETL process failure", ex).Log();
            }
        }

        #endregion Event Handlers

        #region Private Members

        /// <summary>
        /// Attempts to start the services. If another ESFAMService instance is already running
        /// against the database, it will continue to check every 5 minutes to see if this
        /// instance should start running the services.
        /// </summary>
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
                    "   WHERE [FPSFileName] = 'ETL'").Any())
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
                    _fileProcessingDb.RecordFAMSessionStart("ETL", "", false, false);
                    _fileProcessingDb.RegisterActiveFAM();

                    _running = true;

                    using (DataTable dbServiceDefinitions = DBMethods.ExecuteDBQuery(
                                _oleDbConnection, "SELECT [ID], [Settings] FROM [DatabaseService] WHERE Enabled = 1"))
                    {
                        foreach (DataRow dbServiceRow in dbServiceDefinitions.Rows)
                        {
                            var dbService = DatabaseService.FromJson((string)dbServiceRow["Settings"]);

                            dbService.DatabaseServiceID = (int)dbServiceRow["ID"];
                            dbService.DatabaseServer = _oleDbConnection.DataSource;
                            dbService.DatabaseName = _oleDbConnection.Database;

                            dbService.Schedule.EventStarted += HandleScheduleEvent_EventStarted;
                            _databaseServices.TryAdd(dbService.Schedule, dbService);
                        }
                    }
                }

                if (!_databaseServices.Any())
                {
                    Stop();
                }

                // _oleDbConnection only needed until the service has started.
                _oleDbConnection.Dispose();
                _oleDbConnection = null;
            }
            catch (Exception ex)
            {
                new ExtractException("ELI45415", "ETL failed to start", ex).Log();
            }
        }

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

        #endregion Private Members
    }
}
