using Extract.Testing.Utilities;
using Extract.Utilities;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;
using UCLID_FILEPROCESSORSLib;
using System.Data.SqlClient;
using System.Data;

namespace Extract.FileActionManager.Database.Test
{
    [Category("TestFAMDBStoredProcedures")]
    [TestFixture]
    public class TestFAMDBStoredProcedures
    {
        /// <summary>
        /// Manages test FAM DBs.
        /// </summary>
        static FAMTestDBManager<TestFAMDBStoredProcedures> _testDbManager;

        #region Overhead

        /// <summary>
        /// Initializes the test fixture.
        /// </summary>
        [OneTimeSetUp]
        public static void Setup()
        {
            GeneralMethods.TestSetup();

            _testDbManager = new FAMTestDBManager<TestFAMDBStoredProcedures>();
        }

        /// <summary>
        /// Cleanup after all tests have run.
        /// </summary>
        [OneTimeTearDown]
        public static void FinalCleanup()
        {
            if (_testDbManager != null)
            {
                _testDbManager.Dispose();
                _testDbManager = null;
            }
        }
        #endregion

        [Test]
        [Category("Automated")]
        [Category("Database")]
        [Category("StoredProcedure")]
        [TestCase("OneValue", TestName = "List with one value")]
        [TestCase("First,Second", TestName = "List with 2 values")]
        [TestCase("First,Second, Third", TestName ="List with 3 values with space")]
        public static void Function_FN_TableFromCommaSeparatedList(string commaSeparatedList)
        {
            string testDBName = "Test_fn_TableFromCommaSeparatedList";
            FileProcessingDB famDB = null;
            try
            {
                // Create an empty database
                famDB = _testDbManager.GetNewDatabase(testDBName);

                using var sqlDB = NewSQLConnection(famDB);
                using var cmd = sqlDB.CreateCommand();
                cmd.CommandText = "SELECT ItemValue FROM dbo.fn_TableFromCommaSeparatedList(@List)";
                cmd.Parameters.AddWithValue("@List", commaSeparatedList);
                sqlDB.Open();
                using var reader = cmd.ExecuteReader();
                var results = reader.Cast<IDataRecord>()
                    .Select(r => r.GetString(r.GetOrdinal("ItemValue")));

                var expected = commaSeparatedList.Split(',').Select (x => x.Trim());
                Assert.True(results.SequenceEqual(expected), "Table generated contains the comma separated list values.");
            }
            finally
            {
                famDB?.CloseAllDBConnections();
                famDB = null;
                _testDbManager.RemoveDatabase(testDBName);
            }


        }

        #region Private methods

        private static SqlConnection NewSQLConnection(FileProcessingDB famDB)
        {
            // Build the connection string from the settings
            SqlConnectionStringBuilder sqlConnectionBuild = new SqlConnectionStringBuilder();
            sqlConnectionBuild.DataSource = famDB.DatabaseServer;
            sqlConnectionBuild.InitialCatalog = famDB.DatabaseName;

            sqlConnectionBuild.IntegratedSecurity = true;
            sqlConnectionBuild.NetworkLibrary = "dbmssocn";
            sqlConnectionBuild.MultipleActiveResultSets = true;
            return new SqlConnection(sqlConnectionBuild.ConnectionString);
        }

        #endregion
    }
}
