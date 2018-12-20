using Extract;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using static WebAPI.Utils;

namespace WebAPI.Models
{
    /// <summary>
    /// This class presents a factory method that returns a FileApi instance
    /// </summary>
    static public class FileApiMgr
    {
        static List<FileApi> _interfaces = new List<FileApi>();
        static object _lock = new object();

        /// <summary>
        /// get (an existing unused interface) or make a FAM file processing DB interface
        /// </summary>
        /// <param name="apiContext">the API context to use</param>
        /// <param name="sessionOwner">The <see cref="ClaimsPrincipal"/> this returned instance should be
        /// specific to or <c>null</c> if the instance need not be specific to a particular user.</param>
        /// <returns>a FileApi instance</returns>
        static public FileApi GetInterface(ApiContext apiContext, ClaimsPrincipal sessionOwner = null)
        {
            FileApi fileApi = null;

            try
            {
                HTTPError.Assert("ELI46389", apiContext != null, "empty API context used");

                lock (_lock)
                {
                    Int32.TryParse(sessionOwner?.GetClaim("FAMSessionId"), out int requestedFAMSessionId);

                    fileApi = FindAvailable(apiContext);

                    if (fileApi != null)
                    {
                        var requestedSessionId = sessionOwner?.GetClaim(JwtRegisteredClaimNames.Jti);
                        if (fileApi.Expired ||
                            (!string.IsNullOrWhiteSpace(requestedSessionId) &&
                                (requestedFAMSessionId != 0 &&
                                 (fileApi.SessionId != apiContext.SessionId || requestedFAMSessionId != fileApi.FAMSessionId))))
                        {
                            // If a FAM session was requested the returned instance is expired or is not for that session,
                            // abort the old session.
                            fileApi?.AbortSession(requestedFAMSessionId);

                            throw new HTTPError("ELI45230", StatusCodes.Status401Unauthorized, "Session expired");
                        }

                        fileApi.InUse = true;
                    }
                    else
                    {
                        fileApi = new FileApi(apiContext, setInUse: true);
                        
                        // If a FAM session was requested but a new instance had to be created, abort the old session.
                        if (requestedFAMSessionId > 0)
                        {
                            fileApi.AbortSession(requestedFAMSessionId);

                            throw new HTTPError("ELI46255", StatusCodes.Status401Unauthorized, "Session expired");
                        }

                        _interfaces.Add(fileApi);
                        Log.WriteLine(Inv($"Number of file API interfaces is now: {_interfaces.Count}"), "ELI43251");
                    }

                    return fileApi;
                }
            }
            catch (HTTPError re)
            {
                // fileApi session is already aborted.
                throw re;
            }
            catch (ExtractException ee)
            {
                fileApi?.AbortSession();

                throw new ExtractException("ELI43343", ee.Message, ee);
            }
            catch (Exception ex)
            {
                fileApi?.AbortSession();

                var ee = ex.AsExtract("ELI42160");
                ee.AddDebugData("FileAPI factory failed for workflow:", apiContext.WorkflowName, encrypt: false);
                ee.AddDebugData("database server name", apiContext.DatabaseServerName, encrypt: false);
                ee.AddDebugData("database name", apiContext.DatabaseName, encrypt: false);

                throw ee;
            }
        }

        /// <summary>
        /// Finds an available <see cref="FileApi"/> instance for the specified
        /// apiContext and sessionOwner.
        /// </summary>
        /// <param name="apiContext">the API context to use</param>
        static FileApi FindAvailable(ApiContext apiContext)
        {
            // Try first to look up an instance that was specificially associated with apiContext.
            FileApi availableInstance =
                _interfaces.FirstOrDefault(instance =>
                    !instance.InUse && apiContext.SessionId.Equals(instance.SessionId));

            // If that fails, look for an instance set up for apiContext's DB and workflow that has not
            // been tied to a different context.
            if (availableInstance == null)
            {
                availableInstance = _interfaces.FirstOrDefault(instance =>
                    !instance.InUse &&
                    string.IsNullOrWhiteSpace(instance.SessionId) &&
                    instance.Workflow.Name.IsEquivalent(apiContext.WorkflowName) &&
                    instance.Workflow.DatabaseServerName.IsEquivalent(apiContext.DatabaseServerName) &&
                    instance.Workflow.DatabaseName.IsEquivalent(apiContext.DatabaseName));
            }

            return availableInstance;
        }

        /// <summary>
        /// Use for unit testing to close all DB connections
        /// </summary>
        public static void ReleaseAll()
        {
            foreach (var inf in _interfaces)
            {
                inf.FileProcessingDB.CloseAllDBConnections();
            }
            _interfaces.Clear();
        }
    }
}
