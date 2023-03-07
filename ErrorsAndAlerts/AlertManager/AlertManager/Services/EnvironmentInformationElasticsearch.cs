using AlertManager.Models.AllDataClasses;
using Extract.ErrorHandling;
using System;
using System.Collections.Generic;
using System.Configuration;
using Nest;
using Elasticsearch.Net;
using System.Linq;
using AlertManager.Interfaces;

namespace AlertManager.Services
{
    /// <inheritdoc/>
    public class EnvironmentInformationElasticsearch : IEnvironmentInformationSearch
    {
        private readonly string? elasticSearchKeyPath = ConfigurationManager.AppSettings["ElasticSearchAPIKey"];
        private readonly string? elasticSearchCloudId = ConfigurationManager.AppSettings["ElasticSearchCloudId"];
        
        private readonly string? elasticSearchEnvironmentInformationIndex = ConfigurationManager.AppSettings["ElasticSearchEnvironmentInformationIndex"];

        public EnvironmentInformationElasticsearch()
        {
            CheckPaths();
        }

        /// <summary>
        /// Validates paths to elastic search service.
        /// </summary>
        private void CheckPaths()
        {
            try
            {
                if (elasticSearchKeyPath == null)
                {
                    throw new ExtractException("ELI54008", "Configuration for elastic search key path is invalid, path in " +
                        "configuration is: ConfigurationManager.AppSettings[\"ElasticSearchCloudId\"]");
                }
                if (elasticSearchCloudId == null)
                {
                    throw new ExtractException("ELI54009", "Configuration for elastic search cloud id is invalid, path " +
                        "in configuration is: ConfigurationManager.AppSettings[\"ElasticSearchAPIKey\"]");
                }
                if (elasticSearchEnvironmentInformationIndex == null)
                {
                    throw new ExtractException("ELI54010", "Configuration for elastic search environment information index is invalid, " +
                        "path in configuration is: ConfigurationManager.AppSettings[\"ElasticSearchEnvironmentInformationIndex\"]");
                }
            }
            catch (Exception e)
            {
                ExtractException ex = new ExtractException("ELI54011", "Issue with null paths ", e);
                throw ex;
            }
        }

        /// <inheritdoc />
        public List<EnvironmentInformation> TryGetInfoWithDataEntry(DateTime searchBackwardsFrom, string dataKeyName) 
        {
            var elasticClient = new ElasticClient(elasticSearchCloudId, new ApiKeyAuthenticationCredentials(elasticSearchKeyPath));

            var response = elasticClient.Search<EnvironmentInformation>(s => s
                .Index(elasticSearchEnvironmentInformationIndex)
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

        /// <inheritdoc />
        public List<EnvironmentInformation> TryGetInfoWithContextType(DateTime searchBackwardsFrom, string contextType)
        {
            var elasticClient = new ElasticClient(elasticSearchCloudId, new ApiKeyAuthenticationCredentials(elasticSearchKeyPath));

            var response = elasticClient.Search<EnvironmentInformation>(s => s
                .Index(elasticSearchEnvironmentInformationIndex)
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
