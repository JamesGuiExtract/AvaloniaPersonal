using Extract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System;
using WebAPI.Models;
using static WebAPI.Utils;


namespace WebAPI.Controllers
{
    /// <summary>
    /// workflow controller
    /// </summary>
    [Authorize]
    [Route("api/[controller]")]
    [EnableCors("AllowAll")]
    public class WorkflowController : Controller
    {
        /// <summary>
        /// get status of specified workflow
        /// </summary>
        /// <returns>a workflow status object</returns>
        [HttpGet("GetWorkflowStatus")]
        [Produces(typeof(WorkflowStatus))]
        public IActionResult GetWorkflowStatus()
        {
            try
            {
                var result = WorkflowData.GetWorkflowStatus(ClaimsToContext(User));
                return result.Error.ErrorOccurred ? (IActionResult)BadRequest(result) : Ok(result);
            }
            catch (Exception ex)
            {
                var ee = ex.AsExtract("ELI43660");
                Log.WriteLine(ee);
                return BadRequest(MakeWorkflowStatusError(ee.Message));
            }
        }
    }
}
