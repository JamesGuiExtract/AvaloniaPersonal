using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using UCLID_RASTERANDOCRMGMTLib;
using System.Threading;

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
    [Serializable]
    public class InputConfiguration
    {
        #region Fields

        // Backing fields for properties
        private string _attributesPath;
        private string _answerPath;
        private string _inputPath;
        private InputType _inputPathType;
        private int _trainingSetPercentage;

        #endregion Fields

        #region Properties

        /// <summary>
        /// Gets/sets the input path
        /// </summary>
        public string InputPath
        {
            get
            {
                return _inputPath;
            }
            set
            {
                _inputPath = string.IsNullOrWhiteSpace(value) ? null : value;
            }
        }

        /// <summary>
        /// Gets/sets the <see cref="InputType"/> of <see cref="InputPath"/>
        /// </summary>
        public InputType InputPathType
        {
            get
            {
                return _inputPathType;
            }
            set
            {
                if (value != _inputPathType)
                {
                    _inputPathType = value;
                }
            }
        }

        /// <summary>
        /// Gets/sets the relative path string to expand for VOA paths
        /// </summary>
        public string AttributesPath
        {
            get
            {
                return _attributesPath;
            }
            set
            {
                _attributesPath = string.IsNullOrWhiteSpace(value) ? null : value;
            }
        }

        /// <summary>
        /// Gets/sets the relative path string to expand for answers or answer files
        /// </summary>
        public string AnswerPath
        {
            get
            {
                return _answerPath;
            }
            set
            {
                _answerPath = string.IsNullOrWhiteSpace(value) ? null : value;
            }
        }

        /// <summary>
        /// Gets/sets the percentage of input data to be used for the training set
        /// </summary>
        public int TrainingSetPercentage
        {
            get
            {
                return _trainingSetPercentage;
            }
            set
            {
                if (value != _trainingSetPercentage)
                {
                    _trainingSetPercentage = value;
                }
            }
        }

        /// <summary>
        /// Gets whether this instance is properly configured
        /// </summary>
        public bool IsConfigured
        {
            get
            {
                return !(string.IsNullOrEmpty(InputPath) || InputPathType == InputType.Folder && string.IsNullOrEmpty(AnswerPath));
            }
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
        public void GetInputData(out string[] spatialStringFilePaths, out string[] attributeFilePaths,
            out string[] answersOrAnswerFilePaths)
        {
            GetInputData(out spatialStringFilePaths, out attributeFilePaths, out answersOrAnswerFilePaths, _ => { }, CancellationToken.None);
        }

        /// <summary>
        /// Builds data arrays from specification
        /// </summary>
        /// <param name="spatialStringFilePaths">Computed paths to USS input files</param>
        /// <param name="attributeFilePaths">Computed paths to feature VOA files</param>
        /// <param name="answersOrAnswerFilePaths">Computed answer strings or paths to answer files</param>
        /// <param name="updateStatus">Function to use for sending progress updates to caller</param>
        /// <param name="cancellationToken">Token indicating that processing should be canceled</param>
        [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]
        public void GetInputData(out string[] spatialStringFilePaths, out string[] attributeFilePaths,
            out string[] answersOrAnswerFilePaths,
            Action<StatusArgs> updateStatus, System.Threading.CancellationToken cancellationToken)
        {
            ExtractException.Assert("ELI39802", "This instance has not been fully configured", IsConfigured);

            spatialStringFilePaths = new string[0];
            attributeFilePaths = new string[0];
            answersOrAnswerFilePaths = new string[0];
            
            List<string> imageFiles = new List<string>();
            if (InputPathType == InputType.Folder)
            {
                // Get all image files where there is a corresponding uss file
                foreach(var imagePath in Directory.EnumerateFiles(InputPath, "*.uss", SearchOption.AllDirectories)
                    .Select(ussPath => Path.ChangeExtension(ussPath, null))
                    .Where(imagePath => File.Exists(imagePath)))
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    imageFiles.Add(imagePath);
                    updateStatus(new StatusArgs { StatusMessage = "Getting input files: {0:N0} files", Int32Value = 1 });
                };

                answersOrAnswerFilePaths = new string[imageFiles.Count];
            }
            else if (InputPathType == InputType.TextFileOrCsv)
            {
                // Text file list
                if (!string.IsNullOrWhiteSpace(AnswerPath))
                {
                    foreach (var line in File.ReadLines(InputPath))
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        imageFiles.Add(line.Trim());
                        updateStatus(new StatusArgs { StatusMessage = "Getting input files: {0:N0} files", Int32Value = 1 });
                    }
                    answersOrAnswerFilePaths = new string[imageFiles.Count];
                }
                // CSV of images and answers
                else
                {
                    ExtractException.Assert("ELI39756", "Input file does not exist", File.Exists(InputPath));
                    var answers = new List<string>();
                    using (var csvReader = new Microsoft.VisualBasic.FileIO.TextFieldParser(InputPath))
                    {
                        csvReader.Delimiters = new[] { "," };
                        while (!csvReader.EndOfData)
                        {
                            cancellationToken.ThrowIfCancellationRequested();

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
                            updateStatus(new StatusArgs { StatusMessage = "Getting input files: {0:N0} files", Int32Value = 1 });
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

            var pathTags = new AttributeFinderPathTags();
            for (int i = 0; i < imageFiles.Count; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                string imagePath = imageFiles[i];
                pathTags.Document = new UCLID_AFCORELib.AFDocumentClass { Text = new SpatialStringClass { SourceDocName = imagePath } };
                spatialStringFilePaths[i] = imagePath + ".uss";
                if (attributeFilePaths != null)
                {
                    attributeFilePaths[i] = pathTags.Expand(AttributesPath);
                }
                if (!string.IsNullOrWhiteSpace(AnswerPath))
                {
                    answersOrAnswerFilePaths[i] = pathTags.Expand(AnswerPath);
                }
            }
        }

        /// <summary>
        /// Pretty prints this object with supplied <see cref="System.CodeDom.Compiler.IndentedTextWriter"/>
        /// </summary>
        /// <param name="writer">The <see cref="System.CodeDom.Compiler.IndentedTextWriter"/> to use</param>
        public void PrettyPrint(System.CodeDom.Compiler.IndentedTextWriter writer)
        {
            try
            {
                var oldIndent = writer.Indent;
                writer.Indent++;
                writer.WriteLine("AnswerPath: " + AnswerPath);
                writer.WriteLine("AttributesPath: " + AttributesPath);
                writer.WriteLine("InputPath: " + InputPath);
                writer.WriteLine("InputPathType: " + InputPathType);
                writer.WriteLine("TrainingSetPercentage: " + TrainingSetPercentage);
                writer.Indent = oldIndent;
            }
            catch (Exception e)
            {
                throw e.AsExtract("ELI40066");
            }
        }

        #endregion Public Methods

        #region Overrides

        /// <summary>
        /// Whether this instance has equal property values to another
        /// </summary>
        /// <param name="obj">The instance to compare with</param>
        /// <returns><see langword="true"/> if this instance has equal property values, else <see langword="false"/></returns>
        public override bool Equals(object obj)
        {
            if (Object.ReferenceEquals(this, obj))
            {
                return true;
            }

            var other = obj as InputConfiguration;
            if (other == null
                || other.AnswerPath != AnswerPath
                || other.AttributesPath != AttributesPath
                || other.InputPath != InputPath
                || other.InputPathType != InputPathType
                || other.TrainingSetPercentage != TrainingSetPercentage)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Gets the hash code for this object
        /// </summary>
        /// <returns>The hash code for this object</returns>
        public override int GetHashCode()
        {
            return HashCode.Start
                .Hash(AnswerPath)
                .Hash(AttributesPath)
                .Hash(InputPath)
                .Hash(InputPathType)
                .Hash(TrainingSetPercentage);
        }

        #endregion Overrides
    }
}