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
        /// The number of minutes that should be allowed to pass without user interaction before
        /// the current session is automatically closed; zero if the session should never be
        /// automatically closed because of inactivity.
        /// </summary>
        public int InactivityTimeout = 5;
    }
}
