using Extract.Database;
using LinqToDB;
using LinqToDB.Data;

namespace Extract.LabResultsCustomComponents
{
    /// A database with only a Settings table in it
    /// Replace this with a T4 Template if/when a complete mapping from a typical OMDB is needed
    public class DatabaseWithSettingsTable : DataConnection
    {
        /// Connect to a database
        public DatabaseWithSettingsTable(string databasePath)
            : base(SqliteMethods.BuildConnectionOptions(databasePath))
        {
        }

        /// Return the Settings table
        public ITable<Settings> Settings => GetTable<Settings>();
    }
}
