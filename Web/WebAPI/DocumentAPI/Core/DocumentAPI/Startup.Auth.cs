using Extract;
using System;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.IdentityModel.Tokens;

using DocumentAPI.Models;
using DocumentAPI.Controllers;
using static DocumentAPI.Utils;

namespace DocumentAPI
{
    public partial class Startup
    {
        static string _secretKey;

        /// <summary>
        /// configure authorization
        /// </summary>
        /// <param name="app">application builder instance</param>
        private void ConfigureAuth(IApplicationBuilder app)
        {
            var signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(_secretKey));

            // This allows for token creation on authentication
            UsersController.SecretKey = _secretKey;

            var tokenValidationParameters = new TokenValidationParameters
            {
                // The signing key must match!
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = signingKey,

                // Validate the JWT Issuer (iss) claim
                ValidateIssuer = true,
                ValidIssuer = Issuer,

                // Validate the JWT Audience (aud) claim
                ValidateAudience = true,
                ValidAudience = Audience,

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

            // Removed for now, as it causes http return values of 400 instead of 401 when enabled, probably because
            // there is no handler for cookie-based auth.
            /*
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
            */
        }
    }
}
