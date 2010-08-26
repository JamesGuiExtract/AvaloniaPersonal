using Microsoft.SharePoint;
using System;
using System.Globalization;

namespace Extract.SharePoint.Redaction
{
    internal static class IdShieldHelper
    {
        /// <summary>
        /// Gets the ID Shield feature from the specified SharePoint web.
        /// </summary>
        /// <param name="site">The site to search for the feature.</param>
        /// <returns>The ID Shield feature (or <see langword="null"/> if it is
        /// not installed.</returns>
        internal static SPFeature GetIdShieldFeature(SPSite site)
        {
            try
            {
                return site.Features[IdShieldSettings._IDSHIELD_FEATURE_GUID];
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Handles logging the specified exception to the exception service
        /// and to the SharePoint log.
        /// </summary>
        /// <param name="ex">The exception to log.</param>
        /// <param name="categoryId">The category id for the exception.</param>
        internal static void LogException(Exception ex, ErrorCategoryId categoryId)
        {
            try
            {
                AddMachineNameDebug(ex);

                IdShieldSettings settings = IdShieldSettings.GetIdShieldSettings(false);
                if (settings != null)
                {
                    ExtractSharePointHelper.LogExceptionTcp(
                        settings.ExceptionServiceIPAddress, ex);
                }
                ExtractSharePointLoggingService.LogError(categoryId, ex);
            }
            catch (Exception ex2)
            {
                ExtractSharePointLoggingService.LogError(categoryId, ex);
                ExtractSharePointLoggingService.LogError(ErrorCategoryId.ExceptionLogger, ex2);
            }
        }

        /// <summary>
        /// Adds the current machine name as additional debug data to the exception.
        /// </summary>
        /// <param name="ex">The exception to add the data to.</param>
        static void AddMachineNameDebug(Exception ex)
        {
            try
            {
                string debugRoot = "Machine Name";
                string debugKey = debugRoot;
                int i = 1;
                while (ex.Data.Contains(debugKey))
                {
                    debugKey = debugRoot + " " + i.ToString(CultureInfo.InvariantCulture);
                    i++;
                }

                ex.Data.Add(debugKey, Environment.MachineName);
            }
            catch
            {
            }
        }
    }
}
