using System;
using System.Collections.Generic;
using System.Text;

namespace Extract.SourceControl
{
    /// <summary>
    /// Represents a source control history item.
    /// </summary>
    public interface IHistoryItem
    {
        /// <summary>
        /// Gets the comment associated with the history item.
        /// </summary>
        /// <returns>The comment associated with the history item.</returns>
        string Comment
        {
            get;
        }

        /// <summary>
        /// Gets the repository path of the history item after the change was made.
        /// </summary>
        /// <returns>The repository path of the history item after the change was made.</returns>
        string RepositoryPath
        {
            get;
        }

        /// <summary>
        /// Gets the repository path of the history item before the change was made.
        /// </summary>
        /// <returns>The repository path of the history item before the change was made.</returns>
        string FormerRepositoryPath
        {
            get;
        }

        /// <summary>
        /// Gets whether the history item has been deleted, moved, or renamed.
        /// </summary>
        /// <returns><see langword="true"/> if the history item was deleted, moved, or renamed;
        /// <see langword="false"/> if the local file was added or is still present under the same 
        /// name.</returns>
        bool LocalFileDeleted
        {
            get;
        }
    }
}
