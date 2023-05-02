using Extract.Database.Sqlite;
using Extract.Utilities.ContextTags.SqliteModels.Version3;
using LinqToDB.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Extract.Utilities.ContextTags
{
    /// <inheritdoc/>
    public class ContextTagsSqliteDatabaseManager : ISqliteDatabaseManager
    {
        /// The current custom tags database schema version.
        public static readonly int CurrentSchemaVersion = CustomTagsDB.SchemaVersion;

        private string _databasePath;

        /// <summary>
        /// Create a new instance that needs to be configured with <see cref="SetDatabase"/>
        /// </summary>
        public ContextTagsSqliteDatabaseManager()
            : this("")
        { }

        /// <summary>
        /// Create a new instance and set the database file
        /// </summary>
        /// <param name="databasePath">The database file to manage</param>
        public ContextTagsSqliteDatabaseManager(string databasePath)
        {
            _databasePath = databasePath;
        }

        public bool IsUpdateRequired => GetSchemaVersion() < CurrentSchemaVersion;

        public bool IsNewerVersion => GetSchemaVersion() > CurrentSchemaVersion;

        public IEnumerable<object> UIReplacementPlugins => new object[] { new ContextTagsPlugin() };

        public IEnumerable<string> UISupplementPluginAssemblies { get; }

        public async Task<string> UpdateToLatestSchema()
        {
            try
            {
                int version = GetSchemaVersion();

                ExtractException.Assert("ELI51815",
                    "Database version must be less than the current schema version",
                    version < CurrentSchemaVersion);

                return await Task.Run(() =>
                {
                    string backUpFileName = BackupDatabase();

                    switch (version)
                    {
                        // Insert code to update schema here
                        default:
                            throw new ExtractException("ELI51816", $"Unsupported schema version: {version}");
                    }

#pragma warning disable CS0162 // Unreachable code detected
                    return backUpFileName;
#pragma warning restore CS0162 // Unreachable code detected

                }).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI51817", ex);
            }
        }

        public int GetSchemaVersion()
        {
            try
            {
                using CustomTagsDB db = new(SqliteMethods.BuildConnectionOptions(_databasePath));

                var schemaVersion = db.Settings.Find(CustomTagsDBSettings.ContextTagsDBSchemaVersionKey);

                if (schemaVersion is null)
                {
                    throw new ExtractException("ELI51818", "No schema version found in the database");
                }

                if (!int.TryParse(schemaVersion.Value, out int version))
                {
                    var uex = new ExtractException("ELI51819", "Invalid schema version number format");
                    uex.AddDebugData("Schema Version Number", schemaVersion.Value, false);
                    throw uex;
                }

                return version;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI51820");
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
                throw ex.AsExtract("ELI51821");
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public Dictionary<string, string> GetSettings()
        {
            try
            {
                using CustomTagsDB db = new(SqliteMethods.BuildConnectionOptions(_databasePath));
                return db.Settings.ToDictionary(s => s.Name, s => s.Value);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI51822");
            }
        }

        /// The class that manages this schema and can perform upgrades to the latest schema.
        public static string DBSchemaManager => CustomTagsDBSettings.DBSchemaManager;

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

                using CustomTagsDB db = new(SqliteMethods.BuildConnectionOptions(_databasePath));
                db.CreateDatabaseStructure();
                db.BulkCopy(CustomTagsDBSettings.DefaultSettings);

                return backupFile;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI51823");
            }
        }

        /// The context name associated with the database
        public string GetContextNameForDirectory(string directory)
        {
            using CustomTagsDB db = new(SqliteMethods.BuildConnectionOptions(_databasePath));
            return db.GetContextNameForDirectory(directory);
        }

        /// <summary>
        /// Compute the tag values for each workflow, using the default (empty workflow) value if the tag isn't defined for a non-empty workflow
        /// </summary>
        /// <param name="contextName">The context name to compute the tag values for</param>
        /// <returns>A dictionary mapping workflow name to dictionary mapping tag to value</returns>
        public Dictionary<string, Dictionary<string, string>> GetContextTagsByWorkflow(string contextName)
        {
            // NOTE: Due to bugs with SQLCDBEditor there can be empty custom tag names that should be ignored
            // (thus the tagValue.Tag.Name.Length != 0 filters)

            using CustomTagsDB db = new(SqliteMethods.BuildConnectionOptions(_databasePath));

            // Get the values for the empty workflow
            var defaultValues = db.TagValues
                .Where(tagValue =>
                    tagValue.Context.Name == contextName &&
                    tagValue.Tag.Name.Length != 0 &&
                    tagValue.Workflow.Length == 0)
                .Select(tagValue => new KeyValuePair<string, string>(tagValue.Tag.Name, tagValue.Value))
                .ToDictionary(kv => kv.Key, kv => kv.Value, StringComparer.OrdinalIgnoreCase);

            Dictionary<string, Dictionary<string, string>> workflowTagValues = new(StringComparer.OrdinalIgnoreCase)
            {
                { "", defaultValues }
            };

            // Get the values for the non-empty workflows
            List<string> workflows = db.TagValues
                .Where(tagValue =>
                    tagValue.Context.Name == contextName &&
                    tagValue.Tag.Name.Length != 0 &&
                    tagValue.Workflow.Length != 0)
                .Select(tagValue => tagValue.Workflow)
                .Distinct()
                .ToList();

            foreach (string workflow in workflows)
            {
                // Get the values that are for the current workflow
                var workflowValues = db.TagValues
                    .Where(tagValue =>
                        tagValue.Context.Name == contextName &&
                        tagValue.Tag.Name.Length != 0 &&
                        tagValue.Workflow == workflow)
                    .Select(tagValue => new KeyValuePair<string, string>(tagValue.Tag.Name, tagValue.Value))
                    .ToDictionary(kv => kv.Key, kv => kv.Value, StringComparer.OrdinalIgnoreCase);

                var definedTags = new HashSet<string>(workflowValues.Select(kv => kv.Key));

                // Combine with default values if they haven't been defined for this workflow
                workflowTagValues[workflow] = workflowValues
                    .Union(defaultValues.Where(kv => !definedTags.Contains(kv.Key)))
                    .ToDictionary(kv => kv.Key, kv => kv.Value, StringComparer.OrdinalIgnoreCase);
            }

            return workflowTagValues;
        }

        /// Whether there are no contexts and no custom tags defined
        public bool IsDatabaseEmpty()
        {
            using CustomTagsDB db = new(SqliteMethods.BuildConnectionOptions(_databasePath));
            return !db.Contexts.Any() && !db.CustomTags.Any();
        }
    }
}
