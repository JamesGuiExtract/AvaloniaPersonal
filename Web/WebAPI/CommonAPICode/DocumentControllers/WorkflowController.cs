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
        /// Gets the overall status of a workflow (# of files in each state)
        /// </summary>
        /// <returns>a workflow status object</returns>
        [HttpGet("Status")]
        [ProducesResponseType(200, Type = typeof(WorkflowStatusResult))]
        [ProducesResponseType(400, Type = typeof(ErrorResult))]
        [ProducesResponseType(401)]
        public IActionResult GetWorkflowStatus()
        {
            try
            {
                var result = WorkflowData.GetWorkflowStatus(ClaimsToContext(User));
                return Ok(result);
            }
            catch (Exception ex)
            {
                return this.GetAsHttpError(ex, "ELI46397");
            }
        }

        /// <summary>
        /// Gets the status of all files in the workflow
        /// </summary>
        /// <returns>a workflow status object</returns>
        [HttpGet("DocumentStatuses")]
        [ProducesResponseType(200, Type = typeof(FileStatusResult))]
        [ProducesResponseType(400, Type = typeof(ErrorResult))]
        [ProducesResponseType(401)]
        public IActionResult GetDocumentStatuses()
        {
            try
            {
                var result = WorkflowData.GetDocumentStatuses(ClaimsToContext(User));
                return Ok(result);
            }
            catch (Exception ex)
            {
                return this.GetAsHttpError(ex, "ELI46413");
            }
        }
    }
}
