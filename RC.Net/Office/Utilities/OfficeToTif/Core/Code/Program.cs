using Extract.Licensing;
using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Extract.Office.Utilities.OfficeToTif
{
    static class Program
    {
        /// <summary>
        /// The directory that this executable is running from.
        /// </summary>
        static readonly string _APPLICATION_PATH = Path.GetDirectoryName(Application.ExecutablePath);

        /// <summary>
        /// The path to the office 2007 tif converter.
        /// </summary>
        static readonly string _OFFICE_2007_CONVERTER = Path.Combine(_APPLICATION_PATH,
            "Office2007ToTif.exe");

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            string exceptionFile = null;
            try
            {
                if (args.Length == 1)
                {
                    ShowUsage(
                        args[0].Equals("/?", StringComparison.OrdinalIgnoreCase) ?
                        string.Empty : "Invalid command line option - " + args[0]);
                    return;
                }
                else if (args.Length < 1 || args.Length > 4)
                {
                    ShowUsage("Incorrect number of arguments.");
                    return;
                }

                string sourceDocument = Path.GetFullPath(args[0]);
                string destinationDocument = Path.GetFullPath(args[1]);
                for (int i = 2; i < args.Length; i++)
                {
                    string temp = args[i];
                    if (temp.Equals("/ef", StringComparison.OrdinalIgnoreCase))
                    {
                        i++;
                        if (i < args.Length)
                        {
                            exceptionFile = Path.GetFullPath(args[i]);
                        }
                        else
                        {
                            ShowUsage("/ef requires a file be specified.");
                            return;
                        }
                    }
                    else
                    {
                        ShowUsage("Invalid command line option - " + temp);
                        return;
                    }
                }

                // Ensure the file exists
                ExtractException.Assert("ELI30271", "The specified source file does not exist.",
                    File.Exists(sourceDocument), "File Name", sourceDocument);

                // Load and validate the license
                LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects,
                    "ELI30280", Path.GetFileNameWithoutExtension(Application.ExecutablePath));

                // Determine the office application that should be used to process
                // the document.
                OfficeApplication application = GetOfficeApplicationFromFilename(sourceDocument);

                using (TemporaryFile tempUex = new TemporaryFile("uex"),
                    tempArgs = new TemporaryFile(),
                    tempDocument = new TemporaryFile(".tif"))
                using (Process process = new Process())
                {
                    // Build the arguments for the process
                    StringBuilder arguments = new StringBuilder();
                    arguments.AppendLine(sourceDocument);
                    arguments.AppendLine(tempDocument.FileName);
                    arguments.AppendLine(application.ToString("d"));
                    arguments.AppendLine(tempUex.FileName);
                    File.WriteAllText(tempArgs.FileName, arguments.ToString());

                    process.StartInfo.Arguments = "\"" + tempArgs.FileName + "\"";
                    switch (OfficeMethods.CheckOfficeVersion())
                    {
                        case 12:
                            process.StartInfo.FileName = _OFFICE_2007_CONVERTER;
                            break;

                        case 13:
                            // Perform office to Tif conversion with office 2010
                            break;

                        default:
                            // This is an unsupported version or office was not installed
                            break;
                    }
                    process.Start();
                    process.WaitForExit();

                    // Check for exception in the temp file
                    FileInfo info = new FileInfo(tempUex.FileName);
                    if (info.Length > 0)
                    {
                        // Load the exception from the temp file and throw it.
                        ExtractException ee = ExtractException.LoadFromFile("ELI30278",
                            tempUex.FileName);
                        throw ee;
                    }

                    // Ensure the destination directory exists
                    string destDir = Path.GetDirectoryName(destinationDocument);
                    if (!Directory.Exists(destDir))
                    {
                        Directory.CreateDirectory(destDir);
                    }

                    // Copy the file to the destination
                    File.Copy(tempDocument.FileName, destinationDocument, true);
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI30273", ex);
                if (!string.IsNullOrEmpty(exceptionFile))
                {
                    ee.Log(exceptionFile);
                }
                else
                {
                    ee.Display();
                }
            }
        }

        /// <summary>
        /// Attempts to determine the office application to use to print the file
        /// based on the file extension.
        /// </summary>
        /// <param name="fileName">The name of the file to be processed.</param>
        /// <returns>The office application to use to open the file.</returns>
        static OfficeApplication GetOfficeApplicationFromFilename(string fileName)
        {
            string extension = Path.GetExtension(fileName).ToUpperInvariant();

            if (extension.Contains("DOC") || extension.Contains("TXT")
                || extension.Contains("RTF") || extension.Contains("ODT")
                || extension.Contains("WPS"))
            {
                return OfficeApplication.Word;
            }
            else if (extension.Contains("XLS") || extension.Contains("CSV") ||
                extension.Contains("ODS") || extension.Contains("PRN"))
            {
                return OfficeApplication.Excel;
            }
            else if (extension.Contains("PPT") || extension.Contains("PPS")
                || extension.Contains("ODP"))
            {
                return OfficeApplication.PowerPoint;
            }
            else
            {
                return OfficeApplication.Unknown;
            }
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
            usage.AppendLine(" </?>|<source> <destination> [/ef <exceptionfile>]");
            usage.AppendLine();
            usage.AppendLine("Options:");
            usage.AppendLine();
            usage.AppendLine("    /? - Display help");
            usage.AppendLine("    source - The name of the file to convert to a TIF");
            usage.AppendLine("    destination - The destination for the converted TIF");
            usage.AppendLine("    /ef <exceptionfile> - Log any exceptions to the specified");
            usage.AppendLine("          exception file rather than display them.");

            // Display the usage as an error or as an information box
            MessageBox.Show(usage.ToString(), isError ? "Error" : "Usage", MessageBoxButtons.OK,
                isError ? MessageBoxIcon.Error : MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1, 0);
        }
    }
}