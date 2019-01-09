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
        static Func<Task> _login;
        static string _textFilePrefix;

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

            HttpClient client = new HttpClient
            {
                // Set timeout to be higher than default to avoid the occasional TaskCanceledExceptions
                Timeout = TimeSpan.FromMinutes(5)
            };
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
                    Password = pwd,
                    WorkflowName = workflow
                };

                _login = async () =>
                {
                    var tokenResult = await userClient.LoginAsync(user);
                    token = tokenResult.Access_token;
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                };

                _login().GetAwaiter().GetResult();
            }
            else
            {
                token = token ?? File.ReadAllText(tokenFile).Trim();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            var t = new Stopwatch();
            t.Start();

            IEnumerable<string> GetFiles(params string[] extensions)
            {
                var extSet = new HashSet<string>(extensions, StringComparer.OrdinalIgnoreCase);
                var files = Directory.EnumerateFiles(root, "*.*", SearchOption.AllDirectories);
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

            _textFilePrefix = Guid.NewGuid().ToString("N");

            // Cache text file enumeration because otherwise the list grow exponentially
            string[] textFiles = null;
            while(t.Elapsed < minTimeToRun)
            {
                // Process files with only one task working on any particular file
                var (tasksStarted, tasksCompleted) = ProcessAll(docClient, workflowClient, GetFiles(".tif", ".pdf"), batchSize, pollingInterval, false, RunTests).GetAwaiter().GetResult();
                totalTasksStarted += tasksStarted;
                totalTasksCompleted += tasksCompleted;

                textFiles = textFiles ?? GetFiles(".txt").Where(f => f.StartsWith(_textFilePrefix)).ToArray();
                (tasksStarted, tasksCompleted) = ProcessAll(docClient, workflowClient, textFiles, batchSize, pollingInterval, false, RunTests).GetAwaiter().GetResult();
                totalTasksStarted += tasksStarted;
                totalTasksCompleted += tasksCompleted;

                // Process files where random files in the valid range are processed (so that it is possible that the same
                // file is processed by multiple threads at a time)
                (tasksStarted, tasksCompleted) = ProcessAll(docClient, workflowClient, GetFiles(".tif", ".pdf"), batchSize, pollingInterval, true, RunTests).GetAwaiter().GetResult();
                totalTasksStarted += tasksStarted;
                totalTasksCompleted += tasksCompleted;

                (tasksStarted, tasksCompleted) = ProcessAll(docClient, workflowClient, textFiles, batchSize, pollingInterval, true, RunTests).GetAwaiter().GetResult();
                totalTasksStarted += tasksStarted;
                totalTasksCompleted += tasksCompleted;
            }

            Log(FormattableString.Invariant($"Total tasks started: {totalTasksStarted}"));
            Log(FormattableString.Invariant($"Total tasks completed: {totalTasksCompleted}"));

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
                            .Any(ex => ex.Response.Contains("File not in the workflow") || ex.Response.Contains("Attribute not found")))
                    {
                        Log("Exception about deleted file caught while processing random IDs");
                        tasksCompleted++;
                    }
                    else
                    {
                        Log(FormattableString.Invariant($"FAILURE at {DateTime.Now} {finished.Exception}"), true);
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
            bool fileIsText = Path.GetExtension(fileName).Equals(".txt", StringComparison.OrdinalIgnoreCase);
            DocumentDataResult origData = null;
            bool docDataAllExist = false;
            string text = " ";
            int docDataCount = 0;

            void ResetStateVars()
            {
                docDataAllExist = false;
                docDataCount = 0;
            }

            async Task GetDataTest()
            {
                DocumentDataResult data = null;
                int attempts = 0;

                Naive:
                Log(FormattableString.Invariant($"Naive attempt {++attempts} for {fileName}"));
                try
                {
                    data = await docClient.GetDocumentDataAsync(id);
                }
                catch (Exception)
                {
                    Log(FormattableString.Invariant($"Naive attempt {attempts} failed with exception"));
                    data = null;
                    if (attempts < 10)
                        goto Naive;
                }

                if (data != null)
                {
                    Log("Successful naive get-data for " + fileName);
                }
                else
                {
                    await Log("Waiting for " + fileName, async () =>
                        await PollForCompletion(docClient, id, pollingInterval));

                    data = await Log("Getting data " + fileName, async () =>
                        await docClient.GetDocumentDataAsync(id));
                }

                origData = data;
                docDataCount = origData.Attributes.Count;
            }

            async Task PatchDataUpdateTest()
            {
                try
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
                    }
                    else if (docDataCount == 0)
                    {
                        throw new Exception("No attributes to patch");
                    }
                }
                catch (SwaggerException ex) when (!docDataAllExist && ex.Response.Contains("Attribute not found"))
                {
                    Log(FormattableString.Invariant($"Expected exception caught for file {fileName}"));
                }
            }

            async Task PatchDataDeleteTest()
            {
                try
                {
                    var attr = origData.Attributes.FirstOrDefault();
                    if (attr != null)
                    {
                        var attrPatch = new DocumentAttributePatch
                        {
                            ID = attr.ID,
                            Operation = PatchOperation.Delete
                        };

                        var patch = new DocumentDataPatch
                        {
                            Attributes = new[] { attrPatch }
                        };

                        await Log("Delete-patching " + fileName, async () =>
                            await docClient.PatchDocumentDataAsync(id, patch));

                        var patchedData = await Log("Getting patched data for " + fileName, async () =>
                            await docClient.GetDocumentDataAsync(id));

                        docDataAllExist = false;
                        docDataCount--;

                        if (patchedData.Attributes.Count != docDataCount)
                        {
                            throw new Exception("Delete patch had no effect!");
                        }
                    }
                    else if (docDataCount == 0)
                    {
                        throw new Exception("No attributes to patch");
                    }
                }
                catch (SwaggerException ex) when (!docDataAllExist && ex.Response.Contains("Attribute not found"))
                {
                    Log(FormattableString.Invariant($"Expected exception caught for file {fileName}"));
                }
            }

            async Task PatchDataCreateTest()
            {
                try
                {
                    var attrPatch = new DocumentAttributePatch
                    {
                        ConfidenceLevel = "High",
                        HasPositionInfo = false,
                        ID = Guid.NewGuid().ToString(),
                        Name = "MyName",
                        Type = "SSN",
                        Value = "123-12-1234",
                        Operation = PatchOperation.Create
                    };

                    var patch = new DocumentDataPatch
                    {
                        Attributes = new[] { attrPatch }
                    };

                    await Log("Create-patching " + fileName, async () =>
                        await docClient.PatchDocumentDataAsync(id, patch));

                    var patchedData = await Log("Getting patched data for " + fileName, async () =>
                        await docClient.GetDocumentDataAsync(id));

                    docDataCount++;

                    if (patchedData.Attributes.Count != docDataCount)
                    {
                        throw new Exception(FormattableString.Invariant(
                            $"Create patch had no effect! Expecting {docDataCount} attributes but got {patchedData.Attributes.Count}"));
                    }
                }
                catch (SwaggerException ex) when (!docDataAllExist && ex.Response.Contains("Attribute not found"))
                {
                    Log(FormattableString.Invariant($"Expected exception caught for file {fileName}"));
                }
            }

            async Task PatchBadDataTest()
            {
                try
                {
                    var attrPatch = new DocumentAttributePatch
                    {
                        ConfidenceLevel = "High",
                        HasPositionInfo = false,
                        ID = Guid.NewGuid().ToString(),
                        Name = "MyName",
                        Type = "SSN",
                        Value = "123-12-1234",
                        Operation = PatchOperation.Update
                    };

                    var patch = new DocumentDataPatch
                    {
                        Attributes = new[] { attrPatch }
                    };

                    await Log("Patching " + fileName, async () =>
                        await docClient.PatchDocumentDataAsync(id, patch));

                    throw new Exception("Lack of exception patch-updating non-existent attribute");
                }
                catch (SwaggerException ex) when (ex.Response.Contains("Attribute not found"))
                {
                    Log(FormattableString.Invariant($"Expected exception caught for file {fileName}"));
                }
            }

            async Task ClearDataTest()
            {
                var input = new DocumentDataInput
                {
                    Attributes = new DocumentAttribute[0]
                };

                await Log("Clearing data for " + fileName, async () =>
                    await docClient.PutDocumentDataAsync(id, input));

                var clearedData = await Log("Getting cleared data for " + fileName, async () =>
                    await docClient.GetDocumentDataAsync(id));

                docDataCount = 0;
                if (clearedData.Attributes.Count != docDataCount)
                {
                    throw new Exception(FormattableString.Invariant(
                        $"Clear data had no effect! Expecting 0 attributes but got {clearedData.Attributes.Count}"));
                }

                docDataAllExist = false;
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

                docDataCount = origData.Attributes.Count;
                docDataAllExist = true;

                if (restoredData.Attributes.Count != docDataCount)
                {
                    throw new Exception(FormattableString.Invariant(
                        $"Restore data failed! Expecting {docDataCount} attributes but got {restoredData.Attributes.Count}"));
                }
            }

            async Task DeleteDocumentTest()
            {
                await Log("Deleting " + fileName, async () =>
                    await docClient.DeleteDocumentAsync(id));
            }

            async Task GetDocumentTest()
            {
                var file = await Log("Getting document from " + fileName, async () =>
                    await docClient.GetDocumentAsync(id));

                using (var ms = new MemoryStream())
                {
                    file.Stream.CopyTo(ms);

                    var origInfo = new FileInfo(fileName);
                    if (origInfo.Length != ms.Length)
                    {
                        using (var fs = new FileStream(fileName + ".wrongSize" + Path.GetExtension(fileName), FileMode.Create))
                        {
                            ms.Position = 0;
                            ms.CopyTo(fs);
                        }

                        throw new Exception(FormattableString.Invariant(
                            $"Retrieved file size differs from original file size! (expected: {origInfo.Length} retrieved: {ms.Length})"));
                    }
                }
            }

            async Task GetDocumentTypeTest()
            {
                var docType = await Log("Getting document type from " + fileName, async () =>
                    await docClient.GetDocumentTypeAsync(id));
            }

            async Task GetOutputFileTest()
            {
                var file = await Log("Getting output file for " + fileName, async () =>
                    await docClient.GetOutputFileAsync(id));
            }

            int pageCount = 0;
            async Task GetPageInfoTest()
            {
                var info = await Log("Getting page info for " + fileName, async () =>
                    await docClient.GetPageInfoAsync(id));

                if (info.PageCount == 0 || info.PageCount != info.PageInfos.Count)
                {
                    throw new Exception("Unexpected discrepency in page info counts");
                }

                pageCount = info.PageCount;
            }

            async Task GetPageZonesTest()
            {
                var pageNum = Math.Max(pageCount, 1);
                var zones = await Log(FormattableString.Invariant($"Getting page text from page {pageCount} of {fileName}"), async () =>
                    await docClient.GetPageWordZonesAsync(id, pageNum));
            }

            async Task GetOutputTextTest()
            {
                var textResult = await Log("Getting output text from " + fileName, async () =>
                    await docClient.GetOutputTextAsync(id));

                text = string.Join("\r\n\r\n", textResult.Pages.Select(p => p.Text));
            }

            async Task GetPageTextTest()
            {
                // Use either first or last page, depending on whether GetPageInfoTest has been run already
                var pageNum = Math.Max(pageCount, 1);

                var textResult = await Log(FormattableString.Invariant($"Getting page text from page {pageNum} of {fileName}"), async () =>
                    await docClient.GetPageTextAsync(id, pageNum));

                text = textResult.Pages.Single().Text;
            }

            async Task GetTextTest()
            {
                var textResult = await Log(FormattableString.Invariant($"Getting text from {fileName}"), async () =>
                    await docClient.GetTextAsync(id));

                text = string.Join("\r\n\r\n", textResult.Pages.Select(p => p.Text));
                if (!fileIsText)
                {
                    Log("Writing text from " + fileName, () =>
                        File.WriteAllText(Path.Combine(Path.GetDirectoryName(fileName), _textFilePrefix + Guid.NewGuid().ToString("N") + ".txt"), text));
                }
            }

            async Task PostTextTest()
            {
                // Currently posting an empty string is an error so ensure there is at least one character
                var textSubmittedIDResult = await Log(FormattableString.Invariant($"Posting text from {fileName}"), async () =>
                    await docClient.PostTextAsync(text ?? " "));
            }

            async Task GetDocumentStatusesTest()
            {
                var docStatuses = await Log("Getting workflow document statuses", workflowClient.GetDocumentStatusesAsync);
            }

            async Task GetWorkflowStatusTest()
            {
                var status = await Log("Getting workflow status", workflowClient.GetWorkflowStatusAsync);
            }

            async Task LoginTest()
            {
                await Log("Logging in user", _login);
            }

            var generalTests = new List<(Func<Task> test, string description)>
            {
                (GetDataTest, "getting data from"),
                (GetDocumentTest, "getting document from"),
                (GetDocumentTypeTest, "getting document type from"),
                (GetOutputFileTest, "getting output file for"),
                (GetPageTextTest, "getting page text from"),
                (GetPageZonesTest, "getting page zones from"),
                (GetTextTest, "getting text from"),
                (GetOutputTextTest, "getting output text from"),
                (PatchDataUpdateTest, "update-patching data to"),
                (PatchDataDeleteTest, "delete-patching data to"),
                (PatchDataCreateTest, "create-patching data to"),
                (PatchBadDataTest, "bad-patching data to"),
                (RestoreDataTest, "restoring data to"),
                (ClearDataTest, "clearing data from"),
            };

            var imageOnlyTests = new List<(Func<Task> test, string description)>
            {
                (GetPageInfoTest, "getting page info for"),
            };

            var testsNotRequiringDocument = new List<(Func<Task> test, string description)>
            {
                (PostTextTest, "posting text from"),
                (GetDocumentStatusesTest, "getting document statuses for "),
                (GetWorkflowStatusTest, "getting workflow status for "),
            };

            if (_login != null)
            {
                testsNotRequiringDocument.Add((LoginTest, "re-logging in for"));
            }

            var allButDelete = generalTests.Concat(imageOnlyTests).Concat(testsNotRequiringDocument).ToList();

            // Randomly sort but make sure GetDataTest comes first so that origData has a value
            Random rng = new Random();
            allButDelete = allButDelete.OrderBy(pair =>
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

            Log(FormattableString.Invariant($"{DateTime.Now}: file: {fileName}, ID: {id}, test order: {string.Join("|", allButDelete.Select(t => SimpleName(t.test)))}"));

            foreach (var pair in allButDelete)
            {
                bool failBecauseText = fileIsText && imageOnlyTests.Contains(pair);
                try
                {
                    await pair.test();
                    if (failBecauseText)
                    {
                        throw new Exception(FormattableString.Invariant($"Lack of exception {pair.description} posted text! File: {fileName}"));
                    }
                }
                catch (SwaggerException) when (failBecauseText)
                {
                    Log(FormattableString.Invariant($"Expected exception caught for file {fileName}"));
                }
                catch (Exception ex) when (ex.Message.Contains("No attributes to patch"))
                {
                    Log(FormattableString.Invariant($"Expected exception caught for file {fileName}"));
                }
                catch (Exception ex)
                {
                    throw new Exception(FormattableString.Invariant($"Unexpected exception caught running {SimpleName(pair.test)} on {fileName}"), ex);
                }
            }

            ResetStateVars();

            await DeleteDocumentTest();

            allButDelete = allButDelete.OrderBy(_ => rng.Next()).ToList();
            Log(FormattableString.Invariant($"After deleting: file: {fileName}, ID: {id}, test order: {string.Join("+", allButDelete.Select(t => SimpleName(t.test)))}"));
            foreach (var pair in allButDelete)
            {
                bool failWhenDeleted = !testsNotRequiringDocument.Contains(pair);
                bool failBecauseText = fileIsText && imageOnlyTests.Contains(pair);
                try
                {
                    await pair.test();

                    if (failWhenDeleted)
                    {
                        throw new Exception(FormattableString.Invariant($"Lack of exception {pair.description} deleted file! File: {fileName}"));
                    }
                    if (failBecauseText)
                    {
                        throw new Exception(FormattableString.Invariant($"Lack of exception {pair.description} posted text! File: {fileName}"));
                    }
                }
                catch (SwaggerException ex) when (failBecauseText || failWhenDeleted && ex.Response.Contains("File not in the workflow"))
                {
                    Log(FormattableString.Invariant($"Expected exception caught for file {fileName}"));
                }
                catch (SwaggerException ex) when (failWhenDeleted && string.IsNullOrWhiteSpace(ex.Response))
                {
                    throw new Exception(FormattableString.Invariant($"Generic (empty response) exception instead of the expected \"file not in the workflow\" exception"));
                }
                catch (SwaggerException ex)
                {
                    throw new Exception(FormattableString.Invariant($"Unexpected post-deletion exception caught running {SimpleName(pair.test)} on {fileName}"), ex);
                }
                catch (Exception ex) when (ex.Message.Contains("No attributes to patch"))
                {
                    Log(FormattableString.Invariant($"Expected exception caught for file {fileName}"));
                }
            }
        }

        static async Task<(string fileName, int id)> PostAsync(DocumentClient client, string fileName)
        {
            DocumentIdResult result;

            if (Path.GetExtension(fileName).Equals(".txt", StringComparison.OrdinalIgnoreCase))
            {
                var text = File.ReadAllText(fileName);
                result = await client.PostTextAsync(text ?? " ");
            }
            else
            {
                using (var stream = File.OpenRead(fileName))
                {
                    var file = new FileParameter(stream, fileName);
                    result = await client.PostDocumentAsync(file);
                }
            }

            Log(DateTime.Now.ToLongTimeString() + " Created: " + result.Id);

            return (fileName, result.Id);
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
    }

    public partial class DocumentAttributePatch : DocumentAttributeCore
    {
        public DocumentAttributePatch() { }

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
