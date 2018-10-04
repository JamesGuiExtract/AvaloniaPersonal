using Extract.Interop;
using Extract.Licensing;
using Extract.Utilities;
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
        /// The (encrypted) template library file
        /// </summary>
        /// <remarks>Can contain path tags/functions</remarks>
        string TemplateLibrary { get; set; }

        /// <summary>
        /// Additional CLI options to pass to the RedactionPredictor
        /// </summary>
        string RedactionPredictorOptions { get; set; }
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
        const int _CURRENT_VERSION = 2;

        /// <summary>
        /// The license id to validate in licensing calls
        /// </summary>
        const LicenseIdName _LICENSE_ID = LicenseIdName.FlexIndexIDShieldCoreObjects;

        /// <summary>
        /// The path to the NERAnnotator application
        /// </summary>
        static readonly string _REDACTION_PREDICTOR_APPLICATION =
            Path.Combine(FileSystemMethods.CommonComponentsPath, "RedactionPredictor.exe");

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

        string _templateLibrary;

        string _redactionPredictorOptions;

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
        public string TemplateLibrary
        {
            get
            {
                return _templateLibrary;
            }
            set
            {
                if (string.CompareOrdinal(value, _templateLibrary) != 0)
                {
                    _templateLibrary = value;
                    _dirty = true;
                }
            }
        }

        /// <summary>
        /// Additional CLI options to pass to the RedactionPredictor
        /// </summary>
        public string RedactionPredictorOptions
        {
            get
            {
                return _redactionPredictorOptions;
            }
            set
            {
                if (string.CompareOrdinal(value, _redactionPredictorOptions) != 0)
                {
                    _redactionPredictorOptions = value;
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

                var templateLibrary = _pathTags.Expand(TemplateLibrary);

                var input = pDocument.Text;

                var returnValue = ApplyTemplate(templateLibrary, input, RedactionPredictorOptions);

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
                return !string.IsNullOrWhiteSpace(TemplateLibrary);
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
        /// <summary>
        /// Additional CLI options to pass to the RedactionPredictor
        /// </summary>
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
                    TemplateLibrary = reader.ReadString();

                    if (reader.Version >= 2)
                    {
                        RedactionPredictorOptions = reader.ReadString();
                    }

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
                    writer.Write(TemplateLibrary);
                    writer.Write(RedactionPredictorOptions);

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
            TemplateLibrary = source.TemplateLibrary;
            RedactionPredictorOptions = source.RedactionPredictorOptions;

            _dirty = true;
        }

        /// <summary>
        /// Searches for matching templates and creates the attributes associated with the best match
        /// </summary>
        /// <param name="templateLibrary">The (encrypted) template library file</param>
        /// <param name="input">The spatial string used to determine which pages to apply the templates to</param>
        /// <param name="options">Optional options to pass along to the the redaction predictor</param>
        private static IUnknownVector ApplyTemplate(string templateLibrary, SpatialString input, string options)
        {
            var pageVector = input.GetPages(false, "");
            pageVector.ReportMemoryUsage();
            var pages = pageVector
                .ToIEnumerable<SpatialString>()
                .Select(s => s.GetFirstPageNumber())
                .ToRangeString();

            using (var outputFile = new TemporaryFile(extension: ".voa", sensitive: true))
            {
                var args = new List<string>
                {
                    "--pages",
                    pages,
                    "--apply-template",
                    templateLibrary,
                    input.SourceDocName,
                    outputFile.FileName
                };

                string argumentString = string.Join(" ", args
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Select(s => (s.Contains(' ') && s[0] != '"') ? s.Quote() : s));

                if (!string.IsNullOrWhiteSpace(options))
                {
                    argumentString += (" " + options);
                }

                SystemMethods.RunExtractExecutable(_REDACTION_PREDICTOR_APPLICATION, argumentString);

                var attributes = new IUnknownVectorClass();
                attributes.LoadFrom(outputFile.FileName, false);
                return attributes;
            }
        }

        #endregion Private Members
    }
}
