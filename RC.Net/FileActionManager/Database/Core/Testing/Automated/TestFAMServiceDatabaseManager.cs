using Extract.Testing.Utilities;
using Extract.Utilities;
using Extract.Database;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Globalization;
using System.IO;
using System.Threading;

// Tuple used for the fps file name list - FileName, AutoStart, NumberOfFilesToProcess
using FPSFileData = System.Tuple<string, bool, string>;
using System.Data.SqlServerCe;

namespace Extract.FileActionManager.Database.Test
{
    /// <summary>
    /// Testing class for the <see cref="FAMServiceDatabaseManager"/>
    /// </summary>
    [Category("FAMServiceDatabaseManager")]
    [TestFixture]
    public class TestFAMServiceDatabaseManager
    {
        #region Constants

        /// <summary>
        /// Constant collection of data used to build the test FPS file tables in the databases
        /// </summary>
        static readonly List<FPSFileData> _fpsFileNames = new List<FPSFileData>(new FPSFileData[] { 
            new FPSFileData(@"C:\fpsfiles\test1.fps", true, "300"),
            new FPSFileData(@"c:\FPSfiles\test1.fps", false, "100"),
            new FPSFileData(@"C:\fpsfiles\test1.fps", false, "300"),
            new FPSFileData(@"C:\fpsfiles\test2.fps", true, "400"),
            new FPSFileData(@"C:\fpsfiles\test2.fps", false, "200"),
            new FPSFileData(@"C:\fpsFiles\test2.FPS", true, "300"),
            new FPSFileData(@"c:\FPSFILES\test3.Fps", true, "200"),
            new FPSFileData(@"C:\FPSFILES\TEST3.FPS", true, "100"),
            new FPSFileData(@"C:\fpsFiles\test3.FPs", true, "0"),
            new FPSFileData(@"C:\fpsfiles\test4.fps", true, "0"),
            new FPSFileData(@"C:\fpsfiles\test4.fps", false, "0"),
            new FPSFileData(@"C:\fpsfiles\test4.fps", true, "0"),
            new FPSFileData(@"C:\fpsfiles\test4.fps", false, "0"),
            new FPSFileData(@"C:\fpsfiles\test4.fps", true, "0"),
            new FPSFileData(@"C:\fpsfiles\test4.fps", false, "0"),
            new FPSFileData(@"C:\fpsfiles\test4.fps", true, "0"),
            new FPSFileData(@"C:\fpsfiles\test4.fps", false, "0") });

        /// <summary>
        /// Collection of file names to number of times to use, this collection should match
        /// the fps file table on a database that has been upgraded from V2 to V5
        /// </summary>
        static readonly Dictionary<string, int> _fromV2FpsTable = new Dictionary<string, int>(
            StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Collection of file names to number of times to use and number of files to process,
        /// this collection should match the fps file table on a database that has been upgraded
        /// from V3 - V5 to V6
        /// </summary>
        static readonly Dictionary<string, Tuple<int, int>> _fromV3ToV5FpsTable =
            new Dictionary<string, Tuple<int, int>>(StringComparer.OrdinalIgnoreCase);

        #endregion Constants

        #region Setup And Teardown

        /// <summary>
        /// Initializes the test fixture.
        /// </summary>
        [TestFixtureSetUp]
        public static void Setup()
        {
            GeneralMethods.TestSetup();

            var fileNames = new string[] { 
                @"C:\fpsfiles\test1.fps",
                @"C:\fpsfiles\test2.fps",
                @"C:\fpsfiles\test3.fps",
                @"C:\fpsfiles\test4.fps" };
            var filesToProcess = new int[] { 100, 200, 0, 0 };

            _fromV2FpsTable.Clear();
            _fromV3ToV5FpsTable.Clear();

            // Fill the dictionary with appropriate count information
            for (int i = 0; i < fileNames.Length; i++)
            {
                _fromV2FpsTable.Add(fileNames[i], i + 1);
                _fromV3ToV5FpsTable.Add(fileNames[i], new Tuple<int, int>(i + 1, filesToProcess[i]));
            }
        }

        #endregion Setup And Teardown

        #region Test Methods

        /// <summary>
        /// Tests whether the db manager will create a new database.
        /// </summary>
        [Test, Category("Automated")]
        public static void CreatesNewDatabase()
        {
            string tempDbFile = null;
            try
            {
                tempDbFile = FileSystemMethods.GetTemporaryFileName(".sdf");
                if (File.Exists(tempDbFile))
                {
                    File.Delete(tempDbFile);
                }
                var manager = new FAMServiceDatabaseManager(tempDbFile);
                bool created = manager.CreateDatabase(false);

                var info = new FileInfo(tempDbFile);
                Assert.That(created && info.Exists && info.Length > 0);
            }
            finally
            {
                if (tempDbFile != null && File.Exists(tempDbFile))
                {
                    File.Delete(tempDbFile);
                }
            }
        }

        /// <summary>
        /// Tests whether the db manager will create a new database and will not create
        /// a backup file.
        /// </summary>
        [Test, Category("Automated")]
        public static void DoesNotCreateDatabaseIfBackupFalseAndFileExists()
        {
            string tempDbFile = null;
            string backUpFileName = null;
            try
            {
                tempDbFile = FileSystemMethods.GetTemporaryFileName(".sdf");
                var manager = new FAMServiceDatabaseManager(tempDbFile);
                bool created = manager.CreateDatabase(false, out backUpFileName);

                Assert.That(!created && backUpFileName == null);
            }
            finally
            {
                if (tempDbFile != null && File.Exists(tempDbFile))
                {
                    File.Delete(tempDbFile);
                }
                if (backUpFileName != null && File.Exists(backUpFileName))
                {
                    File.Delete(backUpFileName);
                }
            }
        }

        /// <summary>
        /// Tests whether the db manager will create a new database and create
        /// a backup file.
        /// </summary>
        [Test, Category("Automated")]
        public static void CreatesNewDatabaseWithBackup()
        {
            string tempDbFile = null;
            string backUpFileName = null;
            try
            {
                tempDbFile = FileSystemMethods.GetTemporaryFileName(".sdf");
                File.WriteAllText(tempDbFile, "This is a test string.");
                var tempInfo = new FileInfo(tempDbFile);
                var tempSize = tempInfo.Length;
                var manager = new FAMServiceDatabaseManager(tempDbFile);
                bool created = manager.CreateDatabase(true, out backUpFileName);
                tempInfo = new FileInfo(backUpFileName);

                Assert.That(created && tempInfo.Exists && tempInfo.Length == tempSize);
            }
            finally
            {
                if (tempDbFile != null && File.Exists(tempDbFile))
                {
                    File.Delete(tempDbFile);
                }
                if (backUpFileName != null && File.Exists(backUpFileName))
                {
                    File.Delete(backUpFileName);
                }
            }
        }

        /// <summary>
        /// Tests whether opening a V2 service database correctly indicates that it
        /// needs to be updated.
        /// </summary>
        [Test, Category("Automated")]
        public static void CorrectlyIndicatesUpdateRequiredV2()
        {
            using (var tempV2 = new TemporaryFile(".sdf", false))
            {
                CreateV2Database(tempV2.FileName);
                var manager = new FAMServiceDatabaseManager(tempV2.FileName);
                Assert.That(manager.IsUpdateRequired);
            }
        }

        #region Schema Version 2 Tests

        /// <summary>
        /// Test whether a V2 service database is correctly updated to the current schema.
        /// </summary>
        [Test, Category("Automated")]
        public static void UpdateFromV2ToCurrent()
        {
            TemporaryFile tempV2 = null, tempBackup = null;
            try
            {
                tempV2 = new TemporaryFile(".sdf", false);
                CreateV2Database(tempV2.FileName);
                var manager = new FAMServiceDatabaseManager(tempV2.FileName);
                var task = manager.BeginUpdateToLatestSchema(null, new CancellationTokenSource());
                var backupFile = new FileInfo(task.Result);
                tempBackup = new TemporaryFile(backupFile, false);

                // Check that the database has been upgraded correctly
                var fpsFileTable = new List<FpsFileTableData>(manager.GetFpsFileData(true));
                Assert.That(fpsFileTable.Count == _fromV2FpsTable.Count);
                foreach (var data in fpsFileTable)
                {
                    Assert.That(_fromV2FpsTable[data.FileName] == data.NumberOfInstances);
                }
            }
            finally
            {
                if (tempV2 != null)
                {
                    tempV2.Dispose();
                }
                if (tempBackup != null)
                {
                    tempBackup.Dispose();
                }
            }
        }

        /// <summary>
        /// Checks whether the V2 database has been updated to the correct schema after an
        /// update is performed.
        /// </summary>
        [Test, Category("Automated")]
        public static void UpdateFromV2ToCurrentSchemaCorrect()
        {
            TemporaryFile tempV2 = null, tempBackup = null;
            try
            {
                tempV2 = new TemporaryFile(".sdf", false);
                CreateV2Database(tempV2.FileName);
                var manager = new FAMServiceDatabaseManager(tempV2.FileName);
                var task = manager.BeginUpdateToLatestSchema(null, new CancellationTokenSource());
                var backupFile = new FileInfo(task.Result);
                tempBackup = new TemporaryFile(backupFile, false);

                int schemaVersion = new FAMServiceDatabaseManager(tempV2.FileName).GetSchemaVersion();

                Assert.That(schemaVersion == FAMServiceDatabaseManager.CurrentSchemaVersion);
            }
            finally
            {
                if (tempV2 != null)
                {
                    tempV2.Dispose();
                }
                if (tempBackup != null)
                {
                    tempBackup.Dispose();
                }
            }
        }

        /// <summary>
        /// Test whether a V2 service database is correctly updated to the current schema
        /// when using the <see cref="IDatabaseSchemaUpdater"/> interface.
        /// </summary>
        [Test, Category("Automated")]
        public static void UpdateFromV2ToCurrentIDatabaseSchemaUpdater()
        {
            TemporaryFile tempV2 = null, tempBackup = null;
            try
            {
                tempV2 = new TemporaryFile(".sdf", false);
                CreateV2Database(tempV2.FileName);
                var manager = new FAMServiceDatabaseManager();
                IDatabaseSchemaUpdater updater = (IDatabaseSchemaUpdater)manager;
                using (var connection = new SqlCeConnection(
                    SqlCompactMethods.BuildDBConnectionString(tempV2.FileName, false)))
                {
                    updater.SetDatabaseConnection(connection);
                    var task =
                        updater.BeginUpdateToLatestSchema(null,
                        new CancellationTokenSource());

                    var backupFile = new FileInfo(task.Result);
                    tempBackup = new TemporaryFile(backupFile, false);
                }
                manager = new FAMServiceDatabaseManager(tempV2.FileName);

                // Check that the database has been upgraded correctly
                var fpsFileTable = new List<FpsFileTableData>(manager.GetFpsFileData(true));
                Assert.That(fpsFileTable.Count == _fromV2FpsTable.Count);
                foreach (var data in fpsFileTable)
                {
                    Assert.That(_fromV2FpsTable[data.FileName] == data.NumberOfInstances);
                }
            }
            finally
            {
                if (tempV2 != null)
                {
                    tempV2.Dispose();
                }
                if (tempBackup != null)
                {
                    tempBackup.Dispose();
                }
            }
        }

        #endregion Schema Version 2 tests

        #region Schema Version 3 tests

        /// <summary>
        /// Tests whether opening a V3 service database correctly indicates that it
        /// needs to be updated.
        /// </summary>
        [Test, Category("Automated")]
        public static void CorrectlyIndicatesUpdateRequiredV3()
        {
            using (var tempV3 = new TemporaryFile(".sdf", false))
            {
                CreateV3Database(tempV3.FileName);
                var manager = new FAMServiceDatabaseManager(tempV3.FileName);
                Assert.That(manager.IsUpdateRequired);
            }
        }

        /// <summary>
        /// Test whether a V3 service database is correctly updated to the current schema.
        /// </summary>
        [Test, Category("Automated")]
        public static void UpdateFromV3ToCurrent()
        {
            TemporaryFile tempV3 = null, tempBackup = null;
            try
            {
                tempV3 = new TemporaryFile(".sdf", false);
                CreateV3Database(tempV3.FileName);
                var manager = new FAMServiceDatabaseManager(tempV3.FileName);
                var task = manager.BeginUpdateToLatestSchema(null, new CancellationTokenSource());
                var backupFile = new FileInfo(task.Result);
                tempBackup = new TemporaryFile(backupFile, false);

                // Check that the database has been upgraded correctly
                var fpsFileTable = new List<FpsFileTableData>(manager.GetFpsFileData(true));
                Assert.That(fpsFileTable.Count == _fromV3ToV5FpsTable.Count);
                foreach (var data in fpsFileTable)
                {
                    var expected = _fromV3ToV5FpsTable[data.FileName];
                    Assert.That(expected.Item1 == data.NumberOfInstances
                        && expected.Item2 == data.NumberOfFilesToProcess);
                }
            }
            finally
            {
                if (tempV3 != null)
                {
                    tempV3.Dispose();
                }
                if (tempBackup != null)
                {
                    tempBackup.Dispose();
                }
            }
        }

        /// <summary>
        /// Checks whether the V3 database has been updated to the correct schema after an
        /// update is performed.
        /// </summary>
        [Test, Category("Automated")]
        public static void UpdateFromV3ToCurrentSchemaCorrect()
        {
            TemporaryFile tempV3 = null, tempBackup = null;
            try
            {
                tempV3 = new TemporaryFile(".sdf", false);
                CreateV3Database(tempV3.FileName);
                var manager = new FAMServiceDatabaseManager(tempV3.FileName);
                var task = manager.BeginUpdateToLatestSchema(null, new CancellationTokenSource());
                var backupFile = new FileInfo(task.Result);
                tempBackup = new TemporaryFile(backupFile, false);

                int schemaVersion = new FAMServiceDatabaseManager(tempV3.FileName).GetSchemaVersion();

                Assert.That(schemaVersion == FAMServiceDatabaseManager.CurrentSchemaVersion);
            }
            finally
            {
                if (tempV3 != null)
                {
                    tempV3.Dispose();
                }
                if (tempBackup != null)
                {
                    tempBackup.Dispose();
                }
            }
        }

        /// <summary>
        /// Test whether a V3 service database is correctly updated to the current schema
        /// when using the <see cref="IDatabaseSchemaUpdater"/> interface.
        /// </summary>
        [Test, Category("Automated")]
        public static void UpdateFromV3ToCurrentIDatabaseSchemaUpdater()
        {
            TemporaryFile tempV3 = null, tempBackup = null;
            try
            {
                tempV3 = new TemporaryFile(".sdf", false);
                CreateV3Database(tempV3.FileName);
                var manager = new FAMServiceDatabaseManager();
                IDatabaseSchemaUpdater updater = (IDatabaseSchemaUpdater)manager;
                using (var connection = new SqlCeConnection(
                    SqlCompactMethods.BuildDBConnectionString(tempV3.FileName, false)))
                {
                    updater.SetDatabaseConnection(connection);
                    var task =
                        updater.BeginUpdateToLatestSchema(null,
                        new CancellationTokenSource());

                    var backupFile = new FileInfo(task.Result);
                    tempBackup = new TemporaryFile(backupFile, false);
                }
                manager = new FAMServiceDatabaseManager(tempV3.FileName);

                // Check that the database has been upgraded correctly
                var fpsFileTable = new List<FpsFileTableData>(manager.GetFpsFileData(true));
                Assert.That(fpsFileTable.Count == _fromV3ToV5FpsTable.Count);
                foreach (var data in fpsFileTable)
                {
                    var expected = _fromV3ToV5FpsTable[data.FileName];
                    Assert.That(expected.Item1 == data.NumberOfInstances
                        && expected.Item2 == data.NumberOfFilesToProcess);
                }
            }
            finally
            {
                if (tempV3 != null)
                {
                    tempV3.Dispose();
                }
                if (tempBackup != null)
                {
                    tempBackup.Dispose();
                }
            }
        }

        #endregion Schema Version 3 tests

        #region Schema Version 4 tests

        /// <summary>
        /// Tests whether opening a V4 service database correctly indicates that it
        /// needs to be updated.
        /// </summary>
        [Test, Category("Automated")]
        public static void CorrectlyIndicatesUpdateRequiredV4()
        {
            using (var tempV4 = new TemporaryFile(".sdf", false))
            {
                CreateV4Database(tempV4.FileName);
                var manager = new FAMServiceDatabaseManager(tempV4.FileName);
                Assert.That(manager.IsUpdateRequired);
            }
        }

        /// <summary>
        /// Test whether a V4 service database is correctly updated to the current schema.
        /// </summary>
        [Test, Category("Automated")]
        public static void UpdateFromV4ToCurrent()
        {
            TemporaryFile tempV4 = null, tempBackup = null;
            try
            {
                tempV4 = new TemporaryFile(".sdf", false);
                CreateV4Database(tempV4.FileName);
                var manager = new FAMServiceDatabaseManager(tempV4.FileName);
                var task = manager.BeginUpdateToLatestSchema(null, new CancellationTokenSource());
                var backupFile = new FileInfo(task.Result);
                tempBackup = new TemporaryFile(backupFile, false);

                // Check that the database has been upgraded correctly
                var fpsFileTable = new List<FpsFileTableData>(manager.GetFpsFileData(true));
                Assert.That(fpsFileTable.Count == _fromV3ToV5FpsTable.Count);
                foreach (var data in fpsFileTable)
                {
                    var expected = _fromV3ToV5FpsTable[data.FileName];
                    Assert.That(expected.Item1 == data.NumberOfInstances
                        && expected.Item2 == data.NumberOfFilesToProcess);
                }
            }
            finally
            {
                if (tempV4 != null)
                {
                    tempV4.Dispose();
                }
                if (tempBackup != null)
                {
                    tempBackup.Dispose();
                }
            }
        }

        /// <summary>
        /// Checks whether the V4 database has been updated to the correct schema after an
        /// update is performed.
        /// </summary>
        [Test, Category("Automated")]
        public static void UpdateFromV4ToCurrentSchemaCorrect()
        {
            TemporaryFile tempV4 = null, tempBackup = null;
            try
            {
                tempV4 = new TemporaryFile(".sdf", false);
                CreateV4Database(tempV4.FileName);
                var manager = new FAMServiceDatabaseManager(tempV4.FileName);
                var task = manager.BeginUpdateToLatestSchema(null, new CancellationTokenSource());
                var backupFile = new FileInfo(task.Result);
                tempBackup = new TemporaryFile(backupFile, false);
                int schemaVersion = new FAMServiceDatabaseManager(tempV4.FileName).GetSchemaVersion();

                Assert.That(schemaVersion == FAMServiceDatabaseManager.CurrentSchemaVersion);
            }
            finally
            {
                if (tempV4 != null)
                {
                    tempV4.Dispose();
                }
                if (tempBackup != null)
                {
                    tempBackup.Dispose();
                }
            }
        }

        /// <summary>
        /// Test whether a V4 service database is correctly updated to the current schema
        /// when using the <see cref="IDatabaseSchemaUpdater"/> interface.
        /// </summary>
        [Test, Category("Automated")]
        public static void UpdateFromV4ToCurrentIDatabaseSchemaUpdater()
        {
            TemporaryFile tempV4 = null, tempBackup = null;
            try
            {
                tempV4 = new TemporaryFile(".sdf", false);
                CreateV4Database(tempV4.FileName);
                var manager = new FAMServiceDatabaseManager();
                IDatabaseSchemaUpdater updater = (IDatabaseSchemaUpdater)manager;
                using (var connection = new SqlCeConnection(
                    SqlCompactMethods.BuildDBConnectionString(tempV4.FileName, false)))
                {
                    updater.SetDatabaseConnection(connection);
                    var task =
                        updater.BeginUpdateToLatestSchema(null,
                        new CancellationTokenSource());

                    var backupFile = new FileInfo(task.Result);
                    tempBackup = new TemporaryFile(backupFile, false);
                }
                manager = new FAMServiceDatabaseManager(tempV4.FileName);

                // Check that the database has been upgraded correctly
                var fpsFileTable = new List<FpsFileTableData>(manager.GetFpsFileData(true));
                Assert.That(fpsFileTable.Count == _fromV3ToV5FpsTable.Count);
                foreach (var data in fpsFileTable)
                {
                    var expected = _fromV3ToV5FpsTable[data.FileName];
                    Assert.That(expected.Item1 == data.NumberOfInstances
                        && expected.Item2 == data.NumberOfFilesToProcess);
                }
            }
            finally
            {
                if (tempV4 != null)
                {
                    tempV4.Dispose();
                }
                if (tempBackup != null)
                {
                    tempBackup.Dispose();
                }
            }
        }

        #endregion Schema Version 4 tests

        /// <summary>
        /// Tests whether opening a V5 service database correctly indicates that it
        /// needs to be updated.
        /// </summary>
        [Test, Category("Automated")]
        public static void CorrectlyIndicatesUpdateRequiredV5()
        {
            using (var tempV5 = new TemporaryFile(".sdf", false))
            {
                CreateV5Database(tempV5.FileName);
                var manager = new FAMServiceDatabaseManager(tempV5.FileName);
                Assert.That(manager.IsUpdateRequired);
            }
        }

        /// <summary>
        /// Test whether a V5 service database is correctly updated to the current schema.
        /// </summary>
        [Test, Category("Automated")]
        public static void UpdateFromV5ToCurrent()
        {
            TemporaryFile tempV5 = null, tempBackup = null;
            try
            {
                tempV5 = new TemporaryFile(".sdf", false);
                CreateV5Database(tempV5.FileName);
                var manager = new FAMServiceDatabaseManager(tempV5.FileName);
                var task = manager.BeginUpdateToLatestSchema(null, new CancellationTokenSource());
                var backupFile = new FileInfo(task.Result);
                tempBackup = new TemporaryFile(backupFile, false);

                // Check that the database has been upgraded correctly
                var fpsFileTable = new List<FpsFileTableData>(manager.GetFpsFileData(true));
                Assert.That(fpsFileTable.Count == _fromV2FpsTable.Count);
                foreach (var data in fpsFileTable)
                {
                    var expected = _fromV3ToV5FpsTable[data.FileName];
                    Assert.That(expected.Item1 == data.NumberOfInstances
                        && expected.Item2 == data.NumberOfFilesToProcess);
                }
            }
            finally
            {
                if (tempV5 != null)
                {
                    tempV5.Dispose();
                }
                if (tempBackup != null)
                {
                    tempBackup.Dispose();
                }
            }
        }

        /// <summary>
        /// Checks whether the V5 database has been updated to the correct schema after an
        /// update is performed.
        /// </summary>
        [Test, Category("Automated")]
        public static void UpdateFromV5ToCurrentSchemaCorrect()
        {
            TemporaryFile tempV5 = null, tempBackup = null;
            try
            {
                tempV5 = new TemporaryFile(".sdf", false);
                CreateV5Database(tempV5.FileName);
                var manager = new FAMServiceDatabaseManager(tempV5.FileName);
                var task = manager.BeginUpdateToLatestSchema(null, new CancellationTokenSource());
                var backupFile = new FileInfo(task.Result);
                tempBackup = new TemporaryFile(backupFile, false);
                int schemaVersion = new FAMServiceDatabaseManager(tempV5.FileName).GetSchemaVersion();

                Assert.That(schemaVersion == FAMServiceDatabaseManager.CurrentSchemaVersion);
            }
            finally
            {
                if (tempV5 != null)
                {
                    tempV5.Dispose();
                }
                if (tempBackup != null)
                {
                    tempBackup.Dispose();
                }
            }
        }

        /// <summary>
        /// Test whether a V5 service database is correctly updated to the current schema
        /// when using the <see cref="IDatabaseSchemaUpdater"/> interface.
        /// </summary>
        [Test, Category("Automated")]
        public static void UpdateFromV5ToCurrentIDatabaseSchemaUpdater()
        {
            TemporaryFile tempV5 = null, tempBackup = null;
            try
            {
                tempV5 = new TemporaryFile(".sdf", false);
                CreateV5Database(tempV5.FileName);
                var manager = new FAMServiceDatabaseManager();
                IDatabaseSchemaUpdater updater = (IDatabaseSchemaUpdater)manager;
                using (var connection = new SqlCeConnection(
                    SqlCompactMethods.BuildDBConnectionString(tempV5.FileName, false)))
                {
                    updater.SetDatabaseConnection(connection);
                    var task =
                        updater.BeginUpdateToLatestSchema(null,
                        new CancellationTokenSource());

                    var backupFile = new FileInfo(task.Result);
                    tempBackup = new TemporaryFile(backupFile, false);
                }
                manager = new FAMServiceDatabaseManager(tempV5.FileName);

                // Check that the database has been upgraded correctly
                var fpsFileTable = new List<FpsFileTableData>(manager.GetFpsFileData(true));
                Assert.That(fpsFileTable.Count == _fromV3ToV5FpsTable.Count);
                foreach (var data in fpsFileTable)
                {
                    var expected = _fromV3ToV5FpsTable[data.FileName];
                    Assert.That(expected.Item1 == data.NumberOfInstances
                        && expected.Item2 == data.NumberOfFilesToProcess);
                }
            }
            finally
            {
                if (tempV5 != null)
                {
                    tempV5.Dispose();
                }
                if (tempBackup != null)
                {
                    tempBackup.Dispose();
                }
            }
        }

        #region Schema Version 5 tests


        #endregion Schema Version 5 tests

        #region Schema Version 6 tests

        /// <summary>
        /// Tests whether opening a version 6 database correctly indicates no
        /// update needed
        /// </summary>
        [Test, Category("Automated")]
        public static void CorrectlyIndicatesNoUpdateRequiredCurrentVersion()
        {
            using (var tempV6 = new TemporaryFile(".sdf", false))
            {
                CreateV6Database(tempV6.FileName);
                var manager = new FAMServiceDatabaseManager(tempV6.FileName);
                Assert.That(!manager.IsUpdateRequired);
            }
        }

        #endregion Schema Version 6 tests

        #endregion Test Methods

        #region Helper Methods

        /// <summary>
        /// Builds the default settings list for the specified version of the service database.
        /// </summary>
        /// <param name="version">The schema version settings to generate.</param>
        /// <returns>A settings list for the specified db schema version.</returns>
        static Settings[] BuildSettingList(int version)
        {
            var settings = new Settings[] {
                new Settings() {
                    Name = FAMServiceDatabaseManager.SleepTimeOnStartupKey,
                    Value = FAMServiceDatabaseManager.DefaultSleepTimeOnStartup.ToString(CultureInfo.InvariantCulture) },
                new Settings() {
                    Name = FAMServiceDatabaseManager.DependentServicesKey,
                    Value = "" },
                new Settings() {
                    Name = FAMServiceDatabaseManager.NumberOfFilesToProcessGlobalKey,
                    Value = FAMServiceDatabaseManager.DefaultNumberOfFilesToProcess.ToString(CultureInfo.InvariantCulture) },
                new Settings() {
                    Name = FAMServiceDatabaseManager.ServiceDBSchemaVersionKey,
                    Value = version.ToString(CultureInfo.InvariantCulture) }
            };

            return settings;
        }

        /// <summary>
        /// Creates a V2 schema service database.
        /// </summary>
        /// <param name="fileName">The file to write the database to.</param>
        static void CreateV2Database(string fileName)
        {
            var fpsFiles = new List<FpsFileTableV2>();
            foreach (var data in _fpsFileNames)
            {
                fpsFiles.Add(new FpsFileTableV2()
                {
                    FileName = data.Item1,
                    AutoStart = data.Item2
                });
            }

            using (var v2Db = new FAMServiceDatabaseV2(fileName))
            {
                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }

                v2Db.CreateDatabase();
                v2Db.Settings.InsertAllOnSubmit(BuildSettingList(2));
                v2Db.FpsFile.InsertAllOnSubmit(fpsFiles);
                v2Db.SubmitChanges(ConflictMode.FailOnFirstConflict);
            }
        }

        /// <summary>
        /// Creates a V3 schema service database.
        /// </summary>
        /// <param name="fileName">The file to write the database to.</param>
        static void CreateV3Database(string fileName)
        {
            var fpsFiles = new List<FpsFileTableV3>();
            foreach (var data in _fpsFileNames)
            {
                fpsFiles.Add(new FpsFileTableV3()
                {
                    FileName = data.Item1,
                    AutoStart = data.Item2,
                    NumberOfFilesToProcess = data.Item3
                });
            }

            using (var v3Db = new FAMServiceDatabaseV3(fileName))
            {
                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }

                v3Db.CreateDatabase();
                v3Db.Settings.InsertAllOnSubmit(BuildSettingList(3));
                v3Db.FpsFile.InsertAllOnSubmit(fpsFiles);
                v3Db.SubmitChanges(ConflictMode.FailOnFirstConflict);
            }
        }

        /// <summary>
        /// Creates a V4 schema service database.
        /// </summary>
        /// <param name="fileName">The file to write the database to.</param>
        static void CreateV4Database(string fileName)
        {
            var fpsFiles = new List<FpsFileTableV4>();
            foreach (var data in _fpsFileNames)
            {
                fpsFiles.Add(new FpsFileTableV4()
                {
                    FileName = data.Item1,
                    AutoStart = data.Item2,
                    NumberOfFilesToProcess = data.Item3
                });
            }

            using (var v4Db = new FAMServiceDatabaseV4(fileName))
            {
                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }

                v4Db.CreateDatabase();
                v4Db.Settings.InsertAllOnSubmit(BuildSettingList(4));
                v4Db.FpsFile.InsertAllOnSubmit(fpsFiles);
                v4Db.SubmitChanges(ConflictMode.FailOnFirstConflict);
            }
        }

        /// <summary>
        /// Creates a V5 schema service database.
        /// </summary>
        /// <param name="fileName">The file to write the database to.</param>
        static void CreateV5Database(string fileName)
        {
            var fpsFiles = new List<FpsFileTableV5>();
            foreach (var data in _fromV3ToV5FpsTable)
            {
                fpsFiles.Add(new FpsFileTableV5()
                {
                    FileName = data.Key,
                    NumberOfInstances = data.Value.Item1,
                    NumberOfFilesToProcess = data.Value.Item2.ToString(CultureInfo.InvariantCulture)
                });
            }

            using (var v5db = new FAMServiceDatabaseV5(fileName))
            {
                // Create the fam database
                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }

                v5db.CreateDatabase();
                v5db.Settings.InsertAllOnSubmit(BuildSettingList(5));
                v5db.FpsFile.InsertAllOnSubmit(fpsFiles);
                v5db.SubmitChanges(ConflictMode.FailOnFirstConflict);
            }
        }

        /// <summary>
        /// Creates a V6 schema service database.
        /// </summary>
        /// <param name="fileName">The file to write the database to.</param>
        static void CreateV6Database(string fileName)
        {
            var fpsFiles = new List<FpsFileTableV6>();
            foreach (var data in _fromV3ToV5FpsTable)
            {
                fpsFiles.Add(new FpsFileTableV6()
                {
                    FileName = data.Key,
                    NumberOfInstances = data.Value.Item1,
                    NumberOfFilesToProcess = data.Value.Item2
                });
            }

            using (var v6db = new FAMServiceDatabaseV6(fileName))
            {
                // Create the fam database
                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }

                v6db.CreateDatabase();
                v6db.Settings.InsertAllOnSubmit(BuildSettingList(6));
                v6db.FpsFile.InsertAllOnSubmit(fpsFiles);
                v6db.SubmitChanges(ConflictMode.FailOnFirstConflict);
            }
        }

        #endregion Helper Methods
    }
}
