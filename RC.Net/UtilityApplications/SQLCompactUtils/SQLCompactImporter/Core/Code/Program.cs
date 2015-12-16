using Extract;
using Extract.Licensing;
using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlServerCe;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace Extract.SqlCompactImporter
{
    class Program
    {
        /// <summary>
        /// The settings that dictate how to import data into a database.
        /// </summary>
        class Settings
        {
            /// <summary>
            /// The SQL Compact database to import data into.
            /// </summary>
            public string DatabaseFile;

            /// <summary>
            /// The table name the data should be imported into.
            /// </summary>
            public string TableName;

            /// <summary>
            /// The SQL command used to add the last row.
            /// </summary>
            public string CommandText;

            /// <summary>
            /// The file containing the data to import.
            /// </summary>
            public string InputFile;

            /// <summary>
            /// The string that delimits each row of data in the text file.
            /// </summary>
            public string RowDelimiter = Environment.NewLine;

            /// <summary>
            /// The string that delimits each column of data in the text file.
            /// </summary>
            public string ColumnDelimiter = "\t";

            /// <summary>
            /// Initializes a new <see cref="Settings"/> instance.
            /// </summary>
            /// <param name="args">The command-line arguments the application was launched with.</param>
            public Settings(string[] args)
            {
                ExtractException.Assert("ELI27131", "Missing required argument.", args.Length >= 3);

                // Read the mandatory settings
                DatabaseFile = args[0];
                TableName = args[1];
                InputFile = args[2];

                // Scan for optional settings.
                for (int i = 3; i < args.Length; i++)
                {
                    if (args[i].Equals("/rd", StringComparison.OrdinalIgnoreCase))
                    {
                        i++;
                        ExtractException.Assert("ELI27132", "Missing row delimeter value.", i < args.Length);

                        RowDelimiter = ParamUnescape(args[i]);
                    }
                    else if (args[i].Equals("/cd", StringComparison.OrdinalIgnoreCase))
                    {
                        i++;
                        ExtractException.Assert("ELI27133", "Missing column delimeter value.", i < args.Length);

                        ColumnDelimiter = ParamUnescape(args[i]);
                    }
                    else
                    {
                        ExtractException.Assert("ELI27134", "Unrecognized argument: " + args[i], false);
                    }
                }
            }
        }

        /// <summary>
        /// Populates a table in an SQL compact DB using the data in a text file.
        /// </summary>
        static void Main(string[] args)
        {
            Settings settings = null;
            int rowsProcessed = 0;
            int rowsFailed = 0;

            try
            {
                Console.WriteLine();

                LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());
                LicenseUtilities.ValidateLicense(LicenseIdName.FlexIndexIDShieldCoreObjects, "ELI27250",
                    "SqlCompactImporter");

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
                ExtractException.Log("ELI27135", ex);
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

                    string data = File.ReadAllText(settings.InputFile);
                    string[] rows = data.Split(new string[] { settings.RowDelimiter },
                            StringSplitOptions.RemoveEmptyEntries);

                    // Obtain information about the columns the data is to be imported into.
                    List<int> columnSizes = new List<int>();
                    List<string> columnNames = new List<string>();
                    string schemaQuery = "SELECT COLUMN_NAME, CHARACTER_MAXIMUM_LENGTH FROM " +
                        "INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = '" + settings.TableName + "'";
                    using (SqlCeCommand schemaCommand = new SqlCeCommand(schemaQuery, sqlConnection))
                    {
                        using (SqlCeDataReader reader = schemaCommand.ExecuteReader())
                        {
                            for (int i = 0; reader.Read(); i++)
                            {
                                columnNames.Add(reader.GetString(0));

                                if (reader.IsDBNull(1))
                                {
                                    columnSizes.Add(0);
                                }
                                else
                                {
                                    columnSizes.Add(reader.GetInt32(1));
                                }
                            }
                        }
                    }

                    ExtractException.Assert("ELI27253",
                        "Could not find table: " + settings.TableName, columnSizes.Count > 0);

                    // Loop through each row of the input file and it to the DB.
                    for (rowsProcessed = 0; rowsProcessed < rows.Length; rowsProcessed++)
                    {
                        Dictionary<string, string> columnValues =
                            new Dictionary<string, string>(columnSizes.Count);

                        // Split the input row into columns using the column delimiter.
                        string[] columns = rows[rowsProcessed].Split(
                            new string[] { settings.ColumnDelimiter }, StringSplitOptions.None);

                        int columnCount = Math.Min(columns.Length, columnNames.Count);
                        string[] includedColumns = new string[columnCount];
                        columnNames.CopyTo(0, includedColumns, 0, columnCount);

                        // Initialize the SQL command used to add the data.
                        StringBuilder commandText = new StringBuilder("INSERT INTO [");
                        commandText.Append(settings.TableName);
                        commandText.Append("] ([");
                        commandText.Append(string.Join("], [", includedColumns)); 
                        commandText.Append("]) VALUES (");

                        // Parameterize the data for each column and build them into the command.
                        for (int i = 0; i < columnCount; i++)
                        {
                            string key = "@" + i.ToString(CultureInfo.InvariantCulture);
                            string value = (i < columns.Length) ? columns[i] : "";
                            if (columnSizes[i] > 0 && columnSizes[i] < value.Length)
                            {
                                value = value.Substring(0, columnSizes[i]);
                            }

                            columnValues[key] = value;
                            commandText.Append(key);

                            if (i + 1 < columnCount)
                            {
                                commandText.Append(", ");
                            }
                        }

                        // Complete the command
                        commandText.Append(")");
                        settings.CommandText = commandText.ToString();

                        // Issue the command to add the data.
                        using (SqlCeCommand command =
                            new SqlCeCommand(settings.CommandText, sqlConnection))
                        {
                            foreach (string key in columnValues.Keys)
                            {
                                command.Parameters.AddWithValue(key, columnValues[key]);
                            }

                            try
                            {
                                command.ExecuteNonQuery();
                            }
                            catch (Exception ex)
                            {
                                rowsFailed++;
                                Console.WriteLine("Failed to import row \"" + rows[rowsProcessed] +
                                    "\" with the following error:");
                                Console.WriteLine(ex.Message);
                                Console.WriteLine();
                            }
                        }
                    }
                } 
            }
            catch (Exception ex)
            {
                ExtractException.Log("ELI27127", ex);

                Console.WriteLine(ex.Message);
                Console.WriteLine("DatabaseFile: " + settings.DatabaseFile);
                Console.WriteLine("TableName: " + settings.TableName);
                Console.WriteLine("LastExecutedCommand: " + settings.CommandText);
                Console.WriteLine("InputFile: " + settings.InputFile );
                Console.WriteLine("RowDelimiter: " + ParamEscape(settings.RowDelimiter));
                Console.WriteLine("ColumnDelimter: " + ParamEscape(settings.ColumnDelimiter));

                Console.WriteLine();
            }

            int rowsAdded = rowsProcessed - rowsFailed;
            Console.WriteLine("Added " + rowsAdded.ToString(CultureInfo.CurrentCulture) + " rows.");
            if (rowsFailed > 0)
            {
                Console.WriteLine("Failed to import " +
                    rowsFailed.ToString(CultureInfo.CurrentCulture) + " rows.");
            }
        }

        /// <summary>
        /// Prints usage information for SQLCompactImporter.
        /// </summary>
        static void PrintUsage()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("------------");
            Console.Write("SqlCompactImporter.exe <DatabaseFile> <TableName> <InputFileName> ");
            Console.WriteLine("[/rd <RowDelimiter>] [/cd <ColumnDelimiter>]");
            Console.WriteLine();
            Console.WriteLine("DatabaseFile: The SQL Compact database the data is to be imported into.");
            Console.WriteLine("TableName: The table the data is to be imported into.");
            Console.Write("InputFileName: The file containing the data to import. All columns in ");
            Console.Write("the input file will be imported unless there are fewer columns in the ");
            Console.Write("destination table than in the input file in which case the extra ");
            Console.WriteLine("columns in the input file will be disregarded.");
            Console.Write("/rd <RowDelimiter>: The string that delimits each row of data in the ");
            Console.WriteLine("input file. (default = \\r\\n)");
            Console.Write("/cd <ColumnDelimiter>: The string that delimits each column in a row ");
            Console.WriteLine("of data in the input file.(default = \\t)");
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
