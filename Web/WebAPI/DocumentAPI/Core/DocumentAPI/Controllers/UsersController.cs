using DocumentAPI.Models;
using static DocumentAPI.Utils;
using Extract;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;


namespace DocumentAPI.Controllers
{
    /// <summary>
    /// Users (controller) class
    /// </summary>
    [Route("api/[controller]")]
    [BindRequired]
    public class UsersController : Controller
    {
        static readonly TimeSpan _expiration = TimeSpan.FromHours(12);

        static SymmetricSecurityKey _signingKey;
        static string _secretKey;

        /// <summary>
        /// login
        /// </summary>
        /// <param name="user">A User object (name, password, optional claim)</param>
        // POST api/Users/Login
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

            var userData = new UserData(FileApiMgr.GetInterface(Utils.CurrentApiContext));
            if (userData.MatchUser(user))
            {
                var token = GenerateToken(user);
                return Ok(token);
            }

            return BadRequest("Unknown user or password");
        }

        /// <summary>
        /// This method creates a JWT
        /// </summary>
        /// <param name="user">the User DTO instance</param>
        /// <returns>JSON-encoded JWT</returns>
        private string GenerateToken(User user)
        {
            try
            {
                String username = user.Username;
                String password = user.Password;
                String workflowName = user.WorkflowName;

                var now = DateTime.UtcNow;

                var apiContext = CurrentApiContext;

                // Preserve user-specified workflow if it exists.
                var namedWorkflow = !String.IsNullOrEmpty(workflowName) ? workflowName : apiContext.WorkflowName;

                // Specifically add the jti (nonce), iat (issued timestamp), and sub (subject/user) claims.
                // You can add other claims here, if you want:
                var claims = new Claim[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, username),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Iat, ToUnixEpochDate(now).ToString(), ClaimValueTypes.Integer64),

                    // Add custom claims. The database info is from the current context, while the workflow name may be 
                    // from the user login request.
                    new Claim("DatabaseServerName", apiContext.DatabaseServerName),
                    new Claim("DatabaseName", apiContext.DatabaseName),
                    new Claim("WorkflowName", namedWorkflow)
                };

                // Create the JWT and write it to a string
                var jwt = new JwtSecurityToken(
                    issuer: Issuer,
                    audience: Audience,
                    claims: claims,
                    notBefore: now,
                    expires: now.Add(_expiration),
                    signingCredentials: new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256));

                var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

                JsonSerializerSettings serializerSettings = new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented
                };

                var responseToken = new
                {
                    access_token = encodedJwt,
                    expires_in = (int)_expiration.TotalSeconds
                };

                var response = JsonConvert.SerializeObject(responseToken, serializerSettings);
                return response;
            }
            catch (Exception ex)
            {
                var ee = ex.AsExtract("ELI43194");
                Log.WriteLine(ee);

                throw ee;
            }
        }


        /// <summary>
        /// Get this datetime as a Unix epoch timestamp (seconds since Jan 1, 1970, midnight UTC).
        /// </summary>
        /// <param name="date">The date to convert.</param>
        /// <returns>Seconds since Unix epoch.</returns>
        static long ToUnixEpochDate(DateTime date)
        {
            try
            {
                return new DateTimeOffset(date).ToUniversalTime().ToUnixTimeSeconds();
            }
            catch (Exception ex)
            {
                var ee = ex.AsExtract("ELI43196");
                Log.WriteLine(ee);

                throw ee;
            }
        }

        /// <summary>
        /// a property to set the secret key value
        /// </summary>
        public static string SecretKey
        {
            set
            {
                _secretKey = value;
                _signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_secretKey));
            }
        }
    }
}
