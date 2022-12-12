using Extract.FileActionManager.Database.Test;
using Extract.Testing.Utilities;
using Extract.Web.ApiConfiguration.Models;
using Extract.Web.ApiConfiguration.Services;
using Microsoft.AspNetCore.Http;
using NUnit.Framework;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using UCLID_FILEPROCESSINGLib;
using WebAPI;
using WebAPI.Controllers;

namespace Extract.Web.WebAPI.Test
{
    [TestFixture]
    [NUnit.Framework.Category("DocumentAPI")]
    public class TestWorkflow
    {
        #region Fields

        /// <summary>
        /// test DB Manager, used to extract a database backup file from the resource, and the attach/detach it
        /// to the local database server. 
        /// </summary>
        static FAMTestDBManager<TestWorkflow> _testDbManager;

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

            _testDbManager = new FAMTestDBManager<TestWorkflow>();
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
        /// basic test that list of current workflows can be retrieved
        /// </summary>
        [Test, Category("Automated")]
        [TestCase(ApiContext.LEGACY_VERSION)]
        [TestCase(ApiContext.CURRENT_VERSION)]
        public static void Test_GetDefaultWorkflow(string apiVersion)
        {
            string dbName = "Test_DocumentAPI_GetDefaultWorkflow";

            try
            {
                var fileProcessingDB = _testDbManager.GetDatabase("Resources.Demo_LabDE.bak", dbName);
                fileProcessingDB.AddWebAPIConfiguration(_labDEDefaultConfiguration.ConfigurationName, ConfigurationDatabaseService.Serialize(_labDEDefaultConfiguration));


                var c = ApiTestUtils.SetDefaultApiContext(apiVersion, dbName, _labDEDefaultConfiguration);

                var fileApi = FileApiMgr.Instance.GetInterface(c);

                Assert.IsTrue(fileApi.WorkflowName.IsEquivalent("CourtOffice"), "Incorrect value for name: {0}", fileApi.WorkflowName);
                Assert.IsTrue(fileApi.FileProcessingDB.GetWorkflowID(fileApi.WorkflowName) == 1, "Incorrect value for Id: {0}", fileApi.FileProcessingDB.GetWorkflowID(fileApi.WorkflowName));
                Assert.IsTrue(fileApi.APIWebConfiguration.StartWorkflowAction.IsEquivalent("A01_ExtractData"), "Incorrect value for startAction: {0}", fileApi.APIWebConfiguration.StartWorkflowAction);
                Assert.IsTrue(fileApi.APIWebConfiguration.AttributeSet.IsEquivalent("DataFoundByRules"), "Incorrect value for OutputAttributeSet: {0}", fileApi.APIWebConfiguration.AttributeSet);
                Assert.IsTrue(fileApi.WorkflowType == EWorkflowType.kExtraction, "Incorrect value for Type: {0}", fileApi.WorkflowType);
            }
            finally
            {
                FileApiMgr.Instance.ReleaseAll();
                _testDbManager.RemoveDatabase(dbName);
            }
        }

        /// <summary>
        /// basic test that list of current workflows can be retrieved
        /// </summary>
        [Test, Category("Automated")]
        [TestCase(ApiContext.LEGACY_VERSION)]
        [TestCase(ApiContext.CURRENT_VERSION)]
        public static void Test_GetWorkflowStatus(string apiVersion)
        {
            string dbName = "Test_DocumentAPI_GetWorkflowStatus";

            try
            {
                (FileProcessingDB fileProcessingDb, User user, UsersController usersController) =
                _testDbManager.InitializeEnvironment(
                    controller: () =>
                    {
                        var configurationDatabaseService = new ConfigurationDatabaseService(new FileProcessingDBClass() { DatabaseName = dbName, DatabaseServer = "(local)" });
                        return new UsersController(configurationDatabaseService);
                    }
                    , apiVersion: apiVersion
                    , dbResource: "Resources.Demo_LabDE.bak"
                    , dbName: dbName
                    , username: "jon_doe"
                    , password: "123"
                    , webConfiguration: _labDEDefaultConfiguration);

                // Should cause file 1 to be counted as incomplete.
                fileProcessingDb.SetStatusForFile(1, "A02_Verify", -1, EActionStatus.kActionUnattempted, false, false, out EActionStatus oldStatus);

                // Should cause file 2 to be counted as complete.
                fileProcessingDb.SetStatusForFile(2, "A02_Verify", -1, EActionStatus.kActionCompleted, false, false, out EActionStatus oldStatus2);
                fileProcessingDb.SetStatusForFile(2, "Z_AdminAction", -1, EActionStatus.kActionCompleted, false, false, out EActionStatus oldStatus3);

                // Should cause file 3 to be counted as failed.
                fileProcessingDb.SetStatusForFile(3, "A02_Verify", -1, EActionStatus.kActionFailed, false, false, out EActionStatus oldStatus4);

                var result = usersController.Login(user);
                var token = result.AssertGoodResult<JwtSecurityToken>();

                var workflowController = user.SetupController(new WorkflowController());

                var statusResult = workflowController.GetWorkflowStatus()
                    .AssertGoodResult<WorkflowStatusResult>();

                Assert.AreEqual(1, statusResult.NumberIncomplete,
                    "Incorrect incomplete count");
                Assert.AreEqual(15, statusResult.NumberProcessing,
                    "Incorrect processing count");
                Assert.AreEqual(1, statusResult.NumberDone,
                    "Incorrect done count");
                Assert.AreEqual(1, statusResult.NumberFailed,
                    "Incorrect failed count");
            }
            finally
            {
                FileApiMgr.Instance.ReleaseAll();
                _testDbManager.RemoveDatabase(dbName);
            }
        }

        /// <summary>
        /// basic test that list of current workflows can be retrieved
        /// </summary>
        [Test, Category("Automated")]
        [TestCase(ApiContext.LEGACY_VERSION)]
        [TestCase(ApiContext.CURRENT_VERSION)]
        public static void Test_GetFileStatuses(string apiVersion)
        {
            string dbName = "Test_DocumentAPI_GetFileStatuses";

            try
            {
                (FileProcessingDB fileProcessingDb, User user, UsersController usersController) =
                _testDbManager.InitializeEnvironment(
                    controller: () =>
                    {
                        var configurationDatabaseService = new ConfigurationDatabaseService(new FileProcessingDBClass() { DatabaseName = dbName, DatabaseServer = "(local)" });
                        return new UsersController(configurationDatabaseService);
                    }
                    , apiVersion: apiVersion
                    , dbResource: "Resources.Demo_LabDE.bak"
                    , dbName: dbName
                    , username: "jon_doe"
                    , password: "123"
                    , webConfiguration: _labDEDefaultConfiguration);

                // Should cause file 1 to be counted as incomplete.
                fileProcessingDb.SetStatusForFile(1, "A02_Verify", -1, EActionStatus.kActionUnattempted, false, false, out EActionStatus oldStatus);

                // Should cause file 2 to be counted as complete.
                fileProcessingDb.SetStatusForFile(2, "A02_Verify", -1, EActionStatus.kActionCompleted, false, false, out EActionStatus oldStatus2);
                fileProcessingDb.SetStatusForFile(2, "Z_AdminAction", -1, EActionStatus.kActionCompleted, false, false, out EActionStatus oldStatus3);

                // Should cause file 3 to be counted as failed.
                fileProcessingDb.SetStatusForFile(3, "A02_Verify", -1, EActionStatus.kActionFailed, false, false, out EActionStatus oldStatus4);

                var result = usersController.Login(user);
                var token = result.AssertGoodResult<JwtSecurityToken>();

                var workflowController = user.SetupController(new WorkflowController());

                var statusResult = workflowController.GetDocumentStatuses()
                    .AssertGoodResult<FileStatusResult>();

                foreach (var fileStatus in statusResult.FileStatuses)
                {
                    switch (fileStatus.ID)
                    {
                        case 1: Assert.AreEqual("Incomplete", fileStatus.Status); break;
                        case 2: Assert.AreEqual("Done", fileStatus.Status); break;
                        case 3: Assert.AreEqual("Failed", fileStatus.Status); break;
                        case 4: Assert.AreEqual("Processing", fileStatus.Status); break;
                    }
                }
            }
            catch (Exception ex)
            {
                Assert.Fail("Exception: {0}, in: {1}", ex.Message, ApiTestUtils.GetMethodName());
            }
            finally
            {
                FileApiMgr.Instance.ReleaseAll();
                _testDbManager.RemoveDatabase(dbName);
            }
        }

        /// <summary>
        /// basic test that list of current workflows can be retrieved
        /// </summary>
        [Test, Category("Automated")]
        [TestCase(ApiContext.LEGACY_VERSION)]
        [TestCase(ApiContext.CURRENT_VERSION)]
        public static void Test_GetFileDeletion(string apiVersion)
        {
            string dbName = "Test_DocumentAPI_GetFileDeletion";

            try
            {
                (FileProcessingDB fileProcessingDb, User user, UsersController usersController) =
                _testDbManager.InitializeEnvironment(
                    controller: () =>
                    {
                        var configurationDatabaseService = new ConfigurationDatabaseService(new FileProcessingDBClass() { DatabaseName = dbName, DatabaseServer = "(local)" });
                        return new UsersController(configurationDatabaseService);
                    }
                    , apiVersion: apiVersion
                    , dbResource: "Resources.Demo_LabDE.bak"
                    , dbName: dbName
                    , username: "jon_doe"
                    , password: "123"
                    , webConfiguration: _labDEDefaultConfiguration);

                var documentController = user.SetupController(new DocumentController());

                // There are 18 completed files to begin with, there should be 17 after deletion.
                documentController.DeleteDocument(3)
                    .AssertResultCode(StatusCodes.Status204NoContent);

                var workflowController = user.SetupController(new WorkflowController());

                var workflowStatus = workflowController.GetWorkflowStatus()
                    .AssertGoodResult<WorkflowStatusResult>();

                Assert.AreEqual(0, workflowStatus.NumberIncomplete,
                    "Incorrect incomplete count");
                Assert.AreEqual(17, workflowStatus.NumberProcessing,
                    "Incorrect processing count");
                Assert.AreEqual(0, workflowStatus.NumberDone,
                    "Incorrect done count");
                Assert.AreEqual(0, workflowStatus.NumberFailed,
                    "Incorrect failed count");

                var fileStatuses = workflowController.GetDocumentStatuses()
                    .AssertGoodResult<FileStatusResult>();

                Assert.AreEqual(17, fileStatuses.FileStatuses.Count);
                Assert.IsFalse(fileStatuses.FileStatuses.Any(status => status.ID == 3));
            }
            catch (Exception ex)
            {
                Assert.Fail("Exception: {0}, in: {1}", ex.Message, ApiTestUtils.GetMethodName());
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
