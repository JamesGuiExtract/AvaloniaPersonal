using System.ServiceModel;

namespace Extract.ExceptionService
{
    /// <summary>
    /// WCF Service contract interface for the exception logging service.
    /// </summary>
    [ServiceContract]
    public interface IExtractExceptionLogger
    {
        /// <summary>
        /// Interface method used to log the specified exception to the
        /// Extract exception log via a WCF service.
        /// </summary>
        /// <param name="exceptionData">The data to log.</param>
        [OperationContract]
        void LogException(ExceptionLoggerData exceptionData);
    }
}
