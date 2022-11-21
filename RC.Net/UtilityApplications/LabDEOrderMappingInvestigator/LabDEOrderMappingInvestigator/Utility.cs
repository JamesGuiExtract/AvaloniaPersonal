using LinqToDB.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;
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
        public static LinqToDBConnectionOptions BuildConnectionOptions(string databasePath)
        {
            return new LinqToDBConnectionOptionsBuilder()
                .UseSQLiteOfficial(BuildConnectionString(databasePath))
                .Build();
        }
    }

    public class StringLogicalComparer : Comparer<string>
    {
        [DllImport("shlwapi.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        private static extern int StrCmpLogicalW(string? x, string? y);

        public override int Compare(string? x, string? y)
        {
            return StrCmpLogicalW(x, y);
        }
    }

    public static class PathUtils
    {
        public static bool IsFullyQualifiedExistingFolder(this string? path)
        {
            return path is not null && Path.IsPathFullyQualified(path) && Directory.Exists(path);
        }

        public static bool IsFullyQualifiedExistingFile(this string? path)
        {
            return path is not null && Path.IsPathFullyQualified(path) && File.Exists(path);
        }
    }
}
