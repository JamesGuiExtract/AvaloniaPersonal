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
        /// Get an observable for the cached <see cref="ICommonWebConfiguration"/>s
        /// with the ID property set and possibly two different versions of the name
        /// </summary>
        IObservable<IList<ConfigurationForEditing>> ConfigurationsForEditing { get; }

        /// <summary>
        /// Get an observable for the cached metadata field names
        /// </summary>
        IObservable<IList<string>> MetadataFieldNames { get; }

        /// <summary>
        /// Get an observable for the cached main-sequence workflow actions
        /// </summary>
        IObservable<IList<WorkflowAction>> MainSequenceWorkflowActions { get; }

        /// <summary>
        /// Get an observable for the cached non-main-sequence workflow actions
        /// </summary>
        IObservable<IList<WorkflowAction>> NonMainSequenceWorkflowActions { get; }

        /// <summary>
        /// Get an observable for the cached verification session timeout value (in minutes)
        /// </summary>
        IObservable<decimal> VerificationSessionTimeoutMinutes { get; }

        /// <summary>
        /// Retrieve the current values from the database and cache them
        /// </summary>
        void RefreshCache();

        /// <summary>
        /// Add or update the configuration record in the DB and refresh the cache
        /// </summary>
        void SaveConfiguration(ICommonWebConfiguration config);

        /// <summary>
        /// Remove the specified configuration record from the DB and refresh the cache
        /// </summary>
        void DeleteConfiguration(Guid configID);
    }
}
