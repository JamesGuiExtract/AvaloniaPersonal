using Extract.Encryption;
using Extract.Licensing;
using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;

namespace Extract.AttributeFinder
{
    /// <summary>
    /// An IFeatureVectorizer that produces feature vectors from <see cref="ComAttribute"/>s
    /// </summary>
    [CLSCompliant(false)]
    [Serializable]
    // Don't rename because it could break serialization
    [Obfuscation(Feature = "renaming", Exclude = true)]
    public class AttributeFeatureVectorizer : IFeatureVectorizer
    {
        #region Constants

        /// <summary>
        /// Version 2: Added field _bitmapSize to ensure compatible Bitmaps (used for FeatureVectorType.Bitmap)
        /// Version 3: Changed to store some fields encrypted so that the whole encoder doesn't need to be encrypted anymore
        /// </summary>
        const int _CURRENT_VERSION = 3;

        // Encryption password for serialization, renamed to obfuscate purpose
        private static readonly byte[] _CONVERGENCE_MATRIX = new byte[64]
            {
                185, 105, 109, 83, 148, 254, 79, 173, 128, 172, 12, 76, 61, 131, 66, 69, 236, 2, 76, 172, 158,
                197, 70, 243, 131, 95, 163, 206, 89, 164, 145, 134, 6, 25, 175, 201, 97, 177, 190, 24, 163, 144,
                141, 55, 75, 250, 20, 9, 176, 172, 55, 107, 172, 231, 69, 151, 34, 7, 232, 26, 112, 63, 202, 33
            };

        #endregion Constants

        #region Fields

        // Backing fields for properties
        private string _name;
        private FeatureVectorizerType _featureType;
        private bool _enabled;
        private uint _countOfNumericValuesOccurred;
        private uint _countOfNonnumericValuesOccurred;
        private uint _countOfMultipleValuesOccurred;

        /// <summary>
        /// Bag-of-words object to be created if/when it is needed
        /// </summary>
        [Obsolete("Use _nonSerializedBagOfWords")]
        private Lazy<Accord.MachineLearning.BagOfWords> _bagOfWords;

        [NonSerialized]
        private Lazy<Accord.MachineLearning.BagOfWords> _nonSerializedBagOfWords;

        /// <summary>
        /// Set of values seen during configuration
        /// </summary>
        private HashSet<string> _distinctValuesSeen = new HashSet<string>(StringComparer.Ordinal);

        [OptionalField(VersionAdded = 2)]
        private int _version = _CURRENT_VERSION;

        [OptionalField(VersionAdded = 2)]
        private int _bitmapSize;

        [OptionalField(VersionAdded = 3)]
        private byte[] _encryptedVocabulary;

        // -------------------------------------------------------------------------------------------------------------------------------
        // Non-serialized collections to track term frequency information in order to compute tf*idf score (used as a relevance heuristic)
        // -------------------------------------------------------------------------------------------------------------------------------
        [NonSerialized]
        private Dictionary<string, Dictionary<string, int>> _termFrequency = new Dictionary<string, Dictionary<string, int>>(StringComparer.Ordinal);
        [NonSerialized]
        private HashSet<string> _categoriesSeen = new HashSet<string>(StringComparer.Ordinal);
        [NonSerialized]
        private HashSet<string> _documentsSeen = new HashSet<string>(StringComparer.Ordinal);
        [NonSerialized]
        private Dictionary<string, HashSet<string>> _termToDocument = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Creates an instance of <see cref="AttributeFeatureVectorizer"/>
        /// </summary>
        /// <param name="name">The name of proto-feature attributes that this instance will use to generate its feature vector.</param>
        public AttributeFeatureVectorizer(string name)
        {
            Name = name;
            Enabled = true;
            FeatureType = FeatureVectorizerType.Exists;

            // Set up initialization of the bag of words object for if/when it is used
            _nonSerializedBagOfWords = new Lazy<Accord.MachineLearning.BagOfWords>(() => CreateInitialBagOfWords());
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// The name of the feature vectorizer, which is also the name of protoFeature attributes from which this vectorizer will derive features
        /// </summary>
        public string Name
        {
            get
            {
                return _name;
            }
            private set
            {
                if (value != _name)
                {
                    if (!UtilityMethods.IsValidIdentifier(value))
                    {
                        throw new ExtractException("ELI39619", @"Name must be a valid identifier of the form [_a-zA-Z]\w*");
                    }
                    _name = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets/sets the <see cref="FeatureVectorizerType"/> of this feature vectorizer (controls how the input is interpreted)
        /// </summary>
        public FeatureVectorizerType FeatureType
        {
            get
            {
                return _featureType;
            }
            set
            {
                if (value != _featureType)
                {
                    _featureType = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Whether changing the <see cref="FeatureVectorizerType"/> is allowed. Always <see langword="true"/>.
        /// </summary>
        public bool IsFeatureTypeChangeable
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Whether this feature vectorizer will produce a feature vector of length
        /// <see cref="FeatureVectorLength"/> (if <see langword="true"/>) or of zero length (if <see langword="false"/>)
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
        /// The number of numeric values seen during configuration
        /// </summary>
        public uint CountOfNumericValuesOccurred
        {
            get
            {
                return _countOfNumericValuesOccurred;
            }
            private set
            {
                if (value != _countOfNumericValuesOccurred)
                {
                    _countOfNumericValuesOccurred = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// The number of non-numeric values seen during configuration
        /// </summary>
        public uint CountOfNonnumericValuesOccurred
        {
            get
            {
                return _countOfNonnumericValuesOccurred;
            }
            private set
            {
                if (value != _countOfNonnumericValuesOccurred)
                {
                    _countOfNonnumericValuesOccurred = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// The number of cases where multiple attributes were present in a single example
        /// </summary>
        public uint CountOfMultipleValuesOccurred
        {
            get
            {
                return _countOfMultipleValuesOccurred;
            }
            set
            {
                if (value != _countOfMultipleValuesOccurred)
                {
                    _countOfMultipleValuesOccurred = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        /// A collection of distinct attribute values that this instance can utilize
        /// </summary>
        public IEnumerable<string> RecognizedValues
        {
            get
            {
                switch (FeatureType)
                {
                    case FeatureVectorizerType.Exists:
                        return Enumerable.Repeat("Present or not present", 1);
                    case FeatureVectorizerType.Numeric:
                        return Enumerable.Repeat(
                            string.Format(CultureInfo.CurrentCulture,
                                "{0:G4}\u2013{1:G4}", double.MinValue, double.MaxValue), 1);
                    case FeatureVectorizerType.DiscreteTerms:
                        // If BoW hasn't yet been created, skip that step
                        if (!_nonSerializedBagOfWords.IsValueCreated)
                        {
                            return GetBagOfWordsVocab();
                        }
                        else
                        {
                            return _nonSerializedBagOfWords.Value.CodeToString
                                .OrderBy(p => p.Key)
                                .Select(p => p.Value);
                        }
                    case FeatureVectorizerType.Bitmap:
                        return Enumerable.Repeat(string.Format(CultureInfo.CurrentCulture, "Bitmap of length {0:N0}", BitmapSize), 1);

                    default:
                        return Enumerable.Empty<string>();
                }
            }
        }

        /// <summary>
        /// The length of the feature vector that this vectorizer will produce.
        /// </summary>
        public int FeatureVectorLength
        {
            get
            {
                switch (FeatureType)
                {
                    case FeatureVectorizerType.Exists:
                        return 1;
                    case FeatureVectorizerType.Numeric:
                        return 2;
                    case FeatureVectorizerType.DiscreteTerms:
                        return RecognizedValues.Count() + 1;
                    case FeatureVectorizerType.Bitmap:
                        return BitmapSize + 1;
                    default:
                        return 0;
                }
            }
        }

        /// <summary>
        /// The size of the bitmap (used for FeatureVectorizerType.Bitmap)
        /// </summary>
        public int BitmapSize
        {
            get
            {
                return _bitmapSize;
            }
            private set
            {
                _bitmapSize = value;
            }
        }

        #endregion Properties

        #region Public Methods

        /// <summary>
        /// Creates a deep clone of this instance
        /// </summary>
        /// <returns>A clone of this instance</returns>
        public AttributeFeatureVectorizer DeepClone()
        {
            try
            {
                var clone = (AttributeFeatureVectorizer)MemberwiseClone();
                clone._distinctValuesSeen = new HashSet<string>(_distinctValuesSeen, StringComparer.Ordinal);

                clone._termFrequency = new Dictionary<string, Dictionary<string, int>>(StringComparer.Ordinal);
                foreach (var kv in _termFrequency)
                {
                    clone._termFrequency[kv.Key] = new Dictionary<string, int>(kv.Value, StringComparer.Ordinal);
                }

                clone._termToDocument = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);
                foreach (var kv in _termToDocument)
                {
                    clone._termToDocument[kv.Key] = new HashSet<string>(kv.Value, StringComparer.Ordinal);
                }

                clone._categoriesSeen = new HashSet<string>(_categoriesSeen, StringComparer.Ordinal);
                clone._documentsSeen = new HashSet<string>(_documentsSeen, StringComparer.Ordinal);

                return clone;
            }
            catch (Exception e)
            {
                throw e.AsExtract("ELI39813");
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
                if (FeatureType == FeatureVectorizerType.DiscreteTerms)
                {
                    var topTerms = RecognizedValues.Take(limit).ToArray();
                    _nonSerializedBagOfWords = new Lazy<Accord.MachineLearning.BagOfWords>(() =>
                        new Accord.MachineLearning.BagOfWords(topTerms));

                    // Instantiate the lazy object
                    if (_nonSerializedBagOfWords.Value != null) { }

                    // Clear full list
                    // https://extract.atlassian.net/browse/ISSUE-15231
                    _distinctValuesSeen.Clear();
                    
                    NotifyPropertyChanged("RecognizedValues");
                }
            }
            catch (Exception ex)
            {
                ex.ExtractDisplay("ELI41824");
            }
        }

        #endregion Public Methods

        #region Internal Methods

        /// <summary>
        /// Configures this instance by examining the values of all <see paramref="protoFeatures"/>.
        /// After this method has been run, this object will be ready to produce feature vectors
        /// </summary>
        /// <param name="protoFeatures">The values to consider when configuring.</param>
        internal void ComputeEncodingsFromTrainingData(IEnumerable<string> protoFeatures, string category, string docName)
        {
            try
            {
                // Track values seen and their types
                foreach (var protoFeature in protoFeatures)
                {
                    _distinctValuesSeen.Add(protoFeature);

                    var tfForCategory = _termFrequency.GetOrAdd(category, () =>
                        new Dictionary<string, int>(StringComparer.Ordinal));
                    int tf = tfForCategory.GetOrAdd(protoFeature, () => 0);
                    tfForCategory[protoFeature] = tf + 1;

                    var documents = _termToDocument.GetOrAdd(protoFeature, () =>
                        new HashSet<string>(StringComparer.Ordinal));
                    documents.Add(docName);

                    _documentsSeen.Add(docName);
                    _categoriesSeen.Add(category);

                    if (Double.TryParse(protoFeature, out double _))
                    {
                        CountOfNumericValuesOccurred++;
                    }
                    else
                    {
                        CountOfNonnumericValuesOccurred++;

                        if ((FeatureType == FeatureVectorizerType.Exists || FeatureType == FeatureVectorizerType.Bitmap)
                            && protoFeature.StartsWith("Bitmap", StringComparison.OrdinalIgnoreCase)
                            && XPathContext.TryGetBitmapDataFromString(protoFeature,
                                out int width, out int height, out double[] data))
                        {
                            // Validate dimensions
                            int dataSize = data.Length;
                            ExtractException.Assert("ELI44670",
                                UtilityMethods.FormatInvariant($"Invalid/mismatched bitmap data for feature {Name}"),
                                BitmapSize == 0 || dataSize == BitmapSize);

                            BitmapSize = dataSize;
                            FeatureType = FeatureVectorizerType.Bitmap;
                        }
                        else if (FeatureType == FeatureVectorizerType.Bitmap)
                        {
                            FeatureType = FeatureVectorizerType.DiscreteTerms;
                        }
                    }
                }

                // If there are multiple values then set FeatureType to be DiscreteTerms
                if (protoFeatures.Count() > 1)
                {
                    CountOfMultipleValuesOccurred++;
                    FeatureType = FeatureVectorizerType.DiscreteTerms;
                }
                // If FeatureType is Exists (default) that means that only one distinct value has been seen before this point
                // and so need to check to see if the type should be switched to numeric or discrete
                else if (FeatureType == FeatureVectorizerType.Exists)
                {
                    if (_distinctValuesSeen.Count > 1)
                    {
                        if (CountOfNonnumericValuesOccurred > 0)
                        {
                            FeatureType = FeatureVectorizerType.DiscreteTerms;
                        }
                        else
                        {
                            FeatureType = FeatureVectorizerType.Numeric;
                        }
                    }
                }
                else if (FeatureType == FeatureVectorizerType.Numeric
                    && CountOfNonnumericValuesOccurred > 0)
                {
                    FeatureType = FeatureVectorizerType.DiscreteTerms;
                }
                else if (FeatureType == FeatureVectorizerType.Bitmap
                    && CountOfNumericValuesOccurred > 0)
                {
                    FeatureType = FeatureVectorizerType.DiscreteTerms;
                }

                if (_nonSerializedBagOfWords.IsValueCreated)
                {
                    _nonSerializedBagOfWords = new Lazy<Accord.MachineLearning.BagOfWords>(() =>
                        CreateInitialBagOfWords());
                }
            }
            catch (Exception e)
            {
                throw e.AsExtract("ELI39524");
            }
        }

        /// <summary>
        /// Get feature vector using top-level <see cref="ComAttribute"/>s
        /// </summary>
        /// <param name="protoFeatures">The mapping of names to <see cref="ComAttribute"/>s to use for protoFeatures of this
        /// <see cref="AttributeFeatureVectorizer"/></param>
        /// <returns>Array of doubles of <see cref="FeatureVectorLength"/> or length of zero if not <see cref="Enabled"/></returns>
        internal double[] GetDocumentFeatureVector(NameToProtoFeaturesMap protoFeatures)
        {
            try
            {
                if (!Enabled)
                {
                    return new double[0];
                }

                IEnumerable<string> values = protoFeatures.GetProtoFeatureValues(Name);

                // Each type of feature vectorizer will have an exists component
                double exists = values.Any() ? 1.0 : 0.0;

                switch (FeatureType)
                {
                    // Just return exists/not exists value
                    case FeatureVectorizerType.Exists:
                        return new[] { exists };

                    // Return feature value + exists/not exists value
                    case FeatureVectorizerType.Numeric:
                        var firstNumeric = values.Select(value =>
                        {
                            bool success = double.TryParse(value, out double number);
                            return new { number, success };
                        })
                        .FirstOrDefault(pair => pair.success);
                        var feature = firstNumeric == null ? 0.0 : firstNumeric.number;
                        return new[] { feature, exists }; 

                    // Return BoW feature vector + exists/not exists value
                    case FeatureVectorizerType.DiscreteTerms:
                        var features = _nonSerializedBagOfWords
                            .Value.GetFeatureVector(values.ToArray())
                            .Select(i => (double)i);
                        return features.Concat(new[] { exists }).ToArray();

                    case FeatureVectorizerType.Bitmap:
                        var firstMatch = values.Select(value =>
                            XPathContext.TryGetBitmapDataFromString(value, out var _, out var _, out var data)
                            ? data
                            : null)
                            .FirstOrDefault(data => data != null);

                        if (firstMatch == null)
                        {
                            return new double[BitmapSize].Concat(new[] { exists }).ToArray();
                        }
                        if (firstMatch.Length != BitmapSize)
                        {
                            return new double[BitmapSize].Concat(new[] { exists }).ToArray();
                        }
                        else
                        {
                            return firstMatch.Concat(new[] { exists }).ToArray();
                        }
                    default:
                        throw new ExtractException("ELI39525", "Unsupported FeatureType: " + FeatureType.ToString());
                }
            }
            catch (Exception e)
            {
                throw e.AsExtract("ELI39526");
            }
        }

        /// <summary>
        /// Gets the feature vector using <see cref="NameToProtoFeaturesMap"/>s
        /// </summary>
        /// <param name="protoFeatureGroups">The collection of <see cref="NameToProtoFeaturesMap"/>s to search for
        /// groups of sub-attribute protoFeatures of this <see cref="AttributeFeatureVectorizer"/>
        /// (sub-attributes with names that matches the name of this instance)</param>
        /// <returns>If <see cref="Enabled"/>, then an enumeration of feature vectors that has a length equal to
        /// that of <see paramref="protoFeatureGroups"/>. Else, an empty enumeration.</returns>
        internal IEnumerable<double[]> GetFeatureVectorsForEachGroup(IEnumerable<NameToProtoFeaturesMap> protoFeatureGroups)
        {
            try
            {
                if (!Enabled)
                {
                    return Enumerable.Empty<double[]>();
                }

                return protoFeatureGroups.Select(GetDocumentFeatureVector);
            }
            catch (Exception e)
            {
                throw e.AsExtract("ELI39527");
            }
        }

        #endregion Internal Methods

        #region Overrides

        /// <summary>
        /// Whether this instance is equal to another.
        /// </summary>
        /// <remarks>Does not compare counts of types of values seen. Only compares <see cref="RecognizedValues"/> if
        /// <see cref="FeatureType"/> is <see cref="FeatureVectorizerType.DiscreteTerms"/></remarks>
        /// <param name="obj">The instance to compare with</param>
        /// <returns><see langword="true"/> if this instance has equal property values, else <see langword="false"/></returns>
        public override bool Equals(object obj)
        {
            if (Object.ReferenceEquals(this, obj))
            {
                return true;
            }

            var other = obj as AttributeFeatureVectorizer;
            if (other == null
                || other.Enabled != Enabled
                || other.FeatureType != FeatureType
                || other.Name != Name
                || FeatureType == FeatureVectorizerType.DiscreteTerms &&
                   !other.RecognizedValues.SequenceEqual(RecognizedValues)
                || FeatureType == FeatureVectorizerType.Bitmap &&
                   other.BitmapSize != BitmapSize
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
                .Hash(Enabled)
                .Hash(FeatureType)
                .Hash(Name)
                .Hash(BitmapSize);
            if (FeatureType == FeatureVectorizerType.DiscreteTerms)
            {
                foreach (var value in RecognizedValues)
                {
                    hash = hash.Hash(value);
                }
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
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Called when serializing
        /// </summary>
        /// <param name="context">The context.</param>
        [OnSerializing]
        private void OnSerializing(StreamingContext context)
        {
            // https://extract.atlassian.net/browse/ISSUE-15231
            // This limits ones ability to change feature type after save/load but
            // it is the simplest way to save storage space
            _distinctValuesSeen.Clear();

            var vocab = _nonSerializedBagOfWords.Value.CodeToString
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

        /// <summary>
        /// Called when deserializing
        /// </summary>
        /// <param name="context">The context.</param>
        [OnDeserializing]
        private void OnDeserializing(StreamingContext context)
        {
            _bitmapSize = 0;
            _termFrequency = new Dictionary<string, Dictionary<string, int>>(StringComparer.Ordinal);
            _termToDocument = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);
            _categoriesSeen = new HashSet<string>(StringComparer.Ordinal);
            _documentsSeen = new HashSet<string>(StringComparer.Ordinal);
            _encryptedVocabulary = null;
            _nonSerializedBagOfWords = new Lazy<Accord.MachineLearning.BagOfWords>(() =>
            {
                ExtractException.Assert("ELI45610", "Logic error: vocabulary is null", _encryptedVocabulary != null);

                var ml = new MapLabel();
                using (var encryptedStream = new MemoryStream(_encryptedVocabulary))
                using (var unencryptedStream = new MemoryStream())
                {
                    ExtractEncryption.DecryptStream(encryptedStream, unencryptedStream, _CONVERGENCE_MATRIX, ml);
                    unencryptedStream.Position = 0;
                    var serializer = new NetDataContractSerializer();
                    serializer.AssemblyFormat = FormatterAssemblyStyle.Simple;
                    return new Accord.MachineLearning.BagOfWords((string[])serializer.Deserialize(unencryptedStream));
                }
            });
        }

        /// <summary>
        /// Called when deserialized
        /// </summary>
        /// <param name="context">The context.</param>
        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            ExtractException.Assert("ELI44672", "Cannot load newer AttributeFeatureVectorizer",
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

            _version = _CURRENT_VERSION;
        }

        /// <summary>
        /// Creates the BagOfWords object using a TFIDF-ordered collection of terms
        /// </summary>
        /// <returns></returns>
        private Accord.MachineLearning.BagOfWords CreateInitialBagOfWords()
        {
            var vocab = GetBagOfWordsVocab().ToArray();
            return new Accord.MachineLearning.BagOfWords(vocab);
        }

        /// <summary>
        /// Creates the BagOfWords object using a TFIDF-ordered collection of terms
        /// </summary>
        /// <returns></returns>
        private IEnumerable<string> GetBagOfWordsVocab()
        {
            // _termFrequency, etc, data is not persisted but Lazy<T> objects are instantiated when they
            // are serialized (I just discovered) so we will never get here when the collections are empty.
            // However, if there have been zero values seen (e.g., only empty or 1-char-long values are tokenized)
            // then there won't be any terms and thus no term frequency data
            // https://extract.atlassian.net/browse/ISSUE-14611
            ExtractException.Assert("ELI41828", "Logic exception: No term-frequency data available",
                !_distinctValuesSeen.Any() || _termFrequency.Any());

            int numberOfExamples = _documentsSeen.Count;
            int numberOfCategories = _categoriesSeen.Count;
            var categoryToMaxTermFrequency = _termFrequency.Select(categoryToTermFrequency =>
                new
                {
                    category = categoryToTermFrequency.Key,
                    maxTF = categoryToTermFrequency.Value.Values.Any()
                            ? categoryToTermFrequency.Value.Values.Max()
                            : 1
                }).ToDictionary(o => o.category, o => o.maxTF, StringComparer.Ordinal);

            TermInfo getTermInfo(string term)
            {
                double augmentedTermFrequency = 0.0;
                foreach (var category in _termFrequency)
                {
                    var categoryName = category.Key;
                    var tfForCategory = category.Value;
                    tfForCategory.TryGetValue(term, out int tf);
                    double maxTf = categoryToMaxTermFrequency[categoryName];
                    augmentedTermFrequency += tf / maxTf;
                }
                var termInfo = new TermInfo(
                    text: term,
                    termFrequency: augmentedTermFrequency,
                    documentFrequency: _termToDocument[term].Count,
                    numberOfExamples: numberOfExamples,
                    numberOfCategories: numberOfCategories);
                return termInfo;
            }
            var orderedTerms = _distinctValuesSeen.Select(getTermInfo)
            .OrderByDescending(termInfo => termInfo.TermFrequencyInverseDocumentFrequency)
            .ThenBy(o => o.Text)
            .Select(o => o.Text);

            return orderedTerms;
        }

        #endregion Private Methods
    }
}