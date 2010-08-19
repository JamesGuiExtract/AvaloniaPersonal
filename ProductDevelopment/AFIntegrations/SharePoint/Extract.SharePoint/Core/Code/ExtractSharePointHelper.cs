using Extract.ExceptionService;
using System;
using System.ServiceModel;
using System.Text;

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
            ChannelFactory<IExtractExceptionLogger> factory = null;
            try
            {
                if (!string.IsNullOrEmpty(ipAddress))
                {
                    // Build the url
                    StringBuilder url = new StringBuilder("net.tcp://");
                    url.Append(ipAddress);
                    url.Append("/");
                    url.Append(ExceptionLoggerData._WCF_TCP_END_POINT);

                    factory = new ChannelFactory<IExtractExceptionLogger>(new NetTcpBinding(),
                        new EndpointAddress(url.ToString()));

                    IExtractExceptionLogger logger = factory.CreateChannel();
                    logger.LogException(new ExceptionLoggerData(ex));

                    factory.Close();
                }
                else
                {
                    ExtractSharePointLoggingService.LogError(ErrorCategoryId.Feature, ex);
                }
            }
            catch (Exception ex2)
            {
                if (factory != null)
                {
                    factory.Abort();
                    factory = null;
                }

                // Unable to use logging service, send the error to the sharepoint log
                ExtractSharePointLoggingService.LogError(ErrorCategoryId.Feature, ex);
                ExtractSharePointLoggingService.LogError(ErrorCategoryId.Feature, ex2);
            }
        }

        #endregion Methods
    }
}
