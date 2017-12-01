using Extract;
using System;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.IdentityModel.Tokens;
using WebAPI.Models;
using static WebAPI.Utils;

namespace WebAPI
{
    public partial class Startup
    {
        /// <summary>
        /// Gets or sets the authentication options.
        /// </summary>
        /// <value>
        /// The authentication options.
        /// </value>
        public JwtBearerOptions AuthenticationOptions
        {
            get;
            set;
        }

        /// <summary>
        /// configure authorization
        /// </summary>
        /// <param name="app">application builder instance</param>
        private void ConfigureAuth(IApplicationBuilder app)
        {
            FileApi fileApi = new FileApi(Utils.CurrentApiContext);
            var secretKey = fileApi.FileProcessingDB.DatabaseID;
            var signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secretKey));

            // This is the encryption key used to generate authentication tokens.
            AuthUtils.SecretKey = secretKey;

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

            AuthenticationOptions =
                new JwtBearerOptions
                {
                    AutomaticAuthenticate = true,
                    AutomaticChallenge = true,
                    TokenValidationParameters = tokenValidationParameters
                };

            app.UseJwtBearerAuthentication(AuthenticationOptions);

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
