﻿using LinqToDB.Configuration;
using System;
using System.Data.SQLite;

namespace Extract.Database
{
    public static class SqliteMethods
    {

        /// <summary>
        /// Escape leading backslashes if needed (For UNC paths. Changes \\ to \\\\)
        /// </summary>
        /// <param name="databasePath">The path to a sqlite database file</param>
        public static string FixDatabasePath(string databasePath)
        {
            try
            {
                _ = databasePath ?? throw new ArgumentNullException(nameof(databasePath));

                databasePath = databasePath.Trim();

                // Fix UNC paths so that they resolve correctly
                // https://extract.atlassian.net/browse/ISSUE-17694
                // http://system.data.sqlite.org/index.html/info/bbdda6eae2
                if (databasePath.StartsWith(@"\\", StringComparison.Ordinal)
                    && !databasePath.StartsWith(@"\\\\", StringComparison.Ordinal))
                {
                    databasePath = @"\\" + databasePath;
                }

                return databasePath;
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
            try
            {
                return $"Data Source={FixDatabasePath(databasePath)};Version=3;";
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI51933");
            }
        }

        /// <summary>
        /// Build connection options from a file path
        /// </summary>
        /// <param name="databasePath">The path to a sqlite database file</param>
        public static LinqToDbConnectionOptions BuildConnectionOptions(string databasePath)
        {
            try
            {
                return new LinqToDbConnectionOptionsBuilder()
                    .UseSQLiteOfficial(BuildConnectionString(databasePath))
                    .Build();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI51934");
            }
        }

        /// <summary>
        /// Escape leading backslashes in the DataSource if needed (For UNC paths. Changes \\ to \\\\)
        /// </summary>
        /// <param name="connectionString">The connection string to fix</param>
        public static string FixConnectionString(string connectionString)
        {
            try
            {
                _ = connectionString ?? throw new ArgumentNullException(nameof(connectionString));

                SQLiteConnectionStringBuilder builder = new(connectionString);
                builder.DataSource = FixDatabasePath(builder.DataSource);

                return builder.ConnectionString;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI51935");
            }
        }
    }
}
