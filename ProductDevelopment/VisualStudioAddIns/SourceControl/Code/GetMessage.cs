extern alias SC;

using SC::Extract.SourceControl;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace SourceControl
{
    /// <summary>
    /// Represents the GetMessage to send. 
    /// </summary>
    public static class GetMessage
    {
        #region GetMessage Constants

        /// <summary>
        /// History items that should not be included in any get messages.
        /// </summary>
        static readonly string[] _EXCLUDE_HISTORY_ITEMS =
        {
            // Note: It is important that these are alphabetized because a binary search is used.
            @"Engineering/ProductDevelopment/Common/ExtractMethodLocationIdentifiers.dat",
            @"Engineering/ProductDevelopment/Common/LatestComponentVersions.mak",
            @"Engineering/ProductDevelopment/Common/UCLIDExceptionLocationIdentifiers.dat",
        };

        #endregion GetMessage Constants

        #region GetMessage Methods

        /// <summary>
        /// Creates a get message compiled from all the checkins since the latest get message was 
        /// created.
        /// </summary>
        /// <returns>A get message compiled from all the checkins since the latest get message was 
        /// created.</returns>
        public static string GetSince(DateTime date)
        {
            // Get the root directory
            ISourceControl sourceControl = SourceControlFactory.Create(new LogOnSettings());
            string rootDirectory = sourceControl.GetRootDirectory();

            // Construct the get message from the history items checked out to the current user
            GetMessageBuilder builder = new GetMessageBuilder(rootDirectory);
            foreach (IHistoryItem item in sourceControl.GetUserHistoryItems(date, DateTime.Now))
            {
                if (!IsExcludedItem(item))
                {
                    builder.Add(item);
                }
            }

            return builder.ToString();
        }

        /// <summary>
        /// Determines whether this item should be excluded from the get message.
        /// </summary>
        /// <param name="item">The history item to check.</param>
        /// <returns><see langword="true"/> if the item should be excluded; <see langword="false"/>
        /// if the item should be included.</returns>
        static bool IsExcludedItem(IHistoryItem item)
        {
            // Perform a binary search for this item in the list of excluded items
            int index = Array.BinarySearch<string>(
                _EXCLUDE_HISTORY_ITEMS, item.RepositoryPath, StringComparer.OrdinalIgnoreCase);

            // Check if the item was found
            return index >= 0;
        }

        #endregion GetMessage Methods
    }
}
