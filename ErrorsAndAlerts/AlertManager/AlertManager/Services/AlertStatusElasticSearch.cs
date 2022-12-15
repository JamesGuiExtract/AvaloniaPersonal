using AlertManager.Interfaces;
using AlertManager.Models.AllDataClasses;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Core.Search;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Elastic.Transport;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Configuration;
using Extract.ErrorHandling;


namespace AlertManager.Services
{
    /// <inheritdoc/>
    public class AlertStatusElasticSearch : IAlertStatus
    {
        private readonly string? elasticSearchCloudId = ConfigurationManager.AppSettings["ElasticSearchCloudId"];
        private readonly string? elasticSearchKeyPath = ConfigurationManager.AppSettings["ElasticSearchAPIKey"];
        private readonly string? elasticSearchAlertsPath = ConfigurationManager.AppSettings["ElasticSearchAlertsIndex"];
        private readonly string? elasticSearchResolutionsIndex = ConfigurationManager.AppSettings["ElasticSearchAlertResolutionsIndex"];

        private const int PAGESIZE = 25;
        public AlertStatusElasticSearch()
        {
            CheckPaths();
        }

        private void CheckPaths()
        {
            try
            {
                if (elasticSearchKeyPath == null)
                {
                    throw new ExtractException("ELI53853" ,"configuration for elastic search key path is invalid, path in " +
                        "configuration is: ConfigurationManager.AppSettings[\"ElasticSearchCloudId\"]");
                }
                else if (elasticSearchCloudId == null)
                {
                    throw new ExtractException("ELI53852", "configuration for elastic search cloud id is invalid, path " +
                        "in configuration is: ConfigurationManager.AppSettings[\"ElasticSearchAPIKey\"]");
                }
                else if (elasticSearchAlertsPath == null)
                {
                    throw new ExtractException("ELI53851" ,"configuration for elastic search alerts is invalid, path " +
                        "in configuration is: ConfigurationManager.AppSettings[\"ElasticSearchAlertsIndex\"]");
                }
                else if (elasticSearchResolutionsIndex == null)
                {
                    throw new ExtractException("ELI53849", "configuration for elastic search resolution index is invalid, " +
                        "path in configuration is: ConfigurationManager.AppSettings[\"ElasticSearchAlertResolutionsIndex\"]");
                }
            }
            catch(Exception e)
            {
                ExtractException ex = new ExtractException("ELI53845", "Issue with null paths ", e);
                throw ex;
            }
        }

        /// <inheritdoc/>
        /// 
        public IList<AlertsObject> GetAllAlerts(int page)
        {
            List<AlertsObject> alerts = new();

            if (page < 0)
            {
                var ex = new ExtractException("ELI53797", "Alert out of range");
                ex.AddDebugData("Page number being accessed", page);
                throw ex.AsExtractException("ELI53798");
            }

            try
            {
                alerts = new List<AlertsObject>();

                if (elasticSearchCloudId == null)
                {
                    var ex = new ExtractException("ELI53797", "Configuration path is null");
                    ex.AddDebugData("Page number being accessed", page);
                    throw ex.AsExtractException("ELI53798"); ;
                }

                var elasticClient = new ElasticsearchClient(elasticSearchCloudId,
                    new ApiKey(elasticSearchKeyPath));

                var responseAlerts = elasticClient.SearchAsync<LoggingTargetAlert>(s => s
                    .Index(elasticSearchAlertsPath)
                    .From(PAGESIZE * page)
                    .Size(PAGESIZE)
                ).Result;

                if (responseAlerts.IsValid)
                {
                    foreach (Hit<LoggingTargetAlert> alert in responseAlerts.Hits)
                    {
                        alerts.Add(ConvertAlert(alert));
                    }
                }
                else
                {
                    throw new ExtractException("ELI53848", "Unable to retrieve Alerts, issue with elastic search retrieval");
                }


                TermsQuery termsQuery = new TermsQuery()
                {
                    Field = "alertId",
                    Terms = new TermsQueryField(alerts
                    .Select(a => FieldValue.String(a.AlertId.ToLower()))
                    .ToList()
                    .AsReadOnly()
                )
                };

                var responseAlertResolutions = elasticClient.SearchAsync<AlertResolution>(s => s
                    .Index(elasticSearchResolutionsIndex)
                    .From(0)
                    .Query(q => q
                        .Terms(termsQuery)
                    )
                ).Result;

                if (responseAlertResolutions.IsValid)
                {
                    alerts.ForEach(a =>
                    {
                        responseAlertResolutions.Documents.ToList().ForEach(r =>
                        {
                            if (a.AlertId == r.AlertId)
                            {
                                a.Resolution = r;
                            }
                        });
                    });
                }
                else
                {
                    throw new ExtractException("ELI53847", "Issue with response alerts calling document");
                }
            }
            catch(Exception e)
            {
                ExtractException ex = new ExtractException("ELI53845", "Error with retrieving alerts: ",  e);
                ex.AddDebugData("page being accessed ", page);
                ex.AddDebugData("alert list being accessed ", JsonConvert.SerializeObject(alerts));
                throw ex;
            }  

            return alerts;
        }

        /// <inheritdoc/>
        public IList<EventObject> GetAllEvents(int page)
        {
            if (page < 0)
            {
                throw new ExtractException("ELI53739", "Issue with page number: " + nameof(page));
            }

            try
            {
                var elasticClient = new ElasticsearchClient(elasticSearchCloudId,
                new ApiKey(elasticSearchKeyPath));

                var response = elasticClient.SearchAsync<LoggingTargetError>(s => s
                    .Index(elasticSearchAlertsPath)
                    .From(0)
                    .Size(PAGESIZE)
                    .From(PAGESIZE * page)
                ).Result;

                if (response.IsValid)
                {
                    List<EventObject> events = new List<EventObject>();
                    foreach (LoggingTargetError error in response.Documents)
                    {
                        events.Add(ConvertException(error));
                    }
                    return events;
                }
                else
                {

                    throw new ExtractException("ELI53740", "Issue at page number: " + nameof(page) + "Elastic Search Client is " + elasticClient.ToString());

                }
            }
            catch(Exception e)
            {
                ExtractException ex = new ExtractException("ELI53854", "Error retrieving Events", e);
                throw ex;
            }

        }

        private EventObject ConvertException(LoggingTargetError logError)
        {
            bool stackTraceCheck = true;
            if (string.IsNullOrEmpty(logError.stackTrace)){
                stackTraceCheck = false;
            }
            return new EventObject()
            {
                eliCode = logError.eliCode,
                message = logError.message,
                contains_Stack_Trace = stackTraceCheck,
                stack_Trace = logError.stackTrace,
                time_Of_Error = logError.exceptionTime
            };
        }

        private AlertsObject ConvertAlert(Hit<LoggingTargetAlert> logAlert)
        {
            AlertsObject alert = new AlertsObject();
            alert.AlertId = logAlert.Id;
            try
            {
                if (logAlert.Source != null)
                {
                    alert.AlertName = logAlert.Source.name;
                    alert.Configuration = logAlert.Source.query;

                    string jsonHits = "[" + logAlert.Source.hits + "]";
                    List<LogIndexObject>? eventList = JsonConvert.DeserializeObject<List<LogIndexObject>>(jsonHits);

                    if(eventList == null)
                    {
                        throw new ExtractException("ELI53785", "Error converting events from Json to LogIndexObject, " +
                            "json Hits: " + jsonHits + "event list is null");
                    }

                    List<EventObject> associatedEvents = new List<EventObject>();
                    foreach (LogIndexObject eventLog in eventList)
                    {
                        associatedEvents.Add(ConvertException(eventLog._source));
                    }
                    alert.AssociatedEvents = associatedEvents;
                }
            }
            catch(Exception e)
            {
                ExtractException ex = new("ELI53855", "Error deserializing alerts from elastic search", e);
                throw ex;
            }
            return alert;
        }
    }
}
