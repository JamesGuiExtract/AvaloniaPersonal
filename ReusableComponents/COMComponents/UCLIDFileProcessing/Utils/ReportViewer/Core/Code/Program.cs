using Extract;
using Extract.Licensing;
using Extract.ReportViewer.Properties;
using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private static readonly string _EMAIL_FILE_APPLICATION = "EmailFile.exe";

        /// <summary>
        /// The timeout to wait for the email file process to exit
        /// </summary>
        private static readonly int _EMAIL_TIMEOUT = 300000; // 300000 ms = 5 min.

        #endregion Constants

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            string exceptionLogFile = null;
            bool logException = false;
            ExtractReport report = null;
            try
            {
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
                else if (args.Length < 2 || args.Length > 9)
                {
                    ShowUsage("Incorrect number of arguments!");
                    return;
                }

                // Get the command line arguments
                string serverName = args[0];
                string databaseName = args[1];
                string reportFile = "";
                string outputFile = "";
                List<string> mailRecipients = new List<string>();
                string mailSubject = null;
                bool overwrite = false;
                bool prompt = false;
                if (args.Length > 2)
                {
                    // Third argument must be report file, attempt to get the report file
                    // from the command line parameter
                    reportFile = GetReportFileName(args[2]);

                    for (int i = 3; i < args.Length; i++)
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
                }

                LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());

                LicenseUtilities.ValidateLicense(LicenseIdName.FlexIndexIDShieldCoreObjects,
                    "ELI23508", "Crystal Report Viewer");

                if (!string.IsNullOrEmpty(reportFile))
                {
                    // Ensure the report file exists and has .rpt extension
                    ExtractException.Assert("ELI23509",
                        "Report file does not exist or does not have proper extension!",
                        File.Exists(reportFile) &&
                        Path.GetExtension(reportFile).Equals(".rpt", StringComparison.OrdinalIgnoreCase),
                        "Report File Name", reportFile);

                    // Prepare the report object
                    report = new ExtractReport(serverName, databaseName, reportFile, prompt);
                }

                // If an output file was specified and the report object exists
                // and all the settings are correct then output the report
                if (!string.IsNullOrEmpty(outputFile)
                    && report != null
                    && !report.CanceledInitialization)
                {
                    report.ExportReportToFile(outputFile, overwrite);
                }
                else if (mailRecipients.Count > 0)
                {
                    string reportPDF = Path.Combine(Path.GetTempPath(), "Report.pdf");
                    try
                    {
                        // Export the report to a PDF file
                        report.ExportReportToFile(reportPDF, true);

                        // Build the list of email recipients
                        StringBuilder sb = new StringBuilder(mailRecipients[0]);
                        for (int i=1; i < mailRecipients.Count; i++)
                        {
                            sb.Append(",");
                            sb.Append(mailRecipients[i]);
                        }

                        // Email the report
                        EmailReport(reportPDF, mailSubject ?? "", sb.ToString());
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
                    if (report != null && report.CanceledInitialization)
                    {
                        // Dispose of the report and set it to null
                        report.Dispose();
                        report = null;
                    }

                    ReportViewerForm reportViewer = new ReportViewerForm(report,
                        serverName, databaseName);

                    Application.Run(reportViewer);
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI23510", ex);

                // Check if logging or displaying exceptions
                if (logException)
                {
                    ee.Log(exceptionLogFile);
                }
                else
                {
                    ee.Display();
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
            usage.AppendLine(" /? | /reset | <ServerName> <DatabaseName> [<CrystalReportFile> "
                + "/f <OutputPDFName> /ow /prompt /mailto <recipient_list> /subject <mail_subject> "
                + "/ef (ExceptionLogFile)");
            usage.AppendLine();
            usage.AppendLine("Usage:");
            usage.AppendLine("-----------------");
            usage.AppendLine("<ServerName> - The name of the database server to connect to");
            usage.AppendLine();
            usage.AppendLine("<DatabaseName> - The name of the database to connect to");
            usage.AppendLine();
            usage.AppendLine("<CrystalReportFile> - The path to the crystal report file to run");
            usage.AppendLine();
            usage.AppendLine("/f <OutputPDFName> - The path to the pdf file that will be output");
            usage.AppendLine();
            usage.AppendLine("/ow - Overwrite the output file if it already exists");
            usage.AppendLine();
            usage.AppendLine("/prompt - Forces a prompt for parameter values");
            usage.AppendLine();
            usage.AppendLine("/mailto <recipient_list> - Comma separated list of report mail recipients");
            usage.AppendLine();
            usage.AppendLine("/subject <mail_subject> - Subject line of report email that will be generated");
            usage.AppendLine("                          Subject line can only be specified if /mailto is specified"); 
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
        private static void EmailReport(string fileName, string subject, string recipient)
        {
            string uexFile = null;
            try
            {
                // Get temporary file for logging any exception email file throws
                uexFile = FileSystemMethods.GetTemporaryFileName("uex");

                // Build arguments for email file application
                string emailFile = Path.Combine(Application.StartupPath, _EMAIL_FILE_APPLICATION);
                string arguments = "/subject \"" + (subject ?? "") + "\" \""
                    + Path.GetFileName(fileName) + "\" \"" + recipient + "\" /ef \"" + uexFile + "\"";

                // Build process information structure to launch the email file application
                ProcessStartInfo processInfo = new ProcessStartInfo();
                processInfo.FileName = emailFile;
                processInfo.Arguments = arguments;
                processInfo.WorkingDirectory = Path.GetDirectoryName(fileName);
                processInfo.WindowStyle = ProcessWindowStyle.Hidden;

                // Start the email file application
                Process process = Process.Start(processInfo);

                // Wait for the application to exit
                process.WaitForExit(_EMAIL_TIMEOUT);

                // If the timeout was exceeded but the process is still running then
                // kill the process and throw an exception
                if (!process.HasExited)
                {
                    // Attempt to kill the email file program
                    process.Kill();

                    // Throw an exception
                    ExtractException eex =
                        new ExtractException("ELI25086", "Email file exceeded timeout!");
                    eex.AddDebugData("Timeout Value", _EMAIL_TIMEOUT, false);
                    throw eex;
                }

                // Ensure a uex file has been defined
                if (!string.IsNullOrEmpty(uexFile))
                {
                    // Get the length of the file (if it is empty then no exception was logged)
                    FileInfo fileInfo = new FileInfo(uexFile);
                    if (fileInfo.Length > 0)
                    {
                        // Get the exception from the file and throw it
                        string[] exceptionText = (File.ReadAllText(uexFile)).Split(
                            new string[] {","}, StringSplitOptions.RemoveEmptyEntries);
                        ExtractException uex = new ExtractException("ELI25084",
                            "Error in EmailFile.exe!",
                            exceptionText[exceptionText.Length - 1].Trim());
                        throw uex;
                    }
                }
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI25085", ex);
                ee.AddDebugData("File To Email", fileName, false);
                ee.AddDebugData("Subject",
                    string.IsNullOrEmpty(subject) ? "null" : subject, false);
                ee.AddDebugData("Recipient", recipient, false);
                throw ee;
            }
            finally
            {
                // Cleanup the uex file if it exists
                if (!string.IsNullOrEmpty(uexFile))
                {
                    FileSystemMethods.TryDeleteFile(uexFile);
                }
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
                string tempReport = Path.Combine(ExtractReport.StandardReportFolder, reportFile) + ".rpt";
                if (File.Exists(tempReport))
                {
                    return tempReport;
                }

                // Check for saved report
                tempReport = Path.Combine(ExtractReport.SavedReportFolder, reportFile) + ".rpt";
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