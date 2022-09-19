using System.Collections.Generic;

namespace WebAPI.Models
{
    /// <summary>
    /// Data model for application settings.
    /// </summary>
    public class WebAppSettingsResult
    {
        /// <summary>
        /// The redaction types that should be available to select
        /// </summary>
        public IEnumerable<string> RedactionTypes;

        /// <summary>
        /// Whether all pending files (when <c>true</c>) or only the current user's files are available
        /// for verification
        /// </summary>
        public bool EnableAllPendingQueue = true;

        /// <summary>
        /// The number of minutes that should be allowed to pass without user interaction before
        /// the current session is automatically closed; zero if the session should never be
        /// automatically closed because of inactivity.
        /// </summary>
        public int InactivityTimeout = 5;

        /// <summary>
        /// A filepath to the document types.
        /// </summary>
        public string DocumentTypes;

        /// <summary>
        /// An array of all of the document types from the document types file.
        /// </summary>
        public string[] ParsedDocumentTypes;

        /// The complexity rules for user login passwords
        public Extract.Utilities.PasswordComplexityRequirements PasswordComplexityRequirements;
    }
}
