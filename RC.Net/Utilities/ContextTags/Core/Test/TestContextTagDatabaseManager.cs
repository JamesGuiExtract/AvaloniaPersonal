using Extract.Testing.Utilities;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Extract.Utilities.ContextTags;

namespace Extract.Utilities.ContextTags.Test
{
    [TestFixture]
    [Category("ContextTagDatabaseManager")]
    public static class TestContextTagDatabaseManager
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
            using var manager = new ContextTagDatabaseManager(tempV2.FileName, false);
            manager.CreateDatabase(false);

            Assert.IsTrue(manager.IsUpdateRequired);
        }

        /// <summary>
        /// Test whether a V7 service database is correctly updated to post-current schema
        /// </summary>
        [Test, Category("Automated")]
        public static async Task UpdateFromV2ToPostCurrentSchema()
        {
            using TemporaryFile tempV2 = new(".sdf", false);
            File.Delete(tempV2.FileName);
            using var manager = new ContextTagDatabaseManager(tempV2.FileName, false);
            manager.CreateDatabase(false);

            using var tokenSource = new CancellationTokenSource();
            var backupFile = await manager.BeginUpdateToLatestSchema(null, tokenSource);
            File.Delete(backupFile);

            // Check that the database has been upgraded correctly
            // The last update should have set the schema version to current version + 1 in order to hand off management
            // to CustomTagsSqliteDatabaseManager
            Assert.AreEqual(ContextTagDatabase.CurrentSchemaVersion + 1, manager.GetSchemaVersion());

            // Make sure the schema manager has been changed 
            var settings = manager.Settings;
            Assert.AreEqual("Extract.Utilities.ContextTags.ContextTagsSqliteDatabaseManager", settings["DatabaseSchemaManager"]);
        }
    }
}
