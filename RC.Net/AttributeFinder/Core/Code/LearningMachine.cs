using Accord.Math;
using Accord.Statistics.Analysis;
using Extract.Utilities;
using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UCLID_COMUTILSLib;
using UCLID_RASTERANDOCRMGMTLib;
using System.Globalization;
using ComAttribute = UCLID_AFCORELib.Attribute;

namespace Extract.AttributeFinder
{
    /// <summary>
    /// Class to hold a trained machine, data encoder and training stats
    /// </summary>
    [CLSCompliant(false)]
    public class LearningMachine : IDisposable
    {
        #region Properties

        /// <summary>
        /// Gets/sets the input configuration
        /// </summary>
        public InputConfiguration InputConfig
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/sets the encoder to be used to create feature vectors
        /// </summary>
        public LearningMachineDataEncoder Encoder
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/sets the classifier to be trained and used to compute answers with encoded feature vectors
        /// </summary>
        public ITrainableClassifier Classifier
        {
            get;
            set;
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
            get;
            set;
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
            get;
            set;
        }

        /// <summary>
        /// Gets/sets the threshold level below which probability will be considered to signify Unknown
        /// </summary>
        public double UnknownCategoryCutoff
        {
            get;
            set;
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

        #endregion Properties

        #region Public Methods

        /// <summary>
        /// Computes encodings without training
        /// </summary>
        public void ComputeEncodings()
        {
            try
            {
                ExtractException.Assert("ELI39803", "Machine is not fully configured", IsConfigured);

                // Compute input files and answers
                string[] ussFiles, voaFiles, answersOrAnswerFiles;
                InputConfig.GetInputData(out ussFiles, out voaFiles, out answersOrAnswerFiles);

                Encoder.ComputeEncodings(ussFiles, voaFiles, answersOrAnswerFiles);
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
        public Tuple<double, double> TrainMachine()
        {
            try
            {
                ExtractException.Assert("ELI39840", "Machine is not fully configured", IsConfigured);

                // Compute input files and answers
                string[] ussFiles, voaFiles, answersOrAnswerFiles;
                InputConfig.GetInputData(out ussFiles, out voaFiles, out answersOrAnswerFiles);

                if (!Encoder.AreEncodingsComputed)
                {
                    Encoder.ComputeEncodings(ussFiles, voaFiles, answersOrAnswerFiles);
                }

                var featureVectorsAndAnswers = Encoder.GetFeatureVectorAndAnswerCollections(ussFiles, voaFiles, answersOrAnswerFiles);

                // Divide data into training and testing subsets
                var rng = new Random(RandomNumberSeed);
                List<int> trainIdx, testIdx;
                GetIndexesOfSubsetsByCategory(featureVectorsAndAnswers.Item2, InputConfig.TrainingSetPercentage / 100.0, out trainIdx, out testIdx, rng);

                // Training set
                double[][] trainInputs = featureVectorsAndAnswers.Item1.Submatrix(trainIdx);
                int[] trainOutputs = featureVectorsAndAnswers.Item2.Submatrix(trainIdx);

                // Testing set
                double[][] testInputs = featureVectorsAndAnswers.Item1.Submatrix(testIdx);
                int[] testOutputs = featureVectorsAndAnswers.Item2.Submatrix(testIdx);

                // Train the classifier
                Classifier.TrainClassifier(trainInputs, trainOutputs, rng);

                return Tuple.Create(GetAccuracyScore(Classifier, trainInputs, trainOutputs), GetAccuracyScore(Classifier, testInputs, testOutputs));
            }
            catch (Exception e)
            {
                throw e.AsExtract("ELI39755");
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
                                for (int i = firstPageInRange - 1; i < lastPageInRange; i++)
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

        #endregion Public Methods

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