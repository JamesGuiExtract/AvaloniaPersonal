using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Text;
using WebAPI.Models;
using static WebAPI.Utils;

namespace WebAPI
{
    /// <summary>
    /// Defines authentication and authorization for both the API and the swagger documentation site.
    /// </summary>
    public static class SecurityConfiguration
    {
        /// <summary>
        /// Configure authentication and authorization for the specified service.
        /// </summary>
        public static void AddSecurity(this IServiceCollection services)
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

            services.AddAuthentication()
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = tokenValidationParameters;
                });

            services.AddAuthorization(options =>
            {
                options.DefaultPolicy = new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme)
                    .RequireAuthenticatedUser()
                    .Build();
            });
        }

        /// <summary>
        /// Configures Swagger documentation to provide authorization to the API
        /// </summary>
        public static void EnableAuthorization(this SwaggerGenOptions swaggerOptions)
        {
            swaggerOptions.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Specify as: \"Bearer {token}\"",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            swaggerOptions.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] {}
                    }
                });
        }
    }
}
