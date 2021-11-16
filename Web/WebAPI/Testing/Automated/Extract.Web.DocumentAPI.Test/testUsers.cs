using Extract.FileActionManager.Database.Test;
using Extract.Testing.Utilities;
using Microsoft.AspNetCore.Http;
using NUnit.Framework;
using System;
using System.IdentityModel.Tokens.Jwt;
using UCLID_FILEPROCESSINGLib;
using WebAPI.Controllers;
using WebAPI.Models;

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
            string dbName = "Test_DocumentAPI_Login";

            try
            {
                (FileProcessingDB fileProcessingDb, User user, UsersController userController) =
                    _testDbManager.InitializeEnvironment<TestUsers, UsersController>
                        (new UsersController(), apiVersion, "Resources.Demo_LabDE.bak", dbName, "Admin", "a");

                // Login should not be allowed for Admin account
                var result = userController.Login(user);
                result.AssertResultCode(StatusCodes.Status401Unauthorized);

                user = new User()
                {
                    Username = "jon_doe",
                    Password = "123",
                    WorkflowName = ApiTestUtils.CurrentApiContext.WorkflowName
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
