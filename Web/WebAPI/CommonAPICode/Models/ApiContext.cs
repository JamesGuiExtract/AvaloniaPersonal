using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
        /// this class maintains the essential API context data - Database server name, database name, and workflow name
        /// </summary>
        /// <param name="databaseServerName">server name</param>
        /// <param name="databaseName">database name</param>
        /// <param name="workflowName">workflow name</param>
        /// <param name="numberOfConnectionRetries">number of retries on DB connection, on failure</param>
        /// <param name="connectionRetryTimeout">timout interval for DB connection</param>
        public ApiContext(string databaseServerName,
                          string databaseName,
                          string workflowName,
                          string numberOfConnectionRetries = "",
                          string connectionRetryTimeout = "")
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
        }

        /// <summary>
        /// Number of times to retry DB connection on failure
        /// </summary>
        public int NumberOfConnectionRetries { get; private set; }

        /// <summary>
        /// retry interval in seconds
        /// </summary>
        public int ConnectionRetryTimeout { get; private set; }

        /// <summary>
        /// database server name
        /// </summary>
        public string DatabaseServerName { get; private set; }

        /// <summary>
        /// database name
        /// </summary>
        public string DatabaseName { get; private set; }

        /// <summary>
        /// workflow name
        /// </summary>
        public string WorkflowName { get; set; }

        /// <summary>
        /// Gets the session identifier for this context.
        /// </summary>
        public string SessionId { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Gets the FAM Session ID to which the <see cref="SessionId"/> maps.
        /// </summary>
        public int FAMSessionId { get; set; }
    }
}
