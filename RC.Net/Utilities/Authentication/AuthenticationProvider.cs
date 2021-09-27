using Extract.Interfaces;
using Microsoft.Identity.Client;
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using UCLID_FILEPROCESSINGLib;

namespace Extract.Utilities.Authentication
{

    [ComVisible(true)]
    [Guid("C59D38E2-1A83-4DC7-8809-2E6C16677FA4")]
    [ProgId("Extract.Utilities.AuthenticationProvider")]
    public sealed class AuthenticationProvider : IAuthenticationProvider
    {
        /// <summary>
        /// This prompt will stall any .net UI. Use the WinForms call for any .net calls.
        /// </summary>
        /// <param name="databaseName"></param>
        /// <param name="databaseServer"></param>
        public void PromptForAndValidateWindowsCredentials(string databaseName, string databaseServer)
        {
            try
            {
                Task authenticator = new Authenticator(databaseName,databaseServer).SignInMicrosoftGraph(true);
                authenticator.Wait();
                if(authenticator.IsFaulted)
                {
                    throw authenticator.Exception.InnerException;
                }
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI49620", "User authentication failed");
            }
        }

        /// <summary>
        /// This will wait for any UI threads to continue execution to prevent a deadlock.
        /// </summary>
        /// <param name="databaseName"></param>
        /// <param name="databaseServer"></param>
        public void PromptForAndValidateWindowsCredentialsWinForms(string databaseName, string databaseServer)
        {
            try
            {
                Task auth = new Authenticator(databaseName, databaseServer).SignInMicrosoftGraph(true);
                while (!(auth.IsCompleted || auth.IsFaulted || auth.IsCanceled))
                {
                    Application.DoEvents();
                }
                if (auth.IsFaulted)
                {
                    throw (auth.Exception.InnerException).AsExtract("ELI51896");
                }
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI51897", "User authentication failed");
            }
        }

        public bool IsAuthenticationRequired (string databaseName, string databaseServer, string oneTimePassword)
        {
            try
            {
                var fileProcessingDB = new FileProcessingDB()
                {
                    DatabaseServer = databaseServer,
                    DatabaseName = databaseName
                };

                if (!string.IsNullOrEmpty(oneTimePassword))
                {
                    fileProcessingDB.LoginUser("<Admin>", oneTimePassword);
                }
                else if (fileProcessingDB.GetDBInfoSetting("RequireAuthenticationBeforeRun", true).Equals("1"))
                {
                    return true;
                }
                return false;
            }
            catch(Exception ex)
            {
                throw ex.AsExtract("ELI51895");
            }
        }
    }
    
}
