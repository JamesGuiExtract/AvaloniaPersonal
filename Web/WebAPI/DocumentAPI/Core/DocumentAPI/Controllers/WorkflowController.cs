using DocumentAPI.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using static DocumentAPI.Utils;

namespace DocumentAPI.Controllers
{
    /// <summary>
    /// workflow controller
    /// </summary>
    [Route("api/[controller]")]
    public class WorkflowController : Controller
    {

        static private ConcurrentDictionary<string, Models.Workflow> _worflowList = new ConcurrentDictionary<string, Workflow>();

        /// <summary>
        /// GET handler - returns a list of workflow names
        /// </summary>
        /// <returns>list of workflow names</returns>
        // GET: api/workflow/Workflows
        [HttpGet("GetWorkflows")]
        public IEnumerable<string> Get()
        {
            return _worflowList.Select(kvp => kvp.Value.Name);
        }


        /// <summary>
        /// get default workflow for the specified user
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        // NOTE: [FromBody] doesn't work here, regardless of argument type. I think that
        // model binding fails because the http verb is a GET. [FromQuery] works fine,
        // as expected; also [FromBody] works with a POST, as expected.
        // For now I'm setting this up as a GET using [FromQuery]...
        //[Route("api/[controller]/GetDefaultWorkflow")]
        //[HttpPost("/GetDefaultWorkflow")]
        //public IActionResult GetDefaultWorkflow([FromQuery] User user)
        [HttpGet("GetDefaultWorkflow/{username}")]
        public string GetDefaultWorkflow(string username)
        {
            if (!ModelState.IsValid || String.IsNullOrEmpty(username))
            {
                return "Error: empty user name";
            }

            return "Extract_Data";
        }

        /// <summary>
        /// get status of specified workflow
        /// </summary>
        /// <param name="workflowName"></param>
        /// <returns></returns>
        [HttpGet("GetWorkflowStatus/{workflowName}")]
        public WorkflowStatus GetWorkflowStatus(string workflowName)
        {
            if (!ModelState.IsValid || String.IsNullOrEmpty(workflowName))
            {
                //return BadRequest("Error: empty user name");
            }

            var status = new WorkflowStatus
            {
                Error = new ErrorInfo(),
                NumberDone = 5,
                NumberFailed = 1,
                NumberIgnored = 0,
                NumberProcessing = 14,
                State = Enum.GetName(typeof(WorkflowState), WorkflowState.Running)
            };

            return status;
        }

        /// <summary>
        /// management only - Post handler for workflow, creates a new workflow
        /// </summary>
        /// <param name="workflow"></param>
        /// <returns></returns>
        // POST api/Workflow
        [HttpPost]
        public IActionResult Post([FromBody] Workflow workflow)
        {
            if (!ModelState.IsValid || 
                workflow == null ||
                String.IsNullOrEmpty(workflow.Name))
            {
                return BadRequest();
            }

            int identifier = workflow?.Id ?? _worflowList.Count();
            var idAlreadyExists = _worflowList.Where(kvp => kvp.Value.Id == identifier).Count() > 0;
            if (idAlreadyExists)
            {
                return BadRequest(Inv($"Error: the workflow id: {identifier}, already exists"));
            }

            bool nameExists = _worflowList.Where(kvp => kvp.Value.Name == workflow.Name).Count() > 0;
            if (nameExists)
            {
                return BadRequest(Inv($"Error: the workflow Name: {workflow.Name}, already exists"));
            }

            _worflowList[workflow.Name] = new Workflow
            {
                Name = workflow.Name,
                Description = workflow.Description,
                EntryAction = workflow.EntryAction,
                ExitAction = workflow.ExitAction,
                RunAfterResultsAction = workflow.RunAfterResultsAction,
                DocumentFolder = workflow.DocumentFolder,
                AttributeSetName = workflow.AttributeSetName,
                Id = identifier,
            };

            return new ObjectResult(workflow);
        }
    }
}
