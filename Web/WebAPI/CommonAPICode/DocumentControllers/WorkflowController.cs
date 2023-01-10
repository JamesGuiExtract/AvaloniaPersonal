using Extract.Web.ApiConfiguration.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using System;
using static WebAPI.Utils;


namespace WebAPI.Controllers
{
    /// <summary>
    /// workflow controller
    /// </summary>
    [Authorize]
    [ApiVersion("3.0")]
    [ApiVersion("3.1")]
    [Route("api/v3.0/[controller]")]
    [Route("api/v3.1/[controller]")]
    // All controller versions will be mapped to "api/[controller]"; 
    // Utils.CurrentApiContext.ApiVersion will determine which to us at this route.
    [Route("api/[controller]")]
    [EnableCors("AllowAll")]
    public class WorkflowController : Controller
    {
        private readonly IConfigurationDatabaseService _configurationDatabaseService;

        /// <summary>
        /// Constructor for workflow controller
        /// </summary>
        /// <param name="configurationDatabaseService"></param>
        public WorkflowController(IConfigurationDatabaseService configurationDatabaseService) : base() 
        {
            _configurationDatabaseService = configurationDatabaseService;
        }
        /// <summary>
        /// Gets the overall status of the workflow (# of files in each state)
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
                var result = WorkflowData.GetWorkflowStatus(ClaimsToContext(User, _configurationDatabaseService));
                return Ok(result);
            }
            catch (Exception ex)
            {
                return this.GetAsHttpError(ex, "ELI50083");
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
                var result = WorkflowData.GetDocumentStatuses(ClaimsToContext(User, _configurationDatabaseService));
                return Ok(result);
            }
            catch (Exception ex)
            {
                return this.GetAsHttpError(ex, "ELI50084");
            }
        }
    }
}
