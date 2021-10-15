using Extract.Database;
using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Extract.LabResultsCustomComponents
{
    /// <inheritdoc/>
    public class OrderMapperSqliteDatabaseManager : ISqliteDatabaseManager
    {
        /// The current order mapping database schema version
        public static readonly int CurrentSchemaVersion = 3;

        /// The database schema version key
        public static readonly string OrderMapperSchemaVersionKey = "OrderMapperSchemaVersion";

        private string _databasePath;

        /// <summary>
        /// Create a new instance that needs to be configured with <see cref="SetDatabase"/>
        /// </summary>
        public OrderMapperSqliteDatabaseManager()
            : this("")
        { }

        /// <summary>
        /// Create a new instance and set the database file
        /// </summary>
        /// <param name="databasePath">The database file to manage</param>
        public OrderMapperSqliteDatabaseManager(string databasePath)
        {
            _databasePath = databasePath;
        }

        public bool IsUpdateRequired => GetSchemaVersion() < CurrentSchemaVersion;

        public bool IsNewerVersion => GetSchemaVersion() > CurrentSchemaVersion;

        public IEnumerable<object> UIReplacementPlugins => Enumerable.Empty<object>();

        public IEnumerable<string> UISupplementPluginAssemblies { get; } =
            new[] { Path.Combine(FileSystemMethods.CommonComponentsPath, "Extract.AlternateTestNameManagerPlugin.dll") };

        public async Task<string> UpdateToLatestSchema()
        {
            try
            {
                int version = GetSchemaVersion();

                ExtractException.Assert("ELI51920",
                    "Database version must be less than the current schema version",
                    version < CurrentSchemaVersion);

                return await Task.Run(() =>
                {
                    string backUpFileName = BackupDatabase();

                    switch (version)
                    {
                        // Insert code to update schema here
                        default:
                            throw new ExtractException("ELI51921", $"Unsupported schema version: {version}");
                    }

#pragma warning disable CS0162 // Unreachable code detected
                    return backUpFileName;
#pragma warning restore CS0162 // Unreachable code detected

                }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI51922", ex);
            }
        }

        public int GetSchemaVersion()
        {
            try
            {
                using DatabaseWithSettingsTable db = new(_databasePath);

                var schemaVersion = db.Settings.FirstOrDefault(setting => setting.Name == OrderMapperSchemaVersionKey);

                if (schemaVersion is null)
                {
                    throw new ExtractException("ELI51923", "No schema version found in the database");
                }

                if (!int.TryParse(schemaVersion.Value, out int version))
                {
                    var uex = new ExtractException("ELI51924", "Invalid schema version number format");
                    uex.AddDebugData("Schema Version Number", schemaVersion.Value, false);
                    throw uex;
                }

                return version;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI51925");
            }
        }

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
                throw ex.AsExtract("ELI51926");
            }
        }

        public Dictionary<string, string> GetSettings()
        {
            try
            {
                using DatabaseWithSettingsTable db = new(_databasePath);
                return db.Settings.ToDictionary(s => s.Name, s => s.Value);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI51927");
            }
        }

        public string CreateDatabase()
        {
            throw new NotImplementedException();
        }
    }
}