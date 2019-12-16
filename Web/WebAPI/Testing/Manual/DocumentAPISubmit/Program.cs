using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Extract.Web.WebAPI.DocumentAPISubmit
{
    class Program
    {
        class Settings
        {
            public string root = ".";
            public string docApiUrlFile = null;
            public string docApiUrl = null;
            public string backendApiUrlFile = null;
            public string backendApiUrl = null;
            public string tokenFile = "token.txt";
            public string token = null;
            public int batchSize = 50;
            public string userName = null;
            public string pwd = null;
            public int pollingInterval = 3000;
            public int maxRetries = 10;
            public int naiveAttempts = 10;
            public bool processText = true;
            public bool reprocessRandom = true;
            public TimeSpan minTimeToRun = TimeSpan.FromMinutes(15);
            public string workflow = null;
            public bool testDocumentApi;
            public bool testBackendApi;

            public Settings(string[] args)
            {
                for (int i = 0; i < args.Length; i++)
                {
                    var arg = args[i];
                    var flag = arg.ToUpperInvariant();
                    if (flag.StartsWith("-") || flag.StartsWith("/")
                        && (i + 1) < args.Length)
                    {
                        flag = flag.Substring(1);
                        switch (flag)
                        {
                            case "DOCURLFILE":
                                docApiUrlFile = args[++i];
                                continue;

                            case "DOCURL":
                                docApiUrl = args[++i];
                                continue;

                            case "BACKENDURLFILE":
                                backendApiUrlFile = args[++i];
                                continue;

                            case "BACKENDURL":
                                backendApiUrl = args[++i];
                                continue;

                            case "TOKENFILE":
                                tokenFile = args[++i];
                                continue;

                            case "TOKEN":
                                token = args[++i];
                                continue;

                            case "BATCHSIZE":
                                batchSize = int.Parse(args[++i]);
                                continue;

                            case "USER":
                                userName = args[++i];
                                continue;

                            case "PWD":
                                pwd = args[++i];
                                continue;

                            case "POLLINT":
                                pollingInterval = int.Parse(args[++i]);
                                continue;

                            case "MAXRETRIES":
                                maxRetries = int.Parse(args[++i]);
                                continue;

                            case "NAIVEATTEMPTS":
                                naiveAttempts = int.Parse(args[++i]);
                                continue;

                            case "PROCESSTEXT":
                                processText = bool.Parse(args[++i]);
                                continue;

                            case "REPROCESSRANDOM":
                                reprocessRandom = bool.Parse(args[++i]);
                                continue;

                            case "MINTIME":
                                minTimeToRun = TimeSpan.Parse(args[++i]);
                                continue;

                            case "WORKFLOW":
                                workflow = args[++i];
                                continue;
                        }
                    }

                    root = arg;
                }

                testDocumentApi = !string.IsNullOrWhiteSpace(docApiUrl)
                    || !string.IsNullOrWhiteSpace(docApiUrlFile);

                testBackendApi = !string.IsNullOrWhiteSpace(backendApiUrl)
                    || !string.IsNullOrWhiteSpace(backendApiUrlFile);

                if (testDocumentApi && processText)
                {
                    processText = false;
                    Log("Text processing is disabled to test backend API");
                }
            }
        }

        static async Task Main(string[] args)
        {
            await new SynchronizationContextRemover();

            var _settings = new Settings(args);
            User user = null;
            string _auth = null;

            HttpClient docHttpClient = null;
            DocumentClient docClient = null;
            UsersClient userClient = null;
            WorkflowClient workflowClient = null;
            if (_settings.testDocumentApi)
            {
                docHttpClient = CreateHttpClient(_settings.docApiUrl);
                docClient = new DocumentClient(_settings.docApiUrl, docHttpClient);
                userClient = new UsersClient(_settings.docApiUrl, docHttpClient);
                workflowClient = new WorkflowClient(_settings.docApiUrl, docHttpClient);
            }

            HttpClient backendHttpClient = null;
            AppBackendClient backendClient = null;
            if (_settings.testBackendApi)
            {
                backendHttpClient = CreateHttpClient(_settings.backendApiUrl);
                backendClient = new AppBackendClient(_settings.backendApiUrl, backendHttpClient);
            }

            if (_settings.userName != null && _settings.pwd != null)
            {
                user = new User()
                {
                    Username = _settings.userName,
                    Password = _settings.pwd,
                    WorkflowName = _settings.workflow
                };

            }
            else
            {
                _auth = _auth ?? "Bearer " + File.ReadAllText(_settings.tokenFile).Trim();
            }

            var t = new Stopwatch();
            t.Start();

            IEnumerable<string> GetFiles(params string[] extensions)
            {
                var extSet = new HashSet<string>(extensions, StringComparer.OrdinalIgnoreCase);
                var files = Directory.EnumerateFiles(_settings.root, "*.*", SearchOption.AllDirectories);
                foreach (var file in files)
                {
                    var ext = Path.GetExtension(file);
                    if (extSet.Contains(ext))
                    {
                        yield return file;
                    }
                }
            }

            int totalTasksStarted = 0;
            int totalTasksCompleted = 0;

            while (t.Elapsed < _settings.minTimeToRun!)
            {
                Log($"------------ {DateTime.Now} Starting new test set ------------");
                var t2 = new Stopwatch();
                t2.Start();

                // Process files with only one task working on any particular file at a time.
                // NOTE: If processText, text files corresponding the document text of each source image will be 
                // generated as a side-effect of running this process, so pre-existing text files do not need to be supplied
                var files = _settings.processText ? GetFiles(".tif", ".pdf", ".txt") : GetFiles(".tif", ".pdf");
                var (tasksStarted, tasksCompleted) = RunTests(
                    user, _auth, docClient, workflowClient, backendClient, userClient, files, _settings)
                    .GetAwaiter().GetResult();
                totalTasksStarted += tasksStarted;
                totalTasksCompleted += tasksCompleted;

                Log($"------------ Test set complete ------------");
                Log($"Tasks started: {tasksStarted}");
                Log($"Tasks completed: {tasksCompleted}");
                Log($"Time elapsed: {t2.Elapsed}");
                Log($"Total tasks started: {totalTasksStarted}");
                Log($"Total tasks completed: {totalTasksCompleted}");
                Log($"Total time elapsed: {t.Elapsed}");
            }

            Log($"Total tasks started: {totalTasksStarted}");
            Log($"Total tasks completed: {totalTasksCompleted}");
            Log($"Time elapsed: {t.Elapsed}");
            Log("TESTING COMPLETE");
        }

        static async Task<(int tasksStarted, int tasksCompleted)> RunTests(User user, string auth,
            DocumentClient docClient, WorkflowClient workflowClient, AppBackendClient backendClient, UsersClient userClient,
            IEnumerable<string> files, Settings settings)
        {
            int tasksStarted = 0;
            int tasksCompleted = 0;
            var taskQueue = new ConcurrentQueue<(ApiTester tester, Func<Task> task)>();

            if (settings.testDocumentApi)
            {
                var activeDocumentAPITesters = new ConcurrentDictionary<int, DocumentAPITests>();
                Random rng = new Random();
                var testers = files
                    .Select(file =>
                    {
                        var fileName = Path.GetFullPath(file);
                        DocumentAPITests documentApiTester = new DocumentAPITests(
                            fileName, user, auth, docClient, workflowClient, userClient, 
                            settings.pollingInterval, settings.maxRetries, settings.naiveAttempts, settings.processText);

                        return documentApiTester;
                    });

                foreach (var documentApiTester in testers)
                {
                    taskQueue.Enqueue((tester: documentApiTester, task: async () =>
                    {
                        var taskNumber = Interlocked.Increment(ref tasksStarted);
                        await documentApiTester.PostDocument();

                        activeDocumentAPITesters.TryAdd(taskNumber, documentApiTester);
                        if (settings.reprocessRandom
                            && activeDocumentAPITesters.TryGetValue(rng.Next(1, activeDocumentAPITesters.Count), out var randomTester))
                        {
                            taskQueue.Enqueue((tester: randomTester, task: async () =>
                            {
                                randomTester.reprocessing = true;
                                await randomTester.RunMainSequenceTests("Reprocessing");
                            }));
                        }

                        await documentApiTester.RunMainSequenceTests();
                        taskQueue.Enqueue((tester: documentApiTester, task: async () =>
                        {
                            await documentApiTester.RunDeletionTests();
                            Interlocked.Increment(ref tasksCompleted);
                        }));
                    }));

                    if (settings.testBackendApi)
                    {
                        var backendApiTester = new AppBackendAPITests(
                            user, auth, backendClient, settings.pollingInterval, settings.maxRetries);

                        taskQueue.Enqueue((tester: backendApiTester, task: async () =>
                        {
                            await backendApiTester.GetSessionAsync();
                            await backendApiTester.RunDocumentTests();
                        }));
                    }
                }
            }
            else if (settings.testBackendApi)
            {
                for (int i = 0; i < settings.batchSize; i++)
                {
                    var backendApiTester = new AppBackendAPITests(
                        user, auth, backendClient, settings.pollingInterval, settings.maxRetries);

                    taskQueue.Enqueue((tester: backendApiTester, task: async () =>
                    {
                        Interlocked.Increment(ref tasksStarted);
                        await backendApiTester.GetSessionAsync();
                        await backendApiTester.RunDocumentTests();
                        Interlocked.Increment(ref tasksCompleted);
                    }));
                }
            }

            var runningTasks = new List<Task>();
            while (runningTasks.Count() < settings.batchSize && taskQueue.TryDequeue(out var task))
            {
                runningTasks.Add(task.task.Invoke());
            }

            while (runningTasks.Count > 0)
            {
                var finished = await Task.WhenAny(runningTasks);
                runningTasks.Remove(finished);

                if (taskQueue.TryDequeue(out var task))
                {
                    runningTasks.Add(task.task.Invoke());
                }
            }

            return (tasksStarted, tasksCompleted);
        }

        static HttpClient CreateHttpClient(string url)
        {
            HttpClient httpClient = new HttpClient
            {
                // Set timeout to be higher than default to avoid the occasional TaskCanceledExceptions
                Timeout = TimeSpan.FromMinutes(5)
            };
            // This seems to make connections against a remote address behave like connections to localhost
            ServicePointManager.DefaultConnectionLimit = int.MaxValue;

            httpClient.BaseAddress = new Uri(url);

            httpClient.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            return httpClient;
        }

        static void Log(string message)
        {
            Console.WriteLine(message);
            Trace.WriteLine(message);
        }
    }
}
