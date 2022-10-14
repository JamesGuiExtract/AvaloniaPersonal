using LinqToDB.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UCLID_COMUTILSLib;

namespace LabDEOrderMappingInvestigator
{
    public static class COMExtensionMethods
    {
        public static IEnumerable<T> ToIEnumerable<T>(this IIUnknownVector comVector)
        {
            _ = comVector ?? throw new ArgumentNullException(nameof(comVector));

            int size = comVector.Size();

            for (int i = 0; i < size; i++)
            {
                yield return (T)comVector.At(i);
            }
        }
    }

    public static class ExceptionMethods
    {
        [SuppressMessage("Usage", "CA2201:Do not raise reserved exception types", Justification = "<Pending>")]
        public static void Assert([DoesNotReturnIf(false)] this bool condition, string message, Action<Exception>? configureException = null)
        {
            if (condition)
            {
                return;
            }

            Exception ex = new(message ?? "Condition not met");

            if (configureException is not null)
            {
                configureException(ex);
            }

            throw ex;
        }
    }

    public static class SqliteUtils
    {
        /// <summary>
        /// Escape leading backslashes if needed (For UNC paths. Changes \\ to \\\\)
        /// </summary>
        /// <param name="databasePath">The path to a sqlite database file</param>
        public static string FixDatabasePath(string databasePath)
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

        /// <summary>
        /// Build connection string from a file path
        /// </summary>
        /// <param name="databasePath">The path to a sqlite database file</param>
        public static string BuildConnectionString(string databasePath)
        {
            return $"Data Source={FixDatabasePath(databasePath)};Version=3;";
        }

        /// <summary>
        /// Build connection options from a file path
        /// </summary>
        /// <param name="databasePath">The path to a sqlite database file</param>
        public static LinqToDbConnectionOptions BuildConnectionOptions(string databasePath)
        {
            return new LinqToDbConnectionOptionsBuilder()
                .UseSQLiteOfficial(BuildConnectionString(databasePath))
                .Build();
        }
    }

}
