using Extract.Interfaces;
using Extract.Licensing;
using System;
using System.ServiceModel;
using System.ServiceProcess;
using System.Threading;

namespace Extract.UtilityApplications.Services
{
    /// <summary>
    /// A service that facilitates inter-process communication between Extract Systems processes via
    /// WCF.
    /// </summary>
    public partial class ESIPCService : ServiceBase
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(ESIPCService).ToString();

        #endregion Constants

        #region Fields

        /// <summary>
        /// Maintains communication channels used to relay file paths between Extract processes.
        /// </summary>
        ServiceHost _fileReceiverHost;

        /// <summary>
        /// Mutex to provide synchronized access to data.
        /// </summary>
        readonly object _lock = new object();

        /// <summary>
        /// Event used to indicate that the service is stopping.
        /// </summary>
        ManualResetEvent _endService = new ManualResetEvent(false);

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ESIPCService"/> class.
        /// </summary>
        public ESIPCService()
        {
            InitializeComponent();
        }

        #endregion Constructors

        #region Overrides

        /// <summary>
        /// When implemented in a derived class, executes when a Start command is sent to the
        /// service by the Service Control Manager (SCM) or when the operating system starts (for
        /// a service that starts automatically). Specifies actions to take when the service starts.
        /// </summary>
        /// <param name="args">Data passed by the start command.</param>
        protected override void OnStart(string[] args)
        {
            try
            {
                LicenseUtilities.ValidateLicense(LicenseIdName.IDShieldCoreObjects, "ELI33126",
                    _OBJECT_NAME);

                _endService.Reset();
                ResetFileReceiverHost();
            }
            catch (Exception ex)
            {
                ExtractException ee = ex.AsExtract("ELI33127");
                ee.Log();
                throw ee;
            }
        }

        /// <summary>
        /// When implemented in a derived class, executes when a Stop command is sent to the service
        /// by the Service Control Manager (SCM). Specifies actions to take when a service stops
        /// running.
        /// </summary>
        protected override void OnStop()
        {
            try
            {
                StopService();
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI33128", ex);
                ee.Log();
                throw ee;
            }
            finally
            {
                base.OnStop();
            }
        }

        /// <summary>
        /// When implemented in a derived class, executes when the system is shutting down.
        /// Specifies what should occur immediately prior to the system shutting down.
        /// </summary>
        protected override void OnShutdown()
        {
            try
            {
                StopService();
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI33129", ex);
                ee.Log();
                throw ee;
            }
            finally
            {
                base.OnShutdown();
            }
        }

        #endregion Overrides

        #region Methods

        /// <summary>
        /// Signals the processing threads to stop.  Does not return until all threads have stopped.
        /// Do not call this method from anywhere except OnStop or OnShutdown
        /// </summary>
        void StopService()
        {
            // Signal the threads to stop
            _endService.Set();

            CloseFileReceiverHost();

            // Set successful exit code
            ExitCode = 0;
        }

        /// <summary>
        /// Handles the host faulted event.
        /// </summary>
        /// <param name="sender">The object which sent the event.</param>
        /// <param name="e">The data associated with the event.</param>
        void HandleFileReceiverHostFaulted(object sender, EventArgs e)
        {
            // Launch a thread to reset the host since it is in a faulted state.
            Thread resetThread = new Thread(ResetFileReceiverHostWithSleep);
            resetThread.Start(true);
        }

        /// <summary>
        /// Handles the host closed event.
        /// </summary>
        /// <param name="sender">The object which sent the event.</param>
        /// <param name="e">The data associated with the event.</param>
        void HandleFileReceiverHostClosed(object sender, EventArgs e)
        {
            try
            {
                lock (_lock)
                {
                    if (_fileReceiverHost != null)
                    {
                        _fileReceiverHost.Closed -= HandleFileReceiverHostClosed;
                        _fileReceiverHost = null;
                    }

                    // If the service is still running, then respawn the host
                    if (!_endService.WaitOne(0))
                    {
                        ResetFileReceiverHost();
                        ExtractException.Log("ELI33130",
                            "Application Trace: Exception service host was closed and restarted.",
                            true);
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractLog("ELI33131", true);
            }
        }

        /// <summary>
        /// Closes the service host.
        /// </summary>
        void CloseFileReceiverHost()
        {
            lock (_lock)
            {
                if (_fileReceiverHost != null)
                {
                    _fileReceiverHost.Closed -= HandleFileReceiverHostClosed;
                    _fileReceiverHost.Close();
                    _fileReceiverHost = null;
                }
            }
        }

        /// <summary>
        /// Aborts the service host. This should only be called if there was an error initializing
        /// it.
        /// </summary>
        void AbortFileReceiverHost()
        {
            if (_fileReceiverHost != null)
            {
                _fileReceiverHost.Abort();
                _fileReceiverHost = null;
            }
        }

        /// <summary>
        /// Handles starting/resetting the service host, but sleeps before resetting the service.
        /// </summary>
        void ResetFileReceiverHostWithSleep()
        {
            try
            {
                Thread.Sleep(1000);
                ResetFileReceiverHost();
            }
            catch (Exception ex)
            {
                ex.ExtractLog("ELI33132", true);
            }
        }

        /// <summary>
        /// Starts or restarts the service host.
        /// </summary>
        void ResetFileReceiverHost()
        {
            lock (_lock)
            {
                if (_fileReceiverHost != null)
                {
                    CloseFileReceiverHost();
                }

                try
                {
                    _fileReceiverHost = new ServiceHost(typeof(FileReceiverManager));
                    NetNamedPipeBinding binding = new NetNamedPipeBinding();
                    _fileReceiverHost.AddServiceEndpoint(typeof(IWcfFileReceiverManager),
                        binding, FileReceiver.WcfAddress);
                    _fileReceiverHost.Open();
                    _fileReceiverHost.Faulted += HandleFileReceiverHostFaulted;
                    _fileReceiverHost.Closed += HandleFileReceiverHostClosed;
                }
                catch (Exception)
                {
                    AbortFileReceiverHost();
                    throw;
                }
            }
        }

        #endregion Methods
    }
}
