using AttributeDbMgrComponentsLib;
using Extract.FileActionManager.Database.Test;
using Extract.Imaging.Utilities;
using Extract.Utilities;
using Extract.Web.ApiConfiguration.Models;
using Extract.Web.ApiConfiguration.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;
using WebAPI;

namespace Extract.Web.WebAPI.Test
{
    public static class ApiTestUtils
    {
        static readonly DataTransferObjectSerializer _webConfigSerializer = new(typeof(ICommonWebConfiguration).Assembly);

        private static readonly DocumentApiConfiguration _labDEFileStatusConfiguration = new(
            configurationName: "DocumentAPITesting",
            isDefault: true,
            workflowName: "CourtOffice",
            attributeSet: "DataFoundByRules",
            processingAction: "A02_Verify",
            postProcessingAction: "A05_Cleanup",
            documentFolder: @"c:\temp\DocumentFolder",
            startAction: "A01_ExtractData",
            endAction: "A04_Output",
            postWorkflowAction: "",
            outputFileNameMetadataField: "Outputfile",
            outputFileNameMetadataInitialValueFunction: "<SourceDocName>.result.tif");

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
        /// <param name="apiVersion">The API version to use.</param>
        /// <param name="dbResource">The project resource name of the database to use.</param>
        /// <param name="dbName">Name to assign to the restored DB.</param>
        /// <param name="username">The database username to use for this context.</param>
        /// <param name="password">The user's password</param>
        /// <returns></returns>
        public static (FileProcessingDB fileProcessingDb, User user, TController controller) InitializeEnvironment<TTestClass, TController>(
            this FAMTestDBManager<TTestClass> testManager,
            Func<TController> controller,
            string apiVersion,
            string dbResource,
            string dbName,
            string username,
            string password,
            ICommonWebConfiguration webConfiguration) where TController : ControllerBase
        {
            try
            {
                UnlockLeadtools.UnlockLeadToolsSupport();
                FileProcessingDB fileProcessingDb = testManager.InitializeDatabase(dbResource, dbName);
                var createdController = controller();

                ApiTestUtils.SetDefaultApiContext(apiVersion, dbName, webConfiguration);
                fileProcessingDb.ActiveWorkflow = ApiTestUtils.CurrentApiContext.WebConfiguration.WorkflowName;
                User user = CreateUser(username, password);

                user.SetupController(createdController);

                return (fileProcessingDb, user, createdController);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45299");
            }
        }

        /// <summary>
        /// Set the default API context info - this also creates a FileApi object.
        /// </summary>
        /// <param name="apiVersion">The API version to use.</param>
        /// <param name="databaseName">The name of the FileProcessingDB for this context.</param>
        /// <param name="webConfiguration">The web configuration to use in the database.</param>
        /// <param name="databaseServer">The database server name.</param>
        public static ApiContext SetDefaultApiContext(string apiVersion,
                                                      string databaseName,
                                                      ICommonWebConfiguration webConfiguration,
                                                      string databaseServer = "(local)")
        {
            var apiContext = new ApiContext(apiVersion, databaseServer, databaseName, webConfiguration);
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
        /// Gets a <see cref="AttributeDBMgr"/> instance to store/retrieve attributes sets to/from
        /// <see paramref="FileProcessingDB"/>.
        /// </summary>
        /// <param name="fileProcessingDB">The <see cref="FileProcessingDB"/> for which data will be
        /// stored/retrieved.</param>
        public static AttributeDBMgr GetAttributeDBMgr(this FileProcessingDB fileProcessingDB)
        {
            try
            {
                var attributeDbMgr = new AttributeDBMgr();
                attributeDbMgr.FAMDB = fileProcessingDB;

                return attributeDbMgr;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI49469");
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
                WorkflowName = CurrentApiContext.WebConfiguration.WorkflowName,
                ConfigurationName = CurrentApiContext.WebConfiguration.ConfigurationName
            };
        }

        /// <summary>
        /// Setup a controller with its context
        /// </summary>
        /// <param name="user">The user for which the controller is to be used.</param>
        public static T SetupController<T>(this User user, T controller, ICommonWebConfiguration commonWebConfiguration = null) where T : ControllerBase
        {
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Username),
                new Claim(JwtRegisteredClaimNames.Jti, ApiTestUtils.CurrentApiContext.SessionId),
                new Claim("WorkflowName", commonWebConfiguration != null ? commonWebConfiguration.WorkflowName : CurrentApiContext.WebConfiguration.WorkflowName),
                new Claim("ConfigurationName", commonWebConfiguration != null ? commonWebConfiguration.ConfigurationName :  CurrentApiContext.WebConfiguration.ConfigurationName)
            }));

            controller.ControllerContext =
                new ControllerContext()
                {
                    HttpContext = new DefaultHttpContext() { User = claimsPrincipal }
                };

            return controller;
        }

        /// <summary>
        /// Gets the ID of the the active document session (FileTaskSession ID).
        /// </summary>
        /// <param name="controller">The controller for which the session should be checked.</param>
        /// <returns>The ID of the the active document session or -1 if no session is active.
        /// </returns>
        public static int GetActiveDocumentSessionId(this ControllerBase controller)
        {
            // NOTE: This is a heavy-handed way to get the active document session ID for a controller.
            // For unit tests, it should be fine, but please re-think before trying to use as
            // part of the any code run in production.

            var apiInterface =
                FileApiMgr.Instance.GetInterface(CurrentApiContext, controller.ControllerContext.HttpContext.User);
            int fileTaskSessionId = apiInterface.DocumentSession.Id;

            // InUse is required to return the instance back to the available pool.
            apiInterface.InUse = false;

            return fileTaskSessionId;
        }

        /// <summary>
        /// Gets an array of pages numbers for which image data has been cached.
        /// </summary>
        /// <param name="controller">The controller for which the cached page array should be retrieved.</param>
        /// <param name="fileProcessingDB">The database hosting the cache.</param>
        /// <returns>An array of the page numbers for which data has been cached.</returns>
        public static int[] GetCachedImagePageNumbers(this ControllerBase controller, FileProcessingDB fileProcessingDB)
        {
            int fileTaskSessionId = controller.GetActiveDocumentSessionId();

            var cachedPages = fileProcessingDB.GetCachedPageNumbers(fileTaskSessionId,
                ECacheDataType.kImage);

            return (int[])cachedPages;
        }

        /// <summary>
        /// Gets an array of page numbers for which image data has been cached
        /// </summary>
        /// <param name="fileProcessingDB">The database hosting the cache.</param>
        /// <param name="fileTaskSessionId">The document session ID for which to check for cached data.</param>
        /// <returns>An array of the page numbers for which data has been cached.</returns>
        public static int[] GetCachedImagePageNumbers(this FileProcessingDB fileProcessingDB, int fileTaskSessionId)
        {
            var cachedPages = fileProcessingDB.GetCachedPageNumbers(fileTaskSessionId,
                ECacheDataType.kImage);

            return (int[])cachedPages;
        }

        /// <summary>
        /// Checks whether data has been cached for the specified page; will throw the exception
        /// that occured for unsuccessful cache attempts.
        /// </summary>
        /// <param name="controller">The controller for which the cache status should be checked.</param>
        /// <param name="fileProcessingDB">The database hosting the cache.</param>
        /// <param name="page">The page to check</param>
        /// <param name="dataType">An <see cref="ECacheDataType"/> enum (treated as flags) that specify
        /// all types of data to check for on the specified page.</param>
        /// <returns><c>true</c> if cached data is found; otherwise <c>false</c>.</returns>
        public static bool IsPageDataCached(this ControllerBase controller,
            FileProcessingDB fileProcessingDB, int page, ECacheDataType dataType)
        {
            try
            {
                int fileTaskSessionId = controller.GetActiveDocumentSessionId();

                var cachedPages = (int[])fileProcessingDB.GetCachedPageNumbers(fileTaskSessionId, dataType);

                return cachedPages.Contains(page);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI49462");
            }
        }

        /// <summary>
        /// Creates a new <see cref="DocumentAttribute"/>.
        /// </summary>
        /// <param name="value">The attribute value</param>
        /// <param name="type">The attribute type</param>
        /// <param name="page">The page the attribute is on.</param>
        /// <param name="startX">SpatialLineZone coordinate</param>
        /// <param name="startY">SpatialLineZone coordinate</param>
        /// <param name="endX">SpatialLineZone coordinate</param>
        /// <param name="endY">SpatialLineZone coordinate</param>
        /// <param name="height">SpatialLineZone coordinate</param>
        public static DocumentAttribute CreateDocumentAttribute(string value, string type,
            int page, int startX, int startY, int endX, int endY, int height)
        {
            return new DocumentAttribute()
            {
                ID = Guid.NewGuid().ToString(),
                Name = "Data",
                Value = value,
                Type = type,
                HasPositionInfo = true,
                SpatialPosition = new Position()
                {
                    Pages = new[] { page }.ToList(),
                    LineInfo = new[]
                    {
                        new SpatialLine()
                        {
                            SpatialLineZone = new SpatialLineZone()
                            {
                                PageNumber = page,
                                StartX = startX,
                                StartY = startY,
                                EndX = endX,
                                EndY = endY,
                                Height = height
                            }
                        }
                    }.ToList()
                }
            };
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
        public static void AssertResultCode(this Task<IActionResult> resultTask, int expectedCode, string message = "")
        {
            AssertResultCode(resultTask.Result, expectedCode, message);
        }

        /// <summary>
        /// Validates a the <see cref="IActionResult"/> has the specified HTTP status code.
        /// </summary>
        public static void AssertResultCode(this IActionResult result, int expectedCode, string message = "")
        {
            try
            {
                if (result is ObjectResult objectResult)
                {
                    Assert.AreEqual(expectedCode, objectResult.StatusCode, message);
                }
                else if (result is StatusCodeResult statusCodeResult)
                {
                    Assert.AreEqual(expectedCode, statusCodeResult.StatusCode, message);
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

        public static string SerializeWebConfig(ICommonWebConfiguration config)
        {
            _ = config ?? throw new ArgumentNullException(nameof(config));

            IDomainObject domainObject = config as IDomainObject;
            ExtractException.Assert("ELI53804", "Configuration object cannot be serialized", domainObject is not null);

            return _webConfigSerializer.Serialize(domainObject.CreateDataTransferObject());
        }

        public static (
                FileProcessingDB fileProcessingDb,
                User user,
                TController controller,
                Dictionary<int, DocumentProcessingStatus> expectedStatuses)
            CreateStatusTestEnvironment<TTestClass, TController>(
                FAMTestDBManager<TTestClass> testDBManager,
                Func<IConfigurationDatabaseService, TController> controller,
                string apiVersion,
                string dbName,
                string username,
                string password) where TController : ControllerBase
        {
            Mock<IConfigurationDatabaseService> mock = new();
            mock.Setup(x => x.DocumentAPIWebConfigurations).Returns(new List<IDocumentApiWebConfiguration>()
                { _labDEFileStatusConfiguration });
            mock.Setup(x => x.Configurations).Returns(new List<ICommonWebConfiguration>() { _labDEFileStatusConfiguration });

            (FileProcessingDB fileProcessingDb, User user, TController outputController) =
            testDBManager.InitializeEnvironment(
                controller: () => controller(mock.Object)
                , apiVersion: apiVersion
                , dbResource: "Resources.Demo_LabDE.bak"
                , dbName: dbName
                , username: username
                , password: password
                , webConfiguration: _labDEFileStatusConfiguration);

            fileProcessingDb.RenameAction("B01_ViewNonLab", "A04_Output");
            fileProcessingDb.DefineNewAction("A05_Cleanup");
            fileProcessingDb.SetWorkflowActions(1, new[]
                {
                    new object[] { "A01_ExtractData", true }.ToVariantVector(), // Main sequence
                    new object[] { "A02_Verify", true }.ToVariantVector(),      // Main sequence
                    new object[] { "A03_QA", false }.ToVariantVector(), 
                    new object[] { "A04_Output", true }.ToVariantVector(),      // Main sequence
                    new object[] { "A05_Cleanup", false }.ToVariantVector()
                }.ToIUnknownVector<VariantVector>());

            Dictionary<int, DocumentProcessingStatus> expectedStatuses = new();

            // A01: P, A02: U, A03: U, A04: U, A05: U
            expectedStatuses[1] = DocumentProcessingStatus.Processing;
            fileProcessingDb.SetStatusForFile(1, "A01_ExtractData", -1, EActionStatus.kActionPending, false, false, out _);
            fileProcessingDb.SetStatusForFile(1, "A02_Verify", -1, EActionStatus.kActionUnattempted, false, false, out _);

            // A01: C, A02: U, A03: U, A04: U, A05: U
            // Not complete, but not pending/pending for any main sequence action
            expectedStatuses[2] = DocumentProcessingStatus.Incomplete;
            fileProcessingDb.SetStatusForFile(2, "A02_Verify", -1, EActionStatus.kActionUnattempted, false, false, out _);

            // A01: C, A02: C, A03: U, A04: C, A05: U
            expectedStatuses[3] = DocumentProcessingStatus.Done;
            fileProcessingDb.SetStatusForFile(3, "A02_Verify", -1, EActionStatus.kActionCompleted, false, false, out _);
            fileProcessingDb.SetStatusForFile(3, "A04_Output", -1, EActionStatus.kActionCompleted, false, false, out _);

            // A01: C, A02: C, A03: U, A04: C, A05: F
            // Even tho A05_Cleanup which is not main sequence failed
            expectedStatuses[4] = DocumentProcessingStatus.Done;
            fileProcessingDb.SetStatusForFile(4, "A02_Verify", -1, EActionStatus.kActionCompleted, false, false, out _);
            fileProcessingDb.SetStatusForFile(4, "A04_Output", -1, EActionStatus.kActionCompleted, false, false, out _);
            fileProcessingDb.SetStatusForFile(4, "A05_Cleanup", -1, EActionStatus.kActionFailed, false, false, out _);

            // A01: C, A02: F, A03: U, A04: U, A05: U
            expectedStatuses[5] = DocumentProcessingStatus.Failed;
            fileProcessingDb.SetStatusForFile(5, "A02_Verify", -1, EActionStatus.kActionFailed, false, false, out _);

            // A01: C, A02: U, A03: U, A04: C, A05: U
            // Even tho A02_Verify unattempted
            expectedStatuses[6] = DocumentProcessingStatus.Done;
            fileProcessingDb.SetStatusForFile(6, "A04_Output", -1, EActionStatus.kActionCompleted, false, false, out _);

            // A01: C, A02: S, A03: U, A04: U, A05: U
            // Not complete, but not pending/pending for any main sequence action
            expectedStatuses[7] = DocumentProcessingStatus.Incomplete;
            fileProcessingDb.SetStatusForFile(7, "A02_Verify", -1, EActionStatus.kActionSkipped, false, false, out _);

            // A01: C, A02: U, A03: F, A04: C, A05: U
            // Even tho verify unattempted, A03_QA failed (not main sequence)
            expectedStatuses[8] = DocumentProcessingStatus.Done;
            fileProcessingDb.SetStatusForFile(8, "A03_QA", -1, EActionStatus.kActionFailed, false, false, out _);
            fileProcessingDb.SetStatusForFile(8, "A04_Output", -1, EActionStatus.kActionCompleted, false, false, out _);

            // A01: C, A02: P, A03: U, A04: C, A05: U
            // Despite end action completed, it is pending in main workflow action
            expectedStatuses[9] = DocumentProcessingStatus.Processing;
            fileProcessingDb.SetStatusForFile(9, "A04_Output", -1, EActionStatus.kActionCompleted, false, false, out _);

            // A01: C, A02: P, A03: U, A04: U, A05: U
            // All remaining files in the DB where action statuses haven't been changed.
            int remainingFiles = (int)fileProcessingDb.GetFileCount(false) - 9;
            expectedStatuses.AddRange(Enumerable.Range(10, remainingFiles)
                .ToDictionary(id => id, id => DocumentProcessingStatus.Processing));

            return (fileProcessingDb, user, outputController, expectedStatuses);
        }
    }
}
