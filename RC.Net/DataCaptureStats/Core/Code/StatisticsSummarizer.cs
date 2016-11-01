using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web.UI;

namespace Extract.DataCaptureStats
{
    /// <summary>
    /// Class to summarize aggregate data
    /// </summary>
    public static class StatisticsSummarizer
    {
        #region Constants

        private static readonly IEqualityComparer<string> StringComparer = System.StringComparer.OrdinalIgnoreCase;
        private static readonly StringComparison StringComparison = System.StringComparison.OrdinalIgnoreCase;
        private static readonly CultureInfo CultureInfo = CultureInfo.InvariantCulture;

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
        /// Generates an HTML report from summarized accuracy details.
        /// </summary>
        /// <param name="statisticsToReport">The <see cref="IEnumerable{AccuracyDetail}"/> data to be used to build the report.</param>
        /// <returns>A string containing an html document representation of the data</returns>
        public static string AccuracyDetailsToHtml(this IEnumerable<AccuracyDetail> statisticsToReport)
        {
            try
            {
                var summaryLookup = statisticsToReport.ToLookup(a => new { a.Path, a.Label });
                var columns = new[] { "Path", "Expected", "Correct", "% Correct", "Incorrect", "ROCE" }.ToDictionary(s => s, _ => "");

                var baseWriter = new StringWriter(CultureInfo);
                using (var writer = new HtmlTextWriter(baseWriter))
                {
                    // Table
                    writer.AddAttribute(HtmlTextWriterAttribute.Class, "DataCaptureStats");
                    writer.RenderBeginTag(HtmlTextWriterTag.Table);
                    // Header row
                    writer.RenderBeginTag(HtmlTextWriterTag.Thead);
                    writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                    foreach (var col in columns.Keys)
                    {
                        writer.RenderBeginTag(HtmlTextWriterTag.Th);
                        writer.Write(col);
                        writer.RenderEndTag();
                    }
                    writer.RenderEndTag();
                    writer.RenderEndTag();
                    // Other rows
                    foreach (var p in statisticsToReport.Select(a => a.Path).Distinct().OrderBy(p => p))
                    {
                        var expected = summaryLookup[new { Path = p, Label = AccuracyDetailLabel.Expected }]
                            .Sum(a => a.Value);
                        var correct = summaryLookup[new { Path = p, Label = AccuracyDetailLabel.Correct }]
                            .Sum(a => a.Value);
                        var incorrect = summaryLookup[new { Path = p, Label = AccuracyDetailLabel.Incorrect }]
                            .Sum(a => a.Value);
                        columns["Path"] = p;
                        columns["Expected"] = expected.ToString(CultureInfo);
                        columns["Correct"] = correct.ToString(CultureInfo);
                        columns["% Correct"] = (expected == 0 ? double.NaN : Math.Round(correct * 100.0 / expected, 2))
                            .ToString(CultureInfo);
                        columns["Incorrect"] = incorrect.ToString(CultureInfo);
                        columns["ROCE"] = (incorrect == 0 ? double.NaN : Math.Round((double)correct / incorrect, 2))
                            .ToString(CultureInfo);

                        // Write row header
                        writer.RenderBeginTag(HtmlTextWriterTag.Tr);
                        writer.RenderBeginTag(HtmlTextWriterTag.Th);
                        writer.Write(columns["Path"]);
                        writer.RenderEndTag();
                        // Write row data cells
                        foreach (var col in columns.Values.Skip(1))
                        {
                            writer.RenderBeginTag(HtmlTextWriterTag.Td);
                            writer.Write(col);
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
        /// Internal class used to summarize the statistics
        /// </summary>
        private class Summarizer
        {
            /// <summary>
            /// A list of paths that appear for container-only labels as well as another label
            /// </summary>
            private List<string> _conflictingPaths;

            /// <summary>
            /// A map of paths to <see cref="AccuracyDetail"/>s that are to be replaced with summary stats
            /// </summary>
            private Dictionary<string, AccuracyDetail> _containerOnlyPaths;

            /// <summary>
            /// A map of paths to <see cref="ILookup{AccuracyDetailLabel, AccuracyDetail}"/> of paths to be summarized.
            /// </summary>
            private Dictionary<string, ILookup<AccuracyDetailLabel, AccuracyDetail>> _otherPaths;

            /// <summary>
            /// Computes stats for container-only items.
            /// </summary>
            /// <param name="statisticsToSummarize">The statistics to summarize.</param>
            /// <param name="throwIfConflict">If set to <c>true</c> will throw an exception if
            /// a path appears for an <see cref="AccuracyDetail"/> with a <see cref="AccuracyDetailLabel.ContainerOnly"/> label
            /// as well as for some other <see cref="AccuracyDetailLabel"/>.
            /// If set to <c>false</c> then conflicting paths will be marked with an asterisk.
            /// <returns>An <see cref="IEnumerable{AccuracyDetail}"/> where any items in the input that were labeled
            /// <see cref="AccuracyDetailLabel.ContainerOnly"/> have been replaced with new <see cref="AccuracyDetail"/>s</returns>
            public IEnumerable<AccuracyDetail> SummarizeStatistics(IEnumerable<AccuracyDetail> statisticsToSummarize, bool throwIfConflict)
            {
                MakeContainerAndOtherMaps(statisticsToSummarize.ToList());

                ExtractException.Assert("ELI41529", "There was a container-only/non-container-only conflict for an attribute path",
                    !throwIfConflict || !_conflictingPaths.Any(), "Conflicting paths", _conflictingPaths);

                var newPaths = new List<AccuracyDetail>();
                foreach (var path in _containerOnlyPaths.Values.Select(a => a.Path).Concat(Enumerable.Repeat("", 1)))
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
                                return accuracyDetail.Path.Equals(path, StringComparison);
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
            private void MakeContainerAndOtherMaps(IEnumerable<AccuracyDetail> statisticsToSummarize)
            {
                ExtractException.Assert("ELI41528", "Path/label pairs in the collection must be distinct",
                    statisticsToSummarize.Select(a => new { a.Label, a.Path }).Distinct().Count() == statisticsToSummarize.Count());

                _containerOnlyPaths = new Dictionary<string, AccuracyDetail>(StringComparer);
                _otherPaths = new Dictionary<string, ILookup<AccuracyDetailLabel, AccuracyDetail>>(StringComparer);
                _conflictingPaths = new List<string>();

                var grouped = statisticsToSummarize
                    .GroupBy(a => a.Path)
                    .Select(g => new { Path = g.Key, Labels = g.ToLookup(a => a.Label) });

                // Divide into container-only and other, value-holding paths
                // Keep track of any container-only paths that were sometimes deemed to be value-holding paths
                foreach (var p in grouped)
                {
                    var container = p.Labels[AccuracyDetailLabel.ContainerOnly];
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
                    .Where(path => path.StartsWith(prefix, StringComparison))
                    .Sum(path => _otherPaths[path][otherLabel].Sum(accuracyDetail => accuracyDetail.Value));

                return new AccuracyDetail(otherLabel, summaryPath, value);
            }
        }
    }
}