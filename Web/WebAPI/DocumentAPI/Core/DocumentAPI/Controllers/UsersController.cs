using DocumentAPI.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;

namespace DocumentAPI.Controllers
{
    /// <summary>
    /// Users (controller) class
    /// </summary>
    [Route("api/[controller]")]
    [BindRequired]
    public class UsersController : Controller
    {
        /// <summary>
        /// login
        /// </summary>
        /// <param name="user">A User object (name, password, optional claim)</param>
        // POST api/User/Login
        [HttpPost("Login")]
        public IActionResult Login([FromBody] User user)
        {
            if (user == null)
            {
                return BadRequest("null Model.User");
            }
            if (String.IsNullOrEmpty(user.Username))
            {
                return BadRequest("Username is empty");
            }
            if (String.IsNullOrEmpty(user.Password))
            {
                return BadRequest("Password is empty");
            }

            if (UserData.MatchUser(user))
            {
                return Ok("Logged in");
            }

            return BadRequest("Unknown user or password");
        }
    }
}
