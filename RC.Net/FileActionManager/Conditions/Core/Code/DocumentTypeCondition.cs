using Extract.Interop;
using Extract.Licensing;
using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using UCLID_AFCORELib;
using UCLID_AFUTILSLib;
using UCLID_COMLMLib;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;

namespace Extract.FileActionManager.Conditions
{
    /// <summary>
    /// A <see cref="IFAMCondition"/> based on the document type from the VOA file.
    /// </summary>
    [ComVisible(true)]
    [Guid("926A1AE4-0258-4504-BDF6-4A5C2648919E")]
    [ProgId("Extract.FileActionManager.Conditions.DocumentTypeCondition")]
    public class DocumentTypeCondition : ICategorizedComponent, IConfigurableObject,
        IMustBeConfiguredObject, ICopyableObject, IFAMCondition, ILicensedComponent,
        IPersistStream
    {
        #region Constants

        /// <summary>
        /// The object name.
        /// </summary>
        const string _COMPONENT_DESCRIPTION = "Document type condition";

        /// <summary>
        /// Current task version.
        /// </summary>
        internal const int _CURRENT_VERSION = 1;

        #endregion Constants

        #region Fields

        /// <summary>
        /// If <see langword="true"/> the condition will be met if the specified condition is
        /// <see langword="true"/>; if <see langword="false"/> the condition will be met if the
        /// specified condition is <see langword="false"/>.
        /// </summary>
        bool _metIfTrue = true;

        /// <summary>
        /// Specifies the filename of the VOA file that is to be tested.
        /// </summary>
        string _voaFileName = "<SourceDocName>.voa";

        /// <summary>
        /// The document types used to satisify the condition.
        /// </summary>
        string[] _documentTypes = new string[0];

        /// <summary>
        /// The default industry to use when displaying available document types in configuration.
        /// </summary>
        string _industry;

        /// <summary>
        /// An <see cref="AFUtility"/> used to evaluate attribute queries.
        /// </summary>
        AFUtility _afUtility;

        /// <summary>
        /// <see langword="true"/> if changes have been made to
        /// <see cref="DocumentTypeCondition"/> since it was created;
        /// <see langword="false"/> if no changes have been made since it was created.
        /// </summary>
        bool _dirty;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentTypeCondition"/> class.
        /// </summary>
        public DocumentTypeCondition()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentTypeCondition"/> class as a copy
        /// of the specified <see paramref="task"/>.
        /// </summary>
        /// <param name="task">The <see cref="DocumentTypeCondition"/> from which
        /// settings should be copied.</param>
        public DocumentTypeCondition(DocumentTypeCondition task)
        {
            try
            {
                CopyFrom(task);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI32748");
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether the condition is met when the specified
        /// condition is <see langword="true"/>.
        /// </summary>
        /// <value>If <see langword="true"/> the condition will be met if the specified condition is
        /// <see langword="true"/>; if <see langword="false"/> the condition will be met if the
        /// specified condition is <see langword="false"/>.
        /// </value>
        public bool MetIfTrue
        {
            get
            {
                return _metIfTrue;
            }

            set
            {
                if (value != _metIfTrue)
                {
                    _metIfTrue = value;
                    _dirty = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets the name of the VOA file to be tested.
        /// </summary>
        /// <value>
        /// The name of the VOA file to be tested.
        /// </value>
        public string VOAFileName
        {
            get
            {
                return _voaFileName;
            }

            set
            {
                try
                {
                    if (value != _voaFileName)
                    {
                        _voaFileName = value;
                        _dirty = true;
                    }
                }
                catch (Exception ex)
                {
                    throw ex.CreateComVisible("ELI32760", ex.Message);
                }
            }
        }

        /// <summary>
        /// Gets or sets the list of document types the VOA files is to be compared against.
        /// </summary>
        /// <value>The list of document types the VOA files is to be compared against.</value>
        // Allow the return value to be an array since this is a COM visible property.
        [SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays")]
        public string[] DocumentTypes
        {
            get
            {
                return _documentTypes;
            }

            set
            {
                try
                {
                    if (value == null)
                    {
                        _documentTypes = null;
                    }
                    else
                    {
                        _documentTypes = new string[value.Length];
                        value.CopyTo(_documentTypes, 0);
                    }

                    _dirty = true;
                }
                catch (Exception ex)
                {
                    throw ex.CreateComVisible("ELI32749", ex.Message);
                }
            }
        }

        /// <summary>
        /// Gets or sets the default industry to use when displaying available document types in
        /// configuration.
        /// </summary>
        /// <value>The default industry to use when displaying available document types in
        /// configuration.</value>
        public string Industry
        {
            get
            {
                return _industry;
            }

            set
            {
                try
                {
                    if (value != _industry)
                    {
                        _industry = value;
                        _dirty = true;
                    }
                }
                catch (Exception ex)
                {
                    throw ex.CreateComVisible("ELI32759", ex.Message);
                }
            }
        }

        #endregion Properties

        #region IFAMCondition Members

        /// <summary>
        /// Tests the VOA data file indicated by the specified <see paramref="pFileRecord"/> against
        /// the specified settings to determine if the condition is met.
        /// </summary>
        /// <param name="pFileRecord">A <see cref="FileRecord"/> specifing the file to be tested.
        /// </param>
        /// <param name="pFPDB">The <see cref="FileProcessingDB"/> currently in use.</param>
        /// <param name="lActionID">The ID of the database action in use.</param>
        /// <param name="pFAMTagManager">A <see cref="FAMTagManager"/> to be used to evaluate any
        /// FAM tags used by the condition.</param>
        /// <returns><see langword="true"/> if the condition was met, <see langword="false"/> if it
        /// was not.</returns>
        public bool FileMatchesFAMCondition(FileRecord pFileRecord, FileProcessingDB pFPDB,
            int lActionID, FAMTagManager pFAMTagManager)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.FlexIndexCoreObjects, "ELI32735",
                    _COMPONENT_DESCRIPTION);

                string voaFileName = pFAMTagManager.ExpandTags(VOAFileName, pFileRecord.Name);

                if (!File.Exists(voaFileName))
                {
                    ExtractException ee =
                        new ExtractException("ELI32736", "Could not find specified VOA file");
                    ee.AddDebugData("Configured Path", VOAFileName, false);
                    ee.AddDebugData("Expanded Path", voaFileName, false);
                    throw ee;
                }

                // Load the attributes from the VOA file.
                IUnknownVector attributes = new IUnknownVector();
                attributes.LoadFrom(voaFileName, false);

                // Find all document types that have been applied to the document.
                IEnumerable<string> docTypes =
                    AFUtility.QueryAttributes(attributes, "/DocumentType", false)
                    .ToIEnumerable<IAttribute>()
                    .Select(attribute => attribute.Value.String);

                int docTypeCount = docTypes.Count();

                if (docTypeCount == 1 && DocumentTypes.Contains("Any Unique"))
                {
                    return MetIfTrue;
                }
                else if (docTypeCount > 1 && DocumentTypes.Contains("Multiply Classified"))
                {
                    return MetIfTrue;
                }
                else if (docTypes
                    .Where(docType => DocumentTypes.Contains(docType))
                    .Any())
                {
                    return MetIfTrue;
                }

                return !MetIfTrue;
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI32738",
                    "Error occured in '" + _COMPONENT_DESCRIPTION + "'", ex);
            }
        }

        /// <summary>
        /// Returns bool value indicating if the condition requires admin access
        /// </summary>
        /// <returns><see langword="true"/> if the condition requires admin access
        /// <see langword="false"/> if condition does not require admin access</returns>
        public bool RequiresAdminAccess()
        {
            return false;
        }

        #endregion IFAMCondition Members

        #region IConfigurableObject Members

        /// <summary>
        /// Performs configuration needed to create a valid <see cref="DocumentTypeCondition"/>.
        /// </summary>
        /// <returns><see langword="true"/> if the configuration was successfully updated or
        /// <see langword="false"/> if configuration was unsuccessful.</returns>
        public bool RunConfiguration()
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.FlexIndexCoreObjects, "ELI32739",
                    _COMPONENT_DESCRIPTION);

                // Make a clone to update settings and only copy if ok
                DocumentTypeCondition cloneOfThis = (DocumentTypeCondition)Clone();

                using (DocumentTypeConditionSettingsDialog dlg
                    = new DocumentTypeConditionSettingsDialog(cloneOfThis))
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
                throw ex.CreateComVisible("ELI32740", "Error running configuration.");
            }
        }

        #endregion IConfigurableObject Members

        #region IMustBeConfiguredObject Members

        /// <summary>
        /// Checks if the object has been configured properly.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if the object has been configured and <see langword="false"/>
        /// otherwise.
        /// </returns>
        public bool IsConfigured()
        {
            try
            {
                return (!string.IsNullOrEmpty(VOAFileName) && DocumentTypes.Length > 0);
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI32741",
                    "Failed to check '" + _COMPONENT_DESCRIPTION + "' configuration.");
            }
        }

        #endregion IMustBeConfiguredObject Members

        #region ICopyableObject Members

        /// <summary>
        /// Creates a copy of the <see cref="DocumentTypeCondition"/> instance.
        /// </summary>
        /// <returns>A copy of the <see cref="DocumentTypeCondition"/> instance.
        /// </returns>
        public object Clone()
        {
            try
            {
                return new DocumentTypeCondition(this);
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI32742",
                    "Failed to clone '" + _COMPONENT_DESCRIPTION + "' object.");
            }
        }

        /// <summary>
        /// Copies the specified <see cref="DocumentTypeCondition"/> instance into
        /// this one.
        /// </summary>
        /// <param name="pObject">The object from which to copy.</param>
        public void CopyFrom(object pObject)
        {
            try
            {
                var source = pObject as DocumentTypeCondition;
                if (source == null)
                {
                    throw new InvalidCastException("Invalid cast to DocumentTypeCondition");
                }
                CopyFrom(source);
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI32743",
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
            return LicenseUtilities.IsLicensed(LicenseIdName.FlexIndexCoreObjects);
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
        /// <returns>
        ///   <see cref="HResult.Ok"/> if changes have been made;
        /// <see cref="HResult.False"/> if changes have not been made.
        /// </returns>
        public int IsDirty()
        {
            return HResult.FromBoolean(_dirty);
        }

        /// <summary>
        /// Initializes an object from the <see cref="IStream"/> where it was previously saved.
        /// </summary>
        /// <param name="stream"><see cref="IStream"/> from which the object should be loaded.</param>
        public void Load(System.Runtime.InteropServices.ComTypes.IStream stream)
        {
            try
            {
                using (IStreamReader reader = new IStreamReader(stream, _CURRENT_VERSION))
                {
                    MetIfTrue = reader.ReadBoolean();
                    VOAFileName = reader.ReadString();
                    DocumentTypes = reader.ReadStringArray();
                    Industry = reader.ReadString();
                }

                // Freshly loaded object is no longer dirty
                _dirty = false;
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI32744",
                    "Failed to load '" + _COMPONENT_DESCRIPTION + "'.");
            }
        }

        /// <summary>
        /// Saves an object into the specified <see cref="IStream"/> and indicates whether the
        /// object should reset its dirty flag.
        /// </summary>
        /// <param name="stream"><see cref="IStream"/> into which the object should be saved.</param>
        /// <param name="clearDirty">Value that indicates whether to clear the dirty flag after the
        /// save is complete. If <see langword="true"/>, the flag should be cleared. If
        /// <see langword="false"/>, the flag should be left unchanged.</param>
        public void Save(System.Runtime.InteropServices.ComTypes.IStream stream, bool clearDirty)
        {
            try
            {
                using (IStreamWriter writer = new IStreamWriter(_CURRENT_VERSION))
                {
                    writer.Write(MetIfTrue);
                    writer.Write(VOAFileName);
                    writer.Write(DocumentTypes);
                    writer.Write(Industry);

                    // Write to the provided IStream.
                    writer.WriteTo(stream);
                }

                if (clearDirty)
                {
                    _dirty = false;
                }
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI32745",
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
        /// "Extract FAM Conditions" COM category.
        /// </summary>
        /// <param name="type">The <paramref name="type"/> being registered.</param>
        [ComRegisterFunction]
        [ComVisible(false)]
        static void RegisterFunction(Type type)
        {
            ComMethods.RegisterTypeInCategory(type, ExtractGuids.FileActionManagerConditions);
        }

        /// <summary>
        /// Code to be executed upon unregistration in order to remove this class from the
        /// "Extract FAM Conditions" COM category.
        /// </summary>
        /// <param name="type">The <paramref name="type"/> being unregistered.</param>
        [ComUnregisterFunction]
        [ComVisible(false)]
        static void UnregisterFunction(Type type)
        {
            ComMethods.UnregisterTypeInCategory(type, ExtractGuids.FileActionManagerConditions);
        }

        /// <summary>
        /// Copies the specified <see cref="DocumentTypeCondition"/> instance into this one.
        /// </summary>
        /// <param name="source">The <see cref="DocumentTypeCondition"/> from which to copy.
        /// </param>
        void CopyFrom(DocumentTypeCondition source)
        {
            MetIfTrue = source.MetIfTrue;
            VOAFileName = source.VOAFileName;
            DocumentTypes = source.DocumentTypes;
            Industry = source.Industry;

            _dirty = true;
        }

        /// <summary>
        /// Gets the <see cref="AFUtility"/> used to evaluate attribute queries.
        /// </summary>
        AFUtility AFUtility
        {
            get
            {
                if (_afUtility == null)
                {
                    _afUtility = new AFUtility();
                }

                return _afUtility;
            }
        }

        #endregion Private Members
    }
}
