using System;
using System.Globalization;
using System.Text;

namespace Extract.Database
{
    /// <summary>
    /// A collection of helper methods for working with Sql Compact databases.
    /// </summary>
    public static class SqlCompactMethods
    {
        /// <summary>
        /// The default max buffer size (in KB)
        /// </summary>
        const int _DEFAULT_MAX_BUFFER = 4096;

        /// <summary>
        /// The maximum size any sql compact database can be.
        /// </summary>
        public static readonly int MaxCompactDatabaseSize = 4091;

        /// <summary>
        /// Builds the DB connection string.
        /// </summary>
        /// <param name="compactDBFile">The compact DB file.</param>
        /// <returns>The connection string for connecting to the DB.</returns>
        public static string BuildDBConnectionString(string compactDBFile)
        {
            return BuildDBConnectionString(compactDBFile, false, -1, _DEFAULT_MAX_BUFFER);
        }

        /// <summary>
        /// Builds the DB connection string.
        /// </summary>
        /// <param name="compactDBFile">The compact DB file.</param>
        /// <param name="exclusive">if set to <see langword="true"/> then builds a connection
        /// string that will provide exclusive access to the DB file.</param>
        /// <returns>
        /// The connection string for connecting to the DB.
        /// </returns>
        public static string BuildDBConnectionString(string compactDBFile, bool exclusive)
        {
            return BuildDBConnectionString(compactDBFile, exclusive, -1, _DEFAULT_MAX_BUFFER);
        }

        /// <summary>
        /// Builds the DB connection string.
        /// </summary>
        /// <param name="compactDBFile">The compact DB file.</param>
        /// <param name="exclusive">if set to <see langword="true"/> then builds a connection
        /// string that will provide exclusive access to the DB file.</param>
        /// <param name="maxDatabaseSize">Max size of the database (in MB).</param>
        /// <returns>
        /// The connection string for connecting to the DB.
        /// </returns>
        public static string BuildDBConnectionString(string compactDBFile, bool exclusive,
            int maxDatabaseSize)
        {
            return BuildDBConnectionString(compactDBFile, exclusive, maxDatabaseSize,
                _DEFAULT_MAX_BUFFER);
        }

        /// <summary>
        /// Builds the DB connection string.
        /// </summary>
        /// <param name="compactDBFile">The compact DB file.</param>
        /// <param name="exclusive">if set to <see langword="true"/> then builds a connection
        /// string that will provide exclusive access to the DB file.</param>
        /// <param name="maxDatabaseSize">Max size of the database (in MB).</param>
        /// <param name="maxBufferSize">Max size of the buffer (in KB).</param>
        /// <returns>
        /// The connection string for connecting to the DB.
        /// </returns>
        public static string BuildDBConnectionString(string compactDBFile, bool exclusive,
            int maxDatabaseSize, int maxBufferSize)
        {
            try
            {
                if (maxDatabaseSize > MaxCompactDatabaseSize)
                {
                    throw new ArgumentOutOfRangeException("maxDatabaseSize", maxDatabaseSize,
                        "Value must be less than "
                        + MaxCompactDatabaseSize.ToString(CultureInfo.CurrentCulture));
                }

                var sb = new StringBuilder("Data Source='");
                sb.Append(compactDBFile);
                sb.Append("';");
                if (exclusive)
                {
                    sb.Append("File Mode=Exclusive;");
                }
                if (maxDatabaseSize > 0)
                {
                    sb.Append("Max Database Size=");
                    sb.Append(maxDatabaseSize);
                    sb.Append(";");
                }
                if (maxBufferSize > 0)
                {
                    sb.Append("Max Buffer Size=");
                    sb.Append(maxBufferSize);
                    sb.Append(";");
                }

                return sb.ToString();
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI31093", ex);
            }
        }
    }
}
