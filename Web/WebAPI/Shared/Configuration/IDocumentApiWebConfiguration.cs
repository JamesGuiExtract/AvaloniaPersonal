namespace WebAPI.Configuration
{
    public interface IDocumentApiWebConfiguration : IWebConfiguration
    {
        public string DocumentFolder { get; set; }
        public string StartWorkflowAction { get; set; }
        public string EndWorkflowAction { get; set; }
        public string FileNameMetadataInitialValueFunction { get; set; }
        public string PostWorkflowAction { get; set; }
        public string OutputFileNameMetadataField { get; set; }
    }
}
