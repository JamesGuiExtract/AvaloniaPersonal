using Extract;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using WebAPI.Models;
using static WebAPI.Utils;

namespace WebAPI.Controllers
{
    /// <summary>
    /// Backed API support for web verification applications.
    /// </summary>
    [Route("api/[controller]")]
    [EnableCors("AllowAll")]
    [BindRequired]
    public class AppBackendController : Controller
    {
        /// <summary>
        /// Authenticates <see paramref="user"/>, and if successful returns a JWT.
        /// </summary>
        /// <param name="user">A User object (name, password, optional claim)</param>
        // POST api/Users/Login
        [HttpPost("Login")]
        public IActionResult Login([FromBody] User user)
        {
            try
            {
                if (user == null)
                {
                    return BadRequest("null Model.User");
                }
                if (String.IsNullOrWhiteSpace(user.Username))
                {
                    return BadRequest("Username is empty");
                }
                if (String.IsNullOrWhiteSpace(user.Password))
                {
                    return BadRequest("Password is empty");
                }

                // The user may have specified a workflow - if so then ensure that the API context uses
                // the specified workflow.
                var context = LoginContext(user.WorkflowName);
                using (var userData = new UserData(context))
                {
                    if (userData.MatchUser(user))
                    {
                        var token = AuthUtils.GenerateToken(user, context);

                        return Ok(token);
                    }
                }

                return BadRequest("Unknown user or password");
            }
            catch (Exception ex)
            {
                var ee = ex.AsExtract("ELI45091");
                Log.WriteLine(ee);

                return BadRequest(ee.Message);
            }
        }
    }
}
