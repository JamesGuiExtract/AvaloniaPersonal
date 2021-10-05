using Extract.FileActionManager.Database.Test;
using Extract.SqlDatabase;
using Extract.Testing.Utilities;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace Extract.ETL.Test
{
    [TestFixture]
    public class TestHIMStatsService
    {
        #region Constants

        static readonly string _DATABASE = "Resources.HIMStatsDB.bak";
        static readonly string _DATA_UPDATE_HIMSTATS_TEST_SQL = "Resources.DataUpdate_HIMStatsTest.sql";
        static readonly string _REPORT_HIMSTATS_DATA = @"SELECT 
                          [PaginationID]
                          ,[FAMUserID]
                          ,[SourceFileID]
                          ,[DestFileID]
                          ,[OriginalFileID]
                          ,[DateProcessed]
                          ,[ActionID]
                          ,[FileTaskSessionID]
                          ,[ActionName]
                FROM [dbo].[ReportingHIMStats] ";

        #endregion

        #region Fields

        static CancellationToken _noCancel = new CancellationToken(false);
        static CancellationToken _cancel = new CancellationToken(true);

        Dictionary<int, Dictionary<string, object>> expectedData = new Dictionary<int, Dictionary<string, object>>()
        {
            // This record is for the file completed outside of pagination
            {
                0,
                new Dictionary<string, object>()
                {
                    {"PaginationID", DBNull.Value },
                    {"FAMUserID", 1 },
                    {"SourceFileID", 2 },
                    {"DestFileID", 2 },
                    {"OriginalFileID", 2 },
                    {"DateProcessed", new DateTime(2018,12,20) },
                    {"ActionID", 4 },
                    {"FileTaskSessionID", 4 },
                    {"ActionName", "A02_Verify" },
                    {"ID", 1 }
                }
            },

            // This record was processed through pagination
            {
                1,
                new Dictionary<string, object>()
                {
                    {"PaginationID", 1 },
                    {"FAMUserID", 1 },
                    {"SourceFileID", 1 },
                    {"DestFileID", 3 },
                    {"OriginalFileID", 1 },
                    {"DateProcessed", new DateTime(2018,12,20) },
                    {"ActionID", 8 },
                    {"FileTaskSessionID", 2 },
                    {"ActionName", "B01_ViewNonLab" },
                    {"ID", 2 }
                }
            },

            // This record was processed through pagination
            {
                2,
                new Dictionary<string, object>()
                {
                    {"PaginationID", 2 },
                    {"FAMUserID", 1 },
                    {"SourceFileID", 1 },
                    {"DestFileID", 4 },
                    {"OriginalFileID", 1 },
                    {"DateProcessed", new DateTime(2018,12,20) },
                    {"ActionID", 8 },
                    {"FileTaskSessionID", 2 },
                    {"ActionName", "B01_ViewNonLab" },
                    {"ID", 3 }
                }
            }
        };

        #endregion

        #region Overhead

        /// <summary>
        /// Initializes the test fixture.
        /// </summary>
        [OneTimeSetUp]
        public static void Setup()
        {
            GeneralMethods.TestSetup();
        }

        /// <summary>
        /// Cleanup after all tests have run.
        /// </summary>
        [OneTimeTearDown]
        public static void FinalCleanup()
        {
        }

        #endregion Overhead

        /// <summary>
        /// Test to verify the HIMStats services processes records as expected
        /// </summary>
        [Test]
        [Category("Automated")]
        [Category("ETL")]
        public void ProcessPaginationAndOtherVerify()
        {
            using (var testManager = new FAMTestDBManager<TestHIMStatsService>())
            {
                string testDBName = "Test_ProcessPaginationAndOtherVerify";
                try
                {
                    var fileProcessingDb = testManager.GetDatabase(_DATABASE, testDBName);
                    HIMStats himStats = new HIMStats()
                    {
                        DatabaseName = fileProcessingDb.DatabaseName,
                        DatabaseServer = fileProcessingDb.DatabaseServer
                    };

                    himStats.AddToDatabase(himStats.DatabaseServer, himStats.DatabaseName);

                    using var connection = new ExtractRoleConnection(himStats.DatabaseServer, himStats.DatabaseName);
                    connection.Open();
                    using var cmd = connection.CreateCommand();

                    cmd.CommandText = "SELECT Status, LastFileTaskSessionIDProcessed FROM DatabaseService WHERE ID = @DatabaseServiceID";
                    cmd.Parameters.AddWithValue("@DatabaseServiceID", himStats.DatabaseServiceID);
                    var reader = cmd.ExecuteReader();
                    var record = reader.Cast<IDataRecord>().First();

                    var status = himStats.Status as HIMStats.HIMStatsStatus;

                    Assert.AreEqual(status.ToJson(), record["Status"], "Status from database should be same as objects.");

                    Assert.AreEqual(status.LastFileTaskSessionIDProcessed, record["LastFileTaskSessionIDProcessed"],
                        "LastFileTaskSessionIDProcessed from database should be same as that in status");

                    reader.Close();
                    reader.Dispose();

                    // Process with cancel
                    Assert.Throws<ExtractException>(() => himStats.Process(_cancel), "Process with cancel should throw an ExtractException");

                    reader = cmd.ExecuteReader();
                    record = reader.Cast<IDataRecord>().First();

                    himStats.RefreshStatus();
                    status = himStats.Status as HIMStats.HIMStatsStatus;

                    Assert.AreEqual(status.ToJson(), record["Status"], "Status from database should be same as objects.");

                    Assert.AreEqual(status.LastFileTaskSessionIDProcessed, record["LastFileTaskSessionIDProcessed"],
                        "LastFileTaskSessionIDProcessed from database should be same as that in status");

                    reader.Close();
                    reader.Dispose();

                    Assert.DoesNotThrow(() => himStats.Process(_noCancel), "Process without cancel should not throw exception");

                    Assert.Throws<ExtractException>(() => himStats.Process(_cancel), "Process with cancel should throw an ExtractException");

                    // verify status was updated properly
                    reader = cmd.ExecuteReader();
                    record = reader.Cast<IDataRecord>().First();

                    himStats.RefreshStatus();
                    status = himStats.Status as HIMStats.HIMStatsStatus;

                    Assert.AreEqual(status.ToJson(), record["Status"], "Status from database should be same as objects.");

                    Assert.AreEqual(status.LastFileTaskSessionIDProcessed, record["LastFileTaskSessionIDProcessed"],
                        "LastFileTaskSessionIDProcessed from database should be same as that in status");

                    Assert.AreEqual(6, status.LastFileTaskSessionIDProcessed, "LastFileTaskSessionIDProcessed should be 6");

                    reader.Close();
                    reader.Dispose();

                    // Check that ReportingHIMStats contains expected data
                    cmd.CommandText = "SELECT * FROM ReportingHIMStats";
                    using reader = cmd.ExecuteReader();
                    var results = reader.Cast<IDataRecord>().ToList();
                    for (int i = 0; i < results.Count; i++)
                    {
                        Assert.That(expectedData.ContainsKey(i));
                        var dr = Enumerable.Range(0, results[i].FieldCount)
                                    .ToDictionary(d => results[i].GetName(d), d => results[i].GetValue(d));
                        var er = expectedData[i];
                        CollectionAssert.AreEquivalent(er, dr);
                    }

                    reader.Close();
                }
                finally
                {
                    testManager.RemoveDatabase(testDBName);
                }
            }
        }

        /// <summary>
        /// Test to verify the HIMStats services processes records as expected
        /// </summary>
        [Test]
        [Category("Automated")]
        [Category("ETL")]
        public static void MultipleBatchTest()
        {
            using var testFileManager = new TestFileManager<TestHIMStatsService>();
            using var testManager = new FAMTestDBManager<TestHIMStatsService>();

            string testDBName1 = "Test_MultipleBatch1";
            string testDBName2 = "Test_MultipleBatch2";
            try
            {
                var famDB1 = testManager.GetNewDatabase(testDBName1);
                var sqlUpdateFile = testFileManager.GetFile(_DATA_UPDATE_HIMSTATS_TEST_SQL);
                var updateSql = System.IO.File.ReadAllText(sqlUpdateFile);
                famDB1.ExecuteCommandQuery(updateSql);

                HIMStats himStats = new()
                {
                    DatabaseName = famDB1.DatabaseName,
                    DatabaseServer = famDB1.DatabaseServer
                };

                himStats.AddToDatabase(himStats.DatabaseServer, himStats.DatabaseName);

                himStats.BatchSize = 1;
                himStats.Process(_noCancel);

                himStats.BatchSize = 100;
                himStats.Process(_noCancel);

                var famDB2 = testManager.GetNewDatabase(testDBName2);
                famDB2.ExecuteCommandQuery(updateSql);

                HIMStats himExpected = new()
                {
                    DatabaseName = famDB2.DatabaseName,
                    DatabaseServer = famDB2.DatabaseServer
                };

                himExpected.AddToDatabase(himExpected.DatabaseServer, himExpected.DatabaseName);
                himExpected.BatchSize = 101;
                himExpected.Process(_noCancel);

                // The data for each should be the same
                using var connection1 = NewSqlConnection(famDB1.DatabaseServer, famDB1.DatabaseName);
                using var cmd1 = connection1.CreateCommand();
                connection1.Open();
                cmd1.CommandText = _REPORT_HIMSTATS_DATA;
                using var reader1 = cmd1.ExecuteReader();

                using var connection2 = NewSqlConnection(famDB2.DatabaseServer, famDB2.DatabaseName);
                connection2.Open();
                using var cmd2 = connection2.CreateCommand();
                cmd2.CommandText = _REPORT_HIMSTATS_DATA;

                using var reader2 = cmd2.ExecuteReader();

                CheckResults(reader1, reader2);

            }
            finally
            {
                testManager.RemoveDatabase(testDBName1);
                testManager.RemoveDatabase(testDBName2);
            }
        }

        #region Helper methods

        /// <summary>
        /// Compares what is in the foundResults with expected
        /// </summary>
        /// <param name="found">The SqlDataReader for the results generated</param>
        /// <param name="expected">The Expected results</param>
        static void CheckResults(SqlDataReader found, SqlDataReader expected)
        {

            // Compare the found results to the expected
            var foundResults = ConvertResults(found);
            var expectedResults = ConvertResults(expected);

            Assert.That(foundResults.Count() == expectedResults.Count,
                string.Format(CultureInfo.InvariantCulture, "Found {0} and expected {1} ",
                foundResults.Count(), expectedResults.Count));

            Assert.That(foundResults
                    .OrderBy(a => a.PaginationID)
                .SequenceEqual(expectedResults
                    .OrderBy(r => r.PaginationID )),
                "Compare the actual data with the expected");

            found.Close();
            expected.Close();
        }

        private static List<(int PaginationID, int FAMUserID, int SourceFileID, int DestFileID, int OriginalFileID, int ActionID, int FileTaskSessionID, string ActionName)>
            ConvertResults(SqlDataReader reader)
        { 
            // Convert reader to IEnummerable 
            var results  = reader.Cast<IDataRecord>();

            return results.Select(r => (
                   PaginationID: r.GetInt32(r.GetOrdinal("PaginationID")),
                   FAMUserID: r.GetInt32(r.GetOrdinal("FAMUserID")),
                   SourceFileID: r.GetInt32(r.GetOrdinal("SourceFileID")),
                   DestFileID: (r.IsDBNull(r.GetOrdinal("DestFileID"))) ? 0 : r.GetInt32(r.GetOrdinal("DestFileID")),
                   OriginalFileID: r.GetInt32(r.GetOrdinal("OriginalFileID")),
                   ActionID: r.GetInt32(r.GetOrdinal("ActionID")),
                   FileTaskSessionID: r.GetInt32(r.GetOrdinal("FileTaskSessionID")),
                   ActionName: r.GetString(r.GetOrdinal("ActionName")))).ToList();
        }


        /// <summary>
        /// Creates a new <see cref="SqlConnection"/> with the given database server and database name
        /// </summary>
        /// <param name="databaseServer">Database server to use</param>
        /// <param name="databaseName">Database name to use</param>
        /// <returns>A new <see cref="SqlConnection"/></returns>
        static SqlConnection NewSqlConnection(string databaseServer, string databaseName)
        {
            // Build the connection string from the settings
            SqlConnectionStringBuilder sqlConnectionBuild = new SqlConnectionStringBuilder();
            sqlConnectionBuild.DataSource = databaseServer;
            sqlConnectionBuild.InitialCatalog = databaseName;

            sqlConnectionBuild.IntegratedSecurity = true;
            sqlConnectionBuild.NetworkLibrary = "dbmssocn";
            sqlConnectionBuild.MultipleActiveResultSets = true;

            return new SqlConnection(sqlConnectionBuild.ConnectionString);
        }

        #endregion
    }
}
