using IndexConverterV2.Models;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Subjects;
using System.Text.RegularExpressions;

namespace IndexConverterV2.Services
{
    public class EAVWriter
    {
        private List<AttributeListItem> _attributes = new();

        private TextFieldParser? _csvReader;
        private int _processingAttributeIndex = -1;
        //used to remember what output files have already been created
        private List<string> _touchedOutputFiles = new();
        //used to remember what files have already been processed
        private List<FileListItem> _processedFiles = new();
        private string _outputFolder = "";

        //Used to make observable from Processing
        private readonly BehaviorSubject<bool> _processingSubject = new(false);
        public bool Processing
        {
            get => _processingSubject.Value;
            private set => _processingSubject.OnNext(value);
        }
        public IObservable<bool> ObservableProcessing => _processingSubject;



        /// <summary>
        /// Configures variables to start processing attributes.
        /// </summary>
        /// <param name="attributes">The list of attributes to be made into EAVs.</param>
        /// <param name="outputFolder">The folder where new EAVs will be created.</param>
        /// <returns>True if able to start succesfully, false otherwise.</returns>
        public string StartProcessing(List<AttributeListItem> attributes, string outputFolder)
        {
            //Can't process if there are no attributes
            if (attributes.Count <= 0)
                return "attributes are empty";

            //Create the output folder
            if (Uri.TryCreate(outputFolder, UriKind.Absolute, out Uri? pathURI)
                && !string.IsNullOrWhiteSpace(pathURI.ToString()))
            {
                Directory.CreateDirectory(outputFolder);
                _outputFolder = outputFolder;
            }
            else
                return "invalid output folder";

            _attributes = attributes;
            _processingAttributeIndex = 0;
            try
            {
                FileListItem firstFile = _attributes[_processingAttributeIndex].File;
                _csvReader = MakeCSVReader(firstFile.Path);
                _csvReader.SetDelimiters(firstFile.Delimiter.ToString());
                _processedFiles = new()
                {
                    firstFile
                };
            }
            catch
            {
                return "could not open file of first attribute";
            }

            _touchedOutputFiles = new();
            Processing = true;

            return "processing started";
        }

        /// <summary>
        /// Stops EAV processing. EAVWriter will need to be started again from the beginning.
        /// </summary>
        public void StopProcessing()
        {
            if (Processing)
            {
                _ = _csvReader ?? throw new NullReferenceException("_csvReader is null!");
                _csvReader.Close();
            }

            Processing = false;
        }

        /// <summary>
        /// Used to process the next line of input, creating an EAV.
        /// The next line of the currently processing file will be read, 
        /// and all attributes associated with that file will process on that line.
        /// </summary>
        /// /// <returns>A string representing the text that was written to the eav</returns>
        public string ProcessNextLine()
        {
            //If not processing, end.
            if (!Processing)
                return "";

            //next line from attribute's input file
            //null when end of processing has been reached
            string[]? curLine = GetNextLine();
            if (curLine == null)
                return "";


            string toReturn = "";
            //get eav text for each attribute based on this line
            foreach (var attribute in _attributes)
            {
                //If the attribute's file is the currently processing file
                if (attribute.File == _processedFiles.Last())
                {
                    //and if its condition passes
                    if (AttributeConditionShouldPrint(attribute, curLine))
                    {
                        string outputFilePath = MakeOutputFilePath(attribute, curLine);
                        string curEAVText = GetEAVText(attribute, curLine) + '\n';
                        if (!String.IsNullOrEmpty(curEAVText))
                        {
                            WriteEAV(outputFilePath, curEAVText);
                            toReturn += curEAVText;
                        }
                    }
                }
            }

            return toReturn;
        }

        /// <summary>
        /// Finishes processing for the currently processing file.
        /// </summary>
        /// <returns>Name of the file that was processed</returns>
        public string ProcessNextFile()
        {
            if (!Processing)
                return "";

            _ = _csvReader ?? throw new NullReferenceException("_csvReader is null!");
            //while current file reader has more to read, process
            while (_csvReader.PeekChars(1) != null)
            {
                ProcessNextLine();
            }

            //when a file reader is empty, the file it was reading for is completed
            string toReturn = Path.GetFileName(_processedFiles.Last().Path);
            _csvReader.Close();
            _csvReader = GetNextCSVReader();
            return toReturn;
        }

        /// <summary>
        /// Processes to end.
        /// </summary>
        /// <returns>True if processing finished succesfully, false otherwise</returns>
        public bool ProcessAll()
        {
            if (!Processing)
                return false;

            while (Processing)
            {
                ProcessNextLine();
            }

            return true;
        }

        /// <summary>
        /// Generates a path for the output file of the current attribute.
        /// </summary>
        /// <param name="values">Tokenized line of input</param>
        /// <returns>String representing path for the output file of the current attribute</returns>
        private string MakeOutputFilePath(AttributeListItem attribute, string[] values)
        {
            string outputPath = Path.GetFullPath(
                Path.Combine(
                    _outputFolder, 
                    ReplacePercents(attribute.OutputFileName, values)));

            if (!outputPath.EndsWith(".eav", StringComparison.OrdinalIgnoreCase))
                outputPath += ".eav";

            if (Path.GetDirectoryName(outputPath) is string dir)
                Directory.CreateDirectory(dir);

            return outputPath;
        }

        /// <summary>
        /// Writes a single EAV to a file.
        /// </summary>
        /// <param name="outputFilePath">File path to write to</param>
        /// <param name="toWrite">The text that will be written to the EAV</param>
        /// <returns>String that was written to file</returns>
        private string WriteEAV(string outputFilePath, string toWrite)
        {
            //StreamWriter should create it the first time, append every time after
            //Should eventually allow users to determine this behaviour
            bool append;
            if (_touchedOutputFiles.Contains(outputFilePath))
                append = true;
            else
            {
                append = false;
                _touchedOutputFiles.Add(outputFilePath);
            }
            using StreamWriter attributeWriter = new(outputFilePath, append);

            attributeWriter.Write(toWrite);
            return toWrite;
        }

        /// <summary>
        /// Creates a string representing an EAV file text.
        /// </summary>
        /// <param name="attribute">Attribute to have an EAV made from</param>
        /// <param name="values">Current line of tokenized inputs</param>
        /// <returns></returns>
        internal string GetEAVText(AttributeListItem attribute, string[] values)
        {
            string name = attribute.Name;
            string value = ReplacePercents(attribute.Value, values);
            string type = ReplacePercents(attribute.Type, values);

            if (string.IsNullOrEmpty(type))
                return name + "|" + value;
            return name + "|" + value + "|" + type;
        }

        /// <summary>
        /// Gets the next line from appropriate input file for current attribute. 
        /// If it reaches the end of an attribute's input file, it will get the first line of input for the next attribute.
        /// Changes system state.
        /// </summary>
        /// <returns>The next line of input, or null if end of input has been reached.</returns>
        private string[]? GetNextLine()
        {
            _ = _csvReader ?? throw new NullReferenceException("_csvReader is null!");
            string[]? curLine = _csvReader.ReadFields();
            //If line is null, keep reading new lines until there are no more parsers
            while (curLine == null)
            {
                _csvReader.Close();
                _csvReader = GetNextCSVReader();

                //if GetNextCSVReader returns null, then there are no more files to process
                if (_csvReader == null)
                    return null;
                curLine = _csvReader.ReadFields();
            }

            return curLine;
        }

        /// <summary>
        /// Determines whether an attribute should print, based on if it is conditional and if that condition is met.
        /// </summary>
        /// <param name="attribute">Attribute to test the condition of</param>
        /// <param name="values">Tokenized line of input</param>
        /// <returns>True if attribute is not conditional or if its condition is met, false otherwise</returns>
        internal bool AttributeConditionShouldPrint(AttributeListItem attribute, string[] values)
        {
            //If conditional, check type of condition
            if (attribute.IsConditional)
            {
                if (attribute.LeftCondition == null
                    || attribute.RightCondition == null
                    || attribute.ConditionType == null)
                    return false;

                string leftCondition = ReplacePercents(attribute.LeftCondition, values);
                string rightCondition = ReplacePercents(attribute.RightCondition, values);
                bool match = leftCondition.Equals(rightCondition);

                //If ConditionType is true, then left and right should equal
                if (attribute.ConditionType.Value)
                    return match;
                //If ConditionType is false, then left and right should not equal
                else
                    return !match;
            }
            //If not conditional, should print
            return true;
        }

        /// <summary>
        /// Creates a TextFieldParser that reads from the next unprocessed input file.
        /// Changes system state.
        /// </summary>
        /// <returns>A TextFieldParser that is reading from an attribute's input file, or null if there are no further files to process.</returns>
        private TextFieldParser? GetNextCSVReader()
        {
            //Check if it's the last attribute
            if (_processingAttributeIndex >= _attributes.Count)
            {
                Processing = false;
                return null;
            }

            //Get the next unprocessed file
            while (_processedFiles.Contains(_attributes[_processingAttributeIndex].File))
            {
                _processingAttributeIndex++;
                if (_processingAttributeIndex >= _attributes.Count)
                {
                    Processing = false;
                    return null;
                }
            }

            //Make and return a new parser
            FileListItem nextFile = _attributes[_processingAttributeIndex].File;
            TextFieldParser newReader = MakeCSVReader(nextFile.Path);
            newReader.SetDelimiters(nextFile.Delimiter.ToString());
            _processedFiles.Add(nextFile);
            return newReader;
        }

        /// <summary>
        /// Creates a TextFieldParser from a file stream.
        /// This is done to prevent the input folder from being modified while EAVWriter is using it.
        /// </summary>
        /// <param name="path">Path to the input file.</param>
        /// <returns>TextFieldParser reading from the file at path</returns>
        private static TextFieldParser MakeCSVReader(string path)
        {
            FileStreamOptions opts = new()
            {
                Mode = FileMode.Open,
                Access = FileAccess.Read,
                Share = FileShare.Read
            };

            StreamReader fileReader = new(path, opts);
            return new(fileReader);
        }

        /// <summary>
        /// Replaces any occurrences of a % followed by a number with the relevant index in an array of strings. Not 0 based.
        /// </summary>
        /// <param name="replaceIn">String to search and replace %s in</param>
        /// <param name="values">Tokenized line of input. %3 would match values[2]</param>
        /// <returns>The string replaceIn but with all %s replaced with relevant values.</returns>
        internal string ReplacePercents(string replaceIn, string[] values)
        {
            // matches group num on any sequence of numerical digits
            Regex digits = new(@"%(?'num'\d+)");
            return FormatNewlines(digits.Replace(replaceIn, new MatchEvaluator(GetPercentIndexValue)));

            string GetPercentIndexValue(Match m)
            {
                string matchText = m.Groups["num"].Value;
                //%3 means third item in list, so values[2]
                int index = int.Parse(matchText) - 1;
                if (index < 0 || index > values.Length - 1)
                {
                    _ = _csvReader ?? throw new NullReferenceException("_csvReader is null!");
                    throw new IndexOutOfRangeException(
                        $"Error replacing {matchText} at line number {_csvReader.LineNumber}.");
                }
                return values[index];
            }
        }

        /// <summary>
        /// Replaces escaped newline and return characters with a displayable string version.
        /// </summary>
        /// <param name="input">The text to be modified.</param>
        /// <returns>The original text with all escaped newline and returns characters now displayable.</returns>
        internal static string FormatNewlines(string input)
        {
            input = input.Replace("\r", "\\r");
            input = input.Replace("\n", "\\n");
            return input;
        }

        /// <summary>
        /// Determines if an attribute in the attributes list has a child.
        /// This is determined by if the next attribute in the list begins with '.'
        /// </summary>
        /// <param name="index">The index of the attribute in the list</param>
        /// <returns>true if the attribute has child attributes, false otherwise</returns>
        /// <exception cref="IndexOutOfRangeException"></exception>
        internal bool AttributeHasChildren(int index) 
        {
            if (index <= -1 || index >= _attributes.Count)
                throw new IndexOutOfRangeException($"Error checking for children at index {index}");

            if (index == _attributes.Count - 1)
                return false;

            return AttributeIsChild(index + 1);
        }

        /// <summary>
        /// Determines if an attribute is a child, based on if its name starts with '.'
        /// </summary>
        /// <param name="index">The index of the attribute in the list</param>
        /// <returns>true if the attribute is a child, false otherwise</returns>
        /// <exception cref="IndexOutOfRangeException"></exception>
        internal bool AttributeIsChild(int index)
        {
            if (index <= -1 || index >= _attributes.Count)
                throw new IndexOutOfRangeException($"Error checking if child at index {index}");

            if (_attributes.ElementAt(index).Name.StartsWith('.'))
                return true;

            return false;
        }
    }
}
