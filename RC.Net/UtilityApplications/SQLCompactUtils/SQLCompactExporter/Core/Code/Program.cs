using Extract;
using Extract.Database;
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
        /// Exports a table in an SQL compact DB into a text file
        /// </summary>
        static void Main(string[] args)
        {
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
            }
            catch (Exception ex)
            {
                ExtractException.Log("ELI27126", ex);
                Console.WriteLine(ex.Message);
                PrintUsage();
                return;
            }

            ExportSettings settings = new ExportSettings(args);

            try
            {
                string result = ExportTable.ExportToFile(settings);

                string[] results = result.Split(new string[] {settings.RowDelimiter}, 
                                                StringSplitOptions.RemoveEmptyEntries);

                int rowsExported = results.Length;

                Console.WriteLine("Exported " +
                                  rowsExported.ToString(CultureInfo.CurrentCulture) + " rows.");
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
                Console.WriteLine("EscapeForRegEx: " + (settings.EscapeForRegex ? "Yes" : "No"));
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
            Console.WriteLine("[/fs <FileSuffix>] [/enc <CodePageName>] [/esc]");
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
            Console.WriteLine("/enc <CodePageName>: The name of the codepage used to encode the " +
                "text output. \r\nFor example \"ascii\", \"unicode\", \"utf-8\", or any named " +
                "code page. If not specified, the default ANSI code page will be used.");
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
    }
}
