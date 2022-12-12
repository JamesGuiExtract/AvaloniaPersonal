using Extract.Utilities;
using System;

namespace Extract.Web.ApiConfiguration.Models
{
    public interface ICommonWebConfiguration : IDomainObject
    {
        /// <summary>
        /// The unique ID for this configuration in the source database
        /// </summary>
        Guid? ID { get; set; }

        /// <summary>
        /// Name for the configuration (must be unique in the database)
        /// </summary>
        string ConfigurationName { get; }

        /// <summary>
        /// Whether this configuration is the default for the associated workflow
        /// </summary>
        bool IsDefault { get; }

        /// <summary>
        /// The name of the workflow this configuration belongs to
        /// </summary>
        string WorkflowName { get; }

        /// <summary>
        /// The attribute set that the document data (e.g., redactions) will be retrieved/stored from/to
        /// </summary>
        string AttributeSet { get; }

        /// <summary>
        /// The name of the action (DB queue) that files are retrieved from
        /// </summary>
        public string ProcessingAction { get; }

        /// <summary>
        /// The name of the action (DB queue) that files will be moved to after they have been processed by the web client
        /// </summary>
        public string PostProcessingAction { get; }
    }
}