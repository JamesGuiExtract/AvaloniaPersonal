namespace WebAPI
{
    /// <summary>
    /// The overall status information of a workflow
    /// </summary>
    public class WorkflowStatusResult
    {
        /// <summary>
        /// The number of documents processing
        /// </summary>
        public int NumberProcessing { get; set; }

        /// <summary>
        /// The number of documents done processing
        /// </summary>
        public int NumberDone { get; set; }

        /// <summary>
        /// The number of documents that have failed
        /// </summary>
        public int NumberFailed { get; set; }

        /// <summary>
        /// The number of document submitted but that are no longer progressing through the workflow.
        /// </summary>
        public int NumberIncomplete { get; set; }
    }
}
