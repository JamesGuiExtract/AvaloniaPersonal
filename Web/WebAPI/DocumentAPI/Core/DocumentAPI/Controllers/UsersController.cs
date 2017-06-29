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
        static TimeSpan _expiration = TimeSpan.FromHours(12);

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
                        var token = GenerateToken(user, context);
                        return Ok(token);
                    }
                }

                return BadRequest("Unknown user or password");
            }
            catch (Exception ex)
            {
                var ee = ex.AsExtract("ELI42178");
                Log.WriteLine(ee);

                return BadRequest(ee.Message);
            }
        }

        /// <summary>
        /// This method creates a JWT
        /// </summary>
        /// <param name="user">the User DTO instance</param>
        /// <param name="context">the user's context</param>
        /// <returns>JSON-encoded JWT</returns>
        private string GenerateToken(User user, ApiContext context)
        {
            try
            {
                var now = DateTime.UtcNow;

                // Specifically add the jti (nonce), iat (issued timestamp), and sub (subject/user) claims.
                var claims = new Claim[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, user.Username),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Iat, ToUnixEpochDate(now).ToString(), ClaimValueTypes.Integer64),

                    // Add custom claims. The workflow name may be from the user login request.
                    new Claim("WorkflowName", context.WorkflowName)
                };

                // Create the JWT and write it to a string
                var jwt = new JwtSecurityToken(
                    issuer: Issuer,
                    audience: Audience,
                    claims: claims,
                    notBefore: now,
                    expires: now.Add(TokenTimeout),
                    signingCredentials: new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256));

                var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

                JsonSerializerSettings serializerSettings = new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented
                };

                var responseToken = new
                {
                    access_token = encodedJwt,
                    expires_in = TokenTimeoutInSeconds
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

        /// <summary>
        /// gets/sets the Token timeout interval used in JWTs
        /// </summary>
        public static int TokenTimeoutInSeconds
        {
            get
            {
                return Convert.ToInt32(_expiration.TotalSeconds);
            }
            set
            {
                var ts = TimeSpan.FromSeconds(Convert.ToDouble(value));
                Contract.Assert(value > 0 && ts < TimeSpan.FromHours(24), "Invalid value for token timeout seconds: {0}", value);
                _expiration = ts;
            }
        }

        /// <summary>
        /// For internal use, just makes this more readable
        /// </summary>
        static TimeSpan TokenTimeout
        {
            get
            {
                return _expiration;
            }
        }
    }
}
