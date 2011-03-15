using Extract.Interop;
using Extract.Licensing;
using Extract.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Windows.Forms;
using UCLID_AFCORELib;
using UCLID_AFUTILSLib;
using UCLID_COMLMLib;
using UCLID_COMUTILSLib;
using UCLID_FILEPROCESSINGLib;
using UCLID_RASTERANDOCRMGMTLib;

namespace Extract.Redaction
{
    /// <summary>
    /// An IFileProcessingTask which merges redaction data from two different VOA files.
    /// </summary>
    [ComVisible(true)]
    [Guid("54D4B5BC-32EC-470C-8DBE-E30FFBF370E9")]
    [ProgId("Extract.Redaction.VOAFileMergeTask")]
    public class VOAFileMergeTask : IFileProcessingTask, IConfigurableObject, IMustBeConfiguredObject,
        IAccessRequired, ICategorizedComponent, ICopyableObject, ILicensedComponent, IPersistStream
    {
        #region Constants

        /// <summary>
        /// The COM object name.
        /// </summary>
        internal const string _COMPONENT_DESCRIPTION = "Redaction: Merge ID Shield data files";

        /// <summary>
        /// Current task version.
        /// </summary>
        internal const int _CURRENT_VERSION = 1;

        #endregion Constants

        #region Fields

        /// <summary>
        /// <see langword="true"/> if changes have been made to
        /// <see cref="VOAFileMergeTask"/> since it was created;
        /// <see langword="false"/> if no changes have been made since it was created.
        /// </summary>
        bool _dirty;

        /// <summary>
        /// The ID Shield ini settings.
        /// </summary>
        InitializationSettings _idShieldSettings = new InitializationSettings();

        /// <summary>
        /// Used to remove metadata attributes from the merged result.
        /// </summary>
        AFUtility _afUtility = new AFUtility();

        /// <summary>
        /// The settings for this object.
        /// </summary>
        VOAFileMergeTaskSettings _settings;

        /// <summary>
        /// Used to compare and merge the files.
        /// </summary>
        SpatialAttributeMergeUtils _attributeMerger;

        #endregion Fields

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="VOAFileMergeTask"/> class.
        /// </summary>
        public VOAFileMergeTask()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="VOAFileMergeTask"/> class.
        /// </summary>
        /// <param name="task">The <see cref="VOAFileMergeTask"/> from which
        /// settings should be copied.</param>
        public VOAFileMergeTask(VOAFileMergeTask task)
        {
            try
            {
                CopyFrom(task);
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI32038");
            }
        }

        #endregion Constructors

        #region IFileProcessingTask Members

        /// <summary>
        /// Called before any file processing starts.
        /// </summary>
        [CLSCompliant(false)]
        public void Init(int nActionID, FAMTagManager pFAMTM, FileProcessingDB pDB)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.IDShieldCoreObjects, "ELI32041",
                    _COMPONENT_DESCRIPTION);

                InitializeAttributeMerger();
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI32042",
                    "Unable to initialize 'Extend redactions to surround context' task.", ex);
            }
        }

        /// <summary>
        /// Processes the specified file.
        /// </summary>
        /// <param name="pFileRecord">The file record that contains the info of the file being 
        /// processed.</param>
        /// <param name="nActionID">The ID of the action being processed.</param>
        /// <param name="pFAMTM">A File Action Manager Tag Manager for expanding tags.</param>
        /// <param name="pDB">The File Action Manager database.</param>
        /// <param name="pProgressStatus">Object to provide progress status updates to caller.
        /// </param>
        /// <param name="bCancelRequested"><see langword="true"/> if cancel was requested; 
        /// <see langword="false"/> otherwise.</param>
        /// <returns><see langword="true"/> if processing should continue; <see langword="false"/> 
        /// if all file processing should be cancelled.</returns>
        [CLSCompliant(false)]
        public EFileProcessingResult ProcessFile(FileRecord pFileRecord, int nActionID,
            FAMTagManager pFAMTM, FileProcessingDB pDB, ProgressStatus pProgressStatus, bool bCancelRequested)
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.IDShieldCoreObjects, "ELI32043",
                    _COMPONENT_DESCRIPTION);

                // Load the redactions
                FileActionManagerPathTags pathTags =
                    new FileActionManagerPathTags(pFileRecord.Name, pFAMTM.FPSFileDir);
                string dataFile1 = pathTags.Expand(_settings.DataFile1);
                string dataFile2 = pathTags.Expand(_settings.DataFile2);

                // Create a RedactionFileLoader to load redactions from the data files.
                RedactionFileLoader voaLoader =
                    new RedactionFileLoader(
                        new ConfidenceLevelsCollection(_idShieldSettings.ConfidenceLevels));

                // Load spatial & redacted attributes from dataFile1
                voaLoader.LoadFrom(dataFile1, pFileRecord.Name);
                IUnknownVector attributeSet1 = voaLoader.Items
                    .Where(sensitiveItem => sensitiveItem.Attribute.Redacted)
                    .Select(sensitiveItem => sensitiveItem.Attribute.ComAttribute)
                    .Where(attribute => attribute.Value.HasSpatialInfo())
                    .ToIUnknownVector<IAttribute>();

                // Load spatial & redacted attributes from dataFile2
                voaLoader.LoadFrom(dataFile2, pFileRecord.Name);
                IUnknownVector attributeSet2 = voaLoader.Items
                    .Where(sensitiveItem => sensitiveItem.Attribute.Redacted)
                    .Select(sensitiveItem => sensitiveItem.Attribute.ComAttribute)
                    .Where(attribute => attribute.Value.HasSpatialInfo())
                    .ToIUnknownVector<IAttribute>();

                // Load spatial info (used to normalize the attribute raster zones into the same
                // coordinate system for comparison).
                SpatialString docText = new SpatialString();
                docText.LoadFrom(pFileRecord.Name + ".uss", false);

                _attributeMerger.CompareAttributeSets(attributeSet1, attributeSet2, docText);

                // Apply the merges to both sets. The output will be the merged redactions plus
                // any un-merged redactions from either set.
                _attributeMerger.ApplyMerges(attributeSet1);
                _attributeMerger.ApplyMerges(attributeSet2);

                // To get all un-merged redactions into the ouput, select all attributes unique
                // to set2 and add them to set1.
                IEnumerable<IAttribute> set1Enumerable = attributeSet1.ToIEnumerable<IAttribute>();
                attributeSet1.Append(attributeSet2
                    .ToIEnumerable<IAttribute>()
                    .Where(attribute => !set1Enumerable.Contains(attribute))
                    .ToIUnknownVector<IAttribute>());

                // Strip out the metadata attributes which won't be meaningful in the merged
                // output.
                _afUtility.RemoveMetadataAttributes(attributeSet1);

                // Save set 1 as the merged output.
                string outputFile = pathTags.Expand(_settings.OutputFile);
                attributeSet1.SaveTo(outputFile, false);

                return EFileProcessingResult.kProcessingSuccessful;
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI32044",
                    "Unable to extend redactions to cover surrounding context.", ex);
            }
        }

        /// <summary>
        /// Stops processing the current file.
        /// </summary>
        public void Cancel()
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.IDShieldCoreObjects, "ELI32045",
                    _COMPONENT_DESCRIPTION);
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI32046",
                    "Unable to cancel 'Extend redactions to surround context' task.", ex);
            }
        }

        /// <summary>
        /// Called when all file processing has completed.
        /// </summary>
        public void Close()
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.IDShieldCoreObjects, "ELI32047",
                    _COMPONENT_DESCRIPTION);
            }
            catch (Exception ex)
            {
                throw ExtractException.CreateComVisible("ELI32048",
                    "Unable to close 'Extend redactions to surround context' task.", ex);
            }
        }

        #endregion IFileProcessingTask Members

        #region IConfigurableObject Members

        /// <summary>
        /// Performs configuration needed to create a valid
        /// <see cref="VOAFileMergeTask"/>.
        /// </summary>
        /// <returns><see langword="true"/> if the configuration was successfully updated or
        /// <see langword="false"/> if configuration was unsuccessful.</returns>
        public bool RunConfiguration()
        {
            try
            {
                // Validate the license
                LicenseUtilities.ValidateLicense(LicenseIdName.IDShieldCoreObjects, "ELI32049",
                    _COMPONENT_DESCRIPTION);

                // Allow the user to set the verification settings
                using (var dialog = new VOAFileMergeTaskSettingsDialog(_settings))
                {
                    bool result = dialog.ShowDialog() == DialogResult.OK;

                    // Store the result
                    if (result)
                    {
                        _settings = dialog.VOAFileMergeTaskSettings;
                        _dirty = true;
                    }

                    return result;
                }
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI32050", "Error running configuration.");
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
                return (_settings != null &&
                        !string.IsNullOrWhiteSpace(_settings.DataFile1) &&
                        !string.IsNullOrWhiteSpace(_settings.DataFile2) &&
                        !string.IsNullOrWhiteSpace(_settings.OutputFile));
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI32131",
                    "Failed to check " + _COMPONENT_DESCRIPTION + " configuration.");
            }
        }

        #endregion IMustBeConfiguredObject Members

        #region IAccessRequired Members

        /// <summary>
        /// Returns bool value indicating if the task requires admin access
        /// </summary>
        /// <returns><see langword="true"/> if the task requires admin access
        /// <see langword="false"/> if task does not require admin access</returns>
        public bool RequiresAdminAccess()
        {
            return false;
        }

        #endregion IAccessRequired Members

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

        #region ICopyableObject Members

        /// <summary>
        /// Creates a copy of the <see cref="VOAFileMergeTask"/> instance.
        /// </summary>
        /// <returns>A copy of the <see cref="VOAFileMergeTask"/> instance.
        /// </returns>
        public object Clone()
        {
            try
            {
                return new VOAFileMergeTask(this);
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI32051",
                    "Failed to clone " + _COMPONENT_DESCRIPTION + " object.");
            }
        }

        /// <summary>
        /// Copies the specified <see cref="VOAFileMergeTask"/> instance into
        /// this one.
        /// </summary>
        /// <param name="pObject">The object from which to copy.</param>
        public void CopyFrom(object pObject)
        {
            try
            {
                CopyFrom((VOAFileMergeTask)pObject);
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI32052",
                    "Failed to copy " + _COMPONENT_DESCRIPTION + " object.");
            }
        }

        #endregion ICopyableObject Members

        #region ILicensedComponent Members

        /// <summary>
        /// Gets whether this component is licensed.
        /// </summary>
        /// <returns><see langword="true"/> if the component is licensed; <see langword="false"/> 
        /// if the component is not licensed.</returns>
        public bool IsLicensed()
        {
            return LicenseUtilities.IsLicensed(LicenseIdName.IDShieldCoreObjects);
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
        /// <see cref="HResult.False"/> if changes have not been made.</returns>
        public int IsDirty()
        {
            return HResult.FromBoolean(_dirty);
        }

        /// <summary>
        /// Initializes an object from the <see cref="IStream"/> where it was previously saved.
        /// </summary>
        /// <param name="stream"><see cref="IStream"/> from which the object should be loaded.
        /// </param>
        public void Load(IStream stream)
        {
            try
            {
                using (IStreamReader reader = new IStreamReader(stream, _CURRENT_VERSION))
                {
                    // Read the settings
                    _settings = VOAFileMergeTaskSettings.ReadFrom(reader);
                }

                // Freshly loaded object is no longer dirty
                _dirty = false;
            }
            catch (Exception ex)
            {
                throw ex.CreateComVisible("ELI32053", "Unable to load verification task.");
            }
        }

        /// <summary>
        /// Saves an object into the specified <see cref="IStream"/> and indicates whether the 
        /// object should reset its dirty flag.
        /// </summary>
        /// <param name="stream"><see cref="IStream"/> into which the object should be saved.
        /// </param>
        /// <param name="clearDirty">Value that indicates whether to clear the dirty flag after the
        /// save is complete. If <see langword="true"/>, the flag should be cleared. If 
        /// <see langword="false"/>, the flag should be left unchanged.</param>
        public void Save(IStream stream, bool clearDirty)
        {
            try
            {
                using (IStreamWriter writer = new IStreamWriter(_CURRENT_VERSION))
                {
                    // Serialize the settings
                    _settings.WriteTo(writer);

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
                throw ex.CreateComVisible("ELI32054",
                    "Unable to save replaced indexed text settings.");
            }
        }

        /// <summary>
        /// Returns the size in bytes of the stream needed to save the object.
        /// </summary>
        /// <param name="size">Pointer to a 64-bit unsigned integer value indicating the size, in 
        /// bytes, of the stream needed to save this object.</param>
        public void GetSizeMax(out long size)
        {
            size = HResult.NotImplemented;
        }

        #endregion IPersistStream Members

        #region Private Members

        /// <summary>
        /// Code to be executed upon registration in order to add this class to the
        /// <see cref="ExtractGuids.FileProcessors"/> COM category.
        /// </summary>
        /// <param name="type">The <paramref name="type"/> being registered.</param>
        [ComRegisterFunction]
        [ComVisible(false)]
        static void RegisterFunction(Type type)
        {
            ComMethods.RegisterTypeInCategory(type, ExtractGuids.FileProcessors);
        }

        /// <summary>
        /// Code to be executed upon unregistration in order to remove this class from the
        /// <see cref="ExtractGuids.FileProcessors"/> COM category.
        /// </summary>
        /// <param name="type">The <paramref name="type"/> being unregistered.</param>
        [ComUnregisterFunction]
        [ComVisible(false)]
        static void UnregisterFunction(Type type)
        {
            ComMethods.UnregisterTypeInCategory(type, ExtractGuids.FileProcessors);
        }

        /// <summary>
        /// Copies the specified <see cref="VOAFileMergeTask"/> instance into
        /// this one.
        /// </summary>
        /// <param name="task">The <see cref="VOAFileMergeTask"/> from which to
        /// copy.</param>
        public void CopyFrom(VOAFileMergeTask task)
        {
            try
            {
                if (task._settings == null)
                {
                    _settings = null;
                }
                else
                {
                    _settings = new VOAFileMergeTaskSettings(task._settings);
                }
            }
            catch (Exception ex)
            {
                throw ex.AsExtract("ELI32137");
            }
        }

        /// <summary>
        /// Initializes the attribute merger.
        /// </summary>
        void InitializeAttributeMerger()
        {
            _attributeMerger = new SpatialAttributeMergeUtils();

            _attributeMerger.OverlapPercent = _settings.OverlapThreshold;
            _attributeMerger.UseMutualOverlap = true;
            _attributeMerger.NameMergeMode = EFieldMergeMode.kPreserveField;
            _attributeMerger.TypeMergeMode = EFieldMergeMode.kCombineField;
            _attributeMerger.SpecifiedValue = "Merged";
            _attributeMerger.PreserveAsSubAttributes = false;
            _attributeMerger.CreateMergedRegion = false;

            // Add the standard confidence levels to the name merge priority.
            VariantVector nameMergePriority = new VariantVector();
            nameMergePriority.PushBack("Manual");
            nameMergePriority.PushBack("HCData");
            nameMergePriority.PushBack("MCData");
            nameMergePriority.PushBack("LCData");
            nameMergePriority.PushBack("Clues");

            // And any non-standard confidence level query from the ini file.
            foreach (string name in _idShieldSettings.ConfidenceLevels
                .Select(confidenceLevel => confidenceLevel.Query)
                .Where(name => nameMergePriority.Find(name) == -1))
            {
                nameMergePriority.PushBack(name);
            }

            _attributeMerger.NameMergePriority = nameMergePriority;
        }

        #endregion Private Members
    }
}
