using Extract.FileActionManager.Database.Test;
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
                string testDBName = "ProcessPaginationAndOtherVerify_Test";
                try
                {
                    var fileProcessingDb = testManager.GetDatabase(_DATABASE, testDBName);
                    HIMStats himStats = new HIMStats()
                    {
                        DatabaseName = fileProcessingDb.DatabaseName,
                        DatabaseServer = fileProcessingDb.DatabaseServer
                    };

                    himStats.AddToDatabase(himStats.DatabaseServer, himStats.DatabaseName);

                    using (var connection = NewSqlConnection(himStats.DatabaseServer, himStats.DatabaseName))
                    {
                        connection.Open();
                        using (var cmd = connection.CreateCommand())
                        {
                            cmd.CommandText = "SELECT Status, LastFileTaskSessionIDProcessed FROM DatabaseService WHERE ID = @DatabaseServiceID";
                            cmd.Parameters.AddWithValue("@DatabaseServiceID", himStats.DatabaseServiceID);
                            var reader = cmd.ExecuteReader();
                            var record = reader.Cast<IDataRecord>().First();

                            var status = himStats.Status as HIMStats.HIMStatsStatus;

                            Assert.AreEqual(status.ToJson(), record["Status"], "Status from database should be same as objects.");

                            Assert.AreEqual(status.LastFileTaskSessionIDProcessed, record["LastFileTaskSessionIDProcessed"],
                                "LastFileTaskSessionIDProcessed from database should be same as that in status");

                            reader.Close();
                            
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

                            // Check that ReportingHIMStats contains expected data
                            cmd.CommandText = "SELECT * FROM ReportingHIMStats";
                            reader = cmd.ExecuteReader();
                            var results = reader.Cast<IDataRecord>().ToList();
                            for (int i = 0;i < results.Count; i++)
                            {
                                Assert.That(expectedData.ContainsKey(i));
                                var dr = Enumerable.Range(0, results[i].FieldCount)
                                            .ToDictionary(d => results[i].GetName(d), d => results[i].GetValue(d));
                                var er = expectedData[i];
                                CollectionAssert.AreEquivalent(er, dr);
                            }

                            reader.Close();
                        }
                    }
                }
                finally
                {
                    testManager.RemoveDatabase(testDBName);
                }
            }
        }

        #region Helper methods

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
