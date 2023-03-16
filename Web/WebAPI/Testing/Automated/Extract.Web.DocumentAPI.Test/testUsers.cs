using DynamicData.Kernel;
using Extract.FileActionManager.Database.Test;
using Extract.Testing.Utilities;
using Extract.Web.ApiConfiguration.Models;
using Extract.Web.ApiConfiguration.Services;
using Microsoft.AspNetCore.Http;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using UCLID_FILEPROCESSINGLib;
using WebAPI;
using WebAPI.Controllers;

namespace Extract.Web.WebAPI.Test
{
    [TestFixture]
    [NUnit.Framework.Category("DocumentAPI")]
    public class TestUsers
    {
        #region Fields

        /// <summary>
        /// test DB Manager, used to extract a database backup file from the resource, and the attach/detach it
        /// to the local database server. 
        /// </summary>
        static FAMTestDBManager<TestUsers> _testDbManager;

        private static readonly DocumentApiConfiguration _labDEDefaultConfiguration = new(
            configurationName: "DocumentAPITesting",
            isDefault: true,
            workflowName: "CourtOffice",
            attributeSet: "DataFoundByRules",
            processingAction: "A02_Verify",
            postProcessingAction: "Output",
            documentFolder: @"c:\temp\DocumentFolder",
            startAction: "A01_ExtractData",
            endAction: "Z_AdminAction",
            postWorkflowAction: "",
            outputFileNameMetadataField: "Outputfile",
            outputFileNameMetadataInitialValueFunction: "<SourceDocName>.result.tif");

        #endregion Fields

        #region Setup and Teardown

        [OneTimeSetUp]
        public static void Setup()
        {
            GeneralMethods.TestSetup();

            _testDbManager = new FAMTestDBManager<TestUsers>();
        }

        [OneTimeTearDown]
        public static void FinalCleanup()
        {
            if (_testDbManager != null)
            {
                _testDbManager.Dispose();
            }
        }

        #endregion Setup and Teardown

        #region Public Test Functions

        /// <summary>
        /// basic test of login funcitonality - login as admin
        /// </summary>
        [Test, Category("Automated")]
        [TestCase(ApiContext.LEGACY_VERSION)]
        [TestCase(ApiContext.CURRENT_VERSION)]
        public static void Test_Login(string apiVersion)
        {
            string dbName = $"Test_DocumentAPI_Login_{apiVersion}";

            try
            {
                Mock<IConfigurationDatabaseService> mock = new();
                mock.Setup(x => x.DocumentAPIWebConfigurations).Returns(new List<IDocumentApiWebConfiguration>() { _labDEDefaultConfiguration });
                mock.Setup(x => x.Configurations).Returns(new List<ICommonWebConfiguration>() { _labDEDefaultConfiguration });

                (FileProcessingDB fileProcessingDb, User user, UsersController userController) =
                _testDbManager.InitializeEnvironment(
                    controller: () => new UsersController(mock.Object, _labDEDefaultConfiguration)
                    , apiVersion: apiVersion
                    , dbResource: "Resources.Demo_LabDE.bak"
                    , dbName: dbName
                    , username: "Admin"
                    , password: "a"
                    , webConfiguration: _labDEDefaultConfiguration);

               
                // Login should not be allowed for Admin account
                var result = userController.Login(user);
                result.AssertResultCode(StatusCodes.Status401Unauthorized);

                var config = ApiTestUtils.CurrentApiContext.WebConfiguration.ValueOrDefault();
                Assert.NotNull(config);

                user = new User()
                {
                    Username = "jon_doe",
                    Password = "123",
                    WorkflowName = config.WorkflowName,
                    ConfigurationName = config.ConfigurationName
                };

                result = userController.Login(user);

                var token = result.AssertGoodResult<JwtSecurityToken>();

                Assert.AreEqual("jon_doe", token.Subject, "Unexpected token subject");
            }
            finally
            {
                FileApiMgr.Instance.ReleaseAll();
                _testDbManager.RemoveDatabase(dbName);
            }
        }

        #endregion Public Test Functions
    }
}
