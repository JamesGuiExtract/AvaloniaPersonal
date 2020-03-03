using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Extract.Imaging;
using Extract.Imaging.Forms;
using Extract.Imaging.Utilities;
using Extract.Interop;
using Extract.Licensing;
using Extract.Utilities;
using Microsoft.Win32;
using UCLID_AFCORELib;
using UCLID_AFSELECTORSLib;
using UCLID_COMLMLib;
using UCLID_COMUTILSLib;

using ComAttribute = UCLID_AFCORELib.Attribute;
using ComRasterZone = UCLID_RASTERANDOCRMGMTLib.RasterZone;
using SpatialString = UCLID_RASTERANDOCRMGMTLib.SpatialString;

namespace Extract.AttributeFinder.Rules
{
    /// <summary>
    /// An interface for the <see cref="AutoShrinkRedactionZones"/> class.
    /// </summary>
    [ComVisible(true)]
    [Guid("5BA2E8E7-814B-4057-8412-E3ED7EAAD407")]
    [CLSCompliant(false)]
    public interface IAutoShrinkRedactionZones : IOutputHandler, ICategorizedComponent,
        IConfigurableObject, ICopyableObject, ILicensedComponent, IPersistStream,
        IMustBeConfiguredObject, IIdentifiableObject
    {

        /// <summary>
        /// Gets or sets the <see cref="IAttributeSelector"/> used to specify which attribute(s)
        /// are to be shrunk.
        /// </summary>
        /// <value>
        /// The <see cref="IAttributeSelector"/> used to specify which attribute(s) are to be shrunk.
        /// </value>
        IAttributeSelector AttributeSelector
        {
            get;
            set;
        }
    }

    /// <summary>
    /// An <see cref="IOutputHandler"/> that auto-shrinks the spatial area of selected attributes
    /// so that they do not unnecessarily cover whitespace.
    /// </summary>
    [ComVisible(true)]
    [Guid("A2D53C02-EEA2-4BB0-8374-DA2BA13716B9")]
    [CLSCompliant(false)]
    public class AutoShrinkRedactionZones : IdentifiableObject, IAutoShrinkRedactionZones
    {
        #region Constants

        /// <summary>
        /// The description of the rule.
        /// </summary>
        const string _COMPONENT_DESCRIPTION = "Auto-shrink redaction zones";

        /// <summary>
        /// Current version.
        /// </summary>
        const int _CURRENT_VERSION = 2;

        /// <summary>
        /// The license id to validate in licensing calls
        /// </summary>
        const LicenseIdName _LICENSE_ID = LicenseIdName.FlexIndexIDShieldCoreObjects;

        // Minimum height to accept as a shrunk zone
        const int _MIN_SHRINK_HEIGHT = 10;

        /// <summary>
        /// Registry key where the AutoFitZonePadding setting is stored
        /// </summary>
        const string _IMAGING_SETTINGS_KEYNAME = @"HKEY_CURRENT_USER\Software\Extract Systems\Imaging";

        /// <summary>
        /// Default value for AutoFitZonePadding (in case no value can be obtained from the registry)
        /// </summary>
        const int _DEFAULT_AUTO_FIT_ZONE_PADDING = 1;

        #endregion Constants

        #region Fields

        /// <summary>
        /// The <see cref="IAttributeSelector"/> used to specify which attribute(s) are to be shrunk.
        /// </summary>
        IAttributeSelector _attributeSelector;

        /// <summary>
        /// Amount to inflate zones after auto-shrink
        /// </summary>
        int _padding;

        /// <summary>
        /// <see langword="true"/> if changes have been made to this instance since it was created;
        /// <see langword="false"/> if no changes have been made since it was created.
        /// </summary>
        bool _dirty;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="AutoShrinkRedactionZones"/> class.
        /// </summary>
        public AutoShrinkRedactionZones()
        {
            try
            {
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI38493");
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AutoShrinkRedactionZones"/> class as a
        /// copy of <see paramref="source"/>.
        /// </summary>
        /// <param name="source">The <see cref="AutoShrinkRedactionZones"/> from which
        /// settings should be copied.</param>
        public AutoShrinkRedactionZones(AutoShrinkRedactionZones source)
        {
            try
            {
                CopyFrom(source);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI38494");
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the <see cref="IAttributeSelector"/> used to specify which attribute(s)
        /// are to be shrunk.
        /// </summary>
        /// <value>
        /// The <see cref="IAttributeSelector"/> used to specify which attribute(s) are to be shrunk.
        /// </value>
        public IAttributeSelector AttributeSelector
        {
            get
            {
                if (_attributeSelector == null)
                {
                    _attributeSelector = (IAttributeSelector)
                        new QueryBasedAS { QueryText = "HCData|MCData|LCData|NCData" };
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
                    throw ex.AsExtract("ELI38495");
                }
            }
        }

        /// <summary>
        /// Whether to first expand each edge until row/column of white pixels before shrinking down to black pixels
        /// </summary>
        public bool AutoExpandBeforeAutoShrink { get; set; } = true;

        /// <summary>
        /// The maximum number of pixels allowed for expansion
        /// </summary>
        public float MaxPixelsToExpand { get; set; } = 10;

        #endregion Properties

        #region IOutputHandler Members

        /// <summary>
        /// Processes the output (<see paramref="pAttributes"/>) by shrinking selected attributes.
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
                LicenseUtilities.ValidateLicense(_LICENSE_ID, "ELI38496", _COMPONENT_DESCRIPTION);

                ExtractException.Assert("ELI38497", "Rule is not properly configured.",
                    IsConfigured());

                // Attempt to unlock PDF support
                ExtractException ee = UnlockLeadtools.UnlockPdfSupport(returnExceptionIfUnlicensed: false);
                if (ee != null)
                {
                    throw ee;
                }

                // So that the garbage collector knows of and properly manages the associated
                // memory.
                pAttributes.ReportMemoryUsage();

                // Obtain all attributes specified to be shrunk.
                IEnumerable<ComAttribute> selectedAttributes;
                using (RuleObjectProfiler profiler =
                    new RuleObjectProfiler("", "", AttributeSelector, 0))
                {
                    selectedAttributes = AttributeSelector.SelectAttributes(pAttributes, pDoc, pAttributes)
                        .ToIEnumerable<ComAttribute>();
                }

                // Set minimum shrink size to the same value that IDShield uses
                ZoneGeometry.MinSize = LayerObject.MinSize;

                // Set padding value from registry if the setting exists. Need to add 1 to the value
                // to emulate fitEdge's use of AutoFitZonePadding
                int autoFitZonePadding;
                if (Int32.TryParse((string)Registry.GetValue
                    (_IMAGING_SETTINGS_KEYNAME, "AutoFitZonePadding", ""), out autoFitZonePadding))
                {
                    _padding = autoFitZonePadding + 1;
                }
                else
                {
                    _padding = _DEFAULT_AUTO_FIT_ZONE_PADDING + 1;
                }

                // Process each of the selected attributes.
                using (ImageCodecs codecs = new ImageCodecs())
                using (ImageReader imageReader = codecs.CreateReader(pDoc.Text.SourceDocName))
                foreach (ComAttribute attribute in selectedAttributes)
                {
                    AutoShrinkAttribute(attribute, imageReader);
                }
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI38498", "Failed to shrink redaction zones.");
            }
        }

        #endregion IOutputHandler Members

        #region IConfigurableObject Members

        /// <summary>
        /// Displays a form to allow configuration of this <see cref="AutoShrinkRedactionZones"/>
        /// instance.
        /// </summary>
        /// <returns><see langword="true"/> if the configuration was successfully updated or
        /// <see langword="false"/> if configuration was unsuccessful.</returns>
        public bool RunConfiguration()
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(_LICENSE_ID, "ELI38499", _COMPONENT_DESCRIPTION);

                // Make a clone to update settings and only copy if ok
                AutoShrinkRedactionZones cloneOfThis = (AutoShrinkRedactionZones)Clone();

                using (AutoShrinkRedactionZonesSettingsDialog dlg
                    = new AutoShrinkRedactionZonesSettingsDialog(cloneOfThis))
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
                throw ex.CreateComVisible("ELI38500", "Error running configuration.");
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
                return AttributeSelector != null;
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI38501",
                    "Error checking configuration of Auto shrink redaction zones.");
            }
        }

        #endregion IMustBeConfiguredObject Members

        #region ICopyableObject Members

        /// <summary>
        /// Creates a copy of the <see cref="AutoShrinkRedactionZones"/> instance.
        /// </summary>
        /// <returns>A copy of the <see cref="AutoShrinkRedactionZones"/> instance.
        /// </returns>
        public object Clone()
        {
            try
            {
                return new AutoShrinkRedactionZones(this);
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI38502",
                    "Failed to clone '" + _COMPONENT_DESCRIPTION + "' object.");
            }
        }

        /// <summary>
        /// Copies the specified <see cref="AutoShrinkRedactionZones"/> instance into this one.
        /// </summary>
        /// <param name="pObject">The object from which to copy.</param>
        public void CopyFrom(object pObject)
        {
            try
            {
                var source = pObject as AutoShrinkRedactionZones;
                if (source == null)
                {
                    throw new InvalidCastException("Invalid cast to AutoShrinkRedactionZones");
                }
                CopyFrom(source);
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI38503",
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

                    if (reader.Version >= 2)
                    {
                        AutoExpandBeforeAutoShrink = reader.ReadBoolean();
                        MaxPixelsToExpand = reader.ReadSingle();
                    }
                    else
                    {
                        AutoExpandBeforeAutoShrink = false;
                    }

                    // Load the GUID for the IIdentifiableObject interface.
                    LoadGuid(stream);
                }

                // Freshly loaded object is no longer dirty
                _dirty = false;
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI38504",
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
                    writer.Write(ComUtilities.GetIPersistStreamInterface(AttributeSelector), clearDirty);
                    writer.Write(AutoExpandBeforeAutoShrink);
                    writer.Write(MaxPixelsToExpand);

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
                throw ex.CreateComVisible("ELI38505",
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
        /// Copies the specified <see cref="AutoShrinkRedactionZones"/> instance into this one.
        /// </summary><param name="source">The <see cref="AutoShrinkRedactionZones"/> from which to copy.
        /// </param>
        void CopyFrom(AutoShrinkRedactionZones source)
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

            AutoExpandBeforeAutoShrink = source.AutoExpandBeforeAutoShrink;
            MaxPixelsToExpand = source.MaxPixelsToExpand;

            _dirty = true;
        }

        /// <summary>
        /// Shrinks an <see paramref="attribute"/>
        /// </summary>
        /// <param name="attribute">The <see cref="ComAttribute"/> to shrink.</param>
        /// <param name="imageReader">The image reader to use for edge fitting.</param>
        void AutoShrinkAttribute(ComAttribute attribute, ImageReader imageReader)
        {
            SpatialString value = attribute.Value;
            if (value.HasSpatialInfo())
            {
                IUnknownVector zones = value.GetOriginalImageRasterZones();
                var shrunkZones = zones.ToIEnumerable<ComRasterZone>()
                    .Select(z =>
                    {
                        var zone = new RasterZone(z);
                        using (PixelProbe probe = imageReader.CreatePixelProbe(zone.PageNumber))
                        {
                            ZoneGeometry data = new ZoneGeometry(zone);

                            if (AutoExpandBeforeAutoShrink)
                            {
                                float max = Math.Max(1, MaxPixelsToExpand);
                                data.FitEdge(Side.Left, probe, false, false, null, 0, 0, 0, max);
                                data.FitEdge(Side.Top, probe, false, false, null, 0, 0, 0, max);
                                data.FitEdge(Side.Right, probe, false, false, null, 0, 0, 0, max);
                                data.FitEdge(Side.Bottom, probe, false, false, null, 0, 0, 0, max);
                            }

                            // Shrink each side of the zone. Use no padding here because zones will be
                            // inflated next.
                            data.FitEdge(Side.Left, probe, true, true, null, 0);
                            data.FitEdge(Side.Top, probe, true, true, null, 0);
                            data.FitEdge(Side.Right, probe, true, true, null, 0);
                            data.FitEdge(Side.Bottom, probe, true, true, null, 0, 0);

                            // Inflate each side of the zone to be sure that all text is covered.
                            // This will make redactions that match the typical results of using
                            // auto-shrink (block fitting) or word redaction in IDShield since manual
                            // redactions tend to be drawn larger than necessary (and so are padded by
                            // ZoneGeometry.FitEdge) and since the WordHighlightManager pads the OCR
                            // zones that are used by the word highlighter redaction tool.
                            data.InflateSide(Side.Left, _padding);
                            data.InflateSide(Side.Top, _padding);
                            data.InflateSide(Side.Right, _padding);
                            data.InflateSide(Side.Bottom, _padding);

                            zone = data.ToRasterZone(RoundingMode.Safe);
                        }
                        return (zone.Height < _MIN_SHRINK_HEIGHT) ? null : zone.ToComRasterZone();
                    })
                    .Where(shrunkZone => shrunkZone != null)
                    .ToIUnknownVector<ComRasterZone>();

                // Update the spatial string
                // Leave the value the way it was if no zones are left after shrinking
                // https://extract.atlassian.net/browse/ISSUE-13531
                if (shrunkZones.Size() > 0)
                {
                    LongToObjectMap ocrPageInfos = value.SpatialPageInfos;
                    LongToObjectMap imagePageInfos = value.GetUnrotatedPageInfoMap();
                    value.CreateHybridString(shrunkZones, value.String, value.SourceDocName, imagePageInfos);
                    value.TranslateToNewPageInfo(ocrPageInfos);
                }
            }
        }

        #endregion Private Members
    }
}
