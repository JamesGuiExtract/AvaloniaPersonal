using Extract.Interop;
using Extract.Licensing;
using Extract.Utilities;
using Extract.Utilities.Parsers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using UCLID_AFCORELib;
using UCLID_COMLMLib;
using UCLID_COMUTILSLib;
using UCLID_RASTERANDOCRMGMTLib;
using UCLID_SSOCRLib;
using ComAttribute = UCLID_AFCORELib.Attribute;
using OCRParam = Extract.Utilities.Union<(int key, int value), (int key, double value), (string key, int value), (string key, double value), (string key, string value)>;

namespace Extract.AttributeFinder.Rules
{
    /// <summary>
    /// An interface for the <see cref="MicrFinder"/> class.
    /// </summary>
    [ComVisible(true)]
    [Guid("732FE480-2DB6-4840-93C5-82E555FFC47E")]
    [CLSCompliant(false)]
    public interface IMicrFinder : IAttributeFindingRule, ICategorizedComponent, IConfigurableObject,
        ICopyableObject, ILicensedComponent, IPersistStream, IIdentifiableObject, Dto.IUseDto<Dto.MicrFinderV2>
    {
        /// <summary>
        /// MICR lines having an average confidence of at least this value will be returned.
        /// </summary>
        int HighConfidenceThreshold { get; set; }

        /// <summary>
        /// If a MICR line's averate confidence does not meet <see cref="HighConfidenceThreshold"/>,
        /// it can optionally be compared to the confidence of standard OCR of the same text for inclusion.
        /// </summary>
        bool UseLowConfidenceThreshold { get; set; }

        /// <summary>
        /// In the case of <see cref="UseLowConfidenceThreshold"/>, MICR confidence must have at least
        /// this confidence to quality to compare to standard OCR text.
        /// </summary>
        int LowConfidenceThreshold { get; set; }

        /// <summary>
        /// Any MICR line, regardless of confidence, must contain a match for this regular expression
        /// to be returned. (if specified)
        /// </summary>
        string FilterRegex { get; set; }

        /// <summary>
        /// Whether to create a sub-attribute for a routing number that can be successfully parsed
        /// from the MICR line.
        /// </summary>
        bool SplitRoutingNumber { get; set; }

        /// <summary>
        /// Whether to create a sub-attribute for an account number that can be successfully parsed
        /// from the MICR line.
        /// </summary>
        bool SplitAccountNumber { get; set; }

        /// <summary>
        /// Whether to create a sub-attribute for a check number that can be successfully parsed
        /// from the MICR line.
        /// </summary>
        bool SplitCheckNumber { get; set; }

        /// <summary>
        /// Whether to create a sub-attribute for an amount that can be successfully parsed from
        /// the MICR line.
        /// </summary>
        bool SplitAmount { get; set; }

        /// <summary>
        /// The regular expression used to parse the component elements of a MICR line.
        /// </summary>
        string MicrSplitterRegex { get; set; }

        /// <summary>
        /// Indicates whether to remove special MICR chars and spaces when splitting components
        /// into sub-attributes.
        /// </summary>
        bool FilterCharsWhenSplitting { get; set; }

        /// <summary>
        /// Whether to use OCRParameters specified in the containing ruleset or the input spatial string
        /// (if <c>true</c>) or to use only built-in parameters (if <c>false</c>)
        /// </summary>
        bool InheritOCRParameters { get; set; }

        /// <summary>
        /// Whether to return unrecognized characters (as ^) or not
        /// </summary>
        bool ReturnUnrecognizedCharacters { get; set; }

        /// <summary>
        /// The type of MICR engine to use
        /// </summary>
        Dto.MicrEngineType EngineType { get; set; }

        /// <summary>
        /// Whether to search all pages or only pages with OCR results
        /// </summary>
        bool SearchAllPages { get; set; }
    }

    /// <summary>
    /// An <see cref="IAttributeFindingRule"/> finds MICR lines
    /// </summary>
    [ComVisible(true)]
    [Guid("299DED4B-73FC-4747-9965-A4454A8B562A")]
    [ClassInterface(ClassInterfaceType.None)]
    [ComDefaultInterface(typeof(IMicrFinder))]
    [CLSCompliant(false)]
    public class MicrFinder : IdentifiableObject, IMicrFinder
    {
        #region Constants

        /// <summary>
        /// The description of the rule
        /// </summary>
        const string _COMPONENT_DESCRIPTION = "MICR finder (v2)";

        /// <summary>
        /// Current version.
        ///   Version 4: Change to save settings as JSON string
        /// </summary>
        const int _CURRENT_VERSION = 4;

        /// <summary>
        /// The license id to validate in licensing calls
        /// </summary>
        const LicenseIdName _LICENSE_ID = LicenseIdName.MicrFindingEngineFeature;

        const string _AUTO_ENCRYPT_KEY = @"Software\Extract Systems\AttributeFinder\Settings\AutoEncrypt";

        #endregion Constants

        #region Fields

        Dto.MicrFinderV2 _unmodified;

        AFDocument _currentDocument;

        /// <summary>
        /// A cached set of <see cref="SpatialStringSearcher"/>s for each page of the current
        /// document so as not to have to re-initialize a searcher multiple times for the same page.
        /// </summary>
        [ThreadStatic]
        static Dictionary<int, SpatialStringSearcher> _searchers;

        /// <summary>
        /// Cached original OCR data that has been read from a uss file.
        /// </summary>
        [ThreadStatic]
        static SpatialString _cachedOcrData;

        /// <summary>
        /// When original OCR data is read from disk, keep track of the file information from which
        /// it was read so that cached OCR data need not be re-read unless the uss file changes.
        /// </summary>
        [ThreadStatic]
        static FileInfo _cachedUSSFileInfo;

        /// <summary>
        /// Parser to use to filter non-qualified MICR lines from being returned from this rule.
        /// </summary>
        [ThreadStatic]
        static DotNetRegexParser _filterParser;

        /// <summary>
        /// Parser to use to split MICR components into sub-attributes
        /// </summary>
        [ThreadStatic]
        static DotNetRegexParser _splitterParser;

        /// <summary>
        /// Parser to use to filter non-numeric characters for MICR components split from the full MICR line.
        /// </summary>
        [ThreadStatic]
        static DotNetRegexParser _charRemovalParser;

        [ThreadStatic]
        static MiscUtils _miscUtils;

        static ThreadLocal<ScansoftOCRClass> _ocrEngine = new ThreadLocal<ScansoftOCRClass>(() =>
        {
            var engine = new ScansoftOCRClass();
            engine.InitPrivateLicense(LicenseUtilities.GetMapLabelValue(new MapLabel()));
            return engine;
        });

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MicrFinder"/> class.
        /// </summary>
        public MicrFinder()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MicrFinder"/> class as a copy of
        /// the specified <see paramref="micrFinder"/>.
        /// </summary>
        /// <param name="micrFinder">The <see cref="MicrFinder"/> from which settings should be copied.</param>
        public MicrFinder(MicrFinder micrFinder)
        {
            try
            {
                CopyFrom(micrFinder);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI46894");
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MicrFinder"/> class from a <see cref="Dto.MicrFinderV2"/>
        /// </summary>
        /// <param name="micrFinderSettings">The <see cref="Dto.MicrFinderV2"/> from which settings should be copied.</param>
        public MicrFinder(Dto.MicrFinderV2 micrFinderSettings)
        {
            try
            {
                DataTransferObject = micrFinderSettings;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI51546");
            }
        }

        #endregion Constructors

        #region IMicrFinder

        /// <summary>
        /// MICR lines having an average confidence of at least this value will be returned.
        /// </summary>
        public int HighConfidenceThreshold { get; set; } = 80;

        /// <summary>
        /// If a MICR line's average confidence does not meet <see cref="HighConfidenceThreshold"/>,
        /// it can optionally be compared to the confidence of standard OCR of the same text for inclusion.
        /// </summary>
        public bool UseLowConfidenceThreshold { get; set; } = true;

        /// <summary>
        /// In the case of <see cref="UseLowConfidenceThreshold"/>, MICR confidence must have at least
        /// this confidence to quality to compare to standard OCR text.
        /// </summary>
        public int LowConfidenceThreshold { get; set; } = 50;

        /// <summary>
        /// Any MICR line, regardless of confidence, must contain a match for this regular expression
        /// to be returned. (if specified)
        /// </summary>
        public string FilterRegex { get; set; } = @"file://<ComponentDataDir>\Redaction\Common\Checks\MICRFinderFilter.dat.etf";

        /// <summary>
        /// Whether to create a sub-attribute for a routing number that can be successfully parsed
        /// from the MICR line.
        /// </summary>
        public bool SplitRoutingNumber { get; set; }

        /// <summary>
        /// Whether to create a sub-attribute for an account number that can be successfully parsed
        /// from the MICR line.
        /// </summary>
        public bool SplitAccountNumber { get; set; }

        /// <summary>
        /// Whether to create a sub-attribute for a check number that can be successfully parsed
        /// from the MICR line.
        /// </summary>
        public bool SplitCheckNumber { get; set; }

        /// <summary>
        /// Whether to create a sub-attribute for an amount that can be successfully parsed from
        /// the MICR line.
        /// </summary>
        public bool SplitAmount { get; set; }

        /// <summary>
        /// The regular expression used to parse the component elements of a MICR line.
        /// </summary>
        public string MicrSplitterRegex { get; set; } = @"file://<ComponentDataDir>\Redaction\Common\Checks\MICRFinderSplitter.dat.etf";

        /// <summary>
        /// Indicates whether to remove special MICR chars and spaces when splitting components
        /// into sub-attributes.
        /// </summary>
        public bool FilterCharsWhenSplitting { get; set; } = true;

        /// <summary>
        /// Whether to use OCRParameters specified in the containing ruleset or the input spatial string
        /// (if <c>true</c>) or to use only built-in parameters (if <c>false</c>)
        /// </summary>
        public bool InheritOCRParameters { get; set; }

        /// <summary>
        /// Whether to return unrecognized characters (as ^) or not
        /// </summary>
        public bool ReturnUnrecognizedCharacters { get; set; }

        /// <summary>
        /// The type of OCR engine to use
        /// </summary>
        public Dto.MicrEngineType EngineType { get; set; } = Dto.MicrEngineType.Kofax;

        /// <summary>
        /// Whether to search all pages or only pages with OCR results
        /// </summary>
        public bool SearchAllPages { get; set; } = true;

        #endregion IMicrFinder

        #region IAttributeFindingRule

        /// <summary>
        /// Parses the <see paramref="pDocument"/> and returns a vector of found
        /// <see cref="ComAttribute"/> objects.
        /// </summary>
        /// <param name="pDocument">The <see cref="AFDocument"/> to parse.</param>
        /// <param name="pProgressStatus">The <see cref="ProgressStatus"/> to indicate processing
        /// progress.</param>
        /// <returns>An <see cref="IUnknownVector"/> of found <see cref="ComAttribute"/>s.</returns>
        public IUnknownVector ParseText(AFDocument pDocument, ProgressStatus pProgressStatus)
        {
            try
            {
                LicenseUtilities.ValidateLicense(_LICENSE_ID, "ELI49618", _COMPONENT_DESCRIPTION);

                _currentDocument = pDocument;

                var results = FindMicrs(pDocument);

                return results;
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI46895", "Failed to search for MICR.");
            }
        }

        #endregion IAttributeFindingRule

        #region IConfigurableObject Members

        /// <summary>
        /// Performs configuration needed to create a valid <see cref="MicrFinder"/>.
        /// </summary>
        /// <returns><see langword="true"/> if the configuration was successfully updated or
        /// <see langword="false"/> if configuration was unsuccessful.</returns>
        public bool RunConfiguration()
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(_LICENSE_ID, "ELI46911", _COMPONENT_DESCRIPTION);

                using (MicrFinderSettingsDialog dlg = new MicrFinderSettingsDialog(DataTransferObject))
                {
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        DataTransferObject = dlg.Settings;
                        return true;
                    }
                }

                return false;
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI46912", "Error running configuration.");
            }
        }

        #endregion IConfigurableObject Members

        #region ICopyableObject Members

        /// <summary>
        /// Creates a copy of the <see cref="MicrFinder"/> instance.
        /// </summary>
        /// <returns>A copy of the <see cref="MicrFinder"/> instance.
        /// </returns>
        public object Clone()
        {
            try
            {
                return new MicrFinder(this);
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI46896",
                    "Failed to clone '" + _COMPONENT_DESCRIPTION + "' object.");
            }
        }

        /// <summary>
        /// Copies the specified <see cref="MicrFinder"/> instance into this one.
        /// </summary>
        /// <param name="pObject">The object from which to copy.</param>
        public void CopyFrom(object pObject)
        {
            try
            {
                var source = pObject as MicrFinder;
                if (source == null)
                {
                    throw new InvalidCastException("Invalid cast to MicrFinder");
                }
                CopyFrom(source);
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI46897",
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
            var isDirty = DataTransferObject.Equals(_unmodified);
            return HResult.FromBoolean(isDirty);
        }

        /// <summary>
        /// Initializes an object from the IStream where it was previously saved.
        /// </summary>
        /// <param name="stream">IStream from which the object should be loaded.</param>
        public void Load(System.Runtime.InteropServices.ComTypes.IStream stream)
        {
            try
            {
                using IStreamReader reader = new IStreamReader(stream, _CURRENT_VERSION);
                if (reader.Version >= 4)
                {
                    var json = reader.ReadString();
                    var dto = JsonConvert.DeserializeObject<Dto.MicrFinderV2>(json);
                    _unmodified = DataTransferObject = dto;
                }
                else
                {
                    HighConfidenceThreshold = reader.ReadInt32();
                    UseLowConfidenceThreshold = reader.ReadBoolean();
                    LowConfidenceThreshold = reader.ReadInt32();
                    FilterRegex = reader.ReadString();
                    SplitRoutingNumber = reader.ReadBoolean();
                    SplitAccountNumber = reader.ReadBoolean();
                    SplitCheckNumber = reader.ReadBoolean();
                    SplitAmount = reader.ReadBoolean();
                    MicrSplitterRegex = reader.ReadString();
                    FilterCharsWhenSplitting = reader.ReadBoolean();

                    if (reader.Version >= 2)
                    {
                        InheritOCRParameters = reader.ReadBoolean();
                    }
                    if (reader.Version >= 3)
                    {
                        ReturnUnrecognizedCharacters = reader.ReadBoolean();
                    }
                    else
                    {
                        ReturnUnrecognizedCharacters = true;
                    }

                    EngineType = Dto.MicrEngineType.Kofax;
                    SearchAllPages = false;

                    // Load the GUID for the IIdentifiableObject interface.
                    LoadGuid(stream);

                    // Freshly loaded object is no longer dirty
                    _unmodified = DataTransferObject;
                }
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI46898",
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
                var dto = DataTransferObject;
                using (IStreamWriter writer = new IStreamWriter(_CURRENT_VERSION))
                {
                    var json = JsonConvert.SerializeObject(dto);
                    writer.Write(json);

                    // Write to the provided IStream.
                    writer.WriteTo(stream);
                }
                if (clearDirty)
                {
                    _unmodified = dto;
                }
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI46899",
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
        /// Copies the specified <see cref="MicrFinder"/> instance into this one.
        /// </summary><param name="source">The <see cref="MicrFinder"/> from which to copy.
        /// </param>
        void CopyFrom(MicrFinder source)
        {
            DataTransferObject = source.DataTransferObject;
        }

        /// <summary>
        /// Finds MICR lines in <see cref="pDocument"/>.
        /// </summary>
        /// <param name="doc">The document to search.</param>
        IUnknownVector FindMicrs(AFDocument doc)
        {
            var recognized = Recognize(doc);

            var resultVector = recognized.GetLines()
                .ToIEnumerable<SpatialString>()
                .Where(line => QualifyLine(line, doc))
                .Select(line =>
                {
                    var attribute = new AttributeClass();
                    attribute.Value = line;
                    SplitMicrComponents(attribute);
                    return attribute;
                })
                .ToIUnknownVector();

            resultVector.ReportMemoryUsage();

            return resultVector;
        }

        SpatialString Recognize(AFDocument doc)
        {
            var pages = Enumerable.Empty<int>();
            // If no original recognized text then it is still possible to search MICR on all pages if we have an image file
            if (SearchAllPages && !String.IsNullOrEmpty(doc.Text.SourceDocName))
            {
                pages = null;
            }
            else if (doc.Text.HasSpatialInfo())
            {
                pages = doc.Text
                    .GetPages(false, "")
                    .ToIEnumerable<SpatialString>()
                    .Select(p => p.GetFirstPageNumber());
            }
            else
            {
                return new SpatialStringClass();
            }

            return EngineType switch
            {
                Dto.MicrEngineType.Kofax => RecognizeKofax(doc, pages),
                Dto.MicrEngineType.GdPicture => throw new ExtractException("ELI51589", "Unsupported engine type!"),
                _ => throw new ExtractException("ELI51470", "Unknown engine type!")
            };
        }

        // Search on supplied pages or on all pages if pages is null
        SpatialString RecognizeKofax(AFDocument doc, IEnumerable<int> pages)
        {
            IOCRParameters ocrParams = GetOCRParams(doc);

            if (pages == null)
            {
                return _ocrEngine.Value.RecognizeTextInImage(doc.Text.SourceDocName, 1, -1, EFilterCharacters.kNoFilter, null, EOcrTradeOff.kRegistry, true, null, ocrParams);
            }
            else
            {
                var pagesToSearch = string.Join(",", pages);
                return _ocrEngine.Value.RecognizeTextInImage2(doc.Text.SourceDocName, pagesToSearch, true, null, ocrParams);
            }
        }

        IOCRParameters GetOCRParams(AFDocument pDocument)
        {
            static bool tru<T>(T _) { return true; }

            var ocrParams = InheritOCRParameters
                ? ((IHasOCRParameters)pDocument).OCRParameters
                    .ToIEnumerable()
                    .Where(p => p.Match(
                        kv => (EOCRParameter)kv.key != EOCRParameter.kOCRType,
                        tru, tru, tru, tru))
                    .ToList()
                : new List<OCRParam>();

            ocrParams.Add(new OCRParam(((int)EOCRParameter.kOCRType, (int)EOCRFindType.kFindMICROnly)));
            ocrParams.Add(new OCRParam(((int)EOCRParameter.kReturnUnrecognizedCharacters, ReturnUnrecognizedCharacters ? 1 : 0)));

            return ocrParams.ToOCRParameters();
        }

        bool QualifyLine(SpatialString line, AFDocument document)
        {
            if (!line.HasSpatialInfo())
            {
                return false;
            }

            if (FilterRegexInstance != null)
            {
                if (!FilterRegexInstance.IsMatch(line.String))
                {
                    return false;
                }
            }

            int min = 0, max = 0, confidence = 0;
            line.GetCharConfidence(ref min, ref max, ref confidence);
            if (confidence >= HighConfidenceThreshold)
            {
                return true;
            }

            if (UseLowConfidenceThreshold && confidence >= LowConfidenceThreshold)
            {
                var ocrData = GetOriginalOcrData(document);

                var originalOcr = GetOCRText(line, ocrData);
                originalOcr.Trim(" \t\r\n", " \t\r\n");
                int origOcrConf = 0;
                originalOcr.GetCharConfidence(ref min, ref max, ref origOcrConf);

                if (confidence <= origOcrConf)
                {
                    return false;
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets the original OCR data for <see paramref="pOriginInput"/> from the uss file if
        /// necessary or from a cache, if possible.
        /// </summary>
        /// <param name="pOriginInput">The <see cref="AFDocument"/> for which the OCR data is
        /// needed.</param>
        /// <returns>A <see cref="SpatialString"/> representing the OCR data for
        /// <see paramref="pOriginInput"/>.</returns>
        static SpatialString GetOriginalOcrData(AFDocument pOriginInput)
        {
            string ussFileName = pOriginInput.Text.SourceDocName + ".uss";

            ExtractException.Assert("ELI46920",
                "Cannot find original OCR for document \"" +
                pOriginInput.Text.SourceDocName + "\"", File.Exists(ussFileName));

            SpatialString sourceString = null;

            // If we already have cached OCR data for this file on this thread, use it.
            FileInfo fileInfo = new FileInfo(ussFileName);
            if (_cachedUSSFileInfo != null &&
                _cachedUSSFileInfo.FullName == ussFileName &&
                fileInfo.LastWriteTime == _cachedUSSFileInfo.LastWriteTime)
            {
                sourceString = _cachedOcrData;
            }
            // Need to read the OCR data from the uss file.
            else
            {
                // [FlexIDSCore:5143]
                // Though the interwebs has lots of advice stating not to used FinalReleaseComObject
                // (and instead leave it up to garbage collection), garbage collection seems not to
                // be able to keep up in some usages of this rule. Since there is no risk of
                // sourceString being used, use a heavy hand to release it here. 
                if (_cachedOcrData != null)
                {
                    Marshal.FinalReleaseComObject(_cachedOcrData);
                }

                sourceString = new SpatialString();
                sourceString.LoadFrom(ussFileName, false);

                _cachedUSSFileInfo = fileInfo;
                _cachedOcrData = sourceString;
                _cachedOcrData.ReportMemoryUsage();
            }

            return sourceString;
        }

        /// <summary>
        /// Gets a <see cref="SpatialString"/> via the uss file representing the original OCR from
        /// the zone's spatial area.
        /// </summary>
        static SpatialString GetOCRText(SpatialString line, SpatialString documentSource)
        {
            SpatialString ocrText;
            if (documentSource.HasSpatialInfo())
            {
                int page = line.GetFirstPageNumber();

                SpatialStringSearcher searcher = GetSearcherForPage(page, documentSource);

                // So that the garbage collector knows of and properly manages the associated
                // memory.
                searcher.ReportMemoryUsage();

                LongRectangle bounds = line.GetOCRImageBounds();
                ocrText = searcher.GetDataInRegion(bounds, false);
            }
            else
            {
                ocrText = new SpatialStringClass();
            }
            ocrText.ReportMemoryUsage();
            return ocrText;
        }

        /// <summary>
        /// Gets a <see cref="SpatialStringSearcher"/> for the <see paramref="page"/>. This method
        /// will a searcher for each page to avoid needing to needlessly re-initialize.
        /// </summary>
        /// <param name="page">The page for which the searcher is needed.</param>
        /// <param name="sourceString">The source string representing the OCR text.</param>
        /// <returns>A <see cref="SpatialStringSearcher"/> for the <see paramref="page"/>.</returns>
        static SpatialStringSearcher GetSearcherForPage(int page, SpatialString sourceString)
        {
            SpatialStringSearcher searcher;
            if (_searchers == null)
            {
                _searchers = new Dictionary<int, SpatialStringSearcher>();
            }
            if (!_searchers.TryGetValue(page, out searcher))
            {
                searcher = new SpatialStringSearcher();
                searcher.InitSpatialStringSearcher(sourceString.GetSpecifiedPages(page, page), false);
                searcher.SetIncludeDataOnBoundary(true);
                searcher.SetBoundaryResolution(ESpatialEntity.kWord);
            }

            return searcher;
        }

        /// <summary>
        /// Creates sub-attributes to the specified attribute to represent recognized MICR components
        /// per configuration.
        /// </summary>
        void SplitMicrComponents(IAttribute micrAttribute)
        {
            if (SplitterRegexInstance == null)
            {
                return;
            }

            var componentNames = new List<string>();

            var groupNames = SplitterRegexInstance.GetGroupNames();

            if (SplitRoutingNumber)
            {
                ExtractException.Assert("ELI46957",
                    "Failed to get routing number because regex group is missing",
                    groupNames.Any(name => name == "Routing"));

                componentNames.Add("Routing");
            }
            if (SplitAccountNumber)
            {
                ExtractException.Assert("ELI46958",
                    "Failed to get account number because regex group is missing",
                    groupNames.Any(name => name == "Account"));

                componentNames.Add("Account");
            }
            if (SplitCheckNumber)
            {
                ExtractException.Assert("ELI46959",
                    "Failed to get check number because regex group is missing",
                    groupNames.Any(name => name == "CheckNumber"));

                componentNames.Add("CheckNumber");
            }
            if (SplitAmount)
            {
                ExtractException.Assert("ELI46960",
                    "Failed to get amount because regex group is missing",
                    groupNames.Any(name => name == "Amount"));

                componentNames.Add("Amount");
            }

            if (!componentNames.Any())
            {
                return;
            }

            var match = SplitterRegexInstance.Match(micrAttribute.Value.String);
            if (match.Success)
            {
                foreach (string componentName in componentNames)
                {
                    var matchGroup = match.Groups[componentName];

                    if (matchGroup.Success && matchGroup.Length > 0)
                    {
                        var componentAttribute = new AttributeClass();
                        componentAttribute.Type = componentName;
                        componentAttribute.Value = micrAttribute.Value.GetSubString(
                            matchGroup.Index, matchGroup.Index + matchGroup.Length - 1);

                        if (FilterCharsWhenSplitting)
                        {
                            // Replace spaces and special MICR chars, keep digits and "unsure" chars.
                            componentAttribute.Value.Replace(@"[^\d\?]", "", false, 0, CharRemovalParser);
                        }

                        micrAttribute.SubAttributes.PushBack(componentAttribute);
                    }
                }
            }
        }

        /// <summary>
        /// Returns a regular expression either as the literal text of <see paramref="regexSpecification"/>, or
        /// if a file is specified, the contents of the file.
        /// </summary>
        /// <param name="regexSpecification"></param>
        /// <param name="document"></param>
        /// <returns></returns>
        static string GetRegex(string regexSpecification, AFDocument document)
        {
            if (regexSpecification.StartsWith("file://", StringComparison.OrdinalIgnoreCase))
            {
                var pathTags = new AttributeFinderPathTags(document);
                var expandedSpec = pathTags.Expand(regexSpecification.Substring(7));
                MiscUtils.AutoEncryptFile(expandedSpec, _AUTO_ENCRYPT_KEY);
                return MiscUtils.GetStringOptionallyFromFile(expandedSpec);
            }
            else
            {
                return regexSpecification;
            }
        }

        /// <summary>
        /// <see cref="Regex"/> to use to filter non-qualified MICR lines from being returned from this rule.
        /// </summary>
        Regex FilterRegexInstance
        {
            get
            {
                if (string.IsNullOrWhiteSpace(FilterRegex))
                {
                    return null;
                }

                if (_filterParser == null)
                {
                    _filterParser = new DotNetRegexParser();
                    _filterParser.RegexOptions |= RegexOptions.IgnorePatternWhitespace;
                }

                _filterParser.Pattern = GetRegex(FilterRegex, _currentDocument);
                return _filterParser.Regex;
            }
        }

        /// <summary>
        /// <see cref="Regex"/> to use to split MICR components into sub-attributes
        /// </summary>
        Regex SplitterRegexInstance
        {
            get
            {
                if (string.IsNullOrWhiteSpace(MicrSplitterRegex))
                {
                    return null;
                }

                if (_splitterParser == null)
                {
                    _splitterParser = new DotNetRegexParser();
                    _splitterParser.RegexOptions |= RegexOptions.IgnorePatternWhitespace;
                }

                _splitterParser.Pattern = GetRegex(MicrSplitterRegex, _currentDocument);
                return _splitterParser.Regex;
            }
        }

        /// <summary>
        /// <see cref="DotNetRegexParser"/> to use to filter non-numeric characters for MICR
        /// components split from the full MICR line.
        /// </summary>
        static DotNetRegexParser CharRemovalParser
        {
            get
            {
                if (_charRemovalParser == null)
                {
                    _charRemovalParser = new DotNetRegexParser();
                    _charRemovalParser.RegexOptions |= RegexOptions.IgnorePatternWhitespace;
                }

                return _charRemovalParser;
            }
        }

        static MiscUtils MiscUtils
        {
            get
            {
                if (_miscUtils == null)
                {
                    _miscUtils = new MiscUtils();
                }

                return _miscUtils;
            }
        }

        public Dto.MicrFinderV2 DataTransferObject
        {
            get
            {
                return new Dto.MicrFinderV2(
                    highConfidenceThreshold: HighConfidenceThreshold,
                    useLowConfidenceThreshold: UseLowConfidenceThreshold,
                    lowConfidenceThreshold: LowConfidenceThreshold,
                    filterRegex: FilterRegex,
                    splitRoutingNumber: SplitRoutingNumber,
                    splitAccountNumber: SplitAccountNumber,
                    splitCheckNumber: SplitCheckNumber,
                    splitAmount: SplitAmount,
                    micrSplitterRegex: MicrSplitterRegex,
                    filterCharsWhenSplitting: FilterCharsWhenSplitting,
                    inheritOCRParameters: InheritOCRParameters,
                    returnUnrecognizedCharacters: ReturnUnrecognizedCharacters,
                    engineType: EngineType,
                    searchAllPages: SearchAllPages
                );
            }
            set
            {
                try
                {
                    HighConfidenceThreshold = value.HighConfidenceThreshold;
                    UseLowConfidenceThreshold = value.UseLowConfidenceThreshold;
                    LowConfidenceThreshold = value.LowConfidenceThreshold;
                    FilterRegex = value.FilterRegex;
                    SplitRoutingNumber = value.SplitRoutingNumber;
                    SplitAccountNumber = value.SplitAccountNumber;
                    SplitCheckNumber = value.SplitCheckNumber;
                    SplitAmount = value.SplitAmount;
                    MicrSplitterRegex = value.MicrSplitterRegex;
                    FilterCharsWhenSplitting = value.FilterCharsWhenSplitting;
                    InheritOCRParameters = value.InheritOCRParameters;
                    ReturnUnrecognizedCharacters = value.ReturnUnrecognizedCharacters;
                    // Default to Kofax for old versions without this setting
                    EngineType = value.EngineType == Dto.MicrEngineType.None
                        ? Dto.MicrEngineType.Kofax
                        : value.EngineType;
                    SearchAllPages = value.SearchAllPages;
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI51547");
                }
            }
        }

        #endregion Private Members
    }
}
