using AlertManager.Models.AllDataClasses;
using Extract.ErrorHandling;
using System;
using System.Collections.Generic;

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

        /// <summary>
        /// Returns events that have an ExceptionTime within a specified timeframe.
        /// </summary>
        /// <param name="startTime">Time to start search window.</param>
        /// <param name="endTime">Time to end search window.</param>
        /// <returns>List of events within specified window.</returns>
        public List<ExceptionEvent> GetEventsInTimeframe(DateTime startTime, DateTime endTime);

        /// <summary>
        /// Gets a list of events that possess a given key-value pair in their Data dictionary.
        /// Events are sorted and returned by most recent.
        /// </summary>
        /// <param name="expectedKey"></param>
        /// <param name="expectedValue"></param>
        /// <returns>A list of recent events with the specified pair</returns>
        public List<ExceptionEvent> GetEventsByDictionaryKeyValuePair(string expectedKey, string expectedValue);

        /// <summary>
        /// Gets environment information for a specified context-entity pair.
        /// Gets most recent documents for each measurement type.
        /// </summary>
        /// <param name="searchBackwardsFrom">Date to begin search at, looking backwards. Usually the event CollectionTime</param>
        /// <param name="contextType">The context type of the entity</param>
        /// <param name="entityName">The name of the entity</param>
        /// <returns></returns>
        public List<EnvironmentInformation> GetEnvInfoWithContextAndEntity(DateTime searchBackwardsFrom, string contextType, string entityName);
    }
}
