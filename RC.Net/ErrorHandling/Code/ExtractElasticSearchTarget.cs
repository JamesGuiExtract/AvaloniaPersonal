using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using Newtonsoft.Json;
using NLog;
using NLog.Common;
using NLog.Config;
using NLog.Targets;
using System;
using System.Threading.Tasks;

namespace Extract.ErrorHandling
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
            Task<IndexResponse> response;
            if (elasticsearchClient is null)
            {
                InitializeTarget();
            }
            if (logEvent.Exception != null)
            {
                //NOTE: if you update the error message below, update the check in following line
                if (logEvent.Exception.Message.Contains("Failed to send log to Elasticsearch"))
                {
                    return;
                }
                response = elasticsearchClient!.IndexAsync(new ExceptionEvent(logEvent.Exception), request => request.Index(Index.ToLower()));
            }
            else if (logEvent.Parameters.Length == 1)
            {
                response = elasticsearchClient!.IndexAsync(logEvent.Parameters[0], request => request.Index(Index.ToLower()));
            }
            else if(!string.IsNullOrEmpty(logEvent.Message))
            {
                response = elasticsearchClient!.IndexAsync(logEvent.Message, request => request.Index(Index.ToLower()));
            }
            else
            {
                var ex = new ExtractException("ELI53677", "Invalid log event passed to ExtractElasticSearch target");
                ex.AddDebugData("LogEvent Object", logEvent.ToString());
                response = elasticsearchClient!.IndexAsync(ex, request => request.Index(Index.ToLower()));
            }
            if (response != null && !response.Result.IsSuccess())
            {
                //NOTE: if you update the error message below, update the check in above line
                var ex = new ExtractException("ELI53780",
                    "Failed to send log to Elasticsearch");
                ex.AddDebugData("Elasticsearch Response", response.Result.ToString());
                throw ex;
            }
        }
    }
}
