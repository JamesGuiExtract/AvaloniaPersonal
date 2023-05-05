using AlertManager.Models.AllEnums;
using Elasticsearch.Net;
using Extract.ErrorHandling;
using Nest;
using System.Collections.Concurrent;
using System.Configuration;
using Extract.ErrorsAndAlerts.ElasticDTOs;

namespace AlertManager.Benchmark.Populator
{
    public class ElasticSearchBenchmarkPopulator
    {
        private readonly string? _elasticCloudId = ConfigurationManager.AppSettings["ElasticSearchCloudId"];
        private readonly string? _elasticKeyPath = ConfigurationManager.AppSettings["ElasticSearchAPIKey"];
        private ElasticClient _elasticClient;
        private Random random = new(438);

        private readonly Nest.IndexName alertIndex = ConfigurationManager.AppSettings["ElasticSearchAlertsIndex"];
        private readonly Nest.IndexName environmentIndex = ConfigurationManager.AppSettings["ElasticSearchEnvironmentInformationIndex"];
        private readonly Nest.IndexName eventIndex = ConfigurationManager.AppSettings["ElasticSearchEventsIndex"];

        //value arrays for randomizing document values
        //alert values
        private string[] configurations = new string[] { "Config 1", "Config 2", "Config 3" };
        private AlertActionDto[] actions = new AlertActionDto[] {
            new AlertActionDto{
                ActionComment = "Unresolved alert",
                ActionTime = DateTime.Now,
                ActionType = Enum.GetName(typeof(AlertStatus), 0)
            },
            new AlertActionDto{
                ActionComment = "Resolved alert",
                ActionTime = DateTime.Now,
                ActionType = Enum.GetName(typeof(AlertStatus), 2)
            },
            new AlertActionDto{
                ActionComment = "Snoozed alert",
                ActionTime = DateTime.Now,
                SnoozeDuration = DateTime.Now.AddDays(1),
                ActionType = Enum.GetName(typeof(AlertStatus), 1)
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
        private string[] contexts = new string[] { "Machine", "DB" , "FPS"};
        private string[] entities = new string[] { "Server 1", "Server 2", "Server 3", "ProdDB" , "Machine 1", "Machine 2", "Machine 3", "FPS 1"};
        List<KeyValuePair<string, string>>[] envDatas = new List<KeyValuePair<string, string>>[] 
        {
            new List<KeyValuePair<string, string>>{
                new KeyValuePair<string, string>("Version", "2023.3.1.42"),
                new KeyValuePair<string, string>("OS", "Server 2019"),
                new KeyValuePair<string, string>("License", "LabDE Server"),
            },
            new List<KeyValuePair<string, string>>{
                new KeyValuePair<string, string>("CPU %", "81"),
                new KeyValuePair<string, string>("Memory %", "66"),
            },
            new List<KeyValuePair<string, string>>{
                new KeyValuePair<string, string>("Machine", "Server1"),
                new KeyValuePair<string, string>("User", "ServiceUser"),
                new KeyValuePair<string, string>("DB", "ProdDB"),
            },
        };

        //event values
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
        private ContextInfoDto[] applicationStates = new ContextInfoDto[]
        {
            new ContextInfoDto
            {
                ApplicationName = "Application 1",
                ApplicationVersion = "Version 1",
                MachineName = "Machine 1",
                UserName = "User 1",
                PID = 123,
                FileID = 321,
                ActionID = 222,
            },
            new ContextInfoDto
            {
                ApplicationName = "Application 2",
                ApplicationVersion = "Version 2",
                MachineName = "Machine 2",
                DatabaseServer = "Server 1",
                UserName = "User 2",
                PID = 248,
                FileID = 963,
                ActionID = 190,
            },
            new ContextInfoDto
            {
                ApplicationName = "Application 3",
                ApplicationVersion = "Version 3",
                MachineName = "Machine 3",
                DatabaseServer = "Server 2",
                FpsContext = "FPS 1",
                UserName = "User 3",
                PID = 1000,
                FileID = 500,
                ActionID = 27,
            },

        };

        internal ElasticSearchBenchmarkPopulator()
        {
            var settings = new ConnectionSettings(_elasticCloudId, new ApiKeyAuthenticationCredentials(_elasticKeyPath));
            //This setting is needed for bulk index command
            settings.EnableApiVersioningHeader();
            _elasticClient = new(settings);
        }

        //Simple randomizers for generating test data
        private string GetRandomValue(string[] values)
        {
            return values.ElementAt(random.Next(0, values.Length));
        }

        private ContextInfoDto GetRandomValue(ContextInfoDto[] values)
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

        /// <summary>
        /// Generates fake environment data and populates a testable elasticsearch index with the data
        /// </summary>
        internal void BulkIndexEnvironments()
        {
            List<EnvironmentDto> documents = new();
            int numToPopulate = 1000000;
            random = new(438);

            for (int i = 0; i < numToPopulate; i++)
            {
                DateTime collectionTime = DateTime.Now.AddDays(random.NextDouble() * -300);
                string customer = GetRandomValue(customers);
                List<KeyValuePair<string, string>> data = GetRandomValue(envDatas);
                string measurementType = GetRandomValue(measurementTypes);
                string context = GetRandomValue(contexts);
                string entity = GetRandomValue(entities);

                EnvironmentDto env = new()
                {
                    CollectionTime = collectionTime,
                    Customer = customer,
                    Data = data,
                    MeasurementType = measurementType,
                    ContextType = context,
                    Entity = entity
                };

                documents.Add(env);
            }

            TryDeleteIndex(environmentIndex);

            TryCreateIndexAutoMap<EnvironmentDto>(environmentIndex);

            TryBulkIndex<EnvironmentDto>(environmentIndex, documents);
        }

        /// <summary>
        /// Generates fake alert data and populates a testable elasticsearch index with the data.
        /// Requires populated Environments and Events first
        /// </summary>
        internal void BulkIndexAlerts()
        {
            List<AlertDto> documents = new();
            int numToPopulate = 1000000;
            random = new(438);

            int randomDtoIterator = 0;
            List<EventDto> randomEvents = Get1000RandomDocsFromIndex<EventDto>(eventIndex);
            List<EnvironmentDto> randomEnvironments = Get1000RandomDocsFromIndex<EnvironmentDto>(environmentIndex);

            for (int i = 0; i < numToPopulate; i++)
            {
                string alertName = "Alert" + i.ToString();
                string configuration = GetRandomValue(configurations);
                DateTime activationTime = DateTime.Now.AddDays(random.NextDouble() * -30);
                AlertActionDto action = GetRandomValue(actions);

                List<EventDto> associatedEvents = new();
                List<EnvironmentDto> associatedEnvs = new();
                string HitsType;
                if (i % 2 == 0)
                {
                    HitsType = "Events";
                    for (int j = random.Next(1, 3); j <= 3; j++)
                    {
                        associatedEvents.Add(randomEvents.ElementAt(randomDtoIterator++ % 1000));
                    }
                }
                else
                {
                    HitsType = "Environments";
                    for (int j = random.Next(1, 3); j <= 3; j++)
                    {
                        associatedEnvs.Add(randomEnvironments.ElementAt(randomDtoIterator++ % 1000));
                    }
                }

                AlertDto alert = new() {
                    AlertName = alertName,
                    Configuration = configuration,
                    ActivationTime = activationTime,
                    Actions = new List<AlertActionDto>() { action },
                    HitsType = HitsType,
                    //Hits is set to whichever associated objects list is populated
                    Hits = associatedEnvs.Count != 0 ? associatedEnvs : associatedEvents,
                };

                documents.Add(alert);
            }

            TryDeleteIndex(alertIndex);

            TryCreateIndexAutoMap<AlertDto>(alertIndex);

            TryBulkIndex<AlertDto>(alertIndex, documents);
        }

        /// <summary>
        /// Generates fake event data and populates a testable elasticsearch index with the data
        /// </summary>
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
                Int32 actionID = i;
                Stack<string> stackTrace = GetRandomValue(stackTraces);
                List<KeyValuePair<string, string>> data = GetRandomValue(eventDatas);
                string level = GetRandomValue(levels);
                ContextInfoDto applicationState = GetRandomValue(applicationStates);
                EventDto? inner = null;
                //25% chance of grabbing previous document as inner event
                if (i > 0 && random.NextDouble() <= 0.25)
                    inner = documents.ElementAt(i - 1);

                EventDto eventDto = new()
                {
                    Id = id,
                    EliCode = eliCode,
                    Message = message,
                    ExceptionTime = exceptionTime,
                    StackTrace = stackTrace,
                    Data = data,
                    Level = level,
                    Context = applicationState,
                    Inner = inner,
                };

                documents.Add(eventDto);
            }

            TryDeleteIndex(eventIndex);

            TryCreateIndexAutoMap<EventDto>(eventIndex);

            TryBulkIndex<EventDto>(eventIndex, documents);
        }

        /// <summary>
        /// Gets a random document from an elasticsearch index
        /// </summary>
        /// <typeparam name="T">Type of document being queried for</typeparam>
        /// <param name="index">Name of the elasticsearch index being queried</param>
        /// <returns>Document received from elasticsearch</returns>
        private T GetRandomDocumentFromIndex<T>(Nest.IndexName index) where T : class
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

        private List<T> Get1000RandomDocsFromIndex<T>(Nest.IndexName index) where T : class
        {
            var response = _elasticClient.Search<T>(s => s
                .Index(index)
                .Size(1000)
                .Query(q => q
                    .FunctionScore(fs => fs
                        .Functions(f => f
                            .RandomScore())
                        .Query(fq => fq.MatchAll()))));

            return response.Documents.ToList();
        }

        /// <summary>
        /// Gets the elasticsearch-generated ID value for a random document
        /// </summary>
        /// <typeparam name="T">Type of object being queried for</typeparam>
        /// <param name="index">Name of the index being queried</param>
        /// <returns>String representing a document ID in the elasticsearch index</returns>
        public string GetRandomIdFromIndex<T>(Nest.IndexName index) where T : class
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

        /// <summary>
        /// Tries to delete an elasticsearch index by name
        /// </summary>
        /// <param name="index">Name of the index to be deleted</param>
        private void TryDeleteIndex(Nest.IndexName index)
        {
            try 
            {
                var deleteIndexResponse = _elasticClient.Indices.Delete(index);
            }
            catch (Exception e)
            {
                ExtractException ex = new("ELI54165", "Issue deleting index " + index, e);
                throw ex;
            }
        }

        /// <summary>
        /// Tries to create an elasticsearch index for the given object.
        /// </summary>
        /// <typeparam name="T">Type of the objects to be documented in new index</typeparam>
        /// <param name="index">Name for the newly created index</param>
        private void TryCreateIndexAutoMap<T>(Nest.IndexName index) where T : class
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

        /// <summary>
        /// Indexes a list of documents to a specified elasticsearch index.
        /// </summary>
        /// <typeparam name="T">Type of the documents being indexed</typeparam>
        /// <param name="index">Name of the index to upload documents to</param>
        /// <param name="documents">List of objects to be indexed</param>
        private void TryBulkIndex<T>(Nest.IndexName index, List<T> documents) where T : class
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

        /// <summary>
        /// Gets a random event from the test event index.
        /// </summary>
        /// <returns>Random event dto</returns>
        private EventDto GetRandomEvent()
        {
            try
            {
                var response = _elasticClient.Search<EventDto>(s => s
                .Index(eventIndex)
                //Need the most recent hit
                .Size(1)
                .Query(q => q
                    .FunctionScore(c => c
                        .Functions(f => f
                            .RandomScore(r => r.Seed("goodseed045")))
                        .ScoreMode(FunctionScoreMode.Sum))));

                return response.Hits.ElementAt(0).Source;
            }
            catch (Exception e)
            {
                ExtractException ex = new("ELI54197",
                    "Issue with random get from event index",
                    e);
                throw ex;
            }
        }

        /// <summary>
        /// Gets a random environment from the test environment index.
        /// </summary>
        /// <returns>Random environment dto</returns>
        private EnvironmentDto GetRandomEnvironment()
        {
            try
            {
                var response = _elasticClient.Search<EnvironmentDto>(s => s
                .Index(environmentIndex)
                //Need the most recent hit
                .Size(1)
                .Query(q => q
                    .FunctionScore(c => c
                        .Functions(f => f
                            .RandomScore(r => r.Seed("goodseed045")))
                        .ScoreMode(FunctionScoreMode.Sum))));

                return response.Hits.ElementAt(0).Source;
            }
            catch (Exception e)
            {
                ExtractException ex = new("ELI54198",
                    "Issue with random get from environment index",
                    e);
                throw ex;
            }
        }
    }
}