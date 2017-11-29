﻿using Extract;
using Extract.FileActionManager.Database.Test;
using Extract.Testing.Utilities;
using NUnit.Framework;
using System;
using WebAPI;
using WebAPI.Models;

using WorkflowType = UCLID_FILEPROCESSINGLib.EWorkflowType;

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
        static FAMTestDBManager<TestDocumentAttributeSet> _testDbManager;

        #endregion Fields

        #region Setup and Teardown

        [TestFixtureSetUp]
        public static void Setup()
        {
            GeneralMethods.TestSetup();

            _testDbManager = new FAMTestDBManager<TestDocumentAttributeSet>();
        }

        [TestFixtureTearDown]
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
        public static void Test_GetDefaultWorkflow()
        {
            string dbName = "DocumentAPI_Test_GetDefaultWorkflow";

            try
            {
                _testDbManager.GetDatabase("Resources.Demo_LabDE.bak", dbName);

                var c = ApiTestUtils.SetDefaultApiContext(dbName);
                var fileApi = FileApiMgr.GetInterface(c);

                try
                {
                    var workflow = fileApi.Workflow;
                    Assert.IsTrue(workflow != null, "Couldn't get default workflow");
                    Assert.IsTrue(workflow.Name.IsEquivalent("CourtOffice"), "Incorrect value for name: {0}", workflow.Name);
                    Assert.IsTrue(workflow.Id == 1, "Incorrect value for Id: {0}", workflow.Id);
                    Assert.IsTrue(workflow.StartAction.IsEquivalent("A01_ExtractData"), "Incorrect value for startAction: {0}", workflow.StartAction);
                    Assert.IsTrue(workflow.OutputAttributeSet.IsEquivalent("DataFoundByRules"), "Incorrect value for OutputAttributeSet: {0}", workflow.OutputAttributeSet);
                    Assert.IsTrue(workflow.Type == WorkflowType.kExtraction, "Incorrect value for Type: {0}", workflow.Type);
                }
                catch (Exception)
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                Assert.Fail("Exception: {0}, in: {1}", ex.Message, ApiTestUtils.GetMethodName());
            }
            finally
            {
                FileApiMgr.ReleaseAll();
                _testDbManager.RemoveDatabase(dbName);
            }
        }

        /// <summary>
        /// basic test that list of current workflows can be retrieved
        /// </summary>
        [Test, Category("Automated")]
        public static void Test_GetWorkflowStatus()
        {
            string dbName = "DocumentAPI_Test_GetWorkflowStatus";

            try
            {
                _testDbManager.GetDatabase("Resources.Demo_LabDE.bak", dbName);

                var c = ApiTestUtils.SetDefaultApiContext(dbName);
                FileApiMgr.GetInterface(c);

                try
                {
                    var workflowStatus = WorkflowData.GetWorkflowStatus(Utils.CurrentApiContext);
                    Assert.IsTrue(workflowStatus.Error.ErrorOccurred == false, "status should NOT have the error flag set and it is");
                }
                catch (Exception)
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                Assert.Fail("Exception: {0}, in: {1}", ex.Message, ApiTestUtils.GetMethodName());
            }
            finally
            {
                FileApiMgr.ReleaseAll();
                _testDbManager.RemoveDatabase(dbName);
            }
        }


        #endregion Public Test Functions
    }
}