namespace Extract.Web.ApiConfiguration.Models
{
    public class WorkflowAction
    {
        public string WorkflowName { get; }
        public string ActionName { get; }

        public WorkflowAction(string workflowName, string actionName)
        {
            WorkflowName = workflowName;
            ActionName = actionName;
        }
    }
}
