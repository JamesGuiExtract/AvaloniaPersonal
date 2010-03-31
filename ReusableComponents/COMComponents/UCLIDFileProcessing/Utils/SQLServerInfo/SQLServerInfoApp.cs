using CSharpDatabaseUtilities;
using Extract;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;

namespace SQLServerInfo
{
    class SQLServerInfoApp
    {
        #region Constants

        /// <summary>
        /// string constant with the usage
        /// </summary>
        private const string strUsage =
            "USAGE:\r\n" +
            " SQLServerInfo [/?] | [/s <ServerFileName>] | [/d <ServerName> <DatabaseFileName>] [/ef <ExceptionFileName>]\r\n" +
                    "\t/s <ServerFileName>\r\n" +
                        "\t\t- Puts a list of SQL server names in <ServerFileName>.\r\n" +
                    "\t/d <ServerName> <DatabasesFileName>\r\n" +
                        "\t\t- Puts a list of FAM databases on <ServerName> in <DatabaseFileName>." +
                    "\t/ef <ExceptionFileName>\r\n" +
                        "\t\t- If there is an exception it is saved to <ExceptionFileName>.\r\n" +
                    "\t/? - Displays this help message.";

        #endregion Constants

        #region Methods

        /// <summary>
        /// Displays usage string
        /// </summary>
        private static void Usage()
        {
            // Display the usage string
            Console.WriteLine(strUsage);
        }

        /// <summary>
        /// Outputs the list to the output file. 
        /// </summary>
        /// <param name="listOutput">
        ///     The list to output
        /// </param>
        /// <param name="strOutputFile">
        ///     The name of the file to save the list to.
        /// </param>
        private static void OutputToFile(IEnumerable<string> listOutput, string strOutputFile)
        {
            // Open the stream writer to save the list to
            using (StreamWriter sw = new StreamWriter(strOutputFile, false))
            {
                // Save each string on a line in the output file
                foreach (string s in listOutput)
                {
                    sw.WriteLine(s);
                }
            }
        }
       
        /// <summary>
        /// Finds the option in the argument list
        /// </summary>
        /// <param name="strOption">
        ///     The option to find
        /// </param>
        /// <param name="args">
        ///     The argument list passed to main
        /// </param>
        /// <returns>
        ///     If option found the index is returned else returns -1
        /// </returns>
        private static int FindOption(string strOption, string[] args)
        {
            // Search the arguments for the option
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == strOption)
                {
                    // Option found so return the index
                    return i;
                }
            }

            // Option not found so return -1
            return -1;
        }

        /// <summary>
        /// Gets the filename for the /ef option.
        /// </summary>
        /// <param name="args">
        ///     Argument list passed to main
        /// </param>
        /// <returns>
        ///     If /ef switch is found the filename that follows it is returned
        ///     else returns an empty string
        /// </returns>
        private static string GetExceptionFile(string[] args)
        {
            // Find the /ef option in the argument list
            int iOption = FindOption("/ef", args);

            // if the option was found get the filename
            if ( iOption >= 0 )
            {
                // Make sure there are enough arguments for the filename
                if (iOption + 1 < args.Length)
                {
                    // Return the filename
                    return args[iOption + 1];
                }
            }

            // Was not found so return empty string
            return "";
        }

        #endregion Methods

        /// <summary>
        /// The Main function for the application.
        /// </summary>
        /// <param name="args">An array of strings containing the command line arguments.</param>
        static void Main(string[] args)
        {
            // Set the strExceptionFilename to empty string
            string strExceptionFilename = "";

            try
            {
                // get the exception filename if it is there
                strExceptionFilename = GetExceptionFile(args);

                // There should be a minimum of 2 arguments and no more than 5
                if (args.Length < 2 || args.Length > 5)
                {
                    Usage();
                }
                else
                {
                    // Search for servers option
                    int iCurrOption = FindOption("/s", args);

                    // if Servers option found 
                    if (iCurrOption >= 0)
                    {
                        // Make sure there is a filename argument
                        if (iCurrOption + 1 < args.Length)
                        {
                            // Save the list of servers to the file
                            OutputToFile(SqlDatabaseMethods.GetSqlServerList(), args[iCurrOption + 1]);
                        }
                        else
                        {
                            Usage();
                        }
                    }
                    else
                    {
                        // Check for databases option
                        iCurrOption = FindOption("/d",args);

                        // if database option is selected
                        if (iCurrOption >= 0)
                        {
                            // Make sure there is a server name and a filename
                            if (iCurrOption + 2 < args.Length)
                            {
                                // Save the list of databases to the file
                                OutputToFile(SqlDatabaseMethods.GetDBNameList(args[iCurrOption + 1]), args[iCurrOption + 2]);
                            }
                            else
                            {
                                Usage();
                            }
                        }
                        else
                        {
                            Usage();
                        }
                    }
                 }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI19852", ex);

                // if the /ef switch was specified save the exception to that file
                if (!string.IsNullOrEmpty(strExceptionFilename))
                {
                    ee.Log(strExceptionFilename);
                }
                else
                {
                    // since the /ef switch was not specified display the exception
                    ee.Display();
                }
            }
        }
    }
}
