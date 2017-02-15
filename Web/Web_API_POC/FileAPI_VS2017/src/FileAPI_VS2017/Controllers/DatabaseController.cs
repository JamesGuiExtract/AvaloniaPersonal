using Microsoft.AspNetCore.Mvc;

using System;


namespace FileAPI_VS2017.Controllers
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
            if (!ModelState.IsValid || String.IsNullOrEmpty(id))
            {
                return BadRequest("id cannot be empty");
            }

            Utils.DatabaseServer = id;
            Utils.ResetAttributeMgr();
            Utils.ResetFileProcessingDB();
            return Ok("id");
        }

        /// <summary>
        /// Set the database name, supported for testing only
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost("SetDatabaseName/{id}")]
        public IActionResult SetDatabaseName(string id)
        {
            if (!ModelState.IsValid || String.IsNullOrEmpty(id))
            {
                return BadRequest("id cannot be empty");
            }

            Utils.DatabaseName = id;
            Utils.ResetAttributeMgr();
            Utils.ResetFileProcessingDB();
            return Ok("id");
        }
    }
}
