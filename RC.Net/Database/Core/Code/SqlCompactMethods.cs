using System;
using System.Text;

namespace Extract.Database
{
    /// <summary>
    /// A collection of helper methods for working with Sql Compact databases.
    /// </summary>
    public static class SqlCompactMethods
    {
        /// <summary>
        /// Builds the DB connection string.
        /// </summary>
        /// <param name="compactDBFile">The compact DB file.</param>
        /// <returns>The connection string for connecting to the DB.</returns>
        public static string BuildDBConnectionString(string compactDBFile)
        {
            return BuildDBConnectionString(compactDBFile, false);
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
            try
            {
                var sb = new StringBuilder("Data Source='");
                sb.Append(compactDBFile);
                sb.Append("';");
                if (exclusive)
                {
                    sb.Append("File Mode=Exclusive;");
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
