﻿using Accord.Math;
using Accord.Statistics.Analysis;
using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Threading;
using UCLID_COMUTILSLib;
using UCLID_RASTERANDOCRMGMTLib;
using ComAttribute = UCLID_AFCORELib.Attribute;
using AccuracyData = Extract.Utilities.Union<Accord.Statistics.Analysis.GeneralConfusionMatrix, Accord.Statistics.Analysis.ConfusionMatrix>;
using Extract.Licensing;
using Extract.Encryption;
using System.CodeDom.Compiler;

namespace Extract.AttributeFinder
{
    /// <summary>
    /// Class to hold a trained machine, data encoder and training stats
    /// </summary>
    [CLSCompliant(false)]
    [Serializable]
    public class LearningMachine : IDisposable
    {
        #region Constants

        /// <summary>
        /// Current version.
        /// </summary>
        const int _CURRENT_VERSION = 1;

        // Encryption password for serialization
        private static readonly byte[] _ENCRYPTION_PASSWORD = new byte[64]
            {
              185, 105, 109, 83, 148, 254, 79, 173, 128, 172, 12, 76, 61, 131, 66, 69, 236, 2, 76, 172, 158, 197, 70, 243, 131,
              95, 163, 206, 89, 164, 145, 134, 6, 25, 175, 201, 97, 177, 190, 24, 163, 144, 141, 55, 75, 250, 20, 9, 176, 172,
              0x37, 0x6b, 0xac, 0xe7, 0x45, 0x97, 0x22, 0x07, 0xe8, 0x1a, 0x70, 0x3f, 0xca, 0x21
            };

        #endregion Constants

        #region Fields

        /// <summary>
        /// Persist the current version so to prevent newer, incompatible, versions from being loaded
        /// </summary>
        private int _version = _CURRENT_VERSION;

        /// <summary>
        /// Persist the <see cref="LearningMachineDataEncoder"/> encrypted to hide potentially sensitive information
        /// </summary>
        private Byte[] _encryptedEncoder;

        // Backing fields for properties
        private InputConfiguration _inputConfig;
        [NonSerialized]
        private LearningMachineDataEncoder _encoder;
        private ITrainableClassifier _classifier;
        private int _randomNumberSeed;
        private bool _useUnknownCategory;
        private double _unknownCategoryCutoff;
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
        public Tuple<AccuracyData, AccuracyData> TrainMachine()
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
        public Tuple<AccuracyData, AccuracyData> TrainMachine(Action<StatusArgs> updateStatus, CancellationToken cancellationToken)
        {
            try
            {
                return TrainAndTestMachine(false, updateStatus, cancellationToken);
            }
            catch (Exception e)
            {
                throw e.AsExtract("ELI39755");
            }
        }

        /// <summary>
        /// Tests the machine using files specified with <see cref="InputConfig"/>
        /// </summary>
        /// <returns>Tuple of training set accuracy score and testing set accuracy score</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public Tuple<AccuracyData, AccuracyData> TestMachine()
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
        public Tuple<AccuracyData, AccuracyData> TestMachine(Action<StatusArgs> updateStatus, CancellationToken cancellationToken)
        {
            try
            {
                return TrainAndTestMachine(true, updateStatus, cancellationToken);
            }
            catch (Exception e)
            {
                throw e.AsExtract("ELI39807");
            }
        }

        /// <summary>
        /// Computes an answer for the input data
        /// </summary>
        /// <remarks>If <see paramref="preserveInputAttributes"/>=<see langword="true"/> and
        /// <see cref="Usage"/>=<see cref="LearningMachineUsage.Pagination"/> then the input Page <see cref="ComAttribute"/>s will be
        /// returned as subattributes of the resulting Document <see cref="ComAttribute"/>s.</remarks>
        /// <param name="document">The <see cref="SpatialString"/> used for encoding auto-BoW features</param>
        /// <param name="protoFeaturesOrPagesOfProtoFeatures">The VOA used for encoding attribute features</param>
        /// <param name="preserveInputAttributes">Whether to preserve the input <see cref="ComAttribute"/>s or not.</param>
        /// <returns>A VOA representation of the computed answer</returns>
        public IUnknownVector ComputeAnswer(SpatialString document, IUnknownVector protoFeaturesOrPagesOfProtoFeatures, bool preserveInputAttributes)
        {
            try
            {
                ExtractException.Assert("ELI39762", "Machine has not been trained", IsTrained);

                if (preserveInputAttributes && protoFeaturesOrPagesOfProtoFeatures == null)
                {
                    protoFeaturesOrPagesOfProtoFeatures = new IUnknownVectorClass();
                }

                IEnumerable<double[]> inputs = Encoder.GetFeatureVectors(document, protoFeaturesOrPagesOfProtoFeatures);
                IEnumerable<Tuple<int, double?>> outputs = inputs.Select(Classifier.ComputeAnswer);
                if (Usage == LearningMachineUsage.DocumentCategorization)
                {
                    IEnumerable<ComAttribute> categories = outputs.Select(answerAndScore =>
                        {
                            string category;
                            if (UseUnknownCategory && answerAndScore.Item2 != null && answerAndScore.Item2 < UnknownCategoryCutoff)
                            {
                                category = "Unknown";
                            }
                            else
                            {
                                category = Encoder.AnswerCodeToName[answerAndScore.Item1];
                            }
                            var ss = new SpatialStringClass();
                            ss.CreateNonSpatialString(category, document.SourceDocName);
                            return new ComAttribute { Name = "DocumentType", Value = ss };
                        });
                    if (preserveInputAttributes)
                    {
                        return protoFeaturesOrPagesOfProtoFeatures.ToIEnumerable<ComAttribute>()
                            .Concat(categories).ToIUnknownVector();
                    }
                    else
                    {
                        return categories.ToIUnknownVector();
                    }
                }
                else if (Usage == LearningMachineUsage.Pagination)
                {
                    var inputPageAttributes = new List<ComAttribute>();
                    var resultingAttributes = new List<ComAttribute>();
                    if (preserveInputAttributes)
                    {
                        foreach (var attribute in protoFeaturesOrPagesOfProtoFeatures.ToIEnumerable<ComAttribute>())
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

                    List<bool> isFirstPage = outputs.Select(answerAndScore =>
                        answerAndScore.Item1 == LearningMachineDataEncoder.FirstPageCategoryCode)
                        .ToList();
                    int numberOfPages = isFirstPage.Count + 1;
                    int firstPageInRange = 1;

                    // Calculate where each document ends by checking to see if the next page number is a predicted
                    // first page or is greater than the number of pages in the image.
                    for (int nextPageNumber = 2; nextPageNumber <= numberOfPages + 1; nextPageNumber++)
                    {
                        // isFirstPage is zero-indexed and does not include the first page of the image
                        if (nextPageNumber > numberOfPages || isFirstPage[nextPageNumber - 2])
                        {
                            int lastPageInRange = nextPageNumber - 1;

                            // Get OCRed text for the page range for the Document value
                            var ss = document.GetSpecifiedPages(firstPageInRange, lastPageInRange);
                            // Prevent empty value that could result in the attribute getting thrown away
                            if (string.IsNullOrEmpty(ss.String))
                            {
                                ss.CreateNonSpatialString(" ", ss.SourceDocName);
                            }
                            var documentAttribute = new ComAttribute { Name = "Document", Value = ss };

                            // Add a Pages attribute to denote the range of pages in this document
                            ss = new SpatialStringClass();
                            ss.CreateNonSpatialString(string.Format(CultureInfo.CurrentCulture, "{0}-{1}", firstPageInRange, lastPageInRange),
                                document.SourceDocName);
                            documentAttribute.SubAttributes.PushBack(new ComAttribute { Name = "Pages", Value = ss });
                            resultingAttributes.Add(documentAttribute);

                            // Add input page attributes that are in this range
                            if (preserveInputAttributes)
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

                    return resultingAttributes.ToIUnknownVector();
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
                int[] predictions = inputs.Apply(Classifier.ComputeAnswer)
                    .Select(t =>
                        {
                            if (UseUnknownCategory && t.Item2 != null
                                && t.Item2 < UnknownCategoryCutoff)
                            {
                                return LearningMachineDataEncoder.UnknownCategoryCode;
                            }
                            else
                            {
                                return t.Item1;
                            }
                        })

                    .ToArray();

                AccuracyData accuracyData;
                if (Usage == LearningMachineUsage.Pagination)
                {
                    var confusionMatrix = new ConfusionMatrix(predictions, outputs);
                    accuracyData = new AccuracyData(confusionMatrix);
                }
                else
                {
                    var confusionMatrix = new GeneralConfusionMatrix(Classifier.NumberOfClasses, predictions, outputs);
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
                using (var stream = new FileStream(tempFile, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    Save(stream);
                }

                // Once the save process is complete, copy the file into the real destination.
                File.Copy(tempFile, fileName, overwrite: true);
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
                using (var stream = new FileStream(fileName, FileMode.Open))
                {
                    return Load(stream);
                }
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
                throw e.AsExtract("ELI39810");
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
                return LearningMachine.Load(savedMachine);
            }
            catch (Exception e)
            {
                throw e.AsExtract("ELI39872");
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
        private Tuple<AccuracyData, AccuracyData> TrainAndTestMachine(bool testOnly, Action<StatusArgs> updateStatus,
            CancellationToken cancellationToken)
        {
            ExtractException.Assert("ELI39840", "Machine is not fully configured", IsConfigured);

            // Compute input files and answers
            string[] ussFiles, voaFiles, answersOrAnswerFiles;
            InputConfig.GetInputData(out ussFiles, out voaFiles, out answersOrAnswerFiles, updateStatus, cancellationToken);

            if (!Encoder.AreEncodingsComputed)
            {
                Encoder.ComputeEncodings(ussFiles, voaFiles, answersOrAnswerFiles, updateStatus, cancellationToken);
            }

            var featureVectorsAndAnswers = Encoder.GetFeatureVectorAndAnswerCollections(ussFiles, voaFiles, answersOrAnswerFiles,
                updateStatus, cancellationToken);

            // Divide data into training and testing subsets
            if (InputConfig.TrainingSetPercentage > 0)
            {
                var rng = new Random(RandomNumberSeed);
                List<int> trainIdx, testIdx;
                GetIndexesOfSubsetsByCategory(featureVectorsAndAnswers.Item2,
                    InputConfig.TrainingSetPercentage / 100.0, out trainIdx, out testIdx, rng);

                // Training set
                double[][] trainInputs = featureVectorsAndAnswers.Item1.Submatrix(trainIdx);
                int[] trainOutputs = featureVectorsAndAnswers.Item2.Submatrix(trainIdx);

                // Testing set
                double[][] testInputs = featureVectorsAndAnswers.Item1.Submatrix(testIdx);
                int[] testOutputs = featureVectorsAndAnswers.Item2.Submatrix(testIdx);

                // Train the classifier
                if (!testOnly)
                {
                    Classifier.TrainClassifier(trainInputs, trainOutputs, rng, updateStatus, cancellationToken);
                }

                cancellationToken.ThrowIfCancellationRequested();

                return Tuple.Create(GetAccuracyScore(trainInputs, trainOutputs), GetAccuracyScore(testInputs, testOutputs));
            }
            // If no training data, just test testing set
            else
            {
                return Tuple.Create<AccuracyData, AccuracyData>
                    (null, GetAccuracyScore(featureVectorsAndAnswers.Item1, featureVectorsAndAnswers.Item2));
            }
        }

        /// <summary>
        /// Called when serializing
        /// </summary>
        /// <param name="context">The context.</param>
        [OnSerializing]
        private void OnSerializing(StreamingContext context)
        {
            var ml = new MapLabel();
            using (var unencryptedStream = new MemoryStream())
            using (var encryptedStream = new MemoryStream())
            {
                var serializer = new NetDataContractSerializer();
                serializer.AssemblyFormat = FormatterAssemblyStyle.Simple;
                serializer.Serialize(unencryptedStream, Encoder);
                unencryptedStream.Position = 0;
                ExtractEncryption.EncryptStream(unencryptedStream, encryptedStream, _ENCRYPTION_PASSWORD, ml);
                _encryptedEncoder = encryptedStream.ToArray();
            }
        }

        /// <summary>
        /// Called when deserialized
        /// </summary>
        /// <param name="context">The context.</param>
        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            // Don't support loading newer versions
            ExtractException.Assert("ELI40071", "Cannot load newer version", _version <= _CURRENT_VERSION,
                "Current version", _CURRENT_VERSION, "Version to load", _version);

            // Update version number
            _version = _CURRENT_VERSION;

            // Set 
            var ml = new MapLabel();
            using (var encryptedStream = new MemoryStream(_encryptedEncoder))
            using (var unencryptedStream = new MemoryStream())
            {
                ExtractEncryption.DecryptStream(encryptedStream, unencryptedStream, _ENCRYPTION_PASSWORD, ml);
                unencryptedStream.Position = 0;
                var serializer = new NetDataContractSerializer();
                serializer.AssemblyFormat = FormatterAssemblyStyle.Simple;
                Encoder = (LearningMachineDataEncoder)serializer.Deserialize(unencryptedStream);
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
                var disposable = Classifier as IDisposable;
                if (disposable != null)
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

        #endregion Static Methods
    }
}