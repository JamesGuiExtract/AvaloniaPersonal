using System.Collections.Generic;

namespace Extract.Web.ApiConfiguration.Models
{
    public interface IRedactionWebConfiguration : ICommonWebConfiguration
    {
        /// <summary>
        /// The active directory group(s) this configuration is assigned to.
        /// </summary>
        IList<string> ActiveDirectoryGroups { get; }

        /// <summary>
        /// The redaction types available for this configuration
        /// </summary>
        IList<string> RedactionTypes { get; }

        /// <summary>
        /// Whether users are allowed to verify files queued for other users
        /// </summary>
        bool EnableAllUserPendingQueue { get; }

        /// <summary>
        /// Path to the document type file
        /// </summary>
        string DocumentTypeFileLocation { get; }

        /// <summary>
        /// Whether to show users the most-recently added/updated comment instead of
        /// restricting to the comment saved for the ProcessingAction
        /// </summary>
        bool ReturnLatestFileActionComment { get; }
    }
}
