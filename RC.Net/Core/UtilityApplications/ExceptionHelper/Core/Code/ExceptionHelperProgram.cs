using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Extract.ExceptionHelper
{
    static class ExceptionHelperProgram
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                if (args.Length < 1 || args.Length > 2)
                {
                    ShowUsage("Incorrect number of arguments.");
                    return;
                }
                else if (args[0].Equals("/?", StringComparison.OrdinalIgnoreCase))
                {
                    ShowUsage();
                    return;
                }
                else if (args[0].Contains("/"))
                {
                    ShowUsage("Invalid command line argument - " + args[0]);
                    return;
                }

                // Check for display flag
                bool display = false;
                if (args.Length == 2)
                {
                    if (!args[1].Equals("/d", StringComparison.OrdinalIgnoreCase))
                    {
                        ShowUsage("Invalid command line argument = " + args[1]);
                        return;
                    }
                    display = true;
                }

                // Read the text from the file
                string fileName = Path.GetFullPath(args[0]);
                string hexException = File.ReadAllText(fileName);

                // Wrap the exception as an extract exception
                ExtractException ee = ExtractException.AsExtractException("ELI30288",
                    ExceptionHelperMethods.DeserializeExceptionFromHexString(hexException));

                // Display or log based on command line arguments
                if (display)
                {
                    ee.Display();
                }
                else
                {
                    ee.Log();
                }
            }
            catch (Exception ex)
            {
                ExtractException.Log("ELI30289", ex);
            }
        }

        /// <overloads>Displays the usage message.</overloads>
        /// <summary>
        /// Displays the usage message.
        /// </summary>
        static void ShowUsage()
        {
            ShowUsage(null);
        }

        /// <summary>
        /// Displays the usage message prepended with the specified error message.
        /// </summary>
        /// <param name="errorMessage">The error message to display before the usage message.</param>
        static void ShowUsage(string errorMessage)
        {
            // Check if there is an error message
            bool isError = !string.IsNullOrEmpty(errorMessage);

            // Initialize the string builder with the error message if specified
            StringBuilder usage = new StringBuilder(isError ? errorMessage : "");
            if (isError)
            {
                usage.AppendLine();
                usage.AppendLine();
            }

            // Add the command line syntax
            usage.Append(Environment.GetCommandLineArgs()[0]);
            usage.AppendLine(" </?>|<exceptionfile> [/d]");
            usage.AppendLine();
            usage.AppendLine("Options:");
            usage.AppendLine();
            usage.AppendLine("    /? - Display help");
            usage.AppendLine("    exceptionfile - The name of the file containing");
            usage.AppendLine("        the serialized exception");
            usage.AppendLine("    /d - Display the exception rather than logging it.");

            // Display the usage as an error or as an information box
            MessageBox.Show(usage.ToString(), isError ? "Error" : "Usage", MessageBoxButtons.OK,
                isError ? MessageBoxIcon.Error : MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1, 0);
        }
    }
}