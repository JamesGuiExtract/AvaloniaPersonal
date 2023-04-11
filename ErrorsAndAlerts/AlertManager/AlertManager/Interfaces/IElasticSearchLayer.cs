﻿using AlertManager.Models.AllDataClasses;
using Extract.ErrorHandling;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AlertManager.Interfaces
{
    public interface IElasticSearchLayer
    {
        /// <summary>
        /// Queries for an environment document in elastic search that has a given entry in its data dictionary.
        /// </summary>
        /// <param name="searchBackwardsFrom">Date and time of the alert or error. Query will find most recent document that is still before this time.</param>
        /// <param name="dataKeyName">Name of the entry to look for in the documents data dictionary.</param>
        /// <returns>List containing single best match EnvironmentInformation from query or empty list.</returns>
        List<EnvironmentInformation> TryGetInfoWithDataEntry(DateTime searchBackwardsFrom, string dataKeyName);

        /// <summary>
        /// Queries for an environment document in elastic search that has a given context type.
        /// </summary>
        /// <param name="searchBackwardsFrom">Date and time of the alert or error. Query will find most recent document that is still before this time.</param>
        /// <param name="contextType">Value for the context field of the desired document.</param>
        /// <returns>List containing single best match EnvironmentInformation from query or empty list.</returns>
        List<EnvironmentInformation> TryGetInfoWithContextType(DateTime searchBackwardsFrom, string contextType);

        /// <summary>
        /// Gets a list of all logged alerts from a given source
        /// </summary>
        /// <param name="page">0 indexed page number to display</param>
        /// <returns>Collection of all Alerts from the logging source</returns>
        IList<AlertsObject> GetAllAlerts(int page);

        /// <summary>
        /// Gets a list of all available exceptions from a given source
        /// </summary>
        /// <param name="page">0 indexed page number to display</param>
        /// <returns>Collection of all Exceptions from the logging source</returns>
        IList<ExceptionEvent> GetAllEvents(int page);

        /// <summary>
        /// Gets the maximum number of allowed pages based on a PAGESIZE constant
        /// </summary>
        /// <returns>Number of valid alert pages</returns>
        int GetMaxAlertPages();

        /// <summary>
        /// Gets the maximum number of allowed pages based on a PAGESIZE constant
        /// </summary>
        /// <returns>Number of valid event pages</returns>
        int GetMaxEventPages();

        void SetNewResolutionToElasticAlerts(AlertResolution resolution, string documentId);

        public List<EnvironmentInformation> GetEnvInfoWithContextAndEntity(DateTime searchBackwardsFrom, string contextType, string entityName);
    }
}