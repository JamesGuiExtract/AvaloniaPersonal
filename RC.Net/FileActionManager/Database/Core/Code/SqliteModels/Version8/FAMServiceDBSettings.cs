using Extract.Database;
using System.Collections.Generic;
using System.Globalization;

namespace Extract.FileActionManager.Database.SqliteModels.Version8
{
    /// Settings for the current schema. These should be versioned (copied to a new class when they change)
    /// to facilitate unit testing
    public static class FAMServiceDBSettings
    {
        /// The class that manages this schema and can perform upgrades to the latest schema.
        public static string DBSchemaManager => typeof(FAMServiceSqliteDatabaseManager).ToString();

        /// The setting key for the sleep time on startup
        public static string SleepTimeOnStartupKey => "SleepTimeOnStart";

        /// The setting key for the default number of files to process for all fps files.
        public static string NumberOfFilesToProcessGlobalKey => "NumberOfFilesToProcessPerFAMInstance";

        /// The settings key for the dependent services list.
        public static string DependentServicesKey => "DependentServices";

        /// The setting key for the current fam service database schema
        public static string ServiceDBSchemaVersionKey => "ServiceDBSchemaVersion";

        // The default sleep time the service should use when starting (default is 2 minutes)
        private const int DefaultSleepTimeOnStartup = 120000;

        // The default number of files to process before respawning the FAMProcess.
        // A value of 0 indicates that the process should keep processing until it is
        // stopped and will not be re-spawned. Negative values are not allowed.
        private const int DefaultNumberOfFilesToProcess = 1000;

        // The setting key for the DatabaseServer that will be used for database specific operations for the ESFAMService
        private const string DatabaseServerKey = "DatabaseServer";

        // The setting key for the DatabaseName that will be used for database specific operations for the ESFAMService
        private const string DatabaseNameKey = "DatabaseName";

        /// The Settings data used for a new database
        public static IList<Settings> DefaultSettings => new []
        {
            new Settings
            {
                Name = SleepTimeOnStartupKey,
                Value = DefaultSleepTimeOnStartup.ToString(CultureInfo.InvariantCulture)
            },
            new Settings
            {
                Name = DependentServicesKey,
                Value = ""
            },
            new Settings
            {
                Name = NumberOfFilesToProcessGlobalKey,
                Value = DefaultNumberOfFilesToProcess.ToString(CultureInfo.InvariantCulture)
            },
            new Settings
            {
                Name = ServiceDBSchemaVersionKey,
                Value = FAMServiceDB.SchemaVersion.ToString(CultureInfo.InvariantCulture)
            },
            new Settings
            {
                Name = DatabaseHelperMethods.DatabaseSchemaManagerKey,
                Value = DBSchemaManager
            },
            new Settings
            {
                Name = DatabaseServerKey,
                Value = ""
            },
            new Settings
            {
                Name = DatabaseNameKey,
                Value = ""
            }
        };
    }
}
