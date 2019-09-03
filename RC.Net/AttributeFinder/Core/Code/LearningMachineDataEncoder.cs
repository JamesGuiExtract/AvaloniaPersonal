using Extract.Utilities;
using LearningMachineTrainer;
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
using AttributeOrAnswerCollection = Extract.Utilities.Union<string[], byte[][]>;
using UCLID_AFCORELib;

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
        AttributeCategorization = 3,

        /// <summary>
        /// Predicting which pages should be deleted
        /// </summary>
        Deletion = 4
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
                else if (attributes.TryDivideAttributesWithSimpleQuery(
                    queryForAttributesToTokenize, out var tokenize, out var noTokenize))
                {
                    _nameToFeatureValues = MakeDictionary(noTokenize);
                    var tokenizer = new SpatialStringFeatureVectorizer(null, shingleSize, -1);
                    foreach (var attribute in tokenize)
                    {
                        var text = attribute.Value.String;
                        var values = _nameToFeatureValues.GetOrAdd(attribute.Name,
                            _ => new List<string>());
                        values.AddRange(tokenizer.GetTerms(text));
                    }
                }
                else
                {
                    var attributesVector = attributes.ToIUnknownVector();
                    var attributesToTokenize = LearningMachineDataEncoder._afUtility.Value.QueryAttributes(attributesVector,
                        queryForAttributesToTokenize, bRemoveMatches: true)
                        .ToIEnumerable<ComAttribute>()
                        .ToList();

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
                                _ => new List<string>());
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
        /// <returns>A list of values for the <see paramref="name"/></returns>
        public List<string> GetProtoFeatureValues(string name)
        {
            if (_nameToFeatureValues == null || !_nameToFeatureValues.TryGetValue(name, out var values))
            {
                return new List<string>(0);
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
    public class LearningMachineDataEncoder : ILearningMachineDataEncoderModel
    {

        #region Constants

        /// <summary>
        /// Current version.
        /// Version 2: Add AttributesToTokenizeFilter property and backing field
        ///            Add AttributeFeatureShingleSize property and backing field
        /// Version 3: Add more space efficient saving of answer name to code map
        /// </summary>
        const int _CURRENT_VERSION = 3;

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

        // Values used for pagination categories
        public static readonly string FirstPageCategory = "FirstPage";
        public static readonly string NotFirstPageCategory = "NotFirstPage";
        static readonly int _NOT_FIRST_PAGE_CATEGORY_CODE = 0;

        // For pagination, the query for answer page range attributes
        static readonly string _PAGE_ATTRIBUTE_QUERY = "Document/Pages";

        public static readonly int DeletedPageCategoryCode = 1;

        // Values used for deletion categories
        public static readonly string DeletedPageCategory = "DeletedPage";
        public static readonly string NotDeletedPageCategory = "NotDeletedPage";
        static readonly int _NOT_DELETED_PAGE_CATEGORY_CODE = 0;

        static readonly string _DELETED_PAGE_ATTRIBUTE_QUERY = "Document/DeletedPages";

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
        internal static readonly ThreadLocal<IAFUtility> _afUtility = new ThreadLocal<IAFUtility>(() => new AFUtilityClass());

        /// <summary>
        /// Regex used to parse page ranges of pagination expected VOA files
        /// </summary>
        private static readonly ThreadLocal<Regex> _pageRangeRegex =
            new ThreadLocal<Regex>(() => new Regex(@"\b(?'start'\d+)\b(\s*-\s*(?'end'\d+\b))?"));

        // Backing fields for properties
        private SpatialStringFeatureVectorizer _autoBagOfWords;
        private IEnumerable<AttributeFeatureVectorizer> _attributeFeatureVectorizers;

        [Obsolete("Use _nonSerializedAnswerCodeToNameList instead")]
        [SuppressMessage("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
#pragma warning disable 0414
        private Dictionary<string, int> _answerNameToCode;
#pragma warning restore 0414

        [NonSerialized]
        private Dictionary<string, int> _nonSerializedAnswerNameToCode;

        [Obsolete("Use _answerCodeToNameList instead")]
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

        [OptionalField(VersionAdded = 3)]
        private List<string> _answerCodeToNameList;

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
            set
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
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public Dictionary<string, int> AnswerNameToCode
        {
            get
            {
                return _nonSerializedAnswerNameToCode;
            }
            set
            {
                if (value != _nonSerializedAnswerNameToCode)
                {
                    _nonSerializedAnswerNameToCode = value;
                }
            }
        }

        /// <summary>
        /// Gets the list of numeric codes to category names
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        public List<string> AnswerCodeToName
        {
            get
            {
                return _answerCodeToNameList;
            }
            set
            {
                _answerCodeToNameList = value;
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
                var autoBoWSize = AutoBagOfWords != null && AutoBagOfWords.Enabled
                    ? MachineUsage == LearningMachineUsage.Pagination
                        ? AutoBagOfWords.FeatureVectorLengthForPagination
                        : AutoBagOfWords.FeatureVectorLength
                    : 0;

                return autoBoWSize + AttributeFeatureVectorLength;
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
            set
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
                bool vectorizersAreComputed = AutoBagOfWords != null && AutoBagOfWords.AreEncodingsComputed
                    || AutoBagOfWords == null && AttributeFeatureVectorizers.Any();

                // With the new feature hashing feature, it is possible for the vectorizers to be ready without
                // any computing happening so check the answer mapping to ensure that this is ready for use
                bool answersAreComputed = AnswerCodeToName.Any();
                return vectorizersAreComputed && answersAreComputed;
            }
        }

        /// <summary>
        /// Change an answer (e.g., document type)
        /// </summary>
        /// <param name="oldAnswer">The name to change from</param>
        /// <param name="newAnswer">The name to change to</param>
        /// <remarks>Throws an exception if the <see paramref="oldAnswer"/> doesn't exist</remarks>
        public void ChangeAnswer(string oldAnswer, string newAnswer)
        {
            try
            {
                ExtractException.Assert("ELI45848", "Answer to change doesn't exist in the encoder",
                    AnswerNameToCode.ContainsKey(oldAnswer));

                // Ensure new name doesn't exist already but allow change of case (so use Ordinal comparison)
                ExtractException.Assert("ELI45849", "New answer already exists in the encoder",
                    !AnswerNameToCode.Keys.Contains(newAnswer, StringComparer.Ordinal));

                int code = AnswerNameToCode[oldAnswer];
                AnswerNameToCode.Remove(oldAnswer);

                AnswerNameToCode[newAnswer] = code;
                AnswerCodeToName[code] = newAnswer;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45847");
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
            AnswerCodeToName = new List<string>();
            AnswerNameToCode = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            AttributeVectorizerMaxDiscreteTermsFeatures = attributeVectorizerMaxFeatures;
            AttributesToTokenizeFilter = attributesToTokenize;
            AttributeVectorizerShingleSize = attributeVectorizerShingleSize;

            if (negativeClassName != null)
            {
                NegativeClassName = negativeClassName;
            }
            else if (MachineUsage == LearningMachineUsage.AttributeCategorization)
            {
                NegativeClassName = "";
            }
            else if (MachineUsage == LearningMachineUsage.Pagination)
            {
                NegativeClassName = NotFirstPageCategory;
            }
            else if (MachineUsage == LearningMachineUsage.Deletion)
            {
                NegativeClassName = NotDeletedPageCategory;
            }
            else
            {
                NegativeClassName = UnknownCategoryName;
            }
        }

        #endregion Constructors

        #region Public Methods

        /// <summary>
        /// Gets an enumeration of predictions (category names) from a VOA file
        /// </summary>
        /// <param name="attributesFile">The path to the VOA or EAV file</param>
        /// <param name="numberOfPages">The number of pages in the associated image</param>
        /// <returns>An enumeration of predictions (category names) from a VOA file</returns>
        public static IEnumerable<string> ExpandPaginationAnswerVOA(string attributesFile, int numberOfPages)
        {
            try
            {
                var attributes = _afUtility.Value.GetAttributesFromFile(attributesFile);
                attributes.ReportMemoryUsage();
                return ExpandPaginationAnswerVOA(attributes, numberOfPages);
            }
            catch (Exception e)
            {
                var ue = e.AsExtract("ELI45784");
                ue.AddDebugData("Answer file", attributesFile, false);
                throw ue;
            }
        }

        /// <summary>
        /// Gets an enumeration of predictions (category names) from a VOA
        /// </summary>
        /// <param name="attributes">The attributes containing the encoded category names</param>
        /// <param name="numberOfPages">The number of pages in the associated image</param>
        /// <returns>An enumeration of predictions (category names) from a VOA</returns>
        public static IEnumerable<string> ExpandPaginationAnswerVOA(IUnknownVector attributes, int numberOfPages)
        {
            try
            {
                var documentBreaks = Enumerable.Repeat(NotFirstPageCategory, numberOfPages - 1).ToArray();

                // Check for a Flag attribute that means this document should be treated as
                // if there were no pagination
                // https://extract.atlassian.net/browse/ISSUE-14923
                var flags = _afUtility.Value.QueryAttributes(attributes,
                    SpecialAttributeNames.IncompatibleWithPaginationTraining, false);
                if (flags.Size() == 0)
                {
                    // No flags so parse Pages attributes with regex and mark any document breaks
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
                            return new { startPage, endPage, hasEndPage };
                        });

                    foreach (var pageRange in pageRanges)
                    {
                        if (pageRange.startPage > 1)
                        {
                            // Since documentBreaks starts at page 2, startPage - 2 is the index of the starting page
                            documentBreaks[pageRange.startPage - 2] = FirstPageCategory;
                        }

                        // In case of gaps between documents, set a document break after each end page.
                        if (pageRange.hasEndPage && pageRange.endPage + 1 < numberOfPages)
                        {
                            // Since documentBreaks starts at page 2, endPage - 1 is the index of the next starting page
                            documentBreaks[pageRange.endPage - 1] = FirstPageCategory;
                        }
                        // If no end page then set a break after start page (this is a one-page sub-document)
                        else if (!pageRange.hasEndPage && pageRange.startPage + 1 < numberOfPages)
                        {
                            // Since documentBreaks starts at page 2, startPage - 1 is the index of the next starting page
                            documentBreaks[pageRange.startPage - 1] = FirstPageCategory;
                        }
                    }
                }

                return documentBreaks;
            }
            catch (Exception e)
            {
                throw e.AsExtract("ELI39543");
            }
        }

        /// <summary>
        /// Gets an enumeration of predictions (category names) from a VOA file
        /// </summary>
        /// <param name="attributesFilePath">The path to the VOA or EAV file</param>
        /// <param name="numberOfPages">The number of pages in the associated image</param>
        /// <returns>An enumeration of predictions (category names) from a VOA file</returns>
        public static IEnumerable<string> ExpandDeletionAnswerVOA(string attributesFilePath, int numberOfPages)
        {
            try
            {
                var attributes = _afUtility.Value.GetAttributesFromFile(attributesFilePath);
                attributes.ReportMemoryUsage();
                return ExpandDeletionAnswerVOA(attributes, numberOfPages);
            }
            catch (Exception e)
            {
                var ue = e.AsExtract("ELI45773");
                ue.AddDebugData("Answer file", attributesFilePath, false);
                throw ue;
            }
        }
        /// <summary>
        /// Gets an enumeration of predictions (category names) from a VOA
        /// </summary>
        /// <param name="attributes">The VOA</param>
        /// <param name="numberOfPages">The number of pages in the associated image</param>
        /// <returns>An enumeration of predictions (category names)</returns>
        public static IEnumerable<string> ExpandDeletionAnswerVOA(IUnknownVector attributes, int numberOfPages)
        {
            try
            {
                var pages = Enumerable.Repeat(NotDeletedPageCategory, numberOfPages).ToArray();

                // Parse DeletedPages attributes with regex and mark any deleted pages
                var pageRanges = _afUtility.Value.QueryAttributes(attributes, _DELETED_PAGE_ATTRIBUTE_QUERY, false)
                    .ToIEnumerable<ComAttribute>()
                    .SelectMany(attr => _pageRangeRegex.Value.Matches(attr.Value.String).Cast<Match>())
                    .Select(pageRange =>
                    {
                        int startPage = 0;
                        int endPage = 0;
                        if (Int32.TryParse(pageRange.Groups["start"].Value, out startPage))
                        {
                            var endGroup = pageRange.Groups["end"];
                            if (!(endGroup.Success && Int32.TryParse(endGroup.Value, out endPage)))
                            {
                                endPage = startPage;
                            }
                        }
                        return new { startPage, endPage };
                    });

                foreach (var pageRange in pageRanges)
                {
                    for (int i = pageRange.startPage - 1; i < pageRange.endPage; i++)
                    {
                        pages[i] = DeletedPageCategory;
                    }
                }

                return pages;
            }
            catch (Exception e)
            {
                throw e.AsExtract("ELI45840");
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
                    return GetPaginationOrDeletionFeatureVectors(document, protoFeaturesOrGroupsOfProtoFeatures, true);
                }
                else if (MachineUsage == LearningMachineUsage.Deletion)
                {
                    return GetPaginationOrDeletionFeatureVectors(document, protoFeaturesOrGroupsOfProtoFeatures, false);
                }
                else if (MachineUsage == LearningMachineUsage.AttributeCategorization)
                {
                    return GetAttributesFeatureVectors(document,
                        protoFeaturesOrGroupsOfProtoFeatures
                            .ToIEnumerable<ComAttribute>()
                            .ToList());
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
                else if (MachineUsage == LearningMachineUsage.Deletion)
                {
                    ComputeDeletionEncodings(ussFilePaths, inputVOAFilePaths, answersOrAnswerFiles, updateStatus, cancellationToken);
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
        /// Uses the training data to automatically configure the <see cref="AutoBagOfWords"/> and
        /// <see cref="AttributeFeatureVectorizers"/> collection so that feature vectors can be
        /// generated with this instance.
        /// </summary>
        /// <param name="ussFilePaths">The paths to the USS files to be used to configure this object</param>
        /// <param name="inputAttributes">The paths to the proto-feature VOA files to be used to
        /// configure this object</param>
        /// <param name="answersOrAnswerFiles">The predictions for each example (if <see cref="MachineUsage"/> is
        /// <see cref="LearningMachineUsage.DocumentCategorization"/>) or the paths to VOA files of predictions
        /// (if <see cref="MachineUsage"/> is <see cref="LearningMachineUsage.Pagination"/></param>
        /// <param name="updateStatus">Function to use for sending progress updates to caller</param>
        /// <param name="cancellationToken">Token indicating that processing should be canceled</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId="answersOrAnswerPaths")]
        public void ComputeEncodings(SpatialString[] spatialStrings, IUnknownVector[] inputAttributes, string[] answersOrAnswerPaths)
        {
            try
            {
                // Clear results of any previously computed encodings
                Clear();

                if (MachineUsage == LearningMachineUsage.DocumentCategorization)
                {
                    // TODO: Implement doc categorization
                    //ComputeDocumentEncodings(spatialStrings, inputAttributes, answersOrAnswerPaths);
                    throw new NotImplementedException();
                }
                else if (MachineUsage == LearningMachineUsage.AttributeCategorization)
                {
                    ComputeAttributesEncodings(spatialStrings, inputAttributes);
                }
                else
                {
                    throw new ExtractException("ELI44711", "Unsupported LearningMachineUsage: " + MachineUsage.ToString());
                }
                ExtractException.Assert("ELI44712", "Unable to successfully compute encodings", AreEncodingsComputed);

                foreach(var vectorizer in AttributeFeatureVectorizers)
                {
                    vectorizer.LimitToTopTerms(AttributeVectorizerMaxDiscreteTermsFeatures);
                }
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
                throw e.AsExtract("ELI44713");
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
            return GetFeatureVectorAndAnswerCollections
                (ussFilePaths, inputVOAFilePaths, AttributeOrAnswerCollection.Maybe(answersOrAnswerFiles), updateAnswerCodes);
        }

        /// <summary>
        /// Gets enumerations of feature vectors and answer codes for enumerations of input files
        /// </summary>
        /// <param name="ussFilePaths">The paths to the USS files to be used to generate the feature vectors</param>
        /// <param name="inputVOAFilePaths">The paths to the VOA files to be used to generate the feature vectors</param>
        /// <param name="answersOrAnswerFiles">The predictions for each example (if <see cref="MachineUsage"/> is
        /// <see cref="LearningMachineUsage.DocumentCategorization"/>) or the paths to/encoded bytes of VOA files of predictions
        /// (if <see cref="MachineUsage"/> is <see cref="LearningMachineUsage.Pagination"/></param>
        /// <param name="updateAnswerCodes">Whether to update answer code to name mappings to reflect the input</param>
        /// <returns>A tuple where the first item is an enumeration of feature vectors and the second
        /// item is an enumeration of answer codes for each example</returns>
        public Tuple<double[][], int[]> GetFeatureVectorAndAnswerCollections
            (string[] ussFilePaths, string[] inputVOAFilePaths, AttributeOrAnswerCollection answersOrAnswerFiles, bool updateAnswerCodes = false)
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
        /// <see cref="LearningMachineUsage.DocumentCategorization"/>) or the paths to/encoded bytes of VOA files of predictions
        /// (if <see cref="MachineUsage"/> is <see cref="LearningMachineUsage.Pagination"/></param>
        /// <param name="updateStatus">Function to use for sending progress updates to caller</param>
        /// <param name="cancellationToken">Token indicating that processing should be canceled</param>
        /// <param name="updateAnswerCodes">Whether to update answer code to name mappings to reflect the input</param>
        /// <returns>A tuple where the first item is an enumeration of feature vectors, the second
        /// item answer codes for each example and the third item the uss path for each example</returns>
        public (double[][] featureVectors, int[] answerCodes, string[] ussPathsPerExample) GetFeatureVectorAndAnswerCollections
            (string[] ussFilePaths, string[] inputVOAFilePaths, AttributeOrAnswerCollection answersOrAnswerFiles,
                Action<StatusArgs> updateStatus, CancellationToken cancellationToken, bool updateAnswerCodes)
        {
            return GetFeatureVectorAndAnswerCollections(ussFilePaths, inputVOAFilePaths, answersOrAnswerFiles,
                false, false, null, null, null,
                updateStatus, cancellationToken, updateAnswerCodes);
        }

        /// <summary>
        /// Gets enumerations of feature vectors and answer codes for enumerations of input files
        /// </summary>
        /// <param name="ussFilePaths">The paths to the USS files to be used to generate the feature vectors</param>
        /// <param name="inputVOAFilePaths">The paths to the VOA files to be used to generate the feature vectors</param>
        /// <param name="answersOrAnswerFiles">The predictions for each example (if <see cref="MachineUsage"/> is
        /// <see cref="LearningMachineUsage.DocumentCategorization"/>) or the paths to/encoded bytes of VOA files of predictions
        /// (if <see cref="MachineUsage"/> is <see cref="LearningMachineUsage.Pagination"/></param>
        /// <param name="runRuleSetForFeatures">Whether to run a ruleset to produce feature/candidate attributes</param>
        /// <param name="runRuleSetIfMissing">Whether to, as a backup, run a ruleset to produce feature/candidate attributes</param>
        /// <param name="ruleSetPath">The path to a ruleset to be used to produce feature/candidate attributes</param>
        /// <param name="labelAttributesSettings">The logic to label candidate attributes (needed for attribute categorization
        /// in the case that a candidate-producing ruleset is run)</param>
        /// <param name="alternateComponentDataDir">The alt component data dir to pass along to the ruleset</param>
        /// <param name="updateStatus">Function to use for sending progress updates to caller</param>
        /// <param name="cancellationToken">Token indicating that processing should be canceled</param>
        /// <param name="updateAnswerCodes">Whether to update answer code to name mappings to reflect the input</param>
        /// <returns>A tuple where the first item is an enumeration of feature vectors, the second
        /// item answer codes for each example and the third item the uss path for each example</returns>
        /// <returns></returns>
        public (double[][] featureVectors, int[] answerCodes, string[] ussPathsPerExample)
            GetFeatureVectorAndAnswerCollections(
                string[] ussFilePaths,
                string[] inputVOAFilePaths,
                AttributeOrAnswerCollection answersOrAnswerFiles,
                bool runRuleSetForFeatures,
                bool runRuleSetIfMissing,
                string ruleSetPath,
                LabelAttributes labelAttributesSettings,
                string alternateComponentDataDir,
                Action<StatusArgs> updateStatus,
                CancellationToken cancellationToken,
                bool updateAnswerCodes)
        {
            try
            {
                updateStatus(new StatusArgs { StatusMessage = "Building feature vector and answer collections:" });

                ExtractException.Assert("ELI40261", "Encodings have not been computed", AreEncodingsComputed);

                // Null or empty VOA collection is OK. Set to null to simplify code
                if (inputVOAFilePaths != null && inputVOAFilePaths.Length == 0)
                {
                    inputVOAFilePaths = null;
                }

                int answerLength = answersOrAnswerFiles?.Match(a => a.Length, a => a.Length) ?? 0;

                if (inputVOAFilePaths != null && inputVOAFilePaths.Length != ussFilePaths.Length
                    || answersOrAnswerFiles != null && answerLength != ussFilePaths.Length)
                {
                    throw new ExtractException("ELI39700", "Arguments are of different lengths");
                }

                // Indent sub-status messages
                Action<StatusArgs> updateStatus2 = args =>
                    {
                        args.Indent++;
                        updateStatus(args);
                    };

                // Set up lazy array of input data using thread-safe methods so that the calls
                // to Get_FeatureVectorAndAnswerCollections below can expand the specifications into real
                // data using parallel processing
                Lazy<IUnknownVector>[] inputVOAs = null;
                using (var attributes = new ThreadLocal<IUnknownVectorClass>(() => new IUnknownVectorClass()))
                using (var spatialString = new ThreadLocal<SpatialStringClass>(() => new SpatialStringClass()))
                {
                    ThreadLocal<RuleSet> ruleset = new ThreadLocal<RuleSet>(() =>
                    {
                        RuleSet rsd = new RuleSet();
                        rsd.LoadFrom(ruleSetPath, false);
                        return rsd;
                    });

                    IUnknownVector runRules(string ussPath, Lazy<IUnknownVector> answerVoa)
                    {
                        try
                        {
                            spatialString.Value.LoadFrom(ussPath, false);
                            spatialString.Value.ReportMemoryUsage();
                            var doc = new AFDocument { Text = spatialString.Value };
                            var voa = ruleset.Value.ExecuteRulesOnText(doc, null, alternateComponentDataDir, null);
                            voa.ReportMemoryUsage();

                            // Need to label attributes for attribute categorization
                            if (MachineUsage == LearningMachineUsage.AttributeCategorization)
                            {
                                labelAttributesSettings.LabelAttributesVector(Path.ChangeExtension(ussPath, null),
                                    voa, answerVoa.Value, cancellationToken);
                            }

                            return voa;
                        }
                        catch (Exception ex)
                        {
                            var uex = ex.AsExtract("ELI46640");
                            uex.AddDebugData("USS File", ussPath);
                            throw uex;
                        }
                    }

                    if (inputVOAFilePaths != null)
                    {
                        inputVOAs = new Lazy<IUnknownVector>[inputVOAFilePaths.Length];
                        for (int i = 0; i < inputVOAFilePaths.Length; i++)
                        {
                            int safeIdx = i;
                            inputVOAs[i] = new Lazy<IUnknownVector>(() =>
                            {
                                string voa = inputVOAFilePaths[safeIdx];
                                if (File.Exists(voa))
                                {
                                    attributes.Value.LoadFrom(voa, false);
                                    attributes.Value.ReportMemoryUsage();
                                    return attributes.Value;
                                }
                                else if (runRuleSetIfMissing)
                                {
                                    return runRules(ussFilePaths[safeIdx], GetLazyAttributes(answersOrAnswerFiles, safeIdx));
                                }
                                else
                                {
                                    var ex = new ExtractException("ELI45785", "Input VOA file doesn't exist");
                                    ex.AddDebugData("VOA File", voa, false);
                                    throw ex;
                                }
                            }, isThreadSafe: false);
                        }
                    }
                    else if (runRuleSetForFeatures)
                    {
                        inputVOAs = new Lazy<IUnknownVector>[ussFilePaths.Length];
                        for (int i = 0; i < ussFilePaths.Length; i++)
                        {
                            int safeIdx = i;
                            inputVOAs[i] = new Lazy<IUnknownVector>(() =>
                                runRules(ussFilePaths[safeIdx], GetLazyAttributes(answersOrAnswerFiles, safeIdx)));
                        }
                    }

                    if (MachineUsage == LearningMachineUsage.DocumentCategorization)
                    {
                        ExtractException.Assert("ELI45987", "No answers specified",
                            answersOrAnswerFiles != null);

                        string[] answers = answersOrAnswerFiles.Match(s => s,
                            _ => throw new ExtractException("ELI45782", "Internal logic error"));

                        return GetDocumentFeatureVectorAndAnswerCollection(ussFilePaths, inputVOAs,
                            answers, updateStatus2, cancellationToken, updateAnswerCodes);
                    }
                    else if (MachineUsage == LearningMachineUsage.Pagination)
                    {
                        var results = GetPaginationFeatureVectorAndAnswerCollection(ussFilePaths, inputVOAs,
                            answersOrAnswerFiles, updateStatus2, cancellationToken);

                        double[][] featureVectors = new double[results.Length][];
                        int[] answers = new int[results.Length];
                        string[] ussPathsPerExample = new string[results.Length];
                        for (int i = 0; i < results.Length; i++)
                        {
                            featureVectors[i] = results[i].Item1;
                            answers[i] = results[i].Item2;
                            ussPathsPerExample[i] = results[i].Item3;
                        }
                        return (featureVectors, answers, ussPathsPerExample);
                    }
                    else if (MachineUsage == LearningMachineUsage.Deletion)
                    {
                        var results = GetDeletionFeatureVectorAndAnswerCollection(ussFilePaths, inputVOAs,
                            answersOrAnswerFiles, updateStatus2, cancellationToken);

                        double[][] featureVectors = new double[results.Length][];
                        int[] answers = new int[results.Length];
                        string[] ussPathsPerExample = new string[results.Length];
                        for (int i = 0; i < results.Length; i++)
                        {
                            featureVectors[i] = results[i].Item1;
                            answers[i] = results[i].Item2;
                            ussPathsPerExample[i] = results[i].Item3;
                        }
                        return (featureVectors, answers, ussPathsPerExample);
                    }
                    else if (MachineUsage == LearningMachineUsage.AttributeCategorization)
                    {
                        var results = GetAttributesFeatureVectorAndAnswerCollection(ussFilePaths, inputVOAs,
                            updateStatus2, cancellationToken);

                        double[][] featureVectors = new double[results.Length][];
                        int[] answers = new int[results.Length];
                        string[] ussPathsPerExample = new string[results.Length];
                        for (int i = 0; i < results.Length; i++)
                        {
                            featureVectors[i] = results[i].Item1;
                            answers[i] = results[i].Item2;
                            ussPathsPerExample[i] = results[i].Item3;
                        }
                        return (featureVectors, answers, ussPathsPerExample);
                    }
                    else
                    {
                        throw new ExtractException("ELI40372", "Unsupported LearningMachineUsage: " + MachineUsage.ToString());
                    }
                }
            }
            catch (Exception e)
            {
                throw e.AsExtract("ELI39544");
            }
        }

        /// <summary>
        /// Loads the indexed VOA from a file or from a sql byte array, depending on the union case
        /// </summary>
        /// <param name="answersOrAnswerFiles">The source of the attributes</param>
        /// <param name="i">The index to load</param>
        private static IUnknownVector GetAttributes(AttributeOrAnswerCollection answersOrAnswerFiles, int i)
        {
            var voa =
            answersOrAnswerFiles.Match(
                s => _afUtility.Value.GetAttributesFromFile(s[i]),
                b => AttributeMethods.GetVectorOfAttributesFromSqlBinary(b[i]));
            voa.ReportMemoryUsage();
            return voa;
        }

        /// <summary>
        /// Sets up a lazy load of the indexed VOA from a file or from a sql byte array, depending on the union case
        /// </summary>
        /// <param name="answersOrAnswerFiles">The source of the attributes</param>
        /// <param name="i">The index to load</param>
        private static Lazy<IUnknownVector> GetLazyAttributes(AttributeOrAnswerCollection answersOrAnswerFiles, int i)
        {
            return new Lazy<IUnknownVector>(() => GetAttributes(answersOrAnswerFiles, i));
        }

        /// <summary>
        /// Gets enumerations of feature vectors and answer codes for enumerations of input files
        /// </summary>
        /// <param name="spatialStrings">The spatial strings to be used to generate the feature vectors</param>
        /// <param name="inputAttributes">The VOAs to be used to generate the feature vectors</param>
        /// <param name="answers">The predictions for each example
        /// <param name="updateAnswerCodes">Whether to update answer code to name mappings to reflect the input</param>
        /// <returns>A tuple where the first item is an enumeration of feature vectors, the second
        /// item answer codes for each example and the third item the uss path for each example</returns>
        public (double[][] featureVector, int[] answers) GetFeatureVectorAndAnswerCollections
            (SpatialString[] spatialStrings, IUnknownVector[] inputAttributes, string[] answers, bool updateAnswerCodes)
        {
            try
            {
                ExtractException.Assert("ELI44714", "Encodings have not been computed", AreEncodingsComputed);

                // Null or empty VOA collection is OK. Set to null to simplify code
                if (inputAttributes != null && inputAttributes.Length == 0)
                {
                    inputAttributes = null;
                }

                if ( inputAttributes != null && inputAttributes.Length != spatialStrings.Length
                    || answers != null && answers.Length != spatialStrings.Length)
                {
                    throw new ExtractException("ELI44715", "Arguments are of different lengths");
                }

                if (MachineUsage == LearningMachineUsage.DocumentCategorization)
                {
                    return GetDocumentFeatureVectorAndAnswerCollection(spatialStrings, inputAttributes, answers, updateAnswerCodes);
                }
                else if (MachineUsage == LearningMachineUsage.Pagination)
                {
                    throw new ExtractException("ELI44716", "Pagination is not supported for this method");
                }
                else if (MachineUsage == LearningMachineUsage.AttributeCategorization)
                {
                    var results = GetAttributesFeatureVectorAndAnswerCollection(spatialStrings, inputAttributes);

                    double[][] featureVectors = new double[results.Length][];
                    int[] answerCodes = new int[results.Length];
                    for (int i = 0; i < results.Length; i++)
                    {
                        featureVectors[i] = results[i].Item1;
                        answerCodes[i] = results[i].Item2;
                    }
                    return (featureVectors, answerCodes);
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

        private Tuple<double[], int>[]  GetAttributesFeatureVectorAndAnswerCollection(SpatialString[] spatialStrings, IUnknownVector[] inputVOAs)
        {
            var results = new Tuple<double[], int>[spatialStrings.Length][];
            for(int i = 0; i < spatialStrings.Length; i++)
            {
                var attributes = inputVOAs[i];

                var answerCodes = new List<int>(attributes.Size());
                var filteredAttributes = new List<ComAttribute>(attributes.Size());
                foreach(var attribute in attributes.ToIEnumerable<ComAttribute>())
                {
                    var categoryAttributes =
                        AttributeMethods.GetAttributesByName(attribute.SubAttributes, CategoryAttributeName);

                    int countOfCategoryAttributes = categoryAttributes.Count();
                    ExtractException.Assert("ELI44717", "There should be zero or one category attribute",
                        countOfCategoryAttributes <= 1,
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
                        var ex = new ExtractException("ELI44718",
                            "Unknown attribute category/label encountered, treating as if unlabeled (as if empty type)");
                        ex.AddDebugData("Unknown category name", categoryName, false);
                        ex.Log();
                        code = UnknownOrNegativeCategoryCode;
                    }
                    filteredAttributes.Add(attribute);
                    answerCodes.Add(code);
                }

                List<double[]> featureVectors = GetAttributesFeatureVectors(spatialStrings[i], filteredAttributes).ToList();

                results[i] = new Tuple<double[], int>[featureVectors.Count];
                for (int j = 0; j < featureVectors.Count; j++)
                {
                    results[i][j] = (Tuple.Create(featureVectors[j], answerCodes[j]));
                }
            }

            return results.SelectMany(a => a).ToArray();
        }

        private (double[][] featureVector, int[] answers) GetDocumentFeatureVectorAndAnswerCollection(
            SpatialString[] spatialStrings, IUnknownVector[] inputVOAs, string[] answers, bool updateAnswerCodes)
        {
            // Initialize answer code mappings if updating answers (true if training the classifier)
            if (updateAnswerCodes)
            {
                InitializeAnswerCodeMappings(answers, NegativeClassName);
            }

            double[][] featureVectors = new double[spatialStrings.Length][];
            int[] answerCodes = new int[spatialStrings.Length];
            for(int i = 0; i < spatialStrings.Length; i++)
            {
                string answer = answers[i];
                double[] featureVector = GetDocumentFeatureVector(spatialStrings[i], inputVOAs[i]);

                int answerCode;
                if (!AnswerNameToCode.TryGetValue(answer, out answerCode))
                {
                    answerCode = UnknownOrNegativeCategoryCode;
                }

                featureVectors[i] = featureVector;
                answerCodes[i] = answerCode;
            }

            return (featureVectors, answerCodes);
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
                clone.AnswerCodeToName = new List<string>(AnswerCodeToName);
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
        /// <param name="returnFirstPage">Whether to include the first page (e.g., for Deletion mode)</param>
        /// <returns>An enumeration of <see cref="NameToProtoFeaturesMap"/>s for pagination input</returns>
        private IEnumerable<NameToProtoFeaturesMap> GetPaginationProtoFeatures(IUnknownVector attributes, bool returnFirstPage = false)
        {
            var pages = attributes
                .ToIEnumerable<ComAttribute>()
                .Where(a => a.Name.Equals(SpecialAttributeNames.Page, StringComparison.OrdinalIgnoreCase));

            if (!returnFirstPage)
            {
                pages = pages.Skip(1);
            }
            return pages.Select(pageAttr => GetFilteredMapOfNamesToValues(pageAttr.SubAttributes));
        }

        /// <summary>
        /// Gets an enumeration of <see cref="NameToProtoFeaturesMap"/>s for pagination input from a VOA file
        /// </summary>
        /// <param name="attributesFilePath">The path to the VOA or EAV file</param>
        /// <param name="returnFirstPage">Whether to include the first page (e.g., for Deletion mode)</param>
        /// <returns>An enumeration of <see cref="NameToProtoFeaturesMap"/>s for pagination input</returns>
        private IEnumerable<NameToProtoFeaturesMap> GetPaginationProtoFeatures(string attributesFilePath, bool returnFirstPage = false)
        {
            try
            {
                var attributes = _afUtility.Value.GetAttributesFromFile(attributesFilePath);
                attributes.ReportMemoryUsage();
                return GetPaginationProtoFeatures(attributes, returnFirstPage);
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
                return GetAttributesProtoFeatures(attributes);
            }
            catch (Exception e)
            {
                var ue = e.AsExtract("ELI41400");
                ue.AddDebugData("Attributes file path", attributesFilePath, false);
                throw ue;
            }
        }

        private IEnumerable<Tuple<string, NameToProtoFeaturesMap>>  GetAttributesProtoFeatures(IUnknownVector attributes)
        {
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

        /// Gets zero or more feature vectors from a <see cref="ISpatialString"/> and vector of <see cref="ComAttribute"/>s
        /// </summary>
        /// <param name="document">The <see cref="ISpatialString"/> to use for auto-bag-of-words features.</param>
        /// <param name="pagesOfProtoFeatures">The vector of <see cref="ComAttribute"/>s containing Page attributes with
        /// sub-attributes to use for attribute features.</param>
        /// <param name="pagination">Whether to get feature vectors for pagination or deletion mode</param>
        /// <returns>The feature vectors computed from the input arguments.</returns>
        private IEnumerable<double[]> GetPaginationOrDeletionFeatureVectors(
            ISpatialString document,
            IUnknownVector pagesOfProtoFeatures,
            bool pagination)
        {
            // At runtime (e.g., called from LearningMachineOutputHandler) handle empty VOA
            // by creating empty feature vector for each page rather than returning an empty
            // collection that would cause an exception
            var protoFeatureGroups = (pagesOfProtoFeatures?.Size() > 0
                    ? GetPaginationProtoFeatures(pagesOfProtoFeatures, !pagination)
                    : SpatialStringFeatureVectorizer.GetPaginationTexts(document, true)
                        .Select(_ => new NameToProtoFeaturesMap()))
                .ToList();

            var numberOfExamples = protoFeatureGroups.Count();

            // NOTE: vectorizers return an empty enumerable if not enabled
            // TODO: pass along cancellation token!
            List<List<double[]>> attributeFeatures = AttributeFeatureVectorizers
                .Select(vectorizer => vectorizer.GetFeatureVectorsForEachGroup(protoFeatureGroups).ToList())
                .Where(v => v.Any()) // Since vectorizers return an empty enumerable if not enabled
                .ToList();

            if (AutoBagOfWords == null || !AutoBagOfWords.Enabled)
            {
                // Transpose from <# vectorizers> X <# page pairs> to <# page pairs> X <# vectorizers>
                for (int i = 0; i < numberOfExamples; i++)
                {
                    yield return attributeFeatures
                        .SelectMany(v => v[i])
                        .ToArray();
                }
            }
            else
            {
                var shingleFeatures = pagination
                    ?  AutoBagOfWords.GetPaginationFeatureVectors(document).ToList()
                    : AutoBagOfWords.GetDeletionFeatureVectors(document).ToList();

                numberOfExamples = shingleFeatures.Count;

                if (attributeFeatures.Any())
                {
                    for (int i = 0; i < numberOfExamples; i++)
                    {
                        yield return shingleFeatures[i].Concat(attributeFeatures
                                .SelectMany(v => v[i]))
                                .ToArray();
                    }
                }
                else
                {
                    foreach (var v in shingleFeatures)
                    {
                        yield return v;
                    }
                }
            }
        }

        /// <summary>
        /// Gets zero or more feature vectors from a <see cref="ISpatialString"/> and vector of <see cref="ComAttribute"/>s
        /// </summary>
        /// <param name="document">The <see cref="ISpatialString"/> to use for auto-bag-of-words features.</param>
        /// <param name="attributes">The collection of <see cref="ComAttribute"/>s containing attributes to be classified</param>
        /// <returns>The feature vectors computed from the input arguments.</returns>
        private IEnumerable<double[]> GetAttributesFeatureVectors(ISpatialString document, List<ComAttribute> attributes)
        {
            ExtractException.Assert("ELI41405", "Attributes to be classified cannot be null", attributes != null);

            int numberOfExamples = attributes.Count;

            // Prevent multiple enumerations (else large VOA files take a long time to process)
            // https://extract.atlassian.net/browse/ISSUE-15605
            var protoFeatureGroups = GetAttributesProtoFeatures(attributes).ToList();

            // NOTE: vectorizers return an empty enumerable if not enabled
            var attributeFeatures = AttributeFeatureVectorizers
                .Select(vectorizer => vectorizer.GetFeatureVectorsForEachGroup(protoFeatureGroups).ToList())
                .ToList();

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
        /// <param name="inputVOAs">The input VOAs corresponding to each uss file</param>
        /// <param name="answers">The categories for each input file</param>
        /// <param name="updateStatus">Function to use for sending progress updates to caller</param>
        /// <param name="cancellationToken">Token indicating that processing should be canceled</param>
        /// <param name="updateAnswerCodes">Whether to update answer code to name mappings to reflect the input</param>
        /// <returns>A tuple of feature vectors, predictions and the uss path for each example</returns>
        private (double[][] featureVectors, int[] answerCodes, string[] ussPathsPerExample) GetDocumentFeatureVectorAndAnswerCollection
            (string[] ussFilePaths, Lazy<IUnknownVector>[] inputVOAs, string[] answers,
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
                bool[] failed = new bool[ussFilePaths.Length];
                bool anyFailed = false;

                // Prevent too much memory consumption when loading large documents
                var opts = new ParallelOptions
                {
                    MaxDegreeOfParallelism = Environment.ProcessorCount
                };
                using (var spatialString = new ThreadLocal<SpatialStringClass>(() => new SpatialStringClass()))
                Parallel.For(0, ussFilePaths.Length, opts, (i, loopState) =>
                {
                    try
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            loopState.Stop();
                        }

                        string answer = answers[i];

                        if (AutoBagOfWords != null)
                        {
                            string uss = ussFilePaths[i];
                            try
                            {
                                spatialString.Value.LoadFrom(uss, false);
                                spatialString.Value.ReportMemoryUsage();
                            }
                            catch (Exception e)
                            {
                                var ue = e.AsExtract("ELI39654");
                                ue.AddDebugData("USS path", uss, false);
                                throw ue;
                            }
                        }

                        IUnknownVector attributes = null;
                        if (AttributeFeatureVectorizers.Any() && inputVOAs != null)
                        {
                            attributes = inputVOAs[i].Value;
                        }

                        double[] featureVector = GetDocumentFeatureVector(spatialString.Value, attributes);

                        int answerCode;
                        if (!AnswerNameToCode.TryGetValue(answer, out answerCode))
                        {
                            answerCode = UnknownOrNegativeCategoryCode;
                        }

                        featureVectors[i] = featureVector;
                        answerCodes[i] = answerCode;

                        updateStatus(new StatusArgs { StatusMessage = "Files processed: {0:N0}", Int32Value = 1 });
                    }
                    catch (Exception ex)
                    {
                        ex.ExtractLog("ELI45791");

                        // Record failure to denote file to skip
                        failed[i] = true;
                        anyFailed = true;
                    }
                });

                cancellationToken.ThrowIfCancellationRequested();

                // If any failed, rebuild the collections to omit them
                if (anyFailed)
                {
                    int numSuccessful = failed.Count(f => !f);
                    var tmpFV = featureVectors;
                    var tmpAC = answerCodes;
                    var tmpFP = ussFilePaths;
                    featureVectors = new double[numSuccessful][];
                    answerCodes = new int[numSuccessful];
                    ussFilePaths = new string[numSuccessful];
                    for (int i = 0, j = 0; i < tmpFV.Length; i++)
                    {
                        if (!failed[i])
                        {
                            featureVectors[j] = tmpFV[i];
                            answerCodes[j] = tmpAC[i];
                            ussFilePaths[j] = tmpFP[i];
                            j++;
                        }
                    }
                }

                return (featureVectors, answerCodes, ussFilePaths);
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
        internal void InitializeAnswerCodeMappings(IEnumerable<string> answers, string negativeCategory = null)
        {
            AnswerCodeToName.Clear();
            AnswerNameToCode.Clear();

            // Add the negative category or an 'other' category
            string otherCategory = negativeCategory ?? UnknownCategoryName;

            AnswerCodeToName.Add(otherCategory);

            AnswerCodeToName.AddRange(answers.Distinct(StringComparer.OrdinalIgnoreCase)
                .Where(s => !s.Equals(otherCategory, StringComparison.OrdinalIgnoreCase))
                .OrderBy(s => s, StringComparer.OrdinalIgnoreCase));

            for (int i = 0; i < AnswerCodeToName.Count; i++)
            {
                AnswerNameToCode[AnswerCodeToName[i]] = i;
            }

            ExtractException.Assert("ELI40251", "There must be at least two categories of input", AnswerNameToCode.Count > 1);
        }

        /// <summary>
        /// Builds a collection of feature vector and answer code tuples. Assumes that this
        /// object has been configured with <see cref="ComputePaginationEncodings"/>
        /// </summary>
        /// <param name="ussFilePaths">The uss paths of each input file</param>
        /// <param name="inputVOAs">The input VOAs corresponding to each uss file</param>
        /// <param name="answerFiles">The VOAs with pagination boundary info for each input file</param>
        /// <param name="updateStatus">Function to use for sending progress updates to caller</param>
        /// <param name="cancellationToken">Token indicating that processing should be canceled</param>
        /// <returns>An array of feature vector, answer and uss path tuples</returns>
        private Tuple<double[], int, string>[] GetPaginationFeatureVectorAndAnswerCollection
            (string[] ussFilePaths, Lazy<IUnknownVector>[] inputVOAs, AttributeOrAnswerCollection attributeOrAnswerCollection,
                Action<StatusArgs> updateStatus,
                CancellationToken cancellationToken)
        {
            try
            {
                var results = new Tuple<double[], int, string>[ussFilePaths.Length][];

                // Prevent too much memory consumption when loading large documents
                var opts = new ParallelOptions
                {
                    MaxDegreeOfParallelism = Environment.ProcessorCount
                };
                using (var spatialString = new ThreadLocal<SpatialStringClass>(() => new SpatialStringClass()))
                Parallel.For(0, ussFilePaths.Length, opts, (i, loopState) =>
                {
                    try
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            loopState.Stop();
                        }

                        if (AutoBagOfWords != null)
                        {
                            string uss = ussFilePaths[i];
                            try
                            {
                                spatialString.Value.LoadFrom(uss, false);
                                spatialString.Value.ReportMemoryUsage();
                            }
                            catch (Exception e)
                            {
                                var ue = e.AsExtract("ELI39689");
                                ue.AddDebugData("USS path", uss, false);
                                throw ue;
                            }
                        }

                        IUnknownVector attributes = null;
                        if (AttributeFeatureVectorizers.Any() && inputVOAs != null)
                        {
                            attributes = inputVOAs[i].Value;
                        }

                        List<double[]> featureVectors =
                            GetPaginationOrDeletionFeatureVectors(spatialString.Value, attributes, true)
                                .ToList();

                        // Get page count so that missing page numbers in the answer VOA can be filled in.
                        var answers = GetAttributes(attributeOrAnswerCollection, i);

                        int pageCount = featureVectors.Count + 1;
                        var expandedAnswers = ExpandPaginationAnswerVOA(answers, pageCount).ToList();

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
                    }
                    catch (Exception ex)
                    {
                        ex.ExtractLog("ELI45788");

                        // Store null to indicate a file to skip
                        results[i] = null;
                    }
                });

                cancellationToken.ThrowIfCancellationRequested();
                return results.Where(a => a != null)
                    .SelectMany(a => a).ToArray();
            }
            catch (Exception e)
            {
                throw e.AsExtract("ELI39713");
            }
        }

        /// <summary>
        /// Builds a collection of feature vector and answer code tuples. Assumes that this
        /// object has been configured with <see cref="ComputeDeletionEncodings"/>
        /// </summary>
        /// <param name="ussFilePaths">The uss paths of each input file</param>
        /// <param name="inputVOAs">The input VOAs corresponding to each uss file</param>
        /// <param name="answerFiles">The VOAs with pagination boundary info for each input file</param>
        /// <param name="updateStatus">Function to use for sending progress updates to caller</param>
        /// <param name="cancellationToken">Token indicating that processing should be canceled</param>
        /// <returns>An array of feature vector, answer and uss path tuples</returns>
        private Tuple<double[], int, string>[] GetDeletionFeatureVectorAndAnswerCollection
            (string[] ussFilePaths, Lazy<IUnknownVector>[] inputVOAs, AttributeOrAnswerCollection attributeOrAnswerCollection,
                Action<StatusArgs> updateStatus,
                CancellationToken cancellationToken)
        {
            try
            {
                var results = new Tuple<double[], int, string>[ussFilePaths.Length][];

                // Prevent too much memory consumption when loading large documents
                var opts = new ParallelOptions
                {
                    MaxDegreeOfParallelism = Environment.ProcessorCount
                };
                using (var spatialString = new ThreadLocal<SpatialStringClass>(() => new SpatialStringClass()))
                Parallel.For(0, ussFilePaths.Length, opts, (i, loopState) =>
                {
                    try
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            loopState.Stop();
                        }

                        if (AutoBagOfWords != null)
                        {
                            string uss = ussFilePaths[i];
                            try
                            {
                                spatialString.Value.LoadFrom(uss, false);
                                spatialString.Value.ReportMemoryUsage();
                            }
                            catch (Exception e)
                            {
                                var ue = e.AsExtract("ELI45841");
                                ue.AddDebugData("USS path", uss, false);
                                throw ue;
                            }
                        }

                        IUnknownVector attributes = null;
                        if (AttributeFeatureVectorizers.Any() && inputVOAs != null)
                        {
                            attributes = inputVOAs[i].Value;
                        }

                        List<double[]> featureVectors =
                            GetPaginationOrDeletionFeatureVectors(spatialString.Value, attributes, false)
                                .ToList();

                        // Get page count so that missing page numbers in the answer VOA can be filled in.
                        int pageCount = featureVectors.Count + 1;

                        var answers = GetAttributes(attributeOrAnswerCollection, i);

                        var expandedAnswers = ExpandDeletionAnswerVOA(answers, pageCount).ToList();

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
                    }
                    catch (Exception ex)
                    {
                        ex.ExtractLog("ELI45789");

                        // Store null to indicate a file to skip
                        results[i] = null;
                    }
                });

                cancellationToken.ThrowIfCancellationRequested();
                return results.Where(a => a != null)
                    .SelectMany(a => a).ToArray();
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
        /// <param name="inputVOAs">The input VOAs corresponding to each uss file</param>
        /// <param name="updateStatus">Function to use for sending progress updates to caller</param>
        /// <param name="cancellationToken">Token indicating that processing should be canceled</param>
        /// <returns>An array of feature vector, answer and uss path tuples</returns>
        private Tuple<double[], int, string>[] GetAttributesFeatureVectorAndAnswerCollection
            (string[] ussFilePaths, Lazy<IUnknownVector>[] inputVOAs,
                Action<StatusArgs> updateStatus,
                CancellationToken cancellationToken)
        {
            try
            {
                ExtractException.Assert("ELI41413", "Input VOA collection cannot be null",
                    inputVOAs != null);

                var results = new Tuple<double[], int, string>[ussFilePaths.Length][];

                // Prevent too much memory consumption when loading large documents
                var opts = new ParallelOptions
                {
                    MaxDegreeOfParallelism = Environment.ProcessorCount
                };
                using (var spatialString = new ThreadLocal<SpatialStringClass>(() => new SpatialStringClass()))
                Parallel.For(0, ussFilePaths.Length, opts, (i, loopState) =>
                {
                    try
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            loopState.Stop();
                        }

                        if (AutoBagOfWords != null)
                        {
                            string uss = ussFilePaths[i];
                            try
                            {
                                spatialString.Value.LoadFrom(uss, false);
                                spatialString.Value.ReportMemoryUsage();
                            }
                            catch (Exception e)
                            {
                                var ue = e.AsExtract("ELI41401");
                                ue.AddDebugData("USS path", uss, false);
                                throw ue;
                            }
                        }

                        var attributes = inputVOAs[i].Value;

                        var answerCodes = new List<int>(attributes.Size());
                        var filteredAttributes = new List<ComAttribute>(attributes.Size());
                        foreach (var attribute in attributes.ToIEnumerable<ComAttribute>())
                        {
                            var categoryAttributes =
                                AttributeMethods.GetAttributesByName(attribute.SubAttributes, CategoryAttributeName);

                            int countOfCategoryAttributes = categoryAttributes.Count();
                            ExtractException.Assert("ELI41412", "There should be zero or one category attribute",
                                countOfCategoryAttributes <= 1,
                                "USS file", ussFilePaths[i],
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

                        List<double[]> featureVectors = GetAttributesFeatureVectors(spatialString.Value, filteredAttributes).ToList();

                        results[i] = new Tuple<double[], int, string>[featureVectors.Count];
                        for (int j = 0; j < featureVectors.Count; j++)
                        {
                            results[i][j] = (Tuple.Create(featureVectors[j], answerCodes[j], ussFilePaths[i]));
                        }

                        updateStatus(new StatusArgs { StatusMessage = "Files processed: {0:N0}", Int32Value = 1 });
                    }
                    catch (Exception ex)
                    {
                        ex.ExtractLog("ELI45790");

                        // Store null to indicate a file to skip
                        results[i] = null;
                    }
                });

                cancellationToken.ThrowIfCancellationRequested();
                return results.Where(a => a != null)
                    .SelectMany(a => a).ToArray();
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
            if (AutoBagOfWords != null && !AutoBagOfWords.UseFeatureHashing)
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
            ExtractException.Assert("ELI46197", "Negative class name must be '" + NotFirstPageCategory + "'",
                string.Equals(NegativeClassName, NotFirstPageCategory, StringComparison.OrdinalIgnoreCase));

            // Indent sub-status messages
            Action<StatusArgs> updateStatus2 = args =>
                {
                    args.Indent++;
                    updateStatus(args);
                };
            // Configure SpatialStringFeatureVectorizer
            if (AutoBagOfWords != null && !AutoBagOfWords.UseFeatureHashing)
            {
                updateStatus(new StatusArgs { StatusMessage = "Computing auto-bag-of-words encodings:" });
                AutoBagOfWords.ComputeEncodingsFromPaginationTrainingData(ussFilePaths, answerFiles, updateStatus2, cancellationToken);
            }

            if (inputVOAFilePaths.Length > 0)
            {
                updateStatus(new StatusArgs { StatusMessage = "Computing attribute feature encodings:" });

                // Configure AttributeFeatureVectorizer collection
                IEnumerable<IEnumerable<NameToProtoFeaturesMap>> pagePairProtofeatureCollection =
                    inputVOAFilePaths.Select(p => GetPaginationProtoFeatures(p));

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
            }

            // Add category names and codes
            ExtractException.Assert("ELI45689", "Internal logic error", AnswerCodeToName.Count == 0);
            AnswerCodeToName.Add(NotFirstPageCategory);
            AnswerNameToCode.Add(NotFirstPageCategory, _NOT_FIRST_PAGE_CATEGORY_CODE);
            AnswerCodeToName.Add(FirstPageCategory);
            AnswerNameToCode.Add(FirstPageCategory, FirstPageCategoryCode);
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
        private void ComputeDeletionEncodings(string[] ussFilePaths, string[] inputVOAFilePaths, string[] answerFiles,
            Action<StatusArgs> updateStatus, CancellationToken cancellationToken)
        {
            ExtractException.Assert("ELI46196", "Negative class name must be '" + NotDeletedPageCategory + "'",
                string.Equals(NegativeClassName, NotDeletedPageCategory, StringComparison.OrdinalIgnoreCase));

            // Indent sub-status messages
            Action<StatusArgs> updateStatus2 = args =>
                {
                    args.Indent++;
                    updateStatus(args);
                };
            // Configure SpatialStringFeatureVectorizer
            if (AutoBagOfWords != null && !AutoBagOfWords.UseFeatureHashing)
            {
                updateStatus(new StatusArgs { StatusMessage = "Computing auto-bag-of-words encodings:" });
                AutoBagOfWords.ComputeEncodingsFromDeletionTrainingData(ussFilePaths, answerFiles, updateStatus2, cancellationToken);
            }

            if (inputVOAFilePaths.Length > 0)
            {
                updateStatus(new StatusArgs { StatusMessage = "Computing attribute feature encodings:" });

                // Configure AttributeFeatureVectorizer collection
                IEnumerable<IEnumerable<NameToProtoFeaturesMap>> pageProtofeatureCollection =
                    inputVOAFilePaths.Select(p => GetPaginationProtoFeatures(p, true));

                // Pass the page count of each image along so that missing pages in the answer VOA can be filled in
                var pageProtofeaturesAndCategories = pageProtofeatureCollection.Zip(answerFiles, (pages, answerFile) =>
                    {
                        var answers = ExpandDeletionAnswerVOA(answerFile, pages.Count());
                        return pages.Zip(answers, (pageProtofeatures, answer) =>
                            new { answer, pageProtofeatures });
                    })
                    .SelectMany(answersForFile => answersForFile);

                Dictionary<string, AttributeFeatureVectorizer> vectorizerMap
                    = new Dictionary<string, AttributeFeatureVectorizer>(StringComparer.OrdinalIgnoreCase);

                // Count each page as a separate document for purposes of TF*IDF score
                int exampleNumber = 0;
                foreach (var labeledExample in pageProtofeaturesAndCategories)
                {
                    ++exampleNumber;
                    cancellationToken.ThrowIfCancellationRequested();

                    foreach (var group in labeledExample.pageProtofeatures)
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
            }

            // Add category names and codes
            ExtractException.Assert("ELI45842", "Internal logic error", AnswerCodeToName.Count == 0);
            AnswerCodeToName.Add(NotDeletedPageCategory);
            AnswerNameToCode.Add(NotDeletedPageCategory, _NOT_DELETED_PAGE_CATEGORY_CODE);
            AnswerCodeToName.Add(DeletedPageCategory);
            AnswerNameToCode.Add(DeletedPageCategory, DeletedPageCategoryCode);
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
                    .SelectMany((voa, i) =>
                    {
                        var labels = CollectLabelsFromLabeledCandidateAttributesFile(voa);
                        if (i % LearningMachineMethods.UpdateFrequency == 0)
                        {
                            updateStatus(new StatusArgs
                            {
                                StatusMessage = "Files processed: {0:N0}",
                                Int32Value = LearningMachineMethods.UpdateFrequency,
                                Indent = 1
                            });
                        }

                        return labels;
                    })
                    .Distinct(StringComparer.OrdinalIgnoreCase) // https://extract.atlassian.net/browse/ISSUE-14761
                    .ToList();

                if (answers.Count % LearningMachineMethods.UpdateFrequency > 0)
                {
                    updateStatus(new StatusArgs
                    {
                        StatusMessage = "Files processed: {0:N0}",
                        Int32Value = answers.Count % LearningMachineMethods.UpdateFrequency,
                        Indent = 1
                    });
                }
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

            // Give helpful exception as to why this this process will fail rather than
            // wait for generic "Unable to successfully compute encodings" exception to be thrown
            ExtractException.Assert("ELI45994", "No features found", AutoBagOfWords != null || vectorizerMap.Any());

            // Add category names and codes
            InitializeAnswerCodeMappings(answers, NegativeClassName);
        }

        // TODO: Implement bag of words
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId="spatialStrings")]
        private void ComputeAttributesEncodings(SpatialString[] spatialStrings, IUnknownVector[] labeledCandidateAttributes)
        {
            // Configure SpatialStringFeatureVectorizer
            List<string> answers = null;
            if (AutoBagOfWords != null)
            {
                //answers = AutoBagOfWords.ComputeEncodingsFromAttributesTrainingData
                //    (spatialStrings, labeledCandidateAttributes)
                //    .ToList();
                throw new NotImplementedException();
            }
            else
            {
                answers = labeledCandidateAttributes
                    .SelectMany(voa =>
                    {
                        var labels = CollectLabelsFromLabeledCandidateAttributes(voa);
                        return labels;
                    })
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();
            }

            // Configure AttributeFeatureVectorizer collection
            IEnumerable<Tuple<string, NameToProtoFeaturesMap>> categoryAndProtoFeaturesCollection =
                labeledCandidateAttributes.SelectMany(GetAttributesProtoFeatures);

            Dictionary<string, AttributeFeatureVectorizer> vectorizerMap
                = new Dictionary<string, AttributeFeatureVectorizer>(StringComparer.OrdinalIgnoreCase);

            // Count each attribute as a separate document for purposes of TF*IDF score
            int exampleNumber = 0;
            foreach (var labeledExample in categoryAndProtoFeaturesCollection)
            {
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
            }
            AttributeFeatureVectorizers = vectorizerMap.Values;

            // Add category names and codes
            InitializeAnswerCodeMappings(answers, NegativeClassName);
        }

        internal static IEnumerable<string> CollectLabelsFromLabeledCandidateAttributes(IUnknownVector attributes)
        {
            return attributes
                .ToIEnumerable<ComAttribute>()
                .Select(attr =>
                {
                    var categoryAttributes =
                        AttributeMethods.GetAttributesByName(attr.SubAttributes, CategoryAttributeName);

                    int countOfCategoryAttributes = categoryAttributes.Count();
                    ExtractException.Assert("ELI41416", "There should be zero or one category attribute",
                        countOfCategoryAttributes <= 1,
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

        /// <summary>
        /// Collects the labels (types) from a candidate attributes file
        /// </summary>
        /// <param name="attributesFilePath">The attributes file path</param>
        /// <returns>A collection of labels for each candidate attribute</returns>
        internal static List<string> CollectLabelsFromLabeledCandidateAttributesFile(string attributesFilePath)
        {
            try
            {
                var attributes = _afUtility.Value.GetAttributesFromFile(attributesFilePath);
                attributes.ReportMemoryUsage();

                // Convert to a list so that any exceptions generated processing the attributes are caught here where the filename debug info will be added
                // https://extract.atlassian.net/browse/ISSUE-15952
                return CollectLabelsFromLabeledCandidateAttributes(attributes).ToList();
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

            if (protoFeatures.ToIEnumerable<ComAttribute>().TryDivideAttributesWithSimpleQuery(
                AttributeFilter, out var matched, out var rest))
            {
                return NegateFilter
                    ? rest
                    : matched;
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

            if (MachineUsage == LearningMachineUsage.AttributeCategorization)
            {
                _negativeClassName = "";
            }
            else if (MachineUsage == LearningMachineUsage.Pagination)
            {
                _negativeClassName = NotFirstPageCategory;
            }
            else if (MachineUsage == LearningMachineUsage.Deletion)
            {
                _negativeClassName = NotDeletedPageCategory;
            }
            else
            {
                _negativeClassName = UnknownCategoryName;
            }

            _answerCodeToNameList = null;
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

            // Initialize code-to-name list for pre-version-3 encoders
            if (_version < 3)
            {
#pragma warning disable CS0618 // Type or member is obsolete
                _answerCodeToNameList = _answerCodeToName
                    .OrderBy(kv => kv.Key)
                    .Select(kv => kv.Value)
                    .ToList();

                // Don't need the source dictionary anymore
                _answerCodeToName = null;

                // Clear unused answer-name-to-code dictionary (can't make it nonserialized since old versions may be
                // original values of strings referenced in other places)
                _answerNameToCode = null;
#pragma warning restore CS0618 // Type or member is obsolete
            }

            // Create the non-serialized name-to-code dictionary
            _nonSerializedAnswerNameToCode = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < _answerCodeToNameList.Count; i++)
            {
                _nonSerializedAnswerNameToCode[_answerCodeToNameList[i]] = i;
            }

            _version = _CURRENT_VERSION;
        }

        #endregion Private Methods
    }
}