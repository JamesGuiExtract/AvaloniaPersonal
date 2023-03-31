using AlertManager.Benchmark.DtoObjects;
using AlertManager.Models.AllEnums;
using Elastic.Clients.Elasticsearch.IndexManagement;
using Elasticsearch.Net;
using Extract.ErrorHandling;
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
        private string eventIndex = "cory-test-event-mappings";

        //value arrays for randomizing document values
        //alert values
        private string[] alertTypes = new string[] { "Type 1", "Type 2", "Type 3" };
        private string[] configurations = new string[] { "Config 1", "Config 2", "Config 3" };
        private string[] userFounds = new string[] { "Me", "You" };
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
        private string[] customers = new string[] { "Customer 1", "Customer 2", "Customer 3" };
        private string[] measurementTypes = new string[]
        {
            "Software", "Compute", "Services",
            "Type 4", "Type 5", "Type 6",
            "Type 7", "Type 8", "Type 9",
            "Type 10"
        };
        private string[] contexts = new string[] { "Machine", "DB" };
        private string[] entities = new string[] { "Server 1", "Server 2", "Server 3", "ProdDB" };
        private Dictionary<string, string>[] envDatas = new Dictionary<string, string>[]
        {
            new Dictionary<string, string>
            { { "Version", "2023.3.1.42" }, { "OS", "Server 2019"}, { "License", "LabDE Server"} },
            new Dictionary<string, string>
            { { "CPU %", "81" }, { "Memory %", "66"} },
            new Dictionary<string, string>
            { { "Machine", "Server1" }, { "User", "ServiceUser"}, { "DB","ProdDB"} },
        };

        //event values
        private string[] servers = new string[] { "Server 1", "Server 2", "Server 3" };
        private string[] databases = new string[] { "DB 1", "DB 2", "DB 3" };
        private Stack<string>[] stackTraces = new Stack<string>[]
        {
            new Stack<string>(new[] { "Stack level 1", "Stack level 2", "Stack level 3"}),
            new Stack<string>(new[] { "Stack level 1", "Stack level 2"}),
            new Stack<string>(new[] { "Stack level 1"})
        };
        private List<KeyValuePair<string, string>>[] eventDatas = new List<KeyValuePair<string, string>>[]
        {
            new List<KeyValuePair<string, string>>{
                new KeyValuePair<string, string>("SavedServer", "Extract:123456789"),
                new KeyValuePair<string, string>("SavedDatabase", "Extract:987654321")
            },
            new List<KeyValuePair<string, string>>{
                new KeyValuePair<string, string>("File", "C:\\FakePath"),
                new KeyValuePair<string, string>("Task", "task #1")
            },
            new List<KeyValuePair<string, string>>{
                new KeyValuePair<string, string>("CatchID", "ELI123"),
                new KeyValuePair<string, string>("HRESULT", "0x12345678")
            },
        };
        private string[] levels = new string[] { "level 1", "level 2", "level 3" };
        private ApplicationStateDto[] applicationStates = new ApplicationStateDto[]
        {
            new ApplicationStateDto
            {
                ApplicationName = "Application 1",
                ApplicationVersion = "Version 1",
                ComputerName = "Computer 1",
                UserName = "User 1",
                PID = 123,
            },
            new ApplicationStateDto
            {
                ApplicationName = "Application 2",
                ApplicationVersion = "Version 2",
                ComputerName = "Computer 2",
                UserName = "User 2",
                PID = 248,
            },
            new ApplicationStateDto
            {
                ApplicationName = "Application 3",
                ApplicationVersion = "Version 3",
                ComputerName = "Computer 3",
                UserName = "User 3",
                PID = 1000,
            },

        };

        internal ElasticSearchBenchmarkPopulator()
        {
            var settings = new ConnectionSettings(_elasticCloudId, new ApiKeyAuthenticationCredentials(_elasticKeyPath));
            //This setting is needed for bulk index command
            settings.EnableApiVersioningHeader();
            _elasticClient = new(settings);
        }

        private string GetRandomValue(string[] values)
        {
            return values.ElementAt(random.Next(0, values.Length));
        }

        private ApplicationStateDto GetRandomValue(ApplicationStateDto[] values)
        {
            return values.ElementAt(random.Next(0, values.Length));
        }

        private Stack<string> GetRandomValue(Stack<string>[] values)
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

        private List<KeyValuePair<string, string>> GetRandomValue(List<KeyValuePair<string, string>>[] values)
        {
            return values.ElementAt(random.Next(0, values.Length));
        }

        internal void BulkIndexEnvironments()
        {
            List<EnvironmentDto> documents = new();
            int numToPopulate = 1000000;
            random = new(438);

            for (int i = 0; i < numToPopulate; i++)
            {
                DateTime collectionTime = DateTime.Now.AddDays(random.NextDouble() * -300);
                string customer = GetRandomValue(customers);
                Dictionary<string, string> data = GetRandomValue(envDatas);
                string measurementType = GetRandomValue(measurementTypes);
                string context = GetRandomValue(contexts);
                string entity = GetRandomValue(entities);

                EnvironmentDto env = new()
                {
                    CollectionTime = collectionTime,
                    Customer = customer,
                    Data = data,
                    MeasurementType = measurementType,
                    Context = context,
                    Entity = entity
                };

                documents.Add(env);
            }

            TryDeleteIndex(environmentIndex);

            TryCreateIndexAutoMap<EnvironmentDto>(environmentIndex);

            TryBulkIndex<EnvironmentDto>(environmentIndex, documents);
        }

        internal void BulkIndexAlerts()
        {
            List<AlertDto> documents = new();
            int numToPopulate = 1000000;
            random = new(438);

            for (int i = 0; i < numToPopulate; i++)
            {
                string alertName = "Alert" + i.ToString();
                string alertType = GetRandomValue(alertTypes);
                string configuration = GetRandomValue(configurations);
                DateTime activationTime = DateTime.Now.AddDays(random.NextDouble() * -30);
                string userFound = GetRandomValue(userFounds);
                string machineFoundError = GetRandomValue(machineFoundErrors);
                AlertActionDto action = GetRandomValue(actions);

                AlertDto alert = new() {
                    AlertName = alertName,
                    AlertType = alertType,
                    Configuration = configuration,
                    ActivationTime = activationTime,
                    UserFound = userFound,
                    MachineFoundError = machineFoundError,
                    Actions = new() { action },
                };

                documents.Add(alert);
            }

            TryDeleteIndex(alertIndex);

            TryCreateIndexAutoMap<AlertDto>(alertIndex);

            TryBulkIndex<AlertDto>(alertIndex, documents);
        }

        internal void BulkIndexEvents()
        {
            int numToPopulate = 1000000;
            List<EventDto> documents = new();
            random = new(438);

            for (int i = 0; i < numToPopulate; i++)
            {
                string id = i.ToString();
                string eliCode = "ELI" + id;
                string message = "Fake event of code " + eliCode;
                DateTime exceptionTime = DateTime.Now.AddDays(random.NextDouble() * -30);
                Int32 fileID = i;
                Int32 actionID = i;
                string databaseServer = GetRandomValue(servers);
                string databaseName = GetRandomValue(databases);
                Stack<string> stackTrace = GetRandomValue(stackTraces);
                List<KeyValuePair<string, string>> data = GetRandomValue(eventDatas);
                string level = GetRandomValue(levels);
                ApplicationStateDto applicationState = GetRandomValue(applicationStates);
                EventDto? inner = null;
                if (((i + 1) % 10000) == 0)
                    inner = documents.ElementAt(random.Next() % 10);

                EventDto eventDto = new()
                {
                    Id = id,
                    EliCode = eliCode,
                    Message = message,
                    ExceptionTime = exceptionTime,
                    FileID = fileID,
                    ActionID = actionID,
                    DatabaseServer = databaseServer,
                    DatabaseName = databaseName,
                    StackTrace = stackTrace,
                    Data = data,
                    Level = level,
                    ApplicationState = applicationState,
                    Inner = inner,
                };

                documents.Add(eventDto);
            }

            TryDeleteIndex(eventIndex);

            TryCreateIndexAutoMap<EventDto>(eventIndex);

            TryBulkIndex<EventDto>(eventIndex, documents);
        }

        private T GetRandomDocumentFromIndex<T>(string index) where T : class
        {
            var response = _elasticClient.Search<T>(s => s
                .Index(index)
                .Size(1)
                .Query(q => q
                    .FunctionScore(fs => fs
                        .Functions(f => f
                            .RandomScore())
                        .Query(fq => fq.MatchAll()))));

            return response.Hits.ElementAt(0).Source;
        }

        public string GetRandomIdFromIndex<T>(string index) where T : class
        {
            var response = _elasticClient.Search<T>(s => s
                .Index(index)
                .Size(1)
                .Query(q => q
                    .FunctionScore(fs => fs
                        .Functions(f => f
                            .RandomScore())
                        .Query(fq => fq.MatchAll()))));

            return response.Hits.ElementAt(0).Id;
        }

        private void TryDeleteIndex(string index)
        {
            try 
            {
                var deleteIndexResponse = _elasticClient.Indices.Delete(index);

                ExtractException.Assert("ELI54180", deleteIndexResponse.DebugInformation, deleteIndexResponse.IsValid);
            }
            catch (Exception e)
            {
                ExtractException ex = new("ELI54165", "Issue deleting index " + index, e);
                throw ex;
            }
        }

        private void TryCreateIndexAutoMap<T>(string index) where T : class
        {
            try 
            {
                var createIndexResponse = _elasticClient.Indices.Create(index, c => c
                .Map<T>(m => m
                    .AutoMap()));

                if (createIndexResponse.IsValid)
                {
                }
                else
                {
                    throw new ExtractException("ELI54180", "Unable to create index: " + index 
                        + " of type " + typeof(T).ToString());
                }
            }
            catch (Exception e) 
            {
                ExtractException ex = new("ELI54166", "Issue with index creation and mapping ", e);
                throw ex;
            }
        }

        private void TryBulkIndex<T>(string index, List<T> documents) where T : class
        {
            try
            {
                ConcurrentBag<BulkResponse> bulkResponses = new();
                int requests = 0;

                var bulkAllObservable = _elasticClient.BulkAll(documents, b => b
                    .Index(index)
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
            catch (Exception e) 
            {
                ExtractException ex = new("ELI54167", 
                    "Issue with bulk index on " + index + " of type " + typeof(T).ToString(), 
                    e);
                throw ex;
            }
        }
    }
}