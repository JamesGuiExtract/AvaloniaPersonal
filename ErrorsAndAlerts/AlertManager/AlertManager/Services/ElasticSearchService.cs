using AlertManager.Interfaces;
using AlertManager.Models.AllDataClasses;
using AlertManager.Models.AllDataClasses.JSONObjects;
using Elasticsearch.Net;
using Extract.ErrorHandling;
using Extract.ErrorsAndAlerts.ElasticDTOs;
using Nest;
using Newtonsoft.Json;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace AlertManager.Services
{
    public class ElasticSearchService : IElasticSearchLayer
    {
        private const int PAGESIZE = 25;

        //ElasticSearch credentials
        private readonly string? _elasticCloudId = ConfigurationManager.AppSettings["ElasticSearchCloudId"];
        private readonly string? _elasticKeyPath = ConfigurationManager.AppSettings["ElasticSearchAPIKey"];

        //ElasticSearch indices names
        private readonly Nest.IndexName
            _elasticEventsIndex = ConfigurationManager.AppSettings["ElasticSearchEventsIndex"],
            _elasticAlertsIndex = ConfigurationManager.AppSettings["ElasticSearchAlertsIndex"],
            _elasticEnvInfoIndex = ConfigurationManager.AppSettings["ElasticSearchEnvironmentInformationIndex"];

        private readonly ElasticClient _elasticClient;

        public ElasticSearchService()
        {
            CheckPaths();
            _elasticClient = new(_elasticCloudId, new ApiKeyAuthenticationCredentials(_elasticKeyPath));
        }

        /// <summary>
        /// Checks the values retrieved from app settings and throws an error if any are null.
        /// </summary>
        private void CheckPaths()
        {
            try
            {
                if (_elasticKeyPath == null)
                {
                    throw new ExtractException("ELI54046", "Configuration for elastic search key path is invalid, path in " +
                        "configuration is: ConfigurationManager.AppSettings[\"ElasticSearchCloudId\"]");
                }
                else if (_elasticCloudId == null)
                {
                    throw new ExtractException("ELI54047", "Configuration for elastic search cloud id is invalid, path " +
                        "in configuration is: ConfigurationManager.AppSettings[\"ElasticSearchAPIKey\"]");
                }
                else if (_elasticAlertsIndex == null)
                {
                    throw new ExtractException("ELI54048", "Configuration for elastic search alerts is invalid, path " +
                        "in configuration is: ConfigurationManager.AppSettings[\"ElasticSearchAlertsIndex\"]");
                }
                else if (_elasticEventsIndex == null)
                {
                    throw new ExtractException("ELI54049", "Configuration for elastic search events is invalid, path " +
                        "in configuration is: ConfigurationManager.AppSettings[\"ElasticSearchExceptionIndex\"]");
                }
                else if (_elasticEnvInfoIndex == null)
                {
                    throw new ExtractException("ELI54051", "Configuration for elastic search environment information index is invalid, " +
                        "path in configuration is: ConfigurationManager.AppSettings[\"ElasticSearchEnvironmentInformationIndex\"]");
                }
            }
            catch (Exception e)
            {
                ExtractException ex = new("ELI54052", "Issue with null paths ", e);
                throw ex;
            }
        }

        /// <summary>
        /// Gets a list of all logged alerts from a given source
        /// </summary>
        /// <param name="page">0 indexed page number to display</param>
        /// <returns>Collection of all Alerts from the logging source</returns>
        public IList<AlertsObject> GetAllAlerts(int page)
        {
            if (page < 0)
            {
                var ex = new ExtractException("ELI54053", "Alert out of range");
                ex.AddDebugData("Page number being accessed", page);
                throw ex;
            }

            List<AlertsObject> alertsList = new();
            try
            {
                var responseAlerts = _elasticClient.Search<AlertDto>(s => s
                    .Index(_elasticAlertsIndex)
                    .From(PAGESIZE * page)
                    .Size(PAGESIZE)
                );

                if (responseAlerts.IsValid)
                {
                    for (int i = 0; i < responseAlerts.Hits.Count; i++)
                    {
                        AlertDto alertObject = responseAlerts.Documents.ElementAt(i);
                        alertsList.Add(ElasticAlertToLocalAlertObject(alertObject, responseAlerts.Hits.ElementAt(i).Id));
                    }
                }
                else
                {
                    throw new ExtractException("ELI54055", "Unable to retrieve Alerts, issue with elastic search retrieval");
                }

            }
            catch (Exception e)
            {
                ExtractException ex = new("ELI54057", "Error with retrieving alerts: ", e);
                ex.AddDebugData("page being accessed ", page);
                throw ex;
            }

            return alertsList;
        }

        public AlertsObject GetAlertById(string alertId)
        {
            try
            {
                var responseAlert = _elasticClient.Get<AlertDto>(alertId, g => g
                    .Index(_elasticAlertsIndex));

                if (responseAlert.IsValid && responseAlert.Found)
                {
                    return ElasticAlertToLocalAlertObject(responseAlert.Source, alertId);
                }
                else
                {
                    throw new ExtractException("ELI54108", "Issue with response alert");
                }
            }
            catch (Exception e)
            {
                ExtractException ex = new("ELI54107", "Error with retrieving alert: ", e);
                ex.AddDebugData("alert being accessed ", alertId);
                throw ex;
            }
        }



        /// <summary>
        /// Gets a list of all logged alerts from a given source that do not have an attached action
        /// </summary>
        /// <param name="page">0 indexed page number to display</param>
        /// <returns>Collection of all unresolved Alerts from the logging source</returns>
        public IList<AlertsObject> GetUnresolvedAlerts(int page)
        {/* Depreciated TODO resolve in Jira https://extract.atlassian.net/browse/ISSUE-19144
            }*/
            return new List<AlertsObject>();
        }


        /// <summary>
        /// Gets a list of all available events from a given source
        /// </summary>
        /// <param name="page">0 indexed page number to display</param>
        /// <returns>Collection of all Events from the logging source</returns>
        public IList<EventDto> GetAllEvents(int page)
        {
            if (page < 0)
            {
                throw new ExtractException("ELI54060", "Issue with page number: " + nameof(page));
            }

            try
            {
                var response = _elasticClient.Search<EventDto>(s => s
                    .Index(_elasticEventsIndex)
                    .From(PAGESIZE * page)
                    .Size(PAGESIZE));

                if (response.IsValid)
                {
                    return response.Documents.ToList();
                }
                else
                {
                    throw new ExtractException("ELI54061", "Issue at page number: " + nameof(page) + "Elastic Search Client is " + _elasticClient.ToString());
                }
            }
            catch (Exception e)
            {
                ExtractException ex = new("ELI54062", "Error retrieving Events", e);
                throw ex;
            }
        }

        /// <summary>
        /// Retrieves the maximum number of alert pages available in the ElasticSearch index.
        /// </summary>
        /// <returns>
        /// The maximum number of alert pages. Returns 1 if there are no alerts or in case of an error.
        /// </returns>
        /// <exception cref="ExtractException">
        /// Thrown when there is an error retrieving the alert count from the ElasticSearch index.
        /// </exception>
        public int GetMaxAlertPages()
        {
            try
            {
                var response = _elasticClient.Count<AlertDto>(s => s
                    .Index(_elasticAlertsIndex));

                if (response.IsValid)
                {
                    int toReturn = (int)(response.Count + PAGESIZE - 1) / PAGESIZE;
                    if (toReturn > 0)
                        return toReturn;
                }
                return 1;
            }
            catch (Exception e)
            {
                ExtractException ex = new("ELI54063", "Error retrieving Alert count", e);
                throw ex;
            }
        }

        /// <summary>
        /// Retrieves the maximum number of event pages available in the ElasticSearch index.
        /// </summary>
        /// <returns>
        /// The maximum number of event pages. Returns 1 if there are no events or in case of an error.
        /// </returns>
        /// <exception cref="ExtractException">
        /// Thrown when there is an error retrieving the event count from the ElasticSearch index.
        /// </exception>
        public int GetMaxEventPages()
        {
            try
            {
                var response = _elasticClient.Count<EventDto>(s => s
                    .Index(_elasticEventsIndex));

                if (response.IsValid)
                {
                    int toReturn = (int)(response.Count + PAGESIZE - 1) / PAGESIZE;
                    if (toReturn > 0)
                        return toReturn;
                }
                return 1;
            }
            catch (Exception e)
            {
                ExtractException ex = new("ELI54064", "Error retrieving Event count", e);
                throw ex;
            }
        }

        /// <summary>
        /// Converts an AlertObjectElastic instance retrieved from Elasticsearch into an AlertsObject instance used in the project.
        /// </summary>
        /// <param name="logAlert">An instance of AlertObjectElastic representing an alert from Elasticsearch.</param>
        /// <returns>An instance of AlertsObject converted from the given AlertObjectElastic.</returns>
        /// <exception cref="ExtractException">Thrown when there is an error deserializing alerts from Elasticsearch.</exception>
        private static AlertsObject ElasticAlertToLocalAlertObject(AlertDto logAlert, string alertId)
        {
            try
            {
                if (logAlert == null)
                {
                    throw new Exception("Issue with log alert");
                }

                if (logAlert.Hits == null)
                {
                    throw new Exception("no hits retrieved");
                }

                // TODO: Fix this. ToString probably won't give the right result...
                string jsonString = logAlert.Hits.ToString() ?? "";

                return JsonToAlertObject(logAlert, alertId, jsonString, logAlert.HitsType);
            }
            catch (Exception e)
            {
                ExtractException ex = new("ELI54059", "Error deserializing alerts from Elasticsearch", e);
                throw ex;
            }
        }

        private static AlertsObject JsonToAlertObject(AlertDto logAlert, string alertId, string jsonString, string type)
        {
            try
            {
                if (type == "")
                {
                    throw new Exception("issue getting type");
                }

                if (type == "event")
                {
                    List<EventFromJson>? eventFromJSON = JsonConvert.DeserializeObject<List<EventFromJson>>(jsonString);

                    if (eventFromJSON == null)
                    {
                        throw new Exception("Issue parsing json from elastic");
                    }

                    return new(
                        alertId,
                        logAlert.HitsType,
                        logAlert.AlertName,
                        logAlert.Configuration,
                        logAlert.ActivationTime,
                        ConvertJSONClassToEvent(eventFromJSON == null ? new() : eventFromJSON),
                        logAlert.Actions ?? new List<AlertActionDto>()
                    );
                }

                if (type == "environment")
                {
                    List<EnvironmentDto>? evironmentFromJSON = JsonConvert.DeserializeObject<List<EnvironmentDto>>(jsonString);

                    if (evironmentFromJSON == null)
                    {
                        throw new Exception("Issue parsing json from elastic");
                    }

                    return new(
                        alertId,
                        logAlert.HitsType,
                        logAlert.AlertName,
                        logAlert.Configuration,
                        logAlert.ActivationTime,
                        evironmentFromJSON == null ? new() : evironmentFromJSON,
                        logAlert.Actions ?? new List<AlertActionDto>()
                    );
                }

                throw new Exception("type not found");
            }
            catch (Exception e)
            {
                throw e.AsExtractException("ELI54269");
            }

        }

        /// <summary>
        /// Converts a list of EventFromJson objects to a list of EventDto objects.
        /// </summary>
        /// <param name="jsonClasses">The list of EventFromJson objects to convert.</param>
        /// <returns>A list of EventDto objects converted from the provided EventFromJson objects.</returns>
        /// <exception cref="ExtractException">
        /// Thrown when there is an error during the conversion process.
        /// </exception>
        private static List<EventDto> ConvertJSONClassToEvent(List<EventFromJson> jsonClasses)
        {
            List<EventDto> events = new();
            try
            {
                foreach (EventFromJson evt in jsonClasses)
                {
                    EventDto newEvent = new();

                    newEvent.Id = evt.Id;
                    //newEvent.StackTrace = evt.Score;
                    newEvent.EliCode = evt.Source.EliCode ?? "";
                    newEvent.Message = evt.Source.Message ?? "";
                    newEvent.ExceptionTime = evt.Source.ExceptionTime;


                    //If desired, could probabally make context json friendly but that would modify another class
                    newEvent.Context.ApplicationName = evt.Source.Context.ApplicationName ?? "";
                    newEvent.Context.ApplicationVersion = evt.Source.Context.ApplicationVersion ?? "";
                    newEvent.Context.MachineName = evt.Source.Context.MachineName ?? "";
                    newEvent.Context.UserName = evt.Source.Context.UserName ?? "";
                    newEvent.Context.PID = (int)evt.Source.Context.PID; //uint to int, potential for overflow?
                    newEvent.Context.FileID = evt.Source.Context.FileID;
                    newEvent.Context.ActionID = evt.Source.Context.ActionID;
                    newEvent.Context.DatabaseServer = evt.Source.Context.DatabaseServer ?? "";
                    newEvent.Context.DatabaseName = evt.Source.Context.DatabaseName ?? "";
                    newEvent.Context.FpsContext = evt.Source.Context.FpsContext ?? "";

                    events.Add(newEvent);
                }
            }
            catch (Exception e)
            {
                throw e.AsExtractException("ELI54215");
            }
            return events;
        }

        /// <summary>
        /// Converts an EventDto object to a ContextInfo object.
        /// </summary>
        /// <param name="eventDto">The EventDto object to convert.</param>
        /// <returns>A ContextInfo object converted from the provided EventDto object.</returns>
        public static ContextInfo ConvertEventDtoToContextInfo(EventDto eventDto)
        {
            try
            {

                var contextInfo = new ContextInfo();

                contextInfo.ApplicationName = eventDto.Context.ApplicationName;
                contextInfo.ApplicationVersion = eventDto.Context.ApplicationVersion;
                contextInfo.MachineName = eventDto.Context.MachineName;
                contextInfo.UserName = eventDto.Context.UserName;
                contextInfo.PID = (UInt32)eventDto.Context.PID;

                contextInfo.FileID = eventDto.Context.FileID;
                contextInfo.ActionID = eventDto.Context.ActionID;
                contextInfo.DatabaseServer = eventDto.Context.DatabaseServer;
                contextInfo.DatabaseName = eventDto.Context.DatabaseName;
                contextInfo.FpsContext = eventDto.Context.FpsContext;

                return contextInfo;
            }
            catch (Exception e)
            {
                RxApp.DefaultExceptionHandler.OnNext(e.AsExtractException("ELI54196"));
            }

            return new();
        }

        /// <summary>
        /// Sets a new action for an alert in the ElasticSearch index.
        /// </summary>
        /// <param name="action">Action to add to the alert's actions list.</param>
        /// <param name="documentId">The document ID of the alert in the ElasticSearch index.</param>
        public async void AddAlertAction(AlertActionDto action, string documentId)
        {
            try
            {
                // Use the ElasticSearch client to update the document with the given documentId in the specified index
                var updateResponse = await _elasticClient.UpdateByQueryAsync<EventDto>(u => u
                    // Set the index name to the ElasticSearchAlertsIndex value from the app configuration file
                    .Index(ConfigurationManager.AppSettings["ElasticSearchAlertsIndex"])
                    // Set the query to find the document with the given documentId
                    .Query(q => q
                        .Term(t => t
                            .Field("_id")
                            .Value(documentId)
                        )
                    )
                    // Set the script to add the new AlertActionDto object to the "actions" array property in the document
                    .Script(s => s
                        .Source("if(ctx._source.actions == null) { ctx._source.actions = []; } ctx._source.actions.add(params.newAction)")
                        .Params(p => p
                            .Add("newAction", action)
                        )
                    )
                );

                // If the update response is not valid, throw an exception with a message indicating the issue
                if (!updateResponse.IsValid)
                {
                    throw new Exception("Issue updating the document" + updateResponse.DebugInformation);
                }
            }
            catch (Exception e)
            {
                // If an exception is thrown during execution, log the exception using the RxApp.DefaultExceptionHandler.OnNext() method
                // with the specified error code "ELI54199"
                RxApp.DefaultExceptionHandler.OnNext(e.AsExtractException("ELI54199"));
            }
        }

        /// <summary>
        /// Queries for an environment document in elastic search that has a given entry in its data dictionary.
        /// </summary>
        /// <param name="searchBackwardsFrom">Date and time of the alert or error. Query will find most recent document that is still before this time.</param>
        /// <param name="dataKeyName">Name of the entry to look for in the documents data dictionary.</param>
        /// <returns>List containing single best match EnvironmentDto from query or empty list.</returns>
        public List<EnvironmentDto> TryGetEnvInfoWithDataEntry(DateTime searchBackwardsFrom, string dataKey, string dataValue)
        {
            KeyValuePair<string, string> targetPair = new(dataKey, dataValue);

            var response = _elasticClient.Search<EnvironmentDto>(s => s
                .Index(_elasticEnvInfoIndex)
                //Need the most recent hit
                .Size(1)
                .Sort(ss => ss
                    .Descending(p => p.CollectionTime))
                .Query(q => q
                    .Bool(b => b
                        .Must(m => m
                            //must have dictionary with desired key
                            .Match(c => c
                                .Field(p => p.Data.Contains(targetPair)))
                            //and be before DateTime parameter
                            &&
                            m.DateRange(c => c
                                .Field(p => p.CollectionTime)
                                .LessThanOrEquals(searchBackwardsFrom))))));

            List<EnvironmentDto> toReturn = new();

            if (response.Hits.Count == 0)
                return toReturn;

            toReturn.Add(response.Hits.ElementAt(0).Source);
            return toReturn;
        }

        /// <summary>
        /// Attempts to retrieve the most recent EnvironmentDto object with the specified context type and before the given date.
        /// </summary>
        /// <param name="searchBackwardsFrom">The date up to which the method should search for records.</param>
        /// <param name="contextType">The context type to filter the records by.</param>
        /// <returns>A list containing the most recent EnvironmentDto object with the specified context type, or an empty list if no matching record is found.</returns>
        public List<EnvironmentDto> TryGetEnvInfoWithContextType(DateTime searchBackwardsFrom, string contextType)
        {
            var response = _elasticClient.Search<EnvironmentDto>(s => s
                .Index(_elasticEnvInfoIndex)
                //Need the most recent hit
                .Size(1)
                .Sort(ss => ss
                    .Descending(p => p.CollectionTime))
                .Query(q => q
                    .Bool(b => b
                        .Must(m => m
                            //must have matching context field
                            .Term(c => c
                                .Field(p => p.ContextType)
                                .Value(contextType))
                            //and be before DateTime parameter
                            && m.DateRange(c => c
                                .Field(p => p.CollectionTime)
                                .LessThanOrEquals(searchBackwardsFrom))))));

            List<EnvironmentDto> toReturn = new();

            if (response.Hits.Count == 0)
                return toReturn;

            toReturn.Add(response.Hits.ElementAt(0).Source);
            return toReturn;
        }

        /// <summary>
        /// Retrieves a list of EnvironmentDto objects with the specified context and entity values, sorted by CollectionTime.
        /// The method searches for records within a 2-day range before the provided date.
        /// </summary>
        /// <param name="searchBackwardsFrom">The date up to which the method should search for records.</param>
        /// <param name="contextType">The context type to filter the records by.</param>
        /// <param name="entityName">The entity name to filter the records by.</param>
        /// <returns>A list of EnvironmentDto objects that match the specified context and entity values.</returns>
        public List<EnvironmentDto> GetEnvInfoWithContextAndEntity(DateTime searchBackwardsFrom, string contextType, string entityName)
        {
            var envResponse = _elasticClient.Search<EnvironmentDto>(s => s
                .Index(_elasticEnvInfoIndex)
                .Sort(ss => ss
                    .Descending(p => p.CollectionTime))
                .Size(0)
                .Query(q => q
                    .Bool(b => b
                        .Must(m => m
                            //Must be before specified date, and relatively recent to it
                            .DateRange(c => c
                                .Field(p => p.CollectionTime)
                                .GreaterThanOrEquals(searchBackwardsFrom.AddDays(-2))
                                .LessThanOrEquals(searchBackwardsFrom))
                            //and have matching context field
                            && m.Match(m => m
                                .Field(p => p.ContextType)
                                .Query(contextType))
                            //and matching entity field
                            && m.Match(c => c
                                .Field(p => p.Entity)
                                .Query(entityName)))))
                .Aggregations(a => a
                    .Terms("by_measurementType", t => t
                        .Size(25)
                        .Field(p => p.MeasurementType)
                        .Aggregations(aa => aa
                            .TopHits("top_measurement_hits", th => th
                                .Size(1)
                                .Sort(ss => ss
                                    .Descending(p => p.CollectionTime))
                                .Source(src => src
                                    .IncludeAll()))))));

            List<EnvironmentDto> toReturn = new();
            var myMeasurementTypeAgg = envResponse.Aggregations.Terms("by_measurementType");

            foreach (var hit in myMeasurementTypeAgg.Buckets
                .SelectMany(b => b.TopHits("top_measurement_hits").Hits<EnvironmentDto>()))
            {
                toReturn.Add(hit.Source);
            }

            return toReturn;
        }

        /// <summary>
        /// Retrieves a list of EventDto objects from the index that occurred within the specified timeframe.
        /// </summary>
        /// <param name="startTime">The start of the timeframe to search for events.</param>
        /// <param name="endTime">The end of the timeframe to search for events.</param>
        /// <returns>A List of EventDto objects that occurred within the specified timeframe.</returns>
        public List<EventDto> GetEventsInTimeframe(DateTime startTime, DateTime endTime)
        {
            if (startTime > endTime)
            {
                throw new ExtractException("ELI54226", "Starting search time is later than ending search time.");
            }

            var eventResponse = _elasticClient.Search<EventDto>(s => s
                .Index(_elasticEventsIndex)
                .Sort(ss => ss
                    .Descending(p => p.ExceptionTime))
                //Default query size is always 10, change this to arbritrarily high value to get all matching docs
                .Size(10)
                .Query(q => q
                    .Bool(b => b
                        .Must(m => m
                            //Must be before specified date, and relatively recent to it
                            .DateRange(c => c
                                .Field(p => p.ExceptionTime)
                                .GreaterThanOrEquals(startTime)
                                .LessThanOrEquals(endTime))))));

            List<EventDto> toReturn = new();

            foreach (var hit in eventResponse.Hits)
            {
                toReturn.Add(hit.Source);
            }

            return toReturn;
        }

        /// /// <summary>
        /// Retrieves a list of EventDto objects from the index that match the specified key-value pair in their Data field.
        /// </summary>
        /// <param name="expectedKey">The key to search for in the Data field of each document.</param>
        /// <param name="expectedValue">The value to search for in the Data field of each document.</param>
        /// <returns>A List of EventDto objects that contain the specified key-value pair in their Data field.</returns>
        public List<EventDto> GetEventsByDictionaryKeyValuePair(string expectedKey, string expectedValue)
        {
            KeyValuePair<string, string> expectedEntry = new(expectedKey, expectedValue);

            var eventResponse = _elasticClient.Search<EventDto>(s => s
                .Index(_elasticEventsIndex)
                .Sort(ss => ss
                    .Descending(p => p.ExceptionTime))
                //Default query size for elastic is always 10, change this to arbitrarily high value to get all matching docs
                .Size(10)
                .Query(q => q
                    .Bool(b => b
                        .Must(m => m
                            .Match(c => c
                                .Field(p => p.Data.Contains(expectedEntry)))))));

            List<EventDto> toReturn = new();

            foreach (var hit in eventResponse.Hits)
            {
                toReturn.Add(hit.Source);
            }

            return toReturn;
        }

        /// <summary>
        /// Returns a list of all unique values for the MeasurementType field in the environment index
        /// </summary>
        /// <returns>A list containing each unique value found</returns>
        private List<string> GetEnvMeasurementTypes()
        {
            var aggResponse = _elasticClient.Search<EnvironmentDto>(s => s
                .Index(_elasticEnvInfoIndex)
                .Aggregations(a => a
                    .Terms("measurementAgg", t => t
                        .Field(p => p.MeasurementType)//Arbitrary window of 2 days
                        )));

            var measurementAgg = aggResponse.Aggregations.Terms("measurementAgg");

            List<string> measurementTypes = new List<string>();

            foreach (var bucket in measurementAgg.Buckets)
            {
                measurementTypes.Add(bucket.Key);
            }

            return measurementTypes;
        }

    }
}

