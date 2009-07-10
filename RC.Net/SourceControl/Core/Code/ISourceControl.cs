using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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

        /// <summary>
        /// Gets the physical directory to which the repository root is bound.
        /// </summary>
        /// <returns>The physical directory to which the repository root is bound.</returns>
        // This accesses the source control server, so is better suited as a method.
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        string GetRootDirectory();

        /// <summary>
        /// Gets the changes made by the currently logged in user within the specified time range.
        /// </summary>
        /// <param name="startDate">The start date of the changes.</param>
        /// <param name="endDate">The end date of the changes.</param>
        /// <returns>The changes made by the currently logged in user after 
        /// <paramref name="startDate"/> and before <paramref name="endDate"/>.</returns>
        IEnumerable<IHistoryItem> GetUserHistoryItems(DateTime startDate, DateTime endDate);
    }
}
