using Microsoft.Extensions.Configuration;
using System;
using System.Diagnostics;
using System.IO;                        // FileStream
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace SubmitFilesConsole
{
    class Program
    {
        // Configuration parameters that come from the appsettings.json configuration file in the same folder as the .exe
        static string _baseAddr = "http://localhost:58926";
        static string _userName = "Admin";
        static string _password = "a";
        static string _workflowName = "CourtOffice";
        static string _logPath = "c:/temp/MvcUplaodFile/Logs";
        static string _webApiPortionOfUrl = "api/Document/SubmitFile";

        /* JWT is returned JSON encoded, looks like this:
        {
            "access_token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJhZG1pbiIsImp0aSI6ImIwZTZiMzdkLWNhZjUtNDdjNC04YzViLWRhNDY4NmI4OTc0YSIsImlhdCI6MTQ5NzYyNzM3OCwiV29ya2Zsb3dOYW1lIjoiQ291cnRPZmZpY2UiLCJuYmYiOjE0OTc2MjczNzgsImV4cCI6MTQ5NzY3MDU3OCwiaXNzIjoiRG9jdW1lbnRBUEl2MSIsImF1ZCI6IkVTV2ViQ2xpZW50cyJ9.8q4We3gbrOGvxn1x4On8YvwqPUJVG_eccRhzCmYpX2E",
            "expires_in": 43200
        }
        */
        /// <summary>
        /// Takes a JSON-encoded JWT, and extracts the token text and returns it.
        /// </summary>
        /// <param name="token">JWT</param>
        /// <returns>token text</returns>
        static string ExtractAccessToken(string token)
        {
            int startPosition = token.IndexOf(":");
            startPosition += 3;                         // advance to first char past the following space and " chars, to start of access_token
            int endPosition = token.IndexOf("\"", startPosition + 1);
            var result = token.Substring(startPosition, endPosition - startPosition);

            return result;
        }

        /// <summary>
        /// Logs into the specified FAM user account, and gets back a JWT from DocumentAPI
        /// </summary>
        /// <returns>token text extracted from the JWT returned by the DocumentAPI</returns>
        static string Login()
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(_baseAddr);

                    UserData loginInfo = new UserData
                    {
                        Username = _userName,
                        Password = _password,
                        WorkflowName = _workflowName
                    };

                    string loginAsText = loginInfo.ToJsonString();

                    var content = new StringContent(loginAsText, Encoding.UTF8, "application/json");
                    HttpResponseMessage response = client.PostAsync("api/Users/Login", content).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        string jwtToken = response.Content.ReadAsStringAsync().Result;
                        return ExtractAccessToken(jwtToken);
                    }
                }
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
            }

            return "";
        }

        /// <summary>
        /// Reads the appsettings.json configuration file, and sets all elements it finds there.
        /// </summary>
        static void ReadConfig()
        {
            var cwd = Directory.GetCurrentDirectory();

            var builder = new ConfigurationBuilder()
                .SetBasePath(cwd)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            IConfigurationRoot Configuration = builder.Build();

            var baseAddr = Configuration["BaseAddr"];
            if (!String.IsNullOrWhiteSpace(baseAddr))
            {
                _baseAddr = baseAddr;
            }

            var username = Configuration["UserName"];
            if (!String.IsNullOrWhiteSpace(username))
            {
                _userName = username;
            }

            var password = Configuration["Password"];
            if (!String.IsNullOrWhiteSpace(password))
            {
                _password = password;
            }

            var workflowName = Configuration["WorkflowName"];
            if (!String.IsNullOrWhiteSpace(workflowName))
            {
                _workflowName = workflowName;
            }

            var webApiPortionOfUrl = Configuration["WebApiPortionOfUrl"];
            if (!String.IsNullOrWhiteSpace(webApiPortionOfUrl))
            {
                _webApiPortionOfUrl = webApiPortionOfUrl;
            }

            var logPath = Configuration["LogPath"];
            if (!String.IsNullOrWhiteSpace(logPath))
            {
                _logPath = logPath;
            }
        }

        /// <summary>
        /// calls the DocumentAPI.SubmitFile() method, and logs the outcome of that action
        /// </summary>
        /// <param name="fileName">name of the file - no path</param>
        /// <param name="file">stream associated with the file</param>
        /// <param name="JWT">the login token text</param>
        /// <param name="log">log instance to write output too</param>
        static void SubmitFile(string fileName, FileStream file, string JWT, Log log)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(_baseAddr);
                    var token = JWT;

                    if (file.Length <= 0)
                    {
                        return;
                    }

                    var fileLength = file.Length;
                    var timer = Stopwatch.StartNew();

                    using (var content = new MultipartFormDataContent())
                    {
                        content.Add(new StreamContent(file)
                        {
                            Headers =
                            {
                                ContentLength = fileLength,
                                ContentType = new MediaTypeHeaderValue("application/octet-stream")
                            }
                        }, "File", fileName);

                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                        var response = client.PostAsync(_webApiPortionOfUrl, content).Result;

                        timer.Stop();
                        var code = response.StatusCode;
                        log.WriteLine($"Filename: {fileName}, \t\tLength: {fileLength}, \t\tElapsed time (mS): {timer.ElapsedMilliseconds}, \t\tStatus code: {code}");
                    }
                }
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
                log.WriteLine($"Error reported: {msg}");
            }
        }

        /// <summary>
        /// Main method
        /// </summary>
        /// <param name="args">arg[0] is required, path+filename, and the filename can have wildcards embedded (* or ?)</param>
        static void Main(string[] args)
        {
            Log log = null;

            try
            {
                // Get file from command line, can contain wildcards
                if (args.Length < 1)
                {
                    Console.WriteLine("There is one required input argument, the file(s) to submit. The filename can contain wildcards");
                    return;
                }
                if (args.Length > 1)
                {
                    Console.WriteLine("There is only one allowed input argument, the file(s) to submit. The filename can contain wildcards");
                    return;
                }

                var arg = args[0];
                var path = Path.GetDirectoryName(arg);
                string filename = Path.GetFileName(arg);
                string[] files;
                if (filename.Contains("*") || filename.Contains("?"))
                {
                    files = Directory.GetFiles(path, filename);
                }
                else
                {
                    files = new string[1];
                    files[0] = Path.Combine(path, filename);
                }

                ReadConfig();

                var token = Login();

                log = new Log(_logPath);

                foreach (var namedFile in files)
                {
                    using (var streamedFile = new FileStream(path: namedFile, mode: FileMode.Open))
                    {
                        var nameOfFile = Path.GetFileName(namedFile);
                        SubmitFile(nameOfFile, streamedFile, token, log);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error reported: {0}", ex.Message);
            }
            finally
            {
                if (log != null)
                {
                    log.Close();
                    log = null;
                }
            }
        }
    }
}