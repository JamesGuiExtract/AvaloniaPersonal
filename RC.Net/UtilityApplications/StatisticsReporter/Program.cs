using Extract;
using Extract.DataCaptureStats;
using Extract.Licensing;
using Extract.Utilities;
using StatisticsReporter.Properties;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Web.UI;

namespace StatisticsReporter
{
    class Program
    {
        static void Main(string[] args)
        {
            ConfigSettings<Settings> ReportSettings = null;
            Tuple<DateTime, DateTime> range = null;
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
                ReportSettings = new ConfigSettings<Settings>(configFileName);

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

                range = ConvertDateTimeStrings(ReportSettings.Settings.StartDateTime, ReportSettings.Settings.EndDateTime);
                dcSettings.StartDate = range.Item1;
                dcSettings.EndDate = range.Item2;
                
                // Make sure the Start date is less than the end date
                if (dcSettings.StartDate > dcSettings.EndDate)
                {
                    ExtractException ee = new ExtractException("ELI41586", "Report start date is after end date.");
                    ee.AddDebugData("StartDate", dcSettings.StartDate, false);
                    ee.AddDebugData("EndDate", dcSettings.EndDate, false);
                    throw ee;
                }

                // Validate the XPaths
                UtilityMethods.IsValidXPathExpression(dcSettings.FileSettings.XPathOfContainerOnlyAttributes, throwException: true);
                UtilityMethods.IsValidXPathExpression(dcSettings.FileSettings.XPathToIgnore, throwException: true);

                dcSettings.ApplyDatesToFound = ReportSettings.Settings.ApplyDateRangeToFound;

                // Create the Process Statistics 
                ProcessStatistics Statistics = new ProcessStatistics(dcSettings);

                // Set the delegates
                Statistics.PerFileAction = AttributeTreeComparer.CompareAttributes;

                // Start time to show how much time the processing took
                Stopwatch timeToProcess = new Stopwatch();
                timeToProcess.Start();

                long fileCount = 0;

                // Get the results for the per file comparisons
                var Results = Statistics.ProcessData(out fileCount);
                
                // Aggregate the results
                var AggrgatedResults = Results.AggregateStatistics();

                // Summarize the data
                var SummarizedResults = AggrgatedResults.SummarizeStatistics(ReportSettings.Settings.ErrorIfContainerOnlyConflict);

                // Get the Html page to save
                string HtmlOutputPage = CreateHtmlPage(SummarizedResults.AccuracyDetailsToHtml(), ReportSettings, range, fileCount);
                
                // Save the data to the configured output file
                File.WriteAllText(ReportSettings.Settings.ReportOutputFileName, HtmlOutputPage);

                // Stop the timer and write the elapsed time
                timeToProcess.Stop();
                Console.WriteLine("Elapsed time {0}", timeToProcess.Elapsed);
            }
            catch (Exception e)
            {
                e.AsExtract("ELI41525").Log();
                Console.Error.WriteLine(e.Message);
                try
                {
                    if (ReportSettings != null && range != null)
                    {
                        string HtmlOutput = CreateHtmlPage("", ReportSettings, range, 0);
                        // Save the data to the configured output file

                        File.WriteAllText(ReportSettings.Settings.ReportOutputFileName, HtmlOutput);
                    }
                }
                catch(Exception ex)
                {
                    ex.AsExtract("ELI41589").Log();
                }
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

        /// <summary>
        /// Converts the given StartDateString and EndDateString to dates. 
        /// </summary>
        /// <param name="StartDateString">The start date as a parsable date or a date range specifier</param>
        /// <param name="EndDateString">The end date as a parsable date or a date range specifier</param>
        /// <returns>Tuple with two <see cref="DateTime"/> values, the start date as Item1 and end date as Item2</returns>
        static Tuple<DateTime,DateTime> ConvertDateTimeStrings(string StartDateString, string EndDateString)
        {
            DateTime startDate;
            DateTime endDate;
            if (DateTime.TryParse(StartDateString, out startDate))
            {
                // if the start date was a valid date the end date should be a valid date
                if (!DateTime.TryParse(EndDateString, out endDate))
                {
                    ExtractException ee = new ExtractException("ELI41578", "EndDate setting is not a valid date.");
                    ee.AddDebugData("EndDate", EndDateString, false);
                    throw ee;
                }
                return new Tuple<DateTime, DateTime>(startDate, endDate);
            }

            // If start date is not a valid date then it may be a relative to now
            // it is expected that the StartDateString and EndDateString have the same value that specifies the relative range
            if (String.IsNullOrWhiteSpace(StartDateString) || string.IsNullOrWhiteSpace(EndDateString) ||
                StartDateString.ToUpperInvariant() != EndDateString.ToUpperInvariant())
            {
                ExtractException ee = new ExtractException("ELI41579", "StartDate and EndDate are not valid dates or Relative time periods.");
                ee.AddDebugData("StartDate", StartDateString, false);
                ee.AddDebugData("EndDate", EndDateString, false);
                throw ee;
            }

            // Check for All
            if (StartDateString.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                DateTime referenceDate = DateTime.Now;
                startDate = referenceDate.AddYears(-100);
                endDate = referenceDate.AddYears(100);
                return new Tuple<DateTime, DateTime>(startDate, endDate);
            }

            // Any remaining values should be convertible to a TimePeriodRange
            TimePeriodRange timeRange;
            if (!Enum.TryParse(StartDateString, out timeRange))
            {
                ExtractException ee = new ExtractException("ELI41582", "Specified Time Period is invalid.");
                ee.AddDebugData("StartDate", StartDateString, false);
                throw ee;
            }

            return DateTimeMethods.GetDateRangeForTimePeriodRange(timeRange);
        }

        /// <summary>
        /// Creates a HTML page with a table for the report settings and the table contained in the <paramref name="statsData"/> parameter
        /// </summary>
        /// <param name="statsData">The HTML formated table of stats data</param>
        /// <param name="reportSettings">Instance of report settings used to generate report</param>
        /// <param name="range">The Tuple that contains the start and end DateTime for the report</param>
        /// <returns>String that is HTML formated page with the <paramref name="statsData"/> string and other page formatting</returns>
        static string CreateHtmlPage(string statsData, ConfigSettings<Settings> reportSettings, Tuple<DateTime, DateTime> range, long fileCount)
        {
            // The order that settings should be displayed
            List<string> SettingsOrder = new List<string>
            {
                "TypeOfStatistics",
                "DatabaseServerName",
                "DatabaseName",
                "StartDateTime",
                "EndDateTime",
                "IncludeFilesIfNoExpectedVoa",
                "ExpectedAttributeSetName",
                "FoundAttributeSetName",
                "ApplyDateRangeToFound",
                "XPathOfAttributesToIgnore",
                "XPathOfContainerOnlyAttributes",
                "ReportOutputFileName",
                "ErrorIfContainerOnlyConflict"
            };

            var baseWriter = new StringWriter(CultureInfo.InvariantCulture);
            using (var writer = new HtmlTextWriter(baseWriter))
            {
                writer.RenderBeginTag(HtmlTextWriterTag.Html);
                
                // Header for the Html
                writer.RenderBeginTag(HtmlTextWriterTag.Head);
                writer.AddAttribute(HtmlTextWriterAttribute.Type, "text/css");
                writer.RenderBeginTag(HtmlTextWriterTag.Style);
                writer.WriteLine("table.ReportSettings th { text-align: left; }");
                writer.WriteLine("table.ReportSettings td { text-align: left; }");
                writer.WriteLine("table.DataCaptureStats th { text-align: left; }");
                writer.WriteLine("table.DataCaptureStats td { text-align: right; }");
                writer.Write("thead { background: DarkSeaGreen; }");
                writer.RenderEndTag();
                writer.RenderEndTag();

                // Table for configuration
                writer.AddAttribute(HtmlTextWriterAttribute.Class, "ReportSettings");
                writer.RenderBeginTag(HtmlTextWriterTag.Table);

                // Header row
                writer.RenderBeginTag(HtmlTextWriterTag.Thead);
                writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                writer.RenderBeginTag(HtmlTextWriterTag.Th);
                writer.Write("Setting");
                writer.RenderEndTag();// Th
                writer.RenderBeginTag(HtmlTextWriterTag.Th);
                writer.Write("Value");
                writer.RenderEndTag();//Th
                writer.RenderEndTag();//Tr
                writer.RenderEndTag();//Thead

                // Get the settings in order
                foreach (string settingName in SettingsOrder)
                {
                    // Get the setting with the setting name
                    SettingsPropertyValue setting = reportSettings.Settings.PropertyValues[settingName];
                    
                    // If no setting with name was found log exception and continue
                    if (setting == null)
                    {
                        ExtractException ex = new ExtractException("ELI41585", "Setting was not found.");
                        ex.AddDebugData("Setting", settingName, false);
                        ex.Log();
                        continue;
                    }

                    string Value;
                    if (setting.Name.Equals("StartDateTime"))
                    {
                        Value = String.Format(CultureInfo.CurrentCulture, "{0} ({1:MM/dd/yyyy hh:mm:ss tt})", setting.PropertyValue, range.Item1);
                    }
                    else if (setting.Name.Equals("EndDateTime"))
                    {
                        Value = String.Format(CultureInfo.CurrentCulture, "{0} ({1:MM/dd/yyyy hh:mm:ss tt})", setting.PropertyValue, range.Item2);
                    }
                    else
                    {
                        Value = setting.PropertyValue.ToString();
                    }

                    AddPropertyToHTML(writer, setting.Name, Value);
                }
                writer.RenderEndTag(); // table

                // Add a line between the tables over the entire width of the pages
                writer.AddAttribute(HtmlTextWriterAttribute.Width, "100%");
                writer.RenderBeginTag(HtmlTextWriterTag.Hr);
                writer.RenderEndTag(); // Hr

                 writer.Write(statsData);

                // Add a line after the stats tables over the entire width of the pages
                writer.AddAttribute(HtmlTextWriterAttribute.Width, "100%");
                writer.RenderBeginTag(HtmlTextWriterTag.Hr);
                writer.RenderEndTag(); // Hr

                // Add the file count
                writer.RenderBeginTag(HtmlTextWriterTag.P);
                writer.WriteLine("Files processed : {0}", fileCount);
                writer.RenderEndTag(); // p

                writer.RenderEndTag(); // Html
                return baseWriter.ToString();
            }
        }

        /// <summary>
        /// Method adds a row for the property to the <paramref name="writer"/>
        /// </summary>
        /// <param name="writer">The writer that is being used to create the HTML page</param>
        /// <param name="propertyName">Name of property being added</param>
        /// <param name="propertyValue">Value of property being added</param>
        static void AddPropertyToHTML(HtmlTextWriter writer, string propertyName, string propertyValue)
        {
            writer.RenderBeginTag(HtmlTextWriterTag.Tr);
            writer.RenderBeginTag(HtmlTextWriterTag.Th);
            writer.Write(propertyName);
            writer.RenderEndTag(); //Th
            writer.RenderBeginTag(HtmlTextWriterTag.Td);
            writer.Write(propertyValue);
            writer.RenderEndTag(); //Td
            writer.RenderEndTag(); //Tr
        }

    }
}
