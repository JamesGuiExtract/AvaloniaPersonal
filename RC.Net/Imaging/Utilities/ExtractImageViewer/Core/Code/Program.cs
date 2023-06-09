using Extract.Licensing;
using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;
using System.Text;
using System.Windows.Forms;

namespace Extract.Imaging.Utilities.ExtractImageViewer
{
    static class Program
    {
        #region Constants

        /// <summary>
        /// The tif file extension that will be registered if the application
        /// is run with the /r option.
        /// </summary>
        const string _TIF_FILE_EXTENSION = ".tif";

        /// <summary>
        /// The tif file description that will be registered if the application
        /// is run with the /r option.
        /// </summary>
        const string _TIF_FILE_DESCRIPTION = "TIF Image File";

        #endregion Constants

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
                if (args.Length > 4)
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

                // Load the licenses
                // NOTE: Need to do this before processing command line arguments since
                // some of the options call into licensed code (i.e. RegisterTifFileAssociation)
                LicenseUtilities.LoadLicenseFilesFromFolder(0, new Extract.Licensing.MapLabel());

                string fileToOpen = null;
                string scriptFile = null;
                string ocrTextFile = null;
                bool sendOcrToClipboard = false;
                bool reuseAlreadyOpenImageViewer = false;
                bool showSearchWindow = false;
                bool formatOcrTextAsXml = false;
                bool resetToolStrips = false;
                for (int i=0; i < args.Length; i++)
                {
                    string argument = args[i];
                    if (argument.Equals("/r", StringComparison.OrdinalIgnoreCase))
                    {
                        RegisterTifFileAssociation();
                        return;
                    }
                    else if (argument.Equals("/u", StringComparison.OrdinalIgnoreCase))
                    {
                        UnregisterTifFileAssociation();
                        return;
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
                    else if (argument.Equals("/xml", StringComparison.OrdinalIgnoreCase))
                    {
                        formatOcrTextAsXml = true;
                    }
                    else if (argument.Equals("/s", StringComparison.OrdinalIgnoreCase))
                    {
                        showSearchWindow = true;
                    }
                    else if (argument.Equals("/l", StringComparison.OrdinalIgnoreCase))
                    {
                        reuseAlreadyOpenImageViewer = true;
                    }
                    else if (argument.Equals("/closeall", StringComparison.OrdinalIgnoreCase))
                    {
                        // Close all open image viewers
                        CloseAllOpenImageViewers();
                        return;
                    }
                    else if (argument.Equals("/e", StringComparison.OrdinalIgnoreCase))
                    {
                        // Check for file name
                        i++;
                        if (i < args.Length)
                        {
                            scriptFile = Path.GetFullPath(args[i]);

                            // Ensure the file exists [DNRCAU #477]
                            if (!File.Exists(scriptFile))
                            {
                                ExtractException ee = new ExtractException("ELI30208",
                                    "Script file cannot be found.");
                                ee.AddDebugData("Script File Name", scriptFile, false);
                                throw ee;
                            }
                        }
                        else
                        {
                            ShowUsage("Must specify a file name with /e option.");
                            return;
                        }
                    }
                    else if (argument.Equals("/script?", StringComparison.OrdinalIgnoreCase))
                    {
                        // Show the script usage message and exit
                        ShowScriptUsage();
                        return;
                    }
                    else if (argument.Equals("/ctrlid?", StringComparison.OrdinalIgnoreCase))
                    {
                        // Show the control ID help and exit
                        ShowControlIdHelp();
                        return;
                    }
                    else if (argument.Equals("/reset", StringComparison.OrdinalIgnoreCase))
                    {
                        resetToolStrips = true;
                    }
                    else if (argument.StartsWith("/", StringComparison.OrdinalIgnoreCase))
                    {
                        ShowUsage("Unrecognized command line option: " + argument);
                        return;
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

                // Check that if /xml is specified either /o or /c has been specified
                if (formatOcrTextAsXml && !sendOcrToClipboard && string.IsNullOrEmpty(ocrTextFile))
                {
                    ShowUsage(
                        "Cannot specify /xml option without specifying either /c or /o options.");
                    return;
                }

                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.ExtractCoreObjects,
                    "ELI21841", "Extract Image Viewer");

                if (reuseAlreadyOpenImageViewer)
                {
                    // If there are other imageviewers open, send data to them
                    if (UseOpenImageViewers(fileToOpen, sendOcrToClipboard, ocrTextFile,
                        scriptFile, formatOcrTextAsXml))
                    {
                        // Just return since the open image viewer is handling the object
                        return;
                    }
                }

                // Run the image viewer, opening the image file if specified
                Application.Run(new ExtractImageViewerForm(fileToOpen, ocrTextFile,
                    sendOcrToClipboard, showSearchWindow, scriptFile, formatOcrTextAsXml,
                    resetToolStrips));
            }
            catch (Exception ex)
            {
                ExtractException ee = new ExtractException("ELI21972",
                    "Failed while opening Extract Image Viewer.", ex);
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
            usage.AppendLine("    /xml - Must be combined with either /c or /o <ocrfile>. Specifies");
            usage.AppendLine("         that OCRed text will be written in an XML format to either");
            usage.AppendLine("         the clipboard or the specified file.");
            usage.AppendLine("    /s - Display the search window");
            usage.AppendLine("    /l - Reuse a current Image Viewer if one already exists");
            usage.AppendLine("    /closeall - Close any currently open Image Viewer");
            usage.AppendLine("    /e <scriptfile> - Execute script commands specified in <scriptfile>");
            usage.AppendLine("    /script? - Displays help for script commands");
            usage.AppendLine("    /ctrlid? - Lists the toolbar control ID numbers");
            usage.AppendLine("    /reset - Resets the toolstrips to their default locations.");
            usage.AppendLine();
            usage.AppendLine("<filename> - The image file to open");

            // Display the usage as an error or as an information box
            MessageBox.Show(usage.ToString(), isError ? "Error" : "Usage", MessageBoxButtons.OK,
                isError ? MessageBoxIcon.Error : MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1, 0);
        }

        /// <summary>
        /// Unregisters the .tif file extension with ExtractImageViewer.exe
        /// </summary>
        static void UnregisterTifFileAssociation()
        {
            // Unregister the tif file extension
            RegistryMethods.UnregisterFileAssociation(_TIF_FILE_EXTENSION,
                _TIF_FILE_DESCRIPTION);
        }

        /// <summary>
        /// Registers the .tif file extension with ExtractImageViewer.exe
        /// </summary>
        static void RegisterTifFileAssociation()
        {
            // Register the application with the tif file extension
            RegistryMethods.RegisterFileAssociation(_TIF_FILE_EXTENSION,
                _TIF_FILE_DESCRIPTION, Application.ExecutablePath, false);
        } 

        /// <summary>
        /// Closes all open Extract Image Viewers
        /// </summary>
        static void CloseAllOpenImageViewers()
        {
            int currentID = SystemMethods.GetCurrentProcessId();
            string name = SystemMethods.GetProcessName(currentID);
            foreach (Process process in Process.GetProcessesByName(name))
            {
                if (process.Id != currentID)
                {
                    process.CloseMainWindow();
                }

                // Dispose of the process object
                process.Dispose();
            }
        }

        /// <summary>
        /// Will attempt to send all options/commands to the already open image viewer.
        /// </summary>
        /// <param name="fileToOpen">The image file to open.</param>
        /// <returns><see langword="true"/> if another image viewer was open and has
        /// handled the options, <see langword="false"/> otherwise.</returns>
        /// <param name="sendToClipboard">If <see langword="true"/> then OCR results will
        /// be sent to the clipboard rather than displayed in a message box.</param>
        /// <param name="ocrTextFile">The file name to send OCR text results to. If
        /// <see langword="null"/> or <see cref="String.Empty"/> then results will be
        /// sent to a message box.</param>
        /// <param name="scriptFile">The script file to execute in the open image viewer.</param>
        /// <param name="formatOcrTextAsXml">Whether or not OCRed text should be formatted
        /// as XML before being output.</param>
        static bool UseOpenImageViewers(string fileToOpen, bool sendToClipboard, string ocrTextFile,
            string scriptFile, bool formatOcrTextAsXml)
        {
            IpcChannel channel = new IpcChannel();
            bool channelRegistered = false;
            bool sentToImageViewer = false;
            try
            {
                ChannelServices.RegisterChannel(channel, true);
                channelRegistered = true;
                int currentID = SystemMethods.GetCurrentProcessId();
                string name = SystemMethods.GetProcessName(currentID);
                foreach (Process process in Process.GetProcessesByName(name))
                {
                    int id = process.Id;
                    if (id != currentID)
                    {
                        // Get the remote handler object.
                        // NOTE: Do not dispose of this handler since it is a reference to
                        // the one held in the remote image viewer
                        RemoteMessageHandler handler = (RemoteMessageHandler)Activator.GetObject(
                            typeof(RemoteMessageHandler),
                            RemoteMessageHandler.BuildRemoteObjectUri(id));

                        // If the image viewer is currently minimized then restore it
                        // [DNRCAU #465]
                        handler.UnminimizeForm();

                        // Set whether text should be sent to the clipboard or to
                        // a text file rather than to a message box.
                        handler.SendOcrTextToClipboard(sendToClipboard);
                        handler.SendOcrTextToFile(ocrTextFile);

                        // Set whether output text should be formatted as XML
                        handler.FormatOcrResultAsXml(formatOcrTextAsXml);

                        // Open the image in the image viewer
                        if (!string.IsNullOrEmpty(fileToOpen))
                        {
                            handler.OpenImage(fileToOpen);
                        }

                        // Process the script file if there is one
                        if (!string.IsNullOrEmpty(scriptFile))
                        {
                            handler.ExecuteScriptFile(scriptFile);
                        }

                        sentToImageViewer = true;
                    }

                    // Dispose of the process
                    process.Dispose();
                }
            }
            catch (Exception ex)
            {
                throw ExtractException.AsExtractException("ELI30144", ex);
            }
            finally
            {
                if (channelRegistered)
                {
                    ChannelServices.UnregisterChannel(channel);
                }
            }

            return sentToImageViewer;
        }

        /// <summary>
        /// Displays the scripting usage help to the user.
        /// </summary>
        static void ShowScriptUsage()
        {
            StringBuilder usage = new StringBuilder();
            usage.AppendLine("Script commands:");
            usage.AppendLine("    SetWindowPos <position> - Set the position and size of the image viewer");
            usage.AppendLine("        <position> - May be one of the following values:");
            usage.AppendLine("            Full - Fullscreen");
            usage.AppendLine("            Left - Left half of the screen");
            usage.AppendLine("            Top - Top half of the screen");
            usage.AppendLine("            Right - Right half of the screen");
            usage.AppendLine("            Bottom - Bottom half of the screen");
            usage.AppendLine("            <left>,<right>,<top>,<bottom> - Sized to specified pixel coordinates");
            usage.AppendLine("    HideButtons <ctrlid> - Hide a toolbar control");
            usage.AppendLine("        <ctrlid> - Comma separated list of toolbar control id numbers");
            usage.AppendLine("    OpenFile <filename> - Opens the specified file");
            usage.AppendLine("    AddTempHighlight <startX>,<startY>,<endX>,<endY>,<height>,<pagenumber> -");
            usage.AppendLine("        Creates a temporary highlight at the specified location");
            usage.AppendLine("    ClearTempHighlights - Clears all highlights created by AddTempHighlight");
            usage.AppendLine("    ClearImage - Closes any open image in the image window");
            usage.AppendLine("    SetCurrentPageNumber <pagenumber> - Goes to the specified page");
            usage.AppendLine("    ZoomIn - Zooms in");
            usage.AppendLine("    ZoomOut - Zooms out");
            usage.AppendLine("    ZoomExtents - Toggles fit to page mode");
            usage.AppendLine("    CenterOnTempHighlight - Centers on the first temporary highlight");
            usage.AppendLine("    ZoomToTempHighlight - Centers on the first temporary highlight and");
            usage.AppendLine("        zooms in around the highlight");
            usage.AppendLine("    HideMenu - Hides the image viewer menu bar");
            usage.AppendLine("    Pause [Text] - Pauses the script and displays a message box");
            usage.AppendLine("        containing the optional text");

            // Display the usage to the user
            MessageBox.Show(usage.ToString(), "Script Usage", MessageBoxButtons.OK,
                MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, 0);
        }

        /// <summary>
        /// Displays the control ID help to the user
        /// </summary>
        static void ShowControlIdHelp()
        {
            // Display the control ID usage to the user
            string usage = ImageViewerControlId.BuildControlIdHelp();
            MessageBox.Show(usage, "Control Ids", MessageBoxButtons.OK,
                MessageBoxIcon.Information, MessageBoxDefaultButton.Button1, 0);
        }
    }
}