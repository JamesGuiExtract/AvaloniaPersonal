﻿using Accord.Math;
using Accord.Statistics;
using Accord.Statistics.Analysis;
using Extract.Encryption;
using Extract.Licensing;
using Extract.SqlDatabase;
using Extract.Utilities;
using LearningMachineTrainer;
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
using UCLID_AFUTILSLib;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;
using UCLID_RASTERANDOCRMGMTLib;
using ZstdNet;
using AccuracyData = Extract.Utilities.Union<Accord.Statistics.Analysis.GeneralConfusionMatrix, Accord.Statistics.Analysis.ConfusionMatrix>;
using AttributeOrAnswerCollection = Extract.Utilities.Union<string[], byte[][]>;
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
    public sealed class LearningMachine : ILearningMachineModel, IDisposable
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
        /// Version 6: No longer encrypt Encoder and training log
        /// </summary>
        const int _CURRENT_VERSION = 6;

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

        // Encrypted versions of potentially sensitive fields
        [Obsolete("Use _encoder instead")]
        private byte[] _encryptedEncoder;
        private byte[] _encryptedInputConfig;
        [Obsolete("Use _trainingLog instead")]
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

        [OptionalField(VersionAdded = 6)]
        private LearningMachineDataEncoder _encoder;

        [OptionalField(VersionAdded = 6)]
        private string _trainingLog;

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
                // Make this property use null for empty value, vs empty string
                // used elsewhere so that the serializer doesn't share refs (causes error with trainer application)
                if (string.IsNullOrEmpty(value))
                {
                    _csvOutputFile = null;
                }
                else
                {
                    _csvOutputFile = value;
                }
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

        IClassifierModel ILearningMachineModel.Classifier { get => Classifier; set => Classifier = value as ITrainableClassifier; }

        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        List<string> ILearningMachineModel.AnswerCodeToName { get => Encoder.AnswerCodeToName; set => Encoder.AnswerCodeToName = value; }

        Dictionary<string, int> ILearningMachineModel.AnswerNameToCode => Encoder.AnswerNameToCode;

        string ILearningMachineModel.NegativeClassName => Encoder.NegativeClassName;

        ILearningMachineDataEncoderModel ILearningMachineModel.Encoder { get => Encoder; set => Encoder = value as LearningMachineDataEncoder; }

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
                InputConfig.GetInputData(out ussFiles, out voaFiles, out answersOrAnswerFiles, updateStatus, cancellationToken, false);

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
        /// Computes an answer for the input data. Modifies attributeVector with the answer
        /// </summary>
        /// <remarks>If <see paramref="preserveInputAttributes"/>=<see langword="true"/> and
        /// <see cref="Usage"/>=<see cref="LearningMachineUsage.Pagination"/> then the input Page <see cref="ComAttribute"/>s will be
        /// moved to be subattributes of the resulting Document <see cref="ComAttribute"/>s.</remarks>
        /// <param name="document">The <see cref="ISpatialString"/> used for encoding auto-BoW features</param>
        /// <param name="attributeVector">The VOA used for encoding attribute features</param>
        /// <param name="preserveInputAttributes">Whether to preserve the input <see cref="ComAttribute"/>s or not.</param>
        public void ComputeAnswer(ISpatialString document, IUnknownVector attributeVector, bool preserveInputAttributes)
        {
            try
            {
                ExtractException.Assert("ELI39762", "Machine has not been trained", IsTrained);

                ExtractException.Assert("ELI41629", "Attribute vector cannot be null", attributeVector != null);

                string unknownCategory = TranslateUnknownCategoryTo;
                if (UseUnknownCategory && !TranslateUnknownCategory)
                {
                    switch (Usage)
                    {
                        case LearningMachineUsage.AttributeCategorization:
                            unknownCategory = "";
                            break;
                        case LearningMachineUsage.Deletion:
                            unknownCategory = LearningMachineDataEncoder.NotDeletedPageCategory;
                            break;
                        case LearningMachineUsage.DocumentCategorization:
                            unknownCategory = _UNKNOWN_CATEGORY;
                            break;
                        case LearningMachineUsage.Pagination:
                            unknownCategory = LearningMachineDataEncoder.NotFirstPageCategory;
                            break;
                        default:
                            throw new ArgumentException(
                                UtilityMethods.FormatCurrent($"Unknown machine learning usage! {Usage}"));
                    }
                }

                IEnumerable<double[]> inputs = Encoder.GetFeatureVectors(document, attributeVector);
                List<(int answerCode, double? score)> outputsAndScores = inputs.Select(v => Classifier.ComputeAnswer(v)).ToList();
                List<string> outputs = (UseUnknownCategory
                        ? outputsAndScores
                            .Select(t =>
                                t.score is double score && score < UnknownCategoryCutoff
                                    ? unknownCategory
                                    : Encoder.AnswerCodeToName[t.answerCode])
                        : outputsAndScores.Select(t => Encoder.AnswerCodeToName[t.answerCode]))
                    .ToList();

                if (Usage == LearningMachineUsage.DocumentCategorization)
                {
                    if (!preserveInputAttributes)
                    {
                        attributeVector.Clear();
                    }
                    foreach (var category in outputs)
                    {
                        var ss = new SpatialStringClass();
                        ss.CreateNonSpatialString(category, document.SourceDocName);
                        var attr = new ComAttribute { Name = "DocumentType", Value = ss };
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
                            if (attribute.Name.Equals(SpecialAttributeNames.Page, StringComparison.OrdinalIgnoreCase))
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
                        .Select(answer => string.Equals(answer, LearningMachineDataEncoder.FirstPageCategory, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                    int numberOfPages = isFirstPageList.Count + 1;

                    // - 2 because isFirstPageList is zero-indexed and does not include the first page of the image
                    bool isFirstPage(int sourcePage) => sourcePage == 1
                        ? true
                        : isFirstPageList[sourcePage - 2];
                    double pageScore(int sourcePage) => sourcePage == 1 || sourcePage > numberOfPages
                        ? 1
                        : outputsAndScores[sourcePage - 2].score ?? double.NaN;

                    var paginationAttributes = CreatePaginationAttributes(document.SourceDocName,
                        numberOfPages, isFirstPage, pageScore, document, inputPageAttributes);
                    resultingAttributes.AddRange(paginationAttributes);

                    attributeVector.Clear();
                    foreach (var attr in resultingAttributes)
                    {
                        attributeVector.PushBack(attr);
                    }
                }
                else if (Usage == LearningMachineUsage.Deletion)
                {
                    if (!preserveInputAttributes)
                    {
                        attributeVector.Clear();
                    }

                    List<bool> isDeletedPageList = outputs
                        .Select(answer => answer == LearningMachineDataEncoder.DeletedPageCategory)
                        .ToList();
                    int numberOfPages = isDeletedPageList.Count;

                    // - 1 because isDeletedPageList is zero-indexed
                    Func<int, bool> isDeletedPage = sourcePage => isDeletedPageList[sourcePage - 1];

                    var deletedPages = CreateDeletedPagesAttribute(document.SourceDocName, numberOfPages, isDeletedPage);
                    if (deletedPages != null)
                    {
                        attributeVector.PushBack(deletedPages);
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

                        string category = attrAndAnswer.Item2;
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
                tempFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".tmp");
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
                serializer.AssemblyFormat = FormatterAssemblyStyle.Simple;
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
                using (var savedMachine = new MemoryStream())
                {
                    Save(savedMachine);
                    savedMachine.Position = 0;
                    return Load(savedMachine);
                }
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
        /// <remarks>The CSV fields are: "ussFileName", "index", "answer", features...</remarks>
        public void WriteDataToCsv(Action<StatusArgs> updateStatus, CancellationToken cancellationToken)
        {
            try
            {
                ExtractException.Assert("ELI44890", "Machine is not fully configured", IsConfigured);
                ExtractException.Assert("ELI44949", "Encodings are not computed", Encoder.AreEncodingsComputed);

                // Compute input files and answers
                InputConfig.GetInputData(out string[] ussFiles, out string[] voaFiles, out string[] answersOrAnswerFiles,
                    updateStatus, cancellationToken, false);

                ExtractException.Assert("ELI44891", "No inputs available to train/test machine",
                    ussFiles.Length > 0);

                var (featureVectors, answerCodes, ussPathsPerExample) =
                    Encoder.GetFeatureVectorAndAnswerCollections(ussFiles, voaFiles,
                    AttributeOrAnswerCollection.Maybe(answersOrAnswerFiles),
                        updateStatus, cancellationToken, updateAnswerCodes: false);

                var (trainingData, testingData) = CombineFeatureVectorsAndAnswers(featureVectors, answerCodes, ussPathsPerExample, updateStatus, cancellationToken);

                if (trainingData != null)
                {
                    var trainingCsv = CsvOutputFile + ".train.csv";
                    File.WriteAllLines(trainingCsv, trainingData.Select(l => string.Join(",", l.Select(f => f.QuoteIfNeeded("\"", ",")))));
                }

                if (testingData != null)
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
        /// Gets feature vectors and answers as CSV to be written to a database
        /// </summary>
        /// <remarks>The CSV fields are: "answer", features...</remarks>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public (IList<IList<string>> trainingData, IList<IList<string>> testingData)
            GetDataToWriteToDatabase(
                CancellationToken cancelToken,
                string databaseServer,
                string databaseName,
                string attributeSetName,
                long lowestIDToProcess,
                long highestIDToProcess,
                bool useAttributeSetForExpected,
                bool runRuleSetForFeatures,
                bool runRuleSetIfFeaturesAreMissing,
                string featureRuleSetName)
        {
            try
            {
                ExtractException.Assert("ELI45436", "Machine is not fully configured", IsConfigured);
                ExtractException.Assert("ELI45437", "Encodings are not computed", Encoder.AreEncodingsComputed);

                // Compute input files and answers
                var (imageFiles, answers) = GetImageFileListFromDB(databaseServer, databaseName, attributeSetName,
                    lowestIDToProcess, highestIDToProcess, useAttributeSetForExpected);

                // It is possible that all files are missing so just return without doing anything if no images are found
                if (imageFiles.Length == 0)
                {
                    return (new string[0][], new string[0][]);
                }

                // If usage is attribute categorization and we are not using the attribute set for answers, then in order to
                // label rules-generated candidates the paths to source-of-labels VOAs need to be supplied to the downstream process,
                // not the answers configured in the input configuration.
                // (If the attribute set is being used then that is treated as the source of labels to the candidate attributes)
                // https://extract.atlassian.net/browse/ISSUE-15716
                string sourceOfLabelsOverrideOfAnswerPath = null;
                if (Usage == LearningMachineUsage.AttributeCategorization
                    && !useAttributeSetForExpected
                    && (runRuleSetForFeatures || runRuleSetIfFeaturesAreMissing))
                {
                    sourceOfLabelsOverrideOfAnswerPath = LabelAttributesSettings?.SourceOfLabelsPath;
                }

                InputConfig.GetRelatedInputData(imageFiles, answers, runRuleSetForFeatures, sourceOfLabelsOverrideOfAnswerPath,
                    out string[] ussFiles, out string[] voaFiles, out AttributeOrAnswerCollection answersOrAnswerFiles,
                    cancelToken);

                // Get the alternate FKB dir from the DB
                var fpdb = new FileProcessingDB { DatabaseServer = databaseServer, DatabaseName = databaseName };
                string altCDD = fpdb.GetDBInfoSetting("AlternateComponentDataDir", false);
                fpdb.CloseAllDBConnections();

                // Compute the data
                var (featureVectors, answerCodes, ussPathsPerExample) = Encoder.GetFeatureVectorAndAnswerCollections(
                    ussFiles,
                    voaFiles,
                    answersOrAnswerFiles,
                    runRuleSetForFeatures,
                    runRuleSetIfFeaturesAreMissing,
                    featureRuleSetName,
                    LabelAttributesSettings,
                    altCDD,
                    _ => { },
                    cancelToken,
                    updateAnswerCodes: true);
                // It is possible that no files in a batch have any candidate attributes so that even though there are images
                // there will be no feature vectors to write
                if (featureVectors.Length == 0)
                {
                    return (new string[0][], new string[0][]);
                }

                var (trainingData, testingData) = CombineFeatureVectorsAndAnswers(featureVectors, answerCodes, ussPathsPerExample, _ => { }, cancelToken);
                return (trainingData?.ToList() ?? new List<IList<string>>(), testingData?.ToList() ?? new List<IList<string>>());
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
            writer.WriteLine("CSV data basename: {0}", CsvOutputFile ?? "");
            writer.WriteLine("Encoder:");
            Encoder.PrettyPrint(writer);
            writer.WriteLine("Classifier ({0}):", MachineType);
            Classifier.PrettyPrint(writer);
            if (Classifier is SupportVectorMachineClassifier svm
                && svm.CalibrateMachineToProduceProbabilities
                && UseUnknownCategory)
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
        private (AccuracyData trainingSet, AccuracyData testingSet) TrainAndTestMachine(
            bool testOnly,
            Action<StatusArgs> updateStatus,
            CancellationToken cancellationToken)
        {
            ExtractException.Assert("ELI39840", "Machine is not fully configured", IsConfigured);

            // Compute input files and answers
            InputConfig.GetInputData(out string[] ussFiles, out string[] voaFiles, out string[] answersOrAnswerFiles, updateStatus, cancellationToken, false);

            ExtractException.Assert("ELI41834", "No inputs available to train/test machine",
                ussFiles.Length > 0);

            if (!Encoder.AreEncodingsComputed)
            {
                Encoder.ComputeEncodings(ussFiles, voaFiles, answersOrAnswerFiles, updateStatus, cancellationToken);
            }

            var (featureVectors, answerCodes, _) =
                Encoder.GetFeatureVectorAndAnswerCollections(ussFiles, voaFiles,
                    AttributeOrAnswerCollection.Maybe(answersOrAnswerFiles),
                    updateStatus, cancellationToken, updateAnswerCodes: !testOnly);

            // Divide data into training and testing subsets
            if (InputConfig.TrainingSetPercentage > 0)
            {
                var rng = new Random(RandomNumberSeed);
                LearningMachineMethods.GetIndexesOfSubsetsByCategory(answerCodes,
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

                // Training mutates the trainInputs array in order to save memory so don't modify it again
                // if training has taken place
                var trainResult = LearningMachineMethods.GetAccuracyScore(this, trainInputs, trainOutputs, standardizeInputs: testOnly);
                var testResult = LearningMachineMethods.GetAccuracyScore(this, testInputs, testOutputs, standardizeInputs: true);
                AccuracyData =
                    (train: new SerializableConfusionMatrix(Encoder, trainResult),
                    test: new SerializableConfusionMatrix(Encoder, testResult));
                return (trainResult, testResult);
            }
            // If no training data, just test testing set
            else
            {
                var testResult = LearningMachineMethods.GetAccuracyScore(this, featureVectors, answerCodes, standardizeInputs: true);
                AccuracyData = (train: null, test: new SerializableConfusionMatrix(Encoder, testResult));
                return (null, testResult);
            }
        }

        /// <summary>
        /// Gets the image file list and, if usage is doc classification, the answers from the DB
        /// </summary>
        private (string[] imagePaths, AttributeOrAnswerCollection maybeAnswers) GetImageFileListFromDB(
            string databaseServer,
            string databaseName,
            string attributeSetName,
            long lowestIDToProcess,
            long highestIDToProcess,
            bool useAttributeSetForExpected)
        {
            try
            {
                using var connection = new ExtractRoleConnection(databaseServer, databaseName);
                connection.Open();

                bool getDocTypeFromVoa = useAttributeSetForExpected
                    && Usage == LearningMachineUsage.DocumentCategorization;

                // Check for a configuration error of using the LM specification for answers
                // when the LM doesn't specify a pattern
                ExtractException.Assert("ELI45988", "No answer source available. Try using the attribute set for expected values",
                    Usage != LearningMachineUsage.DocumentCategorization
                    || useAttributeSetForExpected
                    || !string.IsNullOrEmpty(InputConfig.AnswerPath));

                using var cmd = connection.CreateCommand();
                cmd.CommandText = useAttributeSetForExpected
                    ? _GET_FILE_LIST_AND_VOA
                    : _GET_FILE_LIST;
                cmd.Parameters.AddWithValue("@AttributeSetName", attributeSetName);
                cmd.Parameters.AddWithValue("@FirstIDToProcess", lowestIDToProcess);
                cmd.Parameters.AddWithValue("@LastIDToProcess", highestIDToProcess);

                // Set the timeout so that it waits indefinitely
                cmd.CommandTimeout = 0;
                using var reader = cmd.ExecuteReader();
                var imagePaths = new List<string>();
                var answers = getDocTypeFromVoa
                    ? new List<string>()
                    : null;
                var answerVOAs = !getDocTypeFromVoa && useAttributeSetForExpected
                    ? new List<byte[]>()
                    : null;
                var afutil = new AFUtilityClass();
                foreach (IDataRecord record in reader)
                {
                    // Filter out images that are missing or have missing uss files
                    // so that this process doesn't log a ton of errors and/or fail at some later point
                    var imagePath = record.GetString(0);
                    var ussPath = imagePath + ".uss";
                    var imageExists = File.Exists(imagePath);
                    var ussExists = File.Exists(ussPath);
                    if (!imageExists || !ussExists)
                    {
                        var ue = new ExtractException("ELI46574", "Missing ML data source file");
                        if (!imageExists)
                        {
                            ue.AddDebugData("Missing image file", imagePath);
                        }
                        if (!ussExists)
                        {
                            ue.AddDebugData("Missing uss file", ussPath);
                        }
                        ue.Log();
                        continue;
                    }

                    if (useAttributeSetForExpected)
                    {
                        object answer = null;
                        if (!reader.IsDBNull(1))
                        {
                            using (var stream = reader.GetStream(1))
                            {
                                if (getDocTypeFromVoa)
                                {
                                    var voa = AttributeMethods.GetVectorOfAttributesFromSqlBinary(stream);
                                    voa.ReportMemoryUsage();
                                    answer = afutil.QueryAttributes(voa, "DocumentType", false)
                                        .ToIEnumerable<ComAttribute>()
                                        .FirstOrDefault()?.Value.String;

                                    // Skip file if no document type exists
                                    // https://extract.atlassian.net/browse/ISSUE-15724
                                    if (answer == null)
                                    {
                                        var ue = new ExtractException("ELI46575", "Missing DocumentType for ML training (file will be skipped)");
                                        ue.AddDebugData("Image file", imagePath);
                                        ue.AddDebugData("Attribute set name", attributeSetName);
                                        ue.Log();
                                        continue;
                                    }
                                }
                                else
                                {
                                    answer = stream.ToByteArray();
                                }
                            }
                        }
                        if (getDocTypeFromVoa)
                        {
                            answers.Add((string)answer ?? "");
                        }
                        else
                        {
                            answerVOAs.Add((byte[])answer);
                        }
                    }

                    imagePaths.Add(record.GetString(0));
                }
                var attributeOrAnswerCollection = getDocTypeFromVoa
                    ? new AttributeOrAnswerCollection(answers.ToArray())
                    : useAttributeSetForExpected
                        ? new AttributeOrAnswerCollection(answerVOAs.ToArray())
                        : null;
                return (imagePaths.ToArray(), attributeOrAnswerCollection);

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
        private (IEnumerable<IList<string>> training, IEnumerable<IList<string>> testing)
            CombineFeatureVectorsAndAnswers(
            double[][] featureVectors,
            int[] answerCodes,
            string[] ussPaths,
            Action<StatusArgs> updateStatus,
            CancellationToken cancellationToken)
        {
            if (StandardizeFeaturesForCsvOutput)
            {
                featureVectors.Standardize();
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
                LearningMachineMethods.GetIndexesOfSubsetsByCategory(answerCodes,
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
                testFileIndices = docIndex.Submatrix(testIdx);
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
            IEnumerable<List<string>> zip(string[] subsetFiles, int[] subsetFileIndices, double[][] subsetInputs, int[] subsetOutputs, string setName)
            {
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
                    yield return data;
                }
            }

            var trainingData = trainInputs != null && trainInputs.Any()
                ? zip(trainFiles, trainFileIndices, trainInputs, trainOutputs, "training")
                : null;

            var testingData = testInputs != null && testInputs.Any()
                ? zip(testFiles, testFileIndices, testInputs, testOutputs, "testing")
                : null;

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
                // Change new enum values InputType.TextFile and InputType.CSV
                // to old value for serializing so that machines can be used by older
                // versions of the software
                var objectToSerialize = InputConfig;
                if (InputConfig.InputPathType == InputType.TextFile
                    || InputConfig.InputPathType == InputType.Csv)
                {
                    objectToSerialize = InputConfig.ShallowClone();
#pragma warning disable CS0618 // Type or member is obsolete
                    objectToSerialize.InputPathType = InputType.TextFileOrCsv;
#pragma warning restore CS0618 // Type or member is obsolete
                }
                var serializer = new NetDataContractSerializer();
                serializer.AssemblyFormat = FormatterAssemblyStyle.Simple;
                serializer.Serialize(unencryptedStream, objectToSerialize);
                unencryptedStream.Position = 0;
                ExtractEncryption.EncryptStream(unencryptedStream, encryptedStream, _CONVERGENCE_MATRIX, ml);
                _encryptedInputConfig = encryptedStream.ToArray();
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
        }

        [OnSerialized]
        private void OnSerialized(StreamingContext context)
        {
            // Reset the version that may have been decremented for serialization
            _version = _CURRENT_VERSION;
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
            _encoder = null;
            _trainingLog = null;
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
            _encryptedInputConfig = null;

#pragma warning disable CS0618 // Type or member is obsolete
            // Decrypt data encoder
            if (_version < 6)
            {
                using (var encryptedStream = new MemoryStream(_encryptedEncoder))
                using (var unencryptedStream = new MemoryStream())
                {
                    ExtractEncryption.DecryptStream(encryptedStream, unencryptedStream, _CONVERGENCE_MATRIX, ml);
                    unencryptedStream.Position = 0;
                    var serializer = new NetDataContractSerializer();
                    serializer.AssemblyFormat = FormatterAssemblyStyle.Simple;
                    Encoder = (LearningMachineDataEncoder)serializer.Deserialize(unencryptedStream);
                }
                _encryptedEncoder = null;

                // Decrypt training log
                if (_encryptedTrainingLog != null)
                {
                    TrainingLog = ExtractEncryption.DecryptString(_encryptedTrainingLog, ml);
                }
            }
            _encryptedEncoder = null;
            _encryptedTrainingLog = null;
#pragma warning restore CS0618 // Type or member is obsolete

            // Update property from persisted value
            LabelAttributesSettings = _labelAttributesPersistedSettings;

            // Remove ambiguity with text file/csv file input
#pragma warning disable CS0618 // Type or member is obsolete
            if (InputConfig.InputPathType == InputType.TextFileOrCsv)
#pragma warning restore CS0618 // Type or member is obsolete
            {
                if (Usage == LearningMachineUsage.AttributeCategorization
                    || Usage == LearningMachineUsage.Deletion
                    || Usage == LearningMachineUsage.Pagination)
                {
                    InputConfig.InputPathType = InputType.TextFile;
                }
                else
                {
                    InputConfig.InputPathType = InputType.Csv;
                }
            }

            // Update version number
            _version = _CURRENT_VERSION;
        }

        /// <summary> Creates a collection of document <see cref="ComAttribute"/>s that represents pagination boundaries
        /// (Document/Pages|start-end).
        /// </summary>
        /// <param name="sourceDocName">Name of the source document</param>
        /// <param name="numberOfPages">The number of pages in the source document</param>
        /// <param name="isFirstPage">Function used to determine whether a source page number is
        /// the first page of a paginated document</param>
        /// <param name="pageScore">Function to lookup the confidence score for a source page number</param>
        /// <param name="sourceDocument">Optional source document <see cref="SpatialString"/></param>
        /// <param name="inputPageAttributes">Optional collection of one attribute per page to be
        /// added as subattributes to the appropriate Document attribute in the output</param>
        /// <returns>A collection of Document/Page attributes</returns>
        private static IEnumerable<ComAttribute> CreatePaginationAttributes(
            string sourceDocName,
            int numberOfPages,
            Func<int, bool> isFirstPage,
            Func<int, double> pageScore,
            ISpatialString sourceDocument = null,
            IList<ComAttribute> inputPageAttributes = null)
        {
            try
            {
                ExtractException.Assert("ELI40157", "Incorrect number of inputPageAttributes",
                    inputPageAttributes == null || inputPageAttributes.Count == numberOfPages);

                var resultingAttributes = new List<ComAttribute>();
                int firstPageInRange = 1;
                double docScore = 1;
                for (int nextPageNumber = 2; nextPageNumber <= numberOfPages + 1; nextPageNumber++)
                {
                    docScore *= pageScore(nextPageNumber);

                    if (nextPageNumber > numberOfPages || isFirstPage(nextPageNumber))
                    {
                        int lastPageInRange = nextPageNumber - 1;
                        string range = Enumerable
                            .Range(firstPageInRange, lastPageInRange - firstPageInRange + 1)
                            .ToRangeString();
                        SpatialString ss = null;
                        if (sourceDocument != null)
                        {
                            // If this logical document is the entire page range then
                            // simply clone the input
                            if (firstPageInRange == 1 && lastPageInRange == numberOfPages)
                            {
                                ss = (SpatialString)((ICopyableObject)sourceDocument).Clone();
                            }
                            // Else if it is spatial, get OCRed text for the page range for the Document value
                            else if (sourceDocument.HasSpatialInfo())
                            {
                                ss = sourceDocument.GetSpecifiedPages(firstPageInRange, lastPageInRange);
                            }
                        }

                        if (ss == null)
                        {
                            ss = new SpatialStringClass();
                        }

                        // Prevent empty value that could result in the attribute getting thrown away
                        if (string.IsNullOrEmpty(ss.String))
                        {
                            ss.CreateNonSpatialString(_DOCUMENT_PLACEHOLDER_TEXT, sourceDocName);
                        }

                        var documentAttribute = new ComAttribute { Name = "Document", Value = ss };
                        resultingAttributes.Add(documentAttribute);

                        // Add a Pages attribute to denote the range of pages in this document
                        ss = new SpatialStringClass();
                        ss.CreateNonSpatialString(range, sourceDocName);
                        documentAttribute.SubAttributes.PushBack(new ComAttribute { Name = "Pages", Value = ss });

                        // Add a PaginationConfidence attribute
                        ss = new SpatialStringClass();
                        ss.CreateNonSpatialString(string.Format(CultureInfo.InvariantCulture, "{0:N4}", docScore), sourceDocName);
                        documentAttribute.SubAttributes.PushBack(new ComAttribute { Name = SpecialAttributeNames.PaginationConfidence, Value = ss });

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
                        docScore = pageScore(nextPageNumber);
                    }
                }

                return resultingAttributes;
            }
            catch (Exception e)
            {
                throw e.AsExtract("ELI40158");
            }
        }

        /// <summary> Creates a DeletedPages <see cref="ComAttribute"/> that represents the pages predicted to be deleted
        /// (DeletedPages|1-2,5).
        /// </summary>
        /// <param name="sourceDocName">Name of the source document</param>
        /// <param name="numberOfPages">The number of pages in the source document</param>
        /// <param name="isDeletedPage">Function used to determine whether a source page number is
        /// a deleted page of a document</param>
        private static ComAttribute CreateDeletedPagesAttribute(
            string sourceDocName,
            int numberOfPages,
            Func<int, bool> isDeletedPage)
        {
            try
            {
                int firstPageInRange = Enumerable.Range(1, numberOfPages)
                    .FirstOrDefault(isDeletedPage);

                if (firstPageInRange == 0)
                {
                    return null;
                }

                string value = null;
                for (int nextPageNumber = firstPageInRange + 1; nextPageNumber <= numberOfPages + 1; nextPageNumber++)
                {
                    if (nextPageNumber > numberOfPages || !isDeletedPage(nextPageNumber))
                    {
                        int lastPageInRange = nextPageNumber - 1;
                        string range = Enumerable
                            .Range(firstPageInRange, lastPageInRange - firstPageInRange + 1)
                            .ToRangeString();

                        if (value == null)
                        {
                            value = range;
                        }
                        else
                        {
                            value += ("," + range);
                        }

                        // Next page is not deleted so find next deleted page to set up next page range
                        if (nextPageNumber <= numberOfPages)
                        {
                            firstPageInRange = nextPageNumber =
                                Enumerable.Range(nextPageNumber, numberOfPages - nextPageNumber)
                                    .FirstOrDefault(isDeletedPage);
                            if (firstPageInRange == 0)
                            {
                                break;
                            }
                        }
                    }
                }

                // Add a DeletedPages attribute
                SpatialString ss = new SpatialStringClass();
                ss.CreateNonSpatialString(value, sourceDocName);

                return new ComAttribute { Name = "DeletedPages", Value = ss };
            }
            catch (Exception e)
            {
                throw e.AsExtract("ELI45787");
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
        void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (Classifier is IDisposable disposable)
                {
                    disposable?.Dispose();
                    Classifier = null;
                }
            }

            // Dispose of unmanaged resources
        }

        #endregion IDisposable Members

        #region Static Methods

        /// <summary>
        /// Computes the accuracy or F1 score of the classifier
        /// </summary>
        /// <param name="classifier">The <see cref="IClassifierModel"/> to use to compute answers</param>
        /// <param name="inputs">The feature vectors</param>
        /// <param name="outputs">The expected results</param>
        /// <param name="standardizeInputs">Whether to apply zero-center and normalize the input</param>
        /// <returns>The F1 score if there are two classes else the overall agreement</returns>
        public static double GetAccuracyScore(IClassifierModel classifier, double[][] inputs, int[] outputs,
            bool standardizeInputs = true)
        {
            try
            {
                var result = LearningMachineMethods.GetAccuracyScore(classifier, inputs, outputs, standardizeInputs);
                return result.Match(
                    gc => gc.OverallAgreement,
                    cm => Double.IsNaN(cm.FScore) ? 0.0 : cm.FScore
                );
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
        /// <param name="document">The <see cref="ISpatialString"/> used for encoding auto-BoW features</param>
        /// <param name="attributeVector">The VOA used for encoding attribute features</param>
        /// <param name="preserveInputAttributes">Whether to preserve the input <see cref="ComAttribute"/>s or not.</param>
        /// <returns>A VOA representation of the computed answer</returns>
        public static void ComputeAnswer(string learningMachinePath, ISpatialString document,
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
