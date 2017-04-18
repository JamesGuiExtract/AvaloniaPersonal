﻿// Copyright (c) Nate Barbettini. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using Extract;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;

namespace SimpleTokenProvider
{
    /// <summary>
    /// Provides options for <see cref="TokenProviderMiddleware"/>.
    /// </summary>
    public class TokenProviderOptions
    {
        /// <summary>
        /// The relative request path to listen on.
        /// </summary>
        /// <remarks>The default path is <c>/token</c>.</remarks>
        public string Path { get; set; } = "/token";

        /// <summary>
        ///  The Issuer (iss) claim for generated tokens.
        /// </summary>
        public string Issuer { get; set; }

        /// <summary>
        /// The Audience (aud) claim for the generated tokens.
        /// </summary>
        public string Audience { get; set; }

        /// <summary>
        /// The expiration time for the generated tokens.
        /// </summary>
        /// <remarks>The default is five minutes (300 seconds).</remarks>
        public TimeSpan Expiration { get; set; } = TimeSpan.FromHours(12);

        /// <summary>
        /// The signing key to use when generating tokens.
        /// </summary>
        public SigningCredentials SigningCredentials { get; set; }

        /// <summary>
        /// Resolves a user identity given a username and password.
        /// <remarks>the three strings below are input arguments that correspond to username, password, and workflowname</remarks>
        /// </summary>
        public Func<string, string, string, Task<ClaimsIdentity>> IdentityResolver { get; set; }

        /// <summary>
        /// Gets an API Context, returning Tuple of (DatabaseServerName, DatabaseName, WorkflowName)
        /// Used to populate the claims
        /// </summary>
        public Func<Tuple<string, string, string>> ApiContext { get; set; }

        /// <summary>
        /// Gets a log writeLine method - used to log to the extract UEX log.
        /// </summary>
        public Action<string> LogWriteLine { get; set; }

        /// <summary>
        /// Gets a log writeLine(ExtractException) method - used to log EE to the extract UEX log.
        /// </summary>
        public Action<ExtractException> LogWriteLineEE { get; set; }

        /// <summary>
        /// Generates a random value (nonce) for each generated token.
        /// </summary>
        /// <remarks>The default nonce is a random GUID.</remarks>
        public Func<Task<string>> NonceGenerator { get; set; }
            = new Func<Task<string>>(() => Task.FromResult(Guid.NewGuid().ToString()));
    }
}