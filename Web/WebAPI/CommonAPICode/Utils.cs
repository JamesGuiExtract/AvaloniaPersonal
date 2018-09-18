using Extract;
using Microsoft.AspNetCore.Hosting;     // for IHostingEnvironment
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using WebAPI.Models;

namespace WebAPI
{
    /// <summary>
    /// static Utils are kept here for global use
    /// </summary>
    public static class Utils
    {
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
        /// make an error info instance
        /// </summary>
        /// <param name="isError">true or false</param>
        /// <param name="message">error message</param>
        /// <param name="code">error code (-1 for error, 0 for no error)</param>
        /// <returns>error info instance</returns>
        public static ErrorInfo MakeError(bool isError, string message = "", int code = 0)
        {
            return new ErrorInfo
            {
                ErrorOccurred = isError,
                Message = message,
                Code = code
            };
        }

        /// <summary>
        /// makes a document attribute set for returning an error
        /// </summary>
        /// <param name="message">error mesasge</param>
        /// <returns>DocumentAttributeSet instance with the error info set</returns>
        public static DocumentAttributeSet MakeDocumentAttributeSetError(string message)
        {
            return new DocumentAttributeSet
            {
                Error = new ErrorInfo
                {
                    ErrorOccurred = true,
                    Message = message,
                    Code = -1
                },

                Attributes = null
            };
        }

        /// <summary>
        /// make a document attribute set for a successful return case
        /// </summary>
        /// <returns>new DocumentAttributeSet</returns>
        public static DocumentAttributeSet MakeNewDocumentAttributeSet()
        {
            return new DocumentAttributeSet
            {
                Error = MakeError(isError: false),
                Attributes = new List<DocumentAttribute>()
            };
        }

        /// <summary>
        /// makes a WorkflowStatus with an error message
        /// </summary>
        /// <param name="message"></param>
        /// <returns>a workflow status instance with the error info set</returns>
        public static WorkflowStatus MakeWorkflowStatusError(string message)
        {
            return new WorkflowStatus
            {
                Error = Utils.MakeError(isError: true,
                                        message: message,
                                        code: -1)
            };
        }

        /// <summary>
        /// environment getter/setter
        /// </summary>
        public static IHostingEnvironment environment
        {
            get
            {
                Contract.Assert(_environment != null, "environment is null");
                return _environment;
            }
            
            set
            {
                Contract.Assert(value != null, "environment is being set to null");
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
        /// makes a DocumentSubmitResult
        /// </summary>
        /// <param name="fileId">file id</param>
        /// <param name="isError">true or false</param>
        /// <param name="message">empty, or error message</param>
        /// <param name="code">error code value, 0 (no error) or -1 (error)</param>
        /// <param name="submitType">file or text submission type</param>
        /// <returns>completed DocumentSubmitResult object</returns>
        public static DocumentSubmitResult MakeDocumentSubmitResult(int fileId, 
                                                                    bool isError = false, 
                                                                    string message = "", 
                                                                    int code = 0,
                                                                    DocumentSubmitType submitType = DocumentSubmitType.File)
        {
            DocumentSubmitResult result = new DocumentSubmitResult()
            {
                Id = isError ? -1 : fileId,
                Error = MakeError(isError: isError, message: message, code: code)
            };

            return result;
        }

        /// <summary>
        /// routine to simplify making a ProcessingStatus instance
        /// </summary>
        /// <param name="status">status value</param>
        /// <param name="isError">true if error, false otherwise, defaults to false</param>
        /// <param name="message">error message, defaults to empty</param>
        /// <param name="code">error code, defaults to zero (no error)</param>
        /// <returns>completed ProcessingStatus object</returns>
        public static ProcessingStatus MakeProcessingStatus(DocumentProcessingStatus status,
                                                            bool isError = false,
                                                            string message = "",
                                                            int code = 0)
        {
            return new ProcessingStatus()
            {
                DocumentStatus = status,
                StatusText = Enum.GetName(typeof(DocumentProcessingStatus), status),
                Error = MakeError(isError, message, code)
            };
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
        /// makes a TextResult object
        /// </summary>
        /// <param name="text">text of the TextResult</param>
        /// <param name="isError">true if error</param>
        /// <param name="errorMessage">error message</param>
        /// <returns>completed TextResult object</returns>
        public static TextResult MakeTextResult(string text, bool isError = false, string errorMessage = "")
        {
            return new TextResult
            {
                Text = text,
                Error = MakeError(isError, errorMessage, isError == true ? -1 : 0)
            };
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
                    Contract.Assert(_currentApiContext != null, "Default API context is not set");
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
        public static void SetCurrentApiContext(string databaseServerName, 
                                                string databaseName, 
                                                string workflowName,
                                                string dbNumberOfConnectionRetries = "",
                                                string dbConnectionRetryTimeout = "")
        {
            try
            {
                lock (_apiContextLock)
                {
                    _currentApiContext = new ApiContext(databaseServerName, 
                                                        databaseName, 
                                                        workflowName,
                                                        dbNumberOfConnectionRetries,
                                                        dbConnectionRetryTimeout);
                }
            }
            catch (Exception ex)
            {
                var ee = ex.AsExtract("ELI42161");
                throw ee;
            }
        }

        /// <summary>
        /// set the default API context instance - an overload of above for convenience
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
        /// "apply" the current API context - this creates a FileApi member using the context, useful for
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
        /// the JWT Issuer (iss: )
        /// </summary>
        public static string Issuer
        {
            get
            {
                return "DocumentAPIv1";
            }
        }

        /// <summary>
        /// the JWT Audience (aud: )
        /// </summary>
        public static string Audience
        {
            get
            {
                return "ESWebClients";
            }
        }

        /// <summary>
        /// create an ApiContext object from Claims.
        /// The JWT is expected to have non-empty values for all of the API context members
        /// NOTE: This function is only intended for use by Controller methods
        /// </summary>
        /// <param name="user">the Controller.User instance</param>
        /// <returns> an API context object</returns>
        public static ApiContext ClaimsToContext(ClaimsPrincipal user)
        {
            try
            {
                var workflowName = user.Claims.Where(claim => claim.Type == "WorkflowName").Select(claim => claim.Value).FirstOrDefault();
                var databaseServerName = CurrentApiContext.DatabaseServerName;
                var databaseName = CurrentApiContext.DatabaseName;

                Contract.Assert(!String.IsNullOrWhiteSpace(databaseServerName), "Database server name is empty, from Claims");
                Contract.Assert(!String.IsNullOrWhiteSpace(databaseName), "Database name is empty");
                Contract.Assert(!String.IsNullOrWhiteSpace(workflowName), "Workflow name is empty");

                var context = new ApiContext(databaseServerName, databaseName, workflowName);
                context.SessionId = user.GetClaim(JwtRegisteredClaimNames.Jti);
                context.FAMSessionId = user.Claims
                    .Where(claim => claim.Type.Equals("FAMSessionId", StringComparison.OrdinalIgnoreCase))
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
        /// Gets the value for the specified <see paramref="claimName"/>.
        /// </summary>
        /// <param name="claimsPrincipal">The claims principal.</param>
        /// <param name="claimName">Name of the claim.</param>
        /// <returns></returns>
        public static string GetClaim(this ClaimsPrincipal claimsPrincipal, string claimName)
        {
            return claimsPrincipal.Claims
                .Single(claim => claim.Type.Equals(claimName, StringComparison.OrdinalIgnoreCase))
                .Value;
        }

        /// <summary>
        /// create an ApiContext object.
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
    }
}
