using Extract.ETL;
using Extract.FileActionManager.Database;
using Extract.Licensing;
using Extract.Utilities;
using FAMProcessLib;
using Microsoft.Win32.SafeHandles;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using UCLID_FILEPROCESSINGLib;

namespace Extract.FileActionManager.Utilities
{
    /// <summary>
    /// Messages that can be handled when sent to the named pipe
    /// </summary>
    public enum RequestMessage
    {
        None = 0,

        /// <summary>
        /// Get array of identifiers for processes that this process has created and not destroyed
        /// </summary>
        GetSpawnedProcessIDs = 1
    }

    /// <summary>
    /// Class that manages the FAM service start/stop and FAMProcess.exe instances.
    /// </summary>
    sealed partial class ESFAMService : ServiceBase
    {
        #region Private Internal Class

        /// <summary>
        /// Internal class used to pass multiple parameters to the thread function.
        /// </summary>
        class ProcessingThreadArguments
        {
            /// <summary>
            /// The fps file to process.
            /// </summary>
            readonly string _fpsFileName;

            /// <summary>
            /// The number of files to process before respawning the FAM instance.
            /// </summary>
            readonly int _numberOfFilesToProcess;

            /// <summary>
            /// Initializes a new instance of the <see cref="ProcessingThreadArguments"/> class.
            /// </summary>
            /// <param name="fpsFileName">The fps file to process.</param>
            /// <param name="numberOfFilesToProcess">The number of files to process for
            /// this processing thread, before respawning the FAM process.</param>
            public ProcessingThreadArguments(string fpsFileName, int numberOfFilesToProcess)
            {
                _fpsFileName = fpsFileName;
                _numberOfFilesToProcess = numberOfFilesToProcess;
            }

            /// <summary>
            /// Gets the fps file name.
            /// </summary>
            public string FpsFileName
            {
                get
                {
                    return _fpsFileName;
                }
            }

            /// <summary>
            /// Gets the number of files to process
            /// </summary>
            public int NumberOfFilesToProcess
            {
                get
                {
                    return _numberOfFilesToProcess;
                }
            }
        }

        #endregion Private Internal Class

        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(ESFAMService).ToString();

        /// <summary>
        /// The Maximum time to wait for a process to exit after processing has stopped
        /// </summary>
        const int _MAX_WAIT_FOR_PROCESS_EXIT = 30000;

        #endregion Constants

        #region Enums

        /// <summary>
        /// Enum for the process wait result when waiting for a FAMProcess to exit and a stop request
        /// </summary>
        enum ProcessWaitResult
        {
            /// <summary>
            /// Indicates the _stopProcessing is signaled
            /// </summary>
            StopRequested = 0,

            /// <summary>
            /// Indicates the process exited
            /// </summary>
            ProcessExited = 1,

            /// <summary>
            /// Indicates the wait timed out
            /// </summary>
            ProcessTimeOut = WaitHandle.WaitTimeout
        }

        #endregion

        #region Fields

        /// <summary>
        /// Path to the database that contains the FAM service settings.
        /// </summary>
        // Store the database parallel to the ESFAMService.exe file [DNRCAU #381]
        string _databaseFile;

        /// <summary>
        /// Allow for custom service name
        /// </summary>
        string _serviceName;

        /// <summary>
        /// Event to indicate processing should stop.
        /// </summary>
        ManualResetEvent _stopProcessing = new ManualResetEvent(false);

        /// <summary>
        /// Event to indicate the threads may start (set to true by the sleep time thread)
        /// </summary>
        ManualResetEvent _startThreads = new ManualResetEvent(false);

        /// <summary>
        /// Event handle that is set when all threads have stopped
        /// </summary>
        CountdownEvent _threadsStopped;

        /// <summary>
        /// The count of processing threads
        /// </summary>
        volatile int _threadCount;

        /// <summary>
        /// The count of threads that required authentication to run.
        /// </summary>
        volatile int _threadsThatRequireAuthentication;

        /// <summary>
        /// List of <see cref="DatabaseServiceManager"/> to run against the database.
        /// </summary>
        readonly ConcurrentBag<DatabaseServiceManager> _databaseServiceManagers = new ConcurrentBag<DatabaseServiceManager>();

        /// <summary>
        /// Keep track of spawned processes so that they can be killed if needed
        /// </summary>
        readonly ConcurrentDictionary<int, int> _spawnedProcessIDs = new ConcurrentDictionary<int, int>();

        /// <summary>
        /// Mutex to provide synchronized access to data.
        /// </summary>
        readonly object _lock = new object();

        /// <summary>
        /// Database server to run the configured ETL processes
        /// </summary>
        string _etlDatabaseServer;

        /// <summary>
        /// Name of the database to run configured ETL processes 
        /// </summary>
        string _etlDatabaseName;

        /// <summary>
        /// List of the enabled services from the database that have DatabaseServiceManagers records in _databaseServiceManagers
        /// </summary>
        List<string> _listOfEnabledServices;

        /// <summary>
        /// FAM database used by the ETL Polling
        /// </summary>
        FileProcessingDB _etlFAMDB;

        /// <summary>
        /// Last time the ETL Process was started
        /// </summary>
        DateTime _lastETLStart;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ESFAMService"/> class.
        /// </summary>
        public ESFAMService()
        {
            InitializeComponent();
        }

        #endregion Constructors

        #region ServiceBase Overrides

        /// <summary>
        /// Called when the service starts.
        /// </summary>
        /// <param name="args">The arguments passed to the service.</param>
        protected override void OnStart(string[] args)
        {
            try
            {
                // Validate the license on startup
                LicenseUtilities.ValidateLicense(LicenseIdName.FlexIndexIDShieldCoreObjects, "ELI28495",
                    _OBJECT_NAME);

                if (TryGetServiceName(out _serviceName))
                {
                    _databaseFile = GetDatabaseFileName(_serviceName);
                }
                else
                {
                    throw new ExtractException("ELI50266", "Could not determine service name");
                }

                var dbManager = new FAMServiceDatabaseManager(_databaseFile);

                // Validate the service database schema
                int schemaVersion = dbManager.GetSchemaVersion();
                if (schemaVersion != FAMServiceDatabaseManager.CurrentSchemaVersion)
                {
                    ExtractException ee = new ExtractException("ELI29802",
                        dbManager.IsUpdateRequired ? "Service database must be updated to current schema."
                        : "Invalid service database schema version.");
                    ee.AddDebugData("Current Supported Schema Version",
                        FAMServiceDatabaseManager.CurrentSchemaVersion, false);
                    ee.AddDebugData("Database Schema Version", schemaVersion, false);
                    throw ee;
                }

                // Get the list of FPS file processing arguments from the database
                List<ProcessingThreadArguments> processingFileArguments =
                    GetFpsFileProcessingArguments(dbManager);

                // Get just the fps files
                var fpsFileArguments = processingFileArguments
                    .Where(f => f.FpsFileName.EndsWith(".fps", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                // [DNRCAU #357] - Log application trace when service is starting
                ExtractException ee2 = new ExtractException("ELI28772", UtilityMethods.FormatInvariant(
                    $"Application trace: FAM Service starting. ({_serviceName})"));
                ee2.Log();

                // Create the sleep thread and start it
                new Thread(SleepAndCheckDependentServices).Start(dbManager);

                lock (_lock)
                {
                    _threadCount = fpsFileArguments.Count;
                    _threadsStopped = new CountdownEvent(_threadCount);
                }

                // Create and launch a processing thread for each fps file.
                foreach (var threadData in fpsFileArguments)
                {
                    // Create a new thread and add it to the collection.
                    Thread thread = new Thread(ProcessFiles);

                    // Start the thread in a Multithreaded apartment
                    thread.SetApartmentState(ApartmentState.MTA);
                    thread.Start(threadData);
                }
                StartETL(dbManager);
                CreatePipeListenerThread();
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI28496", ex);
                ee.AddDebugData("ServiceName", _serviceName);
                ee.Log();
                throw ee;
            }
        }

        bool WaitForConnection(NamedPipeServerStream pipeStream)
        {

            Exception waitForConnectionException = null;
            using AutoResetEvent connectedEvent = new AutoResetEvent(false);
            pipeStream.BeginWaitForConnection(asyncResult =>
            {
                try
                {
                    pipeStream.EndWaitForConnection(asyncResult);
                    connectedEvent.Set();
                }
                catch (Exception ex)
                {
                    waitForConnectionException = ex;
                }

            }, null);

            if (WaitHandle.WaitAny(new WaitHandle[] { connectedEvent, _stopProcessing}) == 1)
            {
                // Stop waiting if Stop has been called
                pipeStream.Close();
                return false;
            }

            if (waitForConnectionException != null)
            {
                throw waitForConnectionException.AsExtract("ELI51509");
            }

            return true;
        }

        void CreatePipeListenerThread()
        {
            new Thread(() =>
            {
                try
                {
                    var pipeName = UtilityMethods.FormatInvariant($"ESFAMServicePipe_{Process.GetCurrentProcess().Id}");
                    using var pipeStream =
                        new NamedPipeServerStream(pipeName, PipeDirection.InOut, 50, PipeTransmissionMode.Message, PipeOptions.Asynchronous);

                    if (WaitForConnection(pipeStream))
                    {
                        CreatePipeListenerThread(); // Listen for new connections

                        StringBuilder messageBuilder = new StringBuilder();
                        byte[] messageBuffer = new byte[16];
                        do
                        {
                            var bytesRead = pipeStream.Read(messageBuffer, 0, messageBuffer.Length);
                            var messageChunk = Encoding.UTF8.GetString(messageBuffer, 0, bytesRead);
                            messageBuilder.Append(messageChunk);
                        }
                        while (!pipeStream.IsMessageComplete);

                        var msg = JsonConvert.DeserializeObject<RequestMessage>(messageBuilder.ToString());

                        if (msg == RequestMessage.GetSpawnedProcessIDs)
                        {
                            var serialized = JsonConvert.SerializeObject(_spawnedProcessIDs.Keys);
                            var bytes = Encoding.UTF8.GetBytes(serialized);
                            pipeStream.Write(bytes, 0, bytes.Length);
                            pipeStream.WaitForPipeDrain();
                        }
                    }
                }
                catch (Exception ex)
                {
                    ex.ExtractLog("ELI51483");
                }
            }).Start();
        }

        /// <summary>
        /// Called when the service is stopping.
        /// </summary>
        protected override void OnStop()
        {
            try
            {
                StopService("ELI28773", true);
            }
            catch (Exception ex)
            {
                ExtractException.Log("ELI29809", ex);
            }
            finally
            {
                // Call the base class OnStop method
                base.OnStop();
            }
        }

        /// <summary>
        /// Called when the system is shutting down
        /// </summary>
        protected override void OnShutdown()
        {
            try
            {
                StopService("ELI29822", false);
            }
            catch (Exception ex)
            {
                ExtractException.Log("ELI29811", ex);
            }
            finally
            {
                // Call the base class OnShutdown method
                base.OnShutdown();
            }
        }

        #endregion ServiceBase Overrides

        #region Thread Methods

        /// <summary>
        /// Thread function that sleeps on initial startup and then sets the startThreads event.
        /// </summary>
        void SleepAndCheckDependentServices(object serviceDBManager)
        {
            try
            {
                var dbManager = serviceDBManager as FAMServiceDatabaseManager;
                if (dbManager == null)
                {
                    throw new ArgumentException(
                        "Specified db manager is either null or not a FAMServiceDatabaseManager.");
                }

                // [DotNetRCAndUtils:858]
                // Only honor SleepTimeOnStartup during the first 15 minutes of machine up time.
                if (SystemMethods.SystemUptime.TotalMinutes < 15)
                {
                    int sleepTime = int.Parse(dbManager.Settings[FAMServiceDatabaseManager.SleepTimeOnStartupKey],
                        CultureInfo.InvariantCulture);
                    if (sleepTime <= 0)
                    {
                        var ee = new ExtractException("ELI31139", "Sleep time on startup must be > 0.");
                        ee.AddDebugData("Sleep Time", sleepTime, false);
                        throw ee;
                    }

                    _stopProcessing.WaitOne(sleepTime);
                }
                else
                {
                    var ee = new ExtractException("ELI35295", "Application trace: SleepTimeOnStart "
                        + "ignored since system is not in the process of starting up.");
                    ee.AddDebugData("System uptime", SystemMethods.SystemUptime, false);
                    ee.Log();
                }

                // Get the collection of ServiceController's for the dependent services
                List<ServiceController> dependentServices = GetDependentServiceControllers(dbManager);

                // Check if the dependent services have started.
                // Keep checking until either:
                // 1. All dependent services have started
                // 2. The service is stopped
                bool allStartedAfterSleep = true;
                while (dependentServices.Count > 0
                    && _stopProcessing != null && !_stopProcessing.WaitOne(0))
                {
                    // Check for running services
                    List<string> serviceNotStarted = new List<string>();
                    for (int i = 0; i < dependentServices.Count; i++)
                    {
                        // Get the service controller and refresh its status
                        ServiceController controller = dependentServices[i];
                        controller.Refresh();

                        // Check for running service
                        if (controller.Status != ServiceControllerStatus.Running)
                        {
                            if (allStartedAfterSleep)
                            {
                                serviceNotStarted.Add(controller.DisplayName);
                            }
                        }
                        else
                        {
                            // Dispose of the controller and remove it from the collection
                            controller.Dispose();
                            dependentServices.RemoveAt(i);
                            i--;
                        }
                    }

                    // Log an exception if any services have not started yet (only log on
                    // the first iteration)
                    if (allStartedAfterSleep && serviceNotStarted.Count > 0)
                    {
                        allStartedAfterSleep = false;

                        string services = string.Join(", ", serviceNotStarted);
                        ExtractException ee = new ExtractException("ELI29147",
                            "Application Trace: Waiting for dependent services to start.");
                        ee.AddDebugData("Services", services, false);
                        ee.Log();
                    }

                    // If all services started, just break from the loop
                    if (dependentServices.Count == 0)
                    {
                        break;
                    }

                    // Sleep for 1 second and check dependent services again.
                    Thread.Sleep(1000);
                }

                // If all dependent services have not started and stop has been called
                // log an exception listing which services have not started yet.
                // [DNRCAU #385]
                if (dependentServices.Count > 0
                    && _stopProcessing != null && _stopProcessing.WaitOne(0))
                {
                    StringBuilder sb =
                        new StringBuilder(dependentServices[0].DisplayName);
                    for (int i = 1; i < dependentServices.Count; i++)
                    {
                        sb.Append(", ");
                        sb.Append(dependentServices[i].DisplayName);
                    }

                    ExtractException ee = new ExtractException("ELI29559",
                        "Application Trace: Service stopped before all dependent services started.");
                    ee.AddDebugData("Services", sb.ToString(), false);
                    throw ee;
                }

                // Check if all the services had started after the sleep time
                // Only log the application trace if the FAM is actually going to start
                if (!allStartedAfterSleep
                    && _stopProcessing != null && !_stopProcessing.WaitOne(0))
                {
                    // Log an application trace since after the sleep time not all services
                    // had started.
                    ExtractException ee = new ExtractException("ELI29148",
                        "Application Trace: All dependent services are running and File Action Manager service will now begin processing.");
                    ee.Log();
                }

                // Dispose of the service controllers
                CollectionMethods.ClearAndDispose(dependentServices);
            }
            catch (ThreadAbortException)
            {
                // Do not log any exception if the thread was aborted
            }
            catch (Exception ex)
            {
                ExtractException.Log("ELI29139", ex);
            }
            finally
            {
                if (_startThreads != null)
                {
                    _startThreads.Set();
                }
            }
        }

        /// <summary>
        /// Thread function that handles launching the FAMProcess.
        /// </summary>
        /// <param name="threadParameters">The <see cref="ProcessingThreadArguments"/>
        /// for this thread.</param>
        void ProcessFiles(object threadParameters)
        {
            FileProcessingManagerProcessClass famProcess = null;
            Process process = null;
            string fpsFileName = string.Empty;
            int processID = 0;
            bool keepProcessing = false;
            try
            {
                // Get the processing thread arguments
                ProcessingThreadArguments arguments = (ProcessingThreadArguments)threadParameters;
                int numberOfFilesToProcess = arguments.NumberOfFilesToProcess;

                // Log Application to indicate this thread is starting.
                ExtractException eeStarted = new ExtractException("ELI39766", "Application Trace: Service FAMProcess thread has started.");
                eeStarted.AddDebugData("ThreadID", Thread.CurrentThread.ManagedThreadId, false);
                eeStarted.Log();

                if (!File.Exists(arguments.FpsFileName))
                {
                    var ee = new ExtractException("ELI32302", "Cannot find FPS file.");
                    ee.AddDebugData("FPS File Name", arguments.FpsFileName, false);
                    throw ee;
                }

                // Wait for the sleep time to expire
                if (_startThreads != null)
                {
                    _startThreads.WaitOne();
                }

                // Flag that will be set to true to indicate initialization completed, so the loop
                // to create the process will retry
                bool processInitialized;

                // Run FAMProcesses in a loop respawning them if needed
                do
                {
                    processInitialized = false;
                    try
                    {
                        // Create the FAM process
                        famProcess = new FileProcessingManagerProcessClass();
                        processID = famProcess.ProcessID;
                        _spawnedProcessIDs.TryAdd(processID, processID);
                        process = Process.GetProcessById(processID);

                        // Provide the service authentication
                        famProcess.AuthenticateService(LicenseUtilities.GetMapLabelValue(new MapLabel()));

                        // Set the FPS file name
                        fpsFileName = arguments.FpsFileName;
                        famProcess.FPSFile = fpsFileName;

                        // Check if authentication is required
                        if (famProcess.AuthenticationRequired)
                        {
                            // Do not keep trying to start process since it requires authentication
                            // which requires a configuration change
                            processInitialized = true;

                            // Mutex around incrementing _threadsThatRequireAuthentication
                            lock (_lock)
                            {
                                _threadsThatRequireAuthentication++;
                            }

                            ExtractException ee = new ExtractException("ELI29208",
                                "Authentication is required to launch this FPS file.");
                            ee.AddDebugData("FPS File Name", fpsFileName, false);
                            throw ee;
                        }

                        // [LegacyRCAndUtils:6389]
                        // Check for whether the process wants to keep processing now rather than after
                        // it is done with this batch of files, when it is not at risk of having been
                        // shut down unexpectedly.
                        keepProcessing = famProcess.KeepProcessingAsFilesAdded;
                        bool isProcessingEnabled = famProcess.IsProcessingEnabled;

                        // Everything should be initialized now
                        processInitialized = true;

                        // Log an exception if keepProcessing is not enabled since it is not a 
                        // recommended setting when running fps file in a service
                        // https://extract.atlassian.net/browse/ISSUE-2039
                        if (isProcessingEnabled && !keepProcessing)
                        {
                            ExtractException keepProcessingEx = new ExtractException("ELI38370",
                                "Application Trace: Inadvisable configuration -- It is not recommended to " +
                                "configure a FPS file used by a service to stop processing once the queue is empty.");
                            keepProcessingEx.AddDebugData("FPSFile", fpsFileName, false);
                            keepProcessingEx.Log();
                        }

                        // Ensure that processing has not been stopped before starting processing
                        if (_stopProcessing != null && !_stopProcessing.WaitOne(0))
                        {
                            // Start processing
                            famProcess.Start(numberOfFilesToProcess);

                            ExtractException ee = new ExtractException("ELI29808",
                                "Application trace: Started new FAM instance.");
                            ee.AddDebugData("FPS Filename", fpsFileName, false);
                            ee.AddDebugData("Process ID", processID, false);
                            ee.AddDebugData("Number Of Files To Process", numberOfFilesToProcess, false);
                            ee.Log();
                        }

                        // Wait until the process has exited, a stop is requested or processing has stopped
                        ProcessWaitResult waitResult = new ProcessWaitResult();
                        do
                        {
                            waitResult = WaitForProcessExitOrStopRequest(process, 2000);
                        }
                        while (waitResult == ProcessWaitResult.ProcessTimeOut && famProcess.IsRunning);

                        // [LegacyRCAndUtils:6367]
                        // If stop processing has been called, don't bother to check whether famProcess
                        // has ended naturally because we already know we need the process to stop.
                        if (waitResult == ProcessWaitResult.StopRequested)
                        {
                            break;
                        }

                        // [DotNetRCAndUtils:835]
                        // Avoid calling ProcessShouldRestart when the FAM process is configured to keep
                        // processing as files are added to ensure that processing does not stop.
                        if (!keepProcessing)
                        {
                            // [LegacyRCAndUtils:6389]
                            // Safely check the running process to see if it should be restarted.
                            // If any exceptions are caught, they are logged rather than thrown to ensure
                            // processing doesn't stop unexpectedly. If the FAMProcess was supposed to
                            // stop because the queue was empty, this should not have any ill effects as
                            // the queue will be empty and the new process will get another chance to
                            // exit cleanly.
                            keepProcessing = ProcessShouldRestart(famProcess, process, numberOfFilesToProcess, fpsFileName);
                        }

                        // Safely close the running process (any exceptions logged rather than thrown).
                        CloseProcess(ref famProcess, ref process);
                    }
                    catch (Exception ex)
                    {
                        if (ex.Message.Contains("The remote procedure call failed") ||
                            ex.Message.Contains("The RPC server is unavailable."))
                        {
                            ExtractException ee = new ExtractException("ELI29823",
                                "Unable to communicate with the underlying FAM process.", ex);
                            ee.Log();
                        }
                        else
                        {
                            // Just log any exceptions from the process threads
                            ExtractException.Log("ELI28498", ex);
                        }
                    }
                    finally
                    {
                        if (process != null)
                        {
                            // Wrap in a try/catch and log to ensure no exceptions thrown
                            // from the finally block.
                            try
                            {
                                // Ensure stop has been called on the fam process
                                if (!process.HasExited && famProcess != null && famProcess.IsRunning)
                                {
                                    famProcess.Stop();
                                }

                                // Wait for the famprocess to stop
                                while (!process.HasExited && famProcess != null && famProcess.IsRunning)
                                {
                                    Thread.Sleep(1000);
                                }

                                // [DNRCAU #876] 
                                // Release the COM object so the FAMProcess.exe is cleaned up
                                Marshal.FinalReleaseComObject(famProcess);

                                // Perform a GC to force the object to clean up
                                GC.Collect();
                                GC.WaitForPendingFinalizers();
                            }
                            catch (Exception ex)
                            {
                                ExtractException.Log("ELI28505", ex);
                            }
                            finally
                            {
                                WaitForProcessToExitAndDispose(ref process, fpsFileName);
                                famProcess = null;
                                _spawnedProcessIDs.TryRemove(processID, out var _);
                            }
                        }
                    }
                    // Log Application trace to show that the FAM instance is stopped
                    ExtractException eeClosed =
                        new ExtractException("ELI39764", "Application Trace: FAM instance has stopped.");
                    eeClosed.AddDebugData("FPS Filename", fpsFileName, false);
                    eeClosed.AddDebugData("Process ID", processID, false);
                    eeClosed.AddDebugData("Number Of Files To Process", numberOfFilesToProcess, false);
                    eeClosed.Log();
                }
                while ((keepProcessing || !processInitialized) && _stopProcessing != null && !_stopProcessing.WaitOne(0));
            }
            catch (Exception ex)
            {
                // Just log any exceptions from the process threads
                ExtractException.Log("ELI40192", ex);
            }
            finally
            {
                // [DotNetRCAndUtils:695]
                // Stop will require the ProcessFiles thread to lock _lock, so don't call Stop from
                // within the lock; Only check if we should stop within the lock.
                bool stopService = false;

                // Mutex around the active thread count decrement
                lock (_lock)
                {
                    if (_threadsStopped != null)
                    {
                        _threadsStopped.Signal();
                    }

                    // Check if all threads failed to launch because they required authentication
                    if (_threadsThatRequireAuthentication == _threadCount)
                    {
                        // All threads failed due to authentication requirement
                        // call Stop to stop the service
                        stopService = true;
                    }
                }

                // Log Application trace exception to indicate this thread is exiting.
                ExtractException eeThreadExited = new ExtractException("ELI39765", "Application Trace: Service FAMProcess thread has exited.");
                eeThreadExited.AddDebugData("ThreadID", Thread.CurrentThread.ManagedThreadId, false);
                eeThreadExited.Log();

                if (stopService)
                {
                    Stop();
                }
            }
        }

        /// <summary>
        /// Makes sure the process has exited, if not kills it after 1 minute and then disposes of the process
        /// </summary>
        /// <param name="process">The process to wait for exit</param>
        /// <param name="fpsFileName">The FPS file that is being processed</param>
        static void WaitForProcessToExitAndDispose(ref Process process, string fpsFileName)
        {
            try
            {
                if (process == null)
                {
                    return;
                }

                // Wait for the process to exit
                // Wait for up to a minute for the process to stop
                int i = 0;
                while (!process.HasExited && i < 60)
                {
                    Thread.Sleep(1000);
                    i++;
                }

                // If after a minute the process has not exited, then kill it
                if (!process.HasExited)
                {
                    try
                    {
                        var ee = new ExtractException("ELI35308", "Killing hung process.");
                        ee.AddDebugData("FPS Filename", fpsFileName, false);
                        ee.AddDebugData("Process ID", process.Id, false);
                        ee.Log();
                        process.Kill();
                    }
                    catch (Exception ex)
                    {
                        ExtractException.Log("ELI30038", ex);
                    }
                }
            }
            catch (Exception ex)
            {
                ExtractException.Log("ELI38365", ex);
            }
            finally
            {
                process.Dispose();
                process = null;
            }
        }

        /// <summary>
        /// Checks if the specified <see paramref="famProcess"/> needs to be restarted.
        /// </summary>
        /// <param name="famProcess">The <see cref="FileProcessingManagerProcessClass"/>.</param>
        /// <param name="process">The <see cref="Process"/> <see paramref="famProcess"/> is running
        /// in.</param>
        /// <param name="numberOfFilesToProcess">The number of files to process for
        /// this processing thread, before respawning the FAM process.</param>
        /// <param name="fpsFileName">Name of FPS file that is running.</param>
        /// <returns><see langword="true"/> if the process should be restarted, otherwise, 
        /// <see langword="false"/>.</returns>
        static bool ProcessShouldRestart(FileProcessingManagerProcessClass famProcess,
            Process process, int numberOfFilesToProcess, string fpsFileName)
        {
            try
            {
                // Get the count of files processed (if limiting processing to a
                // specified number of files)
                int filesProcessed = 0;
                if (numberOfFilesToProcess != 0 && !process.HasExited && famProcess != null)
                {
                    int processedSuccessfully;
                    int processingErrors;
                    int filesSupplied;
                    int supplyingErrors;
                    famProcess.GetCounts(out processedSuccessfully, out processingErrors,
                        out filesSupplied, out supplyingErrors);
                    filesProcessed = processedSuccessfully + processingErrors;
                }

                // If the number of files to process is 0 OR the number of files actually
                // processed is less than the number of files specified, then just
                // exit the loop and do not respawn a new FAM instance
                if (numberOfFilesToProcess == 0 || filesProcessed < numberOfFilesToProcess)
                {
                    // Log exception so users can figure out why processing did not restart
                    // fpsFileName is used instead of famProcess.FPSFile because famProcess could be 
                    // unavailable if it has exited
                    // https://extract.atlassian.net/browse/ISSUE-2039
                    ExtractException ee = new ExtractException("ELI38371",
                        "Application Trace: Service FPS file has stopped processing and will not be restarted.");
                    ee.AddDebugData("FPSFile", fpsFileName, false);
                    ee.AddDebugData("Reason",
                        "FPS file set to stop once the queue is empty and the queue was empty " +
                        "or the FAMProcess terminated unexpectedly.", false);
                    ee.Log();
                    return false;
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("The remote procedure call failed") ||
                    ex.Message.Contains("The RPC server is unavailable."))
                {
                    ExtractException ee = new ExtractException("ELI35378",
                        "Unable to communicate with the underlying FAM process.", ex);
                    ee.Log();
                }
                else
                {
                    // Just log any exceptions from the process threads
                    ExtractException.Log("ELI35379", ex);
                }
            }

            return true;
        }

        /// <summary>
        /// Safely closes the specified <see paramref="famProcess"/>. Any exceptions are logged
        /// rather than thrown.
        /// </summary>
        /// <param name="famProcess">The <see cref="FileProcessingManagerProcessClass"/>.</param>
        /// <param name="process">The <see cref="Process"/> <see paramref="famProcess"/> is running
        /// in.</param>
        void CloseProcess(ref FileProcessingManagerProcessClass famProcess, ref Process process)
        {
            try
            {
                // Release the current FAM process before looping around and spawning a new one
                // this way the memory is released and we are not leaving extra orphaned
                // EXE's lying around.
                // If the process.HasExited  is true famProcess EXE has already exited and if the it
                // has exited the call to FinalReleaseComObject will throw an exception
                if (!process.HasExited && famProcess != null) // Sanity check, it should never be null at this point
                {
                    // [DNRCAU #429, 876] 
                    // Force release the COM object so the EXE will exit
                    Marshal.FinalReleaseComObject(famProcess);

                    GC.Collect();
                    GC.WaitForPendingFinalizers();

                    WaitForProcessExitOrStopRequest(process, _MAX_WAIT_FOR_PROCESS_EXIT);
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("The remote procedure call failed") ||
                    ex.Message.Contains("The RPC server is unavailable."))
                {
                    ExtractException ee = new ExtractException("ELI35380",
                        "Unable to communicate with the underlying FAM process.", ex);
                    ee.Log();
                }
                else
                {
                    // Just log any exceptions from the process threads
                    ExtractException.Log("ELI35381", ex);
                }
            }
            finally
            {
                // If there is a valid process handle for an exited process, dispose of it
                if (process != null)
                {
                    process.Dispose();
                }

                process = null;
                famProcess = null;
            }
        }

        /// <summary>
        /// Method waits on the given process to exit or the _stopProcessing event to signal.
        /// This should only be called after processing has stopped and the process should be
        /// exiting.
        /// </summary>
        /// <param name="process">The process to wait for exit</param>
        /// <param name="timeOut">The number of milliseconds to wait</param>
        /// <returns>
        /// 	<see cref="ProcessWaitResult.StopRequested"/> if _stopProcessing is signaled,
        /// 	<see cref="ProcessWaitResult.ProcessExited"/> if process has exited
        ///		<see cref="ProcessWaitResult.ProcessTimeOut"/> if _stopProcessing has not been signaled and process has not exited
		/// </returns>
        ProcessWaitResult WaitForProcessExitOrStopRequest(Process process, int timeOut)
        {
            if (process == null || process.HasExited)
            {
                return (_stopProcessing.WaitOne(0)) ?
                    ProcessWaitResult.StopRequested : ProcessWaitResult.ProcessExited;
            }

            // Wait for the process to exit
            using (SafeWaitHandle safeWaitHandleForProcess = new SafeWaitHandle(process.Handle, false))
            using (ManualResetEvent processExitEvent = new ManualResetEvent(true))
            {
                processExitEvent.SafeWaitHandle = safeWaitHandleForProcess;

                // _stopProcessing is first because if both are signaled the lowest index will be 
                // returned from the WaitAny
                WaitHandle[] waitHandleList = new WaitHandle[] { _stopProcessing, processExitEvent };

                return (ProcessWaitResult)WaitHandle.WaitAny(waitHandleList, timeOut);
            }
        }

        #endregion Thread Methods

        #region Methods

        /// <summary>
        /// Checks for ETL processes in the <see cref="FAMServiceDatabaseManager"/> and starts any that are configured.
        /// A polling thread will also be started that will check periodically if there are changes in the configured Database
        /// </summary>
        /// <param name="dbManager"></param>
        void StartETL(FAMServiceDatabaseManager dbManager)
        {
            try
            {
                // Get just the unique ETL processes
                var etlArguments = dbManager.GetFpsFileData(true)
                    .Where(d => d.FileName.StartsWith("ETL", StringComparison.OrdinalIgnoreCase))
                    .GroupBy(g => g.FileName)
                    .Select(g => g.First())
                    .ToList();

                // Get the ETL database and server if configured
                dbManager.Settings.TryGetValue("DatabaseServer", out _etlDatabaseServer);
                dbManager.Settings.TryGetValue("DatabaseName", out _etlDatabaseName);

                // This is called here so that it will open the the database - if it fails it will throw an exception
                // https://extract.atlassian.net/browse/ISSUE-16841
                _listOfEnabledServices = GetListOfEnabledServicesFromDB();

                // check for database settings for ETL
                if (etlArguments.Any())
                {
                    if (string.IsNullOrEmpty(_etlDatabaseServer) || string.IsNullOrEmpty(_etlDatabaseName))
                    {
                        ExtractException etlException = new ExtractException("ELI46269",
                            "ETL processes are configured to run in the service but no database is configured in Settings.");
                        etlException.AddDebugData("DatabaseServer", _etlDatabaseServer ?? string.Empty, false);
                        etlException.AddDebugData("DatabaseName", _etlDatabaseName ?? string.Empty, false);
                        throw etlException;
                    }

                    // Start the DatabaseServiceManagers
                    List<string> etlProcesses = etlArguments.Select(e => e.FileName).ToList();
                    new Thread(() =>
                    {
                        try
                        {
                            // if _stopProcessing is set return without starting the DatabaseServiceManagers
                            if (WaitHandle.WaitAny(new WaitHandle[] { _startThreads, _stopProcessing }) == 1)
                            {
                                return;
                            }

                            foreach (var etl in etlArguments)
                            {
                                string etlName = etl.FileName.Replace("ETL:", string.Empty).Trim();
                                if (etl.FileName == "ETL" || _listOfEnabledServices
                                    .Select(s => DatabaseService.FromJson(s).Description)
                                    .Contains(etlName, StringComparer.CurrentCultureIgnoreCase))
                                {
                                    _databaseServiceManagers.Add(
                                        new DatabaseServiceManager(etl.FileName, _etlDatabaseServer, _etlDatabaseName, etlProcesses, etl.NumberOfInstances));
                                }
                            }
                        }
                        catch (ThreadAbortException)
                        {
                        // Don't log or throw ThreadAbortException
                        }
                        catch (Exception ex)
                        {
                            ex.ExtractLog("ELI46288");
                        }
                    }).Start();
                }

                _lastETLStart = DateTime.Now;

                // if the etlDatabaseServer and etlDatabaseName is not configured there is no way to poll for ETL changes
                if (!string.IsNullOrEmpty(_etlDatabaseServer) && !string.IsNullOrEmpty(_etlDatabaseName))
                {
                    new Thread(() =>
                    {
                        // if _stopProcessing is set return without starting the DatabaseServiceManagers
                        if (WaitHandle.WaitAny(new WaitHandle[] { _startThreads, _stopProcessing }) == 1)
                        {
                            return;
                        }

                        if (_etlFAMDB is null || _etlFAMDB.DatabaseServer != _etlDatabaseServer || _etlFAMDB.DatabaseName != _etlDatabaseName)
                        {
                            if (_etlFAMDB != null)
                            {
                                _etlFAMDB.UnregisterActiveFAM();
                                _etlFAMDB.RecordFAMSessionStop();
                                _etlFAMDB.CloseAllDBConnections();
                                _etlFAMDB = null;
                            }
                            _etlFAMDB = new FileProcessingDB();
                            _etlFAMDB.DatabaseName = _etlDatabaseName;
                            _etlFAMDB.DatabaseServer = _etlDatabaseServer;
                        }

                        // Changed this from using a timer to a thread because timer
                        // would fire on different thread and cause lots of connections 
                        // to be created in _etlFAMDB
                        // https://extract.atlassian.net/browse/ISSUE-16675
                        new Thread(() =>
                        {

                            do
                            {
                                try
                                {
                                    // Check the FAM still active
                                    if (_etlFAMDB.ActiveFAMID == 0 && _etlFAMDB.IsConnected)
                                    {
                                        ExtractException lostActiveFAM = new ExtractException("ELI46691", "Application Trace: ETL Polling ActiveFAM was lost");
                                        lostActiveFAM.Log();
                                        _etlFAMDB.RegisterActiveFAM();
                                        ExtractException restroredActiveFAM = new ExtractException("ELI46692", "Application Trace: ETL Polling ActiveFAM was restored.");
                                        restroredActiveFAM.Log();
                                    }
                                    using var connection = GetSQLConneciton();
                                    connection.Open();
                                    
                                    using var cmd = connection.CreateCommand();
                                    cmd.CommandText = "SELECT Value FROM DBInfo WHERE Name = 'ETLRestart'";
                                    string restartSetting = cmd.ExecuteScalar() as string;

                                    DateTime restartTime;
                                    if (!DateTime.TryParse(restartSetting, out restartTime))
                                    {
                                        ExtractException restartInvalid = new ExtractException("ELI46424",
                                            "ETLRestart value in DBInfo should be a date time string. Updated to last valid ETL start time.");
                                        restartInvalid.AddDebugData("ETLRestart", restartSetting, false);
                                        restartInvalid.AddDebugData("LastETL start", _lastETLStart, false);
                                        
                                        // Fix the problem
                                        _etlFAMDB.SetDBInfoSetting("ETLRestart", _lastETLStart.ToString("yyyy-MM-ddTHH:mm:ss.fffzzz"), true, false);
                                        restartInvalid.Log();
                                    }

                                    if (restartTime > _lastETLStart)
                                    {
                                        StopAndRestartETL();
                                        break;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    ex.ExtractLog("ELI46302");
                                }
                            }
                            while (!_stopProcessing.WaitOne(30000));
                        }).Start();
                        _etlFAMDB.RecordFAMSessionStart("ETL Polling", string.Empty, false, false);
                        _etlFAMDB.RegisterActiveFAM();
                    }).Start();
                }
            }
            catch(Exception ex)
            {
                // Debug info added to help trouble shoot ESFAMService exit
                // https://extract.atlassian.net/browse/ISSUE-16841
                var ee = ex.AsExtract("ELI49583");
                ee.AddDebugData("EtlDatabaseServer", _etlDatabaseServer);
                ee.AddDebugData("EtlDatabaseName", _etlDatabaseName);
                throw ee;
            }
        }

        /// <summary>
        /// Stops any running ETL processing, cleans up the processing and call StartETL to restart using the new settings
        /// </summary>
        void StopAndRestartETL()
        {
            // Stop the ETL services
            foreach (var dbServiceManager in _databaseServiceManagers)
            {
                dbServiceManager.Stop();
            }

            try
            {
                if (_etlFAMDB != null)
                {
                    _etlFAMDB.UnregisterActiveFAM();
                    _etlFAMDB.RecordFAMSessionStop();
                    _etlFAMDB.CloseAllDBConnections();
                    _etlFAMDB = null;
                }
            }
            catch (Exception etlFAMException)
            {
                etlFAMException.ExtractLog("ELI46309");
            }

            var stoppedEvents = _databaseServiceManagers
                .Select(mgr => mgr.StoppedWaitHandle)
                .Cast<WaitHandle>()
                .ToArray();

            if (stoppedEvents.Any())
            {
                // Wait for all the services to stop
                WaitHandle.WaitAll(stoppedEvents);
            }

            // Dispose of the DatabaseServiceManagers
            for (int i = 0; i < _databaseServiceManagers.Count; i++)
            {
                DatabaseServiceManager manager;
                if (_databaseServiceManagers.TryTake(out manager))
                {
                    manager.Dispose();
                }
            }

            // Get the Service config file
            var dbManager = new FAMServiceDatabaseManager(_databaseFile);
            StartETL(dbManager);
        }

        /// <summary>
        /// Gets a list of the Enabled DatabaseServices from the database
        /// </summary>
        /// <returns></returns>
        List<string> GetListOfEnabledServicesFromDB()
        {
            // if the Server and Database are not setup return empty list
            if (string.IsNullOrEmpty(_etlDatabaseServer) || string.IsNullOrEmpty(_etlDatabaseName))
            {
                return new List<string>();
            }

            using (var connection = GetSQLConneciton())
            {
                connection.Open();
                using (var cmd = connection.CreateCommand())
                {
                    cmd.CommandText = "SELECT Settings FROM DatabaseService WHERE Enabled = 1";
                    var result = cmd.ExecuteReader().Cast<IDataRecord>()
                        .Select(r => r.GetString(r.GetOrdinal("Settings"))).ToList();
                    result.Sort();
                    return result;
                }
            }
        }

        private SqlConnection  GetSQLConneciton()
        {

            // Build the connection string from the settings
            SqlConnectionStringBuilder sqlConnectionBuild = new SqlConnectionStringBuilder();
            sqlConnectionBuild.DataSource = _etlDatabaseServer;
            sqlConnectionBuild.InitialCatalog = _etlDatabaseName;
            sqlConnectionBuild.IntegratedSecurity = true;
            sqlConnectionBuild.NetworkLibrary = "dbmssocn";
            sqlConnectionBuild.MultipleActiveResultSets = true;
            return new SqlConnection(sqlConnectionBuild.ConnectionString);
        }

        /// <summary>
        /// Gets the list of dependent services from the service database.
        /// </summary>
        /// <returns>The list of dependent services from the service database.</returns>
        static List<string> GetDependentServices(FAMServiceDatabaseManager dbManager)
        {
            try
            {
                var names = dbManager.Settings[FAMServiceDatabaseManager.DependentServicesKey];
                var serviceNames = new HashSet<string>();
                if (!string.IsNullOrWhiteSpace(names))
                {
                    // Tokenize the list by the pipe character
                    foreach (string service in names.Split(
                        new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        serviceNames.Add(service.ToUpperInvariant().Trim());
                    }
                }

                // Return the collection
                return serviceNames.ToList();
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI29145", ex);
            }
        }

        /// <summary>
        /// Gets a <see cref="List{T}"/> of <see cref="ServiceController"/>s
        /// for each dependent service listed in the service database settings table.
        /// </summary>
        /// <param name="dbManager">The service database manager to use.</param>
        /// <returns>A collection of <see cref="ServiceController"/>s for the
        /// dependent services in the service database settings table.</returns>
        static List<ServiceController> GetDependentServiceControllers(
            FAMServiceDatabaseManager dbManager)
        {
            // Get the list of dependent service names 
            List<string> dependentServiceNames = GetDependentServices(dbManager);

            // Get the list of all services
            Dictionary<string, ServiceController> displayNames;
            Dictionary<string, ServiceController> serviceNames;
            GetCollectionOfServiceAndDisplayNames(out displayNames, out serviceNames);

            // Iterate through the list of dependent services and get the controllers
            // for each one
            List<ServiceController> dependentServices = new List<ServiceController>();
            for (int i = 0; i < dependentServiceNames.Count; i++)
            {
                string serviceName = dependentServiceNames[i];
                ServiceController controller;
                bool foundByServiceName = serviceNames.TryGetValue(serviceName, out controller);
                if (foundByServiceName || displayNames.TryGetValue(serviceName, out controller))
                {
                    // If the service was found in the dependency list, add it to the list
                    // of controllers
                    dependentServices.Add(controller);

                    // Remove the controller from the dictionaries
                    serviceNames.Remove(foundByServiceName ?
                        serviceName : controller.ServiceName.ToUpperInvariant());
                    displayNames.Remove(foundByServiceName ?
                        controller.DisplayName.ToUpperInvariant() : serviceName);

                    // Remove the service name from the dependent services list
                    // and decrement the index
                    dependentServiceNames.RemoveAt(i);
                    i--;
                }
            }

            // If there are any names left in this collection, they were not found on 
            // the current system, log an exception with this list.
            if (dependentServiceNames.Count > 0)
            {
                // Sort the list before adding it to debug data
                dependentServiceNames.Sort();
                string services = string.Join(", ", dependentServiceNames);
                ExtractException ee = new ExtractException("ELI29146",
                    "Application Trace: Services not found on current system.");
                ee.AddDebugData("Services", services, false);
                ee.Log();
            }

            // Need to dispose of all other controllers (it is sufficient to only dispose
            // of the controllers in one collection since they mirror each other and
            // contain the same references
            foreach (ServiceController controller in serviceNames.Values)
            {
                controller.Dispose();
            }

            // Clear the collections
            serviceNames.Clear();
            displayNames.Clear();

            // Return the list of ServiceControllers
            return dependentServices;
        }

        /// <summary>
        /// Builds collections of display names and service names for each service installed
        /// on the current system.
        /// </summary>
        /// <param name="displayNames">Collection of display names to
        /// the services they represent (these names are unique).</param>
        /// <param name="serviceNames">Collection of service names to
        /// the service it represents (these names are unique).</param>
        static void GetCollectionOfServiceAndDisplayNames(
            out Dictionary<string, ServiceController> displayNames,
            out Dictionary<string, ServiceController> serviceNames)
        {
            try
            {
                // Create the new display and service name collections
                displayNames = new Dictionary<string, ServiceController>();
                serviceNames = new Dictionary<string, ServiceController>();

                // Fill the collections with data about all services
                foreach (ServiceController controller in ServiceController.GetServices())
                {
                    // Add the service display name to the collection (these are unique)
                    displayNames.Add(controller.DisplayName.ToUpperInvariant(), controller);

                    // Add the service name to the collection (these are unique)
                    serviceNames.Add(controller.ServiceName.ToUpperInvariant(), controller);
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI29797", ex);
            }
        }

        /// <summary>
        /// Goes to the database and gets the list of <see cref="ProcessingThreadArguments"/>
        /// for each row in the FPSFile table that has AutoStart = 1.
        /// </summary>
        /// <param name="dbManager">The service DB manager.</param>
        /// <returns>
        /// A list containing the processing thread arguments for each
        /// FPS file that will be launched when the processing threads are started.
        /// </returns>
        static List<ProcessingThreadArguments> GetFpsFileProcessingArguments(
            FAMServiceDatabaseManager dbManager)
        {
            try
            {
                // Get the global number of files to process
                int globalNumberOfFilesToProcess = 0;
                var numberToProcessString =
                    dbManager.Settings[FAMServiceDatabaseManager.NumberOfFilesToProcessGlobalKey];
                if (string.IsNullOrWhiteSpace(numberToProcessString))
                {
                    new ExtractException("ELI31140",
                        "Application Trace: Default number of files to process is empty.").Log();
                }
                else if (!int.TryParse(numberToProcessString, out globalNumberOfFilesToProcess))
                {
                    var ee = new ExtractException("ELI31141",
                        "Application Trace: Default number of files to process is out of range or incorrect format.");
                    ee.AddDebugData("Value Found", numberToProcessString, false);
                    ee.AddDebugData("Value Expected", "Number >= 0", false);
                    ee.Log();
                }

                List<ProcessingThreadArguments> fpsFiles = new List<ProcessingThreadArguments>();
                foreach (var fpsFileData in dbManager.GetFpsFileData(true))
                {
                    int numberToProcess = fpsFileData.NumberOfFilesToProcess == -1 ?
                        globalNumberOfFilesToProcess : fpsFileData.NumberOfFilesToProcess;
                    for (int i = 0; i < fpsFileData.NumberOfInstances; i++)
                    {
                        fpsFiles.Add(new ProcessingThreadArguments(fpsFileData.FileName,
                            numberToProcess));
                    }
                }

                return fpsFiles;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI28499", ex);
            }
        }

        /// <summary>
        /// Signals the processing threads to stop.  Does not return until all
        /// threads have stopped.  Requests additional stop time if threads
        /// are still running. Do not call this method from anywhere except
        /// OnStop or OnShutdown
        /// </summary>
        /// <param name="eliCode">The ELI code to log when adding the FAM Service stopping
        /// application trace (this way we can distinguish between OnStop and OnShutdown).</param>
        /// <param name="canRequestAdditionalTime">If <see langword="true"/> then will request additional
        /// stop time from the service manager when stopping processing.
        /// <para><b>Note:</b></para>This should only be called with <see langword="true"/>
        /// if the method is being called from OnStop.</param>
        void StopService(string eliCode, bool canRequestAdditionalTime)
        {
            try
            {
                // [DNRCAU #357] - Log application trace when service is stopping
                ExtractException ee = new ExtractException(eliCode, UtilityMethods.FormatInvariant(
                    $"Application trace: FAM Service stopping. ({_serviceName})"));
                ee.Log();

                // Signal the threads to stop
                _stopProcessing.Set();

                foreach (var dbServiceManager in _databaseServiceManagers)
                {
                    dbServiceManager.Stop();
                }

                try
                {
                    if (_etlFAMDB != null)
                    {
                        _etlFAMDB.UnregisterActiveFAM();
                        _etlFAMDB.RecordFAMSessionStop();
                        _etlFAMDB.CloseAllDBConnections();
                        _etlFAMDB = null;
                    }
                }
                catch (Exception etlFAMException)
                {
                    etlFAMException.ExtractLog("ELI46310");
                }

                var stoppedEvents = _databaseServiceManagers
                    .Select(mgr => mgr.StoppedWaitHandle)
                    .Cast<WaitHandle>()
                    .ToArray();
                if (_threadsStopped != null)
                {
                    stoppedEvents = stoppedEvents.Union(new[] { _threadsStopped.WaitHandle }).ToArray();
                }

                // Only wait if there is a wait handle
                // and the caller has specified to wait
                while (canRequestAdditionalTime && !WaitHandle.WaitAll(stoppedEvents, 1000))
                {
                    RequestAdditionalTime(1200);
                }

                // [DNRCAU #357] - Log application trace when service has shutdown
                ExtractException ee2 = new ExtractException("ELI28774", UtilityMethods.FormatInvariant(
                    $"Application trace: FAM Service stopped. ({_serviceName})"));
                ee2.AddDebugData("Threads Stopped",
                    _threadsStopped != null ? _threadsStopped.Wait(0) : true, false);
                ee2.Log();

                // Set successful exit code
                ExitCode = 0;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI28497", ex);
            }
            finally
            {
                // Mutex around accessing the thread handle collection
                lock (_lock)
                {
                    // Done with the thread event close the handle and set to null
                    if (_threadsStopped != null)
                    {
                        _threadsStopped.Dispose();
                        _threadsStopped = null;
                    }

                    // Dispose of the DatabaseServiceManagers
                    for (int i = 0; i < _databaseServiceManagers.Count; i++)
                    {
                        DatabaseServiceManager manager;
                        if (_databaseServiceManagers.TryTake(out manager))
                        {
                            manager.Dispose();
                        }
                    }

                    // Set the threads that required authentication back to 0
                    _threadsThatRequireAuthentication = 0;
                }
            }
        }

        // Query for the name of this instance
        static bool TryGetServiceName(out string serviceName)
        {
            using var searcher = new ManagementObjectSearcher(UtilityMethods.FormatInvariant(
                $"SELECT * FROM Win32_Service where ProcessId = {Process.GetCurrentProcess().Id}"));
            using var results = searcher.Get();
            serviceName = results
                .Cast<ManagementBaseObject>()
                .FirstOrDefault()?["Name"] as string;
            return serviceName != null;
        }

        #endregion Methods

        #region Properties

        /// <summary>
        /// Gets the name of the database file that is used by the service.
        /// </summary>
        /// <param name="serviceName">The name of the service (e.g., ESFAMService)</param>
        /// <returns>The name of the database file that is used by the service.</returns>
        public static string GetDatabaseFileName(string serviceName)
        {
            return FileSystemMethods.PathCombine(
                FileSystemMethods.CommonApplicationDataPath, "ESFAMService", serviceName + ".sdf");
        }

        #endregion Properties
    }
}
