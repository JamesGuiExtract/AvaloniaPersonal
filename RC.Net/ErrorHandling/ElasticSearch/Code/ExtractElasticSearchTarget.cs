using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
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
            var ex = logEvent.Exception as ExtractException;

            if (ex == null)
            {
                ex = ex.AsExtractException("ELI53677");
            }

            if (elasticsearchClient is null)
            {
                InitializeTarget();
            }
            if (ex != null)
            {
                var response = elasticsearchClient!.IndexAsync(new ExceptionEvent(ex), request => request.Index(Index.ToLower()));
                if (!response.Result.IsValid)
                {
                    Console.WriteLine($"Error sending to Elastic with {response.Result.DebugInformation}");
                }
                else
                {
                    Console.WriteLine("Success");
                }
            }
        }
    }
}
