using System;
using VaultClientIntegrationLib;
using VaultClientNetLib;
using VaultClientOperationsLib;
using VaultLib;

namespace Extract.SourceControl
{
    /// <summary>
    /// Represents a Vault source control file.
    /// </summary>
    public class VaultSourceControlItem : ISourceControlItem
    {
        #region Fields

        readonly VaultClientFile _file;
        string _path;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="VaultSourceControlItem"/> class.
        /// </summary>
        public VaultSourceControlItem(string path)
        {
            _file = RepositoryUtil.FindVaultFileAtReposOrLocalPath(path);
        }

        #endregion Constructors

        #region Properties

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

        #endregion Properties

        #region Methods

        /// <summary>
        /// Creates a <see cref="ChangeSetItemColl"/> for the <see cref="VaultSourceControlItem"/>.
        /// </summary>
        /// <returns>A <see cref="ChangeSetItemColl"/> for the 
        /// <see cref="VaultSourceControlItem"/>.</returns>
        ChangeSetItemColl GetChangeSet()
        {
            // Get the working folder for this file
            WorkingFolder workingFolder = Client.GetWorkingFolder(_file);

            // Create the change set item
            ChangeSetItem item = Client.MakeChangeSetItemForKnownChange(_file, workingFolder, false);

            // Add the change set item to a new collection
            ChangeSetItemColl changeSet = new ChangeSetItemColl(1);
            changeSet.Add(item);

            return changeSet;
        }

        #endregion Methods

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
            bool result = Client.Commit(GetChangeSet(), false, false);
            if (!result)
            {
                throw new InvalidOperationException("Commit failed: " + _file.FullPath);
            }
        }

        /// <summary>
        /// Exclusively checks out the source control item.
        /// </summary>
        public void CheckOut()
        {
            // Exclusively check out the file
            if (Client.CheckOut(_file, VaultCheckOutType.Exclusive, "") == null)
            {
                throw new InvalidOperationException("Could not check out file: " + _file.FullPath);
            }

            // Refresh the repository structure.
            // Note: This is important, because if the Client is not up to date, the Get will fail.
            Client.Refresh();

            // Get the latest version
            VaultGetResponse[] list = Client.Get(_file, false, MakeWritableType.MakeAllFilesWritable, 
                SetFileTimeType.Current, MergeType.OverwriteWorkingCopy, null);
            if (list != null && list.Length > 0)
            {
                foreach (VaultGetResponse response in list)
                {
                    int statusCode = response.Response.Status;
                    if (statusCode != VaultStatusCode.Success && statusCode != VaultStatusCode.SuccessRequireFileDownload)
                    {
                        // Throw an exception
                        string message = "Error getting " + response.File.FullPath + 
                            ": " + VaultConnection.GetSoapExceptionMessage(statusCode);

                        throw new InvalidOperationException(message);
                    }
                }
            }
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
