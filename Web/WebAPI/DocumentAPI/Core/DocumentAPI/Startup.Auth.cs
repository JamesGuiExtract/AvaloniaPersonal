using Extract;
using System;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.IdentityModel.Tokens;

using SimpleTokenProvider;
using DocumentAPI.Models;
using static DocumentAPI.Utils;

namespace DocumentAPI
{
    public partial class Startup
    {
        static string _secretKey;

        static readonly string _issuer = "DocumentAPIv1";
        static readonly string _audience = "ESWebClients";

        /// <summary>
        /// configure authorization
        /// </summary>
        /// <param name="app">application builder instance</param>
        private void ConfigureAuth(IApplicationBuilder app)
        {
            var signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_secretKey));

            // -- This call registers the TokenProviderMiddleware class.
            app.UseSimpleTokenProvider(new TokenProviderOptions
            {
                Path = "/api/login",    // Note that the Path is also set in TokenProviderOptions - this overrides that setting.
                Audience = _audience,
                Issuer = _issuer,
                SigningCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256),
                IdentityResolver = GetIdentity,
                ApiContext = GetCurrentApiContext,
                LogWriteLine = Log.WriteOneLine,
                LogWriteLineEE = Log.WriteOneLineEE
            });

            var tokenValidationParameters = new TokenValidationParameters
            {
                // The signing key must match!
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = signingKey,

                // Validate the JWT Issuer (iss) claim
                ValidateIssuer = true,
                ValidIssuer = _issuer,

                // Validate the JWT Audience (aud) claim
                ValidateAudience = true,
                ValidAudience = _audience,

                // Validate the token expiry
                ValidateLifetime = true,
                
                // amount of clock drift
                ClockSkew = TimeSpan.Zero
            };

            app.UseJwtBearerAuthentication(new JwtBearerOptions
            {
                AutomaticAuthenticate = true,
                AutomaticChallenge = true,
                TokenValidationParameters = tokenValidationParameters
            });

            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AutomaticAuthenticate = true,
                AutomaticChallenge = true,
                AuthenticationScheme = "Cookie",
                CookieName = "access_token",
                TicketDataFormat = new CustomJwtDataFormat(
                    SecurityAlgorithms.HmacSha256,
                    tokenValidationParameters)
            });
        }

        /// <summary>
        /// This method drives the login process
        /// </summary>
        /// <param name="username">username - required</param>
        /// <param name="password">password - required</param>
        /// <param name="workflowName">optional workflow name</param>
        /// <returns>JWT with claims</returns>
        private Task<ClaimsIdentity> GetIdentity(string username, string password, string workflowName)
        {
            try
            {
                var userData = new UserData(FileApiMgr.GetInterface(Utils.CurrentApiContext));
                var user = new User()
                {
                    Username = username,
                    Password = password,
                    WorkflowName = workflowName
                };  

                if (userData.MatchUser(user))
                {
                    return Task.FromResult(new ClaimsIdentity(new GenericIdentity(username, "Token"), new Claim[] { }));
                }

                // Credentials are invalid, or account doesn't exist
                return Task.FromResult<ClaimsIdentity>(null);
            }
            catch (Exception ex)
            {
                var ee = ex.AsExtract("ELI43183");
                ee.AddDebugData("Username", username, encrypt: false);
                Log.WriteLine(ee);

                throw ee;
            }
        }
    }
}
