using Extract.Encryption;
using Extract.Licensing;
using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Text;

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
        /// The name of the ID Shield printer.
        /// </summary>
        static readonly string _ID_SHIELD_PRINTER = "CutePDF";

        /// <summary>
        /// Map code passed to the proper converter executable (this is used
        /// rather than a license check internal to the helper executable since
        /// the license is validated in this executable).
        /// </summary>
        static readonly string _internalMapCode = 
            ExtractEncryption.EncryptString(LicenseUtilities.GetMapLabelValue(new MapLabel()),
            new MapLabel());

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            string exceptionFile = null;
            try
            {
                if (args.Length < 1 || args.Length > 3)
                {
                    ShowUsage("Incorrect number of arguments.");
                    return;
                }

                string fileName = null;
                for (int i = 0; i < args.Length; i++)
                {
                    string temp = args[i];
                    if (temp.Equals("/?", StringComparison.OrdinalIgnoreCase))
                    {
                        ShowUsage();
                        return;
                    }
                    else if (temp.Equals("/ef", StringComparison.OrdinalIgnoreCase))
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
                    else if (temp.Contains("/"))
                    {
                        ShowUsage("Invalid command line option - " + temp);
                        return;
                    }
                    else
                    {
                        // Assume this is the file name
                        fileName = Path.GetFullPath(temp);
                    }
                }

                //// Ensure the file exists
                //ExtractException.Assert("ELI30271", "The specified file does not exist.",
                //    File.Exists(fileName), "File Name", fileName);

                // Load and validate the license
                LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects,
                    "ELI30280", "OfficeToTif.exe");


                // Find the ID Shield printer
                WindowsPrinterInformation idShield =
                    WindowsPrinterInformation.GetPrinterInformation(_ID_SHIELD_PRINTER);
                if (idShield == null)
                {
                    throw new ExtractException("ELI30272",
                        "The ID Shield printer is not installed.");
                }

                // Determine the office application that should be used to process
                // the document.
                OfficeApplication application = GetOfficeApplicationFromFilename(fileName);

                // Get the appropriate printer name for the office application
                string printerName = application == OfficeApplication.Excel ?
                    idShield.PrinterNameWithPortInformation() : idShield.Name;

                // Determine the version of Word installed (we will assume
                // that the current version of word is also the current
                // version of office that is installed).
                int version = RegistryManager.GetWordVersion();

                using (TemporaryFile tempUex = new TemporaryFile("uex"),
                    tempArgs = new TemporaryFile())
                using (Process process = new Process())
                {
                    // Build the arguments for the process
                    StringBuilder arguments = new StringBuilder();
                    arguments.AppendLine(fileName);
                    arguments.AppendLine(application.ToString("d"));
                    arguments.AppendLine(printerName);
                    arguments.AppendLine(tempUex.FileName);
                    arguments.AppendLine(_internalMapCode);
                    File.WriteAllText(tempArgs.FileName, arguments.ToString());

                    process.StartInfo.Arguments = "\"" + tempArgs.FileName + "\"";
                    switch (version)
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
            usage.AppendLine(" </?>|<filename> [/ef <exceptionfile>]");
            usage.AppendLine();
            usage.AppendLine("Options:");
            usage.AppendLine();
            usage.AppendLine("    /? - Display help");
            usage.AppendLine("    filename - The name of the file to convert to a TIF");
            usage.AppendLine("    /ef <exceptionfile> - Log any exceptions to the specified");
            usage.AppendLine("          exception file rather than display them.");

            // Display the usage as an error or as an information box
            MessageBox.Show(usage.ToString(), isError ? "Error" : "Usage", MessageBoxButtons.OK,
                isError ? MessageBoxIcon.Error : MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1, 0);
        }
    }
}