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
        private readonly string? elasticSearchEventsPath = ConfigurationManager.AppSettings["ElasticSearchExceptionIndex"];
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
                    throw new ExtractException("ELI53853" ,"Configuration for elastic search key path is invalid, path in " +
                        "configuration is: ConfigurationManager.AppSettings[\"ElasticSearchCloudId\"]");
                }
                else if (elasticSearchCloudId == null)
                {
                    throw new ExtractException("ELI53852", "Configuration for elastic search cloud id is invalid, path " +
                        "in configuration is: ConfigurationManager.AppSettings[\"ElasticSearchAPIKey\"]");
                }
                else if (elasticSearchAlertsPath == null)
                {
                    throw new ExtractException("ELI53851" , "Configuration for elastic search alerts is invalid, path " +
                        "in configuration is: ConfigurationManager.AppSettings[\"ElasticSearchAlertsIndex\"]");
                }
                else if (elasticSearchResolutionsIndex == null)
                {
                    throw new ExtractException("ELI53849", "Configuration for elastic search resolution index is invalid, " +
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

                if (elasticSearchCloudId == null 
                    || elasticSearchAlertsPath == null 
                    || elasticSearchResolutionsIndex == null 
                    || elasticSearchKeyPath == null)
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

                if (responseAlerts.IsValidResponse)
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

                if (responseAlertResolutions.IsValidResponse)
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
        public IList<ExceptionEvent> GetAllEvents(int page)
        {
            if (page < 0)
            {
                throw new ExtractException("ELI53739", "Issue with page number: " + nameof(page));
            }

            try
            {
                if (elasticSearchCloudId == null 
                    || elasticSearchKeyPath == null 
                    || elasticSearchAlertsPath == null
                    || elasticSearchEventsPath == null)
                {
                    throw new Exception("invalid paths");
                }

                var elasticClient = new ElasticsearchClient(elasticSearchCloudId,
                new ApiKey(elasticSearchKeyPath));

                var response = elasticClient.SearchAsync<ExceptionEvent>(s => s
                    .Index(elasticSearchEventsPath)
                    .From(0)
                    .Size(PAGESIZE)
                    .From(PAGESIZE * page)
                ).Result;

                if (response.IsValidResponse)
                {
                    return response.Documents.ToList();
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

                    List<ExceptionEvent> associatedEvents = new List<ExceptionEvent>();
                    foreach (LogIndexObject eventLog in eventList)
                    {
                        associatedEvents.Add(eventLog._source);
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
