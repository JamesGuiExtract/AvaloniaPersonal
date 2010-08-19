using System;
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

                ResetHost(false);
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
                if (_host != null)
                {
                    CloseHost();
                }

                base.OnStop();
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
        /// Handles the host faulted event.
        /// </summary>
        /// <param name="sender">The object which sent the event.</param>
        /// <param name="e">The data associated with the event.</param>
        void HandleHostFaulted(object sender, EventArgs e)
        {
            // Launch a thread to reset the host since it is in a faulted state.
            Thread resetThread = new Thread(ResetHost);
            resetThread.Start(true);
        }

        /// <summary>
        /// Closes the service host.
        /// </summary>
        void CloseHost()
        {
            lock (_lock)
            {
                _host.Close();
                _host = null;
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
        /// Handles starting/resetting the service host.
        /// </summary>
        /// <param name="calledFromThread">Indicates whether this method
        /// was called from a thread start.</param>
        void ResetHost(object calledFromThread)
        {
            bool runningInThread = (bool)calledFromThread;
            try
            {
                if (runningInThread)
                {
                    Thread.Sleep(2000);
                }

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
                            ExceptionLoggerData._WCF_TCP_END_POINT);
                        _host.Open();
                        _host.Faulted += HandleHostFaulted;
                    }
                    catch (Exception)
                    {
                        AbortHost();
                        throw;
                    }
                }
            }
            catch (Exception ex)
            {
                // If running in a thread then log any exception
                if (runningInThread)
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
                else
                {
                    throw;
                }
            }
        }
    }
}
