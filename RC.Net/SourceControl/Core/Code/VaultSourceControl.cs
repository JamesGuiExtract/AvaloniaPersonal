using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using VaultClientIntegrationLib;
using VaultClientNetLib;
using System.Web.Services.Protocols;

namespace Extract.SourceControl
{
    /// <summary>
    /// Represents the Vault source control database.
    /// </summary>
    internal class VaultSourceControl : ISourceControl
    {
        #region VaultSourceControl Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="VaultSourceControl"/> class.
        /// </summary>
        public VaultSourceControl(LogOnSettings settings)
        {
            bool loggedIn = AttemptLogin(settings, false);
            if (!loggedIn)
            {
                LogOnSettings storedSettings = new LogOnSettings(
                    ServerOperations.client.LoginOptions.URL,
                    ServerOperations.client.LoginOptions.User);

                LogOnDialog dialog = new LogOnDialog(storedSettings);
                while (!loggedIn)
                {
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        loggedIn = AttemptLogin(dialog.LogOnSettings, true);
                    }
                    else
                    {
                        throw new InvalidOperationException("Action cancelled by user.");
                    } 
                }
            }
        }

        static bool AttemptLogin(LogOnSettings settings, bool saveSettings)
        {
            ServerOperations.client.LoginOptions.URL = settings.Server;
            ServerOperations.client.LoginOptions.User = settings.UserName;
            ServerOperations.client.LoginOptions.Password = settings.Password;

            // TODO: Is this necessary?
            ServerOperations.client.LoginOptions.Repository = "Extract";

            bool loggedIn = true;
            try
            {
                ServerOperations.Login(VaultConnection.AccessLevelType.Client, !saveSettings, 
                    saveSettings);
            }
            catch (Exception ex)
            {
                if (!IsInvalidPasswordException(ex))
                {
                    throw;
                }

                if (saveSettings)
                {
                    MessageBox.Show("Invalid username or password", "Invalid login",
                        MessageBoxButtons.OK, MessageBoxIcon.None, MessageBoxDefaultButton.Button1, 0);
                }

                loggedIn = false;
            }
            return loggedIn;
        }

        #endregion VaultSourceControl Constructors

        #region VaultSourceControl Methods

        static bool IsInvalidPasswordException(Exception ex)
        {
            // Check for inner soap exception with FailNotValidLogin message
            SoapException soapEx = ex.InnerException as SoapException;
            if (ex != null)
            {
                return soapEx.Message.StartsWith("1000", StringComparison.Ordinal);
            }

            // If no session information stored it is a usage exception (ie. first time logging in)
            return ex.GetType().ToString().EndsWith("UsageException", StringComparison.Ordinal);
        }

        #endregion VaultSourceControl Methods

        #region ISourceControl Members

        public void Open(string repository)
        {
            // TODO
            //throw new NotImplementedException("The method or operation is not implemented.");
        }

        public ISourceControlItem GetItem(string path)
        {
            return new VaultSourceControlItem(path);
        }

        /// <summary>
        /// Refreshes the database connection
        /// </summary>
        public void RefreshConnection()
        {
            RepositoryUtil.Refresh();
        }

        #endregion
    }
}
