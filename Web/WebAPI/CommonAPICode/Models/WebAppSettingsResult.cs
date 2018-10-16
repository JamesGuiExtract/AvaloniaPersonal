using System.Collections.Generic;

namespace WebAPI.Models
{
    /// <summary>
    /// Data model for application settings.
    /// </summary>
    public class WebAppSettingsResult
    {
        /// <summary>
        /// The redaction types that should be available to select.
        /// </summary>
        public IEnumerable<string> RedactionTypes;
    }
}
