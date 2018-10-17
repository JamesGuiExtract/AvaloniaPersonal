using Extract;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WebAPI.Models;
using static WebAPI.Utils;

namespace WebAPI
{
    /// <summary>
    /// Utility methods for generating authentication tokens.
    /// </summary>
    static public class AuthUtils
    {
        static TimeSpan _expiration = TimeSpan.FromHours(12);

        static SymmetricSecurityKey _signingKey;
        static string _secretKey;

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
        /// The Token timeout interval used in JWTs
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
                HTTPError.Assert("ELI46374", value > 0 && ts < TimeSpan.FromHours(24),
                    "Token timeout out of range", ("seconds", value, true));
                _expiration = ts;
            }
        }

        /// <summary>
        /// For internal use, just makes this more readable
        /// </summary>
        public static TimeSpan TokenTimeout
        {
            get
            {
                return _expiration;
            }
        }

        /// <summary>
        /// This method creates a <see cref="ClaimsPrincipal"/> representing the authenticated
        /// user.
        /// </summary>
        /// <param name="user">the User DTO instance</param>
        /// <param name="context">the user's context</param>
        /// <returns>JSON-encoded JWT.</returns>
        internal static LoginToken GenerateToken(User user, ApiContext context)
        {
            try
            {
                var now = DateTime.UtcNow;

                // Specifically add the jti (nonce), iat (issued timestamp), and sub (subject/user) claims.
                var claims = new Claim[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, user.Username),
                    new Claim(JwtRegisteredClaimNames.Jti, context.SessionId),
                    new Claim(JwtRegisteredClaimNames.Iat, ToUnixEpochDate(now).ToString(), ClaimValueTypes.Integer64),

                    // Add custom claims. The workflow name may be from the user login request.
                    new Claim(_WORKFLOW_NAME, context.WorkflowName),
                    new Claim("FAMSessionId", context.FAMSessionId.ToString(CultureInfo.InvariantCulture))
                };

                var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(claims));

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

                var responseToken = new LoginToken()
                {
                    access_token = encodedJwt,
                    expires_in = AuthUtils.TokenTimeoutInSeconds
                };

                return responseToken;
            }
            catch (Exception ex)
            {
                var ee = ex.AsExtract("ELI43194");
                Log.WriteLine(ee);

                throw ee;
            }
        }

        /// <summary>
        /// Gets this datetime as a Unix epoch timestamp (seconds since Jan 1, 1970, midnight UTC).
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
    }
}
