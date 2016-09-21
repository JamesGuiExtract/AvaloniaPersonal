using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
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
        private HashSet<string> _distinctValuesSeen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

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
            _bagOfWords = new Lazy<Accord.MachineLearning.BagOfWords>(() =>
                new Accord.MachineLearning.BagOfWords(DistinctValuesSeen.ToArray()));
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
        /// A collection of distinct attribute values seen during configuration
        /// </summary>
        public IEnumerable<string> DistinctValuesSeen
        {
            get
            {
                return _distinctValuesSeen.OrderBy(s => s, StringComparer.OrdinalIgnoreCase);
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
                        return DistinctValuesSeen.Count() + 1;
                    default:
                        return 0;
                }
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
                clone._distinctValuesSeen = new HashSet<string>(_distinctValuesSeen);
                return clone;
            }
            catch (Exception e)
            {
                throw e.AsExtract("ELI39813");
            }
        }

        #endregion Public Methods

        #region Internal Methods

        /// <summary>
        /// Configures this instance by examining the values of all <see paramref="protoFeatures"/>.
        /// After this method has been run, this object will be ready to produce feature vectors
        /// </summary>
        /// <param name="protoFeatures">The values to consider when configuring.</param>
        internal void ComputeEncodingsFromTrainingData(IEnumerable<string> protoFeatures)
        {
            try
            {
                // Track values seen and their types
                foreach (var protoFeature in protoFeatures)
                {
                    _distinctValuesSeen.Add(protoFeature);
                    double _;
                    if (Double.TryParse(protoFeature, out _))
                    {
                        CountOfNumericValuesOccurred++;
                    }
                    else
                    {
                        CountOfNonnumericValuesOccurred++;
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

                if (_bagOfWords.IsValueCreated)
                {
                    _bagOfWords = new Lazy<Accord.MachineLearning.BagOfWords>(() =>
                        new Accord.MachineLearning.BagOfWords(DistinctValuesSeen.ToArray()));
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
                            double number;
                            bool success = double.TryParse(value, out number);
                            return new { number, success };
                        })
                        .FirstOrDefault(pair => pair.success);
                        var feature = firstNumeric == null ? 0.0 : firstNumeric.number;
                        return new[] { feature, exists }; 

                    // Return BoW feature vector + exists/not exists value
                    case FeatureVectorizerType.DiscreteTerms:
                        var features = _bagOfWords.Value.GetFeatureVector(values.ToArray()).Select(i => (double)i);
                        return features.Concat(new[] { exists }).ToArray();

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
        /// <remarks>Does not compare counts of types of values seen. Only compares <see cref="DistinctValuesSeen"/> if
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
                   !other.DistinctValuesSeen.SequenceEqual(DistinctValuesSeen)
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
                .Hash(Name);
            if (FeatureType == FeatureVectorizerType.DiscreteTerms)
            {
                foreach (var value in DistinctValuesSeen)
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
        /// In VS 2015 could use this: [CallerMemberName] String propertyName = ""
        private void NotifyPropertyChanged(string propertyName = "")
        {
            var eventHandler = PropertyChanged;
            if (eventHandler != null)
            {
                eventHandler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion Private Methods
    }
}