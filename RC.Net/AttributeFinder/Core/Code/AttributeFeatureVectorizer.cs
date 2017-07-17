using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using ComAttribute = UCLID_AFCORELib.Attribute;

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
        /// </summary>
        const int _CURRENT_VERSION = 2;

        const string _BITMAP_PATTERN = @"(?in)\ABitmap: (?'width'\d+) x (?'height'\d+) = (?'data'\d+(,\d+)*)\z";

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
        private Lazy<Accord.MachineLearning.BagOfWords> _bagOfWords;

        /// <summary>
        /// Set of values seen during configuration
        /// </summary>
        private HashSet<string> _distinctValuesSeen = new HashSet<string>(StringComparer.Ordinal);

        [OptionalField(VersionAdded = 2)]
        private int _version = _CURRENT_VERSION;

        [OptionalField(VersionAdded = 2)]
        private int _bitmapSize;

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
            _bagOfWords = new Lazy<Accord.MachineLearning.BagOfWords>(() => CreateInitialBagOfWords());
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
                        return _bagOfWords.Value.CodeToString
                            .OrderBy(p => p.Key)
                            .Select(p => p.Value);
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
                    _bagOfWords = new Lazy<Accord.MachineLearning.BagOfWords>(() =>
                        new Accord.MachineLearning.BagOfWords(topTerms));

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

                        Match match;
                        if ((FeatureType == FeatureVectorizerType.Exists || FeatureType == FeatureVectorizerType.Bitmap)
                            && protoFeature.StartsWith("Bitmap", StringComparison.OrdinalIgnoreCase)
                            && (match = Regex.Match(protoFeature, _BITMAP_PATTERN)).Success
                            && int.TryParse(match.Groups["width"].Value, out int width)
                            && int.TryParse(match.Groups["height"].Value, out int height))
                        {
                            // Validate dimensions
                            int dataSize = match.Groups["data"].Value.Split(new[] { ',' }).Length;
                            ExtractException.Assert("ELI44670",
                                UtilityMethods.FormatInvariant($"Invalid/mismatched bitmap data for feature {Name}"),
                                dataSize == width * height && (BitmapSize == 0 || dataSize == BitmapSize));

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

                if (_bagOfWords.IsValueCreated)
                {
                    _bagOfWords = new Lazy<Accord.MachineLearning.BagOfWords>(() =>
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
                        var features = _bagOfWords.Value.GetFeatureVector(values.ToArray()).Select(i => (double)i);
                        return features.Concat(new[] { exists }).ToArray();

                    case FeatureVectorizerType.Bitmap:
                        var firstMatch = values.Select(value =>
                            Regex.Match(value, _BITMAP_PATTERN))
                            .FirstOrDefault(match => match.Success);

                        if (firstMatch == null)
                        {
                            return new double[BitmapSize].Concat(new[] { exists }).ToArray();
                        }
                        var data = firstMatch.Groups["data"].Value.Split(new[] { ',' });
                        if (data.Length != BitmapSize)
                        {
                            return new double[BitmapSize].Concat(new[] { exists }).ToArray();
                        }
                        else
                        {
                            return data.Select(s => double.TryParse(s, out double d) ? d : 0.0)
                                .Concat(new[] { exists }).ToArray();
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
        /// Called when deserializing
        /// </summary>
        /// <param name="context">The context.</param>
        [OnDeserializing]
        private void OnDeserializing(StreamingContext context)
        {
            _bitmapSize = 0;
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

            _version = _CURRENT_VERSION;
            _termFrequency = new Dictionary<string, Dictionary<string, int>>(StringComparer.Ordinal);
            _termToDocument = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);
            _categoriesSeen = new HashSet<string>(StringComparer.Ordinal);
            _documentsSeen = new HashSet<string>(StringComparer.Ordinal);
        }

        /// <summary>
        /// Creates the BagOfWords object using a TFIDF-ordered collection of terms
        /// </summary>
        /// <returns></returns>
        private Accord.MachineLearning.BagOfWords CreateInitialBagOfWords()
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

            var orderedTerms = _distinctValuesSeen.Select(term =>
            {
                double augmentedTermFrequency = 0.0;
                foreach (var category in _termFrequency)
                {
                    var categoryName = category.Key;
                    var tfForCategory = category.Value;
                    int tf = 0;
                    tfForCategory.TryGetValue(term, out tf);
                    double maxTf = categoryToMaxTermFrequency[categoryName];
                    augmentedTermFrequency += tf / maxTf;
                }
                return new TermInfo(
                    text: term,
                    termFrequency: augmentedTermFrequency,
                    documentFrequency: _termToDocument[term].Count,
                    numberOfExamples: numberOfExamples,
                    numberOfCategories: numberOfCategories);
            })
            .OrderByDescending(termInfo => termInfo.TermFrequencyInverseDocumentFrequency)
            .ThenBy(o => o.Text)
            .Select(o => o.Text)
            .ToArray();

            return new Accord.MachineLearning.BagOfWords(orderedTerms);
        }

        #endregion Private Methods
    }
}