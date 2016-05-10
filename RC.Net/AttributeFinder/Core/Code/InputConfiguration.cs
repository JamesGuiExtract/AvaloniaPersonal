using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using UCLID_RASTERANDOCRMGMTLib;

namespace Extract.AttributeFinder
{
    /// <summary>
    /// Type of input
    /// </summary>
    public enum InputType
    {
        /// <summary>
        /// Input is a text file or CSV path
        /// </summary>
        TextFileOrCsv = 0,

        /// <summary>
        /// Input is a folder path
        /// </summary>
        Folder = 1
    }

    /// <summary>
    /// Class to hold specification of input files and to build the actual arrays
    /// </summary>
    public class InputConfiguration
    {
        #region Fields

        AttributeFinderPathTags _pathTags = new AttributeFinderPathTags();

        #endregion Fields

        #region Properties

        /// <summary>
        /// Gets/sets the input path
        /// </summary>
        public string InputPath
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/sets the <see cref="InputType"/> of <see cref="InputPath"/>
        /// </summary>
        public InputType InputPathType
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/sets the relative path string to expand for VOA paths
        /// </summary>
        public string AttributesPath
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/sets the relative path string to expand for answers or answer files
        /// </summary>
        public string AnswerPath
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/sets the percentage of input data to be used for the training set
        /// </summary>
        public int TrainingSetPercentage
        {
            get;
            set;
        }

        #endregion Properties

        #region Public Methods

        /// <summary>
        /// Builds data arrays from specification
        /// </summary>
        /// <param name="spatialStringFilePaths">Computed paths to USS input files</param>
        /// <param name="attributeFilePaths">Computed paths to feature VOA files</param>
        /// <param name="answersOrAnswerFilePaths">Computed answer strings or paths to answer files</param>
        [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]
        public void GetInputData(out string[] spatialStringFilePaths, out string[] attributeFilePaths, out string[] answersOrAnswerFilePaths)
        {
            List<string> imageFiles;

            if (InputPathType == InputType.Folder)
            {
                // Get all image files where there is a corresponding uss file
                imageFiles = Directory.GetFiles(InputPath, "*.uss", SearchOption.AllDirectories)
                    .Select(ussPath => Path.ChangeExtension(ussPath, null))
                    .Where(imagePath => File.Exists(imagePath)).ToList();

                answersOrAnswerFilePaths = new string[imageFiles.Count];
            }
            else if (InputPathType == InputType.TextFileOrCsv)
            {
                // Text file list
                if (!string.IsNullOrWhiteSpace(AnswerPath))
                {
                    imageFiles = File.ReadAllLines(InputPath).Select(line => line.Trim()).ToList();
                    answersOrAnswerFilePaths = new string[imageFiles.Count];
                }
                // CSV of images and answers
                else
                {
                    ExtractException.Assert("ELI39756", "Input file does not exist", File.Exists(InputPath));
                    imageFiles = new List<string>();
                    var answers = new List<string>();
                    using (var csvReader = new Microsoft.VisualBasic.FileIO.TextFieldParser(InputPath))
                    {
                        csvReader.Delimiters = new[] { "," };
                        while (!csvReader.EndOfData)
                        {
                            string[] fields;
                            try
                            {
                                fields = csvReader.ReadFields();
                            }
                            catch (Exception e)
                            {
                                var ue = new ExtractException("ELI39767", "Error parsing CSV input file", e);
                                ue.AddDebugData("CSV path", InputPath, false);
                                throw ue;
                            }

                            ExtractException.Assert("ELI39757", "CSV rows should contain exactly two fields", fields.Length == 2);

                            string imageName = fields[0];
                            string answer = fields[1];

                            // Ignore header row
                            try
                            {
                                // LineNumber is one more than the line number of last read fields
                                if (csvReader.LineNumber == 2
                                    && (imageName.IndexOfAny(Path.GetInvalidPathChars()) >= 0 || !Path.IsPathRooted(imageName)))
                                {
                                    continue;
                                }
                            }
                            catch (ArgumentException)
                            {
                                continue;
                            }

                            ExtractException.Assert("ELI39758", "Image path must not be relative", Path.IsPathRooted(imageName));
                            ExtractException.Assert("ELI39759", "File doesn't exist", File.Exists(imageName));
                            imageFiles.Add(imageName);
                            answers.Add(answer);
                        }
                    }
                    answersOrAnswerFilePaths = answers.ToArray();
                }
            }
            else
            {
                throw new ExtractException("ELI39760", "Unknown input path type: " + InputPathType.ToString());
            }

            spatialStringFilePaths = new string[imageFiles.Count];

            // If no attributes path given then use null for the collection
            attributeFilePaths = string.IsNullOrWhiteSpace(AttributesPath) ? null : new string[imageFiles.Count];

            for (int i = 0; i < imageFiles.Count; i++)
            {
                string imagePath = imageFiles[i];
                _pathTags.Document = new UCLID_AFCORELib.AFDocumentClass { Text = new SpatialStringClass { SourceDocName = imagePath } };
                spatialStringFilePaths[i] = imagePath + ".uss";
                if (attributeFilePaths != null)
                {
                    attributeFilePaths[i] = _pathTags.Expand(AttributesPath);
                }
                if (!string.IsNullOrWhiteSpace(AnswerPath))
                {
                    answersOrAnswerFilePaths[i] = _pathTags.Expand(AnswerPath);
                }
            }
        }

        #endregion Public Methods
    }
}