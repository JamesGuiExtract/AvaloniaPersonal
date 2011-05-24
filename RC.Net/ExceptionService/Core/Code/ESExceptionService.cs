using System;
using System.Diagnostics.CodeAnalysis;
using System.ServiceModel;
using System.ServiceProcess;
using System.Threading;

namespace Extract.ExceptionService
{
    /// <summary>
    /// Class that implements the Extract Systems exception logging service.
    /// </summary>
    public partial class ESExceptionService : ServiceBase
    {
        #region Fields

        /// <summary>
        /// Maintains the open communication channels used to communicate with the service.
        /// </summary>
        ServiceHost _host;

        /// <summary>
        /// Mutex object used to ensure serialized access to the service host.
        /// </summary>
        object _lock = new object();

        /// <summary>
        /// Event used to indicate that the service is stopping.
        /// </summary>
        ManualResetEvent _endService = new ManualResetEvent(false);

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ESExceptionService"/> class.
        /// </summary>
        public ESExceptionService()
        {
            InitializeComponent();
        }

        #endregion Constructors

        /// <summary>
        /// Raised when the service is starting.
        /// </summary>
        /// <param name="args">Arguments associated with starting the service.</param>
        // Not raising the application exception, just creating a trace, there is
        // not a more specific exception type to use here.
        [SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes")]
        protected override void OnStart(string[] args)
        {
            try
            {
                base.OnStart(args);

                // Ensure the end service event is reset
                _endService.Reset();

                // Request additional time for the application trace to log
                this.RequestAdditionalTime(60000);

                ResetHost();

                ExtractException.Log("ELI30558", "Application Trace: Exception service started.",
                    true);
            }
            catch (Exception ex)
            {
                ex.ExtractLog("ELI30559", true);
            }
        }

        /// <summary>
        /// Raised when the service is stopped.
        /// </summary>
        // Not raising the application exception, just creating a trace, there is
        // not a more specific exception type to use here.
        [SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes")]
        protected override void OnStop()
        {
            try
            {
                _endService.Set();


                // Request additional time for the application trace to log
                this.RequestAdditionalTime(60000);

                CloseHost();

                ExtractException.Log("ELI30560", "Application Trace: Exception service stopped.",
                    true);

                base.OnStop();
            }
            catch (Exception ex)
            {
                ex.ExtractLog("ELI30561", true);
            }
        }

        /// <summary>
        /// Raises the shutdown event.
        /// </summary>
        // Not raising the application exception, just creating a trace, there is
        // not a more specific exception type to use here.
        [SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes")]
        protected override void OnShutdown()
        {
            try
            {
                _endService.Set();

                CloseHost();

                base.OnShutdown();

                ExtractException.Log("ELI30565",
                    "Application Trace: Exception service was shutdown.", true);
            }
            catch (Exception ex)
            {
                ex.ExtractLog("ELI30566", true);
            }
        }

        /// <summary>
        /// Handles the host faulted event.
        /// </summary>
        /// <param name="sender">The object which sent the event.</param>
        /// <param name="e">The data associated with the event.</param>
        void HandleHostFaulted(object sender, EventArgs e)
        {
            // Launch a thread to reset the host since it is in a faulted state.
            Thread resetThread = new Thread(ResetHostWithSleep);
            resetThread.Start(true);
        }


        /// <summary>
        /// Handles the host closed event.
        /// </summary>
        /// <param name="sender">The object which sent the event.</param>
        /// <param name="e">The data associated with the event.</param>
        // Not raising the application exception, just creating a trace, there is
        // not a more specific exception type to use here.
        [SuppressMessage("Microsoft.Usage", "CA2201:DoNotRaiseReservedExceptionTypes")]
        void HandleHostClosed(object sender, EventArgs e)
        {
            try
            {
                lock (_lock)
                {
                    if (_host != null)
                    {
                        _host.Closed -= HandleHostClosed;
                        _host = null;
                    }

                    // If the service is still running, then respawn the host
                    if (!_endService.WaitOne(0))
                    {
                        ResetHost();
                        ExtractException.Log("ELI30562",
                            "Application Trace: Exception service host was closed and restarted.",
                            true);
                    }
                }
            }
            catch (Exception ex)
            {
                ex.ExtractLog("ELI30563", true);
            }
        }

        /// <summary>
        /// Closes the service host.
        /// </summary>
        void CloseHost()
        {
            lock (_lock)
            {
                if (_host != null)
                {
                    _host.Closed -= HandleHostClosed;
                    _host.Close();
                    _host = null;
                }
            }
        }

        /// <summary>
        /// Aborts the service host. This should only be called if there was an
        /// error initializing it.
        /// </summary>
        void AbortHost()
        {
            _host.Abort();
            _host = null;
        }

        /// <summary>
        /// Handles starting/resetting the service host, but sleeps before
        /// resetting the service.
        /// </summary>
        void ResetHostWithSleep()
        {
            try
            {
                Thread.Sleep(1000);
                ResetHost();
            }
            catch (Exception ex)
            {
                ex.ExtractLog("ELI30564", true);
            }
        }

        /// <summary>
        /// Handles starting/resetting the service host.
        /// </summary>
        void ResetHost()
        {
            lock (_lock)
            {
                if (_host != null)
                {
                    CloseHost();
                }

                try
                {
                    _host = new ServiceHost(typeof(ExtractExceptionLogger));
                    var address = "net.tcp://localhost/" + ExceptionLoggerData.WcfTcpEndPoint;
                    NetTcpBinding binding = new NetTcpBinding();
                    binding.PortSharingEnabled = true;
                    _host.AddServiceEndpoint(typeof(IExtractExceptionLogger), binding, address);
                    _host.Open();
                    _host.Faulted += HandleHostFaulted;
                    _host.Closed += HandleHostClosed;
                }
                catch (Exception)
                {
                    AbortHost();
                    throw;
                }
            }
        }
    }
}
