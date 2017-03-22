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
        public string Name { get; set; }
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
        /// <param name="arg"></param>
        /// <returns></returns>
        [HttpPost("SetAttributeSetName")]
        public IActionResult SetAttributeSetName([FromBody]TestArgs arg)
        {
            try
            {
                if (!ModelState.IsValid || String.IsNullOrEmpty(arg.Name))
                {
                    var message = "name cannot be empty";
                    Log.WriteLine(message);
                    return BadRequest(message);
                }

                Utils.AttributeSetName = arg.Name;
                return Ok(arg);
            }
            catch (Exception ex)
            {
                var message = Inv($"Exception: {ex.Message}, setting attribute set name to: {arg.Name}");
                Log.WriteLine(message);
                return BadRequest(message);
            }
        }
    }
}
