using Extract.Encryption;
using Extract.Licensing;
using Extract.Utilities;
using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Core;
using Lucene.Net.Analysis.En;
using Lucene.Net.Analysis.Miscellaneous;
using Lucene.Net.Analysis.Shingle;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Analysis.TokenAttributes;
using Lucene.Net.Documents;
using Lucene.Net.Facet;
using Lucene.Net.Facet.Taxonomy;
using Lucene.Net.Facet.Taxonomy.Directory;
using Lucene.Net.Index;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LearningMachineTrainer;
using UCLID_RASTERANDOCRMGMTLib;

namespace Extract.AttributeFinder
{
    /// <summary>
    /// An IFeatureVectorizer that produces feature vectors from <see cref="ISpatialString"/>s
    /// </summary>
    [CLSCompliant(false)]
    [Serializable]
    // Don't rename because it could break serialization
    [Obfuscation(Feature = "renaming", Exclude = true)]
    public class SpatialStringFeatureVectorizer : IFeatureVectorizer
    {
        #region Constants

#pragma warning disable CS0618 // Type or member is obsolete
        static readonly LuceneVersion _LUCENE_VERSION = LuceneVersion.LUCENE_30;
#pragma warning restore CS0618 // Type or member is obsolete

        /// <summary>
        /// Version 2: Add versioning
        /// Version 3: Add _useFeatureHashing
        /// Version 4: Changed to store some fields encrypted so that the whole encoder doesn't need to be encrypted anymore
        /// </summary>
        const int _CURRENT_VERSION = 4;

        // Encryption password for serialization, renamed to obfuscate purpose
        private static readonly byte[] _CONVERGENCE_MATRIX = new byte[64]
            {
                185, 105, 109, 83, 148, 254, 79, 173, 128, 172, 12, 76, 61, 131, 66, 69, 236, 2, 76, 172, 158,
                197, 70, 243, 131, 95, 163, 206, 89, 164, 145, 134, 6, 25, 175, 201, 97, 177, 190, 24, 163, 144,
                141, 55, 75, 250, 20, 9, 176, 172, 55, 107, 172, 231, 69, 151, 34, 7, 232, 26, 112, 63, 202, 33
            };

        #endregion Constants

        #region Fields

        /// <summary>
        /// Bag-of-words object used to create this object's feature vector
        /// </summary>
        [Obsolete("Use _nonSerializedBagOfWords")]
        private Accord.MachineLearning.BagOfWords _bagOfWords;

        [NonSerialized]
        private Accord.MachineLearning.BagOfWords _nonSerializedBagOfWords;

        // Backing fields for properties
        private string _pagesToProcess;
        private string _name;
        private bool _enabled;
        private int _shingleSize;
        private int _maxFeatures;

        [OptionalField(VersionAdded = 2)]
        private int _version = _CURRENT_VERSION;

        [OptionalField(VersionAdded = 3)]
        private bool _useFeatureHashing;

        [OptionalField(VersionAdded = 4)]
        private byte[] _encryptedVocabulary;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Creates an instance of <see cref="SpatialStringFeatureVectorizer"/>
        /// </summary>
        /// <param name="pagesToProcess">The string representation of which pages to process
        /// to compute the feature vector. If empty or null then all pages will be used.
        /// If computing pagination feature vectors then no pages should be specified.</param>
        /// <param name="shingleSize">The maximum size of shingles (word-n-grams) to be considered for terms.
        /// If less than 2 then only single word terms will be used.</param>
        /// <param name="maxFeatures">The maximum size feature vector to produce</param>
        public SpatialStringFeatureVectorizer(string pagesToProcess, int shingleSize, int maxFeatures)
        {
            try
            {
                Enabled = true;
                Name = "Auto Bag of Words";
                PagesToProcess = pagesToProcess;
                ShingleSize = shingleSize;
                MaxFeatures = maxFeatures;
            }
            catch (Exception e)
            {
                throw e.AsExtract("ELI39533");
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// The name of the feature vectorizer. Used for display purposes only
        /// </summary>
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = string.IsNullOrWhiteSpace(value) ? null : value;
                NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// Gets the <see cref="FeatureVectorizerType"/> of this feature vectorizer
        /// (always <see cref="FeatureVectorizerType.DiscreteTerms"/>)
        /// Calling Set will throw an exception.
        /// </summary>
        public FeatureVectorizerType FeatureType
        {
            get
            {
                return FeatureVectorizerType.DiscreteTerms;
            }
            set
            {
                throw new ExtractException("ELI39528", "Cannot change type of SpatialStringFeatureVectorizer");
            }
        }

        /// <summary>
        /// Whether changing the <see cref="FeatureVectorizerType"/> is allowed. Always <see langword="false"/>.
        /// </summary>
        public bool IsFeatureTypeChangeable
        {
            get
            {
                return false;
            }
        }

        /// <summary>
        /// Whether this feature vectorizer will produce a feature vector of greater than
        /// zero length (if <c>true</c>) or of zero length (if <c>false</c>)
        /// </summary>
        public bool Enabled
        {
            get
            {
                return _enabled;
            }
            set
            {
                if (value != _enabled)
                {
                    _enabled = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets/sets the string representation of which pages to process.
        /// If computing pagination feature vectors then no pages should be specified.
        /// </summary>
        public string PagesToProcess
        {
            get
            {
                return _pagesToProcess;
            }
            set
            {
                try
                {
                    string newValue = string.IsNullOrWhiteSpace(value) ? null : value;
                    if (newValue != _pagesToProcess)
                    {
                        if (!String.IsNullOrWhiteSpace(newValue))
                        {
                            UtilityMethods.ValidatePageNumbers(newValue);
                        }

                        _pagesToProcess = newValue;
                        NotifyPropertyChanged();
                    }
                }
                catch (Exception e)
                {
                    throw e.AsExtract("ELI39616");
                }
            }
        }

        /// <summary>
        /// Gets/sets the maximum size of shingles (word-n-grams) to be considered for terms.
        /// If less than 2 then only single word terms will be used.
        /// </summary>
        public int ShingleSize
        {
            get
            {
                return _shingleSize;
            }
            set
            {
                if (value != _shingleSize)
                {
                    _shingleSize = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets/sets the maximum size feature vector to produce
        /// </summary>
        public int MaxFeatures
        {
            get
            {
                return _maxFeatures;
            }
            set
            {
                if (value != _maxFeatures)
                {
                    _maxFeatures = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// A collection of distinct attribute values seen during configuration
        /// </summary>
        public IEnumerable<string> RecognizedValues
        {
            get
            {
                // Order by code to match original order of terms passed to constructor
                // This could be interesting to see (since terms will have been ordered by TFIDF score)
                // and simplifies reconstructing the bag of words when cloning
                return _nonSerializedBagOfWords == null
                    ? Enumerable.Empty<string>()
                    : _nonSerializedBagOfWords.CodeToString.OrderBy(p => p.Key).Select(p => p.Value);
            }
        }

        /// <summary>
        /// The length of the feature vector that this vectorizer will produce.
        /// </summary>
        /// <remarks>If pagination usage then the true length might differ from this value</remarks>
        public int FeatureVectorLength
        {
            get
            {
                if (UseFeatureHashing)
                {
                    return MaxFeatures;
                }
                else
                {
                    return RecognizedValues.Count();
                }
            }
        }

        /// <summary>
        /// The length of the feature vector that this vectorizer will produce when
        /// used for pagination.
        /// </summary>
        public int FeatureVectorLengthForPagination
        {
            get
            {
                var pagesPer = PagesPerPaginationCandidate;
                var baseLength = FeatureVectorLength;
                if (pagesPer <= 2)
                {
                    return baseLength * pagesPer;
                }
                else
                {
                    return baseLength * pagesPer + pagesPer - 2;
                }
            }
        }

        /// <summary>
        /// The number of pages per candidate
        /// </summary>
        /// <remarks>This is the integer value of PagesToProcess or 1 if PagesToProcess is empty or unparseable</remarks>
        public int PagesPerPaginationCandidate
        {
            get
            {
                var range = 1;
                if (!string.IsNullOrWhiteSpace(PagesToProcess)
                    && int.TryParse(PagesToProcess, out int parsed)
                    && parsed > 0)
                {
                    range = parsed;
                }

                return range;
            }
        }

        /// <summary>
        /// Whether this instance is ready to generate feature vectors
        /// </summary>
        public bool AreEncodingsComputed
        {
            get
            {
                return _nonSerializedBagOfWords != null || _useFeatureHashing;
            }
        }

        /// <summary>
        /// Whether to use feature hashing instead of a fixed vocabulary
        /// </summary>
        public bool UseFeatureHashing
        {
            get
            {
                return _useFeatureHashing;
            }
            set
            {
                if (_useFeatureHashing != value)
                {
                    _useFeatureHashing = value;
                    NotifyPropertyChanged();
                }
            }
        }

        #endregion Properties

        #region Public Methods

        /// <summary>
        /// Creates a deep clone of this instance
        /// </summary>
        /// <returns>A clone of this instance</returns>
        public SpatialStringFeatureVectorizer DeepClone()
        {
            try
            {
                var clone = (SpatialStringFeatureVectorizer)MemberwiseClone();

                var valuesSeen = RecognizedValues.ToArray();
                if (valuesSeen.Length > 0)
                {
                    clone._nonSerializedBagOfWords = new Accord.MachineLearning.BagOfWords(valuesSeen);
                }
                return clone;
            }
            catch (Exception e)
            {
                throw e.AsExtract("ELI39804");
            }
        }

        /// <summary>
        /// Limits bag of words to the top <see paramref="limit"/>terms.
        /// </summary>
        /// <param name="limit">The number of terms to limit to.</param>
        public void LimitToTopTerms(int limit)
        {
            try
            {
                var topTerms = RecognizedValues.Take(limit).ToArray();
                _nonSerializedBagOfWords = new Accord.MachineLearning.BagOfWords(topTerms);

                NotifyPropertyChanged("DistinctValuesSeen");
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI41836");
            }
        }

        #endregion Public Methods

        #region Internal Methods

        /// <summary>
        /// Configures this instance for document categorization by examining all <see paramref="ussFiles"/>.
        /// After this method has been run, this object will be ready to produce feature vectors.
        /// </summary>
        /// <param name="ussFiles">Paths to the spatial string files from which to collect terms for the bag-of-words
        /// vocabulary.</param>
        /// <param name="answers">The categories that each uss file belongs to.</param>
        /// <param name="updateStatus">Function to use for sending progress updates to caller</param>
        /// <param name="cancellationToken">Token indicating that processing should be canceled</param>
        internal void ComputeEncodingsFromDocumentTrainingData(IEnumerable<string> ussFiles, IEnumerable<string> answers,
            Action<StatusArgs> updateStatus, CancellationToken cancellationToken)
        {
            try
            {
                var inputTexts = ussFiles.Select(GetDocumentText);
                var textsAndCategories = inputTexts.Zip(answers, (example, answer) => Tuple.Create(example, answer));

                SetBagOfWords(textsAndCategories, updateStatus, cancellationToken);
            }
            catch (Exception e)
            {
                throw e.AsExtract("ELI39536");
            }
        }

        /// <summary>
        /// Configures this instance for pagination by examining all <see paramref="ussFiles"/>.
        /// After this method has been run, this object will be ready to produce feature vectors.
        /// </summary>
        /// <param name="ussFiles">Paths to the spatial string files from which to collect terms for the bag-of-words
        /// vocabulary.</param>
        /// <param name="answerFiles">The paths to the VOA files that contain the expected pagination boundary information
        /// for each uss file.</param>
        /// <param name="updateStatus">Function to use for sending progress updates to caller</param>
        /// <param name="cancellationToken">Token indicating that processing should be canceled</param>
        internal void ComputeEncodingsFromPaginationTrainingData(IEnumerable<string> ussFiles, IEnumerable<string> answerFiles,
            Action<StatusArgs> updateStatus, CancellationToken cancellationToken)
        {
            try
            {
                var inputTextsCollection = ussFiles.Select(file => GetPaginationTexts(file));

                // Pass the page count of each image along so that missing pages in the answer VOA can be filled in
                var textsAndCategories = inputTextsCollection.Zip(answerFiles, (examples, answerFile) =>
                    {
                        var answers = LearningMachineDataEncoder.ExpandPaginationAnswerVOA(answerFile, examples.Count() + 1);
                        return examples.Zip(answers, (example, answer) => Tuple.Create(example, answer));
                    })
                    .SelectMany(answersForFile => answersForFile);

                SetBagOfWords(textsAndCategories, updateStatus, cancellationToken);
            }
            catch (Exception e)
            {
                throw e.AsExtract("ELI40371");
            }
        }

        /// <summary>
        /// Configures this instance for pagination by examining all <see paramref="ussFiles"/>.
        /// After this method has been run, this object will be ready to produce feature vectors.
        /// </summary>
        /// <param name="ussFiles">Paths to the spatial string files from which to collect terms for the bag-of-words
        /// vocabulary.</param>
        /// <param name="answerFiles">The paths to the VOA files that contain the expected pagination boundary information
        /// for each uss file.</param>
        /// <param name="updateStatus">Function to use for sending progress updates to caller</param>
        /// <param name="cancellationToken">Token indicating that processing should be canceled</param>
        internal void ComputeEncodingsFromDeletionTrainingData(IEnumerable<string> ussFiles, IEnumerable<string> answerFiles,
            Action<StatusArgs> updateStatus, CancellationToken cancellationToken)
        {
            try
            {
                var inputTextsCollection = ussFiles.Select(file => GetPaginationTexts(file, true));

                // Pass the page count of each image along so that missing pages in the answer VOA can be filled in
                var textsAndCategories = inputTextsCollection.Zip(answerFiles, (examples, answerFile) =>
                    {
                        var answers = LearningMachineDataEncoder.ExpandDeletionAnswerVOA(answerFile, examples.Count() + 1);
                        return examples.Zip(answers, (example, answer) => Tuple.Create(example, answer));
                    })
                    .SelectMany(answersForFile => answersForFile);

                SetBagOfWords(textsAndCategories, updateStatus, cancellationToken);
            }
            catch (Exception e)
            {
                throw e.AsExtract("ELI45843");
            }
        }

        /// <summary>
        /// Configures this instance for attribute categorization by examining all <see paramref="ussFiles" />.
        /// After this method has been run, this object will be ready to produce feature vectors.
        /// </summary>
        /// <param name="ussFiles">Paths to the spatial string files from which to collect terms for the bag-of-words
        /// vocabulary.</param>
        /// <param name="labeledAttributesFiles">The paths to the VOA files that contain the labeled candidate
        /// <see cref="ComAttribute"/>s (the <see cref="ComAttribute.Type"/> is the label).
        /// <param name="updateStatus">Function to use for sending progress updates to caller</param>
        /// <param name="cancellationToken">Token indicating that processing should be canceled</param>
        /// <returns>A collection of all the distinct category names found in the <paramref name="labeledAttributesFiles"/></returns>
        internal IEnumerable<string> ComputeEncodingsFromAttributesTrainingData(string[] ussFiles, string[] labeledAttributesFiles,
            Action<StatusArgs> updateStatus, CancellationToken cancellationToken)
        {
            try
            {
                var inputTextsCollection = ussFiles.Select(GetDocumentText);

                // Associate each category with the text of each document it appears with
                var textsAndCategories = inputTextsCollection.Zip(labeledAttributesFiles, (text, attributeFile) =>
                    {
                        var answers = LearningMachineDataEncoder
                            .CollectLabelsFromLabeledCandidateAttributesFile(attributeFile)
                            .Distinct(StringComparer.OrdinalIgnoreCase);
                        return answers.Select(answer => Tuple.Create(text, answer));
                    })
                    .SelectMany(answersForFile => answersForFile)
                    .ToList();

                // Skip processing the texts if using feature hashing since there is no need
                // to build a vocabulary
                if (!UseFeatureHashing)
                {
                    SetBagOfWords(textsAndCategories, updateStatus, cancellationToken);
                }

                return textsAndCategories
                    .Select(textAndCategory => textAndCategory.Item2)
                    .Distinct(StringComparer.OrdinalIgnoreCase);
            }
            catch (Exception e)
            {
                throw e.AsExtract("ELI41407");
            }
        }

        /// <summary>
        /// Get feature vector from document text using computed <see cref="_nonSerializedBagOfWords"/>. Throws
        /// an exception if <see cref="ComputeEncodingsFromDocumentTrainingData"/> has not been called.
        /// </summary>
        /// <param name="document">The <see cref="ISpatialString"/> to be used to generate the feature vector.</param>
        /// <returns>Array of doubles of <see cref="FeatureVectorLength"/> or length of zero if not <see cref="Enabled"/></returns>
        internal double[] GetDocumentFeatureVector(ISpatialString document)
        {
            try
            {
                ExtractException.Assert("ELI39529", "This object has not been fully configured.", AreEncodingsComputed);

                return GetFeatureVector(GetDocumentText(document));
            }
            catch (Exception e)
            {
                throw e.AsExtract("ELI39530");
            }
        }
        
        double[] GetFeatureVector(string text)
        {
            if (UseFeatureHashing)
            {
                double[] featureVector = new double[MaxFeatures];
                var hasher = Murmur.MurmurHash.Create32();
                foreach(var term in GetTerms(text))
                {
                    var bytes = hasher.ComputeHash(Encoding.UTF8.GetBytes(term));
                    var number = BitConverter.ToInt32(bytes, 0);
                    var index = Math.Abs(number) % MaxFeatures;
                    var value = number > 0 ? 1 : -1;
                    featureVector[index] += value;
                }
                return featureVector;
            }
            else
            {
                string[] terms = GetTerms(text).ToArray();
                return _nonSerializedBagOfWords.GetFeatureVector(terms).Select(i => (double)i).ToArray();
            }
        }

        /// <summary>
        /// Get feature vectors from page text using computed <see cref="_nonSerializedBagOfWords"/>. Throws
        /// an exception if <see cref="ComputeEncodingsFromPaginationTrainingData"/> has not been called.
        /// </summary>
        /// <param name="document">The <see cref="ISpatialString"/> to be used to generate the feature vectors.</param>
        /// <returns>If <see cref="Enabled"/> then an enumeration of feature vectors that has a length of
        /// the number of pages in <see paramref="document"/>-1. Else an empty enumeration.</returns>
        internal IEnumerable<double[]> GetPaginationFeatureVectors(ISpatialString document)
        {
            try
            {
                ExtractException.Assert("ELI39531", "This object has not been fully configured.", AreEncodingsComputed);

                int totalPagesPer = PagesPerPaginationCandidate;
                int priorPages = totalPagesPer / 2;
                int postPages = totalPagesPer - priorPages;

                var pageFeatures = GetPaginationTexts(document, priorPages > 0)
                    .Select(text => GetFeatureVector(text))
                    .ToList();

                var firstCandidateIdx = priorPages > 0
                    ? 1
                    : 0;
                var pages = new double[pageFeatures.Count - firstCandidateIdx][];

                // Add subvectors for preceeding pages
                int featureVectorLength = FeatureVectorLengthForPagination;
                for (int candidateIdx = firstCandidateIdx, resultIdx = 0;
                    resultIdx < pages.Length;
                    candidateIdx++, resultIdx++)
                {
                    var featureVector = new double[featureVectorLength];
                    pages[resultIdx] = featureVector;
                    int featureVectorSubsetStart = 0;
                    for (int i = priorPages; i > 0; i--)
                    {
                        int pageOfInterestIdx = candidateIdx - i;
                        if (pageOfInterestIdx >= 0)
                        {
                            // Add flag for pages that aren't always there
                            if (i > 1)
                            {
                                featureVector[featureVectorSubsetStart] = 1;
                                featureVectorSubsetStart++;
                            }

                            // Copy page feature values into larger array
                            var pageOfinterest = pageFeatures[pageOfInterestIdx];
                            for (int j = 0; j < pageOfinterest.Length; j++)
                            {
                                featureVector[featureVectorSubsetStart + j] = pageOfinterest[j];
                            }
                        }
                        // Else page is missing so leave flag at 0
                        else
                        {
                            featureVectorSubsetStart++;
                        }
                        featureVectorSubsetStart += FeatureVectorLength;
                    }

                    // Add subvectors for following pages
                    for (int i = 0; i < postPages; i++)
                    {
                        int pageOfInterestIdx = candidateIdx + i;
                        if (pageOfInterestIdx < pages.Length)
                        {
                            // Add flag for pages that aren't always there
                            if (i > 0)
                            {
                                featureVector[featureVectorSubsetStart] = 1;
                                featureVectorSubsetStart++;
                            }

                            // Copy page feature values into larger array
                            var pageOfinterest = pageFeatures[pageOfInterestIdx];
                            for (int j = 0; j < pageOfinterest.Length; j++)
                            {
                                featureVector[featureVectorSubsetStart + j] = pageOfinterest[j];
                            }
                        }
                        featureVectorSubsetStart += FeatureVectorLength;
                    }
                }
                return pages;
            }
            catch (Exception e)
            {
                throw e.AsExtract("ELI39532");
            }
        }

        /// <summary>
        /// Get feature vectors from page text using computed <see cref="_nonSerializedBagOfWords"/>. Throws
        /// an exception if <see cref="ComputeEncodingsFromPaginationTrainingData"/> has not been called.
        /// </summary>
        /// <param name="document">The <see cref="ISpatialString"/> to be used to generate the feature vectors.</param>
        /// <returns>If <see cref="Enabled"/> then an enumeration of feature vectors that has a length of
        /// the number of pages in <see paramref="document"/>-1. Else an empty enumeration.</returns>
        internal IEnumerable<double[]> GetDeletionFeatureVectors(ISpatialString document)
        {
            try
            {
                ExtractException.Assert("ELI45844", "This object has not been fully configured.", AreEncodingsComputed);

                int totalPagesPer = PagesPerPaginationCandidate;
                int priorPages = totalPagesPer / 2;
                int postPages = totalPagesPer - priorPages;

                var pageFeatures = GetPaginationTexts(document, true)
                    .Select(text => GetFeatureVector(text))
                    .ToList();

                var pages = new double[pageFeatures.Count][];

                // Add subvectors for preceeding pages
                int featureVectorLength = FeatureVectorLengthForPagination;
                for (int resultIdx = 0; resultIdx < pages.Length; resultIdx++)
                {
                    var featureVector = new double[featureVectorLength];
                    pages[resultIdx] = featureVector;
                    int featureVectorSubsetStart = 0;
                    for (int i = priorPages; i > 0; i--)
                    {
                        int pageOfInterestIdx = resultIdx - i;
                        if (pageOfInterestIdx >= 0)
                        {
                            // Add flag for pages that aren't always there
                            if (i > 1)
                            {
                                featureVector[featureVectorSubsetStart] = 1;
                                featureVectorSubsetStart++;
                            }

                            // Copy page feature values into larger array
                            var pageOfinterest = pageFeatures[pageOfInterestIdx];
                            for (int j = 0; j < pageOfinterest.Length; j++)
                            {
                                featureVector[featureVectorSubsetStart + j] = pageOfinterest[j];
                            }
                        }
                        featureVectorSubsetStart += FeatureVectorLength;
                    }

                    // Add subvectors for following pages
                    for (int i = 0; i < postPages; i++)
                    {
                        int pageOfInterestIdx = resultIdx + i;
                        if (pageOfInterestIdx < pages.Length)
                        {
                            // Add flag for pages that aren't always there
                            if (i > 0)
                            {
                                featureVector[featureVectorSubsetStart] = 1;
                                featureVectorSubsetStart++;
                            }

                            // Copy page feature values into larger array
                            var pageOfinterest = pageFeatures[pageOfInterestIdx];
                            for (int j = 0; j < pageOfinterest.Length; j++)
                            {
                                featureVector[featureVectorSubsetStart + j] = pageOfinterest[j];
                            }
                        }
                        featureVectorSubsetStart += FeatureVectorLength;
                    }
                }
                return pages;
            }
            catch (Exception e)
            {
                throw e.AsExtract("ELI45845");
            }
        }

        /// <summary>
        /// Whether this instance is configured as another
        /// </summary>
        /// <param name="other">The other <see cref="SpatialStringFeatureVectorizer"/> to compare with</param>
        /// <returns><see langword="true"/> if the configurations are the same, else <see langword="false"/></returns>
        internal bool IsConfigurationEqualTo(SpatialStringFeatureVectorizer other)
        {
            if (Object.ReferenceEquals(this, other))
            {
                return true;
            }

            if (other == null
                || other.Enabled != Enabled
                || other.MaxFeatures != MaxFeatures
                || other.Name != Name
                || other.PagesToProcess != PagesToProcess
                || other.ShingleSize != ShingleSize
                || other.UseFeatureHashing != UseFeatureHashing
                )
            {
                return false;
            }

            return true;
        }

        #endregion Internal Methods

        #region Overrides

        /// <summary>
        /// Whether this instance is equal to another.
        /// </summary>
        /// <param name="obj">The instance to compare with</param>
        /// <returns><see langword="true"/> if this instance has equal property values, else <see langword="false"/></returns>
        public override bool Equals(object obj)
        {
            if (Object.ReferenceEquals(this, obj))
            {
                return true;
            }

            var other = obj as SpatialStringFeatureVectorizer;
            if (other == null
                || !IsConfigurationEqualTo(other)
                || other.FeatureVectorLength != FeatureVectorLength
                || other._nonSerializedBagOfWords != null && _nonSerializedBagOfWords != null &&
                    !other._nonSerializedBagOfWords.CodeToString.SequenceEqual(_nonSerializedBagOfWords.CodeToString)
                )
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
            var hash = HashCode.Start
                .Hash(AreEncodingsComputed)
                .Hash(Enabled)
                .Hash(MaxFeatures)
                .Hash(Name)
                .Hash(PagesToProcess)
                .Hash(ShingleSize);
            foreach (var keyValuePair in _nonSerializedBagOfWords.CodeToString)
            {
                hash = hash.Hash(keyValuePair.Value);
            }

            return hash;
        }

        #endregion Overrides

        #region INotifyPropertyChanged

        /// <summary>
        /// Property changed event
        /// </summary>
        [field:NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion INotifyPropertyChanged

        #region Private Methods

        /// <summary>
        /// This method is called by the Set accessor of each property
        /// </summary>
        /// <param name="propertyName">Optional name of property that changed</param>
        /// In VS 2015 could use this: [CallerMemberName] String propertyName = ""
        private void NotifyPropertyChanged(string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Gets the string value from a <see cref="ISpatialString"/> limited to <see cref="PagesToProcess"/>
        /// </summary>
        /// <param name="document">The <see cref="ISpatialString"/> from which to get text.</param>
        /// <returns>A substring of the <see cref="ISpatialString"/> as a plain <see langword="string"/>.</returns>
        private string GetDocumentText(ISpatialString document)
        {
            if (String.IsNullOrWhiteSpace(PagesToProcess))
            {
                return document.String;
            }
            else
            {
                var pages = document.GetPages(true, " ").ToIEnumerable<ISpatialString>().ToArray();
                var pageNumbers = UtilityMethods.GetSortedPageNumberFromString(PagesToProcess, pages.Length, false);
                return String.Join("\r\n\r\n", pageNumbers.Select(i => pages[i - 1].String).ToArray());
            }
        }

        /// <summary>
        /// Gets the string value from a <see cref="ISpatialString"/> limited to <see cref="PagesToProcess"/>
        /// </summary>
        /// <param name="ussPath">The path to the saved <see cref="ISpatialString"/> from which to get text.</param>
        /// <returns>A substring of the <see cref="ISpatialString"/> as a plain <see langword="string"/>.</returns>
        private string GetDocumentText(string ussPath)
        {
            try
            {
                var ussObject = new SpatialStringClass();
                ussObject.LoadFrom(ussPath, false);
                ussObject.ReportMemoryUsage();
                return GetDocumentText(ussObject);
            }
            catch (Exception e)
            {
                throw e.AsExtract("ELI39534");
            }
        }

        /// <summary>
        /// Gets the string values of each page in <see paramref="document"/> except for the first.
        /// </summary>
        /// <param name="document">The <see cref="ISpatialString"/> from which to get text.</param>
        /// <returns>A list of strings containing the text for each candidate first page
        /// (first page after possible pagination boundary)</returns>
        internal static List<string> GetPaginationTexts(ISpatialString document, bool includeFirstPage = false)
        {
            var pages = document.GetPages(true, " ")
                .ToIEnumerable<ISpatialString>()
                .Select(s => s.String);

            if (includeFirstPage)
            {
                return pages.ToList();
            }
            else
            {
                return pages.Skip(1).ToList();
            }
        }

        /// <summary>
        /// Gets the string values of each page in <see paramref="ussPath"/> except for the first.
        /// </summary>
        /// <param name="ussPath">The path to the USS file</param>
        /// <returns>A list of strings containing the text for each candidate first page
        /// (first page after possible pagination boundary)</returns>
        private static List<string> GetPaginationTexts(string ussPath, bool includeFirstPage = false)
        {
            try
            {
                var document = new SpatialStringClass();
                document.LoadFrom(ussPath, false);
                document.ReportMemoryUsage();
                return GetPaginationTexts(document, includeFirstPage);
            }
            catch (Exception e)
            {
                throw e.AsExtract("ELI39535");
            }
        }

        /// <summary>
        /// Creates the bag of words object from a collection of categorized strings.
        /// </summary>
        /// <param name="textsAndCategories">Collection of categorized strings</param>
        /// <param name="updateStatus">Function to use for sending progress updates to caller</param>
        /// <param name="cancellationToken">Token indicating that processing should be canceled</param>
        private void SetBagOfWords(IEnumerable<Tuple<string, string>> textsAndCategories,
            Action<StatusArgs> updateStatus, CancellationToken cancellationToken)
        {
            DirectoryInfo tempDirForMainIndex = FileSystemMethods.GetTemporaryFolder();
            DirectoryInfo tempDirForFacetIndex = FileSystemMethods.GetTemporaryFolder();
            try
            {
                using (var mainDir = FSDirectory.Open(tempDirForMainIndex))
                using (var facetDir = FSDirectory.Open(tempDirForFacetIndex))
                {
                    // Create a Lucene inverted index out of the input texts
                    WriteTermsToIndex(mainDir, facetDir, textsAndCategories, updateStatus, cancellationToken);

                    // Use the index to score the terms and pick the top scoring
                    string[] topScoringTerms = GetTopScoringTerms(mainDir, facetDir, updateStatus, cancellationToken);

                    // Create the bag-of-words object
                    _nonSerializedBagOfWords = new Accord.MachineLearning.BagOfWords(topScoringTerms);
                }
            }
            catch (Exception e)
            {
                throw e.AsExtract("ELI41409");
            }
            finally
            {
                // Delete index
                foreach (var folder in new[] { tempDirForMainIndex.FullName, tempDirForFacetIndex.FullName })
                {
                    foreach (var file in System.IO.Directory.GetFiles(folder))
                    {
                        FileSystemMethods.DeleteFile(file);
                    }
                    System.IO.Directory.Delete(folder, true);
                }
            }
        }

        /// <summary>
        /// Builds a Lucene inverted index out of the <see paramref="inputTexts"/> so that there is
        /// one document for each distinct value of <see paramref="textCategories"/>.
        /// </summary>
        /// <param name="mainDir">The <see cref="FSDirectory"/> in which to write the index files.</param>
        /// <param name="facetDir">The <see cref="FSDirectory"/> in which to write the facet index files.</param>
        /// <param name="textAndCategoryCollection">Collection of text and category pairs</param>
        /// <param name="updateStatus">Function to use for sending progress updates to caller</param>
        /// <param name="cancellationToken">Token indicating that processing should be canceled</param>
        /// <returns>The number of examples (<see paramref="inputTexts"/>) written</returns>
        private void WriteTermsToIndex(FSDirectory mainDir, FSDirectory facetDir, IEnumerable<Tuple<string, string>> textAndCategoryCollection,
            Action<StatusArgs> updateStatus, CancellationToken cancellationToken)
        {
            // I don't think it makes any difference which analyzer is used since an already-tokenized streams are passed in
            var analyzer = new SimpleAnalyzer(_LUCENE_VERSION);
            FacetsConfig config = new FacetsConfig();
            using (var writer = new IndexWriter(mainDir, new IndexWriterConfig(_LUCENE_VERSION, analyzer)))
            using (var taxoWriter = new DirectoryTaxonomyWriter(facetDir))
            {
                Parallel.ForEach(textAndCategoryCollection, (textAndCategory, loopState) =>
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        loopState.Stop();
                    }

                    var text = textAndCategory.Item1;
                    var category = textAndCategory.Item2;
                    var tokenStream = GetTokenStream(text);
                    var document = new Document
                    {
                        new FacetField("category", category),
                        new TextField("shingles", tokenStream)
                    };
                    writer.AddDocument(config.Build(taxoWriter, document));
                    updateStatus(new StatusArgs { StatusMessage = "Writing terms to index... Texts processed: {0:N0}", Int32Value = 1 });
                });

                cancellationToken.ThrowIfCancellationRequested();

                writer.Commit();
                taxoWriter.Commit();
            }
        }

        /// <summary>
        /// Computes a relevance score for each term in a Lucene inverted index in which
        /// each document is a collection of terms for a single classification category built
        /// by <see cref="WriteTermsToIndex"/>.
        /// </summary>
        /// <param name="mainDir">The <see cref="FSDirectory"/> where the index files have been written.</param>
        /// <param name="facetDir">The <see cref="FSDirectory"/> where the facet index files have been written.</param>
        /// <param name="updateStatus">Function to use for sending progress updates to caller</param>
        /// <param name="cancellationToken">Token indicating that processing should be canceled</param>
        /// <returns>The <see cref="MaxFeatures"/> top-scoring terms in the index.</returns>
        private string[] GetTopScoringTerms(FSDirectory mainDir, FSDirectory facetDir,
            Action<StatusArgs> updateStatus, CancellationToken cancellationToken)
        {
            // Collect all terms (words and shingles)
            using (var reader = DirectoryReader.Open(mainDir))
            using (var taxoReader = new DirectoryTaxonomyReader(facetDir))
            {
                FacetsConfig config = new FacetsConfig();
                var facetCollector = new FacetsCollector();
                var indexSearcher = new IndexSearcher(reader);
                Query allDocsQuery = new MatchAllDocsQuery();
                FacetsCollector.Search(indexSearcher, allDocsQuery, int.MaxValue, facetCollector); 
                Facets facets = new FastTaxonomyFacetCounts(taxoReader, config, facetCollector);
                var results = facets.GetAllDims(int.MaxValue).First().LabelValues;

                int numberOfExamples = (int)results.Sum(result => result.Value);
                int numberOfCategories = results.Count();

                // Store the number of documents for each category in order to normalize term frequency numbers
                var documentsForCategory = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                foreach (var category in results)
                {
                    documentsForCategory[category.Label] = (int)category.Value;
                }

                TermInfo getTermInfo(BytesRef termBytes, int i)
                {
                    var term = new Term("shingles", termBytes);
                    double augmentedTermFrequency = 0.0;
                    var query = new TermQuery(term);
                    facetCollector = new FacetsCollector();
                    var hits = FacetsCollector.Search(indexSearcher, query, int.MaxValue, facetCollector);
                    facets = new FastTaxonomyFacetCounts(taxoReader, config, facetCollector);
                    results = facets.GetAllDims(int.MaxValue).First().LabelValues;
                    foreach (var category in results)
                    {
                        var categoryName = category.Label;
                        double tf = category.Value;
                        double maxTf = documentsForCategory[categoryName];
                        augmentedTermFrequency += tf / maxTf;
                    }

                    // This count need not be precise, nor be updated for every word
                    if ((i + 1) % LearningMachineMethods.UpdateFrequency == 0)
                    {
                        updateStatus(new StatusArgs
                        {
                            StatusMessage = "Scoring Terms... Terms processed: {0:N0}",
                            Int32Value = LearningMachineMethods.UpdateFrequency - 1
                        });
                    }

                    cancellationToken.ThrowIfCancellationRequested();

                    var termInfo = new TermInfo(
                        text: term.Text(),
                        termFrequency: augmentedTermFrequency,
                        documentFrequency: hits.TotalHits,
                        numberOfExamples: numberOfExamples,
                        numberOfCategories: numberOfCategories);
                    return termInfo;
                }

                // Since the 'tf-idf' is very similar to doc frequency, which is _much_ faster to get, start by limiting terms by that measure
                // Use a size-limited set to conserve memory
                var termPoolSize = MaxFeatures * numberOfCategories * 10;
                var topDocFreqTerms = new LimitedSizeSortedSet<Tuple<BytesRef, int>>(new DocFreqComparer(), termPoolSize);
                var termsEnum = MultiFields.GetTerms(reader, "shingles")?.GetIterator(null);
                if (termsEnum != null)
                {
                    BytesRef bytes = null;
                    while ((bytes = termsEnum.Next()) != null)
                    {
                        topDocFreqTerms.Add(Tuple.Create(BytesRef.DeepCopyOf(bytes), termsEnum.DocFreq));
                    }
                }

                var topTerms = new LimitedSizeSortedSet<TermInfo>(topDocFreqTerms.Select((t, i) => getTermInfo(t.Item1, i)), new TfIdfComparer(), MaxFeatures);
                string[] topTermsArray = new string[topTerms.Count];
                int count = 0;
                foreach(var term in topTerms.Reverse())
                {
                    topTermsArray[count++] = term.Text;
                }

                cancellationToken.ThrowIfCancellationRequested();

                return topTermsArray;
            }
        }

        /// <summary>
        /// Gets a <see cref="TokenFilter"/> to be used to generate terms. The terms returned from
        /// this <see cref="TokenFilter"/> will be upper-case and, for each input, will not contain
        /// duplicate values.
        /// </summary>
        /// <param name="input">The source text.</param>
        /// <returns>The <see cref="TokenFilter"/></returns>
        private TokenStream GetTokenStream(string input)
        {
            var reader = new StringReader(input.ToUpper(System.Globalization.CultureInfo.InvariantCulture));
            TokenStream result = new StandardTokenizer(_LUCENE_VERSION, reader);
            result = new StandardFilter(_LUCENE_VERSION, result);

            // With LUCENE_30, the StandardFilter already handles possessives but starting with the next version
            // it doesn't so the EnglishPossessiveFilter needs to be added to the chain
#pragma warning disable CS0618 // Type or member is obsolete
            if (_LUCENE_VERSION > LuceneVersion.LUCENE_30)
            {
                result = new EnglishPossessiveFilter(_LUCENE_VERSION, result);
            }
#pragma warning restore CS0618 // Type or member is obsolete

            // To maintain LUCENE 3.03 behavior, the deprecated setPositionIncrements=false setting
            // is necessary or else single character tokens that the length filter removes will be replaced with underscores
            // by the shingle filter (NOTE: this will not work starting with LUCENE_44)
#pragma warning disable CS0618 // Type or member is obsolete
            result = new LengthFilter(_LUCENE_VERSION, false, result, 2, 500);
#pragma warning restore CS0618 // Type or member is obsolete

            if (ShingleSize > 1)
            {
                result = new ShingleFilter(result, ShingleSize);
            }
            result = new DistinctFilter(result);
            return result;
        }

        /// <summary>
        /// An enumeration of distinct, upper-case string values (words and/or shingles) from a source text.
        /// </summary>
        /// <param name="input">The source text.</param>
        /// <returns>An enumeration of distinct, upper-case string values</returns>
        public IEnumerable<string> GetTerms(string input)
        {
            using (var stream = GetTokenStream(input))
            {
                stream.Reset();
                while (stream.IncrementToken())
                {
                    yield return stream.GetAttribute<ICharTermAttribute>().ToString();
                }
            }
        }

        /// <summary>
        /// Sets the internal bag-of-words object to null
        /// </summary>
        internal void Clear()
        {
            _nonSerializedBagOfWords = null;
        }

        /// <summary>
        /// Called when serializing
        /// </summary>
        /// <param name="context">The context.</param>
        [OnSerializing]
        private void OnSerializing(StreamingContext context)
        {
            if (_nonSerializedBagOfWords != null)
            {
                var vocab = _nonSerializedBagOfWords.CodeToString
                    .OrderBy(kv => kv.Key)
                    .Select(kv => kv.Value)
                    .ToArray();

                var ml = new MapLabel();
                using (var unencryptedStream = new MemoryStream())
                using (var encryptedStream = new MemoryStream())
                {
                    var serializer = new NetDataContractSerializer();
                    serializer.AssemblyFormat = FormatterAssemblyStyle.Simple;
                    serializer.Serialize(unencryptedStream, vocab);
                    unencryptedStream.Position = 0;
                    ExtractEncryption.EncryptStream(unencryptedStream, encryptedStream, _CONVERGENCE_MATRIX, ml);
                    _encryptedVocabulary = encryptedStream.ToArray();
                }
            }
        }

        /// <summary>
        /// Called when deserializing
        /// </summary>
        /// <param name="context">The context.</param>
        [OnDeserializing]
        private void OnDeserializing(StreamingContext context)
        {
            _version = 1;
            _useFeatureHashing = false;
            _encryptedVocabulary = null;
            _nonSerializedBagOfWords = null;
        }

        /// <summary>
        /// Called when deserialized
        /// </summary>
        /// <param name="context">The context.</param>
        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            ExtractException.Assert("ELI45477", "Cannot load newer SpatialStringFeatureVectorizer",
                _version <= _CURRENT_VERSION,
                "Current version", _CURRENT_VERSION,
                "Version to load", _version);

            if (_version < 3)
            {
#pragma warning disable CS0618 // Type or member is obsolete
                _nonSerializedBagOfWords = _bagOfWords;
                _bagOfWords = null;
#pragma warning restore CS0618 // Type or member is obsolete
            }

            // Build bag of words from vocab
            if (_encryptedVocabulary != null)
            {
                var ml = new MapLabel();
                using (var encryptedStream = new MemoryStream(_encryptedVocabulary))
                using (var unencryptedStream = new MemoryStream())
                {
                    ExtractEncryption.DecryptStream(encryptedStream, unencryptedStream, _CONVERGENCE_MATRIX, ml);
                    unencryptedStream.Position = 0;
                    var serializer = new NetDataContractSerializer();
                    serializer.AssemblyFormat = FormatterAssemblyStyle.Simple;
                    _nonSerializedBagOfWords = new Accord.MachineLearning.BagOfWords((string[])serializer.Deserialize(unencryptedStream));
                }
            }

            _version = _CURRENT_VERSION;
        }

        #endregion Private Methods

        #region Private Classes

        /// <summary>
        /// Token filter that produces only distinct terms for each input
        /// </summary>
        private sealed class DistinctFilter : TokenFilter
        {
            private HashSet<string> tokensSeen = new HashSet<string>();
            public DistinctFilter(TokenStream tokenStream)
                : base(tokenStream) { }

            public override bool IncrementToken()
            {
                while (m_input.IncrementToken())
                {
                    string currentTerm = m_input.GetAttribute<ICharTermAttribute>().ToString();
                    if (tokensSeen.Add(currentTerm))
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        #endregion Private Classes
    }
}
