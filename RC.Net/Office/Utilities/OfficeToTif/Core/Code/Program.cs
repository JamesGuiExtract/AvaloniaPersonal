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
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                string fileName = Path.GetFullPath(args[0]);
                ExtractException.Assert("ELI30271", "The specified file does not exist.",
                    File.Exists(fileName), "File Name", fileName);

                // Find the ID Shield printer
                WindowsPrinterInformation idShield =
                    WindowsPrinterInformation.GetPrinterInformation("ID Shield");
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

                using (TemporaryFile tempFile = new TemporaryFile("uex"))
                using (Process process = new Process())
                {
                    // Build the arguments for the process
                    StringBuilder arguments = new StringBuilder();
                    arguments.Append('"');
                    arguments.Append(fileName);
                    arguments.Append('"');
                    arguments.Append(" ");
                    arguments.Append(application.ToString("d"));
                    arguments.Append(" ");
                    arguments.Append('"');
                    arguments.Append(printerName);
                    arguments.Append('"');
                    arguments.Append(" ");
                    arguments.Append('"');
                    arguments.Append(tempFile.FileName);
                    arguments.Append('"');

                    process.StartInfo.Arguments = arguments.ToString();
                    switch (version)
                    {
                        case 12:
                            process.StartInfo.FileName = "Office2007.exe";
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
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI30273", ex);
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
    }
}