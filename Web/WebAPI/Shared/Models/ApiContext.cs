﻿using Extract.Web.ApiConfiguration.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace WebAPI
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
        /// The API version initially released to customers (before versioning was instituted).
        /// This is the version that should be assumed if no version is specified.
        /// </summary>
        public const string LEGACY_VERSION = "2.0";

        /// <summary>
        /// The current API version 
        /// </summary>
        public const string CURRENT_VERSION = "3.1";

        /// <summary>
        /// This class maintains the essential API context data - Database server name, database name, and workflow name
        /// </summary>
        /// <param name="apiVersion">The API version to use</param>
        /// <param name="databaseServerName">server name</param>
        /// <param name="databaseName">database name</param>
        /// <param name="workflowName">workflow name</param>
        /// <param name="numberOfConnectionRetries">number of retries on DB connection, on failure</param>
        /// <param name="connectionRetryTimeout">timout interval for DB connection</param>
        /// <param name="maxInterfaces">Specifies the maximum number of concurrent COM API interfaces for
        /// a specific database workflow.</param>
        /// <param name="requestWaitTimeout">The number of seconds a call may wait for an available COM API
        /// instance.</param>
        /// <param name="exceptionLogFilter">Specifies the HTTP result codes that should not be logged
        /// to the main Extract exception log. Specify <c>null</c> to use the default value or empty
        /// string to log all error codes.</param>
        public ApiContext(string apiVersion,
                          string databaseServerName,
                          string databaseName,
                          ICommonWebConfiguration webConfiguration,
                          string numberOfConnectionRetries = "",
                          string connectionRetryTimeout = "",
                          string maxInterfaces = "",
                          string requestWaitTimeout = "",
                          string exceptionLogFilter = null)
        {
            ApiVersion = ApiVersion.Parse(apiVersion ?? LEGACY_VERSION);

            HTTPError.Assert("ELI46375", !string.IsNullOrWhiteSpace(databaseServerName),
                "Database server name is empty");
            DatabaseServerName = databaseServerName;

            HTTPError.Assert("ELI46376", !string.IsNullOrWhiteSpace(databaseName),
                "Database name is empty");
            DatabaseName = databaseName;

            WebConfiguration = webConfiguration;

            var numberOfRetries =
                !string.IsNullOrWhiteSpace(numberOfConnectionRetries) ?
                numberOfConnectionRetries :
                _defaultRetryCount;

            if (int.TryParse(numberOfRetries, out int retries))
            {
                HTTPError.Assert("ELI46378", retries > 0, "Number of DB connection retries must be > 0");
                NumberOfConnectionRetries = retries;
            }

            var timeoutInterval =
                !string.IsNullOrWhiteSpace(connectionRetryTimeout) ?
                connectionRetryTimeout :
                _defaultRetryTimeout;

            if (int.TryParse(timeoutInterval, out int timeout))
            {
                ConnectionRetryTimeout = timeout;
            }

            if (int.TryParse(maxInterfaces, out int maxInterfacesValue))
            {
                MaxInterfaces = maxInterfacesValue;
            }

            if (int.TryParse(requestWaitTimeout, out int requestWaitTimeoutValue))
            {
                RequestWaitTimeout = requestWaitTimeoutValue;
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
        /// Creates a copy of this instance with equally properties except session specific members
        /// (<see cref="SessionId"/>, <see cref="FAMSessionId"/>).
        /// </summary>
        /// <param name="webConfiguration">The web configuration the clone is to use. If <c>null</c> or whitespace,
        /// it will use the same workflow as this instance.</param>
        public ApiContext CreateCopy()
        {
            var newContext = new ApiContext
            {
                ApiVersion = ApiVersion,
                DatabaseServerName = DatabaseServerName,
                DatabaseName = DatabaseName,
                NumberOfConnectionRetries = NumberOfConnectionRetries,
                ConnectionRetryTimeout = ConnectionRetryTimeout,
                MaxInterfaces = MaxInterfaces,
                RequestWaitTimeout = RequestWaitTimeout,
                ExceptionLogFilter = ExceptionLogFilter,
                WebConfiguration = WebConfiguration
            };

            return newContext;
        }

        private ApiContext()
        {

        }

        /// <summary>
        /// The API version to use.
        /// </summary>
        public ApiVersion ApiVersion { get; private set; }

        /// <summary>
        /// The number of times to retry DB connection on failure
        /// </summary>
        public int NumberOfConnectionRetries { get; private set; }

        /// <summary>
        /// The retry interval in seconds
        /// </summary>
        public int ConnectionRetryTimeout { get; private set; }

        /// <summary>
        /// Specifies the maximum number of concurrent COM API interfaces for a specific database workflow.
        /// Beyond this number, requests will block until an interface becomes available.
        /// </summary>
        public int MaxInterfaces { get; private set; } = 15;

        /// <summary>
        /// The number of seconds a call may wait for an available COM API instance. If more than
        /// this amount of time passes before one is available, an 500 error will be thrown.
        /// </summary>
        public int RequestWaitTimeout { get; private set; } = 60;

        /// <summary>
        /// Specifies the HTTP result codes that should not be logged to the main Extract exception log.
        /// Only needed for codes that pertain to an error or some sort (>= 400).
        /// </summary>
        public IEnumerable<NumericRange> ExceptionLogFilter { get; private set; } = new[] { new NumericRange(404) };

        /// <summary>
        /// The database server name
        /// </summary>
        public string DatabaseServerName { get; private set; }

        /// <summary>
        /// The database name
        /// </summary>
        public string DatabaseName { get; private set; }

        /// <summary>
        /// The web configuration.
        /// </summary>
        public ICommonWebConfiguration WebConfiguration { get; set; }

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
