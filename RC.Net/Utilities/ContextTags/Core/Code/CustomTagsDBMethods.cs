using LinqToDB.Data;
using System;
using System.Data.SQLite;
using System.IO;

namespace Extract.Utilities.ContextTags
{
    public static class CustomTagsDBMethods
    {
        // Wrap ref-param conversion function that so that it can be memoized
        private static readonly Func<string, string> _convertToNetworkPath = path =>
        {
            FileSystemMethods.ConvertToNetworkPath(ref path, false);
            if (path.EndsWith("\\", StringComparison.OrdinalIgnoreCase))
            {
                path = path.Substring(0, path.Length - 1);
            }
            return path;
        };

        // Memoize (cache) conversion results because FileSystemMethods.ConvertToNetworkPath is slow
        // and it also causes odd errors when it is called as part of plugin UI events for some reason.
        // (the old, ContextTagDatabase.DatabaseDirectory property did a similar thing, presumably for the same reasons)
        private static readonly Func<string, string> ConvertToNetworkPath =
            _convertToNetworkPath.Memoize(threadSafe: true);

        /// Convert a directory to a UNC path and cache the result for future calls
        public static string GetCanonicalFPSFileDir(string directory)
        {
            if (String.IsNullOrEmpty(directory))
            {
                return null;
            }

            return ConvertToNetworkPath(directory);
        }

        /// Get the directory from a DataConnection that is connected to a sqlite database
        public static string GetDatabaseDirectory(DataConnection sqliteDataConnection)
        {
            _ = sqliteDataConnection ?? throw new ArgumentNullException(nameof(sqliteDataConnection));

            string databasePath = ((SQLiteConnection)sqliteDataConnection.Connection).FileName;
            return Path.GetDirectoryName(databasePath);
        }
    }
}
