using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;

namespace AuthorizationAPI.Controllers
{
    [Authorize]
    [Route("[controller]")]
    [ApiController]
    public class WindowsAuthorizationController : ControllerBase
    {
        // GET URL + WindowsAuthorization
        [HttpGet]
        public string Get()
        {
            return Encryption.AESThenHMAC.SimpleEncryptWithPassword(this.User.Identity.Name + "|" + DateTime.Now.AddMinutes(1));
        }
    }
}
