using Extract.FileActionManager.Database.Test;
using Extract.Testing.Utilities;
using NUnit.Framework;
using System;
using System.Data.SqlClient;
using System.Linq;

namespace Extract.SqlDatabase.Tests
{
    [TestFixture()]
    public class SqlUtilTests
    {
        /// <summary>
        /// Manages test FAM DBs
        /// </summary>
        static FAMTestDBManager<SqlUtilTests> testDbManager;

        #region Overhead

        /// <summary>
        /// Initializes the test fixture.
        /// </summary>
        [OneTimeSetUp]
        public static void Setup()
        {
            GeneralMethods.TestSetup();

            testDbManager = new FAMTestDBManager<SqlUtilTests>();
        }

        /// <summary>
        /// Cleanup after all tests have run.
        /// </summary>
        [OneTimeTearDown]
        public static void FinalCleanup()
        {
            if (testDbManager != null)
            {
                testDbManager.Dispose();
                testDbManager = null;
            }
        }

        #endregion Overhead

        [Test]
        [Category("Automated")]
        [Category("SQLUtil")]
        public static void CreateApplicationRoleTest()
        {
            string testDBName = "TestCreateApplicationRoleTest";
            try
            {
                var fileProcessingDb = testDbManager.GetNewDatabase(testDBName);

                using var applicationRoleConnection = new NoAppRoleConnection(fileProcessingDb.DatabaseServer, fileProcessingDb.DatabaseName);
                SqlConnection connection = applicationRoleConnection.SqlConnection;

                Assert.DoesNotThrow(() => { SqlApplicationRole.CreateApplicationRole(connection, "TestRole", "Test-Password2", SqlApplicationRole.AppRoleAccess.AllAccess); }
                    , "CreateApplication call must not throw an exception");

                // Check that the application role exists
                using var cmd = connection.CreateCommand();
                cmd.CommandText = "SELECT Count(name) FROM sys.database_principals p where type_desc = 'APPLICATION_ROLE' AND name = 'TestRole'";
                var result = cmd.ExecuteScalar();
                Assert.AreEqual(1, (int)result, "Application role 'TestRole' should exist");

                connection.Close();

            }
            finally
            {
                testDbManager.RemoveDatabase(testDBName);
            }
        }
    }
}