using Extract;
using System;
using System.Collections.Generic;
using static DocumentAPI.Utils;

namespace DocumentAPI.Models
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
        /// <returns>a FileApi instance</returns>
        static public FileApi GetInterface(ApiContext apiContext)
        {
            try
            {
                Contract.Assert(apiContext != null, "empty API context used");

                lock (_lock)
                {
                    var fileApi = FindAvailable(apiContext);
                    if (fileApi != null)
                    {
                        fileApi.InUse = true;
                        return fileApi;
                    }

                    var fa = new FileApi(apiContext, setInUse: true);
                    _interfaces.Add(fa);
                    Log.WriteLine(Inv($"Number of file API interfaces is now: {_interfaces.Count}"));

                    return fa;
                }
            }
            catch (Exception ex)
            {
                var ee = ex.AsExtract("ELI42160");
                ee.AddDebugData("FileAPI factory failed for workflow:", apiContext.WorkflowName, encrypt: false);
                ee.AddDebugData("database server name", apiContext.DatabaseServerName, encrypt: false);
                ee.AddDebugData("database name", apiContext.DatabaseName, encrypt: false);
                Log.WriteLine(ee);

                throw ee;
            }
        }


        /// <summary>
        /// Make a new interface object - this is done whenever the default API context changes
        /// </summary>
        static public void MakeInterface(ApiContext apiContext)
        {
            try
            {
                lock (_lock)
                {
                    // Only add the new interface if it is not already present and available
                    var fileApi = FindAvailable(apiContext);
                    if (fileApi == null)
                    {
                        var fa = new FileApi(apiContext, setInUse: false);
                        _interfaces.Add(fa);
                        Log.WriteLine(Inv($"Number of file API interfaces is now: {_interfaces.Count}"));
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


        static FileApi FindAvailable(ApiContext apiContext)
        {
            for (int i = 0; i < _interfaces.Count; ++i)
            {
                var fileApi = _interfaces[i];
                var workflow = fileApi.GetWorkflow;
                if (fileApi.InUse == false &&
                    workflow.Name.IsEquivalent(apiContext.WorkflowName) &&
                    workflow.DatabaseServerName.IsEquivalent(apiContext.DatabaseServerName) &&
                    workflow.DatabaseName.IsEquivalent(apiContext.DatabaseName))
                {
                    return fileApi;
                }
            }

            return null;
        }

        /// <summary>
        /// Use for unit testing to close all DB connections
        /// </summary>
        public static void ReleaseAll()
        {
            foreach (var inf in _interfaces)
            {
                inf.Interface.CloseAllDBConnections();
            }
            _interfaces.Clear();
        }

    }
}
