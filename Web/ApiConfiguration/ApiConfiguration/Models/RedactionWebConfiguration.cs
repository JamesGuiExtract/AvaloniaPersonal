using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Extract.Web.ApiConfiguration.Models
{
    public sealed class RedactionWebConfiguration : IRedactionWebConfiguration, IDomainObject
    {
        /// <inheritdoc/>
        public Guid? ID { get; set; }

        /// <inheritdoc/>
        public string ConfigurationName { get; set; }

        /// <inheritdoc/>
        public bool IsDefault { get; }

        /// <inheritdoc/>
        public string WorkflowName { get; }

        /// <inheritdoc/>
        public string AttributeSet { get; }

        /// <inheritdoc/>
        public IList<string> ActiveDirectoryGroups { get; }

        /// <inheritdoc/>
        public string ProcessingAction { get; }

        /// <inheritdoc/>
        public string PostProcessingAction { get; }

        /// <inheritdoc/>
        public IList<string> RedactionTypes { get; }

        /// <inheritdoc/>
        public bool EnableAllUserPendingQueue { get; }

        /// <inheritdoc/>
        public string DocumentTypeFileLocation { get; }


        /// <summary>
        /// Create an instance with default values for all properties
        /// </summary>
        public RedactionWebConfiguration() { }

        /// <summary>
        /// Create an instance with values for all read-only properties
        /// </summary>
        public RedactionWebConfiguration(
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

        /// <summary>
        /// Structural equality
        /// </summary>
        public override bool Equals(object obj)
        {
            return obj is RedactionWebConfiguration configuration &&
                ID == configuration.ID &&
                ConfigurationName == configuration.ConfigurationName &&
                IsDefault == configuration.IsDefault &&
                WorkflowName == configuration.WorkflowName &&
                AttributeSet == configuration.AttributeSet &&
                (ActiveDirectoryGroups is null && configuration.ActiveDirectoryGroups is null
                    || ActiveDirectoryGroups is not null && configuration.ActiveDirectoryGroups is not null
                        && ActiveDirectoryGroups.SequenceEqual(configuration.ActiveDirectoryGroups)) &&
                ProcessingAction == configuration.ProcessingAction &&
                PostProcessingAction == configuration.PostProcessingAction &&
                (RedactionTypes is null && configuration.RedactionTypes is null
                    || RedactionTypes is not null && configuration.RedactionTypes is not null
                        && RedactionTypes.SequenceEqual(configuration.RedactionTypes)) &&
                EnableAllUserPendingQueue == configuration.EnableAllUserPendingQueue &&
                DocumentTypeFileLocation == configuration.DocumentTypeFileLocation;
        }

        /// <summary>
        /// Structural hash, doesn't include redaction types
        /// </summary>
        public override int GetHashCode()
        {
            return HashCode.Start
                .Hash(ID)
                .Hash(ConfigurationName)
                .Hash(IsDefault)
                .Hash(WorkflowName)
                .Hash(AttributeSet)
                .Hash(ProcessingAction)
                .Hash(PostProcessingAction)
                .Hash(EnableAllUserPendingQueue)
                .Hash(DocumentTypeFileLocation);
        }

        /// <inheritdoc/>
        public DataTransferObjectWithType CreateDataTransferObject()
        {
            return new(new Dto.RedactionWebConfigurationV1(
                configurationName: ConfigurationName,
                isDefault: IsDefault,
                workflowName: WorkflowName,
                activeDirectoryGroups: ActiveDirectoryGroups,
                processingAction: ProcessingAction,
                postProcessingAction: PostProcessingAction,
                attributeSet: AttributeSet,
                redactionTypes: RedactionTypes,
                enableAllUserPendingQueue: EnableAllUserPendingQueue,
                documentTypeFileLocation: DocumentTypeFileLocation));
        }
    }
}
