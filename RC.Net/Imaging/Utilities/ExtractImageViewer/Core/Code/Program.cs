using Extract.Licensing;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
                if (args.Length > 1)
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

                // Run the image viewer, opening the image file if specified
                // Load the licenses
                LicenseUtilities.LoadLicenseFilesFromFolder(0, new Extract.Licensing.MapLabel());

                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects,
                    "ELI21841", "Extract Image Viewer");

                Application.Run(new ExtractImageViewerForm(args.Length == 1 ? args[0] : null));
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
            usage.AppendLine();
            usage.AppendLine("<filename> - The image file to open");

            // Display the usage as an error or as an information box
            MessageBox.Show(usage.ToString(), isError ? "Error" : "Usage", MessageBoxButtons.OK,
                isError ? MessageBoxIcon.Error : MessageBoxIcon.Information);
        }
    }
}