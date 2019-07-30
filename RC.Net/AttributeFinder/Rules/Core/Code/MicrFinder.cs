using Extract.Interop;
using Extract.Licensing;
using Extract.Utilities;
using Extract.Utilities.Parsers;
using Nuance.OmniPage.CSDK.ArgTypes;
using Nuance.OmniPage.CSDK.Objects;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
using ComAttribute = UCLID_AFCORELib.Attribute;
using ComRasterZone = UCLID_RASTERANDOCRMGMTLib.RasterZone;

namespace Extract.AttributeFinder.Rules
{
    /// <summary>
    /// An interface for the <see cref="MicrFinder"/> class.
    /// </summary>
    [ComVisible(true)]
    [Guid("732FE480-2DB6-4840-93C5-82E555FFC47E")]
    [CLSCompliant(false)]
    public interface IMicrFinder : IAttributeFindingRule, ICategorizedComponent, IConfigurableObject,
        ICopyableObject, ILicensedComponent, IPersistStream, IIdentifiableObject
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
    }

    /// <summary>
    /// An <see cref="IAttributeFindingRule"/> that adds the <see cref="T:AFDocument.Attribute"/>
    /// (and children) as a literal output attribute.
    /// </summary>
    [ComVisible(true)]
    [Guid("299DED4B-73FC-4747-9965-A4454A8B562A")]
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
        /// </summary>
        const int _CURRENT_VERSION = 1;

        /// <summary>
        /// The license id to validate in licensing calls
        /// </summary>
        const LicenseIdName _LICENSE_ID = LicenseIdName.MicrFindingEngineFeature;

        /// <summary>
        /// Special MICR chars
        /// </summary>
        const char _TRANSIT_CHAR = (char)0x2446;
        const char _AMOUNT_CHAR = (char)0x2447;
        const char _ON_US_CHAR = (char)0x2448;
        const char _DASH_CHAR = (char)0x2449;

        /// <summary>
        /// Then character Nuance returns when it can not properly recognize a character.
        /// </summary>
        const char _UNRECOGNIZED_CHAR = (char)0xFFFD;

        /// <summary>
        /// Nuance mask to retrieve only confidence from character error level (and exclude the suspect word flag).
        /// </summary>
        const int RE_ERROR_LEVEL_MASK = ~0x80;

        const string _AUTO_ENCRYPT_KEY = @"Software\Extract Systems\AttributeFinder\Settings\AutoEncrypt";

        #endregion Constants

        /// <summary>
        /// Represents the spatial area and recognized letters of a candidate MICR zone.
        /// </summary>
        class ZoneData
        {
            public ComRasterZone RasterZone { get; set; }
            public IEnumerable<LETTER> Letters { get; set; }
        }

        #region Fields

        int _highConfidenceThreshold = 80;
        bool _useLowConfidenceThreshold = true;
        int _lowConfidenceThreshold = 50;
        string _filterRegexSpec = @"file://<ComponentDataDir>\Redaction\Common\Checks\MICRFinderFilter.dat.etf";
        bool _splitRoutingNumber = false;
        bool _splitAccountNumber = false;
        bool _splitCheckNumber = false;
        bool _splitAmount;
        string _micrSplitterRegexSpec = @"file://<ComponentDataDir>\Redaction\Common\Checks\MICRFinderSplitter.dat.etf";
        bool _filterCharsWhenSplitting = true;
        bool _dirty;

        AFDocument _currentDocument;
        static int _instanceCount;

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

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="MicrFinder"/> class.
        /// </summary>
        public MicrFinder()
        {
            try
            {
                if (Interlocked.Increment(ref _instanceCount) == 1)
                {
                    Engine.SetLicenseKey(null, "9d478fe171d5");
                    Engine.Init("Extract Systems", "Extract Systems");
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI46893");
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MicrFinder"/> class as a copy of
        /// the specified <see paramref="MicrFinder"/>.
        /// </summary>
        /// <param name="MicrFinder">The <see cref="MicrFinder"/> from which settings should be copied.</param>
        public MicrFinder(MicrFinder MicrFinder)
        {
            try
            {
                CopyFrom(MicrFinder);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI46894");
            }
        }

        #endregion Constructors

        #region IMicrFinder

        /// <summary>
        /// MICR lines having an average confidence of at least this value will be returned.
        /// </summary>
        public int HighConfidenceThreshold
        {
            get
            {
                return _highConfidenceThreshold;
            }

            set
            {
                if (value != _highConfidenceThreshold)
                {
                    _highConfidenceThreshold = value;
                    _dirty = true;
                }
            }
        }

        /// <summary>
        /// If a MICR line's average confidence does not meet <see cref="HighConfidenceThreshold"/>,
        /// it can optionally be compared to the confidence of standard OCR of the same text for inclusion.
        /// </summary>
        public bool UseLowConfidenceThreshold
        {
            get
            {
                return _useLowConfidenceThreshold;
            }

            set
            {
                if (value != _useLowConfidenceThreshold)
                {
                    _useLowConfidenceThreshold = value;
                    _dirty = true;
                }
            }
        }

        /// <summary>
        /// In the case of <see cref="UseLowConfidenceThreshold"/>, MICR confidence must have at least
        /// this confidence to quality to compare to standard OCR text.
        /// </summary>
        public int LowConfidenceThreshold
        {
            get
            {
                return _lowConfidenceThreshold;
            }

            set
            {
                if (value != _lowConfidenceThreshold)
                {
                    _lowConfidenceThreshold = value;
                    _dirty = true;
                }
            }
        }

        /// <summary>
        /// Any MICR line, regardless of confidence, must contain a match for this regular expression
        /// to be returned. (if specified)
        /// </summary>
        public string FilterRegex
        {
            get
            {
                return _filterRegexSpec;
            }

            set
            {
                if (value != _filterRegexSpec)
                {
                    _filterRegexSpec = value;
                    _dirty = true;
                }
            }
        }

        /// <summary>
        /// Whether to create a sub-attribute for a routing number that can be successfully parsed
        /// from the MICR line.
        /// </summary>
        public bool SplitRoutingNumber
        {
            get
            {
                return _splitRoutingNumber;
            }

            set
            {
                if (value != _splitRoutingNumber)
                {
                    _splitRoutingNumber = value;
                    _dirty = true;
                }
            }
        }

        /// <summary>
        /// Whether to create a sub-attribute for an account number that can be successfully parsed
        /// from the MICR line.
        /// </summary>
        public bool SplitAccountNumber
        {
            get
            {
                return _splitAccountNumber;
            }

            set
            {
                if (value != _splitAccountNumber)
                {
                    _splitAccountNumber = value;
                    _dirty = true;
                }
            }
        }

        /// <summary>
        /// Whether to create a sub-attribute for a check number that can be successfully parsed
        /// from the MICR line.
        /// </summary>
        public bool SplitCheckNumber
        {
            get
            {
                return _splitCheckNumber;
            }

            set
            {
                if (value != _splitCheckNumber)
                {
                    _splitCheckNumber = value;
                    _dirty = true;
                }
            }
        }

        /// <summary>
        /// Whether to create a sub-attribute for an amount that can be successfully parsed from
        /// the MICR line.
        /// </summary>
        [SuppressMessage("ExtractRules", "ES0001:PublicMethodsContainTryCatch")]
        public bool SplitAmount
        {
            get
            {
                return _splitAmount;
            }

            set
            {
                if (value != _splitAmount)
                {
                    _splitAmount = value;
                    _dirty = true;
                }
            }
        }

        /// <summary>
        /// The regular expression used to parse the component elements of a MICR line.
        /// </summary>
        public string MicrSplitterRegex
        {
            get
            {
                return _micrSplitterRegexSpec;
            }

            set
            {
                if (value != _micrSplitterRegexSpec)
                {
                    _micrSplitterRegexSpec = value;
                    _dirty = true;
                }
            }
        }

        /// <summary>
        /// Indicates whether to remove special MICR chars and spaces when splitting components
        /// into sub-attributes.
        /// </summary>
        public bool FilterCharsWhenSplitting
        {
            get
            {
                return _filterCharsWhenSplitting;
            }

            set
            {
                if (value != _filterCharsWhenSplitting)
                {
                    _filterCharsWhenSplitting = value;
                    _dirty = true;
                }
            }
        }

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
        /// Performs configuration needed to create a valid <see cref="CreateAttribute"/>.
        /// </summary>
        /// <returns><see langword="true"/> if the configuration was successfully updated or
        /// <see langword="false"/> if configuration was unsuccessful.</returns>
        public bool RunConfiguration()
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(_LICENSE_ID, "ELI46911", _COMPONENT_DESCRIPTION);

                // Make a clone to update settings and only copy if ok
                var cloneOfThis = (MicrFinder)Clone();

                using (MicrFinderSettingsDialog dlg = new MicrFinderSettingsDialog(cloneOfThis))
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

                    // Load the GUID for the IIdentifiableObject interface.
                    LoadGuid(stream);
                }

                // Freshly loaded object is no longer dirty
                _dirty = false;
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
                using (IStreamWriter writer = new IStreamWriter(_CURRENT_VERSION))
                {
                    writer.Write(HighConfidenceThreshold);
                    writer.Write(UseLowConfidenceThreshold);
                    writer.Write(LowConfidenceThreshold);
                    writer.Write(FilterRegex);
                    writer.Write(SplitRoutingNumber);
                    writer.Write(SplitAccountNumber);
                    writer.Write(SplitCheckNumber);
                    writer.Write(SplitAmount);
                    writer.Write(MicrSplitterRegex);
                    writer.Write(FilterCharsWhenSplitting);

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
        // Even though this currently does nothing, this method is here to keep the ICopyableObject
        // pattern consistent. Block FXCop warnings related to the fact this currently does nothing.
        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "source")]
        void CopyFrom(MicrFinder source)
        {
            HighConfidenceThreshold = source.HighConfidenceThreshold;
            UseLowConfidenceThreshold = source.UseLowConfidenceThreshold;
            LowConfidenceThreshold = source.LowConfidenceThreshold;
            FilterRegex = source.FilterRegex;
            SplitRoutingNumber = source.SplitRoutingNumber;
            SplitAccountNumber = source.SplitAccountNumber;
            SplitCheckNumber = source.SplitCheckNumber;
            SplitAmount = source.SplitAmount;
            MicrSplitterRegex = source.MicrSplitterRegex;
            FilterCharsWhenSplitting = source.FilterCharsWhenSplitting;
        }

        /// <summary>
        /// Finds MICR lines in <see cref="pDocument"/>.
        /// </summary>
        /// <param name="pDocument">The document to search.</param>
        IUnknownVector FindMicrs(AFDocument pDocument)
        {
            var results = new List<IAttribute>();

            using (SettingCollection settings = new SettingCollection())
            {
                settings.DefaultRecognitionModule = RECOGNITIONMODULE.RM_MAT;
                settings.DefaultFillingMethod = FILLINGMETHOD.FM_MICR;
                settings.Languages.LanguagesPlus = new string(new char[]
                    { _TRANSIT_CHAR, _AMOUNT_CHAR, _ON_US_CHAR, _DASH_CHAR });
                settings.DTXTOutputformat = DTXTOUTPUTFORMATS.DTXT_TXTS;

                var pagesToSearch = pDocument.Text.GetPages(true, "[BLANK]")
                    .ToIEnumerable<SpatialString>()
                    .Select(p => p.GetFirstPageNumber());

                foreach (var page in pagesToSearch)
                {
                    using (Page p = new Page(pDocument.Text.SourceDocName, page - 1, settings))
                    {
                        var pageResults = FindMicrsOnPage(pDocument, page, p);
                        results.AddRange(pageResults);
                    }
                }
            }

            var resultVector = results.ToIUnknownVector<IAttribute>();
            resultVector.ReportMemoryUsage();

            return resultVector;
        }

        /// <summary>
        /// Finds MICR lines on the specified page of a document.
        /// </summary>
        /// <returns>Each MICR line represented as an <see cref="IAttribute"/>.</returns>
        List<IAttribute> FindMicrsOnPage(AFDocument pDocument, int page, Page imagePage)
        {
            var results = new List<IAttribute>();

            imagePage.Preprocess();
            imagePage.Recognize();

            var spatialPageInfos = new LongToObjectMap();
            var letters = imagePage[IMAGEINDEX.II_CURRENT].GetLetters();

            if (letters.Any() && !spatialPageInfos.Contains(page))
            {
                InitSpatialPageInfo(imagePage, page, spatialPageInfos);
            }

            // Initialize loop variables the track current/previous letter and letters that are being built
            // up into the next candidate MICR line.
            var lastLetter = letters.FirstOrDefault();
            if (lastLetter == null)
            {
                return results;
            }

            var zoneLetters = new[] { lastLetter }.ToList();
            var letterEnumerator = letters.Skip(1).GetEnumerator();
            letterEnumerator.MoveNext();
            var letter = letterEnumerator.Current;
            ZoneData lastZone = null;
            var zoneStart = (zone: lastLetter.zone, top: lastLetter.top, bottom: lastLetter.top + lastLetter.height);

            // Loop thru every regonized char to compile candidate zones
            while (lastLetter != null)
            {
                // If the current letter is in the same Nuance-assigned zone as the previous and arranged
                // either to the left of right of the zone (not above/below), add this char to the candidate zone
                // and continue
                if (letter != null
                    && letter.zone == zoneStart.zone
                    && (letter.top == 0 || (letter.top < zoneStart.bottom && (letter.top + letter.height) > zoneStart.top)))
                {
                    zoneLetters.Add(letter);
                }
                else
                // At the end of the candidate zone; evaluate it.
                {
                    var micrZoneData = GetQualifiedZone(zoneLetters, pDocument, page, ref lastZone);

                    if (micrZoneData != null)
                    {
                        var attribute = CreateAttribute(micrZoneData, pDocument, spatialPageInfos);

                        if (micrZoneData == lastZone)
                        {
                            results.Remove(results.Last());
                            results.Add(attribute);
                        }
                        else
                        {
                            lastZone = micrZoneData;
                            results.Add(attribute);
                        }
                    }

                    if (letter != null)
                    {
                        zoneLetters = new[] { letter }.ToList();
                        zoneStart = (zone: letter.zone, top: letter.top, bottom: letter.top + letter.height);
                    }
                }

                lastLetter = letter;
                letter = letterEnumerator.MoveNext()
                    ? letterEnumerator.Current
                    : null;
            }

            return results;
        }

        /// <summary>
        /// Initialize the <see cref="SpatialPageInfo"/> within <see paramref="spatialPageInfos"/>
        /// for the current page (to be used by any recognized MICR attributes).
        /// </summary>
        static void InitSpatialPageInfo(Page imagePage, int pageNumber, LongToObjectMap spatialPageInfos)
        {
            var size = imagePage[IMAGEINDEX.II_CURRENT].ImageInfo.Size;
            var pageInfo = new SpatialPageInfo();
            var deskew = Math.Atan2(imagePage.PreprocessInfo.Slope, 1000) * (180.0 / Math.PI);
            EOrientation orientation = EOrientation.kRotNone;
            switch (imagePage.PreprocessInfo.Rotation)
            {
                case IMG_ROTATE.ROT_NO:
                    orientation = EOrientation.kRotNone;
                    break;

                case IMG_ROTATE.ROT_RIGHT:
                    orientation = EOrientation.kRotRight;
                    break;

                case IMG_ROTATE.ROT_DOWN:
                    orientation = EOrientation.kRotDown;
                    break;

                case IMG_ROTATE.ROT_LEFT:
                    orientation = EOrientation.kRotLeft;
                    break;

                default:
                    throw new ExtractException("ELI46901", "");
            }
            var width = (orientation == EOrientation.kRotNone || orientation == EOrientation.kRotDown)
                ? size.cx
                : size.cy;
            var height = (orientation == EOrientation.kRotNone || orientation == EOrientation.kRotDown)
                ? size.cy
                : size.cx;
            pageInfo.Initialize(width, height, orientation, deskew);

            spatialPageInfos.Set(pageNumber, pageInfo);
        }

        /// <summary>
        /// Evaluates <see paramref="zoneLetters"/> as a MICR zone candidate.
        /// </summary>
        /// <param name="lastZone">The previously qualified zone; Used to merge separate zones along
        /// the same line if needed.</param>
        /// <returns>If the letters are determined to represent a qualified MICR line, an <see cref="ZoneData"/>
        /// instance representing the MICR line is returned. In the case that the new zone is to be merged with
        /// the last, both the return value and lastZone will be this newly merged zone.
        /// <c>null</c> if the letters do not represent a qualified zone.
        /// </returns>
        ZoneData GetQualifiedZone(IEnumerable<LETTER> zoneLetters, AFDocument document, int page, ref ZoneData lastZone)
        {
            if (!zoneLetters.Any(l => l.code != ' '))
            {
                return null;
            }

            var rect = new LongRectangle();
            rect.SetBounds(zoneLetters.Min(l => l.left),
                zoneLetters.Min(l => l.top),
                zoneLetters.Max(l => l.left + l.width),
                zoneLetters.Max(l => l.top + l.height));
            var micrZone = new ComRasterZone();

            micrZone.CreateFromLongRectangle(rect, page);

            // Check if new zone should be combined with the previous zone.
            if (lastZone != null
                && (rect.Left > lastZone.RasterZone.EndX || rect.Right < lastZone.RasterZone.StartX)
                && rect.Top < lastZone.RasterZone.EndY
                && rect.Bottom > lastZone.RasterZone.EndY)
            {
                var combinedRect = micrZone.GetBoundsFromMultipleRasterZones(
                    new[] { lastZone.RasterZone, micrZone }.ToIUnknownVector(), null);

                var combinedZone = new ComRasterZone();
                combinedZone.CreateFromLongRectangle(combinedRect, page);

                var combinedLetters = (zoneLetters.First().left > lastZone.Letters.First().left)
                    ? lastZone.Letters
                        .Concat(new[] { new LETTER() { code = ' ' } })
                        .Concat(zoneLetters)
                    : zoneLetters
                        .Concat(new[] { new LETTER() { code = ' ' } })
                        .Concat(lastZone.Letters);

                if (QualifyZone(combinedZone, combinedLetters, document))
                {
                    lastZone = new ZoneData() { RasterZone = combinedZone, Letters = combinedLetters };
                    return lastZone;
                }
                else
                {
                    lastZone = null;
                }
            }

            // Otherwise, qualify this new zone individually
            if (QualifyZone(micrZone, zoneLetters, document))
            {
                return new ZoneData() { RasterZone = micrZone, Letters = zoneLetters }; ;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Determines if the specified <see paramref="micrZone"/> qualifies as a valid MICR zone.
        /// </summary>
        bool QualifyZone(ComRasterZone micrZone, IEnumerable<LETTER> zoneLetters, AFDocument document)
        {
            if (!zoneLetters.Any())
            {
                return false;
            }

            var confidence = (int)zoneLetters
                .Where(l => !char.IsWhiteSpace(l.code))
                .Select(l => 100 - Math.Min(99, l.err & RE_ERROR_LEVEL_MASK))
                .Average();

            if (FilterRegexInstance != null)
            {
                var text = new string(zoneLetters.Select(l => l.code).ToArray());
                if (!FilterRegexInstance.IsMatch(text))
                {
                    return false;
                }
            }

            if (confidence >= HighConfidenceThreshold)
            {
                return true;
            }

            if (UseLowConfidenceThreshold && confidence >= LowConfidenceThreshold)
            {
                var ocrData = GetOriginalOcrData(document);

                var originalOcr = GetOCRText(micrZone, ocrData);
                originalOcr.Trim(" \t\r\n", " \t\r\n");
                int min = 0, max = 0, origOcrConf = 0;
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
        static SpatialString GetOCRText(ComRasterZone rasterZone, SpatialString documentSource)
        {
            // [FlexIDSCore:5093] Don't process any pages without spatial info.
            int page = rasterZone.PageNumber;
            if (documentSource.GetSpecifiedPages(page, page).IsEmpty())
            {
                return null;
            }

            SpatialStringSearcher searcher = GetSearcherForPage(page, documentSource);

            // So that the garbage collector knows of and properly manages the associated
            // memory.
            searcher.ReportMemoryUsage();

            var pageInfo = documentSource.GetPageInfo(rasterZone.PageNumber);
            var pageBounds = new LongRectangle();
            pageBounds.SetBounds(0, 0, pageInfo.Width, pageInfo.Height);
            LongRectangle bounds = rasterZone.GetRectangularBounds(pageBounds);
            var ocrText = searcher.GetDataInRegion(bounds, false);
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
        /// Creates an <see cref="IAttribute"/> to reprevent the specified <see cref="ZoneData"/>.
        /// This includes splitting out MICR components if so configured.
        /// </summary>
        IAttribute CreateAttribute(ZoneData micrZoneData, AFDocument pDocument, LongToObjectMap spatialPageInfos)
        {
            var letters = micrZoneData.Letters.Select(zoneLetter =>
            {
                var letter = new Letter();
                switch (zoneLetter.code)
                {
                    case _TRANSIT_CHAR:
                        letter.Guess1 = 'T';
                        break;

                    case _AMOUNT_CHAR:
                        letter.Guess1 = 'A';
                        break;

                    case _ON_US_CHAR:
                        letter.Guess1 = 'U';
                        break;

                    case _DASH_CHAR:
                        letter.Guess1 = 'D';
                        break;

                    case _UNRECOGNIZED_CHAR:
                        letter.Guess1 = '?';
                        break;

                    default:
                        letter.Guess1 = (char)zoneLetter.code;
                        break;
                }

                letter.Bottom = zoneLetter.top + zoneLetter.height;
                letter.CharConfidence = (zoneLetter.err > 127) ? 0 : (100 - zoneLetter.err);
                letter.FontSize = (int)zoneLetter.pointSize;
                letter.IsBold = (zoneLetter.fontAttrib & FONTATTRIB.R_BOLD) > 0;
                letter.IsItalic = (zoneLetter.fontAttrib & FONTATTRIB.R_ITALIC) > 0;
                letter.IsSpatialChar = !char.IsWhiteSpace(zoneLetter.code);
                letter.IsSubScript = (zoneLetter.fontAttrib & FONTATTRIB.R_SUBSCRIPT) > 0;
                letter.IsSuperScript = (zoneLetter.fontAttrib & FONTATTRIB.R_SUPERSCRIPT) > 0;
                letter.IsUnderline = (zoneLetter.fontAttrib & FONTATTRIB.R_UNDERLINE) > 0;
                letter.Left = zoneLetter.left;
                letter.PageNumber = micrZoneData.RasterZone.PageNumber;
                letter.Right = zoneLetter.left + zoneLetter.width;
                letter.Top = zoneLetter.top;
                return letter;
            }).ToList();

            if (!letters.Last().IsSpatialChar)
            {
                letters.Remove(letters.Last());
            }

            letters.Last().IsEndOfParagraph = true;
            letters.Last().IsEndOfZone = true;

            var spatialString = new SpatialString();
            spatialString.CreateFromILetters(letters.ToIUnknownVector<ILetter>(), pDocument.Text.SourceDocName, spatialPageInfos);

            var attribute = new AttributeClass();
            attribute.Value = spatialString;

            SplitMicrComponents(attribute);

            return attribute;
        }

        /// <summary>
        /// Creates sub-attributes to the specified attribute to represent recognized MICR components
        /// per configuration.
        /// </summary>
        void SplitMicrComponents(IAttribute micrAttribute)
        {
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
        string GetRegex(string regexSpecification, AFDocument document)
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
        DotNetRegexParser CharRemovalParser
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

        MiscUtils MiscUtils
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

        #endregion Private Members
    }
}
