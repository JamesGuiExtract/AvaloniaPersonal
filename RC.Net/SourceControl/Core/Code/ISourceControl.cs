using System;
using System.Collections.Generic;
using System.Text;

namespace Extract.SourceControl
{
    /// <summary>
    /// Represents a source control database
    /// </summary>
    public interface ISourceControl
    {
        /// <summary>
        /// Opens a connection to the specified repository.
        /// </summary>
        /// <param name="repository">If repository is <see langword="null"/> opens the last used 
        /// repository; otherwise opens the specified repository.</param>
        void Open(string repository);

        /// <summary>
        /// Gets the specified source control item.
        /// </summary>
        /// <param name="path">The repository path to the item.</param>
        /// <returns>The source control item at <paramref name="path"/>.</returns>
        ISourceControlItem GetItem(string path);

        /// <summary>
        /// Method for refreshing the source control connection
        /// </summary>
        void RefreshConnection();
    }
}
