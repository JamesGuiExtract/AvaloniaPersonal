using Extract.Licensing;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Extract.Imaging.Utilities.ExtractImageViewer
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
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                // Check for a valid number of arguments
                if (args.Length > 3)
                {
                    ShowUsage("Too many arguments.");
                    return;
                }

                // Check for usage message
                if (args.Length == 1 && args[0] == @"/?")
                {
                    ShowUsage();
                    return;
                }

                string fileToOpen = null;
                //string scriptFile = null;
                string ocrTextFile = null;
                bool sendOcrToClipboard = false;
                //bool reuseAlreadyOpenImageViewer = false;
                bool showSearchWindow = false;
                for (int i=0; i < args.Length; i++)
                {
                    string argument = args[i];
                    if (argument.Equals("/r", StringComparison.OrdinalIgnoreCase))
                    {
                        RegisterTifFileAssociation();
                    }
                    else if (argument.Equals("/u", StringComparison.OrdinalIgnoreCase))
                    {
                        UnregisterTifFileAssociation();
                    }
                    else if (argument.Equals("/o", StringComparison.OrdinalIgnoreCase))
                    {
                        if (sendOcrToClipboard)
                        {
                            ShowUsage("Cannot specify both /o and /c options.");
                            return;
                        }

                        // Check for file name
                        i++;
                        if (i < args.Length)
                        {
                            ocrTextFile = Path.GetFullPath(args[i]);
                        }
                        else
                        {
                            ShowUsage("Must specify a file name with /o option.");
                            return;
                        }
                    }
                    else if (argument.Equals("/c", StringComparison.OrdinalIgnoreCase))
                    {
                        if (!string.IsNullOrEmpty(ocrTextFile))
                        {
                            ShowUsage("Cannot specify both /c and /o options.");
                            return;
                        }

                        sendOcrToClipboard = true;
                    }
                    else if (argument.Equals("/s", StringComparison.OrdinalIgnoreCase))
                    {
                        showSearchWindow = true;
                    }
                    else if (argument.Equals("/l", StringComparison.OrdinalIgnoreCase))
                    {
                        // TODO: Send all argument values to existing ExtractImageViewer
                    }
                    else if (argument.Equals("/closeall", StringComparison.OrdinalIgnoreCase))
                    {
                        // Close all open image viewers
                        CloseAllOpenImageViewers();
                    }
                    else if (argument.Equals("/e", StringComparison.OrdinalIgnoreCase))
                    {
                    }
                    else if (argument.Equals("/script?", StringComparison.OrdinalIgnoreCase))
                    {
                    }
                    else if (argument.Equals("/ctrlid?", StringComparison.OrdinalIgnoreCase))
                    {
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(fileToOpen))
                        {
                            ShowUsage("Cannot specify more than one file to open.");
                            return;
                        }

                        // Assume the argument is the file to open
                        // Get the absolute path to the file
                        fileToOpen = Path.GetFullPath(args[i]);
                    }
                }

                // Run the image viewer, opening the image file if specified
                // Load the licenses
                LicenseUtilities.LoadLicenseFilesFromFolder(0, new Extract.Licensing.MapLabel());

                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects,
                    "ELI21841", "Extract Image Viewer");

                Application.Run(new ExtractImageViewerForm(fileToOpen, ocrTextFile,
                    sendOcrToClipboard, showSearchWindow));
            }
            catch (Exception ex)
            {
                ExtractException ee = new ExtractException("ELI21972",
                    "Failed while opening Extract Image Viewer!", ex);
                ee.Display();
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
        // This code will not be localized for a culture that uses right to left reading order.
        [SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions")]
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
            usage.AppendLine(" [options] [<filename>]");
            usage.AppendLine();
            usage.AppendLine("Options:");
            usage.AppendLine();
            usage.AppendLine("    /? - Display usage information");
            usage.AppendLine("    /r - Register the .tif file extension such that .tif files");
            usage.AppendLine("         open by default with the Image Viewer");
            usage.AppendLine("    /u - Unregister the .tif file extension such that .tif files");
            usage.AppendLine("         do NOT open by default with the Image Viewer");
            usage.AppendLine("    /o <ocrfile> - Text that is OCRed in the Image Viewer will be");
            usage.AppendLine("         written to <ocrfile> rather than displayed in a message box");
            usage.AppendLine("    /c - Text that is OCRed in the Image Viewer will be copied to the");
            usage.AppendLine("         clipboard rather than displayed in a message box");
            usage.AppendLine("    /s - Display the search window");
            usage.AppendLine("    /l - Reuse a current Image Viewer if one already exists");
            usage.AppendLine("    /closeall - Close any currently open Image Viewer");
            usage.AppendLine("    /e <scriptfile> - Execute script commands specified in <scriptfile>");
            usage.AppendLine("    /script? - Displays help for script commands");
            usage.AppendLine("    /ctrlid? - Lists the toolbar control ID numbers");
            usage.AppendLine();
            usage.AppendLine("<filename> - The image file to open");

            // Display the usage as an error or as an information box
            MessageBox.Show(usage.ToString(), isError ? "Error" : "Usage", MessageBoxButtons.OK,
                isError ? MessageBoxIcon.Error : MessageBoxIcon.Information);
        }

        /// <summary>
        /// 
        /// </summary>
        static void UnregisterTifFileAssociation()
        {
            // TODO: Implement this method
        }

        /// <summary>
        /// 
        /// </summary>
        static void RegisterTifFileAssociation()
        {
            // TODO: Implement this method
        } 

        /// <summary>
        /// 
        /// </summary>
        static void CloseAllOpenImageViewers()
        {
            // TODO: Implement this method
        }
    }
}