using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Extract.Interop;
using Extract.Licensing;
using Extract.Utilities;
using UCLID_AFCORELib;
using UCLID_COMLMLib;
using UCLID_COMUTILSLib;

using ComAttribute = UCLID_AFCORELib.Attribute;

namespace Extract.AttributeFinder.Rules
{
    /// <summary>
    /// An interface for the <see cref="SetDocumentTags"/> class.
    /// </summary>
    [ComVisible(true)]
    [Guid("37D01C84-BDA7-4260-885D-B0EFD14E0D0E")]
    [CLSCompliant(false)]
    public interface ISetDocumentTags : IOutputHandler, IDocumentPreprocessor, ICategorizedComponent,
        IConfigurableObject, ICopyableObject, ILicensedComponent, IPersistStream,
        IMustBeConfiguredObject, IIdentifiableObject
    {

        /// <summary>
        /// Gets or sets whether to set a string tag.
        /// </summary>
        bool SetStringTag
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the name of the string tag to be set.
        /// </summary>
        string StringTagName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the delimiter to be used when combining multiple values into one string tag.
        /// </summary>
        string Delimiter
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets whether to use the specified value for the string tag.
        /// </summary>
        bool UseSpecifiedValueForStringTag
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the specified value to be used for the string tag.
        /// </summary>
        string SpecifiedValueForStringTag
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets whether to use a document tag for the value of the string tag.
        /// </summary>
        bool UseTagValueForStringTag
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the tag name to use for the string tag's value.
        /// </summary>
        string TagNameForStringTagValue
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets whether to use an attribute selector for the value of the string tag.
        /// </summary>
        bool UseSelectedAttributesForStringTagValue
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the <see cref="IAttributeSelector"/> used to specify values for string tag.
        /// </summary>
        IAttributeSelector StringTagAttributeSelector
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets whether to set an object tag.
        /// </summary>
        bool SetObjectTag
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the name of the object tag to be set.
        /// </summary>
        string ObjectTagName
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets whether to use the specified value for the object tag.
        /// </summary>
        bool UseSpecifiedValueForObjectTag
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the specified value to be used for the object tag.
        /// </summary>
        string SpecifiedValueForObjectTag
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets whether to use an attribute selector for the value of the object tag.
        /// </summary>
        bool UseSelectedAttributesForObjectTagValue
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the <see cref="IAttributeSelector"/> used to specify values for object tag.
        /// </summary>
        IAttributeSelector ObjectTagAttributeSelector
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets whether to create any tags if one has an empty value
        /// </summary>
        bool NoTagsIfEmpty
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets whether to generate source attributes with a RSD file
        /// </summary>
        bool GenerateSourceAttributesWithRSDFile
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the name of the RSD file used to generate source attributes 
        /// </summary>
        string SourceAttributeRSDFile
        {
            get;
            set;
        }
    }

    /// <summary>
    /// An <see cref="IOutputHandler"/> and <see cref="IDocumentPreprocessor"/> that sets string
    /// or object tags of the <see cref="AFDocument"/> object.
    /// </summary>
    [ComVisible(true)]
    [Guid("FECF9960-718E-483C-84DE-F6213A3ECEB4")]
    [CLSCompliant(false)]
    public class SetDocumentTags : IdentifiableObject, ISetDocumentTags
    {
        #region Constants

        /// <summary>
        /// The description of the rule.
        /// </summary>
        const string _COMPONENT_DESCRIPTION = "Set document tags";

        /// <summary>
        /// Current version.
        /// </summary>
        const int _CURRENT_VERSION = 2;

        /// <summary>
        /// The license id to validate in licensing calls
        /// </summary>
        const LicenseIdName _LICENSE_ID = LicenseIdName.FlexIndexIDShieldCoreObjects;

        #endregion Constants

        #region Fields

        /// <summary>
        /// Whether to set a string tag
        /// </summary>
        bool _setStringTag = true;

        /// <summary>
        /// The name of the string tag to set.
        /// </summary>
        string _stringTagName;

        /// <summary>
        /// Whether to use the specified value for the string tag.
        /// </summary>
        bool _useSpecifiedValueForStringTag = true;

        /// <summary>
        /// The specified value to be used for the string tag.
        /// </summary>
        string _specifiedValueForStringTag;

        /// <summary>
        /// Whether to use a document tag for the value of the string tag.
        /// </summary>
        bool _useTagValueForStringTag;

        /// <summary>
        /// The tag name to use for the string tag's value.
        /// </summary>
        string _tagNameForStringTagValue;

        /// <summary>
        /// Whether the tag will be set based on selected attributes.
        /// </summary>
        bool _useSelectedAttributesForStringTagValue;

        /// <summary>
        /// The <see cref="IAttributeSelector"/> used to specify attribute(s) to be used for tag values.
        /// </summary>
        IAttributeSelector _stringTagAttributeSelector;

        /// <summary>
        /// Delimiter to be used to combine multiple values into one
        /// </summary>
        string _delimiter = ";";

        /// <summary>
        /// Whether to set an object tag
        /// </summary>
        bool _setObjectTag;

        /// <summary>
        /// The name of the object tag to set.
        /// </summary>
        string _objectTagName;

        /// <summary>
        /// Whether to use the specified value for the object tag.
        /// </summary>
        bool _useSpecifiedValueForObjectTag = true;

        /// <summary>
        /// The specified value to be used for the object tag.
        /// </summary>
        string _specifiedValueForObjectTag;

        /// <summary>
        /// Whether the object tag will be set based on selected attributes.
        /// </summary>
        bool _useSelectedAttributesForObjectTagValue;

        /// <summary>
        /// The <see cref="IAttributeSelector"/> used to specify attribute(s) to be used for tag values.
        /// </summary>
        IAttributeSelector _objectTagAttributeSelector;

        /// <summary>
        /// An <see cref="AttributeFinderPathTags"/> to expand tags from specified values.
        /// </summary>
        AttributeFinderPathTags _pathTags = new AttributeFinderPathTags();

        /// <summary>
        /// Whether to create any tags if one has an empty value
        /// </summary>
        bool _noTagsIfEmpty;

        /// <summary>
        /// Whether to generate source attributes with a RSD file
        /// </summary>
        bool _generateSourceAttributesWithRSDFile;

        /// <summary>
        /// The name of the RSD file used to generate source attributes, can use tags
        /// </summary>
        string _sourceAttributeRSDFile;

        /// <summary>
        /// <see langword="true"/> if changes have been made to this instance since it was created;
        /// <see langword="false"/> if no changes have been made since it was created.
        /// </summary>
        bool _dirty;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="SetDocumentTags"/> class.
        /// </summary>
        public SetDocumentTags()
        {
            try
            {
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI38566");
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SetDocumentTags"/> class as a
        /// copy of <see paramref="source"/>.
        /// </summary>
        /// <param name="source">The <see cref="SetDocumentTags"/> from which
        /// settings should be copied.</param>
        public SetDocumentTags(SetDocumentTags source)
        {
            try
            {
                CopyFrom(source);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI38567");
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets whether to set a string tag.
        /// </summary>
        public bool SetStringTag
        {
            get
            {
                return _setStringTag;
            }

            set
            {
                try
                {
                    if (_setStringTag != value)
                    {
                        _setStringTag = value;

                        _dirty = true;
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI38568");
                }
            }
        }

        /// <summary>
        /// Gets or sets the name of the string tag to be set.
        /// </summary>
        public string StringTagName
        {
            get
            {
                return _stringTagName;
            }

            set
            {
                try
                {
                    if (_stringTagName != value)
                    {
                        _stringTagName = value;

                        _dirty = true;
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI38569");
                }
            }
        }

        /// <summary>
        /// Gets or sets the delimiter to be used when combining multiple values into one string tag.
        /// </summary>
        public string Delimiter
        {
            get
            {
                return _delimiter;
            }

            set
            {
                try
                {
                    if (_delimiter != value)
                    {
                        _delimiter = value;

                        _dirty = true;
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI38570");
                }
            }
        }

        /// <summary>
        /// Gets or sets whether to use the specified value for the string tag.
        /// </summary>
        public bool UseSpecifiedValueForStringTag
        {
            get
            {
                return _useSpecifiedValueForStringTag;
            }

            set
            {
                try
                {
                    if (_useSpecifiedValueForStringTag != value)
                    {
                        _useSpecifiedValueForStringTag = value;

                        _dirty = true;
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI38571");
                }
            }
        }
        /// <summary>
        /// Gets or sets the specified value to be used for the string tag.
        /// </summary>
        public string SpecifiedValueForStringTag
        {
            get
            {
                return _specifiedValueForStringTag;
            }

            set
            {
                try
                {
                    if (_specifiedValueForStringTag != value)
                    {
                        _specifiedValueForStringTag = value;

                        _dirty = true;
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI38572");
                }
            }
        }

        /// <summary>
        /// Gets or sets whether to use a document tag for the value of the string tag.
        /// </summary>
        public bool UseTagValueForStringTag
        {
            get
            {
                return _useTagValueForStringTag;
            }

            set
            {
                try
                {
                    if (_useTagValueForStringTag != value)
                    {
                        _useTagValueForStringTag = value;

                        _dirty = true;
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI38573");
                }
            }
        }

        /// <summary>
        /// Gets or sets the tag name to use for the string tag's value.
        /// </summary>
        public string TagNameForStringTagValue
        {
            get
            {
                return _tagNameForStringTagValue;
            }

            set
            {
                try
                {
                    if (_tagNameForStringTagValue != value)
                    {
                        _tagNameForStringTagValue = value;

                        _dirty = true;
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI38574");
                }
            }
        }

        /// <summary>
        /// Gets or sets whether to use an attribute selector for the value of the string tag.
        /// </summary>
        public bool UseSelectedAttributesForStringTagValue
        {
            get
            {
                return _useSelectedAttributesForStringTagValue;
            }

            set
            {
                try
                {
                    if (_useSelectedAttributesForStringTagValue != value)
                    {
                        _useSelectedAttributesForStringTagValue = value;

                        _dirty = true;
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI38575");
                }
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="IAttributeSelector"/> used to specify values for string tag.
        /// </summary>
        public IAttributeSelector StringTagAttributeSelector
        {
            get
            {
                return _stringTagAttributeSelector;
            }

            set
            {
                try
                {
                    if (_stringTagAttributeSelector != value)
                    {
                        _stringTagAttributeSelector = value;

                        _dirty = true;
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI38576");
                }
            }
        }

        /// <summary>
        /// Gets or sets whether to set an object tag.
        /// </summary>
        public bool SetObjectTag
        {
            get
            {
                return _setObjectTag;
            }

            set
            {
                try
                {
                    if (_setObjectTag != value)
                    {
                        _setObjectTag = value;

                        _dirty = true;
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI38577");
                }
            }
        }

        /// <summary>
        /// Gets or sets the name of the object tag to be set.
        /// </summary>
        public string ObjectTagName
        {
            get
            {
                return _objectTagName;
            }

            set
            {
                try
                {
                    if (_objectTagName != value)
                    {
                        _objectTagName = value;

                        _dirty = true;
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI38578");
                }
            }
        }

        /// <summary>
        /// Gets or sets whether to use the specified value for the object tag.
        /// </summary>
        public bool UseSpecifiedValueForObjectTag
        {
            get
            {
                return _useSpecifiedValueForObjectTag;
            }

            set
            {
                try
                {
                    if (_useSpecifiedValueForObjectTag != value)
                    {
                        _useSpecifiedValueForObjectTag = value;

                        _dirty = true;
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI38579");
                }
            }
        }
        /// <summary>
        /// Gets or sets the specified value to be used for the object tag.
        /// </summary>
        public string SpecifiedValueForObjectTag
        {
            get
            {
                return _specifiedValueForObjectTag;
            }

            set
            {
                try
                {
                    if (_specifiedValueForObjectTag != value)
                    {
                        _specifiedValueForObjectTag = value;

                        _dirty = true;
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI38580");
                }
            }
        }

        /// <summary>
        /// Gets or sets whether to use an attribute selector for the value of the object tag.
        /// </summary>
        public bool UseSelectedAttributesForObjectTagValue
        {
            get
            {
                return _useSelectedAttributesForObjectTagValue;
            }

            set
            {
                try
                {
                    if (_useSelectedAttributesForObjectTagValue != value)
                    {
                        _useSelectedAttributesForObjectTagValue = value;

                        _dirty = true;
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI38581");
                }
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="IAttributeSelector"/> used to specify values for object tag.
        /// </summary>
        public IAttributeSelector ObjectTagAttributeSelector
        {
            get
            {
                return _objectTagAttributeSelector;
            }

            set
            {
                try
                {
                    if (_objectTagAttributeSelector != value)
                    {
                        _objectTagAttributeSelector = value;

                        _dirty = true;
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI38582");
                }
            }
        }

        /// <summary>
        /// Gets or sets whether to create any tags if one has an empty value
        /// </summary>
        public bool NoTagsIfEmpty
        {
            get
            {
                return _noTagsIfEmpty;
            }

            set
            {
                try
                {
                    if (_noTagsIfEmpty != value)
                    {
                        _noTagsIfEmpty = value;
                        
                        _dirty = true;
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI39928");
                }
            }
        }

        /// <summary>
        /// Gets or sets whether to generate source attributes with a RSD file
        /// </summary>
        public bool GenerateSourceAttributesWithRSDFile
        {
            get
            {
                return _generateSourceAttributesWithRSDFile;
            }

            set
            {
                try
                {
                    if (_generateSourceAttributesWithRSDFile != value)
                    {
                        _generateSourceAttributesWithRSDFile = value;
                        
                        _dirty = true;
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI39943");
                }
            }
        }

        /// <summary>
        /// Gets or sets the name of the RSD file used to generate source attributes
        /// </summary>
        public string SourceAttributeRSDFile
        {
            get
            {
                return _sourceAttributeRSDFile;
            }

            set
            {
                try
                {
                    if (_sourceAttributeRSDFile != value)
                    {
                        _sourceAttributeRSDFile = value;

                        _dirty = true;
                    }
                }
                catch (Exception ex)
                {
                    throw ex.AsExtract("ELI39948");
                }
            }
        }

        #endregion Properties

        #region IDocumentProcessor

        /// <summary>
        /// Sets tags on the specified <see paramref="pDocument"/>
        /// </summary>
        /// <param name="pDocument">The <see cref="AFDocument"/> to process.</param>
        /// <param name="pProgressStatus">The <see cref="ProgressStatus"/> displaying the progress.
        /// </param>
        public void Process(AFDocument pDocument, ProgressStatus pProgressStatus)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(_LICENSE_ID, "ELI38583", _COMPONENT_DESCRIPTION);

                ExtractException.Assert("ELI38584", "Rule is not properly configured.",
                    IsConfigured());

                var sourceAttributes = (GenerateSourceAttributesWithRSDFile) ? GetSourceAttributes(pDocument) 
                    : pDocument.Attribute.SubAttributes;

                setDocumentTags(pDocument, sourceAttributes);

                // Report memory usage of hierarchy after processing to ensure all COM objects
                // referenced in final result are reported.
                pDocument.Attribute.ReportMemoryUsage();
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI38585", "Failed to set document tags.");
            }
        }

        #endregion IDocumentProcessor

        #region IOutputHandler Members

        /// <summary>
        /// Sets document tags, using the output (<see paramref="pAttributes"/>) if specified.
        /// </summary>
        /// <param name="pAttributes">The output to use for tag values if so configured.</param>
        /// <param name="pDoc">The <see cref="AFDocument"/> the document on which to set tags.</param>
        /// <param name="pProgressStatus">A <see cref="ProgressStatus"/> that can be used to update
        /// processing status.</param>
        public void ProcessOutput(IUnknownVector pAttributes, AFDocument pDoc, ProgressStatus pProgressStatus)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(_LICENSE_ID, "ELI38586", _COMPONENT_DESCRIPTION);

                ExtractException.Assert("ELI38587", "Rule is not properly configured.",
                    IsConfigured());

                var sourceAttributes = (GenerateSourceAttributesWithRSDFile) ? GetSourceAttributes(pDoc)
                    : pAttributes;

                setDocumentTags(pDoc, sourceAttributes);

                // So that the garbage collector knows of and properly manages the associated
                // memory.
                pAttributes.ReportMemoryUsage();
                pDoc.Attribute.ReportMemoryUsage();

            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI38588", "Failed to shrink attributes.");
            }
        }

        #endregion IOutputHandler Members

        #region IConfigurableObject Members

        /// <summary>
        /// Displays a form to allow configuration of this <see cref="SetDocumentTags"/>
        /// instance.
        /// </summary>
        /// <returns><see langword="true"/> if the configuration was successfully updated or
        /// <see langword="false"/> if configuration was unsuccessful.</returns>
        public bool RunConfiguration()
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(_LICENSE_ID, "ELI38589", _COMPONENT_DESCRIPTION);

                // Make a clone to update settings and only copy if ok
                SetDocumentTags cloneOfThis = (SetDocumentTags)Clone();

                using (SetDocumentTagsSettingsDialog dlg
                    = new SetDocumentTagsSettingsDialog(cloneOfThis))
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
                throw ex.CreateComVisible("ELI38590", "Error running configuration.");
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
                if (!SetStringTag && !SetObjectTag)
                {
                    return false;
                }

                if (SetStringTag)
                {
                    // Exactly one value source should be specified
                    if (  (UseSpecifiedValueForStringTag ? 1:0)
                        + (UseTagValueForStringTag ? 1:0)
                        + (UseSelectedAttributesForStringTagValue ? 1:0) != 1)
                    {
                        return false;
                    }

                        
                    // Check for missing values
                    if (// Empty tag name
                           string.IsNullOrWhiteSpace(StringTagName)
                        // Null specified value
                        || UseSpecifiedValueForStringTag
                           && SpecifiedValueForStringTag == null
                        // Empty tag to use for value name
                        || UseTagValueForStringTag
                           && string.IsNullOrWhiteSpace(TagNameForStringTagValue)
                        // Missing or not configured attribute selector
                        || UseSelectedAttributesForStringTagValue
                              && (StringTagAttributeSelector == null
                                  || StringTagAttributeSelector is IMustBeConfiguredObject
                                     && !((IMustBeConfiguredObject)StringTagAttributeSelector).IsConfigured()))
                    {
                        return false;
                    }
                }

                if (SetObjectTag)
                {
                    // Exactly one value source should be specified
                    if (!(UseSpecifiedValueForObjectTag ^ UseSelectedAttributesForObjectTagValue))
                    {
                        return false;
                    }

                    // Check for missing values
                    if (// Empty tag name
                           string.IsNullOrWhiteSpace(ObjectTagName)
                        // Null specified value
                        || UseSpecifiedValueForObjectTag
                           && SpecifiedValueForObjectTag == null
                        // Missing or not configured attribute selector
                        || UseSelectedAttributesForObjectTagValue
                              && (ObjectTagAttributeSelector == null
                                  || ObjectTagAttributeSelector is IMustBeConfiguredObject
                                     && !((IMustBeConfiguredObject)ObjectTagAttributeSelector).IsConfigured()))
                    {
                        return false;
                    }
                }

                if (GenerateSourceAttributesWithRSDFile)
                {
                    return !string.IsNullOrWhiteSpace(SourceAttributeRSDFile);
                }

                return true;
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI38591",
                    "Error checking configuration of Rule object.");
            }
        }

        #endregion IMustBeConfiguredObject Members

        #region ICopyableObject Members

        /// <summary>
        /// Creates a copy of the <see cref="SetDocumentTags"/> instance.
        /// </summary>
        /// <returns>A copy of the <see cref="SetDocumentTags"/> instance.
        /// </returns>
        public object Clone()
        {
            try
            {
                return new SetDocumentTags(this);
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI38592",
                    "Failed to clone '" + _COMPONENT_DESCRIPTION + "' object.");
            }
        }

        /// <summary>
        /// Copies the specified <see cref="SetDocumentTags"/> instance into this one.
        /// </summary>
        /// <param name="pObject">The object from which to copy.</param>
        public void CopyFrom(object pObject)
        {
            try
            {
                var source = pObject as SetDocumentTags;
                if (source == null)
                {
                    throw new InvalidCastException("Invalid cast to SetDocumentTags");
                }
                CopyFrom(source);
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI38593",
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
                    SetStringTag = reader.ReadBoolean();
                    if (SetStringTag)
                    {
                        StringTagName = reader.ReadString();

                        Delimiter = reader.ReadString();

                        UseSpecifiedValueForStringTag = reader.ReadBoolean();
                        if (UseSpecifiedValueForStringTag)
                        {
                            SpecifiedValueForStringTag = reader.ReadString();
                        }

                        UseTagValueForStringTag = reader.ReadBoolean();
                        if (UseTagValueForStringTag)
                        {
                            TagNameForStringTagValue = reader.ReadString();
                        }

                        UseSelectedAttributesForStringTagValue = reader.ReadBoolean();
                        if (UseSelectedAttributesForStringTagValue)
                        {
                            StringTagAttributeSelector = reader.ReadIPersistStream() as IAttributeSelector;
                        }
                    }

                    SetObjectTag = reader.ReadBoolean();
                    if (SetObjectTag)
                    {
                        ObjectTagName = reader.ReadString();

                        UseSpecifiedValueForObjectTag = reader.ReadBoolean();
                        if (UseSpecifiedValueForObjectTag)
                        {
                            SpecifiedValueForObjectTag = reader.ReadString();
                        }

                        UseSelectedAttributesForObjectTagValue = reader.ReadBoolean();
                        if (UseSelectedAttributesForObjectTagValue)
                        {
                            ObjectTagAttributeSelector = reader.ReadIPersistStream() as IAttributeSelector;
                        }
                    }

                    NoTagsIfEmpty = false;
                    GenerateSourceAttributesWithRSDFile = false;
                    SourceAttributeRSDFile = "";
                    if (reader.Version > 1)
                    {
                        NoTagsIfEmpty = reader.ReadBoolean();
                        GenerateSourceAttributesWithRSDFile = reader.ReadBoolean();
                        if (GenerateSourceAttributesWithRSDFile)
                        {
                            SourceAttributeRSDFile = reader.ReadString();
                        }
                    }

                    // Load the GUID for the IIdentifiableObject interface.
                    LoadGuid(stream);
                }

                // Freshly loaded object is no longer dirty
                _dirty = false;
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI38594",
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

                    writer.Write(SetStringTag);
                    if (SetStringTag)
                    {
                        writer.Write(StringTagName);

                        writer.Write(Delimiter);

                        writer.Write(UseSpecifiedValueForStringTag);
                        if (UseSpecifiedValueForStringTag)
                        {
                            writer.Write(SpecifiedValueForStringTag);
                        }

                        writer.Write(UseTagValueForStringTag);
                        if (UseTagValueForStringTag)
                        {
                            writer.Write(TagNameForStringTagValue);
                        }

                        writer.Write(UseSelectedAttributesForStringTagValue);
                        if (UseSelectedAttributesForStringTagValue)
                        {
                            writer.Write((IPersistStream)StringTagAttributeSelector, clearDirty);
                        }
                    }

                    writer.Write(SetObjectTag);
                    if (SetObjectTag)
                    {
                        writer.Write(ObjectTagName);

                        writer.Write(UseSpecifiedValueForObjectTag);
                        if (UseSpecifiedValueForObjectTag)
                        {
                            writer.Write(SpecifiedValueForObjectTag);
                        }

                        writer.Write(UseSelectedAttributesForObjectTagValue);
                        if (UseSelectedAttributesForObjectTagValue)
                        {
                            writer.Write((IPersistStream)ObjectTagAttributeSelector, clearDirty);
                        }
                    }

                    writer.Write(NoTagsIfEmpty);

                    writer.Write(GenerateSourceAttributesWithRSDFile);

                    if (GenerateSourceAttributesWithRSDFile)
                    {
                        writer.Write(SourceAttributeRSDFile);
                    }

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
                throw ex.CreateComVisible("ELI38595",
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
            ComMethods.RegisterTypeInCategory(type, ExtractCategories.DocumentPreprocessorsGuid);
            ComMethods.RegisterTypeInCategory(type, ExtractCategories.OutputHandlersGuid);
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
            ComMethods.UnregisterTypeInCategory(type, ExtractCategories.DocumentPreprocessorsGuid);
            ComMethods.UnregisterTypeInCategory(type, ExtractCategories.OutputHandlersGuid);
        }

        /// <summary>
        /// Copies the specified <see cref="SetDocumentTags"/> instance into this one.
        /// </summary><param name="source">The <see cref="SetDocumentTags"/> from which to copy.
        /// </param>
        void CopyFrom(SetDocumentTags source)
        {
            SetStringTag = source.SetStringTag;
            StringTagName = source.StringTagName;
            Delimiter = source.Delimiter;
            UseSpecifiedValueForStringTag = source.UseSpecifiedValueForStringTag;
            SpecifiedValueForStringTag = source.SpecifiedValueForStringTag;
            UseTagValueForStringTag = source.UseTagValueForStringTag;
            TagNameForStringTagValue = source.TagNameForStringTagValue;
            UseSelectedAttributesForStringTagValue = source.UseSelectedAttributesForStringTagValue;
            if (source.StringTagAttributeSelector == null)
            {
                StringTagAttributeSelector = null;
            }
            else
            {
                ICopyableObject copyThis = (ICopyableObject)source.StringTagAttributeSelector;
                StringTagAttributeSelector = (IAttributeSelector)copyThis.Clone();
            }

            SetObjectTag = source.SetObjectTag;
            ObjectTagName = source.ObjectTagName;
            UseSpecifiedValueForObjectTag = source.UseSpecifiedValueForObjectTag;
            SpecifiedValueForObjectTag = source.SpecifiedValueForObjectTag;
            UseSelectedAttributesForObjectTagValue = source.UseSelectedAttributesForObjectTagValue;
            if (source.ObjectTagAttributeSelector == null)
            {
                ObjectTagAttributeSelector = null;
            }
            else
            {
                ICopyableObject copyThis = (ICopyableObject)source.ObjectTagAttributeSelector;
                ObjectTagAttributeSelector = (IAttributeSelector)copyThis.Clone();
            }

            NoTagsIfEmpty = source.NoTagsIfEmpty;
            GenerateSourceAttributesWithRSDFile = source.GenerateSourceAttributesWithRSDFile;
            SourceAttributeRSDFile = source.SourceAttributeRSDFile;

            _dirty = true;
        }

        /// <summary>
        /// Sets tags on the specified <see paramref="pDocument"/>
        /// </summary>
        /// <param name="document">The <see cref="AFDocument"/> to process.</param>
        /// <param name="sourceAttributes">Attributes to use for tag values.</param>
        void setDocumentTags(AFDocument document, IUnknownVector sourceAttributes)
        {
            string valueForStringTag = null;
            if (SetStringTag)
            {
                if (UseSpecifiedValueForStringTag)
                {
                    // Expand any tags in the specified value
                    _pathTags.Document = document;
                    valueForStringTag = _pathTags.Expand(SpecifiedValueForStringTag);
                }
                else if (UseTagValueForStringTag)
                {
                    // Get the string tag value if it exists
                    if (document.StringTags.Contains(TagNameForStringTagValue))
                    {
                        valueForStringTag = document.StringTags.GetValue(TagNameForStringTagValue);
                    }
                    // Get the object tag value if it exists, concatenate if there are multiple
                    // elements in the vector.
                    else if (document.ObjectTags.Contains(TagNameForStringTagValue))
                    {
                        valueForStringTag = string.Join(Delimiter,
                            ((IVariantVector)document.ObjectTags.GetValue(TagNameForStringTagValue))
                            .ToIEnumerable<string>());
                    }
                }
                else if (UseSelectedAttributesForStringTagValue)
                {
                    using (RuleObjectProfiler profiler =
                        new RuleObjectProfiler("", "", StringTagAttributeSelector, 0))
                    {

                        // Get all values, concatenating if there are multiples
                        var values = StringTagAttributeSelector
                            .SelectAttributes(sourceAttributes, document, sourceAttributes)
                            .ToIEnumerable<ComAttribute>()
                            .Select(attr => attr.Value.String);
                        valueForStringTag = string.Join(Delimiter, values);
                    }
                }
            }
            
            VariantVector objectTagValues = null;
            if (SetObjectTag)
            {
                if (UseSpecifiedValueForObjectTag)
                {
                    objectTagValues = new VariantVector();

                    // Expand any tags in the specified value
                    _pathTags.Document = document;
                    string tagValue = _pathTags.Expand(SpecifiedValueForObjectTag);
                    if (!NoTagsIfEmpty || !string.IsNullOrWhiteSpace(tagValue))
                    {
                        objectTagValues.PushBack(_pathTags.Expand(SpecifiedValueForObjectTag));
                    }
                }
                else if (UseSelectedAttributesForObjectTagValue)
                {
                    using (RuleObjectProfiler profiler =
                        new RuleObjectProfiler("", "", ObjectTagAttributeSelector, 0))
                    {
                        // Get all values as a VariantVector
                        objectTagValues = ObjectTagAttributeSelector
                            .SelectAttributes(sourceAttributes, document, sourceAttributes)
                            .ToIEnumerable<ComAttribute>()
                            .Select(attr => attr.Value.String)
                            .ToVariantVector();
                    }
                }
            }

            bool createTags = !NoTagsIfEmpty;

            if (NoTagsIfEmpty)
            {
                createTags = true;
                if (SetStringTag)
                {
                    createTags = !string.IsNullOrWhiteSpace(valueForStringTag);
                }

                if (SetObjectTag)
                {
                    createTags = createTags && (objectTagValues.Size > 0);
                } 
            }

            if (createTags)
            {
                if (SetStringTag)
                {
                    // Set the string tag
                    document.StringTags.Set(StringTagName, valueForStringTag);
                }

                if (SetObjectTag)
                {
                    // Set the object tag
                    document.ObjectTags.Set(ObjectTagName, objectTagValues);
                }
            }
        }

        /// <summary>
        /// Loads the rule set configured to be run.
        /// </summary>
        /// <param name="afDoc">A <see cref="AFDocument"/> to provide context for expanding
        /// path tags.</param>
        /// <returns>The <see cref="RuleSet"/>.</returns>
        RuleSet LoadRuleset(AFDocument afDoc)
        {
            _pathTags.Document = afDoc;
            string rsdFileName = _pathTags.Expand(_sourceAttributeRSDFile);

            RuleSet ruleSet = new RuleSet();
            ruleSet.LoadFrom(rsdFileName, false);

            return ruleSet;
        }

        /// <summary>
        /// Runs rules on the <see paramref="pDocument"/> if <see cref="GenerateSourceAttributesWithRSDFile"/>
        /// is true
        /// </summary>
        /// <param name="pDocument">The <see cref="AFDocument"/> to run the rules on</param>
        /// <returns>null if <see cref="GenerateSourceAttributesWithRSDFile"/> is false or the 
        /// results of running the rules otherwise</returns>
        IUnknownVector GetSourceAttributes(AFDocument pDocument)
        {
            IUnknownVector sourceAttributes = null;

            if (GenerateSourceAttributesWithRSDFile)
            {
                // Clone the AFDocument so changes made by the rule set are not propagated

                AFDocument afDoc = pDocument.PartialClone(false, true);

                RuleSet rules = LoadRuleset(afDoc);

                // Use ComObjectReleaser on the RuleExecutionSession so that the current RSD file name is
                // handled correctly.
                using (ComObjectReleaser ComObjectReleaser = new ComObjectReleaser())
                {
                    RuleExecutionSession session = new RuleExecutionSession();
                    ComObjectReleaser.ManageObjects(session);
                    session.SetRSDFileName(rules.FileName);

                    sourceAttributes = rules.ExecuteRulesOnText(afDoc, null, "", null);

                    // So that the garbage collector knows of and properly manages the associated
                    // memory.
                    sourceAttributes.ReportMemoryUsage();
                }
            }

            return sourceAttributes;
        }

        #endregion Private Members
    }
}
