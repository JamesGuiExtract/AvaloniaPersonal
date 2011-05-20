using System;
using System.ServiceModel;

namespace Extract.ExceptionService
{
    class ExtractExceptionLogger : IExtractExceptionLogger
    {
        #region IExtractExceptionLogger Members

        /// <summary>
        /// Logs the exception data to the local Extract exception log file.
        /// </summary>
        /// <param name="exceptionData">The data to be logged.</param>
        public void LogException(ExceptionLoggerData exceptionData)
        {
            try
            {
                var ee = exceptionData.ExceptionData.AsExtract(exceptionData.EliCode);
                ee.Log(exceptionData.MachineName, exceptionData.UserName,
                    exceptionData.DateTimeUtc, exceptionData.ProcessId,
                    exceptionData.ProductVersion);
            }
            catch (Exception exception)
            {
                try
                {
                    exception.ExtractLog("ELI32580");
                }
                catch
                {
                }

                throw new FaultException("Unable to log exception. " + exception.Message);
            }
        }

        #endregion
    }
}
