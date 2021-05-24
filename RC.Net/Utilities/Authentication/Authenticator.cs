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
        private readonly string ClientId;// = "190b65ca-620a-4979-95eb-f79c4b0da944";

        // Note: Tenant is important for the quickstart.
        private readonly string Tenant;// = "bd07e2c0-7f9a-478c-a4f2-0d3865717565";
        private readonly string Instance;// = "https://login.microsoftonline.com/";
        private IPublicClientApplication _clientApp;

        public IPublicClientApplication PublicClientApp { get { return _clientApp; } }

        //Set the scope for API call to user.read
        readonly string[] scopes = new string[] { "user.read" };

        private readonly FileProcessingDB _fileProcessingDB;

        public Authenticator(string databaseName, string databaseServer)
        {
            _fileProcessingDB = new FileProcessingDB()
            {
                DatabaseServer = databaseServer,
                DatabaseName = databaseName
            };

            ClientId = _fileProcessingDB.GetDBInfoSetting("AzureClientId", false);
            Tenant = _fileProcessingDB.GetDBInfoSetting("AzureTenant", false);
            Instance = _fileProcessingDB.GetDBInfoSetting("AzureInstance", false);
        }

        /// <summary>
        /// Call AcquireToken - to acquire a token requiring user to sign-in
        /// </summary>
        public async Task<AuthenticationResult> SignInMicrosoftGraph(bool forceMFA)
        {
            if (PublicClientApp != null)
            {
                SignOut();
            }

            AuthenticationResult authResult;

            IAccount firstAccount;
            CreateApplication(!forceMFA);

            if(forceMFA)
            {
                //  Use any account(Azure AD). It's not using WAM
                var accounts = await PublicClientApp.GetAccountsAsync();
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
                authResult = await PublicClientApp.AcquireTokenInteractive(scopes)
                       .WithAccount(firstAccount)
                       .WithPrompt(Prompt.ForceLogin)
                       .ExecuteAsync();
            }
            catch (Exception ex)
            {
                throw new ExtractException("ELI51784", $"Error Acquiring Token:{Environment.NewLine}{ex}");
            }

            return authResult;
        }

        /// <summary>
        /// Perform an HTTP GET request to a URL using an HTTP Authorization header
        /// </summary>
        /// <param name="url">The URL</param>
        /// <param name="token">The token</param>
        /// <returns>String containing the results of the GET operation</returns>
        public async Task<string> GetHttpContentWithToken(string url, string token)
        {
            var httpClient = new System.Net.Http.HttpClient();
            System.Net.Http.HttpResponseMessage response;
            try
            {
                var request = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Get, url);
                //Add the token in Authorization header
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                response = await httpClient.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();
                return content;
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        /// <summary>
        /// Sign out the current user
        /// </summary>
        private async void SignOut()
        {
            var accounts = await PublicClientApp.GetAccountsAsync();
            if (accounts.Any())
            {
                try
                {
                    await PublicClientApp.RemoveAsync(accounts.FirstOrDefault());

                }
                catch (MsalException ex)
                {
                    throw new ExtractException("ELI51785", $"Error signing-out user: {ex.Message}");
                }
            }
        }

        public void CreateApplication(bool useWam)
        {
            var builder = PublicClientApplicationBuilder.Create(ClientId)
                .WithAuthority($"{Instance}{Tenant}")
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