using DocumentAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DocumentAPI.Controllers
{
    /// <summary>
    /// workflow controller
    /// </summary>
    [Authorize]
    [Route("api/[controller]")]
    public class WorkflowController : Controller
    {
        /// <summary>
        /// get status of specified workflow
        /// </summary>
        /// <param name="workflowName"></param>
        /// <returns></returns>
        [HttpGet("GetWorkflowStatus/{workflowName}")]
        public WorkflowStatus GetWorkflowStatus(string workflowName)
        {
            return WorkflowData.GetWorkflowStatus(workflowName);
        }
    }
}
