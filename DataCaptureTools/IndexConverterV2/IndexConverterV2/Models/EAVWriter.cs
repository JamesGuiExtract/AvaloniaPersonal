using IndexConverterV2.Views;
using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Subjects;
using System.Text.RegularExpressions;

namespace IndexConverterV2.Models
{
    public class EAVWriter
    {
        private List<AttributeListItem> _attributes = new();

        private TextFieldParser? _csvReader;
        private int _processingAttributeIndex = -1;
        private List<string> _touchedFiles = new();
        private string _outputFolder = "";

        //Used to make observable from Processing
        private readonly BehaviorSubject<bool> _processingSubject = new BehaviorSubject<bool>(false);
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
        public bool StartProcessing(List<AttributeListItem> attributes, string outputFolder)
        {
            if (attributes.Count <= 0)
                return false;

            _attributes = attributes;
            _processingAttributeIndex = 0;
            try
            {
                _csvReader = new TextFieldParser(_attributes[_processingAttributeIndex].File.Path);
                _csvReader.SetDelimiters(_attributes[_processingAttributeIndex].File.Delimiter.ToString());
                _csvReader.HasFieldsEnclosedInQuotes = true;
            }
            catch
            {
                return false;
            }

            this._outputFolder = outputFolder;
            _touchedFiles = new();
            Processing = true;

            return true;
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
        /// Used to process the next line of input, creating an EAV
        /// </summary>
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

            //if the print condition of the attribute isn't fulfilled, return
            if (!AttributeConditionShouldPrint(_attributes[_processingAttributeIndex], curLine))
                return "";

            string outputFilePath = MakeOutputFilePath(curLine);

            string written = WriteEAV(outputFilePath, curLine);
            return written;
        }

        /// <summary>
        /// Finishes processing for the currently processing attribute
        /// </summary>
        /// <returns>Name of the attribute that was processed</returns>
        public string ProcessNextAttribute() 
        {
            if (!Processing)
                return "";

            string attributeName = _attributes[_processingAttributeIndex].Name;

            _ = _csvReader ?? throw new NullReferenceException("_csvReader is null!");
            //while current file reader has more to read, process
            while (_csvReader.PeekChars(1) != null)
            {
                ProcessNextLine();
            }

            //when a file reader is empty, the attribute it was reading for is completed
            _csvReader = GetNextCSVReader();
            return attributeName;
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
        private string MakeOutputFilePath(string[] values) 
        {
            return _outputFolder 
                + "\\" 
                + ReplacePercents(_attributes[_processingAttributeIndex].OutputFileName, values)
                + ".eav";
        }

        /// <summary>
        /// Writes a single EAV to a file.
        /// </summary>
        /// <param name="outputFilePath">File path to write to</param>
        /// <param name="values">Tokenized line of input</param>
        /// <returns>String that was written to file</returns>
        private string WriteEAV(string outputFilePath, string[] curLine) 
        {
            //Determine whether StreamWriter should append based on whether the file has already been created by EAVWriter.
            bool append;
            if (_touchedFiles.Contains(outputFilePath))
                append = true;
            else
            {
                append = false;
                _touchedFiles.Add(outputFilePath);
            }
            using StreamWriter attributeWriter = new(outputFilePath, append);

            string toWrite = GetEAVText(_attributes[_processingAttributeIndex], curLine);
            attributeWriter.WriteLine(@toWrite);
            return toWrite;
        }

        /// <summary>
        /// Creates a string representing an EAV file text.
        /// </summary>
        /// <param name="attribute">Attribute to have an EAV made from</param>
        /// <param name="curLine">Current line of tokenized inputs</param>
        /// <returns></returns>
        private string GetEAVText(AttributeListItem attribute, string[] curLine)
        { 
            string name = attribute.Name;
            string value = ReplacePercents(attribute.Value, curLine);
            string type = ReplacePercents(attribute.Type, curLine);

            return name + "|" + FormatNewline(value) + "|" + FormatNewline(type);
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
            while (curLine == null)
            {
                _csvReader.Close();
                _csvReader = GetNextCSVReader();

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
        private bool AttributeConditionShouldPrint(AttributeListItem attribute, string[] values) 
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
        /// Creates a StreamReader that reads from the next attribute's input file.
        /// Changes system state.
        /// </summary>
        /// <returns>A StreamReader that is reading from an attribute's input file, or null if there are no further attributes.</returns>
        private TextFieldParser? GetNextCSVReader()
        {
            if (_processingAttributeIndex >= _attributes.Count - 1)
            {
                Processing = false;
                return null;
            }

            _processingAttributeIndex++;
            TextFieldParser newReader = new TextFieldParser(_attributes[_processingAttributeIndex].File.Path);
            newReader.SetDelimiters(_attributes[_processingAttributeIndex].File.Delimiter.ToString());
            newReader.HasFieldsEnclosedInQuotes = true;
            return newReader;
        }

        /// <summary>
        /// Replaces any occurrences of a % followed by a number with the relevant index in an array of strings. Not 0 based.
        /// </summary>
        /// <param name="replaceIn">String to search and replace %s in</param>
        /// <param name="values">Tokenized line of input. %3 would match values[2]</param>
        /// <returns>The string replaceIn but with all %s replaced with relevant values.</returns>
        internal string ReplacePercents(string replaceIn, string[] values)
        {
            Regex rgx = new(@"%(?'num'\d+)");
            return rgx.Replace(replaceIn, new MatchEvaluator(GetPercentIndexValue));

            string GetPercentIndexValue(Match m)
            {
                string matchText = m.Groups["num"].Value;
                //%3 means third item in list, so values[2]
                int index = int.Parse(matchText) - 1;
                if (index < 0 || index > values.Length - 1)
                {
                    _ = _csvReader ?? throw new NullReferenceException("_csvReader is null!");
                    throw new IndexOutOfRangeException(
                        $"Error replacing {matchText} at {_attributes[_processingAttributeIndex].File.Path}, line number " +
                        $"{_csvReader.LineNumber}.");
                } 
                return values[index];
            }
        }

        internal static string FormatNewline(string input) 
        {
            input = input.Replace("\r", "\\r");
            input = input.Replace("\n", "\\n");
            return input;
        }
    }
}
