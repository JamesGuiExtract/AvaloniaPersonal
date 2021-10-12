using NUnit.Framework;
using System.Threading.Tasks;
using Extract.Testing.Utilities;
using Extract.Utilities;
using System.IO;
using System.Threading;

namespace Extract.LabResultsCustomComponents.Test
{
    [TestFixture]
    [Category("OrderMapperDatabaseSchemaManager")]
    public static class TestOrderMapperDatabaseSchemaManager
    {
        [OneTimeSetUp]
        public static void Setup()
        {
            GeneralMethods.TestSetup();
        }

        /// <summary>
        /// Tests whether opening a newly-created database correctly indicates an update is needed
        /// </summary>
        [Test, Category("Automated")]
        public static void CorrectlyIndicatesUpdateRequiredForNewDatabase()
        {
            using TemporaryFile tempV2 = new(".sdf", false);
            File.Delete(tempV2.FileName);
            var manager = new OrderMapperDatabaseSchemaManager(tempV2.FileName);
            manager.CreateDatabase();

            Assert.IsTrue(manager.IsUpdateRequired);
        }

        /// <summary>
        /// Test whether a V2 order mapper database is correctly updated to post-current schema
        /// </summary>
        [Test, Category("Automated")]
        public static async Task UpdateFromV2ToPostCurrentSchema()
        {
            using TemporaryFile tempV2 = new(".sdf", false);
            File.Delete(tempV2.FileName);
            var manager = new OrderMapperDatabaseSchemaManager(tempV2.FileName);
            manager.CreateDatabase();

            using var tokenSource = new CancellationTokenSource();
            var backupFile = await manager.BeginUpdateToLatestSchema(null, tokenSource);
            File.Delete(backupFile);

            // Check that the database has been upgraded correctly
            // The last update should have set the schema version to current version + 1 in order to hand off management
            // to OrderMapperSqliteDatabaseManager
            Assert.AreEqual(OrderMapperDatabaseSchemaManager.CurrentSchemaVersion + 1, manager.GetSchemaVersion());

            // Make sure the schema manager has been changed 
            var settings = manager.Settings;
            Assert.AreEqual("Extract.LabResultsCustomComponents.OrderMapperSqliteDatabaseManager", settings["DatabaseSchemaManager"]);
        }
    }
}