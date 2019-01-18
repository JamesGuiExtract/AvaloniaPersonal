using Extract.Interop;
using Extract.Licensing;
using Extract.Utilities;
using opennlp.tools.namefind;
using opennlp.tools.sentdetect;
using opennlp.tools.tokenize;
using opennlp.tools.util.model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using UCLID_AFCORELib;
using UCLID_COMLMLib;
using UCLID_COMUTILSLib;
using UCLID_RASTERANDOCRMGMTLib;

namespace Extract.AttributeFinder.Rules
{
    /// <summary>
    /// Type of name finder
    /// </summary>
    [ComVisible(true)]
    public enum NamedEntityRecognizer
    {
        None = 0,
        OpenNLP = 1,
        Stanford = 2
    }

    /// <summary>
    /// Type of tokenizer to use prior to name finding
    /// </summary>
    [ComVisible(true)]
    public enum OpenNlpTokenizer
    {
        None = 0,
        WhiteSpaceTokenizer = 1,
        SimpleTokenizer = 2,
        LearnableTokenizer = 3
    }

    /// <summary>
    /// An interface for the <see cref="NERFinder"/> class.
    /// </summary>
    [ComVisible(true)]
    [Guid("C2E3E6C1-91C0-4CA1-8553-C794D7661A1D")]
    [CLSCompliant(false)]
    public interface INERFinder : IAttributeFindingRule, ICategorizedComponent,
        IConfigurableObject, ICopyableObject, ILicensedComponent, IPersistStream,
        IMustBeConfiguredObject, IIdentifiableObject
    {
        /// <summary>
        /// The type of NER to use
        /// </summary>
        NamedEntityRecognizer NameFinderType { get; set; }

        /// <summary>
        /// Wether to divide the input into sentences prior to tokenizing
        /// </summary>
        bool SplitIntoSentences { get; set; }

        /// <summary>
        /// Path to sentence detector model file
        /// </summary>
        string SentenceDetectorPath { get; set; }

        /// <summary>
        /// Type of tokenizer to use
        /// </summary>
        OpenNlpTokenizer TokenizerType { get; set; }

        /// <summary>
        /// Path to tokenizer model file (used when <see cref="TokenizerType"/> is <see cref="OpenNlpTokenizer.LearnableTokenizer"/>)
        /// </summary>
        /// <remarks>Can contain path tags/functions</remarks>
        string TokenizerPath { get; set; }

        /// <summary>
        /// Path to classifier model file
        /// </summary>
        /// <remarks>Can contain path tags/functions</remarks>
        string NameFinderPath { get; set; }

        /// <summary>
        /// The entity types to be used to create the resulting attributes (a subset of the entity types recognized by the classifier)
        /// </summary>
        string EntityTypes { get; set; }

        /// <summary>
        /// Whether to output a subattribute to indicate model confidence in the attribute
        /// </summary>
        bool OutputConfidenceSubAttribute { get; set; }

        /// <summary>
        /// Whether to apply the logistic function to the confidence value in order
        /// to make it prettier (e.g., make average values higher)
        /// </summary>
        bool ApplyLogFunctionToConfidence { get; set; }

        /// <summary>
        /// The base for the log function
        /// </summary>
        double LogBase { get; set; }

        /// <summary>
        /// The steepness of the log function
        /// </summary>
        double LogSteepness { get; set; }

        /// <summary>
        /// The x value of the middle of the sigmoid curve
        /// </summary>
        double LogXValueOfMiddle { get; set; }

        /// <summary>
        /// Whether to convert the confidence value from a real number between 0 and 1 to
        /// an integer percent
        /// </summary>
        bool ConvertConfidenceToPercent { get; set; }
    }

    /// <summary>
    /// An <see cref="IAttributeFindingRule"/> that uses OpenNLP named entity recognition to find attributes
    /// </summary>
    [ComVisible(true)]
    [Guid("9BC947D6-FFCE-4E65-9952-826173EAA8FB")]
    [CLSCompliant(false)]
    public class NERFinder : IdentifiableObject, INERFinder
    {
        #region Constants

        /// <summary>
        /// The description of the rule.
        /// </summary>
        const string _COMPONENT_DESCRIPTION = "Named entity recognition finder";

        /// <summary>
        /// Current version.
        /// </summary>
        /// <remarks>
        /// Version 2: Added OutputConfidenceSubattribute and related settings
        /// </remarks>
        const int _CURRENT_VERSION = 2;

        /// <summary>
        /// The license id to validate in licensing calls
        /// </summary>
        const LicenseIdName _LICENSE_ID = LicenseIdName.FlexIndexIDShieldCoreObjects;

        #endregion Constants

        #region Fields

        /// <summary>
        /// An <see cref="AttributeFinderPathTags"/> to expand any tags in the models dir
        /// </summary>
        AttributeFinderPathTags _pathTags = new AttributeFinderPathTags();

        /// <summary>
        /// <see langword="true"/> if changes have been made to this instance since it was created;
        /// <see langword="false"/> if no changes have been made since it was created.
        /// </summary>
        bool _dirty;

        // Backing fields for properties
        private string _classifierPath;
        private string _tokenizerPath;
        private string _entityTypes;
        private bool _splitIntoLines;
        private string _sentenceDetectorPath;
        private OpenNlpTokenizer _tokenizerType = OpenNlpTokenizer.SimpleTokenizer;
        private NamedEntityRecognizer _nameFinderType = NamedEntityRecognizer.OpenNLP;
        private bool _outputConfidenceSubattribute = false;
        private bool _applyLogFunctionToConfidence = false;
        private double _logBase = 2;
        private double _logSteepness = 10;
        private bool _convertConfidenceToPercent = true;
        private double _logXValueOfMiddle = 0.1;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="NERFinder"/> class.
        /// </summary>
        public NERFinder()
        {
            try
            {
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI44744");
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NERFinder"/> class as a
        /// copy of <see paramref="source"/>.
        /// </summary>
        /// <param name="source">The <see cref="NERFinder"/> from which
        /// settings should be copied.</param>
        public NERFinder(NERFinder source)
        {
            try
            {
                CopyFrom(source);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI44745");
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// The type of NER to use
        /// </summary>
        public NamedEntityRecognizer NameFinderType
        {
            get
            {
                return _nameFinderType;
            }
            set
            {
                if (value != _nameFinderType)
                {
                    _nameFinderType = value;
                    _dirty = true;
                }
            }
        }

        /// <summary>
        /// Wether to divide the input into sentences prior to tokenizing
        /// </summary>
        public bool SplitIntoSentences
        {
            get
            {
                return _splitIntoLines;
            }
            set
            {
                if (value != _splitIntoLines)
                {
                    _splitIntoLines = value;
                    _dirty = true;
                }
            }
        }

        /// <summary>
        /// Path to sentence detector model file
        /// </summary>
        public string SentenceDetectorPath
        {
            get
            {
                return _sentenceDetectorPath;
            }
            set
            {
                if (!string.Equals(value, _sentenceDetectorPath, StringComparison.Ordinal))
                {
                    _sentenceDetectorPath = value;
                    _dirty = true;
                }
            }
        }

        /// <summary>
        /// Type of tokenizer to use
        /// </summary>
        public OpenNlpTokenizer TokenizerType
        {
            get
            {
                return _tokenizerType;
            }
            set
            {
                if (value != _tokenizerType)
                {
                    _tokenizerType = value;
                    _dirty = true;
                }
            }
        }

        /// <summary>
        /// The location of the tokenizer model file
        /// </summary>
        /// <remarks>Can contain path tags/functions</remarks>
        public string TokenizerPath
        {
            get
            {
                return _tokenizerPath;
            }
            set
            {
                if (!string.Equals(value, _tokenizerPath, StringComparison.Ordinal))
                {
                    _tokenizerPath = value;
                    _dirty = true;
                }
            }
        }

        /// <summary>
        /// The location of the classifier model file
        /// </summary>
        /// <remarks>Can contain path tags/functions</remarks>
        public string NameFinderPath
        {
            get
            {
                return _classifierPath;
            }
            set
            {
                if (!string.Equals(value, _classifierPath, StringComparison.Ordinal))
                {
                    _classifierPath = value;
                    _dirty = true;
                }
            }
        }

        /// <summary>
        /// The entity types to be used to create the resulting attributes (a subset of the entity types recognized by the classifier)
        /// </summary>
        public string EntityTypes
        {
            get
            {
                return _entityTypes;
            }
            set
            {
                if (!string.Equals(value, _entityTypes, StringComparison.Ordinal))
                {
                    _entityTypes = value;
                    _dirty = true;
                }
            }
        }

        /// <summary>
        /// Whether to output a subattribute to indicate model confidence in the attribute
        /// </summary>
        public bool OutputConfidenceSubAttribute
        {
            get
            {
                return _outputConfidenceSubattribute;
            }
            set
            {
                if (value != _outputConfidenceSubattribute)
                {
                    _outputConfidenceSubattribute = value;
                    _dirty = true;
                }
            }
        }

        /// <summary>
        /// Whether to apply the logistic function to the confidence value in order
        /// to make it prettier (e.g., make average values higher)
        /// </summary>
        public bool ApplyLogFunctionToConfidence
        {
            get
            {
                return _applyLogFunctionToConfidence;
            }
            set
            {
                if (value != _applyLogFunctionToConfidence)
                {
                    _applyLogFunctionToConfidence = value;
                    _dirty = true;
                }
            }
        }

        /// <summary>
        /// The base for the log function
        /// </summary>
        public double LogBase
        {
            get
            {
                return _logBase;
            }
            set
            {
                if (value != _logBase)
                {
                    _logBase = value;
                    _dirty = true;
                }
            }
        }

        /// <summary>
        /// The steepness of the log function
        /// </summary>
        public double LogSteepness
        {
            get
            {
                return _logSteepness;
            }
            set
            {
                if (value != _logSteepness)
                {
                    _logSteepness = value;
                    _dirty = true;
                }
            }
        }

        /// <summary>
        /// Whether to convert the confidence value from a real number between 0 and 1 to
        /// an integer percent
        /// </summary>
        public bool ConvertConfidenceToPercent
        {
            get
            {
                return _convertConfidenceToPercent;
            }
            set
            {
                if (value != _convertConfidenceToPercent)
                {
                    _convertConfidenceToPercent = value;
                    _dirty = true;
                }
            }
        }

        /// <summary>
        /// The x value of the middle of the sigmoid curve
        /// </summary>
        public double LogXValueOfMiddle
        {
            get
            {
                return _logXValueOfMiddle;
            }
            set
            {
                if (value != _logXValueOfMiddle)
                {
                    _logXValueOfMiddle = value;
                    _dirty = true;
                }
            }
        }

        #endregion Properties

        #region IAttributeFindingRule

        /// <summary>
        /// Parses the <see paramref="pDocument"/> and returns a vector of found
        /// <see cref="IAttribute"/> objects.
        /// </summary>
        /// <param name="pDocument">The <see cref="AFDocument"/> to parse.</param>
        /// <param name="pProgressStatus">The <see cref="ProgressStatus"/> to indicate processing
        /// progress.</param>
        /// <returns>An <see cref="IUnknownVector"/> of found <see cref="IAttribute"/>s.</returns>
        public IUnknownVector ParseText(AFDocument pDocument, ProgressStatus pProgressStatus)
        {
            try
            {
                LicenseUtilities.ValidateLicense(_LICENSE_ID, "ELI44746", _COMPONENT_DESCRIPTION);

                // So that the garbage collector knows of and properly manages the associated
                // memory.
                pDocument.Attribute.ReportMemoryUsage();

                // Initialize for use in any embedded path tags/functions.
                _pathTags.Document = pDocument;

                var input = pDocument.Text;
                var typesToReturn = string.IsNullOrWhiteSpace(EntityTypes)
                    ? null
                    : EntityTypes.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries).ToDictionary(t => t, StringComparer.OrdinalIgnoreCase);

                IUnknownVector returnValue = null;

                if (NameFinderType == NamedEntityRecognizer.OpenNLP)
                {

                    returnValue = FindNames(input, typesToReturn);
                }
                else
                {
                    throw new ExtractException("ELI45539","Unsupported NamedEntityRecognizer: "
                        + NameFinderType.ToString());
                }

                // So that the garbage collector knows of and properly manages the associated
                // memory from the created return value.
                returnValue.ReportMemoryUsage();

                return returnValue;
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI44747", "Failed to find names.");
            }
        }

        #endregion IAttributeFindingRule

        #region IConfigurableObject Members

        /// <summary>
        /// Displays a form to allow configuration of this <see cref="NERFinder"/>
        /// instance.
        /// </summary>
        /// <returns><see langword="true"/> if the configuration was successfully updated or
        /// <see langword="false"/> if configuration was unsuccessful.</returns>
        public bool RunConfiguration()
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(_LICENSE_ID, "ELI44748", _COMPONENT_DESCRIPTION);

                // Make a clone to update settings and only copy if ok
                NERFinder cloneOfThis = (NERFinder)Clone();

                using (NERFinderSettingsDialog dlg
                    = new NERFinderSettingsDialog(cloneOfThis))
                {
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        CopyFrom(dlg.Settings);
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI44749", "Error running configuration.");
            }
        }

        #endregion IConfigurableObject Members

        #region IMustBeConfiguredObject Members

        /// <summary>
        /// Determines whether this instance is configured.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if this instance is configured; otherwise,
        /// <see langword="false"/>.
        /// </returns>
        public bool IsConfigured()
        {
            try
            {
                return !string.IsNullOrWhiteSpace(NameFinderPath)
                    && !(TokenizerType == OpenNlpTokenizer.LearnableTokenizer && string.IsNullOrWhiteSpace(TokenizerPath));
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI44750",
                    "Error checking configuration of NER finder.");
            }
        }

        #endregion IMustBeConfiguredObject Members

        #region ICopyableObject Members

        /// <summary>
        /// Creates a copy of the <see cref="NERFinder"/> instance.
        /// </summary>
        /// <returns>A copy of the <see cref="NERFinder"/> instance.
        /// </returns>
        public object Clone()
        {
            try
            {
                return new NERFinder(this);
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI44751",
                    "Failed to clone '" + _COMPONENT_DESCRIPTION + "' object.");
            }
        }

        /// <summary>
        /// Copies the specified <see cref="NERFinder"/> instance into this one.
        /// </summary>
        /// <param name="pObject">The object from which to copy.</param>
        public void CopyFrom(object pObject)
        {
            try
            {
                var source = pObject as NERFinder;
                if (source == null)
                {
                    throw new InvalidCastException("Invalid cast to NERFinder");
                }
                CopyFrom(source);
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI44752",
                    "Failed to copy '" + _COMPONENT_DESCRIPTION + "' object.");
            }
        }

        #endregion ICopyableObject Members

        #region ICategorizedComponent Members

        /// <summary>
        /// Gets the name of the COM object.
        /// </summary>
        /// <returns>The name of the COM object.</returns>
        public string GetComponentDescription()
        {
            return _COMPONENT_DESCRIPTION;
        }

        #endregion ICategorizedComponent Members

        #region ILicensedComponent Members

        /// <summary>
        /// Gets whether this component is licensed.
        /// </summary>
        /// <returns><see langword="true"/> if the component is licensed; <see langword="false"/> 
        /// if the component is not licensed.</returns>
        public bool IsLicensed()
        {
            return LicenseUtilities.IsLicensed(_LICENSE_ID);
        }

        #endregion ILicensedComponent Members

        #region IPersistStream Members

        /// <summary>
        /// Returns the class identifier (CLSID) <see cref="Guid"/> for the component object.
        /// </summary>
        /// <param name="classID">Pointer to the location of the CLSID <see cref="Guid"/> on 
        /// return.</param>
        public void GetClassID(out Guid classID)
        {
            classID = GetType().GUID;
        }

        /// <summary>
        /// Checks the object for changes since it was last saved.
        /// </summary>
        /// <returns><see cref="HResult.Ok"/> if changes have been made;
        /// <see cref="HResult.False"/> if changes have not been made.
        /// </returns>
        public int IsDirty()
        {
            return HResult.FromBoolean(_dirty);
        }

        /// <summary>
        /// Initializes an object from the IStream where it was previously saved.
        /// </summary>
        /// <param name="stream">IStream from which the object should be loaded.</param>
        public void Load(System.Runtime.InteropServices.ComTypes.IStream stream)
        {
            try
            {
                using (IStreamReader reader = new IStreamReader(stream, _CURRENT_VERSION))
                {
                    NameFinderType = (NamedEntityRecognizer)reader.ReadInt32();

                    SplitIntoSentences = reader.ReadBoolean();
                    SentenceDetectorPath = reader.ReadString();

                    TokenizerType = (OpenNlpTokenizer)reader.ReadInt32();
                    TokenizerPath = reader.ReadString();

                    NameFinderPath = reader.ReadString();
                    EntityTypes = reader.ReadString();

                    if (reader.Version >=2)
                    {
                        OutputConfidenceSubAttribute = reader.ReadBoolean();
                        ApplyLogFunctionToConfidence = reader.ReadBoolean();
                        LogBase = reader.ReadDouble();
                        LogSteepness = reader.ReadDouble();
                        LogXValueOfMiddle = reader.ReadDouble();
                        ConvertConfidenceToPercent = reader.ReadBoolean();
                    }
                    else
                    {
                        OutputConfidenceSubAttribute = false;
                    }

                    // Load the GUID for the IIdentifiableObject interface.
                    LoadGuid(stream);
                }

                // Freshly loaded object is no longer dirty
                _dirty = false;
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI44753",
                    "Failed to load '" + _COMPONENT_DESCRIPTION + "'.");
            }
        }

        /// <summary>
        /// Saves an object into the specified IStream and indicates whether the object should reset
        /// its dirty flag.
        /// </summary>
        /// <param name="stream">IStream into which the object should be saved.</param>
        /// <param name="clearDirty">Value that indicates whether to clear the dirty flag after the
        /// save is complete. If <see langword="true"/>, the flag should be cleared. If
        /// <see langword="false"/>, the flag should be left unchanged.</param>
        public void Save(System.Runtime.InteropServices.ComTypes.IStream stream, bool clearDirty)
        {
            try
            {
                using (IStreamWriter writer = new IStreamWriter(_CURRENT_VERSION))
                {
                    writer.Write((int)NameFinderType);

                    writer.Write(SplitIntoSentences);
                    writer.Write(SentenceDetectorPath);

                    writer.Write((int)TokenizerType);
                    writer.Write(TokenizerPath);

                    writer.Write(NameFinderPath);
                    writer.Write(EntityTypes);

                    writer.Write(OutputConfidenceSubAttribute);
                    writer.Write(ApplyLogFunctionToConfidence);
                    writer.Write(LogBase);
                    writer.Write(LogSteepness);
                    writer.Write(LogXValueOfMiddle);
                    writer.Write(ConvertConfidenceToPercent);

                    // Write to the provided IStream.
                    writer.WriteTo(stream);
                }

                // Save the GUID for the IIdentifiableObject interface.
                SaveGuid(stream);

                if (clearDirty)
                {
                    _dirty = false;
                }
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI44754",
                    "Failed to save '" + _COMPONENT_DESCRIPTION + "'.");
            }
        }

        /// <summary>
        /// Returns the size in bytes of the stream needed to save the object.
        /// </summary>
        /// <param name="size">Pointer to a 64-bit unsigned integer value indicating the size, in
        /// bytes, of the stream needed to save this object.</param>
        public void GetSizeMax(out long size)
        {
            throw new NotImplementedException();
        }

        #endregion IPersistStream Members

        #region Private Members

        /// <summary>
        /// Searches the input text for names using the given models
        /// </summary>
        /// <param name="text">The text to search</param>
        /// <param name="tokenizerModel">The path to a learnable tokenizer model file</param>
        /// <param name="nameModel">The path to a learnable name finder model file</param>
        /// <param name="typesToReturn">A dictionary of case-insensitive class names to case-sensitive types to be output</param>
        /// <returns>Any found names or an empty vector if no names are found</returns>
        IUnknownVector FindNames(SpatialString text, Dictionary<string, string> typesToReturn)
        {
            try
            {

                string sentDetectPath = null;
                SentenceDetectorME sentenceDetector = null;
                if (SplitIntoSentences)
                {
                    try
                    {
                        sentDetectPath = _pathTags.Expand(SentenceDetectorPath);
                        var sentenceModel = GetModel(sentDetectPath, modelIn => new SentenceModel(modelIn));
                        sentenceDetector = new SentenceDetectorME(sentenceModel);
                    }
                    catch (Exception inner)
                    {
                        ExtractException ex = new ExtractException("ELI44811", "Could not load sentence detector model", inner);
                        ex.AddDebugData("Model path", sentDetectPath, false);
                        throw ex;
                    }
                }

                string tokenizerPath = null;
                Tokenizer tokenizer = null;
                switch (TokenizerType)
                {
                    case OpenNlpTokenizer.WhiteSpaceTokenizer:
                        tokenizer = WhitespaceTokenizer.INSTANCE;
                        break;
                    case OpenNlpTokenizer.SimpleTokenizer:
                        tokenizer = SimpleTokenizer.INSTANCE;
                        break;
                    case OpenNlpTokenizer.LearnableTokenizer:
                        try
                        {
                            tokenizerPath = _pathTags.Expand(TokenizerPath);
                            var tokenModel = GetModel(tokenizerPath, modelIn => new TokenizerModel(modelIn));
                            tokenizer = new TokenizerME(tokenModel);
                        }
                        catch (Exception inner)
                        {
                            ExtractException ex = new ExtractException("ELI44812", "Could not load tokenizer model", inner);
                            ex.AddDebugData("Model path", tokenizerPath, false);
                            throw ex;
                        }
                        break;
                };

                var finderPath = _pathTags.Expand(NameFinderPath);
                NameFinderME nameFinder = null;
                try
                {
                    var finderModel = GetModel(finderPath, modelIn => new TokenNameFinderModel(modelIn));
                    nameFinder = new NameFinderME(finderModel);
                }
                catch (Exception inner)
                {
                    ExtractException ex = new ExtractException("ELI44814", "Could not load name finder model", inner);
                    ex.AddDebugData("Model path", finderPath, false);
                    throw ex;
                }

                var result = new IUnknownVectorClass();
                var inputString = text.String;
                var sentences = sentenceDetector == null
                    ? Enumerable.Repeat((offset: 0, sentence: inputString), 1)
                    : sentenceDetector.sentPosDetect(inputString)
                        .Select(sentenceSpan =>
                        {
                            var start = sentenceSpan.getStart();
                            var end = sentenceSpan.getEnd();
                            return (offset: start, sentence: inputString.Substring(start, end - start));
                        });

                foreach (var (offset, sentence) in sentences)
                {
                    opennlp.tools.util.Span[] tokenPositions = tokenizer.tokenizePos(sentence);

                    // The implementation of AbstractTokenizer.tokenize repeats the above call and then calls spansToStrings
                    // so call that directly to avoid repeating the work
                    string[] tokens = opennlp.tools.util.Span.spansToStrings(tokenPositions, sentence);

                    opennlp.tools.util.Span[] nameSpans = nameFinder.find(tokens);

                    double probability = 0;
                    foreach (var span in nameSpans)
                    {
                        var type = span.getType();

                        // Limit to specified types, if any
                        // Use letter case of type provided by user
                        if (typesToReturn == null || typesToReturn.TryGetValue(type, out type))
                        {
                            // Find char offsets (end indices are exclusive)
                            int start = tokenPositions[span.getStart()].getStart() + offset;
                            int end = tokenPositions[span.getEnd() - 1].getEnd() - 1 + offset;

                            var value = text.GetSubString(start, end);
                            var at = new AttributeClass
                            {
                                Value = value,
                                Type = type
                            };
                            result.PushBack(at);

                            if (OutputConfidenceSubAttribute)
                            {
                                probability = span.getProb();
                                var confidenceSpatialString = new SpatialString();

                                if (ApplyLogFunctionToConfidence)
                                {
                                    probability = 1 / (1 + Math.Pow(LogBase, -LogSteepness * (probability-LogXValueOfMiddle)));
                                }

                                if (ConvertConfidenceToPercent)
                                {
                                    int conf = (int)Math.Round(100 * probability);
                                    confidenceSpatialString.CreateNonSpatialString(
                                        conf.ToString("G", CultureInfo.CurrentCulture),
                                        text.SourceDocName);
                                }
                                else
                                {
                                    confidenceSpatialString.CreateNonSpatialString(
                                        probability.ToString("r", CultureInfo.CurrentCulture),
                                        text.SourceDocName);
                                }

                                at.SubAttributes.PushBack(new AttributeClass
                                {
                                    Name = "Confidence",
                                    Value = confidenceSpatialString
                                });
                            }
                        }
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI44810");
            }
        }

        /// <summary>
        /// Get/cache a, possibly encrypted, tokenizer, sentence detector or name finder model
        /// </summary>
        /// <typeparam name="TModel">The model type</typeparam>
        /// <param name="modelPath">The path to the model (.bin or .etf)</param>
        /// <param name="thunk">The function to be used to instantiate the model from a stream</param>
        /// <returns>An instance of the model</returns>
        /// <remarks>The default <see cref="MemoryCache"/> will be used to store the loaded model.
        /// The cache will be updated if <see paramref="modelPath"/> is modified or if the unencrypted
        /// file (modelPath without '.etf') is modified.</remarks>
        [ComVisible(false)]
        public static TModel GetModel<TModel>(string modelPath, Func<java.io.InputStream, TModel> thunk)
            where TModel:BaseModel
        {
            try
            {
                var model = FileDerivedResourceCache.GetCachedObject(
                    paths: modelPath,
                    creator: () =>
                    {
                        var str = FileDerivedResourceCache.ThreadLocalMiscUtils.GetBase64StringFromFile(modelPath);
                        var bytes = Convert.FromBase64String(str);
                        var modelIn = new java.io.ByteArrayInputStream(bytes);
                        return thunk(modelIn);
                    });

                return model;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI44903");
            }
        }

        /// <summary>
        /// Code to be executed upon registration in order to add this class to the
        /// appropriate COM categories.
        /// </summary>
        /// <param name="type">The <paramref name="type"/> being registered.</param>
        [ComRegisterFunction]
        [ComVisible(false)]
        static void RegisterFunction(Type type)
        {
            ComMethods.RegisterTypeInCategory(type, ExtractCategories.ValueFindersGuid);
        }

        /// <summary>
        /// Code to be executed upon unregistration in order to remove this class from the
        /// appropriate COM categories.
        /// </summary>
        /// <param name="type">The <paramref name="type"/> being unregistered.</param>
        [ComUnregisterFunction]
        [ComVisible(false)]
        static void UnregisterFunction(Type type)
        {
            ComMethods.UnregisterTypeInCategory(type, ExtractCategories.ValueFindersGuid);
        }

        /// <summary>
        /// Copies the specified <see cref="NERFinder"/> instance into this one.
        /// </summary><param name="source">The <see cref="NERFinder"/> from which to copy.
        /// </param>
        void CopyFrom(NERFinder source)
        {
            NameFinderType = source.NameFinderType;
            SplitIntoSentences = source.SplitIntoSentences;
            SentenceDetectorPath = source.SentenceDetectorPath;
            TokenizerType = source.TokenizerType;
            TokenizerPath = source.TokenizerPath;
            NameFinderPath = source.NameFinderPath;
            EntityTypes = source.EntityTypes;
            OutputConfidenceSubAttribute = source.OutputConfidenceSubAttribute;
            ApplyLogFunctionToConfidence = source.ApplyLogFunctionToConfidence;
            LogBase = source.LogBase;
            LogSteepness = source.LogSteepness;
            LogXValueOfMiddle = source.LogXValueOfMiddle;
            ConvertConfidenceToPercent = source.ConvertConfidenceToPercent;

            _dirty = true;
        }

        #endregion Private Members
    }
}