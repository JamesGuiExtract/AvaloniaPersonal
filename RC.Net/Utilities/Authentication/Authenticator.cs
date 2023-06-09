﻿using Microsoft.Identity.Client;
using System;
using System.Linq;
using System.Net;
using System.Security;
using System.Threading.Tasks;
using UCLID_FILEPROCESSINGLib;

namespace Extract.Utilities.Authentication
{
    public class Authenticator
    {
        // Below are the clientId (Application Id) of your app registration and the tenant information. 
        // You have to replace:
        // - the content of ClientID with the Application Id for your app registration
        // - The content of Tenant by the information about the accounts allowed to sign-in in your application:
        //   - For Work or School account in your org, use your tenant ID, or domain
        //   - for any Work or School accounts, use organizations
        //   - for Microsoft Personal account, use consumers
        private readonly string _clientId;

        // Note: Tenant is important for the quickstart.
        private readonly string _tenant;
        private readonly string _instance;
        private IPublicClientApplication _clientApp;

        readonly string[] twoFactorScope = new string[] { "user.read" };

        readonly string[] emailScope = new string[] { "user.read", "Mail.ReadWrite", "Mail.ReadWrite.Shared" };

        static Authenticator()
        {
            // Add support for secure protocols when this is run from non-CLR apps (else ServicePointManager.SecurityProtocol = Ssl3 | Tls)
            // https://extract.atlassian.net/browse/ISSUE-18425
            ServicePointManager.SecurityProtocol |=
                 SecurityProtocolType.Tls11
                | SecurityProtocolType.Tls12
                | SecurityProtocolType.Tls13;
        }

        /// <summary>
        /// Create an instance by retrieving azure application settings from a database
        /// </summary>
        [CLSCompliant(false)]
        public Authenticator(FileProcessingDB fileProcessingDB)
            : this(fileProcessingDB.GetDBInfoSetting("AzureClientId", false),
                fileProcessingDB.GetDBInfoSetting("AzureTenant", false),
                fileProcessingDB.GetDBInfoSetting("AzureInstance", false))
        {
        }

        /// <summary>
        /// Create an instance with the supplied azure application settings
        /// </summary>
        public Authenticator(string clientID, string tenant, string instance)
        {
            try
            {
                _clientId = clientID;
                _tenant = tenant;
                _instance = instance;

                if (string.IsNullOrWhiteSpace(_clientId))
                {
                    throw new ExtractException("ELI51888", "Missing Client ID! Configure in DBAdmin (Database->Database options->Azure)");
                }
                if (string.IsNullOrWhiteSpace(_tenant))
                {
                    throw new ExtractException("ELI51889", "Missing Tenant! Configure in DBAdmin (Database->Database options->Azure)");
                }
                if (string.IsNullOrWhiteSpace(_instance))
                {
                    throw new ExtractException("ELI51890", "Missing Instance! Configure in DBAdmin (Database->Database options->Azure)");
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53204");
            }
        }

        public async Task<AuthenticationResult> GetATokenForGraphUserNamePassword(SecureString password, string userName)
        {
            IPublicClientApplication app;
            app = PublicClientApplicationBuilder.Create(_clientId)
                                                .WithAuthority(InstanceTenantConcat(_instance, _tenant))
                                                .Build();
            var accounts = await app.GetAccountsAsync();

            AuthenticationResult result;
            if (accounts.Any())
            {
                result = await app.AcquireTokenSilent(twoFactorScope, accounts.FirstOrDefault())
                                    .ExecuteAsync();
            }
            else
            {
                try
                {
                    result = await app.AcquireTokenByUsernamePassword(emailScope,
                                                                        userName,
                                                                        password)
                                        .ExecuteAsync();
                }
                catch (MsalUiRequiredException ex) when (ex.Message.Contains("AADSTS65001"))
                {
                    var ee = ex.AsExtract("ELI53125");
                    ee.AddDebugData("Info", "The user does not have access to the app registration, or the app registration was configured incorrectly");
                    throw ee;
                }
                catch (MsalServiceException ex) when (ex.ErrorCode == "invalid_request")
                {
                    var ee = ex.AsExtract("ELI53126");
                    ee.AddDebugData("Info", "AADSTS90010: The grant type is not supported over the /common or /consumers endpoints. Please use the /organizations or tenant-specific endpoint");
                    throw ee;

                }
                catch (MsalServiceException ex) when (ex.ErrorCode == "unauthorized_client")
                {
                    var ee = ex.AsExtract("ELI53127");
                    ee.AddDebugData("Info", "Application with identifier '{clientId}' was not found in the directory '{domain}'");
                    throw ee;
                }
                catch (MsalServiceException ex) when (ex.ErrorCode == "invalid_client")
                {
                    var ee = ex.AsExtract("ELI53128");
                    ee.AddDebugData("Info", "The request body must contain the following parameter: 'client_secret or client_assertion'");
                    throw ee;
                }
                catch (MsalServiceException ex)
                {
                    throw ex.AsExtract("ELI53129");
                }
                catch (MsalClientException ex) when (ex.ErrorCode == "unknown_user_type"
                                                    || ex.ErrorCode == "user_realm_discovery_failed"
                                                    || ex.ErrorCode == "unknown_user")
                {
                    throw new ArgumentException("U/P: Wrong username", ex).AsExtract("ELI53131");
                }
                catch (MsalClientException ex) when (ex.ErrorCode == "parsing_wstrust_response_failed")
                {
                    throw new ExtractException("ELI53130", "Incorrect username or password");
                }
            }

            return result;
        }

        /// <summary>
        /// Call AcquireToken - to acquire a token requiring user to sign-in
        /// </summary>
        public async Task<AuthenticationResult> SignInMicrosoftGraph(bool forceMultifactorAuthentication)
        {
            try
            {
                if (_clientApp != null)
                {
                    await SignOut();
                }

                AuthenticationResult authResult;

                IAccount firstAccount;
                CreateApplication();

                if (forceMultifactorAuthentication)
                {
                    //  Use any account(Azure AD). It's not using WAM
                    var accounts = await _clientApp.GetAccountsAsync();
                    firstAccount = accounts.FirstOrDefault();
                }
                else
                {
                    // WAM will always get an account in the cache. So if we want
                    // to have a chance to select the accounts interactively, we need to
                    // force the non-account
                    firstAccount = PublicClientApplication.OperatingSystemAccount;
                }

                try
                {
                    authResult = await _clientApp.AcquireTokenInteractive(twoFactorScope)
                           .WithAccount(firstAccount)
                           .WithPrompt(Prompt.ForceLogin)
                           .ExecuteAsync();
                }
                catch (Exception ex)
                {
                    throw new ExtractException("ELI51784", "Error Acquiring Token", ex);
                }

                return authResult;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI53205");
            }
        }

        /// <summary>
        /// Sign out the current user
        /// </summary>
        private async Task SignOut()
        {
            var accounts = await _clientApp.GetAccountsAsync();
            if (accounts.Any())
            {
                try
                {
                    await _clientApp.RemoveAsync(accounts.FirstOrDefault());
                }
                catch (MsalException ex)
                {
                    throw new ExtractException("ELI51785", "Error signing-out user", ex);
                }
            }
        }

        public void CreateApplication()
        {
            var builder = PublicClientApplicationBuilder.Create(_clientId)
                .WithAuthority(InstanceTenantConcat(_instance, _tenant))
                .WithDefaultRedirectUri();

            _clientApp = builder.Build();
        }

        private static Uri InstanceTenantConcat(string instance, string tenant)
        {
            return new Uri(instance.EndsWith("/", StringComparison.OrdinalIgnoreCase) ? $"{instance}{tenant}" : $"{instance}/{tenant}");
        }
    }
}