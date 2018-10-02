using Extract.FileActionManager.Database.Test;
using Extract.Testing.Utilities;
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

        [TestFixtureSetUp]
        public static void Setup()
        {
            GeneralMethods.TestSetup();

            _testDbManager = new FAMTestDBManager<TestUsers>();
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
        /// basic test of login funcitonality - login as admin
        /// </summary>
        [Test, Category("Automated")]
        public static void Test_Login()
        {
            string dbName = "DocumentAPI_Test_Login";

            try
            {
                (FileProcessingDB fileProcessingDb, User user, UsersController userController) =
                    _testDbManager.InitializeEnvironment<TestUsers, UsersController>
                        ("Resources.Demo_LabDE.bak", dbName, "admin", "a");

                var result = userController.Login(user);
                var token = result.AssertGoodResult<JwtSecurityToken>();

                Assert.AreEqual("admin", token.Subject, "Unexpected token subject");
            }
            catch (Exception ex)
            {
                Assert.Fail("Failed: {0}", ex.Message);
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
