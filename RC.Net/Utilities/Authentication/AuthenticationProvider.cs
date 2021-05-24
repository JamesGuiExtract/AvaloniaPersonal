using Extract.Interfaces;
using System;
using System.Runtime.InteropServices;
using UCLID_FILEPROCESSINGLib;

namespace Extract.Utilities.Authentication
{

    [ComVisible(true)]
    [Guid("C59D38E2-1A83-4DC7-8809-2E6C16677FA4")]
    [ProgId("Extract.Utilities.AuthenticationProvider")]
    public sealed class AuthenticationProvider : IAuthenticationProvider
    {
        public void PromptForAndValidateWindowsCredentials(string databaseName, string databaseServer)
        {
            try
            {
                new Authenticator(databaseName,databaseServer).SignInMicrosoftGraph(true).Wait();
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI49620", "User authentication failed");
            }
        }
    }
    
}
