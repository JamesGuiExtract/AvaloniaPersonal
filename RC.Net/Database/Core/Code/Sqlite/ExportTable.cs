using System;
using System.Data.Common;
using System.IO;
using System.Text;

namespace Extract.Database.Sqlite
{
    /// <summary>
    /// The settings that dictate how to output the data from a database.
    /// </summary>
    public class ExportSettings
    {
        /// <summary>
        /// The query used to retrieve the data.
        /// </summary>
        public string Query { get; set; }

        /// <summary>
        /// The file to output the results to.
        /// </summary>
        public string OutputFile { get; set; }

        /// <summary>
        /// Initializes a new <see cref="ExportSettings"/> instance.
        /// </summary>
        /// <param name="query">The query used to retrieve results</param>
        /// <param name="outputFile">The file to write results too</param>
        public ExportSettings(string query, string outputFile)
        {
            ExtractException.Assert("ELI39041", "Empty query", !String.IsNullOrEmpty(query));
            ExtractException.Assert("ELI39042", "Empty output file name", !String.IsNullOrEmpty(outputFile));

            Query = query;
            OutputFile = outputFile;
        }
    }

    /// <summary>
    /// This class is used to "export" a table to an output file
    /// </summary>
    public static class ExportTable
    {
        /// <summary>
        /// Exports a table in a SQLite DB into a text file.
        /// </summary>
        public static string ExportToFile(ExportSettings settings, DbConnection sqlConnection, bool writeEmptyFile = false)
        {
            try
            {
                // Issue the query
                using DbCommand queryCommand = DBMethods.CreateDBCommand(sqlConnection, settings.Query, null);
                // Initialize a reader for the results.
                using DbDataReader reader = queryCommand.ExecuteReader();
                int rowsExported = 0;

                // Declare a writer to output the results.
                StringBuilder sb = new StringBuilder();

                // Has any data been read?
                bool readData = false;

                // Loop throw each row of the results.
                while (reader.Read())
                {
                    // If previous rows have been read, append the row separator
                    if (readData)
                    {
                        sb.Append(Environment.NewLine);
                    }
                    readData = true;

                    // Keep track of all column delimiters that are appended. They are
                    // only added once it is confirmed that there is more data in the
                    // row.
                    StringBuilder pendingColumnDelimiters = new StringBuilder();

                    // Loop through each column in the row.
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        // If not the first column result, a column separator may be needed.
                        if (i > 0)
                        {
                            pendingColumnDelimiters.Append(',');
                        }

                        // If the column value is NULL, there is nothing to write.
                        if (reader.IsDBNull(i))
                        {
                            continue;
                        }

                        // If the column value is empty, there is nothing to write.
                        string value = reader.GetValue(i).ToString();
                        if (string.IsNullOrEmpty(value))
                        {
                            continue;
                        }

                        // If there is data to write, go ahead and commit all pending
                        // column delimiters.
                        sb.Append(pendingColumnDelimiters.ToString());

                        // Reset the pending column delimiters
                        pendingColumnDelimiters = new StringBuilder();

                        value = value.QuoteIfNeeded("\"", ",");

                        // Write the field value
                        sb.Append(value);
                    }

                    rowsExported++;
                }

                if ((readData && sb.Length > 0) || true == writeEmptyFile)
                {
                    File.WriteAllText(settings.OutputFile, sb.ToString());
                }

                return sb.ToString();
            }
            catch (Exception ex)
            {
                ExtractException.Log("ELI40317", ex);
                throw;
            }
        }
    }
}
