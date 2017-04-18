// Copyright (c) Nate Barbettini. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.
using Extract;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace SimpleTokenProvider
{
    /// <summary>
    /// Token generator middleware component which is added to an HTTP pipeline.
    /// This class is not created by application code directly,
    /// instead it is added by calling the <see cref="TokenProviderAppBuilderExtensions.UseSimpleTokenProvider(Microsoft.AspNetCore.Builder.IApplicationBuilder, TokenProviderOptions)"/>
    /// extension method.
    /// </summary>
    public class TokenProviderMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly TokenProviderOptions _options;
        private readonly JsonSerializerSettings _serializerSettings;

        static Action<ExtractException> _logWriteLineEE = null;

        /// <summary>
        /// CTOR
        /// </summary>
        /// <param name="next">next delegate to pass request too if not handled here</param>
        /// <param name="options">token provider options instance</param>
        public TokenProviderMiddleware(
            RequestDelegate next,
            IOptions<TokenProviderOptions> options)
        {
            try
            {
                _next = next;

                _options = options.Value;
                ThrowIfInvalidOptions(_options);

                _logWriteLineEE = _options.LogWriteLineEE;

                _serializerSettings = new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented
                };
            }
            catch (Exception ex)
            {
                var ee = ex.AsExtract("ELI43192");
                _options.LogWriteLineEE(ee);

                throw ee;
            }
        }

        /// <summary>
        /// Invoke handles the incoming http login request
        /// </summary>
        /// <param name="context">http request context</param>
        /// <returns>void</returns>
        public Task Invoke(HttpContext context)
        {
            try
            {
                // If the request path doesn't match, skip
                if (!context.Request.Path.Equals(_options.Path, StringComparison.Ordinal))
                {
                    return _next(context);
                }

                // Request must be POST with Content-Type: application/x-www-form-urlencoded
                if (!context.Request.Method.Equals("POST")
                   || !context.Request.HasFormContentType)
                {
                    context.Response.StatusCode = 400;
                    return context.Response.WriteAsync("Bad request.");
                }

                String username = context.Request.Form["username"];
                String workflowName = context.Request.Form["workflowname"];
                String message = String.Format("Login request - user: {0}, workflow name: {1}",
                                               username,
                                               workflowName);
                _options.LogWriteLine(message);

                return GenerateToken(context);
            }
            catch (Exception ex)
            {
                var ee = ex.AsExtract("ELI43193");
                _options.LogWriteLineEE(ee);

                throw ee;
            }
        }

        /// <summary>
        /// This method creates a JWT
        /// </summary>
        /// <param name="context">the request http context</param>
        /// <returns>void</returns>
        private async Task GenerateToken(HttpContext context)
        {
            try
            {
                String username = context.Request.Form["username"];
                String password = context.Request.Form["password"];
                String workflowName = context.Request.Form["workflowname"];

                var identity = await _options.IdentityResolver(username, password, workflowName);
                if (identity == null)
                {
                    context.Response.StatusCode = 400;
                    await context.Response.WriteAsync("Invalid username or password.");
                    return;
                }

                var now = DateTime.UtcNow;

                var apiContext = _options.ApiContext();

                // Preserve user-specified workflow if it exists.
                var namedWorkflow = !String.IsNullOrEmpty(workflowName) == true ? workflowName : apiContext.Item3;

                // Specifically add the jti (nonce), iat (issued timestamp), and sub (subject/user) claims.
                // You can add other claims here, if you want:
                var claims = new Claim[]
                {
                new Claim(JwtRegisteredClaimNames.Sub, username),
                new Claim(JwtRegisteredClaimNames.Jti, await _options.NonceGenerator()),
                new Claim(JwtRegisteredClaimNames.Iat, ToUnixEpochDate(now).ToString(), ClaimValueTypes.Integer64),

                // Add custom claims. The database info is from the current context, while the workflow name may be 
                // from the user login request.
                new Claim("DatabaseServerName", apiContext.Item1),
                new Claim("DatabaseName", apiContext.Item2),
                new Claim("WorkflowName", namedWorkflow)
                };

                // Create the JWT and write it to a string
                var jwt = new JwtSecurityToken(
                    issuer: _options.Issuer,
                    audience: _options.Audience,
                    claims: claims,
                    notBefore: now,
                    expires: now.Add(_options.Expiration),
                    signingCredentials: _options.SigningCredentials);
                var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

                var response = new
                {
                    access_token = encodedJwt,
                    expires_in = (int)_options.Expiration.TotalSeconds
                };

                // Serialize and return the response
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(JsonConvert.SerializeObject(response, _serializerSettings));
            }
            catch (Exception ex)
            {
                var ee = ex.AsExtract("ELI43194");
                _options.LogWriteLineEE(ee);

                throw ee;
            }
        }

        /// <summary>
        /// options state checker
        /// </summary>
        /// <param name="options">options instance to test for validity</param>
        private static void ThrowIfInvalidOptions(TokenProviderOptions options)
        {
            try
            {
                if (string.IsNullOrEmpty(options.Path))
                {
                    throw new ArgumentNullException(nameof(TokenProviderOptions.Path));
                }

                if (string.IsNullOrEmpty(options.Issuer))
                {
                    throw new ArgumentNullException(nameof(TokenProviderOptions.Issuer));
                }

                if (string.IsNullOrEmpty(options.Audience))
                {
                    throw new ArgumentNullException(nameof(TokenProviderOptions.Audience));
                }

                if (options.Expiration == TimeSpan.Zero)
                {
                    throw new ArgumentException("Must be a non-zero TimeSpan.", nameof(TokenProviderOptions.Expiration));
                }

                if (options.IdentityResolver == null)
                {
                    throw new ArgumentNullException(nameof(TokenProviderOptions.IdentityResolver));
                }

                if (options.ApiContext == null)
                {
                    throw new ArgumentNullException(nameof(TokenProviderOptions.ApiContext));
                }

                if (options.LogWriteLine == null)
                {
                    throw new ArgumentNullException(nameof(TokenProviderOptions.LogWriteLine));
                }

                if (options.LogWriteLineEE == null)
                {
                    throw new ArgumentNullException(nameof(TokenProviderOptions.LogWriteLineEE));
                }

                if (options.SigningCredentials == null)
                {
                    throw new ArgumentNullException(nameof(TokenProviderOptions.SigningCredentials));
                }

                if (options.NonceGenerator == null)
                {
                    throw new ArgumentNullException(nameof(TokenProviderOptions.NonceGenerator));
                }
            }
            catch (Exception ex)
            {
                var ee = ex.AsExtract("ELI43197");
                _logWriteLineEE(ee);

                throw ee;
            }
        }

        /// <summary>
        /// Get this datetime as a Unix epoch timestamp (seconds since Jan 1, 1970, midnight UTC).
        /// </summary>
        /// <param name="date">The date to convert.</param>
        /// <returns>Seconds since Unix epoch.</returns>
        public static long ToUnixEpochDate(DateTime date)
        {
            try
            {
                return new DateTimeOffset(date).ToUniversalTime().ToUnixTimeSeconds();
            }
            catch (Exception ex)
            {
                var ee = ex.AsExtract("ELI43196");
                _logWriteLineEE(ee);

                throw ee;
            }
        }
    }
}
