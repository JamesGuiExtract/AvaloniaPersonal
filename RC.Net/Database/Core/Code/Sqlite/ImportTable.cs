using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Globalization;
using System.Text;
using Extract.Utilities;

namespace Extract.Database.Sqlite
{

    /// <summary>
    /// The settings that dictate how to import data into a database.
    /// </summary>
    public class ImportSettings
    {
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
        /// Initializes a new <see cref="ImportSettings"/> instance.
        /// </summary>
        public ImportSettings(string tableName, 
                              string inputFile,
                              bool useTransaction,
                              bool replaceDataRows)
        {
            ExtractException.Assert("ELI39037", "Empty table name", !String.IsNullOrEmpty(tableName));
            ExtractException.Assert("ELI39039", "Empty input file name", !String.IsNullOrEmpty(inputFile));

            TableName = tableName;
            InputFile = inputFile;
            UseTransaction = useTransaction;
            ReplaceData = replaceDataRows;
        }

    }           // end of class ImportSettings

    /// <summary>
    /// This class is used to "import" a table from a specified database file.
    /// </summary>
    public static class ImportTable
    {
        /// <summary>
        /// Populates a table in a SQLite DB using the data in a text file.
        /// </summary>
        /// <param name="settings"> import settings for all operations </param>
        /// <param name="sqlConnection">connection to use for DB access</param>
        /// <returns>Tuple where Item1 is an int which is the number of rows that failed,
        ///  and Item2 is a string[] that is the set of execution messages, one for each row 
        ///  operation processed</returns>        
        public static Tuple<int, string[]> ImportFromFile( ImportSettings settings, DbConnection sqlConnection )
        {
            List<string> messages = new List<string>();
            int rowsFailed = 0;
            DbTransaction tx = null;
            SqliteColumnCollection tci = null;

            try
            {
                if (settings.UseTransaction)
                {
                    tx = sqlConnection.BeginTransaction();
                }

                using var csvReader = new Microsoft.VisualBasic.FileIO.TextFieldParser(settings.InputFile);
                csvReader.Delimiters = new[] { "," };

                // Obtain information about the columns the data is to be imported into.
                List<int> columnSizes = new List<int>();
                List<string> columnNames = new List<string>();
                tci = new SqliteColumnCollection(settings.TableName, sqlConnection);
                foreach (var ci in tci)
                {
                    columnNames.Add(ci.ColumnName);
                    columnSizes.Add(ci.ColumnSize);
                }

                ExtractException.Assert("ELI40316",
                    "Could not get column info for table: " + settings.TableName,
                    columnSizes.Count > 0);

                if (settings.ReplaceData)
                {
                    string deleteRows = UtilityMethods.FormatInvariant($"DELETE FROM [{settings.TableName}]");
                    using (DbCommand deleteCommand =
                        DBMethods.CreateDBCommand(sqlConnection, deleteRows, null))
                    {
                        deleteCommand.ExecuteNonQuery();
                    }
                }

                // Loop through each row of the input file and add it to the table.
                while (!csvReader.EndOfData)
                {
                    Dictionary<string, string> columnValues =
                        new Dictionary<string, string>(columnSizes.Count);

                    string[] columns = csvReader.ReadFields();

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

                        // TODO: Sqlite doesn't enforce length restrictions so this could be deleted
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
                            messages.Add(ReplaceInsert(settings, sqlConnection, columnValues, tx));
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
                        message.AppendFormat(CultureInfo.CurrentCulture,
                            "*Failed to import row \"{0}\" with the following error: {1}",
                            String.Join(",", columns),
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
                var uex = new ExtractException("ELI40310", "Import operation failed.", ex);
                if (null != tx)
                {
                    try
                    {
                        tx.Rollback();
                    }
                    catch (Exception x)
                    {
                        uex = new ExtractException("ELI41651", "Failed rolling back transaction.", uex);
                        uex.AddDebugData("Error message", x.Message, false);
                    }
                }

                throw uex;
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
        /// Insert a row into the table, overwriting what was present.
        /// </summary>
        /// <param name="settings">import settings</param>
        /// <param name="connection">database connection, open</param>
        /// <param name="columnValues">a set of column values for the row being inserted</param>
        /// <param name="tx">transaction context</param>
        /// <returns>result string</returns>
        static string ReplaceInsert(ImportSettings settings,
                                    DbConnection connection,
                                    Dictionary<string, string> columnValues,
                                    DbTransaction tx)
        {
            using (DbCommand command = DBMethods.CreateDBCommand(connection, settings.CommandText, columnValues))
            {
                if (settings.UseTransaction)
                {
                    command.Transaction = tx;
                }
                command.ExecuteNonQuery();

                StringBuilder sb = new StringBuilder();
                sb.AppendFormat(CultureInfo.CurrentCulture, "Succeeded: {0}", command.ToString());

                return sb.ToString();
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
                sb.AppendFormat(CultureInfo.CurrentCulture, "Succeeded: {0}", command.ToString());

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
                                            SqliteColumnCollection columnInfo, 
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
    }
}
