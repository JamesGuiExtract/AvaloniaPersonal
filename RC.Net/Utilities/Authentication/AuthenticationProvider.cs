using System;
using System.Runtime.InteropServices;
using System.Threading;
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
        /// Display a login window and validate the input credentials against the Azure Active Directory
        /// per the app registration configured in the specified file processing database.
        /// <param name="database">The database to get configuration from</param>
        [CLSCompliant(false)]
        public void PromptForAndValidateWindowsCredentials(FileProcessingDB database)
        {
            try
            {
                Task auth = new Authenticator(database).SignInMicrosoftGraph(true);

                // If this wasn't called from the main UI thread then it is OK to wait on the task
                if (SynchronizationContext.Current is null)
                {
                    auth.Wait();
                }
                else
                {
                    // Prevent deadlocks by allowing the UI thread to process the event queue
                    while (!(auth.IsCompleted || auth.IsFaulted || auth.IsCanceled))
                    {
                        Application.DoEvents();
                    }
                }
                if (auth.IsFaulted)
                {
                    throw auth.Exception.InnerException;
                }
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI49620", "User authentication failed");
            }
        }

        /// <summary>
        /// Check to see if it is necessary to have a user authenticate against a domain acccount
        /// </summary>
        /// <param name="database">The database to check for the authentication requirement</param>
        /// <param name="onetimePassword">Optional one-time password to validate</param>
        /// <remarks>If onetimePassword is given and is valid then this method will return true;
        /// if oneTimePassword is invalid then this method will throw an exception</remarks>
        [CLSCompliant(false)]
        public bool IsAuthenticationRequired(FileProcessingDB database, string onetimePassword)
        {
            try
            {
                if (!string.IsNullOrEmpty(onetimePassword))
                {
                    database.LoginUser("<Admin>", onetimePassword);
                }
                else if (database.GetDBInfoSetting("RequireAuthenticationBeforeRun", true).Equals("1"))
                {
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI51895");
            }
        }
    }

}
