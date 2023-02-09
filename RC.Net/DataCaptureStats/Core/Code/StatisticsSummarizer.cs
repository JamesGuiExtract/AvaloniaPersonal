using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web.UI;

namespace Extract.DataCaptureStats
{
    /// <summary>
    /// Class to hold statistics information for a group of files
    /// </summary>
    public class GroupStatistics
    {
        /// <summary>
        /// Gets the file count of the group.
        /// </summary>
        public int FileCount { get; }

        /// <summary>
        /// Gets the array of the names of the group-by fields,
        /// e.g., ExpectedUserName, //LabInfo/Name or FileName
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly")]
        public string[] GroupByNames { get; }

        /// <summary>
        /// Gets the array of the values of the group-by fields,
        /// e.g., nathaniel_heyer, EXTERNAL_LAB or c:\abc.123
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly")]
        public string[] GroupByValues { get; }

        /// <summary>
        /// Gets or sets the accuracy detail collection for this group.
        /// </summary>
        public IEnumerable<AccuracyDetail> AccuracyDetails { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GroupStatistics"/> class.
        /// </summary>
        /// <param name="fileCount">The file count.</param>
        /// <param name="groupByNames">The group-by field names.</param>
        /// <param name="groupByValues">The group-by field values.</param>
        /// <param name="accuracyDetails">The accuracy details.</param>
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly")]
        public GroupStatistics(
            int fileCount,
            string[] groupByNames,
            string[] groupByValues,
            IEnumerable<AccuracyDetail> accuracyDetails)
        {
            FileCount = fileCount;
            GroupByNames = groupByNames;
            GroupByValues = groupByValues;
            AccuracyDetails = accuracyDetails;
        }
    }

    /// <summary>
    /// Class to summarize aggregate data
    /// </summary>
    public static class StatisticsSummarizer
    {
        #region Constants

        private static readonly StringComparison StringComparison = StringComparison.InvariantCultureIgnoreCase;
        private static readonly CultureInfo CultureInfo = CultureInfo.InvariantCulture;

        private static readonly string _CSV_DELIMITER = ",";

        #endregion Constants

        /// <summary>
        /// Computes stats for container-only items.
        /// </summary>
        /// <remarks>Path/label pairs in <see paramref="statisticsToSummarize"/> must be distinct</remarks>
        /// <param name="statisticsToSummarize">The statistics to summarize.</param>
        /// <param name="throwIfContainerOnlyConflict">If set to <c>true</c> will throw an exception if
        /// a path appears for an <see cref="AccuracyDetail"/> with a <see cref="AccuracyDetailLabel.ContainerOnly"/> label
        /// as well as for some other <see cref="AccuracyDetailLabel"/>.
        /// If set to <c>false</c> then conflicting paths will be marked with an asterisk.
        /// <returns>An <see cref="IEnumerable{AccuracyDetail}"/> where any items in the input that were labeled
        /// <see cref="AccuracyDetailLabel.ContainerOnly"/> have been replaced with new <see cref="AccuracyDetail"/>s</returns>
        public static IEnumerable<AccuracyDetail> SummarizeStatistics(this IEnumerable<AccuracyDetail> statisticsToSummarize,
            bool throwIfContainerOnlyConflict = true)
        {
            try
            {
                return new Summarizer().SummarizeStatistics(statisticsToSummarize, throwIfContainerOnlyConflict);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI41530");
            }
        }

        /// <summary>
        /// Generates an HTML report from a group of summarized accuracy details.
        /// </summary>
        /// <param name="group">The <see cref="GroupStatistics"/> data to be used to build the report.</param>
        /// <returns>A string containing an html document representation of the data</returns>
        public static string AccuracyDetailsToHtml(this GroupStatistics group)
        {
            try
            {
                var attributePaths = group.AccuracyDetails
                    .Select(a => a.Path)
                    .Distinct()
                    .OrderBy(p => p)
                    .ToList();
                var attributeColumns = ExpandedAccuracy.GetFormattedFields()
                    .Select(kv => kv.Key).ToArray();
                var summaryLookup = group.AccuracyDetails.ToLookup(a => new { a.Path, a.Label });

                var baseWriter = new StringWriter(CultureInfo);
                using (var writer = new HtmlTextWriter(baseWriter))
                {
                    // Table
                    writer.AddAttribute(HtmlTextWriterAttribute.Class, "DataCaptureStats");
                    writer.RenderBeginTag(HtmlTextWriterTag.Table);
                    writer.RenderBeginTag(HtmlTextWriterTag.Caption);
                    // Write the group-by values as a caption
                    for (int i = 0; i < group.GroupByNames.Length;)
                    {
                        var name = group.GroupByNames[i];
                        var value = group.GroupByValues[i];
                        writer.WriteEncodedText(name + ": " + value + Environment.NewLine);
                        if (++i < group.GroupByNames.Length)
                        {
                            writer.WriteFullBeginTag("br");
                        }
                    }
                    writer.RenderEndTag();
                    // Header row
                    writer.RenderBeginTag(HtmlTextWriterTag.Thead);
                    writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                    writer.RenderBeginTag(HtmlTextWriterTag.Th);
                    writer.Write("Path");
                    writer.RenderEndTag();
                    foreach (var col in attributeColumns)
                    {
                        writer.RenderBeginTag(HtmlTextWriterTag.Th);
                        writer.WriteEncodedText(col);
                        writer.RenderEndTag();
                    }
                    writer.RenderEndTag();
                    writer.RenderEndTag();
                    // Footer row
                    writer.RenderBeginTag(HtmlTextWriterTag.Tfoot);
                    writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                    writer.RenderBeginTag(HtmlTextWriterTag.Th);
                    writer.Write("File count");
                    writer.RenderEndTag();
                    writer.RenderBeginTag(HtmlTextWriterTag.Td);
                    writer.WriteEncodedText(string.Format(CultureInfo, "{0:N0}", group.FileCount));
                    writer.RenderEndTag();
                    writer.RenderEndTag();
                    writer.RenderEndTag();

                    // Attribute rows
                    foreach (var p in attributePaths)
                    {
                        var expected = summaryLookup[new { Path = p, Label = AccuracyDetailLabel.Expected }]
                            .Sum(a => a.Value);
                        var correct = summaryLookup[new { Path = p, Label = AccuracyDetailLabel.Correct }]
                            .Sum(a => a.Value);
                        var incorrect = summaryLookup[new { Path = p, Label = AccuracyDetailLabel.Incorrect }]
                            .Sum(a => a.Value);

                        var accuracy = new ExpandedAccuracy(expected, correct, incorrect);

                        // Write row header
                        writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                        writer.RenderBeginTag(HtmlTextWriterTag.Th);
                        writer.WriteEncodedText(p.Value);
                        writer.RenderEndTag();

                        // Write column data for this path
                        IEnumerable<KeyValuePair<string, Func<ExpandedAccuracy, string>>>
                            attributeColumnValues = ExpandedAccuracy.GetFormattedFields();
                        foreach (var col in attributeColumnValues)
                        {
                            writer.RenderBeginTag(HtmlTextWriterTag.Td);
                            writer.WriteEncodedText(col.Value(accuracy));
                            writer.RenderEndTag();
                        }
                        writer.RenderEndTag(); // End row
                    }
                    writer.RenderEndTag(); // End table
                }

                return baseWriter.ToString();
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI41532");
            }
        }

        /// <summary>
        /// Generates a CSV report from summarized accuracy details.
        /// </summary>
        /// <param name="statisticGroups">The <see cref="GroupStatistics"/> data to be used to build the report.</param>
        /// <returns>A string containing an html document representation of the data</returns>
        public static string AccuracyDetailsToCsv(this IList<GroupStatistics> statisticGroups)
        {
            try
            {
                if (!statisticGroups.Any())
                {
                    return "";
                }

                var groupByFieldNames = statisticGroups.First().GroupByNames;
                var attributePaths = statisticGroups.SelectMany(g => g.AccuracyDetails)
                    .Select(a => a.Path)
                    .Distinct()
                    .OrderBy(p => p)
                    .ToList();
                var attributeColumns = ExpandedAccuracy.GetFormattedFields()
                    .Select(kv => kv.Key).ToArray();

                var headerRow = 
                    // Each group-by field
                    groupByFieldNames
                    // File Count
                    .Concat(Enumerable.Repeat("File count", 1))
                    // Each path x each accuracy field
                    .Concat(attributePaths.SelectMany(path => attributeColumns.Select(column =>
                        path + "." + column)))
                    .ToArray();

                var statisticsToReportList = statisticGroups.ToList();
                var resultArray = new string[statisticsToReportList.Count + 1][];
                resultArray[0] = headerRow;
                int i = 0;
                foreach(var group in statisticsToReportList)
                {
                    var row = resultArray[++i] = new string[headerRow.Length];
                    var summaryLookup = group.AccuracyDetails.ToLookup(a => new { a.Path, a.Label });

                    // Increment an offset value for each column written
                    int offset = 0;

                    // Write the group-by columns
                    foreach (var groupBy in group.GroupByValues)
                    {
                        row[offset++] = groupBy;
                    }

                    // File count column
                    row[offset++] = string.Format(CultureInfo, "{0:N0}", group.FileCount);

                    // Attribute columns
                    foreach (var p in attributePaths)
                    {
                        int expected = summaryLookup[new { Path = p, Label = AccuracyDetailLabel.Expected }]
                            .Sum(a => a.Value);
                        int correct = summaryLookup[new { Path = p, Label = AccuracyDetailLabel.Correct }]
                            .Sum(a => a.Value);
                        int incorrect = summaryLookup[new { Path = p, Label = AccuracyDetailLabel.Incorrect }]
                            .Sum(a => a.Value);

                        var accuracy = new ExpandedAccuracy(expected, correct, incorrect);

                        // Write column data for this path
                        IEnumerable<KeyValuePair<string, Func<ExpandedAccuracy, string>>>
                            attributeColumnValues = ExpandedAccuracy.GetFormattedFields();
                        foreach (var col in attributeColumnValues)
                        {
                            row[offset++] = col.Value(accuracy);
                        }
                    }
                }

                // Convert each row to CSV
                var csvRows = resultArray.Select(row =>
                    string.Join(_CSV_DELIMITER, row.Select(cell =>
                        {
                            if (cell == null)
                            {
                                return "";
                            }
                            bool quote = cell.IndexOf(_CSV_DELIMITER, StringComparison.Ordinal) != -1
                                || cell.IndexOfAny(new[] { '\r', '\n', '"' }) != -1;
                            if (quote)
                            {
                                return "\"" + cell.Replace("\"", "\"\"") + "\"";
                            }
                            return cell;
                        })));

                return string.Join(Environment.NewLine, csvRows);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI41847");
            }
        }

        /// <summary>
        /// Internal class used to summarize the statistics
        /// </summary>
        private class Summarizer
        {
            /// <summary>
            /// A list of paths that appear for container-only labels as well as another label
            /// </summary>
            private List<NoCaseString> _conflictingPaths;

            /// <summary>
            /// A map of paths to <see cref="AccuracyDetail"/>s that are to be replaced with summary stats
            /// </summary>
            private Dictionary<NoCaseString, AccuracyDetail> _containerOnlyPaths;

            /// <summary>
            /// A map of paths to <see cref="ILookup{AccuracyDetailLabel, AccuracyDetail}"/> of paths to be summarized.
            /// </summary>
            private Dictionary<NoCaseString, ILookup<AccuracyDetailLabel, AccuracyDetail>> _otherPaths;

            /// <summary>
            /// Computes stats for container-only items.
            /// </summary>
            /// <param name="statisticsToSummarize">The statistics to summarize.</param>
            /// <param name="throwIfConflict">If set to <c>true</c> will throw an exception if
            /// a path appears for an <see cref="AccuracyDetail"/> with a <see cref="AccuracyDetailLabel.ContainerOnly"/> label
            /// as well as for some other <see cref="AccuracyDetailLabel"/>.
            /// If set to <c>false</c> then conflicting paths will be marked with an asterisk.</param>
            /// <returns>An <see cref="IEnumerable{AccuracyDetail}"/> where any items in the input that were labeled
            /// <see cref="AccuracyDetailLabel.ContainerOnly"/> have been replaced with new <see cref="AccuracyDetail"/>s</returns>
            public IEnumerable<AccuracyDetail> SummarizeStatistics(IEnumerable<AccuracyDetail> statisticsToSummarize, bool throwIfConflict)
            {
                MakeContainerAndOtherMaps(statisticsToSummarize.ToList());

                ExtractException.Assert("ELI41529", "There was a container-only/non-container-only conflict for an attribute path",
                    !throwIfConflict || !_conflictingPaths.Any(), "Conflicting paths",
                    string.Join(Environment.NewLine, _conflictingPaths));

                var newPaths = new List<AccuracyDetail>();
                foreach (var path in _containerOnlyPaths.Values.Select(a => a.Path.Value).Concat(Enumerable.Repeat("", 1)))
                {
                    newPaths.Add(CreateSummary(path, AccuracyDetailLabel.Expected));
                    newPaths.Add(CreateSummary(path, AccuracyDetailLabel.Correct));
                    newPaths.Add(CreateSummary(path, AccuracyDetailLabel.Incorrect));
                }

                var allPaths = newPaths.Concat(_otherPaths.Values.SelectMany(labels => labels.SelectMany(g => g.Cast<AccuracyDetail>())));

                // Mark any path conflicts with an asterisk
                if (_conflictingPaths.Any())
                {
                    allPaths = allPaths
                        .Select(accuracyDetail =>
                        {
                            if (_conflictingPaths.Exists(conflictingPath =>
                            {
                                var path = conflictingPath + " (Summary)";
                                return accuracyDetail.Path.Equals(path);
                            }))
                            {
                                return new AccuracyDetail(accuracyDetail.Label, accuracyDetail.Path + " *", accuracyDetail.Value);
                            }
                            else
                            {
                                return accuracyDetail;
                            }
                        });
                }

                return allPaths;
            }

            /// <summary>
            /// Makes the container and other maps and checks for conflicts.
            /// </summary>
            /// <remarks>Asserts that path/label pairs in <see paramref="statisticsToSummarize"/> are distinct</remarks>
            /// <param name="statisticsToSummarize">The statistics to summarize.</param>
            private void MakeContainerAndOtherMaps(List<AccuracyDetail> statisticsToSummarize)
            {
                ExtractException.Assert("ELI41528", "Path/label pairs in the collection must be distinct",
                    statisticsToSummarize.Select(a => new { a.Label, a.Path }).Distinct().Count() == statisticsToSummarize.Count());

                _containerOnlyPaths = new Dictionary<NoCaseString, AccuracyDetail>();
                _otherPaths = new Dictionary<NoCaseString, ILookup<AccuracyDetailLabel, AccuracyDetail>>();
                _conflictingPaths = new List<NoCaseString>();

                var grouped = statisticsToSummarize
                    .GroupBy(a => a.Path)
                    .Select(g => new { Path = g.Key, Labels = g.ToLookup(a => a.Label) });

                // Divide into container-only and other, value-holding paths
                // Keep track of any container-only paths that were sometimes deemed to be value-holding paths
                foreach (var p in grouped)
                {
                    var container = p.Labels[AccuracyDetailLabel.ContainerOnly].ToList();
                    if (container.Any())
                    {
                        _containerOnlyPaths[p.Path] = container.First();
                        if (p.Labels.Count > 1)
                        {
                            _conflictingPaths.Add(p.Path);
                        }
                    }
                    else
                    {
                        _otherPaths[p.Path] = p.Labels.ToLookup(g => g.Key, g => g.First());
                    }
                }
            }

            /// <summary>
            /// Creates the summary value by summing all subattributes of <see paramref="containerPath"/>.
            /// </summary>
            /// <param name="containerPath">The path of the container.</param>
            /// <param name="otherLabel">The <see cref="AccuracyDetailLabel"/> denoting the details to be summed.</param>
            /// <returns></returns>
            private AccuracyDetail CreateSummary (string containerPath, AccuracyDetailLabel otherLabel)
            {
                string prefix = "";
                string summaryPath = "(Summary)";
                if (!string.IsNullOrEmpty(containerPath))
                {
                    prefix = containerPath + "/";
                    summaryPath = containerPath + " " + summaryPath;
                }

                int value = _otherPaths.Keys
                    .Where(path => path.Value.StartsWith(prefix, StringComparison))
                    .Sum(path => _otherPaths[path][otherLabel].Sum(accuracyDetail => accuracyDetail.Value));

                return new AccuracyDetail(otherLabel, summaryPath, value);
            }
        }

        /// <summary>
        /// Calculates and formats accuracy/error measurements
        /// </summary>
        private class ExpandedAccuracy
        {
            /// <summary>
            /// An enumeration of accuracy field names and string-value-generating functions
            /// </summary>
            static KeyValuePair<string, Func<ExpandedAccuracy, string>>[] _allFields = {
                new KeyValuePair<string, Func<ExpandedAccuracy, string>>
                    ("Expected", a => string.Format(CultureInfo, "{0:N0}", a.Expected)),

                new KeyValuePair<string, Func<ExpandedAccuracy, string>>
                    ("Correct", a => string.Format(CultureInfo, "{0:N0}", a.Correct)),

                new KeyValuePair<string, Func<ExpandedAccuracy, string>>
                    ("Missing", a => string.Format(CultureInfo, "{0:N0}", a.Missing)),

                new KeyValuePair<string, Func<ExpandedAccuracy, string>>
                    ("Incorrect", a => string.Format(CultureInfo, "{0:N0}", a.Incorrect)),

                new KeyValuePair<string, Func<ExpandedAccuracy, string>>
                    ("% Correct (Recall)", a => string.Format(CultureInfo, "{0:P2}", a.Recall)),

                new KeyValuePair<string, Func<ExpandedAccuracy, string>>
                    ("Precision", a => string.Format(CultureInfo, "{0:P2}", a.Precision)),

                new KeyValuePair<string, Func<ExpandedAccuracy, string>>
                    ("F1-Score", a => string.Format(CultureInfo, "{0:N4}", a.F1Score)),

                new KeyValuePair<string, Func<ExpandedAccuracy, string>>
                    ("ROCE (C/I)", a => string.Format(CultureInfo, "{0:N2}", a.ROCE)),

                new KeyValuePair<string, Func<ExpandedAccuracy, string>>
                    ("EiF1 [2E/(1+EF)]", a => string.Format(CultureInfo, "{0:N2}", a.ExpectedInverseF1Score)),
            };

            /// <summary>
            /// True positives + false negatives
            /// </summary>
            public int Expected { get; }

            /// <summary>
            /// True positives
            /// </summary>
            public int Correct { get; }

            /// <summary>
            /// False negatives
            /// </summary>
            public int Missing { get; }

            /// <summary>
            /// False positives
            /// </summary>
            public int Incorrect { get; }

            /// <summary>
            /// Correct / (Incorrect + Correct)
            /// </summary>
            public double Precision { get; }

            /// <summary>
            /// Expected / Correct;
            /// </summary>
            public double Recall { get; }

            /// <summary>
            /// The harmonic mean of Precision and Recall
            /// </summary>
            public double F1Score { get; }

            /// <summary>
            /// Correct / Incorrect
            /// </summary>
            public double ROCE { get; }

            /// <summary>
            /// Harmonic mean of 1/F1Score and Expected
            /// </summary>
            public double ExpectedInverseF1Score { get; }

            /// <summary>
            /// Initializes a new instance of the <see cref="ExpandedAccuracy" /> class.
            /// </summary>
            /// <param name="expected">The number of expected attributes (true positives + false negatives)</param>
            /// <param name="correct">The number of correctly found attributes (true positives)</param>
            /// <param name="incorrect">The number of incorrectly found attributes (false positives)</param>
            public ExpandedAccuracy(int expected, int correct, int incorrect)
            {
                Expected = expected;
                Correct = correct;
                Incorrect = incorrect;
                ROCE = incorrect == 0
                    ? double.NaN
                    : (double) correct / incorrect;

                Precision = incorrect + correct == 0
                    ? 1
                    : (double) correct / (incorrect + correct);

                Recall = expected + incorrect == 0
                    ? 1
                    : expected == 0
                        ? double.NaN
                        : (double) correct / expected;

                F1Score = Precision + Recall == 0
                    ? 0
                    : 2 * Precision * Recall / (Precision + Recall);

                Missing = expected - correct;

                ExpectedInverseF1Score = 2 * Expected / (1 + F1Score * Expected);
            }

            /// <summary>
            /// Gets an enumeration of accuracy field names and string-value-generating functions
            /// </summary>
            /// <returns>The enumeration of names to functions</returns>
            public static IEnumerable<KeyValuePair<string, Func<ExpandedAccuracy, string>>> GetFormattedFields()
            {
                return _allFields;
            }
        }
    }
}