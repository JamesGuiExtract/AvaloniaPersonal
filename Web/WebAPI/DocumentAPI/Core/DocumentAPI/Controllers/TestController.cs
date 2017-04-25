using static DocumentAPI.Utils;
using Microsoft.AspNetCore.Mvc;
using System;


namespace DocumentAPI.Controllers
{
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
                if (String.IsNullOrWhiteSpace(args.DatabaseServerName))
                {
                    var message = "database server name cannot be empty";
                    Log.WriteLine(message);
                    return BadRequest(message);
                }

                if (String.IsNullOrWhiteSpace(args.DatabaseName))
                {
                    var message = "database name cannot be empty";
                    Log.WriteLine(message);
                    return BadRequest(message);
                }

                if (String.IsNullOrWhiteSpace(args.WorkflowName) )
                {
                    var message = "workflow name cannot be empty";
                    Log.WriteLine(message);
                    return BadRequest(message);
                }

                Utils.SetCurrentApiContext(args.DatabaseServerName, args.DatabaseName, args.WorkflowName);
                return Ok(args);
            }
            catch (Exception ex)
            {
                var message = Inv($"Exception: {ex.Message}, setting API context");
                Log.WriteLine(message);
                return BadRequest(message);
            }
        }
    }
}
