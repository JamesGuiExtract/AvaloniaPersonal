using Extract.ExceptionService;
using Microsoft.SharePoint;
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
        /// <param name="eliCode">The ELI code for this exception.</param>
        internal static void LogExceptionTcp(string ipAddress, Exception ex, string eliCode)
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
                    url.Append(ExceptionLoggerData.WcfTcpEndPoint);

                    factory = new ChannelFactory<IExtractExceptionLogger>(new NetTcpBinding(),
                        new EndpointAddress(url.ToString()));

                    IExtractExceptionLogger logger = factory.CreateChannel();
                    logger.LogException(new ExceptionLoggerData(ex, eliCode));

                    factory.Close();
                }

                // Always log to SharePoint log
                ExtractSharePointLoggingService.LogError(ErrorCategoryId.Feature, ex, eliCode);
            }
            catch (Exception ex2)
            {
                if (factory != null)
                {
                    factory.Abort();
                    factory = null;
                }

                // Unable to use logging service, send the error to the sharepoint log
                ExtractSharePointLoggingService.LogError(ErrorCategoryId.Feature, ex, eliCode);
                ExtractSharePointLoggingService.LogError(ErrorCategoryId.ExceptionLogger, ex2,
                    "ELI30548");
            }
        }

        /// <summary>
        /// Gets the site relative path for the specified folder and the site Id
        /// </summary>
        /// <param name="folderUrl">The server relative url for the folder.</param>
        /// <param name="siteId">The id for the site to compute the relative folder for.</param>
        /// <returns>The site relative path to the specified folder.</returns>
        internal static string GetSiteRelativeFolderPath(string folderUrl, Guid siteId)
        {
            string folder = string.Empty;
            using (SPSite site = new SPSite(siteId))
            {
                string siteUrl = site.ServerRelativeUrl;
                int index = folderUrl.IndexOf(siteUrl, StringComparison.OrdinalIgnoreCase);
                if (index >= 0)
                {
                    folder = folderUrl.Substring(index + siteUrl.Length);
                }
                else
                {
                    folder = folderUrl;
                }
            }

            // Ensure the folder starts with a '/'
            if (!folder.StartsWith("/", StringComparison.Ordinal))
            {
                folder = "/" + folder;
            }

            return folder;
        }

        /// <summary>
        /// Gets the folder id.
        /// </summary>
        /// <param name="web">The web.</param>
        /// <param name="folderPath">The current folder.</param>
        /// <returns>The unique Id for the folder.</returns>
        static internal Guid GetFolderId(SPWeb web, string folderPath)
        {
            var sb = new StringBuilder(web.Url);
            sb.Append(folderPath);
            var folder = web.GetFolder(sb.ToString());
            if (!folder.Exists)
            {
                throw new SPException("Cannot find folder.");
            }

            return folder.UniqueId;
        }

        #endregion Methods
    }
}
