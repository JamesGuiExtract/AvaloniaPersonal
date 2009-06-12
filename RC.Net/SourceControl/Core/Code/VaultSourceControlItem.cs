using System;
using System.Collections.Generic;
using System.Text;
using VaultClientIntegrationLib;
using System.Collections;
using VaultClientOperationsLib;

namespace Extract.SourceControl
{
    /// <summary>
    /// Represents a Vault source control file.
    /// </summary>
    public class VaultSourceControlItem : ISourceControlItem
    {
        #region VaultSourceControlItem Fields

        readonly VaultClientFile _file;
        string _path;

        #endregion VaultSourceControlItem Fields

        #region VaultSourceControlItem Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="VaultSourceControlItem"/> class.
        /// </summary>
        public VaultSourceControlItem(string path)
        {
            _file = RepositoryUtil.FindVaultFileAtReposOrLocalPath(path);
        }

        #endregion VaultSourceControlItem Constructors

        #region VaultSourceControlItem Properties

        /// <summary>
        /// Gets the Vault client instance.
        /// </summary>
        /// <returns>The Vault client instance.</returns>
        static ClientInstance Client
        {
            get
            {
                return ServerOperations.client.ClientInstance;
            }
        }

        #endregion VaultSourceControlItem Properties

        #region ISourceControlItem Members

        /// <summary>
        /// Gets the local path of the source control item.
        /// </summary>
        /// <returns>The local path of the source control item.</returns>
        public string LocalSpec
        {
            get
            {
                if (_path == null)
                {
                    _path = Client.GetWorkingFilename(_file);
                }

                return _path;
            }
        }

        /// <summary>
        /// Checks in the source control item.
        /// </summary>
        public void CheckIn()
        {
            // Since we already have VaultClientFile, it would be faster to go through 
            // ClientInstance directly, but ProcessCommandCheckin does a lot of additional 
            // processing (e.g. if no changes were made the check out is undone, ensures the 
            // file is checked out to the current user). It's worth the hit for the extra work.
            ServerOperations.ProcessCommandCheckin(new string[] { _file.FullPath },
                UnchangedHandler.UndoCheckout, false, LocalCopyType.Leave, false);
        }

        /// <summary>
        /// Exclusively checks out the source control item.
        /// </summary>
        public void CheckOut()
        {
            if (Client.CheckOut(_file, 2, "") == null)
            {
                throw new InvalidOperationException("Could not check out file: " + _file.FullPath);
            }
            Client.Get(_file, false, MakeWritableType.MakeAllFilesWritable, 
                SetFileTimeType.Current, MergeType.OverwriteWorkingCopy, null);
        }

        /// <summary>
        /// Undoes the check out on a source control item.
        /// </summary>
        public void UndoCheckOut()
        {
            Client.UndoCheckOut(_file, LocalCopyType.Replace);
        }

        /// <summary>
        /// Gets whether the source control item is checked out to anyone.
        /// </summary>
        /// <returns><see langword="true"/> if someone has the source control item checked out;
        /// <see langword="false"/> if no one has the source control item checked out.</returns>
        public bool IsCheckedOut
        {
            get
            {
                return Client.IsCheckedOutByAnyone(_file);
            }
        }

        /// <summary>
        /// Gets whether the source control item is checked out to current user.
        /// </summary>
        /// <returns><see langword="true"/> if the current user has the source control item checked 
        /// out; <see langword="false"/> if the current user does not have the item checked out.
        /// </returns>
        public bool IsCheckedOutToMe
        {
            get
            {
                // TODO: For performance, this result could be cached and reused by 
                // IsCheckedOut and CheckIn
                return Client.IsCheckedOutByMeOnAnyMachine(_file);
            }
        }

        /// <summary>
        /// Gets the name(s) of the user(s) who have the source control item checked out.
        /// </summary>
        /// <returns>The name(s) of the user(s) who have the source control item checked out.
        /// </returns>
        public string UserWhoCheckedOut
        {
            get
            {
                return Client.GetCheckOuts(_file);
            }
        }

        #endregion
    }
}
