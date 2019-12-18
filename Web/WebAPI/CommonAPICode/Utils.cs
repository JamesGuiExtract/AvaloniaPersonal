﻿using Extract;
using Microsoft.AspNetCore.Hosting;     // for IHostingEnvironment
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading;
using UCLID_COMUTILSLib;
using WebAPI.Models;

namespace WebAPI
{
    /// <summary>
    /// static Utils are kept here for global use
    /// </summary>
    public static class Utils
    {
        /// <summary>
        /// The name of the workflow name claim used for ClaimsPrincipals
        /// </summary>
        public static string _WORKFLOW_NAME = "WorkflowName";

        /// <summary>
        /// The name of the FAMSessionID claim used for ClaimsPrincipals
        /// </summary>
        public static string _FAM_SESSION_ID = "FAMSessionId";

        /// <summary>
        /// The name of the ExpiresTime claim used for ClaimsPrincipals 
        /// </summary>
        public static string _EXPIRES_TIME = "ExpiresTime";

        private static IHostingEnvironment _environment = null;
        private static ApiContext _currentApiContext = null;
        private static object _apiContextLock = new Object();

        /// <summary>
        /// Inv - short form of Invariant. Normally I would use the full name, but in this case the 
        /// full name is just noise, a distraction from the more important functionality. All this
        /// function does is prevent FXCop warnings!
        /// </summary>
        /// <param name="strings">strings - one or more strings to format</param>
        /// <returns>string</returns>
        public static string Inv(params FormattableString[] strings)
        {
            return string.Join("", strings.Select(str => FormattableString.Invariant(str)));
        }

        /// <summary>
        /// environment getter/setter
        /// </summary>
        public static IHostingEnvironment environment
        {
            get
            {
                HTTPError.Assert("ELI46363", _environment != null, "Environment is null");
                return _environment;
            }

            set
            {
                HTTPError.Assert("ELI46364", value != null, "Environment is being set to null");
                _environment = value;
            }
        }


        /// <summary>
        /// String compare made easier to use and read...
        /// Note that this always uses CultureInfo.InvariantCulture
        /// </summary>
        /// <param name="s1">string 1 - the "this" parameter</param>
        /// <param name="s2">the string to compare "this" too</param>
        /// <param name="ignoreCase">true to ignore case</param>
        /// <returns>true if string matches</returns>
        public static bool IsEquivalent(this string s1,
                                        string s2,
                                        bool ignoreCase = true)
        {
            if (String.Compare(s1, s2, ignoreCase, CultureInfo.InvariantCulture) == 0)
                return true;

            return false;
        }

        /// <summary>
        /// returns the method name of the caller - do NOT set the default argument!
        /// </summary>
        /// <param name="caller">do not set this!</param>
        /// <returns>the method name of the caller</returns>
        public static string GetMethodName([CallerMemberName] string caller = null)
        {
            return caller;
        }

        /// <summary>
        /// get the current api context
        /// </summary>
        public static ApiContext CurrentApiContext
        {
            get
            {
                lock (_apiContextLock)
                {
                    HTTPError.Assert("ELI46368", _currentApiContext != null,
                        "Default API context is not set");
                    return _currentApiContext;
                }
            }
        }

        /// <summary>
        /// set the default API context instance
        /// </summary>
        /// <param name="databaseServerName">database server name</param>
        /// <param name="databaseName">database name</param>
        /// <param name="workflowName">workflow name</param>
        /// <param name="dbNumberOfConnectionRetries">number of times to retry DB connection on failure</param>
        /// <param name="dbConnectionRetryTimeout">timeout value in seconds</param>
        /// <param name="maxInterfaces">Specifies the maximum number of concurrent COM API interfaces for
        /// a specific database workflow.</param>
        /// <param name="requestWaitTimeout">The number of seconds a call may wait for an available COM API
        /// instance.</param>
        /// <param name="exceptionLogFilter">Specifies the HTTP result codes that should not be logged
        /// to the main Extract exception log. Specify <c>null</c> to use the default value or empty
        /// string to log all error codes.</param>
        public static void SetCurrentApiContext(string databaseServerName,
                                                string databaseName,
                                                string workflowName,
                                                string dbNumberOfConnectionRetries = "",
                                                string dbConnectionRetryTimeout = "",
                                                string maxInterfaces = null,
                                                string requestWaitTimeout = null,
                                                string exceptionLogFilter = null)
        {
            try
            {
                lock (_apiContextLock)
                {
                    _currentApiContext = new ApiContext(databaseServerName,
                                                        databaseName,
                                                        workflowName,
                                                        dbNumberOfConnectionRetries,
                                                        dbConnectionRetryTimeout,
                                                        maxInterfaces,
                                                        requestWaitTimeout,
                                                        exceptionLogFilter);
                }
            }
            catch (Exception ex)
            {
                var ee = ex.AsExtract("ELI42161");
                throw ee;
            }
        }

        /// <summary>
        /// Set the default API context instance - an overload of above for convenience
        /// NOTE: this overload is used only by nunit tests currently
        /// </summary>
        /// <param name="apiContext"></param>
        public static void SetCurrentApiContext(ApiContext apiContext)
        {
            try
            {
                lock (_apiContextLock)
                {
                    _currentApiContext = apiContext;
                }
            }
            catch (Exception ex)
            {
                var ee = ex.AsExtract("ELI42165");
                throw ee;
            }
        }

        /// <summary>
        /// Validates the current API context - this creates a FileApi member using the context, useful for
        /// checking that the named workflow actually exists in the configured DatabaseServer/Database.
        /// </summary>
        public static void ValidateCurrentApiContext()
        {
            try
            {
                lock (_apiContextLock)
                {
                    new FileApi(_currentApiContext, setInUse: false);
                }
            }
            catch (Exception ex)
            {
                var ee = ex.AsExtract("ELI43274");
                throw ee;
            }
        }


        /// <summary>
        /// The JWT Issuer (iss: )
        /// </summary>
        public static string Issuer
        {
            get
            {
                return "DocumentAPIv1";
            }
        }

        /// <summary>
        /// The JWT Audience (aud: )
        /// </summary>
        public static string Audience
        {
            get
            {
                return "ESWebClients";
            }
        }

        /// <summary>
        /// Creates an ApiContext object from Claims.
        /// The JWT is expected to have non-empty values for all of the API context members
        /// NOTE: This function is only intended for use by Controller methods
        /// </summary>
        /// <param name="user">the Controller.User instance</param>
        /// <returns> an API context object</returns>
        public static ApiContext ClaimsToContext(ClaimsPrincipal user)
        {
            try
            {
                var workflowName = user.Claims.Where(claim => claim.Type == _WORKFLOW_NAME).Select(claim => claim.Value).FirstOrDefault();
                var databaseServerName = CurrentApiContext.DatabaseServerName;
                var databaseName = CurrentApiContext.DatabaseName;

                HTTPError.Assert("ELI46369", !String.IsNullOrWhiteSpace(databaseServerName),
                    "Database server name not provided");
                HTTPError.Assert("ELI46370", !String.IsNullOrWhiteSpace(databaseName),
                    "Database name is not provided");
                HTTPError.Assert("ELI46371", !String.IsNullOrWhiteSpace(workflowName),
                    "Workflow name is not provided");

                var context = new ApiContext(databaseServerName, databaseName, workflowName);
                context.SessionId = user.GetClaim(JwtRegisteredClaimNames.Jti);
                context.FAMSessionId = user.Claims
                    .Where(claim => claim.Type.Equals(_FAM_SESSION_ID, StringComparison.OrdinalIgnoreCase))
                    .Select(claim => Int32.TryParse(claim.Value, out int id) ? id : 0)
                    .FirstOrDefault();

                return context;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI43662");
            }
        }

        /// <summary>
        /// Gets the value for the specified claimName.
        /// </summary>
        /// <param name="claimsPrincipal">The claims principal.</param>
        /// <param name="claimName">Name of the claim.</param>
        /// <returns></returns>
        public static string GetClaim(this ClaimsPrincipal claimsPrincipal, string claimName)
        {
            return claimsPrincipal.Claims
                .SingleOrDefault(claim => claim.Type.Equals(claimName, StringComparison.OrdinalIgnoreCase))
                ?.Value;
        }

        /// <summary>
        /// Gets the username associated with the claimsPrincipal.
        /// </summary>
        /// <param name="claimsPrincipal">The claims principal.</param>
        /// <returns></returns>
        public static string GetUsername(this ClaimsPrincipal claimsPrincipal)
        {
            var usernameClaim = claimsPrincipal.Claims
                .SingleOrDefault(claim =>
                    claim.Type.Equals(JwtRegisteredClaimNames.Sub, StringComparison.OrdinalIgnoreCase));

            if (usernameClaim == null)
            {
                usernameClaim = claimsPrincipal.Claims
                .SingleOrDefault(claim =>
                    claim.Type.Equals("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier",
                        StringComparison.OrdinalIgnoreCase));
            }

            ExtractException.Assert("ELI46291", "Username not found", usernameClaim != null);

            return usernameClaim.Value;
        }

        /// <summary>
        /// This checks for the Custom claims added with a Login or SessionLogin
        /// </summary>
        /// <param name="claimsPrincipal">The claims principal</param>
        /// <returns></returns>
        public static bool HasExpectedClaims(this ClaimsPrincipal claimsPrincipal)
        {
            List<string> claimNames = new List<string>()
            {
                _EXPIRES_TIME,
                _WORKFLOW_NAME,
                _FAM_SESSION_ID
            };
            return claimsPrincipal.Claims.Where(c => claimNames.Contains(c.Type)).Count() == claimNames.Count();
        }

        /// <summary>
        /// Creates an ApiContext object.
        /// </summary>
        /// <param name="workflowName">the user-specified workflow name</param>
        /// <returns> an API context object</returns>
        public static ApiContext LoginContext(string workflowName)
        {
            // Get the current API context once to ensure thread safety.
            var context = CurrentApiContext;

            var databaseServerName = context.DatabaseServerName;
            var databaseName = context.DatabaseName;

            var namedWorkflow = !String.IsNullOrWhiteSpace(workflowName) ? workflowName : context.WorkflowName;

            return new ApiContext(databaseServerName, databaseName, namedWorkflow);
        }

        /// <summary>
        /// Converts comVector into an enumerable.
        /// </summary>
        /// <typeparam name="T">The type of object in the vector.</typeparam>
        /// <param name="comVector">The <see cref="IIUnknownVector"/> to convert.</param>
        /// <returns>An enumerable of type T.</returns>
        public static IEnumerable<T> ToIEnumerable<T>(this IIUnknownVector comVector)
        {
            int size = comVector.Size();

            for (int i = 0; i < size; i++)
            {
                yield return (T)comVector.At(i);
            }
        }

        /// <summary>
        /// Converts <see paramref="enumerable"/> into an <see cref="IIUnknownVector"/>.
        /// </summary>
        /// <typeparam name="T">The type of object in the enumerable.</typeparam>
        /// <param name="enumerable">The <see cref="IEnumerable{T}"/> to convert.</param>
        /// <returns>An <see cref="IIUnknownVector"/> of type <see paramref="T"/>.</returns>
        public static IUnknownVector ToIUnknownVector<T>(this IEnumerable<T> enumerable)
        {
            try
            {
                IUnknownVector vector = new IUnknownVector();
                foreach (T value in enumerable)
                {
                    vector.PushBack(value);
                }

                return vector;
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI46745", ex);
            }
        }

        internal static void ReportMemoryUsage(object comObject)
        {
            try
            {
                IManageableMemory manageableMemoryObject = comObject as IManageableMemory;
                ExtractException.Assert("ELI46752", "COM object memory is not manageable.",
                    manageableMemoryObject != null);

                manageableMemoryObject.ReportMemoryUsage();
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI46753", ex);
            }
        }

        /// <summary>
        /// Used to validate identifiers (identifiers must start with either an underscore
        /// or a letter and can be followed by 0 or more underscores, letters or numbers).
        /// </summary>
        static ThreadLocal<Regex> _identifierValidator = new ThreadLocal<Regex>(() => new Regex(@"^[_a-zA-Z]\w*$"));

        /// <summary>
        /// Determines whether all of the specified identifiers are valid.
        /// <para>Note:</para>
        /// A valid identifier must be of the form '[_a-zA-Z]\w*'
        /// </summary>
        /// <param name="identifiers">The identifiers.</param>
        /// <returns>
        /// <see langword="true"/> if all of the identifiers are valid;
        /// otherwise, <see langword="false"/>.
        /// </returns>
        public static bool IsValidIdentifier(params string[] identifiers)
        {
            try
            {
                return identifiers.All(s => _identifierValidator.Value.IsMatch(s));
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI48352");
            }
        }

        /// <summary>
        /// Retrieves the remote IP address for the specified request. If not available, assume assume 127.0.0.1.
        /// In OpenSession calls, the IP address is used to identify the caller via the "Machine" column in FAMSession.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequest"/></param>
        public static string GetIpAddress(this HttpRequest request)
        {
            try
            {
                string ipAddress = (request.HttpContext.Connection.RemoteIpAddress ?? IPAddress.Parse("127.0.0.1")).ToString();
                return ipAddress;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI49579");
            }
        }
    }
}
