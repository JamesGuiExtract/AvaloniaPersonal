using Extract.Web.ApiConfiguration.Models;
using System;
using System.Collections.Generic;

namespace Extract.Web.ApiConfiguration.Services
{
    public interface IObservableConfigurationDatabaseService
    {
        /// <summary>
        /// Get an observable for the cached <see cref="Workflow"/>s
        /// </summary>
        IObservable<IList<Workflow>> Workflows { get; }

        /// <summary>
        /// Get an observable for the cached <see cref="WorkflowAction"/>s
        /// </summary>
        IObservable<IList<WorkflowAction>> WorkflowActions { get; }

        /// <summary>
        /// Get an observable for the cached attribute set names
        /// </summary>
        IObservable<IList<string>> AttributeSetNames { get; }

        /// <summary>
        /// Get an observable for the cached <see cref="ICommonWebConfiguration"/>s
        /// </summary>
        IObservable<IList<ICommonWebConfiguration>> Configurations { get; }

        /// <summary>
        /// Get an observable for the cached metadata field names
        /// </summary>
        IObservable<IList<string>> MetadataFieldNames { get; }

        /// <summary>
        /// Retrieve the current values from the database and cache them
        /// </summary>
        void RefreshCache();
    }
}
