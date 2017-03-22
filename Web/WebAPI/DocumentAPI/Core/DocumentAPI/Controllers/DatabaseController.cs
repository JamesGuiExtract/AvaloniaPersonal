using static DocumentAPI.Utils;
using Microsoft.AspNetCore.Mvc;
using System;


namespace DocumentAPI.Controllers
{
    /// <summary>
    /// Database controller, for testing only
    /// </summary>
    [Route("api/[controller]")]
    public class DatabaseController : Controller
    {
        /// <summary>
        /// Set the database server, supported for testing only
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost("SetDatabaseServer/{id}")]
        public IActionResult SetDatabaseServer(string id)
        {
            try
            {
                if (!ModelState.IsValid || String.IsNullOrEmpty(id))
                {
                    var message = "id cannot be empty";
                    Log.WriteLine(message);
                    return BadRequest(message);
                }

                Utils.DatabaseServer = id;
                Utils.ResetAttributeMgr();
                Utils.ResetFileProcessingDB();
                return Ok("id");
            }
            catch (Exception ex)
            {
                var message = Inv($"Exception: {ex.Message}, while getting ASFF for fileId: {id}, resetting attribute manager");
                Log.WriteLine(message);
                return BadRequest(message);
            }
        }

        /// <summary>
        /// Set the database name, supported for testing only
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost("SetDatabaseName/{id}")]
        public IActionResult SetDatabaseName(string id)
        {
            try
            {
                if (!ModelState.IsValid || String.IsNullOrEmpty(id))
                {
                    var message = "id cannot be empty";
                    Log.WriteLine(message);
                    return BadRequest(message);
                }

                Utils.DatabaseName = id;
                Utils.ResetAttributeMgr();
                Utils.ResetFileProcessingDB();
                return Ok("id");
            }
            catch (Exception ex)
            {
                var message = Inv($"Exception: {ex.Message}, while getting ASFF for fileId: {id}, resetting attribute manager");
                Log.WriteLine(message);
                return BadRequest(message);
            }
        }
    }
}
