using System.Collections.Generic;

namespace Extract.Web.Shared.Configuration
{
    public class BackendWebConfiguration
    {
        /// <summary>
        /// Gets or sets the unique configuration name.
        /// </summary>
        public string ConfigurationName { get; set; }

        /// <summary>
        /// Gets or sets the active directory group this configuration is assigned to.
        /// (None, means it applys to everyone)
        /// </summary>
        public string ActiveDirectoryGroup { get; set; } = "None";

        /// <summary>
        /// Gets or sets the workflow this configuration is assigned to.
        /// </summary>
        public string Workflow { get; set; }

        /// <summary>
        /// Gets or sets the verify action this configuration is assigned to.
        /// </summary>
        public string VerifyAction { get; set; }

        /// <summary>
        /// Gets or sets the post verify action this configuration is assigned to.
        /// </summary>
        public string PostVerifyAction { get; set; }

        /// <summary>
        /// Gets or sets the attribute set this configuration is assigned to.
        /// </summary>
        public string AttributeSet { get; set; }

        /// <summary>
        /// Gets or sets the redaction types available for this configuration.
        /// </summary>
        public IList<string> RedactionTypes { get; set; }

        /// <summary>
        /// Gets or sets if this configuration supports the All queue.
        /// </summary>
        public bool EnableAllUserPendingQueue { get; set; }

        /// <summary>
        /// Gets or sets where the document types are stored.
        /// </summary>
        public string DocumentTypeFileLocation { get; set; }
    }
}
