using Extract.FileActionManager.Database.Test;
using Extract.Testing.Utilities;
using NUnit.Framework;
using System;
using WebAPI.Models;

namespace Extract.Web.WebAPI.Test
{
    [TestFixture]
    [NUnit.Framework.Category("WebAPI")]
    public class TestBackendAPI
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
        static FAMTestDBManager<TestBackendAPI> _testDbManager;

        #endregion Fields

        #region Setup and Teardown

        [TestFixtureSetUp]
        public static void Setup()
        {
            GeneralMethods.TestSetup();

            _testDbManager = new FAMTestDBManager<TestBackendAPI>();
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
        /// basic test of login functionality - login as admin
        /// </summary>
        [Test, Category("Automated")]
        public static void Test_Login()
        {
            string dbName = DbLabDE + "10";

            try
            {
                _testDbManager.GetDatabase("Resources.Demo_LabDE.bak", dbName);

                Utils.SetDefaultApiContext(dbName);

                try
                {
                    var user = new User()
                    {
                        Username = "admin",
                        Password = "a"
                    };

                    using (var userData = new UserData(Utils.GetCurrentApiContext))
                    {
                        Assert.IsTrue(userData.MatchUser(user), "User did not match");
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
