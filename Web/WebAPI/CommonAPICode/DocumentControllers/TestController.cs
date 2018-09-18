using Microsoft.AspNetCore.Mvc;
using System;
using static WebAPI.Utils;

namespace WebAPI.Controllers
{
#if Add_Test_Controller
    /// <summary>
    /// necessary evil because model binding won't bind to primitive types in FromBody...
    /// </summary>
    public class TestArgs
    {
        /// <summary>
        /// Name argument
        /// </summary>
        public string DatabaseServerName { get; set; }

        /// <summary>
        /// database name
        /// </summary>
        public string DatabaseName { get; set; }

        /// <summary>
        /// workflow name
        /// </summary>
        public string WorkflowName { get; set; }
    }
    /// <summary>
    /// Test controller - for testing only, and probably temporary as well
    /// </summary>
    [Route("api/[controller]")]
    public class TestController: Controller
    {
        /// <summary>
        /// Testing only - set the AttributeSetName, this allows testing Document.GetResultSet().
        /// </summary>
        /// <param name="args">TestArgs instance, all elements must be non-empty</param>
        /// <returns>IActionResult containing the input args</returns>
        [HttpPost("SetApiContext")]
        public IActionResult SetApiContext([FromBody]TestArgs args)
        {
            try
            {
                RequestAssertion.AssertSpecified("ELI45207", args, "null args");
                RequestAssertion.AssertSpecified("ELI45208", args.DatabaseServerName, "database server name cannot be empty");
                RequestAssertion.AssertSpecified("ELI45209", args.DatabaseName, "database name cannot be empty");
                RequestAssertion.AssertSpecified("ELI45210", args.WorkflowName, "workflow name cannot be empty");

                Utils.SetCurrentApiContext(args.DatabaseServerName, args.DatabaseName, args.WorkflowName);
                Utils.ValidateCurrentApiContext();

                return Ok(args);
            }
            catch (Exception ex)
            {
                return this.GetAsHttpError(ex, "ELI45211");
            }
        }
    }
#endif
}
