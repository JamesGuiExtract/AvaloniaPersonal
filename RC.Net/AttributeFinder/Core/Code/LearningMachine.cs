﻿using Accord.Math;
using Accord.Statistics;
using Accord.Statistics.Analysis;
using Extract.Encryption;
using Extract.Licensing;
using Extract.Utilities;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Text;
using System.Threading;
using System.Transactions;
using UCLID_AFUTILSLib;
using UCLID_COMUTILSLib;
using UCLID_RASTERANDOCRMGMTLib;
using ZstdNet;
using AccuracyData = Extract.Utilities.Union<Accord.Statistics.Analysis.GeneralConfusionMatrix, Accord.Statistics.Analysis.ConfusionMatrix>;
using ComAttribute = UCLID_AFCORELib.Attribute;

namespace Extract.AttributeFinder
{
    /// <summary>
    /// Class to hold a trained machine, data encoder and training stats
    /// </summary>
    [CLSCompliant(false)]
    [Serializable]
    // Don't rename because it could break serialization
    [Obfuscation(Feature = "renaming", Exclude = true)]
    public class LearningMachine : IDisposable
    {
        #region Constants

        /// <summary>
        /// Current version.
        /// Version 2: Add LearningMachineUsage.AttributeCategorization
        ///            Add LabelAttributesSettings property and backing field
        /// Version 3: Add versioning to LearningMachineDataEncoder
        /// Version 4: Add TranslateUnknownCategory property and backing field
        ///            TranslateUnknownCategoryTo property and backing field
        /// Version 5: Add CsvOutputFile property and backing field
        /// </summary>
        const int _CURRENT_VERSION = 5;

        // Encryption password for serialization, renamed to obfuscate purpose
        private static readonly byte[] _CONVERGENCE_MATRIX = new byte[64]
            {
                185, 105, 109, 83, 148, 254, 79, 173, 128, 172, 12, 76, 61, 131, 66, 69, 236, 2, 76, 172, 158,
                197, 70, 243, 131, 95, 163, 206, 89, 164, 145, 134, 6, 25, 175, 201, 97, 177, 190, 24, 163, 144,
                141, 55, 75, 250, 20, 9, 176, 172, 55, 107, 172, 231, 69, 151, 34, 7, 232, 26, 112, 63, 202, 33
            };

        /// <summary>
        /// Text to use for otherwise empty Document attribute values in pagination hierarchy
        /// </summary>
        private static readonly string _DOCUMENT_PLACEHOLDER_TEXT = "N/A";

        // Used for document categorization to represent low probability classification
        private static readonly string _UNKNOWN_CATEGORY = "Unknown";

        static readonly string _GET_FILE_LIST =
            @"SELECT DISTINCT FAMFile.FileName FROM AttributeSetForFile
            JOIN AttributeSetName ON AttributeSetForFile.AttributeSetNameID = AttributeSetName.ID
            JOIN FileTaskSession ON AttributeSetForFile.FileTaskSessionID = FileTaskSession.ID
            JOIN FAMFile ON FileTaskSession.FileID = FAMFile.ID
                WHERE Description = @AttributeSetName
                AND AttributeSetForFile.ID >= @FirstIDToProcess
                AND AttributeSetForFile.ID <= @LastIDToProcess";

        static readonly string _GET_FILE_LIST_AND_VOA =
            @"SELECT DISTINCT FAMFile.FileName, AttributeSetForFile.VOA FROM AttributeSetForFile
            JOIN AttributeSetName ON AttributeSetForFile.AttributeSetNameID = AttributeSetName.ID
            JOIN FileTaskSession ON AttributeSetForFile.FileTaskSessionID = FileTaskSession.ID
            JOIN FAMFile ON FileTaskSession.FileID = FAMFile.ID
                WHERE Description = @AttributeSetName
                AND AttributeSetForFile.ID >= @FirstIDToProcess
                AND AttributeSetForFile.ID <= @LastIDToProcess";

        #endregion Constants

        #region Fields

        /// <summary>
        /// Persist the current version so to prevent newer, incompatible, versions from being loaded
        /// </summary>
        private int _version = _CURRENT_VERSION;


        // Backing fields for properties
        private ITrainableClassifier _classifier;
        private int _randomNumberSeed;
        private bool _useUnknownCategory;
        private double _unknownCategoryCutoff;

        // Don't serialize fields with potentially sensitive information in them
        [NonSerialized]
        private InputConfiguration _inputConfig;
        [NonSerialized]
        private LearningMachineDataEncoder _encoder;
        [NonSerialized]
        private string _trainingLog;

        // Encrypted versions of potentially sensitive fields
        private Byte[] _encryptedEncoder;
        private byte[] _encryptedInputConfig;
        private string _encryptedTrainingLog;

        // Only serialize the label attributes settings if Usage is AttributeCategorization
        [NonSerialized]
        private LabelAttributes _labelAttributesSettings;

        [OptionalField(VersionAdded = 2)]
        private LabelAttributes _labelAttributesPersistedSettings;

        [OptionalField(VersionAdded = 4)]
        private bool _translateUnknownCategory;

        [OptionalField(VersionAdded = 4)]
        private string _translateUnknownCategoryTo;

        [OptionalField(VersionAdded = 5)]
        private string _csvOutputFile;

        [OptionalField(VersionAdded = 5)]
        private bool _standardizeFeaturesForCsvOutput;

        [OptionalField(VersionAdded = 5)]
        private (SerializableConfusionMatrix train, SerializableConfusionMatrix test)? _accuracyData;

        #endregion Fields

        #region Properties

        /// <summary>
        /// Gets/sets the input configuration
        /// </summary>
        public InputConfiguration InputConfig
        {
            get
            {
                return _inputConfig;
            }
            set
            {
                if (value != _inputConfig)
                {
                    _inputConfig = value;
                }
            }
        }

        /// <summary>
        /// Gets/sets the encoder to be used to create feature vectors
        /// </summary>
        public LearningMachineDataEncoder Encoder
        {
            get
            {
                return _encoder;
            }
            set
            {
                if (value != _encoder)
                {
                    _encoder = value;
                }
            }
        }

        /// <summary>
        /// Gets/sets the classifier to be trained and used to compute answers with encoded feature vectors
        /// </summary>
        public ITrainableClassifier Classifier
        {
            get
            {
                return _classifier;
            }
            set
            {
                if (value != _classifier)
                {
                    _classifier = value;
                }
            }
        }

        /// <summary>
        /// Gets the <see cref="LearningMachineUsage"/> of this instance
        /// </summary>
        public LearningMachineUsage Usage
        {
            get
            {
                return Encoder == null ? LearningMachineUsage.Unknown : Encoder.MachineUsage;
            }
        }

        /// <summary>
        /// Gets the <see cref="LearningMachineType"/> of this instance
        /// </summary>
        public LearningMachineType MachineType 
        {
            get
            {
                if (Classifier as MulticlassSupportVectorMachineClassifier != null)
                {
                    return LearningMachineType.MulticlassSVM;
                }
                else if (Classifier as MultilabelSupportVectorMachineClassifier != null)
                {
                    return LearningMachineType.MultilabelSVM;
                }
                else if (Classifier as NeuralNetworkClassifier != null)
                {
                    return LearningMachineType.ActivationNetwork;
                }
                else
                {
                    return LearningMachineType.Unknown;
                }
            }
        }

        /// <summary>
        /// Gets/sets the seed to use for random number generation
        /// </summary>
        public int RandomNumberSeed
        {
            get
            {
                return _randomNumberSeed;
            }
            set
            {
                if (value != _randomNumberSeed)
                {
                    _randomNumberSeed = value;
                }
            }
        }

        /// <summary>
        /// Gets whether this classifier has been trained and is ready to compute answers
        /// </summary>
        public bool IsTrained
        {
            get
            {
                return Classifier == null ? false : Classifier.IsTrained;
            }
        }

        /// <summary>
        /// Gets/sets whether to use Unknown as a value if answer probability is low
        /// </summary>
        public bool UseUnknownCategory
        {
            get
            {
                return _useUnknownCategory;
            }
            set
            {
                if (value != _useUnknownCategory)
                {
                    _useUnknownCategory = value;
                }
            }
        }

        /// <summary>
        /// Gets/sets the threshold level below which probability will be considered to signify Unknown
        /// </summary>
        public double UnknownCategoryCutoff
        {
            get
            {
                return _unknownCategoryCutoff;
            }
            set
            {
                if (value != _unknownCategoryCutoff)
                {
                    _unknownCategoryCutoff = value;
                }
            }
        }

        /// <summary>
        /// Gets whether this instance has been configured
        /// </summary>
        public bool IsConfigured
        {
            get
            {
                return InputConfig != null && Encoder != null && Classifier != null;
            }
        }

        /// <summary>
        /// Record of machine training
        /// </summary>
        public string TrainingLog
        {
            get
            {
                return _trainingLog;
            }
            set
            {
                if (value != _trainingLog)
                {
                    _trainingLog = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the settings for attribute labeling
        /// </summary>
        public LabelAttributes LabelAttributesSettings
        {
            get
            {
                return _labelAttributesSettings;
            }
            set
            {
                if (value != _labelAttributesSettings)
                {
                    _labelAttributesSettings = value;
                }
            }
        }

        /// <summary>
        /// Whether to translate the special Unknown class to another value.
        /// </summary>
        /// <remarks>
        /// This Unknown value is assigned when the prediction probability score does not exceed the
        /// Unknown category cutoff value.
        /// </remarks>
        public bool TranslateUnknownCategory
        {
            get
            {
                return _translateUnknownCategory;
            }
            set
            {
                _translateUnknownCategory = value;
            }
        }

        /// <summary>
        /// The value to which to translate the special Unknown class
        /// </summary>
        public string TranslateUnknownCategoryTo
        {
            get
            {
                return _translateUnknownCategoryTo ?? "";
            }
            set
            {
                _translateUnknownCategoryTo = value;
            }
        }

        /// <summary>
        /// The basename to which write feature data CSV files
        /// </summary>
        /// <remarks>".train.txt" or ".test.txt" will be added to this file name
        /// when writing out the data.</remarks>
        public string CsvOutputFile
        {
            get
            {
                return _csvOutputFile;
            }
            set
            {
                _csvOutputFile = value ?? "";
            }
        }

        /// <summary>
        /// Whether to standardize feature values before writing out to CSV
        /// </summary>
        /// <remarks>Standardizing means subtracting the mean and dividing by the standard deviation of each feature</remarks>
        public bool StandardizeFeaturesForCsvOutput
        {
            get
            {
                return _standardizeFeaturesForCsvOutput;
            }
            set
            {
                _standardizeFeaturesForCsvOutput = value;
            }
        }

        /// <summary>
        /// Accuracy data from the last training/testing session
        /// </summary>;
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public (SerializableConfusionMatrix train, SerializableConfusionMatrix test)? AccuracyData
        {
            get
            {
                return _accuracyData;
            }
            set
            {
                _accuracyData = value;
            }
        }

        #endregion Properties

        #region Public Methods

        /// <summary>
        /// Computes encodings without training
        /// </summary>
        public void ComputeEncodings()
        {
            try
            {
                ComputeEncodings(_ => { }, CancellationToken.None);
            }
            catch (Exception e)
            {
                throw e.AsExtract("ELI39811");
            }
        }

        /// <summary>
        /// Computes encodings without training
        /// </summary>
        /// <param name="updateStatus">Function to use for sending progress updates to caller</param>
        /// <param name="cancellationToken">Token indicating that processing should be canceled</param>
        public void ComputeEncodings(Action<StatusArgs> updateStatus, CancellationToken cancellationToken)
        {
            try
            {
                ExtractException.Assert("ELI39803", "Machine is not fully configured", IsConfigured);

                // Compute input files and answers
                string[] ussFiles, voaFiles, answersOrAnswerFiles;
                InputConfig.GetInputData(out ussFiles, out voaFiles, out answersOrAnswerFiles, updateStatus, cancellationToken);

                cancellationToken.ThrowIfCancellationRequested();

                Encoder.ComputeEncodings(ussFiles, voaFiles, answersOrAnswerFiles, updateStatus, cancellationToken);
            }
            catch (Exception e)
            {
                throw e.AsExtract("ELI39826");
            }
        }

        /// <summary>
        /// Trains and tests the machine using files specified with <see cref="InputConfig"/>
        /// </summary>
        /// <returns>Tuple of training set accuracy score and testing set accuracy score</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public (AccuracyData trainingSet, AccuracyData testingSet) TrainMachine()
        {
            try
            {
                return TrainAndTestMachine(testOnly: false, updateStatus: _ => { }, cancellationToken: CancellationToken.None);
            }
            catch (Exception e)
            {
                throw e.AsExtract("ELI39755");
            }
        }

        /// <summary>
        /// Trains and tests the machine using files specified with <see cref="InputConfig"/>
        /// </summary>
        /// <param name="updateStatus">Function to use for sending progress updates to caller</param>
        /// <param name="cancellationToken">Token indicating that processing should be canceled</param>
        /// <returns>Tuple of training set accuracy score and testing set accuracy score</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public (AccuracyData trainingSet, AccuracyData testingSet) TrainMachine(Action<StatusArgs> updateStatus, CancellationToken cancellationToken)
        {
            try
            {
                return TrainAndTestMachine(false, updateStatus, cancellationToken);
            }
            catch (Exception e)
            {
                throw e.AsExtract("ELI40258");
            }
        }

        /// <summary>
        /// Tests the machine using files specified with <see cref="InputConfig"/>
        /// </summary>
        /// <returns>Tuple of training set accuracy score and testing set accuracy score</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public (AccuracyData trainingSet, AccuracyData testingSet) TestMachine()
        {
            try
            {
                return TrainAndTestMachine(testOnly: true, updateStatus: _ => { }, cancellationToken: CancellationToken.None);
            }
            catch (Exception e)
            {
                throw e.AsExtract("ELI39807");
            }
        }

        /// <summary>
        /// Tests the machine using files specified with <see cref="InputConfig"/>
        /// </summary>
        /// <param name="updateStatus">Function to use for sending progress updates to caller</param>
        /// <param name="cancellationToken">Token indicating that processing should be canceled</param>
        /// <returns>Tuple of training set accuracy score and testing set accuracy score</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public (AccuracyData trainingSet, AccuracyData testingSet) TestMachine(Action<StatusArgs> updateStatus, CancellationToken cancellationToken)
        {
            try
            {
                return TrainAndTestMachine(true, updateStatus, cancellationToken);
            }
            catch (Exception e)
            {
                throw e.AsExtract("ELI40259");
            }
        }

        /// <summary>
        /// Computes an answer for the input data. Modifies <see paramref="attributeVector"/> with the answer
        /// </summary>
        /// <remarks>If <see paramref="preserveInputAttributes"/>=<see langword="true"/> and
        /// <see cref="Usage"/>=<see cref="LearningMachineUsage.Pagination"/> then the input Page <see cref="ComAttribute"/>s will be
        /// moved to be subattributes of the resulting Document <see cref="ComAttribute"/>s.</remarks>
        /// <param name="document">The <see cref="SpatialString"/> used for encoding auto-BoW features</param>
        /// <param name="attributeVector">The VOA used for encoding attribute features</param>
        /// <param name="preserveInputAttributes">Whether to preserve the input <see cref="ComAttribute"/>s or not.</param>
        public void ComputeAnswer(SpatialString document, IUnknownVector attributeVector, bool preserveInputAttributes)
        {
            try
            {
                ExtractException.Assert("ELI39762", "Machine has not been trained", IsTrained);

                ExtractException.Assert("ELI41629", "Attribute vector cannot be null", attributeVector != null);

                IEnumerable<double[]> inputs = Encoder.GetFeatureVectors(document, attributeVector);
                var outputs = inputs.Select(Classifier.ComputeAnswer);
                if (Usage == LearningMachineUsage.DocumentCategorization)
                {
                    IEnumerable<ComAttribute> categories = outputs.Select(res =>
                        {
                            string category;
                            if (UseUnknownCategory && res.score != null && res.score < UnknownCategoryCutoff)
                            {
                                category = TranslateUnknownCategory
                                    ? TranslateUnknownCategoryTo
                                    : _UNKNOWN_CATEGORY;
                            }
                            else
                            {
                                category = Encoder.AnswerCodeToName[res.answerCode];
                            }
                            var ss = new SpatialStringClass();
                            ss.CreateNonSpatialString(category, document.SourceDocName);
                            return new ComAttribute { Name = "DocumentType", Value = ss };
                        });
                    if (!preserveInputAttributes)
                    {
                        attributeVector.Clear();
                    }
                    foreach (var attr in categories)
                    {
                        attributeVector.PushBack(attr);
                    }
                }
                else if (Usage == LearningMachineUsage.Pagination)
                {
                    List<ComAttribute> inputPageAttributes = null;
                    List<ComAttribute> resultingAttributes = new List<ComAttribute>();
                    if (preserveInputAttributes)
                    {
                        inputPageAttributes = new List<ComAttribute>();
                        foreach (var attribute in attributeVector.ToIEnumerable<ComAttribute>())
                        {
                            if (attribute.Name.Equals(LearningMachineDataEncoder.PageAttributeName, StringComparison.OrdinalIgnoreCase))
                            {
                                inputPageAttributes.Add(attribute);
                            }
                            else
                            {
                                resultingAttributes.Add(attribute);
                            }
                        }
                    }

                    List<bool> isFirstPageList = outputs
                        .Select(answerAndScore => answerAndScore.Item1 == LearningMachineDataEncoder.FirstPageCategoryCode)
                        .ToList();
                    int numberOfPages = isFirstPageList.Count + 1;

                    // - 2 because isFirstPageList is zero-indexed and does not include the first page of the image
                    Func<int, bool> isFirstPage = sourcePage => isFirstPageList[sourcePage - 2];

                    var paginationAttributes = CreatePaginationAttributes(document.SourceDocName,
                        numberOfPages, isFirstPage, document, inputPageAttributes);
                    resultingAttributes.AddRange(paginationAttributes);

                    attributeVector.Clear();
                    foreach (var attr in resultingAttributes)
                    {
                        attributeVector.PushBack(attr);
                    }
                }
                else if (Usage == LearningMachineUsage.AttributeCategorization)
                {
                    var attributeCreator = new AttributeCreator(document.SourceDocName);
                    foreach(var attrAndAnswer in
                        attributeVector.ToIEnumerable<ComAttribute>()
                        .Zip(outputs, Tuple.Create))
                    {
                        var attribute = attrAndAnswer.Item1;
                        if (!preserveInputAttributes)
                        {
                            attribute.SubAttributes.Clear();
                        }

                        string category = Encoder.AnswerCodeToName[attrAndAnswer.Item2.answerCode];
                        attribute.SubAttributes.PushBack(
                            attributeCreator.Create(LearningMachineDataEncoder.CategoryAttributeName, category));
                    }
                }
                else
                {
                    throw new ExtractException("ELI39768", "Unsupported LearningMachineUsage: " + Usage.ToString());
                }
            }
            catch (Exception e)
            {
                throw e.AsExtract("ELI39770");
            }
        }

        /// <summary>
        /// Computes accuracy score(s) of this machine
        /// </summary>
        /// <remarks>Unlike the static version of this method, the instance version is affected
        /// by <see cref="UseUnknownCategory"/> and <see cref="UnknownCategoryCutoff"/> settings</remarks>
        /// <param name="inputs">The feature vectors</param>
        /// <param name="outputs">The expected results</param>
        /// <returns>An <see cref="AccuracyData"/> instance </returns>
        public AccuracyData GetAccuracyScore(double[][] inputs, int[] outputs)
        {
            try
            {
                bool unknownCategoryUsed = false;
                int[] predictions = inputs.Apply(Classifier.ComputeAnswer)
                    .Select(t =>
                        {
                            if (UseUnknownCategory && t.score.HasValue
                                && t.score < UnknownCategoryCutoff)
                            {
                                if (TranslateUnknownCategory
                                    && Encoder.AnswerNameToCode.TryGetValue(TranslateUnknownCategoryTo, out int answerCode))
                                {
                                    return answerCode;
                                }

                                // Use value beyond any that the classifier would use for unknown
                                // rather than LearningMachineDataEncoder.UnknownCategoryCode to avoid
                                // misleading 100% accuracy results
                                // https://extract.atlassian.net/browse/ISSUE-13894
                                unknownCategoryUsed = true;
                                return Classifier.NumberOfClasses;
                            }
                            else
                            {
                                return t.answerCode;
                            }
                        })

                    .ToArray();

                int numberOfClasses = unknownCategoryUsed
                    ? Classifier.NumberOfClasses + 1
                    : Classifier.NumberOfClasses;

                AccuracyData accuracyData;
                if (numberOfClasses == 2)
                {
                    var confusionMatrix = new ConfusionMatrix(predictions, outputs);
                    accuracyData = new AccuracyData(confusionMatrix);
                }
                else
                {
                    var confusionMatrix = new GeneralConfusionMatrix(numberOfClasses, predictions, outputs);
                    accuracyData = new AccuracyData(confusionMatrix);
                }

                return accuracyData;
            }
            catch (Exception e)
            {
                throw e.AsExtract("ELI39868");
            }
        }


        /// <summary>
        /// Whether this instance is configured the same as another
        /// </summary>
        /// <param name="other">The <see cref="LearningMachine"/> to compare with</param>
        /// <returns><see langword="true"/> if the configurations are the same, else <see langword="false"/></returns>
        public bool IsConfigurationEqualTo(LearningMachine other)
        {
            try
            {
                if (Object.ReferenceEquals(this, other))
                {
                    return true;
                }

                if (other == null
                    || other.MachineType != MachineType
                    || other.RandomNumberSeed != RandomNumberSeed
                    || other.UnknownCategoryCutoff != UnknownCategoryCutoff
                    || other.Usage != Usage
                    || other.UseUnknownCategory != UseUnknownCategory
                    || other.Classifier == null && Classifier != null
                    || other.Classifier != null && !other.Classifier.IsConfigurationEqualTo(Classifier)
                    || other.Encoder == null && Encoder != null
                    || other.Encoder != null && !other.Encoder.IsConfigurationEqualTo(Encoder)
                    || other.InputConfig == null && InputConfig != null
                    || other.InputConfig != null && !other.InputConfig.Equals(InputConfig)
                    || other.LabelAttributesSettings == null && LabelAttributesSettings != null
                    || other.LabelAttributesSettings != null && !other.LabelAttributesSettings.Equals(LabelAttributesSettings)
                    || other.TranslateUnknownCategory != TranslateUnknownCategory
                    || TranslateUnknownCategory && !string.Equals(other.TranslateUnknownCategoryTo, TranslateUnknownCategoryTo)
                    || !string.Equals(other.CsvOutputFile, CsvOutputFile)
                    || other.StandardizeFeaturesForCsvOutput != StandardizeFeaturesForCsvOutput
                    )
                {
                    return false;
                }

                return true;
            }
            catch (Exception e)
            {
                throw e.AsExtract("ELI39825");
            }
        }

        /// <summary>
        /// Creates an instance of <see cref="LearningMachine"/> that is a shallow clone of this instance
        /// </summary>
        public LearningMachine ShallowClone()
        {
            try
            {
                return (LearningMachine)MemberwiseClone();
            }
            catch (Exception e)
            {
                throw e.AsExtract("ELI39836");
            }
        }

        /// <summary>
        /// Save machine to specified file
        /// </summary>
        /// <param name="fileName">File name to save machine into.</param>
        public void Save(string fileName)
        {
            string tempFile = null;

            try
            {
                // Save to a temporary file
                tempFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
                using (var fstream = new FileStream(tempFile, FileMode.Create, FileAccess.Write, FileShare.None))
                using (var stream = new MemoryStream())
                using (var options = new CompressionOptions(9))
                using (var compressor = new Compressor(options))
                {
                    Save(stream);
                    var bytes = compressor.Wrap(stream.ToArray());
                    fstream.Write(bytes, 0, bytes.Length);
                }

                // Once the save process is complete, copy the file into the real destination.
                FileSystemMethods.MoveFile(tempFile, fileName, overwrite: true);

                // Set tempFile to null in case SecureDelete is turned on to avoid missing file exception
                tempFile = null;
            }
            catch (Exception e)
            {
                throw e.AsExtract("ELI39809");
            }
            finally
            {
                if (tempFile != null)
                {
                    FileSystemMethods.DeleteFile(tempFile);
                }
            }
        }

        /// <summary>
        /// Save machine to specified stream
        /// </summary>
        /// <param name="stream">Stream to save machine into</param>
        public void Save(Stream stream)
        {
            try
            {
                var serializer = new NetDataContractSerializer();
                serializer.Serialize(stream, this);
            }
            catch (Exception e)
            {
                throw e.AsExtract("ELI39810");
            }
        }

        /// <summary>
        /// Load machine from specified file
        /// </summary>
        /// <param name="fileName">File name to load machine from</param>
        /// <returns>Returns instance of <see cref="LearningMachine"/> with all properties initialized from file</returns>
        public static LearningMachine Load(string fileName)
        {
            try
            {
                LearningMachine machine = null;
                FileSystemMethods.PerformFileOperationWithRetry(() =>
                {
                    using (var decompressor = new Decompressor())
                    {
                        var bytes = File.ReadAllBytes(fileName);
                        byte[] uncompressedBytes = null;
                        if (Encoding.ASCII.GetString(bytes.Take(16).ToArray())
                            .Equals("<LearningMachine", StringComparison.Ordinal))
                        {
                            uncompressedBytes = bytes;
                        }
                        else
                        {
                            uncompressedBytes = decompressor.Unwrap(bytes);
                        }
                        using (var stream = new MemoryStream(uncompressedBytes))
                        {
                            machine = Load(stream);
                        }
                    }
                }, onlyOnSharingViolation: true);

                return machine;
            }
            catch (Exception e)
            {
                throw e.AsExtract("ELI39808");
            }
        }

        /// <summary>
        /// Load machine from specified stream.
        /// </summary>
        /// <param name="stream">Stream to load machine from.</param>
        /// <returns>Returns instance of <see cref="LearningMachine"/> class with all properties initialized from file.</returns>
        public static LearningMachine Load(Stream stream)
        {
            try
            {
                var serializer = new NetDataContractSerializer();
                serializer.AssemblyFormat = FormatterAssemblyStyle.Simple; // Allows for different assembly versions
                LearningMachine machine = (LearningMachine)serializer.Deserialize(stream);
                return machine;
            }
            catch (Exception e)
            {
                throw e.AsExtract("ELI40260");
            }
        }


        /// <summary>
        /// Creates a new instance that is a deep clone of this instance
        /// </summary>
        /// <returns>A deep clone of this instance</returns>
        public LearningMachine DeepClone()
        {
            try
            {
                var savedMachine = new System.IO.MemoryStream();
                Save(savedMachine);
                savedMachine.Position = 0;
                return Load(savedMachine);
            }
            catch (Exception e)
            {
                throw e.AsExtract("ELI39872");
            }
        }

        /// <summary>
        /// Writes out feature vectors and answers as a csv file, one file for training and one for testing sets
        /// </summary>
        /// <param name="updateStatus">Function to use for sending progress updates to caller</param>
        /// <param name="cancellationToken">Token indicating that processing should be canceled</param>
        /// <remarks>The CSV fields are: "ussFileName", "answer", features...</remarks>
        public void WriteDataToCsv(Action<StatusArgs> updateStatus, CancellationToken cancellationToken)
        {
            try
            {
                ExtractException.Assert("ELI44890", "Machine is not fully configured", IsConfigured);
                ExtractException.Assert("ELI44949", "Encodings are not computed", Encoder.AreEncodingsComputed);

                // Compute input files and answers
                InputConfig.GetInputData(out string[] ussFiles, out string[] voaFiles, out string[] answersOrAnswerFiles, updateStatus, cancellationToken);

                ExtractException.Assert("ELI44891", "No inputs available to train/test machine",
                    ussFiles.Length > 0);

                var (featureVectors, answerCodes, ussPathsPerExample) = Encoder.GetFeatureVectorAndAnswerCollections(ussFiles, voaFiles, answersOrAnswerFiles,
                    updateStatus, cancellationToken, updateAnswerCodes: false);

                var (trainingData, testingData) = CombineFeatureVectorsAndAnswers(featureVectors, answerCodes, ussPathsPerExample, updateStatus, cancellationToken);

                if (trainingData.Any())
                {
                    var trainingCsv = CsvOutputFile + ".train.csv";
                    File.WriteAllLines(trainingCsv, trainingData.Select(l => string.Join(",", l.Select(f => f.QuoteIfNeeded("\"", ",")))));
                }

                if (testingData.Any())
                {
                    var testingCsv = CsvOutputFile + ".test.csv";
                    File.WriteAllLines(testingCsv, testingData.Select(l => string.Join(",", l.Select(f => f.QuoteIfNeeded("\"", ",")))));
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI44893");
            }
        }

        /// <summary>
        /// Writes out feature vectors and answers as CSV to a database
        /// </summary>
        /// <remarks>The CSV fields are: "answer", features...</remarks>
        public void WriteDataToDatabase(CancellationToken cancelToken, string databaseServer, string databaseName, string attributeSetName, 
            string modelName, long lowestIDToProcess, long highestIDToProcess)
        {
            try
            {
                ExtractException.Assert("ELI45436", "Machine is not fully configured", IsConfigured);
                ExtractException.Assert("ELI45437", "Encodings are not computed", Encoder.AreEncodingsComputed);

                // Compute input files and answers
                var (imageFiles, answers) = GetImageFileListFromDB(databaseServer, databaseName, attributeSetName, lowestIDToProcess, highestIDToProcess);

                ExtractException.Assert("ELI45438", "No inputs available to train/test machine",
                    imageFiles.Length > 0);

                InputConfig.GetRelatedInputData(imageFiles, answers, out string[] ussFiles, out string[] voaFiles, out string[] answersOrAnswerFiles, cancelToken);

                var (featureVectors, answerCodes, ussPathsPerExample) = Encoder.GetFeatureVectorAndAnswerCollections(ussFiles, voaFiles, answersOrAnswerFiles,
                    _ => { }, cancelToken, updateAnswerCodes: true);

                var (trainingData, testingData) = CombineFeatureVectorsAndAnswers(featureVectors, answerCodes, ussPathsPerExample, _ => { }, cancelToken);

                void WriteCsvToDB(List<List<string>> data, bool isTrainingSet)
                {
                    // Build the connection string from the settings
                    SqlConnectionStringBuilder sqlConnectionBuild = new SqlConnectionStringBuilder
                    {
                        DataSource = databaseServer,
                        InitialCatalog = databaseName,
                        IntegratedSecurity = true,
                        NetworkLibrary = "dbmssocn"
                    };

                    using (var scope = new TransactionScope())
                    {
                        using (var connection = new SqlConnection(sqlConnectionBuild.ConnectionString))
                        {
                            connection.Open();

                            var cmdText = @"INSERT INTO MLData(MLModelID, FileID, IsTrainingData, DateTimeStamp, Data)
                            SELECT MLModel.ID, FAMFile.ID, @IsTrainingData, GETDATE(), @Data
                            FROM MLModel, FAMFILE WHERE MLModel.Name = @ModelName AND FAMFile.FileName = @FileName";
                            foreach (var record in data)
                            {
                                using (var cmd = new SqlCommand(cmdText, connection))
                                {
                                    cmd.Parameters.AddWithValue("@IsTrainingData", isTrainingSet.ToString());
                                    cmd.Parameters.AddWithValue("@ModelName", modelName);
                                    var ussPath = record[0];
                                    cmd.Parameters.AddWithValue("@Data", string.Join(",", record.Skip(2).Select(s => s.QuoteIfNeeded("\"", ","))));
                                    cmd.Parameters.AddWithValue("@FileName", ussPath.Substring(0, ussPath.Length - 4));
                                    cmd.ExecuteNonQuery();
                                }
                            }
                        }
                        scope.Complete();
                    }
                }

                WriteCsvToDB(trainingData, true);
                WriteCsvToDB(testingData, false);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45439");
            }
        }

        /// <summary>
        /// Trains the machine using data specified
        /// </summary>
        /// <param name="spatialString">The input document/partial document</param>
        /// <param name="inputAttributes">The input attributes (candidates and/or feature attributes)</param>
        /// <param name="answer">The expected category (or <c>null</c> if not needed, e.g., for attribute categorization)</param>
        /// <returns>Tuple of training set accuracy score and testing set accuracy score</returns>
        public void IncrementallyTrainMachine(SpatialString spatialString, IUnknownVector inputAttributes, string answer)
        {
            try
            {
                var spatialStrings = new[] { spatialString };
                var inputVOAs = new[] { inputAttributes };
                var answers = answer is null ? null : new[] { answer };

                IncrementallyTrainMachine(spatialStrings, inputVOAs, answers);
            }
            catch (ExtractException)
            {
                throw;
            }
        }

        /// <summary>
        /// Trains the machine using data specified
        /// </summary>
        /// <param name="spatialStrings">The input documents/partial documents</param>
        /// <param name="inputAttributes">The input attributes (candidates and/or feature attributes)</param>
        /// <param name="answers">The expected categories (or <c>null</c> if not needed, e.g., for attribute categorization)</param>
        /// <returns>Tuple of training set accuracy score and testing set accuracy score</returns>
        public void IncrementallyTrainMachine(SpatialString[] spatialStrings, IUnknownVector[] inputAttributes, string[] answers)
        {
            try
            {
                ExtractException.Assert("ELI44719", "Machine is not fully configured", Encoder != null && Classifier != null);
                if (Classifier is IIncrementallyTrainableClassifier classifier)
                {
                    if (!Encoder.AreEncodingsComputed)
                    {
                        Encoder.ComputeEncodings(spatialStrings, inputAttributes, answers);
                    }

                    var (trainInputs, trainOutputs) = Encoder.GetFeatureVectorAndAnswerCollections(spatialStrings, inputAttributes, answers, true);
                    var numberOfClasses = trainOutputs.Max() + 1;
                    for (int i = 0; i < trainInputs.Length; i++)
                    {
                        classifier.TrainClassifier(trainInputs[i], trainOutputs[i], numberOfClasses);
                    }
                }
                else
                {
                    throw new ExtractException("ELI44720", "Machine does not support incremental training");
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45136");
            }
        }

        #endregion Public Methods

        #region Overrides

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            var baseWriter = new StringWriter(CultureInfo.CurrentCulture);
            var writer = new IndentedTextWriter(baseWriter, "  ");
            writer.WriteLine("LearningMachine:");
            writer.Indent++;
            writer.WriteLine("Usage: {0}", Usage);
            writer.WriteLine("RandomNumberSeed: {0}", RandomNumberSeed);
            writer.WriteLine("InputConfig:");
            InputConfig.PrettyPrint(writer);
            writer.WriteLine("Encoder:");
            Encoder.PrettyPrint(writer);
            writer.WriteLine("Classifier ({0}):", MachineType);
            Classifier.PrettyPrint(writer);
            if (UseUnknownCategory)
            {
                writer.WriteLine("UnknownCategoryCutoff: {0}", UnknownCategoryCutoff);
            }
            if (TranslateUnknownCategory)
            {
                writer.WriteLine("TranslateUnknownTo: {0}", TranslateUnknownCategoryTo);
            }
            return baseWriter.ToString().Trim();
        }

        #endregion Overrides

        #region Private Methods

        /// <summary>
        /// Optionally trains and then tests the machine using files specified with <see cref="InputConfig"/>
        /// </summary>
        /// <param name="testOnly">Whether to only test, not train and test</param>
        /// <param name="updateStatus">Function to use for sending progress updates to caller</param>
        /// <param name="cancellationToken">Token indicating that processing should be canceled</param>
        /// <returns>Tuple of training set accuracy score and testing set accuracy score</returns>
        private (AccuracyData trainingSet, AccuracyData testingSet) TrainAndTestMachine(bool testOnly,
            Action<StatusArgs> updateStatus,
            CancellationToken cancellationToken)
        {
            ExtractException.Assert("ELI39840", "Machine is not fully configured", IsConfigured);

            // Compute input files and answers
            InputConfig.GetInputData(out string[] ussFiles, out string[] voaFiles, out string[] answersOrAnswerFiles, updateStatus, cancellationToken);

            ExtractException.Assert("ELI41834", "No inputs available to train/test machine",
                ussFiles.Length > 0);

            if (!Encoder.AreEncodingsComputed)
            {
                Encoder.ComputeEncodings(ussFiles, voaFiles, answersOrAnswerFiles, updateStatus, cancellationToken);
            }

            var (featureVectors, answerCodes, _) =
                Encoder.GetFeatureVectorAndAnswerCollections(ussFiles, voaFiles, answersOrAnswerFiles,
                    updateStatus, cancellationToken, updateAnswerCodes: !testOnly);

            // Divide data into training and testing subsets
            if (InputConfig.TrainingSetPercentage > 0)
            {
                var rng = new Random(RandomNumberSeed);
                GetIndexesOfSubsetsByCategory(answerCodes,
                    InputConfig.TrainingSetPercentage / 100.0, out List<int> trainIdx, out List<int> testIdx, rng);

                // Training set
                double[][] trainInputs = featureVectors.Submatrix(trainIdx);
                int[] trainOutputs = answerCodes.Submatrix(trainIdx);

                // Testing set
                double[][] testInputs = featureVectors.Submatrix(testIdx);
                int[] testOutputs = answerCodes.Submatrix(testIdx);

                // Train the classifier
                if (!testOnly)
                {
                    Classifier.TrainClassifier(trainInputs, trainOutputs, rng, updateStatus, cancellationToken);
                }

                cancellationToken.ThrowIfCancellationRequested();

                var trainResult = GetAccuracyScore(trainInputs, trainOutputs);
                var testResult = GetAccuracyScore(testInputs, testOutputs);
                AccuracyData =
                    (train: new SerializableConfusionMatrix(Encoder, trainResult),
                    test: new SerializableConfusionMatrix(Encoder, testResult));
                return (trainResult, testResult);
            }
            // If no training data, just test testing set
            else
            {
                var testResult = GetAccuracyScore(featureVectors, answerCodes);
                AccuracyData = (train: null, test: new SerializableConfusionMatrix(Encoder, testResult));
                return (null, testResult);
            }
        }


        /// <summary>
        /// Gets the image file list and, if usage is doc classification, the answers from the DB
        /// </summary>
        private (string[] imagePaths, string[] maybeAnswers) GetImageFileListFromDB(
            string databaseServer,
            string databaseName,
            string attributeSetName,
            long lowestIDToProcess,
            long highestIDToProcess)
        {
            try
            {
                // Build the connection string from the settings
                SqlConnectionStringBuilder sqlConnectionBuild = new SqlConnectionStringBuilder
                {
                    DataSource = databaseServer,
                    InitialCatalog = databaseName,
                    IntegratedSecurity = true,
                    NetworkLibrary = "dbmssocn"
                };

                using (var connection = new SqlConnection(sqlConnectionBuild.ConnectionString))
                {
                    connection.Open();

                    bool getDocTypeFromVoa = Usage == LearningMachineUsage.DocumentCategorization;
                    using (var cmd = connection.CreateCommand())
                    {
                        cmd.CommandText = getDocTypeFromVoa
                            ? _GET_FILE_LIST_AND_VOA
                            : _GET_FILE_LIST;
                        cmd.Parameters.AddWithValue("@AttributeSetName", attributeSetName);
                        cmd.Parameters.AddWithValue("@FirstIDToProcess", lowestIDToProcess);
                        cmd.Parameters.AddWithValue("@LastIDToProcess", highestIDToProcess);

                        // Set the timeout so that it waits indefinitely
                        cmd.CommandTimeout = 0;
                        var reader = cmd.ExecuteReader();
                        var imagePaths = new List<string>();
                        var answers = getDocTypeFromVoa
                            ? new List<string>()
                            : null;
                        var afutil = new AFUtilityClass();
                        foreach (IDataRecord record in reader)
                        {
                            imagePaths.Add(record.GetString(0));

                            if (getDocTypeFromVoa)
                            {
                                string answer = null;
                                if (!reader.IsDBNull(1))
                                {
                                    IUnknownVector voa = null;
                                    using (var stream = reader.GetStream(1))
                                    {
                                        voa = AttributeMethods.GetVectorOfAttributesFromSqlBinary(stream);
                                        voa.ReportMemoryUsage();
                                        answer = afutil.QueryAttributes(voa, "DocumentType", false)
                                            .ToIEnumerable<ComAttribute>()
                                            .FirstOrDefault()?.Value.String;
                                    }
                                }
                                answers.Add(answer ?? "");
                            }
                        }
                        return (imagePaths.ToArray(), answers?.ToArray());
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45435");
            }
        }

        /// <summary>
        /// Prepares training/testing data for writing to CSV or DB by zipping the separate collections together and,
        /// if <see cref="StandardizeFeaturesForCsvOutput"/> = <c>true</c>, converts to the features to Z-scores
        /// </summary>
        private (List<List<string>> training, List<List<string>> testing) CombineFeatureVectorsAndAnswers(
            double[][] featureVectors,
            int[] answerCodes,
            string[] ussPaths,
            Action<StatusArgs> updateStatus,
            CancellationToken cancellationToken)
        {
            if (StandardizeFeaturesForCsvOutput)
            {
                var mean = featureVectors.Mean();
                var sigma = featureVectors.StandardDeviation(mean);

                // Prevent divide by zero
                if (sigma.Any(factor => factor == 0))
                {
                    sigma.ApplyInPlace(factor => factor + 0.0001);
                }

                // Standardize input
                featureVectors = featureVectors.Subtract(mean).ElementwiseDivide(sigma, inPlace: true);
            }

            var docIndex = ussPaths.GroupBy(p => p)
                .SelectMany(g => g.Select((p, i) => i))
                .ToArray();

            // Training set
            double[][] trainInputs = null;
            int[] trainOutputs = null;
            string[] trainFiles = null;
            int[] trainFileIndices = null;

            // Testing set
            double[][] testInputs = null;
            int[] testOutputs = null;
            string[] testFiles = null;
            int[] testFileIndices = null;

            // Divide data into training and testing subsets
            if (InputConfig.TrainingSetPercentage > 0)
            {
                var rng = new Random(RandomNumberSeed);
                GetIndexesOfSubsetsByCategory(answerCodes,
                    InputConfig.TrainingSetPercentage / 100.0, out List<int> trainIdx, out List<int> testIdx, rng);

                // Training set
                trainInputs = featureVectors.Submatrix(trainIdx);
                trainOutputs = answerCodes.Submatrix(trainIdx);
                trainFiles = ussPaths.Submatrix(trainIdx);
                trainFileIndices = docIndex.Submatrix(trainIdx);

                // Testing set
                testInputs = featureVectors.Submatrix(testIdx);
                testOutputs = answerCodes.Submatrix(testIdx);
                testFiles = ussPaths.Submatrix(testIdx);
                testFileIndices = docIndex.Submatrix(trainIdx);
            }
            else
            {
                // Testing set
                testInputs = featureVectors;
                testOutputs = answerCodes;
                testFiles = ussPaths;
                testFileIndices = docIndex;
            }

            int numColumns = featureVectors[0].Length + 1;

            // Zip each subset together
            List<List<string>> zip(string[] subsetFiles, int[] subsetFileIndices, double[][] subsetInputs, int[] subsetOutputs, string setName)
            {
                List<List<string>> result = new List<List<string>>(subsetFiles.Length);
                for (int num = 0; num < subsetFiles.Length; num++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    updateStatus(new StatusArgs { StatusMessage = "Processing " + setName + " set records: {0:N0}", Int32Value = 1 });

                    var data = new List<string>(numColumns)
                    {
                        subsetFiles[num],
                        subsetFileIndices[num].ToString(CultureInfo.CurrentCulture),
                        Encoder.AnswerCodeToName[subsetOutputs[num]],
                    };
                    data.AddRange(subsetInputs[num].Select(n => n.ToString(CultureInfo.InvariantCulture)));
                    result.Add(data);
                }
                return result;
            }
            var trainingData = trainInputs != null && trainInputs.Any()
                ? zip(trainFiles, trainFileIndices, trainInputs, trainOutputs, "training")
                : new List<List<string>>(0);
            var testingData = zip(testFiles, testFileIndices, testInputs, testOutputs, "testing");

            return (trainingData, testingData);
        }

        /// <summary>
        /// Called when serializing
        /// </summary>
        /// <param name="context">The context.</param>
        [OnSerializing]
        private void OnSerializing(StreamingContext context)
        {
            // Encrypt input configuration
            var ml = new MapLabel();
            using (var unencryptedStream = new MemoryStream())
            using (var encryptedStream = new MemoryStream())
            {
                var serializer = new NetDataContractSerializer();
                serializer.AssemblyFormat = FormatterAssemblyStyle.Simple;
                serializer.Serialize(unencryptedStream, InputConfig);
                unencryptedStream.Position = 0;
                ExtractEncryption.EncryptStream(unencryptedStream, encryptedStream, _CONVERGENCE_MATRIX, ml);
                _encryptedInputConfig = encryptedStream.ToArray();
            }

            // Encrypt data encoder
            using (var unencryptedStream = new MemoryStream())
            using (var encryptedStream = new MemoryStream())
            {
                var serializer = new NetDataContractSerializer();
                serializer.AssemblyFormat = FormatterAssemblyStyle.Simple;
                serializer.Serialize(unencryptedStream, Encoder);
                unencryptedStream.Position = 0;
                ExtractEncryption.EncryptStream(unencryptedStream, encryptedStream, _CONVERGENCE_MATRIX, ml);
                _encryptedEncoder = encryptedStream.ToArray();
            }

            // Encrypt training log
            if (TrainingLog != null)
            {
                _encryptedTrainingLog = ExtractEncryption.EncryptString(TrainingLog, ml);
            }

            // Set the label attributes settings if usage is AttributeCategorization
            if (Usage == LearningMachineUsage.AttributeCategorization)
            {
                _labelAttributesPersistedSettings = _labelAttributesSettings;
            }
            else
            {
                _labelAttributesPersistedSettings = null;
            }

            if (_version == 5)
            {
                if (IsCompatibleWithVersion(4))
                {
                    _version = 4;
                }
            }

            if (_version == 4)
            {
                if (IsCompatibleWithVersion(3))
                {
                    _version = 3;
                }
            }

            // Since v1 of LearningMachineDataEncoder had no versioning, this object's version
            // was incremented at the same time versioning was added to that object so as to prevent
            // incompatible usage.
            // But, in order to allow for use in older software when possible, decrease the version if this object
            // is compatible with the previous version
            if (_version == 3)
            {
                if (IsCompatibleWithVersion(2))
                {
                    _version = 2;
                }
            }
        }

        [OnSerialized]
        private void OnSerialized(StreamingContext context)
        {
            // Reset the version that may have been decremented for serialization
            _version = _CURRENT_VERSION;
        }

        /// <summary>
        /// Gets a value indicating whether this instance is safe (won't behave differently)
        /// if used by software that is unaware of features added in <see cref="_version" />
        /// </summary>
        /// <param name="version">The version in question.</param>
        /// <returns>
        ///   <c>true</c> if this instance is compatible with the specified version
        /// </returns>
        internal bool IsCompatibleWithVersion(int version)
        {
            if (_version == 5 && version == 4)
            {
                return string.IsNullOrEmpty(CsvOutputFile);
            }

            if (_version == 4 && version == 3)
            {
                if (TranslateUnknownCategory
                    && !string.Equals(TranslateUnknownCategoryTo, "Unknown", StringComparison.Ordinal))
                {
                    return false;
                }
                return true;
            }

            if (_version == 3 && version == 2)
            {
                // LearningMachineDataEncoder v1 corresponds to LearningMachine v2
                return Encoder.IsCompatibleWithVersion(1);
            }
            return _version <= version;
        }

        /// <summary>
        /// Called when deserializing
        /// </summary>
        /// <param name="context">The context.</param>
        [OnDeserializing]
        private void OnDeserializing(StreamingContext context)
        {
            _labelAttributesPersistedSettings = null;
            _translateUnknownCategory = false;
            _translateUnknownCategoryTo = null;
        }

        /// <summary>
        /// Called when deserialized
        /// </summary>
        /// <param name="context">The context.</param>
        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            // Don't support loading newer versions
            ExtractException.Assert("ELI40071", "Cannot load newer LearningMachine",
                _version <= _CURRENT_VERSION,
                "Current version", _CURRENT_VERSION,
                "Version to load", _version);

            // Update version number
            _version = _CURRENT_VERSION;

            // Decrypt input configuration
            var ml = new MapLabel();
            using (var encryptedStream = new MemoryStream(_encryptedInputConfig))
            using (var unencryptedStream = new MemoryStream())
            {
                ExtractEncryption.DecryptStream(encryptedStream, unencryptedStream, _CONVERGENCE_MATRIX, ml);
                unencryptedStream.Position = 0;
                var serializer = new NetDataContractSerializer();
                serializer.AssemblyFormat = FormatterAssemblyStyle.Simple;
                InputConfig = (InputConfiguration)serializer.Deserialize(unencryptedStream);
            }

            // Decrypt data encoder
            using (var encryptedStream = new MemoryStream(_encryptedEncoder))
            using (var unencryptedStream = new MemoryStream())
            {
                ExtractEncryption.DecryptStream(encryptedStream, unencryptedStream, _CONVERGENCE_MATRIX, ml);
                unencryptedStream.Position = 0;
                var serializer = new NetDataContractSerializer();
                serializer.AssemblyFormat = FormatterAssemblyStyle.Simple;
                Encoder = (LearningMachineDataEncoder)serializer.Deserialize(unencryptedStream);
            }

            // Decrypt training log
            if (_encryptedTrainingLog != null)
            {
                TrainingLog = ExtractEncryption.DecryptString(_encryptedTrainingLog, ml);
            }

            // Update property from persisted value
            LabelAttributesSettings = _labelAttributesPersistedSettings;
        }

        /// <summary> Creates a collection of document <see cref="ComAttribute"/>s that represents pagination boundaries
        /// (Document/Pages|start-end).
        /// </summary>
        /// <param name="sourceDocName">Name of the source document</param>
        /// <param name="numberOfPages">The number of pages in the source document</param>
        /// <param name="isFirstPage">Function used to determine whether a source page number is
        /// the first page of a paginated document</param>
        /// <param name="sourceDocument">Optional source document <see cref="SpatialString"/></param>
        /// <param name="inputPageAttributes">Optional collection of one attribute per page to be
        /// added as subattributes to the appropriate Document attribute in the output</param>
        /// <returns>A collection of Document/Page attributes</returns>
        private static IEnumerable<ComAttribute> CreatePaginationAttributes(
            string sourceDocName,
            int numberOfPages,
            Func<int, bool> isFirstPage,
            ISpatialString sourceDocument = null,
            IList<ComAttribute> inputPageAttributes = null)
        {
            try
            {
                ExtractException.Assert("ELI40157", "Incorrect number of inputPageAttributes",
                    inputPageAttributes == null || inputPageAttributes.Count == numberOfPages);

                var resultingAttributes = new List<ComAttribute>();
                int firstPageInRange = 1;
                for (int nextPageNumber = 2; nextPageNumber <= numberOfPages + 1; nextPageNumber++)
                {
                    if (nextPageNumber > numberOfPages || isFirstPage(nextPageNumber))
                    {
                        int lastPageInRange = nextPageNumber - 1;
                        string range = Enumerable
                            .Range(firstPageInRange, lastPageInRange - firstPageInRange + 1)
                            .ToRangeString();
                        SpatialString ss;
                        if (sourceDocument != null)
                        {
                            // Get OCRed text for the page range for the Document value
                            ss = sourceDocument.GetSpecifiedPages(firstPageInRange, lastPageInRange);
                        }
                        else
                        {
                            ss = new SpatialStringClass();
                        }

                        // Prevent empty value that could result in the attribute getting thrown away
                        if (string.IsNullOrEmpty(ss.String))
                        {
                            ss.CreateNonSpatialString(_DOCUMENT_PLACEHOLDER_TEXT, sourceDocName);
                        }

                        var documentAttribute = new ComAttribute { Name = "Document", Value = ss };

                        // Add a Pages attribute to denote the range of pages in this document
                        ss = new SpatialStringClass();
                        ss.CreateNonSpatialString(range, sourceDocName);
                        documentAttribute.SubAttributes.PushBack(new ComAttribute { Name = "Pages", Value = ss });
                        resultingAttributes.Add(documentAttribute);

                        // Add input page attributes that are in this range
                        if (inputPageAttributes != null)
                        {
                            for (int i = firstPageInRange - 1;
                                i < lastPageInRange && i < inputPageAttributes.Count; i++)
                            {
                                documentAttribute.SubAttributes.PushBack(inputPageAttributes[i]);
                            }
                        }

                        // Set up next page range
                        firstPageInRange = nextPageNumber;
                    }
                }

                return resultingAttributes;
            }
            catch (Exception e)
            {
                throw e.AsExtract("ELI40158");
            }
        }

        #endregion Private Methods

        #region IDisposable Members

        /// <summary>
        /// Releases all resources used by the <see cref="LearningMachine"/>.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <overloads>Releases resources used by the <see cref="LearningMachine"/>.</overloads>
        /// <summary>
        /// Releases all unmanaged resources used by the <see cref="LearningMachine"/>.
        /// </summary>
        /// <param name="disposing"><see langword="true"/> to release both managed and unmanaged 
        /// resources; <see langword="false"/> to release only unmanaged resources.</param>        
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (Classifier is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }

            // Dispose of unmanaged resources
        }

        #endregion IDisposable Members

        #region Static Methods

        /// <summary>
        /// Split data into two subset by computing the indexes of random subsets of each category. At least one example
        /// of each category will be represented in each subset so the subsets may overlap by one.
        /// </summary>
        /// <param name="categories">Category codes for each example in the set of data</param>
        /// <param name="subset1Fraction">The fraction of indexes to be selected for the first subset</param>
        /// <param name="subset1Indexes">The indexes selected for the first subset</param>
        /// <param name="subset2Indexes">The indexes selected for the second subset</param>
        /// <param name="randomGenerator">Optional random number generator used to select the subsets</param>
        [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters")]
        public static void GetIndexesOfSubsetsByCategory<TCategory>(TCategory[] categories, double subset1Fraction,
            out List<int> subset1Indexes, out List<int> subset2Indexes, Random randomGenerator=null)
            where TCategory : IComparable
        {
            try
            {
                ExtractException.Assert("ELI39761", "Fraction must be between 0 and 1", subset1Fraction <= 1 && subset1Fraction >= 0);
                subset1Indexes = new List<int>();
                subset2Indexes = new List<int>();
                foreach(var category in categories.Distinct())
                {
                    // Retrieve the indexes for this category
                    int[] idx = categories.Find(x => x.CompareTo(category) == 0);
                    if (idx.Length > 0)
                    {
                        int subset1Size = Math.Max((int)Math.Round(idx.Length * subset1Fraction), 1);
                        int subset2Size = Math.Max(idx.Length - subset1Size, 1);
                        Utilities.CollectionMethods.Shuffle(idx, randomGenerator);
                        var subset1 = idx.Submatrix(0, subset1Size - 1);
                        var subset2 = idx.Submatrix(idx.Length - subset2Size, idx.Length - 1);
                        subset1Indexes.AddRange(subset1);
                        subset2Indexes.AddRange(subset2);
                    }
                }
            }
            catch (Exception e)
            {
                throw e.AsExtract("ELI39738");
            }
        }

        /// <summary>
        /// Computes the accuracy or F1 score of the classifier
        /// </summary>
        /// <param name="classifier">The <see cref="ITrainableClassifier"/> to use to compute answers</param>
        /// <param name="inputs">The feature vectors</param>
        /// <param name="outputs">The expected results</param>
        /// <returns>The F1 score if there are two classes else the overall agreement</returns>
        public static double GetAccuracyScore(ITrainableClassifier classifier, double[][] inputs, int[] outputs)
        {
            try
            {
                int[] predictions = inputs.Apply(classifier.ComputeAnswer).Select(t => t.Item1).ToArray();
                if (classifier.NumberOfClasses == 2)
                {
                    var cm = new ConfusionMatrix(predictions, outputs);
                    return Double.IsNaN(cm.FScore) ? 0.0 : cm.FScore;
                }
                else
                {
                    var gc = new GeneralConfusionMatrix(classifier.NumberOfClasses, predictions, outputs);
                    return gc.OverallAgreement;
                }
            }
            catch (Exception e)
            {
                throw e.AsExtract("ELI39739");
            }
        }

        /// <summary>
        /// Computes an answer for the input data
        /// </summary>
        /// <remarks>If <see paramref="preserveInputAttributes"/>=<see langword="true"/> and
        /// <see cref="Usage"/>=<see cref="LearningMachineUsage.Pagination"/> then the input Page <see cref="ComAttribute"/>s will be
        /// returned as subattributes of the resulting Document <see cref="ComAttribute"/>s.</remarks>
        /// <param name="learningMachinePath">Path to saved <see cref="LearningMachine"/></param>
        /// <param name="document">The <see cref="SpatialString"/> used for encoding auto-BoW features</param>
        /// <param name="attributeVector">The VOA used for encoding attribute features</param>
        /// <param name="preserveInputAttributes">Whether to preserve the input <see cref="ComAttribute"/>s or not.</param>
        /// <returns>A VOA representation of the computed answer</returns>
        public static void ComputeAnswer(string learningMachinePath, SpatialString document,
            IUnknownVector attributeVector, bool preserveInputAttributes)
        {
            try
            {
                var machine = FileDerivedResourceCache.GetCachedObject(
                    paths: learningMachinePath,
                    creator: () => Load(learningMachinePath));

                machine.ComputeAnswer(document, attributeVector, preserveInputAttributes);
            }
            catch (Exception e)
            {
                throw e.AsExtract("ELI40156");
            }
        }

        #endregion Static Methods
    }
}