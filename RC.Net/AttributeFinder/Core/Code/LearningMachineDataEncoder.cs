using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using UCLID_AFUTILSLib;
using UCLID_COMUTILSLib;
using UCLID_RASTERANDOCRMGMTLib;
using ComAttribute = UCLID_AFCORELib.Attribute;

namespace Extract.AttributeFinder
{

    /// <summary>
    /// Determines input and output format and other details
    /// </summary>
    public enum LearningMachineUsage
    {
        /// <summary>
        /// Unknown usage
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Predicting the category of an input document
        /// </summary>
        DocumentCategorization = 1,

        /// <summary>
        /// Predicting where document boundaries should be created
        /// </summary>
        Pagination = 2,

        /// <summary>
        /// Predicting the category of a collection of <see cref="ComAttribute"/>s
        /// </summary>
        AttributeCategorization = 3
    }

    /// <summary>
    /// The type of the underlying learning machine
    /// </summary>
    public enum LearningMachineType
    {
        /// <summary>
        /// Unknown type
        /// </summary>
        Unknown = 0,
        
        /// <summary>
        /// Accord.Neuro.ActivationNetwork
        /// </summary>
        ActivationNetwork = 1,

        /// <summary>
        /// Accord.MachineLearning.VectorMachines.MulticlassSupportVectorMachine
        /// </summary>
        MulticlassSVM = 2,
        
        /// <summary>
        /// Accord.MachineLearning.VectorMachines.MultilabelSupportVectorMachine
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Multilabel")]
        MultilabelSVM = 3
    }

    /// <summary>
    /// Controls how Attributes are converted into feature vectors
    /// </summary>
    public enum FeatureVectorizerType
    {
        /// <summary>
        /// Creates a feature vector with a length of one with two possible values, one if the Attribute exists
        /// and a second if it does not
        /// </summary>
        Exists = 0,

        /// <summary>
        /// Creates a feature vector with a length of two where one feature is whether the Attribute exists
        /// and the other feature is a continuous numeric value.
        /// </summary>
        Numeric = 1,

        /// <summary>
        /// Creates a feature vector of at least a length of two where one feature is whether the Attribute exists
        /// and the other features are for presence/absence of vocabulary terms. AKA Bag-of-Words.
        /// </summary>
        DiscreteTerms = 2,

        /// <summary>
        /// Creates a feature vector of pixelcount + 1 from an array of pixel values encoded in the attribute's value
        /// </summary>
        Bitmap = 3
    }

    /// <summary>
    /// An IFeatureVectorizer is capable of turning input of some kind into string values for display
    /// and numeric arrays for machine learning purposes. This interface allows enabling/disabling
    /// and changing the type of a feature vectorizer, getting the feature vector length and viewing
    /// the values that it has been exposed to during configuration from training data.
    /// </summary>
    public interface IFeatureVectorizer : INotifyPropertyChanged
    {
        /// <summary>
        /// The name of the feature
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The string representation of the input examples that this vectorizer has been configured with.
        /// </summary>
        IEnumerable<string> RecognizedValues { get; }

        /// <summary>
        /// The length of the feature vector that this vectorizer can produce.
        /// </summary>
        int FeatureVectorLength { get; }

        /// <summary>
        /// Gets/sets the <see cref="FeatureVectorizerType"/> of this feature vectorizer (controls how the input is interpreted)
        /// </summary>
        FeatureVectorizerType FeatureType { get; set; }

        /// <summary>
        /// Whether changing the <see cref="FeatureVectorizerType"/> is allowed.
        /// </summary>
        bool IsFeatureTypeChangeable { get; }

        /// <summary>
        /// Whether this feature vectorizer will produce a feature vector of length
        /// <see cref="FeatureVectorLength"/> (if <see langword="true"/>) or of zero length (if <see langword="false"/>)
        /// </summary>
        bool Enabled { get; set; }

        /// <summary>
        /// Limits bag of words to the top <see paramref="limit"/>terms.
        /// </summary>
        /// <param name="limit">The number of terms to limit to.</param>
        void LimitToTopTerms(int limit);
    }

    /// <summary>
    /// Class used to wrap a dictionary of proto-feature names to string values
    /// </summary>
    internal class NameToProtoFeaturesMap
    {
        private Dictionary<string, List<string>> _nameToFeatureValues;

        /// <summary>
        /// Constructs a new instance from a collection of <see cref="ComAttribute"/>s
        /// <param name="attributes">The <see cref="ComAttribute"/>s that will be used
        /// to generate a mapping of names to values.</param>
        /// </summary>
        public NameToProtoFeaturesMap(IEnumerable<ComAttribute> attributes = null)
        {
            if (attributes != null)
            {
                _nameToFeatureValues = MakeDictionary(attributes);
            }
        }

        /// <summary>
        /// Constructs a new instance from a collection of <see cref="ComAttribute"/>s
        /// <param name="attributes">The <see cref="ComAttribute"/>s that will be used
        /// to generate a mapping of names to values.</param>
        /// <param name="queryForAttributesToTokenize">The AFQuery to be used to divide
        /// the input into attributes to tokenize and attributes to use the values of
        /// in their entirety</param>
        /// <param name="shingleSize">The size of word-n-grams to make out of any tokens</param>
        /// </summary>
        public NameToProtoFeaturesMap(IEnumerable<ComAttribute> attributes,
            string queryForAttributesToTokenize, int shingleSize)
        {
            if (attributes != null)
            {
                if (string.IsNullOrEmpty(queryForAttributesToTokenize))
                {
                    _nameToFeatureValues = MakeDictionary(attributes);
                }
                else
                {
                    var afutil = new AFUtilityClass();
                    var attributesVector = attributes.ToIUnknownVector();
                    var attributesToTokenize = afutil.QueryAttributes(attributesVector,
                        queryForAttributesToTokenize, bRemoveMatches: true)
                        .ToIEnumerable<ComAttribute>();

                    if (!attributesToTokenize.Any())
                    {
                        _nameToFeatureValues = MakeDictionary(attributes);
                    }
                    else
                    {
                        _nameToFeatureValues = MakeDictionary(attributesVector.ToIEnumerable<ComAttribute>());

                        var tokenizer = new SpatialStringFeatureVectorizer(null, shingleSize, -1);
                        foreach (var attribute in attributesToTokenize)
                        {
                            var text = attribute.Value.String;
                            var values = _nameToFeatureValues.GetOrAdd(attribute.Name,
                                () => new List<string>());
                            values.AddRange(tokenizer.GetTerms(text));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets the proto-feature values for the <see paramref="name"/>
        /// </summary>
        /// <param name="name">The name of the proto-feature</param>
        /// <returns>An enumerable of values for the <see paramref="name"/></returns>
        public IEnumerable<string> GetProtoFeatureValues(string name)
        {
            List<string> values;
            if (_nameToFeatureValues == null || !_nameToFeatureValues.TryGetValue(name, out values))
            {
                return Enumerable.Empty<string>();
            }

            return values;
        }

        /// <summary>
        /// Get the enumerator for the internal name-to-values dictionary
        /// </summary>
        /// <returns>The enumerator for the name-to-values dictionary</returns>
        public IEnumerator<KeyValuePair<string, List<string>>> GetEnumerator()
        {
            if (_nameToFeatureValues == null)
            {
                return Enumerable.Empty<KeyValuePair<string, List<string>>>().GetEnumerator();
            }
            return _nameToFeatureValues.GetEnumerator();
        }

        // Initialize map of name-to-values
        static Dictionary<string, List<string>> MakeDictionary(IEnumerable<ComAttribute> attributes)
        {
            return attributes
                .GroupBy(a => a.Name)
                .ToDictionary(g => g.Key,
                              g => g.Select(a => a.Value.String).ToList(),
                              StringComparer.OrdinalIgnoreCase);
        }
    }

    /// <summary>
    /// Object that can transform <see cref="ISpatialString"/>s and <see cref="ComAttribute"/>s into numeric feature vectors.
    /// </summary>
    [CLSCompliant(false)]
    [Serializable]
    // Don't rename because it could break serialization
    [Obfuscation(Feature = "renaming", Exclude = true)]
    public class LearningMachineDataEncoder
    {

        #region Constants

        /// <summary>
        /// Current version.
        /// Version 2: Add AttributesToTokenizeFilter property and backing field
        ///            Add AttributeFeatureShingleSize property and backing field
        /// </summary>
        const int _CURRENT_VERSION = 2;

        // Used for document categorization.
        // Used to represent categories that are in testing data but not in training data.
        public static readonly string UnknownCategoryName = "Unknown_CB588EBE-4861-40FF-A640-BEF6BB42A54A";

        /// <summary>
        /// Code reserved to represent an 'other' category that will not be assigned to a real
        /// category when usage is <see cref="LearningMachineUsage.DocumentCategorization"/>
        /// </summary>
        public static readonly int UnknownOrNegativeCategoryCode = 0;

        /// <summary>
        /// Code used to represent the prediction that a page pair encloses a document break
        /// </summary>
        public static readonly int FirstPageCategoryCode = 1;

        // Private values used for pagination categories
        static readonly string _FIRST_PAGE_CATEGORY = "FirstPage";
        public static readonly string NotFirstPageCategory = "NotFirstPage";
        static readonly int _NOT_FIRST_PAGE_CATEGORY_CODE = 0;

        /// <summary>
        /// The name of page attributes that have pagination protofeature subattributes
        /// </summary>
        public static readonly string PageAttributeName = "Page";

        // For pagination, the query for answer page range attributes
        static readonly string _PAGE_ATTRIBUTE_QUERY = "Document/Pages";

        public static readonly string CategoryAttributeName = "AttributeType";

        #endregion Constants

        #region Fields

        /// <summary>
        /// Persist the current version to prevent the use of newer versions of this object from
        /// being used incorrectly on an older software version.
        /// </summary>
        [OptionalField(VersionAdded = 2)]
        private int _version = _CURRENT_VERSION;

        /// <summary>
        /// <see cref="IAFUtility"/> to be used by this thread to resolve attribute queries
        /// </summary>
        private static readonly ThreadLocal<IAFUtility> _afUtility = new ThreadLocal<IAFUtility>(() => new AFUtilityClass());

        /// <summary>
        /// Regex used to parse page ranges of pagination expected VOA files
        /// </summary>
        private static readonly ThreadLocal<Regex> _pageRangeRegex =
            new ThreadLocal<Regex>(() => new Regex(@"\b(?'start'\d+)\b(\s*-\s*(?'end'\d+\b))?"));

        // Backing fields for properties
        private SpatialStringFeatureVectorizer _autoBagOfWords;
        private IEnumerable<AttributeFeatureVectorizer> _attributeFeatureVectorizers;
        private Dictionary<string, int> _answerNameToCode;
        private Dictionary<int, string> _answerCodeToName;
        private string _attributeFilter;
        private bool _negateFilter;
        private LearningMachineUsage _machineUsage;

        [OptionalField(VersionAdded = 2)]
        private string _attributesToTokenizeFilter;

        [OptionalField(VersionAdded = 2)]
        private int _attributeVectorizerShingleSize;

        [OptionalField(VersionAdded = 2)]
        private int _attributeVectorizerMaxFeatures;

        [OptionalField(VersionAdded = 2)]
        private string _negativeClassName;

        #endregion Fields

        #region Properties

        /// <summary>
        /// Gets the <see cref="SpatialStringFeatureVectorizer"/> object.
        /// </summary>
        public SpatialStringFeatureVectorizer AutoBagOfWords
        {
            get
            {
                return _autoBagOfWords;
            }
            private set
            {
                if (value != _autoBagOfWords)
                {
                    _autoBagOfWords = value;
                }
            }
        }

        /// <summary>
        /// Gets the <see cref="AttributeFeatureVectorizer"/> collection.
        /// </summary>
        public IEnumerable<AttributeFeatureVectorizer> AttributeFeatureVectorizers
        {
            get
            {
                return _attributeFeatureVectorizers;
            }
            private set
            {
                if (value != _attributeFeatureVectorizers)
                {
                    _attributeFeatureVectorizers = value;
                }
            }
        }

        /// <summary>
        /// Gets the dictionary of category names to the numeric codes assigned to them
        /// </summary>
        public Dictionary<string, int> AnswerNameToCode
        {
            get
            {
                return _answerNameToCode;
            }
            private set
            {
                if (value != _answerNameToCode)
                {
                    _answerNameToCode = value;
                }
            }
        }

        /// <summary>
        /// Gets the dictionary of numeric codes to category names
        /// </summary>
        public Dictionary<int, string> AnswerCodeToName
        {
            get
            {
                return _answerCodeToName;
            }
            private set
            {
                if (value != _answerCodeToName)
                {
                    _answerCodeToName = value;
                }
            }
        }

        private int AttributeFeatureVectorLength
        {
            get
            {
                // NOTE: vectorizers return their lengths even if not enabled so only count if enabled
                return AttributeFeatureVectorizers
                    .Where(vectorizer => vectorizer.Enabled)
                    .Sum(vectorizer => vectorizer.FeatureVectorLength);
            }

        }
        /// <summary>
        /// Gets the size of the feature vectors that this instance will produce for any input
        /// </summary>
        public int FeatureVectorLength
        {
            get
            {
                return AutoBagOfWords != null && AutoBagOfWords.Enabled
                    ? AutoBagOfWords.FeatureVectorLength + AttributeFeatureVectorLength
                    : AttributeFeatureVectorLength;
            }
        }

        /// <summary>
        /// Gets/sets the AFQuery to select proto-feature attributes. If <see langword="null"/>
        /// then all attributes will be used.
        /// </summary>
        public string AttributeFilter
        {
            get
            {
                return _attributeFilter;
            }
            set
            {
                if (value != _attributeFilter)
                {
                    _attributeFilter = value;
                }
            }
        }

        /// <summary>
        /// Gets/sets the AFQuery to select which proto-feature attributes will be tokenized.
        /// If <see langword="null"/> then no attributes will be tokenized.
        /// </summary>
        public string AttributesToTokenizeFilter
        {
            get
            {
                return _attributesToTokenizeFilter;
            }
            set
            {
                _attributesToTokenizeFilter = value;
            }
        }

        /// <summary>
        /// Gets/sets whether to use only attributes that are not selected by
        /// <see cref="AttributeFilter"/> for proto-features
        /// </summary>
        public bool NegateFilter
        {
            get
            {
                return _negateFilter;
            }
            set
            {
                if (value != _negateFilter)
                {
                    _negateFilter = value;
                }
            }
        }

        /// <summary>
        /// Gets/sets the max shingle (word n-gram) size to be used for tokenized
        /// attribute features.
        /// </summary>
        public int AttributeVectorizerShingleSize
        {
            get
            {
                return _attributeVectorizerShingleSize;
            }
            set
            {
                _attributeVectorizerShingleSize = value;
            }
        }

        /// <summary>
        /// Gets/sets the maximum number of features (distinct values seen) to retain per attribute feature vectorizer.
        /// </summary>
        public int AttributeVectorizerMaxDiscreteTermsFeatures
        {
            get
            {
                return _attributeVectorizerMaxFeatures;
            }
            set
            {
                _attributeVectorizerMaxFeatures = value;
            }
        }

        /// <summary>
        /// Gets the <see cref="LearningMachineUsage"/> of this instance
        /// </summary>
        public LearningMachineUsage MachineUsage
        {
            get
            {
                return _machineUsage;
            }
            private set
            {
                if (value != _machineUsage)
                {
                    _machineUsage = value;
                }
            }
        }

        /// <summary>
        /// Gets whether feature encodings have been computed successfully
        /// </summary>
        public bool AreEncodingsComputed
        {
            get
            {
                return AutoBagOfWords != null && AutoBagOfWords.AreEncodingsComputed
                    || AutoBagOfWords == null && AttributeFeatureVectorizers.Any();
            }
        }

        /// <summary>
        /// The name to use for the negative/unknown class (e.g., the document type assigned when a testing a classifier
        /// against a new set of data and the expected document type is not one that is recognized by the classifier)
        /// </summary>
        public string NegativeClassName
        {
            get
            {
                return _negativeClassName;
            }
            set
            {
                _negativeClassName = value ?? "";
            }
        }

        #endregion Properties

        #region Constructors

        // Reserve default constructor for private use
        private LearningMachineDataEncoder()
        {
        }

        /// <summary>
        /// Creates an instance of <see cref="LearningMachineDataEncoder" />
        /// </summary>
        /// <param name="usage">The <see cref="LearningMachineUsage" /> for this instance.</param>
        /// <param name="autoBagOfWords">The optional <see cref="SpatialStringFeatureVectorizer" /> for this instance.</param>
        /// <param name="attributeFilter">AFQuery to select proto-feature attributes. If <see langword="null" />
        /// then all attributes will be used.</param>
        /// <param name="negateFilter">Whether to use only attributes that are not selected by
        /// <see paramref="attributeFilter" /> for proto-features</param>
        /// <param name="attributeVectorizerMaxFeatures">The max number of terms retained for each attribute vectorizer.</param>
        /// <param name="attributesToTokenize">A query used to select a subset of attributes to be tokenized and converted into shingles.</param>
        /// <param name="attributeVectorizerShingleSize">The maximum size of word-n-grams to be derived from attribute vectorizer tokens.</param>
        /// <param name="negativeClassName">The name to designate as the negative class (use <c>null</c> for default according to <see paramref="usage"/>)</param>
        public LearningMachineDataEncoder(LearningMachineUsage usage, SpatialStringFeatureVectorizer autoBagOfWords = null,
            string attributeFilter = null, bool negateFilter = false, int attributeVectorizerMaxFeatures = 500,
            string attributesToTokenize = null, int attributeVectorizerShingleSize = 1, string negativeClassName = null)
        {
            MachineUsage = usage;
            AutoBagOfWords = autoBagOfWords;
            AttributeFeatureVectorizers = Enumerable.Empty<AttributeFeatureVectorizer>();
            AttributeFilter = attributeFilter;
            NegateFilter = negateFilter;
            AnswerCodeToName = new Dictionary<int, string>();
            AnswerNameToCode = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            AttributeVectorizerMaxDiscreteTermsFeatures = attributeVectorizerMaxFeatures;
            AttributesToTokenizeFilter = attributesToTokenize;
            AttributeVectorizerShingleSize = attributeVectorizerShingleSize;
            NegativeClassName = negativeClassName ??
                (MachineUsage == LearningMachineUsage.AttributeCategorization
                    ? ""
                    : MachineUsage == LearningMachineUsage.Pagination
                        ? NotFirstPageCategory
                        : UnknownCategoryName);
        }

        #endregion Constructors

        #region Public Methods

        /// <summary>
        /// Gets an enumeration of predictions (category names) from a VOA file
        /// </summary>
        /// <param name="attributesFilePath">The path to the VOA or EAV file</param>
        /// <param name="numberOfPages">The number of pages in the associated image</param>
        /// <returns>An enumeration of predictions (category names) from a VOA file</returns>
        public static IEnumerable<string> ExpandPaginationAnswerVOA(string attributesFilePath, int numberOfPages)
        {
            try
            {
                var attributes = _afUtility.Value.GetAttributesFromFile(attributesFilePath);
                attributes.ReportMemoryUsage();

                // Parse Pages attributes with regex
                var pageRanges = _afUtility.Value.QueryAttributes(attributes, _PAGE_ATTRIBUTE_QUERY, false)
                    .ToIEnumerable<ComAttribute>()
                    .SelectMany(attr => _pageRangeRegex.Value.Matches(attr.Value.String).Cast<Match>())
                    .Select(pageRange =>
                    {
                        int startPage = 0;
                        int endPage = 0;
                        bool hasEndPage = false;
                        if (Int32.TryParse(pageRange.Groups["start"].Value, out startPage))
                        {
                            var endGroup = pageRange.Groups["end"];
                            if (endGroup.Success && Int32.TryParse(endGroup.Value, out endPage))
                            {
                                hasEndPage = true;
                            }
                        }
                        return new {startPage, endPage, hasEndPage};
                    });

                var documentBreaks = Enumerable.Repeat(NotFirstPageCategory, numberOfPages - 1).ToArray();
                foreach(var pageRange in pageRanges)
                {
                    if (pageRange.startPage > 1)
                    {
                        // Since documentBreaks starts at page 2, startPage - 2 is the index of the starting page
                        documentBreaks[pageRange.startPage - 2] = _FIRST_PAGE_CATEGORY;
                    }

                    // In case of gaps between documents, set a document break after each end page.
                    if (pageRange.hasEndPage && pageRange.endPage + 1 < numberOfPages)
                    {
                        // Since documentBreaks starts at page 2, endPage - 1 is the index of the next starting page
                        documentBreaks[pageRange.endPage - 1] = _FIRST_PAGE_CATEGORY;
                    }
                    // If no end page then set a break after start page (this is a one-page sub-document)
                    else if (!pageRange.hasEndPage && pageRange.startPage + 1 < numberOfPages)
                    {
                        // Since documentBreaks starts at page 2, startPage - 1 is the index of the next starting page
                        documentBreaks[pageRange.startPage - 1] = _FIRST_PAGE_CATEGORY;
                    }
                }
                return documentBreaks;
            }
            catch (Exception e)
            {
                var ue = e.AsExtract("ELI39543");
                ue.AddDebugData("Answer file", attributesFilePath, false);
                throw ue;
            }
        }

        /// <summary>
        /// Gets an enumeration of feature vectors from a <see cref="ISpatialString"/> and <see cref="IUnknownVector"/> of
        /// <see cref="ComAttribute"/>s.
        /// </summary>
        /// <param name="document">The <see cref="ISpatialString"/> that will be used as input to the
        /// <see cref="AutoBagOfWords"/></param>
        /// <param name="protoFeaturesOrGroupsOfProtoFeatures">The <see cref="IUnknownVector"/> of
        /// <see cref="ComAttribute"/>s that will be used with the <see cref="AttributeFeatureVectorizer"/>s</param>
        /// <returns>An enumeration of feature vectors</returns>
        public IEnumerable<double[]> GetFeatureVectors(ISpatialString document,
            IUnknownVector protoFeaturesOrGroupsOfProtoFeatures)
        {
            try
            {
                ExtractException.Assert("ELI39691", "Encodings have not been computed", AreEncodingsComputed);

                if (MachineUsage == LearningMachineUsage.DocumentCategorization)
                {
                    return Enumerable.Repeat(GetDocumentFeatureVector(document, protoFeaturesOrGroupsOfProtoFeatures), 1);
                }
                else if (MachineUsage == LearningMachineUsage.Pagination)
                {
                    return GetPaginationFeatureVectors(document, protoFeaturesOrGroupsOfProtoFeatures);
                }
                else if (MachineUsage == LearningMachineUsage.AttributeCategorization)
                {
                    return GetAttributesFeatureVectors(document,
                        protoFeaturesOrGroupsOfProtoFeatures.ToIEnumerable<ComAttribute>());
                }
                else
                {
                    throw new ExtractException("ELI39537", "Unsupported LearningMachineUsage: " + MachineUsage.ToString());
                }
            }
            catch (Exception e)
            {
                throw e.AsExtract("ELI39647");
            }
        }

        /// <summary>
        /// Uses the training data to automatically configure the <see cref="AutoBagOfWords"/> and
        /// <see cref="AttributeFeatureVectorizers"/> collection so that feature vectors can be
        /// generated with this instance.
        /// </summary>
        /// <param name="ussFilePaths">The paths to the USS files to be used to configure this object</param>
        /// <param name="inputVOAFilePaths">The paths to the proto-feature VOA files to be used to
        /// configure this object</param>
        /// <param name="answersOrAnswerFiles">The predictions for each example (if <see cref="MachineUsage"/> is
        /// <see cref="LearningMachineUsage.DocumentCategorization"/>) or the paths to VOA files of predictions
        /// (if <see cref="MachineUsage"/> is <see cref="LearningMachineUsage.Pagination"/></param>
        public void ComputeEncodings(string[] ussFilePaths, string[] inputVOAFilePaths, string[] answersOrAnswerFiles)
        {
            try
            {
                ComputeEncodings(ussFilePaths, inputVOAFilePaths, answersOrAnswerFiles, _ => { }, CancellationToken.None);
            }
            catch (Exception e)
            {
                throw e.AsExtract("ELI39812");
            }
        }

        /// <summary>
        /// Uses the training data to automatically configure the <see cref="AutoBagOfWords"/> and
        /// <see cref="AttributeFeatureVectorizers"/> collection so that feature vectors can be
        /// generated with this instance.
        /// </summary>
        /// <param name="ussFilePaths">The paths to the USS files to be used to configure this object</param>
        /// <param name="inputVOAFilePaths">The paths to the proto-feature VOA files to be used to
        /// configure this object</param>
        /// <param name="answersOrAnswerFiles">The predictions for each example (if <see cref="MachineUsage"/> is
        /// <see cref="LearningMachineUsage.DocumentCategorization"/>) or the paths to VOA files of predictions
        /// (if <see cref="MachineUsage"/> is <see cref="LearningMachineUsage.Pagination"/></param>
        /// <param name="updateStatus">Function to use for sending progress updates to caller</param>
        /// <param name="cancellationToken">Token indicating that processing should be canceled</param>
        public void ComputeEncodings(string[] ussFilePaths, string[] inputVOAFilePaths, string[] answersOrAnswerFiles,
            Action<StatusArgs> updateStatus, CancellationToken cancellationToken)
        {
            try
            {
                ExtractException.Assert("ELI41833", "No USS files available to compute encodings",
                    ussFilePaths.Length > 0);

                // Null or empty VOA collection is OK. Create empty collection if null to simplify code
                if (inputVOAFilePaths == null)
                {
                    inputVOAFilePaths = new string[0];
                }

                if (ussFilePaths.Length != inputVOAFilePaths.Length && inputVOAFilePaths.Length != 0
                    || answersOrAnswerFiles != null && answersOrAnswerFiles.Length != ussFilePaths.Length)
                {
                    throw new ExtractException("ELI39538", "Arguments are of different lengths");
                }

                // Clear results of any previously computed encodings
                Clear();

                if (MachineUsage == LearningMachineUsage.DocumentCategorization)
                {
                    ComputeDocumentEncodings(ussFilePaths, inputVOAFilePaths, answersOrAnswerFiles, updateStatus, cancellationToken);
                }
                else if (MachineUsage == LearningMachineUsage.Pagination)
                {
                    ComputePaginationEncodings(ussFilePaths, inputVOAFilePaths, answersOrAnswerFiles, updateStatus, cancellationToken);
                }
                else if (MachineUsage == LearningMachineUsage.AttributeCategorization)
                {
                    ComputeAttributesEncodings(ussFilePaths, inputVOAFilePaths, updateStatus, cancellationToken);
                }
                else
                {
                    throw new ExtractException("ELI39539", "Unsupported LearningMachineUsage: " + MachineUsage.ToString());
                }
                ExtractException.Assert("ELI39693", "Unable to successfully compute encodings", AreEncodingsComputed);

                foreach(var vectorizer in AttributeFeatureVectorizers)
                {
                    vectorizer.LimitToTopTerms(AttributeVectorizerMaxDiscreteTermsFeatures);
                }

                updateStatus(new StatusArgs
                {
                    StatusMessage = "Feature vector length: {0:N0}",
                    Int32Value = FeatureVectorLength
                });
            }
            catch (Exception e)
            {
                // Clear any partially computed encodings
                try
                {
                    Clear();
                }
                catch (ExtractException e2)
                {
                    e2.Log();
                }
                throw e.AsExtract("ELI39540");
            }
        }

        /// <summary>
        /// Gets enumerations of feature vectors and answer codes for enumerations of input files
        /// </summary>
        /// <param name="ussFilePaths">The paths to the USS files to be used to generate the feature vectors</param>
        /// <param name="inputVOAFilePaths">The paths to the VOA files to be used to generate the feature vectors</param>
        /// <param name="answersOrAnswerFiles">The predictions for each example (if <see cref="MachineUsage"/> is
        /// <see cref="LearningMachineUsage.DocumentCategorization"/>) or the paths to VOA files of predictions
        /// (if <see cref="MachineUsage"/> is <see cref="LearningMachineUsage.Pagination"/></param>
        /// <param name="updateAnswerCodes">Whether to update answer code to name mappings to reflect the input</param>
        /// <returns>A tuple where the first item is an enumeration of feature vectors and the second
        /// item is an enumeration of answer codes for each example</returns>
        public Tuple<double[][], int[]> GetFeatureVectorAndAnswerCollections
            (string[] ussFilePaths, string[] inputVOAFilePaths, string[] answersOrAnswerFiles, bool updateAnswerCodes = false)
        {
            try
            {
                var triple = GetFeatureVectorAndAnswerCollections(ussFilePaths, inputVOAFilePaths,
                    answersOrAnswerFiles, _ => { }, CancellationToken.None, updateAnswerCodes);
                return Tuple.Create(triple.Item1, triple.Item2);
            }
            catch (Exception e)
            {
                throw e.AsExtract("ELI39869");
            }
        }

        /// <summary>
        /// Gets enumerations of feature vectors and answer codes for enumerations of input files
        /// </summary>
        /// <param name="ussFilePaths">The paths to the USS files to be used to generate the feature vectors</param>
        /// <param name="inputVOAFilePaths">The paths to the VOA files to be used to generate the feature vectors</param>
        /// <param name="answersOrAnswerFiles">The predictions for each example (if <see cref="MachineUsage"/> is
        /// <see cref="LearningMachineUsage.DocumentCategorization"/>) or the paths to VOA files of predictions
        /// (if <see cref="MachineUsage"/> is <see cref="LearningMachineUsage.Pagination"/></param>
        /// <param name="updateStatus">Function to use for sending progress updates to caller</param>
        /// <param name="cancellationToken">Token indicating that processing should be canceled</param>
        /// <param name="updateAnswerCodes">Whether to update answer code to name mappings to reflect the input</param>
        /// <returns>A tuple where the first item is an enumeration of feature vectors, the second
        /// item answer codes for each example and the third item the uss path for each example</returns>
        public Tuple<double[][], int[], string[]> GetFeatureVectorAndAnswerCollections
            (string[] ussFilePaths, string[] inputVOAFilePaths, string[] answersOrAnswerFiles,
                Action<StatusArgs> updateStatus, CancellationToken cancellationToken, bool updateAnswerCodes)
        {
            try
            {
                updateStatus(new StatusArgs { StatusMessage = "Building feature vector and answer collections:" });

                // Indent sub-status messages
                Action<StatusArgs> updateStatus2 = args =>
                    {
                        args.Indent++;
                        updateStatus(args);
                    };

                ExtractException.Assert("ELI40261", "Encodings have not been computed", AreEncodingsComputed);

                // Null or empty VOA collection is OK. Set to null to simplify code
                if (inputVOAFilePaths != null && inputVOAFilePaths.Length == 0)
                {
                    inputVOAFilePaths = null;
                }

                if ( inputVOAFilePaths != null && inputVOAFilePaths.Length != ussFilePaths.Length
                    || answersOrAnswerFiles != null && answersOrAnswerFiles.Length != ussFilePaths.Length)
                {
                    throw new ExtractException("ELI39700", "Arguments are of different lengths");
                }

                if (MachineUsage == LearningMachineUsage.DocumentCategorization)
                {
                    return GetDocumentFeatureVectorAndAnswerCollection(ussFilePaths, inputVOAFilePaths,
                        answersOrAnswerFiles, updateStatus2, cancellationToken, updateAnswerCodes);
                }
                else if (MachineUsage == LearningMachineUsage.Pagination)
                {
                    var results = GetPaginationFeatureVectorAndAnswerCollection(ussFilePaths, inputVOAFilePaths,
                        answersOrAnswerFiles, updateStatus2, cancellationToken);

                    double[][] featureVectors = new double[results.Length][];
                    int[] answers = new int[results.Length];
                    string[] ussFilePaths2 = new string[results.Length];
                    for (int i = 0; i < results.Length; i++)
                    {
                        featureVectors[i] = results[i].Item1;
                        answers[i] = results[i].Item2;
                        ussFilePaths2[i] = results[i].Item3;
                    }
                    return Tuple.Create(featureVectors, answers, ussFilePaths2);
                }
                else if (MachineUsage == LearningMachineUsage.AttributeCategorization)
                {
                    var results = GetAttributesFeatureVectorAndAnswerCollection(ussFilePaths, inputVOAFilePaths,
                        updateStatus2, cancellationToken);

                    ExtractException.Assert("ELI41417", "No labeled candidate attributes found",
                       results.Length > 0);

                    double[][] featureVectors = new double[results.Length][];
                    int[] answers = new int[results.Length];
                    string[] ussFilePaths2 = new string[results.Length];
                    for (int i = 0; i < results.Length; i++)
                    {
                        featureVectors[i] = results[i].Item1;
                        answers[i] = results[i].Item2;
                        ussFilePaths2[i] = results[i].Item3;
                    }
                    return Tuple.Create(featureVectors, answers, ussFilePaths2);
                }
                else
                {
                    throw new ExtractException("ELI40372", "Unsupported LearningMachineUsage: " + MachineUsage.ToString());
                }
            }
            catch (Exception e)
            {
                throw e.AsExtract("ELI39544");
            }
        }

        /// <summary>
        /// Whether this instance is configured the same as another
        /// </summary>
        /// <param name="other">The <see cref="LearningMachineDataEncoder"/> to compare with</param>
        /// <returns><see langword="true"/> if the configurations are the same, else <see langword="false"/></returns>
        public bool IsConfigurationEqualTo(LearningMachineDataEncoder other)
        {
            try
            {
                if (Object.ReferenceEquals(this, other))
                {
                    return true;
                }

                if (other == null
                    || other.AttributeFilter != AttributeFilter
                    || (other.AutoBagOfWords == null) != (AutoBagOfWords == null)
                    || other.AutoBagOfWords != null && !other.AutoBagOfWords.IsConfigurationEqualTo(AutoBagOfWords)
                    || other.MachineUsage != MachineUsage
                    || other.NegateFilter != NegateFilter
                    || other.AttributeVectorizerMaxDiscreteTermsFeatures != AttributeVectorizerMaxDiscreteTermsFeatures
                    || other.AttributesToTokenizeFilter != AttributesToTokenizeFilter
                    || other.AttributeVectorizerShingleSize != AttributeVectorizerShingleSize
                    || other.NegativeClassName != NegativeClassName
                    || other.AreEncodingsComputed && AreEncodingsComputed &&
                        (  !other.AttributeFeatureVectorizers.SequenceEqual(AttributeFeatureVectorizers)
                        || other.AutoBagOfWords != null && !other.AutoBagOfWords.Equals(AutoBagOfWords)
                        )
                   )
                {
                    return false;
                }

                return true;
            }
            catch (Exception e)
            {
                throw e.AsExtract("ELI39824");
            }
        }

        /// <summary>
        /// Creates an instance of <see cref="LearningMachineDataEncoder"/> that is a shallow clone of this instance
        /// </summary>
        public LearningMachineDataEncoder ShallowClone()
        {
            try
            {
                return (LearningMachineDataEncoder)MemberwiseClone();
            }
            catch (Exception e)
            {
                throw e.AsExtract("ELI39834");
            }
        }

        /// <summary>
        /// Creates an instance of <see cref="LearningMachineDataEncoder"/> that is a deep clone of this instance
        /// </summary>
        public LearningMachineDataEncoder DeepClone()
        {
            try
            {
                var clone = ShallowClone();
                clone.AutoBagOfWords = AutoBagOfWords == null ? null : AutoBagOfWords.DeepClone();
                clone.AnswerCodeToName = new Dictionary<int, string>(AnswerCodeToName);
                clone.AnswerNameToCode = new Dictionary<string, int>(AnswerNameToCode, StringComparer.OrdinalIgnoreCase);
                clone.AttributeFeatureVectorizers = AttributeFeatureVectorizers.Select(afv => afv.DeepClone()).ToList();
                return clone;
            }
            catch (Exception e)
            {
                throw e.AsExtract("ELI39835");
            }
        }

        /// <summary>
        /// Clear computed values. After this call, <see cref="AreEncodingsComputed"/>==<see langword="false"/>
        /// </summary>
        public void Clear()
        {
            try
            {
                // Clear name to code mappings
                AnswerCodeToName.Clear();
                AnswerNameToCode.Clear();

                // Clear vectorizer collection
                AttributeFeatureVectorizers = Enumerable.Empty<AttributeFeatureVectorizer>();

                if (AutoBagOfWords != null)
                {
                    AutoBagOfWords.Clear();
                }
            }
            catch (Exception e)
            {
                throw e.AsExtract("ELI39877");
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
                writer.WriteLine("AutoBagOfWordsEnabled: {0}", AutoBagOfWords != null && AutoBagOfWords.Enabled);
                if (string.IsNullOrWhiteSpace(AttributeFilter))
                {
                    writer.WriteLine("AttributeFilter: None");
                }
                else
                {
                    writer.WriteLine("AttributeFilter: {0} {1}", NegateFilter ? "Not Matching" : "Matching",
                        AttributeFilter);
                }
                if (AreEncodingsComputed)
                {
                    writer.WriteLine("FeatureVectorLength: {0:N0}", FeatureVectorLength);
                }
                writer.WriteLine("NegativeClassName: {0}", NegativeClassName);
                writer.Indent = oldIndent;
            }
            catch (Exception e)
            {
                throw e.AsExtract("ELI40067");
            }
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// Gets an enumeration of <see cref="ComAttribute"/>s for document categorization input from a VOA file
        /// </summary>
        /// <param name="attributesFilePath">The path to the VOA or EAV file</param>
        /// <returns>An enumeration of <see cref="ComAttribute"/>s for document categorization input.</returns>
        private NameToProtoFeaturesMap GetDocumentProtoFeatures(string attributesFilePath)
        {
            try
            {
                if (!File.Exists(attributesFilePath))
                {
                    var ue = new ExtractException("ELI44904", "Attributes file is missing");
                    ue.AddDebugData("Attributes file path", attributesFilePath, false);
                    ue.Log();
                    return new NameToProtoFeaturesMap();
                }
                var attributes = _afUtility.Value.GetAttributesFromFile(attributesFilePath);
                attributes.ReportMemoryUsage();
                return GetFilteredMapOfNamesToValues(attributes);
            }
            catch (Exception e)
            {
                var ue = e.AsExtract("ELI39542");
                ue.AddDebugData("Attributes file path", attributesFilePath, false);
                throw ue;
            }
        }

        /// <summary>
        /// Gets an enumeration of <see cref="NameToProtoFeaturesMap"/>s for pagination input from a vector of
        /// <see cref="ComAttribute"/>s
        /// </summary>
        /// <param name="attributes">The vector of <see cref="ComAttribute"/>s from which to get protofeatures</param>
        /// <returns>An enumeration of <see cref="NameToProtoFeaturesMap"/>s for pagination input</returns>
        private IEnumerable<NameToProtoFeaturesMap> GetPaginationProtoFeatures(IUnknownVector attributes)
        {
            var pages = attributes
                .ToIEnumerable<ComAttribute>()
                .Where(a => a.Name.Equals(PageAttributeName, StringComparison.OrdinalIgnoreCase));

            return pages.Skip(1).Select(pageAttr => GetFilteredMapOfNamesToValues(pageAttr.SubAttributes));
        }

        /// <summary>
        /// Gets an enumeration of <see cref="NameToProtoFeaturesMap"/>s for pagination input from a VOA file
        /// </summary>
        /// <param name="attributesFilePath">The path to the VOA or EAV file</param>
        /// <returns>An enumeration of <see cref="NameToProtoFeaturesMap"/>s for pagination input</returns>
        private IEnumerable<NameToProtoFeaturesMap> GetPaginationProtoFeatures(string attributesFilePath)
        {
            try
            {
                var attributes = _afUtility.Value.GetAttributesFromFile(attributesFilePath);
                attributes.ReportMemoryUsage();
                return GetPaginationProtoFeatures(attributes);
            }
            catch (Exception e)
            {
                var ue = e.AsExtract("ELI39541");
                ue.AddDebugData("Attributes file path", attributesFilePath, false);
                throw ue;
            }
        }

        /// <summary>
        /// Gets an enumeration of <see cref="NameToProtoFeaturesMap"/>s for attribute categorization from a vector of
        /// <see cref="ComAttribute"/>s
        /// </summary>
        /// <param name="attributes">The collection of <see cref="ComAttribute"/>s from which to get protofeatures</param>
        /// <returns>An enumeration of <see cref="NameToProtoFeaturesMap"/>s for attribute categorization</returns>
        private IEnumerable<NameToProtoFeaturesMap> GetAttributesProtoFeatures(IEnumerable<ComAttribute> attributes)
        {
            return attributes
                .Select(attr => GetFilteredMapOfNamesToValues(attr.SubAttributes));
        }

        /// <summary>
        /// Gets an enumeration of <see cref="Tuple{string, NameToProtoFeaturesMap}"/>s for attribute categorization
        /// from a VOA file. The string value of the tuple is the category of the attribute to which the protofeatures relate.
        /// </summary>
        /// <remarks>
        /// Only top-level attributes that are labeled (have an AttributeType subattribute) will be used.
        /// For this reason this method is only to be used during the training process. 
        /// </remarks>
        /// <param name="attributesFilePath">The path to the VOA or EAV file</param>
        /// <returns>An enumeration of <see cref="Tuple{string, NameToProtoFeaturesMap}"/>s for attribute categorization</returns>
        private IEnumerable<Tuple<string, NameToProtoFeaturesMap>>  GetAttributesProtoFeatures(string attributesFilePath)
        {
            try
            {
                var attributes = _afUtility.Value.GetAttributesFromFile(attributesFilePath);
                attributes.ReportMemoryUsage();
                var filteredAttributes = attributes
                    .ToIEnumerable<ComAttribute>()
                    .Select(attribute =>
                    {
                        string category = AttributeMethods
                            .GetAttributesByName(attribute.SubAttributes, CategoryAttributeName)
                            .FirstOrDefault()?.Value.String;
                        return Tuple.Create(category, attribute);
                    })
                    .Where(t => t.Item1 != null);

                return filteredAttributes
                    .Select(categoryAttributePair =>
                        Tuple.Create(categoryAttributePair.Item1,
                        GetFilteredMapOfNamesToValues(categoryAttributePair.Item2.SubAttributes)));
            }
            catch (Exception e)
            {
                var ue = e.AsExtract("ELI41400");
                ue.AddDebugData("Attributes file path", attributesFilePath, false);
                throw ue;
            }
        }

        /// <summary>
        /// Gets a feature vector from a <see cref="ISpatialString"/> and vector of <see cref="ComAttribute"/>s
        /// </summary>
        /// <param name="document">The <see cref="ISpatialString"/> to use for auto-bag-of-words features.</param>
        /// <param name="protoFeatures">The vector of <see cref="ComAttribute"/>s to use for attribute features.</param>
        /// <returns>The feature vector computed from the input arguments.</returns>
        private double[] GetDocumentFeatureVector(ISpatialString document, IUnknownVector protoFeatures)
        {
            var attributeFeatures = Enumerable.Empty<double>();

            if (AttributeFeatureVectorizers.Any())
            {
                if (protoFeatures != null)
                {
                    var protoFeatureMap = GetFilteredMapOfNamesToValues(protoFeatures);

                    // NOTE: vectorizers return an empty enumerable if not enabled
                    attributeFeatures = AttributeFeatureVectorizers
                        .SelectMany(vectorizer => vectorizer.GetDocumentFeatureVector(protoFeatureMap));
                }
                else
                {
                    // Use all zeros for null VOA
                    attributeFeatures = Enumerable.Repeat(0.0, AttributeFeatureVectorLength);
                }
            }

            if (AutoBagOfWords == null || !AutoBagOfWords.Enabled)
            {
                return attributeFeatures.ToArray();
            }
            else
            {
                var shingleFeatures = AutoBagOfWords.GetDocumentFeatureVector(document);
                return shingleFeatures.Concat(attributeFeatures).ToArray();
            }
        }

        /// <summary>
        /// Gets zero or more feature vectors from a <see cref="ISpatialString"/> and vector of <see cref="ComAttribute"/>s
        /// </summary>
        /// <param name="document">The <see cref="ISpatialString"/> to use for auto-bag-of-words features.</param>
        /// <param name="pagesOfProtoFeatures">The vector of <see cref="ComAttribute"/>s containing Page attributes with
        /// sub-attributes to use for attribute features.</param>
        /// <returns>The feature vectors computed from the input arguments.</returns>
        private IEnumerable<double[]> GetPaginationFeatureVectors(ISpatialString document, IUnknownVector pagesOfProtoFeatures)
        {
            var attributeFeatures = Enumerable.Empty<List<double[]>>();
            List<double[]> shingleFeatures = null;
            int numberOfExamples = 0;

            if (pagesOfProtoFeatures != null)
            {
                // At runtime (e.g., called from LearningMachineOutputHandler) handle empty VOA
                // by creating empty feature vector for each page pair rather than returning an empty
                // collection that would cause an exception
                var protoFeatureGroups = pagesOfProtoFeatures.Size() > 0
                    ? GetPaginationProtoFeatures(pagesOfProtoFeatures)
                    : SpatialStringFeatureVectorizer.GetPaginationTexts(document)
                        .Select(_ => new NameToProtoFeaturesMap());

                numberOfExamples = protoFeatureGroups.Count();

                // NOTE: vectorizers return an empty enumerable if not enabled
                attributeFeatures = AttributeFeatureVectorizers
                    .Select(vectorizer => vectorizer.GetFeatureVectorsForEachGroup(protoFeatureGroups)
                    .ToList());
            }
            else
            {
                var protoFeatureGroups = SpatialStringFeatureVectorizer.GetPaginationTexts(document).Select(_ =>
                    Enumerable.Empty<NameToProtoFeaturesMap>()
                );

            }

            if (AutoBagOfWords == null || !AutoBagOfWords.Enabled)
            {
                // Transpose from <# vectorizers> X <# page pairs> to <# page pairs> X <# vectorizers>
                for (int i = 0; i < numberOfExamples; i++)
                {
                    yield return attributeFeatures
                        .Where(v => v.Any()) // Since vectorizers return an empty enumerable if not enabled
                        .SelectMany(v => v[i])
                        .ToArray();
                }
            }
            else
            {
                shingleFeatures = AutoBagOfWords.GetPaginationFeatureVectors(document).ToList();
                numberOfExamples = shingleFeatures.Count;
                for (int i = 0; i < numberOfExamples; i++)
                {
                    yield return shingleFeatures[i].Concat(attributeFeatures
                        .Where(v => v.Any()) // Since vectorizers return an empty enumerable if not enabled
                        .SelectMany(v => v[i]))
                        .ToArray();
                }
            }
        }

        /// <summary>
        /// Gets zero or more feature vectors from a <see cref="ISpatialString"/> and vector of <see cref="ComAttribute"/>s
        /// </summary>
        /// <param name="document">The <see cref="ISpatialString"/> to use for auto-bag-of-words features.</param>
        /// <param name="attributes">The collection of <see cref="ComAttribute"/>s containing attributes to be classified
        /// <returns>The feature vectors computed from the input arguments.</returns>
        private IEnumerable<double[]> GetAttributesFeatureVectors(ISpatialString document, IEnumerable<ComAttribute> attributes)
        {
            ExtractException.Assert("ELI41405", "Attributes to be classified cannot be null", attributes != null);

            var attributeFeatures = Enumerable.Empty<List<double[]>>();
            int numberOfExamples = attributes.Count();

            var protoFeatureGroups = GetAttributesProtoFeatures(attributes);

            // NOTE: vectorizers return an empty enumerable if not enabled
            attributeFeatures = AttributeFeatureVectorizers
                .Select(vectorizer => vectorizer.GetFeatureVectorsForEachGroup(protoFeatureGroups).ToList());

            if (AutoBagOfWords == null || !AutoBagOfWords.Enabled)
            {
                // Transpose from <# vectorizers> X <# candidate attributes> to <# candidate attributes> X <# vectorizers>
                for (int i = 0; i < numberOfExamples; i++)
                {
                    yield return attributeFeatures
                        .Where(v => v.Any()) // Since vectorizers return an empty enumerable if not enabled
                        .SelectMany(v => v[i])
                        .ToArray();
                }
            }
            else
            {
                var shingleFeatures = AutoBagOfWords.GetDocumentFeatureVector(document);
                for (int i = 0; i < numberOfExamples; i++)
                {
                    yield return shingleFeatures.Concat(attributeFeatures
                        .Where(v => v.Any()) // Since vectorizers return an empty enumerable if not enabled
                        .SelectMany(v => v[i]))
                        .ToArray();
                }
            }
        }

        /// <summary>
        /// Computes feature vectors and answer codes. Assumes that this
        /// object has been configured with <see cref="ComputeDocumentEncodings"/>
        /// </summary>
        /// <param name="ussFilePaths">The uss paths of each input file</param>
        /// <param name="inputVOAFilePaths">The input VOA paths corresponding to each uss file</param>
        /// <param name="answers">The categories for each input file</param>
        /// <param name="updateStatus">Function to use for sending progress updates to caller</param>
        /// <param name="cancellationToken">Token indicating that processing should be canceled</param>
        /// <param name="updateAnswerCodes">Whether to update answer code to name mappings to reflect the input</param>
        /// <returns>A tuple of feature vectors, predictions and the uss path for each example</returns>
        private Tuple<double[][], int[], string[]> GetDocumentFeatureVectorAndAnswerCollection
            (string[] ussFilePaths, string[] inputVOAFilePaths, string[] answers,
                Action<StatusArgs> updateStatus, CancellationToken cancellationToken, bool updateAnswerCodes)
        {
            try
            {
                // Initialize answer code mappings if updating answers (true if training the classifier)
                if (updateAnswerCodes)
                {
                    InitializeAnswerCodeMappings(answers, NegativeClassName);
                }

                double[][] featureVectors = new double[ussFilePaths.Length][];
                int[] answerCodes = new int[ussFilePaths.Length];
                Parallel.For(0, ussFilePaths.Length, (i, loopState) =>
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        loopState.Stop();
                    }

                    string answer = answers[i];

                    SpatialString spatialString = null;
                    if (AutoBagOfWords != null)
                    {
                        string uss = ussFilePaths[i];
                        try
                        {
                            spatialString = new SpatialStringClass();
                            spatialString.LoadFrom(uss, false);
                            spatialString.ReportMemoryUsage();
                        }
                        catch (Exception e)
                        {
                            var ue = e.AsExtract("ELI39654");
                            ue.AddDebugData("USS path", uss, false);
                            throw ue;
                        }
                    }

                    IUnknownVector attributes = null;
                    if (AttributeFeatureVectorizers.Any() && inputVOAFilePaths != null)
                    {
                        string voa = inputVOAFilePaths[i];
                        if (!File.Exists(voa))
                        {
                            var ue = new ExtractException("ELI39655", "Attributes file is missing");
                            ue.AddDebugData("Attributes file path", voa, false);
                            ue.Log();
                        }
                        else
                        {
                            attributes = _afUtility.Value.GetAttributesFromFile(voa);
                            attributes.ReportMemoryUsage();
                        }
                    }

                    double[] featureVector = GetDocumentFeatureVector(spatialString, attributes);

                    int answerCode;
                    if (!AnswerNameToCode.TryGetValue(answer, out answerCode))
                    {
                        answerCode = UnknownOrNegativeCategoryCode;
                    }

                    featureVectors[i] = featureVector;
                    answerCodes[i] = answerCode;

                    updateStatus(new StatusArgs { StatusMessage = "Files processed: {0:N0}", Int32Value = 1 });
                });

                cancellationToken.ThrowIfCancellationRequested();

                return Tuple.Create(featureVectors, answerCodes, ussFilePaths);
            }
            catch (Exception e)
            {
                throw e.AsExtract("ELI39705");
            }
        }

        /// <summary>
        /// Computes answer code to name mappings
        /// </summary>
        /// <param name="answers">The answer names/categories. Can contain repeats</param>
        /// <param name="negativeCategory">The value to be used for the negative category if there
        /// are only two categories total</param>
        private void InitializeAnswerCodeMappings(IEnumerable<string> answers, string negativeCategory = null)
        {
            AnswerCodeToName.Clear();
            AnswerNameToCode.Clear();

            // Add the negative category or an 'other' category
            string otherCategory = negativeCategory ?? UnknownCategoryName;

            AnswerCodeToName.Add(UnknownOrNegativeCategoryCode, otherCategory);
            AnswerNameToCode.Add(otherCategory, UnknownOrNegativeCategoryCode);

            // Add category code for each name seen
            int nextCategoryCode = 0;
            foreach (var category in answers.Distinct()
                .Where(k => !k.Equals(otherCategory, StringComparison.OrdinalIgnoreCase))
                .OrderBy(s => s, StringComparer.OrdinalIgnoreCase))
            {
                while (AnswerCodeToName.ContainsKey(nextCategoryCode))
                {
                    nextCategoryCode++;
                }
                AnswerCodeToName.Add(nextCategoryCode, category);
                AnswerNameToCode.Add(category, nextCategoryCode);
            }

            ExtractException.Assert("ELI40251", "There must be at least two categories of input", AnswerNameToCode.Count > 1);
        }

        /// <summary>
        /// Builds a collection of feature vector and answer code tuples. Assumes that this
        /// object has been configured with <see cref="ComputeDocumentEncodings"/>
        /// </summary>
        /// <param name="ussFilePaths">The uss paths of each input file</param>
        /// <param name="inputVOAFilePaths">The input VOA paths corresponding to each uss file</param>
        /// <param name="answerFiles">The VOAs with pagination boundary info for each input file</param>
        /// <param name="updateStatus">Function to use for sending progress updates to caller</param>
        /// <param name="cancellationToken">Token indicating that processing should be canceled</param>
        /// <returns>An array of feature vector, answer and uss path tuples</returns>
        private Tuple<double[], int, string>[] GetPaginationFeatureVectorAndAnswerCollection
            (string[] ussFilePaths, string[] inputVOAFilePaths, string[] answerFiles,
                Action<StatusArgs> updateStatus,
                CancellationToken cancellationToken)
        {
            try
            {
                var results = new Tuple<double[], int, string>[ussFilePaths.Length][];
                Parallel.For(0, ussFilePaths.Length, (i, loopState) =>
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        loopState.Stop();
                    }

                    string answerFile = answerFiles[i];

                    SpatialString spatialString = null;
                    if (AutoBagOfWords != null)
                    {
                        string uss = ussFilePaths[i];
                        try
                        {
                            spatialString = new SpatialStringClass();
                            spatialString.LoadFrom(uss, false);
                            spatialString.ReportMemoryUsage();
                        }
                        catch (Exception e)
                        {
                            var ue = e.AsExtract("ELI39689");
                            ue.AddDebugData("USS path", uss, false);
                            throw ue;
                        }
                    }

                    IUnknownVector attributes = null;
                    if (AttributeFeatureVectorizers.Any() && inputVOAFilePaths != null)
                    {
                        string voa = inputVOAFilePaths[i];
                        ExtractException.Assert("ELI39690", "Input VOA file doesn't exist", File.Exists(voa),
                            "Filename", voa);
                        attributes = _afUtility.Value.GetAttributesFromFile(voa);
                        attributes.ReportMemoryUsage();
                    }

                    List<double[]> featureVectors = GetPaginationFeatureVectors(spatialString, attributes).ToList();

                    // Get page count so that missing page numbers in the answer VOA can be filled in.
                    int pageCount = featureVectors.Count + 1;
                    var expandedAnswers = ExpandPaginationAnswerVOA(answerFile, pageCount).ToList();

                    var answerCodes = expandedAnswers.Select(answer =>
                    {
                        int code;
                        if (!AnswerNameToCode.TryGetValue(answer, out code))
                        {
                            var ex = new ExtractException("ELI39627", "Internal logic error: unknown category");
                            ex.AddDebugData("Category name", answer, false);
                            throw ex;
                        }
                        return code;
                    }).ToList();

                    results[i] = new Tuple<double[], int, string>[featureVectors.Count];
                    for (int j = 0; j < featureVectors.Count; j++)
                    {
                        results[i][j] = (Tuple.Create(featureVectors[j], answerCodes[j], ussFilePaths[i]));
                    }

                    updateStatus(new StatusArgs { StatusMessage = "Files processed: {0:N0}", Int32Value = 1 });
                });

                cancellationToken.ThrowIfCancellationRequested();
                return results.SelectMany(a => a).ToArray();
            }
            catch (Exception e)
            {
                throw e.AsExtract("ELI39713");
            }
        }

        /// <summary>
        /// Builds a collection of feature vector and answer code tuples. Assumes that this
        /// object has been configured with <see cref="ComputeAttributesEncodings"/>
        /// </summary>
        /// <param name="ussFilePaths">The uss paths of each input file</param>
        /// <param name="inputVOAFilePaths">The input VOA paths corresponding to each uss file</param>
        /// <param name="updateStatus">Function to use for sending progress updates to caller</param>
        /// <param name="cancellationToken">Token indicating that processing should be canceled</param>
        /// <returns>An array of feature vector, answer and uss path tuples</returns>
        private Tuple<double[], int, string>[] GetAttributesFeatureVectorAndAnswerCollection
            (string[] ussFilePaths, string[] inputVOAFilePaths,
                Action<StatusArgs> updateStatus,
                CancellationToken cancellationToken)
        {
            try
            {
                ExtractException.Assert("ELI41413", "Input VOA collection cannot be null",
                    inputVOAFilePaths != null);

                var results = new Tuple<double[], int, string>[ussFilePaths.Length][];
                Parallel.For(0, ussFilePaths.Length, (i, loopState) =>
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        loopState.Stop();
                    }

                    SpatialString spatialString = null;
                    if (AutoBagOfWords != null)
                    {
                        string uss = ussFilePaths[i];
                        try
                        {
                            spatialString = new SpatialStringClass();
                            spatialString.LoadFrom(uss, false);
                            spatialString.ReportMemoryUsage();
                        }
                        catch (Exception e)
                        {
                            var ue = e.AsExtract("ELI41401");
                            ue.AddDebugData("USS path", uss, false);
                            throw ue;
                        }
                    }

                    string voa = inputVOAFilePaths[i];
                    ExtractException.Assert("ELI41402", "Input VOA file doesn't exist", File.Exists(voa),
                        "Filename", voa);
                    var attributes = _afUtility.Value.GetAttributesFromFile(voa);
                    attributes.ReportMemoryUsage();

                    var answerCodes = new List<int>(attributes.Size());
                    var filteredAttributes = new List<ComAttribute>(attributes.Size());
                    foreach(var attribute in attributes.ToIEnumerable<ComAttribute>())
                    {
                        var categoryAttributes =
                            AttributeMethods.GetAttributesByName(attribute.SubAttributes, CategoryAttributeName);

                        int countOfCategoryAttributes = categoryAttributes.Count();
                        ExtractException.Assert("ELI41412", "There should be zero or one category attribute",
                            countOfCategoryAttributes <= 1,
                            "VOA file", inputVOAFilePaths[i],
                            "Candidate attribute name", attribute.Name,
                            "Category attribute name", CategoryAttributeName,
                            "Count of category attributes", countOfCategoryAttributes);

                        // Not a candidate attribute
                        if (countOfCategoryAttributes == 0)
                        {
                            continue;
                        }

                        string categoryName = categoryAttributes.First().Value.String;
                        int code;
                        if (!AnswerNameToCode.TryGetValue(categoryName, out code))
                        {
                            var ex = new ExtractException("ELI41403",
                                "Unknown attribute category/label encountered, treating as if unlabeled (as if empty type)");
                            ex.AddDebugData("Unknown category name", categoryName, false);
                            ex.Log();
                            code = UnknownOrNegativeCategoryCode;
                        }
                        filteredAttributes.Add(attribute);
                        answerCodes.Add(code);
                    }

                    List<double[]> featureVectors = GetAttributesFeatureVectors(spatialString, filteredAttributes).ToList();

                    results[i] = new Tuple<double[], int, string>[featureVectors.Count];
                    for (int j = 0; j < featureVectors.Count; j++)
                    {
                        results[i][j] = (Tuple.Create(featureVectors[j], answerCodes[j], ussFilePaths[i]));
                    }

                    updateStatus(new StatusArgs { StatusMessage = "Files processed: {0:N0}", Int32Value = 1 });
                });

                cancellationToken.ThrowIfCancellationRequested();
                return results.SelectMany(a => a).ToArray();
            }
            catch (Exception e)
            {
                throw e.AsExtract("ELI41404");
            }
        }

        /// <summary>
        /// Uses the training data to automatically configure the <see cref="AutoBagOfWords"/> and
        /// <see cref="AttributeFeatureVectorizers"/> collection so that feature vectors can be
        /// generated with this instance.
        /// </summary>
        /// <param name="ussFilePaths">The paths to the USS files to be used to configure this object</param>
        /// <param name="inputVOAFilePaths">The paths to the proto-feature VOA files to be used to
        /// configure this object</param>
        /// <param name="answers">The predictions (categories) for each example</param>
        /// <param name="updateStatus">Function to use for sending progress updates to caller</param>
        /// <param name="cancellationToken">Token indicating that processing should be canceled</param>
        private void ComputeDocumentEncodings(string[] ussFilePaths, string[] inputVOAFilePaths, string[] answers,
            Action<StatusArgs> updateStatus, System.Threading.CancellationToken cancellationToken)
        {
            // Indent sub-status messages
            Action<StatusArgs> updateStatus2 = args =>
                {
                    args.Indent++;
                    updateStatus(args);
                };

            // Configure SpatialStringFeatureVectorizer
            if (AutoBagOfWords != null)
            {
                updateStatus(new StatusArgs { StatusMessage = "Computing auto-bag-of-words encodings:" });
                AutoBagOfWords.ComputeEncodingsFromDocumentTrainingData(ussFilePaths, answers, updateStatus2, cancellationToken);
            }

            if (inputVOAFilePaths.Length > 0)
            {
                updateStatus(new StatusArgs { StatusMessage = "Computing attribute feature encodings:" });
            }

            // Configure AttributeFeatureVectorizer collection
            var protoFeatures = inputVOAFilePaths.Select(path => new { path, protoFeatures = GetDocumentProtoFeatures(path) });

            Dictionary<string, AttributeFeatureVectorizer> vectorizerMap
                = new Dictionary<string, AttributeFeatureVectorizer>(StringComparer.OrdinalIgnoreCase);

            foreach (var labeledExample in protoFeatures.Zip(answers,
                (example, answer) => new { example.path, answer, example.protoFeatures }))
            {
                cancellationToken.ThrowIfCancellationRequested();
                updateStatus2(new StatusArgs { StatusMessage = "Documents processed: {0:N0}", Int32Value = 1 });

                foreach (var group in labeledExample.protoFeatures)
                {
                    string name = group.Key;
                    var vectorizer = vectorizerMap.GetOrAdd(name, k => new AttributeFeatureVectorizer(k));
                    vectorizer.ComputeEncodingsFromTrainingData(
                        protoFeatures: group.Value,
                        category: labeledExample.answer,
                        docName: labeledExample.path);
                }
            }
            AttributeFeatureVectorizers = vectorizerMap.Values;

            // Add category names and codes
            InitializeAnswerCodeMappings(answers, NegativeClassName);
        }

        /// <summary>
        /// Uses the training data to automatically configure the <see cref="AutoBagOfWords"/> and
        /// <see cref="AttributeFeatureVectorizers"/> collection so that feature vectors can be
        /// generated with this instance.
        /// </summary>
        /// <param name="ussFilePaths">The paths to the USS files to be used to configure this object</param>
        /// <param name="inputVOAFilePaths">The paths to the proto-feature VOA files to be used to
        /// configure this object</param>
        /// <param name="answerFiles">The paths to VOA files of predictions</param>
        /// <param name="updateStatus">Function to use for sending progress updates to caller</param>
        /// <param name="cancellationToken">Token indicating that processing should be canceled</param>
        private void ComputePaginationEncodings(string[] ussFilePaths, string[] inputVOAFilePaths, string[] answerFiles,
            Action<StatusArgs> updateStatus, System.Threading.CancellationToken cancellationToken)
        {
            // Indent sub-status messages
            Action<StatusArgs> updateStatus2 = args =>
                {
                    args.Indent++;
                    updateStatus(args);
                };
            // Configure SpatialStringFeatureVectorizer
            if (AutoBagOfWords != null)
            {
                updateStatus(new StatusArgs { StatusMessage = "Computing auto-bag-of-words encodings:" });
                AutoBagOfWords.ComputeEncodingsFromPaginationTrainingData(ussFilePaths, answerFiles, updateStatus2, cancellationToken);
            }

            if (inputVOAFilePaths.Length > 0)
            {
                updateStatus(new StatusArgs { StatusMessage = "Computing attribute feature encodings:" });
            }

            // Configure AttributeFeatureVectorizer collection
            IEnumerable<IEnumerable<NameToProtoFeaturesMap>> pagePairProtofeatureCollection =
                inputVOAFilePaths.Select(GetPaginationProtoFeatures);

            // Pass the page count of each image along so that missing pages in the answer VOA can be filled in
            var pagePairProtofeaturesAndCategories = pagePairProtofeatureCollection.Zip(answerFiles, (pagePairs, answerFile) =>
                {
                    var answers = ExpandPaginationAnswerVOA(answerFile, pagePairs.Count() + 1);
                    return pagePairs.Zip(answers, (pagePairProtofeatures, answer) =>
                        new { answer, pagePairProtofeatures });
                })
                .SelectMany(answersForFile => answersForFile);

            Dictionary<string, AttributeFeatureVectorizer> vectorizerMap
                = new Dictionary<string, AttributeFeatureVectorizer>(StringComparer.OrdinalIgnoreCase);

            // Count each page pair as a separate document for purposes of TF*IDF score
            int exampleNumber = 0;
            foreach (var labeledExample in pagePairProtofeaturesAndCategories)
            {
                ++exampleNumber;
                cancellationToken.ThrowIfCancellationRequested();

                foreach (var group in labeledExample.pagePairProtofeatures)
                {
                    string name = group.Key;
                    var vectorizer = vectorizerMap.GetOrAdd(name, k => new AttributeFeatureVectorizer(k));
                    vectorizer.ComputeEncodingsFromTrainingData(
                        protoFeatures: group.Value,
                        category: labeledExample.answer,
                        docName: exampleNumber.ToString(CultureInfo.InvariantCulture));
                }

                updateStatus2(new StatusArgs { StatusMessage = "Pages processed: {0:N0}", Int32Value = 1 });
            }
            AttributeFeatureVectorizers = vectorizerMap.Values;

            // Add category names and codes
            AnswerCodeToName.Add(_NOT_FIRST_PAGE_CATEGORY_CODE, NotFirstPageCategory);
            AnswerNameToCode.Add(NotFirstPageCategory, _NOT_FIRST_PAGE_CATEGORY_CODE);
            AnswerCodeToName.Add(FirstPageCategoryCode, _FIRST_PAGE_CATEGORY);
            AnswerNameToCode.Add(_FIRST_PAGE_CATEGORY, FirstPageCategoryCode);
        }

        /// <summary>
        /// Uses the training data to automatically configure the <see cref="AutoBagOfWords"/> and
        /// <see cref="AttributeFeatureVectorizers"/> collection so that feature vectors can be
        /// generated with this instance.
        /// </summary>
        /// <param name="ussFilePaths">The paths to the USS files to be used to configure this object</param>
        /// <param name="labeledCandidateAttributesFiles">The paths to the candidate VOA files to be used to
        /// configure this object</param>
        /// <param name="updateStatus">Function to use for sending progress updates to caller</param>
        /// <param name="cancellationToken">Token indicating that processing should be canceled</param>
        private void ComputeAttributesEncodings(string[] ussFilePaths, string[] labeledCandidateAttributesFiles,
            Action<StatusArgs> updateStatus, CancellationToken cancellationToken)
        {
            ExtractException.Assert("ELI41408", "Candidate attribute collection cannot be empty",
                labeledCandidateAttributesFiles.Length > 0);

            // Indent sub-status messages
            Action<StatusArgs> updateStatus2 = args =>
                {
                    args.Indent++;
                    updateStatus(args);
                };

            // Configure SpatialStringFeatureVectorizer
            List<string> answers = null;
            if (AutoBagOfWords != null)
            {
                updateStatus(new StatusArgs { StatusMessage = "Computing auto-bag-of-words encodings:" });
                answers = AutoBagOfWords.ComputeEncodingsFromAttributesTrainingData
                    (ussFilePaths, labeledCandidateAttributesFiles, updateStatus2, cancellationToken)
                    .ToList();
            }
            else
            {
                updateStatus(new StatusArgs { StatusMessage = "Collecting labels from VOAs:" });
                answers = labeledCandidateAttributesFiles
                    .SelectMany(voa =>
                    {
                        var labels = CollectLabelsFromLabeledCandidateAttributesFile(voa);
                        updateStatus(new StatusArgs { StatusMessage = "Files processed: {0:N0}", Int32Value = 1, Indent = 1 });
                        return labels;
                    })
                    .Distinct(StringComparer.OrdinalIgnoreCase) // https://extract.atlassian.net/browse/ISSUE-14761
                    .ToList();
            }

            if (labeledCandidateAttributesFiles.Length > 0)
            {
                updateStatus(new StatusArgs { StatusMessage = "Computing attribute feature encodings:" });
            }

            // Configure AttributeFeatureVectorizer collection
            IEnumerable<Tuple<string, NameToProtoFeaturesMap>> categoryAndProtoFeaturesCollection =
                labeledCandidateAttributesFiles.SelectMany(GetAttributesProtoFeatures);

            Dictionary<string, AttributeFeatureVectorizer> vectorizerMap
                = new Dictionary<string, AttributeFeatureVectorizer>(StringComparer.OrdinalIgnoreCase);

            // Count each attribute as a separate document for purposes of TF*IDF score
            int exampleNumber = 0;
            foreach (var labeledExample in categoryAndProtoFeaturesCollection)
            {
                cancellationToken.ThrowIfCancellationRequested();

                ++exampleNumber;
                var category = labeledExample.Item1;
                var example = labeledExample.Item2;

                foreach (var group in example)
                {
                    string name = group.Key;
                    var vectorizer = vectorizerMap.GetOrAdd(name, k => new AttributeFeatureVectorizer(k));
                    vectorizer.ComputeEncodingsFromTrainingData(
                        protoFeatures: group.Value,
                        category: category,
                        docName: exampleNumber.ToString(CultureInfo.InvariantCulture));
                }
                updateStatus2(new StatusArgs { StatusMessage = "Attributes processed: {0:N0}", Int32Value = 1 });
            }
            AttributeFeatureVectorizers = vectorizerMap.Values;

            // Add category names and codes
            InitializeAnswerCodeMappings(answers, NegativeClassName);
        }

        /// <summary>
        /// Collects the labels (types) from a candidate attributes file
        /// </summary>
        /// <param name="attributesFilePath">The attributes file path</param>
        /// <returns>A collection of labels for each candidate attribute</returns>
        static internal IEnumerable<string> CollectLabelsFromLabeledCandidateAttributesFile(string attributesFilePath)
        {
            try
            {
                var attributes = _afUtility.Value.GetAttributesFromFile(attributesFilePath);
                attributes.ReportMemoryUsage();
                return attributes
                    .ToIEnumerable<ComAttribute>()
                    .Select(attr =>
                    {
                        var categoryAttributes =
                            AttributeMethods.GetAttributesByName(attr.SubAttributes, CategoryAttributeName);

                        int countOfCategoryAttributes = categoryAttributes.Count();
                        ExtractException.Assert("ELI41416", "There should be zero or one category attribute",
                            countOfCategoryAttributes <= 1,
                            "VOA file", attributesFilePath,
                            "Candidate attribute name", attr.Name,
                            "Category attribute name", CategoryAttributeName,
                            "Count of category attributes", countOfCategoryAttributes);

                        // Not a candidate attribute
                        if (countOfCategoryAttributes == 0)
                        {
                            return null;
                        }

                        return categoryAttributes.First().Value.String;
                    })
                    .Where(a => a != null);
            }
            catch (Exception e)
            {
                var ue = e.AsExtract("ELI41399");
                ue.AddDebugData("Attributes file path", attributesFilePath, false);
                throw ue;
            }
        }

        /// <summary>
        /// Filter the <see paramref="protoFeatures"/> so that only the attributes that are selected
        /// (or not selected if <see cref="NegateFilter"/> is <see langword="true"/>)
        /// by the <see cref="AttributeFilter"/> AFQuery are returned.
        /// </summary>
        /// <param name="protoFeatures">The vector of <see cref="ComAttribute"/>s to filter</param>
        /// <returns>A vector of the <see cref="ComAttribute"/>s remaining after filtering</returns>
        private IEnumerable<ComAttribute> FilterProtoFeatures(IUnknownVector protoFeatures)
        {
            // If no filter or protoFeatures is empty then return unchanged
            if (string.IsNullOrWhiteSpace(AttributeFilter) || protoFeatures.Size() == 0)
            {
                return protoFeatures.ToIEnumerable<ComAttribute>();
            }

            var matching = _afUtility.Value.QueryAttributes(protoFeatures, AttributeFilter, false)
                    .ToIEnumerable<ComAttribute>()
                    .Distinct();

            // If negating filter, clone the vector and remove matching
            if (NegateFilter)
            {
                return protoFeatures.ToIEnumerable<ComAttribute>().Except(matching);
            }

            return matching;
        }

        /// <summary>
        /// Filter the <see paramref="protoFeatures"/> and build a <see cref="NameToProtoFeaturesMap"/>
        /// </summary>
        /// <param name="protoFeatures">The vector of <see cref="ComAttribute"/>s to be used to build the filtered map</param>
        /// <returns>A <see cref="NameToProtoFeaturesMap"/> built from the filtered <see cref="ComAttribute"/>s</returns>
        private NameToProtoFeaturesMap GetFilteredMapOfNamesToValues(IUnknownVector protoFeatures)
        {
            var filtered = FilterProtoFeatures(protoFeatures);

            if (string.IsNullOrEmpty(AttributesToTokenizeFilter))
            {
                return new NameToProtoFeaturesMap(filtered);
            }
            else
            {
                return new NameToProtoFeaturesMap(filtered, AttributesToTokenizeFilter, AttributeVectorizerShingleSize);
            }
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
            if (_version == 2 && version == 1)
            {
                return string.IsNullOrEmpty(AttributesToTokenizeFilter);
            }
            return _version <= version;
        }

        /// <summary>
        /// Called when serializing
        /// </summary>
        /// <param name="context">The context.</param>
        [OnSerializing]
        private void OnSerializing(StreamingContext context)
        {
            if (_version == 3 && IsCompatibleWithVersion(2))
            {
                _version = 2;
            }
            if (_version == 2 && IsCompatibleWithVersion(1))
            {
                _version = 1;
            }
        }

        /// <summary>
        /// Called when deserializing
        /// </summary>
        /// <param name="context">The context.</param>
        [OnDeserializing]
        private void OnDeserializing(StreamingContext context)
        {
            // Set optional fields
            _attributeVectorizerMaxFeatures = Int32.MaxValue;
            _attributesToTokenizeFilter = null;
            _attributeVectorizerShingleSize = 1;
            _negativeClassName =
                MachineUsage == LearningMachineUsage.AttributeCategorization
                    ? ""
                    : MachineUsage == LearningMachineUsage.Pagination
                        ? NotFirstPageCategory
                        : UnknownCategoryName;
        }

        /// <summary>
        /// Called when deserialized
        /// </summary>
        /// <param name="context">The context.</param>
        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            ExtractException.Assert("ELI42074", "Cannot load newer LearningMachineDataEncoder",
                _version <= _CURRENT_VERSION,
                "Current version", _CURRENT_VERSION,
                "Version to load", _version);

            _version = _CURRENT_VERSION;
        }

        #endregion Private Methods
    }
}