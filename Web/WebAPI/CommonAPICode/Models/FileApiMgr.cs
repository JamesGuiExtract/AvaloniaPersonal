using Extract;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using static WebAPI.Utils;

namespace WebAPI.Models
{
    /// Presents a factory method that returns a FileApi instance
    public interface IFileApiMgr
    {
        /// <summary>
        /// Get (an existing unused interface) or make a FAM file processing DB interface
        /// </summary>
        /// <param name="apiContext">The API context to use</param>
        /// <param name="sessionOwner">The <see cref="ClaimsPrincipal"/> this returned instance should be
        /// specific to or <c>null</c> if the instance need not be specific to a particular user.</param>
        /// <returns>a FileApi instance</returns>
        IFileApi GetInterface(ApiContext apiContext, ClaimsPrincipal sessionOwner = null);

        /// For unit testing, to close all DB connections
        void ReleaseAll();
    }

    /// <inheritdoc/>
    public class FileApiMgr : IFileApiMgr
    {
        readonly List<FileApi> _interfaces = new();
        readonly object _lock = new();

        // -1 indicates that items should not be removed from the queue via a WaitForTurn call;
        // items will need to be explicitly removed.
        readonly Sequencer<long> _sequencer = new(0, requireExplicitRemoval: true);
        long _waitingInstanceId = 0;

        private FileApiMgr() { }

        /// The singleton instance of this class
        public static FileApiMgr Instance { get; } = new();

        /// <inheritdoc/>
        public IFileApi GetInterface(ApiContext apiContext, ClaimsPrincipal sessionOwner = null)
        {
            HTTPError.Assert("ELI46389", apiContext != null, "empty API context used");

            FileApi fileApi = null;
            bool waiting = false;
            int remainingTimeout = 60000;
            var waitTime = new Stopwatch();
            waitTime.Start();

            int requestedFAMSessionID = 0;
            string requestedSessionID = "";
            if (sessionOwner != null)
            {
                Int32.TryParse(sessionOwner.GetClaim("FAMSessionId"), out requestedFAMSessionID);
                requestedSessionID = sessionOwner.GetClaim(JwtRegisteredClaimNames.Jti);

                ExtractException.Assert("ELI46662", "Logic error", !string.IsNullOrWhiteSpace(requestedSessionID));
            }

            long requestId = Interlocked.Increment(ref _waitingInstanceId);
            _sequencer.AddToQueue(requestId);

            try
            {
                try
                {
                    // Will throw exception if timeout expires
                    _sequencer.WaitForTurn(requestId, apiContext.RequestWaitTimeout * 1000);
                }
                catch (Exception ex)
                {
                    throw new HTTPError("ELI46633", StatusCodes.Status503ServiceUnavailable,
                        "Timeout waiting to process request", ex);
                }

                while (fileApi == null)
                {
                    if (waiting)
                    {
                        remainingTimeout = Math.Max(0,
                            (apiContext.RequestWaitTimeout * 1000) - (int)waitTime.ElapsedMilliseconds);
                        HTTPError.Assert("ELI46635", StatusCodes.Status503ServiceUnavailable,
                            FileApi.WaitForInstanceNoLongerInUse(remainingTimeout), "Timeout waiting to process request");
                    }

                    lock (_lock)
                    {
                        if (requestId != _sequencer.Peek())
                        {
                            continue;
                        }

                        fileApi = FindAvailable(apiContext);

                        if (fileApi != null)
                        {
                            fileApi.InUse = true;

                            if (requestedFAMSessionID > 0)
                            {
                                fileApi.ResumeSession(requestedFAMSessionID);
                            }
                        }
                        else if (!waiting)
                        {
                            // The number of instances being used for this context.
                            int instanceCount = _interfaces.Count(instance =>
                                instance.Workflow.Name.IsEquivalent(apiContext.WorkflowName) &&
                                instance.Workflow.DatabaseServerName.IsEquivalent(apiContext.DatabaseServerName) &&
                                instance.Workflow.DatabaseName.IsEquivalent(apiContext.DatabaseName));

                            if (instanceCount < apiContext.MaxInterfaces)
                            {
                                fileApi = new FileApi(apiContext, setInUse: true);
                                fileApi.Releasing += HandleFileApi_Releasing;
                                _interfaces.Add(fileApi);
                                Log.WriteLine(Inv($"Number of file API interfaces is now: {_interfaces.Count}"), "ELI43251");

                                if (requestedFAMSessionID > 0)
                                {
                                    fileApi.ResumeSession(requestedFAMSessionID);
                                }
                            }
                            else
                            {
                                waiting = true;
                            }
                        }
                    }
                }

                return fileApi;
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
            finally
            {
                _sequencer.Remove(requestId);
            }
        }

        /// <summary>
        /// Handles the Releasing event of a FileApi instance.
        /// </summary>
        void HandleFileApi_Releasing(object sender, EventArgs e)
        {
            try
            {
                var fileApi = (FileApi)sender;

                if (_sequencer.Count == 0 || fileApi.UsesSinceClose > 100)
                {
                    fileApi.FileProcessingDB.CloseAllDBConnections();
                    fileApi.UsesSinceClose = 0;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractLog("ELI46638");
            }
        }

        /// <summary>
        /// Finds an available <see cref="FileApi"/> instance for the specified
        /// apiContext and sessionOwner.
        /// </summary>
        /// <param name="apiContext">the API context to use</param>
        FileApi FindAvailable(ApiContext apiContext)
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

        /// <inheritdoc/>
        public void ReleaseAll()
        {
            foreach (var inf in _interfaces)
            {
                inf.FileProcessingDB.CloseAllDBConnections();
                inf.UsesSinceClose = 0;
            }
            _interfaces.Clear();
        }
    }
}
