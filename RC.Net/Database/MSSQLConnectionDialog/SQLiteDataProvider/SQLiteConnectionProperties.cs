using System;
using System.Data.SQLite;

namespace Microsoft.Data.ConnectionUI
{

    public class SQLiteConnectionProperties : AdoDotNetConnectionProperties
    {
        public SQLiteConnectionProperties()
            : base("System.Data.SqlClient")
        {
        }

        public override void Reset()
        {
            base.Reset();
        }

        public override bool IsComplete
        {
            get
            {

                string dataSource = this["Data Source"] as string;

                if (String.IsNullOrEmpty(dataSource))
                {
                    return false;
                }

                return true;
            }
        }

        protected override string ToTestString()
        {
            bool savedPooling = (bool)ConnectionStringBuilder["Pooling"];
            bool wasDefault = !ConnectionStringBuilder.ShouldSerialize("Pooling");
            ConnectionStringBuilder["Pooling"] = false;
            string testString = ConnectionStringBuilder.ConnectionString;
            ConnectionStringBuilder["Pooling"] = savedPooling;
            if (wasDefault)
            {
                ConnectionStringBuilder.Remove("Pooling");
            }
            return testString;
        }

        public override void Test()
        {
            string connectionString = ToFullString();
            SQLiteConnectionStringBuilder builder = new(connectionString);
            builder.DataSource = FixDatabasePath(builder.DataSource);

            // Without "FailIfMissing=True", if data source is set to a non-existing file it will
            // create that file. This dialog is intented to validate a connection to an existing database.
            builder.FailIfMissing = true;

            using var connection = new SQLiteConnection();
            connection.ConnectionString = builder.ConnectionString;
            connection.Open();

            // SQLiteConnection seems to allow connections to be openend on any type of file.
            // Errors are only thrown once a query is attempted against the specified DB.

            try
            {
                using var command = connection.CreateCommand();
                command.CommandText = "pragma schema_version";

                using var reader = command.ExecuteReader();
                
                // Value of 0 can represent an empty file. For purposes of test, look for a DB
                // that has been populated with at least some kind of schema element.
                if (!reader.Read() || reader.GetInt32(0) <= 0)
                {
                    throw new Exception("No schema version found");
                }
            }
            catch (Exception ex)
            {
                // Exception messages caught from SQLite here from can be poorly formatted;
                // ensure tidy message.
                throw new Exception("Not a valid SQLite database", ex);
            }
        }

        /// <summary>
        /// Escape leading backslashes if needed (For UNC paths. Changes \\ to \\\\)
        /// </summary>
        /// <param name="databasePath">The path to a sqlite database file</param>
        private static string FixDatabasePath(string databasePath)
        {
            _ = databasePath ?? throw new ArgumentNullException(nameof(databasePath));

            databasePath = databasePath.Trim();

            // Fix UNC paths so that they resolve correctly
            // http://system.data.sqlite.org/index.html/info/bbdda6eae2
            if (databasePath.StartsWith(@"\\", StringComparison.Ordinal)
                && !databasePath.StartsWith(@"\\\\", StringComparison.Ordinal))
            {
                databasePath = @"\\" + databasePath;
            }

            return databasePath;
        }
    }
}

