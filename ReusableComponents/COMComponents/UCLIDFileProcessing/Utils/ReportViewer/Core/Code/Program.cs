using Extract.Licensing;
using Extract.Reporting;
using Extract.ReportViewer.Properties;
using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Extract.ReportViewer
{
    static class Program
    {
        #region Constants

        /// <summary>
        /// The application for sending a file by email.
        /// </summary>
        private static readonly string _EMAIL_FILE_APPLICATION =
            Path.Combine(Application.StartupPath, "EmailFile.exe");

        /// <summary>
        /// A tag which will be substituted with the report name if included in the subject
        /// argument.
        /// </summary>
        static readonly string _REPORT_NAME_TAG = "<ReportName>";

        #endregion Constants

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        [SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "_userConfigChecker")]
        static void Main(string[] args)
        {
            string exceptionLogFile = null;
            bool logException = false;
            IExtractReport report = null;
            try
            {
                // Make sure user.config file is not corrupt 
                // related to https://extract.atlassian.net/browse/ISSUE-12830
                UserConfigChecker _userConfigChecker = new UserConfigChecker();

                // These two lines must be called before any window is shown [LRCAU #5261]
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                if (args.Length == 1)
                {
                    // Check for usage flag
                    if (args[0].Equals("/?", StringComparison.OrdinalIgnoreCase))
                    {
                        ShowUsage();
                        return;
                    }
                    // Check for reset flag
                    else if (args[0].Equals("/reset", StringComparison.OrdinalIgnoreCase))
                    {
                        // Set the set flag back to false
                        Settings.Default.ReportViewerUsePersistedSettings = false;
                        Settings.Default.OpenReportUsePersistedSettings = false;
                        MessageBox.Show("Settings have been reset", "Settings Reset",
                            MessageBoxButtons.OK, MessageBoxIcon.Information,
                            MessageBoxDefaultButton.Button1, 0);
                        return;
                    }
                    else
                    {
                        ShowUsage("Unrecognized argument: " + args[0]);
                        return;
                    }
                }
                else if (args.Length < 3 || args.Length > 14)
                {
                    ShowUsage("Incorrect number of arguments!");
                    return;
                }

                // Get the command line arguments
                string serverName = args[0];
                string databaseName = args[1];
                string workflowName = args[2];
                string reportFile = "";
                string outputFile = "";
                List<string> mailRecipients = new List<string>();
                string mailSubject = null;
                string senderAddress = null;
                string senderName = null;
                bool overwrite = false;
                bool prompt = false;
                if (args.Length > 3)
                {
                    for (int i = 4; i < args.Length; i++)
                    {
                        if (args[i].Equals("/f", StringComparison.OrdinalIgnoreCase))
                        {
                            // Check for a file name following /f
                            if ((i + 1) < args.Length)
                            {
                                // If /f then next argument should be file name
                                outputFile = FileSystemMethods.GetAbsolutePath(args[++i]);
                            }
                            else
                            {
                                ShowUsage("/f must be followed by a file name!");
                                return;
                            }
                        }
                        else if (args[i].Equals("/ow", StringComparison.OrdinalIgnoreCase))
                        {
                            if (string.IsNullOrEmpty(outputFile))
                            {
                                ShowUsage("Must specify /f <OutputFile> when using /ow option!");
                                return;
                            }
                            overwrite = true;
                        }
                        else if (args[i].Equals("/prompt", StringComparison.OrdinalIgnoreCase))
                        {
                            prompt = true;
                        }
                        else if (args[i].Equals("/ef", StringComparison.OrdinalIgnoreCase))
                        {
                            logException = true;
                            if ((i + 1) < args.Length)
                            {
                                exceptionLogFile = FileSystemMethods.GetAbsolutePath(args[++i]);
                            }
                        }
                        else if (args[i].Equals("/mailto", StringComparison.OrdinalIgnoreCase))
                        {
                            if ((i + 1) < args.Length)
                            {
                                // Get the recipient string
                                string recipientString = args[++i];

                                // Add the list of mail recipients (tokenize by ',' or ';')
                                foreach (string recipient in
                                    recipientString.Split(new string[] { ",", ";" },
                                    StringSplitOptions.RemoveEmptyEntries))
                                {
                                    // Ensure the recipient contains an @ symbol
                                    if (!recipient.Contains("@"))
                                    {
                                        ExtractException ee = new ExtractException("ELI25078",
                                            "Invalid email recipient! No '@' found.");
                                        ee.AddDebugData("Mail Recipient", recipient, false);
                                        ee.AddDebugData("Recipients String", recipientString, false);
                                        throw ee;
                                    }

                                    mailRecipients.Add(recipient);
                                }
                            }
                            else
                            {
                                ShowUsage("/mailto must be followed by a recipient list!");
                                return;
                            }
                        }
                        else if (args[i].Equals("/subject", StringComparison.OrdinalIgnoreCase))
                        {
                            if ((i + 1) < args.Length)
                            {
                                mailSubject = args[++i];
                            }
                            else
                            {
                                ShowUsage("/subject must be followed by an email subject line!");
                                return;
                            }
                        }
                        else if (args[i].Equals("/senderAddress", StringComparison.OrdinalIgnoreCase))
                        {
                            if ((i + 1) < args.Length)
                            {
                                senderAddress = args[++i];

                                if (!senderAddress.Contains("@"))
                                {
                                    ExtractException ee = new ExtractException("ELI33218",
                                        "Invalid email sender! No '@' found.");
                                    ee.AddDebugData("Mail Recipient", senderAddress, false);
                                    ee.AddDebugData("Recipients String", senderAddress, false);
                                    throw ee;
                                }
                            }
                            else
                            {
                                ShowUsage("/senderAddress must be followed by an email address!");
                                return;
                            }
                        }
                        else if (args[i].Equals("/senderName", StringComparison.OrdinalIgnoreCase))
                        {
                            if ((i + 1) < args.Length)
                            {
                                senderName = args[++i];
                            }
                            else
                            {
                                ShowUsage("/senderName must be followed by the name of the sender!");
                                return;
                            }
                        }
                        else
                        {
                            ShowUsage("Invalid argument: " + args[i]);
                            return;
                        }
                    }

                    // Ensure that if a subject is specified there is at least one mail recipient
                    if (!string.IsNullOrEmpty(mailSubject) && mailRecipients.Count == 0)
                    {
                        ShowUsage("Cannot specify /subject without /mailto <recipient_list>!");
                        return;
                    }
                    // Fourth argument must be report file, attempt to get the report file
                    // from the command line parameter
                    reportFile = GetReportFileName(args[3]);
                }

                LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());

                LicenseUtilities.ValidateLicense(LicenseIdName.FlexIndexIDShieldCoreObjects,
                    "ELI23508", "Report Viewer");

                bool parametersSet = false;

                if (!string.IsNullOrEmpty(reportFile))
                {
                    // Ensure the report file exists and has .repx extension
                    ExtractException.Assert("ELI23509",
                        "Report file does not exist or does not have proper extension!",
                        File.Exists(reportFile) &&
                        Path.GetExtension(reportFile).Equals(".repx", StringComparison.OrdinalIgnoreCase),
                        "Report File Name", reportFile);

                    // Prepare the report object
                    report = ExtractReportUtils.CreateExtractReport(serverName, databaseName, workflowName, reportFile);
                    parametersSet = report.SetParameters(prompt, false);
                }

                // If an output file was specified and the report object exists
                // and all the settings are correct then output the report
                if (!string.IsNullOrEmpty(outputFile)
                    && report != null
                    && parametersSet)
                {
                    report.ExportReportToFile(outputFile, overwrite);
                }
                else if (mailRecipients.Count > 0)
                {
                    var reportName = Path.GetFileNameWithoutExtension(reportFile);

                    string reportPDF = Path.Combine(Path.GetTempPath(), reportName + ".pdf");
                    try
                    {
                        // Export the report to a PDF file
                        report.ExportReportToFile(reportPDF, true);

                        var recipients = string.Join(",", mailRecipients);

                        // Email the report
                        EmailReport(reportPDF, mailSubject, recipients, senderAddress, senderName);
                    }
                    finally
                    {
                        if (File.Exists(reportPDF))
                        {
                            FileSystemMethods.TryDeleteFile(reportPDF);
                        }
                    }
                }
                else
                {
                    // User canceled the initialization of the report object
                    if (report != null && !parametersSet)
                    {
                        // Dispose of the report and set it to null
                        report.Dispose();
                        report = null;
                    }

                    var reportViewer = ExtractReportUtils.CreateReportViewerForm(report,
                        serverName, databaseName, workflowName);

                    Application.Run(reportViewer);
                }
            }
            catch (Exception ex)
            {
                // Check if logging or displaying exceptions
                if (logException)
                {
                    ex.AsExtract("ELI23510").Log(exceptionLogFile);
                }
                else
                {
                    ex.ExtractDisplay("ELI51908");
                }
            }
            finally
            {
                // Ensure the settings get saved
                Settings.Default.Save();

                // Dispose of the report if it is not null
                if (report != null)
                {
                    report.Dispose();
                    report = null;
                }
            }
        }

        /// <overloads>Displays the usage message.</overloads>
        /// <summary>
        /// Displays the usage message.
        /// </summary>
        private static void ShowUsage()
        {
            ShowUsage(null);
        }

        /// <summary>
        /// Displays the usage message prepended with the specified error message.
        /// </summary>
        /// <param name="errorMessage">The error message to display before the usage message.</param>
        private static void ShowUsage(string errorMessage)
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
            usage.AppendLine(" /? | /reset | <ServerName> <DatabaseName> <WorkflowName> "
                + "[<ReportFile> "
                + "/f <OutputPDFName> /ow /prompt /mailto <recipient_list> /subject <mail_subject> "
                + "/senderAddress <sender_address> /senderName <sender_name> "
                + "/ef (ExceptionLogFile)]");
            usage.AppendLine();
            usage.AppendLine("Usage:");
            usage.AppendLine("-----------------");
            usage.AppendLine("<ServerName> - The name of the database server to connect to");
            usage.AppendLine();
            usage.AppendLine("<DatabaseName> - The name of the database to connect to");
            usage.AppendLine();
            usage.AppendLine("<WorkflowName> - The name of the workflow to use for the report");
            usage.AppendLine();
            usage.AppendLine("<ReportFile> - The path to the report file to run");
            usage.AppendLine();
            usage.AppendLine("/f <OutputPDFName> - The path to the pdf file that will be output");
            usage.AppendLine();
            usage.AppendLine("/ow - Overwrite the output file if it already exists");
            usage.AppendLine();
            usage.AppendLine("/prompt - Forces a prompt for parameter values");
            usage.AppendLine();
            usage.AppendLine("/mailto <recipient_list> - Comma separated list of report mail recipients");
            usage.AppendLine();
            usage.AppendLine("/subject <mail_subject> - Subject line of report email that will be");
            usage.AppendLine("    generated. Subject line can only be specified if /mailto is");
            usage.AppendLine("    specified. If subject includes \"" + _REPORT_NAME_TAG + "\"");
            usage.AppendLine("    it will be replaced with the name of the report.");
            usage.AppendLine();
            usage.AppendLine("/senderAddress <sender_address> - The address the email should");
            usage.AppendLine("    appear to come from. Sender address can only be specified if");
            usage.AppendLine("    /mailto is specified. If not specified, the setting from the");
            usage.AppendLine("    general Extract email settings will be used.");
            usage.AppendLine();
            usage.AppendLine("/senderName <sender_name> - The sender name the email should");
            usage.AppendLine("    appear to come from. Sender name can only be specified if");
            usage.AppendLine("    /mailto is specified. If not specified, the setting from the");
            usage.AppendLine("    general Extract email settings will be used.");
            usage.AppendLine();

            usage.AppendLine("/ef <ExceptionLogFile> - Log exceptions to the specified file");
            usage.AppendLine();
            usage.AppendLine("/? - Display usage");
            usage.AppendLine();
            usage.AppendLine("/reset - Resets report viewer dialogs to default size and location");
            usage.AppendLine();

            // Display the usage as an error or as an information box
            MessageBox.Show(usage.ToString(), isError ? "Error" : "Usage", MessageBoxButtons.OK,
                isError ? MessageBoxIcon.Error : MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1, 0);
        }

        /// <summary>
        /// Emails the specified report file to the specified recipient(s) with the specified
        /// subject.
        /// </summary>
        /// <param name="fileName">The name of the file to email.</param>
        /// <param name="subject">The subject for the email.</param>
        /// <param name="recipient">The recipient(s) for the email.</param>
        /// <param name="senderAddress">The address the email should appear to come from.
        /// If <see langword="null"/>, the setting from the general Extract email settings will be
        /// used.</param>
        /// <param name="senderName">The sender name the email should appear to come from.
        /// If <see langword="null"/>, the setting from the general Extract email settings will be
        /// used.</param>
        private static void EmailReport(string fileName, string subject, string recipient,
            string senderAddress, string senderName)
        {
            try
            {
                var arguments = new List<string>();
                arguments.Add(recipient);
                arguments.Add(fileName);

                // If the subject has not been specified, use the report name as the email subject.
                if (string.IsNullOrWhiteSpace(subject))
                {
                    subject = _REPORT_NAME_TAG;
                }

                // Substitute <ReportName> with the name of the report.
                subject = subject.Replace(_REPORT_NAME_TAG, Path.GetFileNameWithoutExtension(fileName));

                arguments.Add("/subject");
                arguments.Add(subject);

                // Override the default sender address and name (if specified).
                if (!string.IsNullOrWhiteSpace(senderAddress))
                {
                    arguments.Add("/senderAddress");
                    arguments.Add(senderAddress);
                }

                if (!string.IsNullOrWhiteSpace(senderName))
                {
                    arguments.Add("/senderName");
                    arguments.Add(senderName);
                }

                SystemMethods.RunExtractExecutable(_EMAIL_FILE_APPLICATION,
                    arguments);
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI25085", ex);
                ee.AddDebugData("File To Email", fileName, false);
                ee.AddDebugData("Subject", subject ?? "null", false);
                ee.AddDebugData("Recipient", recipient, false);
                throw ee;
            }
        }

        /// <summary>
        /// Checks the report file argument and tries to find it in either the standard
        /// or saved reports folder if it is not an absolute path.  Will throw an exception
        /// if the file is not an absolute path and there is no standard/saved report with
        /// the same name.
        /// </summary>
        /// <param name="reportFile">The report file to build the path for.</param>
        /// <returns>The absolute path to the report file</returns>
        // [LRCAU #5177]
        private static string GetReportFileName(string reportFile)
        {
            // First check if this is an absolute path
            if (Path.IsPathRooted(reportFile))
            {
                return reportFile;
            }
            // Check if the path is a relative path
            else if (reportFile.Contains("\\"))
            {
                ExtractException ee = new ExtractException("ELI25107",
                    "Report file must be a fully qualified path or name of Standard or Saved report.");
                ee.AddDebugData("Report File Name", reportFile, false);
                throw ee;
            }
            // Look for a standard or saved report with the same name
            else
            {
                // Check for standard report
                string tempReport = Path.Combine(ExtractReportUtils.StandardReportFolder, reportFile) + ".repx";
                if (File.Exists(tempReport))
                {
                    return tempReport;
                }

                // Check for saved report
                tempReport = Path.Combine(ExtractReportUtils.SavedReportFolder, reportFile) + ".repx";
                if (File.Exists(tempReport))
                {
                    return tempReport;
                }

                ExtractException ee = new ExtractException("ELI25102",
                    "Specified report cannot be found!");
                ee.AddDebugData("Report Name", reportFile, false);
                throw ee;
            }
        }
    }
}