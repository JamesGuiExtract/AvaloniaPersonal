using Extract;
using Extract.DataCaptureStats;
using Extract.Licensing;
using Extract.Utilities;
using StatisticsReporter.Properties;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;

namespace StatisticsReporter
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                // Validate the license
                LicenseUtilities.LoadLicenseFilesFromFolder(0, new MapLabel());
                LicenseUtilities.ValidateLicense(LicenseIdName.FlexIndexIDShieldCoreObjects,
                                                 "ELI41500",
                                                 "Statistics Reporter");

                // The config file must be specified on the command line
                if (args.Length == 0)
                {
                    Usage();
                    throw new ExtractException("ELI41542", "Config file must be specified on the command line.");
                }

                string configFileName =  args[0];
                ConfigSettings<Settings> ReportSettings = new ConfigSettings<Settings>(configFileName);

                // Verify that there are settings that make sense
                ExtractException.Assert("ELI41543", "Must specify ReportOutputFileName in config file.", !string.IsNullOrWhiteSpace(ReportSettings.Settings.ReportOutputFileName));

                // Transfer settings to the DataCapture settings
                DataCaptureSettings dcSettings = new DataCaptureSettings();
                
                dcSettings.DatabaseName = ReportSettings.Settings.DatabaseName;
                dcSettings.DatabaseServer = ReportSettings.Settings.DatabaseServerName;
                dcSettings.IncludeFilesIfNoExpectedVOA = ReportSettings.Settings.IncludeFilesIfNoExpectedVoa;
                dcSettings.FileSettings.TypeOfStatistics = ReportSettings.Settings.TypeOfStatistics;
                dcSettings.FileSettings.XPathToIgnore = ReportSettings.Settings.XPathOfAttributesToIgnore;
                dcSettings.FileSettings.XPathOfContainerOnlyAttributes = ReportSettings.Settings.XPathOfContainerOnlyAttributes;
                dcSettings.ExpectedAttributeSetName = ReportSettings.Settings.ExpectedAttributeSetName;
                dcSettings.FoundAttributeSetName = ReportSettings.Settings.FoundAttributeSetName;
                dcSettings.StartDate = DateTime.Parse( ReportSettings.Settings.StartDateTime, CultureInfo.CurrentCulture);
                dcSettings.EndDate = DateTime.Parse(ReportSettings.Settings.EndDateTime, CultureInfo.CurrentCulture);

                //// Create the Process Statistics 
                ProcessStatistics Statistics = new ProcessStatistics(dcSettings);

                // Set the delegates
                Statistics.PerFileAction = AttributeTreeComparer.CompareAttributes;

                // Start time to show how much time the processing took
                Stopwatch timeToProcess = new Stopwatch();
                timeToProcess.Start();

                // Get the results for the per file comparisons
                var Results = Statistics.ProcessData();
                
                // Aggregate the results
                var AggrgatedResults = Results.AggregateStatistics();

                // Summarize the data
                var SummarizedResults = AggrgatedResults.SummarizeStatistics(false);

                // Save the data to the configure output file
                File.WriteAllText(ReportSettings.Settings.ReportOutputFileName, SummarizedResults.AccuracyDetailsToHtml());

                // Stop the timer and write the elapsed time
                timeToProcess.Stop();
                Console.WriteLine("Elapsed time {0}", timeToProcess.Elapsed);
            }
            catch (Exception e)
            {
                e.AsExtract("ELI41525").Log();
                Console.Error.WriteLine(e.Message);
                Environment.ExitCode = 1;
            }
        }


        /// <summary>
        /// Display the usage on the command line
        /// </summary>
        static void Usage()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("\tStatisticsReporter <ConfigFileName>");
            Console.WriteLine("\t\t<ConfigFileName - File containing the config settings for the report.");
        }
    }
}
