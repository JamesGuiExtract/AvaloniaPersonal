using Extract.Utilities;
using System;

namespace Extract.Web.ApiConfiguration.Models
{
    public class DocumentApiConfiguration : IDocumentApiWebConfiguration, IDomainObject
    {
        /// <inheritdoc/>
        public Guid? ID { get; set; }

        /// <inheritdoc/>
        public string ConfigurationName { get; }

        /// <inheritdoc/>
        public bool IsDefault { get; }

        /// <inheritdoc/>
        public string WorkflowName { get; }

        /// <inheritdoc/>
        public string AttributeSet { get; }

        /// <inheritdoc/>
        public string ProcessingAction { get; }

        public string DocumentFolder { get; }
        public string StartWorkflowAction { get; }
        public string EndWorkflowAction { get; }
        public string PostWorkflowAction { get; }
        public string OutputFileNameMetadataField { get; }
        public string OutputFileNameMetadataInitialValueFunction { get; }
        public string PostProcessingAction { get; }

        /// <summary>
        /// Create an instance with values for all properties read-only
        /// </summary>
        public DocumentApiConfiguration(
            string configurationName,
            bool isDefault,
            string workflowName,
            string attributeSet,
            string processingAction,
            string postProcessingAction,
            string documentFolder,
            string startAction,
            string endAction,
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
            StartWorkflowAction = startAction;
            EndWorkflowAction = endAction;
            PostWorkflowAction = postWorkflowAction;
            OutputFileNameMetadataField = outputFileNameMetadataField;
            OutputFileNameMetadataInitialValueFunction = outputFileNameMetadataInitialValueFunction;
        }

        /// <summary>
        /// Structural equality
        /// </summary>
        public override bool Equals(object obj)
        {
            return obj is DocumentApiConfiguration configuration &&
                ID == configuration.ID &&
                ConfigurationName == configuration.ConfigurationName &&
                IsDefault == configuration.IsDefault &&
                WorkflowName == configuration.WorkflowName &&
                AttributeSet == configuration.AttributeSet &&
                ProcessingAction == configuration.ProcessingAction &&
                PostProcessingAction == configuration.PostProcessingAction &&
                DocumentFolder == configuration.DocumentFolder &&
                StartWorkflowAction == configuration.StartWorkflowAction &&
                EndWorkflowAction == configuration.EndWorkflowAction &&
                PostWorkflowAction == configuration.PostWorkflowAction &&
                OutputFileNameMetadataField == configuration.OutputFileNameMetadataField &&
                OutputFileNameMetadataInitialValueFunction == configuration.OutputFileNameMetadataInitialValueFunction;
        }

        /// <summary>
        /// Structural hash
        /// </summary>
        public override int GetHashCode()
        {
            return HashCode.Start
                .Hash(ID)
                .Hash(IsDefault)
                .Hash(ConfigurationName)
                .Hash(WorkflowName)
                .Hash(AttributeSet)
                .Hash(ProcessingAction)
                .Hash(PostProcessingAction)
                .Hash(DocumentFolder)
                .Hash(StartWorkflowAction)
                .Hash(EndWorkflowAction)
                .Hash(PostWorkflowAction)
                .Hash(OutputFileNameMetadataField)
                .Hash(OutputFileNameMetadataInitialValueFunction);
        }

        /// <inheritdoc/>
        public DataTransferObjectWithType CreateDataTransferObject()
        {
            return new(new Dto.DocumentApiWebConfigurationV1(
                configurationName: ConfigurationName,
                isDefault: IsDefault,
                workflowName: WorkflowName,
                attributeSet: AttributeSet,
                processingAction: ProcessingAction,
                postProcessingAction: PostProcessingAction,
                documentFolder: DocumentFolder,
                startWorkflowAction: StartWorkflowAction,
                endWorkflowAction: EndWorkflowAction,
                postWorkflowAction: PostWorkflowAction,
                outputFileNameMetadataField: OutputFileNameMetadataField,
                outputFileNameMetadataInitialValueFunction: OutputFileNameMetadataInitialValueFunction));
        }
    }
}
