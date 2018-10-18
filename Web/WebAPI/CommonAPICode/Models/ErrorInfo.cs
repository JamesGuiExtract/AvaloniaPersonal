using System.Collections.Generic;

namespace WebAPI.Models
{
    /// <summary>
    /// Describes the reason for a failed call.
    /// </summary>
    public class ErrorInfo
    {
        /// <summary>
        /// Description of the error
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Code associated with the error
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Codes providing further context for the error
        /// </summary>
        public List<string> AdditionalCodes { get; set; }

        /// <summary>
        /// Parameters pertinent to the error
        /// </summary>
        public List<string> ErrorDetails { get; set; }
    }
}
