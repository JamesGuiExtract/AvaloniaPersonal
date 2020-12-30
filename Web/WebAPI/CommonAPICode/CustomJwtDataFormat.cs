using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.IdentityModel.Tokens;

namespace WebAPI
{
    /// <summary>
    /// Custom JSON Web Token (JWT) format class
    /// </summary>
    public class CustomJwtDataFormat : ISecureDataFormat<AuthenticationTicket>
    {
        private readonly string algorithm;
        private readonly TokenValidationParameters validationParameters;

        /// <summary>
        /// CTOR
        /// </summary>
        /// <param name="algorithm">crypto algorithm to use</param>
        /// <param name="validationParameters">token validation parameters</param>
        public CustomJwtDataFormat(string algorithm, TokenValidationParameters validationParameters)
        {
            this.algorithm = algorithm;
            this.validationParameters = validationParameters;
        }

        /// <summary>
        /// un-protect (decrypt)
        /// </summary>
        /// <param name="protectedText">encrypted text</param>
        /// <returns>auth ticket</returns>
        public AuthenticationTicket Unprotect(string protectedText)
            => Unprotect(protectedText, null);

        /// <summary>
        /// un-protect overload
        /// </summary>
        /// <param name="protectedText">encrptyed text</param>
        /// <param name="purpose">unused</param>
        /// <returns>auth ticket</returns>
        public AuthenticationTicket Unprotect(string protectedText, string purpose)
        {
            var handler = new JwtSecurityTokenHandler();
            ClaimsPrincipal principal = null;
            SecurityToken validToken = null;

            try
            {
                principal = handler.ValidateToken(protectedText, this.validationParameters, out validToken);
                
                var validJwt = validToken as JwtSecurityToken;

                if (validJwt == null)
                {
                    throw new ArgumentException("Invalid JWT");
                }

                if (!validJwt.Header.Alg.Equals(algorithm, StringComparison.Ordinal))
                {
                    throw new ArgumentException($"Algorithm must be '{algorithm}'");
                }

                // Additional custom validation of JWT claims here (if any)
            }
            catch (SecurityTokenValidationException)
            {
                return null;
            }
            catch (ArgumentException)
            {
                return null;
            }

            // Validation passed. Return a valid AuthenticationTicket:
            return new AuthenticationTicket(principal, new AuthenticationProperties(), "Cookie");
        }

        /// <summary>
        /// protect - should never be called
        /// </summary>
        /// <param name="data">auth ticket</param>
        /// <returns>encrypted string</returns>
        // This ISecureDataFormat implementation is decode-only
        public string Protect(AuthenticationTicket data)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// protect - should never be called
        /// </summary>
        /// <param name="data">auth ticket</param>
        /// <param name="purpose">unknown - unused</param>
        /// <returns>encrypted string</returns>
        public string Protect(AuthenticationTicket data, string purpose)
        {
            throw new NotImplementedException();
        }
    }
}
