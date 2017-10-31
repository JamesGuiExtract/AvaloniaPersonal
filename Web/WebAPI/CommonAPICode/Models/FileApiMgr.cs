using Extract;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
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
            try
            {
                Contract.Assert(apiContext != null, "empty API context used");

                lock (_lock)
                {
                    var fileApi = FindAvailable(apiContext, sessionOwner);
                    if (fileApi != null)
                    {
                        fileApi.InUse = true;
                        return fileApi;
                    }

                    var fa = new FileApi(apiContext, setInUse: true, sessionOwner: sessionOwner);
                    _interfaces.Add(fa);
                    Log.WriteLine(Inv($"Number of file API interfaces is now: {_interfaces.Count}"), "ELI43251");

                    return fa;
                }
            }
            catch (ExtractException ee)
            {
                throw new ExtractException("ELI43343", ee.Message, ee);
            }
            catch (Exception ex)
            {
                var ee = ex.AsExtract("ELI42160");
                ee.AddDebugData("FileAPI factory failed for workflow:", apiContext.WorkflowName, encrypt: false);
                ee.AddDebugData("database server name", apiContext.DatabaseServerName, encrypt: false);
                ee.AddDebugData("database name", apiContext.DatabaseName, encrypt: false);

                throw ee;
            }
        }


        /// <summary>
        /// Make a new interface object - this is done whenever the default API context changes
        /// </summary>
        static public void MakeInterface(ApiContext apiContext, ClaimsPrincipal sessionOwner)
        {
            try
            {
                lock (_lock)
                {
                    // Only add the new interface if it is not already present and available
                    var fileApi = FindAvailable(apiContext, sessionOwner);
                    if (fileApi == null)
                    {
                        var fa = new FileApi(apiContext, setInUse: false, sessionOwner: sessionOwner);
                        _interfaces.Add(fa);
                        Log.WriteLine(Inv($"Number of file API interfaces is now: {_interfaces.Count}"), "ELI43252");
                    }
                }
            }
            catch (Exception ex)
            {
                var ee = ex.AsExtract("ELI42160");
                ee.AddDebugData("FileAPI creation failed for workflow:", apiContext.WorkflowName, encrypt: false);
                ee.AddDebugData("database server name", apiContext.DatabaseServerName, encrypt: false);
                ee.AddDebugData("database name", apiContext.DatabaseName, encrypt: false);
                Log.WriteLine(ee);

                throw ee;
            }
        }


        /// <summary>
        /// Finds an available <see cref="FileApi"/> instance for the specified
        /// <see paramref="apiContext"/> and <see paramref="sessionOwner"/>.
        /// </summary>
        /// <param name="apiContext">the API context to use</param>
        /// <param name="sessionOwner">The <see cref="ClaimsPrincipal"/> this returned instance should be
        /// specific to or <c>null</c> if the instance need not be specific to a particular user.</param>
        /// <returns></returns>
        static FileApi FindAvailable(ApiContext apiContext, ClaimsPrincipal sessionOwner = null)
        {
            var availableInstance = _interfaces.FirstOrDefault(instance =>
                 !instance.InUse &&
                 instance.Workflow.Name.IsEquivalent(apiContext.WorkflowName) &&
                 instance.Workflow.DatabaseServerName.IsEquivalent(apiContext.DatabaseServerName) &&
                 instance.Workflow.DatabaseName.IsEquivalent(apiContext.DatabaseName) &&
                 (sessionOwner == null || instance.SessionId.Equals(sessionOwner.GetClaim("jti"))));

            if (availableInstance != null && availableInstance.Expired)
            {
                throw new RequestAssertion("ELI45230", "Session expired", StatusCodes.Status401Unauthorized);
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
