using Extract.Utilities;
using Extract.Utilities.Parsers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace SplitRTFCollection
{
    class Program
    {
        static Encoding _encoding;

        static void Main(string[] args)
        {
            _encoding = Encoding.GetEncoding("windows-1252");
            if (args.Length == 2 && File.Exists(args[0]))
            {
                string inputFile = args[0];
                string outputDir = args[1];
                SplitFile(inputFile, outputDir);
            }
            else if (args.Length == 2 && Directory.Exists(args[0]))
            {
                string inputDir = args[0];
                string outputBaseDir = args[1];
                foreach(var file in Directory.GetFiles(inputDir, "*.txt"))
                {
                    string outputDir = Path.Combine(outputBaseDir, Path.GetFileNameWithoutExtension(file));
                    Console.WriteLine(UtilityMethods.FormatCurrent($"Splitting {file} to {outputDir}"));
                    SplitFile(file, outputDir);
                }
            }
        }

        private static void SplitFile(string inputFile, string outputDir)
        {
            if (!Directory.Exists(outputDir))
            {
                Directory.CreateDirectory(outputDir);
            }

            int lineNum = 1;
            int fileNum = 1;
            string subDir = "";
            string label = "None";
            string fileNameBase = null;
            StringBuilder contents = new StringBuilder();
            foreach (var line in File.ReadLines(inputFile))
            {
                if (fileNum % 1000 == 1)
                {
                    int subDirNum = (fileNum - 1) / 1000 + 1;
                    subDir = Path.Combine(outputDir, UtilityMethods.FormatInvariant($"{subDirNum:D3}"));
                    Directory.CreateDirectory(subDir);
                }

                if (String.IsNullOrWhiteSpace(line))
                {
                    contents.AppendLine();
                }
                else
                {
                    int searchEnd = Math.Min(20, line.Length);
                    var splitPoint = line.IndexOf('|', 0, searchEnd);
                    if (splitPoint > 0)
                    {
                        if (fileNameBase != null)
                        {
                            var contentString = contents.ToString();
                            contents = new StringBuilder();

                            // Header line is special case
                            if (lineNum == 2 && label == "AFFIDAVIT_ID" && contentString.Trim() == "AFF_TEMPLATE_RTF")
                            {
                                File.WriteAllText(fileNameBase + ".txt", contentString);
                            }
                            else
                            {
                                WriteFiles(fileNameBase, contentString);
                            }
                        }

                        label = line.Substring(0, splitPoint);
                        fileNameBase = Path.Combine(subDir, UtilityMethods.FormatInvariant($"line-{lineNum:D6}.label-{label}"));
                        contents.AppendLine(line.Substring(splitPoint + 1));
                        fileNum++;
                    }
                    else
                    {
                        contents.AppendLine(line);
                    }

                }
                lineNum++;
            }
        }

        private static void WriteFiles(string fileNameBase, string contentString)
        {
            List<(string label, string contents, bool isRTF)> subFiles = SplitIntoSubFiles(contentString);
            int idx = 1;
            foreach (var (label, contents, isRTF) in subFiles)
            {
                string ext = isRTF ? "rtf" : "txt";
                var fileName = UtilityMethods.FormatInvariant($"{fileNameBase}.sub-{idx:D3}.sublabel-{label}.{ext}");
                File.WriteAllText(fileName, contents, _encoding);
                idx++;
            }
        }

        static readonly string RTF_PATTERN =
            @"(?inx)
              \G(
                  \s*
                  (?'RTF'
                      [{]
                      \\rtf
                      (?>
                          \\\\
                        | \\[{}]
                        | [{](?'openGroup')
                        | [}](?'-openGroup')
                        | [^{}]
                      )+
                      [}]
                      (?(openGroup)(?!))
                  )
                  (?>\s*(?'Label'\w+))?
                  (?=\s*({\\rtf|\z))
                | (?'Invalid'\S[\S\s]*?(?={\\rtf|\z))
              )";
        private static List<(string label, string contents, bool isRTF)> SplitIntoSubFiles(string contentString)
        {
            var subFiles = new List<(string, string, bool)>();
            foreach (Match match in Regex.Matches(contentString, RTF_PATTERN))
            {
                var label = "None";
                var labelGroup = match.Groups["Label"];
                var rtfGroup = match.Groups["RTF"];
                var invalidGroup = match.Groups["Invalid"];
                if (rtfGroup.Success)
                {
                    if (labelGroup.Success)
                    {
                        label = labelGroup.Value;
                    }
                    subFiles.Add((label, rtfGroup.Value, true));
                }
                else if (invalidGroup.Success)
                {
                    subFiles.Add(("InvalidRTF", invalidGroup.Value, false));
                }
            }

            if (subFiles.Count == 0)
            {
                subFiles.Add(("None", contentString, false));
            }

            return subFiles;
        }
    }
}
