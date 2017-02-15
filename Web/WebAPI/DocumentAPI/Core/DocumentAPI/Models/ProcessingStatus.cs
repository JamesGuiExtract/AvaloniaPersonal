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
        /// status of submitted document
        /// </summary>
        public DocumentProcessingStatus DocumentStatus { get; set; }
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
        Failed
    }
}
