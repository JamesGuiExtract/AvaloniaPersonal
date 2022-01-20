using System;
using System.Data.Common;
using System.Data.SqlServerCe;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Extract.Database
{
    /// <summary>
    /// The settings that dictate how to output the data from a database.
    /// </summary>
    public class ExportSettings
    {
        #region Properties

        /// <summary>
        /// The SQLlite database to export from.
        /// </summary>
        public string DatabaseFile { get; set; }

        /// <summary>
        /// The table name the data is in.
        /// </summary>
        public string TableName { get; set; }

        /// <summary>
        /// The query used to retrieve the data.
        /// </summary>
        public string Query { get; set; }

        /// <summary>
        /// The file to output the results to.
        /// </summary>
        public string OutputFile { get; set; }

        /// <summary>
        /// The string that should be used to delimit each row of the results (the delimiter
        /// will not appear before the first row or after the last row)
        /// </summary>
        public string RowDelimiter { get; set; }

        /// <summary>
        /// The string that should be used to delimit each column within a row of the results
        /// (the delimiter will not appear before the first column or after the last column).
        /// </summary>
        public string ColumnDelimiter { get; set; }

        /// <summary>
        /// The string that should appear at the start of the output file (if any)
        /// </summary>
        public string FilePrefix { get; set; }

        /// <summary>
        /// The string that should appear at the end of the output file (if any)
        /// </summary>
        public string FileSuffix { get; set; }

        /// <summary>
        /// Specifies whether output fields should be escaped for use in a regular
        /// expression.
        /// </summary>
        public bool EscapeForRegex { get; set; }

        /// <summary>
        /// Specifies the encoding use to use for the output text.
        /// </summary>
        public Encoding Encoding { get; set; }

        /// <summary>
        /// True to add double quotes to output string values (iff column type is char/string).
        /// </summary>
        public bool AddQuotesToStringValues { get; set; }

        #endregion Properties

        #region Constructors
        /// <summary>
        /// Initializes a new <see cref="Settings"/> instance.
        /// </summary>
        /// <param name="databaseFile">The database file to use</param>
        /// <param name="query">The query used to retrieve results</param>
        /// <param name="outputFile">The file to write results too</param>
        /// <param name="tableName">The name of the table being exported</param>
        /// <param name="addQuotesToStringValues">true to add double quotes to output string values</param>
        public ExportSettings(string databaseFile, 
                              string query, 
                              string outputFile, 
                              string tableName,
                              bool addQuotesToStringValues)
        {
            ExtractException.Assert("ELI39040", "Empty database file name", !String.IsNullOrEmpty(databaseFile));
            ExtractException.Assert("ELI39041", "Empty query", !String.IsNullOrEmpty(query));
            ExtractException.Assert("ELI39042", "Empty output file name", !String.IsNullOrEmpty(outputFile));
            ExtractException.Assert("ELI39145", "Empty table name", !String.IsNullOrWhiteSpace(tableName));

            DatabaseFile = databaseFile;
            Query = query;
            OutputFile = outputFile;
            TableName = tableName;
            AddQuotesToStringValues = addQuotesToStringValues;

            Encoding = Encoding.Default;
            RowDelimiter = Environment.NewLine;
            ColumnDelimiter = "\t";
        }

        /// <summary>
        /// Initializes a new <see cref="Settings"/> instance.
        /// </summary>
        /// <param name="args">The command-line arguments the application was launched with.</param>
        public ExportSettings(string[] args)
        {
            ExtractException.Assert("ELI27125", "Missing required argument.", args.Length >= 3);

            Encoding = Encoding.Default;
            RowDelimiter = Environment.NewLine;
            ColumnDelimiter = "\t";

            // Read the mandatory settings
            DatabaseFile = args[0];
            Query = args[1];
            OutputFile = args[2];

            // Scan for optional settings.
            for (int i = 3; i < args.Length; i++)
            {
                if (args[i].Equals("/rd", StringComparison.OrdinalIgnoreCase))
                {
                    i++;
                    ExtractException.Assert("ELI27122", "Missing row delimiter value.", i < args.Length);

                    RowDelimiter = ParamUnescape(args[i]);
                }
                else if (args[i].Equals("/cd", StringComparison.OrdinalIgnoreCase))
                {
                    i++;
                    ExtractException.Assert("ELI27123", "Missing column delimiter value.", i < args.Length);

                    ColumnDelimiter = ParamUnescape(args[i]);
                }
                else if (args[i].Equals("/fp", StringComparison.OrdinalIgnoreCase))
                {
                    i++;
                    ExtractException.Assert("ELI27128", "Missing file prefix value.", i < args.Length);

                    FilePrefix = ParamUnescape(args[i]);
                }
                else if (args[i].Equals("/fs", StringComparison.OrdinalIgnoreCase))
                {
                    i++;
                    ExtractException.Assert("ELI27129", "Missing file suffix value.", i < args.Length);

                    FileSuffix = ParamUnescape(args[i]);
                }
                else if (args[i].Equals("/enc", StringComparison.OrdinalIgnoreCase))
                {
                    i++;
                    ExtractException.Assert("ELI27729", "Missing code page name.", i < args.Length);

                    Encoding = Encoding.GetEncoding(args[i]);
                }
                else if (args[i].Equals("/esc", StringComparison.OrdinalIgnoreCase))
                {
                    EscapeForRegex = true;
                }
                else
                {
                    ExtractException.Assert("ELI27124", "Unrecognized argument: " + args[i], false);
                }
            }
        }

        #endregion Constructors

        #region Private Functions

        /// <summary>
        /// Replaces printable escape sequences for carriage returns, line feeds and tab characters
        /// with the characters themselves.
        /// </summary>
        /// <param name="parameter">The parameter to un-escape.</param>
        /// <returns>The un-escaped parameter.</returns>
        static string ParamUnescape(string parameter)
        {
            if (parameter == null)
            {
                return null;
            }
            else
            {
                string unescapedParameter = parameter;
                unescapedParameter = unescapedParameter.Replace("\\r", "\r");
                unescapedParameter = unescapedParameter.Replace("\\n", "\n");
                unescapedParameter = unescapedParameter.Replace("\\t", "\t");

                return unescapedParameter;
            }
        }

        #endregion Private Functions
    }

    /// <summary>
    /// This class is used to "export" a table to an output file
    /// </summary>
    public static class ExportTable
    {
        #region Public Functions
        /// <summary>
        /// Exports a table in a SQLite DB into a text file.
        /// NOTE: This is the form used by programs that already have an open connection to DB.
        /// </summary>
        public static string ExportToFile(ExportSettings settings, DbConnection sqlConnection, bool writeEmptyFile = false)
        {
            try
            {
                DbTableColumnInfo tableColumnInfo = new DbTableColumnInfo(settings.TableName, sqlConnection);

                // Issue the query
                using (DbCommand queryCommand = DBMethods.CreateDBCommand(sqlConnection, settings.Query, null))
                {
                    // Initialize a reader for the results.
                    using (DbDataReader reader = queryCommand.ExecuteReader())
                    {
                        int rowsExported = 0;

                        // Declare a writer to output the results.
                        StringBuilder sb = new StringBuilder();

                        // Write the file prefix if specified
                        if (!string.IsNullOrEmpty(settings.FilePrefix))
                        {
                            sb.Append(settings.FilePrefix);
                        }

                        // Has any data been read?
                        bool readData = false;

                        // Loop throw each row of the results.
                        while (reader.Read())
                        {
                            // If previous rows have been read, append the row separator
                            if (readData)
                            {
                                sb.Append(settings.RowDelimiter);
                            }
                            readData = true;

                            // Keep track of all column delimiters that are appended. They are
                            // only added once it is confirmed that there is more data in the
                            // row.
                            StringBuilder pendingColumnDelimiters = new StringBuilder();

                            if (true == settings.AddQuotesToStringValues)
                            {
                                ExtractException.Assert("ELI39148",
                                                        String.Format(CultureInfo.InvariantCulture,
                                                                      "column info size: {0}, mismatches table column size: {1}",
                                                                      tableColumnInfo.Count,
                                                                      reader.FieldCount),
                                                        tableColumnInfo.Count == reader.FieldCount);
                            }

                            // Loop through each column in the row.
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                // If not the first column result, a column separator may be needed.
                                if (i > 0)
                                {
                                    pendingColumnDelimiters.Append(settings.ColumnDelimiter);
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

                                // Escape for RegEx's if necessary
                                if (settings.EscapeForRegex)
                                {
                                    value = Regex.Escape(value);

                                    // Since our RegEx rule removes whitespace before processing
                                    // the RegEx, escaped whitespace needs to be converted to
                                    // hex escaped equivalents to ensure it is preserved.
                                    value = value.Replace("\\ ", "\\x20");
                                    value = value.Replace("\\t", "\\x09");
                                    value = value.Replace("\\r", "\\x0D");
                                    value = value.Replace("\\n", "\\x0A");
                                }

                                if (true == settings.AddQuotesToStringValues)
                                {
                                    value = AddQuotesToValue(value, tableColumnInfo[i]);
                                }

                                // Write the field value
                                sb.Append(value);
                            }

                            rowsExported++;
                        }

                        // Write the file suffix if specified
                        if (!string.IsNullOrEmpty(settings.FileSuffix))
                        {
                            sb.Append(settings.FileSuffix);
                        }

                        if ((readData && sb.Length > 0) || true == writeEmptyFile)
                        {
                            File.WriteAllText(settings.OutputFile, 
                                              sb.ToString(),
                                              settings.Encoding);
                        }

                        return sb.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                ExtractException.Log("ELI40317", ex);
                throw;
            }
        }
    
        #endregion Public Functions

        #region Private Functions

        /// <summary>
        /// This function takes care of the details required to correctly 
        /// surround a string value with quotes, so that it is ready to
        /// be inserted into a database.
        /// </summary>
        /// <param name="value">text to optionally surround with quotes</param>
        /// <param name="columnInfo">contains necessary size and type info</param>
        /// <returns>string value with quotes added ready for SQL insert</returns>
        static string AddQuotesToValue(string value, ColumnInfo columnInfo)
        {
            const int minSizeOfOneCharPlusQuotes = 3;
            if (columnInfo.IsTextColumn() && columnInfo.ColumnSize >= minSizeOfOneCharPlusQuotes)
            {
                const int numberOfSurroundingQuotes = 2;
                int countOfQuotes = value.Count(c => c == '"');
                int totalValueLength = value.Length + countOfQuotes + numberOfSurroundingQuotes;
                if (totalValueLength <= columnInfo.ColumnSize)
                {
                    // Double quote any existing quotes to ensure that adding surrounding quotes
                    // will work correctly, then perform surround quoting.
                    value = value.Replace("\"", "\"\"");
                    value = String.Format(CultureInfo.InvariantCulture, "\"{0}\"", value);
                }
            }

            return value;
        }

        #endregion Private Functions
    }
}
