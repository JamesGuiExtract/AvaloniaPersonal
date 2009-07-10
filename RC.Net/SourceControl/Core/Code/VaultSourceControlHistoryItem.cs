using System;
using System.Collections.Generic;
using System.Text;
using VaultLib;

namespace Extract.SourceControl
{
    /// <summary>
    /// Represents a Vault source control history item.
    /// </summary>
    [CLSCompliant(false)]
    public class VaultSourceControlHistoryItem : IHistoryItem
    {
        #region VaultSourceControlHistoryItem Fields

        readonly VaultHistoryItem _item;

        #endregion VaultSourceControlHistoryItem Fields

        #region VaultSourceControlHistoryItem Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="VaultSourceControlHistoryItem"/> class.
        /// </summary>
        public VaultSourceControlHistoryItem(VaultHistoryItem item)
        {
            _item = item;
        }

        #endregion VaultSourceControlHistoryItem Constructors

        #region VaultSourceControlHistoryItem Properties

        /// <summary>
        /// Gets whether the history item is a delete operation.
        /// </summary>
        /// <returns><see langword="true"/> if the history item is a delete;
        /// <see langword="false"/> if the history item is a type other than delete.</returns>
        public bool IsDelete
        {
            get
            {
                return _item.HistItemType == VaultHistoryType.Deleted;
            }
        }

        /// <summary>
        /// Gets whether history item is a move operation.
        /// </summary>
        /// <returns><see langword="true"/> if the history item is a move;
        /// <see langword="false"/> if the history item is a type other than move.</returns>
        public bool IsMoveFrom
        {
            get
            {
                return _item.HistItemType == VaultHistoryType.MovedFrom;
            }
        }

        /// <summary>
        /// Gets whether the history item is a move to operation.
        /// </summary>
        /// <returns><see langword="true"/> if the history item is a move to;
        /// <see langword="false"/> if the history item is a type other than move to.</returns>
        public bool IsMoveTo
        {
            get
            {
                return _item.HistItemType == VaultHistoryType.MovedTo;
            }
        }

        /// <summary>
        /// Gets whether the history item is a rename operation.
        /// </summary>
        /// <returns><see langword="true"/> if the history item is a rename;
        /// <see langword="false"/> if the history item is a type other than rename.</returns>
        public bool IsRename
        {
            get
            {
                return _item.HistItemType == VaultHistoryType.Renamed || 
                    _item.HistItemType == VaultHistoryType.RenamedItem;
            }
        }

        #endregion VaultSourceControlHistoryItem Properties

        #region VaultSourceControlHistoryItem Methods

        static string GetDirectoryName(string path)
        {
            int index = path.LastIndexOf("/", StringComparison.Ordinal);
            if (index < 0)
            {
                return "";
            }
            else
            {
                return path.Substring(0, index);
            }
        }

        #endregion VaultSourceControlHistoryItem Methods

        #region IHistoryItem Members

        /// <summary>
        /// Gets the comment associated with the history item.
        /// </summary>
        /// <returns>The comment associated with the history item.</returns>
        public string Comment
        {
            get
            {
                return _item.Comment;
            }
        }

        /// <summary>
        /// Gets the repository path of the history item after the change was made.
        /// </summary>
        /// <returns>The repository path of the history item after the change was made.</returns>
        public string RepositoryPath
        {
            get
            {
                // Some types should be handled specially
                if (IsMoveTo)
                {
                    // Drop the "$/" from the front.
                    return _item.MiscInfo2.Substring(2);
                }
                else if (IsMoveFrom)
                {
                    return _item.Name + "/" + _item.MiscInfo1;
                }
                else if (IsDelete)
                {
                    // Deletes no longer exist in the repository
                    return "";
                }

                return _item.Name;
            }
        }

        /// <summary>
        /// Gets the repository path of the history item before the change was made.
        /// </summary>
        /// <returns>The repository path of the history item before the change was made.</returns>
        public string FormerRepositoryPath
        {
            get
            {
                if (IsMoveFrom)
                {
                    // Drop the "$/" from the front.
                    return _item.MiscInfo2.Substring(2);
                }
                else if (IsMoveTo || IsDelete)
                {
                    return _item.Name + "/" + _item.MiscInfo1;
                }
                else if (IsRename)
                {
                    string directory = GetDirectoryName(_item.Name);
                    return directory + "/" + _item.MiscInfo2;
                }

                return _item.Name;
            }
        }

        /// <summary>
        /// Gets whether the history item has been deleted, moved, or renamed.
        /// </summary>
        /// <returns><see langword="true"/> if the history item was deleted, moved, or renamed;
        /// <see langword="false"/> if the local file was added or is still present under the same 
        /// name.</returns>
        public bool LocalFileDeleted
        {
            get
            {
                return IsDelete || IsMoveFrom || IsMoveTo || IsRename;
            }
        }

        #endregion
    }
}
