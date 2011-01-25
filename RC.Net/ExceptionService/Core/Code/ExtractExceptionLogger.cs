using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.ServiceModel;
using System.Text;

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
                string eliCode = exceptionData.EliCode;
                var ee = exceptionData.ExceptionData.LogExceptionWithHelperApp(eliCode);
                if (ee != null)
                {
                    throw ee;
                }
            }
            catch (Exception exception)
            {
                throw new FaultException("Unable to log exception. " + exception.Message);
            }
        }

        #endregion
    }
}
