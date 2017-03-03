namespace DocumentAPI.Models
{
    /// <summary>
    /// Workflow data model
    /// </summary>
    public class Workflow
    {
        /// <summary>
        /// Name of the workflow
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// description of the workflow
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// the entry action for the workflow
        /// </summary>
        public string EntryAction { get; set; }

        /// <summary>
        /// The exit action for the workflow
        /// </summary>
        public string ExitAction { get; set; }

        /// <summary>
        /// the run-after-results action for the workflow
        /// </summary>
        public string RunAfterResultsAction { get; set; }

        /// <summary>
        /// The workflow document folder name
        /// </summary>
        public string DocumentFolder { get; set; }

        /// <summary>
        /// The workflow attribute set name
        /// </summary>
        public string AttributeSetName { get; set; }

        /// <summary>
        /// The Id of the Workflow, used to get by Id.
        /// </summary>
        public int Id { get; set; }
    }

    /// <summary>
    /// State of the workflow - running, stopped, or error
    /// </summary>
    public enum WorkflowState
    {
        /// <summary>
        /// running
        /// </summary>
        Running = 1,

        /// <summary>
        /// stopped
        /// </summary>
        Stopped,

        /// <summary>
        /// error
        /// </summary>
        Error
    }

    /// <summary>
    /// overall status information of a workflow
    /// </summary>
    public class WorkflowStatus
    {
        /// <summary>
        /// error information, when Error.ErrorOccurred == true
        /// </summary>
        public ErrorInfo Error { get; set; }

        /// <summary>
        /// number of documents processing
        /// </summary>
        public uint NumberProcessing { get; set; }

        /// <summary>
        /// number of documents done processing
        /// </summary>
        public uint NumberDone { get; set; }

        /// <summary>
        /// number of documents that have failed
        /// </summary>
        public uint NumberFailed { get; set; }

        /// <summary>
        /// number of documents that have been ignored
        /// </summary>
        public uint NumberIgnored { get; set; }

        /// <summary>
        /// the state of the specified workflow
        /// </summary>
        public string State { get; set; }
    }
}
