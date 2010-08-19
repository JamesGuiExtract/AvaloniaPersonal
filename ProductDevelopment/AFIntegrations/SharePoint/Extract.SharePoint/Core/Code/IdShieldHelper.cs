using Microsoft.SharePoint;
using System;

namespace Extract.SharePoint.Redaction
{
    internal static class IdShieldHelper
    {
        /// <summary>
        /// Handles logging the specified exception to either the exception service
        /// or to the exception logging service.
        /// </summary>
        /// <param name="web">The web to get the ID Shield feature from.</param>
        /// <param name="ex">The exception to log.</param>
        internal static void LogException(SPWeb web, Exception ex)
        {
            try
            {
                SPFeature feature = GetIdShieldFeature(web);
                if (feature != null)
                {
                    SPFeatureProperty ipProperty =
                        feature.Properties[IdShieldSettings._IP_ADDRESS_SETTING_STRING];
                    if (ipProperty != null)
                    {
                        ExtractSharePointHelper.LogExceptionTcp(ipProperty.Value, ex);
                        return;
                    }
                }

                ExtractSharePointLoggingService.LogError(ErrorCategoryId.IdShieldFileReceiver, ex);
            }
            catch
            {
            }
        }

        /// <summary>
        /// Gets the ID Shield feature from the specified SharePoint web.
        /// </summary>
        /// <param name="web">The web to search for the feature.</param>
        /// <returns>The ID Shield feature (or <see langword="null"/> if it is
        /// not installed.</returns>
        internal static SPFeature GetIdShieldFeature(SPWeb web)
        {
            try
            {
                return web.Features[IdShieldSettings._IDSHIELD_FEATURE_GUID];
            }
            catch
            {
                return null;
            }
        }
    }
}
