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
    /// Users (controller) class
    /// </summary>
    [Route("api/[controller]")]
    [EnableCors("AllowAll")]
    [BindRequired]
    public class UsersController : Controller
    {
        /// <summary>
        /// Authenticates a user and, if successful, returns a JSON Web Token.
        /// </summary>
        /// <param name="user">Login credentials. WorkflowName is optional;
        /// specify only if a special workflow is required.</param>
        [HttpPost("Login")]
        [ProducesResponseType(200, Type = typeof(LoginToken))]
        [ProducesResponseType(400, Type = typeof(ErrorResult))]
        [ProducesResponseType(401)]
        public IActionResult Login([FromBody] User user)
        {
            try
            {
                HTTPError.AssertRequest("ELI45185", !string.IsNullOrEmpty(user.Username), "Username is empty");
                HTTPError.AssertRequest("ELI45186", !string.IsNullOrEmpty(user.Password), "Password is empty");

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
