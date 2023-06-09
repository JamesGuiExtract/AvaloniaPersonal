﻿using Extract.Database.Sqlite;
using Extract.FileActionManager.Database.SqliteModels.Version8;
using Extract.Utilities;
using LinqToDB;
using LinqToDB.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Extract.FileActionManager.Database
{
    /// <summary>
    /// Schema manager for FAM Service configuration databases
    /// </summary>
    public class FAMServiceSqliteDatabaseManager : ISqliteDatabaseManager
    {
        /// <summary>
        /// The current service database schema version.
        /// </summary>
        public static readonly int CurrentSchemaVersion = FAMServiceDB.SchemaVersion;

        static readonly char[] _TRIM_QUOTES = new char[] { '"' };

        private string _databasePath;

        /// <summary>
        /// Create a new instance that needs to be configured with <see cref="SetDatabase"/>
        /// </summary>
        public FAMServiceSqliteDatabaseManager()
            : this("")
        { }

        /// <summary>
        /// Create a new instance and set the database file
        /// </summary>
        /// <param name="databasePath">The database file to manage</param>
        public FAMServiceSqliteDatabaseManager(string databasePath)
        {
            _databasePath = databasePath;
        }

        /// <inheritdoc/>
        public bool IsUpdateRequired => GetSchemaVersion() < CurrentSchemaVersion;

        /// <inheritdoc/>
        public bool IsNewerVersion => GetSchemaVersion() > CurrentSchemaVersion;

        /// <inheritdoc/>
        public IEnumerable<object> UIReplacementPlugins { get; }

        /// <inheritdoc/>
        public IEnumerable<string> UISupplementPluginAssemblies { get; }

        /// <inheritdoc/>
        public async Task<string> UpdateToLatestSchema()
        {
            try
            {
                int version = GetSchemaVersion();

                ExtractException.Assert("ELI51803",
                    "Database version must be less than the current schema version",
                    version < CurrentSchemaVersion);

                return await Task.Run(() =>
                {
                    string backUpFileName = BackupDatabase();

                    switch (version)
                    {
                        // Insert code to update schema here
                        default:
                            throw new ExtractException("ELI51808", $"Unsupported schema version: {version}");
                    }

#pragma warning disable CS0162 // Unreachable code detected
                    return backUpFileName;
#pragma warning restore CS0162 // Unreachable code detected

                }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI51802", ex);
            }
        }

        /// <inheritdoc/>
        public int GetSchemaVersion()
        {
            try
            {
                using FAMServiceDB db = new(SqliteMethods.BuildConnectionOptions(_databasePath));

                var schemaVersion = db.Settings.Find(FAMServiceDBSettings.ServiceDBSchemaVersionKey);

                if (schemaVersion is null)
                {
                    throw new ExtractException("ELI51800", "No schema version found in the database");
                }

                if (!int.TryParse(schemaVersion.Value, out int version))
                {
                    var uex = new ExtractException("ELI51801", "Invalid schema version number format");
                    uex.AddDebugData("Schema Version Number", schemaVersion.Value, false);
                    throw uex;
                }

                return version;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI51809");
            }
        }

        /// <inheritdoc/>
        public void SetDatabase(string databasePath)
        {
            _databasePath = databasePath;
        }

        /// <summary>
        /// Computes the time-stamped file name for the backup file and copies the current database file to that location.
        /// </summary>
        /// <returns>The path to the backup file.</returns>
        public string BackupDatabase()
        {
            try
            {
                string backupFile = FileSystemMethods.BuildTimeStampedBackupFileName(_databasePath, true);
                File.Copy(_databasePath, backupFile, true);
                return backupFile;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI51810");
            }
        }

        /// <summary>
        /// Gets the settings from the database
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public Dictionary<string, string> GetSettings()
        {
            try
            {
                using FAMServiceDB db = new(SqliteMethods.BuildConnectionOptions(_databasePath));
                return db.Settings.ToDictionary(s => s.Name, s => s.Value);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI51812");
            }
        }

        /// <summary>
        /// The class that manages this schema and can perform upgrades to the latest schema.
        /// </summary>
        public static string DBSchemaManager => FAMServiceDBSettings.DBSchemaManager;

        /// <summary>
        /// The setting key for the sleep time on startup
        /// </summary>
        public static string SleepTimeOnStartupKey => FAMServiceDBSettings.SleepTimeOnStartupKey;

        /// <summary>
        /// The settings key for the dependent services list.
        /// </summary>
        public static string DependentServicesKey => FAMServiceDBSettings.DependentServicesKey;

        /// <summary>
        /// The setting key for the default number of files to process for all fps files.
        /// </summary>
        public static string NumberOfFilesToProcessGlobalKey => FAMServiceDBSettings.NumberOfFilesToProcessGlobalKey;

        /// <summary>
        /// The setting key for the restart stopped file suppliers option
        /// </summary>
        public static string RestartStoppedFileSuppliersAfterDelayMsKey => FAMServiceDBSettings.RestartStoppedFileSuppliersAfterDelayMsKey;

        /// <summary>
        /// The default value for the restart stopped file suppliers option
        /// </summary>
        public static string RestartStoppedFileSuppliersAfterDelayMsValue => FAMServiceDBSettings.RestartStoppedFileSuppliersAfterDelayMsValue;

        /// <summary>
        /// Gets a collection containing the data from the FPSFile table in the service database.
        /// </summary>
        /// <param name="ignoreZeroRows">If <see langword="true"/> then any row whose
        /// number of instances value is 0 will be ignored.</param>
        /// <returns>The collection of rows in the FPS file table.</returns>
        public IList<FpsFileTableData> GetFpsFileData(bool ignoreZeroRows)
        {
            try
            {
                using FAMServiceDB db = new(SqliteMethods.BuildConnectionOptions(_databasePath));

                var fpsFiles = db.FPSFiles
                    .Select(f =>
                        new FpsFileTableData(
                            f.FileName.Trim(_TRIM_QUOTES),
                            f.NumberOfInstances,
                            f.NumberOfFilesToProcess))
                    .ToList();

                if (ignoreZeroRows)
                {
                    fpsFiles.RemoveAll(f => f.NumberOfInstances == 0);
                }

                return fpsFiles;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI51811");
            }
        }

        /// <summary>
        /// Creates a database. If the database file already exists then it will be copied to a backup file before a new database is created.
        /// <returns>The name of the file that was backed up or null if no backup was created</returns>
        public string CreateDatabase()
        {
            try
            {
                string backupFile = null;
                if (!File.Exists(_databasePath))
                {
                    // Ensure the directory exists
                    Directory.CreateDirectory(Path.GetDirectoryName(_databasePath));
                }
                else
                {
                    backupFile = BackupDatabase();
                    FileSystemMethods.DeleteFile(_databasePath);
                }

                // Create an empty database
                new FileStream(_databasePath, FileMode.CreateNew).Close();

                using FAMServiceDB db = new(SqliteMethods.BuildConnectionOptions(_databasePath));
                db.CreateDatabaseStructure();
                db.BulkCopy(FAMServiceDBSettings.DefaultSettings);

                return backupFile;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI51804");
            }
        }
    }
}
