using Extract;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Security.Claims;
using WebAPI.Models;
using static WebAPI.Utils;

namespace WebAPI.Controllers
{
    /// <summary>
    /// Users (controller) class
    /// </summary>
    [Route("api/[controller]")]
    [EnableCors("AllowAll")]
    [BindRequired]
    public class UsersController : Controller
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
                RequestAssertion.AssertSpecified("ELI45185", user.Username, "Username is empty");
                RequestAssertion.AssertSpecified("ELI45186", user.Password, "Password is empty");

                // The user may have specified a workflow - if so then ensure that the API context uses
                // the specified workflow.
                var context = LoginContext(user.WorkflowName);
                using (var userData = new UserData(context))
                {
                    userData.LoginUser(user);
                    var token = AuthUtils.GenerateToken(user, context);

                    return Ok(token);
                }
            }
            catch (Exception ex)
            {
                return this.GetAsHttpError(ex, "ELI45212");
            }
        }
    }
}
