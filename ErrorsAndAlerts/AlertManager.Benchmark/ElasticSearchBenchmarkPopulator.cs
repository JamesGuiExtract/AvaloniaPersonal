using AlertManager.Benchmark.DtoObjects;
using AlertManager.Models.AllEnums;
using AlertManager.Services;
using Elasticsearch.Net;
using Nest;
using System.Collections.Concurrent;
using System.Configuration;

namespace AlertManager.Benchmark.Populator
{
    public class ElasticSearchBenchmarkPopulator 
    {
        private readonly string? _elasticCloudId = ConfigurationManager.AppSettings["ElasticSearchCloudId"];
        private readonly string? _elasticKeyPath = ConfigurationManager.AppSettings["ElasticSearchAPIKey"];
        private ElasticClient _elasticClient;
        private Random random = new(438);

        //value arrays for randomizing document values
        private string[] alertTypes = new string[]{ "Type 1", "Type 2", "Type 3"};
        private DateTime[] activationTimes = new DateTime[] { DateTime.Now, DateTime.Now.AddDays(-1), DateTime.Now.AddHours(-1)};
        private string[] configurations = new string[] { "Config 1", "Config 2", "Config 3"};
        private string[] userFounds = new string[] { "Me", "You"};
        private string[] machineFoundErrors = new string[] { "You caused an error >:(", "I caused an error >:)" };
        private AlertActionDto[] actions = new AlertActionDto[] {
            new AlertActionDto{
                ActionComment = "Unresolved alert",
                ActionTime = DateTime.Now,
                ActionType = Enum.GetName(typeof(TypeOfResolutionAlerts), 0)
            }, 
            new AlertActionDto{
                ActionComment = "Resolved alert",
                ActionTime = DateTime.Now,
                ActionType = Enum.GetName(typeof(TypeOfResolutionAlerts), 2)
            },
            new AlertActionDto{
                ActionComment = "Snoozed alert",
                ActionTime = DateTime.Now,
                SnoozeDuration = DateTime.Now.AddDays(1),
                ActionType = Enum.GetName(typeof(TypeOfResolutionAlerts), 1)
            }
        };

        internal ElasticSearchBenchmarkPopulator() 
        {
            var settings = new ConnectionSettings(_elasticCloudId, new ApiKeyAuthenticationCredentials(_elasticKeyPath));
            settings.EnableApiVersioningHeader();
            _elasticClient = new(settings);
        }

        private string GetRandomValue(string[] values) 
        {
            return values.ElementAt(random.Next(0, values.Length));
        }

        private DateTime GetRandomValue(DateTime[] values)
        {
            return values.ElementAt(random.Next(0, values.Length));
        }

        private AlertActionDto GetRandomValue(AlertActionDto[] values)
        {
            return values.ElementAt(random.Next(0, values.Length));
        }

        internal void BulkIndexAlerts() 
        {
            List<AlertDto> documents = new();
            int numToPopulate = 1000000;

            for (int i = 0; i < numToPopulate; i++)
            {
                string alertId = i.ToString();
                string alertName = "Alert" + alertId;
                string alertType = GetRandomValue(alertTypes);
                string configuration = GetRandomValue(configurations);
                DateTime activationTime = GetRandomValue(activationTimes);
                string userFound = GetRandomValue(userFounds);
                string machineFoundError = GetRandomValue(machineFoundErrors);
                AlertActionDto action = GetRandomValue(actions);
                
                AlertDto alert = new() { 
                    AlertId = alertId,
                    AlertName = alertName,
                    AlertType= alertType,
                    Configuration= configuration,
                    ActivationTime = activationTime,
                    UserFound = userFound,
                    MachineFoundError= machineFoundError,
                    Actions = new() { action},
                };

                documents.Add(alert);
            }

            var deleteIndexResponse = _elasticClient.Indices.Delete("cory-test-alert-mappings");

            var createIndexResponse = _elasticClient.Indices.Create("cory-test-alert-mappings", c => c
                .Map<AlertDto>(m => m
                    .AutoMap()));

            ConcurrentBag<BulkResponse> bulkResponses = new();
            int requests = 0;

            var bulkAllObservable = _elasticClient.BulkAll(documents, b => b
                .Index("cory-test-alert-mappings")
                .BulkResponseCallback(r =>
                {
                    bulkResponses.Add(r);
                    Interlocked.Increment(ref requests);
                })
                .BackOffTime("30s")
                .BackOffRetries(2)
                .RefreshOnCompleted()
                .MaxDegreeOfParallelism(Environment.ProcessorCount)
                .Size(1000))
            .Wait(TimeSpan.FromMinutes(15), next => { });
        }
    }

    public class Program 
    {
        static void Main(string[] args)
        {
            ElasticSearchBenchmarkPopulator populator = new ElasticSearchBenchmarkPopulator();

            if (args.Contains("BulkIndexAlerts"))
            {
                populator.BulkIndexAlerts();
            }

            //Used to verify index operation resulted in queryable data
            ElasticSearchService searchClient = new();
            var alert = searchClient.GetAlertById("1");

            //Used for breakpoint
            _ = 1;
        }
    }
}



