using Extract;
using Extract.Licensing;
using Extract.Utilities.Forms;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace IDShieldOffice
{
    static class Program
    {
        /// <summary>
        /// The license string for the SandDock manager
        /// </summary>
        private static readonly string _SANDOCK_LICENSE_STRING =
            @"1970|FmYPlfMJx5y4I4w2mqXzIDiFUPM=";

        /// <summary>
        /// The full path to the user license utility.
        /// </summary>
        static readonly string _USER_LICENSE_PATH = 
#if DEBUG
            Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), @"UserLicense.exe");
#else
            Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), 
                @"..\..\CommonComponents\UserLicense.exe");
#endif

        /// <summary>
        /// The index to the licensing section of the ID Shield Office help file
        /// </summary>
        static readonly string _LICENSING_HELP_INDEX = "license activation";

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            IDShieldOfficeForm mainForm = null;
            bool deleteFile = false;
            bool reset = false;
            bool openLicenseUtility = false;
            string fileName = "";
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                // Check for a valid number of arguments
                if (args.Length > 2)
                {
                    ShowUsage("Too many arguments.");
                    return;
                }

                if (args.Length == 1)
                {
                    // Check for usage message
                    if (args[0] == @"/?")
                    {
                        ShowUsage();
                        return;
                    }
                    else if (args[0].Equals(@"/r", StringComparison.OrdinalIgnoreCase) || 
                        args[0].Equals(@"/reset", StringComparison.OrdinalIgnoreCase))
                    {
                        reset = true;
                    }
                    else if(args[0].Equals(@"/l", StringComparison.OrdinalIgnoreCase) ||
                        args[0].Equals(@"/license", StringComparison.OrdinalIgnoreCase))
                    {
                        openLicenseUtility = true;
                    }
                }

                // Get the file name from the command line and ensure it exists
                if (!reset && args.Length >= 1)
                {
                    fileName = Path.GetFullPath(args[0]);
                }

                // Check if the delete file flag is present
                if (args.Length == 2)
                {
                    if (args[1].Equals(@"/d", StringComparison.OrdinalIgnoreCase))
                    {
                        deleteFile = true;
                    }
                    else
                    {
                        ShowUsage(args[1] + " is not a recognized command line argument.");
                        return;
                    }
                }

                // Check if licenses need to be loaded
                if (!openLicenseUtility)
                {
                    // Load licenses
                    LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());

                    // Open the license utility if ID Shield Office is not licensed
                    openLicenseUtility = 
                        !LicenseUtilities.IsLicensed(LicenseIdName.IdShieldOfficeObject);

                    // Display a message to the user if it the license utility will be opened
                    if (openLicenseUtility)
                    {
                        DisplayLicensingMessage();
                    }
                }

                // Open the licensing utility if necessary
                if (openLicenseUtility)
                {
                    OpenLicenseUtility();
                    return;
                }

                // Also need to license SandDock before creating the form
                TD.SandDock.SandDockManager.ActivateProduct(_SANDOCK_LICENSE_STRING);

                // Check for file name
                if (string.IsNullOrEmpty(fileName))
                {
                    // Show a splash screen and initialize the main form
                    SplashScreen.ShowDuringFormCreation("Resources.Splash.bmp", out mainForm);
                }
                else
                {
                    // Initialize the main form and open the specified file
                    // (Set tempFile if this is a temporary file)
                    mainForm = new IDShieldOfficeForm(fileName, deleteFile);
                }

                // Set the reset flag
                mainForm.ResetForm = reset;

                // Display the main form
                Application.Run(mainForm);
            }
            catch (Exception ex)
            {
                ExtractException.Display("ELI21975", ex);
            }
            finally
            {
                // Dispose of the main form
                if (mainForm != null)
                {
                    mainForm.Dispose();
                }

                if (deleteFile)
                {
                    if(File.Exists(fileName))
                    {
                        try
                        {
                            File.Delete(fileName);
                        }
                        catch (Exception ex)
                        {
                            // Ensure exception is not thrown from finally block
                            ExtractException.Log("ELI23261", ex);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Opens the licensing utility and product activation help dialog.
        /// </summary>
        static void OpenLicenseUtility()
        {
            // Open the license utility
            Process licenseUtility = Process.Start(_USER_LICENSE_PATH);

            // Wait up to five seconds for the window to open
            if (!licenseUtility.WaitForInputIdle(5000))
            {
                throw new ExtractException("ELI23314", "Unable to open licensing utility.");
            }

            // Get the window handle
            IntPtr utilityHandle = licenseUtility.MainWindowHandle;

            // Get the screen coordinates for the two windows
            Rectangle utilityBounds =
                NativeMethods.GetScreenBoundsFromWindowHandle(utilityHandle);

            // Get the working area of the screen
            Screen utilityScreen = Screen.FromHandle(utilityHandle);
            Rectangle workArea = utilityScreen.WorkingArea;

            // Reposition the license utility if there is enough 
            // space on the screen to nicely resize the windows
            bool enoughSpace = workArea.Width - utilityBounds.Width >= 450 &&
                workArea.Height > utilityBounds.Height;
            Rectangle utilityArea = Rectangle.Empty;
            if (enoughSpace)
            {
                // Calculate an area of the screen for the licensing utility, 
                // roughly half the size of the screen
                utilityArea = new Rectangle(workArea.Left, workArea.Top,
                    Math.Max(workArea.Width / 2, utilityBounds.Width), workArea.Height);

                // Center the licensing utility in this area
                utilityBounds.X =
                    utilityArea.Left + (utilityArea.Width - utilityBounds.Width) / 2;
                utilityBounds.Y =
                    utilityArea.Top + (utilityArea.Height - utilityBounds.Height) / 2;
                NativeMethods.SetWindowBounds(utilityHandle, utilityBounds, true);
            }

            // Display the help file
            IntPtr helpHandle = NativeMethods.ShowKeywordHelp(utilityHandle,
                IDShieldOfficeForm.HelpFileUrl, _LICENSING_HELP_INDEX);

            // Ensure that the help file stays open as long as the license utility
            try
            {
                // Set the focus on the license utility
                NativeMethods.SetForegroundWindow(utilityHandle);

                // If there is not enough space to reposition the help dialog, we are done.
                if (!enoughSpace)
                {
                    return;
                }

                // Get the screens of the help dialog
                Screen helpScreen = Screen.FromHandle(helpHandle);

                // There is no need to resize the dialog if it is open on a different screen
                if (!utilityScreen.Equals(helpScreen))
                {
                    return;
                }

                // Expand the help dialog to fit in the remaining area
                Rectangle helpBounds = new Rectangle(utilityArea.Right, utilityArea.Top,
                    workArea.Width - utilityArea.Width, utilityArea.Height);
                NativeMethods.SetWindowBounds(helpHandle, helpBounds, true);
            }
            finally
            {
                // Keep the help file open until the license utility closes
                licenseUtility.WaitForExit();
            }
        }
        
        /// <summary>
        /// Displays a friendly message to the user indicating that ID Shield Office isn't 
        /// licensed.
        /// </summary>
        static void DisplayLicensingMessage()
        {
            string message = "ID Shield Office needs to be licensed before it can be used." +
                Environment.NewLine + 
                "Click OK to open the licensing utility and to request a license file.";

            MessageBox.Show(message, "ID Shield Office needs license", MessageBoxButtons.OK, 
                MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, 0);
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
            usage.AppendLine(" [<filename>|/?|/reset|/license] [/d]");
            usage.AppendLine();
            usage.AppendLine("Options:");
            usage.AppendLine("-----------------");
            usage.AppendLine("/reset - Resets the form and toolstrip locations to their defaults");
            usage.AppendLine();
            usage.AppendLine("/? - Display usage information");
            usage.AppendLine();
            usage.AppendLine("/license - Open the licensing utility and licensing help file");
            usage.AppendLine();
            usage.AppendLine("<filename> - The image file to open");
            usage.AppendLine();
            usage.AppendLine("/d - Deletes <filename> when the application exits");
            usage.AppendLine();

            // Display the usage as an error or as an information box
            MessageBox.Show(usage.ToString(), isError ? "Error" : "Usage", MessageBoxButtons.OK,
                isError ? MessageBoxIcon.Error : MessageBoxIcon.Information);
        }
    }
}