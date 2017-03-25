using System;
using System.IO;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Threading;
using System.Xml;

using NUnit.Framework;
using IO.Swagger.Api;
using IO.Swagger.Model;

using Extract.FileActionManager.Database.Test;
using Extract.Testing.Utilities;
using UCLID_FILEPROCESSINGLib;

namespace Extract.Web.DocumentAPI.Test
{
    [TestFixture]
    [NUnit.Framework.Category("WebAPI")]
    public class TestWorkflow
    {
        #region Constants

        /// <summary>
        /// Names for the temporary databases that are extracted from the resource folder and
        /// attached to the local database server, as needed for tests.
        /// </summary>
        static readonly string DbLabDE = "Demo_LabDE_Temp";
        #endregion Constants

        #region Fields

        /// <summary>
        /// test DB Manager, used to extract a database backup file from the resource, and the attach/detach it
        /// to the local database server. 
        /// </summary>
        static FAMTestDBManager<TestDocumentAttributeSet> _testDbManager;

        /// <summary>
        /// If this test invokes the web service, then on tear down this flag is used to signal that
        /// condition and shut down the service.
        /// </summary>
        static bool documentAPIInvoked;

        static bool usedDbLabDE;

        #endregion Fields

        #region Setup and Teardown

        [TestFixtureSetUp]
        public static void Setup()
        {
            GeneralMethods.TestSetup();

            _testDbManager = new FAMTestDBManager<TestDocumentAttributeSet>();

            documentAPIInvoked = Utils.StartWebServer(workingDirectory: Utils.GetWebApiFolder, webApiURL: Utils.WebApiURL);

            _testDbManager.GetDatabase("Resources.Demo_LabDE.bak", DbLabDE);
            usedDbLabDE = true;

            Utils.SetDatabase(DbLabDE, Utils.WebApiURL);
        }

        [TestFixtureTearDown]
        public static void FinalCleanup()
        {
            if (documentAPIInvoked)
            {
                Utils.ShutdownWebServer(args: "/f /im DocumentAPI.exe");
            }

            if (usedDbLabDE)
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
        public static void Test_GetWorkflows()
        {
            try
            {
                // TODO - this will be removed later
                MockWorkflowsForLabDE();

                var wfApi = new IO.Swagger.Api.WorkflowApi(basePath: Utils.WebApiURL);
                var names = wfApi.ApiWorkflowGetWorkflowsGet();
                Assert.IsTrue(names.Count == 4);

                Assert.IsTrue(!String.IsNullOrEmpty(names.Find(name => name.IsEquivalent("Extract Data"))));
                Assert.IsTrue(!String.IsNullOrEmpty(names.Find(name => name.IsEquivalent("Verify"))));
                Assert.IsTrue(!String.IsNullOrEmpty(names.Find(name => name.IsEquivalent("QA"))));
                Assert.IsTrue(!String.IsNullOrEmpty(names.Find(name => name.IsEquivalent("View Non Lab"))));
            }
            catch (Exception ex)
            {
                Assert.Fail("Exception: {0}, in: {1}", ex.Message, Utils.GetMethodName());
            }
        }

        /// <summary>
        /// test to get expected default workflow
        /// </summary>
        [Test, Category("Automated")]
        public static void Test_GetDefaultWorkflow()
        {
            var wfApi = new IO.Swagger.Api.WorkflowApi(basePath: Utils.WebApiURL);
            var defaultWf = wfApi.ApiWorkflowGetDefaultWorkflowByUsernameGet("John Doe");
            Assert.IsTrue(defaultWf.IsEquivalent("Extract_Data"));
        }

        /*
        Mocked workflowStatus values:
		{
		"error": {
			"errorOccurred": false,
			"message": null,
			"code": 0
		},
		"numberProcessing": 14,
		"numberDone": 5,
		"numberFailed": 1,
		"numberIgnored": 0,
		"state": 1
		}
        */
        /// <summary>
        /// Test of workflow status
        /// TODO - in the near future this will be more extensive
        /// </summary>
        [Test, Category("Automated")]
        public static void Test_GetWorkflowStatus()
        {
            var wfApi = new IO.Swagger.Api.WorkflowApi(basePath: Utils.WebApiURL);
            var status = wfApi.ApiWorkflowGetWorkflowStatusByWorkflowNameGet("Extract Data");

            Assert.IsTrue(status.Error.ErrorOccurred == false);
            Assert.IsTrue(status.Error.Code == 0);
            Assert.IsTrue(status.NumberProcessing == 14);
            Assert.IsTrue(status.NumberDone == 5);
            Assert.IsTrue(status.NumberFailed == 1);
            Assert.IsTrue(status.NumberIgnored == 0);
            Assert.IsTrue(status.State.IsEquivalent("Running"));

            // TODO (future)- 2) add a file to the workflow

            // TODO (future)- 3) verify that the workflow changes predictably
        }

        #endregion Public Test Functions

        static List<String> WorkflowGetWorkflows(string webApiUrl)
        {
            Assert.IsFalse(String.IsNullOrEmpty(webApiUrl));

            var wfApi = new IO.Swagger.Api.WorkflowApi(basePath: webApiUrl);
            var wfNames = wfApi.ApiWorkflowGetWorkflowsGet();
            return wfNames;
        }

        static void AddWorkflow(Workflow workflow)
        {
            Assert.IsFalse(workflow == null);

            var wfApi = new IO.Swagger.Api.WorkflowApi(basePath: Utils.WebApiURL);
            wfApi.ApiWorkflowPost(workflow);
        }

        static void MockWorkflowsForLabDE()
        {
            AddWorkflow(MakeWorkflow("Extract Data", 1));
            AddWorkflow(MakeWorkflow("Verify", 2));
            AddWorkflow(MakeWorkflow("QA", 3));
            AddWorkflow(MakeWorkflow("View Non Lab", 4));
        }

        static Workflow MakeWorkflow(string name, int index)
        {
            var sIndex = index.ToString();
            var testWf = new IO.Swagger.Model.Workflow(Name: name,
                                                       Description: "Describe_" + name,
                                                       EntryAction: "entry_" + name,
                                                       ExitAction: "cleanup_" + name,
                                                       RunAfterResultsAction: "runAfter_" + name,
                                                       DocumentFolder: "docFolder_" + name,
                                                       AttributeSetName: "asn_" + name,
                                                       Id: index);
            return testWf;
        }
    }
}
