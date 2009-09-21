using Extract;
using Extract.Licensing;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlServerCe;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Extract.SqlCompactExporter
{
    class Program
    {
        /// <summary>
        /// The settings that dictate how to output the data from a database.
        /// </summary>
        class Settings
        {
            /// <summary>
            /// The SQL Compact database to export from.
            /// </summary>
            public string DatabaseFile;

            /// <summary>
            /// The query used to retrieve the data.
            /// </summary>
            public string Query;

            /// <summary>
            /// The file to output the results to.
            /// </summary>
            public string OutputFile;

            /// <summary>
            /// The string that should be used to delimit each row of the results (the delimiter
            /// will not appear before the first row or after the last row)
            /// </summary>
            public string RowDelimiter = Environment.NewLine;

            /// <summary>
            /// The string that should be used to delimit each column within a row of the results
            /// (the delimiter will not appear before the first column or after the last column).
            /// </summary>
            public string ColumnDelimiter = "\t";

            /// <summary>
            /// The string that should appear at the start of the output file (if any)
            /// </summary>
            public string FilePrefix;

            /// <summary>
            /// The string that should appear at the end of the output file (if any)
            /// </summary>
            public string FileSuffix;

            /// <summary>
            /// Specifies whether output fields should be escaped for use in a regular
            /// expression.
            /// </summary>
            public bool EscapeForRegEx;

            /// <summary>
            /// Initializes a new <see cref="Settings"/> instance.
            /// </summary>
            /// <param name="args">The command-line arguments the application was launched with.</param>
            public Settings(string[] args)
            {
                ExtractException.Assert("ELI27125", "Missing required argument.", args.Length >= 3);

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
                        ExtractException.Assert("ELI27122", "Missing row delimeter value.", i < args.Length);

                        RowDelimiter = ParamUnescape(args[i]);
                    }
                    else if (args[i].Equals("/cd", StringComparison.OrdinalIgnoreCase))
                    {
                        i++;
                        ExtractException.Assert("ELI27123", "Missing column delimeter value.", i < args.Length);

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
                    else if (args[i].Equals("/esc", StringComparison.OrdinalIgnoreCase))
                    {
                        EscapeForRegEx = true;
                    }
                    else
                    {
                        ExtractException.Assert("ELI27124", "Unrecognized argument: " + args[i], false);
                    }
                }
            }
        }

        /// <summary>
        /// Exports a table in an SQL compact DB into a text file
        /// </summary>
        static void Main(string[] args)
        {

            Settings settings = null;

            try
            {
                Console.WriteLine();

                LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());
                LicenseUtilities.ValidateLicense(LicenseIdName.FlexIndexIDShieldCoreObjects, "ELI27249",
                    "SqlCompactExporter");

                // Check to see if the user is looking for usage information.
                if (args.Length >= 1 && (args[0].Equals("/?") || args[0].Equals("-?")))
                {
                    PrintUsage();
                    return;
                }

                // Attempt to load the settings from the command-line argument.  If unsuccessful, 
                // log the problem, print usage, then return.
                settings = new Settings(args);
            }
            catch (Exception ex)
            {
                ExtractException.Log("ELI27126", ex);
                Console.WriteLine(ex.Message);
                PrintUsage();
                return;
            }

            try
            {
                // Attempt to connect to the database
                string connectionString = "Data Source='" + settings.DatabaseFile + "';";
                using (SqlCeConnection sqlConnection = new SqlCeConnection(connectionString))
                {
                    sqlConnection.Open();

                    // Issue the query
                    using (SqlCeCommand queryCommand = new SqlCeCommand(settings.Query, sqlConnection))
                    {
                        // Initialize a reader for the results.
                        using (SqlCeDataReader reader = queryCommand.ExecuteReader())
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
                                    string value = reader.GetString(i);
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
                                    if (settings.EscapeForRegEx)
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

                            if (readData && sb.Length > 0)
                            {
                                File.WriteAllText(settings.OutputFile, sb.ToString());
                            }

                            Console.WriteLine("Exported " +
                                rowsExported.ToString(CultureInfo.CurrentCulture) + " rows.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ExtractException.Log("ELI27323", ex);

                Console.WriteLine(ex.Message);
                Console.WriteLine("DatabaseFile: " + settings.DatabaseFile);
                Console.WriteLine("Query: " + settings.Query);
                Console.WriteLine("OutputFile: " + settings.OutputFile);
                Console.WriteLine("RowDelimiter: " + ParamEscape(settings.RowDelimiter));
                Console.WriteLine("ColumnDelimter: " + ParamEscape(settings.ColumnDelimiter));
                Console.WriteLine("FilePrefix: " + ParamEscape(settings.FilePrefix));
                Console.WriteLine("FileSuffix: " + ParamEscape(settings.FileSuffix));
                Console.WriteLine("EscapeForRegEx: " + (settings.EscapeForRegEx ? "Yes" : "No"));
            }
        }

        /// <summary>
        /// Prints usage information for SQLCompactExporter.
        /// </summary>
        static void PrintUsage()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("------------");
            Console.Write("SqlCompactExporter.exe <DatabaseFile> <Query> <OutputFileName> ");
            Console.Write("[/rd <RowDelimiter>] [/cd <ColumnDelimiter>] [/fp <FilePrefix>] ");
            Console.WriteLine("[/fs <FileSuffix>] [/esc]");
            Console.WriteLine();
            Console.WriteLine("DatabaseFile: The SQL Compact database to export from.");
            Console.WriteLine("Query: The SQL query that specifies the output data.");
            Console.WriteLine("OutputFileName: The file to export the data to.");
            Console.WriteLine(
                "/rd <RowDelimiter>: Appears between each row in the output. (default = \\r\\n)");
            Console.WriteLine(
                "/cd <ColumnDelimiter>: Appears between each field in a row. (default = \\t)");
            Console.WriteLine("/fp <FilePrefix>: The output file will begin with this text.");
            Console.WriteLine("/fs <FileSuffix>: The output file will end with this text.");
            Console.WriteLine(
                "/esc: The output for each field will be escaped for use in a regular expression.");
        }

        /// <summary>
        /// Replaces carriage returns, line feeds and tab characters with printable escape sequences.
        /// </summary>
        /// <param name="parameter">The parameter to escape.</param>
        /// <returns>The escaped parameter.</returns>
        static string ParamEscape(string parameter)
        {
            if (parameter == null)
            {
                return null;
            }
            else
            {
                string escapedParameter = parameter;
                escapedParameter = escapedParameter.Replace("\r", "\\r");
                escapedParameter = escapedParameter.Replace("\n", "\\n");
                escapedParameter = escapedParameter.Replace("\t", "\\t");
                
                return escapedParameter;
            }
        }

        /// <summary>
        /// Replaces printable escape sequences for carriage returns, line feeds and tab characters
        /// with the characters themselves.
        /// </summary>
        /// <param name="parameter">The parameter to unescape.</param>
        /// <returns>The unescaped parameter.</returns>
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
    }
}
