namespace Extract.Web.Shared.Configuration
{
    public class APIWebConfiguration
    {
        /// <summary>
        /// Gets or sets the unique configuration name.
        /// </summary>
        public string ConfigurationName { get; set; }

        /// <summary>
        /// Gets or sets the workflow this configuration is assigned to.
        /// </summary>
        public string Workflow { get; set; }

        /// <summary>
        /// Gets or sets the document folder this configuration is assigned to.
        /// </summary>
        public string DocumentFolder { get; set; }

        /// <summary>
        /// Gets or sets the start action this configuration is assigned to.
        /// </summary>
        public string StartAction { get; set; }

        /// <summary>
        /// Gets or sets the update action this configuration is assigned to.
        /// </summary>
        public string UpdateAction { get; set; }

        /// <summary>
        /// Gets or sets the post update action this configuration is assigned to.
        /// </summary>
        public string PostUpdateAction { get; set; }

        /// <summary>
        /// Gets or sets the post update action this configuration is assigned to.
        /// </summary>
        public string EndAction { get; set; }

        /// <summary>
        /// Gets or sets the post workflow action this configuration is assigned to.
        /// </summary>
        public string PostWorkflowAction { get; set; }

        /// <summary>
        /// Gets or sets the attribute set this configuration is assigned to.
        /// </summary>
        public string AttributeSet { get; set; }

        /// <summary>
        /// Gets or sets the metadata field this configuration is assigned to.
        /// This is expected to be used for the file name.
        /// </summary>
        public string MetadataField { get; set; }

        /// <summary>
        /// Gets or sets the initial value function this configuration is assigned to.
        /// This can be a tag value.
        /// </summary>
        public string InitialValueFunction { get; set; }
    }
}
