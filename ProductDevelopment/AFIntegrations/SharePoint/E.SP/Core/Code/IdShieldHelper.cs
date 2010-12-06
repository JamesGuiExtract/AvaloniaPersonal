using System;
using System.Globalization;

namespace Extract.SharePoint.Redaction
{
    internal static class IdShieldHelper
    {
        /// <summary>
        /// Name for the hidden list that is created to handle tracking files to ignore in
        /// the add event receiver
        /// </summary>
        internal static readonly string _HIDDEN_LIST_NAME =
            "C888293C-0DC0-4F3E-8B3F-4151929E5CE0";

        /// <summary>
        /// Handles logging the specified exception to the exception service
        /// and to the SharePoint log.
        /// </summary>
        /// <param name="ex">The exception to log.</param>
        /// <param name="categoryId">The category id for the exception.</param>
        /// <param name="eliCode">The ELI code for this exception.</param>
        internal static void LogException(Exception ex, ErrorCategoryId categoryId, string eliCode)
        {
            try
            {
                AddMachineNameDebug(ex);

                IdShieldProcessingFeatureSettings settings =
                    IdShieldProcessingFeatureSettings.GetIdShieldSettings(false);
                if (settings != null)
                {
                    ExtractSharePointHelper.LogExceptionTcp(
                        settings.ExceptionServiceIpAddress, ex, eliCode);
                }
            }
            catch (Exception ex2)
            {
                ExtractSharePointLoggingService.LogError(categoryId, ex, eliCode);
                ExtractSharePointLoggingService.LogError(ErrorCategoryId.ExceptionLogger, ex2,
                    "ELI30550");
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
