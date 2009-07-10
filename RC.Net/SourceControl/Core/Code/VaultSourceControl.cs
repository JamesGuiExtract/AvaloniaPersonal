using System;
using System.Collections.Generic;
using System.Text;
using System.Web.Services.Protocols;
using System.Windows.Forms;
using VaultClientIntegrationLib;
using VaultClientNetLib;
using VaultClientOperationsLib;
using VaultLib;

namespace Extract.SourceControl
{
    /// <summary>
    /// Represents the Vault source control database.
    /// </summary>
    internal class VaultSourceControl : ISourceControl
    {
        #region VaultSourceControl Constants

        /// <summary>
        /// The repository root of the engineering tree.
        /// </summary>
        static readonly string _ENGINEERING_ROOT = "$/Engineering";

        /// <summary>
        /// History item actions for which to query.
        /// </summary>
        static readonly long[] _HISTORY_ITEM_ACTIONS = new long[] 
        { 
            VaultRequestType.AddFile, 
            VaultRequestType.AddFolder,
            VaultRequestType.CheckIn,
            VaultRequestType.CopyBranch,
            VaultRequestType.Delete,
            VaultRequestType.Move,
            VaultRequestType.Obliterate,
            VaultRequestType.Rename,
            VaultRequestType.Restore,
            VaultRequestType.Rollback,
            VaultRequestType.Share,
            VaultRequestType.ShareBranch,
            VaultRequestType.Undelete
        };

        /// <summary>
        /// The maximum number of history items to retrieve.
        /// </summary>
        const int _MAX_HISTORY_ITEMS = 1000;

        #endregion VaultSourceControl Constants

        #region VaultSourceControl Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="VaultSourceControl"/> class.
        /// </summary>
        public VaultSourceControl(LogOnSettings settings)
        {
            bool loggedIn = AttemptLogin(settings, false);
            if (!loggedIn)
            {
                LogOnSettings storedSettings = 
                    new LogOnSettings(LoginOptions.URL, LoginOptions.User);

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
            LoginOptions.URL = settings.Server;
            LoginOptions.User = settings.UserName;
            LoginOptions.Password = settings.Password;

            // TODO: Is this necessary?
            LoginOptions.Repository = "Extract";

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

        #region VaultSourceControl Properties

        /// <summary>
        /// Gets the client instance associated with Vault.
        /// </summary>
        /// <returns>The client instance associated with Vault.</returns>
        static ClientInstance Client
        {
            get
            {
                return ServerOperations.client.ClientInstance;
            }
        }

        /// <summary>
        /// Gets the connection associated with Vault.
        /// </summary>
        /// <returns>The connection associated with Vault.</returns>
        static VaultConnection Connection
        {
            get
            {
                return Client.Connection;
            }
        }

        /// <summary>
        /// Gets the login options associated with Vault.
        /// </summary>
        /// <returns>The login options associated with Vault.</returns>
        static LoginOptions LoginOptions
        {
            get
            {
                return ServerOperations.client.LoginOptions;
            }
        }
		
	    #endregion VaultSourceControl Properties

        #region VaultSourceControl Methods

        static bool IsInvalidPasswordException(Exception ex)
        {
            // Check for inner soap exception with FailNotValidLogin message
            SoapException soapEx = ex.InnerException as SoapException;
            if (soapEx != null)
            {
                return soapEx.Message.StartsWith("1000", StringComparison.Ordinal);
            }

            // If no session information stored it is a usage exception (ie. first time logging in)
            return ex.GetType().ToString().EndsWith("UsageException", StringComparison.Ordinal);
        }

        static VaultUser GetCurrentUser()
        {
            VaultUser[] users = null;
            Connection.GetUserList(ref users);
            foreach (VaultUser user in users)
	        {
                if (IsCurrentUser(user))
	            {
                    return user;
	            }
	        }

            throw new InvalidOperationException("Currently logged in user could not be determined.");
        }

        static VaultClientTreeObject GetEngineeringRoot()
        {
            return RepositoryUtil.FindVaultTreeObjectAtReposOrLocalPath(_ENGINEERING_ROOT);
        }

        static bool IsCurrentUser(VaultUser user)
        {
            return user.Login.Equals(Connection.UserLogin, StringComparison.OrdinalIgnoreCase);
        }

        static VaultHistoryItem[] GetVaultHistoryItems(VaultHistoryQueryRequest query)
        {
            string token = null;

            try
            {
                // Start the history query
                int rowsRetrieved = 0;
                Connection.HistoryBegin(query, _MAX_HISTORY_ITEMS, ref rowsRetrieved, ref token);

                // Get the history items
                VaultHistoryItem[] items = null;
                if (rowsRetrieved > 0)
                {
                    Connection.HistoryFetch(token, 0, rowsRetrieved - 1, ref items);
                }
                return items ?? new VaultHistoryItem[0];
            }
            finally
            {
                // End the history query
                if (token != null)
                {
                    Connection.HistoryEnd(token);
                }
            }
        }

        #endregion VaultSourceControl Methods

        #region ISourceControl Members

        /// <summary>
        /// Opens a connection to the specified repository.
        /// </summary>
        /// <param name="repository">If repository is <see langword="null"/> opens the last used 
        /// repository; otherwise opens the specified repository.</param>
        public void Open(string repository)
        {
            // TODO
            //throw new NotImplementedException("The method or operation is not implemented.");
        }

        /// <summary>
        /// Gets the specified source control item.
        /// </summary>
        /// <param name="path">The repository path to the item.</param>
        /// <returns>The source control item at <paramref name="path"/>.</returns>
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

        /// <summary>
        /// Gets the physical directory to which the repository root is bound.
        /// </summary>
        /// <returns>The physical directory to which the repository root is bound.</returns>
        public string GetRootDirectory()
        {
            return Client.TreeCache.GetBestWorkingFolder("$");
        }

        /// <summary>
        /// Gets the changes made by the currently logged in user within the specified time range.
        /// </summary>
        /// <param name="startDate">The start date of the changes.</param>
        /// <param name="endDate">The end date of the changes.</param>
        /// <returns>The changes made by the currently logged in user after 
        /// <paramref name="startDate"/> and before <paramref name="endDate"/>.</returns>
        public IEnumerable<IHistoryItem> GetUserHistoryItems(DateTime startDate, DateTime endDate)
        {
            // Prepare vault history query
            VaultHistoryQueryRequest query = new VaultHistoryQueryRequest();
            query.RepID = Client.ActiveRepositoryID;

            // Search from the engineering root
            VaultClientTreeObject root = GetEngineeringRoot();
            query.TopName = root.FullPath;
            query.TopID = root.ID;

            // Don't sort the result
            query.Sorts = new long[0];

            // Only return changes by the current user
            query.Users = new VaultUser[] { GetCurrentUser() };

            // Set the date range
            query.BeginDate = new VaultDateTime(startDate, VaultDateTime.LocalKind);
            query.EndDate = new VaultDateTime(endDate, VaultDateTime.LocalKind);
            query.DateFilterMask = VaultQueryRequestDateTypes.IncludeRange;

            // Set the actions for which to search
            query.Actions = _HISTORY_ITEM_ACTIONS;

            // Iterate through the Vault history items
            foreach (VaultHistoryItem item in GetVaultHistoryItems(query))
            {
                yield return new VaultSourceControlHistoryItem(item);
            }
        }

        #endregion ISourceControl Members
    }
}
