using Extract.Web.ApiConfiguration.Models;
using Extract.Web.ApiConfiguration.Models.Dto;
using System;
using System.Collections.Generic;

namespace Extract.Web.ApiConfiguration.Services
{
    public interface IConfigurationDatabaseService
    {
        /// <summary>
        /// Get the cached <see cref="Workflow"/>s
        /// </summary>
        IList<Workflow> Workflows { get; }

        /// <summary>
        /// Get the cached <see cref="WorkflowAction"/>s
        /// </summary>
        IList<WorkflowAction> WorkflowActions { get; }

        /// <summary>
        /// Get the cached attribute set names
        /// </summary>
        IList<string> AttributeSetNames { get; }

        /// <summary>
        /// Get the cached <see cref="ICommonWebConfiguration"/>s
        /// </summary>
        IList<ICommonWebConfiguration> Configurations { get; }

        /// <summary>
        /// Get the cached metadata field names
        /// </summary>
        IList<string> MetadataFieldNames { get; }

        /// <summary>
        /// Contains a list of redaction web configurations.
        /// </summary>
        IList<IRedactionWebConfiguration> RedactionWebConfigurations { get; }

        /// <summary>
        /// Contains a list of document API web configurations.
        /// </summary>
        IList<IDocumentApiWebConfiguration> DocumentAPIWebConfigurations { get; }

        /// <summary>
        /// Retrieve the current values from the database and cache them
        /// </summary>
        void RefreshCache();
    }
}
