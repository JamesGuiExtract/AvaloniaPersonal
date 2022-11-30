namespace Extract.Web.ApiConfiguration.Models
{
    public interface IDocumentApiWebConfiguration : ICommonWebConfiguration
    {
        string DocumentFolder { get; }
        string StartWorkflowAction { get; }
        string EndWorkflowAction { get; }
        string OutputFileNameMetadataInitialValueFunction { get; }
        string PostWorkflowAction { get; }
        string OutputFileNameMetadataField { get; }
    }
}
