using DocumentAPI.Models;
using Extract;
using Microsoft.AspNetCore.Mvc;
using System;
using static DocumentAPI.Utils;

namespace DocumentAPI.Controllers
{
    /// <summary>
    /// workflow controller
    /// </summary>
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
