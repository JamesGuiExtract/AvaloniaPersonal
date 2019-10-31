using System.Web.Mvc;

namespace WebApplication1.Controllers
{
    public class AuthorizationController : Controller
    {
        /// <summary>
        /// Encrypts the username, and passes it back to the front end.
        /// </summary>
        /// <returns>Returns an encrypted username</returns>
        [Authorize]
        public string GetWindowsAuthorizationToken()
        {
            return Encryption.AESThenHMAC.SimpleEncryptWithPassword(this.User.Identity.Name);
        }
    }
}
