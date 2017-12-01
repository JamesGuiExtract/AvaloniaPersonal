using Extract.FileActionManager.Database.Test;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Text;
using UCLID_FILEPROCESSINGLib;
using WebAPI;
using WebAPI.Models;

//using ApiUtils = WebAPI.Utils;

namespace Extract.Web.WebAPI.Test
{
    public static class ApiTestUtils
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
        /// Initializes the environment.
        /// </summary>
        /// <param name="dbResource">The database resource.</param>
        /// <param name="dbName">Name of the database.</param>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <returns></returns>
        public static (FileProcessingDB fileProcessingDb, User user, TController controller)
            InitializeEnvironment<TTestClass, TController>
                (this FAMTestDBManager<TTestClass> testManager, string dbResource, string dbName, string username, string password)
                where TController : ControllerBase, new()
        {
            FileProcessingDB fileProcessingDb = testManager.InitializeDatabase(dbResource, dbName);
            ApiTestUtils.SetDefaultApiContext(dbName);
            fileProcessingDb.ActiveWorkflow = ApiTestUtils.CurrentApiContext.WorkflowName;
            User user = CreateUser(username, password);
            TController controller = CreateController<TController>(user);

            return (fileProcessingDb, user, controller);
        }

        /// <summary>
        /// Set the default API context info - this also creates a FileApi object.
        /// </summary>
        /// <param name="databaseName"></param>
        /// <param name="workflowName"></param>
        /// <param name="databaseServer"></param>
        public static ApiContext SetDefaultApiContext(string databaseName,
                                                      string workflowName = "CourtOffice",
                                                      string databaseServer = "(local)")
        {
            var apiContext = new ApiContext(databaseServer, databaseName, workflowName);
            Utils.SetCurrentApiContext(apiContext);
            Utils.ApplyCurrentApiContext();

            return apiContext;
        }

        public static FileProcessingDB InitializeDatabase<T>(this FAMTestDBManager<T> testManager, string resourceName, string databaseName)
        {
            try
            {
                var fileProcessingDB = testManager.GetDatabase(resourceName, databaseName);

                var secretKey = fileProcessingDB.DatabaseID;
                var signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secretKey));
                AuthUtils.SecretKey = secretKey;

                return fileProcessingDB;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45294");
            }
        }

        public static User CreateUser(string username, string password)
        {
            return new User()
            {
                Username = "admin",
                Password = "a",
                WorkflowName = CurrentApiContext.WorkflowName
            };
        }

        public static T CreateController<T>(User user) where T : ControllerBase, new()
        {
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Username),
                new Claim(JwtRegisteredClaimNames.Jti, Utils.TestSessionID),
                new Claim("WorkflowName", CurrentApiContext.WorkflowName)
            }));

            T controller = new T();
            controller.ControllerContext =
                new ControllerContext()
                {
                    HttpContext = new DefaultHttpContext() { User = claimsPrincipal }
                };

            return controller;
        }

        /// <summary>
        /// Gets or sets the i action result.
        /// </summary>
        /// <value>
        /// The i action result.
        /// </value>
        public static T AssertGoodResult<T>(this IActionResult result) where T : class
        {
            T typedResult = result as T;
            if (typedResult == null)
            {
                Assert.IsInstanceOf(typeof(ObjectResult), result, "Unexpected result type");
                var objectResult = result as ObjectResult;
                Assert.AreEqual((int)HttpStatusCode.OK, objectResult.StatusCode);

                Assert.IsInstanceOf(typeof(T), objectResult.Value, "Unexpected result object type");
                typedResult = objectResult.Value as T;
            }

            return typedResult;
        }

        /// <summary>
        /// Get the current API context.
        /// </summary>
        /// <returns>the current API context</returns>
        public static ApiContext CurrentApiContext
        {
            get
            {
                return Utils.CurrentApiContext;
            }
        }
    }
}
