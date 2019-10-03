using Extract.FileActionManager.Forms;
using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using UCLID_COMUTILSLib;

namespace Extract.Redaction.Davidson
{
    /// <summary>
    /// Parser for Davidson County Rich Text Format batches
    /// K:\SecureSamples\NashvilleCriminalCourt\Set001\Original
    /// </summary>
    [CLSCompliant(false)]
    public static class RichTextFormatBatchProcessor
    {
        #region Constants

        /// <summary>
        /// Some magic strings
        /// </summary>
        static class MagicStrings
        {
            /// <summary>
            /// Value to use in filenames for the sub-label component when the content is invalid Rich Text Format
            /// meaning the existence and extent of the sub-label (stuff between files) is ambiguous
            /// </summary>
            public static readonly string InvalidRTF = "Extract_Invalid_RTF";

            /// <summary>
            /// Value to use in filenames for the sub-label component when there is not a sub-label after a file's contents
            /// </summary>
            public static readonly string None = "Extract_No_Label";

            /// <summary>
            /// Label that occurs for the first line in a batch file that doesn't have any associated files
            /// </summary>
            public static readonly string AffidavitID = "AFFIDAVIT_ID";

            /// <summary>
            /// Content that occurs for the first line in a batch file that doesn't need to be split or written out to a file
            /// </summary>
            public static readonly string AffTemplateRTF = "AFF_TEMPLATE_RTF";
        }

        // Pattern to match valid or invalid RTF sequences and non-RTF suffixes by matching curly brackets
        const string RTF_PATTERN =
            @"(?inx)
              \G(
                  (?>(?'RTF'
                      \s*
                      [{]
                      \\rtf
                      (?>(
                          \\\\
                        | \\[{}]
                        | [{](?'openGroup')
                        | [}](?'-openGroup')
                        | [^{}]
                      )+)
                      [}]
                      (?(openGroup)(?!))
                  ))
                  (?>(?'Suffix'\s*(?'Label'\w+)?))
                  (?=({\\rtf|\z))
                | (?'Invalid'(?>\s*\S)[\S\s]*?(?={\\rtf|\z))
              )"; // Ignore this comment (close the }} to help code editors)

        // Regex to match valid or invalid RTF sequences and non-RTF suffixes by matching curly brackets
        static readonly ThreadLocal<Regex> _RTFMatcher = new ThreadLocal<Regex>(() => new Regex(RTF_PATTERN, RegexOptions.Compiled, TimeSpan.FromMinutes(5)));

        /// <summary>
        /// Path tag used for separating files into sub-batches of 1000 labels each
        /// </summary>
        public static readonly string SubBatchNumber = "<SubBatchNumber>";

        #endregion Constants

        #region Static Methods

        /// <summary>
        /// Divides input (stuff between labels) into multiple output files by matching rich text format group delimiters
        /// </summary>
        /// <param name="fileNameBase">The beginning of the final filename, to be updated with more details for each output</param>
        /// <param name="contentString">The string containing zero or more RTF files with optional suffixes</param>
        /// <returns>One or more <see cref="OutputFileData"/> instances</returns>
        static IEnumerable<OutputFileData> SplitIntoSubFiles(string fileNameBase, string contentString)
        {
            var subFiles = new List<OutputFileData>();
            string getFileName(string subLabel)
            {
                return UtilityMethods.FormatInvariant($"{fileNameBase}.sub-{subFiles.Count + 1:D3}.sublabel-{subLabel}");
            }
            foreach (Match match in _RTFMatcher.Value.Matches(contentString))
            {
                var label = MagicStrings.None;
                var rtfGroup = match.Groups["RTF"];
                var invalidGroup = match.Groups["Invalid"];
                var suffixGroup = match.Groups["Suffix"];
                var labelGroup = match.Groups["Label"];
                if (rtfGroup.Success)
                {
                    string suffix = "";
                    if (suffixGroup.Success)
                    {
                        suffix = suffixGroup.Value;
                        if (labelGroup.Success)
                        {
                            label = labelGroup.Value;
                        }
                    }
                    subFiles.Add(
                        new OutputFileData
                        (
                            fileNameBase: getFileName(label),
                            contents: rtfGroup.Value,
                            suffix: suffix,
                            fileType: OutputFileType.RichTextFile
                        ));
                }
                else if (invalidGroup.Success)
                {
                    subFiles.Add(
                        new OutputFileData
                        (
                            fileNameBase: getFileName(MagicStrings.InvalidRTF),
                                contents: invalidGroup.Value,
                                suffix: "",
                                fileType: OutputFileType.TextFile
                            ));
                }
            }

            // Output a plain text result if the input wasn't parsable
            if (subFiles.Count == 0)
            {
                subFiles.Add(
                    new OutputFileData
                    (
                        fileNameBase: getFileName(MagicStrings.None),
                        contents: contentString,
                        suffix: "",
                        fileType: OutputFileType.TextFile
                    ));
            }

            return subFiles;
        }

        /// <summary>
        /// Create items from accumulated content
        /// </summary>
        /// <param name="fileNameBase">The beginning of the final filename, to be updated with more details for each output</param>
        /// <param name="lineNumber">The last line number that this content appeared on (e.g., 3 if the content started on line 1 and continued on lines 2 and 3)</param>
        /// <param name="label">The label that appeared before this content</param>
        /// <param name="contentString">The string containing zero or more RTF files with optional suffixes</param>
        /// <returns>An enumeration of <see cref="BatchFileItem"/>s</returns>
        static IEnumerable<BatchFileItem> CreateBatchItems(string fileNameBase, int lineNumber, string label, string contentString)
        {
            // Header line and empty contents don't need to be written out
            if (string.IsNullOrWhiteSpace(contentString) ||
                lineNumber == 1 && label == MagicStrings.AffidavitID && contentString.Trim() == MagicStrings.AffTemplateRTF)
            {
                yield return new BetweenFileData(label + "|" + contentString);
            }
            else
            {
                yield return new BetweenFileData(label + "|");
                foreach (var subFile in SplitIntoSubFiles(fileNameBase, contentString))
                {
                    yield return subFile;
                }
            }
        }

        /// <summary>
        /// Parse batch file lines and return list of batch components
        /// </summary>
        /// <remarks>
        /// Batch format has lines that are either LABEL|CONTENTS or CONTENTS_CONTINUED
        ///   where '|' is the literal character
        ///   where LABEL at least one character and up to 19 characters that aren't '|'
        ///   where CONTENTS is any text
        ///   where CONTENTS_CONTINUED is any text where none of the first 20 characters are '|'
        /// CONTENTS_CONTINUED will be concatenated with the previous line's contents
        /// Contents may be plain text or contain multiple RTF files separated by label suffixes
        /// </remarks>
        /// <param name="lines">The lines of the batch file to be divided</param>
        /// <param name="outputDirectoryPathTagFunction">The path tag function to expand into the output directory for files.
        /// Can include the special &lt;SubBatchNumber&gt; tag in addition to &lt;SourceDocName&gt; and functions</param>
        /// <param name="pathTags">Used to expand tags/functions in <see paramref="outputDirectoryPathTagFunction"/> and to get the base name for the output file</param>
        public static IEnumerable<BatchFileItem> DivideBatch(IEnumerable<string> lines, string outputDirectoryPathTagFunction, PathTagsBase pathTags)
        {
            string sourceDocName = pathTags.Expand("$FileOf(<SourceDocName>)");
            int lineNum = 1;
            int subBatchNum = 1;
            string label = MagicStrings.None;
            string fileNameBase = null;
            StringBuilder contents = new StringBuilder();
            foreach (var (line, index) in lines.Select((line, i) => (line, i)))
            {
                lineNum = index + 1;
                if (subBatchNum % 1000 == 1)
                {
                    if (outputDirectoryPathTagFunction.Contains(SubBatchNumber))
                    {
                        int subDirNum = (subBatchNum - 1) / 1000 + 1;
                        string subDir = UtilityMethods.FormatInvariant($"{subDirNum:D3}");
                        {
                            pathTags.AddTag(SubBatchNumber, subDir);
                        }
                    }
                }

                if (String.IsNullOrWhiteSpace(line))
                {
                    contents.AppendLine();
                }
                else
                {
                    // Check for LABEL| in the first 20 characters (this might be a false positive but that will be checked for later)
                    int searchEnd = Math.Min(20, line.Length);
                    var splitPoint = line.IndexOf('|', 0, searchEnd);
                    if (splitPoint <= 0)
                    {
                        contents.AppendLine(line);
                    }
                    else
                    {
                        // A new group started so yield the items from the content collected
                        if (fileNameBase != null)
                        {
                            var contentString = contents.ToString();
                            List<BatchFileItem> potentialBatchItems = CreateBatchItems(fileNameBase, lineNum - 1, label, contentString).ToList();

                            if (potentialBatchItems.OfType<OutputFileData>().LastOrDefault()?.FileType == OutputFileType.TextFile)
                            {
                                // The last item was not a valid RTF file so consider this a false positive label and keep accumulating lines
                                contents.AppendLine(line);
                                continue;
                            }

                            contents = new StringBuilder();
                            foreach (var subFile in potentialBatchItems)
                            {
                                yield return subFile;
                            }
                        }

                        string expandedOutputDir = Path.GetFullPath(pathTags.Expand(outputDirectoryPathTagFunction));
                        label = line.Substring(0, splitPoint);
                        fileNameBase = Path.Combine(expandedOutputDir, UtilityMethods.FormatInvariant($"{sourceDocName}.line-{lineNum:D6}.label-{label}"));

                        contents.AppendLine(line.Substring(splitPoint + 1));

                        subBatchNum++;
                    }
                }
            }

            // Write out the last group
            if (fileNameBase != null)
            {
                var contentString = contents.ToString();
                foreach (var subFile in CreateBatchItems(fileNameBase, lineNum, label, contentString))
                {
                    yield return subFile;
                }
            }
        }

        #endregion Static Methods
    }
}
