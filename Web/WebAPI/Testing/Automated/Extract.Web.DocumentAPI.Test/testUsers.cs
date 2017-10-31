using Extract.FileActionManager.Database.Test;
using Extract.Testing.Utilities;
using NUnit.Framework;
using System;
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
        /// basic test of login funcitonality - login as admin
        /// </summary>
        [Test, Category("Automated")]
        public static void Test_Login()
        {
            string dbName = "DocumentAPI_Test_Login";

            try
            {
                _testDbManager.GetDatabase("Resources.Demo_LabDE.bak", dbName);

                ApiTestUtils.SetDefaultApiContext(dbName);

                try
                {
                    var user = new User()
                    {
                        Username = "admin",
                        Password = "a"
                    };

                    using (var userData = new UserData(ApiTestUtils.GetCurrentApiContext))
                    {
                        userData.LoginUser(user);
                    }
                }
                catch (Exception)
                {
                    throw;
                }
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
