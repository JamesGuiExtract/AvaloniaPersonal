using Extract.Database;
using Extract.Database.Sqlite;
using LinqToDB;
using LinqToDB.Data;

namespace Extract.LabResultsCustomComponents
{
    /// <summary>
    /// A database with only a Settings table in it
    /// Replace this with a T4 Template if/when a complete mapping from a typical OMDB is needed
    /// </summary>
    public class DatabaseWithSettingsTable : DataConnection
    {
        /// <summary>
        /// Connect to a database
        /// </summary>
        public DatabaseWithSettingsTable(string databasePath)
            : base(SqliteMethods.BuildConnectionOptions(databasePath))
        {
        }

        /// <summary>
        /// Return the Settings table
        /// </summary>
        public ITable<Settings> Settings => this.GetTable<Settings>();
    }
}
