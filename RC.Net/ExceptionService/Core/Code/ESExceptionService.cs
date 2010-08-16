using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.ServiceModel;

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

                _host = new ServiceHost(typeof(ExtractExceptionLogger),
                    new Uri("net.tcp://localhost"));
                NetTcpBinding binding = new NetTcpBinding();
                binding.PortSharingEnabled = true;
                _host.AddServiceEndpoint(typeof(IExtractExceptionLogger), binding,
                    "TcpESExceptionLog");

                _host.Open();
            }
            catch (Exception ex)
            {
                // Exception occurred, ensure the host is closed and
                // cleaned up
                if (_host != null)
                {
                    _host.Close();
                    (_host as System.IDisposable).Dispose();
                    _host = null;
                }

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
                    _host.Close();
                    (_host as IDisposable).Dispose();
                    _host = null;
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
    }
}
