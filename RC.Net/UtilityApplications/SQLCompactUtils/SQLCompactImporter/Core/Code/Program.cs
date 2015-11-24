using Extract;
using Extract.Database;
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
        /// Populates a table in an SQL compact DB using the data in a text file.
        /// </summary>
        static void Main(string[] args)
        {
            ImportSettings settings = null;

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
                settings = new ImportSettings(args);
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
                var result = ImportTable.ImportFromFile(settings);
                int rowsProcessed = result.Item2.Length;
                int rowsFailed = result.Item1;

                int rowsAdded = rowsProcessed - rowsFailed;
                Console.WriteLine("Added " + rowsAdded.ToString(CultureInfo.CurrentCulture) + " rows.");
                if (rowsFailed > 0)
                {
                    Console.WriteLine("Failed to import " +
                        rowsFailed.ToString(CultureInfo.CurrentCulture) + " rows.");

                    int counter = 1;
                    foreach (var line in result.Item2)
                    {
                        if (line.StartsWith("*", StringComparison.OrdinalIgnoreCase))
                        {
                            Console.WriteLine("{0}: {1}", counter, line);
                            ++counter;
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

    }
}
