using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;


namespace Extract.Web.WebAPI.AppBackendTester
{
    class AppBackendTester
    {
        static string _textFilePrefix;
        static User _user;
        static string _url;
        static CancellationTokenSource _cancel = new CancellationTokenSource();
        static CancellationToken _cancelToken = _cancel.Token;

        static async Task Main(string[] args)
        {
            string root = ".";
            int batchSize = 50;
            string userName = null;
            string pwd = null;
            int pollingInterval = 10000;
            TimeSpan minTimeToRun = TimeSpan.FromMinutes(15);
            string workflow = null;

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
                        case "URL":
                            _url = args[++i];
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
            Log(FormattableString.Invariant($"BatchSize {batchSize}"));
            // This seems to make connections against a remote address behave like connections to localhost
            ServicePointManager.DefaultConnectionLimit = int.MaxValue;

            if (string.IsNullOrWhiteSpace(_url))
            {
                Log("No URL provided", true);
            }

            if (string.IsNullOrWhiteSpace(userName))
            {
                Log("No UserName provided", true);
            }

            if (string.IsNullOrWhiteSpace(pwd))
            {
                Log("No Password provided", true);
            }

            if (string.IsNullOrWhiteSpace(workflow))
            {
                Log("No Workflow provided", true);
            }

            _user = new User()
            {
                Username = userName,
                Password = pwd,
                WorkflowName = workflow
            };

            try
            {
                Console.CancelKeyPress += new ConsoleCancelEventHandler((o, e) =>
                {
                    e.Cancel = true;
                    _cancel.Cancel();
                });

                var t = new Stopwatch();
                t.Start();

                int totalTasksStarted = 0;
                int totalTasksCompleted = 0;

                _textFilePrefix = Guid.NewGuid().ToString("N");

                while (t.Elapsed < minTimeToRun)
                {
                    var (tasksStarted, tasksCompleted) = await ProcessAll(batchSize, pollingInterval, false, RunTests);
                    totalTasksStarted += tasksStarted;
                    totalTasksCompleted += tasksCompleted;
                }

                Log(FormattableString.Invariant($"Total sessions started: {totalTasksStarted}"));
                Log(FormattableString.Invariant($"Total sessions completed: {totalTasksCompleted}"));

                Log("Time elapsed: " + t.Elapsed);
            }
            catch (Exception ex)
            {
                Log(ex.Message);
            }
        }

        static async Task<AppBackendClient> Login()
        {
            HttpClient client = new HttpClient
            {
                // Set timeout to be higher than default to avoid the occasional TaskCanceledExceptions
                Timeout = TimeSpan.FromMinutes(5)
            };
            client.BaseAddress = new Uri(_url);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var backendClient = new AppBackendClient(_url, client);

            var tokenResult = await backendClient.LoginAsync(_user);

            var token = tokenResult.Access_token;
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            Log(FormattableString.Invariant($"Logged in, token ...{token.Last(43)}"));

            return backendClient;
        }

        static async Task<(int tasksStarted, int tasksCompleted)> ProcessAll(int batchSize, int pollingInterval, bool processRandomFileIDs,
            Func<int, AppBackendClient, int, Task> operation)
        {
            int tasksStarted = 0;
            int tasksCompleted = 0;

            var runningTasks = new List<Task>();
            for (int i = 0; i < batchSize; i++)
            {
                try
                {
                    var client = await Login();

                    var id = -1;
                    while (id < 0)
                    {
                        var idResult = await KeepTrying(() => client.OpenDocumentAsync(-1), -1, pollingInterval);
                        id = idResult.Id;
                        if (id > 0)
                        {
                            tasksStarted++;
                            Log(FormattableString.Invariant($"File ID {id} opened for token {client.ShortID}"));
                            runningTasks.Add(operation(id, client, pollingInterval));
                        }
                        else
                        {
                            Log(FormattableString.Invariant($"No files available for token {client.ShortID}"));
                            await Task.Delay(pollingInterval);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log(ex.Message, true);
                }
            }

            while (runningTasks.Count > 0)
            {
                var finished = await Task.WhenAny(runningTasks);

                runningTasks.Remove(finished);
                if (finished.IsFaulted)
                {
                    Log(FormattableString.Invariant($"FAILURE {finished.Exception}"), true);
                }
                else if (finished.IsCanceled)
                {
                    Log("FAILURE, CANCELLED", true);
                }
                else
                {
                    tasksCompleted++;
                }
            }
            Log(FormattableString.Invariant($"Sessions started: {tasksStarted}"));
            Log(FormattableString.Invariant($"Sessions completed: {tasksCompleted}"));
            return (tasksStarted, tasksCompleted);
        }

        static async Task RunTests(int id, AppBackendClient client, int pollingInterval)
        {
            DocumentDataResult documentData = null;
            PagesInfoResult info = null;
            Random pageGen = new Random();
            Stopwatch sw = new Stopwatch();
            sw.Start();

            void ResetStateVars()
            {
                documentData = null;
                info = null;
                pageGen = new Random();
                sw = new Stopwatch();
                sw.Start();
            }

            async Task GetDataTest()
            {
                await Log(FormattableString.Invariant($"Getting data for document ID {id} for token {client.ShortID}"), async () =>
                    documentData = await client.GetDocumentDataAsync(id));
            }

            async Task GetInfoTest()
            {
                await Log(FormattableString.Invariant($"Getting info for document ID {id} for token {client.ShortID}"), async () =>
                    info = await client.GetPageInfoAsync(id));
            }

            async Task GetPageTest()
            {
                var page = info == null ? 1 : pageGen.Next(1, info.PageCount + 1);
                await Log(FormattableString.Invariant($"Getting page data for page {page} of document ID {id} for token {client.ShortID}"), async () =>
                    await client.GetDocumentPageAsync(id, page));
            }

            async Task SaveDocumentTest()
            {
                await Log(FormattableString.Invariant($"Saving document ID {id} for token {client.ShortID}"), async () =>
                    await client.SaveDocumentDataAsync(id, new DocumentDataInput(documentData.Attributes)));
            }

            async Task PageWordZonesTest()
            {
                var page = info == null ? 1 : pageGen.Next(1, info.PageCount + 1);
                await Log(FormattableString.Invariant($"Getting word zones from page {page} of document ID {id} for token {client.ShortID}"), async () =>
                    await client.GetPageWordZonesAsync(id, page));
            }

            async Task QueueStatusTest()
            {
                await Log(FormattableString.Invariant($"Getting queue status for token {client.ShortID}"), async () =>
                    await client.GetQueueStatusAsync(id));
            }

            async Task SettingsTest()
            {
                await Log(FormattableString.Invariant($"Getting settings for token {client.ShortID}"), async () =>
                    await client.GetSettingsAsync());
            }

            // These tests will always come last, and in order
            async Task CloseDocumentTest()
            {
                await Log(FormattableString.Invariant($"Closing document ID {id} for token {client.ShortID}"), async () =>
                    await client.CloseDocumentAsync(id, true, (int)sw.ElapsedMilliseconds));
            }

            async Task GetNextDocumentTest()
            {
                await Log(FormattableString.Invariant($"Getting next document for token {client.ShortID}"), async () =>
                {
                    var idResult = await client.OpenDocumentAsync(-1);
                    id = idResult.Id;
                });
                Log(FormattableString.Invariant($"Next document ID {id} for token {client.ShortID}"));
            }

            // This will be run after no next file is available
            async Task LogoutTest()
            {
                await Log(FormattableString.Invariant($"Logging out token {client.ShortID}"), async () =>
                    await client.LogoutAsync());
            }

            var tests = new List<(Func<Task> test, string description)>
            {
                (GetDataTest, "Getting document data for ID"),
                (GetInfoTest, "Getting pages info for ID"),
                (GetPageTest, "Getting a page of ID"),
                (SaveDocumentTest, "Saving document ID"),
                (PageWordZonesTest, "Getting page word zones for document ID"),
                (QueueStatusTest, "Getting queue status while processing document ID"),
                (SettingsTest, "Getting settings while processing document ID"),
            };

            // Randomly sort but make sure GetDataTest comes first
            Random rng = new Random();
            tests = tests.OrderBy(pair =>
            {
                if (pair.test == GetDataTest)
                {
                    return -1;
                }
                else
                {
                    return rng.Next();
                }
            }).ToList();

            Log(FormattableString.Invariant($"fileID: {id}, test order: {string.Join("|", tests.Select(t => SimpleName(t.test)))}"));

            try
            {
                _cancelToken.ThrowIfCancellationRequested();
                while (id > 0)
                {
                    foreach (var pair in tests)
                    {
                        try
                        {
                            await KeepTrying(pair.test, id, pollingInterval, client);
                            _cancelToken.ThrowIfCancellationRequested();
                        }
                        catch (Exception ex)
                        {
                            throw new Exception(FormattableString.Invariant($"Unexpected exception caught running {SimpleName(pair.test)} on fileID {id}"), ex);
                        }
                    }

                    await KeepTrying(CloseDocumentTest, id, pollingInterval, client);

                    _cancelToken.ThrowIfCancellationRequested();

                    ResetStateVars();
                    await KeepTrying(GetNextDocumentTest, id, pollingInterval, client);

                    _cancelToken.ThrowIfCancellationRequested();
                }
            }
            finally
            {
                await LogoutTest();
            }
        }

        public static void Log(string message, bool error = false)
        {
            if (error)
            {
                Console.Error.WriteLine(FormattableString.Invariant($"{DateTime.Now}: {message}"));
                Trace.TraceError(FormattableString.Invariant($"{DateTime.Now}: {message}"));
            }
            else
            {
                Console.WriteLine(FormattableString.Invariant($"{DateTime.Now}: {message}"));
                Trace.WriteLine(FormattableString.Invariant($"{DateTime.Now}: {message}"));
            }
        }

        public static void Log(string message, Action func)
        {
            Console.WriteLine(FormattableString.Invariant($"{DateTime.Now}:      {message}"));
            Trace.WriteLine(FormattableString.Invariant($"{DateTime.Now}:      {message}"));
            func();
            Console.WriteLine(FormattableString.Invariant($"{DateTime.Now}: Done {message.Substring(0, 1).ToLower() + message.Substring(1)}"));
            Trace.WriteLine(FormattableString.Invariant($"{DateTime.Now}: Done {message.Substring(0, 1).ToLower() + message.Substring(1)}"));
        }

        public static async Task Log(string message, Func<Task> func)
        {
            Console.WriteLine(FormattableString.Invariant($"{DateTime.Now}:      {message}"));
            Trace.WriteLine(FormattableString.Invariant($"{DateTime.Now}:      {message}"));

            await func();

            Console.WriteLine(FormattableString.Invariant($"{DateTime.Now}: Done {message.Substring(0, 1).ToLower() + message.Substring(1)}"));
            Trace.WriteLine(FormattableString.Invariant($"{DateTime.Now}: Done {message.Substring(0, 1).ToLower() + message.Substring(1)}"));
        }

        public static async Task<T> Log<T>(string message, Func<Task<T>> func)
        {
            Console.WriteLine(FormattableString.Invariant($"{DateTime.Now}:      {message}"));
            Trace.WriteLine(FormattableString.Invariant($"{DateTime.Now}:      {message}"));

            var ret = await func();

            Console.WriteLine(FormattableString.Invariant($"{DateTime.Now}: Done {message.Substring(0, 1).ToLower() + message.Substring(1)}"));
            Trace.WriteLine(FormattableString.Invariant($"{DateTime.Now}: Done {message.Substring(0, 1).ToLower() + message.Substring(1)}"));

            return ret;
        }

        public static string SimpleName<T>(Func<T> func)
        {
            try
            {
                return func.Method.Name.Split(new[] { '_', '|' })[2];
            }
            catch (NullReferenceException)
            {
                return "UnknownMethodName:" + (func?.Method?.Name ?? "");
            }
        }

        static async Task KeepTrying(Func<Task> action, int fileID, int pollingInterval, AppBackendClient client)
        {
            bool success = false;
            while (!success)
            {
                try
                {
                    await action();
                    success = true;
                    break;
                }
                catch (SwaggerException ex) when (ex.StatusCode == 500 && ex.Message.Contains("Timeout waiting to process request"))
                {
                    Log(FormattableString.Invariant($"Handled 500, 'timeout...' exception for ID {fileID}. Retrying in {pollingInterval}ms..."));
                }
                catch (SwaggerException ex) when (ex.StatusCode == 500)
                {
                    Log(FormattableString.Invariant($"Handled 500 exception for ID {fileID} for token {client.ShortID}. Retrying in {pollingInterval}ms..."));
                }
                catch (SwaggerException ex) when (ex.StatusCode == 502)
                {
                    Log(FormattableString.Invariant($"Handled 502 exception for ID {fileID}. Retrying in {pollingInterval}ms..."));
                }
                catch (SwaggerException ex) when (ex.StatusCode == 503)
                {
                    Log(FormattableString.Invariant($"Handled 503 exception for ID {fileID}. Retrying in {pollingInterval}ms..."));
                }
                catch (OperationCanceledException)
                {
                    Log(FormattableString.Invariant($"Handled OperationCanceledException for ID {fileID}. Retrying in {pollingInterval}ms..."));
                }

                await Task.Delay(pollingInterval);
            }
        }

        static async Task<T> KeepTrying<T>(Func<Task<T>> func, int fileID, int pollingInterval)
        {
            T result = default;
            bool success = false;
            while (!success)
            {
                try
                {
                    result = await func();
                    success = true;
                    break;
                }
                catch (SwaggerException ex) when (ex.StatusCode == 500 && ex.Message.Contains("Timeout waiting to process request"))
                {
                    Log(FormattableString.Invariant($"Handled 500, 'timeout...' exception for ID {fileID}. Retrying in {pollingInterval}ms..."));
                }
                catch (SwaggerException ex) when (ex.StatusCode == 500)
                {
                    Log(FormattableString.Invariant($"Handled 500 exception for ID {fileID}. Retrying in {pollingInterval}ms..."));
                }
                catch (SwaggerException ex) when (ex.StatusCode == 502)
                {
                    Log(FormattableString.Invariant($"Handled 502 exception for ID {fileID}. Retrying in {pollingInterval}ms..."));
                }
                catch (SwaggerException ex) when (ex.StatusCode == 503)
                {
                    Log(FormattableString.Invariant($"Handled 503 exception for ID {fileID}. Retrying in {pollingInterval}ms..."));
                }
                catch (OperationCanceledException)
                {
                    Log(FormattableString.Invariant($"Handled OperationCanceledException for ID {fileID}. Retrying in {pollingInterval}ms..."));
                }

                await Task.Delay(pollingInterval);
            }
            return result;
        }
    }

    public partial class AppBackendClient
    {
        public string BearerToken => _httpClient.DefaultRequestHeaders.Authorization.Parameter;

        public string ShortID => "..." + BearerToken.Last(43);
    }

    public partial class DocumentDataInput
    {
        public DocumentDataInput(ICollection<DocumentAttribute> attributes)
        {
            Attributes = attributes;
        }
    }

    public static class Extensions
    {
        public static string Last(this string x, int length)
        {
            if (length >= x.Length)
            {
                return x;
            }

            return x.Substring(x.Length - length);
        }
    }
}
