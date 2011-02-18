using Extract.Database;
using Extract.Licensing;
using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlServerCe;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Extract.SetOperations
{
    /// <summary>
    /// Enumeration for possible set operations
    /// </summary>
    enum SetOperation
    {
        /// <summary>
        /// Union operation of sets: A ⋃ B
        /// </summary>
        Union = 0,

        /// <summary>
        /// Intersection operation of sets: A ⋂ B
        /// </summary>
        Intersect = 1,

        /// <summary>
        /// Complement operation of sets: A ~ B
        /// </summary>
        Complement = 2
    }

    static class SetOperationsProgram
    {
        /// <summary>
        /// Constant for the first table name in the temporary database.
        /// </summary>
        const string _TABLE_A = "Table_A";

        /// <summary>
        /// Constant for the second table name in the temporary database.
        /// </summary>
        const string _TABLE_B = "Table_B";

        /// <summary>
        /// Constant for the result table name in the temporary database.
        /// </summary>
        const string _TABLE_R = "Table_Result";

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            string exceptionLogFile = null;
            TemporaryFile tempDb = null;
            try
            {
                // Validate argument count
                var argCount = args.Length;
                if (argCount < 4 || argCount > 8)
                {
                    if (argCount == 1 && args[0].Equals("/?", StringComparison.Ordinal))
                    {
                        DisplayUsage();
                        return;
                    }
                    else
                    {
                        DisplayUsage("Invalid command line: Incorrect number of arguments");
                        return;
                    }
                }

                // Get the input and output file names
                var fileA = Path.GetFullPath(args[0]);
                var fileB = Path.GetFullPath(args[2]);
                var outputFile = Path.GetFullPath(args[3]);

                // Get the operation
                SetOperation operation;
                if (!Enum.TryParse<SetOperation>(args[1], true, out operation))
                {
                    DisplayUsage("Invalid set operation specified: " + args[1]);
                    return;
                }

                // Get remaining arguments
                bool ignoreCase = true;
                bool ignoreDuplicates = false;
                for (int i = 4; i < argCount; i++)
                {
                    var temp = args[i];
                    if (temp.Equals("/c", StringComparison.OrdinalIgnoreCase))
                    {
                        ignoreCase = false;
                    }
                    else if (temp.Equals("/i", StringComparison.OrdinalIgnoreCase))
                    {
                        ignoreDuplicates = true;
                    }
                    else if (temp.Equals("/ef", StringComparison.OrdinalIgnoreCase))
                    {
                        // Requires a file specified as next argument
                        if ((++i) >= argCount)
                        {
                            DisplayUsage("/ef option requires a file name specified as next argument.");
                            return;
                        }
                        exceptionLogFile = Path.GetFullPath(args[i]);
                    }
                    else if (temp.Equals("/?", StringComparison.OrdinalIgnoreCase))
                    {
                        DisplayUsage();
                        return;
                    }
                    else
                    {
                        DisplayUsage("Invalid argument: " + temp);
                        return;
                    }
                }

                // Load and validate license
                LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects, "ELI31625",
                    "SetOperations.exe");

                // Load input files into temporary database
                tempDb = LoadTemporaryDb(fileA, fileB, ignoreCase, ignoreDuplicates);

                // Perform the specified operation and write out result file
                PerformOperation(tempDb, operation, outputFile);
            }
            catch (Exception ex)
            {
                if (!string.IsNullOrWhiteSpace(exceptionLogFile))
                {
                    ex.ExtractLog("ELI31626", exceptionLogFile);
                }
                else
                {
                    ex.ExtractDisplay("ELI31627");
                }
            }
            finally
            {
                if (tempDb != null)
                {
                    tempDb.Dispose();
                }
            }
        }

        /// <summary>
        /// Loads the temporary db.
        /// </summary>
        /// <param name="fileA">The file A.</param>
        /// <param name="fileB">The file B.</param>
        /// <param name="ignoreCase">if set to <see langword="true"/> [ignore case].</param>
        /// <param name="ignoreDuplicates">if set to <see langword="true"/> [ignore duplicates].</param>
        /// <returns></returns>
        static TemporaryFile LoadTemporaryDb(string fileA, string fileB,
            bool ignoreCase, bool ignoreDuplicates)
        {
            TemporaryFile tempFile = null;
            try
            {
                const string valueParameter = "@valueParameter";

                tempFile = new TemporaryFile(".sdf");
                var connectionString = SqlCompactMethods.BuildDBConnectionString(
                    tempFile.FileName);
                using (var engine = new SqlCeEngine(connectionString))
                {
                    // Since the temp file generation creates a file, it must
                    // be deleted before creating the temp database
                    File.Delete(tempFile.FileName);
                    engine.CreateDatabase();
                    if (!ignoreCase)
                    {
                        engine.Compact("DataSource=; Case Sensitive=True;");
                    }
                }

                // Open connection to the new database
                using (var connection = new SqlCeConnection(connectionString))
                {
                    if (connection.State == ConnectionState.Closed)
                    {
                        connection.Open();
                    }

                    // Fill list of tables and their corresponding input files.
                    var tables = new Dictionary<string, string>();
                    tables[_TABLE_A] = fileA;
                    tables[_TABLE_B] = fileB;
                    tables[_TABLE_R] = string.Empty;

                    // For each table in the collection:
                    // 1. Create it
                    // 2. Create index on it
                    // 3. If there is a corresponding file, load the table with it.
                    foreach (var table in tables)
                    {
                        var duplicates = new HashSet<string>();
                        var tableName = table.Key;
                        var fileName = table.Value;
                        string query = "CREATE TABLE " + tableName
                            + " (DataValue nvarchar(260))";
                        string index = "CREATE NONCLUSTERED INDEX IX_"
                            + tableName + " ON " + tableName + " (DataValue)";
                        string insert = "INSERT INTO " + tableName
                            + " SELECT " + valueParameter +
                            " WHERE NOT EXISTS (SELECT [DataValue] FROM "
                            + tableName + " WHERE [DataValue] = "
                            + valueParameter + ")";
                        using (var command = new SqlCeCommand(query, connection))
                        {
                            try
                            {
                                command.ExecuteNonQuery();
                                command.CommandText = index;
                                command.ExecuteNonQuery();
                                command.CommandText = insert;
                                if (string.IsNullOrWhiteSpace(fileName))
                                {
                                    continue;
                                }

                                command.Parameters.Add(valueParameter, "").DbType = DbType.String;
                                foreach (var line in File.ReadLines(fileName))
                                {
                                    // Ignore blank lines
                                    if (!string.IsNullOrWhiteSpace(line))
                                    {
                                        // If the command does not update a row
                                        // then there is a duplicate, add to the list.
                                        command.Parameters[valueParameter].Value = line;
                                        if (command.ExecuteNonQuery() == 0)
                                        {
                                            duplicates.Add(line);
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                var ee = ex.AsExtract("ELI31684");
                                ee.AddDebugData("Current Query", command.CommandText, true);
                                throw ee;
                            }
                        }

                        // If there were duplicates, write the file name and duplicates to the
                        // console. If not ignoring duplicates then throw an exception
                        if (duplicates.Count > 0)
                        {
                            Console.WriteLine("Duplicates in file:");
                            Console.WriteLine(fileName);
                            Console.WriteLine("List of duplicates:");
                            foreach (var duplicate in duplicates)
                            {
                                Console.WriteLine(duplicate);
                            }

                            if (!ignoreDuplicates)
                            {
                                var ee = new ExtractException("ELI31628",
                                    "There were duplicate items in the specified list");
                                ee.AddDebugData("File With Duplicates", fileName, false);
                                throw ee;
                            }
                        }
                    }

                    // Close the connection
                    connection.Close();
                }

                return tempFile;
            }
            catch
            {
                if (tempFile != null)
                {
                    tempFile.Dispose();
                }

                throw;
            }
        }

        /// <summary>
        /// Performs the operation.
        /// </summary>
        /// <param name="tempDb">The temp db.</param>
        /// <param name="operation">The operation.</param>
        /// <param name="outputFile">The output file.</param>
        static void PerformOperation(TemporaryFile tempDb,
            SetOperation operation, string outputFile)
        {
            using (var connection = new SqlCeConnection(
                SqlCompactMethods.BuildDBConnectionString(tempDb.FileName)))
            {
                if (connection.State == ConnectionState.Closed)
                {
                    connection.Open();
                }

                // Get the set operation query to execute
                var query = BuildSetOperationQuery(operation);
                using (var command = new SqlCeCommand(query, connection))
                {
                    command.ExecuteNonQuery();
                }

                // Output the updated list
                using (var output = new StreamWriter(outputFile))
                using (var command = new SqlCeCommand("SELECT [DataValue] FROM " + _TABLE_R, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        output.WriteLine(reader.GetString(0));
                    }

                    // Ensure all data is written to the file
                    output.Flush();
                }

                connection.Close();
            }
        }

        /// <summary>
        /// Builds the set operation query.
        /// </summary>
        /// <param name="operation">The operation.</param>
        /// <returns></returns>
        private static string BuildSetOperationQuery(SetOperation operation)
        {
            var query = new StringBuilder("INSERT INTO ", 1024);
            query.Append(_TABLE_R);
            query.Append(" ([DataValue]) ( ");
            switch (operation)
            {
                case SetOperation.Union:
                    {
                        query.Append("SELECT [DataValue] FROM ");
                        query.Append(_TABLE_A);
                        query.Append(" UNION SELECT [DataValue] FROM ");
                        query.Append(_TABLE_B);
                    }
                    break;

                case SetOperation.Intersect:
                case SetOperation.Complement:
                    {
                        query.Append("SELECT ");
                        query.Append(_TABLE_A);
                        query.Append(".[DataValue] FROM ");
                        query.Append(_TABLE_A);
                        query.Append(
                            operation == SetOperation.Intersect ? " INNER " : " LEFT ");
                        query.Append(" JOIN ");
                        query.Append(_TABLE_B);
                        query.Append(" ON ");
                        query.Append(_TABLE_A);
                        query.Append(".[DataValue] = ");
                        query.Append(_TABLE_B);
                        query.Append(".[DataValue] ");
                        if (operation == SetOperation.Complement)
                        {
                            query.Append("WHERE ");
                            query.Append(_TABLE_B);
                            query.Append(".[DataValue] IS NULL ");
                        }
                    }
                    break;

                default:
                    ExtractException.ThrowLogicException("ELI31629");
                    break;
            }
            query.Append(" )");

            return query.ToString();
        }

        /// <summary>
        /// Displays the usage.
        /// </summary>
        /// <param name="error">The error.</param>
        static void DisplayUsage(string error = null)
        {
            var sb = new StringBuilder(1024);
            if (!string.IsNullOrWhiteSpace(error))
            {
                sb.AppendLine(error);
            }
            sb.AppendLine("Usage:");
            sb.AppendLine("--------------");
            sb.Append("SetOperations.exe <FileA> <Operation> <FileB> <OutputFile> ");
            sb.AppendLine("[/c] [/i] [/ef <LogFileName>]");
            sb.AppendLine();
            sb.AppendLine("FileA: The first list file");
            sb.AppendLine("Operation: union, intersection, or complement");
            sb.AppendLine("    Union: FileA ⋃ FileB => All elements from A and B (no duplicates)");
            sb.AppendLine("    Intersection: FileA ⋂ FileB => All elements that are in both A and B");
            sb.AppendLine("    Complement: FileA ~ FileB => All elements in A that are not in B");
            sb.AppendLine("FileB: The second list file");
            sb.AppendLine("OutputFile: The file that will contain the result of the list operation");
            sb.AppendLine("/c: Perform a case sensitive list comparison (default is case insensitive)");
            sb.AppendLine("/i: Ignore duplicates if a list contains duplicates (will still print duplicates to console)");
            sb.AppendLine("/ef LogFileName: Logs exceptions to the specified log file rather than displaying them.");
            sb.AppendLine();
            sb.AppendLine("Example:");
            sb.AppendLine("--------------");
            sb.AppendLine("SetOperations.exe \"C:\\testing\\lista.txt\" union \"C:\\testing\\listb.txt\" /c /i");
            sb.AppendLine();

            MessageBox.Show(sb.ToString(), "Usage", MessageBoxButtons.OK,
                string.IsNullOrWhiteSpace(error) ? MessageBoxIcon.Information : MessageBoxIcon.Error,
                MessageBoxDefaultButton.Button1, 0);
        }
    }
}
