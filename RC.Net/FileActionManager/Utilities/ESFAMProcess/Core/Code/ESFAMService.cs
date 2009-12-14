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
            /// The number for this thread (used to access the ManualResetEvent for the thread)
            /// </summary>
            readonly int _threadNumber;

            /// <summary>
            /// Initializes a new instance of the <see cref="ProcessingThreadArguments"/> class.
            /// </summary>
            /// <param name="fpsFileName">The fps file to process.</param>
            /// <param name="threadNumber">The index for this threads stop event in the
            /// array of thread stop events.</param>
            public ProcessingThreadArguments(string fpsFileName, int threadNumber)
            {
                _fpsFileName = fpsFileName;
                _threadNumber = threadNumber;
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

            /// <summary>
            /// Gets the thread number
            /// </summary>
            public int ThreadNumber
            {
                get
                {
                    return _threadNumber;
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
        /// The name of the FAM Process, used for the IsProcessingRunning call.
        /// </summary>
        static readonly string _FAM_PROCESS_NAME = "FAMProcess";

        #endregion Constants

        #region Fields

        /// <summary>
        /// Path to the database that contains the FAM service settings.
        /// </summary>
        static readonly string _databaseFile = FileSystemMethods.PathCombine(
            Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
            "Extract Systems", "Service", "ESFAMService.sdf");

        /// <summary>
        /// The connection string for connecting to the SqlCE database.
        /// </summary>
        static readonly string _connection = "Data Source='" + _databaseFile + "';";

        /// <summary>
        /// Event to indicate processing should stop.
        /// </summary>
        ManualResetEvent _stopProcessing = new ManualResetEvent(false);

        /// <summary>
        /// Contains the event handles for each thread so that the threads can indicate
        /// that they have stopped processing.
        /// </summary>
        ManualResetEvent[] _threadStopped;

        /// <summary>
        /// Event handle to indicate that the dns service has started
        /// </summary>
        ManualResetEvent _dnsStarted = new ManualResetEvent(false);

        /// <summary>
        /// Event handle to indicate that the net login service has started
        /// </summary>
        ManualResetEvent _netLogonStarted = new ManualResetEvent(false);

        /// <summary>
        /// The collection of threads which are running.
        /// </summary>
        List<Thread> _processingThreads = new List<Thread>();

        /// <summary>
        /// Thread for checking if the DNS Client service is running.
        /// </summary>
        Thread _dnsCheck;

        /// <summary>
        /// Thread for checking if the Net Logon service is running
        /// </summary>
        Thread _netLogonCheck;

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

                // Get the list of FPS files to run
                List<string> fpsFiles = GetFPSFilesToRun();

                // [DNRCAU #357] - Log application trace when service is starting
                ExtractException ee = new ExtractException("ELI28772",
                    "Application trace: FAM Service starting.");
                ee.Log();

                // Kick off threads to check for dependent services running
                _dnsCheck = new Thread(CheckForDnsServiceRunning);
                _netLogonCheck = new Thread(CheckForNetLogonServiceRunning);
                _dnsCheck.Start();
                _netLogonCheck.Start();

                lock (_lock)
                {
                    // Create and launch a processing thread for each fps file.
                    _threadStopped = new ManualResetEvent[fpsFiles.Count];
                    for (int i = 0; i < fpsFiles.Count; i++)
                    {
                        // Create a new stopped event to be signaled and add it to
                        // the collection.
                        _threadStopped[i] = new ManualResetEvent(false);

                        // Create a new thread and add it to the collection.
                        Thread thread = new Thread(ProcessFiles);
                        _processingThreads.Add(thread);

                        // Start the thread
                        thread.Start(new ProcessingThreadArguments(fpsFiles[i], i));
                    }
                }
            }
            catch (Exception ex)
            {
                ExtractException.Log("ELI28496", ex);
            }
        }

        /// <summary>
        /// Called when the service is shutting down.
        /// </summary>
        protected override void OnStop()
        {
            try
            {
                // [DNRCAU #357] - Log application trace when service is stopping
                ExtractException ee = new ExtractException("ELI28773",
                    "Application trace: FAM Service stopping.");
                ee.Log();

                // Signal the threads to stop
                _stopProcessing.Set();

                // Only wait if there are wait handles
                if (_threadStopped != null && _threadStopped.Length > 0)
                {
                    // Wait for all the threads to exit (timeout after a second to request
                    // additional time to stop)
                    while (!WaitHandle.WaitAll(_threadStopped, 1000))
                    {
                        // All wait handles were not signaled,
                        // request additional time to stop
                        RequestAdditionalTime(1200);
                    }
                }

                // [DNRCAU #357] - Log application trace when service has shutdown
                ExtractException ee2 = new ExtractException("ELI28774",
                    "Application trace: FAM Service stopped.");
                ee2.Log();
            }
            catch (Exception ex)
            {
                ExtractException.Log("ELI28497", ex);
            }
            finally
            {
                // Mutex around accessing the thread handle collection
                lock (_lock)
                {
                    // Done with the thread events, close the handles and reset the
                    // array to null
                    if (_threadStopped != null)
                    {
                        foreach (ManualResetEvent threadEvent in _threadStopped)
                        {
                            if (threadEvent != null)
                            {
                                threadEvent.Close();
                            }
                        }

                        _threadStopped = null;
                    }
                }

                // Empty the processing threads collection since we are done with the threads
                _processingThreads.Clear();
            }
        }

        #endregion ServiceBase Overrides

        #region Thread Methods

        /// <summary>
        /// Waits for the DNS Client service to start running and sets the service started handle
        /// </summary>
        void CheckForDnsServiceRunning()
        {
            try
            {
                // Ensure there is a wait handle to set and that it is not set already
                if (_dnsStarted != null && !_dnsStarted.WaitOne(0))
                {
                    // Wait for the dns client to start (set the wait timeout to 1 minute)
                    ServiceController dnsService = new ServiceController("DNS Client");
                    dnsService.WaitForStatus(ServiceControllerStatus.Running, new TimeSpan(0, 1, 0));
                }
            }
            // Log the timeout exception, ignore the other exceptions
            catch (System.ServiceProcess.TimeoutException ex)
            {
                // Log the timeout application trace
                ExtractException ee = new ExtractException("ELI28814",
                    "Application Trace: Timeout expired while waiting for DNS client to start.",
                    ex);
                ee.Log();
            }
            finally
            {
                // Ensure there is a wait handle to set and that it is not set already
                if (_dnsStarted != null && !_dnsStarted.WaitOne(0))
                {
                    // Set the wait handle
                    _dnsStarted.Set();
                }
            }
        }

        /// <summary>
        /// Waits for the Net Logon service to start running and sets the service started handle
        /// </summary>
        void CheckForNetLogonServiceRunning()
        {
            try
            {
                // Ensure there is a wait handle to set and that it is not set already
                if (_netLogonStarted != null && !_netLogonStarted.WaitOne(0))
                {
                    // Wait for the Net Logon to start (set the wait timeout to 1 minute)
                    ServiceController netLogon = new ServiceController("Net Logon");
                    netLogon.WaitForStatus(ServiceControllerStatus.Running, new TimeSpan(0, 1, 0));
                }
            }
            // Log the timeout exception, ignore the other exceptions
            catch (System.ServiceProcess.TimeoutException ex)
            {
                // Log the timeout application trace
                ExtractException ee = new ExtractException("ELI28815",
                    "Application Trace: Timeout expired while waiting for Net Logon to start.",
                    ex);
                ee.Log();
            }
            finally
            {
                // Ensure there is a wait handle to set and that it is not set already
                if (_netLogonStarted != null && !_netLogonStarted.WaitOne(0))
                {
                    // Set the wait handle
                    _netLogonStarted.Set();
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
            int threadNumber = 0;
            int pid = -1;
            try
            {
                // Get the processing thread arguments
                ProcessingThreadArguments arguments = (ProcessingThreadArguments)threadParameters;
                threadNumber = arguments.ThreadNumber;

                // Create the FAM process
                famProcess = new FileProcessingManagerProcessClass();
                pid = famProcess.ProcessID;

                // Set the FPS file name
                famProcess.FPSFile = arguments.FPSFileName;

                // Wait for both the dns service and the net login service to start
                if (_dnsStarted != null)
                {
                    // Wait for the event to signal
                    _dnsStarted.WaitOne(2000);
                }
                if (_netLogonStarted != null)
                {
                    // Wait for the event to signal
                    _netLogonStarted.WaitOne(2000);
                }

                // Ensure that processing has not been stopped before starting processing
                if (!_stopProcessing.WaitOne(0))
                {
                    // Start processing
                    famProcess.Start();
                }

                // Wait for stop signal
                _stopProcessing.WaitOne();

                // Check if the process is still running
                if (SystemMethods.IsProcessRunning(_FAM_PROCESS_NAME, pid) && famProcess.IsRunning)
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
                        Marshal.FinalReleaseComObject(famProcess);

                        // Wait for the process to exit
                        while (pid != -1 && SystemMethods.IsProcessRunning(_FAM_PROCESS_NAME, pid))
                        {
                            System.Threading.Thread.Sleep(100);
                        }
                    }
                    catch (Exception ex)
                    {
                        ExtractException.Log("ELI28505", ex);
                    }
                }

                if (_threadStopped != null)
                {
                    lock (_lock)
                    {
                        if (_threadStopped.Length > threadNumber &&
                            _threadStopped[threadNumber] != null)
                        {
                            // Wrap in a try/catch and log to ensure no exceptions thrown
                            // from the finally block.
                            try
                            {
                                _threadStopped[threadNumber].Set();
                            }
                            catch (Exception ex)
                            {
                                ExtractException.Log("ELI28506", ex);
                            }
                        }
                    }
                }
            }
        }

        #endregion Thread Methods

        #region Methods

        /// <summary>
        /// Goes to the database and gets the list of FPS files to run.
        /// </summary>
        /// <returns></returns>
        static List<string> GetFPSFilesToRun()
        {
            SqlCeConnection dbConnection = null;
            SqlCeDataAdapter adapter = null;
            DataTable data = null;
            try
            {
                List<string> fpsFiles = new List<string>();

                // Connect to the database
                dbConnection = new SqlCeConnection(_connection);

                // Get a data adapter connected to the FPSFile table
                adapter = new SqlCeDataAdapter(
                    "SELECT [FileName] FROM [FPSFile] WHERE [AutoStart] = 1", dbConnection);

                // Fill a table from the data adapter
                data = new DataTable();
                data.Locale = CultureInfo.InvariantCulture;
                adapter.Fill(data);

                // Add each enabled FPS file to the list
                foreach (DataRow row in data.Rows)
                {
                    fpsFiles.Add(row[0].ToString());
                }

                return fpsFiles;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI28499", ex);
            }
            finally
            {
                if (data != null)
                {
                    data.Dispose();
                }
                if (adapter != null)
                {
                    adapter.Dispose();
                }
                if (dbConnection != null)
                {
                    dbConnection.Dispose();
                }
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
