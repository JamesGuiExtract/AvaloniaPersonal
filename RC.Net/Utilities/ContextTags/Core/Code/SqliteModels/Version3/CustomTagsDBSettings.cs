using Extract.Database;
using System.Collections.Generic;
using System.Globalization;

namespace Extract.Utilities.ContextTags.SqliteModels.Version3
{
    /// Settings for the current schema. These should be versioned (copied to a new class when they change)
    /// to facilitate unit testing
    public static class CustomTagsDBSettings
    {
        /// The class that manages this schema and can perform upgrades to the latest schema.
        public static string DBSchemaManager => typeof(ContextTagsSqliteDatabaseManager).ToString();

        /// The setting key for the current context tags database schema
        public static string ContextTagsDBSchemaVersionKey => "ContextTagsDBSchemaVersion";

        /// The Settings data used for a new database
        public static IList<Settings> DefaultSettings => new []
        {
            new Settings
            {
                Name = ContextTagsDBSchemaVersionKey,
                Value = CustomTagsDB.SchemaVersion.ToString(CultureInfo.InvariantCulture)
            },
            new Settings
            {
                Name = DatabaseHelperMethods.DatabaseSchemaManagerKey,
                Value = DBSchemaManager
            }
        };
    }
}
