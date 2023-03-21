using AlertManager.Benchmark.DtoObjects;
using AlertManager.Models.AllEnums;
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

        private string alertIndex = "cory-test-alert-mappings";
        private string environmentIndex = "cory-test-environment-mappings";

        //value arrays for randomizing document values
        //alert values
        private string[] alertTypes = new string[]{ "Type 1", "Type 2", "Type 3"};
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

        //environment values
        private string[] customers = new string[] { "Customer 1", "Customer 2", "Customer 3"};
        private string[] measurementTypes = new string[] 
        { 
            "Software", "Compute", "Services",
            "Type 4", "Type 5", "Type 6",
            "Type 7", "Type 8", "Type 9",
            "Type 10"
        };
        private string[] contexts = new string[] { "Machine", "DB"};
        private string[] entities = new string[] { "Server1", "Server2", "ProdDB" };
        private Dictionary<string, string>[] datas = new Dictionary<string, string>[] 
        {
            new Dictionary<string, string> 
            { { "Version", "2023.3.1.42" }, { "OS", "Server 2019"}, { "License", "LabDE Server"} },
            new Dictionary<string, string>
            { { "CPU %", "81" }, { "Memory %", "66"} },
            new Dictionary<string, string>
            { { "Machine", "Server1" }, { "User", "ServiceUser"}, { "DB","ProdDB"} },
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

        private Dictionary<string, string> GetRandomValue(Dictionary<string, string>[] values)
        {
            return values.ElementAt(random.Next(0, values.Length));
        }

        internal void BulkIndexEnvironments() 
        {
            List<EnvironmentDto> documents = new();
            int numToPopulate = 10000000;

            for (int i = 0; i < numToPopulate; i++)
            {
                DateTime collectionTime = DateTime.Now.AddDays(random.NextDouble() * -300);
                string customer = GetRandomValue(customers);
                Dictionary<string, string> data = GetRandomValue(datas);
                string measurementType = GetRandomValue(measurementTypes);
                string context = GetRandomValue(contexts);
                string entity = GetRandomValue(entities);

                EnvironmentDto env = new()
                {
                    CollectionTime= collectionTime,
                    Customer= customer,
                    Data= data,
                    MeasurementType= measurementType,
                    Context= context,
                    Entity= entity
                };

                documents.Add(env);
            }

            var deleteIndexResponse = _elasticClient.Indices.Delete(environmentIndex);

            var createIndexResponse = _elasticClient.Indices.Create(environmentIndex, c => c
                .Map<EnvironmentDto>(m => m
                    .AutoMap()));

            ConcurrentBag<BulkResponse> bulkResponses = new();
            int requests = 0;

            var bulkAllObservable = _elasticClient.BulkAll(documents, b => b
                .Index(environmentIndex)
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
                DateTime activationTime = DateTime.Now.AddDays(random.NextDouble() * -30);
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

            var deleteIndexResponse = _elasticClient.Indices.Delete(alertIndex);

            var createIndexResponse = _elasticClient.Indices.Create(alertIndex, c => c
                .Map<AlertDto>(m => m
                    .AutoMap()));

            ConcurrentBag<BulkResponse> bulkResponses = new();
            int requests = 0;

            var bulkAllObservable = _elasticClient.BulkAll(documents, b => b
                .Index(alertIndex)
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
}