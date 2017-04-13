namespace DocumentAPI.Models
{
    /// <summary>
    /// per document processing status
    /// </summary>
    public class ProcessingStatus
    {
        /// <summary>
        /// error information, if Error.ErrorOccurred = true
        /// </summary>
        public ErrorInfo Error { get; set; }

        /// <summary>
        /// status (code value) of submitted document
        /// </summary>
        public DocumentProcessingStatus DocumentStatus { get; set; }

        /// <summary>
        /// text name of status of submitted document
        /// </summary>
        public string StatusText { get; set; }
    }

    /// <summary>
    /// enumeration, processing status of document
    /// </summary>
    public enum DocumentProcessingStatus
    {
        /// <summary>
        /// The document is being processed
        /// </summary>
        Processing = 1,

        /// <summary>
        /// the document has finished processing; results are available
        /// </summary>
        Done,

        /// <summary>
        /// The document has failed to process
        /// </summary>
        Failed,

        /// <summary>
        /// The enum value is not applicable - i.e. getting the processing status failed.
        /// In this case review the Error information.
        /// </summary>
        NotApplicable
    }
}
