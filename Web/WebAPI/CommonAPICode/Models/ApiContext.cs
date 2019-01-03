using Extract;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

namespace WebAPI.Models
{
    /// <summary>
    /// This class is used to contain the three elements of the FAM FileProcessingDB API context:
    /// 1) Database Server Name
    /// 2) database name
    /// 3) workflow name
    /// </summary>
    public class ApiContext
    {
        // These default values are from FileProcessingDB. The intent here 
        // is that if these values are not overridden, then apply the defaults 
        // for normal execution. 
        const string _defaultRetryCount = "10";
        const string _defaultRetryTimeout = "120";

        /// <summary>
        /// This class maintains the essential API context data - Database server name, database name, and workflow name
        /// </summary>
        /// <param name="databaseServerName">server name</param>
        /// <param name="databaseName">database name</param>
        /// <param name="workflowName">workflow name</param>
        /// <param name="numberOfConnectionRetries">number of retries on DB connection, on failure</param>
        /// <param name="connectionRetryTimeout">timout interval for DB connection</param>
        /// <param name="exceptionLogFilter">Specifies the HTTP result codes that should not be logged
        /// to the main Extract exception log. Specify <c>null</c> to use the default value or empty
        /// string to log all error codes.</param>
        public ApiContext(string databaseServerName,
                          string databaseName,
                          string workflowName,
                          string numberOfConnectionRetries = "",
                          string connectionRetryTimeout = "",
                          string exceptionLogFilter = null)
        {
            HTTPError.Assert("ELI46375", !String.IsNullOrWhiteSpace(databaseServerName),
                "Database server name is empty");
            DatabaseServerName = databaseServerName;

            HTTPError.Assert("ELI46376", !String.IsNullOrWhiteSpace(databaseName),
                "Database name is empty");
            DatabaseName = databaseName;

            HTTPError.Assert("ELI46377", !String.IsNullOrWhiteSpace(workflowName),
                "Workflow name is empty");
            WorkflowName = workflowName;

            var numberOfRetries =
                !String.IsNullOrWhiteSpace(numberOfConnectionRetries) ?
                numberOfConnectionRetries :
                _defaultRetryCount;

            bool parsed = Int32.TryParse(numberOfRetries, out int retries);
            if (parsed)
            {
                HTTPError.Assert("ELI46378", retries > 0, "Number of DB connection retries must be > 0");
                NumberOfConnectionRetries = retries;
            }

            var timeoutInterval =
                !String.IsNullOrWhiteSpace(connectionRetryTimeout) ?
                connectionRetryTimeout :
                _defaultRetryTimeout;

            parsed = Int32.TryParse(timeoutInterval, out int timeout);
            if (parsed)
            {
                ConnectionRetryTimeout = timeout;
            }

            if (exceptionLogFilter != null)
            {
                try
                {
                    ExceptionLogFilter = NumericRange.Parse(exceptionLogFilter);
                }
                catch (Exception ex)
                {
                    ExceptionLogFilter = null;

                    var error = new HTTPError("ELI46604", StatusCodes.Status500InternalServerError,
                        "Failed to parse ExceptionLogFilter", ex);
                    error.AddDebugData("Filter", exceptionLogFilter, false);
                    throw error;
                }
            }
        }

        /// <summary>
        /// The number of times to retry DB connection on failure
        /// </summary>
        public int NumberOfConnectionRetries { get; }

        /// <summary>
        /// The retry interval in seconds
        /// </summary>
        public int ConnectionRetryTimeout { get; }

        /// <summary>
        /// Specifies the HTTP result codes that should not be logged to the main Extract exception log.
        /// Only needed for codes that pertain to an error or some sort (>= 400).
        /// </summary>
        public IEnumerable<NumericRange> ExceptionLogFilter { get; } = new[] { new NumericRange(404) };

        /// <summary>
        /// The database server name
        /// </summary>
        public string DatabaseServerName { get; }

        /// <summary>
        /// The database name
        /// </summary>
        public string DatabaseName { get; }

        /// <summary>
        /// The workflow name
        /// </summary>
        public string WorkflowName { get; set; }

        /// <summary>
        /// The session identifier for this context.
        /// </summary>
        public string SessionId { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// The FAM Session ID to which the <see cref="SessionId"/> maps.
        /// </summary>
        public int FAMSessionId { get; set; }
    }
}
