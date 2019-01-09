using Extract.FileActionManager.Database.Test;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using NUnit.Framework;
using System;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using UCLID_FILEPROCESSINGLib;
using WebAPI;
using WebAPI.Models;

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
        /// Initializes the FileProcessingDB, User, ApiContext and controller for a unit test.
        /// </summary>
        /// <param name="dbResource">The project resource name of the database to use.</param>
        /// <param name="dbName">Name to assign to the restored DB.</param>
        /// <param name="username">The database username to use for this context.</param>
        /// <param name="password">The user's password</param>
        /// <returns></returns>
        public static (FileProcessingDB fileProcessingDb, User user, TController controller)
            InitializeEnvironment<TTestClass, TController>
                (this FAMTestDBManager<TTestClass> testManager, string dbResource, string dbName, string username, string password)
                where TController : ControllerBase, new()
        {
            try
            {
                FileProcessingDB fileProcessingDb = testManager.InitializeDatabase(dbResource, dbName);
                ApiTestUtils.SetDefaultApiContext(dbName);
                fileProcessingDb.ActiveWorkflow = ApiTestUtils.CurrentApiContext.WorkflowName;
                User user = CreateUser(username, password);
                TController controller = CreateController<TController>(user);

                return (fileProcessingDb, user, controller);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45299");
            }
        }

        /// <summary>
        /// Set the default API context info - this also creates a FileApi object.
        /// </summary>
        /// <param name="databaseName">The name of the FileProcessingDB for this context.</param>
        /// <param name="workflowName">The workflow to use in the database.</param>
        /// <param name="databaseServer">The database server name.</param>
        public static ApiContext SetDefaultApiContext(string databaseName,
                                                      string workflowName = "CourtOffice",
                                                      string databaseServer = "(local)")
        {
            var apiContext = new ApiContext(databaseServer, databaseName, workflowName);
            Utils.SetCurrentApiContext(apiContext);
            Utils.ValidateCurrentApiContext();

            return apiContext;
        }

        /// <summary>
        /// Retrieves and initializes a <see cref="FileProcessingDB"/> instance for use by assigning
        /// its database ID as the secret key for token encryption.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="testManager">The test manager from which to retrieve the database.</param>
        /// <param name="resourceName">The project resource name of the database to use.</param>
        /// <param name="databaseName">The name to assign to the restored database.</param>
        /// <returns>The <see cref="FileProcessingDB"/> instance to use.</returns>
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

        /// <summary>
        /// Creates a <see cref="User"/> instance.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        public static User CreateUser(string username, string password)
        {
            return new User()
            {
                Username = username,
                Password = password,
                WorkflowName = CurrentApiContext.WorkflowName
            };
        }

        /// <summary>
        /// Creates a controller of type <typeparam name="T"/>.
        /// </summary>
        /// <param name="user">The user for which the controller is to be used.</param>
        public static T CreateController<T>(this User user) where T : ControllerBase, new()
        {
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Username),
                new Claim(JwtRegisteredClaimNames.Jti, ApiTestUtils.CurrentApiContext.SessionId),
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

        /// <summary>
        /// Validates a good <see cref="IActionResult"/> from a call to a controller and that can
        /// be interpreted as type <typeparam name="T"/>.
        /// </summary>
        /// <returns>If validated, the result interpreted as type <typeparam name="T"/></returns>.
        public static T AssertGoodResult<T>(this Task<IActionResult> resultTask) where T : class
        {
            return AssertGoodResult<T>(resultTask.Result);
        }

        /// <summary>
        /// Validates a good <see cref="IActionResult"/> from a call to a controller and that can
        /// be interpreted as type <typeparam name="T"/>.
        /// </summary>
        /// <returns>If validated, the result interpreted as type <typeparam name="T"/></returns>.
        public static T AssertGoodResult<T>(this IActionResult result) where T : class
        {
            try
            {
                T typedResult = result as T;

                // If the result itself isn't of type T, the type may refer to the value of an ObjectResult.
                if (typedResult == null)
                {
                    Assert.IsInstanceOf(typeof(ObjectResult), result, "Unexpected result type");
                    var objectResult = (ObjectResult)result;
                    Assert.GreaterOrEqual(objectResult.StatusCode, 200, "Invalid API return code");
                    Assert.Less(objectResult.StatusCode, 300, "API call failed");

                    // If caller is looking for a JwtSecurityToken, re-create one from the claims in a LoginToken
                    if (typeof(JwtSecurityToken).IsAssignableFrom(typeof(T)) && objectResult.Value is LoginToken loginToken)
                    {
                        var handler = new JwtSecurityTokenHandler();
                        var token = handler.ReadToken(loginToken.access_token) as JwtSecurityToken;

                        // We would expect a token issued by our API to use the our Issuer string
                        // and to have a session ID claim.
                        Assert.IsTrue(token.Issuer == Utils.Issuer);
                        Assert.IsTrue(token.Claims.Any(claim =>
                            claim.Type == JwtRegisteredClaimNames.Jti && !string.IsNullOrWhiteSpace(claim.Value)));

                        return token as T;
                    }

                    Assert.IsInstanceOf(typeof(T), objectResult.Value, "Unexpected result object type");
                    typedResult = (T)objectResult.Value;
                }
                else
                {
                    if (result is StatusCodeResult statusCodeResult)
                    {
                        Assert.GreaterOrEqual(statusCodeResult.StatusCode, 200, "Invalid API return code");
                        Assert.Less(statusCodeResult.StatusCode, 300, "API call failed");
                    }

                    return typedResult;
                }

                return typedResult;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45300");
            }
        }

        /// <summary>
        /// Validates a the <see cref="IActionResult"/> has the specified HTTP status code.
        /// </summary>
        public static void AssertResultCode(this Task<IActionResult> resultTask, int expectedCode)
        {
            AssertResultCode(resultTask.Result, expectedCode);
        }

        /// <summary>
        /// Validates a the <see cref="IActionResult"/> has the specified HTTP status code.
        /// </summary>
        public static void AssertResultCode(this IActionResult result, int expectedCode)
        {
            try
            {
                if (result is ObjectResult objectResult)
                {
                    Assert.AreEqual(expectedCode, objectResult.StatusCode);
                }
                else if (result is StatusCodeResult statusCodeResult)
                {
                    Assert.AreEqual(expectedCode, statusCodeResult.StatusCode);
                }
                else
                {
                    throw new Exception("Unexpected result type");
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI46262");
            }
        }

    /// <summary>
    /// Sets the context claim principal.
    /// </summary>
    /// <param name="controller">The <see cref="Controller"/> for which the context should be set.
    /// </param>
    /// <param name="securityToken">The <see cref="JwtSecurityToken"/> containing the claims necessary
    /// to create the appropriate <see cref="ClaimsPrincipal"/>.</param>
    public static void ApplyTokenClaimPrincipalToContext(this Controller controller, JwtSecurityToken securityToken)
        {
            try
            {
                var claimsIdentity = new ClaimsIdentity(securityToken.Claims);
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                controller.ControllerContext.HttpContext.User = claimsPrincipal;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI46261");
            }
        }
    }
}
