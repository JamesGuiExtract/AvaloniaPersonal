using Extract.Licensing;
using Extract.Utilities;
using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Forms;
using UCLID_FILEPROCESSINGLib;

namespace Extract.FileActionManager.RunFPSFile
{
    /// <summary>
    /// Processes a single file using the specified FPS file.
    /// </summary>
    static class Program
    {
        #region Constants

        /// <summary>
        /// The name of the object to be used in the validate license calls.
        /// </summary>
        const string _OBJECT_NAME = "RunFPSFile";

        #endregion Constants

        #region Fields

        /// <summary>
        /// The FPS file to use to process the document.
        /// </summary>
        static string _fpsFileName;

        /// <summary>
        /// The document to be processed.
        /// </summary>
        static string _sourceDocName;

        /// <summary>
        /// Uses the file processing tasks defined in the FPS file to process the source document
        /// but does not utilize the database settings or action name specified in the FPS file in
        /// any way. The file is processed whether or not it exists in the database and regardless
        /// of what its database status may be.
        /// <para><b>Requirements:</b></para>
        /// Cannot be combined the /queue, /process or /forceProcessing arguments.
        /// The FPS file cannot contain any tasks that depend on access to the defined database or
        /// action.
        /// </summary>
        static bool _ignoreDb;

        /// <summary>
        /// Queues the document into the FPS file's database if it if it has not already been queued.
        /// </summary>
        static bool _queue;

        /// <summary>
        /// Processes the document immediately if it is currently pending in the source FPS file's
        /// action or is combined with the queue option and it was not already queued.
        /// </summary>
        static bool _process;

        /// <summary>
        /// Forces the status of the document to pending in the database for the specified action
        /// if the document was previously queued.
        /// <para><b>Requirements:</b></para>
        /// Must be used in conjunction with the /queue command-line option.
        /// </summary>
        static bool _forceProcessing;

        /// <summary>
        /// The priority assigned to a file being queued.
        /// </summary>
        static EFilePriority _filePriority = EFilePriority.kPriorityDefault;

        /// <summary>
        /// Specifies a file to which exceptions should be logged rather than displaying them.
        /// </summary>
        static string _logFileName;

        #endregion Fields

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                // Validate license
                LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());
                LicenseUtilities.ValidateLicense(LicenseIdName.FlexIndexIDShieldCoreObjects,
                    "ELI29492", _OBJECT_NAME);

                // Parse the command-line arguments to initialize the program settings.
                if (!ParseArgs(args))
                {
                    // If the settings could not be parsed, no file can be processed; return.
                    return;
                }

                // Create a new FileProcessingManagerClass instance to do the processing.
                FileProcessingManagerClass fileProcessingManager = new FileProcessingManagerClass();
                fileProcessingManager.LoadFrom(_fpsFileName, false);

                if (_ignoreDb)
                {
                    // If no database interaction is required, the processingManager does not need
                    // to manage the processing. Execute the processingManager tasks directly.

                    FAMTagManagerClass tagManager = new FAMTagManagerClass();
                    tagManager.FPSFileDir = Path.GetDirectoryName(_fpsFileName);

                    // Use a local task executor to directly execute the file processing tasks.
                    FileProcessingTaskExecutorClass taskExecutor =
                        new FileProcessingTaskExecutorClass();
                    taskExecutor.InitProcessClose(_sourceDocName,
                        fileProcessingManager.FileProcessingMgmtRole.FileProcessors, 0, 0, null,
                        tagManager, null, false);
                }
                else
                {
                    // If database interaction is required, allow fileProcessigManager to manage the
                    // processing.
                    fileProcessingManager.ProcessSingleFile(_sourceDocName, _queue, _process,
                        _forceProcessing, _filePriority);
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI29491", ex);

                // If an exception was thrown while the _ignoreDb is being used, check to see if the
                // exception was thrown while trying to obtain a database schema version.
                if (_ignoreDb)
                {
                    for (ExtractException nextException = ee;
                         nextException != null;
                         nextException = nextException.InnerException as ExtractException)
                    {
                        if (nextException.EliCode == "ELI14775" ||
                            nextException.EliCode == "ELI14973" ||
                            nextException.EliCode == "ELI27530" ||
                            nextException.EliCode == "ELI29796")
                        {
                            // If so, database access was attempted but is not allowed. Create a new
                            // outer exception that better states the problem.
                            ee = new ExtractException("ELI29558", "File processing task attempted " +
                                "to access database while using /ignoreDB switch!", ex);
                            break;
                        }
                    }
                }

                if (string.IsNullOrEmpty(_logFileName))
                {
                    ee.Display();
                }
                else
                {
                    ee.Log(_logFileName);
                }
            }
        }

        #region Private Methods

        /// <summary>
        /// Attempts to initialize the program's fields using the specified arguments.
        /// </summary>
        /// <param name="args">The command-line arguments to parse.</param>
        static bool ParseArgs(string[] args)
        {
            if (args.Length == 1 && args[0].Equals("/?", StringComparison.Ordinal))
            {
                ShowUsage(null);
                return false;
            }

            if (args.Length < 3 || args.Length > 8)
            {
                ShowUsage("Invalid number of command-line arguments.");
                return false;
            }

            _fpsFileName = args[0];
            if (!File.Exists(_fpsFileName))
            {
                ShowUsage("Cannot find FPS file:\r\n" + _fpsFileName);
                return false;
            }

            _sourceDocName = args[1];
            if (!File.Exists(_sourceDocName))
            {
                ShowUsage("Cannot find source document:\r\n" + _sourceDocName);
                return false;
            }

            _sourceDocName =
                    FileSystemMethods.GetAbsolutePath(_sourceDocName, Environment.CurrentDirectory);

            for (int i = 2; i < args.Length; i++)
            {
                string argument = args[i].ToLower(CultureInfo.CurrentCulture);
                
                if (argument.Equals("/ignoredb", StringComparison.Ordinal))
                {
                    _ignoreDb = true;
                }
                else if (argument.Equals("/queue", StringComparison.Ordinal))
                {
                    _queue = true;
                }
                else if (argument.Equals("/process", StringComparison.Ordinal))
                {
                    _process = true;
                }
                else if (argument.Equals("/forceprocessing", StringComparison.Ordinal))
                {
                    _forceProcessing = true;
                }
                else if (argument.Equals("/priority", StringComparison.Ordinal))
                {
                    if (i + 1 >= args.Length)
                    {
                        ShowUsage("Priority value required.");
                        return false;
                    }

                    i++;
                    int intValue;
                    if (int.TryParse(args[i], out intValue) && intValue >= 0 && intValue <= 5)
                    {
                        _filePriority = (EFilePriority)intValue;
                    }
                    else
                    {
                        ShowUsage("Invalid priority value specified: " + args[i]);
                        return false;
                    }
                }
                else if (argument.Equals("/ef", StringComparison.Ordinal))
                {
                    if (i + 1 >= args.Length)
                    {
                        ShowUsage("Log file argument required.");
                        return false;
                    }

                    i++;
                    _logFileName = args[i];
                }
                else
                {
                    ShowUsage("Unrecognized command-line argument: \"" + args[i] + "\"");
                    return false;
                }
            }

            if (!_ignoreDb && !_queue && !_process)
            {
                ShowUsage("At least one of the following three switches must be specified: " +
                    "/ignoreDB, /process or /queue.");
                return false;
            }

            if (_ignoreDb && (_queue || _process || _forceProcessing || _filePriority != 0))
            {
                ShowUsage("/ignoreDB is not compatible with /queue, /process /forceProcessing " +
                    " or /priority.");
                return false;
            }

            if (!_queue)
            {
                if (_forceProcessing)
                {
                    ShowUsage("/forceProcessing must be used in conjunction with /queue.");
                    return false;
                }
                else if (_filePriority > 0)
                {
                    ShowUsage("/priority must be used in conjunction with /queue.");
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Displays the usage message with the specified error message.
        /// </summary>
        /// <param name="errorMessage">The error message to display.</param>
        static void ShowUsage(string errorMessage)
        {
            bool isError = !string.IsNullOrEmpty(errorMessage);

            StringBuilder usage = new StringBuilder();

            if (isError)
            {
                usage.AppendLine(errorMessage);
                usage.AppendLine();
                usage.AppendLine("Usage:");
            }
            else
            {
                usage.AppendLine("Processes a single file using the specified FPS file.");
                usage.AppendLine();
            }

            usage.Append(Environment.GetCommandLineArgs()[0]);
            usage.Append(" <FPSFileName> <SourceDocName> [/ignoreDb] [/queue] [/process]");
            usage.AppendLine(" [/forceProcessing] [/priority <integer value>] [/ef <filename>]");
            usage.AppendLine();

            usage.Append("FPSFileName: The FPS file to use to process the document.");
            usage.AppendLine();
            usage.AppendLine();

            usage.Append("SourceDocName: The document to be processed.");
            usage.AppendLine();
            usage.AppendLine();

            usage.Append("/ignoreDb: Uses the file processing tasks defined in the FPS ");
            usage.Append("file to process the source document but does not utilize the database ");
            usage.Append("settings or action name specified in the FPS file in any way. The file ");
            usage.Append("is processed whether or not it exists in the database and regardless ");
            usage.Append("of what its database status may be. Cannot be combined the /queue, ");
            usage.Append("/process /forceProcessing or /priority switches. The FPS file cannot ");
            usage.Append("contain any tasks that depend on access to the FPS file's specified ");
            usage.Append("database or action.");
            usage.AppendLine();
            usage.AppendLine();

            usage.Append("/queue: Queues the document into the FPS file's database and ");
            usage.Append("action if it has not already been queued.");
            usage.AppendLine();
            usage.AppendLine();

            usage.Append("/process: Processes the document immediately if it is currently ");
            usage.Append("pending in the source FPS file's action or is combined with the queue ");
            usage.Append("option and it was not already queued.");
            usage.AppendLine();
            usage.AppendLine();

            usage.Append("/forceProcessing: Forces the status of the document to pending ");
            usage.Append("in the database for the FPS file's action if the document was ");
            usage.Append("previously queued. Must be used in conjunction with the /queue switch.");
            usage.AppendLine();
            usage.AppendLine();

            usage.Append("/priority: The priority as an integer (where 1 = low and 5 = high and ");
            usage.Append("0 = default) to assign to a file being queued. Must be used in ");
            usage.Append("conjunction with the /queue switch.");
            usage.AppendLine();
            usage.AppendLine();

            usage.Append("/ef <filename>: Log exceptions to the specified file instead of ");
            usage.AppendLine("displaying them.");

            MessageBox.Show(usage.ToString(), isError ? "Error" : "Usage", MessageBoxButtons.OK,
                isError ? MessageBoxIcon.Error : MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1, 0);
        }

        #endregion Private Methods
    }
}