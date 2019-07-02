using Extract.Interop;
using Extract.Licensing;
using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using UCLID_AFCORELib;
using UCLID_COMLMLib;
using UCLID_COMUTILSLib;
using UCLID_RASTERANDOCRMGMTLib;

using ComAttribute = UCLID_AFCORELib.Attribute;

namespace Extract.AttributeFinder.Rules
{
    /// <summary>
    /// An interface for the <see cref="ExtractOcrTextInImageArea"/> class.
    /// </summary>
    [ComVisible(true)]
    [Guid("159E9B31-B411-44D7-AFC4-1D0194B992FC")]
    [CLSCompliant(false)]
    public interface IExtractOcrTextInImageArea : IAttributeModifyingRule, ICategorizedComponent,
        IConfigurableObject, ICopyableObject, ILicensedComponent, IPersistStream,
        IIdentifiableObject
    {
        /// <summary>
        /// Gets or sets whether the original document OCR results should be used instead of the OCR
        /// from the document context passed to the rule.
        /// </summary>
        bool UseOriginalDocumentOcr
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets whether to use the overall attribute bounds or the bounds from each raster
        /// zone separately as the area from which to extract the text.
        /// </summary>
        bool UseOverallBounds
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets whether to include text that intersects with the bounds rather than being
        /// completely inside of the bounds.
        /// </summary>
        bool IncludeTextOnBoundary
        {
            get;
            set;
        }

        /// <summary>
        /// For <see cref="IncludeTextOnBoundary"/>, specifies what the boundaries are when
        /// determining how much text is considered intersecting text.
        /// </summary>
        ESpatialEntity SpatialEntityType
        {
            get;
            set;
        }
    }

    /// <summary>
    /// An <see cref="IAttributeModifyingRule"/> that can extract original OCR text from image area.
    /// </summary>
    [ComVisible(true)]
    [Guid("FF9E81A6-EBE7-4C95-A968-E836A255C37B")]
    [CLSCompliant(false)]
    public class ExtractOcrTextInImageArea : IdentifiableObject, IExtractOcrTextInImageArea
    {
        #region Constants

        /// <summary>
        /// The description of the rule
        /// </summary>
        const string _COMPONENT_DESCRIPTION = "Extract OCR text in image area";

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
        /// Indicates whether the original document OCR results should be used instead of the OCR
        /// from the document context passed to the rule.
        /// </summary>
        bool _useOriginalDocumentOcr;

        /// <summary>
        /// Indicates whether to use the overall attribute bounds or the bounds from each raster
        /// zone separately as the area from which to extract the text.
        /// </summary>
        bool _useOverallBounds;

        /// <summary>
        /// Indicates whether to include text that intersects with the bounds rather than being
        /// completely inside of the bounds.
        /// </summary>
        bool _includeTextOnBoundary;

        /// <summary>
        /// For <see cref="IncludeTextOnBoundary"/>, specifies what the boundaries are when
        /// determining how much text is considered intersecting text.
        /// </summary>
        ESpatialEntity _spatialEntityType;

        /// <summary>
        /// A cached set of <see cref="SpatialStringSearcher"/>s for each page of the current
        /// document so as not to have to re-initialize a searcher multiple times for the same page.
        /// </summary>
        Dictionary<int, SpatialStringSearcher> _searchers =
            new Dictionary<int, SpatialStringSearcher>();

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
        /// <see langword="true"/> if changes have been made to <see cref="ExtractOcrTextInImageArea"/>
        /// since it was created; <see langword="false"/> if no changes have been made since it was
        /// created.
        /// </summary>
        bool _dirty;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtractOcrTextInImageArea"/> class.
        /// </summary>
        public ExtractOcrTextInImageArea()
        {
            try
            {
                UseOriginalDocumentOcr = false;
                UseOverallBounds = true;
                IncludeTextOnBoundary = true;
                SpatialEntityType = ESpatialEntity.kCharacter;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI33708");
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExtractOcrTextInImageArea"/> class as a
        /// copy of the specified <see paramref="extractOcrTextInImageArea"/>.
        /// </summary>
        /// <param name="extractOcrTextInImageArea">The <see cref="ExtractOcrTextInImageArea"/>
        /// from which settings should be copied.</param>
        public ExtractOcrTextInImageArea(ExtractOcrTextInImageArea extractOcrTextInImageArea)
        {
            try
            {
                CopyFrom(extractOcrTextInImageArea);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI33709");
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets whether the original document OCR results should be used instead of the OCR
        /// from the document context passed to the rule.
        /// </summary>
        public bool UseOriginalDocumentOcr
        {
            get
            {
                return _useOriginalDocumentOcr;
            }

            set
            {
                if (value != _useOriginalDocumentOcr)
                {
                    _useOriginalDocumentOcr = value;
                    _dirty = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets whether to use the overall attribute bounds or the bounds from each raster
        /// zone separately as the area from which to extract the text.
        /// </summary>
        public bool UseOverallBounds
        {
            get
            {
                return _useOverallBounds;
            }

            set
            {
                if (value != _useOverallBounds)
                {
                    _useOverallBounds = value;
                    _dirty = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets whether to include text that intersects with the bounds rather than being
        /// completely inside of the bounds.
        /// </summary>
        public bool IncludeTextOnBoundary
        {
            get
            {
                return _includeTextOnBoundary;
            }

            set
            {
                if (value != _includeTextOnBoundary)
                {
                    _includeTextOnBoundary = value;
                    _dirty = true;
                }
            }
        }

        /// <summary>
        /// For <see cref="IncludeTextOnBoundary"/>, specifies what the boundaries are when
        /// determining how much text is considered intersecting text.
        /// </summary>
        public ESpatialEntity SpatialEntityType
        {
            get
            {
                return _spatialEntityType;
            }

            set
            {
                if (value != _spatialEntityType)
                {
                    _spatialEntityType = value;
                    _dirty = true;
                }
            }
        }

        #endregion Properties

        #region IAttributeModifyingRule

        /// <summary>
        /// Modifies the attribute value by replacing it with the OCR text from its image area.
        /// </summary>
        /// <param name="pAttributeToBeModified">The attribute to be modified.</param>
        /// <param name="pOriginInput">The original <see cref="AFDocument"/>.</param>
        /// <param name="pProgressStatus">A <see cref="ProgressStatus"/> instance that can be used
        /// to indicate progress.</param>
        public void ModifyValue(ComAttribute pAttributeToBeModified, AFDocument pOriginInput, ProgressStatus pProgressStatus)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(_LICENSE_ID, "ELI33710", _COMPONENT_DESCRIPTION);

                SpatialString sourceString = null;

                // Initialize sourceString either from the uss file or from pOriginInput.
                if (UseOriginalDocumentOcr)
                {
                    sourceString = GetOriginalOcrData(pOriginInput);
                }
                else
                {
                    sourceString = pOriginInput.Text;
                }

                // So that the garbage collector knows of and properly manages the associated
                // memory.
                sourceString.ReportMemoryUsage();
                pAttributeToBeModified.ReportMemoryUsage();

                // If there is no image area associated with this attribute, the resulting value
                // will be null.
                SpatialString extractedText = null;

                // Loop through every raster zone to be extracted for this attribute.
                foreach (RasterZone rasterZone in
                    GetZonesToExtract(pAttributeToBeModified.Value, sourceString))
                {
                    int page = rasterZone.PageNumber;
                    LongRectangle bounds = rasterZone.GetRectangularBounds(
                        sourceString.GetOCRImagePageBounds(page));

                    SpatialStringSearcher searcher = GetSearcherForPage(page, sourceString);
                    // So that the garbage collector knows of and properly manages the associated
                    // memory.
                    searcher.ReportMemoryUsage();

                    // If this is the first zone, initialize extractedText with the result.
                    if (extractedText == null)
                    {
                        extractedText = searcher.GetDataInRegion(bounds, false);
                    }
                    // Otherwise, append this result to the existing value.
                    else
                    {
                        extractedText.Append(searcher.GetDataInRegion(bounds, false));
                    }
                }

                pAttributeToBeModified.Value = extractedText;

                // Report memory usage of heirarchy after processing to ensure all COM objects
                // referenced in final result are reported.
                pAttributeToBeModified.ReportMemoryUsage();
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI33711", "Failed to extract OCR text from image area.");
            }
            finally
            {
                _searchers.Clear();
            }
        }

        #endregion IAttributeModifyingRule

        #region IConfigurableObject Members

        /// <summary>
        /// Performs configuration needed to create a valid <see cref="ExtractOcrTextInImageArea"/>.
        /// </summary>
        /// <returns><see langword="true"/> if the configuration was successfully updated or
        /// <see langword="false"/> if configuration was unsuccessful.</returns>
        public bool RunConfiguration()
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(_LICENSE_ID, "ELI33712", _COMPONENT_DESCRIPTION);

                // Make a clone to update settings and only copy if ok
                ExtractOcrTextInImageArea cloneOfThis = (ExtractOcrTextInImageArea)Clone();

                using (ExtractOcrTextInImageAreaSettingsDialog dlg
                    = new ExtractOcrTextInImageAreaSettingsDialog(cloneOfThis))
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
                throw ex.CreateComVisible("ELI33713", "Error running configuration.");
            }
        }

        #endregion IConfigurableObject Members

        #region ICopyableObject Members

        /// <summary>
        /// Creates a copy of the <see cref="ExtractOcrTextInImageArea"/> instance.
        /// </summary>
        /// <returns>A copy of the <see cref="ExtractOcrTextInImageArea"/> instance.
        /// </returns>
        public object Clone()
        {
            try
            {
                return new ExtractOcrTextInImageArea(this);
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI33714",
                    "Failed to clone '" + _COMPONENT_DESCRIPTION + "' object.");
            }
        }

        /// <summary>
        /// Copies the specified <see cref="ExtractOcrTextInImageArea"/> instance into this one.
        /// </summary>
        /// <param name="pObject">The object from which to copy.</param>
        public void CopyFrom(object pObject)
        {
            try
            {
                var source = pObject as ExtractOcrTextInImageArea;
                if (source == null)
                {
                    throw new InvalidCastException("Invalid cast to ExtractOcrTextInImageArea");
                }
                CopyFrom(source);
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI33715",
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
                    UseOriginalDocumentOcr = reader.ReadBoolean();
                    UseOverallBounds = reader.ReadBoolean();
                    IncludeTextOnBoundary = reader.ReadBoolean();
                    SpatialEntityType = (ESpatialEntity)reader.ReadInt32();

                    // Load the GUID for the IIdentifiableObject interface.
                    LoadGuid(stream);
                }

                // Freshly loaded object is no longer dirty
                _dirty = false;
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI33716",
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
                    writer.Write(UseOriginalDocumentOcr);
                    writer.Write(UseOverallBounds);
                    writer.Write(IncludeTextOnBoundary);
                    writer.Write((int)SpatialEntityType);

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
                throw ex.CreateComVisible("ELI33717",
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
        /// "UCLID AF-API Value Modifiers" COM category.
        /// </summary>
        /// <param name="type">The <paramref name="type"/> being registered.</param>
        [ComRegisterFunction]
        [ComVisible(false)]
        static void RegisterFunction(Type type)
        {
            ComMethods.RegisterTypeInCategory(type, ExtractCategories.ValueModifiersGuid);
        }

        /// <summary>
        /// Code to be executed upon unregistration in order to remove this class from the
        /// "UCLID AF-API Value Modifiers" COM category.
        /// </summary>
        /// <param name="type">The <paramref name="type"/> being unregistered.</param>
        [ComUnregisterFunction]
        [ComVisible(false)]
        static void UnregisterFunction(Type type)
        {
            ComMethods.UnregisterTypeInCategory(type, ExtractCategories.ValueModifiersGuid);
        }

        /// <summary>
        /// Copies the specified <see cref="ExtractOcrTextInImageArea"/> instance into this one.
        /// </summary><param name="source">The <see cref="ExtractOcrTextInImageArea"/> from which
        /// to copy.</param>
        void CopyFrom(ExtractOcrTextInImageArea source)
        {
            UseOriginalDocumentOcr = source.UseOriginalDocumentOcr;
            UseOverallBounds = source.UseOverallBounds;
            IncludeTextOnBoundary = source.IncludeTextOnBoundary;
            SpatialEntityType = source.SpatialEntityType;

            _dirty = true;
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

            ExtractException.Assert("ELI33721",
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
        /// Gets the <see cref="RasterZone"/>s from which OCR text is to be extracted.
        /// </summary>
        /// <param name="spatialString">The attribute's value.</param>
        /// <param name="documentSource">The document's source text.</param>
        /// <returns>The <see cref="RasterZone"/>s from which OCR text should be extracted.
        /// </returns>
        IEnumerable<RasterZone> GetZonesToExtract(SpatialString spatialString,
            SpatialString documentSource)
        {
            if (spatialString.HasSpatialInfo())
            {
                // Loop through each page of the attribute.
                foreach (SpatialString pageText in
                    spatialString.GetPages(false, "").ToIEnumerable<SpatialString>())
                {
                    // [FlexIDSCore:5093] Don't process any pages without spatial info.
                    int page = pageText.GetFirstPageNumber();
                    if (documentSource.GetSpecifiedPages(page, page).IsEmpty())
                    {
                        continue;
                    }

                    // If using the overall bounds, there will be only one result for each page:
                    // the overall attribute bounds.
                    if (UseOverallBounds)
                    {
                        RasterZone rasterZone = new RasterZone();
                        rasterZone.CreateFromLongRectangle(
                            pageText.GetTranslatedImageBounds(documentSource.SpatialPageInfos),
                            pageText.GetFirstPageNumber());

                        yield return rasterZone;
                    }
                    else
                    {
                        foreach (RasterZone rasterZone in
                            pageText.GetTranslatedImageRasterZones(documentSource.SpatialPageInfos)
                                .ToIEnumerable<RasterZone>())
                        {
                            yield return rasterZone;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets a <see cref="SpatialStringSearcher"/> for the <see paramref="page"/>. This method
        /// will a searcher for each page to avoid needing to needlessly re-initialize.
        /// </summary>
        /// <param name="page">The page for which the searcher is needed.</param>
        /// <param name="sourceString">The source string representing the OCR text.</param>
        /// <returns>A <see cref="SpatialStringSearcher"/> for the <see paramref="page"/>.</returns>
        SpatialStringSearcher GetSearcherForPage(int page, SpatialString sourceString)
        {
            return _searchers.GetOrAdd(page, _ =>
            {
                var searcher = new SpatialStringSearcher();
                searcher.InitSpatialStringSearcher(sourceString.GetSpecifiedPages(page, page), false);
                searcher.SetIncludeDataOnBoundary(IncludeTextOnBoundary);
                searcher.SetBoundaryResolution(SpatialEntityType);
                return searcher;
            });
        }

        #endregion Private Members
    }
}
