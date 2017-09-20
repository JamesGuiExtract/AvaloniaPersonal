namespace WebAPI.Models
{
    // embed this into top-level returned POCO classes
    /// <summary>
    /// standard error information for returned objects
    /// </summary>
    public class ErrorInfo
    {
        /// <summary>
        /// Boolean flag, true if an error has occurred
        /// </summary>
        public bool ErrorOccurred { get; set; }

        /// <summary>
        /// error message
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// error code - non-zero signals an error
        /// </summary>
        public int Code { get; set; }
    }
}
