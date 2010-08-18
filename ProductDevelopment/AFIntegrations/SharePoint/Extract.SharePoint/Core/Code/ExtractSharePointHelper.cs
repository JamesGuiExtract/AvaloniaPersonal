using Extract.ExceptionService;
using Microsoft.SharePoint;
using System;
using System.ServiceModel;

namespace Extract.SharePoint
{
    internal static class ExtractSharePointHelper
    {
        #region Methods

        /// <summary>
        /// Logs the specified exception to the Extract exception logging service
        /// located at the specified IP address.
        /// </summary>
        /// <param name="ipAddress">The ip address to log to.</param>
        /// <param name="ex">The exception to log.</param>
        internal static void LogExceptionTcp(string ipAddress, Exception ex)
        {
            try
            {
                var factory = new ChannelFactory<IExtractExceptionLogger>(new NetTcpBinding(),
                    new EndpointAddress("net.tcp://" + ipAddress + "/TcpExceptionLog"));
                IExtractExceptionLogger logger = SPChannelFactoryOperations.CreateChannelAsProcess(
                    factory);
                logger.LogException(new ExceptionLoggerData(ex));
            }
            catch
            {
            }
        }

        #endregion Methods
    }
}
