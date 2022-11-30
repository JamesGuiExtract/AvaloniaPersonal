namespace Extract.Web.ApiConfiguration.Models
{
    public class Workflow
    {
        public string WorkflowName { get; }
        public int WorkflowID { get; }

        public Workflow(string workflowName, int workflowID)
        {
            WorkflowName = workflowName;
            WorkflowID = workflowID;
        }
    }
}
