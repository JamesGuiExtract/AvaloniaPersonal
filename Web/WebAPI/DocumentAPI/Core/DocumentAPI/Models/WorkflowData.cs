using Extract;
using System;
using static DocumentAPI.Utils;

namespace DocumentAPI.Models
{
    /// <summary>
    /// Workflow data model class
    /// </summary>
    public class WorkflowData
    {
        /// <summary>
        /// get the workflow status
        /// </summary>
        /// <param name="workflowName">workflow name</param>
        /// <returns>WorkflowStatus object</returns>
        public static WorkflowStatus GetWorkflowStatus(string workflowName)
        {
            FileApi fileApi = null;

            try
            {
                if (String.IsNullOrEmpty(workflowName))
                {
                    return MakeWorkflowStatusError("workFlowName argument is empty");
                }

                // The workflow name specified might be the name used in the current api context, 
                // or it could be a different workflow name. Treat it as if it were different; if not,
                // no problem. If the workflowName specified is not valid, this will be determined
                // by FileApiMgr.GetInterface below.
                var apiContext = CurrentApiContext;
                var context = new ApiContext(apiContext.DatabaseServerName, apiContext.DatabaseName, workflowName);
    
                // Two possibilites here: 
                // 1) file api mgr GetInterface() call will find a matching file API and return it, or
                // 2) it will attempt to create a new file api, which will throw if an unknown 
                //      workflowName has been specified 
                fileApi = FileApiMgr.GetInterface(context);
    
                var fileProcessingDB = fileApi.Interface;
    
                fileProcessingDB.GetWorkflowStatusAllFiles(out int unattempted,
                                                           out int processing,
                                                           out int completed,
                                                           out int failed);
                return new WorkflowStatus
                {
                    Error = MakeError(false),
                    NumberProcessing = processing,
                    NumberDone = completed,
                    NumberFailed = failed,
                    NumberUnattempted = unattempted
                };
            }
            catch (Exception ex)
            {
                var ee = ex.AsExtract("ELI42181");
                Log.WriteLine(ee);
                return MakeWorkflowStatusError(ee.Message);
            }
            finally
            {
                if (fileApi != null)
                {
                    fileApi.InUse = false;
                    fileApi = null;
                }
            }
        }
    }
}
