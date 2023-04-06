using AlertManager.Interfaces;
using AlertManager.Models.AllDataClasses;
using Elasticsearch.Net;
using Extract.ErrorHandling;
using Nest;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Extract.ErrorsAndAlerts.ElasticDTOs;

namespace AlertManager.Services
{
    public class ElasticSearchService : IElasticSearchLayer
    {
        private const int PAGESIZE = 25;

        //credentials
        private readonly string? _elasticCloudId = ConfigurationManager.AppSettings["ElasticSearchCloudId"];
        private readonly string? _elasticKeyPath = ConfigurationManager.AppSettings["ElasticSearchAPIKey"];

        //elastic search index names
        private readonly Nest.Indices _elasticEventsIndex = ConfigurationManager.AppSettings["ElasticSearchEventsIndex"];
        private readonly Nest.Indices _elasticAlertsIndex = ConfigurationManager.AppSettings["ElasticSearchAlertsIndex"];
        private readonly Nest.Indices _elasticEnvInfoIndex = ConfigurationManager.AppSettings["ElasticSearchEnvironmentInformationIndex"];

        private readonly Nest.IndexName tempAlertIndex = ConfigurationManager.AppSettings["PopulatedAlertsTestIndex"];
        private readonly Nest.IndexName tempEnvironmentIndex = ConfigurationManager.AppSettings["PopulatedEnvironmentInformationTestIndex"];
        private readonly Nest.IndexName tempEventIndex = ConfigurationManager.AppSettings["PopulatedEventsTestIndex"];

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
            List<AlertsObject> alerts = new();

            if (page < 0)
            {
                var ex = new ExtractException("ELI54053", "Alert out of range");
                ex.AddDebugData("Page number being accessed", page);
                throw ex;
            }

            try
            {
                var responseAlerts = _elasticClient.Search<LoggingTargetAlert>(s => s
                    .Index(tempAlertIndex)
                    .From(PAGESIZE * page)
                    .Size(PAGESIZE)
                );

                if (responseAlerts.IsValid)
                {
                    foreach (var hit in responseAlerts.Hits)
                    {
                        alerts.Add(ConvertAlert(hit));
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
                ex.AddDebugData("alert list being accessed ", JsonConvert.SerializeObject(alerts));
                throw ex;
            }

            return alerts;
        }

        public AlertsObject GetAlertById(string alertId)
        {
            try 
            {
                var responseAlert = _elasticClient.Get<AlertsObject>(alertId, g => g.Index(tempAlertIndex));
       
                if (responseAlert.IsValid) 
                {
                    return responseAlert.Source;
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
        /// Gets a list of all logged alerts from a given source that do not have an attached resolution
        /// </summary>
        /// <param name="page">0 indexed page number to display</param>
        /// <returns>Collection of all unresolved Alerts from the logging source</returns>
        public IList<AlertsObject> GetUnresolvedAlerts(int page)
        {
            if (page < 0)
            {
                var ex = new ExtractException("ELI54104", "Page out of range");
                ex.AddDebugData("Page number being accessed", page);
                throw ex;
            }

            try
            {
                var responseAlerts = _elasticClient.Search<AlertsObject>(s => s
                    .Index(tempAlertIndex)
                    .From(PAGESIZE * page)
                    .Size(PAGESIZE)
                    .Query(q => q
                    .Bool(b => b
                        .MustNot(m => m
                        .Exists(c => c
                            .Field(p => p.Resolution))))));

                if (responseAlerts.IsValid)
                {
                    return responseAlerts.Documents.ToList();
                }
                else
                {
                    throw new ExtractException("ELI54105", "Unable to retrieve Alerts, issue with elastic search retrieval");
                }
            }
            catch (Exception e)
            {
                ExtractException ex = new("ELI54106", "Error with retrieving alerts: ", e);
                ex.AddDebugData("page being accessed ", page);
                throw ex;
            }
        }

        /// <inheritdoc/>
        public IList<ExceptionEvent> GetAllEvents(int page)
        {
            if (page < 0)
            {
                throw new ExtractException("ELI54060", "Issue with page number: " + nameof(page));
            }

            try
            {
                var response = _elasticClient.Search<ExceptionEvent>(s => s
                    .Index(tempEventIndex)
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

        /// <inheritdoc/>
        public int GetMaxAlertPages()
        {
            //TODO: When we introduce filtering into this code, consider combining GetMaxAlertPages and GetMaxEventPages
            //Also consider combining GetAllEvents and GetAllAlerts

            try
            {
                var response = _elasticClient.Count<AlertsObject>(s => s
                    .Index(tempAlertIndex));

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
                ExtractException ex = new("ELI54063", "Error retrieving Alert Count", e);
                throw ex;
            }
        }

        /// <inheritdoc/>
        public int GetMaxEventPages()
        {
            try
            {
                var response = _elasticClient.Count<ExceptionEvent>(s => s
                    .Index(tempEventIndex));

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
                ExtractException ex = new("ELI54064", "Error retrieving Alert Count", e);
                throw ex;
            }
        }

        private static AlertsObject ConvertAlert(IHit<LoggingTargetAlert> logAlert)
        {
            AlertsObject alert = new()
            {
                AlertId = logAlert.Id
            };
            try
            {
                if (logAlert.Source != null)
                {
                    alert.AlertName = logAlert.Source.name;
                    alert.Configuration = logAlert.Source.query;

                    string jsonHits = "[" + logAlert.Source.hits + "]";
                    List<LogIndexObject>? eventList = JsonConvert.DeserializeObject<List<LogIndexObject>>(jsonHits);

                    if (eventList == null)
                    {
                        throw new ExtractException("ELI54058", "Error converting events from Json to LogIndexObject, " +
                            "json Hits: " + jsonHits + "event list is null");
                    }

                    List<ExceptionEvent> associatedEvents = new();
                    foreach (LogIndexObject eventLog in eventList)
                    {
                        associatedEvents.Add(eventLog._source);
                    }
                    alert.AssociatedEvents = associatedEvents;
                }
            }
            catch (Exception e)
            {
                ExtractException ex = new("ELI54059", "Error deserializing alerts from elastic search", e);
                throw ex;
            }
            return alert;
        }

        /// <inheritdoc/>
        public List<EnvironmentInformation> TryGetInfoWithDataEntry(DateTime searchBackwardsFrom, string dataKeyName)
        {
            var response = _elasticClient.Search<EnvironmentInformation>(s => s
                .Index(tempEnvironmentIndex)
                //Need the most recent hit
                .Size(1)
                .Sort(ss => ss
                    .Descending(p => p.CollectionTime))
                .Query(q => q
                    .Bool(b => b
                        .Must(m => m
                            //must have dictionary with desired key
                            .Match(c => c
                                .Field(p => p.Data.ContainsKey(dataKeyName)))
                            //and be before DateTime parameter
                            &&
                            m.DateRange(c => c
                                .Field(p => p.CollectionTime)
                                .LessThanOrEquals(searchBackwardsFrom))))));

            List<EnvironmentInformation> toReturn = new();

            if (response.Hits.Count == 0)
                return toReturn;

            toReturn.Add(response.Hits.ElementAt(0).Source);
            return toReturn;
        }

        /// <inheritdoc/>
        public List<EnvironmentInformation> TryGetInfoWithContextType(DateTime searchBackwardsFrom, string contextType)
        {
            var response = _elasticClient.Search<EnvironmentInformation>(s => s
                .Index(tempEnvironmentIndex)
                //Need the most recent hit
                .Size(1)
                .Sort(ss => ss
                    .Descending(p => p.CollectionTime))
                .Query(q => q
                    .Bool(b => b
                        .Must(m => m
                            //must have matching context field
                            .Term(c => c
                                .Field(p => p.Context)
                                .Value(contextType))
                            //and be before DateTime parameter
                            && m.DateRange(c => c
                                .Field(p => p.CollectionTime)
                                .LessThanOrEquals(searchBackwardsFrom))))));

            List<EnvironmentInformation> toReturn = new();

            if (response.Hits.Count == 0)
                return toReturn;

            toReturn.Add(response.Hits.ElementAt(0).Source);
            return toReturn;
        }

        /// <inheritdoc/>
        public List<EnvironmentInformation> GetEnvInfoWithContextAndEntity(DateTime searchBackwardsFrom, string contextType, string entityName)
        {
            var envResponse = _elasticClient.Search<EnvironmentInformation>(s => s
                .Index(tempEnvironmentIndex)
                .Sort(ss => ss
                    .Descending(p => p.CollectionTime))
                .Size(0)
                .Query(q => q
                    .Bool(b => b
                        .Must(m => m
                            //Must be before specified date, and relatively recent to it
                            .DateRange(c => c
                                .Field(p => p.CollectionTime)
                                //Arbitrary window of 2 days
                                //.GreaterThanOrEquals(searchBackwardsFrom.AddDays(-2))
                                .LessThanOrEquals(searchBackwardsFrom))
                            //and have matching context field
                            && m.Match(m => m
                                .Field(p => p.Context)
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

            List<EnvironmentInformation> toReturn = new();
            var myMeasurementTypeAgg = envResponse.Aggregations.Terms("by_measurementType");

            foreach (var hit in myMeasurementTypeAgg.Buckets
                .SelectMany(b => b.TopHits("top_measurement_hits").Hits<EnvironmentInformation>()))
            { 
                toReturn.Add(hit.Source);
            }

            return toReturn;
        }

        /// <inheritdoc/>
        public List<ExceptionEvent> GetEventsInTimeframe(DateTime startTime, DateTime endTime)
        {
            var eventResponse = _elasticClient.Search<ExceptionEvent>(s => s
                .Index(tempEventIndex)
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

            List<ExceptionEvent> toReturn = new();

            foreach (var hit in eventResponse.Hits)
            { 
                toReturn.Add(hit.Source);
            }

            return toReturn;
        }

        /// <inheritdoc/>
        public List<ExceptionEvent> GetEventsByDictionaryKeyValuePair(string expectedKey, string expectedValue)
        {
            DictionaryEntry expectedDictEntry = new DictionaryEntry(expectedKey, expectedValue);

            var eventResponse = _elasticClient.Search<ExceptionEvent>(s => s
                .Index(tempEventIndex)
                .Sort(ss => ss
                    .Descending(p => p.ExceptionTime))
                //Default query size for elastic is always 10, change this to arbitrarily high value to get all matching docs
                .Size(10)
                .Query(q => q
                    .Bool(b => b
                        .Must(m => m
                            .Match(c => c
                                .Field(p => p.Data.Contains(expectedDictEntry)))))));

            List<ExceptionEvent> toReturn = new();

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
            var aggResponse = _elasticClient.Search<EnvironmentInformation>(s => s
                .Index(tempEnvironmentIndex)
                .Aggregations(a => a
                    .Terms("measurementAgg", t => t
                        .Field(p => p.MeasurementType)
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
