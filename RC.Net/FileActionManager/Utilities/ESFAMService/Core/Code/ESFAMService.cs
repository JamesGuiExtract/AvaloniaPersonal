using Extract.Licensing;
using Extract.Utilities;
using FAMProcessLib;
using System;
using System.Collections.Generic;
using System.Data.SqlServerCe;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.ServiceProcess;
using System.Text;
using System.Threading;

namespace Extract.FileActionManager.Utilities
{
    /// <summary>
    /// Class that manages the FAM service start/stop and FAMProcess.exe instances.
    /// </summary>
    internal sealed partial class ESFAMService : ServiceBase
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
        /// The default sleep time the service should use when starting (default is 2 minutes)
        /// </summary>
        internal static readonly int DefaultSleepTimeOnStartup = 120000;

        /// <summary>
        /// The setting key to read the sleep time on startup value from.
        /// </summary>
        internal static readonly string SleepTimeOnStartupKey = "SleepTimeOnStart";

        /// <summary>
        /// The setting key to read the dependent services value from.
        /// </summary>
        internal static readonly string DependentServices = "DependentServices";

        /// <summary>
        /// The setting key to read the number of files to process from.
        /// </summary>
        internal static readonly string NumberOfFilesToProcessGlobal =
            "NumberOfFilesToProcessPerFAMInstance";

        /// <summary>
        /// The column name for the number of files to process specified for each FPS file.
        /// </summary>
        internal static readonly string NumberOfFilesToProcess = "NumberOfFilesToProcess";

        /// <summary>
        /// The default number of files to process before respawning the FAMProcess
        /// <para><b>Note:</b></para>
        /// A value of 0 indicates that the process should keep processing until it is
        /// stopped and will not be respawned. Negative values are not allowed.
        /// </summary>
        internal const int DefaultNumberOfFilesToProcess = 0;

        /// <summary>
        /// The setting key for the current fam service database schema
        /// </summary>
        internal static readonly string ServiceDatabaseSchemaVersion = "ServiceDBSchemaVersion";

        /// <summary>
        /// The current FAM Service database schema version
        /// </summary>
        internal const int CurrentDatabaseSchemaVersion = 3;

        #endregion Constants

        #region Fields

        /// <summary>
        /// Path to the database that contains the FAM service settings.
        /// </summary>
        // Store the database parallel to the ESFAMService.exe file [DNRCAU #381]
        static readonly string _databaseFile = FileSystemMethods.PathCombine(
            Path.GetDirectoryName(Assembly.GetAssembly(typeof(ESFAMService)).Location),
            "ESFAMService.sdf");

        /// <summary>
        /// The connection string for connecting to the SqlCE database.
        /// </summary>
        static readonly string _connection = "Data Source='" + _databaseFile + "';";

        /// <summary>
        /// Event to indicate processing should stop.
        /// </summary>
        ManualResetEvent _stopProcessing = new ManualResetEvent(false);

        /// <summary>
        /// Event to indicate the threads may start (set to true by the sleep time thread)
        /// </summary>
        ManualResetEvent _startThreads = new ManualResetEvent(false);

        /// <summary>
        /// The collection of threads which are running.
        /// </summary>
        readonly List<Thread> _processingThreads = new List<Thread>();

        /// <summary>
        /// Thread which will sleep for the initial sleep time value and then set the
        /// _startThreads event handle
        /// </summary>
        Thread _sleepThread;

        /// <summary>
        /// Event handle that is set when all threads have stopped
        /// </summary>
        ManualResetEvent _threadsStopped;

        /// <summary>
        /// The count of active processing threads
        /// </summary>
        volatile int _activeProcessingThreadCount;

        /// <summary>
        /// The count of threads that required authentication to run.
        /// </summary>
        volatile int _threadsThatRequireAuthentication;

        /// <summary>
        /// Mutex to provide synchronized access to data.
        /// </summary>
        readonly object _lock = new object();

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

                // Validate the service database schema
                int schemaVersion = GetDatabaseSchemaVersion();
                if (schemaVersion != CurrentDatabaseSchemaVersion)
                {
                    ExtractException ee = new ExtractException("ELI29802",
                        "Invalid service database schema version.");
                    ee.AddDebugData("Current Supported Schema Version",
                        CurrentDatabaseSchemaVersion, false);
                    ee.AddDebugData("Database Schema Version", schemaVersion, false);
                    throw ee;
                }

                // Get the list of FPS file processing arguments from the database
                List<ProcessingThreadArguments> fpsFileArguments =
                    GetFpsFileProcessingArguments(GetGlobalNumberOfFilesToProcess());

                // [DNRCAU #357] - Log application trace when service is starting
                ExtractException ee2 = new ExtractException("ELI28772",
                    "Application trace: FAM Service starting.");
                ee2.Log();

                // Create the sleep thread and start it
                _sleepThread = new Thread(SleepAndCheckDependentServices);
                _sleepThread.Start();

                lock (_lock)
                {
                    // Create and launch a processing thread for each fps file.
                    _threadsStopped = new ManualResetEvent(fpsFileArguments.Count == 0);
                    for (int i = 0; i < fpsFileArguments.Count; i++)
                    {
                        // Create a new thread and add it to the collection.
                        Thread thread = new Thread(ProcessFiles);
                        _processingThreads.Add(thread);

                        // Start the thread in a Multithreaded apartment
                        thread.SetApartmentState(ApartmentState.MTA);
                        thread.Start(fpsFileArguments[i]);
                        _activeProcessingThreadCount++;
                    }
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI28496", ex);
                ee.Log();
                throw ee;
            }
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
        /// Thread function that sleeps on intial startup and then sets the startThreads event.
        /// </summary>
        void SleepAndCheckDependentServices()
        {
            try
            {
                int sleepTime = GetStartupSleepTime();
                _stopProcessing.WaitOne(sleepTime);

                // Get the collection of ServiceController's for the dependent services
                List<ServiceController> dependentServices = GetDependentServiceControllers();

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

                        string services =
                            StringMethods.ConvertArrayToDelimitedList(serviceNotStarted, ",");
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
            try
            {
                // Wait for the sleep time to expire
                if (_startThreads != null)
                {
                    _startThreads.WaitOne();
                }

                // Get the processing thread arguments
                ProcessingThreadArguments arguments = (ProcessingThreadArguments)threadParameters;
                int numberOfFilesToProcess = arguments.NumberOfFilesToProcess;

                // Run FAMProcesses in a loop respawning them if needed
                do
                {
                    // Create the FAM process
                    famProcess = new FileProcessingManagerProcessClass();
                    int pid = famProcess.ProcessID;
                    process = Process.GetProcessById(pid);

                    // Set the FPS file name
                    famProcess.FPSFile = arguments.FpsFileName;

                    // Check if authentication is required
                    if (famProcess.AuthenticationRequired)
                    {
                        // Mutex around incrementing _threadsThatRequireAuthentication
                        lock (_lock)
                        {
                            _threadsThatRequireAuthentication++;
                        }

                        ExtractException ee = new ExtractException("ELI29208",
                            "Authentication is required to launch this FPS file.");
                        ee.AddDebugData("FPS File Name", arguments.FpsFileName, false);
                        throw ee;
                    }

                    // Ensure that processing has not been stopped before starting processing
                    if (_stopProcessing != null && !_stopProcessing.WaitOne(0))
                    {
                        // Start processing
                        famProcess.Start(numberOfFilesToProcess);

                        ExtractException ee = new ExtractException("ELI29808",
                            "Application trace: Started new FAM instance.");
                        ee.AddDebugData("FPS Filename", arguments.FpsFileName, false);
                        ee.AddDebugData("Process ID", pid, false);
                        ee.AddDebugData("Number Of Files To Process", numberOfFilesToProcess, false);
                        ee.Log();
                    }

                    // Sleep for two seconds and check if processing has stopped
                    // or the FAMProcess has exited
                    while (_stopProcessing != null && !_stopProcessing.WaitOne(0)
                        && !process.HasExited && famProcess.IsRunning)
                    {
                        Thread.Sleep(2000);
                    }

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

                    // If the number of files to proces is 0 OR the number of files actually
                    // processed is less than the number of files specified, then just
                    // exit the loop and do not respawn a new FAM instance
                    if (numberOfFilesToProcess == 0 || filesProcessed < numberOfFilesToProcess)
                    {
                        break;
                    }

                    // Release the current FAM process before looping around and spawning a new one
                    // this way the memory is released and we are not leaving extra orphaned
                    // EXE's lying around.
                    if (famProcess != null) // Sanity check, it should never be null at this point
                    {
                        // Set the object to NULL so that the EXE will exit
                        famProcess = null;

                        // Perform a GC to force the object to clean up
                        // [DNRCAU #429]
                        GC.Collect();
                        GC.WaitForPendingFinalizers();

                        // Wait for the process to exit
                        while (process != null && !process.HasExited)
                        {
                            Thread.Sleep(1000);
                            if (_stopProcessing != null && _stopProcessing.WaitOne(0))
                            {
                                // If stop processing has been called and this process is
                                // still hanging around, kill it
                                if (!process.HasExited)
                                {
                                    try
                                    {
                                        process.Kill();
                                    }
                                    catch (Exception ex)
                                    {
                                        ExtractException.Log("ELI30030", ex);
                                    }
                                }

                                break;
                            }
                        }
                    }

                    // If there is a valid Process handle, dispose of it
                    if (process != null)
                    {
                        process.Dispose();
                        process = null;
                    }
                }
                while (_stopProcessing != null && !_stopProcessing.WaitOne(0));

                // Check if the process is still running
                if (process != null && !process.HasExited && famProcess.IsRunning)
                {
                    // Tell the process to stop
                    famProcess.Stop();
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("The remote procedure call failed"))
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
                if (famProcess != null)
                {
                    // Wrap in a try/catch and log to ensure no exceptions thrown
                    // from the finally block.
                    try
                    {
                        // Ensure stop has been called on the fam process
                        if (process != null && !process.HasExited && famProcess.IsRunning)
                        {
                            famProcess.Stop();
                        }

                        // Wait for the famprocess to stop
                        while (process != null && !process.HasExited
                            && famProcess != null && famProcess.IsRunning)
                        {
                            Thread.Sleep(1000);
                        }
                        
                        // Release the COM object so the FAMProcess.exe is cleaned up
                        famProcess = null;
                        
                        // Perform a GC to force the object to clean up
                        GC.Collect();
                        GC.WaitForPendingFinalizers();

                        // Wait for the process to exit
                        if (process != null)
                        {
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
                                    process.Kill();
                                }
                                catch (Exception ex)
                                {
                                    ExtractException.Log("ELI30038", ex);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ExtractException.Log("ELI28505", ex);
                    }
                }

                if (process != null)
                {
                    process.Dispose();
                }

                // Mutex around the active thread count decrement
                lock (_lock)
                {
                    _activeProcessingThreadCount--;
                    if (_activeProcessingThreadCount <= 0)
                    {
                        if (_threadsStopped != null)
                        {
                            // Wrap in a try/catch and log to ensure no exceptions thrown
                            // from the finally block.
                            try
                            {
                                _threadsStopped.Set();
                            }
                            catch (Exception ex)
                            {
                                ExtractException.Log("ELI28506", ex);
                            }
                        }

                        // Check if all threads failed to launch because they required authentication
                        if (_threadsThatRequireAuthentication == _processingThreads.Count)
                        {
                            // All threads failed due to authentication requirement
                            // call Stop to stop the service
                            Stop();
                        }
                    }
                }
            }
        }

        #endregion Thread Methods

        #region Methods

        /// <summary>
        /// Gets the specified setting from the database.
        /// </summary>
        /// <param name="settingName">The setting to retrieve.</param>
        /// <returns>The string value for the setting.</returns>
        static string GetSettingFromDatabase(string settingName)
        {
            SqlCeConnection dbConnection = null;
            SqlCeCommand command = null;
            SqlCeDataReader reader = null;
            try
            {
                string query = "SELECT [Value] FROM Settings WHERE [Name] = '"
                    + settingName + "'";

                // Connect to the database
                dbConnection = new SqlCeConnection(_connection);
                dbConnection.Open();
                command = new SqlCeCommand(query, dbConnection);
                reader = command.ExecuteReader();

                string value = "";
                if (reader.Read())
                {
                    value = reader.IsDBNull(0) ? "" : reader.GetString(0);
                }

                return value;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI29151", ex);
            }
            finally
            {
                if (reader != null)
                {
                    reader.Dispose();
                }
                if (command != null)
                {
                    command.Dispose();
                }
                if (dbConnection != null)
                {
                    dbConnection.Dispose();
                }
            }
        }

        /// <summary>
        /// Gets the startup sleep time value from the database.
        /// </summary>
        /// <returns>The startup sleep time value.</returns>
        static int GetStartupSleepTime()
        {
            try
            {
                string time = GetSettingFromDatabase(SleepTimeOnStartupKey);
                int value = DefaultSleepTimeOnStartup;
                if (!string.IsNullOrEmpty(time))
                {
                    if (!int.TryParse(time, out value))
                    {
                        ExtractException ee = new ExtractException("ELI29138",
                            "Initial sleep time value is not a valid integer value.");
                        ee.AddDebugData("Initial Sleep Time Value", time, false);
                        throw ee;
                    }
                    else if (value < 0)
                    {
                        ExtractException ee = new ExtractException("ELI29140",
                            "Sleep time must be a positive value.");
                        ee.AddDebugData("Sleep Time Value", time, false);
                        throw ee;
                    }
                }

                return value;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI29141", ex);
            }
        }

        /// <summary>
        /// Gets the list of dependent services from the service database.
        /// </summary>
        /// <returns>The list of dependent services from the service database.</returns>
        static List<string> GetDependentServices()
        {
            try
            {
                string names = GetSettingFromDatabase(DependentServices);
                Dictionary<string, object> serviceNames = new Dictionary<string,object>();
                object temp = new object();
                if (!string.IsNullOrEmpty(names))
                {
                    // Tokenize the list by the pipe character
                    foreach (string service in names.Split(
                        new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        // Add each unique service name to the collection
                        string upper = service.ToUpperInvariant().Trim();
                        if (!serviceNames.ContainsKey(upper))
                        {
                            serviceNames.Add(upper, temp);
                        }
                    }
                }

                // Get a list of service names from the collection
                List<string> services = new List<string>();
                foreach (string service in serviceNames.Keys)
                {
                    services.Add(service);
                }

                // Return the collection
                return services;
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
        /// <returns>A collection of <see cref="ServiceController"/>s for the
        /// dependent services in the service database settings table.</returns>
        static List<ServiceController> GetDependentServiceControllers()
        {
            // Get the list of dependent service names and sort it
            List<string> dependentServiceNames = GetDependentServices();
            dependentServiceNames.Sort();

            // Get the list of all services
            Dictionary<string, ServiceController> displayNames;
            Dictionary<string, ServiceController> serviceNames;
            GetCollectionOfServiceAndDisplayNames(out displayNames, out serviceNames);

            // Iterate through the list of dependent services and get the controllers
            // for each one
            List<ServiceController> dependentServices = new List<ServiceController>();
            for (int i=0; i < dependentServiceNames.Count; i++)
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
                string services =
                    StringMethods.ConvertArrayToDelimitedList(dependentServiceNames, ",");
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
        /// <param name="globalNumberOfFilesToProcess">The global number of files to
        /// process per instance setting.  This is used as the default value for the
        /// number of files to process for each thread.</param> 
        /// <returns>A list containing the processing thread arguments for each
        /// FPS file that will be launched when the processing threads are started.</returns>
        static List<ProcessingThreadArguments> GetFpsFileProcessingArguments(
            int globalNumberOfFilesToProcess)
        {
            SqlCeConnection dbConnection = null;
            SqlCeCommand command = null;
            SqlCeDataReader reader = null;
            try
            {
                List<ProcessingThreadArguments> fpsFiles = new List<ProcessingThreadArguments>();

                // Connect to the database
                dbConnection = new SqlCeConnection(_connection);
                dbConnection.Open();

                // Get a data adapter connected to the FPSFile table
                command = new SqlCeCommand(
                    "SELECT [FileName], [" + NumberOfFilesToProcess
                    + "] FROM [FPSFile] WHERE [AutoStart] = 1", dbConnection);

                reader = command.ExecuteReader();
                while (reader.Read())
                {
                    // Get the file name
                    string fileName = reader.GetString(0).Trim();

                    // Default the number of files to process to the global number
                    int numberOfFilesToProcess = globalNumberOfFilesToProcess;

                    // If a different value has been specified for this FPS file, read it.
                    if (!reader.IsDBNull(1))
                    {
                        string temp = reader.GetString(1).Trim();
                        if (!string.IsNullOrEmpty(temp))
                        {
                            // Parse the number
                            if (!int.TryParse(temp, out numberOfFilesToProcess) ||
                                numberOfFilesToProcess < 0)
                            {
                                ExtractException ee = new ExtractException("ELI30257",
                                    "NumberOfFilesToProcess is not a valid positive integer.");
                                ee.AddDebugData("FPS File Name", fileName, false);
                                ee.AddDebugData("NumberOfFilesToProcess", temp, false);
                                throw ee;
                            }
                        }
                    }

                    fpsFiles.Add(new ProcessingThreadArguments(fileName, numberOfFilesToProcess));
                }

                return fpsFiles;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI28499", ex);
            }
            finally
            {
                if (reader != null)
                {
                    reader.Dispose();
                }
                if (command != null)
                {
                    command.Dispose();
                }
                if (dbConnection != null)
                {
                    dbConnection.Dispose();
                }
            }
        }

        /// <summary>
        /// Gets the current schema version from the database setting.
        /// </summary>
        /// <returns>The database schema version.</returns>
        static int GetDatabaseSchemaVersion()
        {
            try
            {
                string schemaVersionString = GetSettingFromDatabase(ServiceDatabaseSchemaVersion);
                if (!string.IsNullOrEmpty(schemaVersionString))
                {
                    int schemaVersion;
                    if (!int.TryParse(schemaVersionString, out schemaVersion) ||
                        schemaVersion < 0)
                    {
                        throw new ExtractException("ELI29799",
                            "Setting is not a valid positive integer.");
                    }

                    return schemaVersion;
                }
                else
                {
                    throw new ExtractException("ELI29800",
                        "Schema version setting is missing from database.");
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI29801", ex);
            }
        }

        /// <summary>
        /// Gets the number of files to process from the database setting.
        /// </summary>
        /// <returns>The number of files to process.</returns>
        static int GetGlobalNumberOfFilesToProcess()
        {
            string numberString = "";
            try
            {
                int numberOfFiles = DefaultNumberOfFilesToProcess;
                numberString = GetSettingFromDatabase(NumberOfFilesToProcessGlobal);
                if (!string.IsNullOrEmpty(numberString))
                {
                    if (!int.TryParse(numberString, out numberOfFiles) ||
                        numberOfFiles < 0)
                    {
                        throw new ExtractException("ELI29186",
                            "Setting is not a valid positive integer.");
                    }
                }

                return numberOfFiles;
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI29187", ex);
                ee.AddDebugData("Default Number Of Files", numberString, false);
                throw ee;
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
                ExtractException ee = new ExtractException(eliCode,
                    "Application trace: FAM Service stopping.");
                ee.Log();

                // Signal the threads to stop
                _stopProcessing.Set();

                // Only wait if there is a wait handle
                // and the caller has specified to wait
                while (canRequestAdditionalTime && _threadsStopped != null && !_threadsStopped.WaitOne(1000))
                {
                    RequestAdditionalTime(1200);
                }

                // [DNRCAU #357] - Log application trace when service has shutdown
                ExtractException ee2 = new ExtractException("ELI28774",
                    "Application trace: FAM Service stopped.");
                ee2.AddDebugData("Threads Stopped",
                    _threadsStopped != null ? _threadsStopped.WaitOne(0) : true, false);
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
                        _threadsStopped.Close();
                        _threadsStopped = null;
                    }

                    // Set the active processing count back to 0
                    _activeProcessingThreadCount = 0;

                    // Set the threads that required authentication back to 0
                    _threadsThatRequireAuthentication = 0;
                }

                // Empty the processing threads collection since we are done with the threads
                _processingThreads.Clear();
            }
        }

        #endregion Methods

        #region Properties

        /// <summary>
        /// Gets the name of the database file that is used by the service.
        /// </summary>
        /// <returns>The name of the database file that is used by the service.</returns>
        public static string DatabaseFile
        {
            get
            {
                return _databaseFile;
            }
        }

        /// <summary>
        /// Gets the database connection string.
        /// </summary>
        /// <returns>The database connection string.</returns>
        public static string DatabaseConnectionString
        {
            get
            {
                return _connection;
            }
        }

        #endregion Properties
    }
}
