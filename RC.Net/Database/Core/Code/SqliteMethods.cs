using LinqToDB.Configuration;
using System;

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
            try
            {
                _ = databasePath ?? throw new ArgumentNullException(nameof(databasePath));

                // Fix UNC paths so that they resolve correctly
                // https://extract.atlassian.net/browse/ISSUE-17694
                // http://system.data.sqlite.org/index.html/info/bbdda6eae2
                if (databasePath.StartsWith(@"\\", StringComparison.Ordinal)
                    && !databasePath.StartsWith(@"\\\\", StringComparison.Ordinal))
                {
                    databasePath = @"\\" + databasePath;
                }

                return new LinqToDbConnectionOptionsBuilder()
                    .UseSQLiteOfficial($"Data Source={databasePath};Version=3;")
                    .Build();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI51885");
            }
        }

        /// <summary>
        /// Build connection string from a file path
        /// </summary>
        /// <param name="databasePath">The path to a sqlite database file</param>
        public static string BuildConnectionString(string databasePath)
        {
            return BuildConnectionOptions(databasePath).ConfigurationString;
        }
    }
}
