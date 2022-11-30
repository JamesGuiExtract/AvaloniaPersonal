using Extract.Utilities;

namespace Extract.Web.ApiConfiguration.Models.Dto
{
    public class DocumentApiWebConfigurationV1 : IDataTransferObject
    {
        public string ConfigurationName { get; set; }
        public bool IsDefault { get; set; }
        public string WorkflowName { get; set; }
        public string AttributeSet { get; set; }
        public string ProcessingAction { get; set; }
        public string PostProcessingAction { get; }
        public string DocumentFolder { get; set; }
        public string StartWorkflowAction { get; set; }
        public string EndWorkflowAction { get; set; }
        public string PostWorkflowAction { get; set; }
        public string OutputFileNameMetadataField { get; set; }
        public string OutputFileNameMetadataInitialValueFunction { get; set; }

        public DocumentApiWebConfigurationV1(
            string configurationName,
            bool isDefault,
            string workflowName,
            string attributeSet,
            string processingAction,
            string postProcessingAction,
            string documentFolder,
            string startWorkflowAction,
            string endWorkflowAction,
            string postWorkflowAction,
            string outputFileNameMetadataField,
            string outputFileNameMetadataInitialValueFunction)
        {
            ConfigurationName = configurationName;
            IsDefault = isDefault;
            WorkflowName = workflowName;
            AttributeSet = attributeSet;
            ProcessingAction = processingAction;
            PostProcessingAction = postProcessingAction;
            DocumentFolder = documentFolder;
            StartWorkflowAction = startWorkflowAction;
            EndWorkflowAction = endWorkflowAction;
            PostWorkflowAction = postWorkflowAction;
            OutputFileNameMetadataField = outputFileNameMetadataField;
            OutputFileNameMetadataInitialValueFunction = outputFileNameMetadataInitialValueFunction;
        }

        /// <inheritdoc/>
        public IDomainObject CreateDomainObject()
        {
            return new DocumentApiConfiguration(
                configurationName: ConfigurationName,
                isDefault: IsDefault,
                workflowName: WorkflowName,
                attributeSet: AttributeSet,
                processingAction: ProcessingAction,
                postProcessingAction: PostProcessingAction,
                documentFolder: DocumentFolder,
                startAction: StartWorkflowAction,
                endAction: EndWorkflowAction,
                postWorkflowAction: PostWorkflowAction,
                outputFileNameMetadataField: OutputFileNameMetadataField,
                outputFileNameMetadataInitialValueFunction: OutputFileNameMetadataInitialValueFunction);
        }
    }
}
