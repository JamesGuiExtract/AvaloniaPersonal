using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Globalization;

namespace TokenGen
{
    /// <summary>
    /// User model
    /// </summary>
    public class User
    {
        /// <summary>
        /// user name
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// user password
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// user-specified workflow name that overrides the default workflow
        /// </summary>
        public string WorkflowName { get; set; }
    }

    public static class Utils
    {
        public static bool IsEquivalent(this string s1, string s2, bool ignoreCase = true)
        {
            if (String.Compare(s1, s2, ignoreCase) == 0)
            {
                return true;
            }

            return false;
        }
    }

    public class Program
    {
        static TimeSpan _expiration = TimeSpan.FromHours(12);

        static SymmetricSecurityKey _signingKey;
        static string _secretKey;

        static void DisplayHelp()
        {
            Console.WriteLine("All arguments are optional");
            Console.WriteLine("args:");
            Console.WriteLine("-u username");
            Console.WriteLine("-p password");
            Console.WriteLine("-w workflow");
            Console.WriteLine("-s signing key");
        }

        // args:
        // -u username
        // -p password
        // -w workflow
        // -s signing key
        static void Main(string[] args)
        {
            string username = "admin";
            string password = "a";
            string workflow = "CourtOffice";
            SecretKey = Environment.GetEnvironmentVariable("WebAPI_Private");

            // if this loop processing fails with an out-of-range index, user got the input wrong and needs to fix
            if (args.Length > 0)
            {
                var firstArg = args[0];
                if (firstArg.IsEquivalent("-h") || firstArg.IsEquivalent("-help") || firstArg.IsEquivalent("-?"))
                {
                    DisplayHelp();
                    return;
                }

                for (int i = 0; i < args.Length; ++i)
                {
                    var arg = args[i];
                    if (arg.IsEquivalent("-u"))
                    {
                        username = args[i + 1];
                        ++i;
                    }
                    else if (arg.IsEquivalent("-p"))
                    {
                        password = args[i + 1];
                        ++i;
                    }
                    else if (arg.IsEquivalent("-w"))
                    {
                        workflow = args[i + 1];
                        ++i;
                    }
                    else if (arg.IsEquivalent("-s"))
                    {
                        SecretKey = args[i + 1];
                        ++i;
                    }
                }
            }

            User user = new User
            {
                Username = username,
                Password = password,
                WorkflowName = workflow
            };

            var s = GenerateToken(user);

            Console.WriteLine("{0}", s);
        }

        static private string GenerateToken(User user)
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
                    new Claim("WorkflowName", user.WorkflowName)
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
                var items = response.Split(new char[] { ' ', ',' });
                string token = items[3].Trim();
                var tokenOnly = token.Trim('\"');

                return tokenOnly;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception reported: {0}", ex.Message);
                throw;
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
                Console.WriteLine("Exception reported: {0}", ex.Message);
                throw;
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

        /// <summary>
        /// the JWT Issuer (iss: )
        /// </summary>
        public static string Issuer
        {
            get
            {
                return "DocumentAPIv1";
            }
        }

        /// <summary>
        /// the JWT Audience (aud: )
        /// </summary>
        public static string Audience
        {
            get
            {
                return "ESWebClients";
            }
        }
    }
}