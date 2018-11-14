using Extract.FileActionManager.Database.Test;
using Extract.FileActionManager.FileProcessors;
using Extract.Testing.Utilities;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;

namespace Extract.UtilityApplications.TrainingCoordinator.Test
{
    /// <summary>
    /// Unit tests for TrainingCoordinator class
    /// </summary>
    [TestFixture]
    [NUnit.Framework.Category("TrainingCoordinator")]
    public class TestTrainingCoordinator
    {
        #region Fields

        /// <summary>
        /// Manages the test data files
        /// </summary>
        static TestFileManager<TestTrainingCoordinator> _testFiles;
        static List<string> _inputFolder = new List<string>();

        /// <summary>
        /// Manages test FAM DBs.
        /// </summary>
        static FAMTestDBManager<TestTrainingCoordinator> _testDbManager;

        public static readonly string DBName = "_TestTrainingCoordinator_FAB72B42-CC90-4E05-8C1C-AC9B2FA16667";

        #endregion Fields

        #region Overhead

        /// <summary>
        /// Setup method to initialize the testing environment.
        /// </summary>
        [TestFixtureSetUp]

        public static void Setup()
        {
            GeneralMethods.TestSetup();
            _testFiles = new TestFileManager<TestTrainingCoordinator>();
            _testDbManager = new FAMTestDBManager<TestTrainingCoordinator>();
        }

        /// <summary>
        /// Cleanup after all tests have run.
        /// </summary>
        [TestFixtureTearDown]
        public static void FinalCleanup()
        {
            // Dispose of the test image manager
            if (_testFiles != null)
            {
                _testFiles.Dispose();
            }

            // The first temp folder exists after it has been deleted (until I close nunit) so to
            // safe, remove them from the list so as not to attempt to delete them more than once if you run
            // test twice.
            for (int i = _inputFolder.Count; i > 0;)
            {
                Directory.Delete(_inputFolder[--i], true);
                _inputFolder.RemoveAt(i);
            }

            if (_testDbManager != null)
            {
                _testDbManager.Dispose();
                _testDbManager = null;
            }
        }

        #endregion Overhead

        #region Helper Methods

        public static void CreateDatabase()
        {
            try
            {
                // Create DB
                _testDbManager.GetNewDatabase(DBName);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI46491");
            }
        }

        #endregion Helper Methods

        #region Tests

        // Test that the status column is used for contained services
        // https://extract.atlassian.net/browse/ISSUE-15366
        [Test, Category("TrainingCoordinator")]
        public static void StatusColumn()
        {
            try
            {
                CreateDatabase();

                var coordinator = new TrainingCoordinator();
                coordinator.DataCollectors = new List<ETL.MachineLearningService>
                {
                    new TrainingDataCollector.TrainingDataCollector
                    {
                        LastIDProcessed = 5
                    }
                };

                int id = coordinator.AddToDatabase("(local)", DBName);

                string statusJson = coordinator.Status.ToJson();

                // Disconnect from the DB and change values
                coordinator.DatabaseServiceID = 0;
                coordinator.DataCollectors[0].LastIDProcessed = 6;

                string modifiedStatusJson = coordinator.Status.ToJson();
                Assert.AreNotEqual(statusJson, modifiedStatusJson);

                Assert.AreEqual(6, coordinator.DataCollectors[0].LastIDProcessed);

                // Reconnect to the DB and confirm values are reset
                coordinator.DatabaseServiceID = id;
                string resetStatusJson = coordinator.Status.ToJson();
                Assert.AreEqual(statusJson, resetStatusJson);

                // Clear the status column
                SqlConnectionStringBuilder sqlConnectionBuild = new SqlConnectionStringBuilder
                {
                    DataSource = "(local)",
                    InitialCatalog = DBName,
                    IntegratedSecurity = true,
                    NetworkLibrary = "dbmssocn"
                };

                using (var connection = new SqlConnection(sqlConnectionBuild.ConnectionString))
                {
                    connection.Open();

                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = "UPDATE DatabaseService SET Status = NULL";
                        cmd.ExecuteNonQuery();
                    }
                }

                // Refresh the status and confirm values are set back to the original values
                // (This is inconsistent and called as a bug in https://extract.atlassian.net/browse/ISSUE-15505
                // but it is arguably more useful than if the default values were used...)
                coordinator.RefreshStatus();
                Assert.AreEqual(5, coordinator.DataCollectors[0].LastIDProcessed);

                // Set back to original values
                using (var connection = new SqlConnection(sqlConnectionBuild.ConnectionString))
                {
                    connection.Open();

                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = "UPDATE DatabaseService SET Status = @Status";
                        cmd.Parameters.AddWithValue("@Status", statusJson);
                        cmd.ExecuteNonQuery();
                    }
                }

                // Refresh the status and confirm values are back to original values
                coordinator.RefreshStatus();
                Assert.AreEqual(5, coordinator.DataCollectors[0].LastIDProcessed);
                string resetToOriginalStatusJson = coordinator.Status.ToJson();
                Assert.AreEqual(statusJson, resetToOriginalStatusJson);
            }
            finally
            {
                _testDbManager.RemoveDatabase(DBName);
            }
        }

        #endregion Tests
    }
}