namespace Extract.Web.ApiConfiguration.Models
{
    public class WorkflowAction
    {
        public string WorkflowName { get; }
        public string ActionName { get; }
        public bool IsMainSequence { get; }

        public WorkflowAction(string workflowName, string actionName, bool isMainSequence)
        {
            WorkflowName = workflowName;
            ActionName = actionName;
            IsMainSequence = isMainSequence;
        }
    }
}
