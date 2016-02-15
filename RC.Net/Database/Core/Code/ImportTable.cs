using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlServerCe;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Extract.Database
{

    /// <summary>
    /// The settings that dictate how to import data into a database.
    /// </summary>
    public class ImportSettings
    {
        #region Properties

        /// <summary>
        /// The SQL Compact database to import data into.
        /// </summary>
        public string DatabaseFile {get; set;}

        /// <summary>
        /// The table name the data should be imported into.
        /// </summary>
        public string TableName {get; set;}

        /// <summary>
        /// The SQL command used to add the last row. Unused AFAICT.
        /// </summary>
        public string CommandText {get; set;}

        /// <summary>
        /// The file containing the data to import.
        /// </summary>
        public string InputFile {get; set;}

        /// <summary>
        /// The string that delimits each row of data in the text file.
        /// </summary>
        public string RowDelimiter {get; set;}

        /// <summary>
        /// The string that delimits each column of data in the text file.
        /// </summary>
        public string ColumnDelimiter {get; set;}

        /// <summary>
        /// For import the GUI client only supports executing the entire import 
        /// from within a transaction context.
        /// </summary>
        public bool UseTransaction {get; set;}

        /// <summary>
        /// When true, the import replaces the current data with the imported data.
        /// When false, the import appends the imported data to the current data.
        /// </summary>
        public bool ReplaceData { get; set; }

        /// <summary>
        /// When true, this instance is an "extended usage" scenario. 
        /// Originally this code was used by a console application,
        /// and that application has fewer requirements, so this flag has been added
        /// to ensure backward compatibility.
        /// </summary>
        public bool ExtendedUse { get; set; }

        #endregion Properties

        #region Constructors

        /// <summary>
        /// Initializes a new <see cref="Settings"/> instance.
        /// This CTOR is used by GUI clients
        /// </summary>
        public ImportSettings(string databaseFile, 
                              string tableName, 
                              string inputFile,
                              bool useTransaction,
                              bool replaceDataRows)
        {
            ExtractException.Assert("ELI39036", "Empty database file name", !String.IsNullOrEmpty(databaseFile));
            ExtractException.Assert("ELI39037", "Empty table name", !String.IsNullOrEmpty(tableName));
            ExtractException.Assert("ELI39039", "Empty input file name", !String.IsNullOrEmpty(inputFile));

            DatabaseFile = databaseFile;
            TableName = tableName;
            InputFile = inputFile;
            UseTransaction = useTransaction;
            ReplaceData = replaceDataRows;

            RowDelimiter = Environment.NewLine;
            ColumnDelimiter = "\t";

            ExtendedUse = true;
        }

        /// <summary>
        /// Initializes a new <see cref="Settings"/> instance.
        /// This CTOR is used by console apps, passed command line args.
        /// </summary>
        /// <param name="args">The command-line arguments the application was launched with.</param>
        public ImportSettings(string[] args)
        {
            ExtractException.Assert("ELI27131", "Missing required argument.", args.Length >= 3);

            RowDelimiter = Environment.NewLine;
            ColumnDelimiter = "\t";
            UseTransaction = false;     // not supported for Console apps

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
                    ExtractException.Assert("ELI27132", "Missing row delimiter value.", i < args.Length);

                    RowDelimiter = ParamUnescape(args[i]);
                }
                else if (args[i].Equals("/cd", StringComparison.OrdinalIgnoreCase))
                {
                    i++;
                    ExtractException.Assert("ELI27133", "Missing column delimiter value.", i < args.Length);

                    ColumnDelimiter = ParamUnescape(args[i]);
                }
                else
                {
                    ExtractException.Assert("ELI27134", "Unrecognized argument: " + args[i], false);
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

    }           // end of class ImportSettings

    /// <summary>
    /// This class is used to "import" a table from a specified database file.
    /// </summary>

    public static class ImportTable
    {
        #region Public Functions

        /// <summary>
        /// Populates a table in an SQL compact DB using the data in a text file.
        /// </summary>
        /// <param name="settings"> import settings for all operations </param>
        /// <param name="sqlConnection">connection to use for DB access</param>
        /// <returns>Tuple where Item1 is an int which is the number of rows that failed,
        ///  and Item2 is a string[] that is the set of execution messages, one for each row 
        ///  operation processed</returns>        
        public static Tuple<int, string[]> ImportFromFile( ImportSettings settings, DbConnection sqlConnection )
        {
            List<string> messages = new List<string>();
            int rowsProcessed = 0;
            int rowsFailed = 0;
            DbTransaction tx = null;

            try
            {
                if (settings.UseTransaction)
                {
                    tx = sqlConnection.BeginTransaction();
                }

                string data = File.ReadAllText(settings.InputFile);
                string[] rows = data.Split(new string[] { settings.RowDelimiter },
                        StringSplitOptions.RemoveEmptyEntries);

                // Obtain information about the columns the data is to be imported into.
                List<int> columnSizes = new List<int>();
                List<string> columnNames = new List<string>();
                DbTableColumnInfo tci = new DbTableColumnInfo(settings.TableName, sqlConnection);
                foreach (var ci in tci)
                {
                    columnNames.Add(ci.ColumnName);
                    columnSizes.Add(ci.ColumnSize);
                }

                ExtractException.Assert("ELI27253",
                    "Could not get column info for table: " + settings.TableName,
                    columnSizes.Count > 0);

                if (settings.ReplaceData)
                {
                    string deleteRows = "DELETE FROM " + settings.TableName;
                    using (DbCommand deleteCommand =
                        DBMethods.CreateDBCommand(sqlConnection, deleteRows, null))
                    {
                        deleteCommand.ExecuteNonQuery();
                    }
                }

                // Loop through each row of the input file and add it to the table.
                for (rowsProcessed = 0; rowsProcessed < rows.Length; rowsProcessed++)
                {
                    Dictionary<string, string> columnValues =
                        new Dictionary<string, string>(columnSizes.Count);

                    // Split the input row into columns using the column delimiter.
                    string[] columns = MakeColumns(rowText: rows[rowsProcessed],
                                                   delimiter: settings.ColumnDelimiter,
                                                   useAdvancedSplitter: settings.ExtendedUse);

                    int columnCount = Math.Min(columns.Length, columnNames.Count);
                    string[] includedColumns = CopyIncludedColumns(columnNames, columnCount, tci, settings.ReplaceData);

                    // Initialize the SQL command used to add the data.
                    StringBuilder commandText = new StringBuilder("INSERT INTO [");
                    commandText.Append(settings.TableName);
                    commandText.Append("] ([");
                    commandText.Append(string.Join("], [", includedColumns));
                    commandText.Append("]) VALUES (");

                    // Parameterize the data for each column and build them into the command.
                    int columnDataAdded = 0;
                    for (int i = 0; i < columnCount; i++)
                    {
                        // In append mode, do NOT populate auto-increment columns
                        if (!settings.ReplaceData && tci[i].IsAutoIncrement)
                        {
                            continue;
                        }

                        string key = "@" + i.ToString(CultureInfo.InvariantCulture);
                        string value = (i < columns.Length) ? columns[i] : "";

                        if (settings.ExtendedUse)
                        {
                            value = AdjustDoubleQuotes(value);
                        }

                        if (tci[i].IsTextColumn() && columnSizes[i] > 0 && columnSizes[i] < value.Length)
                        {
                            value = value.Substring(0, columnSizes[i]);
                        }

                        columnValues[key] = value;
                        if (columnDataAdded > 0)
                        {
                            commandText.Append(", ");
                        }

                        commandText.Append(key);
                        ++columnDataAdded;
                    }

                    // Complete the command
                    commandText.Append(")");
                    settings.CommandText = commandText.ToString();

                    try
                    {
                        if (settings.ReplaceData)
                        {
                            messages.Add(ReplaceInsert(settings, sqlConnection, columnValues, tci, tx));
                        }
                        else
                        {
                            messages.Add(AppendInsert(settings, sqlConnection, columnValues, tx));
                        }
                    }
                    catch (Exception ex)
                    {
                        rowsFailed++;

                        StringBuilder message = new StringBuilder();
                        message.AppendFormat("*Failed to import row \"{0}\" with the following error: {1}",
                                              rows[rowsProcessed],
                                              ex.Message);
                        messages.Add(message.ToString());
                    }
                }

                if (settings.UseTransaction)
                {
                    if (0 != rowsFailed)
                    {
                        tx.Rollback();
                    }
                    else
                    {
                        tx.Commit();
                    }
                }
            }
            catch (Exception ex)
            {
                if (null != tx)
                {
                    tx.Rollback();
                }

                throw new ExtractException("ELI27127", "Import operation failed.", ex);
            }
            finally
            {
                if (null != tx)
                {
                    tx.Dispose();
                }
            }

            return Tuple.Create(rowsFailed, messages.ToArray());
        }

        /// <summary>
        /// Populates a table in an SQL compact DB using the data in a text file.
        /// </summary>
        /// <param name="settings"> import settings for all operations </param>
        /// <returns>Tuple where Item1 is an int which is the number of rows that failed,
        ///  and Item2 is a string[] that is the set of execution messages, one for each row 
        ///  operation processed</returns>        
        public static Tuple<int, string[]> ImportFromFile( ImportSettings settings )
        {
            try
            {
                // Attempt to connect to the database
                string connectionString = "Data Source='" + settings.DatabaseFile + "';";
                using (SqlCeConnection sqlConnection = new SqlCeConnection(connectionString))
                {
                    sqlConnection.Open();
                    return ImportFromFile(settings, sqlConnection);
                }
            }
            catch (Exception ex)
            {
                ExtractException.Log("ELI27127", ex);
                throw;
            }
        }

        /// <summary>
        /// This function splits columns in the presence of embedded quote marks
        /// when useAdvancedSplitter is true. for backwards compatibility, it also
        /// includes the original split-based algorithm, used by console apps.
        /// </summary>
        /// <param name="rowText">The line of text to split</param>
        /// <param name="delimiter">The delimiter to use. Note that single 
        /// character delimiter is required when useAdvancedSplitter is true</param>
        /// <param name="useAdvancedSplitter">true to use advanced splitting, false to use simple String.Split()</param>
        /// <returns>string[] of column data in input order, including empty columns</returns>
        [SuppressMessage("ExtractRules", "ES0001: WrapExceptionAsExtractException")]
        public static string[] MakeColumns(string rowText, string delimiter, bool useAdvancedSplitter)
        {
            if (!useAdvancedSplitter)
            {
                return rowText.Split(new string[] { delimiter }, StringSplitOptions.None);
            }
            else
            {
                // Here to split in a fashion that will exclude quoted substrings from being split.
                // Delimiters inside of quoted substrings will NOT be considered for splitting.
                ExtractException.Assert("ELI39152",
                                        "Delimiter with more than one character not allowed",
                                        delimiter.Length == 1);

                List<string> columns = new List<string>();

                const int outside = 1;
                const int inside = 2;
                const char quoteMark = '"';

                var count = rowText.Count(c => c == quoteMark);
                ExtractException.Assert("ELI39153",
                                        "There is an unmatched quote mark in the input text",
                                        count % 2 == 0,
                                        "Input row of text",
                                        rowText);
                int start = 0;
                char cDelimiter = delimiter[0];
                int state = outside;
                for (int i = 0; i < rowText.Length; ++i)
                {
                    char c = rowText[i];

                    if (c == cDelimiter)
                    {
                        if (state == outside)
                        {
                            columns.Add(rowText.Substring(start, length: i - start));
                            start = i + 1;
                        }
                    }
                    else if (c == quoteMark)
                    {
                        state = state == inside ? outside : inside;
                    }
                }

                // Copy the last segment of the line - this is the final state transition
                // start == text.Length when the last character is a comma, otherwise copy
                if (start < rowText.Length)
                {
                    columns.Add(rowText.Substring(start));
                }

                return columns.ToArray();
            }
        }

        #endregion Public Functions

        #region Private Functions

        /// <summary>
        /// Replace all double quote marks, and then remove any leading and trailing double quote (") characters 
        /// </summary>
        /// <param name="value">input string to remove quotes from</param>
        /// <returns>Returns string with double quote characters at start and 
        /// end removed iff quotes existed</returns>
        static string AdjustDoubleQuotes(string value)
        {
            const string doubledQuoteMark = "\"\"";
            const string quoteMark = "\"";
            value = value.Replace(doubledQuoteMark, quoteMark);

            if (value.StartsWith(quoteMark, StringComparison.OrdinalIgnoreCase) &&
                value.EndsWith(quoteMark, StringComparison.OrdinalIgnoreCase))
            {
                const int excludeFirstChar = 1;
                int excludeLastChar = value.Length - 2;
                return value.Substring(excludeFirstChar, excludeLastChar);
            }

            return value;
        }

        /// <summary>
        /// Insert a row into the table, overwriting what was present.
        /// </summary>
        /// <param name="settings">import settings</param>
        /// <param name="connection">database connection, open</param>
        /// <param name="columnValues">a set of column values for the row being inserted</param>
        /// <param name="columnInfo">column information</param>
        /// <param name="tx">transaction context</param>
        /// <returns>result string</returns>
        static string ReplaceInsert(ImportSettings settings,
                                    DbConnection connection,
                                    Dictionary<string, string> columnValues,
                                    DbTableColumnInfo columnInfo,
                                    DbTransaction tx)
        {
            // Only set identity_insert ON|OFF iff the table has an identity column.
            bool hasAutoIncrement = columnInfo.Count(column => column.IsAutoIncrement) > 0;

            using (DbCommand command = DBMethods.CreateDBCommand(connection, settings.CommandText, columnValues))
            {
                if (settings.UseTransaction)
                {
                    command.Transaction = tx;
                }

                if (hasAutoIncrement)
                {
                    SetIdentityInsert(connection, settings.UseTransaction ? tx : null,
                        settings.TableName, true);
                }

                command.ExecuteNonQuery();

                if (hasAutoIncrement)
                {
                    SetIdentityInsert(connection, settings.UseTransaction ? tx : null,
                        settings.TableName, false);
                }

                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("Succeeded: {0}", command.ToString());

                return sb.ToString();
            }
        }

        /// <summary>
        /// Turns IDENTITY_INSERT on or off for the specified <see paramref="tableName"/>.
        /// </summary>
        /// <param name="connection">The open <see cref="DbConnection"/> to use.</param>
        /// <param name="tx">The <see cref="DbTransaction"/> to use or <see langword="null"/> if no
        /// transaction is to be used.</param>
        /// <param name="tableName">Name of the table for which IDENTITY_INSERT is to be modified.
        /// </param>
        /// <param name="setOn"><see langword="true"/> to set IDENTITY_INSERT ON;
        /// <see langword="false"/> to set IDENTITY_INSERT OFF.</param>
        static void SetIdentityInsert(DbConnection connection, DbTransaction tx, string tableName, bool setOn)
        {
            using (var command = DBMethods.CreateDBCommand(connection,
                    string.Format(CultureInfo.InvariantCulture,
                        "SET IDENTITY_INSERT [{0}] {1}", tableName, setOn ? "ON" : "OFF"), 
                    null))
            {
                if (tx != null)
                {
                    command.Transaction = tx;
                }

                command.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// Append a row to the table.
        /// </summary>
        /// <param name="settings">import settings</param>
        /// <param name="connection">database connection, open</param>
        /// <param name="columnValues">set of column values for the row being inserted</param>
        /// <param name="tx">transaction context</param>
        /// <returns>result string</returns>
        static string AppendInsert(ImportSettings settings, 
                                   DbConnection connection, 
                                   Dictionary<string, string>columnValues,
                                   DbTransaction tx)
        {
            using (var command = DBMethods.CreateDBCommand(
                connection, settings.CommandText, columnValues))
            {
                if (settings.UseTransaction)
                {
                    command.Transaction = tx;
                }

                command.ExecuteNonQuery();
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("Succeeded: {0}", command.ToString());

                return sb.ToString();
            }
        }


        /// <summary>
        /// This function abstracts copying included columns, hiding the differences between 
        /// append and replace modes.
        /// </summary>
        /// <param name="columnNames">table column names</param>
        /// <param name="columnCount">the number of columns to include</param>
        /// <param name="columnInfo">column info, so that auto-increment columns can be skipped iff appending</param>
        /// <param name="replaceMode">append or replace</param>
        /// <returns>array of included column names</returns>
        static string[] CopyIncludedColumns(List<string> columnNames, 
                                            int columnCount,
                                            DbTableColumnInfo columnInfo, 
                                            bool replaceMode)
        {
            if (replaceMode)
            {
                string[] columnsToInclude = new string[columnCount];
                columnNames.CopyTo(0, columnsToInclude, 0, columnCount);
                return columnsToInclude;
            }

            // Here to handle append mode. Remove any columns that are auto-increment.
            List<string> outColumns = new List<string>();
            for (int i = 0; i < columnInfo.Count; ++i)
            {
                if (!columnInfo[i].IsAutoIncrement)
                {
                    outColumns.Add(columnNames[i]);
                }
            }

            return outColumns.ToArray();
        }

        #endregion Private Functions
    }
}
