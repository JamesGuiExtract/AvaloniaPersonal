using Extract.Licensing;
using Extract.Utilities;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using UCLID_COMUTILSLib;
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

        /// <summary>
        /// Pointer to the currently processing management role
        /// </summary>
        static FileProcessingMgmtRole _fileProcessingManagementRole;

        /// <summary>
        /// Pointer to the file record for the processing file
        /// </summary>
        static FileRecord _fileRecord;

        /// <summary>
        /// Pointer to the current tag manager
        /// </summary>
        static FAMTagManager _tagManager;

        /// <summary>
        /// Pointer to the current tag executor;
        /// </summary>
        static FileProcessingTaskExecutor _taskExecutor;

        #endregion Fields

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        // https://extract.atlassian.net/browse/ISSUE-12316
        // Do not initialize this thread as [STAThread] as multithread COM objects are created then
        // used on different threads which can lead to lockups if this thread is STA.
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
                    // [FlexIDSCore:3088]
                    // Set the stack size of the processing thread to be 1 MB or the maximum of the
                    // processing task's MinStackSize properties.
                    int stackSize = (int)
                        fileProcessingManager.FileProcessingMgmtRole.FileProcessors
                        .ToIEnumerable<IObjectWithDescription>()
                        .Select(objectWithDescription => objectWithDescription.Object)
                        .OfType<IFileProcessingTask>()
                        .Select(fileProcessingTask => fileProcessingTask.MinStackSize)
                        .Max();

                    // If no database interaction is required, the processingManager does not need
                    // to manage the processing. Execute the processingManager tasks directly.
                    ExtractException processingException = null;
                    Thread processingThread = new Thread(new ThreadStart(() =>
                        {
                            try
                            {
                                // Create a new copy of FileProcessingManagerClass on the processing
                                // thread to do the processing;
                                var fileProcessingManager2 = new FileProcessingManagerClass();
                                fileProcessingManager2.LoadFrom(_fpsFileName, false);

                                _fileProcessingManagementRole = fileProcessingManager2.FileProcessingMgmtRole;

                                _tagManager = new FAMTagManagerClass();
                                _tagManager.FPSFileDir = Path.GetDirectoryName(_fpsFileName);
                                _tagManager.FPSFileName = _fpsFileName;

                                // Setup file record for call to InitProcessClose
                                _fileRecord = new FileRecordClass();
                                _fileRecord.Name = _sourceDocName;
                                _fileRecord.FileID = 0;

                                // Use a local task executor to directly execute the file processing
                                // tasks.
                                _taskExecutor = new FileProcessingTaskExecutorClass();
                                try
                                {
                                    _taskExecutor.InitProcessClose(_fileRecord, _fileProcessingManagementRole.FileProcessors, 
                                        0, null, _tagManager, null, null, false);
                                }
                                catch (Exception taskEx)
                                {
                                    handleTaskExceptionNoDB(taskEx);
                                    throw taskEx.AsExtract("ELI37891");
                                }
                            }
                            catch (Exception ex)
                            {
                                processingException = ex.AsExtract("ELI35037");
                            }
                        }), stackSize);
                    
                    processingThread.Start();
                    processingThread.Join();

                    if (processingException != null)
                    {
                        throw processingException;
                    }
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
                        if (nextException.EliCode.EndsWith("14775", StringComparison.Ordinal) ||
                            nextException.EliCode.EndsWith("14973", StringComparison.Ordinal) ||
                            nextException.EliCode.EndsWith("27530", StringComparison.Ordinal) ||
                            nextException.EliCode.EndsWith("29796", StringComparison.Ordinal))
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

            if (args.Length < 3 || args.Length > 10)
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
            _fpsFileName =
                FileSystemMethods.GetAbsolutePath(_fpsFileName, Environment.CurrentDirectory);

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

        /// <summary>
        /// Performs the options selected in the fps when a task throws and exception when running
        /// RunFPSFile without a database.
        /// </summary>
        /// <remarks>The method is intended to be called from a catch block and logs all exceptions</remarks>
        /// <param name="taskException">The exception that was thrown by the task execution</param>
        private static void handleTaskExceptionNoDB(Exception taskException)
        {
            logErrorDetails(taskException);

            sendErrorEmail(taskException);

            executeErrorTask();
        }

        /// <summary>
        /// Executes the error task if there it is selected in the fpmRole. Logs exceptions.
        /// </summary>
        private static void executeErrorTask()
        {
            try
            {
                // Execute error task if configured
                if (_fileProcessingManagementRole.ExecuteErrorTask)
                {
                    // Push the error task into an IUnknownVector to pass to the TaskExecutor
                    IUnknownVectorClass tasksToRun = new IUnknownVectorClass();
                    tasksToRun.PushBack(_fileProcessingManagementRole.ErrorTask);

                    // Execute the error task
                    _taskExecutor.InitProcessClose(_fileRecord, tasksToRun, 0, null, _tagManager,
                        null, null, false);
                }
            }
            catch (Exception errorTaskEx)
            {
                errorTaskEx.ExtractLog("ELI37897");
            }
        }

        /// <summary>
        /// Sends an error email if it is selected in the _fpmRole. Logs exceptions.
        /// </summary>
        /// <remarks>Since this is expected to be ran with no database the SMTPSettings.config 
        /// must exist in "C:\ProgramData\Extract Systems\EmailSettings". This file is created by
        /// configuring the email settings using USBLicenseKeyManager</remarks>
        /// <param name="taskException">The exception to be sent in the email.</param>
        private static void sendErrorEmail(Exception taskException)
        {
            try
            {
                // Send error email if selected
                if (_fileProcessingManagementRole.SendErrorEmail)
                {
                    // The ErrorEmailTask should be configured.
                    var sendEmail = _fileProcessingManagementRole.ErrorEmailTask;
                    ExtractException.Assert("ELI37910", "No ErrorEmailTask configured.", sendEmail != null);

                    // Add the exception to the email
                    sendEmail.StringizedException = taskException.AsExtract("ELI37894").AsStringizedByteStream();

                    // Put the Email task in a IUnknownVector to pass to the TaskExcecutor
                    ObjectWithDescription errorEmailTaskWrapper = new ObjectWithDescription();
                    errorEmailTaskWrapper.Object = sendEmail;
                    IUnknownVectorClass tasksToRun = new IUnknownVectorClass();
                    tasksToRun.PushBack(errorEmailTaskWrapper);

                    // Execute the Email error task.
                    _taskExecutor.InitProcessClose(_fileRecord, tasksToRun, 0, null, _tagManager,
                        null, null, false);
                }
            }
            catch (Exception emailEx)
            {
                emailEx.ExtractLog("ELI37896");
            }
        }

        /// <summary>
        /// Logs the exception passed in if the _fpmRole has Log error details selected.
        /// </summary>
        /// <param name="taskException">The exception to be logged.</param>
        private static void logErrorDetails(Exception taskException)
        {
            try
            {
                // If error details are to be logged log the details to the file specified in the
                // fps configuration.
                if (_fileProcessingManagementRole.LogErrorDetails)
                {
                    string logFileName = _tagManager.ExpandTagsAndFunctions(_fileProcessingManagementRole.ErrorLogName, _fileRecord.Name);
                    if (Path.GetExtension(logFileName).ToLowerInvariant() == ".uex")
                    {
                        taskException.ExtractLog("ELI37893", logFileName);
                    }
                    else
                    {
                        ExtractException logException =
                            new ExtractException("ELI37911", "Error log file name must have uex extension.");
                        throw logException;
                    }
                }
            }
            catch (Exception logEx)
            {
                logEx.ExtractLog("ELI37895");
            }
        }
        
        #endregion Private Methods
    }
}
