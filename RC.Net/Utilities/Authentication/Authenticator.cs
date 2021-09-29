using Microsoft.Identity.Client;
using Microsoft.Identity.Client.Desktop;
using System;
using System.Linq;
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
        //   - for any Work or School accounts, or Microsoft personal account, use bd07e2c0-7f9a-478c-a4f2-0d3865717565
        //   - for Microsoft Personal account, use consumers
        private readonly string _clientId;// = "190b65ca-620a-4979-95eb-f79c4b0da944";

        // Note: Tenant is important for the quickstart.
        private readonly string _tenant;// = "bd07e2c0-7f9a-478c-a4f2-0d3865717565";
        private readonly string _instance;// = "https://login.microsoftonline.com/";
        private IPublicClientApplication _clientApp;

        //Set the scope for API call to user.read
        readonly string[] scopes = new string[] { "user.read" };

        private readonly FileProcessingDB _fileProcessingDB;

        [CLSCompliant(false)]
        public Authenticator(FileProcessingDB fileProcessingDB)
        {
            _fileProcessingDB = fileProcessingDB;

            _clientId = _fileProcessingDB.GetDBInfoSetting("AzureClientId", false);
            _tenant = _fileProcessingDB.GetDBInfoSetting("AzureTenant", false);
            _instance = _fileProcessingDB.GetDBInfoSetting("AzureInstance", false);

            if (string.IsNullOrWhiteSpace(_clientId))
            {
                throw new ExtractException("ELI51888", "You need to specify a ClientId in the database administration tool (azure settings).");
            }
            if (string.IsNullOrWhiteSpace(_tenant))
            {
                throw new ExtractException("ELI51889", "You need to specify a Tenant in the database administration tool (azure settings).");
            }
            if (string.IsNullOrWhiteSpace(_instance))
            {
                throw new ExtractException("ELI51890", "You need to specify an Instance in the database administration tool (azure settings).");
            }
        }

        /// <summary>
        /// Call AcquireToken - to acquire a token requiring user to sign-in
        /// </summary>
        public async Task<AuthenticationResult> SignInMicrosoftGraph(bool forceMFA)
        {
            if (_clientApp != null)
            {
                await SignOut();
            }

            AuthenticationResult authResult;

            IAccount firstAccount;
            CreateApplication(!forceMFA);

            if(forceMFA)
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
                authResult = await _clientApp.AcquireTokenInteractive(scopes)
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

        public void CreateApplication(bool useWam)
        {
            var builder = PublicClientApplicationBuilder.Create(_clientId)
                .WithAuthority($"{_instance}{_tenant}")
                .WithDefaultRedirectUri();

            if (useWam)
            {
                builder.WithExperimentalFeatures();
                builder.WithWindowsBroker(true);  // Requires redirect URI "ms-appx-web://microsoft.aad.brokerplugin/{client_id}" in app registration
            }
            _clientApp = builder.Build();
        }
    }
}