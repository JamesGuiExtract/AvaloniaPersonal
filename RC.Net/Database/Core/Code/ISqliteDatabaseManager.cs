using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Extract.Database
{
    /// Like <see cref="IDatabaseSchemaManager"/> but for SQLite instead of SQLCompact databases
    public interface ISqliteDatabaseManager
    {
        /// Set the path to the database
        void SetDatabase(string databasePath);

        /// Gets the schema version from the database
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public int GetSchemaVersion();

        /// Whether a database schema update is required
        bool IsUpdateRequired { get; }

        /// Whether the database schema is newer than the current software supports
        bool IsNewerVersion { get; }

        /// The SQLCDBEditorPlugin implementation(s) that should completely replace the normal SQLCDBEditor UI
        IEnumerable<object> UIReplacementPlugins { get; }

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