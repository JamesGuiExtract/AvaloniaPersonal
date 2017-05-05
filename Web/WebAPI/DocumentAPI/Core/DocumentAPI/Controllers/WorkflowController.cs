using DocumentAPI.Models;
using Extract;
using static DocumentAPI.Utils;
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
        /// <returns>a workflow status object</returns>
        [HttpGet("GetWorkflowStatus/{workflowName}")]
        [Produces(typeof(WorkflowStatus))]
        public IActionResult GetWorkflowStatus(string workflowName)
        {
            try
            {
                var result = WorkflowData.GetWorkflowStatus(workflowName, ClaimsToContext(User));
                return result.Error.ErrorOccurred ? (IActionResult)BadRequest(result) : Ok(result);
            }
            catch (ExtractException ee)
            {
                Log.WriteLine(ee);
                return BadRequest(MakeWorkflowStatusError(ee.Message));
            }
        }
    }
}
