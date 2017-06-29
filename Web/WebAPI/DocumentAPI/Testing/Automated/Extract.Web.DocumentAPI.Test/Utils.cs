using DocumentAPI.Models;
using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using ApiUtils = DocumentAPI.Utils;

namespace Extract.Web.DocumentAPI.Test
{
    public static class Utils
    {
        const string DbDemoLabDE = "Demo_LabDE_Temp";


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
        /// Set the default API context info - this also creates a FileApi object.
        /// </summary>
        /// <param name="databaseName"></param>
        /// <param name="databaseServer"></param>
        /// <param name="workflowName"></param>
        public static ApiContext SetDefaultApiContext(string databaseName, 
                                                      string databaseServer = "(local)", 
                                                      string workflowName = "CourtOffice")
        {
            var apiContext = new ApiContext(databaseServer, databaseName, workflowName);
            ApiUtils.SetCurrentApiContext(apiContext);
            ApiUtils.ApplyCurrentApiContext();

            return apiContext;
        }

        /// <summary>
        /// Get the current API context.
        /// </summary>
        /// <returns>the current API context</returns>
        public static ApiContext GetCurrentApiContext
        {
            get
            {
                return ApiUtils.CurrentApiContext;
            }
        }
    }
}
