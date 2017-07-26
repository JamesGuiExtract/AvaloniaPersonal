using Extract.Interop;
using Extract.Licensing;
using Extract.Utilities;
using Nuance.OmniPage.CSDK;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using UCLID_AFCORELib;
using UCLID_COMLMLib;
using UCLID_COMUTILSLib;
using UCLID_RASTERANDOCRMGMTLib;

namespace Extract.AttributeFinder.Rules
{
    /// <summary>
    /// An interface for the <see cref="TemplateFinder"/> class.
    /// </summary>
    [ComVisible(true)]
    [Guid("6655341D-076F-4A99-AEC2-6F1DD505A993")]
    [CLSCompliant(false)]
    public interface ITemplateFinder : IAttributeFindingRule, ICategorizedComponent,
        IConfigurableObject, ICopyableObject, ILicensedComponent, IPersistStream,
        IMustBeConfiguredObject, IIdentifiableObject
    {
        /// <summary>
        /// The location of predefined template files (*.tpt)
        /// </summary>
        /// <remarks>Can contain path tags/functions</remarks>
        string TemplatesDir { get; set; }
    }

    /// <summary>
    /// An <see cref="IAttributeFindingRule"/> that uses predefined templates to create attributes
    /// </summary>
    [ComVisible(true)]
    [Guid("A91972EE-B795-4599-8634-9506B6FB4F43")]
    [CLSCompliant(false)]
    public class TemplateFinder : IdentifiableObject, ITemplateFinder
    {
        #region Constants

        /// <summary>
        /// The description of the rule.
        /// </summary>
        const string _COMPONENT_DESCRIPTION = "Template finder";

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
        /// An <see cref="AttributeFinderPathTags"/> to expand any tags in the template dir
        /// </summary>
        AttributeFinderPathTags _pathTags = new AttributeFinderPathTags();

        /// <summary>
        /// <see langword="true"/> if changes have been made to this instance since it was created;
        /// <see langword="false"/> if no changes have been made since it was created.
        /// </summary>
        bool _dirty;
        private string _templatesDir;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="TemplateFinder"/> class.
        /// </summary>
        public TemplateFinder()
        {
            try
            {
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI44786");
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TemplateFinder"/> class as a
        /// copy of <see paramref="source"/>.
        /// </summary>
        /// <param name="source">The <see cref="TemplateFinder"/> from which
        /// settings should be copied.</param>
        public TemplateFinder(TemplateFinder source)
        {
            try
            {
                CopyFrom(source);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI44787");
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// The location of predefined template files (*.tpt)
        /// </summary>
        /// <remarks>Can contain path tags/functions</remarks>
        public string TemplatesDir
        {
            get
            {
                return _templatesDir;
            }
            set
            {
                if (string.CompareOrdinal(value, _templatesDir) != 0)
                {
                    _templatesDir = value;
                    _dirty = true;
                }
            }
        }

        #endregion Properties

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
                LicenseUtilities.ValidateLicense(_LICENSE_ID, "ELI44788", _COMPONENT_DESCRIPTION);

                // So that the garbage collector knows of and properly manages the associated
                // memory.
                pDocument.Attribute.ReportMemoryUsage();

                // Initialize for use in any embedded path tags/functions.
                _pathTags.Document = pDocument;

                var templatesDir = _pathTags.Expand(TemplatesDir);

                var input = pDocument.Text;

                var returnValue = ApplyTemplate(templatesDir, input);

                // So that the garbage collector knows of and properly manages the associated
                // memory from the created return value.
                returnValue.ReportMemoryUsage();

                return returnValue;
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI44789", "Failed to apply template.");
            }
        }

        #endregion IAttributeFindingRule

        #region IConfigurableObject Members

        /// <summary>
        /// Displays a form to allow configuration of this <see cref="TemplateFinder"/>
        /// instance.
        /// </summary>
        /// <returns><see langword="true"/> if the configuration was successfully updated or
        /// <see langword="false"/> if configuration was unsuccessful.</returns>
        public bool RunConfiguration()
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(_LICENSE_ID, "ELI44790", _COMPONENT_DESCRIPTION);

                // Make a clone to update settings and only copy if ok
                TemplateFinder cloneOfThis = (TemplateFinder)Clone();

                using (TemplateFinderSettingsDialog dlg
                    = new TemplateFinderSettingsDialog(cloneOfThis))
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
                throw ex.CreateComVisible("ELI44791", "Error running configuration.");
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
                return !string.IsNullOrWhiteSpace(TemplatesDir);
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI44792",
                    "Error checking configuration of Template finder.");
            }
        }

        #endregion IMustBeConfiguredObject Members

        #region ICopyableObject Members

        /// <summary>
        /// Creates a copy of the <see cref="TemplateFinder"/> instance.
        /// </summary>
        /// <returns>A copy of the <see cref="TemplateFinder"/> instance.
        /// </returns>
        public object Clone()
        {
            try
            {
                return new TemplateFinder(this);
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI44793",
                    "Failed to clone '" + _COMPONENT_DESCRIPTION + "' object.");
            }
        }

        /// <summary>
        /// Copies the specified <see cref="TemplateFinder"/> instance into this one.
        /// </summary>
        /// <param name="pObject">The object from which to copy.</param>
        public void CopyFrom(object pObject)
        {
            try
            {
                var source = pObject as TemplateFinder;
                if (source == null)
                {
                    throw new InvalidCastException("Invalid cast to TemplateFinder");
                }
                CopyFrom(source);
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI44794",
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
                    TemplatesDir = reader.ReadString();

                    // Load the GUID for the IIdentifiableObject interface.
                    LoadGuid(stream);
                }

                // Freshly loaded object is no longer dirty
                _dirty = false;
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI44795",
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
                    writer.Write(TemplatesDir);

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
                throw ex.CreateComVisible("ELI44796",
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
        /// Copies the specified <see cref="TemplateFinder"/> instance into this one.
        /// </summary><param name="source">The <see cref="TemplateFinder"/> from which to copy.
        /// </param>
        void CopyFrom(TemplateFinder source)
        {
            TemplatesDir = source.TemplatesDir;

            _dirty = true;
        }

        /// <summary>
        /// Searches for matching templates and creates the attributes associated with the best match
        /// </summary>
        /// <param name="templateDir">The directory where the template files are located</param>
        /// <param name="imagePath">The path to the source document</param>
        /// <param name="pageInfoMap">The map of page numbers to page info of the source document</param>
        /// <returns></returns>
        private static IUnknownVector ApplyTemplate(string templateDir, SpatialString input)
        {
            IntPtr[] templates = null;
            IntPtr fileHandle = IntPtr.Zero;
            try
            {
                var voa = new IUnknownVectorClass();
                var imagePath = input.SourceDocName;
                var pageInfoMap = input.SpatialPageInfos;
                if (Directory.Exists(templateDir))
                {
                    ThrowIfFails(() => RecAPI.kRecSetLicense(null, "9d478fe171d5"), "ELI44797", "Unable to license Nuance API");
                    ThrowIfFails(() => RecAPI.kRecInit(null, null), "ELI44798", "Unable to initialize Nuance engine");
                    var templateFiles = Directory.GetFiles(templateDir, "*.tpt");
                    templates = templateFiles.Select(templatePath =>
                        {
                            IntPtr templateHandle = IntPtr.Zero;
                            ThrowIfFails(() => RecAPI.kRecLoadFormTemplate(0, out templateHandle, templatePath), "ELI44799", "Unable to load template",
                                new KeyValuePair<string, string>("Path", templatePath));
                            return templateHandle;
                        }).ToArray();
                }

                if (templates != null)
                {
                    int pageCount = 0;
                    ThrowIfFails(() => RecAPI.kRecOpenImgFile(imagePath, out fileHandle, FILEOPENMODE.IMGF_READ, IMF_FORMAT.FF_TIFNO), "ELI44800", "Unable to open image",
                        new KeyValuePair<string, string>("Image path", imagePath));
                    ThrowIfFails(() => RecAPI.kRecGetImgFilePageCount(fileHandle, out pageCount), "ELI44801", "Unable to obtain page count",
                        new KeyValuePair<string, string>("Image path", imagePath));

                    for (int pageNum = 1; pageNum <= pageCount; pageNum++)
                    {
                        if (input.GetSpecifiedPages(pageNum, pageNum).HasSpatialInfo())
                        {
                            ApplyTemplateToPage(templates, fileHandle, imagePath, pageNum, pageInfoMap, voa);
                        }
                    }
                }
                return voa;
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI44802");
            }
            finally
            {
                try
                {
                    if (fileHandle != IntPtr.Zero)
                    {
                        RecAPI.kRecCloseImgFile(fileHandle);
                    }

                    if (templates != null)
                    {
                        foreach (var template in templates)
                        {
                            RecAPI.kRecFreeFormTemplate(template);
                        }
                    }
                }
                catch { }
                try
                {

                    RecAPI.kRecQuit();
                }
                catch { }
            }
        }

        private static void ApplyTemplateToPage(IntPtr[] templates, IntPtr fileHandle, string imagePath, int pageNum, LongToObjectMap pageInfoMap, IUnknownVector voa)
        {
            IntPtr pageHandle = IntPtr.Zero;
            IntPtr formTmplCollection = IntPtr.Zero;
            try
            {
                string matchName = null;
                IntPtr bestMatchingID = IntPtr.Zero;

                ThrowIfFails(() => RecAPI.kRecLoadImg(0, fileHandle, out pageHandle, pageNum - 1), "ELI44803", "Unable to load image page",
                    new KeyValuePair<string, string>("File name", imagePath),
                    new KeyValuePair<string, string>("Page number", pageNum.AsString()));
                RecAPI.kRecFindFormTemplate(0, pageHandle, templates, out formTmplCollection, out bestMatchingID, out var confidence, out var numMatching);
                if (numMatching > 0)
                {
                    RecAPI.kRecGetMatchingInfo(bestMatchingID, out matchName);
                }
                else
                {
                    formTmplCollection = IntPtr.Zero;
                }

                if (matchName != null)
                {
                    RecAPI.kRecApplyFormTemplateEx(0, pageHandle, bestMatchingID);
                    RecAPI.kRecGetZoneCount(pageHandle, out int numZones);
                    for (int i=0; i < numZones; i++)
                    {
                        RecAPI.kRecGetZoneAttribute(pageHandle, i, "VoaIndex", out string voaIndex);
                        if (!string.IsNullOrEmpty(voaIndex))
                        {
                            RecAPI.kRecGetZoneAttribute(pageHandle, i, "Type", out string attributeType);
                            RecAPI.kRecGetZoneName(pageHandle, i, out string attributeName);
                            RecAPI.kRecGetZoneInfo(pageHandle, IMAGEINDEX.II_CURRENT, out var userZone, i);
                            var spatialString = ZoneToSpatialString(userZone, " ", imagePath, pageNum, pageInfoMap);

                            var attribute = new AttributeClass
                            {
                                Name = attributeName,
                                Type = attributeType ?? "",
                                Value = spatialString
                            };
                            voa.PushBack(attribute);

                            var templateNameSpatialString = new SpatialStringClass();
                            templateNameSpatialString.CreateNonSpatialString(matchName, imagePath);
                            var templateNameAttribute = new AttributeClass
                            {
                                Name = "TemplateName",
                                Value = templateNameSpatialString
                            };
                            attribute.SubAttributes.PushBack(templateNameAttribute);

                            RecAPI.kRecGetZoneAttribute(pageHandle, i, "FormField", out string formField);
                            if (formField != null && int.TryParse(formField, out int zoneIndex))
                            {
                                RecAPI.kRecGetZoneInfo(pageHandle, IMAGEINDEX.II_CURRENT, out var formFieldZone, zoneIndex);
                                RecAPI.kRecGetZoneName(pageHandle, zoneIndex, out string fieldName);
                                spatialString = ZoneToSpatialString(formFieldZone, fieldName, imagePath, pageNum, pageInfoMap);
                                var formFieldAttribute = new AttributeClass
                                {
                                    Name = "FormField",
                                    Value = spatialString
                                };
                                attribute.SubAttributes.PushBack(formFieldAttribute);
                            }
                        }
                    }
                }
            }
            finally
            {
                try
                {
                    if (pageHandle != IntPtr.Zero)
                    {
                        RecAPI.kRecFreeImg(pageHandle);
                    }

                    // Freeing this collection causes exceptions sometimes
                    //if (formTmplCollection != IntPtr.Zero)
                    //{
                    //    RecAPI.kRecFreeFormTemplateCollection(formTmplCollection);
                    //}
                }
                catch { }
            }
        }

        private static SpatialString ZoneToSpatialString(ZONE userZone, string value, string imagePath, int pageNum, LongToObjectMap pageInfoMap)
        {
            var sourceRect = userZone.rectBBox;
            var rect = new LongRectangleClass();
            rect.SetBounds(sourceRect.left, sourceRect.top, sourceRect.right, sourceRect.bottom);
            var zone = new RasterZoneClass();
            zone.CreateFromLongRectangle(rect, pageNum);
            var spatialString = new SpatialStringClass();
            spatialString.CreatePseudoSpatialString(zone, value, imagePath, pageInfoMap);
            return spatialString;
        }

        private static void ThrowIfFails(Func<RECERR> recApiMethod, string eli, string message, params KeyValuePair<string, string>[] debugData)
        {
            RECERR rc = recApiMethod();
            if (rc != RECERR.REC_OK && rc != RECERR.API_INIT_WARN)
            {
                var uex = new ExtractException(eli, message);
                foreach (var kv in debugData)
                    uex.AddDebugData(kv.Key, kv.Value, false);
                throw uex;
            }
        }

        #endregion Private Members
    }
}
