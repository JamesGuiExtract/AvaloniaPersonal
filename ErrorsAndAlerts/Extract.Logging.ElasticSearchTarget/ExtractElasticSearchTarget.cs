using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using Newtonsoft.Json;
using NLog;
using NLog.Common;
using NLog.Config;
using NLog.Targets;
using System;

namespace Extract.ErrorHandling.ElasticSearch
{
    [Target("ExtractElasticSearch")]
    public class ExtractElasticSearchTarget : TargetWithLayout
    {
        public ExtractElasticSearchTarget() : base()
        {
            CloudID = "";
            APIKey = "";

            Index = "extract";
        }

        [RequiredParameter]
        public string CloudID { get; set; }
        [RequiredParameter]
        public string APIKey { get; set; }
        [RequiredParameter]
        public string Index { get; set; }

        ElasticsearchClient? elasticsearchClient;

        protected override void InitializeTarget()
        {
            base.InitializeTarget();
            elasticsearchClient = new ElasticsearchClient(CloudID, new ApiKey(APIKey));

        }

        protected override void CloseTarget()
        {
            base.CloseTarget();
            elasticsearchClient = null;
        }

        protected override void Write(AsyncLogEventInfo logEvent)
        {
            base.Write(logEvent);
        }

        protected override void Write(LogEventInfo logEvent)
        {
            if (elasticsearchClient is null)
            {
                InitializeTarget();
            }
            if (logEvent.Exception != null)
            {
                var response = elasticsearchClient!.IndexAsync(logEvent.Exception, request => request.Index(Index.ToLower()));
            }
            else if (logEvent.Parameters.Length == 1)
            {
                var response = elasticsearchClient!.IndexAsync(logEvent.Parameters[0], request => request.Index(Index.ToLower()));
            }
            else
            {
                var response = elasticsearchClient!.IndexAsync(logEvent.Message, request => request.Index(Index.ToLower()));
            }
        }
    }
}
