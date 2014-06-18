﻿using Extract.Licensing;
using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using UCLID_FILEPROCESSINGLib;

namespace Extract.FileActionManager.Utilities
{
    /// <summary>
    /// Application that allows searching and inspection of files in a File Action Manager database
    /// based on database conditions, OCR content and data content.
    /// </summary>
    static class FAMFileInspectorProgram
    {
        #region Fields

        /// <summary>
        /// The name of the database server to connect to.
        /// </summary>
        static string _databaseServer;

        /// <summary>
        /// The name of the database to connect to.
        /// </summary>
        static string _databaseName;

        /// <summary>
        /// The directory being inspected by this instance (rather than a database).
        /// </summary>
        static string _directory;

        /// <summary>
        /// A semicolon delimited list of file extensions that should be displayed when
        /// inspecting the contents of <see cref="P:SourceDirectory"/> rather than
        /// <see cref="FileProcessingDB"/>.
        /// </summary>
        static string _fileFilter;

        /// <summary>
        /// Indicates if files in subdirectories of <see cref="_directory"/> should be included.
        /// </summary>
        static bool _recursive;

        /// <summary>
        /// The name of the action when limiting the initial file selection based on a file action
        /// status condition.
        /// </summary>
        static string _actionName;

        /// <summary>
        /// The target status when limiting the initial file selection based on a file action
        /// status condition.
        /// </summary>
        static string _actionStatus;

        /// <summary>
        /// An SQL query to be used to limit the initial file selection.
        /// </summary>
        static string _queryFilename;

        /// <summary>
        /// The number of files that may be displayed in the file list at once, or
        /// <see langword="null"/> to use the default file count limit.
        /// </summary>
        static int? _fileCount;

        /// <summary>
        /// The <see cref="T:SubsetType"/> specifying the method a limited subset should be
        /// selected from the overall set of files by default.
        /// </summary>
        static SubsetType? _subsetType;

        #endregion Fields

        #region Main

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

                // Load the license files from folder
                LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());

                LicenseUtilities.ValidateLicense(LicenseIdName.FileActionManagerObjects,
                    "ELI35709", "FAMFileInspectorProgram");

                // Parse the command-line arguments to initialize the program settings.
                if (!ParseArgs(args))
                {
                    // If the settings could not be parsed, return without displaying the form.
                    return;
                }

                var famFileInspectorForm = new FAMFileInspectorForm();

                if (!string.IsNullOrWhiteSpace(_databaseServer))
                {
                    famFileInspectorForm.DatabaseServer = _databaseServer;
                    famFileInspectorForm.DatabaseName = _databaseName;
                }
                else if (!string.IsNullOrWhiteSpace(_directory))
                {
                    famFileInspectorForm.SourceDirectory = _directory;
                    famFileInspectorForm.Recursive = _recursive;

                    // Check that _fileFilter is not null which is a separate case from being empty
                    // since it indicates the command line option was used.
                    if (_fileFilter != null)
                    {
                        famFileInspectorForm.FileFilter = _fileFilter;
                    }
                }

                if (_fileCount.HasValue)
                {
                    famFileInspectorForm.MaxFilesToDisplay = _fileCount.Value;
                }
                if (_subsetType.HasValue)
                {
                    famFileInspectorForm.SubsetType = _subsetType.Value;
                }

                if (!string.IsNullOrWhiteSpace(famFileInspectorForm.SourceDirectory))
                {
                    Application.Run(famFileInspectorForm);
                    return;
                }

                bool loggedIn = false;
                while (!loggedIn)
                {
                    try
                    {
                        // Don't show the database selection prompt if one has been specified.
                        if (!string.IsNullOrWhiteSpace(famFileInspectorForm.DatabaseName) ||
                            famFileInspectorForm.ShowSelectDB(false))
                        {
                            // Checks schema
                            famFileInspectorForm.FileProcessingDB.ResetDBConnection(true);
                            loggedIn = true;

                            famFileInspectorForm.ResetFileSelectionSettings();

                            // If any conditions were specified via command-line arguments, apply
                            // them.
                            ApplyConditions(famFileInspectorForm);

                            famFileInspectorForm.ResetSearch();

                            Application.Run(famFileInspectorForm);
                        }
                        else
                        {
                            // User cancelled.
                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        ex.ExtractDisplay("ELI35838");
                    }

                    // If unable to connect to the database, clear the database connection settings
                    // to display the database connection dialog.
                    famFileInspectorForm.DatabaseServer = null;
                    famFileInspectorForm.DatabaseName = null;
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI35710");
            }
        }

        #endregion Main

        #region Private Methods

        /// <summary>
        /// Applies condition(s) specified in the command-line args (if any).
        /// </summary>
        /// <param name="famFileInspectorForm">The <see cref="FAMFileInspectorForm"/> to which the
        /// condition(s) should be applied.</param>
        static void ApplyConditions(FAMFileInspectorForm famFileInspectorForm)
        {
            ApplyActionStatusCondition(famFileInspectorForm);
            ApplyQueryCondition(famFileInspectorForm);
        }

        /// <summary>
        /// Applies the action status condition specified in the command-line args (if any).
        /// </summary>
        /// <param name="famFileInspectorForm">The <see cref="FAMFileInspectorForm"/> to which the
        /// condition should be applied.</param>
        static void ApplyActionStatusCondition(FAMFileInspectorForm famFileInspectorForm)
        {   
            int actionId = -1;
            EActionStatus status = EActionStatus.kActionUnattempted;

            try
            {
                if (!string.IsNullOrWhiteSpace(_actionName))
                {
                    actionId = famFileInspectorForm.FileProcessingDB.GetActionID(_actionName);
                }
            }
            catch (Exception ex)
            {
                var ee = new ExtractException("ELI36098",
                    "Unable to apply action status condition: Invalid action name", ex);
                ee.AddDebugData("Action", _actionName, false);
                ee.Display();
                return;
            }

            try
            {
                if (!string.IsNullOrWhiteSpace(_actionName))
                {
                    status = famFileInspectorForm.FileProcessingDB.AsEActionStatus(_actionStatus);
                }
            }
            catch (Exception ex)
            {
                var ee = new ExtractException("ELI36099",
                    "Unable to apply action status condition: Invalid action status", ex);
                ee.AddDebugData("Status", _actionStatus, false);
                ee.Display();
                return;
            }

            try
            {
                if (!string.IsNullOrWhiteSpace(_actionName))
                {
                    famFileInspectorForm.FileSelector.AddActionStatusCondition(
                        famFileInspectorForm.FileProcessingDB, actionId, status);
                }
            }
            catch (Exception ex)
            {
                var ee = new ExtractException("ELI36069", "Unable to apply action status condition", ex);
                ee.AddDebugData("Action", _actionName, false);
                ee.AddDebugData("Status", _actionStatus, false);
                ee.Display();
            }
        }

        /// <summary>
        /// Applies the query condition specified in the command-line args (if any).
        /// </summary>
        /// <param name="famFileInspectorForm">The <see cref="FAMFileInspectorForm"/> to which the
        /// condition should be applied.</param>
        static void ApplyQueryCondition(FAMFileInspectorForm famFileInspectorForm)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(_queryFilename))
                {
                    string query = File.ReadAllText(_queryFilename);

                    famFileInspectorForm.FileSelector.AddQueryCondition(
                        famFileInspectorForm.FileProcessingDB, query);
                }
            }
            catch (Exception ex)
            {
                var ee = new ExtractException("ELI36094", "Unable to apply query condition", ex);
                ee.AddDebugData("Query filename", _queryFilename, false);
                ee.Display();
            }
        }

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

            for (int i = 0; i < args.Length; i++)
            {
                string argument = args[i];

                if (argument.Equals("/filecount", StringComparison.OrdinalIgnoreCase))
                {
                    i++;
                    if (i == args.Length)
                    {
                        ShowUsage("File count expected");
                        return false;
                    }

                    argument = args[i];
                    if (argument.Equals("top", StringComparison.OrdinalIgnoreCase))
                    {
                        _subsetType = SubsetType.Top;
                        i++;
                    }
                    else if (argument.Equals("bottom", StringComparison.OrdinalIgnoreCase))
                    {
                        _subsetType = SubsetType.Bottom;
                        i++;
                    }
                    else if (argument.Equals("random", StringComparison.OrdinalIgnoreCase))
                    {
                        _subsetType = SubsetType.Random;
                        i++;
                    }

                    int fileCount = 0;
                    if (int.TryParse(args[i], out fileCount))
                    {
                        _fileCount = fileCount;
                    }
                    else
                    {
                        ShowUsage("Unable to parse file count: \"" + args[i] + "\"");
                    }
                }
                else if (argument.Equals("/directory", StringComparison.OrdinalIgnoreCase))
                {
                    i++;
                    if (i == args.Length)
                    {
                        ShowUsage("Directory expected.");
                        return false;
                    }

                    _directory = args[i];
                }
                else if (i == 0)
                {
                    _databaseServer = argument;
                }
                else if (!string.IsNullOrWhiteSpace(_databaseServer) && i == 1)
                {
                    _databaseName = argument;
                }
                else if (argument.Equals("/query", StringComparison.OrdinalIgnoreCase))
                {
                    if (string.IsNullOrWhiteSpace(_databaseName))
                    {
                        ShowUsage("Query condition cannot be specified without a " +
                            "database server and name.");
                    }

                    i++;
                    if (i == args.Length)
                    {
                        ShowUsage("SQL query filename expected.");
                        return false;
                    }

                    _queryFilename = args[i];
                }
                else if (argument.Equals("/action", StringComparison.OrdinalIgnoreCase))
                {
                    if (string.IsNullOrWhiteSpace(_databaseName))
                    {
                        ShowUsage("File action condition cannot be specified without a " +
                            "database server and name.");
                    }

                    i++;
                    if (i == args.Length)
                    {
                        ShowUsage("Action name expected");
                        return false;
                    }

                    _actionName = args[i];
                }
                else if (argument.Equals("/status", StringComparison.OrdinalIgnoreCase))
                {
                    if (string.IsNullOrWhiteSpace(_databaseName))
                    {
                        ShowUsage("File action condition cannot be specified without a " +
                            "database server and name.");
                    }

                    i++;
                    if (i == args.Length)
                    {
                        ShowUsage("Action status expected");
                        return false;
                    }

                    _actionStatus = args[i];

                    if (_actionStatus.Equals("Unattempted", StringComparison.OrdinalIgnoreCase))
                    {
                        _actionStatus = "U";
                    }
                    else if (_actionStatus.Equals("Pending", StringComparison.OrdinalIgnoreCase))
                    {
                        _actionStatus = "P";
                    }
                    else if (_actionStatus.Equals("Processing", StringComparison.OrdinalIgnoreCase))
                    {
                        _actionStatus = "R";
                    }
                    else if (_actionStatus.Equals("Complete", StringComparison.OrdinalIgnoreCase) ||
                             _actionStatus.Equals("Completed", StringComparison.OrdinalIgnoreCase))
                    {
                        _actionStatus = "C";
                    }
                    else if (_actionStatus.Equals("Skipped", StringComparison.OrdinalIgnoreCase))
                    {
                        _actionStatus = "S";
                    }
                    else if (_actionStatus.Equals("Failed", StringComparison.OrdinalIgnoreCase))
                    {
                        _actionStatus = "F";
                    }
                }
                else if (argument.Equals("/filefilter", StringComparison.OrdinalIgnoreCase))
                {
                    i++;
                    if (i == args.Length)
                    {
                        ShowUsage("File filter list expected.");
                        return false;
                    }

                    _fileFilter = args[i];
                }
                else if (argument.Equals("/r", StringComparison.OrdinalIgnoreCase))
                {
                    _recursive = true;
                }
                else
                {
                    ShowUsage("Unrecognized command-line argument: \"" + argument + "\"");
                    return false;
                }
            }

            if (!string.IsNullOrWhiteSpace(_directory))
            {
                if (!string.IsNullOrWhiteSpace(_databaseServer) || !string.IsNullOrWhiteSpace(_databaseName))
                {
                    ShowUsage("A directory can be specified only in the absence of a database.");
                }
            }
            else
            {
                if (string.IsNullOrWhiteSpace(_databaseServer) != string.IsNullOrWhiteSpace(_databaseName))
                {
                    ShowUsage("A database server cannot be specified without a database name.");
                    return false;
                }

                if (string.IsNullOrWhiteSpace(_actionName) != string.IsNullOrWhiteSpace(_actionStatus))
                {
                    ShowUsage("The action and status command-line arguments must be used together " +
                        " and cannot be specified without a database server and name.");
                    return false;
                }

                if (_recursive)
                {
                    ShowUsage("The /r (recursive) paramater cannot be used without the /directory parameter");
                    return false;
                }

                // Check that _fileFilter is not null which is a separate case from being empty
                // since it indicates the command line option was used.
                if (_fileFilter != null)
                {
                    ShowUsage("The /filefilter paramater cannot be used without the /directory parameter");
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
                usage.AppendLine("Allows inspection of files in a FAM database.");
                usage.AppendLine();
            }

            usage.Append(Environment.GetCommandLineArgs()[0]);
            usage.AppendLine(" /? | [<ServerName> <DatabaseName> [/action <actionName> " +
                "/status <statusName>] [/query <queryFileName>] | /directory <directory> " +
                "[/filefilter <filefilter>] [/r]] " +
                "[/filecount [top|bottom|random] <count>]");
            usage.AppendLine();
            usage.AppendLine("ServerName: The name of the database server to connect to.");
            usage.AppendLine();
            usage.AppendLine("DatabaseName: The name of the database to connect to.");
            usage.AppendLine();
            usage.AppendLine("/action <actionName>: The name of the action when limiting the " +
                "initial file selection based on a file action status condition. Must be used " +
                "in conjunction with the /status argument.");
            usage.AppendLine();
            usage.AppendLine("/status <statusName>: The target status when limiting the " +
                "initial file selection based on a file action status condition. Must be used " +
                "in conjunction with the /action argument.");
            usage.AppendLine();
            usage.AppendLine("/query <queryFileName>: An file containing an SQL query to be " +
                "used to limit the initial file selection.");
            usage.AppendLine();
            usage.AppendLine("/directory <directory>: A directory to inspect (rather than a database).");
            usage.AppendLine();
            usage.AppendLine("/filefilter <filefilter>: A semicolon delimited specification of file " +
                "filters that should limit the files to be inspected within the specified directory.");
            usage.AppendLine();
            usage.AppendLine("/r: Specifies that files in subdirectories to the specified directory " +
                "should be included.");
            usage.AppendLine();
            usage.AppendLine("/filecount [top|bottom|random] <count>: Specifies number of files that may be " +
                "displayed in the file list at once and, optionally, how the subset should be selected from " +
                "the overall set of files by default. If top/bottom/random is omitted, the default is top.");

            MessageBox.Show(usage.ToString(), isError ? "Error" : "Usage", MessageBoxButtons.OK,
                isError ? MessageBoxIcon.Error : MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1, 0);
        }

        #endregion Private Methods
    }
}
