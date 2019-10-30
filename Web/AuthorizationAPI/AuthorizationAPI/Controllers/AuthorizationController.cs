using System.Web.Mvc;

namespace WebApplication1.Controllers
{
    public class AuthorizationController : Controller
    {
        [Authorize]
        public string GetWindowsAuthorizationToken()
        {
            return Encryption.AESThenHMAC.SimpleEncryptWithPassword(this.User.Identity.Name);
        }
    }
}
