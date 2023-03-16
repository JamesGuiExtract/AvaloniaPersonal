using AlertManager.Interfaces;
using AlertManager.Models.AllDataClasses;
using Extract.ErrorHandling;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Elasticsearch.Net;
using Nest;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace AlertManager.Services
{
    public class ElasticSearchService : IElasticSearchLayer
    {
        private const int PAGESIZE = 25;

        //credentials
        private readonly string? _elasticCloudId = ConfigurationManager.AppSettings["ElasticSearchCloudId"];
        private readonly string? _elasticKeyPath = ConfigurationManager.AppSettings["ElasticSearchAPIKey"];

        //elastic search index names
        private readonly Nest.Indices _elasticEventsIndex = ConfigurationManager.AppSettings["ElasticSearchExceptionIndex"];
        private readonly Nest.Indices _elasticAlertsIndex = ConfigurationManager.AppSettings["ElasticSearchAlertsIndex"];
        private readonly Nest.Indices _elasticResolutionsIndex = ConfigurationManager.AppSettings["ElasticSearchAlertResolutionsIndex"];
        private readonly Nest.Indices _elasticEnvInfoIndex = ConfigurationManager.AppSettings["ElasticSearchEnvironmentInformationIndex"];

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
                else if (_elasticResolutionsIndex == null)
                {
                    throw new ExtractException("ELI54050", "Configuration for elastic search resolution index is invalid, " +
                        "path in configuration is: ConfigurationManager.AppSettings[\"ElasticSearchAlertResolutionsIndex\"]");
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
                    .Index(_elasticAlertsIndex)
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


                //gets all resolutions that have an alert ID matching an alert from previous query
                var responseAlertsResolutions = _elasticClient.Search<AlertResolution>(s => s
                    .Index(_elasticResolutionsIndex)
                    .From(0)
                    .Query(q => q
                        .Terms(c => c
                            .Field(p => p.AlertId)
                            .Terms(alerts.Select(a => a.AlertId.ToLower())
                                .ToList()
                                .AsReadOnly()
                            )
                )));

                if (responseAlertsResolutions.IsValid)
                {
                    foreach (var alert in alerts)
                    {
                        foreach (var response in responseAlertsResolutions.Documents.ToList())
                        {
                            if (alert.AlertId == response.AlertId)
                            {
                                alert.Resolution = response;
                            }
                        }
                    }
                }
                else
                {
                    throw new ExtractException("ELI54056", "Issue with response alerts calling document");
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
                var responseAlert = _elasticClient.Search<AlertsObject>(s => s
                    .Index("cory-test-alert-mappings")
                    .Size(1)
                    .Query(q => q
                        .Match(c => c
                            .Field(p => p.AlertId)
                            .Query(alertId))));

                if (responseAlert.IsValid) 
                {
                    return responseAlert.Documents.ElementAt(0);
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
                    .Index("cory-test-alert-mappings")
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

        /// <summary>
        /// Gets a list of all available exceptions from a given source
        /// </summary>
        /// <param name="page">0 indexed page number to display</param>
        /// <returns>Collection of all Exceptions from the logging source</returns>
        public IList<ExceptionEvent> GetAllEvents(int page)
        {
            if (page < 0)
            {
                throw new ExtractException("ELI54060", "Issue with page number: " + nameof(page));
            }

            try
            {
                var response = _elasticClient.Search<ExceptionEvent>(s => s
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
        /// Gets the maximum number of allowed pages based on a PAGESIZE constant
        /// </summary>
        /// <returns>Number of valid alert pages</returns>
        public int GetMaxAlertPages()
        {
            //TODO: When we introduce filtering into this code, consider combining GetMaxAlertPages and GetMaxEventPages
            //Also consider combining GetAllEvents and GetAllAlerts

            try
            {
                var response = _elasticClient.Count<AlertsObject>(s => s
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
                ExtractException ex = new("ELI54063", "Error retrieving Alert Count", e);
                throw ex;
            }
        }

        /// <summary>
        /// Gets the maximum number of allowed pages based on a PAGESIZE constant
        /// </summary>
        /// <returns>Number of valid event pages</returns>
        public int GetMaxEventPages()
        {
            try
            {
                var response = _elasticClient.Count<ExceptionEvent>(s => s
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

        /// <summary>
        /// Queries for an environment document in elastic search that has a given entry in its data dictionary.
        /// </summary>
        /// <param name="searchBackwardsFrom">Date and time of the alert or error. Query will find most recent document that is still before this time.</param>
        /// <param name="dataKeyName">Name of the entry to look for in the documents data dictionary.</param>
        /// <returns>List containing single best match EnvironmentInformation from query or empty list.</returns>
        public List<EnvironmentInformation> TryGetInfoWithDataEntry(DateTime searchBackwardsFrom, string dataKeyName)
        {
            var response = _elasticClient.Search<EnvironmentInformation>(s => s
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

        /// <summary>
        /// Queries for an environment document in elastic search that has a given context type.
        /// </summary>
        /// <param name="searchBackwardsFrom">Date and time of the alert or error. Query will find most recent document that is still before this time.</param>
        /// <param name="contextType">Value for the context field of the desired document.</param>
        /// <returns>List containing single best match EnvironmentInformation from query or empty list.</returns>
        public List<EnvironmentInformation> TryGetInfoWithContextType(DateTime searchBackwardsFrom, string contextType)
        {
            var response = _elasticClient.Search<EnvironmentInformation>(s => s
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
    }
}
