using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Extract.Database.Sqlite
{
    /// <summary>
    /// Like <see cref="IDatabaseSchemaManager"/> but for SQLite instead of SQLCompact databases
    /// </summary>
    public interface ISqliteDatabaseManager
    {
        /// <summary>
        /// Set the path to the database
        /// </summary>
        void SetDatabase(string databasePath);

        /// <summary>
        /// Gets the schema version from the database
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public int GetSchemaVersion();

        /// <summary>
        /// Gets the settings from the database
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        Dictionary<string, string> GetSettings();

        /// <summary>
        /// Whether a database schema update is required
        /// </summary>
        bool IsUpdateRequired { get; }

        /// <summary>
        /// Whether the database schema is newer than the current software supports
        /// </summary>
        bool IsNewerVersion { get; }

        /// <summary>
        /// The SQLCDBEditorPlugin implementation(s) that should completely replace the normal SQLCDBEditor UI
        /// </summary>
        IEnumerable<object> UIReplacementPlugins { get; }

        /// <summary>
        /// The assembly paths containing SQLCDBEditorPlugin implementation(s) that should be added to the normal SQLCDBEditor UI
        /// </summary>
        IEnumerable<string> UISupplementPluginAssemblies { get; }

        /// <summary>
        /// Creates a database. If the database file already exists then it will be copied to a backup file before a new database is created.
        /// <returns>The name of the file that was backed up or null if no backup was created</returns>
        string CreateDatabase();

        /// <summary>
        /// Computes the time-stamped file name for the backup file and copies the current database file to that location.
        /// </summary>
        /// <returns>The path to the backup file.</returns>
        public string BackupDatabase();

        /// <summary>
        /// Updates to latest schema
        /// </summary>
        /// <returns>
        /// A Task that is updating the schema. This result should contain the path to the backed up copy
        /// of the database before it was updated to the latest schema.
        /// </returns>
        Task<string> UpdateToLatestSchema();
    }
}