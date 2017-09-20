using Extract;
using System;
using static WebAPI.Utils;

namespace WebAPI.Models
{
    /// <summary>
    /// Workflow data model class
    /// </summary>
    public class WorkflowData
    {
        /// <summary>
        /// get the workflow status
        /// </summary>
        /// <param name="apiContext">User's API context</param>
        /// <returns>WorkflowStatus object</returns>
        public static WorkflowStatus GetWorkflowStatus(ApiContext apiContext)
        {
            FileApi fileApi = null;

            try
            {
                fileApi = FileApiMgr.GetInterface(apiContext);
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
                    NumberIncomplete = unattempted
                };
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI42181");
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
