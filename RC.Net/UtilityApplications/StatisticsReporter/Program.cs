using Extract;
using Extract.DataCaptureStats;
using Extract.Licensing;
using Extract.Utilities;
using StatisticsReporter.Properties;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.UI;
using GroupByCriterion = Extract.Utilities.Union<
    StatisticsReporter.GroupByDBField,
    StatisticsReporter.GroupByFoundXPath,
    StatisticsReporter.GroupByExpectedXPath>;

namespace StatisticsReporter
{
    class Program
    {
        /// <summary>
        /// The default CSS style sheet (these settings can be overridden
        /// by adding rules to the config file)
        /// </summary>
        private const string _DEFAULT_STYLE = @"
            heading summary { background: #f2f2f2; }
                    H1 { font-size: 1.5em; }
                    H3
                    {
                      font-size: 1em;
                      font-weight: normal;
                    }
            thead { background: DarkSeaGreen; }
            tfoot { border-top: 2px solid gray; }
            tr:nth-child(even) { background: #f2f2f2; }
            table.ReportSettings th { text-align: left; }
            table.ReportSettings td { text-align: left; }
            table.DataCaptureStats
            {
              border-spacing: 10px;
              border-collapse: collapse;
              margin-top: 20px;
              margin-bottom: 20px;
            }
            table.DataCaptureStats th { text-align: left; }
            table.DataCaptureStats td { text-align: right; }
            table.DataCaptureStats thead th { border-left: 2px solid gray; }
            table.DataCaptureStats caption { text-align: left; }";
			
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

                string configFileName = args[0];
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

                // Set/validate the tagged criteria
                try
                {
                    dcSettings.Tagged = ReportSettings.Settings.Tagged
                        ?.Cast<string>()
                        .Select(s =>
                        {
                            // This equals requirements of CFileProcessingDB::validateTagName()
                            ExtractException.Assert("ELI41897", "Invalid tag name",
                                Regex.IsMatch(s, @"\A\w[\w\s]{0,99}\z"));

                            return s;
                        }).ToList()
                        ?? Enumerable.Empty<string>();
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI41898");
                }

                // Set/validate the group-by criteria
                try
                {
                    dcSettings.GroupByCriteria = ReportSettings.Settings.GroupByCriteria
                        ?.Cast<string>()
                        .Select(s =>
                        {
                            using (var csvReader = new Microsoft.VisualBasic.FileIO.TextFieldParser
                                (new StringReader(s)) { Delimiters = new[] { "," } })
                            {
                                string[] tokens = csvReader.ReadFields();
                                ExtractException.Assert("ELI41904", "At least two tokens are required (Label, GroupByCriterion)",
                                    tokens.Length > 1);

                                string label = tokens[0];
                                string criterion = tokens[1];
                                string additionalInfo = tokens.Length > 2 ? tokens[2] : null;
                                if (additionalInfo != null
                                    && criterion.Equals("ExpectedXPath", StringComparison.OrdinalIgnoreCase)
                                    && UtilityMethods.IsValidXPathExpression(additionalInfo, throwException: true))
                                {
                                    return new GroupByCriterion(new GroupByExpectedXPath
                                    {
                                        Label = label,
                                        XPath = additionalInfo
                                    });
                                }
                                if (additionalInfo != null
                                    && criterion.Equals("FoundXPath", StringComparison.OrdinalIgnoreCase)
                                    && UtilityMethods.IsValidXPathExpression(additionalInfo, throwException: true))
                                {
                                    return new GroupByCriterion(new GroupByFoundXPath
                                    {
                                        Label = label,
                                        XPath = additionalInfo
                                    });
                                }

                                GroupByDBField e = GroupByDBField.None;
                                if (!Enum.TryParse<GroupByDBField>(criterion, out e) | e == GroupByDBField.None)
                                {
                                    var ee = new ExtractException("ELI41842", "Unknown group-by criterion");
                                    ee.AddDebugData("Group by criterion", criterion, false);
                                    throw ee;
                                }
                                if (string.IsNullOrWhiteSpace(label))
                                {
                                    label = e.ToString();
                                }
                                e.SetReadableValue(label);
                                return new GroupByCriterion(e);
                            }
                        })
                        .ToList()
                        ?? Enumerable.Empty<GroupByCriterion>();
                }
                catch (Exception ex)
                {
                    throw new ExtractException("ELI41843", "Bad group-by criterion", ex);
                }

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

                // Get the results for the per file comparisons
                var Results = Statistics.ProcessData();

                // Aggregate/summarize the results
                foreach (var group in Results)
                {
                    group.AccuracyDetails =
                        group.AccuracyDetails.AggregateStatistics()
                        .SummarizeStatistics(ReportSettings.Settings.ErrorIfContainerOnlyConflict);
                }

                if (ReportSettings.Settings.ReportOutputFileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
                {
                    var statsData = Results.AccuracyDetailsToCsv();

                    // Save the data to the configured output file
                    File.WriteAllText(ReportSettings.Settings.ReportOutputFileName, statsData);
                }
                else
                {
                    var statsData = Results.Select(group => group.AccuracyDetailsToHtml());
                    var groupByFieldLabels = Results.FirstOrDefault()?.GroupByNames ?? new string [0];

                    // Get the Html page to save
                    string HtmlOutputPage = CreateHtmlPage(statsData, ReportSettings, range, groupByFieldLabels);

                    // Save the data to the configured output file
                    File.WriteAllText(ReportSettings.Settings.ReportOutputFileName, HtmlOutputPage);
                }

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
                        string HtmlOutput = CreateHtmlPage(Enumerable.Empty<string>(), ReportSettings, range, new string[0]);
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
        static string CreateHtmlPage(IEnumerable<string> statsData, ConfigSettings<Settings> reportSettings,
            Tuple<DateTime, DateTime> range, string[] groupByFieldLabels)
        {
            var baseWriter = new StringWriter(CultureInfo.InvariantCulture);
            using (var writer = new HtmlTextWriter(baseWriter))
            {
                writer.RenderBeginTag(HtmlTextWriterTag.Html);

                // Header for the Html
                writer.RenderBeginTag(HtmlTextWriterTag.Head);
                writer.AddAttribute(HtmlTextWriterAttribute.Type, "text/css");
                writer.RenderBeginTag(HtmlTextWriterTag.Style);
                // Default CSS
                writer.WriteLine(_DEFAULT_STYLE);
                // Optional, config-specified CSS
                writer.Write(reportSettings.Settings.CSS);
                writer.RenderEndTag();
                writer.RenderEndTag();

                // Write header
                WriteHeading(writer, reportSettings, range, groupByFieldLabels);

                // Write statistics tables
                writer.WriteFullBeginTag("section class=\"Statistics\"");
                var tables = statsData.ToList();
                for (int i = 0; i < tables.Count; i++)
                {
                    writer.Write(tables[i]);

                    // Add a line between the tables over the entire width of the pages
                    if (i + 1 < tables.Count)
                    {
                        writer.AddAttribute(HtmlTextWriterAttribute.Class, "StatsTableSeparator");
                        writer.AddAttribute(HtmlTextWriterAttribute.Width, "100%");
                        writer.RenderBeginTag(HtmlTextWriterTag.Hr);
                        writer.RenderEndTag(); // Hr
                    }
                }
                writer.WriteEndTag("section");

                writer.RenderEndTag(); // Html
                return baseWriter.ToString();
            }
        }

        /// <summary>
        /// Writes the heading section, including settings.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="settings">The settings.</param>
        /// <param name="range">The range.</param>
        private static void WriteHeading(HtmlTextWriter writer, ConfigSettings<Settings> settings,
            Tuple<DateTime, DateTime> range, string[] groupByFieldLabels)
        {
            var dateRange = string.Format(CultureInfo.CurrentCulture,
                "Date range: {0:MM/dd/yyyy hh:mm:ss tt}&ndash;{1:MM/dd/yyyy hh:mm:ss tt}", range.Item1, range.Item2);
            var groupedBy = string.Format(CultureInfo.CurrentCulture,
                "Statistics grouped by: {0}",
                    string.Join("; ", groupByFieldLabels));
            writer.WriteFullBeginTag("heading");
            writer.WriteFullBeginTag("details");
            writer.WriteFullBeginTag("summary");
            writer.RenderBeginTag(HtmlTextWriterTag.H1);
            writer.WriteEncodedText(settings.Settings.Header1);
            writer.RenderEndTag(); // H1
            writer.RenderBeginTag(HtmlTextWriterTag.H3);
            writer.Write(dateRange);
            writer.RenderEndTag(); // H3
            writer.RenderBeginTag(HtmlTextWriterTag.H3);
            writer.WriteEncodedText(groupedBy);
            writer.RenderEndTag(); // H3
            writer.WriteEndTag("summary");
            WriteSettingsTable(writer, settings, range);
            writer.WriteEndTag("details");
            writer.WriteEndTag("heading");
        }

        /// <summary>
        /// Writes the settings table.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="reportSettings">The report settings.</param>
        /// <param name="range">The date range.</param>
        private static void WriteSettingsTable(HtmlTextWriter writer,
            ConfigSettings<Settings> reportSettings,
            Tuple<DateTime, DateTime> range)
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
                "ErrorIfContainerOnlyConflict",
                "Tagged",
                "GroupByCriteria",
            };

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
                else if (setting.Name.Equals("Tagged"))
                {
                    Value = setting.PropertyValue == null
                        ? ""
                        : string.Join(", ", ((StringCollection)setting.PropertyValue).Cast<string>());
                }
                else if (setting.Name.Equals("GroupByCriteria"))
                {
                    Value =  setting.PropertyValue == null
                        ? ""
                        : string.Join("; ", ((StringCollection)setting.PropertyValue).Cast<string>());
                }
                else
                {
                    Value = setting.PropertyValue?.ToString() ?? "";
                }

                AddPropertyToHTML(writer, setting.Name, Value);
            }
            writer.RenderEndTag(); // table
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
            writer.WriteEncodedText(propertyName);
            writer.RenderEndTag(); //Th
            writer.RenderBeginTag(HtmlTextWriterTag.Td);
            writer.WriteEncodedText(propertyValue);
            writer.RenderEndTag(); //Td
            writer.RenderEndTag(); //Tr
        }

    }
}
