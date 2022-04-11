using Extract.Interop;
using Extract.Licensing;
using Extract.Utilities;
using Lucene.Net.Analysis.Synonym;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using UCLID_AFCORELib;
using UCLID_AFSELECTORSLib;
using UCLID_AFUTILSLib;
using UCLID_COMLMLib;
using UCLID_COMUTILSLib;
using ComAttribute = UCLID_AFCORELib.Attribute;

namespace Extract.AttributeFinder.Rules
{
    /// <summary>
    /// An interface for the <see cref="TranslateValueToBestMatch"/> class.
    /// </summary>
    [ComVisible(true)]
    [Guid("EF2840CC-A12E-48FA-A9AF-F0C149F9CD4B")]
    [CLSCompliant(false)]
    public interface ITranslateValueToBestMatch : IOutputHandler, ICategorizedComponent,
        IConfigurableObject, ICopyableObject, ILicensedComponent, IPersistStream,
        IMustBeConfiguredObject, IIdentifiableObject
    {

        /// <summary>
        /// Gets or sets the <see cref="IAttributeSelector"/> used to specify which attribute(s)
        /// are to be translated.
        /// </summary>
        /// <value>
        /// The <see cref="IAttributeSelector"/> used to specify which attribute(s) are to be translated.
        /// </value>
        IAttributeSelector AttributeSelector { get; set; }

        /// <summary>
        /// The path to the (optionally) encrypted list of translate-to targets
        /// </summary>
        string SourceListPath { get; set; }

        /// <summary>
        /// Optional path to the (optionally) encrypted synonym CSV
        /// </summary>
        string SynonymMapPath { get; set; }

        /// <summary>
        /// The minimum score that a match needs for translation to occur
        /// </summary>
        double MinimumMatchScore { get; set; }

        /// <summary>
        /// Action to take if no good match is found
        /// </summary>
        NoGoodMatchAction UnableToTranslateAction { get; set; }

        /// <summary>
        /// Whether to create a subattribute with the best match score as a value
        /// </summary>
        bool CreateBestMatchScoreSubAttribute { get; set; }
    }

    [ComVisible(true)]
    public enum NoGoodMatchAction
    {
        DoNothing = 0,
        ClearValue = 1,
        RemoveAttribute = 2,
        SetTypeToUntranslated = 3,
    }

    /// <summary>
    /// An <see cref="IOutputHandler"/> that translated attribute values to the best match in a list
    /// </summary>
    [ComVisible(true)]
    [Guid("9EA6FF84-DC89-4F18-AC2B-93E507A5404B")]
    [CLSCompliant(false)]
    public class TranslateValueToBestMatch : IdentifiableObject, ITranslateValueToBestMatch
    {
        #region Constants

        /// <summary>
        /// The description of the rule object.
        /// </summary>
        const string _COMPONENT_DESCRIPTION = "Translate value to best match";

        /// <summary>
        /// Current version.
        /// </summary>
        const int _CURRENT_VERSION = 1;

        /// <summary>
        /// The license id to validate in licensing calls
        /// </summary>
        const LicenseIdName _LICENSE_ID = LicenseIdName.FlexIndexIDShieldCoreObjects;

        #endregion Constants

        #region Fields

        /// <summary>
        /// <see langword="true"/> if changes have been made to this instance since it was created;
        /// <see langword="false"/> if no changes have been made since it was created.
        /// </summary>
        bool _dirty;

        /// <summary>
        /// The <see cref="IAttributeSelector"/> used to specify which attribute(s) are to be translated.
        /// </summary>
        IAttributeSelector _attributeSelector;

        /// <summary>
        /// The path to the (optionally) encrypted list of translate-to targets
        /// </summary>
        string _sourceListPath;

        /// <summary>
        /// Optional path to the (optionally) encrypted synonym CSV
        /// </summary>
        string _synonymMapPath;

        /// <summary>
        /// The minimum score that a match needs for translation to occur
        /// </summary>
        double _minimumMatchScore;

        /// <summary>
        /// The action to take when there is no good match
        /// </summary>
        NoGoodMatchAction _unableToTranslateAction;

        /// <summary>
        /// Whether to create a subattribute with the best match score as a value
        /// </summary>
        bool _createBestMatchScoreSubattribute;

        /// <summary>
        /// An <see cref="AttributeFinderPathTags"/> to expand any tags in the source lists paths
        /// </summary>
        AttributeFinderPathTags _pathTags = new AttributeFinderPathTags();

        /// <summary>
        /// Used to locate the parent of attributes to be deleted
        /// </summary>
        AFUtility _afUtility;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="TranslateValueToBestMatch"/> class.
        /// </summary>
        public TranslateValueToBestMatch()
        {
            try
            {
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45452");
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TranslateValueToBestMatch"/> class as a
        /// copy of <see paramref="source"/>.
        /// </summary>
        /// <param name="source">The <see cref="TranslateValueToBestMatch"/> from which
        /// settings should be copied.</param>
        public TranslateValueToBestMatch(TranslateValueToBestMatch source)
        {
            try
            {
                CopyFrom(source);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45453");
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets an <see cref="AFUtility"/> instance.
        /// </summary>
        AFUtility AFUtility => _afUtility = _afUtility ?? new AFUtilityClass();

        /// <summary>
        /// Gets or sets the <see cref="IAttributeSelector"/> used to specify which attribute(s)
        /// are to be translated.
        /// </summary>
        /// <value>
        /// The <see cref="IAttributeSelector"/> used to specify which attribute(s) are to be translated.
        /// </value>
        public IAttributeSelector AttributeSelector
        {
            get
            {
                if (_attributeSelector == null)
                {
                    _attributeSelector = (IAttributeSelector)
                        new QueryBasedAS { QueryText = "*" };
                }
                return _attributeSelector;
            }

            set
            {
                try
                {
                    if (_attributeSelector != value)
                    {
                        _attributeSelector = value;

                        _dirty = true;
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI45454");
                }
            }
        }

        /// <summary>
        /// The path to the (optionally) encrypted list of translate-to targets
        /// </summary>
        public string SourceListPath
        {
            get
            {
                return _sourceListPath;
            }

            set
            {
                try
                {
                    if (_sourceListPath != value)
                    {
                        _sourceListPath = value;

                        _dirty = true;
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI45455");
                }
            }
        }

        /// <summary>
        /// Optional path to the (optionally) encrypted synonym CSV
        /// </summary>
        public string SynonymMapPath
        {
            get
            {
                return _synonymMapPath;
            }

            set
            {
                try
                {
                    if (_synonymMapPath != value)
                    {
                        _synonymMapPath = value;

                        _dirty = true;
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI45456");
                }
            }
        }

        /// <summary>
        /// The minimum score that a match needs for translation to occur
        /// </summary>
        public double MinimumMatchScore
        {
            get
            {
                return _minimumMatchScore;
            }

            set
            {
                try
                {
                    if (_minimumMatchScore != value)
                    {
                        _minimumMatchScore = value;

                        _dirty = true;
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI45481");
                }
            }
        }

        /// <summary>
        /// Action to take if no good match is found
        /// </summary>
        public NoGoodMatchAction UnableToTranslateAction
        {
            get
            {
                return _unableToTranslateAction;
            }

            set
            {
                try
                {
                    if (_unableToTranslateAction != value)
                    {
                        _unableToTranslateAction = value;

                        _dirty = true;
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI45482");
                }
            }
        }

        /// <summary>
        /// Whether to create a subattribute with the best match score as a value
        /// </summary>
        public bool CreateBestMatchScoreSubAttribute
        {
            get
            {
                return _createBestMatchScoreSubattribute;
            }

            set
            {
                try
                {
                    if (_createBestMatchScoreSubattribute != value)
                    {
                        _createBestMatchScoreSubattribute = value;

                        _dirty = true;
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI45483");
                }
            }
        }

        #endregion Properties

        #region IOutputHandler Members

        /// <summary>
        /// Processes the output (<see paramref="pAttributes"/>) by translating the selected attributes.
        /// </summary>
        /// <param name="pAttributes">The output to process.</param>
        /// <param name="pDoc">The <see cref="AFDocument"/> the output is from.</param>
        /// <param name="pProgressStatus">A <see cref="ProgressStatus"/> that can be used to update
        /// processing status.</param>
        public void ProcessOutput(IUnknownVector pAttributes, AFDocument pDoc, ProgressStatus pProgressStatus)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(_LICENSE_ID, "ELI45457", _COMPONENT_DESCRIPTION);

                ExtractException.Assert("ELI45458", "Rule is not properly configured.",
                    IsConfigured());

                // So that the garbage collector knows of and properly manages the associated
                // memory.
                pAttributes.ReportMemoryUsage();

                // Initialize for use in any embedded path tags/functions.
                _pathTags.Document = pDoc;

                var sourceListPath = _pathTags.Expand(SourceListPath);
                var synonymMapPath = _pathTags.Expand(SynonymMapPath);

                // Obtain all attributes specified to be translated
                IEnumerable<ComAttribute> selectedAttributes;
                using (RuleObjectProfiler profiler =
                    new RuleObjectProfiler("", "", AttributeSelector, 0))
                {
                    selectedAttributes = AttributeSelector.SelectAttributes(pAttributes, pDoc, pAttributes)
                        .ToIEnumerable<ComAttribute>();
                }

                // Process each of the selected attributes.
                foreach (ComAttribute attribute in selectedAttributes)
                {
                    // So that the garbage collector knows of and properly manages the associated
                    // memory.
                    attribute.ReportMemoryUsage();
                    TranslateAttribute(attribute, sourceListPath, synonymMapPath, pAttributes);
                }
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI45459", "Failed to translate attributes.");
            }
        }

        #endregion IOutputHandler Members

        #region IConfigurableObject Members

        /// <summary>
        /// Displays a form to allow configuration of this <see cref="TranslateValueToBestMatch"/>
        /// instance.
        /// </summary>
        /// <returns><see langword="true"/> if the configuration was successfully updated or
        /// <see langword="false"/> if configuration was unsuccessful.</returns>
        public bool RunConfiguration()
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(_LICENSE_ID, "ELI45460", _COMPONENT_DESCRIPTION);

                // Make a clone to update settings and only copy if ok
                TranslateValueToBestMatch cloneOfThis = (TranslateValueToBestMatch)Clone();

                using (TranslateValueToBestMatchSettingsDialog dlg
                    = new TranslateValueToBestMatchSettingsDialog(cloneOfThis))
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
                throw ex.CreateComVisible("ELI45461", "Error running configuration.");
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
                if (AttributeSelector == null)
                {
                    return false;
                }

                if (AttributeSelector is IMustBeConfiguredObject mustBeConfigured
                    && !mustBeConfigured.IsConfigured())
                {
                    return false;
                }

                return !string.IsNullOrWhiteSpace(SourceListPath);
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI45462",
                    "Error checking configuration of Rule object.");
            }
        }

        #endregion IMustBeConfiguredObject Members

        #region ICopyableObject Members

        /// <summary>
        /// Creates a copy of the <see cref="TranslateValueToBestMatch"/> instance.
        /// </summary>
        /// <returns>A copy of the <see cref="TranslateValueToBestMatch"/> instance.
        /// </returns>
        public object Clone()
        {
            try
            {
                return new TranslateValueToBestMatch(this);
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI45463",
                    "Failed to clone '" + _COMPONENT_DESCRIPTION + "' object.");
            }
        }

        /// <summary>
        /// Copies the specified <see cref="TranslateValueToBestMatch"/> instance into this one.
        /// </summary>
        /// <param name="pObject">The object from which to copy.</param>
        public void CopyFrom(object pObject)
        {
            try
            {
                var source = pObject as TranslateValueToBestMatch;
                if (source == null)
                {
                    throw new InvalidCastException("Invalid cast to TranslateValueToBestMatch");
                }
                CopyFrom(source);
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI45464",
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
                    AttributeSelector = reader.ReadIPersistStream() as IAttributeSelector;
                    SourceListPath = reader.ReadString();
                    SynonymMapPath = reader.ReadString();
                    MinimumMatchScore = reader.ReadDouble();
                    UnableToTranslateAction = (NoGoodMatchAction)reader.ReadInt32();
                    CreateBestMatchScoreSubAttribute = reader.ReadBoolean();

                    // Load the GUID for the IIdentifiableObject interface.
                    LoadGuid(stream);
                }

                // Freshly loaded object is no longer dirty
                _dirty = false;
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI45465",
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
                    writer.Write((IPersistStream)AttributeSelector, clearDirty);
                    writer.Write(SourceListPath);
                    writer.Write(SynonymMapPath);
                    writer.Write(MinimumMatchScore);
                    writer.Write((int)UnableToTranslateAction);
                    writer.Write(CreateBestMatchScoreSubAttribute);

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
                throw ex.CreateComVisible("ELI45466",
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
        /// Code to be executed upon registration in order to add this class to the
        /// "UCLID AF-API Output Handlers" COM category.
        /// </summary>
        /// <param name="type">The <paramref name="type"/> being registered.</param>
        [ComRegisterFunction]
        [ComVisible(false)]
        static void RegisterFunction(Type type)
        {
            ComMethods.RegisterTypeInCategory(type, ExtractCategories.OutputHandlersGuid);
        }

        /// <summary>
        /// Code to be executed upon unregistration in order to remove this class from the
        /// "UCLID AF-API Output Handlers" COM category.
        /// </summary>
        /// <param name="type">The <paramref name="type"/> being unregistered.</param>
        [ComUnregisterFunction]
        [ComVisible(false)]
        static void UnregisterFunction(Type type)
        {
            ComMethods.UnregisterTypeInCategory(type, ExtractCategories.OutputHandlersGuid);
        }

        /// <summary>
        /// Copies the specified <see cref="TranslateValueToBestMatch"/> instance into this one.
        /// </summary><param name="source">The <see cref="TranslateValueToBestMatch"/> from which to copy.
        /// </param>
        void CopyFrom(TranslateValueToBestMatch source)
        {
            if (source.AttributeSelector == null)
            {
                AttributeSelector = null;
            }
            else
            {
                ICopyableObject copyThis = (ICopyableObject)source.AttributeSelector;
                AttributeSelector = (IAttributeSelector)copyThis.Clone();
            }

            SourceListPath = source.SourceListPath;
            SynonymMapPath = source.SynonymMapPath;
            MinimumMatchScore = source.MinimumMatchScore;
            UnableToTranslateAction = source.UnableToTranslateAction;
            CreateBestMatchScoreSubAttribute = source.CreateBestMatchScoreSubAttribute;

            _dirty = true;
        }

        /// <summary>
        /// Translate an <see paramref="attribute"/>
        /// </summary>
        /// <param name="attribute">The <see cref="ComAttribute"/> to translate.</param>
        /// <param name="sourceListPath">The expanded path to the list of translation targets</param>
        /// <param name="synonymMapPath">The expanded path to the synonym map CSV (could be null)</param>
        /// <param name="rootAttributes">The VOA passed to <see cref="ProcessOutput(IUnknownVector, AFDocument, ProgressStatus)"/>.
        /// Used to delete untranslated attributes, if necessary</param>
        void TranslateAttribute(ComAttribute attribute, string sourceListPath, string synonymMapPath, IUnknownVector rootAttributes)
        {
            var provider = GetLuceneSuggestionProvider(sourceListPath, synonymMapPath);
            var suggestion = provider.GetBestMatch(attribute.Value.String);
            if (suggestion != null && suggestion.Score >= MinimumMatchScore)
            {
                attribute.Value.Replace(attribute.Value.String, provider.GetValue(suggestion.Doc), true, 0, null);
                if (CreateBestMatchScoreSubAttribute)
                {
                    var ac = new AttributeCreator(attribute.Value.SourceDocName);
                    attribute.SubAttributes.PushBack(ac.Create("BestMatchScore", suggestion.Score));
                }
            }
            else
            {
                if (UnableToTranslateAction == NoGoodMatchAction.RemoveAttribute)
                {
                    AFUtility.RemoveAttribute(rootAttributes, attribute);
                }
                else
                {
                    if (CreateBestMatchScoreSubAttribute)
                    {
                        var ac = new AttributeCreator(attribute.Value.SourceDocName);
                        attribute.SubAttributes.PushBack(ac.Create("BestMatchScore", suggestion?.Score ?? 0));
                    }

                    switch (UnableToTranslateAction)
                    {
                        case NoGoodMatchAction.DoNothing:
                            break;
                        case NoGoodMatchAction.ClearValue:
                            attribute.Value.Clear();
                            break;
                        case NoGoodMatchAction.SetTypeToUntranslated:
                            attribute.Type = "Untranslated";
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Get/update cached provider object
        /// </summary>
        /// <param name="sourceListPath">The expanded path to the source list, either a text file or an encrypted text file</param>
        /// <param name="synonymMapPath">Path to CSV defining synonym groups. Can be a text file or an encrypted text file</param>
        static LuceneBestMatchProvider GetLuceneSuggestionProvider(string sourceListPath, string synonymMapPath)
        {
            var paths = new List<string> { sourceListPath };
            if (!string.IsNullOrWhiteSpace(synonymMapPath))
            {
                paths.Add(synonymMapPath);
            }

            var provider = FileDerivedResourceCache.GetCachedObject(
                paths: paths,
                creator: () => BuildProvider(sourceListPath, synonymMapPath));

            return provider;
        }

        static LuceneBestMatchProvider BuildProvider(string sourceListPath, string synonymMapPath)
        {
            var values = FileDerivedResourceCache.ThreadLocalMiscUtils.GetStringOptionallyFromFile(sourceListPath)
                .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            SynonymMap synonymMap = null;
            if (!string.IsNullOrWhiteSpace(synonymMapPath))
            {
                synonymMap = BuildSynonymMap(synonymMapPath);
            }

            var provider = new LuceneBestMatchProvider(values, synonymMap);

            return provider;
        }

        static SynonymMap BuildSynonymMap(string synonymMapPath)
        {
            var map = new SynonymMap.Builder(true);
            var mapSource = new StringReader(FileDerivedResourceCache.ThreadLocalMiscUtils.GetStringOptionallyFromFile(synonymMapPath));
            var lineSets = new List<HashSet<string>>();
            using (var csvReader = new Microsoft.VisualBasic.FileIO.TextFieldParser(mapSource))
            {
                csvReader.Delimiters = new[] { "," };
                int rowsRead = 0;
                while (!csvReader.EndOfData)
                {
                    string[] fields;
                    try
                    {
                        fields = csvReader.ReadFields();
                        rowsRead++;
                    }
                    catch (Exception e)
                    {
                        var ue = new ExtractException("ELI45467", "Error parsing CSV synonym map file", e);
                        ue.AddDebugData("CSV path", synonymMapPath, false);
                        throw ue;
                    }

                    ExtractException.Assert("ELI45468", "CSV rows should contain at least two fields",
                        fields.Length > 1,
                        "Field count", fields.Length,
                        "Row number", rowsRead);

                    // Process the string with the same filters that will be used to analyze the input
                    // and queries so that, e.g., non-word chars are removed and words are stemmed
                    for (int i = 0; i < fields.Length; i++)
                    {
                        fields[i] = LuceneSuggestionAnalyzer.ProcessString(fields[i]);
                    }

                    // Merge this row with any others that have the same term
                    fields = fields.Where(f => !string.IsNullOrWhiteSpace(f)).ToArray();
                    if (fields.Length > 1)
                    {
                        var overlap = lineSets.FirstOrDefault(s => fields.Any(f => s.Contains(f)));
                        if (overlap == null)
                        {
                            lineSets.Add(new HashSet<string>(fields));
                        }
                        else
                        {
                            foreach (var f in fields)
                            {
                                overlap.Add(f);
                            }
                        }
                    }
                }
            }
            
            // Work-around Lucene multi-word synonym shortcomings by expanding everything to the largest value
            foreach(var fields in lineSets)
            {
                var sorted = fields
                    .OrderByDescending(f => f.Split(' ').Length)
                    .ThenByDescending(f => f.Length)
                    .ThenBy(f => f)
                    .ToList();
                foreach (var f in sorted.Skip(1))
                {
                    map.Add(new Lucene.Net.Util.CharsRef(f), new Lucene.Net.Util.CharsRef(sorted[0]), includeOrig: false);
                }
            }
            return map.Build();
        }

        #endregion Private Members
    }
}