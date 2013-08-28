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
        #region Constants

        /// <summary>
        /// The name of this object.
        /// </summary>
        static readonly string _OBJECT_NAME = typeof(FAMFileInspectorProgram).ToString();
        
        #endregion Constants

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
                    "ELI35709", _OBJECT_NAME);

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

                if (_fileCount.HasValue)
                {
                    famFileInspectorForm.MaxFilesToDisplay = _fileCount.Value;
                }

                bool loggedIn = false;
                while (!loggedIn)
                {
                    try
                    {
                        // Don't show the database selection prompt if one has been specified.
                        if (!string.IsNullOrWhiteSpace(famFileInspectorForm.DatabaseName) ||
                            famFileInspectorForm.FileProcessingDB.ShowSelectDB(
                                        "Select database", false, false))
                        {
                            // Checks schema
                            famFileInspectorForm.FileProcessingDB.ResetDBConnection();
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
                else if (!_fileCount.HasValue && i == 0)
                {
                    _databaseServer = argument;
                }
                else if (!_fileCount.HasValue && i == 1)
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
                    else if (_actionStatus.Equals("Completed", StringComparison.OrdinalIgnoreCase))
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
                else
                {
                    ShowUsage("Unrecognized command-line argument: \"" + argument + "\"");
                    return false;
                }
            }

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
            usage.AppendLine(" /? | [/filecount <count>] | <ServerName> <DatabaseName> " +
                "[/action <actionName> /status <statusName>] [/query <queryFileName>] " +
                "[/filecount <count>]");
            usage.AppendLine();
            usage.AppendLine("ServerName: The name of the database server to connect to.");
            usage.AppendLine();
            usage.AppendLine("DatabaseName: The name of the database to connect to.");
            usage.AppendLine();
            usage.AppendLine("/action <actionName>: The name of the action when limiting the " +
                "initial file selection based on a file action status condition. Must be used " +
                "in conjuntion with the /status argument.");
            usage.AppendLine();
            usage.AppendLine("/status <statusName>: The target status when limiting the " +
                "initial file selection based on a file action status condition. Must be used " +
                "in conjuntion with the /status argument.");
            usage.AppendLine();
            usage.AppendLine("/query <queryFileName>: An file containing an SQL query to be " +
                "used to limit the initial file selection.");
            usage.AppendLine();
            usage.AppendLine("/filecount <count>: Specifies number of files that may be " +
                "displayed in the file list at once.");

            MessageBox.Show(usage.ToString(), isError ? "Error" : "Usage", MessageBoxButtons.OK,
                isError ? MessageBoxIcon.Error : MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1, 0);
        }

        #endregion Private Methods
    }
}
