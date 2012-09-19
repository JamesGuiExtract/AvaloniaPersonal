using Extract.Licensing;
using System;
using System.Text;
using System.Windows.Forms;

namespace Extract.IDShieldStatisticsReporter
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            IDShieldStatisticsReporterForm reporterForm = null;

            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                // Validate the license
                LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());
                LicenseUtilities.ValidateLicense(LicenseIdName.FlexIndexIDShieldCoreObjects,
                    "ELI28533", "ID Shield Statistics Reporter");

                reporterForm = new IDShieldStatisticsReporterForm();

                // Parse the command-line arguments to initialize the program settings.
                if (!ParseArgs(args, reporterForm))
                {
                    // If the settings could not be parsed, no file can be processed; return.
                    return;
                }

                Application.Run(reporterForm);
            }
            catch (Exception ex)
            {
                ExtractException ee = ExtractException.AsExtractException("ELI28536", ex);

                if (reporterForm != null && !string.IsNullOrEmpty(reporterForm.LogFileName))
                {
                    ee.ExtractLog("ELI34959", reporterForm.LogFileName);
                }
                else if (reporterForm != null && reporterForm.Silent)
                {
                    ee.ExtractLog("ELI34960");
                }
                else
                {
                    ee.Display();
                }
            }
        }

        /// <summary>
        /// Attempts to initialize the program's fields using the specified arguments.
        /// </summary>
        /// <param name="args">The command-line arguments to parse.</param>
        /// <param name="reporterForm"></param>
        static bool ParseArgs(string[] args, IDShieldStatisticsReporterForm reporterForm)
        {
            if (args.Length == 1 && args[0].Equals("/?", StringComparison.Ordinal))
            {
                ShowUsage(null);
                return false;
            }

            if (args.Length > 15)
            {
                ShowUsage("Invalid number of command-line arguments.");
                return false;
            }

            bool easyMode = false;

            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].Equals("/Easy", StringComparison.OrdinalIgnoreCase))
                {
                    easyMode = true;
                    reporterForm.TesterSettings.OutputHybridStats = false;
                    reporterForm.TesterSettings.OutputAutomatedStatsOnly = true;
                    reporterForm.TesterSettings.QueryForAutomatedRedaction = "HCData|MCData|LCData|Manual";
                    reporterForm.TesterSettings.AutomatedConditionQuantifier = "any";
                    reporterForm.TesterSettings.AutomatedCondition = "HCData|MCData|LCData|Clues|Manual";
                }
                else if (args[i].Equals("/AutoRun", StringComparison.OrdinalIgnoreCase))
                {
                    reporterForm.AutoRun = true;
                }
                else if (args[i].Equals("/AutoRunSilent", StringComparison.OrdinalIgnoreCase))
                {
                    reporterForm.AutoRun = true;
                    reporterForm.Silent = true;
                }
                else if (args[i].Equals("/FeedbackDataFolder", StringComparison.OrdinalIgnoreCase))
                {
                    if (i + 1 >= args.Length)
                    {
                        ShowUsage("Feedback folder name not specified.");
                        return false;
                    }

                    i++;
                    reporterForm.TestFolder.TestFolderName = args[i];
                }
                else if (args[i].Equals("/StatisticsOutputFolder", StringComparison.OrdinalIgnoreCase))
                {
                    if (i + 1 >= args.Length)
                    {
                        ShowUsage("Statistics Output folder name not specified.");
                        return false;
                    }

                    i++;
                    reporterForm.TesterSettings.ExplicitOutputFilesFolder = args[i];
                }
                else if (args[i].Equals("/CustomReport", StringComparison.OrdinalIgnoreCase))
                {
                    if (i + 1 >= args.Length)
                    {
                        ShowUsage("Custom report template file not specified.");
                        return false;
                    }

                    i++;
                    reporterForm.CustomReportTemplate = args[i];
                }
                else if (args[i].Equals("/Print", StringComparison.OrdinalIgnoreCase))
                {
                    if (i + 1 >= args.Length)
                    {
                        ShowUsage("Report to print not specified.");
                        return false;
                    }

                    i++;
                    reporterForm.ReportToPrint = args[i];
                }
                else if (args[i].Equals("/ef", StringComparison.Ordinal))
                {
                    if (i + 1 >= args.Length)
                    {
                        ShowUsage("Log file argument required.");
                        return false;
                    }

                    i++;
                    reporterForm.LogFileName = args[i];
                }
                else
                {
                    ShowUsage("Unrecognized command-line argument: \"" + args[i] + "\"");
                    return false;
                }
            }

            ExtractException.Assert("ELI34961",
                "/Easy and /FeedbackDataFolder switches must be used with /AutoRun switch",
                !reporterForm.AutoRun ||
                (easyMode && !string.IsNullOrEmpty(reporterForm.TestFolder.TestFolderName)));

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
                usage.AppendLine("Evaluates capture rates for ID Shield and reports the results.");
                usage.AppendLine();
            }

            usage.Append(Environment.GetCommandLineArgs()[0]);
            usage.AppendLine(" [/Easy] [/AutoRun | /AutoRunSilent] [/FeedbackDataFolder <folder>]");
            usage.AppendLine("[/StatisticsOutputFolder <folder>] [/Print <report name>]");
            usage.AppendLine("[/CustomReport <template>] [/Print <report name>] [/ef <filename>]");
            usage.AppendLine();

            usage.AppendLine("/Easy: Automatically configure for comparing two sets of VOA files.");
            usage.AppendLine();

            usage.AppendLine("/AutoRun: Simulates clicking the analyze button immediately after");
            usage.AppendLine("opening; requires both /FeedbackDataFolder and /Easy switches.");
            usage.AppendLine();

            usage.AppendLine("/AutoRunSilent: Same as /AutoRun except that the analysis is run");
            usage.AppendLine("without displaying the UI or any message boxes and the application will");
            usage.AppendLine("then immediately exit. Exceptions will be logged instead of displayed.");
            usage.AppendLine();

            usage.AppendLine("/FeedbackDataFolder: Specifies the feedback folder to use.");
            usage.AppendLine();

            usage.AppendLine("/StatisticsOutputFolder: Specifies a folder all report text files");
            usage.AppendLine("should be written to instead of a time-stamped analysis folder");
            usage.AppendLine("under the FeedbackDataFolder.");
            usage.AppendLine();

            usage.AppendLine("/Print: Prints the specified report whenever analysis is performed");
            usage.AppendLine("(automatically or otherwise).");
            usage.AppendLine();

            usage.AppendLine("/CustomReport: Generates a custom report and uses it as the default");
            usage.AppendLine("report to display. The template can use any of the following parameters");
            usage.AppendLine("enclosed in parentheses as well as these mathematical operators");
            usage.AppendLine("inserted between them without spaces: +, -, *, / and % where %");
            usage.AppendLine("performs division and presents it as a percentage, including the % sign.");
            usage.AppendLine("Available Parameters:  TotalExpectedRedactions,");
            usage.AppendLine("NumCorrectRedactions,TotalNumberOfCorrectRedactions, NumOverRedactions,");
            usage.AppendLine("NumUnderRedactions,NumMisses, TotalFilesProcessed,");
            usage.AppendLine("NumFilesWithExpectedRedactions, NumFilesSelectedForReview,");
            usage.AppendLine("NumFilesAutomaticallyRedacted, NumExpectedRedactionsInReviewedFiles,");
            usage.AppendLine("NumExpectedRedactionsInRedactedFiles, NumFilesWithExistingVOA,");
            usage.AppendLine("NumFilesWithOverlappingExpectedRedactions, TotalPages,");
            usage.AppendLine("NumPagesWithExpectedRedactions, DocsClassified,");
            usage.AppendLine("AutomatedTotalExpectedRedactions,");
            usage.AppendLine("AutomatedExpectedRedactionsInSelectedFiles,");
            usage.AppendLine("AutomatedFoundRedactions, AutomatedNumCorrectRedactions,");
            usage.AppendLine("AutomatedNumFalsePositives, AutomatedNumOverRedactions,");
            usage.AppendLine("AutomatedNumUnderRedactions, AutomatedNumMisses,");
            usage.AppendLine("VerificationTotalExpectedRedactions,");
            usage.AppendLine("VerificationExpectedRedactionsInSelectedFiles,");
            usage.AppendLine("VerificationFoundRedactions, VerificationNumCorrectRedactions,");
            usage.AppendLine("VerificationNumFalsePositives, VerificationNumOverRedactions,");
            usage.AppendLine("VerificationNumUnderRedactions, VerificationNumMisses");
            usage.AppendLine();

            usage.AppendLine("/ef: Log exceptions to the specified file.");
            usage.AppendLine();

            MessageBox.Show(usage.ToString(), isError ? "Error" : "Usage", MessageBoxButtons.OK,
                isError ? MessageBoxIcon.Error : MessageBoxIcon.Information,
                MessageBoxDefaultButton.Button1, 0);
        }
    }
}