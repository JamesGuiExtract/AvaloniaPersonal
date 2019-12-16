using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Extract.Web.WebAPI.DocumentAPISubmit
{
    public class ApiTester
    {
        int _fileId = -1;
        string _fileName;
        int _pageCount = 0;
        int _pollingInterval;
        int _maxRetries;
        bool _fileIsText;
        DocumentDataResult _origData = null;
        DocumentDataResult _currentData = null;
        string _documentText = null;
        User _user = null;
        string _auth = null;
        bool _reprocessing;
        bool _deleted;

        public ApiTester(string fileName, User user, string auth, int pollingInterval, int maxRetries)
        {
            _fileName = fileName;
            _fileIsText = string.IsNullOrWhiteSpace(_fileName)
                ? false
                : Path.GetExtension(_fileName).Equals(".txt", StringComparison.OrdinalIgnoreCase);
            _user = user;
            _auth = auth;
            _pollingInterval = pollingInterval;
            _maxRetries = maxRetries;
        }

        public int fileId
        {
            get => _fileId;
            set => _fileId = value;
        }
        public string fileName
        {
            get
            {
                return _fileName;
            }

            set
            {
                _fileName = value;
                _fileIsText = string.IsNullOrWhiteSpace(value)
                    ? false
                    : Path.GetExtension(_fileName).Equals(".txt", StringComparison.OrdinalIgnoreCase);
            }
        }
        public int pageCount
        {
            get => _pageCount;
            set => _pageCount = value;
        }
        public int pollingInterval => _pollingInterval;
        public int maxRetries => _maxRetries;
        public bool fileIsText => _fileIsText;
        public DocumentDataResult origData
        {
            get => _origData;
            set => _origData = value;
        }
        public DocumentDataResult currentData
        {
            get => _currentData;
            set => _currentData = value;
        }
        public string documentText
        {
            get => _documentText;
            set => _documentText = value;
        }
        public User user => _user;
        public string auth
        {
            get => _auth;
            set => _auth = value;
        }
        public bool reprocessing
        {
            get => _reprocessing;
            set => _reprocessing = value;
        }
        public bool deleted
        {
            get => _deleted;
            set => _deleted = value;
        }

        protected static Random rng = new Random();

        protected virtual string testerName
        {
            get;
        }

        protected async Task RunTests(List<TestInfo> tests, string statusPrefix)
        {
            var orderedTests = tests
                .OrderBy(testInfo => testInfo.priority)
                .ThenBy(testInfo => rng.Next())
                .ToList();

            Log(FormattableString.Invariant($"{DateTime.Now:HH:mm:ss:fff}: {statusPrefix}file: {fileName}, ID: {fileId}, test order: {string.Join("|", orderedTests.Select(t => SimpleName(t.test)))}"));

            foreach (var test in orderedTests)
            {
                try
                {
                    await KeepTrying(test.test, retryOn404: test.retryOn404);
                    if (test.expect404)
                    {
                        throw new Exception(FormattableString.Invariant($"Lack of expected exception for {test.description}! File: {fileName}"));
                    }
                }
                catch (SwaggerException ex) when ((test.expect404 || deleted) && ex.StatusCode == (int)HttpStatusCode.NotFound)
                {
                    Log(FormattableString.Invariant($"{statusPrefix}Expected exception caught for file {fileName}"));
                }
                catch (Exception ex)
                {
                    throw new Exception(FormattableString.Invariant($"{statusPrefix}Unexpected exception caught running {SimpleName(test.test)} on {fileName}"), ex);
                }
            }
        }

        public string FormatLogLine(string message, string prefix = null)
        {
            message = message.Trim();
            if (!string.IsNullOrWhiteSpace(prefix))
            {
                message = $"{prefix} {message.Substring(0, 1).ToLower()}{message.Substring(1)}";
            }

            string fileNameOnly = string.IsNullOrWhiteSpace(fileName) ? "" : Path.GetFileName(fileName);
            return $"[{testerName}] {DateTime.Now:HH:mm:ss:fff} ID:{fileId} {message} {fileNameOnly}";
        }

        public void Log(string message, bool error = false)
        {
            message = FormatLogLine(message);

            if (error)
            {
                Console.Error.WriteLine(message);
                Trace.TraceError(message);
                Console.Error.FlushAsync();
                Trace.Flush();
            }
            else
            {
                Console.WriteLine(message);
                Trace.WriteLine(message);
            }
        }

        public void Log(string message, Action func)
        {
            string startMessage = FormatLogLine(message);
            Console.WriteLine(startMessage);
            Trace.WriteLine(startMessage);

            func();

            string endMessage = FormatLogLine(message, "Done");
            Console.WriteLine(endMessage);
            Trace.WriteLine(endMessage);
        }

        public async Task Log(string message, Func<Task> func)
        {
            string startMessage = FormatLogLine(message);
            Console.WriteLine(startMessage);
            Trace.WriteLine(startMessage);

            await func();

            string endMessage = FormatLogLine(message, "Done");
            Console.WriteLine(endMessage);
            Trace.WriteLine(endMessage);
        }

        public async Task<T> Log<T>(string message, Func<Task<T>> func)
        {
            string startMessage = FormatLogLine(message);
            Console.WriteLine(startMessage);
            Trace.WriteLine(startMessage);

            var ret = await func();

            string endMessage = FormatLogLine(message, "Done");
            Console.WriteLine(endMessage);
            Trace.WriteLine(endMessage);

            return ret;
        }

        public static string SimpleName<T>(Func<T> func)
        {
            try
            {
                return func.Method.Name; //.Split(new[] { '_', '|' })[2];
            }
            catch (NullReferenceException)
            {
                return "UnknownMethodName:" + (func?.Method?.Name ?? "");
            }
        }

        public async Task KeepTrying(Func<Task> action, bool retryOn404, bool logFailures = false)
        {
            try
            {
                for (int i = 0; maxRetries > 0 && i < maxRetries; i++)
                {
                    try
                    {
                        await action();
                        break;
                    }
                    catch (SwaggerException ex) when (i >= maxRetries - 1)
                    {
                        Log(FormattableString.Invariant($"({SimpleName(action)}) After retries: {ex.StatusCode} exception for {fileName}\r\n{ex.ToString()}"), error: true);
                        throw ex;
                    }
                    catch (Exception ex) when (i >= maxRetries - 1)
                    {
                        Log(FormattableString.Invariant($"({SimpleName(action)}) After retries: Exception \"{ex.Message}\" for {fileName}\r\n{ex.ToString()}"), error: true);
                        throw ex;
                    }
                    catch (SwaggerException ex) when (retryOn404 && !deleted && ex.StatusCode == 404)
                    {
                        // Depending on the FAM workflow that is paired with this utility, some elements such as 
                        // the output file may not yet be available when called initially
                        Log(FormattableString.Invariant($"Handled 404 (not found) exception for {fileName}. Retrying ({i + 1}) in {pollingInterval}ms..."));
                    }
                    catch (SwaggerException ex) when (ex.StatusCode == 423)
                    {
                        // This utility is designed such that occassionally the same document will be targetted by
                        // more than one thread at a time; if the other thread is editing data at the same time
                        // this thread tries to, 423 will result.
                        Log(FormattableString.Invariant($"Handled 423 (locked) exception for {fileName}. Retrying ({i + 1}) in {pollingInterval}ms..."));
                    }
                    catch (SwaggerException ex) when (ex.StatusCode == 500 && ex.Message.Contains("Timeout waiting to process request"))
                    {
                        Log(FormattableString.Invariant($"Handled 500, 'timeout...' exception for {fileName}. Retrying ({i + 1}) in {pollingInterval}ms..."));
                    }
                    catch (SwaggerException ex) when (ex.StatusCode == 500)
                    {
                        Log(FormattableString.Invariant($"Handled 500 exception for {fileName}. Retrying ({i + 1}) in {pollingInterval}ms..."));
                    }
                    catch (SwaggerException ex) when (ex.StatusCode == 502)
                    {
                        Log(FormattableString.Invariant($"Handled 502 exception for {fileName}. Retrying ({i + 1}) in {pollingInterval}ms..."));
                    }
                    catch (SwaggerException ex) when (ex.StatusCode == 503)
                    {
                        Log(FormattableString.Invariant($"Handled 503 exception for {fileName}. Retrying ({i + 1}) in {pollingInterval}ms..."));
                    }
                    catch (OperationCanceledException)
                    {
                        Log(FormattableString.Invariant($"Handled OperationCanceledException for {fileName}. Retrying ({i + 1}) in {pollingInterval}ms..."));
                    }
                    catch (Exception ex) when (logFailures)
                    {
                        Log($"FAILURE {ex}", error: true);
                    }

                    await Task.Delay(pollingInterval);
                }
            }
            catch (OperationCanceledException ex) when (logFailures)
            {
                Log("CANCELLED: {ex}", error: true);
                throw ex;
            }
            catch (Exception ex) when (logFailures)
            {
                Log("FAILURE: {ex}", error: true);
                throw ex;
            }
        }

        public async Task<T> KeepTrying<T>(Func<Task<T>> func, bool retryOn404, bool logFailures = false)
        {
            try
            {
                T result = default;
                for (int i = 0; maxRetries > 0 && i < maxRetries; i++)
                {
                    try
                    {
                        result = await func();
                        break;
                    }
                    catch (SwaggerException ex) when (i >= maxRetries - 1)
                    {
                        Log(FormattableString.Invariant($"({SimpleName(func)} After retries): {ex.StatusCode} exception for {fileName}\r\n{ex.ToString()}"), error: true);
                        throw ex;
                    }
                    catch (Exception ex) when (i >= maxRetries - 1)
                    {
                        Log(FormattableString.Invariant($"({SimpleName(func)}) After retries: Exception \"{ex.Message}\" for {fileName}\r\n{ex.ToString()}"), error: true);
                        throw ex;
                    }
                    catch (SwaggerException ex) when (retryOn404 && !deleted && ex.StatusCode == 404)
                    {
                        // Depending on the FAM workflow that is paired with this utility, some elements such as 
                        // the output file may not yet be available when called initially
                        Log(FormattableString.Invariant($"Handled 404 (not found) exception for {fileName}. Retrying ({i + 1}) in {pollingInterval}ms..."));
                    }
                    catch (SwaggerException ex) when (ex.StatusCode == 423)
                    {
                        // This utility is designed such that occassionally the same document will be targetted by
                        // more than one thread at a time; if the other thread is editing data at the same time
                        // this thread tries to, 423 will result.
                        Log(FormattableString.Invariant($"Handled 423 (locked) exception for {fileName}. Retrying ({i + 1}) in {pollingInterval}ms..."));
                    }
                    catch (SwaggerException ex) when (ex.StatusCode == 500 && ex.Message.Contains("Timeout waiting to process request"))
                    {
                        Log(FormattableString.Invariant($"Handled 500, 'timeout...' exception for {fileName}. Retrying ({i + 1}) in {pollingInterval}ms..."));
                    }
                    catch (SwaggerException ex) when (ex.StatusCode == 500)
                    {
                        Log(FormattableString.Invariant($"Handled 500 exception for {fileName}. Retrying ({i + 1}) in {pollingInterval}ms..."));
                    }
                    catch (SwaggerException ex) when (ex.StatusCode == 502)
                    {
                        Log(FormattableString.Invariant($"Handled 502 exception for {fileName}. Retrying ({i + 1}) in {pollingInterval}ms..."));
                    }
                    catch (SwaggerException ex) when (ex.StatusCode == 503)
                    {
                        Log(FormattableString.Invariant($"Handled 503 exception for {fileName}. Retrying ({i + 1}) in {pollingInterval}ms..."));
                    }
                    catch (OperationCanceledException)
                    {
                        Log(FormattableString.Invariant($"Handled OperationCanceledException for {fileName}. Retrying ({i + 1}) in {pollingInterval}ms..."));
                    }

                    await Task.Delay(pollingInterval);
                }

                return result;
            }
            catch (OperationCanceledException ex) when (logFailures)
            {
                Log("CANCELLED: {ex}", error: true);
                throw ex;
            }
            catch (Exception ex) when (logFailures)
            {
                Log("FAILURE: {ex}", error: true);
                throw ex;
            }
        }

        public class TestInfo
        {
            public TestInfo(int priority, Func<Task> test, string description)
            {
                this.priority = priority;
                this.test = test;
                this.description = description;
                this.expect404 = false;
                this.retryOn404 = true;
            }

            public TestInfo(int priority, Func<Task> test, string description, bool expect404, bool retryOn404)
            {
                this.priority = priority;
                this.test = test;
                this.description = description;
                this.expect404 = expect404;
                this.retryOn404 = retryOn404;
            }

            public int priority;
            public string description;
            public bool expect404;
            public bool retryOn404;
            public Func<Task> test;
        }
    }
}
