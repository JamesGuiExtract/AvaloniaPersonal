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
        private const int PAGESIZE = 25;
        public AlertStatusElasticSearch()
        {
        }

        /// <inheritdoc/>
        public IList<AlertsObject> GetAllAlerts(int page)
        {
            if(page < 0)
            {
                throw new ExtractException("ELI53737", "Alert out of range");
            }

            List<AlertsObject> alerts = new List<AlertsObject>();
            var elasticClient = new ElasticsearchClient(ConfigurationManager.AppSettings["ElasticSearchCloudId"],
                new ApiKey(ConfigurationManager.AppSettings["ElasticSearchAPIKey"]));

            var responseAlerts = elasticClient.SearchAsync<LoggingTargetAlert>(s => s
                .Index(ConfigurationManager.AppSettings["ElasticSearchAlertsIndex"])
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
                throw new ExtractException("ELI53738", "Unalbe to retrieve Alerts");
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
                .Index(ConfigurationManager.AppSettings["ElasticSearchAlertResolutionsIndex"])
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
                        if(a.AlertId == r.AlertId)
                        {
                            a.Resolution = r;
                        }
                    });
                });
            }
            else
            {
               throw new ExtractException("ELI53749", "Issue with response alerts calling document");
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
                var elasticClient = new ElasticsearchClient(ConfigurationManager.AppSettings["ElasticSearchCloudId"],
                new ApiKey(ConfigurationManager.AppSettings["ElasticSearchAPIKey"]));

                var response = elasticClient.SearchAsync<LoggingTargetError>(s => s
                    .Index(ConfigurationManager.AppSettings["ElasticSearchExceptionIndex"])
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
                    throw new ExtractException("ELI53740", "Issue at page number: " + nameof(page));

                }
            }
            catch(Exception e)
            {
                throw e.AsExtractException("ELI53782");
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
                        throw new ExtractException("Error converting events from Json to LogIndexObject" ,"ELI53785");
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
                throw e.AsExtractException("ELI53784");
            }
            return alert;
        }
    }
}
