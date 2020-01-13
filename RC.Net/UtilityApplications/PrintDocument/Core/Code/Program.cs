using Extract.Imaging;
using Extract.Licensing;
using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using Extract.Imaging.Utilities;

namespace Extract.PrintDocument
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            string exceptionFile = "";
            string fileName = "";
            string printerName = "";
            bool printAnnotations = true;
            bool useInstalledPrinterList = true;

            try
            {
                // Load license files from folder
                LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());

                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects,
                    "ELI36005", "Print document utility");

                UnlockLeadtools.UnlockLeadToolsSupport();

                // Check the number of command line arguments
                if (args.Length < 1 || args[0] == "/?")
                {
                    ShowUsage();
                    return;
                }

                // Parse the command-line arguments.
                for (int i = 0; i < args.Length; i++)
                {
                    string arg = args[i];

                    // Check for log exceptions argument
                    if (i == 0)
                    {
                        // Get the fully qualified path to the file
                        fileName = Path.GetFullPath(arg);

                        // Ensure the file exists
                        ExtractException.Assert("ELI36004", "File does not exist!",
                            File.Exists(fileName), "File Name", fileName);
                    }
                    else if (arg.Equals("/a+", StringComparison.OrdinalIgnoreCase))
                    {
                        printAnnotations = true;
                    }
                    else if (arg.Equals("/a-", StringComparison.OrdinalIgnoreCase))
                    {
                        printAnnotations = false;
                    }
                    else if (arg.Equals("/x", StringComparison.OrdinalIgnoreCase))
                    {
                        useInstalledPrinterList = false;
                    }
                    else if (arg.Equals("/ef", StringComparison.OrdinalIgnoreCase))
                    {
                        i++;
                        if (i >= args.Length)
                        {
                            ShowUsage("Log filename expected.");
                            return;
                        }
                        
                        exceptionFile = Path.GetFullPath(args[i]);
                    }
                    else if (string.IsNullOrEmpty(printerName))
                    {
                        printerName = arg;
                    }
                    else
                    {
                        ShowUsage("Unrecognized option: \"" + arg + "\"");
                        return;
                    }
                }

                if (!useInstalledPrinterList && string.IsNullOrWhiteSpace(printerName))
                {
                    ShowUsage("/x option is not valid if a printer name has not been specified.");
                    return;
                }

                ImagePrinter.Print(fileName, printerName, useInstalledPrinterList, printAnnotations);
            }
            catch (Exception ex)
            {
                // Wrap the exception as an extract exception
                ExtractException ee = new ExtractException("ELI36006",
                    "Failed to print document.", ex);

                // Add the file name as debug data
                ee.AddDebugData("Filename", fileName, false);

                // If logExceptions is true, just log otherwise display the exception
                if (!string.IsNullOrEmpty(exceptionFile))
                {
                    // Log the exception
                    ee.Log(exceptionFile);
                }
                else
                {
                    // Display the exception to the user
                    ee.Display();
                }
            }
        }

        /// <summary>
        /// Displays a usage message to the user
        /// </summary>
        /// <param name="error">If specified, describes a problem with the specified command-line
        /// arguments.</param>
        static void ShowUsage(string error = null)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(error ?? "Usage:");
            sb.AppendLine("------------");
            sb.AppendLine("PrintDocument.exe <FileName> [PrinterName [/x]] [/a+|/a-] [/ef <ExceptionFile>]");
            sb.AppendLine("FileName: Name of file to print");
            sb.AppendLine("PrinterName: Name of the printer to use (default printer is used if not specified)");
            sb.AppendLine("/x: Perform extended search for printers on the network but not currently in the " +
                "installed printers list.");
            sb.AppendLine("/a+: Print image annotations (default if not specified)");
            sb.AppendLine("/a-: Do not print image annotations");
            sb.Append("/ef <ExceptionFile>: Log exceptions to the specified file rather than");
            sb.AppendLine(" display them");

            MessageBox.Show(sb.ToString(), "PrintDocument Usage", MessageBoxButtons.OK,
                string.IsNullOrEmpty(error) ? MessageBoxIcon.Information : MessageBoxIcon.Error,
                MessageBoxDefaultButton.Button1, 0);
        }
    }
}
