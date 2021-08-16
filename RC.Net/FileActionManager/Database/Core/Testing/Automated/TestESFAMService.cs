using Extract.FileActionManager.Utilities.FAMServiceManager;
using Extract.Testing.Utilities;
using Extract.Utilities;
using LinqToDB;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Management;
using System.Threading.Tasks;
using CurrentFAMServiceDBSettings = Extract.FileActionManager.Database.SqliteModels.Version8.FAMServiceDBSettings;

namespace Extract.FileActionManager.Database.Test
{
    [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "ESFAM")]
    [TestFixture]
    [Category("ESFAMService")]
    public class TestESFAMService
    {
        [OneTimeSetUp]
        public static void Setup()
        {
            GeneralMethods.TestSetup();
        }

        /// When a service is installed it should create a sqlite database instead of a sql compact database
        [Test]
        [Category("Automated")]
        public static void NewServiceCreatesSqliteDatabase()
        {
            using var service = InstallTempService();
            string databaseName = GetDatabaseName(service.Name);

            FileAssert.Exists(databaseName);
            FileAssert.DoesNotExist(Path.ChangeExtension(databaseName, ".sdf"));

            // Confirm the DB is valid
            var manager = new FAMServiceSqliteDatabaseManager(databaseName);
            var expectedSettings = CurrentFAMServiceDBSettings.DefaultSettings.Select(s => new KeyValuePair<string, string>(s.Name, s.Value)).ToList();
            var actualSettings = manager.GetSettings().ToList();

            CollectionAssert.AreEquivalent(expectedSettings, actualSettings);
        }

        /// When a service is installed and there is an existing .sdf file for the service but no .sqlite file then the .sdf file should be converted to a .sqlite file
        [Test]
        [Category("Automated")]
        public static void NewServiceConvertsSqlCompactToSqliteDatabase()
        {
            string name = Guid.NewGuid().ToString();
            string displayName = Guid.NewGuid().ToString();
            string databaseName = GetDatabaseName(name);
            string sqlCompactDatabaseName = Path.ChangeExtension(databaseName, ".sdf");

            FileAssert.DoesNotExist(databaseName);
            FileAssert.DoesNotExist(sqlCompactDatabaseName);

            try
            {
                // Create an sdf file
                var manager = new FAMServiceDatabaseManager(sqlCompactDatabaseName);
                manager.CreateDatabase(false);
                FileAssert.Exists(sqlCompactDatabaseName);

                InstallService(name, displayName);
                using var service = new TempService(name);

                // Confirm that the sqlite file has been created from the sdf file
                FileAssert.Exists(databaseName);

                // Confirm the DB is valid
                var sqliteManager = new FAMServiceSqliteDatabaseManager(databaseName);
                var expectedSettings = CurrentFAMServiceDBSettings.DefaultSettings.Select(s => new KeyValuePair<string, string>(s.Name, s.Value)).ToList();
                var actualSettings = sqliteManager.GetSettings().ToList();

                CollectionAssert.AreEquivalent(expectedSettings, actualSettings);
            }
            finally
            {
                File.Delete(sqlCompactDatabaseName);
            }
        }

        /// When a service is run and there is an existing .sdf file for the service then the .sdf should be converted to a .sqlite file
        [Test]
        [Category("Automated")]
        public static async Task RunningServiceConvertsSqlCompactToSqliteDatabase()
        {
            string name = Guid.NewGuid().ToString();
            string displayName = Guid.NewGuid().ToString();
            string databaseName = GetDatabaseName(name);
            string sqlCompactDatabaseName = Path.ChangeExtension(databaseName, ".sdf");

            FileAssert.DoesNotExist(databaseName);
            FileAssert.DoesNotExist(sqlCompactDatabaseName);

            try
            {
                // Create an sdf file
                var manager = new FAMServiceDatabaseManager(sqlCompactDatabaseName);
                manager.CreateDatabase(false);
                FileAssert.Exists(sqlCompactDatabaseName);

                InstallService(name, displayName);
                using var service = new TempService(name);

                // Delete the sqlite file that has been created from the sdf file
                File.Delete(databaseName);

                // Run the service
                FAMService.FAMServiceModule.startService(name);

                await WaitForServiceToStart(name);

                // Confirm that the sqlite file has been created from the sdf file
                FileAssert.Exists(databaseName);

                // Confirm the DB is valid
                var sqliteManager = new FAMServiceSqliteDatabaseManager(databaseName);
                var expectedSettings = CurrentFAMServiceDBSettings.DefaultSettings.Select(s => new KeyValuePair<string, string>(s.Name, s.Value)).ToList();
                var actualSettings = sqliteManager.GetSettings().ToList();

                CollectionAssert.AreEquivalent(expectedSettings, actualSettings);
            }
            finally
            {
                File.Delete(sqlCompactDatabaseName);
            }
        }

        private static async Task WaitForServiceState(string name, string desiredState, string transitionState = "")
        {
            bool transistionStateSeen = false;
            var mgmtObj = FAMService.FAMServiceModule.getFamService(name);
            for (int tries = 0; tries < 50; tries++)
            {
                string actualState = (string)mgmtObj["State"];
                if (actualState == desiredState)
                {
                    return;
                }
                else if (transistionStateSeen && actualState != transitionState)
                {
                    return; // Handle service starting, then stopped without seeing running state
                }
                else if (actualState == transitionState)
                {
                    transistionStateSeen = true;
                }
                await Task.Delay(100);
            }
        }

        private static async Task WaitForServiceToStart(string name)
        {
            await WaitForServiceState(name, "Running", "Start Pending");
        }

        private static async Task WaitForServiceToStop(string name)
        {
            await WaitForServiceState(name, "Stopped");
        }

        private static void InstallService(string serviceName, string serviceDisplayName)
        {
            FAMService.FAMServiceModule.install(serviceName, serviceDisplayName);
        }

        private static async Task RemoveServiceAndDB(string name)
        {
            try
            {
                ManagementBaseObject mgmtObj = FAMService.FAMServiceModule.getFamService(name);
                string state = mgmtObj["State"] as string;
                if (state != "Stopped")
                {
                    FAMService.FAMServiceModule.stopService(name);
                    await WaitForServiceToStop(name);
                }
            }
            catch (Exception) { }

            try
            {
                FAMService.FAMServiceModule.uninstall(name);
            }
            catch (Exception) { }

            try
            {
                File.Delete(GetDatabaseName(name));
            }
            catch (Exception) { }
        }

        private static TempService InstallTempService()
        {
            string name = Guid.NewGuid().ToString();
            string displayName = Guid.NewGuid().ToString();

            InstallService(name, displayName);

            return new TempService(name);
        }

        private static string GetDatabaseName(string serviceName)
        {
            return UtilityMethods.FormatInvariant($@"C:\ProgramData\Extract Systems\ESFAMService\{serviceName}.sqlite");
        }

        private class TempService : IDisposable
        {
            private readonly string _name;

            public string Name => _name;

            public TempService(string name)
            {
                _name = name;
            }

            public void Dispose()
            {
                RemoveServiceAndDB(_name).ConfigureAwait(false).GetAwaiter().GetResult();
            }
        }
    }
}
