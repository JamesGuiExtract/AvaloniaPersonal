using System;
using System.Linq;
using Extract;
using Extract.Web.ApiConfiguration.Models;

namespace WebAPI
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
        public static WorkflowStatusResult GetWorkflowStatus(ApiContext apiContext)
        {
            IFileApi fileApi = null;

            try
            {
                fileApi = FileApiMgr.Instance.GetInterface(apiContext);
                var fileProcessingDB = fileApi.FileProcessingDB;

                string actionName = null;
                if (apiContext.WebConfiguration is IRedactionWebConfiguration redactionConfiguration)
                {
                    actionName = redactionConfiguration.PostProcessingAction;
                }
                else if (apiContext.WebConfiguration is IDocumentApiWebConfiguration documentAPIconfiguration)
                {
                    actionName = documentAPIconfiguration.EndWorkflowAction;
                }

                fileProcessingDB.GetAggregateWorkflowStatus(actionName, out int unattempted,
                                                           out int processing,
                                                           out int completed,
                                                           out int failed);
                return new WorkflowStatusResult
                {
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
                }
            }
        }

        /// <summary>
        /// get the workflow status
        /// </summary>
        /// <param name="apiContext">User's API context</param>
        /// <returns>WorkflowStatus object</returns>
        public static FileStatusResult GetDocumentStatuses(ApiContext apiContext)
        {
            IFileApi fileApi = null;

            try
            {
                fileApi = FileApiMgr.Instance.GetInterface(apiContext);
                var fileProcessingDB = fileApi.FileProcessingDB;
    
                string statusListing = fileProcessingDB.GetWorkflowStatusAllFiles(fileApi.APIWebConfiguration.EndWorkflowAction);

                var fileStatuses = 
                    statusListing.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(status =>
                        {
                            var statusParts = status.Split(':');
                            string statusString;
                            switch (statusParts[1][0])
                            {
                                case 'R': statusString = "Processing"; break;
                                case 'C': statusString = "Done"; break;
                                case 'F': statusString = "Failed"; break;
                                case 'U': statusString = "Incomplete"; break;
                                default:
                                    throw new HTTPError("ELI46411", "Failed to parse file status");
                            }

                            return new FileStatus()
                            {

                                ID = int.Parse(statusParts[0]),
                                Status = statusString
                            };
                        });

                return new FileStatusResult
                {
                    FileStatuses = fileStatuses.ToList()
                };
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI46412");
            }
            finally
            {
                if (fileApi != null)
                {
                    fileApi.InUse = false;
                }
            }
        }
    }
}
