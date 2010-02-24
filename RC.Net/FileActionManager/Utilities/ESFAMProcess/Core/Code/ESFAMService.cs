using Extract.Licensing;
using Extract.Utilities;
using FAMProcessLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlServerCe;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
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
            /// Initializes a new instance of the <see cref="ProcessingThreadArguments"/> class.
            /// </summary>
            /// <param name="fpsFileName">The fps file to process.</param>
            public ProcessingThreadArguments(string fpsFileName)
            {
                _fpsFileName = fpsFileName;
            }

            /// <summary>
            /// Gets the fps file name.
            /// </summary>
            public string FPSFileName
            {
                get
                {
                    return _fpsFileName;
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
        internal static readonly string NumberOfFilesToProcess = "FilesToProcessPerFAMInstance";

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
        internal const int CurrentDatabaseSchemaVersion = 1;

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
        List<Thread> _processingThreads = new List<Thread>();

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
        /// The number of files to process
        /// </summary>
        int _numberOfFilesToProcess;

        /// <summary>
        /// Mutex to provide synchronized access to data.
        /// </summary>
        object _lock = new object();

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

                // Get the list of FPS files to run
                List<string> fpsFiles = GetFPSFilesToRun();

                // [DNRCAU #357] - Log application trace when service is starting
                ExtractException ee2 = new ExtractException("ELI28772",
                    "Application trace: FAM Service starting.");
                ee2.Log();

                // Create the sleep thread and start it
                _sleepThread = new Thread(SleepAndCheckDependentServices);
                _sleepThread.Start();

                // Get the number of files to process
                _numberOfFilesToProcess = GetNumberOfFilesToProcess();

                lock (_lock)
                {
                    // Create and launch a processing thread for each fps file.
                    _threadsStopped = new ManualResetEvent(fpsFiles.Count == 0);
                    for (int i = 0; i < fpsFiles.Count; i++)
                    {
                        // Create a new thread and add it to the collection.
                        Thread thread = new Thread(ProcessFiles);
                        _processingThreads.Add(thread);

                        // Start the thread in a Multithreaded apartment
                        thread.SetApartmentState(ApartmentState.MTA);
                        thread.Start(new ProcessingThreadArguments(fpsFiles[i]));
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
                StopService("ELI28773");
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
                StopService("ELI29822");
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
            int pid = -1;
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

                // Run FAMProcesses in a loop respawning them if needed
                while(_stopProcessing != null && !_stopProcessing.WaitOne(0))
                {
                    // If there is a valid Process handle, dispose of it
                    if (process != null)
                    {
                        process.Dispose();
                        process = null;
                    }

                    // If we are spawning a new fam process, release the old
                    // one so that the memory is released before spawning the new
                    // process.
                    if (famProcess != null)
                    {
                        // Set the object to NULL so that the EXE will exit
                        famProcess = null;
                    }

                    // Create the FAM process
                    famProcess = new FileProcessingManagerProcessClass();
                    pid = famProcess.ProcessID;
                    process = Process.GetProcessById(pid);

                    // Set the FPS file name
                    famProcess.FPSFile = arguments.FPSFileName;

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
                        ee.AddDebugData("FPS File Name", arguments.FPSFileName, false);
                        throw ee;
                    }

                    {
                        ExtractException ee = new ExtractException("ELI29808",
                            "Application trace: Started new FAM instance.");
                        ee.AddDebugData("FPS Filename", arguments.FPSFileName, false);
                        ee.AddDebugData("Process ID", pid, false);
                        ee.Log();
                    }

                    // Ensure that processing has not been stopped before starting processing
                    if (_stopProcessing != null && !_stopProcessing.WaitOne(0))
                    {
                        // Start processing
                        famProcess.Start(_numberOfFilesToProcess);
                    }

                    // Sleep for two seconds and check if processing has stopped
                    // or the FAMProcess has exited
                    while (_stopProcessing != null && !_stopProcessing.WaitOne(2000)
                        && !process.HasExited && famProcess.IsRunning) ;

                    // Only loop back around if the number of files to process is not 0
                    if (_numberOfFilesToProcess == 0)
                    {
                        break;
                    }
                }

                // Check if the process is still running
                if (process != null && !process.HasExited && famProcess.IsRunning)
                {
                    // Tell the process to stop
                    famProcess.Stop();
                }
            }
            catch (Exception ex)
            {
                // Just log any exceptions from the process threads
                ExtractException.Log("ELI28498", ex);
            }
            finally
            {
                if (famProcess != null)
                {
                    // Wrap in a try/catch and log to ensure no exceptions thrown
                    // from the finally block.
                    try
                    {
                        // Release the COM object so the FAMProcess.exe is cleaned up
                        famProcess = null;
                        
                        // Perform a GC to force the object to clean up
                        GC.Collect();
                        GC.WaitForPendingFinalizers();

                        // Wait for the process to exit
                        if (process != null)
                        {
                            while (!process.HasExited)
                            {
                                System.Threading.Thread.Sleep(500);
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
                            // call OnStop to stop the service
                            OnStop();
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
                    value = reader.GetString(0);
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
            Dictionary<string, ServiceController> displayNames = null;
            Dictionary<string, ServiceController> serviceNames = null;
            GetCollectionOfServiceAndDisplayNames(out displayNames, out serviceNames);

            // Iterate through the list of dependent services and get the controllers
            // for each one
            List<ServiceController> dependentServices = new List<ServiceController>();
            for (int i=0; i < dependentServiceNames.Count; i++)
            {
                string serviceName = dependentServiceNames[i];
                ServiceController controller = null;
                bool found = false;
                if (serviceNames.TryGetValue(serviceName, out controller))
                {
                    // If the service was found in the dependency list, add it to the list
                    // of controllers
                    dependentServices.Add(controller);
                    found = true;

                    // Remove the controller from the dictionary
                    serviceNames.Remove(serviceName);
                }
                else if (displayNames.TryGetValue(serviceName, out controller))
                {
                    // If the service was found in the dependency list, add it to the list
                    // of controllers
                    dependentServices.Add(controller);
                    found = true;

                    // Remove the controller from the dictionary
                    displayNames.Remove(serviceName);
                }

                // If the service name was found, remove it from the list and
                // decrement the index
                if (found)
                {
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

            // Need to dispose of all other controllers
            foreach (ServiceController controller in displayNames.Values)
            {
                controller.Dispose();
            }
            foreach (ServiceController controller in serviceNames.Values)
            {
                controller.Dispose();
            }

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
        /// Goes to the database and gets the list of FPS files to run.
        /// </summary>
        /// <returns></returns>
        static List<string> GetFPSFilesToRun()
        {
            SqlCeConnection dbConnection = null;
            SqlCeCommand command = null;
            SqlCeDataReader reader = null;
            try
            {
                List<string> fpsFiles = new List<string>();

                // Connect to the database
                dbConnection = new SqlCeConnection(_connection);
                dbConnection.Open();

                // Get a data adapter connected to the FPSFile table
                command = new SqlCeCommand(
                    "SELECT [FileName] FROM [FPSFile] WHERE [AutoStart] = 1", dbConnection);

                reader = command.ExecuteReader();
                while (reader.Read())
                {
                    fpsFiles.Add(reader.GetString(0));
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
                    int schemaVersion = 0;
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
        static int GetNumberOfFilesToProcess()
        {
            string numberString = "";
            try
            {
                int numberOfFiles = DefaultNumberOfFilesToProcess;
                numberString = GetSettingFromDatabase(NumberOfFilesToProcess);
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
        void StopService(string eliCode)
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
                if (_threadsStopped != null)
                {
                    while (_threadsStopped != null && !_threadsStopped.WaitOne(1000))
                    {
                        RequestAdditionalTime(1200);
                    }
                }

                // [DNRCAU #357] - Log application trace when service has shutdown
                ExtractException ee2 = new ExtractException("ELI28774",
                    "Application trace: FAM Service stopped.");
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
