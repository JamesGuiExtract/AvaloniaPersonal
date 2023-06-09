﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Principal;
using WebAPI;

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
            return ActiveDirectoryUtilities.GetEncryptedJsonUserAndGroups((WindowsIdentity)this.User.Identity);
        }
    }
}
