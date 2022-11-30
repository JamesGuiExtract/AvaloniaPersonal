using Extract.Utilities;
using System.Collections.Generic;

namespace Extract.Web.ApiConfiguration.Models.Dto
{
    public sealed class RedactionWebConfigurationV1 : IDataTransferObject
    {
        public string ConfigurationName { get; }
        public bool IsDefault { get; set; }
        public string WorkflowName { get; }
        public IList<string> ActiveDirectoryGroups { get; }
        public string ProcessingAction { get; }
        public string PostProcessingAction { get; }
        public string AttributeSet { get; }
        public IList<string> RedactionTypes { get; }
        public bool EnableAllUserPendingQueue { get; }
        public string DocumentTypeFileLocation { get; }

        public RedactionWebConfigurationV1(
            string configurationName,
            bool isDefault,
            string workflowName,
            IList<string> activeDirectoryGroups,
            string processingAction,
            string postProcessingAction,
            string attributeSet,
            IList<string> redactionTypes,
            bool enableAllUserPendingQueue,
            string documentTypeFileLocation)
        {
            ConfigurationName = configurationName;
            IsDefault = isDefault;
            ActiveDirectoryGroups = activeDirectoryGroups;
            WorkflowName = workflowName;
            ProcessingAction = processingAction;
            PostProcessingAction = postProcessingAction;
            AttributeSet = attributeSet;
            RedactionTypes = redactionTypes;
            EnableAllUserPendingQueue = enableAllUserPendingQueue;
            DocumentTypeFileLocation = documentTypeFileLocation;
        }

        /// <inheritdoc/>
        public IDomainObject CreateDomainObject()
        {
            return new RedactionWebConfiguration(
                configurationName: ConfigurationName,
                isDefault: IsDefault,
                workflowName: WorkflowName,
                activeDirectoryGroups: ActiveDirectoryGroups,
                processingAction: ProcessingAction,
                postProcessingAction: PostProcessingAction,
                attributeSet: AttributeSet,
                redactionTypes: RedactionTypes,
                enableAllUserPendingQueue: EnableAllUserPendingQueue,
                documentTypeFileLocation: DocumentTypeFileLocation);
        }
    }
}
