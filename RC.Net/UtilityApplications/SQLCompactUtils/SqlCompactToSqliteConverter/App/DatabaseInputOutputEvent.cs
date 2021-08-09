namespace Extract.Utilities.SqlCompactToSqliteConverter
{
    /// Source/destination files for database conversion
    public class DatabaseInputOutputEvent
    {
        /// The source database file
        public string InputDatabasePath { get; set; }

        /// The destination database file
        public string OutputDatabasePath { get; set; }
    }
}
