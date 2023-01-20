using Extract.Web.ApiConfiguration.Models;
using Extract.Web.ApiConfiguration.Services;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.Linq;
using static WebAPI.Utils;

namespace WebAPI.Controllers.v2_0
{
    /// <summary>
    /// Users (controller) class
    /// </summary>
    [ApiVersion("2.0")]
    [Route("api/v2.0/[controller]")]
    // All controller versions will be mapped to "api/[controller]"; 
    // Utils.CurrentApiContext.ApiVersion will determine which to us at this route.
    [Route("api/[controller]")]
    [EnableCors("AllowAll")]
    [BindRequired]
    public class UsersController : Controller
    {
        private readonly IConfigurationDatabaseService _configurationDatabaseService;
        private readonly IDocumentApiWebConfiguration _defaultConfiguration;

        /// <summary>
        /// Create controller with dependencies
        /// </summary>
        public UsersController(IConfigurationDatabaseService configurationDatabaseService,
            IDocumentApiWebConfiguration defaultConfiguration) : base()
        {
            _configurationDatabaseService = configurationDatabaseService;
            _defaultConfiguration = defaultConfiguration;
        }

        /// <summary>
        /// Authenticates a user and, if successful, returns a JSON Web Token. Prefix the returned
        /// access_token with "Bearer " to use for authorization in all other API methods.
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

                var context = LoginContext();
                using var userData = new UserData(context);
                userData.LoginUser(user);

                // The user may have specified a workflow or configuration - if so then ensure that the API context uses them.
                if (!string.IsNullOrEmpty(user.WorkflowName) || !string.IsNullOrEmpty(user.ConfigurationName))
                {
                    context.WebConfiguration = LoadConfigurationBasedOnSettings(
                        workflowName: user.WorkflowName,
                        configurationName: user.ConfigurationName,
                        webConfigurations: _configurationDatabaseService.DocumentAPIWebConfigurations);
                }
                else
                {
                    // Use the default if the user did not specify a workflow/configuration.
                    context.WebConfiguration = _defaultConfiguration;
                }

                var token = AuthUtils.GenerateToken(user, context);

                return Ok(token);
            }
            catch (Exception ex)
            {
                return this.GetAsHttpError(ex, "ELI45212");
            }
        }
    }
}
