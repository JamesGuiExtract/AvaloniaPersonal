using LinqToDB.Configuration;

namespace Extract.Database
{
    public static class SqliteMethods
    {
        /// <summary>
        /// Build connection options from a file path
        /// </summary>
        /// <param name="databasePath">The path to a sqlite database file</param>
        public static LinqToDbConnectionOptions BuildConnectionOptions(string databasePath)
        {
            return new LinqToDbConnectionOptionsBuilder()
                .UseSQLiteOfficial($"Data Source={databasePath};Version=3;")
                .Build();
        }
    }
}
