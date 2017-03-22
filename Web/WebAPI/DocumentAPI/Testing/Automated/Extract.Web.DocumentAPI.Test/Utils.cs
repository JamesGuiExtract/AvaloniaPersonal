using System;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Threading;

using NUnit.Framework;
using IO.Swagger.Api;
using IO.Swagger.Model;

namespace Extract.Web.DocumentAPI.Test
{
    public static class Utils
    {
        // TODO - this should be an extension method somewhere in the Extract framework, 
        // as I've now copied this method...
        //
        /// <summary>
        /// string extension method to simplify determining if two strings are equivalent
        /// </summary>
        /// <param name="s1">this</param>
        /// <param name="s2">string to compare this against</param>
        /// <param name="ignoreCase">defaults to true</param>
        /// <returns>true or false</returns>
        public static bool IsEquivalent(this string s1,
                        string s2,
                        bool ignoreCase = true)
        {
            if (String.Compare(s1, s2, ignoreCase, CultureInfo.InvariantCulture) == 0)
                return true;

            return false;
        }

        /// <summary>
        /// returns the method name of the caller - do NOT set the default argument!
        /// </summary>
        /// <param name="caller"></param>
        /// <returns></returns>
        public static string GetMethodName([CallerMemberName] string caller = null)
        {
            return caller;
        }

        /// <summary>
        /// start a hidden process (typically a console application)
        /// </summary>
        /// <param name="processName"></param>
        /// <param name="args"></param>
        /// <param name="workingFolder"></param>
        public static void StartHiddenProcess(string processName, string args = "", string workingFolder = "")
        {
            try
            {
                ProcessStartInfo psi = new ProcessStartInfo();
                psi.Arguments = args;
                psi.UseShellExecute = false;            // WARNING: if true must use STA! Also affects WorkingDirectory.
                psi.WorkingDirectory = workingFolder;
                psi.FileName = processName;
                psi.WindowStyle = ProcessWindowStyle.Hidden;
                psi.CreateNoWindow = true;
                var process = Process.Start(psi);

                Assert.IsTrue(process != null,
                              "Failed to start Process: {0}, args: {1}, working folder: {2}",
                              processName,
                              args,
                              workingFolder);

                Assert.IsTrue(!process.HasExited,
                              "Process has exited already, process: {0}, args: {1}, working folder: {2}",
                              processName,
                              args,
                              workingFolder);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception: {0}, starting process: {1}, args: {2}, working folder: {3}",
                                ex.Message,
                                processName,
                                args,
                                workingFolder);
                throw;
            }
        }

        /// <summary>
        /// start the web server if necessary, and return an invocation indicator
        /// </summary>
        /// <param name="workingDirectory"></param>
        /// <param name="defaultWebApiUrl"></param>
        /// <returns>true if invoked</returns>
        public static bool StartWebServer(string workingDirectory, string webApiURL)
        {
            try
            {
                if (webApiURL.Contains("localhost"))
                {
                    var extendedArgs = $"run -p {workingDirectory}\\DocumentAPI.csproj";
                    Utils.StartHiddenProcess(processName: "dotnet.exe",
                         args: extendedArgs,
                         workingFolder: "");

                    Thread.Sleep(2 * 1000);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception starting WebAPI: {0}", ex.Message);
                throw;
            }
        }

        /// <summary>
        /// shut down the web server
        /// </summary>
        /// <param name="args"></param>
        public static void ShutdownWebServer(string args)
        {
            Utils.StartHiddenProcess("taskkill.exe", args);
        }

        /// <summary>
        /// Set the DB context on the web server
        /// </summary>
        /// <param name="dbName"></param>
        /// <param name="WebApiURL"></param>
        public static void SetDatabase(string dbName, string WebApiURL)
        {
            try
            {
                Assert.IsFalse(String.IsNullOrEmpty(WebApiURL));
                var dbApi = new IO.Swagger.Api.DatabaseApi(basePath: WebApiURL);
                dbApi.ApiDatabaseSetDatabaseNameByIdPost(id: dbName);
            }
            catch (Exception ex)
            {
                Assert.Fail($"Exception: {ex.Message}, in method: {Utils.GetMethodName()}");
            }
        }

        /// <summary>
        /// get the folder where the web api resides.
        /// </summary>
        public static string GetWebApiFolder
        {
            get
            {
                return @"C:\Engineering\Web\WebAPI\DocumentAPI\Core\DocumentAPI";
            }
        }
    }
}
