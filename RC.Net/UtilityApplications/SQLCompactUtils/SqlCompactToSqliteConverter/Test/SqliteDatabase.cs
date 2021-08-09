using Extract.Database;
using LinqToDB;
using LinqToDB.Configuration;
using LinqToDB.Data;

namespace Extract.Utilities.SqlCompactToSqliteConverter.Test
{
    /// A database with only a Settings table in it
    public class SqliteDatabaseWithSettingsTable : DataConnection
    {
        /// Connect to a database
        public SqliteDatabaseWithSettingsTable(string fileName)
            : base(
                  new LinqToDbConnectionOptionsBuilder()
                  .UseSQLiteOfficial($"Data Source={fileName};Version=3;")
                  .Build())
        {
        }

        /// Return the Settings table
        public ITable<Settings> Settings => GetTable<Settings>();
    }
}
