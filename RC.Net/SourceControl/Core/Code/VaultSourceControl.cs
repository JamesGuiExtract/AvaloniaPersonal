using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using VaultClientIntegrationLib;
using VaultClientNetLib;

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
                LogOnDialog dialog = new LogOnDialog(settings);
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
                if (!IsUsageException(ex))
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

        static bool IsUsageException(Exception ex)
        {
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

        #endregion
    }
}
