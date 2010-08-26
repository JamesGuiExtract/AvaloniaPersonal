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
        protected override void OnStart(string[] args)
        {
            try
            {
                base.OnStart(args);

                // Ensure the end service event is reset
                _endService.Reset();

                ResetHost();
            }
            catch (Exception ex)
            {
                try
                {
                    var logger = new ExtractExceptionLogger();
                    logger.LogException(new ExceptionLoggerData(ex));
                }
                catch
                {
                }
            }
        }

        /// <summary>
        /// Raised when the service is stopped.
        /// </summary>
        protected override void OnStop()
        {
            try
            {
                _endService.Set();

                CloseHost();

                base.OnStop();
            }
            catch (Exception ex)
            {
                LogException(ex);
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
                        ApplicationException ee =
                            new ApplicationException("Application Trace: Exception service host was closed and was restarted.");
                        LogException(ee);
                    }
                }
            }
            catch (Exception ex)
            {
                LogException(ex);
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
                try
                {
                    var logger = new ExtractExceptionLogger();
                    logger.LogException(new ExceptionLoggerData(ex));
                }
                catch
                {
                }
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
                    _host = new ServiceHost(typeof(ExtractExceptionLogger),
                        new Uri("net.tcp://localhost"));
                    NetTcpBinding binding = new NetTcpBinding();
                    binding.PortSharingEnabled = true;
                    _host.AddServiceEndpoint(typeof(IExtractExceptionLogger), binding,
                        ExceptionLoggerData.WcfTcpEndPoint);
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

        /// <summary>
        /// Attempts to log the specified <see cref="Exception"/> to the Extract exception log.
        /// </summary>
        /// <param name="ex">The <see cref="Exception"/> to log.</param>
        static void LogException(Exception ex)
        {
            try
            {
                var logger = new ExtractExceptionLogger();
                logger.LogException(new ExceptionLoggerData(ex));
            }
            catch
            {
            }
        }
    }
}
