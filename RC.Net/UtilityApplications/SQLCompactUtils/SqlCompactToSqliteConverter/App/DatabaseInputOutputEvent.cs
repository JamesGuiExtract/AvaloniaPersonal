using System;
using System.Collections.Generic;

namespace Extract.Utilities.SqlCompactToSqliteConverter
{
    /// Source/destination files for database conversion
    public class DatabaseInputOutputEvent : IEquatable<DatabaseInputOutputEvent>
    {
        /// Create a new instance with specified paths
        public DatabaseInputOutputEvent(string inputPath, string outputPath)
        {
            InputDatabasePath = inputPath;
            OutputDatabasePath = outputPath;
        }

        /// The source database file
        public string InputDatabasePath { get; set; }

        /// The destination database file
        public string OutputDatabasePath { get; set; }

        public override bool Equals(object obj)
        {
            return Equals(obj as DatabaseInputOutputEvent);
        }

        public bool Equals(DatabaseInputOutputEvent other)
        {
            return other != null &&
               InputDatabasePath == other.InputDatabasePath &&
               OutputDatabasePath == other.OutputDatabasePath;
        }

        public override int GetHashCode()
        {
            return HashCode.Start
                .Hash(InputDatabasePath)
                .Hash(OutputDatabasePath);
        }

        public static bool operator ==(DatabaseInputOutputEvent left, DatabaseInputOutputEvent right)
        {
            return EqualityComparer<DatabaseInputOutputEvent>.Default.Equals(left, right);
        }

        public static bool operator !=(DatabaseInputOutputEvent left, DatabaseInputOutputEvent right)
        {
            return !(left == right);
        }
    }
}
