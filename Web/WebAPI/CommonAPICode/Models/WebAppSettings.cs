using System.Collections.Generic;

namespace WebAPI.Models
{
    /// <summary>
    /// Data model for application settings.
    /// </summary>
    public class WebAppSettings : IResultData
    {
        /// <summary>
        /// The redaction types that should be available to select.
        /// </summary>
        public IEnumerable<string> RedactionTypes;

        /// <summary>
        /// Error info - Error == true if there has been an error
        /// </summary>
        public ErrorInfo Error { get; set; }
    }
}
