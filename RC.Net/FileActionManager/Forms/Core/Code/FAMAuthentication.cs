using System;
using UCLID_FILEPROCESSINGLib;

namespace Extract.FileActionManager.Forms
{
    public static class FAMAuthentication
    {
        /// <summary>
        /// Displays a dialog for a user to authenticate against a domain acccount if required
        /// </summary>
        /// <param name="fileProcessingDB">The database to check for the authentication requirement</param>
        /// <param name="onetimePassword">Optional one-time password to validate</param>
        /// <remarks>If onetimePassword is given and is valid then this method will return succeed;
        /// if onetimePassword is invalid then this method will throw an exception</remarks>
        [CLSCompliant(false)]
        public static void PromptForAndValidateWindowsCredentialsIfRequired(FileProcessingDB fileProcessingDB,
            string onetimePassword = null)
        {
            if (fileProcessingDB == null
                || String.IsNullOrEmpty(fileProcessingDB.DatabaseServer)
                || String.IsNullOrEmpty(fileProcessingDB.DatabaseName))
            {
                return;
            }

            try
            {
                var authenticationProvider = (IAuthenticationProvider)
                    Activator.CreateInstance(Type.GetTypeFromProgID("Extract.Utilities.AuthenticationProvider"));
                if (authenticationProvider.IsAuthenticationRequired(fileProcessingDB, onetimePassword))
                {
                    authenticationProvider.PromptForAndValidateWindowsCredentials(fileProcessingDB);
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI51898");
            }
        }

        /// <summary>
        /// Displays a dialog for a user to authenticate against a domain acccount if required
        /// </summary>
        /// <param name="databaseServer">The server for the database to check for the authentication requirement</param>
        /// <param name="databaseName">The name of the database to check for the authentication requirement</param>
        /// <param name="onetimePassword">Optional one-time password to validate</param>
        /// <remarks>If onetimePassword is given and is valid then this method will return fail;
        /// if onetimePassword is invalid then this method will throw an exception</remarks>
        [CLSCompliant(false)]
        public static void PromptForAndValidateWindowsCredentialsIfRequired(string databaseServer, string databaseName,
            string onetimePassword = null)
        {
            if (String.IsNullOrEmpty(databaseServer)
                || String.IsNullOrEmpty(databaseName))
            {
                return;
            }

            FileProcessingDBClass fileProcessingDB = new() { DatabaseServer = databaseServer, DatabaseName = databaseName };
            PromptForAndValidateWindowsCredentialsIfRequired(fileProcessingDB, onetimePassword);
        }
    }
}
