using Extract.FileActionManager.Database.Test;
using Extract.Testing.Utilities;
using NUnit.Framework;
using System;
using System.Data.SqlClient;

namespace Extract.SqlDatabase.Tests
{
    [TestFixture()]
    public class SqlApplicationRoleTest
    {
        class TestAppRoleConnection : SqlAppRoleConnection
        {
            public string Role { get; set; }
            public string Password { get; set; }

            public TestAppRoleConnection(string server, string database, bool enlist = true)
                : base(SqlUtil.NewSqlDBConnection(server, database, enlist))
            {
            }

            public TestAppRoleConnection(string connectionString)
                : base(SqlUtil.NewSqlDBConnection(connectionString))
            {
            }

            protected override void AssignRole()
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(Role) || string.IsNullOrWhiteSpace(Password))
                        return;

                    SetApplicationRole(Role, Password);
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI51850");
                }
            }
        }

        /// <summary>
        /// Manages test FAM DBs
        /// </summary>
        static FAMTestDBManager<SqlApplicationRoleTest> testDbManager;

        #region Overhead

        /// <summary>
        /// Initializes the test fixture.
        /// </summary>
        [OneTimeSetUp]
        public static void Setup()
        {
            GeneralMethods.TestSetup();

            testDbManager = new FAMTestDBManager<SqlApplicationRoleTest>();
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
        [Category("SQLApplicationRoleTests")]
        [TestCase(SqlApplicationRole.AppRoleAccess.NoAccess, "TestSqlApplicationRoleTest_NoAccess", Description = "Sql Application role for no access")]
        [TestCase(SqlApplicationRole.AppRoleAccess.SelectExecuteAccess, "TestSqlApplicationRoleTest_SelectExecuteAccess", Description = "Sql Application role for Select and Execute access")]
        [TestCase(SqlApplicationRole.AppRoleAccess.InsertAccess, "TestSqlApplicationRoleTest_InsertAccess", Description = "Sql Application role for Insert access")]
        [TestCase(SqlApplicationRole.AppRoleAccess.UpdateAccess, "TestSqlApplicationRoleTest_UpdateAccess", Description = "Sql Application role for Update access")]
        [TestCase(SqlApplicationRole.AppRoleAccess.DeleteAccess, "TestSqlApplicationRoleTest_DeleteAccess", Description = "Sql Application role for Delete access")]
        [TestCase(SqlApplicationRole.AppRoleAccess.AllAccess, "TestSqlApplicationRoleTest_AllAccess", Description = "Sql Application role for All access")]
        public void TestSqlApplicationRole(SqlApplicationRole.AppRoleAccess access, string testDBName)
        {
            try
            {
                var fileProcessingDb = testDbManager.GetNewDatabase(testDBName);

                using var roleConnection = new NoAppRoleConnection(fileProcessingDb.DatabaseServer, fileProcessingDb.DatabaseName);
                roleConnection.Open();

                // create new records in the DBInfo table that will be used in the test
                using (var cmd = roleConnection.CreateCommand())
                {
                    cmd.CommandText = "INSERT INTO DBInfo (Name, Value) VALUES ('Access Set', 'Access Set'), ('Restored access', 'Restored access');";
                    cmd.ExecuteNonQuery();
                }

                SqlApplicationRole.CreateApplicationRole(roleConnection, "TestRole", "Test-Password2", access);
                using (var sqlApplicationRole = new TestAppRoleConnection(fileProcessingDb.DatabaseServer, fileProcessingDb.DatabaseName))
                {
                    sqlApplicationRole.Role = "TestRole";
                    sqlApplicationRole.Password = "Test-Password2";
                    sqlApplicationRole.Open();
                    CheckAccess(access, sqlApplicationRole, "Access Set");
                    sqlApplicationRole.Close();

                    sqlApplicationRole.Role = string.Empty;
                    sqlApplicationRole.Password = string.Empty;
                    sqlApplicationRole.Open();
                    CheckAccess(SqlApplicationRole.AppRoleAccess.AllAccess, sqlApplicationRole, "Restored access");
                }
            }
            finally
            {
                testDbManager.RemoveDatabase(testDBName);
            }
        }

        private static void CheckAccess(SqlApplicationRole.AppRoleAccess access, SqlAppRoleConnection connection, string description)
        {
            // Verify that the required access is given
            using var selectCmd = ValidateSelectCommand(access, connection, description);
            using var insertCmd = connection.CreateCommand();
            insertCmd.CommandText = $"INSERT INTO DBInfo (Name, Value) VALUES( '{description}_TestName_Delete', '1');";
            if ((access & SqlApplicationRole.AppRoleAccess.InsertAccess & ~SqlApplicationRole.AppRoleAccess.SelectExecuteAccess) > 0)
            {
                Assert.DoesNotThrow(() =>
                {
                    Assert.AreEqual(1, insertCmd.ExecuteNonQuery(), $"{description}: Should add one record");
                }, $"{description}: Insert statement should execute ");
                // verify that the data was added to the table
                using var cmd = connection.CreateCommand();
                cmd.CommandText = $"SELECT COUNT (Name) FROM DBInfo WHERE Name = '{description}_TestName_Delete' and Value = '1'";
                Assert.AreEqual(1, cmd.ExecuteScalar(), $"{description}: Should be 1 record");
            }
            else
            {
                Assert.Throws<SqlException>(() =>
                {
                    insertCmd.ExecuteNonQuery();
                }, $"{description}: Insert command should throw SqlException");
            }
            using var updateCmd = connection.CreateCommand();
            updateCmd.CommandText = "UPDATE DBInfo Set Value = '200' WHERE Name = 'CommandTimeout'";
            if ((access & SqlApplicationRole.AppRoleAccess.UpdateAccess & ~SqlApplicationRole.AppRoleAccess.SelectExecuteAccess) > 0)
            {
                Assert.DoesNotThrow(() =>
                {
                    Assert.AreEqual(1, updateCmd.ExecuteNonQuery(), $"{description}: Should update 1 record");
                }, $"{description}: Update statement should execute");
            }
            else
            {
                Assert.Throws<SqlException>(() =>
                {
                    updateCmd.ExecuteNonQuery();
                }, $"{description}: Update command should throw SqlException");

            }
            using var deleteCmd = connection.CreateCommand();
            deleteCmd.CommandText = $"DELETE TOP(1) FROM DBInfo";
            if ((access & SqlApplicationRole.AppRoleAccess.DeleteAccess & ~SqlApplicationRole.AppRoleAccess.SelectExecuteAccess) > 0)
            {
                Assert.DoesNotThrow(() =>
                {
                    Assert.AreEqual(1, deleteCmd.ExecuteNonQuery(), $"{description}: Should delete one record");
                }, $"{description}: Delete statement should execute");
            }
            else
            {
                Assert.Throws<SqlException>(() =>
                {
                    deleteCmd.ExecuteNonQuery();
                }, $"{description}: Delete statement should throw SqlException");
            }
        }

        private static SqlCommand ValidateSelectCommand(SqlApplicationRole.AppRoleAccess access, SqlAppRoleConnection connection, string description)
        {
            using SqlCommand selectCmd = connection.CreateCommand();
            selectCmd.CommandText = "SELECT Count(ID) FROM DBINFO";
            if ((access & SqlApplicationRole.AppRoleAccess.SelectExecuteAccess) > 0)
            {
                Assert.DoesNotThrow(() =>
                {
                    Assert.Greater((int)selectCmd.ExecuteScalar(), 0, $"{description}: Number of records in DBInfo should be > 0");
                }, $"{description}: Should execute Select statement on database");

            }
            else
            {
                Assert.Throws<SqlException>(() =>
                {
                    selectCmd.ExecuteScalar();
                }, $"{ description}: Select statement Should throw the SqlException");

            }

            return selectCmd;
        }
    }
}