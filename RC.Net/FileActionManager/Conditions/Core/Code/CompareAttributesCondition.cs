using AttributeDbMgrComponentsLib;
using Extract.DataCaptureStats;
using Extract.Interop;
using Extract.Licensing;
using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using UCLID_COMLMLib;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;

namespace Extract.FileActionManager.Conditions
{
    [ComVisible(true)]
    [Guid("FD8989D6-6080-4478-8BD9-1AA2D00C4808")]
    public enum AttributeFilterType
    {
        /// <summary>
        /// No attribute filter
        /// </summary>
        None = 0,

        /// <summary>
        /// Filter empty attributes
        /// </summary>
        FilterEmpty = 1,

        /// <summary>
        /// Attributes matched by xpath will be ignored
        /// </summary>
        FilterByXPath = 2
    }

    /// <summary>
    /// A <see cref="IFAMCondition"/> based on comparing attribute trees
    /// </summary>
    [ComVisible(true)]
    [Guid("D49E9FD5-8ECE-4E95-B3D5-E3747FC68454")]
    [ProgId("Extract.FileActionManager.Conditions.CompareAttributesCondition")]
    public class CompareAttributesCondition : ICategorizedComponent, IConfigurableObject,
        IMustBeConfiguredObject, ICopyableObject, IFAMCondition, ILicensedComponent,
        IPersistStream
    {
        #region Constants

        /// <summary>
        /// The object name.
        /// </summary>
        const string _COMPONENT_DESCRIPTION = "Compare attributes condition";

        /// <summary>
        /// Current task version.
        /// </summary>
        internal const int _CURRENT_VERSION = 1;

        /// <summary>
        /// XPath that will ignore empty attributes
        /// </summary>
        const string _IGNORE_EMPTY_XPATH = "/*//*[not(text())]";

        #endregion Constants

        #region Fields

        /// <summary>
        /// Indicates whether the current settings have been validated.
        /// </summary>
        bool _settingsValidated;

        /// <summary>
        /// <c>true</c> if changes have been made to
        /// <see cref="CompareAttributesCondition"/> since it was created;
        /// <c>false</c> if no changes have been made since it was created.
        /// </summary>
        bool _dirty;

        /// <summary>
        /// Used to validate attribute set names and to retrieve VOAs
        /// </summary>
        AttributeDBMgrClass _attributeDBMgr;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="CompareAttributesCondition"/> class.
        /// </summary>
        public CompareAttributesCondition()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CompareAttributesCondition"/> class as a copy
        /// of the specified <see paramref="task"/>.
        /// </summary>
        /// <param name="task">The <see cref="CompareAttributesCondition"/> from which
        /// settings should be copied.</param>
        public CompareAttributesCondition(CompareAttributesCondition task)
        {
            try
            {
                CopyFrom(task);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI45546");
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// If <c>true</c> the condition will be met if the specified condition is
        /// <c>true</c>; if <c>false</c> the condition will be met if the
        /// specified condition is <c>false</c>.
        /// </summary>
        public bool MetIfDifferent { get; set; } = true;

        /// <summary>
        /// The name of one attribute set to compare
        /// </summary>
        public string FirstAttributeSetName { get; set; }

        /// <summary>
        /// The name of another attribute set to compare
        /// </summary>
        public string SecondAttributeSetName { get; set; }

        /// <summary>
        /// The type of attribute filtering that will be performed prior to comparing
        /// </summary>
        public AttributeFilterType AttributesToIgnoreType { get; set; } = AttributeFilterType.FilterEmpty;

        /// <summary>
        /// The XPath query used to designate nodes as container-only. These nodes will not be compared
        /// but their descendants will be. This is only used if <see cref="AttributesToIgnoreType"/> is <see cref="AttributeFilterType.FilterByXPath"/>
        /// </summary>
        public string XPathToIgnore { get; set; } = _IGNORE_EMPTY_XPATH;

        #endregion Properties

        #region Public Methods

        /// <summary>
        /// Validates the instance's current settings.
        /// </summary>
        /// <throws><see cref="ExtractException"/> if the instance's settings are not valid.</throws>
        public void ValidateSettings()
        {
            try
            {
                if (_settingsValidated)
                {
                    return;
                }

                // Validate XPaths
                if (AttributesToIgnoreType == AttributeFilterType.FilterByXPath
                    && !string.IsNullOrWhiteSpace(XPathToIgnore)
                    && !UtilityMethods.IsValidXPathExpression(XPathToIgnore))
                {
                    ExtractException ee = new ExtractException("ELI45547", "Invalid XPath Query");
                    ee.AddDebugData("Query", XPathToIgnore, false);
                    throw ee;
                }

                // Validate attribute set names and initialize _attributeDBMgr
                var fileProcessingDb = new FileProcessingDBClass();
                fileProcessingDb.ConnectLastUsedDBThisProcess();
                _attributeDBMgr = new AttributeDBMgrClass
                {
                    FAMDB = fileProcessingDb
                };
                var attributeSets = new HashSet<string>(_attributeDBMgr.GetAllAttributeSetNames().GetKeys().ToIEnumerable<string>());

                if (string.IsNullOrWhiteSpace(FirstAttributeSetName)
                    || !attributeSets.Contains(FirstAttributeSetName))
                {
                    ExtractException ee = new ExtractException("ELI45549", "Invalid attribute set name");
                    ee.AddDebugData("Attribute set name", FirstAttributeSetName, false);
                    throw ee;
                }

                if (string.IsNullOrWhiteSpace(SecondAttributeSetName)
                    || !attributeSets.Contains(SecondAttributeSetName))
                {
                    ExtractException ee = new ExtractException("ELI45550", "Invalid attribute set name");
                    ee.AddDebugData("Attribute set name", SecondAttributeSetName, false);
                    throw ee;
                }

                _settingsValidated = true;
            }
            catch (Exception ex)
            {
                _settingsValidated = false;

                throw ex.CreateComVisible("ELI45551", ex.Message);
            }
        }

        #endregion Public Methods

        #region IFAMCondition Members

        /// <summary>
        /// Compares the attribute trees to determine if the condition is met.
        /// </summary>
        /// <param name="pFileRecord">A <see cref="FileRecord"/> specifying the file to be tested.
        /// </param>
        /// <param name="pFPDB">The <see cref="FileProcessingDB"/> currently in use.</param>
        /// <param name="lActionID">The ID of the database action in use.</param>
        /// <param name="pFAMTagManager">A <see cref="FAMTagManager"/> to be used to evaluate any
        /// FAM tags used by the condition.</param>
        /// <returns><c>true</c> if the condition was met, <c>false</c> if it
        /// was not.</returns>
        public bool FileMatchesFAMCondition(FileRecord pFileRecord, FileProcessingDB pFPDB,
            int lActionID, FAMTagManager pFAMTagManager)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.FlexIndexIDShieldCoreObjects, "ELI45552",
                    _COMPONENT_DESCRIPTION);

                // Validating the settings also initializes objects used by the condition (I.e., _attributeDBMgr)
                ValidateSettings();

                // Relative index of -1 means last stored
                var firstVOA = _attributeDBMgr.GetAttributeSetForFile(pFileRecord.FileID, FirstAttributeSetName,
                    relativeIndex: -1, closeConnection: false);

                var secondVOA = _attributeDBMgr.GetAttributeSetForFile(pFileRecord.FileID, SecondAttributeSetName,
                    relativeIndex: -1, closeConnection: false);

                // If an attribute is designated as 'container-only' then it won't contribute to false positive or missed counts
                // so it is effectively ignored.
                // This is used instead of the ignoreXPath parameter because that one excludes the entire subtree
                var containerXPath = AttributesToIgnoreType == AttributeFilterType.None
                    ? null
                    : AttributesToIgnoreType == AttributeFilterType.FilterEmpty
                        ? _IGNORE_EMPTY_XPATH
                        : XPathToIgnore;

                var results = AttributeTreeComparer.CompareAttributes(firstVOA, secondVOA, ignoreXPath: null, containerXPath: containerXPath)
                    .SummarizeStatistics(throwIfContainerOnlyConflict: false)
                    .Where(a => string.Equals("(Summary)", a.Path))
                    .ToLookup(a => a.Label);

                // Sum is used to convert enumerable into a number
                // (There will be either 0 or 1 AccuracyDetails for each label)
                int correct = results[AccuracyDetailLabel.Correct].Sum(a => a.Value);
                int expected = results[AccuracyDetailLabel.Expected].Sum(a => a.Value);
                int incorrect = results[AccuracyDetailLabel.Incorrect].Sum(a => a.Value);
                bool attributeTreesAreDifferent = correct != expected || incorrect > 0;

                // Determine if the condition is met.
                return attributeTreesAreDifferent == MetIfDifferent;
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI45553",
                    "Error occurred in '" + _COMPONENT_DESCRIPTION + "'", ex);
            }
        }

        /// <summary>
        /// Returns bool value indicating if the condition requires admin access
        /// </summary>
        /// <returns><c>true</c> if the condition requires admin access
        /// <c>false</c> if condition does not require admin access</returns>
        public bool RequiresAdminAccess()
        {
            return false;
        }

        #endregion IFAMCondition Members

        #region IConfigurableObject Members

        /// <summary>
        /// Performs configuration needed to create a valid <see cref="CompareAttributesCondition"/>.
        /// </summary>
        /// <returns><c>true</c> if the configuration was successfully updated or
        /// <c>false</c> if configuration was unsuccessful.</returns>
        public bool RunConfiguration()
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.FlexIndexIDShieldCoreObjects, "ELI45554",
                    _COMPONENT_DESCRIPTION);

                // Make a clone to update settings and only copy if ok
                CompareAttributesCondition cloneOfThis = (CompareAttributesCondition)Clone();

                using (CompareAttributesConditionSettingsDialog dlg
                    = new CompareAttributesConditionSettingsDialog(cloneOfThis))
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
                throw ex.CreateComVisible("ELI45555", "Error running configuration.");
            }
        }

        #endregion IConfigurableObject Members

        #region IMustBeConfiguredObject Members

        /// <summary>
        /// Checks if the object has been configured properly.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the object has been configured and <c>false</c>
        /// otherwise.
        /// </returns>
        public bool IsConfigured()
        {
            try
            {
                try
                {
                    // Return true if ValidateSettings does not throw an exception.
                    ValidateSettings();

                    return true;
                }
                catch
                {
                    // Otherwise return false and eat the exception.
                    return false;
                }
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI45556",
                    "Failed to check '" + _COMPONENT_DESCRIPTION + "' configuration.");
            }
        }

        #endregion IMustBeConfiguredObject Members

        #region ICopyableObject Members

        /// <summary>
        /// Creates a copy of the <see cref="CompareAttributesCondition"/> instance.
        /// </summary>
        /// <returns>A copy of the <see cref="CompareAttributesCondition"/> instance.
        /// </returns>
        public object Clone()
        {
            try
            {
                return new CompareAttributesCondition(this);
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI45557",
                    "Failed to clone '" + _COMPONENT_DESCRIPTION + "' object.");
            }
        }

        /// <summary>
        /// Copies the specified <see cref="CompareAttributesCondition"/> instance into
        /// this one.
        /// </summary>
        /// <param name="pObject">The object from which to copy.</param>
        public void CopyFrom(object pObject)
        {
            try
            {
                var source = pObject as CompareAttributesCondition;
                if (source == null)
                {
                    throw new InvalidCastException("Invalid cast to CompareAttributesCondition");
                }
                CopyFrom(source);
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI45558",
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
        /// <returns><c>true</c> if the component is licensed; <c>false</c> 
        /// if the component is not licensed.</returns>
        public bool IsLicensed()
        {
            return LicenseUtilities.IsLicensed(LicenseIdName.FlexIndexIDShieldCoreObjects);
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
        /// Initializes an object from the IStream where it was previously saved.
        /// </summary>
        /// <param name="stream">IStream from which the object should be loaded.</param>
        public void Load(System.Runtime.InteropServices.ComTypes.IStream stream)
        {
            try
            {
                using (IStreamReader reader = new IStreamReader(stream, _CURRENT_VERSION))
                {
                    MetIfDifferent = reader.ReadBoolean();
                    FirstAttributeSetName = reader.ReadString();
                    SecondAttributeSetName = reader.ReadString();
                    AttributesToIgnoreType = (AttributeFilterType)reader.ReadInt32();
                    XPathToIgnore = reader.ReadString();
                }

                // Freshly loaded object is no longer dirty
                SetDirty(false);
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI45559",
                    "Failed to load '" + _COMPONENT_DESCRIPTION + "'.");
            }
        }

        /// <summary>
        /// Saves an object into the specified IStream and indicates whether the object should reset
        /// its dirty flag.
        /// </summary>
        /// <param name="stream">IStream into which the object should be saved.</param>
        /// <param name="clearDirty">Value that indicates whether to clear the dirty flag after the
        /// save is complete. If <c>true</c>, the flag should be cleared. If
        /// <c>false</c>, the flag should be left unchanged.</param>
        public void Save(System.Runtime.InteropServices.ComTypes.IStream stream, bool clearDirty)
        {
            try
            {
                ValidateSettings();

                using (IStreamWriter writer = new IStreamWriter(_CURRENT_VERSION))
                {
                    writer.Write(MetIfDifferent);
                    writer.Write(FirstAttributeSetName);
                    writer.Write(SecondAttributeSetName);
                    writer.Write((int)AttributesToIgnoreType);
                    writer.Write(XPathToIgnore);

                    // Write to the provided IStream.
                    writer.WriteTo(stream);
                }

                if (clearDirty)
                {
                    SetDirty(false);
                }
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI45560",
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
            ComMethods.RegisterTypeInCategory(type, ExtractCategories.FileActionManagerConditionsGuid);
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
            ComMethods.UnregisterTypeInCategory(type, ExtractCategories.FileActionManagerConditionsGuid);
        }

        /// <summary>
        /// Copies the specified <see cref="CompareAttributesCondition"/> instance into this one.
        /// </summary>
        /// <param name="source">The <see cref="CompareAttributesCondition"/> from which to copy.
        /// </param>
        void CopyFrom(CompareAttributesCondition source)
        {
            MetIfDifferent = source.MetIfDifferent;
            FirstAttributeSetName = source.FirstAttributeSetName;
            SecondAttributeSetName = source.SecondAttributeSetName;
            AttributesToIgnoreType = source.AttributesToIgnoreType;
            XPathToIgnore = source.XPathToIgnore;

            SetDirty(true);
        }

        /// <summary>
        /// Sets the dirty flag.
        /// </summary>
        /// <param name="dirty"><c>true</c> to set the dirty flag; <c>false</c>
        /// to clear it.</param>
        void SetDirty(bool dirty)
        {
            _dirty = dirty;
            if (dirty)
            {
                _settingsValidated = false;
            }
        }

        #endregion Private Members
    }
}
