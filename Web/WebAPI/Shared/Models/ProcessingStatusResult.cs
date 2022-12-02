namespace WebAPI
{
    /// <summary>
    /// A result containing the processing status of a document.
    /// </summary>
    public class ProcessingStatusResult
    {
        /// <summary>
        /// Status code representing a document status
        /// </summary>
        public DocumentProcessingStatus DocumentStatus { get; set; }

        /// <summary>
        /// Textual name of a document status
        /// </summary>
        public string StatusText { get; set; }
    }

    /// <summary>
    /// Domain of potential document processing statuses
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
        /// The document was submitted for processing, but is no longer progressing in the workflow
        /// </summary>
        Incomplete,

        /// <summary>
        /// The enum value is not applicable - i.e. getting the processing status failed.
        /// In this case review the Error information.
        /// </summary>
        NotApplicable
    }
}
