using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Extract.Web.WebAPI.DocumentAPISubmit
{
    class Program
    {
        static void Main(string[] args)
        {
            string root = ".";
            string urlFile = "url.txt";
            string tokenFile = "token.txt";
            string url = null;
            string token = null;
            int batchSize = 50;
            string userName = null;
            string pwd = null;
            int pollingInterval = 10000;

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
                        case "URLFILE":
                            urlFile = args[++i];
                            continue;

                        case "URL":
                            url = args[++i];
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
                    }
                }

                root = arg;
            }

            HttpClient client = new HttpClient();
            url = url ?? File.ReadAllText(urlFile).Trim();
            client.BaseAddress = new Uri(url);

            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            var docClient = new DocumentClient(url, client);
            var userClient = new UsersClient(url, client);
            var workflowClient = new WorkflowClient(url, client);

            if (userName != null && pwd != null)
            {
                var user = new User()
                {
                    Username = userName,
                    Password = pwd
                };
                var tokenResult = userClient.LoginAsync(user).GetAwaiter().GetResult();
                token = tokenResult.Access_token;
            }

            token = token ?? File.ReadAllText(tokenFile).Trim();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var t = new Stopwatch();
            t.Start();

            var files = Directory.EnumerateFiles(root, "*.*", SearchOption.AllDirectories)
                .Where(f =>
                {
                    var ext = Path.GetExtension(f).ToUpperInvariant();
                    return ext == ".PDF" || ext == ".TIF";
                });

            // Process files with only one task working on any particular file
            int tasksStarted2 = 0;
            int tasksCompleted2 = 0;
            for (int i = 0; i < 50; i++)
            {
                var (tasksStarted, tasksCompleted) = ProcessAll(docClient, workflowClient, files, batchSize, pollingInterval, false, RunTests).GetAwaiter().GetResult();
                tasksStarted2 += tasksStarted;
                tasksCompleted2 += tasksCompleted;
            }

            // Process files where random files in the valid range are processed (so that it is possible that the same
            // file is processed by multiple threads at a time)
           // var (tasksStarted2, tasksCompleted2) = ProcessAll(docClient, workflowClient, files, batchSize, pollingInterval, true, RunTests).GetAwaiter().GetResult();

            Log(FormattableString.Invariant($"Total tasks started: {tasksStarted2}"));
            Log(FormattableString.Invariant($"Total tasks completed: {tasksCompleted2}"));

            Log("Time elapsed: " + t.Elapsed);
        }

        static async Task<(int tasksStarted, int tasksCompleted)> ProcessAll(DocumentClient docClient, WorkflowClient workflowClient, IEnumerable<string> files,
            int batchSize, int pollingInterval, bool processRandomFileIDs,
            Func<int, string, DocumentClient, WorkflowClient, int, Task> operation)
        {
            int tasksStarted = 0;
            int tasksCompleted = 0;
            Random rng = new Random();
            var tasks = files
                .Select(async file =>
                {
                    tasksStarted++;
                    var (fileName, id) = await PostAsync(docClient, Path.GetFullPath(file));
                    var fileID = processRandomFileIDs
                    ? rng.Next(1, tasksStarted + 1)
                    : id;
                    await operation(id, fileName, docClient, workflowClient, pollingInterval);
                });

            var waitingTasksEnum = tasks.GetEnumerator();
            var runningTasks = new List<Task>();
            for (int i = 0; i < batchSize; i++)
            {
                if (waitingTasksEnum.MoveNext())
                {
                    runningTasks.Add(waitingTasksEnum.Current);
                }
                else
                {
                    break;
                }
            }

            while (runningTasks.Count > 0)
            {
                var finished = await Task.WhenAny(runningTasks);
                runningTasks.Remove(finished);
                if (finished.IsFaulted)
                {
                    // Accept that files will be deleted when testing random file IDs
                    if (processRandomFileIDs
                        && finished.Exception.InnerExceptions
                            .OfType<SwaggerException>()
                            .Any(ex => ex.Response.Contains("File not in the workflow")))
                    {
                        Log("Exception about deleted file caught while processing random IDs");
                        tasksCompleted++;
                    }
                    else
                    {
                        Log(FormattableString.Invariant($"FAILURE {finished.Exception}"), true);
                    }
                }
                else
                {
                    tasksCompleted++;
                }

                if (waitingTasksEnum.MoveNext())
                {
                    runningTasks.Add(waitingTasksEnum.Current);
                }
            }
            Log(FormattableString.Invariant($"Tasks started: {tasksStarted}"));
            Log(FormattableString.Invariant($"Tasks completed: {tasksCompleted}"));
            return (tasksStarted, tasksCompleted);
        }

        static async Task RunTests(int id, string fileName, DocumentClient docClient, WorkflowClient workflowClient, int pollingInterval)
        {
            DocumentDataResult origData = null;

            async Task GetDataTest()
            {
                origData = null;
                int attempts = 0;

                Naive:
                Log(FormattableString.Invariant($"Naive attempt {++attempts} for {fileName}"));
                try
                {
                    origData = await docClient.GetDocumentDataAsync(id);
                }
                catch (Exception)
                {
                    Log(FormattableString.Invariant($"Naive attempt {attempts} failed with exception"));
                    origData = null;
                    if (attempts < 10)
                        goto Naive;
                }

                if (origData != null)
                {
                    Log("Successful naive get-data for " + fileName);
                }
                else
                {
                    await Log("Waiting for " + fileName, async () =>
                        await PollForCompletion(docClient, id, pollingInterval));

                    origData = await Log("Getting data " + fileName, async () =>
                        await docClient.GetDocumentDataAsync(id));
                }

                Log("Writing " + fileName + ".json", () =>
                    File.WriteAllText(fileName + ".json", origData.ToJson()));
            }

            async Task PatchDataTest()
            {
                var attr = origData.Attributes.FirstOrDefault();
                if (attr != null)
                {
                    var attrPatch = new DocumentAttributePatch(attr) { Operation = PatchOperation.Update };
                    attrPatch.Name = "Blah";
                    var patch = new DocumentDataPatch
                    {
                        Attributes = new[] { attrPatch }
                    };

                    await Log("Patching " + fileName, async () =>
                        await docClient.PatchDocumentDataAsync(id, patch));

                    var patchedData = await Log("Getting patched data for " + fileName, async () =>
                        await docClient.GetDocumentDataAsync(id));

                    var newName = fileName + ".patched.json";
                    Log("Writing " + newName, () =>
                        File.WriteAllText(newName, patchedData.ToJson()));
                }
                else
                {
                    Log(FormattableString.Invariant($"WWWWWHHHHHHATTTT! WHY NO ATTR? File: {fileName}"), true);
                }
            }

            async Task WipeDataTest()
            {
                var input = new DocumentDataInput
                {
                    Attributes = new DocumentAttribute[0]
                };

                await Log("Wiping out data for " + fileName, async () =>
                    await docClient.PutDocumentDataAsync(id, input));

                var wipedData = await Log("Getting wiped data for " + fileName, async () =>
                    await docClient.GetDocumentDataAsync(id));

                var newName = fileName + ".wiped.json";
                Log("Writing " + newName, () =>
                    File.WriteAllText(newName, wipedData.ToJson()));
            }

            async Task RestoreDataTest()
            {
                var input = new DocumentDataInput
                {
                    Attributes = origData.Attributes
                };

                await Log("Restoring data for " + fileName, async () =>
                    await docClient.PutDocumentDataAsync(id, input));

                var restoredData = await Log("Getting restored data for " + fileName, async () =>
                    await docClient.GetDocumentDataAsync(id));

                var newName = fileName + ".restored.json";
                Log("Writing " + newName, () =>
                    File.WriteAllText(newName, restoredData.ToJson()));
            }

            async Task DeleteDocumentTest()
            {
                await Log("Deleting " + fileName, async () =>
                    await docClient.DeleteDocumentAsync(id));
            }

            await GetDataTest();
            await PatchDataTest();
            await WipeDataTest();
            await RestoreDataTest();

            await DeleteDocumentTest();
            try
            {
                await RestoreDataTest();
                throw new Exception("Expected an exception restoring data to deleted file!");
            }
            catch (SwaggerException ex) when (ex.Response.Contains("File not in the workflow"))
            {
                Log("Expected exception caught");
            }

            try
            {
                await PatchDataTest();
                throw new Exception("Expected an exception patching data to deleted file!");
            }
            catch (SwaggerException ex) when (ex.Response.Contains("File not in the workflow"))
            {
                Log("Expected exception caught");
            }

            try
            {
                await GetDataTest();
                throw new Exception("Expected an exception getting data from a deleted file!");
            }
            catch (SwaggerException ex) when (ex.Response.Contains("File not in the workflow"))
            {
                Log("Expected exception caught");
            }
        }


        static async Task<(string fileName, int id)> PostAsync(DocumentClient client, string filename)
        {
            using (var stream = File.OpenRead(filename))
            {
                var file = new FileParameter(stream, filename);
                var result = await client.PostDocumentAsync(file);
                var output = DateTime.Now.ToLongTimeString() + " Created: " + result.Id;

                Log(output);

                return (filename, result.Id);
            }
        }

        static async Task PollForCompletion(DocumentClient client, int id, int pollingInterval = 10000)
        {
            while (true)
            {
                var result = await client.GetStatusAsync(id);

                if (result.DocumentStatus == DocumentProcessingStatus.Processing)
                {
                    await Task.Delay(pollingInterval);
                }
                else if (result.DocumentStatus == DocumentProcessingStatus.Done)
                {
                    return;
                }
                else
                {
                    var ex = new InvalidOperationException("Unexpected status: " + result.StatusText);
                    Log(ex.Message, true);
                    throw ex;
                }
            }
        }

        public static void Log(string message, bool error = false)
        {
            if (error)
            {
                Console.Error.WriteLine(message);
                Trace.TraceError(message);
            }
            else
            {
                Console.WriteLine(message);
                Trace.WriteLine(message);
            }
        }

        public static void Log(string message, Action func)
        {
            Console.WriteLine(message);
            Trace.WriteLine(message);
            func();
            Console.WriteLine("Done " + message.Substring(0, 1).ToLower() + message.Substring(1));
            Trace.WriteLine("Done " + message.Substring(0, 1).ToLower() + message.Substring(1));
        }

        public static async Task Log(string message, Func<Task> func)
        {
            Console.WriteLine(message);
            Trace.WriteLine(message);

            await func();

            Console.WriteLine("Done " + message.Substring(0, 1).ToLower() + message.Substring(1));
            Trace.WriteLine("Done " + message.Substring(0, 1).ToLower() + message.Substring(1));
        }

        public static async Task<T> Log<T>(string message, Func<Task<T>> func)
        {
            Console.WriteLine(message);
            Trace.WriteLine(message);

            var ret = await func();

            Console.WriteLine("Done " + message.Substring(0, 1).ToLower() + message.Substring(1));
            Trace.WriteLine("Done " + message.Substring(0, 1).ToLower() + message.Substring(1));

            return ret;
        }
    }

    public partial class DocumentAttributePatch : DocumentAttributeCore
    {
        public DocumentAttributePatch(DocumentAttribute attribute)
        {
            ConfidenceLevel = attribute.ConfidenceLevel;
            HasPositionInfo = attribute.HasPositionInfo;
            ID = attribute.ID;
            Name = attribute.Name;
            SpatialPosition = attribute.SpatialPosition;
            Type = attribute.Type;
            Value = attribute.Value;
        }
    }
}
